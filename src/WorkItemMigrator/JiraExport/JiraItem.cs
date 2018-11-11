using Atlassian.Jira;
using Migration.Common;
using Newtonsoft.Json.Linq;
***REMOVED***
***REMOVED***
using System.Globalization;
***REMOVED***
using System.Text;
using System.Threading.Tasks;

***REMOVED***
***REMOVED***
    public class JiraItem
    ***REMOVED***
***REMOVED***   #region Static

***REMOVED***   public static JiraItem CreateFromRest(string issueKey, JiraProvider jiraProvider)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var remoteIssue = jiraProvider.DownloadIssue(issueKey);
***REMOVED******REMOVED***  Logger.Log(LogLevel.Debug, $"Downloaded ***REMOVED***issueKey***REMOVED***");

***REMOVED******REMOVED***  var jiraItem = new JiraItem(jiraProvider, remoteIssue);
***REMOVED******REMOVED***  var revisions = BuildRevisions(jiraItem, jiraProvider);
***REMOVED******REMOVED***  jiraItem.Revisions = revisions;
***REMOVED******REMOVED***  Logger.Log(LogLevel.Debug, $"Formed representation of jira item ***REMOVED***issueKey***REMOVED***");

***REMOVED******REMOVED***  return jiraItem;
***REMOVED***   ***REMOVED***

***REMOVED***   private static List<JiraRevision> BuildRevisions(JiraItem jiraItem, JiraProvider jiraProvider)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  string issueKey = jiraItem.Key;
***REMOVED******REMOVED***  var remoteIssue = jiraItem.RemoteIssue;
***REMOVED******REMOVED***  Dictionary<string, object> fields = ExtractFields((JObject)remoteIssue.SelectToken("$.fields"), jiraProvider);
***REMOVED******REMOVED***  List<JiraAttachment> attachments = ExtractAttachments(remoteIssue.SelectTokens("$.fields.attachment[*]").Cast<JObject>()) ?? new List<JiraAttachment>();
***REMOVED******REMOVED***  List<JiraLink> links = ExtractLinks(issueKey, remoteIssue.SelectTokens("$.fields.issuelinks[*]").Cast<JObject>()) ?? new List<JiraLink>();


***REMOVED******REMOVED***  var changelog = jiraProvider.DownloadChangelog(issueKey).ToList();
***REMOVED******REMOVED***  changelog.Reverse();

***REMOVED******REMOVED***  Stack<JiraRevision> revisions = new Stack<JiraRevision>();

***REMOVED******REMOVED***  foreach (var change in changelog)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** DateTime created = change.ExValue<DateTime>("$.created");
***REMOVED******REMOVED******REMOVED*** string author = change.ExValue<string>("$.author.name");

***REMOVED******REMOVED******REMOVED*** List<RevisionAction<JiraLink>> linkChanges = new List<RevisionAction<JiraLink>>();
***REMOVED******REMOVED******REMOVED*** List<RevisionAction<JiraAttachment>> attachmentChanges = new List<RevisionAction<JiraAttachment>>();
***REMOVED******REMOVED******REMOVED*** Dictionary<string, object> fieldChanges = new Dictionary<string, object>();

***REMOVED******REMOVED******REMOVED*** var items = change.SelectTokens("$.items[*]").Cast<JObject>().Select(i => new JiraChangeItem(i));
***REMOVED******REMOVED******REMOVED*** foreach (var item in items)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***if (item.Field == "Link")
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    var linkChange = TransformLinkChange(item, issueKey, jiraProvider);
***REMOVED******REMOVED******REMOVED******REMOVED***    if (linkChange == null)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   continue;

***REMOVED******REMOVED******REMOVED******REMOVED***    linkChanges.Add(linkChange);

***REMOVED******REMOVED******REMOVED******REMOVED***    UndoLinkChange(linkChange, links);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***else if (item.Field == "Attachment")
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    var attachmentChange = TransformAttachmentChange(item);
***REMOVED******REMOVED******REMOVED******REMOVED***    if (attachmentChange == null)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   continue;

