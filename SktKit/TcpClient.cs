using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace STLib
{
    public class TcpClient
    {
        public event SktTcpReceiveHandler OnReceive;
        /// <summary>
        /// 服务端关闭事件
        /// </summary>
        public event SessionHandler SessionClosed;
        public event SessionHandler SessionConnected;
        public event SktError OnError;
        Socket SktClient;
        byte[] buffer = new byte[1024];
        /// <summary>
        /// 默认ip地址
        /// </summary>
        public string ip = "127.0.0.1";
        /// <summary>
        /// 默认端口
        /// </summary>
        public int port = 6803;
        /// <summary>
        /// 字符编码
        /// </summary>
        public Encoding encoding = Encoding.UTF8;


        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            Start(ip, port);
        }
        /// <summary>
        /// 以指定ip和端口号开启连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Start(string ip, int port) {
            try
            {
                SktClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);
                SktClient.BeginConnect(ipe, new AsyncCallback(connectCallBack), SktClient);
                SktClient.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReciveMsg), SktClient);
            }
            catch (SocketException e)
            {
                if (OnError != null)
                {
                    OnError(e.Message);
                }
            }
        }
        /// <summary>
        /// 客户端停止
        /// </summary>
        public void Stop()
        {
            SktClient.Close();
            SktClient = null;
        }

        private void connectCallBack(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            try
            {
                client.EndConnect(iar);
                if (SessionConnected != null)
                {
                    SessionConnected(new AppSession(client));
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10061)
                {
                    if (OnError != null)
                    {
                        OnError("服务器程序未运行或服务器端口未开放");
                    }
                }
                else
                {
                    if (OnError != null)
                    {
                        OnError(e.Message);
                    }
                }
            }
        }

        void ReciveMsg(IAsyncResult ar)
        {
            var skt = ar.AsyncState as Socket;
            byte[] data = new byte[1024];
            try
            {
                if (SktClient == null) return;
                var length = skt.EndReceive(ar);
                AppSession s = new AppSession(skt);
                if (length == 0)
                {
                    if (SessionClosed != null)
                    {
                        SessionClosed(s);
                    }
                }
                else
                {
                    var message = encoding.GetString(buffer, 0, length);
                    AppSession appSession = new AppSession(skt);
                    if (OnReceive != null)
                    {
                        OnReceive(appSession, message);
                    }
                    SktClient.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReciveMsg), skt);
                }
            }
            catch (Exception ex)
            {
                if (OnError != null)
                {
                    OnError(ex.Message);
                }
            }
        }

        /// <summary>
        /// 向服务器发送消息
        /// </summary>
        /// <param name="msg">消息</param>
        public void Send(string msg)
        {
            try
            {
                var outputBuffer = encoding.GetBytes(msg);
                SktClient.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
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
}
