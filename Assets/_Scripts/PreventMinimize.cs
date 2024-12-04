using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class PreventMinimize : MonoBehaviour
{
    // Import the necessary Windows API functions
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    private WndProcDelegate newWndProc;

    private const int GWL_WNDPROC = -4; // Window procedure offset
    private const uint WM_SYSCOMMAND = 0x0112;
    private const uint SC_MINIMIZE = 0xF020;

    private IntPtr originalWndProc;
    private IntPtr hwnd;

    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public System.Drawing.Point pt;
    }

    void Start()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            hwnd = GetForegroundWindow(); // Get the Unity window handle

            // Get the original window procedure
            originalWndProc = GetWindowLongPtr(hwnd, GWL_WNDPROC);

            // Create a new WndProc delegate
            newWndProc = CustomWndProc;

            // Replace the original WndProc with the custom one
            SetWindowLongPtr(hwnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(newWndProc));
        }
    }

    private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        // Intercept minimize message
        if (msg == WM_SYSCOMMAND && wParam.ToInt32() == SC_MINIMIZE)
        {
            Debug.Log("Minimize action prevented!");
            return IntPtr.Zero; // Block the minimize action
        }

        // Call the original WndProc for other messages
        return CallWindowProc(originalWndProc, hWnd, msg, wParam, lParam);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
}