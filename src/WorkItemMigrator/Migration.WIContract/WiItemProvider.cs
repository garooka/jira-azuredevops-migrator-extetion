using Newtonsoft.Json;
***REMOVED***
***REMOVED***

namespace Migration.WIContract
***REMOVED***
    public class WiItemProvider
    ***REMOVED***
***REMOVED***   private readonly string _itemsDir;

***REMOVED***   public WiItemProvider(string itemsDir)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  _itemsDir = itemsDir;
***REMOVED***   ***REMOVED***

***REMOVED***   public WiItem Load(string originId)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var path = Path.Combine(_itemsDir, $"***REMOVED***originId***REMOVED***.json");
***REMOVED******REMOVED***  return LoadFile(path);
***REMOVED***   ***REMOVED***

***REMOVED***   private WiItem LoadFile(string path)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var serialized = File.ReadAllText(path);
***REMOVED******REMOVED***  var deserialized = JsonConvert.DeserializeObject<WiItem>(serialized, new JsonSerializerSettings() ***REMOVED*** NullValueHandling = NullValueHandling.Ignore***REMOVED***);

***REMOVED******REMOVED***  foreach (var rev in deserialized.Revisions)
***REMOVED******REMOVED******REMOVED*** rev.ParentOriginId = deserialized.OriginId;

***REMOVED******REMOVED***  return deserialized;
***REMOVED***   ***REMOVED***

***REMOVED***   public void Save(WiItem item)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  string path = Path.Combine(_itemsDir, $"***REMOVED***item.OriginId***REMOVED***.json");
***REMOVED******REMOVED***  var serialized = JsonConvert.SerializeObject(item);
***REMOVED******REMOVED***  File.WriteAllText(path, serialized);
***REMOVED***   ***REMOVED***

***REMOVED***   public IEnumerable<WiItem> EnumerateAllItems()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  foreach (var filePath in Directory.EnumerateFiles(_itemsDir, "*.json"))
***REMOVED******REMOVED******REMOVED*** yield return LoadFile(filePath);
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***