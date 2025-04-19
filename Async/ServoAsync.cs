using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WinFormsApp_Draft.Async
{
    internal class ServoAsync
    {
        public async Task MoveAwayAsync(CheckBox moveaway, SerialPort servo_port)
        {
            const string move_away = "1";
            const string move_back = "2";

            var task_start = new TaskCompletionSource<bool>();

            if (moveaway.Checked)
            {
                servo_port.Write(move_away);
            }
            else
            {
                servo_port.Write(move_back);
            }
          
            await task_start.Task;
        }
    }
}
