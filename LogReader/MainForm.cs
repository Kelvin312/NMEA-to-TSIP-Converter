using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            cmbPort.SelectedIndex = Properties.Settings.Default.nPort;
            cmbBaudRate.SelectedIndex = Properties.Settings.Default.nBaudRate;
            cmbParity.Items.Add(System.IO.Ports.Parity.None);
            cmbParity.Items.Add(System.IO.Ports.Parity.Odd);
            cmbParity.Items.Add(System.IO.Ports.Parity.Even);
            cmbParity.SelectedItem = Properties.Settings.Default.vParity;
            port.DataReceived += Port_DataReceived;
        }


        private enum Flags
        {
            None,
            PacketName,
            PpsTime,
            Error,
            TsipPacket
        }

        private void Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] readBuffer = new byte[1024];
            int readCount = port.Read(ref readBuffer, 5);
            Flags flag = Flags.None;

            for (int i=0; i<readCount; i++)
            {
                if (flag == Flags.None && i+2 < readCount)
                {
                    switch (readBuffer[i])
                    {
                        case 0x3F: //?
                                flag = Flags.PacketName;
                            break;
                        case 0x50: //P
                            if(i + 3 < readCount)
                            flag = Flags.PpsTime;
                            break;
                        case 0xEE:
                            flag = Flags.Error;
                            break;
                        case 0x10:
                            flag = Flags.TsipPacket;
                            break;
                    }
                }
                switch (flag)
                {
                    case Flags.PacketName:
                        txtLog.AppendText(string.Format("{0}{1}{2}\r\n", ((readBuffer[i+1]== 0x4D) ?'R':'G'), (char)readBuffer[i+1], (char)readBuffer[i+2]));
                        flag = Flags.None;
                        i += 2;
                        break;
                    case Flags.PpsTime:
                        txtLog.AppendText(string.Format("{0:D}s {1:D}ms\r\n", readBuffer[i+1], readBuffer[i+2]*256 + readBuffer[i + 3]));
                        flag = Flags.None;
                        i += 3;
                        break;
                    case Flags.Error:
                        txtLog.AppendText(string.Format("{0}\r\n", ((readBuffer[i + 1] == 0xCC) ? "CRC error" : "Persing error")));
                        flag = Flags.None;
                        i += 2;
                        break;
                    case Flags.TsipPacket:
                        txtLog.AppendText(string.Format("{0:X2} ", readBuffer[i]));
                        if(readBuffer[i] == 0x03 && i>0 && readBuffer[i-1] == 0x10)
                        {
                            flag = Flags.None;
                            txtLog.AppendText("\r\n");
                        }
                        break;
                    default:
                        txtLog.AppendText(string.Format("\\{0:X2} ", readBuffer[i]));
                        break;

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
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            port.Close();
        }
    }
}
