using Newtonsoft.Json;
***REMOVED***
***REMOVED***
using System.ComponentModel;

namespace Migration.WIContract
***REMOVED***
    public enum ReferenceChangeType
    ***REMOVED***
***REMOVED***   Added,
***REMOVED***   Removed
***REMOVED***

    public enum TemplateType
    ***REMOVED***
***REMOVED***   Scrum,
***REMOVED***   Agile,
***REMOVED***   CMMI
***REMOVED***

    public class WorkItemType
    ***REMOVED***
***REMOVED***   public static string ProductBacklogItem => "Product Backlog Item";
***REMOVED***   public static string UserStory => "User Story";
***REMOVED***   public static string Requirement => "Requirement";
***REMOVED***   public static string Bug => "Bug";
***REMOVED***   public static string Task => "Task";
***REMOVED***   public static string Epic => "Epic";
***REMOVED***   public static string Feature => "Feature";

***REMOVED***   public static List<string> GetWorkItemTypes(string notFor = "")
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var list = new List<string>();
***REMOVED******REMOVED***  var properties = typeof(WorkItemType).GetProperties();
***REMOVED******REMOVED***  foreach (var prop in properties)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var value = prop.GetValue(typeof(WorkItemType)).ToString();
***REMOVED******REMOVED******REMOVED*** if (!string.IsNullOrWhiteSpace(notFor))
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***if (value != notFor)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    list.Add(value);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** else
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***list.Add(value);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  return list;
***REMOVED***   ***REMOVED***
***REMOVED***

    public class WiRevision
    ***REMOVED***
***REMOVED***   public WiRevision()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  Fields = new List<WiField>();
***REMOVED******REMOVED***  Links = new List<WiLink>();
***REMOVED******REMOVED***  Attachments = new List<WiAttachment>();
***REMOVED***   ***REMOVED***

***REMOVED***   [JsonIgnore]
***REMOVED***   public string ParentOriginId ***REMOVED*** get; set; ***REMOVED***
***REMOVED***   public string Author ***REMOVED*** get; set; ***REMOVED***
***REMOVED***   public DateTime Time ***REMOVED*** get; set; ***REMOVED*** = DateTime.Now;
***REMOVED***   public int Index ***REMOVED*** get; set; ***REMOVED*** = 1;
***REMOVED***   public List<WiField> Fields ***REMOVED*** get; set; ***REMOVED***
***REMOVED***   public List<WiLink> Links ***REMOVED*** get; set; ***REMOVED***
***REMOVED***   public List<WiAttachment> Attachments ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [DefaultValue(false)]
***REMOVED***   public bool AttachmentReferences ***REMOVED*** get; set; ***REMOVED*** = false;

***REMOVED***   public override string ToString()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return $"(***REMOVED***ParentOriginId***REMOVED***, ***REMOVED***Index***REMOVED***)";
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***