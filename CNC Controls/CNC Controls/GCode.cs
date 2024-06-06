/*
 * GCode.cs - part of CNC Controls library for Grbl
 *
 * v0.41 / 2022-07-19 / Io Engineering (Terje Io)
 *
 */

/*

Copyright (c) 2018-2022, Io Engineering (Terje Io)
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

using CNC.Core;
using CNC.GCode;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace CNC.Controls
{
    public class GCode
    {
        private struct GCodeConverter
        {
            public Type Type;
            public string FileType;
            public string FileExtensions;
        }
        private struct GCodeTransformer
        {
            public Type Type;
            public string Name;
        }

        public const string FileTypes = "cnc,nc,ncc,ngc,gcode,tap";

        private GCodeJob Program { get; set; } = new GCodeJob();
        private List<GCodeConverter> Converters = new List<GCodeConverter>();
        private List<GCodeTransformer> Transformers = new List<GCodeTransformer>();

        private static readonly Lazy<GCode> file = new Lazy<GCode>(() => new GCode());

        public event GCodeJob.ToolChangedHandler ToolChanged = null;

        public ObservableCollection<int> testCollection { get; set; }

        private GCode()
        {
            Program.FileChanged += Program_FileChanged;
            Program.ToolChanged += Program_ToolChanged;

            testCollection = new ObservableCollection<int> { 3, 4, 7 };

        }

        private bool Program_ToolChanged(int toolNumber)
        {
            return ToolChanged == null ? true : ToolChanged(toolNumber);
        }

        private void Program_FileChanged(string filename)
        {
            if (Model != null)
            {
                if (filename == "")
                    Model.ProgramLimits.Clear();
                else foreach (int i in AxisFlags.All.ToIndices())
                {
                    Model.ProgramLimits.MinValues[i] = Model.ConvertMM2Current(Program.BoundingBox.Min[i]);
                    Model.ProgramLimits.MaxValues[i] = Model.ConvertMM2Current(Program.BoundingBox.Max[i]);
                }

                Model.FileName = filename;
            }
        }

        public static GCode File { get { return file.Value; } }
        public bool IsLoaded { get { return Program.Loaded; } }
        public string FileName { get { return Model == null ? string.Empty : Model.FileName; } }
        public int ToolChanges { get { return Program.Parser.ToolChanges; } }
        public bool HasGoPredefinedPosition { get { return Program.Parser.HasGoPredefinedPosition; } }
        public int Decimals { get { return Program.Parser.Decimals; } }
        public bool HeightMapApplied { get { return Program.HeightMapApplied; } set { Program.HeightMapApplied = value; } }

        public DataTable Data { get { return Program.Data; } }
        public int Blocks { get { return Program.Data.Rows.Count; } }
        public List<GCodeToken> Tokens { get { return Program.Tokens; } }
        public Queue<string> Commands { get { return Program.commands; } }
        public GCodeParser Parser { get { return Program.Parser; } }

        public GrblViewModel Model { get; set; }

        public bool AddConverter(Type converter, string filetype, string fileextensions)
        {
            bool ok = converter.GetInterface("CNC.Controls.IGCodeConverter") != null;
            if (ok)
                Converters.Add(new GCodeConverter { Type = converter, FileType = filetype, FileExtensions = fileextensions });

            return ok;
        }

        private string getConversionTypes ()
        {
            string types = string.Empty;
            foreach (var converter in Converters)
                types += (types == string.Empty ? "" : ",") + converter.FileExtensions;

            return types;
        }

        public bool AddTransformer(Type converter, string name, ObservableCollection<MenuItem> menu)
        {
            bool ok = converter.GetInterface("CNC.Controls.IGCodeTransformer") != null;
            if (ok)
            {
                Transformers.Add(new GCodeTransformer { Type = converter, Name = name });

                MenuItem item = new MenuItem()
                {
                    Header = name,
                    Tag = menu.Count
                };

                item.Click += TransformMenu_Click;

                menu.Add(item);
            }

            return ok;
        }

        public bool HasTransformer(Type converter)
        {
            return Transformers.Where(x => x.Type == converter).FirstOrDefault().Type == converter;
        }

        private void TransformMenu_Click(object sender, RoutedEventArgs e)
        {
            Transform((int)(sender as MenuItem).Tag);
        }

        public void Transform(int id)
        {
            if (Transformers.Count > id)
            {
                var loader = (IGCodeTransformer)Activator.CreateInstance(Transformers[id].Type);
                loader.Apply();
            }
        }

        public void AddBlock(string block, Core.Action action)
        {
            Program.AddBlock(block, action);

            if(action == Core.Action.End)
                Model.Blocks = Blocks;
        }

        public void AddBlock(string block)
        {
            Program.AddBlock(block);
        }

        public void ClearStatus()
        {
            foreach (DataRow row in Program.Data.Rows)
                if ((string)row["Sent"] != string.Empty)
                    row["Sent"] = string.Empty;
        }

        public void Drag(object sender, DragEventArgs e)
        {
            bool allow = Model != null && GrblParserState.IsLoaded && (Model.StreamingState == StreamingState.Idle || Model.StreamingState == StreamingState.NoFile);

            //FIXME    if (allow && e.Data.GetDataPresent(DataFormats.FileDrop))
            //{
            //    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            //    allow = files.Count() == 1 && FileUtils.IsAllowedFile(files[0].ToLower(), FileTypes + (getConversionTypes() == string.Empty ? "" : "," + getConversionTypes()) + ",txt");
            //}

            e.Handled = true;
            //e.Effects = allow ? DragDropEffects.Copy : DragDropEffects.None;
            e.DragEffects = allow ? DragDropEffects.Copy : DragDropEffects.Move;
            
        }

        public void Drop(object sender, DragEventArgs e)
        {
            //string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            //FIXME    if (files.Count() == 1)
            //{
            //    Load(files[0]);
            //}
        }

        public void Close()
        {
            Program.CloseFile();
            Model.Blocks = Blocks;
        }

        public async Task OpenAsync(Visual? visual, string title)
        {
            string filename = string.Empty;
            string conversionFilter = string.Empty; //conversionTypes == string.Empty ? string.Empty : string.Format("Other files ({0})|{0}|", FileUtils.ExtensionsToFilter(conversionTypes));
            
            foreach (var converter in Converters)
                conversionFilter += string.Format("{0} ({1})|{1}|", converter.FileType, FileUtils.ExtensionsToFilter(converter.FileExtensions));

            //FIXME  ADD CONVERTED TYPES TO FILTERS
            //var filter = string.Format("GCode files ({0})|{0}|{1}", FileUtils.ExtensionsToFilter(FileTypes), conversionFilter);

            string[] filetypes = FileTypes.Split(',');

            for (int i = 0; i < filetypes.Length; i++)
                filetypes[i] = "*." + filetypes[i];

            FilePickerFileType gcode_files = new("Gcode files")
            {
                Patterns = filetypes
            };


            var tl = TopLevel.GetTopLevel(visual);

            var file = await tl.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                SuggestedStartLocation = await StorageProviderExtensions.TryGetFolderFromPathAsync(tl.StorageProvider, Core.Resources.Path),
                FileTypeFilter = new[] { gcode_files, FilePickerFileTypes.TextPlain },
                AllowMultiple = false,
            });

            if (file != null)
                filename = file[0].Name.ToString();

            if (filename != string.Empty)
                Load(filename);

            Model.Blocks = Blocks;
        }

        public void Load(string filename)
        {
            foreach (var converter in Converters)
            {
                var filetypes = converter.FileExtensions.Split(',');

                foreach (var filetype in filetypes) {
                    if (filename.EndsWith(filetype))
                    {
                        var loader = (IGCodeConverter)Activator.CreateInstance(converter.Type);
                        loader.LoadFile(File, filename);
                        return;
                    }
                }
            }

            using (new UIUtils.WaitCursor())
            {
                Program.LoadFile(filename);
            }

            Model.Blocks = Blocks;
        }

        public void Save()
        {
            
            //SaveFileDialog saveDialog = new SaveFileDialog()
            //{
            //    Filter = "GCode file (*.nc)|*.nc",
            //    AddExtension = true,
            //    DefaultExt = ".nc",
            //};

            //if (saveDialog.ShowDialog() == true)
            //{
            //    try
            //    {
            //        //ORIG
            //        //using (new UIUtils.WaitCursor())
            //        //{
            //        //    GCodeParser.Save(saveDialog.FileName, GCodeParser.TokensToGCode(File.Tokens));
            //        //}

            //        //REMOVE using (StreamWriter stream = new StreamWriter(saveDialog.FileName))
            //        using (StreamWriter stream = new StreamWriter(saveDialog.InitialFileName))
            //        {
            //            using (new UIUtils.WaitCursor())
            //            {
            //                foreach (DataRow line in Program.Data.Rows)
            //                    stream.WriteLine((string)line["Data"]);
            //            }
            //        }
            //    }
            //    catch (IOException) { }

            //    //REMOVE  Model.FileName = saveDialog.FileName;
            //    Model.FileName = saveDialog.InitialFileName;
            //}
        }
    }
}
