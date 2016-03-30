using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients.Senders.Raw
{
	sealed class RawSentApiData : ISentApiData
	{
		public int ByteCount
		{
			get
			{
				return Encoding.UTF8.GetByteCount(this.contentString);
			}
		}

		public SentApiDataBehavior Behavior
		{
			get
			{
				return SentApiDataBehavior.Application_XWwwFormUrlEncorded;
			}
		}

		public byte[] ToByteArray()
		{
			return Encoding.UTF8.GetBytes(this.contentString);
		}

		public override string ToString()
		{
			return this.contentString;
		}

		public RawSentApiData(string contentString)
		{
			this.contentString = contentString;
		}

		private readonly string contentString;
	}
}
