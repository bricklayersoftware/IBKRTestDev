# https://ca.finance.yahoo.com/quote/MSFT/options/?date=1755820800&straddle=true
# python -m pip install requests
# py -m pip install requests
# pip install chromedriver-binary
# python.exe -m pip install --upgrade pip
# https://developer.chrome.com/docs/chromedriver/get-started
# https://googlechromelabs.github.io/chrome-for-testing/#stable
# https://storage.googleapis.com/chrome-for-testing-public/139.0.7258.68/win64/chromedriver-win64.zip

import json
import requests
import os
from selenium import webdriver
from selenium.webdriver.common.keys import Keys
# import chromedriver_binary  # Adds chromedriver binary to path
import sys
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
import sys
from datetime import date, timezone
import datetime as dtt

closedate = "20250815"

now = dtt.datetime.now()
loaddatetime = now.strftime("%Y%m%d%H%M%S")

verbose=True

def timestamp():
  dt = dtt.datetime.now()
  formatted_with_microseconds = dt.strftime("%Y-%m-%d %H:%M:%S.%f")
  return formatted_with_microseconds

def fnametimestamp():
  dt = dtt.datetime.now()
  formatted_with_microseconds = dt.strftime("%Y%m%d%H%M%S")
  return formatted_with_microseconds


log_file = None # open(script_directory + "\\" + "log_"+timestamp+".log", "a")
logmsgs=[]

def log(msg):
  global logmsgs
  if ( verbose ):
    outstr="["+timestamp() + "] :: " + msg
    logmsgs.append(outstr)
    print(outstr)
    if ( ( log_file ) and ( len(logmsgs) > 0 ) ):
      for logmsg in logmsgs:
        log_file.write(logmsg+"\n")
        log_file.flush()
      logmsgs=[]

def date_to_epoch(datestr):
    month_to_num = {
        "Jan":1,"Feb":2,"Mar":3,"Apr":4,"May":5,"Jun":6,"Jul":7,"Aug":8,"Sep":9,"Oct":10,"Nov":11,"Dec":12    
    }
    
    month=datestr[0:3:1]
    day=datestr[4:6:1]
    year=datestr.split(",")[1][1:5:1]
    
    month = month_to_num[month]
    
    #print(year)
    #print(month)
    #print(day)
    
    day = day.split(',')[0]

    day=int(day,base=10)
    year=int(year,base=10)
    month=int(str(month),base=10)
    
    dt_object = dtt.datetime(year, month, day, 0, 0, 0, tzinfo=dtt.timezone.utc)
    unix_timestamp = dt_object.timestamp()
    
    return (str(unix_timestamp).split(".")[0])

def is_blank_or_none(s):
    return s is None or (isinstance(s, str) and not s.strip())

log("args: " + ", ".join(sys.argv))

script_path = os.path.abspath(__file__)
script_directory = os.path.dirname(script_path)

log(f"path: {script_path}")
log(f"directory: {script_directory}")

os.chdir(script_directory)

log(f"cwd: {os.getcwd()}")

log_file = open(script_directory + "\\" + "log_"+fnametimestamp()+".log", "a")

exec_path = script_directory+"\\chromedriver-win64\\chromedriver-win64\\chromedriver.exe"

service = Service(executable_path=exec_path)
options = webdriver.ChromeOptions()
options.add_argument("--ignore-certificate-errors")
options.add_argument('--ignore-ssl-errors')
# options.add_argument("--headless")  # Example: Run Chrome in headless mode
driver = webdriver.Chrome(service=service, options=options)


def navigate(url):
  log("navigating to: "+url)
  driver.get(url)
  log("title: "+driver.title)
  log("current_url: "+driver.current_url)

def extract_expirydates():
  # click on expiry drop down to grab list of expiry dates
  elements = driver.find_elements(By.CLASS_NAME, "yf-12wq9rr")

  for elem in elements:
    log(elem.tag_name + " " + elem.text)
    if ( elem.tag_name == "div" ):
      elem.click()
      break

  elements = driver.find_elements(By.CLASS_NAME, "yf-1si11q")

  expiry_dates = []

  for elem in elements:
    log("tag_name: "+elem.tag_name + " text: " + elem.text)
    if ( elem.tag_name == 'div' ):
      expiry_dates.append(elem.text)

  return expiry_dates

