using SequenceDiagramEvaluation.Parser;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using SequenceDiagramEvaluation.Core;

namespace SequenceDiagramEvaluation.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ISequenceDiagramParser _parser;
        private List<SequenceDiagram>? _currentDiagrams;
        private string? _currentFilePath;

        public MainWindow()
        {
            InitializeComponent();
            _parser = new LatexMermaidSequenceDiagramParser();
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "ZIP files (*.zip)|*.zip|All files (*.*)|*.*",
                Title = "Select LaTeX ZIP File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _currentFilePath = openFileDialog.FileName;
                SelectedFileText.Text = Path.GetFileName(_currentFilePath);
                LoadAndProcessFile();
            }
        }

        private void SplitMessagesCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_currentDiagrams != null)
            {
                ProcessDiagrams();
            }
        }

        private void LoadAndProcessFile()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
                return;

            try
            {
                using var fileStream = File.OpenRead(_currentFilePath);
                _currentDiagrams = _parser.Parse(fileStream).ToList();

                if (_currentDiagrams.Count == 0)
                {
                    MessageBox.Show("No sequence diagrams found in the selected file.", 
                        "No Diagrams Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ClearResults();
                    return;
                }

                ProcessDiagrams();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing file: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ClearResults();
            }
        }

        private void ProcessDiagrams()
        {
            if (_currentDiagrams == null)
                return;

            var splitMessages = SplitMessagesCheckBox.IsChecked == true;
            var metricsResults = new List<DiagramMetricsViewModel>();
            var ratingsResults = new List<DiagramRatingsViewModel>();

            for (int i = 0; i < _currentDiagrams.Count; i++)
            {
                var diagram = _currentDiagrams[i];
                var metrics = new SequenceDiagramMetrics(diagram);
                var metricsResult = metrics.GetAllMetrics();
                var diagramName = !string.IsNullOrEmpty(diagram.Title) ? diagram.Title : $"Diagram {i + 1}";

                // Check if this diagram has only main block (FirstLevelBlockCount == 1 means only main block)
                bool hasOnlyMainBlock = metricsResult.FirstLevelBlockCount == 1;
                bool shouldSplit = splitMessages && hasOnlyMainBlock && metricsResult.MainBlockMessageScore > 0;

                if (shouldSplit)
                {
                    // Split main messages and average messages in two parts
                    int splitMainMessages = (int)Math.Ceiling(metricsResult.MainBlockMessageScore / 2.0);
                    double splitAvgMessages = metricsResult.MainBlockMessageScore / 2.0;

                    // Create metrics view model with split values
                    var metricsViewModel = new DiagramMetricsViewModel
                    {
                        DiagramName = diagramName,
                        ParticipantScore = metricsResult.ParticipantScore,
                        MainBlockMessageScore = splitMainMessages,
                        FirstLevelBlockCount = metricsResult.FirstLevelBlockCount,
                        AverageMessagesInFirstLevelBlocks = splitAvgMessages,
                        IsSplitRow = true
                    };
                    metricsResults.Add(metricsViewModel);

                    // Create modified metrics for evaluation
                    var modifiedMetrics = new DiagramMetricsResult
                    {
                        ParticipantScore = metricsResult.ParticipantScore,
                        MainBlockMessageScore = splitMainMessages,
                        FirstLevelBlockCount = metricsResult.FirstLevelBlockCount,
                        AverageMessagesInFirstLevelBlocks = splitAvgMessages
                    };

                    var evaluation = SequenceDiagramEvaluator.Evaluate(modifiedMetrics);
                    var ratingsViewModel = new DiagramRatingsViewModel
                    {
                        DiagramName = diagramName,
                        ObjectsRating = evaluation.ObjectsRating,
                        MainBlockMessagesRating = evaluation.MainBlockMessagesRating,
                        BlocksRating = evaluation.BlocksRating,
                        AverageMessagesRating = evaluation.AverageMessagesRating,
                        OverallAverageRating = evaluation.OverallAverageRating,
                        IsSplitRow = true
                    };
                    ratingsResults.Add(ratingsViewModel);
                }
                else
                {
                    // Create metrics view model (no split)
                    var metricsViewModel = new DiagramMetricsViewModel
                    {
                        DiagramName = diagramName,
                        ParticipantScore = metricsResult.ParticipantScore,
                        MainBlockMessageScore = metricsResult.MainBlockMessageScore,
                        FirstLevelBlockCount = metricsResult.FirstLevelBlockCount,
                        AverageMessagesInFirstLevelBlocks = metricsResult.AverageMessagesInFirstLevelBlocks,
                        IsSplitRow = false
                    };
                    metricsResults.Add(metricsViewModel);

                    // Create ratings view model
                    var evaluation = SequenceDiagramEvaluator.Evaluate(metricsResult);
                    var ratingsViewModel = new DiagramRatingsViewModel
                    {
                        DiagramName = diagramName,
                        ObjectsRating = evaluation.ObjectsRating,
                        MainBlockMessagesRating = evaluation.MainBlockMessagesRating,
                        BlocksRating = evaluation.BlocksRating,
                        AverageMessagesRating = evaluation.AverageMessagesRating,
                        OverallAverageRating = evaluation.OverallAverageRating,
                        IsSplitRow = false
                    };
                    ratingsResults.Add(ratingsViewModel);
                }
            }

            ResultsGrid.ItemsSource = metricsResults;
            RatingsGrid.ItemsSource = ratingsResults;
        }

        private void ClearResults()
        {
            _currentDiagrams = null;
            ResultsGrid.ItemsSource = null;
            RatingsGrid.ItemsSource = null;
        }
    }

    /// <summary>
    /// View model for displaying diagram metrics in the DataGrid.
    /// </summary>
    public class DiagramMetricsViewModel
    {
        public string DiagramName { get; set; } = string.Empty;
        public int ParticipantScore { get; set; }
        public int MainBlockMessageScore { get; set; }
        public int FirstLevelBlockCount { get; set; }
        public double AverageMessagesInFirstLevelBlocks { get; set; }
        public string AverageMessagesFormatted => AverageMessagesInFirstLevelBlocks.ToString("F2");
        public bool IsSplitRow { get; set; }
    }

    /// <summary>
    /// View model for displaying diagram ratings in the DataGrid.
    /// </summary>
    public class DiagramRatingsViewModel
    {
        public string DiagramName { get; set; } = string.Empty;
        public int ObjectsRating { get; set; }
        public int MainBlockMessagesRating { get; set; }
        public int BlocksRating { get; set; }
        public int AverageMessagesRating { get; set; }
        public double OverallAverageRating { get; set; }
        public string OverallAverageFormatted => OverallAverageRating.ToString("F2");
        public bool IsSplitRow { get; set; }

        /// <summary>
        /// Gets the scale based on the overall average rating.
        /// </summary>
        public string Scale => OverallAverageRating switch
        {
            >= 9.1 => "Optimal",
            >= 7.6 => "High",
            >= 5.1 => "Medium",
            _ => "Low"
        };
    }
}