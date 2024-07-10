using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FFXIVMultiLang.Hooks;

public class GetClientLanguageHook : IDisposable
{
    private unsafe delegate void GetItemDetail_GenerateTooltipDelegate(AtkEventListener* listener, long a2, long a3);

    [Signature("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC 50 48 8B 42 20", DetourName = nameof(DetourItemDetail_GenerateTooltip))]
    private Hook<GetItemDetail_GenerateTooltipDelegate>? getItemDetail_GenerateTooltipHook;

    public GetClientLanguageHook()
    {
        Services.GameInteropProvider.InitializeFromAttributes(this);

        Services.PluginLog.Info("ItemDetail_GenerateTooltip initialized!");
        getItemDetail_GenerateTooltipHook?.Enable();
    }

    public void Dispose()
    {
        Services.PluginLog.Info("ItemDetail_GenerateTooltip disposed!");
        getItemDetail_GenerateTooltipHook?.Dispose();
    }

    private unsafe void DetourItemDetail_GenerateTooltip(AtkEventListener* listener, long a2, long a3)
    {
        getItemDetail_GenerateTooltipHook!.Original(listener, a2, a3);

        Services.PluginLog.Info($"Start Hook for ItemDetail_GenerateTooltip");
        Services.PluginLog.Info($"Got Received {a2}, {a3}");
        Services.PluginLog.Info($"End Hook for ItemDetail_GenerateTooltip");
    }
}
