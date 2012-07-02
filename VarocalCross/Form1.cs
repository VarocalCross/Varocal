using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
//using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using ComponentAce.Compression.Archiver;
using ComponentAce.Compression.Interfaces;
using ComponentAce.Compression.ZipForge;

namespace VarocalCross {
    public partial class Form1 : BaseForm {
        [DllImport( "user32.dll" )]
        public static extern void keybd_event( byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo );
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        Memorize mem = new Memorize( );
        FolderBrowserDialog fbd = new FolderBrowserDialog( ) {
            SelectedPath = Directory.GetParent( Application.ExecutablePath ).FullName
        };
        [DefaultValue( typeof( Form1 ) )]
        public static Form1 runningForm {
            get;
            set;
        }

        [DefaultValue( typeof( TextStyle ) )]
        public static Style keywordStyle {
            get;
            set;
        }

        [DefaultValue( typeof( TextStyle ) )]
        public static Style objectStyle {
            get;
            set;
        }

        [DefaultValue( typeof( TextStyle ) )]
        public static Style valueStyle {
            get;
            set;
        }

        [DefaultValue( typeof( TextStyle ) )]
        public static Style commentStyle {
            get;
            set;
        }
        SaveFileDialog SFD = new SaveFileDialog( );
        OpenFileDialog OFD = new OpenFileDialog( );
        public class Lexer {
            public static List<string> keywords = new List<string>( );
            public static void Lexing( ref List<Token> lt ) {
                foreach ( Token t in lt ) {
                    if ( t.Type == TokenType.Word ) {
                        foreach ( string str in keywords ) {
                            if ( t.Value == str ) {
                                t.Type = TokenType.Keyword;
                                break;
                            }
                        }
                    }
                }
            }
        }
        Point MouseXY;
        Random random = new Random( );
        bool MousePanelClick = false;
        long ticks = 0;
        public string SolutionPath {
            get {
                List<string> tst=new List<string>();
                foreach ( TreeNode node in treeView1.Nodes )
                    tst.Add( node.Tag as string );
                return string.Join( ";", tst.ToArray( ) );
            }
            set {
                if (!OpenSolutionStartUp)
                    return;
                string[ ] projects = value.Split( ';' );
                if ( projects[ 0 ] == "" )
                    return;
                treeView1.Nodes.Clear( );
                foreach ( string project in projects )
                    LoadProject( project );
            }
        }
        public bool OpenSolutionStartUp {
            get {
                return preferencesWindow.last_startup.Checked;
            }
            set {
                preferencesWindow.last_startup.Checked = value;
            }
        }
        public static PrefWindow preferencesWindow = new PrefWindow( );
        public static Tokenizer tokenizer = new Tokenizer( );
        public static Parser parser = Parser.Standard;
        Build build = new Build( );

