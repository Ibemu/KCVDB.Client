using System;
using System.Threading.Tasks;

namespace KCVDB.Client
{
	public interface IKCVDBClient : IDisposable
	{
		/// <summary>
		/// Send API data to the server
		/// </summary>
		/// <param name="requestUri">The request uri of the API call</param>
		/// <param name="responseBody">The request body text of the API call.</param>
		/// <param name="requestBody">The response body text of the API call.</param>
		/// <param name="trackingId">ID to track this sending request. The method generate a new ID if null is specified.</param>
		/// <returns>Track ID</returns>
		Task<Guid> SendRequestDataAsync(
			Uri requestUri,
			int statusCode,
			string requestBody,
			string responseBody,
			string dateHeaderValue,
			Guid? trackingId = null);

		event EventHandler<ApiDataSendEventArgs> ApiDataSending;
		event EventHandler<ApiDataSendEventArgs> ApiDataSent;
		event EventHandler<FatalErrorEventArgs> FatalError;
		event EventHandler<InternalErrorEventArgs> InternalError;
		event EventHandler<SendingErrorEventArgs> SendingError;
	}
}
