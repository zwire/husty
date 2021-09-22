using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenCvSharp.WpfExtensions;

namespace Tools.Yolo_Labeling
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool _mouseMove;
        private string _openDir = "C:";
        private string _saveDir = "C:";
        private readonly string[] _labels;

        public MainWindow()
        {
            InitializeComponent();
            BackButton.IsEnabled = false;
            NextButton.IsEnabled = false;
            SaveButton.IsEnabled = false;
            UndoButton.IsEnabled = false;
            ClearButton.IsEnabled = false;
            _mouseMove = false;
            var labelPath = $"..\\..\\..\\classes.txt";
            _labels = File.ReadAllLines(labelPath).ToArray();
            foreach (var l in _labels)
                ClassCombo.Items.Add(l);
            ClassCombo.Text = _labels[0];
            if (File.Exists("cache.txt"))
            {
                var lines = File.ReadAllText("cache.txt").Split("\n");
                if (lines.Length > 0)
                {
                    _openDir = lines[0].TrimEnd();
                    _saveDir = lines[1].TrimEnd();
                }
            }
            Closed += (s, e) =>
            {
                using var sw = new StreamWriter("cache.txt", false);
                sw.WriteLine(_openDir);
                sw.WriteLine(_saveDir);
            };
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            using var cofd1 = new CommonOpenFileDialog()
            {
                Title = "画像フォルダ選択",
                InitialDirectory = _openDir,
                IsFolderPicker = true
            };
            if (cofd1.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _openDir = cofd1.FileName;
                using var cofd2 = new CommonOpenFileDialog()
                {
                    Title = "保存先フォルダ選択",
                    InitialDirectory = _saveDir,
                    IsFolderPicker = true
                };
                if (cofd2.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    _saveDir = cofd2.FileName;
                    var img = Process.Initialize(cofd1.FileName, cofd2.FileName, _labels);
                    Image.Width = img.Width;
                    Image.Height = img.Height;
                    Image.Source = img.ToBitmapSource();
                    Image.IsEnabled = true;
                    ProgressLabel.Content = $"{Process.FrameNumber + 1} / {Process.FileCount}";
                    BackButton.IsEnabled = false;
                    NextButton.IsEnabled = false;
                    SaveButton.IsEnabled = false;
                    UndoButton.IsEnabled = false;
                    ClearButton.IsEnabled = false;
                    if (Process.FrameNumber != Process.FileCount - 1) NextButton.IsEnabled = true;
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Process.GoBack().ToBitmapSource();
            ProgressLabel.Content = $"{Process.FrameNumber + 1} / {Process.FileCount}";
            NextButton.IsEnabled = true;
            if (Process.FrameNumber == 0) BackButton.IsEnabled = false;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Process.GoNext().ToBitmapSource();
            ProgressLabel.Content = $"{Process.FrameNumber + 1} / {Process.FileCount}";
            BackButton.IsEnabled = true;
            if (Process.FrameNumber == Process.FileCount - 1) NextButton.IsEnabled = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Save();
            SaveButton.IsEnabled = false;
            if (Process.FrameNumber != 0) BackButton.IsEnabled = true;
            if (Process.FrameNumber != Process.FileCount - 1) NextButton.IsEnabled = true;
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Process.RemoveLast().ToBitmapSource();
            SaveButton.IsEnabled = true;
            if (Process.RectCount == 0)
            {
                UndoButton.IsEnabled = false;
                ClearButton.IsEnabled = false;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = Process.Clear().ToBitmapSource();
            UndoButton.IsEnabled = false;
            ClearButton.IsEnabled = false;
            SaveButton.IsEnabled = false;
        }

        private void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseMove = true;
            var p = e.GetPosition(Image);
            Image.Source = Process.SelectStart((int)p.X, (int)p.Y).ToBitmapSource();
            BackButton.IsEnabled = false;
            NextButton.IsEnabled = false;
        }

        private void Image_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_mouseMove) return;
            _mouseMove = false;
            var index = ClassCombo.SelectedIndex;
            if (index == -1) index = 0;
            var p = e.GetPosition(Image);
            Image.Source = Process.SelectGoal((int)p.X, (int)p.Y, index).ToBitmapSource();
            UndoButton.IsEnabled = true;
            ClearButton.IsEnabled = true;
            SaveButton.IsEnabled = true;
        }

        private void Image_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseMove)
            {
                var p = e.GetPosition(Image);
                Image.Source = Process.Drag((int)p.X, (int)p.Y).ToBitmapSource();
            }
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                    if (BackButton.IsEnabled == false) break;
                    Image.Source = Process.GoBack().ToBitmapSource();
                    ProgressLabel.Content = $"{Process.FrameNumber + 1} / {Process.FileCount}";
                    NextButton.IsEnabled = true;
                    if (Process.FrameNumber == 0) BackButton.IsEnabled = false;
                    break;
                case Key.D:
                    if (NextButton.IsEnabled == false) break;
                    Image.Source = Process.GoNext().ToBitmapSource();
                    ProgressLabel.Content = $"{Process.FrameNumber + 1} / {Process.FileCount}";
                    BackButton.IsEnabled = true;
                    if (Process.FrameNumber == Process.FileCount - 1) NextButton.IsEnabled = false;
                    break;
                case Key.S:
                    if (SaveButton.IsEnabled == false) break;
                    Process.Save();
                    SaveButton.IsEnabled = false;
                    if (Process.FrameNumber != 0) BackButton.IsEnabled = true;
                    if (Process.FrameNumber != Process.FileCount - 1) NextButton.IsEnabled = true;
                    break;
                case Key.C:
                    if (ClearButton.IsEnabled == false) break;
                    Image.Source = Process.Clear().ToBitmapSource();
                    UndoButton.IsEnabled = false;
                    ClearButton.IsEnabled = false;
                    SaveButton.IsEnabled = false;
                    break;
                case Key.X:
                    if (UndoButton.IsEnabled == false) break;
                    Image.Source = Process.RemoveLast().ToBitmapSource();
                    SaveButton.IsEnabled = true;
                    if (Process.RectCount == 0)
                    {
                        UndoButton.IsEnabled = false;
                        ClearButton.IsEnabled = false;
                    }
                    break;
            }
        }

        private void ClassCombo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}
