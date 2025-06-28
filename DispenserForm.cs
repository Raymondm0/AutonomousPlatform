using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Timers;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using WinFormsApp_Draft.DK;
using System.IO.Ports;

namespace WinFormsApp_Draft
{
    public partial class DispenserForm : Form
    {
        private SerialPort port = new SerialPort();
        private Axes axes_movement = new Axes();
        private Pipette pip_action = new Pipette();
        private DKPoint point = new DKPoint();
        private CancellationTokenSource cancellationToken_state = new CancellationTokenSource();
        public bool port_opened = false;

        private static byte index;
        private const uint baudrate = 115200;

        private const byte left_tip = 1, right_tip = 2;
        private const byte left_z = 3, right_z = 4;
        private const byte x_id = 5, y_id = 6;
        public static byte left_tip_status = 1, right_tip_status = 1;//1为达到预定位置
        public static byte left_z_status = 1, right_z_status = 1;
        public static byte x_status = 1, y_status = 1;

        private byte left_tip_check = 2, right_tip_check = 2;


        public DispenserForm()
        {
            InitializeComponent();
        }

        public void DispenserForm_Load(object sender, EventArgs e) 
        {
            DispenserPorts.Items.Clear();

            LeftTipEnable.Enabled = false;
            RightTipEnable.Enabled = false;

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                DispenserPorts.Items.Add(port[3]);
            }
            DispenserPorts.SelectedIndex = 0;
            
            X.Text = "0";
            Y.Text = "0";
            LeftZ.Text = "0";
            RightZ.Text = "0";
            LeftTipVolume.Text = "0";
            RightTipVolume.Text = "0";
        }

        private void DispenserForm_Closed(object sender, EventArgs e)
        {
            cancellationToken_state?.Cancel();
        }

        private async void DispenserSerialSwitch_Click(object sender, EventArgs e)
        {
            string selected_port = DispenserPorts.SelectedItem.ToString();
            index = Convert.ToByte(selected_port);

            if (port_opened)
            {
                await Task.Run(new Action(() =>
                {
                    cancellationToken_state?.Cancel();
                    Axes.Enable_motor_c(index, x_id, 0);
                    Axes.Enable_motor_c(index, y_id, 0);
                    Axes.Enable_motor_c(index, left_z, 0);
                    Axes.Enable_motor_c(index, right_z, 0);
                    Axes.Enable_motor_c(index, left_tip, 0);
                    Axes.Enable_motor_c(index, right_tip, 0);
                }));

                int response = Axes.CloseComPort(index);
                Response.Text = Convert.ToString(response);
                if (response == 1)
                {
                    port_opened = false;
                    DispenserSerialSwitch.Text = "connect";
                    DispenserConnectionState.Text = "dispenser disconnected";
                }
            }
            else
            {
                int response = Axes.OpenComPort(index, baudrate);
                Response.Text = Convert.ToString(response);

                if (response == 1)
                {
                    port_opened = true;

                    await Task.Run(new Action(() =>
                    {
                        DispenserConnectionState.Invoke(new Action(() =>
                        {
                            DispenserConnectionState.Text = "connecting...";
                        }));
                        Axes.Enable_motor_c(index, x_id, 1);
                        Axes.Enable_motor_c(index, y_id, 1);
                        Axes.Enable_motor_c(index, left_z, 1);
                        Axes.Enable_motor_c(index, right_z, 1);
                        Axes.Enable_motor_c(index, left_tip, 1);
                        Axes.Enable_motor_c(index, right_tip, 1);
                        Axes.Set_speed_ac_de_time_c(index, x_id, 150, 200, 200);
                        Axes.Set_speed_ac_de_time_c(index, y_id, 150, 200, 200);
                        Axes.Set_speed_ac_de_time_c(index, left_z, 600, 200, 200);
                        Axes.Set_speed_ac_de_time_c(index, right_z, 300, 200, 200);

                        Pipette.Cpump_zero_p(index, left_tip);
                        Pipette.Cpump_zero_p(index, right_tip);
                    }));

                    DispenserSerialSwitch.Text = "disconnect";
                    DispenserConnectionState.Text = "dispenser connected";
                    check_pipette();
                    reset_dispenser();
                }
                else
                {
                    Response.Text = "connection failed";
                }
            }
        }

        private async void ResetDispensor_Click(object sender, EventArgs e)
        {
            check_pipette();
            reset_dispenser();
            Response.Text = "reset done";
        }

        //reset the dispenser, moving it back to zero point and dropping the tips, but not spitting liquid even if residual
        public async void reset_dispenser()
        {
            Pipette.Back_tip_p(index, left_tip);
            Pipette.Back_tip_p(index, right_tip);
            left_tip_check = 2;
            right_tip_check = 2;

            LeftTipEnable.Enabled = false;
            RightTipEnable.Enabled = false;

            Axes.Zero_c(index, left_z);
            Axes.Zero_c(index, right_z);
            await Task.Delay(3000);
            Axes.Zero_c(index, x_id);
            Axes.Zero_c(index, y_id);

            if (this.Response.InvokeRequired)
            {
                this.Response.Invoke(new Action(() => { Response.Text = "reset done"; }));
            }
            else
            {
                Response.Text = "reset done";
            }
        }

