# python.exe -m pip install --upgrade pip
# py -m pip install pandas
from ib_insync import *
import sys
import traceback
import time
import os
import datetime
import pyodbc 

# connstr = "DRIVER={ODBC Driver 18 for SQL Server};Server=tcp:testdev31415.database.windows.net,1433;Initial Catalog=testdevrdbms; Persist Security Info = False; User ID=nvaiuwefa12452; Password = Michael101!; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;";
connstr = "DRIVER={ODBC Driver 17 for SQL Server};SERVER=tcp:testdev31415.database.windows.net,1433;DATABASE=testdevrdbms; UID=nvaiuwefa12452; PWD=Michael101!; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;";
conn = pyodbc.connect(connstr)

cursor = conn.cursor()
cursor.execute("SELECT * FROM [dbo].[SecurityDetails]")

vals = {}

for row in cursor.fetchall():
    # print(row)
    vals[row[0]] = row[1]


util.startLoop()

ib = IB()
ib.connect('127.0.0.1', 7496, clientId=12)
# spx = Index('SPX', 'CBOE')
# ib.qualifyContracts(spx)
# ib.reqMarketDataType(4)
# [ticker] = ib.reqTickers(spx)
# spxValue = ticker.marketPrice()
# chains = ib.reqSecDefOptParams(spx.symbol, '', spx.secType, spx.conId)
chains = ib.reqSecDefOptParams('AAPL', '', 'STK', vals['AAPL'])

for chain in chains:
    print(chain.tradingClass + ' - ' + str(len(chain.expirations)) + " " + str(len(chain.strikes)) )

# util.df(chains)

ib.disconnect()