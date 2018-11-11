using Migration.Common;
***REMOVED***
***REMOVED***
***REMOVED***

namespace WorkItemImport
***REMOVED***
    public class ExecutionPlanBuilder
    ***REMOVED***
***REMOVED***   private readonly MigrationContext _context;

***REMOVED***   public ExecutionPlanBuilder(MigrationContext context)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  _context = context;
***REMOVED***   ***REMOVED***
***REMOVED***   
***REMOVED***   public ExecutionPlan BuildExecutionPlan()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var path = _context.MigrationWorkspace;

***REMOVED******REMOVED***  // get the file attributes for file or directory
***REMOVED******REMOVED***  FileAttributes attr = File.GetAttributes(path);
***REMOVED******REMOVED***  if (attr.HasFlag(FileAttributes.Directory))
***REMOVED******REMOVED******REMOVED*** return new ExecutionPlan(BuildExecutionPlanFromDir(), _context);
***REMOVED******REMOVED***  else
***REMOVED******REMOVED******REMOVED*** return new ExecutionPlan(BuildExecutionPlanFromFile(path), _context);
***REMOVED***   ***REMOVED***

***REMOVED***   private IEnumerable<RevisionReference> BuildExecutionPlanFromDir()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var actionPlan = new List<RevisionReference>();
***REMOVED******REMOVED***  foreach (var wi in _context.EnumerateAllItems())
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Debug, $"Processing ***REMOVED***wi.OriginId***REMOVED***");
***REMOVED******REMOVED******REMOVED*** foreach (var rev in wi.Revisions)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***var revRef = new RevisionReference() ***REMOVED*** OriginId = wi.OriginId, RevIndex = rev.Index, Time = rev.Time ***REMOVED***;
***REMOVED******REMOVED******REMOVED******REMOVED***actionPlan.Add(revRef);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  actionPlan.Sort();

***REMOVED******REMOVED***  EnsureIncreasingTimes(actionPlan);

***REMOVED******REMOVED***  return actionPlan;
***REMOVED***   ***REMOVED***

***REMOVED***   private void EnsureIncreasingTimes(List<RevisionReference> actionPlan)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  for (int i = 1; i < actionPlan.Count; i++)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var prev = actionPlan[i - 1];
***REMOVED******REMOVED******REMOVED*** var current = actionPlan[i];

***REMOVED******REMOVED******REMOVED*** DateTime? nextTime = null;
***REMOVED******REMOVED******REMOVED*** if (i + 1 < actionPlan.Count)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***var next = actionPlan[i + 1];
***REMOVED******REMOVED******REMOVED******REMOVED***if (next.Time > prev.Time)
***REMOVED******REMOVED******REMOVED******REMOVED***    nextTime = next.Time;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** if (prev.Time >= current.Time)
***REMOVED******REMOVED******REMOVED******REMOVED***current.Time = RevisionUtility.NextValidDeltaRev(prev.Time, nextTime);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private IEnumerable<RevisionReference> BuildExecutionPlanFromFile(string path)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  throw new NotImplementedException();
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***
