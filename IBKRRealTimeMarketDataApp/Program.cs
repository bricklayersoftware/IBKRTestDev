/*   IBKRRealTimeMarketDataApp
 *   
 *   execute and process requests by persisting responses in rdbms
 *   rolling 30 day, 1 day bars --> do initial
 *   real-time market data (5 second bars) --> used to basically measure up / down trends
 */

// https://www.programiz.com/csharp-programming/online-compiler/
// https://interactivebrokers.github.io/tws-api/classIBApi_1_1Contract.html
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
using static IBKRRealTimeMarketDataApp.ResultSet;
using System.IdentityModel.Protocols.WSTrust;


namespace IBKRRealTimeMarketDataApp
{
    public static class IBKRRealTimeMarketDataApp
    {
        public static EClientSocket clientSocket;

        public static void RetrieveSingleDay(string datestr, string symbol, string barlength="1 day")
        {
            Action<string> log = Logger.GetLogger(MethodBase.GetCurrentMethod().Name);

            log("BEGIN");

            Request req = Request.GetStockRequestSingleDay(clientSocket, datestr, symbol, barsize: barlength);

            log("END");

            return;
        }

        // populates HistoricalData table with past 30 days of data
        // grab top 19 stocks from FundHoldings, check table count for
        // past 30 days, populate as needed
        // for daily bar on equities, TimeInterval = '1D', Time = ''
        // symbol x date --> should be the same across all symbols, report otherwise
        public static void RetrieveDailyBars(int days = 30, int symbolcount = 19, string barlength = "1 day", List<string> symbols = null)
        {
            Action<string> log = Logger.GetLogger(MethodBase.GetCurrentMethod().Name);

            log("BEGIN");

            if (symbols == null || symbols.Count == 0)
            {
                if (symbolcount < 0)
                    symbols = (List<string>)Helper.GetQQQSymbols(); //.Take(symbolcount).ToList();
                else
                    symbols = (List<string>)Helper.GetQQQSymbols(symbolcount).Take(symbolcount).ToList();

                if (!symbols.Contains("QQQ"))
                    symbols.Add("QQQ");
            } else {
                if (symbolcount < 0)
                    symbolcount = symbols.Count;

                symbols = symbols.Take(symbolcount).ToList();
            }

            log("symbols: " + string.Join(",", symbols));

            for (int i = 0; i < symbols.Count; i++)
            {
                string symbol = symbols[i];
                symbol = symbol.Trim().ToUpper();

                Request stockreq = Request.GetStockRequestDailyBar(clientSocket, i, symbol, days, barlength);
            }

            log("END");

            return;
        }

        public static void RetrieveOptionsBars(int days = 30, int symbolcount = 19, string barlength = "1 day", List<string> symbols = null)
        {
            // each symbol needs high/low pair
            Action<string> log = Logger.GetLogger(MethodBase.GetCurrentMethod().Name);

            log("BEGIN");

            if (symbols == null || symbols.Count == 0)
            {
                symbols = (List<string>)Helper.GetQQQSymbols(symbolcount).Take(symbolcount).ToList();

                if (!symbols.Contains("QQQ"))
                    symbols.Add("QQQ");
            }
            else
            {
                if (symbolcount < 0)
                    symbolcount = symbols.Count;
                symbols = symbols.Take(symbolcount).ToList();
            }

            log("symbols: " + string.Join(",", symbols));

            for (int i = 0; i < symbols.Count; i++)
            {
                string symbol = symbols[i];
                symbol = symbol.Trim().ToUpper();

                // Request stockreq = Request.GetStockRequestDailyBar(clientSocket, i, symbol, days, barlength, "OPT", );

                //Dictionary<string, (decimal high, decimal low)> dailybars = Helper.GetQQQDailyBars();
                //Dictionary<(string, string), OptionsChain> optionschain = OptionsChainHelper.GetOptionsChain();

            }

            log("END");

            return;
        }

