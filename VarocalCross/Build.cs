using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CSharp;

namespace VarocalCross {
    public class Build {
        public Parser parser {
            get;
            set;
        }
        public Parser lastParser {
            get;
            set;
        }
        public CSharpCodeProvider provider {
            get;
            set;
        }
        public bool Threaded {
            get {
                return parser.Threaded;
            }
            set {
                parser.Threaded = value;
            }
        }
        public Build( ) {
            parser = new Parser( );
            provider = new CSharpCodeProvider( );
        }
        public void AddSource( string content ) {
            parser.Collect( content );
        }
        /// <summary>
        /// Function that Does The Process of Interpreting
        /// </summary>
        /// <param name="path">Directory to Save to</param>
        /// <returns>Array of Files's Path</returns>
        public string[] InterpretTo( string path,out BuildResult result,bool isDll,string namepack,int ID ) {
            Parser newParser = new Parser( );
            try {
                if ( Directory.Exists( path ) )
                    Directory.Delete( path, true );
                Directory.CreateDirectory( path );
                parser.CompilingDirectory = path;
                parser.Package = ( isDll ? namepack : "main" );
                parser.isDll = isDll;
                parser.ID = ID;
                parser.ParseDetails( );
                parser.WaitThreadZero( );
                List<String> files = new List<string>( );
                foreach ( Parser.Source source in parser.Sources ) {
                    File.WriteAllText( Path.Combine( path, source.Name + ".cs" ), source.Content );
                    files.Add( source.Name );
                }
                result = new BuildResult( parser.Successful ? BuildResultType.None : BuildResultType.InterpretExceptions );
                result.Result = parser.ErrorCollector.ToArray( );
                newParser = new Parser( );
                newParser.Acc = parser.Acc;
                newParser.CodeStyle = parser.CodeStyle;
                newParser.Threaded = parser.Threaded;
                newParser.ThreadHit = parser.ThreadHit;
                newParser.Tokenizer = parser.Tokenizer;
                parser = newParser;
                return files.ToArray( );
            } catch ( Exception ex ) {
                result = new BuildResult( BuildResultType.InterpretExceptions );
                result.Result = new Parser.CodeErrorException[ ] { new Parser.CodeErrorException( 0, 0, "", ex.Message ) };
                return new string[ 0 ];
            } finally {
                lastParser = parser;
                newParser.Acc = parser.Acc;
                newParser.CodeStyle = parser.CodeStyle;
                newParser.Threaded = parser.Threaded;
                newParser.ThreadHit = parser.ThreadHit;
                newParser.Tokenizer = parser.Tokenizer;
                newParser.CompilingDirectory = path;
                parser = newParser;
            }
        }
        public string Linking( string path, out BuildResult result,
            int WarningLevel, bool TreatWarningAsErrors, bool isDll, List<string> References,string Package ) {
                var parameters = new CompilerParameters( new string[ 0 ], Path.Combine( path, ( Package == "BaseProject" ? Package : Path.GetRandomFileName( ) ) + ".obj" ), false );
            parameters.TreatWarningsAsErrors = false;
            parameters.WarningLevel = 0;
            parameters.GenerateExecutable = ( isDll ? false : true );
            parameters.GenerateInMemory = false;
            parameters.CompilerOptions = "/optimize- /platform:x86 /unsafe"
                + ( TreatWarningAsErrors ? " /warnaserror+" : " /warnaserror-" )
                + " /warn:" + WarningLevel;
            string[ ] refs = References.ToArray( );
            AssemblyResolver.Update( refs );
            //parameters.ReferencedAssemblies.AddRange( refs );
            List<string> test_files = new List<string>( );
            test_files.AddRange( Directory.GetFiles( path, "*.cs" ) );
            foreach ( string _ref in refs )
                try {
                    test_files.AddRange( Directory.GetFiles( "src\\pkg\\" + Path.GetFileNameWithoutExtension( _ref ), "*.cs" ) );
                } catch {
                    MessageBox.Show( "Unable to load reference ID: " + Path.GetFileNameWithoutExtension( _ref ) );
                }
            var compiler = new CSharpCodeProvider( new Dictionary<string, string> { { "CompilerVersion", "v4.0" } } );
            var linkingtest = compiler.CompileAssemblyFromFile( parameters, test_files.ToArray( ) );
            result = new BuildResult( linkingtest.Errors.HasErrors ? BuildResultType.LinkingExceptions : BuildResultType.None );
            if ( result.Type == BuildResultType.LinkingExceptions ) {
                List<CompilerError> test = new List<CompilerError>( );
                foreach ( CompilerError v in linkingtest.Errors ) {
                    if ( !v.IsWarning )
                        test.Add( v );
                }
                result.Result = test.ToArray( );
            }
            return parameters.OutputAssembly;
        }
        public string[ ] Analyzing( string path, bool isDll, string namepack, List<string> References, Platform platform, bool is64
            ,string cmpdir,int ID) {
            ExecutableAnalied executableAnalied = new ExecutableAnalied( Directory.GetFiles( path, "*.obj" )[ 0 ] );
            List<string> GoFiles = new List<string>( );
            executableAnalied.RunSources( ( key, text ) => {
                GoFiles.Add( key = Path.Combine( path, key += ".go" ) );
                File.WriteAllText( key, text );
                return true;
            }, ( str ) => {
                return true;
            }, isDll, ( isDll ? namepack : "main" ), Directory.GetFiles( path, "*.c" ).ToList( ), References, platform, is64
            , cmpdir,ID );
            return GoFiles.ToArray( );
        }
        public enum Platform {
            Windows,
            Linux,
            Mac
        }
        static public string GeneratePlatform( Platform platform, bool is64X ) {
            string plat = "";
            switch ( platform ) {
                case Platform.Windows:
                    plat = "windows";
                    break;

                case Platform.Linux:
                    plat = "linux";
                    break;

                case Platform.Mac:
                    plat = "darwin";
                    break;
            }
            return plat + "_" + ( is64X ? "amd64" : "386" );
        }
        public string Compiling( string path, out BuildResult result,
            bool UPX,Platform platform,bool is64,bool isDll) {
            string output = Path.Combine( path, Path.GetRandomFileName( ) + ".exe" );
            ProcessStartInfo psi = new ProcessStartInfo( "bin\\go.exe", ( isDll ? "build -o pkg\\" + GeneratePlatform( platform, is64 ) + "\\" + ( path.Contains( "\\" )
                ? path.Substring( path.LastIndexOf( '\\' ) + 1 ) : path )+".a " + ( path.Contains( "\\" )
                ? path.Substring( path.LastIndexOf( '\\' ) + 1 ) : path ) : "build -o main.exe" ) );
            if ( !isDll )
                psi.Arguments += " \"" + string.Join( "\" \"", Directory.GetFiles( path, "*.go" ) ) + "\"";
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            string plat = "";
            #region Corrent Platform
            switch ( platform ) {
                case Platform.Windows:
                    plat = "windows";
                    break;

                case Platform.Linux:
                    plat = "linux";
                    break;

                case Platform.Mac:
                    plat = "darwin";
                    break;
            }
            #endregion
            if ( psi.EnvironmentVariables.ContainsKey( "GOOS" ) )
                psi.EnvironmentVariables[ "GOOS" ] = plat;
            else
                psi.EnvironmentVariables.Add( "GOOS", plat );
            if ( psi.EnvironmentVariables.ContainsKey( "GOARCH" ) )
                psi.EnvironmentVariables[ "GOARCH" ] = is64 ? "amd64" : "386";
            else
                psi.EnvironmentVariables.Add( "GOARCH", ( is64 ? "amd64" : "386" ) );
            if ( isDll )
                output = "pkg\\" + psi.EnvironmentVariables[ "GOOS" ] + "_" + psi.EnvironmentVariables[ "GOARCH" ] + "\\"
                    + ( path.Contains( "\\" ) ? path.Substring( path.LastIndexOf( '\\' ) + 1 ) : path ) + ".a";
            Process proc = Process.Start( psi );
            proc.WaitForExit( );
            string error = proc.StandardOutput.ReadToEnd( ) + proc.StandardError.ReadToEnd( );
            result = new BuildResult( ( error == "" ? BuildResultType.None : BuildResultType.CompiliingException ) );
            List<CompileError> lce=new List<CompileError>();
            foreach ( string str in error.Split( '\n' ) )
                lce.Add( new CompileError( ) {
                    FullText = str
                } );
            result.Result = lce.ToArray();
            if ( UPX ) {
                psi.FileName = "bin\\upx.exe";
                psi.Arguments = "-q main.exe";
                Process.Start( psi ).WaitForExit( );
            }
            if ( File.Exists( "main.exe" ) )
                File.Move( "main.exe", output );
            return output;
        }
    }
    public class BuildResult {
        /// <summary>
        /// This object can be Parser.CodeErrorException[] ,CompilerError[] ,CompileError[]
        /// </summary>
        public object Result {
            get;
            set;
        }
        public BuildResultType Type {
            get;
            set;
        }
        public BuildResult( BuildResultType Type ) {
            this.Type = Type;
        }
    }
    public class CompileError {
        public string FullText {
            get;
            set;
        }
    }
    public enum BuildResultType {
        InterpretExceptions,
        LinkingExceptions,
        CompiliingException,
        None
    }
}