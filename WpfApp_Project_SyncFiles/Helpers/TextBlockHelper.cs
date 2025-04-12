using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WpfApp_Project_SyncFiles.Helpers
{
    public static class TextBlockHelper
    {
        public static readonly DependencyProperty BindableInlinesProperty =
            DependencyProperty.RegisterAttached("BindableInlines", typeof(ObservableCollection<Inline>), typeof(TextBlockHelper),
                new PropertyMetadata(null, OnBindableInlinesChanged));

        public static ObservableCollection<Inline> GetBindableInlines(DependencyObject obj)
        {
            return (ObservableCollection<Inline>)obj.GetValue(BindableInlinesProperty);
        }

        public static void SetBindableInlines(DependencyObject obj, ObservableCollection<Inline> value)
        {
            obj.SetValue(BindableInlinesProperty, value);
        }

        private static void OnBindableInlinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock && e.NewValue is ObservableCollection<Inline> newInlines)
            {
                textBlock.Inlines.Clear();

                foreach (Inline inline in newInlines)
                {
                    textBlock.Inlines.Add(inline);
                }

                newInlines.CollectionChanged += (s, args) =>
                {
                    if (args.Action == NotifyCollectionChangedAction.Reset)
                    {
                        textBlock.Inlines.Clear(); // 👈 This handles Clear()
                        return;
                    }

                    if (args.NewItems != null)
                    {
                        foreach (Inline newInline in args.NewItems)
                        {
                            textBlock.Inlines.Add(newInline);
                        }
                    }

                    if (args.OldItems != null)
                    {
                        foreach (Inline oldInline in args.OldItems)
                        {
                            textBlock.Inlines.Remove(oldInline);
                        }
                    }
                };
            }
        }
    }
}
