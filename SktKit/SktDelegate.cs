using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace STLib
{
    public delegate void SktTcpReceiveHandler(AppSession session, string command);
    public delegate void SktUpdReceiveHandler(IPEndPoint ipe, string msg);
    public delegate void SktError(string ErrorInfo);
}
