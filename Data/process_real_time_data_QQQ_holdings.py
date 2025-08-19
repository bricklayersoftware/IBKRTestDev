# https://www.online-python.com/
import sys
import traceback
import time
import os
import datetime


fieldsout = [ "Symbol", "Date", "Time", "_Open", "_High", "_Low", "_Close", "_Volume", "_Count", "_WAP" ]

sys.argv[1] = 'real_time_market_data_QQQ_holdings_options_20250620.txt'

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

fnameout = fname+".sql"

i=1
while os.path.exists(fnameout):
    fnameout = fname+"_"+(str(i)).zfill(2)+"_.sql"
    i=i+1

index_to_symbol={}

try:
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
                    index_to_symbol[str(int(linearr[1]))] = ( linearr[3],linearr[5],linearr[7],linearr[9] )

                if ( linearr[0] == 'Error' ) and ( linearr[4] == 'Code' ):
                    symbol = linearr[2]
                    del index_to_symbol[symbol]

                if ( linearr[0] != "RealTimeBars." ):
                    continue

                fields = [ "Open", "High", "Low", "Close", "Volume", "Count", "WAP" ]

                vals = []
                
                reqid = linearr[1]

                vals.append(index_to_symbol[linearr[1]])

                unix_timestamp = linearr[4]

                dt = datetime.datetime.fromtimestamp(int(unix_timestamp)).strftime("%Y%m%d%H%M%S")

                vals.append(str(dt)) # timestamp

                startindex = 6

                for field in fields:
                    if ( field in linearr ):
                        pos = startindex+2*fields.index(field)
                        vals.append(linearr[pos])


                # print(", ('" + '\',\''.join(vals) + "')")
                linestr = "('" + '\',\''.join(vals) + "')"
                lineout = "INSERT INTO [dbo].[HistoricalData] ( ["+( "], [".join(fieldsout) ) +"] ) VALUES " +linestr + "\nGO\n"
                #print(linearr)
                print(lineout)
                ffile.write(lineout)

except Exception as e:
    traceback.print_exc()



