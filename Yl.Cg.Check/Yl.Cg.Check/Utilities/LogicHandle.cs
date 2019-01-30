using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using YL.Check.Model;
using YL.Check.Panels;
using System.IO;
using Yl.Ticket5.Common40;
using Yl.Cg.Model.Args.Check;
using Yl.Cg.Model;
using System.Configuration;
using Yl.Ticket5.Common40.Utilities;
using System.Data;
using System.Xml;
using Yl.Cg.Model.Bm;
using System.Data.SQLite;

using YL.Check.Devs;

namespace YL.Check.Utilities
{
    public class LogicHandle
    {
        #region 变量属性
        private Dictionary<string, CheckResultBM> msListOpenGate = new Dictionary<string, CheckResultBM>();//开闸队列

        private bool IsCheckEnd = true;//验票开始结束标识
        private MainForm _mainForm = Config.Instance.MainForm;//主窗体
        private bool isAlwaysOpen = false; //当前闸机的开关状态【true-开 false-关】
        public bool is_Online { get; set; }
        SQLiteHelper sqliteHelper;
        bool isDownload = true;

        bool isDownloadTicket = true;

        #endregion

        public LogicHandle()
        {
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
                    if (key == "OutLine_CONN")
                    {
                        dbPath = value;
                    }
                    if (key == "CHECK_TICKET")
                    {
                        Config.TicketArr = value;
                    }
                }
                //string dbPath = ConfigurationManager.AppSettings["OutLine_CONN"];
                sqliteHelper = new SQLiteHelper(dbPath);
            }
            catch (Exception ex)
            {

            }

        }

        #region 获取当前检票机可检门票ID

        public void CheckPriceLoad()
        {
            #region 加载可检票类型

            try
            {
                string url = ConfigurationManager.AppSettings["InterfacePath"] + "api/t/ticket/get_ticket_venue_id" + "?venueId=" + ConfigurationManager.AppSettings["venueId"];
                var callResult = TextureHelper.FromJson<CallResult<List<long>>>(HttpHelper.Instance.Get(url, ""));
                StringBuilder sb = new StringBuilder();
                if (callResult != null && callResult.Result.Count == 1 && callResult.Result[0] == 0)
                {
                    //ConfigurationManager.Set("CheckPices", "");
                    //MySetting.SaveSet("xCheckPices", "", false);
                }
                else
                {
                    foreach (var item in callResult.Result)
                    {
                        sb.Append(item.ToString());
                        sb.Append(",");
                    }
                    //MySetting.SaveSet("xCheckPices", sb.ToString().TrimEnd(','), true);
                    //ConfigurationManager.AppSettings["InterfacePath"] = sb.ToString().TrimEnd(',');
                    UpdateConfig("CHECK_TICKET", sb.ToString().TrimEnd(','));
                }
            }
            catch (Exception ex)
            {
                //LogFile.WriteLine(_currentDirectory + @"\CheckIn.txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + ex.Message + "\r", true);
            }
            #endregion
        }

        /// <summary>
        /// 更新配置节点数据
        /// </summary>
        /// <param name="newKey"></param>
        /// <param name="newValue"></param>
        public void UpdateConfig(string newKey, string newValue)
        {
            try
            {
                string ConfigPath = Environment.CurrentDirectory + "\\config.xml";
                XmlDocument xml = new XmlDocument();
                xml.Load(ConfigPath);
                XmlNodeList checkTicketNodes = xml.SelectNodes("/Config/add");
                foreach (XmlNode node in checkTicketNodes)
                {
                    if (node.Attributes["key"].Value == "CHECK_TICKET")
                    {
                        string oldValue = node.Attributes["value"].Value;
                        if (oldValue != newValue)
                        {
                            node.Attributes["value"].Value = newValue;
                            //xml.SelectSingleNode(newKey).InnerText = newValue;
                            xml.Save(ConfigPath);

                            Config.TicketArr = newValue;

                            break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region 开闸处理函数
        public void OpenGataFlow()
        {
            while (true)
            {
                if (msListOpenGate.Count > 0)
                {
                    var item = msListOpenGate.First();

                    #region 开闸逻辑



                    #endregion

                    msListOpenGate.Remove(item.Key);//移除第一个等待过闸的实体类

                    IsCheckEnd = true;
                }
                else
                {
                    Thread.Sleep(30);
                }
            }
        }

        #endregion

        #region 过闸数量检测函数

        private int CheckPassCount(int allowInCount, IDCardInfoArg iDCardInfoArg)
        {
            int inCount = 0;
            for (int y = 0; y < allowInCount; y++)
            {
                int i = 0;
                bool isEnd = false;
                for (i = 0; i < 100; i++) //10秒之内过闸有效
                {
                    if (Config.Instance.meQueuePassGate.Count > 0)
                    {
                        Config.Instance.meQueuePassGate.Dequeue(); //移除过闸信号
                        break;
                    }
                    Thread.Sleep(100);
                }
                if (i < 100)
                {
                    inCount++;

                    #region 更新进入人数

                    MessageWithSuccessUC lsbic;
                    foreach (var ite in _mainForm.pnContainer.Controls)
                    {

                        if (ite as MessageWithSuccessUC != null)
                        {
                            lsbic = ite as MessageWithSuccessUC;
                            lsbic.Invoke(new MethodInvoker(() =>
                            {
                                lsbic.SetInCount(inCount);
                            }));
                        }
                    }
                    #endregion

                    #region 如果有人进入，且进入人数小于可进入人数则重新计时
                    _mainForm.Invoke(new MethodInvoker(() =>
                    {
                        _mainForm.ReStartTimer();
                    }));
                    #endregion

                    if (inCount >= allowInCount)
                    {
                        //YLGateDev.CloseChnRight(Config.Instance.GateSerialPort);//过闸人数已达到允许数量，关闭闸机

                        isEnd = true;
                    }
                }
                else
                {
                    //YLGateDev.CloseChnRight(Config.Instance.GateSerialPort);//过闸超时，关闭闸机

                    isEnd = true;
                }
                if (isEnd) break;
            }

            return inCount;
        }

        /// <summary>
        /// 功能说明：过闸人数检测
        /// 创建人：ys
        /// 创建日期：2016-05-05 17：27
        /// </summary>
        /// <param name="allowInCount"></param>
        /// <returns></returns>
        private int CheckPassCount(int allowInCount)
        {
            int inCount = 0;
            for (int y = 0; y < allowInCount; y++)
            {
                int i = 0;
                bool isEnd = false;
                for (i = 0; i < 100; i++) //10秒之内过闸有效
                {
                    if (Config.Instance.meQueuePassGate.Count > 0)
                    {
                        Config.Instance.meQueuePassGate.Dequeue(); //移除过闸信号
                        break;
                    }
                    Thread.Sleep(100);
                }
                if (i < 100)
                {
                    inCount++;
                    #region 如果有人进入，且进入人数小于可进入人数则重新计时
                    _mainForm.Invoke(new MethodInvoker(() =>
                    {
                        _mainForm.ReStartTimer();
                    }));
                    #endregion

                    if (inCount >= allowInCount)
                    {
                        //YLGateDev.CloseChnLeft(ConfigurationManager.AppSettings["CloseLeftCMD"].ToString(), Config.Instance.GateSerial, "");//过闸人数已达到允许数量，关闭闸机

                        isEnd = true;
                    }
                }
                else
                {
                    //YLGateDev.CloseChnLeft(ConfigurationManager.AppSettings["CloseLeftCMD"].ToString(), Config.Instance.GateSerial, "");//过闸超时，关闭闸机

                    isEnd = true;
                }
                if (isEnd) break;
            }

            return inCount;
        }

        #endregion 过闸检测函数

        #region --------------------------------------身份证检票start-------------------------------------------

        /// <summary>
        /// 功能:身份证检票
        /// 作者:贾增义
        /// 时间:2016-4-24 17:32
        /// </summary>
        /// <param name="bm">身份证实体</param>
        public void TicketCheckByIdCard(IdCardInfoBM bm)
        {
            try
            {
                #region 身份证检票业务逻辑                
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("deviceCode", Config.Instance.DeviceCode.ToString());
                dic.Add("deviceId", Config.Instance.DeviceId.ToString());
                dic.Add("checkticketlist", Config.Instance.checkticketlist.ToString());
                dic.Add("inspectticketlist", Config.Instance.inspectticketlist.ToString());
                dic.Add("paperScanner", "");
                dic.Add("userId", ConfigurationManager.AppSettings["UserId"].ToString());
                dic.Add("cardId", bm.idcode);
                dic.Add("paperCode", "");
                dic.Add("palmScanner", "");

                if (Config.is_Online)
                {
                    #region 在线身份证验票逻辑处理

                    GetCheck(dic, obj =>
                    {
                        #region 验票结果处理
                        if (obj != null)
                        {
                            CheckResultBM checkResultBM = (CheckResultBM)obj.Result;
                            CheckOrderByCardId(checkResultBM, bm.idcode);
                        }
                        else
                        {
                            CheckResultBM checkResultBM = new CheckResultBM();
                            checkResultBM.InUserCardNo = bm.idcode;
                            ShowCheckResultUC(1, checkResultBM, "");
                            Ring(ThreadPlay.Rings.ScanFailed);
                        }
                        #endregion
                    });

                    #endregion
                }
                else
                {
                    ShowCheckResultUC(1, null, "系统已离线，请与工作人员联系！");
                    #region 离线身份证检票逻辑处理

                    //CheckResultBM checkResultBM = new CheckResultBM();
                    //CallResult<CheckResultBM> cr = new CallResult<CheckResultBM>();
                    //GetOrderInfoByCardId(1, bm.number, Config.Instance.DeviceCode.ToString(), Convert.ToInt64(Config.Instance.DeviceId), Convert.ToInt64(Config.Instance.venueId), "0", dic, ref checkResultBM, ref cr);

                    //CheckOrderByCardId(checkResultBM, bm.number);

                    #endregion
                }

                #endregion
            }
            catch (Exception ex)
            {
                Logger.WriteLog("身份证检票报错：" + ex.Message);
            }
        }

        #region 身份证离线检票

        private void CheckOrderByCardId(CheckResultBM checkResultBM, string cardId)
        {
            if (checkResultBM != null)
            {
                #region 验票结果处理

                switch (checkResultBM.CheckStatus)
                {
                    case DevCheckStatus.成功:
                        #region 验票成功

                        switch (checkResultBM.OrderType)
                        {
                            case Yl.Cg.Model.OrderTypeEnum.团队:
                                //显示验票结果界面
                                ShowCheckResultUC(5, checkResultBM, "检 票 成 功");
                                Ring(ThreadPlay.Rings.TeamPass);
                                break;
                            default:
                                //显示验票结果界面
                                ShowCheckResultUC(4, checkResultBM, "检 票 成 功");
                                Ring(ThreadPlay.Rings.Pass);
                                break;
                        }
                        //工控机发送开闸命令
                        SendControlOrder("OpenCountCMD", checkResultBM, "");

                        #endregion
                        break;
                    default:
                        #region 验票失败
                        switch (checkResultBM.OrderType)
                        {
                            case Yl.Cg.Model.OrderTypeEnum.团队:
                                #region 团队验票失败的入馆情况处理

                                switch (checkResultBM.CheckStatus)
                                {
                                    case DevCheckStatus.已入馆:
                                        ShowCheckResultUC(3, checkResultBM, checkResultBM.CheckStatus.ToString());
                                        Ring(ThreadPlay.Rings.ScanFailed);
                                        break;
                                    case DevCheckStatus.不是领队:
                                        ShowCheckResultUC(1, checkResultBM, "请刷带团人身份证入馆");
                                        Ring(ThreadPlay.Rings.ScanFailed);
                                        break;
                                    case DevCheckStatus.非本场票:
                                        ShowCheckResultUC(8, checkResultBM, checkResultBM.CheckStatus.ToString());
                                        Ring(ThreadPlay.Rings.WrongVenue);
                                        break;
                                    case DevCheckStatus.未到检票时间:
                                        ShowCheckResultUC(1, checkResultBM, "未 到 检 票 时 间");
                                        Ring(ThreadPlay.Rings.ScanFailed);
                                        break;
                                    case DevCheckStatus.检票时间已过:
                                        ShowCheckResultUC(1, checkResultBM, "检 票 时 间 已 过");
                                        Ring(ThreadPlay.Rings.InvalidDate);
                                        break;
                                    case DevCheckStatus.非本场馆票:
                                        ShowCheckResultUC(1, checkResultBM, "非本场馆或今日票");
                                        Ring(ThreadPlay.Rings.WrongVenue);
                                        break;
                                    case DevCheckStatus.已兑换:
                                        ShowCheckResultUC(1, checkResultBM, "已 兑 换 门 票");
                                        Ring(ThreadPlay.Rings.ScanFailed);
                                        break;
                                    default:
                                        ShowCheckResultUC(1, checkResultBM, checkResultBM.CheckStatus.ToString());
                                        Ring(ThreadPlay.Rings.ScanFailed);
                                        break;
                                }

                                #endregion
                                break;
                            case Yl.Cg.Model.OrderTypeEnum.散客:
                                #region 散客验票失败的入馆情况处理

                                switch (checkResultBM.CheckStatus)
                                {
                                    case DevCheckStatus.已入馆:
                                        ShowCheckResultUC(2, checkResultBM, checkResultBM.CheckStatus.ToString());
                                        Ring(ThreadPlay.Rings.ScanFailed);
                                        break;
                                    case DevCheckStatus.非本场票:
                                        ShowCheckResultUC(8, checkResultBM, checkResultBM.CheckStatus.ToString());
                                        Ring(ThreadPlay.Rings.WrongVenue);
                                        break;
                                    case DevCheckStatus.未到检票时间:
                                        ShowCheckResultUC(1, checkResultBM, "未到检票时间");
                                        Ring(ThreadPlay.Rings.ScanFailed);
                                        break;
                                    case DevCheckStatus.检票时间已过:
                                        ShowCheckResultUC(1, checkResultBM, "检票时间已过");
                                        Ring(ThreadPlay.Rings.InvalidDate);
                                        break;
                                    case DevCheckStatus.非本场馆票:
                                        ShowCheckResultUC(1, checkResultBM, "非本场馆或今日票");
                                        Ring(ThreadPlay.Rings.WrongVenue);
                                        break;
                                    case DevCheckStatus.已兑换:
                                        ShowCheckResultUC(1, checkResultBM, "已 兑 换 门 票");
                                        Ring(ThreadPlay.Rings.ScanFailed);
                                        break;
                                    default:
                                        ShowCheckResultUC(1, checkResultBM, checkResultBM.CheckStatus.ToString());
                                        Ring(ThreadPlay.Rings.ScanFailed);
                                        break;
                                }

                                #endregion
                                break;
                            default:
                                ShowCheckResultUC(1, checkResultBM, checkResultBM.CheckStatus.ToString());
                                Ring(ThreadPlay.Rings.ScanFailed);
                                break;
                        }
                        #endregion
                        break;
                }

                #endregion
            }
            else
            {
                checkResultBM = new CheckResultBM();
                checkResultBM.InUserCardNo = cardId;
                ShowCheckResultUC(1, checkResultBM, "");
                Ring(ThreadPlay.Rings.ScanFailed);
            }
        }

        #region 注释非莱芜更新查询

        /// <summary>
        /// 功能说明：根据刷取得身份证号查询该身份证下对应的子订单信息
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="deviceCode"></param>
        /// <param name="deviceId"></param>
        /// <param name="crbm"></param>
        /// <param name="cr"></param>
        private void GetOrderInfoByCardId(int isChange, string cardId, string deviceCode, long deviceId, long venueId, string userId, Dictionary<string, string> dic, ref CheckResultBM crbm, ref CallResult<CheckResultBM> cr)
        {
            //bool isOrderDetail = false;
            //bool isOrderDetailSessions = false;
            //switch (venueId)
            //{
            //    case 2:
            //    case 6:
            //        isOrderDetail = GetOrderDetailInfoByCardId(isChange, cardId, deviceCode, venueId, ref crbm, ref cr);
            //        if (!isOrderDetail)
            //        {
            //            bool isOrderDetailLong = GetOrderDetailLongInfoByCardId(isChange, cardId, deviceCode, venueId, ref crbm, ref cr);
            //        }
            //        break;
            //    case 4:
            //    case 8:
            //    case 10:
            //    case 11:
            //        isOrderDetailSessions = GetOrderDetailSessionsInfoByCardId(isChange, cardId, deviceCode, venueId, ref crbm, ref cr);
            //        break;
            //    default:
            //        isOrderDetail = GetOrderDetailInfoByCardId(isChange, cardId, deviceCode, venueId, ref crbm, ref cr);
            //        if (!isOrderDetail)
            //        {
            //            bool isOrderDetailLong = GetOrderDetailLongInfoByCardId(isChange, cardId, deviceCode, venueId, ref crbm, ref cr);
            //            if (!isOrderDetailLong)
            //            {
            //                isOrderDetailSessions = GetOrderDetailSessionsInfoByCardId(isChange, cardId, deviceCode, venueId, ref crbm, ref cr);
            //            }
            //        }
            //        break;
            //}
            //SaveCheckResultByCard(crbm, cr, deviceId, "0", dic);
        }

        /// <summary>
        /// 功能说明：根据身份证信息获取普通订单子订单及主订单信息
        /// 创建人：ys
        /// 创建日期：2016-04-29 15：49
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="deviceCode"></param>
        /// <param name="crbm"></param>
        /// <param name="cr"></param>
        /// <returns></returns>
        private bool GetOrderDetailInfoByCardId(int isChange, string cardId, string deviceCode, long venueId, ref CheckResultBM crbm, ref CallResult<CheckResultBM> cr)
        {
            //bool flag = false;
            //string orderId = "";
            //string orderDetailId = "";
            ////int isSpecialTicket = 0;
            //try
            //{
            //    List<OrderCheckBM> listCheckBM = new List<OrderCheckBM>();

            //    //1.根据身份证查询对应的普通子订单信息及主单信息
            //    StringBuilder sbSqlDetail = new StringBuilder();
            //    sbSqlDetail.Append(" select distinct orderChannel,orderType,ord.actualCount,ord.id as orderId,ordd.id,ordd.inTime ,ord.inTime as ordInTime ");
            //    sbSqlDetail.Append(" ,ticketId,ordd.personCount,paperCode,inUserCardNo,initialInDate,ord.updateTime as orderInTime ");
            //    sbSqlDetail.Append(" ,venueName,ordd.inStatus,ordd.isPrintTicket,ord.inStatus as mainInStatus,priceType,ordd.refundStatus,ordd.isDelete ");
            //    //sbSqlDetail.Append(" ,openTicketName as TicketName,tic.dayCheckCount,ordd.isSpecialTicket ");
            //    sbSqlDetail.Append(" from t_order_detail ordd ");
            //    sbSqlDetail.Append(" inner join t_order ord on ord.id = ordd.orderId ");
            //    sbSqlDetail.Append(" inner join t_ticket tic on tic.id = ordd.ticketId ");
            //    sbSqlDetail.Append(" inner join t_venue ven on tic.venueId = ven.id ");
            //    sbSqlDetail.Append(" inner join t_venue_device vendev on vendev.venueId = ven.id WHERE 1=1  ");
            //    sbSqlDetail.Append(" and deviceCode = '" + deviceCode + "' ");
            //    sbSqlDetail.Append(" and tic.venueId = '" + venueId + "' ");
            //    sbSqlDetail.Append(" and inUserCardNo = '" + cardId + "' ");
            //    sbSqlDetail.Append(" and ord.payStatus = 1 ");  //已支付的订单
            //    sbSqlDetail.Append(" and ordd.orderDetailStatus <> 0 ");
            //    //sbSqlDetail.Append(" and ordd.initialInDate = " + TextureHelper.DateToInt(DateTime.Now.Date) + " ");
            //    sbSqlDetail.Append(" group by ordd.id ");

            //    DataTable dtCard = sqliteHelper.ExecuteDataTable(sbSqlDetail.ToString());
            //    if (dtCard != null && dtCard.Rows.Count > 0)
            //    {
            //        int dateNow = TextureHelper.DateToInt(DateTime.Now);
            //        foreach (DataRow drOrd in dtCard.Rows)  //循环遍历当前身份证下所有的订单信息
            //        {
            //            int ordInTime = Convert.ToInt32(drOrd["initialInDate"].ToString());
            //            if (ordInTime == dateNow)  //获取订单的预约入馆时间是今日的订单
            //            {
            //                #region 是今日订单

            //                flag = true;
            //                //isSpecialTicket = Convert.ToInt32(drOrd["isSpecialTicket"].ToString());
            //                orderId = drOrd["orderId"].ToString();
            //                orderDetailId = drOrd["id"].ToString();

            //                if (!dtCard.Columns.Contains("ListId"))
            //                    dtCard.Columns.Add("ListId", typeof(string));
            //                string refundStatus = "0,2,6,7";

            //                if (drOrd["isDelete"].ToString() == "1")
            //                {
            //                    #region 订单已作废

            //                    drOrd["ListId"] = drOrd["id"];

            //                    DataTable dtTemp = dtCard.Clone();
            //                    dtTemp.ImportRow(drOrd);
            //                    listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                    crbm.CheckStatus = DevCheckStatus.门票已作废;
            //                    crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                    crbm.InUserCardNo = cardId;
            //                    cr.Result = crbm;
            //                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                    #endregion
            //                }
            //                else if (!refundStatus.Contains(drOrd["refundStatus"].ToString()))
            //                {
            //                    #region 订单已退款

            //                    drOrd["ListId"] = drOrd["id"];
            //                    DataTable dtTemp = dtCard.Clone();
            //                    dtTemp.ImportRow(drOrd);
            //                    listCheckBM = OrderCheckBM.FromTable(dtTemp);
            //                    //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                    crbm.CheckStatus = DevCheckStatus.门票已退款;
            //                    crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                    crbm.InUserCardNo = cardId;
            //                    cr.Result = crbm;
            //                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                    #endregion
            //                }
            //                else
            //                {
            //                    #region 订单有效

            //                    string orderType = drOrd["orderType"].ToString();
            //                    string isPrintTicket = drOrd["isPrintTicket"].ToString();
            //                    if (isPrintTicket == "1")  //已出票且并不是特种票则身份证不能验
            //                    {
            //                        drOrd["ListId"] = drOrd["id"];
            //                        DataTable dtTemp = dtCard.Clone();
            //                        dtTemp.ImportRow(drOrd);
            //                        listCheckBM = OrderCheckBM.FromTable(dtTemp);
            //                        //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                        crbm.CheckStatus = DevCheckStatus.已兑换;
            //                        crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                        crbm.InUserCardNo = cardId;
            //                        cr.Result = crbm;
            //                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                    }
            //                    else
            //                    {
            //                        #region 1.若该订单的主订单为团队订单

            //                        if (orderType == "2")
            //                        {
            //                            crbm.OrderType = Yl.Cg.Model.OrderTypeEnum.团队;
            //                            if (drOrd["inStatus"].ToString() == "1") //主订单已入馆
            //                            {
            //                                //若验票时间在1分钟之内仍然有效需要进行验票成功开闸 1分钟有效改为10秒（2016-6-6 11:07 贾增义）
            //                                //if ((isChange == 0) && Convert.ToInt64(dtCard.Rows[0]["inTime"].ToString()) >= TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.AddSeconds(-10).ToString("yyyy-MM-dd HH:mm:ss"))))
            //                                if (Convert.ToInt64(drOrd["inTime"].ToString()) >= TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.AddSeconds(-3).ToString("yyyy-MM-dd HH:mm:ss"))))
            //                                {
            //                                    #region 若在1分钟内刷卡则有效，需要开闸 1分钟有效改为10秒（2016-6-6 11:07 贾增义）

            //                                    //1.判断当前刷卡人是否是导游领队
            //                                    #region 验证当前身份证为领队身份证获取该主订单下对应的所有属于该场馆的子订单信息
            //                                    string sql1 = " select count(*) from t_order ord inner join t_order_detail ordd  on ord.id = ordd.orderId where ord.id = " + Convert.ToInt64(drOrd["orderId"].ToString()) + " and ordd.inUserCertificateType =7  and ordd.inUserCardNo = '" + cardId + "' ";
            //                                    object obj = sqliteHelper.ExecuteScalar(sql1);
            //                                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()) && Convert.ToInt32(obj.ToString()) > 0)
            //                                    {
            //                                        StringBuilder strSql = new StringBuilder();
            //                                        strSql.Append(" select orderChannel,orderType,ord.actualCount,ord.id as orderId,ordd.id,ordd.inTime ");
            //                                        strSql.Append(" ,ticketId,ordd.personCount,paperCode,inUserCardNo,initialInDate,ord.updateTime as orderInTime ");
            //                                        strSql.Append(" ,venueName,ordd.inStatus,ordd.isPrintTicket,ord.inStatus as mainInStatus,priceType,ordd.isDelete ");
            //                                        strSql.Append(" ,openTicketName as TicketName ");
            //                                        strSql.Append(" from t_order ord  ");
            //                                        strSql.Append(" inner join t_order_detail ordd  on ord.id = ordd.orderId ");
            //                                        strSql.Append(" inner join t_ticket tic on tic.id = ordd.ticketId ");
            //                                        strSql.Append(" inner join t_venue ven on tic.venueId = ven.id ");
            //                                        strSql.Append(" inner join t_venue_device vendev on vendev.venueId = ven.id WHERE 1=1 ");
            //                                        strSql.Append(" and deviceCode = '" + deviceCode + "' ");
            //                                        strSql.Append(" and tic.venueId = '" + venueId + "' ");
            //                                        strSql.Append(" and ord.id = " + Convert.ToInt64(drOrd["orderId"].ToString()) + " ");
            //                                        strSql.Append(" and ord.payStatus = 1 ");  //已支付的订单
            //                                        strSql.Append(" and ordd.refundStatus in (0,2,6,7) ");
            //                                        strSql.Append(" and ordd.orderDetailStatus <> 0 ");
            //                                        strSql.Append(" group by ordd.id ");

            //                                        DataTable dtOrder = sqliteHelper.ExecuteDataTable(strSql.ToString());
            //                                        if (dtOrder != null && dtOrder.Rows.Count > 0)
            //                                        {
            //                                            if (!dtOrder.Columns.Contains("ListId"))
            //                                                dtOrder.Columns.Add("ListId", typeof(string));
            //                                            DataTable dtTem = dtOrder.Clone();

            //                                            #region 按照票价类型进行拆分子订单数据
            //                                            Dictionary<long, int> dic = new Dictionary<long, int>(); //根据票价类型统计票数
            //                                            foreach (DataRow dr in dtOrder.Rows)
            //                                            {
            //                                                long ticketId = Convert.ToInt64(dr["ticketId"].ToString());

            //                                                if (ticketId != 279)
            //                                                {
            //                                                    if (!dic.ContainsKey(ticketId))
            //                                                        dic.Add(ticketId, Convert.ToInt32(dr["personCount"].ToString()));
            //                                                    else
            //                                                        dic[ticketId] = dic[ticketId] + Convert.ToInt32(dr["personCount"].ToString());   //dic.Add(ticketId, Convert.ToInt32(dr["personCount"].ToString()) + dic[ticketId]);
            //                                                }
            //                                            }
            //                                            #endregion

            //                                            #region 根据票价类型重新整合对应的子订单总数和Id集合

            //                                            foreach (long key in dic.Keys)
            //                                            {
            //                                                DataRow[] rows = dtOrder.Select(" ticketId =" + key + "");
            //                                                DataRow drTem = rows[0];
            //                                                StringBuilder sbListId = new StringBuilder();
            //                                                foreach (DataRow dr in rows)
            //                                                {
            //                                                    long id = Convert.ToInt64(dr["id"].ToString());
            //                                                    sbListId.Append(id + ",");
            //                                                }

            //                                                drTem["ListId"] = sbListId.ToString().TrimEnd(',');
            //                                                drTem["personCount"] = dic[key];
            //                                                drTem["InUserCardNo"] = cardId;
            //                                                dtTem.ImportRow(drTem);
            //                                            }

            //                                            #endregion

            //                                            DataView dv = dtTem.DefaultView;
            //                                            dv.Sort = " personCount DESC ";
            //                                            listCheckBM = OrderCheckBM.FromTable(dv.ToTable());
            //                                            crbm.CheckStatus = DevCheckStatus.成功;
            //                                            crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                                            crbm.ListCheckBM = listCheckBM;
            //                                            crbm.InUserCardNo = cardId;
            //                                            cr.Result = crbm;
            //                                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                            return flag;
            //                                        }
            //                                    }
            //                                    else
            //                                    {
            //                                        #region 当前身份证不是领队身份证

            //                                        crbm.CheckStatus = DevCheckStatus.不是领队;
            //                                        crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                                        crbm.InUserCardNo = cardId;
            //                                        cr.Result = crbm;
            //                                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                        #endregion
            //                                    }
            //                                    #endregion

            //                                    #endregion
            //                                }
            //                                else
            //                                {
            //                                    //if (isSpecialTicket == 1) //是特种票
            //                                    //    flag = false;

            //                                    #region 该订单已入馆

            //                                    StringBuilder strSql = new StringBuilder();
            //                                    strSql.Append(" select orderChannel,orderType,ord.actualCount,ord.id as orderId,ordd.id,ordd.inTime ");
            //                                    strSql.Append(" ,ticketId,ordd.personCount,paperCode,inUserCardNo,initialInDate,ord.updateTime as orderInTime ");
            //                                    strSql.Append(" ,venueName,ordd.inStatus,ordd.isPrintTicket,ord.inStatus as mainInStatus,priceType,ordd.isDelete ");
            //                                    strSql.Append(" ,openTicketName as TicketName ");
            //                                    strSql.Append(" from t_order ord  ");
            //                                    strSql.Append(" inner join t_order_detail ordd  on ord.id = ordd.orderId ");
            //                                    strSql.Append(" inner join t_ticket tic on tic.id = ordd.ticketId ");
            //                                    strSql.Append(" inner join t_venue ven on tic.venueId = ven.id ");
            //                                    strSql.Append(" inner join t_venue_device vendev on vendev.venueId = ven.id WHERE 1=1  ");
            //                                    strSql.Append(" and deviceCode = '" + deviceCode + "' ");
            //                                    strSql.Append(" and tic.venueId = '" + venueId + "' ");
            //                                    strSql.Append(" and ord.id = " + Convert.ToInt64(drOrd["orderId"].ToString()) + " ");
            //                                    strSql.Append(" and ord.payStatus = 1 ");  //已支付的订单
            //                                    strSql.Append(" and ordd.refundStatus in (0,2,6,7) ");
            //                                    strSql.Append(" and ordd.orderDetailStatus <> 0 ");
            //                                    strSql.Append(" group by ordd.id ");

            //                                    DataTable dtOrder = sqliteHelper.ExecuteDataTable(strSql.ToString());
            //                                    if (dtOrder != null && dtOrder.Rows.Count > 0)
            //                                    {
            //                                        if (!dtOrder.Columns.Contains("ListId"))
            //                                            dtOrder.Columns.Add("ListId", typeof(string));
            //                                        DataTable dtTem = dtOrder.Clone();

            //                                        #region 按照票价类型进行拆分子订单数据
            //                                        Dictionary<long, int> dic = new Dictionary<long, int>(); //根据票价类型统计票数
            //                                        foreach (DataRow dr in dtOrder.Rows)
            //                                        {
            //                                            long ticketId = Convert.ToInt64(dr["ticketId"].ToString());
            //                                            if (ticketId != 279)
            //                                            {
            //                                                if (!dic.ContainsKey(ticketId))
            //                                                    dic.Add(ticketId, Convert.ToInt32(dr["personCount"].ToString()));
            //                                                else
            //                                                    dic[ticketId] = dic[ticketId] + Convert.ToInt32(dr["personCount"].ToString());   // dic.Add(ticketId, Convert.ToInt32(dr["personCount"].ToString()) + dic[ticketId]);
            //                                            }
            //                                        }
            //                                        #endregion

            //                                        #region 根据票价类型重新整合对应的子订单总数和Id集合

            //                                        foreach (long key in dic.Keys)
            //                                        {
            //                                            DataRow[] rows = dtOrder.Select(" ticketId =" + key + "");
            //                                            DataRow drTem = rows[0];
            //                                            StringBuilder sbListId = new StringBuilder();
            //                                            foreach (DataRow dr in rows)
            //                                            {
            //                                                long id = Convert.ToInt64(dr["id"].ToString());
            //                                                sbListId.Append(id + ",");
            //                                            }

            //                                            drTem["ListId"] = sbListId.ToString().TrimEnd(',');
            //                                            drTem["personCount"] = rows.Length;
            //                                            drTem["InUserCardNo"] = cardId;
            //                                            dtTem.ImportRow(drTem);
            //                                        }

            //                                        #endregion

            //                                        DataView dv = dtTem.DefaultView;
            //                                        dv.Sort = " personCount DESC ";
            //                                        listCheckBM = OrderCheckBM.FromTable(dv.ToTable());
            //                                        crbm.CheckStatus = DevCheckStatus.已入馆;
            //                                        crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                                        crbm.ListCheckBM = listCheckBM;
            //                                        crbm.InUserCardNo = cardId;
            //                                        cr.Result = crbm;
            //                                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                    }

            //                                    //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                    //crbm.ListCheckBM = listCheckBM;
            //                                    //crbm.CheckStatus = DevCheckStatus.已入馆;
            //                                    //crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                                    //crbm.InUserCardNo = cardId;
            //                                    //cr.Result = crbm;
            //                                    //cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                    //cr.Type = Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                    #endregion
            //                                }
            //                            }
            //                            else
            //                            {
            //                                //1.判断当前刷卡人是否是导游领队
            //                                #region 验证当前身份证为领队身份证获取该主订单下对应的所有属于该场馆的子订单信息
            //                                string sql1 = " select count(*) from t_order ord inner join t_order_detail ordd  on ord.id = ordd.orderId where ord.id = " + Convert.ToInt64(drOrd["orderId"].ToString()) + " and ordd.inUserCertificateType =7  and ordd.inUserCardNo = '" + cardId + "' ";
            //                                object obj = sqliteHelper.ExecuteScalar(sql1);
            //                                if (obj != null && !string.IsNullOrEmpty(obj.ToString()) && Convert.ToInt32(obj.ToString()) > 0)
            //                                {
            //                                    StringBuilder strSql = new StringBuilder();
            //                                    strSql.Append(" select orderChannel,orderType,ord.actualCount,ord.id as orderId,ordd.id,ordd.inTime ");
            //                                    strSql.Append(" ,ticketId,ordd.personCount,paperCode,inUserCardNo,initialInDate,ord.updateTime as orderInTime ");
            //                                    strSql.Append(" ,venueName,ordd.inStatus,ordd.isPrintTicket,ord.inStatus as mainInStatus,priceType,ordd.isDelete ");
            //                                    strSql.Append(" ,openTicketName as TicketName ");
            //                                    strSql.Append(" from t_order ord  ");
            //                                    strSql.Append(" inner join t_order_detail ordd  on ord.id = ordd.orderId ");
            //                                    strSql.Append(" inner join t_ticket tic on tic.id = ordd.ticketId ");
            //                                    strSql.Append(" inner join t_venue ven on tic.venueId = ven.id ");
            //                                    strSql.Append(" inner join t_venue_device vendev on vendev.venueId = ven.id WHERE 1=1  ");
            //                                    strSql.Append(" and deviceCode = '" + deviceCode + "' ");
            //                                    strSql.Append(" and tic.venueId = '" + venueId + "' ");
            //                                    strSql.Append(" and ord.id = " + Convert.ToInt64(drOrd["orderId"].ToString()) + " ");
            //                                    strSql.Append(" and ord.payStatus = 1 ");  //已支付的订单
            //                                    strSql.Append(" and ordd.refundStatus in (0,2,6,7) ");
            //                                    strSql.Append(" and ordd.orderDetailStatus <> 0 ");
            //                                    strSql.Append(" group by ordd.id ");

            //                                    DataTable dtOrder = sqliteHelper.ExecuteDataTable(strSql.ToString());
            //                                    if (dtOrder != null && dtOrder.Rows.Count > 0)
            //                                    {
            //                                        if (!dtOrder.Columns.Contains("ListId"))
            //                                            dtOrder.Columns.Add("ListId", typeof(string));
            //                                        DataTable dtTem = dtOrder.Clone();

            //                                        #region 按照票价类型进行拆分子订单数据
            //                                        Dictionary<long, int> dic = new Dictionary<long, int>(); //根据票价类型统计票数
            //                                        foreach (DataRow dr in dtOrder.Rows)
            //                                        {
            //                                            long ticketId = Convert.ToInt64(dr["ticketId"].ToString());
            //                                            if (ticketId != 279)
            //                                            {
            //                                                if (!dic.ContainsKey(ticketId))
            //                                                    dic.Add(ticketId, Convert.ToInt32(dr["personCount"].ToString()));
            //                                                else
            //                                                    dic[ticketId] = dic[ticketId] + Convert.ToInt32(dr["personCount"].ToString());   // dic.Add(ticketId, Convert.ToInt32(dr["personCount"].ToString()) + dic[ticketId]);
            //                                            }
            //                                        }
            //                                        #endregion

            //                                        #region 根据票价类型重新整合对应的子订单总数和Id集合

            //                                        foreach (long key in dic.Keys)
            //                                        {
            //                                            DataRow[] rows = dtOrder.Select(" ticketId =" + key + "");
            //                                            DataRow drTem = rows[0];
            //                                            StringBuilder sbListId = new StringBuilder();
            //                                            foreach (DataRow dr in rows)
            //                                            {
            //                                                long id = Convert.ToInt64(dr["id"].ToString());
            //                                                sbListId.Append(id + ",");
            //                                            }

            //                                            drTem["ListId"] = sbListId.ToString().TrimEnd(',');
            //                                            drTem["personCount"] = rows.Length;
            //                                            drTem["InUserCardNo"] = cardId;
            //                                            dtTem.ImportRow(drTem);
            //                                        }

            //                                        #endregion

            //                                        DataView dv = dtTem.DefaultView;
            //                                        dv.Sort = " personCount DESC ";
            //                                        listCheckBM = OrderCheckBM.FromTable(dv.ToTable());
            //                                        crbm.CheckStatus = DevCheckStatus.成功;
            //                                        crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                                        crbm.ListCheckBM = listCheckBM;
            //                                        crbm.InUserCardNo = cardId;
            //                                        cr.Result = crbm;
            //                                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                        //领队子订单独立更新操作2017-01-17 17:13
            //                                        long inTime = TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            //                                        UpdateLeaderOrderDetail(Convert.ToInt64(orderDetailId), inTime);

            //                                        return flag;
            //                                    }
            //                                }
            //                                else
            //                                {
            //                                    #region 当前身份证不是领队身份证

            //                                    crbm.CheckStatus = DevCheckStatus.不是领队;
            //                                    crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                                    crbm.InUserCardNo = cardId;
            //                                    cr.Result = crbm;
            //                                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                    #endregion
            //                                }

            //                                #endregion
            //                            }
            //                        }

            //                        #endregion

            //                        #region 2.该订单的主订单为散客订单
            //                        if (orderType == "1")
            //                        {
            //                            crbm.OrderType = Yl.Cg.Model.OrderTypeEnum.散客;
            //                            drOrd["ListId"] = drOrd["id"];
            //                            if (drOrd["isPrintTicket"].ToString() == "1") //门票已兑换票纸，身份证已无效
            //                            {
            //                                crbm.CheckStatus = DevCheckStatus.已兑换;
            //                                crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                                crbm.InUserCardNo = cardId;
            //                                cr.Result = crbm;
            //                                cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                            }
            //                            else if (drOrd["inStatus"].ToString() == "1")
            //                            {
            //                                //若验票时间在1分钟之内仍然有效需要进行验票成功开闸 1分钟有效改为10秒（2016-6-6 11:07 贾增义）
            //                                if (Convert.ToInt64(drOrd["inTime"].ToString()) >= TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.AddSeconds(-3).ToString("yyyy-MM-dd HH:mm:ss"))))
            //                                {

            //                                    DataTable dtTemp = dtCard.Clone();
            //                                    dtTemp.ImportRow(drOrd);
            //                                    listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                    //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                    crbm.CheckStatus = DevCheckStatus.成功;
            //                                    crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                                    crbm.ListCheckBM = listCheckBM;
            //                                    crbm.InUserCardNo = cardId;
            //                                    cr.Result = crbm;
            //                                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                    return flag;
            //                                }
            //                                else
            //                                {
            //                                    DataTable dtTemp = dtCard.Clone();
            //                                    dtTemp.ImportRow(drOrd);
            //                                    listCheckBM = OrderCheckBM.FromTable(dtTemp);
            //                                    //listCheckBM = OrderCheckBM.FromTable(dtCard);

            //                                    crbm.ListCheckBM = listCheckBM;
            //                                    crbm.CheckStatus = DevCheckStatus.已入馆;
            //                                    crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                                    crbm.InUserCardNo = cardId;
            //                                    cr.Result = crbm;
            //                                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                }
            //                            }
            //                            else  //验票成功
            //                            {

            //                                DataTable dtTemp = dtCard.Clone();
            //                                dtTemp.ImportRow(drOrd);
            //                                listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                crbm.CheckStatus = DevCheckStatus.成功;
            //                                crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                                crbm.ListCheckBM = listCheckBM;
            //                                crbm.InUserCardNo = cardId;
            //                                cr.Result = crbm;
            //                                cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                            }
            //                        }

            //                        flag = true;

            //                        #endregion
            //                    }

            //                    #endregion
            //                }

            //                #endregion
            //            }
            //        }
            //        if (!flag)
            //        {
            //            crbm.CheckStatus = DevCheckStatus.非今日票;
            //            crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //            crbm.InUserCardNo = cardId;
            //            cr.Result = crbm;
            //            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //        }
            //    }
            //    else
            //    {
            //        crbm.CheckStatus = DevCheckStatus.非本场馆票;
            //        crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //        crbm.InUserCardNo = cardId;
            //        cr.Result = crbm;
            //        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    cr.Error = ex.Message;
            //    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckBreath_Exception;
            //    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Error;
            //    cr.Exception = ex;
            //    flag = false;
            //}

            ////特种票检票成功则需要更新特种票的入馆状态及入馆次数信息
            ////if (crbm.CheckStatus == DevCheckStatus.成功 && isSpecialTicket == 1)
            ////{
            ////    //更新对应特种票的入馆状态
            ////    UpdateOrderLongSpecialInStatus(Convert.ToInt64(orderId), Convert.ToInt64(orderDetailId), TextureHelper.DateTimeToInt(DateTime.Now), "A");
            ////}

            //return flag;
            return false;
        }

        /// <summary>
        /// 功能说明：根据身份证信息获取计次订单及主订单信息
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="deviceCode"></param>
        /// <param name="crbm"></param>
        /// <param name="cr"></param>
        /// <returns></returns>
        private bool GetOrderDetailLongInfoByCardId(int isChange, string cardId, string deviceCode, long venueId, ref CheckResultBM crbm, ref CallResult<CheckResultBM> cr)
        {
            //bool flag = false;
            //string orderId = "";
            //string orderDetailId = "";
            //int isSpecialTicket = 0;
            //int dayInCount = 1;
            //try
            //{
            //    List<OrderCheckBM> listCheckBM = new List<OrderCheckBM>();

            //    StringBuilder sbSqlCard = new StringBuilder();
            //    sbSqlCard.Append(" select distinct orderChannel,orderType,ord.actualCount,ord.id as orderId,ordd.id,ordd.inTime,ord.inTime as ordInTime,ordd.inCount,ordd.totalCount ");
            //    sbSqlCard.Append(" ,ticketId,1 as personCount,paperCode,inUserCardNo,ord.inStatus as mainInStatus,ord.updateTime as orderInTime,ordd.beginDate,ordd.endDate ");
            //    sbSqlCard.Append(" ,venueName,ordd.inStatus,ordd.isPrintTicket,priceType,ordd.beginTime,ordd.endTime,ordd.refundStatus,ordd.isDelete,ordd.inCount,ordd.totalCount ");
            //    sbSqlCard.Append(" ,openTicketName as TicketName");
            //    //sbSqlCard.Append(" ,tic.dayCheckCount ,ordd.isSpecialTicket  ");
            //    sbSqlCard.Append(" from t_order_detail_long ordd ");
            //    sbSqlCard.Append(" inner join t_order ord on ord.id = ordd.orderId ");
            //    sbSqlCard.Append(" inner join t_ticket tic on tic.id = ordd.ticketId ");
            //    sbSqlCard.Append(" inner join t_venue ven on tic.venueId = ven.id ");
            //    sbSqlCard.Append("  WHERE 1=1  ");
            //    //sbSqlCard.Append(" inner join t_venue_device vendev on vendev.venueId = ven.id WHERE 1=1  ");
            //    //sbSqlCard.Append(" and deviceCode = '" + deviceCode + "' ");
            //    //sbSqlCard.Append(" and tic.venueId = '" + venueId + "' ");
            //    sbSqlCard.Append(" and inUserCardNo = '" + cardId + "' ");
            //    //sbSqlCard.Append(" and ordd.inCount < totalCount ");  //次数未使用完
            //    sbSqlCard.Append(" and ord.payStatus = 1 ");  //已支付的订单
            //    sbSqlCard.Append(" and ordd.orderDetailStatus <> 0 ");
            //    sbSqlCard.Append(" group by ordd.id ");

            //    DataTable dtCard = sqliteHelper.ExecuteDataTable(sbSqlCard.ToString());
            //    if (dtCard != null && dtCard.Rows.Count > 0)
            //    {
            //        int dateNow = TextureHelper.DateToInt(DateTime.Now);
            //        foreach (DataRow drOrd in dtCard.Rows)
            //        {
            //            int beginDate = Convert.ToInt32(drOrd["beginDate"].ToString());
            //            int endDate = Convert.ToInt32(drOrd["endDate"].ToString());

            //            int inCount = Convert.ToInt32(drOrd["inCount"].ToString());
            //            int totalCount = Convert.ToInt32(drOrd["totalCount"].ToString());

            //            if (beginDate <= dateNow && dateNow <= endDate)
            //            {
            //                flag = true;
            //                orderId = drOrd["orderId"].ToString();
            //                orderDetailId = drOrd["id"].ToString();
            //                //dayInCount = Convert.ToInt32(drOrd["dayCheckCount"].ToString());
            //                //isSpecialTicket = Convert.ToInt32(drOrd["isSpecialTicket"].ToString());

            //                if (isSpecialTicket == 1)  //当前身份证是特种票
            //                {
            //                    #region 查询特种票信息

            //                    StringBuilder sbSql = new StringBuilder();
            //                    sbSql.Append(" select distinct orderChannel,orderType,ord.actualCount,ord.id as orderId,ordd.id,ordd.inTime ,ord.inTime as ordInTime ");
            //                    sbSql.Append(" ,ticketId,personCount,paperCode,inUserCardNo,ord.inStatus as mainInStatus ,priceType ,ordd.inCount ,ordd.totalCount ");
            //                    sbSql.Append(" ,venueName,ordd.inStatus,ordd.isPrintTicket,ord.updateTime as orderInTime,ordd.beginTime,ordd.endTime,ordd.refundStatus,ordd.isDelete,ordd.totalCount,ordd.inCount ");
            //                    sbSql.Append(" ,openTicketName as TicketName,tic.dayCheckCount ");
            //                    sbSql.Append(" ,ordde.InCount as venueInCount,ordde.TotalCount as venueTotalCount ");
            //                    sbSql.Append(" from t_order_detail_long ordd ");
            //                    sbSql.Append(" inner join t_order ord on ord.id = ordd.orderId ");
            //                    sbSql.Append(" inner join t_orderlong_special_extend ordde on ordde.orderDetailLongId = ordd.id ");
            //                    sbSql.Append(" inner join t_ticket tic on tic.id = ordd.ticketId ");
            //                    sbSql.Append(" inner join t_venue ven on ordde.venueId = ven.id ");
            //                    sbSql.Append("  WHERE 1=1 ");
            //                    //sbSql.Append(" inner join t_venue_device vendev on vendev.venueId = ven.id WHERE 1=1 ");
            //                    sbSql.Append(" and ordde.venueId = '" + venueId + "' ");
            //                    sbSql.Append(" and ordd.id = '" + orderDetailId + "' ");
            //                    sbSql.Append(" and ordd.isSpecialTicket = " + isSpecialTicket + " ");
            //                    sbSql.Append(" and ord.payStatus = 1 ");  //已支付的订单
            //                    sbSql.Append(" and ordd.orderDetailStatus <> 0 ");
            //                    sbSql.Append(" group by ordd.id ");

            //                    DataTable dtTem = sqliteHelper.ExecuteDataTable(sbSql.ToString());

            //                    #endregion

            //                    if (dtTem != null && dtTem.Rows.Count > 0)
            //                    {
            //                        #region 特种票处理

            //                        #region 是今日订单

            //                        flag = true;
            //                        if (!dtTem.Columns.Contains("ListId"))
            //                            dtTem.Columns.Add("ListId", typeof(string));
            //                        string refundStatus = "0,2,6,7";
            //                        if (dtTem.Rows[0]["isDelete"].ToString() == "1")
            //                        {
            //                            #region 门票已作废

            //                            dtTem.Rows[0]["ListId"] = dtTem.Rows[0]["id"];

            //                            DataTable dtTemp = dtTem.Clone();
            //                            dtTemp.ImportRow(dtTem.Rows[0]);
            //                            listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                            //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                            crbm.CheckStatus = DevCheckStatus.门票已作废;
            //                            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                            crbm.PaperCode = cardId;
            //                            cr.Result = crbm;
            //                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                            #endregion
            //                        }
            //                        else if (inCount >= totalCount)
            //                        {
            //                            dtTem.Rows[0]["ListId"] = dtTem.Rows[0]["id"];

            //                            DataTable dtTemp = dtTem.Clone();
            //                            dtTemp.ImportRow(dtTem.Rows[0]);
            //                            listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                            //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                            crbm.CheckStatus = DevCheckStatus.次数超限;
            //                            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                            crbm.ListCheckBM = listCheckBM;
            //                            crbm.PaperCode = cardId;
            //                            cr.Result = crbm;
            //                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                        }
            //                        else if (!refundStatus.Contains(dtTem.Rows[0]["refundStatus"].ToString()))
            //                        {
            //                            #region 门票已退款

            //                            dtTem.Rows[0]["ListId"] = dtTem.Rows[0]["id"];

            //                            DataTable dtTemp = dtTem.Clone();
            //                            dtTemp.ImportRow(dtTem.Rows[0]);
            //                            listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                            //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                            crbm.CheckStatus = DevCheckStatus.门票已退款;
            //                            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                            crbm.PaperCode = cardId;
            //                            cr.Result = crbm;
            //                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                            #endregion
            //                        }
            //                        else
            //                        {
            //                            #region 订单有效

            //                            string orderType = dtTem.Rows[0]["orderType"].ToString();
            //                            string isPrintTicket = dtTem.Rows[0]["isPrintTicket"].ToString(); ;

            //                            int beginTime = Convert.ToInt32(dtTem.Rows[0]["beginTime"]);
            //                            int endTime = Convert.ToInt32(dtTem.Rows[0]["endTime"]);
            //                            int currentTime = TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks));
            //                            if (currentTime >= beginTime && currentTime <= endTime)  //是当次门票
            //                            {
            //                                #region 1.若该订单的主订单为团队订单

            //                                if (orderType == "2")
            //                                {
            //                                    crbm.OrderType = Yl.Cg.Model.OrderTypeEnum.团队;
            //                                    if (dtTem.Rows[0]["inStatus"].ToString() == "1") //主订单已入馆
            //                                    {
            //                                        //判定当前得入馆时间是否是今日入馆
            //                                        long inTime = Convert.ToInt64(dtTem.Rows[0]["inTime"].ToString());
            //                                        long todayBeginTime = TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00")));
            //                                        long todayEndTime = TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")));
            //                                        if (inTime >= todayBeginTime && inTime <= todayEndTime)  //今日已有验票记录
            //                                        {
            //                                            if (dayInCount == 0)  //每天可以不限次进入
            //                                            {
            //                                                if (Convert.ToInt32(dtTem.Rows[0]["inCount"]) < Convert.ToInt32(dtTem.Rows[0]["totalCount"]))
            //                                                {
            //                                                    DataTable dtTemp = dtTem.Clone();
            //                                                    dtTemp.ImportRow(dtTem.Rows[0]);
            //                                                    listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                                    //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                                    crbm.CheckStatus = DevCheckStatus.成功;
            //                                                    crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                                    crbm.ListCheckBM = listCheckBM;
            //                                                    crbm.InUserCardNo = cardId.ToString();
            //                                                    cr.Result = crbm;
            //                                                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                                    break;
            //                                                }
            //                                                else
            //                                                {
            //                                                    DataTable dtTemp = dtTem.Clone();
            //                                                    dtTemp.ImportRow(dtTem.Rows[0]);
            //                                                    listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                                    //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                                    crbm.ListCheckBM = listCheckBM;
            //                                                    crbm.CheckStatus = DevCheckStatus.次数超限;
            //                                                    crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                                    crbm.InUserCardNo = cardId.ToString();
            //                                                    cr.Result = crbm;
            //                                                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                                }
            //                                            }
            //                                            else
            //                                            {
            //                                                DataTable dtTemp = dtTem.Clone();
            //                                                dtTemp.ImportRow(dtTem.Rows[0]);
            //                                                listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                                //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                                crbm.ListCheckBM = listCheckBM;
            //                                                crbm.CheckStatus = DevCheckStatus.已入馆;
            //                                                crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                                crbm.InUserCardNo = cardId.ToString();
            //                                                cr.Result = crbm;
            //                                                cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                                cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                            }
            //                                        }
            //                                        else
            //                                        {
            //                                            DataTable dtTemp = dtTem.Clone();
            //                                            dtTemp.ImportRow(dtTem.Rows[0]);
            //                                            listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                            //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                            crbm.CheckStatus = DevCheckStatus.成功;
            //                                            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                            crbm.ListCheckBM = listCheckBM;
            //                                            crbm.InUserCardNo = cardId;
            //                                            cr.Result = crbm;
            //                                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                            break;
            //                                        }
            //                                    }
            //                                    else
            //                                    {
            //                                        if (dtTem.Rows[0]["isPrintTicket"].ToString() == "1") //门票已兑换票纸，身份证已无效
            //                                        {
            //                                            crbm.CheckStatus = DevCheckStatus.已兑换;
            //                                            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                            crbm.InUserCardNo = cardId;
            //                                            cr.Result = crbm;
            //                                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                        }
            //                                        else
            //                                        {
            //                                            DataTable dtTemp = dtTem.Clone();
            //                                            dtTemp.ImportRow(dtTem.Rows[0]);
            //                                            listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                            //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                            crbm.CheckStatus = DevCheckStatus.成功;
            //                                            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                            crbm.ListCheckBM = listCheckBM;
            //                                            crbm.InUserCardNo = cardId;
            //                                            cr.Result = crbm;
            //                                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                            break;
            //                                        }

            //                                    }
            //                                }

            //                                #endregion

            //                                #region 2.计次票子订单信息及主单信息为散客订单
            //                                if (orderType == "1")
            //                                {
            //                                    crbm.OrderType = Yl.Cg.Model.OrderTypeEnum.散客;
            //                                    dtTem.Rows[0]["ListId"] = dtTem.Rows[0]["id"];
            //                                    if (dtTem.Rows[0]["isPrintTicket"].ToString() == "1") //门票已兑换票纸，身份证已无效
            //                                    {
            //                                        crbm.CheckStatus = DevCheckStatus.已兑换;
            //                                        crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                        crbm.InUserCardNo = cardId;
            //                                        cr.Result = crbm;
            //                                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                    }
            //                                    else if (dtTem.Rows[0]["inStatus"].ToString() == "1")
            //                                    {
            //                                        //判定当前得入馆时间是否是今日入馆
            //                                        long inTime = Convert.ToInt64(dtTem.Rows[0]["inTime"].ToString());
            //                                        long todayBeginTime = TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00:00")));
            //                                        long todayEndTime = TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59")));
            //                                        if (inTime >= todayBeginTime && inTime <= todayEndTime)  //今日已有验票记录
            //                                        {
            //                                            #region 已入馆

            //                                            if (dayInCount == 0)  //每天可以不限次进入
            //                                            {
            //                                                if (Convert.ToInt32(dtTem.Rows[0]["inCount"]) < Convert.ToInt32(dtTem.Rows[0]["totalCount"]))
            //                                                {
            //                                                    DataTable dtTemp = dtTem.Clone();
            //                                                    dtTemp.ImportRow(dtTem.Rows[0]);
            //                                                    listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                                    //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                                    crbm.CheckStatus = DevCheckStatus.成功;
            //                                                    crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                                    crbm.ListCheckBM = listCheckBM;
            //                                                    crbm.InUserCardNo = cardId.ToString();
            //                                                    cr.Result = crbm;
            //                                                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                                    break;
            //                                                }
            //                                                else
            //                                                {
            //                                                    DataTable dtTemp = dtTem.Clone();
            //                                                    dtTemp.ImportRow(dtTem.Rows[0]);
            //                                                    listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                                    //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                                    crbm.ListCheckBM = listCheckBM;
            //                                                    crbm.CheckStatus = DevCheckStatus.次数超限;
            //                                                    crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                                    crbm.InUserCardNo = cardId.ToString();
            //                                                    cr.Result = crbm;
            //                                                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                                }
            //                                            }
            //                                            else
            //                                            {
            //                                                DataTable dtTemp = dtTem.Clone();
            //                                                dtTemp.ImportRow(dtTem.Rows[0]);
            //                                                listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                                //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                                crbm.ListCheckBM = listCheckBM;
            //                                                crbm.CheckStatus = DevCheckStatus.已入馆;
            //                                                crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                                crbm.InUserCardNo = cardId.ToString();
            //                                                cr.Result = crbm;
            //                                                cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                                cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                            }

            //                                            #endregion
            //                                        }
            //                                        else
            //                                        {
            //                                            DataTable dtTemp = dtTem.Clone();
            //                                            dtTemp.ImportRow(dtTem.Rows[0]);
            //                                            listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                            //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                            crbm.CheckStatus = DevCheckStatus.成功;
            //                                            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                            crbm.InUserCardNo = cardId;
            //                                            crbm.ListCheckBM = listCheckBM;
            //                                            cr.Result = crbm;
            //                                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                            break;
            //                                        }

            //                                    }
            //                                    else  //验票成功
            //                                    {
            //                                        DataTable dtTemp = dtTem.Clone();
            //                                        dtTemp.ImportRow(dtTem.Rows[0]);
            //                                        listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                        //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                        crbm.CheckStatus = DevCheckStatus.成功;
            //                                        crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                        crbm.InUserCardNo = cardId;
            //                                        crbm.ListCheckBM = listCheckBM;
            //                                        cr.Result = crbm;
            //                                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                        break;
            //                                    }
            //                                }
            //                                flag = true;
            //                                #endregion
            //                            }
            //                            else
            //                            {
            //                                #region 非本场票

            //                                crbm.CheckStatus = DevCheckStatus.非本场票;
            //                                crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                crbm.PaperCode = cardId;
            //                                cr.Result = crbm;
            //                                cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                #endregion
            //                            }

            //                            #endregion
            //                        }

            //                        #endregion
            //                        //}

            //                        #endregion
            //                    }
            //                }
            //                else
            //                {
            //                    #region 期效票处理

            //                    #region 是今日订单

            //                    flag = true;
            //                    if (!dtCard.Columns.Contains("ListId"))
            //                        dtCard.Columns.Add("ListId", typeof(string));
            //                    string refundStatus = "0,2,6,7";
            //                    if (drOrd["isDelete"].ToString() == "1")
            //                    {
            //                        #region 门票已作废

            //                        drOrd["ListId"] = drOrd["id"];

            //                        DataTable dtTemp = dtCard.Clone();
            //                        dtTemp.ImportRow(drOrd);
            //                        listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                        //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                        crbm.CheckStatus = DevCheckStatus.门票已作废;
            //                        crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                        crbm.PaperCode = cardId;
            //                        cr.Result = crbm;
            //                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                        #endregion
            //                    }
            //                    else if (inCount >= totalCount)
            //                    {
            //                        drOrd["ListId"] = drOrd["id"];

            //                        DataTable dtTemp = dtCard.Clone();
            //                        dtTemp.ImportRow(drOrd);
            //                        listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                        //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                        crbm.CheckStatus = DevCheckStatus.次数超限;
            //                        crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                        crbm.ListCheckBM = listCheckBM;
            //                        crbm.PaperCode = cardId;
            //                        cr.Result = crbm;
            //                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                    }
            //                    else if (!refundStatus.Contains(drOrd["refundStatus"].ToString()))
            //                    {
            //                        #region 门票已退款

            //                        drOrd["ListId"] = drOrd["id"];

            //                        DataTable dtTemp = dtCard.Clone();
            //                        dtTemp.ImportRow(drOrd);
            //                        listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                        //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                        crbm.CheckStatus = DevCheckStatus.门票已退款;
            //                        crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                        crbm.PaperCode = cardId;
            //                        cr.Result = crbm;
            //                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                        #endregion
            //                    }
            //                    else
            //                    {
            //                        #region 订单有效

            //                        string orderType = drOrd["orderType"].ToString();

            //                        string isPrintTicket = drOrd["isPrintTicket"].ToString(); ;
            //                        if (isPrintTicket == "1")  //已出票则身份证不能验
            //                        {
            //                            #region 门票已兑换

            //                            drOrd["ListId"] = drOrd["id"];

            //                            DataTable dtTemp = dtCard.Clone();
            //                            dtTemp.ImportRow(drOrd);
            //                            listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                            //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                            crbm.CheckStatus = DevCheckStatus.已兑换;
            //                            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                            cr.Result = crbm;
            //                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                            #endregion
            //                        }
            //                        else
            //                        {
            //                            int beginTime = Convert.ToInt32(drOrd["beginTime"]);
            //                            int endTime = Convert.ToInt32(drOrd["endTime"]);
            //                            int currentTime = TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks));
            //                            if (currentTime >= beginTime && currentTime <= endTime)  //是当次门票
            //                            {
            //                                #region 1.若该订单的主订单为团队订单

            //                                if (orderType == "2")
            //                                {
            //                                    crbm.OrderType = Yl.Cg.Model.OrderTypeEnum.团队;
            //                                    if (drOrd["inStatus"].ToString() == "1") //主订单已入馆
            //                                    {
            //                                        if (Convert.ToInt32(drOrd["inCount"]) < Convert.ToInt32(drOrd["totalCount"]))
            //                                        {

            //                                            DataTable dtTemp = dtCard.Clone();
            //                                            dtTemp.ImportRow(drOrd);
            //                                            listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                            //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                            crbm.CheckStatus = DevCheckStatus.成功;
            //                                            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                            crbm.ListCheckBM = listCheckBM;
            //                                            crbm.InUserCardNo = cardId.ToString();
            //                                            cr.Result = crbm;
            //                                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                            break;
            //                                        }
            //                                        else
            //                                        {
            //                                            DataTable dtTemp = dtCard.Clone();
            //                                            dtTemp.ImportRow(drOrd);
            //                                            listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                            //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                            crbm.ListCheckBM = listCheckBM;
            //                                            crbm.CheckStatus = DevCheckStatus.次数超限;
            //                                            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                            crbm.InUserCardNo = cardId.ToString();
            //                                            cr.Result = crbm;
            //                                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                        }
            //                                    }
            //                                    else
            //                                    {
            //                                        if (drOrd["isPrintTicket"].ToString() == "1") //门票已兑换票纸，身份证已无效
            //                                        {
            //                                            crbm.CheckStatus = DevCheckStatus.已兑换;
            //                                            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                            crbm.InUserCardNo = cardId;
            //                                            cr.Result = crbm;
            //                                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                        }
            //                                        else
            //                                        {
            //                                            DataTable dtTemp = dtCard.Clone();
            //                                            dtTemp.ImportRow(drOrd);
            //                                            listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                            //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                            crbm.CheckStatus = DevCheckStatus.成功;
            //                                            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                            crbm.ListCheckBM = listCheckBM;
            //                                            crbm.InUserCardNo = cardId;
            //                                            cr.Result = crbm;
            //                                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                            break;
            //                                        }

            //                                    }
            //                                }

            //                                #endregion

            //                                #region 2.计次票子订单信息及主单信息为散客订单
            //                                if (orderType == "1")
            //                                {
            //                                    crbm.OrderType = Yl.Cg.Model.OrderTypeEnum.散客;
            //                                    drOrd["ListId"] = drOrd["id"];
            //                                    if (drOrd["isPrintTicket"].ToString() == "1") //门票已兑换票纸，身份证已无效
            //                                    {
            //                                        crbm.CheckStatus = DevCheckStatus.已兑换;
            //                                        crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                        crbm.InUserCardNo = cardId;
            //                                        cr.Result = crbm;
            //                                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                    }
            //                                    else if (drOrd["inStatus"].ToString() == "1")
            //                                    {
            //                                        #region 已入馆

            //                                        if (dayInCount == 0)  //每天可以不限次进入
            //                                        {
            //                                            if (Convert.ToInt32(drOrd["inCount"]) < Convert.ToInt32(drOrd["totalCount"]))
            //                                            {
            //                                                DataTable dtTemp = dtCard.Clone();
            //                                                dtTemp.ImportRow(drOrd);
            //                                                listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                                //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                                crbm.CheckStatus = DevCheckStatus.成功;
            //                                                crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                                crbm.ListCheckBM = listCheckBM;
            //                                                crbm.InUserCardNo = cardId.ToString();
            //                                                cr.Result = crbm;
            //                                                cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                                cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                                break;
            //                                            }
            //                                            else
            //                                            {
            //                                                DataTable dtTemp = dtCard.Clone();
            //                                                dtTemp.ImportRow(drOrd);
            //                                                listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                                //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                                crbm.ListCheckBM = listCheckBM;
            //                                                crbm.CheckStatus = DevCheckStatus.次数超限;
            //                                                crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                                crbm.InUserCardNo = cardId.ToString();
            //                                                cr.Result = crbm;
            //                                                cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                                cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                            }
            //                                        }
            //                                        else
            //                                        {
            //                                            DataTable dtTemp = dtCard.Clone();
            //                                            dtTemp.ImportRow(drOrd);
            //                                            listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                            //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                            crbm.ListCheckBM = listCheckBM;
            //                                            crbm.CheckStatus = DevCheckStatus.已入馆;
            //                                            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                            crbm.InUserCardNo = cardId.ToString();
            //                                            cr.Result = crbm;
            //                                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                        }

            //                                        #endregion


            //                                    }
            //                                    else  //验票成功
            //                                    {
            //                                        DataTable dtTemp = dtCard.Clone();
            //                                        dtTemp.ImportRow(drOrd);
            //                                        listCheckBM = OrderCheckBM.FromTable(dtTemp);

            //                                        //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                        crbm.CheckStatus = DevCheckStatus.成功;
            //                                        crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                        crbm.InUserCardNo = cardId;
            //                                        crbm.ListCheckBM = listCheckBM;
            //                                        cr.Result = crbm;
            //                                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                        break;
            //                                    }
            //                                }
            //                                flag = true;
            //                                #endregion
            //                            }
            //                            else
            //                            {
            //                                #region 非本场票

            //                                crbm.CheckStatus = DevCheckStatus.非本场票;
            //                                crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //                                crbm.PaperCode = cardId;
            //                                cr.Result = crbm;
            //                                cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                #endregion
            //                            }
            //                        }

            //                        #endregion
            //                    }

            //                    #endregion

            //                    #endregion
            //                }
            //            }
            //        }
            //        if (!flag)
            //        {
            //            crbm.CheckStatus = DevCheckStatus.非今日票;
            //            crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //            cr.Result = crbm;
            //            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //        }
            //    }
            //    else
            //    {
            //        crbm.CheckStatus = DevCheckStatus.非本场馆票;
            //        crbm.TicketOrderType = TicketOrderTypeEnum.计次票;
            //        cr.Result = crbm;
            //        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    cr.Error = ex.Message;
            //    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckBreath_Exception;
            //    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Error;
            //    cr.Exception = ex;
            //    flag = false;
            //}

            ////***特种票验证成功则需要更新特种票子订单信息及生成相应的对应订单信息***
            ////if (crbm.CheckStatus == DevCheckStatus.成功 && isSpecialTicket == 1)
            ////{
            ////    StringBuilder sbSql = new StringBuilder();
            ////    sbSql.Append(" select id,venueId,TotalCount,InCount,grownManCount,studentCount,oldManCount,grownManTicketId,studentTicketId,oldManTicketId from t_orderlong_special_extend ");
            ////    sbSql.Append(" where status = 1 and  isDelete = 0 and venueId = " + venueId + " and orderDetailLongId = " + orderDetailId + "");
            ////    DataTable dt = sqliteHelper.ExecuteDataTable(sbSql.ToString());
            ////    if (dt != null && dt.Rows.Count > 0)
            ////    {
            //        #region 门票信息处理

            ////        Dictionary<long, int> dicTicket = new Dictionary<long, int>();
            ////        if (!string.IsNullOrEmpty(dt.Rows[0]["grownManTicketId"].ToString()))
            ////        {
            ////            long grownManTicketId = Convert.ToInt64(dt.Rows[0]["grownManTicketId"].ToString());
            ////            int grownManCount = Convert.ToInt32(dt.Rows[0]["grownManCount"].ToString());
            ////            dicTicket.Add(grownManTicketId, grownManCount);
            ////        }
            ////        if (!string.IsNullOrEmpty(dt.Rows[0]["studentTicketId"].ToString()))
            ////        {
            ////            long studentTicketId = Convert.ToInt64(dt.Rows[0]["studentTicketId"].ToString());
            ////            int studentCount = Convert.ToInt32(dt.Rows[0]["studentCount"].ToString());
            ////            dicTicket.Add(studentTicketId, studentCount);
            ////        }
            ////        if (!string.IsNullOrEmpty(dt.Rows[0]["oldManTicketId"].ToString()))
            ////        {
            ////            long oldManTicketId = Convert.ToInt64(dt.Rows[0]["oldManTicketId"].ToString());
            ////            int oldManCount = Convert.ToInt32(dt.Rows[0]["oldManCount"].ToString());
            ////            dicTicket.Add(oldManTicketId, oldManCount);
            ////        }

            //        #endregion

            ////        string detailType = "A";
            ////        if (venueId == 4 || venueId == 8 || venueId == 10 || venueId == 11 || venueId == 13)
            ////            detailType = "C";

            ////        CreatOrder(Convert.ToInt64(orderId), Convert.ToInt64(orderDetailId), dicTicket, 0, Convert.ToInt64(deviceId), detailType, venueId);
            ////    }
            ////}


            //return flag;
            return false;
        }

        /// <summary>
        /// 功能说明：根据身份证信息获取场次订单及主订单信息
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="deviceCode"></param>
        /// <param name="crbm"></param>
        /// <param name="cr"></param>
        /// <returns></returns>
        private bool GetOrderDetailSessionsInfoByCardId(int isChange, string cardId, string deviceCode, long venueId, ref CheckResultBM crbm, ref CallResult<CheckResultBM> cr)
        {
            bool flag = false;
            //bool bl = false;
            //string orderId = "";
            //string orderDetailId = "";
            ////int isSpecialTicket = 0;
            //try
            //{
            //    List<OrderCheckBM> listCheckBM = new List<OrderCheckBM>();

            //    StringBuilder sbSqlCard = new StringBuilder();
            //    sbSqlCard.Append(" select distinct orderType,orderChannel,ord.actualCount,ord.id as orderId,ordd.id,ordd.inTime ,ord.inTime as ordInTime");
            //    sbSqlCard.Append(" ,ticketId,1 as personCount,paperCode,inUserCardNo,ord.inStatus as mainInStatus,ord.updateTime as orderInTime,ordd.beginDate,ordd.endDate ");
            //    sbSqlCard.Append(" ,venueName,ordd.inStatus,ordd.isPrintTicket,priceType,ordd.beginTime,ordd.endTime,ordd.refundStatus,ordd.isDelete ");
            //    sbSqlCard.Append(" ,openTicketName as TicketName,videoName ");
            //    //sbSqlCard.Append(" ,tic.dayCheckCount,ordd.isSpecialTicket ");
            //    sbSqlCard.Append(" from t_order_detail_sessions ordd ");
            //    sbSqlCard.Append(" inner join t_order ord on ord.id = ordd.orderId ");
            //    sbSqlCard.Append(" inner join t_ticket tic on tic.id = ordd.ticketId ");
            //    sbSqlCard.Append(" inner join t_sessions ts on ts.id = ordd.sessionsId ");
            //    sbSqlCard.Append(" inner join t_session_video tsv on ts.sessionsVideoId = tsv.id ");
            //    sbSqlCard.Append(" inner join t_venue ven on tic.venueId = ven.id ");
            //    sbSqlCard.Append(" inner join t_venue_device vendev on vendev.venueId = ven.id WHERE 1=1  ");
            //    sbSqlCard.Append(" and deviceCode = '" + deviceCode + "' ");
            //    sbSqlCard.Append(" and tic.venueId = '" + venueId + "' ");
            //    sbSqlCard.Append(" and inUserCardNo = '" + cardId + "' ");
            //    sbSqlCard.Append(" and ord.payStatus = 1 ");  //已支付的订单
            //    sbSqlCard.Append(" and ordd.orderDetailStatus <> 0 ");
            //    //sbSqlCard.Append(" and ordd.orderDetailStatus <> 0 ");
            //    //sbSqlCard.Append(" and ordd.refundStatus in (0,2,6,7) ");
            //    //sbSqlCard.Append(" and ordd.beginDate <= " + TextureHelper.DateToInt(DateTime.Now.Date) + " and endDate >=  " + TextureHelper.DateToInt(DateTime.Now.Date) + " ");
            //    //sbSqlCard.Append(" group by ordd.id ");
            //    string sqlAdd = " order by ordd.beginTime DESC ";

            //    DataTable dtAll = sqliteHelper.ExecuteDataTable(sbSqlCard.ToString() + sqlAdd);
            //    if (dtAll != null && dtAll.Rows.Count > 0)
            //    {
            //        int dateNow = TextureHelper.DateToInt(DateTime.Now);
            //        foreach (DataRow drOrd in dtAll.Rows)
            //        {
            //            int beginDate = Convert.ToInt32(drOrd["beginDate"].ToString());
            //            int endDate = Convert.ToInt32(drOrd["endDate"].ToString());

            //            int sessionsBeginTime = Convert.ToInt32(drOrd["beginTime"]);
            //            int sessionsEndTime = Convert.ToInt32(drOrd["endTime"]);
            //            int beforeMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["BEFORE_MINUTES"]);
            //            string beginTime = TextureHelper.IntToTime(sessionsBeginTime).ToString();
            //            long tem = Convert.ToDateTime(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd" + " " + beginTime)).AddMinutes(-beforeMinutes).ToString("HH:mm:ss")).Ticks;
            //            int beforeTime = TextureHelper.TimeToInt(new TimeSpan(tem));

            //            //int orderDetailStatus = Convert.ToInt32(drOrd["orderDetailStatus"]);

            //            if (beginDate <= dateNow && dateNow <= endDate)
            //            {
            //                flag = true;
            //                if (TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks)) >= beforeTime && TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks)) <= sessionsEndTime)
            //                {
            //                    bl = true;

            //                    //isSpecialTicket = Convert.ToInt32(drOrd["isSpecialTicket"].ToString());
            //                    orderId = drOrd["orderId"].ToString();
            //                    orderDetailId = drOrd["id"].ToString();

            //                    #region 是今日订单

            //                    string refundStatus = "0,2,6,7";

            //                    if (drOrd["isDelete"].ToString() == "1")
            //                    {
            //                        crbm.CheckStatus = DevCheckStatus.门票已作废;
            //                        crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                        crbm.InUserCardNo = cardId;
            //                        cr.Result = crbm;
            //                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                    }
            //                    else if (!refundStatus.Contains(drOrd["refundStatus"].ToString()))
            //                    {
            //                        crbm.CheckStatus = DevCheckStatus.门票已退款;
            //                        crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                        crbm.InUserCardNo = cardId;
            //                        cr.Result = crbm;
            //                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                    }
            //                    else
            //                    {
            //                        #region 订单有效

            //                        //int sessionsBeginTime = Convert.ToInt32(drOrd["beginTime"]);
            //                        //int sessionsEndTime = Convert.ToInt32(drOrd["endTime"]);
            //                        //int beforeMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["BEFORE_MINUTES"]);
            //                        //string beginTime = TextureHelper.IntToTime(sessionsBeginTime).ToString();
            //                        //long tem = Convert.ToDateTime(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd" + " " + beginTime)).AddMinutes(-beforeMinutes).ToString("HH:mm:ss")).Ticks;
            //                        //int beforeTime = TextureHelper.TimeToInt(new TimeSpan(tem));

            //                        if (TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks)) >= beforeTime && TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks)) <= sessionsEndTime)
            //                        {
            //                            #region 订单信息存在

            //                            bl = true;
            //                            //票纸信息存在
            //                            long detailId = Convert.ToInt64(drOrd["id"]);
            //                            sbSqlCard.Append(" and ordd.id = " + detailId + " ");
            //                            sbSqlCard.Append(" group by ordd.id ");
            //                            DataTable dtCard = sqliteHelper.ExecuteDataTable(sbSqlCard.ToString() + sqlAdd);
            //                            if (dtCard != null && dtCard.Rows.Count > 0)
            //                            {
            //                                if (!dtCard.Columns.Contains("ListId"))
            //                                    dtCard.Columns.Add("ListId", typeof(string));
            //                                string orderType = dtCard.Rows[0]["orderType"].ToString();

            //                                string isPrintTicket = dtCard.Rows[0]["isPrintTicket"].ToString(); ;
            //                                if (isPrintTicket == "1")  //已出票则身份证不能验
            //                                {
            //                                    crbm.CheckStatus = DevCheckStatus.已兑换;
            //                                    crbm.TicketOrderType = TicketOrderTypeEnum.普通门票;
            //                                    cr.Result = crbm;
            //                                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                }
            //                                else
            //                                {
            //                                    #region 1.若该订单的主订单为团队订单

            //                                    if (orderType == "2")
            //                                    {
            //                                        crbm.OrderType = Yl.Cg.Model.OrderTypeEnum.团队;
            //                                        if (dtCard.Rows[0]["inStatus"].ToString() == "1") //主订单已入馆
            //                                        {
            //                                            //若验票时间在1分钟之内仍然有效需要进行验票成功开闸 1分钟有效改为10秒（2016-6-6 11:07 贾增义）
            //                                            if (Convert.ToInt64(dtCard.Rows[0]["inTime"].ToString()) >= TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.AddSeconds(-3).ToString("yyyy-MM-dd HH:mm:ss"))))
            //                                            {
            //                                                //1.判断当前刷卡人是否是导游领队
            //                                                #region 验证当前身份证为领队身份证获取该主订单下对应的所有属于该场馆的子订单信息
            //                                                string sql1 = " select count(*) from t_order ord inner join t_order_detail_sessions ordd  on ord.id = ordd.orderId where ord.id = " + Convert.ToInt64(dtCard.Rows[0]["orderId"].ToString()) + " and ordd.inUserCertificateType =7 and ordd.inUserCardNo = '" + cardId + "' ";
            //                                                object obj = sqliteHelper.ExecuteScalar(sql1);
            //                                                if (obj != null && !string.IsNullOrEmpty(obj.ToString()) && Convert.ToInt32(obj.ToString()) > 0)
            //                                                {
            //                                                    StringBuilder strSql = new StringBuilder();
            //                                                    strSql.Append(" select orderChannel,orderType,ord.actualCount,ord.id as orderId,ordd.id,ordd.inTime ");
            //                                                    strSql.Append(" ,ticketId,1 as personCount,paperCode,inUserCardNo,beginDate,ord.updateTime as orderInTime ");
            //                                                    strSql.Append(" ,venueName,ordd.inStatus,ordd.isPrintTicket,ord.inStatus as mainInStatus,priceType,ordd.isDelete ");
            //                                                    strSql.Append(" ,openTicketName as TicketName,videoName ");
            //                                                    strSql.Append(" from t_order_detail_sessions ordd ");
            //                                                    strSql.Append(" inner join t_order ord on ord.id = ordd.orderId ");
            //                                                    strSql.Append(" inner join t_ticket tic on tic.id = ordd.ticketId ");
            //                                                    strSql.Append(" inner join t_sessions ts on ts.id = ordd.sessionsId ");
            //                                                    strSql.Append(" inner join t_session_video tsv on ts.sessionsVideoId = tsv.id ");
            //                                                    strSql.Append(" inner join t_venue ven on tic.venueId = ven.id ");
            //                                                    strSql.Append(" inner join t_venue_device vendev on vendev.venueId = ven.id WHERE 1=1 ");
            //                                                    strSql.Append(" and deviceCode = '" + deviceCode + "' ");
            //                                                    strSql.Append(" and tic.venueId = '" + venueId + "' ");
            //                                                    strSql.Append(" and ord.id = '" + dtCard.Rows[0]["orderId"].ToString() + "' ");
            //                                                    strSql.Append(" and ord.payStatus = 1 ");  //已支付的订单
            //                                                    strSql.Append(" and ordd.beginDate <= " + TextureHelper.DateToInt(DateTime.Now.Date) + " and endDate >=  " + TextureHelper.DateToInt(DateTime.Now.Date) + " ");
            //                                                    strSql.Append(" and ordd.refundStatus in (0,2,6,7) ");
            //                                                    strSql.Append(" and ordd.orderDetailStatus <> 0 ");
            //                                                    strSql.Append(" and ordd.beginTime >= " + beforeTime);
            //                                                    strSql.Append(" and ordd.endTime <= " + sessionsEndTime);
            //                                                    //strSql.Append(" and " + (TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks)) >= beforeTime));
            //                                                    //strSql.Append(" and " + (TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks)) <= sessionsEndTime));
            //                                                    strSql.Append(" group by ordd.id ");

            //                                                    DataTable dtOrder = sqliteHelper.ExecuteDataTable(strSql.ToString());
            //                                                    if (dtOrder != null && dtOrder.Rows.Count > 0)
            //                                                    {
            //                                                        if (!dtOrder.Columns.Contains("ListId"))
            //                                                            dtOrder.Columns.Add("ListId", typeof(string));
            //                                                        DataTable dtTem = dtOrder.Clone();

            //                                                        #region 按照票价类型进行拆分子订单数据
            //                                                        Dictionary<long, int> dic = new Dictionary<long, int>(); //根据票价类型统计票数
            //                                                        foreach (DataRow dr in dtOrder.Rows)
            //                                                        {
            //                                                            long ticketId = Convert.ToInt64(dr["ticketId"].ToString());
            //                                                            if (ticketId != 279)
            //                                                            {
            //                                                                if (!dic.ContainsKey(ticketId))
            //                                                                    dic.Add(ticketId, Convert.ToInt32(dr["personCount"].ToString()));
            //                                                                else
            //                                                                    dic[ticketId] = dic[ticketId] + Convert.ToInt32(dr["personCount"].ToString());  //dic.Add(ticketId, Convert.ToInt32(dr["personCount"].ToString()) + dic[ticketId]);
            //                                                            }
            //                                                        }
            //                                                        #endregion

            //                                                        #region 根据票价类型重新整合对应的子订单总数和Id集合

            //                                                        foreach (long key in dic.Keys)
            //                                                        {
            //                                                            DataRow[] rows = dtOrder.Select(" ticketId =" + key + "");
            //                                                            DataRow drTem = rows[0];
            //                                                            StringBuilder sbListId = new StringBuilder();
            //                                                            foreach (DataRow dr in rows)
            //                                                            {
            //                                                                long id = Convert.ToInt64(dr["id"].ToString());
            //                                                                sbListId.Append(id + ",");
            //                                                            }
            //                                                            drTem["ListId"] = sbListId.ToString().TrimEnd(',');
            //                                                            drTem["personCount"] = rows.Length;
            //                                                            drTem["InUserCardNo"] = cardId;
            //                                                            dtTem.ImportRow(drTem);
            //                                                        }

            //                                                        #endregion

            //                                                        DataView dv = dtTem.DefaultView;
            //                                                        dv.Sort = " personCount DESC ";
            //                                                        listCheckBM = OrderCheckBM.FromTable(dv.ToTable());
            //                                                        crbm.CheckStatus = DevCheckStatus.成功;
            //                                                        crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                                                        crbm.ListCheckBM = listCheckBM;
            //                                                        crbm.InUserCardNo = cardId;
            //                                                        cr.Result = crbm;
            //                                                        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                                        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                                        return flag;
            //                                                    }
            //                                                }
            //                                                else
            //                                                {
            //                                                    #region 当前身份证不是领队身份证

            //                                                    crbm.CheckStatus = DevCheckStatus.不是领队;
            //                                                    crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                                                    crbm.InUserCardNo = cardId;
            //                                                    cr.Result = crbm;
            //                                                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                                    #endregion
            //                                                }
            //                                                #endregion
            //                                            }
            //                                            else
            //                                            {
            //                                                #region 订单已入馆

            //                                                StringBuilder strSql = new StringBuilder();
            //                                                strSql.Append(" select orderChannel,orderType,ord.actualCount,ord.id as orderId,ordd.id,ordd.inTime ");
            //                                                strSql.Append(" ,ticketId,1 as personCount,paperCode,inUserCardNo,beginDate,ord.updateTime as orderInTime ");
            //                                                strSql.Append(" ,venueName,ordd.inStatus,ordd.isPrintTicket,ord.inStatus as mainInStatus,priceType,ordd.isDelete ");
            //                                                strSql.Append(" ,openTicketName as TicketName,videoName ");
            //                                                strSql.Append(" from t_order_detail_sessions ordd ");
            //                                                strSql.Append(" inner join t_order ord on ord.id = ordd.orderId ");
            //                                                strSql.Append(" inner join t_ticket tic on tic.id = ordd.ticketId ");
            //                                                strSql.Append(" inner join t_sessions ts on ts.id = ordd.sessionsId ");
            //                                                strSql.Append(" inner join t_session_video tsv on ts.sessionsVideoId = tsv.id ");
            //                                                strSql.Append(" inner join t_venue ven on tic.venueId = ven.id ");
            //                                                strSql.Append(" inner join t_venue_device vendev on vendev.venueId = ven.id WHERE 1=1 ");
            //                                                strSql.Append(" and deviceCode = '" + deviceCode + "' ");
            //                                                strSql.Append(" and tic.venueId = '" + venueId + "' ");
            //                                                strSql.Append(" and ord.id = '" + dtCard.Rows[0]["orderId"].ToString() + "' ");
            //                                                strSql.Append(" and ord.payStatus = 1 ");  //已支付的订单
            //                                                strSql.Append(" and ordd.beginDate <= " + TextureHelper.DateToInt(DateTime.Now.Date) + " and endDate >=  " + TextureHelper.DateToInt(DateTime.Now.Date) + " ");
            //                                                strSql.Append(" and ordd.refundStatus in (0,2,6,7) ");
            //                                                strSql.Append(" and ordd.orderDetailStatus <> 0 ");
            //                                                strSql.Append(" and ordd.beginTime >= " + beforeTime);
            //                                                strSql.Append(" and ordd.endTime <= " + sessionsEndTime);
            //                                                //strSql.Append(" and " + (TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks)) >= beforeTime));
            //                                                //strSql.Append(" and " + (TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks)) <= sessionsEndTime));
            //                                                strSql.Append(" group by ordd.id ");

            //                                                DataTable dtOrder = sqliteHelper.ExecuteDataTable(strSql.ToString());
            //                                                if (dtOrder != null && dtOrder.Rows.Count > 0)
            //                                                {
            //                                                    if (!dtOrder.Columns.Contains("ListId"))
            //                                                        dtOrder.Columns.Add("ListId", typeof(string));
            //                                                    DataTable dtTem = dtOrder.Clone();

            //                                                    #region 按照票价类型进行拆分子订单数据
            //                                                    Dictionary<long, int> dic = new Dictionary<long, int>(); //根据票价类型统计票数
            //                                                    foreach (DataRow dr in dtOrder.Rows)
            //                                                    {
            //                                                        long ticketId = Convert.ToInt64(dr["ticketId"].ToString());
            //                                                        if (ticketId != 279)
            //                                                        {
            //                                                            if (!dic.ContainsKey(ticketId))
            //                                                                dic.Add(ticketId, Convert.ToInt32(dr["personCount"].ToString()));
            //                                                            else
            //                                                                dic[ticketId] = dic[ticketId] + Convert.ToInt32(dr["personCount"].ToString());  //dic.Add(ticketId, Convert.ToInt32(dr["personCount"].ToString()) + dic[ticketId]);
            //                                                        }
            //                                                    }
            //                                                    #endregion

            //                                                    #region 根据票价类型重新整合对应的子订单总数和Id集合

            //                                                    foreach (long key in dic.Keys)
            //                                                    {
            //                                                        DataRow[] rows = dtOrder.Select(" ticketId =" + key + "");
            //                                                        DataRow drTem = rows[0];
            //                                                        StringBuilder sbListId = new StringBuilder();
            //                                                        foreach (DataRow dr in rows)
            //                                                        {
            //                                                            long id = Convert.ToInt64(dr["id"].ToString());
            //                                                            sbListId.Append(id + ",");
            //                                                        }
            //                                                        drTem["ListId"] = sbListId.ToString().TrimEnd(',');
            //                                                        drTem["personCount"] = rows.Length;
            //                                                        drTem["InUserCardNo"] = cardId;
            //                                                        dtTem.ImportRow(drTem);
            //                                                    }

            //                                                    #endregion

            //                                                    DataView dv = dtTem.DefaultView;
            //                                                    dv.Sort = " personCount DESC ";
            //                                                    listCheckBM = OrderCheckBM.FromTable(dv.ToTable());
            //                                                    crbm.CheckStatus = DevCheckStatus.已入馆;
            //                                                    crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                                                    crbm.ListCheckBM = listCheckBM;
            //                                                    crbm.InUserCardNo = cardId;
            //                                                    cr.Result = crbm;
            //                                                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                                }
            //                                                //listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                                //crbm.ListCheckBM = listCheckBM;
            //                                                //crbm.CheckStatus = DevCheckStatus.已入馆;
            //                                                //crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                                                //crbm.InUserCardNo = cardId;
            //                                                //cr.Result = crbm;
            //                                                //cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                                //cr.Type = Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                                #endregion
            //                                            }
            //                                        }
            //                                        else
            //                                        {
            //                                            #region 订单未入馆则需判断当前刷卡人是否是导游领队

            //                                            #region 验证当前身份证为领队身份证获取该主订单下对应的所有属于该场馆的子订单信息
            //                                            string sql1 = " select count(*) from t_order ord inner join t_order_detail_sessions ordd  on ord.id = ordd.orderId where ord.id = " + Convert.ToInt64(dtCard.Rows[0]["orderId"].ToString()) + " and ordd.inUserCertificateType =7  and ordd.inUserCardNo = '" + cardId + "'";
            //                                            object obj = sqliteHelper.ExecuteScalar(sql1);
            //                                            if (obj != null && !string.IsNullOrEmpty(obj.ToString()) && Convert.ToInt32(obj.ToString()) > 0)
            //                                            {
            //                                                StringBuilder strSql = new StringBuilder();
            //                                                strSql.Append(" select orderChannel,orderType,ord.actualCount,ord.id as orderId,ordd.id,ordd.inTime ");
            //                                                strSql.Append(" ,ticketId,1 as personCount,paperCode,inUserCardNo,beginDate,ord.updateTime as orderInTime ");
            //                                                strSql.Append(" ,venueName,ordd.inStatus,ordd.isPrintTicket,ord.inStatus as mainInStatus,priceType,ordd.isDelete ");
            //                                                strSql.Append(" ,openTicketName as TicketName,videoName ");
            //                                                strSql.Append(" from t_order_detail_sessions ordd ");
            //                                                strSql.Append(" inner join t_order ord on ord.id = ordd.orderId ");
            //                                                strSql.Append(" inner join t_ticket tic on tic.id = ordd.ticketId ");
            //                                                strSql.Append(" inner join t_sessions ts on ts.id = ordd.sessionsId ");
            //                                                strSql.Append(" inner join t_session_video tsv on ts.sessionsVideoId = tsv.id ");
            //                                                strSql.Append(" inner join t_venue ven on tic.venueId = ven.id ");
            //                                                strSql.Append(" inner join t_venue_device vendev on vendev.venueId = ven.id WHERE 1=1  ");
            //                                                strSql.Append(" and deviceCode = '" + deviceCode + "' ");
            //                                                strSql.Append(" and tic.venueId = '" + venueId + "' ");
            //                                                strSql.Append(" and ord.id = '" + dtCard.Rows[0]["orderId"].ToString() + "' ");
            //                                                strSql.Append(" and ord.payStatus = 1 ");  //已支付的订单
            //                                                strSql.Append(" and ordd.refundStatus in (0,2,6,7) ");
            //                                                strSql.Append(" and ordd.orderDetailStatus <> 0 ");
            //                                                strSql.Append(" and ordd.beginDate <= " + TextureHelper.DateToInt(DateTime.Now.Date) + " and endDate >=  " + TextureHelper.DateToInt(DateTime.Now.Date) + " ");
            //                                                strSql.Append(" and ordd.beginTime >= " + beforeTime);
            //                                                strSql.Append(" and ordd.endTime <= " + sessionsEndTime);

            //                                                //strSql.Append(" and " + (TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks)) >= beforeTime));
            //                                                //strSql.Append(" and " + (TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks)) <= sessionsEndTime));
            //                                                //strSql.Append(" and " + (TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks)) >= beforeTime));
            //                                                //strSql.Append(" and " + (TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks)) <= sessionsEndTime));
            //                                                strSql.Append(" group by ordd.id ");

            //                                                DataTable dtOrder = sqliteHelper.ExecuteDataTable(strSql.ToString());
            //                                                if (dtOrder != null && dtOrder.Rows.Count > 0)
            //                                                {
            //                                                    if (!dtOrder.Columns.Contains("ListId"))
            //                                                        dtOrder.Columns.Add("ListId", typeof(string));
            //                                                    DataTable dtTem = dtOrder.Clone();

            //                                                    #region 按照票价类型进行拆分子订单数据
            //                                                    Dictionary<long, int> dic = new Dictionary<long, int>(); //根据票价类型统计票数
            //                                                    foreach (DataRow dr in dtOrder.Rows)
            //                                                    {
            //                                                        long ticketId = Convert.ToInt64(dr["ticketId"].ToString());
            //                                                        if (ticketId != 279)
            //                                                        {
            //                                                            if (!dic.ContainsKey(ticketId))
            //                                                                dic.Add(ticketId, Convert.ToInt32(dr["personCount"].ToString()));
            //                                                            else
            //                                                                dic[ticketId] = dic[ticketId] + Convert.ToInt32(dr["personCount"].ToString());   //dic.Add(ticketId, Convert.ToInt32(dr["personCount"].ToString()) + dic[ticketId]);
            //                                                        }
            //                                                    }
            //                                                    #endregion

            //                                                    #region 根据票价类型重新整合对应的子订单总数和Id集合

            //                                                    foreach (long key in dic.Keys)
            //                                                    {
            //                                                        DataRow[] rows = dtOrder.Select(" ticketId =" + key + "");
            //                                                        DataRow drTem = rows[0];
            //                                                        StringBuilder sbListId = new StringBuilder();
            //                                                        foreach (DataRow dr in rows)
            //                                                        {
            //                                                            long id = Convert.ToInt64(dr["id"].ToString());
            //                                                            sbListId.Append(id + ",");
            //                                                        }

            //                                                        drTem["ListId"] = sbListId.ToString().TrimEnd(',');
            //                                                        drTem["personCount"] = rows.Length;
            //                                                        drTem["InUserCardNo"] = cardId;
            //                                                        dtTem.ImportRow(drTem);
            //                                                    }

            //                                                    #endregion

            //                                                    DataView dv = dtTem.DefaultView;
            //                                                    dv.Sort = " personCount DESC ";
            //                                                    listCheckBM = OrderCheckBM.FromTable(dv.ToTable());
            //                                                    crbm.CheckStatus = DevCheckStatus.成功;
            //                                                    crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                                                    crbm.ListCheckBM = listCheckBM;
            //                                                    crbm.InUserCardNo = cardId;
            //                                                    cr.Result = crbm;
            //                                                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                                    //领队子订单独立更新操作2017-01-17 17:13
            //                                                    long inTime = TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            //                                                    UpdateLeaderOrderDetailSessions(Convert.ToInt64(orderDetailId), inTime);

            //                                                    return flag;
            //                                                }
            //                                            }
            //                                            else
            //                                            {
            //                                                #region 当前身份证不是领队身份证

            //                                                crbm.CheckStatus = DevCheckStatus.不是领队;
            //                                                crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                                                crbm.InUserCardNo = cardId;
            //                                                cr.Result = crbm;
            //                                                cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                                cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                                #endregion
            //                                            }
            //                                            #endregion

            //                                            #endregion
            //                                        }
            //                                    }
            //                                    #endregion

            //                                    #region 2.该订单的主订单为散客订单
            //                                    if (orderType == "1")
            //                                    {
            //                                        crbm.OrderType = Yl.Cg.Model.OrderTypeEnum.散客;
            //                                        dtCard.Rows[0]["ListId"] = dtCard.Rows[0]["id"];
            //                                        if (dtCard.Rows[0]["isPrintTicket"].ToString() == "1") //门票已兑换票纸，身份证已无效
            //                                        {
            //                                            crbm.CheckStatus = DevCheckStatus.已兑换;
            //                                            crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                                            cr.Result = crbm;
            //                                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                        }
            //                                        else if (dtCard.Rows[0]["inStatus"].ToString() == "1")
            //                                        {
            //                                            //若验票时间在1分钟之内仍然有效需要进行验票成功开闸 1分钟有效改为10秒（2016-6-6 11:07 贾增义）
            //                                            if (Convert.ToInt64(dtCard.Rows[0]["inTime"].ToString()) >= TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.AddSeconds(-3).ToString("yyyy-MM-dd HH:mm:ss"))))
            //                                            {
            //                                                listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                                crbm.CheckStatus = DevCheckStatus.成功;
            //                                                crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                                                crbm.ListCheckBM = listCheckBM;
            //                                                crbm.InUserCardNo = cardId;
            //                                                cr.Result = crbm;
            //                                                cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                                cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                            }
            //                                            else
            //                                            {
            //                                                listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                                crbm.ListCheckBM = listCheckBM;
            //                                                crbm.CheckStatus = DevCheckStatus.已入馆;
            //                                                crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                                                crbm.InUserCardNo = cardId;
            //                                                cr.Result = crbm;
            //                                                cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Error;
            //                                                cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                                            }
            //                                        }
            //                                        else  //验票成功
            //                                        {
            //                                            listCheckBM = OrderCheckBM.FromTable(dtCard);
            //                                            crbm.CheckStatus = DevCheckStatus.成功;
            //                                            crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                                            crbm.ListCheckBM = listCheckBM;
            //                                            crbm.InUserCardNo = cardId;
            //                                            cr.Result = crbm;
            //                                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;

            //                                            return flag;
            //                                        }
            //                                    }

            //                                    flag = true;

            //                                    #endregion
            //                                }

            //                                break;
            //                            }

            //                            #endregion
            //                        }
            //                        if (!bl)
            //                        {
            //                            string orderType = dtAll.Rows[dtAll.Rows.Count - 1]["orderType"].ToString();
            //                            if (orderType == "2")
            //                                crbm.OrderType = Yl.Cg.Model.OrderTypeEnum.团队;
            //                            else
            //                                crbm.OrderType = Yl.Cg.Model.OrderTypeEnum.散客;
            //                            //若符合条件的数据不存在且当前时间在最晚结束验票时间之前则说明当前还未到验票时间
            //                            if (TextureHelper.TimeToInt(new TimeSpan(Convert.ToDateTime(DateTime.Now.ToString("HH:mm:ss")).Ticks)) < Convert.ToInt32(dtAll.Rows[0]["endTime"].ToString()))
            //                                crbm.CheckStatus = DevCheckStatus.未到检票时间;

            //                            else
            //                                crbm.CheckStatus = DevCheckStatus.检票时间已过;

            //                            crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                            crbm.InUserCardNo = cardId;
            //                            cr.Result = crbm;
            //                            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                        }

            //                        break;
            //                        #endregion
            //                    }

            //                    #endregion
            //                }
            //                else
            //                {
            //                    crbm.CheckStatus = DevCheckStatus.非本场票;
            //                    crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //                    crbm.InUserCardNo = cardId;
            //                    cr.Result = crbm;
            //                    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //                    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //                }
            //            }
            //        }
            //        if (!flag)
            //        {
            //            crbm.CheckStatus = DevCheckStatus.非今日票;
            //            crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //            crbm.InUserCardNo = cardId;
            //            cr.Result = crbm;
            //            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //        }

            //        if (!bl)
            //        {
            //            crbm.CheckStatus = DevCheckStatus.非本场票;
            //            crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //            crbm.InUserCardNo = cardId;
            //            cr.Result = crbm;
            //            cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //            cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //        }
            //    }
            //    else
            //    {
            //        crbm.CheckStatus = DevCheckStatus.非本场馆票;
            //        crbm.TicketOrderType = TicketOrderTypeEnum.场次票;
            //        crbm.InUserCardNo = cardId;
            //        cr.Result = crbm;
            //        cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckPaper_Success;
            //        cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Info;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    cr.Error = ex.Message;
            //    cr.Code = LogCodeEnumCG.Ticket_Check_GetCheckBreath_Exception;
            //    cr.Type = Yl.Ticket5.Common40.Log.LogTypeEnum.Error;
            //    cr.Exception = ex;
            //    flag = false;
            //}

            ////特种票检票成功则需要更新特种票的入馆状态及入馆次数信息
            ////if (crbm.CheckStatus == DevCheckStatus.成功 && isSpecialTicket == 1)
            ////{
            ////    //更新对应特种票的入馆状态
            ////    UpdateOrderLongSpecialInStatus(Convert.ToInt64(orderId), Convert.ToInt64(orderDetailId), TextureHelper.DateTimeToInt(DateTime.Now), "C");
            ////}

            return flag;
        }

        /// <summary>
        /// 功能说明：根据身份证验票结果更新主订单及对应的子订单状态及验票记录信息
        /// 创建人：ys
        /// 创建日期：2016-05-02 10：25
        /// </summary>
        /// <param name="crbm"></param>
        /// <param name="cr"></param>
        private void SaveCheckResultByCard(CheckResultBM crbm, CallResult<CheckResultBM> cr, long deviceId, string userId, Dictionary<string, string> dic)
        {
            //try
            //{
            //    if (crbm.CheckStatus == DevCheckStatus.成功)
            //    {
            //        #region 验票成功

            //        List<OrderCheckBM> listCheckBM = cr.Result.ListCheckBM;
            //        if (listCheckBM != null && listCheckBM.Count > 0)
            //        {
            //            foreach (OrderCheckBM checkBM in listCheckBM)
            //            {
            //                List<long> listId = new List<long>();
            //                string[] idArr = checkBM.ListId.Split(',');
            //                foreach (string id in idArr)
            //                {
            //                    listId.Add(Convert.ToInt64(id));

            //                    if (!dic.ContainsKey("detailId"))
            //                        dic.Add("detailId", id);
            //                    else
            //                        dic["detailId"] = id;

            //                    string inTime = TextureHelper.DateTimeToInt(DateTime.Now).ToString();
            //                    if (!dic.ContainsKey("inTime"))
            //                        dic.Add("inTime", inTime);
            //                    else
            //                        dic["inTime"] = inTime;

            //                    CheckOutLineCard(checkBM.InUserCardNo.ToString(), "2", ((int)cr.Result.OrderType).ToString(), "api/zk_check/ticket/get_outline_check", TextureHelper.ToJson<Dictionary<string, string>>(dic),id);
            //                }
            //                UpdateOrderStatus(Convert.ToInt64(checkBM.OrderId), listId, (int)cr.Result.OrderType, checkBM.InUserCardNo.ToString(), 0, deviceId, userId, crbm.TicketOrderType);
            //            }
            //        }

            //        #endregion
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
        }

        #region 更新离线数据的检票状态

        /// <summary>
        /// 功能说明：更新验票结果及对应的主订单及子订单状态，验票日期信息
        /// </summary>
        /// <param name="orderId">主订单编号</param>
        /// <param name="listId">子订单编号List</param>
        /// <param name="orderType">验票类型【1-票纸 2-身份证】</param>
        /// <param name="checkCode">验票类型对应的票纸编号或者身份证号</param>
        /// <param name="result">验票结果【0-成功 1-失败】</param>
        /// <param name="deviceId">当前闸机验票设备编号</param>
        /// <returns></returns>
        private bool UpdateOrderStatus(long orderId, List<long> listId, int orderType, string checkCode, int result, long deviceId, string userId, TicketOrderTypeEnum TicketOrderType)
        {
            bool flag = false;
            //try
            //{
            //    if (result == 0)  //验票成功则更新相应的订单入馆状态
            //    {
            //        ////先判定当前票纸是否已经验票入馆
            //        //string sqlCheck = " select count(*) from t_order where id = " + orderId + " and inStatus = 0 ";
            //        //object obj = sqliteHelper.ExecuteScalar(sqlCheck);
            //        //if(obj != null && !string.IsNullOrEmpty(obj.ToString()) && obj.ToString() != "0")
            //        //{
            //        long inTime = TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            //        //1.验票结果存入验票表中
            //        //InsertCheckRecord(orderId, listId, orderType, checkCode, result, deviceId, inTime, userId, (int)TicketOrderType);

            //        //2.更新主订单入馆状态为已入馆
            //        //判定当前主订单下的子单是否已全部入馆
            //        string sqlOrder = "";
            //        bool bl = CheckOrderCompleted(orderId.ToString(), listId.Count);
            //        if (bl)
            //            sqlOrder = " update t_order set inStatus = 1,status = 2,updateTime = " + inTime + ",updateUser = 0 where id = " + orderId + "";
            //        else
            //            sqlOrder = " update t_order set inStatus = 1,updateTime = " + inTime + ",updateUser = 0 where id = " + orderId + "";
            //        int row0 = sqliteHelper.ExecuteNonQuery(sqlOrder);

            //        switch (TicketOrderType)
            //        {
            //            case TicketOrderTypeEnum.普通门票:
            //                flag = UpdateOrderDetail(orderId, listId, orderType, checkCode, result, deviceId);
            //                break;
            //            case TicketOrderTypeEnum.计次票:
            //                flag = UpdateOrderDetailLong(orderId, listId, orderType, checkCode, result, deviceId);
            //                break;
            //            case TicketOrderTypeEnum.场次票:
            //                flag = UpdateOrderDetailSessions(orderId, listId, orderType, checkCode, result, deviceId);
            //                break;
            //            case TicketOrderTypeEnum.工作证:

            //                break;
            //            case TicketOrderTypeEnum.凭单:

            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{

            //}


            return flag;
        }

        /// <summary>
        /// 功能说明：更新主订单及普通子订单的验票结果信息
        /// 创建人：ys
        /// 创建日期：2016-04-29 22：55
        /// </summary>
        /// <param name="orderId">主订单ID</param>
        /// <param name="id">普通子订单Id</param>
        /// <param name="orderType">验票类型【1-票纸 2-身份证】</param>
        /// <param name="checkCode">验票类型对应的票纸编号或者身份证号</param>
        /// <param name="result">验票结果【1-成功 2-失败】</param>
        /// <param name="deviceId">当前闸机验票设备编号</param>
        /// <returns></returns>
        private bool UpdateOrderDetail(long orderId, List<long> listId, int orderType, string checkCode, int result, long deviceId)
        {
            //try
            //{
            //    long inTime = TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            //    if (listId != null)
            //    {
            //        int successCount = 0;
            //        foreach (long id in listId)
            //        {
            //            //1.先判定当前票纸是否已经验票入馆
            //            string sqlCheck = " select count(*) from t_order_detail where id = " + id + " and inStatus = 0 ";
            //            object obj = sqliteHelper.ExecuteScalar(sqlCheck);
            //            if (obj != null && !string.IsNullOrEmpty(obj.ToString()) && obj.ToString() != "0")
            //            {
            //                //2.更新普通订单子订单中实际入馆日期和入馆状态和订单状态
            //                string sqlOrderDetail = " update t_order_detail set inTime  = " + inTime + ",orderDetailStatus = 2,inStatus = 1,updateTime = " + inTime + ",updateUser = 0 where id = " + id + " ";
            //                int row1 = sqliteHelper.ExecuteNonQuery(sqlOrderDetail);
            //                successCount += row1;
            //            }
            //        }
            //        if (successCount > 0)
            //            return true;
            //        else
            //            return false;
            //    }
            //    else
            //        return false;
            //}
            //catch (Exception ex)
            //{
            //    return false;
            //}
            return false;
        }

        /// <summary>
        /// 功能说明：更新主订单及计次子订单的验票结果信息【计次订单需要在可用次数减一操作】
        /// 创建人：ys
        /// 创建日期：2016-04-29 22：55
        /// </summary>
        /// <param name="orderId">主订单ID</param>
        /// <param name="id">计次订单Id</param>
        /// <param name="orderType">验票类型【1-票纸 2-身份证】</param>
        /// <param name="checkCode">验票类型对应的票纸编号或者身份证号</param>
        /// <param name="result">验票结果【1-成功 2-失败】</param>
        /// <param name="deviceId">当前闸机验票设备编号</param>
        /// <returns></returns>
        private bool UpdateOrderDetailLong(long orderId, List<long> listId, int orderType, string checkCode, int result, long deviceId)
        {
            //try
            //{
            //    long inTime = TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            //    if (listId != null)
            //    {
            //        int successCount = 0;
            //        ////1.验票结果存入验票表中
            //        //InsertCheckRecord(orderId, listId, orderType, checkCode, result, deviceId, inTime);
            //        foreach (long id in listId)
            //        {
            //            //1.先判定当前票纸是否已经验票入馆
            //            string sqlCheck = " select count(*) from t_order_detail_long where id = " + id + "";
            //            object obj = sqliteHelper.ExecuteScalar(sqlCheck);
            //            if (obj != null && !string.IsNullOrEmpty(obj.ToString()) && obj.ToString() != "0")
            //            {
            //                //2.更新普通订单子订单中实际入馆日期和入馆状态和订单状态
            //                string sqlOrderDetail = " update t_order_detail_long set inCount = (inCount+1),inTime  = " + inTime + ",orderDetailStatus = 2,inStatus = 1,updateTime = " + inTime + ",updateUser = 0 where id = " + id + " ";
            //                int row1 = sqliteHelper.ExecuteNonQuery(sqlOrderDetail);
            //                successCount += row1;
            //            }
            //        }
            //        if (successCount > 0)
            //            return true;
            //        else
            //            return false;
            //    }
            //    else
            //        return false;
            //}
            //catch (Exception ex)
            //{
            //    return false;
            //}
            return false;
        }

        /// <summary>
        /// 功能说明：更新主订单及场次子订单的验票结果信息
        /// 创建人：ys
        /// 创建日期：2016-04-29 22：55
        /// </summary>
        /// <param name="orderId">主订单ID</param>
        /// <param name="id">场次订单Id</param>
        /// <param name="orderType">验票类型【1-票纸 2-身份证】</param>
        /// <param name="result">验票结果【1-成功 2-失败】</param>
        /// <param name="deviceId">当前闸机验票设备编号</param>
        /// <returns></returns>
        private bool UpdateOrderDetailSessions(long orderId, List<long> listId, int orderType, string checkCode, int result, long deviceId)
        {
            ////var tran = sqliteHelper.BeginTransaction();
            //try
            //{
            //    long inTime = TextureHelper.DateTimeToInt(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            //    if (listId != null)
            //    {
            //        int successCount = 0;
            //        ////1.验票结果存入验票表中
            //        //InsertCheckRecord(orderId, listId, orderType, checkCode, result, deviceId, inTime);
            //        foreach (long id in listId)
            //        {
            //            //11.先判定当前票纸是否已经验票入馆
            //            string sqlCheck = " select count(*) from t_order_detail_sessions where id = " + id + " and inStatus = 0 ";
            //            object obj = sqliteHelper.ExecuteScalar(sqlCheck);
            //            if (obj != null && !string.IsNullOrEmpty(obj.ToString()) && obj.ToString() != "0")
            //            {
            //                //1.2.更新普通订单子订单中实际入馆日期和入馆状态和订单状态
            //                string sqlOrderDetail = " update t_order_detail_sessions set inTime  = " + inTime + ",orderDetailStatus = 2,inStatus = 1,updateTime = " + inTime + ",updateUser = 0 where id = " + id + " ";
            //                int row1 = sqliteHelper.ExecuteNonQuery(sqlOrderDetail);
            //                successCount += row1;
            //            }
            //        }
            //        if (successCount > 0)
            //            return true;
            //        else
            //            return false;
            //    }
            //    else
            //        return false;
            //}
            //catch (Exception ex)
            //{
            //    return false;
            //} 
            return false;
        }

        /// <summary>
        /// 功能说明：判断主订单下边的子订单是否均已入馆完成
        /// 创建人：ys
        /// 创建日期：2016-06-01 13:48
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private bool CheckOrderCompleted(string orderId, int count)
        {
            //bool flag = false;

            //StringBuilder sbSql = new StringBuilder();
            //sbSql.Append(" select sum(reCount) from (");
            //sbSql.Append(" select count(*) as reCount from t_order_detail where orderId = " + orderId + " and inStatus = 0 and payStatus =1 and refundStatus in (0,2,6,7) and orderDetailStatus = 1 ");
            //sbSql.Append(" union all ");
            //sbSql.Append(" select count(*) as reCount from t_order_detail_long where orderId = " + orderId + " and inStatus = 0 and payStatus =1 and refundStatus in (0,2,6,7) and orderDetailStatus = 1 ");
            //sbSql.Append(" union all ");
            //sbSql.Append(" select count(*) as reCount from t_order_detail_sessions where orderId = " + orderId + " and inStatus = 0 and payStatus =1 and refundStatus in (0,2,6,7) and orderDetailStatus = 1  ) as t ");

            //object obj = sqliteHelper.ExecuteScalar(sbSql.ToString());
            //if (obj != null && !string.IsNullOrEmpty(obj.ToString()) && obj.ToString() != "0")
            //{
            //    if (Convert.ToInt32(obj.ToString()) == count)
            //        flag = true;
            //}
            //else
            //    flag = true;

            //return flag;
            return false;
        }
        #endregion

        #endregion

        //#region 单独更新领队人子订单信息

        //private bool UpdateLeaderOrderDetail(long id, long inTime)
        //{
        //    string sql = "  update t_order_detail set inTime  = " + inTime + ",orderDetailStatus = 2,inStatus = 1,updateTime = " + inTime + ",updateUser = 0 where id = " + id + " ";
        //    int rows = sqliteHelper.ExecuteNonQuery(sql);
        //    return rows > 0;
        //}

        //private bool UpdateLeaderOrderDetailSessions(long id, long inTime)
        //{
        //    string sql = "  update t_order_detail_sessions set inTime  = " + inTime + ",orderDetailStatus = 2,inStatus = 1,updateTime = " + inTime + ",updateUser = 0 where id = " + id + " ";
        //    int rows = sqliteHelper.ExecuteNonQuery(sql);
        //    return rows > 0;
        //}

        //#endregion

        #endregion


        #endregion --------------------------------------身份证检票end-------------------------------------------


        #region ----------------------------------二维码检票start--------------------------------------------
        /// <summary>
        /// 功能：二维码检票
        /// 作者: 贾增义
        /// 时间：2016-4-24 17:27
        /// </summary>
        /// <param name="QRCode"></param>
        public void TicketCheckByQRCode(string QRCode)
        {
            try
            {
                QRCode = QRCode.Trim('\0').Trim();

                #region 二维码检票逻辑 参数组合
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("deviceCode", Config.Instance.DeviceCode.ToString());
                dic.Add("deviceId", Config.Instance.DeviceId.ToString());
                dic.Add("checkticketlist", Config.Instance.checkticketlist.ToString());
                dic.Add("inspectticketlist", Config.Instance.inspectticketlist.ToString());
                dic.Add("paperScanner", QRCode);
                dic.Add("userId", ConfigurationManager.AppSettings["UserId"].ToString());
                dic.Add("cardId", "");
                dic.Add("paperCode", "");
                dic.Add("palmScanner", "");
                #endregion

                #region 二维码数据有效性验证

                string factor = "";
                string paperInfo = TextureHelper.DecryptByFactor(QRCode, out factor);  //解析出来的二维码信息
                string title = paperInfo.Substring(0, 1);

                if (title == "A" || title == "C")
                {
                    if (Config.is_Online)
                    {
                        PaperCheckResult(dic);// 在线检票
                    }
                    else
                    {
                        ShowCheckResultUC(1, null, "系统已离线，请与工作人员联系！");
                    }
                }
                else
                {
                    ShowCheckResultUC(1, null, "非法票");
                }

                #region   注释

                //    //有效票前缀验证【A-普通门票 B-计次票 C-场次票 D-工作证 E-凭单】
                //    //if (title == "A" || title == "B" || title == "C" || title == "D" || title == "E" || title == "T")
                //    if (title == "A" || title == "B" || title == "C" || title == "D" || title == "E")
                //    {
                //    //if (title == "T")
                //    //{
                //    //    //if ("4,8,10,11,13".ToString().IndexOf(Config.Instance.venueId.ToString()) >= 0)
                //    //    //{
                //    //    //    string orderInfo = paperInfo.Substring(2);
                //    //    //    string[] orderArr = orderInfo.Split('|');
                //    //    //    string spLongPaperCode = orderArr[2];
                //    //    //    CheckResultBM checkResultBM = new CheckResultBM();
                //    //    //    checkResultBM.PaperCode = spLongPaperCode;
                //    //    //    ShowCheckResultUC(1, checkResultBM, "特殊票，请换取影院票");
                //    //    //    Ring(ThreadPlay.Rings.ScanFailed);
                //    //    //    return;
                //    //    //}
                //    //}

                //    if (Config.is_Online)
                //    {
                //        #region 在线检票

                //        PaperCheckResult(dic);

                //        #endregion
                //    }
                //    //else
                //    //{
                //    //    #region 离线检票

                //    //    CheckOutLine(dic);

                //    //    #endregion
                //    //}
                //}
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                Logger.WriteLog("二维码检票报错" + ex.Message);
            }
        }


        #region 离线检票

        /// <summary>
        /// 离线检票
        /// </summary>
        private void CheckOutLine(Dictionary<string, string> dic)
        {
            //string factor = "";
            //string paperInfo = TextureHelper.DecryptByFactor(dic["paperScanner"], out factor);  //解析出来的二维码信息
            //string title = paperInfo.Substring(0, 1);

            //string ticketId = "";
            ////获取当前闸机可检票信息
            //string strTicket = Config.TicketArr;
            //if (!string.IsNullOrEmpty(strTicket))
            //{
            //    dic.Add("inTime", TextureHelper.DateTimeToInt(DateTime.Now).ToString());

            //    #region 离线检票
            //    CheckResultBM checkResultBM = new CheckResultBM();
            //    bool bl = false;
            //    string orderInfo = paperInfo.Substring(2);
            //    string[] orderArr = orderInfo.Split('|');
            //    switch (title)
            //    {
            //        case "A":
            //            #region 普通子订单信息

            //            long detailInTime = Convert.ToInt64(orderArr[5]); //预约入馆时间
            //            string detailPaperCode = orderArr[2];  //票纸编号
            //            string orderType = orderArr[0];  //订单类型
            //            string ticketInfo = orderArr[6]; //订单中的门票信息
            //            string[] ticketArr = ticketInfo.Split('#');
            //            string personCount = orderArr[4];
            //            string venueId = "";
            //            List<OrderCheckBM> listOrderCheckBM = new List<OrderCheckBM>();
            //            for(int i = 0;i<ticketArr.Length;i++)
            //            {
            //                //场馆信息
            //                venueId = ticketArr[i].Split(',')[2];

            //                if (venueId == Config.Instance.venueId)
            //                {
            //                    OrderCheckBM ocbm = new OrderCheckBM();
            //                    ocbm.VenueName = GetVenueNameById(venueId);
            //                    ticketId = ticketArr[i].Split(',')[0];
            //                    //门票信息
            //                    ocbm.TicketId = Convert.ToInt64(ticketId);
            //                    string ticketName = GetTicketNameById(ticketId);
            //                    ocbm.TicketName = ticketName;
            //                    //门票人数信息
            //                    int ocbmPersonCount = Convert.ToInt32(ticketArr[i].Split(',')[1]);
            //                    ocbm.PersonCount = ocbmPersonCount;

            //                    checkResultBM.VenueName = ocbm.VenueName;
            //                    checkResultBM.VenueId = Convert.ToInt64(venueId);

            //                    listOrderCheckBM.Add(ocbm);
            //                }
            //            }
            //            checkResultBM.ListCheckBM = listOrderCheckBM;
            //            checkResultBM.PaperCode = detailPaperCode;
            //            if (checkResultBM.VenueId.ToString() != Config.Instance.venueId)
            //            {
            //                ShowCheckResultUC(1, checkResultBM, "非 本 场 馆 票");
            //                Ring(ThreadPlay.Rings.WrongVenue);
            //                return;
            //            }

            //            try
            //            {
            //                checkResultBM.PersonCount = Convert.ToInt32(personCount);
            //            }
            //            catch(Exception ex)
            //            {
            //                checkResultBM.PersonCount = 1;
            //            }

            //            if (strTicket.IndexOf(ticketId) >= 0)
            //            {
            //                if (detailInTime == TextureHelper.DateToInt(DateTime.Now)) //是今日入馆门票
            //                {
            //                    bl = CheckOutLinePaper(checkResultBM,detailPaperCode, "2", orderType, "api/zk_check/ticket/get_outline_check", TextureHelper.ToJson<Dictionary<string, string>>(dic), false);
            //                }
            //                else
            //                {
            //                    ShowCheckResultUC(1, checkResultBM, "非 今 日 票");
            //                    Ring(ThreadPlay.Rings.ScanFailed);
            //                }   
            //            }
            //            else
            //            {
            //                ShowCheckResultUC(1, checkResultBM, "非 本 场 馆 票");
            //                Ring(ThreadPlay.Rings.WrongVenue);
            //            }                     
            //            #endregion
            //            break;
            //        case "B":
            //            #region 计次票

            //            string orderTypeB = orderArr[0];  //订单类型
            //            long longId = Convert.ToInt64(orderArr[1]);  //子订单编号
            //            int longBeginDate = Convert.ToInt32(orderArr[6]); //有效期起始
            //            int longEndDate = Convert.ToInt32(orderArr[7]);  //有效期结束
            //            //int longBeginTime = Convert.ToInt32(orderArr[8]); //有效期起始时间
            //            //int longEndTime = Convert.ToInt32(orderArr[9]);  //有效期结束时间
            //            string longPaperCode = orderArr[2];//票纸编号
            //            checkResultBM.PaperCode = longPaperCode;
            //            checkResultBM.PersonCount = 1;
            //            string lTicketId = orderArr[10]; //门票ID

            //            //可用次数
            //            int lPersonCount = 1;
            //            if(orderArr.Length > 11)
            //                lPersonCount = Convert.ToInt32(orderArr[11]);

            //            //场馆
            //            string lvenueId = "";
            //            if (orderArr.Length > 12)
            //                lvenueId = orderArr[12];

            //            if (!string.IsNullOrWhiteSpace(lvenueId) && lvenueId != Config.Instance.venueId)
            //            {
            //                ShowCheckResultUC(1, checkResultBM, "非 本 场 馆 票");
            //                Ring(ThreadPlay.Rings.WrongVenue);
            //                return;
            //            }

            //            List<OrderCheckBM> lListOrderCheckBM = new List<OrderCheckBM>();
            //            OrderCheckBM locbm = new OrderCheckBM();
            //            //门票信息
            //            locbm.TicketId = Convert.ToInt64(lTicketId);
            //            string lTicketName = GetTicketNameById(lTicketId);
            //            locbm.TicketName = lTicketName;
            //            //门票人数信息
            //            locbm.PersonCount = lPersonCount;
            //            if (!string.IsNullOrWhiteSpace(lvenueId))
            //            {
            //                //场馆信息
            //                locbm.VenueName = GetVenueNameById(lvenueId);
            //                locbm.VenueId = Convert.ToInt64(lvenueId);
            //                checkResultBM.VenueName = locbm.VenueName;
            //                checkResultBM.VenueId = Convert.ToInt64(lvenueId);
            //            }
            //            lListOrderCheckBM.Add(locbm);
            //            checkResultBM.ListCheckBM = lListOrderCheckBM;

            //            if (strTicket.IndexOf(lTicketId) >= 0)
            //            {
            //                if (longBeginDate <= TextureHelper.DateToInt(DateTime.Now) && longEndDate >= TextureHelper.DateToInt(DateTime.Now)) //是今日入馆门票
            //                {
            //                    bl = CheckOutLinePaper(checkResultBM,longPaperCode, "2", orderTypeB, "api/zk_check/ticket/get_outline_check", TextureHelper.ToJson<Dictionary<string, string>>(dic), false);
            //                }
            //                else
            //                {
            //                    ShowCheckResultUC(1, checkResultBM, "非 今 日 票");
            //                    Ring(ThreadPlay.Rings.ScanFailed);
            //                }  
            //            }
            //            else
            //            {
            //                ShowCheckResultUC(1, checkResultBM, "非 本 场 馆 票");
            //                Ring(ThreadPlay.Rings.WrongVenue);
            //            }


            //            #endregion

            //            break;
            //        case "C":
            //            #region 场次票

            //            long sessionsId = Convert.ToInt64(orderArr[1]);  //子订单编号
            //            int sessionsBeginDate = Convert.ToInt32(orderArr[3]); //有效期起始
            //            int sessionsEndDate = Convert.ToInt32(orderArr[4]);  //有效期结束
            //            int sessionsBeginTime = Convert.ToInt32(orderArr[5]); //有效期起始
            //            int sessionsEndTime = Convert.ToInt32(orderArr[6]);  //有效期结束
            //            string sessionsPaperCode = orderArr[2];//票纸编号
            //            string orderTypeC = orderArr[0];  //订单类型
            //            checkResultBM.PaperCode = sessionsPaperCode;
            //            checkResultBM.PersonCount = 1;
            //            //ticketId = orderArr[7]; //门票ID
            //            int sPersonCount = 1;
            //            if(orderArr.Length > 8)
            //                sPersonCount = Convert.ToInt32(orderArr[8]);

            //            //场馆信息
            //            string sVenueId = orderArr[7];

            //            if (sVenueId != Config.Instance.venueId)
            //            {
            //                ShowCheckResultUC(1, checkResultBM, "非 本 场 馆 票");
            //                Ring(ThreadPlay.Rings.WrongVenue);
            //                return;
            //            }

            //            checkResultBM.VenueName = GetVenueNameById(sVenueId);
            //            checkResultBM.VenueId = Convert.ToInt64(sVenueId);

            //            if(orderArr.Length > 9)
            //            {
            //                string[] sTicketArr = orderArr[9].Split('#');
            //                List < OrderCheckBM > sListOrderCheckBM = new List<OrderCheckBM>();
            //                for (int i = 0; i < sTicketArr.Length; i++)
            //                {
            //                    OrderCheckBM ocbm = new OrderCheckBM();
            //                    ticketId = sTicketArr[i].Split(',')[0];
            //                    //门票信息
            //                    ocbm.TicketId = Convert.ToInt64(ticketId);
            //                    string ticketName = GetTicketNameById(ticketId);
            //                    ocbm.TicketName = ticketName;
            //                    //门票人数信息
            //                    int ocbmPersonCount = Convert.ToInt32(sTicketArr[i].Split(',')[1]);
            //                    ocbm.PersonCount = ocbmPersonCount;

            //                    sListOrderCheckBM.Add(ocbm);
            //                }
            //                checkResultBM.ListCheckBM = sListOrderCheckBM;
            //            }

            //            //if (strTicket.IndexOf(ticketId) >= 0)
            //            //{
            //            int beforeMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["BEFORE_MINUTES"]);
            //            string beginTime = TextureHelper.IntToTime(sessionsBeginTime).ToString();
            //            long tem = Convert.ToDateTime(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd" + " " + beginTime)).AddMinutes(-beforeMinutes).ToString("HH:mm:ss")).Ticks;
            //            int beforeTime = TextureHelper.TimeToInt(new TimeSpan(tem));

            //            if (sessionsBeginDate <= TextureHelper.DateToInt(DateTime.Now) && sessionsEndDate >= TextureHelper.DateToInt(DateTime.Now)) //是今日入馆门票
            //            {
            //                if (TextureHelper.TimeToInt(new TimeSpan(DateTime.Now.Ticks)) < beforeTime)
            //                {
            //                    ShowCheckResultUC(1, checkResultBM, "未 到 检 票 时 间");
            //                    Ring(ThreadPlay.Rings.ScanFailed);
            //                }
            //                else if (TextureHelper.TimeToInt(new TimeSpan(DateTime.Now.Ticks)) > sessionsEndTime)
            //                {
            //                    ShowCheckResultUC(1, checkResultBM, "检 票 时 间 已 过");
            //                    Ring(ThreadPlay.Rings.InvalidDate);
            //                }
            //                else
            //                {
            //                    bl = CheckOutLinePaper(checkResultBM,sessionsPaperCode, "2", orderTypeC, "api/zk_check/ticket/get_outline_check", TextureHelper.ToJson<Dictionary<string, string>>(dic), false);
            //                }
            //            }
            //            else
            //            {
            //                ShowCheckResultUC(1, checkResultBM, "非 今 日 票");
            //                Ring(ThreadPlay.Rings.ScanFailed);
            //            }

            //            #endregion

            //            //}
            //            //else
            //            //    ShowCheckResultUC(1, checkResultBM, "非本场馆票");
            //            break;
            //        case "D":
            //            //crbm.TicketOrderType = TicketOrderTypeEnum.工作证;
            //            #region 工作证

            //            int openType = Convert.ToInt32(orderArr[0]);
            //            int uId = Convert.ToInt32(orderArr[1]);
            //            int empolyeeId = Convert.ToInt32(orderArr[2]);
            //            ticketId = orderArr[3].ToString();
            //            string ticketNameCard = GetTicketNameById(ticketId);
            //            checkResultBM.TicketName = ticketNameCard;
            //            string empersonCount = orderArr[4].ToString();
            //            int validEndDate = 0;  /***********工作证有效期 修改人 ys 修改日期：2016-10-20 09：30************
            //            if (orderArr.Length > 5)
            //                validEndDate = Convert.ToInt32(orderArr[5]);

            //            checkResultBM.PaperCode = empolyeeId.ToString();

            //            string emVenueId = GetVenueIdByTicketId(ticketId);
            //            if (!string.IsNullOrWhiteSpace(emVenueId) && emVenueId != Config.Instance.venueId)
            //            {
            //                ShowCheckResultUC(1, checkResultBM, "非 本 场 馆 票");
            //                Ring(ThreadPlay.Rings.WrongVenue);
            //                return;
            //            }

            //            if (validEndDate == 0 || (validEndDate >= TextureHelper.DateToInt(DateTime.Now))) 
            //            {
            //                checkResultBM.PersonCount = 1;
            //                //ShowCheckResultUC(7, checkResultBM, "");
            //                bl = CheckOutLinePaper(checkResultBM, empolyeeId.ToString(), "2", "1", "api/zk_check/ticket/get_outline_check", TextureHelper.ToJson<Dictionary<string, string>>(dic), true);
            //            }
            //            else
            //            {
            //                ShowCheckResultUC(1, checkResultBM, "工 作 证 已 到 期");
            //                Ring(ThreadPlay.Rings.ScanFailed);
            //            }

            //            #endregion

            //            break;
            //        case "E":

            //            break;
            //    }

            //    #endregion
            //}
            //else
            //{
            //    ShowCheckResultUC(1, null, "当前闸机不可检票");
            //    Ring(ThreadPlay.Rings.ScanFailed);
            //}
        }

        //private bool CheckOutLinePaper(CheckResultBM checkResultBM, string paperCode, string check_type, string orderType, string check_url, string check_content, bool isEmployeeCard)
        //{
        //    #region 门票单离线检票

        //    //try
        //    //{
        //    //    //CheckResultBM checkResultBM = new CheckResultBM();
        //    //    //if (isEmployeeCard)
        //    //    //{
        //    //    //    //2.检票成功，存入本地sqlite库中
        //    //    //    string sql = " insert into check_list(paperCode,check_type,check_url,check_content,is_sync,create_time) values ('" + paperCode + "','" + check_type + "','" + check_url + "','" + check_content + "',0," + TextureHelper.DateTimeToInt(DateTime.Now) + ")";

        //    //    //    int rows = sqliteHelper.ExecuteNonQuery(sql);
        //    //    //    if (rows > 0)
        //    //    //    {
        //    //    //        checkResultBM.PaperCode = paperCode;
        //    //    //        //if (orderType == "1")
        //    //    //        //    ShowCheckResultUC(9, checkResultBM, "");
        //    //    //        //else if (orderType == "2")
        //    //    //        ShowCheckResultUC(7, checkResultBM, "");  //工作证不区分团散
        //    //    //        Ring(ThreadPlay.Rings.Pass);
        //    //    //        //开闸操作
        //    //    //        SendControlOrder("OpenCountCMD", checkResultBM, "");
        //    //    //    }
        //    //    //}
        //    //    //else
        //    //    //{
        //    //    //    //1.判定该票是否在本机已经进行检票了
        //    //    //    string selSql = " select paperCode,create_time from check_list where paperCode ='" + paperCode + "' ";
        //    //    //    DataTable dt = sqliteHelper.ExecuteDataTable(selSql);
        //    //    //    if (dt != null && dt.Rows.Count > 0)
        //    //    //    {
        //    //    //        checkResultBM.PaperCode = paperCode;
        //    //    //        checkResultBM.InTime = Convert.ToInt64(dt.Rows[0]["create_time"].ToString());
        //    //    //        if (orderType == "1")
        //    //    //        {
        //    //    //            ShowCheckResultUC(2, checkResultBM, "已入馆");
        //    //    //            Ring(ThreadPlay.Rings.ScanFailed);
        //    //    //        }
        //    //    //        else if (orderType == "2")
        //    //    //        {
        //    //    //            ShowCheckResultUC(3, checkResultBM, "已入馆");
        //    //    //            Ring(ThreadPlay.Rings.ScanFailed);
        //    //    //        }

        //    //    //    }
        //    //    //    else
        //    //    //    {
        //    //    //        //2.检票成功，存入本地sqlite库中
        //    //    //        string sql = " insert into check_list(paperCode,check_type,check_url,check_content,is_sync,create_time) values ('" + paperCode + "','2','" + check_url + "','" + check_content + "',0," + TextureHelper.DateTimeToInt(DateTime.Now) + ")";

        //    //    //        int rows = sqliteHelper.ExecuteNonQuery(sql);
        //    //    //        if (rows > 0)
        //    //    //        {
        //    //    //            checkResultBM.PaperCode = paperCode;
        //    //    //            if (orderType == "1")
        //    //    //            {
        //    //    //                ShowCheckResultUC(4, checkResultBM, "");
        //    //    //                Ring(ThreadPlay.Rings.Pass);
        //    //    //            }
        //    //    //            else if (orderType == "2")
        //    //    //            {
        //    //    //                ShowCheckResultUC(5, checkResultBM, "");
        //    //    //                Ring(ThreadPlay.Rings.TeamPass);
        //    //    //            }

        //    //    //            //开闸操作
        //    //    //            SendControlOrder("OpenCountCMD", checkResultBM, "");
        //    //    //        }
        //    //    //    }
        //    //    }

        //    //    return true;
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    Logger.WriteLog("门票离线检票失败，票纸号：" + paperCode + "，原因：" + ex.Message);
        //    //    ShowCheckResultUC(1, null, "");
        //    //    Ring(ThreadPlay.Rings.ScanFailed);
        //    //    return false;
        //    //}

        //    #endregion
        //}

        //private bool CheckOutLineCard(string paperCode, string check_type, string orderType, string check_url, string check_content, string detailId)
        //{
        //    #region 身份证离线检票

        //    try
        //    {
        //        CheckResultBM checkResultBM = new CheckResultBM();
        //        //1.判定该票是否在本机已经进行检票了
        //        string selSql = " select paperCode,create_time from check_list where paperCode ='" + paperCode + "' and detailId = '" + detailId + "' ";
        //        DataTable dt = sqliteHelper.ExecuteDataTable(selSql);
        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            checkResultBM.PaperCode = paperCode;
        //            checkResultBM.InTime = Convert.ToInt64(dt.Rows[0]["create_time"].ToString());
        //        }
        //        else
        //        {
        //            //2.检票成功，存入本地sqlite库中
        //            string sql = " insert into check_list(paperCode,check_type,check_url,check_content,is_sync,create_time,detailId) values ('" + paperCode + "','" + check_type + "','" + check_url + "','" + check_content + "',0," + TextureHelper.DateTimeToInt(DateTime.Now) + ",'" + detailId + "')";

        //            int rows = sqliteHelper.ExecuteNonQuery(sql);
        //            if (rows > 0)
        //            {
        //                checkResultBM.PaperCode = paperCode;
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteLog("门票离线检票失败，票纸号：" + paperCode + "，原因：" + ex.Message);
        //        ShowCheckResultUC(1, null, "");
        //        Ring(ThreadPlay.Rings.ScanFailed);
        //        return false;
        //    }

        //    #endregion
        //}

        /// <summary>
        /// 功能说明：根据票Id获取门票名称
        /// 创建人：ys
        /// 创建日期：2016-10-20 10：09
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        //private string GetTicketNameById(string ticketId)
        //{
        //    try
        //    {
        //        string sql = "select openTicketName from t_ticket where id = " + ticketId + "";
        //        object obj = sqliteHelper.ExecuteScalar(sql);
        //        if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
        //            return obj.ToString();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return "";
        //}

        /// <summary>
        /// 功能说明：根据门票ID获取对应的场馆信息
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        //private string GetVenueIdByTicketId(string ticketId)
        //{
        //    try
        //    {
        //        string sql = "select venueId from t_ticket where id = " + ticketId + "";
        //        object obj = sqliteHelper.ExecuteScalar(sql);
        //        if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
        //            return obj.ToString();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return "";
        //}

        /// <summary>
        /// 功能说明：根据场馆Id获取场馆名称
        /// 创建人：ys
        /// 创建日期：2016-10-20 10：09
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        //private string GetVenueNameById(string venueId)
        //{
        //    try
        //    {
        //        string sql = "select venueName from t_venue where id = " + venueId + "";
        //        object obj = sqliteHelper.ExecuteScalar(sql);
        //        if (obj != null && !string.IsNullOrWhiteSpace(obj.ToString()))
        //            return obj.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteLog("获取本地数据场馆信息失败，原因：" + ex.Message);
        //    }
        //    return "";
        //}

        #endregion 离线检票

        #region 在线验票

        /// <summary>
        /// 功能说明：调用远程验票
        /// </summary>
        /// <param name="dic"></param>
        private void PaperCheckResult(Dictionary<string, string> dic)
        {
            #region 在线验票结果处理
            string QRCode = dic["paperScanner"];
            GetCheck(dic, obj =>
            {
                if (obj != null)
                {
                    #region 在线验票结果信息处理

                    try
                    {

                        CheckResultBM checkResultBM = (CheckResultBM)obj.Result;
                        if (checkResultBM != null)
                        {
                            if (checkResultBM.CheckStatus == DevCheckStatus.成功)
                            {
                                #region 验票成功情况处理
                                switch (checkResultBM.OrderType)
                                {
                                    case Yl.Cg.Model.OrderTypeEnum.团队:
                                        //显示验票结果界面
                                        ShowCheckResultUC(5, checkResultBM, checkResultBM.CheckStatus.ToString());

                                        bool bol = false;
                                        foreach (OrderCheckBM ocbm in checkResultBM.ListCheckBM)
                                        {
                                            if (ocbm.TicketId.Value == 12)
                                            {
                                                bol = true;
                                                break;
                                            }
                                        }
                                        if (bol)
                                            Ring(ThreadPlay.Rings.StudentPass);
                                        else
                                            Ring(ThreadPlay.Rings.TeamPass);

                                        break;
                                    default:  //工作证作为散客票中的一种，进行判定当前刷票是工作证则显示免费票页面
                                        {
                                            //if (QRCode != "")
                                            //{
                                            string factor = "";
                                            string paperInfo = TextureHelper.DecryptByFactor(QRCode, out factor);  //解析出来的二维码信息
                                            string title = paperInfo.Substring(0, 1);
                                            switch (title)
                                            {
                                                case "D":
                                                    ShowCheckResultUC(7, checkResultBM, "");
                                                    break;
                                                default:
                                                    //显示验票结果界面
                                                    ShowCheckResultUC(4, checkResultBM, "");
                                                    break;
                                            }

                                            Ring(ThreadPlay.Rings.Pass);
                                        }

                                        break;
                                }
                                //工控机发送开闸命令
                                SendControlOrder("OpenCountCMD", checkResultBM, "");
                                #endregion
                            }
                            else //验票失败
                            {
                                #region 验票失败情况处理
                                switch (checkResultBM.OrderType)
                                {
                                    case Yl.Cg.Model.OrderTypeEnum.团队:
                                        #region 团队票验票失败分为已入馆和其他情况处理

                                        switch (checkResultBM.CheckStatus)
                                        {
                                            case DevCheckStatus.不是领队:
                                                ShowCheckResultUC(1, checkResultBM, checkResultBM.CheckStatus.ToString());
                                                Ring(ThreadPlay.Rings.ScanFailed);
                                                break;
                                            case DevCheckStatus.已入馆:
                                                ShowCheckResultUC(3, checkResultBM, checkResultBM.CheckStatus.ToString());
                                                Ring(ThreadPlay.Rings.ScanFailed);
                                                break;
                                            case DevCheckStatus.非本场票:
                                                ShowCheckResultUC(8, checkResultBM, checkResultBM.CheckStatus.ToString());
                                                Ring(ThreadPlay.Rings.WrongVenue);
                                                break;
                                            case DevCheckStatus.未到检票时间:
                                                ShowCheckResultUC(1, checkResultBM, "未 到 检 票 时 间");
                                                Ring(ThreadPlay.Rings.ScanFailed);
                                                break;
                                            case DevCheckStatus.检票时间已过:
                                                ShowCheckResultUC(1, checkResultBM, "检 票 时 间 已 过");
                                                Ring(ThreadPlay.Rings.InvalidDate);
                                                break;
                                            case DevCheckStatus.非本场馆票:
                                                ShowCheckResultUC(1, checkResultBM, "非 本 场 馆 票");
                                                Ring(ThreadPlay.Rings.WrongVenue);
                                                //CheckOutLine(dic);  //***************如果服务端检票信息不存在则按照离线检票******* 去除 ys 现场售票中出票规则包含当前订单的所有子订单信息会产生在线验不通过，但是离线的规则可以通过的问题 2017-04-10 13:21 ********************
                                                break;
                                            default:
                                                ShowCheckResultUC(1, checkResultBM, checkResultBM.CheckStatus.ToString());
                                                Ring(ThreadPlay.Rings.ScanFailed);
                                                break;
                                        }
                                        #endregion
                                        break;
                                    case Yl.Cg.Model.OrderTypeEnum.散客:
                                        #region 散客票验票失败分为已入馆和其他情况处理

                                        switch (checkResultBM.CheckStatus)
                                        {
                                            case DevCheckStatus.已入馆:
                                                ShowCheckResultUC(2, checkResultBM, "");
                                                Ring(ThreadPlay.Rings.ScanFailed);
                                                break;
                                            case DevCheckStatus.非本场票:
                                                ShowCheckResultUC(8, checkResultBM, "");
                                                Ring(ThreadPlay.Rings.WrongVenue);
                                                break;
                                            case DevCheckStatus.未到检票时间:
                                                ShowCheckResultUC(1, checkResultBM, "未 到 检 票 时 间");
                                                Ring(ThreadPlay.Rings.ScanFailed);
                                                break;
                                            case DevCheckStatus.停止检票:
                                            case DevCheckStatus.检票时间已过:
                                                ShowCheckResultUC(1, checkResultBM, "检 票 时 间 已 过");
                                                Ring(ThreadPlay.Rings.InvalidDate);
                                                break;
                                            case DevCheckStatus.非本场馆票:
                                                ShowCheckResultUC(1, checkResultBM, "非 本 场 馆 票");
                                                Ring(ThreadPlay.Rings.WrongVenue);
                                                //CheckOutLine(dic);  //***************如果服务端检票信息不存在则按照离线检票******* 去除 ys 现场售票中出票规则包含当前订单的所有子订单信息会产生在线验不通过，但是离线的规则可以通过的问题 2017-04-10 13:21 ********************
                                                break;
                                            default:
                                                ShowCheckResultUC(1, checkResultBM, checkResultBM.CheckStatus.ToString());
                                                Ring(ThreadPlay.Rings.ScanFailed);
                                                break;
                                        }
                                        #endregion
                                        break;
                                    default:
                                        ShowCheckResultUC(1, checkResultBM, checkResultBM.CheckStatus.ToString());
                                        Ring(ThreadPlay.Rings.ScanFailed);
                                        break;
                                }
                                #endregion
                            }
                        }
                        else
                            ShowCheckResultUC(1, null, "检 票 失 败");
                    }
                    catch (Exception ex)
                    {
                        ShowCheckResultUC(1, null, "数 据 解 析 异 常");
                    }

                    #endregion
                }
                else
                {
                    ShowCheckResultUC(1, null, "接 口 数 据 异 常");
                }
            });

            #endregion
        }

        /// <summary>
        /// 功能说明：Post方式调用服务接口验证票据信息
        /// 创建人；ys
        /// 创建日期：2016-05-07 14:28
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="action"></param>
        private void GetCheck(Dictionary<string, string> dic, Action<CallResult<CheckResultBM>> action)
        {
            HttpX.Instance.Post<CheckResultBM>("api/zk_check/ticket/get_check_new", TextureHelper.ToJson<Dictionary<string, string>>(dic), true, arg =>
            {
                if (arg != null)
                {
                    action(arg.CallResult);
                }
            }, new MemoryStream());
        }

        #endregion 在线验票

        #region 根据验票结果展示页面

        /// <summary>
        /// 功能说明：显示当前页面及提示相应的信息
        /// 创建人：ys
        /// 创建日期：2016-05-07 13:30
        /// </summary>
        /// <param name="index"></param>
        private void ShowCheckResultUC(int index, CheckResultBM checkResultBM, string tipMessage)
        {
            _mainForm.Invoke(new MethodInvoker(() =>
            {
                _mainForm.SetFormType(_mainForm.GetUcZr(index), checkResultBM, tipMessage);
            }));
        }

        #endregion

        #region 中科开闸控制命令发送

        /// <summary>
        /// 功能说明：中科工控机发送指令
        /// 创建人：ys
        /// 创建日期：2016-05-07 13:39
        /// </summary>
        /// <param name="configKey"></param>
        /// <param name="checkResultBM"></param>
        /// <param name="soundSrc"></param>
        private void SendControlOrder(string configKey, CheckResultBM checkResultBM, string soundSrc)
        {
            string OpenCountCMD = ConfigurationManager.AppSettings["OpenCountCMD"];
            int personCount = 1;
            try
            {

                if (checkResultBM.ListCheckBM != null && checkResultBM.ListCheckBM.Count > 0)
                {
                    for (int i = 0; i < checkResultBM.ListCheckBM.Count; i++)
                    {

                        if (i == 0)
                        {
                            personCount = checkResultBM.ListCheckBM[0].PersonCount.Value;
                        }
                        else
                        {
                            personCount += checkResultBM.ListCheckBM[i].PersonCount.Value;
                        }
                    }
                }
                else
                {
                    if (checkResultBM.PersonCount.Value != 0)
                        personCount = checkResultBM.PersonCount.Value;
                    else
                    {
                        checkResultBM.PersonCount = 1;
                        personCount = checkResultBM.ListCheckBM[0].PersonCount.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex.ToString());
            }
            //加入开闸队列中
            lock (Config.Instance.Qu)
            {
                for (int i = 0; i < personCount; i++)
                {
                    Config.Instance.Qu.Enqueue(OpenCountCMD);
                }
            }
            //Config.Instance.inflag = false;
            YLGateDev.OpenCount(OpenCountCMD, Config.Instance.GateSerial, soundSrc);
            //for (int i = 0; i < personCount; i++)
            //{
            //    YLGateDev.OpenCount(OpenCountCMD, Config.Instance.GateSerial, soundSrc);
            //    Thread.Sleep(100);
            //}
            try
            {
                if (checkResultBM != null)
                {
                    Config.Instance.LastCount = checkResultBM.PersonCount.Value;
                }
            }
            catch (Exception ex)
            {

            }

        }

        #region 播放声音文件

        /// <summary>
        /// 放音。
        /// </summary>
        public void Ring(string ring)
        {
            var tp = new ThreadPlay(Application.StartupPath + "\\", ring);
            //var tp = new ThreadPlay(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\", ring);
            var t = new Thread(new ThreadStart(tp.Play)) { IsBackground = true, Priority = ThreadPriority.Lowest };
            t.Start();
        }

        #endregion

        #endregion

        #region 获取工控机发送命令异或结果

        /// <summary>
        /// 功能说民：获取字符串相应结果的异或值
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private string GetExclusiveOR(string command)
        {
            int result = 0;
            string[] strArr = command.Split(' ');

            foreach (string str in strArr)
            {
                int tem = Convert.ToInt32(Int32.Parse(str, System.Globalization.NumberStyles.HexNumber));
                result ^= tem;
            }
            return result.ToString("X2");
        }

        #endregion

        #endregion ----------------------------------二维码检票end--------------------------------------------

        #region 心跳检测

        private void GetCheckBreath(Dictionary<string, string> dic, Action<CallResult<CheckBreathBM>> action)
        {
            HttpX.Instance.Post<CheckBreathBM>("api/zk_check/ticket/get_checkbreath", TextureHelper.ToJson<Dictionary<string, string>>(dic), true, arg =>
            {
                if (arg != null)
                {
                    action(arg.CallResult);
                }
            }, null);
        }

        public void DevCheckBreath(string deviceCode, int deviceId)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("deviceCode", deviceCode);
            GetCheckBreath(dic, obj =>
           {
               if (obj != null)
               {
                   CheckBreathBM checkBreathBM = (CheckBreathBM)obj.Result;
                   if (checkBreathBM != null)
                   {
                       if (checkBreathBM.LineStatus == DevLineStatus.online)  //在线
                       {
                           _mainForm.Invoke(new MethodInvoker(() =>
                           {
                               _mainForm.RefreshState(0);
                           }));

                           if (!Config.is_Online)
                           {
                               Config.is_Online = true;
                           }
                           CheckPriceLoad();  //在线状态时下载当前闸机可检门票信息
                       }
                       if (checkBreathBM.LineStatus == DevLineStatus.outline) //离线
                       {
                           _mainForm.Invoke(new MethodInvoker(() =>
                           {
                               _mainForm.RefreshState(1);
                           }));

                           Config.is_Online = false;
                       }
                       if (checkBreathBM.EnableStatus == DevEnableStatus.disable) //不可用
                       {
                           _mainForm.Invoke(new MethodInvoker(() =>
                           {
                               _mainForm.RefreshState(1);
                           }));

                           ShowCheckResultUC(6, null, "");
                           Config.is_Online = false;
                       }
                   }
                   else
                   {
                       _mainForm.Invoke(new MethodInvoker(() =>
                       {
                           _mainForm.RefreshState(1);
                       }));
                       Config.is_Online = false;
                   }

               }
               else
               {
                   _mainForm.Invoke(new MethodInvoker(() =>
                   {
                       _mainForm.RefreshState(1);
                   }));
                   Config.is_Online = false;
               }
           });

        }

        #endregion

        #region 离线检票数据在有网络时进行上传

        //public void SyncCheckTicket()
        //{
        //    if (Config.is_Online)
        //    {
        //        try
        //        {
        //            string sql = " select * from check_list where is_sync = 0 ";
        //            DataTable dt = sqliteHelper.ExecuteDataTable(sql);
        //            if (dt != null && dt.Rows.Count > 0)
        //            {
        //                Config.Instance.CurrentUploadCount++;
        //                foreach (DataRow dr in dt.Rows)
        //                {
        //                    string id = dr["id"].ToString();
        //                    string url = dr["check_url"].ToString();
        //                    string dicJson = dr["check_content"].ToString();
        //                    //上传离线检票数据
        //                    UploadOutLineCheckTicket(id, url, dicJson);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //}

        //public void UploadOutLineCheckTicket(string id, string url, string dicJson)
        //{
        //    PostOutLineCheck(id, url, dicJson, obj =>
        //    {
        //        if (obj != null)
        //        {
        //            #region 在线验票结果信息处理

        //            try
        //            {
        //                CheckResultBM checkResultBM = (CheckResultBM)obj.Result;
        //                if (checkResultBM != null)
        //                {
        //                    if (checkResultBM.CheckStatus == DevCheckStatus.成功 || checkResultBM.CheckStatus == DevCheckStatus.已入馆)
        //                    {
        //                        string sql = "delete from check_list where id = '" + id + "'";
        //                        int rows = sqliteHelper.ExecuteNonQuery(sql);
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {

        //            }

        //            #endregion
        //        }
        //    });
        //}

        private void PostOutLineCheck(string id, string url, string dicJson, Action<CallResult<CheckResultBM>> action)
        {
            HttpX.Instance.Post<CheckResultBM>(url, dicJson, true, arg =>
            {
                if (arg != null)
                {
                    action(arg.CallResult);
                }
            }, new MemoryStream());
        }

        #endregion 离线检票数据在有网络时进行上传

        #region 时间同步方法

        #region 时间设置相关外部dll 
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMiliseconds;
        }
        [DllImport("Kernel32.dll")]
        public static extern bool SetSystemTime(ref SystemTime sysTime);
        public static bool SetTime(DateTime dt)
        {
            bool flag = false;
            try
            {
                SystemTime sysTime = new SystemTime();
                sysTime.wYear = Convert.ToUInt16(dt.ToUniversalTime().Year);
                sysTime.wMonth = Convert.ToUInt16(dt.ToUniversalTime().Month);
                sysTime.wDay = Convert.ToUInt16(dt.ToUniversalTime().Day);
                sysTime.wHour = Convert.ToUInt16(dt.ToUniversalTime().Hour);
                sysTime.wMinute = Convert.ToUInt16(dt.ToUniversalTime().Minute);
                sysTime.wSecond = Convert.ToUInt16(dt.ToUniversalTime().Second);
                flag = SetSystemTime(ref sysTime);
            }
            catch (Exception ex)
            {

            }
            return flag;
        }
        #endregion

        /// <summary>
        /// 获取当前时间
        /// </summary>
        private void GetServerTime()
        {
            // SetStateText("正在菜单信息...");
            GetTime(crTime =>
            {
                if (crTime != null && crTime.Result > 0)
                {
                    DateTime dtServerTime = Yl.Ticket5.Common40.Utilities.TextureHelper.IntToDateTime(crTime.Result);
                    SetTime(dtServerTime);
                }
            });
        }
        /// <summary>
        /// 同步服务器时间
        /// </summary>
        /// <param name="action"></param>
        public void GetTime(Action<CallResult<long>> action)
        {
            HttpX.Instance.Post<long>("api/loginall/get_time", "", true, arg =>
            {
                if (arg != null)
                {
                    action(arg.CallResult);
                }
            });
        }

        #endregion 时间同步方法结束

        #region 不关机的闸机初始化下载状态，每天0晨0点即可符合下载当天的网络预售信息

        public void IniDownLoadState()
        {
            isDownload = true;

            isDownloadTicket = true;
        }

        #endregion

        #region 下载今日预售订单信息存储在本地数据库中以备离线检票用

        // 功能说明：下载今日预售订单信息存储在本地数据库中
        //public void DownLoadTodayPreSaleOrder(Action<CallResult<PreSaleOrderBM>> action)
        //{
        //    HttpX.Instance.Post<PreSaleOrderBM>("api/zk_check/ticket/get_presale_order", "", true, arg =>
        //    {
        //        if (arg != null)
        //        {
        //            action(arg.CallResult);
        //        }
        //    }, new MemoryStream());
        //}

        //public void SaveOrderInfo()
        //{
        //    if (Config.is_Online && isDownload)
        //    {
        //        DownLoadTodayPreSaleOrder(obj =>
        //        {
        //            #region 验票结果处理

        //            if (obj != null)
        //            {
        //                try
        //                {
        //                    isDownload = false;

        //                    PreSaleOrderBM preSaleOrderBM = (PreSaleOrderBM)obj.Result;
        //                    List<OrderBM> listMainOrder = preSaleOrderBM.ListOrder;
        //                    List<OrderDetailBM> listOrderDetail = preSaleOrderBM.ListOrderDetail;
        //                    List<OrderDetailLongBM> listOrderDetailLong = preSaleOrderBM.ListOrderDetailLong;
        //                    List<OrderDetailSessionsBM> listOrderDetailSessions = preSaleOrderBM.ListOrderDetailSessions;

        //                    //保存主订单信息
        //                    SaveMainOrder(listMainOrder);
        //                    //保存普通子订单信息
        //                    SaveOrderDetail(listOrderDetail);
        //                    //保存计次子订单信息
        //                    SaveOrderDetailLong(listOrderDetailLong);
        //                    //保存场次子订单信息
        //                    SaveOrderDetailSessions(listOrderDetailSessions);
        //                }
        //                catch (Exception ex)
        //                {
        //                    isDownload = true;
        //                    Logger.WriteLog("下载当日预售订单信息失败，原因：" + ex.Message);
        //                }
        //            }

        //            #endregion
        //        });
        //    }
        //}

        //private bool SaveMainOrder(List<OrderBM> listOrder)
        //{
        //    bool flag = false;
        //    //1.删除本地的订单信息
        //    string sql = " delete from t_order ";
        //    int rows = sqliteHelper.ExecuteNonQuery(sql);
        //    if (listOrder != null && listOrder.Count > 0)
        //    {
        //        foreach (OrderBM _model in listOrder)
        //        {
        //            //2.保存下载的订单信息
        //            StringBuilder sbSql = new StringBuilder();
        //            sbSql.Append(@"INSERT INTO t_order (id,orderChannel, orderType, orderTime, orderUserId, orderUserType, appointmentCount, 
        //                    actualCount, orderTotal,payTotal, refundTotal, contactName, phone, isInvoice,isCompony, invoiceTitle, invoiceStatus,
        //                    payStatus, payTime, payWay, payRemark, ticketCode, orderCode, outOrderId, inTime, ticketInfo, inStatus, createUser, createTime, updateUser, 
        //                    updateTime, ip, remark, isMoneyCoupon, orderUserName, status,deviceid,isDelete) 
        //                    VALUES (@id,@orderChannel, @orderType, @orderTime, @orderUserId, @orderUserType, @appointmentCount, @actualCount, @orderTotal,@payTotal,
        //                    @refundTotal, @contactName, @phone, @isInvoice, @isCompony, @invoiceTitle, @invoiceStatus, @payStatus, @payTime, @payWay, @payRemark,
        //                    @ticketCode, @orderCode, @outOrderId, @inTime, @ticketInfo, @inStatus, @createUser, @createTime, @updateUser, @updateTime, @ip, @remark, 
        //                    @isMoneyCoupon, @orderUserName, @status,@deviceid,@isDelete); ");
        //            int rowResult = sqliteHelper.ExecuteNonQuery(sbSql.ToString(), new SQLiteParameter[]
        //            {
        //                new SQLiteParameter("@id",_model.Id),
        //                new SQLiteParameter("@orderChannel",Convert.ToInt32( _model.OrderChannel)),
        //                new SQLiteParameter("@orderType",Convert.ToInt32( _model.OrderType)),
        //                new SQLiteParameter("@orderTime",Convert.ToInt64( _model.OrderTime)),
        //                new SQLiteParameter("@orderUserId",Convert.ToInt64(_model.OrderUserId)),
        //                new SQLiteParameter("@orderUserType",Convert.ToInt32(_model.OrderUserType)),
        //                new SQLiteParameter("@appointmentCount",Convert.ToInt32(_model.AppointmentCount)),
        //                new SQLiteParameter("@actualCount",Convert.ToInt32(_model.ActualCount)),
        //                new SQLiteParameter("@orderTotal",Convert.ToDecimal(_model.OrderTotal)),
        //                new SQLiteParameter("@payTotal",Convert.ToDecimal(_model.PayTotal)),
        //                new SQLiteParameter("@refundTotal",Convert.ToDecimal(_model.PayTotal)),
        //                new SQLiteParameter("@contactName",Convert.ToString( _model.ContactName)),
        //                new SQLiteParameter("@phone",Convert.ToString( _model.Phone)),
        //                new SQLiteParameter("@isInvoice",Convert.ToInt32(_model.IsInvoice)),
        //                new SQLiteParameter("@isCompony",Convert.ToInt32(_model.IsCompony)),
        //                new SQLiteParameter("@invoiceTitle",Convert.ToString( _model.InvoiceTitle)),
        //                new SQLiteParameter("@invoiceStatus",Convert.ToInt32(_model.InvoiceStatus)),
        //                new SQLiteParameter("@payStatus",Convert.ToInt32(_model.PayStatus)),
        //                new SQLiteParameter("@payTime",Convert.ToInt64(_model.PayTime)),
        //                new SQLiteParameter("@payWay",Convert.ToInt32(_model.PayWay)),
        //                new SQLiteParameter("@payRemark",Convert.ToString( _model.PayRemark)),
        //                new SQLiteParameter("@ticketCode",Convert.ToString( _model.TicketCode)),
        //                new SQLiteParameter("@orderCode",Convert.ToString( _model.OrderCode)),
        //                new SQLiteParameter("@outOrderId",Convert.ToString( _model.OutOrderId)),
        //                new SQLiteParameter("@inTime",Convert.ToInt32(_model.InTime)),
        //                new SQLiteParameter("@ticketInfo",Convert.ToString( _model.TicketInfo)),
        //                new SQLiteParameter("@inStatus",Convert.ToInt32(_model.InStatus)),
        //                new SQLiteParameter("@createUser",Convert.ToInt64(_model.CreateUser)),
        //                new SQLiteParameter("@createTime",Convert.ToInt64(_model.CreateTime)),
        //                new SQLiteParameter("@updateUser",Convert.ToInt64(_model.UpdateUser)),
        //                new SQLiteParameter("@updateTime",Convert.ToInt64(_model.UpdateTime)),
        //                new SQLiteParameter("@ip",_model.Ip.ToString()),
        //                new SQLiteParameter("@status",Convert.ToInt32(_model.Status)),
        //                new SQLiteParameter("@remark",Convert.ToString( _model.Remark)),
        //                new SQLiteParameter("@isMoneyCoupon",Convert.ToInt32(_model.IsMoneyCoupon)),
        //                new SQLiteParameter("@orderUserName",Convert.ToString( _model.OrderUserName)),
        //                new SQLiteParameter("@deviceid",Convert.ToInt64(_model.DeviceId)),
        //                new SQLiteParameter("@isDelete",_model.IsDelete ),
        //            });
        //            if (rowResult > 0)
        //                flag = true;
        //        }
        //    }

        //    return flag;
        //}

        //private bool SaveOrderDetail(List<OrderDetailBM> listOrderDetail)
        //{
        //    bool flag = false;
        //    string sql = "delete from t_order_detail";
        //    int rows = sqliteHelper.ExecuteNonQuery(sql);
        //    if (listOrderDetail != null && listOrderDetail.Count > 0)
        //    {
        //        foreach (OrderDetailBM _detailModel in listOrderDetail)
        //        {
        //            StringBuilder sbSql = new StringBuilder();
        //            sbSql.Append(@"INSERT INTO t_order_detail 
        //                            (id,orderId, ticketId, actualPrice, inUserId, inName, inUserCardNo, inUserCertificateType, 
        //                            inUserCertificate,personCount, firstInitialInDate, initialInDate, InTime, orderDetailStatus,payStatus, refundStatus,
        //                            reason, auditRemark, changeTimeStatus, isTransfer, transferStatus, outOrderDetailId, isReport, isPrint, inStatus,
        //                            createUser, createTime, updateUser, updateTime, ip, status , remark, isVideoCupon, isVip,paperCode,
        //                            beforePaperCode,isPrintTicket,deviceId,isDelete) VALUES (@id,@orderId,@ticketId,@actualPrice,@inUserId,@inName,
        //                            @inUserCardNo,@inUserCertificateType,@inUserCertificate,@personCount,@firstInitialInDate,@initialInDate,
        //                            @InTime,@orderDetailStatus,@payStatus,@refundStatus,@reason,@auditRemark,@changeTimeStatus,@isTransfer,
        //                            @transferStatus,@outOrderDetailId,@isReport,@isPrint,@inStatus,@createUser,@createTime,@updateUser,@updateTime,
        //                            @ip,@STATUS,@remark,@isVideoCupon,@isVip,@paperCode,@beforePaperCode,@isPrintTicket,@deviceId,@isDelete);");
        //            int rowResult = sqliteHelper.ExecuteNonQuery(sbSql.ToString(), new SQLiteParameter[]
        //            {
        //                new SQLiteParameter("@id",_detailModel.Id),
        //                new SQLiteParameter("@orderId",_detailModel.OrderId),
        //                new SQLiteParameter("@ticketId",_detailModel.TicketId),
        //                new SQLiteParameter("@actualPrice",_detailModel.ActualPrice),
        //                new SQLiteParameter("@inUserId",_detailModel.InUserId),
        //                new SQLiteParameter("@inName",_detailModel.InName),
        //                new SQLiteParameter("@inUserCardNo",_detailModel.InUserCardNo),
        //                new SQLiteParameter("@inUserCertificateType",_detailModel.InUserCertificateType),
        //                new SQLiteParameter("@inUserCertificate",_detailModel.InUserCertificate),
        //                new SQLiteParameter("@personCount",_detailModel.PersonCount),
        //                new SQLiteParameter("@firstInitialInDate",_detailModel.FirstInitialInDate),
        //                new SQLiteParameter("@initialInDate",_detailModel.InitialInDate),
        //                new SQLiteParameter("@InTime",_detailModel.InTime),
        //                new SQLiteParameter("@orderDetailStatus",_detailModel.OrderDetailStatus),
        //                new SQLiteParameter("@payStatus",_detailModel.PayStatus),
        //                new SQLiteParameter("@refundStatus",_detailModel.RefundStatus),
        //                new SQLiteParameter("@reason",_detailModel.Reason),
        //                new SQLiteParameter("@auditRemark",_detailModel.AuditRemark),
        //                new SQLiteParameter("@changeTimeStatus",_detailModel.ChangeTimeStatus),
        //                new SQLiteParameter("@isTransfer",_detailModel.IsTransfer),
        //                new SQLiteParameter("@transferStatus",_detailModel.TransferStatus),
        //                new SQLiteParameter("@outOrderDetailId",_detailModel.OutOrderDetailId),
        //                new SQLiteParameter("@isReport",_detailModel.IsReport),
        //                new SQLiteParameter("@isPrint",_detailModel.IsPrint),
        //                new SQLiteParameter("@inStatus",_detailModel.InStatus),
        //                new SQLiteParameter("@createUser",_detailModel.CreateUser),
        //                new SQLiteParameter("@createTime",_detailModel.CreateTime),
        //                new SQLiteParameter("@updateUser",_detailModel.UpdateUser),
        //                new SQLiteParameter("@updateTime",_detailModel.UpdateTime),
        //                new SQLiteParameter("@ip",_detailModel.Ip),
        //                new SQLiteParameter("@STATUS",_detailModel.Status),
        //                new SQLiteParameter("@isDelete",_detailModel.IsDelete),
        //                new SQLiteParameter("@remark",_detailModel.Remark),
        //                new SQLiteParameter("@isVideoCupon",_detailModel.IsVideoCupon),
        //                new SQLiteParameter("@isVip",_detailModel.IsVip),
        //                new SQLiteParameter("@paperCode",_detailModel.PaperCode),
        //                new SQLiteParameter("@beforePaperCode",_detailModel.BeforePaperCode),
        //                new SQLiteParameter("@isPrintTicket",_detailModel.IsPrintTicket),
        //                new SQLiteParameter("@deviceId",_detailModel.DeviceId)
        //            });

        //            if (rowResult > 0)
        //                flag = true;
        //        }
        //    }

        //    return flag;
        //}

        //private bool SaveOrderDetailLong(List<OrderDetailLongBM> listOrderDetailLong)
        //{
        //    bool flag = false;
        //    string sql = "delete from t_order_detail_long";
        //    int rows = sqliteHelper.ExecuteNonQuery(sql);
        //    if (listOrderDetailLong != null && listOrderDetailLong.Count > 0)
        //    {
        //        foreach (OrderDetailLongBM _longModel in listOrderDetailLong)
        //        {
        //            StringBuilder sbSql = new StringBuilder();
        //            sbSql.Append(@"INSERT INTO t_order_detail_long (id,orderId, ticketId, actualPrice, inUserId, inName, inUserCardNo, inUserCertificateType
        //                , inUserCertificate,beginDate, endDate, beginTime, endTime, inCount,totalCount,orderDetailStatus,payStatus, refundStatus, reason
        //                , auditRemark, isTransfer, transferStatus,  isReport, isPrint, inStatus,inTime, createUser, createTime, updateUser, updateTime
        //                , ip, status , remark, isVideoCupon,mediaType,certificate,isVip,paperCode,beforePaperCode,isPrintTicket,deviceId) VALUES 
        //                (@id,@orderId,@ticketId,@actualPrice,@inUserId,@inName,@inUserCardNo,@inUserCertificateType,@inUserCertificate,@beginDate,@endDate
        //                ,@beginTime,@endTime,@inCount,@totalCount,@orderDetailStatus,@payStatus,@refundStatus,@reason,@auditRemark,@isTransfer,@transferStatus
        //                ,@isReport,@isPrint,@inStatus,@inTime,@createUser,@createTime,@updateUser,@updateTime,@ip,@STATUS,@remark,@isVideoCupon,@mediaType
        //                ,@certificate,@isVip,@paperCode,@beforePaperCode,@isPrintTicket,@deviceId);");
        //            int rowResult = sqliteHelper.ExecuteNonQuery(sbSql.ToString(), new SQLiteParameter[]
        //            {
        //                new SQLiteParameter("@id",_longModel.Id),
        //                new SQLiteParameter("@orderId",_longModel.OrderId),
        //                new SQLiteParameter("@ticketId",_longModel.TicketId),
        //                new SQLiteParameter("@actualPrice",_longModel.ActualPrice),
        //                new SQLiteParameter("@inUserId",_longModel.InUserId),
        //                new SQLiteParameter("@inName",_longModel.InName),
        //                new SQLiteParameter("@inUserCardNo",_longModel.InUserCardNo),
        //                new SQLiteParameter("@inUserCertificateType",_longModel.InUserCertificateType),
        //                new SQLiteParameter("@inUserCertificate",_longModel.InUserCertificate),
        //                new SQLiteParameter("@beginDate",_longModel.BeginDate),
        //                new SQLiteParameter("@endDate",_longModel.EndDate),
        //                new SQLiteParameter("@beginTime",_longModel.BeginTime),
        //                new SQLiteParameter("@endTime",_longModel.EndTime),
        //                new SQLiteParameter("@inCount",_longModel.InCount),
        //                new SQLiteParameter("@totalCount",_longModel.TotalCount),
        //                new SQLiteParameter("@orderDetailStatus",/*_longModel.OrderDetailStatus"1"),
        //                new SQLiteParameter("@payStatus",_longModel.PayStatus),
        //                new SQLiteParameter("@refundStatus",/*_longModel.RefundStatus*/"0"),
        //                new SQLiteParameter("@reason",_longModel.Reason),
        //                new SQLiteParameter("@auditRemark",_longModel.AuditRemark),
        //                new SQLiteParameter("@isTransfer",/*_longModel.IsTransfer*/"0"),
        //                new SQLiteParameter("@transferStatus",/*_longModel.TransferStatus*/-1),
        //                new SQLiteParameter("@isReport",/*_longModel.IsReport*/"1"),
        //                new SQLiteParameter("@isPrint","0"),
        //                new SQLiteParameter("@inStatus",/*_longModel.InStatus*/"0"),
        //                new SQLiteParameter("@inTime",_longModel.InTime),
        //                new SQLiteParameter("@createUser",_longModel.CreateUser),
        //                new SQLiteParameter("@createTime",Yl.Ticket5.Common40.Utilities.TextureHelper.DateTimeToInt(DateTime.Now)),
        //                new SQLiteParameter("@updateUser",_longModel.UpdateUser),
        //                new SQLiteParameter("@updateTime",_longModel.UpdateTime),
        //                new SQLiteParameter("@ip",_longModel.Ip),
        //                new SQLiteParameter("@STATUS",/*(int)DataStatus.可用*/"1"),
        //                //new SQLiteParameter("@isDelete",/*_longModel.IsDelete*/0),
        //                new SQLiteParameter("@remark",_longModel.Remark),
        //                new SQLiteParameter("@isVideoCupon",_longModel.IsVideoCupon),
        //                new SQLiteParameter("@mediaType",_longModel.MediaType),
        //                new SQLiteParameter("@certificate",_longModel.Certificate),
        //                new SQLiteParameter("@isVip",_longModel.IsVip),
        //                new SQLiteParameter("@paperCode",_longModel.PaperCode),
        //                new SQLiteParameter("@beforePaperCode",_longModel.BeforePaperCode),
        //                new SQLiteParameter("@isPrintTicket",_longModel.IsPrintTicket),
        //                new SQLiteParameter("@deviceId",_longModel.DeviceId)
        //            });

        //            if (rowResult > 0)
        //                flag = true;
        //        }
        //    }

        //    return flag;
        //}

        //private bool SaveOrderDetailSessions(List<OrderDetailSessionsBM> listOrderDetailSessions)
        //{
        //    bool flag = false;
        //    string sql = "delete from t_order_detail_sessions";
        //    int rows = sqliteHelper.ExecuteNonQuery(sql);
        //    if (listOrderDetailSessions != null && listOrderDetailSessions.Count > 0)
        //    {
        //        foreach (OrderDetailSessionsBM _sessionModel in listOrderDetailSessions)
        //        {
        //            StringBuilder sbSql = new StringBuilder();
        //            sbSql.Append(@"INSERT INTO t_order_detail_sessions 
        //                        (id,orderId, ticketId, actualPrice,inUserId,inName,inUserCardNo,inUserCertificateType,inUserCertificate,beginDate, 
        //                        endDate, beginTime, endTime,sessionsId,seatId,seatDetail,orderDetailStatus,payStatus, refundStatus, reason, auditRemark, 
        //                        isTransfer, transferStatus, isReport, inStatus,inTime, createUser, createTime, updateUser, updateTime, ip, status , remark,
        //                        isVideoCupon,isVip,paperCode,beforePaperCode,isPrintTicket,deviceId,isDelete) VALUES (@id,@orderId,@ticketId,@actualPrice,@inUserId,@inName,
        //                        @inUserCardNo,@inUserCertificateType,@inUserCertificate,@beginDate,@endDate,@beginTime,@endTime,@sessionsId,@seatId,@seatDetail,
        //                        @orderDetailStatus,@payStatus,@refundStatus,@reason,@auditRemark,@isTransfer,@transferStatus,@isReport,@inStatus,@inTime,@createUser,
        //                        @createTime,@updateUser,@updateTime,@ip,@STATUS,@remark,@isVideoCupon,@isVip,@paperCode,@beforePaperCode,@isPrintTicket,@deviceId,@isDelete);");
        //            int rowResult = sqliteHelper.ExecuteNonQuery(sbSql.ToString(), new SQLiteParameter[]
        //            {
        //                new SQLiteParameter("@id",_sessionModel.Id),
        //                new SQLiteParameter("@orderId",_sessionModel.OrderId),
        //                new SQLiteParameter("@ticketId",_sessionModel.TicketId),
        //                new SQLiteParameter("@actualPrice",_sessionModel.ActualPrice),
        //                new SQLiteParameter("@inUserId",_sessionModel.InUserId),
        //                new SQLiteParameter("@inName",_sessionModel.InName),
        //                new SQLiteParameter("@inUserCardNo",_sessionModel.InUserCardNo),
        //                new SQLiteParameter("@inUserCertificateType",_sessionModel.InUserCertificateType),
        //                new SQLiteParameter("@inUserCertificate",_sessionModel.InUserCertificate),
        //                new SQLiteParameter("@beginDate",_sessionModel.BeginDate),
        //                new SQLiteParameter("@endDate",_sessionModel.EndDate),
        //                new SQLiteParameter("@beginTime",_sessionModel.BeginTime),
        //                new SQLiteParameter("@endTime",_sessionModel.EndTime),
        //                new SQLiteParameter("@sessionsId",_sessionModel.SessionsId),
        //                new SQLiteParameter("@seatId",_sessionModel.SeatId),
        //                new SQLiteParameter("@seatDetail",_sessionModel.SeatDetail),
        //                new SQLiteParameter("@orderDetailStatus",_sessionModel.OrderDetailStatus),
        //                new SQLiteParameter("@payStatus",_sessionModel.PayStatus),
        //                new SQLiteParameter("@refundStatus",_sessionModel.RefundStatus),
        //                new SQLiteParameter("@reason",_sessionModel.Reason),
        //                new SQLiteParameter("@auditRemark",_sessionModel.AuditRemark),
        //                new SQLiteParameter("@isTransfer",_sessionModel.IsTransfer),
        //                new SQLiteParameter("@transferStatus",_sessionModel.TransferStatus),
        //                new SQLiteParameter("@isReport",_sessionModel.IsReport),
        //                new SQLiteParameter("@isPrint",_sessionModel.IsPrint),
        //                new SQLiteParameter("@inStatus",_sessionModel.InStatus),
        //                new SQLiteParameter("@inTime",_sessionModel.InTime),
        //                new SQLiteParameter("@createUser",_sessionModel.CreateUser),
        //                new SQLiteParameter("@createTime",_sessionModel.CreateTime),
        //                new SQLiteParameter("@updateUser",_sessionModel.UpdateUser),
        //                new SQLiteParameter("@updateTime",_sessionModel.UpdateTime),
        //                new SQLiteParameter("@ip",_sessionModel.Ip),
        //                new SQLiteParameter("@STATUS",_sessionModel.Status),
        //                new SQLiteParameter("@isDelete",_sessionModel.IsDelete ),
        //                new SQLiteParameter("@remark",_sessionModel.Remark),
        //                new SQLiteParameter("@isVideoCupon",_sessionModel.IsVideoCupon),
        //                new SQLiteParameter("@isVip",_sessionModel.IsVip),
        //                new SQLiteParameter("@paperCode",_sessionModel.PaperCode),
        //                new SQLiteParameter("@beforePaperCode",_sessionModel.BeforePaperCode),
        //                new SQLiteParameter("@isPrintTicket",_sessionModel.IsPrintTicket),
        //                new SQLiteParameter("@deviceId",_sessionModel.DeviceId)
        //            });

        //            if (rowResult > 0)
        //                flag = true;
        //        }
        //    }

        //    return flag;
        //}

        #endregion

        #region 下载门票场馆信息

        /// <summary>n
        /// 功能说明：下载门票场馆信息
        /// 创建人：ys
        /// 创建日期：2016-08-24 18：00
        /// </summary>
        /// <param name="action"></param>
        //private void DownLoadTicketVenueInfo(Action<CallResult<TicketVenueBM>> action)
        //{
        //    HttpX.Instance.Post<TicketVenueBM>("api/zk_check/ticket/get_ticketvenue", "", true, arg =>
        //    {
        //        if (arg != null)
        //        {
        //            action(arg.CallResult);
        //        }
        //    }, new MemoryStream());
        //}

        //public void SaveTicketVenueInfo()
        //{
        //    if (Config.is_Online && isDownloadTicket)
        //    {
        //        DownLoadTicketVenueInfo(obj =>
        //        {
        //            #region 验票结果处理

        //            if (obj != null)
        //            {
        //                try
        //                {
        //                    isDownloadTicket = false;
        //                    TicketVenueBM ticketVenueBM = (TicketVenueBM)obj.Result;

        //                    List<TicketBM> listTicket = ticketVenueBM.ListTicket;
        //                    List<VenueBM> listVenue = ticketVenueBM.ListVenue;
        //                    List<VenueDeviceBM> listVenueDevice = ticketVenueBM.ListVenueDevice;
        //                    List<SessionsBM> listSessions = ticketVenueBM.ListSessions;
        //                    List<SessionVideoBM> listSessionVideo = ticketVenueBM.ListSessionVideo;

        //                    //保存门票信息
        //                    SaveTicket(listTicket);
        //                    //保存场馆信息
        //                    SaveVenue(listVenue);
        //                    //保存场馆设备信息
        //                    SaveVenueDevice(listVenueDevice);
        //                    //保存场次信息
        //                    SaveSessions(listSessions);
        //                    //保存场次放映信息
        //                    SaveSessionsVideo(listSessionVideo);

        //                }
        //                catch (Exception ex)
        //                {
        //                    isDownloadTicket = true;
        //                    Logger.WriteLog("下载门票场馆场次信息失败，原因：" + ex.Message);
        //                }
        //            }

        //            #endregion
        //        });
        //    }
        //}

        //private bool SaveTicket(List<TicketBM> listTicket)
        //{
        //    bool flag = false;
        //    if (listTicket != null && listTicket.Count > 0)
        //    {
        //        //1.删除本地的订单信息
        //        string sql = " delete from t_ticket ";
        //        int rows = sqliteHelper.ExecuteNonQuery(sql);
        //        foreach (TicketBM _model in listTicket)
        //        {
        //            //2.保存下载的订单信息
        //            StringBuilder sbSql = new StringBuilder();
        //            sbSql.Append(@"INSERT INTO t_ticket (id,ticketName,openTicketName, ticketType,priceType, price
        //                    , ticketStyleId, sellWay, venueId, teamType, validCount
        //                    ,personCount, couponTypeId, validDays, validBeginDate, validEndDate
        //                    ,preSellDays, sellStartDate, sellEndDate,sellStartTime, sellEndTime
        //                    , inStartDate, inEndDate, inStartTime, inEndTime, isAutoSchedule
        //                    , seasonType, dayCount, createUser, createTime, updateUser
        //                    , updateTime, ip, remark, sort,status,isDelete) 
        //                    VALUES (@id,@ticketName,@openTicketName, @ticketType, @priceType, @price, @ticketStyleId, @sellWay, @venueId, @teamType,@validCount,
        //                    @personCount, @couponTypeId, @validDays, @validBeginDate, @validEndDate, @preSellDays, @sellStartDate, @sellEndDate, @sellStartTime, @sellEndTime, @inStartDate,
        //                    @inEndDate, @inStartTime, @inEndTime, @isAutoSchedule, @seasonType, @dayCount, @createUser, @createTime, @updateUser, @updateTime, @ip, @remark, 
        //                    @sort,@status,@isDelete); ");
        //            int rowResult = sqliteHelper.ExecuteNonQuery(sbSql.ToString(), new SQLiteParameter[]
        //            {
        //                new SQLiteParameter("@id",_model.Id),
        //                new SQLiteParameter("@ticketName",_model.TicketName),
        //                new SQLiteParameter("@openTicketName",_model.OpenTicketName),
        //                new SQLiteParameter("@ticketType",Convert.ToInt32( _model.TicketType)),
        //                new SQLiteParameter("@priceType",Convert.ToInt32( _model.PriceType)),
        //                new SQLiteParameter("@price",Convert.ToDecimal(_model.Price)),
        //                new SQLiteParameter("@ticketStyleId",Convert.ToInt64(_model.TicketStyleId)),
        //                new SQLiteParameter("@sellWay",Convert.ToInt32(_model.SellWay)),
        //                new SQLiteParameter("@venueId",Convert.ToInt64(_model.VenueId)) ,
        //                new SQLiteParameter("@teamType",Convert.ToInt32(_model.TeamType)),
        //                new SQLiteParameter("@validCount",Convert.ToInt32(_model.ValidCount)),
        //                new SQLiteParameter("@personCount",Convert.ToInt32(_model.PersonCount)),
        //                new SQLiteParameter("@couponTypeId",_model.CouponTypeId),
        //                new SQLiteParameter("@validDays",Convert.ToInt32( _model.ValidDays)),
        //                new SQLiteParameter("@validBeginDate",Convert.ToInt32(_model.ValidBeginDate)),
        //                new SQLiteParameter("@validEndDate",Convert.ToInt32(_model.ValidEndDate)),
        //                new SQLiteParameter("@preSellDays",Convert.ToInt32( _model.PreSellDays)),
        //                new SQLiteParameter("@sellStartDate",Convert.ToInt32(_model.SellStartDate)),
        //                new SQLiteParameter("@sellEndDate",Convert.ToInt32(_model.SellEndDate)),
        //                new SQLiteParameter("@sellStartTime",Convert.ToInt32(_model.SellStartTime)),
        //                new SQLiteParameter("@sellEndTime",Convert.ToInt32(_model.SellEndTime)),
        //                new SQLiteParameter("@inStartDate",Convert.ToInt32( _model.InStartDate)),
        //                new SQLiteParameter("@inEndDate",Convert.ToInt32( _model.InEndDate)),
        //                new SQLiteParameter("@inStartTime",Convert.ToInt32( _model.InStartTime)),
        //                new SQLiteParameter("@inEndTime",Convert.ToInt32( _model.InEndTime)),
        //                new SQLiteParameter("@isAutoSchedule",_model.IsAutoSchedule),
        //                new SQLiteParameter("@seasonType",Convert.ToInt32( _model.SeasonType)),
        //                new SQLiteParameter("@dayCount",Convert.ToInt32(_model.DayCount)),
        //                new SQLiteParameter("@createUser",Convert.ToInt64(_model.CreateUser)),
        //                new SQLiteParameter("@createTime",Convert.ToInt64(_model.CreateTime)),
        //                new SQLiteParameter("@updateUser",Convert.ToInt64(_model.UpdateUser)),
        //                new SQLiteParameter("@updateTime",Convert.ToInt64(_model.UpdateTime)),
        //                new SQLiteParameter("@ip",_model.Ip),
        //                new SQLiteParameter("@remark",Convert.ToString( _model.Remark)),
        //                new SQLiteParameter("@sort",_model.Sort),
        //                new SQLiteParameter("@status",Convert.ToInt32(_model.Status)),
        //                new SQLiteParameter("@isDelete",Convert.ToInt32(_model.IsDelete)),
        //            });
        //            if (rowResult > 0)
        //                flag = true;
        //        }
        //    }

        //    return flag;
        //}

        //private bool SaveVenue(List<VenueBM> listVenue)
        //{
        //    bool flag = false;

        //    if (listVenue != null && listVenue.Count > 0)
        //    {
        //        //1.删除本地的订单信息
        //        string sql = " delete from t_venue ";
        //        int rows = sqliteHelper.ExecuteNonQuery(sql);
        //        foreach (VenueBM _model in listVenue)
        //        {
        //            //2.保存下载的订单信息
        //            StringBuilder sbSql = new StringBuilder();
        //            sbSql.Append(@"INSERT INTO t_venue (id,venueName,venueType, stadiumId,seatCount
        //                    , logo, createUser, createTime, updateUser
        //                    , updateTime, ip, remark, status,isDelete) 
        //                    VALUES ('" + _model.Id + "','" + _model.VenueName + "','" + _model.VenueType + "', '" + _model.StadiumId + "', '" + _model.SeatCount + "', '" + _model.Logo + "' ");
        //            sbSql.Append(@", '" + _model.CreateUser + "', '" + _model.CreateTime + "', '" + _model.UpdateUser + "', '" + _model.UpdateTime + "', '" + _model.Ip + "', '" + _model.Remark + "', '" + _model.Status + "','" + _model.IsDelete + "'); ");
        //            int rowResult = sqliteHelper.ExecuteNonQuery(sbSql.ToString());
        //            //int rowResult = sqliteHelper.ExecuteNonQuery(sbSql.ToString(), new SQLiteParameter[] 
        //            //{
        //            //    new SQLiteParameter("@id",_model.Id),
        //            //    new SQLiteParameter("@venueName",_model.VenueName),
        //            //    new SQLiteParameter("@venueType",_model.VenueType),
        //            //    new SQLiteParameter("@stadiumId",Convert.ToInt64(_model.StadiumId)),
        //            //    new SQLiteParameter("@seatCount",Convert.ToInt32( _model.SeatCount)),
        //            //    new SQLiteParameter("@desp",_model.Desp),
        //            //    new SQLiteParameter("@logo",_model.Logo),
        //            //    new SQLiteParameter("@createUser",Convert.ToInt64(_model.CreateUser)), 
        //            //    new SQLiteParameter("@createTime",Convert.ToInt64(_model.CreateTime)), 
        //            //    new SQLiteParameter("@updateUser",Convert.ToInt64(_model.UpdateUser)), 
        //            //    new SQLiteParameter("@updateTime",Convert.ToInt64(_model.UpdateTime)), 
        //            //    new SQLiteParameter("@ip",_model.Ip.ToString()), 
        //            //    new SQLiteParameter("@remark",Convert.ToString( _model.Remark)), 
        //            //    new SQLiteParameter("@status",Convert.ToInt32(_model.Status)),  
        //            //    new SQLiteParameter("@isDelete",Convert.ToInt32(_model.IsDelete)),
        //            //});
        //            if (rowResult > 0)
        //                flag = true;
        //        }
        //    }

        //    return flag;
        //}

        //private bool SaveVenueDevice(List<VenueDeviceBM> listVenueDevice)
        //{
        //    bool flag = false;

        //    if (listVenueDevice != null && listVenueDevice.Count > 0)
        //    {
        //        //1.删除本地的订单信息
        //        string sql = " delete from t_venue_device ";
        //        int rows = sqliteHelper.ExecuteNonQuery(sql);
        //        foreach (VenueDeviceBM _model in listVenueDevice)
        //        {
        //            //2.保存下载的订单信息
        //            StringBuilder sbSql = new StringBuilder();
        //            sbSql.Append(@"INSERT INTO t_venue_device (id,deviceCode,deviceType, deviceName,venueId, deviceStatus
        //                    ,createUser, createTime, updateUser
        //                    , updateTime, ip, remark, status,isDelete,ticketPaperCount) 
        //                    VALUES ( ");
        //            sbSql.Append("'" + _model.id + "','" + _model.deviceCode + "','" + _model.deviceType + "','" + _model.deviceName + "' ");
        //            sbSql.Append(",'" + _model.venueId + "','" + _model.deviceStatus + "','" + _model.createUser + "','" + _model.createTime + "' ");
        //            sbSql.Append(",'" + _model.updateUser + "','" + _model.updateTime + "','" + _model.ip + "','" + _model.remark + "' ");
        //            sbSql.Append(",'" + _model.status + "','" + _model.isDelete + "','" + _model.ticketPaperCount + "'); ");
        //            int rowResult = sqliteHelper.ExecuteNonQuery(sbSql.ToString());
        //            //int rowResult = sqliteHelper.ExecuteNonQuery(sbSql.ToString(), new SQLiteParameter[] 
        //            //{
        //            //    new SQLiteParameter("@id",_model.id),
        //            //    new SQLiteParameter("@deviceCode",_model.deviceCode),
        //            //    new SQLiteParameter("@deviceType",_model.deviceType),
        //            //    new SQLiteParameter("@deviceName",_model.deviceName),
        //            //    new SQLiteParameter("@deviceStatus",Convert.ToInt32( _model.deviceStatus)),
        //            //    new SQLiteParameter("@createUser",Convert.ToInt64(_model.createUser)), 
        //            //    new SQLiteParameter("@createTime",Convert.ToInt64(_model.createTime)), 
        //            //    new SQLiteParameter("@updateUser",Convert.ToInt64(_model.updateUser)), 
        //            //    new SQLiteParameter("@updateTime",Convert.ToInt64(_model.updateTime)), 
        //            //    new SQLiteParameter("@ip",_model.ip), 
        //            //    new SQLiteParameter("@remark",Convert.ToString( _model.remark)), 
        //            //    new SQLiteParameter("@status",Convert.ToInt32(_model.status)),  
        //            //    new SQLiteParameter("@isDelete",Convert.ToInt32(_model.isDelete)),
        //            //    new SQLiteParameter("@ticketPaperCount",Convert.ToInt32(_model.ticketPaperCount)),
        //            //});
        //            if (rowResult > 0)
        //                flag = true;
        //        }
        //    }

        //    return flag;
        //}

        //private bool SaveSessions(List<SessionsBM> listSessions)
        //{
        //    bool flag = false;

        //    if (listSessions != null && listSessions.Count > 0)
        //    {
        //        //1.删除本地的订单信息
        //        string sql = " delete from t_sessions ";
        //        int rows = sqliteHelper.ExecuteNonQuery(sql);
        //        foreach (SessionsBM _model in listSessions)
        //        {
        //            //2.保存下载的订单信息
        //            StringBuilder sbSql = new StringBuilder();
        //            sbSql.Append(@"INSERT INTO t_sessions (id,sessionsDate,venueId, beginTime,endTime
        //                    , sessionsVideoId,limitCount,isSeat, createUser,createTime, updateUser
        //                    , updateTime, ip, remark, status,isDelete) 
        //                    VALUES ('" + _model.Id + "','" + _model.SessionsDate + "','" + _model.VenueId + "', '" + _model.BeginTime + "', '" + _model.EndTime + "', '" + _model.SessionsVideoId + "' ");
        //            sbSql.Append(@", '" + _model.LimitCount + "', '" + _model.IsSeat + "'");
        //            sbSql.Append(@", '" + _model.CreateUser + "', '" + _model.CreateTime + "', '" + _model.UpdateUser + "', '" + _model.UpdateTime + "', '" + _model.Ip + "', '" + _model.Remark + "', '" + _model.Status + "','" + _model.IsDelete + "'); ");
        //            int rowResult = sqliteHelper.ExecuteNonQuery(sbSql.ToString());

        //            if (rowResult > 0)
        //                flag = true;
        //        }
        //    }

        //    return flag;
        //}

        //private bool SaveSessionsVideo(List<SessionVideoBM> listSessionVideo)
        //{
        //    bool flag = false;

        //    if (listSessionVideo != null && listSessionVideo.Count > 0)
        //    {
        //        //1.删除本地的订单信息
        //        string sql = " delete from t_session_video ";
        //        int rows = sqliteHelper.ExecuteNonQuery(sql);
        //        foreach (SessionVideoBM _model in listSessionVideo)
        //        {
        //            //2.保存下载的订单信息
        //            StringBuilder sbSql = new StringBuilder();
        //            sbSql.Append(@"INSERT INTO t_session_video (id,videoName,duration, venueId
        //                    , createUser, createTime, updateUser
        //                    , updateTime, ip, remark, status,isDelete) 
        //                    VALUES ('" + _model.Id + "','" + _model.VideoName + "','" + _model.Duration + "', '" + _model.VenueId + "' ");
        //            sbSql.Append(@", '" + _model.CreateUser + "', '" + _model.CreateTime + "', '" + _model.UpdateUser + "', '" + _model.UpdateTime + "', '" + _model.Ip + "', '" + _model.Remark + "', '" + _model.Status + "','" + _model.IsDelete + "'); ");
        //            int rowResult = sqliteHelper.ExecuteNonQuery(sbSql.ToString());

        //            if (rowResult > 0)
        //                flag = true;
        //        }
        //    }

        //    return flag;
        //}

        #endregion

        #region 新增出馆人数计数统计上传

        /// <summary>
        /// 上传当前出馆人数
        /// </summary>
        /// <param name="count"></param>
        /// <param name="deviceId"></param>
        public void GetOutCount(int CheckoutPersonCount)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            dic.Add("deviceId", Config.Instance.DeviceId);
            dic.Add("count", CheckoutPersonCount);
            GetOutPersonCount(dic, act =>
            {
                //返回为1时代表成功 置空出馆人数
                if (act.Result == 1)
                {
                    Config.CheckoutPersonCount = 0;
                }
            });
        }
        /// <summary>
        /// 上传出口人数
        /// </summary>
        /// <param name="count"></param>
        /// <param name="action"></param>
        private void GetOutPersonCount(Dictionary<string, int> dic, Action<CallResult<int>> action)
        {
            HttpX.Instance.Post<int>("api/zk_check/ticket/get_outPerson_count", TextureHelper.ToJson<Dictionary<string, int>>(dic), true, arg =>
            {
                if (arg != null)
                {
                    action(arg.CallResult);
                }
            });
        }
        #endregion

        #region 获取IC卡是否存在开闸
        /// <summary>
        /// IC卡验卡
        /// </summary>
        /// <param name="icNumber"></param>
        public void ICCardCheck(string icNumber)
        {
            try
            {
                CheckResultBM checkResultBM = new CheckResultBM();
                GetIcCardCheck(icNumber, act =>
                {
                    if (act.Result == true)
                    {
                        ShowCheckResultUC(7, checkResultBM, "IC卡验证通过");
                        YLGateDev.OpenCount(Config.Instance.OpenCountCMD, Config.Instance.GateSerial, "");
                    }
                    else
                    {
                        ShowCheckResultUC(1, checkResultBM, "无效IC卡");
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.WriteLog("开闸报错:" + ex.Message);
            }
        }
        /// <summary>
        /// 获取IC卡号是否存在
        /// </summary>
        /// <param name="icCard"></param>
        /// <param name="action"></param>
        private void GetIcCardCheck(string icCard, Action<CallResult<bool>> action)
        {
            HttpX.Instance.Post<bool>("api/zk_check/ticket/icCard_check/" + icCard, "", true, arg =>
             {
                 if (arg != null)
                 {
                     action(arg.CallResult);
                 }
             });
        }


        #endregion


    }
}
