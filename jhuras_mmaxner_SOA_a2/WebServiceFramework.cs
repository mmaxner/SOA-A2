using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.IO;

namespace SOA___Assignment_2___Web_Services
{
    public class SOAPArgument
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
            public ArgumentListSource(string serviceNameIn, string displayMemberIn, string dataMemberIn)
            {
                ServiceName = serviceNameIn;
                DisplayMember = displayMemberIn;
                DataMember = dataMemberIn;
            }
        }

        public SOAPArgument(string dataNameIn, string uiNameIn, string typeIn, ArgumentListSource listSourceIn, List<string> customValidationExpression)
        {
            DataName = dataNameIn;
            UIName = uiNameIn;
            DataType = typeIn;
            ListSource = listSourceIn;
            Value = string.Empty;
			CustomValidationExpressions = customValidationExpression;
        }
    }

	public class ResultDisplayProperties
	{
		public bool AddSpacingOnAllElements { get; set; }
		public bool TrimAllWhitespace { get; set; }
		public List<string> IgnoreElementList { get; set; }
		public Dictionary<string,string> AddPrefixForElementList { get; set; }
	}

	//https://stackoverflow.com/questions/4791794/client-to-send-soap-request-and-received-response
	public class WebServiceFramework
	{
		public static string CallWebService(string url, string action, List<SOAPArgument> parameters, string serviceNamespace)
		{
			string soapResult = string.Empty;
			XmlDocument soapEnvelopeXml = createSoapEnvelope(action, parameters, serviceNamespace);
			HttpWebRequest webRequest = createWebRequest(url, action, serviceNamespace);
			insertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

			// begin async call to web request
			IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

			// suspend this thread until call is complete
			asyncResult.AsyncWaitHandle.WaitOne();

			using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
			{
				using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
				{
					soapResult = rd.ReadToEnd();
				}
			}

			return soapResult;
		}

		private static HttpWebRequest createWebRequest(string url, string action, string serviceNamespace)
		{
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
			webRequest.Headers.Add("SOAPAction", serviceNamespace + action);
			webRequest.ContentType = "text/xml;charset=\"utf-8\"";
			webRequest.Accept = "text/xml";
			webRequest.Method = "POST";
			return webRequest;
		}

		private static XmlDocument createSoapEnvelope(string action, List<SOAPArgument> parameters, string serviceNamespace)
		{
			XmlDocument soapEnvelopeDocument = new XmlDocument();
			string loadXmlData =
				@"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
					<soap:Body>";

			loadXmlData += string.Format(@"<{0} xmlns=""{1}"">", action, serviceNamespace);

			// do this part in a for loop for however many arguments we need?
            for (int i = 0; i < parameters.Count; i++)
            {
                loadXmlData +=  string.Format("<{0}>{1}</{0}>", parameters[i].DataName, parameters[i].Value);
            }
			loadXmlData += string.Format(@"</{0}>", action);
			loadXmlData +=
					@"</soap:Body>
				</soap:Envelope>";
			soapEnvelopeDocument.LoadXml(loadXmlData);
			return soapEnvelopeDocument;
		}

		private static void insertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
		{
			using (Stream stream = webRequest.GetRequestStream())
			{
				soapEnvelopeXml.Save(stream);
			}
		}
	}
}
