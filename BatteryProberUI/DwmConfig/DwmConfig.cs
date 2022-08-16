using System;
using System.Runtime.InteropServices;

namespace BatteryProberUI
{
    public static class DwmConfig
    {
        public enum DWM_SYSTEMBACKDROP_TYPE
        {
            Mica = 2,
            Acyrlic = 3,
            Tabbed = 4
        }
        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_SYSTEMBACKDROP_TYPE = 38
        }

        public struct MARGINS
        {
            public int cxLeftWidth;   
            public int cxRightWidth;  
            public int cyTopHeight;   
            public int cyBottomHeight;
        };

        [DllImport("DwmApi.dll")]
        static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref MARGINS pMarInset);

        [DllImport("DwmApi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbAttribute);

        public static int ExtendFrame(IntPtr hwnd, MARGINS margins)
            => DwmExtendFrameIntoClientArea(hwnd, ref margins);

        public static int SetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, int parameter)
            => DwmSetWindowAttribute(hwnd, attribute, ref parameter, Marshal.SizeOf<int>());
    }
}
