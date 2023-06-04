using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSIsoft.AF.Asset;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Data;
using System.Data.SqlClient;
using OSIsoft.AF.Time;
using OSIsoft.AF.Data;

namespace AFSDK_AvocetDR
{
    /******************************************************************************
     * A custom data reference for the PI AF server that retrieves the timestamps and
     * values from a specified SQL table. 
     *****************************************************************************/
    [Serializable]
    [Guid("42B2AC6D-8A11-49DD-BD92-DC1F67CCBCC5")]
    [Description("AvocetDR10.1; Get values from slb Avocet SQLName, DBName, TableNam, FieldColumn, TimestampColumn")]
    public class AvocetDR : AFDataReference
    {
        // Private fields storing configuration of data reference
        private string _tableName = String.Empty;
        private string _dbName = String.Empty;
        private string _sqlName = String.Empty;
        private string _fieldcolumn = String.Empty;
        private string _timestampcolumn=String.Empty;

        // Public property for name of the SQL table
        public string TableName
        {
            get
            {
                return _tableName;
            }
            set
            {
                if (_tableName != value)
                {
                    _tableName = value;
                    SaveConfigChanges();
                }
            }
        }

        // Public property for name of the SQL database
        public string DBName
        {
            get
            {
                return _dbName;
            }
            set
            {
                if (_dbName != value)
                {
                    _dbName = value;
                    SaveConfigChanges();
                }
            }
        }

        // Public property for name of the SQL instance
        public string SQLName
        {
            get
            {
                return _sqlName;
            }
            set
            {
                if (_sqlName != value)
                {
                    _sqlName = value;
                    SaveConfigChanges();
                }
            }
        }

        // Public property for name of the SQL table
        public string FieldColumn
        {
            get
            {
                return _fieldcolumn;
            }
            set
            {
                if (_fieldcolumn != value)
                {
                    _fieldcolumn = value;
                    SaveConfigChanges();
                }
            }
        }

        // Public property for name of the SQL table
        public string TimestampColumn
        {
            get
            {
                return _timestampcolumn;
            }
            set
            {
                if (_timestampcolumn != value)
                {
                    _timestampcolumn = value;
                    SaveConfigChanges();
                }
            }
        }

        // Get or set the config string for the SQL data reference
        public override string ConfigString
        {
            get
            {
                return String.Format("{0};{1};{2};{3};{4}", SQLName, DBName, TableName, FieldColumn, TimestampColumn);
            }
            set
            {
                if (value != null)
                {
                    string[] configSplit = value.Split(';');
                    SQLName = configSplit[0];
                    DBName = configSplit[1];
                    TableName = configSplit[2];
                    FieldColumn = configSplit[3];
                    TimestampColumn = configSplit[4];
                    SaveConfigChanges();
                }
            }
        }

        // Return latest value if timeContext is null, otherwise return latest value before a specific time
        public override AFValue GetValue(object context, object timeContext, AFAttributeList inputAttributes, AFValues inputValues)
        {
            AFValue currentVal = new AFValue();
            DateTime time;
            // Get the TimeZone ID to handle Day Saving Time
             TimeZoneInfo localTZID = TimeZoneInfo.Local;
            //TimeZoneInfo localTZ = TimeZoneInfo.FindSystemTimeZoneById(localTZID);

            if (timeContext != null)
            {
                time = ((AFTime)timeContext).LocalTime;
            }
            else
            {
                time = AFTime.Now;
                
            }
            using (SqlDataReader reader = SQLHelper.GetSQLData(SQLName, DBName, TableName, DateTime.MinValue, time, FieldColumn, TimestampColumn))
            {
                if (reader.Read())
                {
                    DateTime queryOfSQLTimeStamp = DateTime.Parse(reader[0].ToString());
                    queryOfSQLTimeStamp = DateTime.SpecifyKind(queryOfSQLTimeStamp, DateTimeKind.Local);
                    if (localTZID.IsInvalidTime(queryOfSQLTimeStamp))
                    {                        
                        currentVal.Timestamp = queryOfSQLTimeStamp.AddHours(1);
                        //currentVal.Value = -100;

                    }
                    else if(localTZID.IsAmbiguousTime(queryOfSQLTimeStamp)) 
                    {
                        currentVal.Timestamp = queryOfSQLTimeStamp.AddHours(-1);
                        //currentVal.Value = -200;
                    }
                    else
                    {
                        currentVal.Timestamp = queryOfSQLTimeStamp ;
                        currentVal.Value = reader[1];

                    }

                    

                }
            }

            return currentVal;
        }

        // Return all values (converted to AFValues) over a specific time interval
        public override AFValues GetValues(object context, AFTimeRange timeRange, int numberOfValues, AFAttributeList inputAttributes, AFValues[] inputValues)
        {
            AFValues values = new AFValues();
            // Get the TimeZone ID to handle Day Saving Time
            TimeZoneInfo localTZID = TimeZoneInfo.Local;
            //TimeZoneInfo localTZ = TimeZoneInfo.FindSystemTimeZoneById(localTZID);
            DateTime startTime = DateTime.SpecifyKind(timeRange.StartTime, DateTimeKind.Local);
            DateTime endTime = DateTime.SpecifyKind(timeRange.EndTime, DateTimeKind.Local);
            using (SqlDataReader reader = SQLHelper.GetSQLData(SQLName, DBName, TableName, startTime, endTime, FieldColumn, TimestampColumn))
            {
                while (reader.Read())
                {
                    AFValue newVal = new AFValue();
                    DateTime queryOfSQLTimeStamp = DateTime.Parse(reader[0].ToString());
                    queryOfSQLTimeStamp=DateTime.SpecifyKind(queryOfSQLTimeStamp, DateTimeKind.Local);
                    if (localTZID.IsInvalidTime(queryOfSQLTimeStamp))
                    {                      
                        newVal.Timestamp = queryOfSQLTimeStamp.AddHours(1);                        
                    }
                    else if(localTZID.IsAmbiguousTime(queryOfSQLTimeStamp))
                    {
                        newVal.Timestamp = queryOfSQLTimeStamp.AddHours(-1);
                    }
                    else 
                    {
                        newVal.Timestamp = queryOfSQLTimeStamp;                           
                    }

                    newVal.Value = reader[1];
                    values.Add(newVal);
                }
            }
            return values;
        }

        // Return an AFEventSource object for this custom data reference
        public static object CreateDataPipe()
        {
            EventSource pipe = new EventSource();
            return pipe;
        }

        public override AFDataReferenceMethod SupportedMethods
        {
            get
            {
                return AFDataReferenceMethod.GetValue | AFDataReferenceMethod.GetValues;
            }
        }

        public override AFDataMethods SupportedDataMethods
        {
            get
            {
                return AFDataMethods.DataPipe;
            }
        }

        public override AFDataReferenceContext SupportedContexts
        {
            get
            {
                return AFDataReferenceContext.Time;
            }
        }



    }
}