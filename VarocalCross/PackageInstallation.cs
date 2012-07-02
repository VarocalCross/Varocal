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
    public partial class PackageInstallation : BaseForm {
        string pkg;
        Package package = new Package( );

        public PackageInstallation( string path ) {
            InitializeComponent( );
            pkg = path;
        }

        private void PackageInstallation_Load( object sender, EventArgs e ) {
            MemorizePortable mem = new MemorizePortable( );
            mem.AddOwner( package );
            mem.Load( Path.Combine( pkg, "install.dat" ) );
            txt_name.Text = package.Name;
            txt_platform.Text = package.Platform.ToString( ) + " (" + ( package.is64X ? "64x)" : "86x)" );
            txt_namespace.Text = package.DefaultNamespace;
            txt_description.Text = package.Description;
            txt_version.Text = package.Version;
        }

        private void button3_Click( object sender, EventArgs e ) {
            Close( );
        }

        private void button2_Click( object sender, EventArgs e ) {
            StreamWriter streamWriter = new StreamWriter( Stream.Null );
            try {
                List<Package> packages = new List<Package>( );
                MemorizePortable mem;
                StreamReader SR = new StreamReader( Stream.Null );
                try {
                    SR = new StreamReader( "pkg.m" );
                    while ( !SR.EndOfStream ) {
                        Package package = new Package( );
                        mem = new MemorizePortable( );
                        mem.AddOwner( package );
                        mem.Load( SR );
                        packages.Add( package );
                    }
                } catch {
                } finally {
                    SR.Close( );
                }
                foreach ( Package pack in packages )
                    if ( package.ID == pack.ID ) {
                        alertLabel1.Alert( Color.Red, "Package is exists" );
                        return;
                    }
                streamWriter = new StreamWriter( "pkg.m", true );
                mem = new MemorizePortable( );
                mem.AddOwner( package );
                File.Move( Directory.GetFiles( pkg, "*.a" )[ 0 ], Path.Combine("pkg\\"+Build.GeneratePlatform(package.Platform,package.is64X),package.ID + ".a") );
                if ( Directory.Exists( "src\\pkg\\" + package.ID ) )
                    Directory.Delete( "src\\pkg\\" + package.ID, true );
                Directory.CreateDirectory( "src\\pkg\\" + package.ID );
                File.Move( Directory.GetFiles( pkg, "*.obj" )[ 0 ], Path.Combine( "pkg\\" + Build.GeneratePlatform( package.Platform, package.is64X ), package.ID + ".obj" ) );
                //foreach ( string file in Directory.GetFiles( pkg, "*.c" ) )
                //    File.Move( file, "src\\pkg\\" + package.ID + "\\" + Path.GetFileName( file ) );
                foreach ( string file in Directory.GetFiles( pkg, "*.go" ) )
                    File.Move( file, "src\\pkg\\" + package.ID + "\\" + Path.GetFileName( file ) );
                foreach ( string file in Directory.GetFiles( pkg, "*.cs" ) )
                    File.Move( file, "src\\pkg\\" + package.ID + "\\" + Path.GetFileName( file ) );
                alertLabel1.Alert( Color.Green, "Package's installation succesfully installed" );
                mem.Save( streamWriter );
                streamWriter.Close( );
            //} catch (Exception ex) {
                //alertLabel1.Alert( Color.Red, "Unable to install package" );
            } finally {
                streamWriter.Close( );
            }
        }

        public static string CompressGo( string path,string pack="" ) {
            Parser.Source res = new Parser.Source();
            StreamReader sr = new StreamReader( path );
            while ( !sr.EndOfStream ) {
                string line = sr.ReadLine( ).Trim( );
                if ( line.StartsWith( "package" ) ) {
                    if ( !line.Trim( ).EndsWith( " " + pack ) && pack != "" ) {
                        sr.Close( );
                        return "";
                    }
                    res.AppendLine( line.Trim( ) );
                } else if ( line.StartsWith( "func" ) ) {
                    res.AppendLine( ( line.Contains( '{' ) ? line.Remove( line.LastIndexOf( '{' ) ) : line ).Trim( ) );
                    if ( line.Contains( '{' ) && !line.Contains( '}' ) ) {
                        int i = 1;
                        while ( i > 0 ) {
                            line = sr.ReadLine( );
                            while ( line.Contains( "\"" ) ) {
                                int start = line.IndexOf( '"' );
                                line = line.Remove( start, line.IndexOf( '"', start + 1 ) - start + 1 );
                            }
                            i += line.Count( '{' ) - line.Count( '}' );
                        }
                    }
                } else if ( line.StartsWith( "type" ) ) {
                    res.AppendLine( line.Trim( ) );
                    if ( line.Contains( '{' ) && !line.Contains( '}' ) ) {
                        int i = 1;
                        while ( i > 0 ) {
                            line = sr.ReadLine( );
                            while ( line.Contains( "\"" ) ) {
                                int start = line.IndexOf( '"' );
                                line = line.Remove( start, line.IndexOf( '"', start + 1 ) - start );
                            }
                            i += line.Count( '{' ) - line.Count( '}' );
                            res.AppendLine( line );
                        }
                    }
                } else if ( line.StartsWith( "var" ) ) {
                    if ( line.Contains( "=" ) )
                        line = line.Remove( line.IndexOf( '=' ) );
                    res.AppendLine( line.Trim( ) );
                }
            }
            sr.Close( );
            return res.Content;
        }

        private void button1_Click( object sender, EventArgs e ) {
            new AssemblyView( new string[ ] { Directory.GetFiles( pkg, "*.obj" )[ 0 ] } ).ShowDialog( this );
        }
    }
}
