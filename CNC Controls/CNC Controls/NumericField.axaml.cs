/*
 * NumericField.xaml.cs - part of CNC Controls library
 *
 * v0.43 / 2023-06-28 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2018-2023, Io Engineering (Terje Io)
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

//using System.Windows;
//using System.Windows.Controls;
using Avalonia;
using Avalonia.Controls;
using CNC.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace CNC.Controls
{
    /// <summary>
    /// Interaction logic for NumericField.xaml
    /// </summary>
    public partial class NumericField : UserControl
    {
        protected string format;
        protected bool metric = true, allow_dp = true, allow_sign = false;
        protected int precision = 3;

        public NumericField()
        {
            InitializeComponent();

            data.DataContext = this;

            ValueProperty.Changed.AddClassHandler<NumericField>(OnValueChanged);
            ColonAtProperty.Changed.AddClassHandler<NumericField>(OnColonAtChanged);
        }

        //public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumericField), new PropertyMetadata(double.NaN, new PropertyChangedCallback(OnValueChanged)), new ValidateValueCallback(IsValidReading));
        public static readonly StyledProperty<double> ValueProperty = StyledProperty<double>.Register<NumericField, double>(nameof(Value), double.NaN, false, Avalonia.Data.BindingMode.Default, IsValidReading, null, false);

        public double Value
        {
            get { double v = (double)GetValue(ValueProperty); return double.IsNaN(v) ? 0d : v; }
            set { SetValue(ValueProperty, value); }
        }
        //private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        private static void OnValueChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            if (double.IsNaN((double)e.NewValue))
                ((NumericField)d).data.Clear();
        }

        //public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(nameof(Format), typeof(string), typeof(NumericField), new PropertyMetadata(NumericProperties.MetricFormat));
        public static readonly StyledProperty<string> FormatProperty = StyledProperty<string>.Register<NumericField, string>(nameof(Format), NumericProperties.MetricFormat, false, Avalonia.Data.BindingMode.Default);
        //new PropertyMetadata(NumericProperties.MetricFormat));
        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        //public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(NumericField), new PropertyMetadata("Label:"));
        public static readonly StyledProperty<string> LabelProperty = StyledProperty<string>.Register<NumericField, string>(nameof(Label), "Label:", false, Avalonia.Data.BindingMode.Default);
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        //public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(nameof(Unit), typeof(string), typeof(NumericField), new PropertyMetadata("mm"));
        public static readonly StyledProperty<string> UnitProperty = StyledProperty<string>.Register<NumericField,string>(nameof(Unit), "mm", false, Avalonia.Data.BindingMode.Default);
        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        //public static readonly DependencyProperty Tooltip2Property = DependencyProperty.Register(nameof(Tooltip2), typeof(string), typeof(NumericField), new PropertyMetadata(string.Empty));
        public static readonly StyledProperty<string> Tooltip2Property = StyledProperty<string>.Register<NumericField,string>(nameof(Tooltip2), string.Empty, false, Avalonia.Data.BindingMode.Default);
        public string Tooltip2
        {
            get { return (string)GetValue(Tooltip2Property); }
            set { SetValue(Tooltip2Property, value); }
        }

        //public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(NumericField), new PropertyMetadata());
        public static readonly StyledProperty<bool> IsReadOnlyProperty = StyledProperty<bool>.Register<NumericField,bool>(nameof(IsReadOnly), default, false, Avalonia.Data.BindingMode.Default);
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        //public static readonly DependencyProperty ColonAtProperty = DependencyProperty.Register(nameof(ColonAt), typeof(double), typeof(NumericField), new PropertyMetadata(70.0d, new PropertyChangedCallback(OnColonAtChanged)));
        public static readonly StyledProperty<double> ColonAtProperty = StyledProperty<double>.Register<NumericField,double>(nameof(ColonAt), 70.0d, false, Avalonia.Data.BindingMode.Default);
        public double ColonAt
        {
            get { return (double)GetValue(ColonAtProperty); }
            set { SetValue(ColonAtProperty, value); }
        }
        //private static void OnColonAtChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        private static void OnColonAtChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((NumericField)d).OnColonAtChanged();
        }
        private void OnColonAtChanged()
        {
            grid.ColumnDefinitions[0].Width = new GridLength(ColonAt);
        }

        public string Text
        {
            get { return Value.ToInvariantString(data.DisplayFormat); }
        }

        //public static bool IsValidReading(object value)
        public static bool IsValidReading(double value)
        {
            double v = (double)value;
            return (!v.Equals(double.PositiveInfinity));
        }
                   
        public Control Field { get { return data; } }

        public void Clear()
        {
            data.Clear();
        }
    }
}

