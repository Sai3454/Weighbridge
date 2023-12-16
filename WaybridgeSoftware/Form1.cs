using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data.OleDb;
using System.IO;

namespace WaybridgeSoftware
{
    public partial class idloginform : Form
    {
        string connectionString = "";
        public idloginform()
        {
            InitializeComponent();
            //CreateFolderToRoot();
            txtUsername.Text = "";
            txtpassword.Text = "";
            connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\WaybridgeDB\WeighingBridge_AccDB.accdb;";
            GetLoggedInCompanyInfo();
        }
        public void CreateFolderToRoot()
        {

            try
            {
                string folderPath = @"C:WaybridgeDB";
                string filePath = Path.Combine(folderPath, "WaybridgeDB.accdb");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string binFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WeighingBridge_AccDB.accdb");
                string destinationFilePath = Path.Combine(folderPath, "WeighingBridge_AccDB.accdb");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                File.Copy(binFilePath, destinationFilePath, true);
                //
            }
            catch (Exception ex)
            {

                MessageBox.Show("An error occured while creating root folder," + ex.Message);
            }
        
        }

        public void GetLoggedInCompanyInfo()
        {
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    
                    using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM companyInfo", connection))
                    {
                        DataSet ds = new DataSet();
                        cmd.CommandType = CommandType.Text;
                        using (OleDbDataAdapter sda = new OleDbDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);
                                label3.Text = dt.Rows[0]["CompanyName"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Some error occured while connecting database please contact administrator"+ex.Message);
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
               
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    //MessageBox.Show("Connection establish successfully !");
                    // your code here
                }
                if (txtUsername.Text == "Admin" && txtpassword.Text == "Admin@123")
                {
                    this.Hide();
                    dashboard db = new dashboard();
                    //CameraDisplay db = new CameraDisplay();
                    db.Show();
                }
                else
                {
                    MessageBox.Show("Please enter Correct Username and Password");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Error=>" + ex.Message);
            }
        }

        private void btnloginexit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
