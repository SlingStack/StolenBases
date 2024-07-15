using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using StolenBasesLib.Models.OnBase;
using StolenBasesLib.Extensions;
using Supabase.Postgrest.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Connections
{
	public enum ItemDataStatus
	{
		[StringValue("Indexed")]
		Indexed = 0,
		[StringValue("Awaiting Index")]
		AwaitingIndex = 1,
		[StringValue("Deleted")]
		Deleted = 16
	}

	public class OnBaseDB : SqlServer
	{
		public OnBaseDB(string connectionString) : base(connectionString) { }

		public async Task<int[]> RetrieveNewDocHandles(int maxDocHandleRecorded, int reportFrequency = 1)
		{
			//FOR TESTING
			//------------------------
			int[] ints = new int[10];
			for (int i = 0; i < ints.Length; i++) { ints[i] = i + maxDocHandleRecorded + 1; }
			return ints;
			//------------------------

			int itemsAdded = 0;
			int rowCount = 0;

			await Connection.OpenAsync();

			//Get row count
			using (SqlCommand command = Connection.CreateCommand())
			{
				command.CommandText = $"SELECT COUNT(1) FROM hsi.itemdata WHERE itemnum > {maxDocHandleRecorded}";
				using (SqlDataReader reader = await command.ExecuteReaderAsync())
				{
					await reader.ReadAsync();
					rowCount = (int)reader[0];
				}
			}

			int[] docHandles = new int[rowCount];

			using (SqlCommand command = Connection.CreateCommand())
			{
				command.CommandText = $"SELECT itemnum FROM hsi.itemdata WHERE itemnum > {maxDocHandleRecorded}";
				using (SqlDataReader reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.SequentialAccess))
				{
					while (await reader.ReadAsync())
					{
						//if (itemsAdded % reportFrequency == 0) { progress.Report((double)itemsAdded / rowCount); }

						docHandles[itemsAdded] = await reader.GetFieldValueAsync<int>(0);
						itemsAdded++;

						//if(!await ConversionDB.IdentifierExists<int>(itemnum, typeof(OnBaseDocument)))
						//{
						//    await ConversionDB.InsertConversionObject(itemnum, typeof(OnBaseDocument));
						//    itemsAdded++;
						//}
					}
				}
			}

			await Connection.CloseAsync();
			return docHandles;
		}

		public async Task<Document> GetDocumentInfo(int itemnum)
		{
			await Connection.OpenAsync();

			Document document = new Document();

			//Get itemdata
			using (SqlCommand command = Connection.CreateCommand())
			{
				command.CommandText = $"" +
					$"SELECT id.itemnum, id.itemname, id.batchnum, id.status, itg.itemtypegroupname, dt.itemtypename" +
					$" , id.itemdate, id.datestored, ua.username, ua.realname" +
					$" FROM hsi.itemdata id" +
					$" LEFT JOIN hsi.itemtypegroup itg ON id.itemtypegroupnum = itg.itemtypegroupnum" +
					$" LEFT JOIN hsi.doctype dt ON id.itemtypenum = dt.itemtypenum" +
					$" LEFT JOIN hsi.useraccount ua ON id.usernum = ua.usernum" +
					$" WHERE itemnum = {itemnum}";

				SqlDataReader reader = await command.ExecuteReaderAsync();
				if (!reader.HasRows)
				{
					return null;
				}

				await reader.ReadAsync();
				document.DocumentHandle = itemnum;
				document.DocumentName = reader.GetString("itemname");
				document.DocTypeGroup = reader.GetString("itemtypegroupname");
				document.DocType = reader.GetString("itemtypename");
				document.DocumentDate = reader.GetDateTime("itemdate");
				document.StorageDate = reader.GetDateTime("datestored");
				document.CreatedBy = string.IsNullOrEmpty(reader.GetString("realname")) ? reader.GetString("username") : reader.GetString("realname");
				document.Status = ((ItemDataStatus)reader.GetInt32("status")).GetStringValue();
				document.Batchnum = reader.GetInt32("batchnum");
				document.Keywords = new List<Keyword>();
				document.Audit = new Audit();
			}

			//Get keywords
			using (SqlCommand command = Connection.CreateCommand())
			{
				string query = await GetKeywordQuery(document.DocType);
				query = query.Replace("<<itemnum>>", document.DocumentHandle.ToString());
				command.CommandText = query;
				SqlDataReader reader = await command.ExecuteReaderAsync();

				while (reader.Read())
				{
					string name = reader.GetString("name");
					string value = reader.GetString("value");

					Keyword? keyword = document.Keywords.FirstOrDefault(a => a.Name == name);

					if (keyword == null)
					{
						keyword = new Keyword
						{
							Name = name,
							Values = new List<string>()
						};

						document.Keywords.Add(keyword);
						keyword = document.Keywords.First(a => a.Name == name);
					}

					keyword.Values.Add(value);
				}

			}

			//Get revisions & renditions
			using (SqlCommand command = Connection.CreateCommand())
			{
				command.CommandText = $"" +
					$"SELECT rnt.docrevnum, rnt.docrevdate, rnt.notetext, rnt.filetypenum" +
					$" , ua.username, ua.realname" +
					$" FROM hsi.revnotetable rnt" +
					$" LEFT JOIN hsi.useraccount ua ON rnt.usernum = ua.usernum" +
					$" WHERE itemnum = {itemnum}";

				SqlDataReader reader = await command.ExecuteReaderAsync();
				document.Revisions = new List<Revision>();

				while (await reader.ReadAsync())
				{
					Revision rev;
					int revNum = (int)reader["docrevnum"];

					if (!document.Revisions.Any(x => x.RevisionNumber == revNum))
					{
						rev = new Revision
						{
							RevisionNumber = revNum,
							RevisionDate = reader.GetDateTime("docrevdate"),
							Comment = reader.GetString("comment"),
							Renditions = new List<Rendition>()
						};
						document.Revisions.Add(rev);
					}

					rev = document.Revisions.First(x => x.RevisionNumber == revNum);

					Rendition rendition = new Rendition
					{
						RenditionId = reader.GetInt32("filetypenum"),
						CreatedDate = reader.GetDateTime("docrevdate"),
						CreatedBy = string.IsNullOrEmpty(reader.GetString("realname")) ? reader.GetString("username") : reader.GetString("realname"),
						Comment = reader.GetString("comment"),
						Files = new List<Models.OnBase.File>()
					};

					using (SqlCommand filesCommand = Connection.CreateCommand())
					{
						filesCommand.CommandText =
							$"SELECT idp.itempagenum, CONCAT(pp.lastuseddrive, idp.filepath) 'filepath', idp.filesize, idp.numberpages, idp.itempagenum" +
							$" , ft.fileext, ft.mimetype" +
							$" FROM hsi.itemdatapage idp" +
							$" LEFT JOIN hsi.physicalplatter pp ON idp.diskgroupnum = pp.diskgroupnum" +
							$" LEFT JOIN hsi.filetype ft ON idp.filetypenum = dt.filetypenum" +
							$" WHERE idp.itemnum = {itemnum} AND idp.docrevnum = {rev.RevisionNumber}";

						SqlDataReader filesReader = await filesCommand.ExecuteReaderAsync();
						while (filesReader.Read())
						{
							string? path = filesReader.GetString("filepath");
							string? filename = null;
							string? ext = null;

							if (!string.IsNullOrEmpty(path))
							{
								filename = Path.GetFileName(path);
								ext = Path.GetExtension(path);
							}

							StolenBasesLib.Models.OnBase.File file = new Models.OnBase.File
							{
								FilePath = path,
								FileName = filename,
								Ext = ext,
								Page = filesReader.GetInt32("itempagenum"),
								FileSize = filesReader.GetInt32("filesize"),
								PageCount = filesReader.GetInt32("numberpages"),
								MimeType = filesReader.GetString("mimetype")
							};

							rendition.Files.Add(file);
						}
					}

					rev.Renditions.Add(rendition);

				}
			}

			//Get transaction logs
			using (SqlCommand command = Connection.CreateCommand())
			{
				command.CommandText = "" +
					"SELECT txl.transactionnum, txl.tmessage, txl.logdate, txl.docrevnum" +
					", ua.username, ua.realname" +
					" FROM hsi.transactionxlog txl" +
					" LEFT JOIN hsi.useraccount ua ON txl.usernum = ua.usernum" +
					$" WHERE itemnum = {itemnum}";

				SqlDataReader reader = await command.ExecuteReaderAsync();

				while (reader.Read())
				{
					TransactionLog txLog = new TransactionLog
					{
						TransactionNum = reader.GetInt32("transactionnum"),
						Message = reader.GetString("tmessage"),
						LogDate = reader.GetDateTime("logdate"),
						User = string.IsNullOrEmpty(reader.GetString("realname")) ? reader.GetString("username") : reader.GetString("realname"),
						DocumentRevisionNum = reader.GetInt32("docrevnum")
					};

					document.Audit.TransactionLog.Add(txLog);
				}
			}

			//Get workflow logs
			using (SqlCommand command = Connection.CreateCommand())
			{
				command.CommandText = "" +
					"SELECT lc.lifecyclename, lcs.statename 'queue', lcs2.statename 'exit_queue'" +
					", wfl.entrytime, wfl.exittime" +
					", ua.username 'entry_un', ua.realname 'entry_rn'" +
					", ua2.username 'exit_un', ua2.realname 'exit_rn'" +
					" FROM hsi.wflog wfl" +
					" LEFT JOIN hsi.lifecycle lc ON wfl.lcnum = lc.lcnum" +
					" LEFT JOIN hsi.lcstate lcs ON wfl.statenum = lcs.statenum" +
					" LEFT JOIN hsi.lcstate lcs2 ON wfl.statenumto = lcs2.statenum" +
					" LEFT JOIN hsi.useraccount ua ON wfl.usernum = ua.usernum" +
					" LEFT JOIN hsi.useraccount ua2 ON wfl.exitusernum = ua.usernum" +
					$" WHERE itemnum = {itemnum}";

				SqlDataReader reader = await command.ExecuteReaderAsync();

				while (reader.Read())
				{
					WorkflowLog wfLog = new WorkflowLog
					{
						Lifecycle = reader.GetString("lifecyclename"),
						Queue = reader.GetString("queue"),
						ToQueue = reader.GetString("exit_queue"),
						EntryUser = string.IsNullOrEmpty(reader.GetString("entry_rn")) ? reader.GetString("entry_un") : reader.GetString("entry_rn"),
						ExitUser = string.IsNullOrEmpty(reader.GetString("exit_rn")) ? reader.GetString("exit_un") : reader.GetString("exit_rn"),
						EntryDate = reader.GetDateTime("entrytime"),
						ExitDate = reader.GetDateTime("exittime")
					};

					document.Audit.WorkflowLog.Add(wfLog);
				}
			}

			//Get workflow transaction logs
			using (SqlCommand command = Connection.CreateCommand())
			{
				command.CommandText = "" +
					"SELECT wftl.wftransactionnum, lc.lifecyclename, lcs.statename, wftl.logdate, wftm.wfmessage" +
					", ua.username, ua.realname" +
					" FROM hsi.wftransactionlog wftl" +
					" LEFT JOIN hsi.wftransactionmsg wftm ON wftl.wftransactionnum = wftm.wftransactionnum" +
					" LEFT JOIN hsi.lifecycle lc ON wftl.lcnum = lc.lcnum" +
					" LEFT JOIN hsi.lcstate lcs ON wftl.statenum = lcs.statenum" +
					" LEFT JOIN hsi.useraccount ua ON wftl.usernum = ua.usernum" +
					$" WHERE itemnum = {itemnum}";

				SqlDataReader reader = await command.ExecuteReaderAsync();

				while (reader.Read())
				{
					WorkflowTransaction wfTransaction = new WorkflowTransaction
					{
						TransactionNum = reader.GetInt32("wftransactionnum"),
						Lifecycle = reader.GetString("lifecyclename"),
						Queue = reader.GetString("statename"),
						User = string.IsNullOrEmpty(reader.GetString("realname")) ? reader.GetString("username") : reader.GetString("realname"),
						LogDate = reader.GetDateTime("logdate"),
						Message = reader.GetString("wfmessage")
					};

					document.Audit.WorkflowTransactions.Add(wfTransaction);
				}
			}

			Connection.CloseAsync();

			return document;
		}

		public async Task<string> GetKeywordQuery(string docType)
		{
			ModeledResponse<KeywordQuery> kwQuery = await ConversionDB.Connection.client.From<KeywordQuery>()
				.Where(a => a.DocType == docType)
				.Limit(1)
				.Get();

			if (kwQuery != null) { return kwQuery.Model.Query; }


			await Connection.OpenAsync();

			string query = string.Empty;
			List<int> keyNums = new List<int>();

			using (SqlCommand command = Connection.CreateCommand())
			{
				command.CommandText = $"SELECT itxk.keytypenum, ktt.keytypename, ktt.datatype" +
					$" FROM hsi.itemtypexkeyword itxk" +
					$" LEFT JOIN hsi.doctype dt ON itxk.itemtypenum = dt.itemtypenum" +
					$" LEFT JOIN hsi.keytypetable ktt ON itxk.keytypenum = ktt.keytypenum" +
					$" WHERE dt.itemtypename = '{docType}'";

				SqlDataReader reader = await command.ExecuteReaderAsync();
				while (reader.Read())
				{
					if (!string.IsNullOrEmpty(query))
					{
						query += " UNION ";
					}

					int keyTypeNum = reader.GetInt32("keytypenum");
					string keyTypeName = reader.GetString("keytypename");
					int datatype = reader.GetInt32("datatype");

					switch (datatype)
					{
						case 6:
							query += $"SELECT '{keyTypeName}' 'name', CAST(keyvaluesmall AS varchar) 'value' ";
							break;
						case 1:
							query += $"SELECT '{keyTypeName}' 'name', CAST(keyvaluebig AS varchar) 'value' ";
							break;
						case 4:
							query += $"SELECT '{keyTypeName}' 'name', CAST(keyvaluedate AS varchar) 'value' ";
							break;
						case 9:
							query += $"SELECT '{keyTypeName}' 'name', CAST(keyvaluetod AS varchar) 'value' ";
							break;
						case 3:
						case 11:
							query += $"SELECT '{keyTypeName}' 'name', CAST(keyvaluecurr AS varchar) 'value' ";
							break;
						case 5:
							query += $"SELECT '{keyTypeName}' 'name', CAST(keyvaluefloat AS varchar) 'value' ";
							break;
						case 10:
						case 2:
							query += $"SELECT '{keyTypeName}' 'name', CAST(keyvaluechar AS varchar) 'value' ";
							break;
						case 13:
						case 12:
							query += $"SELECT '{keyTypeName}' 'name', CAST(keyvaluecharcs AS varchar) 'value' ";
							break;

					}

					if (datatype == 2 || datatype == 12)
					{
						query += $"FROM hsi.keyxitem{keyTypeNum} kxi " +
							$"LEFT JOIN hsi.keytable{keyTypeNum} kt ON kxi.keywordnum = kxi.keywordnum ";
					}
					else
					{
						query += $"FROM hsi.keyitem{keyTypeNum} ";
					}

					query += $"WHERE itemnum = <<itemnum>>";


				}
			}

			KeywordQuery keywordQuery = new KeywordQuery
			{
				DocType = docType,
				Query = query
			};
			await ConversionDB.Connection.client.From<KeywordQuery>().Insert(keywordQuery);

			return query;
		}
	}
}
