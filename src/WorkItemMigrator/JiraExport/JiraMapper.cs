***REMOVED***
***REMOVED***
***REMOVED***
using Migration.Common;
using System.Text.RegularExpressions;
***REMOVED***
***REMOVED***
using System.Reflection;
using Newtonsoft.Json;
using Migration.WIContract;

***REMOVED***
***REMOVED***
    internal class JiraMapper : BaseMapper<JiraRevision>
    ***REMOVED***
***REMOVED***   private JiraProvider jiraProvider;
***REMOVED***   private Dictionary<string, FieldMapping<JiraRevision>> _fieldMappingsPerType;

***REMOVED***   public JiraMapper(JiraProvider jiraProvider, ConfigJson config) : base(jiraProvider?.Settings?.UserMappingFile)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  this.jiraProvider = jiraProvider;

***REMOVED******REMOVED***  _fieldMappingsPerType = InitializeFieldMappings(config);
***REMOVED***   ***REMOVED***

***REMOVED***   /// <summary>
***REMOVED***   /// Add or remove single link
***REMOVED***   /// </summary>
***REMOVED***   /// <param name="r"></param>
***REMOVED***   /// <param name="links"></param>
***REMOVED***   /// <param name="field"></param>
***REMOVED***   /// <param name="type"></param>
***REMOVED***   /// <returns>True if link is added, false if it's not</returns>
***REMOVED***   private bool AddRemoveSingleLink(JiraRevision r, List<WiLink> links, string field, string type)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (r.Fields.TryGetValue(field, out object value))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var changeType = value == null ? ReferenceChangeType.Removed : ReferenceChangeType.Added;
***REMOVED******REMOVED******REMOVED*** var linkType = MapLinkType(type);

***REMOVED******REMOVED******REMOVED*** // regardless if action is add or remove, as there can be only one, we remove previous epic link if it exists
***REMOVED******REMOVED******REMOVED*** if (r.Index != 0)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***var prevLinkValue = r.ParentItem.Revisions[r.Index - 1].GetFieldValue(field);
***REMOVED******REMOVED******REMOVED******REMOVED***// if previous value is not null, add removal of previous link
***REMOVED******REMOVED******REMOVED******REMOVED***if (!string.IsNullOrWhiteSpace(prevLinkValue))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    var removeLink = new WiLink()
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   Change = ReferenceChangeType.Removed,
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   SourceOriginId = r.ParentItem.Key,
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   TargetOriginId = prevLinkValue,
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   WiType = linkType
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***;

***REMOVED******REMOVED******REMOVED******REMOVED***    links.Add(removeLink);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** if (changeType == ReferenceChangeType.Added)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***string linkedItemKey = (string)value;

***REMOVED******REMOVED******REMOVED******REMOVED***var link = new WiLink()
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    Change = changeType,
***REMOVED******REMOVED******REMOVED******REMOVED***    SourceOriginId = r.ParentItem.Key,
***REMOVED******REMOVED******REMOVED******REMOVED***    TargetOriginId = linkedItemKey,
***REMOVED******REMOVED******REMOVED******REMOVED***    WiType = linkType,
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***;

***REMOVED******REMOVED******REMOVED******REMOVED***links.Add(link);

***REMOVED******REMOVED******REMOVED******REMOVED***return true;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return false;
***REMOVED***   ***REMOVED***

***REMOVED***   private List<string> GetWorkItemTypes(string notFor = "")
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return !string.IsNullOrWhiteSpace(notFor) ? WorkItemType.GetWorkItemTypes(notFor) : WorkItemType.GetWorkItemTypes();
***REMOVED***   ***REMOVED***

***REMOVED***   #region Mapping definitions

***REMOVED***   private WiRevision MapRevision(JiraRevision r, TemplateType template)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  List<WiAttachment> attachments = MapAttachments(r);
***REMOVED******REMOVED***  List<Migration.WIContract.WiField> fields = MapFields(r, template);
***REMOVED******REMOVED***  List<WiLink> links = MapLinks(r);

