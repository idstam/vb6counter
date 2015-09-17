using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace vb6counter
{
    public class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            Parser.Default.ParseArguments(args , options);
            //Get all forders from the current folder
            var rootFolder = options.RootFolder == "" ? Environment.CurrentDirectory : options.RootFolder;
            var folders = new DirectoryInfo(rootFolder).GetDirectories("*.*", SearchOption.AllDirectories);
            var stats = new List<CodeFile>();

            foreach (var d in folders.Where(d => !d.Name.StartsWith(".")))
            {
                //Check if there's a .git folder here and do a git pull if that is asked for
                //if (options.GitPull) GitPull(d);

                //Check if there's a .vbp file here.
                var vbpFiles = d.GetFiles().Where(f => f.Extension.ToLower() == ".vbp");

                //Get all referenced files from the .vbp
                foreach (var vbp in vbpFiles)
                {
                    var codeFiles = GetProjectFiles(vbp);
                    //Parse each file
                    foreach (var codeFile in codeFiles)
                    {
                        var code = new CodeFile(Path.Combine(d.FullName, codeFile));
                        code.Parse();
                        stats.Add(code);
                    }
                }
            }
            if(!string.IsNullOrEmpty(options.ResultFile))
            {
                if(!File.Exists(options.ResultFile))
                {
                    File.WriteAllText(options.ResultFile, "Date\tFiles\tLines\tComments\tMethods" + Environment.NewLine);
                }
                File.AppendAllText(options.ResultFile, string.Format("{0}\t{1}\t{2}\t{3}\t{4}" + Environment.NewLine,
                DateTime.Now,
                stats.Count(),
                stats.Sum(s => s.Lines),
                stats.Sum(c => c.Comments),
                stats.Sum(m => m.Methods)
                ));
            }
            //Present the result
            if (!options.Silent)
            {
                Console.WriteLine(string.Format("Files:{0} Lines:{1} Comments:{2} Methods:{3}",
                    stats.Count(),
                    stats.Sum(s => s.Lines),
                    stats.Sum(c => c.Comments),
                    stats.Sum(m => m.Methods)
                    ));
            }
        }

        private static List<string> GetProjectFiles(FileInfo vbp)
        {
            var ret = new List<string>();

            foreach(var l in File.ReadAllLines(vbp.FullName, Encoding.GetEncoding("ISO8859-1")))
            {
                int pos = 0;
                if (l.StartsWith("Form=")) ret.Add(l.Replace("Form=", ""));
                if (l.StartsWith("UserControl=")) ret.Add(l.Replace("UserControl=", ""));
                if (l.StartsWith("Module="))
                {
                    pos = l.IndexOf(";") + 1;
                    ret.Add(l.Substring(pos, l.Length - pos).Trim());
                }

                if (l.StartsWith("Class="))
                {
                    pos = l.IndexOf(";") + 1;
                    ret.Add(l.Substring(pos, l.Length - pos).Trim());
                }


            }
            return ret;
        }

        private static void GitPull(DirectoryInfo d)
        {
            throw new NotImplementedException();
        }
    }

 
     internal class Options
     { 
         //[Option('g', "false", Required = false, HelpText = "Do a git pull in all folders")] 
         //public bool GitPull { get; set; }

        [Option('o', "", Required = false, HelpText = "Filename for result")]
        public string ResultFile { get; set; }

        [Option('f', "", Required = false, HelpText = "Root folder")]
        public string RootFolder { get; set; }

        [Option('s', "false", Required = false, HelpText = "Silent mode")]
        public bool Silent { get; set; }


        [HelpOption] 
         public string GetUsage()
         { 
             return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current)); 
         } 
     } 

}