***REMOVED******REMOVED******REMOVED******REMOVED***    attachmentChanges.Add(attachmentChange);

***REMOVED******REMOVED******REMOVED******REMOVED***    UndoAttachmentChange(attachmentChange, attachments);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***else
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    var (fieldref, from, to) = TransformFieldChange(item, jiraProvider);

***REMOVED******REMOVED******REMOVED******REMOVED***    fieldChanges[fieldref] = to;

***REMOVED******REMOVED******REMOVED******REMOVED***    // undo field change
***REMOVED******REMOVED******REMOVED******REMOVED***    if (string.IsNullOrEmpty(from))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   fields.Remove(fieldref);
***REMOVED******REMOVED******REMOVED******REMOVED***    else
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   fields[fieldref] = from;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** var revision = new JiraRevision(jiraItem) ***REMOVED*** Time = created, Author = author, AttachmentActions = attachmentChanges, LinkActions = linkChanges, Fields = fieldChanges ***REMOVED***;
***REMOVED******REMOVED******REMOVED*** revisions.Push(revision);
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  // what is left after undoing all changes is first revision
***REMOVED******REMOVED***  var attActions = attachments.Select(a => new RevisionAction<JiraAttachment>() ***REMOVED*** ChangeType = RevisionChangeType.Added, Value = a ***REMOVED***).ToList();
***REMOVED******REMOVED***  var linkActions = links.Select(l => new RevisionAction<JiraLink>() ***REMOVED*** ChangeType = RevisionChangeType.Added, Value = l ***REMOVED***).ToList();
***REMOVED******REMOVED***  var fieldActions = fields;

***REMOVED******REMOVED***  var reporter = (string)fields["reporter"]; // customization point
***REMOVED******REMOVED***  var createdOn = (DateTime)fields["created"];

***REMOVED******REMOVED***  var firstRevision = new JiraRevision(jiraItem) ***REMOVED*** Time = createdOn, Author = reporter, AttachmentActions = attActions, Fields = fieldActions, LinkActions = linkActions ***REMOVED***;
***REMOVED******REMOVED***  revisions.Push(firstRevision);
***REMOVED******REMOVED***  var listOfRevisions = revisions.ToList();

***REMOVED******REMOVED***  List<JiraRevision> commentRevisions = BuildCommentRevisions(jiraItem, jiraProvider);
***REMOVED******REMOVED***  listOfRevisions.AddRange(commentRevisions);
***REMOVED******REMOVED***  listOfRevisions.Sort();

***REMOVED******REMOVED***  foreach (var revAndI in listOfRevisions.Select((r, i) => (r, i)))
***REMOVED******REMOVED******REMOVED*** revAndI.Item1.Index = revAndI.Item2;

***REMOVED******REMOVED***  return listOfRevisions;
***REMOVED***   ***REMOVED***

***REMOVED***   private static List<JiraRevision> BuildCommentRevisions(JiraItem jiraItem, JiraProvider jiraProvider)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var comments = jiraProvider.Jira.Issues.GetCommentsAsync(jiraItem.Key).Result;
***REMOVED******REMOVED***  return comments.Select(c => new JiraRevision(jiraItem) ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***Author = c.Author,
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***Time = c.CreatedDate.Value,
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***Fields = new Dictionary<string, object>() ***REMOVED*** ***REMOVED*** "comment", c.Body***REMOVED*** ***REMOVED***,
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***AttachmentActions = new List<RevisionAction<JiraAttachment>>(), 
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***LinkActions = new List<RevisionAction<JiraLink>>()
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  .ToList();
***REMOVED***   ***REMOVED***

***REMOVED***   private static void UndoAttachmentChange(RevisionAction<JiraAttachment> attachmentChange, List<JiraAttachment> attachments)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (attachmentChange.ChangeType == RevisionChangeType.Removed)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Debug, $"Skipping undo for: ***REMOVED***attachmentChange.ToString()***REMOVED***");
***REMOVED******REMOVED******REMOVED*** return;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  if (attachments.Remove(attachmentChange.Value))
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Debug, $"Undone: ***REMOVED***attachmentChange.ToString()***REMOVED***");
***REMOVED******REMOVED***  else
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Debug, $"No attachment to undo: ***REMOVED***attachmentChange.ToString()***REMOVED***");
***REMOVED***   ***REMOVED***

