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
using WinFormsApp_Draft.Async;
using WinFormsApp_Draft.Auto;
using WinFormsApp_Draft.DK;
namespace WinFormsApp_Draft
{
    public partial class MainForm : Form
    {
        //general declarations
        private SerialPort motor_port = new SerialPort();
        private IModbusMaster master;

        //coater declarations
        private MotorAsync motorAsync = new MotorAsync();
        private static int spin_speed = 0;
        private System.Timers.Timer spin_timer = new System.Timers.Timer();
        private CancellationTokenSource cancellationTokenSource_pos;
        private CancellationTokenSource cancellationTokenSource_beat;

        //subforms
        private ArmForm armForm = new ArmForm();
        private CoaterForm coaterForm = new CoaterForm();
        private DispenserForm dispenserForm = new DispenserForm();
        private AutoMove autoMove = new AutoMove();

        //auto declarations
        private ExcelReader mExcelReader = new ExcelReader();
        public static Workbook mWorkbook;
        public static SharedStringTable mSharedStringTable;
        public static WorkbookPart mWorkbookPart;
        private static Sheet mSheet;
        public static Worksheet mWorksheet;
        public static List<string>? param_names;
        public static List<List<string>>? param_list;

        public static sheet_properties properties;
        public struct sheet_properties
        {
            public int parameters { get; init; }
            public int rounds { get; set; }
        }

        public MainForm()
        {
            InitializeComponent();
            Refresh.Click += armForm.ArmForm_Load;
            Refresh.Click += dispenserForm.DispenserForm_Load;
            Refresh.Click += coaterForm.CoaterForm_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            new TipLayout().init_btns(8, 12, TipLayoutPanel, Response);

            string strPath = System.Windows.Forms.Application.StartupPath + "\\";
            ErrorInfoHelper.ParseControllerJsonFile(strPath + "alarm_controller.json");
            ErrorInfoHelper.ParseServoJsonFile(strPath + "alarm_servo.json");

            FilePath.Text = "C:\\Users\\DELL\\Desktop\\test.xlsx";
            SheetName.Text = "Sheet1";

            armForm.TopLevel = false;
            coaterForm.TopLevel = false;
            dispenserForm.TopLevel = false;
        }

        private void Refresh_Click(object sender, EventArgs e){ }

        // auto read functions, complete automatic
        private async void AutoRead_CheckedChanged(object sender, EventArgs e)
        {
            if (AutoRead.Checked)
            {
                try
                {
                    using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(FilePath.Text, false))
                    {
                        WorkbookPart mWorkbookPart = spreadsheetDocument.WorkbookPart;
                        mWorkbook = mWorkbookPart.Workbook;

                        if (mWorkbookPart != null)
                        {
                            mSharedStringTable = mWorkbookPart.SharedStringTablePart.SharedStringTable;

                            mSheet = mWorkbook.Sheets.Elements<Sheet>().FirstOrDefault(s => s.Name == SheetName.Text);
                            WorksheetPart worksheetPart = (WorksheetPart)mWorkbookPart.GetPartById(mSheet.Id);
                            mWorksheet = worksheetPart.Worksheet;

                            param_names = new List<string>();
                            param_list = new List<List<string>>();

                            mExcelReader.init_Properties();
                            mExcelReader.init_ParamNames();
                            for (int i = 1; i <= properties.rounds; i++)
                            {
                                param_list.Add(mExcelReader.GetRowData(i));
                            }
                            //mExcelReader.printData(ShowData);
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

        private async void MoveTest_Click(object sender, EventArgs e)
        {
            string armPath = "ArmPoints.json";
            string arm_json = File.ReadAllText(armPath);
            ArmConfig? arm_conf = JsonConvert.DeserializeObject<ArmConfig>(arm_json);

            string dispenserPath = "DispenserPoints.json";
            string dispenser_json = File.ReadAllText(dispenserPath);
            DispenserConfig? dispenser_conf = JsonConvert.DeserializeObject<DispenserConfig>(dispenser_json);

            DescartesPoint arm_pt = new DescartesPoint();
            DKPoint dispenser_pt = new DKPoint();

            await armForm.Grip(13);
            Response.Text += "Gripping start.";

            arm_conf.Points.TryGetValue("Zero", out arm_pt);
            await armForm.MovL(arm_pt);
            arm_conf.Points.TryGetValue("P3", out arm_pt);
            await armForm.MovL(arm_pt);
            arm_conf.Points.TryGetValue("Zero", out arm_pt);
            await armForm.MovL(arm_pt);
            Response.Text += "Moving done. ";

            await armForm.Release(13);
            Response.Text += "Gripping done. ";

            dispenser_conf.Points.TryGetValue("P1", out dispenser_pt);// (6500,21500,170000,0)
            await dispenserForm.MovL(dispenser_pt);
            await dispenserForm.Tip_Suck(100);

            dispenser_conf.Points.TryGetValue("P2", out dispenser_pt);// (6500,21500,0,0)
            await dispenserForm.reverse_MovL(dispenser_pt);

            dispenser_conf.Points.TryGetValue("P3", out dispenser_pt);// (6250,13500,100000,0)
            await dispenserForm.MovL(dispenser_pt);
            await dispenserForm.Tip_Spit(100);

            dispenser_conf.Points.TryGetValue("Zero", out dispenser_pt);
            await dispenserForm.reverse_MovL(dispenser_pt);
            Response.Text += "Dispensing liquid done. ";
            
            await coaterForm.Spin_Coat(4000, 1000, 6);
            Response.Text += "Coating done. ";
        }
    }
}
