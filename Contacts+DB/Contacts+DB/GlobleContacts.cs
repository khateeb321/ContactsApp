using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Contacts_DB
{
    class GlobleContacts
    {
        public static List<string> Names = new List<string>();
        public static List<string> Numbers = new List<string>();
        public static List<string> Emails = new List<string>();
        public static List<BitmapImage> Images = new List<BitmapImage>();
        public static List<string> Addresses = new List<string>();
        public static List<string> Websites = new List<string>();
        public static List<int> isFav = new List<int>();
    }
}
