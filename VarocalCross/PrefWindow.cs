using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;

namespace VarocalCross
{
	public partial class PrefWindow : Form
	{
		public List<bool> bold=new List<bool>();
		public List<bool> italic = new List<bool>( );
		public List<Color> colors = new List<Color>( );
		public Font f;

		public int getIndex( string str )
		{
			return code_colors.Items.IndexOf( str );
		}
		public PrefWindow( )
		{
			InitializeComponent( );
			f = new Font( "Consolas", 10, FontStyle.Regular );
			foreach ( string item in code_colors.Items )
			{
				bold.Add( false );
				italic.Add( false );
				colors.Add( Color.Black );
			}
            try {
                if ( File.Exists( "colors.dat" ) ) {
                    string[ ] lines = File.ReadAllLines( "colors.dat" );
                    int ind = 0;
                    int c = code_colors.Items.Count;
                    for ( int i = 0 ; i < c ; i++ ) {
                        colors[ i ] = Color.FromArgb( int.Parse( lines[ ind++ ] ) );
                        bold[ i ] = bool.Parse( lines[ ind++ ] );
                        italic[ i ] = bool.Parse( lines[ ind++ ] );
                    }
                    string fontname = lines[ ind++ ];
                    string fontsize = lines[ ind++ ];
                    f = new Font( fontname, float.Parse( fontsize ), FontStyle.Regular );
                }
                if ( File.Exists( "other.dat" ) ) {
                    string[ ] lines = File.ReadAllLines( "other.dat" );
                    int ind = 0;
                    tab_spaces.Text = lines[ ind++ ];
                    paint_delay.Text = lines[ ind++ ];
                    recently_number.Text = lines[ ind++ ];
                    last_startup.Checked = bool.Parse( lines[ ind++ ] );
                    backup.Checked = bool.Parse( lines[ ind++ ] );
                    backup_number.Text = lines[ ind++ ];
                    update_none.Checked = bool.Parse( lines[ ind++ ] );
                    update_reminds.Checked = bool.Parse( lines[ ind++ ] );
                    update_download_ask.Checked = bool.Parse( lines[ ind++ ] );
                    update_download_startup.Checked = bool.Parse( lines[ ind++ ] );
                }
            } catch {
            }
			code_colors.SelectedIndex = 0;
		}

		public void Save( )
		{
			if ( File.Exists( "colors.dat" ) )
				File.Delete( "colors.dat" );
			StreamWriter SW = new StreamWriter( "colors.dat" );
			int c = code_colors.Items.Count;
			for ( int i = 0 ; i < c ; i++ )
			{
				SW.WriteLine( colors[ i ].ToArgb( ) );
				SW.WriteLine( bold[i] );
				SW.WriteLine( italic[ i ] );
			}
			SW.WriteLine( f.Name );
			SW.WriteLine( f.Size );
			SW.Close( );
			if ( File.Exists( "other.dat" ) )
				File.Delete( "other.dat" );
			SW = new StreamWriter( "other.dat" );
			SW.WriteLine( tab_spaces.Text );
			SW.WriteLine( paint_delay.Text );
			SW.WriteLine( recently_number.Text );
			SW.WriteLine( last_startup.Checked.ToString() );
			SW.WriteLine( backup.Checked.ToString( ) );
			SW.WriteLine( backup_number.Text );
			SW.WriteLine( update_none.Checked.ToString( ) );
			SW.WriteLine( update_reminds.Checked.ToString( ) );
			SW.WriteLine( update_download_ask.Checked.ToString( ) );
			SW.WriteLine( update_download_startup.Checked.ToString( ) );
			SW.Close( );
		}

		public void rect_change_Click( object sender, EventArgs e )
		{
			ColorDialog CD = new ColorDialog( );
			CD.Color = rect_change.BackColor;
			if ( CD.ShowDialog( ) == System.Windows.Forms.DialogResult.OK )
			{
				rect_change.BackColor = CD.Color;
				rect_change.ForeColor = Color.FromArgb( ( CD.Color.R + 128 ) % 255, ( CD.Color.G + 128 ) % 255, ( CD.Color.B + 128 ) % 255 );
				colors[ code_colors.SelectedIndex ] = CD.Color;
			}
		}

		public void code_colors_SelectedIndexChanged( object sender, EventArgs e )
		{
			rect_change.BackColor = colors[ code_colors.SelectedIndex ];
			rect_change.ForeColor = Color.FromArgb( ( colors[ code_colors.SelectedIndex ].R + 128 ) % 255, ( colors[ code_colors.SelectedIndex ].G + 128 ) % 255, ( colors[ code_colors.SelectedIndex ].B + 128 ) % 255 );
			text_bold.Checked = bold[ code_colors.SelectedIndex ];
			text_italic.Checked = italic[ code_colors.SelectedIndex ];
		}

		public void but_font_Click( object sender, EventArgs e )
		{
			FontDialog FD = new FontDialog( );
			FD.Font = f;
			FD.ShowColor = false;
			FD.ShowApply = false;
			FD.ShowEffects = false;
			FD.ScriptsOnly = false;
			FD.ShowHelp = false;
			FD.FontMustExist = true;
			if ( FD.ShowDialog( ) == System.Windows.Forms.DialogResult.OK )
				f = new Font( FD.Font.Name, FD.Font.Size,FontStyle.Regular );
		}

		public void PrefWindow_FormClosing( object sender, FormClosingEventArgs e )
		{
			e.Cancel = true;
			Hide( );
		}

		public void PrefWindow_Load( object sender, EventArgs e )
		{
			faTabStripItem2.BackColor = faTabStripItem2.BackColor = this.BackColor;
		}

		public void text_bold_CheckedChanged( object sender, EventArgs e )
		{
			bold[ code_colors.SelectedIndex ] = text_bold.Checked;
		}

		public void text_italic_CheckedChanged( object sender, EventArgs e )
		{
			italic[ code_colors.SelectedIndex ] = text_italic.Checked;
		}
	}
}
