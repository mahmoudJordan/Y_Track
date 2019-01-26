using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y_Track.ViewModels
{
    public class SettingViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;


        private string _videosPath;
        private string _databasePath;


        public SettingViewModel()
        {
            _videosPath = Properties.Settings.Default.OutputDirectory;
            _databasePath = Properties.Settings.Default.DatabaseDirectory;
        }


        public string VideosPath
        {
            get
            {
                return _videosPath;
            }
            set
            {

                _videosPath = value;
                PropertyChanged(this, new PropertyChangedEventArgs("VideosPath"));
            }

        }

        public string DatabasePath
        {
            get
            {
                return _databasePath;
            }
            set
            {

                _databasePath = value;
                PropertyChanged(this, new PropertyChangedEventArgs("DatabasePath"));
            }

        }





    }
}
