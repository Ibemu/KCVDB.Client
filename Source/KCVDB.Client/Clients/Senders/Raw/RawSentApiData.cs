using System.Text;

namespace KCVDB.Client.Clients.Senders.Raw
{
	sealed class RawSentApiData : ISentApiData
	{
		public int PayloadByteCount
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

		public byte[] PayloadByteArray
		{
			get
			{
				return Encoding.UTF8.GetBytes(this.contentString);
			}
		}

		public string PayloadString
		{
			get
			{
				return this.contentString;
			}
		}

		public SentApiDataPayloadFlags PayloadFlags
		{
			get
			{
				return SentApiDataPayloadFlags.String | SentApiDataPayloadFlags.ByteArray;
			}
		}

		public RawSentApiData(string contentString)
		{
			this.contentString = contentString;
		}

		private readonly string contentString;
	}
}
