using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using YL.Check.Utilities;
using YL.Check.Devs;
using System.Threading;

namespace YL.Check.Forms
{
    /// <summary>
    /// 初始化硬件加载
    /// </summary>
    /*
     * 创建人：姚睿
     * 创建时间：2014-05-21
     *
     * 修改人：贾增义
     * 描述：
     * 修改时间：2016年4月24日
     */

    public partial class SplashForm : BaseForm
    {
        #region 构造函数 & 属性 & 变量

        /// <summary>
        /// 构造函数
        /// </summary>
        public SplashForm()
        {
            InitializeComponent();
            Size = new Size(1024, 768);
        }

        /// <summary>
        /// 初始化身份证
        /// </summary>
        private bool isCheckingCvr = false;

        /// <summary>
        /// 初始化串口
        /// </summary>
        private bool isCheckingSerialPort;

        /// <summary>
        /// 初始化闸机
        /// </summary>
        private bool isCheckingGate;

        /// <summary>
        /// 二维码读取串口
        /// </summary>
        private static SerialPort _serialPort;

        /// <summary>
        /// 闸机串口
        /// </summary>
        private static SerialPort _gateSerialPort;


        #endregion 构造函数 & 属性 & 变量

        #region 窗体事件

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SplashForm_Load(object sender, EventArgs e)
        {
            try
            {
                lblInfo.Text = "";
                SetInfo(@"正在检查硬件设备...");
                Thread SplashTh = new Thread(new ThreadStart(CheckCvr));
                SplashTh.Start();
            }
            catch (Exception ex)
            {
                Logger.WriteLog("窗口加载时报错" + ex.Message);
            }
        }

        #endregion 窗体事件

        #region 业务处理

