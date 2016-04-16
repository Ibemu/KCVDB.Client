using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using KCVDB.Client.Clients.Utilities.Http;

namespace KCVDB.Client.Clients.Senders.Gzip
{
	class GzipApiDataSender : IApiDataSender, IDisposable
	{
		static string SendGzipDataRequestPath = "api/send/gzip";
		static UTF8Encoding Utf8nEncoding { get; } = new UTF8Encoding(false);
		
		public bool SupportsMultiPost { get; } = true;
		public string AgentId { get; }
		public string SessionId { get; }
		public Uri BaseUri { get; }
		public HttpClient HttpClient { get; }

		public GzipApiDataSender(
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

		public Task<ISentApiData> SendData(ApiData data)
		{
			throw new NotImplementedException();
		}

		public async Task<ISentApiData> SendData(IEnumerable<ApiData> apiData)
		{
			if (apiData == null) { throw new ArgumentNullException(nameof(apiData)); }
			if (!apiData.Any()) { throw new ArgumentException("empty sequence", nameof(apiData)); }

			// Create request URI
			var uriBuilder = new UriBuilder(BaseUri);
			var basePath = uriBuilder.Path.TrimEnd('/');
			uriBuilder.Path = basePath + "/" + SendGzipDataRequestPath.TrimStart('/');
			var requestUri = uriBuilder.Uri;

			// Convert metadata and api data to json, and compress
			var metadata = new RequestMetadata {
				AgentId = AgentId,
				SessionId = SessionId,
			};
			var requestApiData = apiData
				.Select(x => new RequestApiData(x))
				.ToArray();
			var jsonMetadata = await ConvertToJsonAsync(metadata);
			var jsonApiData = await ConvertToJsonAsync(requestApiData);
			var compressedApiData = await CompressStringAsync(jsonApiData);
			
			// construct content
			var multipartContent = new MultipartFormDataContent();
			multipartContent.Add(new StringContent(jsonMetadata), "metadata");
			multipartContent.Add(new ByteArrayContent(compressedApiData), "body");

			// send the content
			try {
				using (var response = await HttpClient.PostAsync(requestUri, multipartContent)) {
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

			return new GzipSendApiData(jsonApiData, compressedApiData);
		}

		static async Task<string> ConvertToJsonAsync<TObject>(TObject obejctToConvert)
		{
			using (var memoryStream = new MemoryStream()) {
				var serializer = new DataContractJsonSerializer(typeof(TObject));
				serializer.WriteObject(memoryStream, obejctToConvert);

				memoryStream.Seek(0, SeekOrigin.Begin);
				using (var reader = new StreamReader(memoryStream)) {
					return await reader.ReadToEndAsync();
				}
			}
		}

		static async Task<byte[]> CompressStringAsync(string text)
		{
			using (var compressedStream = new MemoryStream()) {
				using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress, true))
				using (var plainStream = new MemoryStream(Utf8nEncoding.GetBytes(text))) {
					await plainStream.CopyToAsync(gzipStream);
				}
				return compressedStream.ToArray();
			}
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
