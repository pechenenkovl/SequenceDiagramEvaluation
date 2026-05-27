namespace SequenceDiagramEvaluation.Core
{
    /// <summary>
    /// Provides metrics calculation for sequence diagrams.
    /// </summary>
    public class SequenceDiagramMetrics
    {
        private readonly SequenceDiagram _diagram;

        // Block types that are branches within other blocks, not standalone blocks
        private static readonly HashSet<BlockType> BranchTypes = new()
        {
            BlockType.Else,
            BlockType.And
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDiagramMetrics"/> class.
        /// </summary>
        /// <param name="diagram">The sequence diagram to calculate metrics for.</param>
        public SequenceDiagramMetrics(SequenceDiagram diagram)
        {
            _diagram = diagram ?? throw new ArgumentNullException(nameof(diagram));
        }

        /// <summary>
        /// Gets the score for the number of objects and actors.
        /// </summary>
        /// <returns>The total count of participants (actors and objects).</returns>
        public int GetParticipantScore()
        {
            return _diagram.Participants.Count;
        }

        /// <summary>
        /// Gets the score for the number of messages only inside the main block (block level 0).
        /// Messages that are directed from a block or to a block are not included.
        /// </summary>
        /// <returns>The count of messages at the root level (not inside any block).</returns>
        public int GetMainBlockMessageScore()
        {
            return _diagram.Messages.Count;
        }

        /// <summary>
        /// Gets the number of blocks including the main block.
        /// Counts the main block (level 0) plus first-level blocks (direct children).
        /// Does not include nested blocks (block level 2 or deeper),
        /// or branch blocks (else, and) which are part of their parent blocks.
        /// </summary>
        /// <returns>The count of blocks (1 for main + first-level blocks).</returns>
        public int GetFirstLevelBlockCount()
        {
            // Count main block (1) + first-level blocks (excluding branch types)
            return 1 + _diagram.Blocks.Count(b => !BranchTypes.Contains(b.Type));
        }

        /// <summary>
        /// Gets the average number of messages inside first-level blocks.
        /// Does not include the main block. Only counts messages in blocks at level 1.
        /// Messages going to/from internal (nested) blocks are not included.
        /// Messages that go into or out of the target block are included.
        /// Includes messages from branch blocks (else, and) as part of their parent blocks.
        /// </summary>
        /// <returns>The average number of messages per first-level block, or 0 if there are no first-level blocks.</returns>
        public double GetAverageMessagesInFirstLevelBlocks()
        {
            // Get only the main blocks (exclude branch types)
            var firstLevelBlocks = _diagram.Blocks.Where(b => !BranchTypes.Contains(b.Type)).ToList();

            if (firstLevelBlocks.Count == 0)
            {
                return 0;
            }

            int totalMessages = 0;

            foreach (var block in firstLevelBlocks)
            {
                // Count direct messages in the block
                totalMessages += block.Messages.Count;

                // Also count messages from related branch blocks (else/and) that follow this block
                totalMessages += GetBranchMessagesCount(block);
            }

            return (double)totalMessages / firstLevelBlocks.Count;
        }

        /// <summary>
        /// Gets the message count from branch blocks (else/and) that are associated with the given block.
        /// </summary>
        private int GetBranchMessagesCount(Block parentBlock)
        {
            int count = 0;
            var blockIndex = _diagram.Blocks.IndexOf(parentBlock);

            // Look for subsequent branch blocks at the same level
            for (int i = blockIndex + 1; i < _diagram.Blocks.Count; i++)
            {
                var block = _diagram.Blocks[i];

                // If we hit a non-branch block, stop
                if (!BranchTypes.Contains(block.Type))
                {
                    break;
                }

                count += block.Messages.Count;
            }

            return count;
        }

        /// <summary>
        /// Gets all metrics for the sequence diagram.
        /// </summary>
        /// <returns>A <see cref="DiagramMetricsResult"/> containing all calculated metrics.</returns>
        public DiagramMetricsResult GetAllMetrics()
        {
            return new DiagramMetricsResult
            {
                ParticipantScore = GetParticipantScore(),
                MainBlockMessageScore = GetMainBlockMessageScore(),
                FirstLevelBlockCount = GetFirstLevelBlockCount(),
                AverageMessagesInFirstLevelBlocks = GetAverageMessagesInFirstLevelBlocks()
            };
        }
    }

    /// <summary>
    /// Represents the calculated metrics for a sequence diagram.
    /// </summary>
    public class DiagramMetricsResult
    {
        /// <summary>
        /// Gets or sets the score for the number of objects and actors.
        /// </summary>
        public int ParticipantScore { get; set; }

        /// <summary>
        /// Gets or sets the score for the number of messages only inside the main block (block level 0).
        /// </summary>
        public int MainBlockMessageScore { get; set; }

        /// <summary>
        /// Gets or sets the number of blocks at the first level (direct children of main block).
        /// </summary>
        public int FirstLevelBlockCount { get; set; }

        /// <summary>
        /// Gets or sets the average number of messages inside first-level blocks.
        /// </summary>
        public double AverageMessagesInFirstLevelBlocks { get; set; }

        public override string ToString()
        {
            return $"Participants: {ParticipantScore}, " +
                   $"Main Block Messages: {MainBlockMessageScore}, " +
                   $"First Level Blocks: {FirstLevelBlockCount}, " +
                   $"Avg Messages in First Level Blocks: {AverageMessagesInFirstLevelBlocks:F2}";
        }
    }
}
