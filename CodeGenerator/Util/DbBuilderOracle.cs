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
            sb.Append($"CREATE SEQUENCE Seq{strTableName} START WITH 1 MAXVALUE 999999999999999999999999999 MINVALUE 1 NOCYCLE NOCACHE NOORDER;");

            return sb;
        }

        public StringBuilder BuildSaveProcedure(string strTableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"PROCEDURE usp{strTableName.Replace("T_", "").Replace("t_", "")}Save(");
            foreach (DataRow row in dt.Rows)
            {
				var type = row["DATA_TYPE"].ToString();
				var col = row["COLUMN_NAME"].ToString();

				if (col.Contains(_STRUID) || col.Contains(_STRLASTUID) || col.Contains(_DTUDT) || col.Contains(_DTLASTUDT))
                    continue;

                sb.Append($"p{col}\t{type},".n1t1());
            }
            //sb.Remove(sb.Length -1, 1); //To Remove last Comma
            sb.Append($"p_{_STRUID}  VARCHAR2, {"p_STRMODE".n1t2()}  VARCHAR2, {"po_errorcode".n1t2()}  OUT NUMBER, {"po_errormessage".n1t2()}  OUT VARCHAR2)".n1t2());
			
            sb.Append("IS".n1t1());
            sb.Append("BEGIN".n1t1());
            sb.Append("po_errorcode:=0;".n1t2());
            sb.Append("po_errormessage:='SUCCESSFUL';".n1t2());

            //Insert
            sb.Append("IF(UPPER(p_STRMODE)='I') THEN".n1t2());
            sb.Append($"INSERT INTO {strTableName}({_NUMID},".n1t3());
            foreach (DataRow row in dt.Rows)
            {
				var col = row["COLUMN_NAME"].ToString();
				if (col.Contains(_NUMID) || col.Contains(_STRUID) || col.Contains(_STRLASTUID) || col.Contains(_DTUDT) || col.Contains(_DTLASTUDT))
                    continue;

                sb.Append($"{col},".n1t4());
            }
            sb.Append($"{_STRUID.n1t4()}, {_STRLASTUID.n1t4()}, {_DTUDT.n1t4()}, {_DTLASTUDT.n1t4()})");


			sb.Append($"VALUES(Seq{strTableName }.NextVal,".n1t3());
            foreach (DataRow row in dt.Rows)
            {
				var col = row["COLUMN_NAME"].ToString();
				if (col.Contains(_NUMID) || col.Contains(_STRUID) || col.Contains(_STRLASTUID) || col.Contains(_DTUDT) || col.Contains(_DTLASTUDT))
                    continue;

                sb.Append($"p{col},".n1t4());
            }
            sb.Append($"p_{_STRUID.n1t4()},p_{_STRUID.n1t4()},{"sysdate".n1t4()},{"sysdate".n1t4()});");
            sb.Append($"SELECT Seq{strTableName}.CURRVAL INTO po_errorcode FROM dual;".n1t3());

            //Edit
            sb.Append("ELSIF(UPPER(p_STRMODE)='U') THEN".n1t2());
            sb.Append($"UPDATE {strTableName} SET".n1t3());
            foreach (DataRow row in dt.Rows)
            {
				var col = row["COLUMN_NAME"].ToString();
				if (col.Contains(_NUMID) || col.Contains(_STRUID) || col.Contains(_STRLASTUID) || col.Contains(_DTUDT) || col.Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append($"{col}\t= p{col},".n0t4());
            }
            sb.Append($"{_STRLASTUID} = p_{_STRUID},".n1t4());
			sb.Append($"{_DTLASTUDT} = sysdate".n1t4());
			sb.Append($"WHERE {_NUMID} = p{_NUMID};".n1t4());
            sb.Append($"SELECT p{_NUMID} INTO po_errorcode FROM dual;".n1t3());
            sb.Append("--UPDATE TBLAUDITMASTER SET STRUSER=p_STRUID WHERE NUMSESSIONID=userenv('sessionid');".n1t3());
            //Delete
            sb.Append("ELSIF(UPPER(p_STRMODE)='D') THEN".n1t2());
            sb.Append($"DELETE FROM {strTableName} WHERE {_NUMID} = p{_NUMID};".n1t3());
            sb.Append("UPDATE TBLAUDITMASTER SET STRUSER=p_STRUID WHERE NUMSESSIONID=userenv('sessionid');".n1t3());

            sb.Append("ELSE".n1t2());
            sb.Append("po_errorcode:=1;".n1t3());
            sb.Append("po_errormessage:='Invalid Mode, No operations done';".n1t3());
            sb.Append("END IF;".n1t2());

			sb.AppendLine();
            sb.Append("EXCEPTION WHEN OTHERS THEN".n1t2());
            sb.Append("po_errorcode := SQLCODE;".n1t3());
            sb.Append("po_errormessage := SQLERRM;".n1t3());

            sb.Append("END;".n1t1());

            return sb;
        }

        public StringBuilder BuildGetProcedure(string strTableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"PROCEDURE usp{strTableName.Replace("T_", "").Replace("t_", "")}Get(");
            foreach (DataRow row in dt.Rows)
            {
				var type = row["DATA_TYPE"].ToString();
				var col = row["COLUMN_NAME"].ToString();

				if (col.Contains(_STRUID) || col.Contains(_STRLASTUID) ||
                    col.Contains(_DTUDT) || col.Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();

                if (type.ToUpper().Equals("DATE"))
                {
                    sb.Append($"p{col}From\t{type},".n0t2());
                    sb.Append($"p{col}To\t{type},".n1t2());
                    continue;
                }

                sb.Append($"p{col}\t{type},".n0t2());
            }
            //sb.Remove(sb.Length -1, 1); //To Remove last Comma
            sb.Append("p_NUMMODE   NUMBER,".n1t2());
            sb.Append("p_SORTEXPRESSION  VARCHAR2,".n1t2());
            sb.Append("p_SORTDIRECTION VARCHAR2,".n1t2());
            sb.Append("p_STARTROW   NUMBER, --  START INDEX".n1t2());
            sb.Append("p_MAXROWS   NUMBER, -- OPTIONAL ; 0 TO GET ALL SELECTED".n1t2());
            sb.Append("po_errorcode OUT    NUMBER,".n1t2());
            sb.Append("po_errormessage OUT VARCHAR2,".n1t2());
            sb.Append("po_cursor OUT Types.Ref_Cursor)".n1t2());

            sb.Append("IS".n1t1());
            sb.Append("strSql VARCHAR2(5000) DEFAULT '';".n1t2());

            sb.Append("BEGIN".n1t1());
            sb.Append("po_errorcode:=0;".n1t2());
            sb.Append("po_errormessage:='SUCCESSFUL';".n1t2());
            sb.AppendLine();

            //RowCount
            sb.Append("IF (p_NUMMODE=0) THEN    -- FOR ROW COUNT ".n1t2());
            sb.Append("strSql := 'SELECT COUNT(1) NUMROWS FROM " + strTableName + " T ".n1t3());
            sb.Append("WHERE 0 = 0 ';".n1t3());

            //SelectAll
            sb.Append("ELSIF (p_NUMMODE=1) THEN -- GRID VIEW ".n1t2());
            sb.Append("strSql := 'SELECT * FROM (SELECT T.*,rowNum as ROWINDEX FROM (".n1t3());
            sb.Append("SELECT T.* FROM " + strTableName + " T ".n1t3());
            sb.Append("WHERE 0 = 0 ';".n1t3());

            sb.Append("END IF;".n1t2());

            foreach (DataRow row in dt.Rows)
            {
				var type = row["DATA_TYPE"].ToString();
				var col = row["COLUMN_NAME"].ToString();

				if (col.Contains(_STRUID) || col.Contains(_STRLASTUID) || col.Contains(_DTUDT) || col.Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();

				//Rules#:INT/NUMBER/NUMERIC/DECIMAL/DOUBLE/FLOAT kinds of DataType must be prefixed by NUM
				//if (type.ToUpper().Equals("NUMBER") || type.ToUpper().Equals("NUMERIC") || type.ToUpper().Equals("INT") || type.ToUpper().Equals("INTEGER") || type.ToUpper().Equals("DECIMAL") || type.ToUpper().Equals("DEC") || type.ToUpper().Equals("DOUBLE") || type.ToUpper().Equals("FLOAT") )  //if (col.StartsWith("NUM"))
				string[] numericTypes = new string[] { "NUMBER", "NUMERIC", "INT", "INTEGER", "SMALLINT", "DECIMAL", "DEC", "DECIMAL", "DOUBLE", "FLOAT", "REAL", "DOUBLE PRECISION" };
				if (type.ToUpper().In(numericTypes))
				{
                    sb.Append($"IF p{col}>0 THEN ".n1t2());
                    sb.Append($"strSql := strSql || ' AND T.{col} = '|| p" + col + ";".n1t3());
                    sb.Append("END IF;".n1t2());
                }

				//Rules#:CHAR/VARCHAR/VARCHAR2/STRING/CLOB kinds of DataType must be prefixed by STR
				//if (type.ToUpper().Equals("CHAR") || type.ToUpper().Equals("VARCHAR") || type.ToUpper().Equals("VARCHAR2") || type.ToUpper().Equals("STRING") || type.ToUpper().Equals("CLOB")) //if (col.StartsWith("STR"))
				string[] charTypes = new string[] { "CHAR", "NCHAR", "VARCHAR", "VARCHAR2", "NVARCHAR2", "STRING", "LONG", "RAW", "LONG RAW"};
				if (type.ToUpper().In(charTypes))
				{
                    sb.Append($"IF LENGTH(p{col})>0 THEN ".n1t2());

					sb.Append($"strSql := strSql || ' AND UPPER(T.{col}) LIKE ''%' || REPLACE(UPPER(p{col}),'''','''''') || '%''';".n1t3());
					sb.Append($"-- strSql := strSql || ' AND UPPER(T.{col})=''' || REPLACE(UPPER(p{col}),'''','''''') || '''';".n1t3());

					sb.Append("END IF;".n1t2());
                }

                //Rules#:DATE/DATETIME kinds of DataType must be prefixed by DT
                if (type.ToUpper().Equals("DATE") || type.ToUpper().Equals("TIMESTAMP")) //|| (col.StartsWith("DT"))
                {
                    sb.Append($"IF p{col}From IS NOT NULL  AND p{col}To IS NOT NULL  THEN".n1t2());
                    sb.Append($"strSql := strSql || ' AND T.{col} BETWEEN '''|| p{col}From ||''' AND '''||p{col}To|| '''';".n1t3());
                    sb.Append($"ELSIF p{col}From IS NOT NULL  AND p{col}To IS NULL  THEN".n1t2());
                    sb.Append($"strSql := strSql || ' AND T.{col} >= '''|| p{col}From|| '''' ;".n1t3());
                    sb.Append($"ELSIF p{col}From IS NULL AND p{col}To IS NOT NULL  THEN".n1t2());
                    sb.Append($"strSql := strSql || ' AND T.{col} <= ''' ||p{col}To|| '''';".n1t3());
                    sb.Append($"END IF;".n1t2());
                }
            }

			sb.AppendLine();
            sb.Append("-- Paging and Sorting Parameters".n1t2());
            sb.Append("IF (p_NUMMODE=1) THEN".n1t2());
            sb.Append("strSql :=strSql || ' ORDER BY ' || UPPER(p_SORTEXPRESSION) ||' ' ||  UPPER(p_SORTDIRECTION);".n1t3());
            sb.Append("IF(p_MAXROWS >0)THEN".n1t3());
            sb.Append("strSql :=strSql || ' )T)T WHERE ROWINDEX>= '||p_STARTROW||' AND ROWINDEX <'|| (p_STARTROW + p_MAXROWS);".n1t4());
            sb.Append("ELSE".n1t3());
            sb.Append("strSql :=strSql || ' )T)T';".n1t4());
            sb.Append("END IF;".n1t3());
            sb.Append("END IF;".n1t2());

			sb.AppendLine();
			sb.Append("IF (po_errorcode = 0) THEN".n1t2());
            sb.Append("OPEN po_cursor FOR strSql ;".n1t3());
            sb.Append("END IF;".n1t2());

			sb.AppendLine();
			sb.Append("EXCEPTION WHEN OTHERS THEN".n1t2());
            sb.Append("po_errorcode := SQLCODE;".n1t3());
            sb.Append("po_errormessage := SQLERRM;".n1t3());

            sb.Append("END;".n1t1());

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
