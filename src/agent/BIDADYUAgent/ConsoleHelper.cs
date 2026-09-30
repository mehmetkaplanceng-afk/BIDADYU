using System;
using System.Runtime.InteropServices;

namespace BIDADYUAgent;

public static class ConsoleHelper
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeConsole();

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    public static bool IsConsoleVisible { get; private set; } = false;

    public static void ShowConsole()
    {
        IntPtr handle = GetConsoleWindow();
        if (handle == IntPtr.Zero)
        {
            AllocConsole();
            handle = GetConsoleWindow();
        }

        if (handle != IntPtr.Zero)
        {
            ShowWindow(handle, SW_SHOW);
            IsConsoleVisible = true;
            Console.WriteLine("=========================================================");
            Console.WriteLine("  BİDADYU Agent - Canlı Konsol Log Ekranı");
            Console.WriteLine("=========================================================");
        }
    }

    public static void HideConsole()
    {
        IntPtr handle = GetConsoleWindow();
        if (handle != IntPtr.Zero)
        {
            ShowWindow(handle, SW_HIDE);
            IsConsoleVisible = false;
        }
    }

    public static void ToggleConsole()
    {
        if (IsConsoleVisible)
        {
            HideConsole();
        }
        else
        {
            ShowConsole();
        }
    }
}
