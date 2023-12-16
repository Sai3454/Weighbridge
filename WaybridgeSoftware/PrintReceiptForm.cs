using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;

using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WaybridgeSoftware
{
	public partial class PrintReceiptForm : Form
	{
        string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\WaybridgeDB\WeighingBridge_AccDB.accdb;";
        public PrintReceiptForm()
		{
			InitializeComponent();
            this.FormClosing += PrintReceiptForm_FormClosing;
        }
        private void PrintReceiptForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Perform any necessary cleanup or processing before the form is closed
            dashboard dash = new dashboard();
            dash.Show();
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
		{

		}
        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(memoryImage, 0, 0);
        }
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern long BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
        private Bitmap memoryImage;

        private void PrintScreen()
        {
            Graphics mygraphics = this.CreateGraphics();
            Size s = this.Size;
            memoryImage = new Bitmap(s.Width, s.Height, mygraphics);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            IntPtr dc1 = mygraphics.GetHdc();
            IntPtr dc2 = memoryGraphics.GetHdc();
            BitBlt(dc2, 0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height, dc1, 0, 0, 13369376);
            mygraphics.ReleaseHdc(dc1);
            memoryGraphics.ReleaseHdc(dc2);
        }

        private void PrintReceiptForm_Load(object sender, EventArgs e)
        {
             Pen pen = new Pen(Color.FromArgb(255, 0, 0, 255), 8);
            pen.StartCap = LineCap.ArrowAnchor;
            pen.EndCap = LineCap.RoundAnchor;
          //  e.Graphics.DrawLine(pen, 20, 175, 300, 175);
            FillCompDetails();
            //PrintScreen();
            //printPreviewDialog1.ShowDialog();
        }

        public void FillCompDetails()
        {
            // Create a connection string  
            //string ConnectionString = "Integrated Security = SSPI; " +
            //"Initial Catalog= Northwind; " + " Data source = localhost; ";
            string SQL = "SELECT CompanyName,Address, Operator FROM CompanyInfo where id =1";

            // create a connection object  
            OleDbConnection  conn = new OleDbConnection(connectionString);

            // Create a command object  
            OleDbCommand cmd = new OleDbCommand(SQL, conn);
            conn.Open();

            // Call ExecuteReader to return a DataReader  
            OleDbDataReader reader = cmd.ExecuteReader();
           

            while (reader.Read())
            {
                lblCompanyName.Text = reader["CompanyName"].ToString();

                lblCompanyAddress.Text = reader["Address"].ToString();
                lblcompoperator.Text = reader["Operator"].ToString();

            }

            //Release resources  
            reader.Close();
            conn.Close();
        }
        private void btnPrint_Click(object sender, EventArgs e)
        {
            //PrintScreen();
            
            //printPreviewDialog1.ShowDialog();

        }

        private void btnprint_Click_1(object sender, EventArgs e)
        {
            PrintScreen();

            printPreviewDialog1.ShowDialog();
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
