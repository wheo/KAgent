using System;
using System.ComponentModel;
using System.Diagnostics;

namespace KAgent.Interface
{
    internal class Item : INotifyPropertyChanged
    {
        public DateTime start_at { get; set; } = DateTime.Now;

        private DateTime _end_at = DateTime.MinValue;

        public DateTime end_at
        {
            get
            {
                return _end_at;
            }
            set
            {
                if (_end_at != value)
                {
                    _end_at = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("end_at"));
                }
            }
        }

        public string filepath { get; set; }
        public string customer { get; set; }
        private int _elapsed = 0;

        public int elapsed
        {
            get { return _elapsed; }
            set
            {
                if (_elapsed != value)
                {
                    _elapsed = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("elapsed"));
                }
            }
        }

        private double _progress = 0;

        public double progress
        {
            get
            {
                return _progress;
            }
            set
            {
                if (_progress != value)
                {
                    _progress = Math.Round(value, 2);
                    OnPropertyChanged(new PropertyChangedEventArgs("progress"));
                }
            }
        }

        private string _status;

        public string status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("status"));
                }
            }
        }

        private string _api_status;

        public string api_status
        {
            get
            {
                return _api_status;
            }
            set
            {
                if (_api_status != value)
                {
                    _api_status = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("api_status"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}