***REMOVED******REMOVED***  return new WiRevision()
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** ParentOriginId = r.ParentItem.Key,
***REMOVED******REMOVED******REMOVED*** Index = r.Index,
***REMOVED******REMOVED******REMOVED*** Time = r.Time,
***REMOVED******REMOVED******REMOVED*** Author = MapUser(r.Author),
***REMOVED******REMOVED******REMOVED*** Attachments = attachments,
***REMOVED******REMOVED******REMOVED*** Fields = fields,
***REMOVED******REMOVED******REMOVED*** Links = links
***REMOVED******REMOVED***  ***REMOVED***;
***REMOVED***   ***REMOVED***

***REMOVED***   protected override string MapUser(string username)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (string.IsNullOrWhiteSpace(username))
***REMOVED******REMOVED******REMOVED*** return null;

***REMOVED******REMOVED***  var email = jiraProvider.GetUserEmail(username);
***REMOVED******REMOVED***  return base.MapUser(email);
***REMOVED***   ***REMOVED***

***REMOVED***   internal WiItem Map(JiraItem issue, TemplateType template)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  List<WiRevision> revisions = new List<WiRevision>();

***REMOVED******REMOVED***  string type = MapType(issue.Type, template);

***REMOVED******REMOVED***  revisions = issue.Revisions.Select(r => MapRevision(r, template)).ToList();

***REMOVED******REMOVED***  MapLastDescription(revisions, issue, template);

***REMOVED******REMOVED***  return new WiItem()
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** OriginId = issue.Key,
***REMOVED******REMOVED******REMOVED*** Type = type,
***REMOVED******REMOVED******REMOVED*** Revisions = revisions
***REMOVED******REMOVED***  ***REMOVED***;
***REMOVED***   ***REMOVED***

***REMOVED***   private List<WiLink> MapLinks(JiraRevision r)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var links = new List<WiLink>();
***REMOVED******REMOVED***  if (r.LinkActions == null)
***REMOVED******REMOVED******REMOVED*** return links;

***REMOVED******REMOVED***  // map issue links
***REMOVED******REMOVED***  foreach (var jiraLinkAction in r.LinkActions)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var changeType = jiraLinkAction.ChangeType == RevisionChangeType.Added ? ReferenceChangeType.Added : ReferenceChangeType.Removed;
***REMOVED******REMOVED******REMOVED*** var linkType = MapLinkType(jiraLinkAction.Value.LinkType);

***REMOVED******REMOVED******REMOVED*** if (linkType != null)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***var link = new WiLink()
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    Change = changeType,
***REMOVED******REMOVED******REMOVED******REMOVED***    SourceOriginId = jiraLinkAction.Value.SourceItem,
***REMOVED******REMOVED******REMOVED******REMOVED***    TargetOriginId = jiraLinkAction.Value.TargetItem,
***REMOVED******REMOVED******REMOVED******REMOVED***    WiType = linkType
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***;

***REMOVED******REMOVED******REMOVED******REMOVED***links.Add(link);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  // map epic link
***REMOVED******REMOVED***  AddRemoveSingleLink(r, links, jiraProvider.Settings.EpicLinkField, "Epic");

***REMOVED******REMOVED***  // map parent
***REMOVED******REMOVED***  AddRemoveSingleLink(r, links, "parent", "Parent");

***REMOVED******REMOVED***  return links;
***REMOVED***   ***REMOVED***

***REMOVED***   private List<WiAttachment> MapAttachments(JiraRevision rev)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var attachments = new List<WiAttachment>();
***REMOVED******REMOVED***  if (rev.AttachmentActions == null)
***REMOVED******REMOVED******REMOVED*** return attachments;

***REMOVED******REMOVED***  jiraProvider.DownloadAttachments(rev).Wait();

