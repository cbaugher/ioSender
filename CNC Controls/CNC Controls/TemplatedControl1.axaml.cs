using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace CNC.Controls
{
    public class TemplatedControl1 : TemplatedControl
    {
        //private static Brush ScaledOn = Brushes.Yellow, ScaledOff;
        private static IBrush ScaledOn = Brushes.Yellow;
        private static IBrush ScaledOff = null;

        public delegate void ZeroClickHandler(object sender, RoutedEventArgs e);
        public event ZeroClickHandler ZeroClick;

        public TemplatedControl1()
        {
            //InitializeComponent();

//            ScaledOff = (IBrush)btnScaled.Background;
            //ScaledOn.SetCurrentValue(ValueProperty, Brushes.Yellow);

            IsScaledProperty.Changed.AddClassHandler<DROBaseControl>(OnIsScaledChanged);
            LabelProperty.Changed.AddClassHandler<DROBaseControl>(OnLabelPropertyChanged);
        }

        //public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(DROBaseControl), new PropertyMetadata());
        public static readonly StyledProperty<string> LabelProperty = StyledProperty<string>.Register<DROBaseControl, string>(nameof(Label), default, false,
            Avalonia.Data.BindingMode.Default, null, null, false);
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        private static void OnLabelPropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            //FIXME REPLACE DROBASECONTROL
            ((DROBaseControl)d).lblAxis.Content = e.NewValue;
        }

        //public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(DROBaseControl), new PropertyMetadata());
        public static readonly StyledProperty<double> ValueProperty = StyledProperty<double>.Register<DROBaseControl, double>(nameof(Value), default, false,
            Avalonia.Data.BindingMode.Default);
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly StyledProperty<bool> IsReadOnlyProperty = StyledProperty<bool>.Register<DROBaseControl, bool>(nameof(IsReadOnly), false, false, Avalonia.Data.BindingMode.TwoWay);
        public bool IsReadOnly
        {
            //get { return txtReadout.IsReadOnly; }
            get { return GetValue(IsReadOnlyProperty); }
            //set { txtReadout.IsReadOnly = value; }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        //public static readonly DependencyProperty IsScaledProperty = DependencyProperty.Register(nameof(IsScaled), typeof(bool), typeof(DROBaseControl), new PropertyMetadata(false, new PropertyChangedCallback(OnIsScaledChanged)));
        public static readonly StyledProperty<bool> IsScaledProperty = StyledProperty<bool>.Register<DROBaseControl, bool>(nameof(IsScaled), default, false, Avalonia.Data.BindingMode.Default,
            null, null, false);
        public bool IsScaled
        {
            get { return (bool)GetValue(IsScaledProperty); }
            set { SetValue(IsScaledProperty, value); }
        }
        //private static void OnIsScaledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        private static void OnIsScaledChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            //FIXME REPLACE DROBASECONTROL
            ((DROBaseControl)d).btnScaled.Background = (bool)e.NewValue ? ScaledOn : ScaledOff;
        }

//        public new object Tag
//        {
//            get { return txtReadout.Tag; }
//            set { txtReadout.Tag = btnZero.Tag = value; }
//        }

        private void btnZero_Click(object? sender, RoutedEventArgs e)
        {
            ZeroClick?.Invoke(sender, e);
        }

        private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }
    }
}
