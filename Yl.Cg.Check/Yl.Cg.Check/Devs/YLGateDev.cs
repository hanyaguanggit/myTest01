using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Threading;
using YL.Check.Utilities;

namespace YL.Check.Devs
{
    /// <summary>
    /// 闸机操作类。
    /// </summary>
    /*
     * 创建人：贾增义
     * 创建时间：2016-04-12
     *
     * 修改人：（无）
     * 描述：
     * 修改时间：（无）
     */

    public class YLGateDev
    {
        #region 构造函数 & 属性 & 变量
        [DllImport("CoreDll.DLL", EntryPoint = "PlaySound", SetLastError = true)]
        internal extern static int PlaySound(string szSound, IntPtr hMod, int flags);
        internal enum Flags
        {
            SND_SYNC = 0x0000,
            SND_ASYNC = 0x0001,
            SND_NODEFAULT = 0x0002,
            SND_MEMORY = 0x0004,
            SND_LOOP = 0x0008,
            SND_NOSTOP = 0x0010,
            SND_NOWAIT = 0x00002000,
            SND_ALIAS = 0x00010000,
            SND_ALIAS_ID = 0x00110000,
            SND_FILENAME = 0x00020000,
            SND_RESOURCE = 0x00040004
        }

        [DllImport("kernel32.dll", EntryPoint = "Beep")]
        internal static extern bool Beep(int frequency, int duration);// 第一个参数是指频率的高低，越大越高，第二个参数是指响的时间毫秒。
        /// <summary>
        /// 过闸类型
        /// </summary>
        public enum PassDerEnum
        {
            未知 = 0,
            右通过 = 1,
            右超时 = 2,
            左通过 = 3,
            左超时 = 4,
            有尾随 = 5,
            读机号 = 6,
            #region 中科闸机及过闸信号

            超时 = 7,
            已通过 = 8,
            部分通过 = 9,

            #endregion

        }

        /// <summary>
        /// Instance
        /// </summary>
        public static readonly YLGateDev Instance = new YLGateDev();


        LogicHandle lHandle = new LogicHandle();
        #endregion 构造函数 & 属性 & 变量

        #region 闸机串口初始化

        /// <summary>
        /// 闸机串口初始化
        /// </summary>
        public static void SerialPortInit(SerialPort _gateSerialPort)
        {
            try
            {
                _gateSerialPort = new SerialPort();
                _gateSerialPort.PortName = Config.Instance.GateSerialPort;
                //_gateSerialPort.BaudRate = 19200;
                _gateSerialPort.BaudRate = 9600;
                _gateSerialPort.ReadBufferSize = 1024;
                _gateSerialPort.Parity = Parity.None; //设置奇偶校验检查协议
                _gateSerialPort.StopBits = StopBits.One; //设置每个字节的标准停止位数
                _gateSerialPort.Handshake = Handshake.None; //设置串行端口数据传输的握手协议
                if (_gateSerialPort.IsOpen) _gateSerialPort.Close();
                _gateSerialPort.Open();
                Config.Instance.GateInit = true;
                Config.Instance.GateSerial = _gateSerialPort;
            }
            catch (Exception ex)
            {
                Logger.WriteLog("初始化闸机串口时报错" + ex.Message);
                throw;
            }
        }

        #endregion



        #region 多方向开关闸

        /// <summary>   
        /// 右方向开闸
        /// 作者：贾增义
        /// 时间：2016-04-12 11:29
        /// </summary>
        /// <param name="cmd">右开闸命令</param>
        /// <param name="comm">COM端口</param>
        /// <param name="szSound">语音文件路径</param>
        /// <returns></returns>
        public static void OpenChnRight(string cmd, SerialPort comm, string szSound)
        {
            OpenChn(cmd, comm, szSound);
        }

        /// <summary>
        /// 右方向常开
        /// 作者：贾增义
        /// 时间：2016-04-12 11:34
        /// </summary>
        /// <param name="cmd">右常开命令</param>
        /// <param name="comm">COM端口</param>
        /// <param name="szSound">语音文件路径</param>
        /// <returns></returns>
        public static void NormallyOpenRight(string cmd, SerialPort comm, string szSound)
        {
            OpenChn(cmd, comm, szSound);
        }

