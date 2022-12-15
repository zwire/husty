using System.IO;
using System.Text.Json;
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
        if (dir is "") dir = Directory.GetCurrentDirectory();
        if (!Directory.Exists(dir))
            throw new DirectoryNotFoundException(nameof(dir));
        var classPath = config["ClassFilePath"];
        if (!File.Exists(classPath))
        {
            classPath = Path.Combine(dir, classPath);
            if (!File.Exists(classPath))
                throw new FileNotFoundException(nameof(classPath));
        }
        var outputPath = config["OutputFilePath"];
        if (!File.Exists(outputPath))
        {
            var outputPath2 = Path.Combine(dir, outputPath);
            if (File.Exists(outputPath2))
                outputPath = outputPath2;
        }
        if (!int.TryParse(config["StandardLineWidth"], out int standardLineWidth) ||
            !int.TryParse(config["BoldLineWidth"], out int boldLineWidth) ||
            !int.TryParse(config["SelectPixelTolerance"], out int tolerance) ||
            !float.TryParse(config["WheelSpeed"], out float wheelSpeed)
        ) throw new Exception();
        var navigator = new DirectoryNavigator(dir);
        ClassList.ItemsSource = File.ReadAllLines(classPath)
            .Where(line => line is not null && line is not "")
            .Select(x => " " + x);
        ClassList.SelectedIndex = 0;
        FileList.ItemsSource = navigator.ImagePaths
            .Select(x => x.Split('\\').LastOrDefault())
            .Where(x => x is not null)
            .Select(x => " " + x);
        FileList.SelectedIndex = 0;
        var getRatio = () => (double)Image.ActualWidth / WindowFrame.Width;
        var factory = new AttributeWindowFactory(type, standardLineWidth, boldLineWidth, tolerance, getRatio, wheelSpeed);
        var ann = new AnnotationData(outputPath, navigator.ImagePaths, ClassList.Items.Cast<string>().Select(x => x.TrimStart(' ')));
        IWpfInteractiveWindow window = factory.GetInstance(new[] { ann }, navigator.Current, ClassList.SelectedIndex);
        var preferenceColorPath = config["PreferenceColorFilePath"];
        if (preferenceColorPath is not null)
        {
            if (!File.Exists(preferenceColorPath))
            {
                var preferenceColorPath2 = Path.Combine(dir, preferenceColorPath);
                if (File.Exists(preferenceColorPath2))
                    preferenceColorPath = preferenceColorPath2;
                else
                    throw new FileNotFoundException(nameof(preferenceColorPath));
            }
            var colors = JsonSerializer.Deserialize<int[][]>(File.ReadAllText(preferenceColorPath));
            if (!colors.Any()) 
                throw new Exception();
            window.SetColors(colors);
        }
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
            if (key == config["KeyMap:Back"].ToLower())
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
                File.WriteAllText(outputPath, window.Annotation.ExportAsJson());
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
            else
            {
                window.AcceptOtherKeyInput(key.ToLower());
            }
            Image.Source = window.GetViewImage();
        };
    }
}
