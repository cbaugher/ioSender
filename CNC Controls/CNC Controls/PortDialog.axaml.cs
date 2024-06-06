﻿/*
 * PortDialog.xaml.cs - part of CNC Controls library
 *
 * v0.41 / 2022-09-25 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2019-2022, Io Engineering (Terje Io)
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

using Avalonia.Interactivity;
using CNC.Core;
using System;
using Avalonia.Controls;
using System.Collections.ObjectModel;

namespace CNC.Controls
{
    public partial class PortDialog : UserControl
    {
        private string port = null;
        //private PortProperties prop;

        public PortDialog()
        {
            InitializeComponent();

//            DataContext = prop = new PortProperties();
            Prop = new PortDialogViewModel();
        }

        public static PortDialogViewModel Prop { get; set; }


        private void CbxPorts_DropDownOpened(object sender, System.EventArgs e)
        {
            Prop.Com.Refresh();
        }

        private bool PortAvailable(string port)
        {
            bool found = false;

            foreach (var p in Prop.Com.Ports)
                found = found || p.Name == port;

            return found;
        }

        private void parsenet(string uri)
        {
            int port = 0;
            string[] values = uri.Split(':');

            Prop.IpAddress = values[0];
            if (values.Length == 2 && int.TryParse(values[1], out port))
                Prop.NetPort = port;
            else
                Prop.NetPort = Prop.IsWebSocket ? 80 : 23;

            //FIXME  tab.SelectedIndex = 1;
        }

        public string ShowDialog(string orgport)
        {
            if (!string.IsNullOrEmpty(orgport)) {

                if ((Prop.IsWebSocket = orgport.ToLower().StartsWith("ws://")))
                    parsenet(orgport.Substring(5));
                else if (char.IsDigit(orgport[0])) // We have an IP address
                    parsenet(orgport);
                else
                {
                    string portname = orgport.Substring(0, orgport.IndexOf(':'));
                    if (PortAvailable(portname))
                    {
                        Prop.Com.SelectedPort = portname;
                        string[] values = orgport.Split(':')[1].Split(',');

                        if (!Prop.Com.Baud.Contains(values[0]))
                            Prop.Com.Baud.Add(values[0]);

                        Prop.Com.SelectedBaud = values[0];

                        if (values.Length > 5)
                        {
                            Comms.ResetMode mode = Comms.ResetMode.None;
                            Enum.TryParse(values[5], true, out mode);
                            if (mode != Comms.ResetMode.None)
                            {
                                foreach (ConnectMode m in Prop.Com.ConnectModes)
                                    if (m.Mode == mode)
                                        Prop.Com.SelectedMode = m;
                            }
                        }
                    }
                }
            }

//            ShowDialog();

            return port;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
//            if (tab.SelectedIndex == 1)
//            {
//                port = string.Format("{0}{1}:{2}", prop.IsWebSocket ? "ws://" : string.Empty, prop.IpAddress, prop.NetPort.ToString());
//            }
//            else if(prop.Com.Ports.Count > 0)
//            {
//                port = prop.Com.SelectedPort + ":" + prop.Com.SelectedBaud + ",N,8,1";
//                if (prop.Com.SelectedMode.Mode != Comms.ResetMode.None)
//                    port += ",," + prop.Com.SelectedMode.Mode.ToString();
//            }

//            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
//            Close();
        }
    }

    //class PortProperties : ViewModelBase
    public partial class PortDialogViewModel : ViewModelBase
    {
        bool isWebSocket = false;
        string ipAddress = "192.168.5.1";
        int netport = 23;

        public SerialPorts Com { get; private set; } = new SerialPorts();
        public bool IsWebSocket {
            get { return isWebSocket; }
            set {
                if (isWebSocket != value)
                    NetPort = value ? 80 : 23;
                isWebSocket = value;
                OnPropertyChanged();
            }
        }
        public string IpAddress { get { return ipAddress; } set { ipAddress = value; OnPropertyChanged(); } }
        public int NetPort { get { return netport; } set { netport = value; OnPropertyChanged(); } }
    }
}
