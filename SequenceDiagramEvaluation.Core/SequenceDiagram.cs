using System;
namespace SequenceDiagramEvaluation.Core
{
    /// <summary>
    /// Represents a complete sequence diagram parsed from Mermaid format.
    /// </summary>
    public class SequenceDiagram
    {
        /// <summary>
        /// Gets or sets the title of the sequence diagram.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the list of participants (actors and objects) in the diagram.
        /// </summary>
        public List<Participant> Participants { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of top-level messages (not inside any block).
        /// </summary>
        public List<Message> Messages { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of top-level blocks (alt, opt, loop, etc.).
        /// </summary>
        public List<Block> Blocks { get; set; } = new();

        /// <summary>
        /// Gets all messages in the diagram including nested ones.
        /// </summary>
        public IEnumerable<Message> GetAllMessages()
        {
            foreach (var message in Messages)
            {
                yield return message;
            }

            foreach (var block in Blocks)
            {
                foreach (var message in GetAllMessagesFromBlock(block))
                {
                    yield return message;
                }
            }
        }

        private static IEnumerable<Message> GetAllMessagesFromBlock(Block block)
        {
            foreach (var message in block.Messages)
            {
                yield return message;
            }

            foreach (var childBlock in block.ChildBlocks)
            {
                foreach (var message in GetAllMessagesFromBlock(childBlock))
                {
                    yield return message;
                }
            }
        }

        /// <summary>
        /// Gets all blocks in the diagram including nested ones.
        /// </summary>
        public IEnumerable<Block> GetAllBlocks()
        {
            foreach (var block in Blocks)
            {
                yield return block;
                foreach (var childBlock in GetAllBlocksFromBlock(block))
                {
                    yield return childBlock;
                }
            }
        }

        private static IEnumerable<Block> GetAllBlocksFromBlock(Block block)
        {
            foreach (var childBlock in block.ChildBlocks)
            {
                yield return childBlock;
                foreach (var nestedBlock in GetAllBlocksFromBlock(childBlock))
                {
                    yield return nestedBlock;
                }
            }
        }
    }
}
