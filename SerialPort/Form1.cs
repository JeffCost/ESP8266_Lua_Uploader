using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;


namespace LuaUploader
{
    /// <summary>
    /// Lua Uploader by Hari Wiguna, 2014
    /// 
    /// You can find me at:
    ///     YouTube: https://www.youtube.com/user/hwiguna/videos?view_as=public
    ///     Hackaday.io: http://hackaday.io/projects/hacker/9819
    ///     My blog: http://g33k.blogspot.com/
    ///     Google+: https://plus.google.com/u/0/+HariWiguna/posts
    ///     
    /// "Refresh" icon courtesy of "PC" at iconFinder: https://www.iconfinder.com/icons/59198/refresh_reload_repeat_reset_icon#size=32
    /// </summary>
    public partial class Form1 : Form
    {
        

        EnhancedSerialPort serialPort;
        
        DateTime lastSerialActivity = DateTime.MinValue;

        public Form1()
        {
            InitializeComponent();

            //ComboboxItem item = new ComboboxItem();
            //item.Text = "[0] GPIO16";
            //item.Value = 0;
            //gpioSetModePin.Items.Add(item);
            //gpioModeSetValuePin.Items.Add(item);
            
            //gpioSetModePin.SelectedIndex = 0;
            //gpioModeSetValuePin.SelectedIndex = 0;

            //ComboboxItem item1 = new ComboboxItem();
            //item1.Text = "[1] GPIO5";
            //item1.Value = 1;
            //gpioSetModePin.Items.Add(item1);
            //gpioModeSetValuePin.Items.Add(item1);

            //ComboboxItem item2 = new ComboboxItem();
            //item2.Text = "[2] GPIO4";
            //item2.Value = 2;
            //gpioSetModePin.Items.Add(item2);
            //gpioModeSetValuePin.Items.Add(item2);

            //ComboboxItem item3 = new ComboboxItem();
            //item3.Text = "[3] GPIO0";
            //item3.Value = 3;
            //gpioSetModePin.Items.Add(item3);
            //gpioModeSetValuePin.Items.Add(item3);

            //ComboboxItem item4 = new ComboboxItem();
            //item4.Text = "[4] GPIO2";
            //item4.Value = 4;
            //gpioSetModePin.Items.Add(item4);
            //gpioModeSetValuePin.Items.Add(item4);

            //ComboboxItem item5 = new ComboboxItem();
            //item5.Text = "[5] GPIO14";
            //item5.Value = 5;
            //gpioSetModePin.Items.Add(item5);
            //gpioModeSetValuePin.Items.Add(item5);

            //ComboboxItem item6 = new ComboboxItem();
            //item6.Text = "[6] GPIO12";
            //item6.Value = 6;
            //gpioSetModePin.Items.Add(item6);
            //gpioModeSetValuePin.Items.Add(item6);

            //ComboboxItem item7 = new ComboboxItem();
            //item7.Text = "[7] GPIO13";
            //item7.Value = 7;
            //gpioSetModePin.Items.Add(item7);
            //gpioModeSetValuePin.Items.Add(item7);

            //ComboboxItem item8 = new ComboboxItem();
            //item8.Text = "[8] GPIO15";
            //item8.Value = 8;
            //gpioSetModePin.Items.Add(item8);
            //gpioModeSetValuePin.Items.Add(item8);

            //ComboboxItem item9 = new ComboboxItem();
            //item9.Text = "[9] GPIO3";
            //item9.Value = 9;
            //gpioSetModePin.Items.Add(item9);
            //gpioModeSetValuePin.Items.Add(item9);

            //ComboboxItem item10 = new ComboboxItem();
            //item10.Text = "[10] GPIO1";
            //item10.Value = 10;
            //gpioSetModePin.Items.Add(item10);
            //gpioModeSetValuePin.Items.Add(item10);

            //ComboboxItem item11 = new ComboboxItem();
            //item11.Text = "[11] GPIO9";
            //item11.Value = 11;
            //gpioSetModePin.Items.Add(item11);
            //gpioModeSetValuePin.Items.Add(item11);

            //ComboboxItem item12 = new ComboboxItem();
            //item12.Text = "[12] GPIO10";
            //item12.Value = 12;
            //gpioSetModePin.Items.Add(item12);
            //gpioModeSetValuePin.Items.Add(item12);

            //gpioSetModePin.DropDownWidth = DropDownWidth(gpioSetModePin);

            snippetFileName.DropDownStyle = ComboBoxStyle.DropDownList;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshPortList();
            RestoreUserSettings();

            timer1.Stop();
            timer1.Interval = 200;

            serialPort = new EnhancedSerialPort(PortComboBox.Text, int.Parse(BaudRateBox.Text));
            serialPort.NewLine = "\r\n"; // CR followed by LF
            serialPort.DataReceived += (s,ea) => output.Invoke(new System.Action(() => serialPort_DataReceived(s,ea)));
            espUploadProgressBar.Visible = false;

            serialPort.ReadTimeout = 1000;
            serialPort.WriteTimeout = 1000;

            string path = getSnippetsDirectoryPath();
            bool exists = System.IO.Directory.Exists(path);

            if (!exists)
            {
                System.IO.Directory.CreateDirectory(path);
            }

            string[] fileEntries = GetFileNames(path, "*.lua");
            snippetFileName.DataSource = fileEntries;

        }

