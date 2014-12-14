using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CodeGenerator.MVP.Util;

namespace CodeGenerator.MVP.Util
{
    public class ObjectBuilderSqlServer : ObjectBuilderBase
    {
        public ObjectBuilderSqlServer()
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
            sb.Append("\n using System.Data.SqlClient;");
            sb.Append("\n using " + strProjectName + ".DAL.DTO;");
            sb.AppendLine();

            sb.Append("\n namespace " + strProjectName + ".DAL;");
            sb.Append("\n {");

            sb.Append("\n\t public class " + strObjectName + "DAL");
            sb.Append("\n\t {");

            #region SaveObj
            sb.AppendLine();
            sb.Append("\n\t\t public static string SaveObj(" + strObjectName + " obj, string strMode , DBTransaction transaction, out int intErrorCode)");
            sb.Append("\n\t\t {");
            sb.Append("\n\t\t\t DataSet dsResult = new DataSet();");
            sb.Append("\n\t\t\t intErrorCode = 0;");
            sb.Append("\n\t\t\t string UID = \"\";");

            sb.AppendLine();
            sb.Append("\n\t\t\t try");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t SqlCommand command = new SqlCommand(\"usp" + strObjectName + "Save\", dbTransaction.Connection, dbTransaction.CurrentTransaction);");
            sb.Append("\n\t\t\t\t command.CommandType = CommandType.StoredProcedure;");
            sb.AppendLine();
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append("\t\t\t\t command.Parameters.AddWithValue(\"@" + row["COLUMN_NAME"] + "\", obj." + row["COLUMN_NAME"] + ");");
            }
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@strUser\", obj." + _STRUID + ");");
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@strMode\", obj.strMode);");
            sb.AppendLine();
            sb.Append("\n\t\t\t\t command.Parameters.Add(\"@strErrorCode\", System.Data.SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;");
            sb.Append("\n\t\t\t\t command.Parameters.Add(\"@strErrorMsg\", System.Data.SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;");
            
            sb.AppendLine(); //Nullify
            sb.Append("\n\t\t\t\t foreach (SqlParameter sp in command.Parameters)");
            sb.Append("\n\t\t\t\t {");
            sb.Append("\n\t\t\t\t\t if (sp.Direction != ParameterDirection.Output)");
            sb.Append("\n\t\t\t\t\t {");
            sb.Append("\n\t\t\t\t\t\t if (sp.Value == null)");
            sb.Append("\n\t\t\t\t\t\t {");
            sb.Append("\n\t\t\t\t\t\t\t sp.Value = DBNull.Value;");
            sb.Append("\n\t\t\t\t\t\t }");
            sb.Append("\n\t\t\t\t\t\t else");
            sb.Append("\n\t\t\t\t\t\t {");
            sb.Append("\n\t\t\t\t\t\t\t if (sp.Value.ToString() == DateTime.MinValue.ToString())");
            sb.Append("\n\t\t\t\t\t\t\t\t sp.Value = DBNull.Value;");
            sb.Append("\n\t\t\t\t\t\t }");
            sb.Append("\n\t\t\t\t\t }");
            sb.Append("\n\t\t\t\t }");

            sb.Append("\n\t\t\t\t SqlDataAdapter adapter = new SqlDataAdapter(command);");
            sb.Append("\n\t\t\t\t adapter.Fill(dsResult);");
            sb.Append("\n\t\t\t\t UID = Convert.ToString(command.Parameters[\"@strErrorCode\"].Value);");
            sb.Append("\n\t\t\t }"); //end of Try
            sb.Append("\n\t\t\t catch (SqlException ex)");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t intErrorCode = -(ex.Number);");
            sb.Append("\n\t\t\t\t throw (ex);");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t\t catch (Exception ex)");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t throw (ex);");
            sb.Append("\n\t\t\t }");
            sb.AppendLine();
            sb.Append("\n\t\t\t return UID;");
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
            sb.Append("\n\t\t\t DataSet dsResult = new DataSet();");
            sb.Append("\n\t\t\t SqlConnection connection = null;");
            sb.Append("\n\t\t\t int intCount = 0;");
            sb.AppendLine();

            sb.Append("\n\t\t\t try");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t connection = new SqlConnection(Connection.ConnectionString);");
            sb.Append("\n\t\t\t\t connection.Open();");
            sb.AppendLine();
            sb.Append("\n\t\t\t\t SqlCommand command = new SqlCommand(\"usp" + strObjectName + "Get\", connection);");
            sb.Append("\n\t\t\t\t command.CommandType = CommandType.StoredProcedure;");
            sb.AppendLine();

            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append("\t\t\t\t command.Parameters.AddWithValue(\"@" + row["COLUMN_NAME"] + "\", " + row["COLUMN_NAME"] + ");");
            }
            sb.AppendLine();
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@strMode\", 0);");
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@strSortBy\", \"\");");
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@strSortType\", \"\");");
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@startRowIndex\", 1);");
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@maximumRows\", 0);");
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@numErrorCode\", 0);");
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@strErrorMsg\", \"\");");
            
            sb.AppendLine();
            sb.Append("\n\t\t\t\t SqlDataAdapter adapter = new SqlDataAdapter(command);");
            sb.Append("\n\t\t\t\t adapter.Fill(dsResult);");
            
            sb.AppendLine();
            sb.Append("\n\t\t\t\t if (dsResult.Tables.Count > 0)");
            sb.Append("\n\t\t\t\t {");
            sb.Append("\n\t\t\t\t\t intCount = int.Parse(dsResult.Tables[0].Rows[0][0].ToString());");
            sb.Append("\n\t\t\t\t }");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t\t catch (Exception ex)");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t throw (ex);");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t\t finally");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t if (connection != null) connection.Dispose();");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t\t return intCount;");
            sb.Append("\n\t\t }");
            #endregion End of GetObjCount

            #region GetObjList
            sb.AppendLine();
            sb.Append("\n\t\t public static List<" + strObjectName + "> GetObjList(");
            sb.Append(strPatrams);
            sb.Append(", \n\t\t\t\t\t\t string strMode, string strSortBy, string strSortType, int startRowIndex, int maximumRows");
            sb.Append(")");
            sb.Append("\n\t\t {");
            sb.Append("\n\t\t\t DataSet dsResult = new DataSet();");
            sb.Append("\n\t\t\t SqlConnection connection = null;");
            sb.Append("\n\t\t\t List<" + strObjectName + "> results = null;");
            
            sb.AppendLine();
            sb.Append("\n\t\t\t try");
            sb.Append("\n\t\t\t {");

            sb.Append("\n\t\t\t\t connection = new SqlConnection(Connection.ConnectionString);");
            sb.Append("\n\t\t\t\t connection.Open();");
            sb.AppendLine();
            sb.Append("\n\t\t\t\t SqlCommand command = new SqlCommand(\"usp" + strObjectName + "Get\", connection);");
            sb.Append("\n\t\t\t\t command.CommandType = CommandType.StoredProcedure;");
            sb.AppendLine();
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append("\t\t\t\t command.Parameters.AddWithValue(\"@" + row["COLUMN_NAME"] + "\", " + row["COLUMN_NAME"] + ");");
            }
            sb.AppendLine();
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@strMode\", strMode);");
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@strSortBy\", strSortBy);");
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@strSortType\", strSortType);");
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@startRowIndex\", startRowIndex);");
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@maximumRows\", maximumRows);");
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@numErrorCode\", 0);");
            sb.Append("\n\t\t\t\t command.Parameters.AddWithValue(\"@strErrorMsg\", \"\");");
            sb.AppendLine();

            sb.Append("\n\t\t\t\t SqlDataAdapter adapter = new SqlDataAdapter(command);");
            sb.Append("\n\t\t\t\t adapter.Fill(dsResult);");
            sb.Append("\n\t\t\t\t results = new List<" + strObjectName + ">();");
            sb.AppendLine();
            sb.Append("\n\t\t\t\t foreach (DataRow dr in dsResult.Tables[0].Rows)");
            sb.Append("\n\t\t\t\t {");
            sb.Append("\n\t\t\t\t\t " + strObjectName + " obj = new " + strObjectName + "();");
            sb.Append("\n\t\t\t\t\t MapperBase.GetInstance().MapItem(obj, dr);");
            sb.Append("\n\t\t\t\t\t results.Add(obj);");
            sb.Append("\n\t\t\t\t }");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t\t catch (Exception ex)");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t throw (ex);");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t\t finally");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t if (connection != null) connection.Dispose();");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t\t return results;");
            sb.Append("\n\t\t }");
            #endregion End of GetList

            sb.Append("\n\t }");
            sb.Append("\n }");

            return sb;
        }
    }
}
