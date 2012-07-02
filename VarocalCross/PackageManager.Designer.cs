namespace VarocalCross {
    partial class PackageManager {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing ) {
            if ( disposing && ( components != null ) ) {
                components.Dispose( );
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent( ) {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.button4 = new System.Windows.Forms.Button();
            this.packs = new System.Windows.Forms.DataGridView();
            this.col_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_version = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_defaultnamespace = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button3 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.packs)).BeginInit();
            this.SuspendLayout();
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.button4.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.button4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.button4.Location = new System.Drawing.Point(93, 427);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 28;
            this.button4.Text = "Remove";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // packs
            // 
            this.packs.AllowUserToAddRows = false;
            this.packs.AllowUserToDeleteRows = false;
            this.packs.AllowUserToOrderColumns = true;
            this.packs.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.packs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 11F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.packs.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.packs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.packs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_name,
            this.col_version,
            this.col_id,
            this.col_defaultnamespace});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 11F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.packs.DefaultCellStyle = dataGridViewCellStyle2;
            this.packs.Location = new System.Drawing.Point(12, 12);
            this.packs.Name = "packs";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.packs.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.packs.RowHeadersVisible = false;
            this.packs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.packs.Size = new System.Drawing.Size(533, 409);
            this.packs.TabIndex = 27;
            // 
            // col_name
            // 
            this.col_name.HeaderText = "Name";
            this.col_name.Name = "col_name";
            this.col_name.ReadOnly = true;
            this.col_name.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.col_name.Width = 108;
            // 
            // col_version
            // 
            this.col_version.HeaderText = "Version";
            this.col_version.Name = "col_version";
            this.col_version.ReadOnly = true;
            this.col_version.Width = 107;
            // 
            // col_id
            // 
            this.col_id.HeaderText = "ID";
            this.col_id.Name = "col_id";
            this.col_id.ReadOnly = true;
            this.col_id.Width = 107;
            // 
            // col_defaultnamespace
            // 
            this.col_defaultnamespace.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.col_defaultnamespace.HeaderText = "Namespace";
            this.col_defaultnamespace.Name = "col_defaultnamespace";
            this.col_defaultnamespace.ReadOnly = true;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.button3.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.button3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.button3.Location = new System.Drawing.Point(470, 427);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 26;
            this.button3.Text = "Install";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.button1.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.button1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.button1.Location = new System.Drawing.Point(389, 427);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 29;
            this.button1.Text = "View";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.button2.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.button2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.button2.Location = new System.Drawing.Point(12, 427);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 30;
            this.button2.Text = "Exit";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // PackageManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(557, 462);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.packs);
            this.Controls.Add(this.button3);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PackageManager";
            this.Text = "Package Manager";
            this.Load += new System.EventHandler(this.PackageManager_Load);
            ((System.ComponentModel.ISupportInitialize)(this.packs)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button4;
        public System.Windows.Forms.DataGridView packs;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_version;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_defaultnamespace;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}