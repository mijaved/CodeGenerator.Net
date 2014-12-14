using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace CodeGenerator.MVP
{
    /// <summary>
    /// Interaction logic for SPCreator_Oracle.xaml
    /// </summary>
    public partial class SPCreatorUI : Window
    {
        #region Properties
        Util.IDbBuilder _iDbBuilder = null;
        Util.IObjectBuilder _iObjectBuilder = null;
        DataTable _dtTables = null;
        string _strProjectName = "";
        string _strTableName = "";
        string _strFileSavedDirectory = "";
        string _strMessageBoxCaption = "C#.Net MVP Code Generator";
        #endregion

        public SPCreatorUI()
        {
            InitializeComponent();

            _strProjectName = Util.Utility.GetProjectName();
            _strFileSavedDirectory = Util.Utility.GetSaveDirectoty();

            GetDbBuilder();
            BindDropDownList();
        }

        #region UserFunctions
        private void GetDbBuilder()
        {
            if (Util.Utility.GetSelectedDB().Equals("Oracle"))
            {
                _iDbBuilder = new Util.DbBuilderOracle();
                _iObjectBuilder = new Util.ObjectBuilderOracle();
            }
            else if (Util.Utility.GetSelectedDB().Equals("SqlServer"))
            {
                _iDbBuilder = new Util.DbBuilderSqlServer();
                _iObjectBuilder = new Util.ObjectBuilderSqlServer();
            }
            else
            {
                MessageBox.Show("No Database Selected!", _strMessageBoxCaption, MessageBoxButton.OK);
            }
        }

        private void BindDropDownList()
        {
            _dtTables = DAL.SPCreatorDAL.GetTables();

            foreach (DataRow row in _dtTables.Rows)
            {
                ddlTables.Items.Add(row["TABLE_NAME"]);
            }
            ddlTables.Items.Insert(0, "ALL");
            ddlTables.SelectedIndex = 0;
        }
        
        private DataTable GetColumnsDesc()
        {
            return DAL.SPCreatorDAL.GetColumnsDesc(_strTableName);
        }
        #endregion

        #region Events
        private void btnCreateSequence_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ddlTables.SelectedValue != null && ddlTables.SelectedValue.ToString().Equals("ALL"))
                {
                    if (MessageBox.Show("Do you want to Create Sequence for All tables?", _strMessageBoxCaption, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        foreach (DataRow row in _dtTables.Rows)
                        {
                            _strTableName = row["TABLE_NAME"].ToString();
                            StringBuilder sb = _iDbBuilder.BuildSequence(_strTableName);
                            string filePath = _strFileSavedDirectory + "SEQUENCE_Script.sql";
                            
                            if (!string.IsNullOrEmpty(sb.ToString()))
                            {
                                Util.Utility.WriteToDisk(filePath, sb.ToString());
                            }
                            else
                            {
                                MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                            }
                        }
                    }
                }
                else if (ddlTables.SelectedValue != null && !string.IsNullOrEmpty(ddlTables.SelectedValue.ToString()))
                {
                    _strTableName = ddlTables.SelectedValue.ToString();
                    StringBuilder sb = _iDbBuilder.BuildSequence(_strTableName);
                    string filePath = _strFileSavedDirectory + "SEQUENCE_" + _strTableName + "_Script.sql";
                    
                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        Util.Utility.WriteToDisk(filePath, sb.ToString());
                    }
                    else
                    {
                        MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _strMessageBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCreateSaveProcedure_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ddlTables.SelectedValue != null && ddlTables.SelectedValue.ToString().Equals("ALL"))
                {
                    if (MessageBox.Show("Do you want to Create Save Procedure for All tables?", _strMessageBoxCaption, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        foreach (DataRow row in _dtTables.Rows)
                        {
                            _strTableName = row["TABLE_NAME"].ToString();
                            DataTable dt = GetColumnsDesc();
                            StringBuilder sb = _iDbBuilder.BuildSaveProcedure(_strTableName, dt);
                            string filePath = _strFileSavedDirectory + "usp" + _strTableName + "Save.sql";
                            
                            if (!string.IsNullOrEmpty(sb.ToString()))
                            {
                                Util.Utility.WriteToDisk(filePath, sb.ToString());
                            }
                            else
                            {
                                MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                            }
                        }
                    }
                }
                else if (ddlTables.SelectedValue != null && !string.IsNullOrEmpty(ddlTables.SelectedValue.ToString()))
                {
                    _strTableName = ddlTables.SelectedValue.ToString();
                    DataTable dt = GetColumnsDesc();
                    StringBuilder sb = _iDbBuilder.BuildSaveProcedure(_strTableName, dt);
                    string filePath = _strFileSavedDirectory + "usp" + _strTableName + "Save.sql";
                    
                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        Util.Utility.WriteToDisk(filePath, sb.ToString());
                    }
                    else
                    {
                        MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _strMessageBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCreateGetProcedure_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ddlTables.SelectedValue != null && ddlTables.SelectedValue.ToString().Equals("ALL"))
                {
                    if (MessageBox.Show("Do you want to Create Get Procedure for All tables?", _strMessageBoxCaption, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        foreach (DataRow row in _dtTables.Rows)
                        {
                            _strTableName = row["TABLE_NAME"].ToString();
                            DataTable dt = GetColumnsDesc();
                            StringBuilder sb = _iDbBuilder.BuildGetProcedure(_strTableName, dt);
                            string filePath = _strFileSavedDirectory + "usp" + _strTableName + "Get.sql";

                            if (!string.IsNullOrEmpty(sb.ToString()))
                            {
                                Util.Utility.WriteToDisk(filePath, sb.ToString());
                            }
                            else
                            {
                                MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                            }
                        }
                    }
                }
                else if (ddlTables.SelectedValue != null && !string.IsNullOrEmpty(ddlTables.SelectedValue.ToString()))
                {
                    _strTableName = ddlTables.SelectedValue.ToString();
                    DataTable dt = GetColumnsDesc();
                    StringBuilder sb = _iDbBuilder.BuildGetProcedure(_strTableName, dt);

                    string filePath = _strFileSavedDirectory + "usp" + _strTableName + "Get.sql";

                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        Util.Utility.WriteToDisk(filePath, sb.ToString());
                    }
                    else
                    {
                        MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                    }

                    //Ext
                    StringBuilder sb2 = _iDbBuilder.BuildGetProcedureSingle(_strTableName, dt);
                    string filePath2 = _strFileSavedDirectory + "usp" + _strTableName + "SingleGet.sql";

                    if (!string.IsNullOrEmpty(sb2.ToString()))
                    {
                        Util.Utility.WriteToDisk(filePath2, sb2.ToString());
                    }
                    //else { MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK); }

                    //Ext
                    StringBuilder sb3 = _iDbBuilder.BuildGetProcedureParameterized(_strTableName, dt);
                    string filePath3 = _strFileSavedDirectory + "usp" + _strTableName + "ParameterizedGet.sql";

                    if (!string.IsNullOrEmpty(sb3.ToString()))
                    {
                        Util.Utility.WriteToDisk(filePath3, sb3.ToString());
                    }
                    //else { MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK); }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _strMessageBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnObjectClassCreator_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string strProjectName = _strProjectName;
                if (ddlTables.SelectedValue != null && ddlTables.SelectedValue.ToString().Equals("ALL"))
                {
                    if (MessageBox.Show("Do you want to Create Object Class for All tables?", _strMessageBoxCaption, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        foreach (DataRow row in _dtTables.Rows)
                        {
                            _strTableName = row["TABLE_NAME"].ToString();
                            string strObjectName = _strTableName.Replace("TBL_", "").Replace("tbl_", "").Replace("TBL", "").Replace("tbl", "").Replace("T_", "").Replace("t_", "");
                            DataTable dt = GetColumnsDesc();
                            StringBuilder sb = _iObjectBuilder.BuildObject(strProjectName, strObjectName, dt);
                            string filePath = _strFileSavedDirectory + strObjectName + ".cs";

                            if (!string.IsNullOrEmpty(sb.ToString()))
                            {
                                Util.Utility.WriteToDisk(filePath, sb.ToString());
                            }
                            else
                            {
                                MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                            }
                        }
                    }
                }
                else if (ddlTables.SelectedValue != null && !string.IsNullOrEmpty(ddlTables.SelectedValue.ToString()))
                {
                    _strTableName = ddlTables.SelectedValue.ToString();
                    string strObjectName = _strTableName.Replace("TBL_", "").Replace("tbl_", "").Replace("TBL", "").Replace("tbl", "").Replace("T_", "").Replace("t_", "");
                    DataTable dt = GetColumnsDesc();
                    StringBuilder sb = _iObjectBuilder.BuildObject(strProjectName, strObjectName, dt);
                    string filePath = _strFileSavedDirectory + strObjectName + ".cs";

                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        Util.Utility.WriteToDisk(filePath, sb.ToString());
                    }
                    else
                    {
                        MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _strMessageBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDALClassCreator_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string strProjectName = _strProjectName;
                if (ddlTables.SelectedValue != null && ddlTables.SelectedValue.ToString().Equals("ALL"))
                {
                    if (MessageBox.Show("Do you want to Create DAL Class for All tables?", _strMessageBoxCaption, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        foreach (DataRow row in _dtTables.Rows)
                        {
                            _strTableName = row["TABLE_NAME"].ToString();
                            string strObjectName = _strTableName.Replace("TBL_", "").Replace("tbl_", "").Replace("TBL", "").Replace("tbl", "").Replace("T_", "").Replace("t_", "");
                            DataTable dt = GetColumnsDesc();
                            StringBuilder sb = _iObjectBuilder.BuildDAL(strProjectName, strObjectName, dt);
                            string filePath = _strFileSavedDirectory + strObjectName + "DAL.cs";

                            if (!string.IsNullOrEmpty(sb.ToString()))
                            {
                                Util.Utility.WriteToDisk(filePath, sb.ToString());
                            }
                            else
                            {
                                MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                            }
                        }
                    }
                }
                else if (ddlTables.SelectedValue != null && !string.IsNullOrEmpty(ddlTables.SelectedValue.ToString()))
                {
                    _strTableName = ddlTables.SelectedValue.ToString();
                    string strObjectName = _strTableName.Replace("TBL_", "").Replace("tbl_", "").Replace("TBL", "").Replace("tbl", "").Replace("T_", "").Replace("t_", "");
                    DataTable dt = GetColumnsDesc();
                    StringBuilder sb = _iObjectBuilder.BuildDAL(strProjectName, strObjectName, dt);
                    string filePath = _strFileSavedDirectory + strObjectName + "DAL.cs";

                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        Util.Utility.WriteToDisk(filePath, sb.ToString());
                    }
                    else
                    {
                        MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _strMessageBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDATAClassCreator_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string strProjectName = _strProjectName;
                if (ddlTables.SelectedValue != null && ddlTables.SelectedValue.ToString().Equals("ALL"))
                {
                    if (MessageBox.Show("Do you want to Create Data Class for All tables?", _strMessageBoxCaption, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        foreach (DataRow row in _dtTables.Rows)
                        {
                            _strTableName = row["TABLE_NAME"].ToString();
                            string strObjectName = _strTableName.Replace("TBL_", "").Replace("tbl_", "").Replace("TBL", "").Replace("tbl", "").Replace("T_", "").Replace("t_", "");
                            DataTable dt = GetColumnsDesc();
                            StringBuilder sb = _iObjectBuilder.BuildDATA(strProjectName, strObjectName, dt);
                            string filePath = _strFileSavedDirectory + strObjectName + "Data.cs";

                            if (!string.IsNullOrEmpty(sb.ToString()))
                            {
                                Util.Utility.WriteToDisk(filePath, sb.ToString());
                            }
                            else
                            {
                                MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                            }
                        }
                    }
                }
                else if (ddlTables.SelectedValue != null && !string.IsNullOrEmpty(ddlTables.SelectedValue.ToString()))
                {
                    _strTableName = ddlTables.SelectedValue.ToString();
                    string strObjectName = _strTableName.Replace("TBL_", "").Replace("tbl_", "").Replace("TBL", "").Replace("tbl", "").Replace("T_", "").Replace("t_", "");
                    DataTable dt = GetColumnsDesc();
                    StringBuilder sb = _iObjectBuilder.BuildDATA(strProjectName, strObjectName, dt);
                    string filePath = _strFileSavedDirectory + strObjectName + "Data.cs";

                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        Util.Utility.WriteToDisk(filePath, sb.ToString());
                    }
                    else
                    {
                        MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _strMessageBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnPresenterClassCreator_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string strProjectName = _strProjectName;
                if (ddlTables.SelectedValue != null && ddlTables.SelectedValue.ToString().Equals("ALL"))
                {
                    if (MessageBox.Show("Do you want to Create Presenter Class for All tables?", _strMessageBoxCaption, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        foreach (DataRow row in _dtTables.Rows)
                        {
                            _strTableName = row["TABLE_NAME"].ToString();
                            string strObjectName = _strTableName.Replace("TBL_", "").Replace("tbl_", "").Replace("TBL", "").Replace("tbl", "").Replace("T_", "").Replace("t_", "");
                            DataTable dt = GetColumnsDesc();
                            StringBuilder sbView = new StringBuilder();
                            StringBuilder sb = _iObjectBuilder.BuildPRESENTER(strProjectName, strObjectName, dt, out sbView);
                            string filePath = _strFileSavedDirectory + strObjectName + "Presenter.cs";
                            string fileViewPath = _strFileSavedDirectory + "I" + strObjectName + "View.cs";

                            if (!string.IsNullOrEmpty(sb.ToString()))
                            {
                                Util.Utility.WriteToDisk(filePath, sb.ToString());

                                if (!string.IsNullOrEmpty(sbView.ToString()))
                                    Util.Utility.WriteToDisk(fileViewPath, sbView.ToString());
                            }
                            else
                            {
                                MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                            }
                        }
                    }
                }
                else if (ddlTables.SelectedValue != null && !string.IsNullOrEmpty(ddlTables.SelectedValue.ToString()))
                {
                    _strTableName = ddlTables.SelectedValue.ToString();
                    string strObjectName = _strTableName.Replace("TBL_", "").Replace("tbl_", "").Replace("TBL", "").Replace("tbl", "").Replace("T_", "").Replace("t_", "");
                    DataTable dt = GetColumnsDesc();
                    StringBuilder sbView = new StringBuilder();
                    StringBuilder sb = _iObjectBuilder.BuildPRESENTER(strProjectName, strObjectName, dt, out sbView);
                    string filePath = _strFileSavedDirectory + strObjectName + "Presenter.cs";
                    string fileViewPath = _strFileSavedDirectory + "I" + strObjectName + "View.cs";

                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        Util.Utility.WriteToDisk(filePath, sb.ToString());

                        if (!string.IsNullOrEmpty(sbView.ToString()))
                            Util.Utility.WriteToDisk(fileViewPath, sbView.ToString());
                    }
                    else
                    {
                        MessageBox.Show("OOPs!! There's nothing to create...", _strMessageBoxCaption, MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _strMessageBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
        
    }
}
