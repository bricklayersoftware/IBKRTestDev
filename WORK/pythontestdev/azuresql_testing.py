# https://www.online-python.com/
# python -m pip install requests
# py -m pip install requestsimport sys
import sys
import traceback
import time
import os
import datetime
import pyodbc 

# connstr = "DRIVER={ODBC Driver 18 for SQL Server};Server=tcp:testdev31415.database.windows.net,1433;Initial Catalog=testdevrdbms; Persist Security Info = False; User ID=nvaiuwefa12452; Password = Michael101!; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;";
connstr = "DRIVER={ODBC Driver 17 for SQL Server};SERVER=localhost;DATABASE=testdevrdbms;UID=ibkrtestdev;PWD=Michael101!;MultipleActiveResultSets = False;Encrypt = True;TrustServerCertificate = False;";
connstr = "SERVER=localhost;DATABASE=testdevrdbms;UID=ibkrtestdev;PWD=Michael101!;MultipleActiveResultSets = False;Encrypt = True;TrustServerCertificate = False;";
conn = pyodbc.connect(connstr)

cursor = conn.cursor()
cursor.execute("SELECT * FROM [dbo].[HistoricalData]")
for row in cursor.fetchall():
    print(row)
