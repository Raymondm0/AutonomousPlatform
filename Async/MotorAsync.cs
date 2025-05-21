using Modbus.Device;
using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;


namespace WinFormsApp_Draft.Async
{
    internal class MotorAsync
    {
        //start driver heart beat 
        public async Task StartBeatAsync(IModbusMaster master ,CancellationToken cancellationToken)
        {
            byte slaveID = 0x01;
            ushort stopAddress = 0x177E;
            ushort modeAddress = 0x1771;
            ushort beatAddress = 0x1770;
            master.WriteSingleRegister(slaveID, stopAddress, 0x0000);
            master.WriteSingleRegister(slaveID, modeAddress, 0x0006);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    master.WriteSingleRegister(slaveID, beatAddress, 0x0001);
                    master.WriteSingleRegister(slaveID, beatAddress, 0x0002);
                    await Task.Delay(10);
                }
                catch { }
            }
        }

        //freely stop the motor
        public async Task FreeStopAsync(Button freestop, IModbusMaster master)
        {
            var task_start = new TaskCompletionSource<bool>();
            EventHandler handler = (sender, e) =>
            {
                byte slaveID = 0x01;
                ushort stopAddress = 0x177E;
                ushort modeAddress = 0x1771;
                master.WriteSingleRegister(slaveID, stopAddress, 0x0000);
                master.WriteSingleRegister(slaveID, modeAddress, 0x0006);

                task_start.SetResult(true);
            };

            freestop.Click += handler;
            await task_start.Task;
            freestop.Click -= handler;
        }

        //stop the motor with brutal force
        public async Task ForceStopAsync(Button forcestop, IModbusMaster master)
        {
            var task_start = new TaskCompletionSource<bool>();
            EventHandler handler = (sender, e) =>
            {
                byte slaveID = 0x01;
                ushort stopAddress = 0x177E;
                ushort modeAddress = 0x1771;
                master.WriteSingleRegister(slaveID, stopAddress, 0x0064);
                master.WriteSingleRegister(slaveID, modeAddress, 0x0006);
            
                task_start.SetResult(true);
            };

            forcestop.Click += handler;
            await task_start.Task;
            forcestop.Click -= handler;   
        }

        //set motor's zero point
        public async Task ClearPosAsync(Button clearpos, IModbusMaster master)
        {
            var task_start = new TaskCompletionSource<bool>();
            EventHandler handler = (sender, e) =>
            {
                byte slaveID = 0x01;
                ushort registerAddress_write = 0x177c;
                ushort[] val = { 0x0000, 0x0000 };
                master.WriteMultipleRegisters(slaveID, registerAddress_write, val);

                task_start.SetResult(true);    
            };

            clearpos.Click += handler;
            await task_start.Task;
            clearpos.Click -= handler;
        }
 
        //reset motor to original position
        public async Task ResetMotorAsync(Button reset, IModbusMaster master, int spin_speed = 14000)
        {
            var task_start = new TaskCompletionSource<bool>();
            EventHandler handler = null;
            handler = (sender, e) =>
            {
                byte slaveID = 0x01;
                ushort modeAddress = 0x1771;
                ushort stopAddress = 0x177E;
                ushort cur_posAddress = 0x1392;
                ushort resetAddress = 0x1776;

                master.WriteSingleRegister(slaveID, stopAddress, 0x0000);
                master.WriteSingleRegister(slaveID, modeAddress, 0x0006);
                Task.Delay(spin_speed / 7);

                ushort[] pos = master.ReadInputRegisters(slaveID, cur_posAddress, 2);
                int high = pos[0];
                int low = pos[1];
                int cur_pos = ((high << 16) + low) / 100 + 1; ;

                int reset_pos = 100 * (cur_pos - (cur_pos % 360));
                ushort high_convert = (ushort)(reset_pos >> 16);
                ushort low_convert = (ushort)reset_pos;
                ushort[] reset_to = { high_convert, low_convert };

                master.WriteSingleRegister(slaveID, modeAddress, 0xFFFF);
                master.WriteMultipleRegisters(slaveID, resetAddress, reset_to);
                master.WriteSingleRegister(slaveID, modeAddress, 0x0003);

                task_start.SetResult(true);
            };

            reset.Click += handler;
            await task_start.Task;
            reset.Click -= handler;
        }

        //show motor position at live
        public async Task UpdatePositionAsync(CheckBox checkBox, TextBox textBox, IModbusMaster master, CancellationToken cancellationToken)
        {
            while (checkBox.Checked)
            {
                try
                {
                    textBox.Invoke((MethodInvoker)delegate
                    {
                        byte slaveID = 0x01;
                        ushort posAddress = 0x1392;
                        ushort[] pos = master.ReadInputRegisters(slaveID, posAddress, 2);
                        int high = pos[0];
                        int low = pos[1];
                        int cur_pos = ((high << 16) + low) / 100 + 1;
                        textBox.Text = Convert.ToString(cur_pos);
                    });
                    await Task.Delay(10);    
                }
                catch { }
            }
        }
    }
}