***REMOVED******REMOVED***  foreach (var att in rev.AttachmentActions)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var change = att.ChangeType == RevisionChangeType.Added ? ReferenceChangeType.Added : ReferenceChangeType.Removed;

***REMOVED******REMOVED******REMOVED*** var wiAtt = new WiAttachment()
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Change = change,
***REMOVED******REMOVED******REMOVED******REMOVED***AttOriginId = att.Value.Id,
***REMOVED******REMOVED******REMOVED******REMOVED***FilePath = att.Value.LocalPath,
***REMOVED******REMOVED******REMOVED******REMOVED***Comment = "Imported from Jira" // customization point
***REMOVED******REMOVED******REMOVED*** ***REMOVED***;
***REMOVED******REMOVED******REMOVED*** attachments.Add(wiAtt);

***REMOVED******REMOVED******REMOVED*** if (!string.IsNullOrWhiteSpace(att.Value.LocalThumbPath))
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***var wiThumbAtt = new WiAttachment()
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    Change = change,
***REMOVED******REMOVED******REMOVED******REMOVED***    AttOriginId = att.Value.Id + "-thumb",
***REMOVED******REMOVED******REMOVED******REMOVED***    FilePath = att.Value.LocalThumbPath,
***REMOVED******REMOVED******REMOVED******REMOVED***    Comment = $"Thumbnail for ***REMOVED***att.Value.Filename***REMOVED***"
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***;

***REMOVED******REMOVED******REMOVED******REMOVED***attachments.Add(wiThumbAtt);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return attachments;
***REMOVED***   ***REMOVED***

***REMOVED***   private List<Migration.WIContract.WiField> MapFields(JiraRevision r, TemplateType template)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  List<Migration.WIContract.WiField> fields = new List<Migration.WIContract.WiField>();
***REMOVED******REMOVED***  string type = MapType(r.ParentItem.Type, template);

***REMOVED******REMOVED***  if (_fieldMappingsPerType.TryGetValue(type, out var mapping))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** foreach (var field in mapping)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***try
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    var fieldreference = field.Key;
***REMOVED******REMOVED******REMOVED******REMOVED***    var (include, value) = field.Value(r);

***REMOVED******REMOVED******REMOVED******REMOVED***    if (include)
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   fields.Add(new Migration.WIContract.WiField()
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  ReferenceName = fieldreference,
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  Value = value
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   ***REMOVED***);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***catch (Exception ex)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    Logger.Log(LogLevel.Error, $"Error mapping field ***REMOVED***field.Key***REMOVED*** on item ***REMOVED***r.OriginId***REMOVED***: ***REMOVED***ex.Message***REMOVED***");
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return fields;
***REMOVED***   ***REMOVED***

