using System;
using System.Linq;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

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
        var itemId = Service.GameGui.HoveredItem;

        Item? originalItem = Service.DataManager.GetExcelSheet<Item>()!.GetRow((uint)(itemId % 500000));
        Item? item = Service.DataManager.GetExcelSheet<Item>(configuration.ConfiguredLanguage)!.GetRow((uint)(itemId % 500000));

        if (item == null) return;

        var stringArrayData = (AtkStage.Instance()->GetStringArrayData())[26];

        var nameStr = GetTooltipString(stringArrayData, ItemTooltipField.ItemName);
        var categoryStr = GetTooltipString(stringArrayData, ItemTooltipField.ItemUiCategory);
        var descriptionStr = GetTooltipString(stringArrayData, ItemTooltipField.ItemDescription);
        var effectsStr = GetTooltipString(stringArrayData, ItemTooltipField.Effects);

        UpdateItemTooltipName(nameStr, item);
        UpdateItemTooltipCategory(categoryStr, originalItem, item);
        UpdateItemTooltipDescription(descriptionStr, originalItem, item);

        stringArrayData->SetValue(ItemTooltipField.ItemName, nameStr.Encode(), false, true, false);
        stringArrayData->SetValue(ItemTooltipField.ItemUiCategory, categoryStr.Encode(), false, true, false);
        stringArrayData->SetValue(ItemTooltipField.ItemDescription, descriptionStr.Encode(), false, true, false);


        var tooltipData = (AtkStage.Instance()->GetStringArrayData())[7];
        tooltipData->SetValue(26, "90001", false, true, false);

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

    private void UpdateItemTooltipName(SeString seStr, Item item)
    {
        if (seStr.ToString() == "") return;
        if (seStr.TextValue.StartsWith('[')) return;

        seStr.Payloads.Clear();
        seStr.Payloads.Add(new TextPayload($"{item.Name}{(Service.GameGui.HoveredItem > 500000 ? " \xE03C" : "")}"));
    }

    private void UpdateItemTooltipCategory(SeString seStr, Item? originalItem, Item? item)
    {
        if (originalItem == null || item == null) return;

        var originalCategory = originalItem.ItemUICategory.Value?.Name;
        var localizedCategory = item.ItemUICategory.Value?.Name;

        if (originalCategory == null || originalCategory == "" || localizedCategory == null || localizedCategory == "") return;

        seStr.Payloads.Clear();
        seStr.Payloads.Add(new TextPayload(localizedCategory));
    }

    private void UpdateItemTooltipDescription(SeString seStr, Item? originalItem, Item? item)
    {
        if (originalItem == null || item == null) return;

        seStr.Payloads.Clear();
        seStr.Payloads.Add(new TextPayload($"{item?.Description}\n"));
    }

    private void UpdateItemTooltipEffects(SeString seStr, Item? originalItem, Item? item)
    {
        if (originalItem == null || item == null) return;

        seStr.Payloads.Add(new TextPayload(item?.Description));
    }

}

