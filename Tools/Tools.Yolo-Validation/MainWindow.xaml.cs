using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Husty;

namespace Tools.Yolo_Validation
{

    internal record Preset(
        string ModelDir = "C:", 
        string ImageDir = "C:", 
        string LabelDir = "C:", 
        int Width = 640,
        int Height = 480,
        double Probthresh = 0.25,
        double IouThresh = 0.5
    );

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            var settings = new Preset<Preset>(new());
            var val = settings.Load();
            ModelDirButton.Content = val.ModelDir;
            ImageDirButton.Content = val.ImageDir;
            LabelDirButton.Content = val.LabelDir;
            WidthTx.Text = val.Width.ToString();
            HeightTx.Text = val.Height.ToString();
            ProbTx.Text = val.Probthresh.ToString();
            IouTx.Text = val.IouThresh.ToString();
        }

        private void ModelDirButton_Click(object sender, RoutedEventArgs e)
        {
            using var cofd = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください",
                InitialDirectory = ModelDirButton.Content.ToString(),
                IsFolderPicker = true,
            };
            if (cofd.ShowDialog() is CommonFileDialogResult.Ok)
            {
                ModelDirButton.Content = cofd.FileName;
            }
        }

        private void ImageDirButton_Click(object sender, RoutedEventArgs e)
        {
            using var cofd = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください",
                InitialDirectory = ImageDirButton.Content.ToString(),
                IsFolderPicker = true,
            };
            if (cofd.ShowDialog() is CommonFileDialogResult.Ok)
            {
                ImageDirButton.Content = cofd.FileName;
            }
        }

        private void LabelDirButton_Click(object sender, RoutedEventArgs e)
        {
            using var cofd = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください",
                InitialDirectory = LabelDirButton.Content.ToString(),
                IsFolderPicker = true,
            };
            if (cofd.ShowDialog() is CommonFileDialogResult.Ok)
            {
                LabelDirButton.Content = cofd.FileName;
            }
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            ResultLabel.Content = "Result";
            var modelFolder = (string)ModelDirButton.Content;
            var imgDir = (string)ImageDirButton.Content;
            var labDir = (string)LabelDirButton.Content;
            var width = int.Parse(WidthTx.Text);
            var height = int.Parse(HeightTx.Text);
            var classNum = int.Parse(ClassNumTx.Text);
            var probThresh = double.Parse(ProbTx.Text);
            var iouThresh = double.Parse(IouTx.Text);
            var r = new BackgroundProcess(modelFolder, width, height, classNum, probThresh, iouThresh, imgDir, labDir).Go();
            ResultLabel.Content = $"ClassName : {r.ClassName}\n";
            ResultLabel.Content += $"AverageTime : {r.AvgTime}\n";
            ResultLabel.Content += $"TP : {r.Tp}\n";
            ResultLabel.Content += $"FP : {r.Fp}\n";
            ResultLabel.Content += $"FN : {r.Fn}\n";
            ResultLabel.Content += $"Precision : {r.Precision:f3}\n";
            ResultLabel.Content += $"Recall : {r.Recall:f3}\n";
            ResultLabel.Content += $"AP : {r.Ap:f3}\n";
        }

    }
}
