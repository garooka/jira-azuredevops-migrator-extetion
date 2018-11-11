***REMOVED***

***REMOVED***
***REMOVED***
    public class JiraAttachment : IEquatable<JiraAttachment>
    ***REMOVED***
***REMOVED***   public string Id ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string Filename ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string Url ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string ThumbUrl ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string LocalPath ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string LocalThumbPath ***REMOVED*** get; internal set; ***REMOVED***

***REMOVED***   public bool Equals(JiraAttachment other)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return Id == other.Id;
***REMOVED***   ***REMOVED***

***REMOVED***   public override string ToString()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return $"***REMOVED***Id***REMOVED***/***REMOVED***Filename***REMOVED***";
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***