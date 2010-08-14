using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bend
{
    class FileExtensions
    {
        SortedDictionary<String, String> extensions = new SortedDictionary<string, string>();

        public FileExtensions()
        {
            //Just a few extensions
            extensions.Add("Ruby (*.rb)", "*.rb");
            extensions.Add("Text files (*.txt)", "*.txt");
            extensions.Add("All files (*.*)", "*.*");
            extensions.Add("Python (*.py)", "*.py");
            extensions.Add("C# (*.cs)", "*.cs");
            extensions.Add("HTML", "*.htm;*.html");
            extensions.Add("C/C++", "*.h;*.hxx;*.hpp;*.c;*.cxx;*.cpp");
            extensions.Add("JavaScript", "*.js");
            extensions.Add("PHP", "*.php");
            extensions.Add("XML", "*.xml");
        }

        //Returns a filter string -> OpenFileDialog.Filter
        public String GetFilterString()
        {
            String filterString = String.Empty;
            int helper = 0;
            foreach (KeyValuePair<String, String> kvp in extensions)
            {
                filterString += kvp.Key + "|" + kvp.Value;
                if (helper < extensions.Count - 1)
                {
                    filterString += "|";
                    helper++;
                }
            }
            return filterString;
        }

        //Public method to add extension filters
        public void addExtension(String desc, String ext)
        {
            extensions.Add(desc, ext);
        }
    }
}
