# qlikview-server-get-active-docs-and-users
Get the active Qlikview server user&lt;-->docs list to csv or json file

Little command line app that will get the active qlikview users and docs from the Qlikview server (or cluster) via the Qlikview Management API

The app accept 3 arguments:

  -o, --out       Full path to output file. If not passed the result will be displayed in the console
  
  -f, --format    Return data format. csv or json. Default is csv
  
  -a, --append    Append the result to the output file or overwrite it. true or false. Default is true

Example: QV_GetActiveUsersDocs.exe -o "c:\output\users.csv" -a true -f csv

IMPORANT: To change the QMS URL edit the .config file ("address" part on rows 35 and 38)

If you receiving "cannot reach QMS" make sure port 4799 is open to accept requests or try and remove " , Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" from the .config file
	
Also make sure that the user, under which the app is started, is member of QlikView Management API group.

The file call_from_node.js include a sample code that can be used to call the app from Node.js
