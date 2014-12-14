using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CodeGenerator.MVP.Util
{
    public class DbBuilderOracle : IDbBuilder
    {
        string _NUMID = "";
        string _STRUID = "";
        string _STRLASTUID = "";
        string _DTUDT = "";
        string _DTLASTUDT = "";

        public DbBuilderOracle()
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
            sb.AppendLine();
            sb.Append("CREATE SEQUENCE Seq" + strTableName);
            sb.Append("\n\t START WITH 1");
            sb.Append("\n\t MAXVALUE 999999999999999999999999999");
            sb.Append("\n\t MINVALUE 1");
            sb.Append("\n\t NOCYCLE");
            sb.Append("\n\t NOCACHE");
            sb.Append("\n\t NOORDER;");

            return sb;
        }

        public StringBuilder BuildSaveProcedure(string strTableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("PROCEDURE usp" + strTableName.Replace("T_", "").Replace("t_", "") + "Save(");
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append("\t\t p" + row["COLUMN_NAME"] + "\t" + row["DATA_TYPE"] + ",");
            }
            //sb.Remove(sb.Length -1, 1); //To Remove last Comma
            sb.Append("\n\t\t p_" + _STRUID + "   VARCHAR2, \n\t\t p_STRMODE  VARCHAR2, \n\t\t po_errorcode OUT NUMBER, \n\t\t po_errormessage OUT VARCHAR2)");


            sb.Append("\n\t IS");
            sb.Append("\n\t BEGIN");
            sb.Append("\n\t\t po_errorcode:=0;");
            sb.Append("\n\t\t po_errormessage:='SUCCESSFUL';");

            //Insert
            sb.Append("\n\t\t IF(UPPER(p_STRMODE)='I') THEN");
            sb.Append("\n\t\t\t INSERT INTO " + strTableName + "(" + _NUMID + ",");
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_NUMID) ||
                    row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append("\t\t\t\t " + row["COLUMN_NAME"] + ",");
            }
            sb.Append("\n\t\t\t\t STRUID, \n\t\t\t\t STRLASTUID, \n\t\t\t\t DTUDT, \n\t\t\t\t DTLASTUDT)");

            sb.Append("\n\t\t\t VALUES(" + "Seq" + strTableName + ".NextVal,");
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_NUMID) ||
                    row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append("\t\t\t\t p" + row["COLUMN_NAME"] + ",");
            }
            sb.Append("\n\t\t\t\t p_" + _STRUID + ", \n\t\t\t\t p_" + _STRUID + ", \n\t\t\t\t sysdate, \n\t\t\t\t sysdate);");
            sb.Append("\n\t\t\t SELECT " + "Seq" + strTableName + ".CURRVAL INTO po_errorcode FROM dual;");

            //Edit
            sb.Append("\n\t\t ELSIF(UPPER(p_STRMODE)='U') THEN");
            sb.Append("\n\t\t\t UPDATE " + strTableName + " SET");
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_NUMID) ||
                    row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append("\t\t\t\t " + row["COLUMN_NAME"] + " \t = p" + row["COLUMN_NAME"] + ",");
            }
            sb.Append("\n\t\t\t\t " + _STRLASTUID + " = p_" + _STRUID + ", \n\t\t\t\t " + _DTLASTUDT + " = sysdate");
            sb.Append("\n\t\t\t\t WHERE " + _NUMID + " = p" + _NUMID + ";");
            sb.Append("\n\t\t\t SELECT p" + _NUMID + " INTO po_errorcode FROM dual;");
            sb.Append("\n\t\t\t --UPDATE TBLAUDITMASTER SET STRUSER=p_STRUID WHERE NUMSESSIONID=userenv('sessionid');");
            //Delete
            sb.Append("\n\t\t ELSIF(UPPER(p_STRMODE)='D') THEN");
            sb.Append("\n\t\t\t DELETE FROM " + strTableName + " WHERE " + _NUMID + " = p" + _NUMID + ";");
            sb.Append("\n\t\t\t UPDATE TBLAUDITMASTER SET STRUSER=p_STRUID WHERE NUMSESSIONID=userenv('sessionid');");

            sb.Append("\n\t\t ELSE");
            sb.Append("\n\t\t\t po_errorcode:=1;");
            sb.Append("\n\t\t\t po_errormessage:='Invalid Mode, No operations done';");
            sb.Append("\n\t\t END IF;");

            sb.Append("\n\n\t\t EXCEPTION WHEN OTHERS THEN");
            sb.Append("\n\t\t\t po_errorcode := SQLCODE;");
            sb.Append("\n\t\t\t po_errormessage := SQLERRM;");

            sb.Append("\n\t END;");

            //MessageBox.Show(sb.ToString());

            return sb;
        }

        public StringBuilder BuildGetProcedure(string strTableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("PROCEDURE usp" + strTableName.Replace("T_", "").Replace("t_", "") + "Get(");
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();

                if (row["DATA_TYPE"].ToString().ToUpper().Equals("DATE"))
                {
                    sb.Append("\t\t p" + row["COLUMN_NAME"] + "From\t" + row["DATA_TYPE"] + ",");
                    sb.AppendLine();
                    sb.Append("\t\t p" + row["COLUMN_NAME"] + "To\t" + row["DATA_TYPE"] + ",");
                    continue;
                }

                sb.Append("\t\t p" + row["COLUMN_NAME"] + "\t" + row["DATA_TYPE"] + ",");
            }
            //sb.Remove(sb.Length -1, 1); //To Remove last Comma
            sb.Append("\n\t\t p_NUMMODE   NUMBER,");
            sb.Append("\n\t\t p_SORTEXPRESSION  VARCHAR2,");
            sb.Append("\n\t\t p_SORTDIRECTION VARCHAR2,");
            sb.Append("\n\t\t p_STARTROW   NUMBER, --  START INDEX");
            sb.Append("\n\t\t p_MAXROWS   NUMBER, -- OPTIONAL ; 0 TO GET ALL SELECTED");
            sb.Append("\n\t\t po_errorcode OUT    NUMBER,");
            sb.Append("\n\t\t po_errormessage OUT VARCHAR2,");
            sb.Append("\n\t\t po_cursor OUT Types.Ref_Cursor)");

            sb.Append("\n\t IS");
            sb.Append("\n\t\t strSql VARCHAR2(5000) DEFAULT '';");

            sb.Append("\n\t BEGIN");
            sb.Append("\n\t\t po_errorcode:=0;");
            sb.Append("\n\t\t po_errormessage:='SUCCESSFUL';");
            sb.Append("\n");

            //RowCount
            sb.Append("\n\t\t IF (p_NUMMODE=0) THEN    -- FOR ROW COUNT ");
            sb.Append("\n\t\t\t strSql := 'SELECT COUNT(1) NUMROWS FROM " + strTableName + " T ");
            sb.Append("\n\t\t\t WHERE 0 = 0 ';");

            //SelectAll
            sb.Append("\n\t\t ELSIF (p_NUMMODE=1) THEN -- GRID VIEW ");
            sb.Append("\n\t\t\t strSql := 'SELECT * FROM (SELECT T.*,rowNum as ROWINDEX FROM (");
            sb.Append("\n\t\t\t SELECT T.* FROM " + strTableName + " T ");
            sb.Append("\n\t\t\t WHERE 0 = 0 ';");

            sb.Append("\n\t\t END IF;");

            foreach (DataRow row in dt.Rows)
            {
                if (//row["COLUMN_NAME"].ToString().Contains(_NUMID) ||
                    row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();

                //Rules#:INT/NUMBER/DECIMAL/DOUBLE/FLOAT kinds of DataType must be prefixed by NUM
                if (row["COLUMN_NAME"].ToString().StartsWith("NUM"))
                {
                    sb.Append("\n\t\t IF p" + row["COLUMN_NAME"] + ">0 THEN ");
                    sb.Append("\n\t\t\t strSql := strSql || ' AND T." + row["COLUMN_NAME"] + " = '|| p" + row["COLUMN_NAME"].ToString() + ";");
                    sb.Append("\n\t\t END IF;");
                }

                //Rules#:CHAR/VARCHAR/VARCHAR2/STRING kinds of DataType must be prefixed by STR
                if (row["COLUMN_NAME"].ToString().StartsWith("STR"))
                {
                    sb.Append("\n\t\t IF LENGTH(p" + row["COLUMN_NAME"] + ")>0 THEN ");
                    sb.Append("\n\t\t\t strSql := strSql || ' AND UPPER(T." + row["COLUMN_NAME"] + ") LIKE ''%' || REPLACE(UPPER(p" + row["COLUMN_NAME"] + "),'''','''''') || '%''';");
                    sb.Append("\n\t\t\t -- strSql := strSql || ' AND UPPER(T." + row["COLUMN_NAME"] + ")=''' || REPLACE(UPPER(p" + row["COLUMN_NAME"] + "),'''','''''') || '''';");
                    sb.Append("\n\t\t END IF;");
                }

                //Rules#:DATE/DATETIME kinds of DataType must be prefixed by DT
                if (row["DATA_TYPE"].ToString().ToUpper().Equals("DATE")) //|| (row["COLUMN_NAME"].ToString().StartsWith("DT"))
                {
                    sb.Append("\n\t\t IF p" + row["COLUMN_NAME"] + "From IS NOT NULL  AND p" + row["COLUMN_NAME"] + "To IS NOT NULL  THEN");
                    sb.Append("\n\t\t\t strSql := strSql || ' AND T." + row["COLUMN_NAME"] + " BETWEEN '''|| p" + row["COLUMN_NAME"] + "From ||''' AND '''||p" + row["COLUMN_NAME"] + "To|| '''';");
                    sb.Append("\n\t\t ELSIF p" + row["COLUMN_NAME"] + "From IS NOT NULL  AND p" + row["COLUMN_NAME"] + "To IS NULL  THEN");
                    sb.Append("\n\t\t\t strSql := strSql || ' AND T." + row["COLUMN_NAME"] + " >= '''|| p" + row["COLUMN_NAME"] + "From|| '''' ;");
                    sb.Append("\n\t\t ELSIF p" + row["COLUMN_NAME"] + "From IS NULL AND p" + row["COLUMN_NAME"] + "To IS NOT NULL  THEN");
                    sb.Append("\n\t\t\t strSql := strSql || ' AND T." + row["COLUMN_NAME"] + " <= ''' ||p" + row["COLUMN_NAME"] + "To|| '''';");
                    sb.Append("\n\t\t END IF;");
                }
            }

            sb.Append("\n\n\t\t -- Paging and Sorting Parameters");
            sb.Append("\n\t\t IF (p_NUMMODE=1) THEN");
            sb.Append("\n\t\t\t strSql :=strSql || ' ORDER BY ' || UPPER(p_SORTEXPRESSION) ||' ' ||  UPPER(p_SORTDIRECTION);");
            sb.Append("\n\t\t\t IF(p_MAXROWS >0)THEN");
            sb.Append("\n\t\t\t\t strSql :=strSql || ' )T)T WHERE ROWINDEX>= '||p_STARTROW||' AND ROWINDEX <'|| (p_STARTROW + p_MAXROWS);");
            sb.Append("\n\t\t\t ELSE");
            sb.Append("\n\t\t\t\t strSql :=strSql || ' )T)T';");
            sb.Append("\n\t\t\t END IF;");
            sb.Append("\n\t\t END IF;");

            sb.Append("\n\n\t\t IF (po_errorcode = 0) THEN");
            sb.Append("\n\t\t\t OPEN po_cursor FOR strSql ;");
            sb.Append("\n\t\t END IF;");

            sb.Append("\n\n\t\t EXCEPTION");
            sb.Append("\n\t\t\t\t WHEN OTHERS THEN");
            sb.Append("\n\t\t\t\t po_errorcode := SQLCODE;");
            sb.Append("\n\t\t\t\t po_errormessage := SQLERRM;");

            sb.Append("\n\t END;");

            //MessageBox.Show(sb.ToString());

            return sb;
        }

        public StringBuilder BuildGetProcedureSingle(string strTableName, DataTable dt)
        {
            return new StringBuilder();
        }

        public StringBuilder BuildGetProcedureParameterized(string strTableName, DataTable dt)
        {
            return new StringBuilder();
        }
    }
}
