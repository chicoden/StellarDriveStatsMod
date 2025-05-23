﻿using MelonLoader;
using FrameStats;
using LibreHardwareMonitor.Hardware;
using System.Diagnostics;
using UnityEngine;
using TMPro;
using Lock = System.Object;
using ThreadState = System.Threading.ThreadState;

namespace UI.FrameStats {
    using FrameStats = global::FrameStats;

    public class FrameStatsUpdater : MonoBehaviour {
        private static class StatId {
            public const int BatteryLevel = 0;
            public const int CpuTemperature = 1;
            public const int CpuLoad = 2;
            public const int RamUsed = 3;
            public const int GpuTemperature = 4;
            public const int GpuLoad = 5;
            public const int DedicatedRamUsed = 6;
            public const int SharedRamUsed = 7;
            public const int Max = 8;
        }

        private const int _HW_MONITOR_UPDATE_DELAY = 50;
        private const int _HW_MONITOR_IDLE_DELAY = 50;
        private const string _FIELD_UNKNOWN = "???";

        private MelonPreferences_Entry<bool> _frameStatsEnabled;

        private TMP_Text _fieldFps;
        private TMP_Text _fieldFrameTime;
        private TMP_Text[] _fieldHwStats;

        private Lock _hwStatsLock;
        private string[] _hwStatsToDisplay;

        private volatile bool _flagKeepHwMonitorAlive;
        private volatile bool _flagKeepHwMonitorMonitoring;
        private Thread _hwMonitorThread;

        private RollingAverager _frameRateTracker;
        private RollingAverager _frameTimeTracker;

        private void Awake() {
            MelonPreferences_Category frameStatsPreferences = MelonPreferences.GetCategory("FrameStatsPreferences");
            _frameStatsEnabled = frameStatsPreferences.GetEntry<bool>("FrameStatsEnabled");

            TMP_Text[] textComponents = GetComponentsInChildren<TMP_Text>();
            _fieldFps = textComponents[1];
            _fieldFrameTime = textComponents[3];
            _fieldHwStats = new TMP_Text[StatId.Max];
            for (int i = 0; i < StatId.Max; i++) {
                _fieldHwStats[i] = textComponents[i * 2 + 5];
            }

            _hwStatsLock = new Lock();
            _hwStatsToDisplay = new string[StatId.Max];
            for (int i = 0; i < StatId.Max; i++) {
                _hwStatsToDisplay[i] = _FIELD_UNKNOWN;
            }

            _flagKeepHwMonitorAlive = false;
            _flagKeepHwMonitorMonitoring = false;
            ThreadStart hwMonitorDelegate = new ThreadStart(MonitorHardware);
            _hwMonitorThread = new Thread(hwMonitorDelegate);

            _frameRateTracker = new RollingAverager(TimeSpan.FromSeconds(2));
            _frameTimeTracker = new RollingAverager(TimeSpan.FromSeconds(2));

            gameObject.SetActive(_frameStatsEnabled.Value);
            _frameStatsEnabled.OnEntryValueChanged.Subscribe(OnToggleEnabled);
        }

        private void OnDestroy() {
            _frameStatsEnabled.OnEntryValueChanged.Unsubscribe(OnToggleEnabled);
            if (_hwMonitorThread.ThreadState != ThreadState.Unstarted) {
                _flagKeepHwMonitorAlive = false;
                _hwMonitorThread.Join();
            }
        }

        private void OnEnable() {
            _frameRateTracker.Reset();
            _frameTimeTracker.Reset();

            _flagKeepHwMonitorMonitoring = true;
            if (_hwMonitorThread.ThreadState == ThreadState.Unstarted) {
                _flagKeepHwMonitorAlive = true;
                _hwMonitorThread.Start();
            }
        }

        private void OnDisable() {
            _flagKeepHwMonitorMonitoring = false;
        }

        private void Update() {
            double deltaTime = Time.unscaledDeltaTime;
            TimeSpan frameTimeSpan = TimeSpan.FromSeconds(deltaTime);

            double avgFrameRate = _frameRateTracker.AddSample(new Sample(1.0 / deltaTime, frameTimeSpan));
            _fieldFps.text = $"{avgFrameRate:0}";

            double avgFrameTime = _frameTimeTracker.AddSample(new Sample(deltaTime, frameTimeSpan)) * 1000.0;
            _fieldFrameTime.text = $"{avgFrameTime:0.0}ms";

            lock (_hwStatsLock) {
                for (int i = 0; i < StatId.Max; i++) {
                    _fieldHwStats[i].text = _hwStatsToDisplay[i];
                }
            }
        }

        private void OnToggleEnabled(bool prevValue, bool curValue) {
            gameObject.SetActive(curValue);
        }

