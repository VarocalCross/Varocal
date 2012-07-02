using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;
using Microsoft.Win32;
using Mono.Cecil;

namespace VarocalCross {
    public class AssemblyResolver :IAssemblyResolver {
        static IAssemblyResolver bar = GlobalAssemblyResolver.Instance;

        public AssemblyResolver( ) {
        }
        public AssemblyDefinition Resolve( string fullName, ReaderParameters parameters ) {
            try {
                return bar.Resolve( fullName, parameters );
            } catch {
                return OurAssemblies[ fullName ];
            }
        }
        public AssemblyDefinition Resolve( string fullName ) {
            try {
                return bar.Resolve( fullName );
            } catch {
                return OurAssemblies[ fullName ];
            }
        }
        public AssemblyDefinition Resolve( AssemblyNameReference name, ReaderParameters parameters ) {
            try {
                return bar.Resolve( name, parameters );
            } catch {
                return OurAssemblies[ name.FullName ];
            }
        }
        public AssemblyDefinition Resolve( AssemblyNameReference name ) {
            try {
                return OurAssemblies[ name.FullName ];
            } catch {
                AssemblyDefinition def;
                OurAssemblies.Add( name.FullName, def=bar.Resolve( name ) );
                return def;
            }
        }
        static public Dictionary<string, AssemblyDefinition> OurAssemblies {
            get;
            set;
        }
        static public AssemblyResolver Resolver = new AssemblyResolver( );
        static Dictionary<string, bool> Paths = new Dictionary<string, bool>( );
        static AssemblyResolver( ) {
            OurAssemblies = new Dictionary<string, AssemblyDefinition>( );
        }
        static public bool Update( string path ) {
            try {
                Paths.Add( path, false );
                AssemblyDefinition def = AssemblyDefinition.ReadAssembly( path, new ReaderParameters( ) {
                    AssemblyResolver = Resolver
                } );
                OurAssemblies.Add( def.FullName, def );
                return true;
            } catch {
                return false;
            }
        }
        static public void Update( string[] paths ) {
            foreach ( var path in paths )
                Update( path );
        }
    }
    class ExecutableAnalied {
        public static List<string> endlines = new List<string>( );
        public static bool TidiedCode = true;
        public AssemblyDefinition Source {
            get;
            set;
        }
        public TypeAnalied[ ] Types {
            get {
                List<TypeAnalied> lta = new List<TypeAnalied>( );
                foreach ( ModuleDefinition md in Source.Modules )
                    foreach ( TypeDefinition type in md.Types )
                        if ( type.isValid( ) )
                            lta.Add( new TypeAnalied( type ) );
                return lta.ToArray( );
            }
        }
        public string Location {
            get;
            protected set;
        }
        public bool isValid {
            get {
                return Source != null;
            }
        }
        public ExecutableAnalied( string filepath ) {
            Location = filepath;
            Source = AssemblyDefinition.ReadAssembly( filepath, new ReaderParameters( ) {
                AssemblyResolver = AssemblyResolver.Resolver
            } );
        }
        public ExecutableAnalied( Assembly source ) {
            Location = source.Location;
            Source = AssemblyDefinition.ReadAssembly( Location, new ReaderParameters( ) {
                AssemblyResolver = AssemblyResolver.Resolver
            } );
        }
        public static Dictionary<string, MethodStatmentAnalied[ ]> Codes = new Dictionary<string, MethodStatmentAnalied[ ]>( );
        public static MethodStatmentAnalied[ ] LoadMethodBody( MethodDefinition mi ) {
            MethodStatmentAnalied[ ] tst;
            //try {
                try {
                    tst = Codes[ mi.FullName ];
                    return tst;
                } catch {
                }
                Codes.Add( mi.FullName, tst = AnalyzeMethod( mi, new DecompilerContext( mi.DeclaringType.Module ) {
                    CurrentMethod = mi,
                    CurrentType = mi.DeclaringType.Resolve( ),
                    CurrentModule = mi.Module,
                    Settings = new DecompilerSettings( ) {

                    }
                } ) );
                return tst;
            //} catch {
                //throw;
                //Codes.Add( mi.FullName, tst = new MethodStatmentAnalied[ ] { } );
                //return tst;
            //}

        }
        public static int AttachedFunction_LeftTypes = 0;
        public static string[ ] unparsable = new string[ ] { "System_Boolean", "System_Byte", "System_Char", "System_DateTime",
            "System_DateTimeOffset", "System_Decimal", "System_Double", "System_Single", "System_Int16", "System_Int32",
            "System_Int64", "System_SByte", "System_String", "System_TimeSpan", "System_UInt16","System_DllFieldAttribute",
            "System_UInt32", "System_UInt64","System_Void","System_Attribute","System_CFunctionAttribute","System_MulticastDelegate" };
        public void RunSources( Func<string, string, bool> invoke, Func<string, bool> torun,bool isdll, string package, List<string> cinclude ,List<String> refs,
            Build.Platform platform,bool is64x,string cmpdir,int ID) {
            if ( cinclude == null )
                cinclude = new List<string>( );
            Codes.Clear( );
            Dictionary<string, bool> dss = new Dictionary<string, bool>( ),refs_check=new Dictionary<string,bool>();
            refs_check.Add( Source.MainModule.FullyQualifiedName, false );
            List<TypeAnalied> lta = new List<TypeAnalied>( );
            lta.AddRange( Types );
            int c = cinclude.Count;
            bool hasMain = false;
            foreach ( string _ref in refs )
                try {
                    //foreach ( string file in Directory.GetFiles( "src\\pkg\\" + Path.GetFileNameWithoutExtension( _ref ), "*.go" ) ) {
                    //    if ( !file.EndsWith( "\\main.go" ) ) {
                    //        StreamReader reader = new StreamReader( file );
                    //        reader.ReadLine( );
                    //        invoke( Path.GetFileNameWithoutExtension( file ), "package main\r\n" + reader.ReadToEnd( ) );
                    //        reader.Close( );
                    //    }
                    //}
                    //foreach ( string file in Directory.GetFiles( "src\\pkg\\" + Path.GetFileNameWithoutExtension( _ref ), "*.c" ) ) {
                    //        File.WriteAllText( Path.Combine( cmpdir, Path.GetFileName( file ) ), File.ReadAllText( file ) );
                    //}
                } catch {
                    MessageBox.Show( "Unable to load reference ID: " + Path.GetFileNameWithoutExtension( _ref ) );
                }
            for ( int i = 0 ; i < c ; i++ ) {
                if ( cinclude[ i ].Contains( "\\" ) )
                    cinclude[ i ] = Path.GetFileName( cinclude[ i ] );
            }
            string code = "package " + package + @"
XX" + ( cinclude.Count != 0 ? "/*#include \"" + string.Join( "\"\r\n#include \"", cinclude.ToArray( ) ) + "\"*/\r\nimport \"C\"" : "" ) + ( isdll ? "" : @"
func main() {
XXXX()
}" ), imports = "";
            for ( ; lta.Count != 0 ; ) {
                TypeAnalied ta = lta[ 0 ];
                lta.RemoveAt( 0 );
                try {
                    dss.Add( ta.Source.FullName, false );
                    if ( ta.FullName.Contains( "]" ) )
                        ta.FullName = ta.FullName.Substring( ta.FullName.LastIndexOf( "]" ) + 1 );

                    bool _continue = false;
                    foreach ( String str in TypeExt.dss.Values )
                        if ( "A_" + str == ta.FullName && str != "System_Object" ) {
                            _continue = true;
                            break;
                        }
                    foreach ( String str in unparsable )
                        if ( "A_" + str == ta.FullName && str != "System_Object" ) {
                            _continue = true;
                            break;
                        }
                    if ( _continue )
                        continue;
                    if ( ta.Inherit != null )
                        if ( ta.Inherit.FullName == "A_System_MulticastDelegate" )
                            continue;
                    try {
                        foreach ( var v in ta.Source.CustomAttributes ) {
                            if ( v.AttributeType.FullName == "System.DllFieldAttribute" ) {
                                if ( ( int )v.ConstructorArguments[ 0 ].Value != ID ) {
                                    try {
                                        refs_check.Add( (( int )v.ConstructorArguments[ 0 ].Value).ToString(), false );
                                        foreach ( string _ref in refs ) {
                                            if ( Path.GetFileNameWithoutExtension( _ref) == ( ( int )v.ConstructorArguments[ 0 ].Value ).ToString( ) ) {
                                                imports += "import . \"" + Path.GetFileNameWithoutExtension( ( ( int )v.ConstructorArguments[ 0 ].Value ).ToString( ) ) + "\"\r\n";
                                                break;
                                            }
                                        }
                                    } catch {
                                    }
                                    ta = null;
                                    break;
                                }
                            }
                        }
                        if ( ta == null )
                            continue;
                    } catch {
                    }
                    TypeAnalied.OptimizeIncludeListGlobal( ref lta );
                    AttachedFunction_LeftTypes = lta.Count;
                    if ( ta.Source.Module.FullyQualifiedName == Source.MainModule.FullyQualifiedName )
                        if ( torun( ta.Source.FullName.GetSimpleType( false, true ) ) ) {
                            if ( !hasMain )
                                foreach ( MethodAnalied ma in ta.Methods )
                                    if ( ma.Name == "A_Main" && ma.isStatic && ma.hasBody ) {
                                        code = code.Insert( code.IndexOf( "XXXX" ) + 4, ta.FullName + "_" + ma.Name ).Remove( code.IndexOf( "XXXX" ), 4 );
                                        hasMain = true;
                                        break;
                                    }
                            if ( !isdll ) {
                                foreach ( var v in ta.Source.CustomAttributes ) {
                                    if ( v.AttributeType.FullName == "System.DllFieldAttribute" ) {
                                        ta = null;
                                        break;
                                    }
                                }
                            }
                            if ( ta == null )
                                continue;
                            code += ta.GoCode;
                            lta.AddRange( ta.Includes );
                        }
                } catch ( Exception ex ) {
                    if ( ex.Message.Contains( "same" ) )
                        continue;
                    string stack = "", line = "";
                    int _in = ex.StackTrace.IndexOf( ") in" ) + 1, _at = ex.StackTrace.IndexOf( "\r\n" ), res = 0;
                    if ( _in > _at )
                        res = _at;
                    else
                        res = _in;
                    try {
                        stack = ex.StackTrace.Remove( res );
                        line = "\r\nLine:\t\t" + ex.StackTrace.Substring( ex.StackTrace.IndexOf( ":line" ) + 6, ex.StackTrace.IndexOf( "\r\n" ) - ex.StackTrace.IndexOf( ":line" ) - 6 );
                    } catch {
                    }
                    stack = stack.Substring( stack.IndexOf( "at" ) + 3 );
                    Console.WriteLine(
                        "Type:\t\t" + ta.FullName
                        + "\r\nFunction:\t" + stack
                        + line
                        + "\r\nMessage:\t" + ex.Message );
                    Console.WriteLine( );
                }
            }
            code = code.Insert( code.IndexOf( "XX" ) + 2, imports ).Remove( code.IndexOf( "XX" ), 2 );
            invoke( "main", code );
            return;
        }
        public static MethodStatmentAnalied[ ] AnalyzeMethod( MethodDefinition methodDef, DecompilerContext context ) {
            MethodDefinition currentMethod = context.CurrentMethod;
            context.CurrentMethod = methodDef;
            context.CurrentMethodIsAsync = false;
            PrivateMemory pri = new PrivateMemory( new AstMethodBodyBuilder( ) );
            pri[ "methodDef" ] = methodDef;
            pri[ "context" ] = context;
            pri[ "typeSystem" ] = methodDef.Module.TypeSystem;
            return AnalyzeInternal( pri, null ).GetStatements( methodDef );
        }
        public static BlockStatement AnalyzeInternal( PrivateMemory pri, IEnumerable<ParameterDeclaration> parameters ) {
            MethodDefinition methodDef = pri[ "methodDef" ];
            DecompilerContext context = pri[ "context" ];
            if ( methodDef.Body == null )
                return null;
            ILBlock iLBlock = new ILBlock( new ILNode[ 0 ] );
            iLBlock.Body = new ILAstBuilder( ).Build( methodDef, true, context );
            new ILAstOptimizer( ).Optimize( context, iLBlock, ILAstOptimizationStep.None );
            BlockStatement blockStatement = new BlockStatement( );

            return (BlockStatement)pri.Invoke( "TransformBlock", new object[ ] { iLBlock } );
        }
        #region Other
        public struct MemoryStatus {
            public uint Length;
            public uint MemoryLoad;
            public uint TotalPhysical;
            public uint AvailablePhysical;
            public uint TotalPageFile;
            public uint AvailablePageFile;
            public uint TotalVirtual;
            public uint AvailableVirtual;
        }
        [DllImport( "kernel32.dll" )]
        public static extern void GlobalMemoryStatus( out MemoryStatus stat );
        static long TotalMemory( ) {
            MemoryStatus stat = new MemoryStatus( );
            GlobalMemoryStatus( out stat );
            return ( long )stat.TotalPhysical;
        }
        #endregion
    }
    class TypeAnalied {
        public TypeDefinition Source {
            get;
            set;
        }
        public TypeAnalied LastParent {
            get {
                try {
                    if ( Source.DeclaringType == null )
                        return this;
                } catch {
                    return null;
                }
                return Parent.LastParent;
            }
        }
        public string FullName {
            get;
            set;
        }
        public string Name {
            get;
            set;
        }
        public string NameWithArguments {
            get {
                if ( Source.GenericParameters.Count == 0 )
                    return Name;
                string res = Name + "<";
                foreach ( string parameter in GenericParameters )
                    res += ( res.EndsWith( "<" ) ? "" : ", " ) + parameter;
                res += ">";
                return res;
            }
        }
        public string[ ] GenericParameters {
            get {
                List<string> ls = new List<string>( );
                try {
                    foreach ( GenericParameter parameter in Source.GenericParameters )
                        ls.Add( parameter.Name );
                } catch {
                }
                return ls.ToArray( );
            }
        }
        public bool isValid {
            get {
                return Source != null;
            }
        }
        public TypeAnalied Inherit {
            get {
                if ( Source.BaseType == null )
                    return null;
                return Source.BaseType.OwnResolve( );
            }
        }
        public TypeAnalied[ ] InheritedInterfaces {
            get {
                List<TypeAnalied> lta = new List<TypeAnalied>( );
                foreach ( TypeReference type in Source.Interfaces )
                    try {
                        if ( !type.FullName.Contains( "+" ) )
                            lta.Add( type.OwnResolve( ) );
                    } catch {
                    }
                return lta.ToArray( );
            }
        }
        public TypeAnalied[ ] Types {
            get {
                List<TypeAnalied> lta = new List<TypeAnalied>( );
                foreach ( TypeReference type in Source.NestedTypes )
                    try {
                        if ( !type.FullName.Contains( "+" ) && type.Name != "<Module>" && type.Name != "" ) {
                            lta.Add( type.OwnResolve( ) );
                        }
                    } catch {
                    }
                return lta.ToArray( );
            }
        }
        public MethodAnalied[ ] Methods {
            get {
                MethodDefinition[ ] mda = GetMethods( Source );
                List<MethodAnalied> lma = new List<MethodAnalied>( );
                foreach ( MethodDefinition md in mda )
                    lma.Add( new MethodAnalied( md ) );
                return lma.ToArray( );
            }
        }
        public VariableAnalied[ ] Variables {
            get {
                FieldDefinition[ ] mda = GetVariables( Source );
                List<VariableAnalied> lma = new List<VariableAnalied>( );
                foreach ( FieldDefinition md in mda )
                    lma.Add( new VariableAnalied( md ) );
                return lma.ToArray( );
            }
        }
        public TypeAnalied Parent {
            get {
                try {
                    if ( Source.DeclaringType == null )
                        return null;
                    return new TypeAnalied( Source.DeclaringType );
                } catch {
                    return null;
                }
            }
        }
        public TypeAnalied[ ] Parents {
            get {
                List<TypeAnalied> lta = new List<TypeAnalied>( );
                TypeAnalied ta = this;
                while ( ta.Parent != null ) {
                    lta.Add( ta = ta.Parent );
                }
                return lta.ToArray( );
            }
        }
        public TypeAnalied[ ] ParentsWithSelf {
            get {
                List<TypeAnalied> lta = new List<TypeAnalied>( new TypeAnalied[ ] { this } );
                TypeAnalied ta = this;
                while ( ta.Parent != null ) {
                    lta.Add( ta = ta.Parent );
                }
                return lta.ToArray( );
            }
        }
        public string[ ][ ] AllGenericParameters {
            get {
                List<string[ ]> lsa = new List<string[ ]>( );
                if ( GenericParameters.Length != 0 )
                    lsa.Add( GenericParameters );
                TypeAnalied type = this;
                while ( ( type = type.Parent ) != null )
                    if ( type.GenericParameters.Length != 0 )
                        lsa.Add( type.GenericParameters );
                return lsa.ToArray( );
            }
        }
        public bool isClass {
            get {
                return Source.IsClass;
            }
        }
        public bool isInterface {
            get {
                return Source.IsInterface;
            }
        }
        public bool isEnum {
            get {
                return Source.IsEnum;
            }
        }
        public static FieldDefinition[ ] GetVariables( TypeDefinition type ) {
            List<FieldDefinition> lmd = new List<FieldDefinition>( );
            lmd.AddRange( type.Fields );
            return lmd.ToArray( );
        }
        public static MethodDefinition[ ] GetMethods( TypeDefinition type ) {
            List<MethodDefinition> lmd = new List<MethodDefinition>( );
            lmd.AddRange( type.Methods );
            return lmd.ToArray( );
        }
        TypeAnalied[ ] __INCLUDES;
        [DefaultValue( true )]
        public static bool IncludeCode {
            get;
            set;
        }
        public string Namespace {
            get {
                return LastParent.Source.Namespace;
            }
        }
        public string GoCode {
            get {
                DateTime dt = DateTime.Now;
                string __HEADER = "", __HEADER2 = "", __SOURCE = "";
                List<TypeAnalied> includes = new List<TypeAnalied>( );
                GetGoCode( this, ref includes, ref __HEADER, ref __SOURCE );
                OptimizeIncludeList( ref includes );
                __HEADER2 += "/*\r\nType's Name: " + Source.FullName + "\r\nTime to Parse: " + ( DateTime.Now - dt ).TotalMilliseconds + "ms\r\n*/\r\n\r\n";
                foreach ( TypeAnalied include in includes ) {
                    bool _continue = false;
                    foreach ( string str in ExecutableAnalied.unparsable )
                        if ( include.FullName == str ) {
                            _continue = true;
                        }
                    if ( _continue )
                        continue;
                    if ( IncludeCode )
                        __HEADER2 += "import . \"" + include.Source.FullName.GetSimpleType( ).Replace( '.', '_' ) + "\"\r\n";
                }
                List<TypeAnalied> includes_sum = new List<TypeAnalied>( );
                includes_sum.AddRange( includes );
                includes_sum.AddRange( Types );
                __INCLUDES = includes_sum.ToArray( );
                __HEADER = ( IncludeCode ? "package " + FullName.Replace( '.', '_' ) + "\r\n" : "" ) + __HEADER2 + __HEADER;
                if ( ExecutableAnalied.TidiedCode ) {
                    __HEADER = __HEADER.TidyCode( );
                    __SOURCE = __SOURCE.TidyCode( );
                }
                return __HEADER + "\r\n\r\n" + __SOURCE;
            }
        }
        public List<string> PasteParameters {
            get;
            set;
        }
        public void OptimizeIncludeList( ref List<TypeAnalied> includes ) {
            int c = includes.Count;
            for ( int i = 0 ; i < c ; i++ ) {
                if ( includes[ i ] == null ) {
                    includes.RemoveAt( i );
                    i--;
                    c--;
                    continue;
                }
                for ( int i2 = i + 1 ; i2 < c ; i2++ ) {
                    if ( includes[ i2 ] == null ) {
                        includes.RemoveAt( i2 );
                        i2--;
                        c--;
                        continue;
                    }
                    if ( includes[ i ].FullName == includes[ i2 ].FullName || includes[ i2 ].FullName == FullName || includes[ i2 ].FullName == "" || !includes[ i2 ].isValid ) {
                        includes.RemoveAt( i2 );
                        i2--;
                        c--;
                    }
                }
            }
        }
        public static void OptimizeIncludeListGlobal( ref List<TypeAnalied> includes ) {
            int c = includes.Count;
            for ( int i = 0 ; i < c ; i++ ) {
                if ( includes[ i ] == null ) {
                    includes.RemoveAt( i );
                    i--;
                    c--;
                    continue;
                }
                for ( int i2 = i + 1 ; i2 < c ; i2++ ) {
                    if ( includes[ i2 ] == null ) {
                        includes.RemoveAt( i2 );
                        i2--;
                        c--;
                        continue;
                    }
                    if ( includes[ i ].FullName == includes[ i2 ].FullName || includes[ i2 ].FullName == "" || !includes[ i2 ].isValid ) {
                        includes.RemoveAt( i2 );
                        i2--;
                        c--;
                    }
                }
            }
        }
        public TypeAnalied[ ] Includes {
            get {
                if ( __INCLUDES == null ) {
                    string test = GoCode;
                }
                return __INCLUDES;
            }
        }
        static void GetGoCode( TypeAnalied ta, ref List<TypeAnalied> includes, ref string header, ref string source ) {
            if ( ta == null )
                return;
            if ( !ta.isValid )
                return;
            //if ( ta.GenericParameters.Length != 0 ) {
            //    header += "template<";
            //    foreach ( string param in ta.GenericParameters )
            //        header += ( header.EndsWith( "<" ) ? "" : ", " ) + "classname " + param.BetterIdentifier( );
            //    header += ">\r\n";
            //}
            header += "type " + ta.FullName;//.Replace( ".", "_" );
            if ( ta.isClass || ta.isInterface )
                header += " struct ";
            else if ( ta.isEnum )
                header += " enum ";
            header += " {\r\n";
            TypeAnalied[ ] types = ta.Types;
            VariableAnalied[ ] variables = ta.Variables;
            MethodAnalied[ ] methods = ta.Methods;
            if ( ta.Inherit != null && ta.FullName != "System_Object" ) {
                header += "A__inherit_object_ " + ta.Inherit.FullName + "\r\n";
                includes.Add( ta.Inherit );
            }
            if ( variables.Length != 0 ) {
                foreach ( VariableAnalied variable in variables ) {
                    string tst = "A_" + variable.Name.GetSimpleType( false, true ) + " " + variable.Type.FullName + "\r\n";
                    if ( variable.isStatic )
                        source += "var " + tst;
                    else
                        header += tst;
                    includes.Add( variable.Type );
                }
            }

            if ( methods.Length != 0 ) {
                foreach ( MethodAnalied method in methods ) {
                    if ( method.Source.Body.MaxStackSize == 0 )
                        continue;
                    string typename = ta.FullName.Replace( '.', '_' ), methodname = method.FullName.Replace( '.', '_' ).GetSimpleType( false, false );
                    source += "func " + ( method.isStatic || method.isConstractor ? "" : "(this *" + typename + ") " )
                        + methodname + "( ";
                    if ( method.ReturnType.FullName != "void" )
                        includes.Add( method.ReturnType );
                    string parameters = "";
                    foreach ( ParameterAnalied parameter in method.Parameters ) {
                        parameters += ( parameters == "" ? "" : ", " ) + parameter.Name + " " + parameter.Type.FullName;
                        includes.Add( parameter.Type );
                    }
                    source += parameters + ( source.EndsWith( " " ) ? ")" : " )" ) +
                        ( method.isConstractor ? " " + "*" + typename : ( method.ReturnType.FullName == "void" ? "" : " " +
                        method.ReturnType.FullName ) ) + " {\r\n";
                    if ( method.hasBody && !ta.isInterface ) {
                        //source += "\r\n";
                        //foreach ( string[ ] array in ta.AllGenericParameters ) {
                        //    source += "template<";
                        //    foreach ( string param in array )
                        //        source += ( source.EndsWith( "<" ) ? "" : ", " ) + "classname " + param.BetterIdentifier( );
                        //    source += "> ";
                        //}
                        source += ( method.isConstractor ? "\r\nthis:=new(" + typename + ")\r\n" : "" ) + method.GoCode +
                            ( method.isConstractor ? "\r\nreturn this" : "" ) + "\r\n}\r\n";
                        includes.AddRange( method.Includes );
                        if ( method.isConstractor ) {
                            source += "func (this *" + typename + ") A__ctor"
                                + "( " + parameters + ( source.EndsWith( " " ) ? ")" : " )" ) +
                                " {\r\n" + method.GoCode + "\r\n}\r\n";
                        }
                    }
                }
            }
            if ( header.EndsWith( "\r\n" ) )
                header = header.Remove( header.Length - 2 );
            header += "\r\n}\r\n";
        }
        public TypeAnalied( TypeDefinition source ) {
            PasteParameters = new List<string>( );
            Source = source;
            if ( isValid ) {
                FullName = Source.FullName.GetSimpleType( false );
                if ( FullName.Contains( "List" ) )
                    Debugger.Break( );
                bool _dontdothat = false;
                foreach ( var v in TypeExt.dss.Values ) {
                    if ( ( " ]" + v ).EndsWith( FullName ) || FullName == v ) {
                        _dontdothat = true;
                        break;
                    }
                }
                if ( !_dontdothat )
                    if ( FullName.StartsWith( "[" ) && !FullName.EndsWith( ")" ) ) {
                        FullName = FullName.Insert( FullName.LastIndexOf( "]" ) + 1, "A_" );
                    } else if ( FullName.EndsWith( ")" ) ) {
                    } else
                        FullName = "A_" + FullName;
                if ( !Source.FullName.Contains( '.' ) ) {
                    Name = FullName;
                } else {
                    Name = FullName.Substring( FullName.LastIndexOf( '.' ) + 1 );
                }
            }
        }
        static T Convert<T>( object X ) {
            return ( T )X;
        }
        static bool Castable<T>( object X ) {
            return X.GetType( ) == typeof( T );
        }
        public override string ToString( ) {
            return ( isClass ? "class " : ( isInterface ? "interface " : "enum " ) ) + " " + FullName;
        }
        public override bool Equals( object obj ) {
            if ( obj is TypeAnalied )
                return this == ( ( TypeAnalied )obj );
            return base.Equals( obj );
        }
        public override int GetHashCode( ) {
            return base.GetHashCode( );
        }
        static void PrintIt( object obj ) {
            Type type = obj.GetType( );
            foreach ( PropertyInfo pi in type.GetProperties( ) ) {
                if ( pi.PropertyType.Name == "Boolean" )
                    if ( ( bool )pi.GetGetMethod( ).Invoke( obj, new object[ ] { } ) )
                        Console.WriteLine( pi.Name );
            }
            foreach ( FieldInfo fi in type.GetFields( ) ) {
                if ( fi.FieldType.Name == "Boolean" )
                    if ( ( bool )fi.GetValue( obj ) )
                        Console.WriteLine( fi.Name );
            }
        }
    }
    class MethodStatmentAnalied {
        public AstNode Source {
            get;
            set;
        }
        public MethodAnalied Parent {
            get;
            protected set;
        }
        public string StatementType {
            get {
                return ( ( dynamic )Source ).FullName.Substring( Source.GetType( ).FullName.LastIndexOf( '.' ) + 1 );
            }
        }
        public string Name {
            get {
                return ( string )Source.GetType( ).GetProperty( "Name" ).GetGetMethod( ).Invoke( Source, new object[ ] { } );
            }
        }
        public string Type {
            get {
                return ( ( AstType )Source.GetType( ).GetProperty( "Type" ).GetGetMethod( ).Invoke( Source, new object[ ] { } ) ).ToString( );
            }
        }
        public string[ ] Variables {
            get {
                List<string> vars = new List<string>( );
                foreach ( VariableInitializer node in ( AstNodeCollection<VariableInitializer> )Source.GetType( ).GetProperty( "Variables" ).GetGetMethod( ).Invoke( Source, new object[ ] { } ) )
                    vars.Add( node.Name );
                return vars.ToArray( );
            }
        }
        public MethodStatmentAnalied[ ] Statements {
            get {
                List<MethodStatmentAnalied> lmbna = new List<MethodStatmentAnalied>( );
                foreach ( AstNode node in Source.Children )
                    lmbna.Add( new MethodStatmentAnalied( node, null ) );
                return lmbna.ToArray( );
            }
        }
        public MethodStatmentAnalied( AstNode source, MethodAnalied method ) {
            Source = source;
            Parent = method;
        }
        public string CSharpCode {
            get {
                return Source.ToString( );
            }
        }
        public string GoCode {
            get {
                string tst = GetGoCode( Source );
                if ( tst.StartsWith( "this.A__inherit_object_" ) && Parent.Parent.FullName == "System_Object" )
                    tst = "//" + tst;
                return tst;
            }
        }
        Dictionary<string, TypeAnalied> dsta = new Dictionary<string, TypeAnalied>( );
        public TypeAnalied[ ] Includes {
            get {
                return dsta.Values.ToArray( );
            }
        }
        public string GetGoCode( AstNode node ) {
            #region SimpleType
            if ( Castable<SimpleType>( node ) ) {
                SimpleType st = Convert<SimpleType>( node );
                string res = "";
                if ( st.Annotations.Count( ) == 1 ) {
                    TypeReference tr = Convert<TypeReference>( st.Annotations.First( ) );
                    TypeAnalied ta = tr.OwnResolve( );
                    try {
                        if ( st.TypeArguments.Count != 0 ) {
                            IEnumerator<AstType> iest = st.TypeArguments.GetEnumerator( );
                            iest.MoveNext( );
                            ta.FullName += ta.GenericParameters.Length + "__" + GetGoCode( iest.Current );
                            while ( iest.MoveNext( ) ) {
                                string tst;
                                ta.FullName += "__" + (tst = GetGoCode( iest.Current ));
                                ta.PasteParameters.Add( tst );
                            }
                            ta.FullName += "__";
                        }
                        dsta.Add( ta.FullName, ta );
                    } catch {

                    }
                    res += ta.FullName.GetSimpleType( );
                } else {
                    TypeAnalied[ ] types = Parent.Parent.ParentsWithSelf;
                    foreach ( TypeAnalied type in types )
                        foreach ( string gene in type.GenericParameters )
                            if ( gene == st.Identifier )
                                Debugger.Break( );
                    res += st.Identifier.BetterIdentifier( );
                }
                if ( st.TypeArguments.Count != 0 ) {
                    IEnumerator<AstType> iest = st.TypeArguments.GetEnumerator( );
                    iest.MoveNext( );
                    res += "<" + GetGoCode( iest.Current );
                    while ( iest.MoveNext( ) )
                        res += ", " + GetGoCode( iest.Current );
                    res += ">";
                }
                return res;
            }
            #endregion
            #region VariableInitializer
 else if ( Castable<VariableInitializer>( node ) ) {
                return Convert<VariableInitializer>( node ).Name.BetterIdentifier( );
            }
            #endregion
            #region Identifier
 else if ( Castable<Identifier>( node ) ) {
                return Convert<Identifier>( node ).Name.BetterIdentifier( );
            }
            #endregion
            #region IdentifierExpression
 else if ( Castable<IdentifierExpression>( node ) ) {
                IdentifierExpression ie = Convert<IdentifierExpression>( node );
                if ( ie.Annotations.Count( ) != 0 ) {
                    Object obj = ie.Annotations.First( );
                    if ( typeof( MethodReference ).IsAssignableFrom( obj.GetType( ) ) )
                        return new MethodAnalied( ( ( MethodReference )obj ).Resolve( ) ).FullName.GetSimpleType( false, true );
                }
                return ie.Identifier.BetterIdentifier( );
            }
            #endregion
            #region ObjectCreateExpression
 else if ( Castable<ObjectCreateExpression>( node ) ) {
                ObjectCreateExpression oce = Convert<ObjectCreateExpression>( node );
                string res = GetGoCode( oce.Type );
                if ( res.StartsWith( "*" ) )
                    res = res.Substring( 1 );
                res += ( res.EndsWith( ")" ) ? "" : "_A__ctor" ) + "( ";
                foreach ( Expression expr in oce.Arguments )
                    res += ( res.EndsWith( "( " ) ? "" : ", " ) + GetGoCode( expr );
                if ( oce.Initializer.Elements.Count != 0 )
                    res = "[...]" + GetGoCode( oce.Type ) + GetGoCode( oce.Initializer );
                return res + ( res.EndsWith( " " ) ? ")" : " )" );
            }
            #endregion
            #region TypeReferenceExpression
 else if ( Castable<TypeReferenceExpression>( node ) ) {
                return GetGoCode( Convert<TypeReferenceExpression>( node ).Type );
            }
            #endregion
            #region PrimitiveType
 else if ( Castable<PrimitiveType>( node ) ) {
                PrimitiveType pt = Convert<PrimitiveType>( node );
                if ( pt.Keyword == "object" )
                    return "System_Object";
                return pt.Keyword.GetSimpleType( );
            }
            #endregion
            #region MemberReferenceExpression
 else if ( Castable<MemberReferenceExpression>( node ) ) {
                MemberReferenceExpression mre = Convert<MemberReferenceExpression>( node );
                string res = GetGoCode( mre.Target );
                if ( res.StartsWith( "*" ) )
                    res = res.Substring( 1 );
                if ( mre.TypeArguments.Count != 0 ) {
                    IEnumerator<AstType> iest = mre.TypeArguments.GetEnumerator( );
                    iest.MoveNext( );
                    res += "<" + GetGoCode( iest.Current );
                    while ( iest.MoveNext( ) )
                        res += ", " + GetGoCode( iest.Current );
                    res += ">";
                }
                string target = "A_" + mre.MemberName.BetterIdentifier( );
                try {
                    if ( mre.Parent.Annotations.First( ) is PropertyReference ) {
                        bool isSet = mre.Parent.Ancestors.First( ) is AssignmentExpression;
                        if ( isSet )
                            isSet = ( ( AssignmentExpression )mre.Parent.Ancestors.First( ) ).Left == mre.Parent;
                        target = ( isSet ? "set_" : "get_" ) + target + "(" + ( isSet ? "" : ")" );
                    } else if ( mre.Parent.Annotations.First( ) is MethodReference ) {
                        if ( mre.Annotations.Count( ) != 0 )
                            if ( mre.Annotations.First( ) is FieldReference || mre.Annotations.First( ) is PropertyReference )
                                throw new Exception( "" );
                        MethodAnalied ma = new MethodAnalied( ( ( MethodReference )mre.Parent.Annotations.First( ) ).Resolve( ) );
                        if ( ma.Source.Body.MaxStackSize == 0 ) {
                            if ( ma.Source.CustomAttributes.Count > 0 )
                                res = ( ma.Source.CustomAttributes[ 0 ].AttributeType.FullName
                                    == "System.CFunctionAttribute" ? "C." : "" ) + ma.Source.Name.BetterIdentifier( );
                            else
                                res = ma.Source.Name.BetterIdentifier( );
                            target = "";
                        } else
                            target += ma.CombinedParameters;
                    }
                } catch {
                }
                return res + ( target == "" ? "" : ( mre.Target is TypeReferenceExpression ? "_" : "." ) + target );
            }
            #endregion
            #region InvocationExpression
 else if ( Castable<InvocationExpression>( node ) ) {
                InvocationExpression ie = Convert<InvocationExpression>( node );
                if ( Castable<IdentifierExpression>( ie.Target ) )
                    if ( Convert<IdentifierExpression>( ie.Target ).Identifier == "ldftn" ) {
                        return GetGoCode( ie.Arguments.First( ) );
                    }
                if ( ie.Annotations.First( ) is MethodReference ) {
                    MethodAnalied ma = new MethodAnalied( ( ( MethodReference )ie.Annotations.First( ) ).Resolve( ) );
                    if ( ma.Parent.FullName == "A_System_LanguageFunctions" && ma.Source.Name == "Split" ) {
                        try {
                            string first = GetGoCode( ie.Arguments.First( ).Children.ElementAt( 1 ) );
                            if ( !( ie.Arguments.First( ).Children.ElementAt( 1 ) is NullReferenceExpression ) )
                                return "go " + first + "." + GetGoCode( ie.Arguments.First( ).Children.ElementAt( 2 ) ) + "()";
                        } catch {
                        }
                        return "go " + GetGoCode( ie.Arguments.First( ).Children.ElementAt( 2 ) ) + "()";
                    } else if ( ma.Parent.FullName == "A_System_LanguageFunctions" && ma.Source.Name == "Finally" ) {
                        return "defer " + GetGoCode( ie.Arguments.First( ).Children.ElementAt( 2 ) ) + "()";
                    } else if ( ma.Parent.FullName == "string" && ma.Source.Name == "Concat" ) {
                        return GetGoCode( ie.Arguments.First( ) ) + " + " + GetGoCode( ie.Arguments.ElementAt( 1 ) );
                    }
                }
                string res = GetGoCode( ie.Target ) + "( ";
                foreach ( Expression expr in ie.Arguments )
                    res += ( res.EndsWith( "( " ) ? "" : ", " ) + GetGoCode( expr );
                return res + ( res.EndsWith( " " ) ? ")" : " )" );
            }
            #endregion
            #region AssignmentExpression
            else if ( Castable<AssignmentExpression>( node ) ) {
                AssignmentExpression ae = Convert<AssignmentExpression>( node );
                string additional = "";
                bool isSet = false;
                if ( ae.Left is MemberReferenceExpression ) {
                    MemberReferenceExpression mre = ( MemberReferenceExpression )ae.Left;
                    try {
                        if ( mre.Annotations.First( ) is PropertyReference ) {
                            if ( isSet = mre.Ancestors.First( ) is AssignmentExpression )
                                additional += " )";
                        }
                    } catch {
                    }
                }
                string left = GetGoCode( ae.Left ) + ( isSet ? "" : " " ), right = GetGoCode( ae.Right ), opera = AssignmentExpression.GetOperatorRole( ae.Operator ).Token;
                if ( isSet )
                    opera = ( opera.Length == 1 ? "" : left.Remove( left.LastIndexOf( "set" ), 3 ).Insert( left.LastIndexOf( "set" ), "get" ) );
                if ( ae.Left.Annotations.First( ) is ILVariable ) {
                    try {
                        Parent.vars.Add( left, false );
                        opera = ":=";
                    } catch {
                    }
                }
                return left + opera + " " + right + additional;
            }
            #endregion
            #region VariableDeclarationStatement
 else if ( Castable<VariableDeclarationStatement>( node ) ) {
                return "";
            }
            #endregion
            #region ExpressionStatement
 else if ( Castable<ExpressionStatement>( node ) ) {
                return GetGoCode( Convert<ExpressionStatement>( node ).Children.First( ) );
            }
            #endregion
            #region IfElseStatement
 else if ( Castable<IfElseStatement>( node ) ) {
                IfElseStatement ies = Convert<IfElseStatement>( node );
                return "if ( " + GetGoCode( ies.Condition ) + " ) {\r\n" + GetGoCode( ies.TrueStatement ) +
                    "\r\n}" + ( ies.FalseStatement.Children.Count( ) == 0 ? "" : "\r\nelse {\r\n" + GetGoCode( ies.FalseStatement ) + "\r\n}" );
            }
            #endregion
            #region UnaryOperatorExpression
 else if ( Castable<UnaryOperatorExpression>( node ) ) {
                UnaryOperatorExpression uoe = Convert<UnaryOperatorExpression>( node );
                if ( Castable<UnaryOperatorExpression>( uoe.Expression ) ) {
                    UnaryOperatorExpression uoe2 = Convert<UnaryOperatorExpression>( uoe.Expression );
                    if ( uoe.Operator == UnaryOperatorType.Not && uoe.Operator == uoe2.Operator )
                        return GetGoCode( uoe2.Expression );
                }
                if ( Castable<BinaryOperatorExpression>( uoe.Expression ) ) {
                    BinaryOperatorExpression boe = Convert<BinaryOperatorExpression>( uoe.Expression );
                    if ( uoe.Operator == UnaryOperatorType.Not && boe.Operator == BinaryOperatorType.InEquality )
                        return GetGoCode( boe.Left ) + " == " + GetGoCode( boe.Right );
                }
                return UnaryOperatorExpression.GetOperatorRole( uoe.Operator ).Token + GetGoCode( uoe.Expression );
            }
            #endregion
            #region BinaryOperatorExpression
 else if ( Castable<BinaryOperatorExpression>( node ) ) {
                BinaryOperatorExpression boe = Convert<BinaryOperatorExpression>( node );
                return "( " + GetGoCode( boe.Left ) + " " + BinaryOperatorExpression.GetOperatorRole( boe.Operator ).Token + " " + GetGoCode( boe.Right ) + " )";
            }
            #endregion
            #region PrimitiveExpression
 else if ( Castable<PrimitiveExpression>( node ) ) {
                PrimitiveExpression pe = Convert<PrimitiveExpression>( node );
                if ( pe.Value is bool )
                    return ( bool )pe.Value ? "true" : "false";
                bool isString = pe.Value is string || pe.Value is char || pe.Value is char[ ];
                return ( isString ? '"'.ToString( ) : "" ) + pe.Value.ToString( ) + ( isString ? '"'.ToString( ) : "" );
            }
            #endregion
            #region BlockStatement
 else if ( Castable<BlockStatement>( node ) ) {
                MethodStatmentAnalied[ ] msaa = Convert<BlockStatement>( node ).GetStatements( Parent.Source );
                string res = "";
                foreach ( MethodStatmentAnalied nsa in msaa ) {
                    res += ( res == "" ? "" : "\r\n" ) + nsa.GoCode;
                    foreach ( KeyValuePair<string, TypeAnalied> kvp in nsa.dsta )
                        try {
                            dsta.Add( kvp.Key, kvp.Value );
                        } catch {
                        }
                }
                return res;
            }
            #endregion
            #region IndexerExpression
 else if ( Castable<IndexerExpression>( node ) ) {
                IndexerExpression ie = Convert<IndexerExpression>( node );
                string res = GetGoCode( ie.Target ) + "[ ";
                foreach ( var expr in ie.Arguments )
                    res += ( res.EndsWith( "[ " ) ? "" : ", " ) + GetGoCode( expr );
                return res + ( res.EndsWith( " " ) ? "]" : " ]" );
            }
            #endregion
            #region ThisReferenceExpression
 else if ( Castable<ThisReferenceExpression>( node ) ) {
                return "this";
            }
            #endregion
            #region BaseReferenceExpression
 else if ( Castable<BaseReferenceExpression>( node ) ) {
                return "this.A__inherit_object_";
            }
            #endregion
            #region MemberType
 else if ( Castable<MemberType>( node ) ) {
                MemberType mt = Convert<MemberType>( node );
                string add = "";
                if ( mt.TypeArguments.Count != 0 ) {
                    IEnumerator<AstType> iest = mt.TypeArguments.GetEnumerator( );
                    iest.MoveNext( );
                    add = "<" + GetGoCode( iest.Current );
                    while ( iest.MoveNext( ) )
                        add += ", " + GetGoCode( iest.Current );
                    add += ">";
                }
                return GetGoCode( mt.Target ) + add + "_" + mt.MemberName.BetterIdentifier( );
            }
            #endregion
            #region ReturnStatement
 else if ( Castable<ReturnStatement>( node ) ) {
                ReturnStatement rs = Convert<ReturnStatement>( node );
                return "return" + ( rs.Expression.GetType( ).FullName.Contains( "Null" ) ? "" : " " + GetGoCode( rs.Expression ) );
            }
            #endregion
            #region TryCatchStatement
 else if ( Castable<TryCatchStatement>( node ) ) {
                TryCatchStatement tcs = Convert<TryCatchStatement>( node );
                string res = "try {\r\n" + GetGoCode( tcs.TryBlock ) + "\r\n}";
                foreach ( CatchClause cc in tcs.CatchClauses ) {
                    res += "\r\ncatch" + ( !cc.Type.GetType( ).FullName.Contains( "Null" ) ? " ( " + GetGoCode( cc.Type ) + ( cc.VariableName != "" ? " " + cc.VariableName : "" ) + " )" : " (...)" ) + " {\r\n" + GetGoCode( tcs.FinallyBlock ) + "\r\n}";
                }
                return res + "\r\nfinally {\r\n" + GetGoCode( tcs.FinallyBlock ) + "\r\n}";
            }
            #endregion
            #region WhileStatement
 else if ( Castable<WhileStatement>( node ) ) {
                WhileStatement ws = Convert<WhileStatement>( node );
                return "for ( " + GetGoCode( ws.Condition ) + " ) {\r\n" + GetGoCode( ws.EmbeddedStatement ) + "\r\n}";
            }
            #endregion
            #region CastExpression
 else if ( Castable<CastExpression>( node ) ) {
                CastExpression ws = Convert<CastExpression>( node );
                return "( " + GetGoCode( ws.Type ) + " )" + GetGoCode( ws.Expression );
            }
            #endregion
            #region NullReferenceExpression
 else if ( Castable<NullReferenceExpression>( node ) ) {
                return "0";
            }
            #endregion
            #region ComposedType
 else if ( Castable<ComposedType>( node ) ) {
                ComposedType ct = Convert<ComposedType>( node );
                string res = GetGoCode( ct.BaseType );
                foreach ( ArraySpecifier arr in ct.ArraySpecifiers )
                    res += GetGoCode( arr );
                return res;
            }
            #endregion
            #region ArraySpecifier
 else if ( Castable<ArraySpecifier>( node ) ) {
                ArraySpecifier _as = Convert<ArraySpecifier>( node );
                return "[ " + new string( ',', _as.Dimensions - 1 ).Replace( ",", " ," ) + ( _as.Dimensions == 1 ? "]" : " ]" );
            }
            #endregion
            #region BreakStatement
 else if ( Castable<BreakStatement>( node ) ) {
                return "break";
            }
            #endregion
            #region ContinueStatement
 else if ( Castable<ContinueStatement>( node ) ) {
                return "continue";
            }
            #endregion
            #region LabelStatement
 else if ( Castable<LabelStatement>( node ) ) {
                return Convert<LabelStatement>( node ).Label + ":";
            }
            #endregion
            #region GotoStatement
 else if ( Castable<GotoStatement>( node ) ) {
                return "goto " + Convert<GotoStatement>( node ).Label;
            }
            #endregion
            #region ArrayInitializerExpression
 else if ( Castable<ArrayInitializerExpression>( node ) ) {
                ArrayInitializerExpression aie = Convert<ArrayInitializerExpression>( node );
                string res = "";
                foreach ( var n in aie.Elements ) {
                    res += ( res == "" ? "" : ", " ) + GetGoCode( n );
                }
                return "{ " + res + " }";
            }
            #endregion
            #region ArrayCreateExpression
 else if ( Castable<ArrayCreateExpression>( node ) ) {
                ArrayCreateExpression ace = Convert<ArrayCreateExpression>( node );
                string res = "";
                IEnumerator<Expression> iee = ace.Arguments.GetEnumerator( );
                if ( iee.MoveNext( ) ) {
                    res += "[";//+GetGoCode( iee.Current );
                    while ( iee.MoveNext( ) )
                        res += ", ";// + iee.Current;
                    res += ( res.EndsWith( "[" ) ? " ]" : "]" );
                }
                foreach ( ArraySpecifier arr in ace.AdditionalArraySpecifiers ) {
                    res += GetGoCode( arr );
                }
                string tst;
                res += GetGoCode( ace.Type ) + ( tst = GetGoCode( ace.Initializer ) );
                if ( tst == "" )
                    res += "{ }";
                return res;
            }
            #endregion
            #region ConditionalExpression
 else if ( Castable<ConditionalExpression>( node ) ) {
                ConditionalExpression ce = Convert<ConditionalExpression>( node );
                return "(" + GetGoCode( ce.Condition ) + "?" + GetGoCode( ce.TrueExpression ) + ":" + GetGoCode( ce.FalseExpression ) + ")";
            }
            #endregion
            #region AsExpression
 else if ( Castable<AsExpression>( node ) ) {
                AsExpression ae = Convert<AsExpression>( node );
                return "(" + GetGoCode( ae.Type ) + ")" + GetGoCode( ae.Expression );
            }
            #endregion
            #region TypeOfExpression
 else if ( Castable<TypeOfExpression>( node ) ) {
                return "typeof(" + GetGoCode( Convert<TypeOfExpression>( node ).Type ) + ")";
            }
            #endregion
            #region ThrowStatement
 else if ( Castable<ThrowStatement>( node ) ) {
                return "throw " + GetGoCode( Convert<ThrowStatement>( node ).Expression );
            }
            #endregion
            #region SwitchStatement
 else if ( Castable<SwitchStatement>( node ) ) {
                SwitchStatement ss = Convert<SwitchStatement>( node );
                string res = "switch (" + GetGoCode( ss.Expression ) + ") {\r\n";
                foreach ( SwitchSection section in ss.SwitchSections )
                    res += GetGoCode( section );
                return res + "}";
            }
            #endregion
            #region SwitchSection
 else if ( Castable<SwitchSection>( node ) ) {
                SwitchSection ss = Convert<SwitchSection>( node );
                string res = "";
                foreach ( CaseLabel label in ss.CaseLabels )
                    res += "case " + label.Expression + ": {\r\n";
                foreach ( MethodStatmentAnalied stmt in ss.Statements.GetStatements( Parent.Source ) )
                    res += stmt.GoCode + "\r\n";
                return res + "}";
            }
            #endregion
            #region DirectionExpression
 else if ( Castable<DirectionExpression>( node ) ) {
                return GetGoCode( Convert<DirectionExpression>( node ).Expression );
            }
            #endregion
            #region DefaultValueExpression
 else if ( Castable<DefaultValueExpression>( node ) ) {
                return GetGoCode( Convert<DefaultValueExpression>( node ).Type ) + "()";
            }
            #endregion
            #region SizeOfExpression
 else if ( Castable<SizeOfExpression>( node ) ) {
                SizeOfExpression nae = Convert<SizeOfExpression>( node );
                return "(" + GetGoCode( nae.Type ) + ").SizeOf()";
            }
            #endregion
            #region NamedArgumentExpression
 else if ( Castable<NamedArgumentExpression>( node ) ) {
                NamedArgumentExpression nae = Convert<NamedArgumentExpression>( node );
                return nae.Identifier + " = " + GetGoCode( nae.Expression );
            }
            #endregion
            #region FixedStatement
 else if ( Castable<FixedStatement>( node ) ) {
                FixedStatement fs = Convert<FixedStatement>( node );
                string res = "" + GetGoCode( fs.Type ) + "";
                foreach ( VariableInitializer vi in fs.Variables )
                    res += ( res.EndsWith( " " ) ? ", " : " " ) + GetGoCode( vi );
                return res + " {\r\n" + GetGoCode( fs.EmbeddedStatement ) + "\r\n}";
            }
            #endregion
            #region StackAllocExpression
 else if ( Castable<StackAllocExpression>( node ) ) {
                StackAllocExpression sae = Convert<StackAllocExpression>( node );
                return "new " + GetGoCode( sae.Type ) + "[" + GetGoCode( sae.CountExpression ) + "]";
            }
            #endregion
            #region Unable to generate part
 else {
                #region Filter nulls
                string name = node.GetType( ).FullName;
                if ( name.Contains( "+" ) )
                    if ( name.Substring( name.IndexOf( "+" ) + 1, 4 ) == "Null" )
                        return "";
                #endregion
                if ( Unknowns == null )
                    Unknowns = "";
                if ( node.GetType( ).IsAssignableFrom( typeof( AnonymousTypeCreateExpression ) ) ) {
                    if ( node.Parent.GetType( ).IsAssignableFrom( typeof( ReturnStatement ) ) ) {
                        AnonymousTypeCreateExpression atce = Convert<AnonymousTypeCreateExpression>( node );
                        string ret = Parent.ReturnType.FullName.GetSimpleType( false ) + "( ";
                        foreach ( Expression expr in atce.Initializers )
                            ret += ( ret.EndsWith( "( " ) ? "" : ", " ) + GetGoCode( expr );
                        Console.WriteLine( ret + ( ret.EndsWith( " " ) ? ")" : " )" ) );
                        return ret + ( ret.EndsWith( " " ) ? ")" : " )" );
                    }
                    Console.WriteLine( node.GetType( ).FullName );
                    Console.Write( node.GetType( ).Name + " - " + node.Parent.ToString( ) + " (" + Parent.ReturnType.ToString( ) + ")\r\n" );
                } else {
                    if ( node.GetType( ) == typeof( UndocumentedExpression ) ) {
                        UndocumentedExpression ue = ( UndocumentedExpression )node;
                    }
                    Console.WriteLine( node.GetType( ).FullName );
                    Console.Write( node.GetType( ).Name + " - " + node.Parent.ToString( ) + " (" + Parent.ReturnType.ToString( ) + ")\r\n" );
                }
                string res = "";
                foreach ( var v in node.Children )
                    res += GetGoCode( v );
                return res;
            }
            #endregion
        }
        static T Convert<T>( object X ) {
            return ( T )X;
        }
        static bool Castable<T>( object X ) {
            return X.GetType( ) == typeof( T );
        }
        public override string ToString( ) {
            return StatementType;
        }
        public static string Unknowns {
            get;
            set;
        }
    }
    #region Done
    class MethodAnalied {
        public MethodDefinition Source {
            get;
            set;
        }
        public bool isStatic {
            get {
                return Source.IsStatic;
            }
        }
        public string Name {
            get {
                return "A_" + Source.Name.BetterIdentifier( ) + CombinedParameters;
            }
        }
        public string CombinedParameters {
            get {
                string res = "";
                foreach ( ParameterAnalied pa in Parameters )
                    res += pa.Type.FullName.GetSimpleType( false );
                return res;
            }
        }
        public string FullName {
            get {
                return Parent.FullName + "." + Name;
            }
        }
        public bool isValid {
            get {
                return Source != null;
            }
        }
        public string FullNameWithArguments {
            get {
                TypeAnalied stop_type = Parent.LastParent, current_type = Parent;
                string res = Name, res2;
                while ( stop_type.FullName != current_type.FullName ) {
                    if ( current_type.GenericParameters.Length == 0 ) {
                        res = current_type.Name + "." + res;
                        current_type = current_type.Parent;
                        continue;
                    }
                    res2 = current_type.Name + "<";
                    foreach ( string parameter in current_type.GenericParameters )
                        res2 += ( res2.EndsWith( "<" ) ? "" : ", " ) + parameter;
                    res2 += ">";
                    res = res2 + "." + res;
                    current_type = current_type.Parent;
                }
                res2 = current_type.Name;
                if ( current_type.GenericParameters.Length != 0 ) {
                    res2 += "<";
                    foreach ( string parameter in current_type.GenericParameters )
                        res2 += ( res2.EndsWith( "<" ) ? "" : ", " ) + parameter;
                    res2 += ">";
                }
                return ( ( stop_type.Namespace != "" ? stop_type.Namespace + "." : "" ) + res2 + "." + res ).GetSimpleType( false );
            }
        }
        public MethodStatmentAnalied[ ] Statements {
            get {
                return ExecutableAnalied.LoadMethodBody( Source );
            }
        }
        public ParameterAnalied[ ] Parameters {
            get {
                ParameterDefinition[ ] pda = Source.Parameters.ToArray( );
                List<ParameterAnalied> lpa = new List<ParameterAnalied>( );
                foreach ( ParameterDefinition pa in pda )
                    lpa.Add( new ParameterAnalied( pa ) );
                return lpa.ToArray( );
            }
        }
        TypeAnalied[ ] __INCLUDES;
        public bool isAbstract {
            get {
                return Source.IsAbstract;
            }
        }
        public bool isInvoke {
            get {
                return Source.HasPInvokeInfo || Source.IsPInvokeImpl;
            }
        }
        public bool isRuntime {
            get {
                return Source.IsRuntime;
            }
        }
        public bool isVirtual {
            get {
                return Source.IsVirtual;
            }
        }
        public bool IsInternalCall {
            get {
                return Source.IsInternalCall;
            }
        }
        public string GoCode {
            get {
                string code = "";
                List<TypeAnalied> lta = new List<TypeAnalied>( );
                foreach ( MethodStatmentAnalied msa in Statements ) {
                    code += ( code == "" ? "" : "\r\n" ) + msa.GoCode;
                    lta.AddRange( msa.Includes );
                }
                vars.Clear( );
                __INCLUDES = lta.ToArray( );
                return code;
            }
        }
        public bool hasBody {
            get {
                return Source.HasBody;
            }
        }
        public bool isConstractor {
            get {
                return Source.Name == ".ctor";
            }
        }
        public TypeAnalied[ ] Includes {
            get {
                return __INCLUDES;
            }
        }
        public TypeAnalied Parent {
            get {
                return new TypeAnalied( Source.DeclaringType );
            }
        }
        public TypeAnalied ReturnType {
            get {
                return Source.ReturnType.OwnResolve( );
            }
        }
        public MethodAnalied( MethodDefinition source ) {
            Source = source;
        }
        public override string ToString( ) {
            string parameters = "( ";
            foreach ( ParameterAnalied parameter in Parameters )
                parameters += ( parameters == "( " ? "" : ", " ) + parameter.ToString( );
            parameters += ( parameters.EndsWith( " " ) ? ")" : " )" );
            return ReturnType.FullName + " " + Name + parameters;
        }
        internal Dictionary<string, bool> vars = new Dictionary<string, bool>( );
    }
    class ParameterAnalied {
        public ParameterDefinition Source {
            get;
            set;
        }
        public string Name {
            get {
                return Source.Name;
            }
        }
        public TypeAnalied Type {
            get {
                return Source.ParameterType.OwnResolve( );
            }
        }
        public ParameterAnalied( ParameterDefinition source ) {
            Source = source;
        }
        public override string ToString( ) {
            return Type.FullName + " " + Name;
        }
    }
    class VariableAnalied {
        public FieldDefinition Source {
            get;
            set;
        }
        public bool isStatic {
            get {
                return Source.IsStatic;
            }
        }
        public string FullName {
            get {
                return Source.DeclaringType.ToString( ).GetSimpleType( false ) + "." + Name;
            }
        }
        public string Name {
            get {
                return Source.Name.BetterIdentifier( );
            }
        }
        public TypeAnalied Type {
            get {
                return Source.FieldType.OwnResolve( );
            }
        }
        public bool isValid {
            get {
                return Source != null;
            }
        }
        public TypeAnalied Parent {
            get {
                return Source.DeclaringType.OwnResolve( );
            }
        }
        public VariableAnalied( FieldDefinition source ) {
            Source = source;
        }
        public override string ToString( ) {
            return Type.FullName + " " + Name;
        }
    }
    static class TypeExt {
        public static Dictionary<string, string> dss = new Dictionary<string, string>( );
        static TypeExt( ) {
            dss.Add( "boolean", "bool" );
            dss.Add( "bool", "bool" );
            dss.Add( "byte", "int8" );
            dss.Add( "char", "char" );
            dss.Add( "decimal", "system::Decimal" );
            dss.Add( "double", "float64" );
            dss.Add( "float", "float32" );
            dss.Add( "int16", "int16" );
            dss.Add( "short", "int16" );
            dss.Add( "int32", "int" );
            dss.Add( "int", "int" );
            dss.Add( "int64", "int64" );
            dss.Add( "long", "int64" );
            dss.Add( "object", "System_Object" );
            dss.Add( "sbyte", "uint8" );
            dss.Add( "string", "string" );
            dss.Add( "uint16", "uint16" );
            dss.Add( "ushort", "uint16" );
            dss.Add( "uint32", "uint" );
            dss.Add( "uint", "uint" );
            dss.Add( "uint64", "uint64" );
            dss.Add( "ulong", "uint64" );
            dss.Add( "void", "void" );
            dss.Add( "system_boolean", "bool" );
            dss.Add( "system_byte", "int8" );
            dss.Add( "system_char", "char" );
            dss.Add( "system_decimal", "uint64" );
            dss.Add( "system_double", "float64" );
            dss.Add( "system_singel", "float32" );
            dss.Add( "system_int16", "int16" );
            dss.Add( "system_int32", "int" );
            dss.Add( "system_int64", "int64" );
            dss.Add( "system_object", "System_Object" );
            dss.Add( "system_sbyte", "uint8" );
            dss.Add( "system_string", "string" );
            dss.Add( "system_uint16", "uint16" );
            dss.Add( "system_uint32", "uint" );
            dss.Add( "system_uint64", "uint64" );
            dss.Add( "system_void", "void" );
        }
        public static string GetSimpleType( this string t, bool pointer = true,bool nodssuse=false ) {
            try {
                if ( nodssuse )
                    throw new Exception( "" );
                return dss[ t.ToLower( ).Replace( '.', '_' ) ];
            } catch {
                if ( t.StartsWith( "System.Action<" ) ) {
                    t = t.Substring( t.IndexOf( '<' ) + 1 );
                    t = t.Remove( t.Length - 1 );
                    List<string> types = new List<string>( );
                    int i = 0, ind = 0, s = 0;
                    while ( ind < t.Length ) {
                        if ( t[ ind ] == '<' ) {
                            i++;
                        }
                        if ( t[ ind ] == '>' ) {
                            i--;
                        }
                        if ( t[ ind ] == ',' && i == 0 ) {
                            types.Add( t.Substring( s, ind - s - 1 ) );
                            ind++;
                            s = ind;
                            continue;
                        }
                        ind++;
                    }
                    //if ( FullName.StartsWith( "[" ) && !FullName.EndsWith( ")" ) )
                    //    FullName = FullName.Insert( FullName.LastIndexOf( "]" ) + 1, "A_" );
                    //else if ( FullName.EndsWith( ")" ) ) {
                    //} else
                    //    FullName = "A_" + FullName;
                    types.Add( t.Substring( s ) );
                    return "func(" + string.Join( ", ", ( from a in types select a.GetSimpleType( ) ).ToArray( ) ) + ")";
                } else if ( t == "System.Action" ) {
                    return "func()";
                } else if ( t.StartsWith( "System.Func<" ) ) {
                    t = t.Substring( t.IndexOf( '<' ) + 1 );
                    t = t.Remove( t.Length - 1 );
                    List<string> types = new List<string>( );
                    int i = 0, ind = 0, s = 0;
                    while ( ind < t.Length ) {
                        if ( t[ ind ] == '<' ) {
                            i++;
                        }
                        if ( t[ ind ] == '>' ) {
                            i--;
                        }
                        if ( t[ ind ] == ',' && i == 0 ) {
                            types.Add( t.Substring( s, ind - s - 1 ) );
                            ind++;
                            s = ind;
                            continue;
                        }
                        ind++;
                    }
                    return "func(" + string.Join( ", ", ( from a in types select a.GetSimpleType( ) ).ToArray( ) ) + ") " + t.Substring( s ).GetSimpleType( );
                }
                if ( t == null )
                    return "";
                if ( t.Contains( "," ) )
                    t = t.Remove( t.IndexOf( "," ) );
                int c = t.Length;
                t = t.Replace( "/", "." ).Replace( "+", "." );
                for ( int i = 0 ; i < c ; i++ )
                    if ( !( t[ i ] >= 'A' && t[ i ] <= 'Z' )
                        && !( t[ i ] >= 'a' && t[ i ] <= 'z' )
                        && !( t[ i ] >= '0' && t[ i ] <= '9' )
                        && t[ i ] != '_'
                        && t[ i ] != '.'
                        && t[ i ] != '['
                        && t[ i ] != ']' )
                        t = t.Remove( i, 1 ).Insert( i, "_" );
                t = t.Replace( '.', '_' );
                return ( pointer ? "*" : "" ) + t;
            }
        }
        public static MethodStatmentAnalied[ ] GetStatements( this BlockStatement _this, MethodDefinition md ) {
            if ( _this == null )
                return new MethodStatmentAnalied[ 0 ];
            List<MethodStatmentAnalied> lmbna = new List<MethodStatmentAnalied>( _this.Statements.Count );
            MethodAnalied md2 = new MethodAnalied( md );
            foreach ( AstNode node in _this.Statements )
                lmbna.Add( new MethodStatmentAnalied( node, md2 ) );
            return lmbna.ToArray( );
        }
        public static MethodStatmentAnalied[ ] GetStatements( this AstNodeCollection<Statement> _this, MethodDefinition md ) {
            if ( _this == null )
                return new MethodStatmentAnalied[ 0 ];
            List<MethodStatmentAnalied> lmbna = new List<MethodStatmentAnalied>( _this.Count );
            MethodAnalied md2 = new MethodAnalied( md );
            foreach ( AstNode node in _this )
                lmbna.Add( new MethodStatmentAnalied( node, md2 ) );
            return lmbna.ToArray( );
        }
        public static List<string> Split( string source, char x ) {
            List<string> test = new List<string>( );
            int index = -1, last_index = 0;
            while ( ( index = source.IndexOf( x, index + 1 ) ) != -1 ) {
                test.Add( source.Substring( last_index, index - last_index ) );
                last_index = index + 1;
            }
            test.Add( source.Substring( last_index ) );
            return test;
        }
        public static int IndexOfChar( string source, char dest, int StartIndex = 0 ) {
            int c = source.Length;
            for ( int i = StartIndex ; i < c ; i++ )
                if ( dest == source[ i ] )
                    return i;
            return -1;
        }
        public static int Count( this string source, char dest ) {
            int count = 0, index = -1;
            while ( ( index = IndexOfChar( source, dest, index + 1 ) ) != -1 )
                count++;
            return count;
        }
        public static string TidyCode( this string Code ) {
            List<string> lines = Split( Code, '\n' );
            int c = lines.Count, spaces = 0;
            string text = "";
            for ( int i = 0 ; i < c ; i++ ) {
                spaces -= Count( lines[ i ], '}' );
                if ( spaces < 0 )
                    spaces = 0;
                lines[ i ] = lines[ i ].Replace( "\r", "" ).Replace( ( ( char )9 ).ToString( ), "" );
                if ( lines[ i ] == new string( ' ', lines[ i ].Length ) )
                    continue;
                int cc = 0;
                while ( lines[ i ][ cc ] == ' ' )
                    cc++;
                text += ( text == "" ? "" : "\r\n" ) + new string( ' ', spaces * 4 ) + lines[ i ].Substring( cc );
                spaces += Count( lines[ i ], '{' ) + Count( lines[ i ], '(' ) - Count( lines[ i ], ')' );
            }
            return text;
        }
        //public static string ToHighTypeName( this string _this, bool BottomLine = true ) {
        //    if ( _this == null )
        //        return "";
        //    string dot = ".";
        //    if ( BottomLine )
        //        dot = "_";
        //    string this2 = _this;
        //    if ( this2.Contains( "," ) )
        //        this2 = this2.Remove( this2.IndexOf( "," ) );
        //    int c = this2.Length;
        //    this2 = this2.Replace( "/", "." ).Replace( "+", "." );
        //    for ( int i = 0 ; i < c ; i++ )
        //        if ( !( this2[ i ] >= 'A' && this2[ i ] <= 'Z' )
        //            && !( this2[ i ] >= 'a' && this2[ i ] <= 'z' )
        //            && !( this2[ i ] >= '0' && this2[ i ] <= '9' )
        //            && this2[ i ] != '_'
        //            && this2[ i ] != '.'
        //            && this2[ i ] != '['
        //            && this2[ i ] != ']' )
        //            this2 = this2.Remove( i, 1 ).Insert( i, "_" );
        //    this2 = this2.Replace( ".", dot );
        //    return this2;
        //}
        public static TypeAnalied OwnResolve( this TypeReference _this ) {
            TypeDefinition td = _this.Resolve( );
            if ( td != null ) {
                TypeAnalied ta = new TypeAnalied( td );
                if ( _this is ArrayType ) {
                    ArrayType at=(ArrayType)_this;
                    if ( at.IsVector ) {
                        ta.FullName = "[]" + ta.FullName;
                        return ta;
                    }
                    ta.FullName = "[" + string.Join( ", ", ( from v in at.Dimensions select v.ToString( ) ) ) + "]" + ta.FullName;
                    return ta;
                }
                return ta;
            }
            return new TypeAnalied( null ) {
                FullName = _this.ToString( ),
                Name = _this.Name
            };
        }
        public static TypeAnalied OwnResolve( this TypeDefinition _this ) {
            TypeDefinition td = _this.Resolve( );
            if ( td != null )
                return new TypeAnalied( td );
            return new TypeAnalied( null ) {
                FullName = _this.ToString( ),
                Name = _this.Name
            };
        }
        public static bool isValid( this TypeDefinition _this ) {
            if ( !_this.FullName.Contains( "+" ) && _this.Name != "<Module>" && _this.Name != "" && !_this.FullName.Contains( "{" ) && !_this.FullName.Contains( "}" ) )
                return true;
            return false;
        }
        public static string BetterIdentifier( this string _this ) {
            return _this.Replace( '<', '_' ).Replace( '>', '_' ).Replace( '$', '_' ).Replace( '.', '_' );
        }
    }
    #endregion
}
