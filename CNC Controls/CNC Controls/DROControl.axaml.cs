/*
 * DROControl.xaml.cs - part of CNC Controls library
 *
 * v0.42 / 2023-02-14 / Io Engineering (Terje Io)
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

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Media;
using CNC.Core;
using CNC.GCode;

namespace CNC.Controls
{
    public partial class DROControl : UserControl
    {
        private double orgpos;
        private bool hasFocus = false;
        private IBrush background = null;
        private static bool keyboardMappingsOk = false;

        public string DisplayFormat { get; private set; }

        public delegate void DROEnabledChangedHandler(bool enabled);
        public event DROEnabledChangedHandler DROEnabledChanged;

        public DROControl()
        {
            InitializeComponent();

            foreach (DROBaseControl axis in UIUtils.FindLogicalChildren<DROBaseControl>(this))
            {
                axis.txtReadout.GotFocus += txtReadout_GotFocus;
                axis.txtReadout.LostFocus += txtReadout_LostFocus;
                //axis.txtReadout.PreviewKeyDown += txtReadout_PreviewKeyDown;
                //axis.txtReadout.PreviewKeyUp += txtReadout_PreviewKeyUp;
                axis.btnZero.Click += btnZero_Click;
            }
        }

        public new bool IsFocused { get { return hasFocus; } }
        public bool IsFocusable { get; set; }

        public void EnableFocus()
        {
            IsFocusable = true;
        }

        private void DRO_Loaded(object sender, RoutedEventArgs e)
        {
            if (!keyboardMappingsOk && DataContext is GrblViewModel)
            {
                KeypressHandler keyboard = (DataContext as GrblViewModel).Keyboard;

                keyboardMappingsOk = true;

                keyboard.AddHandler(Key.X, KeyModifiers.Control | KeyModifiers.Shift, ZeroX);
                keyboard.AddHandler(Key.Y, KeyModifiers.Control | KeyModifiers.Shift, ZeroY);
                keyboard.AddHandler(Key.Z, KeyModifiers.Control | KeyModifiers.Shift, ZeroZ);
                if (GrblInfo.AxisFlags.HasFlag(AxisFlags.A))
                    keyboard.AddHandler(Key.A, KeyModifiers.Control | KeyModifiers.Shift, ZeroA);
                if (GrblInfo.AxisFlags.HasFlag(AxisFlags.B))
                    keyboard.AddHandler(Key.B, KeyModifiers.Control | KeyModifiers.Shift, ZeroB);
                if (GrblInfo.AxisFlags.HasFlag(AxisFlags.C))
                    keyboard.AddHandler(Key.C, KeyModifiers.Control | KeyModifiers.Shift, ZeroC);
                keyboard.AddHandler(Key.D0, KeyModifiers.Control | KeyModifiers.Shift, ZeroAxes);
            }

            foreach (DROBaseControl axis in UIUtils.FindLogicalChildren<DROBaseControl>(this))
                axis.Tag = GrblInfo.AxisLetterToIndex(axis.Label);
        }

        private void txtReadout_GotFocus(object sender, RoutedEventArgs e)
        {
            if (IsFocusable && !(DataContext as GrblViewModel).IsJobRunning)
            {
                (DataContext as GrblViewModel).SuspendPositionNotifications = true;

                //orgpos = (DataContext as GrblViewModel).Position.Values[(int)((NumericTextBox)(sender)).Tag];
                orgpos = (DataContext as GrblViewModel).Position.Values[(int)((NumericUpDown)(sender)).Tag];

                //background = (Brush)(sender as NumericTextBox).Background;  // SUSPECT
                //(sender as NumericTextBox).IsReadOnly = false;
                //(sender as NumericTextBox).Background = Brushes.White;
                background = (IBrush)(sender as NumericUpDown).Background;  // SUSPECT
                (sender as NumericUpDown).IsReadOnly = false;
                (sender as NumericUpDown).Background = Brushes.White;

                hasFocus = true;

                DROEnabledChanged?.Invoke(true);
            }
        }

        void txtReadout_LostFocus(object sender, EventArgs e)
        {
            //((NumericTextBox)(sender)).IsReadOnly = true;
            ((NumericUpDown)(sender)).IsReadOnly = true;

            (DataContext as GrblViewModel).SuspendPositionNotifications = false;

            if (hasFocus)
            {
                //(sender as NumericTextBox).Background = background;
                //(DataContext as GrblViewModel).Position.Values[(int)(sender as NumericTextBox).Tag] = orgpos;
                (sender as NumericUpDown).Background = background;
                (DataContext as GrblViewModel).Position.Values[(int)(sender as NumericUpDown).Tag] = orgpos;

            }

            hasFocus = false;

            DROEnabledChanged?.Invoke(false);
        }

        private void txtReadout_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!hasFocus)
                e.Handled = true;
        }

        private void txtReadout_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //NumericTextBox axis = (NumericTextBox)sender;
                NumericUpDown axis = (NumericUpDown)sender;

                //CAN'T COMPARE WITH DOUBLES   if (axis.Value != orgpos)
                    AxisPositionChanged(GrblInfo.AxisIndexToLetter((int)axis.Tag), (double)axis.Value);

                axis.IsReadOnly = true;

                DROEnabledChanged?.Invoke(false);
            }
        }

        void btnZero_Click(object? sender, RoutedEventArgs e)
        {
            AxisPositionChanged(GrblInfo.AxisIndexToLetter((int)(sender as Button).Tag), 0.0d);
        }

        void btnZeroAll_Click(object? sender, RoutedEventArgs e)
        {
            AxisPositionChanged("ALL", 0.0d);
        }

        private bool ZeroAxes(Key key)
        {
            AxisPositionChanged("ALL", 0d);

            return true;
        }

        private bool ZeroX(Key key)
        {
            AxisPositionChanged("X", 0d);

            return true;
        }

        private bool ZeroY(Key key)
        {
            AxisPositionChanged("Y", 0d);

            return true;
        }
        private bool ZeroZ(Key key)
        {
            AxisPositionChanged("Z", 0d);

            return true;
        }
        private bool ZeroA(Key key)
        {
            AxisPositionChanged(GrblInfo.AxisIndexToLetter(3), 0d);

            return true;
        }
        private bool ZeroB(Key key)
        {
            AxisPositionChanged(GrblInfo.AxisIndexToLetter(4), 0d);

            return true;
        }
        private bool ZeroC(Key key)
        {
            AxisPositionChanged(GrblInfo.AxisIndexToLetter(5), 0d);

            return true;
        }

        void AxisPositionChanged(string axis, double position)
        {
            if (axis == "ALL")
            {
                string s = "G90G10L20P0";
                foreach (int i in GrblInfo.AxisFlags.ToIndices())
                    s += GrblInfo.AxisIndexToLetter(i) + "{0}";
                (DataContext as GrblViewModel).ExecuteCommand(string.Format(s, position.ToInvariantString("F3")));
            }
            else
                (DataContext as GrblViewModel).ExecuteCommand(string.Format("G10L20P0{0}{1}", axis, position.ToInvariantString("F3")));
        }

    }
}
