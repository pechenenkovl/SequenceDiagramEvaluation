namespace SequenceDiagramEvaluation.Core
{
    /// <summary>
    /// Represents a block in the sequence diagram (alt, opt, loop, par, critical, break, rect, etc.).
    /// </summary>
    public class Block
    {
        /// <summary>
        /// Gets or sets the type of the block.
        /// </summary>
        public BlockType Type { get; set; }

        /// <summary>
        /// Gets or sets the label/condition of the block.
        /// For alt blocks, this is the condition of the first branch.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parent block.
        /// Null if this is a top-level block.
        /// </summary>
        public Block? ParentBlock { get; set; }

        /// <summary>
        /// Gets or sets the list of child blocks nested within this block.
        /// </summary>
        public List<Block> ChildBlocks { get; set; } = new();

        /// <summary>
        /// Gets or sets the messages within this block (direct children, not including nested block messages).
        /// </summary>
        public List<Message> Messages { get; set; } = new();

        /// <summary>
        /// Gets or sets the order of this block in the diagram.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the nesting depth of this block.
        /// Top-level blocks have depth 0.
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Gets the total count of messages in this block and all nested blocks.
        /// </summary>
        public int GetTotalMessageCount()
        {
            var count = Messages.Count;
            foreach (var child in ChildBlocks)
            {
                count += child.GetTotalMessageCount();
            }
            return count;
        }

        public override string ToString()
        {
            return $"{Type} [{Label}] - {Messages.Count} messages, {ChildBlocks.Count} child blocks";
        }
    }
}
