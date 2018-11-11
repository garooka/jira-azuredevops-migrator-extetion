using Newtonsoft.Json;

namespace Migration.Common.Config
***REMOVED***
    public class Field
    ***REMOVED***
***REMOVED***   [JsonProperty("target", Required = Required.Always)]
***REMOVED***   public string Target ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty("source", Required = Required.Always)]
***REMOVED***   public string Source ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty("for")]
***REMOVED***   public string For ***REMOVED*** get; set; ***REMOVED*** = "All";

***REMOVED***   [JsonProperty("not-for")]
***REMOVED***   public string NotFor ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty("type")]
***REMOVED***   public string Type ***REMOVED*** get; set; ***REMOVED*** = "string";
***REMOVED***   
***REMOVED***   [JsonProperty("mapper")]
***REMOVED***   public string Mapper ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty("process")]
***REMOVED***   public string Process ***REMOVED*** get; set; ***REMOVED*** = "Common";
***REMOVED***
***REMOVED***