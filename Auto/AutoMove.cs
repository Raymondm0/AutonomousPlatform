using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpTcpDemo;
using CSharpTcpDemo.com.dobot.api;
using CSharthiscpDemo.com.dobot.api;

namespace WinFormsApp_Draft.Auto
{
    class AutoMove
    {
        public async Task Arm_Running()
        {
            while (true)
            {
                byte run_state = ArmForm.running;
                await Task.Delay(500);
                if(ArmForm.running == 0)
                {
                    break;
                }
            }
            return;
        }

        public async Task Dispenser_Running()
        {
            //while (true)
            //{
                
            //}
            
            return;
        }
    }
}
