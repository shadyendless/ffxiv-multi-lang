using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game;
using Dalamud.Interface.Components;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace FFXIVMultiLang.Windows;

public class MainWindow : Window, IDisposable
{
    private FFXIVMultiLang Plugin;
    private Configuration Configuration;

    private List<ClientLanguage> _availableLanguages = [ClientLanguage.Japanese, ClientLanguage.English, ClientLanguage.German, ClientLanguage.French];
    private int _selectedLanguage = 0;

    public MainWindow(FFXIVMultiLang plugin)
        : base("FFXIV Multi Language##FFXIVMultiLang", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        Configuration = Plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        int selectedIndex = _availableLanguages.IndexOf(Configuration.ConfiguredLanguage);

        var availableLanguages = _availableLanguages
            .Where(lang => lang != ClientLanguage.English).ToList();

        var availableLanguagesStr = availableLanguages
            .Select(lang => lang.ToString());

        ImGui.TextUnformatted($"Client Language: {Service.ClientState.ClientLanguage}");

        if (ImGui.ListBox("Language", ref selectedIndex, availableLanguagesStr.ToArray(), availableLanguages.Count))
        {
            Configuration.ConfiguredLanguage = availableLanguages[selectedIndex];
            Configuration.Save();
        }
    }
}
