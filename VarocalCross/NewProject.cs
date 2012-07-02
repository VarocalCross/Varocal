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
    public partial class NewProject : BaseForm {
        NewProjectResult npr;
        public NewProject( NewProjectResult resulttarget ) {
            npr = resulttarget;
            InitializeComponent( );
            if ( Directory.Exists( "templates" ) )
                treeView1.Nodes.AddRange( ( from v in Directory.GetDirectories( "templates" ) select new TreeNode( new DirectoryInfo( v ).Name ) ).ToArray( ) );
            textBox1.Focus( );
        }

        private void button2_Click( object sender, EventArgs e ) {
            npr.Cancelled = true;
            Close( );
        }

        private void button1_Click( object sender, EventArgs e ) {
            if ( textBox1.Text == "" ) {
                alertLabel1.Alert( Color.Red, "Please fill project's name" );
                return;
            }
            if ( Directory.Exists( "projects\\" + textBox1.Text ) ) {
                alertLabel1.Alert( Color.Red, "Project exists" );
                return;
            }
            foreach ( char chr in Path.GetInvalidFileNameChars() ) {
                if ( textBox1.Text.Contains(chr) ) {
                    alertLabel1.Alert( Color.Red, "Project's name is invalid" );
                    return;
                }
            }
            npr.Name = textBox1.Text;
            foreach ( TreeNode node in treeView1.Nodes )
                if ( node.Checked )
                    npr.importfrom.Add( node.Name );
            npr.Cancelled = false;
            Close( );
        }

        private void textBox1_KeyPress( object sender, KeyPressEventArgs e ) {
            if ( e.KeyChar == (char)Keys.Enter )
                button1_Click( null, null );
        }
    }
    public class NewProjectResult {
        public string Name="";
        public List<String> importfrom = new List<String>( );
        public bool Cancelled = true;
    }
}
