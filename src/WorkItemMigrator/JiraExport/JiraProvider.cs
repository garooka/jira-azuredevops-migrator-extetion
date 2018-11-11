using Atlassian.Jira;
using Migration.Common;
using Newtonsoft.Json.Linq;
***REMOVED***
***REMOVED***
***REMOVED***
***REMOVED***
using System.Text;
using System.Threading.Tasks;

***REMOVED***
***REMOVED***
    public class JiraProvider
    ***REMOVED***
***REMOVED***   [Flags]
***REMOVED***   public enum DownloadOptions
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  IncludeParentEpics,
***REMOVED******REMOVED***  IncludeEpicChildren,
***REMOVED******REMOVED***  IncludeParents,
***REMOVED******REMOVED***  IncludeSubItems,
***REMOVED******REMOVED***  IncludeLinkedItems
***REMOVED***   ***REMOVED***

    public static JiraProvider Initialize(JiraSettings settings)
***REMOVED***   ***REMOVED***

***REMOVED******REMOVED***  var provider = new JiraProvider();

***REMOVED******REMOVED***  provider.Jira = ConnectToJira(settings);
***REMOVED******REMOVED***  provider.Settings = settings;

***REMOVED******REMOVED***  Logger.Log(LogLevel.Info, "Gathering project info...");

***REMOVED******REMOVED***  // ensure that Custom fields cache is full
***REMOVED******REMOVED***  provider.Jira.Fields.GetCustomFieldsAsync().Wait();
***REMOVED******REMOVED***  Logger.Log(LogLevel.Info, "Custom field cache set up.");
***REMOVED******REMOVED***  Logger.Log(LogLevel.Info, "Custom parsers set up.");

***REMOVED******REMOVED***  provider.LinkTypes = provider.Jira.Links.GetLinkTypesAsync().Result;
***REMOVED******REMOVED***  Logger.Log(LogLevel.Info, "Link types cache set up.");

***REMOVED******REMOVED***  return provider;
***REMOVED***   ***REMOVED***

***REMOVED***   private static Jira ConnectToJira(JiraSettings jiraSettings)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  Logger.Log(LogLevel.Debug, "Connecting to Jira...");
***REMOVED******REMOVED***  Jira jira = null;

***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** jira = Jira.CreateRestClient(jiraSettings.Url, jiraSettings.UserID, jiraSettings.Pass);
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Info, "Connected to Jira.");
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (Exception ex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Critical, $"Could not connect to Jira! Message: ***REMOVED***ex.Message***REMOVED***");
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return jira;
***REMOVED***   ***REMOVED***

***REMOVED***   private JiraProvider()
***REMOVED***   ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   public Jira Jira ***REMOVED*** get; private set;***REMOVED***
***REMOVED***   public JiraSettings Settings ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public IEnumerable<IssueLinkType> LinkTypes ***REMOVED*** get; private set; ***REMOVED***


***REMOVED***   private JiraItem ProcessItem(string issueKey, HashSet<string> skipList, string successMessage)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var issue = JiraItem.CreateFromRest(issueKey, this);
***REMOVED******REMOVED***  Logger.Log(LogLevel.Info, $"Downloaded ***REMOVED***issueKey***REMOVED*** - ***REMOVED***successMessage***REMOVED***");
***REMOVED******REMOVED***  skipList.Add(issue.Key);
***REMOVED******REMOVED***  return issue;
***REMOVED***   ***REMOVED***

***REMOVED***   public IEnumerable<JiraItem> EnumerateIssues(string jql, HashSet<string> skipList, DownloadOptions downloadOptions)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  Logger.Log(LogLevel.Info, "Processing issues...");
***REMOVED******REMOVED***  int currentStart = 0;
***REMOVED******REMOVED***  IEnumerable<string> remoteIssueBatch = null;
***REMOVED******REMOVED***  int index = 0;
***REMOVED******REMOVED***  do
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var response = Jira.RestClient.ExecuteRequestAsync(RestSharp.Method.GET,
***REMOVED******REMOVED******REMOVED******REMOVED***$"rest/api/2/search?jql=***REMOVED***jql***REMOVED***&startAt=***REMOVED***currentStart***REMOVED***&maxResults=***REMOVED***Settings.BatchSize***REMOVED***&fields=key").Result;

***REMOVED******REMOVED******REMOVED*** remoteIssueBatch = response.SelectTokens("$.issues[*]").OfType<JObject>()
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   .Select(i => i.SelectToken("$.key").Value<string>());

***REMOVED******REMOVED******REMOVED*** currentStart += Settings.BatchSize;

***REMOVED******REMOVED******REMOVED*** int totalItems = (int)response.SelectToken("$.total");

***REMOVED******REMOVED******REMOVED*** foreach (var issueKey in remoteIssueBatch)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***if (skipList.Contains(issueKey))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    Logger.Log(LogLevel.Info, $"Skipped ***REMOVED***issueKey***REMOVED*** - already downloaded [***REMOVED***index + 1***REMOVED***/***REMOVED***totalItems***REMOVED***]");
***REMOVED******REMOVED******REMOVED******REMOVED***    index++;
***REMOVED******REMOVED******REMOVED******REMOVED***    continue;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***


