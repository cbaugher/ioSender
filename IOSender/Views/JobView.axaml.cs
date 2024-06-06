/*
 * JobView.xaml.cs - part of Grbl Code Sender
 *
 * v0.43 / 2023-07-09 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2019-2023, Io Engineering (Terje Io)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

� Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.

� Redistributions in binary form must reproduce the above copyright notice, this
list of conditions and the following disclaimer in the documentation and/or
other materials provided with the distribution.

� Neither the name of the copyright holder nor the names of its contributors may
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
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Utilities;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using Avalonia.Threading;
using CNC.Core;
using CNC.Controls;


namespace IOSender.Views
{

    /// <summary>
    /// Interaction logic for JobView.xaml
    /// </summary>
    public partial class JobView : UserControl, ICNCView
    {
        private bool? initOK = null;
        private bool isBooted = false, isCameraClaimed = false;
        private GrblViewModel model;
        private IInputElement focusedControl = null;
        private Controller Controller = null;
        private IDisposable dataContextChangedRemover;

        public JobView()
        {
            InitializeComponent();

            //DRO.DROEnabledChanged += DRO_DROEnabledChanged;
            DRO.GetPropertyChangedObservable(IsVisibleProperty).AddClassHandler<JobView>(DRO_DROEnabledChanged);
            //DataContextChanged += View_DataContextChanged;
            DataContextProperty.Changed.AddClassHandler<JobView>(JobView_DataContextChanged);

            dataContextChangedRemover = DataContextProperty.Changed.AddClassHandler<JobView>(JobView_DataContextChanged);
        }

        private void JobView_DataContextChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if(e.NewValue is GrblViewModel)
            {
                model = (GrblViewModel)e.NewValue;

                model.PropertyChanged += OnDataContextPropertyChanged;
                //DataContextProperty.Changed.AddClassHandler<JobView>(OnDataContextPropertyChanged);  // Will this work??
                
                dataContextChangedRemover.Dispose();
                
                //ORIG  DataContextChanged -= JobView_DataContextChanged;
            }
        }

        //REMOVE IF FIXED
        //private void View_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //private void View_DataContextChanged(object? sender, EventArgs e)
        //{
            
        //    //if (( e as EventArgs).NewValue is GrblViewModel))
        //    {
        //        //model = (GrblViewModel)e.NewValue;
                
        //        //FIXME  model.PropertyChanged += OnDataContextPropertyChanged;
        //        //FIXME  DataContextChanged -= View_DataContextChanged;
        //        //ORIG          model.OnGrblReset += Model_OnGrblReset;
        //    }
        //}

        private void OnDataContextPropertyChanged(object sender, PropertyChangedEventArgs e)
 //       private void OnDataContextPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (sender is GrblViewModel) switch (e.PropertyName)
//            if (sender is GrblViewModel) switch (e.Property.Name)
                {
                    case nameof(GrblViewModel.GrblState):
                        if (Controller != null && !Controller.ResetPending)
                        {
                            if (initOK == false && isBooted && (sender as GrblViewModel).GrblState.State != GrblStates.Alarm)
                            {
                                //Dispatcher.BeginInvoke(new System.Action(() => InitSystem()), DispatcherPriority.ApplicationIdle);
                                Dispatcher.UIThread.InvokeAsync(new System.Action(() => InitSystem()), DispatcherPriority.ApplicationIdle);
                            }
                        }
                        break;

                    case nameof(GrblViewModel.IsGCLock):
                        GrblView.ui.JobRunning = (sender as GrblViewModel).IsJobRunning;
                        //ORIG             MainWindow.EnableView(!(sender as GrblViewModel).IsGCLock, ViewType.Probing);
                        break;

                    case nameof(GrblViewModel.IsSleepMode):
                        EnableUI(!(sender as GrblViewModel).IsSleepMode);
                        break;

                    case nameof(GrblViewModel.IsJobRunning):
                        GrblView.ui.JobRunning = (sender as GrblViewModel).IsJobRunning;
                        if (GrblInfo.ManualToolChange)
                            GrblCommand.ToolChange = (sender as GrblViewModel).IsJobRunning ? "T{0}M6" : "M61Q{0}";
                        break;

                    case nameof(GrblViewModel.IsToolChanging):
                        GrblView.ui.JobRunning = (sender as GrblViewModel).IsToolChanging || (sender as GrblViewModel).IsJobRunning;
                        break;

                    case nameof(GrblViewModel.Tool):
                        if (GrblInfo.ManualToolChange && (sender as GrblViewModel).Tool != GrblConstants.NO_TOOL)
                            GrblWorkParameters.RemoveNoTool();
                        break;

                    case nameof(GrblViewModel.GrblReset):
                        if ((sender as GrblViewModel).IsReady)
                        {
                            if (!Controller.ResetPending && (sender as GrblViewModel).GrblReset)
                            {
                                initOK = null;
                                //Dispatcher.BeginInvoke(new System.Action(() => Activate(true, ViewType.GRBL)), DispatcherPriority.ApplicationIdle);
                                Dispatcher.UIThread.InvokeAsync(new System.Action(() => Activate(true, ViewType.GRBL)), DispatcherPriority.ApplicationIdle);
                            }
                        }
                        break;

                    case nameof(GrblViewModel.ParserState):
                        if (!Controller.ResetPending && (sender as GrblViewModel).GrblReset)
                        {
                            EnableUI(true);
                            (sender as GrblViewModel).GrblReset = false;
                        }
                        break;

                    case nameof(GrblViewModel.FileName):
                        string filename = (sender as GrblViewModel).FileName;
//FIXME                        GrblView.ui.WindowTitle = filename;

                        if (string.IsNullOrEmpty(filename))
                            GrblView.CloseFile();
                        else if ((sender as GrblViewModel).IsSDCardJob)
                        {
                            GrblView.EnableView(false, ViewType.GCodeViewer);
                        }
                        else if (AppConfig.Settings.GCodeViewer.IsEnabled)
                        {
                            if (filename.StartsWith("Wizard:"))
                            {
                                //ORIG  MainWindow.EnableView(true, ViewType.GCodeViewer);
//                                gcodeRenderer.Open(GCode.File.Tokens);
                            }
                            else if (!string.IsNullOrEmpty(filename))
                            {
                                //ORIG  MainWindow.GCodeViewer.Open(GCode.File.Tokens);
                                //ORIG  MainWindow.EnableView(true, ViewType.GCodeViewer);
                                GCodeSender.EnablePolling(false);
//                                gcodeRenderer.Open(GCode.File.Tokens);
                                GCodeSender.EnablePolling(true);
                            }
                        }
                        break;
                }
        }

        #region Methods and properties required by CNCView interface

        public ViewType ViewType { get { return ViewType.GRBL; } }
        public bool CanEnable { get { return true; } }

        public void Activate(bool activate, ViewType chgMode)
        {
            if (activate)
            {
                GCodeSender.RewindFile();
                GCodeSender.CallHandler(model.IsSDCardJob ? StreamingState.Start : (GCode.File.IsLoaded ? StreamingState.Idle : StreamingState.NoFile), false);

                model.ResponseLogFilterOk = AppConfig.Settings.Base.FilterOkResponse;

                if (Controller == null)
                    Controller = new Controller(model);

                if (initOK != true)
                {
                    focusedControl = this;
                    initOK = false;

                    switch (Controller.Restart())
                    {
                        case Controller.RestartResult.Ok:
                            if (!isBooted)
                                //Dispatcher.BeginInvoke(new System.Action(() => OnBooted()), DispatcherPriority.ApplicationIdle);
                                Dispatcher.UIThread.InvokeAsync(new System.Action(() => OnBooted()), DispatcherPriority.ApplicationIdle);
                            initOK = InitSystem();
                            break;

                        case Controller.RestartResult.Close:
//SHOULD CLOSE APP?                            GrblView.ui.Close();
                            break;

                        case Controller.RestartResult.Exit:
                            Environment.Exit(-1);
                            break;
                    }

                    model.Message = Controller.Message;
                }

#if ADD_CAMERA
                if (MainWindow.UIViewModel.Camera != null && !isCameraClaimed)
                {
                    MainWindow.UIViewModel.Camera.MoveOffset += Camera_MoveOffset;
                    MainWindow.UIViewModel.Camera.IsVisibilityChanged += Camera_Opened;
                    MainWindow.UIViewModel.Camera.IsMoveEnabled = isCameraClaimed = true;
                }
#endif
                //ORIG  if (viewer == null)
                //    viewer = new Viewer();

//FIXME                if (GCode.File.IsLoaded)
//                    GrblView.ui.WindowTitle = ((GrblViewModel)DataContext).FileName;

                model.Keyboard.JogStepDistance = AppConfig.Settings.Jog.LinkStepJogToUI ? AppConfig.Settings.JogUiMetric.Distance0 : AppConfig.Settings.Jog.StepDistance;
                model.Keyboard.JogDistances[(int)KeypressHandler.JogMode.Slow] = AppConfig.Settings.Jog.SlowDistance;
                model.Keyboard.JogDistances[(int)KeypressHandler.JogMode.Fast] = AppConfig.Settings.Jog.FastDistance;
                model.Keyboard.JogFeedrates[(int)KeypressHandler.JogMode.Step] = AppConfig.Settings.Jog.StepFeedrate;
                model.Keyboard.JogFeedrates[(int)KeypressHandler.JogMode.Slow] = AppConfig.Settings.Jog.SlowFeedrate;
                model.Keyboard.JogFeedrates[(int)KeypressHandler.JogMode.Fast] = AppConfig.Settings.Jog.FastFeedrate;

                model.Keyboard.IsJoggingEnabled = AppConfig.Settings.Jog.Mode != JogConfig.JogMode.UI;

                if (!GrblInfo.IsGrblHAL)
                    model.Keyboard.IsContinuousJoggingEnabled = AppConfig.Settings.Jog.KeyboardEnable;
            }
            else if (ViewType != ViewType.Shutdown)
            {
                DRO.IsFocusable = false;
#if ADD_CAMERA
                if (MainWindow.UIViewModel.Camera != null) {
                    MainWindow.UIViewModel.Camera.MoveOffset-= Camera_MoveOffset;
                    MainWindow.UIViewModel.Camera.IsMoveEnabled = isCameraClaimed = false;
                }
#endif
//FIXME                focusedControl = AppConfig.Settings.Base.KeepMdiFocus &&
//                                  Keyboard.FocusedElement is TextBox &&
//                                   (Keyboard.FocusedElement as TextBox).Tag is string &&
//                                    (string)(Keyboard.FocusedElement as TextBox).Tag == "MDI"
//                                  ? Keyboard.FocusedElement
//                                  : this;
            }

            if (GCodeSender.Activate(activate))
            {
                showProgramLimits();
                Task.Delay(500).ContinueWith(t => DRO.EnableFocus());
                //Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                Dispatcher.UIThread.InvokeAsync(new System.Action(() =>
                {
                    focusedControl.Focus();
                }), DispatcherPriority.Render);
            }
        }

        public void CloseFile()
        {
//            gcodeRenderer.Close();
        }

        public void Setup(UIViewModel model, AppConfig profile)
        {
        }

        #endregion

        // https://stackoverflow.com/questions/5707143/how-to-get-the-width-height-of-a-collapsed-control-in-wpf
        private void showProgramLimits()
        {
            double height;

            if (limitsControl.IsVisible == false)
            {
                limitsControl.IsVisible = false;
                limitsControl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                height = limitsControl.DesiredSize.Height;
                limitsControl.IsVisible = false;
            }
            else
                height = limitsControl.Bounds.Height; // Bounds.Height replaces WPF's ActualHeight

            limitsControl.IsVisible = (dp.Bounds.Height - t1.Bounds.Height - t2.Bounds.Height + limitsControl.Bounds.Height) > height ? true : false;
        }

#if ADD_CAMERA
        void Camera_Opened()
        {
            model.IsCameraVisible = MainWindow.UIViewModel.Camera.IsVisible;
            Focus();
        }

        void Camera_MoveOffset(CameraMoveMode Mode, double XOffset, double YOffset)
        {
            GrblParserState.Get();
            CNC.GCode.Units units = GrblParserState.Units;
            CNC.GCode.DistanceMode distanceMode = GrblParserState.DistanceMode;

            Comms.com.WriteString("G91G21G0\r"); // Enter relative metric G0 mode - set scale to 1.0?

            switch (Mode)
            {
                case CameraMoveMode.XAxisFirst:
                    Comms.com.WriteString(string.Format("X{0}\r", XOffset.ToInvariantString("F3")));
                    Comms.com.WriteString(string.Format("Y{0}\r", YOffset.ToInvariantString("F3")));
                    break;

                case CameraMoveMode.YAxisFirst:
                    Comms.com.WriteString(string.Format("Y{0}\r", YOffset.ToInvariantString("F3")));
                    Comms.com.WriteString(string.Format("X{0}\r", XOffset.ToInvariantString("F3")));
                    break;

                case CameraMoveMode.BothAxes:
                    ((GrblViewModel)DataContext).ExecuteCommand(string.Format("X{0}Y{1}", XOffset.ToInvariantString("F3"), YOffset.ToInvariantString("F3")));
                    break;
            }

            if(distanceMode != CNC.GCode.DistanceMode.Incremental)
                Comms.com.WriteString("G90\r");

            if (units != CNC.GCode.Units.Metric)
                Comms.com.WriteString("G20\r");
        }
#endif

        private void OnBooted()
        {
            isBooted = true;
            string filename = CNC.Core.Resources.Path + string.Format("KeyMap{0}.xml", (int)AppConfig.Settings.Jog.Mode);

            if (System.IO.File.Exists(filename))
                model.Keyboard.LoadMappings(filename);

//FIXME            if (GrblInfo.NumAxes > 3)
//                GCode.File.AddTransformer(typeof(GCodeWrapViewModel), "Wrap to rotary (WIP)", GrblView.UIViewModel.TransformMenuItems);
        }

        bool InitSystem()
        {
            initOK = true;
            int timeout = 5;

            using (new UIUtils.WaitCursor())
            {
                GCodeSender.EnablePolling(false);
                while (!GrblInfo.Get())
                {
                    if (--timeout == 0)
                    {
                        model.Message = (string)Resources["MsgNoResponse"];
                        return false;
                    }
                    Thread.Sleep(500);
                }
                GrblAlarms.Get();
                GrblErrors.Get();
                GrblSettings.Load();
                if (GrblInfo.IsGrblHAL)
                {
                    GrblParserState.Get();
                    GrblWorkParameters.Get();
                }
                else
                    GrblParserState.Get(true);
                GCodeSender.EnablePolling(true);
            }

            GrblCommand.ToolChange = GrblInfo.ManualToolChange ? "M61Q{0}" : "T{0}";

            showProgramLimits();

            if (!AppConfig.Settings.GCodeViewer.IsEnabled)
                tabGCode.Items.Remove(tab3D);

            if (GrblInfo.LatheModeEnabled)
            {
                GrblView.EnableView(true, ViewType.Turning);
                GrblView.EnableView(true, ViewType.Parting);
                GrblView.EnableView(true, ViewType.Facing);
                GrblView.EnableView(true, ViewType.G76Threading);
            }
            else
            {
                GrblView.ShowView(false, ViewType.Turning);
                GrblView.ShowView(false, ViewType.Parting);
                GrblView.ShowView(false, ViewType.Facing);
                GrblView.ShowView(false, ViewType.G76Threading);
            }

            if (GrblInfo.HasSDCard)
                GrblView.EnableView(true, ViewType.SDCard);
            else
                GrblView.ShowView(false, ViewType.SDCard);

            if (GrblInfo.HasPIDLog)
                GrblView.EnableView(true, ViewType.PIDTuner);
            else
                GrblView.ShowView(false, ViewType.PIDTuner);

            if (GrblInfo.NumTools > 0)
                GrblView.EnableView(true, ViewType.Tools);
            else
                GrblView.ShowView(false, ViewType.Tools);

            if (GrblInfo.HasProbe && GrblSettings.ReportProbeCoordinates)
                GrblView.EnableView(true, ViewType.Probing);

            GrblView.EnableView(true, ViewType.Offsets);
            GrblView.EnableView(true, ViewType.GRBLConfig);

            if (!string.IsNullOrEmpty(GrblInfo.TrinamicDrivers))
                GrblView.EnableView(true, ViewType.TrinamicTuner);
            else
                GrblView.ShowView(false, ViewType.TrinamicTuner);

            return true;
        }

        void EnableUI(bool enable)
        {
            foreach (UserControl control in UIUtils.FindFirstLogicalChildren<UserControl>(this))
            {
                if (control.Name != nameof(statusControl))
                    control.IsEnabled = enable;
            }
            // disable ui components when in sleep mode
        }
        #region UIevents

        void JobView_Load(object sender, EventArgs e)
        {
            GCodeSender.CallHandler(StreamingState.Idle, true);
        }

        //private void JobView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        private void JobView_IsVisibleChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (IsVisible)
                GCodeSender.Focus();
        }

        private void JobView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (GrblInfo.IsLoaded)
                showProgramLimits();
        }

        //private void outside_MouseDown(object sender, MouseButtonEventArgs e)
        private void outside_MouseDown(object sender, PointerPressedEventArgs e)
        {
            Focus();
        }

        //void DRO_DROEnabledChanged(bool enabled)
        void DRO_DROEnabledChanged(JobView jv, AvaloniaPropertyChangedEventArgs e)
        {
            //REMOVE IF WORKS
            //if (!enabled)
            //    Focus();
            
            if(!(bool)e.NewValue)
                Focus();
        }

        //protected override void OnPreviewKeyDown(KeyEventArgs e)
        //{
        //    if (!(e.Handled = ProcessKeyPreview(e)))
        //    {
        //        if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
        //            Focus();

        //        base.OnPreviewKeyDown(e);
        //    }
        //}
        //protected override void OnPreviewKeyUp(KeyEventArgs e)
        //{
        //    if (!(e.Handled = ProcessKeyPreview(e)))
        //        base.OnPreviewKeyDown(e);
        //}

        //protected bool ProcessKeyPreview(KeyEventArgs e)
        //{
        //    return model.Keyboard.ProcessKeypress(e, !(mdiControl.IsFocused || DRO.IsFocused || spindleControl.IsFocused || workParametersControl.IsFocused), this);
        //}

        #endregion
    }
}

