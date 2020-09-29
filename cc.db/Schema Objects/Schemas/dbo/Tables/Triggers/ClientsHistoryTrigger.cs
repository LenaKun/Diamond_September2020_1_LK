//------------------------------------------------------------------------------
// <copyright file="CSSqlTrigger.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;

public partial class Triggers
{
	// Enter existing table or view for the target and uncomment the attribute line
	[Microsoft.SqlServer.Server.SqlTrigger(Name = "ClientsUpdateClrTrigger", Target = "Clients", Event = "FOR UPDATE")]
	public static void ClientsHistoryTrigger()
	{
		
		// Replace with your own code
		SqlContext.Pipe.Send("Trigger FIRED");
		var TableName = "Clients";
		var UpdateDate = DateTime.Now;
		SqlTriggerContext triggContext = SqlContext.TriggerContext;

		if (triggContext.TriggerAction == TriggerAction.Update)
		{
			using (SqlConnection conn = new SqlConnection("context connection=true"))
			{
				conn.Open();
				SqlPipe sqlP = SqlContext.Pipe;
				var schemaTable = GetSchema(conn);


				using (var auditAdapter = new SqlDataAdapter("select top 0 * from History", conn))
				{
					var auditTable = new DataTable();
					auditAdapter.Fill(auditTable);

					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					sb.Append("Select inserted.Id,");
					for (int i = 0; i < triggContext.ColumnCount; i++)
					{
						if (triggContext.IsUpdatedColumn(i))
						{
							var name = schemaTable.Columns[i].ColumnName;
							if (!ignoreColumn(name))
							{
								sb.Append("Inserted.").Append(name)
									.Append(" as Inserted_").Append(name).Append(",");
								sb.Append("Deleted.").Append(name)
									.Append(" as Deleted_").Append(name).Append(",");
							}
						}
					}

					sb.Append("inserted.UpdatedAt, inserted.UpdatedById as UpdatedBy ");
					sb.Append("from Inserted join Deleted on inserted.id = deleted.id");

					var changeSql = new SqlCommand();
					changeSql.CommandText = sb.ToString();
					changeSql.Connection = conn;
					using (var reader = changeSql.ExecuteReader())
					{
						while (reader.Read())
						{
							for (int i = 0; i < triggContext.ColumnCount; i++)
							{
								if (triggContext.IsUpdatedColumn(i))
								{
									string colName = schemaTable.Columns[i].ColumnName;

									if (!ignoreColumn(colName))
									{
										var row = auditTable.NewRow();
										var oldValue = reader.GetValue(reader.GetOrdinal("Deleted_" + colName));
										var newValue = reader.GetValue(reader.GetOrdinal("Inserted_" + colName));

										var changed = Changed(oldValue, newValue, schemaTable.Columns[i].DataType);
										
										if (changed)
										{
											row["OldValue"] = oldValue;
											row["NewValue"] = newValue;
											row["ReferenceId"] = reader.GetValue(reader.GetOrdinal("Id"));
											row["UpdateDate"] = UpdateDate;
											row["UpdatedBy"] = reader.GetValue(reader.GetOrdinal("UpdatedBy"));
											row["TableName"] = TableName;
											row["FieldName"] = colName;

											auditTable.Rows.Add(row);
										}
									}
								}
							}
						}
						if (!reader.IsClosed)
						{
							reader.Close();
						}
					}


					var commandBuilder = new SqlCommandBuilder(auditAdapter);
					auditAdapter.Update(auditTable);
				}

				sqlP.Send("exiting trigger");
			}
		}
	}
	private static bool Changed(object oldValue, object newValue, Type type)
	{
		var res = true;
		if (oldValue.Equals(newValue))
		{
			res = false;
		}
		else 
		{
			var cov = CanonicalValue(oldValue,type);
			var cnv = CanonicalValue(newValue,type);
			res = !cov.Equals(cnv);	
		}
		return res;
	}
	private static object CanonicalValue(object obj, Type type)
	{
		if (obj == DBNull.Value)
		{
			return obj;
		}
		else if (type == typeof(String))
		{
			if (((string)obj).Trim() == string.Empty)
			{
				obj = DBNull.Value;
			}
		}
		else if (type == typeof(int))
		{
			if ((int)obj == default(int))
			{
				obj = DBNull.Value;
			}
		}
		return obj;

	}
	private static readonly string[] ignoreFields = new string[] { "UpdatedById", "UpdatedAt", "ACPExported", "CreatedAt", "GGReportedCount" };
	private static bool ignoreColumn(string name)
	{
		foreach(var field in ignoreFields)
		{
			if(field.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				return true;
		}
		return false;
	}
	private static DataTable GetSchema(SqlConnection conn)
	{

		var chemaCmd = new SqlCommand();
		chemaCmd.Connection = conn;

		chemaCmd.CommandText = "select top 0 * from inserted";
		var schemaTable = new DataTable();
		using (var schemaAdapter = new SqlDataAdapter(chemaCmd))
		{
			schemaAdapter.Fill(schemaTable);
		}
		return schemaTable;
	}
	private static SqlCommand Insert(object ReferenceId, object TableName, object FieldName, object OldValue, object NewValue, object UpdatedBy, object UpdateDate)
	{
		var cmd = new SqlCommand();

		cmd.CommandText = @"
				INSERT INTO [dbo].[History]
						   ([ReferenceId]
						   ,[TableName]
						   ,[FieldName]
						   ,[OldValue]
						   ,[NewValue]
						   ,[UpdatedBy]
						   ,[UpdateDate])
					 VALUES
						   (@ReferenceId
						   ,@TableName
						   ,@FieldName
						   ,@OldValue
						   ,@NewValue
						   ,@UpdatedBy
						   ,@UpdateDate)
				";
		cmd.Parameters.AddWithValue("@ReferenceId", ReferenceId);
		cmd.Parameters.AddWithValue("@TableName", TableName);
		cmd.Parameters.AddWithValue("@FieldName", FieldName);
		cmd.Parameters.AddWithValue("@OldValue", OldValue);
		cmd.Parameters.AddWithValue("@NewValue", NewValue);
		cmd.Parameters.AddWithValue("@UpdatedBy", UpdatedBy);
		cmd.Parameters.AddWithValue("@UpdateDate", UpdateDate);

		return cmd;


	}
	private static SqlCommand InsertCommand()
	{
		var cmd = new SqlCommand();

		cmd.CommandText = @"
				INSERT INTO [dbo].[History]
						   ([ReferenceId]
						   ,[TableName]
						   ,[FieldName]
						   ,[OldValue]
						   ,[NewValue]
						   ,[UpdatedBy]
						   ,[UpdateDate])
					 VALUES
						   (@ReferenceId
						   ,@TableName
						   ,@FieldName
						   ,@OldValue
						   ,@NewValue
						   ,@UpdatedBy
						   ,@UpdateDate)
				";
		cmd.Parameters.Add("@ReferenceId", SqlDbType.Int);
		cmd.Parameters.Add("@TableName", SqlDbType.NVarChar);
		cmd.Parameters.Add("@FieldName", SqlDbType.NVarChar);
		cmd.Parameters.Add("@OldValue", SqlDbType.NVarChar);
		cmd.Parameters.Add("@NewValue", SqlDbType.NVarChar);
		cmd.Parameters.Add("@UpdatedBy", SqlDbType.Int);
		cmd.Parameters.Add("@UpdateDate", SqlDbType.DateTime);

		return cmd;


	}
}

