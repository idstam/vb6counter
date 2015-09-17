using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace vb6counter
{
    public class CodeFile
    {
        public int Comments { get; set; }
        public int Lines { get; set; }
        public int Methods { get; set; }
        private string _fullPath;
      
        public CodeFile(string fullPath)
        {
            _fullPath = fullPath;
        }
        public void Parse()
        {
            var lines = File.ReadAllLines(_fullPath);
            foreach(var l in lines)
            {
                if (l.Trim().StartsWith("'")) Comments++;
                Lines++;
                if (l.ToLower().Contains("sub ")) Methods++;
                if (l.ToLower().Contains("function ")) Methods++;
            }


        }

       
    }
}
