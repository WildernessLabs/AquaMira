using System;

namespace AquaMira.Core;

internal static class Extensions
{
    public static bool Implements<TInterface>(this Type type)
    {
        return typeof(TInterface).IsAssignableFrom(type);
    }
}
