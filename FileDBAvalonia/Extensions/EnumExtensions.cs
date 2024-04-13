using System;
using System.Linq;

namespace FileDBAvalonia.Extensions;

public static class EnumExtensions
{
    public static T GetAttribute<T>(this Enum enumVal) where T : Attribute
    {
        var type = enumVal.GetType();
        var memInfo = type.GetMember(enumVal.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
        return (T)attributes.First();
    }
}
