using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using YL.Check.Utilities;
namespace YL.Check.Devs
{
    /// <summary>
    /// 功能说明：二维码扫描器操作类
    /// 创建人：ys
    /// 创建日期：2016-04-25 13：47
    /// </summary>
    public class YLScannerDev
    {
        LogicHandle lHandle = new LogicHandle();
        private static DateTime DateN = new DateTime();
        /// <summary>
        /// 二维码检票
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var currentSerialPort = sender as SerialPort;
            if (currentSerialPort == null) return;

            if (DateN <= DateTime.Now.AddSeconds(-1))
            {
                DateN = DateTime.Now;
                //接收数据缓冲区
                Thread.Sleep(1000);
                var readByte = new byte[10 * 1024];
                currentSerialPort.Read(readByte, 0, currentSerialPort.BytesToRead);
                var strQRCode = Encoding.Default.GetString(readByte);

                #region 二维码检票业务逻辑
                lHandle.TicketCheckByQRCode(strQRCode);
                #endregion
            }


        }

        /// <summary>
        /// 二维码串口的初始化
        /// </summary>
        public static void SerialPortInit(ref SerialPort _serialPort)
        {
            try
            {
                _serialPort = new SerialPort();
                _serialPort.PortName = Config.Instance.SerialPort; //设置通信端口
                _serialPort.BaudRate = 115200; //设置串行波特率
                _serialPort.Parity = Parity.None; //设置奇偶校验检查协议
                _serialPort.DataBits = 8; //设置每个字节的标准数据位长度
                _serialPort.StopBits = StopBits.One; //设置每个字节的标准停止位数
                _serialPort.Handshake = Handshake.None; //设置串行端口数据传输的握手协议
                DateN = DateTime.Now;
                if (_serialPort.IsOpen) _serialPort.Close();
                _serialPort.Open();
                Config.Instance.SerialPortInit = true;
            }
            catch (Exception ex)
            {
                Logger.WriteLog("初始化二维码串口时报错" + ex.Message);
                throw;
            }
        }
    }
}
