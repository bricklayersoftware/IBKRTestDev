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
using static System.Windows.Forms.LinkLabel;

namespace IBKRRealTimeMarketDataApp
{
    public class MultipleWriter : TextWriter
    {
        public static string loaddatetime = Helper.timestamp;
        public static int _index = 1;
        public static int index
        {
            get
            {
                return _index++;
            }
        }

        StreamWriter writer;
        TextWriter old;
        StringBuilder sb = new StringBuilder("");

        public MultipleWriter()
        {
            Console.WriteLine("MultipleWriter: " + Assembly.GetExecutingAssembly().Location);

            var dtstr = DateTimeOffset.Now.ToString("yyyyMMddHHmmssffff");
            string fname = "Logs\\log_" + dtstr + ".txt";

            Console.WriteLine(fname);

            bool exists = System.IO.Directory.Exists("Logs");

            if (!exists)
            {
                Console.WriteLine("creating directory Logs");
                var dirinfo = System.IO.Directory.CreateDirectory("Logs");
                Console.WriteLine("FullName: " + dirinfo.FullName);
            }


            var ostrm = new FileStream(fname, FileMode.OpenOrCreate, FileAccess.Write);
            Console.WriteLine("full path: " + ostrm.Name);

            old = Console.Out;
            writer = new StreamWriter(ostrm);
        }

        private void Writesql(string line)
        {
            if (line.Length <= 1 || string.IsNullOrWhiteSpace(line) )
                return;

            Helper.InsertRecord("LoadLog", new List<string> { "LoadDateTime", "MessageID", "Message" }, new List<string> { loaddatetime, MultipleWriter.index.ToString(), line });
        }

        public override void Write(char value)
        {
            // 0x0A, 0x0D

            string line = "";

            if (value == '\r' || value == '\n')
            {
                line = sb.ToString();
                sb = new StringBuilder();

                Writesql(line);
            }
            else
                sb.Append(value);

            writer.Write(value);
            old.Write(value);
        }

        public override void Write(string value)
        {
            Writesql(value);

            writer.Write(value);
            old.WriteLine(value);
        }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.ASCII;
            }
        }
    }


    public class Logger
    {
        public static string timestamp
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMddHHMMss");
            }
        }

        public static Action<string> log;

        public static Action<string> GetLogger(string methodName)
        {
            Action<string> logger = (msg) =>
            {
                Console.WriteLine(methodName + " :: [" + timestamp + "] :: " + msg);
            };

            return logger;
        }

    }
}