***REMOVED******REMOVED******REMOVED******REMOVED***var issue = ProcessItem(issueKey, skipList, $"[***REMOVED***index + 1***REMOVED***/***REMOVED***totalItems***REMOVED***]");
***REMOVED******REMOVED******REMOVED******REMOVED***yield return issue;
***REMOVED******REMOVED******REMOVED******REMOVED***index++;

***REMOVED******REMOVED******REMOVED******REMOVED***if (downloadOptions.HasFlag(DownloadOptions.IncludeParentEpics) && issue.EpicParent != null && !skipList.Contains(issue.EpicParent))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    var parentEpic = ProcessItem(issue.EpicParent, skipList, $"epic parent of ***REMOVED***issueKey***REMOVED***");
***REMOVED******REMOVED******REMOVED******REMOVED***    yield return parentEpic;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***

***REMOVED******REMOVED******REMOVED******REMOVED***if (downloadOptions.HasFlag(DownloadOptions.IncludeParents) && issue.Parent != null && !skipList.Contains(issue.EpicParent))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    var parent = ProcessItem(issue.Parent, skipList, $"parent of ***REMOVED***issueKey***REMOVED***");
***REMOVED******REMOVED******REMOVED******REMOVED***    yield return parent;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***

***REMOVED******REMOVED******REMOVED******REMOVED***if (downloadOptions.HasFlag(DownloadOptions.IncludeSubItems) && issue.SubItems != null && issue.SubItems.Any())
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    foreach (var subitemKey in issue.SubItems)
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   if (!skipList.Contains(subitemKey))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  var subItem = ProcessItem(subitemKey, skipList, $"sub-item of ***REMOVED***issueKey***REMOVED***");
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  yield return subItem;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  while (remoteIssueBatch != null && remoteIssueBatch.Any());
***REMOVED***   ***REMOVED***

***REMOVED***   public IEnumerable<JObject> DownloadChangelog(string issueKey)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  bool isLast = true;
***REMOVED******REMOVED***  int batchSize = 100;
***REMOVED******REMOVED***  int currentStart = 0;
***REMOVED******REMOVED***  do
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var response = (JObject)Jira.RestClient.ExecuteRequestAsync(RestSharp.Method.GET,
***REMOVED******REMOVED******REMOVED******REMOVED***$"rest/api/2/issue/***REMOVED***issueKey***REMOVED***/changelog?maxResults=***REMOVED***batchSize***REMOVED***&startAt=***REMOVED***currentStart***REMOVED***").Result;

***REMOVED******REMOVED******REMOVED*** currentStart += batchSize;
***REMOVED******REMOVED******REMOVED*** isLast = (bool)response.SelectToken("$.isLast");

***REMOVED******REMOVED******REMOVED*** var changes = response.SelectTokens("$.values[*]").Cast<JObject>();
***REMOVED******REMOVED******REMOVED*** foreach (var change in changes)
***REMOVED******REMOVED******REMOVED******REMOVED***yield return change;

***REMOVED******REMOVED***  ***REMOVED*** while (!isLast);
***REMOVED***   ***REMOVED***

***REMOVED***   public JObject DownloadIssue(string key)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var response = Jira.RestClient.ExecuteRequestAsync(RestSharp.Method.GET, $"rest/api/2/issue/***REMOVED***key***REMOVED***?expand=renderedFields").Result;
***REMOVED******REMOVED***  var remoteItem = (JObject)response;
***REMOVED******REMOVED***  return remoteItem;
***REMOVED***   ***REMOVED***

***REMOVED***   public async Task<List<RevisionAction<JiraAttachment>>> DownloadAttachments(JiraRevision rev)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var attChanges = rev.AttachmentActions;

***REMOVED******REMOVED***  if (attChanges != null && attChanges.Any(a => a.ChangeType == RevisionChangeType.Added))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var downloadedAtts = new List<JiraAttachment>();
***REMOVED******REMOVED******REMOVED*** using (var web = new WebClientWrapper(this))
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***foreach (var remoteAtt in attChanges)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    var jiraAtt = await DownloadAttachmentAsync(remoteAtt.Value, rev.ParentItem.Key, web);
***REMOVED******REMOVED******REMOVED******REMOVED***    if (jiraAtt != null && !string.IsNullOrWhiteSpace(jiraAtt.LocalPath))
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   Logger.Log(LogLevel.Info, $"Downloaded ***REMOVED***jiraAtt.ToString()***REMOVED*** to ***REMOVED***jiraAtt.LocalPath***REMOVED***");
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   downloadedAtts.Add(jiraAtt);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** // of added attachments, leave only attachments that have been successfully downloaded
***REMOVED******REMOVED******REMOVED*** attChanges.RemoveAll(ac => ac.ChangeType == RevisionChangeType.Added);
***REMOVED******REMOVED******REMOVED*** attChanges.AddRange(downloadedAtts.Select(da => new RevisionAction<JiraAttachment>() ***REMOVED*** ChangeType = RevisionChangeType.Added, Value = da ***REMOVED***));
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return attChanges;
***REMOVED***   ***REMOVED***

