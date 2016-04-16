using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients.Senders.Diff
{
	class DiffApiDataSender : IApiDataSender, IDisposable
	{
		public static string SendSingleDataRequestPath = "api/send";

		public string AgentId { get; }
		public string SessionId { get; }
		public Uri BaseUri { get; }
		public HttpClient HttpClient { get; }

		public bool SupportsMultiPost { get; } = false;

		private ApiDataSerializer serializer;

		public DiffApiDataSender(
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
			serializer = new ApiDataSerializer(SessionId, AgentId);

			HttpClient = new HttpClient();
		}

		public async Task<ISentApiData> SendData(ApiData data)
		{
			if (data == null) { throw new ArgumentNullException(nameof(data)); }

			// Create request URI
			var uriBuilder = new UriBuilder(BaseUri);
			uriBuilder.Path = SendSingleDataRequestPath;
			var requestUri = uriBuilder.Uri;

			using (var stream = new MemoryStream()) {
				serializer.Serialize(stream, data);
				await Task.Delay(1);
				return new DiffSentApiData(stream.ToArray());
			}
		}

		public Task<ISentApiData> SendData(IEnumerable<ApiData> apiData)
		{
			throw new NotSupportedException();
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