        public static void RetrieveMarketData()
        {/*
            Console.WriteLine("RetrieveMarketData :: BEGIN");

            IBApi.Contract contract = new IBApi.Contract();
            contract.Symbol = "";
            contract.SecType = "STK";
            contract.Currency = "USD";
            contract.Exchange = "SMART";

                IBApi.Contract optcontract = new IBApi.Contract();
                optcontract.Symbol = contract.Symbol; // "QQQ";
                optcontract.SecType = "OPT";
                optcontract.Exchange = "SMART";
                optcontract.Currency = "USD";
                optcontract.LastTradeDateOrContractMonth = "20250704"; // DateTime.Today.ToString("yyyyMMdd");
                optcontract.Strike = 530;
                optcontract.Right = "P";
                optcontract.Multiplier = "100";

            List<string> symbols = Helper.GetQQQSymbols(); //  dailybars.Keys.ToList(); // Helper.GetSymbols();

            Dictionary<string, (decimal high, decimal low)> dailybars = Helper.GetQQQDailyBars();
            Dictionary<(string, string), OptionsChain> optionschain = OptionsChainHelper.GetOptionsChain();

            if (dailybars == null || dailybars.Keys.Count == 0)
            {
                throw new Exception("dailybars is not populated");
            }

            Console.WriteLine("dailybars: " + dailybars.Keys.Count);

            List<string> _symbols = dailybars.Keys.ToList();

            symbols = _symbols.Intersect(symbols).ToList();

            Console.WriteLine("symbols: " + String.Join(", ", symbols.ToArray()));

            int nlim = 20;
            nlim = symbols.Count < nlim ? symbols.Count : nlim;

            string outstr = "";

            for (int i = 0; i < nlim; i++)
            {
                string symbol = symbols[i];

                Console.WriteLine("RetrieveMarketData :: processing symbol " + symbol + " index: " + i);

                OptionsChain putchain = optionschain[(symbol, "P")];
                OptionsChain callchain = optionschain[(symbol, "C")];

                contract.Symbol = symbol;

                optcontract.Symbol = contract.Symbol;

                decimal high = dailybars[symbol].high;
                decimal low = dailybars[symbol].low;


                int ihigh = (int)Math.Truncate(high) + 1;
                int ilow = (int)Math.Truncate(low) - 1;

                Console.WriteLine("symbol: " + symbol + " high: " + high.ToString() + " low: " + low.ToString() + " ilow: " + ilow + " ihigh: " + ihigh);

                List<decimal> strikes = putchain.GetStrikes(ilow, ihigh);
                string exprstr = putchain.GetExpiration("20250714").Item2;

                optcontract.LastTradeDateOrContractMonth = exprstr;

                Console.WriteLine("symbol: " + symbol + " expiry date: " + exprstr);

                Request stockreq = new Request();
                (int _requestid, outstr) = stockreq.GetStockRequest(i, symbol);

                Console.WriteLine(outstr);

                clientSocket.reqHistoricalData(_requestid, contract, "", "10 D", "1 day", "TRADES", 1, 1, false, null);
                clientSocket.reqHistoricalData(_requestid, contract, "", "1 D", "5 secs", "TRADES", 1, 1, false, null);

                // clientSocket.reqRealTimeBars((int) _requestid, contract, 5, "TRADES", false, null);

                for (int j = 0; j < strikes.Count; j++)
                {
                    decimal strike = strikes[j];
                    optcontract.Strike = decimal.ToDouble(strike); // .ToString("#.##");

                    var req = stockreq.GetOptionRequest(i, symbol, j, strike, exprstr);

                    (_requestid, outstr) = req.Item1;
                    Console.WriteLine(outstr);

                    optcontract.Right = "P";
                    //clientSocket.reqRealTimeBars((int) _requestid, optcontract, 5, "TRADES", true, null);
                    clientSocket.reqHistoricalData(_requestid, optcontract, "", "1 D", "5 secs", "TRADES", 1, 1, false, null);


                    (_requestid, outstr) = req.Item2;
                    Console.WriteLine(outstr);

                    optcontract.Right = "C";
                    //clientSocket.reqRealTimeBars(_requestid, optcontract, 5, "TRADES", true, null);
                    clientSocket.reqHistoricalData(_requestid, optcontract, "", "1 D", "5 secs", "TRADES", 1, 1, false, null);

                }

            }
            */
        }

