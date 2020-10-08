using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cryptool
{
    [Serializable]
    public class Entry
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Comments { get; set; }
        public Entry()
        {

        }

    }
}
