using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVMultiLang.Utils;

#pragma warning disable 8618
namespace FFXIVMultiLang;

internal class Service
{
    internal static void Initialize(IDalamudPluginInterface pluginInterface) => pluginInterface.Create<Service>();

    public static TextDecoder TextDecoder = new TextDecoder();

    [PluginService]
    internal static IAddonEventManager AddonEventManager { get; private set; }

    [PluginService]
    internal static IAddonLifecycle AddonLifecycle { get; private set; }

    [PluginService]
    internal static IChatGui ChatGui { get; private set; } = null!;

    [PluginService]
    public static IClientState ClientState { get; private set; }

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; }

    [PluginService]
    internal static IDataManager DataManager { get; private set; }

    [PluginService]
    internal static IFramework Framework { get; private set; }

    [PluginService]
    internal static IGameGui GameGui { get; private set; }

    [PluginService]
    internal static IGameInteropProvider GameInteropProvider { get; private set; }

    [PluginService]
    internal static IKeyState KeyState { get; private set; }

    [PluginService]
    internal static IPluginLog PluginLog { get; private set; }

    [PluginService]
    internal static ISigScanner SigScanner { get; private set; }
}
#pragma warning restore 8618
