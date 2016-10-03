using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.OleDb;

namespace CodeGenerator.MVP.DAL
{
    public class SPCreatorDAL
    {        
        public static OleDbConnection con = null;
        
        //SELECT name AS DATABASENAME FROM master.dbo.sysdatabases --SqlServer Database names
        //select * from all_users; OR select username from dba_users;  --Oracle user names
        
        public static DataTable GetTables()
        {
            con = new OleDbConnection(Util.Utility.GetConnectionString());
            con.Open();
            OleDbCommand cmd = null;

            if (Util.Utility.GetSelectedDB().Equals("Oracle"))
            {
                cmd = new OleDbCommand("SELECT T.TABLE_NAME FROM tabs T Order by T.TABLE_NAME", con);
            }
            else if (Util.Utility.GetSelectedDB().Equals("SqlServer"))
            {
                cmd = new OleDbCommand("Select T.Name AS TABLE_NAME from sys.Tables T Order by Name", con);
            }

            OleDbDataReader dr = null;

            DataTable dt = new DataTable();
            dt.Columns.Add("TABLE_NAME");

            try
            {
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    DataRow drow = dt.NewRow();
                    drow["TABLE_NAME"] = dr["TABLE_NAME"].ToString();

                    dt.Rows.Add(drow);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
                //MessageBox.Show(ex.Message, "CodeGenerator.MVP", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dr.Close(); dr.Dispose();
                con.Close(); con.Dispose();
            }

            return dt;
        }

        public static DataTable GetColumnsDesc(string strTableName)
        {
            con = new OleDbConnection(Util.Utility.GetConnectionString());
            con.Open();
            OleDbCommand cmd = null;

            if (Util.Utility.GetSelectedDB().Equals("Oracle"))
            {
                cmd = new OleDbCommand($"SELECT t.COLUMN_NAME, t.DATA_TYPE, t.CHAR_LENGTH AS MAX_LENGTH, NVL(t.DATA_PRECISION, 0) AS PRECISION, NVL(t.DATA_SCALE, 0) AS SCALE from user_tab_columns t where t.TABLE_NAME = '{strTableName}'", con);
            }
            else if (Util.Utility.GetSelectedDB().Equals("SqlServer"))
            {
                cmd = new OleDbCommand($"SELECT cl.name AS COLUMN_NAME, tp.name AS DATA_TYPE, cl.Max_Length AS MAX_LENGTH, cl.Precision AS PRECISION, cl.Scale AS SCALE FROM sys.columns cl JOIN  sys.systypes tp ON tp.xtype = cl.system_type_id WHERE object_id = OBJECT_ID('dbo.{strTableName}') AND tp.status = 0 Order by cl.Column_ID", con);
            }

            OleDbDataReader dr = null;

            DataTable dt = new DataTable();
            dt.Columns.Add("COLUMN_NAME");
            dt.Columns.Add("DATA_TYPE");
            dt.Columns.Add("MAX_LENGTH");
            dt.Columns.Add("PRECISION");
            dt.Columns.Add("SCALE");

            try
            {
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    DataRow drow = dt.NewRow();
                    drow["COLUMN_NAME"] = dr["COLUMN_NAME"].ToString();
                    drow["DATA_TYPE"] = dr["DATA_TYPE"].ToString();
                    drow["MAX_LENGTH"] = dr["MAX_LENGTH"].ToString();
                    drow["PRECISION"] = dr["PRECISION"].ToString();
                    drow["SCALE"] = dr["SCALE"].ToString();

                    dt.Rows.Add(drow);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
                //MessageBox.Show(ex.Message, "CodeGenerator.MVP", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dr.Close();  dr.Dispose();
                con.Close(); con.Dispose();
            }

            return dt;
        }

    }
}
