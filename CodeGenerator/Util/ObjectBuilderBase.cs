using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CodeGenerator.MVP.Util;

namespace CodeGenerator.MVP.Util
{
    public class ObjectBuilderBase : IObjectBuilder
    {
        protected string _NUMID = "";
        protected string _STRUID = "";
        protected string _STRLASTUID = "";
        protected string _DTUDT = "";
        protected string _DTLASTUDT = "";

        public ObjectBuilderBase()
        {
            _NUMID = Util.Utility.GetPkColName();
            _STRUID = Util.Utility.GetRecordCreatorColName();
            _STRLASTUID = Util.Utility.GetRecordModifierColName();
            _DTUDT = Util.Utility.GetRecordCreateDateColName();
            _DTLASTUDT = Util.Utility.GetRecordModifiedDateColName();
        }

        public virtual StringBuilder BuildObject(string strProjectName, string strObjectName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" using System;");
            sb.Append("\n using System.Collections.Generic;");
            sb.Append("\n using System.Linq;");
            sb.Append("\n using System.Text;");
            sb.Append("\n using System.Data;");
            sb.Append("\n using System.Runtime.Serialization;");
            sb.Append("\n using System.Globalization;");

            sb.Append("\n\n namespace " + strProjectName + ".DAL.DTO");
            sb.Append("\n {");
            sb.Append("\n\t [Serializable]");
            sb.Append("\n\t public class " + strObjectName + " : EntityBase");
            sb.Append("\n\t {");

            sb.Append("\n\t\t public " + strObjectName + "() { }");

            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                sb.AppendLine();
                sb.Append("\n\t\t [DataMember, DataColumn(true)]");

                string strDataType = Utility.GetDotNetDataType(row["DATA_TYPE"].ToString().ToUpper(), row["PRECISION"].ToString(), row["SCALE"].ToString());
                sb.Append("\n\t\t public " + strDataType + " " + row["COLUMN_NAME"] + "  { get; set; }");
            }

            sb.Append("\n\t }");
            sb.Append("\n }");

