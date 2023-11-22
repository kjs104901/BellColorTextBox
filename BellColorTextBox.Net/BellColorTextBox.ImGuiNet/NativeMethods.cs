using System.Runtime.InteropServices;
using System.Text;

namespace Bell;

internal static class NativeMethods
{
    [DllImport("imm32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr ImmGetContext(IntPtr hWnd);

    [DllImport("imm32.dll", CharSet = CharSet.Unicode)]
    private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

    [DllImport("imm32.dll", CharSet = CharSet.Unicode)]
    private static extern int ImmGetCompositionString(IntPtr hIMC, uint dwIndex, byte[]? lpBuf, int dwBufLen);

    [DllImport("user32.dll")]
    private static extern IntPtr GetFocus();

    private const uint GCS_COMPSTR = 0x0008;

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern IntPtr SetCursor(IntPtr hCursor);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    private static int IDC_ARROW = 32512;
    private static int IDC_IBEAM = 32513;
    private static int IDC_HAND = 32649;


    internal static IntPtr ArrowCursor = LoadCursor(IntPtr.Zero, IDC_ARROW);
    internal static IntPtr BeamCursor = LoadCursor(IntPtr.Zero, IDC_IBEAM);
    internal static IntPtr HandCursor = LoadCursor(IntPtr.Zero, IDC_HAND);
    
    internal static string GetCompositionString()
    {
        IntPtr hWnd = GetFocus(); // Get the handle to the active window
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
}