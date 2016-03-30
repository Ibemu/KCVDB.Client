using System;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients
{
	public interface IApiDataSender : IDisposable
	{
		Task<ISentApiData> SendData(ApiData data);
	}
}
