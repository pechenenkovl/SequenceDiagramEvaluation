namespace SequenceDiagramEvaluation.Core
{
    /// <summary>
    /// Provides evaluation/rating calculation for sequence diagram metrics.
    /// </summary>
    public static class SequenceDiagramEvaluator
    {
        /// <summary>
        /// Rating table for number of objects, messages in main block, and number of blocks.
        /// Key is the count, value is the rating (1-10).
        /// </summary>
        private static readonly Dictionary<int, int> CountRatingTable = new()
        {
            { 0, 0 },
            { 1, 1 },
            { 2, 7 },
            { 3, 8 },
            { 4, 9 },
            { 5, 10 },
            { 6, 10 },
            { 7, 10 },
            { 8, 9 },
            { 9, 8 },
            { 10, 7 },
            { 11, 6 },
            { 12, 5 },
            { 13, 4 },
            { 14, 3 },
            { 15, 2 }
        };

        /// <summary>
        /// Rating table for average messages within blocks.
        /// Key is the count (rounded), value is the rating (1-10).
        /// </summary>
        private static readonly Dictionary<int, int> AverageMessagesRatingTable = new()
        {
            { 0, 0 },
            { 1, 7 },
            { 2, 7 },
            { 3, 8 },
            { 4, 9 },
            { 5, 10 },
            { 6, 10 },
            { 7, 10 },
            { 8, 10 },
            { 9, 10 },
            { 10, 9 },
            { 11, 8 },
            { 12, 7 },
            { 13, 6 },
            { 14, 5 },
            { 15, 4 },
            { 16, 3 },
            { 17, 2 }
        };

        /// <summary>
        /// Gets the rating for a count value (objects, messages in main, or blocks).
        /// </summary>
        /// <param name="count">The count to evaluate.</param>
        /// <returns>Rating from 1 to 10.</returns>
        public static int GetCountRating(int count)
        {
            if (count < 0)
                return 0;

            if (CountRatingTable.TryGetValue(count, out var rating))
                return rating;

            // For counts >= 16
            return 1;
        }

        /// <summary>
        /// Gets the rating for average messages within blocks.
        /// </summary>
        /// <param name="average">The average to evaluate.</param>
        /// <returns>Rating from 1 to 10.</returns>
        public static int GetAverageMessagesRating(double average)
        {
            // Round to nearest integer for lookup
            var rounded = (int)Math.Round(average);

            if (rounded < 0)
                return 0;

            if (AverageMessagesRatingTable.TryGetValue(rounded, out var rating))
                return rating;

            // For averages >= 18
            return 1;
        }

        /// <summary>
        /// Evaluates all metrics and returns ratings.
        /// </summary>
        /// <param name="metrics">The metrics to evaluate.</param>
        /// <returns>Evaluation result with all ratings.</returns>
        public static DiagramEvaluationResult Evaluate(DiagramMetricsResult metrics)
        {
            var objectsRating = GetCountRating(metrics.ParticipantScore);
            var mainBlockMessagesRating = GetCountRating(metrics.MainBlockMessageScore);
            var blocksRating = GetCountRating(metrics.FirstLevelBlockCount);
            var avgMessagesRating = GetAverageMessagesRating(metrics.AverageMessagesInFirstLevelBlocks);

            var averageRating = (objectsRating + mainBlockMessagesRating + blocksRating + avgMessagesRating) / 4.0;

            return new DiagramEvaluationResult
            {
                ObjectsRating = objectsRating,
                MainBlockMessagesRating = mainBlockMessagesRating,
                BlocksRating = blocksRating,
                AverageMessagesRating = avgMessagesRating,
                OverallAverageRating = averageRating
            };
        }
    }

    /// <summary>
    /// Represents the evaluation/rating result for a sequence diagram.
    /// </summary>
    public class DiagramEvaluationResult
    {
        /// <summary>
        /// Gets or sets the rating for number of objects (1-10).
        /// </summary>
        public int ObjectsRating { get; set; }

        /// <summary>
        /// Gets or sets the rating for number of messages in main block (1-10).
        /// </summary>
        public int MainBlockMessagesRating { get; set; }

        /// <summary>
        /// Gets or sets the rating for number of blocks (1-10).
        /// </summary>
        public int BlocksRating { get; set; }

        /// <summary>
        /// Gets or sets the rating for average messages within blocks (1-10).
        /// </summary>
        public int AverageMessagesRating { get; set; }

        /// <summary>
        /// Gets or sets the overall average rating (sum of 4 ratings divided by 4).
        /// </summary>
        public double OverallAverageRating { get; set; }

        public override string ToString()
        {
            return $"Objects: {ObjectsRating}, Main Messages: {MainBlockMessagesRating}, " +
                   $"Blocks: {BlocksRating}, Avg Messages: {AverageMessagesRating}, " +
                   $"Overall: {OverallAverageRating:F2}";
        }
    }
}
