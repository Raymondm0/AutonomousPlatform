using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsApp_Draft.Async;

namespace WinFormsApp_Draft
{
    public partial class CoaterForm : Form
    {
        //coater declarations
        private MotorAsync motorAsync = new MotorAsync();
        private static int spin_speed = 0;
        private System.Timers.Timer spin_timer = new System.Timers.Timer();
        private CancellationTokenSource cancellationTokenSource_pos;
        private CancellationTokenSource cancellationTokenSource_beat;
        public static bool coater_connect_state = false;

        public CoaterForm()
        {
            InitializeComponent();
        }

        private void CoaterForm_Load(object sender, EventArgs e)
        {

        }
    }
}
