using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using WebApi = Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using WebModel = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Operations;
using VsWebApi = Microsoft.VisualStudio.Services.WebApi;
using Migration.Common;
***REMOVED***
***REMOVED***
***REMOVED***
using System.Threading.Tasks;
using System.Threading;
using Microsoft.VisualStudio.Services.Common;
using System.Net;
using Migration.WIContract;

namespace WorkItemImport
***REMOVED***
    public class Agent
    ***REMOVED***
***REMOVED***   private readonly MigrationContext _context;
***REMOVED***   public Settings Settings ***REMOVED*** get; private set; ***REMOVED***

***REMOVED***   public TfsTeamProjectCollection Collection
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  get; private set;
***REMOVED***   ***REMOVED***

***REMOVED***   private WorkItemStore _store;
***REMOVED***   public WorkItemStore Store
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  get
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (_store == null)
***REMOVED******REMOVED******REMOVED******REMOVED***_store = new WorkItemStore(Collection, WorkItemStoreFlags.BypassRules);

***REMOVED******REMOVED******REMOVED*** return _store;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   public VsWebApi.VssConnection RestConnection ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public Dictionary<string, int> IterationCache ***REMOVED*** get; private set; ***REMOVED*** = new Dictionary<string, int>();
***REMOVED***   public int RootIteration ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public Dictionary<string, int> AreaCache ***REMOVED*** get; private set; ***REMOVED*** = new Dictionary<string, int>();
***REMOVED***   public int RootArea ***REMOVED*** get; private set; ***REMOVED***

***REMOVED***   private WebApi.WorkItemTrackingHttpClient _wiClient;
***REMOVED***   public WebApi.WorkItemTrackingHttpClient WiClient
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  get
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (_wiClient == null)
***REMOVED******REMOVED******REMOVED******REMOVED***_wiClient = RestConnection.GetClient<WebApi.WorkItemTrackingHttpClient>();

***REMOVED******REMOVED******REMOVED*** return _wiClient;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private Agent(MigrationContext context, Settings settings, VsWebApi.VssConnection restConn, TfsTeamProjectCollection soapConnection)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  _context = context;
***REMOVED******REMOVED***  Settings = settings;
***REMOVED******REMOVED***  RestConnection = restConn;
***REMOVED******REMOVED***  Collection = soapConnection;
***REMOVED***   ***REMOVED***

***REMOVED***   #region Static
***REMOVED***   internal static Agent Initialize(MigrationContext context, Settings settings)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var restConnection = EstablishRestConnection(settings);
***REMOVED******REMOVED***  if (restConnection == null)
***REMOVED******REMOVED******REMOVED*** return null;

***REMOVED******REMOVED***  var soapConnection = EstablishSoapConnection(settings);
***REMOVED******REMOVED***  if (soapConnection == null)
***REMOVED******REMOVED******REMOVED*** return null;

***REMOVED******REMOVED***  var agent = new Agent(context, settings, restConnection, soapConnection);

***REMOVED******REMOVED***  // check if projects exists, if not create it
***REMOVED******REMOVED***  var project = agent.GetOrCreateProjectAsync().Result;
***REMOVED******REMOVED***  if (project == null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, "Could not establish connection to the remote Azure DevOps/TFS project.");
***REMOVED******REMOVED******REMOVED*** return null;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  (var iterationCache, int rootIteration) = agent.CreateClasificationCacheAsync(settings.Project, WebModel.TreeStructureGroup.Iterations).Result;
***REMOVED******REMOVED***  if (iterationCache == null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, "Could not build iteration cache.");
***REMOVED******REMOVED******REMOVED*** return null;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  agent.IterationCache = iterationCache;
***REMOVED******REMOVED***  agent.RootIteration = rootIteration;

***REMOVED******REMOVED***  (var areaCache, int rootArea) = agent.CreateClasificationCacheAsync(settings.Project, WebModel.TreeStructureGroup.Areas).Result;
***REMOVED******REMOVED***  if (areaCache == null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, "Could not build area cache.");
***REMOVED******REMOVED******REMOVED*** return null;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  agent.AreaCache = areaCache;
***REMOVED******REMOVED***  agent.RootArea = rootArea;

***REMOVED******REMOVED***  return agent;
***REMOVED***   ***REMOVED***

***REMOVED***   internal WorkItem CreateWI(string type)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var project = Store.Projects[Settings.Project];
***REMOVED******REMOVED***  var wiType = project.WorkItemTypes[type];
***REMOVED******REMOVED***  return wiType.NewWorkItem();
***REMOVED***   ***REMOVED***

***REMOVED***   internal WorkItem GetWorkItem(int wiId)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return Store.GetWorkItem(wiId);
***REMOVED***   ***REMOVED***

