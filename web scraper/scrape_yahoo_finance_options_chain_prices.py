# https://ca.finance.yahoo.com/quote/MSFT/options/?date=1755820800&straddle=true
# python -m pip install requests
# py -m pip install requests
# pip install chromedriver-binary
# python.exe -m pip install --upgrade pip
# https://developer.chrome.com/docs/chromedriver/get-started
# https://googlechromelabs.github.io/chrome-for-testing/#stable


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
from datetime import datetime

def date_to_epoch(datestr):
    month_to_num = {
        "Jan":1,"Feb":2,"Mar":3,"Apr":4,"May":5,"Jun":6,"Jul":7,"Aug":8,"Sep":9,"Oct":10,"Nov":11,"Dec":12    
    }
    
    month=datestr[0:3:1]
    day=datestr[4:6:1]
    year=datestr[8:12:1]
    
    month = month_to_num[month]
    
    #print(year)
    #print(month)
    #print(day)
    
    day=int(day,base=10)
    year=int(year,base=10)
    month=int(str(month),base=10)
    
    dt_object = datetime(year, month, day, 0, 0, 0)
    unix_timestamp = dt_object.timestamp()
    
    return (str(unix_timestamp).split(".")[0])

def timestamp():
  dt = datetime.now()
  formatted_with_microseconds = dt.strftime("%Y-%m-%d %H:%M:%S.%f")
  return formatted_with_microseconds

verbose=True
exec_path = r"C:\\Users\\Administrator\\Downloads\\chromedriver-win64\\chromedriver-win64\\chromedriver.exe"

now = datetime.now()
loaddatetime = now.strftime("%Y%m%d%H%M%S")

def log(msg):
  if ( verbose ):
    print(timestamp() + " :: " + msg)

def is_blank_or_none(s):
    return s is None or (isinstance(s, str) and not s.strip())


log("---BEGIN---")

log("args: " + ", ".join(sys.argv))

# Get the absolute path to the current script
script_path = os.path.abspath(__file__)

# Get the directory containing the script
script_directory = os.path.dirname(script_path)

log(f"path: {script_path}")
log(f"directory: {script_directory}")

os.chdir(script_directory)

log(f"cwd: {os.getcwd()}")

log("driver path: "+exec_path)

# "https://ca.finance.yahoo.com/quote/"+symbol+"/options/?date="+expirydate+"&straddle=true" # date is not necessary
# https://ca.finance.yahoo.com/quote/MSFT/options/?straddle=true

symbol="MSFT"
url = "https://ca.finance.yahoo.com/quote/"+symbol+"/options/?straddle=true" 

service = Service(executable_path=exec_path)
options = webdriver.ChromeOptions()
# options.add_argument("--headless")  # Example: Run Chrome in headless mode
driver = webdriver.Chrome(service=service, options=options)

def extract_option_table(symbol, expirydate):
  log("extract_option_table :: "+symbol+" "+expirydate)

  expirydateepoch = date_to_epoch(expirydate)

  log("expirydateepoch: "+expirydateepoch)

  url = "https://ca.finance.yahoo.com/quote/"+symbol+"/options/?straddle=true&date="+expirydateepoch
  log("attempting: "+url)
  driver.get(url)

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
    log("tag_name: ["+ elem.tag_name + "] text: [" + elem.text + "]")
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
              log("|"+item)

            if ( counter == 1):
              callsdict=dictt
            elif ( counter == 3):
              putsdict=dictt

  return optionschain


def extract_option_prices(symbol):
  url = "https://ca.finance.yahoo.com/quote/"+symbol+"/options/?straddle=true&date=1755820800"
  log("attempting: "+url)
  driver.get(url)

  log("title: "+driver.title)
  log("current_url: "+driver.current_url)

  # click on expiry drop down to grab list of expiry dates
  elements = driver.find_elements(By.CLASS_NAME, "yf-12wq9rr")

  for elem in elements:
    log("tag_name: "+elem.tag_name + " text: " + elem.text)
    if ( elem.tag_name == "div" ):
      elem.click()
      break

  elements = driver.find_elements(By.CLASS_NAME, "yf-1si11q")

  expiry_dates = []

  for elem in elements:
    log("tag_name: " + elem.tag_name + " text: " + elem.text)
    if ( elem.text == "There are no options." ):
      return None
    if ( elem.tag_name == 'div' ):
      expiry_dates.append(elem.text)
      extract_option_table(symbol, elem.text)

symbols = [ "NVDA", "MSFT", "AAPL", "AMZN", "AVGO", "META", "NFLX", "TSLA", "GOOGL", "COST", "GOOG", "PLTR", "CSCO", "TMUS", "AMD", "LIN", "INTU", "TXN", "PEP" ]

optionschain = {}
for symbol in symbols:
    log('processing: '+symbol)
    optionschain[symbol] = extract_option_prices(symbol)

driver.quit()

log("---END---")

sys.exit()