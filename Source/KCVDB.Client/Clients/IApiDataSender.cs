using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients
{
	public interface IApiDataSender : IDisposable
	{
		Task<ISentApiData> SendData(ApiData data);
		Task<ISentApiData> SendData(IEnumerable<ApiData> apiData);
		bool SupportsMultiPost { get; }
	}
}
