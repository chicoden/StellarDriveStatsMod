try:
    import os

    USER_ROOT = os.path.expanduser("~")
    PROJECT_ROOT = os.path.join(__file__, "..\\..\\..")
    STELLARDRIVE_ROOT = os.path.join(PROJECT_ROOT, "StellarDrive_0.6.5")
    USERLIBS_DIR = os.path.join(STELLARDRIVE_ROOT, "UserLibs")
    VS_PROJECT_ROOT = os.path.join(__file__, "..")
    VS_OUTPUT_DIR = os.path.join(VS_PROJECT_ROOT, "bin\\Debug\\netstandard2.1")
    VS_TARGET_NAME = "FrameStats.dll"
    THIRD_PARTY_USERLIBS = {
        "UniverseLib.Mono.dll"
    }

    excluded_libs = {
        VS_TARGET_NAME,
        *os.listdir(os.path.join(STELLARDRIVE_ROOT, "StellarDrive_Data\\Managed")),
        *os.listdir(os.path.join(STELLARDRIVE_ROOT, "MelonLoader\\net35"))
    }

    expected_userlibs = {
        userlib for userlib in os.listdir(VS_OUTPUT_DIR)
        if userlib.endswith(".dll") and userlib not in excluded_libs
    }

    userlibs_to_remove = set(os.listdir(USERLIBS_DIR)) - expected_userlibs - THIRD_PARTY_USERLIBS
    for userlib_to_remove in userlibs_to_remove:
        print("Removing UserLib", userlib_to_remove)
        os.remove(os.path.join(USERLIBS_DIR, userlib_to_remove))

    for userlib_to_add in expected_userlibs:
        print("Adding UserLib", userlib_to_add)

        source_dir = VS_OUTPUT_DIR
        if userlib_to_add == "Microsoft.Win32.Registry.dll": # KILL ME NOW
            source_dir = os.path.join(USER_ROOT, ".nuget\\packages\\microsoft.win32.registry\\5.0.0\\lib\\net46")

        with (
            open(os.path.join(source_dir, userlib_to_add), "rb") as source,
            open(os.path.join(USERLIBS_DIR, userlib_to_add), "wb") as dest
        ):
            dest.write(source.read())

except Exception as unexpected:
    print(unexpected)
