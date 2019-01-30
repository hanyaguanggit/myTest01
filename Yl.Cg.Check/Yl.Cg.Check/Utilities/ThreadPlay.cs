using System;
//using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Common.API;

namespace YL.Check.Utilities
{
    /// <summary>
    /// 播放语音类。
    /// </summary>
    class ThreadPlay
    {
        /// <summary>
        /// 语音文件名。
        /// </summary>
        public struct Rings
        {
            public static string ScanSpace = "";
            public static string ScanFailed = @"wav\alarm.wav";           // 非法票
            public static string WrongVenue = @"wav\wrong.wav";           // 非本展厅票
            public static string Pass = @"wav\pass.wav";                  // 请通过
            public static string InvalidDate = @"wav\invalid.wav";        // 过期票
            public static string TeamPass = @"wav\team.wav";              // 团体票
            public static string StudentPass = @"wav\team.wav";           // 学免票

            public static string MorePass = @"wav\DECODE.wav";              // 超过检票次数
        }

        private string _ringFile = "";                      // 语音文件路径名
 
        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="targetDir">目标目录</param>
        /// <param name="ring">文件路径</param>
        public ThreadPlay(string targetDir, string ring)
        {
            _ringFile = targetDir + ring;
        }
       
        /// <summary>
        /// 播放。
        /// </summary>
        public void Play()
        {
            if (File.Exists(_ringFile))
                WinAPI.PlaySound(_ringFile);
        }
    }
}
