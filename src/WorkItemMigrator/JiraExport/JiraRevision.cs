using Migration.Common;
using Newtonsoft.Json.Linq;
***REMOVED***
***REMOVED***
***REMOVED***

***REMOVED***
***REMOVED***
    public enum RevisionChangeType
    ***REMOVED***
***REMOVED***   Added,
***REMOVED***   Removed
***REMOVED***

    public class RevisionAction<T>
    ***REMOVED***
***REMOVED***   public RevisionChangeType ChangeType ***REMOVED*** get; set; ***REMOVED***
***REMOVED***   public T Value;

***REMOVED***   public override string ToString()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return $"***REMOVED***ChangeType.ToString()***REMOVED*** ***REMOVED***Value.ToString()***REMOVED***";
***REMOVED***   ***REMOVED***
***REMOVED***

    public class JiraRevision : ISourceRevision, IComparable<JiraRevision>
    ***REMOVED***
***REMOVED***   public DateTime Time ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   public string Author ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   public Dictionary<string, object> Fields ***REMOVED*** get; set; ***REMOVED***
***REMOVED***   public List<RevisionAction<JiraLink>> LinkActions ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   public List<RevisionAction<JiraAttachment>> AttachmentActions ***REMOVED*** get; set; ***REMOVED***
***REMOVED***   public JiraItem ParentItem ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public int Index ***REMOVED*** get; internal set; ***REMOVED***

***REMOVED***   public string OriginId => ParentItem.Key;

***REMOVED***   public string Type => ParentItem.Type;


***REMOVED***   public JiraRevision(JiraItem parentItem)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  ParentItem = parentItem;
***REMOVED***   ***REMOVED***

***REMOVED***   public int CompareTo(JiraRevision other)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  int t = this.Time.CompareTo(other.Time);
***REMOVED******REMOVED***  if (t != 0)
***REMOVED******REMOVED******REMOVED*** return t;

***REMOVED******REMOVED***  return this.ParentItem.Key.CompareTo(other.ParentItem.Key);
***REMOVED***   ***REMOVED***

***REMOVED***   public string GetFieldValue(string fieldName)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return (string)(((IEnumerable<JiraRevision>)ParentItem.Revisions)
***REMOVED******REMOVED******REMOVED*** .Reverse()
***REMOVED******REMOVED******REMOVED*** .SkipWhile(r => r.Index > this.Index)
***REMOVED******REMOVED******REMOVED*** .FirstOrDefault(r => r.Fields.ContainsKey(fieldName))
***REMOVED******REMOVED******REMOVED*** ?.Fields[fieldName]);
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***