***REMOVED***   private static VsWebApi.VssConnection EstablishRestConnection(Settings settings)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Info, "Connecting to Azure DevOps/TFS...");
***REMOVED******REMOVED******REMOVED*** var credentials = new VssBasicCredential("", settings.Pat);
***REMOVED******REMOVED******REMOVED*** var uri = new Uri(settings.Account);
***REMOVED******REMOVED******REMOVED*** return new VsWebApi.VssConnection(uri, credentials);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (Exception ex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, $"Cannot establish connection to Azure DevOps/TFS. Operation failed with error: ***REMOVED***ex.Message***REMOVED***");
***REMOVED******REMOVED******REMOVED*** return null;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private static TfsTeamProjectCollection EstablishSoapConnection(Settings settings)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  NetworkCredential netCred = new NetworkCredential(string.Empty, settings.Pat);
***REMOVED******REMOVED***  VssBasicCredential basicCred = new VssBasicCredential(netCred);
***REMOVED******REMOVED***  VssCredentials tfsCred = new VssCredentials(basicCred);
***REMOVED******REMOVED***  var collection = new TfsTeamProjectCollection(new Uri(settings.Account), tfsCred);
***REMOVED******REMOVED***  collection.Authenticate();
***REMOVED******REMOVED***  return collection;
***REMOVED***   ***REMOVED***

***REMOVED***   #endregion

***REMOVED***   #region Setup

***REMOVED***   internal async Task<TeamProject> GetOrCreateProjectAsync()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  ProjectHttpClient projectClient = RestConnection.GetClient<ProjectHttpClient>();
***REMOVED******REMOVED***  Logger.Log(LogLevel.Info, "Retreiving project info from Azure DevOps/TFS...");
***REMOVED******REMOVED***  TeamProject project = null;
***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** project = await projectClient.GetProject(Settings.Project);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch ***REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  if (project == null)
***REMOVED******REMOVED******REMOVED*** project = await CreateProject(Settings.Project, $"***REMOVED***Settings.ProcessTemplate***REMOVED*** project for Jira migration", Settings.ProcessTemplate);

***REMOVED******REMOVED***  return project;
***REMOVED***   ***REMOVED***

***REMOVED***   internal async Task<TeamProject> CreateProject(string projectName, string projectDescription = "", string processName = "Scrum")
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  Logger.Log(LogLevel.Warning, $"Project ***REMOVED***projectName***REMOVED*** does not exist.");
***REMOVED******REMOVED***  Console.WriteLine("Would you like to create one? (Y/N)");
***REMOVED******REMOVED***  var answer = Console.ReadKey();
***REMOVED******REMOVED***  if (answer.KeyChar != 'Y' && answer.KeyChar != 'y')
***REMOVED******REMOVED******REMOVED*** return null;

***REMOVED******REMOVED***  Logger.Log(LogLevel.Info, $"Creating project ***REMOVED***projectName***REMOVED***.");

***REMOVED******REMOVED***  // Setup version control properties
***REMOVED******REMOVED***  Dictionary<string, string> versionControlProperties = new Dictionary<string, string>
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** [TeamProjectCapabilitiesConstants.VersionControlCapabilityAttributeName] = SourceControlTypes.Git.ToString()
***REMOVED******REMOVED***  ***REMOVED***;

***REMOVED******REMOVED***  // Setup process properties***REMOVED***  
***REMOVED******REMOVED***  ProcessHttpClient processClient = RestConnection.GetClient<ProcessHttpClient>();
***REMOVED******REMOVED***  Guid processId = processClient.GetProcessesAsync().Result.Find(process => ***REMOVED*** return process.Name.Equals(processName, StringComparison.InvariantCultureIgnoreCase); ***REMOVED***).Id;

***REMOVED******REMOVED***  Dictionary<string, string> processProperaties = new Dictionary<string, string>
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** [TeamProjectCapabilitiesConstants.ProcessTemplateCapabilityTemplateTypeIdAttributeName] = processId.ToString()
***REMOVED******REMOVED***  ***REMOVED***;

***REMOVED******REMOVED***  // Construct capabilities dictionary
***REMOVED******REMOVED***  Dictionary<string, Dictionary<string, string>> capabilities = new Dictionary<string, Dictionary<string, string>>
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** [TeamProjectCapabilitiesConstants.VersionControlCapabilityName] = versionControlProperties,
***REMOVED******REMOVED******REMOVED*** [TeamProjectCapabilitiesConstants.ProcessTemplateCapabilityName] = processProperaties
***REMOVED******REMOVED***  ***REMOVED***;

***REMOVED******REMOVED***  // Construct object containing properties needed for creating the project
***REMOVED******REMOVED***  TeamProject projectCreateParameters = new TeamProject()
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Name = projectName,
***REMOVED******REMOVED******REMOVED*** Description = projectDescription,
***REMOVED******REMOVED******REMOVED*** Capabilities = capabilities
***REMOVED******REMOVED***  ***REMOVED***;

