using FastDiff;
using KCVDB.Client;
using KCVDB.Client.Clients;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KanColleDbPost
{
	sealed class ApiDataDeserializer
	{
		public string[] Test(Guid trackingId, KCVDB.Client.ApiData sourceData, byte[] requestBodyByteArray)
		{
			using (var stream = new MemoryStream(requestBodyByteArray)) {
				return Serializer.DeserializeItems<KancolleApiSendModel>(stream, PrefixStyle.Base128, 0).SelectMany(sendModel => {
					var targetData = this.ToApiData(sendModel);
					if (sourceData.RequestBody == targetData.RequestBody && sourceData.ResponseBody == targetData.ResponseBody) {
						double originalRequestByteCount = Encoding.UTF8.GetByteCount(sourceData.RequestBody);
						double originalResponseByteCount = Encoding.UTF8.GetByteCount(sourceData.ResponseBody);
						double originalByteCount = originalRequestByteCount + originalResponseByteCount;
						double modifiedRequestByteCount;
						using (var modifiedRequestStream = new MemoryStream()) {
							Serializer.Serialize(modifiedRequestStream, sendModel.RequestValuePatches);
							modifiedRequestByteCount = modifiedRequestStream.Length;
						}
						double modifiedResponseByteCount;
						using (var modifiedResponseStream = new MemoryStream()) {
							Serializer.Serialize(modifiedResponseStream, sendModel.ResponseValuePatches);
							modifiedResponseByteCount = modifiedResponseStream.Length;
						}
						double modifiedByteCount = modifiedRequestByteCount + modifiedResponseByteCount;
						return new[] {
							string.Format("Req: {0,7} -> {1,7}, {2,7:0.00%}", originalRequestByteCount, modifiedRequestByteCount, modifiedRequestByteCount / originalRequestByteCount),
							string.Format("Res: {0,7} -> {1,7}, {2,7:0.00%}", originalResponseByteCount, modifiedResponseByteCount, modifiedResponseByteCount / originalResponseByteCount)
						};
					}
					else {
						return new[] { "Failed." };
					}
				}).ToArray();
			}
		}

		private ApiData ToApiData(KancolleApiSendModel sendModel)
		{
			string previousRequestBody = "";
			string previousResponseBody = "";
			ApiData previousData;
			if (this.items.TryGetValue(sendModel.Path, out previousData)) {
				previousRequestBody = previousData.RequestBody;
				previousResponseBody = previousData.ResponseBody;
			}
			var data = new ApiData {
				RequestBody = Apply(previousRequestBody, sendModel.RequestValuePatches),
				ResponseBody = Apply(previousResponseBody, sendModel.ResponseValuePatches),
			};
			this.items[sendModel.Path] = data;
			return data;
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

		private double sumOriginalRequestByteCount = 0;
		private double sumOriginalResponseByteCount = 0;
		private double sumModifiedRequestByteCount = 0;
		private double sumModifiedResponseByteCount = 0;
		private Dictionary<string, ApiData> items = new Dictionary<string, ApiData>();
	}
}
