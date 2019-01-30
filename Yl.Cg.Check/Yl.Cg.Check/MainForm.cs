using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using YL.Check.Panels;
using YL.Check.Model;
using YL.Check.Utilities;
using YL.Check.Devs;
using YL.Check.Forms;
using Yl.Cg.Model.Args.Check;
using YL.Check.Properties;
using System.Threading;
using System.Data;
using Yl.Ticket5.Common40.Utilities;

namespace YL.Check
{
    /// <summary>
    /// 主窗体
    /// </summary>
    /*
     * 创建人：贾增义
     * 创建时间：2016-04-23
     *
     */

    public partial class MainForm : BaseForm
    {
        #region 构造函数&属性
        /// <summary>
        /// 倒计时用的计时器
        /// </summary>
        private readonly System.Timers.Timer tmr_Countdown;

        /// <summary>
        /// 呼吸用的计时器
        /// </summary>
        private readonly System.Timers.Timer tmr_Breath;

        /// <summary>
        /// 倒计时
        /// </summary>
        private int start = Config.Instance.CountdownNum;

        private readonly System.Timers.Timer tmr_count;

        //离线检票在在线状态时进行上传更新
        Thread thSync;
        private readonly System.Timers.Timer tmr_SyncOutLineTicket;

        Thread thDownload;

        Thread thDownloadTicket;

        LogicHandle logic = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainForm()
        {
            try
            {
               
                InitializeComponent();
                
                tmr_Breath = new System.Timers.Timer(10000);
                tmr_Breath.Elapsed += (tmr_Breath_Elapsed);
                tmr_Breath.Start();

                tmr_Countdown = new System.Timers.Timer(1000);
                tmr_Countdown.Elapsed += (tmr_Countdown_Eapsed);

                Config.Instance.MainForm = this;

                var result = new SplashForm().ShowDialog(this);
                SetFormType(GetUcZr(0), null,"");

                this.BackgroundImage = Resources.backgroud;
            

                logic = new LogicHandle();
                Show();
            }
            catch (Exception ex)
            {

            }
        }

        #endregion 构造函数

        #region 窗体事件

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            #region 时间同步

            try
            {
                if(logic == null)
                    logic = new LogicHandle();
                logic.GetTime(crTime =>
                {
                    if (crTime != null && crTime.Result > 0)
                    {
                        DateTime dtServerTime = Yl.Ticket5.Common40.Utilities.TextureHelper.IntToDateTime(crTime.Result);
                        var rst = LogicHandle.SetTime(dtServerTime);
                    }
                });
            }
            catch (Exception ex)
            {

            }

            #endregion
        }


