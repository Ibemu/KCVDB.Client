using System;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients
{
	interface IApiDataSender : IDisposable
	{
		Task<byte[]> SendData(ApiData data);
	}
}
