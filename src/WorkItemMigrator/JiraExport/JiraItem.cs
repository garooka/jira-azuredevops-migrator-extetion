﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Atlassian.Jira;
using Migration.Common;
using Migration.Common.Config;
using Migration.Common.Log;

using Newtonsoft.Json.Linq;

namespace JiraExport
{
    public class JiraItem
    {
        #region Static

        public static JiraItem CreateFromRest(string issueKey, IJiraProvider jiraProvider)
        {
            var remoteIssue = jiraProvider.DownloadIssue(issueKey);
            if (remoteIssue == null)
                return default(JiraItem);

            Logger.Log(LogLevel.Debug, $"Downloaded item.");

            var jiraItem = new JiraItem(jiraProvider, remoteIssue);
            var revisions = BuildRevisions(jiraItem, jiraProvider);
            jiraItem.Revisions = revisions;
            Logger.Log(LogLevel.Debug, $"Created {revisions.Count} history revisions.");

            return jiraItem;

        }

        private static List<JiraRevision> BuildRevisions(JiraItem jiraItem, IJiraProvider jiraProvider)
        {
            string issueKey = jiraItem.Key;
            var remoteIssue = jiraItem.RemoteIssue;
            Dictionary<string, object> fieldsTemp = ExtractFields(issueKey, remoteIssue, jiraProvider);

            // Add CustomFieldName fields, copy over all non-custom fields.
            // These get removed as we loop over the changeLog, so we're left with the original Jira values by the time we reach firstRevision.
            Dictionary<string, object> fields = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var field in fieldsTemp)
            {
                var key = GetCustomFieldName(field.Key, jiraProvider);
                if (!String.IsNullOrEmpty(key))
                {
                    fields[key] = field.Value;
                } else
                {
                    fields[field.Key] = field.Value;
                }
            }

            List<JiraAttachment> attachments = ExtractAttachments(remoteIssue.SelectTokens("$.fields.attachment[*]").Cast<JObject>()) ?? new List<JiraAttachment>();
            List<JiraLink> links = ExtractLinks(issueKey, remoteIssue.SelectTokens("$.fields.issuelinks[*]").Cast<JObject>()) ?? new List<JiraLink>();
            var epicLinkField = jiraProvider.GetSettings().EpicLinkField;

            // save these field since these might be removed in the loop
            string reporter = GetAuthor(fields);
            var createdOn = fields.TryGetValue("created", out object crdate) ? (DateTime)crdate : default(DateTime);
            if (createdOn == DateTime.MinValue)
                Logger.Log(LogLevel.Debug, "created key was not found, using DateTime default value");

            var changelog = jiraProvider.DownloadChangelog(issueKey).OrderByDescending(c => (long)c.SelectToken("$.id")).ToList();
            Logger.Log(LogLevel.Debug, $"Downloaded issue: {issueKey} changelog.");

            Stack<JiraRevision> revisions = new Stack<JiraRevision>();

            foreach (var change in changelog)
            {
                DateTime created = change.ExValue<DateTime>("$.created");
                string author = GetAuthor(change);

                List<RevisionAction<JiraLink>> linkChanges = new List<RevisionAction<JiraLink>>();
                List<RevisionAction<JiraAttachment>> attachmentChanges = new List<RevisionAction<JiraAttachment>>();
                Dictionary<string, object> fieldChanges = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);



                var items = change.SelectTokens("$.items[*]")?.Cast<JObject>()?.Select(i => new JiraChangeItem(i));
                foreach (var item in items)
                {
                    switch (item.Field)
                    {
                        case "Epic Link" when !string.IsNullOrWhiteSpace(epicLinkField):
                            HandleCustomFieldChange(item, epicLinkField, fieldChanges, fields);
                            break;
                        case "Parent":
                        case "IssueParentAssociation":
                            HandleCustomFieldChange(item, "parent", fieldChanges, fields);
                            break;
                        case "Link":
                            HandleLinkChange(item, issueKey, jiraProvider, linkChanges, links);
                            break;
                        case "Attachment":
                            HandleAttachmentChange(item, attachmentChanges, attachments);
                            break;
                        default:
                            HandleFieldChange(item, jiraProvider, fieldChanges, fields);
                            break;
                    }
                }

