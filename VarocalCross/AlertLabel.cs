using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VarocalCross {
    public partial class AlertLabel : UserControl {
        public AlertLabel( ) {
            InitializeComponent( );
            label1.Text = "";
            label1_SizeChanged( null, null );
        }

        private void label1_SizeChanged( object sender, EventArgs e ) {
            Size = label1.Size;
        }

        public void Alert( Color color, string text ) {
            label1.ForeColor = color;
            label1.Text = text;
            timer1.Enabled = true;
        }

        private void timer1_Tick( object sender, EventArgs e ) {
            Color c = label1.ForeColor, c2 = this.BackColor;
            label1.ForeColor = Color.FromArgb( 255, c.R - Math.Sign( c.R - c2.R ) * 3, c.G - Math.Sign( c.G - c2.G ) * 3, c.B - Math.Sign( c.B - c2.B ) * 3 );
            if ( Math.Abs( c.R -c2.R ) < 4
                && Math.Abs( c.G - c2.G ) < 4
                && Math.Abs( c.B - c2.B ) < 4 ) {
                label1.ForeColor = c2;
                timer1.Enabled = false;
            }
        }
    }
}