        /// <summary>
        /// 身份证阅读器的初始化
        /// </summary>
        private void CheckCvr()
        {
            Logger.WriteLog("当前线程ID" + System.Threading.Thread.CurrentThread.ManagedThreadId);

            try
            {
                while (!isCheckingCvr)
                {
                    if (isCheckingCvr == false)
                    {
                        lblInfo.Invoke(new MethodInvoker(() => SetInfo(@"正在检测身份证阅读器...")));
                        YLIdDevIC.Instance.Init();
                        if (YLIdDevIC.Instance.IsOnline != true)
                        {
                            lblInfo.Invoke(new MethodInvoker(() =>
                            {
                                SetInfo("失败", true);
                                SetInfo(@"身份证阅读器识别失败，3秒后重试！");
                            }));
                            System.Threading.Thread.Sleep(1000);
                            lblInfo.Invoke(new MethodInvoker(() => SetInfo("身份证阅读器识别失败，2秒后重试！")));
                            System.Threading.Thread.Sleep(1000);
                            lblInfo.Invoke(new MethodInvoker(() => SetInfo("身份证阅读器识别失败，1秒后重试！")));
                            System.Threading.Thread.Sleep(1000);
                        }
                        else
                        {
                            while (!IsHandleCreated) { Application.DoEvents(); }

                            Invoke(new MethodInvoker(() =>
                            {
                                Logger.WriteLog("当前验卡线程ID" + System.Threading.Thread.CurrentThread.ManagedThreadId);
                                YLIdDevIC.Instance.OnCvrIcReceived += num => new LogicHandle().ICCardCheck(num);
                                YLIdDevIC.Instance.OnCvrReceived += bm => new LogicHandle().TicketCheckByIdCard(bm);
                                YLIdDevIC.Instance.Start();
                                SetInfo("成功", true);
                            }));

                            isCheckingCvr = true;
                            SerialPortInit();
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                    }
                    else if (!isCheckingCvr)
                    {
                        System.Threading.Thread.Sleep(300);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("检测身份证阅读器报错" + ex.Message);
            }
        }


        /// <summary>
        /// 二维码串口的初始化
        /// </summary>
        private void SerialPortInit()
        {
            try
            {
                while (!Config.Instance.SerialPortInit)
                {
                    if (isCheckingSerialPort == false && Config.Instance.SerialPortInit != true)
                    {
                        isCheckingSerialPort = true;
                        lblInfo.Invoke(new MethodInvoker(() => SetInfo(@"正在检测二维码串口...")));
                        try
                        {
                            _serialPort = new SerialPort();
                            YLScannerDev.SerialPortInit(ref _serialPort);//初始化二维码串口      
                        }
                        catch (Exception)
                        {
                            Config.Instance.SerialPortInit = false;
                        }

                        isCheckingSerialPort = false;
                        if (Config.Instance.SerialPortInit != true)
                        {
                            lblInfo.Invoke(new MethodInvoker(() =>
                            {
                                SetInfo("失败", true);
                                SetInfo(@"二维码串口识别失败，3秒后重试！");
                            }));
                            System.Threading.Thread.Sleep(1000);
                            lblInfo.Invoke(new MethodInvoker(() => SetInfo("二维码串口识别失败，2秒后重试！")));
                            System.Threading.Thread.Sleep(1000);
                            lblInfo.Invoke(new MethodInvoker(() => SetInfo("二维码串口识别失败，1秒后重试！")));
                            System.Threading.Thread.Sleep(1000);
                        }
                        else
                        {
                            while (!IsHandleCreated) { Application.DoEvents(); }
                            Invoke(new MethodInvoker(() =>
                            {
                                _serialPort.DataReceived += new SerialDataReceivedEventHandler(new YLScannerDev()._serialPort_DataReceived);
                                SetInfo("成功", true);
                            }));
                            GateInit();
                        }
                    }
                    else if (isCheckingSerialPort)
                    {
                        System.Threading.Thread.Sleep(300);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("初始化二维码串口时报错" + ex.Message);
            }
        }

        /// <summary>
        /// 功能说明：闸机初始化
        /// </summary>
        private void GateInit()
        {
            try
            {
                while (!Config.Instance.GateInit)
                {
                    if (isCheckingGate == false && Config.Instance.GateInit != true)
                    {
                        isCheckingGate = true;
                        lblInfo.Invoke(new MethodInvoker(() => SetInfo(@"正在检测闸机...")));

                        try
                        {
                            _gateSerialPort = new SerialPort();
                            YLGateDev.SerialPortInit(_gateSerialPort);
                        }
                        catch (Exception)
                        {
                            Config.Instance.GateInit = false;
                        }

                        if (Config.Instance.GateInit != true)
                        {
                            isCheckingGate = false;
                            lblInfo.Invoke(new MethodInvoker(() =>
                            {
                                SetInfo("失败", true);
                                SetInfo(@"闸机初始化失败，3秒后重试！");
                            }));
                            System.Threading.Thread.Sleep(1000);
                            lblInfo.Invoke(new MethodInvoker(() => SetInfo("闸机初始化失败，2秒后重试！")));
                            System.Threading.Thread.Sleep(1000);
                            lblInfo.Invoke(new MethodInvoker(() => SetInfo("闸机初始化失败，1秒后重试！")));
                            System.Threading.Thread.Sleep(1000);
                        }
                        else
                        {
                            while (!IsHandleCreated) { Application.DoEvents(); }

                            Invoke(new MethodInvoker(() =>
                            {
                                Config.Instance.GateSerial.DataReceived += new SerialDataReceivedEventHandler(new YLGateDev()._gateSerialPort_DataReceived);
                                SetInfo("成功", true);
                                DialogResult = DialogResult.OK;
                                Close();
                            }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("初始化闸机串口时报错" + ex.Message);
            }
        }

        /// <summary>
        /// 界面信息的显示
        /// </summary>
        /// <param name="info">信息</param>
        /// <param name="isSuccess">是否验票成功</param>
        private void SetInfo(string info, bool isSuccess = false)
        {
            try
            {
                lblInfo.ForeColor = Color.White;
                if (isSuccess)
                {
                    lblInfo.Text += info;
                }
                else
                {
                    lblInfo.Text += info.IndexOf("...") >= 0 ? "\r\n" + info + "..........................." : "\r\n" + info;
                }
                pnlInfo.AutoScrollPosition = new Point(lblInfo.Width, lblInfo.Height);
            }
            catch (Exception ex)
            {
                Logger.WriteLog("信息设置报错" + ex.Message);
            }
        }

        #endregion 业务处理
    }
}