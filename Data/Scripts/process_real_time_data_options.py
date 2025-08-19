# https://www.online-python.com/
# python -m pip install requests
# py -m pip install requestsimport sys

import sys
import traceback
import time
import os
import datetime
import pyodbc 

def GetRows():
    connstr = "DRIVER={ODBC Driver 17 for SQL Server};SERVER=tcp:testdev31415.database.windows.net,1433;DATABASE=testdevrdbms; UID=nvaiuwefa12452; PWD=Michael101!; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;";

    conn = pyodbc.connect(connstr)

    cursor = conn.cursor()
    cursor.execute("SELECT * FROM [dbo].[HistoricalData]")
    for row in cursor.fetchall():
        print(row)

fnameout = ''
fullfname = ''

def InitScript():
    global fullfname
    global fnameout
    script_path = os.path.realpath(__file__)
    print('script_path: '+script_path)
    directory_path = os.path.dirname(script_path)
    print('directory_path: '+directory_path)
    print('cwd: '+os.getcwd())
    os.chdir(directory_path)
    print('new cwd: '+os.getcwd())

    sys.argv = [ script_path, 'historical_data_QQQ_holdings_options_1D_5S_20250711.txt' ]

    # total arguments
    n = len(sys.argv)

    for arg in sys.argv:
        print("arg: " + arg)

    if ( n != 2 ):
        print("incorrect number of args: "+str(len(sys.argv)))
        sys.exit()

    fullfname = sys.argv[1]
    fnameext = fullfname.split(".")[-1]
    fname = fullfname.split(".")[0]

    if os.path.exists(fullfname):
        print(f"The file '{fullfname}' exists.")
    else:
        print(f"The file '{fullfname}' does not exist.")

    fnameout = fname+".sql"

    i=1
    while os.path.exists(fnameout):
        fnameout = fname+"_"+(str(i)).zfill(2)+".sql"
        i=i+1

InitScript()

fieldsout = [ "Symbol", "Date", "Time", "_Open", "_High", "_Low", "_Close", "_Volume", "_Count", "_WAP", "OptionType", "_Strike", "Expiry", "TimeInterval" ]

index_to_symbol={}

try:
    connstr = "DRIVER={ODBC Driver 17 for SQL Server};SERVER=tcp:testdev31415.database.windows.net,1433;DATABASE=testdevrdbms; UID=nvaiuwefa12452; PWD=Michael101!; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;";
    conn = pyodbc.connect(connstr)
    cursor = conn.cursor()

    with open(fullfname, 'r') as file:
        with open(fnameout, "a") as ffile:
            for line in file:
                linearr = line.strip().replace(",","").replace(":","").split()
                
                if ( len(linearr) == 0 ):
                    continue

                if ( linearr == None ):
                    continue

                # (symbol, strike, expiry, type)
                if ( linearr[0] == 'index' ):
                    if ( linearr[4] == 'snapshot' ):
                        index_to_symbol[str(int(linearr[1]))] = ( linearr[3], '', '', '' )
                    else:
                        # requestid --> symbol, strike, expiry, right
                        index_to_symbol[str(int(linearr[1]))] = ( linearr[3],linearr[5],linearr[7],linearr[9] )

                if ( linearr[0] == 'Error' ) and ( linearr[4] == 'Code' ):
                    symbol = linearr[2]
                    del index_to_symbol[symbol]

                if ( linearr[0] != "RealTimeBars." ) and ( linearr[0] != "HistoricalData." ):
                    continue

                fields = [ "Open", "High", "Low", "Close", "Volume", "Count", "WAP" ]

                vals = []
                
                reqid = linearr[1]
                reqtup = index_to_symbol[reqid]
                symbol = reqtup[0]
                vals.append(index_to_symbol[reqid][0])

                startindex = 6

                if ( linearr[0] == "RealTimeBars." ):
                    unix_timestamp = linearr[4]

                    dt = datetime.datetime.fromtimestamp(int(unix_timestamp)).strftime("%Y%m%d%H%M%S")

                    vals.append(str(dt)[0:8]) 
                    vals.append(str(dt)[8:]) 
                else:
                    vals.append(linearr[4]) 
                    vals.append(linearr[5]) 
                    startindex = 8

                for field in fields:
                    if ( field in linearr ):
                        pos = startindex+2*fields.index(field)
                        vals.append(linearr[pos])

                # "OptionType", "_Strike", "Expiry"

                vals.append(reqtup[3])
                vals.append(reqtup[1])
                vals.append(reqtup[2])
                
                vals.append('5S')

                linestr = "('" + '\',\''.join(vals) + "')"
                lineout = "INSERT INTO [dbo].[HistoricalData] ( ["+( "], [".join(fieldsout) ) +"] ) VALUES " +linestr

                print(lineout)

                #cursor.execute(lineout)
                #conn.commit()

                ffile.write(lineout + "\nGO\n")

except Exception as e:
    print(f"An exception occurred: {e}")
    exc_type, exc_value, exc_traceback = sys.exc_info()

    # Iterate through the traceback frames
    while exc_traceback:
        frame = exc_traceback.tb_frame
        print(f"\n--- Frame in {frame.f_code.co_name} (Line {frame.f_lineno}) ---")
        print("Local variables:")
        for var_name, var_value in frame.f_locals.items():
            print(f"  {var_name}: {var_value}")
        exc_traceback = exc_traceback.tb_next

    # Optionally, print the full traceback as well
    print("\nFull Traceback:")
    traceback.print_exc()



