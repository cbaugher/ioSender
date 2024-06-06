using Avalonia;
using Avalonia.Controls;


namespace CNC.Controls;

public partial class TestControl : UserControl
{
    public TestControl()
    {
        InitializeComponent();
    }

    private static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<TestControl, string>(nameof(Label), default!, false,
        Avalonia.Data.BindingMode.Default);
    public string Label
    {
        get { return GetValue(LabelProperty); }
        set { SetValue(LabelProperty, value); }
    }
}