namespace SequenceDiagramEvaluation.Core
{
    /// <summary>
    /// Represents the type of message arrow in a sequence diagram.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Solid line with arrowhead (->>) - Synchronous request
        /// </summary>
        SyncRequest,

        /// <summary>
        /// Solid line with arrowhead and activation (->>+)
        /// </summary>
        SyncRequestWithArrowhead,

        /// <summary>
        /// Solid line with arrowhead and deactivation (->>-)
        /// </summary>
        SyncRequestDeactivate,

        /// <summary>
        /// Dotted line with arrowhead (-->>) - Asynchronous/Response
        /// </summary>
        AsyncRequest,

        /// <summary>
        /// Dotted line with arrowhead and activation (-->>+)
        /// </summary>
        AsyncRequestWithArrowhead,

        /// <summary>
        /// Dotted line with arrowhead and deactivation (-->>-)
        /// </summary>
        AsyncRequestDeactivate,

        /// <summary>
        /// Solid line without arrowhead (->)
        /// </summary>
        SolidLine,

        /// <summary>
        /// Dotted line without arrowhead (-->)
        /// </summary>
        DottedLine,

        /// <summary>
        /// Solid line with a cross at the end (-x)
        /// </summary>
        SolidLineWithCross,

        /// <summary>
        /// Dotted line with a cross at the end (--x)
        /// </summary>
        DottedLineWithCross,

        /// <summary>
        /// Solid line with an open arrow at the end (async, -) )
        /// </summary>
        SolidLineWithOpenArrow,

        /// <summary>
        /// Dotted line with an open arrow at the end (async, --) )
        /// </summary>
        DottedLineWithOpenArrow
    }
}
