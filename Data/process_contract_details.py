# https://www.online-python.com/
import sys
import traceback
import time
import os
import datetime
import os

script_path = os.path.realpath(__file__)
print(script_path)
directory_path = os.path.dirname(script_path)
print(directory_path)


fullfname = 'contract_details.txt'

sys.argv = [ script_path, fullfname ]

# total arguments
n = len(sys.argv)

for arg in sys.argv:
    print("arg: " + arg)

if ( n != 2 ):
    print("incorrect number of args: "+str(len(sys.argv)))
    sys.exit()

fullfname = directory_path + "\\" + sys.argv[1]
fnameext = fullfname.split(".")[-1]
fname = fullfname.split(".")[0]

fnameout = fname+".sql"

i=1
while os.path.exists(fnameout):
    fnameout = fname+"_"+(str(i)).zfill(2)+"_.sql"
    i=i+1

fieldsout = [ 'ConId', 'Symbol', 'SecType', 'LastTradeDateOrContractMonth', 'Strike', 'Right' ]

index_to_symbol={}

secbegin = False
contracts = {}

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

                if ( linearr[0] == "ContractDetails" ) and ( linearr[1] == "begin." ):
                    secbegin = True
                    reqid = linearr[3]
                    contract = {}
                    contract['RequestID'] = reqid
                    vals = []
                    continue

                if ( linearr[0] == "ContractDetails" ) and ( linearr[1] == "end." ):
                    secbegin = False
                    contracts[reqid] = contract

                    for field in fieldsout:
                        vals.append(contract[field])
                    
                    linestr = "('" + '\',\''.join(vals) + "')"
                    fieldsout[fieldsout.index('Strike')] = '_Strike'
                    lineout = "INSERT INTO [dbo].[SecurityDetails] ( ["+( "], [".join(fieldsout) ) +"] ) VALUES " +linestr + "\nGO\n"
                    fieldsout[fieldsout.index('_Strike')] = 'Strike'

                    ffile.write(lineout)  
                    continue


                if ( secbegin == False ):
                    continue

                if ( len(linearr) <= 1 ):
                    contract[linearr[0]] = ''
                else:
                    contract[linearr[0]] = linearr[1]

                # dt = datetime.datetime.fromtimestamp(int(unix_timestamp)).strftime("%Y%m%d%H%M%S")


                #print(linearr)
                # print(lineout)
                # ffile.write(lineout)

    # print(contracts)

except Exception as e:
    traceback.print_exc()



