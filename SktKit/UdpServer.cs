using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace STLib
{
    public class UdpServer
    {
        /// <summary>
        /// 接收消息事件
        /// </summary>
        public event SktUpdReceiveHandler NewRequestReceived;
        /// <summary>
        /// 错误事件
        /// </summary>
        public event SktError OnError;
        List<EndPoint> points;
        /// <summary>
        /// 监听端口
        /// </summary>
        public int ServerPort = 6801;
        /// <summary>
        /// 字符编码
        /// </summary>
        public Encoding encoding = Encoding.UTF8;

        Socket udpServer;
        Thread receive;
        bool isListening = false;

        /// <summary>
        /// 使用默认端口启动
        /// </summary>
        public void Start()
        {
            Start(ServerPort);
        }
        /// <summary>
        /// 使用指定端口启动
        /// </summary>
        public void Start(int port)
        {
            try
            {
                this.ServerPort = port;
                points = new List<EndPoint>();
                udpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                udpServer.Bind(new IPEndPoint(IPAddress.Any, this.ServerPort));
                receive = new Thread(Receive);
                receive.Start();
                isListening = true;
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
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            isListening = false;
            udpServer.Close();
            udpServer = null;
        }

        void Receive()
        {
            while (isListening)
            {
                try
                {
                    if (udpServer.Available < 1) continue;
                    if (udpServer == null) return;
                    EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] buffer = new byte[1024];
                    int length = udpServer.ReceiveFrom(buffer, ref endPoint);
                    if (!points.Contains(endPoint))
                    {
                        points.Add(endPoint);
                    }
                    string msg = encoding.GetString(buffer, 0, length);
                    if (NewRequestReceived != null)
                    {
                        NewRequestReceived((IPEndPoint)endPoint, msg);
                    }
                }
                catch (SocketException e)
                {
                    if (OnError != null)
                    {
                        OnError(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg">消息</param>
        public void Send(EndPoint ip, string msg)
        {
            try
            {
                //EndPoint s = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 60546);
                udpServer.SendTo(encoding.GetBytes(msg), ip);
            }
            catch (SocketException se)
            {
                if (OnError != null)
                {
                    OnError(se.Message);
                }
            }
        }
        /// <summary>
        /// 向所有客户端发送消息
        /// </summary>
        /// <param name="msg">消息</param>
        public void SendAll(string msg)
        {
            try
            {
                foreach (EndPoint p in points)
                {
                    udpServer.SendTo(encoding.GetBytes(msg), p);
                }
            }
            catch (SocketException e)
            {
                if (OnError != null)
                {
                    OnError(e.Message);
                }
            }
        }
    }
}
