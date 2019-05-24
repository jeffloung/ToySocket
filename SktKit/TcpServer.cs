using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace STLib
{
    public delegate void SessionHandler(AppSession session);
    public class TcpServer
    {
        /// <summary>
        /// 服务端接收到客户端发送事件
        /// </summary>
        public event SktTcpReceiveHandler NewRequestReceviced;
        /// <summary>
        /// 新客户端连接事件
        /// </summary>
        public event SessionHandler NewSessionConnected;
        /// <summary>
        /// 客户端关闭事件
        /// </summary>
        public event SessionHandler SessionClosed;
        /// <summary>
        /// 错误事件
        /// </summary>
        public event SktError OnError;

        /// <summary>
        /// 连接后是否发送欢迎信息
        /// </summary>
        public bool isNeedHello = false;
        /// <summary>
        /// 接收到新消息是否需要回执
        /// </summary>
        public bool isNeedReply = false;
        /// <summary>
        /// 字符编码
        /// </summary>
        public Encoding encoding = Encoding.UTF8;
        /// <summary>
        /// 服务器默认监听端口
        /// </summary>
        public int port = 6801;
        

        Socket tcpSrv;
        bool isListening = false;
        Thread serverListen;
        public Dictionary<string, AppSession> dic_ClientSocket = new Dictionary<string, AppSession>();
        private Dictionary<string, Thread> dic_ClientThread = new Dictionary<string, Thread>();

        /// <summary>
        /// 以默认端口启动监听
        /// </summary>
        public void Start()
        {
            Start(port);
        }
        /// <summary>
        /// 使用指定端口启动监听
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port)
        {
            try
            {
                this.port = port;
                IPAddress anyIP = IPAddress.Any;
                IPEndPoint ipe = new IPEndPoint(anyIP, this.port);
                tcpSrv = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpSrv.Bind(ipe);
                tcpSrv.Listen(999);

                serverListen = new Thread(ListenConnecting);
                serverListen.IsBackground = true;
                serverListen.Start();

                isListening = true;
            }
            catch (SocketException ex)
            {
                if (OnError != null)
                {
                    OnError(ex.Message);
                }
            }
        }
        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            isListening = false;
            lock (dic_ClientThread)
            {
                foreach (var item in dic_ClientThread)
                {
                    item.Value.Abort();
                }
                dic_ClientThread.Clear();
            }
            lock (dic_ClientSocket)
            {
                foreach (var item in dic_ClientSocket)
                {
                    item.Value.Close();
                }
                dic_ClientSocket.Clear();
            }
            
            Thread.Sleep(1000);
            //serverListen.Abort();
            if (tcpSrv != null)
            {
                tcpSrv.Close();
                //tcpSrv = null;
            }
        }
        void ListenConnecting()
        {
            while (isListening)
            {
                try
                {
                    Console.WriteLine("s:"+tcpSrv.Available);
                    if (!isListening) return;
                    Socket s = tcpSrv.Accept();
                    AppSession appSession = new AppSession(s);
                    Thread newConnect = new Thread(Accept);
                    //newConnect.Start(appSession);
                    Thread recData = new Thread(ReceviceData);
                    recData.Start(appSession);

                    string str_EndPoint = s.RemoteEndPoint.ToString();
                    dic_ClientThread.Add(str_EndPoint, recData);
                    dic_ClientSocket.Add(str_EndPoint, appSession);
                    Console.WriteLine("e:"+tcpSrv.Available);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex);
                }
                Thread.Sleep(200);
            }
            
        }

        void Accept(object appSession)
        {
            try
            {
                AppSession s = appSession as AppSession;
                if (NewSessionConnected != null)
                {
                    NewSessionConnected(s);
                }
                if (isNeedHello)
                {
                    Socket skt = s.GetSession();
                    string he = "hello!" + s.IPPort;
                    byte[] buffer = encoding.GetBytes(he);
                    skt.Send(buffer, buffer.Length, SocketFlags.None);
                }
            }
            catch(SocketException ex)
            {
                if (OnError != null)
                {
                    OnError(ex.Message);
                }
            }
        }
        void ReceviceData(object appSession)
        {
            while (isListening)
            {
                AppSession s = appSession as AppSession;
                Socket skt = s.GetSession();
                byte[] msgRec = new byte[1024];
                int length = skt.Receive(msgRec);
                if (length == 0)
                {
                    skt.Close();
                    if (SessionClosed != null)
                    {
                        SessionClosed(s);
                    }
                    break;
                }
                else
                {
                    string command = encoding.GetString(msgRec, 0, length);
                    if (NewRequestReceviced != null)
                    {
                        NewRequestReceviced(s, command);
                    }
                    if (isNeedReply)
                    {
                        byte[] send = encoding.GetBytes("rec data:" + command);
                        skt.Send(send, send.Length, 0);
                    }
                }
            }
        }
    }
}
