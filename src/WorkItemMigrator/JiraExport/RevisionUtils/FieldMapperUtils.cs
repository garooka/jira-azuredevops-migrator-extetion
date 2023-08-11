using Common.Config;
using Migration.Common;
using Migration.Common.Config;
using Migration.Common.Log;
using Migration.WIContract;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using NCalc.Domain;
using NCalc;
using System.Xml.Linq;
using Antlr4.Runtime.Atn;
using Atlassian.Jira;
using PanoramicData.NCalcExtensions;

namespace JiraExport
{
    public static class FieldMapperUtils
    {
        public static object MapRemainingWork(string seconds)
        {
            var secs = 0d;
            try
            {
                if(seconds == null)
                {
                    throw new FormatException();
                }
                secs = Convert.ToDouble(seconds);
            } catch (FormatException)
            {
                Logger.Log(LogLevel.Warning, $"A FormatException was thrown when converting RemainingWork value '{seconds}' to double. Defaulting to RemainingWork = null.");
                return null;
            }
            return TimeSpan.FromSeconds(secs).TotalHours;
        }

        public static (bool, object) MapTitle(JiraRevision r)
        {
            if (r == null)
                throw new ArgumentNullException(nameof(r));

            if (r.Fields.TryGetValue("summary", out object summary))
                return (true, $"[{r.ParentItem.Key}] {summary}");
            else
                return (false, null);
        }
        public static (bool, object) MapTitleWithoutKey(JiraRevision r)
        {
            if (r == null)
                throw new ArgumentNullException(nameof(r));

            if (r.Fields.TryGetValue("summary", out object summary))
                return (true, summary);
            else
                return (false, null);
        }

        public static (bool, object) MapValue(JiraRevision r, string itemSource, string itemTarget, ConfigJson config)
        {
            if (r == null)
                throw new ArgumentNullException(nameof(r));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var targetWit = (from t in config.TypeMap.Types where t.Source == r.Type select t.Target).FirstOrDefault();

            var hasFieldValue = r.Fields.TryGetValue(itemSource, out object value);

            if (!hasFieldValue)
                return (false, null);

            foreach (var item in config.FieldMap.Fields)
            {
                if ((((item.Source == itemSource && item.Target == itemTarget) && (item.For.Contains(targetWit) || item.For == "All")) ||
                      item.Source == itemSource && (!string.IsNullOrWhiteSpace(item.NotFor) && !item.NotFor.Contains(targetWit))) &&
                      item.Mapping?.Values != null)
                {
                    if (value == null)
                    {
                        return (true, null);
                    }
                    var mappedValue = (from s in item.Mapping.Values where s.Source == value.ToString() select s.Target).FirstOrDefault();
                    if (string.IsNullOrEmpty(mappedValue))
                    {
                        Logger.Log(LogLevel.Warning, $"Missing mapping value '{value}' for field '{itemSource}' for item type '{r.Type}'.");
                    }
                    return (true, mappedValue);
                }
            }
            return (true, value);

        }

        public static (bool, object) MapRenderedValue(JiraRevision r, string sourceField, bool isCustomField, string customFieldName, ConfigJson config)
        {
            if (r == null)
                throw new ArgumentNullException(nameof(r));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            sourceField = SetCustomFieldName(sourceField, isCustomField, customFieldName);

            var fieldName = sourceField + "$Rendered";

            var targetWit = (from t in config.TypeMap.Types where t.Source == r.Type select t.Target).FirstOrDefault();

            var hasFieldValue = r.Fields.TryGetValue(fieldName, out object value);
            if (!hasFieldValue)
                return (false, null);

            foreach (var item in config.FieldMap.Fields)
            {
                if (((item.Source == fieldName && (item.For.Contains(targetWit) || item.For == "All")) ||
                      item.Source == fieldName && (!string.IsNullOrWhiteSpace(item.NotFor) && !item.NotFor.Contains(targetWit))) &&
                      item.Mapping?.Values != null)
                {
                    var mappedValue = (from s in item.Mapping.Values where s.Source == value.ToString() select s.Target).FirstOrDefault();
                    if (string.IsNullOrEmpty(mappedValue))
                    {
                        Logger.Log(LogLevel.Warning, $"Missing mapping value '{value}' for field '{fieldName}'.");
                    }
                    return (true, mappedValue);
                }
            }
            value = CorrectRenderedHtmlvalue(value, r);

            return (true, value);
        }

