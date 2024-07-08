using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game;
using FFXIVMultiLang.Augments;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.Inventory;
using System.Text;

namespace FFXIVMultiLang;

public sealed class FFXIVMultiLang : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    private const string CommandName = "/lang";

    private readonly ItemDetailAugment itemDetailAugment;
    private readonly MonsterNoteAugment monsterNoteAugment;
    private readonly ToDoListAugment toDoListAugment;
    private readonly InventoryAugment inventoryAugment;
    private readonly JournalDetailAugment journalDetailAugment;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("FFXIVMultiLang");

    internal bool ShiftHeld { get; private set; }


    public FFXIVMultiLang(IDalamudPluginInterface pluginInterface)
    {
        Service.Initialize(pluginInterface);

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        itemDetailAugment = new ItemDetailAugment(this);
        monsterNoteAugment = new MonsterNoteAugment(this);
        toDoListAugment = new ToDoListAugment(this);
        inventoryAugment = new InventoryAugment(this);
        journalDetailAugment = new JournalDetailAugment(this);

        Service.Framework.Update += FrameworkOnUpdate;
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "ItemDetail", itemDetailAugment.RequestedUpdate);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "ItemDetail", itemDetailAugment.RequestedUpdate);

        Service.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "MonsterNote", monsterNoteAugment.OnPostMonsterNoteSetup);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "MonsterNote", monsterNoteAugment.OnMonsterNotePreDraw);

        Service.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "_ToDoList", toDoListAugment.OnToDoListPreRequestedUpdate);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "_ToDoList", toDoListAugment.OnToDoListPreDraw);

        Service.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "InventoryGrid0E", inventoryAugment.RequestedUpdate);

        Service.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "JournalDetail", journalDetailAugment.OnJournalDetailRequestedUpdate);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "View additional languages in your game. You can set a language with /lang <language>."
        });
    }

    private void FrameworkOnUpdate(IFramework framework)
    {
        bool shiftState = Service.KeyState[VirtualKey.SHIFT];

        if (shiftState == ShiftHeld) return;

        ShiftHeld = shiftState;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        CommandManager.RemoveHandler(CommandName);

        monsterNoteAugment?.Cleanup();
        Service.AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "ItemDetail", itemDetailAugment.RequestedUpdate);
        Service.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "ItemDetail", itemDetailAugment.RequestedUpdate);

        if (monsterNoteAugment != null)
        {
            Service.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "MonsterNote", monsterNoteAugment.OnPostMonsterNoteSetup);
            Service.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "MonsterNote", monsterNoteAugment.OnMonsterNotePreDraw);
        }


        if (toDoListAugment != null)
        {
            Service.AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "_ToDoList", toDoListAugment.OnToDoListPreRequestedUpdate);
            Service.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "_ToDoList", toDoListAugment.OnToDoListPreDraw);
        }

        if (inventoryAugment != null)
        {
            Service.AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "InventoryGrid0E", inventoryAugment.RequestedUpdate);
        }

        if (journalDetailAugment != null)
        {
            Service.AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "JournalDetail", journalDetailAugment.OnJournalDetailRequestedUpdate);
        }
    }

    private unsafe void OnCommand(string command, string args)
    {
        if (args == "")
        {
            Service.ChatGui.Print(new XivChatEntry
            {
                Message = new SeString(new List<Payload>
                    {
                        new UIForegroundPayload(0),
                        new TextPayload($"Your language is currently set to: "),
                        new UIForegroundPayload(34),
                        new TextPayload($"{Configuration.ConfiguredLanguage}."),
                        new UIForegroundPayload(0),
                        new TextPayload($"\nThe supported languages are: "),
                        new TextPayload($"\n    - Japanese (JP)"),
                        new TextPayload($"\n    - English (EN)"),
                        new TextPayload($"\n    - German (DE)"),
                        new TextPayload($"\n    - French (FR)"),
                        new UIForegroundPayload(0),
                    }),
                Type = XivChatType.Echo
            });
        }
        else
        {
            ClientLanguage inputLanguage = Utils.GetClientLanguageFromInput(args);
            Configuration.ConfiguredLanguage = inputLanguage;

            toDoListAugment.HandleLanguageChanged();
            itemDetailAugment.HandleLanguageChanged();
            inventoryAugment.HandleLanguageChanged();
        }
    }
}
