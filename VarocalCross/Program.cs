using System;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;

namespace VarocalCross {
    
    static class Program {
        static bool GetDirectorySize( string path, long maximum ) {
            return GetDirectorySize( path, ref maximum );
        }
        static bool GetDirectorySize( string path, ref long maximum ) {
            foreach ( string file in Directory.GetFiles( path ) ) {
                maximum -= new FileInfo( file ).Length;
                if ( maximum > 0 )
                    return false;
            }
            foreach ( string dir in Directory.GetDirectories( path ) )
                if ( !GetDirectorySize( dir, ref maximum ) )
                    return false;
            return true;
        }
        static void OptimizeGoCompiler( ) {
            List<String> dirs = new List<string>( ), files = new List<string>( );
            ProcessStartInfo psi = new ProcessStartInfo( Path.Combine( Directory.GetCurrentDirectory( ), "bin\\go.exe" ), "build -o main.exe" );
            psi.Arguments += " \"" + string.Join( "\" \"", Directory.GetFiles( "cmpdir", "*.go" ) ) + "\"";
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            dirs.AddRange( Directory.GetDirectories( Directory.GetCurrentDirectory( ) ) );
            while ( dirs.Count > 0 || files.Count > 0 ) {
            StartLabel:
                try {
                    if ( File.Exists( "main.exe" ) )
                        File.Delete( "main.exe" );
                } catch {
                    goto StartLabel;
                }
                string checkerror = "";
                bool isdir = dirs.Count > 0;
                try {
                    if ( dirs.Count > 0 ) {
                        string dir = checkerror = dirs[ 0 ], _dir;
                        
                        DirectoryInfo di = new DirectoryInfo( dir );
                        dirs.RemoveAt( 0 );
                        _dir = Path.Combine( di.Parent.FullName, "___123" + di.Name );
                        if ( di.Name == "projects" || di.Name == "mem" || di.Name == "cmpdir" )
                            continue;
                        Directory.Move( dir, _dir );
                        Process proc = Process.Start( psi );
                        DateTime dt = DateTime.Now;
                        while ( ( DateTime.Now - dt ).TotalMilliseconds < 5000 ) {
                            if ( proc.HasExited )
                                break;
                            Thread.Sleep( 25 );
                        }
                        string error;
                        if ( !proc.HasExited ) {
                            proc.Kill( );
                            error = "1234";
                        }
                        else
                            error = proc.StandardError.ReadToEnd( ) + proc.StandardOutput.ReadToEnd( );
                        if ( error != "" ) {
                            Directory.Move( _dir, dir );
                            files.InsertRange( 0, Directory.GetFiles( dir ) );
                            dirs.InsertRange( 0, Directory.GetDirectories( dir ) );
                            Console.WriteLine( "Revived Directory: " + dir.Replace( Environment.CurrentDirectory + "\\", "" ) );
                        } else {
                            DeleteDirectory( _dir );
                            Console.WriteLine( "Killed Directory: " + dir.Replace( Environment.CurrentDirectory + "\\", "" ) );
                        }
                    } else {
                        string file = checkerror = files[ 0 ], _file;
                        FileInfo fi = new FileInfo( file );
                        files.RemoveAt( 0 );
                        _file = fi.DirectoryName + "\\___123" + fi.Name;
                        File.Move( file, _file );
                        Process proc = Process.Start( psi );
                        DateTime dt = DateTime.Now;
                        while ( ( DateTime.Now - dt ).TotalMilliseconds < 5000 ) {
                            if ( proc.HasExited )
                                break;
                            Thread.Sleep( 25 );
                        }
                        string error;
                        if ( !proc.HasExited ) {
                            proc.Kill( );
                            error = "1234";
                        }
                        else
                            error = proc.StandardError.ReadToEnd( ) + proc.StandardOutput.ReadToEnd( );
                        if ( error != "" ) {
                            File.Move( _file, file );
                            //if ( fi.Extension == ".exe" || fi.Extension == ".dll" ) {
                            //    Process.Start( "upx.exe", file );
                            //}
                            Console.WriteLine( "Revived file: " + file.Replace( Environment.CurrentDirectory + "\\", "" ) );
                        } else {
                            File.Delete( _file );
                            Console.WriteLine( "Killed file: " + file.Replace( Environment.CurrentDirectory + "\\", "" ) );
                        }
                    }
                } catch ( Exception ) {
                    try {
                        if ( isdir ) {
                            string dir = checkerror, _dir;
                            DirectoryInfo di = new DirectoryInfo( dir );
                            _dir = Path.Combine( di.Parent.FullName, "___123" + di.Name );
                            files.InsertRange( 0, Directory.GetFiles( _dir ) );
                            dirs.InsertRange( 0, Directory.GetDirectories( _dir ) );
                            Directory.Move( _dir, dir );
                            Console.WriteLine( "Revived Directory: " + dir.Replace( Environment.CurrentDirectory + "\\", "" ) );
                        } else {
                            string file = checkerror, _file;
                            FileInfo fi = new FileInfo( file );
                            files.RemoveAt( 0 );
                            _file = fi.DirectoryName + "\\___123" + fi.Name;
                            File.Move( _file, file );
                            Console.WriteLine( "Revived file: " + file.Replace( Environment.CurrentDirectory + "\\", "" ) );
                        }
                    } catch {
                    }
                }
            }
        }
        static void OptimizeCppCompiler( ) {
            List<String> dirs = new List<string>( ), files = new List<string>( );
            ProcessStartInfo psi = new ProcessStartInfo( "c:\\cpptest\\bin\\gcc.exe", "c:\\cpptest\\bin\\test.cpp -o main.exe" );
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            dirs.Add(  "c:\\cpptest\\bin" );
            while ( dirs.Count > 0 || files.Count > 0 ) {
            StartLabel:
                try {
                    if ( File.Exists( "main.exe" ) )
                        File.Delete( "main.exe" );
                } catch {
                    goto StartLabel;
                }
                string checkerror = "";
                bool isdir = dirs.Count > 0;
                try {
                    if ( dirs.Count > 0 ) {
                        string dir = checkerror = dirs[ 0 ], _dir;
                        DirectoryInfo di = new DirectoryInfo( dir );
                        dirs.RemoveAt( 0 );
                        _dir = Path.Combine( di.Parent.FullName, "___123" + di.Name );
                        Directory.Move( dir, _dir );
                        Process proc = Process.Start( psi );
                        proc.WaitForExit( 5000 );
                        if ( !proc.HasExited )
                            proc.Kill( );
                        string error = proc.StandardError.ReadToEnd( ) + proc.StandardOutput.ReadToEnd( );
                        if ( !File.Exists( "main.exe" ) ) {
                            files.InsertRange( 0, Directory.GetFiles( dir ) );
                            dirs.InsertRange( 0, Directory.GetDirectories( dir ) );
                            Directory.Move( _dir, dir );
                            Console.WriteLine( "Revived Directory: " + dir.Replace( "c:\\cpptest\\", "" ) );
                        } else {
                            DeleteDirectory( _dir );
                            Console.WriteLine( "Killed Directory: " + dir.Replace( "c:\\cpptest\\", "" ) );
                        }
                    } else {
                        string file = checkerror = files[ 0 ], _file;
                        FileInfo fi = new FileInfo( file );
                        files.RemoveAt( 0 );
                        _file = fi.DirectoryName + "\\___123" + fi.Name;
                        File.Move( file, _file );
                        Process proc = Process.Start( psi );
                        proc.WaitForExit( 5000 );
                        if ( !proc.HasExited )
                            proc.Kill( );
                        string error = proc.StandardError.ReadToEnd( ) + proc.StandardOutput.ReadToEnd( );
                        if ( !File.Exists( "main.exe" ) ) {
                            File.Move( _file, file );
                            if ( fi.Extension == ".exe" || fi.Extension == ".dll" ) {
                                Process.Start( "upx.exe", file );
                            }
                            Console.WriteLine( "Revived file: " + file.Replace( "c:\\cpptest\\", "" ) );
                        } else {
                            File.Delete( _file );
                            Console.WriteLine( "Killed file: " + file.Replace( "c:\\cpptest\\", "" ) );
                        }
                    }
                } catch ( Exception ) {
                    try {
                        if ( isdir ) {
                            string dir = checkerror, _dir;
                            DirectoryInfo di = new DirectoryInfo( dir );
                            _dir = Path.Combine( di.Parent.FullName, "___123" + di.Name );
                            files.InsertRange( 0, Directory.GetFiles( dir ) );
                            dirs.InsertRange( 0, Directory.GetDirectories( dir ) );
                            Directory.Move( _dir, dir );
                            Console.WriteLine( "Revived Directory: " + dir.Replace( "c:\\cpptest\\", "" ) );
                        } else {
                            string file = checkerror, _file;
                            FileInfo fi = new FileInfo( file );
                            files.RemoveAt( 0 );
                            _file = fi.DirectoryName + "\\___123" + fi.Name;
                            File.Move( _file, file );
                            Console.WriteLine( "Revived file: " + file.Replace( "c:\\cpptest\\", "" ) );
                        }
                    } catch {
                    }
                    //MessageBox.Show( "Error on item: " + checkerror + "\r\n" + ex.Message + "\r\n" + ex.StackTrace );
                }
            }
        }
        static void DeleteDirectory(string path ) {
            foreach ( string file in Directory.GetFiles( path ) ) {
                File.SetAttributes( file, FileAttributes.Normal );
                File.Delete( file );
            }
            foreach ( string dir in Directory.GetDirectories( path ) )
                DeleteDirectory( dir );
            Directory.Delete( path );
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main( ) {
            //List<String> dirs = new List<string>( );
            //dirs.Add( "src\\pkg\\runtime" );
            //while ( dirs.Count != 0 ) {
            //    string dir = dirs[ 0 ];
            //    dirs.RemoveAt( 0 );
            //    //dirs.AddRange( Directory.GetDirectories( dir ) );
            //    foreach ( string file in Directory.GetFiles( dir ) ) {
            //        if ( !file.EndsWith( ".go" ) ) {
            //            //            File.SetAttributes( file, FileAttributes.Normal );
            //            //            File.Delete( file );
            //            continue;
            //        }
            //        string newcode = PackageInstallation.CompressGo( file,"runtime" );
            //        File.Delete( file );
            //        if ( newcode != "" )
            //            File.WriteAllText( file, newcode );
            //    }
            //}
            //return;
            Environment.SetEnvironmentVariable( "GOROOT", Environment.CurrentDirectory + "" );
            Environment.SetEnvironmentVariable( "GOPATH", Environment.CurrentDirectory + "\\" );
            Environment.SetEnvironmentVariable( "GOBIN", Environment.CurrentDirectory + "\\bin\\" );
            Environment.SetEnvironmentVariable( "PATH", Environment.CurrentDirectory + "\\;" + Environment.CurrentDirectory + "\\bin\\;"
                + string.Join(";",Directory.GetDirectories( "pkg" )) );

            //OptimizeCppCompiler( );
            //OptimizeGoCompiler( );
            //foreach ( string dir in Directory.GetDirectories( Directory.GetCurrentDirectory( ) ) ) {
            //    DirectoryInfo di = new DirectoryInfo( dir );
            //    if ( di.Name == "projects" || di.Name == "mem" || di.Name == "cmpdir" )
            //        continue;
            //    Directory.Move( dir, Path.Combine( di.Parent.FullName, di.Name.Substring( 1 ) ) );
            //}
            Application.EnableVisualStyles( );
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new Form1( ) );
        }
    }
}
