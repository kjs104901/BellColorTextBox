using System.Runtime.InteropServices;

namespace BellColorTextBox.Demo.Net
{
    internal class Program
    {
        #region Windows API
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 1;
        #endregion

        static void Main(string[] args)
        {
            // Hide console
            ShowWindow(GetConsoleWindow(), SW_HIDE);

            var imGuiThread = ImGuiDemo.ThreadStart();
            imGuiThread.Join();
        }
    }
}