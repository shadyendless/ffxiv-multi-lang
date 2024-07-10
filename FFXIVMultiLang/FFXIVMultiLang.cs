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
using InteropGenerator.Runtime;
using FFXIVMultiLang.Hooks;
using FFXIVMultiLang.Windows;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace FFXIVMultiLang;

public unsafe sealed class FFXIVMultiLang : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    private const string CommandName = "/lang";

    private readonly ItemDetailAugment itemDetailAugment;
    private readonly MonsterNoteAugment monsterNoteAugment;
    private readonly ToDoList_QuestTrackerAugment toDoList_QuestTrackerAugment;
    private readonly ToDoList_DutyFinderAugment toDoList_DutyFinderAugment;
    private readonly InventoryAugment inventoryAugment;
    private readonly JournalDetailAugment journalDetailAugment;
    private readonly GetClientLanguageHook getClientLanguageHook;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("FFXIVMultiLang");
    private ConfigWindow ConfigWindow { get; init; }

    internal bool ShiftHeld { get; private set; }


    public FFXIVMultiLang(IDalamudPluginInterface pluginInterface)
    {
        Services.Initialize(pluginInterface);

        FFXIVClientStructs.Interop.Generated.Addresses.Register();
        Resolver.GetInstance.Setup(
        Services.SigScanner.SearchBase,
            Services.DataManager.GameData.Repositories["ffxiv"].Version,
            new FileInfo(Path.Join(pluginInterface.ConfigDirectory.FullName, "SigCache.json")));
        Resolver.GetInstance.Resolve();

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        itemDetailAugment = new ItemDetailAugment(this);
        monsterNoteAugment = new MonsterNoteAugment(this);
        toDoList_QuestTrackerAugment = new ToDoList_QuestTrackerAugment(this);
        toDoList_DutyFinderAugment = new ToDoList_DutyFinderAugment(this);
        inventoryAugment = new InventoryAugment(this);
        journalDetailAugment = new JournalDetailAugment(this);
        getClientLanguageHook = new GetClientLanguageHook();

        Services.Framework.Update += FrameworkOnUpdate;
        Services.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "ItemDetail", itemDetailAugment.RequestedUpdate);
        Services.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "ItemDetail", itemDetailAugment.RequestedUpdate);

        Services.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "MonsterNote", monsterNoteAugment.OnPostMonsterNoteSetup);
        Services.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "MonsterNote", monsterNoteAugment.OnMonsterNotePreDraw);

        toDoList_DutyFinderAugment.Initialize();
        toDoList_QuestTrackerAugment.Initialize();

        Services.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "InventoryGrid0E", inventoryAugment.RequestedUpdate);

        Services.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "JournalDetail", journalDetailAugment.OnJournalDetailRequestedUpdate);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "View additional languages in your game. You can set a language with /lang <language>."
        });

        ConfigWindow = new ConfigWindow(this);
        ConfigWindow.onConfigurationSaved += TriggerUpdates;

        WindowSystem.AddWindow(ConfigWindow);

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
    }

    private void FrameworkOnUpdate(IFramework framework)
    {
        bool shiftState = Services.KeyState[VirtualKey.SHIFT];

        if (shiftState == ShiftHeld) return;

        ShiftHeld = shiftState;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        CommandManager.RemoveHandler(CommandName);
        monsterNoteAugment?.Cleanup();

        UnregisterAugmentListeners();

        getClientLanguageHook.Dispose();
    }

    private void UnregisterAugmentListeners()
    {
        if (itemDetailAugment != null)
        {
            Services.AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "ItemDetail", itemDetailAugment.RequestedUpdate);
            Services.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "ItemDetail", itemDetailAugment.RequestedUpdate);
        }

        if (monsterNoteAugment != null)
        {
            Services.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "MonsterNote", monsterNoteAugment.OnPostMonsterNoteSetup);
            Services.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "MonsterNote", monsterNoteAugment.OnMonsterNotePreDraw);
        }

        toDoList_DutyFinderAugment?.Cleanup();
        toDoList_QuestTrackerAugment?.Cleanup();

        if (inventoryAugment != null)
        {
            Services.AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "InventoryGrid0E", inventoryAugment.RequestedUpdate);
        }

        if (journalDetailAugment != null)
        {
            Services.AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "JournalDetail", journalDetailAugment.OnJournalDetailRequestedUpdate);
        }
    }

    private unsafe void OnCommand(string command, string args)
    {
        if (args == "")
        {
            Services.ChatGui.Print(new XivChatEntry
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
        else if (args == "config" || args == "set" || args == "settings" || args == "s" || args == "c")
        {
            ToggleConfigUI();
        }
        else
        {
            ClientLanguage inputLanguage = Utils.Command.GetClientLanguageFromInput(args);
            Configuration.ConfiguredLanguage = inputLanguage;
            Configuration.Save();
        }
    }

    public void TriggerUpdates(string property, bool newValue)
    {
        switch (property)
        {
            case "SwapDutyFinderTrackerLanguage":
                if (newValue) toDoList_DutyFinderAugment?.Initialize();
                else toDoList_DutyFinderAugment?.Cleanup();
                break;

            case "SwapQuestTrackerLanguage":
                if (newValue) toDoList_QuestTrackerAugment?.Initialize();
                else toDoList_QuestTrackerAugment?.Cleanup();
                break;
        }

        UIState.Instance()->DirectorTodo.IsFullUpdatePending = true;

        //var numberArrayData = (AtkStage.Instance()->GetNumberArrayData())[27];
        //numberArrayData->SetValue(1, 0, true, false);
        //numberArrayData->SetValue(2, 1, true, false);
        //numberArrayData->SetValue(1, 0, true, false);
        //numberArrayData->SetValue(2, 1, true, false);

        //RaptureAtkUnitManager.Instance()->GetAddonByName("_ToDoList")->OnRequestedUpdate(
        //    AtkStage.Instance()->GetNumberArrayData(),
        //    AtkStage.Instance()->GetStringArrayData()
        //);

        //RaptureAtkUnitManager.Instance()->GetAddonByName("_ToDoList")->OnRequestedUpdate(
        //    AtkStage.Instance()->GetNumberArrayData(),
        //    AtkStage.Instance()->GetStringArrayData()
        //);


        //toDoList_QuestTrackerAugment?.Refresh();

        //itemDetailAugment.HandleLanguageChanged();
        //inventoryAugment.HandleLanguageChanged();
    }

    private void DrawUI() => WindowSystem.Draw();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
