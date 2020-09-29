using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Globalization;

public partial class UserDefinedFunctions
{
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlInt32 GetWeekNumber(DateTime ReportDate, int WeekStartDay)
    {
		DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
		Calendar cal = dfi.Calendar;
        // Put your code here
		return new SqlInt32(cal.GetWeekOfYear(ReportDate, dfi.CalendarWeekRule, (DayOfWeek)WeekStartDay));
    }
}
