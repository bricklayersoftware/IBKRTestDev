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
datestr = '1829001600'

def extract_options(symbol, datestr):
  driver.get("https://ca.finance.yahoo.com/quote/"+symbol+"/options/?date="+datestr+"&straddle=true")

  log("title: "+driver.title)
  log("current_url: "+driver.current_url)

  # click on expiry drop down to grab list of expiry dates
  elements = driver.find_elements(By.CLASS_NAME, "yf-12wq9rr")

  for elem in elements:
    log("tag_name: "+ elem.tag_name + " text: " + elem.text)
    if ( elem.tag_name == "div" ):
      elem.click()
      break

  elements = driver.find_elements(By.CLASS_NAME, "yf-1si11q")

  expiry_dates = []

  for elem in elements:
    log("tag_name: " + elem.tag_name + " text: " + elem.text)
    if ( elem.tag_name == 'div' ):
      expiry_dates.append(elem.text)


  with open(script_directory + "\\" + "expirations.sql", "a") as f:
    for datestr in expiry_dates:
      rowstr="( '"+ loaddatetime+"' , '"+symbol+"','','"+datestr+"'), "
      log("rowstr: " + rowstr)
      f.write(rowstr+"\n")

  # grab all strikes
  strikes = []
  elements = driver.find_elements(By.CLASS_NAME, "yf-17tl1tk")

  for elem in elements:  
    log("tag_name: [" + elem.tag_name + "] text: [" + elem.text + "] href: [" + elem.get_attribute("href") + "] " )
    strikestr = elem.get_attribute("href")
    token="strike="
    index = strikestr.find(token)
    if ( ( not is_blank_or_none(strikestr) ) and ( strikestr.startswith("https://") ) and ( index > -1 ) ):
      strikeamnt = strikestr[index+len(token)::]
      log("strike: " + strikeamnt)
      strikes.append(strikeamnt)
      
  with open(script_directory + "\\" + "strikes.sql", "a") as f:
    for strikeprice in strikes:
      rowstr="( '"+ loaddatetime+"' , '"+symbol+"','','"+strikeprice+"'), "
      log("rowstr: " + rowstr)
      f.write(rowstr+"\n")


symbols = [ "NVDA", "MSFT", "AAPL", "AMZN", "AVGO", "META", "NFLX", "TSLA", "GOOGL", "COST", "GOOG", "PLTR", "CSCO", "TMUS", "AMD", "LIN", "INTU", "TXN", "PEP" ]

for symbol in symbols:
  log('processing: '+symbol)
  extract_options(symbol, datestr)

driver.quit()

log("---END---")

sys.exit()