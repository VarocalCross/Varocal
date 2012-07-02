
using System;
namespace System {
[System.DllField(1862605843)]public partial class Object {
public static object[] GetAttibutes() {return new object[] { };}
}
[System.DllField(1862605843)]public partial class CFunctionAttribute : Attribute {
public static object[] GetAttibutes() {return new object[] { };}
}
[System.DllField(1862605843)]public partial class LanguageFunctions {
public static object[] GetAttibutes() {return new object[] { };}
public static object[] GetAttibutesSplit() {return new object[] { };}
public static void Split( Action action ) {
}
public static object[] GetAttibutesFinally() {return new object[] { };}
public static void Finally( Action action ) {
}
}
[System.DllField(1862605843)]public partial class JunkClass {
public static object[] GetAttibutes() {return new object[] { };}
public static object[] GetAttibutesSystem_Thread_SetCount() {return new object[] { };}
extern public static void System_Thread_SetCount( int c );
public static object[] GetAttibutesSystem_Console_WriteLine() {return new object[] { };}
extern public static void System_Console_WriteLine( string text );
public static object[] GetAttibutesadd() {return new object[] { };}
[System.CFunction]
extern public static void add( int a, int b );
}
[System.DllField(1862605843)]public partial class Thread {
public static object[] GetAttibutes() {return new object[] { };}
public static object[] GetAttibutesSetCount() {return new object[] { };}
public static void SetCount( int Count ) {
JunkClass.System_Thread_SetCount( Count );
}
}
[System.DllField(1862605843)]public partial class Console {
public static object[] GetAttibutes() {return new object[] { };}
public static object[] GetAttibutesWriteLine() {return new object[] { };}
public static void WriteLine( string text ) {
JunkClass.System_Console_WriteLine( text );
}
}
[System.DllField(1862605843)]public partial class DllFieldAttribute : Attribute {
public static object[] GetAttibutes() {return new object[] { };}
public DllFieldAttribute( int ID ) {
}
}
}