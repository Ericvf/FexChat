using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RaboChat.Common;

namespace RaboChat.WpfClient
{
    /// <summary>
    /// https://stackoverflow.com/questions/343468/richtextbox-wpf-binding
    /// </summary>
    public class RichTextBoxHelper : DependencyObject
    {
        public static ObservableCollection<MessageModel> GetFlowDocument(DependencyObject obj)
        {
            return (ObservableCollection<MessageModel>)obj.GetValue(FlowDocumentProperty);
        }

        public static void SetFlowDocument(DependencyObject obj, ObservableCollection<MessageModel> value)
        {
            obj.SetValue(FlowDocumentProperty, value);
        }

        public static readonly DependencyProperty FlowDocumentProperty =
              DependencyProperty.RegisterAttached(
                "FlowDocument",
                typeof(ObservableCollection<MessageModel>),
                typeof(RichTextBoxHelper),
                new UIPropertyMetadata(null, OnFlowDocumentChanged)
        );

        private static void OnFlowDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var richTextBox = (RichTextBox)d;

            var action = new NotifyCollectionChangedEventHandler(
               (o, args) =>
               {
                   if (richTextBox != null)
                   {
                       var flowDocument = richTextBox.Document;

                       var newItemsLines = new StringBuilder();
                       richTextBox.Dispatcher.Invoke(() =>
                       {
                           foreach (MessageModel item in args.NewItems)
                           {
                               var p = new Paragraph();
                               CreateParagraph(item, p);

                               flowDocument.Blocks.Add(p);
                               richTextBox.ScrollToEnd();
                           }
                       });
                   }
               });

            if (e.OldValue != null)
            {
                var coll = (INotifyCollectionChanged)e.OldValue;
                coll.CollectionChanged -= action;
            }

            if (e.NewValue != null)
            {
                var coll = (INotifyCollectionChanged)e.NewValue;
                coll.CollectionChanged += action;
            }
        }

        private static void CreateParagraph(MessageModel message, Paragraph p)
        {
            if (message.Type == Types.Announcement)
            {
                p.FontStyle = FontStyles.Italic;
                p.Foreground = Brushes.DarkGray;
                p.TextAlignment = TextAlignment.Center;
            }
            else 
            {
                var userNameRun = new Run($"{message.UserName} ({DateTime.Now.ToShortTimeString()}):{Environment.NewLine}");
                userNameRun.FontStyle = FontStyles.Italic;
                userNameRun.Foreground = Brushes.Gray;
                p.Inlines.Add(userNameRun);
            }

            if (message.Type == Types.Image)
            {
                var bytes = Convert.FromBase64String(message.Payload);
                var ms = new MemoryStream(bytes);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                var img = new Image()
                {
                    Source = bitmapImage as ImageSource,
                    Stretch = Stretch.None
                };

                var viewBox = new Viewbox()
                {
                    Stretch = Stretch.UniformToFill,
                    StretchDirection = StretchDirection.DownOnly,
                    Child = img
                };

                p.Inlines.Add(viewBox);
                return;
            }

            var splitCharacters = new[] { ' ' };
            var words = Regex.Matches(message.Payload, @"((\s+)?\S+(\s+)?)");

            foreach (Match m in words)
            {
                var item = m.Value.ToString().Trim();

                if (item.StartsWith("http://") || item.StartsWith("https://"))
                {
                    var hyperlink = new Hyperlink();
                    hyperlink.IsEnabled = true;
                    hyperlink.Inlines.Add(m.Value);
                    hyperlink.NavigateUri = new Uri(item);
                    hyperlink.RequestNavigate += (sender, args) => Process.Start(args.Uri.ToString());
                    p.Inlines.Add(hyperlink);
                }
                else
                {
                    p.Inlines.Add(m.Value);
                }
            }
        }
    }
}
