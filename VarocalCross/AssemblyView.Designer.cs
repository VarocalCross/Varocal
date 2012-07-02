namespace VarocalCross {
    partial class AssemblyView {
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
            this.treeView21 = new VarocalCross.TreeView2();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView21
            // 
            this.treeView21.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.treeView21.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView21.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView21.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.treeView21.Location = new System.Drawing.Point(0, 0);
            this.treeView21.Margin = new System.Windows.Forms.Padding(10);
            this.treeView21.Name = "treeView21";
            this.treeView21.Size = new System.Drawing.Size(477, 399);
            this.treeView21.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 399);
            this.panel1.Margin = new System.Windows.Forms.Padding(10);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(477, 23);
            this.panel1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.button1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Margin = new System.Windows.Forms.Padding(10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(477, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // AssemblyView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(477, 422);
            this.Controls.Add(this.treeView21);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.Name = "AssemblyView";
            this.Text = "AssemblyView";
            this.Load += new System.EventHandler(this.AssemblyView_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TreeView2 treeView21;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
    }
}