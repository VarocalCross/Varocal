namespace VarocalCross
{
    using System.Windows.Forms;
	partial class PrefWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		public System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && ( components != null ) )
			{
				components.Dispose( );
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		public void InitializeComponent( )
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrefWindow));
			this.button1 = new VarocalCross.FATabStrip();
			this.faTabStripItem1 = new VarocalCross.FATabStripItem();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.update_download_startup = new RadioButton();
			this.update_download_ask = new RadioButton();
			this.update_reminds = new RadioButton();
			this.update_none = new RadioButton();
			this.label1 = new Label();
			this.recently_number = new TextBox();
			this.backup_number = new TextBox();
			this.last_startup = new CheckBox();
			this.backup = new CheckBox();
			this.faTabStripItem2 = new VarocalCross.FATabStripItem();
			this.text_italic = new CheckBox();
			this.label2 = new Label();
			this.code_colors = new System.Windows.Forms.ListBox();
			this.tab_spaces = new TextBox();
			this.text_bold = new CheckBox();
			this.label3 = new Label();
			this.rect_change = new Button();
			this.paint_delay = new TextBox();
			this.but_font = new Button();
			((System.ComponentModel.ISupportInitialize)(this.button1)).BeginInit();
			this.button1.SuspendLayout();
			this.faTabStripItem1.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.faTabStripItem2.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.AlwaysShowClose = false;
			this.button1.AlwaysShowMenuGlyph = false;
			this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button1.Items.AddRange(new VarocalCross.FATabStripItem[] {
            this.faTabStripItem1,
            this.faTabStripItem2});
			this.button1.Location = new System.Drawing.Point(0, 0);
			this.button1.Name = "button1";
			this.button1.Padding = new System.Windows.Forms.Padding(1, 26, 1, 1);
			this.button1.SelectedItem = this.faTabStripItem1;
			this.button1.Size = new System.Drawing.Size(681, 167);
			this.button1.TabIndex = 8;
			this.button1.Text = "button1";
			// 
			// faTabStripItem1
			// 
			this.faTabStripItem1.CanClose = false;
			this.faTabStripItem1.Controls.Add(this.groupBox1);
			this.faTabStripItem1.Controls.Add(this.label1);
			this.faTabStripItem1.Controls.Add(this.recently_number);
			this.faTabStripItem1.Controls.Add(this.backup_number);
			this.faTabStripItem1.Controls.Add(this.last_startup);
			this.faTabStripItem1.Controls.Add(this.backup);
			this.faTabStripItem1.IsDrawn = true;
			this.faTabStripItem1.Name = "faTabStripItem1";
			this.faTabStripItem1.Selected = true;
			this.faTabStripItem1.Size = new System.Drawing.Size(679, 140);
			this.faTabStripItem1.StripRect = ((System.Drawing.RectangleF)(resources.GetObject("faTabStripItem1.StripRect")));
			this.faTabStripItem1.TabIndex = 0;
			this.faTabStripItem1.Title = "General";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.update_download_startup);
			this.groupBox1.Controls.Add(this.update_download_ask);
			this.groupBox1.Controls.Add(this.update_reminds);
			this.groupBox1.Controls.Add(this.update_none);
			this.groupBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.groupBox1.Location = new System.Drawing.Point(399, 6);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(258, 106);
			this.groupBox1.TabIndex = 7;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Update Settings";
			// 
			// update_download_startup
			// 
			this.update_download_startup.AutoSize = true;
			this.update_download_startup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.update_download_startup.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.update_download_startup.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.update_download_startup.Location = new System.Drawing.Point(7, 80);
			this.update_download_startup.Name = "update_download_startup";
			this.update_download_startup.Size = new System.Drawing.Size(206, 19);
			this.update_download_startup.TabIndex = 9;
			this.update_download_startup.Text = "Download and Update on startup";
			// 
			// update_download_ask
			// 
			this.update_download_ask.AutoSize = true;
			this.update_download_ask.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.update_download_ask.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.update_download_ask.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.update_download_ask.Location = new System.Drawing.Point(7, 60);
			this.update_download_ask.Name = "update_download_ask";
			this.update_download_ask.Size = new System.Drawing.Size(204, 19);
			this.update_download_ask.TabIndex = 8;
			this.update_download_ask.Text = "Download and Ask me for restart";
			// 
			// update_reminds
			// 
			this.update_reminds.AutoSize = true;
			this.update_reminds.Checked = true;
			this.update_reminds.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.update_reminds.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.update_reminds.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.update_reminds.Location = new System.Drawing.Point(7, 40);
			this.update_reminds.Name = "update_reminds";
			this.update_reminds.Size = new System.Drawing.Size(217, 19);
			this.update_reminds.TabIndex = 7;
			this.update_reminds.TabStop = true;
			this.update_reminds.Text = "Reminds me when there is a update";
			// 
			// update_none
			// 
			this.update_none.AutoSize = true;
			this.update_none.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.update_none.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.update_none.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.update_none.Location = new System.Drawing.Point(7, 20);
			this.update_none.Name = "update_none";
			this.update_none.Size = new System.Drawing.Size(52, 19);
			this.update_none.TabIndex = 6;
			this.update_none.Text = "None";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.label1.Location = new System.Drawing.Point(11, 5);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(199, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "How many recently programs show";
			// 
			// recently_number
			// 
			this.recently_number.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.recently_number.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.recently_number.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.recently_number.Location = new System.Drawing.Point(40, 22);
			this.recently_number.Name = "recently_number";
			this.recently_number.Size = new System.Drawing.Size(116, 23);
			this.recently_number.TabIndex = 1;
			this.recently_number.Text = "8";
			// 
			// backup_number
			// 
			this.backup_number.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.backup_number.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.backup_number.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.backup_number.Location = new System.Drawing.Point(40, 106);
			this.backup_number.Name = "backup_number";
			this.backup_number.Size = new System.Drawing.Size(116, 23);
			this.backup_number.TabIndex = 4;
			this.backup_number.Text = "3";
			// 
			// last_startup
			// 
			this.last_startup.AutoSize = true;
			this.last_startup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.last_startup.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.last_startup.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.last_startup.Location = new System.Drawing.Point(7, 64);
			this.last_startup.Name = "last_startup";
			this.last_startup.Size = new System.Drawing.Size(196, 19);
			this.last_startup.TabIndex = 2;
			this.last_startup.Text = "Load last opened file on startup";
			// 
			// backup
			// 
			this.backup.AutoSize = true;
			this.backup.Checked = true;
			this.backup.CheckState = System.Windows.Forms.CheckState.Checked;
			this.backup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.backup.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.backup.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.backup.Location = new System.Drawing.Point(7, 85);
			this.backup.Name = "backup";
			this.backup.Size = new System.Drawing.Size(313, 19);
			this.backup.TabIndex = 3;
			this.backup.Text = "Keep backup copies of files (with extension .tb1, .tb2)";
			// 
			// faTabStripItem2
			// 
			this.faTabStripItem2.CanClose = false;
			this.faTabStripItem2.Controls.Add(this.text_italic);
			this.faTabStripItem2.Controls.Add(this.label2);
			this.faTabStripItem2.Controls.Add(this.code_colors);
			this.faTabStripItem2.Controls.Add(this.tab_spaces);
			this.faTabStripItem2.Controls.Add(this.text_bold);
			this.faTabStripItem2.Controls.Add(this.label3);
			this.faTabStripItem2.Controls.Add(this.rect_change);
			this.faTabStripItem2.Controls.Add(this.paint_delay);
			this.faTabStripItem2.Controls.Add(this.but_font);
			this.faTabStripItem2.IsDrawn = true;
			this.faTabStripItem2.Name = "faTabStripItem2";
			this.faTabStripItem2.Size = new System.Drawing.Size(679, 140);
			this.faTabStripItem2.StripRect = ((System.Drawing.RectangleF)(resources.GetObject("faTabStripItem2.StripRect")));
			this.faTabStripItem2.TabIndex = 1;
			this.faTabStripItem2.Title = "Scripts and Codes";
			// 
			// text_italic
			// 
			this.text_italic.AutoSize = true;
			this.text_italic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.text_italic.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.text_italic.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.text_italic.Location = new System.Drawing.Point(577, 61);
			this.text_italic.Name = "text_italic";
			this.text_italic.Size = new System.Drawing.Size(52, 19);
			this.text_italic.TabIndex = 5;
			this.text_italic.Text = "Italic";
			this.text_italic.CheckedChanged += new System.EventHandler(this.text_italic_CheckedChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.label2.Location = new System.Drawing.Point(3, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(71, 15);
			this.label2.TabIndex = 2;
			this.label2.Text = "Tab amount";
			// 
			// code_colors
			// 
			this.code_colors.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.code_colors.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
			this.code_colors.FormattingEnabled = true;
			this.code_colors.ItemHeight = 18;
            this.code_colors.Items.AddRange( new object[ ] {
            "Keywords",
            "Values",
            "Comments",
            "Resources",
            "Selection"} );
			this.code_colors.Location = new System.Drawing.Point(499, 1);
			this.code_colors.Name = "code_colors";
			this.code_colors.Size = new System.Drawing.Size(69, 112);
			this.code_colors.TabIndex = 0;
			this.code_colors.SelectedIndexChanged += new System.EventHandler(this.code_colors_SelectedIndexChanged);
			// 
			// tab_spaces
			// 
			this.tab_spaces.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.tab_spaces.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.tab_spaces.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.tab_spaces.Location = new System.Drawing.Point(40, 21);
			this.tab_spaces.Name = "tab_spaces";
			this.tab_spaces.Size = new System.Drawing.Size(116, 23);
			this.tab_spaces.TabIndex = 3;
			this.tab_spaces.Text = "4";
			// 
			// text_bold
			// 
			this.text_bold.AutoSize = true;
			this.text_bold.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.text_bold.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.text_bold.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.text_bold.Location = new System.Drawing.Point(577, 36);
			this.text_bold.Name = "text_bold";
			this.text_bold.Size = new System.Drawing.Size(48, 19);
			this.text_bold.TabIndex = 4;
			this.text_bold.Text = "Bold";
			this.text_bold.CheckedChanged += new System.EventHandler(this.text_bold_CheckedChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.label3.Location = new System.Drawing.Point(3, 51);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(198, 15);
			this.label3.TabIndex = 4;
			this.label3.Text = "Show auto-color after delay (msec)";
			// 
			// rect_change
			// 
			this.rect_change.BackColor = System.Drawing.Color.Black;
			this.rect_change.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.rect_change.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.rect_change.ForeColor = System.Drawing.Color.Gray;
			this.rect_change.Location = new System.Drawing.Point(577, 1);
			this.rect_change.Name = "rect_change";
			this.rect_change.Size = new System.Drawing.Size(92, 29);
			this.rect_change.TabIndex = 3;
			this.rect_change.Text = "Change";
			this.rect_change.UseVisualStyleBackColor = false;
			this.rect_change.Click += new System.EventHandler(this.rect_change_Click);
			// 
			// paint_delay
			// 
			this.paint_delay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.paint_delay.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.paint_delay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.paint_delay.Location = new System.Drawing.Point(40, 74);
			this.paint_delay.Name = "paint_delay";
			this.paint_delay.Size = new System.Drawing.Size(116, 23);
			this.paint_delay.TabIndex = 5;
			this.paint_delay.Text = "750";
			// 
			// but_font
			// 
			this.but_font.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
			this.but_font.Font = new System.Drawing.Font("Calibri", 9.75F);
			this.but_font.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
			this.but_font.Location = new System.Drawing.Point(577, 89);
			this.but_font.Name = "but_font";
			this.but_font.Size = new System.Drawing.Size(87, 29);
			this.but_font.TabIndex = 6;
			this.but_font.Text = "Font";
			this.but_font.UseVisualStyleBackColor = false;
			this.but_font.Click += new System.EventHandler(this.but_font_Click);
			// 
			// PrefWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
			this.ClientSize = new System.Drawing.Size(681, 167);
			this.Controls.Add(this.button1);
			this.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "PrefWindow";
			this.Text = "Preferences";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PrefWindow_FormClosing);
			this.Load += new System.EventHandler(this.PrefWindow_Load);
			((System.ComponentModel.ISupportInitialize)(this.button1)).EndInit();
			this.button1.ResumeLayout(false);
			this.faTabStripItem1.ResumeLayout(false);
			this.faTabStripItem1.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.faTabStripItem2.ResumeLayout(false);
			this.faTabStripItem2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.GroupBox groupBox1;
		public RadioButton update_download_ask;
		public RadioButton update_reminds;
		public RadioButton update_none;
		public TextBox backup_number;
		public CheckBox backup;
		public CheckBox last_startup;
		public TextBox recently_number;
		public Label label1;
		public RadioButton update_download_startup;
		public TextBox paint_delay;
		public Label label3;
		public TextBox tab_spaces;
		public Label label2;
		public Button but_font;
		public CheckBox text_italic;
		public CheckBox text_bold;
		public Button rect_change;
		public System.Windows.Forms.ListBox code_colors;
		public FATabStrip button1;
		public FATabStripItem faTabStripItem1;
		public FATabStripItem faTabStripItem2;

	}
}