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
    /// ������
    /// </summary>
    /*
     * �����ˣ�������
     * ����ʱ�䣺2016-04-23
     *
     */

    public partial class MainForm : BaseForm
    {
        #region ���캯��&����
        /// <summary>
        /// ����ʱ�õļ�ʱ��
        /// </summary>
        private readonly System.Timers.Timer tmr_Countdown;

        /// <summary>
        /// �����õļ�ʱ��
        /// </summary>
        private readonly System.Timers.Timer tmr_Breath;

        /// <summary>
        /// ����ʱ
        /// </summary>
        private int start = Config.Instance.CountdownNum;

        private readonly System.Timers.Timer tmr_count;

        //���߼�Ʊ������״̬ʱ�����ϴ�����
        Thread thSync;
        private readonly System.Timers.Timer tmr_SyncOutLineTicket;

        Thread thDownload;

        Thread thDownloadTicket;

        LogicHandle logic = null;

        /// <summary>
        /// ���캯��
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

        #endregion ���캯��

        #region �����¼�

        /// <summary>
        /// ��������¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            #region ʱ��ͬ��

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
        /// ����ر��¼�
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

        #endregion �����¼�

        #region ҵ����

        /// <summary>
        /// ��ȡ��Ӧ����
        /// </summary>
        /// <param name="type">��������1:��Ʊ��Ч���� 2:��Ʊ�����ظ����� 3:��Ʊ�Ŷ��ظ����� 4:��Ʊ���˳ɹ����� 5:��Ʊ�Ŷӳɹ����� ������Ĭ�ϴ���</param>
        /// <returns></returns>
        public BaseFormTypeUC GetUcZr(int type)
        {
            try
            {
                BaseFormTypeUC uc;
                switch (type)
                {
                    //��Ʊ��Ч����
                    case 1:
                        uc = new CheckFailUC();
                        ReStartTimer(); //�´���չʾ�����õ���ʱ��Ϣ
                        this.picState.Visible = true;
                        break;
                    //��Ʊ�����ظ�����
                    case 2:
                        uc = new CheckSingleRepeatUC();
                        ReStartTimer(); //�´���չʾ�����õ���ʱ��Ϣ
                        this.picState.Visible = true;
                        break;
                    //��Ʊ�Ŷ��ظ�����
                    case 3:
                        uc = new CheckTeamRepeatUC();
                        ReStartTimer(); //�´���չʾ�����õ���ʱ��Ϣ
                        this.picState.Visible = true;
                        break;
                    case 4: //��Ʊ4D���˳ɹ�����
                        uc = new CheckSingleSuccessUC();
                        ReStartTimer(); //�´���չʾ�����õ���ʱ��Ϣ
                        this.picState.Visible = true;
                        break;
                    case 5: //��Ʊ�Ŷӳɹ�����
                        uc = new CheckTeamSuccessUC();
                         ReStartTimer(); //�´���չʾ�����õ���ʱ��Ϣ
                        
                        this.picState.Visible = true;
                        break;
                    //��Ʊ�����ô���
                    case 6:
                        uc = new DisableUC();
                        ReStartTimer(); //�´���չʾ�����õ���ʱ��Ϣ
                        this.picState.Visible = true;
                        break;
                    //��Ʊ����ѽ�����
                    case 7:
                        uc = new WorkCardUC();
                        ReStartTimer(); //�´���չʾ�����õ���ʱ��Ϣ
                        this.picState.Visible = true;
                        break;
                    case 8:  //�Ǳ���Ʊ
                        uc = new CheckErrorSessionUC();
                        ReStartTimer(); //�´���չʾ�����õ���ʱ��Ϣ
                        this.picState.Visible = true;
                        break;
                    case 9:  //����ɢ����Ʊ�ɹ�
                        uc = new OutLineSingleSuccessUC();
                        ReStartTimer(); //�´���չʾ�����õ���ʱ��Ϣ
                        this.picState.Visible = true;
                        break;
                    case 10:  //����ɢ����Ʊʧ�ܣ������
                        uc = new OutLineSingleRepeatUC();
                        ReStartTimer(); //�´���չʾ�����õ���ʱ��Ϣ
                        this.picState.Visible = true;
                        break;
                    case 11:  //�����Ŷ���Ʊ�ɹ�
                        uc = new OutLineTeamSuccessUC();
                        ReStartTimer(); //�´���չʾ�����õ���ʱ��Ϣ
                        this.picState.Visible = true;
                        break;
                    case 12:  //�����Ŷ���Ʊʧ�ܣ������
                        uc = new OutLineTeamRepeatUC();
                        ReStartTimer(); //�´���չʾ�����õ���ʱ��Ϣ
                        this.picState.Visible = true;
                        break;
                    //Ĭ��ͳ�ƴ���
                    default:
                        //�ж�բ���Ƿ񱻽���
                        uc = new DefaultUC();
                        this.picState.Visible = false;
                        break;
                }
                return uc;
            }
            catch (Exception ex)
            {
                Logger.WriteLog("��ȡ��Ӧ���屨��" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// ���ô���
        /// </summary>
        /// <param name="uc">��������</param>
        /// <param name="arg">���ݸ��������Ϣ</param>
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
                Logger.WriteLog("���ô������ͱ���" + ex.Message);
            }
        }

        /// <summary>
        /// ����ʱ�ļ�ʱ����ʼ
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
                Logger.WriteLog("����ʱ�ļ�ʱ��ֹͣ����" + ex.Message);
            }
        }

        /// <summary>
        /// ˢ��״̬��
        /// </summary>
        /// <param name="type">0:���� 1������ 2���ϴ����� 3���ϴ������쳣</param>
        public void RefreshState(int type)
        {
            try
            {
                switch (type)
                {
                    case 0:
                        picState.Image = global::YL.Check.Properties.Resources.green;//����
                        break;

                    case 1:
                        picState.Image = global::YL.Check.Properties.Resources.red;//����
                        break;

                    case 2:
                        picState.Image = global::YL.Check.Properties.Resources.green;//�ϴ�����
                        break;

                    case 3:
                        picState.Image = global::YL.Check.Properties.Resources.red;//�ϴ������쳣
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("ˢ��״̬�Ʊ���" + ex.Message);
            }
        }

        #region ��ʱ���¼�

        /// <summary>
        /// Timer�����¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmr_Breath_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                #region ����ҵ���߼�
                logic.DevCheckBreath(Config.Instance.DeviceCode, Config.Instance.DeviceId);
                #endregion
            }
            catch (Exception ex)
            {
                Logger.WriteLog("���ӷ���������ʧ��" + ex.Message);
            }
        }

        /// <summary>
        /// ����ʱ��ʱ��
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
                Logger.WriteLog("����ʱ��ʱ������" + ex.Message);
            }
        }


        #endregion  ��ʱ���¼�

        //#region �������������ϴ�

        //private void tmr_SyncOutLineTicket_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    //ÿ������8�㵽9��
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

        //    if (DateTime.Now.Hour == 0 || DateTime.Now.Hour == 24) //���첻�ص�բ����ÿ���賿��ʼ��Ϊδ��������״̬
        //    {
        //        logic.IniDownLoadState();
        //    }
        //}

        //#endregion
    }
    #endregion ҵ����
}