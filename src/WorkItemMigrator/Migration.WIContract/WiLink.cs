using Newtonsoft.Json;

namespace Migration.WIContract
***REMOVED***
    public class WiLink
    ***REMOVED***
***REMOVED***   public ReferenceChangeType Change ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonIgnore]
***REMOVED***   public string SourceOriginId ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   public string TargetOriginId ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonIgnore]
***REMOVED***   public int SourceWiId ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   public int TargetWiId ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   public string WiType ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   public override string ToString()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return $"[***REMOVED***Change.ToString()***REMOVED***] ***REMOVED***SourceOriginId***REMOVED***/***REMOVED***SourceWiId***REMOVED***->***REMOVED***TargetOriginId***REMOVED***/***REMOVED***TargetWiId***REMOVED*** [***REMOVED***WiType***REMOVED***]";
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***