***REMOVED***   private string MapLinkType(string linkType)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  switch (linkType)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** case "Epic": return "System.LinkTypes.Hierarchy-Reverse";
***REMOVED******REMOVED******REMOVED*** case "Parent": return "System.LinkTypes.Hierarchy-Reverse";
***REMOVED******REMOVED******REMOVED*** case "Relates": return "System.LinkTypes.Related";
***REMOVED******REMOVED******REMOVED*** case "Duplicate": return "System.LinkTypes.Duplicate-Forward";
***REMOVED******REMOVED******REMOVED*** default: return "System.LinkTypes.Related";
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private Dictionary<string, FieldMapping<JiraRevision>> InitializeFieldMappings(ConfigJson config)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var mappingPerWiType = new Dictionary<string, FieldMapping<JiraRevision>>();
***REMOVED******REMOVED***  var commonFields = new FieldMapping<JiraRevision>();
***REMOVED******REMOVED***  var bugFields = new FieldMapping<JiraRevision>();
***REMOVED******REMOVED***  var taskFields = new FieldMapping<JiraRevision>();
***REMOVED******REMOVED***  var pbiFields = new FieldMapping<JiraRevision>();
***REMOVED******REMOVED***  var epicFields = new FieldMapping<JiraRevision>();
***REMOVED******REMOVED***  var featureFields = new FieldMapping<JiraRevision>();
***REMOVED******REMOVED***  var requirementFields = new FieldMapping<JiraRevision>();
***REMOVED******REMOVED***  var userStoryFields = new FieldMapping<JiraRevision>();
***REMOVED******REMOVED***  List<string> witList = null;
***REMOVED******REMOVED***  var processFields = (from f in config.FieldMap.Fields where f.Process == "Common" || f.Process == "All" || f.Process == config.ProcessTemplate select f).ToList();
***REMOVED******REMOVED***  foreach (var item in processFields) 
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (item.Source != null) 
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***// If not-for and for has not been set (should never happen) then get all work item types
***REMOVED******REMOVED******REMOVED******REMOVED***if (string.IsNullOrWhiteSpace(item.NotFor) && string.IsNullOrWhiteSpace(item.For))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    if (witList == null)
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   witList = WorkItemType.GetWorkItemTypes();
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***else
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    // Check if not-for has been set, if so get all work item types except that one, else for has been set and get those
***REMOVED******REMOVED******REMOVED******REMOVED***    witList = !string.IsNullOrWhiteSpace(item.NotFor) ? GetWorkItemTypes(item.NotFor) : item.For.Split(',').ToList();
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Func<JiraRevision, (bool, object)> value;
***REMOVED******REMOVED******REMOVED******REMOVED***if (item.Type == "string")
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    switch (item.Mapper)
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   case "MapTitle":
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  value = r => MapTitle(r);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  break;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   case "MapUser":
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  value = IfChanged<string>(item.Source, MapUser);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  break;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   case "MapPriority":
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  value = IfChanged<string>(item.Source, MapPriority);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  break;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   case "MapSprint":
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  value = IfChanged<string>(jiraProvider.Settings.SprintField, MapSprint);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  break;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   case "MapTags":
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  value = IfChanged<string>(item.Source, MapTags);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  break;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   case "MapStateTask":
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  value = IfChanged<string>(item.Source, MapStateTask);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  break;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   case "MapStateBugAndPBI":
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  value = IfChanged<string>(item.Source, MapStateBugAndPBI);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  break;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   default:
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  value = IfChanged<string>(item.Source);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  break;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***else if (item.Type == "int")
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    value = IfChanged<int>(item.Source);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***else if (item.Type == "double")
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    value = IfChanged<double>(item.Source);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***else
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    // Mainly a fallback if no data type is set or is misspelled
***REMOVED******REMOVED******REMOVED******REMOVED***    value = IfChanged<string>(item.Source);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***

***REMOVED******REMOVED******REMOVED******REMOVED***foreach (var wit in witList)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    if (wit == "All" || wit == "Common")
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   commonFields.Add(item.Target, value);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    if (wit == WorkItemType.Bug)
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   bugFields.Add(item.Target, value);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    if (wit == WorkItemType.Epic)
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   epicFields.Add(item.Target, value);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    if (wit == WorkItemType.Feature)
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   featureFields.Add(item.Target, value);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    if (wit == WorkItemType.ProductBacklogItem)
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   pbiFields.Add(item.Target, value);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    if (wit == WorkItemType.Requirement)
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   requirementFields.Add(item.Target, value);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    if (wit == WorkItemType.Task)
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   taskFields.Add(item.Target, value);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    if (wit == WorkItemType.UserStory)
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   userStoryFields.Add(item.Target, value);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  mappingPerWiType.Add(WorkItemType.Bug, MergeMapping(commonFields, bugFields, taskFields));
***REMOVED******REMOVED***  mappingPerWiType.Add(WorkItemType.ProductBacklogItem, MergeMapping(commonFields, pbiFields));
***REMOVED******REMOVED***  mappingPerWiType.Add(WorkItemType.Task, MergeMapping(commonFields, bugFields, taskFields));
***REMOVED******REMOVED***  mappingPerWiType.Add(WorkItemType.Feature, MergeMapping(commonFields, featureFields));
***REMOVED******REMOVED***  mappingPerWiType.Add(WorkItemType.Epic, MergeMapping(commonFields, epicFields));
***REMOVED******REMOVED***  mappingPerWiType.Add(WorkItemType.Requirement, MergeMapping(commonFields, requirementFields));
***REMOVED******REMOVED***  mappingPerWiType.Add(WorkItemType.UserStory, MergeMapping(commonFields, userStoryFields));

