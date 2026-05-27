namespace SequenceDiagramEvaluation.Core
{
    /// <summary>
    /// Represents the result of parsing sequence diagrams from a source.
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// Gets the list of successfully parsed sequence diagrams.
        /// </summary>
        public List<SequenceDiagram> Diagrams { get; set; } = new();

        /// <summary>
        /// Gets the list of parsing errors or warnings.
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Gets whether the parsing was successful (at least one diagram parsed).
        /// </summary>
        public bool IsSuccess => Diagrams.Count > 0;

        /// <summary>
        /// Gets the total number of parsed diagrams.
        /// </summary>
        public int DiagramCount => Diagrams.Count;
    }
}
