using Newtonsoft.Json.Linq;
***REMOVED***

namespace Migration.Common
***REMOVED***
    public static class JsonExtensions
    ***REMOVED***
***REMOVED***   public static T ExValue<T>(this JToken token, string path)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (!token.HasValues)
***REMOVED******REMOVED******REMOVED*** return default(T);

***REMOVED******REMOVED***  var value = token.SelectToken(path, false);

***REMOVED******REMOVED***  if (value == null)
***REMOVED******REMOVED******REMOVED*** return default(T);

***REMOVED******REMOVED***  return value.Value<T>();
***REMOVED***   ***REMOVED***

***REMOVED***   public static IEnumerable<T> GetValues<T>(this JToken token, string path)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var value = token.SelectToken(path, false);
***REMOVED******REMOVED***  return value.Values<T>();
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***