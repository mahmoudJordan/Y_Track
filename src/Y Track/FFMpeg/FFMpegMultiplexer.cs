
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Y_Track.FFMpeg;
using Y_Track.Helpers;

namespace FFMpeg
{
    public class FFMpegMultiplexer : IDisposable
    {
        public string VideoInput { get; private set; }
        public string AudioInput { get; private set; }
        public string Extention { get; private set; }

        private string _path;
        private ProcessStartInfo _startInfo;
        private Process _process;
        private string _outputTemporaryFile;
        private double? _durationSeconds;
        private bool _fallBackToReEncode = false;
        // a flag to check if encode fallback tried
        private bool _reEncodeTried = false;
        private string _standardOutputContent = null;
        private string _standardErrorContent = null;

        public delegate void MultiplexingProgressChanged(object sender, double progess);
        public event MultiplexingProgressChanged OnMultiplexingProgressChanged;

        public delegate void MultiplexingCompleted(object sender, MultiplexingExitedEventArguments meea);
        public event MultiplexingCompleted OnMultiplixingComplete;

        public delegate void MultiplexingFailed(object sender, Exception exception);
        public event MultiplexingFailed OnMultiplixingFailed;

        public FFMpegMultiplexer(string FFMpegPath, string videoInput, string audioInput, bool fallBackToReEncode)
        {
            _path = FFMpegPath;
            _process = new Process();
            VideoInput = videoInput;
            AudioInput = audioInput;
            _fallBackToReEncode = fallBackToReEncode;
            _configureProcessStart();
        }


        private void _configureProcessStart()
        {

            //start configuring the FFMpeg process
            _startInfo = new ProcessStartInfo();
            _startInfo.UseShellExecute = false;

            // redirect standard output and standard error
            _startInfo.RedirectStandardOutput = true;
            _startInfo.RedirectStandardError = true;

            // hide the console window
            _startInfo.CreateNoWindow = true;

            // Set FFMpeg path
            _startInfo.FileName = _path;

            _process.EnableRaisingEvents = true;
            _process.StartInfo = _startInfo;

            // for debugging
            _process.OutputDataReceived += _handleIncomingStandardOutput;
            _process.ErrorDataReceived += _handleIncomingStandardError;
        }

        private void _handleIncomingStandardError(object sender, DataReceivedEventArgs o)
        {
            if (string.IsNullOrEmpty(o.Data)) return;
            this._standardErrorContent += o.Data;
            _handleOutput(o);
        }

        private void _handleIncomingStandardOutput(object sender, DataReceivedEventArgs o)
        {
            if (string.IsNullOrEmpty(o.Data)) return;
            this._standardOutputContent += o.Data;
            _handleOutput(o);
        }

        private void _handleOutput(DataReceivedEventArgs o)
        {
            double? encodedSeconds = _parseOutputEncodedSeconds(o.Data);
            double? totalSeconds = _durationSeconds;

            if (!encodedSeconds.HasValue || !totalSeconds.HasValue) return;

            double percentage = (encodedSeconds.Value / totalSeconds.Value) * 100;
            OnMultiplexingProgressChanged?.Invoke(this, percentage);
        }

        private async Task<double?> _getDurationSeconds()
        {
            string prefix = "Duration: ";
            string informationString = await new FFMpegCommander(_path).Execute("-i \"" + this.VideoInput + "\"");
            string durationExpression = prefix + @"\d{2}:\d{2}:\d{2}.\d{2}";
            return _matchSecondsPattern(prefix, informationString, durationExpression);
        }

        private double? _parseOutputEncodedSeconds(string currentOutput)
        {
            string prefix = "time=";
            string durationExpression = prefix + @"\d{2}:\d{2}:\d{2}.\d{2}";
            return _matchSecondsPattern(prefix, currentOutput, durationExpression);
        }


        private double? _matchSecondsPattern(string prefix, string informationString, string durationExpression)
        {
            Regex regex = new Regex(durationExpression);
            Match match = regex.Match(informationString);
            if (match.Success)
            {
                string durationTime = match.Value.Replace(prefix, "");
                double seconds = TimeSpan.Parse(durationTime).TotalSeconds;
                return seconds;
            }
            else return null;
        }



        public Task<Process> Muliplix()
        {
            return _startMultiplixing(false);
        }

        private string _getMuxingArguments(bool encode, string outputFilePath)
        {
            // each container has it's own parameters 
            return encode
            // -shortest switch will force ffmpeg to take the shortest stream for the encoded
            ? "-i \"" + VideoInput + "\" -i  \"" + AudioInput + "\" -shortest \"" + outputFilePath + "\""
            // since the input files are Webms then no need to re-encode them
            // -copy switch will just copy both streams without encoding
            : "-i \"" + VideoInput + "\" -i  \"" + AudioInput + "\" -c copy \"" + outputFilePath + "\"";

        }


        /// <summary>
        /// resets the curren process and initialize it again 
        /// this is helpfull because we want to reuse the same instance of this class but we can't use the same instance of _process
        /// </summary>
        private void _resetCurrentProcess()
        {
            this._process.Dispose();
            this._process = new Process();
            _configureProcessStart();
        }

        private async Task<Process> _startMultiplixing(bool reEcnode)
        {
            if (!File.Exists(_path))
                throw new FileNotFoundException("FFMpeg is not found in : " + _path);

            // if reEncodeing is true this means we have to create another process
            // Reusing the same one would make errors since we are reading the standard output/error asyncronously
            if (reEcnode) _resetCurrentProcess();

            _durationSeconds = await _getDurationSeconds();

            // a temporary file to save the output temporarly
            _outputTemporaryFile = Path.GetTempFileName();

            // if reEcnode flag (which I'm reading from settings) is set to true we will use mp4 container 
            // otherwise webm will be used
            Extention = reEcnode ? ".mp4" : ".webm";

            // without re-encoding we use webm container because both inputs are webm's
            string outputFilePath = _outputTemporaryFile + Extention;
            // getting the argument based on encoding
            _startInfo.Arguments = _getMuxingArguments(reEcnode, outputFilePath);


            // debugging
            if (reEcnode)
            {
                Debug.WriteLine("falling back to re-encode");
            }
            else
            {
                Debug.WriteLine("Trying Without Re-encode the output streams");
            }

            _process.Exited += _process_Exited;

            if (!_process.Start()) return null;
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            return _process;
        }


        private async void _process_Exited(object sender, EventArgs e)
        {
            var exitCode = _process.ExitCode;
            if (exitCode == 0)
                _raiseMultiplixingComplete(_outputTemporaryFile + Extention);
            else
            {
                // if copying the streams doesn't work w'll try to re-encode them before raising MultiplixingFailed
                if (_reEncodeTried)
                {
                    _raiseMultiplixingFailed(this._standardErrorContent);
                }
                else
                {
                    _reEncodeTried = true;
                    await _startMultiplixing(true);
                }
            }
        }

        private void _raiseMultiplixingComplete(string outputFilePath)
        {
            OnMultiplixingComplete?.Invoke(this, new MultiplexingExitedEventArguments()
            {
                OutputFilePath = outputFilePath,
                OutputFileName = Path.GetFileName(outputFilePath),
                OutputFileExtention = Extention
            });
        }

        private void _raiseMultiplixingFailed(string errorStd)
        {
            YTrackLogger.Log("Re-Encoding Fallback also failed" + errorStd);
            OnMultiplixingFailed?.Invoke(this, new MultiplexingException("Some thing Went Wrong While Multiplexing"));
        }


        public void Dispose()
        {
            this._process.Dispose();
        }
    }
}