        public Form1( ) {
            InitializeComponent( );
            build.parser = Parser.Standard;
            this.FormClosing += Form1_FormClosing;
            Lexer.keywords.AddRange( parser.Tokenizer.Keywords );
            foreach ( string keyword in TypeExt.dss.Keys )
                if ( !keyword.Contains( "." ) )
                    Lexer.keywords.Add( keyword );
            keywordStyle = new TextStyle( );
            objectStyle = new TextStyle( );
            valueStyle = new TextStyle( );
            commentStyle = new TextStyle( );
            ColorUpdate( );
            tokenizer.ListingComments = true;
            fbd.SelectedPath = Path.Combine( Environment.CurrentDirectory, "projects" );
            OFD.FileName = Path.Combine( Environment.CurrentDirectory, "projects" );
            //AddHotKey( VarocalCross.ModifierKeys.Control, Keys.S );
            //AddHotKey( VarocalCross.ModifierKeys.Control | VarocalCross.ModifierKeys.Shift, Keys.S );
            //OnHotKeyPressed += Form1_OnHotKeyPressed;
            KeyPreview = true;
        }
        void Form1_OnHotKeyPressed( object sender, KeyPressedEventArgs e ) {
            //if ( !ContainsFocus && !Focused )
            //    return;
            //if ( e.Modifier == VarocalCross.ModifierKeys.Control && e.Key == Keys.S )
            //    saveToolStripMenuItem_Click( null, null );
            //else if ( (e.Modifier & ( VarocalCross.ModifierKeys.Shift & VarocalCross.ModifierKeys.Control ))
            //    ==( VarocalCross.ModifierKeys.Shift & VarocalCross.ModifierKeys.Control) && e.Key == Keys.S )
            //    saveAllToolStripMenuItem_Click( null, null );
        }
        void Form1_FormClosing( object sender, FormClosingEventArgs e ) {
            Memorize.SaveAll( );
            preferencesWindow.Save( );
        }
        private void printWindowToolStripMenuItem_Click( object sender, EventArgs e ) {
            new Thread( new ThreadStart( ( ) => {
                Thread.Sleep( 10 );
                keybd_event( unchecked( ( byte )18 ), 0, KEYEVENTF_EXTENDEDKEY, 0 );
                keybd_event( unchecked( ( byte )Keys.PrintScreen ), 0, KEYEVENTF_EXTENDEDKEY, 0 );
                keybd_event( unchecked( ( byte )18 ), 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0 );
                keybd_event( unchecked( ( byte )Keys.PrintScreen ), 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0 );
            } ) ).Start( );
        }
        private void exitToolStripMenuItem_Click( object sender, EventArgs e ) {
            Application.Exit( );
        }
        private void Form1_Load( object sender, EventArgs e ) {
            mem.Add( GetProperty( GetType( ), "Top" ), this );
            mem.Add( GetProperty( GetType( ), "Left" ), this );
            mem.Add( GetProperty( GetType( ), "Width" ), this );
            mem.Add( GetProperty( GetType( ), "Height" ), this );
            mem.Add( GetProperty( GetType( ), "WindowState" ), this );
            mem.Add( GetProperty( GetType( ), "OpenSolutionStartUp" ), this );
            mem.Add( GetProperty( GetType( ), "SolutionPath" ), this );
            mem.Add( GetProperty( treeView1.GetType( ), "Width" ), treeView1 );
            Memorize.LoadAll( );
            printWindowToolStripMenuItem.ShortcutKeyDisplayString = "Alt+PrintScreen";
            saveToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+S";
            saveAllToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Shift+S";
            projectToolStripMenuItem1.ShortcutKeyDisplayString = "Ctrl+N";
            openToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+O";
            runReleaseToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+F5";
        }
        public static PropertyInfo GetProperty( Type Type, string Name ) {
            return Type.GetProperty( Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );
        }
        private void toolStripButton4_Paint( object sender, PaintEventArgs e ) {
            ToolStripButton send = ( ToolStripButton )sender;
            send.ForeColor = Color.Transparent;
            e.Graphics.DrawString( send.Text, send.Font, Brushes.Black, e.ClipRectangle, new StringFormat( ) {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = ( send.TextDirection == ToolStripTextDirection.Horizontal ? 0 : StringFormatFlags.DirectionVertical )
            } );
        }
        private void toolStripButton1_MouseUp( object sender, MouseEventArgs e ) {
            ToolStripButton tsb = ( ToolStripButton )sender;
            if ( tsb.Tag is Control ) {
                Control control = ( Control )tsb.Tag;
                if ( tsb.TextDirection == ToolStripTextDirection.Horizontal ) {
                    control.Width = Width;
                    control.Height = 300;
                    control.Left = 0;
                    control.Top = Height - control.Height;
                } else {
                    control.Width = 300;
                    control.Height = Width;
                    control.Left = Width - control.Width;
                    control.Top = 0;
                }
                Controls.Add( control );
                control.Focus( );
                control.BringToFront( );
            }
        }
        private void panel1_MouseDown( object sender, MouseEventArgs e ) {
            MouseXY = e.Location;
            MousePanelClick = true;
            ticks = DateTime.Now.Ticks;
        }
        private void panel1_MouseMove( object sender, MouseEventArgs e ) {
            if ( MousePanelClick ) {
                if ( DateTime.Now.AddTicks( -ticks ).Millisecond > 40 ) {
                    ticks = DateTime.Now.Ticks;
                    treeView1.Width -= e.Location.X - MouseXY.X;
                }
            }
        }
        private void panel1_MouseUp( object sender, MouseEventArgs e ) {
            if ( MousePanelClick )
                treeView1.Width -= e.Location.X - MouseXY.X;
            MousePanelClick = false;
        }
        private void panel1_Paint( object sender, PaintEventArgs e ) {
            e.Graphics.FillRectangle( new LinearGradientBrush( new Point( 0, 0 ), new Point( panel1.Width, 0 ), BackColor, treeView1.BackColor ), new Rectangle( 0, 0, panel1.Width, panel1.Height ) );
        }
        void DirectoryCopy( string s, string d ) {
            if ( !Directory.Exists( d ) )
                Directory.CreateDirectory( d );
            foreach ( string dir in Directory.GetDirectories( s ) )
                DirectoryCopy( dir, Path.Combine( d,new DirectoryInfo( dir ).Name ) );
            foreach ( string file in Directory.GetFiles( s ) ) {
                string file2 = Path.Combine( d, new FileInfo( file ).Name );
                if ( File.Exists( file2 ) )
                    File.Delete( file2 );
                File.Copy( file, file2 );
            }
        }
        private void projectToolStripMenuItem1_Click( object sender, EventArgs e ) {
            NewProjectResult result = new NewProjectResult( );
            new NewProject( result ).ShowDialog( );
            if ( !result.Cancelled ) {
                Directory.CreateDirectory( "projects\\" + result.Name + "\\Source" );
                Directory.CreateDirectory( "projects\\" + result.Name + "\\Source-Debug" );
                Directory.CreateDirectory( "projects\\" + result.Name + "\\Build\\Debug" );
                Directory.CreateDirectory( "projects\\" + result.Name + "\\Build\\Release" );
                Directory.CreateDirectory( "projects\\" + result.Name + "\\Data" );
                foreach ( string import in result.importfrom )
                    DirectoryCopy( import, "projects\\" + result.Name );
                int tab=int.Parse(preferencesWindow.tab_spaces.Text);
                File.WriteAllText( "projects\\" + result.Name + "\\Data\\main", @"using System;

namespace " + result.Name.Replace( ' ', '_' ) + @" {
" + new string( ' ', tab ) + @"class Program {
" + new string( ' ', tab * 2 ) + @"static void Main() {
" + new string( ' ', tab * 3 ) + @"
" + new string( ' ', tab * 2 ) + @"}
" + new string( ' ', tab ) + @"}
}" );
                LoadProject( "projects\\" + result.Name );
                FormProperties prop = new FormProperties( "projects\\" + result.Name + "\\projectdata.dat" );
                if ( File.Exists( "pkg.m" ) ) {
                    StreamReader reader = new StreamReader( "pkg.m" );
                    while ( !reader.EndOfStream ) {
                        Package package = new Package( );
                        MemorizePortable mem = new MemorizePortable( );
                        mem.AddOwner( package );
                        mem.Load( reader );
                        if ( package.Name == "BaseProject" ) {
                            prop.MemoryCore.AddOwner( package );
                        }
                    }
                    reader.Close( );
                }
                prop.MemoryCore.Save( prop.Path );
                FATabStripItem item = new FATabStripItem( );
                item.Title = "Properties";
                item.Controls.Add( new FormProperties( prop.Path ) );
                _TabControl1.Items.Add( _TabControl1.SelectedItem = item );
                OpenTab( "projects\\" + result.Name + "\\Data\\main" );
            }
        }
        void LoadProject( string path ) {
            DataNode dataNode = new DataNode( );
            try {
                dataNode.Text = Path.GetFileName( path );
                dataNode.ContextMenuStrip = ProjectMenuStrip;
                dataNode.Tag = path;
                LoadDir( path + "\\data\\", dataNode );
            } catch {
                MessageBox.Show( this, "Unable to load random folder ,\nto see if folder is project it needs to contain data folder." );
                return;
            }
            treeView1.Nodes.Add( dataNode );
            dataNode.Expand( );
        }
        void LoadDir( string path, TreeNode holder ) {
            foreach ( string dir in Directory.GetDirectories( path ) ) {
                TreeNode node = new TreeNode( Path.GetFileName( dir ) );
                node.ContextMenuStrip = ProjectMenuStrip;
                LoadDir( dir, node );
                holder.Nodes.Add( node );
            }
            foreach ( string file in Directory.GetFiles( path ) ) {
                TreeNode node = new TreeNode( Path.GetFileName( file ) );
                node.Tag = file;
                node.ContextMenuStrip = nodeMenuStrip;
                holder.Nodes.Add( node );
            }
        }
        private void addItemToolStripMenuItem_Click( object sender, EventArgs e ) {
            TreeNode project = treeView1.SelectedNode;
            while ( project.Parent != null )
                project = project.Parent;
            FormProperties fp = default( FormProperties );
            bool setted = false;
            foreach ( FATabStripItem item in _TabControl1.items ) {
                if ( item.Controls[ 0 ] is FormProperties && !setted ) {
                    if ( ( ( ( FormProperties )item.Controls[ 0 ] ).Path ).StartsWith( project.Tag as string ) ) {
                        fp = ( FormProperties )item.Controls[ 0 ];
                        setted = true;
                    }
                }
                SaveTab( item );
            }
            if ( !setted )
                fp = new FormProperties( Path.Combine( project.Tag as string, "projectdata.dat" ) );
            string path = ToRightPath( GetNodePath( treeView1.SelectedNode ) + "code" );
            int tab = int.Parse( preferencesWindow.tab_spaces.Text );
            File.WriteAllText( path, @"using System;

namespace " + fp.txt_name.Text.Replace( ' ', '_' ) + @" {

}" );
            TreeNode node = new TreeNode( Path.GetFileName( path ) );
            node.Tag = path;
            node.ContextMenuStrip = nodeMenuStrip;
            treeView1.SelectedNode.Nodes.Add( node );
            treeView1.SelectedNode.Expand( );
            OpenTab( path );
        }
        void OpenTab( string path ) {
            foreach ( FATabStripItem page in _TabControl1.Items )
                if ( page.Tag is string )
                    if ( ( string )page.Tag == path ) {
                        _TabControl1.SelectedItem = page;
                        return;
                    }
            FATabStripItem faTabStripItem = new FATabStripItem( );
            faTabStripItem.Title = Path.GetFileName( path );
            FastColoredTextBox RB1 = new FastColoredTextBox( );
            #region TextBox Properties
            RB1.Dock = DockStyle.Fill;
            RB1.CurrentLineColor = Color.FromArgb( 255, 255, 64 );
            RB1.BackColor = Color.FromArgb( 70, 70, 70 );
            RB1.ForeColor = Color.White;
            RB1.PreferredLineWidth = 0;
            RB1.BorderStyle = BorderStyle.FixedSingle;
            RB1.Cursor = Cursors.IBeam;
            RB1.ChangedLineColor = Color.DarkRed;
            RB1.ChangedLineWidth = 2;
            RB1.LeftBracket = '[';
            RB1.LeftBracket2 = '(';
            RB1.LeftBracket3 = '{';
            RB1.RightBracket = ']';
            RB1.RightBracket2 = ')';
            RB1.RightBracket3 = '}';
            RB1.BracketsStyle = RB1.BracketsStyle2 = RB1.BracketsStyle3 = new MarkerStyle( new SolidBrush( Color.FromArgb( 0, 160, 160 ) ) );
            RB1.SelectionStyle.BackgroundBrush = new SolidBrush( preferencesWindow.colors[ 4 ] );
            RB1.Font = preferencesWindow.f;
            RB1.BorderStyle = BorderStyle.None;
            RB1.IndentBackColor = Color.FromArgb( 120, 120, 120 );
            RB1.LineNumberColor = Color.FromArgb( 225, 225, 225 );
            #endregion
            bool iswait = true;
            RB1.TextChanged += new EventHandler<TextChangedEventArgs>( ( object sender, TextChangedEventArgs e ) => {
                new Thread( new ThreadStart( ( ) => {
                    if ( !RB1.IsHandleCreated )
                        return;
                    faTabStripItem.Saved = false;
                    _TabControl1.BeginInvoke( new MethodInvoker( ( ) => {
                        _TabControl1.Invalidate( );
                    } ) );
                    if ( RB1.BackColor != Color.FromArgb( 50, 50, 50 ) )
                        RB1.BackColor = Color.FromArgb( 50, 50, 50 );
                    RB1.BeginInvoke( new MethodInvoker( ( ) => {
                        new Thread( new ThreadStart( ( ) => {
                            try {
                                string txt = RB1.Text;
                                List<Token> lineStream = tokenizer.Tokenize( new Range( RB1, 0,
                                    RB1.Selection.Start.iLine == 0 ? 0 : RB1.Selection.Start.iLine - 1,
                                    RB1.Lines[ RB1.Selection.Start.iLine == RB1.LinesCount - 1 ? RB1.Selection.Start.iLine : RB1.Selection.Start.iLine + 1 ].Length,
                                    RB1.Selection.Start.iLine == RB1.LinesCount - 1 ? RB1.Selection.Start.iLine : RB1.Selection.Start.iLine + 1 ).Text
                                    , RB1.Selection.Start.iLine == 0 ? 1 : RB1.Selection.Start.iLine );
                                Lexer.Lexing( ref lineStream );
                                PaintTextBox( RB1, lineStream );
                                try {
                                    RB1.TabLength = int.Parse( preferencesWindow.tab_spaces.Text );
                                    RB1.TabLength = 4;
                                    if ( iswait )
                                        System.Threading.Thread.Sleep( int.Parse( preferencesWindow.paint_delay.Text ) );
                                    else
                                        iswait = true;
                                } catch {
                                    MessageBox.Show( this, "Fix code editor at preferences window \r\nPath: File->Preferences->Scripts and codes" );
                                    return;
                                }
                                if ( RB1.Text != txt )
                                    return;
                                RB1.VisibleRange.ClearStyles( false );
                                RB1.VisibleRange.ClearFoldingMarkers( false );
                                List<Token> lt = tokenizer.Tokenize( RB1.VisibleRange.Text );
                                Lexer.Lexing( ref lt );
                                PaintTextBox( RB1, lt );
                            } catch {
                                MessageBox.Show(this, "Exception caught while painting" );
                            }
                        } ) ).Start( );
                    } ) );
                } ) ).Start( );
            } );
            RB1.KeyDown += new KeyEventHandler( ( object sender, KeyEventArgs kea ) => {
                RB1.SelectionStyle.BackgroundBrush = new SolidBrush( preferencesWindow.colors[ 4 ] );
                RB1.Font = preferencesWindow.f;
                //runningForm.savedProject = false;
                int speccount = 0;
                bool isstr = false;
                int ind = RB1.Selection.Start.iChar;
                List<Char> txt = RB1[ RB1.Selection.Start.iLine ];
                txt.ForEach( new Action<Char>( ( Char c ) => {
                    if ( --ind <= -1 )
                        return;
                    if ( c.c == '"' || c.c == '\'' )
                        isstr = isstr == false;
                } ) );
                if ( isstr )
                    return;
                string astr = RB1.Text.Substring( 0, RB1.SelectionStart );
                int startline = astr.LastIndexOf( '\n', RB1.SelectionStart - 1 );
                if ( startline == -1 )
                    startline = 0;
                string astr2 = astr.Substring( startline );
                if ( kea.KeyValue == 219 && kea.Shift ) {
                    kea.Handled = true;
                    RB1.InsertText( " {\r\n" + new string( ' ', RB1[ RB1.Selection.Start.iLine ].StartSpacesCount + RB1.TabLength ) );
                    iswait = false;
                    RB1.OnTextChanged( );
                } else if ( kea.KeyValue == 8 && RB1.SelectionStart > 0 && RB1.SelectionLength == 0 ) {
                    astr = RB1.Text;
                    startline = astr.LastIndexOf( '\n', RB1.SelectionStart - 1 ) + 1;
                    if ( astr.Substring( startline, RB1.SelectionStart - startline ) == new string( ' ', RB1.SelectionStart - startline ) ) {
                        speccount = RB1.SelectionStart - startline;
                        if ( startline != 0 )
                            speccount += 2;
                        RB1.SelectionStart -= speccount;
                        RB1.SelectionLength = speccount;
                        RB1.SelectedText = "";
                        kea.Handled = true;
                    }
                } else if ( kea.KeyValue == 13 ) {
                    int spaces = RB1[ RB1.Selection.Start.iLine ].StartSpacesCount;
                    if ( RB1.Selection.CharAfterStart == '\n' )
                        return;
                    RB1.InsertText( "\r\n" + new string( ' ', spaces ) );
                    kea.Handled = true;
                } else if ( kea.KeyValue == 221 && kea.Shift ) {
                    kea.Handled = true;
                    int spaces = RB1[ RB1.Selection.Start.iLine ].StartSpacesCount;
                    if ( astr2.Replace( " ", "" ) == "\n" ) {
                        if ( astr2.Length > RB1.TabLength + 1 ) {
                            RB1.SelectionStart -= RB1.TabLength;
                            RB1.SelectionLength = RB1.TabLength;
                            RB1.SelectedText = "}\r\n" + new string( ' ', spaces - RB1.TabLength );
                        } else {
                            RB1.SelectionStart = startline + 1;
                            RB1.SelectionLength = astr2.Length - 1;
                            RB1.SelectedText = "}\r\n";
                        }
                    } else {
                        if ( spaces < RB1.TabLength + 1 ) {
                            RB1.InsertText( "\r\n}\r\n" + new string( ' ', spaces ) );
                        } else {
                            RB1.InsertText( "\r\n" + new string( ' ', spaces - RB1.TabLength ) + "}\r\n" + new string( ' ', spaces - RB1.TabLength ) );
                        }
                    }
                }
            } );
            RB1.Text = File.ReadAllText( path );
            RB1.Height = RB1.Width = 2500;
            faTabStripItem.Controls.Add( RB1 );
            faTabStripItem.Tag = path;
            _TabControl1.Items.Add( faTabStripItem );
            List<Token> lt2 = tokenizer.Tokenize( RB1.Text );
            Lexer.Lexing( ref lt2 );
            PaintTextBox( RB1, lt2 );
            RB1.OnTextChanged( );
            RB1.ClearLineChanges( );
            faTabStripItem.Saved = true;
            OpenTab( path );
            _TabControl1.Invalidate( );
        }
        void PaintTextBox( FastColoredTextBox target, List<Token> changes ) {
            XManager<Token> XMT = new XManager<Token>( changes );
            while ( !XMT.Eof ) {
                try {
                    if ( XMT.X.Type == TokenType.Keyword ) {
                        target.VisibleRange.getRange( XMT.X.LineNum - 1, XMT.X.Pos - 1, 0, XMT.X.Length ).SetStyle( keywordStyle, false );

                    } else if ( XMT.X.Type == TokenType.Comment ) {
                        if ( XMT.X.Value.Contains( "\n" ) ) {
                            target.VisibleRange.getRange( XMT.X.LineNum - 1, XMT.X.Pos - 1,
                                XMT.X.Value.Split( '\n' ).GetLength( 0 ) - 1,
                                XMT.X.Length - XMT.X.Value.LastIndexOf( "\n" ) - 1 ).SetStyle( commentStyle, false );
                            target[ target.VisibleRange.Start.iLine + XMT.X.LineNum - 1 ].FoldingStartMarker = "/**/";
                            target[ target.VisibleRange.Start.iLine + XMT.X.LineNum - 2 + XMT.X.Value.Split( '\n' ).GetLength( 0 ) ].FoldingEndMarker = "/**/";
                        } else
                            target.VisibleRange.getRange( XMT.X.LineNum - 1, XMT.X.Pos - 1, 0, XMT.X.Length ).SetStyle( commentStyle, false );
                    } else if ( XMT.X.Type == TokenType.String || XMT.X.Type == TokenType.Integer ) {
                        target.VisibleRange.getRange( XMT.X.LineNum - 1, XMT.X.Pos - 1, 0, XMT.X.Length ).SetStyle( valueStyle, false );
                    } else if ( XMT.X.Type == TokenType.Symbol ) {
                        switch ( XMT.X.Value ) {
                            case "{":
                                target[ XMT.X.LineNum - 1 ].FoldingStartMarker = "{}";
                                break;

                            case "}":
                                target[ XMT.X.LineNum - 1 ].FoldingEndMarker = "{}";
                                break;
                        }
                    }
                    //else if ( Objects.IndexOf( XMT.X.Value ) != -1 )
                    //    target.VisibleRange.getRange( XMT.X.LineNum - 1, XMT.X.Pos - 1, 0, XMT.X.Length ).SetStyle( objectStyle );
                } catch {
                    //MessageBox.Show( "Caught Error: " + XMT[ XMT.Indexed ] );
                }
                XMT++;
            }
            target.Invalidate( );
        }
        string GetNodePath( TreeNode node ) {
            string str = node.FullPath;
            if ( str.Contains( "\\" ) )
                str = str.Substring( str.IndexOf( "\\" ) + 1 );
            else
                str = "";
            if ( !str.EndsWith( "\\" ) && str != "" )
                str += "\\";
            return Path.Combine( ( string )LastParent( node ).Tag, "data\\" + str );
        }
        TreeNode LastParent( TreeNode node ) {
            if ( node.Parent == null )
                return node;
            return LastParent( node.Parent );
        }
        string ToRightPath( string path ) {
            if ( !File.Exists( path ) && !Directory.Exists( path ) )
                return path;
            for ( int i = 1 ; ; i++ )
                if ( !File.Exists( path + i ) && !Directory.Exists( path + i ) )
                    return path + i;
        }
        private void treeView1_ItemDrag( object sender, ItemDragEventArgs e ) {
            if ( e.Button == MouseButtons.Left && ( ( TreeNode )e.Item ).Parent == null )
                treeView1.SelectedNode = ( TreeNode )e.Item;
            else {
                treeView1.SelectedNode = ( TreeNode )e.Item;
                DoDragDrop( e.Item, DragDropEffects.Move );
            }
        }
        private void treeView1_MouseDoubleClick( object sender, MouseEventArgs e ) {
            if ( e != null )
                if ( e.Button != MouseButtons.Left )
                    return;
            if ( treeView1.SelectedNode == null )
                return;
            if ( treeView1.SelectedNode.Parent == null )
                return;
            if ( !( treeView1.SelectedNode.Tag is string ) )
                return;
            OpenTab( ( string )treeView1.SelectedNode.Tag );
        }
        private void treeView1_AfterLabelEdit( object sender, NodeLabelEditEventArgs e ) {
            if ( e.Label == null )
                return;
            e.CancelEdit = true;

            try {
                if ( e.Node.Tag is string ) {
                    StreamWriter streamWriter = new StreamWriter( ( string )e.Node.Tag );
                    streamWriter.Close( ); //Check if file is used by another process or editing
                    string randomname = Path.GetRandomFileName( ), rightlabel;
                    FATabStripItem item = default( FATabStripItem );
                    bool setted = false;
                    foreach (FATabStripItem v in _TabControl1.Items)
                        if (v.Tag is string)
                            if ( ( string )v.Tag == ( string )e.Node.Tag ) {
                                item = v;
                                setted = true;
                            }
                    File.Copy( ( string )e.Node.Tag, randomname );
                    File.Delete( ( string )e.Node.Tag );
                    rightlabel = ToRightPath( GetNodePath( e.Node.Parent ) + "\\" + e.Label );
                    if ( setted ) {
                        item.Tag = rightlabel;
                        item.Title = new FileInfo( rightlabel ).Name;
                    }
                    File.Copy( randomname, rightlabel );
                    File.Delete( randomname );
                    e.Node.Tag = rightlabel;
                    e.Node.Text = rightlabel.Substring( rightlabel.LastIndexOf( "\\" ) + 1 );
                } else {
                    string randomname = Path.GetRandomFileName( ), rightlabel;
                    Directory.Move( GetNodePath( e.Node ), randomname );
                    rightlabel = ToRightPath( GetNodePath( e.Node.Parent ) + e.Label );
                    Directory.Move( randomname, rightlabel );
                    ChangeTags( e.Node, Path.GetFileName( rightlabel ), e.Node.Text );
                    e.Node.Text = Path.GetFileName( rightlabel );
                }
            } catch {
            }
        }
        private void treeView1_DrawNode( object sender, DrawTreeNodeEventArgs e ) {
            Point p = e.Bounds.Location;
            p.Y += 1;
            if ( e.Node.IsSelected && treeView1.Focused )
                e.Graphics.DrawString( e.Node.Text, treeView1.Font, new SolidBrush( Color.FromArgb( 0, 0, 0 ) ), p );
            else
                e.DrawDefault = true;
        }
        private void treeView1_NodeMouseClick( object sender, TreeNodeMouseClickEventArgs e ) {
            ( e.Node as TreeNode ).TreeView.SelectedNode = ( e.Node as TreeNode );
        }
        void MoveNode( TreeNode position, TreeNode source ) {
            if ( ParentContainer( source, position ) || position == source )
                return;
            if ( position.Tag is string && !Directory.Exists( position.Tag as string ) ) {
                if ( source.Tag is string ) {
                    string newtag = ToRightPath( GetNodePath( position.Parent ) + source.Text );
                    File.Move( source.Tag as string, newtag );
                    source.Tag = newtag;
                    source.Remove( );
                    position.Parent.Nodes.Insert( 0, source );
                } else {
                    ChangeTags( source, GetNodePath( source ), ToRightPath( GetNodePath( position.Parent ) + source.Text ) );
                    Directory.Move( GetNodePath( source ), ToRightPath( GetNodePath( position.Parent ) + source.Text ) );
                    source.Remove( );
                    position.Parent.Nodes.Insert( 0, source );
                }
                return;
            } else {
                if ( source.Tag is string ) {
                    string newtag = ToRightPath( GetNodePath( position ) + source.Text );
                    File.Move( source.Tag as string, newtag );
                    source.Tag = newtag;
                    source.Remove( );
                    position.Nodes.Insert( 0, source );
                } else {
                    ChangeTags( source, GetNodePath( source ), ToRightPath( GetNodePath( position ) + source.Text ) );
                    Directory.Move( GetNodePath( source ), ToRightPath( GetNodePath( position ) + source.Text ) );
                    source.Remove( );
                    position.Nodes.Insert( 0, source );
                }
                return;
            }
        }
        void ChangeTags( TreeNode folder, string @new, string old ) {
            foreach ( TreeNode node in folder.Nodes )
                if ( node.Tag is string )   //File
                    node.Tag = ( ( string )node.Tag ).Replace( old, @new );
                else                        //Folder
                    ChangeTags( node, @new, old );
        }
        private void treeView1_DragDrop( object sender, DragEventArgs e ) {
            if ( e.Data.GetDataPresent( typeof( TreeNode ) ) ) {
                MoveNode( treeView1.GetNodeAt( treeView1.PointToClient( new Point( e.X, e.Y ) ) ), e.Data.GetData( typeof( TreeNode ) ) as TreeNode );
                e.Effect = DragDropEffects.None;
            } else {
                getContent( ( e.Data.GetData( DataFormats.FileDrop ) as string[ ] ), treeView1.GetNodeAt( treeView1.PointToClient( new Point( e.X, e.Y ) ) ) );
                e.Effect = DragDropEffects.None;
            }
        }
        void DirectoryCopy( string x, string y, string z = "" ) {
            if ( z == "" )
                z = y;
            try {
                if ( !Directory.Exists( y ) )
                    Directory.CreateDirectory( y );
                string[ ] files = Directory.GetFiles( x );
                string[ ] dirs = Directory.GetDirectories( x );
                foreach ( string file in files )
                    File.Copy( file, y + Path.GetFileName( file ) );
                foreach ( string dir in dirs )
                    DirectoryCopy( dir, y + Path.GetFileName( dir ) );
            } catch ( DirectoryNotFoundException ) {
                throw;
            } catch {
                Directory.Delete( z );
                throw new DirectoryNotFoundException( );
            }
        }
        bool ParentContainer( TreeNode container, TreeNode contains ) {
            if ( container == contains )
                return true;
            if ( contains.Parent == null )
                return false;
            return ParentContainer( container, contains.Parent );
        }
        void getContent( string[ ] Files, TreeNode drop ) {
            foreach ( string f in Files ) {
                string path = ToRightPath( GetNodePath( drop ) + "\\" + Path.GetFileName( f ) );
                string name = path.Substring( path.LastIndexOf( "\\" ) + 1 );
                TreeNode node = new TreeNode( );
                node.Tag = path;
                node.Text = name;
                File.Copy( f, path );
                drop.Nodes.Add( node );
            }
        }
        private void treeView1_DragEnter( object sender, DragEventArgs e ) {
            foreach ( string str in e.Data.GetFormats( ) )
                if ( str == "System.Windows.Forms.TreeNode" ) {
                    e.Effect = DragDropEffects.Move;
                    return;
                }
            if ( e.Data.GetDataPresent( DataFormats.FileDrop ) )
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }
        private void treeView1_KeyDown( object sender, KeyEventArgs e ) {
            if ( treeView1.SelectedNode == null )
                return;
            if ( e.KeyCode == Keys.Enter ) {
                if ( !e.Alt )
                    openToolStripMenuItem_Click( null, null );
                //else if ( ( ( ( ViewEditor.SelectedNode ) as DataNode ).Values[ 0 ] as TabPage ).Controls.Count != 0 )
                //    Openinnew( ( TabPage )( ( ( DataNode )ViewEditor.SelectedNode ).Values[ 0 ] ) );
            }
            if ( e.KeyCode == Keys.Delete ) {
                deleteToolStripMenuItem_Click( null, null );
            }
            if ( e.Alt && e.KeyCode == Keys.S ) {
                saveToolStripMenuItem1_Click( null, null );
            }
            if ( e.Alt && e.KeyCode == Keys.L ) {
                loadToolStripMenuItem_Click( null, null );
            }
            if ( e.KeyCode == Keys.F2 ) {
                editToolStripMenuItem_Click( null, null );
            }
            e.Handled = true;
        }
        private void openToolStripMenuItem_Click( object sender, EventArgs e ) {
            fbd.ShowNewFolderButton = false;
            if ( fbd.ShowDialog( ) == System.Windows.Forms.DialogResult.OK ) {
                LoadProject( fbd.SelectedPath );
            }
        }
        private void openToolStripMenuItem1_Click( object sender, EventArgs e ) {
            //TODO: Open In Another Window
        }
        private void openToolStripMenuItem2_Click( object sender, EventArgs e ) {
            treeView1_MouseDoubleClick( null, null );
        }
        private void deleteToolStripMenuItem_Click( object sender, EventArgs e ) {
            if ( treeView1.SelectedNode.Tag == null || treeView1.SelectedNode.Parent == null ) {
                removeToolStripMenuItem_Click( null, null );
                return;
            }
            File.Delete( ( string )treeView1.SelectedNode.Tag );
            treeView1.SelectedNode.Remove( );
        }
        private void saveToolStripMenuItem1_Click( object sender, EventArgs e ) {
            if ( treeView1.SelectedNode.Parent != null ) {
                SFD.Filter = "VarocalCross file (*.vc)|*.vc|Anyfile (*.*)|*.*";
                if ( SFD.ShowDialog( ) == DialogResult.OK ) {
                    File.Copy( ( string )treeView1.SelectedNode.Tag, SFD.FileName );
                }
            }
        }
        private void loadToolStripMenuItem_Click( object sender, EventArgs e ) {
            OFD.Filter = "VarocalCross file (*.vc)|*.vc|Anyfile (*.*)|*.*";
            if ( OFD.ShowDialog( ) == DialogResult.OK ) {
                File.Delete( ( string )treeView1.SelectedNode.Tag );
                File.Copy( SFD.FileName, ( string )treeView1.SelectedNode.Tag );
            }
        }
        private void editToolStripMenuItem_Click( object sender, EventArgs e ) {
            treeView1.LabelEdit = true;
            treeView1.SelectedNode.BeginEdit( );
        }
        private void treeView1_AfterCollapse( object sender, TreeViewEventArgs e ) {
            e.Node.ImageIndex = 0;
            e.Node.SelectedImageIndex = 0;
            if ( e.Node.Parent == null ) {
                e.Node.Expand( );
            }
        }
        private void treeView1_AfterExpand( object sender, TreeViewEventArgs e ) {
            e.Node.ImageIndex = 1;
            e.Node.SelectedImageIndex = 1;
        }
        private void addFolderToolStripMenuItem_Click( object sender, EventArgs e ) {
            string path = ToRightPath( GetNodePath( treeView1.SelectedNode ) + "folder" );
            Directory.CreateDirectory( path );
            TreeNode node = new TreeNode( Path.GetFileName( path ) );
            node.ContextMenuStrip = ProjectMenuStrip;
            treeView1.SelectedNode.Nodes.Add( node );
            treeView1.SelectedNode.Expand( );
        }
        private void optionsToolStripMenuItem_Click( object sender, EventArgs e ) {
            preferencesWindow.ShowDialog( );
            ColorUpdate( );
            preferencesWindow.Save( );
        }
        void ColorUpdate( ) {
            ( keywordStyle as TextStyle ).FontStyle = ( preferencesWindow.bold[ 0 ] ? FontStyle.Bold : FontStyle.Regular ) | ( preferencesWindow.italic[ 0 ] ? FontStyle.Italic : FontStyle.Regular );
            ( keywordStyle as TextStyle ).ForeBrush = new SolidBrush( preferencesWindow.colors[ 0 ] );

            ( valueStyle as TextStyle ).FontStyle = ( preferencesWindow.bold[ 1 ] ? FontStyle.Bold : FontStyle.Regular ) | ( preferencesWindow.italic[ 1 ] ? FontStyle.Italic : FontStyle.Regular );
            ( valueStyle as TextStyle ).ForeBrush = new SolidBrush( preferencesWindow.colors[ 1 ] );

            ( commentStyle as TextStyle ).FontStyle = ( preferencesWindow.bold[ 2 ] ? FontStyle.Bold : FontStyle.Regular ) | ( preferencesWindow.italic[ 2 ] ? FontStyle.Italic : FontStyle.Regular );
            ( commentStyle as TextStyle ).ForeBrush = new SolidBrush( preferencesWindow.colors[ 2 ] );

            ( objectStyle as TextStyle ).FontStyle = ( preferencesWindow.bold[ 3 ] ? FontStyle.Bold : FontStyle.Regular ) | ( preferencesWindow.italic[ 3 ] ? FontStyle.Italic : FontStyle.Regular );
            ( objectStyle as TextStyle ).ForeBrush = new SolidBrush( preferencesWindow.colors[ 3 ] );

            foreach ( FATabStripItem item in _TabControl1.Items )
                if ( item.Tag is string ) {
                    FastColoredTextBox fast = ( ( FastColoredTextBox )item.Controls[ 0 ] );
                    fast.SelectionStyle.BackgroundBrush = new SolidBrush( preferencesWindow.colors[ 4 ] );
                    fast.Font = preferencesWindow.f;
                    fast.OnTextChanged( new TextChangedEventArgs( fast.Range ) );
                }
        }
        void SaveTab( FATabStripItem item ) {
            if ( item.Tag is string ) {
                FormProperties fp = default( FormProperties );
                bool setted = false;
                TreeNode project=default(TreeNode);
                    foreach ( TreeNode node in treeView1.Nodes )
                        if ( ( item.Tag as string ).Contains( node.Tag as string ) ) {
                            project = node;
                            break;
                        }
                    foreach ( FATabStripItem item2 in _TabControl1.items ) {
                        if ( item2.Controls[ 0 ] is FormProperties && !setted ) {
                            if ( ( ( ( FormProperties )item2.Controls[ 0 ] ).Path ).StartsWith( project.Tag as string ) ) {
                                fp = ( FormProperties )item2.Controls[ 0 ];
                                setted = true;
                                break;
                            }
                        }
                }
                if ( !setted )
                    fp = new FormProperties( Path.Combine( project.Tag as string, "projectdata.dat" ) );
                if ( !item.Saved )
                    fp.ver_4.Value += 1;
                fp.MemoryCore.Save( Path.Combine( project.Tag as string, "projectdata.dat" ) );
                try {
                    ( ( FATabStripItem )fp.Parent ).Saved = true;
                } catch {
                }
                string filename = item.Tag as string;
                if ( File.Exists( filename ) )
                    File.Delete( filename );
                File.WriteAllText( filename, ( ( FastColoredTextBox )item.Controls[ 0 ] ).Text );
                item.Saved = true;
            } else if ( item.Controls[ 0 ] is FormProperties ) {
                FormProperties properties = ( FormProperties )item.Controls[ 0 ];
                properties.MemoryCore.Save( properties.Path );
                item.Saved = true;
            }
        }
        private void buildProjectToolStripMenuItem1_Click( object sender, EventArgs e ) {
            DateTime dt = DateTime.Now;
            TreeNode project = null;
            if ( _TabControl1.Items.Count == 0 ) {
                if ( treeView1.Nodes.Count == 0 ) {
                    MessageBox.Show( this,"To build one you should open a project (See File->Open or File->New)" );
                    return;
                }
                project = treeView1.Nodes[ 0 ];
            } else {
                _TabControl1.Invalidate( );
                foreach ( TreeNode node in treeView1.Nodes )
                    if ( _TabControl1.SelectedItem.Tag is string ) {
                        if ( ( _TabControl1.SelectedItem.Tag as string ).Contains( node.Tag as string ) ) {
                            project = node;
                            break;
                        }
                    } else if ( _TabControl1.SelectedItem.Controls[ 0 ] is FormProperties ) {
                        if ( ( ( ( FormProperties )_TabControl1.SelectedItem.Controls[ 0 ] ).Path ).Contains( node.Tag as string ) ) {
                            project = node;
                            break;
                        }
                    }
                if ( project == null ) {
                    MessageBox.Show( this,"To build one you should open a project (See File->Open or File->New)" );
                    return;
                }
                FormProperties fp = default( FormProperties );
                bool setted = false;
                foreach ( FATabStripItem item in _TabControl1.items ) {
                        if ( item.Controls[ 0 ] is FormProperties && !setted ) {
                            if ( ( ( ( FormProperties )item.Controls[ 0 ] ).Path ).StartsWith( project.Tag as string ) ) {
                                fp = ( FormProperties )item.Controls[ 0 ];
                                setted = true;
                            }
                        }
                    SaveTab( item );
                }
                if ( !setted )
                    fp = new FormProperties( Path.Combine( project.Tag as string, "projectdata.dat" ) );
                fp.ver_3.Value += 1;
                fp.MemoryCore.Save( Path.Combine( project.Tag as string, "projectdata.dat" ) );
                try {
                    ( ( FATabStripItem )fp.Parent ).Saved = true;
                } catch {
                }
            }
            List<TreeNode> ls = new List<TreeNode>( );
            ls.Add( project );
            BuildResult result = null;
            FormProperties prop = new FormProperties( Path.Combine( project.Tag as string, "projectdata.dat" ) );
            Package package = new Package( );
            string cmpdir = "cmpdir";
            if ( prop.ProjectType.Text == "Package" ) {
                package.ID = random.Next( );
                cmpdir = "src\\pkg\\" + package.ID;
            }
            while ( ls.Count != 0 ) {
                TreeNode node = ls[ 0 ];
                ls.RemoveAt( 0 );
                if ( node.Parent == null || node.Tag == null ) {
                    foreach ( TreeNode node2 in node.Nodes )
                        ls.Add( node2 );
                    continue;
                }
                string path = node.Tag as string;
                if ( path.EndsWith( ".cs" ) || path.EndsWith( ".go" ) || path.EndsWith( ".c" ) )
                    File.Copy( Path.GetFileName( path ), path );
                else
                    build.AddSource( File.ReadAllText( path ) );
            }
            string bat = Path.GetRandomFileName( ) + ".cmd";
            try {
                ProcessStartInfo psi = new ProcessStartInfo( "cmd", "/C " + bat );
                psi.CreateNoWindow = true;
                psi.UseShellExecute = true;
                File.WriteAllText( bat, prop.PreBuildText.Text );
                Process.Start( psi ).WaitForExit( );
                List<String> packages = new List<string>( );
                foreach ( DataGridViewRow row in prop.packs.Rows ) {
                    packages.Add( Path.GetFullPath( Path.Combine( "pkg\\" + Build.GeneratePlatform( ( prop.cmb_platform.SelectedIndex == 0 ? Build.Platform.Windows :
            ( prop.cmb_platform.SelectedIndex == 1 ? Build.Platform.Linux : Build.Platform.Mac ) ),
            prop.ProjectSet.Text.Contains( "64" ) ), ( string )row.Cells[ 2 ].Value ) + ".obj" ) );
                }
                build.InterpretTo( cmpdir, out result, prop.ProjectType.Text == "Package"
                    , prop.txt_name.Text, package.ID );
                if ( result.Type != BuildResultType.None ) {
                    Parser.Source source = new Parser.Source( );
                    foreach ( Parser.CodeErrorException exception in ( Parser.CodeErrorException[ ] )result.Result )
                        source.AppendLine( exception.Message );
                    MessageBox.Show(this, source.Content );
                    if ( File.Exists( bat ) )
                        File.Delete( bat );
                    File.WriteAllText( bat, prop.PostBuildText.Text );
                    Process.Start( psi ).WaitForExit( );
                    return;
                } else {
                    build.Linking( cmpdir, out result, ( int )prop.WarnLvl.Value, prop.WarnAsErr.Checked,
                        prop.ProjectType.Text == "Package", packages, prop.txt_name.Text );
                    if ( result.Type != BuildResultType.None ) {
                        Parser.Source source = new Parser.Source( );
                        foreach ( CompilerError error in ( CompilerError[ ] )result.Result )
                            source.AppendLine( error.ErrorText );
                        MessageBox.Show(this, source.Content );
                        if ( File.Exists( bat ) )
                            File.Delete( bat );
                        File.WriteAllText( bat, prop.PostBuildText.Text );
                        Process.Start( psi ).WaitForExit( );
                        return;
                    } else {
                        build.Analyzing( cmpdir, prop.ProjectType.Text == "Package", prop.txt_name.Text, packages,
                            ( prop.cmb_platform.SelectedIndex == 0 ? Build.Platform.Windows :
                    ( prop.cmb_platform.SelectedIndex == 1 ? Build.Platform.Linux : Build.Platform.Mac ) ), prop.ProjectSet.Text.Contains( "64" )
                    , cmpdir, package.ID );
                        string output = build.Compiling( cmpdir, out result, prop.UPXComp.Checked,
                            ( prop.cmb_platform.SelectedIndex == 0 ? Build.Platform.Windows :
                    ( prop.cmb_platform.SelectedIndex == 1 ? Build.Platform.Linux : Build.Platform.Mac ) ), prop.ProjectSet.Text.Contains( "64" ), prop.ProjectType.Text == "Package" );
                        if ( result.Type != BuildResultType.None ) {
                            Parser.Source source = new Parser.Source( );
                            foreach ( CompileError error in ( CompileError[ ] )result.Result )
                                source.AppendLine( error.FullText );
                            MessageBox.Show(this, source.Content );
                            if ( File.Exists( bat ) )
                                File.Delete( bat );
                            File.WriteAllText( bat, prop.PostBuildText.Text );
                            Process.Start( psi ).WaitForExit( );
                            return;
                        }
                        if ( prop.ProjectType.Text == "Package" ) {
                            if ( Directory.Exists( "pkgcmp" ) )
                                Directory.Delete( "pkgcmp", true );
                            Directory.CreateDirectory( "pkgcmp" );
                            var v = new ZipForge( );
                            v.FileName = Path.GetFullPath( Path.Combine( project.Tag as string, "build\\release\\" + prop.txt_name.Text + ".vpak" ) );
                            v.OpenArchive( System.IO.FileMode.Create );
                            v.BaseDir = Path.GetFullPath( cmpdir );
                            package.Name = prop.txt_name.Text;
                            package.PlatformString = Environment.GetEnvironmentVariable( "GOOS" ) + "_" + Environment.GetEnvironmentVariable( "GOARCH" );
                            package.Platform = ( prop.cmb_platform.SelectedIndex == 0 ? Build.Platform.Windows :
                    ( prop.cmb_platform.SelectedIndex == 1 ? Build.Platform.Linux : Build.Platform.Mac ) );
                            package.is64X = prop.ProjectSet.Text.Contains( "64" );
                            package.DefaultNamespace = prop.txt_namespace.Text;
                            package.Description = prop.txt_description.Text;
                            package.Version = string.Join( ".", new string[ ] {
                            prop.ver_1.Value.ToString( ),
                            prop.ver_2.Value.ToString( ),
                            prop.ver_3.Value.ToString( ),
                            prop.ver_4.Value.ToString( ) } );
                            MemorizePortable mem = new MemorizePortable( );
                            mem.AddOwner( package );
                            mem.Save( Path.Combine( v.BaseDir, "install.dat" ) );
                            File.Move( output, Path.Combine( v.BaseDir, prop.txt_name.Text + ".a" ) );
                            foreach ( string file in Directory.GetFiles( v.BaseDir,"*.go" ) ) {
                                string newcode = PackageInstallation.CompressGo( file );
                                File.Delete( file );
                                File.WriteAllText( file, newcode );
                            }
                            v.AddFiles( Path.Combine( v.BaseDir, "*.*" ) );
                            v.CloseArchive( );
                        } else {
                            string exe = Path.Combine( project.Tag as string, "build\\release\\" + prop.txt_name.Text + ".exe" );
                            if ( File.Exists( exe ) )
                                File.Delete( exe );
                            File.Move( output, exe );
                        }
                    }
                }
                if ( File.Exists( bat ) )
                    File.Delete( bat );
                File.WriteAllText( bat, prop.PostBuildText.Text );
                Process.Start( psi ).WaitForExit( );
                MessageBox.Show( this,"Build's time: " + ( DateTime.Now - dt ).TotalMilliseconds );
            } finally {
                File.Delete( bat );
                if ( Directory.Exists( "pkgcmp" ) )
                    Directory.Delete( "pkgcmp", true );
                Directory.Delete( cmpdir, true );
            }
        }
        private void cMagnetToolStripMenuItem_Click( object sender, EventArgs e ) {
            OFD.Filter = "Header files (*.h)|*.h|Anyfile (*.*)|*.*";
            OFD.Multiselect = true;
            try {
                if ( OFD.ShowDialog( ) == System.Windows.Forms.DialogResult.OK ) {
                    foreach ( string file in OFD.FileNames ) {
                        CMagnet( file );
                    }
                }
            } catch {
                MessageBox.Show(this, "Are you sure path contains 'include'?" );
            } finally {
                OFD.Multiselect = false;
            }
        }
        void CMagnet( string path ) {
            string basepath = path.Substring( path.IndexOf( "\\include\\" ) + "\\include\\".Length )
                , dirpath = new FileInfo( path ).DirectoryName
                , incpath = path.Remove( path.IndexOf( "\\include\\" ) + "\\include\\".Length );
            if ( File.Exists( Path.Combine( "include", basepath ) ) )
                return;
            File.Copy( path, Path.Combine( "include", basepath ) );
            foreach ( string _line in File.ReadAllLines( path ) ) {
                string line = _line.Trim( );
                if ( line.StartsWith( "#include <" ) || line.StartsWith( "#include<" ) ) {
                    line = line.Substring( line.IndexOf( '<' ) + 1 );
                    line = line.Remove( line.IndexOf( '>' ) );
                    CMagnet( Path.Combine( incpath, line ) );
                } else if ( line.StartsWith( "#include \"" ) || line.StartsWith( "#include\"" ) ) {
                    line = line.Substring( line.IndexOf( '"' ) + 1 );
                    line = line.Remove( line.IndexOf( '"' ) );
                    CMagnet( Path.Combine( basepath, line ) );
                }
            }
        }
        private void nodeMenuStrip_Opening( object sender, CancelEventArgs e ) {
            removeToolStripMenuItem.Text = treeView1.SelectedNode.Parent == null ? "Remove" : "Delete";
        }
        private void removeToolStripMenuItem_Click( object sender, EventArgs e ) {
            if ( treeView1.SelectedNode.Parent == null ) {
                treeView1.SelectedNode.Remove( );
            } else {
                try {
                    Directory.Delete( GetNodePath( treeView1.SelectedNode ), true );
                    treeView1.SelectedNode.Remove( );
                } catch {
                    MessageBox.Show( this,"Error while deleting folder", "IO ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error );
                }
            }
        }
        private void importTemplateToolStripMenuItem_Click( object sender, EventArgs e ) {
            TemplateImportResult result = new TemplateImportResult( );
            new TemplateImport( result ).ShowDialog( );
            TreeNode project = null;
            if ( !result.Cancelled ) {
                if ( _TabControl1.Items.Count == 0 ) {
                    if ( treeView1.Nodes.Count == 0 ) {
                        MessageBox.Show(this,  "To build one you should open a project (See File->Open or File->New)" );
                        return;
                    }
                    project = treeView1.Nodes[ 0 ];
                } else {
                    foreach ( FATabStripItem item in _TabControl1.items ) {
                        if ( item.Tag is string ) {
                            string filename = item.Tag as string;
                            if ( File.Exists( filename ) )
                                File.Delete( filename );
                            File.WriteAllText( filename, ( ( FastColoredTextBox )item.Controls[ 0 ] ).Text );
                            item.Saved = true;
                        }
                    }
                    _TabControl1.Invalidate( );
                    foreach ( TreeNode node in treeView1.Nodes )
                        if ( ( _TabControl1.SelectedItem.Tag as string ).Contains( node.Tag as string ) ) {
                            project = node;
                            break;
                        }
                }
                foreach ( string import in result.importfrom )
                    DirectoryCopy( "templates\\" + import, project.Tag as string );
            }
        }
        private void exportTemplateToolStripMenuItem_Click( object sender, EventArgs e ) {
            TemplateNameResult result = new TemplateNameResult( );
            new TemplateName( result ).ShowDialog( );
            TreeNode project = null;
            if ( !result.Cancelled ) {
                if ( _TabControl1.Items.Count == 0 ) {
                    if ( treeView1.Nodes.Count == 0 ) {
                        MessageBox.Show(this, "To build one you should open a project (See File->Open or File->New)" );
                        return;
                    }
                    project = treeView1.Nodes[ 0 ];
                } else {
                    foreach ( FATabStripItem item in _TabControl1.items ) {
                        if ( item.Tag is string ) {
                            string filename = item.Tag as string;
                            if ( File.Exists( filename ) )
                                File.Delete( filename );
                            File.WriteAllText( filename, ( ( FastColoredTextBox )item.Controls[ 0 ] ).Text );
                            item.Saved = true;
                        }
                    }
                    _TabControl1.Invalidate( );
                    foreach ( TreeNode node in treeView1.Nodes )
                        if ( ( _TabControl1.SelectedItem.Tag as string ).Contains( node.Tag as string ) ) {
                            project = node;
                            break;
                        }
                }
                DirectoryCopy( project.Tag as string, "templates\\"+result.Name );
            }
        }
        private void ProjectMenuStrip_Opening( object sender, CancelEventArgs e ) {
            if ( treeView1.SelectedNode.Parent == null ) {
                propertiesToolStripMenuItem.Visible = true;
                removeToolStripMenuItem.Text = "Remove";
            } else {
                propertiesToolStripMenuItem.Visible = false;
                removeToolStripMenuItem.Text = "Delete";
            }
        }
        private void propertiesToolStripMenuItem_Click( object sender, EventArgs e ) {
            FATabStripItem item=new FATabStripItem();
            item.Title = "Properties";
            item.Controls.Add( new FormProperties( Path.Combine( treeView1.SelectedNode.Tag as string, "projectdata.dat" ) ) );
            _TabControl1.Items.Add( _TabControl1.SelectedItem = item );
        }
        private void saveToolStripMenuItem_Click( object sender, EventArgs e ) {
            if ( _TabControl1.SelectedItem != null )
                SaveTab( _TabControl1.SelectedItem );
        }
        private void saveAllToolStripMenuItem_Click( object sender, EventArgs e ) {
            foreach ( FATabStripItem item in _TabControl1.Items )
                SaveTab( item );
        }
        private void Form1_KeyDown( object sender, KeyEventArgs e ) {
            if ( e.Shift && e.Control && e.KeyCode == Keys.S )
                saveAllToolStripMenuItem_Click( null, null );
            else if ( e.Control && e.KeyCode == Keys.S )
                saveToolStripMenuItem_Click( null, null );
            else if ( e.Control && e.KeyCode == Keys.N )
                projectToolStripMenuItem1_Click( null, null );
            else if ( e.Control && e.KeyCode == Keys.O )
                projectToolStripMenuItem1_Click( null, null );
            else if ( e.Control && e.KeyCode == Keys.F5 )
                runReleaseToolStripMenuItem_Click( null, null );
        }
        private void installPackageToolStripMenuItem_Click( object sender, EventArgs e ) {
            OFD.Filter = "Varocal Package File (*.vpak)|*.vpak|Anyfile (*.*)|*.*";
            if ( OFD.ShowDialog( )==System.Windows.Forms.DialogResult.OK ) {
                var v = new ZipForge( );
                v.FileName = OFD.FileName;
                v.OpenArchive( System.IO.FileMode.Open );
                v.BaseDir = Path.GetFullPath( "pkgcmp" );
                v.ExtractFiles("*.*");
                new Thread( new ThreadStart( ( ) => {
                    new PackageInstallation( "pkgcmp" ).ShowDialog( );
                } ) ).Start( );
            }
        }
        private void packageManagerToolStripMenuItem_Click( object sender, EventArgs e ) {
            new PackageManager( OFD ).ShowDialog( this );
        }
        private void runReleaseToolStripMenuItem_Click( object sender, EventArgs e ) {
            TreeNode project = null;
            FormProperties fp = default( FormProperties );
            if ( _TabControl1.Items.Count == 0 ) {
                if ( treeView1.Nodes.Count == 0 ) {
                    MessageBox.Show( this,"To build one you should open a project (See File->Open or File->New)" );
                    return;
                }
                project = treeView1.Nodes[ 0 ];
            } else {
                _TabControl1.Invalidate( );
                foreach ( TreeNode node in treeView1.Nodes )
                    if ( _TabControl1.SelectedItem.Tag is string ) {
                        if ( ( _TabControl1.SelectedItem.Tag as string ).Contains( node.Tag as string ) ) {
                            project = node;
                            break;
                        }
                    } else if ( _TabControl1.SelectedItem.Controls[ 0 ] is FormProperties ) {
                        if ( ( ( ( FormProperties )_TabControl1.SelectedItem.Controls[ 0 ] ).Path ).Contains( node.Tag as string ) ) {
                            project = node;
                            break;
                        }
                    }
                if ( project == null ) {
                    MessageBox.Show( this,"To build one you should open a project (See File->Open or File->New)" );
                    return;
                }
                
                bool setted = false;
                foreach ( FATabStripItem item in _TabControl1.items ) {
                        if ( item.Controls[ 0 ] is FormProperties && !setted ) {
                            if ( ( ( ( FormProperties )item.Controls[ 0 ] ).Path ).StartsWith( project.Tag as string ) ) {
                                fp = ( FormProperties )item.Controls[ 0 ];
                                setted = true;
                            }
                        }
                    SaveTab( item );
                }
                if ( !setted )
                    fp = new FormProperties( Path.Combine( project.Tag as string, "projectdata.dat" ) );
            }
            if ( File.Exists( Path.Combine( project.Tag as string, "Build\\Release\\" + fp.txt_name.Text + ".exe" ) ) ) {
                Process.Start( Path.Combine( project.Tag as string, "Build\\Release\\" + fp.txt_name.Text + ".exe" ) );
            }
        }
    }
    public class Package {
        public string Name {
            get;
            set;
        }
        public string Version {
            get;
            set;
        }
        public string DefaultNamespace {
            get;
            set;
        }
        public string Description {
            get;
            set;
        }
        public string PlatformString {
            get;
            set;
        }
        public int ID {
            get;
            set;
        }
        public Build.Platform Platform {
            get;
            set;
        }
        public bool is64X {
            get;
            set;
        }
    }
    class _TabControl : TabControl {
        #region Variables And Ctors
        Point mouse;
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer( );
        string drawstr = "";
        bool MouseIn = false;
        List<int> sTabThere = new List<int>( );
        List<int> sTabTo = new List<int>( );
        List<int> sTabIndex = new List<int>( );
        List<int> sTabDuration = new List<int>( );
        TabPage tppp;
        int xadd;
        bool moving = false;
        public _TabControl( )
            : base( ) {
            t.Interval = 25;
            t.Enabled = true;
            t.Tick += new EventHandler( t_Tick );
            this.Alignment = TabAlignment.Bottom;
            this.MouseMove += new MouseEventHandler( _TabControl_MouseMove );
            this.Disposed += new EventHandler( _TabControl_Disposed );
            this.MouseEnter += new EventHandler( _TabControl_MouseEnter );
            this.MouseLeave += new EventHandler( _TabControl_MouseLeave );
            this.SizeChanged += new EventHandler( _TabControl_SizeChanged );
            this.AllowDrop = true;
            this.Font = new Font( "Segoe UI", 8.5F );
        }
        #endregion

        void _TabControl_SizeChanged( object sender, EventArgs e ) {
            Message m = new Message( );
            m.Msg = 0x000F;
            WndProc( ref m );
        }
        void _TabControl_MouseLeave( object sender, EventArgs e ) {
            MouseIn = false;
        }
        void _TabControl_MouseEnter( object sender, EventArgs e ) {
            MouseIn = true;
        }
        void _TabControl_Disposed( object sender, EventArgs e ) {
            t.Dispose( );
        }
        void t_Tick( object sender, EventArgs e ) {
            if ( tppp != null ) {
                drawstr = "";
                t.Interval = 40;
                gmouse.X -= ( gmouse.X - mouse.X ) / 4;
                goto RunIt;
            }
            if ( sTabThere.Count != 0 )
                goto RightP;
            else
                goto RightQ;
        RightP:
            int c = sTabTo.Count;
            for ( int i = 0 ; i < c ; i++ ) {
                if ( sTabDuration[ i ]-- < 0 ) {
                    sTabDuration.RemoveAt( i );
                    sTabIndex.RemoveAt( i );
                    sTabThere.RemoveAt( i );
                    sTabTo.RemoveAt( i );
                    c--;
                    i--;
                    continue;
                }
                sTabThere[ i ] -= ( sTabThere[ i ] - sTabTo[ i ] ) / 4 + Math.Sign( sTabThere[ i ] - sTabTo[ i ] );
            }
            goto RunIt; //until there is no tabs to move
        RightQ:
            string newstr = "";
            newstr += mouseon;
            newstr += MouseIn;
            c = TabPages.Count;
            for ( int i = 0 ; i < c ; i++ ) {
                newstr += this.GetTabRect( i ) + TabPages[ i ].Text;
            }
            t.Interval = 250;
            if ( drawstr == newstr )
                return;
            drawstr = newstr;
        RunIt:
            Message m = new Message( );
            m.Msg = 0x000F;
            WndProc( ref m );
        }
        public int mouseon {
            get {
                int c = TabPages.Count;
                Rectangle r;
                for ( int i = 0 ; i < c ; i++ ) {
                    r = this.GetTabRect( i );
                    if ( r.IntersectsWith( new Rectangle( mouse, new System.Drawing.Size( 10, 10 ) ) ) ) {
                        return i;
                    }
                }
                return -1;
            }
        }
        void _TabControl_MouseMove( object sender, MouseEventArgs e ) {
            int i = mouseon;
            mouse = new Point( e.X, e.Y );
            if ( mouseon != i )
                t_Tick( null, null );
            if ( mmouse != null ) {
                if ( Math.Abs( mmouse.X - mouse.X ) + Math.Abs( mmouse.Y - mouse.Y ) > 8 )
                    moving = true;
            }
        }
        protected override void WndProc( ref Message m ) {
            base.WndProc( ref m );
            if ( m.Msg == 0x000F ) {
                if ( Visible && !IsDisposed ) {
                    Graphics g = CreateGraphics( );
                    g.FillRectangle( new SolidBrush( Color.FromArgb( 50, 50, 50 ) ), new Rectangle( new Point( 0, 0 ), new Size( Width, Height ) ) );
                    if ( TabPages.Count == 0 )
                        return;
                    g.FillRectangle( new SolidBrush( Color.FromArgb( 214, 214, 107 ) ), new Rectangle( new Point( 4, Height - ItemSize.Height - 2 ), new Size( Width - 8, 2 ) ) );
                    int c = TabPages.Count;
                    Rectangle r = new Rectangle( );
                    Brush set;
                    StringFormat sf = new StringFormat( );
                    sf.Alignment = StringAlignment.Center;
                    for ( int i = 0 ; i < c ; i++ ) {
                        if ( i == SelectedIndex )
                            continue;
                        r = this.GetTabRect( i );
                        r.Location = new Point( r.X + 2, r.Y );
                        int ind2 = 0;
                        foreach ( int ind in sTabIndex ) {
                            if ( ind == i ) {
                                r.X = sTabThere[ ind2 ];
                            }
                            ind2++;
                        }
                        set = new SolidBrush( Color.FromArgb( 50, 50, 50 ) );
                        int bcd = 150;
                        if ( r.IntersectsWith( new Rectangle( mouse, new System.Drawing.Size( 1, 1 ) ) ) && MouseIn ) {
                            set = new SolidBrush( Color.FromArgb( 100, 100, 100 ) );
                            if ( tppp != null )
                                set = new SolidBrush( Color.FromArgb( 100, 100, 50 ) );
                            bcd = 200;
                        }
                        g.FillPolygon( set, new Point[ ] { new Point( r.X, r.Y + 3 ), new Point( r.X + r.Width, r.Y + 3 ), new Point( r.X + r.Width - 5, r.Y + r.Height ), new Point( r.X + 5, r.Y + r.Height ) } );
                        g.DrawString( TabPages[ i ].Text, Font, new SolidBrush( Color.FromArgb( bcd, bcd, bcd ) ), new Point( r.X + r.Width / 2, r.Y + 3 ), sf );
                    }
                    if ( SelectedIndex != -1 && SelectedIndex < TabPages.Count ) {
                        r = this.GetTabRect( SelectedIndex );
                        int ind2 = 0;
                        foreach ( int ind in sTabIndex ) {
                            if ( ind == SelectedIndex ) {
                                r.X = sTabThere[ ind2 ];
                            }
                            ind2++;
                        }
                        if ( tppp != null && moving )
                            r.X = gmouse.X - xadd;
                        r.X = Math.Max( Math.Min( r.X + 2, Width - 4 - r.Width ), 2 );
                        set = new LinearGradientBrush( new Point( r.Width / 2, 0 ), new Point( r.Width / 2, r.Height / 2 ), Color.FromArgb( ( tppp == null ? 255 : 128 ), 200, 200, 100 ), Color.FromArgb( ( tppp == null ? 255 : 128 ), 225, 225, 112 ) );
                        g.FillPolygon( set, new Point[ ] { new Point( r.X, r.Y ), new Point( r.X + r.Width, r.Y ), new Point( r.X + r.Width - 5, r.Y + r.Height ), new Point( r.X + 5, r.Y + r.Height ) } );
                        Color txtcol = Color.FromArgb( 196, 100, 100, 50 );
                        int txtspc = 1;
                        g.DrawString( TabPages[ SelectedIndex ].Text, Font, new SolidBrush( txtcol ), new Point( r.X + r.Width / 2 + txtspc, r.Y + 3 ), sf );
                        g.DrawString( TabPages[ SelectedIndex ].Text, Font, new SolidBrush( txtcol ), new Point( r.X + r.Width / 2 - txtspc, r.Y + 3 ), sf );
                        g.DrawString( TabPages[ SelectedIndex ].Text, Font, new SolidBrush( txtcol ), new Point( r.X + r.Width / 2, r.Y + 3 - txtspc ), sf );
                        g.DrawString( TabPages[ SelectedIndex ].Text, Font, new SolidBrush( txtcol ), new Point( r.X + r.Width / 2, r.Y + 3 + txtspc ), sf );
                        g.DrawString( TabPages[ SelectedIndex ].Text, Font, new SolidBrush( Color.FromArgb( 70, 70, 70 ) ), new Point( r.X + r.Width / 2, r.Y + 3 ), sf );
                    }
                }
            }
        }
        protected override void OnMouseUp( MouseEventArgs e ) {
            base.OnMouseUp( e );
            if ( tppp == null )
                return;
            int iii = mouseon;
            if ( iii == TabPages.IndexOf( tppp ) || iii == -1 ) {
                sTabIndex.Add( TabPages.IndexOf( tppp ) );
                sTabThere.Add( mouse.X - xadd );
                sTabDuration.Add( 25 );
                sTabTo.Add( GetTabRect( TabPages.IndexOf( tppp ) ).Left );
                tppp = null;
                return;
            }
            tppp.AccessibleName = "Dragging";
            TabControl tc = new TabControl( );
            SelectedIndex = 0;
            tc.TabPages.Add( tppp );
            TabPages.Insert( iii, tppp );
            tc.Dispose( );
            SelectedIndex = iii;
            tppp.AccessibleName = "";
            tppp = null;
        }
        Point mmouse;
        Point gmouse;
        protected override void OnMouseDown( MouseEventArgs e ) {
            base.OnMouseDown( e );
            if ( mouseon == -1 )
                return;
            tppp = TabPages[ mouseon ];
            xadd = mouse.X - GetTabRect( mouseon ).X;
            moving = false;
            mmouse = mouse;
            gmouse = mmouse;
        }
    }
    class DataNode : TreeNode {
        public DataNode( )
            : base( ) {
            Data = new List<string>( );
        }
        public List<String> Data {
            get;
            set;
        }
    }
}