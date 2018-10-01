using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SOA___Assignment_2___Web_Services
{
    public class SOAPViewerConfig
    {
        public class SOAPService
        {
            public string Name { get; set; }
            public string URL { get; set; }
            public string NameSpace { get; set; }
            public List<SOAPAction> Actions { get; set; }
        }

        public class SOAPAction
        {
            public class ResultDisplayProperties
            {
                public bool AddSpacingOnAllElements { get; set; }
                public bool TrimAllWhitespace { get; set; }
                public List<string> IgnoreElementList { get; set; }
                public Dictionary<string, string> AddPrefixForElementList { get; set; }
            }

            public string Name { get; set; }
            public ResultDisplayProperties DisplayProperties { get; set; }
            public List<SOAPParameter> Parameters { get; set; }
        }

        public class SOAPParameter
        {
            public string DataName { get; set; }
            public string UIName { get; set; }
            public string Value { get; set; }
            public string DataType { get; set; }
            public List<string> CustomValidationExpressions { get; set; }
            public ArgumentListSource ListSource { get; set; }


            public class ArgumentListSource
            {
                public string ServiceName { get; set; }
                public string DisplayMember { get; set; }
                public string DataMember { get; set; }
            }
        }


        public List<SOAPService> Services;

        public static SOAPViewerConfig LoadFromFile(string file)
        {
            SOAPViewerConfig Config = new SOAPViewerConfig();
            Config.Services = new List<SOAPService>();
            XDocument ConfigContents = XDocument.Load(file);

            List<XElement> services = ConfigContents.Descendants("services").Elements("service").ToList();

            for (int s = 0; s < services.Count; s++)
            {
                SOAPService Service = new SOAPService();
                Service.Name = services[s].Element("name").Value;
                Service.URL = services[s].Element("url").Value;
                Service.NameSpace = services[s].Element("namespace").Value;
                Service.Actions = new List<SOAPAction>();
                List<XElement> Actions = services[s].Elements("action").ToList();
                for (int a = 0; a < Actions.Count; a++)
                {
                    SOAPAction Action = new SOAPAction();
                    Action.Name = Actions[a].Element("name").Value;
                    Action.DisplayProperties = 
                        Actions[a].Elements("resultproperties")
                            .Select(z =>
                            {
                                bool addSpacingOnAllElements = bool.Parse(z.Element("add_line_breaks_on_all_elements").Value);
                                bool trimAllWhitespace = bool.Parse(z.Element("trim_all_spacing").Value);
                                List<string> ignoreElementList = z.Element("ignore_elements").Value.Split(',').ToList();
                                Dictionary<string, string> addPrefixForElementList = new Dictionary<string, string>();
                                var prefixList = z.Element("add_prefix_for_elements").Value.Split('|').ToList();
                                foreach (var prefixCombo in prefixList)
                                {
                                    string value = prefixCombo.Split(',')[0];
                                    string key = prefixCombo.Split(',')[1];
                                    addPrefixForElementList.Add(key, value);
                                }
                                return new SOAPAction.ResultDisplayProperties()
                                {
                                    AddSpacingOnAllElements = addSpacingOnAllElements,
                                    IgnoreElementList = ignoreElementList,
                                    AddPrefixForElementList = addPrefixForElementList,
                                    TrimAllWhitespace = trimAllWhitespace
                                };
                            }).FirstOrDefault();
                    if (Action.DisplayProperties == null)
                    {
                        Action.DisplayProperties = new SOAPAction.ResultDisplayProperties();
                    }
                    Action.Parameters = new List<SOAPParameter>();


                    List<XElement> Parameters = Actions[a].Elements("parameter").ToList();
                    for (int p = 0; p < Parameters.Count; p++)
                    {
                        SOAPParameter Parameter = new SOAPParameter();
                        Parameter.DataName = Parameters[p].Element("dataName").Value;
                        Parameter.UIName = Parameters[p].Element("uiName").Value;
                        Parameter.DataType = Parameters[p].Element("type").Value;

                        Parameter.ListSource = null;
                        if (Parameter.DataType == "list")
                        {
                            XElement ListSource = Parameters[p].Element("listsource");
                            Parameter.ListSource = new SOAPParameter.ArgumentListSource();
                            Parameter.ListSource.ServiceName = ListSource.Element("servicename").Value;
                            Parameter.ListSource.DisplayMember = ListSource.Element("displaymember").Value;
                            Parameter.ListSource.DataMember = ListSource.Element("datamember").Value;
                        }

                        Parameter.CustomValidationExpressions =
                            Parameters[p].Elements()
                            .Where(f => f.Name == "custom_validation")
                            .Any()
                            ?
                                Parameters[p].Element("custom_validation").Elements()
                                .Select(b => string.Format("{0},{1}", b.Name.ToString(), b.Value))
                                .ToList()
                            :
                                new List<string>();

                        Action.Parameters.Add(Parameter);
                    }
                    Service.Actions.Add(Action);
                }
                Config.Services.Add(Service);
            }

            return Config;
        }
    }
}
