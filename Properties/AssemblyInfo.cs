using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("AE.Net.Mail")]
[assembly: AssemblyDescription("This is a fork of Andy Edinborough's project. To avoid name conflicts, and to apply our devOp automation, we append a prefix, with full respect for the original author")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Andy Edinborough")]
[assembly: AssemblyProduct("AE.Net.Mail")]
[assembly: AssemblyCopyright("Original Author and All Contributors")]
[assembly: AssemblyTrademark("AE.Net.Mail by Andy Edinborough")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c6666837-f9c8-48d6-8c60-42419405859e")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.8")]  // keep the version stable, as even the build part of the version change will make the winform designer fail to render controls. Rather, we can change the nuget package version instead.
[assembly: AssemblyFileVersion("1.7.10.0")]
//[assembly: InternalsVisibleTo("Tests")]