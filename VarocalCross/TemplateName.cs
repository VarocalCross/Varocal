using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VarocalCross {
    public partial class TemplateName : BaseForm {
        TemplateNameResult tnr;
        public TemplateName( TemplateNameResult resulttarget ) {
            InitializeComponent( );
            tnr = resulttarget;
        }

        private void button1_Click( object sender, EventArgs e ) {
            if ( textBox1.Text == "" ) {
                alertLabel1.Alert( Color.Red, "Please fill template's name" );
                return;
            }
            if ( Directory.Exists( "template\\" + textBox1.Text ) ) {
                alertLabel1.Alert( Color.Red, "Template exists" );
                return;
            }
            foreach ( char chr in Path.GetInvalidFileNameChars( ) ) {
                if ( textBox1.Text.Contains( chr ) ) {
                    alertLabel1.Alert( Color.Red, "Template's name is invalid" );
                    return;
                }
            }
            tnr.Name = textBox1.Text;
            tnr.Cancelled = false;
            Close( );
        }

        private void button2_Click( object sender, EventArgs e ) {
            tnr.Cancelled = true;
            Close( );
        }

        private void textBox1_KeyPress( object sender, KeyPressEventArgs e ) {
            if ( e.KeyChar == ( char )Keys.Enter )
                button1_Click( null, null );
        }
    }
    public class TemplateNameResult {
        public string Name = "";
        public bool Cancelled = false;
    }
}