***REMOVED***   private static RevisionAction<JiraAttachment> TransformAttachmentChange(JiraChangeItem item)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  string attKey = string.Empty;
***REMOVED******REMOVED***  string attFilename = string.Empty;

***REMOVED******REMOVED***  RevisionChangeType changeType = RevisionChangeType.Added;

***REMOVED******REMOVED***  if (item.From == null && item.To != null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** attKey = item.To;
***REMOVED******REMOVED******REMOVED*** attFilename = item.ToString;
***REMOVED******REMOVED******REMOVED*** changeType = RevisionChangeType.Added;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  else if (item.To == null && item.From != null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** attKey = item.From;
***REMOVED******REMOVED******REMOVED*** attFilename = item.FromString;
***REMOVED******REMOVED******REMOVED*** changeType = RevisionChangeType.Removed;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  else
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, "Attachment change not handled!");
***REMOVED******REMOVED******REMOVED*** return null;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return new RevisionAction<JiraAttachment>()
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** ChangeType = changeType,
***REMOVED******REMOVED******REMOVED*** Value = new JiraAttachment()
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Id = attKey,
***REMOVED******REMOVED******REMOVED******REMOVED***Filename = attFilename
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***;
***REMOVED***   ***REMOVED***

***REMOVED***   private static (string, string, string) TransformFieldChange(JiraChangeItem item, JiraProvider jira)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var objectFields = new HashSet<string>() ***REMOVED*** "assignee", "creator", "reporter" ***REMOVED***;
***REMOVED******REMOVED***  string from, to = string.Empty;

***REMOVED******REMOVED***  string fieldId = item.FieldId ?? GetCustomFieldId(item.Field, jira) ?? item.Field;

***REMOVED******REMOVED***  if (objectFields.Contains(fieldId))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** from = item.From;
***REMOVED******REMOVED******REMOVED*** to = item.To;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  else
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** from = item.FromString;
***REMOVED******REMOVED******REMOVED*** to = item.ToString;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return (fieldId, from, to);
***REMOVED***   ***REMOVED***

***REMOVED***   private static string GetCustomFieldId(string fieldName, JiraProvider jira)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (jira.Jira.RestClient.Settings.Cache.CustomFields.TryGetValue(fieldName, out var customField))
***REMOVED******REMOVED******REMOVED*** return customField.Id;
***REMOVED******REMOVED***  else return null;

***REMOVED***   ***REMOVED***

***REMOVED***   private static void UndoLinkChange(RevisionAction<JiraLink> linkChange, List<JiraLink> links)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (linkChange.ChangeType == RevisionChangeType.Removed)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Debug, $"Skipping undo for: ***REMOVED***linkChange.ToString()***REMOVED***");
***REMOVED******REMOVED******REMOVED*** return;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  if (links.Remove(linkChange.Value))
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Debug, $"Undone: ***REMOVED***linkChange.ToString()***REMOVED***");
***REMOVED******REMOVED***  else
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Debug, $"No link to undo: ***REMOVED***linkChange.ToString()***REMOVED***");
***REMOVED***   ***REMOVED***

***REMOVED***   private static RevisionAction<JiraLink> TransformLinkChange(JiraChangeItem item, string sourceItemKey, JiraProvider jira)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  string targetItemKey = string.Empty;
***REMOVED******REMOVED***  string linkTypeString = string.Empty;
***REMOVED******REMOVED***  RevisionChangeType changeType = RevisionChangeType.Added;