        /// <summary>
        /// 右方向关闸
        /// 作者：贾增义
        /// 时间：2016-04-12 11:34
        /// </summary>
        /// <param name="cmd">右关闸命令</param>
        /// <param name="comm">COM端口</param>
        /// <param name="szSound">语音文件路径</param>
        /// <returns></returns>
        public static void CloseChnRight(string cmd, SerialPort comm, string szSound)
        {
            OpenChn(cmd, comm, szSound);
        }

        /// <summary>
        /// 左方向开闸
        /// 作者：贾增义
        /// 时间：2016-04-12 11:40
        /// </summary>
        /// <param name="cmd">左开闸命令</param>
        /// <param name="comm">COM端口</param>
        /// <param name="szSound">语音文件路径</param>
        /// <returns></returns>
        public static void OpenChnLeft(string cmd, SerialPort comm, string szSound)
        {
            OpenChn(cmd, comm, szSound);
        }

        /// <summary>
        /// 左方向常开
        /// 作者：贾增义
        /// 时间：2016-04-12 11:42
        /// </summary>
        /// <param name="cmd">左常开命令</param>
        /// <param name="comm">COM端口</param>
        /// <param name="szSound">语音文件路径</param>
        /// <returns></returns>
        public static void NormallyOpenLeft(string cmd, SerialPort comm, string szSound)
        {
            OpenChn(cmd, comm, szSound);
        }

        /// <summary>
        /// 左方向关闸
        /// 作者：贾增义
        /// 时间：2016-04-12 11:42
        /// </summary>
        /// <param name="cmd">左关闸命令</param>
        /// <param name="comm">COM端口</param>
        /// <param name="szSound">语音文件路径</param>
        /// <returns></returns>
        public static void CloseChnLeft(string cmd, SerialPort comm, string szSound)
        {
            OpenChn(cmd, comm, szSound);
        }

