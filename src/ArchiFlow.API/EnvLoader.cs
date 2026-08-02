using System;
using System.IO;
using System.Threading.Tasks;

namespace ArchiFlow.API;

public static class EnvLoader
{
    public static async Task LoadAsync()
    {
        var envPath = FindEnvFile();
        if (!string.IsNullOrEmpty(envPath))
        {
            await ParseEnvFileAsync(envPath);
        }
    }

    private static string? FindEnvFile()
    {
        var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
        if (File.Exists(envPath))
        {
            return envPath;
        }

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var tempPath = Path.Combine(dir.FullName, ".env");
            if (File.Exists(tempPath))
            {
                return tempPath;
            }
            dir = dir.Parent;
        }

        return null;
    }

    private static async Task ParseEnvFileAsync(string path)
    {
        foreach (var line in await File.ReadAllLinesAsync(path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
            }
        }
    }
}
