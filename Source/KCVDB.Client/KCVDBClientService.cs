using System;
using KCVDB.Client.Clients;
using KCVDB.Client.Clients.Senders;
using KCVDB.Client.Clients.Senders.Diff;
using KCVDB.Client.Clients.Senders.Raw;

namespace KCVDB.Client
{
	public sealed class KCVDBClientService
	{
		#region Singleton
		static KCVDBClientService instance_;
		public static KCVDBClientService Instance => instance_ ?? (instance_ = new KCVDBClientService());
		#endregion

		public static string DefaultServerAddress = "https://kancollevdataapi.azurewebsites.net/";

		public Uri ApiServerUri { get; set; } = new Uri(DefaultServerAddress);

		KCVDBClientService()
		{ }


		/// <summary>
		/// Create a new 
		/// </summary>
		/// <param name="agentId"></param>
		/// <param name="sessionId"></param>
		/// <returns></returns>
		public IKCVDBClient CreateClient(
			string agentId,
			string sessionId = null)
		{
			return CreateClient(agentId, sessionId, KCVDBClientBehavior.Queueing, "application/octet-stream");
		}

		/// <summary>
		/// Just for unit tests
		/// </summary>
		internal IKCVDBClient CreateClient(
			string agentId,
			string sessionId = null,
			KCVDBClientBehavior clientBehaivor = KCVDBClientBehavior.Queueing,
			string apiDataSenderContentType = "application/x-www-form-urlencoded")
		{
			if (agentId == null) { throw new ArgumentNullException(nameof(agentId)); }

			var actualSessionId = sessionId ?? Guid.NewGuid().ToString();
			var apiParser = new ApiParser();
			IApiDataSender dataSender;
			switch (apiDataSenderContentType) {
				case "application/x-www-form-urlencoded":
					dataSender = new RawApiDataSender(ApiServerUri, agentId, actualSessionId);
					break;
				case "application/octet-stream":
					dataSender = new DiffApiDataSender(ApiServerUri, agentId, actualSessionId);
					break;
				default:
					throw new ArgumentException($"Sent api data behavior {apiDataSenderContentType} is not supported yet.");
			}

			switch (clientBehaivor) {
				case KCVDBClientBehavior.Queueing:
					return new QueueingKCVDBClient(apiParser, dataSender);

				case KCVDBClientBehavior.SendImmediately:
					return new ImmediatelyKCVDBClient(apiParser, dataSender);

				default:
					throw new ArgumentException($"Client behavior {clientBehaivor} is not supported yet.");
			}
		}
	}

	internal enum KCVDBClientBehavior
	{
		Queueing,
		SendImmediately
	}
}
