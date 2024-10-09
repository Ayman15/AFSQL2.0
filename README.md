# AFSQL Custom Data Refeence of Asset Framework 
This Application objective is to read data from Avocet (SLB Application for O&G Upstream)..
Avocet is based on MS SQL Server. Avocet has table contain daily reading of O&G Wells, with timestamp.
this application will allow Asset Framework (AF) to read data stored in Avocet SQL DB beside PI Server. so AF can read data from multiple sources, without need to store all these data in PI Server ( to save tags)
This application is based on dotnet Framework 4.6.2 (so please make sure to download this framework).
there are 3 C# files as following:
1. AvocetDR10 : contin the main code to handle the data from SQL to AF framework.
2. EventPipe.cs : to take care of Data Piping from SQL to AF. 
3. SQLHelper.cs : to handle to connectivity with Avocet SQL Server.
 
Note: in near future , I am planning to use Entity Framework to connect Avocet SQL to AF Framework as more secured way to connect to 3rd party MS SQL Server.
 
