using System;
using System.Net;

namespace KCVDB.Client
{
	[Serializable]
	public class KCVDBException : Exception
	{
		public KCVDBException() { }
		public KCVDBException(string message) : base(message) { }
		public KCVDBException(string message, Exception inner) : base(message, inner) { }
		protected KCVDBException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
	}

	[Serializable]
	public class NonKancolleApiCallUriException : Exception
	{
		public NonKancolleApiCallUriException(Uri uri)
			: base($"The uri is not a one ofa Kancolle API call. (URI: {uri})")
		{
			Uri = uri;
		}

		protected NonKancolleApiCallUriException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }

		public Uri Uri { get; }
	}

	[Serializable]
	public class DataSendingException : KCVDBException
	{
		public DataSendingException(SendingErrorReason reason)
			: this($"API data sending faield by {reason}", reason)
		{ }

		public DataSendingException(string message, SendingErrorReason reason)
			: base($"{message} (Reason: {reason}")
		{
			Reason = reason;
		}

		public DataSendingException(string message, Exception inner, SendingErrorReason reason)
			: base($"{message} (Reason: {reason}", inner)
		{
			Reason = reason;
		}

		protected DataSendingException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }

		public SendingErrorReason Reason { get; }
	}

	public enum SendingErrorReason
	{
		Unknown = 0,
		NetworkError,
		HttpProtocolError,
		ServerError,
	}

	[Serializable]
	public class ParseRequestOrResponseFailureException : DataSendingException
	{
		public ParseRequestOrResponseFailureException(Exception inner, SendingErrorReason reason)
			: base("Failed to parse api data", inner, reason)
		{ }

		protected ParseRequestOrResponseFailureException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
	}

	[Serializable]
	public class HttpSendingFailureException : DataSendingException
	{
		public HttpSendingFailureException(WebException webException, SendingErrorReason reason)
			: base($"Failed to send api data to the server (Status: {webException.Status})", webException, reason)
		{ }


		public HttpSendingFailureException(HttpStatusCode statusCode, SendingErrorReason reason)
			: base($"Failed to send api data to the server (Status: {statusCode})", reason)
		{ }

		protected HttpSendingFailureException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
	}
}