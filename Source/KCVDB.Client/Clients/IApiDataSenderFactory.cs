using System;

namespace KCVDB.Client.Clients
{
	public interface IApiDataSenderFactory
	{
		/// <summary>
		/// Creates new instance of ApiDataSender.
		/// </summary>
		/// <param name="apiServerUri">URI of the KCVDB API server</param>
		/// <param name="agentId">Agent ID of the client</param>
		/// <param name="actualSessionId">The session ID</param>
		/// <returns>Created instance of ApiDataSEnder</returns>
		IApiDataSender CreateSender(
			Uri apiServerUri,
			string agentId,
			string actualSessionId);
	}
}
