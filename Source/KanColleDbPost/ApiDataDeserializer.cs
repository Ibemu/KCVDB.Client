using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KCVDB.Client;
using KCVDB.Client.Clients.Senders.Diff;
using ProtoBuf;

namespace KanColleDbPost
{
	sealed class ApiDataDeserializer
	{
		public string[] Test(Guid trackingId, ApiData sourceData, ISentApiData sentApiData)
		{
			using (var stream = new MemoryStream(sentApiData.PayloadByteArray)) {
				return Serializer.DeserializeItems<KancolleApiSendModel>(stream, PrefixStyle.Base128, 0).SelectMany(sendModel => {
					var targetData = this.ToApiData(sendModel);
					if (sourceData.RequestBody == targetData.RequestBody && sourceData.ResponseBody == targetData.ResponseBody) {
						double originalRequestByteCount = Encoding.UTF8.GetByteCount(sourceData.RequestBody);
						this.sumOriginalRequestByteCount += originalRequestByteCount;
						double originalResponseByteCount = Encoding.UTF8.GetByteCount(sourceData.ResponseBody);
						this.sumOriginalResponseByteCount += originalResponseByteCount;
						var originalByteCount = originalRequestByteCount + originalResponseByteCount;
						var sumOriginalByteCount = this.sumOriginalRequestByteCount + this.sumOriginalResponseByteCount;
						double modifiedRequestByteCount;
						using (var modifiedRequestStream = new MemoryStream()) {
							Serializer.Serialize(modifiedRequestStream, sendModel.RequestValuePatches);
							modifiedRequestByteCount = modifiedRequestStream.Length;
						}
						this.sumModifiedRequestByteCount += modifiedRequestByteCount;
						double modifiedResponseByteCount;
						using (var modifiedResponseStream = new MemoryStream()) {
							Serializer.Serialize(modifiedResponseStream, sendModel.ResponseValuePatches);
							modifiedResponseByteCount = modifiedResponseStream.Length;
						}
						this.sumModifiedResponseByteCount += modifiedResponseByteCount;
						var modifiedByteCount = modifiedRequestByteCount + modifiedResponseByteCount;
						var sumModifiedByteCount = this.sumModifiedRequestByteCount + this.sumModifiedResponseByteCount;
						return new[] {
							string.Format(
								"Req:  {0,7} -> {1,7}, {2,7:0.00%}: {3,9} -> {4,9}, {5,7:0.00%}",
								originalRequestByteCount,
								modifiedRequestByteCount,
								modifiedRequestByteCount / originalRequestByteCount,
								this.sumOriginalRequestByteCount,
								this.sumModifiedRequestByteCount,
								this.sumModifiedRequestByteCount / this.sumOriginalRequestByteCount),
							string.Format(
								"Res:  {0,7} -> {1,7}, {2,7:0.00%}: {3,9} -> {4,9}, {5,7:0.00%}",
								originalResponseByteCount,
								modifiedResponseByteCount,
								modifiedResponseByteCount / originalResponseByteCount,
								this.sumOriginalResponseByteCount,
								this.sumModifiedResponseByteCount,
								this.sumModifiedResponseByteCount / this.sumOriginalResponseByteCount),
							string.Format(
								"Both: {0,7} -> {1,7}, {2,7:0.00%}: {3,9} -> {4,9}, {5,7:0.00%}",
								originalByteCount,
								modifiedByteCount,
								modifiedByteCount / originalByteCount,
								sumOriginalByteCount,
								sumModifiedByteCount,
								sumModifiedByteCount / sumOriginalByteCount)
						};
					}
					else {
						return new[] { "APIの復元に失敗しました。" };
					}
				}).ToArray();
			}
		}

		private ApiData ToApiData(KancolleApiSendModel sendModel)
		{
			var previousRequestBody = "";
			var previousResponseBody = "";
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
