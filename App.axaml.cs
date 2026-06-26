using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using ZapretUI.Models;
using ZapretUI.Services;
using ZapretUI.ViewModels;
using ZapretUI.Views;

namespace ZapretUI
{
    public partial class App : Application
    {
        public static TopLevel? AppTopLevel { get; private set; }
        public static string ZapretFolderPath { get; private set; } = string.Empty;
        public static string AppFolderName { get; private set; } = string.Empty;
        public static string AppFolderPath { get; private set; } = string.Empty;
        public static DataStorageService DataStorageService { get; private set; } = new();
        public static List<Strategy> LoadedStrategies { get; private set; } = [];
        public static HttpClient HttpClient { get; private set; } = null!;
        public static string SaveFileName { get; private set; } = "ZapretUIData.json";
        public static string GitHubSourceDirectory { get; private set; } = "https://github.com/Flowseal/zapret-discord-youtube/releases/latest";
        public static string LoadedGitHubVersion { get; set; } = string.Empty;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            DataContext = new ApplicationViewModel();

            //AppFolderPath = AppContext.BaseDirectory;
            AppFolderPath = "C:\\Users\\Adminka\\Desktop\\xxxv\\ZapretUI";
            AppFolderName = new DirectoryInfo(AppFolderPath).Name;
            ZapretFolderPath = new DirectoryInfo(AppFolderPath).Parent!.FullName;

            var handler = new SocketsHttpHandler
            {
                AllowAutoRedirect = true,
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            };

            HttpClient = new HttpClient(handler);

            HttpClient.DefaultRequestHeaders.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new("application/json"));
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var vm = new MainWindowViewModel();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = vm,
                };

                AppTopLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            }
            
            base.OnFrameworkInitializationCompleted();
        }
    }
}