***REMOVED***
using Migration.Common;
using Migration.WIContract;

namespace WorkItemImport
***REMOVED***
    public class ExecutionPlan
    ***REMOVED***
***REMOVED***   public class ExecutionItem
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  public string OriginId ***REMOVED*** get; set; ***REMOVED***
***REMOVED******REMOVED***  public int WiId ***REMOVED*** get; set; ***REMOVED*** = -1;
***REMOVED******REMOVED***  public WiRevision Revision ***REMOVED*** get; set; ***REMOVED***
***REMOVED******REMOVED***  public string WiType ***REMOVED*** get; internal set; ***REMOVED***

***REMOVED******REMOVED***  public override string ToString()
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** return $"***REMOVED***OriginId***REMOVED***/***REMOVED***WiId***REMOVED***, ***REMOVED***Revision.Index***REMOVED***";
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private readonly MigrationContext _context;

***REMOVED***   public Queue<RevisionReference> ReferenceQueue ***REMOVED*** get; private set; ***REMOVED***

***REMOVED***   public ExecutionPlan(IEnumerable<RevisionReference> orderedRevisionReferences, MigrationContext context)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  ReferenceQueue = new Queue<RevisionReference>(orderedRevisionReferences);
***REMOVED******REMOVED***  this._context = context;
***REMOVED***   ***REMOVED***

***REMOVED***   private ExecutionItem TransformToExecutionItem(RevisionReference revRef)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var item = _context.GetItem(revRef.OriginId);
***REMOVED******REMOVED***  var rev = item.Revisions[revRef.RevIndex];
***REMOVED******REMOVED***  rev.Time = revRef.Time;
***REMOVED******REMOVED***  return new ExecutionItem() ***REMOVED*** OriginId = item.OriginId, WiId = item.WiId, WiType = item.Type, Revision = rev ***REMOVED***;
***REMOVED***   ***REMOVED***

***REMOVED***   public bool TryPop(out ExecutionItem nextItem)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  nextItem = null;
***REMOVED******REMOVED***  if (ReferenceQueue.Count > 0)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** nextItem = TransformToExecutionItem(ReferenceQueue.Dequeue());
***REMOVED******REMOVED******REMOVED*** return true;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  else
***REMOVED******REMOVED******REMOVED*** return false;
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***
