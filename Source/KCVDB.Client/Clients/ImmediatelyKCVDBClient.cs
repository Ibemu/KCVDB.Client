using System;
using System.Threading.Tasks;
using KCVDB.Client.Clients.Senders;

namespace KCVDB.Client.Clients
{
	sealed class ImmediatelyKCVDBClient : IKCVDBClient
	{
		IApiDataSender DataSender { get; }
		IApiParser ApiParser { get; }

		public ImmediatelyKCVDBClient(
			IApiParser apiParser,
			IApiDataSender dataSender)
		{
			if (apiParser == null) { throw new ArgumentNullException(nameof(apiParser)); }
			if (dataSender == null) { throw new ArgumentNullException(nameof(dataSender)); }

			ApiParser = apiParser;
			DataSender = dataSender;
		}

		public async Task<Guid> SendRequestDataAsync(
			Uri requestUri,
			int statusCode,
			string requestBody,
			string responseBody,
			string dateHeaderValue,
			Guid? trackingId = null)
		{
			if (requestUri == null) { throw new ArgumentNullException(nameof(requestUri)); }
			if (responseBody == null) { throw new ArgumentNullException(nameof(responseBody)); }
			if (requestBody == null) { throw new ArgumentNullException(nameof(requestBody)); }

			Guid actualTrackingId = trackingId ?? Guid.NewGuid();

			ApiData data = null;
			try {
				data = ApiParser.ParseResponse(requestUri, statusCode, requestBody, responseBody, dateHeaderValue);
			}
			catch (Exception ex) {
				this.InternalError?.Invoke(this, new InternalErrorEventArgs(actualTrackingId, "Failed to parse response data.", ex));
				return actualTrackingId;
			}

			try {
				this.ApiDataSending?.Invoke(this, new ApiDataSendingEventArgs(actualTrackingId, data));
				var sentApiData = await DataSender.SendData(data);
				this.ApiDataSent?.Invoke(this, new ApiDataSentEventArgs(actualTrackingId, data, sentApiData));
			}
			catch (DataSendingException ex) {
				this.SendingError?.Invoke(this, new SendingErrorEventArgs(actualTrackingId, "Failed sending API data.", data, ex));
				return actualTrackingId;
			}
			catch (Exception ex) {
				this.InternalError?.Invoke(this, new InternalErrorEventArgs(actualTrackingId, "Failed to something before sending API data.", ex, data));
				return actualTrackingId;
			}

			return actualTrackingId;
		}


		public event EventHandler<ApiDataSendingEventArgs> ApiDataSending;
		public event EventHandler<ApiDataSentEventArgs> ApiDataSent;
		public event EventHandler<SendingErrorEventArgs> SendingError;
		public event EventHandler<InternalErrorEventArgs> InternalError;
		public event EventHandler<FatalErrorEventArgs> FatalError;


		#region IDisposable メンバ
		bool isDisposed_ = false;

		public void Dispose()
		{
			if (isDisposed_) { return; }

			DataSender.Dispose();

			GC.SuppressFinalize(this);
			isDisposed_ = true;
		}
		#endregion
	}
}
