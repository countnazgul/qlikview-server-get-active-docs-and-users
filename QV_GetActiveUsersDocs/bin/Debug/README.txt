﻿(Run the app with --help to see this info)
This file will generate csv file with the currently active users<->documents on Qlikview Server.

The app accept 2 parameters (Please provide them all):
1. Output file full path. If "false" is used instead file path the raw data will be returned in the console.
2. (optional) append to output - true or false. Default is "true"

Example: QV_GetActiveUsersDocs.exe "c:\output\users.csv" false

To change the QVS URL edit the app.config file ("address" part on rows 35 and 38)