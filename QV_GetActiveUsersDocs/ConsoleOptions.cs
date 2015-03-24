using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace QV_GetActiveUsersDocs
{
    class ConsoleOptions
    {
        [Option('o', "out", HelpText = "Full path to output file. If not passed the result will be displayed in the console")]
        public string Output { get; set; }

        [Option('f', "format", HelpText = "Return data format. csv or json. Default is csv")]
        public string OutFormat { get; set; }

        [Option('a', "append", HelpText = "Append the result to the output file or overwrite. true or false. Default is true")]
        public string Append { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("QV Get Active Users<->Docs", "0.1"),
                Copyright = new CopyrightInfo("stefan.stoichev@gmail.com", 2015),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            help.AddPreOptionsLine(@"Usage: QV_GetActiveUsersDocs.exe -o ""c:\output\users.csv"" -a true -f csv");
            help.AddOptions(this);            
            return help;
        }
    }
}
