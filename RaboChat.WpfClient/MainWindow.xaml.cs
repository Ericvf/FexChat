using RaboChat.Common;
using System.ComponentModel.Composition;
using System.Windows;

namespace RaboChat.WpfClient
{
    public partial class MainWindow : Window
    {
        [Import]
        public MainViewModel MainViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            var assemblyTypes = new[] {
                typeof(MainViewModel),
                typeof(CompositionHelpers)
            };

            CompositionHelpers.InitializeComposition(assemblyTypes, this);

            DataContext = MainViewModel;
        }
    }
}
