using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients.Senders
{
	public interface ISentApiData
	{
		int ByteCount { get; }

		string ToString();

		byte[] ToByteArray();

		SentApiDataBehavior Behavior { get; }
	}
}
