using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVMultiLang.Windows;

namespace FFXIVMultiLang;

public sealed class FFXIVMultiLang : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    private const string CommandName = "/xivl";

    private readonly ItemTooltipAugment itemTooltipAugment;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("FFXIVMultiLang");
    private MainWindow MainWindow { get; init; }

    internal bool ShiftHeld { get; private set; }


    public FFXIVMultiLang(IDalamudPluginInterface pluginInterface)
    {
        Service.Initialize(pluginInterface);

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        MainWindow = new MainWindow(this);
        itemTooltipAugment = new ItemTooltipAugment(this);

        Service.Framework.Update += FrameworkOnUpdate;

        Service.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "ItemDetail", itemTooltipAugment.RequestedUpdate);

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "View additional languages in your game."
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    private void FrameworkOnUpdate(IFramework framework)
    {
        bool shiftState = Service.KeyState[VirtualKey.CONTROL];

        if (shiftState = ShiftHeld) return;

        ShiftHeld = shiftState;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        MainWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);

        Service.AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "ItemDetail", itemTooltipAugment.RequestedUpdate);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleMainUI() => MainWindow.Toggle();
}
