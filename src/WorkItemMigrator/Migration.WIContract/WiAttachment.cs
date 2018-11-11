***REMOVED***

namespace Migration.WIContract
***REMOVED***
    public class WiAttachment
    ***REMOVED***
***REMOVED***   public ReferenceChangeType Change ***REMOVED*** get; set; ***REMOVED***
***REMOVED***   public string FilePath ***REMOVED*** get; set; ***REMOVED***
***REMOVED***   public string Comment ***REMOVED*** get; set; ***REMOVED***
***REMOVED***   public string AttOriginId ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   public override string ToString()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return $"[***REMOVED***Change.ToString()***REMOVED***] ***REMOVED***AttOriginId***REMOVED***/***REMOVED***Path.GetFileName(FilePath)***REMOVED***";
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***