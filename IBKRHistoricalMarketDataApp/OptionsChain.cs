using System;
using System.Collections.Generic;
using System.Data;
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

namespace IBKRRealTimeMarketDataApp
{
    public class OptionsChain
    {
        public List<string> expirations { get; set; }

        public List<decimal> strikes { get; set; }

        public string symbol { get; set; }
        public string exchange { get; set; }

        public string sectype { get; set; }

        public string right { get; set; }

        // get at most two expiry dates one below and above the input date
        public (string, string) GetExpiration(string datestr)
        {
            expirations.Sort((a, b) => a.ToLower().CompareTo(b.ToLower())); // ascending sort

            string retdatelow = string.Empty;
            string retdatehigh = string.Empty;

            // yyyymmdd
            foreach (string exprdate in expirations)
            {
                int _exprdate = Int32.Parse(exprdate);
                int _datestr = Int32.Parse(datestr);

                if ( _exprdate <= _datestr) {
                    retdatelow = exprdate;
                } 
                else
                {
                    retdatehigh = exprdate;
                    return (retdatelow, retdatehigh);
                }
            }

            return (null,null);
        }

        public (decimal, decimal) GetStrike(decimal price)
        {
            decimal low = 0;
            decimal high = 0;

            strikes.Sort((a, b) => a.CompareTo(b)); // ascending sort

            foreach (decimal strike in strikes)
            {
                if (strike < price)
                    low = strike;
                else
                {
                    high = strike;
                    break;
                }
            }

            return (low, high);
        }

        public List<decimal> GetStrikes(decimal low, decimal high)
        {
            List<decimal> ret = new List<decimal>();

            foreach (decimal strike in strikes)
            {
                if ( (strike >= low) && (strike <= high) )
                    ret.Add(strike);
            }

            return ret;
        }
    }

    public static class OptionsChainHelper
    {

        public static Dictionary<(string, string), OptionsChain> GetOptionsChain()
        {
            string sql = @"SELECT [RowID]
                                  ,[Symbol]
                                  ,[Exchange]
                                  ,[SecType]
                                  ,[Right]
                                  ,[Expirations]
                                  ,[Strikes]
                              FROM [dbo].[OptionsChain]
                              WHERE [Exchange] = 'NASDAQOM' AND LEFT(Symbol, 1) != '2'
                              ORDER BY [Right] ASC, [Symbol] ASC, [Exchange] ASC";

            Dictionary<string, List<(string, string)>> vals = new Dictionary<string, List<(string, string)>>();

            string connstr = "Server = tcp:testdev31415.database.windows.net,1433; Initial Catalog = testdevrdbms; Persist Security Info = False; User ID = nvaiuwefa12452; Password = Michael101!; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;";

            try
            {
                using var conn = new SqlConnection(connstr);

                conn.Open();

                var command = new SqlCommand(sql, conn);

                using SqlDataReader reader = command.ExecuteReader();

                Dictionary<(string, string), OptionsChain> dict = new Dictionary<(string, string), OptionsChain>();

                int rowid = 0;
                while (reader.Read())
                {
                    rowid++;

                    IDataRecord dataRecord = (IDataRecord)reader;

                    string exchange = dataRecord[2].ToString();

                    if (exchange != "NASDAQOM")
                        continue;

                    string sectype = dataRecord[3].ToString();
                    string right = dataRecord[4].ToString();
                    string symbol = dataRecord[1].ToString();
                    string expirations = dataRecord[5].ToString();
                    string strikes = dataRecord[6].ToString();

                    string[] expirationsarr = null;
                    string[] strikesarr = null;

                    if (expirations.Contains(','))
                        expirationsarr = expirations.Split(',');
                    else
                        expirationsarr = new string[1];

                    if (strikes.Contains(','))
                        strikesarr = strikes.Split(',');
                    else
                        strikesarr = new string[1];

                    List<decimal> decstrikes = new List<decimal>();

                    foreach (string strike in strikesarr)
                    {
                        decimal decstrike = decimal.Parse(strike);
                        decstrikes.Add(decstrike);
                    }

                    if (dict.ContainsKey((symbol, right)))
                    {
                        throw new Exception("duplicate entry at " + symbol + " " + right);
                    }

                    dict.Add((symbol, right), new OptionsChain
                    {
                        symbol = symbol,
                        exchange = exchange,
                        expirations = expirationsarr.ToList(),
                        right = right,
                        sectype = sectype,
                        strikes = decstrikes
                    });
                }

                return dict;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception :: " + ex.Message);
                throw ex;
            }

            return null;
        }

    }
}