        private void AxesManeuver_Click(object sender, EventArgs e)
        {
            point.x = Convert.ToInt32(X.Text);
            point.y = Convert.ToInt32(Y.Text);
            point.lz = Convert.ToInt32(LeftZ.Text);
            point.rz = Convert.ToInt32(RightZ.Text);
            MovL(point);
            check_pipette();
        }

        public async Task MovL(DKPoint point)
        {
            if (point == null) return;
            else 
            {
                Axes.Motor_Absolute_movement_c(index, x_id, point.x);
                Axes.Motor_Absolute_movement_c(index, y_id, point.y);
                do
                {
                    Axes.Find_status_c(index, y_id, ref x_status);
                    Axes.Find_status_c(index, x_id, ref y_status);
                    await Task.Delay(500);
                } while (x_status != 1 & y_status != 1);

                Axes.Motor_Absolute_movement_c(index, left_z, point.lz);
                Axes.Motor_Absolute_movement_c(index, right_z, point.rz);
                do
                {
                    Axes.Find_status_c(index, left_z, ref left_z_status);
                    Axes.Find_status_c(index, right_z, ref right_z_status);
                    await Task.Delay(500);
                } while (left_z_status != 1 & right_z_status != 1);
            }
        }
        public async Task Reverse_MovL(DKPoint point)
        {
            if (point == null) return;
            else
            {
                Axes.Motor_Absolute_movement_c(index, left_z, point.lz);
                Axes.Motor_Absolute_movement_c(index, right_z, point.rz);
                do
                {
                    Axes.Find_status_c(index, left_z, ref left_z_status);
                    Axes.Find_status_c(index, right_z, ref right_z_status);
                    await Task.Delay(500);
                } while (left_z_status != 1 & right_z_status != 1 );  
                
                Axes.Motor_Absolute_movement_c(index, x_id, point.x);
                Axes.Motor_Absolute_movement_c(index, y_id, point.y);
                do
                {
                    Axes.Find_status_c(index, y_id, ref x_status);
                    Axes.Find_status_c(index, x_id, ref y_status);
                    await Task.Delay(500);
                } while (x_status != 1 & y_status != 1);
            }
        }

        private void CheckTip_Click(object sender, EventArgs e)
        {
            check_pipette();
        }

        //check if tip is on specific pipette
        private void check_pipette()
        {
            Pipette.Find_tip_p(index, left_tip, ref left_tip_check);
            Pipette.Find_tip_p(index, right_tip, ref right_tip_check);
            string response = "";
            int flag = 0;

            if (left_tip_check != 2)
            {
                LeftTipEnable.Enabled = true;
                response += "Left pipette available.";
            }
            else
            {
                LeftTipEnable.Enabled = false;
                flag++;
            }

            if (right_tip_check != 2)
            {
                RightTipEnable.Enabled = true;
                response += "Right pipette available.";
            }
            else
            {
                RightTipEnable.Enabled = false;
                flag++;
            }
            if (flag == 2)
            {
                response = "No pipettes available";
            }
            Response.Text = response;
        }

        private void LeftTipSuck_Click(object sender, EventArgs e)
        {
            byte return_value = 0;
            Int16 volume = Convert.ToInt16(LeftTipVolume.Text);
            Pipette.Suck_p(index, left_tip, volume, ref return_value);
        }

        private void LeftTipSpit_Click(object sender, EventArgs e)
        {
            byte return_value = 0;
            Int16 volume = Convert.ToInt16(LeftTipVolume.Text);
            Pipette.Spit_p(index, left_tip, volume, ref return_value);
        }

        private void LeftTipDrop_Click(object sender, EventArgs e)
        {
            Pipette.Back_tip_p(index, left_tip);
            left_tip_check = 2;
            LeftTipEnable.Enabled = false;
        }

        private void RightTipSuck_Click(object sender, EventArgs e)
        {
            byte return_value = 0;
            Int16 volume = Convert.ToInt16(RightTipVolume.Text);
            Pipette.Suck_p(index, right_tip, volume, ref return_value);
        }

        private void RightTipSpit_Click(object sender, EventArgs e)
        {
            byte return_value = 0;
            Int16 volume = Convert.ToInt16(RightTipVolume.Text);
            Pipette.Spit_p(index, right_tip, volume, ref return_value);
        }

        private void RightTipDrop_Click(object sender, EventArgs e)
        {
            Pipette.Back_tip_p(index, right_tip);
            right_tip_check = 2;
            RightTipEnable.Enabled = false;
        }
    }
}