***REMOVED******REMOVED***  return mappingPerWiType;
***REMOVED***   ***REMOVED***

***REMOVED***   private static Func<JiraRevision, (bool, object)> IfChanged<T>(string sourceField, Func<T, object> mapperFunc)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return (r) =>
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (r.Fields.TryGetValue(sourceField, out object value))
***REMOVED******REMOVED******REMOVED******REMOVED***return (true, mapperFunc((T)value));
***REMOVED******REMOVED******REMOVED*** else
***REMOVED******REMOVED******REMOVED******REMOVED***return (false, null);
***REMOVED******REMOVED***  ***REMOVED***;
***REMOVED***   ***REMOVED***

***REMOVED***   private static Func<JiraRevision, (bool, object)> IfChanged<T>(string sourceField)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return (r) =>
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (r.Fields.TryGetValue(sourceField, out object value))
***REMOVED******REMOVED******REMOVED******REMOVED***return (true, (T)value);
***REMOVED******REMOVED******REMOVED*** else
***REMOVED******REMOVED******REMOVED******REMOVED***return (false, null);
***REMOVED******REMOVED***  ***REMOVED***;
***REMOVED***   ***REMOVED***

***REMOVED***   private static Func<JiraRevision, (bool, object)> Init(object constant)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return (r) =>
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (r.Index == 0)
***REMOVED******REMOVED******REMOVED******REMOVED***return (true, constant);
***REMOVED******REMOVED******REMOVED*** else
***REMOVED******REMOVED******REMOVED******REMOVED***return (false, null);
***REMOVED******REMOVED***  ***REMOVED***;
***REMOVED***   ***REMOVED***

***REMOVED***   private (bool, object) MapTitle(JiraRevision r)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (r.Fields.TryGetValue("summary", out object summary))
***REMOVED******REMOVED******REMOVED*** return (true, $"[***REMOVED***r.ParentItem.Key***REMOVED***] ***REMOVED***summary***REMOVED***");
***REMOVED******REMOVED***  else
***REMOVED******REMOVED******REMOVED*** return (false, null);
***REMOVED***   ***REMOVED***

***REMOVED***   private string MapStateTask(string jiraState)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  jiraState = jiraState.ToLowerInvariant();
***REMOVED******REMOVED***  switch (jiraState)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** case "to do": return "To Do";
***REMOVED******REMOVED******REMOVED*** case "done": return "Done";
***REMOVED******REMOVED******REMOVED*** case "in progress": return "In Progress";
***REMOVED******REMOVED******REMOVED*** case "ready for test": return "Ready for test";
***REMOVED******REMOVED******REMOVED*** default: return "To Do";
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private object MapStateBugAndPBI(string jiraState)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  jiraState = jiraState.ToLowerInvariant();
***REMOVED******REMOVED***  switch (jiraState)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** case "to do": return "New";
***REMOVED******REMOVED******REMOVED*** case "done": return "Done";
***REMOVED******REMOVED******REMOVED*** case "in progress": return "Committed";
***REMOVED******REMOVED******REMOVED*** default: return "Committed";
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private object MapTags(string labels)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (string.IsNullOrWhiteSpace(labels))
***REMOVED******REMOVED******REMOVED*** return null;

***REMOVED******REMOVED***  var tags = labels.Split(' ');
***REMOVED******REMOVED***  if (!tags.Any())
***REMOVED******REMOVED******REMOVED*** return null;
***REMOVED******REMOVED***  else
***REMOVED******REMOVED******REMOVED*** return string.Join(";", tags);
***REMOVED***   ***REMOVED***

