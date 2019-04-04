using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y_Track.FFMpeg
{
    public class FFMpegCommander
    {
        private string _path;
        public FFMpegCommander(string ffmpegPath)
        {
            _path = ffmpegPath;
        }

        /// <summary>
        /// spin new process and execute FFMPEG Command
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<string> Execute(string args)
        {
            using (var process = new Process())
            {
                //start configuring the FFMpeg process
                process.StartInfo = new ProcessStartInfo();
                process.StartInfo.UseShellExecute = false;

                // redirect standard output and standard error
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                // hide the console window
                process.StartInfo.CreateNoWindow = true;

                // Set FFMpeg path
                process.StartInfo.FileName = _path;

                process.StartInfo.Arguments = args;

                process.Start();
                process.WaitForExit();
                if (process.ExitCode == 0) return await process.StandardOutput.ReadToEndAsync();
                else return process.StandardError.ReadToEnd();
            }
      
        }


    }
}
