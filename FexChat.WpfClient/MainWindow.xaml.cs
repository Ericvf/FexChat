using System.ComponentModel.Composition;
using System.Media;
using System.Windows;
using System.Windows.Interop;
using AnimationExtensions;
using FexChat.Common;

namespace FexChat.WpfClient
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

            MainViewModel.LoginAnimation = LoginBox
                .Move(y: 100, duration: 750, eq: Eq.InBack)
                .Fade(0, duration: 750)
                .ThenDo((e) => LoginBox.Visibility = Visibility.Collapsed)
                .New();

            MainViewModel.ChatClient.ChatMessage += ChatClient_ChatMessage;
        }

        private void ChatClient_ChatMessage(object sender, ChatClient.ChatMessageEventArgs e)
        {
            Dispatcher.Invoke(() => {
                if (WindowState == WindowState.Minimized)
                {
                    var windowHandle = new WindowInteropHelper(this).Handle;
                    FlashWindow.Flash(windowHandle, 5);
                }
            });
        }

        private async void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.V && e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                if (MainViewModel.CanPaste())
                {
                    var hasPasted = await MainViewModel.Paste();
                    e.Handled = hasPasted;
                }
            }
        }
    }
}
