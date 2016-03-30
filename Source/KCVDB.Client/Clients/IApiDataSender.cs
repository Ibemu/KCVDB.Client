using System;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients.Senders
{
	interface IApiDataSender : IDisposable
	{
		Task<ISentApiData> SendData(ApiData data);
	}
}