***REMOVED***   public int GetNumberOfComments(string key)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return Jira.Issues.GetCommentsAsync(key).Result.Count();
***REMOVED***   ***REMOVED***

***REMOVED***   private async Task<JiraAttachment> GetAttachmentInfo(string id)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  Logger.Log(LogLevel.Debug, $"Downloading attachment info for attachment ***REMOVED***id***REMOVED***");

***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var response = await Jira.RestClient.ExecuteRequestAsync(RestSharp.Method.GET, $"rest/api/2/attachment/***REMOVED***id***REMOVED***");
***REMOVED******REMOVED******REMOVED*** var attObj = (JObject)response;

***REMOVED******REMOVED******REMOVED*** return new JiraAttachment()
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Id = id,
***REMOVED******REMOVED******REMOVED******REMOVED***Filename = attObj.ExValue<string>("$.filename"),
***REMOVED******REMOVED******REMOVED******REMOVED***Url = attObj.ExValue<string>("$.content"),
***REMOVED******REMOVED******REMOVED******REMOVED***ThumbUrl = attObj.ExValue<string>("$.thumbnail")
***REMOVED******REMOVED******REMOVED*** ***REMOVED***;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (Exception ex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Warning, $"Cannot find info for attachment ***REMOVED***id***REMOVED***. Skipping.");
***REMOVED******REMOVED******REMOVED*** Logger.Log(ex);
***REMOVED******REMOVED******REMOVED*** return null;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private async Task<JiraAttachment> DownloadAttachmentAsync(JiraAttachment att, string key, WebClientWrapper web)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (string.IsNullOrWhiteSpace(att.Url))
***REMOVED******REMOVED******REMOVED******REMOVED***att = await GetAttachmentInfo(att.Id);

***REMOVED******REMOVED******REMOVED*** if (att != null && !string.IsNullOrWhiteSpace(att.Url))
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***string path = Path.Combine(Settings.AttachmentsDir, att.Id, att.Filename);
***REMOVED******REMOVED******REMOVED******REMOVED***EnsurePath(path);
***REMOVED******REMOVED******REMOVED******REMOVED***await web.DownloadWithAuthenticationAsync(att.Url, path);
***REMOVED******REMOVED******REMOVED******REMOVED***att.LocalPath = path;
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Debug, $"Downloaded attachment ***REMOVED***att.ToString()***REMOVED***");
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (Exception ex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Warning, $"Attachment (***REMOVED***att.ToString()***REMOVED***) download failed. Message: ***REMOVED***ex.Message***REMOVED***");
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  if (att != null && !string.IsNullOrWhiteSpace(att.ThumbUrl))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** try
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***string thumbname = Path.GetFileNameWithoutExtension(att.Filename) + ".thumb" + Path.GetExtension(att.Filename);
***REMOVED******REMOVED******REMOVED******REMOVED***var thumbPath = Path.Combine(Settings.AttachmentsDir, att.Id, thumbname);
***REMOVED******REMOVED******REMOVED******REMOVED***EnsurePath(thumbPath);
***REMOVED******REMOVED******REMOVED******REMOVED***await web.DownloadWithAuthenticationAsync(att.ThumbUrl, Path.Combine(Settings.AttachmentsDir, att.Id, thumbname));
***REMOVED******REMOVED******REMOVED******REMOVED***att.LocalThumbPath = thumbPath;
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Debug, $"Downloaded attachment thumbnail ***REMOVED***att.ToString()***REMOVED***");
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** catch (Exception ex)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Warning, $"Attachment thumbnail (***REMOVED***att.ToString()***REMOVED***) download failed. Message: ***REMOVED***ex.Message***REMOVED***");
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return att;
***REMOVED***   ***REMOVED***

***REMOVED***   private void EnsurePath(string path)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var dir = Path.GetDirectoryName(path);
***REMOVED******REMOVED***  if (!Directory.Exists(dir))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var parentDir = Path.GetDirectoryName(dir);
***REMOVED******REMOVED******REMOVED*** EnsurePath(parentDir);
***REMOVED******REMOVED******REMOVED*** Directory.CreateDirectory(dir);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   Dictionary<string, string> _userEmailCache = new Dictionary<string, string>();
***REMOVED***   public string GetUserEmail(string username)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (_userEmailCache.TryGetValue(username, out string email))
***REMOVED******REMOVED******REMOVED*** return email;
***REMOVED******REMOVED***  else
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var user = Jira.Users.GetUserAsync(username).Result;
***REMOVED******REMOVED******REMOVED*** email = user.Email;
***REMOVED******REMOVED******REMOVED*** _userEmailCache.Add(username, email);
***REMOVED******REMOVED******REMOVED*** return email;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***
