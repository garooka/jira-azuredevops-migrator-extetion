***REMOVED***
***REMOVED***
***REMOVED***
using System.Net;
using System.Text;
using System.Threading.Tasks;

***REMOVED***
***REMOVED***
    internal class WebClientWrapper : IDisposable
    ***REMOVED***
***REMOVED***   private readonly WebClient _webClient;
***REMOVED***   private readonly JiraProvider _jira;

***REMOVED***   public WebClientWrapper(JiraProvider jira)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  _jira = jira;
***REMOVED******REMOVED***  _webClient = new WebClient();
***REMOVED******REMOVED***  _webClient.DownloadFileCompleted += _webClient_DownloadFileCompleted;
***REMOVED***   ***REMOVED***

***REMOVED***   void _webClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var completionSource = e.UserState as TaskCompletionSource<object>;

***REMOVED******REMOVED***  if (completionSource != null)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (e.Cancelled)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***completionSource.TrySetCanceled();
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** else if (e.Error != null)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***completionSource.TrySetException(e.Error);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED*** else
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***completionSource.TrySetResult(null);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   public Task DownloadAsync(string url, string fileName)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  _webClient.CancelAsync();

***REMOVED******REMOVED***  var completionSource = new TaskCompletionSource<object>();
***REMOVED******REMOVED***  _webClient.Headers.Remove(HttpRequestHeader.Authorization);
***REMOVED******REMOVED***  _webClient.DownloadFileAsync(new Uri(url), fileName, completionSource);

***REMOVED******REMOVED***  return completionSource.Task;
***REMOVED***   ***REMOVED***

***REMOVED***   public Task DownloadWithAuthenticationAsync(string url, string fileName)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (String.IsNullOrEmpty(_jira.Settings.UserID) || String.IsNullOrEmpty(_jira.Settings.Pass))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** throw new InvalidOperationException("Unable to download file, user and/or password are missing. You can specify credentials in the configuration file");
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  _webClient.CancelAsync();

***REMOVED******REMOVED***  var completionSource = new TaskCompletionSource<object>();
***REMOVED******REMOVED***  string encodedUserNameAndPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(_jira.Settings.UserID + ":" + _jira.Settings.Pass));

***REMOVED******REMOVED***  _webClient.Headers.Remove(HttpRequestHeader.Authorization);
***REMOVED******REMOVED***  _webClient.Headers.Add(HttpRequestHeader.Authorization, "Basic " + encodedUserNameAndPassword);
***REMOVED******REMOVED***  _webClient.DownloadFileAsync(new Uri(url), fileName, completionSource);

***REMOVED******REMOVED***  return completionSource.Task;
***REMOVED***   ***REMOVED***

***REMOVED***   #region IDisposable Support
    
***REMOVED***   private bool disposedValue = false;

***REMOVED***   protected virtual void Dispose(bool disposing)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (!disposedValue)
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** if (disposing)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***_webClient.Dispose();
***REMOVED******REMOVED******REMOVED*** ***REMOVED***

***REMOVED******REMOVED******REMOVED*** disposedValue = true;
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED***   ***REMOVED***

***REMOVED***   public void Dispose()
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  Dispose(true);
***REMOVED***   ***REMOVED***

***REMOVED***   #endregion
***REMOVED***
***REMOVED***
