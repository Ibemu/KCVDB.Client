using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients.Senders.Diff
{
	sealed class DiffSentApiData : ISentApiData
	{
		public int RequestBodyByteCount
		{
			get
			{
				return this.contentByteArray.Length;
			}
		}

		public string ContentType
		{
			get
			{
				return "application/octet-stream";
			}
		}

		public byte[] RequestBodyByteArray
		{
			get
			{
				return this.contentByteArray;
			}
		}

		public string RequestBodyString
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public SentApiDataRequestBodyFlags RequestBodyFlags
		{
			get
			{
				return SentApiDataRequestBodyFlags.ByteArray;
			}
		}

		public DiffSentApiData(byte[] contentByteArray)
		{
			this.contentByteArray = contentByteArray;
		}

		private readonly byte[] contentByteArray;
	}
}
