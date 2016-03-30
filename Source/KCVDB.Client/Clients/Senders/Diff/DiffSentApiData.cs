using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients.Senders.Diff
{
	sealed class DiffSentApiData : ISentApiData
	{
		public int ByteCount
		{
			get
			{
				return this.contentByteArray.Length;
			}
		}

		public SentApiDataBehavior Behavior
		{
			get
			{
				return SentApiDataBehavior.Application_OctetStream;
			}
		}

		public byte[] ToByteArray()
		{
			return this.contentByteArray;
		}

		public override string ToString()
		{
			return base.ToString();
		}

		public DiffSentApiData(byte[] contentByteArray)
		{
			this.contentByteArray = contentByteArray;
		}

		private readonly byte[] contentByteArray;
	}
}
