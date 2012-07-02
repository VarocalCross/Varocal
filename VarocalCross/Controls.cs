using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Forms.VisualStyles;
using System.Xml;

namespace VarocalCross {
    #region FastColoredTextBox
    public class FastColoredTextBox : UserControl {

        #region public Variables
        const int minLeftIndent = 8;
        const int maxBracketSearchIterations = 1000;
        public readonly CommandManager manager = new CommandManager( );
        Keys lastModifiers;
        bool wordWrap;
        WordWrapMode wordWrapMode = WordWrapMode.WordWrapControlWidth;
        int wordWrapLinesCount;
        Range selection;
        FindForm findForm;
        ReplaceForm replaceForm;
        int charHeight;
        int startFoldingLine = -1;
        int endFoldingLine = -1;
        Range leftBracketPosition = null;
        Range rightBracketPosition = null;
        Range leftBracketPosition2 = null;
        Range rightBracketPosition2 = null;
        Range leftBracketPosition3 = null;
        Range rightBracketPosition3 = null;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer( );
        System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer( );
        bool needRiseTextChangedDelayed;
        bool needRiseSelectionChangedDelayed;
        bool needRiseVisibleRangeChangedDelayed;
        Range delayedTextChangedRange;
        readonly List<VisualMarker> visibleMarkers = new List<VisualMarker>( );
        public List<Line> lines = new List<Line>( );
        public bool needRecalc;
        Language language;
        readonly Dictionary<FontStyle, Font> fontsByStyle = new Dictionary<FontStyle, Font>( );
        int TopIndent {
            get;
            set;
        }
        int lineInterval;
        bool isChanged;
        bool showLineNumbers;
        int leftIndent;
        int preferredLineWidth;
        Color lineNumberColor;
        Color indentBackColor;
        Color serviceLinesColor;
        Color foldingIndicatorColor;
        Color currentLineColor;
        Color changedLineColor;
        uint lineNumberStartValue;
        bool highlightFoldingIndicator;
        string descriptionFile;
        int leftPadding;
        int lastLineUniqueId;
        int changedLineWidth = -1;


        /// <summary>
        /// Background color for current line
        /// </summary>
        [DefaultValue( typeof( Color ), "Transparent" )]
        [Description( "Background color for current line. Set to Color.Transparent to hide current line highlighting" )]
        public Color CurrentLineColor {
            get {
                return currentLineColor;
            }
            set {
                currentLineColor = value;
                Invalidate( );
            }
        }

        /// <summary>
        /// Background color for highlighting of changed lines
        /// </summary>
        [DefaultValue( typeof( Color ), "Transparent" )]
        [Description( "Background color for highlighting of changed lines. Set to Color.Transparent to hide changed line highlighting" )]
        public Color ChangedLineColor {
            get {
                return changedLineColor;
            }
            set {
                changedLineColor = value;
                Invalidate( );
            }
        }

        /// <summary>
        /// Height of char in pixels
        /// </summary>
        [Description( "Height of char in pixels" )]
        public int CharHeight {
            get {
                return charHeight;
            }
            set {
                charHeight = value;
                OnCharSizeChanged( );
            }
        }

        /// <summary>
        /// Interval between lines (in pixels)
        /// </summary>
        [Description( "Interval between lines in pixels" )]
        [DefaultValue( 0 )]
        public int LineInterval {
            get {
                return lineInterval;
            }
            set {
                lineInterval = value;
                Font = Font;
                Invalidate( );
            }
        }

        /// <summary>
        /// Changed line's marker
        /// </summary>
        [Description( "Changed line's marker in pixels ,-1 is full" )]
        [DefaultValue( -1 )]
        public int ChangedLineWidth {
            get {
                return changedLineWidth;
            }
            set {
                changedLineWidth = value;
            }
        }

        /// <summary>
        /// Occurs when VisibleRange is changed
        /// </summary>
        public virtual void OnVisibleRangeChanged( ) {
            needRiseVisibleRangeChangedDelayed = true;
            ResetTimer( timer );
            if ( VisibleRangeChanged != null )
                VisibleRangeChanged( this, new EventArgs( ) );
        }

        /// <summary>
        /// Invalidates the entire surface of the control and causes the control to be redrawn.
        /// This method is thread safe and does not require Invoke.
        /// </summary>
        public new void Invalidate( ) {
            if ( InvokeRequired )
                BeginInvoke( new MethodInvoker( Invalidate ) );
            else
                base.Invalidate( );
        }

        public virtual void OnCharSizeChanged( ) {
            VerticalScroll.SmallChange = charHeight;
            VerticalScroll.LargeChange = 10 * charHeight;
            HorizontalScroll.SmallChange = CharWidth;
        }
        /// <summary>
        /// Width of char in pixels
        /// </summary>
        [Description( "Width of char in pixels" )]
        public int CharWidth {
            get;
            set;
        }
        /// <summary>
        /// Spaces count for tab
        /// </summary>
        [DefaultValue( 4 )]
        [Description( "Spaces count for tab" )]
        public int TabLength {
            get;
            set;
        }
        /// <summary>
        /// Text was changed
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public bool IsChanged {
            get {
                return isChanged;
            }
            set {
                if ( !value )
                    //clear line's IsChanged property
                    foreach ( var line in lines )
                        line.IsChanged = false;
                isChanged = value;
            }
        }
        /// <summary>
        /// Text version
        /// </summary>
        /// <remarks>This counter is incremented each time changes the text</remarks>
        [Browsable( false )]
        public int TextVersion {
            get;
            set;
        }
        /// <summary>
        /// Read only
        /// </summary>
        [DefaultValue( false )]
        public bool ReadOnly {
            get;
            set;
        }
        /// <summary>
        /// Shows line numbers.
        /// </summary>
        [DefaultValue( true )]
        [Description( "Shows line numbers." )]
        public bool ShowLineNumbers {
            get {
                return showLineNumbers;
            }
            set {
                showLineNumbers = value;
                Invalidate( );
            }
        }
        /// <summary>
        /// Color of line numbers.
        /// </summary>
        [DefaultValue( typeof( Color ), "Teal" )]
        [Description( "Color of line numbers." )]
        public Color LineNumberColor {
            get {
                return lineNumberColor;
            }
            set {
                lineNumberColor = value;
                Invalidate( );
            }
        }
        /// <summary>
        /// Start value of first line number.
        /// </summary>
        [DefaultValue( typeof( uint ), "1" )]
        [Description( "Start value of first line number." )]
        public uint LineNumberStartValue {
            get {
                return lineNumberStartValue;
            }
            set {
                lineNumberStartValue = value;
                needRecalc = true;
                Invalidate( );
            }
        }
        /// <summary>
        /// Background color of indent area
        /// </summary>
        [DefaultValue( typeof( Color ), "White" )]
        [Description( "Background color of indent area" )]
        public Color IndentBackColor {
            get {
                return indentBackColor;
            }
            set {
                indentBackColor = value;
                Invalidate( );
            }
        }
        /// <summary>
        /// Color of service lines (folding lines, borders of blocks etc.)
        /// </summary>
        [DefaultValue( typeof( Color ), "Silver" )]
        [Description( "Color of service lines (folding lines, borders of blocks etc.)" )]
        public Color ServiceLinesColor {
            get {
                return serviceLinesColor;
            }
            set {
                serviceLinesColor = value;
                Invalidate( );
            }
        }
        /// <summary>
        /// Color of folding area indicator
        /// </summary>
        [DefaultValue( typeof( Color ), "Green" )]
        [Description( "Color of folding area indicator" )]
        public Color FoldingIndicatorColor {
            get {
                return foldingIndicatorColor;
            }
            set {
                foldingIndicatorColor = value;
                Invalidate( );
            }
        }
        /// <summary>
        /// Enables folding indicator (left vertical line between folding bounds)
        /// </summary>
        [DefaultValue( true )]
        [Description( "Enables folding indicator (left vertical line between folding bounds)" )]
        public bool HighlightFoldingIndicator {
            get {
                return highlightFoldingIndicator;
            }
            set {
                highlightFoldingIndicator = value;
                Invalidate( );
            }
        }
        /// <summary>
        /// Left indent in pixels
        /// </summary>
        [Browsable( false )]
        [Description( "Left indent in pixels" )]
        public int LeftIndent {
            get {
                return leftIndent;
            }
            set {
                leftIndent = value;
            }
        }
        /// <summary>
        /// Left padding in pixels
        /// </summary>
        [DefaultValue( 0 )]
        [Description( "Left padding in pixels" )]
        public int LeftPadding {
            get {
                return leftPadding;
            }
            set {
                leftPadding = value;
                Invalidate( );
            }
        }
        /// <summary>
        /// This property draws vertical line after defined char position.
        /// Set to 0 for disable drawing of vertical line.
        /// </summary>
        [DefaultValue( 80 )]
        [Description( "This property draws vertical line after defined char position. Set to 0 for disable drawing of vertical line." )]
        public int PreferredLineWidth {
            get {
                return preferredLineWidth;
            }
            set {
                preferredLineWidth = value;
                Invalidate( );
            }
        }
        /// <summary>
        /// Styles
        /// Maximum style count is 16
        /// </summary>
        public readonly Style[ ] Styles = new Style[ sizeof( ushort ) * 8 ];
        /// <summary>
        /// Default text style
        /// This style is using when no one other TextStyle is not defined in Char.style
        /// </summary>
        [Browsable( false )]
        public TextStyle DefaultStyle {
            get;
            set;
        }
        /// <summary>
        /// Style for rendering Selection area
        /// </summary>
        [Browsable( false )]
        public SelectionStyle SelectionStyle {
            get;
            set;
        }
        /// <summary>
        /// Style for folded block rendering
        /// </summary>
        [Browsable( false )]
        public TextStyle FoldedBlockStyle {
            get;
            set;
        }
        /// <summary>
        /// Style for brackets highlighting
        /// </summary>
        [Browsable( false )]
        public MarkerStyle BracketsStyle {
            get;
            set;
        }
        /// <summary>
        /// Style for alternative brackets highlighting
        /// </summary>
        [Browsable( false )]
        public MarkerStyle BracketsStyle2 {
            get;
            set;
        }
        /// <summary>
        /// Style for alternative brackets highlighting
        /// </summary>
        [Browsable( false )]
        public MarkerStyle BracketsStyle3 {
            get;
            set;
        }
        /// <summary>
        /// Opening bracket for brackets highlighting.
        /// Set to '\x0' for disable brackets highlighting.
        /// </summary>
        [DefaultValue( '\x0' )]
        [Description( "Opening bracket for brackets highlighting. Set to '\\x0' for disable brackets highlighting." )]
        public char LeftBracket {
            get;
            set;
        }
        /// <summary>
        /// Closing bracket for brackets highlighting.
        /// Set to '\x0' for disable brackets highlighting.
        /// </summary>
        [DefaultValue( '\x0' )]
        [Description( "Closing bracket for brackets highlighting. Set to '\\x0' for disable brackets highlighting." )]
        public char RightBracket {
            get;
            set;
        }
        /// <summary>
        /// Alternative opening bracket for brackets highlighting.
        /// Set to '\x0' for disable brackets highlighting.
        /// </summary>
        [DefaultValue( '\x0' )]
        [Description( "Alternative opening bracket for brackets highlighting. Set to '\\x0' for disable brackets highlighting." )]
        public char LeftBracket2 {
            get;
            set;
        }
        /// <summary>
        /// Alternative closing bracket for brackets highlighting.
        /// Set to '\x0' for disable brackets highlighting.
        /// </summary>
        [DefaultValue( '\x0' )]
        [Description( "Alternative closing bracket for brackets highlighting. Set to '\\x0' for disable brackets highlighting." )]
        public char RightBracket2 {
            get;
            set;
        }
        /// <summary>
        /// Alternative opening bracket for brackets highlighting.
        /// Set to '\x0' for disable brackets highlighting.
        /// </summary>
        [DefaultValue( '\x0' )]
        [Description( "Alternative opening bracket for brackets highlighting. Set to '\\x0' for disable brackets highlighting." )]
        public char LeftBracket3 {
            get;
            set;
        }
        /// <summary>
        /// Alternative closing bracket for brackets highlighting.
        /// Set to '\x0' for disable brackets highlighting.
        /// </summary>
        [DefaultValue( '\x0' )]
        [Description( "Alternative closing bracket for brackets highlighting. Set to '\\x0' for disable brackets highlighting." )]
        public char RightBracket3 {
            get;
            set;
        }

        /// <summary>
        /// Comment line prefix.
        /// </summary>
        [DefaultValue( "//" )]
        [Description( "Comment line prefix." )]
        public string CommentPrefix {
            get;
            set;
        }
        /// <summary>
        /// TextChanged event.
        /// It occurs after insert, delete, clear, undo and redo operations.
        /// </summary>
        [Browsable( true )]
        [Description( "It occurs after insert, delete, clear, undo and redo operations." )]
        public new event EventHandler<TextChangedEventArgs> TextChanged;
        /// <summary>
        /// TextChanging event.
        /// It occurs before insert, delete, clear, undo and redo operations.
        /// </summary>
        [Browsable( true )]
        [Description( "It occurs before insert, delete, clear, undo and redo operations." )]
        public event EventHandler<TextChangingEventArgs> TextChanging;
        /// <summary>
        /// SelectionChanged event.
        /// It occurs after changing of selection.
        /// </summary>
        [Browsable( true )]
        [Description( "It occurs after changing of selection." )]
        public event EventHandler SelectionChanged;
        /// <summary>
        /// VisibleRangeChanged event.
        /// It occurs after changing of visible range.
        /// </summary>
        [Browsable( true )]
        [Description( "It occurs after changing of visible range." )]
        public event EventHandler VisibleRangeChanged;
        /// <summary>
        /// TextChangedDelayed event. 
        /// It occurs after insert, delete, clear, undo and redo operations. 
        /// This event occurs with a delay relative to TextChanged, and fires only once.
        /// </summary>
        [Browsable( true )]
        [Description( "It occurs after insert, delete, clear, undo and redo operations. This event occurs with a delay relative to TextChanged, and fires only once." )]
        public event EventHandler<TextChangedEventArgs> TextChangedDelayed;
        /// <summary>
        /// SelectionChangedDelayed event.
        /// It occurs after changing of selection.
        /// This event occurs with a delay relative to SelectionChanged, and fires only once.
        /// </summary>
        [Browsable( true )]
        [Description( "It occurs after changing of selection. This event occurs with a delay relative to SelectionChanged, and fires only once." )]
        public event EventHandler SelectionChangedDelayed;
        /// <summary>
        /// VisibleRangeChangedDelayed event.
        /// It occurs after changing of visible range.
        /// This event occurs with a delay relative to VisibleRangeChanged, and fires only once.
        /// </summary>
        [Browsable( true )]
        [Description( "It occurs after changing of visible range. This event occurs with a delay relative to VisibleRangeChanged, and fires only once." )]
        public event EventHandler VisibleRangeChangedDelayed;
        /// <summary>
        /// It occurs when user click on VisualMarker.
        /// </summary>
        [Browsable( true )]
        [Description( "It occurs when user click on VisualMarker." )]
        public event EventHandler<VisualMarkerEventArgs> VisualMarkerClick;
        /// <summary>
        /// It occurs when visible char is enetering (alphabetic, digit, punctuation, DEL, BACKSPACE)
        /// </summary>
        /// <remarks>Set Handle to True for cancel key</remarks>
        [Browsable( true )]
        [Description( "It occurs when visible char is enetering (alphabetic, digit, punctuation, DEL, BACKSPACE)." )]
        public event KeyPressEventHandler KeyPressing;
        /// <summary>
        /// It occurs when visible char is enetered (alphabetic, digit, punctuation, DEL, BACKSPACE)
        /// </summary>
        [Browsable( true )]
        [Description( "It occurs when visible char is enetered (alphabetic, digit, punctuation, DEL, BACKSPACE)." )]
        public event KeyPressEventHandler KeyPressed;
        /// <summary>
        /// It occurs when calculates AutoIndent for new line
        /// </summary>
        [Browsable( true )]
        [Description( "It occurs when calculates AutoIndent for new line." )]
        public event EventHandler<AutoIndentEventArgs> AutoIndentNeeded;
        /// <summary>
        /// It occurs when line background is painting
        /// </summary>
        [Browsable( true )]
        [Description( "It occurs when line background is painting." )]
        public event EventHandler<PaintLineEventArgs> PaintLine;
        /// <summary>
        /// Occurs when line was inserted/added
        /// </summary>
        [Browsable( true )]
        [Description( "Occurs when line was inserted/added." )]
        public event EventHandler<LineInsertedEventArgs> LineInserted;
        /// <summary>
        /// Occurs when line was removed
        /// </summary>
        [Browsable( true )]
        [Description( "Occurs when line was removed." )]
        public event EventHandler<LineRemovedEventArgs> LineRemoved;
        /// <summary>
        /// Occurs when current highlighted folding area is changed.
        /// Current folding area see in StartFoldingLine and EndFoldingLine.
        /// </summary>
        /// <remarks></remarks>
        [Browsable( true )]
        [Description( "Occurs when current highlighted folding area is changed." )]
        public event EventHandler<EventArgs> FoldingHighlightChanged;
        /// <summary>
        /// Allows text rendering several styles same time.
        /// </summary>
        [Browsable( true )]
        [DefaultValue( false )]
        [Description( "Allows text rendering several styles same time." )]
        public bool AllowSeveralTextStyleDrawing {
            get;
            set;
        }
        /// <summary>
        /// Allows AutoIndent. Inserts spaces before new line.
        /// </summary>
        [DefaultValue( true )]
        [Description( "Allows auto indent. Inserts spaces before line chars." )]
        public bool AutoIndent {
            get;
            set;
        }

        /// <summary>
        /// Minimal delay(ms) for delayed events (except TextChangedDelayed).
        /// </summary>
        [Browsable( true )]
        [DefaultValue( 100 )]
        [Description( "Minimal delay(ms) for delayed events (except TextChangedDelayed)." )]
        public int DelayedEventsInterval {
            get {
                return timer.Interval;
            }
            set {
                timer.Interval = value;
            }
        }

        /// <summary>
        /// Minimal delay(ms) for TextChangedDelayed event.
        /// </summary>
        [Browsable( true )]
        [DefaultValue( 100 )]
        [Description( "Minimal delay(ms) for TextChangedDelayed event." )]
        public int DelayedTextChangedInterval {
            get {
                return timer2.Interval;
            }
            set {
                timer2.Interval = value;
            }
        }

        /// <summary>
        /// Language for highlighting by built-in highlighter.
        /// </summary>
        [Browsable( true )]
        [DefaultValue( typeof( Language ), "Custom" )]
        [Description( "Language for highlighting by built-in highlighter." )]
        public Language Language {
            get {
                return language;
            }
            set {
                language = value;
                Invalidate( );
                /*
                //clear all styles
                ClearStyle(StyleIndex.All);
                //call OnSyntaxHighlight for refresh syntax highlighting
                OnSyntaxHighlight(new TextChangedEventArgs(Range));*/
            }
        }

        /// <summary>
        /// Syntax Highlighter
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public SyntaxHighlighter SyntaxHighlighter {
            get;
            set;
        }

        /// <summary>
        /// XML file with description of syntax highlighting.
        /// This property works only with Language == Language.Custom.
        /// </summary>
        [Browsable( true )]
        [DefaultValue( null )]
        [EditorAttribute( typeof( System.Windows.Forms.Design.FileNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
        [Description( "XML file with description of syntax highlighting. This property works only with Language == Language.Custom." )]
        public string DescriptionFile {
            get {
                return descriptionFile;
            }
            set {
                descriptionFile = value;
                Invalidate( );
            }
        }

        /// <summary>
        /// Position of left highlighted bracket.
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public Range LeftBracketPosition {
            get {
                return leftBracketPosition;
            }
        }
        /// <summary>
        /// Position of right highlighted bracket.
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public Range RightBracketPosition {
            get {
                return rightBracketPosition;
            }
        }
        /// <summary>
        /// Position of left highlighted alternative bracket.
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public Range LeftBracketPosition2 {
            get {
                return leftBracketPosition2;
            }
        }
        /// <summary>
        /// Position of right highlighted alternative bracket.
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public Range RightBracketPosition2 {
            get {
                return rightBracketPosition2;
            }
        }

        /// <summary>
        /// Start line index of current highlighted folding area. Return -1 if start of area is not found.
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public int StartFoldingLine {
            get {
                return startFoldingLine;
            }
        }
        /// <summary>
        /// End line index of current highlighted folding area. Return -1 if end of area is not found.
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public int EndFoldingLine {
            get {
                return endFoldingLine;
            }
        }
        #endregion
        public void ClearLineChanges( ) {
            int c = LinesCount;
            for ( int i = 0 ; i < c ; i++ )
                this[ i ].IsChanged = false;
            Refresh( );
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public FastColoredTextBox( ) {
            try {
                //drawing optimization
                SetStyle( ControlStyles.AllPaintingInWmPaint, true );
                SetStyle( ControlStyles.UserPaint, true );
                SetStyle( ControlStyles.OptimizedDoubleBuffer, true );
                SetStyle( ControlStyles.ResizeRedraw, true );
                //append monospace font
                Font = new Font( "Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point );
                //create one line
                if ( lines.Count == 0 )
                    InsertLine( 0, new Line( GenerateUniqueLineId( ) ) );
                selection = new Range( this ) {
                    Start = new Place( 0, 0 )
                };
                //default settings
                Cursor = Cursors.IBeam;
                BackColor = Color.White;
                LineNumberColor = Color.Teal;
                IndentBackColor = Color.White;
                ServiceLinesColor = Color.Silver;
                FoldingIndicatorColor = Color.Green;
                CurrentLineColor = Color.Transparent;
                ChangedLineColor = Color.Transparent;
                HighlightFoldingIndicator = true;
                ShowLineNumbers = true;
                TabLength = 4;
                FoldedBlockStyle = new FoldedBlockStyle( Brushes.Gray, null, FontStyle.Regular );
                SelectionStyle = new SelectionStyle( new SolidBrush( Color.FromArgb( 50, Color.Blue ) ) );
                BracketsStyle = new MarkerStyle( new SolidBrush( Color.FromArgb( 80, Color.Lime ) ) );
                BracketsStyle2 = new MarkerStyle( new SolidBrush( Color.FromArgb( 60, Color.Red ) ) );
                DelayedEventsInterval = 100;
                DelayedTextChangedInterval = 100;
                AllowSeveralTextStyleDrawing = false;
                LeftBracket = '\x0';
                RightBracket = '\x0';
                LeftBracket2 = '\x0';
                RightBracket2 = '\x0';
                SyntaxHighlighter = new SyntaxHighlighter( );
                language = Language.Custom;
                PreferredLineWidth = 80;
                needRecalc = true;
                lastNavigatedDateTime = DateTime.Now;
                AutoIndent = true;
                CommentPrefix = "//";
                lineNumberStartValue = 1;
                //
                base.AutoScroll = true;
                timer.Tick += new EventHandler( timer_Tick );
                timer2.Tick += new EventHandler( timer2_Tick );
                //
                InitDefaultStyle( );
            } catch ( Exception ex ) {
                Console.WriteLine( ex );
            }
        }


        DateTime lastNavigatedDateTime;

        /// <summary>
        /// Navigates forward (by Line.LastVisit property)
        /// </summary>
        public bool NavigateForward( ) {
            DateTime min = DateTime.Now;
            int iLine = -1;
            for ( int i = 0 ; i < LinesCount ; i++ )
                if ( lines[ i ].LastVisit > lastNavigatedDateTime && lines[ i ].LastVisit < min ) {
                    min = lines[ i ].LastVisit;
                    iLine = i;
                }
            if ( iLine >= 0 ) {
                Navigate( iLine );
                return true;
            } else
                return false;
        }

        /// <summary>
        /// Navigates backward (by Line.LastVisit property)
        /// </summary>
        public bool NavigateBackward( ) {
            DateTime max = new DateTime( );
            int iLine = -1;
            for ( int i = 0 ; i < LinesCount ; i++ )
                if ( lines[ i ].LastVisit < lastNavigatedDateTime && lines[ i ].LastVisit > max ) {
                    max = lines[ i ].LastVisit;
                    iLine = i;
                }
            if ( iLine >= 0 ) {
                Navigate( iLine );
                return true;
            } else
                return false;
        }

        /// <summary>
        /// Navigates to defined line, without Line.LastVisit reseting
        /// </summary>
        public void Navigate( int iLine ) {
            if ( iLine >= LinesCount )
                return;
            lastNavigatedDateTime = lines[ iLine ].LastVisit;
            Selection.Start = new Place( 0, iLine );
            DoSelectionVisible( );
        }

        protected override void OnLoad( EventArgs e ) {
            base.OnLoad( e );
            m_hImc = ImmGetContext( this.Handle );
        }

        void timer2_Tick( object sender, EventArgs e ) {
            timer2.Enabled = false;
            if ( needRiseTextChangedDelayed ) {
                needRiseTextChangedDelayed = false;
                if ( delayedTextChangedRange == null )
                    return;
                delayedTextChangedRange = Range.GetIntersectionWith( delayedTextChangedRange );
                delayedTextChangedRange.Expand( );
                OnTextChangedDelayed( delayedTextChangedRange );
                delayedTextChangedRange = null;
            }
        }

        public void AddVisualMarker( VisualMarker marker ) {
            visibleMarkers.Add( marker );
        }

        void timer_Tick( object sender, EventArgs e ) {
            timer.Enabled = false;
            if ( needRiseSelectionChangedDelayed ) {
                needRiseSelectionChangedDelayed = false;
                OnSelectionChangedDelayed( );
            }
            if ( needRiseVisibleRangeChangedDelayed ) {
                needRiseVisibleRangeChangedDelayed = false;
                OnVisibleRangeChangedDelayed( );
            }
        }

        public virtual void OnTextChangedDelayed( Range changedRange ) {
            if ( TextChangedDelayed != null )
                TextChangedDelayed( this, new TextChangedEventArgs( changedRange ) );
        }

        public virtual void OnSelectionChangedDelayed( ) {
            //highlight brackets
            ClearBracketsPositions( );
            if ( LeftBracket != '\x0' && RightBracket != '\x0' )
                HighlightBrackets( LeftBracket, RightBracket, ref leftBracketPosition, ref rightBracketPosition );
            if ( LeftBracket2 != '\x0' && RightBracket2 != '\x0' )
                HighlightBrackets( LeftBracket2, RightBracket2, ref leftBracketPosition2, ref rightBracketPosition2 );
            if ( LeftBracket3 != '\x0' && RightBracket3 != '\x0' )
                HighlightBrackets( LeftBracket3, RightBracket3, ref leftBracketPosition3, ref rightBracketPosition3 );
            //remember last visit time
            if ( Selection.Start == Selection.End && Selection.Start.iLine < LinesCount ) {
                if ( lastNavigatedDateTime != lines[ Selection.Start.iLine ].LastVisit ) {
                    lines[ Selection.Start.iLine ].LastVisit = DateTime.Now;
                    lastNavigatedDateTime = lines[ Selection.Start.iLine ].LastVisit;
                }
            }

            if ( SelectionChangedDelayed != null )
                SelectionChangedDelayed( this, new EventArgs( ) );
        }

        public virtual void OnVisibleRangeChangedDelayed( ) {
            if ( VisibleRangeChangedDelayed != null )
                VisibleRangeChangedDelayed( this, new EventArgs( ) );
        }

        void ResetTimer( System.Windows.Forms.Timer timer ) {
            timer.Stop( );
            timer.Start( );
        }

        /// <summary>
        /// Adds new style
        /// </summary>
        /// <returns>Layer index of this style</returns>
        public int AddStyle( Style style ) {
            if ( style == null )
                return -1;

            int i = GetStyleIndex( style );
            if ( i >= 0 )
                return i;

            for ( i = Styles.Length - 1 ; i >= 0 ; i-- )
                if ( Styles[ i ] != null )
                    break;

            i++;
            if ( i >= Styles.Length )
                throw new Exception( "Maximum count of Styles is exceeded" );

            Styles[ i ] = style;
            return i;
        }

        public void ClearStyle( ) {
            int c = Styles.Length;
            for ( int i = 0 ; i < c ; i++ )
                Styles.SetValue( null, i );
        }

        /// <summary>
        /// Returns current visible range of text
        /// </summary>
        [Browsable( false )]
        public Range VisibleRange {
            get {
                return GetRange(
                    PointToPlace( new Point( 0, 0 ) ),
                    PointToPlace( new Point( ClientSize.Width, ClientSize.Height ) )
                );
            }
        }

        /// <summary>
        /// Current selection range
        /// </summary>
        [Browsable( false )]
        public Range Selection {
            get {
                return selection;
            }
            set {
                selection.BeginUpdate( );
                selection.Start = value.Start;
                selection.End = value.End;
                selection.EndUpdate( );
                Invalidate( );
            }
        }

        /// <summary>
        /// Background color.
        /// </summary>
        [DefaultValue( typeof( Color ), "White" )]
        [Description( "Background color." )]
        public override Color BackColor {
            get {
                return base.BackColor;
            }
            set {
                base.BackColor = value;
            }
        }

        /// <summary>
        /// WordWrap.
        /// </summary>
        [Browsable( true )]
        [DefaultValue( false )]
        [Description( "WordWrap." )]
        public bool WordWrap {
            get {
                return wordWrap;
            }
            set {
                if ( wordWrap == value )
                    return;
                wordWrap = value;
                RecalcWordWrap( 0, LinesCount - 1 );
                Invalidate( );
            }
        }

        /// <summary>
        /// WordWrap mode.
        /// </summary>
        [Browsable( true )]
        [DefaultValue( typeof( WordWrapMode ), "WordWrapControlWidth" )]
        [Description( "WordWrap mode." )]
        public WordWrapMode WordWrapMode {
            get {
                return wordWrapMode;
            }
            set {
                if ( wordWrapMode == value )
                    return;
                wordWrapMode = value;
                RecalcWordWrap( 0, LinesCount - 1 );
                Invalidate( );
            }
        }


        /// <summary>
        /// Count of lines with wordwrap effect
        /// </summary>
        [Browsable( false )]
        public int WordWrapLinesCount {
            get {
                if ( needRecalc )
                    Recalc( );
                return wordWrapLinesCount;
            }
        }

        /// <summary>
        /// Do not change this property
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public override bool AutoScroll {
            get {
                return base.AutoScroll;
            }
            set {
                ;
            }
        }

        /// <summary>
        /// Count of lines
        /// </summary>
        [Browsable( false )]
        public int LinesCount {
            get {
                return lines.Count;
            }
        }

        /// <summary>
        /// Gets or sets char and styleId for given place
        /// This property does not fire OnTextChanged event
        /// </summary>
        public Char this[ Place place ] {
            get {
                Place p = new Place( place.iChar, place.iLine );
                if ( this[ place.iLine ].Count == place.iChar )
                    p.iChar--;
                if ( p.iChar == -1 )
                    p = new Place( 0, 0 );
                return lines[ p.iLine ][ p.iChar ];
            }
            set {
                lines[ place.iLine ][ place.iChar ] = value;
            }
        }

        /// <summary>
        /// Gets Line
        /// </summary>
        public Line this[ int iLine ] {
            get {
                return lines[ iLine ];
            }
        }

        /// <summary>
        /// Shows find dialog
        /// </summary>
        public void ShowFindDialog( ) {
            if ( findForm == null )
                findForm = new FindForm( this );
            if ( Selection.Start != Selection.End && Selection.Start.iLine == Selection.End.iLine )
                findForm.tbFind.Text = Selection.Text;
            findForm.Show( );
        }

        /// <summary>
        /// Shows replace dialog
        /// </summary>
        public void ShowReplaceDialog( ) {
            if ( ReadOnly )
                return;
            if ( replaceForm == null )
                replaceForm = new ReplaceForm( this );
            if ( Selection.Start != Selection.End && Selection.Start.iLine == Selection.End.iLine )
                replaceForm.tbFind.Text = Selection.Text;
            replaceForm.Show( );
        }

        /// <summary>
        /// Gets length of given line
        /// </summary>
        /// <param name="iLine">Line index</param>
        /// <returns>Length of line</returns>
        public int GetLineLength( int iLine ) {
            if ( iLine < 0 || iLine >= lines.Count )
                throw new ArgumentOutOfRangeException( "Line index out of range" );

            return lines[ iLine ].Count;
        }

        /// <summary>
        /// Get range of line
        /// </summary>
        /// <param name="iLine">Line index</param>
        public Range GetLine( int iLine ) {
            if ( iLine < 0 || iLine >= lines.Count )
                throw new ArgumentOutOfRangeException( "Line index out of range" );

            Range sel = new Range( this );
            sel.Start = new Place( 0, iLine );
            sel.End = new Place( lines[ iLine ].Count, iLine );
            return sel;
        }

        /// <summary>
        /// Copy selected text into Clipboard
        /// </summary>
        public void Copy( ) {
            if ( Selection.End != Selection.Start ) {
                ExportToHTML exp = new ExportToHTML( );
                exp.UseBr = false;
                exp.UseNbsp = false;
                exp.UseStyleTag = true;
                string html = "<pre>" + exp.GetHtml( Selection ) + "</pre>";
                var data = new DataObject( );
                data.SetData( DataFormats.UnicodeText, true, Selection.Text );
                data.SetData( DataFormats.Html, PrepareHtmlForClipboard( html ) );
                Clipboard.SetDataObject( data, true );
            }
        }

        public static MemoryStream PrepareHtmlForClipboard( string html ) {
            Encoding enc = Encoding.UTF8;

            string begin = "Version:0.9\r\nStartHTML:{0:000000}\r\nEndHTML:{1:000000}"
               + "\r\nStartFragment:{2:000000}\r\nEndFragment:{3:000000}\r\n";

            string html_begin = "<html>\r\n<head>\r\n"
               + "<meta http-equiv=\"Content-Type\""
               + " content=\"text/html; charset=" + enc.WebName + "\">\r\n"
               + "<title>HTML clipboard</title>\r\n</head>\r\n<body>\r\n"
               + "<!--StartFragment-->";

            string html_end = "<!--EndFragment-->\r\n</body>\r\n</html>\r\n";

            string begin_sample = String.Format( begin, 0, 0, 0, 0 );

            int count_begin = enc.GetByteCount( begin_sample );
            int count_html_begin = enc.GetByteCount( html_begin );
            int count_html = enc.GetByteCount( html );
            int count_html_end = enc.GetByteCount( html_end );

            string html_total = String.Format(
               begin
               , count_begin
               , count_begin + count_html_begin + count_html + count_html_end
               , count_begin + count_html_begin
               , count_begin + count_html_begin + count_html
               ) + html_begin + html + html_end;

            return new MemoryStream( enc.GetBytes( html_total ) );
        }


        /// <summary>
        /// Cut selected text into Clipboard
        /// </summary>
        public void Cut( ) {
            if ( Selection.End != Selection.Start ) {
                Copy( );
                ClearSelected( );
            }
        }

        /// <summary>
        /// Paste text from clipboard into selection position
        /// </summary>
        public void Paste( ) {
            if ( Clipboard.ContainsText( ) )
                InsertText( Clipboard.GetText( ) );
        }

        /// <summary>
        /// Text of control
        /// </summary>
        [Browsable( true )]
        [Localizable( true )]
        [Editor( "System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof( UITypeEditor ) )]
        [SettingsBindable( true )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Visible )]
        [Description( "Text of control." )]
        public override string Text {
            get {
                Range sel = new Range( this );
                sel.SelectAll( );
                return sel.Text;
            }

            set {
                Selection.BeginUpdate( );
                try {
                    Selection.SelectAll( );
                    InsertText( value );
                } finally {
                    Selection.EndUpdate( );
                }
            }
        }

        /// <summary>
        /// Text lines
        /// </summary>
        [Browsable( false )]
        public IList<string> Lines {
            get {
                string[ ] res = new string[ LinesCount ];

                for ( int i = 0 ; i < LinesCount ; i++ )
                    res[ i ] = this[ i ].Text;
                return Array.AsReadOnly<string>( res );
            }
        }

        [Browsable( false )]
        public new Padding Padding {
            get {
                return new Padding( 0, 0, 0, 0 );
            }
            set {
                ;
            }
        }

        /// <summary>
        /// Gets colored text as HTML
        /// </summary>
        /// <remarks>For more flexibility you can use ExportToHTML class also</remarks>
        [Browsable( false )]
        public string Html {
            get {
                ExportToHTML exporter = new ExportToHTML( );
                exporter.UseNbsp = false;
                exporter.UseStyleTag = false;
                exporter.UseBr = false;
                return "<pre>" + exporter.GetHtml( this ) + "</pre>";
            }
        }

        /// <summary>
        /// Select all chars of text
        /// </summary>
        public void SelectAll( ) {
            Selection.SelectAll( );
        }

        /// <summary>
        /// Text of current selection
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public string SelectedText {
            get {
                return Selection.Text;
            }
            set {
                InsertText( value );
            }
        }

        /// <summary>
        /// Start position of selection
        /// </summary>
        [Browsable( false )]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public int SelectionStart {
            get {
                return Math.Min( PlaceToPosition( Selection.Start ), PlaceToPosition( Selection.End ) );
            }
            set {
                Selection.Start = PositionToPlace( value );
            }
        }

        /// <summary>
        /// Length of selected text
        /// </summary>
        [Browsable( false )]
        [DefaultValue( 0 )]
        public int SelectionLength {
            get {
                return Math.Abs( PlaceToPosition( Selection.Start ) - PlaceToPosition( Selection.End ) );
            }
            set {
                if ( value > 0 )
                    Selection.End = PositionToPlace( SelectionStart + value );
            }
        }

        /// <summary>
        /// Move caret to end of text
        /// </summary>
        public void GoEnd( ) {
            if ( lines.Count > 0 )
                Selection.Start = new Place( lines[ lines.Count - 1 ].Count, lines.Count - 1 );
            else
                Selection.Start = new Place( 0, 0 );
            DoCaretVisible( );
        }

        /// <summary>
        /// Move caret to first position
        /// </summary>
        public void GoHome( ) {
            Selection.Start = new Place( 0, 0 );
            VerticalScroll.Value = 0;
            HorizontalScroll.Value = 0;
        }

        /// <summary>
        /// Clear text, styles, history, caches
        /// </summary>
        public void Clear( ) {
            Selection.BeginUpdate( );
            try {
                Selection.SelectAll( );
                ClearSelected( );
                manager.ClearHistory( );
                Invalidate( );
            } finally {
                Selection.EndUpdate( );
            }
        }

        /// <summary>
        /// Clear buffer of styles
        /// </summary>
        public void ClearStylesBuffer( ) {
            for ( int i = 0 ; i < Styles.Length ; i++ )
                Styles[ i ] = null;
        }

        /// <summary>
        /// Clear style of all text
        /// </summary>
        public void ClearStyle( StyleIndex styleIndex ) {
            foreach ( var line in lines )
                line.ClearStyle( styleIndex );
            Invalidate( );
        }


        /// <summary>
        /// Clears undo and redo stacks
        /// </summary>
        public void ClearUndo( ) {
            manager.ClearHistory( );
        }

        public void InitDefaultStyle( ) {
            DefaultStyle = new TextStyle( null, null, FontStyle.Regular );
        }

        /// <summary>
        /// Insert text into current selection position
        /// </summary>
        /// <param name="text"></param>
        public void InsertText( string text ) {
            if ( text == null )
                return;

            manager.BeginAutoUndoCommands( );
            try {
                if ( Selection.Start != Selection.End )
                    manager.ExecuteCommand( new ClearSelectedCommand( this ) );

                manager.ExecuteCommand( new InsertTextCommand( this, text ) );
                if ( updating <= 0 )
                    DoCaretVisible( );
            } finally {
                manager.EndAutoUndoCommands( );
            }
            //
            Invalidate( );
        }

        /// <summary>
        /// Append string to end of the Text
        /// </summary>
        /// <param name="text"></param>
        public void AppendText( string text ) {
            if ( text == null )
                return;

            var oldStart = Selection.Start;
            var oldEnd = Selection.End;

            Selection.BeginUpdate( );
            manager.BeginAutoUndoCommands( );
            try {
                if ( lines.Count > 0 )
                    Selection.Start = new Place( lines[ lines.Count - 1 ].Count, lines.Count - 1 );
                else
                    Selection.Start = new Place( 0, 0 );

                manager.ExecuteCommand( new InsertTextCommand( this, text ) );
            } finally {
                manager.EndAutoUndoCommands( );
                Selection.Start = oldStart;
                Selection.End = oldEnd;
                Selection.EndUpdate( );
            }
            //
            Invalidate( );
        }

        Font _font;

        /// <summary>
        /// Font
        /// </summary>
        /// <remarks>Use only monospaced font</remarks>
        [DefaultValue( typeof( Font ), "Consolas, 9.75" )]
        public override Font Font {
            get {
                return _font;
            }
            set {
                _font = value;
                //check monospace font
                SizeF sizeM = GetCharSize( _font, 'M' );
                SizeF sizeDot = GetCharSize( _font, '.' );
                if ( sizeM != sizeDot )
                    _font = new Font( "Courier New", _font.SizeInPoints, FontStyle.Regular, GraphicsUnit.Point );
                //clac size
                SizeF size = GetCharSize( _font, 'M' );
                CharHeight = lineInterval + ( int )Math.Round( size.Height * 1f/*0.9*/) - 1/*0*/;
                CharWidth = ( int )Math.Round( size.Width * 1f/*0.85*/) - 1/*0*/;
                //
                Invalidate( );
            }
        }

        /// <summary>
        /// Returns index of the style in Styles
        /// -1 otherwise
        /// </summary>
        /// <param name="style"></param>
        /// <returns>Index of the style in Styles</returns>
        public int GetStyleIndex( Style style ) {
            return Array.IndexOf<Style>( Styles, style );
        }

        /// <summary>
        /// Returns StyleIndex mask of given styles
        /// </summary>
        /// <param name="styles"></param>
        /// <returns>StyleIndex mask of given styles</returns>
        public StyleIndex GetStyleIndexMask( Style[ ] styles ) {
            StyleIndex mask = StyleIndex.None;
            foreach ( Style style in styles ) {
                int i = GetStyleIndex( style );
                if ( i >= 0 )
                    mask |= Range.ToStyleIndex( i );
            }

            return mask;
        }

        public int GetOrSetStyleLayerIndex( Style style ) {
            int i = GetStyleIndex( style );
            if ( i < 0 )
                i = AddStyle( style );
            return i;
        }

        public static SizeF GetCharSize( Font font, char c ) {
            Size sz2 = TextRenderer.MeasureText( "<" + c.ToString( ) + ">", font );
            Size sz3 = TextRenderer.MeasureText( "<>", font );

            return new SizeF( sz2.Width - sz3.Width + 1, sz2.Height );
        }

        IntPtr m_hImc;

        const int WM_IME_SETCONTEXT = 0x0281;
        const int WM_HSCROLL = 0x114;
        const int WM_VSCROLL = 0x115;
        const int SB_ENDSCROLL = 0x8;

        [DllImport( "Imm32.dll" )]
        public static extern IntPtr ImmGetContext( IntPtr hWnd );
        [DllImport( "Imm32.dll" )]
        public static extern IntPtr ImmAssociateContext( IntPtr hWnd, IntPtr hIMC );

        protected override void WndProc( ref Message m ) {
            if ( m.Msg == WM_HSCROLL || m.Msg == WM_VSCROLL )
                if ( m.WParam.ToInt32( ) != SB_ENDSCROLL )
                    Invalidate( );

            base.WndProc( ref m );

            if ( ImeAllowed )
                if ( m.Msg == WM_IME_SETCONTEXT && m.WParam.ToInt32( ) == 1 ) {
                    ImmAssociateContext( this.Handle, m_hImc );
                }
        }

        protected override void OnScroll( ScrollEventArgs se ) {
            base.OnScroll( se );
            UpdateOutsideControlLocation( );
            OnVisibleRangeChanged( );
            Invalidate( );
        }

        public void UpdateOutsideControlLocation( ) {
            /*
            if (tabs != null)
            {
                tabs.Location = new Point(0, 0);
                tabs.Width = ClientRectangle.Width;
            }*/
        }

        void InsertChar( char c ) {
            manager.BeginAutoUndoCommands( );
            try {
                if ( Selection.Start != Selection.End )
                    manager.ExecuteCommand( new ClearSelectedCommand( this ) );

                manager.ExecuteCommand( new InsertCharCommand( this, c ) );
            } finally {
                manager.EndAutoUndoCommands( );
            }

            Invalidate( );
        }

        /// <summary>
        /// Deletes selected chars
        /// </summary>
        public void ClearSelected( ) {
            if ( Selection.Start != Selection.End ) {
                manager.ExecuteCommand( new ClearSelectedCommand( this ) );
                Invalidate( );
            }
        }

        /// <summary>
        /// Deletes current line(s)
        /// </summary>
        public void ClearCurrentLine( ) {
            Selection.Expand( );
            manager.ExecuteCommand( new ClearSelectedCommand( this ) );
            if ( Selection.Start.iLine == 0 )
                if ( !Selection.GoRightThroughFolded( ) )
                    return;
            if ( Selection.Start.iLine > 0 )
                manager.ExecuteCommand( new InsertCharCommand( this, '\b' ) );//backspace
            Invalidate( );
        }

        public void Recalc( ) {
            if ( !needRecalc )
                return;

            needRecalc = false;
            //calc min left indent
            LeftIndent = CharWidth * 2;
            var maxLineNumber = this.LinesCount + lineNumberStartValue - 1;
            int charsForLineNumber = 2 + ( maxLineNumber > 0 ? ( int )Math.Log10( maxLineNumber ) : 0 );
            if ( this.Created ) {
                if ( ShowLineNumbers )
                    LeftIndent += charsForLineNumber * CharWidth + minLeftIndent + 1;
            } else
                needRecalc = true;
            //calc max line length and count of wordWrapLines
            int maxLineLength = 0;
            wordWrapLinesCount = 0;
            foreach ( var line in lines ) {
                if ( line.Count > maxLineLength && line.VisibleState == VisibleState.Visible )
                    maxLineLength = line.Count;
                line.startY = wordWrapLinesCount * CharHeight + TopIndent;
                wordWrapLinesCount += line.WordWrapStringsCount;
            }

            //adjust AutoScrollMinSize
            int minWidth = LeftIndent + ( maxLineLength ) * CharWidth + 2;
            if ( wordWrap )
                switch ( WordWrapMode ) {
                    case WordWrapMode.WordWrapControlWidth:
                    case WordWrapMode.CharWrapControlWidth:
                        minWidth = 0;
                        break;
                    case WordWrapMode.WordWrapPreferredWidth:
                    case WordWrapMode.CharWrapPreferredWidth:
                        minWidth = LeftIndent + PreferredLineWidth * CharWidth + 2;
                        break;
                }
            AutoScrollMinSize = new Size( minWidth, wordWrapLinesCount * CharHeight + TopIndent );
        }

        public void RecalcWordWrap( int fromLine, int toLine ) {
            int maxCharsPerLine = 0;
            bool charWrap = false;

            switch ( WordWrapMode ) {
                case WordWrapMode.WordWrapControlWidth:
                    maxCharsPerLine = ( ClientSize.Width - LeftIndent ) / CharWidth;
                    break;
                case WordWrapMode.CharWrapControlWidth:
                    maxCharsPerLine = ( ClientSize.Width - LeftIndent ) / CharWidth;
                    charWrap = true;
                    break;
                case WordWrapMode.WordWrapPreferredWidth:
                    maxCharsPerLine = PreferredLineWidth;
                    break;
                case WordWrapMode.CharWrapPreferredWidth:
                    maxCharsPerLine = PreferredLineWidth;
                    charWrap = true;
                    break;
            }

            for ( int iLine = fromLine ; iLine <= toLine ; iLine++ )
                if ( !wordWrap )
                    lines[ iLine ].CutOffPositions.Clear( );
                else
                    lines[ iLine ].CalcCutOffs( maxCharsPerLine, ImeAllowed, charWrap );
            needRecalc = true;
        }

        protected override void OnClientSizeChanged( EventArgs e ) {
            base.OnClientSizeChanged( e );
            if ( WordWrap ) {
                RecalcWordWrap( 0, lines.Count - 1 );
                Invalidate( );
            }
            UpdateOutsideControlLocation( );
            OnVisibleRangeChanged( );
        }

        /// <summary>
        /// Scroll control for display defined rectangle
        /// </summary>
        /// <param name="rect"></param>
        public void DoVisibleRectangle( Rectangle rect ) {
            int oldV = VerticalScroll.Value;
            int v = VerticalScroll.Value;
            int h = HorizontalScroll.Value;

            if ( rect.Bottom > ClientRectangle.Height )
                v += rect.Bottom - ClientRectangle.Height;
            else
                if ( rect.Top < 0 )
                    v += rect.Top;

            if ( rect.Right > ClientRectangle.Width )
                h += rect.Right - ClientRectangle.Width;
            else
                if ( rect.Left < LeftIndent )
                    h += rect.Left - LeftIndent;
            //
            try {
                VerticalScroll.Value = Math.Max( 0, v );
                HorizontalScroll.Value = Math.Max( 0, h );
            } catch ( ArgumentOutOfRangeException ) {
                ;
            }

            //some magic for update scrolls
            AutoScrollMinSize -= new Size( 1, 0 );
            AutoScrollMinSize += new Size( 1, 0 );
            //
            if ( oldV != VerticalScroll.Value )
                OnVisibleRangeChanged( );
        }

        /// <summary>
        /// Scroll control for display caret
        /// </summary>
        public void DoCaretVisible( ) {
            Invalidate( );
            Recalc( );
            Point car = PlaceToPoint( Selection.Start );
            car.Offset( -CharWidth, 0 );
            DoVisibleRectangle( new Rectangle( car, new Size( 2 * CharWidth, 2 * CharHeight ) ) );
        }

        /// <summary>
        /// Scroll control left
        /// </summary>
        public void ScrollLeft( ) {
            Invalidate( );
            HorizontalScroll.Value = 0;
            AutoScrollMinSize -= new Size( 1, 0 );
            AutoScrollMinSize += new Size( 1, 0 );
        }

        /// <summary>
        /// Scroll control for display selection area
        /// </summary>
        public void DoSelectionVisible( ) {
            if ( lines[ Selection.End.iLine ].VisibleState != VisibleState.Visible )
                ExpandBlock( Selection.End.iLine );

            if ( lines[ Selection.Start.iLine ].VisibleState != VisibleState.Visible )
                ExpandBlock( Selection.Start.iLine );

            Recalc( );
            DoVisibleRectangle( new Rectangle( PlaceToPoint( new Place( 0, Selection.End.iLine ) ), new Size( 2 * CharWidth, 2 * CharHeight ) ) );
            Point car = PlaceToPoint( Selection.Start );
            Point car2 = PlaceToPoint( Selection.End );
            car.Offset( -CharWidth, -ClientSize.Height / 2 );
            DoVisibleRectangle( new Rectangle( car, new Size( Math.Abs( car2.X - car.X ), /*Math.Abs(car2.Y-car.Y) + 2 * CharHeight*/ClientSize.Height ) ) );
        }


        protected override void OnKeyUp( KeyEventArgs e ) {
            base.OnKeyUp( e );

            if ( e.KeyCode == Keys.ShiftKey )
                lastModifiers &= ~Keys.Shift;
            if ( e.KeyCode == Keys.Alt )
                lastModifiers &= ~Keys.Alt;
            if ( e.KeyCode == Keys.ControlKey )
                lastModifiers &= ~Keys.Control;
        }

        bool handledChar = false;

        protected override void OnKeyDown( KeyEventArgs e ) {
            base.OnKeyDown( e );

            lastModifiers = e.Modifiers;

            handledChar = false;

            if ( e.Handled ) {
                handledChar = true;
                return;
            }

            switch ( e.KeyCode ) {
                case Keys.F:
                    if ( e.Modifiers == Keys.Control )
                        ShowFindDialog( );
                    break;
                case Keys.H:
                    if ( e.Modifiers == Keys.Control )
                        ShowReplaceDialog( );
                    break;
                case Keys.C:
                    if ( e.Modifiers == Keys.Control )
                        Copy( );
                    if ( e.Modifiers == ( Keys.Control | Keys.Shift ) )
                        CommentSelected( );
                    break;
                case Keys.X:
                    if ( e.Modifiers == Keys.Control && !ReadOnly )
                        Cut( );
                    break;
                case Keys.V:
                    if ( e.Modifiers == Keys.Control && !ReadOnly )
                        Paste( );
                    break;
                case Keys.A:
                    if ( e.Modifiers == Keys.Control )
                        Selection.SelectAll( );
                    break;
                case Keys.Z:
                    if ( e.Modifiers == Keys.Control && !ReadOnly )
                        Undo( );
                    break;
                case Keys.R:
                    if ( e.Modifiers == Keys.Control && !ReadOnly )
                        Redo( );
                    break;
                case Keys.U:
                    if ( e.Modifiers == Keys.Control )
                        UpperLowerCase( );
                    break;
                case Keys.Tab:
                    if ( e.Modifiers == Keys.Shift && !ReadOnly )
                        DecreaseIndent( );
                    break;
                case Keys.OemMinus:
                    if ( e.Modifiers == Keys.Control )
                        NavigateBackward( );
                    if ( e.Modifiers == ( Keys.Control | Keys.Shift ) )
                        NavigateForward( );
                    break;

                case Keys.Back:
                    if ( ReadOnly )
                        break;
                    if ( e.Modifiers == Keys.Alt )
                        Undo( );
                    else {
                        if ( e.Control && Selection.End == Selection.Start && Selection.Start.iChar != 0 ) {
                            int fromX = Selection.Start.iChar - 1;
                            int toX = Selection.Start.iChar;
                            for ( int i = Selection.Start.iChar ; i < lines[ Selection.Start.iLine ].Count ; i++ ) {
                                char c = lines[ Selection.Start.iLine ][ i ].c;
                                if ( char.IsLetterOrDigit( c ) || c == '_' )
                                    toX = i + 1;
                                else
                                    break;
                            }

                            for ( int i = Selection.Start.iChar - 1 ; i >= 0 ; i-- ) {
                                char c = lines[ Selection.Start.iLine ][ i ].c;
                                if ( char.IsLetterOrDigit( c ) || c == '_' )
                                    fromX = i;
                                else
                                    break;
                            }
                            Selection.Start = new Place( fromX, Selection.Start.iLine );
                            Selection.End = new Place( toX, Selection.End.iLine );
                            ClearSelected( );
                        } else if ( e.Modifiers == Keys.None ) {
                            if ( OnKeyPressing( '\b' ) )//KeyPress event processed key
                                break;
                            if ( Selection.End != Selection.Start )
                                ClearSelected( );
                            else
                                InsertChar( '\b' );
                            OnKeyPressed( '\b' );
                        }
                    }

                    break;
                case Keys.Delete:
                    if ( ReadOnly )
                        break;
                    if ( e.Modifiers == Keys.None ) {
                        if ( OnKeyPressing( ( char )0xff ) )//KeyPress event processed key
                            break;
                        if ( Selection.End != Selection.Start )
                            ClearSelected( );
                        else {
                            if ( Selection.GoRightThroughFolded( ) ) {
                                int iLine = Selection.Start.iLine;
                                InsertChar( '\b' );
                                //if removed \n then trim spaces
                                if ( iLine != Selection.Start.iLine && AutoIndent )
                                    RemoveSpacesAfterCaret( );
                            }
                        }
                        OnKeyPressed( ( char )0xff );
                    }
                    break;
                case Keys.Space:
                    if ( ReadOnly )
                        break;
                    if ( e.Modifiers == Keys.None || e.Modifiers == Keys.Shift ) {
                        if ( OnKeyPressing( ' ' ) )//KeyPress event processed key
                            break;
                        if ( Selection.End != Selection.Start )
                            ClearSelected( );
                        else
                            InsertChar( ' ' );
                        OnKeyPressed( ' ' );
                    }
                    break;

                case Keys.Left:
                    if ( e.Modifiers == Keys.Control || e.Modifiers == ( Keys.Control | Keys.Shift ) )
                        Selection.GoWordLeft( e.Shift );
                    if ( e.Modifiers == Keys.None || e.Modifiers == Keys.Shift )
                        Selection.GoLeft( e.Shift );
                    break;
                case Keys.Right:
                    if ( e.Modifiers == Keys.Control || e.Modifiers == ( Keys.Control | Keys.Shift ) )
                        Selection.GoWordRight( e.Shift );
                    if ( e.Modifiers == Keys.None || e.Modifiers == Keys.Shift )
                        Selection.GoRight( e.Shift );
                    break;
                case Keys.Up:
                    if ( e.Modifiers == Keys.None || e.Modifiers == Keys.Shift ) {
                        Selection.GoUp( e.Shift );
                        ScrollLeft( );
                    }
                    break;
                case Keys.Down:
                    if ( e.Modifiers == Keys.None || e.Modifiers == Keys.Shift ) {
                        Selection.GoDown( e.Shift );
                        ScrollLeft( );
                    }
                    break;
                case Keys.PageUp:
                    if ( e.Modifiers == Keys.None || e.Modifiers == Keys.Shift ) {
                        Selection.GoPageUp( e.Shift );
                        ScrollLeft( );
                    }
                    break;
                case Keys.PageDown:
                    if ( e.Modifiers == Keys.None || e.Modifiers == Keys.Shift ) {
                        Selection.GoPageDown( e.Shift );
                        ScrollLeft( );
                    }
                    break;
                case Keys.Home:
                    if ( e.Modifiers == Keys.Control || e.Modifiers == ( Keys.Control | Keys.Shift ) )
                        Selection.GoFirst( e.Shift );
                    if ( e.Modifiers == Keys.None || e.Modifiers == Keys.Shift ) {
                        GoHome( e.Shift );
                        ScrollLeft( );
                    }
                    break;
                case Keys.End:
                    if ( e.Modifiers == Keys.Control || e.Modifiers == ( Keys.Control | Keys.Shift ) )
                        Selection.GoLast( e.Shift );
                    if ( e.Modifiers == Keys.None || e.Modifiers == Keys.Shift )
                        Selection.GoEnd( e.Shift );
                    break;
                default:
                    if ( ( e.Modifiers & Keys.Control ) != 0 )
                        return;
                    if ( ( e.Modifiers & Keys.Alt ) != 0 )
                        return;
                    break;
            }

            e.Handled = true;

            DoCaretVisible( );
            Invalidate( );
        }
        public void PublicKeyDown( KeyEventArgs e ) {
            OnKeyDown( e );
        }
        public void GoHome( bool shift ) {
            Selection.BeginUpdate( );
            try {
                int iLine = Selection.Start.iLine;
                int spaces = this[ iLine ].StartSpacesCount;
                if ( Selection.Start.iChar <= spaces )
                    Selection.GoHome( shift );
                else {
                    Selection.GoHome( shift );
                    for ( int i = 0 ; i < spaces ; i++ )
                        Selection.GoRight( shift );
                }
            } finally {
                Selection.EndUpdate( );
            }
        }

        /// <summary>
        /// Convert selected text to upper/lower case
        /// </summary>
        public void UpperLowerCase( ) {
            var old = Selection.Clone( );
            string text = Selection.Text;
            string trimmedText = text.TrimStart( );
            if ( trimmedText.Length > 0 && char.IsUpper( trimmedText[ 0 ] ) )
                SelectedText = SelectedText.ToLower( );
            else
                SelectedText = SelectedText.ToUpper( );
            Selection.Start = old.Start;
            Selection.End = old.End;
        }

        /// <summary>
        /// Insert/remove comment prefix into selected lines
        /// </summary>
        public void CommentSelected( ) {
            CommentSelected( CommentPrefix );
        }

        /// <summary>
        /// Insert/remove comment prefix into selected lines
        /// </summary>
        public void CommentSelected( string commentPrefix ) {
            if ( string.IsNullOrEmpty( commentPrefix ) )
                return;
            Selection.Normalize( );
            bool isCommented = lines[ Selection.Start.iLine ].Text.TrimStart( ).StartsWith( commentPrefix );
            if ( isCommented )
                RemoveLinePrefix( commentPrefix );
            else
                InsertLinePrefix( commentPrefix );
        }

        /*
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                bool proc = ProcessKeyPress('\r');
                if (proc)
                {
                    base.OnKeyDown(new KeyEventArgs(Keys.Enter));
                    return true;
                }
            }
            
            return base.ProcessCmdKey(ref msg, keyData);
        }*/

        public void OnKeyPressing( KeyPressEventArgs args ) {
            if ( KeyPressing != null )
                KeyPressing( this, args );
        }

        bool OnKeyPressing( char c ) {
            KeyPressEventArgs args = new KeyPressEventArgs( c );
            OnKeyPressing( args );
            return args.Handled;
        }

        public void OnKeyPressed( char c ) {
            KeyPressEventArgs args = new KeyPressEventArgs( c );
            if ( KeyPressed != null )
                KeyPressed( this, args );
        }

        protected override bool ProcessMnemonic( char charCode ) {
            return ProcessKeyPress( charCode ) || base.ProcessMnemonic( charCode );
        }

        bool ProcessKeyPress( char c ) {
            if ( handledChar )
                return true;

            if ( c == ' ' )
                return true;

            if ( c == '\b' && ( lastModifiers & Keys.Alt ) != 0 )
                return true;

            if ( char.IsControl( c ) && c != '\r' && c != '\t' )
                return false;

            if ( ReadOnly || !Enabled )
                return false;

            if ( lastModifiers != Keys.None &&
                lastModifiers != Keys.Shift &&
                lastModifiers != ( Keys.Control | Keys.Alt ) &&//ALT+CTRL is special chars (AltGr)
                lastModifiers != ( Keys.Shift | Keys.Control | Keys.Alt ) &&//SHIFT + ALT + CTRL is special chars (AltGr)
                ( lastModifiers != ( Keys.Alt ) || char.IsLetterOrDigit( c ) )//may be ALT+LetterOrDigit is mnemonic code
                )
                return false;//do not process Ctrl+? and Alt+? keys

            char sourceC = c;
            if ( OnKeyPressing( sourceC ) )//KeyPress event processed key
                return true;

            //tab?
            if ( c == '\t' ) {
                if ( Selection.Start == Selection.End ) {
                    //insert tab as spaces
                    int spaces = TabLength - ( Selection.Start.iChar % TabLength );
                    InsertText( new String( ' ', spaces ) );
                } else
                    if ( ( lastModifiers & Keys.Shift ) == 0 )
                        IncreaseIndent( );
            } else {
                //replace \r on \n
                if ( c == '\r' )
                    c = '\n';
                //insert char
                InsertChar( c );
                //do autoindent
                if ( AutoIndent ) {
                    DoCaretVisible( );
                    int needSpaces = CalcAutoIndent( Selection.Start.iLine );
                    if ( this[ Selection.Start.iLine ].AutoIndentSpacesNeededCount != needSpaces ) {
                        DoAutoIndent( Selection.Start.iLine );
                        this[ Selection.Start.iLine ].AutoIndentSpacesNeededCount = needSpaces;
                    }
                }
            }

            DoCaretVisible( );
            Invalidate( );

            OnKeyPressed( sourceC );

            return true;
        }

        public void RemoveSpacesAfterCaret( ) {
            if ( Selection.Start != Selection.End )
                return;
            var end = Selection.Start;
            while ( Selection.CharAfterStart == ' ' )
                Selection.GoRight( true );
            ClearSelected( );
        }

        /// <summary>
        /// Inserts autoindent's spaces in the line
        /// </summary>
        public virtual void DoAutoIndent( int iLine ) {
            Place oldStart = Selection.Start;
            //
            int needSpaces = CalcAutoIndent( iLine );
            //
            int spaces = lines[ iLine ].StartSpacesCount;
            int needToInsert = needSpaces - spaces;
            if ( needToInsert < 0 )
                needToInsert = -Math.Min( -needToInsert, spaces );
            //insert start spaces
            if ( needToInsert == 0 )
                return;
            Selection.Start = new Place( 0, iLine );
            if ( needToInsert > 0 )
                InsertText( new String( ' ', needToInsert ) );
            else {
                Selection.Start = new Place( 0, iLine );
                Selection.End = new Place( -needToInsert, iLine );
                ClearSelected( );
            }

            Selection.Start = new Place( Math.Min( lines[ iLine ].Count, Math.Max( 0, oldStart.iChar + needToInsert ) ), iLine );
        }

        /// <summary>
        /// Returns needed start space count for the line
        /// </summary>
        public virtual int CalcAutoIndent( int iLine ) {
            if ( iLine < 0 || iLine >= LinesCount )
                return 0;


            EventHandler<AutoIndentEventArgs> calculator = AutoIndentNeeded;
            if ( calculator == null )
                if ( Language != Language.Custom && SyntaxHighlighter != null )
                    calculator = SyntaxHighlighter.AutoIndentNeeded;
                else
                    calculator = CalcAutoIndentShiftByCodeFolding;

            int needSpaces = 0;

            Stack<AutoIndentEventArgs> stack = new Stack<AutoIndentEventArgs>( );
            //calc indent for previous lines, find stable line
            int i;
            for ( i = iLine - 1 ; i >= 0 ; i-- ) {
                AutoIndentEventArgs args = new AutoIndentEventArgs( i, lines[ i ].Text, i > 0 ? lines[ i - 1 ].Text : "", TabLength );
                calculator( this, args );
                stack.Push( args );
                if ( args.Shift == 0 && args.LineText.Trim( ) != "" )
                    break;
            }
            int indent = lines[ i >= 0 ? i : 0 ].StartSpacesCount;
            while ( stack.Count != 0 )
                indent += stack.Pop( ).ShiftNextLines;
            //clalc shift for current line
            AutoIndentEventArgs a = new AutoIndentEventArgs( iLine, lines[ iLine ].Text, iLine > 0 ? lines[ iLine - 1 ].Text : "", TabLength );
            calculator( this, a );
            needSpaces = indent + a.Shift;

            return needSpaces;
        }

        public virtual void CalcAutoIndentShiftByCodeFolding( object sender, AutoIndentEventArgs args ) {
            //inset TAB after start folding marker
            if ( string.IsNullOrEmpty( lines[ args.iLine ].FoldingEndMarker ) &&
                !string.IsNullOrEmpty( lines[ args.iLine ].FoldingStartMarker ) ) {
                args.ShiftNextLines = TabLength;
                return;
            }
            //remove TAB before end folding marker
            if ( !string.IsNullOrEmpty( lines[ args.iLine ].FoldingEndMarker ) &&
                string.IsNullOrEmpty( lines[ args.iLine ].FoldingStartMarker ) ) {
                args.Shift = -TabLength;
                args.ShiftNextLines = -TabLength;
                return;
            }
        }


        int GetMinStartSpacesCount( int fromLine, int toLine ) {
            if ( fromLine > toLine )
                return 0;

            int result = int.MaxValue;
            for ( int i = fromLine ; i <= toLine ; i++ ) {
                int count = lines[ i ].StartSpacesCount;
                if ( count < result )
                    result = count;
            }

            return result;
        }

        /// <summary>
        /// Indicates that IME is allowed (for CJK language entering)
        /// </summary>
        [Browsable( false )]
        public bool ImeAllowed {
            get {
                return ImeMode != System.Windows.Forms.ImeMode.Disable &&
                        ImeMode != System.Windows.Forms.ImeMode.Off &&
                        ImeMode != System.Windows.Forms.ImeMode.NoControl;
            }
        }

        /// <summary>
        /// Undo last operation
        /// </summary>
        public void Undo( ) {
            manager.Undo( );
            Invalidate( );
        }

        /// <summary>
        /// Is undo enabled?
        /// </summary>
        [Browsable( false )]
        public bool UndoEnabled {
            get {
                return manager.UndoEnabled;
            }
        }

        /// <summary>
        /// Redo
        /// </summary>
        public void Redo( ) {
            manager.Redo( );
            Invalidate( );
        }

        /// <summary>
        /// Is redo enabled?
        /// </summary>
        [Browsable( false )]
        public bool RedoEnabled {
            get {
                return manager.RedoEnabled;
            }
        }

        protected override bool IsInputChar( char charCode ) {
            return base.IsInputChar( charCode );
        }

        protected override bool IsInputKey( Keys keyData ) {
            if ( ( keyData & Keys.Alt ) == Keys.None ) {
                Keys keys = keyData & Keys.KeyCode;
                if ( keys == Keys.Return )
                    return true;
            }

            if ( ( keyData & Keys.Alt ) != Keys.Alt ) {
                switch ( ( keyData & Keys.KeyCode ) ) {
                    case Keys.Prior:
                    case Keys.Next:
                    case Keys.End:
                    case Keys.Home:
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Up:
                    case Keys.Down:
                        return true;

                    case Keys.Escape:
                        return false;

                    case Keys.Tab:
                        return ( keyData & Keys.Control ) == Keys.None;
                }
            }

            return base.IsInputKey( keyData );
        }

        [DllImport( "User32.dll" )]
        static extern bool CreateCaret( IntPtr hWnd, int hBitmap, int nWidth, int nHeight );
        [DllImport( "User32.dll" )]
        static extern bool SetCaretPos( int x, int y );
        [DllImport( "User32.dll" )]
        static extern bool DestroyCaret( );
        [DllImport( "User32.dll" )]
        static extern bool ShowCaret( IntPtr hWnd );
        [DllImport( "User32.dll" )]
        static extern bool HideCaret( IntPtr hWnd );

        /// <summary>
        /// Draw control
        /// </summary>
        protected override void OnPaint( PaintEventArgs e ) {
            if ( needRecalc )
                Recalc( );
#if debug
            var sw = Stopwatch.StartNew();
#endif
            visibleMarkers.Clear( );
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            //
            Brush lineNumberBrush = new SolidBrush( LineNumberColor );
            Pen servicePen = new Pen( ServiceLinesColor );
            Brush changedLineBrush = new SolidBrush( ChangedLineColor );
            Brush indentBrush = new SolidBrush( IndentBackColor );
            Pen currentLinePen = new Pen( CurrentLineColor );
            Brush currentLineBrush = new SolidBrush( Color.FromArgb( 50, CurrentLineColor ) );
            //draw indent area
            e.Graphics.FillRectangle( indentBrush, 0, 0, LeftIndent - minLeftIndent / 2 - 1, ClientSize.Height );
            if ( LeftIndent > minLeftIndent )
                e.Graphics.DrawLine( servicePen, LeftIndent - CharWidth, 0, LeftIndent - CharWidth, ClientSize.Height );
            //draw preffered line width
            if ( PreferredLineWidth > 0 )
                e.Graphics.DrawLine( servicePen, new Point( LeftIndent + PreferredLineWidth * CharWidth - HorizontalScroll.Value, 0 ), new Point( LeftIndent + PreferredLineWidth * CharWidth - HorizontalScroll.Value, Height ) );
            //
            int firstChar = HorizontalScroll.Value / CharWidth;
            int lastChar = ( HorizontalScroll.Value + ClientSize.Width ) / CharWidth;
            //draw chars
            //draw brackets highlighting
            if ( BracketsStyle != null && leftBracketPosition != null && rightBracketPosition != null ) {
                BracketsStyle.Draw( e.Graphics, PlaceToPoint( leftBracketPosition.Start ), leftBracketPosition );
                BracketsStyle.Draw( e.Graphics, PlaceToPoint( rightBracketPosition.Start ), rightBracketPosition );
            }
            if ( BracketsStyle2 != null && leftBracketPosition2 != null && rightBracketPosition2 != null ) {
                BracketsStyle2.Draw( e.Graphics, PlaceToPoint( leftBracketPosition2.Start ), leftBracketPosition2 );
                BracketsStyle2.Draw( e.Graphics, PlaceToPoint( rightBracketPosition2.Start ), rightBracketPosition2 );
            }
            if ( BracketsStyle3 != null && leftBracketPosition3 != null && rightBracketPosition3 != null ) {
                BracketsStyle3.Draw( e.Graphics, PlaceToPoint( leftBracketPosition3.Start ), leftBracketPosition3 );
                BracketsStyle3.Draw( e.Graphics, PlaceToPoint( rightBracketPosition3.Start ), rightBracketPosition3 );
            }
            for ( int iLine = YtoLineIndex( VerticalScroll.Value ) ; iLine < lines.Count ; iLine++ ) {
                Line line = lines[ iLine ];
                //
                if ( line.startY > VerticalScroll.Value + ClientSize.Height )
                    break;
                if ( line.startY + line.WordWrapStringsCount * CharHeight < VerticalScroll.Value )
                    continue;
                if ( line.VisibleState == VisibleState.Hidden )
                    continue;

                int y = line.startY - VerticalScroll.Value;
                //
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                //draw line background
                if ( line.VisibleState == VisibleState.Visible )
                    OnPaintLine( new PaintLineEventArgs( iLine, new Rectangle( LeftIndent, y, Width, CharHeight * line.WordWrapStringsCount ), e.Graphics, e.ClipRectangle ) );
                //draw current line background
                if ( CurrentLineColor != Color.Transparent && iLine == Selection.Start.iLine )
                    e.Graphics.FillRectangle( currentLineBrush, new Rectangle( LeftIndent - CharWidth / 2, y, Width + CharWidth / 2, CharHeight - 1 ) );
                //draw changed line marker
                if ( ChangedLineColor != Color.Transparent && line.IsChanged )
                    if ( ChangedLineWidth == -1 )
                        e.Graphics.FillRectangle( changedLineBrush, new RectangleF( -10, y, LeftIndent - minLeftIndent - 2, CharHeight + 1 ) );
                    else
                        e.Graphics.FillRectangle( changedLineBrush, new RectangleF( LeftIndent - minLeftIndent - CharWidth * 2, y, ChangedLineWidth, CharHeight + 1 ) );
                //draw line number

                if ( ShowLineNumbers )
                    e.Graphics.DrawString( ( iLine + lineNumberStartValue ).ToString( ), new Font( Font.FontFamily, Font.Size, FontStyle.Regular ),
                        lineNumberBrush, new RectangleF( 0, y, LeftIndent - minLeftIndent - CharWidth * 2 + CharWidth / 3, CharHeight ), new StringFormat( StringFormatFlags.DirectionRightToLeft ) );

                //create markers
                if ( line.VisibleState == VisibleState.StartOfHiddenBlock )
                    visibleMarkers.Add( new ExpandFoldingMarker( iLine, new Rectangle( LeftIndent - CharWidth * 3 / 2, y + ( CharWidth + CharHeight ) / 4, CharWidth, CharWidth ) ) );
                if ( !string.IsNullOrEmpty( line.FoldingStartMarker ) && line.VisibleState == VisibleState.Visible && string.IsNullOrEmpty( line.FoldingEndMarker ) )
                    visibleMarkers.Add( new CollapseFoldingMarker( iLine, new Rectangle( LeftIndent - CharWidth * 3 / 2, y + ( CharWidth + CharHeight ) / 4, CharWidth, CharWidth ) ) );
                if ( line.VisibleState == VisibleState.Visible && !string.IsNullOrEmpty( line.FoldingEndMarker ) && string.IsNullOrEmpty( line.FoldingStartMarker ) )
                    e.Graphics.DrawLine( servicePen, LeftIndent - minLeftIndent / 2 - CharWidth / 2, y + CharHeight * line.WordWrapStringsCount - 1, LeftIndent - minLeftIndent / 2 + CharWidth / 4, y + CharHeight * line.WordWrapStringsCount - 1 );
                //draw wordwrap strings of line
                for ( int iWordWrapLine = 0 ; iWordWrapLine < line.WordWrapStringsCount ; iWordWrapLine++ ) {
                    y = line.startY + iWordWrapLine * CharHeight - VerticalScroll.Value;
                    //draw chars
                    DrawLineChars( e, firstChar, lastChar, iLine, iWordWrapLine, y );
                }
            }
            //
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            //draw folding indicator
            if ( ( startFoldingLine >= 0 || endFoldingLine >= 0 ) && Selection.Start == Selection.End ) {
                //folding indicator
                int startFoldingY = ( startFoldingLine >= 0 ? lines[ startFoldingLine ].startY : 0 ) - VerticalScroll.Value + CharHeight / 2;
                int endFoldingY = ( endFoldingLine >= 0 ? lines[ endFoldingLine ].startY + ( lines[ endFoldingLine ].WordWrapStringsCount - 1 ) * CharHeight : ( WordWrapLinesCount + 1 ) * CharHeight ) - VerticalScroll.Value + CharHeight;
            }
            //draw markers
            foreach ( var m in visibleMarkers )
                m.Draw( e.Graphics, servicePen );
            //draw caret
            Point car = PlaceToPoint( Selection.Start );
            if ( Focused && car.X >= LeftIndent ) {
                CreateCaret( this.Handle, 0, 0, CharHeight );
                SetCaretPos( car.X, car.Y );
                ShowCaret( this.Handle );
            } else {
                HideCaret( this.Handle );
            }

            e.Graphics.FillRectangle( new SolidBrush( Color.FromArgb( 170, 170, 170 ) ), new Rectangle( 0, 1, CharWidth, Height ) );

            //dispose resources
            lineNumberBrush.Dispose( );
            servicePen.Dispose( );
            changedLineBrush.Dispose( );
            indentBrush.Dispose( );
            currentLinePen.Dispose( );
            currentLineBrush.Dispose( );
            //
#if debug
            Console.WriteLine("OnPaint: "+ sw.ElapsedMilliseconds);
#endif
            //
            base.OnPaint( e );

        }

        public void DrawLineChars( PaintEventArgs e, int firstChar, int lastChar, int iLine, int iWordWrapLine, int y ) {

            Line line = lines[ iLine ];
            int from = line.GetWordWrapStringStartPosition( iWordWrapLine );
            int to = line.GetWordWrapStringFinishPosition( iWordWrapLine );

            int startX = LeftIndent - HorizontalScroll.Value;
            if ( startX < LeftIndent )
                firstChar++;

            lastChar = Math.Min( to - from, lastChar );

            if ( Selection.End != Selection.Start && lastChar >= firstChar ) {
                e.Graphics.SmoothingMode = SmoothingMode.None;
                Range textRange = new Range( this, from + firstChar, iLine, from + lastChar + 1, iLine );
                textRange = Selection.GetIntersectionWith( textRange );
                if ( textRange != null && SelectionStyle != null )
                    SelectionStyle.Draw( e.Graphics, new Point( startX + ( textRange.Start.iChar - from ) * CharWidth, y ), textRange );
            }
            //folded block ?
            if ( line.VisibleState == VisibleState.StartOfHiddenBlock ) {
                //rendering by FoldedBlockStyle
                FoldedBlockStyle.Draw( e.Graphics, new Point( startX + firstChar * CharWidth, y ), new Range( this, from + firstChar, iLine, from + lastChar + 1, iLine ) );
            } else {
                //render by custom styles
                StyleIndex currentStyleIndex = StyleIndex.None;
                int iLastFlushedChar = firstChar - 1;

                for ( int iChar = firstChar ; iChar <= lastChar ; iChar++ ) {
                    StyleIndex style = line[ from + iChar ].style;
                    if ( currentStyleIndex != style ) {
                        FlushRendering( e.Graphics, currentStyleIndex, new Point( startX + ( iLastFlushedChar + 1 ) * CharWidth, y ), new Range( this, from + iLastFlushedChar + 1, iLine, from + iChar, iLine ) );
                        iLastFlushedChar = iChar - 1;
                        currentStyleIndex = style;
                    }
                }
                FlushRendering( e.Graphics, currentStyleIndex, new Point( startX + ( iLastFlushedChar + 1 ) * CharWidth, y ), new Range( this, from + iLastFlushedChar + 1, iLine, from + lastChar + 1, iLine ) );
            }
        }

        public void FlushRendering( Graphics gr, StyleIndex styleIndex, Point pos, Range range ) {
            if ( range.End > range.Start ) {
                int mask = 1;
                bool hasTextStyle = false;
                for ( int i = 0 ; i < Styles.Length ; i++ ) {
                    if ( Styles[ i ] != null && ( ( int )styleIndex & mask ) != 0 ) {
                        Style style = Styles[ i ];
                        bool isTextStyle = style is TextStyle;
                        if ( !hasTextStyle || !isTextStyle || AllowSeveralTextStyleDrawing )//cancelling secondary rendering by TextStyle
                            style.Draw( gr, pos, range );//rendering
                        hasTextStyle |= isTextStyle;
                    }
                    mask = mask << 1;
                }
                //draw by default renderer
                if ( !hasTextStyle )
                    DefaultStyle.Draw( gr, pos, range );
            }
        }

        protected override void OnEnter( EventArgs e ) {
            base.OnEnter( e );
            mouseIsDrag = false;
        }

        protected override void OnMouseDown( MouseEventArgs e ) {
            base.OnMouseDown( e );

            if ( e.Button == System.Windows.Forms.MouseButtons.Left ) {
                Place p = PointToPlace( e.Location );
                if ( e.Clicks == 1 ) {
                    mouseIsDrag = true;
                    var marker = FindVisualMarkerForPoint( e.Location );
                    //click on marker
                    if ( marker != null ) {
                        OnMarkerClick( e, marker );
                        return;
                    }
                    //click on text
                    var oldEnd = Selection.End;
                    Selection.BeginUpdate( );
                    Selection.Start = PointToPlace( e.Location );
                    if ( ( lastModifiers & Keys.Shift ) != 0 )
                        Selection.End = oldEnd;
                    Selection.EndUpdate( );
                }
                if ( e.Clicks == 2 ) {

                    int fromX = p.iChar;
                    int toX = p.iChar;
                    for ( int i = p.iChar ; i < lines[ p.iLine ].Count ; i++ ) {
                        char c = lines[ p.iLine ][ i ].c;
                        if ( char.IsLetterOrDigit( c ) || c == '_' )
                            toX = i + 1;
                        else
                            break;
                    }

                    for ( int i = p.iChar - 1 ; i >= 0 ; i-- ) {
                        char c = lines[ p.iLine ][ i ].c;
                        if ( char.IsLetterOrDigit( c ) || c == '_' )
                            fromX = i;
                        else
                            break;
                    }
                    Selection.Start = new Place( toX, p.iLine );
                    Selection.End = new Place( fromX, p.iLine );
                }
                Invalidate( );
                return;
            }
        }

        protected override void OnMouseWheel( MouseEventArgs e ) {
            Invalidate( );
            base.OnMouseWheel( e );
            UpdateOutsideControlLocation( );
            OnVisibleRangeChanged( );
        }

        bool mouseIsDrag = false;

        protected override void OnMouseMove( MouseEventArgs e ) {
            base.OnMouseMove( e );

            if ( e.Button == System.Windows.Forms.MouseButtons.Left && mouseIsDrag ) {
                var oldEnd = Selection.End;
                Selection.BeginUpdate( );
                Selection.Start = PointToPlace( e.Location );
                Selection.End = oldEnd;
                Selection.EndUpdate( );
                DoCaretVisible( );
                Invalidate( );
                return;
            }

            var marker = FindVisualMarkerForPoint( e.Location );
            if ( marker != null )
                Cursor = marker.Cursor;
            else
                Cursor = Cursors.IBeam;

        }

        int YtoLineIndex( int y ) {
            int i = lines.BinarySearch( null, new LineYComparer( y ) );
            i = i < 0 ? -i - 2 : i;
            if ( i < 0 )
                return 0;
            if ( i > lines.Count - 1 )
                return lines.Count - 1;
            return i;
        }

        class LineYComparer : IComparer<Line> {
            int Y;
            public LineYComparer( int Y ) {
                this.Y = Y;
            }


            public int Compare( Line x, Line y ) {
                if ( x == null )
                    return -y.startY.CompareTo( Y );
                else
                    return x.startY.CompareTo( Y );
            }
        }

        /// <summary>
        /// Gets nearest line and char position from coordinates
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Line and char position</returns>
        public Place PointToPlace( Point point ) {
#if debug
            var sw = Stopwatch.StartNew();
#endif
            point.Offset( HorizontalScroll.Value, VerticalScroll.Value );
            point.Offset( -LeftIndent, 0 );
            int iLine = YtoLineIndex( point.Y );
            int y = 0;

            for ( ; iLine < lines.Count ; iLine++ ) {
                y = lines[ iLine ].startY + lines[ iLine ].WordWrapStringsCount * CharHeight;
                if ( y > point.Y && lines[ iLine ].VisibleState == VisibleState.Visible )
                    break;
            }
            if ( iLine >= lines.Count )
                iLine = lines.Count - 1;
            if ( lines[ iLine ].VisibleState != VisibleState.Visible )
                iLine = FindPrevVisibleLine( iLine );
            //
            int iWordWrapLine = lines[ iLine ].WordWrapStringsCount;
            do {
                iWordWrapLine--;
                y -= CharHeight;
            } while ( y > point.Y );
            if ( iWordWrapLine < 0 )
                iWordWrapLine = 0;
            //
            int start = lines[ iLine ].GetWordWrapStringStartPosition( iWordWrapLine );
            int finish = lines[ iLine ].GetWordWrapStringFinishPosition( iWordWrapLine );
            int x = ( int )Math.Round( ( float )point.X / CharWidth );
            x = x < 0 ? start : start + x;
            if ( x > finish )
                x = finish + 1;
            if ( x > lines[ iLine ].Count )
                x = lines[ iLine ].Count;

#if debug
            Console.WriteLine("PointToPlace: " + sw.ElapsedMilliseconds);
#endif

            return new Place( x, iLine );
        }

        /// <summary>
        /// Gets nearest absolute text position for given point
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Position</returns>
        public int PointToPosition( Point point ) {
            return PlaceToPosition( PointToPlace( point ) );
        }

        /// <summary>
        /// Fires TextChanging event
        /// </summary>
        public virtual void OnTextChanging( ref string text ) {
            ClearBracketsPositions( );

            if ( TextChanging != null ) {
                var args = new TextChangingEventArgs( ) {
                    InsertingText = text
                };
                TextChanging( this, args );
                text = args.InsertingText;
            };
        }

        public virtual void OnTextChanging( ) {
            string temp = null;
            OnTextChanging( ref temp );
        }

        /// <summary>
        /// Fires TextChanged event
        /// </summary>
        public virtual void OnTextChanged( ) {
            Range r = new Range( this );
            r.SelectAll( );
            OnTextChanged( new TextChangedEventArgs( r ) );
        }

        /// <summary>
        /// Fires TextChanged event
        /// </summary>
        public virtual void OnTextChanged( int fromLine, int toLine ) {
            Range r = new Range( this );
            r.Start = new Place( 0, Math.Min( fromLine, toLine ) );
            r.End = new Place( lines[ Math.Max( fromLine, toLine ) ].Count, Math.Max( fromLine, toLine ) );
            OnTextChanged( new TextChangedEventArgs( r ) );
        }

        /// <summary>
        /// Fires TextChanged event
        /// </summary>
        public virtual void OnTextChanged( Range r ) {
            OnTextChanged( new TextChangedEventArgs( r ) );
        }

        int updating;
        Range updatingRange = null;

        public void BeginUpdate( ) {
            if ( updating == 0 )
                updatingRange = null;
            updating++;
        }

        public void EndUpdate( ) {
            updating--;

            if ( updating == 0 && updatingRange != null ) {
                updatingRange.Expand( );
                OnTextChanged( updatingRange );
            }
        }


        /// <summary>
        /// Fires TextChanged event
        /// </summary>
        public virtual void OnTextChanged( TextChangedEventArgs args ) {
            //
            args.ChangedRange.Normalize( );
            //
            if ( updating > 0 ) {
                if ( updatingRange == null )
                    updatingRange = args.ChangedRange.Clone( );
                else {
                    if ( updatingRange.Start.iLine > args.ChangedRange.Start.iLine )
                        updatingRange.Start = new Place( 0, args.ChangedRange.Start.iLine );
                    if ( updatingRange.End.iLine < args.ChangedRange.End.iLine )
                        updatingRange.End = new Place( lines[ args.ChangedRange.End.iLine ].Count, args.ChangedRange.End.iLine );
                }
                return;
            }
            //
#if debug
            var sw = Stopwatch.StartNew();
#endif
            IsChanged = true;
            TextVersion++;
            MarkLinesAsChanged( args.ChangedRange );
            //
            if ( wordWrap )
                RecalcWordWrap( args.ChangedRange.Start.iLine, args.ChangedRange.End.iLine );
            //
            base.OnTextChanged( args );

            //dalayed event stuffs
            if ( delayedTextChangedRange == null )
                delayedTextChangedRange = args.ChangedRange.Clone( );
            else {
                if ( delayedTextChangedRange.Start.iLine > args.ChangedRange.Start.iLine )
                    delayedTextChangedRange.Start = new Place( 0, args.ChangedRange.Start.iLine );
                if ( delayedTextChangedRange.End.iLine < args.ChangedRange.End.iLine )
                    delayedTextChangedRange.End = new Place( lines[ args.ChangedRange.End.iLine ].Count, args.ChangedRange.End.iLine );
            }
            needRiseTextChangedDelayed = true;
            ResetTimer( timer2 );
            //
            OnSyntaxHighlight( args );
            //
            if ( TextChanged != null )
                TextChanged( this, args );
            //
#if debug
            Console.WriteLine("OnTextChanged: " + sw.ElapsedMilliseconds);
#endif

            OnVisibleRangeChanged( );
        }

        public void MarkLinesAsChanged( Range range ) {
            for ( int iLine = range.Start.iLine ; iLine <= range.End.iLine ; iLine++ )
                if ( iLine >= 0 && iLine < lines.Count )
                    lines[ iLine ].IsChanged = true;
        }

        /// <summary>
        /// Fires SelectionCnaged event
        /// </summary>
        public virtual void OnSelectionChanged( ) {
            OnSelectionChangedDelayed( );
#if debug
            var sw = Stopwatch.StartNew();
#endif
            //find folding markers for highlighting
            if ( HighlightFoldingIndicator )
                HighlightFoldings( );
            //
            needRiseSelectionChangedDelayed = true;
            ResetTimer( timer );

            if ( SelectionChanged != null )
                SelectionChanged( this, new EventArgs( ) );

#if debug
            Console.WriteLine("OnSelectionChanged: "+ sw.ElapsedMilliseconds);
#endif
        }

        //find folding markers for highlighting
        public void HighlightFoldings( ) {
            int prevStartFoldingLine = startFoldingLine;
            int prevEndFoldingLine = endFoldingLine;
            //
            startFoldingLine = -1;
            endFoldingLine = -1;
            const int maxLines = 2000;
            //
            string marker = null;
            int counter = 0;
            for ( int i = Selection.Start.iLine ; i >= Math.Max( Selection.Start.iLine - maxLines, 0 ) ; i-- ) {
                if ( !string.IsNullOrEmpty( lines[ i ].FoldingStartMarker ) &&
                    !string.IsNullOrEmpty( lines[ i ].FoldingEndMarker ) )
                    continue;

                if ( !string.IsNullOrEmpty( lines[ i ].FoldingStartMarker ) ) {
                    counter--;
                    if ( counter == -1 )//found start folding
					{
                        startFoldingLine = i;
                        marker = lines[ i ].FoldingStartMarker;
                        break;
                    }
                }
                if ( !string.IsNullOrEmpty( lines[ i ].FoldingEndMarker ) && i != Selection.Start.iLine )
                    counter++;
            }
            if ( startFoldingLine >= 0 ) {
                //find end of block
                endFoldingLine = FindEndOfFoldingBlock( startFoldingLine );
                if ( endFoldingLine == startFoldingLine )
                    endFoldingLine = -1;
            }

            if ( startFoldingLine != prevStartFoldingLine || endFoldingLine != prevEndFoldingLine )
                OnFoldingHighlightChanged( );
        }

        public virtual void OnFoldingHighlightChanged( ) {
            if ( FoldingHighlightChanged != null )
                FoldingHighlightChanged( this, EventArgs.Empty );
        }

        protected override void OnGotFocus( EventArgs e ) {
            base.OnGotFocus( e );
            //Invalidate(new Rectangle(PlaceToPoint(Selection.Start), new Size(2, CharHeight+1)));
            Invalidate( );
        }

        protected override void OnLostFocus( EventArgs e ) {
            base.OnLostFocus( e );
            //Invalidate(new Rectangle(PlaceToPoint(Selection.Start), new Size(2, CharHeight+1)));
            Invalidate( );
        }

        /// <summary>
        /// Gets absolute text position from line and char position
        /// </summary>
        /// <param name="point">Line and char position</param>
        /// <returns>Index of text char</returns>
        public int PlaceToPosition( Place point ) {
            if ( point.iLine < 0 || point.iLine >= lines.Count || point.iChar >= lines[ point.iLine ].Count + Environment.NewLine.Length )
                return -1;

            int result = 0;
            for ( int i = 0 ; i < point.iLine ; i++ )
                result += lines[ i ].Count + Environment.NewLine.Length;
            result += point.iChar;

            return result;
        }

        /// <summary>
        /// Gets line and char position from absolute text position
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Place PositionToPlace( int pos ) {
            if ( pos < 0 )
                return new Place( 0, 0 );

            for ( int i = 0 ; i < lines.Count ; i++ ) {
                int lineLength = lines[ i ].Count + Environment.NewLine.Length;
                if ( pos < lines[ i ].Count )
                    return new Place( pos, i );
                if ( pos < lineLength )
                    return new Place( lines[ i ].Count, i );

                pos -= lineLength;
            }

            if ( lines.Count > 0 )
                return new Place( lines[ lines.Count - 1 ].Count, lines.Count - 1 );
            else
                return new Place( 0, 0 );
            //throw new ArgumentOutOfRangeException("Position out of range");
        }

        /// <summary>
        /// Gets point for given line and char position
        /// </summary>
        /// <param name="palce">Line and char position</param>
        /// <returns>Coordiantes</returns>
        public Point PlaceToPoint( Place place ) {
            int y = lines[ place.iLine ].startY;
            //
            int iWordWrapIndex = lines[ place.iLine ].GetWordWrapStringIndex( place.iChar );
            y += iWordWrapIndex * CharHeight;
            int x = ( place.iChar - lines[ place.iLine ].GetWordWrapStringStartPosition( iWordWrapIndex ) ) * CharWidth;
            //
            y = y - VerticalScroll.Value;
            x = LeftIndent + x - HorizontalScroll.Value;

            return new Point( x, y );
        }

        /// <summary>
        /// Range of all text
        /// </summary>
        [Browsable( false )]
        public Range Range {
            get {
                return new Range( this, new Place( 0, 0 ), new Place( lines[ lines.Count - 1 ].Count, lines.Count - 1 ) );
            }
        }

        /// <summary>
        /// Get range of text
        /// </summary>
        /// <param name="fromPos">Absolute start position</param>
        /// <param name="toPos">Absolute finish position</param>
        /// <returns>Range</returns>
        public Range GetRange( int fromPos, int toPos ) {
            var sel = new Range( this );
            sel.Start = PositionToPlace( fromPos );
            sel.End = PositionToPlace( toPos );
            return sel;
        }

        /// <summary>
        /// Get range of text
        /// </summary>
        /// <param name="fromPlace">Line and char position</param>
        /// <param name="toPlace">Line and char position</param>
        /// <returns>Range</returns>
        public Range GetRange( Place fromPlace, Place toPlace ) {
            return new Range( this, fromPlace, toPlace );
        }

        /// <summary>
        /// Finds ranges for given regex pattern
        /// </summary>
        /// <param name="regexPattern">Regex pattern</param>
        /// <returns>Enumeration of ranges</returns>
        public IEnumerable<Range> GetRanges( string regexPattern ) {
            Range range = new Range( this );
            range.SelectAll( );
            //
            foreach ( var r in range.GetRanges( regexPattern, RegexOptions.None ) )
                yield return r;
        }

        /// <summary>
        /// Finds ranges for given regex pattern
        /// </summary>
        /// <param name="regexPattern">Regex pattern</param>
        /// <returns>Enumeration of ranges</returns>
        public IEnumerable<Range> GetRanges( string regexPattern, RegexOptions options ) {
            Range range = new Range( this );
            range.SelectAll( );
            //
            foreach ( var r in range.GetRanges( regexPattern, options ) )
                yield return r;
        }

        /// <summary>
        /// Get text of given line
        /// </summary>
        /// <param name="iLine">Line index</param>
        /// <returns>Text</returns>
        public string GetLineText( int iLine ) {
            if ( iLine < 0 || iLine >= lines.Count )
                throw new ArgumentOutOfRangeException( "Line index out of range" );
            StringBuilder sb = new StringBuilder( lines[ iLine ].Count );
            foreach ( Char c in lines[ iLine ] )
                sb.Append( c.c );
            return sb.ToString( );
        }


        public void ExpandFoldedBlock( int iLine ) {
            if ( iLine < 0 || iLine >= lines.Count )
                throw new ArgumentOutOfRangeException( "Line index out of range" );
            //find all hidden lines afetr iLine
            int end = iLine;
            for ( ; end < LinesCount - 1 ; end++ ) {
                if ( lines[ end + 1 ].VisibleState != VisibleState.Hidden )
                    break;
            };

            ExpandBlock( iLine, end );
        }

        /// <summary>
        /// Expand collapsed block
        /// </summary>
        public void ExpandBlock( int fromLine, int toLine ) {
            int from = Math.Min( fromLine, toLine );
            int to = Math.Max( fromLine, toLine );
            for ( int i = from ; i <= to ; i++ )
                lines[ i ].VisibleState = VisibleState.Visible;
            needRecalc = true;
            Invalidate( );
        }

        /// <summary>
        /// Expand collapsed block
        /// </summary>
        /// <param name="iLine">Any line inside collapsed block</param>
        public void ExpandBlock( int iLine ) {
            if ( lines[ iLine ].VisibleState == VisibleState.Visible )
                return;

            for ( int i = iLine ; i < LinesCount ; i++ )
                if ( lines[ i ].VisibleState == VisibleState.Visible )
                    break;
                else {
                    lines[ i ].VisibleState = VisibleState.Visible;
                    needRecalc = true;
                }

            for ( int i = iLine - 1 ; i >= 0 ; i-- )
                if ( lines[ i ].VisibleState == VisibleState.Visible )
                    break;
                else {
                    lines[ i ].VisibleState = VisibleState.Visible;
                    needRecalc = true;
                }

            Invalidate( );
        }

        /// <summary>
        /// Collapses folding block
        /// </summary>
        /// <param name="iLine">Start folding line</param>
        public void CollapseFoldingBlock( int iLine ) {
            if ( iLine < 0 || iLine >= lines.Count )
                throw new ArgumentOutOfRangeException( "Line index out of range" );
            if ( string.IsNullOrEmpty( lines[ iLine ].FoldingStartMarker ) )
                throw new ArgumentOutOfRangeException( "This line is not folding start line" );
            //find end of block
            int i = FindEndOfFoldingBlock( iLine );
            //collapse
            if ( i >= 0 )
                CollapseBlock( iLine, i );
        }

        int FindEndOfFoldingBlock( int iStartLine ) {
            //find end of block
            int counter = 0;
            int i;
            for ( i = iStartLine/*+1*/; i < LinesCount ; i++ ) {
                if ( lines[ i ].FoldingStartMarker == lines[ iStartLine ].FoldingStartMarker )
                    counter++;
                if ( lines[ i ].FoldingEndMarker == lines[ iStartLine ].FoldingStartMarker ) {
                    counter--;
                    if ( counter <= 0 )
                        return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Collapse text block
        /// </summary>
        public void CollapseBlock( int fromLine, int toLine ) {
            int from = Math.Min( fromLine, toLine );
            int to = Math.Max( fromLine, toLine );
            if ( from == to )
                return;

            //find first non empty line
            for ( ; from <= to ; from++ ) {
                if ( GetLineText( from ).Trim( ).Length > 0 ) {
                    //hide lines
                    for ( int i = from + 1 ; i <= to ; i++ )
                        lines[ i ].VisibleState = VisibleState.Hidden;
                    lines[ from ].VisibleState = VisibleState.StartOfHiddenBlock;
                    Invalidate( );
                    break;
                }
            }
            //Move caret outside
            from = Math.Min( fromLine, toLine );
            to = Math.Max( fromLine, toLine );
            int newLine = FindNextVisibleLine( to );
            if ( newLine == to )
                newLine = FindPrevVisibleLine( from );
            Selection.Start = new Place( 0, newLine );
            //
            needRecalc = true;
            Invalidate( );
        }


        public int FindNextVisibleLine( int iLine ) {
            if ( iLine >= lines.Count - 1 )
                return iLine;
            int old = iLine;
            do
                iLine++;
            while ( iLine < lines.Count - 1 && lines[ iLine ].VisibleState != VisibleState.Visible );

            if ( lines[ iLine ].VisibleState != VisibleState.Visible )
                return old;
            else
                return iLine;
        }


        public int FindPrevVisibleLine( int iLine ) {
            if ( iLine <= 0 )
                return iLine;
            int old = iLine;
            do
                iLine--;
            while ( iLine > 0 && lines[ iLine ].VisibleState != VisibleState.Visible );

            if ( lines[ iLine ].VisibleState != VisibleState.Visible )
                return old;
            else
                return iLine;
        }

        VisualMarker FindVisualMarkerForPoint( Point p ) {
            foreach ( var m in visibleMarkers )
                if ( m.rectangle.Contains( p ) )
                    return m;
            return null;
        }

        /// <summary>
        /// Insert TAB into front of seletcted lines
        /// </summary>
        public void IncreaseIndent( ) {
            if ( Selection.Start == Selection.End )
                return;
            var old = Selection.Clone( );
            int from = Math.Min( Selection.Start.iLine, Selection.End.iLine );
            int to = Math.Max( Selection.Start.iLine, Selection.End.iLine );
            BeginUpdate( );
            Selection.BeginUpdate( );
            manager.BeginAutoUndoCommands( );
            for ( int i = from ; i <= to ; i++ ) {
                if ( lines[ i ].Count == 0 )
                    continue;
                Selection.Start = new Place( 0, i );
                manager.ExecuteCommand( new InsertTextCommand( this, new String( ' ', TabLength ) ) );
            }
            manager.EndAutoUndoCommands( );
            Selection.Start = new Place( 0, from );
            Selection.End = new Place( lines[ to ].Count, to );
            needRecalc = true;
            Selection.EndUpdate( );
            EndUpdate( );
            Invalidate( );
        }

        /// <summary>
        /// Remove TAB from front of seletcted lines
        /// </summary>
        public void DecreaseIndent( ) {
            if ( Selection.Start == Selection.End )
                return;
            var old = Selection.Clone( );
            int from = Math.Min( Selection.Start.iLine, Selection.End.iLine );
            int to = Math.Max( Selection.Start.iLine, Selection.End.iLine );
            BeginUpdate( );
            Selection.BeginUpdate( );
            manager.BeginAutoUndoCommands( );
            for ( int i = from ; i <= to ; i++ ) {
                Selection.Start = new Place( 0, i );
                Selection.End = new Place( Math.Min( lines[ i ].Count, TabLength ), i );
                if ( Selection.Text.Trim( ) == "" )
                    ClearSelected( );
            }
            manager.EndAutoUndoCommands( );
            Selection.Start = new Place( 0, from );
            Selection.End = new Place( lines[ to ].Count, to );
            needRecalc = true;
            EndUpdate( );
            Selection.EndUpdate( );
        }

        /// <summary>
        /// Insert autoindents into selected lines
        /// </summary>
        public void DoAutoIndent( ) {
            var r = Selection.Clone( );
            r.Normalize( );
            //
            BeginUpdate( );
            Selection.BeginUpdate( );
            manager.BeginAutoUndoCommands( );
            //
            for ( int i = r.Start.iLine ; i <= r.End.iLine ; i++ )
                DoAutoIndent( i );
            //
            manager.EndAutoUndoCommands( );
            Selection.Start = r.Start;
            Selection.End = r.End;
            Selection.Expand( );
            //
            Selection.EndUpdate( );
            EndUpdate( );
        }

        /// <summary>
        /// Insert prefix into front of seletcted lines
        /// </summary>
        public void InsertLinePrefix( string prefix ) {
            var old = Selection.Clone( );
            int from = Math.Min( Selection.Start.iLine, Selection.End.iLine );
            int to = Math.Max( Selection.Start.iLine, Selection.End.iLine );
            BeginUpdate( );
            Selection.BeginUpdate( );
            manager.BeginAutoUndoCommands( );
            int spaces = GetMinStartSpacesCount( from, to );
            for ( int i = from ; i <= to ; i++ ) {
                Selection.Start = new Place( spaces, i );
                manager.ExecuteCommand( new InsertTextCommand( this, prefix ) );
            }
            Selection.Start = new Place( 0, from );
            Selection.End = new Place( lines[ to ].Count, to );
            needRecalc = true;
            manager.EndAutoUndoCommands( );
            Selection.EndUpdate( );
            EndUpdate( );
            Invalidate( );
        }

        /// <summary>
        /// Remove prefix from front of seletcted lines
        /// </summary>
        public void RemoveLinePrefix( string prefix ) {
            var old = Selection.Clone( );
            int from = Math.Min( Selection.Start.iLine, Selection.End.iLine );
            int to = Math.Max( Selection.Start.iLine, Selection.End.iLine );
            BeginUpdate( );
            Selection.BeginUpdate( );
            manager.BeginAutoUndoCommands( );
            for ( int i = from ; i <= to ; i++ ) {
                string text = lines[ i ].Text;
                string trimmedText = text.TrimStart( );
                if ( trimmedText.StartsWith( prefix ) ) {
                    int spaces = text.Length - trimmedText.Length;
                    Selection.Start = new Place( spaces, i );
                    Selection.End = new Place( spaces + prefix.Length, i );
                    ClearSelected( );
                }
            }
            Selection.Start = new Place( 0, from );
            Selection.End = new Place( lines[ to ].Count, to );
            needRecalc = true;
            manager.EndAutoUndoCommands( );
            Selection.EndUpdate( );
            EndUpdate( );
        }

        /// <summary>
        /// Begins AutoUndo block.
        /// All changes of text between BeginAutoUndo() and EndAutoUndo() will be canceled in one operation Undo.
        /// </summary>
        public void BeginAutoUndo( ) {
            manager.BeginAutoUndoCommands( );
        }

        /// <summary>
        /// Ends AutoUndo block.
        /// All changes of text between BeginAutoUndo() and EndAutoUndo() will be canceled in one operation Undo.
        /// </summary>
        public void EndAutoUndo( ) {
            manager.EndAutoUndoCommands( );
        }

        public virtual void OnVisualMarkerClick( MouseEventArgs args, StyleVisualMarker marker ) {
            if ( VisualMarkerClick != null )
                VisualMarkerClick( this, new VisualMarkerEventArgs( marker.Style, marker, args ) );
        }

        public virtual void OnMarkerClick( MouseEventArgs args, VisualMarker marker ) {
            if ( marker is StyleVisualMarker ) {
                OnVisualMarkerClick( args, marker as StyleVisualMarker );
                return;
            }
            if ( marker is CollapseFoldingMarker ) {
                CollapseFoldingBlock( ( marker as CollapseFoldingMarker ).iLine );
                Invalidate( );
                return;
            }

            if ( marker is ExpandFoldingMarker ) {
                ExpandFoldedBlock( ( marker as ExpandFoldingMarker ).iLine );
                Invalidate( );
                return;
            }

            if ( marker is FoldedAreaMarker ) {
                //select folded block
                int iStart = ( marker as FoldedAreaMarker ).iLine;
                int iEnd = FindEndOfFoldingBlock( iStart );
                Selection.BeginUpdate( );
                Selection.Start = new Place( 0, iStart );
                Selection.End = new Place( lines[ iEnd ].Count, iEnd );
                Selection.EndUpdate( );
                Invalidate( );
                return;
            }
        }

        public virtual void OnMarkerDoubleClick( VisualMarker marker ) {
            if ( marker is FoldedAreaMarker ) {
                ExpandFoldedBlock( ( marker as FoldedAreaMarker ).iLine );
                Invalidate( );
                return;
            }
        }

        void ClearBracketsPositions( ) {
            leftBracketPosition = null;
            rightBracketPosition = null;
            leftBracketPosition2 = null;
            rightBracketPosition2 = null;
            leftBracketPosition3 = null;
            rightBracketPosition3 = null;
        }

        public void HighlightBrackets( char LeftBracket, char RightBracket, ref Range leftBracketPosition, ref Range rightBracketPosition ) {
            if ( Selection.Start != Selection.End )
                return;
            //
            var oldLeftBracketPosition = leftBracketPosition;
            var oldRightBracketPosition = rightBracketPosition;
            Range range = Selection.Clone( );//need clone because we will move caret
            int counter = 0;
            int maxIterations = maxBracketSearchIterations;
            while ( range.GoLeftThroughFolded( ) )//move caret left
			{
                if ( range.CharAfterStart == LeftBracket )
                    counter++;
                if ( range.CharAfterStart == RightBracket )
                    counter--;
                if ( counter == 1 ) {
                    //highlighting
                    range.End = new Place( range.Start.iChar + 1, range.Start.iLine );
                    leftBracketPosition = range;
                    break;
                }
                //
                maxIterations--;
                if ( maxIterations <= 0 )
                    break;
            }
            //
            range = Selection.Clone( );//need clone because we will move caret
            counter = 0;
            maxIterations = maxBracketSearchIterations;
            do {
                if ( range.CharAfterStart == LeftBracket )
                    counter++;
                if ( range.CharAfterStart == RightBracket )
                    counter--;
                if ( counter == -1 ) {
                    //highlighting
                    range.End = new Place( range.Start.iChar + 1, range.Start.iLine );
                    rightBracketPosition = range;
                    break;
                }
                //
                maxIterations--;
                if ( maxIterations <= 0 )
                    break;
            } while ( range.GoRightThroughFolded( ) );//move caret right
            if ( leftBracketPosition != null && rightBracketPosition != null )
                if ( leftBracketPosition.FromX != Selection.Start.iChar - 1 && rightBracketPosition.FromX != Selection.Start.iChar ) {
                    leftBracketPosition = oldLeftBracketPosition;
                    rightBracketPosition = oldRightBracketPosition;
                }
            if ( oldLeftBracketPosition != leftBracketPosition ||
                oldRightBracketPosition != rightBracketPosition )
                Invalidate( );
        }

        public virtual void OnSyntaxHighlight( TextChangedEventArgs args ) {
#if debug
            Stopwatch sw = Stopwatch.StartNew();
#endif

            if ( SyntaxHighlighter != null ) {
                if ( Language == Language.Custom && !string.IsNullOrEmpty( DescriptionFile ) )
                    SyntaxHighlighter.HighlightSyntax( DescriptionFile, args.ChangedRange );
                else
                    SyntaxHighlighter.HighlightSyntax( Language, args.ChangedRange );
            }

#if debug
            Console.WriteLine("OnSyntaxHighlight: "+ sw.ElapsedMilliseconds);
#endif
        }

        public void InitializeComponent( ) {
            this.SuspendLayout( );
            // 
            // FastColoredTextBox
            // 
            this.Name = "FastColoredTextBox";
            this.ResumeLayout( false );
        }

        /// <summary>
        /// Prints range of text
        /// </summary>
        public void Print( Range range, PrintDialogSettings settings ) {
            WebBrowser wb = new WebBrowser( );
            settings.printRange = range;
            wb.Tag = settings;
            wb.Visible = false;
            wb.Location = new Point( -1000, -1000 );
            wb.Parent = this;
            wb.Navigate( "about:blank" );
            wb.Navigated += new WebBrowserNavigatedEventHandler( ShowPrintDialog );
        }

        /// <summary>
        /// Prints all text
        /// </summary>
        public void Print( PrintDialogSettings settings ) {
            Print( Range, settings );
        }

        /// <summary>
        /// Prints all text, without any dialog windows
        /// </summary>
        public void Print( ) {
            Print( Range, new PrintDialogSettings( ) {
                ShowPageSetupDialog = false,
                ShowPrintDialog = false,
                ShowPrintPreviewDialog = false
            } );
        }

        void ShowPrintDialog( object sender, WebBrowserNavigatedEventArgs e ) {
            WebBrowser wb = sender as WebBrowser;
            PrintDialogSettings settings = wb.Tag as PrintDialogSettings;
            //prepare export with wordwrapping
            ExportToHTML exporter = new ExportToHTML( );
            exporter.UseBr = true;
            exporter.UseForwardNbsp = true;
            exporter.UseNbsp = false;
            exporter.UseStyleTag = false;
            //generate HTML
            string HTML = exporter.GetHtml( settings.printRange );
            //show print dialog
            wb.Document.Body.InnerHtml = HTML;
            if ( settings.ShowPrintPreviewDialog )
                wb.ShowPrintPreviewDialog( );
            else {
                if ( settings.ShowPageSetupDialog )
                    wb.ShowPageSetupDialog( );

                if ( settings.ShowPrintDialog )
                    wb.ShowPrintDialog( );
                else
                    wb.Print( );
            }
            //destroy webbrowser
            wb.Parent = null;
            wb.Dispose( );
        }

        protected override void Dispose( bool disposing ) {
            base.Dispose( disposing );
            if ( disposing ) {
                if ( SyntaxHighlighter != null )
                    SyntaxHighlighter.Dispose( );
                timer.Dispose( );
                timer2.Dispose( );

                if ( findForm != null )
                    findForm.Dispose( );

                if ( replaceForm != null )
                    replaceForm.Dispose( );

                foreach ( var font in this.fontsByStyle.Values )
                    font.Dispose( );

                if ( Font != null )
                    Font.Dispose( );
            }
        }

        public virtual void OnPaintLine( PaintLineEventArgs e ) {
            e.Graphics.SmoothingMode = SmoothingMode.None;

            if ( this[ e.LineIndex ].BackgroundBrush != null )
                e.Graphics.FillRectangle( this[ e.LineIndex ].BackgroundBrush, e.LineRect );

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if ( PaintLine != null )
                PaintLine( this, e );
        }

        public int GenerateUniqueLineId( ) {
            return lastLineUniqueId++;
        }

        public virtual void InsertLine( int index, Line line ) {
            lines.Insert( index, line );

            if ( LineInserted != null )
                LineInserted( this, new LineInsertedEventArgs( index, 1 ) );
        }

        public virtual void RemoveLine( int index ) {
            RemoveLine( index, 1 );
        }

        public virtual void RemoveLine( int index, int count ) {
            List<int> removedLineIds = new List<int>( );
            //
            if ( count > 0 )
                if ( LineRemoved != null )
                    for ( int i = 0 ; i < count ; i++ )
                        removedLineIds.Add( this[ index + i ].UniqueId );
            //
            lines.RemoveRange( index, count );

            if ( count > 0 )
                if ( LineRemoved != null )
                    LineRemoved( this, new LineRemovedEventArgs( index, count, removedLineIds ) );
        }
    }
    public class VisualMarker {
        public Rectangle rectangle;

        public VisualMarker( Rectangle rectangle ) {
            this.rectangle = rectangle;
        }

        public virtual void Draw( Graphics gr, Pen pen ) {
        }

        public virtual Cursor Cursor {
            get {
                return Cursors.Hand;
            }
        }
    }
    class CollapseFoldingMarker : VisualMarker {
        public readonly int iLine;

        public CollapseFoldingMarker( int iLine, Rectangle rectangle )
            : base( rectangle ) {
            this.iLine = iLine;
        }

        public override void Draw( Graphics gr, Pen pen ) {
            //draw minus
            rectangle.Height++;
            gr.DrawRectangle( new Pen( new SolidBrush( Color.FromArgb( 120, 120, 120 ) ), 2f ), rectangle );
            rectangle.Height--;
            gr.FillRectangle( new SolidBrush( Color.FromArgb( 225, 225, 225 ) ), rectangle );
            gr.DrawRectangle( new Pen( Color.FromArgb( 165, 165, 165 ) ), rectangle );
            gr.DrawLine( new Pen( Color.FromArgb( 85, 85, 85 ), rectangle.Width / 5 ), rectangle.Left + 2, rectangle.Top + rectangle.Height / 2, rectangle.Right - 2, rectangle.Top + rectangle.Height / 2 );
        }
    }
    class ExpandFoldingMarker : VisualMarker {
        public readonly int iLine;

        public ExpandFoldingMarker( int iLine, Rectangle rectangle )
            : base( rectangle ) {
            this.iLine = iLine;
        }

        public override void Draw( Graphics gr, Pen pen ) {
            //draw plus
            rectangle.Height++;
            gr.DrawRectangle( new Pen( new SolidBrush( Color.FromArgb( 120, 120, 120 ) ), 2f ), rectangle );
            rectangle.Height--;
            gr.FillRectangle( new SolidBrush( Color.FromArgb( 170, 170, 170 ) ), rectangle );
            gr.DrawRectangle( new Pen( Color.FromArgb( 165, 165, 165 ) ), rectangle );
            gr.DrawLine( new Pen( Color.FromArgb( 85, 85, 85 ), rectangle.Width / 5 ), rectangle.Left + 2, rectangle.Top + rectangle.Height / 2, rectangle.Right - 2, rectangle.Top + rectangle.Height / 2 );
            gr.DrawLine( new Pen( Color.FromArgb( 85, 85, 85 ), rectangle.Width / 5 ), rectangle.Left + rectangle.Width / 2, rectangle.Top + 2, rectangle.Left + rectangle.Width / 2, rectangle.Bottom - 2 );
        }
    }
    class FoldedAreaMarker : VisualMarker {
        public readonly int iLine;

        public FoldedAreaMarker( int iLine, Rectangle rectangle )
            : base( rectangle ) {
            this.iLine = iLine;
        }

        public override void Draw( Graphics gr, Pen pen ) {
            gr.DrawRectangle( pen, rectangle );
        }
    }
    public class StyleVisualMarker : VisualMarker {
        public Style Style {
            get;
            set;
        }

        public StyleVisualMarker( Rectangle rectangle, Style style )
            : base( rectangle ) {
            this.Style = style;
        }
    }
    public class VisualMarkerEventArgs : MouseEventArgs {
        public Style Style {
            get;
            set;
        }
        public StyleVisualMarker Marker {
            get;
            set;
        }

        public VisualMarkerEventArgs( Style style, StyleVisualMarker marker, MouseEventArgs args )
            : base( args.Button, args.Clicks, args.X, args.Y, args.Delta ) {
            this.Style = style;
            this.Marker = marker;
        }
    }
    public class SyntaxHighlighter : IDisposable {
        //styles
        public readonly Style BlueStyle = new TextStyle( Brushes.Blue, null, FontStyle.Regular );
        public readonly Style BlueBoldStyle = new TextStyle( Brushes.Blue, null, FontStyle.Bold );
        public readonly Style BoldStyle = new TextStyle( null, null, FontStyle.Bold | FontStyle.Underline );
        public readonly Style GrayStyle = new TextStyle( Brushes.Gray, null, FontStyle.Regular );
        public readonly Style MagentaStyle = new TextStyle( Brushes.Magenta, null, FontStyle.Regular );
        public readonly Style GreenStyle = new TextStyle( Brushes.Green, null, FontStyle.Italic );
        public readonly Style BrownStyle = new TextStyle( Brushes.Brown, null, FontStyle.Italic );
        public readonly Style RedStyle = new TextStyle( Brushes.Red, null, FontStyle.Regular );
        public readonly Style MaroonStyle = new TextStyle( Brushes.Maroon, null, FontStyle.Regular );
        //
        Dictionary<string, SyntaxDescriptor> descByXMLfileNames = new Dictionary<string, SyntaxDescriptor>( );

        /// <summary>
        /// Highlights syntax for given language
        /// </summary>
        public virtual void HighlightSyntax( Language language, Range range ) {
            switch ( language ) {
                case Language.CSharp:
                    CSharpSyntaxHighlight( range );
                    break;
                case Language.VB:
                    VBSyntaxHighlight( range );
                    break;
                case Language.HTML:
                    HTMLSyntaxHighlight( range );
                    break;
                case Language.SQL:
                    SQLSyntaxHighlight( range );
                    break;
                case Language.PHP:
                    PHPSyntaxHighlight( range );
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Highlights syntax for given XML description file
        /// </summary>
        public virtual void HighlightSyntax( string XMLdescriptionFile, Range range ) {
            SyntaxDescriptor desc = null;
            if ( !descByXMLfileNames.TryGetValue( XMLdescriptionFile, out desc ) ) {
                var doc = new XmlDocument( );
                string file = XMLdescriptionFile;
                if ( !File.Exists( file ) )
                    file = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName( file ) );

                doc.LoadXml( File.ReadAllText( file ) );
                desc = ParseXmlDescription( doc );
                descByXMLfileNames[ XMLdescriptionFile ] = desc;
            }
            HighlightSyntax( desc, range );
        }

        public virtual void AutoIndentNeeded( object sender, AutoIndentEventArgs args ) {
            FastColoredTextBox tb = ( sender as FastColoredTextBox );
            Language language = tb.Language;
            switch ( language ) {
                case Language.CSharp:
                    CSharpAutoIndentNeeded( sender, args );
                    break;
                case Language.VB:
                    VBAutoIndentNeeded( sender, args );
                    break;
                case Language.HTML:
                    HTMLAutoIndentNeeded( sender, args );
                    break;
                case Language.SQL:
                    SQLAutoIndentNeeded( sender, args );
                    break;
                case Language.PHP:
                    PHPAutoIndentNeeded( sender, args );
                    break;
                default:
                    break;
            }
        }

        public void PHPAutoIndentNeeded( object sender, AutoIndentEventArgs args ) {
            /*
            FastColoredTextBox tb = sender as FastColoredTextBox;
            tb.CalcAutoIndentShiftByCodeFolding(sender, args);*/
            //block {}
            if ( Regex.IsMatch( args.LineText, @"^[^""']*\{.*\}[^""']*$" ) )
                return;
            //start of block {}
            if ( Regex.IsMatch( args.LineText, @"^[^""']*\{" ) ) {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            //end of block {}
            if ( Regex.IsMatch( args.LineText, @"}[^""']*$" ) ) {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            //is unclosed operator in previous line ?
            if ( Regex.IsMatch( args.PrevLineText, @"^\s*(if|for|foreach|while|[\}\s]*else)\b[^{]*$" ) )
                if ( !Regex.IsMatch( args.PrevLineText, @"(;\s*$)|(;\s*//)" ) )//operator is unclosed
				{
                    args.Shift = args.TabLength;
                    return;
                }
        }

        public void SQLAutoIndentNeeded( object sender, AutoIndentEventArgs args ) {
            FastColoredTextBox tb = sender as FastColoredTextBox;
            tb.CalcAutoIndentShiftByCodeFolding( sender, args );
        }

        public void HTMLAutoIndentNeeded( object sender, AutoIndentEventArgs args ) {
            FastColoredTextBox tb = sender as FastColoredTextBox;
            tb.CalcAutoIndentShiftByCodeFolding( sender, args );
        }

        public void VBAutoIndentNeeded( object sender, AutoIndentEventArgs args ) {
            //end of block
            if ( Regex.IsMatch( args.LineText, @"^\s*(End|EndIf|Next|Loop)\b", RegexOptions.IgnoreCase ) ) {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            //start of declaration
            if ( Regex.IsMatch( args.LineText, @"\b(Class|Property|Enum|Structure|Sub|Function|Namespace|Interface|Get|Set)\b", RegexOptions.IgnoreCase ) ) {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            // then ...
            if ( Regex.IsMatch( args.LineText, @"\b(Then)\s*\S+", RegexOptions.IgnoreCase ) )
                return;
            //start of operator block
            if ( Regex.IsMatch( args.LineText, @"^\s*(If|While|For|Do|Try|With|Using|Select)\b", RegexOptions.IgnoreCase ) ) {
                args.ShiftNextLines = args.TabLength;
                return;
            }

            //Statements else, elseif, case etc
            if ( Regex.IsMatch( args.LineText, @"^\s*(Else|ElseIf|Case|Catch|Finally)\b", RegexOptions.IgnoreCase ) ) {
                args.Shift = -args.TabLength;
                return;
            }

            //Char _
            if ( args.PrevLineText.TrimEnd( ).EndsWith( "_" ) ) {
                args.Shift = args.TabLength;
                return;
            }
        }

        public void CSharpAutoIndentNeeded( object sender, AutoIndentEventArgs args ) {
            //block {}
            if ( Regex.IsMatch( args.LineText, @"^[^""']*\{.*\}[^""']*$" ) )
                return;
            //start of block {}
            if ( Regex.IsMatch( args.LineText, @"^[^""']*\{" ) ) {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            //end of block {}
            if ( Regex.IsMatch( args.LineText, @"}[^""']*$" ) ) {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            //label
            if ( Regex.IsMatch( args.LineText, @"^\s*\w+\s*:\s*($|//)" ) &&
                !Regex.IsMatch( args.LineText, @"^\s*default\s*:" ) ) {
                args.Shift = -args.TabLength;
                return;
            }
            //some statements: case, default
            if ( Regex.IsMatch( args.LineText, @"^\s*(case|default)\b.*:\s*($|//)" ) ) {
                args.Shift = -args.TabLength / 2;
                return;
            }
            //is unclosed operator in previous line ?
            if ( Regex.IsMatch( args.PrevLineText, @"^\s*(if|for|foreach|while|[\}\s]*else)\b[^{]*$" ) )
                if ( !Regex.IsMatch( args.PrevLineText, @"(;\s*$)|(;\s*//)" ) )//operator is unclosed
				{
                    args.Shift = args.TabLength;
                    return;
                }
        }

        public static SyntaxDescriptor ParseXmlDescription( XmlDocument doc ) {
            SyntaxDescriptor desc = new SyntaxDescriptor( );
            XmlNode brackets = doc.SelectSingleNode( "doc/brackets" );
            if ( brackets != null ) {
                if ( brackets.Attributes[ "left" ] == null || brackets.Attributes[ "right" ] == null ||
                    brackets.Attributes[ "left" ].Value == "" || brackets.Attributes[ "right" ].Value == "" ) {
                    desc.leftBracket = '\x0';
                    desc.rightBracket = '\x0';
                } else {
                    desc.leftBracket = brackets.Attributes[ "left" ].Value[ 0 ];
                    desc.rightBracket = brackets.Attributes[ "right" ].Value[ 0 ];
                }

                if ( brackets.Attributes[ "left2" ] == null || brackets.Attributes[ "right2" ] == null ||
    brackets.Attributes[ "left2" ].Value == "" || brackets.Attributes[ "right2" ].Value == "" ) {
                    desc.leftBracket2 = '\x0';
                    desc.rightBracket2 = '\x0';
                } else {
                    desc.leftBracket2 = brackets.Attributes[ "left2" ].Value[ 0 ];
                    desc.rightBracket2 = brackets.Attributes[ "right2" ].Value[ 0 ];
                }
            }

            Dictionary<string, Style> styleByName = new Dictionary<string, Style>( );

            foreach ( XmlNode style in doc.SelectNodes( "doc/style" ) ) {
                var s = ParseStyle( style );
                styleByName[ style.Attributes[ "name" ].Value ] = s;
                desc.styles.Add( s );
            }
            foreach ( XmlNode rule in doc.SelectNodes( "doc/rule" ) )
                desc.rules.Add( ParseRule( rule, styleByName ) );
            foreach ( XmlNode folding in doc.SelectNodes( "doc/folding" ) )
                desc.foldings.Add( ParseFolding( folding ) );

            return desc;
        }

        public static FoldingDesc ParseFolding( XmlNode foldingNode ) {
            FoldingDesc folding = new FoldingDesc( );
            //regex
            folding.startMarkerRegex = foldingNode.Attributes[ "start" ].Value;
            folding.finishMarkerRegex = foldingNode.Attributes[ "finish" ].Value;
            //options
            var optionsA = foldingNode.Attributes[ "options" ];
            if ( optionsA != null )
                folding.options = ( RegexOptions )Enum.Parse( typeof( RegexOptions ), optionsA.Value );

            return folding;
        }

        public static RuleDesc ParseRule( XmlNode ruleNode, Dictionary<string, Style> styles ) {
            RuleDesc rule = new RuleDesc( );
            rule.pattern = ruleNode.InnerText;
            //
            var styleA = ruleNode.Attributes[ "style" ];
            var optionsA = ruleNode.Attributes[ "options" ];
            //Style
            if ( styleA == null )
                throw new Exception( "Rule must contain style name." );
            if ( !styles.ContainsKey( styleA.Value ) )
                throw new Exception( "Style '" + styleA.Value + "' is not found." );
            rule.style = styles[ styleA.Value ];
            //options
            if ( optionsA != null )
                rule.options = ( RegexOptions )Enum.Parse( typeof( RegexOptions ), optionsA.Value );

            return rule;
        }

        public static Style ParseStyle( XmlNode styleNode ) {
            var typeA = styleNode.Attributes[ "type" ];
            var colorA = styleNode.Attributes[ "color" ];
            var backColorA = styleNode.Attributes[ "backColor" ];
            var fontStyleA = styleNode.Attributes[ "fontStyle" ];
            var nameA = styleNode.Attributes[ "name" ];
            //colors
            SolidBrush foreBrush = null;
            if ( colorA != null )
                foreBrush = new SolidBrush( ParseColor( colorA.Value ) );
            SolidBrush backBrush = null;
            if ( backColorA != null )
                backBrush = new SolidBrush( ParseColor( backColorA.Value ) );
            //fontStyle
            FontStyle fontStyle = FontStyle.Regular;
            if ( fontStyleA != null )
                fontStyle = ( FontStyle )Enum.Parse( typeof( FontStyle ), fontStyleA.Value );

            return new TextStyle( foreBrush, backBrush, fontStyle );
        }

        public static Color ParseColor( string s ) {
            if ( s.StartsWith( "#" ) ) {
                if ( s.Length <= 7 )
                    return Color.FromArgb( 255, Color.FromArgb( Int32.Parse( s.Substring( 1 ), System.Globalization.NumberStyles.AllowHexSpecifier ) ) );
                else
                    return Color.FromArgb( Int32.Parse( s.Substring( 1 ), System.Globalization.NumberStyles.AllowHexSpecifier ) );
            } else
                return Color.FromName( s );
        }

        public void HighlightSyntax( SyntaxDescriptor desc, Range range ) {
            //set style order
            range.tb.ClearStylesBuffer( );
            for ( int i = 0 ; i < desc.styles.Count ; i++ )
                range.tb.Styles[ i ] = desc.styles[ i ];
            //brackets
            range.tb.LeftBracket = desc.leftBracket;
            range.tb.RightBracket = desc.rightBracket;
            range.tb.LeftBracket2 = desc.leftBracket2;
            range.tb.RightBracket2 = desc.rightBracket2;
            //clear styles of range
            range.ClearStyle( desc.styles.ToArray( ) );
            //highlight syntax
            foreach ( var rule in desc.rules )
                range.SetStyle( rule.style, rule.Regex );
            //clear folding
            range.ClearFoldingMarkers( );
            //folding markers
            foreach ( var folding in desc.foldings )
                range.SetFoldingMarkers( folding.startMarkerRegex, folding.finishMarkerRegex, folding.options );
        }

        Regex CSharpStringRegex, CSharpCommentRegex1, CSharpCommentRegex2, CSharpCommentRegex3, CSharpNumberRegex, CSharpAttributeRegex, CSharpClassNameRegex, CSharpKeywordRegex;

        void InitCShaprRegex( ) {
            CSharpStringRegex = new Regex( @"""""|@""""|''|@"".*?""|[^@](?<range>"".*?[^\\]"")|'.*?[^\\]'", RegexOptions.Compiled );
            CSharpCommentRegex1 = new Regex( @"//.*$", RegexOptions.Multiline | RegexOptions.Compiled );
            CSharpCommentRegex2 = new Regex( @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline | RegexOptions.Compiled );
            CSharpCommentRegex3 = new Regex( @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft | RegexOptions.Compiled );
            CSharpNumberRegex = new Regex( @"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b", RegexOptions.Compiled );
            CSharpAttributeRegex = new Regex( @"^\s*(?<range>\[.+?\])\s*$", RegexOptions.Multiline | RegexOptions.Compiled );
            CSharpClassNameRegex = new Regex( @"\b(class|struct|enum|interface)\s+(?<range>\w+?)\b", RegexOptions.Compiled );
            CSharpKeywordRegex = new Regex( @"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|public|is|lock|long|namespace|new|null|object|operator|out|override|params|public|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|add|alias|ascending|descending|dynamic|from|get|global|group|into|join|let|orderby|partial|remove|select|set|value|var|where|yield)\b|#region\b|#endregion\b", RegexOptions.Compiled );
        }
        /// <summary>
        /// Highlights C# code
        /// </summary>
        /// <param name="range"></param>
        public virtual void CSharpSyntaxHighlight( Range range ) {
            range.tb.CommentPrefix = "//";
            range.tb.LeftBracket = '(';
            range.tb.RightBracket = ')';
            range.tb.LeftBracket2 = '\x0';
            range.tb.RightBracket2 = '\x0';
            //clear style of changed range
            range.ClearStyle( BlueStyle, BoldStyle, GrayStyle, MagentaStyle, GreenStyle, BrownStyle );
            //
            if ( CSharpStringRegex == null )
                InitCShaprRegex( );
            //string highlighting
            range.SetStyle( BrownStyle, CSharpStringRegex );
            //comment highlighting
            range.SetStyle( GreenStyle, CSharpCommentRegex1 );
            range.SetStyle( GreenStyle, CSharpCommentRegex2 );
            range.SetStyle( GreenStyle, CSharpCommentRegex3 );
            //number highlighting
            range.SetStyle( MagentaStyle, CSharpNumberRegex );
            //attribute highlighting
            range.SetStyle( GrayStyle, CSharpAttributeRegex );
            //class name highlighting
            range.SetStyle( BoldStyle, CSharpClassNameRegex );
            //keyword highlighting
            range.SetStyle( BlueStyle, CSharpKeywordRegex );

            //clear folding markers
            range.ClearFoldingMarkers( );
            //set folding markers
            range.SetFoldingMarkers( "{", "}" );//allow to collapse brackets block
            range.SetFoldingMarkers( @"#region\b", @"#endregion\b" );//allow to collapse #region blocks
            range.SetFoldingMarkers( @"/\*", @"\*/" );//allow to collapse comment block
        }

        Regex VBStringRegex, VBCommentRegex, VBNumberRegex, VBClassNameRegex, VBKeywordRegex;

        void InitVBRegex( ) {
            VBStringRegex = new Regex( @"""""|"".*?[^\\]""", RegexOptions.Compiled );
            VBCommentRegex = new Regex( @"'.*$", RegexOptions.Multiline | RegexOptions.Compiled );
            VBNumberRegex = new Regex( @"\b\d+[\.]?\d*([eE]\-?\d+)?\b", RegexOptions.Compiled );
            VBClassNameRegex = new Regex( @"\b(Class|Structure|Enum|Interface)[ ]+(?<range>\w+?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled );
            VBKeywordRegex = new Regex( @"\b(AddHandler|AddressOf|Alias|And|AndAlso|As|Boolean|ByRef|Byte|ByVal|Call|Case|Catch|CBool|CByte|CChar|CDate|CDbl|CDec|Char|CInt|Class|CLng|CObj|Const|Continue|CSByte|CShort|CSng|CStr|CType|CUInt|CULng|CUShort|Date|Decimal|Declare|Default|Delegate|Dim|DirectCast|Do|Double|Each|Else|ElseIf|End|EndIf|Enum|Erase|Error|Event|Exit|False|Finally|For|Friend|Function|Get|GetType|GetXMLNamespace|Global|GoSub|GoTo|Handles|If|Implements|Imports|In|Inherits|Integer|Interface|Is|IsNot|Let|Lib|Like|Long|Loop|Me|Mod|Module|MustInherit|MustOverride|MyBase|MyClass|Namespace|Narrowing|New|Next|Not|Nothing|NotInheritable|NotOverridable|Object|Of|On|Operator|Option|Optional|Or|OrElse|Overloads|Overridable|Overrides|ParamArray|Partial|public|Property|Protected|Public|RaiseEvent|ReadOnly|ReDim|REM|RemoveHandler|Resume|Return|SByte|Select|Set|Shadows|Shared|Short|Single|Static|Step|Stop|String|Structure|Sub|SyncLock|Then|Throw|To|True|Try|TryCast|TypeOf|UInteger|ULong|UShort|Using|Variant|Wend|When|While|Widening|With|WithEvents|WriteOnly|Xor|Region)\b|(#Const|#Else|#ElseIf|#End|#If|#Region)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled );
        }

        /// <summary>
        /// Highlights VB code
        /// </summary>
        /// <param name="range"></param>
        public virtual void VBSyntaxHighlight( Range range ) {
            range.tb.CommentPrefix = "'";
            range.tb.LeftBracket = '(';
            range.tb.RightBracket = ')';
            range.tb.LeftBracket2 = '\x0';
            range.tb.RightBracket2 = '\x0';
            //clear style of changed range
            range.ClearStyle( BrownStyle, GreenStyle, MagentaStyle, BoldStyle, BlueStyle );
            //
            if ( VBStringRegex == null )
                InitVBRegex( );
            //string highlighting
            range.SetStyle( BrownStyle, VBStringRegex );
            //comment highlighting
            range.SetStyle( GreenStyle, VBCommentRegex );
            //number highlighting
            range.SetStyle( MagentaStyle, VBNumberRegex );
            //class name highlighting
            range.SetStyle( BoldStyle, VBClassNameRegex );
            //keyword highlighting
            range.SetStyle( BlueStyle, VBKeywordRegex );

            //clear folding markers
            range.ClearFoldingMarkers( );
            //set folding markers
            range.SetFoldingMarkers( @"#Region\b", @"#End\s+Region\b", RegexOptions.IgnoreCase );
            range.SetFoldingMarkers( @"\b(Class|Property|Enum|Structure|Interface)[ \t]+\S+", @"\bEnd (Class|Property|Enum|Structure|Interface)\b", RegexOptions.IgnoreCase );
            range.SetFoldingMarkers( @"^\s*(?<range>While)[ \t]+\S+", @"^\s*(?<range>End While)\b", RegexOptions.Multiline | RegexOptions.IgnoreCase );
            range.SetFoldingMarkers( @"\b(Sub|Function)[ \t]+[^\s']+", @"\bEnd (Sub|Function)\b", RegexOptions.IgnoreCase );//this declared separately because Sub and Function can be unclosed
            range.SetFoldingMarkers( @"(\r|\n|^)[ \t]*(?<range>Get|Set)[ \t]*(\r|\n|$)", @"\bEnd (Get|Set)\b", RegexOptions.IgnoreCase );
            range.SetFoldingMarkers( @"^\s*(?<range>For|For\s+Each)\b", @"^\s*(?<range>Next)\b", RegexOptions.Multiline | RegexOptions.IgnoreCase );
            range.SetFoldingMarkers( @"^\s*(?<range>Do)\b", @"^\s*(?<range>Loop)\b", RegexOptions.Multiline | RegexOptions.IgnoreCase );
        }

        Regex HTMLTagRegex, HTMLTagNameRegex, HTMLEndTagRegex, HTMLAttrRegex, HTMLAttrValRegex;

        void InitHTMLRegex( ) {
            HTMLTagRegex = new Regex( @"<|/>|</|>", RegexOptions.Compiled );
            HTMLTagNameRegex = new Regex( @"<(?<range>[!\w]+)", RegexOptions.Compiled );
            HTMLEndTagRegex = new Regex( @"</(?<range>\w+)>", RegexOptions.Compiled );
            HTMLAttrRegex = new Regex( @"(?<range>\S+?)='[^']*'|(?<range>\S+)=""[^""]*""|(?<range>\S+)=\S+", RegexOptions.Compiled );
            HTMLAttrValRegex = new Regex( @"\S+?=(?<range>'[^']*')|\S+=(?<range>""[^""]*"")|\S+=(?<range>\S+)", RegexOptions.Compiled );
        }

        /// <summary>
        /// Highlights HTML code
        /// </summary>
        /// <param name="range"></param>
        public virtual void HTMLSyntaxHighlight( Range range ) {
            range.tb.CommentPrefix = null;
            range.tb.LeftBracket = '<';
            range.tb.RightBracket = '>';
            range.tb.LeftBracket2 = '(';
            range.tb.RightBracket2 = ')';
            //clear style of changed range
            range.ClearStyle( BlueStyle, MaroonStyle, RedStyle );
            //
            if ( HTMLTagRegex == null )
                InitHTMLRegex( );
            //tag brackets highlighting
            range.SetStyle( BlueStyle, HTMLTagRegex );
            //tag name
            range.SetStyle( MaroonStyle, HTMLTagNameRegex );
            //end of tag
            range.SetStyle( MaroonStyle, HTMLEndTagRegex );
            //attributes
            range.SetStyle( RedStyle, HTMLAttrRegex );
            //attribute values
            range.SetStyle( BlueStyle, HTMLAttrValRegex );

            //clear folding markers
            range.ClearFoldingMarkers( );
            //set folding markers
            range.SetFoldingMarkers( "<head", "</head>", RegexOptions.IgnoreCase );
            range.SetFoldingMarkers( "<body", "</body>", RegexOptions.IgnoreCase );
            range.SetFoldingMarkers( "<table", "</table>", RegexOptions.IgnoreCase );
            range.SetFoldingMarkers( "<form", "</form>", RegexOptions.IgnoreCase );
            range.SetFoldingMarkers( "<div", "</div>", RegexOptions.IgnoreCase );
            range.SetFoldingMarkers( "<script", "</script>", RegexOptions.IgnoreCase );
            range.SetFoldingMarkers( "<tr", "</tr>", RegexOptions.IgnoreCase );
        }

        Regex SQLStringRegex, SQLNumberRegex, SQLCommentRegex1, SQLCommentRegex2, SQLCommentRegex3, SQLVarRegex, SQLStatementsRegex, SQLKeywordsRegex, SQLFunctionsRegex;

        void InitSQLRegex( ) {
            SQLStringRegex = new Regex( @"""""|''|"".*?[^\\]""|'.*?[^\\]'", RegexOptions.Compiled );
            SQLNumberRegex = new Regex( @"\b\d+[\.]?\d*([eE]\-?\d+)?\b", RegexOptions.Compiled );
            SQLCommentRegex1 = new Regex( @"--.*$", RegexOptions.Multiline | RegexOptions.Compiled );
            SQLCommentRegex2 = new Regex( @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline | RegexOptions.Compiled );
            SQLCommentRegex3 = new Regex( @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft | RegexOptions.Compiled );
            SQLVarRegex = new Regex( @"@[a-zA-Z_\d]*\b", RegexOptions.Compiled );
            SQLStatementsRegex = new Regex( @"\b(ALTER APPLICATION ROLE|ALTER ASSEMBLY|ALTER ASYMMETRIC KEY|ALTER AUTHORIZATION|ALTER BROKER PRIORITY|ALTER CERTIFICATE|ALTER CREDENTIAL|ALTER CRYPTOGRAPHIC PROVIDER|ALTER DATABASE|ALTER DATABASE AUDIT SPECIFICATION|ALTER DATABASE ENCRYPTION KEY|ALTER ENDPOINT|ALTER EVENT SESSION|ALTER FULLTEXT CATALOG|ALTER FULLTEXT INDEX|ALTER FULLTEXT STOPLIST|ALTER FUNCTION|ALTER INDEX|ALTER LOGIN|ALTER MASTER KEY|ALTER MESSAGE TYPE|ALTER PARTITION FUNCTION|ALTER PARTITION SCHEME|ALTER PROCEDURE|ALTER QUEUE|ALTER REMOTE SERVICE BINDING|ALTER RESOURCE GOVERNOR|ALTER RESOURCE POOL|ALTER ROLE|ALTER ROUTE|ALTER SCHEMA|ALTER SERVER AUDIT|ALTER SERVER AUDIT SPECIFICATION|ALTER SERVICE|ALTER SERVICE MASTER KEY|ALTER SYMMETRIC KEY|ALTER TABLE|ALTER TRIGGER|ALTER USER|ALTER VIEW|ALTER WORKLOAD GROUP|ALTER XML SCHEMA COLLECTION|BULK INSERT|CREATE AGGREGATE|CREATE APPLICATION ROLE|CREATE ASSEMBLY|CREATE ASYMMETRIC KEY|CREATE BROKER PRIORITY|CREATE CERTIFICATE|CREATE CONTRACT|CREATE CREDENTIAL|CREATE CRYPTOGRAPHIC PROVIDER|CREATE DATABASE|CREATE DATABASE AUDIT SPECIFICATION|CREATE DATABASE ENCRYPTION KEY|CREATE DEFAULT|CREATE ENDPOINT|CREATE EVENT NOTIFICATION|CREATE EVENT SESSION|CREATE FULLTEXT CATALOG|CREATE FULLTEXT INDEX|CREATE FULLTEXT STOPLIST|CREATE FUNCTION|CREATE INDEX|CREATE LOGIN|CREATE MASTER KEY|CREATE MESSAGE TYPE|CREATE PARTITION FUNCTION|CREATE PARTITION SCHEME|CREATE PROCEDURE|CREATE QUEUE|CREATE REMOTE SERVICE BINDING|CREATE RESOURCE POOL|CREATE ROLE|CREATE ROUTE|CREATE RULE|CREATE SCHEMA|CREATE SERVER AUDIT|CREATE SERVER AUDIT SPECIFICATION|CREATE SERVICE|CREATE SPATIAL INDEX|CREATE STATISTICS|CREATE SYMMETRIC KEY|CREATE SYNONYM|CREATE TABLE|CREATE TRIGGER|CREATE TYPE|CREATE USER|CREATE VIEW|CREATE WORKLOAD GROUP|CREATE XML INDEX|CREATE XML SCHEMA COLLECTION|DELETE|DISABLE TRIGGER|DROP AGGREGATE|DROP APPLICATION ROLE|DROP ASSEMBLY|DROP ASYMMETRIC KEY|DROP BROKER PRIORITY|DROP CERTIFICATE|DROP CONTRACT|DROP CREDENTIAL|DROP CRYPTOGRAPHIC PROVIDER|DROP DATABASE|DROP DATABASE AUDIT SPECIFICATION|DROP DATABASE ENCRYPTION KEY|DROP DEFAULT|DROP ENDPOINT|DROP EVENT NOTIFICATION|DROP EVENT SESSION|DROP FULLTEXT CATALOG|DROP FULLTEXT INDEX|DROP FULLTEXT STOPLIST|DROP FUNCTION|DROP INDEX|DROP LOGIN|DROP MASTER KEY|DROP MESSAGE TYPE|DROP PARTITION FUNCTION|DROP PARTITION SCHEME|DROP PROCEDURE|DROP QUEUE|DROP REMOTE SERVICE BINDING|DROP RESOURCE POOL|DROP ROLE|DROP ROUTE|DROP RULE|DROP SCHEMA|DROP SERVER AUDIT|DROP SERVER AUDIT SPECIFICATION|DROP SERVICE|DROP SIGNATURE|DROP STATISTICS|DROP SYMMETRIC KEY|DROP SYNONYM|DROP TABLE|DROP TRIGGER|DROP TYPE|DROP USER|DROP VIEW|DROP WORKLOAD GROUP|DROP XML SCHEMA COLLECTION|ENABLE TRIGGER|EXEC|EXECUTE|FROM|INSERT|MERGE|OPTION|OUTPUT|SELECT|TOP|TRUNCATE TABLE|UPDATE|UPDATE STATISTICS|WHERE|WITH)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled );
            SQLKeywordsRegex = new Regex( @"\b(ADD|ALL|AND|ANY|AS|ASC|AUTHORIZATION|BACKUP|BEGIN|BETWEEN|BREAK|BROWSE|BY|CASCADE|CHECK|CHECKPOINT|CLOSE|CLUSTERED|COLLATE|COLUMN|COMMIT|COMPUTE|CONSTRAINT|CONTAINS|CONTINUE|CROSS|CURRENT|CURRENT_DATE|CURRENT_TIME|CURSOR|DATABASE|DBCC|DEALLOCATE|DECLARE|DEFAULT|DENY|DESC|DISK|DISTINCT|DISTRIBUTED|DOUBLE|DUMP|ELSE|END|ERRLVL|ESCAPE|EXCEPT|EXISTS|EXIT|EXTERNAL|FETCH|FILE|FILLFACTOR|FOR|FOREIGN|FREETEXT|FULL|FUNCTION|GOTO|GRANT|GROUP|HAVING|HOLDLOCK|IDENTITY|IDENTITY_INSERT|IDENTITYCOL|IF|IN|INDEX|INNER|INTERSECT|INTO|IS|JOIN|KEY|KILL|LIKE|LINENO|LOAD|NATIONAL|NOCHECK|NONCLUSTERED|NOT|NULL|OF|OFF|OFFSETS|ON|OPEN|OR|ORDER|OUTER|OVER|PERCENT|PIVOT|PLAN|PRECISION|PRIMARY|PRINT|PROC|PROCEDURE|PUBLIC|RAISERROR|READ|READTEXT|RECONFIGURE|REFERENCES|REPLICATION|RESTORE|RESTRICT|RETURN|REVERT|REVOKE|ROLLBACK|ROWCOUNT|ROWGUIDCOL|RULE|SAVE|SCHEMA|SECURITYAUDIT|SET|SHUTDOWN|SOME|STATISTICS|TABLE|TABLESAMPLE|TEXTSIZE|THEN|TO|TRAN|TRANSACTION|TRIGGER|TSEQUAL|UNION|UNIQUE|UNPIVOT|UPDATETEXT|USE|USER|VALUES|VARYING|VIEW|WAITFOR|WHEN|WHILE|WRITETEXT)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled );
            SQLFunctionsRegex = new Regex( @"(@@CONNECTIONS|@@CPU_BUSY|@@CURSOR_ROWS|@@DATEFIRST|@@DATEFIRST|@@DBTS|@@ERROR|@@FETCH_STATUS|@@IDENTITY|@@IDLE|@@IO_BUSY|@@LANGID|@@LANGUAGE|@@LOCK_TIMEOUT|@@MAX_CONNECTIONS|@@MAX_PRECISION|@@NESTLEVEL|@@OPTIONS|@@PACKET_ERRORS|@@PROCID|@@REMSERVER|@@ROWCOUNT|@@SERVERNAME|@@SERVICENAME|@@SPID|@@TEXTSIZE|@@TRANCOUNT|@@VERSION)\b|\b(ABS|ACOS|APP_NAME|ASCII|ASIN|ASSEMBLYPROPERTY|AsymKey_ID|ASYMKEY_ID|asymkeyproperty|ASYMKEYPROPERTY|ATAN|ATN2|AVG|CASE|CAST|CEILING|Cert_ID|Cert_ID|CertProperty|CHAR|CHARINDEX|CHECKSUM_AGG|COALESCE|COL_LENGTH|COL_NAME|COLLATIONPROPERTY|COLLATIONPROPERTY|COLUMNPROPERTY|COLUMNS_UPDATED|COLUMNS_UPDATED|CONTAINSTABLE|CONVERT|COS|COT|COUNT|COUNT_BIG|CRYPT_GEN_RANDOM|CURRENT_TIMESTAMP|CURRENT_TIMESTAMP|CURRENT_USER|CURRENT_USER|CURSOR_STATUS|DATABASE_PRINCIPAL_ID|DATABASE_PRINCIPAL_ID|DATABASEPROPERTY|DATABASEPROPERTYEX|DATALENGTH|DATALENGTH|DATEADD|DATEDIFF|DATENAME|DATEPART|DAY|DB_ID|DB_NAME|DECRYPTBYASYMKEY|DECRYPTBYCERT|DECRYPTBYKEY|DECRYPTBYKEYAUTOASYMKEY|DECRYPTBYKEYAUTOCERT|DECRYPTBYPASSPHRASE|DEGREES|DENSE_RANK|DIFFERENCE|ENCRYPTBYASYMKEY|ENCRYPTBYCERT|ENCRYPTBYKEY|ENCRYPTBYPASSPHRASE|ERROR_LINE|ERROR_MESSAGE|ERROR_NUMBER|ERROR_PROCEDURE|ERROR_SEVERITY|ERROR_STATE|EVENTDATA|EXP|FILE_ID|FILE_IDEX|FILE_NAME|FILEGROUP_ID|FILEGROUP_NAME|FILEGROUPPROPERTY|FILEPROPERTY|FLOOR|fn_helpcollations|fn_listextendedproperty|fn_servershareddrives|fn_virtualfilestats|fn_virtualfilestats|FORMATMESSAGE|FREETEXTTABLE|FULLTEXTCATALOGPROPERTY|FULLTEXTSERVICEPROPERTY|GETANSINULL|GETDATE|GETUTCDATE|GROUPING|HAS_PERMS_BY_NAME|HOST_ID|HOST_NAME|IDENT_CURRENT|IDENT_CURRENT|IDENT_INCR|IDENT_INCR|IDENT_SEED|IDENTITY\(|INDEX_COL|INDEXKEY_PROPERTY|INDEXPROPERTY|IS_MEMBER|IS_OBJECTSIGNED|IS_SRVROLEMEMBER|ISDATE|ISDATE|ISNULL|ISNUMERIC|Key_GUID|Key_GUID|Key_ID|Key_ID|KEY_NAME|KEY_NAME|LEFT|LEN|LOG|LOG10|LOWER|LTRIM|MAX|MIN|MONTH|NCHAR|NEWID|NTILE|NULLIF|OBJECT_DEFINITION|OBJECT_ID|OBJECT_NAME|OBJECT_SCHEMA_NAME|OBJECTPROPERTY|OBJECTPROPERTYEX|OPENDATASOURCE|OPENQUERY|OPENROWSET|OPENXML|ORIGINAL_LOGIN|ORIGINAL_LOGIN|PARSENAME|PATINDEX|PATINDEX|PERMISSIONS|PI|POWER|PUBLISHINGSERVERNAME|PWDCOMPARE|PWDENCRYPT|QUOTENAME|RADIANS|RAND|RANK|REPLACE|REPLICATE|REVERSE|RIGHT|ROUND|ROW_NUMBER|ROWCOUNT_BIG|RTRIM|SCHEMA_ID|SCHEMA_ID|SCHEMA_NAME|SCHEMA_NAME|SCOPE_IDENTITY|SERVERPROPERTY|SESSION_USER|SESSION_USER|SESSIONPROPERTY|SETUSER|SIGN|SignByAsymKey|SignByCert|SIN|SOUNDEX|SPACE|SQL_VARIANT_PROPERTY|SQRT|SQUARE|STATS_DATE|STDEV|STDEVP|STR|STUFF|SUBSTRING|SUM|SUSER_ID|SUSER_NAME|SUSER_SID|SUSER_SNAME|SWITCHOFFSET|SYMKEYPROPERTY|symkeyproperty|sys\.dm_db_index_physical_stats|sys\.fn_builtin_permissions|sys\.fn_my_permissions|SYSDATETIME|SYSDATETIMEOFFSET|SYSTEM_USER|SYSTEM_USER|SYSUTCDATETIME|TAN|TERTIARY_WEIGHTS|TEXTPTR|TODATETIMEOFFSET|TRIGGER_NESTLEVEL|TYPE_ID|TYPE_NAME|TYPEPROPERTY|UNICODE|UPDATE\(|UPPER|USER_ID|USER_NAME|USER_NAME|VAR|VARP|VerifySignedByAsymKey|VerifySignedByCert|XACT_STATE|YEAR)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled );
        }

        /// <summary>
        /// Highlights SQL code
        /// </summary>
        /// <param name="range"></param>
        public virtual void SQLSyntaxHighlight( Range range ) {
            range.tb.CommentPrefix = "--";
            range.tb.LeftBracket = '(';
            range.tb.RightBracket = ')';
            range.tb.LeftBracket2 = '\x0';
            range.tb.RightBracket2 = '\x0';
            //clear style of changed range
            range.ClearStyle( RedStyle, MagentaStyle, GreenStyle, BlueBoldStyle, BlueStyle, MaroonStyle );
            //
            if ( SQLStringRegex == null )
                InitSQLRegex( );
            //string highlighting
            range.SetStyle( RedStyle, SQLStringRegex );
            //number highlighting
            range.SetStyle( MagentaStyle, SQLNumberRegex );
            //comment highlighting
            range.SetStyle( GreenStyle, SQLCommentRegex1 );
            range.SetStyle( GreenStyle, SQLCommentRegex2 );
            range.SetStyle( GreenStyle, SQLCommentRegex3 );
            //var highlighting
            range.SetStyle( MaroonStyle, SQLVarRegex );
            //statements
            range.SetStyle( BlueBoldStyle, SQLStatementsRegex );
            //keywords
            range.SetStyle( BlueStyle, SQLKeywordsRegex );
            //functions
            range.SetStyle( MaroonStyle, SQLFunctionsRegex );

            //clear folding markers
            range.ClearFoldingMarkers( );
            //set folding markers
            range.SetFoldingMarkers( @"\bBEGIN\b", @"\bEND\b", RegexOptions.IgnoreCase );//allow to collapse BEGIN..END blocks
            range.SetFoldingMarkers( @"/\*", @"\*/" );//allow to collapse comment block
        }

        Regex PHPStringRegex, PHPNumberRegex, PHPCommentRegex1, PHPCommentRegex2, PHPCommentRegex3, PHPVarRegex, PHPKeywordRegex1, PHPKeywordRegex2, PHPKeywordRegex3;

        void InitPHPRegex( ) {
            PHPStringRegex = new Regex( @"""""|''|"".*?[^\\]""|'.*?[^\\]'", RegexOptions.Compiled );
            PHPNumberRegex = new Regex( @"\b\d+[\.]?\d*\b", RegexOptions.Compiled );
            PHPCommentRegex1 = new Regex( @"(//|#).*$", RegexOptions.Multiline | RegexOptions.Compiled );
            PHPCommentRegex2 = new Regex( @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline | RegexOptions.Compiled );
            PHPCommentRegex3 = new Regex( @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft | RegexOptions.Compiled );
            PHPVarRegex = new Regex( @"\$[a-zA-Z_\d]*\b", RegexOptions.Compiled );
            PHPKeywordRegex1 = new Regex( @"\b(die|echo|empty|exit|eval|include|include_once|isset|list|require|require_once|return|print|unset)\b", RegexOptions.Compiled );
            PHPKeywordRegex2 = new Regex( @"\b(abstract|and|array|as|break|case|catch|cfunction|class|clone|const|continue|declare|default|do|else|elseif|enddeclare|endfor|endforeach|endif|endswitch|endwhile|extends|final|for|foreach|function|global|goto|if|implements|instanceof|interface|namespace|new|or|public|protected|public|static|switch|throw|try|use|var|while|xor)\b", RegexOptions.Compiled );
            PHPKeywordRegex3 = new Regex( @"__CLASS__|__DIR__|__FILE__|__LINE__|__FUNCTION__|__METHOD__|__NAMESPACE__", RegexOptions.Compiled );
        }

        /// <summary>
        /// Highlights PHP code
        /// </summary>
        /// <param name="range"></param>
        public virtual void PHPSyntaxHighlight( Range range ) {
            range.tb.CommentPrefix = "#";
            range.tb.LeftBracket = '(';
            range.tb.RightBracket = ')';
            range.tb.LeftBracket2 = '\x0';
            range.tb.RightBracket2 = '\x0';
            //clear style of changed range
            range.ClearStyle( BlueStyle, GrayStyle, MagentaStyle, GreenStyle, RedStyle, MaroonStyle );
            //
            if ( PHPStringRegex == null )
                InitPHPRegex( );
            //string highlighting
            range.SetStyle( RedStyle, PHPStringRegex );
            //comment highlighting
            range.SetStyle( GreenStyle, PHPCommentRegex1 );
            range.SetStyle( GreenStyle, PHPCommentRegex2 );
            range.SetStyle( GreenStyle, PHPCommentRegex3 );
            //number highlighting
            range.SetStyle( RedStyle, PHPNumberRegex );
            //var highlighting
            range.SetStyle( MaroonStyle, PHPVarRegex );
            //keyword highlighting
            range.SetStyle( MagentaStyle, PHPKeywordRegex1 );
            range.SetStyle( BlueStyle, PHPKeywordRegex2 );
            range.SetStyle( GrayStyle, PHPKeywordRegex3 );

            //clear folding markers
            range.ClearFoldingMarkers( );
            //set folding markers
            range.SetFoldingMarkers( "{", "}" );//allow to collapse brackets block
            range.SetFoldingMarkers( @"/\*", @"\*/" );//allow to collapse comment block
        }

        public void Dispose( ) {
            foreach ( var desc in descByXMLfileNames.Values )
                desc.Dispose( );
        }
    }
    public enum Language {
        Custom,
        CSharp,
        VB,
        HTML,
        SQL,
        PHP
    }
    public class SyntaxDescriptor : IDisposable {
        public char leftBracket = '(';
        public char rightBracket = ')';
        public char leftBracket2 = '\x0';
        public char rightBracket2 = '\x0';
        public readonly List<Style> styles = new List<Style>( );
        public readonly List<RuleDesc> rules = new List<RuleDesc>( );
        public readonly List<FoldingDesc> foldings = new List<FoldingDesc>( );

        public void Dispose( ) {
            foreach ( var style in styles )
                style.Dispose( );
        }
    }
    public class RuleDesc {
        Regex regex;
        public string pattern;
        public RegexOptions options = RegexOptions.None;
        public Style style;

        public Regex Regex {
            get {
                if ( regex == null ) {
                    regex = new Regex( pattern, RegexOptions.Compiled | options );
                }
                return regex;
            }
        }
    }
    public class FoldingDesc {
        public string startMarkerRegex;
        public string finishMarkerRegex;
        public RegexOptions options = RegexOptions.None;
    }
    public abstract class Style : IDisposable {
        /// <summary>
        /// This style is exported to outer formats (HTML for example)
        /// </summary>
        public virtual bool IsExportable {
            get;
            set;
        }
        /// <summary>
        /// Occurs when user click on StyleVisualMarker joined to this style 
        /// </summary>
        public event EventHandler<VisualMarkerEventArgs> VisualMarkerClick;

        /// <summary>
        /// Constructor
        /// </summary>
        public Style( ) {
            IsExportable = true;
        }

        /// <summary>
        /// Renders given range of text
        /// </summary>
        /// <param name="gr">Graphics object</param>
        /// <param name="position">Position of the range in absolute control coordinates</param>
        /// <param name="range">Rendering range of text</param>
        public abstract void Draw( Graphics gr, Point position, Range range );

        /// <summary>
        /// Occurs when user click on StyleVisualMarker joined to this style 
        /// </summary>
        public virtual void OnVisualMarkerClick( FastColoredTextBox tb, VisualMarkerEventArgs args ) {
            if ( VisualMarkerClick != null )
                VisualMarkerClick( tb, args );
        }

        /// <summary>
        /// Shows VisualMarker
        /// Call this method in Draw method, when you need to show VisualMarker for your style
        /// </summary>
        protected virtual void AddVisualMarker( FastColoredTextBox tb, StyleVisualMarker marker ) {
            tb.AddVisualMarker( marker );
        }

        public static Size GetSizeOfRange( Range range ) {
            return new Size( ( range.End.iChar - range.Start.iChar ) * range.tb.CharWidth, range.tb.CharHeight );
        }

        public static GraphicsPath GetRoundedRectangle( Rectangle rect, int d ) {
            GraphicsPath gp = new GraphicsPath( );

            gp.AddArc( rect.X, rect.Y, d, d, 180, 90 );
            gp.AddArc( rect.X + rect.Width - d, rect.Y, d, d, 270, 90 );
            gp.AddArc( rect.X + rect.Width - d, rect.Y + rect.Height - d, d, d, 0, 90 );
            gp.AddArc( rect.X, rect.Y + rect.Height - d, d, d, 90, 90 );
            gp.AddLine( rect.X, rect.Y + rect.Height - d, rect.X, rect.Y + d / 2 );

            return gp;
        }

        public virtual void Dispose( ) {
            ;
        }
    }
    public class TextStyle : Style {
        public Brush ForeBrush {
            get;
            set;
        }
        public Brush BackgroundBrush {
            get;
            set;
        }
        public FontStyle FontStyle {
            get;
            set;
        }
        //public readonly Font Font;
        public StringFormat stringFormat;

        public TextStyle( Brush foreBrush, Brush backgroundBrush, FontStyle fontStyle ) {
            this.ForeBrush = foreBrush;
            this.BackgroundBrush = backgroundBrush;
            this.FontStyle = fontStyle;
            stringFormat = new StringFormat( StringFormatFlags.MeasureTrailingSpaces );
        }
        public TextStyle( ) {
        }

        public override void Draw( Graphics gr, Point position, Range range ) {
            //draw background
            if ( BackgroundBrush != null )
                gr.FillRectangle( BackgroundBrush, position.X, position.Y, ( range.End.iChar - range.Start.iChar ) * range.tb.CharWidth, range.tb.CharHeight );
            //draw chars
            Font f = new Font( range.tb.Font, FontStyle );
            //Font fHalfSize = new Font(range.tb.Font.FontFamily, f.SizeInPoints/2, FontStyle);
            Line line = range.tb[ range.Start.iLine ];
            float dx = range.tb.CharWidth;
            float y = position.Y + range.tb.LineInterval / 2;
            float x = position.X - range.tb.CharWidth / 3;

            if ( ForeBrush == null )
                ForeBrush = new SolidBrush( range.tb.ForeColor );

            //IME mode
            if ( range.tb.ImeAllowed )
                for ( int i = range.Start.iChar ; i < range.End.iChar ; i++ ) {
                    SizeF size = FastColoredTextBox.GetCharSize( f, line[ i ].c );

                    var gs = gr.Save( );
                    float k = size.Width > range.tb.CharWidth + 1 ? range.tb.CharWidth / size.Width : 1;
                    gr.TranslateTransform( x, y + ( 1 - k ) * range.tb.CharHeight / 2 );
                    gr.ScaleTransform( k, ( float )Math.Sqrt( k ) );
                    gr.DrawString( line[ i ].c.ToString( ), f, ForeBrush, 0, 0, stringFormat );
                    gr.Restore( gs );
                    /*
                    if(size.Width>range.tb.CharWidth*1.5f)
                        gr.DrawString(line[i].c.ToString(), fHalfSize, foreBrush, x, y+range.tb.CharHeight/4, stringFormat);
                    else
                        gr.DrawString(line[i].c.ToString(), f, foreBrush, x, y, stringFormat);
                     * */
                    x += dx;
                } else
                //classic mode 
                for ( int i = range.Start.iChar ; i < range.End.iChar ; i++ ) {
                    //draw char
                    gr.DrawString( line[ i ].c.ToString( ), f, ForeBrush, x, y, stringFormat );
                    x += dx;
                }
            //
            f.Dispose( );
        }

        public override void Dispose( ) {
            base.Dispose( );

            if ( ForeBrush != null )
                ForeBrush.Dispose( );
            if ( BackgroundBrush != null )
                BackgroundBrush.Dispose( );
        }
    }
    public class FoldedBlockStyle : TextStyle {
        public FoldedBlockStyle( Brush foreBrush, Brush backgroundBrush, FontStyle fontStyle ) :
            base( foreBrush, backgroundBrush, fontStyle ) {
        }

        public override void Draw( Graphics gr, Point position, Range range ) {
            if ( range.End.iChar > range.Start.iChar ) {
                base.Draw( gr, position, range );

                int firstNonSpaceSymbolX = position.X;

                //find first non space symbol
                for ( int i = range.Start.iChar ; i < range.End.iChar ; i++ )
                    if ( range.tb[ range.Start.iLine ][ i ].c != ' ' )
                        break;
                    else
                        firstNonSpaceSymbolX += range.tb.CharWidth;

                //create marker
                range.tb.AddVisualMarker( new FoldedAreaMarker( range.Start.iLine, new Rectangle( firstNonSpaceSymbolX, position.Y, position.X + ( range.End.iChar - range.Start.iChar ) * range.tb.CharWidth - firstNonSpaceSymbolX, range.tb.CharHeight ) ) );
            } else {
                //draw '...'
                using ( Font f = new Font( range.tb.Font, FontStyle ) )
                    gr.DrawString( "...", f, ForeBrush, range.tb.LeftIndent, position.Y - 2 );
                //create marker
                range.tb.AddVisualMarker( new FoldedAreaMarker( range.Start.iLine, new Rectangle( range.tb.LeftIndent + 2, position.Y, 2 * range.tb.CharHeight, range.tb.CharHeight ) ) );
            }
        }
    }
    public class SelectionStyle : Style {
        public Brush BackgroundBrush {
            get;
            set;
        }

        public override bool IsExportable {
            get {
                return false;
            }
            set {
            }
        }

        public SelectionStyle( Brush backgroundBrush ) {
            this.BackgroundBrush = backgroundBrush;
        }

        public override void Draw( Graphics gr, Point position, Range range ) {
            //draw background
            if ( BackgroundBrush != null ) {
                Rectangle rect = new Rectangle( position.X + range.tb.CharWidth / 2, position.Y, ( range.End.iChar - range.Start.iChar ) * range.tb.CharWidth - range.tb.CharWidth, range.tb.CharHeight );
                if ( rect.Width > range.tb.CharWidth / 2 ) {
                    rect = new Rectangle( position.X - 1, position.Y, range.tb.CharWidth, range.tb.CharHeight );
                    gr.FillEllipse( BackgroundBrush, rect );
                    rect.X += ( range.End.iChar - range.Start.iChar - 1 ) * range.tb.CharWidth;
                    gr.FillEllipse( BackgroundBrush, rect );
                    rect = new Rectangle( position.X + range.tb.CharWidth / 2, position.Y, ( range.End.iChar - range.Start.iChar ) * range.tb.CharWidth - range.tb.CharWidth, range.tb.CharHeight );
                    gr.FillRectangle( BackgroundBrush, rect );

                } else if ( range.End.iChar - range.Start.iChar != 0 ) {
                    rect = new Rectangle( position.X - range.tb.CharWidth / 3, position.Y - 1, range.tb.CharWidth + range.tb.CharWidth / 3 * 2, range.tb.CharHeight + 1 );
                    gr.FillEllipse( BackgroundBrush, rect );
                }
            }
        }

        public override void Dispose( ) {
            base.Dispose( );

            if ( BackgroundBrush != null )
                BackgroundBrush.Dispose( );
        }
    }
    public class MarkerStyle : Style {
        public Brush BackgroundBrush {
            get;
            set;
        }

        public MarkerStyle( Brush backgroundBrush ) {
            this.BackgroundBrush = backgroundBrush;
            IsExportable = false;
        }

        public override void Draw( Graphics gr, Point position, Range range ) {
            //draw background
            if ( BackgroundBrush != null ) {
                Rectangle rect = new Rectangle( position.X, position.Y, ( range.End.iChar - range.Start.iChar ) * range.tb.CharWidth, range.tb.CharHeight );
                if ( rect.Width == 0 )
                    return;
                //var path = GetRoundedRectangle(rect, 5);
                //gr.FillPath(BackgroundBrush, path);
                gr.FillRectangle( BackgroundBrush, rect );
            }
        }

        public override void Dispose( ) {
            base.Dispose( );

            if ( BackgroundBrush != null )
                BackgroundBrush.Dispose( );
        }
    }
    public class ShortcutStyle : Style {
        public Pen borderPen;

        public ShortcutStyle( Pen borderPen ) {
            this.borderPen = borderPen;
        }

        public override void Draw( Graphics gr, Point position, Range range ) {
            //get last char coordinates
            Point p = range.tb.PlaceToPoint( range.End );
            //draw small square under char
            Rectangle rect = new Rectangle( p.X - 5, p.Y + range.tb.CharHeight - 3, 4, 3 );
            gr.FillPath( Brushes.White, GetRoundedRectangle( rect, 1 ) );
            gr.DrawPath( borderPen, GetRoundedRectangle( rect, 1 ) );
            //add visual marker for handle mouse events
            AddVisualMarker( range.tb, new StyleVisualMarker( new Rectangle( p.X - range.tb.CharWidth, p.Y, range.tb.CharWidth, range.tb.CharHeight ), this ) );
        }

        public override void Dispose( ) {
            base.Dispose( );

            if ( borderPen != null )
                borderPen.Dispose( );
        }
    }
    class ReplaceForm : Form {
        FastColoredTextBox tb;
        bool firstSearch = true;
        Place startPlace;

        public ReplaceForm( FastColoredTextBox tb ) {
            InitializeComponent( );
            this.tb = tb;
        }

        public void btClose_Click( object sender, EventArgs e ) {
            Close( );
        }

        public void btFindNext_Click( object sender, EventArgs e ) {
            try {
                if ( !Find( ) )
                    MessageBox.Show( "Not found" );
            } catch ( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }

        List<Range> FindAll( ) {
            string pattern = tbFind.Text;
            RegexOptions opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
            if ( !cbRegex.Checked )
                pattern = Regex.Replace( pattern, VarocalCross.FindForm.RegexSpecSymbolsPattern, "\\$0" );
            if ( cbWholeWord.Checked )
                pattern = "\\b" + pattern + "\\b";
            //
            Range range = tb.Selection.Clone( );
            range.Normalize( );
            range.Start = range.End;
            range.End = new Place( tb.GetLineLength( tb.LinesCount - 1 ), tb.LinesCount - 1 );
            //
            List<Range> list = new List<Range>( );
            foreach ( var r in range.GetRanges( pattern, opt ) )
                list.Add( r );

            return list;
        }

        bool Find( ) {
            string pattern = tbFind.Text;
            RegexOptions opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
            if ( !cbRegex.Checked )
                pattern = Regex.Replace( pattern, VarocalCross.FindForm.RegexSpecSymbolsPattern, "\\$0" );
            if ( cbWholeWord.Checked )
                pattern = "\\b" + pattern + "\\b";
            //
            Range range = tb.Selection.Clone( );
            range.Normalize( );
            //
            if ( firstSearch ) {
                startPlace = range.Start;
                firstSearch = false;
            }
            //
            range.Start = range.End;
            if ( range.Start >= startPlace )
                range.End = new Place( tb.GetLineLength( tb.LinesCount - 1 ), tb.LinesCount - 1 );
            else
                range.End = startPlace;
            //
            foreach ( var r in range.GetRanges( pattern, opt ) ) {
                tb.Selection.Start = r.Start;
                tb.Selection.End = r.End;
                tb.DoSelectionVisible( );
                tb.Invalidate( );
                return true;
            }
            if ( range.Start >= startPlace && startPlace > Place.Empty ) {
                tb.Selection.Start = new Place( 0, 0 );
                return Find( );
            }
            return false;
        }

        public void tbFind_KeyPress( object sender, KeyPressEventArgs e ) {
            if ( e.KeyChar == '\r' )
                btFindNext_Click( sender, null );
            if ( e.KeyChar == '\x1b' )
                Hide( );
        }

        public void FindForm_FormClosing( object sender, FormClosingEventArgs e ) {
            if ( e.CloseReason == CloseReason.UserClosing ) {
                e.Cancel = true;
                Hide( );
            }
        }

        public void btReplace_Click( object sender, EventArgs e ) {
            try {
                if ( tb.SelectionLength != 0 )
                    tb.InsertText( tbReplace.Text );
                btFindNext_Click( sender, null );
            } catch ( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }

        public void btReplaceAll_Click( object sender, EventArgs e ) {
            try {
                tb.Selection.BeginUpdate( );
                tb.Selection.Start = new Place( 0, 0 );
                //search
                var ranges = FindAll( );
                //replace
                if ( ranges.Count > 0 ) {
                    tb.manager.ExecuteCommand( new ReplaceTextCommand( tb, ranges, tbReplace.Text ) );
                    tb.Selection.Start = new Place( 0, 0 );
                }
                //
                tb.Invalidate( );
                MessageBox.Show( ranges.Count + " occurrence(s) replaced" );
            } catch ( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
            tb.Selection.EndUpdate( );
        }

        protected override void OnActivated( EventArgs e ) {
            tbFind.Focus( );
            ResetSerach( );
        }

        void ResetSerach( ) {
            firstSearch = true;
        }

        public void cbMatchCase_CheckedChanged( object sender, EventArgs e ) {
            ResetSerach( );
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        public System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing ) {
            if ( disposing && ( components != null ) ) {
                components.Dispose( );
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        public void InitializeComponent( ) {
            this.btClose = new CheckBox( );
            this.btFindNext = new CheckBox( );
            this.tbFind = new TextBox( );
            this.cbRegex = new CheckBox( );
            this.cbMatchCase = new CheckBox( );
            this.label1 = new Label( );
            this.cbWholeWord = new CheckBox( );
            this.btReplace = new CheckBox( );
            this.btReplaceAll = new CheckBox( );
            this.label2 = new Label( );
            this.tbReplace = new TextBox( );
            this.SuspendLayout( );
            // 
            // btClose
            // 
            this.btClose.Location = new System.Drawing.Point( 273, 138 );
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size( 75, 23 );
            this.btClose.TabIndex = 7;
            this.btClose.Text = "Close";
            this.btClose.Click += new System.EventHandler( this.btClose_Click );
            // 
            // btFindNext
            // 
            this.btFindNext.Location = new System.Drawing.Point( 111, 109 );
            this.btFindNext.Name = "btFindNext";
            this.btFindNext.Size = new System.Drawing.Size( 75, 23 );
            this.btFindNext.TabIndex = 4;
            this.btFindNext.Text = "Find next";
            this.btFindNext.Click += new System.EventHandler( this.btFindNext_Click );
            // 
            // tbFind
            // 
            this.tbFind.Font = new System.Drawing.Font( "Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte )( 204 ) ) );
            this.tbFind.Location = new System.Drawing.Point( 62, 12 );
            this.tbFind.Name = "tbFind";
            this.tbFind.Size = new System.Drawing.Size( 286, 20 );
            this.tbFind.TabIndex = 0;
            this.tbFind.TextChanged += new System.EventHandler( this.cbMatchCase_CheckedChanged );
            this.tbFind.KeyPress += new System.Windows.Forms.KeyPressEventHandler( this.tbFind_KeyPress );
            // 
            // cbRegex
            // 
            this.cbRegex.AutoSize = true;
            this.cbRegex.Location = new System.Drawing.Point( 273, 38 );
            this.cbRegex.Name = "cbRegex";
            this.cbRegex.Size = new System.Drawing.Size( 57, 17 );
            this.cbRegex.TabIndex = 3;
            this.cbRegex.Text = "Regex";
            this.cbRegex.CheckedChanged += new System.EventHandler( this.cbMatchCase_CheckedChanged );
            // 
            // cbMatchCase
            // 
            this.cbMatchCase.AutoSize = true;
            this.cbMatchCase.Location = new System.Drawing.Point( 66, 38 );
            this.cbMatchCase.Name = "cbMatchCase";
            this.cbMatchCase.Size = new System.Drawing.Size( 82, 17 );
            this.cbMatchCase.TabIndex = 1;
            this.cbMatchCase.Text = "Match case";
            this.cbMatchCase.CheckedChanged += new System.EventHandler( this.cbMatchCase_CheckedChanged );
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point( 23, 14 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 33, 13 );
            this.label1.TabIndex = 5;
            this.label1.Text = "Find: ";
            // 
            // cbWholeWord
            // 
            this.cbWholeWord.AutoSize = true;
            this.cbWholeWord.Location = new System.Drawing.Point( 154, 38 );
            this.cbWholeWord.Name = "cbWholeWord";
            this.cbWholeWord.Size = new System.Drawing.Size( 113, 17 );
            this.cbWholeWord.TabIndex = 2;
            this.cbWholeWord.Text = "Match whole word";
            this.cbWholeWord.CheckedChanged += new System.EventHandler( this.cbMatchCase_CheckedChanged );
            // 
            // btReplace
            // 
            this.btReplace.Location = new System.Drawing.Point( 192, 109 );
            this.btReplace.Name = "btReplace";
            this.btReplace.Size = new System.Drawing.Size( 75, 23 );
            this.btReplace.TabIndex = 5;
            this.btReplace.Text = "Replace";
            this.btReplace.Click += new System.EventHandler( this.btReplace_Click );
            // 
            // btReplaceAll
            // 
            this.btReplaceAll.Location = new System.Drawing.Point( 273, 109 );
            this.btReplaceAll.Name = "btReplaceAll";
            this.btReplaceAll.Size = new System.Drawing.Size( 75, 23 );
            this.btReplaceAll.TabIndex = 6;
            this.btReplaceAll.Text = "Replace all";
            this.btReplaceAll.Click += new System.EventHandler( this.btReplaceAll_Click );
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point( 6, 74 );
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size( 50, 13 );
            this.label2.TabIndex = 9;
            this.label2.Text = "Replace:";
            // 
            // tbReplace
            // 
            this.tbReplace.Font = new System.Drawing.Font( "Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte )( 204 ) ) );
            this.tbReplace.Location = new System.Drawing.Point( 62, 71 );
            this.tbReplace.Name = "tbReplace";
            this.tbReplace.Size = new System.Drawing.Size( 286, 20 );
            this.tbReplace.TabIndex = 0;
            this.tbReplace.TextChanged += new System.EventHandler( this.cbMatchCase_CheckedChanged );
            this.tbReplace.KeyPress += new System.Windows.Forms.KeyPressEventHandler( this.tbFind_KeyPress );
            // 
            // ReplaceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 360, 173 );
            this.Controls.Add( this.tbFind );
            this.Controls.Add( this.label2 );
            this.Controls.Add( this.tbReplace );
            this.Controls.Add( this.btReplaceAll );
            this.Controls.Add( this.btReplace );
            this.Controls.Add( this.cbWholeWord );
            this.Controls.Add( this.label1 );
            this.Controls.Add( this.cbMatchCase );
            this.Controls.Add( this.cbRegex );
            this.Controls.Add( this.btFindNext );
            this.Controls.Add( this.btClose );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ReplaceForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Find and replace";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.FindForm_FormClosing );
            this.ResumeLayout( false );
            this.PerformLayout( );

        }

        #endregion

        public CheckBox btClose;
        public CheckBox btFindNext;
        public CheckBox cbRegex;
        public CheckBox cbMatchCase;
        public Label label1;
        public CheckBox cbWholeWord;
        public CheckBox btReplace;
        public CheckBox btReplaceAll;
        public Label label2;
        public TextBox tbReplace;
        public TextBox tbFind;
    }
    public class Range : IEnumerable<Place> {
        Place start;
        Place end;
        public readonly FastColoredTextBox tb;
        int preferedPos = -1;
        int updating = 0;

        string cachedText;
        List<Place> cachedCharIndexToPlace;
        int cachedTextVersion = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        public Range( FastColoredTextBox tb ) {
            this.tb = tb;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Range( FastColoredTextBox tb, int iStartChar, int iStartLine, int iEndChar, int iEndLine )
            : this( tb ) {
            start = new Place( iStartChar, iStartLine );
            end = new Place( iEndChar, iEndLine );
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Range( FastColoredTextBox tb, Place start, Place end )
            : this( tb ) {
            this.start = start;
            this.end = end;
        }

        public bool Contains( Place place ) {
            if ( place.iLine < Math.Min( start.iLine, end.iLine ) )
                return false;
            if ( place.iLine > Math.Max( start.iLine, end.iLine ) )
                return false;

            Place s = start;
            Place e = end;

            if ( s.iLine > e.iLine || ( s.iLine == e.iLine && s.iChar > e.iChar ) ) {
                var temp = s;
                s = e;
                e = temp;
            }

            if ( place.iLine == s.iLine && place.iChar < s.iChar )
                return false;
            if ( place.iLine == e.iLine && place.iChar > e.iChar )
                return false;

            return true;
        }

        /// <summary>
        /// Returns intersection with other range
        /// null returned otherwise
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public Range GetIntersectionWith( Range range ) {
            Range r1 = this.Clone( );
            Range r2 = range.Clone( );
            r1.Normalize( );
            r2.Normalize( );
            Place newStart = r1.Start > r2.Start ? r1.Start : r2.Start;
            Place newEnd = r1.End < r2.End ? r1.End : r2.End;
            if ( newEnd < newStart )
                return new Range( tb, start, start );
            return tb.GetRange( newStart, newEnd );
        }

        /// <summary>
        /// Select all chars of control
        /// </summary>
        public void SelectAll( ) {
            Start = new Place( 0, 0 );
            if ( tb.LinesCount == 0 )
                Start = new Place( 0, 0 );
            else {
                end = new Place( 0, 0 );
                start = new Place( tb[ tb.LinesCount - 1 ].Count, tb.LinesCount - 1 );
            }
            if ( this == tb.Selection )
                tb.Invalidate( );
        }

        /// <summary>
        /// Start line and char position
        /// </summary>
        public Place Start {
            get {
                return start;
            }
            set {
                end = start = value;
                preferedPos = -1;

                OnSelectionChanged( );
            }
        }

        /// <summary>
        /// Finish line and char position
        /// </summary>
        public Place End {
            get {
                return end;
            }
            set {
                end = value;
                OnSelectionChanged( );
            }
        }

        /// <summary>
        /// Text of range
        /// </summary>
        /// <remarks>This property has not 'set' accessor because undo/redo stack works only with 
        /// FastColoredTextBox.Selection range. So, if you want to set text, you need to use FastColoredTextBox.Selection
        /// and FastColoredTextBox.InsertText() mehtod.
        /// </remarks>
        public string Text {
            get {
                int fromLine = Math.Min( end.iLine, start.iLine );
                int toLine = Math.Max( end.iLine, start.iLine );
                int fromChar = FromX;
                int toChar = ToX;
                if ( fromLine < 0 )
                    return null;
                //
                StringBuilder sb = new StringBuilder( );
                for ( int y = fromLine ; y <= toLine ; y++ ) {
                    int fromX = y == fromLine ? fromChar : 0;
                    int toX = y == toLine ? toChar - 1 : tb[ y ].Count - 1;
                    for ( int x = fromX ; x <= toX ; x++ )
                        sb.Append( tb[ y ][ x ].c );
                    if ( y != toLine && fromLine != toLine )
                        sb.AppendLine( );
                }
                return sb.ToString( );
            }
        }

        public void GetText( out string text, out List<Place> charIndexToPlace ) {
            //try get cached text
            if ( tb.TextVersion == cachedTextVersion ) {
                text = cachedText;
                charIndexToPlace = cachedCharIndexToPlace;
                return;
            }
            //
            int fromLine = Math.Min( end.iLine, start.iLine );
            int toLine = Math.Max( end.iLine, start.iLine );
            int fromChar = FromX;
            int toChar = ToX;

            StringBuilder sb = new StringBuilder( ( toLine - fromLine ) * 100 );
            charIndexToPlace = new List<Place>( sb.Capacity );
            if ( fromLine >= 0 ) {
                for ( int y = fromLine ; y <= toLine ; y++ ) {
                    int fromX = y == fromLine ? fromChar : 0;
                    int toX = y == toLine ? toChar - 1 : tb[ y ].Count - 1;
                    for ( int x = fromX ; x <= toX ; x++ ) {
                        sb.Append( tb[ y ][ x ].c );
                        charIndexToPlace.Add( new Place( x, y ) );
                    }
                    if ( y != toLine && fromLine != toLine )
                        foreach ( char c in Environment.NewLine ) {
                            sb.Append( c );
                            charIndexToPlace.Add( new Place( tb[ y ].Count/*???*/, y ) );
                        }
                }
            }
            text = sb.ToString( );
            charIndexToPlace.Add( End > Start ? End : Start );
            //caching
            cachedText = text;
            cachedCharIndexToPlace = charIndexToPlace;
            cachedTextVersion = tb.TextVersion;
        }

        /// <summary>
        /// Returns first char after Start place
        /// </summary>
        public char CharAfterStart {
            get {
                if ( Start.iChar >= tb[ Start.iLine ].Count )
                    return '\n';
                else
                    return tb[ Start.iLine ][ Start.iChar ].c;
            }
        }

        /// <summary>
        /// Returns first char before Start place
        /// </summary>
        public char CharBeforeStart {
            get {
                if ( Start.iChar <= 0 )
                    return '\n';
                else
                    return tb[ Start.iLine ][ Start.iChar - 1 ].c;
            }
        }

        /// <summary>
        /// Clone range
        /// </summary>
        /// <returns></returns>
        public Range Clone( ) {
            return ( Range )MemberwiseClone( );
        }

        /// <summary>
        /// Return minimum of end.X and start.X
        /// </summary>
        public int FromX {
            get {
                if ( end.iLine < start.iLine )
                    return end.iChar;
                if ( end.iLine > start.iLine )
                    return start.iChar;
                return Math.Min( end.iChar, start.iChar );
            }
        }

        /// <summary>
        /// Return maximum of end.X and start.X
        /// </summary>
        public int ToX {
            get {
                if ( end.iLine < start.iLine )
                    return start.iChar;
                if ( end.iLine > start.iLine )
                    return end.iChar;
                return Math.Max( end.iChar, start.iChar );
            }
        }

        /// <summary>
        /// Move range right
        /// </summary>
        /// <remarks>This method jump over folded blocks</remarks>
        public bool GoRight( ) {
            Place prevStart = start;
            GoRight( false );
            return prevStart != start;
        }

        /// <summary>
        /// Move range left
        /// </summary>
        /// <remarks>This method can to go inside folded blocks</remarks>
        public bool GoRightThroughFolded( ) {
            if ( start.iLine >= tb.LinesCount - 1 && start.iChar >= tb[ tb.LinesCount - 1 ].Count )
                return false;

            if ( start.iChar < tb[ start.iLine ].Count )
                start.Offset( 1, 0 );
            else
                start = new Place( 0, start.iLine + 1 );

            preferedPos = -1;
            end = start;
            OnSelectionChanged( );
            return true;
        }

        /// <summary>
        /// Move range left
        /// </summary>
        /// <remarks>This method jump over folded blocks</remarks>
        public bool GoLeft( ) {
            Place prevStart = start;
            GoLeft( false );
            return prevStart != start;
        }

        /// <summary>
        /// Move range left
        /// </summary>
        /// <remarks>This method can to go inside folded blocks</remarks>
        public bool GoLeftThroughFolded( ) {
            if ( start.iChar == 0 && start.iLine == 0 )
                return false;

            if ( start.iChar > 0 )
                start.Offset( -1, 0 );
            else
                start = new Place( tb[ start.iLine - 1 ].Count, start.iLine - 1 );

            preferedPos = -1;
            end = start;
            OnSelectionChanged( );
            return true;
        }

        public void GoLeft( bool shift ) {
            if ( start.iChar != 0 || start.iLine != 0 ) {
                if ( start.iChar > 0 && tb[ start.iLine ].VisibleState == VisibleState.Visible )
                    start.Offset( -1, 0 );
                else {
                    int i = tb.FindPrevVisibleLine( start.iLine );
                    if ( i == start.iLine )
                        return;
                    start = new Place( tb[ i ].Count, i );
                }
            }

            if ( !shift )
                end = start;

            OnSelectionChanged( );

            preferedPos = -1;
        }

        public void GoRight( bool shift ) {
            if ( start.iLine < tb.LinesCount - 1 || start.iChar < tb[ tb.LinesCount - 1 ].Count ) {
                if ( start.iChar < tb[ start.iLine ].Count && tb[ start.iLine ].VisibleState == VisibleState.Visible )
                    start.Offset( 1, 0 );
                else {
                    int i = tb.FindNextVisibleLine( start.iLine );
                    if ( i == start.iLine )
                        return;
                    start = new Place( 0, i );
                }
            }

            if ( !shift )
                end = start;

            OnSelectionChanged( );

            preferedPos = -1;
        }

        public void GoUp( bool shift ) {
            if ( preferedPos < 0 )
                preferedPos = start.iChar - tb[ start.iLine ].GetWordWrapStringStartPosition( tb[ start.iLine ].GetWordWrapStringIndex( start.iChar ) );

            int iWW = tb[ start.iLine ].GetWordWrapStringIndex( start.iChar );
            if ( iWW == 0 ) {
                if ( start.iLine <= 0 )
                    return;
                int i = tb.FindPrevVisibleLine( start.iLine );
                if ( i == start.iLine )
                    return;
                start.iLine = i;
                iWW = tb[ start.iLine ].WordWrapStringsCount;
            }

            if ( iWW > 0 ) {
                int finish = tb[ start.iLine ].GetWordWrapStringFinishPosition( iWW - 1 );
                start.iChar = tb[ start.iLine ].GetWordWrapStringStartPosition( iWW - 1 ) + preferedPos;
                if ( start.iChar > finish + 1 )
                    start.iChar = finish + 1;
            }

            if ( !shift )
                end = start;

            OnSelectionChanged( );
        }

        public void GoPageUp( bool shift ) {
            if ( preferedPos < 0 )
                preferedPos = start.iChar - tb[ start.iLine ].GetWordWrapStringStartPosition( tb[ start.iLine ].GetWordWrapStringIndex( start.iChar ) );

            int pageHeight = tb.ClientRectangle.Height / tb.CharHeight - 1;

            for ( int i = 0 ; i < pageHeight ; i++ ) {
                int iWW = tb[ start.iLine ].GetWordWrapStringIndex( start.iChar );
                if ( iWW == 0 ) {
                    if ( start.iLine <= 0 )
                        break;
                    //pass hidden
                    int newLine = tb.FindPrevVisibleLine( start.iLine );
                    if ( newLine == start.iLine )
                        break;
                    start.iLine = newLine;
                    iWW = tb[ start.iLine ].WordWrapStringsCount;
                }

                if ( iWW > 0 ) {
                    int finish = tb[ start.iLine ].GetWordWrapStringFinishPosition( iWW - 1 );
                    start.iChar = tb[ start.iLine ].GetWordWrapStringStartPosition( iWW - 1 ) + preferedPos;
                    if ( start.iChar > finish + 1 )
                        start.iChar = finish + 1;
                }
            }

            if ( !shift )
                end = start;

            OnSelectionChanged( );
        }

        public void GoDown( bool shift ) {
            if ( preferedPos < 0 )
                preferedPos = start.iChar - tb[ start.iLine ].GetWordWrapStringStartPosition( tb[ start.iLine ].GetWordWrapStringIndex( start.iChar ) );

            int iWW = tb[ start.iLine ].GetWordWrapStringIndex( start.iChar );
            if ( iWW >= tb[ start.iLine ].WordWrapStringsCount - 1 ) {
                if ( start.iLine >= tb.LinesCount - 1 )
                    return;
                //pass hidden
                int i = tb.FindNextVisibleLine( start.iLine );
                if ( i == start.iLine )
                    return;
                start.iLine = i;
                iWW = -1;
            }

            if ( iWW < tb[ start.iLine ].WordWrapStringsCount - 1 ) {
                int finish = tb[ start.iLine ].GetWordWrapStringFinishPosition( iWW + 1 );
                start.iChar = tb[ start.iLine ].GetWordWrapStringStartPosition( iWW + 1 ) + preferedPos;
                if ( start.iChar > finish + 1 )
                    start.iChar = finish + 1;
            }

            if ( !shift )
                end = start;

            OnSelectionChanged( );
        }

        public void GoPageDown( bool shift ) {
            if ( preferedPos < 0 )
                preferedPos = start.iChar - tb[ start.iLine ].GetWordWrapStringStartPosition( tb[ start.iLine ].GetWordWrapStringIndex( start.iChar ) );

            int pageHeight = tb.ClientRectangle.Height / tb.CharHeight - 1;

            for ( int i = 0 ; i < pageHeight ; i++ ) {
                int iWW = tb[ start.iLine ].GetWordWrapStringIndex( start.iChar );
                if ( iWW >= tb[ start.iLine ].WordWrapStringsCount - 1 ) {
                    if ( start.iLine >= tb.LinesCount - 1 )
                        break;
                    //pass hidden
                    int newLine = tb.FindNextVisibleLine( start.iLine );
                    if ( newLine == start.iLine )
                        break;
                    start.iLine = newLine;
                    iWW = -1;
                }

                if ( iWW < tb[ start.iLine ].WordWrapStringsCount - 1 ) {
                    int finish = tb[ start.iLine ].GetWordWrapStringFinishPosition( iWW + 1 );
                    start.iChar = tb[ start.iLine ].GetWordWrapStringStartPosition( iWW + 1 ) + preferedPos;
                    if ( start.iChar > finish + 1 )
                        start.iChar = finish + 1;
                }
            }

            if ( !shift )
                end = start;

            OnSelectionChanged( );
        }

        public void GoHome( bool shift ) {
            if ( start.iLine < 0 )
                return;

            if ( tb[ start.iLine ].VisibleState != VisibleState.Visible )
                return;

            start = new Place( 0, start.iLine );

            if ( !shift )
                end = start;

            OnSelectionChanged( );

            preferedPos = -1;
        }

        public void GoEnd( bool shift ) {
            if ( start.iLine < 0 )
                return;
            if ( tb[ start.iLine ].VisibleState != VisibleState.Visible )
                return;

            start = new Place( tb[ start.iLine ].Count, start.iLine );

            if ( !shift )
                end = start;

            OnSelectionChanged( );

            preferedPos = -1;
        }

        /// <summary>
        /// Set style for range
        /// </summary>
        public void SetStyle( Style style,bool inv=true ) {
            //search code for style
            int code = tb.GetOrSetStyleLayerIndex( style );
            //set code to chars
            SetStyle( ToStyleIndex( code ) );
            //
            if ( inv )
                tb.Invalidate( );
        }

        /// <summary>
        /// Set style for given regex pattern
        /// </summary>
        public void SetStyle( Style style, string regexPattern ) {
            //search code for style
            StyleIndex layer = ToStyleIndex( tb.GetOrSetStyleLayerIndex( style ) );
            SetStyle( layer, regexPattern, RegexOptions.None );
        }

        /// <summary>
        /// Set style for given regex
        /// </summary>
        public void SetStyle( Style style, Regex regex ) {
            //search code for style
            StyleIndex layer = ToStyleIndex( tb.GetOrSetStyleLayerIndex( style ) );
            SetStyle( layer, regex );
        }

        public Range getRange( int iline, int ichar, int iline2, int ichar2 ) {
            return new Range( tb, new Place( Start.iChar + ichar, Start.iLine + iline ), new Place( Start.iChar + ichar + ichar2, Start.iLine + iline + iline2 ) );
        }

        /// <summary>
        /// Set style for given regex pattern
        /// </summary>
        public void SetStyle( Style style, string regexPattern, RegexOptions options ) {
            //search code for style
            StyleIndex layer = ToStyleIndex( tb.GetOrSetStyleLayerIndex( style ) );
            SetStyle( layer, regexPattern, options );
        }

        /// <summary>
        /// Set style for given regex pattern
        /// </summary>
        public void SetStyle( StyleIndex styleLayer, string regexPattern, RegexOptions options ) {
            if ( Math.Abs( Start.iLine - End.iLine ) > 1000 )
                options |= RegexOptions.Compiled;
            //
            foreach ( var range in GetRanges( regexPattern, options ) )
                range.SetStyle( styleLayer );
            //
            tb.Invalidate( );
        }

        /// <summary>
        /// Set style for given regex pattern
        /// </summary>
        public void SetStyle( StyleIndex styleLayer, Regex regex ) {
            foreach ( var range in GetRanges( regex ) )
                range.SetStyle( styleLayer );
            //
            tb.Invalidate( );
        }

        /// <summary>
        /// Appends style to chars of range
        /// </summary>
        public void SetStyle( StyleIndex styleIndex ) {
            //set code to chars
            int fromLine = Math.Min( End.iLine, Start.iLine );
            int toLine = Math.Max( End.iLine, Start.iLine );
            int fromChar = FromX;
            int toChar = ToX;
            if ( fromLine < 0 )
                return;
            //
            for ( int y = fromLine ; y <= toLine ; y++ ) {
                int fromX = y == fromLine ? fromChar : 0;
                int toX = y == toLine ? toChar - 1 : tb[ y ].Count - 1;
                for ( int x = fromX ; x <= toX ; x++ ) {
                    Char c = tb[ y ][ x ];
                    c.style |= styleIndex;
                    tb[ y ][ x ] = c;
                }
            }
        }

        /// <summary>
        /// Sets folding markers
        /// </summary>
        /// <param name="startFoldingPattern">Pattern for start folding line</param>
        /// <param name="finishFoldingPattern">Pattern for finish folding line</param>
        public void SetFoldingMarkers( string startFoldingPattern, string finishFoldingPattern ) {
            SetFoldingMarkers( startFoldingPattern, finishFoldingPattern, RegexOptions.Compiled );
        }

        /// <summary>
        /// Sets folding markers
        /// </summary>
        /// <param name="startFoldingPattern">Pattern for start folding line</param>
        /// <param name="finishFoldingPattern">Pattern for finish folding line</param>
        public void SetFoldingMarkers( string startFoldingPattern, string finishFoldingPattern, RegexOptions options ) {
            foreach ( var range in GetRanges( startFoldingPattern, options ) )
                tb[ range.Start.iLine ].FoldingStartMarker = startFoldingPattern;

            foreach ( var range in GetRanges( finishFoldingPattern, options ) )
                tb[ range.Start.iLine ].FoldingEndMarker = startFoldingPattern;
            //
            tb.Invalidate( );
        }
        /// <summary>
        /// Finds ranges for given regex pattern
        /// </summary>
        /// <param name="regexPattern">Regex pattern</param>
        /// <returns>Enumeration of ranges</returns>
        public IEnumerable<Range> GetRanges( string regexPattern ) {
            return GetRanges( regexPattern, RegexOptions.None );
        }

        /// <summary>
        /// Finds ranges for given regex pattern
        /// </summary>
        /// <param name="regexPattern">Regex pattern</param>
        /// <returns>Enumeration of ranges</returns>
        public IEnumerable<Range> GetRanges( string regexPattern, RegexOptions options ) {
            //get text
            string text;
            List<Place> charIndexToPlace;
            GetText( out text, out charIndexToPlace );
            //create regex
            Regex regex = new Regex( regexPattern, options );
            //
            foreach ( Match m in regex.Matches( text ) ) {
                Range r = new Range( this.tb );
                //try get 'range' group, otherwise use group 0
                Group group = m.Groups[ "range" ];
                if ( !group.Success )
                    group = m.Groups[ 0 ];
                //
                r.Start = charIndexToPlace[ group.Index ];
                r.End = charIndexToPlace[ group.Index + group.Length ];
                yield return r;
            }
        }

        /// <summary>
        /// Finds ranges for given regex
        /// </summary>
        /// <returns>Enumeration of ranges</returns>
        public IEnumerable<Range> GetRanges( Regex regex ) {
            //get text
            string text;
            List<Place> charIndexToPlace;
            GetText( out text, out charIndexToPlace );
            //
            foreach ( Match m in regex.Matches( text ) ) {
                Range r = new Range( this.tb );
                //try get 'range' group, otherwise use group 0
                Group group = m.Groups[ "range" ];
                if ( !group.Success )
                    group = m.Groups[ 0 ];
                //
                r.Start = charIndexToPlace[ group.Index ];
                r.End = charIndexToPlace[ group.Index + group.Length ];
                yield return r;
            }
        }

        /// <summary>
        /// Clear styles of range
        /// </summary>
        public void ClearStyle( params Style[ ] styles ) {
            try {
                ClearStyle( tb.GetStyleIndexMask( styles ) );
            } catch {
                ;
            }
        }

        /// <summary>
        /// Clear styles of range
        /// </summary>
        public void ClearStyle( StyleIndex styleIndex ) {
            //set code to chars
            int fromLine = Math.Min( End.iLine, Start.iLine );
            int toLine = Math.Max( End.iLine, Start.iLine );
            int fromChar = FromX;
            int toChar = ToX;
            if ( fromLine < 0 )
                return;
            //
            for ( int y = fromLine ; y <= toLine ; y++ ) {
                int fromX = y == fromLine ? fromChar : 0;
                int toX = y == toLine ? toChar - 1 : tb[ y ].Count - 1;
                for ( int x = fromX ; x <= toX ; x++ ) {
                    Char c = tb[ y ][ x ];
                    c.style &= ~styleIndex;
                    tb[ y ][ x ] = c;
                }
            }
            //
            tb.Invalidate( );
        }
        public void ClearStyles( bool inv=true ) {
            //set code to chars
            int fromLine = Math.Min( End.iLine, Start.iLine );
            int toLine = Math.Max( End.iLine, Start.iLine );
            int fromChar = FromX;
            int toChar = ToX;
            if ( fromLine < 0 )
                return;
            //
            for ( int y = fromLine ; y <= toLine ; y++ ) {
                int fromX = y == fromLine ? fromChar : 0;
                int toX = y == toLine ? toChar - 1 : tb[ y ].Count - 1;
                for ( int x = fromX ; x <= toX ; x++ ) {
                    Char c = tb[ y ][ x ];
                    c.style ^= c.style;
                    tb[ y ][ x ] = c;
                }
            }
            //
            if ( inv )
                tb.Invalidate( );
        }

        /// <summary>
        /// Clear folding markers of all lines of range
        /// </summary>
        public void ClearFoldingMarkers( bool inv=true ) {
            //set code to chars
            int fromLine = Math.Min( End.iLine, Start.iLine );
            int toLine = Math.Max( End.iLine, Start.iLine );
            if ( fromLine < 0 )
                return;
            //
            for ( int y = fromLine ; y <= toLine ; y++ )
                tb[ y ].ClearFoldingMarkers( );
            //
            if ( inv )
                tb.Invalidate( );
        }

        void OnSelectionChanged( ) {
            //clear cache
            cachedTextVersion = -1;
            cachedText = null;
            cachedCharIndexToPlace = null;
            //
            if ( tb.Selection == this )
                if ( updating == 0 )
                    tb.OnSelectionChanged( );
        }

        /// <summary>
        /// Starts selection position updating
        /// </summary>
        public void BeginUpdate( ) {
            updating++;
        }

        /// <summary>
        /// Ends selection position updating
        /// </summary>
        public void EndUpdate( ) {
            updating--;
            if ( updating == 0 )
                OnSelectionChanged( );
        }

        public override string ToString( ) {
            return "Start: " + Start + " End: " + End;
        }

        /// <summary>
        /// Exchanges Start and End if End appears before Start
        /// </summary>
        public void Normalize( ) {
            if ( Start > End )
                Inverse( );
        }

        /// <summary>
        /// Exchanges Start and End
        /// </summary>
        public void Inverse( ) {
            var temp = start;
            start = end;
            end = temp;
        }

        /// <summary>
        /// Expands range from first char of Start line to last char of End line
        /// </summary>
        public void Expand( ) {
            Normalize( );
            start = new Place( 0, start.iLine );
            end = new Place( tb.GetLineLength( end.iLine ), end.iLine );
        }

        IEnumerator<Place> IEnumerable<Place>.GetEnumerator( ) {
            int fromLine = Math.Min( end.iLine, start.iLine );
            int toLine = Math.Max( end.iLine, start.iLine );
            int fromChar = FromX;
            int toChar = ToX;
            if ( fromLine < 0 )
                yield break;
            //
            for ( int y = fromLine ; y <= toLine ; y++ ) {
                int fromX = y == fromLine ? fromChar : 0;
                int toX = y == toLine ? toChar - 1 : tb[ y ].Count - 1;
                for ( int x = fromX ; x <= toX ; x++ )
                    yield return new Place( x, y );
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( ) {
            return ( this as IEnumerable<Place> ).GetEnumerator( );
        }

        /// <summary>
        /// Get fragment of text around Start place. Returns maximal mathed to pattern fragment.
        /// </summary>
        /// <param name="allowedSymbolsPattern">Allowed chars pattern for fragment</param>
        /// <returns>Range of found fragment</returns>
        public Range GetFragment( string allowedSymbolsPattern ) {
            return GetFragment( allowedSymbolsPattern, RegexOptions.None );
        }

        /// <summary>
        /// Get fragment of text around Start place. Returns maximal mathed to pattern fragment.
        /// </summary>
        /// <param name="allowedSymbolsPattern">Allowed chars pattern for fragment</param>
        /// <returns>Range of found fragment</returns>
        public Range GetFragment( string allowedSymbolsPattern, RegexOptions options ) {
            Range r = new Range( tb );
            r.Start = Start;
            Regex regex = new Regex( allowedSymbolsPattern, options );
            //go left, check symbols
            while ( r.GoLeftThroughFolded( ) ) {
                if ( !regex.IsMatch( r.CharAfterStart.ToString( ) ) ) {
                    r.GoRightThroughFolded( );
                    break;
                }
            }
            Place startFragment = r.Start;

            r.Start = Start;
            //go right, check symbols
            do {
                if ( !regex.IsMatch( r.CharAfterStart.ToString( ) ) )
                    break;
            } while ( r.GoRightThroughFolded( ) );
            Place endFragment = r.Start;

            return new Range( tb, startFragment, endFragment );
        }

        bool IsIdentifierChar( char c ) {
            return char.IsLetterOrDigit( c ) || c == '_';
        }

        public void GoWordLeft( bool shift ) {
            Range range = this.Clone( );//for OnSelectionChanged disable

            Place prev;
            bool findIdentifier = IsIdentifierChar( range.CharBeforeStart );

            do {
                prev = range.Start;
                if ( IsIdentifierChar( range.CharBeforeStart ) ^ findIdentifier )
                    break;

                //move left
                range.GoLeft( shift );
            } while ( prev != range.Start );

            this.Start = range.Start;
            this.End = range.End;

            if ( tb[ Start.iLine ].VisibleState != VisibleState.Visible )
                GoRight( shift );
        }

        public void GoWordRight( bool shift ) {
            Range range = this.Clone( );//for OnSelectionChanged disable

            Place prev;
            bool findIdentifier = IsIdentifierChar( range.CharAfterStart );

            do {
                prev = range.Start;
                if ( IsIdentifierChar( range.CharAfterStart ) ^ findIdentifier )
                    break;

                //move right
                range.GoRight( shift );
            } while ( prev != range.Start );

            this.Start = range.Start;
            this.End = range.End;

            if ( tb[ Start.iLine ].VisibleState != VisibleState.Visible )
                GoLeft( shift );
        }

        public void GoFirst( bool shift ) {
            start = new Place( 0, 0 );
            if ( tb[ Start.iLine ].VisibleState != VisibleState.Visible )
                GoRight( shift );
            if ( !shift )
                end = start;

            OnSelectionChanged( );
        }

        public void GoLast( bool shift ) {
            start = new Place( tb[ tb.LinesCount - 1 ].Count, tb.LinesCount - 1 );
            if ( tb[ Start.iLine ].VisibleState != VisibleState.Visible )
                GoLeft( shift );
            if ( !shift )
                end = start;

            OnSelectionChanged( );
        }

        public static StyleIndex ToStyleIndex( int i ) {
            return ( StyleIndex )( 1 << i );
        }
    }
    public struct Place {
        public int iChar;
        public int iLine;

        public Place( int iChar, int iLine ) {
            this.iChar = iChar;
            this.iLine = iLine;
        }

        public void Offset( int dx, int dy ) {
            iChar += dx;
            iLine += dy;
        }

        public static bool operator !=( Place p1, Place p2 ) {
            return !p1.Equals( p2 );
        }

        public static bool operator ==( Place p1, Place p2 ) {
            return p1.Equals( p2 );
        }

        public static bool operator <( Place p1, Place p2 ) {
            if ( p1.iLine < p2.iLine )
                return true;
            if ( p1.iLine > p2.iLine )
                return false;
            if ( p1.iChar < p2.iChar )
                return true;
            return false;
        }

        public static bool operator <=( Place p1, Place p2 ) {
            if ( p1.Equals( p2 ) )
                return true;
            if ( p1.iLine < p2.iLine )
                return true;
            if ( p1.iLine > p2.iLine )
                return false;
            if ( p1.iChar < p2.iChar )
                return true;
            return false;
        }

        public static bool operator >( Place p1, Place p2 ) {
            if ( p1.iLine > p2.iLine )
                return true;
            if ( p1.iLine < p2.iLine )
                return false;
            if ( p1.iChar > p2.iChar )
                return true;
            return false;
        }

        public static bool operator >=( Place p1, Place p2 ) {
            if ( p1.Equals( p2 ) )
                return true;
            if ( p1.iLine > p2.iLine )
                return true;
            if ( p1.iLine < p2.iLine )
                return false;
            if ( p1.iChar > p2.iChar )
                return true;
            return false;
        }

        public static Place Empty {
            get {
                return new Place( );
            }
        }

        public override string ToString( ) {
            return "(" + iChar + "," + iLine + ")";
        }

        public override bool Equals( object obj ) {
            return base.Equals( obj );
        }

        public override int GetHashCode( ) {
            return base.GetHashCode( );
        }
    }
    public class Line : List<Char> {
        List<int> cutOffPositions;

        public string FoldingStartMarker {
            get;
            set;
        }
        public string FoldingEndMarker {
            get;
            set;
        }
        /// <summary>
        /// Text of line was changed
        /// </summary>
        public bool IsChanged {
            get;
            set;
        }
        /// <summary>
        /// Time of last visit of caret in this line
        /// </summary>
        /// <remarks>This property can be used for forward/backward navigating</remarks>
        public DateTime LastVisit {
            get;
            set;
        }
        //Y coordinate of line on screen
        public int startY = -1;
        /// <summary>
        /// Visible state
        /// </summary>
        public VisibleState VisibleState {
            get {
                return VisibleState2;
            }
            set {
                VisibleState2 = value;
            }
        }
        /// <summary>
        /// Visible state
        /// </summary>
        public VisibleState VisibleState2 {
            get;
            set;
        }
        /// <summary>
        /// AutoIndent level for this line
        /// </summary>
        public int IndentLevel {
            get;
            set;
        }
        /// <summary>
        /// Background brush.
        /// </summary>
        public Brush BackgroundBrush {
            get;
            set;
        }
        /// <summary>
        /// User tag
        /// </summary>
        public object Tag {
            get;
            set;
        }
        /// <summary>
        /// Unique ID
        /// </summary>
        public int UniqueId {
            get;
            set;
        }
        /// <summary>
        /// Count of needed start spaces for AutoIndent
        /// </summary>
        public int AutoIndentSpacesNeededCount {
            get;
            set;
        }

        public Line( int uid ) {
            this.UniqueId = uid;
        }

        /// <summary>
        /// Clears style of chars, delete folding markers
        /// </summary>
        public void ClearStyle( StyleIndex styleIndex ) {
            VisibleState = VisibleState.Visible;
            FoldingStartMarker = null;
            FoldingEndMarker = null;
            for ( int i = 0 ; i < Count ; i++ ) {
                Char c = this[ i ];
                c.style &= ~styleIndex;
                this[ i ] = c;
            }
        }

        /// <summary>
        /// Text of the line
        /// </summary>
        public string Text {
            get {
                StringBuilder sb = new StringBuilder( Count );
                foreach ( Char c in this )
                    sb.Append( c.c );
                return sb.ToString( );
            }
            set {

            }
        }

        /// <summary>
        /// Clears folding markers
        /// </summary>
        public void ClearFoldingMarkers( ) {
            FoldingStartMarker = null;
            FoldingEndMarker = null;
        }

        /// <summary>
        /// Positions for wordwrap cutoffs
        /// </summary>
        public List<int> CutOffPositions {
            get {
                if ( cutOffPositions == null )
                    cutOffPositions = new List<int>( );
                return cutOffPositions;
            }
        }

        /// <summary>
        /// Count of wordwrap string count for this line
        /// </summary>
        public int WordWrapStringsCount {
            get {
                if ( VisibleState == VisibleState.Hidden )
                    return 0;
                if ( VisibleState == VisibleState.StartOfHiddenBlock )
                    return 1;
                if ( cutOffPositions == null )
                    return 1;
                return cutOffPositions.Count + 1;
            }
        }

        public int GetWordWrapStringStartPosition( int iWordWrapLine ) {
            return iWordWrapLine == 0 ? 0 : CutOffPositions[ iWordWrapLine - 1 ];
        }

        public int GetWordWrapStringFinishPosition( int iWordWrapLine ) {
            if ( WordWrapStringsCount <= 0 )
                return 0;
            return iWordWrapLine == WordWrapStringsCount - 1 ? Count - 1 : CutOffPositions[ iWordWrapLine ] - 1;
        }

        /// <summary>
        /// Gets index of wordwrap string for given char position
        /// </summary>
        public int GetWordWrapStringIndex( int iChar ) {
            if ( cutOffPositions == null || cutOffPositions.Count == 0 )
                return 0;
            for ( int i = 0 ; i < cutOffPositions.Count ; i++ )
                if ( cutOffPositions[ i ] >/*>=*/ iChar )
                    return i;
            return cutOffPositions.Count;
        }

        /// <summary>
        /// Calculates wordwrap cutoffs
        /// </summary>
        public void CalcCutOffs( int maxCharsPerLine, bool allowIME, bool charWrap ) {
            int segmentLength = 0;
            int cutOff = 0;
            CutOffPositions.Clear( );

            for ( int i = 0 ; i < Count ; i++ ) {
                char c = this[ i ].c;
                if ( charWrap ) {
                    //char wrapping
                    cutOff = Math.Min( i + 1, Count - 1 );
                } else {
                    //word wrapping
                    if ( allowIME && isCJKLetter( c ) )//in CJK languages cutoff can be in any letter
					{
                        cutOff = i;
                    } else
                        if ( !char.IsLetterOrDigit( c ) && c != '_' )
                            cutOff = Math.Min( i + 1, Count - 1 );
                }

                segmentLength++;

                if ( segmentLength == maxCharsPerLine ) {
                    if ( cutOff == 0 || ( cutOffPositions.Count > 0 && cutOff == cutOffPositions[ cutOffPositions.Count - 1 ] ) )
                        cutOff = i + 1;
                    CutOffPositions.Add( cutOff );
                    segmentLength = 1 + i - cutOff;
                }
            }
        }

        public bool isCJKLetter( char c ) {
            int code = Convert.ToInt32( c );
            return
            ( code >= 0x3300 && code <= 0x33FF ) ||
            ( code >= 0xFE30 && code <= 0xFE4F ) ||
            ( code >= 0xF900 && code <= 0xFAFF ) ||
            ( code >= 0x2E80 && code <= 0x2EFF ) ||
            ( code >= 0x31C0 && code <= 0x31EF ) ||
            ( code >= 0x4E00 && code <= 0x9FFF ) ||
            ( code >= 0x3400 && code <= 0x4DBF ) ||
            ( code >= 0x3200 && code <= 0x32FF ) ||
            ( code >= 0x2460 && code <= 0x24FF ) ||
            ( code >= 0x3040 && code <= 0x309F ) ||
            ( code >= 0x2F00 && code <= 0x2FDF ) ||
            ( code >= 0x31A0 && code <= 0x31BF ) ||
            ( code >= 0x4DC0 && code <= 0x4DFF ) ||
            ( code >= 0x3100 && code <= 0x312F ) ||
            ( code >= 0x30A0 && code <= 0x30FF ) ||
            ( code >= 0x31F0 && code <= 0x31FF ) ||
            ( code >= 0x2FF0 && code <= 0x2FFF ) ||
            ( code >= 0x1100 && code <= 0x11FF ) ||
            ( code >= 0xA960 && code <= 0xA97F ) ||
            ( code >= 0xD7B0 && code <= 0xD7FF ) ||
            ( code >= 0x3130 && code <= 0x318F ) ||
            ( code >= 0xAC00 && code <= 0xD7AF );

        }

        /// <summary>
        /// Count of start spaces
        /// </summary>
        public int StartSpacesCount {
            get {
                int spacesCount = 0;
                for ( int i = 0 ; i < Count ; i++ )
                    if ( this[ i ].c == ' ' )
                        spacesCount++;
                    else
                        break;
                return spacesCount;
            }
        }
    }
    public enum VisibleState {
        Visible,
        StartOfHiddenBlock,
        Hidden
    }
    public enum IndentMarker {
        None,
        Increased,
        Decreased
    }
    partial class FindForm : Form {
        public static string RegexSpecSymbolsPattern = @"[\^\$\[\]\(\)\.\\\*\+\|\?\{\}]";
        bool firstSearch = true;
        Place startPlace;
        FastColoredTextBox tb;

        public FindForm( FastColoredTextBox tb ) {
            InitializeComponent( );
            this.tb = tb;
        }

        public void btClose_Click( object sender, EventArgs e ) {
            Close( );
        }

        public void btFindNext_Click( object sender, EventArgs e ) {
            FindNext( );
        }

        public void FindNext( ) {
            try {
                string pattern = tbFind.Text;
                RegexOptions opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
                if ( !cbRegex.Checked )
                    pattern = Regex.Replace( pattern, RegexSpecSymbolsPattern, "\\$0" );
                if ( cbWholeWord.Checked )
                    pattern = "\\b" + pattern + "\\b";
                //
                Range range = tb.Selection.Clone( );
                range.Normalize( );
                //
                if ( firstSearch ) {
                    startPlace = range.Start;
                    firstSearch = false;
                }
                //
                range.Start = range.End;
                if ( range.Start >= startPlace )
                    range.End = new Place( tb.GetLineLength( tb.LinesCount - 1 ), tb.LinesCount - 1 );
                else
                    range.End = startPlace;
                //
                foreach ( var r in range.GetRanges( pattern, opt ) ) {
                    tb.Selection = r;
                    tb.DoSelectionVisible( );
                    tb.Invalidate( );
                    return;
                }
                //
                if ( range.Start >= startPlace && startPlace > Place.Empty ) {
                    tb.Selection.Start = new Place( 0, 0 );
                    FindNext( );
                    return;
                }
                MessageBox.Show( "Not found" );
            } catch ( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }

        public void tbFind_KeyPress( object sender, KeyPressEventArgs e ) {
            if ( e.KeyChar == '\r' ) {
                btFindNext.PerformClick( );
                e.Handled = true;
                return;
            }
            if ( e.KeyChar == '\x1b' ) {
                Hide( );
                e.Handled = true;
                return;
            }
        }

        public void FindForm_FormClosing( object sender, FormClosingEventArgs e ) {
            if ( e.CloseReason == CloseReason.UserClosing ) {
                e.Cancel = true;
                Hide( );
            }
        }

        protected override void OnActivated( EventArgs e ) {
            tbFind.Focus( );
            ResetSerach( );
        }

        void ResetSerach( ) {
            firstSearch = true;
        }

        public void cbMatchCase_CheckedChanged( object sender, EventArgs e ) {
            ResetSerach( );
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        public System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing ) {
            if ( disposing && ( components != null ) ) {
                components.Dispose( );
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        public void InitializeComponent( ) {
            this.btClose = new CheckBox( );
            this.btFindNext = new Button( );
            this.tbFind = new TextBox( );
            this.cbRegex = new CheckBox( );
            this.cbMatchCase = new CheckBox( );
            this.label1 = new Label( );
            this.cbWholeWord = new CheckBox( );
            this.SuspendLayout( );
            // 
            // btClose
            // 
            this.btClose.Location = new System.Drawing.Point( 273, 72 );
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size( 75, 23 );
            this.btClose.TabIndex = 5;
            this.btClose.Text = "Close";
            this.btClose.Click += new System.EventHandler( this.btClose_Click );
            // 
            // btFindNext
            // 
            this.btFindNext.Location = new System.Drawing.Point( 192, 72 );
            this.btFindNext.Name = "btFindNext";
            this.btFindNext.Size = new System.Drawing.Size( 75, 23 );
            this.btFindNext.TabIndex = 4;
            this.btFindNext.Text = "Find next";
            this.btFindNext.Click += new System.EventHandler( this.btFindNext_Click );
            // 
            // tbFind
            // 
            this.tbFind.Font = new System.Drawing.Font( "Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte )( 204 ) ) );
            this.tbFind.Location = new System.Drawing.Point( 42, 12 );
            this.tbFind.Name = "tbFind";
            this.tbFind.Size = new System.Drawing.Size( 306, 20 );
            this.tbFind.TabIndex = 0;
            this.tbFind.TextChanged += new System.EventHandler( this.cbMatchCase_CheckedChanged );
            this.tbFind.KeyPress += new System.Windows.Forms.KeyPressEventHandler( this.tbFind_KeyPress );
            // 
            // cbRegex
            // 
            this.cbRegex.AutoSize = true;
            this.cbRegex.Location = new System.Drawing.Point( 249, 38 );
            this.cbRegex.Name = "cbRegex";
            this.cbRegex.Size = new System.Drawing.Size( 57, 17 );
            this.cbRegex.TabIndex = 3;
            this.cbRegex.Text = "Regex";
            this.cbRegex.CheckedChanged += new System.EventHandler( this.cbMatchCase_CheckedChanged );
            // 
            // cbMatchCase
            // 
            this.cbMatchCase.AutoSize = true;
            this.cbMatchCase.Location = new System.Drawing.Point( 42, 38 );
            this.cbMatchCase.Name = "cbMatchCase";
            this.cbMatchCase.Size = new System.Drawing.Size( 82, 17 );
            this.cbMatchCase.TabIndex = 1;
            this.cbMatchCase.Text = "Match case";
            this.cbMatchCase.CheckedChanged += new System.EventHandler( this.cbMatchCase_CheckedChanged );
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point( 6, 15 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 33, 13 );
            this.label1.TabIndex = 5;
            this.label1.Text = "Find: ";
            // 
            // cbWholeWord
            // 
            this.cbWholeWord.AutoSize = true;
            this.cbWholeWord.Location = new System.Drawing.Point( 130, 38 );
            this.cbWholeWord.Name = "cbWholeWord";
            this.cbWholeWord.Size = new System.Drawing.Size( 113, 17 );
            this.cbWholeWord.TabIndex = 2;
            this.cbWholeWord.Text = "Match whole word";
            this.cbWholeWord.CheckedChanged += new System.EventHandler( this.cbMatchCase_CheckedChanged );
            // 
            // FindForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 360, 103 );
            this.Controls.Add( this.cbWholeWord );
            this.Controls.Add( this.label1 );
            this.Controls.Add( this.cbMatchCase );
            this.Controls.Add( this.cbRegex );
            this.Controls.Add( this.tbFind );
            this.Controls.Add( this.btFindNext );
            this.Controls.Add( this.btClose );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FindForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Find";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.FindForm_FormClosing );
            this.ResumeLayout( false );
            this.PerformLayout( );

        }

        #endregion

        public CheckBox btClose;
        public Button btFindNext;
        public CheckBox cbRegex;
        public CheckBox cbMatchCase;
        public Label label1;
        public CheckBox cbWholeWord;
        public TextBox tbFind;
    }

    public class PaintLineEventArgs : PaintEventArgs {
        public int LineIndex {
            get;
            set;
        }
        public Rectangle LineRect {
            get;
            set;
        }

        public PaintLineEventArgs( int iLine, Rectangle rect, Graphics gr, Rectangle clipRect )
            : base( gr, clipRect ) {
            LineIndex = iLine;
            LineRect = rect;
        }
    }
    public class LineInsertedEventArgs : EventArgs {
        /// <summary>
        /// Inserted line index
        /// </summary>
        public int Index {
            get;
            set;
        }
        /// <summary>
        /// Count of inserted lines
        /// </summary>
        public int Count {
            get;
            set;
        }

        public LineInsertedEventArgs( int index, int count ) {
            this.Index = index;
            this.Count = count;
        }
    }
    public class LineRemovedEventArgs : EventArgs {
        /// <summary>
        /// Removed line index
        /// </summary>
        public int Index {
            get;
            set;
        }
        /// <summary>
        /// Count of removed lines
        /// </summary>
        public int Count {
            get;
            set;
        }
        /// <summary>
        /// UniqueIds of removed lines
        /// </summary>
        public List<int> RemovedLineUniqueIds {
            get;
            set;
        }

        public LineRemovedEventArgs( int index, int count, List<int> removedLineIds ) {
            this.Index = index;
            this.Count = count;
            this.RemovedLineUniqueIds = removedLineIds;
        }
    }
    public class TextChangedEventArgs : EventArgs {
        /// <summary>
        /// This range contains changed area of text
        /// </summary>
        public Range ChangedRange {
            get;
            set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TextChangedEventArgs( Range changedRange ) {
            this.ChangedRange = changedRange;
        }
    }
    public class TextChangingEventArgs : EventArgs {
        public string InsertingText {
            get;
            set;
        }
    }
    public class TabChangingEventArgs : EventArgs {
        public TabChangingReason Reason {
            get;
            set;
        }
        public bool Cancel {
            get;
            set;
        }

        public TabChangingEventArgs( TabChangingReason reason ) {
            this.Reason = reason;
        }
    }
    public enum TabChangingReason {
        Programm,
        User
    }
    public enum WordWrapMode {
        /// <summary>
        /// Word wrapping by control width
        /// </summary>
        WordWrapControlWidth,
        /// <summary>
        /// Word wrapping by preferred line width (PreferredLineWidth)
        /// </summary>
        WordWrapPreferredWidth,
        /// <summary>
        /// Char wrapping by control width
        /// </summary>
        CharWrapControlWidth,
        /// <summary>
        /// Char wrapping by preferred line width (PreferredLineWidth)
        /// </summary>
        CharWrapPreferredWidth
    }
    public class PrintDialogSettings {
        public bool ShowPageSetupDialog = false;
        public bool ShowPrintDialog = false;
        public bool ShowPrintPreviewDialog = true;
        public Range printRange;
    }
    public class AutoIndentEventArgs : EventArgs {
        public int iLine {
            get;
            set;
        }
        public int TabLength {
            get;
            set;
        }
        public string LineText {
            get;
            set;
        }
        public string PrevLineText {
            get;
            set;
        }
        /// <summary>
        /// Additional spaces count for this line, relative to previous line
        /// </summary>
        public int Shift {
            get;
            set;
        }
        /// <summary>
        /// Additional spaces count for next line, relative to previous line
        /// </summary>
        public int ShiftNextLines {
            get;
            set;
        }

        public AutoIndentEventArgs( int iLine, string lineText, string prevLineText, int tabLength ) {
            this.iLine = iLine;
            this.LineText = lineText;
            this.PrevLineText = prevLineText;
            this.TabLength = tabLength;
        }
    }
    public class ExportToHTML {
        /// <summary>
        /// Use nbsp; instead space
        /// </summary>
        public bool UseNbsp {
            get;
            set;
        }
        /// <summary>
        /// Use nbsp; instead space in beginning of line
        /// </summary>
        public bool UseForwardNbsp {
            get;
            set;
        }
        /// <summary>
        /// Use original font
        /// </summary>
        public bool UseOriginalFont {
            get;
            set;
        }
        /// <summary>
        /// Use style tag instead style attribute
        /// </summary>
        public bool UseStyleTag {
            get;
            set;
        }
        /// <summary>
        /// Use br tag instead \n
        /// </summary>
        public bool UseBr {
            get;
            set;
        }

        FastColoredTextBox tb;

        public ExportToHTML( ) {
            UseNbsp = true;
            UseOriginalFont = true;
            UseStyleTag = true;
            UseBr = true;
        }

        public string GetHtml( FastColoredTextBox tb ) {
            this.tb = tb;
            Range sel = new Range( tb );
            sel.SelectAll( );
            return GetHtml( sel );
        }

        public string GetHtml( Range r ) {
            this.tb = r.tb;
            Dictionary<StyleIndex, object> styles = new Dictionary<StyleIndex, object>( );
            StringBuilder sb = new StringBuilder( );
            StringBuilder tempSB = new StringBuilder( );
            StyleIndex currentStyleId = StyleIndex.None;
            r.Normalize( );
            int currentLine = r.Start.iLine;
            styles[ currentStyleId ] = null;
            //
            if ( UseOriginalFont )
                sb.AppendFormat( "<font style=\"font-family: {0}, monospace; font-size: {1}px; line-height: {2}px;\">", r.tb.Font.Name, r.tb.CharHeight - r.tb.LineInterval, r.tb.CharHeight );
            //
            bool hasNonSpace = false;
            foreach ( Place p in r ) {
                Char c = r.tb[ p.iLine ][ p.iChar ];
                if ( c.style != currentStyleId ) {
                    Flush( sb, tempSB, currentStyleId );
                    currentStyleId = c.style;
                    styles[ currentStyleId ] = null;
                }

                if ( p.iLine != currentLine ) {
                    for ( int i = currentLine ; i < p.iLine ; i++ )
                        tempSB.AppendLine( UseBr ? "<br>" : "" );
                    currentLine = p.iLine;
                    hasNonSpace = false;
                }
                switch ( c.c ) {
                    case ' ':
                        if ( ( hasNonSpace || !UseForwardNbsp ) && !UseNbsp )
                            goto default;

                        tempSB.Append( "&nbsp;" );
                        break;
                    case '<':
                        tempSB.Append( "&lt;" );
                        break;
                    case '>':
                        tempSB.Append( "&gt;" );
                        break;
                    case '&':
                        tempSB.Append( "&amp;" );
                        break;
                    default:
                        hasNonSpace = true;
                        tempSB.Append( c.c );
                        break;
                }
            }
            Flush( sb, tempSB, currentStyleId );

            if ( UseOriginalFont )
                sb.AppendLine( "</font>" );

            //build styles
            if ( UseStyleTag ) {
                tempSB.Length = 0;
                tempSB.AppendLine( "<style type=\"text/css\">" );
                foreach ( var styleId in styles.Keys )
                    tempSB.AppendFormat( ".fctb{0}{{ {1} }}\r\n", GetStyleName( styleId ), GetCss( styleId ) );
                tempSB.AppendLine( "</style>" );

                sb.Insert( 0, tempSB.ToString( ) );
            }

            return sb.ToString( );
        }

        public string GetCss( StyleIndex styleIndex ) {
            //find text renderer
            TextStyle textStyle = null;
            int mask = 1;
            bool hasTextStyle = false;
            for ( int i = 0 ; i < tb.Styles.Length ; i++ ) {
                if ( tb.Styles[ i ] != null && ( ( int )styleIndex & mask ) != 0 ) {
                    Style style = tb.Styles[ i ];
                    bool isTextStyle = style is TextStyle;
                    if ( isTextStyle )
                        if ( !hasTextStyle || tb.AllowSeveralTextStyleDrawing ) {
                            hasTextStyle = true;
                            textStyle = style as TextStyle;
                        }
                }
                mask = mask << 1;
            }
            //draw by default renderer
            if ( !hasTextStyle )
                textStyle = tb.DefaultStyle;
            //
            string result = "";
            string s = "";
            if ( textStyle.BackgroundBrush is SolidBrush ) {
                s = GetColorAsString( ( textStyle.BackgroundBrush as SolidBrush ).Color );
                if ( s != "" )
                    result += "background-color:" + s + ";";
            }
            if ( textStyle.ForeBrush is SolidBrush ) {
                s = GetColorAsString( ( textStyle.ForeBrush as SolidBrush ).Color );
                if ( s != "" )
                    result += "color:" + s + ";";
            }
            if ( ( textStyle.FontStyle & FontStyle.Bold ) != 0 )
                result += "font-weight:bold;";
            if ( ( textStyle.FontStyle & FontStyle.Italic ) != 0 )
                result += "font-style:oblique;";
            if ( ( textStyle.FontStyle & FontStyle.Strikeout ) != 0 )
                result += "text-decoration:line-through;";
            if ( ( textStyle.FontStyle & FontStyle.Underline ) != 0 )
                result += "text-decoration:underline;";

            return result;
        }

        public static string GetColorAsString( Color color ) {
            if ( color == Color.Transparent )
                return "";
            return string.Format( "#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B );
        }

        string GetStyleName( StyleIndex styleIndex ) {
            return styleIndex.ToString( ).Replace( " ", "" ).Replace( ",", "" );
        }

        public void Flush( StringBuilder sb, StringBuilder tempSB, StyleIndex currentStyle ) {
            //find textRenderer
            //var textStyle = styles.Where(s => s is TextStyle).FirstOrDefault();
            //
            if ( tempSB.Length == 0 )
                return;
            if ( UseStyleTag )
                sb.AppendFormat( "<font class=fctb{0}>{1}</font>", GetStyleName( currentStyle ), tempSB.ToString( ) );
            else {
                string css = GetCss( currentStyle );
                if ( css != "" )
                    sb.AppendFormat( "<font style=\"{0}\">", css );
                sb.Append( tempSB.ToString( ) );
                if ( css != "" )
                    sb.Append( "</font>" );
            }
            tempSB.Length = 0;
        }
    }
    public class InsertCharCommand : UndoableCommand {
        char c;
        char deletedChar = '\x0';

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tb">Underlaying textbox</param>
        /// <param name="c">Inserting char</param>
        public InsertCharCommand( FastColoredTextBox tb, char c )
            : base( tb ) {
            this.c = c;
        }

        /// <summary>
        /// Undo operation
        /// </summary>
        public override void Undo( ) {
            tb.OnTextChanging( );
            switch ( c ) {
                case '\n':
                    MergeLines( sel.Start.iLine, tb );
                    break;
                case '\r':
                    break;
                case '\b':
                    tb.Selection.Start = lastSel.Start;
                    char cc = '\x0';
                    if ( deletedChar != '\x0' ) {
                        tb.ExpandBlock( tb.Selection.Start.iLine );
                        InsertChar( deletedChar, ref cc, tb );
                    }
                    break;
                default:
                    tb.ExpandBlock( sel.Start.iLine );
                    tb[ sel.Start.iLine ].RemoveAt( sel.Start.iChar );
                    tb.Selection.Start = sel.Start;
                    break;
            }

            tb.needRecalc = true;

            base.Undo( );
        }

        /// <summary>
        /// Execute operation
        /// </summary>
        public override void Execute( ) {
            tb.ExpandBlock( tb.Selection.Start.iLine );
            string s = c.ToString( );
            tb.OnTextChanging( ref s );
            if ( s.Length == 1 )
                c = s[ 0 ];

            if ( tb.LinesCount == 0 )
                InsertLine( tb );
            InsertChar( c, ref deletedChar, tb );
            tb.needRecalc = true;
            base.Execute( );
        }

        public static void InsertChar( char c, ref char deletedChar, FastColoredTextBox tb ) {
            switch ( c ) {
                case '\n':
                    if ( tb.LinesCount == 0 )
                        InsertLine( tb );
                    InsertLine( tb );
                    break;
                case '\r':
                    break;
                case '\b'://backspace
                    if ( tb.Selection.Start.iChar == 0 && tb.Selection.Start.iLine == 0 )
                        return;
                    if ( tb.Selection.Start.iChar == 0 ) {
                        if ( tb[ tb.Selection.Start.iLine - 1 ].VisibleState != VisibleState.Visible )
                            tb.ExpandBlock( tb.Selection.Start.iLine - 1 );
                        deletedChar = '\n';
                        MergeLines( tb.Selection.Start.iLine - 1, tb );
                    } else {
                        deletedChar = tb[ tb.Selection.Start.iLine ][ tb.Selection.Start.iChar - 1 ].c;
                        tb[ tb.Selection.Start.iLine ].RemoveAt( tb.Selection.Start.iChar - 1 );
                        tb.Selection.Start = new Place( tb.Selection.Start.iChar - 1, tb.Selection.Start.iLine );
                    }
                    break;
                default:
                    tb[ tb.Selection.Start.iLine ].Insert( tb.Selection.Start.iChar, new Char( c ) );
                    tb.Selection.Start = new Place( tb.Selection.Start.iChar + 1, tb.Selection.Start.iLine );
                    break;
            }
        }

        public static void InsertLine( FastColoredTextBox tb ) {
            if ( tb.LinesCount == 0 )
                tb.InsertLine( tb.Selection.Start.iLine + 1, new Line( tb.GenerateUniqueLineId( ) ) );
            else
                BreakLines( tb.Selection.Start.iLine, tb.Selection.Start.iChar, tb );

            tb.Selection.Start = new Place( 0, tb.Selection.Start.iLine + 1 );
            tb.needRecalc = true;
        }

        /// <summary>
        /// Merge lines i and i+1
        /// </summary>
        public static void MergeLines( int i, FastColoredTextBox tb ) {
            if ( i + 1 >= tb.LinesCount )
                return;
            tb.ExpandBlock( i );
            tb.ExpandBlock( i + 1 );
            int pos = tb[ i ].Count;
            //
            if ( tb[ i ].Count == 0 )
                tb.RemoveLine( i );
            else
                if ( tb[ i + 1 ].Count == 0 )
                    tb.RemoveLine( i + 1 );
                else {
                    tb[ i ].AddRange( tb[ i + 1 ] );
                    tb.RemoveLine( i + 1 );
                }
            tb.Selection.Start = new Place( pos, i );
            tb.needRecalc = true;
        }

        public static void BreakLines( int iLine, int pos, FastColoredTextBox tb ) {
            Line newLine = new Line( tb.GenerateUniqueLineId( ) );
            for ( int i = pos ; i < tb[ iLine ].Count ; i++ )
                newLine.Add( tb[ iLine ][ i ] );
            tb[ iLine ].RemoveRange( pos, tb[ iLine ].Count - pos );
            tb.InsertLine( iLine + 1, newLine );
        }
    }
    public class InsertTextCommand : UndoableCommand {
        string insertedText;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tb">Underlaying textbox</param>
        /// <param name="insertedText">Text for inserting</param>
        public InsertTextCommand( FastColoredTextBox tb, string insertedText )
            : base( tb ) {
            this.insertedText = insertedText;
        }

        /// <summary>
        /// Undo operation
        /// </summary>
        public override void Undo( ) {
            tb.Selection.Start = sel.Start;
            tb.Selection.End = lastSel.Start;
            tb.OnTextChanging( );
            ClearSelectedCommand.ClearSelected( tb );
            base.Undo( );
        }

        /// <summary>
        /// Execute operation
        /// </summary>
        public override void Execute( ) {
            tb.OnTextChanging( ref insertedText );
            InsertText( insertedText, tb );
            base.Execute( );
        }

        public static void InsertText( string insertedText, FastColoredTextBox tb ) {
            try {
                tb.Selection.BeginUpdate( );
                char cc = '\x0';
                if ( tb.LinesCount == 0 )
                    InsertCharCommand.InsertLine( tb );
                tb.ExpandBlock( tb.Selection.Start.iLine );
                foreach ( char c in insertedText )
                    InsertCharCommand.InsertChar( c, ref cc, tb );
                tb.needRecalc = true;
            } finally {
                tb.Selection.EndUpdate( );
            }
        }
    }
    public class ReplaceTextCommand : UndoableCommand {
        string insertedText;
        List<Range> ranges;
        string prevText;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tb">Underlaying textbox</param>
        /// <param name="ranges">List of ranges for replace</param>
        /// <param name="insertedText">Text for inserting</param>
        public ReplaceTextCommand( FastColoredTextBox tb, List<Range> ranges, string insertedText )
            : base( tb ) {
            this.ranges = ranges;
            this.insertedText = insertedText;
            sel = tb.Selection.Clone( );
            sel.SelectAll( );
        }

        /// <summary>
        /// Undo operation
        /// </summary>
        public override void Undo( ) {
            tb.Text = prevText;
        }

        /// <summary>
        /// Execute operation
        /// </summary>
        public override void Execute( ) {
            tb.OnTextChanging( ref insertedText );

            this.prevText = tb.Text;

            tb.Selection.BeginUpdate( );
            for ( int i = ranges.Count - 1 ; i >= 0 ; i-- ) {
                tb.Selection.Start = ranges[ i ].Start;
                tb.Selection.End = ranges[ i ].End;
                ClearSelectedCommand.ClearSelected( tb );
                InsertTextCommand.InsertText( insertedText, tb );
            }
            tb.Selection.SelectAll( );
            tb.Selection.EndUpdate( );
            tb.needRecalc = true;

            lastSel = tb.Selection.Clone( );
            tb.OnTextChanged( lastSel.Start.iLine, lastSel.End.iLine );
            //base.Execute();
        }
    }
    public class ClearSelectedCommand : UndoableCommand {
        string deletedText;

        /// <summary>
        /// Construstor
        /// </summary>
        /// <param name="tb">Underlaying textbox</param>
        public ClearSelectedCommand( FastColoredTextBox tb )
            : base( tb ) {
        }

        /// <summary>
        /// Undo operation
        /// </summary>
        public override void Undo( ) {
            tb.Selection.Start = new Place( sel.FromX, Math.Min( sel.Start.iLine, sel.End.iLine ) );
            tb.OnTextChanging( );
            InsertTextCommand.InsertText( deletedText, tb );
            tb.OnTextChanged( sel.Start.iLine, sel.End.iLine );
        }

        /// <summary>
        /// Execute operation
        /// </summary>
        public override void Execute( ) {
            tb.OnTextChanging( );
            deletedText = tb.Selection.Text;
            ClearSelected( tb );
            lastSel = tb.Selection.Clone( );
            tb.OnTextChanged( lastSel.Start.iLine, lastSel.Start.iLine );
        }

        public static void ClearSelected( FastColoredTextBox tb ) {
            Place start = tb.Selection.Start;
            Place end = tb.Selection.End;
            int fromLine = Math.Min( end.iLine, start.iLine );
            int toLine = Math.Max( end.iLine, start.iLine );
            int fromChar = tb.Selection.FromX;
            int toChar = tb.Selection.ToX;
            if ( fromLine < 0 )
                return;
            //
            if ( fromLine == toLine )
                tb[ fromLine ].RemoveRange( fromChar, toChar - fromChar );
            else {
                tb[ fromLine ].RemoveRange( fromChar, tb[ fromLine ].Count - fromChar );
                tb[ toLine ].RemoveRange( 0, toChar );
                tb.RemoveLine( fromLine + 1, toLine - fromLine - 1 );
                InsertCharCommand.MergeLines( fromLine, tb );
            }
            //
            tb.Selection.Start = new Place( fromChar, fromLine );
            //
            tb.needRecalc = true;
        }
    }
    public class CommandManager {
        readonly int maxHistoryLength = 200;
        LimitedStack<UndoableCommand> history;
        Stack<UndoableCommand> redoStack = new Stack<UndoableCommand>( );

        public CommandManager( ) {
            history = new LimitedStack<UndoableCommand>( maxHistoryLength );
        }

        public void ExecuteCommand( Command cmd ) {
            if ( disabledCommands > 0 )
                return;

            if ( cmd is UndoableCommand ) {
                ( cmd as UndoableCommand ).autoUndo = autoUndoCommands > 0;
                history.Push( cmd as UndoableCommand );
            }
            cmd.Execute( );
            //
            redoStack.Clear( );
        }

        public void Undo( ) {
            if ( history.Count > 0 ) {
                var cmd = history.Pop( );
                //
                BeginDisableCommands( );//prevent text changing into handlers
                try {
                    cmd.Undo( );
                } finally {
                    EndDisableCommands( );
                }
                //
                redoStack.Push( cmd );
            }

            //undo next autoUndo command
            if ( history.Count > 0 ) {
                if ( history.Peek( ).autoUndo )
                    Undo( );
            }
        }

        int disabledCommands = 0;

        public void EndDisableCommands( ) {
            disabledCommands--;
        }

        public void BeginDisableCommands( ) {
            disabledCommands++;
        }

        int autoUndoCommands = 0;

        public void EndAutoUndoCommands( ) {
            autoUndoCommands--;
            if ( autoUndoCommands == 0 )
                if ( history.Count > 0 )
                    history.Peek( ).autoUndo = false;
        }

        public void BeginAutoUndoCommands( ) {
            autoUndoCommands++;
        }

        public void ClearHistory( ) {
            history.Clear( );
            redoStack.Clear( );
        }

        public void Redo( ) {
            if ( redoStack.Count == 0 )
                return;
            UndoableCommand cmd;
            BeginDisableCommands( );//prevent text changing into handlers
            try {
                cmd = redoStack.Pop( );
                cmd.tb.Selection.Start = cmd.sel.Start;
                cmd.tb.Selection.End = cmd.sel.End;
                cmd.Execute( );
                history.Push( cmd );
            } finally {
                EndDisableCommands( );
            }

            //redo command after autoUndoable command
            if ( cmd.autoUndo )
                Redo( );
        }

        public bool UndoEnabled {
            get {
                return history.Count > 0;
            }
        }

        public bool RedoEnabled {
            get {
                return redoStack.Count > 0;
            }
        }
    }
    public abstract class Command {
        public FastColoredTextBox tb;
        public abstract void Execute( );
    }
    public abstract class UndoableCommand : Command {
        public Range sel;
        public Range lastSel;
        public bool autoUndo;

        public UndoableCommand( FastColoredTextBox tb ) {
            this.tb = tb;
            sel = tb.Selection.Clone( );
        }

        public virtual void Undo( ) {
            OnTextChanged( true );
        }

        public override void Execute( ) {
            lastSel = tb.Selection.Clone( );
            OnTextChanged( false );
        }

        protected virtual void OnTextChanged( bool invert ) {
            bool b = sel.Start.iLine < lastSel.Start.iLine;
            if ( invert ) {
                if ( b )
                    tb.OnTextChanged( sel.Start.iLine, sel.Start.iLine );
                else
                    tb.OnTextChanged( sel.Start.iLine, lastSel.Start.iLine );
            } else {
                if ( b )
                    tb.OnTextChanged( sel.Start.iLine, lastSel.Start.iLine );
                else
                    tb.OnTextChanged( lastSel.Start.iLine, lastSel.Start.iLine );
            }
        }
    }
    public struct Char {
        /// <summary>
        /// Unicode character
        /// </summary>
        public char c;
        /// <summary>
        /// Style bit mask
        /// </summary>
        /// <remarks>Bit 1 in position n means that this char will rendering by FastColoredTextBox.Styles[n]</remarks>
        public StyleIndex style;

        public Char( char c ) {
            this.c = c;
            style = StyleIndex.None;
        }
    }
    public enum StyleIndex : ushort {
        None = 0,
        Style0 = 0x1,
        Style1 = 0x2,
        Style2 = 0x4,
        Style3 = 0x8,
        Style4 = 0x10,
        Style5 = 0x20,
        Style6 = 0x40,
        Style7 = 0x80,
        Style8 = 0x100,
        Style9 = 0x200,
        Style10 = 0x400,
        Style11 = 0x800,
        Style12 = 0x1000,
        Style13 = 0x2000,
        Style14 = 0x4000,
        Style15 = 0x8000,
        All = 0xffff
    }
    public class StyleIndexHelper {
        public static int Index( StyleIndex si ) {
            switch ( si ) {
                case StyleIndex.Style0:
                    return 0;

                case StyleIndex.Style1:
                    return 1;

                case StyleIndex.Style2:
                    return 2;

                case StyleIndex.Style3:
                    return 3;

                case StyleIndex.Style4:
                    return 4;

                case StyleIndex.Style5:
                    return 5;

                case StyleIndex.Style6:
                    return 6;

                case StyleIndex.Style7:
                    return 7;

                case StyleIndex.Style8:
                    return 8;

                case StyleIndex.Style9:
                    return 9;

                case StyleIndex.Style10:
                    return 10;

                case StyleIndex.Style11:
                    return 11;

                case StyleIndex.Style12:
                    return 12;

                case StyleIndex.Style13:
                    return 13;

                case StyleIndex.Style14:
                    return 14;

                case StyleIndex.Style15:
                    return 15;
            }
            return -1;
        }
    }
    public class AutocompleteMenu : ToolStripDropDown {
        AutocompleteListView listView;
        ToolStripControlHost host;
        public Range Fragment {
            get;
            set;
        }

        /// <summary>
        /// Regex pattern for serach fragment around caret
        /// </summary>
        public string SearchPattern {
            get;
            set;
        }
        /// <summary>
        /// Minimum fragment length for popup
        /// </summary>
        public int MinFragmentLength {
            get;
            set;
        }
        /// <summary>
        /// User selects item
        /// </summary>
        public event EventHandler<SelectingEventArgs> Selecting;
        /// <summary>
        /// It fires after item iserting
        /// </summary>
        public event EventHandler<SelectedEventArgs> Selected;
        /// <summary>
        /// Allow TAB for select menu item
        /// </summary>
        public bool AllowTabKey {
            get {
                return listView.AllowTabKey;
            }
            set {
                listView.AllowTabKey = value;
            }
        }
        /// <summary>
        /// Interval of menu appear (ms)
        /// </summary>
        public int AppearInterval {
            get {
                return listView.AppearInterval;
            }
            set {
                listView.AppearInterval = value;
            }
        }

        public AutocompleteMenu( FastColoredTextBox tb ) {
            // create a new popup and add the list view to it 
            AutoClose = false;
            AutoSize = false;
            Margin = Padding.Empty;
            Padding = Padding.Empty;
            listView = new AutocompleteListView( tb );
            host = new ToolStripControlHost( listView );
            host.Margin = new Padding( 2, 2, 2, 2 );
            host.Padding = Padding.Empty;
            host.AutoSize = false;
            host.AutoToolTip = false;
            CalcSize( );
            base.Items.Add( host );
            SearchPattern = @"[\w\.]";
            MinFragmentLength = 2;
        }

        public new void Close( ) {
            listView.toolTip.Hide( listView );
            base.Close( );
        }

        public void CalcSize( ) {
            host.Size = listView.Size;
            Size = new System.Drawing.Size( listView.Size.Width + 4, listView.Size.Height + 4 );
        }

        public virtual void OnSelecting( ) {
            listView.OnSelecting( );
        }

        public void SelectNext( int shift ) {
            listView.SelectNext( shift );
        }

        public void OnSelecting( SelectingEventArgs args ) {
            if ( Selecting != null )
                Selecting( this, args );
        }

        public void OnSelected( SelectedEventArgs args ) {
            if ( Selected != null )
                Selected( this, args );
        }

        public new AutocompleteListView Items {
            get {
                return listView;
            }
        }
    }
    public class AutocompleteListView : UserControl {
        public List<AutocompleteItem> visibleItems;
        IEnumerable<AutocompleteItem> sourceItems = new List<AutocompleteItem>( );
        int selectedItemIndex = 0;
        int hoveredItemIndex = -1;
        int itemHeight;
        AutocompleteMenu Menu {
            get {
                return Parent as AutocompleteMenu;
            }
        }
        int oldItemCount = 0;
        FastColoredTextBox tb;
        public ToolTip toolTip = new ToolTip( );
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer( );

        public bool AllowTabKey {
            get;
            set;
        }
        public ImageList ImageList {
            get;
            set;
        }
        public int AppearInterval {
            get {
                return timer.Interval;
            }
            set {
                timer.Interval = value;
            }
        }

        public AutocompleteListView( FastColoredTextBox tb ) {
            SetStyle( ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true );
            base.Font = new Font( FontFamily.GenericSansSerif, 9 );
            visibleItems = new List<AutocompleteItem>( );
            itemHeight = Font.Height + 2;
            VerticalScroll.SmallChange = itemHeight;
            BackColor = Color.White;
            MaximumSize = new Size( Size.Width, 180 );
            toolTip.ShowAlways = false;
            AppearInterval = 500;
            timer.Tick += new EventHandler( timer_Tick );

            this.tb = tb;

            tb.KeyDown += new KeyEventHandler( tb_KeyDown );
            tb.SelectionChanged += new EventHandler( tb_SelectionChanged );
            tb.KeyPressed += new KeyPressEventHandler( tb_KeyPressed );

            Form form = tb.FindForm( );
            if ( form != null ) {
                form.LocationChanged += ( o, e ) => Menu.Close( );
                form.ResizeBegin += ( o, e ) => Menu.Close( );
                form.FormClosing += ( o, e ) => Menu.Close( );
                form.LostFocus += ( o, e ) => Menu.Close( );
            }

            tb.LostFocus += ( o, e ) => {
                if ( !Menu.Focused )
                    Menu.Close( );
            };
            tb.Scroll += ( o, e ) => Menu.Close( );
        }

        void tb_KeyPressed( object sender, KeyPressEventArgs e ) {
            bool backspaceORdel = e.KeyChar == '\b' || e.KeyChar == 0xff;

            /*
            if (backspaceORdel)
                prevSelection = tb.Selection.Start;*/

            if ( Menu.Visible && !backspaceORdel )
                DoAutocomplete( sender as FastColoredTextBox );
            else
                ResetTimer( timer );
        }

        void timer_Tick( object sender, EventArgs e ) {
            timer.Stop( );
            DoAutocomplete( tb );
        }

        void ResetTimer( System.Windows.Forms.Timer timer ) {
            timer.Stop( );
            timer.Start( );
        }

        void DoAutocomplete( FastColoredTextBox tb ) {
            visibleItems.Clear( );
            selectedItemIndex = 0;
            VerticalScroll.Value = 0;
            //get fragment around caret
            Range fragment = tb.Selection.GetFragment( Menu.SearchPattern );
            string text = fragment.Text;
            //calc screen point for popup menu
            Point point = tb.PlaceToPoint( fragment.End );
            point.Offset( 2, tb.CharHeight );
            //
            if ( text.Length >= Menu.MinFragmentLength && tb.Selection.Start == tb.Selection.End ) {
                Menu.Fragment = fragment;
                bool foundSelected = false;
                //build popup menu
                foreach ( var item in sourceItems ) {
                    item.Parent = Menu;
                    CompareResult res = item.Compare( text );
                    if ( res != CompareResult.Hidden )
                        visibleItems.Add( item );
                    if ( res == CompareResult.VisibleAndSelected && !foundSelected ) {
                        foundSelected = true;
                        selectedItemIndex = visibleItems.Count - 1;
                    }
                }

                if ( foundSelected ) {
                    AdjustScroll( );
                    DoSelectedVisible( );
                }
            }

            //show popup menu
            if ( Count > 0 ) {
                if ( !Menu.Visible ) {
                    //prevSelection = tb.Selection.Start;
                    Menu.Show( tb, point );
                } else
                    Invalidate( );
            } else
                Menu.Close( );
        }

        void tb_SelectionChanged( object sender, EventArgs e ) {
            /*
            FastColoredTextBox tb = sender as FastColoredTextBox;
            
            if (Math.Abs(prevSelection.iChar - tb.Selection.Start.iChar) > 1 ||
                        prevSelection.iLine != tb.Selection.Start.iLine)
                Menu.Close();
            prevSelection = tb.Selection.Start;*/

            if ( Menu.Visible ) {
                bool needClose = false;

                if ( tb.Selection.Start != tb.Selection.End )
                    needClose = true;
                else
                    if ( !Menu.Fragment.Contains( tb.Selection.Start ) ) {
                        if ( tb.Selection.Start.iLine == Menu.Fragment.End.iLine && tb.Selection.Start.iChar == Menu.Fragment.End.iChar + 1 ) {
                            //user press key at end of fragment
                            char c = tb.Selection.CharBeforeStart;
                            if ( !Regex.IsMatch( c.ToString( ), Menu.SearchPattern ) )//check char
                                needClose = true;
                        } else
                            needClose = true;
                    }

                if ( needClose )
                    Menu.Close( );
            }

        }

        void tb_KeyDown( object sender, KeyEventArgs e ) {
            if ( Menu.Visible )
                if ( ProcessKey( e.KeyCode, e.Modifiers ) )
                    e.Handled = true;

            if ( !Menu.Visible )
                if ( e.Modifiers == Keys.Control && e.KeyCode == Keys.Space ) {
                    DoAutocomplete( tb );
                    e.Handled = true;
                }
        }

        void AdjustScroll( ) {
            if ( oldItemCount == visibleItems.Count )
                return;

            int needHeight = itemHeight * visibleItems.Count + 1;
            Height = Math.Min( needHeight, MaximumSize.Height );
            Menu.CalcSize( );

            AutoScrollMinSize = new Size( 0, needHeight );
            oldItemCount = visibleItems.Count;
        }

        protected override void OnPaint( PaintEventArgs e ) {
            AdjustScroll( );
            int startI = VerticalScroll.Value / itemHeight - 1;
            int finishI = ( VerticalScroll.Value + ClientSize.Height ) / itemHeight + 1;
            startI = Math.Max( startI, 0 );
            finishI = Math.Min( finishI, visibleItems.Count );
            int y = 0;
            int leftPadding = 18;
            for ( int i = startI ; i < finishI ; i++ ) {
                y = i * itemHeight - VerticalScroll.Value;

                if ( ImageList != null && visibleItems[ i ].ImageIndex >= 0 )
                    e.Graphics.DrawImage( ImageList.Images[ visibleItems[ i ].ImageIndex ], 1, y );

                if ( i == selectedItemIndex ) {
                    Brush selectedBrush = new LinearGradientBrush( new Point( 0, y - 3 ), new Point( 0, y + itemHeight ), Color.White, Color.Orange );
                    e.Graphics.FillRectangle( selectedBrush, leftPadding, y, ClientSize.Width - 1 - leftPadding, itemHeight - 1 );
                    e.Graphics.DrawRectangle( Pens.Orange, leftPadding, y, ClientSize.Width - 1 - leftPadding, itemHeight - 1 );
                }
                if ( i == hoveredItemIndex )
                    e.Graphics.DrawRectangle( Pens.Red, leftPadding, y, ClientSize.Width - 1 - leftPadding, itemHeight - 1 );
                e.Graphics.DrawString( visibleItems[ i ].ToString( ), Font, Brushes.Black, leftPadding, y );
            }
        }

        protected override void OnScroll( ScrollEventArgs se ) {
            base.OnScroll( se );
            Invalidate( );
        }

        protected override void OnMouseClick( MouseEventArgs e ) {
            base.OnMouseClick( e );

            if ( e.Button == System.Windows.Forms.MouseButtons.Left ) {
                selectedItemIndex = PointToItemIndex( e.Location );
                DoSelectedVisible( );
                Invalidate( );
            }
        }

        protected override void OnMouseDoubleClick( MouseEventArgs e ) {
            base.OnMouseDoubleClick( e );
            selectedItemIndex = PointToItemIndex( e.Location );
            Invalidate( );
            OnSelecting( );
        }

        public virtual void OnSelecting( ) {
            if ( selectedItemIndex < 0 || selectedItemIndex >= visibleItems.Count )
                return;
            tb.manager.BeginAutoUndoCommands( );
            try {
                AutocompleteItem item = visibleItems[ selectedItemIndex ];
                SelectingEventArgs args = new SelectingEventArgs( ) {
                    Item = item,
                    SelectedIndex = selectedItemIndex
                };

                Menu.OnSelecting( args );

                if ( args.Cancel ) {
                    selectedItemIndex = args.SelectedIndex;
                    Invalidate( );
                    return;
                }

                if ( !args.Handled ) {
                    var fragment = Menu.Fragment;
                    DoAutocomplete( item, fragment );
                }

                Menu.Close( );
                //
                SelectedEventArgs args2 = new SelectedEventArgs( ) {
                    Item = item,
                    Tb = Menu.Fragment.tb
                };
                item.OnSelected( Menu, args2 );
                Menu.OnSelected( args2 );
            } finally {
                tb.manager.EndAutoUndoCommands( );
            }
        }

        public void DoAutocomplete( AutocompleteItem item, Range fragment ) {
            string newText = item.GetTextForReplace( );
            //replace text of fragment
            var tb = fragment.tb;
            tb.Selection.Start = fragment.Start;
            tb.Selection.End = fragment.End;
            tb.InsertText( newText );
            tb.Focus( );
        }

        int PointToItemIndex( Point p ) {
            return ( p.Y + VerticalScroll.Value ) / itemHeight;
        }

        protected override bool ProcessCmdKey( ref Message msg, Keys keyData ) {
            ProcessKey( keyData, Keys.None );

            return base.ProcessCmdKey( ref msg, keyData );
        }

        public bool ProcessKey( Keys keyData, Keys keyModifiers ) {
            if ( keyModifiers == Keys.None )
                switch ( keyData ) {
                    case Keys.Down:
                        SelectNext( +1 );
                        return true;
                    case Keys.PageDown:
                        SelectNext( +10 );
                        return true;
                    case Keys.Up:
                        SelectNext( -1 );
                        return true;
                    case Keys.PageUp:
                        SelectNext( -10 );
                        return true;
                    case Keys.Enter:
                        OnSelecting( );
                        return true;
                    case Keys.Tab:
                        if ( !AllowTabKey )
                            break;
                        OnSelecting( );
                        return true;
                    case Keys.Escape:
                        Menu.Close( );
                        return true;
                }

            return false;
        }

        public void SelectNext( int shift ) {
            selectedItemIndex = Math.Max( 0, Math.Min( selectedItemIndex + shift, visibleItems.Count - 1 ) );
            DoSelectedVisible( );
            //
            Invalidate( );
        }

        public void DoSelectedVisible( ) {
            if ( selectedItemIndex >= 0 && selectedItemIndex < visibleItems.Count )
                SetToolTip( visibleItems[ selectedItemIndex ] );

            var y = selectedItemIndex * itemHeight - VerticalScroll.Value;
            if ( y < 0 )
                VerticalScroll.Value = selectedItemIndex * itemHeight;
            if ( y > ClientSize.Height - itemHeight )
                VerticalScroll.Value = selectedItemIndex * itemHeight - ClientSize.Height + itemHeight;
            //some magic for update scrolls
            AutoScrollMinSize -= new Size( 1, 0 );
            AutoScrollMinSize += new Size( 1, 0 );
        }

        public void SetToolTip( AutocompleteItem autocompleteItem ) {
            var title = visibleItems[ selectedItemIndex ].ToolTipTitle;
            var text = visibleItems[ selectedItemIndex ].ToolTipText;

            if ( string.IsNullOrEmpty( title ) ) {
                toolTip.ToolTipTitle = null;
                toolTip.SetToolTip( this, null );
                return;
            }

            if ( string.IsNullOrEmpty( text ) ) {
                toolTip.ToolTipTitle = null;
                toolTip.Show( title, this, Width + 3, 0, 3000 );
            } else {
                toolTip.ToolTipTitle = title;
                toolTip.Show( text, this, Width + 3, 0, 3000 );
            }
        }

        public int Count {
            get {
                return visibleItems.Count;
            }
        }

        public void SetAutocompleteItems( ICollection<string> items ) {
            List<AutocompleteItem> list = new List<AutocompleteItem>( items.Count );
            foreach ( var item in items )
                list.Add( new AutocompleteItem( item ) );
            SetAutocompleteItems( list );
        }

        public void SetAutocompleteItems( ICollection<AutocompleteItem> items ) {
            sourceItems = items;
        }
    }
    public class SelectingEventArgs : EventArgs {
        public AutocompleteItem Item {
            get;
            set;
        }
        public bool Cancel {
            get;
            set;
        }
        public int SelectedIndex {
            get;
            set;
        }
        public bool Handled {
            get;
            set;
        }
    }
    public class SelectedEventArgs : EventArgs {
        public AutocompleteItem Item {
            get;
            set;
        }
        public FastColoredTextBox Tb {
            get;
            set;
        }
    }
    public class AutocompleteItem {
        public string Text;
        public int ImageIndex = -1;
        public object Tag;
        public string Hint;
        public AutocompleteMenu Parent {
            get;
            set;
        }

        public AutocompleteItem( ) {
        }

        public AutocompleteItem( string text ) {
            Text = text;
        }

        public AutocompleteItem( string text, int imageIndex )
            : this( text ) {
            this.ImageIndex = imageIndex;
        }

        /// <summary>
        /// Returns text for inserting into Textbox
        /// </summary>
        public virtual string GetTextForReplace( ) {
            return Text;
        }

        /// <summary>
        /// Compares fragment text with this item
        /// </summary>
        public virtual CompareResult Compare( string fragmentText ) {
            if ( Text.StartsWith( fragmentText, StringComparison.InvariantCultureIgnoreCase ) &&
                   Text != fragmentText )
                return CompareResult.VisibleAndSelected;

            return CompareResult.Hidden;
        }

        /// <summary>
        /// Returns text for display into popup menu
        /// </summary>
        public override string ToString( ) {
            return Text;
        }

        /// <summary>
        /// This method is called after item inserted into text
        /// </summary>
        public virtual void OnSelected( AutocompleteMenu popupMenu, SelectedEventArgs e ) {
            ;
        }

        /// <summary>
        /// Title for tooltip.
        /// </summary>
        /// <remarks>Return null for disable tooltip for this item</remarks>
        public virtual string ToolTipTitle {
            get {
                return null;
            }
        }

        /// <summary>
        /// Tooltip text.
        /// </summary>
        /// <remarks>For display tooltip text, ToolTipTitle must be not null</remarks>
        public virtual string ToolTipText {
            get {
                return null;
            }
        }
    }
    public enum CompareResult {
        /// <summary>
        /// Item do not appears
        /// </summary>
        Hidden,
        /// <summary>
        /// Item appears
        /// </summary>
        Visible,
        /// <summary>
        /// Item appears and will selected
        /// </summary>
        VisibleAndSelected
    }
    public class SnippetAutocompleteItem : AutocompleteItem {
        public SnippetAutocompleteItem( string snippet ) {
            Text = snippet.Replace( "\r", "" );
        }

        public override string ToString( ) {
            return Text.Replace( "\n", " " ).Replace( "^", "" );
        }

        public override string GetTextForReplace( ) {
            return Text;
        }

        public override void OnSelected( AutocompleteMenu popupMenu, SelectedEventArgs e ) {
            e.Tb.BeginUpdate( );
            e.Tb.Selection.BeginUpdate( );
            //remember places
            var p1 = popupMenu.Fragment.Start;
            var p2 = e.Tb.Selection.Start;
            //do auto indent
            if ( e.Tb.AutoIndent ) {
                for ( int iLine = p1.iLine + 1 ; iLine <= p2.iLine ; iLine++ ) {
                    e.Tb.Selection.Start = new Place( 0, iLine );
                    e.Tb.DoAutoIndent( iLine );
                }
            }
            e.Tb.Selection.Start = p1;
            //move caret position right and find char ^
            while ( e.Tb.Selection.CharBeforeStart != '^' )
                if ( !e.Tb.Selection.GoRightThroughFolded( ) )
                    break;
            //remove char ^
            e.Tb.Selection.GoLeft( true );
            e.Tb.InsertText( "" );
            //
            e.Tb.Selection.EndUpdate( );
            e.Tb.EndUpdate( );
        }

        /// <summary>
        /// Compares fragment text with this item
        /// </summary>
        public override CompareResult Compare( string fragmentText ) {
            if ( Text.StartsWith( fragmentText, StringComparison.InvariantCultureIgnoreCase ) &&
                   Text != fragmentText )
                return CompareResult.Visible;

            return CompareResult.Hidden;
        }

        public override string ToolTipTitle {
            get {
                return "Code snippet:";
            }
        }

        public override string ToolTipText {
            get {
                return GetTextForReplace( );
            }
        }
    }
    public class MethodAutocompleteItem : AutocompleteItem {
        string firstPart;
        string lowercaseText;

        public MethodAutocompleteItem( string text )
            : base( text ) {
            lowercaseText = Text.ToLower( );
        }

        public override CompareResult Compare( string fragmentText ) {
            int i = fragmentText.LastIndexOf( '.' );
            if ( i < 0 )
                return CompareResult.Hidden;
            string lastPart = fragmentText.Substring( i + 1 );
            firstPart = fragmentText.Substring( 0, i );

            if ( lastPart == "" )
                return CompareResult.Visible;
            if ( Text.StartsWith( lastPart, StringComparison.InvariantCultureIgnoreCase ) )
                return CompareResult.VisibleAndSelected;
            if ( lowercaseText.Contains( lastPart.ToLower( ) ) )
                return CompareResult.Visible;

            return CompareResult.Hidden;
        }

        public override string GetTextForReplace( ) {
            return firstPart + "." + Text;
        }
    }
    public class LimitedStack<T> {
        T[ ] items;
        int count;
        int start;

        /// <summary>
        /// Max stack length
        /// </summary>
        public int MaxItemCount {
            get {
                return items.Length;
            }
        }

        /// <summary>
        /// Current length of stack
        /// </summary>
        public int Count {
            get {
                return count;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxItemCount">Maximum length of stack</param>
        public LimitedStack( int maxItemCount ) {
            items = new T[ maxItemCount ];
            count = 0;
            start = 0;
        }

        /// <summary>
        /// Pop item
        /// </summary>
        public T Pop( ) {
            if ( count == 0 )
                throw new Exception( "Stack is empty" );

            int i = LastIndex;
            T item = items[ i ];
            items[ i ] = default( T );

            count--;

            return item;
        }

        int LastIndex {
            get {
                return ( start + count - 1 ) % items.Length;
            }
        }

        /// <summary>
        /// Peek item
        /// </summary>
        public T Peek( ) {
            if ( count == 0 )
                return default( T );

            return items[ LastIndex ];
        }

        /// <summary>
        /// Push item
        /// </summary>
        public void Push( T item ) {
            if ( count == items.Length )
                start = ( start + 1 ) % items.Length;
            else
                count++;

            items[ LastIndex ] = item;
        }

        /// <summary>
        /// Clear stack
        /// </summary>
        public void Clear( ) {
            items = new T[ items.Length ];
            count = 0;
            start = 0;
        }

        public T[ ] ToArray( ) {
            T[ ] result = new T[ count ];
            for ( int i = 0 ; i < count ; i++ )
                result[ i ] = items[ ( start + i ) % items.Length ];
            return result;
        }
    }
    #endregion
    #region FATabStrip
    public enum HitTestResult {
        CloseButton,
        MenuGlyph,
        TabItem,
        None
    }
    public enum ThemeTypes {
        WindowsXP,
        Office2000,
        Office2003
    }
    public enum FATabStripItemChangeTypes {
        Added,
        Removed,
        Changed,
        SelectionChanged
    }
    [ToolboxItem( false )]
    public class BaseStyledPanel : ContainerControl {
        #region Fields

        public static ToolStripProfessionalRenderer renderer;

        #endregion

        #region Events

        public event EventHandler ThemeChanged;

        #endregion

        #region Ctor

        static BaseStyledPanel( ) {
            renderer = new ToolStripProfessionalRenderer( );
        }

        public BaseStyledPanel( ) {
            // Set painting style for better performance.
            SetStyle( ControlStyles.AllPaintingInWmPaint, true );
            SetStyle( ControlStyles.OptimizedDoubleBuffer, true );
            SetStyle( ControlStyles.ResizeRedraw, true );
            SetStyle( ControlStyles.UserPaint, true );
        }

        #endregion

        #region Methods

        protected override void OnSystemColorsChanged( EventArgs e ) {
            base.OnSystemColorsChanged( e );
            UpdateRenderer( );
            Invalidate( );
        }

        protected virtual void OnThemeChanged( EventArgs e ) {
            if ( ThemeChanged != null )
                ThemeChanged( this, e );
        }

        public void UpdateRenderer( ) {
            if ( !UseThemes ) {
                renderer.ColorTable.UseSystemColors = true;
            } else {
                renderer.ColorTable.UseSystemColors = false;
            }
        }

        #endregion

        #region Props

        [Browsable( false )]
        public ToolStripProfessionalRenderer ToolStripRenderer {
            get {
                return renderer;
            }
        }

        [DefaultValue( true )]
        [Browsable( false )]
        public bool UseThemes {
            get {
                return VisualStyleRenderer.IsSupported && VisualStyleInformation.IsSupportedByOS && Application.RenderWithVisualStyles;
            }
        }

        #endregion
    }
    public class FATabStripItemCollection : CollectionWithEvents {
        #region Fields

        [Browsable( false )]
        public event CollectionChangeEventHandler CollectionChanged;

        public int lockUpdate;

        #endregion

        #region Ctor

        public FATabStripItemCollection( ) {
            lockUpdate = 0;
        }

        #endregion

        #region Props

        public FATabStripItem this[ int index ] {
            get {
                if ( index < 0 || List.Count - 1 < index )
                    return null;

                return ( FATabStripItem )List[ index ];
            }
            set {
                List[ index ] = value;
            }
        }

        [Browsable( false )]
        public virtual int DrawnCount {
            get {
                int count = Count, res = 0;
                if ( count == 0 )
                    return 0;
                for ( int n = 0 ; n < count ; n++ ) {
                    if ( this[ n ].IsDrawn )
                        res++;
                }
                return res;
            }
        }

        public virtual FATabStripItem LastVisible {
            get {
                for ( int n = Count - 1 ; n > 0 ; n-- ) {
                    if ( this[ n ].Visible )
                        return this[ n ];
                }

                return null;
            }
        }

        public virtual FATabStripItem FirstVisible {
            get {
                for ( int n = 0 ; n < Count ; n++ ) {
                    if ( this[ n ].Visible )
                        return this[ n ];
                }

                return null;
            }
        }

        [Browsable( false )]
        public virtual int VisibleCount {
            get {
                int count = Count, res = 0;
                if ( count == 0 )
                    return 0;
                for ( int n = 0 ; n < count ; n++ ) {
                    if ( this[ n ].Visible )
                        res++;
                }
                return res;
            }
        }

        #endregion

        #region Methods

        protected virtual void OnCollectionChanged( CollectionChangeEventArgs e ) {
            if ( CollectionChanged != null )
                CollectionChanged( this, e );
        }

        protected virtual void BeginUpdate( ) {
            lockUpdate++;
        }

        protected virtual void EndUpdate( ) {
            if ( --lockUpdate == 0 )
                OnCollectionChanged( new CollectionChangeEventArgs( CollectionChangeAction.Refresh, null ) );
        }

        public virtual void AddRange( FATabStripItem[ ] items ) {
            BeginUpdate( );
            try {
                foreach ( FATabStripItem item in items ) {
                    List.Add( item );
                }
            } finally {
                EndUpdate( );
            }
        }

        public virtual void Assign( FATabStripItemCollection collection ) {
            BeginUpdate( );
            try {
                Clear( );
                for ( int n = 0 ; n < collection.Count ; n++ ) {
                    FATabStripItem item = collection[ n ];
                    FATabStripItem newItem = new FATabStripItem( );
                    newItem.Assign( item );
                    Add( newItem );
                }
            } finally {
                EndUpdate( );
            }
        }

        public virtual int Add( FATabStripItem item ) {
            int res = IndexOf( item );
            if ( res == -1 )
                res = List.Add( item );
            return res;
        }

        public virtual void Remove( FATabStripItem item ) {
            if ( List.Contains( item ) )
                List.Remove( item );
        }

        public virtual FATabStripItem MoveTo( int newIndex, FATabStripItem item ) {
            int currentIndex = List.IndexOf( item );
            if ( currentIndex >= 0 ) {
                RemoveAt( currentIndex );
                Insert( 0, item );

                return item;
            }

            return null;
        }

        public virtual int IndexOf( FATabStripItem item ) {
            return List.IndexOf( item );
        }

        public virtual bool Contains( FATabStripItem item ) {
            return List.Contains( item );
        }

        public virtual void Insert( int index, FATabStripItem item ) {
            if ( Contains( item ) )
                return;
            List.Insert( index, item );
        }

        protected override void OnInsertComplete( int index, object item ) {
            FATabStripItem itm = item as FATabStripItem;
            itm.Changed += new EventHandler( OnItem_Changed );
            OnCollectionChanged( new CollectionChangeEventArgs( CollectionChangeAction.Add, item ) );
        }

        protected override void OnRemove( int index, object item ) {
            base.OnRemove( index, item );
            FATabStripItem itm = item as FATabStripItem;
            itm.Changed -= new EventHandler( OnItem_Changed );
            OnCollectionChanged( new CollectionChangeEventArgs( CollectionChangeAction.Remove, item ) );
        }

        protected override void OnClear( ) {
            if ( Count == 0 )
                return;
            BeginUpdate( );
            try {
                for ( int n = Count - 1 ; n >= 0 ; n-- ) {
                    RemoveAt( n );
                }
            } finally {
                EndUpdate( );
            }
        }

        protected virtual void OnItem_Changed( object sender, EventArgs e ) {
            OnCollectionChanged( new CollectionChangeEventArgs( CollectionChangeAction.Refresh, sender ) );
        }

        #endregion
    }
    public class FATabStripMenuGlyph {
        #region Fields

        public Rectangle glyphRect = Rectangle.Empty;
        public bool isMouseOver = false;
        public ToolStripProfessionalRenderer renderer;

        #endregion

        #region Props

        public bool IsMouseOver {
            get {
                return isMouseOver;
            }
            set {
                isMouseOver = value;
            }
        }

        public Rectangle Bounds {
            get {
                return glyphRect;
            }
            set {
                glyphRect = value;
            }
        }

        #endregion

        #region Ctor

        public FATabStripMenuGlyph( ToolStripProfessionalRenderer renderer ) {
            this.renderer = renderer;
        }

        #endregion

        #region Methods

        public void DrawGlyph( Graphics g ) {
            if ( isMouseOver ) {
                Color fill = renderer.ColorTable.ButtonSelectedHighlight; //Color.FromArgb(35, SystemColors.Highlight);
                g.FillRectangle( new SolidBrush( fill ), glyphRect );
                Rectangle borderRect = glyphRect;

                borderRect.Width--;
                borderRect.Height--;

                g.DrawRectangle( SystemPens.Highlight, borderRect );
            }

            SmoothingMode bak = g.SmoothingMode;

            g.SmoothingMode = SmoothingMode.Default;

            using ( Pen pen = new Pen( Color.Black ) ) {
                pen.Width = 2;

                g.DrawLine( pen, new Point( glyphRect.Left + ( glyphRect.Width / 3 ) - 2, glyphRect.Height / 2 - 1 ),
                    new Point( glyphRect.Right - ( glyphRect.Width / 3 ), glyphRect.Height / 2 - 1 ) );
            }

            g.FillPolygon( Brushes.Black, new Point[ ]{
                new Point(glyphRect.Left + (glyphRect.Width / 3)-2, glyphRect.Height / 2+2),
                new Point(glyphRect.Right - (glyphRect.Width / 3), glyphRect.Height / 2+2),
                new Point(glyphRect.Left + glyphRect.Width / 2-1,glyphRect.Bottom-4)} );

            g.SmoothingMode = bak;
        }

        #endregion
    }
    [Designer( typeof( FATabStripItemDesigner ) )]
    [DefaultProperty( "Title" )]
    [DefaultEvent( "Changed" )]
    public class FATabStripItem : Panel {
        #region Events

        public event EventHandler Changed;

        #endregion

        #region Fields

        //public DrawItemState drawState = DrawItemState.None;
        public RectangleF stripRect = Rectangle.Empty;
        public Image image = null;
        public bool canClose = true;
        public bool selected = false;
        public bool visible = true;
        public bool isDrawn = false;
        public string title = string.Empty;

        #endregion

        #region Props

        [Browsable( false )]
        [EditorBrowsable( EditorBrowsableState.Never )]
        public new Size Size {
            get {
                return base.Size;
            }
            set {
                base.Size = value;
            }
        }

        [DefaultValue( true )]
        public new bool Visible {
            get {
                return visible;
            }
            set {
                if ( visible == value )
                    return;

                visible = value;
                OnChanged( );
            }
        }

        bool _saved = true;

        public bool Saved {
            get {
                return _saved;
            }
            set {
                if ( _saved != value ) {
                    _saved = value;
                    Parent.Invalidate( );
                }
            }
        }

        public RectangleF StripRect {
            get {
                return stripRect;
            }
            set {
                stripRect = value;
            }
        }

        [Browsable( false )]
        [DefaultValue( false )]
        [EditorBrowsable( EditorBrowsableState.Never )]
        public bool IsDrawn {
            get {
                return isDrawn;
            }
            set {
                if ( isDrawn == value )
                    return;

                isDrawn = value;
            }
        }

        /// <summary>
        /// Image of <see cref="FATabStripItem"/> which will be displayed
        /// on menu items.
        /// </summary>
        [DefaultValue( null )]
        public Image Image {
            get {
                return image;
            }
            set {
                image = value;
            }
        }

        [DefaultValue( true )]
        public bool CanClose {
            get {
                return canClose;
            }
            set {
                canClose = value;
            }
        }

        [DefaultValue( "Name" )]
        public string Title {
            get {
                return title;
            }
            set {
                if ( title == value )
                    return;

                title = value;
                OnChanged( );
            }
        }

        /// <summary>
        /// Gets and sets a value indicating if the page is selected.
        /// </summary>
        [DefaultValue( false )]
        [Browsable( false )]
        public bool Selected {
            get {
                return selected;
            }
            set {
                if ( selected == value )
                    return;

                selected = value;
            }
        }

        [Browsable( false )]
        public string Caption {
            get {
                return Title;
            }
        }

        #endregion

        #region Ctor

        public FATabStripItem( )
            : this( string.Empty, null ) {
        }

        public FATabStripItem( Control displayControl )
            : this( string.Empty, displayControl ) {
        }

        public FATabStripItem( string caption, Control displayControl ) {
            AutoScroll = true;
            SetStyle( ControlStyles.OptimizedDoubleBuffer, true );
            SetStyle( ControlStyles.ResizeRedraw, true );
            SetStyle( ControlStyles.UserPaint, true );
            SetStyle( ControlStyles.AllPaintingInWmPaint, true );
            SetStyle( ControlStyles.ContainerControl, true );

            selected = false;
            Visible = true;

            UpdateText( caption, displayControl );

            //Add to controls
            if ( displayControl != null )
                Controls.Add( displayControl );
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Handles proper disposition of the tab page control.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose( bool disposing ) {
            try {
                if ( Parent != null ) {
                    ( Parent as FATabStrip ).Items.Remove( this );
                    Parent = null;
                }
            } catch {
            }
            base.Dispose( disposing );
            if ( disposing ) {
                if ( image != null )
                    image.Dispose( );
            }
        }

        #endregion

        #region ShouldSerialize

        public bool ShouldSerializeIsDrawn( ) {
            return false;
        }

        public bool ShouldSerializeDock( ) {
            return false;
        }

        public bool ShouldSerializeControls( ) {
            return Controls != null && Controls.Count > 0;
        }

        public bool ShouldSerializeVisible( ) {
            return true;
        }

        #endregion

        #region Methods

        public void UpdateText( string caption, Control displayControl ) {
            if ( displayControl != null && displayControl is ICaptionSupport ) {
                ICaptionSupport capControl = displayControl as ICaptionSupport;
                Title = capControl.Caption;
            } else if ( caption.Length <= 0 && displayControl != null ) {
                Title = displayControl.Text;
            } else if ( caption != null ) {
                Title = caption;
            } else {
                Title = string.Empty;
            }
        }

        public void Assign( FATabStripItem item ) {
            Visible = item.Visible;
            Text = item.Text;
            CanClose = item.CanClose;
            Tag = item.Tag;
        }

        protected internal virtual void OnChanged( ) {
            if ( Changed != null )
                Changed( this, EventArgs.Empty );
        }

        public new Control Parent {
            get {
                return base.Parent;
            }
            set {
                if ( value == null && base.Parent != null )
                    ( base.Parent as FATabStrip ).Items.Remove( this );
                if ( value != null && base.Parent == null )
                    ( value as FATabStrip ).Items.Add( this );
                base.Parent = value;
            }
        }

        /// <summary>
        /// Return a string representation of page.
        /// </summary>
        /// <returns></returns>
        public override string ToString( ) {
            return Caption;
        }

        #endregion
    }
    public class FATabStripCloseButton {
        #region Fields

        public Rectangle crossRect = Rectangle.Empty;
        public bool isMouseOver = false;
        public ToolStripProfessionalRenderer renderer;

        #endregion

        #region Props

        public bool IsMouseOver {
            get {
                return isMouseOver;
            }
            set {
                isMouseOver = value;
            }
        }

        public Rectangle Bounds {
            get {
                return crossRect;
            }
            set {
                crossRect = value;
            }
        }

        #endregion

        #region Ctor

        public FATabStripCloseButton( ToolStripProfessionalRenderer renderer ) {
            this.renderer = renderer;
        }

        #endregion

        #region Methods

        public void DrawCross( Graphics g ) {
            crossRect.Width--;
            crossRect.Height--;
            if ( isMouseOver ) {

                Brush fill = new LinearGradientBrush( crossRect, renderer.ColorTable.ButtonPressedBorder, renderer.ColorTable.ButtonCheckedGradientEnd, LinearGradientMode.Vertical );

                g.FillRectangle( fill, crossRect );

            }

            using ( Pen pen = new Pen( Color.White, 2f ) ) {
                g.DrawLine( pen, crossRect.Left + 3, crossRect.Top + 3,
                    crossRect.Right - 3, crossRect.Bottom - 3 );

                g.DrawLine( pen, crossRect.Right - 3, crossRect.Top + 3,
                    crossRect.Left + 3, crossRect.Bottom - 3 );
            }
            crossRect.Width++;
            crossRect.Height++;
        }

        #endregion
    }
    [Designer( "FATabControl" )]
    public class FATabStrip : BaseStyledPanel, ISupportInitialize, IDisposable {
        #region Static Fields

        public static int PreferredWidth = 350;
        public static int PreferredHeight = 200;

        #endregion

        #region Constants

        public const int DEF_HEADER_HEIGHT = 25;
        public const int DEF_GLYPH_WIDTH = 40;

        public int DEF_START_POS = 10;

        #endregion

        #region Events

        public event TabStripItemClosingHandler TabStripItemClosing;
        public event TabStripItemChangedHandler TabStripItemSelectionChanged;
        public event HandledEventHandler MenuItemsLoading;
        public event EventHandler MenuItemsLoaded;
        public event EventHandler TabStripItemClosed;

        #endregion

        #region Fields

        public Rectangle stripButtonRect = Rectangle.Empty;
        public FATabStripItem selectedItem = null;
        public ContextMenuStrip menu = null;
        public FATabStripMenuGlyph menuGlyph = null;
        public FATabStripCloseButton closeButton = null;
        public FATabStripItemCollection items;
        public StringFormat sf = null;
        public static Font defaultFont = new Font( "Calibri", 11f, FontStyle.Regular );

        public bool alwaysShowClose = true;
        public bool isIniting = false;
        public bool alwaysShowMenuGlyph = true;
        public bool menuOpen = false;

        public override Font Font {
            get {
                return new Font( "Calibri", 11f, FontStyle.Regular );
            }
            set {
                base.Font = value;
            }
        }

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Returns hit test results
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public HitTestResult HitTest( Point pt ) {
            if ( closeButton.Bounds.Contains( pt ) )
                return HitTestResult.CloseButton;

            if ( menuGlyph.Bounds.Contains( pt ) )
                return HitTestResult.MenuGlyph;

            if ( GetTabItemByPoint( pt ) != null )
                return HitTestResult.TabItem;

            //No other result is available.
            return HitTestResult.None;
        }

        /// <summary>
        /// Add a <see cref="FATabStripItem"/> to this control without selecting it.
        /// </summary>
        /// <param name="tabItem"></param>
        public void AddTab( FATabStripItem tabItem ) {
            AddTab( tabItem, false );
        }

        /// <summary>
        /// Add a <see cref="FATabStripItem"/> to this control.
        /// User can make the currently selected item or not.
        /// </summary>
        /// <param name="tabItem"></param>
        public void AddTab( FATabStripItem tabItem, bool autoSelect ) {
            if ( AddedTab != null )
                AddedTab( this, tabItem );
            tabItem.Dock = DockStyle.Fill;
            Items.Add( tabItem );

            if ( ( autoSelect && tabItem.Visible ) || ( tabItem.Visible && Items.DrawnCount < 1 ) ) {
                SelectedItem = tabItem;
                SelectItem( tabItem );
            }
        }

        public event EventHandler<FATabStripItem> AddedTab;

        /// <summary>
        /// Remove a <see cref="FATabStripItem"/> from this control.
        /// </summary>
        /// <param name="tabItem"></param>
        public void RemoveTab( FATabStripItem tabItem ) {
            int tabIndex = Items.IndexOf( tabItem );

            if ( tabIndex >= 0 ) {
                UnSelectItem( tabItem );
                Items.Remove( tabItem );
            }

            if ( Items.Count > 0 ) {
                if ( RightToLeft == RightToLeft.No ) {
                    if ( Items[ tabIndex - 1 ] != null ) {
                        SelectedItem = Items[ tabIndex - 1 ];
                    } else {
                        SelectedItem = Items.FirstVisible;
                    }
                } else {
                    if ( Items[ tabIndex + 1 ] != null ) {
                        SelectedItem = Items[ tabIndex + 1 ];
                    } else {
                        SelectedItem = Items.LastVisible;
                    }
                }
            }
        }

        /// <summary>
        /// Get a <see cref="FATabStripItem"/> at provided point.
        /// If no item was found, returns null value.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public FATabStripItem GetTabItemByPoint( Point pt ) {
            FATabStripItem item = null;
            bool found = false;

            for ( int i = 0 ; i < Items.Count ; i++ ) {
                FATabStripItem current = Items[ i ];

                if ( current.StripRect.Contains( pt ) && current.Visible && current.IsDrawn ) {
                    item = current;
                    found = true;
                }

                if ( found )
                    break;
            }

            return item;
        }

        /// <summary>
        /// Display items menu
        /// </summary>
        public virtual void ShowMenu( ) {
            if ( menu.Visible == false && menu.Items.Count > 0 ) {
                if ( RightToLeft == RightToLeft.No ) {
                    menu.Show( this, new Point( menuGlyph.Bounds.Left, menuGlyph.Bounds.Bottom ) );
                } else {
                    menu.Show( this, new Point( menuGlyph.Bounds.Right, menuGlyph.Bounds.Bottom ) );
                }

                menuOpen = true;
            }
        }

        #endregion

        #region public

        public void UnDrawAll( ) {
            for ( int i = 0 ; i < Items.Count ; i++ ) {
                Items[ i ].IsDrawn = false;
            }
        }

        public void SelectItem( FATabStripItem tabItem ) {
            tabItem.Dock = DockStyle.Fill;
            tabItem.Visible = true;
            tabItem.Selected = true;
        }

        public void UnSelectItem( FATabStripItem tabItem ) {
            //tabItem.Visible = false;
            tabItem.Selected = false;
        }

        #endregion

        #region Protected

        /// <summary>
        /// Fires <see cref="TabStripItemClosing"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void OnTabStripItemClosing( TabStripItemClosingEventArgs e ) {
            if ( TabStripItemClosing != null )
                TabStripItemClosing( e );
        }

        /// <summary>
        /// Fires <see cref="TabStripItemClosed"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void OnTabStripItemClosed( EventArgs e ) {
            if ( TabStripItemClosed != null )
                TabStripItemClosed( this, e );
        }

        /// <summary>
        /// Fires <see cref="MenuItemsLoading"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMenuItemsLoading( HandledEventArgs e ) {
            if ( MenuItemsLoading != null )
                MenuItemsLoading( this, e );
        }
        /// <summary>
        /// Fires <see cref="MenuItemsLoaded"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMenuItemsLoaded( EventArgs e ) {
            if ( MenuItemsLoaded != null )
                MenuItemsLoaded( this, e );
        }

        /// <summary>
        /// Fires <see cref="TabStripItemSelectionChanged"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTabStripItemChanged( TabStripItemChangedEventArgs e ) {
            if ( TabStripItemSelectionChanged != null )
                TabStripItemSelectionChanged( e );
        }

        /// <summary>
        /// Loads menu items based on <see cref="FATabStripItem"/>s currently added
        /// to this control.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMenuItemsLoad( EventArgs e ) {
            menu.RightToLeft = RightToLeft;
            menu.Items.Clear( );

            for ( int i = 0 ; i < Items.Count ; i++ ) {
                FATabStripItem item = Items[ i ];
                if ( !item.Visible )
                    continue;

                ToolStripMenuItem tItem = new ToolStripMenuItem( item.Title );
                tItem.Tag = item;
                tItem.Image = item.Image;
                menu.Items.Add( tItem );
            }

            OnMenuItemsLoaded( EventArgs.Empty );
        }

        #endregion

        #region Overrides

        protected override void OnRightToLeftChanged( EventArgs e ) {
            base.OnRightToLeftChanged( e );
            UpdateLayout( );
            Invalidate( );
        }

        protected override void OnPaint( PaintEventArgs e ) {
            Font = new Font( "Calibri", 11f, FontStyle.Regular );
            SetDefaultSelected( );
            Rectangle borderRc = ClientRectangle;
            borderRc.Width--;
            borderRc.Height--;

            if ( RightToLeft == RightToLeft.No ) {
                DEF_START_POS = 10;
            } else {
                DEF_START_POS = stripButtonRect.Right;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            #region Draw Pages

            for ( int i = 0 ; i < Items.Count ; i++ ) {
                FATabStripItem currentItem = Items[ i ];
                if ( !currentItem.Visible && !DesignMode )
                    continue;

                OnCalcTabPage( e.Graphics, currentItem );
                currentItem.IsDrawn = false;

                if ( !AllowDraw( currentItem ) )
                    continue;

                OnDrawTabPage( e.Graphics, currentItem );
            }
            DEF_START_POS = 10;
            for ( int i = 0 ; i < Items.Count ; i++ ) {
                FATabStripItem currentItem = Items[ i ];
                if ( !currentItem.Visible && !DesignMode )
                    continue;

                OnCalcTabPage( e.Graphics, currentItem );
                currentItem.IsDrawn = false;

                if ( !AllowDraw( currentItem ) )
                    continue;

                OnDrawTabPage( e.Graphics, currentItem );
                if ( SelectedItem == currentItem )
                    break;
            }

            #endregion

            #region Draw UnderPage Line

            if ( RightToLeft == RightToLeft.No ) {
                if ( Items.DrawnCount == 0 || Items.VisibleCount == 0 ) {
                    e.Graphics.DrawLine( new Pen( Color.FromArgb( BackColor.R + 70, BackColor.G + 70, BackColor.B + 70 ) ), new Point( 0, DEF_HEADER_HEIGHT ),
                                        new Point( ClientRectangle.Width, DEF_HEADER_HEIGHT ) );
                } else if ( SelectedItem != null && SelectedItem.IsDrawn ) {
                    Point end = new Point( ( int )SelectedItem.StripRect.Left - 9, DEF_HEADER_HEIGHT );
                    e.Graphics.DrawLine( new Pen( Color.FromArgb( BackColor.R + 70, BackColor.G + 70, BackColor.B + 70 ) ), new Point( 0, DEF_HEADER_HEIGHT ), end );
                    end.X += ( int )SelectedItem.StripRect.Width + 20;
                    e.Graphics.DrawLine( new Pen( Color.FromArgb( BackColor.R + 70, BackColor.G + 70, BackColor.B + 70 ) ), end, new Point( ClientRectangle.Width, DEF_HEADER_HEIGHT ) );
                }
            } else {
                if ( Items.DrawnCount == 0 || Items.VisibleCount == 0 ) {
                    e.Graphics.DrawLine( new Pen( Color.FromArgb( BackColor.R + 70, BackColor.G + 70, BackColor.B + 70 ) ), new Point( 0, DEF_HEADER_HEIGHT ),
                                        new Point( ClientRectangle.Width, DEF_HEADER_HEIGHT ) );
                } else if ( SelectedItem != null && SelectedItem.IsDrawn ) {
                    Point end = new Point( ( int )SelectedItem.StripRect.Left, DEF_HEADER_HEIGHT );
                    e.Graphics.DrawLine( new Pen( Color.FromArgb( BackColor.R + 70, BackColor.G + 70, BackColor.B + 70 ) ), new Point( 0, DEF_HEADER_HEIGHT ), end );
                    end.X += ( int )SelectedItem.StripRect.Width + 20;
                    e.Graphics.DrawLine( new Pen( Color.FromArgb( BackColor.R + 70, BackColor.G + 70, BackColor.B + 70 ) ), end, new Point( ClientRectangle.Width, DEF_HEADER_HEIGHT ) );
                }
            }


            #endregion

            #region Draw Menu and Close Glyphs

            if ( AlwaysShowMenuGlyph && Items.DrawnCount > Items.VisibleCount )
                menuGlyph.DrawGlyph( e.Graphics );

            if ( AlwaysShowClose && ( SelectedItem != null && SelectedItem.CanClose ) )
                closeButton.DrawCross( e.Graphics );

            #endregion
        }

        protected override void OnMouseDown( MouseEventArgs e ) {
            base.OnMouseDown( e );

            if ( e.Button != MouseButtons.Left )
                return;

            HitTestResult result = HitTest( e.Location );
            if ( result == HitTestResult.MenuGlyph ) {
                HandledEventArgs args = new HandledEventArgs( false );
                OnMenuItemsLoading( args );

                if ( !args.Handled )
                    OnMenuItemsLoad( EventArgs.Empty );

                ShowMenu( );
            } else if ( result == HitTestResult.TabItem ) {
                FATabStripItem item = GetTabItemByPoint( e.Location );
                if ( item != null )
                    SelectedItem = item;
            }

            Invalidate( );
        }

        protected override void OnMouseMove( MouseEventArgs e ) {
            base.OnMouseMove( e );

            if ( menuGlyph.Bounds.Contains( e.Location ) ) {
                menuGlyph.IsMouseOver = true;
                Invalidate( menuGlyph.Bounds );
            } else {
                if ( menuGlyph.IsMouseOver && !menuOpen ) {
                    menuGlyph.IsMouseOver = false;
                    Invalidate( menuGlyph.Bounds );
                }
            }

            if ( closeButton.Bounds.Contains( e.Location ) ) {
                closeButton.IsMouseOver = true;
                Invalidate( closeButton.Bounds );
            } else {
                if ( closeButton.IsMouseOver ) {
                    closeButton.IsMouseOver = false;
                    Invalidate( closeButton.Bounds );
                }
            }
        }

        protected override void OnMouseUp( MouseEventArgs e ) {
            base.OnMouseUp( e );

            if ( e.Button != MouseButtons.Left )
                return;

            HitTestResult result = HitTest( e.Location );

            if ( result == HitTestResult.CloseButton && alwaysShowClose ) {
                if ( SelectedItem != null ) {
                    TabStripItemClosingEventArgs args = new TabStripItemClosingEventArgs( SelectedItem );
                    OnTabStripItemClosing( args );
                    if ( !args.Cancel && SelectedItem.CanClose ) {
                        RemoveTab( SelectedItem );
                        OnTabStripItemClosed( EventArgs.Empty );
                    }
                }
            }
        }

        protected override void OnMouseLeave( EventArgs e ) {
            base.OnMouseLeave( e );
            menuGlyph.IsMouseOver = false;
            Invalidate( menuGlyph.Bounds );

            closeButton.IsMouseOver = false;
            Invalidate( closeButton.Bounds );
        }

        protected override void OnSizeChanged( EventArgs e ) {
            base.OnSizeChanged( e );
            if ( isIniting )
                return;

            UpdateLayout( );
        }

        #endregion

        #region public

        public bool AllowDraw( FATabStripItem item ) {
            bool result = true;

            if ( RightToLeft == RightToLeft.No ) {
                if ( item.StripRect.Right >= stripButtonRect.Width )
                    result = false;
            } else {
                if ( item.StripRect.Left <= stripButtonRect.Left )
                    return false;
            }

            return result;
        }

        public void SetDefaultSelected( ) {
            if ( selectedItem == null && Items.Count > 0 )
                SelectedItem = Items[ 0 ];

            for ( int i = 0 ; i < Items.Count ; i++ ) {
                FATabStripItem itm = Items[ i ];
                itm.Dock = DockStyle.Fill;
            }
        }

        public void OnMenuItemClicked( object sender, ToolStripItemClickedEventArgs e ) {
            FATabStripItem clickedItem = ( FATabStripItem )e.ClickedItem.Tag;
            SelectedItem = clickedItem;
        }

        public void OnMenuVisibleChanged( object sender, EventArgs e ) {
            if ( menu.Visible == false ) {
                menuOpen = false;
            }
        }

        public void OnCalcTabPage( Graphics g, FATabStripItem currentItem ) {
            Font currentFont = Font;
            if ( currentItem == SelectedItem )
                currentFont = new Font( Font, FontStyle.Bold );

            SizeF textSize = g.MeasureString( currentItem.Title, currentFont, new SizeF( 200, 10 ), sf );
            textSize.Width += 20;

            if ( RightToLeft == RightToLeft.No ) {
                RectangleF buttonRect = new RectangleF( DEF_START_POS, 3, textSize.Width, DEF_HEADER_HEIGHT );
                currentItem.StripRect = buttonRect;
                DEF_START_POS += ( int )textSize.Width + DEF_GLYPH_WIDTH / 2;
            } else {
                RectangleF buttonRect = new RectangleF( DEF_START_POS - textSize.Width + 1, 3, textSize.Width - 1, DEF_HEADER_HEIGHT );
                currentItem.StripRect = buttonRect;
                DEF_START_POS -= ( int )textSize.Width;
            }
        }

        public void OnDrawTabPage( Graphics g, FATabStripItem currentItem ) {
            bool isFirstTab = Items.IndexOf( currentItem ) == 0;
            Font currentFont = Font;

            SizeF textSize = g.MeasureString( currentItem.Title + ( currentItem.Saved ? "" : "*" ), currentFont, new SizeF( 200, 10 ), sf );
            textSize.Width += 40;
            RectangleF buttonRect = currentItem.StripRect;
            buttonRect.Height = DEF_HEADER_HEIGHT;
            buttonRect.Width += DEF_GLYPH_WIDTH / 2;
            currentItem.StripRect = buttonRect;

            GraphicsPath path = new GraphicsPath( );
            LinearGradientBrush brush;
            int mtop = 3;

            #region Draw Not Right-To-Left Tab

            if ( RightToLeft == RightToLeft.No ) {
                path.AddLine( buttonRect.Left - 10, buttonRect.Bottom, buttonRect.Left - 3, buttonRect.Top + 4 );
                path.AddLine( buttonRect.Left - 3, buttonRect.Top + 4, buttonRect.Left, buttonRect.Top );

                path.AddLine( buttonRect.Right, buttonRect.Top, buttonRect.Right + 3, buttonRect.Top + 4 );
                path.AddLine( buttonRect.Right + 3, buttonRect.Top + 4, buttonRect.Right + 10, buttonRect.Bottom );
                path.CloseFigure( );
                if ( currentItem == SelectedItem ) {
                    brush = new LinearGradientBrush( buttonRect, Color.FromArgb( BackColor.R + 15, BackColor.G + 15, BackColor.B + 15 ),
                        Color.FromArgb( 0, 0, 0, 0 ), LinearGradientMode.Vertical );
                } else {
                    brush = new LinearGradientBrush( buttonRect, Color.FromArgb( BackColor.R + 7, BackColor.G + 7, BackColor.B + 7 ),
                        Color.FromArgb( 0, 0, 0, 0 ), LinearGradientMode.Vertical );
                }
                g.FillPath( brush, path );
                g.DrawPath( new Pen( Color.FromArgb( BackColor.R + 70, BackColor.G + 70, BackColor.B + 70 ) ), path );

                if ( currentItem == SelectedItem ) {
                    g.DrawLine( new Pen( new SolidBrush( currentItem.BackColor ), 1 ), buttonRect.Left - 10, buttonRect.Height + 1,
                               buttonRect.Left + buttonRect.Width + 10, buttonRect.Height + 1 );
                }

                PointF textLoc = new PointF( buttonRect.Left + buttonRect.Height - 4, buttonRect.Top + ( buttonRect.Height / 2 ) - ( textSize.Height / 2 ) - 3 );
                RectangleF textRect = buttonRect;
                textRect.Location = textLoc;
                textRect.Width = buttonRect.Width - ( textRect.Left - buttonRect.Left ) - 4;
                textRect.Height = textSize.Height + currentFont.Size / 2;
                textRect.Y -= 4;
                g.DrawString( currentItem.Title + ( currentItem.Saved ? "" : "*" ), currentFont, new SolidBrush( ForeColor ), textRect, sf );
            }

            #endregion

            #region Draw Right-To-Left Tab

            if ( RightToLeft == RightToLeft.Yes ) {
                if ( currentItem == SelectedItem || isFirstTab ) {
                    path.AddLine( buttonRect.Right + 10, buttonRect.Bottom - 1,
                                 buttonRect.Right - ( buttonRect.Height / 2 ) + 4, mtop + 4 );
                } else {
                    path.AddLine( buttonRect.Right, buttonRect.Bottom - 1, buttonRect.Right,
                                 buttonRect.Bottom - ( buttonRect.Height / 2 ) - 2 );
                    path.AddLine( buttonRect.Right, buttonRect.Bottom - ( buttonRect.Height / 2 ) - 3,
                                 buttonRect.Right - ( buttonRect.Height / 2 ) + 4, mtop + 3 );
                }

                path.AddLine( buttonRect.Right - ( buttonRect.Height / 2 ) - 2, mtop, buttonRect.Left + 3, mtop );
                path.AddLine( buttonRect.Left, mtop + 2, buttonRect.Left, buttonRect.Bottom - 1 );
                path.AddLine( buttonRect.Left + 4, buttonRect.Bottom - 1, buttonRect.Right, buttonRect.Bottom - 1 );
                path.CloseFigure( );

                if ( currentItem == SelectedItem ) {
                    brush =
                        new LinearGradientBrush( buttonRect, SystemColors.ControlLightLight, SystemColors.Window,
                                                LinearGradientMode.Vertical );
                } else {
                    brush =
                        new LinearGradientBrush( buttonRect, SystemColors.ControlLightLight, SystemColors.Control,
                                                LinearGradientMode.Vertical );
                }

                g.FillPath( brush, path );
                g.DrawPath( SystemPens.ControlDark, path );

                if ( currentItem == SelectedItem ) {
                    g.DrawLine( new Pen( brush ), buttonRect.Right + 9, buttonRect.Height + 2,
                               buttonRect.Right - buttonRect.Width + 1, buttonRect.Height + 2 );
                }

                PointF textLoc = new PointF( buttonRect.Left + 2, buttonRect.Top + ( buttonRect.Height / 2 ) - ( textSize.Height / 2 ) - 2 );
                RectangleF textRect = buttonRect;
                textRect.Location = textLoc;
                textRect.Width = buttonRect.Width - ( textRect.Left - buttonRect.Left ) - 10;
                textRect.Height = textSize.Height + currentFont.Size / 2;

                if ( currentItem == SelectedItem ) {
                    textRect.Y -= 1;
                    g.DrawString( currentItem.Title + ( currentItem.Saved ? "" : "*" ), currentFont, new SolidBrush( ForeColor ), textRect, sf );
                } else {
                    g.DrawString( currentItem.Title + ( currentItem.Saved ? "" : "*" ), currentFont, new SolidBrush( ForeColor ), textRect, sf );
                }

                //g.FillRectangle(Brushes.Red, textRect);
            }

            #endregion

            currentItem.IsDrawn = true;
        }

        public void UpdateLayout( ) {
            if ( RightToLeft == RightToLeft.No ) {
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags |= StringFormatFlags.NoWrap;
                sf.FormatFlags &= StringFormatFlags.DirectionRightToLeft;

                stripButtonRect = new Rectangle( 0, 0, ClientSize.Width - DEF_GLYPH_WIDTH - 2, 10 );
                menuGlyph.Bounds = new Rectangle( ClientSize.Width - DEF_GLYPH_WIDTH, 2, 16, 16 );
                closeButton.Bounds = new Rectangle( ClientSize.Width - 20, 2, 16, 16 );
            } else {
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags |= StringFormatFlags.NoWrap;
                sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;

                stripButtonRect = new Rectangle( DEF_GLYPH_WIDTH + 2, 0, ClientSize.Width - DEF_GLYPH_WIDTH - 15, 10 );
                closeButton.Bounds = new Rectangle( 4, 2, 16, 16 ); //ClientSize.Width - DEF_GLYPH_WIDTH, 2, 16, 16);
                menuGlyph.Bounds = new Rectangle( 20 + 4, 2, 16, 16 ); //this.ClientSize.Width - 20, 2, 16, 16);
            }

            DockPadding.Top = DEF_HEADER_HEIGHT + 1;
            DockPadding.Bottom = 1;
            DockPadding.Right = 1;
            DockPadding.Left = 1;
        }

        public void OnCollectionChanged( object sender, CollectionChangeEventArgs e ) {
            FATabStripItem itm = ( FATabStripItem )e.Element;

            if ( e.Action == CollectionChangeAction.Add ) {
                Controls.Add( itm );
                OnTabStripItemChanged( new TabStripItemChangedEventArgs( itm, FATabStripItemChangeTypes.Added ) );
            } else if ( e.Action == CollectionChangeAction.Remove ) {
                Controls.Remove( itm );
                OnTabStripItemChanged( new TabStripItemChangedEventArgs( itm, FATabStripItemChangeTypes.Removed ) );
            } else {
                OnTabStripItemChanged( new TabStripItemChangedEventArgs( itm, FATabStripItemChangeTypes.Changed ) );
            }

            UpdateLayout( );
            Invalidate( );
        }

        #endregion

        #endregion

        #region Ctor

        public FATabStrip( ) {
            BeginInit( );
            SetStyle( ControlStyles.ContainerControl, true );
            SetStyle( ControlStyles.UserPaint, true );
            SetStyle( ControlStyles.ResizeRedraw, true );
            SetStyle( ControlStyles.AllPaintingInWmPaint, true );
            SetStyle( ControlStyles.OptimizedDoubleBuffer, true );
            SetStyle( ControlStyles.Selectable, true );

            items = new FATabStripItemCollection( );
            items.CollectionChanged += new CollectionChangeEventHandler( OnCollectionChanged );
            base.Size = new Size( 350, 200 );

            menu = new ContextMenuStrip( );
            menu.Renderer = ToolStripRenderer;
            menu.ItemClicked += new ToolStripItemClickedEventHandler( OnMenuItemClicked );
            menu.VisibleChanged += new EventHandler( OnMenuVisibleChanged );

            menuGlyph = new FATabStripMenuGlyph( ToolStripRenderer );
            closeButton = new FATabStripCloseButton( ToolStripRenderer );
            defaultFont = new Font( "Calibri", 11f, FontStyle.Regular );
            Font = defaultFont;
            sf = new StringFormat( );

            EndInit( );

            UpdateLayout( );
            if ( Items.Count > 0 )
                SelectedItem = Items[ 0 ];
        }

        #endregion

        #region Props

        [DefaultValue( null )]
        [RefreshProperties( RefreshProperties.All )]
        public FATabStripItem SelectedItem {
            get {
                return selectedItem;
            }
            set {
                if ( selectedItem == value )
                    return;

                if ( value == null && Items.Count > 0 ) {
                    FATabStripItem itm = Items[ 0 ];
                    if ( itm.Visible ) {
                        selectedItem = itm;
                        selectedItem.Selected = true;
                        selectedItem.Dock = DockStyle.Fill;
                    }
                } else {
                    selectedItem = value;
                }

                foreach ( FATabStripItem itm in Items ) {
                    if ( itm == selectedItem ) {
                        SelectItem( itm );
                        itm.Dock = DockStyle.Fill;
                        itm.Show( );
                    } else {
                        UnSelectItem( itm );
                        itm.Hide( );
                    }
                }

                SelectItem( selectedItem );
                Invalidate( );

                if ( !selectedItem.IsDrawn )
                    Invalidate( );

                OnTabStripItemChanged(
                    new TabStripItemChangedEventArgs( selectedItem, FATabStripItemChangeTypes.SelectionChanged ) );
            }
        }

        [DefaultValue( true )]
        public bool AlwaysShowMenuGlyph {
            get {
                return alwaysShowMenuGlyph;
            }
            set {
                if ( alwaysShowMenuGlyph == value )
                    return;

                alwaysShowMenuGlyph = value;
                Invalidate( );
            }
        }

        [DefaultValue( true )]
        public bool AlwaysShowClose {
            get {
                return alwaysShowClose;
            }
            set {
                if ( alwaysShowClose == value )
                    return;

                alwaysShowClose = value;
                Invalidate( );
            }
        }

        [DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
        public FATabStripItemCollection Items {
            get {
                return items;
            }
        }

        [DefaultValue( typeof( Size ), "350,200" )]
        public new Size Size {
            get {
                return base.Size;
            }
            set {
                if ( base.Size == value )
                    return;

                base.Size = value;
                UpdateLayout( );
            }
        }

        /// <summary>
        /// DesignerSerializationVisibility
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public new ControlCollection Controls {
            get {
                return base.Controls;
            }
        }

        #endregion

        #region ShouldSerialize

        public bool ShouldSerializeFont( ) {
            return Font != null && !Font.Equals( defaultFont );
        }

        public bool ShouldSerializeSelectedItem( ) {
            return true;
        }

        public bool ShouldSerializeItems( ) {
            return items.Count > 0;
        }

        public new void ResetFont( ) {
            Font = defaultFont;
        }

        #endregion

        #region ISupportInitialize Members

        public void BeginInit( ) {
            isIniting = true;
        }

        public void EndInit( ) {
            isIniting = false;
        }

        #endregion

        #region IDisposable

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        protected override void Dispose( bool disposing ) {
            if ( disposing ) {
                items.CollectionChanged -= new CollectionChangeEventHandler( OnCollectionChanged );
                menu.ItemClicked -= new ToolStripItemClickedEventHandler( OnMenuItemClicked );
                menu.VisibleChanged -= new EventHandler( OnMenuVisibleChanged );
                try {
                    foreach ( FATabStripItem item in items ) {
                        if ( item != null && !item.IsDisposed )
                            item.Dispose( );
                    }
                } catch {
                }
                if ( menu != null && !menu.IsDisposed )
                    menu.Dispose( );

                if ( sf != null )
                    sf.Dispose( );
            }

            base.Dispose( disposing );
        }

        #endregion
    }
    #region TabStripItemClosingEventArgs
    public class FATabStripDesigner : ParentControlDesigner {
        #region Fields

        IComponentChangeService changeService;

        #endregion

        #region Initialize & Dispose

        public override void Initialize( System.ComponentModel.IComponent component ) {
            base.Initialize( component );

            //Design services
            changeService = ( IComponentChangeService )GetService( typeof( IComponentChangeService ) );

            //Bind design events
            changeService.ComponentRemoving += new ComponentEventHandler( OnRemoving );

            Verbs.Add( new DesignerVerb( "Add TabStrip", new EventHandler( OnAddTabStrip ) ) );
            Verbs.Add( new DesignerVerb( "Remove TabStrip", new EventHandler( OnRemoveTabStrip ) ) );
        }

        protected override void Dispose( bool disposing ) {
            changeService.ComponentRemoving -= new ComponentEventHandler( OnRemoving );

            base.Dispose( disposing );
        }

        #endregion

        #region public Methods

        public void OnRemoving( object sender, ComponentEventArgs e ) {
            IDesignerHost host = ( IDesignerHost )GetService( typeof( IDesignerHost ) );

            //Removing a button
            if ( e.Component is FATabStripItem ) {
                FATabStripItem itm = e.Component as FATabStripItem;
                if ( Control.Items.Contains( itm ) ) {
                    changeService.OnComponentChanging( Control, null );
                    Control.RemoveTab( itm );
                    changeService.OnComponentChanged( Control, null, null, null );
                    return;
                }
            }

            if ( e.Component is FATabStrip ) {
                for ( int i = Control.Items.Count - 1 ; i >= 0 ; i-- ) {
                    FATabStripItem itm = Control.Items[ i ];
                    changeService.OnComponentChanging( Control, null );
                    Control.RemoveTab( itm );
                    host.DestroyComponent( itm );
                    changeService.OnComponentChanged( Control, null, null, null );
                }
            }
        }

        public void OnAddTabStrip( object sender, EventArgs e ) {
            IDesignerHost host = ( IDesignerHost )GetService( typeof( IDesignerHost ) );
            DesignerTransaction transaction = host.CreateTransaction( "Add TabStrip" );
            FATabStripItem itm = ( FATabStripItem )host.CreateComponent( typeof( FATabStripItem ) );
            changeService.OnComponentChanging( Control, null );
            Control.AddTab( itm );
            int indx = Control.Items.IndexOf( itm ) + 1;
            itm.Title = "TabStrip Page " + indx.ToString( );
            Control.SelectItem( itm );
            changeService.OnComponentChanged( Control, null, null, null );
            transaction.Commit( );
        }

        public void OnRemoveTabStrip( object sender, EventArgs e ) {
            IDesignerHost host = ( IDesignerHost )GetService( typeof( IDesignerHost ) );
            DesignerTransaction transaction = host.CreateTransaction( "Remove Button" );
            changeService.OnComponentChanging( Control, null );
            FATabStripItem itm = Control.Items[ Control.Items.Count - 1 ];
            Control.UnSelectItem( itm );
            Control.Items.Remove( itm );
            changeService.OnComponentChanged( Control, null, null, null );
            transaction.Commit( );
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Check HitTest on <see cref="FATabStrip"/> control and
        /// let the user click on close and menu buttons.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected override bool GetHitTest( Point point ) {
            HitTestResult result = Control.HitTest( point );
            if ( result == HitTestResult.CloseButton || result == HitTestResult.MenuGlyph )
                return true;

            return false;
        }

        protected override void PreFilterProperties( IDictionary properties ) {
            base.PreFilterProperties( properties );

            properties.Remove( "DockPadding" );
            properties.Remove( "DrawGrid" );
            properties.Remove( "Margin" );
            properties.Remove( "Padding" );
            properties.Remove( "BorderStyle" );
            properties.Remove( "ForeColor" );
            properties.Remove( "BackColor" );
            properties.Remove( "BackgroundImage" );
            properties.Remove( "BackgroundImageLayout" );
            properties.Remove( "GridSize" );
            properties.Remove( "ImeMode" );
        }

        protected override void WndProc( ref Message msg ) {
            if ( msg.Msg == 0x201 ) {
                Point pt = Control.PointToClient( Cursor.Position );
                FATabStripItem itm = Control.GetTabItemByPoint( pt );
                if ( itm != null ) {
                    Control.SelectedItem = itm;
                    ArrayList selection = new ArrayList( );
                    selection.Add( itm );
                    ISelectionService selectionService = ( ISelectionService )GetService( typeof( ISelectionService ) );
                    selectionService.SetSelectedComponents( selection );
                }
            }

            base.WndProc( ref msg );
        }

        public override ICollection AssociatedComponents {
            get {
                return Control.Items;
            }
        }

        public new virtual FATabStrip Control {
            get {
                return base.Control as FATabStrip;
            }
        }

        #endregion
    }
    public class TabStripItemClosingEventArgs : EventArgs {
        public TabStripItemClosingEventArgs( FATabStripItem item ) {
            _item = item;
        }

        public bool _cancel = false;
        public FATabStripItem _item;

        public FATabStripItem Item {
            get {
                return _item;
            }
            set {
                _item = value;
            }
        }

        public bool Cancel {
            get {
                return _cancel;
            }
            set {
                _cancel = value;
            }
        }

    }

    #endregion
    public interface ICaptionSupport {
        string Caption {
            get;
        }
    }
    public class FATabStripItemDesigner : ParentControlDesigner {
        #region Fields

        FATabStripItem TabStrip;

        #endregion

        #region Init & Dispose

        public override void Initialize( IComponent component ) {
            base.Initialize( component );
            TabStrip = component as FATabStripItem;
        }

        #endregion

        #region Overrides

        protected override void PreFilterProperties( System.Collections.IDictionary properties ) {
            base.PreFilterProperties( properties );

            properties.Remove( "Dock" );
            properties.Remove( "AutoScroll" );
            properties.Remove( "AutoScrollMargin" );
            properties.Remove( "AutoScrollMinSize" );
            properties.Remove( "DockPadding" );
            properties.Remove( "DrawGrid" );
            properties.Remove( "Font" );
            properties.Remove( "Padding" );
            properties.Remove( "MinimumSize" );
            properties.Remove( "MaximumSize" );
            properties.Remove( "Margin" );
            properties.Remove( "ForeColor" );
            properties.Remove( "BackColor" );
            properties.Remove( "BackgroundImage" );
            properties.Remove( "BackgroundImageLayout" );
            properties.Remove( "RightToLeft" );
            properties.Remove( "GridSize" );
            properties.Remove( "ImeMode" );
            properties.Remove( "BorderStyle" );
            properties.Remove( "AutoSize" );
            properties.Remove( "AutoSizeMode" );
            properties.Remove( "Location" );
        }

        public override SelectionRules SelectionRules {
            get {
                return 0;
            }
        }

        public override bool CanBeParentedTo( IDesigner parentDesigner ) {
            return ( parentDesigner.Component is FATabStrip );
        }

        protected override void OnPaintAdornments( PaintEventArgs pe ) {
            if ( TabStrip != null ) {
                using ( Pen p = new Pen( SystemColors.ControlDark ) ) {
                    p.DashStyle = DashStyle.Dash;
                    pe.Graphics.DrawRectangle( p, 0, 0, TabStrip.Width - 1, TabStrip.Height - 1 );
                }
            }
        }

        #endregion
    }
    #region TabStripItemChangedEventArgs

    public class TabStripItemChangedEventArgs : EventArgs {
        FATabStripItem itm;
        FATabStripItemChangeTypes changeType;

        public TabStripItemChangedEventArgs( FATabStripItem item, FATabStripItemChangeTypes type ) {
            changeType = type;
            itm = item;
        }

        public FATabStripItemChangeTypes ChangeType {
            get {
                return changeType;
            }
        }

        public FATabStripItem Item {
            get {
                return itm;
            }
        }
    }

    #endregion
    /// <summary>
    /// Represents the method that will handle the event that has no data.
    /// </summary>
    public delegate void CollectionClear( );

    /// <summary>
    /// Represents the method that will handle the event that has item data.
    /// </summary>
    public delegate void CollectionChange( int index, object value );

    /// <summary>
    /// Extend collection base class by generating change events.
    /// </summary>
    public abstract class CollectionWithEvents : CollectionBase {
        // Instance fields
        public int _suspendCount;

        /// <summary>
        /// Occurs just before the collection contents are cleared.
        /// </summary>
        [Browsable( false )]
        public event CollectionClear Clearing;

        /// <summary>
        /// Occurs just after the collection contents are cleared.
        /// </summary>
        [Browsable( false )]
        public event CollectionClear Cleared;

        /// <summary>
        /// Occurs just before an item is added to the collection.
        /// </summary>
        [Browsable( false )]
        public event CollectionChange Inserting;

        /// <summary>
        /// Occurs just after an item has been added to the collection.
        /// </summary>
        [Browsable( false )]
        public event CollectionChange Inserted;

        /// <summary>
        /// Occurs just before an item is removed from the collection.
        /// </summary>
        [Browsable( false )]
        public event CollectionChange Removing;

        /// <summary>
        /// Occurs just after an item has been removed from the collection.
        /// </summary>
        [Browsable( false )]
        public event CollectionChange Removed;

        /// <summary>
        /// Initializes DrawTab new instance of the CollectionWithEvents class.
        /// </summary>
        public CollectionWithEvents( ) {
            // Default to not suspended
            _suspendCount = 0;
        }

        /// <summary>
        /// Do not generate change events until resumed.
        /// </summary>
        public void SuspendEvents( ) {
            _suspendCount++;
        }

        /// <summary>
        /// Safe to resume change events.
        /// </summary>
        public void ResumeEvents( ) {
            --_suspendCount;
        }

        /// <summary>
        /// Gets DrawTab value indicating if events are currently suspended.
        /// </summary>
        [Browsable( false )]
        public bool IsSuspended {
            get {
                return ( _suspendCount > 0 );
            }
        }

        /// <summary>
        /// Raises the Clearing event when not suspended.
        /// </summary>
        protected override void OnClear( ) {
            if ( !IsSuspended ) {
                // Any attached event handlers?
                if ( Clearing != null )
                    Clearing( );
            }
        }

        /// <summary>
        /// Raises the Cleared event when not suspended.
        /// </summary>
        protected override void OnClearComplete( ) {
            if ( !IsSuspended ) {
                // Any attached event handlers?
                if ( Cleared != null )
                    Cleared( );
            }
        }

        /// <summary>
        /// Raises the Inserting event when not suspended.
        /// </summary>
        /// <param name="index">Index of object being inserted.</param>
        /// <param name="value">The object that is being inserted.</param>
        protected override void OnInsert( int index, object value ) {
            if ( !IsSuspended ) {
                // Any attached event handlers?
                if ( Inserting != null )
                    Inserting( index, value );
            }
        }

        /// <summary>
        /// Raises the Inserted event when not suspended.
        /// </summary>
        /// <param name="index">Index of inserted object.</param>
        /// <param name="value">The object that has been inserted.</param>
        protected override void OnInsertComplete( int index, object value ) {
            if ( !IsSuspended ) {
                // Any attached event handlers?
                if ( Inserted != null )
                    Inserted( index, value );
            }
        }

        /// <summary>
        /// Raises the Removing event when not suspended.
        /// </summary>
        /// <param name="index">Index of object being removed.</param>
        /// <param name="value">The object that is being removed.</param>
        protected override void OnRemove( int index, object value ) {
            if ( !IsSuspended ) {
                // Any attached event handlers?
                if ( Removing != null )
                    Removing( index, value );
            }
        }

        /// <summary>
        /// Raises the Removed event when not suspended.
        /// </summary>
        /// <param name="index">Index of removed object.</param>
        /// <param name="value">The object that has been removed.</param>
        protected override void OnRemoveComplete( int index, object value ) {
            if ( !IsSuspended ) {
                // Any attached event handlers?
                if ( Removed != null )
                    Removed( index, value );
            }
        }

        /// <summary>
        /// Returns the index of the first occurrence of DrawTab value.
        /// </summary>
        /// <param name="value">The object to locate.</param>
        /// <returns>Index of object; otherwise -1</returns>
        protected int IndexOf( object value ) {
            // Find the 0 based index of the requested entry
            return List.IndexOf( value );
        }
    }
    public delegate void TabStripItemChangedHandler( TabStripItemChangedEventArgs e );
    public delegate void TabStripItemClosingHandler( TabStripItemClosingEventArgs e );
    #endregion
    #region KeyboardHook
    public sealed class KeyboardHook : IDisposable {
        [DllImport( "user32.dll" )]
        private static extern bool RegisterHotKey( IntPtr hWnd, int id, uint fsModifiers, uint vk );
        [DllImport( "user32.dll" )]
        private static extern bool UnregisterHotKey( IntPtr hWnd, int id );
        private class Window : NativeWindow, IDisposable {
            private static int WM_HOTKEY = 0x0312;
            public Window( ) {
                this.CreateHandle( new CreateParams( ) );
            }
            protected override void WndProc( ref Message m ) {
                base.WndProc( ref m );
                if ( m.Msg == WM_HOTKEY ) {
                    Keys key = ( Keys )( ( ( int )m.LParam >> 16 ) & 0xFFFF );
                    ModifierKeys modifier = ( ModifierKeys )( ( int )m.LParam & 0xFFFF );
                    if ( KeyPressed != null )
                        KeyPressed( this, new KeyPressedEventArgs( modifier, key ) );
                }
            }
            public event EventHandler<KeyPressedEventArgs> KeyPressed;
            public void Dispose( ) {
                this.DestroyHandle( );
            }
        }
        private Window _window = new Window( );
        private int _currentId;
        public KeyboardHook( ) {
            _window.KeyPressed += delegate( object sender, KeyPressedEventArgs args ) {
                if ( KeyPressed != null )
                    KeyPressed( this, args );
            };
        }
        public void RegisterHotKey( ModifierKeys modifier, Keys key ) {
            _currentId = _currentId + 1;
            if ( !RegisterHotKey( _window.Handle, _currentId, ( uint )modifier, ( uint )key ) )
                throw new InvalidOperationException( "Couldn’t register the hot key." );
        }
        public event EventHandler<KeyPressedEventArgs> KeyPressed;
        public void Dispose( ) {
            for ( int i = _currentId ; i > 0 ; i-- ) {
                UnregisterHotKey( _window.Handle, i );
            }
            _window.Dispose( );
        }
    }

    public class KeyPressedEventArgs : EventArgs {
        private ModifierKeys _modifier;
        private Keys _key;
        internal KeyPressedEventArgs( ModifierKeys modifier, Keys key ) {
            _modifier = modifier;
            _key = key;
        }
        public ModifierKeys Modifier {
            get {
                return _modifier;
            }
        }
        public Keys Key {
            get {
                return _key;
            }
        }
    }
    [Flags]
    public enum ModifierKeys : uint {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
    #endregion
}
