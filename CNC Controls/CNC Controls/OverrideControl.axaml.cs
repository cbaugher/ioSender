/*
 * OverrideControl.xaml.cs - part of CNC Controls library
 *
 * v0.20 / 2020-07-19 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2018-2020, Io Engineering (Terje Io)
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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CNC.Core;

namespace CNC.Controls
{
    public partial class OverrideControl : UserControl
    {
        //private DependencyPropertyDescriptor minus_dpd;
       
        public delegate void CommandGeneratedHandler(string command);
        public event CommandGeneratedHandler CommandGenerated;
        IDisposable disposer;

        public OverrideControl()
        {
            InitializeComponent();

            //REMOVE
            //minus_dpd = DependencyPropertyDescriptor.FromProperty(OverrideControl.MinusOnlyProperty, typeof(OverrideControl));
            //minus_dpd.AddValueChanged(this, OnMinusChanged);

            disposer = MinusOnlyProperty.Changed.Subscribe(OnMinusChanged);
        }

        ~OverrideControl()
        {
            //REMOVE??  THERE IS NO UNSUBSCRIBE METHOD FOR THIS
            //minus_dpd.RemoveValueChanged(this, OnMinusChanged);
            disposer.Dispose();
        }

        public byte ResetCommand { set { btnOvReset.Tag = ((char)value).ToString(); } }
        public byte FinePlusCommand { set { btnOvFinePlus.Tag = ((char)value).ToString(); } }
        public byte FineMinusCommand { set { btnOvFineMinus.Tag = ((char)value).ToString(); } }
        public byte CoarsePlusCommand { set { btnOvCoarsePlus.Tag = ((char)value).ToString(); } }
        public byte CoarseMinusCommand { set { btnOvCoarseMinus.Tag = ((char)value).ToString(); } }

        public static readonly StyledProperty<double> ValueProperty = StyledProperty<double>.Register<OverrideControl,double>(nameof(Value), default, false, Avalonia.Data.BindingMode.Default);
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        //private static void OnValuehanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        private static void OnValuehanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((OverrideControl)d).txtOverride.Text = ((double)e.NewValue).ToString() + "%"; ;
        }

        public static readonly StyledProperty<bool> MinusOnlyProperty =  StyledProperty<bool>.Register<OverrideControl,bool>(nameof(MinusOnly), default, false, Avalonia.Data.BindingMode.Default);
        public bool MinusOnly
        {
            get { return (bool)GetValue(MinusOnlyProperty); }
            set { SetValue(MinusOnlyProperty, value); }
        }
        //public void OnMinusChanged(object sender, EventArgs args)
        public void OnMinusChanged(EventArgs args)
        {
            if (MinusOnly)
            {
                btnOvFinePlus.IsVisible = false;
                btnOvCoarsePlus.IsVisible = false;
            }
        }

        public static readonly StyledProperty<GrblEncoderMode> EncoderModeProperty = StyledProperty<GrblEncoderMode>.Register<OverrideControl,GrblEncoderMode>
            (nameof(EncoderMode), default, false, Avalonia.Data.BindingMode.Default);
        public GrblEncoderMode EncoderMode
        {
            get { return (GrblEncoderMode)GetValue(EncoderModeProperty); }
            set { SetValue(EncoderModeProperty, value); }
        }

        void btnOverrideClick(object? sender, RoutedEventArgs e)
        {
            CommandGenerated?.Invoke((string)((Button)sender).Tag);
        }

    }
}

