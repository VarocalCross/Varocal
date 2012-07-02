using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VarocalCross {
    #region Token Category
    public class Tokenizer {
        protected string Operators = "+-*/|&^,()[]{}.;<>!%=", linTok = " \r\n";
        Dictionary<string, Token> dst = new Dictionary<string, Token>( );
        Dictionary<string, string> _keys = new Dictionary<string, string>( );
        public Dictionary<string, Token> AutomaticMap {
            get {
                return dst;
            }
        }
        public List<String> Keywords {
            get;
            private set;
        }
        public void AddOperators( params string[ ] operators ) {
            foreach ( string ope in operators )
                AutomaticMap.Add( ope, new Token( ope, TokenType.Symbol ) );
        }
        public void AddKeyword( string keyword ) {
            _keys.Add( keyword, keyword );
            Keywords.Add( keyword );
        }
        public void AddKeywords( string[ ] keywords ) {
            foreach ( string key in keywords )
                AddKeyword( key );
        }
        public void AddKeyword( string keyword, string totext ) {
            _keys.Add( keyword, totext );
            Keywords.Add( keyword );
        }
        public bool ListingComments {
            get;
            set;
        }
        public List<Token> Tokenize( string Code, int LineNum = 1 ) {
            List<Token> R = new List<Token>( 2 );
            int i = 0, c = Code.Length, linepos = -1;
            while ( i < c ) {
                #region Clean Spaces Line and Setup
                if ( Basic.StringHas( linTok, Code[ i ] ) ) {
                    if ( Code[ i ] == '\n' ) {
                        linepos = i;
                        LineNum++;
                    }
                    i++;
                    if ( i == c )
                        return R;
                    while ( Basic.StringHas( linTok, Code[ i ] ) ) {
                        if ( Code[ i ] == '\n' ) {
                            linepos = i;
                            LineNum++;
                        }
                        i++;
                        if ( i == c )
                            return R;
                    }
                }
                int start = i, end = 0;
                TokenType tt = TokenType.Unknown;
                #endregion
                #region Comments
                if ( c - i >= 2 && Code[ i ] == '/' ) {
                    if ( Code[ i + 1 ] == '/' ) {
                        tt = TokenType.Comment;
                        i += 2;
                        end = 2;
                        if ( c - i >= 1 )
                            while ( Code[ i ] != '\r' ) {
                                i++;
                                end++;
                                if ( c - i == 0 )
                                    break;
                            }
                        goto Tokening;
                    } else if ( Code[ i + 1 ] == '*' ) {
                        tt = TokenType.Comment;
                        i += 2;
                        end = 2;
                        if ( c - i >= 1 )
                            if ( c - i == 1 ) {
                                end = 3;
                                goto Tokening;
                            } else {
                                while ( Code[ i ] != '*' && Code[ i + 1 ] != '/' ) {
                                    i++;
                                    end++;
                                    if ( c - i <= 1 ) {
                                        if ( c - i == 1 ) {
                                            i++;
                                            end++;
                                        }
                                        goto Tokening;
                                    }
                                }
                                i += 2;
                                end += 2;
                                goto Tokening;
                            }
                        goto Tokening;
                    }
                }
                #endregion
                #region Fast Calls
                bool is_continue = false;
                foreach ( KeyValuePair<string, Token> kvp in dst ) {
                    if ( Basic.StartsWithIndex( Code, i, kvp.Key ) ) {
                        end = kvp.Key.Length;
                        i += kvp.Key.Length;
                        string _text = Code.Substring( start, end ), _ntext = kvp.Value.Value;
                        linepos = Code.LastIndexOf( '\n', start );
                        R.Add( new Token( _ntext, kvp.Value.Type, LineNum, start - linepos, _ntext.Length ) );
                        LineNum += Basic.Count( _text, '\n' );
                        is_continue = true;
                        //LineNum += Basic.Count( kvp.Key, '\n' );
                        //int pos2 = Code.LastIndexOf( '\n', i );
                        //if ( pos2 == -1 )
                        //    pos2 = 0;
                        //pos2 += kvp.Key.Length;
                        //R.Add( new Token( kvp.Value.Value, kvp.Value.Type, LineNum, pos2 ) );
                        //is_continue = true;
                        //i += kvp.Key.Length;
                    }
                }
                if ( is_continue )
                    continue;
                #endregion
                #region Key\Words
                if ( ( Code[ i ] >= 'a' && Code[ i ] <= 'z' ) || ( Code[ i ] >= 'A' && Code[ i ] <= 'Z' ) || Code[ i ] == '_' ) {
                    tt = TokenType.Word;
                    i++;
                    end++;
                    if ( c - i == 0 )
                        goto Tokening;
                    while ( ( Code[ i ] >= 'a' && Code[ i ] <= 'z' ) || ( Code[ i ] >= 'A' && Code[ i ] <= 'Z' ) || ( Code[ i ] >= '0' && Code[ i ] <= '9' ) || Code[ i ] == '_' ) {
                        i++;
                        end++;
                        if ( c - i == 0 )
                            break;
                    }
                    goto Tokening;
                }
                #endregion
                #region Integers
                if ( Code[ i ] >= '0' && Code[ i ] <= '9' ) {
                    tt = TokenType.Integer;
                    i++;
                    end++;
                    if ( c - i == 0 )
                        goto Tokening;
                    while ( Code[ i ] >= '0' && Code[ i ] <= '9' ) {
                        i++;
                        end++;
                        if ( c - i == 0 )
                            break;
                    }
                    goto Tokening;
                }
                #endregion
                #region Strings
                if ( Code[ i ] == '"' ) {
                    tt = TokenType.String;
                    i++;
                    end++;
                    if ( i - c != 0 ) {
                        while ( Code[ i ] != '"' ) {
                            i++;
                            end++;
                            if ( c - i == 0 )
                                goto Tokening;
                        }
                        i++;
                        end++;
                        goto Tokening;
                    }
                    goto Tokening;
                }
                #endregion
                #region Operators
                if ( Basic.StringHas( Operators, Code[ i ] ) ) {
                    tt = TokenType.Symbol;
                    i++;
                    end++;
                    goto Tokening;
                }
                #endregion
                end = 1;
                i++;
            Tokening:
                if ( tt == TokenType.Comment && !ListingComments )
                    continue;
                string text = Code.Substring( start, end ), ntext;
                ntext = text;
                if ( tt == TokenType.Word ) {
                    foreach ( KeyValuePair<string, string> keyword in _keys )
                        if ( keyword.Key == text ) {
                            tt = TokenType.Keyword;
                            ntext = keyword.Value;
                            break;
                        }
                }
                linepos = Code.LastIndexOf( '\n', start );
                R.Add( new Token( ntext, tt, LineNum, start - linepos, ntext.Length ) );
                LineNum += Basic.Count( text, '\n' );
            }
            return R;
        }
        public Tokenizer( ) {
            Keywords = new List<string>( );
        }
    }
    public enum TokenType {
        String,
        Integer,
        Symbol,
        Unknown,
        Comment,
        Keyword,
        Word
    }
    public class Token {
        public string Value;
        public TokenType Type;
        public int LineNum, Pos, Length;

        public Token( ) {

        }
        public Token( string Value ) {
            this.Value = Value;
        }
        public Token( TokenType Type ) {
            this.Type = Type;
        }
        public Token( string Value, TokenType Type ) {
            this.Value = Value;
            this.Type = Type;
        }
        public Token( string Value, TokenType Type, int LineNum ) {
            this.Value = Value;
            this.Type = Type;
            this.LineNum = LineNum;
        }
        public Token( string Value, TokenType Type, int LineNum, int Pos ) {
            this.Value = Value;
            this.Type = Type;
            this.LineNum = LineNum;
            this.Pos = Pos;
        }
        public Token( string Value, TokenType Type, int LineNum, int Pos, int Length ) {
            this.Value = Value;
            this.Type = Type;
            this.LineNum = LineNum;
            this.Pos = Pos;
            this.Length = Length;
        }
        public static bool operator ==( Token x, Token y ) {
            if ( x.Value.Length != y.Value.Length )
                return false;
            if ( x.Value == y.Value && x.Type == y.Type )
                return true;
            return false;
        }
        public static bool operator ==( Token x, List<Token> y ) {
            if ( y == null )
                return false;
            foreach ( Token t in y )
                if ( t == x )
                    return true;
            return false;
        }
        public static bool operator !=( Token x, List<Token> y ) {
            return !( x == y );
        }
        public static bool operator ==( List<Token> x, Token y ) {
            if ( x == null )
                return false;
            foreach ( Token t in x )
                if ( t == y )
                    return true;
            return false;
        }
        public static bool operator !=( List<Token> x, Token y ) {
            return !( x == y );
        }
        public static bool operator !=( Token x, Token y ) {
            return !( x == y );
        }
        public override int GetHashCode( ) {
            return 1;
        }
        public override bool Equals( object obj ) {
            if ( obj is Token )
                return ( Token )obj == this;
            return false;
        }
        public override string ToString( ) {
            return String.Format( "token {0}", Value );
        }
    }
    #endregion
    #region Other
    public class Basic {
        public static int Count( string source, string dest ) {
            if ( dest.Length == 1 )
                return Count( source, dest[ 0 ] );
            int count = 0, index = -1;
            while ( ( index = source.IndexOf( dest, index + 1 ) ) != -1 )
                count++;
            return count;
        }
        public static int Count( string source, char dest ) {
            int count = 0, index = -1;
            while ( ( index = IndexOfChar( source, dest, index + 1 ) ) != -1 )
                count++;
            return count;
        }
        public static bool StringHas( string str, char chr ) {
            foreach ( char ch in str )
                if ( chr == ch )
                    return true;
            return false;
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
        public static bool StartsWithIndex( string source, int index, string x ) {
            if ( index + x.Length - source.Length > 0 )
                return false;
            if ( x.Length == 1 )
                return source[ index ] == x[ 0 ];
            int c = index + x.Length;
            for ( int i = index ; i < c ; i++ )
                if ( x[ i - index ] != source[ i ] )
                    return false;
            return true;
        }
        public static string GetString( TokenType tt ) {
            switch ( tt ) {
                case TokenType.Comment:
                    return "comment";

                case TokenType.Integer:
                    return "integer";

                case TokenType.Keyword:
                    return "keyword";

                case TokenType.String:
                    return "string";

                case TokenType.Symbol:
                    return "symbol";

                case TokenType.Unknown:
                    return "unknown";

                case TokenType.Word:
                    return "word";
            }
            return "token";
        }
        public static int IndexOfChar( string source, char dest, int StartIndex = 0 ) {
            int c = source.Length;
            for ( int i = StartIndex ; i < c ; i++ )
                if ( dest == source[ i ] )
                    return i;
            return -1;
        }
        static long ticks = 0;
        public static void StartTimer( bool Paused = false ) {
            if ( Paused )
                ticks = 0;
            else
                ticks = DateTime.Now.Ticks;
        }
        public static int WastedTime( ) {
            return new TimeSpan( DateTime.Now.Ticks - ticks ).Milliseconds;
        }
        public static void PauseTimer( ) {
            ticks -= DateTime.Now.Ticks;
        }
        public static void ContinueTimer( ) {
            ticks = DateTime.Now.Ticks + ticks;
        }
    }
    public class XManager<T> : List<T> {
        public int Index = 0;
        public XManager( ) {

        }
        public XManager( List<T> RR ) {
            base.Clear( );
            foreach ( T Item in RR ) {
                base.Add( Item );
            }
        }
        public void ResetX( List<T> RR ) {
            base.Clear( );
            foreach ( T Item in RR ) {
                base.Add( Item );
            }
            Index = 0;
        }
        public void ResetX( ) {
            Index = 0;
        }
        public static XManager<T> operator ++( XManager<T> x ) {
            x.Next( );
            return x;
        }
        public static XManager<T> operator --( XManager<T> x ) {
            x.Index--;
            return x;
        }
        public void Next( ) {
            Index++;
        }
        public bool Eof {
            get {
                return Index >= base.Count;
            }
        }
        public T X {
            get {
                //TODO: REMOVE THIS TRY
                try {
                    return base[ Index ];
                } catch {
                    Debugger.Break( );
                    return base[ Index ];
                }
            }
            set {
                base[ Index ] = value;
            }
        }
        public void DeleteX( ) {
            base.RemoveAt( Index );
        }
        public int Indexed {
            get {
                return Index;
            }
            set {
                Index = value;
            }
        }
        public string Tag {
            get;
            set;
        }
    }
    public class win32 {
        #region Variables
        public const int WS_EX_CONTROLPARENT = 65536;
        public const int WS_EX_CLIENTEDGE = 512;
        public const int WS_BORDER = 8388608;
        public const int HTTRANSPARENT = -1;
        public const int BS_COMMANDLINK = 14;
        public const int BS_SPLITBUTTON = 12;
        public const int BS_DEFSPLITBUTTON = 13;
        public const int BCM_SETNOTE = 5641;
        public const int BCM_SETSHIELD = 5644;
        public const int BM_SETIMAGE = 247;
        public const int BCM_SETSPLITINFO = 5639;
        public const int BCN_SETDROPDOWNSTATE = 5638;
        public const int ECM_FIRST = 5376;
        public const int EM_SETCUEBANNER = 5377;
        public const int CB_SETCUEBANNER = 5891;
        public const int STM_SETICON = 368;
        public const int TV_FIRST = 4352;
        public const int TVM_SETEXTENDEDSTYLE = 4396;
        public const int TVM_GETEXTENDEDSTYLE = 4397;
        public const int TVM_SETAUTOSCROLLINFO = 4411;
        public const int TVS_NOHSCROLL = 32768;
        public const int TVS_EX_AUTOHSCROLL = 32;
        public const int TVS_EX_FADEINOUTEXPANDOS = 64;
        public const int TVS_EX_DOUBLEBUFFER = 4;
        public const int GWL_STYLE = -16;
        public const int LVM_FIRST = 4096;
        public const int LVM_SETEXTENDEDLISTVIEWSTYLE = 4150;
        public const int LVS_EX_FULLROWSELECT = 32;
        public const int LVS_EX_DOUBLEBUFFER = 65536;
        public const int PBS_SMOOTHREVERSE = 16;
        public const int PBST_NORMAL = 1;
        public const int PBST_ERROR = 2;
        public const int PBST_PAUSED = 3;
        public const int PBM_SETSTATE = 1040;
        public const int WM_NULL = 0;
        public const int WM_CREATE = 1;
        public const int WM_DESTROY = 2;
        public const int WM_MOVE = 3;
        public const int WM_SIZE = 5;
        public const int WM_ACTIVATE = 6;
        public const int WM_SETFOCUS = 7;
        public const int WM_KILLFOCUS = 8;
        public const int WM_ENABLE = 10;
        public const int WM_SETREDRAW = 11;
        public const int WM_SETTEXT = 12;
        public const int WM_GETTEXT = 13;
        public const int WM_GETTEXTLENGTH = 14;
        public const int WM_PAINT = 15;
        public const int WM_CLOSE = 16;
        public const int WM_QUERYENDSESSION = 17;
        public const int WM_QUIT = 18;
        public const int WM_QUERYOPEN = 19;
        public const int WM_ERASEBKGND = 20;
        public const int WM_SYSCOLORCHANGE = 21;
        public const int WM_ENDSESSION = 22;
        public const int WM_SYSTEMERROR = 23;
        public const int WM_SHOWWINDOW = 24;
        public const int WM_CTLCOLOR = 25;
        public const int WM_WININICHANGE = 26;
        public const int WM_SETTINGCHANGE = 26;
        public const int WM_DEVMODECHANGE = 27;
        public const int WM_ACTIVATEAPP = 28;
        public const int WM_FONTCHANGE = 29;
        public const int WM_TIMECHANGE = 30;
        public const int WM_CANCELMODE = 31;
        public const int WM_SETCURSOR = 32;
        public const int WM_MOUSEACTIVATE = 33;
        public const int WM_CHILDACTIVATE = 34;
        public const int WM_QUEUESYNC = 35;
        public const int WM_GETMINMAXINFO = 36;
        public const int WM_PAINTICON = 38;
        public const int WM_ICONERASEBKGND = 39;
        public const int WM_NEXTDLGCTL = 40;
        public const int WM_SPOOLERSTATUS = 42;
        public const int WM_DRAWITEM = 43;
        public const int WM_MEASUREITEM = 44;
        public const int WM_DELETEITEM = 45;
        public const int WM_VKEYTOITEM = 46;
        public const int WM_CHARTOITEM = 47;
        public const int WM_SETFONT = 48;
        public const int WM_GETFONT = 49;
        public const int WM_SETHOTKEY = 50;
        public const int WM_GETHOTKEY = 51;
        public const int WM_QUERYDRAGICON = 55;
        public const int WM_COMPAREITEM = 57;
        public const int WM_COMPACTING = 65;
        public const int WM_WINDOWPOSCHANGING = 70;
        public const int WM_WINDOWPOSCHANGED = 71;
        public const int WM_POWER = 72;
        public const int WM_COPYDATA = 74;
        public const int WM_CANCELJOURNAL = 75;
        public const int WM_NOTIFY = 78;
        public const int WM_INPUTLANGCHANGEREQUEST = 80;
        public const int WM_INPUTLANGCHANGE = 81;
        public const int WM_TCARD = 82;
        public const int WM_HELP = 83;
        public const int WM_USERCHANGED = 84;
        public const int WM_NOTIFYFORMAT = 85;
        public const int WM_CONTEXTMENU = 123;
        public const int WM_STYLECHANGING = 124;
        public const int WM_STYLECHANGED = 125;
        public const int WM_DISPLAYCHANGE = 126;
        public const int WM_GETICON = 127;
        public const int WM_SETICON = 128;
        public const int WM_NCCREATE = 129;
        public const int WM_NCDESTROY = 130;
        public const int WM_NCCALCSIZE = 131;
        public const int WM_NCHITTEST = 132;
        public const int WM_NCPAINT = 133;
        public const int WM_NCACTIVATE = 134;
        public const int WM_GETDLGCODE = 135;
        public const int WM_NCMOUSEMOVE = 160;
        public const int WM_NCLBUTTONDOWN = 161;
        public const int WM_NCLBUTTONUP = 162;
        public const int WM_NCLBUTTONDBLCLK = 163;
        public const int WM_NCRBUTTONDOWN = 164;
        public const int WM_NCRBUTTONUP = 165;
        public const int WM_NCRBUTTONDBLCLK = 166;
        public const int WM_NCMBUTTONDOWN = 167;
        public const int WM_NCMBUTTONUP = 168;
        public const int WM_NCMBUTTONDBLCLK = 169;
        public const int WM_KEYFIRST = 256;
        public const int WM_KEYDOWN = 256;
        public const int WM_KEYUP = 257;
        public const int WM_CHAR = 258;
        public const int WM_DEADCHAR = 259;
        public const int WM_SYSKEYDOWN = 260;
        public const int WM_SYSKEYUP = 261;
        public const int WM_SYSCHAR = 262;
        public const int WM_SYSDEADCHAR = 263;
        public const int WM_KEYLAST = 264;
        public const int WM_IME_STARTCOMPOSITION = 269;
        public const int WM_IME_ENDCOMPOSITION = 270;
        public const int WM_IME_COMPOSITION = 271;
        public const int WM_IME_KEYLAST = 271;
        public const int WM_INITDIALOG = 272;
        public const int WM_COMMAND = 273;
        public const int WM_SYSCOMMAND = 274;
        public const int WM_TIMER = 275;
        public const int WM_HSCROLL = 276;
        public const int WM_VSCROLL = 277;
        public const int WM_INITMENU = 278;
        public const int WM_INITMENUPOPUP = 279;
        public const int WM_MENUSELECT = 287;
        public const int WM_MENUCHAR = 288;
        public const int WM_ENTERIDLE = 289;
        public const int WM_CTLCOLORMSGBOX = 306;
        public const int WM_CTLCOLOREDIT = 307;
        public const int WM_CTLCOLORLISTBOX = 308;
        public const int WM_CTLCOLORBTN = 309;
        public const int WM_CTLCOLORDLG = 310;
        public const int WM_CTLCOLORSCROLLBAR = 311;
        public const int WM_CTLCOLORSTATIC = 312;
        public const int WM_MOUSEFIRST = 512;
        public const int WM_MOUSEMOVE = 512;
        public const int WM_LBUTTONDOWN = 513;
        public const int WM_LBUTTONUP = 514;
        public const int WM_LBUTTONDBLCLK = 515;
        public const int WM_RBUTTONDOWN = 516;
        public const int WM_RBUTTONUP = 517;
        public const int WM_RBUTTONDBLCLK = 518;
        public const int WM_MBUTTONDOWN = 519;
        public const int WM_MBUTTONUP = 520;
        public const int WM_MBUTTONDBLCLK = 521;
        public const int WM_MOUSELAST = 522;
        public const int WM_MOUSEWHEEL = 522;
        public const int WM_PARENTNOTIFY = 528;
        public const int WM_ENTERMENULOOP = 529;
        public const int WM_EXITMENULOOP = 530;
        public const int WM_NEXTMENU = 531;
        public const int WM_SIZING = 532;
        public const int WM_CAPTURECHANGED = 533;
        public const int WM_MOVING = 534;
        public const int WM_POWERBROADCAST = 536;
        public const int WM_DEVICECHANGE = 537;
        public const int WM_MDICREATE = 544;
        public const int WM_MDIDESTROY = 545;
        public const int WM_MDIACTIVATE = 546;
        public const int WM_MDIRESTORE = 547;
        public const int WM_MDINEXT = 548;
        public const int WM_MDIMAXIMIZE = 549;
        public const int WM_MDITILE = 550;
        public const int WM_MDICASCADE = 551;
        public const int WM_MDIICONARRANGE = 552;
        public const int WM_MDIGETACTIVE = 553;
        public const int WM_MDISETMENU = 560;
        public const int WM_ENTERSIZEMOVE = 561;
        public const int WM_EXITSIZEMOVE = 562;
        public const int WM_DROPFILES = 563;
        public const int WM_MDIREFRESHMENU = 564;
        public const int WM_IME_SETCONTEXT = 641;
        public const int WM_IME_NOTIFY = 642;
        public const int WM_IME_CONTROL = 643;
        public const int WM_IME_COMPOSITIONFULL = 644;
        public const int WM_IME_SELECT = 645;
        public const int WM_IME_CHAR = 646;
        public const int WM_IME_KEYDOWN = 656;
        public const int WM_IME_KEYUP = 657;
        public const int WM_MOUSEHOVER = 673;
        public const int WM_NCMOUSELEAVE = 674;
        public const int WM_MOUSELEAVE = 675;
        public const int WM_CUT = 768;
        public const int WM_COPY = 769;
        public const int WM_PASTE = 770;
        public const int WM_CLEAR = 771;
        public const int WM_UNDO = 772;
        public const int WM_RENDERFORMAT = 773;
        public const int WM_RENDERALLFORMATS = 774;
        public const int WM_DESTROYCLIPBOARD = 775;
        public const int WM_DRAWCLIPBOARD = 776;
        public const int WM_PAINTCLIPBOARD = 777;
        public const int WM_VSCROLLCLIPBOARD = 778;
        public const int WM_SIZECLIPBOARD = 779;
        public const int WM_ASKCBFORMATNAME = 780;
        public const int WM_CHANGECBCHAIN = 781;
        public const int WM_HSCROLLCLIPBOARD = 782;
        public const int WM_QUERYNEWPALETTE = 783;
        public const int WM_PALETTEISCHANGING = 784;
        public const int WM_PALETTECHANGED = 785;
        public const int WM_HOTKEY = 786;
        public const int WM_PRINT = 791;
        public const int WM_PRINTCLIENT = 792;
        public const int WM_HANDHELDFIRST = 856;
        public const int WM_HANDHELDLAST = 863;
        public const int WM_PENWINFIRST = 896;
        public const int WM_PENWINLAST = 911;
        public const int WM_COALESCE_FIRST = 912;
        public const int WM_COALESCE_LAST = 927;
        public const int WM_DDE_FIRST = 992;
        public const int WM_DDE_INITIATE = 992;
        public const int WM_DDE_TERMINATE = 993;
        public const int WM_DDE_ADVISE = 994;
        public const int WM_DDE_UNADVISE = 995;
        public const int WM_DDE_ACK = 996;
        public const int WM_DDE_DATA = 997;
        public const int WM_DDE_REQUEST = 998;
        public const int WM_DDE_POKE = 999;
        public const int WM_DDE_EXECUTE = 1000;
        public const int WM_DDE_LAST = 1000;
        public const int WM_USER = 1024;
        public const int WM_APP = 32768;
        #endregion
    }
    #endregion
    public class Parser {
        #region Parser Code
        static Random random = new Random( );
        public Tokenizer Tokenizer {
            get;
            set;
        }
        public bool isDll {
            get;
            set;
        }
        public int ID {
            get;
            set;
        }
        public ParserAcc Acc {
            get;
            set;
        }
        public ParserStyle CodeStyle {
            get;
            set;
        }
        public ErrorCollection ErrorCollector {
            get;
            set;
        }
        public string CompilingDirectory {
            get;
            set;
        }
        public bool Successful {
            get {
                return ErrorCollector.Count == 0;
            }
        }
        public string ErrorOutput {
            get {
                string res = "";
                foreach ( CodeErrorException cee in ErrorCollector )
                    if ( res == "" )
                        res = cee.Message;
                    else
                        res += "\r\n" + cee.Message;
                return res;
            }
        }
        public XManager<Token> CurrentPacket {
            get;
            set;
        }
        public List<XManager<Token>> TokenPackets {
            get;
            set;
        }
        public List<Source> Sources = new List<Source>( );
        public List<List<Token>> Detect = new List<List<Token>>( ), Replacement = new List<List<Token>>( );
        public List<PhraseParser> Phrases {
            get;
            set;
        }
        [DefaultValue( false )]
        public bool Threaded {
            get;
            set;
        }
        List<Thread> ThreadArea {
            get;
            set;
        }
        public int ThreadHit {
            get;
            set;
        }
        [DefaultValue( "main" )]
        public string Package {
            get;
            set;
        }
        public Parser( bool setbase = true ) {
            Acc = new ParserAcc( null );
            Tokenizer = new Tokenizer( );
            CodeStyle = new ParserStyle( );
            ErrorCollector = new ErrorCollection( );
            TokenPackets = new List<XManager<Token>>( );
            ThreadArea = new List<Thread>( );
            ThreadHit = Environment.ProcessorCount * 2;
            Phrases = new List<PhraseParser>( );
        }
        public void Collect( string Code ) {
            AddThread( new ThreadStart( ( ) => {
                Collect( new ParserOrganize( this ).CleanPacket( new XManager<Token>( Tokenizer.Tokenize( Code ) ) ) );
            } ) );
        }
        public void Collect( XManager<Token> TokenPacket ) {

            TokenPackets.Add( TokenPacket );
        }
        public string Crack( TokenType tt ) {
            Token t = CurrentPacket.X;
            if ( t.Type == tt ) {
                CurrentPacket.Next( );
                return t.Value;
            } else
                throw new CodeErrorException( t, CurrentPacket.Tag, "expected " + Basic.GetString( tt ) );
        }
        public bool Crack( List<Token> lt ) {
            if ( CurrentPacket.X == lt ) {
                CurrentPacket.Next( );
                return true;
            }
            return false;
        }
        public void Cracks( List<Token> lt ) {
            while ( CurrentPacket.X == lt )
                CurrentPacket.Next( );
        }
        public void mustCrack( List<Token> lt ) {
            if ( !Crack( lt ) )
                throw new CodeErrorException( CurrentPacket.X, CurrentPacket.Tag, "expected '" + lt[ 0 ].Value + "'" );
        }
        public string CrackIdent( bool newtypes = false ) {
            List<String> ident_build = new List<string>( );
            ident_build.Add( Crack( TokenType.Word ) );
            while ( Crack( Acc.Bracket_Split ) )
                ident_build.Add( Crack( TokenType.Word ) );
            return string.Join( ".", ident_build.ToArray( ) );
        }
        public bool Crack( Token t ) {
            if ( CurrentPacket.X == t ) {
                CurrentPacket.Next( );
                return true;
            }
            return false;
        }
        public void ParseDetails( ) {
            int c = Phrases.Count - 1;
            for ( int i = 0 ; i < c ; i++ ) {
                if ( Phrases[ i ].Keys.Count < Phrases[ i + 1 ].Keys.Count ) {
                    Phrases.Insert( i, Phrases[ i + 1 ] );
                    Phrases.RemoveAt( i + 1 );
                }
            }
            WaitThreadZero( );
            foreach ( XManager<Token> TokenPacket in TokenPackets ) {
                if ( TokenPacket.Count == 0 )
                    continue;
                AddThread( new ThreadStart( ( ) => {
                    ParserOrganize parserOrganize = new ParserOrganize( this );
                    parserOrganize.ParseData( TokenPacket );
                    Sources.Add( parserOrganize.source );
                } ) );
            }
//            Sources.Add( new Source( ) {
//                Content = @"namespace System {
//    public class Object {
//    }
//    public class CFunctionAttribute : Attribute {
//    }
//    public class LanguageFunctions {
//        public static void Split(Action action) {}
//        public static void Finally(Action action) {}
//    }
//}"
//            } );
            TokenPackets.Clear( );
        }
        public void AddThread( ThreadStart ts ) {
            if ( ThreadArea.Count >= ThreadHit )
                WaitThreadMax( );
            Thread t = new Thread( ts );
            t.Start( );
            ThreadArea.Add( t );
            if ( !Threaded )
                t.Join( );
        }
        public void WaitThreadZero( ) {
            return;
            //int c = ThreadArea.Count;
            //try {
            //    for( int i = 0 ; i < c ; i++ ) {
            //        if( ThreadArea[ i ].ThreadState != ThreadState.Running ) {
            //            ThreadArea.RemoveAt( i-- );
            //            c--;
            //        }
            //    }
            //} catch {
            //}
            //while( ThreadArea.Count > 0 ) {
            //    Thread.Sleep( 1 );
            //    c = ThreadArea.Count;
            //    try {
            //        for( int i = 0 ; i < c ; i++ ) {
            //            if( ThreadArea[ i ].ThreadState != ThreadState.Running ) {
            //                ThreadArea.RemoveAt( i-- );
            //                c--;
            //            }
            //        }
            //    } catch {
            //    }
            //}
        }
        public void WaitThreadMax( ) {
            return;
            //int c = ThreadArea.Count;
            //try {
            //    for( int i = 0 ; i < c ; i++ ) {
            //        if( ThreadArea[ i ].ThreadState != ThreadState.Running ) {
            //            ThreadArea.RemoveAt( i-- );
            //            c--;
            //        }
            //    }
            //} catch {
            //}
            //while( ThreadArea.Count >= ThreadHit ) {
            //    Thread.Sleep( 1 );
            //    c = ThreadArea.Count;
            //    try {
            //        for( int i = 0 ; i < c ; i++ ) {
            //            if( ThreadArea[ i ].ThreadState != ThreadState.Running ) {
            //                ThreadArea.RemoveAt( i-- );
            //                c--;
            //            }
            //        }
            //    } catch {
            //    }
            //}
        }
        public static Parser Standard {
            get {
                Parser parser = new Parser( );
                parser.Tokenizer.AddKeywords( new string[ ]
                {
                   "assert", "break", "case","class","struct",
                    "catch", "continue", "default", "do",
                    "else", "enum", "extends", "final", "finally",
                    "for", "if", "implements", "import","include",
                    "instanceof",  "interface", "long",
                    "native", "new","bind","module","package",
                    "public", "return", "static", "stricttfp",
                    "super", "switch", "this","using","model","split",
                    "throw", "try", "volatile","operator","shortcut","phrase","statement","stmt","_",
                    "while","loop","false","true","half","percent","partial","get","set","namespace"
                } );
                parser.CodeStyle.AddRange
                (
                    "assert", "break", "case", "class",
                    "catch", "continue", "default", "do",
                    "else", "enum", "final", "finally", "model", "module",
                    "for", "if", "import", "include", "using",
                    "interface", "long", "extends", "implements",
                    "native", "new", "phrase", "statement", "stmt", "_",
                    "return", "static", "stricttfp", "bind", "package",
                    "switch", "synchronized", "this", "shortcut", "split",
                    "throw", "throws", "transient", "try", "volatile", "operator",
                    "while", "loop", "false", "true", "half", "percent", "partial", "get", "set", "namespace"
                );
                parser.CodeStyle[ "phrase" ].AddRange( new Token[ ] { new Token( "statement", TokenType.Keyword ), new Token( "stmt", TokenType.Keyword ) } );
                parser.CodeStyle[ "class" ].AddRange( new Token[ ] { new Token( "struct", TokenType.Keyword ), new Token( "model", TokenType.Keyword ) } );
                parser.CodeStyle[ "namespace" ].AddRange( new Token[ ] { new Token( "module", TokenType.Keyword ), new Token( "package", TokenType.Keyword ) } );
                parser.CodeStyle.Add( "base", "super" );
                parser.CodeStyle.Add( "is", "instanceof" );
                parser.CodeStyle.Add( "inherit", new List<Token>( new Token[ ] { new Token( "extends", TokenType.Keyword ), new Token( "implements", TokenType.Keyword )
                , new Token( ":", TokenType.Symbol )} ) );
                parser.Tokenizer.AddOperators( "++", "--", "==", "<=", ">=", "!=", "=", "+=", "-=", "*=", "/=", "&&", "||",
                    "^^", "&", "|", "^", ".", ":", "{", "}", "<", ">", "(", ")", ",", "+", "-", "*", "/", ";", "#", "@", "[", "]" );
                parser.CodeStyle.AddSymbols( "++", "--", "==", "<=", ">=", "!=", "=", "+=", "-=", "*=", "/=", "&&", "||",
                    "^^", "&", "|", "^", ".", ":", "{", "}", "<", ">", "(", ")", ",", "+", "-", "*", "/", ";", "#", "@", "[", "]" );
                parser.Acc = new ParserAcc( parser.CodeStyle );
                return parser;
            }
        }
        #endregion
        #region ParserOrganize
        public class ParserOrganize {
            public Parser GlobalSet;
            public ParserAcc Acc {
                get;
                set;
            }
            public bool isDll {
                get;
                set;
            }
            public int ID {
                get;
                set;
            }
            public ParserStyle CodeStyle {
                get;
                protected set;
            }
            public ErrorCollection ErrorCollector {
                get;
                set;
            }
            public ParserFilter FilterCode {
                get;
                set;
            }
            public XManager<Token> CurrentPacket {
                get;
                set;
            }
            public List<PhraseParser> Phrases {
                get;
                set;
            }
            public string CompilingDirectory {
                get;
                set;
            }
            public string Package {
                get;
                set;
            }
            public Source source;
            public ParserOrganize( Parser parser ) {
                GlobalSet = parser;
                Acc = parser.Acc;
                CodeStyle = parser.CodeStyle;
                ErrorCollector = parser.ErrorCollector;
                FilterCode = new ParserFilter( );
                FilterCode.canNamespace = true;
                Phrases = parser.Phrases;
                CompilingDirectory = parser.CompilingDirectory;
                Package = parser.Package;
                Detect = parser.Detect;
                Replacement = parser.Replacement;
                source = new Source( );
                isDll = parser.isDll;
                ID = parser.ID;
            }
            Dictionary<string, string> dss = new Dictionary<string, string>( );
            List<List<Token>> Detect = new List<List<Token>>( ), Replacement = new List<List<Token>>( );
            public void ParseObject( ) {
                Cracks( Acc.EndStmt );
                if ( CurrentPacket.Eof )
                    return;
                int stepnext = CurrentPacket.Index + 1;
                try {
                    #region Class and Interface
                    string AttibuteCode = @"return new object[] { ";
                    List<String> attibutes = new List<string>( ), attibutes_types = new List<string>( );
                    while ( Crack( Acc.Array_Start ) ) {
                        do {
                            if ( !AttibuteCode.EndsWith( " { " ) )
                                AttibuteCode += ", ";
                            string attibute = CrackIdent( false );
                            attibutes_types.Add( attibute );
                            if ( Crack( Acc.Bracket_Start ) ) {
                                attibute += "( ";
                                if ( !Crack( Acc.Bracket_End ) ) {
                                    attibute += CrackExpression( );
                                    while ( Crack( Acc.Bracket_Split ) )
                                        attibute += ", " + CrackExpression( );
                                    mustCrack( Acc.Bracket_End );
                                    attibute += " )";
                                } else
                                    attibute += ")";
                            } else
                                attibute += "( )";
                            AttibuteCode += "new " + attibute;
                        }
                        while ( Crack( Acc.Bracket_Split ) );
                        mustCrack( Acc.Array_End );
                    }
                    AttibuteCode += AttibuteCode.EndsWith( " " ) ? "};" : " };";
                    bool istatic = false, parser_class = true;
                    istatic = Crack( Acc.Static );
                    if ( Crack( Acc.Class ) )
                        source.AppendLine( ( isDll ? "[System.DllField(" + ID + ")]" : "" ) + "public partial class " );
                    else if ( Crack( Acc.Interface ) )
                        source.AppendLine( ( isDll ? "[System.DllField(" + ID + ")]" : "" ) + "public partial interface " );
                    else
                        parser_class = false;
                    if ( parser_class ) {
                        FilterCode.canNamespace = false;
                        FilterCode.canMethod = true;
                        string tst = CrackIdent( false );
                        FilterCode.Path += tst + ".";
                        source.Append( tst );
                        if ( Crack( Acc.Inherit ) ) {
                            List<String> build = new List<string>( );
                            build.Add( CrackIdent( false ) );
                            while ( Crack( Acc.Bracket_Split ) )
                                build.Add( CrackIdent( false ) );
                            source.Append( " : " + string.Join( ", ", build.ToArray( ) ) );
                        }
                        mustCrack( Acc.Block_Start );
                        source.Append( " {" );
                        source.AppendLine( "public static object[] GetAttibutes() {" + AttibuteCode + "}" );
                        source.AppendLine( );
                        CrackCode( );
                        source.AppendLine( "}" );
                        FilterCode.PopNamespace( );
                        FilterCode.PopMethod( );
                        FilterCode.PopPath( );
                        return;
                    }
                    #endregion
                    #region Methods ,Variables efc.
                    if ( FilterCode.canMethod ) {
                        #region Constructor
                        if ( FilterCode.Path.EndsWith( "." + Crack( TokenType.Word ) + "." ) ) {
                            source.AppendLine( "public " + CurrentPacket[ CurrentPacket.Index - 1 ].Value );
                            mustCrack( Acc.Bracket_Start );
                            if ( !Crack( Acc.Bracket_End ) ) {
                                List<String> build = new List<string>( );
                                string lasttype = CrackIdent( true );
                                build.Add( lasttype + " " + Crack( TokenType.Word ) );
                                while ( Crack( Acc.Bracket_Split ) ) {
                                    CurrentPacket.Next( );
                                    if ( CurrentPacket.X == Acc.Bracket_Split || CurrentPacket.X == Acc.Bracket_End ) {
                                        CurrentPacket.Index -= 1;
                                        build.Add( lasttype + " " + Crack( TokenType.Word ) );
                                        continue;
                                    }
                                    CurrentPacket.Index--;
                                    build.Add( ( lasttype = CrackIdent( true ) ) + " " + Crack( TokenType.Word ) );
                                }
                                mustCrack( Acc.Bracket_End );
                                source.Append( "( " + string.Join( ", ", build ) + " )" );
                            } else
                                source.Append( "( )" );
                            if ( !Crack( Acc.Block_Start ) ) {
                                source.Append( ";" );
                                return;
                            }
                            source.Append( " {" );
                            FilterCode.canMethod = false;
                            dss.Clear( );
                            while ( !Crack( Acc.Block_End ) )
                                CrackStatement( );
                            dss.Clear( );
                            FilterCode.PopMethod( );
                            source.AppendLine( "}" );
                            return;
                        } else
                            CurrentPacket.Index--;
                        bool ismethod = false, _extern = false;
                        string type = CrackIdent( true ), name;
                        if ( Crack( Acc.Operator ) ) {
                            if ( Crack( Acc.Add ) ) {
                            } else if ( Crack( Acc.Sub ) ) {
                            } else if ( Crack( Acc.Mul ) ) {
                            } else if ( Crack( Acc.Div ) ) {
                            } else if ( Crack( Acc.Incre ) ) {
                            } else if ( Crack( Acc.Decre ) ) {
                            } else if ( Crack( Acc.Ident ) ) {
                            } else if ( Crack( Acc.Equal ) ) {
                            } else if ( Crack( Acc.Asg_Greater ) ) {
                            } else if ( Crack( Acc.Asg_Lesser ) ) {
                            } else if ( Crack( Acc.Lesser ) ) {
                            } else if ( Crack( Acc.Greater ) ) {
                            } else if ( Crack( Acc.Assign ) ) {
                            } else if ( Crack( Acc.Asg_Add ) ) {
                            } else if ( Crack( Acc.Asg_Sub ) ) {
                            } else if ( Crack( Acc.Asg_Mul ) ) {
                            } else if ( Crack( Acc.Asg_Div ) ) {
                            } else
                                throw new CodeErrorException( CurrentPacket.X, CurrentPacket.Tag, "expected operator (+, -,..." );
                            name = "operator" + CurrentPacket[ CurrentPacket.Index - 1 ];
                            ismethod = true;
                        } else {
                            if ( Crack( Acc.ExternBody ) ) {
                                ismethod = _extern = true;
                            }
                            name = Crack( TokenType.Word );
                        }
                        source.AppendLine( "public static object[] GetAttibutes" + name + "() {" + AttibuteCode + "}" );
                        #endregion
                        #region Method
                        if ( Crack( Acc.Bracket_Start ) ) {
                            StringBuilder stringbuilder = new StringBuilder( );
                            stringbuilder.Append( "public " + ( istatic ? "static " : "" ) + type + " " + name );
                            if ( !Crack( Acc.Bracket_End ) ) {
                                List<String> build = new List<string>( );
                                string lasttype = CrackIdent( true );
                                build.Add( lasttype + " " + Crack( TokenType.Word ) );
                                while ( Crack( Acc.Bracket_Split ) ) {
                                    CurrentPacket.Next( );
                                    if ( CurrentPacket.X == Acc.Bracket_Split || CurrentPacket.X == Acc.Bracket_End ) {
                                        CurrentPacket.Index -= 1;
                                        build.Add( lasttype + " " + Crack( TokenType.Word ) );
                                        continue;
                                    }
                                    CurrentPacket.Index--;
                                    build.Add( ( lasttype = CrackIdent( true ) ) + " " + Crack( TokenType.Word ) );
                                }
                                mustCrack( Acc.Bracket_End );
                                stringbuilder.Append( "( " + string.Join( ", ", build ) + " )" );
                            } else
                                stringbuilder.Append( "( )" );
                            if ( _extern ) {
                                source.AppendLine( "[System.CFunction]\r\nextern " + stringbuilder.ToString( ) + ";" );
                                return;
                            }
                            if ( !Crack( Acc.Block_Start ) ) {
                                source.AppendLine( "extern " + stringbuilder.ToString( ) + ";" );
                                return;
                            }
                            source.AppendLine( stringbuilder.ToString( ) + " {" );
                            FilterCode.canMethod = false;
                            dss.Clear( );
                            while ( !Crack( Acc.Block_End ) )
                                CrackStatement( );
                            dss.Clear( );
                            FilterCode.PopMethod( );
                            source.AppendLine( "}" );
                            return;
                        }
                        if ( ismethod ) {
                            throw new CodeErrorException( CurrentPacket.X, CurrentPacket.Tag, "expected brackets with arguments" );
                        }
                        #endregion
                        #region Variable\s
                        List<String> Build = new List<string>( );
                        Build.Add( name );
                        while ( Crack( Acc.Bracket_Split ) ) {
                            Build.Add( name = Crack( TokenType.Word ) );
                            source.AppendLine( "public static object[] GetAttibutes" + name + "() {" + AttibuteCode + "}" );
                        }
                        source.AppendLine( "public " + ( istatic ? "static " : "" ) + type + " " + string.Join( ", ", Build.ToArray( ) ) + ";" );
                        #endregion
                        return;
                    }
                    #endregion
                    #region Namespace
                    if ( FilterCode.canNamespace ) {
                        if ( Crack( Acc.Namespace ) ) {
                            string @namespace;
                            FilterCode.Path += ( @namespace = CrackIdent( false ) ) + ".";
                            source.AppendLine( "namespace " + @namespace + " {" );
                            mustCrack( Acc.Block_Start );
                            CrackCode( );
                            source.AppendLine( "}" );
                            FilterCode.PopPath( );
                            return;
                        }
                    }
                    #endregion
                    #region Using and Import
                    if ( Crack( Acc.Using ) ) {
                        List<String> Build = new List<string>( );
                        Build.Add( Crack( TokenType.Word ) );
                        if ( Crack( Acc.Assign ) ) {
                            source.AppendLine( "using " + Build.First( ) + " = " );
                            Build.RemoveAt( 0 );
                            Build.Add( Crack( TokenType.Word ) );
                            while ( Crack( Acc.Ident ) ) {
                                Build.Add( Crack( TokenType.Word ) );
                            }
                            source.Append( string.Join( ".", Build.ToArray( ) ) + ";" );
                            return;
                        }
                        while ( Crack( Acc.Ident ) ) {
                            Build.Add( Crack( TokenType.Word ) );
                        }
                        source.AppendLine( "using " + string.Join( ".", Build.ToArray( ) ) + ";" );
                        return;
                    }
                    if ( Crack( Acc.Import ) ) {
                        if ( Crack( Acc.Bracket_Start ) ) {
                            string cracked = Crack( TokenType.String ).Replace( "\"", "" ).ToLower( );
                            if ( cracked != "c#" )
                                throw new CodeErrorException( CurrentPacket[ CurrentPacket.Index - 1 ], CurrentPacket.Tag, "Languages allowed are C# ,C ,Go" );
                            mustCrack( Acc.Bracket_End );
                            int line = CurrentPacket.X.LineNum;
                            mustCrack( Acc.Block_Start );
                            int i = 1;
                            string code = "";
                            while ( i > 0 ) {
                                if ( Crack( Acc.Block_Start ) ) {
                                    CurrentPacket.Index--;
                                    i++;
                                }
                                if ( Crack( Acc.Block_End ) ) {
                                    CurrentPacket.Index--;
                                    i--;
                                }
                                if ( i <= 0 ) {
                                    CurrentPacket++;
                                    break;
                                }
                                if ( line != CurrentPacket.X.LineNum ) {
                                    code += "\r\n";
                                    line = CurrentPacket.X.LineNum;
                                }
                                if ( code != "" && !code.EndsWith( "\r\n" ) )
                                    code += " ";
                                code += CurrentPacket.X.Value;
                                CurrentPacket++;
                            }
                            source.AppendLine( code );
                            return;
                        }
                        List<String> Build = new List<string>( );
                        Build.Add( Crack( TokenType.Word ) );
                        if ( Crack( Acc.Assign ) ) {
                            source.AppendLine( "using " + Build.First( ) + " = " );
                            Build.RemoveAt( 0 );
                            Build.Add( Crack( TokenType.Word ) );
                            while ( Crack( Acc.Ident ) ) {
                                Build.Add( Crack( TokenType.Word ) );
                            }
                            source.Append( string.Join( ".", Build.ToArray( ) ) + ";" );
                            return;
                        }
                        while ( Crack( Acc.Ident ) ) {
                            if ( Crack( Acc.Mul ) ) {
                                source.AppendLine( "using " + string.Join( ".", Build.ToArray( ) ) + ";" );
                                return;
                            } else
                                Build.Add( Crack( TokenType.Word ) );
                        }
                        source.AppendLine( "using " + Build.Last( ) + " = " + string.Join( ".", Build.ToArray( ) ) + ";" );
                        return;
                    }
                    #endregion
                    throw new CodeErrorException( CurrentPacket.X, CurrentPacket.Tag, "unexpected " + CurrentPacket.X.ToString( ) );
                } catch ( CodeErrorException cee ) {
                    ErrorCollector.Add( cee );
                    CurrentPacket.Index = Math.Max( CurrentPacket.Index, stepnext );
                }
            }
            public string CrackExpression( ) {
                List<String> Build = new List<string>( );
                int n = CurrentPacket.Index;
                try {
                    bool _first = true, noval;
                    while ( true ) {
                        string val = CurrentPacket.X.Value;
                        noval = false;
                        //First operators
                        if ( Crack( Acc.Add ) ) {
                        } else if ( Crack( Acc.Sub ) ) {
                        } else if ( Crack( Acc.Mul ) && !_first ) {
                        } else if ( Crack( Acc.Div ) && !_first ) {
                        } else if ( Crack( Acc.Incre ) ) {
                        } else if ( Crack( Acc.Decre ) ) {
                        } else if ( Crack( Acc.Ident ) && !_first ) {
                            val += Crack( TokenType.Word );
                            goto ReadedValue;
                        } else if ( Crack( Acc.Equal ) && !_first ) {
                        } else if ( Crack( Acc.Asg_Greater ) && !_first ) {
                        } else if ( Crack( Acc.Asg_Lesser ) && !_first ) {
                        } else if ( Crack( Acc.Lesser ) && !_first ) {
                        } else if ( Crack( Acc.Greater ) && !_first ) {
                        } else if ( Crack( Acc.Assign ) && !_first ) {
                        } else if ( Crack( Acc.Asg_Add ) && !_first ) {
                        } else if ( Crack( Acc.Asg_Sub ) && !_first ) {
                        } else if ( Crack( Acc.Asg_Mul ) && !_first ) {
                        } else if ( Crack( Acc.Asg_Div ) && !_first ) {
                        } else if ( Crack( Acc.And ) && !_first ) {
                        } else if ( Crack( Acc.AndTwo ) && !_first ) {
                        } else if ( Crack( Acc.Or ) && !_first ) {
                        } else if ( Crack( Acc.OrTwo ) && !_first ) {
                        } else if ( Crack( Acc.Xor ) && !_first ) {
                        } else if ( Crack( Acc.XorTwo ) && !_first ) {
                        } else if ( !_first )
                            throw new CodeErrorException( CurrentPacket.X, CurrentPacket.Tag, "expected operator first" );
                        else {
                            val = "";
                            noval = true;
                        }
                        //Read value
                        switch ( CurrentPacket.X.Type ) {
                            case TokenType.Integer:
                                val += Crack( TokenType.Integer );
                                break;

                            case TokenType.Keyword:
                                if ( Crack( Acc.False ) )
                                    val += "false"; //0
                                else if ( Crack( Acc.True ) )
                                    val += "true"; //1
                                else if ( Crack( Acc.Half ) )
                                    val += "0.5";
                                else if ( Crack( Acc.Percent ) )
                                    val += "0.01";
                                else if ( Crack( Acc.New ) ) {
                                    val += "new " + CrackIdent( true );
                                    List<String> b = new List<string>( );
                                    if ( val.EndsWith( "]" ) ) {
                                        if ( Crack( Acc.Block_Start ) ) {
                                            val += "{ ";
                                            if ( !Crack( Acc.Block_End ) ) {
                                                b.Add( CrackExpression( ) );
                                                while ( Crack( Acc.Bracket_Split ) )
                                                    b.Add( CrackExpression( ) );
                                                mustCrack( Acc.Block_End );
                                                val += string.Join( ", ", b.ToArray( ) ) + " }";
                                            } else
                                                val += "}";
                                            break;
                                        }
                                    }
                                    val += "( ";
                                    mustCrack( Acc.Bracket_Start );
                                    if ( !Crack( Acc.Bracket_End ) ) {
                                        b.Add( CrackExpression( ) );
                                        while ( Crack( Acc.Bracket_Split ) )
                                            b.Add( CrackExpression( ) );
                                        mustCrack( Acc.Bracket_End );
                                        val += string.Join( ", ", b.ToArray( ) ) + " )";
                                    } else
                                        val += ")";
                                } else if ( Crack( Acc.This ) )
                                    val += "this";
                                else if ( CurrentPacket.X == Acc.Import ) {
                                    ParseObject( );
                                    return "";
                                }
                                break;


                            case TokenType.String:
                                val += Crack( TokenType.String );
                                break;


                            case TokenType.Word:
                                string crk = Crack( TokenType.Word );
                                string tst = ( CurrentPacket[ CurrentPacket.Index ] == Acc.Assign ? "var " : "" );
                                if ( tst != "" ) {
                                    try {
                                        string _name = "_" + dss[ crk ];
                                        dss[ crk ] = _name;
                                        crk = _name;
                                    } catch {
                                        if ( !FilterCode.Contains( crk ) )
                                            dss.Add( crk, crk );
                                    }
                                }
                                tst += crk;
                                try {
                                    tst = dss[ tst ];
                                } catch {
                                }
                                val += tst;
                                break;

                            default:
                                if ( Crack( Acc.Bracket_Start ) ) {
                                    val += "( " + CrackExpression( ) + " )";
                                    mustCrack( Acc.Bracket_End );
                                }
                                break;
                        }
                    ReadedValue:
                        if ( noval ) {
                            if ( Crack( Acc.Incre ) )
                                val += "++";
                            else if ( Crack( Acc.Decre ) )
                                val += "--";
                        }
                        if ( Crack( Acc.Bracket_Start ) ) {
                            val += "( ";
                            List<String> b = new List<string>( );
                            if ( !Crack( Acc.Bracket_End ) ) {
                                b.Add( CrackExpression( ) );
                                while ( Crack( Acc.Bracket_Split ) )
                                    b.Add( CrackExpression( ) );
                                mustCrack( Acc.Bracket_End );
                                val += string.Join( ", ", b.ToArray( ) ) + " )";
                            } else
                                val += ")";
                        }
                        Build.Add( val );
                        _first = false;
                    }
                } catch {
                    if ( Build.Count == 0 ) {
                        CurrentPacket.Index = n;
                        throw new CodeErrorException( CurrentPacket.X, CurrentPacket.Tag, "expected expression before it" );
                    }
                }
                return string.Join( "", Build.ToArray( ) );
            }
            public string DeclareVariable( string type, string name ) {
                try {
                    string _name = "_" + dss[ name ];
                    dss[ name ] = _name;
                    source.AppendLine( type + " " + _name + ";" );
                    return _name;
                } catch {
                    dss.Add( name, name );
                    source.AppendLine( type + " " + name + ";" );
                    return name;
                }
            }
            public void CrackStatement( ) {
                Cracks( Acc.EndStmt );
                try {
                    if ( Crack( Acc.While ) ) {
                        source.AppendLine( "while (" + CrackExpression( ) + ") {" );
                        if ( Crack( Acc.Block_Start ) )
                            while ( !Crack( Acc.Block_End ) )
                                CrackStatement( );
                        else if ( Crack( Acc.EndStmt ) ) {
                        } else
                            CrackStatement( );
                        source.AppendLine( "}" );
                    } else if ( Crack( Acc.For ) ) {
                        bool Bracks = Crack( Acc.Bracket_Start );
                        if ( !Crack( Acc.EndStmt ) ) {
                            List<String> build = new List<string>( );
                            string lasttype = CrackIdent( true );
                            string lastvar = DeclareVariable( lasttype, Crack( TokenType.Word ) );
                            if ( Crack( Acc.Assign ) )
                                source.AppendLine( lastvar + " = " + CrackExpression( ) + ";" );
                            while ( Crack( Acc.Bracket_Split ) ) {
                                CurrentPacket.Next( );
                                if ( CurrentPacket.X == Acc.Bracket_Split || CurrentPacket.X == Acc.Bracket_End ) {
                                    CurrentPacket.Index -= 1;
                                    lastvar = DeclareVariable( lasttype, Crack( TokenType.Word ) );
                                    if ( Crack( Acc.Assign ) )
                                        source.AppendLine( lastvar + " = " + CrackExpression( ) + ";" );
                                    continue;
                                }
                                CurrentPacket.Index--;
                                lastvar = DeclareVariable( lasttype = CrackIdent( true ), Crack( TokenType.Word ) );
                                if ( Crack( Acc.Assign ) )
                                    source.AppendLine( lastvar + " = " + CrackExpression( ) + ";" );
                            }
                            mustCrack( Acc.EndStmt );
                        }
                        string bl = DeclareVariable( "bool", "__for_check" );
                        source.AppendLine( "while( " );
                        if ( !Crack( Acc.EndStmt ) ) {
                            bool isor = true;
                            if ( Crack( Acc.AndTwo ) )
                                isor = false;
                            else
                                Crack( Acc.OrTwo );
                            List<String> Build = new List<string>( );
                            Build.Add( CrackExpression( ) );
                            while ( Crack( Acc.Bracket_Split ) )
                                Build.Add( CrackExpression( ) );
                            source.Append( string.Join( isor ? " || " : " && ", Build.ToArray( ) ) + " ) {" );
                            mustCrack( Acc.EndStmt );
                        } else {
                            source.Append( "true ) {" );
                        }
                        if ( !Crack( ( Bracks ? Acc.Bracket_End : Acc.Block_Start ) ) ) {
                            source.AppendLine( "if (" + bl + ") {" );
                            CrackStatement( );
                            while ( Crack( Acc.Bracket_Split ) )
                                CrackStatement( );
                            source.AppendLine( "} else " + bl + " = true" );
                            mustCrack( ( Bracks ? Acc.Bracket_End : Acc.Block_Start ) );
                        } else {
                            CurrentPacket.Index--;
                        }
                        if ( Bracks )
                            mustCrack( Acc.Bracket_End );
                        if ( Crack( Acc.EndStmt ) )
                            source.AppendLine( "}" );
                        else {
                            mustCrack( Acc.Block_Start );
                            while ( !Crack( Acc.Block_End ) )
                                CrackStatement( );
                            source.AppendLine( "}" );
                        }
                    } else if ( Crack( Acc.If ) ) {
                        bool isor = true;
                        if ( Crack( Acc.AndTwo ) )
                            isor = false;
                        else
                            Crack( Acc.OrTwo );
                        List<String> Build = new List<string>( );
                        Build.Add( CrackExpression( ) );
                        while ( Crack( Acc.Bracket_Split ) )
                            Build.Add( CrackExpression( ) );
                        source.Append( "if ( " + string.Join( isor ? " || " : " && ", Build.ToArray( ) ) + " ) {" );
                        if ( Crack( Acc.Block_Start ) )
                            while ( !Crack( Acc.Block_End ) )
                                CrackStatement( );
                        else
                            CrackStatement( );
                        source.AppendLine( "}" );
                    } else if ( Crack( Acc.Loop ) ) {
                        source.Append( "while ( true ) {" );
                        if ( Crack( Acc.Block_Start ) )
                            while ( !Crack( Acc.Block_End ) )
                                CrackStatement( );
                        else
                            CrackStatement( );
                        source.AppendLine( "}" );
                    } else if ( Crack( Acc.Return ) ) {
                        try {
                            source.AppendLine( "return ( " + CrackExpression( ) + " );" );
                        } catch {
                            source.AppendLine( "return;" );
                        }
                    } else if ( Crack( Acc.SplitThread ) ) {
                        source.AppendLine( "global::System.LanguageFunctions.Split( ( ) => {" );
                        if ( Crack( Acc.Block_Start ) ) {
                            while ( !Crack( Acc.Block_End ) )
                                CrackStatement( );
                        } else
                            CrackStatement( );
                        source.AppendLine( "} );" );
                    } else if ( Crack( Acc.Finally ) ) {
                        source.AppendLine( "global::System.LanguageFunctions.Finally( ( ) => {" );
                        if ( Crack( Acc.Block_Start ) ) {
                            while ( !Crack( Acc.Block_End ) )
                                CrackStatement( );
                        } else
                            CrackStatement( );
                        source.AppendLine( "} );" );
                    } else {
                        int ind = CurrentPacket.Index;
                        try {
                            string type = CrackIdent( true );
                            string lastvar = DeclareVariable( type, Crack( TokenType.Word ) );
                            if ( Crack( Acc.Assign ) )
                                source.AppendLine( lastvar + " = " + CrackExpression( ) + ";" );
                            while ( Crack( Acc.Bracket_Split ) ) {
                                lastvar = DeclareVariable( type, Crack( TokenType.Word ) );
                                if ( Crack( Acc.Assign ) )
                                    source.AppendLine( lastvar + " = " + CrackExpression( ) + ";" );
                            }
                        } catch {
                            CurrentPacket.Index = ind;
                            source.AppendLine( CrackExpression( ) + ";" );
                        }
                    }
                } finally {
                    Cracks( Acc.EndStmt );
                }
            }
            public string CrackIdent( bool type ) {
                List<String> ident_build = new List<string>( );
                if ( !type ) {
                    if ( Crack( Acc.This ) ) {
                        ident_build.Add( "this" );
                    } else
                        ident_build.Add( Crack( TokenType.Word ) );
                } else
                    ident_build.Add( Crack( TokenType.Word ) );
                while ( Crack( Acc.Bracket_Split ) )
                    ident_build.Add( Crack( TokenType.Word ) );
                string addres = "";
                if ( type ) {
                    if ( Crack( Acc.Greater ) ) {
                        addres += "<" + CrackIdent( true );
                        while ( !Crack( Acc.Lesser ) ) {
                            Crack( Acc.Bracket_Split );
                            addres += "," + CrackIdent( true );
                        }
                        addres += ">";
                    }
                    while ( Crack( Acc.Array_Start ) ) {
                        addres += "[";
                        while ( Crack( Acc.Bracket_Split ) )
                            addres += ",";
                        mustCrack( Acc.Array_End );
                        addres += "]";
                    }
                }
                return string.Join( ".", ident_build.ToArray( ) ) + addres;
            }
            public void CrackCode( ) {
                while ( !Crack( Acc.Block_End ) ) {
                    Cracks( Acc.EndStmt );
                    if ( Crack( Acc.Block_End ) )
                        return;
                    ParseObject( );
                }
            }
            public string Crack( TokenType tt ) {
                Token t = CurrentPacket.X;
                if ( t.Type == tt ) {
                    CurrentPacket.Next( );
                    return t.Value;
                } else
                    throw new CodeErrorException( t, CurrentPacket.Tag, "expected " + Basic.GetString( tt ) );
            }
            public bool Crack( List<Token> lt ) {
                if ( CurrentPacket.X == lt ) {
                    CurrentPacket.Next( );
                    return true;
                }
                return false;
            }
            public bool Crack( Token t ) {
                if ( CurrentPacket.X == t ) {
                    CurrentPacket.Next( );
                    return true;
                }
                return false;
            }
            public void Cracks( List<Token> lt ) {
                while ( CurrentPacket.X == lt )
                    CurrentPacket.Next( );
            }
            public void ParseData( XManager<Token> XMT ) {
                XMT = new XManager<Token>( Precompiling( XMT ) );
                CurrentPacket = XMT;
                while ( !CurrentPacket.Eof )
                    ParseObject( );
            }
            public XManager<Token> CleanPacket( XManager<Token> XMT ) {
                CurrentPacket = XMT;
                List<Token> lt = new List<Token>( ), end = new List<Token>( );
                while ( !CurrentPacket.Eof ) {
                    #region Phrases
                    if ( Crack( Acc.Phrase ) ) {
                        mustCrack( Acc.Bracket_Start );
                        Token[ ] type = CrackIdentTokens( );
                        mustCrack( Acc.Bracket_End );
                        int linenum = CurrentPacket.X.LineNum;
                        string Name = "_func_";
                        PhraseParser pp = new PhraseParser( );
                        List<Token> build = new List<Token>( );
                        while ( !XMT.Eof ) {
                            if ( CurrentPacket.X.LineNum != linenum )
                                break;
                            if ( Crack( Acc.NewLine ) ) {
                                linenum++;
                                continue;
                            } else if ( Crack( Acc.PhraseArgs ) ) {
                                if ( build.Count != 0 )
                                    build.Add( Acc.Bracket_Split[ 0 ] );
                                bool brackets = Crack( Acc.Bracket_Start );
                                Token[ ] lasttype = CrackIdentTokens( );
                                build.AddRange( lasttype );
                                Crack( TokenType.Word );
                                build.Add( CurrentPacket[ CurrentPacket.Index - 1 ] );
                                Name += CurrentPacket[ CurrentPacket.Index - 1 ].Value;
                                pp.Keys.Add( PhraseParser.PhraseType.Expression );
                                while ( Crack( Acc.Bracket_Split ) ) {
                                    build.Add( CurrentPacket[ CurrentPacket.Index - 1 ] );
                                    CurrentPacket.Next( );
                                    if ( CurrentPacket.X == Acc.Bracket_Split || CurrentPacket.X == Acc.Bracket_End ) {
                                        CurrentPacket.Index--;
                                        Crack( TokenType.Word );
                                        Name += CurrentPacket[ CurrentPacket.Index - 1 ].Value;
                                        build.Add( CurrentPacket[ CurrentPacket.Index - 1 ] );
                                        pp.Keys.Add( PhraseParser.PhraseType.Expression );
                                        continue;
                                    }
                                    CurrentPacket.Index--;
                                    build.AddRange( lasttype = CrackIdentTokens( ) );
                                    Crack( TokenType.Word );
                                    Name += CurrentPacket[ CurrentPacket.Index - 1 ].Value;
                                    build.Add( CurrentPacket[ CurrentPacket.Index - 1 ] );
                                    pp.Keys.Add( PhraseParser.PhraseType.Expression );
                                }
                                if ( brackets )
                                    mustCrack( Acc.Bracket_End );
                            } else {
                                pp.Keys.Add( XMT.X );
                                XMT++;
                            }
                        }
                        string classname = "_PHRASE" + random.Next( );
                        pp.InternalLocation = classname + "." + Name;
                        end.Add( Acc.Class[ 0 ] );
                        end.Add( new Token( classname, TokenType.Word ) );
                        end.Add( Acc.Block_Start[ 0 ] );
                        end.Add( Acc.Static[ 0 ] );
                        end.AddRange( type );
                        end.Add( new Token( Name = Name.GetSimpleType( false ), TokenType.Word ) );
                        end.Add( Acc.Bracket_Start[ 0 ] );
                        end.AddRange( build );
                        end.Add( Acc.Bracket_End[ 0 ] );
                        end.Add( CurrentPacket.X );
                        mustCrack( Acc.Block_Start );
                        int i = 1;
                        while ( i > 0 ) {
                            if ( Crack( Acc.Block_Start ) ) {
                                i++;
                                end.Add( CurrentPacket[ CurrentPacket.Index - 1 ] );
                                continue;
                            }
                            if ( Crack( Acc.Block_End ) ) {
                                i--;
                                end.Add( CurrentPacket[ CurrentPacket.Index - 1 ] );
                                continue;
                            }
                            end.Add( CurrentPacket.X );
                            CurrentPacket++;
                        }
                        end.Add( Acc.Block_End[ 0 ] );
                        Phrases.Add( pp );
                    #endregion
                        #region ShortCut
                    } else if ( Crack( Acc.ShortCut ) ) {
                        mustCrack( Acc.Bracket_Start );
                        int i = 1;
                        List<Token> listToken = new List<Token>( ), listToken2 = new List<Token>( );
                        while ( i > 0 ) {
                            if ( Crack( Acc.Bracket_Start ) ) {
                                i++;
                                listToken.Add( CurrentPacket[ CurrentPacket.Index - 1 ] );
                                continue;
                            }
                            if ( Crack( Acc.Bracket_End ) ) {
                                i--;
                                listToken.Add( CurrentPacket[ CurrentPacket.Index - 1 ] );
                                continue;
                            }
                            listToken.Add( CurrentPacket.X );
                            CurrentPacket++;
                        }
                        listToken.RemoveAt( listToken.Count - 1 );
                        mustCrack( Acc.Assign );
                        mustCrack( Acc.Bracket_Start );
                        i = 1;
                        while ( i > 0 ) {
                            if ( Crack( Acc.Bracket_Start ) ) {
                                i++;
                                listToken2.Add( CurrentPacket[ CurrentPacket.Index - 1 ] );
                                continue;
                            }
                            if ( Crack( Acc.Bracket_End ) ) {
                                i--;
                                listToken2.Add( CurrentPacket[ CurrentPacket.Index - 1 ] );
                                continue;
                            }
                            listToken2.Add( CurrentPacket.X );
                            CurrentPacket++;
                        }
                        listToken2.RemoveAt( listToken2.Count - 1 );
                        Detect.Add( listToken );
                        Replacement.Add( listToken2 );
                        continue;
                        #endregion
                    } else {
                        lt.Add( XMT.X );
                        XMT++;
                    }
                }
                lt.AddRange( end );
                return new XManager<Token>( lt );
            }
            public Token[ ] CrackIdentTokens( bool newtypes = false ) {
                List<Token> ident_build = new List<Token>( );
                Crack( TokenType.Word );
                ident_build.Add( CurrentPacket[ CurrentPacket.Index - 1 ] );
                while ( Crack( Acc.Bracket_Split ) ) {
                    ident_build.Add( CurrentPacket[ CurrentPacket.Index - 1 ] );
                    Crack( TokenType.Word );
                    ident_build.Add( CurrentPacket[ CurrentPacket.Index - 1 ] );
                }
                return ident_build.ToArray( );
            }
            public List<Token> Precompiling( XManager<Token> XMT ) {
                CurrentPacket = XMT;
                List<Token> preresult = new List<Token>( ), result = new List<Token>( );
                while ( !XMT.Eof ) {
                    int index = CurrentPacket.Index;
                    #region Import
                    try {
                        if ( Crack( Acc.Import ) ) {
                            mustCrack( Acc.Bracket_Start );
                            string language = Crack( TokenType.String ).ToLower( ).Replace( "\"", "" );
                            mustCrack( Acc.Bracket_End );
                            if ( language == "c#" )
                                throw new Exception( );
                            int line = CurrentPacket.X.LineNum, pos = 1;
                            mustCrack( Acc.Block_Start );
                            int i = 1;
                            string code = "";
                            while ( i > 0 ) {
                                if ( Crack( Acc.Block_Start ) ) {
                                    CurrentPacket.Index--;
                                    i++;
                                }
                                if ( Crack( Acc.Block_End ) ) {
                                    CurrentPacket.Index--;
                                    i--;
                                }
                                if ( i <= 0 ) {
                                    CurrentPacket++;
                                    break;
                                }
                                if ( line != CurrentPacket.X.LineNum ) {
                                    code += "\r\n";
                                    line = CurrentPacket.X.LineNum;
                                    pos = 1;
                                }
                                code += ( CurrentPacket.X.Pos - pos > 0 ? new string( ' ', CurrentPacket.X.Pos - pos ) : "" ) + CurrentPacket.X.Value;
                                pos = CurrentPacket.X.Pos + CurrentPacket.X.Value.Length;
                                CurrentPacket++;
                            }
                            if ( language == "c" ) {
                                File.WriteAllText( Path.Combine( CompilingDirectory, Path.GetRandomFileName( ) + ".C" ), code );
                            } else if ( language == "go" ) {
                                File.WriteAllText( Path.Combine( CompilingDirectory, Path.GetRandomFileName( ) + ".go" ), "package " + Package + "\r\n\r\n" + code );
                            }
                            continue;
                        }
                    } catch {
                        CurrentPacket.Index = index;
                    }
                    #endregion
                    #region Phrase
                    foreach ( PhraseParser phrase in Phrases ) {
                        CurrentPacket.Index = index;
                        List<Token> parameters = new List<Token>( );
                        bool success = true;
                        object lastobj = null;
                        foreach ( object obj in phrase.Keys ) {
                            if ( obj is Token ) {
                                if ( !Crack( ( Token )obj ) ) {
                                    success = false;
                                    break;
                                }
                            } else if ( obj is PhraseParser.PhraseType ) {
                                try {
                                    switch ( ( PhraseParser.PhraseType )obj ) {
                                        case PhraseParser.PhraseType.Expression:
                                            int ind = CurrentPacket.Index, count = 0;
                                            CrackExpression( );
                                            count = CurrentPacket.Index - ind;
                                            if ( parameters.Count != 0 )
                                                parameters.Add( Acc.Bracket_Split[ 0 ] );
                                            parameters.AddRange( CurrentPacket.GetRange( ind, count ) );
                                            break;

                                        case PhraseParser.PhraseType.Code:

                                            break;
                                    }
                                } catch {
                                    success = false;
                                    break;
                                }
                            }
                            lastobj = obj;
                        }
                        if ( !success ) {
                            CurrentPacket.Index = index;
                            continue;
                        }
                        parameters.Insert( 0, Acc.Bracket_Start[ 0 ] );
                        parameters.Insert( 0, new Token( phrase.InternalLocation, TokenType.Word ) );
                        parameters.Add( Acc.Bracket_End[ 0 ] );
                        preresult.AddRange( parameters );
                        index = -1;
                        break;
                    }
                    if ( index == -1 )
                        continue;
                    #endregion
                    #region ShortCut

                    #endregion
                    preresult.Add( XMT.X );
                    XMT++;
                }
                int c = Replacement.Count - 1, c2 = preresult.Count, c3;
                for ( int i = 0 ; i < c ; i++ ) {
                    if ( Detect[ i ].Count < Detect[ i + 1 ].Count ) {
                        Detect.Insert( i, Detect[ i + 1 ] );
                        Replacement.Insert( i, Replacement[ i + 1 ] );
                        Detect.RemoveAt( i + 1 );
                        Replacement.RemoveAt( i + 1 );
                    }
                }
                c++;
                c2 = preresult.Count;
                for ( int i2 = 0 ; i2 < c2 ; i2++ ) {
                    bool _continue = false;
                    for ( int i = 0 ; i < c ; i++ ) {
                        List<Token> _detect = Detect[ i ];
                        c3 = _detect.Count;
                        if ( c2 - i2 - c3 <= 0 )
                            continue;
                        _continue = true;
                        for ( int i3 = 0 ; i3 < c3 ; i3++ ) {
                            if ( _detect[ i3 ] != preresult[ i2 + i3 ] ) {
                                _continue = false;
                                break;
                            }
                        }
                        if ( _continue ) {
                            result.AddRange( Replacement[ i ] );
                            break;
                        }
                    }
                    if ( _continue )
                        continue;
                    result.Add( preresult[ i2 ] );
                }
                return result;
            }
            public void mustCrack( List<Token> lt ) {
                if ( !Crack( lt ) )
                    throw new CodeErrorException( CurrentPacket.X, CurrentPacket.Tag, "expected '" + lt[ 0 ].Value + "'" );
            }
        }
        #endregion
        #region Classes
        public class PhraseParser {
            public List<Object> Keys {
                get;
                set;
            }
            public string InternalLocation {
                get;
                set;
            }
            public PhraseParser( ) {
                Keys = new List<object>( );
                InternalLocation = "";
            }
            public enum PhraseType {
                Expression,
                Code,
                None
            }
        }
        public class ParserStyle : Dictionary<string, List<Token>> {
            public void Add( string KeyNValue ) {
                base.Add( KeyNValue, new List<Token>( new Token[ ] { new Token( KeyNValue, TokenType.Keyword ) } ) );
            }
            public void Add( string Key, string Value ) {
                base.Add( Key, new List<Token>( new Token[ ] { new Token( Value, TokenType.Keyword ) } ) );
            }
            public void Add( string Key, Token Value ) {
                base.Add( Key, new List<Token>( new Token[ ] { Value } ) );
            }
            public void AddRange( params string[ ] KeysNValues ) {
                foreach ( string KeyNValue in KeysNValues )
                    Add( KeyNValue );
            }
            public void AddSymbols( params string[ ] Symbols ) {
                foreach ( string symbol in Symbols )
                    base.Add( symbol, new List<Token>( new Token[ ] { new Token( symbol, TokenType.Symbol ) } ) );
            }
            public new bool ContainsKey( string Key ) {
                foreach ( string key in Keys )
                    if ( key == Key )
                        return true;
                return false;
            }
        }
        public class Source {
            public Source( ) {
                Name = Path.GetRandomFileName( );
                ContentBoostParts = new List<string>( );
                Content = "";
            }
            string __content = "";
            public string Name {
                get;
                private set;
            }
            public string Content {
                get {
                    Rebuild( );
                    return __content;
                }
                set {
                    __content = value;
                    ContentBoostParts.Clear( );
                }
            }
            public List<String> ContentBoostParts {
                get;
                set;
            }
            public String LastContentPart {
                get {
                    if ( ContentBoostParts.Count == 0 )
                        return Content;
                    else
                        return ContentBoostParts.Last( );
                }
                set {
                    if ( ContentBoostParts.Count == 0 )
                        Content = value;
                    else
                        ContentBoostParts[ ContentBoostParts.Count - 1 ] = value;
                }
            }
            public void Append( string SubContent ) {
                ContentBoostParts.Add( SubContent );
            }
            public void AppendLine( string SubContent ) {
                if ( ContentBoostParts.Count == 0 ) {
                    if ( !Content.EndsWith( "\r\n" ) && Content == "" )
                        SubContent = "\r\n" + SubContent;
                } else {
                    if ( !LastContentPart.EndsWith( "\r\n" ) )
                        SubContent = "\r\n" + SubContent;
                }
                ContentBoostParts.Add( SubContent );
            }
            public void AppendLine( ) {
                if ( ContentBoostParts.Count == 0 ) {
                    if ( !Content.EndsWith( "\r\n" ) && Content == "" )
                        ContentBoostParts.Add( "\r\n" );
                } else {
                    if ( !LastContentPart.EndsWith( "\r\n" ) )
                        ContentBoostParts.Add( "\r\n" );
                }
            }
            public void Rebuild( ) {
                if ( ContentBoostParts.Count == 0 )
                    return;
                __content += string.Join( null, ContentBoostParts.ToArray( ) );
                ContentBoostParts.Clear( );
            }
        }
        public class ParserFilter {
            Stack<bool> _method = new Stack<bool>( ), _namespace = new Stack<bool>( );
            Stack<string> _path = new Stack<string>( );
            Stack<int> _path_close = new Stack<int>( );
            List<String> _vars = new List<string>( );
            public ParserFilter( ) {
                Path = "";
                canMethod = false;
            }
            public bool canMethod {
                get {
                    return _method.Peek( );
                }
                set {
                    _method.Push( value );
                }
            }
            public bool canNamespace {
                get {
                    return _namespace.Peek( );
                }
                set {
                    _namespace.Push( value );
                }
            }
            public string Path {
                get {
                    return _path.Peek( );
                }
                set {
                    _path.Push( value );
                    _path_close.Push( _vars.Count );
                }
            }
            public void PopNamespace( ) {
                _namespace.Pop( );
            }
            public void PopPath( ) {
                _path.Pop( );
                int pop = _path_close.Pop( );
                _vars.RemoveRange( pop, _vars.Count - pop );
            }
            public void PopMethod( ) {
                _method.Pop( );
            }
            public bool Contains( string _var ) {
                foreach ( string str in _vars )
                    if ( str == _var )
                        return true;
                return false;
            }
        }
        public class CodeErrorException : Exception {
            public string _source, _text;
            public int _linenum, _pos;

            public CodeErrorException( int linenum, int pos, string source, string text ) {
                _linenum = linenum;
                _pos = pos;
                _source = source;
                _text = text;
            }
            public CodeErrorException( Token token, string source, string text )
                : this( token.LineNum, token.Pos - token.Value.Length, source, text + " (On Token '" + token.Value + "')" ) {

            }
            override public string Message {
                get {
                    return "At File " + _source + " At Position " + _linenum + " ," + _pos + " " + _text;
                }
            }
        }
        public class ParserAcc {
            ParserStyle ps;
            public List<Token> Block_Start, Block_End, Class, Enum, Namespace, Using, Bracket_Start, Bracket_End, Bracket_Split, Inherit, EndStmt,
                Static, Interface, Method, Ident, Add, Sub, Mul, Div, Greater, Lesser, While, For, If, Else, Loop, Return, Asg_Greater, Asg_Lesser, Assign, Asg_Add,
                Asg_Sub, Asg_Mul, Asg_Div, And, AndTwo, Or, OrTwo, Xor, XorTwo, False, True, Half, Percent, New, This, Incre, Decre, Equal, Operator, Import,
                ShortCut, Phrase, NewLine, PhraseArgs, PhraseCode, Array_Start, Array_End, Bind, ExternBody, SplitThread,Finally;
            public ParserAcc( ParserStyle ps ) {
                if ( ps == null )
                    return;
                this.ps = ps;
                Block_Start = GetList( "{" );
                Block_End = GetList( "}" );
                Class = GetList( "class" );
                Enum = GetList( "enum" );
                Namespace = GetList( "namespace" );
                Using = GetList( "using" );
                Bracket_Start = GetList( "(" );
                Bracket_End = GetList( ")" );
                Bracket_Split = GetList( "," );
                Inherit = GetList( "inherit" );
                EndStmt = GetList( ";" );
                Static = GetList( "static" );
                Interface = GetList( "interface" );
                Method = GetList( "method" );
                Ident = GetList( "." );
                Add = GetList( "+" );
                Sub = GetList( "-" );
                Mul = GetList( "*" );
                Div = GetList( "/" );
                Greater = GetList( "<" );
                Lesser = GetList( ">" );
                While = GetList( "while" );
                For = GetList( "for" );
                If = GetList( "if" );
                Else = GetList( "else" );
                Loop = GetList( "loop" );
                Return = GetList( "return" );
                Asg_Greater = GetList( "<=" );
                Asg_Lesser = GetList( ">=" );
                Assign = GetList( "=" );
                Asg_Add = GetList( "+=" );
                Asg_Sub = GetList( "-=" );
                Asg_Mul = GetList( "*=" );
                Asg_Div = GetList( "/=" );
                And = GetList( "&" );
                AndTwo = GetList( "&&" );
                Or = GetList( "|" );
                OrTwo = GetList( "||" );
                Xor = GetList( "^" );
                XorTwo = GetList( "^^" );
                False = GetList( "false" );
                True = GetList( "true" );
                Half = GetList( "half" );
                Percent = GetList( "percent" );
                New = GetList( "new" );
                This = GetList( "this" );
                Incre = GetList( "++" );
                Decre = GetList( "--" );
                Equal = GetList( "==" );
                Operator = GetList( "operator" );
                Import = GetList( "import" );
                ShortCut = GetList( "shortcut" );
                Phrase = GetList( "phrase" );
                NewLine = GetList( "_" );
                PhraseArgs = GetList( "#" );
                PhraseCode = GetList( "@" );
                ExternBody = GetList( "@" );
                Array_Start = GetList( "[" );
                Array_End = GetList( "]" );
                Bind = GetList( "bind" );
                SplitThread = GetList( "split" );
                Finally = GetList( "finally" );
            }
            public List<Token> GetList( string Name ) {
                try {
                    return ps[ Name ];
                } catch {
                    return null;
                }
            }
        }
        public class ErrorCollection : List<CodeErrorException> {
        }
        #endregion
    }
}