using System.Runtime.Serialization;

namespace KCVDB.Client.Clients.Senders.Gzip
{
	[DataContract]
	class RequestMetadata
	{
		[DataMember]
		public string SessionId { get; set; }

		[DataMember]
		public string AgentId { get; set; }
	}
}
