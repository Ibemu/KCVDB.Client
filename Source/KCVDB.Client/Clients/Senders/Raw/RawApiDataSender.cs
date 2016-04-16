using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KCVDB.Client.Clients.Utilities.Http;

namespace KCVDB.Client.Clients.Senders.Raw
{
	class RawApiDataSender : IApiDataSender, IDisposable
	{
		public static string SendSingleDataRequestPath = "api/send";

		public string AgentId { get; }
		public string SessionId { get; }
		public Uri BaseUri { get; }
		public HttpClient HttpClient { get; }

		public bool SupportsMultiPost { get; } = false;

		public RawApiDataSender(
			Uri baseUri,
			string agentId,
			string sessionId)
		{
			if (baseUri == null) { throw new ArgumentNullException(nameof(baseUri)); }
			if (agentId == null) { throw new ArgumentNullException(nameof(agentId)); }
			if (sessionId == null) { throw new ArgumentNullException(nameof(sessionId)); }
			BaseUri = baseUri;
			AgentId = agentId;
			SessionId = sessionId;

			HttpClient = new HttpClient();
		}

		public Task<ISentApiData> SendData(IEnumerable<ApiData> apiData)
		{
			throw new NotSupportedException();
		}

		public async Task<ISentApiData> SendData(ApiData data)
		{
			if (data == null) { throw new ArgumentNullException(nameof(data)); }

			// Create request URI
			var uriBuilder = new UriBuilder(BaseUri);
			var basePath = uriBuilder.Path.TrimEnd('/');
			uriBuilder.Path = basePath + "/" + SendSingleDataRequestPath.TrimStart('/');
			var requestUri = uriBuilder.Uri;

			// Create content
			var parameters = new Dictionary<string, string> {
				["LoginSessionId"] = SessionId,
				["AgentId"] = AgentId,
				["Path"] = data.RequestUri.AbsoluteUri,
				["RequestValue"] = data.RequestBody,
				["ResponseValue"] = data.ResponseBody,
				["StatusCode"] = data.StatusCode.ToString(),
				["HttpDate"] = data.HttpDateHeaderValue,
				["LocalTime"] = data.ReceivedLocalTime.UtcDateTime.ToString("r"),
			};
			var contentString = string.Join("&", parameters.Select(x => string.Format("{0}={1}", WebUtility.UrlEncode(x.Key), WebUtility.UrlEncode(x.Value))));
			var content = new StringContent(contentString, Encoding.UTF8, "application/x-www-form-urlencoded");

			try {
				using (var response = await HttpClient.PostAsync(requestUri, content)) {
					if (!response.IsSuccessStatusCode) {
						throw new HttpSendingFailureException(response.StatusCode, response.StatusCode.IsServerError() ? SendingErrorReason.ServerError : SendingErrorReason.HttpProtocolError);
					}
				}
			}
			catch (WebException ex) {
				throw new HttpSendingFailureException(ex,
					ex.Status == WebExceptionStatus.ProtocolError
						? (ex.Response as HttpWebResponse)?.StatusCode.IsServerError() == true
							? SendingErrorReason.ServerError
							: SendingErrorReason.HttpProtocolError
						: SendingErrorReason.NetworkError
					);
			}

			return new RawSentApiData(
				contentString,
				"application/x-www-form-urlencoded",
				Encoding.UTF8);
		}

		#region IDisposable member
		bool isDisposed_ = false;
		virtual protected void Dispose(bool disposing)
		{
			if (isDisposed_) { return; }
			if (disposing) {
				HttpClient?.Dispose();
			}
			isDisposed_ = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}