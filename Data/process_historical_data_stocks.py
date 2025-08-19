# https://www.online-python.com/
# python -m pip install requests
# py -m pip install requestsimport sys
import traceback
import time
import os
import sys

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

    sys.argv = [ script_path, 'historical_data_QQQ_holdings_1_day_20250627_to_20250711.txt' ]

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

fieldsoutA = [ "Symbol", "Date", "Time", "_Open", "_High", "_Low", "_Close", "_Volume", "_Count", "_WAP", "TimeInterval" ]
fieldsoutB = [ "Symbol", "Date", "_Open", "_High", "_Low", "_Close", "_Volume", "_Count", "_WAP", "TimeInterval" ]

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

                if ( linearr[0] == 'index' ):
                    index_to_symbol[str(int(linearr[1]))] = linearr[3]
                    index_to_symbol["{}".format(100001+int(linearr[1]))] = linearr[3]

                if ( linearr[0] != "HistoricalData." ):
                    continue

                fields = [ "Open", "High", "Low", "Close", "Volume", "Count", "WAP" ]

                vals = []
                
                reqid = linearr[1]

                vals.append(index_to_symbol[linearr[1]])

                vals.append(linearr[4])
                
                if ( reqid.startswith("1") ):
                    vals.append(linearr[5])

                startindex = 6

                if ( reqid.startswith("2") ):
                    startindex = 6

                for field in fields:
                    if ( field in linearr ):
                        pos = startindex+2*fields.index(field)
                        vals.append(linearr[pos])

                fieldsout = fieldsoutB

                if ( reqid.startswith("1") ):
                    vals.append('5S')
                    fieldsout = fieldsoutA

                if ( reqid.startswith("2") ):
                    vals.append('1D')


                # print(", ('" + '\',\''.join(vals) + "')")
                linestr = "('" + '\',\''.join(vals) + "')"
                lineout = "INSERT INTO [dbo].[HistoricalData] ( ["+( "], [".join(fieldsout) ) +"] ) VALUES " +linestr + "\nGO\n"
                #print(linearr)
                #print(lineout)
                ffile.write(lineout)

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


