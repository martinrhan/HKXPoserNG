using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKXPoserNG;

public static class ExternalPrograms {
    public static void HKDump(string path_in, string path_out) {
        ProcessStartInfo info = new ProcessStartInfo() {
            FileName = GlobalConstants.HkDumpExecutablePath,
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
                Debug.WriteLine("hkdump: " + e.Data);
                Console.Write(e.Data);
            }
        };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        Console.WriteLine();
        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"hkdump-bin.exe failed with exit code {process.ExitCode}. Make sure the input file is valid and the output path is writable.");
    }

    public static void HKConv(string path_in, string path_out) {
        string curretDirectory = Directory.GetCurrentDirectory();
        ProcessStartInfo info = new ProcessStartInfo(
                Path.Combine(curretDirectory, @"\hkconv.exe"),
                path_in + " -o " + path_out);
        info.UseShellExecute = false;
        info.RedirectStandardOutput = true;
        Process? process = Process.Start(info);
        if (process == null) {
            throw new InvalidOperationException("Failed to start hkconv.exe. Make sure it is in the current directory.");
        }
        Console.WriteLine(process.StandardOutput.ReadToEnd());
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"hkconv.exe failed with exit code {process.ExitCode}. Make sure the input file is valid and the output path is writable.");
    }

}
