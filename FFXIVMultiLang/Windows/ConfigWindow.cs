using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace FFXIVMultiLang.Windows;

public delegate void OnConfigurationSaved(string property, bool newValue);

public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;
    public OnConfigurationSaved onConfigurationSaved { get; set; } = delegate { };

    public ConfigWindow(FFXIVMultiLang plugin): base("FFXIV Multi Lang Configuration###FFXIVMLC")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(300, 112);
        SizeCondition = ImGuiCond.Always;

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var swapItemDetailLanguage = configuration.SwapItemDetailLanguage;

        if (ImGui.Checkbox("Swap Item Detail Language", ref swapItemDetailLanguage))
        {
            configuration.SwapItemDetailLanguage = swapItemDetailLanguage;
            configuration.Save();

            onConfigurationSaved.Invoke("SwapItemDetailLanguage", swapItemDetailLanguage);
        }

        var swapQuestTrackerLanguage = configuration.SwapQuestTrackerLanguage;

        if (ImGui.Checkbox("Swap Quest Tracker Language", ref swapQuestTrackerLanguage))
        {
            configuration.SwapQuestTrackerLanguage = swapQuestTrackerLanguage;
            configuration.Save();

            onConfigurationSaved.Invoke("SwapQuestTrackerLanguage", swapQuestTrackerLanguage);

        }

        var swapDutyFinderTrackerLanguage = configuration.SwapDutyFinderTrackerLanguage;

        if (ImGui.Checkbox("Swap Duty Finder Tracker Language", ref swapDutyFinderTrackerLanguage))
        {
            configuration.SwapDutyFinderTrackerLanguage = swapDutyFinderTrackerLanguage;
            configuration.Save();

            onConfigurationSaved.Invoke("SwapDutyFinderTrackerLanguage", swapDutyFinderTrackerLanguage);
        }
    }
}
