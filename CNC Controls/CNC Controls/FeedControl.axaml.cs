﻿/*
 * FeedControl.xaml.cs - part of CNC Controls library
 *
 * v0.05 / 2020-02-01 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2018-2019, Io Engineering (Terje Io)
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

using Avalonia.Controls;
using CNC.Core;

namespace CNC.Controls
{
    /// <summary>
    /// Interaction logic for FeedControl.xaml
    /// </summary>
    public partial class FeedControl : UserControl
    {
        public FeedControl()
        {
            InitializeComponent();

            feedOverrideControl.ResetCommand = GrblConstants.CMD_FEED_OVR_RESET;
            feedOverrideControl.FineMinusCommand = GrblConstants.CMD_FEED_OVR_FINE_MINUS;
            feedOverrideControl.FinePlusCommand = GrblConstants.CMD_FEED_OVR_FINE_PLUS;
            feedOverrideControl.CoarseMinusCommand = GrblConstants.CMD_FEED_OVR_COARSE_MINUS;
            feedOverrideControl.CoarsePlusCommand = GrblConstants.CMD_FEED_OVR_COARSE_PLUS;

            rapidsOverrideControl.ResetCommand = GrblConstants.CMD_RAPID_OVR_RESET;
            rapidsOverrideControl.FineMinusCommand = GrblConstants.CMD_RAPID_OVR_MEDIUM;
            rapidsOverrideControl.CoarseMinusCommand = GrblConstants.CMD_RAPID_OVR_LOW;
        }

        void override_CommandGenerated(string command)
        {
            (DataContext as GrblViewModel).ExecuteCommand(command);
        }
    }
}
