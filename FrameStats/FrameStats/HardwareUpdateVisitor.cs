using LibreHardwareMonitor.Hardware;

namespace FrameStats {
    public class HardwareUpdateVisitor : IVisitor {
        public void VisitComputer(IComputer computer) {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware) {
            hardware.Update();
            foreach (IHardware subhardware in hardware.SubHardware) {
                subhardware.Accept(this);
            }
        }

        public void VisitSensor(ISensor sensor) {}
        public void VisitParameter(IParameter parameter) {}
    }
}