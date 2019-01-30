using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using YL.Check.Utilities;
using System.Management;
using System.IO;

namespace YL.Check
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(params string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Config.Instance.ProgramArguments = args;

            CheckVersionUpdate();
            Application.Run(new MainForm());
        }

        /// <summary>
        /// 功能说明：系统错误截获，并根据具体情况进行相应处理 
        /// </summary>
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            try
            {
                Application.Restart();
            }
            catch (Exception ex)
            {

            }
        }

        static bool CheckVersionUpdate()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            //lblInfo.Invoke(new MethodInvoker(() => SetInfo(string.Format("当前系统版本：{0}.{1}.{2}({3})", version.Major, version.Minor, version.Build, version.Revision))));

            #region 版本验证

            //if (Config.Instance.ProgramArguments == null || !Config.Instance.ProgramArguments.Contains("nocheckversion"))
           // {
            //    Rung.AppUpdate.AutoUpdateHelper helper = new Rung.AppUpdate.AutoUpdateHelper(
            //        Config.Instance.ServiceURL, Config.Instance.ClientUpdateURL,
            //        (msg) =>
            //        {
            //            //this.lblInfo.Invoke(new MethodInvoker(() => { lblInfo.Text = msg; lblInfo.Refresh(); }));
            //        },
            //        (max) =>
            //        {
            //            //this.pb.Invoke(new MethodInvoker(() => { pb.Visible = true; pb.Maximum = max; }));
            //        },
            //        (value) =>
            //        {
            //            //this.pb.Invoke(new MethodInvoker(() => { pb.Value = value; }));
            //        },
            //        //update 彭莉莉 2014-08-26 10:04:01
            //        //说明当enable不等于true的时候，不能直接btnLogin为禁用，因为这样无法离线登录
            //        (enable, localDateTime) =>
            //        {
            //            //this.Invoke(new MethodInvoker(() =>
            //            //{
            //            //}));
            //        });

            //    return helper.CheckVersion(version.Revision, 6);
            //}
            //else
                return false;

            #endregion 版本验证
        }

        private static string Detect32or64()
        {
            try
            {
                string addressWidth = String.Empty;
                ConnectionOptions mConnOption = new ConnectionOptions();
                ManagementScope mMs = new ManagementScope("\\\\localhost", mConnOption);
                ObjectQuery mQuery = new ObjectQuery("select AddressWidth from Win32_Processor");
                ManagementObjectSearcher mSearcher = new ManagementObjectSearcher(mMs, mQuery);
                ManagementObjectCollection mObjectCollection = mSearcher.Get();
                foreach (ManagementObject mObject in mObjectCollection)
                {
                    addressWidth = mObject["AddressWidth"].ToString();
                }
                return addressWidth;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return String.Empty;
            }
        }
    }
}
