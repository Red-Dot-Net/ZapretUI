using Octokit;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ZapretUI.Helpers;

namespace ZapretUI.Services;

public class SourceDownloadService
{
    public static async Task<Result> GetLatestVersion(string url)
    {
        var httpClient = App.HttpClient;
        using var request = new HttpRequestMessage(HttpMethod.Head, url);

        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return new Error("Ответ не получен");

        Uri? uri = response.RequestMessage?.RequestUri;

        if (uri == null)
            return new Error("URI не получен");

        var parts = uri.ToString().Split('/');
        if (parts.Length == 0)
            return new Error("Тег не распознан");

        return new Success(parts[^1], "Тег получен");
    }

    public static async Task<Result> GetLatestVersionReleaseDownloadUrl(string author, string repo)
    {
        var github = new GitHubClient(new ProductHeaderValue("ZapretUIUpdater"));

        var laatestRelease = await github.Repository.Release.GetLatest(author, repo);
        var asset = laatestRelease.Assets.FirstOrDefault(r => r.Name.EndsWith(".zip"));

        if (asset == null)
            return new Error("Архив не найден");

        if (string.IsNullOrWhiteSpace(asset.BrowserDownloadUrl))
            return new Error("Архив не найден");

        return new Success(asset.BrowserDownloadUrl, "Ссылка успешно получена");
    }
}
