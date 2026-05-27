using System.IO.Compression;
using System.Text.RegularExpressions;
using SequenceDiagramEvaluation.Core;

namespace SequenceDiagramEvaluation.Parser
{
    /// <summary>
    /// Parser for extracting Mermaid sequence diagrams from LaTeX ZIP archives.
    /// </summary>
    public partial class LatexMermaidSequenceDiagramParser : ISequenceDiagramParser
    {
        private const string MainTexFileName = "main.tex";
        private const string SequenceDiagramStart = "sequenceDiagram";

        /// <inheritdoc />
        public IEnumerable<SequenceDiagram> Parse(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            var latexContent = ExtractLatexContent(stream);
            if (string.IsNullOrEmpty(latexContent))
            {
                yield break;
            }

            var mermaidBlocks = ExtractMermaidSequenceDiagrams(latexContent);

            foreach (var (sectionName, diagramContent) in mermaidBlocks)
            {
                var diagram = ParseMermaidSequenceDiagram(diagramContent);
                if (diagram != null)
                {
                    // Use section name as title if available and diagram doesn't have its own title
                    if (!string.IsNullOrEmpty(sectionName) && string.IsNullOrEmpty(diagram.Title))
                    {
                        diagram.Title = sectionName;
                    }
                    yield return diagram;
                }
            }
        }

