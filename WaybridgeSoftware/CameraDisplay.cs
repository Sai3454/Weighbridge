using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HikVision;


namespace WaybridgeSoftware
{
    public partial class CameraDisplay : Form
    {
        private int _userId;
        private int _realPlayHandle;
        private bool _isPlaying;
        public CameraDisplay()
        {
            InitializeComponent();
            // Initialize the SDK
        }

        private void CameraDisplay_Load(object sender, EventArgs e)
        {
            // Connect to the camera using its IP address and login credentials
            string ipAddress = "192.168.1.13"; // Replace with your camera's IP address
            string username = "admin"; // Replace with your camera's username
            string password = "Admin@13"; // Replace with your camera's password
            Int16 port = 8000; // Replace with your camera's port number

            // Initialize the SDK
            Hikvision.DeviceNetwork.CHCNetSDK.NET_DVR_Init();

            // Login to the camera
            var loginInfo = new Hikvision.DeviceNetwork.CHCNetSDK.NET_DVR_DEVICEINFO_V30();
            _userId = Hikvision.DeviceNetwork.CHCNetSDK.NET_DVR_Login_V30(ipAddress, port, username, password, ref loginInfo);

            if (_userId == 0)
            {
                MessageBox.Show("Failed to login to camera.");
                return;
            }

            // Start real-time video streaming
            var playInfo = new Hikvision.DeviceNetwork.CHCNetSDK.NET_DVR_CLIENTINFO();
            playInfo.lChannel = 1; // channel number
            playInfo.hPlayWnd = pictureBox1.Handle; // handle to the PictureBox control
            _realPlayHandle = Hikvision.DeviceNetwork.CHCNetSDK.NET_DVR_RealPlay_V30(_userId, ref playInfo, null, IntPtr.Zero, 1);

            if (_realPlayHandle == 0)
            {
                MessageBox.Show("Failed to start real-time video streaming.");
                return;
            }

            _isPlaying = true;

            // Start a separate thread to continuously update the PictureBox control with the video frames
            new Thread(() =>
            {
                while (_isPlaying)
                {
                    var bmp = GetBitmapFromVideoStream();
                    if (bmp != null)
                    {
                        pictureBox1.Invoke(new Action(() =>
                        {
                            pictureBox1.Image = bmp;
                        }));
                    }

                    Thread.Sleep(50);
                }
            }).Start();
        }
        //
        private Bitmap GetBitmapFromVideoStream()
        {
            var frameData = new byte[640 * 480 * 3]; // assuming 640x480 resolution and RGB24 format
            var frameSize = Marshal.SizeOf(frameData);

            //uint bytesReturned;
            //if (Hikvision.DeviceNetwork.CHCNetSDK.NET_DVR_GetRealPlayerIndex(_realPlayHandle, frameData, (uint)frameSize, out bytesReturned)!=0)
            //{
            //    return null;
            //}

            var bmp = new Bitmap(640, 480, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
            Marshal.Copy(frameData, 0, bmpData.Scan0, frameSize);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        //


    }
}
