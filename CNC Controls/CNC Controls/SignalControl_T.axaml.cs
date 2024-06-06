using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace CNC.Controls
{
    public class SignalControl_T : TemplatedControl
    {
        //static Brush LEDOn = Brushes.Red, LEDOff = Brushes.LightGray;
//        static IBrush LEDOn = Brushes.Red, LEDOff = Brushes.LightGray;

        public SignalControl_T()
        {
            //InitializeComponent();

//            IsSetProperty.Changed.AddClassHandler<SignalControl>(OnIsSetChanged);
        }

        //public static readonly DependencyProperty IsSetProperty = DependencyProperty.Register(nameof(IsSet), typeof(bool), typeof(SignalControl), new PropertyMetadata(false, new PropertyChangedCallback(OnIsSetChanged)));
        public static readonly StyledProperty<bool> IsSetProperty = StyledProperty<bool>.Register<SignalControl, bool>(nameof(IsSet), false, false, Avalonia.Data.BindingMode.Default);
        public bool IsSet
        {
            get { return (bool)GetValue(IsSetProperty); }
            set { SetValue(IsSetProperty, value); }
        }
        //private static void OnIsSetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        private static void OnIsSetChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
//        {
//            (d as SignalControl).btnLED.Background = (bool)e.NewValue ? LEDOn : LEDOff;
//        }

        //public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(SignalControl), new PropertyMetadata());
        public static readonly StyledProperty<string> LabelProperty = StyledProperty<string>.Register<SignalControl, string>(nameof(Label), "", false, Avalonia.Data.BindingMode.Default);
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
    }
}
