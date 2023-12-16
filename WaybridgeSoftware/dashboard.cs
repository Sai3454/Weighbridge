using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using static WaybridgeSoftware.CHCNetSDK;
using System.IO.Ports;
using System.Collections;
using System.Xml.Linq;
using System.Security.Principal;
using System.Windows;
using System.Threading;
using Microsoft.Office.Interop.Excel;
//using System.Data.SqlClient;
using HikVision;
using System.Text.RegularExpressions;
using AForge.Video;

namespace WaybridgeSoftware
{
    public partial class dashboard : Form
    {
        string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\WaybridgeDB\WeighingBridge_AccDB.accdb;";
        private uint iLastErr = 0;
        private uint iLastErr1 = 0;
        private Int32 m_lUserID = -1;
        private Int32 m_lUserID2 = -1;
        private Int32 m_lUserID3 = -1;
        private bool m_bInitSDK = false;
        private bool m_bRecord = false;
        private bool m_bTalk = false;
        private Int32 m_lRealHandle = -1;
        private Int32 m_lRealHandle2 = -1;
        private Int32 m_lRealHandle3 = -1;
        private int lVoiceComHandle = -1;
        private string str, str1;
        SerialPort port = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);
        public static string tierWeight = string.Empty;
        static bool _continue;
        static SerialPort _serialPort;
        static string portValue;

        //----------------------------------
        CHCNetSDK.CHCNetSDK.REALDATACALLBACK RealData = null;
        public CHCNetSDK.CHCNetSDK.NET_DVR_PTZPOS m_struPtzCfg;

        CHCNetSDK.CHCNetSDK.REALDATACALLBACK RealData1 = null;
        public CHCNetSDK.CHCNetSDK.NET_DVR_PTZPOS m_struPtzCfg1;
        //------------------------

        //string connectionString = ConfigurationManager.ConnectionStrings["MyconnectionString"].ConnectionString;