            return sb;
        }

        public virtual StringBuilder BuildDAL(string strProjectName, string strObjectName, DataTable dt)
        {
            return new StringBuilder();
        }

        public virtual StringBuilder BuildDATA(string strProjectName, string strObjectName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" using System;");
            sb.Append("\n using System.Data;");
            sb.Append("\n using System.Collections.Generic;");
            sb.Append("\n using System.Linq;");
            sb.Append("\n using System.Text;");
            sb.Append("\n using " + strProjectName + ".DAL;");
            sb.Append("\n using " + strProjectName + ".DAL.DTO;");
            sb.AppendLine();

            sb.Append("\n namespace " + strProjectName + ".Data");
            sb.Append("\n {");

            sb.Append("\n\t public class " + strObjectName + "Data");
            sb.Append("\n\t {");

            #region SaveObj
            if (Utility.GetSelectedDB().Equals("Oracle"))
            {
                sb.AppendLine();
                sb.Append("\n\t\t public int Save(" + strObjectName + " obj, DBTransaction transaction)");
                sb.Append("\n\t\t {");
                sb.Append("\n\t\t\t int i = -1;");
                sb.Append("\n\t\t\t i = " + strObjectName + "DAL.SaveObj(obj, \"I\", transaction);");
                sb.Append("\n\t\t\t return i;");
                sb.Append("\n\t\t }");
                sb.AppendLine();
                sb.Append("\n\t\t public int Update(" + strObjectName + " obj, DBTransaction transaction)");
                sb.Append("\n\t\t {");
                sb.Append("\n\t\t\t int i = -1;");
                sb.Append("\n\t\t\t i = " + strObjectName + "DAL.SaveObj(obj, \"U\", transaction);");
                sb.Append("\n\t\t\t return i;");
                sb.Append("\n\t\t }");
                sb.AppendLine();
                sb.Append("\n\t\t public int Delete(" + strObjectName + " obj, DBTransaction transaction)");
                sb.Append("\n\t\t {");
                sb.Append("\n\t\t\t int i = -1;");
                sb.Append("\n\t\t\t i = " + strObjectName + "DAL.SaveObj(obj, \"D\", transaction);");
                sb.Append("\n\t\t\t return i;");
                sb.Append("\n\t\t }");
            }
            else if (Util.Utility.GetSelectedDB().Equals("SqlServer"))
            {
                sb.AppendLine();
                sb.Append("\n\t\t public string Save(" + strObjectName + " obj, DBTransaction transaction, out int intErrorCode)");
                sb.Append("\n\t\t {");
                sb.Append("\n\t\t\t string uid = \"\";");
                sb.Append("\n\t\t\t uid = " + strObjectName + "DAL.SaveObj(obj, \"I\", transaction, out intErrorCode);");
                sb.Append("\n\t\t\t return uid;");
                sb.Append("\n\t\t }");
                sb.AppendLine();
                sb.Append("\n\t\t public string Update(" + strObjectName + " obj, DBTransaction transaction, out int intErrorCode)");
                sb.Append("\n\t\t {");
                sb.Append("\n\t\t\t string uid = \"\";");
                sb.Append("\n\t\t\t uid = " + strObjectName + "DAL.SaveObj(obj, \"U\", transaction, out intErrorCode);");
                sb.Append("\n\t\t\t return uid;");
                sb.Append("\n\t\t }");
                sb.AppendLine();
                sb.Append("\n\t\t public string Delete(" + strObjectName + " obj, DBTransaction transaction, out int intErrorCode)");
                sb.Append("\n\t\t {");
                sb.Append("\n\t\t\t string uid = \"\";");
                sb.Append("\n\t\t\t uid = " + strObjectName + "DAL.SaveObj(obj, \"D\", transaction, out intErrorCode);");
                sb.Append("\n\t\t\t return uid;");
                sb.Append("\n\t\t }");
            }
            #endregion //End of SaveObj

            #region GetObjCount
            sb.AppendLine();
            sb.Append("\n\t\t public int GetObjCount(");
            //Paremeters
            string strPatrams = "";
            string strPatramsWithoutDataType = "";
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                string strDataType = Utility.GetDotNetDataType(row["DATA_TYPE"].ToString().ToUpper(), row["PRECISION"].ToString(), row["SCALE"].ToString());
                strPatrams += " " + strDataType + " " + row["COLUMN_NAME"] + ",";
                strPatramsWithoutDataType += " " + row["COLUMN_NAME"] + ",";
            }
            strPatrams = strPatrams.TrimEnd(',');
            strPatramsWithoutDataType = strPatramsWithoutDataType.TrimEnd(',');
            sb.Append(strPatrams + ")");
            sb.Append("\n\t\t {");
            sb.Append("\n\t\t\t return " + strObjectName + "DAL.GetObjCount(" + strPatramsWithoutDataType + ");");
            sb.Append("\n\t\t }");
            #endregion //End of GetObjCount

            #region GetObjList
            sb.AppendLine();
            sb.Append("\n\t\t public List<" + strObjectName + "> GetObjList(");
            sb.Append(strPatrams + ", \n\t\t\t\t\t\t " + Util.Utility.GetCommonSearchParams(true)+ ")");
            
            sb.Append("\n\t\t {");
            sb.Append("\n\t\t\t return " + strObjectName + "DAL.GetObjList(");
            sb.Append(strPatramsWithoutDataType + ", \n\t\t\t\t\t\t " + Util.Utility.GetCommonSearchParams(false) + ");");

            sb.Append("\n\t\t }");
            #endregion //End of GetObjList

            sb.Append("\n\t }");
            sb.Append("\n }");

            return sb;
        }

        public virtual StringBuilder BuildPRESENTER(string strProjectName, string strObjectName, DataTable dt, out StringBuilder sbView)
        {
            StringBuilder sb = new StringBuilder();

            #region IViewInterface
            sbView = new StringBuilder();
            sbView.Append(" using System;");
            sbView.Append("\n using System.Collections.Generic;");
            sbView.Append("\n using System.Linq;");
            sbView.Append("\n using System.Text;");

            sbView.Append("\n\n namespace " + strProjectName + ".Presenter");
            sbView.Append("\n {");
            sbView.Append("\n\t public interface I" + strObjectName + "View");
            sbView.Append("\n\t {");

            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                //sbView.AppendLine();
                string strDataType = Utility.GetDotNetDataType(row["DATA_TYPE"].ToString().ToUpper(), row["PRECISION"].ToString(), row["SCALE"].ToString());
                sbView.Append("\n\t\t " + strDataType + " " + row["COLUMN_NAME"] + "  { get; set; }");
            }
            sbView.Append("\n\t\t int CanEdit  { get; set; }");
            sbView.Append("\n\t\t string ErrorMessage  { get; set; }");
            sbView.Append("\n\t }"); //end of I.View

            sbView.AppendLine();
            sbView.AppendLine();
            sbView.Append("\n\t public interface I" + strObjectName + "SearchView");
            sbView.Append("\n\t {");

            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                //sbView.AppendLine();
                string strDataType = Utility.GetDotNetDataType(row["DATA_TYPE"].ToString().ToUpper(), row["PRECISION"].ToString(), row["SCALE"].ToString());
                sbView.Append("\n\t\t " + strDataType + " " + row["COLUMN_NAME"] + "  { get; }");
            }
            sbView.Append("\n\t }"); //end of I.SearchView
            sbView.Append("\n }"); //end of Namespace
            #endregion end of IView

            #region Presenter

            sb.Append(" using System;");
            sb.Append("\n using System.Data;");
            sb.Append("\n using System.Collections.Generic;");
            sb.Append("\n using System.Linq;");
            sb.Append("\n using System.Text;");
            sb.Append("\n using " + strProjectName + ".Data;");
            sb.Append("\n using " + strProjectName + ".DAL.DTO;");
            sb.AppendLine();

            sb.Append("\n namespace " + strProjectName + ".Presenter");
            sb.Append("\n {");

            sb.Append("\n\t public class " + strObjectName + "Presenter");
            sb.Append("\n\t {");
            sb.Append("\n\t\t private I" + strObjectName + "View IView;");
            sb.Append("\n\t\t private " + strObjectName + "Data Data;");

            sb.AppendLine();
            sb.Append("\n\t\t public " + strObjectName + "Presenter(I" + strObjectName + "View view)");
            sb.Append("\n\t\t {");
            sb.Append("\n\t\t\t this.IView = view;");
            sb.Append("\n\t\t\t Data = new " + strObjectName + "Data();");
            sb.Append("\n\t\t }");

            #region PopulateObj
            sb.AppendLine();
            sb.Append("\n\t\t public void PopulateObj()");
            sb.Append("\n\t\t {");
            sb.Append("\n\t\t\t " + strObjectName + " obj = new " + strObjectName + "();");
            sb.AppendLine();
            sb.Append("\n\t\t\t if (IView." + _NUMID + " != null)");
            sb.Append("\n\t\t\t {");

            string strPatramsDefaultValues = "";
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                string strDataType = Utility.GetDotNetDataType(row["DATA_TYPE"].ToString().ToUpper(), row["PRECISION"].ToString(), row["SCALE"].ToString());
                strPatramsDefaultValues += ", " + Util.Utility.GetDotNetDataTypeDefaultValue(strDataType);
            }
            sb.Append("\n\t\t\t\t obj = Data.GetObjList(IView." + _NUMID + strPatramsDefaultValues + " 2, \"T." + _NUMID + "\", \"ASC\", 1, 1).SingleOrDefault();");

            sb.AppendLine();
            sb.Append("\n\t\t\t\t if (obj != null)");
            sb.Append("\n\t\t\t\t {");
            //Paremeters
            string strGetPatrams = "";
            string strSavePatrams = "";
            foreach (DataRow row in dt.Rows)
            {
                /*if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;
                string strDataType = Utility.GetDotNetDataType(row["DATA_TYPE"].ToString().ToUpper());*/
                strGetPatrams += "\n\t\t\t\t\t " + "IView." + row["COLUMN_NAME"] + " = " + "obj." + row["COLUMN_NAME"] + ";";
                strSavePatrams += "\n\t\t\t\t " + "obj." + row["COLUMN_NAME"] + " = " + "IView." + row["COLUMN_NAME"] + ";";
            }
            sb.Append(strGetPatrams);
            sb.Append("\n\t\t\t\t\t " + "IView.CanEdit = obj.CanEdit;");
            sb.Append("\n\t\t\t\t }");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t }");
            #endregion

            #region SaveObj
            sb.AppendLine();
            if (Util.Utility.GetSelectedDB().Equals("Oracle"))
            {
                sb.Append("\n\t\t public int SaveObj()");
                sb.Append("\n\t\t {");
                sb.Append("\n\t\t\t int i = -1;");
            }
            else if (Util.Utility.GetSelectedDB().Equals("SqlServer"))
            {
                sb.Append("\n\t\t public string SaveObj(out int intErrorCode)");
                sb.Append("\n\t\t {");
                sb.Append("\n\t\t\t intErrorCode = 0;");
                sb.Append("\n\t\t\t string uid = \"\";");
            }

            sb.Append("\n\t\t\t " + strObjectName + " obj = new " + strObjectName + "();");
            sb.Append("\n\t\t\t DBTransaction transaction = new DBTransaction();");

            sb.AppendLine();
            sb.Append("\n\t\t\t try");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t if (transaction != null) transaction.Begin();");
            sb.Append(strSavePatrams); //Paremeters

            sb.AppendLine();
            if (Util.Utility.GetSelectedDB().Equals("Oracle"))
            {
                sb.Append("\n\t\t\t\t if (IView.NUMID > 0)");
                sb.Append("\n\t\t\t\t {");
                sb.Append("\n\t\t\t\t\t i = Data.Update(obj, transaction);");
                sb.Append("\n\t\t\t\t }");
                sb.Append("\n\t\t\t\t else");
                sb.Append("\n\t\t\t\t {");
                sb.Append("\n\t\t\t\t\t i = Data.Save(obj, transaction);");
                sb.Append("\n\t\t\t\t }");
                sb.AppendLine();
                sb.Append("\n\t\t\t\t if (i > 0)");
            }
            else if (Util.Utility.GetSelectedDB().Equals("SqlServer"))
            {
                sb.Append("\n\t\t\t\t if (IView.uidPK != \"\")");
                sb.Append("\n\t\t\t\t {");
                sb.Append("\n\t\t\t\t\t uid = Data.Update(obj, dbTransaction, out intErrorCode);");
                sb.Append("\n\t\t\t\t }");
                sb.Append("\n\t\t\t\t else");
                sb.Append("\n\t\t\t\t {");
                sb.Append("\n\t\t\t\t\t uid = Data.Save(obj, dbTransaction, out intErrorCode);");
                sb.Append("\n\t\t\t\t }");
                sb.AppendLine();
                sb.Append("\n\t\t\t\t if (intErrorCode >= 0)");
            }

            //sb.Append("\n\t\t\t\t {");
            sb.Append("\n\t\t\t\t\t transaction.Commit();");
            //sb.Append("\n\t\t\t\t }");
            sb.Append("\n\t\t\t\t else");
            //sb.Append("\n\t\t\t\t {");
            sb.Append("\n\t\t\t\t\t throw new Exception();");
            //sb.Append("\n\t\t\t\t }");
            sb.Append("\n\t\t\t }"); //end of Try
            sb.Append("\n\t\t\t catch (Exception ex)");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t transaction.RollBack();");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t\t finally");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t transaction.Dispose();");
            sb.Append("\n\t\t\t }");
            sb.AppendLine();

            if (Util.Utility.GetSelectedDB().Equals("Oracle"))
            {
                sb.Append("\n\t\t\t return i;");
            }
            else if (Util.Utility.GetSelectedDB().Equals("SqlServer"))
            {
                sb.Append("\n\t\t\t return uid;");
            }
            sb.Append("\n\t\t }");
            #endregion

            #region DeleteObj
            sb.AppendLine();
            if (Util.Utility.GetSelectedDB().Equals("Oracle"))
            {
                sb.Append("\n\t\t public int Delete()");
                sb.Append("\n\t\t {");
                sb.Append("\n\t\t\t int i = -1;");
            }
            else if (Util.Utility.GetSelectedDB().Equals("SqlServer"))
            {
                sb.Append("\n\t\t public string Delete(out int intErrorCode)");
                sb.Append("\n\t\t {");
                sb.Append("\n\t\t\t intErrorCode = 0;");
                sb.Append("\n\t\t\t string uid = \"\";");
            }

            sb.Append("\n\t\t\t " + strObjectName + " obj = new " + strObjectName + "();");
            sb.Append("\n\t\t\t DBTransaction transaction = new DBTransaction();");
            sb.AppendLine();
            sb.Append("\n\t\t\t try");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t if (transaction != null) transaction.Begin();");
            sb.AppendLine();
            sb.Append("\n\t\t\t\t obj." + _NUMID + " = IView." + _NUMID + ";");
            sb.Append("\n\t\t\t\t obj." + _STRUID + " = IView." + _STRUID + ";");
            if (Util.Utility.GetSelectedDB().Equals("Oracle"))
            {
                sb.Append("\n\t\t\t\t i = Data.Delete(obj, transaction);");
                sb.AppendLine();
                sb.Append("\n\t\t\t\t if (i >= 0)");
            }
            else if (Util.Utility.GetSelectedDB().Equals("SqlServer"))
            {
                sb.Append("\n\t\t\t\t uid = Data.Delete(obj, out intErrorCode);");
                sb.AppendLine();
                sb.Append("\n\t\t\t\t if (intErrorCode >= 0)");
            }
            sb.Append("\n\t\t\t\t\t transaction.Commit();");
            sb.Append("\n\t\t\t\t else");
            sb.Append("\n\t\t\t\t\t throw new Exception();");
            
            sb.Append("\n\t\t\t }"); //end of Try
            sb.Append("\n\t\t\t catch (Exception ex)");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t transaction.RollBack();");
            sb.Append("\n\t\t\t }");
            sb.Append("\n\t\t\t finally");
            sb.Append("\n\t\t\t {");
            sb.Append("\n\t\t\t\t transaction.Dispose();");
            sb.Append("\n\t\t\t }");
            sb.AppendLine();
            if (Util.Utility.GetSelectedDB().Equals("Oracle"))
            {
                sb.Append("\n\t\t\t return i;");
            }
            else if (Util.Utility.GetSelectedDB().Equals("SqlServer"))
            {
                sb.Append("\n\t\t\t return uid;");
            }
            sb.Append("\n\t\t }");
            #endregion

            sb.Append("\n\t }"); //end of Presenter

            sb.AppendLine();
            sb.AppendLine();

            #region SearchPresenter
            sb.Append("\n\t public class " + strObjectName + "SearchPresenter");
            sb.Append("\n\t {");
            sb.Append("\n\t\t private I" + strObjectName + "SearchView IView;");
            sb.Append("\n\t\t private " + strObjectName + "Data Data;");

            sb.AppendLine();
            sb.Append("\n\t\t public " + strObjectName + "SearchPresenter(I" + strObjectName + "SearchView view)");
            sb.Append("\n\t\t {");
            sb.Append("\n\t\t\t this.IView = view;");
            sb.Append("\n\t\t\t Data = new " + strObjectName + "Data();");
            sb.Append("\n\t\t }");

            sb.AppendLine();
            sb.Append("\n\t\t public int GetObjCount(");
            //Paremeters
            string strPatrams = "";
            string strPatramsWithoutDataType = "";
            foreach (DataRow row in dt.Rows)
            {
                if (row["COLUMN_NAME"].ToString().Contains(_STRUID) || row["COLUMN_NAME"].ToString().Contains(_STRLASTUID) ||
                    row["COLUMN_NAME"].ToString().Contains(_DTUDT) || row["COLUMN_NAME"].ToString().Contains(_DTLASTUDT))
                    continue;

                string strDataType = Utility.GetDotNetDataType(row["DATA_TYPE"].ToString().ToUpper(), row["PRECISION"].ToString(), row["SCALE"].ToString());
                strPatrams += " " + strDataType + " " + row["COLUMN_NAME"] + ",";
                strPatramsWithoutDataType += " " + row["COLUMN_NAME"] + ",";
            }
            strPatrams = strPatrams.TrimEnd(',');
            strPatramsWithoutDataType = strPatramsWithoutDataType.TrimEnd(',');
            sb.Append(strPatrams + ")");
            sb.Append("\n\t\t {");
            sb.Append("\n\t\t\t return Data.GetObjCount(" + strPatramsWithoutDataType + ");");
            sb.Append("\n\t\t }");

            sb.AppendLine();
            sb.Append("\n\t\t public List<" + strObjectName + "> GetObjList(");
            sb.Append(strPatrams + ", \n\t\t\t\t\t\t "+ Util.Utility.GetCommonSearchParams(true) +")");
            sb.Append("\n\t\t {");
            sb.Append("\n\t\t\t return Data.GetObjList(");
            sb.Append(strPatramsWithoutDataType + ", \n\t\t\t\t\t\t " + Util.Utility.GetCommonSearchParams(false) + "" + ");");
            sb.Append("\n\t\t }");
            sb.Append("\n\t }"); //end of SearchPresenter
            #endregion //End of SearchPresenter

            sb.Append("\n }"); //end of Namespace
            #endregion End of Presenter

            return sb;
        }
    }
}
