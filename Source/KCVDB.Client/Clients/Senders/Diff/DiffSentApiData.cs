using System;

namespace KCVDB.Client.Clients.Senders.Diff
{
	sealed class DiffSentApiData : ISentApiData
	{
		public int PayloadByteCount
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

		public byte[] PayloadByteArray
		{
			get
			{
				return this.contentByteArray;
			}
		}

		public string PayloadString
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public SentApiDataPayloadFlags PayloadFlags
		{
			get
			{
				return SentApiDataPayloadFlags.ByteArray;
			}
		}

		public DiffSentApiData(byte[] contentByteArray)
		{
			this.contentByteArray = contentByteArray;
		}

		private readonly byte[] contentByteArray;
	}
}
