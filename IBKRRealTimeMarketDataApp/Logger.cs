using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBKRRealTimeMarketDataApp
{
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
