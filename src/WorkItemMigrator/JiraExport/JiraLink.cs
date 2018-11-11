***REMOVED***

***REMOVED***
***REMOVED***
    public class JiraLink : IEquatable<JiraLink>
    ***REMOVED***
***REMOVED***   public string SourceItem ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string TargetItem ***REMOVED*** get; internal set; ***REMOVED***
***REMOVED***   public string LinkType ***REMOVED*** get; internal set; ***REMOVED***

***REMOVED***   public bool Equals(JiraLink other)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return SourceItem.Equals(other.SourceItem, StringComparison.InvariantCultureIgnoreCase)
***REMOVED******REMOVED******REMOVED*** && TargetItem.Equals(other.TargetItem, StringComparison.InvariantCultureIgnoreCase)
***REMOVED******REMOVED******REMOVED*** && LinkType.Equals(other.LinkType, StringComparison.InvariantCultureIgnoreCase);
***REMOVED***   ***REMOVED***

***REMOVED***   public override string ToString()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return $"[***REMOVED***LinkType***REMOVED***] ***REMOVED***SourceItem***REMOVED***->***REMOVED***TargetItem***REMOVED***";
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***