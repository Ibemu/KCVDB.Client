using System.Text;

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

		public string ContentType
		{
			get
			{
				return "application/x-www-form-urlencoded";
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
