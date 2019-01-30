using L.Check.Devs;
using System;
using System.Linq;
using System.Text;
using System.Timers;
using YL.Check.Model;
using YL.Check.Utilities;

namespace YL.Check.Devs
{
    public class YLIdDevIC : IDisposable
    {

        public static readonly YLIdDevIC Instance = new YLIdDevIC();
        /// <summary>
        /// 身份证委托返回数据
        /// </summary>
        public event CvrReceived OnCvrReceived;
        /// <summary>
        /// IC卡委托返回数据
        /// </summary>
        public event CvrIcReceived OnCvrIcReceived;
        /// <summary>
        /// 当前设备是否在线
        /// </summary>
        public bool? IsOnline = null;
        /// <summary>
        /// 外部工具类实例
        /// </summary>
        LogicHandle logicHandle = new LogicHandle();
        /// <summary>
        /// 当前实例句柄
        /// </summary>
        DLLHelper.HANDLE hand;

        public static int Status;

        private YLIdDevIC()
        {
        }

        /// <summary>
        /// USBHID协议驱动打开
        /// </summary>
        /// <returns></returns>
        public int USBHIDOpen()
        {

            string vid = "1A86"; string pid = "E010";
            try
            {
                ushort VID = Convert.ToUInt16(vid, 16); ushort PID = Convert.ToUInt16(pid, 16);
                hand = DLLHelper.CH9326OpenDevices(VID, PID);
                if (hand.unused > 0)
                {
                    DLLHelper.CH9326CloseDevice(hand);
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex.Message);
                return 0;
            }

        }

        /// <summary>
        /// 初始化身份证IC卡设备
        /// </summary>
        public bool Init()
        {
            int usb = USBHIDOpen();
            if (usb > 0)
            {
                Status = 1;
                IsOnline = true;
                return true;
            }
            else
            {
                Status = 0;
                return false;
            }
        }


        /// <summary>
        /// 打开设备读取数据
        /// </summary>
        public void Start()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                string vid = "1A86";
                string pid = "E010";
                string ver = "3400";
                ushort VID = Convert.ToUInt16(vid, 16);
                ushort PID = Convert.ToUInt16(pid, 16);
                ushort VER = Convert.ToUInt16(ver, 16);
                hand = DLLHelper.CH9326OpenDevices(VID, PID);
                while (true)
                {
                    try
                    {
                        //设置身份
                        bool CodeBool = true;
                        System.Threading.Thread.Sleep(300);
                        int SetUpAntenna = DLLHelper.SetUpAntenna();//设置天线
                        if (SetUpAntenna == 1)
                        {
                            int Colese = DLLHelper.OPenAntenna();
                            int SetUpAgreement = DLLHelper.SetUpAgreement();
                            if (SetUpAgreement == 1)
                            {
                                int FindCard = DLLHelper.FindCard();//找卡
                                if (FindCard == 1)
                                {
                                    int SelectCard = DLLHelper.SelectCard();//选卡
                                    if (SelectCard == 1)
                                    {
                                        CodeBool = false;
                                        FillData();
                                        Console.Beep(2500, 300);
                                    }
                                }
                            }
                            if (CodeBool)
                            {
                                int IC = DLLHelper.SetUpICAgreement();// 设置ic协议
                                if (IC == 1)
                                {

                                    byte[] uid = new byte[8];
                                    int SetICUID = DLLHelper.SetICUID(uid);// 查询出IC卡上的信息
                                    if (SetICUID == 1)
                                    {
                                        OnCvrIcReceived(BitConverter.ToString(uid).Replace("-", ""));
                                        Console.Beep(2500, 300);
                                    }
                                }
                            }
                            DLLHelper.CH9326ClearThreadData(hand);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLog("读卡出错:" + ex.Message);
                    }

                } //while循环
            });
        }
        /// <summary>
        /// 身份证数据填充
        /// </summary>
        private void FillData()
        {
            IdCardInfoBM bm = new IdCardInfoBM();
            byte[] pucBaseMsg = new byte[256];
            byte[] pucPhoto = new byte[1024];
            byte[] find = new byte[1024];
            try
            {
                byte[] name = pucBaseMsg.Skip(0).Take(30).ToArray();
                byte[] sex = pucBaseMsg.Skip(30).Take(2).ToArray();
                byte[] minzu = pucBaseMsg.Skip(32).Take(4).ToArray();
                byte[] brath = pucBaseMsg.Skip(36).Take(16).ToArray();
                byte[] address = pucBaseMsg.Skip(52).Take(70).ToArray();
                byte[] num = pucBaseMsg.Skip(122).Take(36).ToArray();
                byte[] security = pucBaseMsg.Skip(158).Take(30).ToArray();
                byte[] startdate = pucBaseMsg.Skip(188).Take(16).ToArray();
                byte[] enddate = pucBaseMsg.Skip(204).Take(16).ToArray();
                byte[] newaddress = pucBaseMsg.Skip(230).Take(70).ToArray();
                string str = "";

                #region 性别

                str = Encoding.Unicode.GetString(sex);
                bm.sex = "未说明";
                if (str == "1" || str == "0" || str == "2")
                {
                    bm.sex = str.Replace("1", "男").Replace("2", "女").Replace("0", "未知");
                }
                #endregion 性别

                #region 民族

                str = Encoding.Unicode.GetString(minzu);
                bm.nation = "未知";
                string[] nation = "汉,蒙古,回,藏,维吾尔,苗,彝,壮,布依,朝鲜,满,侗,瑶,白,土家,哈尼,哈萨克,傣,黎,傈傈,佤,畲,高山,拉祜,水,东乡,纳西,景颇,柯尔克孜,土,达翰尔,仫佬族,羌,布朗,撒拉,毛南,仡佬,锡伯,阿昌,普米,塔吉克,怒,乌孜别克,俄罗斯,鄂温克,德昂,保安,裕固,京,塔塔尔,独龙,鄂伦春,赫哲,门巴,珞巴,基洛".Split(',');
                int IntPa = int.Parse(str) - 1;
                if (IntPa < nation.Length) bm.nation = nation[IntPa];

                #endregion 民族

                bm.name = Encoding.Unicode.GetString(name);
                bm.dateofbirth = Encoding.Unicode.GetString(brath);
                bm.address = Encoding.Unicode.GetString(address);
                bm.idcode = Encoding.Unicode.GetString(num);
                bm.department = Encoding.Unicode.GetString(security);
                bm.startdate = Encoding.Unicode.GetString(startdate);
                bm.enddate = Encoding.Unicode.GetString(enddate);
                if (this.OnCvrReceived != null)
                {
                    OnCvrReceived(bm);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("身份证实体赋值出错:" + ex.Message);
            }
        }

        /// <summary>
        /// 结束资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                DLLHelper.CH9326CloseDevice(hand);
            }
            catch (Exception ex)
            {
                Logger.WriteLog("结束身份证读卡器句柄错误：" + ex.Message);
            }
        }


        public delegate void CvrIcReceived(string num); // 供外部调用的Ic卡
        public delegate void CvrReceived(IdCardInfoBM bm); //供外部进行调用bm实体类

    }

}

