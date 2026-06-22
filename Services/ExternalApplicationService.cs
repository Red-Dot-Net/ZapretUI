using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ZapretUI.Helpers;
using ZapretUI.Models;

namespace ZapretUI.Services;

public static class ExternalApplicationService
{
    public static async Task<Result> Launch(string appBaseDirectory, Strategy strategy)
    {
        string binPath = Path.Combine(appBaseDirectory, "bin") + "\\";
        string winwsPath = Path.Combine(binPath, "winws.exe");

        if (!File.Exists(winwsPath))
            return new Error($"Исполняемый файл winws.exe (URI: {winwsPath}) не найден. Приложение не может быть запущено");

        string arguments = strategy.Arguments;

        ProcessStartInfo startInfo = new()
        {
            FileName = winwsPath,
            Arguments = arguments,
            UseShellExecute = true,
            CreateNoWindow = true,
            Verb = "runas",
            WindowStyle = ProcessWindowStyle.Hidden
        };

        try
        {
            Process? process = Process.Start(startInfo);

            await Task.Delay(2000);

            if (process == null)
                return new Error("Попытка запуска приложения winws.exe вызвала ошибку.");

            if (Process.GetProcessesByName("winws").Length < 1)
                return new Error("Попытка запуска приложения winws.exe вызвала ошибку.");

            return new Success(process, "Успешно запущен.");
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            return new Error($"Попытка запуска приложения winws.exe вызвала ошибку: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new Error($"Попытка запуска приложения winws.exe вызвала ошибку: {ex.Message}");
        }
    }

    public static bool AreTcpTimestampsEnabled()
    {
        try
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "interface tcp show global",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains("timestamps") &&
                   output.Contains("enabled") &&
                   !output.Contains("disabled");
        }
        catch
        {
            return false;
        }
    }

    public static Result EnableTcpTimestamps()
    {
        try
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "interface tcp set global timestamps=enabled",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
                return new Success(0, "TCP timestamps enabled successfully");
            else
                return new Error("Failed to enable TCP timestamps");
        }
        catch (Exception ex)
        {
            return new Error($"Error enabling TCP timestamps: {ex.Message}");
        }
    }

    public static void KillExistingProcesses(string name)
    {
        var processes = Process.GetProcessesByName(name);
        if (processes.Length == 0)
            return;

        foreach (var process in processes)
        {
            process.Kill();
            process.Dispose();
        }
    }
}
