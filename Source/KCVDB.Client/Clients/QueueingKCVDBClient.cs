﻿using System;
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
		static int MinDelayAfterNetworkErrorInMs { get; } = 1000;
		static int MaxDelayAfterNetworkErrorInMs { get; } = 4000;
		static int MinDelayAfterServerkErrorInMs { get; } = 1000;
		static int MaxDelayAfterServerkErrorInMs { get; } = 4000;
		static int MaxChunkSize { get; } = 10;

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
					lock (QueueLock) {
						var item = new QueueItem {
							TrackingId = actualTrackingId,
							ApiData = apiData
						};
						Queue.Enqueue(item);
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


		public async Task SendingThread(CancellationToken cancellationToken)
		{
			try {
				int timeToDelayInMs = 0;
				var rand = new Random();
				while (!cancellationToken.IsCancellationRequested) {
					bool shouldWait = false;

					if (timeToDelayInMs != 0) {
						try {
							await Task.Delay(timeToDelayInMs, cancellationToken);
						}
						catch (OperationCanceledException) {
							break;
						}
						finally {
							timeToDelayInMs = 0;
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

						this.ApiDataSent?.Invoke(this, new ApiDataSentEventArgs(trackingIds, dataArray, sentApiData));
					}
					catch (DataSendingException ex) {
						this.SendingError?.Invoke(this, new SendingErrorEventArgs(trackingIds, "Failed sending API data.", dataArray, ex));
						switch (ex.Reason) {
							case SendingErrorReason.ServerError:
								timeToDelayInMs = rand.Next(MinDelayAfterServerkErrorInMs, MaxDelayAfterServerkErrorInMs);
								break;

							case SendingErrorReason.NetworkError:
								timeToDelayInMs = rand.Next(MinDelayAfterNetworkErrorInMs, MaxDelayAfterNetworkErrorInMs);
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
