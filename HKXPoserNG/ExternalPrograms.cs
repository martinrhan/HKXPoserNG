using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKXPoserNG;

public static class ExternalPrograms {
    private static void RunExecutable(string exePath, string path_in, string path_out) {
        ProcessStartInfo info = new ProcessStartInfo() {
            FileName = exePath,
            Arguments = $"\"{path_in}\" -o \"{path_out}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        Process? process = new Process() {
            StartInfo = info,
            EnableRaisingEvents = true
        };
        process.OutputDataReceived += (sender, e) => {
            if (e.Data != null) {
                string output = Path.GetFileName(exePath) + " output: " + e.Data;
                Debug.WriteLine(output);
                Console.Write(output);
            }
        };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"{Path.GetFileName(exePath)} failed with exit code {process.ExitCode}. Make sure the input file is valid and the output path is writable.");
    }
    public static void HCT(string path_in, string path_out) {
        RunExecutable(PathConstants.HCTExecutablePath, path_in, path_out);
    }
    public static void HKDump(string path_in, string path_out) {
        RunExecutable(PathConstants.HKDumpExecutablePath, path_in, path_out);
    }

    public static void HKConv(string path_in, string path_out) {
    }

}
