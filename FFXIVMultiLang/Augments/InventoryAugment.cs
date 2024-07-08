using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVMultiLang;

public unsafe class InventoryAugment
{
    private FFXIVMultiLang plugin;
    private Configuration configuration;

    private int intentoryBlockSize = 35;

    public InventoryAugment(FFXIVMultiLang Plugin)
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
        return;

        ProcessInventoryType(InventoryType.Inventory1, 1);

        RaptureAtkUnitManager.Instance()->GetAddonByName("ItemDetail")->OnRequestedUpdate(
            AtkStage.Instance()->GetNumberArrayData(),
            AtkStage.Instance()->GetStringArrayData()
        );
    }

    private void ProcessInventoryType(InventoryType inventoryType, int inventoryNumber)
    {
        var stringArrayData = (AtkStage.Instance()->GetStringArrayData())[6];

        for (var i = 0; i < intentoryBlockSize; i++)
        {
            var invItem = InventoryManager.Instance()->GetInventorySlot(inventoryType, i);

            if (invItem == null) continue;

            var item = Service.DataManager.GetExcelSheet<Item>(configuration.ConfiguredLanguage)?.GetRow(invItem->ItemId);

            if (item == null) continue;

            var isHq = (invItem->Flags & InventoryItem.ItemFlags.HighQuality) != 0;

            var currentStringIndex = i + ((inventoryNumber - 1) * intentoryBlockSize);
            var newString = $"{item.Name}{(isHq ? "\xE03C" : "")}";

            Service.PluginLog.Info($"[{currentStringIndex}] {newString}");

            stringArrayData->SetValue(currentStringIndex, $"{item.Name}{(isHq ? "\xE03C" : "")}", false, true, true);
        }
    }
}

