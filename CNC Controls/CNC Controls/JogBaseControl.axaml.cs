﻿/*
 * JogBaseControl.xaml.cs - part of CNC Controls library
 *
 * v0.37 / 2022-02-27 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2020-2022, Io Engineering (Terje Io)
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CNC.Core;
using CNC.GCode;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;

namespace CNC.Controls
{
    /// <summary>
    /// Interaction logic for JogControl.xaml
    /// </summary>
    public partial class JogBaseControl : UserControl
    {
        private string mode = "G21"; // Metric
        private bool softLimits = false;
        //FIXME REMOVE NOT USED    private int distance = 2, feedrate = 2;
        private int jogAxis = -1;
        private double limitSwitchesClearance = .5d, position = 0d;
        private KeypressHandler keyboard;
        private static bool keyboardMappingsOk = false;

        private const Key xplus = Key.J, xminus = Key.H, yplus = Key.K, yminus = Key.L, zplus = Key.I, zminus = Key.M, aplus = Key.U, aminus = Key.N;

        public JogBaseControl()
        {
            InitializeComponent();

            JogData = new JogViewModel();

            Focusable = true;
        }

        public static JogViewModel JogData { get; private set; }
        //public string MenuLabel { get { return (string)FindResource("MenuLabel"); } }
        public string MenuLabel { get { return (string)Resources["MenuLabel"]; } }

        private void JogData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //private void JogData_PropertyChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(JogViewModel.Distance):
                    if (AppConfig.Settings.Jog.Mode == JogConfig.JogMode.UI || (AppConfig.Settings.Jog.LinkStepJogToUI && JogData.StepSize != JogViewModel.JogStepIncrements.Step3))
                        (DataContext as GrblViewModel).JogStep = JogData.Distance;
                    break;
            }
        }

        //private void Model_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GrblViewModel.MachinePosition) || e.PropertyName == nameof(GrblViewModel.GrblState))
            //if (e.Property.Name == nameof(GrblViewModel.MachinePosition) || e.Property.Name == nameof(GrblViewModel.GrblState))
            {
                if ((sender as GrblViewModel).GrblState.State != GrblStates.Jog)
                    jogAxis = -1;
            }
        }

        private void JogControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GrblViewModel)
            {
                mode = GrblSettings.GetInteger(GrblSetting.ReportInches) == 0 ? "G21" : "G20";
                softLimits = !(GrblInfo.IsGrblHAL && GrblSettings.GetInteger(grblHALSetting.SoftLimitJogging) == 1) && GrblSettings.GetInteger(GrblSetting.SoftLimitsEnable) == 1;
                limitSwitchesClearance = GrblSettings.GetDouble(GrblSetting.HomingPulloff);

                JogData.SetMetric(mode == "G21");

                if (!keyboardMappingsOk)
                {
                    if (!GrblInfo.HasFirmwareJog || AppConfig.Settings.Jog.LinkStepJogToUI)
                        JogData.PropertyChanged += JogData_PropertyChanged;
                        //JogData.Distance.CollectionChanged += JogData_PropertyChanged;
                        
                    if (softLimits)
                        (DataContext as GrblViewModel).PropertyChanged += Model_PropertyChanged;
                        //DataContextProperty.Changed.AddClassHandler<JogBaseControl>(Model_PropertyChanged);
                    
                    keyboard = (DataContext as GrblViewModel).Keyboard;

                    keyboardMappingsOk = true;

                    if (AppConfig.Settings.Jog.Mode == JogConfig.JogMode.UI)
                    {
                        keyboard.AddHandler(Key.PageUp, KeyModifiers.None, CursorJogZplus, false);
                        keyboard.AddHandler(Key.PageDown, KeyModifiers.None, CursorJogZminus, false);
                        keyboard.AddHandler(Key.Left, KeyModifiers.None, CursorJogXminus, false);
                        keyboard.AddHandler(Key.Up, KeyModifiers.None, CursorJogYplus, false);
                        keyboard.AddHandler(Key.Right, KeyModifiers.None, CursorJogXplus, false);
                        keyboard.AddHandler(Key.Down, KeyModifiers.None, CursorJogYminus, false);
                    }

                    keyboard.AddHandler(xplus, KeyModifiers.Control | KeyModifiers.Shift, KeyJogXplus, false);
                    keyboard.AddHandler(xminus, KeyModifiers.Control | KeyModifiers.Shift, KeyJogXminus, false);
                    keyboard.AddHandler(yplus, KeyModifiers.Control | KeyModifiers.Shift, KeyJogYplus, false);
                    keyboard.AddHandler(yminus, KeyModifiers.Control | KeyModifiers.Shift, KeyJogYminus, false);
                    keyboard.AddHandler(zplus, KeyModifiers.Control | KeyModifiers.Shift, KeyJogZplus, false);
                    keyboard.AddHandler(zminus, KeyModifiers.Control | KeyModifiers.Shift, KeyJogZminus, false);
                    if (GrblInfo.AxisFlags.HasFlag(AxisFlags.A))
                    {
                        keyboard.AddHandler(aplus, KeyModifiers.Control | KeyModifiers.Shift, KeyJogAplus, false);
                        keyboard.AddHandler(aminus, KeyModifiers.Control | KeyModifiers.Shift, KeyJogAminus, false);
                    }

                    if (AppConfig.Settings.Jog.Mode != JogConfig.JogMode.Keypad)
                    {
                        keyboard.AddHandler(Key.End, KeyModifiers.None, EndJog, false);

                        keyboard.AddHandler(Key.NumPad0, KeyModifiers.Control, JogStep0);
                        keyboard.AddHandler(Key.NumPad1, KeyModifiers.Control, JogStep1);
                        keyboard.AddHandler(Key.NumPad2, KeyModifiers.Control, JogStep2);
                        keyboard.AddHandler(Key.NumPad3, KeyModifiers.Control, JogStep3);
                        keyboard.AddHandler(Key.NumPad4, KeyModifiers.Control, JogFeed0);
                        keyboard.AddHandler(Key.NumPad5, KeyModifiers.Control, JogFeed1);
                        keyboard.AddHandler(Key.NumPad6, KeyModifiers.Control, JogFeed2);
                        keyboard.AddHandler(Key.NumPad7, KeyModifiers.Control, JogFeed3);

                        keyboard.AddHandler(Key.NumPad2, KeyModifiers.None, FeedDec);
                        keyboard.AddHandler(Key.NumPad4, KeyModifiers.None, StepDec);
                        keyboard.AddHandler(Key.NumPad6, KeyModifiers.None, StepInc);
                        keyboard.AddHandler(Key.NumPad8, KeyModifiers.None, FeedInc);
                    }
                }
            }
        }

        private bool KeyJogXplus(Key key)
        {
            if (keyboard.CanJog2 && !keyboard.IsRepeating)
                JogCommand(GrblInfo.LatheModeEnabled ? "Z+" : "X+");

            return true;
        }

        private bool KeyJogXminus(Key key)
        {
            if (keyboard.CanJog2 && !keyboard.IsRepeating)
                JogCommand(GrblInfo.LatheModeEnabled ? "Z-" : "X-");

            return true;
        }

        private bool KeyJogYplus(Key key)
        {
            if (keyboard.CanJog2 && !keyboard.IsRepeating)
                JogCommand(GrblInfo.LatheModeEnabled ? "X-" : "Y+");

            return true;
        }

        private bool KeyJogYminus(Key key)
        {
            if (keyboard.CanJog2 && !keyboard.IsRepeating)
                JogCommand(GrblInfo.LatheModeEnabled ? "X+" : "Y-");

            return true;
        }

        private bool KeyJogZplus(Key key)
        {
            if (keyboard.CanJog2 && !keyboard.IsRepeating && !GrblInfo.LatheModeEnabled)
                JogCommand("Z+");

            return true;
        }

        private bool KeyJogZminus(Key key)
        {
            if (keyboard.CanJog2 && !keyboard.IsRepeating && !GrblInfo.LatheModeEnabled)
                JogCommand("Z-");

            return true;
        }

        private bool KeyJogAplus(Key key)
        {
            if (keyboard.CanJog2 && !keyboard.IsRepeating)
                JogCommand("A+");

            return true;
        }

        private bool KeyJogAminus(Key key)
        {
            if (keyboard.CanJog2 && !keyboard.IsRepeating)
                JogCommand("A-");

            return true;
        }

        private bool CursorJogXplus(Key key)
        {
            if (keyboard.CanJog && !keyboard.IsRepeating)
                JogCommand(GrblInfo.LatheModeEnabled ? "Z+" : "X+");

            return true;
        }

        private bool CursorJogXminus(Key key)
        {
            if (keyboard.CanJog && !keyboard.IsRepeating)
                JogCommand(GrblInfo.LatheModeEnabled ? "Z-" : "X-");

            return true;
        }

        private bool CursorJogYplus(Key key)
        {
            if (keyboard.CanJog && !keyboard.IsRepeating)
                JogCommand(GrblInfo.LatheModeEnabled ? "X-" : "Y+");

            return true;
        }

        private bool CursorJogYminus(Key key)
        {
            if (keyboard.CanJog && !keyboard.IsRepeating)
                JogCommand(GrblInfo.LatheModeEnabled ? "X+" : "Y-");

            return true;
        }

        private bool CursorJogZplus(Key key)
        {
            if (keyboard.CanJog && !keyboard.IsRepeating && !GrblInfo.LatheModeEnabled)
                JogCommand("Z+");

            return true;
        }

        private bool CursorJogZminus(Key key)
        {
            if (keyboard.CanJog && !keyboard.IsRepeating && !GrblInfo.LatheModeEnabled)
                JogCommand("Z-");

            return true;
        }

        //FIXME REMOVE DOES NOTHING
        private void distance_Click(object sender, RoutedEventArgs e)
        {
//            distance = int.Parse((string)(sender as RadioButton).Tag);
        }

        //FIXME REMOVE DOES NOTHING
        private void feedrate_Click(object sender, RoutedEventArgs e)
        {
//            feedrate = int.Parse((string)(sender as RadioButton).Tag);
        }

        private bool EndJog(Key key)
        {
            if (!keyboard.IsRepeating && keyboard.IsJogging)
                JogCommand("stop");

            return keyboard.IsJogging;
        }

        private bool JogStep0(Key key)
        {
            //JogData.StepSizeIX = JogViewModel.JogStepIncrements.Step0;
            JogData.StepSize = JogViewModel.JogStepIncrements.Step0;

            return true;
        }

        private bool JogStep1(Key key)
        {
            //JogData.StepSizeIX = JogViewModel.JogStepIncrements.Step1;
            JogData.StepSize = JogViewModel.JogStepIncrements.Step1;

            return true;
        }

        private bool JogStep2(Key key)
        {
            //JogData.StepSizeIX = JogViewModel.JogStepIncrements.Step2;
            JogData.StepSize = JogViewModel.JogStepIncrements.Step2;

            return true;
        }

        private bool JogStep3(Key key)
        {
            //JogData.StepSizeIX = JogViewModel.JogStepIncrements.Step3;
            JogData.StepSize = JogViewModel.JogStepIncrements.Step3;

            return true;
        }

        private bool JogFeed0(Key key)
        {
            //JogData.FeedIX = JogViewModel.JogFeedIncrements.Feed0;
            JogData.Feed = JogViewModel.JogFeedIncrements.Feed0;

            return true;
        }

        private bool JogFeed1(Key key)
        {
            //JogData.FeedIX = JogViewModel.JogFeedIncrements.Feed1;
            JogData.Feed = JogViewModel.JogFeedIncrements.Feed1;

            return true;
        }
        private bool JogFeed2(Key key)
        {
            //JogData.FeedIX = JogViewModel.JogFeedIncrements.Feed2;
            JogData.Feed = JogViewModel.JogFeedIncrements.Feed2;

            return true;
        }
        private bool JogFeed3(Key key)
        {
            //JogData.FeedIX = JogViewModel.JogFeedIncrements.Feed3;
            JogData.Feed = JogViewModel.JogFeedIncrements.Feed3;

            return true;
        }

        private bool FeedDec(Key key)
        {
            JogData.FeedDec();

            return true;
        }
        private bool FeedInc(Key key)
        {
            JogData.FeedInc();

            return true;
        }

        private bool StepDec(Key key)
        {
            JogData.StepDec();

            return true;
        }

        private bool StepInc(Key key)
        {
            JogData.StepInc();

            return true;
        }

        private void JogCommand(string cmd)
        {
            GrblViewModel model = DataContext as GrblViewModel;

            if (cmd == "stop")
                cmd = ((char)GrblConstants.CMD_JOG_CANCEL).ToString();
            else
            {
                var distance = cmd[1] == '-' ? -JogData.Distance : JogData.Distance;
                //var distance = cmd[1] == '-' ? -JogData.Distance[(int)JogData.StepSizeIX] : JogData.Distance[(int)JogData.StepSizeIX];

                if (softLimits)
                {
                    int axis = GrblInfo.AxisLetterToIndex(cmd[0]);

                    if (jogAxis != -1 && axis != jogAxis)
                        return;

                    if (axis != jogAxis)
                        position = distance + model.MachinePosition.Values[axis];
                    else
                        position += distance;

                    if (GrblInfo.ForceSetOrigin)
                    {
                        if (!GrblInfo.HomingDirection.HasFlag(GrblInfo.AxisIndexToFlag(axis)))
                        {
                            if (position > 0d)
                                position = 0d;
                            else if (position < (-GrblInfo.MaxTravel.Values[axis] + limitSwitchesClearance))
                                position = (-GrblInfo.MaxTravel.Values[axis] + limitSwitchesClearance);
                        }
                        else
                        {
                            if (position < 0d)
                                position = 0d;
                            else if (position > (GrblInfo.MaxTravel.Values[axis] - limitSwitchesClearance))
                                position = GrblInfo.MaxTravel.Values[axis] - limitSwitchesClearance;
                        }
                    }
                    else
                    {
                        if (position > -limitSwitchesClearance)
                            position = -limitSwitchesClearance;
                        else if (position < -(GrblInfo.MaxTravel.Values[axis] - limitSwitchesClearance))
                            position = -(GrblInfo.MaxTravel.Values[axis] - limitSwitchesClearance);
                    }

                    if (position == 0d)
                        return;

                    jogAxis = axis;

                    cmd = string.Format("$J=G53{0}{1}{2}F{3}", mode, cmd.Substring(0, 1), position.ToInvariantString(), Math.Ceiling(JogData.FeedRate).ToInvariantString());
                    //cmd = string.Format("$J=G53{0}{1}{2}F{3}", mode, cmd.Substring(0, 1), position.ToInvariantString(), Math.Ceiling((double)JogData.FeedRate[(int)JogData.FeedIX]).ToInvariantString());
                }
                else
                    cmd = string.Format("$J=G91{0}{1}{2}F{3}", mode, cmd.Substring(0, 1), distance.ToInvariantString(), Math.Ceiling(JogData.FeedRate).ToInvariantString());
                    //cmd = string.Format("$J=G91{0}{1}{2}F{3}", mode, cmd.Substring(0, 1), distance.ToInvariantString(), Math.Ceiling((double)JogData.FeedRate[(int)JogData.FeedIX]).ToInvariantString());
            }

            model.ExecuteCommand(cmd);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            JogCommand((string)(sender as Button).Tag == "stop" ? "stop" : (string)(sender as Button).Content);
        }
    }

    internal class ArrayValues<T> : ViewModelBase
    {
        private T[] arr = new T[4];

        public int Length { get { return arr.Length; } }

        public T this[int i]
        {
            get { return arr[i]; }
            set
            {
                if (!value.Equals(arr[i]))
                {
                    arr[i] = value;
                    OnPropertyChanged(i.ToString());
                }
            }
        }
    }

    public partial class JogViewModel : ViewModelBase
    {
        public enum JogStepIncrements
        {
            Step0 = 0,
            Step1,
            Step2,
            Step3
        }
        public enum JogFeedIncrements
        {
            Feed0 = 0,
            Feed1,
            Feed2,
            Feed3
        }

        JogStepIncrements _jogStep = JogStepIncrements.Step1;
//        [ObservableProperty][NotifyPropertyChangedFor(nameof(Distance))]JogStepIncrements _stepSizeIX = JogStepIncrements.Step1;
        JogFeedIncrements _jogFeed = JogFeedIncrements.Feed1;
//        [ObservableProperty][NotifyPropertyChangedFor(nameof(FeedRate))]JogFeedIncrements _feedIX = JogFeedIncrements.Feed1;
        private double[] _distance = new double[4];
//        [ObservableProperty] private ObservableCollection<double> _distance = new();
        private int[] _feedRate = new int[4];
//        [ObservableProperty] private ObservableCollection<int> _feedRate = new();

        public void SetMetric(bool on)
        {
            //for (int i = 0; i < FeedRate.Count; i++)
            for (int i = 0; i < AppConfig.Settings.JogUiMetric.Distance.Length; i++)
            {
                _distance[i] = on ? AppConfig.Settings.JogUiMetric.Distance[i] : AppConfig.Settings.JogUiImperial.Distance[i];
                _feedRate[i] = on ? AppConfig.Settings.JogUiMetric.Feedrate[i] : AppConfig.Settings.JogUiImperial.Feedrate[i];
                //Distance.Add(on ? AppConfig.Settings.JogUiMetric.Distance[i] : AppConfig.Settings.JogUiImperial.Distance[i]);
                //FeedRate.Add(on ? AppConfig.Settings.JogUiMetric.Feedrate[i] : AppConfig.Settings.JogUiImperial.Feedrate[i]);

                OnPropertyChanged("Feedrate" + i.ToString());
                OnPropertyChanged("Distance" + i.ToString());
            }
        }

        public JogStepIncrements StepSize
        {
            get { return _jogStep; }
            set { _jogStep = value; OnPropertyChanged(); OnPropertyChanged(nameof(Distance)); }
        }

        //[RelayCommand]
        //private void SetJogStep(JogStepIncrements value)
        //{
        //    _jogStep = value;
        //}


        public double Distance { get { return _distance[(int)_jogStep]; } }
//        public double Distance { get { return _distance[(int)StepSize]; } }
        public JogFeedIncrements Feed { get { return _jogFeed; } set { _jogFeed = value; OnPropertyChanged(); OnPropertyChanged(nameof(FeedRate)); } }
        public double FeedRate { get { return _feedRate[(int)_jogFeed]; } }
//        public double FeedRate { get { return _feedRate[(int)FeedIX]; } }
        public int Feedrate0 { get { return _feedRate[0]; } }
        public int Feedrate1 { get { return _feedRate[1]; } }
        public int Feedrate2 { get { return _feedRate[2]; } }
        public int Feedrate3 { get { return _feedRate[3]; } }
//        public int Feedrate0 { get { return FeedRate[0]; } }
//        public int Feedrate1 { get { return FeedRate[1]; } }
//        public int Feedrate2 { get { return FeedRate[2]; } }
//        public int Feedrate3 { get { return FeedRate[3]; } }


        public double Distance0 { get { return _distance[0]; } }
        public double Distance1 { get { return _distance[1]; } }
        public double Distance2 { get { return _distance[2]; } }
        public double Distance3 { get { return _distance[3]; } }
//        public double Distance0 { get { return Distance[0]; } }
//        public double Distance1 { get { return Distance[1]; } }
//        public double Distance2 { get { return Distance[2]; } }
//        public double Distance3 { get { return Distance[3]; } }




        public void StepInc()
        {
            //if (StepSizeIX != JogStepIncrements.Step3)
            //    StepSizeIX += 1;
            if (_jogStep != JogStepIncrements.Step3)
                _jogStep += 1;
        }
        public void StepDec()
        {
            //if (StepSizeIX != JogStepIncrements.Step0)
            //    StepSizeIX -= 1;
            if (_jogStep != JogStepIncrements.Step0)
                _jogStep -= 1;
        }

        public void FeedInc()
        {
            //if (FeedIX != JogFeedIncrements.Feed3)
            //    FeedIX += 1;
            if (_jogFeed != JogFeedIncrements.Feed3)
                _jogFeed += 1;
        }

        public void FeedDec()
        {
            //if (FeedIX != JogFeedIncrements.Feed0)
            //    FeedIX -= 1;
            if (_jogFeed != JogFeedIncrements.Feed0)
                _jogFeed -= 1;
        }
    }
}
