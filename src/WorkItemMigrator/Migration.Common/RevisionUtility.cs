***REMOVED***

namespace Migration.Common
***REMOVED***
    public static class RevisionUtility
    ***REMOVED***
***REMOVED***   private static TimeSpan _deltaTime = TimeSpan.FromMilliseconds(50);

***REMOVED***   public static DateTime NextValidDeltaRev(DateTime current, DateTime? next = null)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (next == null || current + _deltaTime < next)
***REMOVED******REMOVED******REMOVED*** return current + _deltaTime;

***REMOVED******REMOVED***  TimeSpan diff = next.Value - current;
***REMOVED******REMOVED***  var middle = new TimeSpan(diff.Ticks / 2);
***REMOVED******REMOVED***  return current + middle;
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***