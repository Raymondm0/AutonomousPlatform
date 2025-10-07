using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Timers;
using Modbus.Device;
using Newtonsoft.Json;
using CSharpTcpDemo;
using CSharpTcpDemo.com.dobot.api;
using CSharthiscpDemo.com.dobot.api;
using WinFormsApp_Draft.Auto;
using WinFormsApp_Draft.DK;
using DocumentFormat.OpenXml.Drawing;
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
        
        private static List<List<int>> exp_rounds = new List<List<int>>();
        private static List<List<int>> features = new List<List<int>>();
        private static List<string> free_slides = new List<string>();
        private static List<string> free_tips = new List<string>();
        private static Dictionary<string, List<int>> reagents = new Dictionary<string, List<int>>();

        public MainForm()
        {
            InitializeComponent();
            
            Refresh.Click += armForm.ArmForm_Load;
            Refresh.Click += dispenserForm.DispenserForm_Load;
            Refresh.Click += coaterForm.CoaterForm_Load;
            Refresh.Click += MainForm_Load;

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

        private void MainForm_Load(object sender, EventArgs e)
        {
            exp_rounds.Clear();
            free_slides.Clear();
            free_tips.Clear();
            reagents.Clear();
            ReagentLayout.Controls.Clear();
            ExperimentParameters.Items.Clear();
            AutoRead.Checked = false;
            Method.Enabled = false;

            init_btns(3, 4);
            if(!Method.Contains(ReagentLayout))Method.Controls.Add(ReagentLayout);

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

        //layout of the reagent bottles
        private void init_btns(int row_n, int col_n)
        {
            ReagentLayout.ColumnCount = col_n;
            ReagentLayout.RowCount = row_n;
            set_size(row_n, col_n);

            Button[,] buttons = new Button[row_n, col_n];

            for (int i = 0; i < row_n; i++)
            {
                for (int j = 0; j < col_n; j++)
                {
                    buttons[i, j] = new Button();
                    buttons[i, j].Text = $"RP{i + 1}{j + 1}";
                    buttons[i, j].Dock = DockStyle.Fill;
                    buttons[i, j].BackColor = Color.Red;

                    ReagentLayout.Controls.Add(buttons[i, j], j, i);
                    buttons[i, j].Click += (sender, e) =>
                    {
                        Button button = (Button)sender;
                        add_reagent(button);
                    };
                }
            }
        }

        private void set_size(int row_n, int col_n)
        {
            ReagentLayout.ColumnStyles.Clear();
            ReagentLayout.RowStyles.Clear();
            for (int i = 0; i < col_n; i++)
            {
                ReagentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / col_n));
            }
            for (int j = 0; j < row_n; j++)
            {
                ReagentLayout.RowStyles.Add(new ColumnStyle(SizeType.Percent, 100F / row_n));
            }
        }

        private async void add_reagent(Button button)
        {
            if (button.BackColor == Color.Red)
            {
                button.BackColor = Color.Yellow;
                ReagentFeatures.SelectedIndexChanged += selecting;
                int index = 0;

                while (true)
                {
                    if (ReagentFeatures.BackColor == Color.Yellow)
                    {
                        index = ReagentFeatures.SelectedIndex;

                        ReagentFeatures.BackColor = Color.White;
                        ReagentFeatures.SelectedIndexChanged -= selecting;
                        break;
                    }
                    await Task.Delay(100);
                }

                reagents.Add(button.Text, features[index]);
                button.BackColor = Color.ForestGreen;
                Response.Text = button.Text + System.String.Format("added, correlated with feature{0}", index);
            }
            else
            {
                button.BackColor = Color.Red;
                reagents.Remove(button.Text);
                Response.Text = button.Text + "removed";
            }
        }

        private void selecting(object sender, EventArgs e)
        {
            ReagentFeatures.BackColor = Color.Yellow;
        }

        // auto read functions, complete automatic
        private void AutoRead_CheckedChanged(object sender, EventArgs e)
        {
            if (AutoRead.Checked)
            {
                try
                {
                    int i = 1, x = 0;
                    while (true)
                    {
                        //test data reading process
                        List<int> row_paramerts = readExcel.row_param(i, 3, FilePath.Text);
                        if (row_paramerts[0] != 0)
                        {
                            exp_rounds.Add(row_paramerts);
                            i++;
                            string round_params = "";
                            for (int j = 0; j < row_paramerts.Count(); j++)
                            {
                                if (j < row_paramerts.Count() - 1)
                                {
                                    round_params += row_paramerts[j].ToString() + ",";
                                }
                                else
                                {
                                    round_params += row_paramerts[j].ToString();
                                }
                            }
                            ExperimentParameters.Items.Add(round_params);
                        }
                        else
                        {
                            break;
                        }

                        if (exp_rounds.Count > 0)
                        {
                            Method.Enabled = true;
                        }
                        else
                        {
                            Method.Enabled = false;
                        }
                    }
                    while (true)
                    {
                        List<int> col_paramerts = readExcel.col_param(x, i - 1, FilePath.Text);
                        if (col_paramerts[0] != 0)
                        {
                            features.Add(col_paramerts);
                            x++;
                            string round_params = "";
                            for (int y = 0; y < col_paramerts.Count(); y++)
                            {
                                if (y < col_paramerts.Count() - 1)
                                {
                                    round_params += col_paramerts[y].ToString() + ",";
                                }
                                else
                                {
                                    round_params += col_paramerts[y].ToString();
                                }
                            }
                            ReagentFeatures.Items.Add(round_params);
                        }
                        else
                        {
                            break;
                        }
                    }
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
                ExperimentParameters.Items.Clear();
                ReagentFeatures.Items.Clear();
                Method.Enabled = false;
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
        /// if "pick at tray", arm will descend to slide tray and grip, if "release at tray", vice versa.
        /// if "pick at coater" or "release at coater", similar to slides, but will descend to the height of the coater.
        /// otherwise, gripper will do nothing, only arms will move
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

            if (gripper == "pick at tray" && free_slides.Contains(point))
            {
                arm_pt.z = 66.5;
                await armForm.MovL(arm_pt);
                await armForm.Grip(13);
                arm_pt.z = 200;
                await armForm.MovL(arm_pt);
                free_slides.Remove(point);  
            }
            else if (gripper == "release at tray" && !free_slides.Contains(point))
            {
                arm_pt.z = 67;
                await armForm.MovL(arm_pt);
                await armForm.Release(13);
                arm_pt.z = 200;
                await armForm.MovL(arm_pt);
                free_slides.Add(point);
            }
            else if(gripper == "pick at coater")
            {
                arm_pt.z = 111.5;
                await armForm.MovL(arm_pt);
                await armForm.Grip(13);
                arm_pt.z = 200;
                await armForm.MovL(arm_pt);
            }
            else if (gripper == "release at coater")
            {
                arm_pt.z = 112;
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
            try
            {
                Response.Text = reagents["RP11"][0].ToString();
            }
            catch(Exception ex) 
            {
                Response.Text = ex.Message;
            }
            //foreach (string point in free_slides)
            //{
            //    Response.Text += point;
            //}
            //await arm_MovL("Calibrate");
            //await arm_MovL("Zero");

            //test platform experiment process
            //await arm_MovL("Calibrate", "release at tray");

            //await arm_MovL("P44", "pick at tray");
            //Response.Text += "Gripping start.";
            //await arm_MovL("Coater", "release at coater");

            //await coaterForm.Spin_Coat(2000, 1000, 4);
            //Response.Text += "Coating done. ";

            //await arm_MovL("Coater", "pick at coater");
            //await arm_MovL("P44", "release at tray");
            //Response.Text += "Gripping done. ";

            //await arm_MovL("Calibrate");
            //await arm_MovL("Zero");
            //Response.Text += "Moving done. ";

            //await dispenser_MovL("P1");// (6500,21500,170000,0)
            //await dispenserForm.Tip_Suck(100);

            //await dispenser_MovL("P2", 2);// (6500,21500,0,0)

            //await dispenser_MovL("P3");// (6250,13500,100000,0)
            //await dispenserForm.Tip_Spit(100);

            //await dispenser_MovL("Zero", 2);
            //Response.Text += "Dispensing liquid done. ";


        }
    }
}
