using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace AP.ViewModel
{
    public class ToDoItemViewModel : INotifyPropertyChanged
    {
        private bool _completed;

        private Color _color = Colors.Red;

        public String Text { get; set; }
        public String Text1 { get; set; }
        public BitmapImage Text2 { get; set; }
        public String Text3 { get; set; }
        public String Text4 { get; set; }

        public bool Completed
        {
            get { return _completed; }
            set
            {
                _completed = value;
                OnPropertyChanged("Completed");
            }
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("Color");
            }
        }



        public ToDoItemViewModel()
        {
            // TODO: Complete member initialization
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