        public static void PopulateMissingDays()
        {
            Action<string> log = Logger.GetLogger(MethodBase.GetCurrentMethod().Name);

            log("BEGIN");

            ResultSet rs = Helper.ExecuteSQL(Helper.query_HistoricalDataCountCheck);
            var dict = rs.GetRecordsByField();

            string[,] table = rs.GetTable();

            // cols 5,6,7 where col 12 is error
            // Symbol, Date, TimeInterval
            int rowcount = 0;
            int colcount = 0;

            colcount = table.GetLength(1);
            rowcount = table.GetLength(0);

            log("missing days: columns: " + table.GetLength(1));
            log("missing days: rows: " + table.GetLength(0));

            for (int i = 0; i < rowcount; i++)
            {
                string symbol = "";
                string datestr = "";
                string timeinterval = "";

                for (int j = 0; j < colcount; j++)
                {
                    RSColumn column = rs.columns[j];

                    // log("column: " + j.ToString() + " " + column.fieldname);

                    if (j == 5-1)
                        symbol = table[i, j];
                    else if (j == 6-1)
                        datestr = table[i, j];
                    else if (j == 7-1)
                        timeinterval = table[i, j];

                    if (timeinterval == "1D")
                        timeinterval = "1 day";
                    else if (timeinterval == "5S")
                        timeinterval = "5 secs";

                    if (j == 12-1)
                    {
                        if (table[i, j] != "ERROR")
                            continue;

                        log("row: " + i.ToString());

                        // "8/26/2025 12:00:00 AM"

                        datestr = datestr.Substring(0, 10);
                        string[] parts = datestr.Split('/');
                        parts[1] = (parts[1].Length == 1 ? "0" : "") + parts[1];
                        parts[0] = (parts[0].Length == 1 ? "0" : "") + parts[0];
                        datestr = parts[2].Trim() + parts[0] + parts[1];
                        symbol = symbol.Trim();
                        RetrieveSingleDay(datestr, symbol, timeinterval);
                    }
                }
            }

            log("END");

        }

        public static void TestIBKR()
        {
            EWrapperImpl testImpl = new EWrapperImpl();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;
            //! [connect]
            clientSocket.eConnect("52.188.185.179", 7496, 0);
            //! [connect]
            //! [ereader]
            //Create a reader to consume messages from the TWS. The EReader will consume the incoming messages and put them in a queue
            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();
            //Once the messages are in the queue, an additional thread can be created to fetch them
            new Thread(() => { while (clientSocket.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();
            //! [ereader]
            /*************************************************************************************************************************************************/
            /* One (although primitive) way of knowing if we can proceed is by monitoring the order's nextValidId reception which comes down automatically after connecting. */
            /*************************************************************************************************************************************************/
            while (testImpl.NextOrderId <= 0) { }
            //testIBMethods(clientSocket, testImpl.NextOrderId);            
            Console.WriteLine("Disconnecting...");
            clientSocket.eDisconnect();
            return;
        }

        public static int Main(string[] args)
        {
            Action<string> log = Logger.GetLogger(MethodBase.GetCurrentMethod().Name);

            dynamic jsonconfig = null;

            using (StreamReader r = new StreamReader("runtimeconfig.json"))
            {
                string jsonstr = r.ReadToEnd();
                jsonconfig = JsonSerializer.Deserialize<dynamic>(jsonstr);
                Dictionary<string, object> myDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonstr);

            }

            // System.Diagnostics.Debugger.Launch();

            Console.SetOut(new MultipleWriter());

            EWrapperImpl testImpl = new EWrapperImpl();

            clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            // TestIBKR();
            clientSocket.eConnect("52.188.185.179", 7496, 2);             
            // clientSocket.eConnect("127.0.0.1", 7496, 0);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();

            new Thread(() => {
                while (clientSocket.IsConnected())
                {
                    readerSignal.waitForSignal();
                    reader.processMsgs();
                }
            })
            {
                IsBackground = true
            }.Start();

            while (testImpl.NextOrderId <= 0) { }

            new Thread(() => {
                PopulateMissingDays();

                /*
                RetrieveDailyBars(2);
                // RetrieveDailyBars(15, -1, "5 secs");
                //RetrieveSingleDay("20250815", "AAPL");
                //RetrieveSingleDay("20250816", "AAPL");
                //RetrieveSingleDay("20250817", "AAPL");
                //RetrieveSingleDay("20250818", "AAPL");

                // RetrieveSingleDay("20250820", "AAPL", "5 secs");

                // RetrieveDailyBars(3, 1, "5 secs", new List<string> { "AAPL" });
                RetrieveDailyBars(days:2, barlength:"5 secs");
                // RetrieveDailyBars(1, 19, "5 secs");           
                */

            }).Start();


            new Thread(() =>
            {
                CommandControl.StartControl();
            }).Start();

            // loop if there is a single active request (i.e., terminate
            // when all requests are either in error or have ended)
            while ( ! CommandControl.exitnow )
            {
                Thread.Sleep(1000);

                log("pulse " + Helper.timestamp);         
            }

            return 0;
        }

    }
}
