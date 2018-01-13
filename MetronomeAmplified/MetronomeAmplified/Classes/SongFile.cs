using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetronomeAmplified.Classes
{
    public class SongFile
    {
        private string name;
        public string GetName { get { return name; } }

        public SongFile(string name)
        {
            this.name = name;
        }
    }
}
