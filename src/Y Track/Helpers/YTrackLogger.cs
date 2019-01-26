using System;
namespace Y_Track.Helpers
{
    public class YTrackLogger
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly log4net.ILog _logSpecial = log4net.LogManager.GetLogger("SpecialLogger");
        private static bool _configurationRead = false;

        private YTrackLogger()
        {
            if (!_configurationRead)
                log4net.Config.XmlConfigurator.Configure();
        }

        public static void Log(string message)
        {
            _log.Info(message);
        }

        public static void Log(string message, bool special)
        {
            if (special) _logSpecial.Info(message);
            else _log.Info(message);
        }

    }
}
