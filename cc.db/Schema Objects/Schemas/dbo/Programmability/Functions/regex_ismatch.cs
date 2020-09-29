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
using System.Text.RegularExpressions;

public partial class UserDefinedFunctions
{
	[Microsoft.SqlServer.Server.SqlFunction]
	public static SqlBoolean regex_ismatch(SqlChars input, SqlString pattern)
	{
		if (pattern.IsNull || input.IsNull)
		{
			return SqlBoolean.Null;
		}
		else
		{
			var regex = new Regex(pattern.Value);


			var ismatch = regex.IsMatch(new string(input.Value));
			
			return new SqlBoolean(ismatch);
		}
	}
}
