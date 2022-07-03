using NodaTime;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArchivePlanner.Util
{
    /// <summary>
    /// Interaction logic for TimeBox.xaml
    /// </summary>
    public partial class TimeBox : UserControl
    {
        public static DependencyProperty TimeProperty = 
            DependencyProperty.Register(nameof(Time), typeof(LocalTime), typeof(TimeBox), new PropertyMetadata(LocalTime.Midnight));

        private readonly Regex _timeCharacterRegex;

        public LocalTime Time
        {
            get
            {
                return (LocalTime)GetValue(TimeProperty);
            }
            set
            {
                SetValue(TimeProperty, value);
            }
        }

        public TimeBox()
        {
            _timeCharacterRegex = new Regex("[:0-9]", RegexOptions.Compiled);

            InitializeComponent();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_timeCharacterRegex.IsMatch(e.Text);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox)sender;
            var timeInput = txt.Text;

            if (!timeInput.Contains(':'))
            {
                if (timeInput.Length == 3)
                {
                    timeInput = $"0{timeInput.Substring(0, 1)}:{timeInput.Substring(1, 2)}";
                }
                else if (timeInput.Length == 4)
                {
                    timeInput = $"{timeInput.Substring(0, 2)}:{timeInput.Substring(2, 2)}";
                }
            }

            txt.Text = timeInput;

            var binding = txt.GetBindingExpression(TextBox.TextProperty);
            if (binding != null && timeInput.Length == 5)
                binding.UpdateSource();
        }
    }
}
