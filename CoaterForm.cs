using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Modbus.Device;
using System.IO.Ports;
using System.Timers;
using CSharpTcpDemo;
using WinFormsApp_Draft.Async;

namespace WinFormsApp_Draft
{
    public partial class CoaterForm : Form
    {
        //coater declarations
        private SerialPort motor_port = new SerialPort();
        public IModbusMaster master;

        private MotorAsync motorAsync = new MotorAsync();
        private static int spin_speed = 0;
        private System.Timers.Timer spin_timer = new System.Timers.Timer();
        private CancellationTokenSource cancellationTokenSource_pos;
        private CancellationTokenSource cancellationTokenSource_beat;
        public static bool coater_connect_state = false;

        public CoaterForm()
        {
            InitializeComponent();
            DisableCoater();

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                MotorPorts.Items.Add(port);
            }
            MotorPorts.SelectedIndex = 0;

            MotorBaudRate.Items.Add("9600");
            MotorBaudRate.Items.Add("19200");
            MotorBaudRate.Items.Add("38400");
            MotorBaudRate.Items.Add("115200");
            MotorBaudRate.SelectedIndex = 0;
        }

        public void EnableCoater()
        {
            foreach (System.Windows.Forms.Control ctr in Controls)
            {
                if (ctr == SpinCoater)
                {
                    ctr.Enabled = true;
                }
            }
        }

        public void DisableCoater()
        {
            foreach (System.Windows.Forms.Control ctr in Controls)
            {
                if (ctr == SpinCoater)
                {
                    ctr.Enabled = false;
                }
            }
        }

        private void CoaterForm_Load(object sender, EventArgs e) { }

        private void MotorPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedMotorPort.Text = MotorPorts.SelectedItem.ToString();
        }

        private void MotorBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedMotorBR.Text = MotorBaudRate.SelectedItem.ToString();
        }

        private async void MotorSerialSwitch_Click(object sender, EventArgs e)
        {
            if (SelectedMotorBR.Text != "" && SelectedMotorPort.Text != "" && motor_port.IsOpen == false)
            {
                motor_port.PortName = SelectedMotorPort.Text;
                motor_port.BaudRate = Convert.ToInt32(SelectedMotorBR.Text);
                motor_port.DataBits = 8;
                motor_port.StopBits = StopBits.Two;
                motor_port.Parity = Parity.None;
                motor_port.WriteTimeout = 500;
                motor_port.ReadTimeout = 500;

                try
                {
                    Response.Clear();
                    motor_port.Open();
                    master = ModbusSerialMaster.CreateRtu(motor_port);
                    MotorConnection.Text = "connecting..";


                    cancellationTokenSource_beat = new CancellationTokenSource();
                    await Task.Run(() => motorAsync.StartBeatAsync(master, cancellationTokenSource_beat.Token));

                    if (motor_port.IsOpen)
                    {
                        EnableCoater();
                        MotorSerialSwitch.Text = "Disconnect Motor";
                        MotorConnection.Text = "opened";
                        coater_connect_state = true;
                    }
                }
                catch (Exception ex)
                {
                    Response.Text = ex.Message;
                    motor_port.Close();
                    MotorSerialSwitch.Text = "Open Serial";
                    MotorConnection.Text = "closed";
                }
            }
            else
            {
                try
                {
                    Response.Clear();
                    motor_port.Close();
                    MotorSerialSwitch.Text = "Open Serial";
                    MotorConnection.Text = "closed";
                    if (cancellationTokenSource_beat != null)
                    {
                        cancellationTokenSource_beat?.Cancel();
                    }
                    coater_connect_state = false;
                    DisableCoater();
                }
                catch (Exception ex)
                {
                    Response.Text = ex.Message;
                }
            }
        }

        private async Task SendRequestAsync()
        {
            if (motor_port.IsOpen == true)
            {
                try
                {
                    byte slaveID = 0x01;
                    ushort modeAddress = 0x1771;

                    if (SpinSpeed.Text != "")
                    {
                        ushort speedAddress = 0x1773;
                        int speed = Convert.ToInt32(SpinSpeed.Text);
                        spin_speed = speed * 7;
                        ushort high = (ushort)(spin_speed >> 16);
                        ushort low = (ushort)spin_speed;
                        ushort[] speed_erpm = { high, low };

                        master.WriteMultipleRegisters(slaveID, speedAddress, speed_erpm);
                    }

                    if (SpinDuration.Text != "")
                    {
                        spin_timer.Elapsed += new ElapsedEventHandler(FreeStop_timer);
                        spin_timer.Interval = Convert.ToInt32(SpinDuration.Text) * 1000;
                        spin_timer.AutoReset = false;
                        spin_timer.Start();
                    }

                    if (AccSpeed.Text != "")
                    {
                        ushort accAddress = 0x1780;
                        int acc = Convert.ToInt32(AccSpeed.Text);
                        acc = acc * 7;
                        ushort high = (ushort)(acc >> 16);
                        ushort low = (ushort)acc;
                        ushort[] acc_erpm = { high, low };

                        master.WriteMultipleRegisters(slaveID, accAddress, acc_erpm);
                    }

                    master.WriteSingleRegister(slaveID, modeAddress, 0x0001);
                }

                catch (Exception ex)
                {
                    Response.Text = ex.Message;
                }
            }
        }

        private async void SendRequest_Click(object sender, EventArgs e)
        {
            await Task.Run(() => SendRequestAsync());
        }

        private void FreeStop_timer(object sender, ElapsedEventArgs e)
        {
            byte slaveID = 0x01;
            ushort stopAddress = 0x177E;
            ushort modeAddress = 0x1771;
            master.WriteSingleRegister(slaveID, stopAddress, 0x0000);
            master.WriteSingleRegister(slaveID, modeAddress, 0x0006);
            spin_timer.Stop();

            spin_speed = 0;
        }

        private async void FreeStop_Click(object sender, EventArgs e)
        {
            await Task.Run(() => motorAsync.FreeStopAsync(FreeStop, master));
            spin_speed = 0;
        }

        private async void ForceStop_Click(object sender, EventArgs e)
        {
            await Task.Run(() => motorAsync.ForceStopAsync(ForceStop, master));
            spin_speed = 0;
        }

        private async void Update_CheckedChanged(object sender, EventArgs e)
        {
            if (Update.Checked)
            {
                cancellationTokenSource_pos = new CancellationTokenSource();
                await Task.Run(() => motorAsync.UpdatePositionAsync(Update, Position, master, cancellationTokenSource_pos.Token));
            }
            else
            {
                cancellationTokenSource_pos?.Cancel();
                Position.Clear();
            }
        }

        private async void ClearPos_Click(object sender, EventArgs e)
        {
            Position.Clear();
            await Task.Run(() => motorAsync.ClearPosAsync(ClearPos, master));
        }

        private async void ResetMotor_Click(object sender, EventArgs e)
        {
            await Task.Run(() => motorAsync.ResetMotorAsync(ResetMotor, master, spin_speed));
            spin_speed = 0;
        }
    }
}
