***REMOVED***
***REMOVED***
***REMOVED***
using System.Text;
using System.Threading.Tasks;

***REMOVED***
***REMOVED***
    public class JiraSettings
    ***REMOVED***
***REMOVED***   public string UserID ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public string Pass ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public string Url ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public string Project ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string EpicLinkField ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string SprintField ***REMOVED*** get; internal set; ***REMOVED*** 
***REMOVED***   public string StoryPointsField ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string UserMappingFile ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public int BatchSize ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string AttachmentsDir ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string JQL ***REMOVED*** get; internal set; ***REMOVED***

***REMOVED***   public JiraSettings(string userID, string pass, string url, string project)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  UserID = userID;
***REMOVED******REMOVED***  Pass = pass;
***REMOVED******REMOVED***  Url = url;
***REMOVED******REMOVED***  Project = project;
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***