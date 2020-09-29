//------------------------------------------------------------------------------
// <copyright file="CSSqlFunction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

public partial class UserDefinedFunctions
{
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlBoolean tz_check(SqlString input)
    {
		SqlBoolean result;
		if (input.IsNull) { result = SqlBoolean.Null; }
		else
		{

			string IDNum = input.Value;

			// Validate correct input
			if (!System.Text.RegularExpressions.Regex.IsMatch(IDNum, @"^\d{5,9}$"))
				result = SqlBoolean.False;

			if (IDNum == "000000000")
				result = SqlBoolean.False;

			// The number is too short - add leading 0000
			if (IDNum.Length < 9)
			{
				while (IDNum.Length < 9)
				{
					IDNum = '0' + IDNum;
				}
			}

			// CHECK THE ID NUMBER
			int mone = 0;
			int incNum;
			for (int i = 0; i < 9; i++)
			{
				incNum = Convert.ToInt32(IDNum[i].ToString());
				incNum *= (i % 2) + 1;
				if (incNum > 9)
					incNum -= 9;
				mone += incNum;
			}
			if (mone % 10 == 0)
				result = SqlBoolean.True;
			else
				result = SqlBoolean.False;
			
		}
		return result;
    }
}
