using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SOA___Assignment_2___Web_Services
{
    /// <summary>
    ///     Parses and stores the data in a config file for a SOAP viewer
    /// </summary>
    public class SOAPViewerConfig
    {
        /// <summary>
        ///     The details for one SOAP service
        /// </summary>
        public class SOAPService
        {
            public string Name { get; set; }    // a display name
            public string URL { get; set; }     // the URL of the service
            public string NameSpace { get; set; }   // the namespace to use when calling the service
            public List<SOAPAction> Actions { get; set; }   // a list of Actions supported for the service
        }

        /// <summary>
        ///     The details for one Action provided by a SOAP Service
        /// </summary>
        public class SOAPAction
        {
            public class ResultDisplayProperties
            {
                public bool AddSpacingOnAllElements { get; set; }
                public bool TrimAllWhitespace { get; set; }
                public List<string> IgnoreElementList { get; set; }
                public Dictionary<string, string> AddPrefixForElementList { get; set; }
            }

            public string Name { get; set; }    // the name of the method
            public ResultDisplayProperties DisplayProperties { get; set; }      // special rules for displaying the results ofthe method
            public List<SOAPParameter> Parameters { get; set; }     // the parameters needed for the method
        }

        public class SOAPParameter
        {
            public string DataName { get; set; }        // the name of the parameter when sent in a SOAP envelope
            public string UIName { get; set; }          // the name of the parameter when displayed to the user
            public string Value { get; set; }           // the Value entered by the user
            public string DataType { get; set; }        // the data type of the parameter
            public List<string> CustomValidationExpressions { get; set; }   // special rules for validating the Value
            public ArgumentListSource ListSource { get; set; }      // special rules for retrieving a list of possible values from the service


            public class ArgumentListSource
            {
                public string ServiceName { get; set; }
                public string DisplayMember { get; set; }
                public string DataMember { get; set; }
            }
        }

        /// <summary>
        ///     The list of services supported by this config file
        /// </summary>
        public List<SOAPService> Services;

        /// <summary>
        ///     Reads a config file, parses it, and populates itself with the data it reads.
        ///     Returns a SOAPViewerConfig with all the details from the config file.
        /// </summary>
        /// <param name="file">filename to parse data from</param>
        /// <returns></returns>
        public static SOAPViewerConfig LoadFromFile(string file)
        {
            SOAPViewerConfig Config = new SOAPViewerConfig();
            Config.Services = new List<SOAPService>();
            XDocument ConfigContents = XDocument.Load(file);

            // for each service node
            List<XElement> services = ConfigContents.Descendants("services").Elements("service").ToList();
            for (int s = 0; s < services.Count; s++)
            {
                // read the service details
                SOAPService Service = new SOAPService();
                Service.Name = services[s].Element("name").Value;
                Service.URL = services[s].Element("url").Value;
                Service.NameSpace = services[s].Element("namespace").Value;
                Service.Actions = new List<SOAPAction>();
                // for each action node in this service
                List<XElement> Actions = services[s].Elements("action").ToList();
                for (int a = 0; a < Actions.Count; a++)
                {
                    // read the action details
                    SOAPAction Action = new SOAPAction();
                    Action.Name = Actions[a].Element("name").Value;
                    Action.DisplayProperties = 
                        Actions[a].Elements("resultproperties")
                            .Select(z =>
                            {
                                SOAPAction.ResultDisplayProperties displayProperties = new SOAPAction.ResultDisplayProperties();
                                if (z.Element("add_line_breaks_on_all_elements") != null)
                                {
                                    displayProperties.AddSpacingOnAllElements = bool.Parse(z.Element("add_line_breaks_on_all_elements").Value);
                                }
                                if (z.Element("trim_all_spacing") != null)
                                {
                                    displayProperties.TrimAllWhitespace = bool.Parse(z.Element("trim_all_spacing").Value);
                                }
                                if (z.Element("ignore_elements") != null)
                                {
                                    displayProperties.IgnoreElementList = z.Element("ignore_elements").Value.Split(',').ToList();
                                }
                                if (z.Element("add_prefix_for_elements") != null)
                                {
                                    Dictionary<string, string> addPrefixForElementList = new Dictionary<string, string>();
                                    var prefixList = z.Element("add_prefix_for_elements").Value.Split('|').ToList();
                                    foreach (var prefixCombo in prefixList)
                                    {
                                        string value = prefixCombo.Split(',')[0];
                                        string key = prefixCombo.Split(',')[1];
                                        addPrefixForElementList.Add(key, value);
                                    }
                                    displayProperties.AddPrefixForElementList = addPrefixForElementList;
                                }

                                return displayProperties;
                            }).FirstOrDefault();
                    if (Action.DisplayProperties == null)
                    {
                        Action.DisplayProperties = new SOAPAction.ResultDisplayProperties();
                    }
                    Action.Parameters = new List<SOAPParameter>();

                    // for each parameter node in this action
                    List<XElement> Parameters = Actions[a].Elements("parameter").ToList();
                    for (int p = 0; p < Parameters.Count; p++)
                    {
                        // read the parameter details
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
                        // add the parameter details to the Action
                        Action.Parameters.Add(Parameter);
                    }
                    // Add the action to the service
                    Service.Actions.Add(Action);
                }
                // add the ervice to the list of supported services
                Config.Services.Add(Service);
            }

            //return the populated object
            return Config;
        }
    }
}
