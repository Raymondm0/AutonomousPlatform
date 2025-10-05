using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Timers;
using Modbus.Device;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using CSharpTcpDemo;
using CSharpTcpDemo.com.dobot.api;
using CSharthiscpDemo.com.dobot.api;
using WinFormsApp_Draft.Auto;
using WinFormsApp_Draft.DK;
using DocumentFormat.OpenXml.Drawing.Diagrams;
namespace WinFormsApp_Draft
{
    public partial class MainForm : Form
    {
        //general declarations
        private SerialPort motor_port = new SerialPort();
        private IModbusMaster master;

        //coater declarations
        private static int spin_speed = 0;
        private System.Timers.Timer spin_timer = new System.Timers.Timer();
        private CancellationTokenSource cancellationTokenSource_pos;
        private CancellationTokenSource cancellationTokenSource_beat;

        //subforms
        private ArmForm armForm = new ArmForm();
        private CoaterForm coaterForm = new CoaterForm();
        private DispenserForm dispenserForm = new DispenserForm();

        //auto declarations
        private ReadExcel readExcel = new ReadExcel();

        private const string armPath = "ArmPoints.json";
        private static string arm_json = File.ReadAllText(armPath);
        private ArmConfig? arm_conf = JsonConvert.DeserializeObject<ArmConfig>(arm_json);

        private const string dispenserPath = "DispenserPoints.json";
        private static string dispenser_json = File.ReadAllText(dispenserPath);
        private DispenserConfig? dispenser_conf = JsonConvert.DeserializeObject<DispenserConfig>(dispenser_json);
        
        private static Queue<List<int>> exp_parameters = new Queue<List<int>>(); 
        private static List<string> free_slides = new List<string>();
        private static List<string> free_tips = new List<string>();

