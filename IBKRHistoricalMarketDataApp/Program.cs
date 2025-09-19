/*   IBKRHistoricalMarketDataApp
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
using static IBKRHistoricalMarketDataApp.ResultSet;
using System.IdentityModel.Protocols.WSTrust;


namespace IBKRHistoricalMarketDataApp
{
    public static class IBKRHistoricalMarketDataApp
    {
        public static EClientSocket clientSocket;

        public static void RetrieveSingleDay(string datestr, string symbol, string barlength="1 day")
        {
            // Action<string> log = Logger.GetLogger(MethodBase.GetCurrentMethod().Name);

            Request req = Request.GetStockRequestSingleDay(datestr, symbol, barlength);
            req.ExecuteStockRequestSingleDay();

            return;
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

            try
            {
                // System.Diagnostics.Debugger.Launch();

                Console.SetOut(new MultipleWriter());

                EWrapperImpl testImpl = new EWrapperImpl();

                clientSocket = testImpl.ClientSocket;
                EReaderSignal readerSignal = testImpl.Signal;

                // TestIBKR();
                //clientSocket.eConnect("52.188.185.179", 1234, 2);
                clientSocket.eConnect("127.0.0.1", 7496, 0);

                var reader = new EReader(clientSocket, readerSignal);
                reader.Start();

                new Thread(() =>
                {
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

                
                new Thread(() =>
                {
                    List<Request> requests = Helper.PopulateMissingDays();

                    foreach ( Request req in requests )
                    {
                        req.ExecuteStockRequestSingleDay();
                    }

                }).Start();
                

                new Thread(() => { CommandControl.StartControl(); }).Start();

                while (!CommandControl.exitnow)
                {
                    Thread.Sleep(1000);

                    log("pulse " + Helper.timestamp);
                }
            }
            catch (Exception ex)
            {
                log(ex.FlattenException());
            }

            return 0;
        }

    }
}
