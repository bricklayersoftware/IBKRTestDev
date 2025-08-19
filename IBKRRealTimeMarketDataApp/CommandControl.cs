using System;
using IBApi;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using Microsoft.Data.SqlClient;
using static System.Net.WebRequestMethods;
using System.Data;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.CodeDom;
using System.Net;      //required
using System.Net.Sockets;
using System.Linq;    //required
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using System.Diagnostics.Contracts;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using System.Drawing;
using System.Reflection;
using System.Linq;

namespace IBKRRealTimeMarketDataApp
{
    public static class CommandControl
    {
        public static string timestamp
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMddHHMMss");
            }
        }

        public static bool exitnow = false;

        public static void StartControl()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 9999);
            server.Start();

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                NetworkStream ns = client.GetStream();

                byte[] hello = new byte[100];
                hello = Encoding.Default.GetBytes("tcp server: hello world! " + CommandControl.timestamp);
                ns.Write(hello, 0, hello.Length);

                while (client.Connected)
                {
                    byte[] msg = new byte[1024];
                    ns.Read(msg, 0, msg.Length);
                    string msgin = Encoding.UTF8.GetString(msg);

                    if (msgin.Length != 0)
                    {
                        Console.WriteLine("tcp client [" + CommandControl.timestamp + "] :: " + msgin);
                    }

                    if (msgin == "EXIT")
                    {
                        exitnow = true;
                    }
                }

            }
            
        }
    }


}
