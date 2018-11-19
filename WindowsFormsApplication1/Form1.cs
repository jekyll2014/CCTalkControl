using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private DataTable CommandDatabase = new DataTable();
        private DataTable ResultDatabase = new DataTable();
        private string SourceFile = "default.cct";
        private int SerialtimeOut = 3000;
        private byte deviceAddress = 0;
        private byte hostAddress = 1;
        private bool flag = false;

        public class ResultColumns
        {
            public static int Description { get; set; } = 0;
            public static int Value { get; set; } = 1;
            public static int Type { get; set; } = 2;
            public static int Length { get; set; } = 3;
            public static int Raw { get; set; } = 4;
        }

        private class commandType
        {
            public static int Command = 0;
            public static int Reply = 1;
            public static int Unrecognized = 2;
        }

        private struct command
        {
            public int type;
            public byte[] data;
        }

        //no hex-recognizable chars allowed (0-9, a-f)
        private string[] commandMark = new string[] { "> ", "< ", "? " };
        private List<command> commandList = new List<command>();

        #region Utilities

        public void ReadCsv(string fileName, DataTable table)
        {
            table.Clear();
            table.Columns.Clear();
            FileStream inputFile;
            try
            {
                inputFile = File.OpenRead(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening file:" + fileName + " : " + ex.Message);
                return;
            }

            //read headers
            StringBuilder inputStr = new StringBuilder();
            int c = inputFile.ReadByte();
            while (c != '\r' && c != '\n' && c != -1)
            {
                byte[] b = new byte[1];
                b[0] = (byte)c;
                inputStr.Append(Encoding.GetEncoding(CCTalkControl.Properties.Settings.Default.CodePage).GetString(b));
                c = inputFile.ReadByte();
            }

            //create and count columns and read headers
            int colNum = 0;
            if (inputStr.Length != 0)
            {
                string[] cells = inputStr.ToString().Split(CCTalkControl.Properties.Settings.Default.CSVdelimiter);
                colNum = cells.Length - 1;
                for (int i = 0; i < colNum; i++)
                {
                    table.Columns.Add(cells[i]);
                }
            }

            //read CSV content string by string
            while (c != -1)
            {
                int i = 0;
                c = 0;
                inputStr.Length = 0;
                while (i < colNum && c != -1 /*&& c != '\r' && c != '\n'*/)
                {
                    c = inputFile.ReadByte();
                    byte[] b = new byte[1];
                    b[0] = (byte)c;
                    if (c == CCTalkControl.Properties.Settings.Default.CSVdelimiter) i++;
                    if (c != -1) inputStr.Append(Encoding.GetEncoding(CCTalkControl.Properties.Settings.Default.CodePage).GetString(b));
                }
                while (c != '\r' && c != '\n' && c != -1) c = inputFile.ReadByte();
                if (inputStr.ToString().Replace(CCTalkControl.Properties.Settings.Default.CSVdelimiter, ' ').Trim().TrimStart('\r').TrimStart('\n').TrimEnd('\n').TrimEnd('\r') != "")
                {
                    string[] cells = inputStr.ToString().Split(CCTalkControl.Properties.Settings.Default.CSVdelimiter);

                    DataRow row = table.NewRow();
                    for (i = 0; i < cells.Length - 1; i++)
                    {
                        row[i] = cells[i].TrimStart('\r').TrimStart('\n').TrimEnd('\n').TrimEnd('\r');
                    }
                    table.Rows.Add(row);
                }
            }
            inputFile.Close();
        }

        private command CollectCommand()
        {
            //combine [dest] + [length] + [src] + [cmd] + [data] + [crc]
            string tmpData = "";
            List<byte> data = new List<byte>() { 0, 0, 0 };
            byte b = 0;
            byte.TryParse(textBox_deviceAddress.Text, out b);
            data[0] = b;
            byte.TryParse(textBox_hostAddress.Text, out b);
            data[2] = b;
            for (int i = 0; i < ResultDatabase.Rows.Count; i++) tmpData += ResultDatabase.Rows[i][ResultColumns.Raw].ToString();
            data.AddRange(Accessory.ConvertHexToByteArray(tmpData));
            //0xFD (Address poll) command always to be sent to adress 0
            if (data[3] == 0xfd) data[0] = 0;
            data[1] = (byte)(data.Count - 4);
            ParseEscPos.CrcType = (byte)toolStripComboBox_CrcType.SelectedIndex;
            byte[] tmpCrc = ParseEscPos.GetCRC(data.ToArray(), data.Count);
            if (ParseEscPos.CrcType == ParseEscPos.CrcTypes.SimpleCRC) data.Add(tmpCrc[0]);
            else if (ParseEscPos.CrcType == ParseEscPos.CrcTypes.CRC16)
            {
                byte[] crc = ParseEscPos.GetCRC(data.ToArray(), data.Count);
                data[2] = crc[0];
                data.Add(crc[1]);
            }

            command backData = new command();
            backData.data = data.ToArray();
            backData.type = commandType.Command;
            return backData;
        }

        #endregion

        #region GUI management

        public Form1()
        {
            InitializeComponent();
            SerialtimeOut = CCTalkControl.Properties.Settings.Default.TimeOut;
            toolStripTextBox_TimeOut.Text = SerialtimeOut.ToString();

            deviceAddress = CCTalkControl.Properties.Settings.Default.DefaultDeviceAddress;
            textBox_deviceAddress.Text = deviceAddress.ToString();

            toolStripComboBox_CrcType.SelectedIndex = 0;
            commandsCSV_ToolStripTextBox.Text = CCTalkControl.Properties.Settings.Default.CommandsDatabaseFile;
            ReadCsv(commandsCSV_ToolStripTextBox.Text, CommandDatabase);
            for (int i = 0; i < CommandDatabase.Rows.Count; i++) CommandDatabase.Rows[i][0] = Accessory.CheckHexString(CommandDatabase.Rows[i][0].ToString());
            dataGridView_commands.DataSource = CommandDatabase;

            dataGridView_result.DataSource = ResultDatabase;
            dataGridView_commands.ReadOnly = true;
            ResultDatabase.Columns.Add("Desc");
            ResultDatabase.Columns.Add("Value");
            ResultDatabase.Columns.Add("Type");
            ResultDatabase.Columns.Add("Length");
            ResultDatabase.Columns.Add("Raw");

            ParseEscPos.commandDataBase = CommandDatabase;
            for (int i = 0; i < dataGridView_commands.Columns.Count; i++) dataGridView_commands.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            for (int i = 0; i < dataGridView_result.Columns.Count; i++) dataGridView_result.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView_result.Columns[ResultColumns.Description].ReadOnly = true;
            dataGridView_result.Columns[ResultColumns.Value].ReadOnly = false;
            dataGridView_result.Columns[ResultColumns.Type].ReadOnly = true;
            dataGridView_result.Columns[ResultColumns.Length].ReadOnly = true;
            dataGridView_result.Columns[ResultColumns.Raw].ReadOnly = false;
            SerialPopulate();
            listBox_code.ContextMenuStrip = contextMenuStrip_code;
            dataGridView_commands.ContextMenuStrip = contextMenuStrip_dataBase;
            textBox_deviceAddress_Leave(this, EventArgs.Empty);
            textBox_hostAddress_Leave(this, EventArgs.Empty);

        }

        private void Button_find_Click(object sender, EventArgs e)
        {
            if (listBox_code.SelectedIndex == -1) return;

            //clear the string to be decoded
            byte[] data = commandList[listBox_code.SelectedIndex].data;
            int cmdType = commandList[listBox_code.SelectedIndex].type;

            ResultDatabase.Clear();
            textBox_search.Clear();
            ParseEscPos.sourceData.Clear();
            ParseEscPos.sourceData.AddRange(data);
            ParseEscPos.deviceAddress = deviceAddress;
            ParseEscPos.hostAddress = hostAddress;
            ParseEscPos.CrcType = (byte)toolStripComboBox_CrcType.SelectedIndex;
            int lineNum = -1;
            if (sender == findThisToolStripMenuItem && dataGridView_commands.CurrentCell != null) lineNum = dataGridView_commands.CurrentCell.RowIndex;
            byte command = 0;

            if (data.Length >= 5)
            {
                //check if it's a command or reply

                // if command from host
                if (data[2] == hostAddress)
                {
                    command = data[3];
                }
                // if reply to host
                else if (data[0] == hostAddress)
                {
                    //assume previous string is a command and take command from it
                    if (listBox_code.SelectedIndex > 0) command = commandList[listBox_code.SelectedIndex - 1].data[3];
                    else return;
                }
                else
                {
                    MessageBox.Show("Can't detect if it's reply or command.");
                    return;
                }

                if (ParseEscPos.FindCommand(0, command, lineNum))
                {
                    ParseEscPos.FindCommandParameter();
                    dataGridView_commands.CurrentCell = dataGridView_commands.Rows[ParseEscPos.commandDbLineNum].Cells[ParseEscPos.CSVColumns.CommandName];
                    DataRow row = ResultDatabase.NewRow();
                    if (ParseEscPos.itIsReply) row[ResultColumns.Value] = "[REPLY] " + ParseEscPos.commandName;
                    else row[ResultColumns.Value] = "[COMMAND] " + ParseEscPos.commandName;
                    row[ResultColumns.Raw] = ParseEscPos.commandName;
                    if (ParseEscPos.crcFailed) row[ResultColumns.Description] += "!!!CRC FAILED!!! ";
                    if (ParseEscPos.lengthIncorrect) row[ResultColumns.Description] += "!!!FRAME LENGTH INCORRECT!!! ";
                    row[ResultColumns.Description] += ParseEscPos.commandDesc;

                    ResultDatabase.Rows.Add(row);
                    for (int i = 0; i < ParseEscPos.commandParamDesc.Count; i++)
                    {
                        row = ResultDatabase.NewRow();
                        row[ResultColumns.Value] = ParseEscPos.commandParamValue[i];
                        row[ResultColumns.Type] = ParseEscPos.commandParamType[i];
                        row[ResultColumns.Length] = ParseEscPos.commandParamSizeDefined[i];
                        row[ResultColumns.Raw] = Accessory.ConvertByteArrayToHex(ParseEscPos.commandParamRAWValue[i].ToArray());
                        row[ResultColumns.Description] = ParseEscPos.commandParamDesc[i];
                        ResultDatabase.Rows.Add(row);
                        if (ParseEscPos.commandParamType[i].ToLower() == ParseEscPos.DataTypes.Bitfield)  //add bitfield display
                        {
                            byte b = byte.Parse(ParseEscPos.commandParamValue[i]);
                            for (int i1 = 0; i1 < 8; i1++)
                            {
                                row = ResultDatabase.NewRow();
                                row[ResultColumns.Value] = (Accessory.GetBit(b, (byte)i1) ? (byte)1 : (byte)0).ToString();
                                row[ResultColumns.Type] = "bit" + i1.ToString();
                                row[ResultColumns.Description] = dataGridView_commands.Rows[ParseEscPos.commandParamDbLineNum[i] + i1 + 1].Cells[ParseEscPos.CSVColumns.CommandDescription].Value;
                                ResultDatabase.Rows.Add(row);
                            }
                        }
                    }
                }
                else  //no command found. consider it an unknown string
                {
                    DataRow row = ResultDatabase.NewRow();
                    int i = 1;
                    while (!ParseEscPos.FindCommand(i, command) &&
                        i < data.Length) //looking for a non-parseable part end
                    {
                        i++;
                    }
                    ParseEscPos.commandName = "";
                    row[ResultColumns.Value] += "";
                    row[ResultColumns.Value] += "\"" + Accessory.ConvertByteArrayToHex(data) + "\"";
                    dataGridView_commands.CurrentCell = dataGridView_commands.Rows[0].Cells[0];
                    if (Accessory.PrintableByteArray(data)) row[ResultColumns.Description] = "\"" + Encoding.GetEncoding(CCTalkControl.Properties.Settings.Default.CodePage).GetString(data) + "\"";
                }
            }
        }

        private void Button_next_Click(object sender, EventArgs e)
        {
            if (listBox_code.SelectedIndex == -1)
            {
                if (listBox_code.Items.Count == 0) return;
                else listBox_code.SelectedIndex = 0;
            }
            if (listBox_code.SelectedIndex < listBox_code.Items.Count - 1) listBox_code.SelectedIndex++;
            Button_find_Click(this, EventArgs.Empty);
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SaveBinFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = SourceFile;
            saveFileDialog.Title = "Save BIN file";
            saveFileDialog.DefaultExt = "bin";
            saveFileDialog.Filter = "BIN files|*.bin|PRN files|*.prn|All files|*.*";
            saveFileDialog.ShowDialog();
        }

        private void SaveHexFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = SourceFile;
            saveFileDialog.Title = "Save .CCT hex file";
            saveFileDialog.DefaultExt = "cct";
            saveFileDialog.Filter = "CCT hex files|*.cct|Text files|*.txt|All files|*.*";
            saveFileDialog.ShowDialog();
        }

        private void SaveCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = CCTalkControl.Properties.Settings.Default.CommandsDatabaseFile;
            saveFileDialog.Title = "Save CSV database";
            saveFileDialog.DefaultExt = "csv";
            saveFileDialog.Filter = "CSV files|*.csv|All files|*.*";
            saveFileDialog.ShowDialog();
        }

        private void SaveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (saveFileDialog.Title == "Save .CCT hex file")
            {
                File.WriteAllText(saveFileDialog.FileName, "");
                foreach (string s in listBox_code.Items) File.AppendAllText(saveFileDialog.FileName, s + "\r\n", Encoding.GetEncoding(CCTalkControl.Properties.Settings.Default.CodePage));
            }
            else if (saveFileDialog.Title == "Save CSV database")
            {
                int columnCount = dataGridView_commands.ColumnCount;
                StringBuilder output = new StringBuilder();
                for (int i = 0; i < columnCount; i++)
                {
                    output.Append(dataGridView_commands.Columns[i].Name.ToString());
                    output.Append(";");
                }
                output.Append("\r\n");
                for (int i = 0; i < dataGridView_commands.RowCount; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        output.Append(dataGridView_commands.Rows[i].Cells[j].Value.ToString());
                        output.Append(";");
                    }
                    output.Append("\r\n");
                }
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, output.ToString(), Encoding.GetEncoding(CCTalkControl.Properties.Settings.Default.CodePage));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error writing to file " + saveFileDialog.FileName + ": " + ex.Message);
                }
            }

        }

        private void LoadBinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.Title = "Open BIN file";
            openFileDialog.DefaultExt = "bin";
            openFileDialog.Filter = "BIN files|*.bin|PRN files|*.prn|All files|*.*";
            openFileDialog.ShowDialog();
        }

        private void LoadHexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.Title = "Open .CCT hex file";
            openFileDialog.DefaultExt = "cct";
            openFileDialog.Filter = "CCT Hex files|*.cct|Text files|*.txt|All files|*.*";
            openFileDialog.ShowDialog();
        }

        private void LoadCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.Title = "Open command CSV database";
            openFileDialog.DefaultExt = "csv";
            openFileDialog.Filter = "CSV files|*.csv|All files|*.*";
            openFileDialog.ShowDialog();
        }

        private void OpenFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (openFileDialog.Title == "Open .CCT hex file") //hex text read
            {
                SourceFile = openFileDialog.FileName;
                commandList.Clear();
                listBox_code.Items.Clear();
                try
                {
                    foreach (string s in File.ReadAllLines(SourceFile))
                    {
                        command tmp = new command();
                        tmp.data = Accessory.ConvertHexToByteArray(Accessory.CheckHexString(s));
                        //if command
                        if (tmp.data[2] == 1 && tmp.data[1] == tmp.data.Length - 5) tmp.type = commandType.Command;
                        // if reply to host
                        else if (tmp.data[0] == 1 && tmp.data[1] == tmp.data.Length - 5) tmp.type = commandType.Reply;
                        //if length is not correct or source/destination address not host(always 1)
                        else tmp.type = commandType.Unrecognized;
                        commandList.Add(tmp);
                        listBox_code.Items.Add(commandMark[tmp.type] + Accessory.ConvertByteArrayToHex(tmp.data));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("\r\nError reading file " + SourceFile + ": " + ex.Message);
                }
                //Form1.ActiveForm.Text += " " + SourceFile;
                //sourceData.Clear();
                //sourceData.AddRange(Accessory.ConvertHexToByteArray(textBox_code.Text));
                listBox_code.SelectedIndex = 0;
                //ParseEscPos.Init(listBox_code.Items[0].ToString(), CommandDatabase);
            }
            else if (openFileDialog.Title == "Open command CSV database") //hex text read
            {
                CommandDatabase = new DataTable();
                ReadCsv(openFileDialog.FileName, CommandDatabase);
                for (int i = 0; i < CommandDatabase.Rows.Count; i++) CommandDatabase.Rows[i][0] = Accessory.CheckHexString(CommandDatabase.Rows[i][0].ToString());
                dataGridView_commands.DataSource = CommandDatabase;
                ParseEscPos.commandDataBase = CommandDatabase;
            }
        }

        private void DefaultCSVToolStripTextBox_Leave(object sender, EventArgs e)
        {
            if (commandsCSV_ToolStripTextBox.Text != CCTalkControl.Properties.Settings.Default.CommandsDatabaseFile)
            {
                CCTalkControl.Properties.Settings.Default.CommandsDatabaseFile = commandsCSV_ToolStripTextBox.Text;
                CCTalkControl.Properties.Settings.Default.Save();
            }
        }

        private void EnableDatabaseEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enableDatabaseEditToolStripMenuItem.Checked = !enableDatabaseEditToolStripMenuItem.Checked;
            dataGridView_commands.ReadOnly = !enableDatabaseEditToolStripMenuItem.Checked;
        }

        private void DataGridView_result_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            this.dataGridView_result.CellValueChanged -= new DataGridViewCellEventHandler(this.DataGridView_result_CellValueChanged);
            if (dataGridView_result.CurrentCell.ColumnIndex == ResultColumns.Value)
            {
                if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.Bitfield)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = ParseEscPos.BitfieldToRaw(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value.ToString());
                    byte n = 0;
                    byte.TryParse(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value.ToString(), out n);
                    int i = dataGridView_result.CurrentRow.Index;
                    for (int i1 = 0; i1 < 8; i1++)
                    {
                        dataGridView_result.Rows[i + 1 + i1].Cells[ResultColumns.Value].Value = Convert.ToInt32(Accessory.GetBit(n, (byte)i1)).ToString();
                    }
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.Data)
                {
                    byte n = 0;
                    if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length].Value.ToString() == "?") n = (byte)dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value.ToString().Length;
                    else byte.TryParse(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length].Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = ParseEscPos.DataToRaw(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value.ToString(), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.Number)
                {
                    byte n = 0;
                    byte.TryParse(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length].Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = ParseEscPos.NumberToRaw(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value.ToString(), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.NumberInvert)
                {
                    byte n = 0;
                    byte.TryParse(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length].Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = ParseEscPos.NumberInvertToRaw(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value.ToString(), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.Money)
                {
                    byte n = 0;
                    byte.TryParse(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length].Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = ParseEscPos.MoneyToRaw(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value.ToString(), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.MoneyInvert)
                {
                    byte n = 0;
                    byte.TryParse(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length].Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = ParseEscPos.MoneyInvertToRaw(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value.ToString(), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.String)
                {
                    byte n = 0;
                    if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length].Value.ToString() == "?") n = (byte)dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value.ToString().Length;
                    else byte.TryParse(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length].Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = ParseEscPos.StringToRaw(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value.ToString(), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString().StartsWith("bit"))
                {
                    int i = dataGridView_result.CurrentCell.RowIndex - 1;
                    while (dataGridView_result.Rows[i].Cells[ResultColumns.Type].Value.ToString() != ParseEscPos.DataTypes.Bitfield) i--;
                    //collect bits to int
                    byte n = 0;
                    for (int i1 = 0; i1 < 8; i1++)
                    {
                        if (dataGridView_result.Rows[i + i1 + 1].Cells[ResultColumns.Value].Value.ToString().Trim() == "1") n += (byte)Math.Pow(2, i1);
                    }
                    dataGridView_result.Rows[i].Cells[ResultColumns.Value].Value = n.ToString();
                    dataGridView_result.Rows[i].Cells[ResultColumns.Raw].Value = ParseEscPos.BitfieldToRaw(dataGridView_result.Rows[i].Cells[ResultColumns.Value].Value.ToString());
                }
            }
            else if (dataGridView_result.CurrentCell.ColumnIndex == ResultColumns.Raw)
            {
                dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString());
                if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.String)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString());
                    byte n = 0;
                    if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length].Value.ToString() == "?") n = (byte)(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString().Length / 3);
                    else byte.TryParse(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length].Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value = ParseEscPos.RawToString(Accessory.ConvertHexToByteArray(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString()), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.Number)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString());
                    double l = ParseEscPos.RawToNumber(Accessory.ConvertHexToByteArray(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString()));
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value = l.ToString();
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.NumberInvert)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString());
                    double l = ParseEscPos.RawToNumberInvert(Accessory.ConvertHexToByteArray(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString()));
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value = l.ToString();
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.Money)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString());
                    double l = ParseEscPos.RawToMoney(Accessory.ConvertHexToByteArray(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString()));
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value = l.ToString();
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.MoneyInvert)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString());
                    double l = ParseEscPos.RawToMoneyInvert(Accessory.ConvertHexToByteArray(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString()));
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value = l.ToString();
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.Data)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString());
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value = ParseEscPos.RawToData(Accessory.ConvertHexToByteArray(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString()));
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value.ToString() == ParseEscPos.DataTypes.Bitfield)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value = Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString());
                    double l = ParseEscPos.RawToBitfield(Accessory.ConvertHexToByte(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString()));
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value = l.ToString();
                    int n = 0;
                    int.TryParse(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value.ToString(), out n);
                    int i = dataGridView_result.CurrentRow.Index;
                    for (int i1 = 0; i1 < 8; i1++)
                    {
                        dataGridView_result.Rows[i + 1 + i1].Cells[ResultColumns.Value].Value = (Accessory.GetBit((byte)n, (byte)i1) ? (byte)1 : (byte)0).ToString();
                    }
                }
            }
            this.dataGridView_result.CellValueChanged += new DataGridViewCellEventHandler(this.DataGridView_result_CellValueChanged);
        }

        private void DataGridView_commands_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            Button_newCommand_Click(this, EventArgs.Empty);
        }

        private void Button_add_Click(object sender, EventArgs e)
        {
            command tmpCmd = CollectCommand();
            commandList.Add(tmpCmd);
            listBox_code.Items.Add(commandMark[tmpCmd.type] + Accessory.ConvertByteArrayToHex(tmpCmd.data));
            listBox_code.SelectedIndex = listBox_code.Items.Count - 1;
        }

        private void Button_replace_Click(object sender, EventArgs e)
        {
            if (listBox_code.SelectedIndex == -1) return;
            command tmp = CollectCommand();
            commandList[listBox_code.SelectedIndex] = tmp;
            listBox_code.Items[listBox_code.SelectedIndex] = commandMark[tmp.type] + Accessory.ConvertByteArrayToHex(commandList[listBox_code.SelectedIndex].data);
        }

        private void Button_insert_Click(object sender, EventArgs e)
        {
            if (listBox_code.SelectedIndex == -1) return;
            command tmp = CollectCommand();
            commandList.Insert(listBox_code.SelectedIndex, tmp);
            listBox_code.Items.Insert(listBox_code.SelectedIndex, commandMark[tmp.type] + Accessory.ConvertByteArrayToHex(commandList[listBox_code.SelectedIndex].data));
            listBox_code.SelectedIndex--;
        }

        private void Button_clear_Click(object sender, EventArgs e)
        {
            commandList.Clear();
            listBox_code.Items.Clear();
        }

        private void TextBox_search_TextChanged(object sender, EventArgs e)
        {
            dataGridView_commands.CurrentCell = null;
            DataGridViewRow row;
            if (textBox_search.Text != "")
            {
                for (int i = 0; i < dataGridView_commands.RowCount; i++)
                {
                    row = dataGridView_commands.Rows[i];
                    if (dataGridView_commands.Rows[i].Cells[ParseEscPos.CSVColumns.CommandName].Value.ToString() != "")
                    {
                        if (dataGridView_commands.Rows[i].Cells[ParseEscPos.CSVColumns.CommandDescription].Value.ToString().ToLower().Contains(textBox_search.Text.ToLower()))
                        {
                            row.Visible = true;
                            i++;
                            while (i < dataGridView_commands.RowCount && dataGridView_commands.Rows[i].Cells[ParseEscPos.CSVColumns.CommandName].Value.ToString() == "")
                            {
                                row = dataGridView_commands.Rows[i];
                                row.Visible = true;
                                i++;
                            }
                            i--;
                        }
                        else row.Visible = false;
                    }
                    else row.Visible = false;
                }
            }
            else
            {
                for (int i = 0; i < dataGridView_commands.RowCount; i++)
                {
                    row = dataGridView_commands.Rows[i];
                    row.Visible = true;
                }
            }
        }

        private void Button_newCommand_Click(object sender, EventArgs e)
        {
            //restore 
            ParseEscPos.CSVColumns.CommandDescription = 1;
            ParseEscPos.CSVColumns.CommandParameterSize = 2;
            ParseEscPos.CSVColumns.CommandParameterType = 3;
            ParseEscPos.CSVColumns.CommandParameterValue = 4;
            ParseEscPos.itIsReply = false;

            if (dataGridView_commands.Rows[dataGridView_commands.CurrentCell.RowIndex].Cells[ParseEscPos.CSVColumns.CommandName].Value.ToString() != "")
            {
                int currentRow = dataGridView_commands.CurrentCell.RowIndex;
                ResultDatabase.Clear();
                DataRow row = ResultDatabase.NewRow();
                row[ResultColumns.Value] = dataGridView_commands.Rows[currentRow].Cells[ParseEscPos.CSVColumns.CommandName].Value.ToString();
                row[ResultColumns.Raw] = row[ResultColumns.Value];
                row[ResultColumns.Description] = dataGridView_commands.Rows[currentRow].Cells[ParseEscPos.CSVColumns.CommandDescription].Value.ToString();
                ResultDatabase.Rows.Add(row);

                //collect parameters and fill with user data
                int i = currentRow + 1;
                while (i < dataGridView_commands.Rows.Count && dataGridView_commands.Rows[i].Cells[ParseEscPos.CSVColumns.CommandName].Value.ToString() == "")
                {
                    if (dataGridView_commands.Rows[i].Cells[ParseEscPos.CSVColumns.CommandParameterSize].Value.ToString() != "")
                    {
                        row = ResultDatabase.NewRow();
                        row[ResultColumns.Type] = dataGridView_commands.Rows[i].Cells[ParseEscPos.CSVColumns.CommandParameterType].Value.ToString();
                        row[ResultColumns.Length] = dataGridView_commands.Rows[i].Cells[ParseEscPos.CSVColumns.CommandParameterSize].Value.ToString();
                        row[ResultColumns.Description] = dataGridView_commands.Rows[i].Cells[ParseEscPos.CSVColumns.CommandDescription].Value.ToString();
                        row[ResultColumns.Value] = "";
                        row[ResultColumns.Raw] = "";

                        ResultDatabase.Rows.Add(row);
                        if (row[ResultColumns.Type].ToString() == ParseEscPos.DataTypes.Bitfield)  //decode bitfield
                        {
                            for (int i1 = 0; i1 < 8; i1++)
                            {
                                row = ResultDatabase.NewRow();
                                row[ResultColumns.Value] = "0";
                                row[ResultColumns.Type] = "bit" + i1.ToString();
                                row[ResultColumns.Description] = dataGridView_commands.Rows[i + i1 + 1].Cells[ParseEscPos.CSVColumns.CommandDescription].Value.ToString();
                                ResultDatabase.Rows.Add(row);
                            }
                        }
                    }
                    i++;
                }
            }
        }

        private void ListBox_code_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Button_find_Click(this, EventArgs.Empty);
        }

        private void ListBox_code_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender != listBox_code) return;

            if (listBox_code.SelectedIndex == -1) return;
            //Ctrl-C - copy string to clipboard
            if (e.Control && e.KeyCode == Keys.C && listBox_code.SelectedItem.ToString() != "") Clipboard.SetText(listBox_code.SelectedItem.ToString());
            //Ctrl-Ins - copy string to clipboard
            else if (e.Control && e.KeyCode == Keys.Insert && listBox_code.SelectedItem.ToString() != "") Clipboard.SetText(listBox_code.SelectedItem.ToString());
            //Ctrl-V - insert string from clipboard
            //Shift-Ins - insert string from clipboard
            else if (e.Control && e.KeyCode == Keys.V && Accessory.GetStringFormat(Clipboard.GetText()) == 16 ||
             e.Shift && e.KeyCode == Keys.Insert && Accessory.GetStringFormat(Clipboard.GetText()) == 16)
            {
                command tmp = new command();
                tmp.data = Accessory.ConvertHexToByteArray(Accessory.CheckHexString(Clipboard.GetText()));
                //if command
                if (tmp.data[2] == 1 && tmp.data[1] == tmp.data.Length - 3) tmp.type = commandType.Command;
                // if reply to host
                else if (tmp.data[0] == 1 && tmp.data[1] == tmp.data.Length - 3) tmp.type = commandType.Reply;
                //if unknown
                else tmp.type = commandType.Unrecognized;
                commandList.Add(tmp);

                listBox_code.Items[listBox_code.SelectedIndex] = commandMark[tmp.type] + Accessory.ConvertByteArrayToHex(tmp.data);
            }
            //DEL - delete string
            else if (e.KeyCode == Keys.Delete)
            {
                int i = listBox_code.SelectedIndex;
                listBox_code.Items.RemoveAt(listBox_code.SelectedIndex);
                if (i >= listBox_code.Items.Count) i = listBox_code.Items.Count - 1;
                listBox_code.SelectedIndex = i;
            }
            //Ctrl-P - parse string
            else if (e.Control && e.KeyCode == Keys.P) Button_find_Click(this, EventArgs.Empty);
            //Ctrl-S - send string to device
            else if (e.Control && e.KeyCode == Keys.S && button_Send.Enabled) Button_Send_Click(this, EventArgs.Empty);
        }

        private void ToolStripMenuItem_Connect_Click(object sender, EventArgs e)
        {
            if (toolStripMenuItem_Connect.Checked)
            {
                try
                {
                    SerialPort1.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error closing port " + SerialPort1.PortName + ": " + ex.Message);
                }
                toolStripComboBox_PortName.Enabled = true;
                toolStripComboBox_PortSpeed.Enabled = true;
                toolStripComboBox_PortHandshake.Enabled = true;
                toolStripComboBox_PortDataBits.Enabled = true;
                toolStripComboBox_PortParity.Enabled = true;
                toolStripComboBox_PortStopBits.Enabled = true;
                button_Send.Enabled = false;
                button_SendAll.Enabled = false;
                sendToolStripMenuItem.Enabled = false;
                toolStripMenuItem_Connect.Text = "Connect";
                toolStripMenuItem_Connect.Checked = false;
            }
            else
            {
                if (toolStripComboBox_PortName.SelectedIndex != 0)
                {
                    toolStripComboBox_PortName.Enabled = false;
                    toolStripComboBox_PortSpeed.Enabled = false;
                    toolStripComboBox_PortHandshake.Enabled = false;
                    toolStripComboBox_PortDataBits.Enabled = false;
                    toolStripComboBox_PortParity.Enabled = false;
                    toolStripComboBox_PortStopBits.Enabled = false;

                    SerialPort1.PortName = toolStripComboBox_PortName.Text;
                    SerialPort1.BaudRate = Convert.ToInt32(toolStripComboBox_PortSpeed.Text);
                    SerialPort1.DataBits = Convert.ToUInt16(toolStripComboBox_PortDataBits.Text);
                    SerialPort1.Handshake = (Handshake)Enum.Parse(typeof(Handshake), toolStripComboBox_PortHandshake.Text);
                    SerialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), toolStripComboBox_PortParity.Text);
                    SerialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), toolStripComboBox_PortStopBits.Text);
                    //SerialPort1.ReadTimeout = CustomFiscalControl.Properties.Settings.Default.ReceiveTimeOut;
                    //SerialPort1.WriteTimeout = CustomFiscalControl.Properties.Settings.Default.SendTimeOut;
                    SerialPort1.ReadBufferSize = 8192;
                    try
                    {
                        SerialPort1.Open();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error opening port " + SerialPort1.PortName + ": " + ex.Message);
                        toolStripComboBox_PortName.Enabled = true;
                        toolStripComboBox_PortSpeed.Enabled = true;
                        toolStripComboBox_PortHandshake.Enabled = true;
                        toolStripComboBox_PortDataBits.Enabled = true;
                        toolStripComboBox_PortParity.Enabled = true;
                        toolStripComboBox_PortStopBits.Enabled = true;
                        return;
                    }
                    toolStripMenuItem_Connect.Text = "Disconnect";
                    toolStripMenuItem_Connect.Checked = true;
                    button_Send.Enabled = true;
                    button_SendAll.Enabled = true;
                    sendToolStripMenuItem.Enabled = true;
                }
            }
        }

        private void Button_Send_Click(object sender, EventArgs e)
        {
            if (listBox_code.SelectedIndex < 0 ||
                commandList.Count <= listBox_code.SelectedIndex ||
                commandList[listBox_code.SelectedIndex].data.Length < 5)
            {
                flag = true;
                return;
            }

            byte[] _txBytes = Accessory.ConvertHexToByteArray(Accessory.CheckHexString(listBox_code.SelectedItem.ToString()));

            //if trying to send reply - get back to command and send it. Not in case it's a "Send All"
            if (sender != button_SendAll && _txBytes.Length >= 5 && _txBytes[0] == hostAddress && _txBytes[2] == deviceAddress && listBox_code.SelectedIndex > 0)
            {
                listBox_code.SelectedIndex--;
                Button_Send_Click(this, EventArgs.Empty);
                return;
            }

            if (_txBytes.Length >= 5 && (_txBytes[0] == deviceAddress || _txBytes[0] == 0) && _txBytes[2] == hostAddress)
            {
                command tmpCmd = commandList[listBox_code.SelectedIndex];
                tmpCmd.type = commandType.Command;
                commandList[listBox_code.SelectedIndex] = tmpCmd;
                listBox_code.Items[listBox_code.SelectedIndex] = commandMark[tmpCmd.type] + Accessory.ConvertByteArrayToHex(tmpCmd.data);
                try
                {
                    SerialPort1.DiscardInBuffer();
                    SerialPort1.Write(_txBytes, 0, _txBytes.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error sending to port " + SerialPort1.PortName + ": " + ex.Message);
                }
                List<byte> _rxBytes = new List<byte>();
                //copy of request deleted from receive buffer
                bool dupeDeleted = false;
                //source/destination address of reply is incorrect
                bool _notReply = false;
                //broadcast message received
                bool _broadcastReply = false;
                //ACK received
                bool _ack = false;
                //NACK received
                bool _nack = false;
                //CRC doesn't match 
                bool _crcError = false;
                //complete frame received 
                bool _frameOK = false;
                //ended with timeout
                bool _timeout = false;
                byte _frameLength = 0;
                DateTime startTime = DateTime.UtcNow;
                try
                {
                    while (!_timeout && !_frameOK)
                    {
                        if (!dupeDeleted)
                        {
                            if (SerialPort1.BytesToRead != 0)
                            {
                                _rxBytes.Add((byte)SerialPort1.ReadByte());
                            }
                            if (Accessory.ByteArrayCompare(_rxBytes.ToArray(), _txBytes))
                            {
                                dupeDeleted = true;
                                _rxBytes.Clear();
                            }
                        }
                        if (dupeDeleted)
                        {
                            while (SerialPort1.BytesToRead > 0 && _rxBytes.Count < 4)
                            {
                                _rxBytes.Add((byte)SerialPort1.ReadByte());
                            }
                            if (_rxBytes.Count >= 4 && ((_rxBytes[0] == hostAddress && _rxBytes[2] == deviceAddress) || (_rxBytes[0] == deviceAddress && _rxBytes[2] == hostAddress)))
                            {
                                if (_rxBytes[0] == 0) _broadcastReply = true;
                                else if (_rxBytes[0] != hostAddress || _rxBytes[2] != deviceAddress) _notReply = true;
                                _frameLength = _rxBytes[1];
                                //ACK
                                if (_frameLength == 0 && _rxBytes[3] == ParseEscPos.ackSign) _ack = true;
                                //NACK
                                else if (_frameLength == 0 && _rxBytes[3] == ParseEscPos.nackSign) _nack = true;
                                //BUSY
                                else if (_frameLength == 0 && _rxBytes[3] == ParseEscPos.busySign) _nack = true;
                                //normal reply
                                while (SerialPort1.BytesToRead > 0 && _rxBytes.Count < _frameLength + 5)
                                {
                                    _rxBytes.Add((byte)SerialPort1.ReadByte());
                                }
                                if (_rxBytes.Count >= _frameLength + 5)
                                {
                                    _frameOK = true;
                                    byte[] crc = new byte[2];
                                    crc = ParseEscPos.GetCRC(_rxBytes.GetRange(0, _frameLength + 5).ToArray(), _rxBytes.Count - 1);
                                    if (toolStripComboBox_CrcType.SelectedIndex == ParseEscPos.CrcTypes.SimpleCRC)
                                    {
                                        if (crc[0] != _rxBytes[_rxBytes.Count - 1]) _crcError = true;
                                    }
                                    else if (toolStripComboBox_CrcType.SelectedIndex == ParseEscPos.CrcTypes.CRC16)
                                    {
                                        if (crc[1] != _rxBytes[_rxBytes.Count - 1] || crc[0] != _rxBytes[2]) _crcError = true;
                                    }
                                }
                            }
                            else if (SerialPort1.BytesToRead > 0)
                                _rxBytes.Add((byte)SerialPort1.ReadByte());
                        }
                        if (DateTime.UtcNow.Subtract(startTime).TotalMilliseconds > SerialtimeOut) _timeout = true;
                        //if (SerialPort1.BytesToRead <= 0 && DateTime.UtcNow.Subtract(startTime).TotalMilliseconds > SerialtimeOut) _timeout = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading port " + SerialPort1.PortName + ": " + ex.Message);
                }
                if (_frameOK || showIncorrectRepliesToolStripMenuItem.Checked)
                {
                    command tmp = new command();
                    tmp.type = commandType.Reply;
                    if (!_frameOK || _notReply || _crcError) tmp.type = commandType.Unrecognized;
                    tmp.data = _rxBytes.ToArray();
                    //if command line is the last - add reply to the bottom of the list
                    if (listBox_code.SelectedIndex + 1 >= listBox_code.Items.Count)
                    {
                        commandList.Add(tmp);
                        listBox_code.Items.Add(commandMark[tmp.type] + Accessory.ConvertByteArrayToHex(tmp.data));
                    }
                    //if next item in the list is reply (supposed to be previous reply) - replace it with new reply
                    else if (commandList[listBox_code.SelectedIndex + 1].data.Length >= 3 &&
                    commandList[listBox_code.SelectedIndex + 1].data[0] == hostAddress &&
                    commandList[listBox_code.SelectedIndex + 1].data[2] == deviceAddress)
                    {
                        commandList[listBox_code.SelectedIndex + 1] = tmp;
                        listBox_code.Items[listBox_code.SelectedIndex + 1] = commandMark[tmp.type] + Accessory.ConvertByteArrayToHex(tmp.data);
                    }
                    //if next item in the list is empty (supposed to be previous incorrect reply) - replace it with new reply
                    else if (commandList[listBox_code.SelectedIndex + 1].data.Length == 0)
                    {
                        commandList[listBox_code.SelectedIndex + 1] = tmp;
                        listBox_code.Items[listBox_code.SelectedIndex + 1] = commandMark[tmp.type] + Accessory.ConvertByteArrayToHex(tmp.data);
                    }
                    //or insert new reply next line to command
                    else
                    {
                        commandList.Insert(listBox_code.SelectedIndex + 1, tmp);
                        listBox_code.Items.Insert(listBox_code.SelectedIndex + 1, commandMark[tmp.type] + Accessory.ConvertByteArrayToHex(tmp.data));
                    }
                    if (autoParseReplyToolStripMenuItem.Checked) Button_next_Click(this, EventArgs.Empty);
                }
            }
            else
            {
                flag = true;
                MessageBox.Show("Incorrect host/device address or data incomplete");
            }
        }

        private void Button_SendAll_Click(object sender, EventArgs e)
        {
            flag = false;
            if (listBox_code.SelectedIndex < 0) listBox_code.SelectedIndex = 0;
            for (int i = listBox_code.SelectedIndex; i < listBox_code.Items.Count; i++)
            {
                listBox_code.SelectedIndex = i;
                if (commandList[i].data.Length >= 5) //check minimum packet length
                {
                    Button_Send_Click(button_SendAll, EventArgs.Empty);
                    if (flag) break;
                }
            }
            flag = false;
        }

        private void SerialPopulate()
        {
            toolStripComboBox_PortName.Items.Clear();
            toolStripComboBox_PortHandshake.Items.Clear();
            toolStripComboBox_PortParity.Items.Clear();
            toolStripComboBox_PortStopBits.Items.Clear();
            //Serial settings populate
            toolStripComboBox_PortName.Items.Add("-None-");
            //Add ports
            foreach (string s in SerialPort.GetPortNames())
            {
                toolStripComboBox_PortName.Items.Add(s);
            }
            //Add handshake methods
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                toolStripComboBox_PortHandshake.Items.Add(s);
            }
            //Add parity
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                toolStripComboBox_PortParity.Items.Add(s);
            }
            //Add stopbits
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                toolStripComboBox_PortStopBits.Items.Add(s);
            }
            toolStripComboBox_PortName.SelectedIndex = toolStripComboBox_PortName.Items.Count - 1;
            if (toolStripComboBox_PortName.Items.Count == 1)
            {
                toolStripMenuItem_Connect.Enabled = false;
            }
            toolStripComboBox_PortSpeed.SelectedIndex = 6;
            toolStripComboBox_PortHandshake.SelectedIndex = 0;
            toolStripComboBox_PortDataBits.SelectedIndex = 0;
            toolStripComboBox_PortParity.SelectedIndex = 0;
            toolStripComboBox_PortStopBits.SelectedIndex = 1;
            if (toolStripComboBox_PortName.SelectedIndex == 0) toolStripMenuItem_Connect.Enabled = false;
            else toolStripMenuItem_Connect.Enabled = true;
        }

        private void ToolStripTextBox_TimeOut_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(toolStripTextBox_TimeOut.Text, out SerialtimeOut)) toolStripTextBox_TimeOut.Text = "3000";
        }

        private void ListBox_code_MouseUp(object sender, MouseEventArgs e)
        {
            int index = this.listBox_code.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches && e.Button == MouseButtons.Right)
            {
                listBox_code.SelectedIndex = index;
                contextMenuStrip_code.Visible = true;
            }
            else
            {
                contextMenuStrip_code.Visible = false;
            }
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox_code.SelectedItem.ToString() != "") Clipboard.SetText(listBox_code.SelectedItem.ToString());
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox_code.SelectedIndex == -1) return;
            listBox_code.Items.RemoveAt(listBox_code.SelectedIndex);
        }

        private void ParseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Button_find_Click(this, EventArgs.Empty);
        }

        private void SendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Button_Send_Click(this, EventArgs.Empty);
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Accessory.GetStringFormat(Clipboard.GetText()) == 16)
            {
                listBox_code.Items[listBox_code.SelectedIndex] = Accessory.CheckHexString(Clipboard.GetText());
            }
        }

        private void NewCommandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Button_newCommand_Click(this, EventArgs.Empty);
        }

        private void FindThisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Button_find_Click(findThisToolStripMenuItem, EventArgs.Empty);
        }

        private void DataGridView_commands_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0) dataGridView_commands.CurrentCell = dataGridView_commands.Rows[e.RowIndex].Cells[e.ColumnIndex];
        }

        private void COMPortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (toolStripComboBox_PortName.Enabled) SerialPopulate();
        }

        private void showIncorrectRepliesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showIncorrectRepliesToolStripMenuItem.Checked = !showIncorrectRepliesToolStripMenuItem.Checked;
        }

        private void autoParseReplyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoParseReplyToolStripMenuItem.Checked = !autoParseReplyToolStripMenuItem.Checked;
        }

        private void Button_removeReplies_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < commandList.Count; i++)
            {
                if (commandList[i].type == commandType.Reply)
                {
                    commandList.RemoveAt(i);
                    listBox_code.Items.RemoveAt(i);
                    i--;
                }
            }
        }

        private void toolStripComboBox_CrcType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ParseEscPos.CrcType = (byte)toolStripComboBox_CrcType.SelectedIndex;
        }

        #endregion

        private void textBox_deviceAddress_Leave(object sender, EventArgs e)
        {
            deviceAddress = 0;
            byte.TryParse(textBox_deviceAddress.Text, out deviceAddress);
        }

        private void textBox_hostAddress_Leave(object sender, EventArgs e)
        {
            hostAddress = 0;
            byte.TryParse(textBox_hostAddress.Text, out hostAddress);
        }
    }
}