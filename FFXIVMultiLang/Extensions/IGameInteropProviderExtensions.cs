using Dalamud.Hooking;
using Dalamud.Plugin.Services;

namespace FFXIVMultiLang.Extensions;

public static class IGameInteropProviderExtensions
{
    public static unsafe Hook<T> HookFromVTable<T>(this IGameInteropProvider GameInteropProvider, void* vtblAddress, int vfIndex, T detour) where T : Delegate
        => GameInteropProvider.HookFromVTable((nint)vtblAddress, vfIndex, detour);

    public static unsafe Hook<T> HookFromVTable<T>(this IGameInteropProvider GameInteropProvider, nint vtblAddress, int vfIndex, T detour) where T : Delegate
        => GameInteropProvider.HookFromAddress(*(nint*)(vtblAddress + vfIndex * 0x08), detour);
}
