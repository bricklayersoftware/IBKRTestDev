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
using Azure.Core;
using static System.Windows.Forms.AxHost;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using static IBKRRealTimeMarketDataApp.Request;
using System.IdentityModel.Claims;

namespace IBKRRealTimeMarketDataApp
{
    public static class RequestHelper
    {
        public static string ToCustomString(this Request.RequestFlags rf)
        {
            switch (rf)
            {
                case RequestFlags.stockflag:
                    return "STK";
                case RequestFlags.putflag:
                    return "P";
                case RequestFlags.callflag:
                    return "R";
            }

            var myKey = Request.BarSizes.FirstOrDefault(x => x.Value == rf).Key;

            return myKey;
        }

        public static Request.RequestFlags ToRequestFlag(this string value)
        {

            return RequestFlags.stockflag;
        }
    }

    public class Request
    {
        public static EClientSocket clientSocket
        {
            get
            {
                return IBKRRealTimeMarketDataApp.clientSocket;
            }
        }

        public Request() {
            this.begintime = DateTime.Now;
            this.state = Request.RequestState.ACTIVE;
            allrequests.Add(this);

            this.LogRequest();
        }

        private static int _seed = 0;
        
        // unique indexer per request
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
            ACTIVE, ERROR, END, IGNORE
        }

        private RequestState _state = Request.RequestState.ACTIVE;

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

