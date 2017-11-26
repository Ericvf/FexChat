using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using AnimationExtensions;
using RaboChat.Common;

namespace RaboChat.WpfClient
{
    [Export]
    public class MainViewModel : ViewModelBase
    {
        static readonly string[] extensions = new[] { "bmp", "png", "jpg", "gif" };
        static HttpClient httpClient = new HttpClient();

        [Import]
        public ChatClient ChatClient { get; set; }

        private async Task Send()
        {
            await ChatClient.WriteText(Text);
            Text = string.Empty;
        }

        private async Task Login()
        {
            ChatClient.ChatMessage += ChatClient_ChatMessage;

            await ChatClient.Start(Alias ?? UserName);

            Title += $" ({UserName})";

            await LoginAnimation.PlayAsync();

            IsLoggedIn = true;
        }

        public async Task<bool> Paste()
        {
            var clipboardData = Clipboard.GetDataObject();
            if (clipboardData != null)
            {
                if (clipboardData.GetDataPresent(DataFormats.Bitmap))
                {
                    var image = (Image)clipboardData.GetData(DataFormats.Bitmap, true);
                    using (var ms = new MemoryStream())
                    {
                        image.Save(ms, ImageFormat.Png);
                        await ChatClient.WriteImage(ms.ToArray());
                        return true;
                    }
                }
                else if (clipboardData.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = (string[])clipboardData.GetData(DataFormats.FileDrop);
                    var imageFiles = files.Where(f => extensions.Any(e => f.ToLower().EndsWith(e)));
                    foreach (var imageFile in imageFiles)
                    {
                        var imageBytes = File.ReadAllBytes(imageFile);
                        await ChatClient.WriteImage(imageBytes);
                        return true;
                    }
                }
                //else if (clipboardData.GetDataPresent(DataFormats.StringFormat))
                //{
                //    var clipboardText = (string)clipboardData.GetData(DataFormats.StringFormat);
                //    try
                //    {
                //        var result = await httpClient.GetAsync(clipboardText);
                //        result.EnsureSuccessStatusCode();

                //        if (result.Content.Headers.ContentType.MediaType.Contains("image"))
                //        {
                //            var contentBytes = await result.Content.ReadAsByteArrayAsync();
                //            await ChatClient.WriteImage(contentBytes);
                //            return true;
                //        }
                //    }
                //    catch
                //    {

                //    }
                //}
            }

            return false;
        }

        public bool CanPaste()
        {
            var clipboardData = Clipboard.GetDataObject();
            if (clipboardData != null)
            {
                return clipboardData.GetDataPresent(DataFormats.Bitmap)
                    || clipboardData.GetDataPresent(DataFormats.FileDrop)
                    || clipboardData.GetDataPresent(DataFormats.StringFormat);
            }

            return false;
        }

        public Animation LoginAnimation { get; set; }

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

        private ICommand _PasteCommand;

        public ICommand PasteCommand
        {
            get
            {
                if (_PasteCommand == null)
                {
                    _PasteCommand = new RelayCommand(
                        async param => await Paste(),
                        p => CanPaste()
                         
                    );
                }

                return _PasteCommand;
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

        public ObservableCollection<MessageModel> Lines
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
        private ObservableCollection<MessageModel> _Lines = new ObservableCollection<MessageModel>();
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
        private string _UserName = Environment.UserName;
        public const string UserNamePropertyName = "UserName";

        public string Alias
        {
            get { return _Alias; }
            set
            {
                if (_Alias != value)
                {
                    _Alias = value;
                    OnPropertyChanged(AliasPropertyName);
                }
            }
        }
        private string _Alias;
        public const string AliasPropertyName = "Alias";

        public System.Windows.Window Window
        {
            get { return _Window; }
            set
            {
                if (_Window != value)
                {
                    _Window = value;
                    OnPropertyChanged(WindowPropertyName);
                }
            }
        }
        private System.Windows.Window _Window;
        public const string WindowPropertyName = "Window";

        #endregion
    }
}
