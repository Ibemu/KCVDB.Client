using FastDiff;
using ProtoBuf;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KCVDB.Client.Clients
{
	public sealed class KancolleApiSendModelDeserializer
	{
		public IEnumerable<ApiData> Deserialize(Stream stream)
		{
			return Serializer.DeserializeItems<KancolleApiSendModel>(stream, PrefixStyle.Base128, 0).Select(sendModel => {
				string previousRequestValue = "";
				string previousResponseValue = "";
				ApiData previousData;
				if (this.dataDictionary.TryGetValue(sendModel.Path, out previousData)) {
					previousRequestValue = previousData.RequestValue;
					previousResponseValue = previousData.ResponseValue;
				}
				var data = new ApiData {
					LoginSessionId = sendModel.LoginSessionId,
					AgentId = sendModel.AgentId,
					Path = sendModel.Path,
					RequestValue = Apply(previousRequestValue, sendModel.RequestValuePatches),
					ResponseValue = Apply(previousResponseValue, sendModel.ResponseValuePatches),
					StatusCode = sendModel.StatusCode,
					HttpDate = sendModel.HttpDate,
					LocalTime = sendModel.LocalTime,
				};
				this.dataDictionary[sendModel.Path] = data;
				return data;
			});
		}

		private static string Apply(string original, IList<DiffResult> patches)
		{
			var modifiedStringBuilder = new StringBuilder(original.Length);
			var start = 0;
			foreach (var patch in patches ?? Enumerable.Empty<DiffResult>()) {
				modifiedStringBuilder.Append(original, start, patch.OriginalStart - start);
				modifiedStringBuilder.Append(patch.Modified);
				start = patch.OriginalStart + patch.OriginalLength;
			}
			modifiedStringBuilder.Append(original, start, original.Length - start);
			return modifiedStringBuilder.ToString();
		}

		private Dictionary<string, ApiData> dataDictionary = new Dictionary<string, ApiData>();

		public sealed class ApiData
		{
			public string LoginSessionId { get; set; }

			public string AgentId { get; set; }

			public string Path { get; set; }

			public string RequestValue { get; set; }

			public string ResponseValue { get; set; }

			public int StatusCode { get; set; }

			public string HttpDate { get; set; }

			public string LocalTime { get; set; }
		}
	}
}
