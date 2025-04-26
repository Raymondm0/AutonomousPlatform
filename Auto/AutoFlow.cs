using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using CSharpTcpDemo.com.dobot.api;
using Modbus.Device;
using WinFormsApp_Draft;
using WinFormsApp_Draft.Auto;

namespace WinFormsApp_Draft.Auto
{
    class AutoFlow
    {
        public static bool coater_running_state = false;
        public static bool arm_running_state = false;

        private AutoMove AutoMove;
        private AutoSpin autoSpin = new AutoSpin();
        private ExcelReader ExcelReader;
        private System.Timers.Timer flow_timer = new System.Timers.Timer();

        private async Task EnableCoater(int round_num, IModbusMaster master)
        {
            System.Timers.Timer spin_timer = new System.Timers.Timer();
            ExcelReader excelReader = new ExcelReader();

            List<string> round_params = excelReader.GetRowData(round_num);
            int speed = Convert.ToInt32(round_params[0]);
            int duration = Convert.ToInt32(round_params[1]);
            int acc_speed = Convert.ToInt32(round_params[2]);

            await autoSpin.SendParamsAsync(duration, speed, acc_speed, spin_timer, master);
        }

        private async Task EnableArm()
        {

        }

        // Operate different tasks between coater and arm. Lock EnableCoater with EnableArm to a single thread
        public async Task OperateTasks(IModbusMaster master ,CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100);
           
            }
        }
    }
}
