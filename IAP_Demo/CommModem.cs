using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Collections;
using System.Threading;
using System.Timers;
using System.IO;
using SysTimer = System.Timers.Timer;
using FileConvert;
using System.Runtime.InteropServices;


    namespace IAP_Demo
{
    public partial class CommModem : Form
    {
        public delegate void CommModemCallback(Byte[] frameData);
        public delegate void CommProgressDelegate(int total, int currVal);
        public delegate void CommThreadFinishDelegate(en_thread_number_t threadNum, en_trans_status_t threadSta, en_packet_cmd_t threadCmd);

        string m_downloadFileName = string.Empty;
        string m_uploadFileName = string.Empty;
        volatile bool m_transThreadFlag = false;    // Transmit thread running flag
        volatile en_trans_status_t m_transStatus;    // Transmit status trace
        Byte m_transNumber;     // Frame number
        Byte[] m_commRecvData = new Byte[(int)en_frame_para_t.FRAME_MAX_SIZE];
        UInt32 m_appFlashAddr = 0;
        SysTimer m_commTimer;   // Communication timeout timer
        int m_commPortNum;      // Total number of serial ports
        SerialPort m_commPort;  // Current serial port


        public enum en_thread_number_t
        {
            TransFileThead  = 0x01,
            RecvFileThead   = 0x02,
        }

        public enum en_trans_status_t
        {
            TransIdle           = 0x00,
            TransBegin          = 0x01,
            TransTimeout        = 0x02,
            TransFinished       = 0x03,
            TransFailed         = 0x04,
            TransAbort          = 0x05,
            TransAddrError      = 0x06,
            TransFileInvalid    = 0x07,
        }

        public enum en_packet_type_t
        {
            PACKET_TYPE_CONTROL     = 0x11,
            PACKET_TYPE_DATA        = 0x12,
        }

        public enum en_packet_cmd_t
        {
            PACKET_CMD_HANDSHAKE    = 0x20,
            PACKET_CMD_JUMP_TO_APP  = 0x21,
            PACKET_CMD_APP_DOWNLOAD = 0x22,
            PACKET_CMD_APP_UPLOAD   = 0x23,
            PACKET_CMD_ERASE_FLASH  = 0x24,
            PACKET_CMD_FLASH_CRC    = 0x25,
            PACKET_CMD_APP_UPGRADE  = 0x26,
        }

        public enum en_packet_status_t
        {
            PACKET_ACK_OK           = 0x00,
            PACKET_ACK_ERROR        = 0x01,
            PACKET_ACK_ABORT        = 0x02,
            PACKET_ACK_TIMEOUT      = 0x03,
            PACKET_ACK_ADDR_ERROR   = 0x04,
        }

        public enum en_frame_para_t
        {
            FRAME_HEAD              = 0xAC6D,
            FRAME_SHELL_SIZE        = 8,
            FRAME_NUM_XOR_BYTE      = 0xFF,
            FRAME_MAX_SIZE          = (FRAME_SHELL_SIZE + en_packet_para_t.PACKET_MAX_SIZE),

            FRAME_HEAD_INDEX        = 0x00,
            FRAME_NUM_INDEX         = 0x02,
            FRAME_XORNUM_INDEX      = 0x03,
            FRAME_LENGTH_INDEX      = 0x04,
            FRAME_PACKET_INDEX      = 0x06,
        }

        public enum en_packet_para_t
        {
            PACKET_INSTRUCT_SIZE    = 10,
            PACKET_DATA_SIZE        = 512,          //包大小
            PACKET_MIN_SIZE         = PACKET_INSTRUCT_SIZE,
            PACKET_MAX_SIZE         = (PACKET_INSTRUCT_SIZE + PACKET_DATA_SIZE),

            PACKET_CMD_INDEX        = (en_frame_para_t.FRAME_PACKET_INDEX + 0x00),
            PACKET_TYPE_INDEX       = (en_frame_para_t.FRAME_PACKET_INDEX + 0x01),
            PACKET_RESULT_INDEX     = (en_frame_para_t.FRAME_PACKET_INDEX + 0x01),
            PACKET_ADDRESS_INDEX    = (en_frame_para_t.FRAME_PACKET_INDEX + 0x02),
            PACKET_DATA_INDEX       = (en_frame_para_t.FRAME_PACKET_INDEX + PACKET_INSTRUCT_SIZE),
        }


        public CommModem()
        {
            InitializeComponent();
        }


