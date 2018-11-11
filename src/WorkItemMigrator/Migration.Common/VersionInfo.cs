***REMOVED***
***REMOVED***
using System.Diagnostics;
***REMOVED***
using System.Text;
using System.Threading.Tasks;

namespace Migration.Common
***REMOVED***
    public class VersionInfo
    ***REMOVED***
***REMOVED***   public static string GetVersionInfo()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly();
***REMOVED******REMOVED***  FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
***REMOVED******REMOVED***  string version = fvi.ProductVersion;
***REMOVED******REMOVED***  return version;
***REMOVED***   ***REMOVED***

***REMOVED***   public static string GetCopyrightInfo()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly();
***REMOVED******REMOVED***  FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
***REMOVED******REMOVED***  return fvi.LegalCopyright;
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***
