using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class NativeTitleBar : MonoBehaviour
{
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    //[DllImport("user32.dll")]
    //private static extern IntPtr GetActiveWindow();

    //[DllImport("dwmapi.dll")]
    //private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    //private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    //void Start()
    //{
    //    var hwnd = GetActiveWindow();
        
    //    // Включаем иммерсивный темный режим для заголовка
    //    // 1 = темный, 0 = светлый
    //    int useDarkMode = 1; 
    //    int result = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
        
    //    if (result == 0)
    //    {
    //        Debug.Log("Атрибут иммерсивного темного режима успешно применен.");
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Не удалось применить атрибут. Код ошибки: " + result);
    //    }
    //}
#endif
}