using Avalonia.Controls;

namespace IOSender.Views
{
    public partial class MainWindow : Window
    {
        private const string version = "0.0.1";



        public MainWindow()
        {
            InitializeComponent();

            Title = "IOSender: v" + version;

        }
    }
}