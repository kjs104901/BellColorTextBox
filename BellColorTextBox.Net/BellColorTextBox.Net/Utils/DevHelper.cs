namespace Bell.Utils;

public static class DevHelper
{
#if DEBUG
    public const bool IsDebugMode = false;
#else
    public const bool IsDebugMode = false;
#endif
}