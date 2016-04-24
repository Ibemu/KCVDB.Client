using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using KCVDB.Client.Clients.Senders;

namespace KCVDB.Client.Clients
{
	sealed class QueueingKCVDBClient : IKCVDBClient
	{
		static TimeSpan MinDelayAfterNetworkError { get; } = TimeSpan.FromSeconds(1);
		static TimeSpan MaxDelayAfterNetworkError { get; } = TimeSpan.FromMinutes(10);
		static TimeSpan NetworkErrorDelayIncrement { get; } = TimeSpan.FromSeconds(10);
		static TimeSpan MinDelayAfterServerkError { get; } = TimeSpan.FromSeconds(1);
		static TimeSpan MaxDelayAfterServerkError { get; } = TimeSpan.FromMinutes(20);
		static TimeSpan ServerErrorDelayIncrement { get; } = TimeSpan.FromSeconds(20);

		static int MaxChunkSize { get; } = 10;
		static int MaxQueueLength { get; } = 2000;

		object QueueLock { get; } = new object();
		object ThreadLock { get; } = new object();
		Queue<QueueItem> Queue { get; } = new Queue<QueueItem>();
		CancellationTokenSource ThreadCancellationTokenSource { get; } = new CancellationTokenSource();
		AutoResetEvent SendEvent { get; } = new AutoResetEvent(true);

		IApiDataSender DataSender { get; }
		IApiParser ApiParser { get; }

		ConfiguredTaskAwaitable? sendingTaskAwaiter_;


		public QueueingKCVDBClient(
			IApiParser apiParser,
			IApiDataSender dataSender
			)
		{
			DataSender = dataSender;
			ApiParser = apiParser;
		}

		public Task<Guid> SendRequestDataAsync(
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

			var actualTrackingId = trackingId ?? Guid.NewGuid();

			// Parse api data
			ApiData apiData = null;
			try {
				apiData = ApiParser.ParseResponse(requestUri, statusCode, requestBody, responseBody, dateHeaderValue);
			}
			catch (Exception ex) {
				this.InternalError?.Invoke(this, new InternalErrorEventArgs(actualTrackingId, "Failed to parse api call data.", ex, apiData));
				return Task.FromResult(Guid.Empty);
			}

			// Enqueue api data
			try {
				if (apiData != null) {
					try {
						lock (QueueLock) {
							if (Queue.Count >= MaxQueueLength) {
								new InvalidOperationException("Reached to maximum queue size limit.");
							}

							var item = new QueueItem {
								TrackingId = actualTrackingId,
								ApiData = apiData
							};
							Queue.Enqueue(item);
						}
					}
					catch (Exception ex) {
						this.FatalError?.Invoke(this, new FatalErrorEventArgs("Failed to enqueue the api data.", ex));
					}
				}
			}
			catch (Exception ex) {
				this.InternalError?.Invoke(this, new InternalErrorEventArgs(actualTrackingId, "Failed to enqueu API call data.", ex, apiData));
				return Task.FromResult(Guid.Empty);
			}

			// trigger sending
			try {
				lock (ThreadLock) {
					if (sendingTaskAwaiter_ == null) {
						sendingTaskAwaiter_ = SendingThread(ThreadCancellationTokenSource.Token).ConfigureAwait(false);
					}
					SendEvent.Set();
				}
			}
			catch (Exception ex) {
				this.InternalError?.Invoke(this, new InternalErrorEventArgs(actualTrackingId, "Failed to trigger sending or creating new thread.", ex, apiData));
			}

			return Task.FromResult(actualTrackingId);
		}

		enum TransferResult {
			Succeeded = 0,
			NetworkError = 1,
			ServerError = 2,
		}


