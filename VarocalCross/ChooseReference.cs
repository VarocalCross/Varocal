using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComponentAce.Compression.ZipForge;

namespace VarocalCross {
    public partial class ChooseReference : BaseForm {
        ReferencesObj ro;
        List<int> una;
        bool is64x;
        Build.Platform platform;
        OpenFileDialog OFD;
        public ChooseReference( ReferencesObj referenceObj,List<int> Unallowed,Build.Platform platform,bool is64x,OpenFileDialog ofd ) {
            InitializeComponent( );
            ro = referenceObj;
            una = Unallowed;
            this.platform = platform;
            this.is64x = is64x;
            OFD = ofd;
        }

        private void ChooseReference_Load( object sender, EventArgs e ) {
            List<Package> packages = new List<Package>( );
            MemorizePortable mem;
            dataGridView1.Rows.Clear( );
            try {
                StreamReader SR = new StreamReader( "pkg.m" );
                while ( !SR.EndOfStream ) {
                    Package package = new Package( );
                    mem = new MemorizePortable( );
                    mem.AddOwner( package );
                    mem.Load( SR );
                    foreach (int id in una)
                        if ( id == package.ID ) {
                            package = null;
                        }
                    if ( package == null )
                        continue;
                    else {
                        if ( package.Platform != platform || package.is64X != is64x )
                            continue;
                    }
                    dataGridView1.Rows.Add( false, package.Name, package.Version, package.ID.ToString( ), package.DefaultNamespace );
                }
                SR.Close( );
            } catch {
            }
        }

        private void button2_Click( object sender, EventArgs e ) {
            ro.Cancelled = false;
            foreach ( DataGridViewRow row in dataGridView1.Rows ) {
                if ( ( bool )row.Cells[ 0 ].Value ) {
                    ro.IDS.Add( int.Parse( ( string )row.Cells[ 3 ].Value ) );
                }
            }
            Close( );
        }

        private void button1_Click( object sender, EventArgs e ) {
            Close( );
        }

        private void button3_Click( object sender, EventArgs e ) {
            OFD.Filter = "Varocal Package File (*.vpak)|*.vpak|Anyfile (*.*)|*.*";
            if ( OFD.ShowDialog( ) == System.Windows.Forms.DialogResult.OK ) {
                var v = new ZipForge( );
                v.FileName = OFD.FileName;
                v.OpenArchive( System.IO.FileMode.Open );
                v.BaseDir = Path.GetFullPath( "pkgcmp" );
                v.ExtractFiles( "*.*" );
                new Thread( new ThreadStart( ( ) => {
                    new PackageInstallation( "pkgcmp" ).ShowDialog( );
                    this.BeginInvoke( new MethodInvoker( ( ) => {
                        ChooseReference_Load( null, null );
                    } ) );
                } ) ).Start( );
            }
        }

        private void button4_Click( object sender, EventArgs e ) {
            if ( dataGridView1.SelectedRows.Count != 0 ) {
                List<string> listString = new List<string>( );
                foreach ( DataGridViewRow row in dataGridView1.SelectedRows ) {
                    listString.Add( Directory.GetFiles( Path.Combine( "src\\pkg\\", ( ( string )row.Cells[ 2 ].Value ) ), "*.obj" )[ 0 ] );
                }
                new Thread( new ThreadStart( ( ) => {
                    new AssemblyView( listString.ToArray( ) ).ShowDialog( );
                } ) ).Start( );
            }
        }
    }
    public class ReferencesObj {
        public List<int> IDS = new List<int>( );
        public bool Cancelled = true;
    }
}
