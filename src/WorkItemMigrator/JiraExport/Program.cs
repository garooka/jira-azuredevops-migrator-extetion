using Migration.Common;
***REMOVED***
***REMOVED***
***REMOVED***
***REMOVED***
using System.Text;
using System.Threading.Tasks;

***REMOVED***
***REMOVED***
    class Program
    ***REMOVED***
***REMOVED***   static void Main(string[] args)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  Logger.Log(LogLevel.Info, $"Jira Exporter v***REMOVED***VersionInfo.GetVersionInfo()***REMOVED***");
***REMOVED******REMOVED***  Logger.Log(LogLevel.Info, VersionInfo.GetCopyrightInfo());

***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var cmd = new JiraCommandLine(args);
***REMOVED******REMOVED******REMOVED*** cmd.Run();
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (Exception ex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, "Closing application due to an unexpected exception: " + ex.Message);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  finally
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Summary();
***REMOVED******REMOVED***  ***REMOVED***

#if DEBUG
***REMOVED******REMOVED***  Console.WriteLine("Press any key to continue...");
***REMOVED******REMOVED***  Console.ReadKey();
#endif

***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***
