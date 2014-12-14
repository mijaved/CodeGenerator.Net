using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CodeGenerator.MVP.Util
{
    public class DbBuilderSqlServer : IDbBuilder
    {
        public DbBuilderSqlServer()
        {
        }

        public StringBuilder BuildSequence(string strTableName)
        {
            StringBuilder sb = new StringBuilder();
            
            return sb;
        }

        public StringBuilder BuildSaveProcedure(string strTableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            string strProcName = "usp" + strTableName.Replace("T_", "").Replace("t_", "").Replace("tbl", "") + "Save";

            sb.Append("CREATE PROCEDURE " + strProcName + "(");
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreatorColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifierColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreateDateColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifiedDateColName()))
                    continue;

                sb.AppendLine();

                //for UNIQUEIDENTIFIER
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append("\t @" + row["COLUMN_NAME"] + "\t" + "VARCHAR(50)" + ",");
                    continue;
                }

                string strLength = row["DATA_TYPE"].ToString().ToUpper().Equals("VARCHAR") || row["DATA_TYPE"].ToString().ToUpper().Equals("CHAR") ? " (" + row["MAX_LENGTH"].ToString() + ")" : "";
                sb.Append("\t @" + row["COLUMN_NAME"] + "\t" + row["DATA_TYPE"].ToString().ToUpper() + strLength + ",");
            }
            //sb.Remove(sb.Length -1, 1); //To Remove last Comma
            sb.Append("\n\t @" + Util.Utility.GetRecordCreatorColName() + "   VARCHAR(20), \n\t @strMode  CHAR(1),");
            sb.Append("\n\t @strErrorCode VARCHAR(50) OUTPUT, --@numErrorCode INT OUTPUT, \n\t @strErrorMsg VARCHAR(200) OUTPUT)");


            sb.Append("\n AS");
            sb.Append("\n\t SET @strErrorCode = 0 --@numErrorCode = 0");
            sb.Append("\n\t SET @strErrorMsg = 'Successful'");
            sb.Append("\n\t DECLARE @strSQL VARCHAR(4000);");
            sb.Append("\n BEGIN");

            //Insert
            sb.Append("\n\t IF (@strMode='I')");
            sb.Append("\n\t BEGIN");
            sb.Append("\n\t\t --SET @" + Util.Utility.GetPkColName() + " = NEWID()");
            sb.Append("\n\t\t INSERT INTO " + strTableName + "(");
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Equals(Util.Utility.GetPkColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreatorColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifierColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreateDateColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifiedDateColName()))
                    continue;

                sb.AppendLine();
                sb.Append("\t\t\t " + row["COLUMN_NAME"] + ",");
            }
            sb.Append("\n\t\t\t " + Util.Utility.GetRecordCreatorColName() + ", \n\t\t\t " + Util.Utility.GetRecordModifierColName() + ")");

            sb.Append("\n\t\t VALUES(");
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Equals(Util.Utility.GetPkColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreatorColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifierColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreateDateColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifiedDateColName()))
                    continue;

                sb.AppendLine();
                sb.Append("\t\t\t @" + row["COLUMN_NAME"] + ",");
            }
            sb.Append("\n\t\t\t @" + Util.Utility.GetRecordCreatorColName() + ", \n\t\t\t @" + Util.Utility.GetRecordCreatorColName() + ");");
            sb.Append("\n\t\t --SELECT @@IDENTITY;");
            sb.Append("\n\t\t SET @" + Util.Utility.GetPkColName() + "=@@identity;");
            sb.Append("\n\t END");
            
            //Edit
            sb.Append("\n\t ELSE IF (@strMode='U')");
            sb.Append("\n\t BEGIN");
            sb.Append("\n\t\t UPDATE " + strTableName + " SET");
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Equals(Util.Utility.GetPkColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreatorColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifierColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreateDateColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifiedDateColName()))
                    continue;

                sb.AppendLine();
                sb.Append("\t\t\t " + row["COLUMN_NAME"] + " \t = @" + row["COLUMN_NAME"] + ",");
            }
            sb.Append("\n\t\t\t " + Util.Utility.GetRecordModifierColName() + "=@" + Util.Utility.GetRecordCreatorColName() + "");
            sb.Append("\n\t\t\t WHERE " + Util.Utility.GetPkColName() + "=@" + Util.Utility.GetPkColName() + " ;");
            sb.Append("\n\t\t --SELECT @" + Util.Utility.GetPkColName() + ";");
            sb.Append("\n\t END");
            
            //Delete
            sb.Append("\n\t ELSE IF (@strMode='D')");
            sb.Append("\n\t BEGIN");
            sb.Append("\n\t\t DELETE FROM " + strTableName + " WHERE " + Util.Utility.GetPkColName() + " = @" + Util.Utility.GetPkColName() + " ;");
            //sb.Append("\n\t\t UPDATE TBLAUDITMASTER SET STRUSER=p_STRUID WHERE NUMSESSIONID=userenv('sessionid');");
            sb.Append("\n\t END");
            sb.Append("\n END");

            sb.Append("\n\t SET @strErrorCode = @" + Util.Utility.GetPkColName()); sb.Append("\t --SET @numErrorCode = @" + Util.Utility.GetPkColName());
            sb.Append("\n\t IF @@error <> 0 goto procError");
            sb.Append("\n\t\t goto procEnd");

            sb.Append("\n\n\t procError:");
            sb.Append("\n\t\t SET @strErrorCode = @@error"); sb.Append("\t --SET @numErrorCode = @@error");
            sb.Append("\n\t\t select @strErrorMsg = [description] from master.dbo.sysmessages where error = @strErrorCode --@numErrorCode");
            sb.Append("\n\t\t insert into error_log (LogDate,Source,ErrMsg) values (getdate(),'" + strProcName + "',@strErrorMsg)");

            sb.Append("\n procEnd:");
            //MessageBox.Show(sb.ToString());

            return sb;
        }

        public StringBuilder BuildGetProcedure(string strTableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            string strProcName = "usp" + strTableName.Replace("T_", "").Replace("t_", "").Replace("tbl", "") + "Get";

            sb.Append("CREATE PROCEDURE " + strProcName + "(");
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreatorColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifierColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreateDateColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifiedDateColName()) ||
                    row["DATA_TYPE"].ToString().ToUpper().Equals("IMAGE") || row["DATA_TYPE"].ToString().ToUpper().Equals("TEXT") ||
                    row["DATA_TYPE"].ToString().ToUpper().Equals("XML"))
                    continue;

                sb.AppendLine();

                //for UNIQUEIDENTIFIER
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append("\t @" + row["COLUMN_NAME"] + "\t" + "VARCHAR(50)" + ",");
                    continue;
                }

                if (row["DATA_TYPE"].ToString().ToUpper().Equals("DATETIME"))
                {
                    sb.Append("\t @" + row["COLUMN_NAME"] + "From\t" + "VARCHAR(20)" + ",");
                    sb.AppendLine();
                    sb.Append("\t @" + row["COLUMN_NAME"] + "To\t" + "VARCHAR(20)" + ",");
                    continue;
                }

                string strLength = row["DATA_TYPE"].ToString().ToUpper().Equals("VARCHAR") || row["DATA_TYPE"].ToString().ToUpper().Equals("CHAR") ? " (" + row["MAX_LENGTH"].ToString() + ")" : "";
                sb.Append("\t @" + row["COLUMN_NAME"] + "\t" + row["DATA_TYPE"].ToString().ToUpper() + strLength + ",");
            }
            //sb.Remove(sb.Length -1, 1); //To Remove last Comma
            sb.Append("\n\t @strMode  VARCHAR(1), --0 for total count, 1 for single detail view, 2 for grid view");
            sb.Append("\n\t @strSortBy    VARCHAR(50),");
            sb.Append("\n\t @strSortType  VARCHAR(4),");
            sb.Append("\n\t @startRowIndex    INT,");
            sb.Append("\n\t @maximumRows  INT, -- OPTIONAL ; 0 TO GET ALL SELECTED");
            sb.Append("\n\t @numErrorCode INT OUTPUT,");
            sb.Append("\n\t @strErrorMsg  VARCHAR(200) OUTPUT)");

            sb.Append("\n AS");
            sb.Append("\n\t SET NOCOUNT ON");
            sb.Append("\n\t SET ANSI_NULLS OFF");
            sb.Append("\n\t SET @numErrorCode = 0");
            sb.Append("\n\t SET @strErrorMsg = 'Successful'");
            sb.Append("\n\t DECLARE @strSQL VARCHAR(4000);");
            sb.Append("\n\t DECLARE @strQry VARCHAR(4000);");

            sb.Append("\n BEGIN");
            sb.Append("\n\t SET @strSQL=''");
            sb.Append("\n\t SET @strQry =''");

            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreatorColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifierColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreateDateColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifiedDateColName()) ||
                    row["DATA_TYPE"].ToString().ToUpper().Equals("IMAGE") || row["DATA_TYPE"].ToString().ToUpper().Equals("TEXT") ||
                    row["DATA_TYPE"].ToString().ToUpper().Equals("XML"))
                    continue;

                sb.AppendLine();

                //for UNIQUEIDENTIFIER
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append("\n\t IF(LEN(@" + row["COLUMN_NAME"] + ")>0)");
                    sb.Append("\n\t\t SET @strSQL = @strSQL+' AND T." + row["COLUMN_NAME"] + " = '''+ @" + row["COLUMN_NAME"] + " + ''''");
                }

                //INT/NUMBER/DECIMAL/DOUBLE/FLOAT
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("INT")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("NUMBER")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("DECIMAL")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("DOUBLE")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("FLOAT")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("SMALLINT")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("NUMERIC")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("MONEY")
                    )
                {
                    sb.Append("\n\t IF (@" + row["COLUMN_NAME"] + ">0)");
                    sb.Append("\n\t\t SET @strSQL = @strSQL+' AND T." + row["COLUMN_NAME"] + " = '+ CAST(@" + row["COLUMN_NAME"] + " AS VARCHAR(" + row["MAX_LENGTH"] + "))");
                }

                //CHAR/VARCHAR/VARCHAR2/STRING
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("VARCHAR")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("VARCHAR2")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("CHAR")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("STRING")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("NVARCHAR"))
                {
                    sb.Append("\n\t IF(LEN(@" + row["COLUMN_NAME"] + ")>0)");
                    sb.Append("\n\t\t SET @strSQL = @strSQL+' AND UPPER(T." + row["COLUMN_NAME"] + ") = '''+ UPPER(@" + row["COLUMN_NAME"] + ") + ''''");
                    //sb.Append("\n\t\t strSql := strSql || ' AND UPPER(T." + row["COLUMN_NAME"] + ") LIKE ''%' || REPLACE(UPPER(p" + row["COLUMN_NAME"] + "),'''','''''') || '%''';");
                }
                //DateTime
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("DATETIME"))
                {
                    sb.Append("\n\t IF(LEN(@" + row["COLUMN_NAME"] + "From)>0 AND LEN(@" + row["COLUMN_NAME"] + "To)>0)");
                    sb.Append("\n\t\t SET @strSQL = @strSQL+' AND T." + row["COLUMN_NAME"] + " BETWEEN '''+ @" + row["COLUMN_NAME"] + "From + ''' AND ''' + @" + row["COLUMN_NAME"] + "To + ''''");
                    //sb.Append("\n\t ELSIF p" + row["COLUMN_NAME"] + "From IS NOT NULL  AND p" + row["COLUMN_NAME"] + "To IS NULL  THEN");
                    //sb.Append("\n\t\t strSql := strSql || ' AND T." + row["COLUMN_NAME"] + " >= '''|| p" + row["COLUMN_NAME"] + "From|| '''' ;");
                    //sb.Append("\n\t ELSIF p" + row["COLUMN_NAME"] + "From IS NULL AND p" + row["COLUMN_NAME"] + "To IS NOT NULL  THEN");
                    //sb.Append("\n\t\t strSql := strSql || ' AND T." + row["COLUMN_NAME"] + " <= ''' ||p" + row["COLUMN_NAME"] + "To|| '''';");
                }
            }

            sb.Append("\n\n\t -- Paging and Sorting Parameters");
            sb.Append("\n\t IF(LEN(@strSortBy)=0) set @strSortBy = '" + Util.Utility.GetPkColName() + "'");
            sb.Append("\n\t IF(LEN(@strSortType)=0) set @strSortType = 'DESC'");

            sb.AppendLine();

            //RowCount
            sb.Append("\n\t IF (@strMode = '0') -- FOR ROW COUNT ");
            sb.Append("\n\t BEGIN");
            sb.Append("\n\t\t SET @strQry ='SELECT COUNT(1) TOTALROWS FROM " + strTableName + " T ");
            sb.Append("\n\t\t WHERE 0 = 0 ' + @strSQL");
            sb.Append("\n\t END");
            //SelectAll
            sb.Append("\n\t IF (@strMode = '1') -- GRID VIEW ");
            sb.Append("\n\t BEGIN");
            sb.Append("\n\t\t SET @strQry = 'SELECT * FROM " + strTableName + " T WHERE 0=0 '+ @strSQL");
            sb.Append("\n\t END");
            sb.Append("\n\t IF (@strMode = '2') -- Join with Other Related Table");
            sb.Append("\n\t BEGIN");
            sb.Append("\n\t\t SET @strQry = 'WITH T AS (SELECT ROW_NUMBER() OVER (ORDER BY '");
            sb.Append("\n\t\t + @strSortBy + ' ' + @strSortType + ' ) AS ROWID, * FROM ( SELECT ");

            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreatorColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifierColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreateDateColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifiedDateColName()))
                    continue;
                sb.Append("\n\t\t\t " + row["COLUMN_NAME"] + ",");
            }
            sb.Remove(sb.Length -1, 1); //To Remove last Comma

            sb.Append("\n\t\t FROM " + strTableName + " T WHERE 0=0 '");
            sb.Append("\n\t\t + @strSQL + ') AS T) SELECT T.* FROM T WHERE 1=1 '");

            sb.Append("\n\t\t\t IF(@maximumRows > 0)");
            sb.Append("\n\t\t\t SET @strQry = @strQry + ' AND ROWID BETWEEN ' ");
            sb.Append("\n\t\t\t + CAST(@startRowIndex as varchar(20)) + ' and ' ");
            sb.Append("\n\t\t\t + CAST(@startRowIndex + @maximumRows -1 as varchar(20))	");
            sb.Append("\n\t END");


            //sb.Append("\n\t END");
            sb.Append("\n\t print(@strQry)");
            sb.Append("\n\t exec (@strQry)");
            sb.Append("\n END");

            sb.Append("\n\n\t IF @@error <> 0 GOTO procError");
            sb.Append("\n\t\t GOTO procEnd");

            sb.Append("\n\n procError:");
            sb.Append("\n\t SET @numErrorCode = @@error");
            sb.Append("\n\t SELECT @strErrorMsg = [description] FROM master.dbo.sysmessages WHERE error = @numErrorCode");
            sb.Append("\n\t INSERT INTO error_log (LogDate,Source,ErrMsg) VALUES (getDate(),'" + strProcName + "',@strErrorMsg)");

            sb.Append("\n procEnd:");
            
            //MessageBox.Show(sb.ToString());
            return sb;
        }

        public StringBuilder BuildGetProcedureSingle(string strTableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            string strProcName = "usp" + strTableName.Replace("T_", "").Replace("t_", "").Replace("tbl", "") + "Get";

            sb.Append("CREATE PROCEDURE " + strProcName + "(");
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreatorColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifierColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreateDateColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifiedDateColName()) ||
                    row["DATA_TYPE"].ToString().ToUpper().Equals("IMAGE") || row["DATA_TYPE"].ToString().ToUpper().Equals("TEXT") ||
                    row["DATA_TYPE"].ToString().ToUpper().Equals("XML"))
                    continue;

                sb.AppendLine();

                //for UNIQUEIDENTIFIER
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append("\t @" + row["COLUMN_NAME"] + "\t" + "VARCHAR(50)" + ",");
                    continue;
                }

                if (row["DATA_TYPE"].ToString().ToUpper().Equals("DATETIME"))
                {
                    sb.Append("\t @" + row["COLUMN_NAME"] + "From\t" + row["DATA_TYPE"].ToString().ToUpper() + ",");
                    sb.AppendLine();
                    sb.Append("\t @" + row["COLUMN_NAME"] + "To\t" + row["DATA_TYPE"].ToString().ToUpper() + ",");
                    continue;
                }

                string strLength = row["DATA_TYPE"].ToString().ToUpper().Equals("VARCHAR") || row["DATA_TYPE"].ToString().ToUpper().Equals("CHAR") ? " (" + row["MAX_LENGTH"].ToString() + ")" : "";
                sb.Append("\t @" + row["COLUMN_NAME"] + "\t" + row["DATA_TYPE"].ToString().ToUpper() + strLength + ",");
            }
            sb.Append("\n\t @SortBy     VARCHAR(50),");
            sb.Append("\n\t @SortOrder  VARCHAR(4),");
            sb.Append("\n\t @startRowIndex    INT,");
            sb.Append("\n\t @maximumRows  INT, -- OPTIONAL ; 0 TO GET ALL SELECTED");
            sb.Append("\n\t @numTotalRows	INT OUTPUT,");
            sb.Append("\n\t @numErrorCode INT OUTPUT,");
            sb.Append("\n\t @strErrorMsg  VARCHAR(200) OUTPUT)");

            sb.Append("\n AS");
            sb.Append("\n\t SET NOCOUNT ON");
            sb.Append("\n\t SET ANSI_NULLS OFF");

            sb.Append("\n\t DECLARE @strSQL VARCHAR(4000);");
            sb.Append("\n\t DECLARE @strQry VARCHAR(4000);");
            sb.Append("\n\t DECLARE @sqlTotal   NVARCHAR(4000);");
            sb.Append("\n\t DECLARE @ParmDefinition NVARCHAR(1000);");
            sb.Append("\n\t DECLARE @strEditCondition   VARCHAR(4000);");

            sb.Append("\n\t SET @numErrorCode = 0");
            sb.Append("\n\t SET @strErrorMsg = 'Successful'");

            sb.Append("\n BEGIN");
            sb.Append("\n\t SET @strSQL=''");
            sb.Append("\n\t SET @strQry =''");

            //Table Name
            sb.Append("\n\n\t SET @strSQL = ' FROM " + strTableName + " T WHERE 0=0 '");

            //Search Params
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreatorColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifierColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreateDateColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifiedDateColName()) ||
                    row["DATA_TYPE"].ToString().ToUpper().Equals("IMAGE") || row["DATA_TYPE"].ToString().ToUpper().Equals("TEXT") ||
                    row["DATA_TYPE"].ToString().ToUpper().Equals("XML"))
                    continue;

                sb.AppendLine();

                //for UNIQUEIDENTIFIER
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append("\n\t IF(LEN(@" + row["COLUMN_NAME"] + ")>0)");
                    sb.Append("\n\t\t SET @strSQL = @strSQL+' AND T." + row["COLUMN_NAME"] + " = '''+ @" + row["COLUMN_NAME"] + " + ''''");
                }

                //INT/NUMBER/DECIMAL/DOUBLE/FLOAT
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("INT")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("NUMBER")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("DECIMAL")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("NUMERIC")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("DOUBLE")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("FLOAT")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("SMALLINT")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("MONEY"))
                {
                    sb.Append("\n\t IF (@" + row["COLUMN_NAME"] + ">0)");
                    sb.Append("\n\t\t SET @strSQL = @strSQL+' AND T." + row["COLUMN_NAME"] + " = '+ CAST(@" + row["COLUMN_NAME"] + " AS VARCHAR(" + row["MAX_LENGTH"] + "))");
                }

                //CHAR/VARCHAR/VARCHAR2/STRING
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("VARCHAR")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("VARCHAR2")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("CHAR")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("STRING")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("NVARCHAR"))
                {
                    sb.Append("\n\t IF(LEN(@" + row["COLUMN_NAME"] + ")>0)");
                    sb.Append("\n\t\t SET @strSQL = @strSQL+' AND UPPER(T." + row["COLUMN_NAME"] + ") = '''+ UPPER(@" + row["COLUMN_NAME"] + ") + ''''");
                    //sb.Append("\n\t\t strSql := strSql || ' AND UPPER(T." + row["COLUMN_NAME"] + ") LIKE ''%' || REPLACE(UPPER(p" + row["COLUMN_NAME"] + "),'''','''''') || '%''';");
                }
                //DateTime
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("DATETIME"))
                {
                    sb.Append("\n\t IF(LEN(@" + row["COLUMN_NAME"] + "From)>0 AND LEN(@" + row["COLUMN_NAME"] + "To)>0)");
                    sb.Append("\n\t\t SET @strSQL = @strSQL+' AND T." + row["COLUMN_NAME"] + " BETWEEN '''+ @" + row["COLUMN_NAME"] + "From + ''' AND ''' + @" + row["COLUMN_NAME"] + "To + ''''");
                    //sb.Append("\n\t ELSIF p" + row["COLUMN_NAME"] + "From IS NOT NULL  AND p" + row["COLUMN_NAME"] + "To IS NULL  THEN");
                    //sb.Append("\n\t\t strSql := strSql || ' AND T." + row["COLUMN_NAME"] + " >= '''|| p" + row["COLUMN_NAME"] + "From|| '''' ;");
                    //sb.Append("\n\t ELSIF p" + row["COLUMN_NAME"] + "From IS NULL AND p" + row["COLUMN_NAME"] + "To IS NOT NULL  THEN");
                    //sb.Append("\n\t\t strSql := strSql || ' AND T." + row["COLUMN_NAME"] + " <= ''' ||p" + row["COLUMN_NAME"] + "To|| '''';");
                }
            }

            sb.Append("\n\n\t SET @ParmDefinition = '@numTotalRowsOUT numeric OUTPUT';");
            sb.Append("\n\t SET @sqlTotal = 'SELECT @numTotalRowsOUT=count(*) ' + @strSQL");
            sb.Append("\n\t EXECUTE sp_executesql @sqlTotal, @ParmDefinition, @numTotalRowsOUT = @numTotalRows OUTPUT");

            sb.Append("\n\n\t -- Paging and Sorting Parameters");
            sb.Append("\n\t IF(LEN(@SortBy)=0) set @SortBy = '" + Util.Utility.GetPkColName() + "'");
            sb.Append("\n\t IF(LEN(@SortOrder)=0) set @SortOrder = 'DESC'");

            sb.AppendLine();
            
            //Select Statement
            sb.Append("\n\t\t SET @strQry = 'WITH T AS (SELECT ROW_NUMBER() OVER (ORDER BY '");
            sb.Append("\n\t\t + @SortBy + ' ' + @SortOrder + ' ) AS ROWID, * FROM ( SELECT ");

            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreatorColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifierColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreateDateColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifiedDateColName()))
                    continue;
                sb.Append("\n\t\t\t " + row["COLUMN_NAME"] + ",");
            }
            sb.Remove(sb.Length - 1, 1); //To Remove last Comma
            sb.Append(" '");

            sb.Append("\n\t\t + @strSQL + ') AS T) SELECT T.* FROM T WHERE 1=1 '");

            sb.Append("\n\t\t IF(@maximumRows > 0)");
            sb.Append("\n\t\t\t SET @strQry = @strQry + ' AND ROWID BETWEEN ' ");
            sb.Append("\n\t\t\t + CAST(@startRowIndex as varchar(20)) + ' and ' ");
            sb.Append("\n\t\t\t + CAST(@startRowIndex + @maximumRows -1 as varchar(20))	");

            sb.Append("\n\t PRINT(@strQry)");
            sb.Append("\n\t EXEC (@strQry)");
            sb.Append("\n END");

            sb.Append("\n\n\t IF @@error <> 0 GOTO procError");
            sb.Append("\n\t\t GOTO procEnd");

            sb.Append("\n\n\t procError:");
            sb.Append("\n\t\t SET @numErrorCode = @@error");
            sb.Append("\n\t\t SELECT @strErrorMsg = [description] FROM master.dbo.sysmessages WHERE error = @numErrorCode");
            sb.Append("\n\t\t INSERT INTO error_log (LogDate,Source,ErrMsg) VALUES (getDate(),'" + strProcName + "',@strErrorMsg)");

            sb.Append("\n procEnd:");

            //MessageBox.Show(sb.ToString());
            return sb;
        }

        public StringBuilder BuildGetProcedureParameterized(string strTableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            string strProcName = "usp" + strTableName.Replace("T_", "").Replace("t_", "").Replace("tbl", "") + "Get";

            sb.Append("CREATE PROCEDURE " + strProcName + "(");
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreatorColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifierColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreateDateColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifiedDateColName()) ||
                    row["DATA_TYPE"].ToString().ToUpper().Equals("IMAGE") || row["DATA_TYPE"].ToString().ToUpper().Equals("TEXT") ||
                    row["DATA_TYPE"].ToString().ToUpper().Equals("XML"))
                    continue;

                sb.AppendLine();

                //for UNIQUEIDENTIFIER
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append("\t @" + row["COLUMN_NAME"] + "\t" + "VARCHAR(50)" + ",");
                    continue;
                }

                if (row["DATA_TYPE"].ToString().ToUpper().Equals("DATETIME"))
                {
                    sb.Append("\t @" + row["COLUMN_NAME"] + "From\t" + "VARCHAR(20)" + ",");
                    sb.AppendLine();
                    sb.Append("\t @" + row["COLUMN_NAME"] + "To\t" + "VARCHAR(20)" + ",");
                    continue;
                }

                string strLength = row["DATA_TYPE"].ToString().ToUpper().Equals("VARCHAR") || row["DATA_TYPE"].ToString().ToUpper().Equals("CHAR") ? " (" + row["MAX_LENGTH"].ToString() + ")" : "";
                sb.Append("\t @" + row["COLUMN_NAME"] + "\t" + row["DATA_TYPE"].ToString().ToUpper() + strLength + ",");
            }
            //sb.Remove(sb.Length -1, 1); //To Remove last Comma
            sb.Append("\n\t @strMode  VARCHAR(1), --0 for total count, 1 for single detail view, 2 for grid view");
            sb.Append("\n\t @strSortBy    VARCHAR(50),");
            sb.Append("\n\t @strSortType  VARCHAR(4),");
            sb.Append("\n\t @startRowIndex    INT,");
            sb.Append("\n\t @maximumRows  INT, -- OPTIONAL ; 0 TO GET ALL SELECTED");
            sb.Append("\n\t @numErrorCode INT OUTPUT,");
            sb.Append("\n\t @strErrorMsg  VARCHAR(200) OUTPUT)");

            sb.Append("\n AS");
            sb.Append("\n\t SET NOCOUNT ON");
            sb.Append("\n\t SET ANSI_NULLS OFF");
            sb.Append("\n\t SET @numErrorCode = 0");
            sb.Append("\n\t SET @strErrorMsg = 'Successful'");

            sb.Append("\n BEGIN");

            sb.AppendLine();
            StringBuilder sbSearchParams = new StringBuilder();
            StringBuilder sbSortParams = new StringBuilder();
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreatorColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifierColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreateDateColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifiedDateColName()) ||
                    row["DATA_TYPE"].ToString().ToUpper().Equals("IMAGE") || row["DATA_TYPE"].ToString().ToUpper().Equals("TEXT") ||
                    row["DATA_TYPE"].ToString().ToUpper().Equals("XML"))
                    continue;

                //for UNIQUEIDENTIFIER
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("UNIQUEIDENTIFIER"))
                {
                    sb.Append("\n\t IF(LEN(@" + row["COLUMN_NAME"] + ") <> 36)");
                    sb.Append("\t SET @" + row["COLUMN_NAME"] + " = NULL;");

                    sbSearchParams.Append("\n\t\t AND ( T." + row["COLUMN_NAME"] + " = @" + row["COLUMN_NAME"] + " OR @" + row["COLUMN_NAME"] + " IS NULL )");
                }

                //INT/NUMBER/DECIMAL/DOUBLE/FLOAT
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("INT")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("NUMBER")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("DECIMAL")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("DOUBLE")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("FLOAT")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("SMALLINT")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("NUMERIC")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("MONEY")
                    )
                {
                    sbSearchParams.Append("\n\t\t AND ( T." + row["COLUMN_NAME"] + " = @" + row["COLUMN_NAME"] + " OR @" + row["COLUMN_NAME"] + " <= 0 )");
                }

                //CHAR/VARCHAR/VARCHAR2/STRING
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("VARCHAR")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("VARCHAR2")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("CHAR")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("STRING")
                    || row["DATA_TYPE"].ToString().ToUpper().Equals("NVARCHAR"))
                {
                    sb.Append("\n\t IF(LEN(@" + row["COLUMN_NAME"] + ")=0)");
                    sb.Append("\t SET @" + row["COLUMN_NAME"] + " = NULL;");

                    sbSearchParams.Append("\n\t\t AND ( T." + row["COLUMN_NAME"] + " = @" + row["COLUMN_NAME"] + " OR @" + row["COLUMN_NAME"] + " IS NULL )");
                }

                //DateTime
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("DATETIME"))
                {
                    sb.Append("\n\t IF(LEN(@" + row["COLUMN_NAME"] + "From)=0)");
                    sb.Append("\t SET @" + row["COLUMN_NAME"] + "From = NULL;");

                    sb.Append("\n\t IF(LEN(@" + row["COLUMN_NAME"] + "To)=0)");
                    sb.Append("\t SET @" + row["COLUMN_NAME"] + "To = NULL;");

                   //sbConditions.Append("\n\t\t AND T." + row["COLUMN_NAME"] + " BETWEEN '''+ @" + row["COLUMN_NAME"] + "From + ''' AND ''' + @" + row["COLUMN_NAME"] + "To + ''''");
                    sbSearchParams.Append("\n\t\t AND (T." + row["COLUMN_NAME"] + " >= + @" + row["COLUMN_NAME"] + "From + OR + @" + row["COLUMN_NAME"] + "From + IS NULL)");
                    sbSearchParams.Append("\n\t\t AND (T." + row["COLUMN_NAME"] + " <= + @" + row["COLUMN_NAME"] + "To + OR + @" + row["COLUMN_NAME"] + "To + IS NULL)");
                }

                //SortParams
                sbSortParams.Append("\n\t\t CASE WHEN UPPER(@strSortBy) = '" + row["COLUMN_NAME"].ToString().ToUpper() + "' AND UPPER(@strSortType) = 'ASC' THEN " + row["COLUMN_NAME"] + " END ASC,");
                sbSortParams.Append("\n\t\t CASE WHEN UPPER(@strSortBy) = '" + row["COLUMN_NAME"].ToString().ToUpper() + "' AND UPPER(@strSortType) = 'DESC' THEN " + row["COLUMN_NAME"] + " END DESC,");
            }
            sbSortParams.Append("\n\t\t CASE WHEN (@strSortBy IS NULL OR @strSortBy = '') THEN " + Util.Utility.GetPkColName() + " END DESC");

            sb.AppendLine();

            //RowCount
            sb.Append("\n\t IF (@strMode = '0') -- FOR ROW COUNT ");
            sb.Append("\n\t BEGIN");
            sb.Append("\n\t\t SELECT COUNT(1) TOTALROWS FROM " + strTableName + " T ");
            sb.Append("\n\t\t WHERE 0 = 0 ");
            sb.Append(sbSearchParams.ToString());
            sb.Append("\n\t END");
            //SelectAll
            sb.Append("\n\t IF (@strMode = '1') -- For Populate ");
            sb.Append("\n\t BEGIN");
            sb.Append("\n\t\t SELECT T.* FROM " + strTableName + " T WHERE 0=0 ");
            sb.Append(sbSearchParams.ToString());
            sb.Append("\n\t END");
            sb.Append("\n\t IF (@strMode = '2') -- Grid View and Join with Other Related Table");
            sb.Append("\n\t BEGIN");
            sb.Append("\n\t\t WITH T AS (SELECT ROW_NUMBER() OVER (ORDER BY ");
            sb.Append(sbSortParams.ToString());
            sb.Append("\n\t\t ) AS ROWID, * FROM ( SELECT ");

            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreatorColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifierColName()) ||
                    row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordCreateDateColName()) || row["COLUMN_NAME"].ToString().Contains(Util.Utility.GetRecordModifiedDateColName()))
                    continue;
                sb.Append("\n\t\t\t T." + row["COLUMN_NAME"] + ",");
            }
            sb.Remove(sb.Length - 1, 1); //To Remove last Comma

            sb.Append("\n\t\t FROM " + strTableName + " T WHERE 0=0 ");
            sb.Append(sbSearchParams.ToString());
            sb.Append("\n\t\t ) AS T) SELECT T.* FROM T WHERE 1=1 ");
            sb.Append("\n\t\t AND (ROWID >= @startRowIndex OR @startRowIndex IS NULL OR @startRowIndex <= 0)");
            sb.Append("\n\t\t AND (ROWID <= @startRowIndex + @maximumRows - 1 OR @maximumRows IS NULL OR @maximumRows <= 0)");
            sb.Append("\n\t END");

            sb.Append("\n END");

            sb.Append("\n\n\t IF @@error <> 0 GOTO procError");
            sb.Append("\n\t\t GOTO procEnd");

            sb.Append("\n\n\t procError:");
            sb.Append("\n\t\t SET @numErrorCode = @@error");
            sb.Append("\n\t\t SELECT @strErrorMsg = [description] FROM master.dbo.sysmessages WHERE error = @numErrorCode");
            sb.Append("\n\t\t INSERT INTO error_log (LogDate,Source,ErrMsg) VALUES (getDate(),'" + strProcName + "',@strErrorMsg)");

            sb.Append("\n procEnd:");

            //MessageBox.Show(sb.ToString());
            return sb;
        }
    }
}
