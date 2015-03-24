using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QV_GetActiveUsersDocs.QMSAPIService;
using QMSAPIStarter.ServiceSupport;
using System.IO;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;

namespace QV_GetActiveUsersDocs
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new ConsoleOptions();
            CommandLine.Parser.Default.ParseArguments(args, options);

            for (int h = 0; h < args.Length; h++)
            {
                var ar = args[h].ToString();
                if (ar.ToLower(CultureInfo.InvariantCulture).IndexOf("help", System.StringComparison.Ordinal) >= 0)
                {
                    Environment.Exit(0);
                }
            }

            var outputArg = options.Output;
            var outFormatArg = options.OutFormat;
            var appendResultArg = options.Append;
            var resultfile = "";
            var outFormat = "";
            var appendResult = "";

            string columns = "timestamp,server,document,userid,username";

            if (appendResultArg == null)
            {
                appendResult = "true";
            }
            else
            {
                appendResult = appendResultArg;
            }


            if (outputArg == null)
            {
                resultfile = "console";
            }
            else
            {
                resultfile = outputArg;
                resultfile = resultfile.Replace("/", "\\");
                if (!File.Exists(resultfile))
                {
                    try
                    {
                        File.WriteAllText(resultfile, columns + Environment.NewLine);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(GenerateReturnMsg("error", "Cannot create output file. Make sure that the path is valid."));
                        Environment.Exit(0);
                    }
                }
                else
                {
                    if (appendResult == "false")
                    {
                        File.WriteAllText(resultfile, columns + Environment.NewLine);
                        Environment.Exit(0);
                    }
                }
            }

            if (outFormatArg == null)
            {
                outFormat = "csv";
            }
            else
            {
                outFormat = outFormatArg.ToString();
            }


            if (resultfile == "console" && appendResult == "true")
            {
                Console.WriteLine(GenerateReturnMsg("error", "You can't have 'console' output and 'append' at the same time. Please provide path to the ouput file or remove append"));
                Environment.Exit(0);
            }

            DataTable dt = GetApiData();

            if (outFormat == "csv")
            {
                string csv = GenerateCSV(dt);
                if (resultfile != "console")
                {
                    File.AppendAllText(resultfile, csv);
                    Console.WriteLine(GenerateReturnMsg("completed", "Data is saved."));
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine(columns + Environment.NewLine + csv);
                    Environment.Exit(0);
                }

            }
            else
            {
                string json = GenerateJSON(dt);
                JArray jsonObj = (JArray)JsonConvert.DeserializeObject(json);

                if (resultfile != "console")
                {
                    if (appendResult == "false")
                    {
                        JObject jsData1 = new JObject(
                           new JProperty("data", jsonObj));
                        File.WriteAllText(resultfile, jsData1.ToString());
                        Console.WriteLine(GenerateReturnMsg("completed", "Data is saved."));
                        Environment.Exit(0);
                    }
                    else
                    {
                        try
                        {
                            JObject o2;
                            using (StreamReader file = File.OpenText(resultfile))
                            using (JsonTextReader reader = new JsonTextReader(file))
                            {
                                o2 = (JObject)JToken.ReadFrom(reader);
                            }

                            var a1 = o2["data"][0];
                            for (int i = 0; i < jsonObj.Count; i++)
                            {
                                a1.AddAfterSelf(jsonObj[i]);
                            }

                            File.WriteAllText(resultfile, o2.ToString());
                            Console.WriteLine(GenerateReturnMsg("completed", "Data is saved."));
                            Environment.Exit(0);
                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine(GenerateReturnMsg("error", "Error parsing the ouput file. Make sure the file is in valid JSON format."));
                            Environment.Exit(0);
                        }
                    }
                }
                else
                {
                    Console.WriteLine(columns + Environment.NewLine + json);
                    Environment.Exit(0);
                }
            }           
        }

        public static DataTable GetApiData() {

            QMSClient Client = null;
            try
            {
                Client = new QMSClient("BasicHttpBinding_IQMS");
            }
            catch (System.Exception ex)
            {
                //Console.Write(ex.Message);
                Console.WriteLine(GenerateReturnMsg("error", "Cannot reach QMS. Make sure that the URL is correct and the services are up"));
                Environment.Exit(0);
            }

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
            var qdsGuid = new Guid[1];
            bool QVSExists = false;
            //bool QDSExists = false;

            try
            {
                ServiceInfo[] myServices = Client.GetServices(ServiceTypes.All);

                foreach (ServiceInfo service in myServices)
                {
                    if (service.Type.ToString() == "QlikViewServer")
                    {
                        guids[0] = service.ID;
                        QVSExists = true;
                    }

                    if (service.Type.ToString() == "QlikViewDirectoryServiceConnector")
                    {
                        qdsGuid[0] = service.ID;
                        //QDSExists = true;
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

            DataTable dtData = new DataTable();
            DataColumn TimeStamp = new DataColumn("timestamp");
            DataColumn Server = new DataColumn("server");
            DataColumn Document = new DataColumn("document");
            DataColumn UserId = new DataColumn("userid");

            dtData.Columns.Add(TimeStamp);
            dtData.Columns.Add(Server);
            dtData.Columns.Add(Document);
            dtData.Columns.Add(UserId);

            for (int m = 0; m < members.Length; m++)
            {
                var a = members[m].ID;
                var t = Client.GetQVSDocumentsAndUsers(a, QueryTarget.ClusterMember);

                foreach (KeyValuePair<string, string[]> entry in t)
                {
                    var doc = entry.Key;
                    for (int b = 0; b < entry.Value.Length; b++)
                    {
                        DataRow dr = dtData.NewRow();
                        dr["timestamp"] = executiondate.ToString();
                        dr["server"] = members[m].Host;
                        dr["document"] = doc;
                        dr["userid"] = entry.Value[b];
                        dtData.Rows.Add(dr);
                    }
                }

            }

            return dtData;
        }

        public static string GenerateReturnMsg(string msgtype, string msgtext)
        {
            string msg = "";
            msg = @"{""msg"": {""status"":""" + msgtype + @""", ""text"": """ +  msgtext + @""" }}";
            return msg;
        }

        public static string GenerateCSV(DataTable dt)
        {
            string returnCSV = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string row = "";
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    if (c == 0)
                    {
                        row = row + dt.Rows[i][c].ToString();
                    }
                    else
                    {
                        row = row + "," + dt.Rows[i][c].ToString();
                    }
                }

                row = row + Environment.NewLine;
                returnCSV = returnCSV + row;
            }

            return returnCSV;
        }

        public static string GenerateJSON(DataTable dt)
        {
            string returnJSON = "";

            JArray jsArray1 = new JArray();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var obj = JObject.FromObject(new
                {
                    timestamp = dt.Rows[i]["timestamp"],
                    server = dt.Rows[i]["server"],
                    document = dt.Rows[i]["document"],
                    userid = dt.Rows[i]["userid"]
                });

                jsArray1.Add(obj);
            }

            //string json = JsonConvert.SerializeObject(dt, Formatting.None);
            //JArray jsArray1 = (JArray)JsonConvert.DeserializeObject(json);

            returnJSON = jsArray1.ToString();
            return returnJSON;
        }

    }
}
