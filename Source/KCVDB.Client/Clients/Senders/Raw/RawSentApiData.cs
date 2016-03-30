using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients.Senders.Raw
{
	sealed class RawSentApiData : ISentApiData
	{
		public int RequestBodyByteCount
		{
			get
			{
				return Encoding.UTF8.GetByteCount(this.contentString);
			}
		}

		public ApiDataSenderType SenderType
		{
			get
			{
				return ApiDataSenderType.Application_XWwwFormUrlEncorded;
			}
		}

		public byte[] RequestBodyByteArray
		{
			get
			{
				return Encoding.UTF8.GetBytes(this.contentString);
			}
		}

		public string RequestBodyString
		{
			get
			{
				return this.contentString;
			}
		}

		public SentApiDataRequestBodyFlags RequestBodyFlags
		{
			get
			{
				return SentApiDataRequestBodyFlags.String | SentApiDataRequestBodyFlags.ByteArray;
			}
		}

		public RawSentApiData(string contentString)
		{
			this.contentString = contentString;
		}

		private readonly string contentString;
	}
}
