using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace VarocalCross {
    public partial class FormProperties : UserControl {
        OpenFileDialog ofd = new OpenFileDialog( );
        public MemorizePortable MemoryCore {
            get;
            set;
        }
        public string Path {
            get;
            set;
        }
        public string Directory {
            get {
                return new FileInfo( Path ).DirectoryName;
            }
        }
        bool isStarted = false;
        public FormProperties( string filepath ) {
            InitializeComponent( );
            Path = filepath;
            MemoryCore = new MemorizePortable( );
            foreach ( FATabStripItem item in _TabControl1.Items ) {
                item.Saved = true;
                foreach ( Control control in item.Controls ) {
                    if ( control is TextBox ) {
                        MemoryCore.Add( GetProperty<TextBox>( "Text" ), control );
                        ( ( TextBox )control ).TextChanged += FormProperties_TextChanged;
                    } else if ( control is CheckBox ) {
                        MemoryCore.Add( GetProperty<CheckBox>( "Checked" ), control );
                        ( ( CheckBox )control ).CheckedChanged += FormProperties_TextChanged;
                    } else if ( control is RichTextBox ) {
                        MemoryCore.Add( GetProperty<RichTextBox>( "Text" ), control );
                        ( ( RichTextBox )control ).TextChanged += FormProperties_TextChanged;
                    } else if ( control is ComboBox ) {
                        MemoryCore.Add( GetProperty<ComboBox>( "SelectedIndex" ), control );
                        ( ( ComboBox )control ).SelectedIndex = 0;
                        ( ( ComboBox )control ).SelectedIndexChanged += FormProperties_TextChanged;
                    } else if ( control is RadioButton ) {
                        MemoryCore.Add( GetProperty<RadioButton>( "Checked" ), control );
                        ( ( RadioButton )control ).CheckedChanged += FormProperties_TextChanged;
                    } else if ( control is NumericUpDown ) {
                        MemoryCore.Add( GetProperty<NumericUpDown>( "Value" ), control );
                        ( ( NumericUpDown )control ).ValueChanged += FormProperties_TextChanged;
                    }
                }
            }
            if ( File.Exists( Path ) ) {
                StreamReader reader = new StreamReader( Path );
                MemoryCore.Load( reader );
                while ( !reader.EndOfStream ) {
                    Package package = new Package( );
                    MemorizePortable mem = new MemorizePortable( );
                    mem.AddOwner( package );
                    mem.Load( reader );
                    packs.Rows.Add( package.Name, package.Version, package.ID.ToString( ), package.DefaultNamespace );
                    MemoryCore.AddOwner( package );
                }
                reader.Close( );
            }
        }
        void FormProperties_TextChanged( object sender, EventArgs e ) {
            if ( !isStarted )
                return;
            ( ( FATabStripItem )Parent ).Saved = false;
        }
        PropertyInfo GetProperty( Type Type, string Name ) {
            return Type.GetProperty( Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );
        }
        PropertyInfo GetProperty<T>( string Name ) {
            return typeof( T ).GetProperty( Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );
        }
        protected override void OnLoad( EventArgs e ) {
            this.BackColor = System.Drawing.Color.FromArgb( 50, 50, 50 );
            base.OnLoad( e );
            foreach ( Control control in Controls )
                BaseForm.BaseForm_ControlAdded( this, new ControlEventArgs( control ) );
            this.ControlAdded += BaseForm.BaseForm_ControlAdded;
            ( ( FATabStripItem )Parent ).Saved = true;
        }
        private void FormProperties_Load( object sender, EventArgs e ) {
            isStarted = true;
            Dock = DockStyle.Fill;
            if ( !File.Exists( Path ) ) {
                txt_namespace.Text = txt_name.Text = new FileInfo( Path ).Directory.Name;
                MemoryCore.Save( Path );
            }
        }
        private void button3_Click( object sender, EventArgs e ) {
            var obj = new ReferencesObj( );
            List<int> listInt = new List<int>( );
            foreach ( DataGridViewRow row in packs.Rows ) {
                listInt.Add( int.Parse( ( string )row.Cells[ 2 ].Value ) );
            }
            new ChooseReference( obj, listInt, ( cmb_platform.SelectedIndex == 0 ? Build.Platform.Windows :
                ( cmb_platform.SelectedIndex == 1 ? Build.Platform.Linux : Build.Platform.Mac ) ),
                ProjectSet.SelectedText.Contains( "64" ) ,ofd).ShowDialog( this );
            if ( !obj.Cancelled ) {
                List<Package> packages = new List<Package>( );
                MemorizePortable mem;
                if ( File.Exists( "pkg.m" ) ) {
                    StreamReader SR = new StreamReader( "pkg.m" );
                    while ( !SR.EndOfStream ) {
                        Package package = new Package( );
                        mem = new MemorizePortable( );
                        mem.AddOwner( package );
                        mem.Load( SR );
                        foreach ( int id in obj.IDS )
                            if ( id == package.ID ) {
                                packs.Rows.Add( package.Name, package.Version, package.ID.ToString( ), package.DefaultNamespace );
                                MemoryCore.AddOwner( package );
                                FormProperties_TextChanged( null, null );
                                break;
                            }
                    }
                    SR.Close( );
                }
            }
        }
        private void button4_Click( object sender, EventArgs e ) {
            if ( packs.SelectedRows.Count != 0 ) {
                List<DataGridViewRow> lggr = new List<DataGridViewRow>( );
                foreach ( DataGridViewRow row in packs.SelectedRows ) {
                    lggr.Add( row );
                    foreach ( PropertyInfo prop in MemoryCore.lpi )
                        if ( prop.DeclaringType.Name == "Package" && prop.Name == "ID" )
                            if ( ( ( string )row.Cells[ 2 ].Value ) == prop.GetGetMethod( ).Invoke(
                                MemoryCore.owners[ MemoryCore.lpi.IndexOf( prop ) ], new object[ 0 ] ).ToString( ) ) {
                                MemoryCore.DeleteOwner( MemoryCore.owners[ MemoryCore.lpi.IndexOf( prop ) ] );
                                break;
                            }
                }
                foreach ( var v in lggr )
                    packs.Rows.Remove( v );
                FormProperties_TextChanged( null, null );
            }
        }
    }
}