using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ZapretUI.Models;

namespace ZapretUI.Services;

public sealed partial class FileParserService
{
    const string _searchWord = "general";
    const string _pattern = @"LOCAL_VERSION=([^\s""]*)";
    static readonly char[] _separator = [' '];

    [GeneratedRegex(_pattern)]
    private static partial Regex RegexPattern();

    public static List<Strategy> LoadAllStrategies(string rootPath)
    {
        List<Strategy> strategies = new();
        string[] files = Directory.GetFiles(rootPath, "*.bat");

        for (int i = 0; i < files.Length; i++)
        {
            var content = File.ReadAllText(files[i]);
            if (Path.GetFileName(files[i]).Contains(_searchWord, StringComparison.OrdinalIgnoreCase))
            {
                Strategy strategy = new()
                {
                    Name = Path.GetFileName(files[i])
                };
                SetStrategyFlags(strategy, content, rootPath);
                strategies.Add(strategy);
            }
        }

        return strategies;
    }

    private static void SetStrategyFlags(Strategy strategy, string content, string rootPath)
    {
        string listReplacement = Path.Combine(rootPath, "lists") + "\\";
        string binReplacement = Path.Combine(rootPath, "bin") + "\\";
        List<string> flagsList = [];
        int dashIndex = content.IndexOf("--");

        if (dashIndex == -1)
            return;

        string flags = content[dashIndex..];
        string[] words = flags.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Contains('^'))
            {
                continue;
            }

            string changedWord = words[i].Replace("%LISTS%", listReplacement);
            changedWord = changedWord.Replace("%BIN%", binReplacement);
            changedWord = changedWord.Replace("%GameFilterUDP%", "12");
            changedWord = changedWord.Replace("%GameFilterTCP%", "12");

            flagsList.Add(changedWord);
        }

        strategy.Flags = flagsList;
        strategy.Arguments = string.Join(" ", flagsList);
    }

    public static string GetSourceVersion(string rootPath)
    {
        string[] files = Directory.GetFiles(rootPath, "*.bat");

        if (files.Length == 0)
            return string.Empty;

        foreach (string file in files)
        {
            using var reader = new StreamReader(file);
            for (int i = 0; i < 3; i++)
            {
                var line = reader.ReadLine();
                if (line == null)
                    continue;

                Match m = RegexPattern().Match(line);
                if (m.Success)
                {
                    return m.Groups[1].Value;
                }
            }
        }

        return string.Empty;
    }
}
