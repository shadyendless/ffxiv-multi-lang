using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Events;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Lumina.Excel.GeneratedSheets;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace FFXIVMultiLang.Augments;

public class JournalDetailAugment
{
    private FFXIVMultiLang plugin;
    private Configuration configuration;

    private uint currentNode = 38;

    public JournalDetailAugment(FFXIVMultiLang Plugin)
    {
        plugin = Plugin;
        configuration = Plugin.Configuration;
        configuration = Plugin.Configuration;
    }

    public unsafe void OnJournalDetailRequestedUpdate(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;

        ((AtkUnitBase*)args.Addon)->GetNodeById(currentNode)->SetScale(1, 1);
    }
}
