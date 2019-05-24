using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace STLib
{
    public class UdpClient
    {
        /// <summary>
        /// 接收udp消息事件
        /// </summary>
        public event SktUpdReceiveHandler NewReceive;
        /// <summary>
        /// 错误事件
        /// </summary>
        public event SktError OnError;
        System.Net.Sockets.UdpClient udpClient;
        /// <summary>
        /// 本机端口
        /// </summary>
        public int localPort = 6802;
        IPEndPoint localIpep;
        /// <summary>
        /// 连接远程udp服务端端口
        /// </summary>
        public int remotePort = 6801;
        /// <summary>
        /// 连接远程udp服务端ip
        /// </summary>
        public string remotIP = "127.0.0.1";
        IPEndPoint remoteIpep;
        /// <summary>
        /// 字符编码
        /// </summary>
        public Encoding encoding = Encoding.UTF8;
        Thread sendTh;
        Thread recTh;
        bool isRuning = false;

        public void Start()
        {
            localIpep = new IPEndPoint(IPAddress.Any, localPort); // 本机IP，指定的端口号  
            udpClient = new System.Net.Sockets.UdpClient(localIpep);
            remoteIpep = new IPEndPoint(IPAddress.Parse(remotIP), remotePort); // 发送到的IP地址和端口号  

            recTh = new Thread(ReceiveMessage);
            recTh.Start();
            isRuning = true;
        }

        /// <summary>
        /// 向服务器发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMsg(string msg)
        {
            sendTh = new Thread(Send);
            sendTh.Start(msg);

        }
        void Send(object msg)
        {
            try
            {
                byte[] sendbytes = encoding.GetBytes(msg.ToString());
                udpClient.Send(sendbytes, sendbytes.Length, remoteIpep);
            }
            catch (SocketException e)
            {
                if (OnError != null)
                {
                    OnError(e.Message);
                }
            }
        }

        void ReceiveMessage()
        {
            while (isRuning)
            {
                try
                {
                    if (udpClient.Available < 1) continue;
                    if (udpClient == null) return;
                    byte[] bytRecv = udpClient.Receive(ref remoteIpep);
                    string message = encoding.GetString(bytRecv, 0, bytRecv.Length);
                    if (NewReceive != null)
                    {
                        NewReceive(remoteIpep, message);
                    }
                }
                catch (SocketException ex)
                {
                    if (OnError != null)
                    {
                        OnError(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 关闭实例
        /// </summary>
        public void Stop()
        {
            isRuning = false;
            udpClient.Close();
            udpClient = null;
        }
    }
}
