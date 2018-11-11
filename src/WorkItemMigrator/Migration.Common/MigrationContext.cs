using Migration.WIContract;
***REMOVED***
***REMOVED***
***REMOVED***

namespace Migration.Common
***REMOVED***
    public class MigrationContext
    ***REMOVED***
***REMOVED***   public static MigrationContext Instance
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  get;
***REMOVED******REMOVED***  private set;
***REMOVED***   ***REMOVED***

***REMOVED***   public string AttachmentsPath ***REMOVED*** get ***REMOVED*** return Path.Combine(MigrationWorkspace, "Attachments"); ***REMOVED*** ***REMOVED***
***REMOVED***   public string UserMappingPath ***REMOVED*** get ***REMOVED*** return Path.Combine(MigrationWorkspace, "users.txt"); ***REMOVED*** ***REMOVED***
***REMOVED***   public Dictionary<string, string> UserMapping ***REMOVED*** get; private set; ***REMOVED*** = new Dictionary<string, string>();

***REMOVED***   public static MigrationContext Init(string workspacePath, LogLevel logLevel, bool forceFresh)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  Instance = new MigrationContext(workspacePath);
***REMOVED******REMOVED***  Instance.LogLevel = logLevel;
***REMOVED******REMOVED***  Instance.ForceFresh = forceFresh;

***REMOVED******REMOVED***  Logger.Init(Instance);
***REMOVED******REMOVED***  Instance.Journal = Journal.Init(Instance);
***REMOVED******REMOVED***  Instance.Provider = new WiItemProvider(Instance.MigrationWorkspace);

***REMOVED******REMOVED***  if (!Directory.Exists(Instance.AttachmentsPath))
***REMOVED******REMOVED******REMOVED*** Directory.CreateDirectory(Instance.AttachmentsPath);

***REMOVED******REMOVED***  return Instance;
***REMOVED***   ***REMOVED***
***REMOVED***   private MigrationContext(string workspacePath)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  MigrationWorkspace = workspacePath;
***REMOVED******REMOVED***  ParseUserMappings(UserMappingPath);
***REMOVED***   ***REMOVED***

***REMOVED***   private void ParseUserMappings(string userMappingPath)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (!File.Exists(userMappingPath))
***REMOVED******REMOVED******REMOVED*** return;

***REMOVED******REMOVED***  string[] userMappings = File.ReadAllLines(userMappingPath);
***REMOVED******REMOVED***  foreach (var userMapping in userMappings.Select(um => um.Split('=')))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** string jiraUser = userMapping[0].Trim();
***REMOVED******REMOVED******REMOVED*** string wiUser = userMapping[1].Trim();

***REMOVED******REMOVED******REMOVED*** UserMapping.Add(jiraUser, wiUser);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   public string MigrationWorkspace ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public LogLevel LogLevel ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public bool ForceFresh ***REMOVED*** get; internal set; ***REMOVED***

***REMOVED***   public Journal Journal ***REMOVED*** get; internal set; ***REMOVED***

***REMOVED***   public WiItemProvider Provider ***REMOVED*** get; private set; ***REMOVED***

***REMOVED***   public WiItem GetItem(string originId)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var item =  this.Provider.Load(originId);
***REMOVED******REMOVED***  item.WiId = Journal.GetMigratedId(originId);
***REMOVED******REMOVED***  foreach (var link in item.Revisions.SelectMany(r => r.Links))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** link.SourceOriginId = item.OriginId;
***REMOVED******REMOVED******REMOVED*** link.SourceWiId = Journal.GetMigratedId(originId);
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return item;
***REMOVED***   ***REMOVED***
***REMOVED***   public IEnumerable<WiItem> EnumerateAllItems()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  foreach (WiItem item in this.Provider.EnumerateAllItems())
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** item.WiId = Journal.GetMigratedId(item.OriginId);
***REMOVED******REMOVED******REMOVED*** yield return item;
***REMOVED******REMOVED***  ***REMOVED*** 
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***
