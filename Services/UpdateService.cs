using Avalonia.Platform.Storage;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using ZapretUI.Helpers;

namespace ZapretUI.Services;
public static class UpdateService
{
    public static async Task<Result> ExtractZipAsync(IStorageFile zipFile)
    {
        var storageProvider = App.AppTopLevel?.StorageProvider;
        if (storageProvider == null)
            return new Error("Ошибка инициализации приложения");

        var basePath = App.DataStorageService.ExternalLibraryResources.FolderPath;
        if (string.IsNullOrEmpty(basePath))
        {
            return new Error("Ошибка: Путь к корневой папке не указан");
        }

        try
        {
            var destFolderPath = Path.Combine(basePath, "temp");
            Directory.CreateDirectory(destFolderPath);

            await using var stream = await zipFile.OpenReadAsync();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                string fullPath = Path.Combine(destFolderPath, entry.FullName);

                fullPath = fullPath.Replace('/', Path.DirectorySeparatorChar)
                                   .Replace('\\', Path.DirectorySeparatorChar);

                if (entry.FullName.EndsWith('/') || entry.Length == 0)
                {
                    var dir = await storageProvider.TryGetFolderFromPathAsync(fullPath);
                    if (dir == null)
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                }
                else
                {
                    string? parentDir = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(parentDir))
                    {
                        var parent = await storageProvider.TryGetFolderFromPathAsync(parentDir);
                        if (parent == null)
                        {
                            Directory.CreateDirectory(parentDir);
                        }
                    }

                    using var fileStream = File.Create(fullPath);
                    await using var entryStream = entry.Open();
                    await entryStream.CopyToAsync(fileStream);
                }
            }
        }
        catch
        {
            return new Error("Ошибка распаковки файлов архива");
        }

        return new Success(null, "Успешно распаковано");
    }

    public static void DeleteContents(string basePath)
    {
        if (!Directory.Exists(basePath))
            return;

        ProcessDirectory(basePath);
    }

    public static void ProcessDirectory(string currentDir)
    {
        var files = Directory.GetFiles(currentDir);
        foreach (string file in files)
        {
            DeleteFileSafely(file);
        }

        var subdirs = Directory.GetDirectories(currentDir);
        foreach (string subDir in subdirs)
        {
            var folderInfo = new DirectoryInfo(subDir).Name;
            if (folderInfo.Equals(App.AppFolderName) || folderInfo.Equals("temp"))
                continue;

            ProcessDirectory(subDir);
        }

        try
        {
            var folderInfo = new DirectoryInfo(currentDir).Name;
            if (folderInfo.Equals(App.AppFolderName) || folderInfo.Equals("temp"))
                return;

            Directory.Delete(currentDir, false);
        }
        catch { }
    }

    public static void DeleteFileSafely(string filePath)
    {
        try
        {
            var fileName = new FileInfo(filePath).Name;

            if (!fileName.Equals(App.SaveFileName))
                File.Delete(filePath);
        }
        catch { }
    }

    public static void CopyDirectory(string sourceDir, string destDir)
    {
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        var files = Directory.GetFiles(sourceDir);
        foreach (string filePath in files)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                string destFilePath = Path.Combine(destDir, fileName);
                File.Copy(filePath, destFilePath, true);
            }
            catch { }
        }

        var subdirs = Directory.GetDirectories(sourceDir);
        foreach (string subDirPath in subdirs)
        {
            string subDirName = Path.GetFileName(subDirPath);
            string destSubDirPath = Path.Combine(destDir, subDirName);
            CopyDirectory(subDirPath, destSubDirPath);
        }
    }
}
