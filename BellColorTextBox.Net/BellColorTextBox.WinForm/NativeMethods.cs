using System.Runtime.InteropServices;
using System.Text;

namespace Bell;

internal static class NativeMethods
{
    internal const int WM_IME_COMPOSITION = 0x0010F;
    internal const int WM_IME_STARTCOMPOSITION = 0x0010D;

    private const int GCS_COMPSTR = 0x0008;

    [DllImport("imm32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr ImmGetContext(IntPtr hWnd);

    [DllImport("imm32.dll", CharSet = CharSet.Unicode)]
    private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

    [DllImport("imm32.dll", CharSet = CharSet.Unicode)]
    private static extern int ImmGetCompositionString(IntPtr hIMC, uint dwIndex, byte[]? lpBuf, int dwBufLen);
    
    [DllImport("imm32.dll", CharSet = CharSet.Unicode)]
    private static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM lpCompForm);

    [StructLayout(LayoutKind.Sequential)]
    private struct COMPOSITIONFORM
    {
        public int dwStyle;
        public POINT ptCurrentPos;
        public RECT rcArea;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
    
    internal static string GetCompositionString(IntPtr hWnd)
    {
        IntPtr hIMC = ImmGetContext(hWnd); // Get the Input Context
        try
        {
            if (hIMC != IntPtr.Zero)
            {
                int strLen = ImmGetCompositionString(hIMC, GCS_COMPSTR, null, 0);
                if (strLen > 0)
                {
                    byte[]? buffer = new byte[strLen];
                    ImmGetCompositionString(hIMC, GCS_COMPSTR, buffer, strLen);
                    return Encoding.Unicode.GetString(buffer);
                }
            }
            return string.Empty;
        }
        finally
        {
            ImmReleaseContext(hWnd, hIMC);
        }
    }

    internal static void SetCompositionWindowOffScreen(IntPtr hWnd)
    {
        IntPtr hIMC = ImmGetContext(hWnd);
        try
        {
            if (hIMC != IntPtr.Zero)
            {
                var compForm = new COMPOSITIONFORM
                {
                    dwStyle = 0x0020, // CFS_FORCE_POSITION
                    ptCurrentPos = new POINT { x = -5000, y = -5000 },
                    rcArea = new RECT()
                };

                ImmSetCompositionWindow(hIMC, ref compForm);
                ImmReleaseContext(hWnd, hIMC);
            }
        }
        finally
        {
            ImmReleaseContext(hWnd, hIMC);
        }
    }
}