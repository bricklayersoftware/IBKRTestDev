using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using System.Security.Cryptography;

namespace IBKRRealTimeMarketDataApp
{
    public class Request
    {
        private static int _seed = 0;
        public static int index
        {
            get { return _seed++; }
        }

        public static Request GetRequest(int reqid)
        {
            if (_requests.ContainsKey(reqid))
                return _requests[reqid];

            return null;
        }

        public delegate void EndCallback(int reqId);
        public EndCallback endCallback;

        public enum RequestState
        {
            ACTIVE, ERROR, END
        }
        

        public int requestid;

        private RequestState _state;

        public RequestState state
        {
            get
            {
                return _state;
            }

            set
            {
                if (value == this._state)
                    return;

                _state = value;

                if (value == RequestState.ACTIVE)
                    return;

                this.endCallback(this.requestid);
            }
        }

        public string msg;
        public string symbol;
        public int stockindex;
        public string sectype; // OPT or STK
        public string right; // C or P
        public decimal strike;
        public string expiry;
        public int optionindex;
        public int dayscount;
        public DateTime begintime;
        public DateTime processtime;
        public DateTime endtime;
        public DateTime errortime;

        public Boolean IsActive
        {
            get
            {
                return this.state == RequestState.ACTIVE;
            }
        }
        
        public List<Dictionary<string, string>> payload = new List<Dictionary<string, string>>();

        public string barsize
        {
            get
            {
                return timeinterval;
            }
        }
        
        public string timeinterval
        {
            get
            {
                int barsize = this.requestid & (int)RequestFlags.barsize_flag;

                if (barsize == (int)RequestFlags.barsize_1day_flag)
                    return "1D";

                if (barsize == (int)RequestFlags.barsize_5secs_flag)
                    return "5S";

                if (barsize == (int)RequestFlags.barsize_30secs_flag)
                    return "30S";

                return "";
            }
        }

        public enum RequestFlags
        {
            requestid = 0b0111_1111_0000_0000_0000_0000_0000_0000,
            putflag = 0b0000_0000_0001_0000_0000_0000_0000_0000,
            callflag = 0b0000_0000_0010_0000_0000_0000_0000_0000,
            stockflag = 0b0000_0000_0100_0000_0000_0000_0000_0000,

            barsize_1secs_flag  = 0b0000_0000_0000_0001_0000_0000_0000_0000,
            barsize_5secs_flag  = 0b0000_0000_0000_0010_0000_0000_0000_0000,
            barsize_10secs_flag = 0b0000_0000_0000_0011_0000_0000_0000_0000,
            barsize_15secs_flag = 0b0000_0000_0000_0100_0000_0000_0000_0000,
            barsize_30secs_flag = 0b0000_0000_0000_0101_0000_0000_0000_0000,
            barsize_1min_flag   = 0b0000_0000_0000_0110_0000_0000_0000_0000,
            barsize_1day_flag   = 0b0000_0000_0000_0111_0000_0000_0000_0000,

            barsize_flag        = 0b0000_0000_0000_1111_0000_0000_0000_0000
        }

        public static Dictionary<string, RequestFlags> BarSizes
        {
            get
            {
                Dictionary<string, RequestFlags> ret = new Dictionary<string, RequestFlags>();

                ret.Add("1 secs", RequestFlags.barsize_1secs_flag);
                ret.Add("5 secs", RequestFlags.barsize_5secs_flag);
                ret.Add("10 secs", RequestFlags.barsize_10secs_flag);
                ret.Add("15 secs", RequestFlags.barsize_15secs_flag);
                ret.Add("30 secs", RequestFlags.barsize_30secs_flag);
                ret.Add("1 min", RequestFlags.barsize_1min_flag);

                /*
                1 min   
                2 mins  
                3 mins  
                5 mins  
                10 mins 
                15 mins 
                20 mins 
                30 mins
                1 hour  
                2 hours 
                3 hours 
                4 hours 
                8 hours
                1 day
                1 week
                1 month
                */

                return ret;
            }
        }

        public RequestFlags GetRequestFlag(string requeststr)
        {
            if ( BarSizes.ContainsKey(requeststr))
                return BarSizes[requeststr];

            return 0;
        }

        public string GetRequestString(RequestFlags flag)
        {
            foreach (KeyValuePair<string, RequestFlags> entry in BarSizes)
            {
                if (entry.Value == flag)
                    return entry.Key;
            }

            return null;
        }

        public static string RequestFlagStr(int requestid)
        {
            string ret = "";

            foreach (RequestFlags flag in (RequestFlags[])Enum.GetValues(typeof(RequestFlags)))
            {
                if ((requestid & ((int)flag)) != 0)
                    ret += flag.ToString() + "|";
            }

            return ret;
        }

