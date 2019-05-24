using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace STLib
{
    public class AppSession
    {
        Socket TcpSocket;
        public string SessionId { get; private set; }
        public string IP { get; private set; }
        public string IPPort { get; private set; }

        public AppSession(Socket skt)
        {
            TcpSocket = skt;
            IPEndPoint ipe = (IPEndPoint)skt.RemoteEndPoint;
            IP = ipe.Address.ToString();
            IPPort = skt.RemoteEndPoint.ToString();
            SessionId = Guid.NewGuid().ToString("D");
        }

        public Socket GetSession()
        {
            return TcpSocket;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buf"></param>
        public void Send(byte[] buf)
        {
            if (buf != null)
            {
                TcpSocket.Send(buf);
            }
        }

        public void Close()
        {
            //TcpSocket.Shutdown(SocketShutdown.Both);
            TcpSocket.Close();
        }
    }
}
