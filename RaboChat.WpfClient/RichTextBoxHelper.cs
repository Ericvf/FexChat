using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace RaboChat.WpfClient
{
    /// <summary>
    /// https://stackoverflow.com/questions/343468/richtextbox-wpf-binding
    /// </summary>
    public class RichTextBoxHelper : DependencyObject
    {
        //public static string GetDocumentXaml(DependencyObject obj)
        //{
        //    return (string)obj.GetValue(DocumentXamlProperty);
        //}

        //public static void SetDocumentXaml(DependencyObject obj, string value)
        //{
        //    obj.SetValue(DocumentXamlProperty, value);
        //}

        //public static readonly DependencyProperty DocumentXamlProperty =
        //  DependencyProperty.RegisterAttached(
        //    "DocumentXaml",
        //    typeof(string),
        //    typeof(RichTextBoxHelper),
        //    new FrameworkPropertyMetadata
        //    {
        //        PropertyChangedCallback = (obj, e) =>
        //        {
        //            var richTextBox = (RichTextBox)obj;

        //            var xaml = GetDocumentXaml(richTextBox);
        //            var doc = richTextBox.Document ?? new FlowDocument();

        //            var range = new TextRange(doc.ContentStart, doc.ContentEnd);
        //            range.Load(new MemoryStream(Encoding.UTF8.GetBytes(xaml)), DataFormats.Text);

        //            richTextBox.Document = doc;
        //        }
        //    });

        public static ObservableCollection<string> GetFlowDocument(DependencyObject obj)
        {
            return (ObservableCollection<string>)obj.GetValue(FlowDocumentProperty);
        }

        public static void SetFlowDocument(DependencyObject obj, ObservableCollection<string> value)
        {
            obj.SetValue(FlowDocumentProperty, value);
        }

        public static readonly DependencyProperty FlowDocumentProperty =
              DependencyProperty.RegisterAttached(
                "FlowDocument",
                typeof(ObservableCollection<string>),
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
                       foreach (var item in args.NewItems)
                       {
                           newItemsLines.AppendLine(item.ToString());
                       }

                       richTextBox.Dispatcher.Invoke(() =>
                       {
                           var p = new Paragraph(new Run(newItemsLines.ToString().Trim()));
                           flowDocument.Blocks.Add(p);
                           richTextBox.ScrollToEnd();
                       });

                       //var textRange = new TextRange(flowDocument.ContentEnd, flowDocument.ContentEnd);
                       //richTextBox.Dispatcher.Invoke(() =>
                       //{
                       //    textRange.Text = newItemsLines.ToString();
                       //    //textRange.Load(new MemoryStream(Encoding.UTF8.GetBytes(newItemsLines.ToString())), DataFormats.Text);
                       //});
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
    }
}
