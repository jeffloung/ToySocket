using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace STLib
{
    public delegate void KitHandler(string msg);
    public delegate void ReceiveHandler(SerialPort sp, string msg);
    /// <summary>
    /// 串口实用工具unity版
    /// </summary>
    public class SerialPortKit
    {
        #region 私有属性
        /// <summary>
        /// 错误事件
        /// </summary>
        public event KitHandler OnError;
        /// <summary>
        /// 接收到新消息事件
        /// </summary>
        public event ReceiveHandler OnDataRecevied;
        /// <summary>
        /// 字符编码
        /// </summary>
        public Encoding encoding = Encoding.UTF8;
        /// <summary>
        /// 查询所有串口名字
        /// </summary>
        public List<string> AllPortName { get; private set; }
        /// <summary>
        /// 结尾符
        /// </summary>
        public char EndCharacter;
        #endregion

        #region 私有属性
        static string comBase = "COM";
        Dictionary<int, SerialPort> SerialPorts;
        Dictionary<int, Thread> SerialThreads;
        bool is0d = false;
        List<byte> listReceive;
        byte nullValue = 0x00;
        #endregion

        public SerialPortKit()
        {
            AllPortName = new List<string>();
            SerialPorts = new Dictionary<int, SerialPort>();
            SerialThreads = new Dictionary<int, Thread>();
            listReceive = new List<byte>();
            EndCharacter = Convert.ToChar(nullValue);
        }

        /// <summary>
        /// 初始化，使用所有串口
        /// </summary>
        public void Init()
        {
            List<int> numbers = new List<int>();
            foreach (string s in SerialPort.GetPortNames())
            {
                string temps = s.Substring(s.Length - 1, s.Length);
                numbers.Add(int.Parse(temps));
            }
            Init(numbers.ToArray());
        }

        /// <summary>
        /// 初始化，使用指定串口
        /// </summary>
        /// <param name="comNumbers">串口端口号</param>
        public void Init(int[] comNumbers)
        {
            try
            {
                foreach (int c in comNumbers)
                {
                    SerialPort sp = new SerialPort();
                    sp.PortName = comBase + c.ToString();
                    //sp.DataReceived += new SerialDataReceivedEventHandler(Sp_DataReceived);//unity中此事件无效
                    if (!sp.IsOpen) sp.Open();
                    Thread spt = new Thread(Sp_DataReceived);
                    spt.Start(sp);

                    AllPortName.Add(sp.PortName);
                    SerialPorts.Add(c, sp);
                    SerialThreads.Add(c, spt);
                }
            }
            catch (Exception e)
            {
                if (OnError != null)
                {
                    OnError(e.Message);
                }
            }
        }

        /// <summary>
        /// 关闭所有初始化已打开的串口
        /// </summary>
        public void CloseAll()
        {
            try
            {
                foreach (var item in SerialPorts)
                {
                    Close(item.Key);
                }
            }
            catch (Exception e)
            {
                if (OnError != null)
                {
                    OnError(e.Message);
                }
            }
        }

        /// <summary>
        /// 根所串口编号关闭端口
        /// </summary>
        /// <param name="number"></param>
        public void Close(int number)
        {
            try
            {
                SerialPorts[number].Close();
                SerialThreads[number].Abort();
            }
            catch (Exception e)
            {
                if (OnError != null)
                {
                    OnError(e.Message);
                }
            }
        }
        /// <summary>
        /// 根所串口实例关闭端口，暂时不使用，要同时abort线程
        /// </summary>
        /// <param name="sp"></param>
        public void Close(SerialPort sp)
        {
            try
            {
                string spNmae = sp.PortName;
                sp.Close();
                int id = int.Parse(spNmae.Substring(spNmae.Length - 1, spNmae.Length));
                SerialThreads[id].Abort();
            }
            catch (Exception e)
            {
                if (OnError != null)
                {
                    OnError(e.Message);
                }
            }
        }

        private void Sp_DataReceived(object spObj)
        {
            SerialPort sp = spObj as SerialPort;
            while (sp != null && sp.IsOpen)
            {
                try
                {
                    byte addr = Convert.ToByte(sp.ReadByte());
                    sp.DiscardInBuffer();
                    if (EndCharacter == Convert.ToChar(nullValue))
                    {
                        if (addr == 0x0d)
                        {
                            is0d = true;
                            continue;
                        }
                        else if (addr == 0x0a && is0d)
                        {
                            if (OnDataRecevied != null)
                            {
                                OnDataRecevied(sp, encoding.GetString(listReceive.ToArray()));
                            }
                            listReceive.Clear();
                            continue;
                        }
                        is0d = false;
                        listReceive.Add(addr);
                    }
                    else
                    {
                        if (addr == Convert.ToByte(EndCharacter))
                        {
                            //Debug.Log("产生事件，获取值并清空list，值：" + ));
                            if (OnDataRecevied != null)
                            {
                                OnDataRecevied(sp, encoding.GetString(listReceive.ToArray()));
                            }
                            listReceive.Clear();
                            continue;
                        }
                        listReceive.Add(addr);
                    }

                }
                catch (Exception e)
                {
                    if (OnError != null)
                    {
                        OnError(e.Message);
                    }
                }
            }

        }

        /// <summary>
        /// 根据串口编号发送消息
        /// </summary>
        /// <param name="ComNumber">串口端口号</param>
        /// <param name="msg">消息</param>
        public void Send(int ComNumber, string msg)
        {
            try
            {
                if (!SerialPorts.ContainsKey(ComNumber))
                {
                    throw new Exception("输入串口编号不正确");
                }
                SerialPort sp = SerialPorts[ComNumber];
                Send(sp, msg);
            }
            catch (Exception e)
            {
                if (OnError != null)
                {
                    OnError(e.Message);
                }
            }
        }

        /// <summary>
        /// 根据serialport发送消息
        /// </summary>
        /// <param name="sp">串口实例</param>
        /// <param name="msg">消息</param>
        public void Send(SerialPort sp, string msg)
        {
            try
            {
                byte[] data = encoding.GetBytes(msg);
                sp.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                if (OnError != null)
                {
                    OnError(e.Message);
                }
            }
        }

        /// <summary>
        /// 所有串口发送消息
        /// </summary>
        /// <param name="msg">消息</param>
        public void SendAll(string msg)
        {
            foreach (var s in SerialPorts)
            {
                Send(s.Value, msg);
            }
        }
    }
}