***REMOVED******REMOVED***  if (item.From == null && item.To != null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** targetItemKey = item.To;
***REMOVED******REMOVED******REMOVED*** linkTypeString = item.ToString;
***REMOVED******REMOVED******REMOVED*** changeType = RevisionChangeType.Added;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  else if (item.To == null && item.From != null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** targetItemKey = item.From;
***REMOVED******REMOVED******REMOVED*** linkTypeString = item.FromString;
***REMOVED******REMOVED******REMOVED*** changeType = RevisionChangeType.Removed;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  else
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, $"Link change not handled!");
***REMOVED******REMOVED******REMOVED*** return null;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  var linkType = jira.LinkTypes.FirstOrDefault(lt => linkTypeString.EndsWith(lt.Outward + " " + targetItemKey));
***REMOVED******REMOVED***  if (linkType == null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Debug, $"Link with descrption \"***REMOVED***linkTypeString***REMOVED***\" is either not found or this issue (***REMOVED***sourceItemKey***REMOVED***) is not inward issue.");
***REMOVED******REMOVED******REMOVED*** return null;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  else
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (linkType.Inward == linkType.Outward && sourceItemKey.CompareTo(targetItemKey) < 0)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Debug, $"Link is non-directional (***REMOVED***linkType.Name***REMOVED***) and sourceItem (***REMOVED***sourceItemKey***REMOVED***) is older then target item (***REMOVED***targetItemKey***REMOVED***). Link change will be part of target item.");
***REMOVED******REMOVED******REMOVED******REMOVED***return null;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** return new RevisionAction<JiraLink>()
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***ChangeType = changeType,
***REMOVED******REMOVED******REMOVED******REMOVED***Value = new JiraLink()
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    SourceItem = sourceItemKey,
***REMOVED******REMOVED******REMOVED******REMOVED***    TargetItem = targetItemKey,
***REMOVED******REMOVED******REMOVED******REMOVED***    LinkType = linkType.Name,
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED*** ***REMOVED***;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private static List<JiraLink> ExtractLinks(string sourceKey, IEnumerable<JObject> issueLinks)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var links = new List<JiraLink>();

***REMOVED******REMOVED***  foreach (var issueLink in issueLinks)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var targetIssueKey = issueLink.ExValue<string>("$.outwardIssue.key");
***REMOVED******REMOVED******REMOVED*** if (string.IsNullOrWhiteSpace(targetIssueKey))
***REMOVED******REMOVED******REMOVED******REMOVED***continue;

***REMOVED******REMOVED******REMOVED*** var type = issueLink.ExValue<string>("$.type.name");

***REMOVED******REMOVED******REMOVED*** var link = new JiraLink() ***REMOVED*** SourceItem = sourceKey, TargetItem = targetIssueKey, LinkType = type ***REMOVED***;
***REMOVED******REMOVED******REMOVED*** links.Add(link);
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return links;
***REMOVED***   ***REMOVED***

***REMOVED***   private static List<JiraAttachment> ExtractAttachments(IEnumerable<JObject> attachmentObjs)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return attachmentObjs.Select(attObj => ***REMOVED***
***REMOVED******REMOVED******REMOVED*** return new JiraAttachment
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Id = attObj.ExValue<string>("$.id"),
***REMOVED******REMOVED******REMOVED******REMOVED***Filename = attObj.ExValue<string>("$.filename"),
***REMOVED******REMOVED******REMOVED******REMOVED***Url = attObj.ExValue<string>("$.content"),
***REMOVED******REMOVED******REMOVED******REMOVED***ThumbUrl = attObj.ExValue<string>("$.thumbnail")
***REMOVED******REMOVED******REMOVED*** ***REMOVED***;
***REMOVED******REMOVED***  ***REMOVED***).ToList();
***REMOVED***   ***REMOVED***

***REMOVED***   private static Dictionary<string, Func<JToken, object>> _fieldExtractionMapping = null;
***REMOVED***   private static Dictionary<string, object> ExtractFields(JObject remoteFields, JiraProvider jira)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var fields = new Dictionary<string, object>();