***REMOVED******REMOVED***  // Get a client
***REMOVED******REMOVED***  ProjectHttpClient projectClient = RestConnection.GetClient<ProjectHttpClient>();

***REMOVED******REMOVED***  TeamProject project = null;
***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Info, "Queuing project creation...");

***REMOVED******REMOVED******REMOVED*** // Queue the project creation operation 
***REMOVED******REMOVED******REMOVED*** // This returns an operation object that can be used to check the status of the creation
***REMOVED******REMOVED******REMOVED*** OperationReference operation = await projectClient.QueueCreateProject(projectCreateParameters);

***REMOVED******REMOVED******REMOVED*** // Check the operation status every 5 seconds (for up to 30 seconds)
***REMOVED******REMOVED******REMOVED*** Operation completedOperation = WaitForLongRunningOperation(operation.Id, 5, 30).Result;

***REMOVED******REMOVED******REMOVED*** // Check if the operation succeeded (the project was created) or failed
***REMOVED******REMOVED******REMOVED*** if (completedOperation.Status == OperationStatus.Succeeded)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***// Get the full details about the newly created project
***REMOVED******REMOVED******REMOVED******REMOVED***project = projectClient.GetProject(
***REMOVED******REMOVED******REMOVED******REMOVED***    projectCreateParameters.Name,
***REMOVED******REMOVED******REMOVED******REMOVED***    includeCapabilities: true,
***REMOVED******REMOVED******REMOVED******REMOVED***    includeHistory: true).Result;

***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Info, $"Project created (ID: ***REMOVED***project.Id***REMOVED***)");
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** else
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Error, "Project creation operation failed: " + completedOperation.ResultMessage);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (Exception ex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, "Exception during create project: " + ex.Message);
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return project;
***REMOVED***   ***REMOVED***

***REMOVED***   private async Task<Operation> WaitForLongRunningOperation(Guid operationId, int interavalInSec = 5, int maxTimeInSeconds = 60, CancellationToken cancellationToken = default(CancellationToken))
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  OperationsHttpClient operationsClient = RestConnection.GetClient<OperationsHttpClient>();
***REMOVED******REMOVED***  DateTime expiration = DateTime.Now.AddSeconds(maxTimeInSeconds);
***REMOVED******REMOVED***  int checkCount = 0;

***REMOVED******REMOVED***  while (true)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Console.WriteLine(" Checking status (***REMOVED***0***REMOVED***)... ", (checkCount++));

***REMOVED******REMOVED******REMOVED*** Operation operation = await operationsClient.GetOperation(operationId, cancellationToken);

***REMOVED******REMOVED******REMOVED*** if (!operation.Completed)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Console.WriteLine("   Pausing ***REMOVED***0***REMOVED*** seconds", interavalInSec);

***REMOVED******REMOVED******REMOVED******REMOVED***await Task.Delay(interavalInSec * 1000);

***REMOVED******REMOVED******REMOVED******REMOVED***if (DateTime.Now > expiration)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    throw new Exception(String.Format("Operation did not complete in ***REMOVED***0***REMOVED*** seconds.", maxTimeInSeconds));
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** else
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***return operation;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private async Task<(Dictionary<string, int>, int)> CreateClasificationCacheAsync(string project, WebModel.TreeStructureGroup structureGroup)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Info, $"Building ***REMOVED***(structureGroup == WebModel.TreeStructureGroup.Iterations ? "iteration" : "area")***REMOVED*** cache...");
***REMOVED******REMOVED******REMOVED*** WebModel.WorkItemClassificationNode all = await WiClient.GetClassificationNodeAsync(project, structureGroup, null, 1000);

***REMOVED******REMOVED******REMOVED*** var clasificationCache = new Dictionary<string, int>();

***REMOVED******REMOVED******REMOVED*** if (all.Children != null && all.Children.Any())
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***foreach (var iteration in all.Children)
***REMOVED******REMOVED******REMOVED******REMOVED***    CreateClasificationCacheRec(iteration, clasificationCache, "");
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** return (clasificationCache, all.Id);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (Exception ex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, $"Error while building ***REMOVED***(structureGroup == WebModel.TreeStructureGroup.Iterations ? "iteration" : "area")***REMOVED*** cache: ***REMOVED***ex.Message***REMOVED***");
***REMOVED******REMOVED******REMOVED*** return (null, -1);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private void CreateClasificationCacheRec(WebModel.WorkItemClassificationNode current, Dictionary<string, int> agg, string parentPath)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  string fullName = !string.IsNullOrWhiteSpace(parentPath) ? parentPath + "/" + current.Name : current.Name;

