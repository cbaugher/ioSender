using CNC.Core;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia;


namespace CNC.Controls
{
    /// <summary>
    /// Interaction logic for GCodeListControl.xaml
    /// </summary>
    public partial class GCodeListControl : UserControl
    {
        public ScrollViewer scroll = null;

        public GCodeListControl()
        {
            InitializeComponent();

        }
        private void grdGCode_Drag(object sender, DragEventArgs e)
        {
            GCode.File.Drag(sender, e);
        }

        private void grdGCode_Drop(object sender, DragEventArgs e)
        {
            GCode.File.Drop(sender, e);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            scroll = UIUtils.GetScrollViewer(grdGCode);
            grdGCode.DataContext = GCode.File.Data.DefaultView;
            if (DataContext is GrblViewModel)
                //REMOVE IF FIXED   (DataContext as GrblViewModel).PropertyChanged += GCodeListControl_PropertyChanged;
                DataContextProperty.Changed.AddClassHandler<GCodeListControl>(GCodeListControl_PropertyChanged);
        }

        private void GCodeListControl_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            //if (sender is GrblViewModel) switch (e.PropertyName)
            if (sender is GrblViewModel) switch (e.Property.Name)
            {
                case nameof(GrblViewModel.ScrollPosition):
                    int sp = ((GrblViewModel)sender).ScrollPosition;
                    if (sp == 0)
                        //scroll.ScrollToTop();
                            scroll.ScrollToHome();
                        //else
                            //FIXME   scroll.ScrollToVerticalOffset(sp);
                    
                    break;
            }
        }
    }
}
