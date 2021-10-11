using Microsoft.WindowsAPICodePack.Dialogs;
using Reactive.Bindings;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Tools.NnDataArranger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public ReactiveProperty<string> InputButtonContent { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> OutputButtonContent { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> InputLabel { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> OutputLabel { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> Instruction { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> CfgTx { private set; get; } = new ReactiveProperty<string>();

        private string _selectedPath;


        public MainWindow()
        {
            InitializeComponent();
            _selectedPath = "C:";
            DataContext = this;
            InputLabel.Value = "C:";
            OutputLabel.Value = "C:";
            ModeCombo.SelectedIndex = 0;
        }

        private void InputButton_Click(object sender, RoutedEventArgs e)
        {
            using var cofd = new CommonOpenFileDialog()
            {
                Title = "Select Input File / Directory.",
                InitialDirectory = _selectedPath,
                IsFolderPicker = ModeCombo.SelectedIndex is 0 || ModeCombo.SelectedIndex is 1 ? false : true,
            };
            if (cofd.ShowDialog() is CommonFileDialogResult.Ok)
            {
                _selectedPath = Path.GetDirectoryName(cofd.FileName);
                InputLabel.Value = cofd.FileName;
            }
        }

        private void OutputButton_Click(object sender, RoutedEventArgs e)
        {
            using var cofd = new CommonOpenFileDialog()
            {
                Title = "Select Output File / Directory.",
                InitialDirectory = _selectedPath,
                IsFolderPicker = true,
            };
            if (cofd.ShowDialog() is CommonFileDialogResult.Ok)
            {
                _selectedPath = Path.GetDirectoryName(cofd.FileName);
                OutputLabel.Value = cofd.FileName;
            }
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            switch(ModeCombo.SelectedIndex)
            {
                case 0:             // Train-Test Split
                    TTSplit.Run(InputLabel.Value, OutputLabel.Value, double.Parse(CfgTx.Value));
                    break;
                case 1:             // Video to Images
                    var w1 = int.Parse(CfgTx.Value.Split(",")[0]);
                    var h1 = int.Parse(CfgTx.Value.Split(",")[1]);
                    var skip = int.Parse(CfgTx.Value.Split(",")[2]);
                    Vid2Img.Run(InputLabel.Value, OutputLabel.Value, new OpenCvSharp.Size(w1, h1), skip);
                    break;
                case 2:             // Image Resize
                    var w2 = int.Parse(CfgTx.Value.Split(",")[0]);
                    var h2 = int.Parse(CfgTx.Value.Split(",")[1]);
                    ImgResize.Run(InputLabel.Value, OutputLabel.Value, new OpenCvSharp.Size(w2, h2));
                    break;
                case 3:             // Json to Binary Mask Image
                    var w3 = int.Parse(CfgTx.Value.Split(",")[0]);
                    var h3 = int.Parse(CfgTx.Value.Split(",")[1]);
                    var val = int.Parse(CfgTx.Value.Split(",")[2]);
                    J2Mask.Run(InputLabel.Value, OutputLabel.Value, new OpenCvSharp.Size(w3, h3), val);
                    break;
                case 4:             // Image-Mask Assignment
                    ImgAssign.Run(InputLabel.Value, OutputLabel.Value);
                    break;
                case 5:             // Disperse Brightness
                    var rate1 = double.Parse(CfgTx.Value);
                    BrightnessDisperser.Run(InputLabel.Value, OutputLabel.Value, rate1);
                    break;
                case 6:             // Horizonal Flip
                    var rate2 = double.Parse(CfgTx.Value);
                    HorizonalFlipper.Run(InputLabel.Value, OutputLabel.Value, rate2);
                    break;
                case 7:             // Equalize Value
                    var rate3 = double.Parse(CfgTx.Value);
                    ValueEqualizer.Run(InputLabel.Value, OutputLabel.Value, rate3);
                    break;
                case 8:             // Extract Zip
                    Zip2Img.Run(InputLabel.Value, OutputLabel.Value, CfgTx.Value);
                    break;
                case 9:             // for Keras or PyTorch
                    var rate4 = double.Parse(CfgTx.Value);
                    TTSplitFolder.Run(InputLabel.Value, OutputLabel.Value, rate4);
                    break;
            }
        }

        private void ModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (ModeCombo.SelectedIndex)
            {
                case 0:             // Train-Test Split
                    InputButtonContent.Value = "Input File";
                    OutputButtonContent.Value = "Output Dir";
                    Instruction.Value = "Test-Rate";
                    CfgTx.Value = "0.2";
                    break;
                case 1:             // Video to Images
                    InputButtonContent.Value = "Input File";
                    OutputButtonContent.Value = "Output Dir";
                    Instruction.Value = "Size,Skip";
                    CfgTx.Value = "640,480,1";
                    break;
                case 2:             // Image Resize
                    InputButtonContent.Value = "Input Dir";
                    OutputButtonContent.Value = "Output Dir";
                    Instruction.Value = "Size";
                    CfgTx.Value = "640,480";
                    break;
                case 3:             // Json to Binary Mask Image
                    InputButtonContent.Value = "Input Dir";
                    OutputButtonContent.Value = "Output Dir";
                    Instruction.Value = "Size";
                    CfgTx.Value = "640,480,255";
                    break;
                case 4:             // Image-Mask Assignment
                    InputButtonContent.Value = "Image Dir";
                    OutputButtonContent.Value = "Mask Dir";
                    Instruction.Value = "";
                    CfgTx.Value = "";
                    break;
                case 5:             // Disperse Brightness
                    InputButtonContent.Value = "Input Dir";
                    OutputButtonContent.Value = "Mask Dir";
                    Instruction.Value = "Rate";
                    CfgTx.Value = "0.2";
                    break;
                case 6:             // Horizonal Flip
                    InputButtonContent.Value = "Input Dir";
                    OutputButtonContent.Value = "Mask Dir";
                    Instruction.Value = "Rate";
                    CfgTx.Value = "0.2";
                    break;
                case 7:             // Equalize Value
                    InputButtonContent.Value = "Input Dir";
                    OutputButtonContent.Value = "Mask Dir";
                    Instruction.Value = "Rate";
                    CfgTx.Value = "0.2";
                    break;
                case 8:             // Extract Zip
                    InputButtonContent.Value = "Input Dir";
                    OutputButtonContent.Value = "Output Dir";
                    Instruction.Value = "C D P";
                    CfgTx.Value = "C";
                    break;
                case 9:             // for Keras or PyTorch
                    InputButtonContent.Value = "Input Dir";
                    OutputButtonContent.Value = "Mask Dir";
                    Instruction.Value = "Rate";
                    CfgTx.Value = "0.2";
                    break;
            }
        }

    }
}
