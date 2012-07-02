using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComponentAce.Compression.ZipForge;

namespace VarocalCross {
    public partial class PackageManager : BaseForm {
        OpenFileDialog OFD;
        public PackageManager( OpenFileDialog ofd ) {
            InitializeComponent( );
            OFD = ofd;
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
                        PackageManager_Load( null, null );
                    } ) );
                } ) ).Start( );
            }
        }

        private void button2_Click( object sender, EventArgs e ) {
            Close( );
        }

        private void button4_Click( object sender, EventArgs e ) {
            if ( packs.SelectedRows.Count != 0 ) {
                StreamReader sr = new StreamReader( "pkg.m" );
                MemorizePortable MemoryCore = new MemorizePortable( );
                while ( !sr.EndOfStream ) {
                    Package package = new Package( );
                    MemorizePortable package_mem = new MemorizePortable( );
                    package_mem.AddOwner( package );
                    package_mem.Load( sr );
                    MemoryCore.AddOwner( package );
                }
                sr.Close( );
                List<DataGridViewRow> lggr = new List<DataGridViewRow>( );
                foreach ( DataGridViewRow row in packs.SelectedRows ) {
                    lggr.Add( row );
                    foreach ( PropertyInfo prop in MemoryCore.lpi )
                        if ( prop.DeclaringType.Name == "Package" && prop.Name == "ID" )
                            if ( ( ( string )row.Cells[ 2 ].Value ) == prop.GetGetMethod( ).Invoke(
                                MemoryCore.owners[ MemoryCore.lpi.IndexOf( prop ) ], new object[ 0 ] ).ToString( ) ) {
                                Package _pack = ( Package )MemoryCore.owners[ MemoryCore.lpi.IndexOf( prop ) ];
                                string str = "pkg\\" + Build.GeneratePlatform( _pack.Platform, _pack.is64X ) + "\\" + _pack.ID;
                                MemoryCore.DeleteOwner( _pack );
                                try {
                                    File.Delete( str + ".a" );
                                } catch {
                                }
                                try {
                                    File.Delete( str + ".obj" );
                                } catch {
                                }
                                try {
                                    Directory.Delete( "src\\pkg\\" + _pack.ID, true );
                                } catch {
                                }
                                break;
                            }
                }
                foreach ( var v in lggr )
                    packs.Rows.Remove( v );
                File.Delete( "pkg.m" );
                MemoryCore.Save( "pkg.m" );
            }
        }

        private void PackageManager_Load( object sender, EventArgs e ) {
            MemorizePortable mem;
            packs.Rows.Clear( );
            try {
                StreamReader SR = new StreamReader( "pkg.m" );
                while ( !SR.EndOfStream ) {
                    Package package = new Package( );
                    mem = new MemorizePortable( );
                    mem.AddOwner( package );
                    mem.Load( SR );
                    packs.Rows.Add( package.Name, package.Version, package.ID.ToString( ), package.DefaultNamespace );
                }
                SR.Close( );
            } catch {
            }
        }

        private void button1_Click( object sender, EventArgs e ) {
            if ( packs.SelectedRows.Count != 0 ) {
                List<string> listString = new List<string>( );
                foreach ( DataGridViewRow row in packs.SelectedRows ) {
                    listString.Add( Directory.GetFiles( Path.Combine( "src\\pkg", ( ( string )row.Cells[ 2 ].Value ) ), "*.obj" )[0] );
                }
                new Thread( new ThreadStart( ( ) => {
                    new AssemblyView( listString.ToArray( ) ).ShowDialog( );
                } ) ).Start();
            }
        }
    }
}