        public MainForm()
        {
            InitializeComponent();
            Refresh.Click += armForm.ArmForm_Load;
            Refresh.Click += dispenserForm.DispenserForm_Load;
            Refresh.Click += coaterForm.CoaterForm_Load;

            foreach(string pos in arm_conf.Points.Keys)
            {
                if (pos.StartsWith('P'))
                {
                    free_slides.Add(pos);
                }
            }
            foreach(string pos in dispenser_conf.Points.Keys)
            {
                if (pos.StartsWith('P'))
                {
                   free_tips.Add(pos); 
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            new TipLayout().init_btns(8, 12, TipLayoutPanel, Response);

            string strPath = System.Windows.Forms.Application.StartupPath + "\\";
            ErrorInfoHelper.ParseControllerJsonFile(strPath + "alarm_controller.json");
            ErrorInfoHelper.ParseServoJsonFile(strPath + "alarm_servo.json");

            FilePath.Text = "C:\\Users\\DELL\\Desktop\\readtest.csv";
            SheetName.Text = "Sheet1";

            armForm.TopLevel = false;
            coaterForm.TopLevel = false;
            dispenserForm.TopLevel = false;
        }

        private void Refresh_Click(object sender, EventArgs e)
        {

        }

        // auto read functions, complete automatic
        private async void AutoRead_CheckedChanged(object sender, EventArgs e)
        {
            if (AutoRead.Checked)
            {
                try
                {
                    
                }
                catch (Exception ex)
                {
                    Response.Text = ex.Message;
                }
            }
            else
            {
                if (armForm.arm_enable_state)
                {
                    armForm.EnableWindow();
                }
                if (coaterForm.coater_connect_state)
                {
                    coaterForm.EnableCoater();
                }
                Response.Clear();
                ShowData.Clear();
            }
        }

        private void OpenArmForm_Click(object sender, EventArgs e)
        {
            Open_Form(armForm);
        }

        private void OpenCoaterForm_Click(object sender, EventArgs e)
        {
            Open_Form(coaterForm);
        }
        private void OpenDispenserForm_Click(object sender, EventArgs e)
        {
            Open_Form(dispenserForm);
        }

        private void Open_Form(Form form)
        {
            if (ControlPanel.Controls.Contains(form))
            {
                form.BringToFront();
            }
            else
            {
                form.FormBorderStyle = FormBorderStyle.None;
                SetWindowSize(form);
                ControlPanel.Controls.Add(form);
                form.Show();
                form.BringToFront();
            }
        }

        private void SetWindowSize(Form form)
        {
            double x = this.ControlPanel.Width / Convert.ToSingle(form.Width);
            double y = this.ControlPanel.Height / Convert.ToSingle(form.Height);
            if (form.Controls.Count > 0)
            {
                foreach (System.Windows.Forms.Control control in form.Controls)
                {
                    if (control.Controls.Count > 0)
                    {
                        foreach (System.Windows.Forms.Control c in control.Controls)
                        {
                            c.Width = Convert.ToInt32(c.Width * x);
                            c.Height = Convert.ToInt32(c.Height * y);
                            c.Left = Convert.ToInt32(c.Left * x);
                            c.Top = Convert.ToInt32(c.Top * y);
                        }
                    }
                    control.Width = Convert.ToInt32(control.Width * x);
                    control.Height = Convert.ToInt32(control.Height * y);
                    control.Left = Convert.ToInt32(control.Left * x);
                    control.Top = Convert.ToInt32(control.Top * y);
                }
            }
            form.Width = Convert.ToInt32(form.Width * x);
            form.Height = Convert.ToInt32(form.Height * y);
        }

        //test arm and dispenser, do group move round
        private class ArmConfig
        {
            public Dictionary<string, DescartesPoint> Points { get; set; } = new Dictionary<string, DescartesPoint>();
        }
        private class DispenserConfig
        {
            public Dictionary<string, DKPoint> Points { get; set; } = new Dictionary<string, DKPoint>();
        }

        /// <summary>
        /// pass in point name(string); if direction is 1(default), will move first horizontal then verticle; if 2, vice versa.
        /// otherwise, will do nothing
        /// </summary>
        /// <param name="point"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private async Task dispenser_MovL(string point,int direction = 1)
        {
            DKPoint dispenser_pt = new DKPoint();
            dispenser_conf.Points.TryGetValue(point, out dispenser_pt);
            if (direction == 1)
            {
                await dispenserForm.MovL(dispenser_pt);  
            }
            else if(direction == 2)
            {
                await dispenserForm.reverse_MovL(dispenser_pt);
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// pass in point name(string) where there should be free tips.
        /// which tip to use must be indicated in the points' configure.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="tip"></param>
        /// <returns></returns>
        private async Task dispenser_tip(string point)
        {
            DKPoint dispenser_pt = new DKPoint();
            dispenser_conf.Points.TryGetValue(point, out dispenser_pt);
            if (free_tips.Contains(point))
            {
                await dispenserForm.MovL(dispenser_pt);
                dispenser_pt.rz = 0;
                dispenser_pt.lz = 0;
                await dispenserForm.reverse_MovL(dispenser_pt);
                free_tips.Remove(point);
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// pass in point name(string); if gripper is "none"(default), gripper won't move.
        /// if "pick tray", arm will descend to slide tray and grip, if "release tray", vice versa.
        /// if "pick coater" or "release coater", similar to slides, but will descend to the height of the coater.
        /// otherwise, will do nothing
        /// </summary>
        /// <param name="point"></param>
        /// <param name="gripper"></param>
        /// <returns></returns>
        private async Task arm_MovL(string point, string gripper = "none")
        {
            if (free_slides.Count == 0)
            {
                return;
            }
            DescartesPoint arm_pt = new DescartesPoint();
            arm_conf.Points.TryGetValue(point, out arm_pt);
            await armForm.MovL(arm_pt);

            if (gripper == "pick tray" && free_slides.Contains(point))
            {
                arm_pt.z = 66;
                await armForm.MovL(arm_pt);
                await armForm.Grip(13);
                arm_pt.z = 200;
                await armForm.MovL(arm_pt);
                free_slides.Remove(point);  
            }
            else if (gripper == "release tray" && !free_slides.Contains(point))
            {
                arm_pt.z = 67;
                await armForm.MovL(arm_pt);
                await armForm.Release(13);
                arm_pt.z = 200;
                await armForm.MovL(arm_pt);
                free_slides.Add(point);
            }
            else if(gripper == "pick coater")
            {
                arm_pt.z = 100;
                await armForm.MovL(arm_pt);
                await armForm.Grip(13);
                arm_pt.z = 200;
                await armForm.MovL(arm_pt);
            }
            else if (gripper == "release coater")
            {
                arm_pt.z = 100;
                await armForm.MovL(arm_pt);
                await armForm.Release(13);
                arm_pt.z = 200;
                await armForm.MovL(arm_pt);
            }
            else
            {
                return;
            }
        }

        private async void MoveTest_Click(object sender, EventArgs e)
        {
            foreach (string point in free_slides)
            {
                Response.Text += point;
            }
            //await arm_MovL("Calibrate");
            //await arm_MovL("Zero");

            //test data reading process
            //List<int> paramerts = readExcel.row_param(2, 3, "C:\\Users\\DELL\\Desktop\\readtest.csv");
            //foreach (int i in paramerts)
            //{
            //    Response.Text += i.ToString();
            //}

            //test platform experiment process
            await arm_MovL("Calibrate");

            await arm_MovL("P53", "pick slide");
            Response.Text += "Gripping start.";

            await arm_MovL("P11", "release slide");
            Response.Text += "Gripping done. ";

            await arm_MovL("Calibrate");
            await arm_MovL("Zero");
            Response.Text += "Moving done. ";

            await dispenser_MovL("P1");// (6500,21500,170000,0)
            await dispenserForm.Tip_Suck(100);

            await dispenser_MovL("P2", 2);// (6500,21500,0,0)

            await dispenser_MovL("P3");// (6250,13500,100000,0)
            await dispenserForm.Tip_Spit(100);

            await dispenser_MovL("Zero", 2);
            Response.Text += "Dispensing liquid done. ";

            //await coaterForm.Spin_Coat(4000, 1000, 6);
            //Response.Text += "Coating done. ";
        }
    }
}
