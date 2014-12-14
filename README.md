CodeGenerator
=============
Description:
--------------
Generates Stored Procedure for Oracle and Sql Server database, Data Access Layer Code and Entity classes

Requirements:
--------------
.Net Framework 3.5 SP1 (WPF)
Oracle Client if you want to generate code from Oracle Database

Configuration:
--------------
Open the app.config file, update the following information:
1) value="SqlServer" if sql server database or value="Oracle" if oracle database
	<add key="ConString" value="SqlServer"/> 
2) Put the name/code of your project in place of STS
	<add key="ProjectName" value="STS"/>
3) If SqlServer then following config block will be application
	<!--For Sql Server Database-->
    <add key="ConString_SqlServer" value="Provider=sqloledb;Data Source=PC69;Initial Catalog=STS;User Id=sa;Password=sa1234;" />
    <add key="PkColName" value="intPK"/>
    <add key="RecordCreatorColName" value="IUser"/>
    <add key="RecordModifierColName" value="EUser"/>
    <add key="RecordCreateDateColName" value="IDate"/>
    <add key="RecordModifiedDateColName" value="EDate"/> 
	
  Else if Oracle the following config block will be application
	<!-- For Oracle Database -->
    <add key="ConString_Oracle" value="Provider=OraOLEDB.Oracle;Data Source=tvl;User ID=wims_p4;Password=wims_p4;" />
    <add key="PkColName" value="NUMID"/>
    <add key="RecordCreatorColName" value="STRUID"/>
    <add key="RecordModifierColName" value="STRLASTUID"/>
    <add key="RecordCreateDateColName" value="DTUDT"/>
    <add key="RecordModifiedDateColName" value="DTLASTUDT"/>
	
	update the ConString with your projects Connection String Property
	
	the value of "PkColName" key will be the primary key column name of your table
	the value of "RecordCreatorColName" key will be the Record Creator's column name of your table
	the value of "RecordModifierColName" key will be the Record Modifier's column name of your table
	the value of "RecordCreateDateColName" key will be the Record Creation timestamp column name of your table
	the value of "RecordCreateDateColName" key will be the Record Modification timestamp column name of your table
	
	usually all the above 5 columns name should be unique in all tables of your database

Run:
-----
Then run the Project/EXE all tables of your connected database will available in the Drop down list
Select 1 table and click on the buttons to generate the Code for you.
The generated files will available at the path defided in 
	<add key="SaveDirectoty" value="D:\\CodeGeneratorFiles\\" />

Happy Coding!!!
Fast Coding!!!
