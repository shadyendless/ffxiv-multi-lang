using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Dalamud.Game;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVMultiLang;

public unsafe class ItemTooltipAugment
{
    private FFXIVMultiLang _Plugin;
    private Configuration _Configuration;

    private const int AlreadyProcessed = 0x40000000;

    public ItemTooltipAugment(FFXIVMultiLang Plugin)
    {
        _Plugin = Plugin;
        _Configuration = Plugin.Configuration;
    }

    public void RequestedUpdate(AddonEvent type, AddonArgs args)
    {
        if (args is not AddonRequestedUpdateArgs requestedUpdateArgs) return;

        var itemId = Service.GameGui.HoveredItem;

        Item? originalItem = Service.DataManager.GetExcelSheet<Item>()!.GetRow((uint)(itemId % 500000));
        Item? item = Service.DataManager.GetExcelSheet<Item>(_Configuration.ConfiguredLanguage)!.GetRow((uint)(itemId % 500000));
        Addon? addon = Service.DataManager.GetExcelSheet<Addon>(_Configuration.ConfiguredLanguage)!.GetRow((uint)(item?.ItemAction.Value.RowId % 500000));


        if (item == null) return;

        var stringArrayData = ((StringArrayData**)requestedUpdateArgs.StringArrayData)[26];

        Service.PluginLog.Info(String.Join(", ", item?.ItemAction.Value.Data.Select(i => i.ToString())));
        Service.PluginLog.Info(String.Join(", ", item?.ItemAction.Value.DataHQ.Select(i => i.ToString())));

        for (var i = 0; i < 50; ++i)
        {
            Service.PluginLog.Info($"[{i}] {GetTooltipString(stringArrayData, i).ToString()}");
        }

        var nameStr = GetTooltipString(stringArrayData, ItemTooltipField.ItemName);
        var categoryStr = GetTooltipString(stringArrayData, ItemTooltipField.ItemUiCategory);
        var descriptionStr = GetTooltipString(stringArrayData, ItemTooltipField.ItemDescription);

        UpdateItemTooltipName(nameStr, nameStr.ToString(), item);
        UpdateItemTooltipCategory(categoryStr, originalItem, item);
        UpdateItemTooltipDescription(descriptionStr, originalItem, item);

        stringArrayData->SetValue(ItemTooltipField.ItemName, nameStr.Encode(), false, true, true);
        stringArrayData->SetValue(ItemTooltipField.ItemUiCategory, categoryStr.Encode(), false, true, true);
        stringArrayData->SetValue(ItemTooltipField.ItemDescription, descriptionStr.Encode(), false, true, true);
    }

    private static unsafe SeString GetTooltipString(StringArrayData* stringArrayData, int field)
    {
        var stringAddress = new nint(stringArrayData->StringArray[field]);
        return stringAddress != nint.Zero ? MemoryHelper.ReadSeStringNullTerminated(stringAddress) : new SeString();
    }

    private void UpdateItemTooltipName(SeString seStr, string originalItemName, Item item)
    {
        if (seStr.TextValue.StartsWith('[')) return;

        seStr.Payloads.Clear();
        seStr.Payloads.Add(new TextPayload($"{originalItemName}\n"));
        seStr.Payloads.Add(new TextPayload(item.Name));
    }

    private void UpdateItemTooltipCategory(SeString seStr, Item? originalItem, Item? item)
    {
        if (originalItem == null || item == null) return;

        var originalCategory = originalItem.ItemUICategory.Value?.Name;
        var localizedCategory = item.ItemUICategory.Value?.Name;

        if (originalCategory == null || originalCategory == "" || localizedCategory == null || localizedCategory == "") return;

        seStr.Payloads.Clear();
        seStr.Payloads.Add(new TextPayload($"{originalCategory} — "));
        seStr.Payloads.Add(new TextPayload(localizedCategory));
    }

    private void UpdateItemTooltipDescription(SeString seStr, Item? originalItem, Item? item)
    {
        if (originalItem == null || item == null) return;

        seStr.Payloads.Clear();
        seStr.Payloads.Add(new TextPayload($"{originalItem?.Description}\n\n"));
        seStr.Payloads.Add(new TextPayload($"{item?.Description}\n"));
    }

    private void UpdateItemTooltipEffects(SeString seStr, Item? originalItem, Item? item)
    {
        if (originalItem == null || item == null) return;

        seStr.Payloads.Add(new TextPayload($"{originalItem?.Description}\n\n{item?.Description}\n"));
    }
}