***REMOVED******REMOVED***  var extractName = new Func<JToken, object>((t) => t.ExValue<string>("$.name"));

***REMOVED******REMOVED***  if (_fieldExtractionMapping == null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** _fieldExtractionMapping = new Dictionary<string, Func<JToken, object>>()
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED*** "priority", extractName ***REMOVED***,
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED*** "labels", t => t.Values<string>().Any() ? string.Join(" ", t.Values<string>()) : null ***REMOVED***,
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED*** "assignee", extractName ***REMOVED***,
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED*** "creator", extractName ***REMOVED***,
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED*** "reporter", extractName***REMOVED***,
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED*** jira.Settings.SprintField, t => string.Join(", ", ParseCustomField(jira.Settings.SprintField, t, jira)) ***REMOVED***,
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED*** "status", extractName ***REMOVED***,
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED*** "parent", t => t.ExValue<string>("$.key") ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  foreach (var prop in remoteFields.Properties())
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** object value = null;
***REMOVED******REMOVED******REMOVED*** if (_fieldExtractionMapping.TryGetValue(prop.Name, out Func<JToken, object> mapping))
***REMOVED******REMOVED******REMOVED******REMOVED***value = mapping(prop.Value);
***REMOVED******REMOVED******REMOVED*** else if (prop.Value.Type == JTokenType.String
***REMOVED******REMOVED******REMOVED******REMOVED***|| prop.Value.Type == JTokenType.Integer
***REMOVED******REMOVED******REMOVED******REMOVED***|| prop.Value.Type == JTokenType.Float)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***value = prop.Value.Value<string>();
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** else if (prop.Value.Type == JTokenType.Date)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***value = prop.Value.Value<DateTime>();
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** if (value != null)
***REMOVED******REMOVED******REMOVED******REMOVED***fields[prop.Name] = value;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return fields;
***REMOVED***   ***REMOVED***

***REMOVED***   private static string[] ParseCustomField(string fieldName, JToken value, JiraProvider provider)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (provider.Jira.RestClient.Settings.Cache.CustomFields.TryGetValue(fieldName, out var customField) && customField != null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (provider.Jira.RestClient.Settings.CustomFieldSerializers.TryGetValue(customField.CustomType, out var serializer))
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***return serializer.FromJson(value);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return null;
***REMOVED***   ***REMOVED***

***REMOVED***   #endregion

***REMOVED***   private readonly JiraProvider _provider;

***REMOVED***   public string Key ***REMOVED*** get ***REMOVED*** return RemoteIssue.ExValue<string>("$.key"); ***REMOVED*** ***REMOVED***
***REMOVED***   public string Type ***REMOVED*** get ***REMOVED*** return RemoteIssue.ExValue<string>("$.fields.issuetype.name"); ***REMOVED*** ***REMOVED***

***REMOVED***   public string EpicParent ***REMOVED*** get ***REMOVED*** return RemoteIssue.ExValue<string>($"$.fields.***REMOVED***_provider.Settings.EpicLinkField***REMOVED***"); ***REMOVED*** ***REMOVED***
***REMOVED***   public string Parent ***REMOVED*** get ***REMOVED*** return RemoteIssue.ExValue<string>("$.fields.parent.key"); ***REMOVED*** ***REMOVED***
***REMOVED***   public List<string> SubItems ***REMOVED*** get ***REMOVED*** return RemoteIssue.SelectTokens("$.fields.subtasks.[*]", false).Select(st => st.ExValue<string>("$.key")).ToList(); ***REMOVED*** ***REMOVED***

***REMOVED***   public JObject RemoteIssue ***REMOVED*** get; private set; ***REMOVED***

***REMOVED***   public List<JiraRevision> Revisions ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   private JiraItem(JiraProvider provider, JObject remoteIssue)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  this._provider = provider;
***REMOVED******REMOVED***  RemoteIssue = remoteIssue;
***REMOVED***   ***REMOVED***

***REMOVED***   internal string GetUserEmail(string author)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return _provider.GetUserEmail(author);
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***