        public static void EndRequest(int requestid, string msg)
        {
            Request req = Request.GetRequest(requestid);

            req.endtime = DateTime.Now;
            req.state = Request.RequestState.END; //"END";
            req.msg = msg;
            req.endtime = DateTime.Now;
            req.endCallback(requestid);
        }

        public static void InvalidateRequest(int requestid, string msg)
        {
            if (requestid < 0)
                return;

            Request req = Request.GetRequest(requestid);

            if (req == null)
                return;

            req.errortime = DateTime.Now;
            req.state = Request.RequestState.ERROR;
            req.msg = msg;
        }

        public void Process()
        {

        }

        public static void ProcessRequest(int requestid, Dictionary<string, string> dict)
        {
            Action<string> log = Logger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().Name);

            Request req = Request.GetRequest(requestid);

            req.processtime = DateTime.Now;
            req.payload.Add(dict);

            if (dict["Source"] != "historicalData")
                return;

            List<string> cols = new List<string> { "[LoadDateTime]", "[Symbol]", "[Date]", "[Time]", "[_Open]", "[_High]",
                    "[_Low]", "[_Close]", "[_Volume]", "[_Count]", "[_WAP]", "[OptionType]", "[_Strike]",
                    "[Expiry]", "[TimeInterval]" };

            List<string> vals = new List<string>();

            vals.Add(Helper.timestamp);
            vals.Add(req.symbol);

            string datestr = "";
            string timestr = "";

            if (req.barsize == "1D") 
            {
                datestr = dict["Time"];
                timestr = "";

                vals.Add(dict["Time"]);
                vals.Add("");
            }
            else
            {
                string _time = dict["Time"];

                datestr = _time.Substring(0, 8);
                timestr = _time.Substring(9, 8);

                vals.Add(datestr);
                vals.Add(timestr);
            }


            vals.Add(dict["Open"]);
            vals.Add(dict["High"]);
            vals.Add(dict["Low"]);
            vals.Add(dict["Close"]);
            vals.Add(dict["Volume"]);
            vals.Add(dict["Count"]);
            vals.Add(dict["WAP"]);

            if (req.sectype == "STK")
            {
                vals.Add(null);
                vals.Add(null);
                vals.Add(null);
            }
            else if (req.sectype == "OPT")
            {
                vals.Add(req.right);
                vals.Add(req.strike.ToString());
                vals.Add(req.expiry);
            }

            vals.Add(req.timeinterval);

            if ( ( DateTime.Now.ToString("yyyyMMdd") == dict["Time"] ) && ( DateTime.Now.Hour < 16 ) )
            {
                dict["isjuvenile"] = "true";

                log("skipping juvenile record");
                return;
            }

            // check if row exists before inserting
            if (!Helper.RowExists(req.symbol, datestr, timestr, req.timeinterval))
            {
                int rowcount = Helper.InsertRecord("HistoricalData", cols, vals);
                dict["rowcount"] = rowcount.ToString(); // Helper.InsertRecord("HistoricalData", cols, vals);
            }

            //([Symbol] --> req.Symbol
            //, [Date] dict["Time"]
            //, [Time] --> daily bar doesn't need time
            //, [_Open]
            //, [_High]
            //, [_Low]
            //, [_Close]
            //, [_Volume]
            //, [_Count]
            //, [_WAP]
            //, [OptionType]
            //, [_Strike]
            //, [Expiry]
            //, [TimeInterval])

            // Source --> historicalData
            // "HistoricalData. " + reqId
            // "Time: " + bar.Time +
            // "Open: " + Util.DoubleMaxString(bar.Open) +
            // "High: " + Util.DoubleMaxString(bar.High) +
            // "Low: " + Util.DoubleMaxString(bar.Low) +
            // "Close: " + Util.DoubleMaxString(bar.Close) +
            // "Volume: " + Util.DecimalMaxString(bar.Volume) +
            // "Count: " + Util.IntMaxString(bar.Count) +
            // "WAP: " + Util.DecimalMaxString(bar.WAP))
        }

        private static Dictionary<int, Request> _requests = new Dictionary<int, Request>();

        public static List<int> requests
        {
            get
            {
                return _requests.Keys.ToList();
            }

        }

        public static List<Request> allrequests
        {
            get
            {
                return _requests.Values.ToList();
            }
        }
        
        public static Request GetStockRequestDailyBar(EClientSocket clientSocket, int stockindex, string symbol, int dayscount = 30, string barsize = "1 day", string sectype="STK", string expirydate = "", double strike = 0, string right = "C")
        {
            Action<string> log = Logger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().Name);

            string days = "30 D"; // dayscount.ToString() + " D";

            if (dayscount > 0)
            {
                days = dayscount.ToString() + " D";
            }

            stockindex = index;

            int _requestid = 0;

            /*
             
             1 secs	5 secs	10 secs	15 secs	30 secs
                1 min	2 mins	3 mins	5 mins	10 mins	15 mins	20 mins	30 mins
                1 hour	2 hours	3 hours	4 hours	8 hours
                1 day
                1 week
                1 month
                */

