using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

using CCTalkControl.Properties;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private DataTable _commandDatabase = new DataTable();
        private readonly DataTable _resultDatabase = new DataTable();
        private string _sourceFile = "default.cct";
        private int _serialTimeOut = 3000;
        private byte _deviceAddress;
        private byte _hostAddress = 1;
        private bool _flag;
        private const int MinFrameLength = 5;

        private class ResultColumns
        {
            public static int Description { get; set; } = 0;
            public static int Value { get; set; } = 1;
            public static int Type { get; set; } = 2;
            public static int Length { get; set; } = 3;
            public static int Raw { get; set; } = 4;
        }

        private class commandType
        {
            public static readonly int Command = 0;
            public static readonly int Reply = 1;
            public static readonly int Unrecognized = 2;
        }

        private struct Command
        {
            public int Type;
            public byte[] Data;
        }

        private readonly string[] commandMark = { "> ", "< ", "? " };
        private readonly List<Command> commandList = new List<Command>();

        #region Utilities

        private void ReadCsv(string fileName, DataTable table)
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
            var inputStr = new StringBuilder();
            var c = inputFile.ReadByte();
            while (c != '\r' && c != '\n' && c != -1)
            {
                var b = new byte[1];
                b[0] = (byte)c;
                inputStr.Append(Encoding.GetEncoding(Settings.Default.CodePage).GetString(b));
                c = inputFile.ReadByte();
            }

            //create and count columns and read headers
            var colNum = 0;
            if (inputStr.Length != 0)
            {
                var cells = inputStr.ToString().Split(Settings.Default.CSVdelimiter);
                colNum = cells.Length - 1;
                for (var i = 0; i < colNum; i++) table.Columns.Add(cells[i]);
            }

            //read CSV content string by string
            while (c != -1)
            {
                var i = 0;
                c = 0;
                inputStr.Length = 0;
                while (i < colNum && c != -1 /*&& c != '\r' && c != '\n'*/)
                {
                    c = inputFile.ReadByte();
                    var b = new byte[1];
                    b[0] = (byte)c;
                    if (c == Settings.Default.CSVdelimiter) i++;
                    if (c != -1) inputStr.Append(Encoding.GetEncoding(Settings.Default.CodePage).GetString(b));
                }

                while (c != '\r' && c != '\n' && c != -1) c = inputFile.ReadByte();
                if (inputStr.ToString().Replace(Settings.Default.CSVdelimiter, ' ').Trim().TrimStart('\r')
                    .TrimStart('\n').TrimEnd('\n').TrimEnd('\r') != "")
                {
                    var cells = inputStr.ToString().Split(Settings.Default.CSVdelimiter);

                    var row = table.NewRow();
                    for (i = 0; i < cells.Length - 1; i++)
                        row[i] = cells[i].TrimStart('\r').TrimStart('\n').TrimEnd('\n').TrimEnd('\r');
                    table.Rows.Add(row);
                }
            }

            inputFile.Close();
        }

        private Command CollectCommand()
        {
            //combine [dest] + [length] + [src] + [cmd] + [data] + [crc]
            var tmpData = "";
            var data = new List<byte> { 0, 0, 0 };
            byte.TryParse(textBox_deviceAddress.Text, out var b);
            data[0] = b;
            byte.TryParse(textBox_hostAddress.Text, out b);
            data[2] = b;
            for (var i = 0; i < _resultDatabase.Rows.Count; i++)
                tmpData += _resultDatabase.Rows[i][ResultColumns.Raw].ToString();
            data.AddRange(Accessory.ConvertHexToByteArray(tmpData));
            //0xFD (Address poll) command always to be sent to adress 0
            if (data[3] == 0xfd) data[0] = 0;
            data[1] = (byte)(data.Count - 4);
            ParseEscPos.CrcType = (byte)toolStripComboBox_CrcType.SelectedIndex;
            var tmpCrc = ParseEscPos.GetCRC(data.ToArray(), data.Count);
            if (ParseEscPos.CrcType == ParseEscPos.CrcTypes.SimpleCRC)
            {
                data.Add(tmpCrc[0]);
            }
            else if (ParseEscPos.CrcType == ParseEscPos.CrcTypes.CRC16)
            {
                var crc = ParseEscPos.GetCRC(data.ToArray(), data.Count);
                data[2] = crc[0];
                data.Add(crc[1]);
            }

            var backData = new Command { Data = data.ToArray(), Type = commandType.Command };
            return backData;
        }

        #endregion

        #region GUI management

        public Form1()
        {
            InitializeComponent();
            _serialTimeOut = Settings.Default.TimeOut;
            toolStripTextBox_TimeOut.Text = _serialTimeOut.ToString();

            _deviceAddress = Settings.Default.DefaultDeviceAddress;
            textBox_deviceAddress.Text = _deviceAddress.ToString();

            toolStripComboBox_CrcType.SelectedIndex = 0;
            commandsCSV_ToolStripTextBox.Text = Settings.Default.CommandsDatabaseFile;
            ReadCsv(commandsCSV_ToolStripTextBox.Text, _commandDatabase);
            for (var i = 0; i < _commandDatabase.Rows.Count; i++)
                _commandDatabase.Rows[i][0] = Accessory.CheckHexString(_commandDatabase.Rows[i][0].ToString());
            dataGridView_commands.DataSource = _commandDatabase;

            dataGridView_result.DataSource = _resultDatabase;
            dataGridView_commands.ReadOnly = true;
            _resultDatabase.Columns.Add("Desc");
            _resultDatabase.Columns.Add("Value");
            _resultDatabase.Columns.Add("Type");
            _resultDatabase.Columns.Add("Length");
            _resultDatabase.Columns.Add("Raw");

            ParseEscPos.commandDataBase = _commandDatabase;
            for (var i = 0; i < dataGridView_commands.Columns.Count; i++)
                dataGridView_commands.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            for (var i = 0; i < dataGridView_result.Columns.Count; i++)
                dataGridView_result.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView_result.Columns[ResultColumns.Description].ReadOnly = true;
            dataGridView_result.Columns[ResultColumns.Value].ReadOnly = false;
            dataGridView_result.Columns[ResultColumns.Type].ReadOnly = true;
            dataGridView_result.Columns[ResultColumns.Length].ReadOnly = true;
            dataGridView_result.Columns[ResultColumns.Raw].ReadOnly = false;
            SerialPopulate();
            listBox_code.ContextMenuStrip = contextMenuStrip_code;
            dataGridView_commands.ContextMenuStrip = contextMenuStrip_dataBase;
            TextBox_deviceAddress_Leave(this, EventArgs.Empty);
            TextBox_hostAddress_Leave(this, EventArgs.Empty);
        }

        private void Button_find_Click(object sender, EventArgs e)
        {
            if (listBox_code.SelectedIndex == -1) return;

            //clear the string to be decoded
            var data = commandList[listBox_code.SelectedIndex].Data;
            var cmdType = commandList[listBox_code.SelectedIndex].Type;

            _resultDatabase.Clear();
            textBox_search.Clear();
            ParseEscPos.sourceData.Clear();
            ParseEscPos.sourceData.AddRange(data);
            ParseEscPos.deviceAddress = _deviceAddress;
            ParseEscPos.hostAddress = _hostAddress;
            ParseEscPos.CrcType = (byte)toolStripComboBox_CrcType.SelectedIndex;
            var lineNum = -1;
            if (sender == findThisToolStripMenuItem && dataGridView_commands.CurrentCell != null)
                lineNum = dataGridView_commands.CurrentCell.RowIndex;

            if (data.Length >= MinFrameLength)
            {
                //check if it's a command or reply

                // if command from host
                byte command = 0;
                if (data[2] == _hostAddress)
                {
                    command = data[3];
                }
                // if reply to host
                else if (data[0] == _hostAddress)
                {
                    //assume previous string is a command and take command from it
                    if (listBox_code.SelectedIndex > 0) command = commandList[listBox_code.SelectedIndex - 1].Data[3];
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
                    dataGridView_commands.CurrentCell = dataGridView_commands.Rows[ParseEscPos.commandDbLineNum]
                        .Cells[ParseEscPos.CSVColumns.CommandName];
                    var row = _resultDatabase.NewRow();
                    if (ParseEscPos.itIsReply) row[ResultColumns.Value] = "[REPLY] " + ParseEscPos.commandName;
                    else row[ResultColumns.Value] = "[COMMAND] " + ParseEscPos.commandName;
                    row[ResultColumns.Raw] = ParseEscPos.commandName;
                    if (ParseEscPos.crcFailed) row[ResultColumns.Description] += "!!!CRC FAILED!!! ";
                    if (ParseEscPos.lengthIncorrect) row[ResultColumns.Description] += "!!!FRAME LENGTH INCORRECT!!! ";
                    row[ResultColumns.Description] += ParseEscPos.commandDesc;

                    _resultDatabase.Rows.Add(row);
                    for (var i = 0; i < ParseEscPos.commandParamDesc.Count; i++)
                    {
                        row = _resultDatabase.NewRow();
                        row[ResultColumns.Value] = ParseEscPos.commandParamValue[i];
                        row[ResultColumns.Type] = ParseEscPos.commandParamType[i];
                        row[ResultColumns.Length] = ParseEscPos.commandParamSizeDefined[i];
                        row[ResultColumns.Raw] =
                            Accessory.ConvertByteArrayToHex(ParseEscPos.commandParamRAWValue[i].ToArray());
                        row[ResultColumns.Description] = ParseEscPos.commandParamDesc[i];
                        _resultDatabase.Rows.Add(row);
                        if (ParseEscPos.commandParamType[i].ToLower() == ParseEscPos.DataTypes.Bitfield
                        ) //add bitfield display
                        {
                            var b = byte.Parse(ParseEscPos.commandParamValue[i]);
                            for (var i1 = 0; i1 < 8; i1++)
                            {
                                row = _resultDatabase.NewRow();
                                row[ResultColumns.Value] =
                                    (Accessory.GetBit(b, (byte)i1) ? (byte)1 : (byte)0).ToString();
                                row[ResultColumns.Type] = "bit" + i1;
                                row[ResultColumns.Description] = dataGridView_commands
                                    .Rows[ParseEscPos.commandParamDbLineNum[i] + i1 + 1]
                                    .Cells[ParseEscPos.CSVColumns.CommandDescription].Value;
                                _resultDatabase.Rows.Add(row);
                            }
                        }
                    }
                }
                else //no command found. consider it an unknown string
                {
                    var row = _resultDatabase.NewRow();
                    var i = 1;
                    while (!ParseEscPos.FindCommand(i, command) &&
                           i < data.Length) //looking for a non-parseable part end
                        i++;
                    ParseEscPos.commandName = "";
                    row[ResultColumns.Value] += "";
                    row[ResultColumns.Value] += "\"" + Accessory.ConvertByteArrayToHex(data) + "\"";
                    dataGridView_commands.CurrentCell = dataGridView_commands.Rows[0].Cells[0];
                    if (Accessory.PrintableByteArray(data))
                        row[ResultColumns.Description] =
                            "\"" + Encoding.GetEncoding(Settings.Default.CodePage).GetString(data) + "\"";
                }
            }
        }

        private void Button_next_Click(object sender, EventArgs e)
        {
            if (listBox_code.SelectedIndex == -1)
            {
                if (listBox_code.Items.Count == 0) return;
                listBox_code.SelectedIndex = 0;
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
            saveFileDialog.FileName = _sourceFile;
            saveFileDialog.Title = "Save BIN file";
            saveFileDialog.DefaultExt = "bin";
            saveFileDialog.Filter = "BIN files|*.bin|PRN files|*.prn|All files|*.*";
            saveFileDialog.ShowDialog();
        }

        private void SaveHexFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = _sourceFile;
            saveFileDialog.Title = "Save .CCT hex file";
            saveFileDialog.DefaultExt = "cct";
            saveFileDialog.Filter = "CCT hex files|*.cct|Text files|*.txt|All files|*.*";
            saveFileDialog.ShowDialog();
        }

        private void SaveCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = Settings.Default.CommandsDatabaseFile;
            saveFileDialog.Title = "Save CSV database";
            saveFileDialog.DefaultExt = "csv";
            saveFileDialog.Filter = "CSV files|*.csv|All files|*.*";
            saveFileDialog.ShowDialog();
        }

        private void SaveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (saveFileDialog.Title == "Save .CCT hex file")
            {
                File.WriteAllText(saveFileDialog.FileName, "");
                foreach (string s in listBox_code.Items)
                    File.AppendAllText(saveFileDialog.FileName, s + "\r\n",
                        Encoding.GetEncoding(Settings.Default.CodePage));
            }
            else if (saveFileDialog.Title == "Save CSV database")
            {
                var columnCount = dataGridView_commands.ColumnCount;
                var output = new StringBuilder();
                for (var i = 0; i < columnCount; i++)
                {
                    output.Append(dataGridView_commands.Columns[i].Name);
                    output.Append(";");
                }

                output.Append("\r\n");
                for (var i = 0; i < dataGridView_commands.RowCount; i++)
                {
                    for (var j = 0; j < columnCount; j++)
                    {
                        output.Append(dataGridView_commands.Rows[i].Cells[j].Value);
                        output.Append(";");
                    }

                    output.Append("\r\n");
                }

                try
                {
                    File.WriteAllText(saveFileDialog.FileName, output.ToString(),
                        Encoding.GetEncoding(Settings.Default.CodePage));
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

        private void OpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (openFileDialog.Title == "Open .CCT hex file") //hex text read
            {
                _sourceFile = openFileDialog.FileName;
                commandList.Clear();
                listBox_code.Items.Clear();
                try
                {
                    foreach (var s in File.ReadAllLines(_sourceFile))
                    {
                        var tmp = new Command();
                        tmp.Data = Accessory.ConvertHexToByteArray(Accessory.CheckHexString(s));
                        //if command
                        if (tmp.Data[2] == _hostAddress && tmp.Data[1] == tmp.Data.Length - MinFrameLength)
                            tmp.Type = commandType.Command;
                        // if reply to host
                        else if (tmp.Data[0] == _hostAddress && tmp.Data[1] == tmp.Data.Length - MinFrameLength)
                            tmp.Type = commandType.Reply;
                        //if length is not correct or source/destination address not host(always 1)
                        else tmp.Type = commandType.Unrecognized;
                        commandList.Add(tmp);
                        listBox_code.Items.Add(commandMark[tmp.Type] + Accessory.ConvertByteArrayToHex(tmp.Data));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("\r\nError reading file " + _sourceFile + ": " + ex.Message);
                }

                listBox_code.SelectedIndex = 0;
            }
            else if (openFileDialog.Title == "Open command CSV database") //hex text read
            {
                _commandDatabase = new DataTable();
                ReadCsv(openFileDialog.FileName, _commandDatabase);
                for (var i = 0; i < _commandDatabase.Rows.Count; i++)
                    _commandDatabase.Rows[i][0] = Accessory.CheckHexString(_commandDatabase.Rows[i][0].ToString());
                dataGridView_commands.DataSource = _commandDatabase;
                ParseEscPos.commandDataBase = _commandDatabase;
            }
        }

        private void DefaultCSVToolStripTextBox_Leave(object sender, EventArgs e)
        {
            if (commandsCSV_ToolStripTextBox.Text != Settings.Default.CommandsDatabaseFile)
            {
                Settings.Default.CommandsDatabaseFile = commandsCSV_ToolStripTextBox.Text;
                Settings.Default.Save();
            }
        }

        private void EnableDatabaseEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enableDatabaseEditToolStripMenuItem.Checked = !enableDatabaseEditToolStripMenuItem.Checked;
            dataGridView_commands.ReadOnly = !enableDatabaseEditToolStripMenuItem.Checked;
        }

        private void DataGridView_result_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView_result.CellValueChanged -= DataGridView_result_CellValueChanged;
            if (dataGridView_result.CurrentCell.ColumnIndex == ResultColumns.Value)
            {
                byte n = 0;
                if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value
                    .ToString() == ParseEscPos.DataTypes.Bitfield)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        ParseEscPos.BitfieldToRaw(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                            .Cells[ResultColumns.Value].Value.ToString());
                    byte.TryParse(
                        dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value]
                            .Value.ToString(), out n);
                    var i = dataGridView_result.CurrentRow.Index;
                    for (var i1 = 0; i1 < 8; i1++)
                        dataGridView_result.Rows[i + 1 + i1].Cells[ResultColumns.Value].Value =
                            Convert.ToInt32(Accessory.GetBit(n, (byte)i1)).ToString();
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type]
                    .Value.ToString() == ParseEscPos.DataTypes.Data)
                {
                    if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length]
                            .Value.ToString() ==
                        "?")
                        n = (byte)dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                            .Cells[ResultColumns.Value].Value.ToString().Length;
                    else
                        byte.TryParse(
                            dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                                .Cells[ResultColumns.Length].Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        ParseEscPos.DataToRaw(
                            dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                                .Cells[ResultColumns.Value].Value.ToString(), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type]
                    .Value.ToString() == ParseEscPos.DataTypes.Number)
                {
                    byte.TryParse(
                        dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length]
                            .Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        ParseEscPos.NumberToRaw(
                            dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                                .Cells[ResultColumns.Value].Value.ToString(), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type]
                    .Value.ToString() == ParseEscPos.DataTypes.NumberInvert)
                {
                    byte.TryParse(
                        dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length]
                            .Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        ParseEscPos.NumberInvertToRaw(
                            dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                                .Cells[ResultColumns.Value].Value.ToString(), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type]
                    .Value.ToString() == ParseEscPos.DataTypes.Money)
                {
                    byte.TryParse(
                        dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length]
                            .Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        ParseEscPos.MoneyToRaw(
                            dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                                .Cells[ResultColumns.Value].Value.ToString(), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type]
                    .Value.ToString() == ParseEscPos.DataTypes.MoneyInvert)
                {
                    byte.TryParse(
                        dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length]
                            .Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        ParseEscPos.MoneyInvertToRaw(
                            dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                                .Cells[ResultColumns.Value].Value.ToString(), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type]
                    .Value.ToString() == ParseEscPos.DataTypes.String)
                {
                    if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length]
                            .Value.ToString() ==
                        "?")
                        n = (byte)dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                            .Cells[ResultColumns.Value].Value.ToString().Length;
                    else
                        byte.TryParse(
                            dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                                .Cells[ResultColumns.Length].Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        ParseEscPos.StringToRaw(
                            dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                                .Cells[ResultColumns.Value].Value.ToString(), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type]
                    .Value.ToString().StartsWith("bit"))
                {
                    var i = dataGridView_result.CurrentCell.RowIndex - 1;
                    while (dataGridView_result.Rows[i].Cells[ResultColumns.Type].Value.ToString() !=
                           ParseEscPos.DataTypes.Bitfield) i--;
                    //collect bits to int
                    for (var i1 = 0; i1 < 8; i1++)
                        if (dataGridView_result.Rows[i + i1 + 1].Cells[ResultColumns.Value].Value.ToString().Trim() ==
                            "1")
                            n += (byte)Math.Pow(2, i1);
                    dataGridView_result.Rows[i].Cells[ResultColumns.Value].Value = n.ToString();
                    dataGridView_result.Rows[i].Cells[ResultColumns.Raw].Value =
                        ParseEscPos.BitfieldToRaw(dataGridView_result.Rows[i].Cells[ResultColumns.Value].Value
                            .ToString());
                }
            }
            else if (dataGridView_result.CurrentCell.ColumnIndex == ResultColumns.Raw)
            {
                dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                    Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                        .Cells[ResultColumns.Raw].Value.ToString());
                if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type].Value
                    .ToString() == ParseEscPos.DataTypes.String)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                            .Cells[ResultColumns.Raw].Value.ToString());
                    byte n = 0;
                    if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Length]
                            .Value.ToString() ==
                        "?")
                        n = (byte)(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                            .Cells[ResultColumns.Raw].Value.ToString().Length / 3);
                    else
                        byte.TryParse(
                            dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                                .Cells[ResultColumns.Length].Value.ToString(), out n);
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value
                        = ParseEscPos.RawToString(
                            Accessory.ConvertHexToByteArray(dataGridView_result
                                .Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value
                                .ToString()), n);
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type]
                    .Value.ToString() == ParseEscPos.DataTypes.Number)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                            .Cells[ResultColumns.Raw].Value.ToString());
                    var l = ParseEscPos.RawToNumber(Accessory.ConvertHexToByteArray(dataGridView_result
                        .Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString()));
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value]
                        .Value = l.ToString();
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type]
                    .Value.ToString() == ParseEscPos.DataTypes.NumberInvert)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                            .Cells[ResultColumns.Raw].Value.ToString());
                    var l = ParseEscPos.RawToNumberInvert(Accessory.ConvertHexToByteArray(dataGridView_result
                        .Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString()));
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value]
                        .Value = l.ToString();
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type]
                    .Value.ToString() == ParseEscPos.DataTypes.Money)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                            .Cells[ResultColumns.Raw].Value.ToString());
                    var l = ParseEscPos.RawToMoney(Accessory.ConvertHexToByteArray(dataGridView_result
                        .Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString()));
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value]
                        .Value = l.ToString();
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type]
                    .Value.ToString() == ParseEscPos.DataTypes.MoneyInvert)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                            .Cells[ResultColumns.Raw].Value.ToString());
                    var l = ParseEscPos.RawToMoneyInvert(Accessory.ConvertHexToByteArray(dataGridView_result
                        .Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString()));
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value]
                        .Value = l.ToString();
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type]
                    .Value.ToString() == ParseEscPos.DataTypes.Data)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                            .Cells[ResultColumns.Raw].Value.ToString());
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value].Value
                        = ParseEscPos.RawToData(Accessory.ConvertHexToByteArray(dataGridView_result
                            .Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString()));
                }
                else if (dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Type]
                    .Value.ToString() == ParseEscPos.DataTypes.Bitfield)
                {
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value =
                        Accessory.CheckHexString(dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex]
                            .Cells[ResultColumns.Raw].Value.ToString());
                    var l = ParseEscPos.RawToBitfield(Accessory.ConvertHexToByte(dataGridView_result
                        .Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Raw].Value.ToString()));
                    dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value]
                        .Value = l.ToString();
                    var n = 0;
                    int.TryParse(
                        dataGridView_result.Rows[dataGridView_result.CurrentCell.RowIndex].Cells[ResultColumns.Value]
                            .Value.ToString(), out n);
                    var i = dataGridView_result.CurrentRow.Index;
                    for (var i1 = 0; i1 < 8; i1++)
                        dataGridView_result.Rows[i + 1 + i1].Cells[ResultColumns.Value].Value =
                            (Accessory.GetBit((byte)n, (byte)i1) ? (byte)1 : (byte)0).ToString();
                }
            }

            dataGridView_result.CellValueChanged += DataGridView_result_CellValueChanged;
        }

        private void DataGridView_commands_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            Button_newCommand_Click(this, EventArgs.Empty);
        }

        private void Button_add_Click(object sender, EventArgs e)
        {
            var tmpCmd = CollectCommand();
            commandList.Add(tmpCmd);
            listBox_code.Items.Add(commandMark[tmpCmd.Type] + Accessory.ConvertByteArrayToHex(tmpCmd.Data));
            listBox_code.SelectedIndex = listBox_code.Items.Count - 1;
        }

        private void Button_replace_Click(object sender, EventArgs e)
        {
            if (listBox_code.SelectedIndex == -1) return;
            var tmp = CollectCommand();
            commandList[listBox_code.SelectedIndex] = tmp;
            listBox_code.Items[listBox_code.SelectedIndex] = commandMark[tmp.Type] +
                                                             Accessory.ConvertByteArrayToHex(
                                                                 commandList[listBox_code.SelectedIndex].Data);
        }

        private void Button_insert_Click(object sender, EventArgs e)
        {
            if (listBox_code.SelectedIndex == -1) return;
            var tmp = CollectCommand();
            commandList.Insert(listBox_code.SelectedIndex, tmp);
            listBox_code.Items.Insert(listBox_code.SelectedIndex,
                commandMark[tmp.Type] + Accessory.ConvertByteArrayToHex(commandList[listBox_code.SelectedIndex].Data));
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
                for (var i = 0; i < dataGridView_commands.RowCount; i++)
                {
                    row = dataGridView_commands.Rows[i];
                    if (dataGridView_commands.Rows[i].Cells[ParseEscPos.CSVColumns.CommandName].Value.ToString() != "")
                    {
                        if (dataGridView_commands.Rows[i].Cells[ParseEscPos.CSVColumns.CommandDescription].Value
                            .ToString().ToLower().Contains(textBox_search.Text.ToLower()))
                        {
                            row.Visible = true;
                            i++;
                            while (i < dataGridView_commands.RowCount && dataGridView_commands.Rows[i]
                                .Cells[ParseEscPos.CSVColumns.CommandName].Value.ToString() == "")
                            {
                                row = dataGridView_commands.Rows[i];
                                row.Visible = true;
                                i++;
                            }

                            i--;
                        }
                        else
                        {
                            row.Visible = false;
                        }
                    }
                    else
                    {
                        row.Visible = false;
                    }
                }
            else
                for (var i = 0; i < dataGridView_commands.RowCount; i++)
                {
                    row = dataGridView_commands.Rows[i];
                    row.Visible = true;
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

            if (dataGridView_commands.Rows[dataGridView_commands.CurrentCell.RowIndex]
                .Cells[ParseEscPos.CSVColumns.CommandName].Value.ToString() != "")
            {
                var currentRow = dataGridView_commands.CurrentCell.RowIndex;
                _resultDatabase.Clear();
                var row = _resultDatabase.NewRow();
                row[ResultColumns.Value] = dataGridView_commands.Rows[currentRow]
                    .Cells[ParseEscPos.CSVColumns.CommandName].Value.ToString();
                row[ResultColumns.Raw] = row[ResultColumns.Value];
                row[ResultColumns.Description] = dataGridView_commands.Rows[currentRow]
                    .Cells[ParseEscPos.CSVColumns.CommandDescription].Value.ToString();
                _resultDatabase.Rows.Add(row);

                //collect parameters and fill with user data
                var i = currentRow + 1;
                while (i < dataGridView_commands.Rows.Count && dataGridView_commands.Rows[i]
                    .Cells[ParseEscPos.CSVColumns.CommandName].Value.ToString() == "")
                {
                    if (dataGridView_commands.Rows[i].Cells[ParseEscPos.CSVColumns.CommandParameterSize].Value
                        .ToString() != "")
                    {
                        row = _resultDatabase.NewRow();
                        row[ResultColumns.Type] = dataGridView_commands.Rows[i]
                            .Cells[ParseEscPos.CSVColumns.CommandParameterType].Value.ToString();
                        row[ResultColumns.Length] = dataGridView_commands.Rows[i]
                            .Cells[ParseEscPos.CSVColumns.CommandParameterSize].Value.ToString();
                        row[ResultColumns.Description] = dataGridView_commands.Rows[i]
                            .Cells[ParseEscPos.CSVColumns.CommandDescription].Value.ToString();
                        row[ResultColumns.Value] = "";
                        row[ResultColumns.Raw] = "";

                        _resultDatabase.Rows.Add(row);
                        if (row[ResultColumns.Type].ToString() == ParseEscPos.DataTypes.Bitfield) //decode bitfield
                            for (var i1 = 0; i1 < 8; i1++)
                            {
                                row = _resultDatabase.NewRow();
                                row[ResultColumns.Value] = "0";
                                row[ResultColumns.Type] = "bit" + i1;
                                row[ResultColumns.Description] = dataGridView_commands.Rows[i + i1 + 1]
                                    .Cells[ParseEscPos.CSVColumns.CommandDescription].Value.ToString();
                                _resultDatabase.Rows.Add(row);
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
            if (e.Control && e.KeyCode == Keys.C && listBox_code.SelectedItem.ToString() != "")
            {
                Clipboard.SetText(listBox_code.SelectedItem.ToString());
            }
            //Ctrl-Ins - copy string to clipboard
            else if (e.Control && e.KeyCode == Keys.Insert && listBox_code.SelectedItem.ToString() != "")
            {
                Clipboard.SetText(listBox_code.SelectedItem.ToString());
            }
            //Ctrl-V - insert string from clipboard
            //Shift-Ins - insert string from clipboard
            else if (e.Control && e.KeyCode == Keys.V && Accessory.GetStringFormat(Clipboard.GetText()) == 16 ||
                     e.Shift && e.KeyCode == Keys.Insert && Accessory.GetStringFormat(Clipboard.GetText()) == 16)
            {
                var tmp = new Command();
                tmp.Data = Accessory.ConvertHexToByteArray(Accessory.CheckHexString(Clipboard.GetText()));
                //if command
                if (tmp.Data[2] == _hostAddress && tmp.Data[1] == tmp.Data.Length - 3) tmp.Type = commandType.Command;
                // if reply to host
                else if (tmp.Data[0] == _hostAddress && tmp.Data[1] == tmp.Data.Length - 3) tmp.Type = commandType.Reply;
                //if unknown
                else tmp.Type = commandType.Unrecognized;
                commandList.Add(tmp);

                listBox_code.Items[listBox_code.SelectedIndex] =
                    commandMark[tmp.Type] + Accessory.ConvertByteArrayToHex(tmp.Data);
            }
            //DEL - delete string
            else if (e.KeyCode == Keys.Delete)
            {
                var i = listBox_code.SelectedIndex;
                listBox_code.Items.RemoveAt(listBox_code.SelectedIndex);
                if (i >= listBox_code.Items.Count) i = listBox_code.Items.Count - 1;
                listBox_code.SelectedIndex = i;
            }
            //Ctrl-P - parse string
            else if (e.Control && e.KeyCode == Keys.P)
            {
                Button_find_Click(this, EventArgs.Empty);
            }
            //Ctrl-S - send string to device
            else if (e.Control && e.KeyCode == Keys.S && button_Send.Enabled)
            {
                Button_Send_Click(this, EventArgs.Empty);
            }
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
                    SerialPort1.Handshake =
                        (Handshake)Enum.Parse(typeof(Handshake), toolStripComboBox_PortHandshake.Text);
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
                commandList[listBox_code.SelectedIndex].Data.Length < MinFrameLength)
            {
                _flag = true;
                return;
            }

            var txBytes =
                Accessory.ConvertHexToByteArray(Accessory.CheckHexString(listBox_code.SelectedItem.ToString()));

            //if trying to send reply - get back to command and send it. Not in case it's a "Send All"
            if (sender != button_SendAll && txBytes.Length >= MinFrameLength && txBytes[0] == _hostAddress &&
                txBytes[2] == _deviceAddress && listBox_code.SelectedIndex > 0)
            {
                listBox_code.SelectedIndex--;
                Button_Send_Click(this, EventArgs.Empty);
                return;
            }

            if (txBytes.Length >= MinFrameLength && (txBytes[0] == _deviceAddress || txBytes[0] == 0) &&
                txBytes[2] == _hostAddress)
            {
                var tmpCmd = commandList[listBox_code.SelectedIndex];
                tmpCmd.Type = commandType.Command;
                commandList[listBox_code.SelectedIndex] = tmpCmd;
                listBox_code.Items[listBox_code.SelectedIndex] =
                    commandMark[tmpCmd.Type] + Accessory.ConvertByteArrayToHex(tmpCmd.Data);
                try
                {
                    SerialPort1.DiscardInBuffer();
                    SerialPort1.Write(txBytes, 0, txBytes.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error sending to port " + SerialPort1.PortName + ": " + ex.Message);
                }

                var rxBytes = new List<byte>();
                //copy of request deleted from receive buffer
                var dupeDeleted = false;
                //source/destination address of reply is incorrect
                var notReply = false;
                //broadcast message received
                var broadcastReply = false;
                //ACK received
                var ack = false;
                //NACK received
                var nack = false;
                //CRC doesn't match 
                var crcError = false;
                //complete frame received 
                var frameOk = false;
                //ended with timeout
                var timeout = false;
                byte frameLength = 0;
                var startTime = DateTime.UtcNow;
                try
                {
                    while (!timeout && !frameOk)
                    {
                        if (!dupeDeleted)
                        {
                            if (SerialPort1.BytesToRead != 0) rxBytes.Add((byte)SerialPort1.ReadByte());
                            if (Accessory.ByteArrayCompare(rxBytes.ToArray(), txBytes))
                            {
                                dupeDeleted = true;
                                rxBytes.Clear();
                            }
                        }

                        if (dupeDeleted)
                        {
                            while (SerialPort1.BytesToRead > 0 && rxBytes.Count < 4)
                                rxBytes.Add((byte)SerialPort1.ReadByte());
                            if (rxBytes.Count >= 4 && (rxBytes[0] == _hostAddress && rxBytes[2] == _deviceAddress ||
                                                        rxBytes[0] == _deviceAddress && rxBytes[2] == _hostAddress))
                            {
                                if (rxBytes[0] == 0) broadcastReply = true;
                                else if (rxBytes[0] != _hostAddress || rxBytes[2] != _deviceAddress) notReply = true;
                                frameLength = rxBytes[1];
                                //ACK
                                if (frameLength == 0 && rxBytes[3] == ParseEscPos.ackSign) ack = true;
                                //NACK
                                else if (frameLength == 0 && rxBytes[3] == ParseEscPos.nackSign) nack = true;
                                //BUSY
                                else if (frameLength == 0 && rxBytes[3] == ParseEscPos.busySign) nack = true;
                                //normal reply
                                while (SerialPort1.BytesToRead > 0 && rxBytes.Count < frameLength + MinFrameLength)
                                    rxBytes.Add((byte)SerialPort1.ReadByte());
                                if (rxBytes.Count >= frameLength + MinFrameLength)
                                {
                                    frameOk = true;
                                    var crc = new byte[2];
                                    crc = ParseEscPos.GetCRC(
                                        rxBytes.GetRange(0, frameLength + MinFrameLength).ToArray(),
                                        rxBytes.Count - 1);
                                    if (toolStripComboBox_CrcType.SelectedIndex == ParseEscPos.CrcTypes.SimpleCRC)
                                    {
                                        if (crc[0] != rxBytes[rxBytes.Count - 1]) crcError = true;
                                    }
                                    else if (toolStripComboBox_CrcType.SelectedIndex == ParseEscPos.CrcTypes.CRC16)
                                    {
                                        if (crc[1] != rxBytes[rxBytes.Count - 1] || crc[0] != rxBytes[2])
                                            crcError = true;
                                    }
                                }
                            }
                            else if (SerialPort1.BytesToRead > 0)
                            {
                                rxBytes.Add((byte)SerialPort1.ReadByte());
                            }
                        }

                        if (DateTime.UtcNow.Subtract(startTime).TotalMilliseconds > _serialTimeOut) timeout = true;
                        //if (SerialPort1.BytesToRead <= 0 && DateTime.UtcNow.Subtract(startTime).TotalMilliseconds > SerialtimeOut) _timeout = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading port " + SerialPort1.PortName + ": " + ex.Message);
                }

                if (frameOk || showIncorrectRepliesToolStripMenuItem.Checked)
                {
                    var tmp = new Command { Type = commandType.Reply };
                    if (!frameOk || notReply || crcError) tmp.Type = commandType.Unrecognized;
                    tmp.Data = rxBytes.ToArray();
                    //if command line is the last - add reply to the bottom of the list
                    if (listBox_code.SelectedIndex + 1 >= listBox_code.Items.Count)
                    {
                        commandList.Add(tmp);
                        listBox_code.Items.Add(commandMark[tmp.Type] + Accessory.ConvertByteArrayToHex(tmp.Data));
                    }
                    //if next item in the list is reply (supposed to be previous reply) - replace it with new reply
                    else if (commandList[listBox_code.SelectedIndex + 1].Data.Length >= 3 &&
                             commandList[listBox_code.SelectedIndex + 1].Data[0] == _hostAddress &&
                             commandList[listBox_code.SelectedIndex + 1].Data[2] == _deviceAddress)
                    {
                        commandList[listBox_code.SelectedIndex + 1] = tmp;
                        listBox_code.Items[listBox_code.SelectedIndex + 1] =
                            commandMark[tmp.Type] + Accessory.ConvertByteArrayToHex(tmp.Data);
                    }
                    //if next item in the list is empty (supposed to be previous incorrect reply) - replace it with new reply
                    else if (commandList[listBox_code.SelectedIndex + 1].Data.Length == 0)
                    {
                        commandList[listBox_code.SelectedIndex + 1] = tmp;
                        listBox_code.Items[listBox_code.SelectedIndex + 1] =
                            commandMark[tmp.Type] + Accessory.ConvertByteArrayToHex(tmp.Data);
                    }
                    //or insert new reply next line to command
                    else
                    {
                        commandList.Insert(listBox_code.SelectedIndex + 1, tmp);
                        listBox_code.Items.Insert(listBox_code.SelectedIndex + 1,
                            commandMark[tmp.Type] + Accessory.ConvertByteArrayToHex(tmp.Data));
                    }

                    if (autoParseReplyToolStripMenuItem.Checked) Button_next_Click(this, EventArgs.Empty);
                }
            }
            else
            {
                _flag = true;
                MessageBox.Show("Incorrect host/device address or data incomplete");
            }
        }

        private void Button_SendAll_Click(object sender, EventArgs e)
        {
            _flag = false;
            if (listBox_code.SelectedIndex < 0) listBox_code.SelectedIndex = 0;
            for (var i = listBox_code.SelectedIndex; i < listBox_code.Items.Count; i++)
            {
                listBox_code.SelectedIndex = i;
                if (commandList[i].Data.Length >= MinFrameLength) //check minimum packet length
                {
                    Button_Send_Click(button_SendAll, EventArgs.Empty);
                    if (_flag) break;
                }
            }

            _flag = false;
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
            foreach (var s in SerialPort.GetPortNames()) toolStripComboBox_PortName.Items.Add(s);
            //Add handshake methods
            foreach (var s in Enum.GetNames(typeof(Handshake))) toolStripComboBox_PortHandshake.Items.Add(s);
            //Add parity
            foreach (var s in Enum.GetNames(typeof(Parity))) toolStripComboBox_PortParity.Items.Add(s);
            //Add stopbits
            foreach (var s in Enum.GetNames(typeof(StopBits))) toolStripComboBox_PortStopBits.Items.Add(s);
            toolStripComboBox_PortName.SelectedIndex = toolStripComboBox_PortName.Items.Count - 1;
            if (toolStripComboBox_PortName.Items.Count == 1) toolStripMenuItem_Connect.Enabled = false;
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
            if (!int.TryParse(toolStripTextBox_TimeOut.Text, out _serialTimeOut)) toolStripTextBox_TimeOut.Text = "3000";
        }

        private void ListBox_code_MouseUp(object sender, MouseEventArgs e)
        {
            var index = listBox_code.IndexFromPoint(e.Location);
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
                listBox_code.Items[listBox_code.SelectedIndex] = Accessory.CheckHexString(Clipboard.GetText());
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
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
                dataGridView_commands.CurrentCell = dataGridView_commands.Rows[e.RowIndex].Cells[e.ColumnIndex];
        }

        private void COMPortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (toolStripComboBox_PortName.Enabled) SerialPopulate();
        }

        private void ShowIncorrectRepliesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showIncorrectRepliesToolStripMenuItem.Checked = !showIncorrectRepliesToolStripMenuItem.Checked;
        }

        private void AutoParseReplyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoParseReplyToolStripMenuItem.Checked = !autoParseReplyToolStripMenuItem.Checked;
        }

        private void Button_removeReplies_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < commandList.Count; i++)
                if (commandList[i].Type == commandType.Reply)
                {
                    commandList.RemoveAt(i);
                    listBox_code.Items.RemoveAt(i);
                    i--;
                }
        }

        private void ToolStripComboBox_CrcType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ParseEscPos.CrcType = (byte)toolStripComboBox_CrcType.SelectedIndex;
        }

        private void TextBox_deviceAddress_Leave(object sender, EventArgs e)
        {
            _deviceAddress = 0;
            byte.TryParse(textBox_deviceAddress.Text, out _deviceAddress);
        }

        private void TextBox_hostAddress_Leave(object sender, EventArgs e)
        {
            _hostAddress = 0;
            byte.TryParse(textBox_hostAddress.Text, out _hostAddress);
        }

        #endregion
    }
}