using UnityEngine;
using System.Runtime.InteropServices;
using System;
using UnityEngine.UI;
using TMPro;


//Modified asset from https://github.com/Slyt4il/UnityBorderlessToggle
public class BorderlessToggle : MonoBehaviour
{
    [SerializeField] private bool borderless = false;
    private Vector2Int borderSize;

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, ref Rect rect);

    [DllImport("dwmapi.dll", PreserveSig = false)]
    private static extern void DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, uint cbAttribute);

    private enum DWMWINDOWATTRIBUTE : uint
    {
        DWMWA_WINDOW_CORNER_PREFERENCE = 33
    }

    private enum DWM_WINDOW_CORNER_PREFERENCE : uint
    {
        DWMWCP_ROUND = 2
    }

    // Window styles
    private const int GWL_STYLE = -16;
    private const int WS_CAPTION = 0x00C00000;     // Title bar
    private const int WS_THICKFRAME = 0x00040000;  // Resizable border
    private const int WS_MINIMIZEBOX = 0x00020000; // Minimize button
    private const int WS_MAXIMIZEBOX = 0x00010000; // Maximize button
    private const int WS_SYSMENU = 0x00080000;     // System menu

    // Флаги для SetWindowPos
    private const uint SWP_FRAMECHANGED = 0x0020;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;

    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width() { return Right - Left; }
        public int Height() { return Bottom - Top; }
    }

    //Maximiser fields
    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern bool IsWindowVisible(IntPtr hWnd);

    const uint GW_HWNDNEXT = 2;

    const int SW_SHOWMINIMIZED = 2; // {minimize, activation}  
    const int SW_SHOWMAXIMIZED = 3; // maximize  
    const int SW_SHOWRESTORE = 1; // Restore

    void Start()
    {
        if (borderless)
            SetBorderless();
    }

    public void ToggleBorders()
    {
        borderless = !borderless;
        if (borderless)
            SetBorderless();
        else
            SetBordered();
    }

    public void SetBorderless()
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        IntPtr hwnd = GetActiveWindow();
        Rect currentRect = new Rect();
        GetWindowRect(hwnd, ref currentRect);

        // Получаем текущий стиль
        IntPtr currentStyle = GetWindowLongPtr(hwnd, GWL_STYLE);
        
        // Убираем только стили связанные с заголовком, оставляем WS_THICKFRAME для изменения размера
        IntPtr newStyle = new IntPtr(currentStyle.ToInt32() & ~(WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX));
        
        SetWindowLongPtr(hwnd, GWL_STYLE, newStyle);

        // Применяем изменения стиля и принудительно перерисовываем рамку
        SetWindowPos(hwnd, IntPtr.Zero, currentRect.Left, currentRect.Top, 
                    currentRect.Width(), currentRect.Height(), 
                    SWP_FRAMECHANGED | SWP_NOZORDER | SWP_NOMOVE | SWP_NOSIZE);

        borderless = true;
#endif
    }

    public void SetBordered()
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        IntPtr hwnd = GetActiveWindow();
        Rect currentRect = new Rect();
        GetWindowRect(hwnd, ref currentRect);

        // Восстанавливаем стандартные стили окна
        IntPtr currentStyle = GetWindowLongPtr(hwnd, GWL_STYLE);
        IntPtr newStyle = new IntPtr(currentStyle.ToInt32() | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        
        SetWindowLongPtr(hwnd, GWL_STYLE, newStyle);

        // Применяем изменения
        SetWindowPos(hwnd, IntPtr.Zero, currentRect.Left, currentRect.Top, 
                    currentRect.Width(), currentRect.Height(), 
                    SWP_FRAMECHANGED | SWP_NOZORDER | SWP_NOMOVE | SWP_NOSIZE);

        borderless = false;
#endif
    }
    //Maximizer methods
    public void OnClickMinimize()
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        IntPtr gameWindow = GetForegroundWindow();

        // Минимизируем окно игры
        ShowWindow(gameWindow, SW_SHOWMINIMIZED);

        // Находим следующее видимое окно
        IntPtr nextWindow = GetWindow(gameWindow, GW_HWNDNEXT);
        while (nextWindow != IntPtr.Zero && nextWindow != gameWindow)
        {
            if (IsWindowVisible(nextWindow))
            {
                SetForegroundWindow(nextWindow);
                break;
            }
            nextWindow = GetWindow(nextWindow, GW_HWNDNEXT);
        }
#endif
    }


    public void OnClickMaximize()
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        //maximize  
        ShowWindow(GetForegroundWindow(), SW_SHOWMAXIMIZED);
#endif
    }

    public void OnClickRestore()
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        //reduction  
        ShowWindow(GetForegroundWindow(), SW_SHOWRESTORE);
#endif
    }
}

