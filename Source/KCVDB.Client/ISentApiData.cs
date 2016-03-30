namespace KCVDB.Client
{
	public interface ISentApiData
	{
		int PayloadByteCount { get; }

		string PayloadString { get; }

		byte[] PayloadByteArray { get; }

		SentApiDataPayloadFlags PayloadFlags { get; }

		string ContentType { get; }
	}
}