***REMOVED******REMOVED***  agg.Add(fullName, current.Id);
***REMOVED******REMOVED***  Logger.Log(LogLevel.Debug, $"***REMOVED***(current.StructureType == WebModel.TreeNodeStructureType.Iteration ? "Iteration" : "Area")***REMOVED*** ***REMOVED***fullName***REMOVED*** added to cache");
***REMOVED******REMOVED***  if (current.Children != null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** foreach (var node in current.Children)
***REMOVED******REMOVED******REMOVED******REMOVED***CreateClasificationCacheRec(node, agg, fullName);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   public int? EnsureClasification(string fullName, WebModel.TreeStructureGroup structureGroup = WebModel.TreeStructureGroup.Iterations)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (string.IsNullOrWhiteSpace(fullName))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, "Invalid value provided for node name/path");
***REMOVED******REMOVED******REMOVED*** throw new ArgumentException("fullName");
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  var path = fullName.Split('/');
***REMOVED******REMOVED***  var name = path.Last();
***REMOVED******REMOVED***  var parent = string.Join("/", path.Take(path.Length - 1));

***REMOVED******REMOVED***  if (!string.IsNullOrEmpty(parent))
***REMOVED******REMOVED******REMOVED*** EnsureClasification(parent, structureGroup);

***REMOVED******REMOVED***  var cache = structureGroup == WebModel.TreeStructureGroup.Iterations ? IterationCache : AreaCache;

***REMOVED******REMOVED***  lock (cache)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (cache.TryGetValue(fullName, out int id))
***REMOVED******REMOVED******REMOVED******REMOVED***return id;

***REMOVED******REMOVED******REMOVED*** WebModel.WorkItemClassificationNode node = null;

***REMOVED******REMOVED******REMOVED*** try
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***node = WiClient.CreateOrUpdateClassificationNodeAsync(
***REMOVED******REMOVED******REMOVED******REMOVED***    new WebModel.WorkItemClassificationNode() ***REMOVED*** Name = name, ***REMOVED***, Settings.Project, structureGroup, parent).Result;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** catch (Exception ex)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Error, $"Error while adding ***REMOVED***(structureGroup == WebModel.TreeStructureGroup.Iterations ? "iteration" : "area")***REMOVED*** ***REMOVED***fullName***REMOVED*** to Azure DevOps/TFS: ***REMOVED***ex.Message***REMOVED***");
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** if (node != null)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Info, $"***REMOVED***(structureGroup == WebModel.TreeStructureGroup.Iterations ? "Iteration" : "Area")***REMOVED*** ***REMOVED***fullName***REMOVED*** added to Azure DevOps/TFS");
***REMOVED******REMOVED******REMOVED******REMOVED***cache.Add(fullName, node.Id);
***REMOVED******REMOVED******REMOVED******REMOVED***Store.RefreshCache();
***REMOVED******REMOVED******REMOVED******REMOVED***return node.Id;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  return null;
***REMOVED***   ***REMOVED***

***REMOVED***   #endregion

***REMOVED***   #region Import Revision

***REMOVED***   private bool UpdateWIFields(List<WiField> fields, WorkItem wi)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  bool success = true;

***REMOVED******REMOVED***  if (!wi.IsOpen | !wi.IsPartialOpen)
***REMOVED******REMOVED******REMOVED*** wi.PartialOpen();

***REMOVED******REMOVED***  foreach (var fieldRev in fields)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** try
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***string fieldRef = fieldRev.ReferenceName;
***REMOVED******REMOVED******REMOVED******REMOVED***object fieldValue = fieldRev.Value;

***REMOVED******REMOVED******REMOVED******REMOVED***if (fieldRef.Equals("System.IterationPath", StringComparison.InvariantCultureIgnoreCase))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    string iterationPath = Settings.BaseIterationPath;

***REMOVED******REMOVED******REMOVED******REMOVED***    if (!string.IsNullOrWhiteSpace((string)fieldValue))
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   if (string.IsNullOrWhiteSpace(iterationPath))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  iterationPath = (string)fieldValue;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   else
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  iterationPath = string.Join("/", iterationPath, (string)fieldValue);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***

***REMOVED******REMOVED******REMOVED******REMOVED***    fieldRef = "System.IterationId";
***REMOVED******REMOVED******REMOVED******REMOVED***    if (!string.IsNullOrWhiteSpace(iterationPath))
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   int? iterationId = EnsureClasification(iterationPath, WebModel.TreeStructureGroup.Iterations);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   fieldValue = iterationId;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    else
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   fieldValue = RootIteration;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***else if (fieldRef.Equals("System.AreaPath", StringComparison.InvariantCultureIgnoreCase))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    string areaPath = Settings.BaseAreaPath;

***REMOVED******REMOVED******REMOVED******REMOVED***    if (!string.IsNullOrWhiteSpace((string)fieldValue))
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   if (string.IsNullOrWhiteSpace(areaPath))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  areaPath = (string)fieldValue;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   else
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***  areaPath = string.Join("/", areaPath, (string)fieldValue);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***

