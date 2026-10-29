using System;
using System.Runtime.InteropServices;

namespace LiteMonitor.src.Plugins

{
    /// <summary>
    /// 全屏时停止后台执行插件
    /// </summary>
    public static class FullscreenDetector
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        /// <summary>
        /// 是否有前台独占全屏窗口（游戏全屏、播放器全屏）
        /// </summary>
        public static bool IsForegroundFullScreen()
        {
            var hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return false;

            GetWindowRect(hWnd, out var rect);
            int screenW = GetSystemMetrics(SM_CXSCREEN);
            int screenH = GetSystemMetrics(SM_CYSCREEN);

            // 窗口大小等于主屏幕分辨率，简单判定全屏
            bool isFull = rect.Left == 0 && rect.Top == 0
                        && rect.Right == screenW
                        && rect.Bottom == screenH;
            return isFull;
        }
    }
}
