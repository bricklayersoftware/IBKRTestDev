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

namespace IBKRHistoricalMarketDataApp
{
    public static class IBKRTestDevDBLibrary
    {
        public static List<Request> PopulateMissingDays()
        {
            Action<string> log = Logger.GetLogger(MethodBase.GetCurrentMethod().Name);

            List<Request> requests = new List<Request>();

            ResultSet rs = Helper.ExecuteSPWithRows("GetMissingTradingDays");
            var dict = rs.GetRecordsByField();

            string[,] table = rs.GetTable();

            int rowcount = 0;
            int colcount = 0;

            colcount = table.GetLength(1);
            rowcount = table.GetLength(0);

            log("missing days: columns: " + table.GetLength(1) + " rows: " + table.GetLength(0));

            for (int i = 0; i < rowcount; i++)
            {
                string symbol = "";
                string datestr = "";
                string timeinterval = "";

                for (int j = 0; j < colcount; j++)
                {
                    ResultSet.RSColumn column = rs.columns[j];

                    if (column.fieldname == "Symbol")
                        symbol = table[i, j];
                    else if (column.fieldname == "Date")
                        datestr = table[i, j];
                    else if (column.fieldname == "TimeInterval")
                        timeinterval = table[i, j];

                    if (timeinterval == "1D")
                        timeinterval = "1 day";
                    else if (timeinterval == "5S")
                        timeinterval = "5 secs";
                }

                if (String.IsNullOrWhiteSpace(symbol) || String.IsNullOrWhiteSpace(datestr) || String.IsNullOrWhiteSpace(timeinterval))
                {
                    log("error with row " + i.ToString());
                }

                log("missing trading day request: row: " + i.ToString() + " symbol: " + symbol + " date: " + datestr + " timeinterval: " + timeinterval);

                string[] parts = datestr.Split(' ');
                datestr = parts[0];
                parts = datestr.Split('/');

                parts[1] = (parts[1].Length == 1 ? "0" : "") + parts[1]; // day
                parts[0] = (parts[0].Length == 1 ? "0" : "") + parts[0]; // month
                datestr = parts[2].Trim() + parts[0].Trim() + parts[1].Trim(); // yyyyMMdd
                symbol = symbol.Trim();

                Request req = Request.GetStockRequestSingleDay(datestr, symbol, barsize: timeinterval);

                requests.Add(req);
            }

            log("total requests: " + requests.Count.ToString());

            return requests;
        }

    }


    public class ResultSet
    {
        private SqlDataReader _reader;

        public SqlDataReader reader
        {
            get { return _reader; }

            set
            {
                _reader = value;

                this.columns = reader.GetColumns();
            }
        }

        public class RSColumn
        {
            public string fieldname;
            public System.Type fieldtype;
        }

        // 1st dimension is rows, 2nd is columns
        public string[,] GetTable(Boolean refresh = false)
        {
            if (!refresh & (table != null))
                return table;

            table = new string[rowCount, columnCount];

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
            foreach (RSColumn column in columns)
            {
                _records = records[col];
                col++;

                ret.Add(column.fieldname, _records);
            }

            return ret;
        }

        public string[,] table;
        public List<List<string>> records; // list of records (tuples), records[i] retrieves row i
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

    public static class IBKRTestDevDB
    {
        public static string connstr = @"Data Source=52.188.185.179,1433;Initial Catalog=testdevrdbms;Persist Security Info=True;User ID=ibkrtestdev;Password=Michael101!;Pooling=False;Multiple Active Result Sets=False;Encrypt=True;Trust Server Certificate=True;Command Timeout=0";
        // public static string connstr = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=testdevrdbms;Persist Security Info=True;User ID=ibkrtestdev;Password=Michael101!;Pooling=False;Multiple Active Result Sets=False;Encrypt=True;Trust Server Certificate=True;Command Timeout=0";


    }
}
