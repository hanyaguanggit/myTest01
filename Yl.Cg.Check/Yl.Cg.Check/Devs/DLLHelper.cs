using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace L.Check.Devs
{
    public class DLLHelper
    {
        #region VID和PID连接com口
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct HANDLE
        {
            public int unused;
        }
        #endregion
        [DllImport("CH9326DLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern void CH9326CloseDevice(HANDLE hDeviceHandle);//关闭那啥
        //[DllImport("CH9326DLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        //public static extern HANDLE CH9326OpenDevice(ushort USB_VID, ushort USB_PID);//通过VID和PID打开设备
        [DllImport("CH9326DLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern bool CH9326SetRate(HANDLE hDeviceHandle, byte ucRate, byte ucCheck, byte ucStop, byte ucData, byte ucInterval);//设置波特率
        [DllImport("CH9326DLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern bool CH9326InitThreadData(HANDLE hDeviceHandle);//初始化线程

        [DllImport("CH9326DLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern ulong CH9326GetBufferLen(HANDLE hDeviceHandle, ref ushort InputReportLen, ref ushort OutputReportLen);//返还硬件能够接受的长度


        [DllImport("CH9326DLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern bool CH9326ReadData(HANDLE hDeviceHandle, ref byte[] buff, ref int ReadLen, HANDLE hd);//读数据
        [DllImport("CH9326DLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern HANDLE CH9326OpenDevice(ushort USB_VID, ushort USB_PID);


        [DllImport("CH9326DLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern bool CH9326ReadThreadData(HANDLE hDeviceHandle, ref byte[] buff, ref int ReadLen);//读数据


        [DllImport("CH9326DLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern bool CH9326WriteData(HANDLE hDeviceHandle, byte[] buff, int ReadLen, HANDLE hd);//写数据
        [DllImport("CH9326DLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern void CH9326ClearThreadData(HANDLE hDeviceHandle);  //设备句柄
        [DllImport("CH9326DLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern bool CH9326ReadData(HANDLE hDeviceHandle, ref char[] ReadBuffer, ref int pReadLen, HANDLE hEventObject);




        [DllImport("CH9326DLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern bool CH9326GetAttributes(HANDLE hDeviceHandle, ref ushort pVID, ref ushort pPID, ref ushort pVer);//返还读卡器VID、PID



        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern HANDLE CH9326OpenDevices(ushort USB_VID, ushort USB_PID);//返还读卡器VID、PID
        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int FindCard();//寻卡

        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SelectCard();//选卡

        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int ReadBaseMsg(byte[] pucManaInfo, byte[] phone);//返还读卡器VID、PID


        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern byte[] mCharToBcd(byte[] mychar);//返还读卡器VID、PID

        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int Come();//返还读卡器VID、PID


        [DllImport("WltRS.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetBmp(byte[] Wlt_File, int intf);//读取追加地址信息0x90成功 0x91无数据


        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SetUpAntenna();//读取追加地址信息0x90成功 0x91无数据

        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int OPenAntenna();//读取追加地址信息0x90成功 0x91无数据

        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SetUpAgreement();//读取追加地址信息0x90成功 0x91无数据
        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SetUID(byte[] pucManaInfo);//读取追加地址信息0x90成功 0x91无数据

        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetUpICAgreement();//设置IC卡协议


        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetICUID(byte[] pucManaInfo);//设置IC卡协议


        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Fingerprint(byte[] pucManaInfo, byte[] pucManaInfoa, byte[] pucManaInfoaa);//返还读卡器VID、PID

        [DllImport("SuperReader.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int ReadSAM_ID(byte[] pucManaInfo);//SAM号

    }
}
