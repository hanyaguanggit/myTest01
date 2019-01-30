using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Common.API
{
    /// <summary>
    /// 常用Windows API类
    /// </summary>
    class WinAPI
    {
        #region DLL接口导出
        /// <summary>
        /// 窗口属性
        /// </summary>
        private enum SHOWWINDOW : uint
        {
            SW_HIDE = 0,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            /// <summary>
            /// 最大化
            /// </summary>
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            /// <summary>
            /// 显示
            /// </summary>
            SW_SHOW = 5,
            /// <summary>
            /// 最小化
            /// </summary>
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            /// <summary>
            /// 恢复默认
            /// </summary>
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11,
        }

        /// <summary>
        /// 语音文件播放标志
        /// </summary>
        private enum PlaySoundFlags : int
        {
            SND_SYNC = 0x0,     // play synchronously (default)            
            SND_ASYNC = 0x1,    // play asynchronously            
            SND_NODEFAULT = 0x2,    // silence (!default) if sound not found            
            SND_MEMORY = 0x4,       // pszSound points to a memory file            
            SND_LOOP = 0x8,     // loop the sound until next sndPlaySound            
            SND_NOSTOP = 0x10,      // don't stop any currently playing sound            
            SND_NOWAIT = 0x2000,    // don't wait if the driver is busy            
            SND_ALIAS = 0x10000,    // name is a registry alias            
            SND_ALIAS_ID = 0x110000,// alias is a predefined ID            
            SND_FILENAME = 0x20000, // name is file name            
            SND_RESOURCE = 0x40004 // name is resource name or atom        
        }

        /// <summary>
        /// 系统时间
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;  //12-8; 这个函数使用的是0时区的时间
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        [DllImport("coredll.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetLocalTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("coredll.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int CreateMutex(IntPtr lpMutexAttributes, bool bInitiaOwner, string lpName);

        [DllImport("coredll.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetLastError();

        [DllImport("coredll.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("coredll.dll")]
        private static extern IntPtr FindWindow(string className, string WindowsName);

        [DllImport("coredll.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, uint nCmdShow);

        [DllImport("coredll.dll")]
        private static extern int PlaySound(string szSound, IntPtr hModule, int flags);

        [DllImport("winmm.dll", EntryPoint = "PlaySound")]
        private static extern int PlaySoundA(string lpszName, IntPtr hModule, int dwFlags);

        [DllImport("Kernel32.dll", EntryPoint = "SetLocalTime")]
        private static extern bool SetLocalTimeA(ref SYSTEMTIME lpSystemTime);

        [DllImport("Kernel32.dll", EntryPoint = "CreateMutex", SetLastError = true)]
        private static extern int CreateMutexA(int lpSecurityAttributes, bool bInitialOwner, string lpName);

        [DllImport("Kernel32.dll", EntryPoint = "GetLastError", SetLastError = true)]
        private static extern int GetLastErrorA();

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        private static extern bool SetForegroundWindowA(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindowA(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        private static extern bool ShowWindowA(IntPtr hwnd, uint nCmdShow);

        private const int ERROR_ALREADY_EXISTS = 183;//十六进制/0xb7,(183/十进制)
        #endregion

        /// <summary>
        /// 窗体实例是否运行。
        /// </summary>
        /// <param name="hwnd">窗体句柄</param>
        /// <param name="title">窗体名称</param>
        /// <returns></returns>
        public static bool IsRunning(out IntPtr hwnd, string title)
        {
            hwnd = IntPtr.Zero;
            if (System.Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                hwnd = FindWindow(null, title);
                SetForegroundWindow(hwnd);
                CreateMutex(IntPtr.Zero, true, "App");
                return (GetLastError() == ERROR_ALREADY_EXISTS);
            }
            else
            {
                hwnd = FindWindowA(null, title);
                SetForegroundWindowA(hwnd);
                CreateMutexA(0, true, "App");
                return (GetLastErrorA() == ERROR_ALREADY_EXISTS);
            }
        }

        /// <summary>
        /// 显示窗体。
        /// </summary>
        /// <param name="title">窗体名称</param>
        public static void ShowWindow(string title)
        {
            uint nShow = (uint)SHOWWINDOW.SW_SHOWDEFAULT;
            if (System.Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                IntPtr hwnd = FindWindow(null, title);
                SetForegroundWindow(hwnd);
                System.Threading.Thread.Sleep(100);
                ShowWindow(hwnd, nShow);
            }
            else
            {
                IntPtr hwnd = FindWindowA(null, title);
                SetForegroundWindowA(hwnd);
                System.Threading.Thread.Sleep(100);
                ShowWindowA(hwnd, nShow);
            }
        }

        /// <summary>
        /// 显示窗体。
        /// </summary>
        /// <param name="hwnd">窗体句柄</param>
        public static void ShowWindow(IntPtr hwnd)
        {
            uint nShow = (uint)SHOWWINDOW.SW_SHOWDEFAULT;
            if (System.Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                SetForegroundWindow(hwnd);
                System.Threading.Thread.Sleep(100);
                ShowWindow(hwnd, nShow);
            }
            else
            {
                SetForegroundWindowA(hwnd);
                System.Threading.Thread.Sleep(100);
                ShowWindowA(hwnd, nShow);
            }
        }

        /// <summary>
        /// 创建快捷方式。
        /// </summary>
        /// <param name="exePath">应用程序路径名</param>
        /// <param name="shortCutPath">快捷方式路径名</param>
        public static void CreateShortcut(string exePath, string shortCutPath)
        {
            try
            {
                if (File.Exists(shortCutPath))
                    File.Delete(shortCutPath);

                StreamWriter objWriter = File.CreateText(shortCutPath);
                objWriter.WriteLine(string.Format("37#\"{0}\"", exePath));
                objWriter.Close();
            }catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// 播放音频文件。
        /// </summary>
        /// <param name="fileName">音频文件路径名</param>
        public static void PlaySound(string fileName)
        {
            try
            {
                if (System.Environment.OSVersion.Platform == PlatformID.WinCE)
                {
                    PlaySound(fileName, IntPtr.Zero, (int)(PlaySoundFlags.SND_FILENAME | PlaySoundFlags.SND_SYNC));
                }
                else
                {
                    PlaySoundA(fileName, IntPtr.Zero, (int)(PlaySoundFlags.SND_FILENAME | PlaySoundFlags.SND_SYNC));
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 同步系统时间。
        /// </summary>
        /// <param name="dtime">时间</param>
        public static void SyncTime(string dtime)
        {
            System.DateTime time;
            try
            {
                if (dtime.Length == 0)
                    return;

                time = System.Convert.ToDateTime(dtime);
            }
            catch
            {
                return;
            }
            SYSTEMTIME systemtime = new SYSTEMTIME();
            systemtime.wYear = System.Convert.ToUInt16(time.Year);
            systemtime.wMonth = System.Convert.ToUInt16(time.Month);
            systemtime.wDay = System.Convert.ToUInt16(time.Day);
            systemtime.wDayOfWeek = System.Convert.ToUInt16(time.DayOfWeek);
            systemtime.wHour = System.Convert.ToUInt16(time.Hour);
            systemtime.wMinute = System.Convert.ToUInt16(time.Minute);
            systemtime.wSecond = System.Convert.ToUInt16(time.Second);
            systemtime.wMilliseconds = System.Convert.ToUInt16(time.Millisecond);
            if (System.Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                SetLocalTime(ref systemtime);
            }
            else
            {
                SetLocalTimeA(ref systemtime);
            }
        }
    }
}
