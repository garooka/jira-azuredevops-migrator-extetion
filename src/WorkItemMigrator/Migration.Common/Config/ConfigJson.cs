***REMOVED***
using Newtonsoft.Json;

namespace Common.Config
***REMOVED***
    public class ConfigJson
    ***REMOVED***
***REMOVED***   [JsonProperty(PropertyName = "source-project", Required = Required.Always)]
***REMOVED***   public string SourceProject ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName = "target-project", Required = Required.Always)]
***REMOVED***   public string TargetProject ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName = "query", Required = Required.Always)]
***REMOVED***   public string Query ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName ="workspace", Required = Required.Always)]
***REMOVED***   public string Workspace ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName = "epic-link-field", Required = Required.AllowNull)]
***REMOVED***   public string EpicLinkField ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName = "sprint-field", Required = Required.AllowNull)]
***REMOVED***   public string SprintField ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName = "batch-size", Required = Required.Always)]
***REMOVED***   public int BatchSize ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName = "log-level", Required = Required.Always)]
***REMOVED***   public string LogLevel ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName = "attachment-folder", Required = Required.Always)]
***REMOVED***   public string AttachmentsFolder ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName = "user-mapping-file", Required = Required.AllowNull)]
***REMOVED***   public string UserMappingFile ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName = "base-area-path", Required = Required.AllowNull)]
***REMOVED***   public string BaseAreaPath ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName = "base-iteration-path", Required = Required.AllowNull)]
***REMOVED***   public string BaseIterationPath ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName = "ignore-failed-links", Required = Required.Always)]
***REMOVED***   public bool IgnoreFailedLinks ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName = "field-map", Required = Required.Always)]
***REMOVED***   public ConfigMap FieldMap ***REMOVED*** get; set; ***REMOVED***

***REMOVED***   [JsonProperty(PropertyName = "process-template", Required = Required.Always)]
***REMOVED***   public string ProcessTemplate ***REMOVED*** get; set; ***REMOVED***
***REMOVED***
***REMOVED***