namespace CanAnalyzer;

public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
{
    public MainWindow()
    {
        InitializeComponent();
        var vm = new MainWindowViewModel();
        DataContext = vm;
        Closed += (s, e) => vm.Dispose();
    }
}
