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
    public partial class TemplateImport : BaseForm {
        TemplateImportResult tir;
        public TemplateImport( TemplateImportResult resulttarget ) {
            InitializeComponent( );
            tir = resulttarget;
            if ( Directory.Exists( "templates" ) )
                treeView1.Nodes.AddRange( ( from v in Directory.GetDirectories( "templates" ) select new TreeNode( new DirectoryInfo( v ).Name ) ).ToArray( ) );
        }

        private void button1_Click( object sender, EventArgs e ) {
            tir.Cancelled = true;
            Close( );
        }

        private void button2_Click( object sender, EventArgs e ) {
            foreach ( TreeNode node in treeView1.Nodes )
                if ( node.Checked )
                    tir.importfrom.Add( node.Name );
            tir.Cancelled = false;
            Close( );
        }
    }
    public class TemplateImportResult{
        public List<String> importfrom = new List<string>( );
        public bool Cancelled = false;
    }
}
