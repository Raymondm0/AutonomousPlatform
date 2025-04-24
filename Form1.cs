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


namespace WinFormsApp_Draft
{
    public partial class Form1 : Form
    {
        //general declarations
        private SerialPort motor_port = new SerialPort();
        private SerialPort servo_port = new SerialPort();
        private IModbusMaster master;

        //coater declarations
        private MotorAsync motorAsync = new MotorAsync();
        private static int spin_speed = 0;
        private System.Timers.Timer spin_timer = new System.Timers.Timer();
        private CancellationTokenSource cancellationTokenSource_pos;
        private CancellationTokenSource cancellationTokenSource_beat;
        public static bool coater_connect_state = false;
        //public static bool coater_running_state = false;

        //servo declarations
        private ServoAsync servoAsync = new ServoAsync();
        private const string move_away = "1";
        private const string move_back = "2";

        //arm declarations
        private System.Timers.Timer mTimerReader = new System.Timers.Timer(300);
        private static bool arm_connect_state = false;
        //public static bool arm_running_state = false;
        private DobotMove mDobotMove = new DobotMove();
        private Feedback mFeedback = new Feedback();
        private Dashboard mDashboard = new Dashboard();
        private bool mIsManualDisconnect = false;

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


        public Form1()
        {
            InitializeComponent();
            DisableCoater();
            DisableDashboard();
            DisableAuto();
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void init_Click(object sender, EventArgs e)
        {
            Controls.Clear();
            InitializeComponent();

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                MotorPorts.Items.Add(port);
                ServoPorts.Items.Add(port);
            }
            MotorPorts.SelectedIndex = 0;
            ServoPorts.SelectedIndex = 0;

            MotorBaudRate.Items.Add("9600");
            MotorBaudRate.Items.Add("19200");
            MotorBaudRate.Items.Add("38400");
            MotorBaudRate.Items.Add("115200");
            MotorBaudRate.SelectedIndex = 0;

            ServoBaudRate.Items.Add("9600");
            ServoBaudRate.Items.Add("19200");
            ServoBaudRate.Items.Add("38400");
            ServoBaudRate.Items.Add("115200");
            ServoBaudRate.SelectedIndex = 0;

            ArmIP.Text = "192.168.1.6";
            DashboardPort.Text = "29999";
            MovePort.Text = "30003";
            FeedbackPort.Text = "30004";

            mFeedback.NetworkErrorEvent += new DobotClient.OnNetworkError(this.OnNetworkErrorEvent_Feedback);
            mDobotMove.NetworkErrorEvent += new DobotClient.OnNetworkError(this.OnNetworkErrorEvent_DobotMove);
            mDashboard.NetworkErrorEvent += new DobotClient.OnNetworkError(this.OnNetworkErrorEvent_Dashboard);

            mTimerReader.Elapsed += new System.Timers.ElapsedEventHandler(TimeoutEvent);
            mTimerReader.AutoReset = true;

            string strPath = System.Windows.Forms.Application.StartupPath + "\\";
            ErrorInfoHelper.ParseControllerJsonFile(strPath + "alarm_controller.json");
            ErrorInfoHelper.ParseServoJsonFile(strPath + "alarm_servo.json");

            FilePath.Text = "C:\\Users\\DELL\\Desktop\\test.xlsx";
            SheetName.Text = "Sheet1";

            DisableCoater();
            DisableDashboard();
            //DisableAuto();
        }

        private void MotorPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedMotorPort.Text = MotorPorts.SelectedItem.ToString();
        }