***REMOVED******REMOVED******REMOVED******REMOVED***    fieldRef = "System.AreaId";
***REMOVED******REMOVED******REMOVED******REMOVED***    if (!string.IsNullOrWhiteSpace(areaPath))
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   int? areaId = EnsureClasification(areaPath, WebModel.TreeStructureGroup.Areas);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   fieldValue = areaId;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    else
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   fieldValue = RootArea;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***

***REMOVED******REMOVED******REMOVED******REMOVED***var field = wi.Fields[fieldRef];
***REMOVED******REMOVED******REMOVED******REMOVED***field.Value = fieldValue;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** catch (Exception ex)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(ex);
***REMOVED******REMOVED******REMOVED******REMOVED***success = false;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return success;
***REMOVED***   ***REMOVED***

***REMOVED***   private bool ApplyAttachments(WiRevision rev, WorkItem wi, Dictionary<string, Attachment> attachmentMap)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  bool success = true;

***REMOVED******REMOVED***  if (!wi.IsOpen)
***REMOVED******REMOVED******REMOVED*** wi.Open();

***REMOVED******REMOVED***  foreach (var att in rev.Attachments)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** try
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Debug, $"***REMOVED***att.ToString()***REMOVED***");
***REMOVED******REMOVED******REMOVED******REMOVED***if (att.Change == ReferenceChangeType.Added)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    var newAttachment = new Attachment(att.FilePath, att.Comment);
***REMOVED******REMOVED******REMOVED******REMOVED***    wi.Attachments.Add(newAttachment);

***REMOVED******REMOVED******REMOVED******REMOVED***    attachmentMap.Add(att.AttOriginId, newAttachment);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***else if (att.Change == ReferenceChangeType.Removed)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    Attachment existingAttachment = IdentifyAttachment(att, wi);
***REMOVED******REMOVED******REMOVED******REMOVED***    if (existingAttachment != null)
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   wi.Attachments.Remove(existingAttachment);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    else
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   success = false;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   Logger.Log(LogLevel.Warning, $"***REMOVED***att.ToString()***REMOVED*** - could not find migrated attachment.");
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** catch (AbortMigrationException)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***throw;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** catch (Exception ex)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(ex);
***REMOVED******REMOVED******REMOVED******REMOVED***success = false;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  if (rev.Attachments.Any(a => a.Change == ReferenceChangeType.Removed))
***REMOVED******REMOVED******REMOVED*** wi.Fields[CoreField.History].Value = $"Removed attachments(s): ***REMOVED*** string.Join(";", rev.Attachments.Where(a => a.Change == ReferenceChangeType.Removed).Select(a => a.ToString()))***REMOVED***";

***REMOVED******REMOVED***  return success;
***REMOVED***   ***REMOVED***

***REMOVED***   private Attachment IdentifyAttachment(WiAttachment att, WorkItem wi)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (_context.Journal.IsAttachmentMigrated(att.AttOriginId, out int attWiId))
***REMOVED******REMOVED******REMOVED*** return wi.Attachments.Cast<Attachment>().SingleOrDefault(a => a.Id == attWiId);
***REMOVED******REMOVED***  return null;
***REMOVED***   ***REMOVED***

***REMOVED***   private bool ApplyLinks(WiRevision rev, WorkItem wi)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  bool success = true;

***REMOVED******REMOVED***  if (!wi.IsOpen)
***REMOVED******REMOVED******REMOVED*** wi.Open();


***REMOVED******REMOVED***  foreach (var link in rev.Links)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** try
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***int sourceWiId = _context.Journal.GetMigratedId(link.SourceOriginId);
***REMOVED******REMOVED******REMOVED******REMOVED***int targetWiId = _context.Journal.GetMigratedId(link.TargetOriginId);

***REMOVED******REMOVED******REMOVED******REMOVED***link.SourceWiId = sourceWiId;
***REMOVED******REMOVED******REMOVED******REMOVED***link.TargetWiId = targetWiId;

***REMOVED******REMOVED******REMOVED******REMOVED***if (link.TargetWiId == -1)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    var errorLevel = Settings.IgnoreFailedLinks ? LogLevel.Warning : LogLevel.Error;
***REMOVED******REMOVED******REMOVED******REMOVED***    Logger.Log(errorLevel, $"***REMOVED***link.ToString()***REMOVED*** - target work item is not yet created in Azure DevOps/TFS.");
***REMOVED******REMOVED******REMOVED******REMOVED***    success = false;
***REMOVED******REMOVED******REMOVED******REMOVED***    continue;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***

