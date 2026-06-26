using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using ZapretUI.Helpers;
using ZapretUI.Models;

namespace ZapretUI.Services;

public sealed class DataStorageService
{
    public ExternalLibraryResources ExternalLibraryResources { get; private set; }
    
    readonly JsonSerializerOptions _options;

    public DataStorageService()
    {
        ExternalLibraryResources = new();

        _options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task SaveStrategies(List<Strategy> strategies)
    {
        ExternalLibraryResources.Strategies = strategies;
        await SaveResources(ExternalLibraryResources, Path.Combine(AppContext.BaseDirectory, App.SaveFileName));
    }

    public async Task SaveSelectedStrategyName(string strategyName)
    {
        ExternalLibraryResources.SelectedStrategyName = strategyName;
        await SaveResources(ExternalLibraryResources, Path.Combine(AppContext.BaseDirectory, App.SaveFileName));
    }

    public async Task SaveCurrentSourceVersion(string sourceVersion)
    {
        ExternalLibraryResources.CurrentSourceVersion = sourceVersion;
        await SaveResources(ExternalLibraryResources, Path.Combine(AppContext.BaseDirectory, App.SaveFileName));
    }

    public async Task SaveBestStrategy(string bestStrategy)
    {
        ExternalLibraryResources.BestStrategy = bestStrategy;
        await SaveResources(ExternalLibraryResources, Path.Combine(AppContext.BaseDirectory, App.SaveFileName));
    }

    public async ValueTask<bool> SaveResources(ExternalLibraryResources resources, string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string jsonString = JsonSerializer.Serialize(resources, _options);
            await File.WriteAllTextAsync(filePath, jsonString);
            ExternalLibraryResources = resources;
            return true;
        }
        catch { }

        return false;
    }

    public async Task<Result> LoadResources(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new Error($"По указанному пути: {filePath} файла не существует.");
            }

            string jsonString = await File.ReadAllTextAsync(filePath);
            var res = JsonSerializer.Deserialize<ExternalLibraryResources>(jsonString, _options);
            if (res != null)
            {
                ExternalLibraryResources = res;
                return new Success(res, "");
            }

            return new Error("Десериализация не выполнена.");
        }
        catch { }

        return new Error("Десериализация не выполнена.");
    }
}
