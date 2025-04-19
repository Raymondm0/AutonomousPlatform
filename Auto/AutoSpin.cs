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
        /// <summary>
        /// auto send request to change parameters to driver; check whether parameter available before calling
        /// </summary>
        /// <param name="spin_speed"></param>
        /// <param name="master"></param>
        /// <returns></returns>
        public async Task SendSpeedAsync(int spin_speed, IModbusMaster master)
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

        public async Task SendAccAsync(int acc_speed, IModbusMaster master)
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

        public async Task SendDurationAsync(int spin_duration, System.Timers.Timer timer, IModbusMaster master)
        {
            //timer.Elapsed += new ElapsedEventHandler(FreeStop_timer);
            timer.Elapsed += (sender, e) => 
            {
                byte slaveID = 0x01;
                ushort stopAddress = 0x177E;
                ushort modeAddress = 0x1771;
                master.WriteSingleRegister(slaveID, stopAddress, 0x0000);
                master.WriteSingleRegister(slaveID, modeAddress, 0x0006);
                timer.Stop();
            };
            timer.Interval = spin_duration * 1000;
            timer.AutoReset = false;
            timer.Start();
        }

        public async Task CoaterRunningState(IModbusMaster master)
        {
            byte slaveID = 0x01;
            ushort posAddress = 0x1392;

            int prev_pos;
            int cur_pos;
            prev_pos = get_pos(master);
            await Task.Delay(1000);
            cur_pos = get_pos(master);

            if (cur_pos != prev_pos)
            {
                WinFormsApp_Draft.Form1.coater_running_state = true;
            }
            else
            {
                WinFormsApp_Draft.Form1.coater_running_state = false;
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