                var revision = new JiraRevision(jiraItem) { Time = created, Author = author, AttachmentActions = attachmentChanges, LinkActions = linkChanges, Fields = fieldChanges };
                revisions.Push(revision);
            }

            // what is left after undoing all changes is first revision
            var attActions = attachments.Select(a => new RevisionAction<JiraAttachment>() { ChangeType = RevisionChangeType.Added, Value = a }).ToList();
            var linkActions = links.Select(l => new RevisionAction<JiraLink>() { ChangeType = RevisionChangeType.Added, Value = l }).ToList();
            var fieldActions = fields;

            var firstRevision = new JiraRevision(jiraItem) { Time = createdOn, Author = reporter, AttachmentActions = attActions, Fields = fieldActions, LinkActions = linkActions };
            revisions.Push(firstRevision);
            var listOfRevisions = revisions.ToList();

            List<JiraRevision> commentRevisions = BuildCommentRevisions(jiraItem, jiraProvider);
            listOfRevisions.AddRange(commentRevisions);
            listOfRevisions.Sort();

            foreach (var revAndI in listOfRevisions.Select((r, i) => (r, i)))
                revAndI.Item1.Index = revAndI.Item2;

            return listOfRevisions;
        }


        private static void HandleCustomFieldChange(JiraChangeItem item, string customFieldName, Dictionary<string, object> fieldChanges, Dictionary<string, object> fields)
        {
            fieldChanges[customFieldName] = item.ToString;

            // undo field change
            if (string.IsNullOrWhiteSpace(item.From))
                fields.Remove(customFieldName);
            else
                fields[customFieldName] = item.FromString;
        }

        private static void HandleFieldChange(JiraChangeItem item, IJiraProvider jiraProvider, Dictionary<string, object> fieldChanges, Dictionary<string, object> fields)
        {
            //var (fieldref, from, to) = TransformFieldChange(item, jiraProvider, fields);

            var objectFields = new HashSet<string>() { "assignee", "creator", "reporter" };
            object from, to;
            
            string fieldref = string.IsNullOrEmpty(item.Field) ? GetCustomFieldId(item.Field, jiraProvider) : GetFieldName(item.Field, jiraProvider);

            fields.TryGetValue(fieldref, out object aFieldValueFrom);
            fieldChanges.TryGetValue(fieldref, out object aFieldValueTo);

            if (objectFields.Contains(fieldref))
            {
                from = item.From;
                to = item.To;
            }
            else
            {
                from = item.FromString;
                to = item.ToString;
            }

            // undo field change
            if (aFieldValueFrom != null && aFieldValueFrom is List<string> &&
                aFieldValueTo != null && aFieldValueTo is List<string>)
            {
                List<string> aFromList = (List<string>)aFieldValueFrom;
                List<string> aToList = (List<string>)aFieldValueTo;

                //never remove the empty List<string>, because it won't be possible to determine the type: fields.Remove(fieldref);

                if (!string.IsNullOrEmpty(from as string ?? "")) aFromList.Add(from.ToString());

                if (!string.IsNullOrEmpty(to as string ?? "")) aToList.Add(to.ToString());
            }
            else if (string.IsNullOrEmpty(from as string ?? ""))
            {
                fields.Remove(fieldref);
                fieldChanges[fieldref] = to;
            }
            else
            {
                fields[fieldref] = from;
                fieldChanges[fieldref] = to;
            }            
        }

        private static void HandleLinkChange(JiraChangeItem item, string issueKey, IJiraProvider jiraProvider, List<RevisionAction<JiraLink>> linkChanges, List<JiraLink> links)
        {
            var linkChange = TransformLinkChange(item, issueKey, jiraProvider);
            if (linkChange == null)
                return;

            linkChanges.Add(linkChange);
            UndoLinkChange(linkChange, links);
        }

        private static void HandleAttachmentChange(JiraChangeItem item, List<RevisionAction<JiraAttachment>> attachmentChanges, List<JiraAttachment> attachments)
        {
            var attachmentChange = TransformAttachmentChange(item);
            if (attachmentChange == null)
                return;

            if (UndoAttachmentChange(attachmentChange, attachments))
            {
                attachmentChanges.Add(attachmentChange);
            }
            else
            {
                Logger.Log(LogLevel.Debug, $"Attachment {item.ToString ?? item.FromString} cannot be migrated because it was deleted.");
            }
        }

        private static List<JiraRevision> BuildCommentRevisions(JiraItem jiraItem, IJiraProvider jiraProvider)
        {
            var renderedFields = jiraItem.RemoteIssue.SelectToken("$.renderedFields.comment.comments");
            var comments = jiraProvider.GetCommentsByItemKey(jiraItem.Key);
            return comments.Select((c, i) =>
            {
                var rc = renderedFields.SelectToken($"$.[{i}].body");
                return BuildCommentRevision(c, rc, jiraItem);
            }).ToList();
        }

        private static JiraRevision BuildCommentRevision(Comment c, JToken rc, JiraItem jiraItem)
        {
            var author = "NoAuthorDefined";
            if (c.AuthorUser is null)
            {
                Logger.Log(LogLevel.Warning, $"c.AuthorUser is null in comment revision for jiraItem.Key: '{jiraItem.Key}'. Using NoAuthorDefined as author. ");
            }
            else
            {
                if (c.AuthorUser.Username is null)
                {
                    author = GetAuthorIdentityOrDefault(c.AuthorUser.AccountId);
                }
                else
                {
                    author = c.AuthorUser.Username;
                }
            }

            return new JiraRevision(jiraItem)
            {
                Author = author,
                Time = c.CreatedDate.Value,
                Fields = new Dictionary<string, object>() { { "comment", c.Body }, { "comment$Rendered", rc.Value<string>() } },
                AttachmentActions = new List<RevisionAction<JiraAttachment>>(),
                LinkActions = new List<RevisionAction<JiraLink>>()
            };
        }

        private static bool UndoAttachmentChange(RevisionAction<JiraAttachment> attachmentChange, List<JiraAttachment> attachments)
        {
            if (attachmentChange.ChangeType == RevisionChangeType.Removed)
            {
                Logger.Log(LogLevel.Debug, $"Skipping undo for attachment '{attachmentChange.ToString()}'.");
                return false;
            }
            return RemoveAttachment(attachmentChange, attachments);
        }

        private static bool RemoveAttachment(RevisionAction<JiraAttachment> attachmentChange, List<JiraAttachment> attachments)
        {
            var result = attachments.Remove(attachmentChange.Value);
            if (result)
                Logger.Log(LogLevel.Debug, $"Undone attachment '{attachmentChange.ToString()}'.");
            else
                Logger.Log(LogLevel.Debug, $"No attachment to undo for '{attachmentChange.ToString()}'.");
            return result;
        }

        private static RevisionAction<JiraAttachment> TransformAttachmentChange(JiraChangeItem item)
        {
            string attKey = string.Empty;
            string attFilename = string.Empty;

            RevisionChangeType changeType;

            if (item.From == null && item.To != null)
            {
                attKey = item.To;
                attFilename = item.ToString;
                changeType = RevisionChangeType.Added;
            }
            else if (item.To == null && item.From != null)
            {
                attKey = item.From;
                attFilename = item.FromString;
                changeType = RevisionChangeType.Removed;
            }
            else
            {
                Logger.Log(LogLevel.Error, "Attachment change not handled!");
                return null;
            }

            return new RevisionAction<JiraAttachment>()
            {
                ChangeType = changeType,
                Value = new JiraAttachment()
                {
                    Id = attKey,
                    Filename = attFilename
                }
            };
        }

        /*
         * Obsolete
         */
        private static (string, object, object) TransformFieldChange(JiraChangeItem item, IJiraProvider jira, Dictionary<string, object> fields)
        {
            var objectFields = new HashSet<string>() { "assignee", "creator", "reporter" };
            object from, to;
            //TODO cleanup string from, to = string.Empty;

            string fieldId = string.IsNullOrEmpty(item.Field) ? GetCustomFieldId(item.Field, jira) : GetFieldName(item.Field, jira);

            fields.TryGetValue(fieldId, out object aFieldValue);

            if (objectFields.Contains(fieldId))
            {
                from = item.From;
                to = item.To;
            }
            else
            {
                from = item.FromString;
                to = item.ToString;
            }

            // undo field change
            if (aFieldValue != null && aFieldValue is List<string>)
            {
                List<string> aList = new List<string>((List<string>)aFieldValue);

                if (!string.IsNullOrEmpty(to as string ?? "")) aList.Remove(to.ToString());

                if (string.IsNullOrEmpty(from as string ?? "")) aList.Add(from.ToString());

                if (!aList.Any())
                {
                    fields.Remove(fieldId);
                } else
                {
                    fields[fieldId] = aList;
                }
            }
            else if (string.IsNullOrEmpty(from as string ?? ""))
            {
                fields.Remove(fieldId);
            }
            else
            {
                fields[fieldId] = from;
            }

            return (fieldId, from, to);
        }

        private static string GetCustomFieldId(string fieldName, IJiraProvider jira)
        {
            var customField = jira.GetCustomField(fieldName);
            if (customField != null)
                return customField.Id;
            else return null;

        }

        protected static string GetCustomFieldName(string fieldId, IJiraProvider jira)
        {
            var customField = jira.GetCustomField(fieldId);
            if (customField != null)
                return customField.Name;
            else return null;

        }

        protected static string GetFieldName(string fieldId, IJiraProvider jira)
        {
            var objectFields = new Dictionary<string, string>() {
                { "components", "Component" },
                { "fixVersions", "Fix Version" }
            };

            objectFields.TryGetValue(fieldId, out string aFieldName);
            
            return string.IsNullOrEmpty(aFieldName) ? fieldId : aFieldName;
        }
        
        private static void UndoLinkChange(RevisionAction<JiraLink> linkChange, List<JiraLink> links)
        {
            if (linkChange.ChangeType == RevisionChangeType.Removed)
            {
                Logger.Log(LogLevel.Debug, $"Skipping undo for link '{linkChange.ToString()}'.");
                return;
            }

            if (links.Remove(linkChange.Value))
                Logger.Log(LogLevel.Debug, $"Undone link '{linkChange.ToString()}'.");
            else
                Logger.Log(LogLevel.Debug, $"No link to undo for '{linkChange.ToString()}'");
        }
        private static RevisionAction<JiraLink> TransformLinkChange(JiraChangeItem item, string sourceItemKey, IJiraProvider jira)
        {
            string targetItemKey = string.Empty;
            string linkTypeString = string.Empty;
            RevisionChangeType changeType;

            if (item.From == null && item.To != null)
            {
                targetItemKey = item.To;
                linkTypeString = item.ToString;
                changeType = RevisionChangeType.Added;
            }
            else if (item.To == null && item.From != null)
            {
                targetItemKey = item.From;
                linkTypeString = item.FromString;
                changeType = RevisionChangeType.Removed;
            }
            else
            {
                Logger.Log(LogLevel.Error, $"Link change not handled!");
                return null;
            }
            var linkType = jira.GetLinkType(linkTypeString, targetItemKey);
            if (linkType == null)
            {
                Logger.Log(LogLevel.Debug, $"Link with description '{linkTypeString}' is either not found or this issue ({sourceItemKey}) is not inward issue.");
                return null;
            }
            else
            {
                if (linkType.Inward == linkType.Outward && sourceItemKey.CompareTo(targetItemKey) < 0)
                {
                    Logger.Log(LogLevel.Debug, $"Link is non-directional ({linkType.Name}) and sourceItem ({sourceItemKey}) is older then target item ({targetItemKey}). Link change will be part of target item.");
                    return null;
                }

                return new RevisionAction<JiraLink>()
                {
                    ChangeType = changeType,
                    Value = new JiraLink()
                    {
                        SourceItem = sourceItemKey,
                        TargetItem = targetItemKey,
                        LinkType = linkType.Name,
                    }
                };
            }
        }

        private static List<JiraLink> ExtractLinks(string sourceKey, IEnumerable<JObject> issueLinks)
        {
            var links = new List<JiraLink>();

            foreach (var issueLink in issueLinks)
            {
                var targetIssueKey = issueLink.ExValue<string>("$.outwardIssue.key");
                if (string.IsNullOrWhiteSpace(targetIssueKey))
                    continue;

                var type = issueLink.ExValue<string>("$.type.name");

                var link = new JiraLink() { SourceItem = sourceKey, TargetItem = targetIssueKey, LinkType = type };
                links.Add(link);
            }

            return links;
        }

        private static List<JiraAttachment> ExtractAttachments(IEnumerable<JObject> attachmentObjs)
        {
            return attachmentObjs.Select(attObj =>
            {
                return new JiraAttachment
                {
                    Id = attObj.ExValue<string>("$.id"),
                    Filename = attObj.ExValue<string>("$.filename"),
                    Url = attObj.ExValue<string>("$.content")
                };
            }).ToList();
        }

        private static Dictionary<string, Func<JToken, object>> _fieldExtractionMapping = null;
        private static Dictionary<string, object> ExtractFields(string key, JObject remoteIssue, IJiraProvider jira)
        {
            var fields = new Dictionary<string, object>();

            var remoteFields = (JObject)remoteIssue.SelectToken("$.fields");
            var renderedFields = (JObject)remoteIssue.SelectToken("$.renderedFields");

            var extractName = new Func<JToken, object>((t) => t.ExValue<string>("$.name"));
            var extractAccountIdOrUsername = new Func<JToken, object>((t) => t.ExValue<string>("$.name") ?? t.ExValue<string>("$.accountId"));

            if (_fieldExtractionMapping == null)
            {
                _fieldExtractionMapping = new Dictionary<string, Func<JToken, object>>()
                    {
                        { "priority", extractName },
                        { "labels", t => t.Values<string>().Any() ? string.Join(" ", t.Values<string>()) : null },
                        { "assignee", extractAccountIdOrUsername },
                        { "creator", extractAccountIdOrUsername },
                        { "reporter", extractAccountIdOrUsername},
                        { jira.GetSettings().SprintField, t => string.Join(", ", ParseCustomField(jira.GetSettings().SprintField, t, jira)) },
                        { "status", extractName },
                        { "parent", t => t.ExValue<string>("$.key") }
                    };
            }

            foreach (var prop in remoteFields.Properties())
            {
                var type = prop.Value.Type;
                var name = prop.Name; // Remove ToLower() method, because field name is not matched by the Revision processing 
                object value = null;

                if (_fieldExtractionMapping.TryGetValue(name, out Func<JToken, object> mapping))
                {
                    value = mapping(prop.Value);
                }
                else if (type == JTokenType.String || type == JTokenType.Integer || type == JTokenType.Float)
                {
                    value = prop.Value.Value<string>();
                }
                else if (prop.Value.Type == JTokenType.Date)
                {
                    value = prop.Value.Value<DateTime>();
                }
                else if (type == JTokenType.Array && prop.Value.Any())
                {
                    /*
                     * Changed from string to List, because by Revision processing there is now way to correctly apply single item removal from array/List
                     */
                    List<string> outValue = prop.Value.Select(st => st.ExValue<string>("$.name")).ToList();
                    
                    if(outValue == null || !outValue.Any() || outValue.All(string.IsNullOrEmpty)) {

                        outValue = prop.Value.Select(st => st.ExValue<string>("$.value")).ToList();
                    }

                    value = outValue == null || !outValue.Any() || outValue.All(string.IsNullOrEmpty) ? new List<string>() : outValue;
                }
                else if (type == Newtonsoft.Json.Linq.JTokenType.Object && prop.Value["value"] != null)
                {
                    value = prop.Value["value"].ToString();
                }

                if (value != null)
                {
                    fields[name] = value;

                    if (renderedFields.TryGetValue(name, out JToken rendered))
                    {
                        if (rendered.Type == JTokenType.String)
                        {
                            fields[name + "$Rendered"] = rendered.Value<string>();
                        }
                        else
                        {
                            Logger.Log(LogLevel.Debug, $"Rendered field {name} contains unparsable type {rendered.Type.ToString()}, using text");
                        }
                    }
                }

                //TODO
                Dictionary<string, object>  outDict = ExtendedExtractFields(prop, type == null);
                fields = fields.Concat(outDict).ToDictionary(x => x.Key, x => x.Value);
            }

            fields["key"] = key;
            fields["issuekey"] = key;
            return fields;
        }


        private static List<string> interestingKeys = new List<string>()
        {
            "name",
            "description",
            "key",
            "releaseDate",
            "released"
        };


        private static Dictionary<string, object> ExtendedExtractFields(JProperty aProperty, bool processSimpletype)
        {
            var result = new Dictionary<string, object>();

            if (processSimpletype)
            {
                // keyPath - removed ToLower() method because of the Revision procesing errot
                var keyPath = $"{aProperty.Name}";

                var anOutList = ExtractFieldValues(aProperty, keyPath);

                if (anOutList.Any())
                {
                    result[keyPath] = anOutList.ToList().Count == 1 ? anOutList.First() : anOutList.ToList();
                }
            }

            foreach (var interestingKey in interestingKeys)
            {
                var keyPath = $"{aProperty.Name}_{interestingKey}";                   

                var anOutList = ExtractFieldValues(aProperty, interestingKey);

                if (anOutList.Any())
                {
                    result[keyPath] = anOutList.ToList().Count == 1 ? anOutList.First() : anOutList.ToList();
                }
            }

            return result;
        }

        private static IEnumerable<object> ExtractFieldValues(JProperty aProperty, string aFieldKey)
        {
            //var values = aProperty.DescendantsAndSelf()
            var values = aProperty.Children()
                .Where(jt => jt.Type == JTokenType.Object && jt[aFieldKey] != null)
                .Select(jt => jt[aFieldKey].ToObject<object>());

            var valuesArray = aProperty.Children()
                .Where(jt => jt.Type == JTokenType.Array).Children()
                .Where(jt => jt.Type == JTokenType.Object && jt[aFieldKey] != null)
                .Select(jt => jt[aFieldKey].ToObject<object>());

            return values.Concat(valuesArray);
        } 
        private static string GetAuthor(Dictionary<string, object> fields)
        {
            var reporter = fields.TryGetValue("reporter", out object rep) ? (string)rep : null;

            return GetAuthorIdentityOrDefault(reporter);
        }
        private static string GetAuthor(JObject change)
        {
            var author = change.ExValue<string>("$.author.name") ?? change.ExValue<string>("$.author.accountId");
            return GetAuthorIdentityOrDefault(author);

        }

        private static string GetAuthorIdentityOrDefault(string author)
        {
            if (string.IsNullOrEmpty(author))
                return default(string);

            return author;

        }

        private static string[] ParseCustomField(string fieldName, JToken value, IJiraProvider provider)
        {
            var serializedValue = new string[] { };
            var customField = provider.GetCustomField(fieldName);
            if (customField != null &&
                provider.GetCustomFieldSerializer(customField.CustomType, out var serializer))
            {
                serializedValue = serializer.FromJson(value);
            }

            return serializedValue;
        }

        #endregion

        private readonly IJiraProvider _provider;

        public string Key { get { return RemoteIssue.ExValue<string>("$.key"); } }
        public string Type { get { return RemoteIssue.ExValue<string>("$.fields.issuetype.name")?.Trim(); } }
        public string EpicParent
        {
            get
            {
                if (!string.IsNullOrEmpty(_provider.GetSettings().EpicLinkField))
                    return RemoteIssue.ExValue<string>($"$.fields.{_provider.GetSettings().EpicLinkField}");
                else
                    return null;
            }
        }
        public string Parent { get { return RemoteIssue.ExValue<string>("$.fields.parent.key"); } }
        public List<string> SubItems { get { return GetSubTasksKey(); } }

        public JObject RemoteIssue { get; private set; }
        public List<JiraRevision> Revisions { get; set; }
        private JiraItem(IJiraProvider provider, JObject remoteIssue)
        {
            this._provider = provider;
            RemoteIssue = remoteIssue;
        }
        internal string GetUserEmail(string author)
        {
            return _provider.GetUserEmail(author);
        }
        internal List<string> GetSubTasksKey()
        {
            return RemoteIssue.SelectTokens("$.fields.subtasks.[*]", false).Select(st => st.ExValue<string>("$.key")).ToList();
        }
    }
}
