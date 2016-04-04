using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace KCVDB.Client.Clients.Senders.Diff
{
	class ApiDataSerializer
	{
		public ApiDataSerializer(string sessionId, string agentId)
		{
			this.sessionId = sessionId;
			this.agentId = agentId;
		}

		public void Serialize(Stream stream, ApiData data)
		{
			var path = data.RequestUri.AbsoluteUri;
			var previousRequestBody = "";
			var previousResponseBody = "";
			ApiData previousData;
			if (this.dataDictionary.TryGetValue(path, out previousData)) {
				previousRequestBody = previousData.RequestBody;
				previousResponseBody = previousData.ResponseBody;
			}
			Serializer.SerializeWithLengthPrefix(stream, new KancolleApiSendModel {
				LoginSessionId = this.sessionId,
				AgentId = this.agentId,
				Path = path,
				RequestValuePatches = FastDiff.DiffChar(previousRequestBody, data.RequestBody),
				ResponseValuePatches = FastDiff.DiffChar(previousResponseBody, data.ResponseBody),
				StatusCode = data.StatusCode,
				HttpDate = data.HttpDateHeaderValue,
				LocalTime = data.ReceivedLocalTime.UtcDateTime.ToString("r"),
			}, PrefixStyle.Base128, 0);
			this.dataDictionary[path] = data;
		}

		private readonly string sessionId;

		private readonly string agentId;

		private readonly Dictionary<string, ApiData> dataDictionary = new Dictionary<string, ApiData>();
	}
}
