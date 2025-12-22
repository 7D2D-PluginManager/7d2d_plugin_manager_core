using System;

namespace PluginManager.Core.Mappers;

public static class EnumMapper<TFrom, TTo>
    where TFrom : struct, Enum
    where TTo : struct, Enum
{
    public static TTo Map(TFrom value) => (TTo)(object)value;

    public static TFrom MapBack(TTo value) => (TFrom)(object)value;
}