            RequestFlags rf = 0;
            
            if ( sectype == "STK" )
                rf = RequestFlags.stockflag;
            else if ( right == "P" )
                rf = RequestFlags.putflag;
            else if (right == "C")
                rf = RequestFlags.callflag;

            if ( barsize == "1 day" )
                _requestid = (int) ( RequestFlags.requestid | rf | RequestFlags.barsize_1day_flag ) + stockindex + 1;
            else if ( barsize == "5 secs" )
                _requestid = (int) ( RequestFlags.requestid | rf | RequestFlags.barsize_5secs_flag ) + stockindex + 1;
            else if (barsize == "15 secs")
                _requestid = (int)(RequestFlags.requestid | rf | RequestFlags.barsize_15secs_flag) + stockindex + 1;
            else if (barsize == "30 secs")
                _requestid = (int) ( RequestFlags.requestid | rf | RequestFlags.barsize_30secs_flag ) + stockindex + 1;

            string outstr = "requestid: " + (_requestid) + " symbol: " + symbol + " bar size: " + barsize;
            outstr += " snapshot ts: " + DateTime.Now.ToString("yyyyMMddHHMMss");

            Request req = new Request();
            req.state = Request.RequestState.ACTIVE;
            req.msg = outstr;
            req.requestid = _requestid;
            req.stockindex = stockindex+1;
            req.symbol = symbol;
            req.sectype = sectype;
            req.endCallback = clientSocket.cancelHistoricalData;

            _requests.Add(_requestid, req);

            log("requesting symbol [" + req.symbol + "] with index: " + req.stockindex + " msg: " + req.msg);


            IBApi.Contract contract = new IBApi.Contract();

            if (sectype == "STK")
            {
                contract.Symbol = symbol;
                contract.SecType = "STK";
                contract.Currency = "USD";
                contract.Exchange = "SMART";
            } 
            else
            {
                contract.Symbol = symbol; // "QQQ";
                contract.SecType = "OPT";
                contract.Exchange = "SMART";
                contract.Currency = "USD";
                contract.LastTradeDateOrContractMonth = expirydate; // DateTime.Today.ToString("yyyyMMdd");
                contract.Strike = strike;
                contract.Right = right;
                contract.Multiplier = "100";
            }

            // EWrapperImpl.cs --  public virtual void historicalData(int reqId, Bar bar) -- 2b7056397a90732100618619e89c82f5 
            // EWrapperImpl.cs --  public virtual void historicalDataEnd(int reqId, string startDate, string endDate)

            // https://interactivebrokers.github.io/tws-api/historical_bars.html
            // https://interactivebrokers.github.io/tws-api/historical_bars.html#hd_duration
            // https://interactivebrokers.github.io/tws-api/historical_bars.html#hd_what_to_show

            clientSocket.reqHistoricalData(req.requestid, contract, "", days, barsize, "TRADES", 1, 1, false, null);

            return req;
        }

        public static (Request putreq, Request callreq) GetOptionRequest(int stockindex, string symbol, int optionindex, decimal strike, string expiry)
        {
            optionindex = index;

            List<Request> ret = new List<Request>();

            int _requestid = (int)(RequestFlags.requestid | RequestFlags.putflag) + (stockindex << 8) + optionindex + 1;

            string outstr = "index: " + (_requestid) + " symbol: " + symbol + " strike: " + strike.ToString();
            outstr += " expiry: " + expiry + " type: " + "P";
            outstr += " snapshot ts: " + DateTime.Now.ToString("yyyyMMddHHMMss");

            Request req = new Request();
            req.state = Request.RequestState.ACTIVE; // "ACTIVE";
            req.msg = outstr;
            req.requestid = _requestid;
            req.stockindex = stockindex;
            req.symbol = symbol;
            req.sectype = "OPT";
            req.strike = strike;
            req.expiry = expiry;
            req.optionindex = optionindex;
            req.right = "P";

            _requests.Add(_requestid, req);
            ret.Add(req);

            _requestid = (int)(RequestFlags.requestid | RequestFlags.callflag) + (stockindex << 8) + optionindex + 1;

            outstr = "index: " + (_requestid) + " symbol: " + symbol + " strike: " + strike.ToString();
            outstr += " expiry: " + expiry + " type: " + "C";
            outstr += " snapshot ts: " + DateTime.Now.ToString("yyyyMMddHHMMss");

            req = new Request();
            req.state = Request.RequestState.ACTIVE;
            req.msg = outstr;
            req.requestid = _requestid;
            req.stockindex = stockindex;
            req.symbol = symbol;
            req.sectype = "OPT";
            req.strike = strike;
            req.expiry = expiry;
            req.optionindex = optionindex;
            req.right = "P";

            _requests.Add(_requestid, req);
            ret.Add(req);

            return (ret[0], ret[1]);
        }

    }



}
