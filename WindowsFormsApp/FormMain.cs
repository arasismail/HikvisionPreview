using Business.Conrete;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using Entities.Concrete;
using NVRCsharpDemo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class FormMain : Form
    {
        private bool m_bInitSDK = false;
        private bool m_bRecord = false;
        private uint iLastErr = 0;
        private Int32 m_lUserID = -1;
        private Int32 m_lRealHandle = -1;
        private string str1;
        private string str2;
        private Int32 i = 0;
        private Int32 m_lTree = 0;
        private string str;
        private long iSelIndex = 0;
        private uint dwAChanTotalNum = 0;
        private uint dwDChanTotalNum = 0;
        private Int32 m_lPort = -1;
        private IntPtr m_ptrRealHandle;
        private int[] iIPDevID = new int[96];
        private int[] iChannelNum = new int[96];

        private CHCNetSDK.REALDATACALLBACK RealData = null;
        public CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo;
        public CHCNetSDK.NET_DVR_IPPARACFG_V40 m_struIpParaCfgV40;
        public CHCNetSDK.NET_DVR_STREAM_MODE m_struStreamMode;
        public CHCNetSDK.NET_DVR_IPCHANINFO m_struChanInfo;
        public CHCNetSDK.NET_DVR_IPCHANINFO_V40 m_struChanInfoV40;
        private PlayCtrl.DECCBFUN m_fDisplayFun = null;
        public delegate void MyDebugInfo(string str);

        UserManager _userManager = new UserManager();
        public FormMain()
        {
            InitializeComponent();
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            else
            {
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);



                for (int i = 0; i < 64; i++)
                {
                    iIPDevID[i] = -1;
                    iChannelNum[i] = -1;
                }
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            TreeListLoad();
        }

        private void TreeListLoad()
        {
            treeList1.KeyFieldName = "Id";
            treeList1.OptionsBehavior.PopulateServiceColumns = true;
            treeList1.DataSource = UserDataGenerator.CreateData();
            treeList1.RowHeight = 25;
            TreeListColumn colUserName = treeList1.Columns["UserName"];
            TreeListColumn colPassword = treeList1.Columns["Password"];
            TreeListColumn colIPAddress = treeList1.Columns["IPAddress"];
            TreeListColumn colPortNumber = treeList1.Columns["PortNumber"];
            treeList1.Columns["Password"].Visible = false;
            treeList1.Columns[treeList1.KeyFieldName].Visible = false;
            colUserName.OptionsColumn.ReadOnly = true;
            colIPAddress.OptionsColumn.ReadOnly = true;
            colPortNumber.OptionsColumn.ReadOnly = true;
            colPassword.OptionsColumn.ReadOnly = true;
        }
        private void treeList1_MouseClick(object sender, MouseEventArgs e)
        {
            var nodes = treeList1.Selection;
            foreach (TreeListNode node in nodes)
            {
                textBoxUserName.Text = node.GetValue("UserName").ToString();
                textBoxPassword.Text = node.GetValue("Password").ToString();
                textBoxIP.Text = node.GetValue("IPAddress").ToString();
                textBoxPort.Text = node.GetValue("PortNumber").ToString();

            }

        }
        private void UserSave()
        {
            var userManager = new UserManager();
            if (userManager.GetByUserName(textBoxUserName.Text) == null)
            {
                _userManager.Add(new User() { UserName = textBoxUserName.Text, Password = textBoxPassword.Text, IPAddress = textBoxIP.Text, PortNumber = Int16.Parse(textBoxPort.Text) });
                TreeListLoad();
            }

        }

        public void DebugInfo(string str)
        {
            if (str.Length > 0)
            {
                str += "\n";
                TextBoxInfo.AppendText(str);
            }
        }


        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (m_lUserID < 0)
            {
                string DVRIPAddress = textBoxIP.Text;
                Int16 DVRPortNumber = Int16.Parse(textBoxPort.Text);
                string DVRUserName = textBoxUserName.Text;
                string DVRPassword = textBoxPassword.Text;

                m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Login_V30 failed, error code= " + iLastErr;
                    DebugInfo(str);
                    return;
                }
                else
                {
                    UserSave();


                    DebugInfo("NET_DVR_Login_V30 succ!");
                    btnLogin.Text = "Logout";

                    dwAChanTotalNum = (uint)DeviceInfo.byChanNum;
                    dwDChanTotalNum = (uint)DeviceInfo.byIPChanNum + 256 * (uint)DeviceInfo.byHighDChanNum;

                }

            }
            else
            {

                if (m_lRealHandle >= 0)
                {
                    DebugInfo("Please stop live view firstly");
                    return;
                }

                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Logout failed, error code= " + iLastErr;
                    DebugInfo(str);
                    return;
                }
                DebugInfo("NET_DVR_Logout succ!");

                m_lUserID = -1;
                btnLogin.Text = "Login";
            }
            return;
        }

        public void InfoIPChannel()
        {
            uint dwSize = (uint)Marshal.SizeOf(m_struIpParaCfgV40);

            IntPtr ptrIpParaCfgV40 = Marshal.AllocHGlobal((Int32)dwSize);
            Marshal.StructureToPtr(m_struIpParaCfgV40, ptrIpParaCfgV40, false);

            uint dwReturn = 0;
            int iGroupNo = 0;

            if (!CHCNetSDK.NET_DVR_GetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, dwSize, ref dwReturn))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_GET_IPPARACFG_V40 failed, error code= " + iLastErr;

                DebugInfo(str);
            }
            else
            {
                DebugInfo("NET_DVR_GET_IPPARACFG_V40 succ!");

                m_struIpParaCfgV40 = (CHCNetSDK.NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIpParaCfgV40, typeof(CHCNetSDK.NET_DVR_IPPARACFG_V40));



                byte byStreamType = 0;
                uint iDChanNum = 64;

                if (dwDChanTotalNum < 64)
                {
                    iDChanNum = dwDChanTotalNum;
                }

                for (i = 0; i < iDChanNum; i++)
                {
                    iChannelNum[i + dwAChanTotalNum] = i + (int)m_struIpParaCfgV40.dwStartDChan;
                    byStreamType = m_struIpParaCfgV40.struStreamMode[i].byGetStreamType;

                    dwSize = (uint)Marshal.SizeOf(m_struIpParaCfgV40.struStreamMode[i].uGetStream);
                    switch (byStreamType)
                    {

                        case 0:
                            IntPtr ptrChanInfo = Marshal.AllocHGlobal((Int32)dwSize);
                            Marshal.StructureToPtr(m_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfo, false);
                            m_struChanInfo = (CHCNetSDK.NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(CHCNetSDK.NET_DVR_IPCHANINFO));



                            iIPDevID[i] = m_struChanInfo.byIPID + m_struChanInfo.byIPIDHigh * 256 - iGroupNo * 64 - 1;

                            Marshal.FreeHGlobal(ptrChanInfo);
                            break;
                        case 6:
                            IntPtr ptrChanInfoV40 = Marshal.AllocHGlobal((Int32)dwSize);
                            Marshal.StructureToPtr(m_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfoV40, false);
                            m_struChanInfoV40 = (CHCNetSDK.NET_DVR_IPCHANINFO_V40)Marshal.PtrToStructure(ptrChanInfoV40, typeof(CHCNetSDK.NET_DVR_IPCHANINFO_V40));



                            iIPDevID[i] = m_struChanInfoV40.wIPID - iGroupNo * 64 - 1;

                            Marshal.FreeHGlobal(ptrChanInfoV40);
                            break;
                        default:
                            break;
                    }
                }
            }
            Marshal.FreeHGlobal(ptrIpParaCfgV40);

        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (m_lUserID < 0)
            {
                MessageBox.Show("Please login the device firstly!");
                return;
            }

            if (m_bRecord)
            {
                MessageBox.Show("Please stop recording firstly!");
                return;
            }

            if (m_lRealHandle < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = RealPlayWnd.Handle;
                lpPreviewInfo.lChannel = iChannelNum[(int)iSelIndex];
                lpPreviewInfo.dwStreamType = 0;
                lpPreviewInfo.dwLinkMode = 0;
                lpPreviewInfo.bBlocked = true;
                lpPreviewInfo.dwDisplayBufNum = 15;

                IntPtr pUser = IntPtr.Zero;



                m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);



                if (m_lRealHandle < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr;
                    DebugInfo(str);
                    return;
                }
                else
                {

                    DebugInfo("NET_DVR_RealPlay_V40 succ!");
                    btnPreview.Text = "Stop View";
                }
            }
            else
            {

                if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                    DebugInfo(str);
                    return;
                }



                DebugInfo("NET_DVR_StopRealPlay succ!");
                m_lRealHandle = -1;
                btnPreview.Text = "Live View";
                RealPlayWnd.Invalidate();
            }
            return;
        }

        private void btnBMP_Click(object sender, EventArgs e)
        {
            if (m_lRealHandle < 0)
            {
                DebugInfo("Please start live view firstly!");
                return;
            }

            string sBmpPicFileName;

            sBmpPicFileName = "test.bmp";



            if (!CHCNetSDK.NET_DVR_CapturePicture(m_lRealHandle, sBmpPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CapturePicture failed, error code= " + iLastErr;
                DebugInfo(str);
                return;
            }
            else
            {
                str = "NET_DVR_CapturePicture succ and the saved file is " + sBmpPicFileName;
                DebugInfo(str);
            }


            return;
        }

        private void btnJPEG_Click(object sender, EventArgs e)
        {
            int lChannel = iChannelNum[(int)iSelIndex];

            CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0;
            lpJpegPara.wPicSize = 0xff;



            string sJpegPicFileName;
            sJpegPicFileName = "filetest.jpg";

            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserID, lChannel, ref lpJpegPara, sJpegPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                DebugInfo(str);
                return;
            }
            else
            {
                str = "NET_DVR_CaptureJPEGPicture succ and the saved file is " + sJpegPicFileName;
                DebugInfo(str);
            }


            uint iBuffSize = 400000;
            byte[] byJpegPicBuffer = new byte[iBuffSize];
            uint dwSizeReturned = 0;

            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture_NEW(m_lUserID, lChannel, ref lpJpegPara, byJpegPicBuffer, iBuffSize, ref dwSizeReturned))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture_NEW failed, error code= " + iLastErr;
                DebugInfo(str);
                return;
            }
            else
            {

                string str = "buffertest.jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)dwSizeReturned;
                fs.Write(byJpegPicBuffer, 0, iLen);
                fs.Close();

                str = "NET_DVR_CaptureJPEGPicture_NEW succ and save the data in buffer to 'buffertest.jpg'.";
                DebugInfo(str);
            }

            return;
        }


        private void btn_Exit_Click(object sender, EventArgs e)
        {
            if (m_lRealHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
                m_lRealHandle = -1;
            }

            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
                m_lUserID = -1;
            }

            CHCNetSDK.NET_DVR_Cleanup();

            Application.Exit();
        }


    }
}