***REMOVED******REMOVED******REMOVED******REMOVED***if (link.Change == ReferenceChangeType.Added)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    if (!AddLink(link, wi))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   success = false;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***else if (link.Change == ReferenceChangeType.Removed)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    if (!RemoveLink(link, wi))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   success = false;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** catch (Exception ex)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(ex);
***REMOVED******REMOVED******REMOVED******REMOVED***success = false;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  if (rev.Links.Any(l => l.Change == ReferenceChangeType.Removed))
***REMOVED******REMOVED******REMOVED*** wi.Fields[CoreField.History].Value = $"Removed link(s): ***REMOVED*** string.Join(";", rev.Links.Where(l => l.Change == ReferenceChangeType.Removed).Select(l => l.ToString()))***REMOVED***";
***REMOVED******REMOVED***  else if (rev.Links.Any(l => l.Change == ReferenceChangeType.Added))
***REMOVED******REMOVED******REMOVED*** wi.Fields[CoreField.History].Value = $"Added link(s): ***REMOVED*** string.Join(";", rev.Links.Where(l => l.Change == ReferenceChangeType.Added).Select(l => l.ToString()))***REMOVED***";


***REMOVED******REMOVED***  return success;
***REMOVED***   ***REMOVED***

***REMOVED***   private bool AddLink(WiLink link, WorkItem wi)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var linkEnd = ParseLinkEnd(link, wi);

***REMOVED******REMOVED***  if (linkEnd != null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var relatedLink = new RelatedLink(linkEnd, link.TargetWiId);
***REMOVED******REMOVED******REMOVED*** relatedLink = ResolveCiclycalLinks(relatedLink, wi);
***REMOVED******REMOVED******REMOVED*** wi.Links.Add(relatedLink);
***REMOVED******REMOVED******REMOVED*** return true;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  else
***REMOVED******REMOVED******REMOVED*** return false;

***REMOVED***   ***REMOVED***

***REMOVED***   private RelatedLink ResolveCiclycalLinks(RelatedLink link, WorkItem wi)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (link.LinkTypeEnd.LinkType.IsNonCircular && DetectCycle(wi, link))
***REMOVED******REMOVED******REMOVED*** return new RelatedLink(link.LinkTypeEnd.OppositeEnd, link.RelatedWorkItemId);

***REMOVED******REMOVED***  return link;
***REMOVED***   ***REMOVED***

***REMOVED***   private bool DetectCycle(WorkItem startingWi, RelatedLink startingLink)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var nextWi = startingWi;
***REMOVED******REMOVED***  var nextWiLink = startingLink;

***REMOVED******REMOVED***  do
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** nextWi = Store.GetWorkItem(nextWiLink.RelatedWorkItemId);
***REMOVED******REMOVED******REMOVED*** nextWiLink = nextWi.Links.OfType<RelatedLink>().FirstOrDefault(rl => rl.LinkTypeEnd.Id == startingLink.LinkTypeEnd.Id);

***REMOVED******REMOVED******REMOVED*** if (nextWiLink != null && nextWiLink.RelatedWorkItemId == startingWi.Id)
***REMOVED******REMOVED******REMOVED******REMOVED***return true;

***REMOVED******REMOVED***  ***REMOVED*** while (nextWiLink != null);

***REMOVED******REMOVED***  return false;
***REMOVED***   ***REMOVED***

***REMOVED***   private WorkItemLinkTypeEnd ParseLinkEnd(WiLink link, WorkItem wi)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var props = link.WiType.Split('-');
***REMOVED******REMOVED***  var linkType = wi.Project.Store.WorkItemLinkTypes.SingleOrDefault(lt => lt.ReferenceName == props[0]);
***REMOVED******REMOVED***  if (linkType == null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, $"***REMOVED***link.ToString()***REMOVED*** - linkt type (***REMOVED***props[0]***REMOVED***) does not exist in project");
***REMOVED******REMOVED******REMOVED*** return null;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  WorkItemLinkTypeEnd linkEnd = null;

***REMOVED******REMOVED***  if (linkType.IsDirectional)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (props.Length > 1)
***REMOVED******REMOVED******REMOVED******REMOVED***linkEnd = props[1] == "Forward" ? linkType.ForwardEnd : linkType.ReverseEnd;
***REMOVED******REMOVED******REMOVED*** else
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Error, $"***REMOVED***link.ToString()***REMOVED*** - directional link info not provided.");
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  else
***REMOVED******REMOVED******REMOVED*** linkEnd = linkType.ForwardEnd;

***REMOVED******REMOVED***  return linkEnd;
***REMOVED***   ***REMOVED***

***REMOVED***   private bool RemoveLink(WiLink link, WorkItem wi)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var linkToRemove = wi.Links.OfType<RelatedLink>().SingleOrDefault(rl => rl.LinkTypeEnd.ImmutableName == link.WiType && rl.RelatedWorkItemId == link.TargetWiId);
***REMOVED******REMOVED***  if (linkToRemove == null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Warning, $"***REMOVED***link.ToString()***REMOVED*** - Cannot identify link to remove");
***REMOVED******REMOVED******REMOVED*** return false;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  wi.Links.Remove(linkToRemove);
***REMOVED******REMOVED***  return true;
***REMOVED***   ***REMOVED***