        private void MonitorHardware() {
            Melon<FrameStats.Core>.Logger.Msg("entering hw monitor");

            Computer computer = new Computer{
                IsBatteryEnabled = true,
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true
            };

            computer.Open();

            HardwareUpdateVisitor hwUpdateVisitor = new HardwareUpdateVisitor();
            ISensor[] sensors = new ISensor[StatId.Max];
            computer.Accept(hwUpdateVisitor);
            foreach (IHardware hardware in computer.Hardware) {
                switch (hardware.HardwareType) {
                    case HardwareType.Battery:
                        foreach (ISensor sensor in hardware.Sensors) {
                            if (sensor.Name == "Charge Level" && sensor.SensorType == SensorType.Level) {
                                sensors[StatId.BatteryLevel] = sensor;
                                break;
                            }
                        } break;

                    case HardwareType.Cpu:
                        foreach (ISensor sensor in hardware.Sensors) {
                            if (sensor.Name == "Core Max" && sensor.SensorType == SensorType.Temperature) {
                                sensors[StatId.CpuTemperature] = sensor;
                            } else if (sensor.Name == "CPU Total" && sensor.SensorType == SensorType.Load) {
                                sensors[StatId.CpuLoad] = sensor;
                            }
                        } break;

                    case HardwareType.Memory:
                        foreach (ISensor sensor in hardware.Sensors) {
                            if (sensor.Name == "Memory Used" && sensor.SensorType == SensorType.Data) {
                                sensors[StatId.RamUsed] = sensor;
                                break;
                            }
                        } break;

                    case HardwareType.GpuNvidia:
                    case HardwareType.GpuAmd:
                    case HardwareType.GpuIntel:
                        if (hardware.Name == SystemInfo.graphicsDeviceName) {
                            foreach (ISensor sensor in hardware.Sensors) {
                                if (sensor.Name == "GPU Core" && sensor.SensorType == SensorType.Temperature) {
                                    sensors[StatId.GpuTemperature] = sensor;
                                } else if (sensor.Name == "GPU Core" && sensor.SensorType == SensorType.Load) {
                                    sensors[StatId.GpuLoad] = sensor; // Aggregate load types for Intel iGPUs?
                                } else if (sensor.Name == "D3D Dedicated Memory Used" && sensor.SensorType == SensorType.SmallData) {
                                    sensors[StatId.DedicatedRamUsed] = sensor;
                                } else if (sensor.Name == "D3D Shared Memory Used" && sensor.SensorType == SensorType.SmallData) {
                                    sensors[StatId.SharedRamUsed] = sensor;
                                }
                            }
                        } break;
                }
            }

            RollingAverager cpuLoadTracker = new RollingAverager(TimeSpan.FromSeconds(15));
            RollingAverager gpuLoadTracker = new RollingAverager(TimeSpan.FromSeconds(15));
            Stopwatch stopwatch = new Stopwatch();

            string[] hwStatsToDisplay = new string[StatId.Max];

            while (_flagKeepHwMonitorAlive) {
                cpuLoadTracker.Reset();
                gpuLoadTracker.Reset();
                stopwatch.Restart();

                while (_flagKeepHwMonitorMonitoring) {
                    computer.Accept(hwUpdateVisitor);
                    TimeSpan sampleInterval = stopwatch.Elapsed;
                    stopwatch.Restart();

                    float? batteryLevel     = sensors[StatId.BatteryLevel    ]?.Value;
                    float? cpuTemp          = sensors[StatId.CpuTemperature  ]?.Value;
                    float? cpuLoad          = sensors[StatId.CpuLoad         ]?.Value;
                    float? ramUsed          = sensors[StatId.RamUsed         ]?.Value;
                    float? gpuTemp          = sensors[StatId.GpuTemperature  ]?.Value;
                    float? gpuLoad          = sensors[StatId.GpuLoad         ]?.Value;
                    float? dedicatedRamUsed = sensors[StatId.DedicatedRamUsed]?.Value;
                    float? sharedRamUsed    = sensors[StatId.SharedRamUsed   ]?.Value;

                    hwStatsToDisplay[StatId.BatteryLevel    ] = batteryLevel     is null ? _FIELD_UNKNOWN : $"{batteryLevel:0}%";
                    hwStatsToDisplay[StatId.CpuTemperature  ] = cpuTemp          is null ? _FIELD_UNKNOWN : $"{cpuTemp:0.0}C";
                    hwStatsToDisplay[StatId.CpuLoad         ] = cpuLoad          is null ? _FIELD_UNKNOWN : $"{cpuLoadTracker.AddSample(new Sample((float)cpuLoad, sampleInterval)):0}%";
                    hwStatsToDisplay[StatId.RamUsed         ] = ramUsed          is null ? _FIELD_UNKNOWN : $"{ramUsed:0.0}GB";
                    hwStatsToDisplay[StatId.GpuTemperature  ] = gpuTemp          is null ? _FIELD_UNKNOWN : $"{gpuTemp:0.0}C";
                    hwStatsToDisplay[StatId.GpuLoad         ] = gpuLoad          is null ? _FIELD_UNKNOWN : $"{gpuLoadTracker.AddSample(new Sample((float)gpuLoad, sampleInterval)):0}%";
                    hwStatsToDisplay[StatId.DedicatedRamUsed] = dedicatedRamUsed is null ? _FIELD_UNKNOWN : $"{dedicatedRamUsed / 1000:0.0}GB";
                    hwStatsToDisplay[StatId.SharedRamUsed   ] = sharedRamUsed    is null ? _FIELD_UNKNOWN : $"{sharedRamUsed / 1000:0.0}GB";

                    lock (_hwStatsLock) {
                        for (int i = 0; i < StatId.Max; i++) {
                            _hwStatsToDisplay[i] = hwStatsToDisplay[i];
                        }
                    }

                    Thread.Sleep(_HW_MONITOR_UPDATE_DELAY);
                }

                Thread.Sleep(_HW_MONITOR_IDLE_DELAY);
            }

            computer.Close();
            Melon<FrameStats.Core>.Logger.Msg("exiting hw monitor");
        }
    }
}