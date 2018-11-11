using Migration.Common;
using Newtonsoft.Json.Linq;
***REMOVED***
***REMOVED***
***REMOVED***
using System.Text;
using System.Threading.Tasks;

***REMOVED***
***REMOVED***
    public class JiraChangeItem
    ***REMOVED***
***REMOVED***   public JiraChangeItem(JObject item)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  Field = item.ExValue<string>("$.field");
***REMOVED******REMOVED***  FieldType = item.ExValue<string>("$.fieldtype");
***REMOVED******REMOVED***  FieldId = item.ExValue<string>("$.fieldId");

***REMOVED******REMOVED***  From = item.ExValue<string>("$.from");
***REMOVED******REMOVED***  FromString = item.ExValue<string>("$.fromString");

***REMOVED******REMOVED***  To = item.ExValue<string>("$.to");
***REMOVED******REMOVED***  ToString = item.ExValue<string>("$.toString");
***REMOVED***   ***REMOVED***

***REMOVED***   public string Field ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public string FieldType ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public string FieldId ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public string From ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public string FromString ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public string To ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public new string ToString ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***
***REMOVED***
