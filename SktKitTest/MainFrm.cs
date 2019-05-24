using STLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace SktKitTest
{
    public partial class MainFrm : Form
    {
        public MainFrm()
        {
            InitializeComponent();
        }

        UdpServer udpServer;
        UdpClient udpClient;
        TcpServer tcpServer;
        TcpClient tcpClient;
        Dictionary<string, AppSession> clients;
        //SerialPortKit spk;
        private void MainFrm_Load(object sender, EventArgs e)
        {
            clients = new Dictionary<string, AppSession>();
            udpClient = new UdpClient()
            {
                remotIP = "192.168.7.10"
            };
            udpClient.NewReceive += UdpClient_NewReceive;
            udpClient.OnError += UdpClient_OnError;
            //udpClient.Start();
            udpServer = new UdpServer();
            udpServer.NewRequestReceived += UdpServer_NewRequestReceived;
            udpServer.OnError += UdpServer_OnError;
            //udpServer.Start();
            tcpServer = new TcpServer
            {
                isNeedReply = true,
                isNeedHello = true
            };
            tcpServer.NewSessionConnected += TcpServer_NewSessionConnected;
            tcpServer.NewRequestReceviced += TcpServer_NewRequestReceviced;
            tcpServer.SessionClosed += TcpServer_SessionClosed;
            //tcpServer.Start();
            tcpClient = new TcpClient() { port = 8189, ip = "192.168.4.1" };
            tcpClient.SessionConnected += TcpClient_SessionConnected;
            tcpClient.OnReceive += TcpClient_OnReceive;
            tcpClient.OnError += TcpClient_OnError;
            tcpClient.SessionClosed += TcpClient_SessionClosed;
            tcpClient.Start();
            //spk = new SerialPortKit();
            //int[] nums = { 1, 5 };
            //spk.Init(nums);
            //spk.NewDataRecevied += Spk_NewDataRecevied;
            //spk.NewKitInfo += Spk_NewKitInfo;
            //spk.NewKitError += Spk_NewKitError;
        }
        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //udpClient.Stop();
            //udpServer.Stop();
            //tcpServer.Stop();
            //tcpClient.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //byte[] b = Encoding.UTF8.GetBytes("server send msg!");
            //clients[textBox1.Text].Send(b);
            tcpClient.Send("tst send !");
            //spk.SendAll(textBox1.Text);
            //spk.Send(1, textBox1.Text);
            //udpClient.SendMsg(textBox1.Text);
            //IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6802);
            //udpServer.Send(ipe, textBox1.Text);
            //tcpServer.Stop();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            tcpServer.Start();
        }


        private void TcpClient_SessionConnected(AppSession session)
        {
            Console.WriteLine("TcpClient_SessionConnected:" + session.IPPort);
        }
        private void TcpClient_SessionClosed(AppSession session)
        {
            Console.WriteLine("TcpClient_SessionClosed:" + session.IPPort);
        }
        private void TcpClient_OnError(string ErrorInfo)
        {
            Console.WriteLine("TcpClient_OnError:" + ErrorInfo);
        }

        private void UdpServer_OnError(string ErrorInfo)
        {
            Console.WriteLine("UdpServer_OnError:" + ErrorInfo);
        }

        private void UdpClient_OnError(string ErrorInfo)
        {
            Console.WriteLine("UdpClient_OnError" + ErrorInfo);
        }

        private void UdpClient_NewReceive(IPEndPoint ipe, string msg)
        {
            Console.WriteLine(msg);
        }

        private void Spk_NewKitError(string msg)
        {
            Console.WriteLine(msg);
        }

        private void Spk_NewKitInfo(string msg)
        {
            Console.WriteLine(msg);
        }

        private void Spk_NewDataRecevied(SerialPort sp, string msg)
        {
            Console.WriteLine(string.Format("{0} <-- rec:{1}", sp.PortName, msg));
        }

        private void TcpClient_OnReceive(AppSession appSession, string msg)
        {
            Console.WriteLine("TcpClient_OnReceive:" + msg);
        }

        private void TcpServer_SessionClosed(AppSession session)
        {
            Console.WriteLine("TcpServer_SessionClosed:" + session.IPPort);
        }

        private void TcpServer_NewRequestReceviced(AppSession session, string command)
        {
            Console.WriteLine("TcpServer_NewRequestReceviced:" + command);
        }

        private void TcpServer_NewSessionConnected(AppSession session)
        {
            Console.WriteLine("TcpServer_NewSessionConnected:" + session.IPPort);
            //clients.Add(session.IP, session);
        }
        

        private void UdpServer_NewRequestReceived(IPEndPoint ipe, string msg)
        {
            Console.WriteLine("udp event:" + msg);
        }

        
    }
}
