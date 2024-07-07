using Dalamud.Configuration;
using Dalamud.Game;
using Dalamud.Plugin;
using System;

namespace FFXIVMultiLang;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public ClientLanguage ConfiguredLanguage { get; set; } = 0;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        FFXIVMultiLang.PluginInterface.SavePluginConfig(this);
    }
}
