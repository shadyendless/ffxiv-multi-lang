using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Memory;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVMultiLang.Utils;
using Lumina.Excel.GeneratedSheets;
using System.Text;

namespace FFXIVMultiLang;

public unsafe class ItemDetailAugment
{
    private FFXIVMultiLang plugin;
    private Configuration configuration;

    public ItemDetailAugment(FFXIVMultiLang Plugin)
    {
        plugin = Plugin;
        configuration = Plugin.Configuration;
    }

    public void RequestedUpdate(AddonEvent type, AddonArgs args)
    {
        HandleLanguageChanged();
    }

    public void HandleLanguageChanged()
    {
        if (!configuration.SwapItemDetailLanguage) return;

        var itemId = Services.GameGui.HoveredItem;

        if (itemId == 0) return;

        var isHq = itemId > 1000000 && itemId < 2000000;
        var isEventItem = itemId > 2000000;

        if (isEventItem)
        {
            EventItem? item = Services.DataManager.GetExcelSheet<EventItem>(configuration.ConfiguredLanguage)!.GetRow((uint)(itemId));

            if (item == null) return;

            var stringArrayData = (AtkStage.Instance()->GetStringArrayData())[26];

            var nameStr = UpdateItemTooltipName(item);
            var categoryStr = UpdateItemTooltipCategory(item);
            var descriptionStr = UpdateItemTooltipDescription(item);

            stringArrayData->SetValue(ItemTooltipField.ItemName, nameStr, false, true, false);
            stringArrayData->SetValue(ItemTooltipField.ItemUiCategory, categoryStr, false, true, false);
            stringArrayData->SetValue(ItemTooltipField.ItemDescription, descriptionStr, false, true, false);
        }
        else
        {
            Item? item = Services.DataManager.GetExcelSheet<Item>(configuration.ConfiguredLanguage)!.GetRow((uint)(itemId % 500000));

            if (item == null) return;

            var stringArrayData = (AtkStage.Instance()->GetStringArrayData())[26];

            var nameStr = UpdateItemTooltipName(item);
            var categoryStr = UpdateItemTooltipCategory(item);
            var descriptionStr = UpdateItemTooltipDescription(item);

            var recastLabel = Services.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(702)?.Text ?? string.Empty;


            stringArrayData->SetValue(ItemTooltipField.ItemName, nameStr, false, true, false);
            stringArrayData->SetValue(ItemTooltipField.ItemUiCategory, categoryStr, false, true, false);
            stringArrayData->SetValue(ItemTooltipField.ItemDescription, descriptionStr, false, true, false);
            stringArrayData->SetValue(ItemTooltipField.ItemRecastLabel, recastLabel, false, true, false);
        }

        RaptureAtkUnitManager.Instance()->GetAddonByName("ItemDetail")->OnRequestedUpdate(
            AtkStage.Instance()->GetNumberArrayData(),
            AtkStage.Instance()->GetStringArrayData()
        );
    }

    private static unsafe SeString GetTooltipString(StringArrayData* stringArrayData, int field)
    {
        var stringAddress = new nint(stringArrayData->StringArray[field]);

        return stringAddress != nint.Zero ? MemoryHelper.ReadSeStringNullTerminated(stringAddress) : new SeString();
    }

    private byte[] UpdateItemTooltipName(Item item)
    {
        return new Lumina.Text.SeString(
            MacroString.ProcessMacroString(item.Name, configuration.ConfiguredLanguage).Data.ToArray()
        ).ToDalamudString().Append(Services.GameGui.HoveredItem > 500000 ? " \xE03C" : "").Encode();
    }
    private byte[] UpdateItemTooltipName(EventItem item)
    {
        var name = item.Name.ToString().Length > 0 ? item.Name : item.Singular;

        return new Lumina.Text.SeString(
            MacroString.ProcessMacroString(name, configuration.ConfiguredLanguage).Data.ToArray()
        ).ToDalamudString().Encode();
    }

    private byte[] UpdateItemTooltipCategory(Item item)
    {
        var localizedCategory = item.ItemUICategory.Value?.Name;

        if (localizedCategory == null) return [];

        return new Lumina.Text.SeString(
            MacroString.ProcessMacroString(localizedCategory, configuration.ConfiguredLanguage).Data.ToArray()
        ).ToDalamudString().Encode();
    }
    private byte[] UpdateItemTooltipCategory(EventItem item)
    {
        var localizedQuestLabel = Services.DataManager.GetExcelSheet<Addon>(configuration.ConfiguredLanguage)?.GetRow(12723)?.Text ?? string.Empty;
        var questName = item.Quest.Value?.Name;

        if (questName == null || questName == string.Empty) return [];

        return Encoding.UTF8.GetBytes($"{localizedQuestLabel}: {questName}");
    }

    private byte[] UpdateItemTooltipDescription(Item item)
    {
        if (item == null) return [];

        return new Lumina.Text.SeString(
            MacroString.ProcessMacroString(item.Description, configuration.ConfiguredLanguage).Data.ToArray()
        ).ToDalamudString().Encode();
    }
    private byte[] UpdateItemTooltipDescription(EventItem item)
    {
        var description = Services.DataManager.GetExcelSheet<EventItemHelp>(configuration.ConfiguredLanguage)?.GetRow(item.RowId)?.Description;

        if (description == null) return [];

        var name = item.Name.ToString().Length > 0 ? item.Name : item.Singular;

        return new Lumina.Text.SeString(
            MacroString.ProcessMacroString(description, configuration.ConfiguredLanguage).Data.ToArray()
        ).ToDalamudString().Encode();
    }

}

