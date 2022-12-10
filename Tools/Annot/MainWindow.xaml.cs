using System.IO;
using Microsoft.Extensions.Configuration;
using OpenCvSharp.WpfExtensions;
using Husty.Extensions;
using Husty.OpenCvSharp.DatasetFormat;
using Annot.Utils;
using Annot.Attributes;

namespace Annot;

public partial class MainWindow : System.Windows.Window
{
    public MainWindow()
    {
        InitializeComponent();
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var type = config["Attribute"].ToLower();
        var dir = config["WorkingDirectory"];
        Directory.Exists(dir);
        File.Exists(config["ClassListFile"]);
        int.TryParse(config["StandardLineWidth"], out var standardLineWidth);
        int.TryParse(config["BoldLineWidth"], out var boldLineWidth);
        int.TryParse(config["SelectPixelTolerance"], out var tolerance);
        float.TryParse(config["WheelSpeed"], out var wheelSpeed);
        var navigator = new DirectoryNavigator(dir);
        ClassList.ItemsSource = File.ReadAllLines(Path.Combine(dir, config["ClassListFile"]))
            .Where(line => line is not null && line is not "");
        ClassList.SelectedIndex = 0;
        FileList.ItemsSource = navigator.ImagePaths
            .Select(x => x.Split('\\').LastOrDefault())
            .Where(x => x is not null);
        FileList.SelectedIndex = 0;
        var getRatio = () => (double)Image.ActualWidth / WindowFrame.Width;
        var factory = new AttributeWindowFactory(type, ClassList.Items.Count, standardLineWidth, boldLineWidth, tolerance, getRatio, wheelSpeed);
        var ann = new AnnotationData("sample.json", navigator.ImagePaths, ClassList.Items.Cast<string>());
        IWpfInteractiveWindow window = factory.GetInstance(new[] { ann }, navigator.Current, ClassList.SelectedIndex);
        Image.Source = window.Canvas.ToBitmapSource();
        Image.MouseWheel            += (s, e) => Image.Source = window.InputMouseWheel(e.GetPosition(Image), e.Delta > 0);
        Image.MouseLeftButtonDown   += (s, e) => Image.Source = window.InputLeftMouseDown(e.GetPosition(Image));
        MouseLeftButtonUp           += (s, e) => Image.Source = window.InputLeftMouseUp(e.GetPosition(Image));
        MouseRightButtonUp          += (s, e) => Image.Source = window.InputRightMouseDown(e.GetPosition(Image));
        MouseMove                   += (s, e) => Image.Source = window.InputMouseMove(e.GetPosition(Image));
        MouseLeave                  += (s, e) => Image.Source = window.InputMouseLeave(e.GetPosition(Image));
        ClassList.SelectionChanged  += (s, e) => window.SetLabelIndex(ClassList.SelectedIndex);
        FileList.SelectionChanged   += (s, e) =>
        {
            window.Dispose();
            window = factory.GetInstance(new[] { window.Annotation }, navigator.Move(FileList.SelectedIndex), ClassList.SelectedIndex);
        };
        KeyDown                     += (s, e) =>
        {
            var key = e.Key.ToString().ToLower();
            if (key == config["KeyMap:Back"])
            {
                window.Back();
                window = factory.GetInstance(window.History, navigator.Move(FileList.SelectedIndex), ClassList.SelectedIndex);
            }
            else if (key == config["KeyMap:GoPreviousImage"].ToLower())
            {
                FileList.SelectedIndex = FileList.SelectedIndex.OrAbove(1) - 1;
            }
            else if (key == config["KeyMap:GoNextImage"].ToLower())
            {
                FileList.SelectedIndex = FileList.SelectedIndex.OrBelow(FileList.Items.Count - 1) + 1;
            }
            else if (key == config["KeyMap:DrawActivation"].ToLower())
            {
                window.SetDrawMode(true);
            }
            else if (key == config["KeyMap:DrawInactivation"].ToLower())
            {
                window.SetDrawMode(false);
                window.Cancel();
            }
            else if (key == config["KeyMap:Save"].ToLower())
            {
                File.WriteAllText("sample.json", window.Annotation.ExportAsJson());
            }
            else if (key == config["KeyMap:DeleteLast"].ToLower())
            {
                window.DeleteLast();
            }
            else if (key == config["KeyMap:DeleteSelected"].ToLower())
            {
                window.DeleteSelected();
            }
            else if (key == config["KeyMap:Clear"].ToLower())
            {
                window.Clear();
            }
            Image.Source = window.GetViewImage();
        };
    }
}
