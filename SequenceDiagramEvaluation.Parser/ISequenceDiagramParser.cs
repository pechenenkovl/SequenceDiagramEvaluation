using SequenceDiagramEvaluation.Core;
using SequenceDiagramEvaluation.Core;

namespace SequenceDiagramEvaluation.Parser
{
    /// <summary>
    /// Interface for parsing sequence diagrams from various input formats.
    /// </summary>
    public interface ISequenceDiagramParser
    {
        /// <summary>
        /// Parses sequence diagrams from a stream (ZIP archive containing LaTeX files).
        /// </summary>
        /// <param name="stream">Stream containing a ZIP archive with main.tex file.</param>
        /// <returns>Collection of parsed sequence diagrams.</returns>
        IEnumerable<SequenceDiagram> Parse(Stream stream);
    }
}
