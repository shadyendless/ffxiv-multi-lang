using Dalamud.Configuration;
using Dalamud.Game;

namespace FFXIVMultiLang;

public delegate void OnConfigurationSaved();

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public ClientLanguage ConfiguredLanguage { get; set; } = 0;
    public bool SwapItemDetailLanguage { get; set; } = true;
    public bool SwapQuestTrackerLanguage { get; set; } = true;
    public bool SwapDutyFinderTrackerLanguage { get; set; } = true;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        FFXIVMultiLang.PluginInterface.SavePluginConfig(this);
    }
}
