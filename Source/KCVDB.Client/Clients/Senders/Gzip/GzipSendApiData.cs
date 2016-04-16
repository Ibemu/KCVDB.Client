using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients.Senders.Gzip
{
	class GzipSendApiData : ISentApiData
	{
		public GzipSendApiData(
			string jsonArray,
			byte[] gzipData)
		{
			PayloadString = jsonArray;
			PayloadByteArray = gzipData;
		}

		public string ContentType => "application/octet-stream";

		public byte[] PayloadByteArray { get; }

		public int PayloadByteCount => PayloadByteArray.Length;

		public SentApiDataPayloadFlags PayloadFlags { get; } = SentApiDataPayloadFlags.ByteArray | SentApiDataPayloadFlags.String;

		public string PayloadString { get; }
	}
}
