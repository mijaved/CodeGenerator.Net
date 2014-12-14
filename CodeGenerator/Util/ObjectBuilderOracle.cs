using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CodeGenerator.MVP.Util;

namespace CodeGenerator.MVP.Util
{
    public class ObjectBuilderOracle : ObjectBuilderBase
    {
        public ObjectBuilderOracle()
        {
        }

        public override StringBuilder BuildDAL(string strProjectName, string strObjectName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" using System;");
            sb.Append("\n using System.Data;");
            sb.Append("\n using System.Collections.Generic;");
            sb.Append("\n using System.Linq;");
            sb.Append("\n using System.Text;");
            sb.Append("\n using System.Data.OracleClient;");
            sb.Append("\n using " + strProjectName + ".DAL.DTO;");
            sb.AppendLine();

            sb.Append("\n namespace " + strProjectName + ".DAL;");
            sb.Append("\n {");

            sb.Append("\n\t public class " + strObjectName + "DAL");
            sb.Append("\n\t {");
            sb.Append("\n\t\t private static string SchemaName = \"[PackageName].\";");

            #region SaveObj
            sb.AppendLine();
            sb.Append("\n\t\t public static int SaveObj(" + strObjectName + " obj, string strMode , DBTransaction transaction)");
            sb.Append("\n\t\t {");
            sb.Append("\n\t\t\t OracleProcedure procedure = new OracleProcedure(SchemaName, \"usp" + strObjectName + "Save\");");
            sb.AppendLine();
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append("\t\t\t procedure.AddInputParameter(\"p" + row["COLUMN_NAME"] + "\", obj." + row["COLUMN_NAME"] + ", OracleType." + row["DATA_TYPE"].ToString().Replace("VARCHAR2", "VarChar").Replace("NUMBER", "Number").Replace("DATE", "DateTime") + ");");
            }
            sb.Append("\n\t\t\t procedure.AddInputParameter(\"p_" + _STRUID + "\", obj." + _STRUID + ", OracleType.VarChar);");
            sb.Append("\n\t\t\t procedure.AddInputParameter(\"p_STRMODE\", strMode, OracleType.VarChar);");

            sb.AppendLine();
            sb.Append("\n\t\t\t try");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t procedure.ExecuteNonQuery(transaction);");
            sb.Append("\n\t\t\t\t if (procedure.ReturnMessage == \"SUCCESSFUL\")");
            sb.Append("\n\t\t\t\t {");
            sb.Append("\n\t\t\t\t\t return procedure.ErrorCode;");
            sb.Append("\n\t\t\t\t }");
            sb.Append("\n\t\t\t\t return procedure.ErrorCode + Utility.ErrorCode;");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t\t catch (Exception ex)");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t throw (ex);");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t }");
            #endregion //End of SaveObj

            #region GetObjCount
            sb.AppendLine();
            sb.Append("\n\t\t public static int GetObjCount(");
            //Paremeters
            string strPatrams = "";
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                string strDataType = Utility.GetDotNetDataType(row["DATA_TYPE"].ToString().ToUpper(), row["PRECISION"].ToString(), row["SCALE"].ToString());
                strPatrams += " " + strDataType + " " + row["COLUMN_NAME"] + ",";
            }
            strPatrams = strPatrams.TrimEnd(',');
            sb.Append(strPatrams);
            sb.Append(")");

            sb.Append("\n\t\t {");
            sb.Append("\n\t\t\t OracleProcedure procedure = new OracleProcedure(SchemaName, \"usp" + strObjectName + "Get\");");
            sb.AppendLine();
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append("\t\t\t procedure.AddInputParameter(\"p" + row["COLUMN_NAME"] + "\", " + row["COLUMN_NAME"] + ", OracleType." + row["DATA_TYPE"].ToString().Replace("VARCHAR2", "VarChar").Replace("NUMBER", "Number").Replace("DATE", "DateTime") + ");");
            }
            sb.Append("\n\t\t\t procedure.AddInputParameter(\"p_NUMMODE\", 0, OracleType.Number);");
            sb.Append("\n\t\t\t procedure.AddInputParameter(\"p_SORTEXPRESSION\", \"\", OracleType.VarChar);");
            sb.Append("\n\t\t\t procedure.AddInputParameter(\"p_SORTDIRECTION\", \"\", OracleType.VarChar);");
            sb.Append("\n\t\t\t procedure.AddInputParameter(\"p_STARTROW\", 1, OracleType.Number);");
            sb.Append("\n\t\t\t procedure.AddInputParameter(\"p_MAXROWS\", 1, OracleType.Number);");

            sb.AppendLine();
            sb.Append("\n\t\t\t try");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t DataTable dt = procedure.ExecuteQueryToDataTable();");
            sb.Append("\n\t\t\t\t return Convert.ToInt32(dt.Rows[0][0]);");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t\t catch (Exception ex)");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t throw (ex);");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t }");
            #endregion End of GetObjCount

            #region GetObjList
            sb.AppendLine();
            sb.Append("\n\t\t public static List<" + strObjectName + "> GetObjList(");
            sb.Append(strPatrams);
            sb.Append(", \n\t\t\t\t\t\t int NUMMODE, string SORTEXPRESSION, string SORTDIRECTION, int STARTROW, int MAXROWS");
            sb.Append(")");
            sb.Append("\n\t\t {");
            sb.Append("\n\t\t\t OracleProcedure procedure = new OracleProcedure(SchemaName, \"usp" + strObjectName + "Get\");");
            sb.AppendLine();
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append("\t\t\t procedure.AddInputParameter(\"p" + row["COLUMN_NAME"] + "\", " + row["COLUMN_NAME"] + ", OracleType." + row["DATA_TYPE"].ToString().Replace("VARCHAR2", "VarChar").Replace("NUMBER", "Number").Replace("DATE", "DateTime") + ");");
            }
            sb.Append("\n\t\t\t procedure.AddInputParameter(\"p_NUMMODE\", NUMMODE, OracleType.Number);");
            sb.Append("\n\t\t\t procedure.AddInputParameter(\"p_SORTEXPRESSION\", SORTEXPRESSION, OracleType.VarChar);");
            sb.Append("\n\t\t\t procedure.AddInputParameter(\"p_SORTDIRECTION\", SORTDIRECTION, OracleType.VarChar);");
            sb.Append("\n\t\t\t procedure.AddInputParameter(\"p_STARTROW\", STARTROW, OracleType.Number);");
            sb.Append("\n\t\t\t procedure.AddInputParameter(\"p_MAXROWS\", MAXROWS, OracleType.Number);");
            
            sb.AppendLine();
            sb.Append("\n\t\t\t try");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t DataTable dt = procedure.ExecuteQueryToDataTable();");
            sb.Append("\n\t\t\t\t List<" + strObjectName + "> results = new List<" + strObjectName + ">();");
            sb.AppendLine();
            sb.Append("\n\t\t\t\t foreach (DataRow dr in dt.Rows)");
            sb.Append("\n\t\t\t\t {");
            sb.Append("\n\t\t\t\t\t " + strObjectName + " obj = new " + strObjectName + "();");
            sb.Append("\n\t\t\t\t\t MapperBase.GetInstance().MapItem(obj, dr);");
            sb.Append("\n\t\t\t\t\t results.Add(obj);");
            sb.Append("\n\t\t\t\t }");
            sb.Append("\n\t\t\t\t return results;");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t\t catch (Exception ex)");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t throw (ex);");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t }");
            #endregion End of GetList

            sb.Append("\n\t }");
            sb.Append("\n }");

            return sb;
        }
    }
}
