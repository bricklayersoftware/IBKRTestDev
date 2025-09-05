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
using System.Linq;
using Microsoft.Identity.Client;    //required

namespace IBKRRealTimeMarketDataApp
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public static class PortChecker
    {
        public static async Task<bool> IsPortOpenAsync(string host, int port, int timeoutMilliseconds = 5000)
        {
            using (TcpClient client = new TcpClient())
            {
                try
                {
                    // Attempt to connect to the remote host and port asynchronously
                    var connectTask = client.ConnectAsync(host, port);

                    // Wait for the connection attempt to complete or timeout
                    if (await Task.WhenAny(connectTask, Task.Delay(timeoutMilliseconds)) == connectTask)
                    {
                        // If the connection task completed within the timeout, check if it was successful
                        return client.Connected;
                    }
                    else
                    {
                        // Connection timed out
                        return false;
                    }
                }
                catch (SocketException)
                {
                    // Connection failed (e.g., port closed, host unreachable)
                    return false;
                }
                catch (Exception)
                {
                    // Other exceptions during the process
                    return false;
                }
            }
        }

        public static bool TestPort(string remoteHost, int remotePort)
        {
            Action<string> log = Logger.GetLogger(MethodBase.GetCurrentMethod().Name);

            // string remoteHost = "google.com"; // Replace with your remote host or IP address
            // int remotePort = 443; // Replace with the port you want to check

            log($"Checking if port {remotePort} on {remoteHost} is open...");

            bool isOpen = IsPortOpenAsync(remoteHost, remotePort).Result;

            if (isOpen)
            {
                log($"Port {remotePort} on {remoteHost} is open.");
            }
            else
            {
                log($"Port {remotePort} on {remoteHost} is closed or unreachable.");
            }

            return isOpen;
        }
    }

    public class ResultSet
    {
        public class RSColumn
        {
            public string fieldname;
            public System.Type fieldtype;
        }

        public string[,] GetTable()
        {

            string[,] table = new string[rowCount, columnCount];

            for (int i = 0; i < columnCount; i++)
            {
                for (int j = 0; j < rowCount; j++)
                {
                    table[j, i] = records[j][i];
                }
            }

            return table;
        }

        public Dictionary<string, List<string>> GetRecordsByField()
        {
            Dictionary<string, List<string>> ret = new Dictionary<string, List<string>>();

            List<string> _records = null; // new List<string>();

            int col = 0;
            foreach (RSColumn column in columns )
            {
                _records = records[col];
                col++;

                ret.Add(column.fieldname, _records);
            }

            return ret;
        }

        public List<List<string>> records; // list of records
        public List<RSColumn> columns;

        public int columnCount
        {
            get { return records[0].Count; }
        }

        public int rowCount
        {
            get { return records.Count; }
        }

        public string[,] GetTableByFields(List<string> fields)
        {
            string[,] ret = null;

            return ret;
        }

        public List<string> GetRowsByField(string fieldname)
        {
            int colindex = 0;

            foreach (var item in columns)
            {
                string _fieldname = item.fieldname;

                if (_fieldname == fieldname)
                    break;

                colindex++;
            }

            return GetRowsByFieldIndex(colindex);
        }

        public List<string> GetRowsByFieldIndex(int colindex)
        {

            List<string> rowset = new List<string>();

            foreach (List<string> items in records)
            {
                rowset.Add(items[colindex]);
            }

            return rowset;
        }

        public string GetRowByField(string fieldname, int row)
        {
            int colindex = 0;

            foreach (var item in columns)
            {
                string _fieldname = item.fieldname;

                if (_fieldname == fieldname)
                    break;

                colindex++;
            }

            List<string> rowset = records[row];
            
            return rowset[colindex];
        }
    }

    public static class Helper
    {
        public static string timestamp
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMddHHMMss");
            }
        }

        public static string query_HistoricalDataCountCheck = @"
            SELECT [Symbol2] AS [Symbol], [Date2] AS [Date], [TimeInterval2] AS [TimeInterval]
            FROM [testdevrdbms].[dbo].[HistoricalDataCountCheck] 
            WHERE COALESCE([Error],'') <> '' AND [Date2] >= DATEADD(day, -5, GETDATE())
            ORDER BY [Symbol2] ASC, [Date2] ASC, [TimeInterval2] ASC
            ";

        // public static string connstr = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=testdevrdbms;Persist Security Info=True;User ID=ibkrtestdev;Password=Michael101!;Pooling=False;Multiple Active Result Sets=False;Encrypt=True;Trust Server Certificate=True;Command Timeout=0";

        public static string connstr = @"Data Source=52.188.185.179,1433;Initial Catalog=testdevrdbms;Persist Security Info=True;User ID=ibkrtestdev;Password=Michael101!;Pooling=False;Multiple Active Result Sets=False;Encrypt=True;Trust Server Certificate=True;Command Timeout=0";

        public static string GetTimestamp()
        {
            // Get the offset from current time in UTC time
            DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
            // Get the unix timestamp in seconds
            string unixTime = dto.ToUnixTimeSeconds().ToString();
            // Get the unix timestamp in seconds, and add the milliseconds
            string unixTimeMilliSeconds = dto.ToUnixTimeMilliseconds().ToString();

            return unixTime;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static List<string> GetSymbols()
        {
            string sql = @"SELECT DISTINCT [Symbol] FROM [dbo].[HistoricalData]";

            List<string> symbols = new List<string>();

            try
            {
                using var conn = new SqlConnection(connstr);

                conn.Open();

                var command = new SqlCommand(sql, conn);

                using SqlDataReader reader = command.ExecuteReader();

                int rowid = 0;
                while (reader.Read())
                {
                    rowid++;

                    IDataRecord dataRecord = (IDataRecord)reader;

                    string symbol = "";

                    symbol = dataRecord[0].ToString();

                    symbols.Add(symbol);
                }

                return symbols;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static int ExecuteSP(string spname)
        {
            using (var conn = new SqlConnection(connstr))
            using (var command = new SqlCommand(spname, conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                conn.Open();
                return command.ExecuteNonQuery();
            }

        }

        public static bool RowExists(string symbol, string date, string time="", string timeinterval="1D") // (string table, List<string> columns, Dictionary<string, string> filter)
        {
            string sql = @"SELECT COUNT(1) AS [RowCount]
                           FROM [testdevrdbms].[dbo].[LatestHistoricalData] 
                           WHERE [Symbol] = '"+symbol+@"' AND [Date] = '"+date+@"' AND [Time] = '"+time+@"' AND [TimeInterval] = '"+timeinterval+@"'";

            ResultSet rs = ExecuteSQL(sql);

            int t = Int32.Parse(rs.GetRowsByFieldIndex(0)[0]);
            return t != 0;
        }
        public static int InsertRecord(string table, List<string> cols, List<string> values)
        {
            SqlConnection conn = new SqlConnection(connstr);

            conn.Open();

            string sq = "\'";
            string sql = "INSERT INTO [dbo].[" + table + "] ( " + String.Join(", ", cols) + " ) VALUES ( " + sq + String.Join(sq + @", " + sq, values) + sq + " )";

            SqlCommand cmd = new SqlCommand(sql, conn);

            int count = cmd.ExecuteNonQuery();
            conn.Close();

            return count;
        }

        public static ResultSet ExecuteSQL(string sql)
        {

            List<List<string>> records = new List<List<string>>();
            List<ResultSet.RSColumn> columns = new List<ResultSet.RSColumn>();

            try
            {
                using var conn = new SqlConnection(connstr);

                conn.Open();

                var command = new SqlCommand(sql, conn);

                using SqlDataReader reader = command.ExecuteReader();

                int rowid = 0;
                while (reader.Read())
                {
                    if (rowid == 0)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var columnName = reader.GetName(i);
                            var dotNetType = reader.GetFieldType(i);
                            columns.Add(new ResultSet.RSColumn { fieldname = columnName, fieldtype = dotNetType });
                        }
                    }

                    rowid++;

                    IDataRecord dataRecord = (IDataRecord)reader;

                    int n = dataRecord.FieldCount;
                    string fieldstr = "";

                    List<string> fields = new List<string>();

                    for (int i = 0; i < n; i++)
                    {
                        fieldstr = dataRecord[i].ToString();
                        fields.Add(fieldstr);
                    }

                    records.Add(fields);
                }

                return new ResultSet { records = records, columns = columns };
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static void InsertHistoricalData(Dictionary<string, string> dict)
        {

        }

        public static Dictionary<string, int> GetPopulationByDate()
        {
            string sql = @"SELECT COUNT([RowID]) As [RowCount], [Date] FROM [testdevrdbms].[dbo].[LatestHistoricalData] GROUP BY [Date]";

            ResultSet rs = Helper.ExecuteSQL(sql);

            List<string> rowcount = rs.GetRowsByFieldIndex(0);
            List<string> dates = rs.GetRowsByFieldIndex(1);

            if ( dates == null || dates.Count == 0 || rowcount == null || rowcount.Count == 0 || rowcount.Count != dates.Count )
            {
                throw new Exception("dates, rowcounts not matching");
            }

            Dictionary<string, int> ret = new Dictionary<string, int>();

            for (int i = 0; i < rowcount.Count; i++)
            {
                ret.Add(dates[i].ToString(), int.Parse(rowcount[i]));
            }

            // DateTime thisDay = DateTime.Today;

            return ret;
        }

        public static List<string> GetQQQSymbols(int count = 19)
        {
            string sql = @"SELECT TOP ("+count.ToString()+") [Symbol] FROM [testdevrdbms].[dbo].[QQQSymbols] ORDER BY [Symbol] ASC";

            ResultSet rs = Helper.ExecuteSQL(sql);

            return rs.GetRowsByFieldIndex(0);
        }

        public static Dictionary<string, (decimal, decimal)> GetQQQDailyBars(string datestr = "")
        {
            Action<string> log = Logger.GetLogger(MethodBase.GetCurrentMethod().Name);

            string sql = "";
            ResultSet rs = null;

            // grab latest populated date
            if (string.IsNullOrEmpty(datestr))
            {
                sql = @"SELECT COUNT(1) AS HoldingsCount, [Date]
                               FROM [dbo].[HistoricalData]
                               WHERE TimeInterval = '1D'
                               GROUP BY Date
                               ORDER BY Date ASC";

                rs = ExecuteSQL(sql);

                List<string> rows = rs.GetRowsByField("Date");

                datestr = rows.Max();
            }

            log("datestr :: [" + datestr + "]");

            sql = @"SELECT [Symbol]
                          ,[High]
                          ,[Low]
                      FROM [dbo].[HistoricalData]
                      WHERE TimeInterval = '1D' AND Date = '"+datestr+"'";
            sql += "ORDER BY Symbol";

            rs = ExecuteSQL(sql);

            Dictionary<string, (decimal, decimal)> vals = new Dictionary<string, (decimal, decimal)>();

            foreach (List<string> row in rs.records)
            {
                decimal high = Decimal.Parse(row[1]);
                decimal low = Decimal.Parse(row[2]);
                vals.Add(row[0], (high, low));
            }

            return vals;
        }
    }
}
