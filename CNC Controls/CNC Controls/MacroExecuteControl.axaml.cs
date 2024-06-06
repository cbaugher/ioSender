/*
 * MacroExecuteControl.xaml.cs - part of CNC Controls library
 *
 * v0.36 / 2021-12-27 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2020-2021, Io Engineering (Terje Io)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

· Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.

· Redistributions in binary form must reproduce the above copyright notice, this
list of conditions and the following disclaimer in the documentation and/or
other materials provided with the distribution.

· Neither the name of the copyright holder nor the names of its contributors may
be used to endorse or promote products derived from this software without
specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
using CNC.Core;

namespace CNC.Controls
{
    /// <summary>
    /// Interaction logic for MacroExecuteControl.xaml
    /// </summary>
    public partial class MacroExecuteControl : UserControl, ISidebarControl
    {

        public MacroExecuteControl()
        {
            InitializeComponent();
            //FIXME   DataContextChanged += View_DataContextChanged;
        }

        //public string MenuLabel { get { return (string)FindResource("MenuLabel"); } }
        public string MenuLabel { get { return (string)Resources["MenuLabel"]; } }

        private void macroExecuteControl_Loaded(object sender, RoutedEventArgs e)
        {
            Macros = AppConfig.Settings.Macros;
        }

        //private void View_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        private void View_DataContextChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && e.OldValue is INotifyPropertyChanged)
                ((INotifyPropertyChanged)e.OldValue).PropertyChanged -= OnDataContextPropertyChanged;
            if (e.NewValue != null && e.NewValue is INotifyPropertyChanged)
                (e.NewValue as GrblViewModel).PropertyChanged += OnDataContextPropertyChanged;
        }

        private void OnDataContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is GrblViewModel && IsVisible == true) switch (e.PropertyName)
            {
                case nameof(GrblViewModel.StreamingState):
                    if ((sender as GrblViewModel).IsJobRunning)
                        IsVisible = false;
                    break;
            }
        }

        //public static readonly DependencyProperty MacrosProperty = DependencyProperty.Register(nameof(MacroExecuteControl.Macros), typeof(ObservableCollection<CNC.GCode.Macro>), typeof(MacroExecuteControl), new PropertyMetadata(new PropertyChangedCallback(OnMacrosChanged)));
        public static readonly StyledProperty<ObservableCollection<CNC.GCode.Macro>> MacrosProperty = StyledProperty<ObservableCollection<CNC.GCode.Macro>>
            .Register<MacroExecuteControl, ObservableCollection<CNC.GCode.Macro>>(nameof(MacroExecuteControl.Macros), null, false, Avalonia.Data.BindingMode.Default, null, null, false);
        public ObservableCollection<CNC.GCode.Macro> Macros
        {
            get { return (ObservableCollection<CNC.GCode.Macro>)GetValue(MacrosProperty); }
            set { SetValue(MacrosProperty, value); }
        }

        //private static void OnMacrosChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        private static void OnMacrosChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            (d as MacroExecuteControl).OnMacrosChanged();
        }
        private void OnMacrosChanged()
        {
            Macros.CollectionChanged += Macros_CollectionChanged;
            Macros_CollectionChanged(Macros, null);
        }
        private void Macros_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //IsMessageVisible = (sender as ObservableCollection<CNC.GCode.Macro>).Count == 0 ? Visibility.Visible : Visibility.Hidden;
            IsMessageVisible = (sender as ObservableCollection<CNC.GCode.Macro>).Count == 0 ? true : false;
        }

        //public static readonly DependencyProperty IsMessageVisibleProperty = DependencyProperty.Register(nameof(IsMessageVisible), typeof(Visibility), typeof(MacroExecuteControl), new PropertyMetadata(Visibility.Visible));
        public static readonly StyledProperty<bool> IsMessageVisibleProperty = StyledProperty<bool>.Register<MacroExecuteControl, bool>(nameof(IsMessageVisible), default, false, Avalonia.Data.BindingMode.TwoWay, null, null, false);
        //public Visibility IsMessageVisible
        public bool IsMessageVisible
        {
            //get { return (Visibility)GetValue(IsMessageVisibleProperty); }
            get { return (bool)GetValue(IsMessageVisibleProperty); }
            set { SetValue(IsMessageVisibleProperty, value); }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            CNC.GCode.Macro macro = Macros.FirstOrDefault(o => o.Id == (int)(sender as Button).Tag);
            //FIXME  if (macro != null && (!macro.ConfirmOnExecute || MessageBox.Show(string.Format((string)FindResource("RunMacro"), macro.Name), "ioSender", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes))
            //    (DataContext as GrblViewModel).ExecuteMacro(macro.Code);
        }

        private void btn_Close(object sender, RoutedEventArgs e)
        {
            //Visibility = Visibility.Hidden;
            IsVisible = false;
        }

        private void button_Edit(object sender, RoutedEventArgs e)
        {
            //FIXME  MacroEditor editor = new MacroEditor(Macros) {Owner = Application.Current.MainWindow};
            //editor.ShowDialog();
            //AppConfig.Settings.Save();
        }
    }
}
