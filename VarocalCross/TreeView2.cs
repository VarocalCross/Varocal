using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VarocalCross
{
	[ToolboxBitmap( typeof( TreeView2 ) )]
	public class TreeView2 : System.Windows.Forms.TreeView
	{
		[DllImport( "user32.dll", CharSet = CharSet.Unicode )]
		public static extern int SendMessage( IntPtr hWnd, uint Msg, int wParam, int lParam );
		[DllImport( "user32.dll", CharSet = CharSet.Auto )]
		public static extern IntPtr SendMessage( IntPtr hWnd, int msg, int wParam, int lParam );
		[DllImport( "user32.dll", CharSet = CharSet.Unicode )]
		public static extern IntPtr SendMessage( IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam );
		[DllImport( "uxtheme.dll", CharSet = CharSet.Unicode )]
		public static extern int SetWindowTheme( IntPtr hWnd, string pszSubAppName, string pszSubIdList );
		protected override void OnHandleCreated( EventArgs e )
		{
			base.OnHandleCreated( e );
			SetWindowTheme( Handle, "explorer", null );
			SendMessage( Handle, 4396, 0, SendMessage( Handle, Convert.ToUInt32( 4397 ), 0, 0 )|100 );
		}
	}
}
