using FastDiff;
using ProtoBuf;
using System.Collections.Generic;

namespace KCVDB.Client.Clients
{
	[ProtoContract]
	public sealed class KancolleApiSendModel
	{
		[ProtoMember(1)]
		public string LoginSessionId { get; set; }

		[ProtoMember(2)]
		public string AgentId { get; set; }

		[ProtoMember(3)]
		public string Path { get; set; }

		[ProtoMember(4, OverwriteList = true)]
		public IList<DiffResult> RequestValuePatches { get; set; }

		[ProtoMember(5, OverwriteList = true)]
		public IList<DiffResult> ResponseValuePatches { get; set; }

		[ProtoMember(6, DataFormat = DataFormat.TwosComplement)]
		public int StatusCode { get; set; }

		[ProtoMember(7)]
		public string HttpDate { get; set; }

		[ProtoMember(8)]
		public string LocalTime { get; set; }
	}
}