        public dashboard()
        {
           

            this.KeyPress += txtTareweight_KeyPress;


            //[DllImport(@"..\bin\HCNetSDK.dll")]
            //
            // Windows ´°ÌåÉè¼ÆÆ÷Ö§³ÖËù±ØÐèµÄ
            //
            InitializeComponent();
            // const string _dllLocation = "D:\\RunningnetProjects\\TRUELANCER\\WayBridge\\WaybridgeSoftware\\lib\\" + "HCNetSDK.dll";
            //Read From Commport

            m_bInitSDK = CHCNetSDK.CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            else
            {
                //±£´æSDKÈÕÖ¾ To save the SDK log
                CHCNetSDK.CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
            }
            //m_bInitSDK =  CHCNetSDK.NET_DVR_Init();
            //if (m_bInitSDK == false)
            //{
            //    MessageBox.Show("NET_DVR_Init error!");
            //    return;
            //}
            //else
            //{
            //    //±£´æSDKÈÕÖ¾ To save the SDK log
            //    CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
            //}
            //
            // TODO: ÔÚ InitializeComponent µ÷ÓÃºóÌí¼ÓÈÎºÎ¹¹Ôìº¯Êý´úÂë
            //
            GenerateTicket();
        }
        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
            if (dwBufSize > 0)
            {
                byte[] sData = new byte[dwBufSize];
                Marshal.Copy(pBuffer, sData, 0, (Int32)dwBufSize);

                string str = "ÊµÊ±Á÷Êý¾Ý.ps";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)dwBufSize;
                fs.Write(sData, 0, iLen);
                fs.Close();
            }
        }
       
        public void SaveAllCamImages(Guid imageuID)
        {
            //string workingDirectory = Environment.CurrentDirectory;
            //// or: Directory.GetCurrentDirectory() gives the same result

            //// This will get the current PROJECT bin directory (ie ../bin/)
            //string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;

            string projectDirectory = @"C://";

            // This will get the current PROJECT directory
            // string projectDirectory1 = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string imagesavepath = projectDirectory + "\\CamImages\\";
            string imageFileName = imageuID.ToString();

            if (!Directory.Exists(imagesavepath))
            {
                Directory.CreateDirectory(imagesavepath);
            }

            string channelNo1 = "1";
            string channelNo2 = "2";
            string channelNo3 = "3";
            string sJpegPicFileName1;
            string sJpegPicFileName2;
            string sJpegPicFileName3;
            //Í¼Æ¬±£´æÂ·¾¶ºÍÎÄ¼þÃû the path and file name to save
            sJpegPicFileName1 = imagesavepath + imageFileName + "_cam1.jpg";
            sJpegPicFileName2 = imagesavepath + imageFileName + "_cam2.jpg";
            sJpegPicFileName3 = imagesavepath + imageFileName + "_cam3.jpg";

            int lChannel1 = Int16.Parse(channelNo1); //Í¨µÀºÅ Channel number
            int lChannel2 = Int16.Parse(channelNo2); //Í¨µÀºÅ Channel number
            int lChannel3 = Int16.Parse(channelNo3); //Í¨µÀºÅ Channel number

            CHCNetSDK.CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //Í¼ÏñÖÊÁ¿ Image quality
            lpJpegPara.wPicSize = 0xff; //×¥Í¼·Ö±æÂÊ Picture size: 2- 4CIF£¬0xff- Auto(Ê¹ÓÃµ±Ç°ÂëÁ÷·Ö±æÂÊ)£¬×¥Í¼·Ö±æÂÊÐèÒªÉè±¸Ö§³Ö£¬¸ü¶àÈ¡ÖµÇë²Î¿¼SDKÎÄµµ

            // Pic one
            //JPEG×¥Í¼ Capture a JPEG picture
            if (!CHCNetSDK.CHCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserID, lChannel1, ref lpJpegPara, sJpegPicFileName1))
            {
                iLastErr = CHCNetSDK.CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;     
                MessageBox.Show(str);
                // return;
            }
            else
            {
                str = "Successful to capture the JPEG file and the saved file is " + sJpegPicFileName1;
                MessageBox.Show(str);
            }
            // Pic Two
            //JPEG×¥Í¼ Capture a JPEG picture
            if (!CHCNetSDK.CHCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserID2, lChannel1, ref lpJpegPara, sJpegPicFileName2))
            {
                iLastErr = CHCNetSDK.CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                MessageBox.Show(str);
                //  return;
            }
            else
            {
                str = "Successful to capture the JPEG file and the saved file is " + sJpegPicFileName2;
                MessageBox.Show(str);
            }
            //Pic Three
            //JPEG×¥Í¼ Capture a JPEG picture
            if (!CHCNetSDK.CHCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserID3, lChannel1, ref lpJpegPara, sJpegPicFileName3))
            {
                iLastErr = CHCNetSDK.CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                MessageBox.Show(str);
                //   return;
            }
            else
            {
                str = "Successful to capture the JPEG file and the saved file is " + sJpegPicFileName3;
                MessageBox.Show(str);
            }
            return;
        }
        public void GetVehicalMaster()
        {
            System.Data.DataTable dtReturn= new System.Data.DataTable();
            try
            {
                using (OleDbConnection con = new OleDbConnection(connectionString))
                {
                    using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM Vehicle_Master", con))
                    {
                        DataSet ds = new DataSet();
                        cmd.CommandType = CommandType.Text;
                        using (OleDbDataAdapter sda = new OleDbDataAdapter(cmd))
                        {
                          
                                sda.Fill(dtReturn);
                            dataGridView1.DataSource = dtReturn;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("An error occured while fatching vehical's list " + ex.Message);
            }
           
        }

        private void btnCapturePics_Click(object sender, EventArgs e)
        {

         
        }
        

        private void dashboard_Load(object sender, EventArgs e)
        {
            FillActiveReports();
            //GenerateTicket();
            // Set the DataSource property
            GetVehicalMaster();
            this.maintabControl.SelectedTab = tabVehicleMaster;
            btncallvehicle.Focus();
            txtVehicleNoT.Focus();
            ArrayList ConfigDetails = new ArrayList();
            this.dgvActiveTransactions.EnableHeadersVisualStyles = false;

            comboBox1.SelectedIndex = 6;

            if (yes.Checked == true)
            {
                rbttboth.Enabled = false;
                rbtmsingle.Enabled = false;
                rbtmMulti.Enabled = false;
                txtVehicleNoT.Enabled = false;
                txtItemNameT.Enabled = false;
                txtSupplierNameT.Enabled = false;
                txtCustomerName2.Enabled = false;
                txtChargesT.Enabled = false;
                lotNo.Enabled = false;
                NoOfBags.Enabled = false;
            }
            else
            {
                rbttboth.Enabled = true;
                rbtmsingle.Enabled = true;
                rbtmMulti.Enabled = true;
                txtVehicleNoT.Enabled = true;
                txtItemNameT.Enabled = true;
                txtSupplierNameT.Enabled = true;
                txtCustomerName2.Enabled = true;
                txtChargesT.Enabled = true;
                lotNo.Enabled = true;
                NoOfBags.Enabled = true;
            }

            DataGridViewTextBoxColumn col5 = new DataGridViewTextBoxColumn();
            col5.DefaultCellStyle.BackColor = Color.Yellow; col5.HeaderCell.Style.Font = new System.Drawing.Font("Arial", 8, FontStyle.Bold);
            col5.HeaderCell.Style.BackColor = Color.Yellow;

            using (OleDbConnection con = new OleDbConnection(connectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM DeviceCofigCams", con))
                {
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.Text;
                    using (OleDbDataAdapter sda = new OleDbDataAdapter(cmd))
                    {
                        using (System.Data.DataTable dt = new System.Data.DataTable())
                        {
                            sda.Fill(dt);
                            sda.Fill(ds);
                            //dataGridView1.DataSource = dt;
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                ConfigDetails.Add(row[2]);
                            }
                        }
                    }
                }
            }
          
            //getPortData();
            //lblAutoweight.Text = portValue.ToString();


            //

            //
            //maintabControl.SelectedTab = btncallvehicle;
            //LoadWeight();
        }
      

     
      
        private void button1_Click(object sender, EventArgs e)
        {
          
        }
       
       

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtvehicleNo.Text))
            {
                 
                txtvehicleNo.Focus();
                //  errorProviderApp.SetError(txtvehicleNo, "vehicle should not be left blank!");
                MessageBox.Show("vehicle should not be left blank!");
            }
            else
            {
                SaveVehicleMaster();
            }
        }

       
        private void txtTareweight_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;

            if (!char.IsNumber(ch) && !char.IsLetter(ch) && ch != 8 && ch != 46)  //8 is Backspace key; 46 is Delete key. This statement accepts dot key. 
                                                                                  //if (!char.IsLetterOrDigit(ch) && !char.IsLetter(ch) && ch != 8 && ch != 46)   //This statement accepts dot key. 
            {
                e.Handled = true;
                MessageBox.Show("Only accept digital character or letter.");
            }
        }
        
        public void SaveVehicleMaster()
        {
            OleDbConnection conn = new OleDbConnection(connectionString);
            OleDbCommand cmd = new OleDbCommand();
            //set our SQL Insert INTO statement
            string sqlInsert = "INSERT INTO Vehicle_Master ( VehicleNo,VehicleType, Account, TareWeight, Notes ) VALUES('" + txtvehicleNo.Text.Trim()  + "','" + txtVehicletype.Text.Trim() + "','" + txtAccount.Text.Trim() + "','" + txtTareweight.Text.Trim() + "','" + txtNotes.Text + "')";
            try
            {

                //open the connection
                conn.Open();
                //set the connection
                cmd.Connection = conn;
                //get the SQL statement to be executed
                cmd.CommandText = sqlInsert;
                //execute the query
                cmd.ExecuteNonQuery();
                //display a message
                MessageBox.Show("Vehicle Details Added!....");
                GetVehicalMaster();
                //close the connection
                conn.Close();

            }
            catch (Exception ex)
            {
                //this will display some error message if something 
                //went wrong to our code above during execution
                MessageBox.Show(ex.ToString());
            }
        }

        private void txtTareweight_KeyUp(object sender, CancelEventArgs e)
        {
            if (txtTareweight.Text.Length > 0)
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^(?=.)([+-]?([0-9]*)(\.([0-9]+))?)$");
                System.Text.RegularExpressions.Match match = regex.Match(txtTareweight.Text);
                if (match.Success)
                {
                   // MessageBox.Show("Valid");
                    
                }
                else
                {
                    MessageBox.Show("Invalid");
                    txtTareweight.Text = "";
                    txtTareweight.Focus();
                }
            }
        }

        private void btncallvehform_Click(object sender, EventArgs e)
        {
            maintabControl.SelectedTab = btncallvehicle;

            OleDbConnection conn1 = new OleDbConnection(connectionString);
            OleDbCommand cmd = new OleDbCommand();
            //set our SQL Insert INTO statement
            string sqlInsert = "select VehicleNo from Vehicle_Master where VehicleNo='" + txtvehicleNo.Text.Trim() + "'";
            try
            {

                //open the connection
                conn1.Open();
                //set the connection
                cmd.Connection = conn1;
                //get the SQL statement to be executed
                cmd.CommandText = sqlInsert;

                string VehicleNo;
                //execute the query
                VehicleNo = (string)cmd.ExecuteScalar();
                conn1.Close();
                if (VehicleNo == null)
                {
                    tabVehicleMaster.Focus();
                    maintabControl.SelectedTab = tabVehicleMaster;
                    txtvehicleNo.Text = txtvehicleNo.Text.Trim();
                }
                //display a message
                // MessageBox.Show("Vehicle Details Added!....");
                //close the connection


            }
            catch (Exception ex)
            {
                //this will display some error message if something 
                //went wrong to our code above during execution
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnTransSubmit_Click(object sender, EventArgs e)
        {
                if (string.IsNullOrWhiteSpace(txtVehicleNoT.Text))
            {

                txtVehicleNoT.Focus();
                //  errorProviderApp.SetError(txtvehicleNo, "vehicle should not be left blank!");
                MessageBox.Show("vehicle Number should not be left blank!");
            }
            else
            {
                SaveTransactionDetails();
            }
          
        }
        public void SaveTransactionDetails()
        {
            int trans_mode = 0;
            if (rbtmsingle.Checked == true)
            {
                trans_mode = 1;
            }
            if (rbtmDouble.Checked == true)
            {
                trans_mode = 2;
            }
            if (rbtmMulti.Checked == true)
            {
                trans_mode = 3;
            }
            //----------------
            int trans_type = 0;
            if (receipt.Checked == true)
            {
                trans_type = 1;
            }
            if (pds.Checked == true)
            {
                trans_type = 2;
            }
            if (wagon.Checked == true)
            {
                trans_type = 3;
            }
            //-------------------
            int weighment_type = 0;
            if (rbwtTare.Checked == true)
            {
                weighment_type = 1;
            }
            if (rbwtGross.Checked == true)
            {
                weighment_type = 2;
            }
            //-------------------
            int transaction_type = 0;
            if (rbttOutgoing.Checked == true)
            {
                transaction_type = 1;
            }
            if (rbttIncoming.Checked == true)
            {
                transaction_type = 2;
            }
            if (rbttboth.Checked == true)
            {
                transaction_type = 3;
            }

            //=====================
            string sqlInsertT = string.Empty;
            OleDbConnection connT = new OleDbConnection(connectionString);
            OleDbCommand cmdT = new OleDbCommand();
            Guid imagerefID = Guid.NewGuid();
            if (!string.IsNullOrEmpty(lblPkId.Text) && lblPkId.Text != ".")
            {
                decimal NetWeight =0;
                decimal TeirWeight = 0;
                decimal GrossWeight = 0;
                String SelectQuery = "";
                SelectQuery = "select * from TransactionDetails where ID="+lblPkId.Text+"";
                using (OleDbCommand cmd = new OleDbCommand(SelectQuery, connT))
                {
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.Text;
                    using (OleDbDataAdapter sda = new OleDbDataAdapter(cmd))
                    {
                        using (System.Data.DataTable dttable = new System.Data.DataTable())
                        {
                            sda.Fill(dttable);
                            if (dttable.Rows.Count > 0)
                            {

                                if (rbwtTare.Checked == true)
                                {
                                    if(no.Checked == true)
                                    {
                                        TeirWeight = Decimal.Parse(lblAutoweight.Text);
                                        NetWeight = Decimal.Parse(dttable.Rows[0]["GrossWeight"].ToString()) - TeirWeight;
                                        GrossWeight = Decimal.Parse(dttable.Rows[0]["GrossWeight"].ToString());
                                        sqlInsertT = "update TransactionDetails set NetWeight = " + NetWeight + ",TeirWeight = " + TeirWeight + ",TeirDate = '" + DateTime.Now.ToString() + "',Weight = '" + GrossWeight + "',GrossWeight = '" + GrossWeight + "',imagerefid = '" + imagerefID.ToString() + "' where ID = " + lblPkId.Text + "";
                                    }
                                    else
                                    {
                                        TeirWeight = Decimal.Parse(dttable.Rows[0]["TeirWeight"].ToString());
                                        NetWeight = Decimal.Parse(dttable.Rows[0]["NetWeight"].ToString());
                                        GrossWeight = Decimal.Parse(dttable.Rows[0]["GrossWeight"].ToString());
                                        sqlInsertT = "update TransactionDetails set NetWeight = " + NetWeight + ",TeirWeight = " + TeirWeight + ",TeirDate = '" + DateTime.Now.ToString() + "',Weight = '" + GrossWeight + "',GrossWeight = '" + GrossWeight + "' where ID = " + lblPkId.Text + "";
                                    }
                                }
                                if (rbwtGross.Checked == true)
                                {
                                    if (no.Checked == true)
                                    {
                                        GrossWeight = Decimal.Parse(lblAutoweight.Text);
                                        NetWeight = GrossWeight - Decimal.Parse(dttable.Rows[0]["TeirWeight"].ToString());
                                        TeirWeight = Decimal.Parse(dttable.Rows[0]["TeirWeight"].ToString());
                                        sqlInsertT = "update TransactionDetails set NetWeight = " + NetWeight + ",TeirWeight = " + TeirWeight + ",GrossDate = '" + DateTime.Now.ToString() + "',Weight = '" + GrossWeight + "',GrossWeight = '" + GrossWeight + "',imagerefid = '" + imagerefID.ToString() + "' where ID = " + lblPkId.Text + "";
                                    }
                                    else
                                    {
                                        TeirWeight = Decimal.Parse(dttable.Rows[0]["TeirWeight"].ToString());
                                        NetWeight = Decimal.Parse(dttable.Rows[0]["NetWeight"].ToString());
                                        GrossWeight = Decimal.Parse(dttable.Rows[0]["GrossWeight"].ToString());
                                        sqlInsertT = "update TransactionDetails set NetWeight = " + NetWeight + ",TeirWeight = " + TeirWeight + ",TeirDate = '" + DateTime.Now.ToString() + "',Weight = '" + GrossWeight + "',GrossWeight = '" + GrossWeight + "' where ID = " + lblPkId.Text + "";
                                    }
                                }
                            }
                            else
                            {
                                
                            }
                        }
                    }
                }
                //

                //update here
               // sqlInsertT ="update TransactionDetails set NetWeight = "+NetWeight+",TeirWeight = "+TeirWeight+",TeirDate = '"+DateTime.Now+"',Weight = '"+GrossWeight+"',GrossWeight = '"+GrossWeight+"' where ID = "+lblPkId.Text+"";
            }
            else
            {
                
               
                DateTime datetime = DateTime.Now;

                lblCurrentDatetime.Text = datetime.ToString();
                decimal TeriWeightF = 0;
                decimal GrossWeightF = 0;
                if (rbwtTare.Checked == true)
                {
                    TeriWeightF=Decimal.Parse(lblAutoweight.Text);
                    sqlInsertT = "INSERT INTO TransactionDetails ( VehicleNo,Customer_Name, Supplier_Name, Item_name, Trans_Mode,weightment_type,Trans_Type,Tran_Datetime,Weight,Charges,imagerefid,TicketNo,CreatedOn,NetWeight,TeirWeight,GrossWeight,TeirDate, Bags, LotNo, trans) " +
                  "VALUES('" + txtVehicleNoT.Text.Trim() + "','" + txtCustomerName2.Text.Trim() + "','" + txtSupplierNameT.Text.Trim() + "','" + txtItemNameT.Text.Trim() + "','" + trans_mode + "','" + weighment_type + "','" + transaction_type + "','" + lblCurrentDatetime.Text + "','" + lblAutoweight.Text.Trim() + "','" + txtChargesT.Text.Trim() + "','" + imagerefID.ToString() + "','" + lblTicketIDtext.Text + "','" + DateTime.Now.ToString() + "','0','" + TeriWeightF + "','" + GrossWeightF + "', '" + DateTime.Now.ToString() + "', '" + NoOfBags.Text.Trim() + "', '" + lotNo.Text.Trim() + "', '" + trans_type + "')";
                }

             
                if (rbwtGross.Checked == true)
                {
                    GrossWeightF = Decimal.Parse(lblAutoweight.Text);
                    sqlInsertT = "INSERT INTO TransactionDetails ( VehicleNo,Customer_Name, Supplier_Name, Item_name, Trans_Mode,weightment_type,Trans_Type,Tran_Datetime,Weight,Charges,imagerefid,TicketNo,CreatedOn,NetWeight,TeirWeight,GrossWeight,GrossDate, Bags, LotNo, trans) " +
                  "VALUES('" + txtVehicleNoT.Text.Trim() + "','" + txtCustomerName2.Text.Trim() + "','" + txtSupplierNameT.Text.Trim() + "','" + txtItemNameT.Text.Trim() + "','" + trans_mode + "','" + weighment_type + "','" + transaction_type + "','" + lblCurrentDatetime.Text + "','" + lblAutoweight.Text.Trim() + "','" + txtChargesT.Text.Trim() + "','" + imagerefID.ToString() + "','" + lblTicketIDtext.Text + "','" + DateTime.Now.ToString() + "','0','" + TeriWeightF + "','" + GrossWeightF + "', '" + DateTime.Now.ToString() + "', '" + NoOfBags.Text.Trim() + "', '" + lotNo.Text.Trim() + "', '" + trans_type +  "')";
                }

                //set our SQL Insert INTO statement
                //sqlInsertT = "INSERT INTO TransactionDetails ( VehicleNo,Customer_Name, Supplier_Name, Item_name, Trans_Mode,weightment_type,Trans_Type,Tran_Datetime,Weight,Charges,imagerefid,TicketNo,CreatedOn,NetWeight,TeirWeight,GrossWeight,GrossDate) " +
                  // "VALUES('" + txtVehicleNoT.Text.Trim() + "','" + txtCustomerName2.Text.Trim() + "','" + txtSupplierNameT.Text.Trim() + "','" + txtItemNameT.Text.Trim() + "','" + trans_mode + "','" + weighment_type + "','" + transaction_type + "','" + lblCurrentDatetime.Text + "','" + lblAutoweight.Text.Trim() + "','" + txtChargesT.Text.Trim() + "','" + imagerefID.ToString() + "','" + lblTicketIDtext.Text + "','" + DateTime.Now + "','0','" + TeriWeightF + "','" + GrossWeightF + "', '" + DateTime.Now + "')";
            }

        
            int Identity;
            try
            {


                Identity = 0;
                //open the connection
                connT.Open();
                //set the connection
                cmdT.Connection = connT;
                //get the SQL statement to be executed
                cmdT.CommandText = sqlInsertT;
                //execute the query
                cmdT.ExecuteNonQuery();
                if (!string.IsNullOrEmpty(lblPkId.Text) && lblPkId.Text != ".")
                {
                   
                }
                else
                {
                    cmdT.CommandText = "SELECT @@IDENTITY";
                    Identity = Convert.ToInt32(cmdT.ExecuteScalar());
                }
                
                   
                //display a message
                //  MessageBox.Show("Vehicle Details Added!....");
                //close the connection
                connT.Close();
                if(no.Checked == true)
                {
                    SaveAllCamImages(imagerefID);     
                }
             

                if (!string.IsNullOrEmpty(lblPkId.Text) && lblPkId.Text != ".")
                {
                    ShowTransactionDetails(Convert.ToInt32(lblPkId.Text));
                }
                else
                {

                    ShowTransactionDetails(Identity);
                }
                    
            }
            catch (Exception ex)
            {
                //this will display some error message if something 
                //went wrong to our code above during execution
                MessageBox.Show(ex.ToString());
            }


        }
        public void ShowTransactionDetails(int transID)
        {
            string workingDirectory = Environment.CurrentDirectory;
            // or: Directory.GetCurrentDirectory() gives the same result

            // This will get the current PROJECT bin directory (ie ../bin/)
            string projectDirectory = @"C://";

            // This will get the current PROJECT directory
            // string projectDirectory1 = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string imagesavepath = projectDirectory + "\\CamImages\\";
            // string imageFileName = imageuID.ToString();
         
            OleDbConnection connG = new OleDbConnection(connectionString);
            OleDbCommand cmdG = new OleDbCommand();
            string sqlInsertG = "select ID,VehicleNo,Customer_Name,Supplier_Name,Item_name,Trans_Mode,weightment_type,Trans_Type,Tran_Datetime,Weight,Charges,imagerefid,TicketNo,TeirWeight,TeirDate,NetWeight,GrossWeight,GrossDate from TransactionDetails where id= " + transID;
            try
            {
                PrintReceiptForm frm2 = new PrintReceiptForm();
                this.Hide();

                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    OleDbCommand command = new OleDbCommand(sqlInsertG, connection);
                    connection.Open();
                    OleDbDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        
                        frm2.lblTicketNo.Text = reader["TicketNo"].ToString();
                        frm2.lblVehicleNo.Text = reader[1].ToString();
                        frm2.lblCustomeName.Text = reader[2].ToString();
                        frm2.lblSupplierName.Text = reader[3].ToString();
                        frm2.lblItemName.Text = reader[4].ToString();
                        frm2.lblGrossweight.Text = reader["GrossWeight"].ToString();
                        frm2.lblTareWeight.Text = reader["TeirWeight"].ToString();
                        frm2.lblCharges.Text = reader[10].ToString();
                        frm2.lblNetWeight.Text = reader["NetWeight"].ToString();
                        frm2.lblTareDate.Text = reader["TeirDate"].ToString();
                        frm2.lblGrossDate.Text = reader["GrossDate"].ToString();
                        //frm2.lblVehicleNo.text = reader[0].ToString();
                        //frm2.lblVehicleNo.text = reader[0].ToString();
                        //frm2.pictureBox1.Image = RealPlayWnd.Image; //Image.FromFile(imagesavepath + reader[11].ToString() + "_cam1.jpg");
                        //frm2.pictureBox2.Image = RealPlayWnd.Image;
                        //frm2.pictureBox3.Image = RealPlayWnd.Image;
                        if (File.Exists(imagesavepath + reader[11].ToString() + "_cam1.jpg"))
                        {
                            frm2.pictureBox1.Image = Image.FromFile(imagesavepath + reader[11].ToString() + "_cam1.jpg");
                        }
                        if (File.Exists(imagesavepath + reader[11].ToString() + "_cam2.jpg"))
                        {
                            frm2.pictureBox2.Image = Image.FromFile(imagesavepath + reader[11].ToString() + "_cam2.jpg");
                        }
                        if (File.Exists(imagesavepath + reader[11].ToString() + "_cam3.jpg"))
                        {
                            frm2.pictureBox3.Image = Image.FromFile(imagesavepath + reader[11].ToString() + "_cam3.jpg");
                        }

                    }
                    frm2.Show();
                   // frm2.btnPrint.Hide();
                    reader.Close();
                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                //this will display some error message if something 
                //went wrong to our code above during execution
                MessageBox.Show(ex.ToString());
            }

            //frm2.lblPatientName.Text = txtPatientName.Text.ToString();
            //frm2.lblAge.Text = txtAge.Text.ToString();
            //frm2.lblAddress.Text = txtAddress.Text.ToString();
            //frm2.lblContactNo.Text = txtContactNo.Text.ToString();
            //frm2.lblEmergencyContactNo.Text = txtEmergencyContactNo.Text.ToString();
            //frm2.lblSex.Text = cmbSex.SelectedItem.ToString();
           // frm2.Show();


        }
        private void txtChargesT_KeyUp(object sender, CancelEventArgs e)
        {
            if (txtChargesT.Text.Length > 0)
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^(?=.)([+-]?([0-9]*)(\.([0-9]+))?)$");
                System.Text.RegularExpressions.Match match = regex.Match(txtChargesT.Text);
                if (match.Success)
                {
                    // MessageBox.Show("Valid");

                }
                else
                {
                    MessageBox.Show("Invalid");
                    txtChargesT.Text = "";
                    txtChargesT.Focus();
                }
            }
        }

        private void no_of_bags_KeyUp(object sender, CancelEventArgs e)
        {
            if (NoOfBags.Text.Length > 0)
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^(?=.)([+-]?([0-9]*)(\.([0-9]+))?)$");
                System.Text.RegularExpressions.Match match = regex.Match(NoOfBags.Text);
                if (match.Success)
                {
                    // MessageBox.Show("Valid");

                }
                else
                {
                    MessageBox.Show("Invalid");
                    NoOfBags.Text = "";
                    NoOfBags.Focus();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime datetime = DateTime.Now;

            lblCurrentDatetime.Text = datetime.ToString();
        }
        public  void tlblCurrentDatetime_Click(object sender, EventArgs e)

        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            

        }
        public void FillActiveReports(string fromDate="",string ToDate="")
        {
            System.Data.DataTable table = new System.Data.DataTable();
            table.Columns.Add("TiketNumber", typeof(string));
            table.Columns.Add("VehicleNumber", typeof(string));
            table.Columns.Add("Customer Name", typeof(string));
            table.Columns.Add("GrossWeight", typeof(string));
             table.Columns.Add("TeirWeight", typeof(string));
            table.Columns.Add("NetWeight", typeof(string));
            table.Columns.Add("Bags", typeof(string));
            table.Columns.Add("Lot Number", typeof(string));
            table.Columns.Add("transmode", typeof(string));
            table.Columns.Add("Status", typeof(string));
            //table.Columns.Add("Status", typeof(string));



            // table.Rows.Add(111, "Devesh", "Ghaziabad");
            //table.Rows.Add(222, "ROLI", "KANPUR");
            //table.Rows.Add(102, "ROLI", "MAINPURI");
            //table.Rows.Add(212, "DEVESH", "KANPUR");




            using (OleDbConnection con = new OleDbConnection(connectionString))
            {
                String SelectQuery = "";
                if (!string.IsNullOrEmpty(fromDate))
                {
                    SelectQuery = "select TicketNo , GrossWeight, TeirWeight, VehicleNo,Customer_Name,Supplier_Name,Item_name,trans,Bags,Customer_Name,lotNo,NetWeight,weightment_type,Trans_Type,Tran_Datetime,Weight,Charges,imagerefid from TransactionDetails where Tran_Datetime>=DateValue('" + fromDate+ "') and Tran_Datetime<=DateValue('" + ToDate+"')";
                }
                else
                {
                   SelectQuery = "select TicketNo , GrossWeight, TeirWeight, VehicleNo,Customer_Name,Supplier_Name,Item_name,trans,Bags,Customer_Name,lotNo,NetWeight,weightment_type,Trans_Type,Tran_Datetime,Weight,Charges,imagerefid from TransactionDetails";
                }

               
                using (OleDbCommand cmd = new OleDbCommand(SelectQuery, con))
                {
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.Text;
                    using (OleDbDataAdapter sda = new OleDbDataAdapter(cmd))
                    {
                        using (System.Data.DataTable dttable = new System.Data.DataTable())
                        {
                            sda.Fill(dttable);
                            //sda.Fill(ds);
                            ///  dgvActiveTransactions.DataSource = dt;
                            //dataGridView1.DataSource = dt;
                            foreach (DataRow row in dttable.Rows)
                            {
                                string TicketNumber;
                                string VehicleNumber;
                                string GrossWeight = "";
                                string transmode ="";
                                string Status="";
                                string TeirWeight = "";
                                string NetWeight = "";
                                string Bags = "";
                                string CustomerName = "";
                                string LotNo = "";
                                //foreach (var item in row.ItemArray)
                                //{
                                //    Console.WriteLine(item);

                                //}
                                TicketNumber = row.ItemArray[0] + "";
                                VehicleNumber = row.ItemArray[3] + "";
                                GrossWeight = row.ItemArray[1].ToString();
                                TeirWeight = row.ItemArray[2].ToString();
                                NetWeight = row.ItemArray[11].ToString();
                                Bags = row.ItemArray[8].ToString();
                                CustomerName = row.ItemArray[9] + "";
                                LotNo = row.ItemArray[10] + "";
                                if (row.ItemArray[7].ToString() == "1")
                                {
                                    transmode = "RECIEPT";
                                    Status = "Active";
                                }
                                if (row.ItemArray[7].ToString() == "2")
                                {
                                    transmode = "PDS";
                                    Status = "Active";
                                }
                                if (row.ItemArray[7].ToString() == "3")
                                {
                                    transmode = "WAGON";
                                    Status = "Active";
                                }

                                table.Rows.Add(TicketNumber, VehicleNumber, CustomerName, GrossWeight, TeirWeight, NetWeight, Bags, LotNo, transmode, Status);
                            }
                            dgvActiveTransactions.DataSource = table;
                            dgvActiveTransactions.Columns[0].Width  = 150;
                            dgvActiveTransactions.Columns[1].Width = 300;
                            dgvActiveTransactions.Columns[1].Width = 100;
                        }

                    }
                }
            }
        }

        private void lnkCompanyinfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //CompanyInfo fcmi = new CompanyInfo();
            //fcmi.Show();

        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            idloginform fm = new idloginform();
            this.Close();
            fm.Show();
        }
        public void LoadWeight()
        {
            try
            {
                // Set up the serial port
               
                port.Open();

                // Send the tare weight request command
                port.Write("T");

                // Read the response from the weighbridge
                string response = port.ReadLine();
                double number;
                double tareWeight;
                // Parse the tare weight from the response
                if (Double.TryParse(response, out number))
                    tareWeight = number;
                else
                    tareWeight = 0;



                // Use the tare weight
                lblAutoweight.Text = tareWeight.ToString();

                // Close the serial port
                port.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Some error occured while connecting with Waybridge,"+ex.Message);
               
            }
         
        }

        //private void pictureBox1_Click(object sender, EventArgs e)
        //{
        //    //LoadWeight();
        //    var baudrate = int.Parse(comboBox1.Text);

        //    SerialPort sp = new SerialPort();
        //    sp.PortName = "COM1";
        //    sp.BaudRate = baudrate;
        //    sp.Parity = Parity.None;
        //    sp.StopBits = StopBits.One;
        //    sp.RtsEnable = true;
        //    sp.DtrEnable = true;
        //    sp.Open();
        //    lblAutoweight.Text = sp.ReadLine().ToString();
        //    Regex digits = new Regex(@"^\D*?((-?(\d+(\.\d+)?))|(-?\.\d+)).*");
        //    Match mx = digits.Match(lblAutoweight.Text);
        //    decimal strValue1 = mx.Success ? Convert.ToDecimal(mx.Groups[1].Value) : 0;
        //    lblAutoweight.Text = strValue1.ToString();
        //    //label1.Text = strValue1.ToString();
        //    sp.Close();
        //}


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                var baudrate = int.Parse(comboBox1.Text);

                using (SerialPort sp = new SerialPort())
                {
                    sp.PortName = "COM1";
                    sp.BaudRate = baudrate;
                    sp.Parity = Parity.None;
                    sp.StopBits = StopBits.One;
                    sp.RtsEnable = true;
                    sp.DtrEnable = true;

                    sp.Open();
                    lblAutoweight.Text = sp.ReadLine().ToString();

                    Regex digits = new Regex(@"^\D*?((-?(\d+(\.\d+)?))|(-?\.\d+)).*");
                    Match mx = digits.Match(lblAutoweight.Text);
                    decimal strValue1 = mx.Success ? Convert.ToDecimal(mx.Groups[1].Value) : 0;
                    lblAutoweight.Text = strValue1.ToString();
                } // The SerialPort will be automatically closed when leaving the using block
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log the error, show a message to the user)
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(dateTimePicker1.Value.ToShortDateString() + "" + dateTimePicker2.Value.ToShortDateString());
            FillActiveReports(dateTimePicker1.Value.AddDays(-1).ToShortDateString(), dateTimePicker2.Value.AddDays(1).ToShortDateString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvActiveTransactions.Rows.Count > 0)
                {
                    Microsoft.Office.Interop.Excel.Application XcelApp = new Microsoft.Office.Interop.Excel.Application();
                    XcelApp.Application.Workbooks.Add(Type.Missing);
                    for (int i = 1; i < dgvActiveTransactions.Columns.Count + 1; i++)
                    {

                        XcelApp.Cells[1, i] = dgvActiveTransactions.Columns[i - 1].HeaderText;
                    }
                    for (int i = 0; i < dgvActiveTransactions.Rows.Count; i++)
                    {
                        for (int j = 0; j < dgvActiveTransactions.Columns.Count; j++)
                        {
                            if (dgvActiveTransactions.Rows[i].Cells[j].Value!=null)
                            {

                                XcelApp.Cells[i + 2, j + 1] = dgvActiveTransactions.Rows[i].Cells[j].Value.ToString();
                            }
                        }
                    }
                    XcelApp.Columns.AutoFit();
                    XcelApp.Visible = true;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("An error occured while =>" + ex.Message);
            }
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtVehicalOrTicket.Text))
                {
                    //
                    yes.Enabled = true;
                    no.Enabled = true;
                    using (OleDbConnection con = new OleDbConnection(connectionString))
                    {
                        String SelectQuery = "";
                        SelectQuery = "select * from TransactionDetails where VehicleNo='"+txtVehicalOrTicket.Text+"' or TicketNo='"+txtVehicalOrTicket.Text+"'";
                        
                        using (OleDbCommand cmd = new OleDbCommand(SelectQuery, con))
                        {
                            DataSet ds = new DataSet();
                            cmd.CommandType = CommandType.Text;
                            using (OleDbDataAdapter sda = new OleDbDataAdapter(cmd))
                            {
                                using (System.Data.DataTable dttable = new System.Data.DataTable())
                                {
                                    sda.Fill(dttable);
                                    if (dttable.Rows.Count > 0)
                                    {
                                        int trans_mode = 0;
                                        if (!string.IsNullOrEmpty(dttable.Rows[0]["Trans_Mode"].ToString()))
                                        {
                                            trans_mode = Convert.ToInt32(dttable.Rows[0]["Trans_Mode"].ToString());
                                        }
                                        if (trans_mode == 1)
                                        {
                                            rbtmsingle.Checked = true;
                                        }

                                        if (trans_mode == 2)
                                        {
                                            rbtmDouble.Checked = true;
                                        }
                                        if (trans_mode == 3)
                                        {
                                            rbtmMulti.Checked = true;
                                        }

                                        //-------------------
                                        int trans_type = 0;
                                        if (!string.IsNullOrEmpty(dttable.Rows[0]["trans"].ToString()))
                                        {
                                            trans_type = Convert.ToInt32(dttable.Rows[0]["trans"].ToString());
                                        }
                                        if (trans_type == 1)
                                        {
                                            receipt.Checked = true;
                                        }

                                        if (trans_type == 2)
                                        {
                                            pds.Checked = true;
                                        }
                                        if (trans_type == 3)
                                        {
                                            wagon.Checked = true;
                                        }

                                        //-------------------
                                        int weighment_type = 0;

                                        if (!string.IsNullOrEmpty(dttable.Rows[0]["weightment_type"].ToString()))
                                        {
                                            weighment_type = Convert.ToInt32(dttable.Rows[0]["weightment_type"].ToString());
                                        }

                                        if (weighment_type==1)
                                        {
                                           
                                            rbwtGross.Checked = true;
                                        }
                                        if (weighment_type == 2)
                                        {
                                            rbwtTare.Checked = true;
                                        }
                                        rbwtGross.Enabled = false;
                                        rbwtTare.Enabled = false;
                                        rbttIncoming.Enabled= false;
                                        rbttOutgoing.Enabled= false;
                                        receipt.Enabled = false;
                                        pds.Enabled = false;
                                        wagon.Enabled = false;

                                        //-------------------
                                        int transaction_type = 0;
                                        if (!string.IsNullOrEmpty(dttable.Rows[0]["Trans_Type"].ToString()))
                                        {
                                            transaction_type = Convert.ToInt32(dttable.Rows[0]["Trans_Type"].ToString());
                                        }
                                        if (transaction_type == 1)
                                        {
                                           
                                            rbttIncoming.Checked = true;
                                        }
                                        if (transaction_type == 2)
                                        {
                                            rbttOutgoing.Checked = true;
                                        }
                                        if (transaction_type == 3)
                                        {
                                            rbttboth.Checked = true;
                                        }

                                        txtVehicleNoT.Text = dttable.Rows[0]["VehicleNo"].ToString();
                                        txtItemNameT.Text= dttable.Rows[0]["Item_name"].ToString();
                                        txtSupplierNameT.Text= dttable.Rows[0]["Supplier_Name"].ToString();
                                        txtCustomerName2.Text= dttable.Rows[0]["Customer_Name"].ToString();
                                        txtChargesT.Text= dttable.Rows[0]["Charges"].ToString();
                                        lblTicketIDtext.Text = dttable.Rows[0]["TicketNo"].ToString();
                                        lblPkId.Text= dttable.Rows[0]["ID"].ToString();
                                        NoOfBags.Text = dttable.Rows[0]["Bags"].ToString();
                                        lotNo.Text = dttable.Rows[0]["LotNo"].ToString();

                                    }
                                }
                            }
                        }
                    }
                    //
                }
                else
                {
                    no.Checked = true;
                    yes.Checked = false;
                    yes.Enabled = false;
                    no.Enabled = false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured while your request=" + ex.Message);
            }
            
        }
        public string GetLastTicketIdFromDatabase() {
            string data = "";
            using (OleDbConnection con = new OleDbConnection(connectionString))
            {
                String SelectQuery = "";
                SelectQuery = "select top 1 * from TransactionDetails order by ID desc";
                //SelectQuery = "select * from TransactionDetails ORDER BY ID DESC LIMIT 1";

                using (OleDbCommand cmd = new OleDbCommand(SelectQuery, con))
                {
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.Text;
                    using (OleDbDataAdapter sda = new OleDbDataAdapter(cmd))
                    {
                        using (System.Data.DataTable dttable = new System.Data.DataTable())
                        {
                            sda.Fill(dttable);
                            if (dttable.Rows.Count > 0)
                            {
                                if (!string.IsNullOrEmpty(dttable.Rows[0]["TicketNo"].ToString()))
                                {
                                    data = dttable.Rows[0]["TicketNo"].ToString();
                                    Console.WriteLine(dttable.Rows[0]["CreatedOn"].ToString());
                                }
                                else
                                {
                                    data = "T00000000001";
                                }

                            }
                            else
                            {
                                data = "T00000000001";
                            }
                        }
                    }
                }
            }
            return data;
        }
        public string GenerateTicket()
        {
            // assume you have a method to retrieve the last generated ticket ID from the database
            string lastTicketId =GetLastTicketIdFromDatabase();
            Console.WriteLine(lastTicketId);

            // extract the first character and numeric part from the last ticket ID
            char lastChar = lastTicketId[0];
            int lastNumericPart = int.Parse(lastTicketId.Substring(1, 9));
            Console.WriteLine(lastNumericPart);

            // generate the next numeric part by adding 1 to the last numeric part
            int nextNumericPart = lastNumericPart + 1;
            Console.WriteLine(nextNumericPart);

            // format the next numeric part to have leading zeros up to 9 digits
            string nextNumericPartString = nextNumericPart.ToString("D9");

            // generate the next ticket ID by concatenating the first character and the formatted numeric part
            string nextTicketId = $"{lastChar}{nextNumericPartString}";
            lblTicketIDtext.Text = nextTicketId;
            Console.WriteLine(nextTicketId); // print the generated ticket ID
            return nextTicketId;
        }

        private void label23_Click(object sender, EventArgs e)
        {

        }

        public void GetCameraData()
        {
          
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //LoadWeight();

            SerialPort sp = new SerialPort();
            sp.PortName = "COM1";
            sp.BaudRate = 2400;
            sp.Parity = Parity.None;
            sp.StopBits = StopBits.One;
            sp.RtsEnable = true;
            sp.DtrEnable = true;
            sp.Open();
            lblAutoweight.Text = sp.ReadLine().ToString();
            Regex digits = new Regex(@"^\D*?((-?(\d+(\.\d+)?))|(-?\.\d+)).*");
            Match mx = digits.Match(lblAutoweight.Text);
            decimal strValue1 = mx.Success ? Convert.ToDecimal(mx.Groups[1].Value) : 0;
            lblAutoweight.Text = strValue1.ToString();
            //label1.Text = strValue1.ToString();
            sp.Close();
        }

        public void LIVEPre()
        {
            if (m_lUserID < 0)
            {
                MessageBox.Show("Please login the device firstly");
                return;
            }

            if (m_lRealHandle < 0)
            {
                CHCNetSDK.CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = RealPlayWnd.Handle;//Ô¤ÀÀ´°¿Ú
                lpPreviewInfo.lChannel = Int16.Parse(textBoxChannel.Text);//Ô¤teÀÀµÄÉè±¸Í¨µÀ
                lpPreviewInfo.dwStreamType = 0;//ÂëÁ÷ÀàÐÍ£º0-Ö÷ÂëÁ÷£¬1-×ÓÂëÁ÷£¬2-ÂëÁ÷3£¬3-ÂëÁ÷4£¬ÒÔ´ËÀàÍÆ
                lpPreviewInfo.dwLinkMode = 0;//Á¬½Ó·½Ê½£º0- TCP·½Ê½£¬1- UDP·½Ê½£¬2- ¶à²¥·½Ê½£¬3- RTP·½Ê½£¬4-RTP/RTSP£¬5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- ·Ç×èÈûÈ¡Á÷£¬1- ×èÈûÈ¡Á÷
                lpPreviewInfo.dwDisplayBufNum = 1; //²¥·Å¿â²¥·Å»º³åÇø×î´ó»º³åÖ¡Êý
                lpPreviewInfo.byProtoType = 0;
                lpPreviewInfo.byPreviewMode = 0;


                if (textBoxID.Text != "")
                {
                    lpPreviewInfo.lChannel = -1;
                    byte[] byStreamID = System.Text.Encoding.Default.GetBytes(textBoxID.Text);
                    lpPreviewInfo.byStreamID = new byte[32];
                    byStreamID.CopyTo(lpPreviewInfo.byStreamID, 0);
                }


                if (RealData == null)
                {
                    RealData = new CHCNetSDK.CHCNetSDK.REALDATACALLBACK(RealDataCallBack);//Ô¤ÀÀÊµÊ±Á÷»Øµ÷º¯Êý
                }

                IntPtr pUser = new IntPtr();//ÓÃ»§Êý¾Ý

                //´ò¿ªÔ¤ÀÀ Start live view 
                m_lRealHandle = CHCNetSDK.CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);
                if (m_lRealHandle < 0)
                {
                    iLastErr = CHCNetSDK.CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //Ô¤ÀÀÊ§°Ü£¬Êä³ö´íÎóºÅ
                    MessageBox.Show(str);
                    return;
                }
                else
                {
                }
            }
            else
            {
                //Í£Ö¹Ô¤ÀÀ Stop live view 
                if (!CHCNetSDK.CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
                {
                    iLastErr = CHCNetSDK.CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                m_lRealHandle = -1;
                
            }
            return;

        }

        public void CAPIMG()
        {
            string sJpegPicFileName;
            //Í¼Æ¬±£´æÂ·¾¶ºÍÎÄ¼þÃû the path and file name to save
            sJpegPicFileName = "JPEG_test.jpg";

            int lChannel = Int16.Parse(textBoxChannel.Text); //Í¨µÀºÅ Channel number

            CHCNetSDK.CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //Í¼ÏñÖÊÁ¿ Image quality
            lpJpegPara.wPicSize = 0xff; //×¥Í¼·Ö±æÂÊ Picture size: 2- 4CIF£¬0xff- Auto(Ê¹ÓÃµ±Ç°ÂëÁ÷·Ö±æÂÊ)£¬×¥Í¼·Ö±æÂÊÐèÒªÉè±¸Ö§³Ö£¬¸ü¶àÈ¡ÖµÇë²Î¿¼SDKÎÄµµ
            
            //JPEG×¥Í¼ Capture a JPEG picture
            if (!CHCNetSDK.CHCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserID, lChannel, ref lpJpegPara, sJpegPicFileName))
            {
                iLastErr = CHCNetSDK.CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                //MessageBox.Show(str);
                return;
            }
            else
            {
                str = "Successful to capture the JPEG file and the saved file is " + sJpegPicFileName;
                //MessageBox.Show(str);
            }
            return;

        }

        private void button7_Click(object sender, EventArgs e)
        {
            string DVRIPAddress = "192.168.29.13";
            string DVRIPAddress1 = "192.168.29.13";
            string DVRIPAddress2 = "192.168.29.13";
            Int16 DVRPortNumber = 8000;
            string DVRUserName = "admin";
            string DVRUserName1 = "admin";
            string DVRUserName2 = "admin";
            string DVRPassword = "Admin@123";
            string DVRPassword1 = "Admin@123";
            string DVRPassword2 = "Admin@123";



            OleDbConnection connT = new OleDbConnection(connectionString);
            OleDbCommand cmdT = new OleDbCommand();
            Guid imagerefID = Guid.NewGuid();
                String SelectQuery = "";
                SelectQuery = "select * from DeviceCofigCams";
                using (OleDbCommand cmd = new OleDbCommand(SelectQuery, connT))
                {
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.Text;
                    using (OleDbDataAdapter sda = new OleDbDataAdapter(cmd))
                    {
                        using (System.Data.DataTable dttable = new System.Data.DataTable())
                        {
                            sda.Fill(dttable);
                            if (dttable.Rows.Count > 0)
                            {
                                DVRIPAddress = dttable.Rows[0]["IPNumber"].ToString();
                                DVRIPAddress1 = dttable.Rows[1]["IPNumber"].ToString();
                                DVRIPAddress2 = dttable.Rows[2]["IPNumber"].ToString();

                                DVRUserName = dttable.Rows[0]["username"].ToString();
                                DVRUserName1 = dttable.Rows[1]["username"].ToString();
                                DVRUserName2 = dttable.Rows[2]["username"].ToString();

                                DVRPassword = dttable.Rows[0]["password"].ToString();
                                DVRPassword1 = dttable.Rows[1]["password"].ToString();
                                DVRPassword2 = dttable.Rows[2]["password"].ToString();
                            }
                        }
                    }
                }

            


            CHCNetSDK.CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.CHCNetSDK.NET_DVR_DEVICEINFO_V30();

            //µÇÂ¼Éè±¸ Login the device
            m_lUserID = CHCNetSDK.CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
            if (m_lUserID < 0)
            {
                iLastErr = CHCNetSDK.CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //µÇÂ¼Ê§°Ü£¬Êä³ö´íÎóºÅ
                MessageBox.Show(str);
                return;
            }
            else
            {
                //µÇÂ¼³É¹¦
                MessageBox.Show("Login Success!");
                
            }
            m_lUserID3 = CHCNetSDK.CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress1, DVRPortNumber, DVRUserName1, DVRPassword1, ref DeviceInfo);
            if (m_lUserID3 < 0)
            {
                iLastErr = CHCNetSDK.CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //µÇÂ¼Ê§°Ü£¬Êä³ö´íÎóºÅ
                MessageBox.Show(str);
                return;
            }
            else
            {
                //µÇÂ¼³É¹¦
                MessageBox.Show("Login Success!");

            }
            m_lUserID2 = CHCNetSDK.CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress2, DVRPortNumber, DVRUserName2, DVRPassword2, ref DeviceInfo);
            if (m_lUserID2 < 0)
            {
                iLastErr1 = CHCNetSDK.CHCNetSDK.NET_DVR_GetLastError();
                str1 = "NET_DVR_Login_V30 failed, error code= " + iLastErr1; //µÇÂ¼Ê§°Ü£¬Êä³ö´íÎóºÅ
                MessageBox.Show(str1);
                return;
            }
            else
            {
                //µÇÂ¼³É¹¦
                MessageBox.Show("Login Success!");

            }
            LIVEPre();
            LIVEPre1();
            LIVEPre2();
            CAPIMG();
            //Guid imagerefID = Guid.NewGuid();
            //SaveAllCamImages(imagerefID);
        }

        public void LIVEPre1()
        {
            if (m_lUserID2 < 0)
            {
                MessageBox.Show("Please login the device firstly");
                return;
            }

            if (m_lRealHandle2 < 0)
            {
                CHCNetSDK.CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo1 = new CHCNetSDK.CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo1.hPlayWnd = RealPlayWnd2.Handle;//Ô¤ÀÀ´°¿Ú
                lpPreviewInfo1.lChannel = Int16.Parse(textBoxChannel.Text);//Ô¤teÀÀµÄÉè±¸Í¨µÀ
                lpPreviewInfo1.dwStreamType = 0;//ÂëÁ÷ÀàÐÍ£º0-Ö÷ÂëÁ÷£¬1-×ÓÂëÁ÷£¬2-ÂëÁ÷3£¬3-ÂëÁ÷4£¬ÒÔ´ËÀàÍÆ
                lpPreviewInfo1.dwLinkMode = 0;//Á¬½Ó·½Ê½£º0- TCP·½Ê½£¬1- UDP·½Ê½£¬2- ¶à²¥·½Ê½£¬3- RTP·½Ê½£¬4-RTP/RTSP£¬5-RSTP/HTTP 
                lpPreviewInfo1.bBlocked = true; //0- ·Ç×èÈûÈ¡Á÷£¬1- ×èÈûÈ¡Á÷
                lpPreviewInfo1.dwDisplayBufNum = 1; //²¥·Å¿â²¥·Å»º³åÇø×î´ó»º³åÖ¡Êý
                lpPreviewInfo1.byProtoType = 0;
                lpPreviewInfo1.byPreviewMode = 0;


                if (textBoxID.Text != "")
                {
                    lpPreviewInfo1.lChannel = -1;
                    byte[] byStreamID = System.Text.Encoding.Default.GetBytes(textBoxID.Text);
                    lpPreviewInfo1.byStreamID = new byte[32];
                    byStreamID.CopyTo(lpPreviewInfo1.byStreamID, 0);
                }


                if (RealData1 == null)
                {
                    RealData1 = new CHCNetSDK.CHCNetSDK.REALDATACALLBACK(RealDataCallBack1);//Ô¤ÀÀÊµÊ±Á÷»Øµ÷º¯Êý
                }

                IntPtr pUser = new IntPtr();//ÓÃ»§Êý¾Ý

                //´ò¿ªÔ¤ÀÀ Start live view 
                m_lRealHandle2 = CHCNetSDK.CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo1, null/*RealData*/, pUser);
                if (m_lRealHandle2 < 0)
                {
                    iLastErr1 = CHCNetSDK.CHCNetSDK.NET_DVR_GetLastError();
                    str1 = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr1; // str1 / Ô¤ÀÀÊ§°Ü£¬Êä³ö´íÎóºÅ
                    MessageBox.Show(str1);
                    return;
                }
                else
                {
                }
            }
            else
            {
                //Í£Ö¹Ô¤ÀÀ Stop live view 
                if (!CHCNetSDK.CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle2))
                {
                    iLastErr1 = CHCNetSDK.CHCNetSDK.NET_DVR_GetLastError();
                    str1 = "NET_DVR_StopRealPlay failed, error code= " + iLastErr1;
                    MessageBox.Show(str1);
                    return;
                }
                m_lRealHandle2 = -1;

            }
            return;

        }

        public void LIVEPre2()
        {
            if (m_lUserID3 < 0)
            {
                MessageBox.Show("Please login the device firstly");
                return;
            }

            if (m_lRealHandle3 < 0)
            {
                CHCNetSDK.CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo2 = new CHCNetSDK.CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo2.hPlayWnd = RealPlayWnd2.Handle;//Ô¤ÀÀ´°¿Ú
                lpPreviewInfo2.lChannel = Int16.Parse(textBoxChannel.Text);//Ô¤teÀÀµÄÉè±¸Í¨µÀ
                lpPreviewInfo2.dwStreamType = 0;//ÂëÁ÷ÀàÐÍ£º0-Ö÷ÂëÁ÷£¬1-×ÓÂëÁ÷£¬2-ÂëÁ÷3£¬3-ÂëÁ÷4£¬ÒÔ´ËÀàÍÆ
                lpPreviewInfo2.dwLinkMode = 0;//Á¬½Ó·½Ê½£º0- TCP·½Ê½£¬1- UDP·½Ê½£¬2- ¶à²¥·½Ê½£¬3- RTP·½Ê½£¬4-RTP/RTSP£¬5-RSTP/HTTP 
                lpPreviewInfo2.bBlocked = true; //0- ·Ç×èÈûÈ¡Á÷£¬1- ×èÈûÈ¡Á÷
                lpPreviewInfo2.dwDisplayBufNum = 1; //²¥·Å¿â²¥·Å»º³åÇø×î´ó»º³åÖ¡Êý
                lpPreviewInfo2.byProtoType = 0;
                lpPreviewInfo2.byPreviewMode = 0;


                if (textBoxID.Text != "")
                {
                    lpPreviewInfo2.lChannel = -1;
                    byte[] byStreamID = System.Text.Encoding.Default.GetBytes(textBoxID.Text);
                    lpPreviewInfo2.byStreamID = new byte[32];
                    byStreamID.CopyTo(lpPreviewInfo2.byStreamID, 0);
                }


                if (RealData1 == null)
                {
                    RealData1 = new CHCNetSDK.CHCNetSDK.REALDATACALLBACK(RealDataCallBack1);//Ô¤ÀÀÊµÊ±Á÷»Øµ÷º¯Êý
                }

                IntPtr pUser = new IntPtr();//ÓÃ»§Êý¾Ý

                //´ò¿ªÔ¤ÀÀ Start live view 
                m_lRealHandle3 = CHCNetSDK.CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo2, null/*RealData*/, pUser);
                if (m_lRealHandle3 < 0)
                {
                    iLastErr1 = CHCNetSDK.CHCNetSDK.NET_DVR_GetLastError();
                    str1 = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr1; // str1 / Ô¤ÀÀÊ§°Ü£¬Êä³ö´íÎóºÅ
                    MessageBox.Show(str1);
                    return;
                }
                else
                {
                }
            }
            else
            {
                //Í£Ö¹Ô¤ÀÀ Stop live view 
                if (!CHCNetSDK.CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle3))
                {
                    iLastErr1 = CHCNetSDK.CHCNetSDK.NET_DVR_GetLastError();
                    str1 = "NET_DVR_StopRealPlay failed, error code= " + iLastErr1;
                    MessageBox.Show(str1);
                    return;
                }
                m_lRealHandle3 = -1;

            }
            return;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void RealPlayWnd2_Click(object sender, EventArgs e)
        {

        }

        private void RealPlayWnd3_Click(object sender, EventArgs e)
        {

        }

        private void rbtmDouble_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (yes.Checked == true)
            {
                rbttboth.Enabled = false;
                rbtmsingle.Enabled = false;
                rbtmDouble.Enabled = false;
                rbtmMulti.Enabled = false;
                txtVehicleNoT.Enabled = false;
                txtItemNameT.Enabled = false;
                txtSupplierNameT.Enabled = false;
                txtCustomerName2.Enabled = false;
                txtChargesT.Enabled = false;
                NoOfBags.Enabled = false;
                lotNo.Enabled = false;
            }
        }

        private void label26_Click(object sender, EventArgs e)
        {
            
        }

        private void no_CheckedChanged(object sender, EventArgs e)
        {
            if (no.Checked == true)
            {
                rbttboth.Enabled = true;
                rbtmsingle.Enabled = true;
                rbtmMulti.Enabled = true;
                rbtmDouble.Enabled = true;
                txtVehicleNoT.Enabled = true;
                txtItemNameT.Enabled = true;
                txtSupplierNameT.Enabled = true;
                txtCustomerName2.Enabled = true;
                txtChargesT.Enabled = true;
                NoOfBags.Enabled = true;
                lotNo.Enabled = true;
            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void lblTicketIDtext_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void dgvActiveTransactions_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        public void RealDataCallBack1(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
            if (dwBufSize > 0)
            {
                byte[] sData = new byte[dwBufSize];
                Marshal.Copy(pBuffer, sData, 0, (Int32)dwBufSize);

                string str1 = "ÊµÊ±Á÷Êý¾Ý.ps";
                FileStream fs = new FileStream(str1, FileMode.Create);
                int iLen = (int)dwBufSize;
                fs.Write(sData, 0, iLen);
                fs.Close();
            }
        }


    }
}
