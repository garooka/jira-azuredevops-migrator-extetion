***REMOVED***
***REMOVED***
***REMOVED***
using System.Text;
using System.Threading.Tasks;
using Migration.WIContract;

namespace Migration.Common
***REMOVED***
    public class AbortMigrationException : Exception 
    ***REMOVED***
***REMOVED***   public AbortMigrationException(string reason)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  Reason = reason;
***REMOVED***   ***REMOVED***

***REMOVED***   public string Reason ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public WiRevision Revision ***REMOVED*** get; set; ***REMOVED***
***REMOVED***
***REMOVED***
