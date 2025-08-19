# https://www.online-python.com/
import sys
import traceback
import time
import os
import datetime
import os
import json

script_path = os.path.realpath(__file__)
print(script_path)
directory_path = os.path.dirname(script_path)
print(directory_path)


fullfname = 'options_chain.txt'

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
    fnameout = fname+"_"+(str(i)).zfill(2)+".sql"
    i=i+1

index_to_symbol={}

secbegin = False
contracts = {}

try:
    with open(fullfname, 'r') as file:
        with open(fnameout, "a") as ffile:
            for line in file:
                # Security Definition Option Parameter. Reqest: 2131755009, Exchange: AMEX, Undrelying contract id: 320227571, Trading class: QQQ, 
                # Multiplier: 100, Expirations: 20250702, 20250703, 20250707, 20250708, 20250709, 20250710, 20250711, 20250714, 20250715, 20250718, 20250725, 20250731, 20250801, 20250808, 20250815, 20250829, 20250919, 20250930, 20251121, 20251219, 20251231, 20260116, 20260320, 20260331, 20260618, 20260630, 20260918, 20261218, 20270115, 20270617, 20271217, 
                # Strikes: 174.78, 179.78, 184.78, 189.78, 194.78, 199.78, 204.78, 205, 209.78, 210, 214.78, 215, 219.78, 220, 224.78, 225, 229.78, 230, 234.78, 235, 239.78, 240, 244.78, 245, 249.78, 250, 254.78, 255, 259.78, 260, 264.78, 265, 269.78, 270, 274.78, 275, 279.78, 280, 284.78, 285, 289.78, 290, 294.78, 295, 299.78, 300, 304.78, 305, 309.78, 310, 314.78, 315, 319.78, 320, 324.78, 325, 329.78, 330, 334.78, 335, 339.78, 340, 344.78, 345, 349.78, 350, 354.78, 355, 359.78, 360, 364.78, 365, 369.78, 370, 374.78, 375, 379.78, 380, 384.78, 385, 389.78, 390, 394.78, 395, 399.78, 400, 404.78, 405, 409.78, 410, 414.78, 415, 419.78, 420, 424.78, 425, 429.78, 430, 434, 434.78, 435, 436, 437, 438, 439, 439.78, 440, 441, 442, 443, 444, 444.78, 445, 446, 447, 448, 449, 449.78, 450, 451, 452, 453, 454, 454.78, 455, 456, 457, 458, 459, 459.78, 460, 461, 462, 463, 464, 464.78, 465, 466, 467, 468, 469, 469.78, 470, 471, 472, 473, 474, 474.78, 475, 476, 477, 478, 479, 479.78, 480, 481, 482, 483, 484, 484.78, 485, 486, 487, 488, 489, 489.78, 490, 491, 492, 493, 494, 494.78, 495, 496, 497, 497.5, 498, 499, 499.78, 500, 501, 502, 502.5, 503, 504, 504.78, 505, 506, 507, 507.5, 508, 509, 509.78, 510, 511, 512, 512.5, 513, 514, 514.78, 515, 516, 517, 517.5, 518, 519, 519.78, 520, 521, 522, 522.5, 523, 524, 524.78, 525, 526, 527, 527.5, 528, 529, 529.78, 530, 531, 532, 532.5, 533, 534, 534.78, 535, 536, 537, 537.5, 538, 539, 539.78, 540, 541, 542, 542.5, 543, 544, 544.78, 545, 546, 547, 547.5, 548, 549, 549.78, 550, 551, 552, 552.5, 553, 554, 554.78, 555, 556, 557, 558, 559, 559.78, 560, 561, 562, 563, 564, 564.78, 565, 566, 567, 568, 569, 569.78, 570, 571, 572, 573, 574, 574.78, 575, 576, 577, 578, 579, 579.78, 580, 581, 582, 583, 584, 584.78, 585, 586, 587, 588, 589, 589.78, 590, 591, 592, 593, 594, 594.78, 595, 596, 597, 598, 599, 599.78, 600, 601, 602, 603, 604, 604.78, 605, 606, 607, 608, 609, 609.78, 610, 611, 615, 620, 625, 630, 635, 640, 645, 650, 655, 660, 665, 670, 675, 680, 685, 690, 695, 700, 705, 710, 715, 720, 725, 730, 735, 740, 745, 750, 755, 760, 765, 770, 775, 780, 785, 790, 795, 800, 805

                # Security Definition Option Parameter End. Request: 2131755009

                linearr = line.strip().replace(",","").replace(":","").split()
                
                if ( len(linearr) == 0 ) or ( linearr == None ):
                    continue

                # index: 2132803603 symbol: INTU strike: 0 right: C expiry:  type: OPT conid: 270662 snapshot ts: 20250701180707
                # (symbol, strike, right, conid)
                if ( linearr[0] == 'index' ):
                    index_to_symbol[str(int(linearr[1]))] = ( linearr[3],linearr[5],linearr[7],linearr[linearr.index('conid')+1] )

                if ( linearr[0] == 'Security' ) and ( linearr[1] == 'Definition' ):
                    reqid = linearr[5]
                    
                    if ( linearr[4] == 'End.' ):
                        continue
                    
                    # if ( reqid in contracts ):
                    #   continue

                    contract = {}
                    # contract['RequestID'] = reqid
                    contract['ConId'] = linearr[11]
                    contract['Symbol'] = linearr[14]
                    contract['SecType'] = 'OPT'
                    contract['Right'] = index_to_symbol[reqid][2]
                    contract['Exchange'] = linearr[7]

                    # fieldsout = [ 'ConId', 'Symbol', 'SecType', 'LastTradeDateOrContractMonth', 'Strike', 'Right' ]

                    exprindex = linearr.index('Expirations')
                    strkindex = linearr.index('Strikes')

                    expirations = []
                    strikes = []

                    for i in range(strkindex - exprindex - 1 ):
                        expirations.append(linearr[exprindex + i + 1])

                    for j in range(len(linearr) - strkindex-1):
                        strikes.append(linearr[strkindex+j+1])

                    contract['Expirations'] = expirations
                    contract['Strikes'] = strikes

                    vals = []
                    fieldsout = [ "Symbol", "Exchange", "SecType", "Right" ]

                    for field in fieldsout:
                        vals.append(contract[field])

                    lineout = "INSERT INTO [dbo].[OptionsChain] ( [Symbol], [Exchange], [SecType], [Right], [Expirations], [Strikes] ) "
                    lineout += " VALUES ( '"+"','".join(vals)+ "', '"

                    lineout += ", ".join(contract['Expirations']) + "', '"
                    lineout += ", ".join(contract['Strikes']) + "' )" + '\nGO\n'

                    # print(lineout)
                    ffile.write(lineout)

                    continue


                # dt = datetime.datetime.fromtimestamp(int(unix_timestamp)).strftime("%Y%m%d%H%M%S")


                #print(linearr)
                # print(lineout)
                # ffile.write(lineout)

    # print(contracts)

except Exception as e:
    traceback.print_exc()