        /// <summary>
        /// 窗体关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                System.Environment.Exit(0);
            }
            catch (Exception ex)
            {

            }
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            this.TopMost = true;
        }

        #endregion 窗体事件

        #region 业务处理

        /// <summary>
        /// 获取对应窗体
        /// </summary>
        /// <param name="type">窗体类型1:检票无效窗口 2:检票单人重复窗口 3:检票团队重复窗口 4:检票单人成功窗口 5:检票团队成功窗口 其他：默认窗口</param>
        /// <returns></returns>
        public BaseFormTypeUC GetUcZr(int type)
        {
            try
            {
                BaseFormTypeUC uc;
                switch (type)
                {
                    //检票无效窗口
                    case 1:
                        uc = new CheckFailUC();
                        ReStartTimer(); //新窗体展示后，启用倒计时信息
                        this.picState.Visible = true;
                        break;
                    //检票单人重复窗口
                    case 2:
                        uc = new CheckSingleRepeatUC();
                        ReStartTimer(); //新窗体展示后，启用倒计时信息
                        this.picState.Visible = true;
                        break;
                    //检票团队重复窗口
                    case 3:
                        uc = new CheckTeamRepeatUC();
                        ReStartTimer(); //新窗体展示后，启用倒计时信息
                        this.picState.Visible = true;
                        break;
                    case 4: //检票4D单人成功窗口
                        uc = new CheckSingleSuccessUC();
                        ReStartTimer(); //新窗体展示后，启用倒计时信息
                        this.picState.Visible = true;
                        break;
                    case 5: //检票团队成功窗口
                        uc = new CheckTeamSuccessUC();
                         ReStartTimer(); //新窗体展示后，启用倒计时信息
                        
                        this.picState.Visible = true;
                        break;
                    //检票机禁用窗口
                    case 6:
                        uc = new DisableUC();
                        ReStartTimer(); //新窗体展示后，启用倒计时信息
                        this.picState.Visible = true;
                        break;
                    //检票机免费进窗口
                    case 7:
                        uc = new WorkCardUC();
                        ReStartTimer(); //新窗体展示后，启用倒计时信息
                        this.picState.Visible = true;
                        break;
                    case 8:  //非本场票
                        uc = new CheckErrorSessionUC();
                        ReStartTimer(); //新窗体展示后，启用倒计时信息
                        this.picState.Visible = true;
                        break;
                    case 9:  //离线散客验票成功
                        uc = new OutLineSingleSuccessUC();
                        ReStartTimer(); //新窗体展示后，启用倒计时信息
                        this.picState.Visible = true;
                        break;
                    case 10:  //离线散客验票失败，已入馆
                        uc = new OutLineSingleRepeatUC();
                        ReStartTimer(); //新窗体展示后，启用倒计时信息
                        this.picState.Visible = true;
                        break;
                    case 11:  //离线团队验票成功
                        uc = new OutLineTeamSuccessUC();
                        ReStartTimer(); //新窗体展示后，启用倒计时信息
                        this.picState.Visible = true;
                        break;
                    case 12:  //离线团队验票失败，已入馆
                        uc = new OutLineTeamRepeatUC();
                        ReStartTimer(); //新窗体展示后，启用倒计时信息
                        this.picState.Visible = true;
                        break;
                    //默认统计窗体
                    default:
                        //判断闸机是否被禁用
                        uc = new DefaultUC();
                        this.picState.Visible = false;
                        break;
                }
                return uc;
            }
            catch (Exception ex)
            {
                Logger.WriteLog("获取对应窗体报错" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 设置窗体
        /// </summary>
        /// <param name="uc">窗体类型</param>
        /// <param name="arg">传递给窗体的信息</param>
        internal void SetFormType(BaseFormTypeUC uc, CheckResultBM arg,string tipMessage)
        {
            try
            {
                uc.SetTipInfo(tipMessage);
                uc.SetValue(arg);
                pnContainer.Controls.Clear();
                uc.Dock = DockStyle.Fill;
                pnContainer.Controls.Add(uc);
            }
            catch (Exception ex)
            {
                Logger.WriteLog("设置窗体类型报错" + ex.Message);
            }
        }

        /// <summary>
        /// 倒计时的计时器开始
        /// </summary>
        public void ReStartTimer()
        {
            try
            {
                tmr_Countdown.Stop();
                lblCheckInfo.Visible = true;
                lblCheckInfo.Text = Config.Instance.CountdownNum.ToString();
                start = Config.Instance.CountdownNum;
                tmr_Countdown.Start();
                lblCheckInfo.BackColor = Color.Red;
            }
            catch (Exception ex)
            {
                Logger.WriteLog("倒计时的计时器停止报错" + ex.Message);
            }
        }

        /// <summary>
        /// 刷新状态灯
        /// </summary>
        /// <param name="type">0:在线 1：离线 2：上传数据 3：上传数据异常</param>
        public void RefreshState(int type)
        {
            try
            {
                switch (type)
                {
                    case 0:
                        picState.Image = global::YL.Check.Properties.Resources.green;//在线
                        break;

                    case 1:
                        picState.Image = global::YL.Check.Properties.Resources.red;//离线
                        break;

                    case 2:
                        picState.Image = global::YL.Check.Properties.Resources.green;//上传数据
                        break;

                    case 3:
                        picState.Image = global::YL.Check.Properties.Resources.red;//上传数据异常
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("刷新状态灯报错" + ex.Message);
            }
        }

        #region 计时器事件

        /// <summary>
        /// Timer呼吸事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmr_Breath_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                #region 心跳业务逻辑
                logic.DevCheckBreath(Config.Instance.DeviceCode, Config.Instance.DeviceId);
                #endregion
            }
            catch (Exception ex)
            {
                Logger.WriteLog("链接服务器心跳失败" + ex.Message);
            }
        }

        /// <summary>
        /// 倒计时计时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmr_Countdown_Eapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (Config.Instance.LastCount != Config.Instance.CurrentCount && Config.Instance.CurrentCount > 0)
                {
                    Config.Instance.LastCount = Config.Instance.CurrentCount;
                    start = Config.Instance.CountdownNum;
                }

                while (!IsHandleCreated) { Application.DoEvents(); }
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(() =>
                    {
                        tmr_Countdown.Enabled = false;
                        if (start == Config.Instance.CountdownNum)
                        {
                            //lblCheckInfo.BackColor = Color.FromArgb(185, 208, 214);
                        }
                        lblCheckInfo.Text = (start -= 1).ToString();
                        if (start != 0)
                        {
                            tmr_Countdown.Enabled = true;
                        }
                        else
                        {
                            lblCheckInfo.Visible = false;
                            tmr_Countdown.Stop();
                            SetFormType(GetUcZr(0), new CheckResultBM(), "");
                        }
                        
                    }));
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("倒计时计时器报错" + ex.Message);
            }
        }


        #endregion  计时器事件

        //#region 离线数据下载上传

        //private void tmr_SyncOutLineTicket_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    //每日早上8点到9点
        //    if (DateTime.Now.Hour == 8 || DateTime.Now.Hour == 9)
        //    {
        //        thDownload = new Thread(new ThreadStart(logic.SaveOrderInfo));
        //        thDownload.Start();

        //        thDownloadTicket = new Thread(new ThreadStart(logic.SaveTicketVenueInfo));
        //        thDownloadTicket.Start();
        //    }

        //    if (DateTime.Now.Hour == Config.Instance.UploadTime || (Config.Instance.CurrentUploadCount >= 0 && Config.Instance.CurrentUploadCount < Config.Instance.UploadCount))
        //    {
        //        thSync = new Thread(new ThreadStart(logic.SyncCheckTicket));
        //        thSync.Start();
        //    }

        //    if (DateTime.Now.Hour == 0 || DateTime.Now.Hour == 24) //整天不关的闸机在每日凌晨初始化为未下载数据状态
        //    {
        //        logic.IniDownLoadState();
        //    }
        //}

        //#endregion
    }
    #endregion 业务处理
}