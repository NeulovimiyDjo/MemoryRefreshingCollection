# DndCharacterPlanner

Character Planner for D&D 5th edition

It's a web application that allows to conveniently choose D&D character attributes, feats and spells for a specific class/subclass and level. It also allows to save that created character on the server's database and later restore it.



## Building client (generates files in server/wwwroot)
```
cd client
npm install
npm run build
```

## Running webpack dev server for client
```
cd client
npm run serve
```



## Building server (server/wwwroot files have to be generated first by building client)
```
cd server
dotnet build
```

## Running server
```
cd server
dotnet run
```



## Building WebScraper
```
cd utilities/WebScraper
dotnet build
```



## Using WebScraper
Parses downloaded pages with descriptions of D&D races/classes/spells/e.t.c to a structured representation.

### Parsing downloaded pages and saving to json
Created file is server/Data/database.json
```
cd utilities/WebScraper
dotnet run --scrape-files
```

##### To disable numerous messages add --silent as 2nd argument
```
cd utilities/WebScraper
dotnet run --scrape-files --silent
```


### Printing scraped classes and spells to a readable text file
Created file is utilities/WebScraper/printed.txt
```
cd utilities/WebScraper
dotnet run --print > printed.txt
```