        public static void Delay(int milliSecond)
        {
            int start = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - start) < milliSecond)
            {
                Application.DoEvents();
            }
        }


        private void UartDisconnectHandle()
        {
            m_transThreadFlag = false;
            m_transStatus = en_trans_status_t.TransAbort;
            m_transNumber = 1;
            // Default
            btnWriteInfo.Enabled = false;
            btnReadInfo.Enabled = false;
            cboPorts.Enabled = true;
            cboBaudRate.Enabled = true;
            btnOpen.Text = "连接";
            staStripUpdateInfo(null);
        }
        
        private void CommModem_Load(object sender, EventArgs e)
        { 
            int[] defBps = { 9600, 57600, 115200, 256000, 512000, 921600, 1228800};
            List<int> listBps = new List<int>(defBps);
            foreach (int tmp in listBps)
            {
                cboBaudRate.Items.Add(tmp);
            }
            txtDestAddress.Text = "0x1000";
            txtFilePath.Text = "";
            cboBaudRate.SelectedIndex = 2;
            //get serial port
            cboPorts_DropDown(sender, e);

            // Load configure file
            string exeRootDir = Directory.GetCurrentDirectory();
            string iniFilePath = exeRootDir + "\\IapConfig.ini";
            if (File.Exists(iniFilePath))
            {
                IniFileHelper iapConfig = new IniFileHelper(iniFilePath);
                string comPort = iapConfig.ReadValue("GENERAL", "ComPort");
                if ((comPort != string.Empty) && (comPort.Trim() != ""))
                {
                    string[] ports = SerialPort.GetPortNames();
                    if (ports.Length != 0)
                    {
                        Array.Sort(ports, new CustomComparer());
                        if (true == ((IList)ports).Contains(comPort))
                        {
                            cboPorts.SelectedIndex = Array.IndexOf(ports, comPort);
                        }
                    }
                }
                string destAddr = iapConfig.ReadValue("GENERAL", "DestAddress");
                if ((destAddr != string.Empty) && (destAddr.Trim() != ""))
                {
                    txtDestAddress.Text = destAddr;
                }
                string baudRate = iapConfig.ReadValue("GENERAL", "BaudRate");
                if ((baudRate != string.Empty) && (baudRate.Trim() != ""))
                {
                    int intBps = int.Parse(baudRate);
                    int firstIndex = Array.IndexOf(defBps, intBps);
                    if (firstIndex >= 0)
                    {
                        cboBaudRate.SelectedIndex = firstIndex;
                    }
                    else
                    {
                        cboBaudRate.Items.Clear();
                        listBps.Add(intBps);
                        listBps.Sort();
                        foreach (int tmp in listBps)
                        {
                            cboBaudRate.Items.Add(tmp);
                        }
                        cboBaudRate.SelectedItem = intBps;
                    }
                }
                string filePath = iapConfig.ReadValue("GENERAL", "FilePath");
                if ((filePath != string.Empty) && (filePath.Trim() != ""))
                {
                    txtFilePath.Text = filePath;
                }
            }

            m_commPortNum = 0;
            m_transNumber = 1;
            m_transStatus = en_trans_status_t.TransIdle;
            tmrPortCheck.Enabled = true;
            m_commPort = new SerialPort();
            m_commTimer = new SysTimer();
            UartDisconnectHandle();
            InitCommTimer();
        }

        private void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int cpIndex = 0;
            int recvDelay, readCnt;

            int bufferSize;
            int frameMaxLen = (int)en_frame_para_t.FRAME_MAX_SIZE;
            Byte[] ReDatas = new Byte[frameMaxLen];
            Byte[] ReceiveData = new Byte[frameMaxLen];
            int dataLength = 0, frameHead = 0;
            UInt16 crc16;

            bufferSize = m_commPort.BytesToRead;
            if (bufferSize > frameMaxLen)
            {
                m_commPort.Read(ReDatas, 0, frameMaxLen);
                return;         //丢弃
            }

            recvDelay = 5;
            while (recvDelay > 0)
            {
                if (bufferSize > 0)
                { readCnt = m_commPort.Read(ReDatas, 0, bufferSize); }
                else
                { readCnt = 0; }

                if (readCnt > 0)
                {
                    if ((bufferSize + cpIndex) > frameMaxLen)
                    { return; }        //丢弃

                    Buffer.BlockCopy(ReDatas, 0, ReceiveData, cpIndex, bufferSize);
                    cpIndex += bufferSize;
                    recvDelay = 5;
                }
                else
                {
                    recvDelay--;
                    Thread.Sleep(1);
                }
                bufferSize = m_commPort.BytesToRead;
            }

            if (cpIndex != 0)            //empty
            {
                //packet
                frameHead = ReceiveData[(int)en_frame_para_t.FRAME_HEAD_INDEX] +
                            (ReceiveData[(int)en_frame_para_t.FRAME_HEAD_INDEX + 1] << 8);
                if (en_frame_para_t.FRAME_HEAD == (en_frame_para_t)frameHead)
                {
                    if (ReceiveData[(int)en_frame_para_t.FRAME_NUM_INDEX] ==
                            (ReceiveData[(int)en_frame_para_t.FRAME_XORNUM_INDEX] ^ (Byte)en_frame_para_t.FRAME_NUM_XOR_BYTE))
                    {
                        dataLength = ReceiveData[(int)en_frame_para_t.FRAME_LENGTH_INDEX] +
                                     (ReceiveData[(int)en_frame_para_t.FRAME_LENGTH_INDEX + 1] << 8);
                        if ((dataLength >= (int)en_packet_para_t.PACKET_MIN_SIZE) && (dataLength <= (int)en_packet_para_t.PACKET_MAX_SIZE))
                        {
                            crc16 = (UInt16)(ReceiveData[(int)en_frame_para_t.FRAME_PACKET_INDEX + dataLength] +
                                             (ReceiveData[(int)en_frame_para_t.FRAME_PACKET_INDEX + dataLength + 1] << 8));
                            if (crc16 == Cal_CRC16(ReceiveData, (int)en_frame_para_t.FRAME_PACKET_INDEX, (UInt32)dataLength))
                            {
                                m_transNumber++;
                                if (m_transNumber == 0)
                                { m_transNumber = 1; }
                                Buffer.BlockCopy(ReceiveData, 0, m_commRecvData, 0, cpIndex - 2);
                                this.BeginInvoke(new CommModemCallback(CommModemHandler), new object[] { m_commRecvData });
                            }
                        }

                    }
                }
            }
        }

        private void CommModemHandler(Byte[] frameData)
        {
            en_packet_status_t retSta;
            en_trans_status_t transSta;

            retSta = (en_packet_status_t)frameData[(int)en_packet_para_t.PACKET_RESULT_INDEX];
            if (m_transStatus == en_trans_status_t.TransBegin)
            {
                StopCommTimer();
                switch (retSta)
                {
                    case en_packet_status_t.PACKET_ACK_OK:
                        transSta = en_trans_status_t.TransFinished;
                        break;
                    case en_packet_status_t.PACKET_ACK_ERROR:
                        transSta = en_trans_status_t.TransFailed;
                        break;
                    case en_packet_status_t.PACKET_ACK_ADDR_ERROR:
                        transSta = en_trans_status_t.TransAddrError;
                        break;
                    default:
                        transSta = en_trans_status_t.TransFailed;
                        break;
                }
                m_transStatus = transSta;
            }
        }

        public class CustomComparer : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                string s1 = (string)x;
                string s2 = (string)y;
                if (s1.Length > s2.Length)
                {
                    return 1;
                }
                if (s1.Length < s2.Length)
                {
                    return -1;
                }
                for (int i = 0; i < s1.Length; i++)
                {
                    if (s1[i] > s2[i])
                    { return 1; }
                    if (s1[i] < s2[i])
                    { return -1; }
                }

                return 0;
            }
        }

        private void cboPorts_DropDown(object sender, EventArgs e)
        {
            string str = null;

            if (cboPorts.Items.Count > 0)
            { str = cboPorts.SelectedItem.ToString(); }
            //get serial port
            cboPorts.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length != 0)
            {
                Array.Sort(ports, new CustomComparer());
                foreach (string port in ports)
                {
                    cboPorts.Items.Add(port);
                }

                if (str == null)
                { cboPorts.SelectedIndex = 0; }
                else
                {
                    if (false == ((IList)ports).Contains(str))
                    { cboPorts.SelectedIndex = 0; }
                    else
                    { cboPorts.SelectedIndex = Array.IndexOf(ports, str); }
                }
            }
        }

        private void InitCommTimer()
        {
            m_commTimer.AutoReset = false;
            m_commTimer.Interval = 100000;
            m_commTimer.Elapsed += new System.Timers.ElapsedEventHandler(CommTimerTimeoutCallback);
        }

        private void StartCommTimer(UInt16 value)
        {
            m_commTimer.Interval = value;
            m_commTimer.Enabled = true;
            m_commTimer.Start();
        }

        private void StopCommTimer()
        {
            m_commTimer.Enabled = false;
            m_commTimer.Stop();
        }

        public void CommTimerTimeoutCallback(object source, System.Timers.ElapsedEventArgs e)
        {
            if (m_transStatus == en_trans_status_t.TransBegin)
            {
                m_transStatus = en_trans_status_t.TransTimeout;
            }
        }

        private void staStripUpdateInfo(string message)
        {
            if (m_commPort.IsOpen)
            {
                stsLabel.Text = message;
            }
            else
            {
                stsLabel.Text = "串口关闭";
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (cboPorts.Items.Count <= 0)
            {
                MessageBox.Show("没有发现串口，请检查端口!");
                return;
            }

            if (btnOpen.Text == "连接")
            {
                m_commPort.PortName = cboPorts.SelectedItem.ToString();
                m_commPort.BaudRate = Convert.ToInt32(cboBaudRate.SelectedItem.ToString());
                m_commPort.Parity = Parity.None;
                m_commPort.DataBits = 8;
                m_commPort.StopBits = StopBits.One;
                m_commPort.ReadTimeout = 500;     //读取数据的超时时间，引发ReadExisting异常
                m_commPort.WriteTimeout = 500;
                m_commPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);

                try
                {
                    if (m_commPort.IsOpen)
                    {
                        m_commPort.Close();
                        m_commPort.Open();
                    }
                    else
                    {
                        m_commPort.Open();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                btnWriteInfo.Enabled = true;
                btnReadInfo.Enabled = true;
                cboPorts.Enabled = false;
                cboBaudRate.Enabled = false;
                btnOpen.Text = "断开";
                staStripUpdateInfo(m_commPort.PortName + "打开成功" + "，波特率" + m_commPort.BaudRate.ToString());
            }
            else
            {
                m_commPort.DataReceived -= comPort_DataReceived;
                try
                {
                    if (m_commPort.IsOpen)
                    { m_commPort.Close(); }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                UartDisconnectHandle();
            }
        }

        private Byte CheckSum(Byte[] pData, UInt16 offset, UInt16 len)
        {
            UInt16 i;
            Byte sum = 0;

            for (i = 0; i < len; i++)
            {
                sum += pData[i + offset];
            }

            return sum;
        }

        private bool CommModemSendData(Byte[] transStr, UInt16 length, UInt16 timeout)
        {
            if (false == m_commPort.IsOpen)
            {
                MessageBox.Show("请先打开串口", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                for(Int32 i = 0; i < length; i++)
                {
                    m_commPort.Write(transStr, i, 1);
                }

            }
            catch (Exception ex)
            {
                return false;
            }
            //启动响应超时计数
            StartCommTimer(timeout);
            m_transStatus = en_trans_status_t.TransBegin;

            return true;
        }

        private bool CommModemPackget(Byte cmd, Byte type, UInt32 addr, Byte[] data, UInt16 length, UInt16 timeout)
        {
            UInt16 index;
            Byte[] txData = new Byte[(int)en_frame_para_t.FRAME_MAX_SIZE];
            UInt16 u16Head = (UInt16)en_frame_para_t.FRAME_HEAD;
            UInt16 frameHeadLength = (UInt16)en_frame_para_t.FRAME_SHELL_SIZE - 2;   //Minus the final CRC
            UInt16 controlLength = (UInt16)en_packet_para_t.PACKET_INSTRUCT_SIZE;
            UInt16 packetHeadLength = (UInt16)(controlLength + frameHeadLength);
            UInt16 totalLength = (UInt16)(length + controlLength);
            UInt16 crc16;

            // Packet
            index = 0;
            txData[index++] = (Byte)(u16Head & 0x00FF);
            txData[index++] = (Byte)((u16Head >> 8) & 0x00FF);

            // Update the serial number after receiving is complete.
            txData[index++] = m_transNumber;
            txData[index++] = (Byte)(m_transNumber ^ (Byte)en_frame_para_t.FRAME_NUM_XOR_BYTE);
            txData[index++] = (Byte)(totalLength & 0x00FF);
            txData[index++] = (Byte)(totalLength >> 8);

            // Content of packet
            txData[index++] = cmd;
            txData[index++] = type;
            txData[index++] = (Byte)(addr & 0x000000FF);
            txData[index++] = (Byte)((addr >> 8) & 0x000000FF);
            txData[index++] = (Byte)((addr >> 16) & 0x000000FF);
            txData[index++] = (Byte)((addr >> 24) & 0x000000FF);
            for (int i = index; i < packetHeadLength; i++)
            {
                txData[i] = 0x00;
            }
            index = packetHeadLength;
            // Copy transfer buffer
            if (length != 0)
            {
                Buffer.BlockCopy(data, 0, txData, index, length);
            }
            index += length;

            // calculate  CRC16
            crc16 = Cal_CRC16(txData, frameHeadLength, totalLength);
            txData[index++] = (Byte)(crc16 & 0x00FF);
            txData[index++] = (Byte)(crc16 >> 8);

            Array.Clear(m_commRecvData, 0, m_commRecvData.Length);
            // Send to packet
            return CommModemSendData(txData, index , timeout);
        }

        //字符串转换16进制字节数组
        private byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
            { hexString += " "; }
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            { returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Replace(" ", ""), 16); }
            return returnBytes;
        }

        /**
         * @brief  Cal CRC16 for Packet
         * @param  data
         * @param  length
         * @retval None
         */
        UInt16 Cal_CRC16(Byte[] p_data, int offset, UInt32 size)
        {
            Byte u8Cnt;
            UInt16 u16CrcResult = 0xA28C;
            UInt32 u32Offset = (UInt32)offset;

            while (size != 0)
            {
                u16CrcResult ^= p_data[u32Offset++];
                for (u8Cnt = 0; u8Cnt < 8; u8Cnt++)
                {
                    if ((u16CrcResult & 0x1) == 0x1)
                    {
                        u16CrcResult >>= 1;
                        u16CrcResult ^= 0x8408;
                    }
                    else
                    {
                        u16CrcResult >>= 1;
                    }
                }
                size--;
            }
            u16CrcResult = (UInt16)(~u16CrcResult);

            return u16CrcResult;
        }

        private void btnReadInfo_Click(object sender, EventArgs e)
        {
            if (btnReadInfo.Text == "上传")
            {
                if (false == m_commPort.IsOpen)
                {
                    MessageBox.Show("请先打开串口", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                btnWriteInfo.Enabled = false;
                btnBrowserFile.Enabled = false;
                btnReadInfo.Text = "停止";
                m_transThreadFlag = true;

                Thread transThread = new Thread(new ThreadStart(RecvThreadCallback));
                transThread.IsBackground = true;
                transThread.Start();
            }
            else if (btnReadInfo.Text == "停止")
            {
                btnWriteInfo.Enabled = true;
                btnBrowserFile.Enabled = true;
                btnReadInfo.Text = "上传";
                m_transThreadFlag = false;
            }
        }

        public bool IsHexadecimal(string str)
        {
            //const string PATTERN = @"([A-F][a-f][0-9])+$";  // @"[A-Fa-f0-9]+$";
            const string PATTERN = @"[A-Fa-f0-9]+$";
            return System.Text.RegularExpressions.Regex.IsMatch(str, PATTERN);
        }

        public bool IsDecimal(string str)
        {
            const string PATTERN = @"[0-9]+$";
            return System.Text.RegularExpressions.Regex.IsMatch(str, PATTERN);
        }

        private string StringToHexString(string s, Encoding encode)
        {
            byte[] b = encode.GetBytes(s);  //按照指定编码将string编程字节数组
            string result = string.Empty;
            for (int i = 0; i < b.Length; i++) //逐字节变为16进制字符
            {
                result += Convert.ToString(b[i], 16);
            }
            return result;
        }

        public bool getHexString(String sourceStr, ref String[] getStr)
        {
            bool hexValid = false;

            int lower = sourceStr.IndexOf("0x");
            int upper = sourceStr.IndexOf("0X");

            if (lower >= 0)
            {
                getStr = sourceStr.Split('x');
                hexValid = true;
            }
            else if (upper >= 0)
            {
                getStr = sourceStr.Split('X');
                hexValid = true;
            }

            return hexValid;
        }

        private void btnWriteInfo_Click(object sender, EventArgs e)
        {
            if (btnWriteInfo.Text == "下载")
            {
                if (false == m_commPort.IsOpen)
                {
                    MessageBox.Show("请先打开串口", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (txtDestAddress.Text.Trim() == "")
                {
                    MessageBox.Show("请设置目标地址值！");
                    return;
                }
                else
                {
                    String[] strAddrBuffer = null;

                    String strAddr = txtDestAddress.Text.ToString();
                    strAddr = strAddr.Replace(" ", "");
                    if ((getHexString(strAddr, ref strAddrBuffer) == true) && (IsHexadecimal(strAddrBuffer[1]) == true))
                    { m_appFlashAddr = Convert.ToUInt32(strAddrBuffer[1], 16); }
                    else if (IsDecimal(strAddr) == true)
                    { m_appFlashAddr = UInt32.Parse(strAddr); }
                    else
                    {
                        MessageBox.Show("请输入有效的目标地址值！");
                        return;
                    }
                }

                if (txtFilePath.Text.Trim() == "")
                {
                    MessageBox.Show("请设置有效的文件路径！");
                    return;
                }
                else
                {
                    String filePath = txtFilePath.Text.ToString();
                    if (File.Exists(filePath))
                    {
                        m_downloadFileName = filePath;
                        //Git file
                        int fileTypeIndex = m_downloadFileName.LastIndexOf(".");
                        String fileType = m_downloadFileName.Substring(fileTypeIndex, m_downloadFileName.Length - fileTypeIndex);
                        if ((fileType == ".bin") || (fileType == ".hex") || (fileType == ".srec"))
                        {
                            // backup download history
                            string port = cboPorts.SelectedItem.ToString();
                            string bps = cboBaudRate.SelectedItem.ToString();
                            string addr = txtDestAddress.Text.ToString();
                            string path = txtFilePath.Text.ToString();
                            BackupIapConfig(port, bps, addr, path);
                        }
                        else
                        {
                            MessageBox.Show("请设置有效的文件类型！");
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("请设置有效的文件路径！");
                        return;
                    }
                }

                btnReadInfo.Enabled = false;
                btnBrowserFile.Enabled = false;
                m_transNumber = 1;
                m_transThreadFlag = true;
                m_transStatus = en_trans_status_t.TransIdle;
                btnWriteInfo.Text = "停止";
                staStripUpdateInfo("开始下载");
                Thread transThread = new Thread(new ThreadStart(TransThreadCallback));
                transThread.IsBackground = true;
                transThread.Start();
            }
            else if (btnWriteInfo.Text == "停止")
            {
                btnReadInfo.Enabled = true;
                btnBrowserFile.Enabled = true;
                m_transThreadFlag = false;
                m_transStatus = en_trans_status_t.TransAbort;
                btnWriteInfo.Text = "下载";
                staStripUpdateInfo("终止下载");
            }
        }

        public void TransThreadCallback()
        {
            en_packet_cmd_t transProcess;
            UInt32 flashAddr = m_appFlashAddr;
            en_trans_status_t threadSta = en_trans_status_t.TransIdle;
            Byte[] filePtr = null;
            int fileLength = 0, fileIndex = 0, transFileSize = 0;
            Byte[] transBuffer = new Byte[(int)en_packet_para_t.PACKET_DATA_SIZE];
            int downTotalEntries = 0, downProgressVal = 0;
            bool txResult = false;
            UInt16 crc16 = 0;

            //Git file
            int fileTypeIndex = m_downloadFileName.LastIndexOf(".");
            String fileType = m_downloadFileName.Substring(fileTypeIndex, m_downloadFileName.Length - fileTypeIndex);
            if (fileType == ".bin")
            {
                FileStream fs = new FileStream(m_downloadFileName, FileMode.Open, FileAccess.Read);
                fileLength = (int)fs.Length;
                filePtr = new Byte[fileLength];
                fs.Read(filePtr, 0, filePtr.Length);
                fs.Close();
            }
            else if (fileType == ".hex")
            {
                UInt32 addr = 0;
                HexToBin hextobin = new HexToBin();
                if (HexToBin.ExecResult.ExecOk == hextobin.HEX_ConvertBin(m_downloadFileName, ref addr, ref filePtr))
                {
                    fileLength = filePtr.Length;
                    /*flashAddr = addr;*/
                }
            }
            else if (fileType == ".srec")
            {
                UInt32 addr = 0;
                HexToBin srectobin = new HexToBin();
                if (HexToBin.ExecResult.ExecOk == srectobin.SREC_ConvertBin(m_downloadFileName, ref addr, ref filePtr))
                {
                    fileLength = filePtr.Length;
                    /*flashAddr = addr;*/
                }
            }

            // Transmit data decompose
            if (fileLength == 0)
            {
                m_transThreadFlag = false;
                threadSta = en_trans_status_t.TransFileInvalid;
            }
            else
            {
                transFileSize = fileLength;
                downTotalEntries = fileLength / (int)en_packet_para_t.PACKET_DATA_SIZE;
                if ((fileLength % (int)en_packet_para_t.PACKET_DATA_SIZE) != 0)
                { downTotalEntries += 1; }
            }

            transProcess = en_packet_cmd_t.PACKET_CMD_APP_UPGRADE;
            CommProgress(100, 0);
            while (m_transThreadFlag)
            {
                switch (transProcess)
                {
                    case en_packet_cmd_t.PACKET_CMD_HANDSHAKE:
                        {
                            txResult = CommModemPackget((Byte)en_packet_cmd_t.PACKET_CMD_HANDSHAKE, 
                                                        (Byte)en_packet_type_t.PACKET_TYPE_CONTROL, 0, null, 0, 5000);
                            if (txResult)
                            {
                                while (m_transStatus == en_trans_status_t.TransBegin) ;
                                if (m_transStatus == en_trans_status_t.TransFinished)
                                {
                                    transProcess = en_packet_cmd_t.PACKET_CMD_ERASE_FLASH;
                                }
                            }
                            else
                            {
                                m_transStatus = en_trans_status_t.TransFailed;
                                StopCommTimer();
                            }
                        }
                        break;
                    case en_packet_cmd_t.PACKET_CMD_ERASE_FLASH:
                        {
                            transBuffer[0] = (Byte)(transFileSize & 0x000000ff);
                            transBuffer[1] = (Byte)((transFileSize >> 8) & 0x000000ff);
                            transBuffer[2] = (Byte)((transFileSize >> 16) & 0x000000ff);
                            transBuffer[3] = (Byte)((transFileSize >> 24) & 0x000000ff);
                            txResult = CommModemPackget((Byte)en_packet_cmd_t.PACKET_CMD_ERASE_FLASH, 
                                                        (Byte)en_packet_type_t.PACKET_TYPE_DATA, flashAddr, transBuffer, 4, 5000);
                            if (txResult)
                            {
                                while (m_transStatus == en_trans_status_t.TransBegin) ;
                                if (m_transStatus == en_trans_status_t.TransFinished)
                                {
                                    transProcess = en_packet_cmd_t.PACKET_CMD_APP_DOWNLOAD;
                                    // Initialize download parameter
                                    downProgressVal = 0;
                                    fileIndex = 0;
                                }
                            }
                            else
                            {
                                m_transStatus = en_trans_status_t.TransFailed;
                                StopCommTimer();
                            }
                        }
                        break;
                    case en_packet_cmd_t.PACKET_CMD_APP_DOWNLOAD:
                        {
                            if (fileLength > (int)en_packet_para_t.PACKET_DATA_SIZE)
                            {
                                Buffer.BlockCopy(filePtr, fileIndex, transBuffer, 0, (int)en_packet_para_t.PACKET_DATA_SIZE);
                                txResult = CommModemPackget((Byte)en_packet_cmd_t.PACKET_CMD_APP_DOWNLOAD, 
                                                            (Byte)en_packet_type_t.PACKET_TYPE_DATA, flashAddr, transBuffer,
                                                            (UInt16)en_packet_para_t.PACKET_DATA_SIZE, 5000);
                            }
                            else
                            {
                                Buffer.BlockCopy(filePtr, fileIndex, transBuffer, 0, fileLength);
                                txResult = CommModemPackget((Byte)en_packet_cmd_t.PACKET_CMD_APP_DOWNLOAD, 
                                                            (Byte)en_packet_type_t.PACKET_TYPE_DATA, flashAddr, transBuffer, 
                                                            (UInt16)fileLength, 5000);
                            }
                            if (txResult)
                            {
                                while (m_transStatus == en_trans_status_t.TransBegin) ;
                                if (m_transStatus == en_trans_status_t.TransFinished)
                                {
                                    if (fileLength > (int)en_packet_para_t.PACKET_DATA_SIZE)
                                    {
                                        fileIndex += (int)en_packet_para_t.PACKET_DATA_SIZE;
                                        fileLength -= (int)en_packet_para_t.PACKET_DATA_SIZE;
                                        flashAddr += (int)en_packet_para_t.PACKET_DATA_SIZE;
                                        downProgressVal++;
                                        CommProgress(downTotalEntries, downProgressVal);
                                    }
                                    else
                                    {
                                        fileLength = 0;
                                        transProcess = en_packet_cmd_t.PACKET_CMD_FLASH_CRC;
                                    }
                                }
                            }
                            else
                            {
                                m_transStatus = en_trans_status_t.TransFailed;
                                StopCommTimer();
                            }
                        }
                        break;
                    case en_packet_cmd_t.PACKET_CMD_JUMP_TO_APP:
                        {
                            txResult = CommModemPackget((Byte)en_packet_cmd_t.PACKET_CMD_JUMP_TO_APP,
                                                        (Byte)en_packet_type_t.PACKET_TYPE_CONTROL,
                                                        0, null, 0, 5000);
                            if (txResult)
                            {
                                while (m_transStatus == en_trans_status_t.TransBegin) ;
                                if (m_transStatus == en_trans_status_t.TransFinished)
                                {
                                    CommProgress(downTotalEntries, downTotalEntries);
                                    threadSta = en_trans_status_t.TransFinished;
                                }
                            }
                            else
                            {
                                m_transStatus = en_trans_status_t.TransFailed;
                                StopCommTimer();
                            }
                        }
                        break;
                    case en_packet_cmd_t.PACKET_CMD_FLASH_CRC:
                        {
                            transBuffer[0] = (Byte)(transFileSize & 0x000000ff);
                            transBuffer[1] = (Byte)((transFileSize >> 8) & 0x000000ff);
                            transBuffer[2] = (Byte)((transFileSize >> 16) & 0x000000ff);
                            transBuffer[3] = (Byte)((transFileSize >> 24) & 0x000000ff);
                            txResult = CommModemPackget((Byte)en_packet_cmd_t.PACKET_CMD_FLASH_CRC,
                                                        (Byte)en_packet_type_t.PACKET_TYPE_DATA,
                                                        m_appFlashAddr, transBuffer, 4, 5000);
                            if (txResult)
                            {
                                while (m_transStatus == en_trans_status_t.TransBegin) ;
                                if (m_transStatus == en_trans_status_t.TransFinished)
                                {
                                    crc16 = (UInt16)(m_commRecvData[(int)en_packet_para_t.PACKET_DATA_INDEX] +
                                                    (m_commRecvData[(int)en_packet_para_t.PACKET_DATA_INDEX + 1] << 8));
                                    if (crc16 == Cal_CRC16(filePtr, 0, (UInt32)transFileSize))
                                    {
                                        transProcess = en_packet_cmd_t.PACKET_CMD_JUMP_TO_APP;
                                    }
                                    else
                                    {
                                        m_transStatus = en_trans_status_t.TransFailed;
                                    }
                                }
                            }
                            else
                            {
                                m_transStatus = en_trans_status_t.TransFailed;
                                StopCommTimer();
                            }
                        }
                        break;
                    case en_packet_cmd_t.PACKET_CMD_APP_UPGRADE:
                        {
                            txResult = CommModemPackget((Byte)en_packet_cmd_t.PACKET_CMD_APP_UPGRADE, 
                                                        (Byte)en_packet_type_t.PACKET_TYPE_CONTROL, 0, null, 0, 3000);
                            if (txResult)
                            {
                                while (m_transStatus == en_trans_status_t.TransBegin);
                                if (m_transStatus == en_trans_status_t.TransFinished)
                                {
                                    transProcess = en_packet_cmd_t.PACKET_CMD_HANDSHAKE;
                                    Thread.Sleep(2000);       /* Wait for MCU reset */
                                }
                            }
                            else
                            {
                                m_transStatus = en_trans_status_t.TransFailed;
                                StopCommTimer();
                            }
                        }
                        break;
                    default:
                        break;
                }

                // error
                if ((m_transStatus == en_trans_status_t.TransTimeout)   ||
                    (m_transStatus == en_trans_status_t.TransFailed)    ||
                    (m_transStatus == en_trans_status_t.TransAbort)     ||
                    (m_transStatus == en_trans_status_t.TransAddrError))
                {
                    threadSta = m_transStatus;
                    break;
                }
                // finished
                if (threadSta == en_trans_status_t.TransFinished)
                {
                    break;
                }
            }
            /* Stop timer when thread abort */
            if (m_commTimer.Enabled == true)
            {
                StopCommTimer();
            }
            m_transThreadFlag = false;
            CommThreadFinish(en_thread_number_t.TransFileThead, threadSta, transProcess);
        }

        public void RecvThreadCallback()
        {
            while (m_transThreadFlag)
            {
                break;
            }

            // result of execution
            if (m_transThreadFlag)  // failed
            {
                m_transThreadFlag = false;
                MessageBox.Show("上传失败！");
            }
            else
            {
                MessageBox.Show("上传完成！");
            }

            //CommThreadFinish(en_thread_number_t.RecvFileThead);
        }

        private void btnBrowserFile_Click(object sender, EventArgs e)
        {
            string filePath = string.Empty;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = "bin";
            dlg.RestoreDirectory = true;
            dlg.Filter = "Bin Files|*.bin|Hex Files|*.hex|Srec Files|*.srec";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                filePath = dlg.FileName;
                if (filePath != "")
                {
                    // Save file name
                    txtFilePath.Text = filePath;
                }
            }
        }

        private void tmrPortChackHandle(object sender, EventArgs e)
        {
            //update port
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length != m_commPortNum)
            {
                m_commPortNum = ports.Length;
                if (false == ((IList)ports).Contains(m_commPort.PortName))
                {
                    m_commPort.DataReceived -= comPort_DataReceived;
                    try
                    {
                        m_commPort.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    UartDisconnectHandle();
                }
                cboPorts_DropDown(sender, e);
            }
        }

        public void CommThreadFinish(en_thread_number_t threadNum, en_trans_status_t threadSta, en_packet_cmd_t threadCmd)
        {
            //判断是否需要进行唤醒的请求，如果控件与主线程在一个线程内，可以写成 if(!InvokeRequired)
            if (!this.btnBrowserFile.InvokeRequired)
            {
                string title = null, command = null;

                if (threadNum == en_thread_number_t.TransFileThead)
                {
                    btnReadInfo.Enabled = true;
                    btnBrowserFile.Enabled = true;
                    btnWriteInfo.Text = "下载";
                }
                else if (threadNum == en_thread_number_t.RecvFileThead)
                {
                    btnWriteInfo.Enabled = true;
                    btnBrowserFile.Enabled = true;
                    btnReadInfo.Text = "上传";
                }

                if (threadNum == en_thread_number_t.TransFileThead)
                {
                    title = "下载";
                }
                else if (threadNum == en_thread_number_t.RecvFileThead)
                {
                    title = "上传";
                }

                switch (threadCmd)
                {
                    case en_packet_cmd_t.PACKET_CMD_HANDSHAKE:
                        command = "握手";
                        break;
                    case en_packet_cmd_t.PACKET_CMD_JUMP_TO_APP:
                        command = "跳转";
                        break;
                    case en_packet_cmd_t.PACKET_CMD_APP_DOWNLOAD:
                        command = "下载";
                        break;
                    case en_packet_cmd_t.PACKET_CMD_APP_UPLOAD:
                        command = "上传";
                        break;
                    case en_packet_cmd_t.PACKET_CMD_ERASE_FLASH:
                        command = "擦除Flash";
                        break;
                    case en_packet_cmd_t.PACKET_CMD_FLASH_CRC:
                        command = "Flash校验";
                        break;
                    case en_packet_cmd_t.PACKET_CMD_APP_UPGRADE:
                        command = "APP升级";
                        break;
                    default:
                        break;
                }

                switch (threadSta)
                {
                    case en_trans_status_t.TransFinished:
                        staStripUpdateInfo(title + "完成！");
                        break;
                    case en_trans_status_t.TransTimeout:
                        staStripUpdateInfo(title + "程序," + command + "超时,"+ "请检查设备及连接线是否正常!");
                        break;
                    case en_trans_status_t.TransFileInvalid:
                        staStripUpdateInfo(title + "程序," + "请选择有效的文件!");
                        break;
                    case en_trans_status_t.TransAddrError:
                        staStripUpdateInfo(title + "程序," + command + "地址错误" + "请输入有效地址值!");
                        break;
                    case en_trans_status_t.TransFailed:
                        staStripUpdateInfo(title + "程序," + command + "失败" + "请检查参数是否合法!");
                        break;
                    case en_trans_status_t.TransAbort:
                        staStripUpdateInfo(title + "程序," + "终止!");
                        break;
                    default:
                        break;
                }
            }
            else
            {
                CommThreadFinishDelegate otherThread = new CommThreadFinishDelegate(CommThreadFinish);
                this.BeginInvoke(otherThread, new object[] { threadNum, threadSta, threadCmd });
            }
        }

        public void CommProgress(int total, int currVal)
        {
            //判断是否需要进行唤醒的请求，如果控件与主线程在一个线程内，可以写成 if(!InvokeRequired)
            if (!this.prgBarTransSchedule.InvokeRequired)
            {
                float percent = (float)currVal / total;
                int perValue = (int)(percent * 100);

                lblTransSchedule.Text = perValue.ToString() + "%";
                prgBarTransSchedule.Maximum = total;
                prgBarTransSchedule.Value = currVal;
            }
            else
            {
                CommProgressDelegate otherThread = new CommProgressDelegate(CommProgress);
                this.BeginInvoke(otherThread, new object[] { total, currVal });     //执行唤醒操作
            }
        }

        public void BackupIapConfig(string port, string bps, string addr, string path)
        {
            string exeRootDir = Directory.GetCurrentDirectory();
            string iniFilePath = exeRootDir + "\\IapConfig.ini";
            if (File.Exists(iniFilePath))
            {
                IniFileHelper iapConfig = new IniFileHelper(iniFilePath);

                string comPort = iapConfig.ReadValue("GENERAL", "ComPort");
                if ((comPort != string.Empty) && (comPort.Trim() != ""))
                {
                    if (comPort != port)
                    {
                        iapConfig.WriteValue("GENERAL", "ComPort", port);
                    }
                }
                string destAddr = iapConfig.ReadValue("GENERAL", "DestAddress");
                if ((destAddr != string.Empty) && (destAddr.Trim() != ""))
                {
                    if (destAddr != addr)
                    {
                        iapConfig.WriteValue("GENERAL", "DestAddress", addr);
                    }
                }
                string baudRate = iapConfig.ReadValue("GENERAL", "BaudRate");
                if ((baudRate != string.Empty) && (baudRate.Trim() != ""))
                {
                    if (baudRate != bps)
                    {
                        iapConfig.WriteValue("GENERAL", "BaudRate", bps);
                    }
                }
                string filePath = iapConfig.ReadValue("GENERAL", "FilePath");
                if ((filePath != string.Empty) && (filePath.Trim() != ""))
                {
                    if (filePath != path)
                    {
                        iapConfig.WriteValue("GENERAL", "FilePath", path);
                    }
                }
            }
        }
    }

    public class IniFileHelper
    {
        // 声明INI文件的写操作函数 WritePrivateProfileString()
        [DllImport("kernel32")]             //返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        // 声明INI文件的读操作函数 GetPrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]      //返回取得字符串缓冲区的长度
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);

        private string curFilePath = null;
        public IniFileHelper(string path)
        {
            this.curFilePath = path;
        }

        public bool WriteValue(string section, string key, string value, string filePath=null)
        {
            if (filePath == null)
                filePath = this.curFilePath;

            if (File.Exists(filePath))
            {
                // section,key,value,path
                long opResult = WritePrivateProfileString(section, key, " "+value, filePath);
                if (opResult == 0)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public string ReadValue(string section, string key, string filePath=null)
        {
            if (filePath == null)
                filePath = this.curFilePath;

            if (File.Exists(filePath))
            {
                // read byte total
                System.Text.StringBuilder temp = new System.Text.StringBuilder(1024);
                // section,key,temp,path
                GetPrivateProfileString(section, key, "", temp, 1024, filePath);
                return temp.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
