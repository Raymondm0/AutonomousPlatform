using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DocumentFormat.OpenXml.Presentation;
using Modbus.Device;
using WinFormsApp_Draft;

namespace WinFormsApp_Draft.Auto
{
    class AutoSpin
    {
        private async Task SendSpeedAsync(int spin_speed, IModbusMaster master)
        {
            byte slaveID = 0x01;
            ushort modeAddress = 0x1771;
            
            ushort speedAddress = 0x1773;
            ushort high = (ushort)(spin_speed >> 16);
            ushort low = (ushort)spin_speed;
            ushort[] speed_erpm = { high, low };

            master.WriteMultipleRegisters(slaveID, speedAddress, speed_erpm);
            master.WriteSingleRegister(slaveID, modeAddress, 0x0001);
        }

        private async Task SendAccAsync(int acc_speed, IModbusMaster master)
        {
            byte slaveID = 0x01;
            ushort modeAddress = 0x1771;

            ushort accAddress = 0x1780;
            int acc = acc_speed * 7;
            ushort high = (ushort)(acc >> 16);
            ushort low = (ushort)acc;
            ushort[] acc_erpm = { high, low };

            master.WriteMultipleRegisters(slaveID, accAddress, acc_erpm);
            master.WriteSingleRegister(slaveID, modeAddress, 0x0001);
        }

        public async Task SendParamsAsync(int spin_duration,int spin_speed, int acc_speed, System.Timers.Timer timer,IModbusMaster master)
        {
            var task_start = new TaskCompletionSource<bool>();
            timer.Elapsed += async (sender, e) => 
            {
                byte slaveID = 0x01;
                ushort modeAddress = 0x1771;
                ushort stopAddress = 0x177E;
                ushort cur_posAddress = 0x1392;
                ushort resetAddress = 0x1776;

                master.WriteSingleRegister(slaveID, stopAddress, 0x0000);
                master.WriteSingleRegister(slaveID, modeAddress, 0x0006);
                await Task.Delay(spin_speed / 7);

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
                AutoFlow.coater_running_state = false;
                timer.Stop();
                timer.Dispose();
            };
            timer.Interval = spin_duration * 1000;
            timer.AutoReset = false;

            await SendAccAsync(acc_speed, master);
            await SendSpeedAsync(spin_speed, master);
            timer.Start();
            AutoFlow.coater_running_state = true;
        }

        public async Task<bool> CoaterRunningState(IModbusMaster master)
        {
            byte slaveID = 0x01;
            ushort posAddress = 0x1392;

            int prev_pos;
            int cur_pos;
            prev_pos = get_pos(master);
            await Task.Delay(500);
            cur_pos = get_pos(master);

            if (cur_pos != prev_pos)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static int get_pos(IModbusMaster master)
        {
            byte slaveID = 0x01;
            ushort posAddress = 0x1392;
            ushort[] pos = master.ReadInputRegisters(slaveID, posAddress, 2);
            int high = pos[0];
            int low = pos[1];
            int cur_pos = ((high << 16) + low) / 100 + 1;
            return cur_pos;
        }
    }
}
