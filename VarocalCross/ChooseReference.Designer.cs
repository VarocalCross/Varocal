namespace VarocalCross {
    partial class ChooseReference {
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.col_select = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.col_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_version = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_defaultnamespace = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.button1.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.button1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.button1.Location = new System.Drawing.Point(13, 384);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.button2.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.button2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.button2.Location = new System.Drawing.Point(478, 384);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "OK";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_select,
            this.col_name,
            this.col_version,
            this.col_id,
            this.col_defaultnamespace});
            this.dataGridView1.Location = new System.Drawing.Point(13, 13);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(540, 365);
            this.dataGridView1.TabIndex = 3;
            // 
            // col_select
            // 
            this.col_select.HeaderText = "";
            this.col_select.Name = "col_select";
            this.col_select.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.col_select.Width = 20;
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
            this.button3.Location = new System.Drawing.Point(397, 384);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "Install";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.button4.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.button4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.button4.Location = new System.Drawing.Point(316, 384);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 5;
            this.button4.Text = "View";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // ChooseReference
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 419);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ChooseReference";
            this.Text = "ChooseReference";
            this.Load += new System.EventHandler(this.ChooseReference_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn col_select;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_version;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_defaultnamespace;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
    }
}