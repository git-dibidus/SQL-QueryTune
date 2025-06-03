using System.Windows;
using ICSharpCode.AvalonEdit;

namespace QueryTune.WPF.Behaviors
{
    public static class AvalonEditBehavior
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached(
                "Text",
                typeof(string),
                typeof(AvalonEditBehavior),
                new FrameworkPropertyMetadata(
                    default(string),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    TextPropertyChanged));

        public static void SetText(TextEditor editor, string value)
            => editor.SetValue(TextProperty, value);

        public static string GetText(TextEditor editor)
            => (string)editor.GetValue(TextProperty);

        private static void TextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = d as TextEditor;
            if (editor == null) return;

            if (e.NewValue is string newText && editor.Text != newText)
            {
                editor.Text = newText;
            }

            editor.TextChanged += (sender, args) =>
            {
                if (editor.Text != (string)editor.GetValue(TextProperty))
                {
                    editor.SetValue(TextProperty, editor.Text);
                }
            };
        }
    }
}
