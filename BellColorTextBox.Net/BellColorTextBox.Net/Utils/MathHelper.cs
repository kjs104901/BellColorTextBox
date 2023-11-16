namespace Bell.Utils;

internal static class MathHelper
{
    private const float FloatTolerance = 0.1f;

    private static bool IsSame(float r, float l) => Math.Abs(r - l) < FloatTolerance;
    internal static bool IsNotSame(float r, float l) => !IsSame(r, l);
}