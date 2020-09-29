//------------------------------------------------------------------------------
// <copyright file="CSSqlAggregate.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections.Generic;
using Microsoft.SqlServer.Server;

[Serializable]
[Microsoft.SqlServer.Server.SqlUserDefinedAggregate(Format.UserDefined, MaxByteSize = -1, IsInvariantToOrder = false)]
public struct string_concat : IBinarySerialize
{
	private List<string> values;

	public void Init()
	{
		values = new List<string>();
	}

	public void Accumulate(SqlString Value)
	{
		if (!Value.IsNull)
		{
			if (!string.IsNullOrEmpty(Value.Value))
			{
				values.Add(Value.Value);
			}
		}
	}

	public void Merge(string_concat Group)
	{
		values.AddRange(Group.values);
	}

	public SqlString Terminate()
	{
		return new SqlString(string.Join(", ", values.ToArray()));
	}

	public void Read(System.IO.BinaryReader r)
	{
		int itemCount = r.ReadInt32();
		this.values = new List<string>(itemCount);
		for (int i = 0; i <= itemCount - 1; i++)
		{
			this.values.Add(r.ReadString());
		}
	}

	public void Write(System.IO.BinaryWriter w)
	{
		w.Write(this.values.Count);
		foreach (string s in this.values)
		{
			w.Write(s);
		}
	}
}
