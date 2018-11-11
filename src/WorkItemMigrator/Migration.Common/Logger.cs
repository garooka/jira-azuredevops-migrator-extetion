***REMOVED***
***REMOVED***
***REMOVED***

namespace Migration.Common
***REMOVED***
    public enum LogLevel
    ***REMOVED***
***REMOVED***   Debug,
***REMOVED***   Info,
***REMOVED***   Warning,
***REMOVED***   Error,
***REMOVED***   Critical
***REMOVED***

    public class Logger
    ***REMOVED***
***REMOVED***   private static string _logFilePath;
***REMOVED***   private static LogLevel _logLevel;
***REMOVED***   private static List<string> Errors = new List<string>();
***REMOVED***   private static List<string> Warnings = new List<string>();

***REMOVED***   public static void Init(string dirPath, LogLevel level)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if(!Directory.Exists(dirPath))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Directory.CreateDirectory(dirPath);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  _logFilePath = Path.Combine(dirPath, $"log.***REMOVED***Guid.NewGuid().ToString()***REMOVED***.txt");
***REMOVED******REMOVED***  _logLevel = level;
***REMOVED***   ***REMOVED***

***REMOVED***   internal static void Init(MigrationContext instance)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  Init(instance.MigrationWorkspace, instance.LogLevel);
***REMOVED***   ***REMOVED***

***REMOVED***   public static void Log(LogLevel level, string message)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  LogInternal(level, message);

***REMOVED******REMOVED***  if (level == LogLevel.Critical)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Errors.Add(message);
***REMOVED******REMOVED******REMOVED*** throw new AbortMigrationException(message);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  else if (level == LogLevel.Error)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Errors.Add(message);
***REMOVED******REMOVED******REMOVED*** Console.Write("Do you want to continue (y/n)? ");
***REMOVED******REMOVED******REMOVED*** var answer = Console.ReadKey();
***REMOVED******REMOVED******REMOVED*** if (answer.Key == ConsoleKey.N)
***REMOVED******REMOVED******REMOVED******REMOVED***throw new AbortMigrationException(message);

***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  else if (level == LogLevel.Warning)
***REMOVED******REMOVED******REMOVED*** Warnings.Add(message);
***REMOVED***   ***REMOVED***

***REMOVED***   private static void LogInternal(LogLevel level, string message)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  ToFile(level, message);

***REMOVED******REMOVED***  if ((int)level >= (int)_logLevel)
***REMOVED******REMOVED******REMOVED*** ToConsole(level, message);
***REMOVED***   ***REMOVED***

***REMOVED***   public static void Log(Exception ex)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  Log(LogLevel.Error, $"[***REMOVED***ex.GetType().ToString()***REMOVED***] ***REMOVED***ex.Message***REMOVED***: ***REMOVED***Environment.NewLine + ex.StackTrace***REMOVED***");
***REMOVED***   ***REMOVED***

***REMOVED***   private static void ToFile(LogLevel level, string message)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  string levelPrefix = GetPrefixFromLogLevel(level);
***REMOVED******REMOVED***  string dateTime = DateTime.Now.ToString("HH:mm:ss");

***REMOVED******REMOVED***  string log = $"[l:***REMOVED***levelPrefix***REMOVED***][d:***REMOVED***dateTime***REMOVED***] ***REMOVED***message***REMOVED******REMOVED***Environment.NewLine***REMOVED***";
***REMOVED******REMOVED***  if (_logFilePath != null)
***REMOVED******REMOVED******REMOVED*** File.AppendAllText(_logFilePath, log);
***REMOVED***   ***REMOVED***

***REMOVED***   private static void ToConsole(LogLevel level, string message)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if ((int)level >= (int)_logLevel)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Console.ForegroundColor = GetColorFromLogLevel(level);
***REMOVED******REMOVED******REMOVED******REMOVED***Console.WriteLine(message);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  finally
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Console.ResetColor();
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private static ConsoleColor GetColorFromLogLevel(LogLevel level)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  switch (level)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** case LogLevel.Debug: return ConsoleColor.Gray;
***REMOVED******REMOVED******REMOVED*** case LogLevel.Info: return ConsoleColor.White;
***REMOVED******REMOVED******REMOVED*** case LogLevel.Warning: return ConsoleColor.Yellow;
***REMOVED******REMOVED******REMOVED*** case LogLevel.Error:
***REMOVED******REMOVED******REMOVED*** case LogLevel.Critical: return ConsoleColor.Red;
***REMOVED******REMOVED******REMOVED*** default: return ConsoleColor.Gray;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED***   ***REMOVED***

***REMOVED***   private static string GetPrefixFromLogLevel(LogLevel level)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  switch (level)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** case LogLevel.Debug: return "D";
***REMOVED******REMOVED******REMOVED*** case LogLevel.Info: return "I";
***REMOVED******REMOVED******REMOVED*** case LogLevel.Warning: return "W";
***REMOVED******REMOVED******REMOVED*** case LogLevel.Error: return "E";
***REMOVED******REMOVED******REMOVED*** case LogLevel.Critical: return "C";
***REMOVED******REMOVED******REMOVED*** default: return "I";
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   public static void Summary()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  foreach (var warning in Warnings)
***REMOVED******REMOVED******REMOVED*** LogInternal(LogLevel.Warning, warning);

***REMOVED******REMOVED***  foreach (var error in Errors)
***REMOVED******REMOVED******REMOVED*** LogInternal(LogLevel.Error, error);
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***
