# Abstract
This article explains how to import data from excel file into SQL Server database through SQL Server Management Studio.

# Prerequisites
* [Microsoft Access Database Engine 2010 Redistributable](https://www.microsoft.com/en-us/download/confirmation.aspx?id=13255) (or newer);

# Importing
By default, SSMS tries to match column types to imported data, format, etc; while it's convenient, it is prone to errors so I would advice to import all columns as text values.

1. Right click on target database, `Tasks` -> `Import Data`;
2. Follow the wizard's instructions;
3. Select `Microsoft Excel` as data source;
4. Provide path to source file;
    
    * If your file has columns in the first row, check the checkbox;

5. As destination, select ` SQL Server Native Client X.Y`, where `X.Y` is a desired version;
6. Select server (should be already selected) and provide credentials, select proper target database;
7. Select `Copy data from one or more tables or views`;
8. Select desired excel sheet and provide desired table name;
9. Check `Run immediately`;
10. Verify the summary screen and click `Finish`;
11. Wait for the task result;
