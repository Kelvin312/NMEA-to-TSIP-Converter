using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogReader
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private ComPort port = new ComPort();

        private void MainForm_Load(object sender, EventArgs e)
        {
            cmbPort.Items.AddRange(port.PortNames());
            cmbBaudRate.Items.AddRange(port.BaudRates());
            
            cmbPort.SelectedIndex = Properties.Settings.Default.nPort >= cmbPort.Items.Count?-1: Properties.Settings.Default.nPort;
            cmbBaudRate.SelectedIndex = Properties.Settings.Default.nBaudRate;
            cmbParity.Items.Add(System.IO.Ports.Parity.None);
            cmbParity.Items.Add(System.IO.Ports.Parity.Odd);
            cmbParity.Items.Add(System.IO.Ports.Parity.Even);
            cmbParity.SelectedItem = Properties.Settings.Default.vParity;
            port.DataReceived += Port_DataReceived;
        }

        

        private const int bufferSize = 1024;
        private byte[] readBuffer = new byte[bufferSize];
        private int rxStart = 0, rxEnd = 0;

        byte ReadBuff(int index)
        {
            if (index < 0) index += bufferSize;
            while (index >= bufferSize) index -= bufferSize;
            return readBuffer[index];
        }

        private int uartNumber = 0;
        private const byte DLE = 0x10;
        private const byte ETX = 0x03;
        private bool isNewPacket = true;
        private DateTime oldTime = DateTime.Now;
        StringBuilder txtLogBuf = new StringBuilder();
        StringBuilder txtLog2Buf = new StringBuilder();
        StringBuilder txtAsciiBuf = new StringBuilder();
        

        void addLog(byte c)
        {

            lock (txtLogBuf)
            {
                ((uartNumber == 1) ? txtLog2Buf : txtLogBuf).Append(string.Format("{0:X2} ", (int) c));
                if (c < 0x20) c = 0x2E;
                txtAsciiBuf.Append(string.Format("{0}", (char) c));
            }
        }

        private void Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if(port.Read(ref readBuffer, bufferSize, ref rxStart, ref rxEnd) < 1) return;

            for (; rxStart != rxEnd; rxStart++)
            {
                if (rxStart >= bufferSize) rxStart -= bufferSize;

                if (isNewPacket || DateTime.Now.Subtract(oldTime).TotalMilliseconds > 100)
                {
                    isNewPacket = false;
                    var dtText = string.Format("\r\n\r\n[{0:HH:mm:ss.ff}]  ", DateTime.Now);
                    lock (txtLogBuf)
                    {
                        ((uartNumber == 1) ? txtLog2Buf : txtLogBuf).Append(dtText);
                        txtAsciiBuf.Append(dtText);
                    }
                }
                oldTime = DateTime.Now;

                byte c = readBuffer[rxStart];
                byte oldByte = ReadBuff(rxStart - 1);

                if (c == 0xAA)
                { }
                else if (c == 0xB0 && oldByte == 0xAA)
                {
                    uartNumber = 0;
                }
                else if (c == 0xB1 && oldByte == 0xAA)
                {
                    uartNumber = 1;
                }
                else
                {
                    if (oldByte == 0xAA)
                    {
                        addLog(oldByte);
                    }
                    addLog(c);
                    if (c == ETX && oldByte == DLE || c == 0x0A && oldByte == 0x0D)
                    {
                        isNewPacket = true;
                    }
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.nPort = cmbPort.SelectedIndex;
            Properties.Settings.Default.nBaudRate = cmbBaudRate.SelectedIndex;
            Properties.Settings.Default.vParity = (System.IO.Ports.Parity)cmbParity.SelectedItem;
            Properties.Settings.Default.Save();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            port.Open(cmbPort.SelectedItem.ToString(), cmbBaudRate.SelectedItem.ToString(), (System.IO.Ports.Parity)cmbParity.SelectedItem);
            lblPortStatus.Text = port.IsOpen() ? "Open" : "Close";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            port.Close();
            lblPortStatus.Text = port.IsOpen() ? "Open" : "Close";
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";
            txtLog2.Text = "";
            txtAscii.Text = "";
        }

        private void btnSendNMEA_Click(object sender, EventArgs e)
        {
            byte[] payload = new byte[256];
            int len = 0;
            payload[len++] = (byte)'$';
            byte CK = (byte)(payload[1] ^ payload[2]);
            foreach (var txtChar in txtHexData.Text)
            {
                byte val = (byte)txtChar;
                payload[len] = val;
                len++;
                CK ^= val;
            }
            payload[len++] = (byte) '*';
            payload[len++] = (byte)string.Format("{0:X2}", (int)CK)[0];
            payload[len++] = (byte)string.Format("{0:X2}", (int)CK)[1];
            payload[len++] = 0x0D;
            payload[len++] = 0x0A;
            port.Write(payload, len);
        }

        private int isAReset = 0;

        private void btnReset_Click(object sender, EventArgs e)
        {
            port.SetDtr(true);
            isAReset = 0;
        }

        private void timer100ms_Tick(object sender, EventArgs e)
        {
            if (isAReset == 1) port.SetDtr(false);
            if (++isAReset > 10) isAReset = 10;

            lock (txtLogBuf)
            {
                if (txtAsciiBuf.Length > 0)
                {
                    txtAscii.AppendText(txtAsciiBuf.ToString());
                    txtAsciiBuf.Clear();
                }
                if (txtLogBuf.Length > 0)
                {
                    txtLog.AppendText(txtLogBuf.ToString());
                    txtLogBuf.Clear();
                }
                if (txtLog2Buf.Length > 0)
                {
                    txtLog2.AppendText(txtLog2Buf.ToString());
                    txtLog2Buf.Clear();
                }
            }
        }

        private void btnSendUBX_Click(object sender, EventArgs e)
        {
            byte[] payload = new byte[256];
            int len = 0;
            payload[len++] = 0xB5;
            payload[len++] = 0x62;
            byte CK_A = 0, CK_B = 0;
            foreach (var txtByte in txtHexData.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                byte val = byte.Parse(txtByte, NumberStyles.AllowHexSpecifier);
                payload[len] = val;
                len++;
                CK_A += val;
                CK_B += CK_A;
            }
            payload[len++] = CK_A;
            payload[len++] = CK_B;
            port.Write(payload, len);
        }
    }
}
