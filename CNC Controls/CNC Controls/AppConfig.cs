﻿/*
 * AppConfig.cs - part of CNC Controls library
 *
 * v0.43 / 2023-07-21 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2019-2023, Io Engineering (Terje Io)
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
using System.IO;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using Avalonia.Media;
using System.Threading;
using Avalonia.Threading;
using Avalonia.Controls;
using CNC.Core;
using CNC.GCode;
using static CNC.GCode.GCodeParser;
using System.Collections.Generic;
using Avalonia.Markup.Xaml.Styling;
using RP.Math;
using Avalonia;
//using MsgBox;
using Avalonia.Controls.ApplicationLifetimes;
using System.Net.WebSockets;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Avalonia.Styling;

namespace CNC.Controls
{
    public static class LibStrings
    {
        private static readonly ResourceDictionary Resource;

        static LibStrings()
        {
            Resource = new ResourceDictionary();
            try
            {
                var uri = new Uri("avares://CNC.Controls/Resources/LibStrings.axaml");
                var include = new ResourceInclude(uri)
                {
                    Source = uri
                };
                Resource.MergedDictionaries.Add(include);
            }
            catch { }
        }

        public static string? FindResource(string key)
        {
            //Original code
            //resource.TryGetValue(key, out object? value);
            //return (value as string) ?? string.Empty;
            
            if(Resource.TryGetValue(key, out var val1))
                return val1 as string;

            foreach(var dict in Resource.MergedDictionaries)
            {
                if(dict.TryGetResource(key, Application.Current.ActualThemeVariant, out var val2))
                    return val2 as string;
            }

            return string.Empty;
        }
    }

    [Serializable]
    public class LatheConfig : ViewModelBase
    {
        private bool _isEnabled = false;
        private LatheMode _latheMode = LatheMode.Disabled;

        [XmlIgnore]
        public double ZDirFactor { get { return ZDirection == Direction.Negative ? -1d : 1d; } }

        [XmlIgnore]
        public LatheMode[] LatheModes { get { return (LatheMode[])Enum.GetValues(typeof(LatheMode)); } }

        [XmlIgnore]
        public Direction[] ZDirections { get { return (Direction[])Enum.GetValues(typeof(Direction)); } }

        [XmlIgnore]
        public bool IsEnabled { get { return _isEnabled; } set { _isEnabled = value; OnPropertyChanged(); } }

        public LatheMode XMode { get { return _latheMode; } set { _latheMode = value; IsEnabled = value != LatheMode.Disabled; } }
        public Direction ZDirection { get; set; } = Direction.Negative;
        public double PassDepthLast { get; set; } = 0.02d;
        public double FeedRate { get; set; } = 300d;
    }

    [Serializable]
    public class ProbeConfig : ViewModelBase
    {
        private bool _CheckProbeStatus = true;
        private bool _ValidateProbeConnected = false;

        public bool CheckProbeStatus { get { return _CheckProbeStatus; } set { _CheckProbeStatus = value; OnPropertyChanged(); } }
        public bool ValidateProbeConnected { get { return _ValidateProbeConnected; } set { _ValidateProbeConnected = value; OnPropertyChanged(); } }
    }

    [Serializable]
    public class CameraConfig : ViewModelBase
    {
        private string _camera = string.Empty;
        private double _xoffset = 0d, _yoffset = 0d;
        private int _guideScale = 10;
        private bool _moveToSpindle = false, _confirmMove = false;
        private CameraMoveMode _moveMode = CameraMoveMode.BothAxes;

        [XmlIgnore]
        internal bool IsDirty { get; set; } = false;

        [XmlIgnore]
        public CameraMoveMode[] MoveModes { get { return (CameraMoveMode[])Enum.GetValues(typeof(CameraMoveMode)); } }

        public string SelectedCamera { get { return _camera; } set { _camera = value; IsDirty = true; OnPropertyChanged(); } }
        public double XOffset { get { return _xoffset; } set { _xoffset = value; OnPropertyChanged(); } }
        public double YOffset { get { return _yoffset; } set { _yoffset = value; OnPropertyChanged(); } }
        public int GuideScale { get { return _guideScale; } set { _guideScale = value; IsDirty = true; OnPropertyChanged(); } }
        public bool InitialMoveToSpindle { get { return _moveToSpindle; } set { _moveToSpindle = value; IsDirty = true; OnPropertyChanged(); } }
        public bool ConfirmMove { get { return _confirmMove; } set { _confirmMove = value; IsDirty = true; OnPropertyChanged(); } }
        public CameraMoveMode MoveMode { get { return _moveMode; } set { _moveMode = value; OnPropertyChanged(); } }
    }

    [Serializable]
    public class GCodeViewerConfig : ViewModelBase
    {
        private bool _isEnabled = true;
        private int _arcResolution = 10;
        private double _minDistance = 0.05d, _toolDiameter = 3d;
        private bool _showGrid = true, _showAxes = true, _showBoundingBox = false, _showViewCube = true, _showCoordSystem = false, _showWorkEnvelope = false;
        private bool _showTextOverlay = false, _renderExecuted = false, _blackBackground = false, _scaleTool = true;
        Color _cutMotion = Colors.Black, _rapidMotion = Colors.LightPink, _retractMotion = Colors.Green, _toolOrigin = Colors.Green, _grid = Colors.Gray, _highlight = Colors.Crimson;

        [XmlIgnore]
        public bool IsHomingEnabled { get { return _isEnabled && GrblInfo.HomingEnabled; } set { OnPropertyChanged(); } }

        public bool IsEnabled { get { return _isEnabled; } set { _isEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsHomingEnabled)); } }
        public int ArcResolution { get { return _arcResolution; } set { _arcResolution = value; OnPropertyChanged(); } }
        public double MinDistance { get { return _minDistance; } set { _minDistance = value; OnPropertyChanged(); } }
        public bool ToolAutoScale { get { return _scaleTool; } set { _scaleTool = value; OnPropertyChanged(); } }
        public double ToolDiameter { get { return _toolDiameter; } set { _toolDiameter = value; OnPropertyChanged(); } }
        public bool ShowGrid { get { return _showGrid; } set { _showGrid = value; OnPropertyChanged(); } }
        public bool ShowAxes { get { return _showAxes; } set { _showAxes = value; OnPropertyChanged(); } }
        public bool ShowBoundingBox { get { return _showBoundingBox; } set { _showBoundingBox = value; OnPropertyChanged(); } }
        public bool ShowWorkEnvelope { get { return _showWorkEnvelope && GrblInfo.HomingEnabled; } set { _showWorkEnvelope = value; OnPropertyChanged(); } }
        public bool ShowViewCube { get { return _showViewCube; } set { _showViewCube = value; OnPropertyChanged(); } }
        public bool ShowTextOverlay { get { return _showTextOverlay; } set { _showTextOverlay = value; OnPropertyChanged(); } }
        public bool ShowCoordinateSystem { get { return _showCoordSystem; } set { _showCoordSystem = value; OnPropertyChanged(); } }
        public bool RenderExecuted { get { return _renderExecuted; } set { _renderExecuted = value; OnPropertyChanged(); } }
        public bool BlackBackground { get { return _blackBackground; } set { _blackBackground = value; OnPropertyChanged(); } }
        public Color CutMotionColor { get { return _cutMotion; } set { _cutMotion = value; OnPropertyChanged(); } }
        public Color RapidMotionColor { get { return _rapidMotion; } set { _rapidMotion = value; OnPropertyChanged(); } }
        public Color RetractMotionColor { get { return _retractMotion; } set { _retractMotion = value; OnPropertyChanged(); } }
        public Color ToolOriginColor { get { return _toolOrigin; } set { _toolOrigin = value; OnPropertyChanged(); } }
        public Color GridColor { get { return _grid; } set { _grid = value; OnPropertyChanged(); } }
        public Color HighlightColor { get { return _highlight; } set { _highlight = value; OnPropertyChanged(); } }
        public int ViewMode { get; set; } = -1;
        public int ToolVisualizer { get; set; } = 1;
        public Point3D CameraPosition { get; set; }
        //public Vector3D CameraLookDirection { get; set; }
        //public Vector3D CameraUpDirection { get; set; }
        public Vector3 CameraLookDirection { get; set; }
        public Vector3 CameraUpDirection { get; set; }
    }

    [Serializable]
    public class JogUIConfig : ViewModelBase
    {
        private int[] _feedrate = new int[4];
        private double[] _distance = new double[4];

        public JogUIConfig()
        {
        }

        public JogUIConfig(int[] feedrate, double[] distance)
        {
            for(int i = 0; i < feedrate.Length; i++)
            {
                _feedrate[i] = feedrate[i];
                _distance[i] = distance[i];
            }
        }

        [XmlIgnore]
        public int[] Feedrate { get { return _feedrate; } }
        public int Feedrate0 { get { return _feedrate[0]; } set { _feedrate[0] = value; OnPropertyChanged(); } }
        public int Feedrate1 { get { return _feedrate[1]; } set { _feedrate[1] = value; OnPropertyChanged(); } }
        public int Feedrate2 { get { return _feedrate[2]; } set { _feedrate[2] = value; OnPropertyChanged(); } }
        public int Feedrate3 { get { return _feedrate[3]; } set { _feedrate[3] = value; OnPropertyChanged(); } }

        [XmlIgnore]
        public double[] Distance { get { return _distance; } }
        public double Distance0 { get { return _distance[0]; } set { _distance[0] = value; OnPropertyChanged(); } }
        public double Distance1 { get { return _distance[1]; } set { _distance[1] = value; OnPropertyChanged(); } }
        public double Distance2 { get { return _distance[2]; } set { _distance[2] = value; OnPropertyChanged(); } }
        public double Distance3 { get { return _distance[3]; } set { _distance[3] = value; OnPropertyChanged(); } }
    }

    [Serializable]
    public class JogConfig : ViewModelBase
    {
        public enum JogMode : int
        {
            UI = 0,
            Keypad,
            KeypadAndUI
        }

        private bool _kbEnable, _linkStepToUi = true;
        private JogMode _jogMode = JogMode.UI;

        private double _fastFeedrate = 500d, _slowFeedrate = 200d, _stepFeedrate = 100d;
        private double _fastDistance = 500d, _slowDistance = 500d, _stepDistance = 0.05d;

        public JogMode Mode { get { return _jogMode; } set { _jogMode = value; OnPropertyChanged(); } }
        public bool KeyboardEnable { get { return _kbEnable; } set { _kbEnable = value; OnPropertyChanged(); } }
        public bool LinkStepJogToUI { get { return _linkStepToUi; } set { _linkStepToUi = value; OnPropertyChanged(); } }
        public double FastFeedrate { get { return _fastFeedrate; } set { _fastFeedrate = value; OnPropertyChanged(); } }
        public double SlowFeedrate { get { return _slowFeedrate; } set { _slowFeedrate = value; OnPropertyChanged(); } }
        public double StepFeedrate { get { return _stepFeedrate; } set { _stepFeedrate = value; OnPropertyChanged(); } }
        public double FastDistance { get { return _fastDistance; } set { _fastDistance = value; OnPropertyChanged(); } }
        public double SlowDistance { get { return _slowDistance; } set { _slowDistance = value; OnPropertyChanged(); } }
        public double StepDistance { get { return _stepDistance; } set { _stepDistance = value; OnPropertyChanged(); } }
    }

    [Serializable]
    public class Macros : ViewModelBase
    {
        public ObservableCollection<CNC.GCode.Macro> Macro { get; private set; } = new ObservableCollection<CNC.GCode.Macro>();
    }

    [Serializable]
    public class Config : ViewModelBase
    {
        private int _pollInterval = 200, /* ms*/  _maxBufferSize = 300;
        private bool _useBuffering = false, _keepMdiFocus = true, _filterOkResponse = false, _saveWindowSize = false, _autoCompress = false;
        private CommandIgnoreState _ignoreM6 = CommandIgnoreState.No, _ignoreM7 = CommandIgnoreState.No, _ignoreM8 = CommandIgnoreState.No, _ignoreG61G64 = CommandIgnoreState.Strip;
        private string _theme = "default";

        [XmlIgnore]
        public Dictionary<string, string> Themes { get; private set; } = new Dictionary<string, string>();

        public string Theme
        {
            get { return _theme; }
            set {
                _theme = value; //.Substring(0, 1).ToUpper() + value.Substring(1);
                //FIXME  Properties.Settings.Default.ColorMode = value; // value.Substring(0, 1).ToUpper() + value.Substring(1);
                //Properties.Settings.Default.Save();  // FIXME
                OnPropertyChanged();
            }
        }
        public int PollInterval { get { return _pollInterval < 100 ? 100 : _pollInterval; } set { _pollInterval = value; OnPropertyChanged(); } }
        public string PortParams { get; set; } = "COMn:115200,N,8,1";
        public int ResetDelay { get; set; } = 2000;
        public bool UseBuffering { get { return _useBuffering; } set { _useBuffering = value; OnPropertyChanged(); } }
        public bool KeepWindowSize { get { return _saveWindowSize; } set { if (_saveWindowSize != value) { _saveWindowSize = value; OnPropertyChanged(); } } }
        public double WindowWidth { get; set; } = 925;
        public double WindowHeight { get; set; } = 660;
        public int OutlineFeedRate { get; set; } = 500;
        public int MaxBufferSize { get { return _maxBufferSize < 300 ? 300 : _maxBufferSize; } set { _maxBufferSize = value; OnPropertyChanged(); } }
        public string Editor { get; set; } = "notepad.exe";
        public bool KeepMdiFocus { get { return _keepMdiFocus; } set { _keepMdiFocus = value; OnPropertyChanged(); } }
        public bool FilterOkResponse { get { return _filterOkResponse; } set { _filterOkResponse = value; OnPropertyChanged(); } }
        public bool AutoCompress { get { return _autoCompress; } set { _autoCompress = value; OnPropertyChanged(); } }

        [XmlIgnore]
        public CommandIgnoreState[] CommandIgnoreStates { get { return (CommandIgnoreState[])Enum.GetValues(typeof(CommandIgnoreState)); } }
        public CommandIgnoreState IgnoreM6 { get { return _ignoreM6; } set { _ignoreM6 = value; OnPropertyChanged(); } }
        public CommandIgnoreState IgnoreM7 { get { return _ignoreM7; } set { _ignoreM7 = value; OnPropertyChanged(); } }
        public CommandIgnoreState IgnoreM8 { get { return _ignoreM8; } set { _ignoreM8 = value; OnPropertyChanged(); } }
        public CommandIgnoreState IgnoreG61G64 { get { return _ignoreG61G64; } set { _ignoreG61G64 = value; OnPropertyChanged(); } }
        public ObservableCollection<CNC.GCode.Macro> Macros { get; set; } = new ObservableCollection<CNC.GCode.Macro>();
        public JogConfig Jog { get; set; } = new JogConfig();
        public JogUIConfig JogUiMetric { get; set; } = new JogUIConfig(new int[4] { 5, 100, 500, 1000 }, new double[4] { .01d, .1d, 1d, 10d });
        public JogUIConfig JogUiImperial { get; set; } = new JogUIConfig(new int[4] { 5, 10, 50, 100 }, new double[4] { .001d, .01d, .1d, 1d });

        public LatheConfig Lathe { get; set; } = new LatheConfig();
        public CameraConfig Camera { get; set; } = new CameraConfig();
        public GCodeViewerConfig GCodeViewer { get; set; } = new GCodeViewerConfig();
        public ProbeConfig Probing { get; set; } = new ProbeConfig();
    }

    public class AppConfig : ViewModelBase
    {
        private string? configfile = null;
        private bool? MPGactive = null;

        public string FileName { get;
            private set; }

        private static readonly Lazy<AppConfig> settings = new Lazy<AppConfig>(() => new AppConfig());

        private AppConfig()
        {
            //Properties.Settings.Default.PropertyChanged += Default_PropertyChanged;  // FIXME
            
        }

        private void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //OnPropertyChanged(nameof(ColorMode));   
        }

        public static AppConfig Settings { get { return settings.Value; } }

        //public static string ColorMode { get { return Properties.Settings.Default.ColorMode; } }
        //public static string ColorMode { get { return Properties.Settings.Default.ColorMode; } }

        public Config? Base { get; private set; } = null;

        public ObservableCollection<CNC.GCode.Macro>? Macros { get { return Base == null ? null : Base.Macros; } }
        public JogConfig? Jog { get { return Base == null ? null : Base.Jog; } }
        public JogUIConfig? JogUiMetric { get { return Base == null ? null : Base.JogUiMetric; } }
        public JogUIConfig? JogUiImperial { get { return Base == null ? null : Base.JogUiImperial; } }

        public CameraConfig? Camera { get { return Base == null ? null : Base.Camera; } }
        public LatheConfig? Lathe { get { return Base == null ? null : Base.Lathe; } }
        public GCodeViewerConfig? GCodeViewer { get { return Base == null ? null : Base.GCodeViewer; } }
        public ProbeConfig? Probing { get { return Base == null ? null : Base.Probing; } }

        public bool Save(string filename)
        {
            bool ok = false;

            if (Base == null)
                Base = new Config();

            XmlSerializer xs = new XmlSerializer(typeof(Config));

            try
            {
                FileStream fsout = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
                using (fsout)
                {
                    xs.Serialize(fsout, Base);
                    configfile = filename;
                    ok = true;
                }
            }
            catch
            {
            }

            return ok;
        }

        public bool Save()
        {
            Camera.IsDirty = false;
            return configfile != null && Save(configfile);
        }

        public bool Load(string filename)
        {
            bool ok = false;
            XmlSerializer xs = new XmlSerializer(typeof(Config));

            try
            {
                StreamReader reader = new StreamReader(filename);
                Base = xs.Deserialize(reader) as Config;
                reader.Close();
                configfile = filename;

                // temp hack...
                foreach (var macro in Base.Macros)
                {
                    if (macro.IsSession)
                        Base.Macros.Remove(macro);
                }

                ok = true;
            }
            catch
            {
            }

            return ok;
        }

        public void Shutdown()
        {
            if (Camera.IsDirty)
                Save();
        }

        private bool isComPort(string port)
        {
            return !(port.ToLower().StartsWith("ws://") || char.IsDigit(port[0]));
        }

        private void setPort(string port, string baud)
        {
            if (isComPort(port) && port.IndexOf(':') == -1)
            {
                string prop = string.Format(":{0},N,8,1", string.IsNullOrEmpty(baud) ? "115200" : baud);
                string[] values = port.Split('!');
                if (isComPort(Base.PortParams))
                {
                    var props = Base.PortParams.Substring(Base.PortParams.IndexOf(':')).Split(',');
                    if(props.Length >= 4)
                        prop = string.Format(":{0},{1},{2},{3}", (string.IsNullOrEmpty(baud) ? props[0] : baud), props[1], props[2], props[3]);
                }
                port = values[0] + prop + (values.Length > 1 ? ",," + values[1] : "");
            }
            Base.PortParams = port;
        }

        //public int SetupAndOpen(string appname, GrblViewModel model, System.Windows.Threading.Dispatcher dispatcher)
        public int SetupAndOpen(string appname, GrblViewModel model, Avalonia.Threading.Dispatcher dispatcher)
        //public int SetupAndOpen(string appname, GrblViewModel model)
        {
            int status = 0;
            bool selectPort = false;
            int jogMode = -1;
            string port = string.Empty, baud = string.Empty;

            CNC.Core.Resources.Path = AppDomain.CurrentDomain.BaseDirectory;

            string[] args = Environment.GetCommandLineArgs();

            int p = 0;
/*
            while (p < args.GetLength(0)) switch (args[p++].ToLowerInvariant())
                {
                    case "-inifile":
                        CNC.Core.Resources.IniName = GetArg(args, p++);
                        break;

                    case "-debugfile":
                        CNC.Core.Resources.DebugFile = GetArg(args, p++);
                        break;

                    case "-configmapping":
                        CNC.Core.Resources.ConfigName = GetArg(args, p++);
                        break;

                    case "-locale":
                    case "-language": // deprecated
                        CNC.Core.Resources.Locale = GetArg(args, p++);
                        break;

                    case "-port":
                        port = GetArg(args, p++);
                        break;

                    case "-baud":
                        baud = GetArg(args, p++);
                        break;

                    case "-selectport":
                        selectPort = true;
                        break;

                    case "-islegacy":
                        CNC.Core.Resources.IsLegacyController = true;
                        break;

                    case "-jogmode":
                        if (int.TryParse(GetArg(args, p++), out jogMode))
                            jogMode = Math.Min(Math.Max(jogMode, 0), (int)JogConfig.JogMode.KeypadAndUI);
                        break;

                    default:
                        //FIXME  DEFAULT SHOULD NOT OPEN JUST ANY FILE WITHOUT CHECKING ITS TYPE
                        //if (!args[p - 1].EndsWith(".exe") && File.Exists(args[p - 1]))
                        //    FileName = args[p - 1];
                        break;
                }
  */          
            if (!Load(CNC.Core.Resources.IniFile))
            {
                //if (MessageBox.Show(LibStrings.FindResource("CreateConfig"), appname, MessageBoxButtons.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
//                if (MsgBox.Show((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow, LibStrings.FindResource("CreateConfig"), appname, MessageBox.MessageBoxButtons.YesNo) == MessageBox.MessageBoxResult.Yes)
//                {
//                    if (!Save(CNC.Core.Resources.IniFile))
//                    {
//                        MessageBox.Show(LibStrings.FindResource("CreateConfigFail"), appname);
//                        status = 1;
//                    }
//                }
//                else
//                    return 1;
            }

//            Base.Themes.Add("Standard", LibStrings.FindResource("ThemeDefault"));
//            Base.Themes.Add("Black", LibStrings.FindResource("ThemeBlack"));
//            Base.Themes.Add("Dark", LibStrings.FindResource("ThemeDark"));
//            Base.Themes.Add("Light", LibStrings.FindResource("ThemeLight"));
//            Base.Themes.Add("White", LibStrings.FindResource("ThemeWhite"));

            if (jogMode != -1)
                Base.Jog.Mode = (JogConfig.JogMode)jogMode;

            if (!string.IsNullOrEmpty(port))
                selectPort = false;

            if (!selectPort)
            {
                if (!string.IsNullOrEmpty(port))
                    setPort(port, baud);
#if USEWEBSOCKET
                if (Base.PortParams.ToLower().StartsWith("ws://"))
                //new WebsocketStream(Base.PortParams, dispatcher);
                { }//FIXME new WebSocket(Base.PortParams, dispatcher);
                else
#endif
                if (char.IsDigit(Base.PortParams[0])) // We have an IP address
                    new TelnetStream(Base.PortParams, dispatcher);
                else
#if USEELTIMA
                    new EltimaStream(Config.PortParams, Config.ResetDelay, dispatcher);
#else
                    new SerialStream(Base.PortParams, Base.ResetDelay, dispatcher);
#endif
            }

            if ((Comms.com == null || !Comms.com.IsOpen) && string.IsNullOrEmpty(port))
            {
                //PortDialog portsel = new PortDialog();  //FIXME

                //port = portsel.ShowDialog(Base.PortParams);
                port = "COM4:115200,N,8,1";  //FIXME
                if (string.IsNullOrEmpty(port))
                    status = 2;

                else
                {
                    setPort(port, string.Empty);
#if USEWEBSOCKET
                    if (port.ToLower().StartsWith("ws://"))
                    { }//FIXME  new WebsocketStream(Base.PortParams, dispatcher);
                    else
#endif
                    if (char.IsDigit(port[0])) // We have an IP address
                        new TelnetStream(Base.PortParams, dispatcher);
                    else
#if USEELTIMA
                        new EltimaStream(Config.PortParams, Config.ResetDelay, dispatcher);
#else
                        new SerialStream(Base.PortParams, Base.ResetDelay, dispatcher);
#endif
                    Save(CNC.Core.Resources.IniFile);
                }
            }

            if (Comms.com != null && Comms.com.IsOpen)
            {
                Comms.com.DataReceived += model.DataReceived;

                CancellationToken cancellationToken = new CancellationToken();

                // Wait 400ms to see if a MPG is polling Grbl...

                new Thread(() =>
                {
                    MPGactive = WaitFor.SingleEvent<string>(
                    cancellationToken,
                    null,
                    a => model.OnRealtimeStatusProcessed += a,
                    a => model.OnRealtimeStatusProcessed -= a,
                    500);
                }).Start();

                while (MPGactive == null)
                    EventUtils.DoEvents();

                if (MPGactive == true)
                {
                    MPGactive = null;

                    new Thread(() =>
                    {
                        MPGactive = WaitFor.SingleEvent<string>(
                        cancellationToken,
                        null,
                        a => model.OnRealtimeStatusProcessed += a,
                        a => model.OnRealtimeStatusProcessed -= a,
                        500, () => Comms.com.WriteByte(GrblConstants.CMD_STATUS_REPORT_ALL));
                    }).Start();

                    while (MPGactive == null)
                        EventUtils.DoEvents();

                    if (MPGactive == true)
                    {
                        if (model.IsMPGActive != true && model.AutoReportingEnabled)
                        {
                            MPGactive = false;
                            if (model.AutoReportInterval > 0)
                                Comms.com.WriteByte(GrblConstants.CMD_AUTO_REPORTING_TOGGLE);
                        }
                    }
                }

                // ...if so show dialog for wait for it to stop polling and relinquish control.
                if (MPGactive == true)
                {
                //FIXME    MPGPending await = new MPGPending(model);
                //    await.ShowDialog();
                //    if (await.Cancelled)
                //    {
                //        Comms.com.Close(); //!!
                //        status = 2;
                //    }
                }

                model.IsReady = true;
            }
            else if (status != 2)
            {
                // FIXME  MessageBox.Show(string.Format(LibStrings.FindResource("ConnectFailed"), Base.PortParams), appname, MessageBoxButton.OK, MessageBoxImage.Error);
                status = 2;
            }

            return status;
        }

        private string GetArg(string[] args, int i)
        {
            return i < args.GetLength(0) ? args[i] : null;
        }
    }

    public class Controller
    {
        GrblViewModel model;

        public enum RestartResult
        {
            Ok = 0,
            NoResponse,
            Close,
            Exit
        }

        public Controller (GrblViewModel model)
        {
            this.model = model;
        }

        public bool ResetPending { get; private set; } = false;
        public string Message { get; private set; }

        public RestartResult Restart ()
        {
            Message = model.Message;
            model.Message = string.Format(LibStrings.FindResource("MsgWaiting"), AppConfig.Settings.Base.PortParams);

            string response = GrblInfo.Startup(model);

            if (response.StartsWith("<"))
            {
                if (model.GrblState.State != GrblStates.Unknown)
                {
                    switch (model.GrblState.State)
                    {
                        case GrblStates.Alarm:

                            model.Poller.SetState(AppConfig.Settings.Base.PollInterval);

                            if (!model.SysCommandsAlwaysAvailable) switch(model.GrblState.Substate)
                            {
                                case 1: // Hard limits
                                    if (!GrblInfo.IsLoaded)
                                    {
                                        if (model.LimitTriggered)
                                        {
                                            //FIXME  MessageBox.Show(string.Format(LibStrings.FindResource("MsgNoCommAlarm"), model.GrblState.Substate.ToString()), "ioSender");
                                            if (AttemptReset())
                                                model.ExecuteCommand(GrblConstants.CMD_UNLOCK);
                                            else
                                            {
                                                //FIXME MessageBox.Show(LibStrings.FindResource("MsgResetFailed"), "ioSender");
                                                return RestartResult.Close;
                                            }
                                        }
                                        else if (AttemptReset())
                                            model.ExecuteCommand(GrblConstants.CMD_UNLOCK);
                                    }
                                    else
                                        response = string.Empty;
                                    break;

                                case 2: // Soft limits
                                    if (!GrblInfo.IsLoaded)
                                    {
                                        //FIXME  MessageBox.Show(string.Format(LibStrings.FindResource("MsgNoCommAlarm"), model.GrblState.Substate.ToString()), "ioSender");
                                        if (AttemptReset())
                                            model.ExecuteCommand(GrblConstants.CMD_UNLOCK);
                                        else
                                        {
                                            //FIXME  MessageBox.Show(LibStrings.FindResource("MsgResetFailed"), "ioSender");
                                            return RestartResult.Close;
                                        }
                                    }
                                    else
                                        response = string.Empty;
                                    break;

                                case 10: // EStop
                                    if (GrblInfo.IsGrblHAL && model.Signals.Value.HasFlag(Signals.EStop))
                                    {
                                        //FIXME  MessageBox.Show(LibStrings.FindResource("MsgEStop"), "ioSender", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        while (!AttemptReset() && model.GrblState.State == GrblStates.Alarm)
                                        {
                                            //FIXME  if (MessageBox.Show(LibStrings.FindResource("MsgEStopExit"), "ioSender", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                                return RestartResult.Close;
                                        };
                                    }
                                    else
                                        AttemptReset();
                                    if (!GrblInfo.IsLoaded)
                                        model.ExecuteCommand(GrblConstants.CMD_UNLOCK);
                                    break;

                                case 11: // Homing required
                                    if (GrblInfo.IsLoaded)
                                        response = string.Empty;
                                    else
                                        Message = LibStrings.FindResource("MsgHome");
                                    break;
                            }
                            break;

                        case GrblStates.Tool:
                            Comms.com.WriteByte(GrblConstants.CMD_STOP);
                            break;

                        case GrblStates.Door:
                            if (!GrblInfo.IsLoaded)
                            {
                                //FIXME if (MessageBox.Show(LibStrings.FindResource("MsgDoorOpen"), "ioSender", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                                if (true)
                                    return RestartResult.Close;
                                else
                                {
                                    bool exit = false;
                                    do
                                    {
                                        Comms.com.PurgeQueue();

                                        bool? res = null;
                                        CancellationToken cancellationToken = new CancellationToken();

                                        new Thread(() =>
                                        {
                                            res = WaitFor.SingleEvent<string>(
                                                cancellationToken,
                                                s => TrapReset(s),
                                                a => model.OnGrblReset += a,
                                                a => model.OnGrblReset -= a,
                                                200, () => Comms.com.WriteByte(GrblConstants.CMD_STATUS_REPORT));
                                        }).Start();

                                        while (res == null)
                                            EventUtils.DoEvents();

                                        if (!(exit = !model.Signals.Value.HasFlag(Signals.SafetyDoor)))
                                        {
                                            //FIXME  if (MessageBox.Show(LibStrings.FindResource("MsgDoorExit"), "ioSender", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                            if (true)
                                            {
                                                exit = true;
                                                return RestartResult.Close;
                                            }
                                        }
                                    } while (!exit);
                                }
                                if(model.GrblState.State == GrblStates.Door && model.GrblState.Substate == 0)
                                    Comms.com.WriteByte(GrblConstants.CMD_RESET);
                            }
                            else
                            {
                                //FIXME  MessageBox.Show(LibStrings.FindResource("MsgDoorPersist"), "ioSender", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                                response = string.Empty;
                            }
                            break;

                        case GrblStates.Hold:
                        case GrblStates.Sleep:
                            //FIXME  if (MessageBox.Show(string.Format(LibStrings.FindResource("MsgNoComm"), model.GrblState.State.ToString()),
                            //                        "ioSender", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                            if(true)
                                return RestartResult.Close;
                            else if (!AttemptReset())
                            {
                                //FIXME  MessageBox.Show(LibStrings.FindResource("MsgResetExit"), "ioSender");
                                return RestartResult.Close;
                            }
                            break;

                        case GrblStates.Idle:
                            if (response.Contains("|SD:Pending"))
                                AttemptReset();
                            break;
                    }
                }
            }
            else
            {
                //FIXME  MessageBox.Show(response == string.Empty
                //                    ? LibStrings.FindResource("MsgNoResponseExit")
                //                    : string.Format(LibStrings.FindResource("MsgBadResponseExit"), response),
                //                    "ioSender", MessageBoxButton.OK, MessageBoxImage.Stop);
                return RestartResult.Exit;
            }

            return response == string.Empty ? RestartResult.NoResponse : RestartResult.Ok;
        }

        private void TrapReset(string rws)
        {
            ResetPending = false;
        }

        private bool AttemptReset()
        {
            ResetPending = true;
            Comms.com.PurgeQueue();

            bool? res = null;
            CancellationToken cancellationToken = new CancellationToken();

            new Thread(() =>
            {
                res = WaitFor.SingleEvent<string>(
                    cancellationToken,
                    s => TrapReset(s),
                    a => model.OnGrblReset += a,
                    a => model.OnGrblReset -= a,
                    AppConfig.Settings.Base.ResetDelay, () => Comms.com.WriteByte(GrblConstants.CMD_RESET));
            }).Start();

            while (res == null)
                EventUtils.DoEvents();

            return !ResetPending;
        }
    }
}
