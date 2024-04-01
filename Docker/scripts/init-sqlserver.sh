for i in {1..90};
  do
     /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P B@mB00ZlErS_  -d master -i initdb.sql
     if [ $? -eq 0 ]
     then
         echo "completed initdb"
         break
     else
         echo "sqlserver not ready yet"
         sleep 1
     fi
 done
