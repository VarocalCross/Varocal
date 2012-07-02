using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VarocalCross {
    public class Memorize {
        static List<Memorize> lm = new List<Memorize>( );
        public static Memorize GetObject( int index ) {
            return lm[ Count ];
        }
        public static int Count {
            get;
            protected set;
        }
        static Memorize( ) {
            Count = 0xffffff;
            if ( !Directory.Exists( "mem" ) )
                Directory.CreateDirectory( "mem" );
        }
        public int Index {
            get;
            protected set;
        }
        List<PropertyInfo> lpi = new List<PropertyInfo>( );
        List<object> owners = new List<object>( );
        public Memorize( ) {
            Index = Count++;
            lm.Add( this );
        }
        public void Add( PropertyInfo prop, object owner ) {
            lpi.Add( prop );
            owners.Add( owner );
        }
        public void Load( ) {
            MemorizePortable.LoadManual( "mem\\" + Index + ".dat", lpi, owners );
        }
        public void Save( ) {
            MemorizePortable.SaveManual( "mem\\" + Index + ".dat", lpi, owners );
        }
        public static void LoadAll( ) {
            foreach ( Memorize mem in lm )
                mem.Load( );
        }
        public static void SaveAll( ) {
            foreach ( Memorize mem in lm )
                mem.Save( );
        }
    }
    public class MemorizePortable {
        public List<PropertyInfo> lpi = new List<PropertyInfo>( );
        public List<object> owners = new List<object>( );
        public void Add( PropertyInfo prop, object owner ) {
            lpi.Add( prop );
            owners.Add( owner );
        }
        public void AddOwner( object owner ) {
            foreach ( PropertyInfo prop in owner.GetType( ).GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) )
                Add( prop, owner );
        }
        public void DeleteOwner( object owner ) {
            for ( int i = 0 ; i < owners.Count ; i++ ) {
                if ( owners[ i ] == owner ) {
                    owners.RemoveAt( i );
                    lpi.RemoveAt( i );
                    i--;
                }
            }
        }
        public void Load( string pathfile ) {
            MemorizePortable.LoadManual( pathfile, lpi, owners );
        }
        public void Load( StreamReader stream ) {
            MemorizePortable.LoadManual( stream, lpi, owners );
        }
        public void Save( string pathfile ) {
            MemorizePortable.SaveManual( pathfile, lpi, owners );
        }
        public void Save( StreamWriter stream ) {
            MemorizePortable.SaveManual( stream, lpi, owners );
        }
        public static void LoadManual( string pathfile,List<PropertyInfo> properties,List<Object> owners ) {
            StreamReader SR = null;
            try {
                SR = new StreamReader( pathfile );
                LoadManual( SR, properties, owners );
            } finally {
                SR.Close( );
            }
        }
        public static void LoadManual( StreamReader stream, List<PropertyInfo> properties, List<Object> owners ) {
            int i = -1;
            foreach ( PropertyInfo pi in properties ) {
                i++;
                if ( pi == null )
                    continue;
                Type obj = pi.PropertyType;
                if ( obj == typeof( int ) || obj.BaseType == typeof( Enum ) ) {
                    pi.SetMethod.Invoke( owners[ i ], new object[ ] { int.Parse( stream.ReadLine( ) ) } );
                } else if ( obj == typeof( decimal ) ) {
                    pi.SetMethod.Invoke( owners[ i ], new object[ ] { decimal.Parse( stream.ReadLine( ) ) } );
                } else if ( obj == typeof( Control ) ){
                    pi.SetMethod.Invoke( owners[ i ], new object[ ] { ( ( Control )owners[ i ] ).FindForm( ).Controls.Find( stream.ReadLine( ), true ) } );
                } else if ( obj == typeof( bool ) ) {
                    pi.SetMethod.Invoke( owners[ i ], new object[ ] { ( stream.ReadLine( ) == "T" ? true : false ) } );
                } else if ( obj == typeof( string ) ) {
                    string tst = "";
                    int i2 = int.Parse( stream.ReadLine( ) );
                    while ( i2-- > 0 )
                        tst += ( tst == "" ? "" : "\r\n" ) + stream.ReadLine( );
                    pi.SetMethod.Invoke( owners[ i ], new object[ ] { tst } );
                } else {
                    MessageBox.Show( obj.FullName );
                }
            }
        }
        public static void SaveManual( string pathfile, List<PropertyInfo> properties, List<Object> owners ) {
            StreamWriter SW = null;
            if ( File.Exists( pathfile ) )
                File.Delete( pathfile );
            try {
                SW = new StreamWriter( pathfile );
                SaveManual( SW, properties, owners );
            } finally {
                SW.Close( );
            }
        }
        public static void SaveManual( StreamWriter stream, List<PropertyInfo> properties, List<Object> owners ) {
            int i = -1;
            foreach ( PropertyInfo pi in properties ) {
                i++;
                if ( pi == null )
                    continue;
                Type obj = pi.PropertyType;
                object obj2 = pi.GetGetMethod( ).Invoke( owners[ i ], new object[ 0 ] );
                if ( obj == typeof( int ) || obj == typeof( decimal ) ) {
                    stream.WriteLine( obj2.ToString( ) );
                } else if ( obj == typeof( Control ) ) {
                    stream.WriteLine( ( ( Control )obj2 ).Name );
                } else if ( obj.BaseType == typeof( Enum ) ) {
                    stream.WriteLine( ( ( int )obj2 ).ToString( ) );
                } else if ( obj == typeof( bool ) ) {
                    stream.WriteLine( ( ( ( bool )obj2 ) ? "T" : "F" ) );
                } else if ( obj == typeof( string ) ) {
                    stream.WriteLine( ( ( string )obj2 ).Count( '\n' ) + 1 );
                    stream.WriteLine( ( string )obj2 );
                } else {
                    MessageBox.Show( obj.FullName );
                }
            }
        }
    }
    public class PrivateMemory {
        public PrivateMemory( object obj ) {
            Object = obj;
        }
        public object Object {
            get;
            private set;
        }
        public dynamic this[ string variablename ] {
            get {
                foreach ( var v in Object.GetType( ).GetProperties( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static ) )
                    if ( v.Name == variablename )
                        return v.GetGetMethod( true ).Invoke( Object, new object[ 0 ] );
                foreach ( var v in Object.GetType( ).GetFields( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static ) )
                    if ( v.Name == variablename )
                        return v.GetValue( Object );
                throw new KeyNotFoundException( variablename );
            }
            set {
                foreach ( var v in Object.GetType( ).GetProperties( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static ) )
                    if ( v.Name == variablename ) {
                        v.GetSetMethod( true ).Invoke( Object, new object[ ]{value} );
                        return;
                    }
                foreach ( var v in Object.GetType( ).GetFields( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static ) )
                    if ( v.Name == variablename ) {
                        v.SetValue( Object, value );
                        return;
                    }
                throw new KeyNotFoundException( variablename );
            }
        }
        public string[ ] Names {
            get {
                List<String> names = new List<string>( );
                foreach ( var v in Object.GetType( ).GetProperties( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance ) )
                    names.Add( v.Name );
                foreach ( var v in Object.GetType( ).GetFields( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance ) )
                    names.Add( v.Name );
                return names.ToArray( );
            }
        }
        public Type[ ] Types {
            get {
                List<Type> names = new List<Type>( );
                foreach ( var v in Object.GetType( ).GetProperties( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance ) )
                    names.Add( v.PropertyType );
                foreach ( var v in Object.GetType( ).GetFields( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance ) )
                    names.Add( v.FieldType );
                return names.ToArray( );
            }
        }
        public object Invoke( string name, object[ ] paras ) {
            foreach ( var v in Object.GetType( ).GetMethods( BindingFlags.NonPublic
                | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static ) ) {
                    ParameterInfo[ ] pi = v.GetParameters( );
                if ( v.Name == name && pi.Length == paras.Length ) {
                    int ind = 0;
                    bool _continue = false;
                    foreach ( var v2 in v.GetParameters( ) )
                        if ( v2.ParameterType != paras[ ind++ ].GetType() ) {
                            _continue = true;
                            break;
                        }
                    if ( _continue )
                        continue;
                    return v.Invoke( Object, paras );
                }
            }
            throw new KeyNotFoundException( name );
        }
    }
}
