namespace SequenceDiagramEvaluation.Core
{
    /// <summary>
    /// Represents the type of a block in a sequence diagram.
    /// </summary>
    public enum BlockType
    {
        /// <summary>
        /// Alternative paths (if-else) - alt/else
        /// </summary>
        Alt,

        /// <summary>
        /// An else branch within an alt block
        /// </summary>
        Else,

        /// <summary>
        /// Optional fragment - opt
        /// </summary>
        Opt,

        /// <summary>
        /// Loop fragment - loop
        /// </summary>
        Loop,

        /// <summary>
        /// Parallel execution - par
        /// </summary>
        Par,

        /// <summary>
        /// An "and" branch within a par block
        /// </summary>
        And,

        /// <summary>
        /// Critical region - critical
        /// </summary>
        Critical,

        /// <summary>
        /// Break out of a loop - break
        /// </summary>
        Break,

        /// <summary>
        /// Colored rectangle - rect
        /// </summary>
        Rect,

        /// <summary>
        /// Note element
        /// </summary>
        Note
    }
}
