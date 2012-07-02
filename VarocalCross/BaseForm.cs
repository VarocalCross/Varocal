﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VarocalCross {
    public class BaseForm : Form {
        private KeyboardHook hook = new KeyboardHook( );
        public event EventHandler<KeyPressedEventArgs> OnHotKeyPressed;
        public void AddHotKey( ModifierKeys mod,Keys keys ) {
            hook.RegisterHotKey( mod, keys );
        }
        protected override void OnLoad(EventArgs e)
        {
            this.ControlAdded += BaseForm_ControlAdded;
            this.BackColor = System.Drawing.Color.FromArgb( 50, 50, 50 );
            base.OnLoad( e );
            foreach ( Control control in Controls )
                BaseForm_ControlAdded( this, new ControlEventArgs( control ) );
            //SetStyle( ControlStyles.OptimizedDoubleBuffer,true );
            DoubleBuffered = true;
            ResizeRedraw = true;
            hook.KeyPressed += hook_KeyPressed;
        }

        void hook_KeyPressed( object sender, KeyPressedEventArgs e ) {
            if ( OnHotKeyPressed != null )
                OnHotKeyPressed( this, e );
        }
        public static void BaseForm_ControlAdded( object sender, ControlEventArgs e2 ) {
            dynamic control = e2.Control;
            #region TextBox
            if ( control is TextBox ) {
                control.Font = new System.Drawing.Font( "Calibri", 9.75f );
                control.ForeColor = Color.FromArgb( 225, 225, 225 );
                control.BackColor = Color.FromArgb( 70, 70, 70 );
                control.Paint += new PaintEventHandler( ( x, e ) => {
                    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                } );
            #endregion
            #region CheckBox
            } else if ( control is CheckBox ) {
                control.Font = new System.Drawing.Font( "Calibri", 9.75f );
                control.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                control.ForeColor = Color.FromArgb( 225, 225, 225 );
                control.Paint += new PaintEventHandler( ( x, e ) => {
                    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                } );
                #endregion
            #region Label
            } else if ( control is Label ) {
                control.Font = new System.Drawing.Font( "Calibri", control.Font.Size );
                control.ForeColor = Color.FromArgb( 225, 225, 225 );
                control.Paint += new PaintEventHandler( ( x, e ) => {
                    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                } );
                #endregion
            #region RadioButton
            } else if ( control is RadioButton ) {
                control.Font = new System.Drawing.Font( "Calibri", 9.75f );
                control.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                control.ForeColor = Color.FromArgb( 225, 225, 225 );
                control.Paint += new PaintEventHandler( ( x, e ) => {
                    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                } );
                #endregion
            #region RichTextBox
            } else if ( control is RichTextBox ) {
                control.Font = new System.Drawing.Font( "Calibri", 11f );
                control.ForeColor = Color.FromArgb( 225, 225, 225 );
                control.BackColor = Color.FromArgb( 70, 70, 70 );
                control.Paint += new PaintEventHandler( ( x, e ) => {
                    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                } );
                control.SelectionChanged += new EventHandler( ( x, e ) => {

                } );
                #endregion
            #region ComboBox
            } else if ( control is ComboBox ) {
                control.Font = new System.Drawing.Font( "Calibri", 9.75f );
                control.DropDownStyle = ComboBoxStyle.DropDownList;
                control.Paint += new PaintEventHandler( ( x, e ) => {
                    control.DropDownStyle = ComboBoxStyle.DropDownList;
                    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                } );
                #endregion
            #region MenuStrip
            } else if ( control is MenuStrip ) {
                control.BackColor = Color.FromArgb( 50, 50, 50 );
                //control.ShowImageMargin = false;
                #endregion
            #region Button
            } else if ( control is Button ) {
                control.Font = new System.Drawing.Font( "Calibri", 9.75f );
                control.ForeColor = Color.FromArgb( 200, 200, 200 );
                control.BackColor = control.Parent.BackColor;
                bool MouseOver = false;
                bool isMousePressed = false;
                control.Paint += new PaintEventHandler( ( x, e ) => {
                    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillRectangle( new SolidBrush( control.BackColor ), e.ClipRectangle );
                    if ( isMousePressed )
                        e.Graphics.FillRectangle( new SolidBrush( Color.FromArgb( 196, 229, 246 ) ), e.ClipRectangle );
                    else if ( MouseOver )
                        e.Graphics.FillRectangle( new SolidBrush( Color.FromArgb( 128, 190, 230, 253 ) ), e.ClipRectangle );
                    e.Graphics.DrawRectangle( ( control.Focused ? new Pen( Color.FromArgb( 128, 60, 127, 177 ), 5 ) : new Pen( Color.FromArgb( 128, 112, 112, 112 ),5 ) ),
                        new Rectangle( 0, 0, control.Width - 1, control.Height - 1 ) );
                    e.Graphics.DrawString( control.Text, control.Font, new SolidBrush( control.ForeColor ), e.ClipRectangle, new StringFormat( ) {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    } );
                } );
                control.MouseEnter += new EventHandler( ( x, e ) => {
                    MouseOver = true;
                } );
                control.MouseLeave += new EventHandler( ( x, e ) => {
                    MouseOver = false;
                } );
                control.MouseDown += new MouseEventHandler( ( x, mevent ) => {
                    if ( mevent.Button == System.Windows.Forms.MouseButtons.Left )
                        isMousePressed = true;
                } );
                control.MouseUp += new MouseEventHandler( ( x, mevent ) => {
                    if ( mevent.Button == System.Windows.Forms.MouseButtons.Left )
                        isMousePressed = false;
                } );
            }
                #endregion
            #region FATabStrip
 else if ( control is FATabStrip ) {
                control.AddedTab += new EventHandler<FATabStripItem>( ( x, e ) => {
                    e.ControlAdded += BaseForm_ControlAdded;
                } );
                foreach ( FATabStripItem item in control.Items ) {
                    item.ControlAdded += BaseForm_ControlAdded;
                    foreach ( Control control2 in item.Controls )
                        BaseForm_ControlAdded( control.Parent, new ControlEventArgs( control2 ) );
                }
            }
            #endregion
            #region TreeView2
 else if ( control is TreeView2 ) {
                control.BackColor = Color.FromArgb( 70, 70, 70 );
                control.ForeColor = Color.FromArgb( 150, 150, 150 );
            }
            #endregion
                #region Panel
 else if ( control is Panel ) {
                control.BackColor = Color.FromArgb( 50, 50, 50 );
                foreach ( Control control2 in control.Controls )
                    BaseForm_ControlAdded( control.Parent, new ControlEventArgs( control2 ) );
            }
            #endregion
            ( ( Control )control ).ControlAdded += BaseForm_ControlAdded;
        }
        private void InitializeComponent( ) {
            this.SuspendLayout();
            // 
            // BaseForm
            // 
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Name = "BaseForm";
            this.ResumeLayout(false);

        }
    }
}