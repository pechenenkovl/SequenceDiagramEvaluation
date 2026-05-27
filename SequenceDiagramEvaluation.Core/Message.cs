namespace SequenceDiagramEvaluation.Core
{
    /// <summary>
    /// Represents a message between participants in the sequence diagram.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the source participant (sender) of the message.
        /// </summary>
        public Participant From { get; set; } = null!;

        /// <summary>
        /// Gets or sets the target participant (receiver) of the message.
        /// </summary>
        public Participant To { get; set; } = null!;

        /// <summary>
        /// Gets or sets the content/label of the message.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the message arrow.
        /// </summary>
        public MessageType Type { get; set; }

        /// <summary>
        /// Gets or sets the parent block this message belongs to.
        /// Null if the message is at the root level (not inside any block).
        /// </summary>
        public Block? ParentBlock { get; set; }

        /// <summary>
        /// Gets or sets the order/sequence number of the message in the diagram.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets whether this is a self-call (message to the same participant).
        /// </summary>
        public bool IsSelfCall => From?.Id == To?.Id;

        public override string ToString()
        {
            var arrow = Type switch
            {
                MessageType.SyncRequest => "->>",
                MessageType.SyncRequestWithArrowhead => "->>+",
                MessageType.SyncRequestDeactivate => "->>\u2212",
                MessageType.AsyncRequest => "-->>",
                MessageType.AsyncRequestWithArrowhead => "-->>+",
                MessageType.AsyncRequestDeactivate => "-->>\u2212",
                MessageType.SolidLine => "->",
                MessageType.DottedLine => "-->",
                MessageType.SolidLineWithCross => "-x",
                MessageType.DottedLineWithCross => "--x",
                MessageType.SolidLineWithOpenArrow => "-)",
                MessageType.DottedLineWithOpenArrow => "--)",
                _ => "->"
            };
            return $"{From?.Id} {arrow} {To?.Id}: {Content}";
        }
    }
}
