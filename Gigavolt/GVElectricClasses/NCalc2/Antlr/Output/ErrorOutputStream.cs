namespace Antlr3.Runtime.PCL.Output {
    class ErrorOutputStream : IOutputStream {
        public void WriteLine() {
            WriteLine(string.Empty);
        }

        public void WriteLine(string text) {
            OutputStreamHost.WriteLine(text);
        }

        public void WriteLine(object someObject) {
            if (someObject != null) {
                OutputStreamHost.WriteLine(someObject.ToString());
            }
            OutputStreamHost.WriteLine();
        }

        public void Write(string text) {
            OutputStreamHost.Write(text);
        }

        public void ReportProgress(double progress, string key, string message) {
            OutputStreamHost.ReportProgress(progress, key, message);
        }
    }
}