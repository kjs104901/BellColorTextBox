using Bell.Inputs;

namespace Bell.Utils;

internal static class EnumFlag
{
    internal static bool Has(HotKeys value, HotKeys subValue) { return (value & subValue) == subValue; }
}