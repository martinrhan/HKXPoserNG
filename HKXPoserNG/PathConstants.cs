using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKXPoserNG;

public static class PathConstants {
    static PathConstants() {
        if (!Design.IsDesignMode)
            Directory.CreateDirectory(TempDirectory);
    }

    public readonly static string AppDirectory = AppContext.BaseDirectory;
    public readonly static string TempDirectory = Path.Combine(AppDirectory, "Temp");
    public readonly static string DataDirectory = Path.Combine(AppDirectory, "Data");
    public readonly static string HKDumpExecutablePath = Path.Combine(AppDirectory, "ExternalPrograms", "hkdump-bin.exe");
    public readonly static string HCTExecutablePath = Path.Combine(AppDirectory, "ExternalPrograms", "hct.exe");
}
