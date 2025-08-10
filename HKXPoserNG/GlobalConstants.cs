using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKXPoserNG;

public static class GlobalConstants {
    static GlobalConstants() {
        Directory.CreateDirectory(HKXDirectory);
        Directory.CreateDirectory(TempDirectory);
    }
    public static string AppDirectory { get; } = Directory.GetCurrentDirectory();
    public static string HKXDirectory { get; } = Path.Combine(AppDirectory, "HKX");
    public static string TempDirectory { get; } = Path.Combine(AppDirectory, "Temp");
    public static string HkDumpExecutablePath { get; } = Path.Combine(AppDirectory, "ExternalPrograms" , "hkdump-bin.exe");
}

