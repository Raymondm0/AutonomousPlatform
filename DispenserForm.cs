using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using WinFormsApp_Draft.DK;

namespace WinFormsApp_Draft
{
    public partial class DispenserForm : Form
    {
        private Axes axes_movement = new Axes();
        private Pipette pip_action = new Pipette();
        private static bool port_opened = false;

        public DispenserForm()
        {
            InitializeComponent();
        }

        private void DispenserForm_Load(object sender, EventArgs e) { }

        private void DispenserSerialSwitch_Click(object sender, EventArgs e)
        {
            byte index = 9;
            uint baudrate = 115200;
            const byte x_id = 5, y_id = 6;
            byte x_status = 0, y_status = 0;

            if (port_opened)
            {
                int pos = 0;
                Axes.Motor_Absolute_movement_c(index, x_id, pos);
                Axes.Motor_Absolute_movement_c(index, y_id, pos);

                Axes.Find_status_c(index, y_id, ref y_status);
                Axes.Find_status_c(index, x_id, ref x_status);

                while (x_status != 1 & y_status != 1)
                {
                    Axes.Find_status_c(index, y_id, ref y_status);
                    Axes.Find_status_c(index, x_id, ref x_status);
                }

                Axes.Enable_motor_c(index, x_id, 0);
                Axes.Enable_motor_c(index, y_id, 0);

                int response = Axes.CloseComPort(index);
                Response.Text += Convert.ToString(response);
                port_opened = false;
            }
            else
            {
                int response = Axes.OpenComPort(index, baudrate);
                Response.Text += Convert.ToString(response);
                port_opened = true;

                Axes.Enable_motor_c(index, x_id, 1);
                Axes.Enable_motor_c(index, y_id, 1);
                int pos = 4096;
                Axes.Motor_Absolute_movement_c(index, x_id, pos);
                Axes.Motor_Absolute_movement_c(index, y_id, pos);
            }
        }
    }
}