***REMOVED***   private void SaveWorkItem(WiRevision rev, WorkItem newWorkItem)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (!newWorkItem.IsValid())
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Warning, $"***REMOVED***rev.ToString()***REMOVED*** - Invalid revision");

***REMOVED******REMOVED******REMOVED*** var reasons = newWorkItem.Validate();
***REMOVED******REMOVED******REMOVED*** foreach (Microsoft.TeamFoundation.WorkItemTracking.Client.Field reason in reasons)
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Info, $"Field: ***REMOVED***reason.Name***REMOVED***, Status: ***REMOVED***reason.Status***REMOVED***, Value: ***REMOVED***reason.Value***REMOVED***");
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** newWorkItem.Save(SaveFlags.MergeAll);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (FileAttachmentException faex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, $"[***REMOVED***faex.GetType().ToString()***REMOVED***] ***REMOVED***faex.Message***REMOVED***. Attachment ***REMOVED***faex.SourceAttachment.Name***REMOVED***(***REMOVED***faex.SourceAttachment.Id***REMOVED***) in ***REMOVED***rev.ToString()***REMOVED*** will be skipped");
***REMOVED******REMOVED******REMOVED*** newWorkItem.Attachments.Remove(faex.SourceAttachment);
***REMOVED******REMOVED******REMOVED*** SaveWorkItem(rev, newWorkItem);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private void EnsureAuthorFields(WiRevision rev)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  string changedByRef = "System.ChangedBy";
***REMOVED******REMOVED***  string createdByRef = "System.CreatedBy";
***REMOVED******REMOVED***  if (rev.Index == 0 && !rev.Fields.Any(f => f.ReferenceName.Equals(createdByRef, StringComparison.InvariantCultureIgnoreCase)))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** rev.Fields.Add(new WiField() ***REMOVED*** ReferenceName = createdByRef, Value = rev.Author ***REMOVED***);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  if (!rev.Fields.Any(f => f.ReferenceName.Equals(changedByRef, StringComparison.InvariantCultureIgnoreCase)))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** rev.Fields.Add(new WiField() ***REMOVED*** ReferenceName = changedByRef, Value = rev.Author ***REMOVED***);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private void EnsureDateFields(WiRevision rev)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  string changedDateRef = "System.ChangedDate";
***REMOVED******REMOVED***  string createdDateRef = "System.CreatedDate";
***REMOVED******REMOVED***  if (rev.Index == 0 && !rev.Fields.Any(f => f.ReferenceName.Equals(createdDateRef, StringComparison.InvariantCultureIgnoreCase)))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** rev.Fields.Add(new WiField() ***REMOVED*** ReferenceName = createdDateRef, Value = rev.Time.ToString("o") ***REMOVED***);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  if (!rev.Fields.Any(f => f.ReferenceName.Equals(changedDateRef, StringComparison.InvariantCultureIgnoreCase)))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** rev.Fields.Add(new WiField() ***REMOVED*** ReferenceName = changedDateRef, Value = rev.Time.ToString("o") ***REMOVED***);
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED******REMOVED*** 

***REMOVED***   private bool CorrectDescription(WorkItem wi, WiItem wiItem, WiRevision rev)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  string currentDescription = wi.Type.Name == "Bug" ? wi.Fields["Microsoft.VSTS.TCM.ReproSteps"].Value.ToString() as string : wi.Description;
***REMOVED******REMOVED***  if (string.IsNullOrWhiteSpace(currentDescription))
***REMOVED******REMOVED******REMOVED*** return false;

***REMOVED******REMOVED***  bool descUpdated = false;
***REMOVED******REMOVED***  foreach (var att in wiItem.Revisions.SelectMany(r => r.Attachments.Where(a => a.Change == ReferenceChangeType.Added)))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (currentDescription.Contains(att.FilePath))
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***var tfsAtt = IdentifyAttachment(att, wi);
***REMOVED******REMOVED******REMOVED******REMOVED***descUpdated = true;

***REMOVED******REMOVED******REMOVED******REMOVED***if (tfsAtt != null)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    currentDescription.Replace(att.FilePath, tfsAtt.Uri.AbsoluteUri);
***REMOVED******REMOVED******REMOVED******REMOVED***    descUpdated = true;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***else
***REMOVED******REMOVED******REMOVED******REMOVED***    Logger.Log(LogLevel.Warning, $"Attachment ***REMOVED***att.ToString()***REMOVED*** referenced in description but is missing from work item ***REMOVED***wiItem.OriginId***REMOVED***/***REMOVED***wi.Id***REMOVED***");

