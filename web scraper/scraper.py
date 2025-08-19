# https":"//www.cnbc.com/quotes/US1Y
# python -m pip install requests
# py -m pip install requests

#
#

"""
Headers = { “Authorization” ": "”our_unique_secret_token” }
response = request.post(“https":"//example.com/get-my-account-detail”, headers=Headers)

requests.get(url, params={key": "value}, args)
args means zero or more of the named arguments in the parameter table below. Example":"
requests.get(url, timeout=2.50)
"""

import json
import requests

headers = { "accept": "*/*", 
"accept-language": "en-US,en;q=0.9",
"origin": "https://www.cnbc.com", 
"priority": "u=1, i", 
"referer": "https://www.cnbc.com/quotes/US1Y", 
"sec-ch-ua": "\"Chromium\";v=\"136\", \"Google Chrome\";v=\"136\", \"Not.A/Brand\";v=\"99\"", 
"sec-ch-ua-mobile": "?0", 
"sec-ch-ua-platform": "\"Windows\"", 
"sec-fetch-dest": "empty", 
"sec-fetch-mode": "cors", 
"sec-fetch-site": "same-site", 
"user-agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36"
}

# url = "https":"//www.cnbc.com/quotes/US1Y"
url = "https://quote.cnbc.com/quote-html-webservice/restQuote/symbolType/symbol?symbols=US1Y&requestMethod=itv&noform=1&partnerId=2&fund=1&exthrs=1&output=json&events=1"
pagestr = requests.get(url, headers=headers)
pageobj = json.loads(pagestr.content)

# url = "https://finance.yahoo.com/quote/AAPL/"
# pagestr = requests.get(url)
# pageobj = json.loads(pagestr.content)

# print(pageobj)
dd = pageobj['FormattedQuoteResult']['FormattedQuote'][0]

print(dd)

"""
last': '4.103%
last_timedate': '11:15 AM EDT'
last_time': '2025-06-16T11:15:12.000-0400'
changetype': 'UP', 
open': '4.106%
high': '4.127%
low': '4.098%
change': '+0.013
change_pct': '+0.3193%
previous_day_closing': '4.09%'
"""