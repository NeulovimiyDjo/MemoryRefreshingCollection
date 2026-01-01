import sys
import xml.etree.cElementTree as ET
import pyodbc


# usage test: import_fias_to_db.py myServerName myDbName fias.xml
serverName = sys.argv[1]
databaseName = sys.argv[2]
mainFileName = sys.argv[3]


def insertToDb(cursor):
    context = iter(ET.iterparse(mainFileName, events=('start', 'end')))
    _, root = next(context)
    
    i = 0
    for event, elem in context:
        i += 1
        
        if event == 'end' and elem.tag == "Object" and int(elem.get('AOLEVEL')) <= int('4'): # cities (level 4)
        
            SQLCommand = ("INSERT INTO [FIAS_TABLE] (FORMALNAME, AOLEVEL, PLAINCODE) VALUES (?,?,?)") 
            Values = [str(elem.get('FORMALNAME')), str(elem.get('AOLEVEL')), str(elem.get('PLAINCODE'))]
            cursor.execute(SQLCommand, Values)            
            
            root.clear()
            
        if i % 1000 == 1: # print progress for every 1'000 objects
            print("Inserted objects: " + str(i))

    print("Total inserted objects: " + str(i))


def main():
    # Windows auth used
    conn = pyodbc.connect(DRIVER='{SQL Server}', Server=serverName, Database=databaseName, trusted_connection='yes')
    cursor = conn.cursor()

    insertToDb(cursor)

    conn.commit()
    conn.close()


if __name__ == "__main__":
    main()
