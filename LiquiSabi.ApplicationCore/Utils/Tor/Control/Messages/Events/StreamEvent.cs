using LiquiSabi.ApplicationCore.Utils.Tor.Control.Exceptions;
using LiquiSabi.ApplicationCore.Utils.Tor.Control.Messages.StreamStatus;
using LiquiSabi.ApplicationCore.Utils.Tor.Control.Utils;

namespace LiquiSabi.ApplicationCore.Utils.Tor.Control.Messages.Events;

/// <summary>Circuit event as specified in <c>4.1.2. Stream status changed</c> spec.</summary>
/// <seealso href="https://gitweb.torproject.org/torspec.git/tree/control-spec.txt"/>
public record StreamEvent : IAsyncEvent
{
	public const string EventName = "STREAM";

	public StreamEvent(StreamInfo streamInfo)
	{
		StreamInfo = streamInfo;
	}

	public StreamInfo StreamInfo { get; }

	/// <exception cref="TorControlReplyParseException"/>
	public static StreamEvent FromReply(TorControlReply reply)
	{
		if (reply.StatusCode != StatusCode.AsynchronousEventNotify)
		{
			throw new TorControlReplyParseException($"StreamEvent: Expected {StatusCode.AsynchronousEventNotify} status code.");
		}

		(string value, string remainder) = Tokenizer.ReadUntilSeparator(reply.ResponseLines[0]);

		if (value != EventName)
		{
			throw new TorControlReplyParseException($"StreamEvent: Expected '{EventName}' event name.");
		}

		return new StreamEvent(StreamInfo.ParseLine(remainder));
	}
}
