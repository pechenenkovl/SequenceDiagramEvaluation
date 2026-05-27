namespace SequenceDiagramEvaluation.Core
{
    /// <summary>
    /// Represents a participant in the sequence diagram.
    /// Can be an actor (first/initiating participant) or an object.
    /// </summary>
    public class Participant
    {
        /// <summary>
        /// Gets or sets the unique identifier/name of the participant.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display alias of the participant.
        /// If not specified, equals to Id.
        /// </summary>
        public string Alias { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this participant is an actor.
        /// </summary>
        public bool IsActor { get; set; }

        /// <summary>
        /// Gets or sets the order of the participant in the diagram.
        /// </summary>
        public int Order { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Alias) || Alias == Id ? Id : $"{Id} ({Alias})";
        }
    }
}