        /// <summary>
        /// Extracts the content of main.tex from a ZIP archive.
        /// </summary>
        private static string ExtractLatexContent(Stream zipStream)
        {
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

            var mainTexEntry = archive.Entries
                .FirstOrDefault(e => e.Name.Equals(MainTexFileName, StringComparison.OrdinalIgnoreCase) ||
                                    e.FullName.EndsWith(MainTexFileName, StringComparison.OrdinalIgnoreCase));

            if (mainTexEntry == null)
            {
                return string.Empty;
            }

            using var entryStream = mainTexEntry.Open();
            using var reader = new StreamReader(entryStream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Extracts all Mermaid sequence diagram blocks from LaTeX content.
        /// Looks for diagrams inside \begin{verbatim} ... \end{verbatim} blocks.
        /// Also extracts the preceding \section name for each diagram.
        /// </summary>
        private static List<(string SectionName, string DiagramContent)> ExtractMermaidSequenceDiagrams(string latexContent)
        {
            var diagrams = new List<(string SectionName, string DiagramContent)>();

            // Find all verbatim blocks
            var verbatimRegex = VerbatimBlockRegex();
            var verbatimMatches = verbatimRegex.Matches(latexContent);

            foreach (Match verbatimMatch in verbatimMatches)
            {
                if (verbatimMatch.Success && verbatimMatch.Groups.Count > 1)
                {
                    var verbatimContent = verbatimMatch.Groups[1].Value;

                    // Check if this verbatim block contains a sequence diagram
                    if (verbatimContent.Contains(SequenceDiagramStart, StringComparison.OrdinalIgnoreCase))
                    {
                        // Find the section name before this verbatim block
                        var sectionName = ExtractPrecedingSectionName(latexContent, verbatimMatch.Index);

                        // Extract the sequence diagram content (after sequenceDiagram keyword)
                        var sequenceRegex = SequenceDiagramRegex();
                        var sequenceMatch = sequenceRegex.Match(verbatimContent);

                        if (sequenceMatch.Success && sequenceMatch.Groups.Count > 1)
                        {
                            var content = sequenceMatch.Groups[1].Value.Trim();
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                diagrams.Add((sectionName, content));
                            }
                        }
                        else
                        {
                            // Fallback: if no 'end' keyword found, extract everything after 'sequenceDiagram'
                            var startIndex = verbatimContent.IndexOf(SequenceDiagramStart, StringComparison.OrdinalIgnoreCase);
                            if (startIndex >= 0)
                            {
                                var content = verbatimContent.Substring(startIndex + SequenceDiagramStart.Length).Trim();
                                if (!string.IsNullOrWhiteSpace(content))
                                {
                                    diagrams.Add((sectionName, content));
                                }
                            }
                        }
                    }
                }
            }

            return diagrams;
        }

        /// <summary>
        /// Extracts the section name that precedes the given position in the LaTeX content.
        /// </summary>
        private static string ExtractPrecedingSectionName(string latexContent, int position)
        {
            // Get the content before the verbatim block
            var contentBefore = latexContent.Substring(0, position);

            // Find the last \section{...} before this position
            var sectionRegex = new Regex(@"\\section\s*\{([^}]+)\}");
            var matches = sectionRegex.Matches(contentBefore);

            if (matches.Count > 0)
            {
                var lastMatch = matches[matches.Count - 1];
                if (lastMatch.Groups.Count > 1)
                {
                    return lastMatch.Groups[1].Value.Trim();
                }
            }

            return string.Empty;
        }

        /// Parses a Mermaid sequence diagram text into a SequenceDiagram model.
        /// </summary>
        private static SequenceDiagram? ParseMermaidSequenceDiagram(string mermaidContent)
        {
            var diagram = new SequenceDiagram();
            var lines = mermaidContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            if (lines.Count == 0)
            {
                return null;
            }

            var participants = new Dictionary<string, Participant>(StringComparer.OrdinalIgnoreCase);
            var blockStack = new Stack<Block>();
            var messageOrder = 0;
            var blockOrder = 0;
            var participantOrder = 0;

            foreach (var line in lines)
            {
                // Skip comments
                if (line.StartsWith("%%"))
                {
                    continue;
                }

                // Parse title
                if (line.StartsWith("title", StringComparison.OrdinalIgnoreCase))
                {
                    diagram.Title = ExtractTitle(line);
                    continue;
                }

                // Parse participant/actor
                if (TryParseParticipant(line, ref participantOrder, out var participant))
                {
                    if (!participants.ContainsKey(participant.Id))
                    {
                        participants[participant.Id] = participant;
                        diagram.Participants.Add(participant);
                    }
                    continue;
                }

                // Parse block start (alt, opt, loop, par, critical, break, rect)
                if (TryParseBlockStart(line, out var blockType, out var label))
                {
                    var newBlock = new Block
                    {
                        Type = blockType,
                        Label = label,
                        ParentBlock = blockStack.Count > 0 ? blockStack.Peek() : null,
                        Order = blockOrder++,
                        Depth = blockStack.Count
                    };

                    if (blockStack.Count > 0)
                    {
                        blockStack.Peek().ChildBlocks.Add(newBlock);
                    }
                    else
                    {
                        diagram.Blocks.Add(newBlock);
                    }

                    blockStack.Push(newBlock);
                    continue;
                }

                // Parse else/and (alternative branches within alt/par blocks)
                if (TryParseAlternativeBranch(line, out var branchType, out var branchLabel))
                {
                    if (blockStack.Count > 0)
                    {
                        var currentBlock = blockStack.Pop();
                        var parentBlock = currentBlock.ParentBlock;

                        var newBranch = new Block
                        {
                            Type = branchType,
                            Label = branchLabel,
                            ParentBlock = parentBlock,
                            Order = blockOrder++,
                            Depth = currentBlock.Depth
                        };

                        if (parentBlock != null)
                        {
                            parentBlock.ChildBlocks.Add(newBranch);
                        }
                        else
                        {
                            diagram.Blocks.Add(newBranch);
                        }

                        blockStack.Push(newBranch);
                    }
                    continue;
                }

                // Parse block end
                if (line.Equals("end", StringComparison.OrdinalIgnoreCase))
                {
                    if (blockStack.Count > 0)
                    {
                        blockStack.Pop();
                    }
                    continue;
                }

                // Parse message
                if (TryParseMessage(line, participants, ref participantOrder, out var message))
                {
                    message.Order = messageOrder++;

                    if (blockStack.Count > 0)
                    {
                        var currentBlock = blockStack.Peek();
                        message.ParentBlock = currentBlock;
                        currentBlock.Messages.Add(message);
                    }
                    else
                    {
                        diagram.Messages.Add(message);
                    }

                    // Add participants to diagram if not already present
                    if (!diagram.Participants.Contains(message.From))
                    {
                        diagram.Participants.Add(message.From);
                    }
                    if (!diagram.Participants.Contains(message.To))
                    {
                        diagram.Participants.Add(message.To);
                    }
                }
            }

            // Mark the first participant as actor if not explicitly set
            if (diagram.Participants.Count > 0 && !diagram.Participants.Any(p => p.IsActor))
            {
                diagram.Participants[0].IsActor = true;
            }

            return diagram;
        }

        private static string ExtractTitle(string line)
        {
            var match = TitleRegex().Match(line);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private static bool TryParseParticipant(string line, ref int order, out Participant participant)
        {
            participant = new Participant();

            // Match: participant ID as Alias
            // or: participant ID
            // or: actor ID as Alias
            // or: actor ID
            var isActor = line.StartsWith("actor", StringComparison.OrdinalIgnoreCase);
            var isParticipant = line.StartsWith("participant", StringComparison.OrdinalIgnoreCase);

            if (!isActor && !isParticipant)
            {
                return false;
            }

            var match = ParticipantRegex().Match(line);
            if (!match.Success)
            {
                return false;
            }

            var id = match.Groups["id"].Value.Trim();
            var alias = match.Groups["alias"].Success ? match.Groups["alias"].Value.Trim() : id;

            participant = new Participant
            {
                Id = id,
                Alias = alias,
                IsActor = isActor,
                Order = order++
            };

            return true;
        }

        private static bool TryParseBlockStart(string line, out BlockType blockType, out string label)
        {
            blockType = BlockType.Alt;
            label = string.Empty;

            var blockKeywords = new Dictionary<string, BlockType>(StringComparer.OrdinalIgnoreCase)
            {
                { "alt", BlockType.Alt },
                { "opt", BlockType.Opt },
                { "loop", BlockType.Loop },
                { "par", BlockType.Par },
                { "critical", BlockType.Critical },
                { "break", BlockType.Break },
                { "rect", BlockType.Rect }
            };

            foreach (var keyword in blockKeywords)
            {
                if (line.StartsWith(keyword.Key, StringComparison.OrdinalIgnoreCase) && 
                    (line.Length == keyword.Key.Length || char.IsWhiteSpace(line[keyword.Key.Length])))
                {
                    blockType = keyword.Value;
                    label = line.Length > keyword.Key.Length ? line[(keyword.Key.Length + 1)..].Trim() : string.Empty;
                    return true;
                }
            }

            return false;
        }

        private static bool TryParseAlternativeBranch(string line, out BlockType branchType, out string label)
        {
            branchType = BlockType.Else;
            label = string.Empty;

            if (line.StartsWith("else", StringComparison.OrdinalIgnoreCase) &&
                (line.Length == 4 || char.IsWhiteSpace(line[4])))
            {
                branchType = BlockType.Else;
                label = line.Length > 4 ? line[5..].Trim() : string.Empty;
                return true;
            }

            if (line.StartsWith("and", StringComparison.OrdinalIgnoreCase) &&
                (line.Length == 3 || char.IsWhiteSpace(line[3])))
            {
                branchType = BlockType.And;
                label = line.Length > 3 ? line[4..].Trim() : string.Empty;
                return true;
            }

            return false;
        }

        private static bool TryParseMessage(string line, Dictionary<string, Participant> participants, 
            ref int participantOrder, out Message message)
        {
            message = new Message();

            var match = MessageRegex().Match(line);
            if (!match.Success)
            {
                return false;
            }

            var fromId = match.Groups["from"].Value.Trim();
            var arrow = match.Groups["arrow"].Value;
            var toId = match.Groups["to"].Value.Trim();
            var content = match.Groups["content"].Success ? match.Groups["content"].Value.Trim() : string.Empty;

            // Get or create participants
            if (!participants.TryGetValue(fromId, out var fromParticipant))
            {
                fromParticipant = new Participant { Id = fromId, Alias = fromId, Order = participantOrder++ };
                participants[fromId] = fromParticipant;
            }

            if (!participants.TryGetValue(toId, out var toParticipant))
            {
                toParticipant = new Participant { Id = toId, Alias = toId, Order = participantOrder++ };
                participants[toId] = toParticipant;
            }

            message = new Message
            {
                From = fromParticipant,
                To = toParticipant,
                Content = content,
                Type = ParseMessageType(arrow)
            };

            return true;
        }

        private static MessageType ParseMessageType(string arrow)
        {
            return arrow switch
            {
                "->>" => MessageType.SyncRequest,
                "->>+" => MessageType.SyncRequestWithArrowhead,
                "->>-" => MessageType.SyncRequestDeactivate,
                "-->>" => MessageType.AsyncRequest,
                "-->>+" => MessageType.AsyncRequestWithArrowhead,
                "-->>-" => MessageType.AsyncRequestDeactivate,
                "->" => MessageType.SolidLine,
                "-->" => MessageType.DottedLine,
                "-x" => MessageType.SolidLineWithCross,
                "--x" => MessageType.DottedLineWithCross,
                "-)" => MessageType.SolidLineWithOpenArrow,
                "--)" => MessageType.DottedLineWithOpenArrow,
                _ => MessageType.SyncRequest
            };
        }

        // Regex patterns using source generators for better performance
        [GeneratedRegex(@"\\begin\{verbatim\}([\s\S]*?)\\end\{verbatim\}", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
        private static partial Regex VerbatimBlockRegex();

        [GeneratedRegex(@"sequenceDiagram\s*([\s\S]*?)(?=\bend\b)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
        private static partial Regex SequenceDiagramRegex();

        [GeneratedRegex(@"^title\s*[:\s]*(.+)$", RegexOptions.IgnoreCase)]
        private static partial Regex TitleRegex();

        [GeneratedRegex(@"^(?:participant|actor)\s+(?<id>\w+)(?:\s+as\s+(?<alias>.+))?$", RegexOptions.IgnoreCase)]
        private static partial Regex ParticipantRegex();

        [GeneratedRegex(@"^(?<from>\w+)\s*(?<arrow>->>[\+\-]?|-->>[\+\-]?|->|-->|-x|--x|-\)|--\))\s*(?<to>\w+)\s*(?::\s*(?<content>.*))?$", RegexOptions.IgnoreCase)]
        private static partial Regex MessageRegex();
    }
}
