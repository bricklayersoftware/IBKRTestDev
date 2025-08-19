# py -m pip install matplotlib

import sqlite3
import os
import pandas as pd
import matplotlib.pyplot as plt

try:
   
    cwd = os.getcwd()
    print(cwd)

    df = pd.read_csv('K:\\temp_git_repos\\ibkrtestdev-1\\WORK\\yield-curve-rates-1990-2025.csv')

    invert = df['10 Yr'] - df['2 Yr'] 

    # print(df.to_string()) 

    df = pd.DataFrame( { 'Date':df['Date'], 'diff':invert } )
    
    df.plot()

    plt.show()

    # print(df.to_string())


except Exception as e:
    print('--- exception occurred ---')


"""
    # Connect to DB and create a cursor
    sqliteConnection = sqlite3.connect('.\\rdbms\\sqllite\\dbs\\ibkrdb.db');

    cursor = sqliteConnection.cursor()
    
    cursor.execute('SELECT * FROM stocks ORDER BY Date ASC')
     
    # Fetch and output result
    result = cursor.fetchall()
    rows = result

    for row in rows:
        print(row)
 
    # Close the cursor
    cursor.close()
    
    if sqliteConnection:
        sqliteConnection.close()
        print('SQLite Connection closed')

# Handle errors
except sqlite3.Error as error:
    print('Error occurred - ', error)

"""