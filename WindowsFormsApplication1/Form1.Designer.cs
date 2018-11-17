namespace WindowsFormsApplication1
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView_commands = new System.Windows.Forms.DataGridView();
            this.button_next = new System.Windows.Forms.Button();
            this.button_find = new System.Windows.Forms.Button();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadHexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveHexFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LoadCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commandsCSVFileName_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commandsCSV_ToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.enableDatabaseEditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showIncorrectRepliesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoParseReplyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.COMPortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripComboBox_PortName = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBox_PortSpeed = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBox_PortHandshake = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBox_PortDataBits = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBox_PortParity = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBox_PortStopBits = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripMenuItem_TimeOut = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripTextBox_TimeOut = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem_Connect = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button_SendAll = new System.Windows.Forms.Button();
            this.button_Send = new System.Windows.Forms.Button();
            this.listBox_code = new System.Windows.Forms.ListBox();
            this.button_clear = new System.Windows.Forms.Button();
            this.button_removeReplies = new System.Windows.Forms.Button();
            this.textBox_search = new System.Windows.Forms.TextBox();
            this.textBox_deviceAddress = new System.Windows.Forms.TextBox();
            this.textBox_hostAddress = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridView_result = new System.Windows.Forms.DataGridView();
            this.button_replace = new System.Windows.Forms.Button();
            this.button_newCommand = new System.Windows.Forms.Button();
            this.button_add = new System.Windows.Forms.Button();
            this.button_insert = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.SerialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.contextMenuStrip_code = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_dataBase = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newCommandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findThisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripComboBox_CrcType = new System.Windows.Forms.ToolStripComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_commands)).BeginInit();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_result)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.contextMenuStrip_code.SuspendLayout();
            this.contextMenuStrip_dataBase.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView_commands
            // 
            this.dataGridView_commands.AllowUserToAddRows = false;
            this.dataGridView_commands.AllowUserToDeleteRows = false;
            this.dataGridView_commands.AllowUserToOrderColumns = true;
            this.dataGridView_commands.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView_commands.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dataGridView_commands.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridView_commands.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_commands.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView_commands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_commands.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridView_commands.Location = new System.Drawing.Point(0, 0);
            this.dataGridView_commands.MultiSelect = false;
            this.dataGridView_commands.Name = "dataGridView_commands";
            this.dataGridView_commands.ReadOnly = true;
            this.dataGridView_commands.RowHeadersVisible = false;
            this.dataGridView_commands.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_commands.Size = new System.Drawing.Size(788, 230);
            this.dataGridView_commands.TabIndex = 2;
            this.dataGridView_commands.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_commands_CellDoubleClick);
            this.dataGridView_commands.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridView_commands_CellMouseUp);
            // 
            // button_next
            // 
            this.button_next.Location = new System.Drawing.Point(2, 30);
            this.button_next.Margin = new System.Windows.Forms.Padding(2);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(80, 24);
            this.button_next.TabIndex = 11;
            this.button_next.Text = "Parse next ->";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.Button_next_Click);
            // 
            // button_find
            // 
            this.button_find.Location = new System.Drawing.Point(2, 2);
            this.button_find.Margin = new System.Windows.Forms.Padding(2);
            this.button_find.Name = "button_find";
            this.button_find.Size = new System.Drawing.Size(80, 24);
            this.button_find.TabIndex = 12;
            this.button_find.Text = "Parse ->";
            this.button_find.UseVisualStyleBackColor = true;
            this.button_find.Click += new System.EventHandler(this.Button_find_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.COMPortToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(792, 24);
            this.menuStrip.TabIndex = 25;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadHexToolStripMenuItem,
            this.saveHexFileToolStripMenuItem,
            this.LoadCSVToolStripMenuItem,
            this.saveCSVToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadHexToolStripMenuItem
            // 
            this.loadHexToolStripMenuItem.Name = "loadHexToolStripMenuItem";
            this.loadHexToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.loadHexToolStripMenuItem.Text = "Load HEX file";
            this.loadHexToolStripMenuItem.Click += new System.EventHandler(this.LoadHexToolStripMenuItem_Click);
            // 
            // saveHexFileToolStripMenuItem
            // 
            this.saveHexFileToolStripMenuItem.Name = "saveHexFileToolStripMenuItem";
            this.saveHexFileToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.saveHexFileToolStripMenuItem.Text = "Save HEX file";
            this.saveHexFileToolStripMenuItem.Click += new System.EventHandler(this.SaveHexFileToolStripMenuItem_Click);
            // 
            // LoadCSVToolStripMenuItem
            // 
            this.LoadCSVToolStripMenuItem.Name = "LoadCSVToolStripMenuItem";
            this.LoadCSVToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.LoadCSVToolStripMenuItem.Text = "Load commands database";
            this.LoadCSVToolStripMenuItem.Click += new System.EventHandler(this.LoadCSVToolStripMenuItem_Click);
            // 
            // saveCSVToolStripMenuItem
            // 
            this.saveCSVToolStripMenuItem.Name = "saveCSVToolStripMenuItem";
            this.saveCSVToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.saveCSVToolStripMenuItem.Text = "Save commands database";
            this.saveCSVToolStripMenuItem.Click += new System.EventHandler(this.SaveCSVToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commandsCSVFileName_ToolStripMenuItem,
            this.enableDatabaseEditToolStripMenuItem,
            this.showIncorrectRepliesToolStripMenuItem,
            this.autoParseReplyToolStripMenuItem});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.aboutToolStripMenuItem.Text = "Settings";
            // 
            // commandsCSVFileName_ToolStripMenuItem
            // 
            this.commandsCSVFileName_ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commandsCSV_ToolStripTextBox});
            this.commandsCSVFileName_ToolStripMenuItem.Name = "commandsCSVFileName_ToolStripMenuItem";
            this.commandsCSVFileName_ToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.commandsCSVFileName_ToolStripMenuItem.Text = "Default commands CSV";
            // 
            // commandsCSV_ToolStripTextBox
            // 
            this.commandsCSV_ToolStripTextBox.Name = "commandsCSV_ToolStripTextBox";
            this.commandsCSV_ToolStripTextBox.Size = new System.Drawing.Size(100, 23);
            this.commandsCSV_ToolStripTextBox.Text = "q3f_commands.csv";
            this.commandsCSV_ToolStripTextBox.Leave += new System.EventHandler(this.DefaultCSVToolStripTextBox_Leave);
            // 
            // enableDatabaseEditToolStripMenuItem
            // 
            this.enableDatabaseEditToolStripMenuItem.Name = "enableDatabaseEditToolStripMenuItem";
            this.enableDatabaseEditToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.enableDatabaseEditToolStripMenuItem.Text = "Enable database edit";
            this.enableDatabaseEditToolStripMenuItem.Click += new System.EventHandler(this.EnableDatabaseEditToolStripMenuItem_Click);
            // 
            // showIncorrectRepliesToolStripMenuItem
            // 
            this.showIncorrectRepliesToolStripMenuItem.Checked = true;
            this.showIncorrectRepliesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showIncorrectRepliesToolStripMenuItem.Name = "showIncorrectRepliesToolStripMenuItem";
            this.showIncorrectRepliesToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.showIncorrectRepliesToolStripMenuItem.Text = "Show incorrect replies";
            this.showIncorrectRepliesToolStripMenuItem.Click += new System.EventHandler(this.showIncorrectRepliesToolStripMenuItem_Click);
            // 
            // autoParseReplyToolStripMenuItem
            // 
            this.autoParseReplyToolStripMenuItem.Checked = true;
            this.autoParseReplyToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoParseReplyToolStripMenuItem.Name = "autoParseReplyToolStripMenuItem";
            this.autoParseReplyToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.autoParseReplyToolStripMenuItem.Text = "Auto parse reply";
            this.autoParseReplyToolStripMenuItem.Click += new System.EventHandler(this.autoParseReplyToolStripMenuItem_Click);
            // 
            // COMPortToolStripMenuItem
            // 
            this.COMPortToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBox_PortName,
            this.toolStripComboBox_PortSpeed,
            this.toolStripComboBox_PortHandshake,
            this.toolStripComboBox_PortDataBits,
            this.toolStripComboBox_PortParity,
            this.toolStripComboBox_PortStopBits,
            this.toolStripComboBox_CrcType,
            this.toolStripMenuItem_TimeOut,
            this.toolStripMenuItem_Connect});
            this.COMPortToolStripMenuItem.Name = "COMPortToolStripMenuItem";
            this.COMPortToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.COMPortToolStripMenuItem.Text = "Connection";
            this.COMPortToolStripMenuItem.Click += new System.EventHandler(this.COMPortToolStripMenuItem_Click);
            // 
            // toolStripComboBox_PortName
            // 
            this.toolStripComboBox_PortName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox_PortName.Name = "toolStripComboBox_PortName";
            this.toolStripComboBox_PortName.Size = new System.Drawing.Size(121, 23);
            // 
            // toolStripComboBox_PortSpeed
            // 
            this.toolStripComboBox_PortSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox_PortSpeed.Items.AddRange(new object[] {
            "250000",
            "230400",
            "115200",
            "57600",
            "38400",
            "19200",
            "9600",
            "4800",
            "2400",
            "1200",
            "600",
            "300"});
            this.toolStripComboBox_PortSpeed.Name = "toolStripComboBox_PortSpeed";
            this.toolStripComboBox_PortSpeed.Size = new System.Drawing.Size(121, 23);
            // 
            // toolStripComboBox_PortHandshake
            // 
            this.toolStripComboBox_PortHandshake.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox_PortHandshake.Name = "toolStripComboBox_PortHandshake";
            this.toolStripComboBox_PortHandshake.Size = new System.Drawing.Size(121, 23);
            // 
            // toolStripComboBox_PortDataBits
            // 
            this.toolStripComboBox_PortDataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox_PortDataBits.Items.AddRange(new object[] {
            "8",
            "7",
            "6",
            "5"});
            this.toolStripComboBox_PortDataBits.Name = "toolStripComboBox_PortDataBits";
            this.toolStripComboBox_PortDataBits.Size = new System.Drawing.Size(121, 23);
            // 
            // toolStripComboBox_PortParity
            // 
            this.toolStripComboBox_PortParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox_PortParity.Name = "toolStripComboBox_PortParity";
            this.toolStripComboBox_PortParity.Size = new System.Drawing.Size(121, 23);
            // 
            // toolStripComboBox_PortStopBits
            // 
            this.toolStripComboBox_PortStopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox_PortStopBits.Name = "toolStripComboBox_PortStopBits";
            this.toolStripComboBox_PortStopBits.Size = new System.Drawing.Size(121, 23);
            // 
            // toolStripMenuItem_TimeOut
            // 
            this.toolStripMenuItem_TimeOut.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBox_TimeOut});
            this.toolStripMenuItem_TimeOut.Name = "toolStripMenuItem_TimeOut";
            this.toolStripMenuItem_TimeOut.Size = new System.Drawing.Size(181, 22);
            this.toolStripMenuItem_TimeOut.Text = "TimeOut";
            // 
            // toolStripTextBox_TimeOut
            // 
            this.toolStripTextBox_TimeOut.MaxLength = 5;
            this.toolStripTextBox_TimeOut.Name = "toolStripTextBox_TimeOut";
            this.toolStripTextBox_TimeOut.Size = new System.Drawing.Size(100, 23);
            this.toolStripTextBox_TimeOut.TextChanged += new System.EventHandler(this.ToolStripTextBox_TimeOut_TextChanged);
            // 
            // toolStripMenuItem_Connect
            // 
            this.toolStripMenuItem_Connect.Name = "toolStripMenuItem_Connect";
            this.toolStripMenuItem_Connect.Size = new System.Drawing.Size(181, 22);
            this.toolStripMenuItem_Connect.Text = "Connect";
            this.toolStripMenuItem_Connect.Click += new System.EventHandler(this.ToolStripMenuItem_Connect_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialog_FileOk);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveFileDialog_FileOk);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.button_SendAll);
            this.splitContainer1.Panel1.Controls.Add(this.button_Send);
            this.splitContainer1.Panel1.Controls.Add(this.listBox_code);
            this.splitContainer1.Panel1.Controls.Add(this.button_clear);
            this.splitContainer1.Panel1.Controls.Add(this.button_removeReplies);
            this.splitContainer1.Panel1MinSize = 150;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBox_search);
            this.splitContainer1.Panel2.Controls.Add(this.textBox_deviceAddress);
            this.splitContainer1.Panel2.Controls.Add(this.textBox_hostAddress);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.dataGridView_result);
            this.splitContainer1.Panel2.Controls.Add(this.button_replace);
            this.splitContainer1.Panel2.Controls.Add(this.button_newCommand);
            this.splitContainer1.Panel2.Controls.Add(this.button_add);
            this.splitContainer1.Panel2.Controls.Add(this.button_insert);
            this.splitContainer1.Panel2.Controls.Add(this.button_next);
            this.splitContainer1.Panel2.Controls.Add(this.button_find);
            this.splitContainer1.Panel2MinSize = 150;
            this.splitContainer1.Size = new System.Drawing.Size(792, 300);
            this.splitContainer1.SplitterDistance = 155;
            this.splitContainer1.TabIndex = 26;
            // 
            // button_SendAll
            // 
            this.button_SendAll.Enabled = false;
            this.button_SendAll.Location = new System.Drawing.Point(77, 4);
            this.button_SendAll.Name = "button_SendAll";
            this.button_SendAll.Size = new System.Drawing.Size(69, 23);
            this.button_SendAll.TabIndex = 12;
            this.button_SendAll.Text = "Send all";
            this.button_SendAll.UseVisualStyleBackColor = true;
            this.button_SendAll.Click += new System.EventHandler(this.Button_SendAll_Click);
            // 
            // button_Send
            // 
            this.button_Send.Enabled = false;
            this.button_Send.Location = new System.Drawing.Point(4, 4);
            this.button_Send.Name = "button_Send";
            this.button_Send.Size = new System.Drawing.Size(69, 23);
            this.button_Send.TabIndex = 12;
            this.button_Send.Text = "Send";
            this.button_Send.UseVisualStyleBackColor = true;
            this.button_Send.Click += new System.EventHandler(this.Button_Send_Click);
            // 
            // listBox_code
            // 
            this.listBox_code.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox_code.FormattingEnabled = true;
            this.listBox_code.HorizontalScrollbar = true;
            this.listBox_code.Location = new System.Drawing.Point(3, 33);
            this.listBox_code.Name = "listBox_code";
            this.listBox_code.Size = new System.Drawing.Size(145, 225);
            this.listBox_code.TabIndex = 0;
            this.listBox_code.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListBox_code_KeyDown);
            this.listBox_code.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListBox_code_MouseDoubleClick);
            this.listBox_code.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListBox_code_MouseUp);
            // 
            // button_clear
            // 
            this.button_clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_clear.Location = new System.Drawing.Point(99, 269);
            this.button_clear.Margin = new System.Windows.Forms.Padding(2);
            this.button_clear.Name = "button_clear";
            this.button_clear.Size = new System.Drawing.Size(47, 24);
            this.button_clear.TabIndex = 11;
            this.button_clear.Text = "Clear";
            this.button_clear.UseVisualStyleBackColor = true;
            this.button_clear.Click += new System.EventHandler(this.Button_clear_Click);
            // 
            // button_removeReplies
            // 
            this.button_removeReplies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_removeReplies.Location = new System.Drawing.Point(4, 270);
            this.button_removeReplies.Margin = new System.Windows.Forms.Padding(2);
            this.button_removeReplies.Name = "button_removeReplies";
            this.button_removeReplies.Size = new System.Drawing.Size(91, 24);
            this.button_removeReplies.TabIndex = 11;
            this.button_removeReplies.Text = "Remove replies";
            this.button_removeReplies.UseVisualStyleBackColor = true;
            this.button_removeReplies.Click += new System.EventHandler(this.Button_removeReplies_Click);
            // 
            // textBox_search
            // 
            this.textBox_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_search.Location = new System.Drawing.Point(2, 243);
            this.textBox_search.MaxLength = 9;
            this.textBox_search.Name = "textBox_search";
            this.textBox_search.Size = new System.Drawing.Size(80, 20);
            this.textBox_search.TabIndex = 14;
            this.textBox_search.TextChanged += new System.EventHandler(this.TextBox_search_TextChanged);
            // 
            // textBox_deviceAddress
            // 
            this.textBox_deviceAddress.Location = new System.Drawing.Point(2, 195);
            this.textBox_deviceAddress.MaxLength = 9;
            this.textBox_deviceAddress.Name = "textBox_deviceAddress";
            this.textBox_deviceAddress.Size = new System.Drawing.Size(80, 20);
            this.textBox_deviceAddress.TabIndex = 14;
            this.textBox_deviceAddress.Text = "0";
            // 
            // textBox_hostAddress
            // 
            this.textBox_hostAddress.Location = new System.Drawing.Point(2, 156);
            this.textBox_hostAddress.MaxLength = 9;
            this.textBox_hostAddress.Name = "textBox_hostAddress";
            this.textBox_hostAddress.ReadOnly = true;
            this.textBox_hostAddress.Size = new System.Drawing.Size(80, 20);
            this.textBox_hostAddress.TabIndex = 14;
            this.textBox_hostAddress.Text = "1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 179);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Device address";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 227);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Search";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 140);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Host address";
            // 
            // dataGridView_result
            // 
            this.dataGridView_result.AllowUserToAddRows = false;
            this.dataGridView_result.AllowUserToDeleteRows = false;
            this.dataGridView_result.AllowUserToOrderColumns = true;
            this.dataGridView_result.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_result.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView_result.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dataGridView_result.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridView_result.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView_result.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridView_result.Location = new System.Drawing.Point(88, 2);
            this.dataGridView_result.Name = "dataGridView_result";
            this.dataGridView_result.RowHeadersVisible = false;
            this.dataGridView_result.Size = new System.Drawing.Size(538, 291);
            this.dataGridView_result.TabIndex = 3;
            this.dataGridView_result.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_result_CellValueChanged);
            // 
            // button_replace
            // 
            this.button_replace.Location = new System.Drawing.Point(2, 86);
            this.button_replace.Margin = new System.Windows.Forms.Padding(2);
            this.button_replace.Name = "button_replace";
            this.button_replace.Size = new System.Drawing.Size(80, 24);
            this.button_replace.TabIndex = 11;
            this.button_replace.Text = "<- Replace";
            this.button_replace.UseVisualStyleBackColor = true;
            this.button_replace.Click += new System.EventHandler(this.Button_replace_Click);
            // 
            // button_newCommand
            // 
            this.button_newCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_newCommand.Location = new System.Drawing.Point(2, 268);
            this.button_newCommand.Margin = new System.Windows.Forms.Padding(2);
            this.button_newCommand.Name = "button_newCommand";
            this.button_newCommand.Size = new System.Drawing.Size(80, 24);
            this.button_newCommand.TabIndex = 11;
            this.button_newCommand.Text = "New";
            this.button_newCommand.UseVisualStyleBackColor = true;
            this.button_newCommand.Click += new System.EventHandler(this.Button_newCommand_Click);
            // 
            // button_add
            // 
            this.button_add.Location = new System.Drawing.Point(2, 114);
            this.button_add.Margin = new System.Windows.Forms.Padding(2);
            this.button_add.Name = "button_add";
            this.button_add.Size = new System.Drawing.Size(80, 24);
            this.button_add.TabIndex = 11;
            this.button_add.Text = "<- Add";
            this.button_add.UseVisualStyleBackColor = true;
            this.button_add.Click += new System.EventHandler(this.Button_add_Click);
            // 
            // button_insert
            // 
            this.button_insert.Location = new System.Drawing.Point(2, 58);
            this.button_insert.Margin = new System.Windows.Forms.Padding(2);
            this.button_insert.Name = "button_insert";
            this.button_insert.Size = new System.Drawing.Size(80, 24);
            this.button_insert.TabIndex = 11;
            this.button_insert.Text = "<- Insert";
            this.button_insert.UseVisualStyleBackColor = true;
            this.button_insert.Click += new System.EventHandler(this.Button_insert_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 24);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
            this.splitContainer2.Panel1MinSize = 300;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dataGridView_commands);
            this.splitContainer2.Panel2MinSize = 50;
            this.splitContainer2.Size = new System.Drawing.Size(792, 538);
            this.splitContainer2.SplitterDistance = 300;
            this.splitContainer2.TabIndex = 27;
            // 
            // contextMenuStrip_code
            // 
            this.contextMenuStrip_code.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.parseToolStripMenuItem,
            this.sendToolStripMenuItem});
            this.contextMenuStrip_code.Name = "contextMenuStrip_code";
            this.contextMenuStrip_code.Size = new System.Drawing.Size(146, 114);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.copyToolStripMenuItem.Text = "Copy (Ctrl-C)";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.CopyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.pasteToolStripMenuItem.Text = "Paste (Ctrl-V)";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.PasteToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.deleteToolStripMenuItem.Text = "Delete (Del)";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // parseToolStripMenuItem
            // 
            this.parseToolStripMenuItem.Name = "parseToolStripMenuItem";
            this.parseToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.parseToolStripMenuItem.Text = "Parse (Ctrl-P)";
            this.parseToolStripMenuItem.Click += new System.EventHandler(this.ParseToolStripMenuItem_Click);
            // 
            // sendToolStripMenuItem
            // 
            this.sendToolStripMenuItem.Enabled = false;
            this.sendToolStripMenuItem.Name = "sendToolStripMenuItem";
            this.sendToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.sendToolStripMenuItem.Text = "Send (Ctrl-S)";
            this.sendToolStripMenuItem.Click += new System.EventHandler(this.SendToolStripMenuItem_Click);
            // 
            // contextMenuStrip_dataBase
            // 
            this.contextMenuStrip_dataBase.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newCommandToolStripMenuItem,
            this.findThisToolStripMenuItem});
            this.contextMenuStrip_dataBase.Name = "contextMenuStrip_dataBase";
            this.contextMenuStrip_dataBase.Size = new System.Drawing.Size(157, 48);
            // 
            // newCommandToolStripMenuItem
            // 
            this.newCommandToolStripMenuItem.Name = "newCommandToolStripMenuItem";
            this.newCommandToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.newCommandToolStripMenuItem.Text = "New command";
            this.newCommandToolStripMenuItem.Click += new System.EventHandler(this.NewCommandToolStripMenuItem_Click);
            // 
            // findThisToolStripMenuItem
            // 
            this.findThisToolStripMenuItem.Name = "findThisToolStripMenuItem";
            this.findThisToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.findThisToolStripMenuItem.Text = "Find this";
            this.findThisToolStripMenuItem.Click += new System.EventHandler(this.FindThisToolStripMenuItem_Click);
            // 
            // toolStripComboBox_CrcType
            // 
            this.toolStripComboBox_CrcType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox_CrcType.Items.AddRange(new object[] {
            "SimpleCRC",
            "CRC8",
            "CRC16"});
            this.toolStripComboBox_CrcType.Name = "toolStripComboBox_CrcType";
            this.toolStripComboBox_CrcType.Size = new System.Drawing.Size(121, 23);
            this.toolStripComboBox_CrcType.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox_CrcType_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 562);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.menuStrip);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "Form1";
            this.Text = "CCTalkControl";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_commands)).EndInit();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_result)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.contextMenuStrip_code.ResumeLayout(false);
            this.contextMenuStrip_dataBase.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView dataGridView_commands;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.Button button_find;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadHexToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveHexFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem LoadCSVToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveCSVToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem commandsCSVFileName_ToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox commandsCSV_ToolStripTextBox;
        private System.Windows.Forms.ToolStripMenuItem enableDatabaseEditToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dataGridView_result;
        private System.Windows.Forms.ListBox listBox_code;
        private System.Windows.Forms.Button button_add;
        private System.Windows.Forms.Button button_insert;
        private System.Windows.Forms.Button button_replace;
        private System.Windows.Forms.Button button_removeReplies;
        private System.Windows.Forms.TextBox textBox_hostAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_clear;
        private System.Windows.Forms.TextBox textBox_search;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_newCommand;
        private System.Windows.Forms.ToolStripMenuItem COMPortToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_PortName;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_PortSpeed;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_PortHandshake;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_PortDataBits;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_PortParity;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_PortStopBits;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Connect;
        private System.Windows.Forms.Button button_Send;
        private System.IO.Ports.SerialPort SerialPort1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_TimeOut;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox_TimeOut;
        private System.Windows.Forms.Button button_SendAll;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_code;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem parseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_dataBase;
        private System.Windows.Forms.ToolStripMenuItem newCommandToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findThisToolStripMenuItem;
        private System.Windows.Forms.TextBox textBox_deviceAddress;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolStripMenuItem showIncorrectRepliesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoParseReplyToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_CrcType;
    }
}