***REMOVED***   private object MapSprint(string iterationPathsString)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (string.IsNullOrWhiteSpace(iterationPathsString))
***REMOVED******REMOVED******REMOVED*** return null;

***REMOVED******REMOVED***  var iterationPaths = iterationPathsString.Split(',').AsEnumerable();
***REMOVED******REMOVED***  iterationPaths = iterationPaths.Select(ip => ip.Trim());

***REMOVED******REMOVED***  var iterationPath = iterationPaths.Last();

***REMOVED******REMOVED***  return iterationPath;
***REMOVED***   ***REMOVED***

***REMOVED***   private object MapPriority(string jiraPriority)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  switch (jiraPriority.ToLowerInvariant())
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** case "blocker":
***REMOVED******REMOVED******REMOVED*** case "critical":
***REMOVED******REMOVED******REMOVED*** case "highest": return 1;
***REMOVED******REMOVED******REMOVED*** case "major":
***REMOVED******REMOVED******REMOVED*** case "high": return 2;
***REMOVED******REMOVED******REMOVED*** case "medium":
***REMOVED******REMOVED******REMOVED*** case "low": return 3;
***REMOVED******REMOVED******REMOVED*** case "lowest":
***REMOVED******REMOVED******REMOVED*** case "minor":
***REMOVED******REMOVED******REMOVED*** case "trivial": return 4;
***REMOVED******REMOVED******REMOVED*** default: return null;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private object MapSeverity(string jiraSeverity)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  switch (jiraSeverity.ToLowerInvariant())
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** case "blocker":
***REMOVED******REMOVED******REMOVED*** case "critical":
***REMOVED******REMOVED******REMOVED*** case "highest": return 1;
***REMOVED******REMOVED******REMOVED*** case "major":
***REMOVED******REMOVED******REMOVED*** case "high": return 2;
***REMOVED******REMOVED******REMOVED*** case "medium":
***REMOVED******REMOVED******REMOVED*** case "low": return 3;
***REMOVED******REMOVED******REMOVED*** case "lowest":
***REMOVED******REMOVED******REMOVED*** case "minor":
***REMOVED******REMOVED******REMOVED*** case "trivial": return 4;
***REMOVED******REMOVED******REMOVED*** default: return null;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   protected string MapType(string type, TemplateType template)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  string backlogItem;
***REMOVED******REMOVED***  switch (template)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** case TemplateType.Scrum:
***REMOVED******REMOVED******REMOVED******REMOVED***backlogItem = WorkItemType.ProductBacklogItem;
***REMOVED******REMOVED******REMOVED******REMOVED***break;
***REMOVED******REMOVED******REMOVED*** case TemplateType.Agile:
***REMOVED******REMOVED******REMOVED******REMOVED***backlogItem = WorkItemType.UserStory;
***REMOVED******REMOVED******REMOVED******REMOVED***break;
***REMOVED******REMOVED******REMOVED*** case TemplateType.CMMI:
***REMOVED******REMOVED******REMOVED******REMOVED***backlogItem = WorkItemType.Requirement;
***REMOVED******REMOVED******REMOVED******REMOVED***break;
***REMOVED******REMOVED******REMOVED*** default:
***REMOVED******REMOVED******REMOVED******REMOVED***backlogItem = WorkItemType.ProductBacklogItem;
***REMOVED******REMOVED******REMOVED******REMOVED***break;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  switch (type)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** case "Task": return backlogItem;
***REMOVED******REMOVED******REMOVED*** case "Sub-task": return WorkItemType.Task;
***REMOVED******REMOVED******REMOVED*** case "Story": return backlogItem; 
***REMOVED******REMOVED******REMOVED*** case "Bug": return WorkItemType.Bug;
***REMOVED******REMOVED******REMOVED*** case "Epic": return WorkItemType.Feature; 
***REMOVED******REMOVED******REMOVED*** default: return backlogItem;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private void MapLastDescription(List<WiRevision> revisions, JiraItem issue, TemplateType template)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var descFieldName = issue.Type == "Bug" ? "Microsoft.VSTS.TCM.ReproSteps" : "System.Description"; 

