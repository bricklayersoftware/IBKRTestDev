from ib_insync import *
import pandas as pd
from datetime import datetime

now = datetime.now()
print(now)


# Connect to IB TWS or Gateway
ib = IB()
ib.connect('127.0.0.1', 7496, clientId=31415)

# Define the contract
contract = Stock('QQQ', 'SMART', 'USD')

bars = ib.reqHistoricalData(
    contract,
    endDateTime='',
    durationStr='1 D',
    barSizeSetting='30 secs',
    whatToShow='TRADES',
    useRTH=True,
    formatDate=1,
    keepUpToDate=False
)

df = util.df(bars)
print(df)

# Disconnect from IB
ib.disconnect()

now = datetime.now()
print(now)
