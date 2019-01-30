using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using YL.Check.Model;
using YL.Check.Devs;
using Yl.Cg.Model.Bm;
using Yl.Ticket5.Redis.Cache;
using System.Data;
using System.Windows.Forms;
using System.Collections;

namespace YL.Check.Utilities
{
    internal class Config
    {
        /// <summary>
        /// 当前登录用户
        /// </summary>
        public AdminUserBM LoginUser { get; set; }

        /// <summary>
        /// 接口路径。
        /// </summary>
        public string InterfacePath { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceCode { get; set; }

        /// <summary>
        /// 主窗体
        /// </summary>
        public MainForm MainForm { get; set; }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// 开闸确认
        /// </summary>
        public bool inflag = true;
        /// <summary>
        /// 每日记录查看的动态标签
        /// </summary>
        public int DailyCheckNum { get; set; }

        /// <summary>
        /// 串口初始化
        /// </summary>
        public bool SerialPortInit { get; set; }

        /// <summary>
        /// 二维码串口名称 
        /// </summary>
        public string SerialPort { get; set; }

        /// <summary>
        /// 闸机串口名称
        /// </summary>
        public string GateSerialPort { get; set; }

        /// <summary>
        /// 闸机串口
        /// </summary>
        public SerialPort GateSerial { get; set; }

        /// <summary>
        /// 闸机初始化
        /// </summary>
        public bool GateInit { get; set; }

        /// <summary>
        /// 上次剩余进入人数
        /// </summary>
        public int LastCount { get; set; }

        /// <summary>
        /// 当前剩余进入人数
        /// </summary>
        public int CurrentCount { get; set; }

        /// <summary>
        /// true:说明正在接受数据 false 可对串口进行操作
        /// </summary>
        public bool GateListening { get; set; }


        /// <summary>
        /// 闸机指令列表
        /// </summary>
        public List<string> GateSerialList { get; set; }

        /// <summary>
        /// 出馆人数上传
        /// </summary>
        public static int CheckoutPersonCount { get; set; }


        /// <summary>
        /// 当前闸机可离线检的门票
        /// </summary>
        public static string TicketArr { get; set; }

        /// <summary>
        /// 功能说明：计数开闸命令
        /// 创建人：ys
        /// 创建日期：2016-05-05 10：06
        /// </summary>
        public string OpenCountCMD { get; set; }



        /// <summary>
        /// 自动更新启动程序命令行参数
        /// </summary>
        public string[] ProgramArguments { get; set; }

        /// <summary>
        /// 服务接口URL地址
        /// </summary>
        public string ServiceURL { get; set; }

        /// <summary>
        /// 客户端更新文件路径URL
        /// </summary>
        public string ClientUpdateURL { get; set; }

        /// <summary>
        /// 身份证验票接口
        /// </summary>
        public string IdCheckInterface { get; set; }

        /// <summary>
        /// 二维码验票接口
        /// </summary>
        public string QRCheckInterface { get; set; }

        /// <summary>
        /// 心跳接口
        /// </summary>
        public string BreathInterface { get; set; }

        /// <summary>
        /// 可以检票的场馆
        /// </summary>
        public string checkticketlist { get; set; }

        /// <summary>
        /// 可以验票的场馆
        /// </summary>
        public string inspectticketlist { get; set; }

        /// <summary>
        /// 检票成功提示音文件地址
        /// </summary>
        public string successTip { get; set; }

        /// <summary>
        /// 检票失败提示音文件地址
        /// </summary>
        public string failTip { get; set; }

        /// <summary>
        /// 倒计时
        /// </summary>
        public int CountdownNum { get; set; }

        public Queue<YLGateDev.PassDerEnum> meQueuePassGate { get; set; }
        /// <summary>
        /// Instance
        /// </summary>
        public static readonly Config Instance = new Config();

        /// <summary>
        /// 当前在线或者离线
        /// </summary>
        public static bool is_Online = false;

        /// <summary>
        /// 每天离线检票数据上传次数
        /// </summary>
        public int UploadCount { get; set; }

        /// <summary>
        /// 当前离线检票数据已上传次数
        /// </summary>
        public int CurrentUploadCount { get; set; }

        /// <summary>
        /// 每天离线检票数据上传时间
        /// </summary>
        public int UploadTime { get; set; }

        /// <summary>
        /// 免费入馆人年龄
        /// </summary>
        public int FreeAge { get; set; }

        /// <summary>
        /// 免费入馆人群对应的权限卡二维码信息
        /// </summary>
        /// 
        public string FreeQRCode { get; set; }
        /// <summary>
        /// 做开闸指令
        /// </summary>
        public string OpenLeftCMD { get; set; }
        /// <summary>
        /// 左常开
        /// </summary>
        public string NormallyOpenLeftCMD { get; set; }
        /// <summary>
        /// 左关闭
        /// </summary>
        public string CloseLeftCMD { get; set; }
        /// <summary>
        /// 左通过
        /// </summary>
        public string PassLeftCMD { get; set; }
        /// <summary>
        /// 左超时
        /// </summary>
        public string TimeOutLeftCMD { get; set; }
        /// <summary>
        /// 防跟随
        /// </summary>
        public string FollowCMD { get; set; }
        /// <summary>
        /// 读机号
        /// </summary>
        public string ReadMachineNoCMD { get; set; }
        /// <summary>
        /// 出闸指令
        /// </summary>
        public string OutCountCMD { get; set; }

        public int openCount = 0;
        /// <summary>
        /// 开闸队列
        /// </summary>
        public Queue Qu { set; get; } = new Queue();
        /// <summary>
        /// 构造函数
        /// </summary>
        private Config()
        {
            Init();
        }

        /// <summary>
        /// 初始化方法
        /// </summary>
        private void Init()
        {
            GateSerialList = new List<string>();
            SerialPortInit = false; //串口初始化
            GateInit = false;//闸机初始化
            GateListening = false;

            redis_dbnum = Convert.ToInt32(ConfigurationManager.AppSettings["REDIS_DBNUM"]);//Redis缓存库编号

            //OpenLeftCMD = ConfigurationManager.AppSettings["OpenLeftCMD"];//左打开
            //NormallyOpenLeftCMD = ConfigurationManager.AppSettings["NormallyOpenLeftCMD"];//左常开
            //CloseLeftCMD = ConfigurationManager.AppSettings["CloseLeftCMD"];//左关闭
            //PassLeftCMD = ConfigurationManager.AppSettings["PassLeftCMD"];//左通过
            //TimeOutLeftCMD = ConfigurationManager.AppSettings["TimeOutLeftCMD"];//左超时
            //OpenRightCMD = ConfigurationManager.AppSettings["OpenRightCMD"];//右打开
            //NormallyOpenRightCMD = ConfigurationManager.AppSettings["NormallyOpenRightCMD"];//右常开
            //CloseRightCMD = ConfigurationManager.AppSettings["CloseRightCMD"];//右关闭
            //PassRightCMD = ConfigurationManager.AppSettings["PassRightCMD"];//右通过
            //TimeOutRightCMD = ConfigurationManager.AppSettings["TimeOutRightCMD"];//右超时
            //FollowCMD = ConfigurationManager.AppSettings["FollowCMD"];//防尾随

            //ReadMachineNoCMD = ConfigurationManager.AppSettings["ReadMachineNoCMD"];//读机号
            OpenCountCMD = ConfigurationManager.AppSettings["OpenCountCMD"];//开闸指令

            OutCountCMD = ConfigurationManager.AppSettings["OutCountCMD"];//出闸指令次数统计
            SerialPort = ConfigurationManager.AppSettings["SerialPort"]; //二维码串口名称
            GateSerialPort = ConfigurationManager.AppSettings["GateSerialPort"]; //闸机串口名称 

            IdCheckInterface = ConfigurationManager.AppSettings["IdCheckInterface"];//身份证验票接口
            QRCheckInterface = ConfigurationManager.AppSettings["QRCheckInterface"];//QRCheckInterface
            BreathInterface = ConfigurationManager.AppSettings["BreathInterface"];//心跳接口  

            ServiceURL = ConfigurationManager.AppSettings["InterfacePath"];
            ClientUpdateURL = ConfigurationManager.AppSettings["ClientUpdateURL"];

            DeviceId = Convert.ToInt32(ConfigurationManager.AppSettings["DeviceId"]);
            DeviceCode = ConfigurationManager.AppSettings["DeviceCode"];
            checkticketlist = ConfigurationManager.AppSettings["checkticketlist"];
            inspectticketlist = ConfigurationManager.AppSettings["inspectticketlist"];
            CountdownNum = 10; //倒计时 

            var Config_InterfacePath = ConfigurationManager.AppSettings["InterfacePath"];

            if (Config_InterfacePath != null)
            {
                InterfacePath = Config_InterfacePath.ToString();
            }
            UploadCount = 6;
            UploadTime = 15;
            try
            {
                string dbPath = string.Empty;
                string ConfigPath = Environment.CurrentDirectory + "\\config.xml";
                DataSet ds = new DataSet();
                ds.ReadXml(ConfigPath);
                var table = ds.Tables[0];
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    var row = table.Rows[i];
                    var key = row["key"].ToString();
                    var value = row["value"].ToString();
                    if (key == "UPLOAD_COUNT")
                    {
                        UploadCount = Convert.ToInt32(value);
                    }
                    if (key == "UPLOAD_TIME")
                    {
                        UploadTime = Convert.ToInt32(value);
                    }
                    if (key == "FreeAge")
                    {
                        FreeAge = Convert.ToInt32(value);
                    }
                    if (key == "FreeQRCode")
                    {
                        FreeQRCode = value;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            try
            {
                successTip = Application.StartupPath + "\\wav\\success.wav";
                failTip = Application.StartupPath + "\\wav\\alarm.wav";
            }
            catch (Exception ex) { }


            LoginUser = new AdminUserBM();
            LoginUser.Id = Convert.ToInt64(ConfigurationManager.AppSettings["UserId"]);//当前登录用户ID;
            meQueuePassGate = new Queue<YLGateDev.PassDerEnum>();//存储过闸信号 
        }

        private int redis_dbnum;

        private RedisCache _cache;

        /// <summary>
        /// Redis 帮助类
        /// </summary>
        public RedisCache cache
        {
            get
            {
                if (this._cache == null)
                {
                    string conn = System.Configuration.ConfigurationManager.AppSettings["REDIS_CONN"];
                    this._cache = new RedisCache(new string[] { conn }, redis_dbnum);
                }

                return this._cache;
            }
        }

        private RedisQueue _queue;

        public RedisQueue queue
        {
            get
            {
                if (this._queue == null)
                {
                    string conn = System.Configuration.ConfigurationManager.AppSettings["REDIS_CONN"];
                    this._queue = new RedisQueue(new string[] { conn }, redis_dbnum);
                }

                return this._queue;
            }
        }
    }
}