***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  if (descUpdated)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** DateTime changedDate;
***REMOVED******REMOVED******REMOVED*** if (wiItem.Revisions.Count > rev.Index + 1)
***REMOVED******REMOVED******REMOVED******REMOVED***changedDate = RevisionUtility.NextValidDeltaRev(rev.Time, wiItem.Revisions[rev.Index + 1].Time);
***REMOVED******REMOVED******REMOVED*** else
***REMOVED******REMOVED******REMOVED******REMOVED***changedDate = RevisionUtility.NextValidDeltaRev(rev.Time);

***REMOVED******REMOVED******REMOVED*** wi.Fields["System.ChangedDate"].Value = changedDate;
***REMOVED******REMOVED******REMOVED*** wi.Fields["System.ChangedBy"].Value = rev.Author;
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return descUpdated;
***REMOVED***   ***REMOVED***

***REMOVED***   public bool ImportRevision(WiRevision rev, WorkItem wi)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** bool incomplete = false;

***REMOVED******REMOVED******REMOVED*** if (rev.Index == 0)
***REMOVED******REMOVED******REMOVED******REMOVED***EnsureClasificationFields(rev);

***REMOVED******REMOVED******REMOVED*** EnsureDateFields(rev);
***REMOVED******REMOVED******REMOVED*** EnsureAuthorFields(rev);

***REMOVED******REMOVED******REMOVED*** var attachmentMap = new Dictionary<string, Attachment>();
***REMOVED******REMOVED******REMOVED*** if (rev.Attachments.Any() && !ApplyAttachments(rev, wi, attachmentMap))
***REMOVED******REMOVED******REMOVED******REMOVED***incomplete = true;

***REMOVED******REMOVED******REMOVED*** if (rev.Fields.Any() && !UpdateWIFields(rev.Fields, wi))
***REMOVED******REMOVED******REMOVED******REMOVED***incomplete = true;

***REMOVED******REMOVED******REMOVED*** if (rev.Links.Any() && !ApplyLinks(rev, wi))
***REMOVED******REMOVED******REMOVED******REMOVED***incomplete = true;

***REMOVED******REMOVED******REMOVED*** if (incomplete)
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Warning, $"***REMOVED***rev.ToString()***REMOVED*** - not all changes were implemented");

***REMOVED******REMOVED******REMOVED*** if (!rev.Attachments.Any(a => a.Change == ReferenceChangeType.Added) && rev.AttachmentReferences)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Info, $"Correcting description on ***REMOVED***rev.ToString()***REMOVED***");
***REMOVED******REMOVED******REMOVED******REMOVED***CorrectDescription(wi, _context.GetItem(rev.ParentOriginId), rev);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** SaveWorkItem(rev, wi);

***REMOVED******REMOVED******REMOVED*** if (rev.Attachments.Any(a => a.Change == ReferenceChangeType.Added) && rev.AttachmentReferences)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Info, $"Correcting description on separate revision on ***REMOVED***rev.ToString()***REMOVED***");

***REMOVED******REMOVED******REMOVED******REMOVED***try
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    if (CorrectDescription(wi, _context.GetItem(rev.ParentOriginId), rev))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   SaveWorkItem(rev, wi);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***catch (Exception ex)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    Logger.Log(ex);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** _context.Journal.MarkRevProcessed(rev.ParentOriginId, wi.Id, rev.Index);
***REMOVED******REMOVED******REMOVED*** foreach (var wiAtt in rev.Attachments)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***if (attachmentMap.TryGetValue(wiAtt.AttOriginId, out Attachment tfsAtt) && tfsAtt.IsSaved)
***REMOVED******REMOVED******REMOVED******REMOVED***    _context.Journal.MarkAttachmentAsProcessed(wiAtt.AttOriginId, tfsAtt.Id);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***


***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Info, $"Imported ***REMOVED***rev.ToString()***REMOVED***");

***REMOVED******REMOVED******REMOVED*** wi.Close();

***REMOVED******REMOVED******REMOVED*** return true;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (AbortMigrationException ame)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** throw new AbortMigrationException(ame.Reason) ***REMOVED*** Revision = rev ***REMOVED***;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (Exception ex)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(ex);
***REMOVED******REMOVED******REMOVED*** return false;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   private void EnsureClasificationFields(Migration.WIContract.WiRevision rev)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (!rev.Fields.Any(f => f.ReferenceName == "System.AreaPath"))
***REMOVED******REMOVED******REMOVED*** rev.Fields.Add(new WiField() ***REMOVED*** ReferenceName = "System.AreaPath", Value = "" ***REMOVED***);

***REMOVED******REMOVED***  if (!rev.Fields.Any(f => f.ReferenceName == "System.IterationPath"))
***REMOVED******REMOVED******REMOVED*** rev.Fields.Add(new WiField() ***REMOVED*** ReferenceName = "System.IterationPath", Value = "" ***REMOVED***);
***REMOVED***   ***REMOVED***

***REMOVED***   #endregion
***REMOVED***
***REMOVED***