        /*
         * @date 21.07.2023
         */
        public static (bool, object) MapExpression(JiraRevision r, Field anItem)
        {
            //Dictionary<string, object> someFields = r.Fields;
            
            try
            {
                // Replace placeholders with values from someFields  
                string expressionString = anItem.Source;

                // Evaluate expression  
                var engine = new ExtendedExpression(expressionString);

                List<string> variables = new List<string>();
                Regex regex = new Regex(@"\[(\w+)\]");
                MatchCollection matches = regex.Matches(expressionString);

                bool includeResult = false;

                foreach (Match match in matches)
                {
                    string variableName = match.Groups[1].Value;
                    if (!variables.Contains(variableName))
                    {
                        variables.Add(variableName);
                    }
                }

                foreach (var aParam in variables) 
                {
                    var value = r.GetFieldValue(aParam);

                    //if (someFields.TryGetValue(aParam.ToLower(), out object value))
                    if(value != null)
                        {
                        Logger.Log(LogLevel.Debug, $"Substituting expression ('{expressionString}') parameters with JIRA issue attribute field '{aParam}', with value '{value}'.");

                        //TODO custom field someFields[aParam]
                        engine.Parameters[aParam] = value; // <= could not determine the required type of field, when its not there (value == null) ? string.Empty : value;

                        includeResult = true;
                    }
                    else
                    {
                        Logger.Log(LogLevel.Debug, $"Missing JIRA issue attribute value for field '{aParam}'.");

                        engine.Parameters[aParam] = string.Empty;
                    }
                }

                engine.EvaluateFunction += delegate (string name, FunctionArgs args)
                {
                    if (name == "MapTags")
                    {
                        object aParamValue = args.Parameters[0].Evaluate();

                        string someLabels = (aParamValue == null ? string.Empty : aParamValue.ToString());
                        args.Result = MapTags(someLabels);

                    }
                    else if (name == "MapArray")
                    {
                        object aParamValue = args.Parameters[0].Evaluate();

                        List<string> someArrayString = (aParamValue != null && aParamValue is List<string> ? (List<string>) aParamValue : null);

                        var aResult = MapArray(someArrayString);

                        args.Result = aResult.Item2;
                        includeResult = includeResult || aResult.Item1;
                    }
                    else if (name == "MapTitle")
                    {
                        args.Result = MapTitle(r);
                        includeResult = true;
                    }
                    else if (name == "MapTitleWithoutKey")
                    {
                        args.Result = MapTitleWithoutKey(r);
                        includeResult = true;
                    }
                    else if (name == "MapSprint")
                    {
                        string someString = args.Parameters[0].Evaluate().ToString();
                        var aSprint = MapSprint(someString);
                        args.Result = aSprint != null ? aSprint.ToString() : "";
                    }
                    else if (name == "MapRemainingWork")
                    {
                        string someString = args.Parameters[0].Evaluate().ToString();
                        args.Result = MapRemainingWork(someString);
                    }
                   /* TODO: else if (name == "MapRendered")
                    {
                        string someString = args.Parameters[0].Evaluate().ToString();

                        args.Result = MapRenderedValue(r, ?, true,
                            jiraProvider.GetCustomId(?), config);

                    }*/
                };

                var result = engine.Evaluate();
                
                // Convert result to string and return  
                return (includeResult, result.ToString());
            }
            catch (Exception ex)
            {
                // Return false and null if something goes wrong  
                return (false, null);
            }
        }

        public static object MapTags(string labels)
        {
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            if (string.IsNullOrWhiteSpace(labels))
                return string.Empty;

            var tags = labels.Split(' ');
            if (!tags.Any())
                return string.Empty;
            else
                return string.Join(";", tags);
        }

        public static (bool, object) MapArray(JiraRevision r, string aFieldName)
        {
            r.Fields.TryGetValue(aFieldName, out object aValue);

            if (aFieldName == null || (aValue != null && aValue is List<string> == false))
                throw new ArgumentNullException(nameof(aFieldName));

            List<string> field = (List<string>)aValue;

            return MapArray(field);
        }
        public static (bool, object) MapArray(List<string> field)
        {            
            if (field == null || !field.Any())
                return (false, string.Empty);
            else
                return (true, string.Join(";", field));
        }

        public static object MapSprint(string iterationPathsString)
        {
            if (string.IsNullOrWhiteSpace(iterationPathsString))
                return null;

            var iterationPaths = iterationPathsString.Split(',').AsEnumerable();
            iterationPaths = iterationPaths.Select(ip => ip.Trim());
            var iterationPath = iterationPaths.Last();

            iterationPath = ReplaceAzdoInvalidCharacters(iterationPath);

            return iterationPath;
        }

        public static string CorrectRenderedHtmlvalue(object value, JiraRevision revision)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (revision == null)
                throw new ArgumentNullException(nameof(revision));

            var htmlValue = value.ToString();

            if (string.IsNullOrWhiteSpace(htmlValue))
                return htmlValue;

            foreach (var att in revision.AttachmentActions.Where(aa => aa.ChangeType == RevisionChangeType.Added).Select(aa => aa.Value))
            {
                if (!string.IsNullOrWhiteSpace(att.Url) && htmlValue.Contains(att.Url))
                    htmlValue = htmlValue.Replace(att.Url, att.Url);
            }

            htmlValue = RevisionUtility.ReplaceHtmlElements(htmlValue);

            string css = ReadEmbeddedFile("JiraExport.jirastyles.css");
            if (string.IsNullOrWhiteSpace(css))
                Logger.Log(LogLevel.Warning, $"Could not read css styles for rendered field in {revision.OriginId}.");
            else
                htmlValue = "<style>" + css + "</style>" + htmlValue;

            return htmlValue;
        }

        private static string ReadEmbeddedFile(string resourceName)
        {
            var assembly = Assembly.GetEntryAssembly();

            try
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (ArgumentNullException)
            {
                return "";
            }
        }
        private static string SetCustomFieldName(string sourceField, bool isCustomField, string customFieldName)
        {
            if (isCustomField)
            {
                sourceField = customFieldName;
            }

            return sourceField;
        }

        private static string ReplaceAzdoInvalidCharacters(string inputString)
        {
            return Regex.Replace(inputString, "[/$?*:\"&<>#%|+]", "", RegexOptions.None, TimeSpan.FromMilliseconds(100));
        }
    }

}