                /*
                bool exitnow = true;
                
                foreach ( Request req in allrequests )
                {
                    if (req.state == RequestState.ACTIVE) {
                        exitnow = false;
                        break;
                    }
                }

                CommandControl.exitnow = exitnow;
                */
            }
        }

        public int requestid
        {
            get
            {
                int _requestid = -1;

                RequestFlags rf = 0;

                if (sectype == "STK")
                    rf = RequestFlags.stockflag;
                else if (right == "P")
                    rf = RequestFlags.putflag;
                else if (right == "C")
                    rf = RequestFlags.callflag;

                _requestid = (int)(RequestFlags.requestid | rf | _barsize) + this.stockindex + 1;

                return _requestid;
            }

        }

        public string symbol;
        public int stockindex;
        public int optionindex; // an option request on top of stock index
        public string sectype; // OPT or STK
        public string right; // C or P
        public decimal strike;
        public string expiry;
        public string requestdate;      // trading day -- IBKR doesn't accept a single trading day as input, only a range
        public string requestbegindate // trading day start
        {
            get
            {
                return this.requestdate;
            }
        }
        public string requestenddate;   // trading day end
        public RequestFlags _barsize;
        
        public bool IsDailyBar
        {
            get
            {
                return this._barsize == RequestFlags.barsize_1day_flag;
            }
        }

        public string barsize
        {
            get
            {
                return _barsize.ToCustomString();
            }

            set
            {
                if (!BarSizes.ContainsKey(value))
                    return;

                this._barsize = BarSizes[value];
            }
        }

        // number of days requested
        public int dayscount
        {
            get
            {
                DateTime startdate = this.requestbegindate.ToDate();
                DateTime enddate = this.requestenddate.ToDate();

                TimeSpan duration = enddate - startdate;

                return duration.Days;
            }
        }

        public string requestbegindatetime; // as reported by IBKR when request ends
        public string requestenddatetime;

        public IBApi.Contract contract
        {
            get
            {
                IBApi.Contract contract = new IBApi.Contract();

                if (this.sectype == "STK")
                {
                    contract.Symbol = this.symbol;
                    contract.SecType = "STK";
                    contract.Currency = "USD";
                    contract.Exchange = "SMART";
                }
                else
                {
                    contract.Symbol = this.symbol; // "QQQ";
                    contract.SecType = "OPT";
                    contract.Exchange = "SMART";
                    contract.Currency = "USD";
                    contract.LastTradeDateOrContractMonth = this.expiry; // DateTime.Today.ToString("yyyyMMdd");
                    contract.Strike = (double) this.strike;
                    contract.Right = this.right;
                    contract.Multiplier = "100";
                }

                return contract;
            }

        }

        // processing metrics
        public DateTime begintime = DateTime.Now;
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
        
        public Dictionary<string, string> ToDict()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>
            {
                  ["LoadID"]=MultipleWriter.loadid.ToString()
                , ["Symbol"]=this.symbol.ToString()
                , ["SecType"]=this.sectype != "STK" ? "OPT" : this.sectype
                , ["_Strike"]=this.strike.ToString()
                , ["_Expiry"]=this.expiry.ToString()
                , ["Right"]=this.right.ToString()
                , ["_Date"]=this.requestdate.ToString()
                , ["TimeInterval"]=this.barsize.ToString()
                , ["RequestID"]=this.requestid.ToString()
                , ["_BeginTS"]=this.requestbegindate.ToString()
                , ["_EndTS"]=this.requestenddate.ToString()
                , ["_BeginReqTS"]=this.requestbegindatetime.ToString()
                , ["_EndReqTS"]=this.requestenddatetime.ToString()
                , ["_ProcessBeginTS"]=this.begintime.ToString()
                , ["_ProcessEndReqTS"]=this.endtime.ToString()
            };

            return dict;
        }

        // single request results in multiple responses (e.g., if 5S bar size specified, or multiple trading days specified)
        public List<Dictionary<string, string>> payload = new List<Dictionary<string, string>>();

        public enum RequestFlags
        {
            requestid = 0b0111_1111_0000_0000_0000_0000_0000_0000,

            putflag   = 0b0000_0000_0001_0000_0000_0000_0000_0000,
            callflag  = 0b0000_0000_0010_0000_0000_0000_0000_0000,
            stockflag = 0b0000_0000_0011_0000_0000_0000_0000_0000,

            barsize_1secs_flag  = 0b0000_0000_0000_0001_0000_0000_0000_0000,
            barsize_5secs_flag  = 0b0000_0000_0000_0010_0000_0000_0000_0000,
            barsize_10secs_flag = 0b0000_0000_0000_0011_0000_0000_0000_0000,
            barsize_15secs_flag = 0b0000_0000_0000_0100_0000_0000_0000_0000,
            barsize_30secs_flag = 0b0000_0000_0000_0101_0000_0000_0000_0000,
            barsize_1min_flag   = 0b0000_0000_0000_0110_0000_0000_0000_0000,
            barsize_1day_flag   = 0b0000_0000_0000_0111_0000_0000_0000_0000,

            barsize_flag        = 0b0000_0000_0000_1111_0000_0000_0000_0000
        }

        public static Dictionary<string, RequestFlags> RequestFlagDict
        {
            get
            {
                return Request.BarSizes;
            }
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
                ret.Add("1 day", RequestFlags.barsize_1day_flag);
                ret.Add("STK", RequestFlags.stockflag);
                ret.Add("P", RequestFlags.putflag);
                ret.Add("C", RequestFlags.callflag);

                /* TO DO
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

        public void EndRequest()
        {
            this.endtime = DateTime.Now;
            this.state = Request.RequestState.END;
            this.endCallback(this.requestid);
        }

        public static void EndRequest(int requestid)
        {
            Request req = Request.GetRequest(requestid);
            req.EndRequest();
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

            if (req.IsDailyBar) 
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

            vals.Add(req.barsize);

            /*
            if ( ( DateTime.Now.ToString("yyyyMMdd") == dict["Time"] ) && ( DateTime.Now.Hour < 16 ) )
            {
                dict["isjuvenile"] = "true";

                log("skipping juvenile record");
                return;
            }
            */

            if ( datestr != req.requestbegindate )
            {
                req.state = RequestState.IGNORE;
            }

            int rowcount = Helper.InsertRecord("HistoricalData", cols, vals); ;            
        }

        private static Dictionary<int, Request> _requests
        {
            get
            {
                Dictionary<int, Request> dict = new Dictionary<int, Request>();

                foreach(Request req in allrequests)
                {
                    dict.Add(req.requestid, req);
                }

                return dict;
            }
        }

        public static List<int> requests
        {
            get
            {
                return _requests.Keys.ToList();
            }

        }

        public static List<Request> allrequests = new List<Request>();

        // ends session if there are no active requests
        public static void EndSession()
        {
            foreach (Request req in Request.allrequests) {
                
                if (req == null)
                    continue;

                if (req.IsActive)
                    return;
            }

            CommandControl.exitnow = true;
        }

        public static Request GetStockRequestSingleDay(string reqdate, string symbol, string barsize = "1 day", string sectype = "STK", string expirydate = "", double strike = 0, string right = "C")
        {
            // Action<string> log = Logger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().Name);

            Request req = new Request();
            req.requestdate = reqdate;
            req.stockindex = index + 1;
            req.symbol = symbol;
            req.sectype = sectype;
            req.endCallback = clientSocket.cancelHistoricalData;
            req.requestenddate = reqdate.ToDate().AddDays(0).ToString("yyyyMMdd") + " 16:00:00 US/Eastern";
            req.barsize = barsize;

            return req;
        }

        public void ExecuteStockRequestSingleDay()
        {
            // EWrapperImpl.cs --  public virtual void historicalData(int reqId, Bar bar) -- 2b7056397a90732100618619e89c82f5 
            // EWrapperImpl.cs --  public virtual void historicalDataEnd(int reqId, string startDate, string endDate) -- a3dbc45370718d8d6bf9a9ae1b1b8b58

            // https://interactivebrokers.github.io/tws-api/historical_bars.html
            // https://interactivebrokers.github.io/tws-api/historical_bars.html#hd_duration
            // https://interactivebrokers.github.io/tws-api/historical_bars.html#hd_what_to_show

            clientSocket.reqHistoricalData(this.requestid, this.contract, this.requestenddate, "1 D", this.barsize, "TRADES", 1, 1, false, null);
        }

        public static Request GetStockRequestDailyBar(EClientSocket clientSocket, string symbol, int dayscount = 30, string barsize = "1 day", string sectype="STK", string expirydate = "", double strike = 0, string right = "C")
        {
            Action<string> log = Logger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().Name);

            string days = "30 D"; // dayscount.ToString() + " D";

            if (dayscount > 0)
            {
                days = dayscount.ToString() + " D";
            }

            /*
             
             1 secs	5 secs	10 secs	15 secs	30 secs
                1 min	2 mins	3 mins	5 mins	10 mins	15 mins	20 mins	30 mins
                1 hour	2 hours	3 hours	4 hours	8 hours
                1 day
                1 week
                1 month
                */

            RequestFlags rf = 0;

            Request req = new Request();
            req.state = Request.RequestState.ACTIVE;
            req.stockindex = index + 1;
            req.symbol = symbol;
            req.sectype = sectype;
            req.endCallback = clientSocket.cancelHistoricalData;

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
            // EWrapperImpl.cs --  public virtual void historicalDataEnd(int reqId, string startDate, string endDate) -- a3dbc45370718d8d6bf9a9ae1b1b8b58

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

        public void LogRequest(bool update=true)
        {
            if ( update )
                Helper.ExecuteSP("SetLoadDetails", this.ToDict());
            else
                Helper.ExecuteSP("InitLoadDetails", this.ToDict());
        }
    }



}
