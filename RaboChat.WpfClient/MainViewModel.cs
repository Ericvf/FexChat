using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using RaboChat.Common;

namespace RaboChat.WpfClient
{
    [Export]
    public class MainViewModel : ViewModelBase
    {
        [Import]
        public ChatClient ChatClient { get; set; }

        private async Task Send()
        {
            var message = Text;
            Text = string.Empty;

            await ChatClient.WriteMessage(message);
        }

        private async Task Login()
        {
            ChatClient.ChatMessage += ChatClient_ChatMessage;

            await ChatClient.Start(UserName);

            Title += $" ({UserName})";
            IsLoggedIn = true;
        }

        private void ChatClient_ChatMessage(object sender, ChatClient.ChatMessageEventArgs e)
        {
            Lines.Add(e.Message);
        }

        #region Commands

        private ICommand _SendCommand;

        public ICommand SendCommand
        {
            get
            {
                if (_SendCommand == null)
                {
                    _SendCommand = new RelayCommand(
                        async param => await Send()
                    );
                }

                return _SendCommand;
            }
        }


        private ICommand _loginCommand;

        public ICommand LoginCommand
        {
            get
            {
                if (_loginCommand == null)
                {
                    _loginCommand = new RelayCommand(
                        async param => await Login()
                    );
                }

                return _loginCommand;
            }
        }

        #endregion

        #region Properties

        public string Title
        {
            get { return _Title; }
            set
            {
                if (_Title != value)
                {
                    _Title = value;
                    OnPropertyChanged(TitlePropertyName);
                }
            }
        }
        private string _Title = "RaboChat";
        public const string TitlePropertyName = "Title";

        public ObservableCollection<string> Lines
        {
            get { return _Lines; }
            set
            {
                if (_Lines != value)
                {
                    _Lines = value;
                    OnPropertyChanged(LinesPropertyName);
                }
            }
        }
        private ObservableCollection<string> _Lines = new ObservableCollection<string>();
        public const string LinesPropertyName = "Lines";

        public string Text
        {
            get { return _Text; }
            set
            {
                if (_Text != value)
                {
                    _Text = value;
                    OnPropertyChanged(TextPropertyName);
                }
            }
        }
        private string _Text;
        public const string TextPropertyName = "Text";

        public bool IsLoggedIn
        {
            get { return _IsLoggedIn; }
            set
            {
                if (_IsLoggedIn != value)
                {
                    _IsLoggedIn = value;
                    OnPropertyChanged(IsLoggedInPropertyName);
                }
            }
        }
        private bool _IsLoggedIn;
        public const string IsLoggedInPropertyName = "IsLoggedIn";

        public string UserName
        {
            get { return _UserName; }
            set
            {
                if (_UserName != value)
                {
                    _UserName = value;
                    OnPropertyChanged(UserNamePropertyName);
                }
            }
        }
        private string _UserName;
        public const string UserNamePropertyName = "UserName";

        #endregion
    }
}
