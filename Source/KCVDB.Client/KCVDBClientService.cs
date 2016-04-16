using System;
using KCVDB.Client.Clients;
using KCVDB.Client.Clients.Senders.Gzip;
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
			return CreateClient(
				agentId,
				sessionId,
				//new RawApiDataSenderFactory(),
				new GzipApiDataSenderFactory(),
				KCVDBClientBehavior.Queueing);
		}

		/// <summary>
		/// Just for unit tests
		/// </summary>
		internal IKCVDBClient CreateClient(
			string agentId,
			string sessionId,
			IApiDataSenderFactory apiSenderFactory,
			KCVDBClientBehavior clientBehaivor = KCVDBClientBehavior.Queueing)
		{
			if (agentId == null) { throw new ArgumentNullException(nameof(agentId)); }
			if (apiSenderFactory == null) { throw new ArgumentNullException(nameof(apiSenderFactory)); }

			var actualSessionId = sessionId ?? Guid.NewGuid().ToString();
			var apiParser = new ApiParser();
			var dataSender = apiSenderFactory.CreateSender(ApiServerUri, agentId, actualSessionId);

			switch (clientBehaivor) {
				case KCVDBClientBehavior.Queueing:
					return new QueueingKCVDBClient(apiParser, dataSender);

				default:
					throw new ArgumentException($"Client behavior {clientBehaivor} is not supported.");
			}
		}
	}

	internal enum KCVDBClientBehavior
	{
		Queueing
	}
}