        private void MotorBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedMotorBR.Text = MotorBaudRate.SelectedItem.ToString();
        }

        private void ServoPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedServoPort.Text = ServoPorts.SelectedItem.ToString();
        }

        private void ServoBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedServoBR.Text = ServoBaudRate.SelectedItem.ToString();
        }

        private void EnableCoater()
        {
            foreach (System.Windows.Forms.Control ctr in Controls)
            {
                if (ctr == SpinCoater)
                {
                    ctr.Enabled = true;
                }
            }
        }

        private void EnableDashboard()
        {
            foreach (System.Windows.Forms.Control ctr in Controls)
            {
                if (ctr == Dashboard)
                {
                    ctr.Enabled = true;
                }
            }
        }

        private void EnableAuto()
        {
            foreach (System.Windows.Forms.Control ctr in Controls)
            {
                if (ctr == AutoRun)
                {
                    ctr.Enabled = true;
                }
            }
        }

        private void DisableCoater()
        {
            foreach (System.Windows.Forms.Control ctr in Controls)
            {
                if (ctr == SpinCoater)
                {
                    ctr.Enabled = false;
                }
            }
        }

        private void DisableDashboard()
        {
            foreach (System.Windows.Forms.Control ctr in Controls)
            {
                if (ctr == Dashboard)
                {
                    ctr.Enabled = false;
                }
            }
        }

        private void DisableAuto()
        {
            foreach (System.Windows.Forms.Control ctr in Controls)
            {
                if (ctr == AutoRun)
                {
                    ctr.Enabled = false;
                }
            }
        }

        // Motor Functions
        private async void MotorSerialSwitch_Click(object sender, EventArgs e)
        {
            if (SelectedMotorBR.Text != "" && SelectedMotorPort.Text != "" && motor_port.IsOpen == false)
            {
                motor_port.PortName = SelectedMotorPort.Text;
                motor_port.BaudRate = Convert.ToInt32(SelectedMotorBR.Text);
                motor_port.DataBits = 8;
                motor_port.StopBits = StopBits.Two;
                motor_port.Parity = Parity.None;
                motor_port.WriteTimeout = 500;
                motor_port.ReadTimeout = 500;

                try
                {
                    Response.Clear();
                    motor_port.Open();
                    master = ModbusSerialMaster.CreateRtu(motor_port);
                    MotorConnection.Text = "connecting..";


                    cancellationTokenSource_beat = new CancellationTokenSource();
                    await Task.Run(() => motorAsync.StartBeatAsync(master, cancellationTokenSource_beat.Token));

                    if (motor_port.IsOpen)
                    {
                        EnableCoater();
                        MotorSerialSwitch.Text = "Disconnect Motor";
                        MotorConnection.Text = "opened";
                        coater_connect_state = true;
                        if (arm_connect_state)
                        {
                            EnableAuto();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Response.Text = ex.Message;
                    motor_port.Close();
                    MotorSerialSwitch.Text = "Open Serial";
                    MotorConnection.Text = "closed";
                    DisableAuto();
                }
            }
            else
            {
                try
                {
                    Response.Clear();
                    motor_port.Close();
                    MotorSerialSwitch.Text = "Open Serial";
                    MotorConnection.Text = "closed";
                    if (cancellationTokenSource_beat != null)
                    {
                        cancellationTokenSource_beat?.Cancel();
                    }
                    DisableCoater();
                    DisableAuto();
                }
                catch (Exception ex)
                {
                    Response.Text = ex.Message;
                }
            }
        }

        //如果AutoSpin的前3个方法能动的话，可写到MotorAsyc中
        private async Task SendRequestAsync()
        {
            if (motor_port.IsOpen == true)
            {
                try
                {
                    byte slaveID = 0x01;
                    ushort modeAddress = 0x1771;

                    if (SpinSpeed.Text != "")
                    {
                        ushort speedAddress = 0x1773;
                        int speed = Convert.ToInt32(SpinSpeed.Text);
                        spin_speed = speed * 7;
                        ushort high = (ushort)(spin_speed >> 16);
                        ushort low = (ushort)spin_speed;
                        ushort[] speed_erpm = { high, low };

                        master.WriteMultipleRegisters(slaveID, speedAddress, speed_erpm);
                    }

                    if (SpinDuration.Text != "")
                    {
                        spin_timer.Elapsed += new ElapsedEventHandler(FreeStop_timer);
                        spin_timer.Interval = Convert.ToInt32(SpinDuration.Text) * 1000;
                        spin_timer.AutoReset = false;
                        spin_timer.Start();
                    }

                    if (AccSpeed.Text != "")
                    {
                        ushort accAddress = 0x1780;
                        int acc = Convert.ToInt32(AccSpeed.Text);
                        acc = acc * 7;
                        ushort high = (ushort)(acc >> 16);
                        ushort low = (ushort)acc;
                        ushort[] acc_erpm = { high, low };

                        master.WriteMultipleRegisters(slaveID, accAddress, acc_erpm);
                    }

                    master.WriteSingleRegister(slaveID, modeAddress, 0x0001);
                }

                catch (Exception ex)
                {
                    Response.Text = ex.Message;
                }
            }
        }

        private async void SendRequest_Click(object sender, EventArgs e)
        {
            await Task.Run(() => SendRequestAsync());
        }

        private void FreeStop_timer(object sender, ElapsedEventArgs e)
        {
            byte slaveID = 0x01;
            ushort stopAddress = 0x177E;
            ushort modeAddress = 0x1771;
            master.WriteSingleRegister(slaveID, stopAddress, 0x0000);
            master.WriteSingleRegister(slaveID, modeAddress, 0x0006);
            spin_timer.Stop();

            spin_speed = 0;
        }

        private async void FreeStop_Click(object sender, EventArgs e)
        {
            await Task.Run(() => motorAsync.FreeStopAsync(FreeStop, master));
            spin_speed = 0;
        }

        private async void ForceStop_Click(object sender, EventArgs e)
        {
            await Task.Run(() => motorAsync.ForceStopAsync(ForceStop, master));
            spin_speed = 0;
        }

        private async void UpdatePosition_CheckedChanged(object sender, EventArgs e)
        {
            if (Update.Checked)
            {
                cancellationTokenSource_pos = new CancellationTokenSource();
                await Task.Run(() => motorAsync.UpdatePositionAsync(Update, Position, master, cancellationTokenSource_pos.Token));
            }
            else
            {
                cancellationTokenSource_pos?.Cancel();
                Position.Clear();
            }
        }

        private async void ClearPos_Click(object sender, EventArgs e)
        {
            Position.Clear();
            await Task.Run(() => motorAsync.ClearPosAsync(ClearPos, master));
        }

        private async void ResetMotor_Click(object sender, EventArgs e)
        {
            await Task.Run(() => motorAsync.ResetMotorAsync(ResetMotor, master, spin_speed));
            spin_speed = 0;
        }

        // Servo Functions
        private void ServoSerialSwitch_Click(object sender, EventArgs e)
        {
            if (SelectedServoBR.Text != "" && SelectedServoPort.Text != "" && servo_port.IsOpen == false)
            {
                servo_port.PortName = SelectedServoPort.Text;
                servo_port.BaudRate = Convert.ToInt32(ServoBaudRate.Text);
                servo_port.StopBits = StopBits.Two;
                servo_port.Parity = Parity.None;

                try
                {
                    servo_port.Open();

                    servo_port.Write(move_back);
                    ServoConnection.Text = "opened";
                    ServoSerialSwitch.Text = "Disconnect Servo";
                }
                catch (Exception ex)
                {
                    Response.Text = ex.Message;
                    servo_port.Close();
                    ServoSerialSwitch.Text = "Open Serial";
                    ServoConnection.Text = "closed";
                }
            }
            else
            {
                try
                {
                    servo_port.Close();
                    ServoSerialSwitch.Text = "Open Serial";
                    ServoConnection.Text = "closed";
                }
                catch (Exception ex)
                {
                    Response.Text = ex.Message;
                }
            }
        }

        private async void MoveAway_CheckedChanged(object sender, EventArgs e)
        {
            await Task.Run(() => servoAsync.MoveAwayAsync(MoveAway, servo_port));
        }


        // Robot Arm Functions
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            mTimerReader.Close();
            if (mFeedback != null)
            {
                mFeedback.Disconnect();
            }
            if (mDobotMove != null)
            {
                mDobotMove.Disconnect();
            }
            if (mDashboard != null)
            {
                mDashboard.Disconnect();
            }
        }

        private void TimeoutEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!mFeedback.DataHasRead)
            {
                return;
            }
            mFeedback.DataHasRead = false;
            this.textBoxX.Invoke(new Action(() =>
            {
                ShowDataResult();
            }));
        }

        private void DoNetworkErrorEvent(DobotClient sender, string strIp, int iPort)
        {
            DisableDashboard();
            Thread thd = new Thread(() =>
            {
                sender.Disconnect();

                mTimerReader.Stop();

                if (!sender.Connect(strIp, iPort))
                {
                    Thread.Sleep(500);
                    DoNetworkErrorEvent(sender, strIp, iPort);
                    return;
                }

                mTimerReader.Start();

                this.Invoke(new Action(() =>
                {
                    EnableDashboard();
                    if (coater_connect_state)
                    {
                        EnableAuto();
                    }
                }));
            });
            thd.Start();
            arm_connect_state = true;
        }

        private void OnNetworkErrorEvent_Feedback(DobotClient sender, SocketError iErrCode)
        {
            if (mIsManualDisconnect) return;
            this.BeginInvoke(new Action(() =>
            {
                string strIp = ArmIP.Text;
                int iPort = Parse2Int(this.FeedbackPort.Text);
                DoNetworkErrorEvent(mFeedback, strIp, iPort);
            }));
        }
        private void OnNetworkErrorEvent_DobotMove(DobotClient sender, SocketError iErrCode)
        {
            if (mIsManualDisconnect) return;
            this.BeginInvoke(new Action(() =>
            {
                string strIp = ArmIP.Text;
                int iPort = Parse2Int(this.MovePort.Text);
                DoNetworkErrorEvent(mDobotMove, strIp, iPort);
            }));
        }
        private void OnNetworkErrorEvent_Dashboard(DobotClient sender, SocketError iErrCode)
        {
            if (mIsManualDisconnect) return;
            this.BeginInvoke(new Action(() =>
            {
                string strIp = ArmIP.Text;
                int iPort = Parse2Int(this.DashboardPort.Text);
                DoNetworkErrorEvent(mDashboard, strIp, iPort);
            }));
        }

        private double Parse2Double(string str)
        {
            double value = 0.0;
            try
            {
                value = Double.Parse(str);
            }
            catch { }
            return value;
        }
        private int Parse2Int(string str)
        {
            int iValue = 0;
            try
            {
                iValue = int.Parse(str);
            }
            catch
            {
            }
            return iValue;
        }

        private bool IsValidIP(string strIp)
        {
            try
            {
                System.Net.IPAddress.Parse(strIp);
            }
            catch
            {
                return false;
            }
            return true;
        }
        private void Connect()
        {
            string strIp = ArmIP.Text;
            if (!IsValidIP(strIp))
            {
                MessageBox.Show("IP Address Invalid");
                return;
            }
            int iPortFeedback = Parse2Int(FeedbackPort.Text);
            int iPortMove = Parse2Int(MovePort.Text);
            int iPortDashboard = Parse2Int(DashboardPort.Text);

            Thread thd = new Thread(() =>
            {
                if (!mDashboard.Connect(strIp, iPortDashboard))
                {
                    Response.Text = string.Format("Connect {0}:{1} Fail!!", strIp, iPortDashboard);
                    return;
                }
                if (!mDobotMove.Connect(strIp, iPortMove))
                {
                    Response.Text = string.Format("Connect {0}:{1} Fail!!", strIp, iPortMove);
                    return;
                }
                if (!mFeedback.Connect(strIp, iPortFeedback))
                {
                    Response.Text = string.Format("Connect {0}:{1} Fail!!", strIp, iPortFeedback);
                    return;
                }

                mIsManualDisconnect = false;
                mTimerReader.Start();

                Invoke(new Action(() =>
                {
                    EnableDashboard();
                    btnConnect.Text = "Disconnect";
                }));
            });
            thd.Start();
        }
        private void Disconnect()
        {
            Thread thd = new Thread(() =>
            {
                mFeedback.Disconnect();
                mDobotMove.Disconnect();
                mDashboard.Disconnect();
                arm_connect_state = false;
                DisableAuto();

                mTimerReader.Stop();

                Invoke(new Action(() =>
                {
                    btnConnect.Text = "Connect";
                }));
            });
            thd.Start();
        }

        private void ShowDataResult()
        {
            if (null != mFeedback.feedbackData.ToolVectorActual && mFeedback.feedbackData.ToolVectorActual.Length >= 4)
            {
                curX.Text = string.Format("X:{0:F3}", mFeedback.feedbackData.ToolVectorActual[0]);
                curY.Text = string.Format("Y:{0:F3}", mFeedback.feedbackData.ToolVectorActual[1]);
                curZ.Text = string.Format("Z:{0:F3}", mFeedback.feedbackData.ToolVectorActual[2]);
                if (textBoxX.Text.Length == 0)
                {
                    textBoxX.Text = string.Format("{0:F3}", mFeedback.feedbackData.ToolVectorActual[0]);
                    textBoxY.Text = string.Format("{0:F3}", mFeedback.feedbackData.ToolVectorActual[1]);
                    textBoxZ.Text = string.Format("{0:F3}", mFeedback.feedbackData.ToolVectorActual[2]);
                }
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "Disconnect")
            {
                mIsManualDisconnect = true;
                Disconnect();
            }
            else
            {
                Connect();
            }
        }

        private void btnEnable_Click(object sender, EventArgs e)
        {
            bool bEnable = btnEnable.Text.Equals("Enable");

            Thread thd = new Thread(() =>
            {
                string ret = bEnable ? mDashboard.EnableRobot() : mDashboard.DisableRobot();
                bool bOk = ret.StartsWith("0");

                this.btnEnable.Invoke(new Action(() =>
                {
                    if (bOk)
                    {
                        btnEnable.Text = bEnable ? "Disable" : "Enable";
                    }
                }));
            });
            thd.Start();
        }

        private void btnEnableAgain_Click(object sender, EventArgs e)
        {
            Thread thd = new Thread(() =>
            {
                string ret = mDashboard.EnableRobot();
                bool bOk = ret.StartsWith("0");
            });
            thd.Start();
        }

        private void ResetArm_Click(object sender, EventArgs e)
        {
            Thread thd = new Thread(() =>
            {
                string ret = mDashboard.ResetRobot();
            });
            thd.Start();
        }

        private void Move_Click(object sender, EventArgs e)
        {
            DescartesPoint pt = new DescartesPoint();
            pt.x = Parse2Double(textBoxX.Text);
            pt.y = Parse2Double(textBoxY.Text);
            pt.z = Parse2Double(textBoxZ.Text);

            Thread thd = new Thread(() =>
            {
                string ret = mDobotMove.MovJ(pt);
            });
            thd.Start();
        }

        // auto read functions, complete automatic
        private async void AutoRead_CheckedChanged(object sender, EventArgs e)
        {
            if (AutoRead.Checked)
            {
                DisableCoater();
                DisableDashboard();

                cancellationTokenSource_pos = new CancellationTokenSource();
                Task.Run(() => motorAsync.UpdatePositionAsync(Update, Position, master, cancellationTokenSource_pos.Token));

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
                if (arm_connect_state)
                {
                    EnableDashboard();
                }
                if (coater_connect_state)
                {
                    EnableCoater();
                } 
                Response.Clear();
                ShowData.Clear();
            }
        }
    }
}
