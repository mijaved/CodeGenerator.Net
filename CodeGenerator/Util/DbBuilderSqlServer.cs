using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CodeGenerator.MVP.Util
{
    public class DbBuilderSqlServer : IDbBuilder
    {
		string _NUMID = "";
		string _STRUID = "";
		string _STRLASTUID = "";
		string _DTUDT = "";
		string _DTLASTUDT = "";

		public DbBuilderSqlServer()
        {
			_NUMID = Util.Utility.GetPkColName();
			_STRUID = Util.Utility.GetRecordCreatorColName();
			_STRLASTUID = Util.Utility.GetRecordModifierColName();
			_DTUDT = Util.Utility.GetRecordCreateDateColName();
			_DTLASTUDT = Util.Utility.GetRecordModifiedDateColName();
		}

        public StringBuilder BuildSequence(string strTableName)
        {
            StringBuilder sb = new StringBuilder();
            
            return sb;
        }

        public StringBuilder BuildSaveProcedure(string strTableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            string strProcName = $"usp{strTableName.Replace("T_", "").Replace("t_", "").Replace("tbl", "")}Save";

            sb.Append($"CREATE PROCEDURE {strProcName}(");
            foreach (DataRow row in dt.Rows)
            {
                var type = row["DATA_TYPE"].ToString();
                var col = row["COLUMN_NAME"].ToString();

                if (col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();

                //for UNIQUEIDENTIFIER
                if (type.ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append($"@{col}" + "VARCHAR(50)".n0t1() + ",".n0t1());
                    continue;
                }

                string strLength = type.ToUpper().Equals("VARCHAR") || type.ToUpper().Equals("CHAR") ? " (" + row["MAX_LENGTH"].ToString() + ")" : "";
                sb.Append($"@{col}" + type.ToUpper().n0t1() + strLength + ",".n0t1());
            }
            //sb.Remove(sb.Length -1, 1); //To Remove last Comma
            sb.Append($"@{_STRUID}   VARCHAR(20), \n\t @strMode  CHAR(1),".n1t1());
            sb.Append("\n\t @strErrorCode VARCHAR(50) OUTPUT, --@numErrorCode INT OUTPUT, \n\t @strErrorMsg VARCHAR(200) OUTPUT)");


            sb.Append("AS".n1t0());
            sb.Append("SET @strErrorCode = 0 --@numErrorCode = 0".n1t1());
            sb.Append("SET @strErrorMsg = 'Successful'".n1t1());
            sb.Append("DECLARE @strSQL VARCHAR(4000);".n1t1());
            sb.Append("BEGIN".n1t0());

            //Insert
            sb.Append("IF (@strMode='I')".n1t1());
            sb.Append("BEGIN".n1t1());
            sb.Append($"--SET @{_NUMID} = NEWID()".n1t2());
            sb.Append("INSERT INTO " + strTableName + "(".n1t2());
            foreach (DataRow row in dt.Rows)
            {
                var type = row["DATA_TYPE"].ToString();
                var col = row["COLUMN_NAME"].ToString();

                if (col.Equals(_NUMID) ||
                    col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append($"{col},".n0t3());
            }
            sb.Append($"{_STRUID}, " + _STRLASTUID.n1t3() + ")".n1t3());

            sb.Append("VALUES(".n1t2());
            foreach (DataRow row in dt.Rows)
            {
                var type = row["DATA_TYPE"].ToString();
                var col = row["COLUMN_NAME"].ToString();

                if (col.Equals(_NUMID) ||
                    col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append($"@{col},".n0t3());
            }
            sb.Append($"@{_STRUID}, @{ _STRUID.n1t3()};".n1t3());
            sb.Append("--SELECT @@IDENTITY;".n1t2());
            sb.Append($"SET @{_NUMID}=@@identity;".n1t2());
            sb.Append("END".n1t1());
            
            //Edit
            sb.Append("ELSE IF (@strMode='U')".n1t1());
            sb.Append("BEGIN".n1t1());
            sb.Append("UPDATE " + strTableName + " SET".n1t2());
            foreach (DataRow row in dt.Rows)
            {
				var type = row["DATA_TYPE"].ToString();
				var col = row["COLUMN_NAME"].ToString();

				if (col.Equals(_NUMID) ||
                    col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append($"{col} \t = @{col},".n0t3());
            }
            sb.Append($"{_STRLASTUID}=@{_STRUID}".n1t3());
            sb.Append($"WHERE {_NUMID}=@{_NUMID} ;".n1t3());
            sb.Append($"--SELECT @{_NUMID};".n1t2());
            sb.Append("END".n1t1());
            
            //Delete
            sb.Append("ELSE IF (@strMode='D')".n1t1());
            sb.Append("BEGIN".n1t1());
            sb.Append($"DELETE FROM {strTableName} WHERE {_NUMID } = @{_NUMID};".n1t2());
            //sb.Append("UPDATE TBLAUDITMASTER SET STRUSER=p_STRUID WHERE NUMSESSIONID=userenv('sessionid');".n1t2());
            sb.Append("END".n1t1());
            sb.Append("END".n1t0());

            sb.Append($"SET @strErrorCode = @{_NUMID}".n1t1()); sb.Append("--SET @numErrorCode = @" + _NUMID.n0t1());
            sb.Append("IF @@error <> 0 goto procError".n1t1());
            sb.Append("goto procEnd".n1t2());

            sb.Append("procError:".n1t0().n1t1());
            sb.Append("SET @strErrorCode = @@error".n1t2()); sb.Append("--SET @numErrorCode = @@error".n0t1());
            sb.Append("select @strErrorMsg = [description] from master.dbo.sysmessages where error = @strErrorCode --@numErrorCode".n1t2());
            sb.Append($"insert into error_log (LogDate,Source,ErrMsg) values (getdate(),'{strProcName}',@strErrorMsg)".n1t2());

            sb.Append("procEnd:".n1t0());
            //MessageBox.Show(sb.ToString());

            return sb;
        }

        public StringBuilder BuildGetProcedure(string strTableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            string strProcName = $"usp{strTableName.Replace("T_", "").Replace("t_", "").Replace("tbl", "")}Get";

            sb.Append($"CREATE PROCEDURE {strProcName}(");
            foreach (DataRow row in dt.Rows)
            {
                var type = row["DATA_TYPE"].ToString();
                var col = row["COLUMN_NAME"].ToString();

                if (col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT) ||
                    type.ToUpper().Equals("IMAGE") || type.ToUpper().Equals("TEXT") ||
                    type.ToUpper().Equals("XML"))
                    continue;

                sb.AppendLine();

                //for UNIQUEIDENTIFIER
                if (type.ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append($"@{col}\tVARCHAR(50),".n0t1());
                    continue;
                }

                if (type.ToUpper().Equals("DATETIME"))
                {
                    sb.Append($"@{col}From\tVARCHAR(20),".n0t1());
                    sb.AppendLine();
                    sb.Append($"@{col}ToVARCHAR(20),".n0t1());
                    continue;
                }

                string strLength = type.ToUpper().Equals("VARCHAR") || type.ToUpper().Equals("CHAR") ? " (" + row["MAX_LENGTH"].ToString() + ")" : "";
                sb.Append($"@{col}\t{type.ToUpper() + strLength},".n0t1());
            }
            //sb.Remove(sb.Length -1, 1); //To Remove last Comma
            sb.Append("@strMode  VARCHAR(1), --0 for total count, 1 for single detail view, 2 for grid view".n1t1());
            sb.Append("@strSortBy    VARCHAR(50),".n1t1());
            sb.Append("@strSortType  VARCHAR(4),".n1t1());
            sb.Append("@startRowIndex    INT,".n1t1());
            sb.Append("@maximumRows  INT, -- OPTIONAL ; 0 TO GET ALL SELECTED".n1t1());
            sb.Append("@numErrorCode INT OUTPUT,".n1t1());
            sb.Append("@strErrorMsg  VARCHAR(200) OUTPUT)".n1t1());

            sb.Append("AS".n1t0());
            sb.Append("SET NOCOUNT ON".n1t1());
            sb.Append("SET ANSI_NULLS OFF".n1t1());
            sb.Append("SET @numErrorCode = 0".n1t1());
            sb.Append("SET @strErrorMsg = 'Successful'".n1t1());
            sb.Append("DECLARE @strSQL VARCHAR(4000);".n1t1());
            sb.Append("DECLARE @strQry VARCHAR(4000);".n1t1());

            sb.Append("BEGIN".n1t0());
            sb.Append("SET @strSQL=''".n1t1());
            sb.Append("SET @strQry =''".n1t1());

            foreach (DataRow row in dt.Rows)
            {
                var type = row["DATA_TYPE"].ToString();
                var col = row["COLUMN_NAME"].ToString();

                if (col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT) ||
                    type.ToUpper().Equals("IMAGE") || type.ToUpper().Equals("TEXT") ||
                    type.ToUpper().Equals("XML"))
                    continue;

                sb.AppendLine();

                //for UNIQUEIDENTIFIER
                if (type.ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append($"IF(LEN(@{col})>0)".n1t1());
                    sb.Append($"SET @strSQL = @strSQL+' AND T.{col} = '''+ @{col} + ''''".n1t2());
                }

                //INT/NUMBER/DECIMAL/DOUBLE/FLOAT
                if (type.ToUpper().Equals("INT") || type.ToUpper().Equals("SMALLINT")
                    || type.ToUpper().Equals("NUMBER") || type.ToUpper().Equals("DECIMAL")
                    || type.ToUpper().Equals("DOUBLE") || type.ToUpper().Equals("FLOAT")
                    || type.ToUpper().Equals("NUMERIC") || type.ToUpper().Equals("MONEY")
                    )
                {
                    sb.Append($"IF (@{col}>0)".n1t1());
                    sb.Append($"SET @strSQL = @strSQL+' AND T.{col} = '+ CAST(@{col} AS VARCHAR(" + row["MAX_LENGTH"] + "))".n1t2());
                }

                //CHAR/VARCHAR/VARCHAR2/STRING
                if (type.ToUpper().Equals("VARCHAR") || type.ToUpper().Equals("VARCHAR2")
                    || type.ToUpper().Equals("CHAR") || type.ToUpper().Equals("STRING") || type.ToUpper().Equals("NVARCHAR"))
                {
                    sb.Append($"IF(LEN(@{col})>0)".n1t1());
                    sb.Append($"SET @strSQL = @strSQL+' AND UPPER(T.{col}) = '''+ UPPER(@{col}) + ''''".n1t2());
                    //sb.Append($"strSql := strSql || ' AND UPPER(T.{col}) LIKE ''%' || REPLACE(UPPER(p{col}),'''','''''') || '%''';".n1t2());
                }
                //DateTime
                if (type.ToUpper().Equals("DATETIME"))
                {
                    sb.Append($"IF(LEN(@{col}From)>0 AND LEN(@{col}To)>0)".n1t1());
                    sb.Append($"SET @strSQL = @strSQL+' AND T.{col} BETWEEN '''+ @{col}From + ''' AND ''' + @{col}To + ''''".n1t2());
                    //sb.Append($"ELSIF p{col}From IS NOT NULL  AND p{col}To IS NULL  THEN".n1t1());
                    //sb.Append($"strSql := strSql || ' AND T.{col} >= '''|| p{col}From|| '''' ;".n1t2());
                    //sb.Append($"ELSIF p{col}From IS NULL AND p{col}To IS NOT NULL  THEN".n1t1());
                    //sb.Append($"strSql := strSql || ' AND T.{col} <= ''' ||p{col}To|| '''';".n1t2());
                }
            }

            sb.Append("-- Paging and Sorting Parameters".n1t0().n1t1());
            sb.Append($"IF(LEN(@strSortBy)=0) set @strSortBy = '" + _NUMID + "'".n1t1());
            sb.Append("IF(LEN(@strSortType)=0) set @strSortType = 'DESC'".n1t1());

            sb.AppendLine();

            //RowCount
            sb.Append("IF (@strMode = '0') -- FOR ROW COUNT ".n1t1());
            sb.Append("BEGIN".n1t1());
            sb.Append($"SET @strQry ='SELECT COUNT(1) TOTALROWS FROM {strTableName} T ".n1t2());
            sb.Append("WHERE 0 = 0 ' + @strSQL".n1t2());
            sb.Append("END".n1t1());
            //SelectAll
            sb.Append("IF (@strMode = '1') -- GRID VIEW ".n1t1());
            sb.Append("BEGIN".n1t1());
            sb.Append($"SET @strQry = 'SELECT * FROM {strTableName} T WHERE 0=0 '+ @strSQL".n1t2());
            sb.Append("END".n1t1());
            sb.Append("IF (@strMode = '2') -- Join with Other Related Table".n1t1());
            sb.Append("BEGIN".n1t1());
            sb.Append("SET @strQry = 'WITH T AS (SELECT ROW_NUMBER() OVER (ORDER BY '".n1t2());
            sb.Append("+ @strSortBy + ' ' + @strSortType + ' ) AS ROWID, * FROM ( SELECT ".n1t2());

            foreach (DataRow row in dt.Rows)
            {
				var type = row["DATA_TYPE"].ToString();
				var col = row["COLUMN_NAME"].ToString();

				if (col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT))
                    continue;
                sb.Append($"{col},".n1t3());
            }
            sb.Remove(sb.Length -1, 1); //To Remove last Comma

            sb.Append($"FROM {strTableName} T WHERE 0=0 '".n1t2());
            sb.Append("+ @strSQL + ') AS T) SELECT T.* FROM T WHERE 1=1 '".n1t2());

            sb.Append("IF(@maximumRows > 0)".n1t3());
            sb.Append("SET @strQry = @strQry + ' AND ROWID BETWEEN ' ".n1t3());
            sb.Append("+ CAST(@startRowIndex as varchar(20)) + ' and ' ".n1t3());
            sb.Append("+ CAST(@startRowIndex + @maximumRows -1 as varchar(20))	".n1t3());
            sb.Append("END".n1t1());


            //sb.Append("END".n1t1());
            sb.Append("print(@strQry)".n1t1());
            sb.Append("exec (@strQry)".n1t1());
            sb.Append("END".n1t0());

            sb.Append("IF @@error <> 0 GOTO procError".n1t0().n1t1());
            sb.Append("GOTO procEnd".n1t2());

            sb.Append("procError:".n1t0().n1t0());
            sb.Append("SET @numErrorCode = @@error".n1t1());
            sb.Append("SELECT @strErrorMsg = [description] FROM master.dbo.sysmessages WHERE error = @numErrorCode".n1t1());
            sb.Append("INSERT INTO error_log (LogDate,Source,ErrMsg) VALUES (getDate(),'" + strProcName + "',@strErrorMsg)".n1t1());

            sb.Append("procEnd:".n1t0());
            
            //MessageBox.Show(sb.ToString());
            return sb;
        }

        public StringBuilder BuildGetProcedureSingle(string strTableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            string strProcName = $"usp{strTableName.Replace("T_", "").Replace("t_", "").Replace("tbl", "")}Get";

            sb.Append($"CREATE PROCEDURE {strProcName}(");
            foreach (DataRow row in dt.Rows)
            {
				var type = row["DATA_TYPE"].ToString();
				var col = row["COLUMN_NAME"].ToString();

				if (col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT) ||
                    type.ToUpper().Equals("IMAGE") || type.ToUpper().Equals("TEXT") ||
                    type.ToUpper().Equals("XML"))
                    continue;

                sb.AppendLine();

                //for UNIQUEIDENTIFIER
                if (type.ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append($"@{col}\tVARCHAR(50),".n0t1());
                    continue;
                }

                if (type.ToUpper().Equals("DATETIME"))
                {
                    sb.Append($"@{col}From\t{type.ToUpper()},".n0t1());
                    sb.AppendLine();
                    sb.Append($"@{col}To\t{type.ToUpper()},".n0t1());
                    continue;
                }

                string strLength = type.ToUpper().Equals("VARCHAR") || type.ToUpper().Equals("CHAR") ? " (" + row["MAX_LENGTH"].ToString() + ")" : "";
                sb.Append($"@{col}\t{type.ToUpper() + strLength},".n0t1());
            }
            sb.Append("@SortBy     VARCHAR(50),".n1t1());
            sb.Append("@SortOrder  VARCHAR(4),".n1t1());
            sb.Append("@startRowIndex    INT,".n1t1());
            sb.Append("@maximumRows  INT, -- OPTIONAL ; 0 TO GET ALL SELECTED".n1t1());
            sb.Append("@numTotalRows	INT OUTPUT,".n1t1());
            sb.Append("@numErrorCode INT OUTPUT,".n1t1());
            sb.Append("@strErrorMsg  VARCHAR(200) OUTPUT)".n1t1());

            sb.Append("AS".n1t0());
            sb.Append("SET NOCOUNT ON".n1t1());
            sb.Append("SET ANSI_NULLS OFF".n1t1());

            sb.Append("DECLARE @strSQL VARCHAR(4000);".n1t1());
            sb.Append("DECLARE @strQry VARCHAR(4000);".n1t1());
            sb.Append("DECLARE @sqlTotal   NVARCHAR(4000);".n1t1());
            sb.Append("DECLARE @ParmDefinition NVARCHAR(1000);".n1t1());
            sb.Append("DECLARE @strEditCondition   VARCHAR(4000);".n1t1());

            sb.Append("SET @numErrorCode = 0".n1t1());
            sb.Append("SET @strErrorMsg = 'Successful'".n1t1());

            sb.Append("BEGIN".n1t0());
            sb.Append("SET @strSQL=''".n1t1());
            sb.Append("SET @strQry =''".n1t1());

            //Table Name
            sb.Append($"SET @strSQL = ' FROM {strTableName} T WHERE 0=0 '".n1t0().n1t1());

            //Search Params
            foreach (DataRow row in dt.Rows)
            {
				var type = row["DATA_TYPE"].ToString();
				var col = row["COLUMN_NAME"].ToString();

				if (col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT) ||
                    type.ToUpper().Equals("IMAGE") || type.ToUpper().Equals("TEXT") ||
                    type.ToUpper().Equals("XML"))
                    continue;

                sb.AppendLine();

                //for UNIQUEIDENTIFIER
                if (type.ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append($"IF(LEN(@{col})>0)".n1t1());
                    sb.Append($"SET @strSQL = @strSQL+' AND T.{col} = '''+ @{col} + ''''".n1t2());
                }

                //INT/NUMBER/DECIMAL/DOUBLE/FLOAT
                if (type.ToUpper().Equals("INT") || type.ToUpper().Equals("SMALLINT")
                    || type.ToUpper().Equals("NUMBER") || type.ToUpper().Equals("NUMERIC")
                    || type.ToUpper().Equals("DECIMAL") || type.ToUpper().Equals("DOUBLE")
                    || type.ToUpper().Equals("FLOAT") || type.ToUpper().Equals("MONEY"))
                {
                    sb.Append($"IF (@{col}>0)".n1t1());
                    sb.Append($"SET @strSQL = @strSQL+' AND T.{col} = '+ CAST(@{col} AS VARCHAR(" + row["MAX_LENGTH"] + "))".n1t2());
                }

                //CHAR/VARCHAR/VARCHAR2/STRING
                if (type.ToUpper().Equals("VARCHAR")
                    || type.ToUpper().Equals("VARCHAR2")
                    || type.ToUpper().Equals("CHAR")
                    || type.ToUpper().Equals("STRING")
                    || type.ToUpper().Equals("NVARCHAR"))
                {
                    sb.Append($"IF(LEN(@{col})>0)".n1t1());
                    sb.Append($"SET @strSQL = @strSQL+' AND UPPER(T.{col}) = '''+ UPPER(@{col}) + ''''".n1t1());
                    //sb.Append($"\n\t\t strSql := strSql || ' AND UPPER(T.{col}) LIKE ''%' || REPLACE(UPPER(p{col}),'''','''''') || '%''';");
                }
                //DateTime
                if (type.ToUpper().Equals("DATETIME"))
                {
                    sb.Append($"IF(LEN(@{col}From)>0 AND LEN(@{col}To)>0)".n1t1());
                    sb.Append($"SET @strSQL = @strSQL+' AND T.{col} BETWEEN '''+ @{col}From + ''' AND ''' + @{col}To + ''''".n1t2());
                    //sb.Append($"\n\t ELSIF p{col}From IS NOT NULL  AND p{col}To IS NULL  THEN");
                    //sb.Append($"\n\t\t strSql := strSql || ' AND T.{col} >= '''|| p{col}From|| '''' ;");
                    //sb.Append($"\n\t ELSIF p{col}From IS NULL AND p{col}To IS NOT NULL  THEN");
                    //sb.Append($"\n\t\t strSql := strSql || ' AND T.{col} <= ''' ||p{col}To|| '''';");
                }
            }

            sb.Append("SET @ParmDefinition = '@numTotalRowsOUT numeric OUTPUT';".n1t0().n1t1());
            sb.Append("SET @sqlTotal = 'SELECT @numTotalRowsOUT=count(*) ' + @strSQL".n1t1());
            sb.Append("EXECUTE sp_executesql @sqlTotal, @ParmDefinition, @numTotalRowsOUT = @numTotalRows OUTPUT".n1t1());

            sb.Append("-- Paging and Sorting Parameters".n1t0().n1t1());
            sb.Append("IF(LEN(@SortBy)=0) set @SortBy = '" + _NUMID + "'".n1t1());
            sb.Append("IF(LEN(@SortOrder)=0) set @SortOrder = 'DESC'".n1t1());

            sb.AppendLine();
            
            //Select Statement
            sb.Append("SET @strQry = 'WITH T AS (SELECT ROW_NUMBER() OVER (ORDER BY '".n1t2());
            sb.Append("+ @SortBy + ' ' + @SortOrder + ' ) AS ROWID, * FROM ( SELECT ".n1t2());

            foreach (DataRow row in dt.Rows)
            {
				var type = row["DATA_TYPE"].ToString();
				var col = row["COLUMN_NAME"].ToString();

				if (col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT))
                    continue;
                sb.Append($"{col},".n1t3());
            }
            sb.Remove(sb.Length - 1, 1); //To Remove last Comma
            sb.Append(" '");

            sb.Append(" + @strSQL + ') AS T) SELECT T.* FROM T WHERE 1=1 '".n1t2());

            sb.Append(" IF(@maximumRows > 0)".n1t2());
            sb.Append("SET @strQry = @strQry + ' AND ROWID BETWEEN ' ".n1t3());
            sb.Append("+ CAST(@startRowIndex as varchar(20)) + ' and ' ".n1t3());
            sb.Append("+ CAST(@startRowIndex + @maximumRows -1 as varchar(20))	".n1t3());

            sb.Append("PRINT(@strQry)".n1t1());
            sb.Append("EXEC (@strQry)".n1t1());
            sb.Append("END".n1t0());

            sb.Append("IF @@error <> 0 GOTO procError".n1t0().n1t1());
            sb.Append("GOTO procEnd".n1t2());

            sb.Append("procError:".n1t0().n1t1());
            sb.Append("SET @numErrorCode = @@error".n1t2());
            sb.Append("SELECT @strErrorMsg = [description] FROM master.dbo.sysmessages WHERE error = @numErrorCode".n1t2());
            sb.Append("INSERT INTO error_log (LogDate,Source,ErrMsg) VALUES (getDate(),'" + strProcName + "',@strErrorMsg)".n1t2());

            sb.Append("procEnd:".n1t0());

            //MessageBox.Show(sb.ToString());
            return sb;
        }

        public StringBuilder BuildGetProcedureParameterized(string strTableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            string strProcName = $"usp{strTableName.Replace("T_", "").Replace("t_", "").Replace("tbl", "")}Get";

            sb.Append("CREATE PROCEDURE " + strProcName + "(");
            foreach (DataRow row in dt.Rows)
            {
				var type = row["DATA_TYPE"].ToString();
				var col = row["COLUMN_NAME"].ToString();

				if (col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT) ||
                    type.ToUpper().Equals("IMAGE") || type.ToUpper().Equals("TEXT") ||
                    type.ToUpper().Equals("XML"))
                    continue;

                sb.AppendLine();

                //for UNIQUEIDENTIFIER
                if (type.ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append($"@{col}\t" + "VARCHAR(50)" + ",".n0t1());
                    continue;
                }

                if (type.ToUpper().Equals("DATETIME"))
                {
                    sb.Append($"@{col}From\t" + "VARCHAR(20)" + ",".n0t1());
                    sb.AppendLine();
                    sb.Append($"@{col}To\t" + "VARCHAR(20)" + ",".n0t1());
                    continue;
                }

                string strLength = type.ToUpper().Equals("VARCHAR") || type.ToUpper().Equals("CHAR") ? " (" + row["MAX_LENGTH"].ToString() + ")" : "";
                sb.Append($"@{col}\t" + type.ToUpper() + strLength + ",".n0t1());
            }
            //sb.Remove(sb.Length -1, 1); //To Remove last Comma
            sb.Append("@strMode  VARCHAR(1), --0 for total count, 1 for single detail view, 2 for grid view".n1t1());
            sb.Append("@strSortBy    VARCHAR(50),".n1t1());
            sb.Append("@strSortType  VARCHAR(4),".n1t1());
            sb.Append("@startRowIndex    INT,".n1t1());
            sb.Append("@maximumRows  INT, -- OPTIONAL ; 0 TO GET ALL SELECTED".n1t1());
            sb.Append("@numErrorCode INT OUTPUT,".n1t1());
            sb.Append("@strErrorMsg  VARCHAR(200) OUTPUT)".n1t1());

            sb.Append("AS".n1t0());
            sb.Append("SET NOCOUNT ON".n1t1());
            sb.Append("SET ANSI_NULLS OFF".n1t1());
            sb.Append("SET @numErrorCode = 0".n1t1());
            sb.Append("SET @strErrorMsg = 'Successful'".n1t1());

            sb.Append("BEGIN".n1t0());

            sb.AppendLine();
            StringBuilder sbSearchParams = new StringBuilder();
            StringBuilder sbSortParams = new StringBuilder();
            foreach (DataRow row in dt.Rows)
            {
                var type = row["DATA_TYPE"].ToString();
                var col = row["COLUMN_NAME"].ToString();

                if (col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT) ||
                    type.ToUpper().Equals("IMAGE") || type.ToUpper().Equals("TEXT") ||
                    type.ToUpper().Equals("XML"))
                    continue;

                //for UNIQUEIDENTIFIER
                if (type.ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append($"IF(LEN(@{col}) <> 36)".n1t1());
                    sb.Append($"SET @{col} = NULL;".n0t1());

                    sbSearchParams.Append($"AND ( T.{col} = @{col} OR @{col} IS NULL )".n1t2());
                }

                //INT/NUMBER/DECIMAL/DOUBLE/FLOAT
                if (type.ToUpper().Equals("INT") || type.ToUpper().Equals("SMALLINT")
                    || type.ToUpper().Equals("NUMBER") || type.ToUpper().Equals("DECIMAL")
                    || type.ToUpper().Equals("DOUBLE") || type.ToUpper().Equals("FLOAT")
                    || type.ToUpper().Equals("NUMERIC") || type.ToUpper().Equals("MONEY")
                    )
                {
                    sbSearchParams.Append($"AND ( T.{col} = @{col} OR @{col} <= 0 )".n1t2());
                }

                //CHAR/VARCHAR/VARCHAR2/STRING
                if (type.ToUpper().Equals("VARCHAR") || type.ToUpper().Equals("VARCHAR2")
                    || type.ToUpper().Equals("CHAR") || type.ToUpper().Equals("STRING") || type.ToUpper().Equals("NVARCHAR"))
                {
                    sb.Append($"\n\t IF(LEN(@{col})=0)".n1t1());
                    sb.Append($"\t SET @{col} = NULL;".n0t1());

                    sbSearchParams.Append($"\n\t\t AND ( T.{col} = @{col} OR @{col} IS NULL )".n1t2());
                }

                //DateTime
                if (type.ToUpper().Equals("DATETIME"))
                {
                    sb.Append($"\n\t IF(LEN(@{col}From)=0)".n1t1());
                    sb.Append($"\t SET @{col}From = NULL;".n0t1());

                    sb.Append($"\n\t IF(LEN(@{col}To)=0)".n1t1());
                    sb.Append($"\t SET @{col} To = NULL;".n0t1());

                    //sbConditions.Append("\n\t\t AND T." + col + " BETWEEN '''+ @" + col + "From + ''' AND ''' + @" + col + "To + ''''");
                    sbSearchParams.Append($"AND (T.{col} >= + @{col}From + OR + @{col}From + IS NULL)".n1t2());
                    sbSearchParams.Append($"AND (T.{col} <= + @{col}To + OR + @{col}To + IS NULL)".n1t2());
                }

                //SortParams
                sbSortParams.Append($"CASE WHEN UPPER(@strSortBy) = '{col.ToUpper()}' AND UPPER(@strSortType) = 'ASC' THEN {col} END ASC,".n1t2());
                sbSortParams.Append($"CASE WHEN UPPER(@strSortBy) = '{col.ToUpper()}' AND UPPER(@strSortType) = 'DESC' THEN {col} END DESC,".n1t2());
            }
            sbSortParams.Append($"CASE WHEN (@strSortBy IS NULL OR @strSortBy = '') THEN {_NUMID} END DESC".n1t2());

            sb.AppendLine();

            //RowCount
            sb.Append("IF (@strMode = '0') -- FOR ROW COUNT ".n1t1());
            sb.Append("BEGIN".n1t1());
            sb.Append("SELECT COUNT(1) TOTALROWS FROM " + strTableName + " T ".n1t2());
            sb.Append("WHERE 0 = 0 ".n1t2());
            sb.Append(sbSearchParams.ToString());
            sb.Append("END".n1t1());
            //SelectAll
            sb.Append("IF (@strMode = '1') -- For Populate ".n1t1());
            sb.Append("BEGIN".n1t1());
            sb.Append("SELECT T.* FROM " + strTableName + " T WHERE 0=0 ".n1t2());
            sb.Append(sbSearchParams.ToString());
            sb.Append("END".n1t1());
            sb.Append("IF (@strMode = '2') -- Grid View and Join with Other Related Table".n1t1());
            sb.Append("BEGIN".n1t1());
            sb.Append("WITH T AS (SELECT ROW_NUMBER() OVER (ORDER BY ".n1t2());
            sb.Append(sbSortParams.ToString());
            sb.Append(") AS ROWID, * FROM ( SELECT ".n1t2());

            foreach (DataRow row in dt.Rows)
            {
				var type = row["DATA_TYPE"].ToString();
				var col = row["COLUMN_NAME"].ToString();

				if (col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT))
                    continue;
                sb.Append("T." + col + ",".n1t3());
            }
            sb.Remove(sb.Length - 1, 1); //To Remove last Comma

            sb.Append("FROM " + strTableName + " T WHERE 0=0 ".n1t2());
            sb.Append(sbSearchParams.ToString());
            sb.Append(") AS T) SELECT T.* FROM T WHERE 1=1 ".n1t2());
            sb.Append("AND (ROWID >= @startRowIndex OR @startRowIndex IS NULL OR @startRowIndex <= 0)".n1t2());
            sb.Append("AND (ROWID <= @startRowIndex + @maximumRows - 1 OR @maximumRows IS NULL OR @maximumRows <= 0)".n1t2());
            sb.Append("END".n1t1());

            sb.Append("END".n1t0());

            sb.Append("IF @@error <> 0 GOTO procError".n1t0().n1t1());
            sb.Append("GOTO procEnd".n1t2());

            sb.Append("procError:".n1t0().n1t1());
            sb.Append("SET @numErrorCode = @@error".n1t2());
            sb.Append("SELECT @strErrorMsg = [description] FROM master.dbo.sysmessages WHERE error = @numErrorCode".n1t2());
            sb.Append("INSERT INTO error_log (LogDate,Source,ErrMsg) VALUES (getDate(),'" + strProcName + "',@strErrorMsg)".n1t2());

            sb.Append("procEnd:".n1t0());

            //MessageBox.Show(sb.ToString());
            return sb;
        }
    }
}