		public async Task SendingThread(CancellationToken cancellationToken)
		{
			try {
				var lastResult = TransferResult.Succeeded;
				var timeToDelay = TimeSpan.Zero;
				var rand = new Random();
				while (!cancellationToken.IsCancellationRequested) {
					bool shouldWait = false;

					if (timeToDelay != TimeSpan.Zero) {
						try {
							await Task.Delay(timeToDelay, cancellationToken);
						}
						catch (OperationCanceledException) {
							break;
						}
					}

					lock (QueueLock) {
						shouldWait = Queue.Count == 0;
					}

					if (shouldWait) {
						try {
							SendEvent.WaitOne();
						}
						catch (Exception ex) {
							this.FatalError(this, new FatalErrorEventArgs("Failed to wait event for sending data. Stopping sending.", ex));
							break;
						}
					}
					
					if (cancellationToken.IsCancellationRequested) { break; }

					lock (QueueLock) {
						if (Queue.Count == 0) {
							continue;
						}
					}

					var itemsToSend = new List<QueueItem>();
					lock (QueueLock) {
						if (DataSender.SupportsMultiPost) {
							itemsToSend.AddRange(Queue.Take(MaxChunkSize));
						}
						else {
							itemsToSend.Add(Queue.Peek());
						}
					}

					if (!itemsToSend.Any()) { continue; }

					var dataArray = itemsToSend.Select(x => x.ApiData).ToArray();
					var trackingIds = itemsToSend.Select(x => x.TrackingId).ToArray();
					try {
						this.ApiDataSending?.Invoke(this, new ApiDataSendingEventArgs(trackingIds, dataArray));

						ISentApiData sentApiData = null;
						if (DataSender.SupportsMultiPost) {
							sentApiData = await DataSender.SendData(dataArray);
						}
						else {
							sentApiData = await DataSender.SendData(dataArray[0]);
						}

						lastResult = TransferResult.Succeeded;
						timeToDelay = TimeSpan.Zero;

						this.ApiDataSent?.Invoke(this, new ApiDataSentEventArgs(trackingIds, dataArray, sentApiData));
					}
					catch (DataSendingException ex) {
						this.SendingError?.Invoke(this, new SendingErrorEventArgs(trackingIds, "Failed sending API data.", dataArray, ex));
						switch (ex.Reason) {
							case SendingErrorReason.ServerError:
								if (lastResult == TransferResult.ServerError) {
									timeToDelay += ServerErrorDelayIncrement;
									if (timeToDelay > MaxDelayAfterServerkError) {
										timeToDelay = MaxDelayAfterServerkError;
									}
								}
								else {
									timeToDelay = MinDelayAfterServerkError;
								}
								lastResult = TransferResult.ServerError;
								break;

							case SendingErrorReason.HttpProtocolError:
							case SendingErrorReason.NetworkError:
							case SendingErrorReason.Unknown:
								if (lastResult == TransferResult.NetworkError) {
									timeToDelay += NetworkErrorDelayIncrement;
									if (timeToDelay > MaxDelayAfterNetworkError) {
										timeToDelay = MaxDelayAfterNetworkError;
									}
								}
								else {
									timeToDelay = MinDelayAfterNetworkError;
								}
								lastResult = TransferResult.NetworkError;
								break;
						}
						continue;
					}
					catch (OperationCanceledException) {
						break;
					}
					catch (Exception ex) {
						this.InternalError?.Invoke(this, new InternalErrorEventArgs(trackingIds, "Failed to something before sending API data.", ex, dataArray));
						continue;
					}

					lock (QueueLock) {
						for (int i = 0; i < itemsToSend.Count && Queue.Count > 0; i++) {
							Queue.Dequeue();
						}
					}
				}
			}
			catch (Exception ex) {
				FatalError?.Invoke(this, new FatalErrorEventArgs("Sending thread aborted. API data will no longer be sent from now.", ex));
			}
		}

		public event EventHandler<ApiDataSendingEventArgs> ApiDataSending;
		public event EventHandler<ApiDataSentEventArgs> ApiDataSent;
		public event EventHandler<InternalErrorEventArgs> InternalError;
		public event EventHandler<SendingErrorEventArgs> SendingError;
		public event EventHandler<FatalErrorEventArgs> FatalError;

		class QueueItem
		{
			public Guid TrackingId { get; set; }
			public ApiData ApiData { get; set; }
		}

		#region IDisposable メンバ
		bool isDisposed_ = false;

		public void Dispose()
		{
			if (isDisposed_) { return; }

			lock (ThreadLock) {
				if (sendingTaskAwaiter_ != null) {
					ThreadCancellationTokenSource.Cancel();
					SendEvent.Set();
					sendingTaskAwaiter_.Value.GetAwaiter().OnCompleted(() => {
						SendEvent.Dispose();
						ThreadCancellationTokenSource.Dispose();
					});
				}
				else {
					SendEvent.Dispose();
					ThreadCancellationTokenSource.Dispose();
				}
			}
			DataSender.Dispose();

			GC.SuppressFinalize(this);
			isDisposed_ = true;
		}
		#endregion
	}
}
