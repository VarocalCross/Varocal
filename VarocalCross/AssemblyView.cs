using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mono.Cecil;

namespace VarocalCross {
    public partial class AssemblyView : BaseForm {
        List<string> ls = new List<string>( );
        public AssemblyView( string[] Paths ) {
            InitializeComponent( );
            ls.AddRange( Paths );
        }
        private void AssemblyView_Load( object sender, EventArgs e ) {
            foreach ( string path in ls ) {
                ExecutableAnalied assembly = new ExecutableAnalied( path );
                TreeNodeCollection basecollection = ( ls.Count == 1 ? treeView21.Nodes
                    : treeView21.Nodes.Add( assembly.Source.Name.Name ).Nodes );
                foreach ( TypeAnalied type in assembly.Types ) {
                    Apply( type,basecollection );
                }
            }
        }
        void Apply( TypeAnalied type, TreeNodeCollection tnc ) {
            if ( type.Parent == null ) {
                foreach ( TreeNode v in tnc ) {
                    if ( v.Text == type.Source.Namespace ) {
                        tnc = v.Nodes;
                        break;
                    }
                    if ( tnc.IndexOf( v ) == tnc.Count - 1 ) {
                        tnc = tnc.Add( type.Source.Namespace ).Nodes;
                        break;
                    }
                }
                if ( tnc.Count == 0 )
                    tnc = tnc.Add( type.Source.Namespace ).Nodes;
            }
            TreeNode node = new TreeNode( type.Source.Name );
            tnc.Add( node );
            foreach ( VariableAnalied var_ana in type.Variables )
                Apply( var_ana, node.Nodes );
            foreach ( MethodAnalied var_ana in type.Methods )
                Apply( var_ana, node.Nodes );
            foreach ( TypeAnalied typ_ana in type.Types )
                Apply( typ_ana, node.Nodes );
        }
        void Apply( VariableAnalied var_ana, TreeNodeCollection tnc ) {
            tnc.Add( var_ana.Source.Name + " : " + var_ana.Type.Source.Name + ( var_ana.isStatic ? " (static)" : "" ) );
        }
        void Apply( MethodAnalied met_ana, TreeNodeCollection tnc ) {
            tnc.Add( met_ana.Source.Name + " : " + met_ana.ReturnType.Source.Name + "( " + string.Join( ", ", ( from v in met_ana.Parameters select v.Type.Source.Name ) ) + ( met_ana.Parameters.Count() == 0 ? ")" : " )" ) + ( met_ana.isStatic ? " (static)" : "" ) );
        }
        private void button1_Click( object sender, EventArgs e ) {
            Close( );
        }
    }
}