        private static string[] GetFileNames(string path, string filter)
        {
            //string[] luaFiles = Directory.GetFiles(path, "*.lua")
            //                             .Select(path => Path.GetFileName(path))
            //                             .ToArray();
            // solution above uses LINQ, so it requires .NET 3.5 at least. Here's a solution that works on earlier versions:

            string[] files = Directory.GetFiles(path, filter);
            for (int i = 0; i < files.Length; i++)
                files[i] = Path.GetFileName(files[i]);
            return files;
        }

        private void RestoreUserSettings()
        {
            //-- Restore User Settings --
            PortComboBox.Text = Properties.Settings.Default.comPort;
            FilePathTextbox.Text = Properties.Settings.Default.filePath;
            BaudRateBox.Text = Properties.Settings.Default.baudRate;
            LineDelayTextbox.Text = Properties.Settings.Default.lineDelay;
            LuaFilenameTextbox.Text = Properties.Settings.Default.LuaFilename;
            CommandTextbox.Text = Properties.Settings.Default.Command;
            RunAfterSaving.Checked = Properties.Settings.Default.RunAfterSaving;
        }


        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();// sp.ReadExisting();
            //output.Text += "Data Received:\r\n";
            output.Text += indata; // +"\r\n";
            output.SelectionStart = output.Text.Length;
            output.SelectionLength = 0;
            output.ScrollToCaret();

            lastSerialActivity = DateTime.Now;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort!=null)
                serialPort.Dispose();
            serialPort = null;

