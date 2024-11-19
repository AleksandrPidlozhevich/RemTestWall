using Autodesk.Revit.UI;
using RemTestWall.ViewModels;
using System.Windows;

namespace RemTestWall.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(UIApplication app)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(app);
        }
    }
}
