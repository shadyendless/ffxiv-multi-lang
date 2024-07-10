using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Events;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVMultiLang.Augments;

public class MonsterNoteAugment
{
    private FFXIVMultiLang plugin;
    private Configuration configuration;

    List<IAddonEventHandle> mouseOver = new List<IAddonEventHandle>();
    List<IAddonEventHandle> mouseOut = new List<IAddonEventHandle>();

    private uint currentNode = 16;

    public MonsterNoteAugment(FFXIVMultiLang Plugin)
    {
        plugin = Plugin;
        configuration = Plugin.Configuration;
        configuration = Plugin.Configuration;
    }

    public unsafe void OnPostMonsterNoteSetup(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*) args.Addon;

        for (uint i = 4; i < 14; i++)
        {
            var targetNode = addon->GetNodeById(i);
            targetNode->NodeFlags |= NodeFlags.EmitsEvents | NodeFlags.RespondToMouse | NodeFlags.HasCollision;

            var ev = Services.AddonEventManager.AddEvent((nint)addon, (nint)targetNode, AddonEventType.MouseOver, TooltipHandler);
            if (ev != null) mouseOver.Add(ev);

            ev = Services.AddonEventManager.AddEvent((nint)addon, (nint)targetNode, AddonEventType.MouseOut, TooltipHandler);
            if (ev != null) mouseOut.Add(ev);
        }
    }

    public unsafe void OnMonsterNotePreDraw(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;

        ((AtkUnitBase*)args.Addon)->GetNodeById(1)->SetScale(1, 1);

        // Update Class Header
        ((AtkUnitBase*)addon)->GetNodeById(3)->GetAsAtkTextNode()->SetText(
            Services.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(1882)?.Text ??
            Marshal.PtrToStringUTF8((nint)((AtkUnitBase*)addon)->GetNodeById(3)->GetAsAtkTextNode()->GetText())
        );

        // Update Rank Header
        ((AtkUnitBase*)addon)->GetNodeById(15)->GetAsAtkTextNode()->SetText(
            Services.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(1883)?.Text ??
            Marshal.PtrToStringUTF8((nint)((AtkUnitBase*)addon)->GetNodeById(3)->GetAsAtkTextNode()->GetText())
        );

        // Update Job Subtext
        //((AtkUnitBase*)addon)->GetNodeById(23)->GetAsAtkTextNode()->SetText(
        //    Service.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(1883)?.Text ??
        //    Marshal.PtrToStringUTF8((nint)((AtkUnitBase*)addon)->GetNodeById(23)->GetAsAtkTextNode()->GetText())
        //);
    }

    public void Cleanup()
    {
        foreach (var ev in mouseOver) Services.AddonEventManager.RemoveEvent(ev);
        foreach (var ev in mouseOut) Services.AddonEventManager.RemoveEvent(ev);
    }

    private unsafe void TooltipHandler(AddonEventType type, IntPtr addon, IntPtr node)
    {
        var addonId = ((AtkUnitBase*)addon)->Id;
        var nodeId = ((AtkResNode*)node)->NodeId;


        switch (type)
        {
            case AddonEventType.MouseOver:
                AtkStage.Instance()->TooltipManager.ShowTooltip(
                    addonId, 
                    (AtkResNode*)node, 
                    Localization.Jobs.GetJobNameForMonsterNoteId(
                        nodeId,
                        configuration.ConfiguredLanguage
                    )
                );
                break;

            case AddonEventType.MouseOut:
                AtkStage.Instance()->TooltipManager.HideTooltip(addonId);
                break;
        }
    }
}
