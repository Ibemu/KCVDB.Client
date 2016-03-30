using System.Text;

namespace KCVDB.Client.Clients.Senders.Raw
{
	sealed class RawSentApiData : ISentApiData
	{
		public RawSentApiData(
			string contentString,
			string contentType,
			Encoding contentEncoding)
		{
			PayloadString = contentString;
			ContentType = contentType;
			PayloadByteArray = contentEncoding.GetBytes(contentString);
		}

		public string ContentType { get; }
		public string PayloadString { get; }
		public byte[] PayloadByteArray { get; }
		public int PayloadByteCount => PayloadByteArray.Length;
		public SentApiDataPayloadFlags PayloadFlags { get; } = SentApiDataPayloadFlags.String;
	}
}