***REMOVED******REMOVED***  var lastDescUpdateRev =
***REMOVED******REMOVED******REMOVED*** ((IEnumerable<WiRevision>)revisions)
***REMOVED******REMOVED******REMOVED***.Reverse()
***REMOVED******REMOVED******REMOVED***.FirstOrDefault(r => r.Fields.Any(i => i.ReferenceName.Equals(descFieldName, StringComparison.InvariantCultureIgnoreCase)));

***REMOVED******REMOVED***  var lastDescUpdate = lastDescUpdateRev
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  ?.Fields
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  ?.FirstOrDefault(i => i.ReferenceName.Equals(descFieldName, StringComparison.InvariantCultureIgnoreCase));


***REMOVED******REMOVED***  var renderedDescription = MapRenderedDescription(issue);

***REMOVED******REMOVED***  if (lastDescUpdate == null && !string.IsNullOrWhiteSpace(renderedDescription))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** lastDescUpdate = new Migration.WIContract.WiField() ***REMOVED*** ReferenceName = descFieldName, Value = renderedDescription ***REMOVED***;
***REMOVED******REMOVED******REMOVED*** lastDescUpdateRev = revisions.First();
***REMOVED******REMOVED******REMOVED*** lastDescUpdateRev.Fields.Add(lastDescUpdate);
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  lastDescUpdate.Value = renderedDescription;

***REMOVED******REMOVED***  lastDescUpdateRev.AttachmentReferences = true;
***REMOVED***   ***REMOVED***

***REMOVED***   private string MapRenderedDescription(JiraItem issue)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  string originalHtml = issue.RemoteIssue.ExValue<string>("$.renderedFields.description");
***REMOVED******REMOVED***  string wiHtml = originalHtml;

***REMOVED******REMOVED***  foreach (var att in issue.Revisions.SelectMany(r => r.AttachmentActions.Where(aa => aa.ChangeType == RevisionChangeType.Added).Select(aa => aa.Value)))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (!string.IsNullOrWhiteSpace(att.ThumbUrl) && wiHtml.Contains(att.ThumbUrl))
***REMOVED******REMOVED******REMOVED******REMOVED***wiHtml.Replace(att.ThumbUrl, att.ThumbUrl);

***REMOVED******REMOVED******REMOVED*** if (!string.IsNullOrWhiteSpace(att.Url) && wiHtml.Contains(att.Url))
***REMOVED******REMOVED******REMOVED******REMOVED***wiHtml.Replace(att.Url, att.Url);
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  string imageWrapPattern = "<span class=\"image-wrap\".*?>.*?(<img .*? />).*?</span>";
***REMOVED******REMOVED***  wiHtml = Regex.Replace(wiHtml, imageWrapPattern, m => m.Groups[1]?.Value);

***REMOVED******REMOVED***  string userLinkPattern = "<a href=.*? class=\"user-hover\" .*?>(.*?)</a>";
***REMOVED******REMOVED***  wiHtml = Regex.Replace(wiHtml, userLinkPattern, m => m.Groups[1]?.Value);

***REMOVED******REMOVED***  string css = ReadEmbeddedFile("JiraExport.jirastyles.css");
***REMOVED******REMOVED***  if (string.IsNullOrWhiteSpace(css))
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Warning, "Could not read css styles for description.");
***REMOVED******REMOVED***  else
***REMOVED******REMOVED******REMOVED*** wiHtml = "<style>" + css + "</style>" + wiHtml;


***REMOVED******REMOVED***  return wiHtml ?? string.Empty;
***REMOVED***   ***REMOVED***

***REMOVED***   #endregion
***REMOVED***
***REMOVED***