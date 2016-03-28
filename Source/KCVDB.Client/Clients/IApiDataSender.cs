using System;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients
{
	interface IApiDataSender : IDisposable
	{
		Task<string> SendData(ApiData data);
	}
}