        /// <summary>
        /// 接受开闸命令，执行开闸操作
        /// 作者：贾增义
        /// 时间：2016-04-12 11:00
        /// </summary>
        /// <param name="cmd">闸机命令</param>
        /// <param name="comm">COM端口</param>
        /// <param name="szSound">语音文件路径</param>
        /// <returns></returns>
        private static void OpenChn(string openCmd, SerialPort comm, string szSound)
        {
            try
            {
                var mc = Regex.Matches(openCmd, @"(?i)[\da-f]{2}");
                var buf = (from Match m in mc select Convert.ToByte(m.Value, 16)).ToList();//填充到这个临时列表中
                comm.Write(buf.ToArray(), 0, buf.Count);
                if (!string.IsNullOrEmpty(szSound))//若设置有声音文件则需要播放声音
                    PlaySound(szSound, IntPtr.Zero, (int)(Flags.SND_ASYNC | Flags.SND_FILENAME));//播放声音文件
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 读取机器号
        /// 作者：贾增义
        /// 时间：2016-04-12 11:42
        /// </summary>
        /// <param name="cmd">读机器号命令</param>
        /// <param name="comm">COM端口</param>
        /// <param name="szSound">语音文件路径</param>
        /// <returns></returns>
        public static void ReadMachineNO(string openCmd, SerialPort comm, string szSound)
        {
            OpenChn(openCmd, comm, szSound);
        }

        #region 中科闸机开闸命令

        /// <summary>   
        /// 计数开闸
        /// 作者：ys
        /// 时间：2016-04-12 11:29
        /// </summary>
        /// <param name="cmd">右开闸命令</param>
        /// <param name="comm">COM端口</param>
        /// <param name="szSound">语音文件路径</param>
        /// <returns></returns>
        public static void OpenCount(string cmd, SerialPort comm, string szSound)
        {
            try
            {
                var mc = Regex.Matches(cmd, @"(?i)[\da-f]{2}");
                var buf = (from Match m in mc select Convert.ToByte(m.Value, 16)).ToList();//填充到这个临时列表中
                comm.Write(buf.ToArray(), 0, buf.Count);
                //if (!string.IsNullOrEmpty(szSound))//若设置有声音文件则需要播放声音
                //    PlaySound(szSound, IntPtr.Zero, (int)(Flags.SND_ASYNC | Flags.SND_FILENAME));//播放声音文件

            }
            catch (Exception ex)
            {
                Logger.WriteLog("OpenCount出错:" + ex.Message.ToString());
            }
        }

        #endregion


        #endregion

        #region 分析闸机反馈信号
        /// <summary>
        /// 判断是否有通过
        /// 作者：贾增义
        /// 时间：2016-03-25 11:33
        /// </summary>
        /// <returns>true：通过 false：未通过</returns>
        public static PassDerEnum IsPass_Rise(byte[] comReadBuffer)
        {
            return PassDerEnum.已通过;

            #region 注释的通过指令

            //    StringBuilder sbReadBuffer = new StringBuilder();

            //    foreach (var item in comReadBuffer)
            //    {
            //        sbReadBuffer.Append(item.ToString("X2"));
            //    }
            //    string ReadBuffer = sbReadBuffer.ToString().ToUpper();

            //    #region 中科特殊命令处理

            //    if (ReadBuffer.IndexOf("020550") >= 0)
            //    {
            //        int leftCount = Int32.Parse(ReadBuffer.Substring(ReadBuffer.IndexOf("020550") + 6, 4), System.Globalization.NumberStyles.HexNumber);  //剩余未进馆人数高位
            //        if (leftCount == 0)
            //            return PassDerEnum.已通过;
            //        else
            //            return PassDerEnum.部分通过;
            //    }

            //    #endregion

            //    else if (ReadBuffer.IndexOf(Config.Instance.PassRightCMD) >= 0)
            //    {
            //        return PassDerEnum.右通过;
            //    }
            //    else if (ReadBuffer.IndexOf(Config.Instance.TimeOutRightCMD) >= 0)
            //    {
            //        return PassDerEnum.右超时;
            //    }
            //    else if (ReadBuffer.IndexOf(Config.Instance.PassLeftCMD) >= 0)
            //    {
            //        return PassDerEnum.左通过;
            //    }
            //    else if (ReadBuffer.IndexOf(Config.Instance.TimeOutLeftCMD) >= 0)
            //    {
            //        return PassDerEnum.左超时;
            //    }
            //    else if (ReadBuffer.IndexOf(Config.Instance.FollowCMD) >= 0)
            //    {
            //        return PassDerEnum.有尾随;
            //    }
            //    else if (ReadBuffer.IndexOf(Config.Instance.FollowCMD) >= 0)
            //    {
            //        return PassDerEnum.读机号;
            //    }
            //    else
            //        return PassDerEnum.未知; 
            #endregion
        }
        #endregion

        #region 接受闸机回传信号
        public static string returnByte = "";

        LogicHandle logicHandle = new LogicHandle();
        /// <summary>
        /// 功能:闸机回传信息
        /// 作者:贾增义
        /// 时间:2016-4-24 21:05
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void _gateSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort currentSerialPort = null;
            try
            {
                Config.Instance.GateListening = true;
                currentSerialPort = sender as SerialPort;
                if (currentSerialPort == null) return;
                int dataLength = 100;
                byte[] readBuffer = new byte[currentSerialPort.ReadBufferSize];
                int bufferdata = currentSerialPort.Read(readBuffer, 0, dataLength);
                //string ReadBuffer = Encoding.UTF8.GetString(readBuffer);
                StringBuilder sbReadBuffer = new StringBuilder();
                foreach (var item in readBuffer)
                {
                    sbReadBuffer.Append(item.ToString("X2"));
                }
                string realBuffer = sbReadBuffer.ToString().Substring(0, 10).ToUpper();
                //出馆人数上传　EB01C6002C
                if (realBuffer == Config.Instance.OutCountCMD)
                {
                    returnByte += realBuffer;
                    if (returnByte.Length == 10)
                    {
                        returnByte += realBuffer;
                        return;
                    }
                    returnByte = "";
                    Config.CheckoutPersonCount += 1;
                    logicHandle.GetOutCount(Config.CheckoutPersonCount);
                }
                //入馆回调释放开闸指令
                if (realBuffer == "EB01C1002B")
                {
                    lock (Config.Instance.Qu)
                    {
                        Config.Instance.Qu.Dequeue();
                        if (Config.Instance.Qu.Count > 0)
                        {
                            OpenCount((Config.Instance.Qu.Dequeue()).ToString(), Config.Instance.GateSerial, "");
                        }
                        else
                        {
                            return;
                        }
                        logicHandle.GetOutCount(-1);// 处理开闸 人数-1 的问题
                    }
                }
                PassDerEnum pde = IsPass_Rise(readBuffer);
            }
            finally
            {
                Config.Instance.inflag = true;
                currentSerialPort.DiscardInBuffer();
                currentSerialPort.DiscardOutBuffer();
                Config.Instance.GateListening = false;
            }
            Thread.Sleep(10);
        }

        #endregion 接受闸机回传信号
    }
}
