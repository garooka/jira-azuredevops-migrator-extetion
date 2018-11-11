namespace WorkItemImport
***REMOVED***
    public class Settings
    ***REMOVED***
***REMOVED***   public Settings(string account, string project, string pat)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  Account = account;
***REMOVED******REMOVED***  Project = project;
***REMOVED******REMOVED***  Pat = pat;
***REMOVED***   ***REMOVED***

***REMOVED***   public string Account ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public string Project ***REMOVED*** get; private set; ***REMOVED******REMOVED***   
***REMOVED***   public string Pat ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public string BaseAreaPath ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string BaseIterationPath ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public bool IgnoreFailedLinks ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string ProcessTemplate ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***
***REMOVED***