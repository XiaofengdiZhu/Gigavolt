// OsmSharp - OpenStreetMap tools & library.
// Copyright (C) 2012 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace Antlr3.Runtime.PCL.Output {
    /// <summary>
    ///     Output stream host.
    /// </summary>
    public static class OutputStreamHost {
        static double _previous_progress;

        static IList<IOutputStream> _output_streams;

        static void InitializeIfNeeded() {
            if (_output_streams == null) {
                _output_streams = new List<IOutputStream>();
                //_output_streams.Add(new ConsoleOutputStream());
            }
        }

        /// <summary>
        ///     Writes a line.
        /// </summary>
        public static void WriteLine() {
            WriteLine(string.Empty);
        }

        /// <summary>
        ///     Writes a line.
        /// </summary>
        /// <param name="text"></param>
        public static void WriteLine(string text) {
            InitializeIfNeeded();
            if (_previous_progress > 0) {
                _previous_progress = 0;
                WriteLine();
            }
            foreach (IOutputStream stream in _output_streams) {
                stream.WriteLine(text);
            }
        }

        /// <summary>
        ///     Writes a line.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        public static void WriteLine(string format, params object[] arg) {
            WriteLine(string.Format(format, arg));
        }

        /// <summary>
        ///     Writes text.
        /// </summary>
        /// <param name="text"></param>
        public static void Write(string text) {
            InitializeIfNeeded();
            if (_previous_progress > 0) {
                _previous_progress = 0;
                WriteLine();
            }
            foreach (IOutputStream stream in _output_streams) {
                stream.Write(text);
            }
        }

        /// <summary>
        ///     Writes text.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        public static void Write(string format, params object[] arg) {
            Write(string.Format(format, arg));
        }

        /// <summary>
        ///     Reports progress.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="key"></param>
        /// <param name="message"></param>
        public static void ReportProgress(double progress, string key, string message) {
            InitializeIfNeeded();
            if (progress > 1) {
                progress = 1;
            }
            if (progress < 0) {
                progress = 0;
            }
            _previous_progress = progress;
            foreach (IOutputStream stream in _output_streams) {
                stream.ReportProgress(progress, key, message);
            }
            if (_previous_progress == 1) {
                foreach (IOutputStream stream in _output_streams) {
                    stream.WriteLine(string.Empty);
                }
                _previous_progress = 0;
            }
        }

        /// <summary>
        ///     Report progress.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="total"></param>
        /// <param name="key"></param>
        /// <param name="message"></param>
        public static void ReportProgress(long current, long total, string key, string message) {
            ReportProgress(current / (double)total, key, message);
        }

        /// <summary>
        ///     Register output stream.
        /// </summary>
        /// <param name="output_stream"></param>
        public static void RegisterOutputStream(IOutputStream output_stream) {
            InitializeIfNeeded();
            _output_streams.Add(output_stream);
        }

        /// <summary>
        ///     Register output stream.
        /// </summary>
        /// <param name="output_stream"></param>
        public static void UnRegisterOutputStream(IOutputStream output_stream) {
            InitializeIfNeeded();
            _output_streams.Remove(output_stream);
        }

        public static IOutputStream Error => new ErrorOutputStream();

        public static IOutputStream Out => new OutOutputStream();
    }
}