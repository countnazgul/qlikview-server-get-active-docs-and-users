This file will generate csv/json file with the currently active users<->documents on Qlikview Server (or Cluster).

QV Get Active Users<->Docs 0.1
Copyright (C) 2015 stefan.stoichev@gmail.com
Usage: QV_GetActiveUsersDocs.exe -o "c:\output\users.csv" -a true -f csv

  -o, --out       Full path to output file. If not passed the result will be displayed in the console
  -f, --format    Return data format. csv or json. Default is csv
  -a, --append    Append the result to the output file or overwrite. true or false. Default is true
  --help          Display help screen.

IMPORANT: To change the QMS URL edit the .config file ("address" part on rows 35 and 38)
Hint: If you receiving "cannot reach QMS":
	- make sure port 4799 is open to accept requests
	- try and remove " , Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" from the .config file