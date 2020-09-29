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
	[Microsoft.SqlServer.Server.SqlTrigger(Name = "CfsUpdateClrTrigger", Target = "CfsRows", Event = "FOR UPDATE")]
	public static void ClientsCfsTrigger()
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
					sb.Append("Select inserted.ClientId,");
					for (int i = 0; i < triggContext.ColumnCount; i++)
					{
						if (triggContext.IsUpdatedColumn(i))
						{
							var name = schemaTable.Columns[i].ColumnName;
							if (!ignoreCfsColumn(name))
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

									if (!ignoreCfsColumn(colName))
									{
										var row = auditTable.NewRow();
										var oldValue = reader.GetValue(reader.GetOrdinal("Deleted_" + colName));
										var newValue = reader.GetValue(reader.GetOrdinal("Inserted_" + colName));

										var changed = Changed(oldValue, newValue, schemaTable.Columns[i].DataType);

										if (changed)
										{
											row["OldValue"] = oldValue;
											row["NewValue"] = newValue;
											row["ReferenceId"] = reader.GetValue(reader.GetOrdinal("ClientId"));
											row["UpdateDate"] = UpdateDate;
											row["UpdatedBy"] = reader.GetValue(reader.GetOrdinal("UpdatedBy"));
											row["TableName"] = TableName;
											row["FieldName"] = "CFS_" + colName;

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
	private static readonly string[] ignoreCfsFields = new string[] { "Id", "UpdatedById", "UpdatedAt", "CreatedAt", "CreatedById", "ClientId" };
	private static bool ignoreCfsColumn(string name)
	{
		foreach (var field in ignoreCfsFields)
		{
			if (field.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				return true;
		}
		return false;
	}
}

