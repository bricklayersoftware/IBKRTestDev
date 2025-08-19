# py -m pip install matplotlib

from ibapi.client import EClient
from ibapi.wrapper import EWrapper
from ibapi.contract import Contract
import pandas as pd

class MyWrapper(EWrapper):
    def __init__(self):
        self.data = []
        self.df=None
        
    def nextValidId(self, orderId:int):
        print("Setting nextValidOrderId: %d", orderId)
        self.nextValidOrderId = orderId
        self.start()

    def historicalData(self, reqId, bar):
        self.data.append(vars(bar));
        
    def historicalDataUpdate(self, reqId, bar):
        line = vars(bar)
        # pop date and make it the index, add rest to df
        # will overwrite last bar at that same time
        self.df.loc[pd.to_datetime(line.pop('date'))] = line
        
    def historicalDataEnd(self, reqId: int, start: str, end: str):
        print("HistoricalDataEnd. ReqId:", reqId, "from", start, "to", end)
        self.df = pd.DataFrame(self.data)
        self.df['date'] = pd.to_datetime(self.df['date'])
        self.df.set_index('date', inplace=True)
        
    def error(self, reqId, errorCode, errorString):
        print("Error. Id: " , reqId, " Code: " , errorCode , " Msg: " , errorString)

    def start(self):
        queryTime = ""
        
        # so everyone can get data use fx
        fx = Contract()
        fx.secType = "STK" 
        fx.symbol = "QQQ"
        fx.currency = "USD"
        fx.exchange = "SMART"
        
        app.reqHistoricalData(1, fx, "", "1 D", "1 min", "TRADES", 1, 1, True, [])

wrap = MyWrapper()        
app = EClient(wrap)
app.connect("127.0.0.1", 7496, clientId=123)

#I just use this in jupyter so I can interact with df
import threading
threading.Thread(target = app.run).start()

#this isn't needed in jupyter, just run another cell
import time
time.sleep(300) # in 5 minutes check the df and close

print(wrap.df)
wrap.df.to_csv("myfile.csv")#save in file
app.disconnect()