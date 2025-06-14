using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Timers;
using Modbus.Device;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using CSharpTcpDemo.com.dobot.api;
using CSharthiscpDemo.com.dobot.api;
using WinFormsApp_Draft.Async;
using WinFormsApp_Draft.Auto;
using CSharpTcpDemo;


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
            coaterForm.DisableCoater();
            DisableAuto();

            string strPath = System.Windows.Forms.Application.StartupPath + "\\";
            ErrorInfoHelper.ParseControllerJsonFile(strPath + "alarm_controller.json");
            ErrorInfoHelper.ParseServoJsonFile(strPath + "alarm_servo.json");

            FilePath.Text = "C:\\Users\\DELL\\Desktop\\test.xlsx";
            SheetName.Text = "Sheet1";

            armForm.TopLevel = false;
            coaterForm.TopLevel = false;
            dispenserForm.TopLevel = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        public void EnableAuto()
        {
            foreach (System.Windows.Forms.Control ctr in Controls)
            {
                if (ctr == AutoRun)
                {
                    ctr.Enabled = true;
                }
            }
        }

        public void DisableAuto()
        {
            foreach (System.Windows.Forms.Control ctr in Controls)
            {
                if (ctr == AutoRun)
                {
                    ctr.Enabled = false;
                }
            }
        }

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
        private class PointsConfig
        {
            public Dictionary<string, DescartesPoint> Points { get; set; } = new Dictionary<string, DescartesPoint>();
        }

        private async void MoveTest_Click(object sender, EventArgs e)
        {
            string filePath = "ArmPoints.json";
            string json_data = File.ReadAllText(filePath);
            PointsConfig? pt_conf = JsonConvert.DeserializeObject<PointsConfig>(json_data);

            DescartesPoint pt = new DescartesPoint();
            pt_conf.Points.TryGetValue("Zero", out pt);
            armForm.mDobotMove.MovL(pt);
            pt_conf.Points.TryGetValue("P1", out pt);
            armForm.mDobotMove.MovL(pt);
            pt_conf.Points.TryGetValue("P2", out pt);
            armForm.mDobotMove.MovL(pt);

        }
    }
}
