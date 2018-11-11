***REMOVED***
***REMOVED***
***REMOVED***
***REMOVED***
***REMOVED***
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Migration.Common.Config
***REMOVED***
    public class ConfigReaderJson : IConfigReader
    ***REMOVED***
***REMOVED***   private readonly string FilePath;
***REMOVED***   private string JsonText;

***REMOVED***   public ConfigReaderJson(string filePath)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  FilePath = filePath;
***REMOVED***   ***REMOVED***

***REMOVED***   public ConfigJson Deserialize()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  LoadFromFile(FilePath);
***REMOVED******REMOVED***  return DeserializeText(JsonText);
***REMOVED***   ***REMOVED***

***REMOVED***   public void LoadFromFile(string filePath)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** JsonText = GetJsonFromFile(filePath);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (FileNotFoundException ex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, "Required JSON configuration file was not found. Please ensure that this file is in the correct location.");
***REMOVED******REMOVED******REMOVED*** throw ex;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (PathTooLongException ex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, "Required JSON configuration file could not be accessed because the file path is too long. Please store your files for this application in a folder location with a shorter path name.");
***REMOVED******REMOVED******REMOVED*** throw ex;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (UnauthorizedAccessException ex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, "Cannot read from the JSON configuration file because you are not authorized to access it. Please try running this application as administrator or moving it to a folder location that does not require special access.");
***REMOVED******REMOVED******REMOVED*** throw ex;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (Exception ex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, "Cannot read from the JSON configuration file. Please ensure it is formatted properly.");
***REMOVED******REMOVED******REMOVED*** throw ex;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   public string GetJsonFromFile(string filePath)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return File.ReadAllText(filePath);
***REMOVED***   ***REMOVED***

***REMOVED***   public ConfigJson DeserializeText(string input)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  ConfigJson result = null;
***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** result = JsonConvert.DeserializeObject<ConfigJson>(input);
***REMOVED******REMOVED******REMOVED*** var obj = JObject.Parse(input);
***REMOVED******REMOVED******REMOVED*** var fields = obj.SelectToken("field-map.field").Select(jt => jt.ToObject<Field>()).ToList();
***REMOVED******REMOVED******REMOVED*** if (result.FieldMap.Fields == null)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***result.FieldMap.Fields = new List<Field>();
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** result.FieldMap.Fields.AddRange(fields);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (Exception ex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, "Cannot deserialize the JSON text from configuration file. Please ensure it is formatted properly.");
***REMOVED******REMOVED******REMOVED*** throw ex;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  return result;
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***