def get_options_prices(expirydate):
  # step through options chain table
  """
  section Calls
  Puts
  Last Price Change % Change Volume Open Interest Strike Last Price Change % Change Volume Open Interest
  273.72 0.00 0.00% 2 1
  250.00
  0.01 -0.08 -88.89% - 4
  """
  colnames = [ "Last_Price", "Change", "Change_Pct", "Volume", "Open_Interest" ]
  optionschain = []

  elements = driver.find_elements(By.CLASS_NAME, "yf-1ptb944")

  for elem in elements:
    # log("--> "+ elem.tag_name + " " + elem.text + " ")
    if ( elem.tag_name == "section" ):
      sectionstr = elem.text
      lines = sectionstr.splitlines()
      if ( sectionstr.startswith("Calls") ):
        if( lines[0] == 'Calls' and lines[1] == "Puts" ):
          lines = lines[3:]
          counter=0
          putsdict={}
          callsdict={}
          for line in lines:
            counter=counter+1
            
            if ( counter == 4 ):
              counter = 1
              optionschain.append( { 'expiry':expirydate, 'call':callsdict, 'strike':strike, 'put':putsdict } )
              callsdict={}
              putsdict={}
              strike=-1

            rowstr = line
            row = rowstr.split()

            if ( counter == 2):
              strike=row[0]
              continue

            dictt = {}
            for index, item in enumerate(row):
              dictt[colnames[index]] = item
              #log("|"+item)

            if ( counter == 1):
              callsdict=dictt
            elif ( counter == 3):
              putsdict=dictt

  return optionschain

def print_record(data):
  colnames = [ "Last_Price", "Change", "Change_Pct", "Volume", "Open_Interest" ]
# optionschain.append( { 'expiry':expirydate, 'call':callsdict, 'strike':strike, 'put':putsdict } )

  strike=data['strike']
  expiry=data['expiry']

  right='C'
  prices=data['call']
  pricelist=[]
  
  for col in colnames:
    pricelist.append("'"+prices[col]+"'")


  callprice = "( '"+ loaddatetime+"', '" + symbol + "', '"+right+"', '" + strike + "',  '" + expiry + "' " + ', '.join(pricelist) + "'"+closedate+"' )"

  right='P'
  prices=data['put']
  pricelist=[]
  
  for col in colnames:
    pricelist.append("'"+prices[col]+"'")

  putprice = "( '"+ loaddatetime+"', '" + symbol + "', '"+right+"', '" + strike + "',  '" + expiry + "' " + ', '.join(pricelist) + ", '"+closedate+"' )"

  print(callprice)
  print(putprice)
  
  with open(script_directory + "\\" + "optionsprices_"+closedate+".sql", "a") as f:
    f.write(callprice+"\n")
    f.write(putprice+"\n")


symbols = [ "NVDA", "MSFT", "AAPL", "AMZN", "AVGO", "META", "NFLX", "TSLA", "GOOGL", "COST", "GOOG", "PLTR", "CSCO", "TMUS", "AMD", "LIN", "INTU", "TXN", "PEP" ]
epochdate = "1755820800" # Aug 22, 2025

optionschain = {}
keys = []

for symbol in symbols:
  url = "https://ca.finance.yahoo.com/quote/"+symbol+"/options/?date="+epochdate+"&straddle=true" # date shouldn't matter as we are extracting expiry dates
  navigate(url)
  expiry_dates = extract_expirydates()

  counter=0
  for expirydate in expiry_dates:
    log("expirydate: "+expirydate)
    epochdate=date_to_epoch(expirydate)
    log("epochdate: "+epochdate)
    url = "https://ca.finance.yahoo.com/quote/"+symbol+"/options/?date="+epochdate+"&straddle=true"
    
    if ( counter > 0):
      navigate(url)
    
    counter=counter+1

    key = (symbol, expirydate)
    keys.append(key)
    data = get_options_prices(expirydate)
    optionschain[key] = data
    rows = optionschain[key]
    
    log("rows: "+str(len(rows)))

    for row in rows:
      print_record(row)


driver.quit()


"""
  (<LoadDateTime, varchar(50),>
  ,<Symbol, varchar(50),>
  ,<Right, varchar(50),>
  ,<_Strike, varchar(50),>
  ,<Expiration, varchar(50),>
  ,<_Last_Price, varchar(50),>
  ,<_Change, varchar(50),>
  ,<_Change_Pct, varchar(50),>
  ,<_Volume, varchar(50),>
  ,<_Open_Interest, varchar(50),>
  ,<CloseDate, varchar(50),>)
"""

# for key in keys:
#   data = optionschain[key]

sys.exit()