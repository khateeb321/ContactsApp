using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Contacts_DB
{
    class Person
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }

        public string Names { get; set; }
        public string Numbers { get; set; }
        public string Emails { get; set; }
        public byte[] Images { get; set; }
        public string Addresses { get; set; }
        public string Websites { get; set; }
        public int isFav { get; set; }
    }
}
