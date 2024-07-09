using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FFXIVMultiLang.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddIServices<T>(this IServiceCollection collection)
    {
        var iType = typeof(T);
        foreach (var type in Assembly.GetCallingAssembly().ExportedTypes.Where(t => t is { IsInterface: false, IsAbstract: false } && iType.IsAssignableFrom(t)))
        {
            if (collection.All(t => t.ServiceType != type))
                collection.AddSingleton(iType, type);
        }
        return collection;
    }
}
