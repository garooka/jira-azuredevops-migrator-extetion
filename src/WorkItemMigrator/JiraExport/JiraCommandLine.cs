***REMOVED***
***REMOVED***
***REMOVED***
***REMOVED***
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
***REMOVED***
using Microsoft.Extensions.CommandLineUtils;
using Migration.Common;
***REMOVED***
using Migration.WIContract;
using Newtonsoft.Json;

***REMOVED***
***REMOVED***
    public class JiraCommandLine
    ***REMOVED***
***REMOVED***   private CommandLineApplication commandLineApplication;
***REMOVED***   private string[] args;

***REMOVED***   public JiraCommandLine(params string[] args)
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
***REMOVED******REMOVED***  commandLineApplication.Name = "jira-export";

***REMOVED******REMOVED***  CommandOption userOption = commandLineApplication.Option("-u <username>", "Username for authentication", CommandOptionType.SingleValue);
***REMOVED******REMOVED***  CommandOption passwordOption = commandLineApplication.Option("-p <password>", "Password for authentication", CommandOptionType.SingleValue);
***REMOVED******REMOVED***  CommandOption urlOption = commandLineApplication.Option("--url <accounturl>", "Url for the account", CommandOptionType.SingleValue);
***REMOVED******REMOVED***  CommandOption configOption = commandLineApplication.Option("--config <configurationfilename>", "Export the work items based on this configuration file", CommandOptionType.SingleValue);
***REMOVED******REMOVED***  CommandOption forceOption = commandLineApplication.Option("--force", "Forces execution from start (instead of continuing from previous run)", CommandOptionType.NoValue);

***REMOVED******REMOVED***  commandLineApplication.OnExecute(() =>
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** bool forceFresh = forceOption.HasValue() ? true : false;

***REMOVED******REMOVED******REMOVED*** if (configOption.HasValue())
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***ExecuteMigration(userOption, passwordOption, urlOption, configOption, forceFresh);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** else
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***commandLineApplication.ShowHelp();
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** return 0;
***REMOVED******REMOVED***  ***REMOVED***);
***REMOVED***   ***REMOVED***

***REMOVED***   private void ExecuteMigration(CommandOption user, CommandOption password, CommandOption url, CommandOption configFile, bool forceFresh)
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

***REMOVED******REMOVED******REMOVED*** // template used in Azure DevOps/TFS
***REMOVED******REMOVED******REMOVED*** TemplateType template;
***REMOVED******REMOVED******REMOVED*** switch(config.ProcessTemplate.ToLower())
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***case "scrum": template = TemplateType.Scrum; break;
***REMOVED******REMOVED******REMOVED******REMOVED***case "agile": template = TemplateType.Agile; break;
***REMOVED******REMOVED******REMOVED******REMOVED***case "cmmi": template = TemplateType.CMMI; break;
***REMOVED******REMOVED******REMOVED******REMOVED***default: template = TemplateType.Scrum; break;
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** var downloadOptions = JiraProvider.DownloadOptions.IncludeParentEpics | JiraProvider.DownloadOptions.IncludeSubItems | JiraProvider.DownloadOptions.IncludeParents;

***REMOVED******REMOVED******REMOVED*** Logger.Init(migrationWorkspace, logLevel);

***REMOVED******REMOVED******REMOVED*** var jiraSettings = new JiraSettings(user.Value(), password.Value(), url.Value(), config.SourceProject)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***BatchSize = config.BatchSize,
***REMOVED******REMOVED******REMOVED******REMOVED***UserMappingFile = config.UserMappingFile != null ? Path.Combine(migrationWorkspace, config.UserMappingFile) : string.Empty,
***REMOVED******REMOVED******REMOVED******REMOVED***AttachmentsDir = Path.Combine(migrationWorkspace, config.AttachmentsFolder),
***REMOVED******REMOVED******REMOVED******REMOVED***EpicLinkField = config.EpicLinkField != null ? config.EpicLinkField : string.Empty,
***REMOVED******REMOVED******REMOVED******REMOVED***SprintField = config.SprintField != null ? config.SprintField : string.Empty,
***REMOVED******REMOVED******REMOVED******REMOVED***JQL = config.Query
***REMOVED******REMOVED******REMOVED*** ***REMOVED***;

***REMOVED******REMOVED******REMOVED*** JiraProvider jiraProvider = JiraProvider.Initialize(jiraSettings);
***REMOVED******REMOVED******REMOVED*** var mapper = new JiraMapper(jiraProvider, config);
***REMOVED******REMOVED******REMOVED*** var localProvider = new WiItemProvider(migrationWorkspace);

***REMOVED******REMOVED******REMOVED*** var exportedKeys = new HashSet<string>(Directory.EnumerateFiles(migrationWorkspace, "*.json").Select(f => Path.GetFileNameWithoutExtension(f)));
***REMOVED******REMOVED******REMOVED*** foreach (var issue in jiraProvider.EnumerateIssues(jiraSettings.JQL, forceFresh ? new HashSet<string>(Enumerable.Empty<string>()) : exportedKeys, downloadOptions))
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***WiItem wiItem = mapper.Map(issue, template);
***REMOVED******REMOVED******REMOVED******REMOVED***localProvider.Save(wiItem);
***REMOVED******REMOVED******REMOVED******REMOVED***Logger.Log(LogLevel.Info, $"Exported ***REMOVED***wiItem.ToString()***REMOVED***");
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