***REMOVED***

namespace WorkItemImport
***REMOVED***
    public class RevisionReference : IComparable<RevisionReference>, IEquatable<RevisionReference>
    ***REMOVED***
***REMOVED***   public string OriginId ***REMOVED*** get; set; ***REMOVED***
***REMOVED***   public int RevIndex ***REMOVED*** get; set; ***REMOVED***
***REMOVED***   public DateTime Time ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   public int CompareTo(RevisionReference other)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  int result = this.Time.CompareTo(other.Time);
***REMOVED******REMOVED***  if (result != 0) return result;

***REMOVED******REMOVED***  result = this.OriginId.CompareTo(other.OriginId);
***REMOVED******REMOVED***  if (result != 0) return result;

***REMOVED******REMOVED***  return this.RevIndex.CompareTo(other.RevIndex);
***REMOVED***   ***REMOVED***

***REMOVED***   public bool Equals(RevisionReference other)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return this.OriginId.Equals(other.OriginId, StringComparison.InvariantCultureIgnoreCase) && this.RevIndex == other.RevIndex;
***REMOVED***   ***REMOVED***

***REMOVED***   public override bool Equals(object obj)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var other = obj as RevisionReference;
***REMOVED******REMOVED***  if (other == null) return false;
***REMOVED******REMOVED***  return this.Equals(other);
***REMOVED***   ***REMOVED***

***REMOVED***   public override int GetHashCode()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  unchecked
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** int hash = 17;
***REMOVED******REMOVED******REMOVED*** hash = hash * 23 + OriginId.GetHashCode();
***REMOVED******REMOVED******REMOVED*** hash = hash * 23 + RevIndex.GetHashCode();
***REMOVED******REMOVED******REMOVED*** return hash;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***
