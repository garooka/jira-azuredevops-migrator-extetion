***REMOVED***
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Migration.Common;
***REMOVED***
***REMOVED***
using System.Threading.Tasks;

namespace WorkItemImport
***REMOVED***
    public class ImportCommandLine
    ***REMOVED***
***REMOVED***   private CommandLineApplication commandLineApplication;
***REMOVED***   private string[] args;

***REMOVED***   public ImportCommandLine(params string[] args)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  InitCommandLine(args);
***REMOVED***   ***REMOVED***

***REMOVED***   private void InitCommandLine(params string[] args)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: true);
***REMOVED******REMOVED***  this.args = args;
***REMOVED******REMOVED***  ConfigureCommandLineParserWithOptions();
***REMOVED***   ***REMOVED***

***REMOVED***   private void ConfigureCommandLineParserWithOptions()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  commandLineApplication.HelpOption("-? | -h | --help");
***REMOVED******REMOVED***  commandLineApplication.FullName = "Work item migration tool that assists with moving Jira items to Azure DevOps or TFS.";
***REMOVED******REMOVED***  commandLineApplication.Name = "wi-import";

***REMOVED******REMOVED***  CommandOption tokenOption = commandLineApplication.Option("--token <accesstoken>", "Personal access token to use for authentication", CommandOptionType.SingleValue);
***REMOVED******REMOVED***  CommandOption urlOption = commandLineApplication.Option("--url <accounturl>", "Url for the account", CommandOptionType.SingleValue);
***REMOVED******REMOVED***  CommandOption configOption = commandLineApplication.Option("--config <configurationfilename>", "Import the work items based on the configuration file", CommandOptionType.SingleValue);
***REMOVED******REMOVED***  CommandOption forceOption = commandLineApplication.Option("--force", "Forces execution from start (instead of continuing from previous run)", CommandOptionType.NoValue);

***REMOVED******REMOVED***  commandLineApplication.OnExecute(() =>
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** bool forceFresh = forceOption.HasValue() ? true : false;

***REMOVED******REMOVED******REMOVED*** if (configOption.HasValue())
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***ExecuteMigration(tokenOption, urlOption, configOption, forceFresh);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** else
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***commandLineApplication.ShowHelp();
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** return 0;
***REMOVED******REMOVED***  ***REMOVED***);
***REMOVED***   ***REMOVED***

***REMOVED***   private void ExecuteMigration(CommandOption token, CommandOption url, CommandOption configFile, bool forceFresh)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  ConfigJson config = null;
***REMOVED******REMOVED***  try
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** string configFileName = configFile.Value();
***REMOVED******REMOVED******REMOVED*** ConfigReaderJson configReaderJson = new ConfigReaderJson(configFileName);
***REMOVED******REMOVED******REMOVED*** config = configReaderJson.Deserialize();

***REMOVED******REMOVED******REMOVED*** // Migration session level settings
***REMOVED******REMOVED******REMOVED*** // where the logs and journal will be saved, logs aid debugging, journal is for recovery of interupted process
***REMOVED******REMOVED******REMOVED*** string migrationWorkspace = config.Workspace;

***REMOVED******REMOVED******REMOVED*** // level of log messages that will be let through to console
***REMOVED******REMOVED******REMOVED*** LogLevel logLevel;
***REMOVED******REMOVED******REMOVED*** switch (config.LogLevel)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***case "Info": logLevel = LogLevel.Info; break;
***REMOVED******REMOVED******REMOVED******REMOVED***case "Debug": logLevel = LogLevel.Debug; break;
***REMOVED******REMOVED******REMOVED******REMOVED***case "Warning": logLevel = LogLevel.Warning; break;
***REMOVED******REMOVED******REMOVED******REMOVED***case "Error": logLevel = LogLevel.Error; break;
***REMOVED******REMOVED******REMOVED******REMOVED***case "Critical": logLevel = LogLevel.Critical; break;
***REMOVED******REMOVED******REMOVED******REMOVED***default: logLevel = LogLevel.Debug; break;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** // set up log, journal and run session settings
***REMOVED******REMOVED******REMOVED*** var context = MigrationContext.Init(migrationWorkspace, logLevel, forceFresh);

***REMOVED******REMOVED******REMOVED*** // connection settings for Azure DevOps/TFS:
***REMOVED******REMOVED******REMOVED*** // full base url incl https, name of the project where the items will be migrated (if it doesn't exist on destination it will be created), personal access token
***REMOVED******REMOVED******REMOVED*** var settings = new Settings(url.Value(), config.TargetProject, token.Value())
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***BaseAreaPath = config.BaseAreaPath ?? string.Empty, // Root area path that will prefix area path of each migrated item
***REMOVED******REMOVED******REMOVED******REMOVED***BaseIterationPath = config.BaseIterationPath ?? string.Empty, // Root iteration path that will prefix each iteration
***REMOVED******REMOVED******REMOVED******REMOVED***IgnoreFailedLinks = config.IgnoreFailedLinks,
***REMOVED******REMOVED******REMOVED******REMOVED***ProcessTemplate = config.ProcessTemplate
***REMOVED******REMOVED******REMOVED*** ***REMOVED***;

***REMOVED******REMOVED******REMOVED*** // initialize Azure DevOps/TFS connection. Creates/fetches project, fills area and iteration caches.
***REMOVED******REMOVED******REMOVED*** var agent = Agent.Initialize(context, settings);
***REMOVED******REMOVED******REMOVED*** if (agent == null)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Error, "Azure DevOps/TFS initialization error. Exiting...");
***REMOVED******REMOVED******REMOVED******REMOVED***return;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** var executionBuilder = new ExecutionPlanBuilder(context);
***REMOVED******REMOVED******REMOVED*** var plan = executionBuilder.BuildExecutionPlan();

***REMOVED******REMOVED******REMOVED*** while (plan.TryPop(out ExecutionPlan.ExecutionItem executionItem))
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***try
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    if (!forceFresh && context.Journal.IsItemMigrated(executionItem.OriginId, executionItem.Revision.Index))
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   continue;

***REMOVED******REMOVED******REMOVED******REMOVED***    WorkItem wi = null;

***REMOVED******REMOVED******REMOVED******REMOVED***    if (executionItem.WiId > 0)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   wi = agent.GetWorkItem(executionItem.WiId);
***REMOVED******REMOVED******REMOVED******REMOVED***    else
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   wi = agent.CreateWI(executionItem.WiType);

***REMOVED******REMOVED******REMOVED******REMOVED***    agent.ImportRevision(executionItem.Revision, wi);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***catch (AbortMigrationException)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    Logger.Log(LogLevel.Info, "Aborting migration...");
***REMOVED******REMOVED******REMOVED******REMOVED***    break;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***catch (Exception ex)
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    try
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   Logger.Log(ex);
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***    catch (AbortMigrationException)
***REMOVED******REMOVED******REMOVED******REMOVED***    ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***   break;
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED******REMOVED***
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (CommandParsingException e)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, $"Invalid command line option(s): ***REMOVED***e***REMOVED***");
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED***  catch (Exception e)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** Logger.Log(LogLevel.Error, $"Unexpected error: ***REMOVED***e***REMOVED***");
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   public void Run()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  commandLineApplication.Execute(args);
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***