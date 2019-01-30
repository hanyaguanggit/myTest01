using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using YL.Check.Model;

namespace YL.Check.Devs
{
    /// <summary>
    /// 功能说明：身份证读卡器操作类
    /// 创建人：ys
    /// 创建日期：2016-04-25 13:45
    /// </summary>
    public class YLIdDev : IDisposable
    {
        public static readonly YLIdDev Instance = new YLIdDev();

        public event CvrReceived OnCvrReceived;

        public bool? IsOnline = null;
        public int ManuId;

        #region extern

        [DllImport("termb.dll", EntryPoint = "CVR_InitComm", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int CVR_InitComm(int Port);//声明外部的标准动态库, 跟Win32API是一样的

        [DllImport("termb.dll", EntryPoint = "CVR_Authenticate", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int CVR_Authenticate();

        [DllImport("termb.dll", EntryPoint = "CVR_Read_Content", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int CVR_Read_Content(int Active);

        [DllImport("termb.dll", EntryPoint = "CVR_CloseComm", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int CVR_CloseComm();

        [DllImport("termb.dll", EntryPoint = "GetPeopleName", CharSet = CharSet.Ansi, SetLastError = false)]
        public static extern unsafe int GetPeopleName(ref byte strTmp, ref int strLen);

        [DllImport("termb.dll", EntryPoint = "GetPeopleNation", CharSet = CharSet.Ansi, SetLastError = false)]
        public static extern int GetPeopleNation(ref byte strTmp, ref int strLen);

        [DllImport("termb.dll", EntryPoint = "GetPeopleBirthday", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetPeopleBirthday(ref byte strTmp, ref int strLen);

        [DllImport("termb.dll", EntryPoint = "GetPeopleAddress", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetPeopleAddress(ref byte strTmp, ref int strLen);

        [DllImport("termb.dll", EntryPoint = "GetPeopleIDCode", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetPeopleIDCode(ref byte strTmp, ref int strLen);

        [DllImport("termb.dll", EntryPoint = "GetDepartment", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetDepartment(ref byte strTmp, ref int strLen);

        [DllImport("termb.dll", EntryPoint = "GetStartDate", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetStartDate(ref byte strTmp, ref int strLen);

        [DllImport("termb.dll", EntryPoint = "GetEndDate", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetEndDate(ref byte strTmp, ref int strLen);

        [DllImport("termb.dll", EntryPoint = "GetPeopleSex", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetPeopleSex(ref byte strTmp, ref int strLen);

        [DllImport("termb.dll", EntryPoint = "GetManuID", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetManuID(out int Port);//声明外部的标准动态库, 跟Win32API是一样的

        #endregion extern

        private YLIdDev()
        {
        }

        public void Init()
        {
            IsOnline = false;
            int iRetUSB = 0;
            try
            {
                int iPort;
                for (iPort = 1001; iPort <= 1016; iPort++)
                {
                    iRetUSB = CVR_InitComm(iPort);
                    if (iRetUSB == 1)
                    {
                        IsOnline = true;
                        break;
                    }
                }
                if (IsOnline.Value)
                {
                    GetManuID(out this.ManuId).ToString();
                }
                else
                {
                }
            }
            catch(Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                string mmm = ex.Message;
            }
        }

        public void Start()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        if (this.IsOnline.Value)
                        {
                            int authenticate = CVR_Authenticate();
                            if (authenticate == 1 || authenticate == 2)
                            {
                                int readContent = CVR_Read_Content(4);

                                if (readContent == 1)
                                {
                                    
                                    //this.label10.Text = "读卡操作成功！";
                                    FillData();
                                    Console.Beep(2500, 300);
                                }
                                else if (readContent == 2)
                                {
                                    //this.label10.Text = "读卡操作成功2！";
                                    FillData();
                                    Console.Beep(2500, 300);
                                }
                                else
                                {
                                    //this.label10.Text = string.Format("[{0}]读卡操作失败！", i);
                                }
                            }
                            System.Threading.Thread.Sleep(100);
                        }
                        else
                        {
                            //MessageBox.Show("初始化失败！");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
            });
        }

        private void FillData()
        {
            try
            {
                //string imgPath = Application.StartupPath + "\\zp.bmp";
                byte[] bsName = new byte[30];
                int length = 30;
                GetPeopleName(ref bsName[0], ref length);
                //MessageBox.Show();
                byte[] bsNumber = new byte[30];
                length = 36;
                GetPeopleIDCode(ref bsNumber[0], ref length);
                byte[] bsPeople = new byte[30];
                length = 3;
                GetPeopleNation(ref bsPeople[0], ref length);
                byte[] bsValidtermOfStart = new byte[30];
                length = 16;
                GetStartDate(ref bsValidtermOfStart[0], ref length);
                byte[] bsBirthday = new byte[30];
                length = 16;
                GetPeopleBirthday(ref bsBirthday[0], ref length);
                byte[] bsAddress = new byte[30];
                length = 70;
                GetPeopleAddress(ref bsAddress[0], ref length);
                byte[] bsValidtermOfEnd = new byte[30];
                length = 16;
                GetEndDate(ref bsValidtermOfEnd[0], ref length);
                byte[] bsSigndate = new byte[30];
                length = 30;
                GetDepartment(ref bsSigndate[0], ref length);
                byte[] bsSex = new byte[30];
                length = 3;
                GetPeopleSex(ref bsSex[0], ref length);

                //IdCardInfoBM bm = new IdCardInfoBM
                //{
                //    address = System.Text.Encoding.GetEncoding("GB2312").GetString(bsAddress),
                //    sex = System.Text.Encoding.GetEncoding("GB2312").GetString(bsSex),
                //    birthday = System.Text.Encoding.GetEncoding("GB2312").GetString(bsBirthday),
                //    signdate = System.Text.Encoding.GetEncoding("GB2312").GetString(bsSigndate),
                //    number = System.Text.Encoding.GetEncoding("GB2312").GetString(bsNumber).Replace("\0", string.Empty).Trim(),
                //    name = System.Text.Encoding.GetEncoding("GB2312").GetString(bsName).Replace("\0", string.Empty).Trim(),
                //    people = System.Text.Encoding.GetEncoding("GB2312").GetString(bsPeople),
                //    validtermOfStart = System.Text.Encoding.GetEncoding("GB2312").GetString(bsValidtermOfStart),
                //    validtermOfEnd = System.Text.Encoding.GetEncoding("GB2312").GetString(bsValidtermOfEnd)
                //};

                //if (this.OnCvrReceived != null)
                //{
                //    this.OnCvrReceived(bm);
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void Dispose()
        {
            try
            {
                CVR_CloseComm();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }

    public delegate void CvrReceived(IdCardInfoBM bm);
}
