using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using AnimationExtensions;
using RaboChat.Common;

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

            var elements = LoginBox.Children.Cast<FrameworkElement>();
            MainViewModel.MyAnim = 
                elements.For((i, e) => e
                  .Wait(i * 100)
                  .Move(y: 50, duration: 750, eq: Eq.InBack)
                  .Fade(0, duration: 750)
              );
        }
    }
}
