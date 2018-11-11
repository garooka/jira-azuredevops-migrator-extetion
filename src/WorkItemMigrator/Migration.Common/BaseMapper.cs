***REMOVED***
***REMOVED***
***REMOVED***
***REMOVED***
using System.Reflection;

namespace Migration.Common
***REMOVED***
    public class BaseMapper<TRevision> where TRevision : ISourceRevision
    ***REMOVED***
***REMOVED***   public BaseMapper(string userMappingPath)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  ParseUserMappings(userMappingPath);
***REMOVED***   ***REMOVED***

***REMOVED***   protected Dictionary<string, string> UserMapping ***REMOVED*** get; private set; ***REMOVED*** = new Dictionary<string, string>();
***REMOVED***   protected void ParseUserMappings(string userMappingPath)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (!File.Exists(userMappingPath))
***REMOVED******REMOVED******REMOVED*** return;

***REMOVED******REMOVED***  string[] userMappings = File.ReadAllLines(userMappingPath);
***REMOVED******REMOVED***  foreach (var userMapping in userMappings.Select(um => um.Split('=')))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** string jiraUser = userMapping[0].Trim();
***REMOVED******REMOVED******REMOVED*** string wiUser = userMapping[1].Trim();

***REMOVED******REMOVED******REMOVED*** UserMapping.Add(jiraUser, wiUser);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   protected virtual string MapUser(string sourceUser)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (sourceUser == null)
***REMOVED******REMOVED******REMOVED*** return sourceUser;

***REMOVED******REMOVED***  if (UserMapping.TryGetValue(sourceUser, out string wiUser))
***REMOVED******REMOVED******REMOVED*** return wiUser;
***REMOVED******REMOVED***  else
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Debug, $"Could not find user ***REMOVED***sourceUser***REMOVED*** identity. Using original identity.");
***REMOVED******REMOVED******REMOVED*** UserMapping.Add(sourceUser, sourceUser);
***REMOVED******REMOVED******REMOVED*** return sourceUser;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   protected string ReadEmbeddedFile(string resourceName)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var assembly = Assembly.GetEntryAssembly();

***REMOVED******REMOVED***  using (Stream stream = assembly.GetManifestResourceStream(resourceName))
***REMOVED******REMOVED***  using (StreamReader reader = new StreamReader(stream))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** return reader.ReadToEnd();
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   protected FieldMapping<TRevision> MergeMapping(params FieldMapping<TRevision>[] mappings)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var merged = new FieldMapping<TRevision>();
***REMOVED******REMOVED***  foreach (var mapping in mappings)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** foreach (var m in mapping)
***REMOVED******REMOVED******REMOVED******REMOVED***if (!merged.ContainsKey(m.Key))
***REMOVED******REMOVED******REMOVED******REMOVED***    merged[m.Key] = m.Value;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  return merged;
***REMOVED***   ***REMOVED***

***REMOVED***   protected string Crop(string value, int maxSize)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var max = Math.Min(value.Length, maxSize);
***REMOVED******REMOVED***  return value.Substring(0, max);
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***
