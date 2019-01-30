using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace YL.Check.Utilities
{
    public class Logger
    {
        /// <summary>
        /// 写日志方法实现
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteLog(string msg)
        {
            StreamWriter writer = null;
            try
            {
                var year = DateTime.Now.Year.ToString();
                var month = DateTime.Now.Month.ToString();
                const string savepath = "Log";
                var filename = DateTime.Now.Day + ".txt";
                var path = Path.Combine(Environment.CurrentDirectory, savepath, year, month);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var file = new FileInfo(path + "/" + filename);
                writer = new StreamWriter(file.FullName, true);
                var str = msg + "{" + "时间:" + DateTime.Now + "}\r\n";
                writer.Write(str);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }
    }
}
