using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients.Senders
{
	public enum SentApiDataBehavior
	{
		Application_XWwwFormUrlEncorded = 1,
		Multipart_FormData,
		Application_OctetStream
	}
}