            SaveUserSettings();
        }

        private void SaveUserSettings()
        {
            //-- Save User Settings --
            Properties.Settings.Default.filePath = FilePathTextbox.Text;
            Properties.Settings.Default.comPort = PortComboBox.Text;
            Properties.Settings.Default.baudRate = BaudRateBox.Text;
            Properties.Settings.Default.lineDelay = LineDelayTextbox.Text;
            Properties.Settings.Default.LuaFilename = LuaFilenameTextbox.Text;
            Properties.Settings.Default.Command = CommandTextbox.Text;
            Properties.Settings.Default.RunAfterSaving = RunAfterSaving.Checked;
            Properties.Settings.Default.Save();
        }

        private void Send(string str)
        {
            if(serialPort.IsOpen)
            {
                output.Text += string.Format("SENT: {0}\r\n", str);
                output.SelectionStart = output.Text.Length;
                output.ScrollToCaret();
                serialPort.WriteLine(str);
            }
            else
            {
                //MessageBox.Show("Serial port is not open.");
            }
        }

        private void SendLines(string[] lines)
        {
            Cursor.Current = Cursors.WaitCursor;
            OpenComPort();

            SetProgressBar(lines.Count());
            
            int count = 0;
            // Send it line by line with small delay to COM port so LUA interpreter on ESP8266 can keep up
            
            foreach (var line in lines)
            {
                //Send("tmr.wdclr()\r\n");
                Send(line);
                UpdateProgressBar(++count);
                
                System.Threading.Thread.Sleep(int.Parse(LineDelayTextbox.Text));
            }

            Cursor.Current = Cursors.Arrow;
        }

        private void SendLines(string multipleLineString)
        {
            string[] lines = multipleLineString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            SendLines(lines);
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            //Open Text file
            string origFile = System.IO.File.ReadAllText(FilePathTextbox.Text);
            string targetFileName = Path.GetFileName(FilePathTextbox.Text);
            Send("uart.setup(0,9600,8,0,1,0)\r\n");
            string luaCode = string.Format(
                "file.remove(\"{0}\")\r\n" +
                "file.open(\"{0}\",\"w\")\r\n" +
                "{1}" +
                "file.close()\r\n", targetFileName, WrapInWriteLine(origFile));
            
            SendLines(luaCode);
        }

        private void ExecuteButton_Click(object sender, EventArgs e)
        {
            //string[] lines = CommandTextbox.Text.Split( new char[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
            SendLines( CommandTextbox.Text );
        }

        private void OpenComPort()
        {
            RefreshButton.BackColor = Color.Green;
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                RefreshButton.BackColor = Color.Red;
                PortComboBox.Enabled = true;
                BaudRateBox.Enabled = true;
            }
            //-- Keep COM port up-to-date --
            serialPort.PortName = PortComboBox.Text;
            PortComboBox.Enabled = false;
            serialPort.BaudRate = int.Parse(BaudRateBox.Text);
            BaudRateBox.Enabled = false;
            try
            {
                serialPort.Open();
            }
            catch(IOException e)
            {
                MessageBox.Show(e.Message);
                return;
            }
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                FilePathTextbox.Text = openFileDialog1.FileName;
            }
        }

        private void SetProgressBar(int numOfLines)
        {
            espUploadProgressBar.Value = 0;
            espUploadProgressBar.Minimum = 0;
            espUploadProgressBar.Maximum = numOfLines;
            espUploadProgressBar.Visible = true;
        }

        private void UpdateProgressBar(int val)
        {
            espUploadProgressBar.Value = val;
            if(val >= espUploadProgressBar.Maximum)
            {
                espUploadProgressBar.Visible = false;
                espUploadProgressBar.Value = 0;
            }
        }

        private void SaveOnESPButton_Click(object sender, EventArgs e)
        {
            Send("uart.setup(0,9600,8,0,1,0)\r\n");
            string luaCode = string.Format(
                "file.remove(\"{0}\")\r\n" +
                "file.open(\"{0}\",\"w\")\r\n" +
                "{1}" +
                "file.close()\r\n", LuaFilenameTextbox.Text, WrapInWriteLine(LuaCodeTextbox.Text));

            if (RunAfterSaving.Checked)
            {
                string doFile = string.Format("dofile(\"{0}\")\r\n", LuaFilenameTextbox.Text);
                luaCode += doFile;
            }

            SendLines(luaCode);
        }

        private string WrapInWriteLine(string luaCode)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var line in luaCode.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                //LuaCodeTextbox.Text += "The line is: [" + line + "]\r\n";
                stringBuilder.AppendLine("file.writeline([[" + line.Trim() + "]])");
            }

            return stringBuilder.ToString();
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            SendLines("node.restart()");
        }

        private void LuaCodeTextbox_keyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.F5)
            //{
            //    SendLines(LuaCodeTextbox.SelectedText);
            //    e.Handled = true;
            //}

            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        LuaCodeTextbox.SelectAll();
                        e.Handled = true;
                        break;
                    case Keys.S:
                        SaveToDisk();
                        e.Handled = true;
                        break;
                }
            }
        }

        private void CommandTextbox_keyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.F5)
            //{
            //    SendLines(LuaCodeTextbox.SelectedText);
            //    e.Handled = true;
            //}

            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        CommandTextbox.SelectAll();
                        e.Handled = true;
                        break;
                    case Keys.S:
                        SaveToDisk();
                        e.Handled = true;
                        break;
                }
            }
        }

        private void output_keyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                int totalLines = output.Lines.Length;
                string command = output.Lines[totalLines - 1];
                if(!String.IsNullOrEmpty(command))
                {
                    string strCmd = ReplaceFirst(command, ">", "", true);
                    //Regex regex = new Regex(@"\s*?-\s*", RegexOptions.Compiled);
                    //regex.Replace(command, " ");
                    //command.ReplaceAll()
                    
                    Send(strCmd);
                }
                e.Handled = true;
            }

            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.L:
                        
                        int totalLines = output.Lines.Length;
                        string command = output.Lines[totalLines - 1];
                        output.Clear();
                        output.Text = command;
                        output.Select(output.Text.Length, 0);
                        e.Handled = true;
                        break;
                }
            }
        }

        string ReplaceFirst(string text, string search, string replace, bool removeAllInstances = false)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            if(removeAllInstances)
            {
                string res = text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
                ReplaceFirst(res, search, replace, removeAllInstances);
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
       

        private void LoadFromESPButton_Click(object sender, EventArgs e)
        {
            string printFileContent =
                "file.open(\""+LuaFilenameTextbox.Text+"\",'r')\r\n" +
                "txt = ''\r\n" +
                "repeat\r\n" +
                "line = file.readline()\r\n" +
                "if (line~=nil) then txt = txt .. line end\r\n" +
                "until line == nil\r\n" +
                "file.close()\r\n" +
                "print(txt)\r\n";

            SendLines(printFileContent);

            timer1.Start();
        }

        private void ClearOutputButton_Click(object sender, EventArgs e)
        {
            output.Clear();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            SendLines("file.remove('" + LuaFilenameTextbox.Text + "')");
        }

        private void ListFilesButton_Click(object sender, EventArgs e)
        {
            string listFiles = "for k,v in pairs(file.list()) do print(\"name:\"..k..\", size:\"..v) end\r\n";
            SendLines(listFiles);
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            string doFile = string.Format("dofile(\"{0}\")\r\n", LuaFilenameTextbox.Text);
            SendLines(doFile);
        }

        private void LoadFromDiskButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                LuaCodeTextbox.Text = System.IO.File.ReadAllText(openFileDialog1.FileName);
                LuaFilenameTextbox.Text = Path.GetFileName(openFileDialog1.FileName);
            }
        }

        private void SaveToDiskButton_Click(object sender, EventArgs e)
        {
            SaveToDisk();
        }

        private void SaveToDisk()
        {
            saveFileDialog1.Filter = "LUA|*.lua|Text File|*.txt";
            DialogResult result = saveFileDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string fileContent = LuaCodeTextbox.Text;
                System.IO.File.WriteAllText(saveFileDialog1.FileName, fileContent);
            }
        }

        private void ExecuteSelectionButton_Click(object sender, EventArgs e)
        {
            espUploadProgressBar.Value = 5;
            SendLines(LuaCodeTextbox.SelectedText);
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshPortList();
        }

        private void RefreshPortList()
        {
            string[] portNames = SerialPort.GetPortNames();
            PortComboBox.DataSource = portNames;
            PortComboBox.SelectedIndex = portNames.Count() - 1;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine("TICK");
            if (DateTime.Now.Subtract( lastSerialActivity ).TotalMilliseconds > 500)
            {
                //Console.WriteLine("Haven't received any chars from ESP for a while.  He must be done sending the lua listing...");
                timer1.Stop();
                CopyOutputToCode();
            }
        }

        private void CopyOutputToCode()
        {
            // Thanks to Eskici for the code to find the lua code sent back by the ESP!
            string splitter = "> print(txt)\r\n";

            // making sure that the output contains the splitter
            if (output.Text.Contains(splitter))
            {
                // split the output string and get the latest loaded code
                string spl_code = output.Text.Split(new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList<string>().Last<string>().Trim();

                // make sure the code ends with ">" and remove it, other wise something gone wrong, just return empty.
                if (spl_code.EndsWith(">"))
                    LuaCodeTextbox.Text = spl_code.Substring(0, spl_code.Length - 1).Trim();
            }
        }

        private void btnGetApIP_Click(object sender, EventArgs e)
        {
            SendLines("print(wifi.ap.getip())");
        }

        private void btnShowHeap_Click(object sender, EventArgs e)
        {
            SendLines("print(node.heap())");
        }

        private int get_io_pin(string pin)
        {
            // 0 [*] GPIO16    [*] D0(GPIO16) can only be used as gpio read/write. no interrupt supported. no pwm/i2c/ow supported.
            // 1 	 GPIO5 	
            // 2 	 GPIO4 	
            // 3 	 GPIO0 	
            // 4 	 GPIO2 	
            // 5 	 GPIO14 		
            // 6 	 GPIO12 		
            // 7 	 GPIO13
            // 8 	 GPIO15
            // 9 	 GPIO3
            // 10 	 GPIO1
            // 11 	 GPIO9
            // 12 	 GPIO10

            int pinIntValue = 3;
            switch (pin)
            {
                case "GPIO16":
                case "[0] GPIO16":
                    pinIntValue = 0;
                    break;
                case "GPIO5":
                case "[1] GPIO5":
                    pinIntValue = 1;
                    break;
                case "GPIO4":
                case "[2] GPIO4":
                    pinIntValue = 2;
                    break;
                case "GPIO0":
                case "[3] GPIO0":
                    pinIntValue = 3;
                    break;
                case "GPIO2":
                case "[4] GPIO2":
                    pinIntValue = 4;
                    break;
                case "GPIO14":
                case "[5] GPIO14":
                    pinIntValue = 5;
                    break;
                case "GPIO12":
                case "[6] GPIO12":
                    pinIntValue = 6;
                    break;
                case "GPIO13":
                case "[7] GPIO13":
                    pinIntValue = 7;
                    break;
                case "GPIO15":
                case "[8] GPIO15":
                    pinIntValue = 8;
                    break;
                case "GPIO3":
                case "[9] GPIO3":
                    pinIntValue = 9;
                    break;
                case "GPIO1":
                case "[10] GPIO1":
                    pinIntValue = 10;
                    break;
                case "GPIO9":
                case "[11] GPIO9":
                    pinIntValue = 11;
                    break;
                case "GPIO10":
                case "[12] GPIO10":
                    pinIntValue = 12;
                    break;
                default:
                    pinIntValue = 3;
                    break;
            }
            return pinIntValue;
        }

        private int get_io_value(string value)
        {
            int pinIntValue = 0;
            switch (value.ToLower())
            {
                case "on":
                case "gpio.input":
                case "gpio.high":
                case "input":
                    pinIntValue = 1;
                    break;
                case "OFF":
                case "gpio.output":
                case "gpio.low":
                case "output":
                    break;
                default:
                    pinIntValue = 0;
                    break;
            }
            return pinIntValue;
        }

        private void btnStaGetIp_Click(object sender, EventArgs e)
        {
            SendLines("print(wifi.sta.getip())");
        }

        private void btnGpioMode_Click(object sender, EventArgs e)
        {
            //MessageBox.Show((gpioSetModePin.SelectedItem as ComboboxItem).Value.ToString());
            int gpioModePin = get_io_pin(gpioSetModePin.Text);
            int gpioModeValue = get_io_value(gpioSetModeValue.Text);
            SendLines("gpio.mode(" + gpioModePin + ", " + gpioModeValue + ")");
        }

        private void btnGetChipId_Click(object sender, EventArgs e)
        {
            SendLines("print(node.chipid())");
        }

        private void btnGetFlashId_Click(object sender, EventArgs e)
        {
            SendLines("print(node.flashid())");
        }

        private void btnGetApMac_Click(object sender, EventArgs e)
        {
            SendLines("print(wifi.ap.getmac())");
        }

        private void btnStaMac_Click(object sender, EventArgs e)
        {
            SendLines("print(wifi.sta.getmac())");
        }

        private void btnNodeInfo_Click(object sender, EventArgs e)
        {
            SendLines("print(node.info())");
        }

        private void btnReadVdd_Click(object sender, EventArgs e)
        {
            //v = node.readvdd33() / 1000
            SendLines("v = node.readvdd33()/100 print(v .. \" volts\")");
        }

        private void btnByteCodeCompile_Click(object sender, EventArgs e)
        {
            string fileName = Path.GetFileNameWithoutExtension(byteCodeCompileFileName.Text);
      
            if(String.IsNullOrEmpty(fileName))
            {
                MessageBox.Show("Please provide file name to compile.");
                return;
            }

            SendLines("node.compile(\""+fileName+".lua\")");

            if (byteCodeCompileRemoveLua.Checked)
            {
                SendLines("file.remove(\"" + fileName + ".lua\")");
            }

            if (byteCodeCompileLoad.Checked)
            {
                SendLines("dofile(\"" + fileName + ".lc\")");
            }
        }

        private void btnRenameFile_Click(object sender, EventArgs e)
        {
            string from = txtRenameFrom.Text;
            string to   = txtRenameTo.Text;
            if (String.IsNullOrEmpty(from))
            {
                MessageBox.Show("Please provide current file name.");
                return;
            }

            if (String.IsNullOrEmpty(to))
            {
                MessageBox.Show("Please provide new file name.");
                return;
            }
            SendLines("file.rename(\"" + from + "\",\"" + to + "\")");
        }

        private void btnWifiGetMode_Click(object sender, EventArgs e)
        {
            SendLines("print(wifi.getmode())");
        }

        private void gpioModeSetValueBtn_Click(object sender, EventArgs e)
        {
            int gpioValuePin = get_io_pin(gpioModeSetValuePin.Text);
            int gpioValueValue = get_io_value(gpioModeSetValueValue.Text);
            SendLines("gpio.write(" + gpioValuePin + "," + gpioValueValue + ")");
        }

        private void getStaStatusBtn_Click(object sender, EventArgs e)
        {
            SendLines("print(wifi.sta.status())");
        }

        private void wifiStaConnectBtn_Click(object sender, EventArgs e)
        {
            string ssid = wifiStaSsid.Text;
            string password = wifiStaPassword.Text;
            if (String.IsNullOrEmpty(ssid) || String.IsNullOrEmpty(password))
            {
                MessageBox.Show("SSID/PWD cannot be empty.");
                return;
            }

            string showConnected =
                "wifi.setmode(wifi.STATION)\r\n" +
                "wifi.sta.config(\"" + ssid + "\", \"" + password + "\") \r\n" +
                "wifi.sta.connect() \r\n"+
                "for i=1,5 do \r\n" +
                    "tmr.wdclr() \r\n" +
                    "status = wifi.sta.status() \r\n" +
                    "tmr.delay(10000000) \r\n" +
                    "if status ~= 5 then \r\n" +
                        "print(\"Connected  to [ " + ssid + " ]\") \r\n" +
                        "do return end \r\n" +
                    "else " +
                        "print(\"Trying to connect... [\" .. i .. \"/5]\") \r\n" +
                    "end \r\n" +
                "end \r\n";
            SendLines(showConnected);
        }

        private void wifiStaDisconnectBtn_Click(object sender, EventArgs e)
        {
            SendLines("wifi.sta.disconnect()");
        }

        private void wifiSetModeBtn_Click(object sender, EventArgs e)
        {
            string wifiMode = wifiSetModeValue.Text;
            SendLines("wifi.setmode(" + wifiMode + ")");
        }
        private void updateSnippetsComboBox()
        {
            string appDir = Environment.CurrentDirectory;
            string driveLetter = Path.GetPathRoot(appDir);
            string path = System.IO.Path.Combine(appDir, "snippets");
            string[] luaFiles = GetFileNames(path, "*.lua");
            snippetFileName.DataSource = luaFiles;
        }

        private void snippetSaveBtn_Click(object sender, EventArgs e)
        {
            string snippetName = snippetFileName.Text;
            string appDir = Environment.CurrentDirectory;
            string driveLetter = Path.GetPathRoot(appDir);

            string path = System.IO.Path.Combine(appDir , "snippets");

            bool exists = System.IO.Directory.Exists(path);

            if (!exists)
            {
                System.IO.Directory.CreateDirectory(path);
            }

            System.IO.File.WriteAllText(System.IO.Path.Combine(path, snippetName), snippetsText.Text);
            updateSnippetsComboBox();
            snippetFileName.Text = snippetName;
            //MessageBox.Show(String.Format("Snippet [{0}] successfully saved.", snippetName));
        }

        private void snippetFileName_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            string snippetName = cmb.Text;

            string appDir = Environment.CurrentDirectory;
            string driveLetter = Path.GetPathRoot(appDir);
            string path = System.IO.Path.Combine(appDir, "snippets", snippetName);

            string snippetContent = System.IO.File.ReadAllText(path);

            snippetsText.Text = snippetContent;
        }

        private void snippetDeleteBtn_Click(object sender, EventArgs e)
        {
            string filename = snippetFileName.Text;
            string path = getSnippetsDirectoryPath();
            File.Delete(System.IO.Path.Combine(path, filename));
            snippetFileName.Text = "";
            updateSnippetsComboBox();
        }

        public string getSnippetsDirectoryPath()
        {
            string appDir      = Environment.CurrentDirectory;
            string driveLetter = Path.GetPathRoot(appDir);
            return System.IO.Path.Combine(appDir, "snippets");
        }

        private void snippetNewBtn_Click(object sender, EventArgs e)
        {
            string snippetName = snippetNewFileName.Text;
            if (String.IsNullOrEmpty(snippetName))
            {
                MessageBox.Show("Please provide a snippet name.");
                return;
            }
            string asnippetsPath = getSnippetsDirectoryPath();
            bool exists = System.IO.Directory.Exists(asnippetsPath);

            if (!exists)
            {
                System.IO.Directory.CreateDirectory(asnippetsPath);
            }

            System.IO.File.WriteAllText(System.IO.Path.Combine(asnippetsPath, snippetName), snippetsText.Text);
            updateSnippetsComboBox();
            snippetFileName.Text = snippetName;
            //MessageBox.Show(String.Format("Snippet [{0}] successfully saved.", snippetName));
        }

        private void snippetSaveToEspBtn_Click(object sender, EventArgs e)
        {
            Send("uart.setup(0,9600,8,0,1,0)\r\n");
            string luaCode = string.Format(
                "file.remove(\"{0}\")\r\n" +
                "file.open(\"{0}\",\"w\")\r\n" +
                "{1}" +
                "file.close()\r\n", snippetFileName.Text, WrapInWriteLine(snippetsText.Text));

            if (snippetSaveToEspAutoRun.Checked)
            {
                string doFile = string.Format("dofile(\"{0}\")\r\n", snippetsText.Text);
                luaCode += doFile;
            }

            SendLines(luaCode);
        }

        private void serialConnectBtn_Click(object sender, EventArgs e)
        {
            OpenComPort();
            serialConnectBtn.Text = "Connect";
            if(serialPort.IsOpen)
            {
                serialConnectBtn.Text = "Disconnect";
            }
            else
            {
                serialConnectBtn.Text = "Connect";
                serialPort.Close();
            }
        }

        private void mqttBrokerConnectBtn_Click(object sender, EventArgs e)
        {
            string brokerConnectionString = "";
            string brokerKeepAlive = mqttBrokerKeepAlive.Text;
            string brokerClientId  = mqttBrokerClientId.Text;
            string brokerUsername  = mqttBrokerUsername.Text;
            string brokerPassword  = mqttBrokerPassword.Text;
            string brokerPort      = mqttBrokerPort.Text;
            string brokerIp        = mqttBrokerIP.Text;
            int brokerIsSecure     = mqttBrokerIsSecure.Checked ? 1 : 0;

            if(string.IsNullOrWhiteSpace(brokerUsername))
            {
                brokerConnectionString += "m = mqtt.Client(\"" + brokerClientId + "\", " + brokerKeepAlive + ")\r\n";
            }
            else
            {
                brokerConnectionString += "m = mqtt.Client(\"" + brokerClientId + "\", " + brokerKeepAlive + ", \"" + brokerUsername + "\", \"" + brokerPassword + "\")\r\b";
            }
            
            brokerConnectionString += "m:connect(\"" + brokerIp + "\", " + brokerPort + ", " + brokerIsSecure + ", function(conn) print(\"Connected\") end)\r\n";
            brokerConnectionString += "m:on(\"message\", function(conn, topic, data)\r\n";
            brokerConnectionString += "if data ~= nil then\r\n";
            brokerConnectionString += "print(\"TOPIC: [\" .. topic .. \"] MSG: [\" .. data .. \"]\")\r\n";
            brokerConnectionString += "end\r\n";
            brokerConnectionString += "end)\r\n";
            
            SendLines(brokerConnectionString);
        }

        private void mqttSubscribeBtn_Click(object sender, EventArgs e)
        {
            string subscribeTopic = mqttSubscribeTopic.Text;
            string subscribeQoS   = mqttSubscribeQos.Text;

            if(string.IsNullOrWhiteSpace(subscribeTopic))
            {
                MessageBox.Show("Please provide a topic to subscribe to.");
                return;
            }

            if (mqttSubscribeTopic.Items.Contains(subscribeTopic))
            {
                mqttSubscribeTopic.Items.Add(subscribeTopic);
            }

            SendLines("m:subscribe(\"" + subscribeTopic + "\", " + subscribeQoS + ", function(conn) print(\"Subscribed to [" + subscribeTopic + "] successfully.\") end)");
        }

        private void mqttPublishBtn_Click(object sender, EventArgs e)
        {
            string publishTopic  = mqttPublishTopic.Text;
            string publishQoS    = mqttPublishQoS.Text;
            string publishRetain = mqttPublishRetain.Checked ? "1" : "0";
            string publishMsg    = mqttPublishMsg.Text;

            if(string.IsNullOrWhiteSpace(publishMsg))
            {
                MessageBox.Show("Please provide a message to publish.");
                return;
            }

            if(string.IsNullOrWhiteSpace(publishTopic))
            {
                MessageBox.Show("Please provide a topic to publush.");
                return;
            }

            SendLines("m:publish(\"" + publishTopic + "\",\"" + publishMsg + "\", " + publishQoS + ", " + publishRetain + ", function(conn) print(\"Published [" + publishMsg + "] on [" + publishTopic + "]\") end)");
        }

        private void mqttLastWillBtn_Click(object sender, EventArgs e)
        {
            string lwtTopic  = mqttLastWillTopic.Text;
            string lwtQoS    = mqttLastWillQoS.Text;
            string lwtRetain = mqttLastWillRetain.Checked ? "1" : "0";
            string lwtMsg    = mqttLastWillMsg.Text;

            if (string.IsNullOrWhiteSpace(lwtMsg))
            {
                MessageBox.Show("Please provide a message for (LWT) last will testament.");
                return;
            }

            if (string.IsNullOrWhiteSpace(lwtTopic))
            {
                MessageBox.Show("Please provide a topic for (LWT) last will testament.");
                return;
            }

            SendLines("m:lwt(\"" + lwtTopic + "\",\"" + lwtMsg + "\", " + lwtQoS + ", " + lwtRetain + ", function(conn) print(\"Last Will [" + lwtMsg + "] [" + lwtTopic + "]\") end)");
        }

        private void mqttBrokerDisconnectBtn_Click(object sender, EventArgs e)
        {
            SendLines("m:close()\r\n");
        }
    }
}

public class ComboboxItem
{
    public string Text { get; set; }
    public object Value { get; set; }

    public override string ToString()
    {
        return Text;
    }
}