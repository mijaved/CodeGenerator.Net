using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGenerator.MVP.Util
{
    public class Utility
    {
        #region Read Configuration
        public static string GetProjectName()
        {
            return System.Configuration.ConfigurationManager.AppSettings["ProjectName"].ToString();
        }
        public static string GetSaveDirectoty()
        {
            return System.Configuration.ConfigurationManager.AppSettings["SaveDirectoty"].ToString();
        }

        public static string GetSelectedDB()
        {
            return System.Configuration.ConfigurationManager.AppSettings["ConString"].ToString();
        }
        public static string GetConnectionString()
        {
            string strCon = "";
            if (Util.Utility.GetSelectedDB().Equals("Oracle"))
            {
                strCon = System.Configuration.ConfigurationManager.AppSettings["ConString_Oracle"].ToString();
            }
            else if (Util.Utility.GetSelectedDB().Equals("SqlServer"))
            {
                strCon = System.Configuration.ConfigurationManager.AppSettings["ConString_SqlServer"].ToString();
            }
            return strCon;
        }

        public static string GetPkColName()
        {
            return System.Configuration.ConfigurationManager.AppSettings["PkColName"].ToString();
        }
        public static string GetRecordCreatorColName()
        {
            return System.Configuration.ConfigurationManager.AppSettings["RecordCreatorColName"].ToString();
        }
        public static string GetRecordModifierColName()
        {
            return System.Configuration.ConfigurationManager.AppSettings["RecordModifierColName"].ToString();
        }
        public static string GetRecordCreateDateColName()
        {
            return System.Configuration.ConfigurationManager.AppSettings["RecordCreateDateColName"].ToString();
        }
        public static string GetRecordModifiedDateColName()
        {
            return System.Configuration.ConfigurationManager.AppSettings["RecordModifiedDateColName"].ToString();
        }
        #endregion

        public static string GetDotNetDataType(string strDBDataType, string strPrecision, string strScale)
        {
            string strDataType = "";

            if (strDBDataType.Contains("INT") || strDBDataType.Contains("INTEGER") || strDBDataType.Contains("TINYINT"))
                strDataType = "int";
            else if (strDBDataType.Contains("DATE") || strDBDataType.Contains("DATETIME") || strDBDataType.Contains("TIMESTAMP"))
                strDataType = "DateTime";
            else if (strDBDataType.Contains("CHAR") || strDBDataType.Contains("TEXT")
                || strDBDataType.Contains("VARCHAR") || strDBDataType.Contains("VARCHAR2")
                || strDBDataType.Contains("UNIQUEIDENTIFIER"))
                strDataType = "string";
            else if (strDBDataType.Contains("NUMERIC") || strDBDataType.Contains("NUMBER")
                || strDBDataType.Contains("DECIMAL") || strDBDataType.Contains("DOUBLE")
                || strDBDataType.Contains("LONG") || strDBDataType.Contains("REAL")
                || strDBDataType.Contains("MONEY"))
            {
                int intPrecision = ParseInt(strPrecision);
                int intScale = ParseInt(strScale);

                if (intPrecision > 10 || intScale > 0)
                    strDataType = "decimal";
                else
                    strDataType = "int";
            }
            else if (strDBDataType.Contains("BIT"))
                strDataType = "bool";
            else if (strDBDataType.Contains("BLOB") || strDBDataType.Contains("CLOB")
                || strDBDataType.Contains("BFILE") || strDBDataType.Contains("BINARY")
                 || strDBDataType.Contains("IMAGE"))
                strDataType = "byte []";

            return strDataType;
        }

        public static string GetDotNetDataTypeDefaultValue(string strDotNetDataType)
        {
            if (strDotNetDataType.Equals("int") || strDotNetDataType.Equals("decimal"))
                return "-1";
            else if (strDotNetDataType.Equals("string"))
                return "\"\"";
            else if (strDotNetDataType.Equals("string"))
                return "DateTime.MinValue";
            else if (strDotNetDataType.Equals("DateTime"))
                return "DateTime.MinValue";
            else if (strDotNetDataType.Equals("bool"))
                return "true";
            else if (strDotNetDataType.Equals("byte []"))
                return "null";
            else
                return "null";
        }

        public static string GetCommonSearchParams(bool IsWithDataTypes)
        {
            if (IsWithDataTypes)
            {
                if (GetSelectedDB().Equals("Oracle"))
                {
                    return "int NUMMODE, string SORTEXPRESSION, string SORTDIRECTION, int STARTROW, int MAXROWS";
                }
                else if (Util.Utility.GetSelectedDB().Equals("SqlServer"))
                {
                    return "string strMode, string strSortBy, string strSortType, int startRowIndex, int maximumRows";
                }
            }
            else
            {
                if (GetSelectedDB().Equals("Oracle"))
                {
                    return "NUMMODE, SORTEXPRESSION, SORTDIRECTION, STARTROW, MAXROWS";
                }
                else if (Util.Utility.GetSelectedDB().Equals("SqlServer"))
                {
                    return "strMode, strSortBy, strSortType, startRowIndex, maximumRows";
                }
            }
            return "";
        }

        public static int ParseInt(object value)
        {
            int intValue = 0;
            if (value == null) return 0;

            string strValue = value.ToString();
            if (!string.IsNullOrEmpty(strValue))
            {
                try
                {
                    intValue = int.Parse(strValue);
                }
                catch
                {
                    intValue = 0;
                }
            }

            return intValue;
        }

        /// <summary>
        /// Writes the content in disk file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        public static void WriteToDisk(string filePath, string content)
        {
            System.IO.StreamWriter fileWriter = null;
            try
            {
                string dirPath = System.IO.Path.GetDirectoryName(filePath);
                if (!System.IO.Directory.Exists(dirPath))
                {
                    System.IO.Directory.CreateDirectory(dirPath);
                }

                if (System.IO.File.Exists(filePath))
                    fileWriter = System.IO.File.AppendText(filePath);
                else
                    fileWriter = System.IO.File.CreateText(filePath);

                fileWriter.Write(content);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            finally
            {
                if (fileWriter != null)
                {
                    fileWriter.Close();
                    fileWriter.Dispose();
                }
            }
        }
    }
}
