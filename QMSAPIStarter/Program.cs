using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QV_GetActiveUsersDocs.QMSAPIService;
using QMSAPIStarter.ServiceSupport;
using System.IO;

namespace QV_GetActiveUsersDocs
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int h = 0; h < args.Length; h++)
            {
                var ar = args[h].ToString();
                if (ar.ToLower(CultureInfo.InvariantCulture).IndexOf("help", System.StringComparison.Ordinal) >= 0)
                {
                    Console.WriteLine("");
                    Console.WriteLine("The app accept 2 parameters:");
                    Console.WriteLine("1. Output file full path");
                    Console.WriteLine(@"2. (optional) append to output - true or false. Default is ""true""");
                    Console.WriteLine("");
                    Console.WriteLine(@"Example: QV_GetActiveUsersDocs.exe ""c:\output\users.csv"" false");
                    Console.WriteLine("");
                    Console.WriteLine(@"To change the QVS URL edit the app.config file (""address"" part on rows 35 and 38)");
                    
                    Environment.Exit(0);
                }

            }

            string resultfile = "";
            bool append = true;
            //resultfile = @"s:\development\stefan\users3.csv";

            if (args.Length > 0)
            {
                resultfile = args[0].ToString();
            }
            else
            {
                Console.WriteLine(GenerateReturnMsg("error", "Please provide the path to the output file as an argument."));
                Environment.Exit(0);
            }

            string arg2 = "";

            try
            {
                arg2 = args[1].ToString();

                if (arg2 == "true")
                {
                    append = true;
                }
                else if (arg2 == "false")
                {
                    append = false;
                }
                else
                {
                    append = true;
                }
            }
            catch (System.Exception ex)
            {
                append = true;
            }

            if (!File.Exists(resultfile))
            {
                try
                {
                    File.WriteAllText(resultfile, "timestamp,server,document,user" + Environment.NewLine);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(GenerateReturnMsg("error", "Cannot create output file. Make sure that the path exists."));
                    Environment.Exit(0);
                }
            }            


            QMSClient Client;
            Client = new QMSClient("BasicHttpBinding_IQMS");

            try
            {
                string key = Client.GetTimeLimitedServiceKey();
                ServiceKeyClientMessageInspector.ServiceKey = key;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(GenerateReturnMsg("error", "Cannot reach QVS. Make sure that the URL is correct"));
                Environment.Exit(0);
            }


            var guids = new Guid[1];  
            try
            {
                ServiceInfo[] myServices = Client.GetServices(ServiceTypes.All);
                bool QVSExists = false;
                foreach (ServiceInfo service in myServices)
                {
                    //Console.WriteLine(service.Type + " " + service.Name + " " + service.ID);
                    if (service.Type.ToString() == "QlikViewServer")
                    {
                        guids[0] = service.ID;
                        QVSExists = true;
                    }
                }

                if (QVSExists == false)
                {
                    Console.WriteLine(GenerateReturnMsg("error", "There is no QVS service available. Make sure that the QVS service is running."));
                    Environment.Exit(0);
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(GenerateReturnMsg("error", "Cannot fetch services list from QVS. Make sure that the URL is correct"));
                Environment.Exit(0);
            }
                     

            var executiondate = DateTime.Now.ToString("yyyyMMddHHmmss");
            
            var stats = Client.GetServiceStatuses(guids);
            var members = stats[0].MemberStatusDetails;

            string userdocs = "";
            userdocs = "";
            for (int m = 0; m < members.Length; m++)
            {
                var a = members[m].ID;
                var t = Client.GetQVSDocumentsAndUsers(a, QueryTarget.ClusterMember);
                
                foreach (KeyValuePair<string, string[]> entry in t)
                {
                    var doc = entry.Key;
                    for (int b = 0; b < entry.Value.Length; b++)
                    {
                        userdocs = userdocs + executiondate.ToString() + "," + members[m].Host + "," + doc + "," + entry.Value[b] + Environment.NewLine;
                    }
                }

            }

            if (append == true)
            {
                File.AppendAllText(resultfile, userdocs);
            }
            else
            {
                File.WriteAllText(resultfile, userdocs);
            }
            

            string msg1 = GenerateReturnMsg("completed", "Data is saved.");
            Console.WriteLine(msg1);

        }

        public static string GenerateReturnMsg(string msgtype, string msgtext)
        {
            string msg = "";
            msg = @"{""msg"": {""status"":""" + msgtype + @""", ""text"": """ +  msgtext + @""" }}";
            return msg;
        }
    }
}
