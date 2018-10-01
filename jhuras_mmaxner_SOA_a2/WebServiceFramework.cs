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
    // handles creating, sending, and receiving SOAP messages
    // based on:
	//https://stackoverflow.com/questions/4791794/client-to-send-soap-request-and-received-response
	public class WebServiceFramework
	{
        /// <summary>
        ///     Makes a SOAP request
        /// </summary>
        /// <param name="url">URL of the service</param>
        /// <param name="action">the method being called</param>
        /// <param name="parameters">the parameters being sent</param>
        /// <param name="serviceNamespace">the namespace to be used</param>
        /// <returns></returns>
		public static string CallWebService(string url, string action, List<SOAPViewerConfig.SOAPParameter> parameters, string serviceNamespace)
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

        /// <summary>
        ///     creates a SOAP request
        /// </summary>
		private static HttpWebRequest createWebRequest(string url, string action, string serviceNamespace)
		{
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
			webRequest.Headers.Add("SOAPAction", serviceNamespace + action);
			webRequest.ContentType = "text/xml;charset=\"utf-8\"";
			webRequest.Accept = "text/xml";
			webRequest.Method = "POST";
			return webRequest;
		}

        /// <summary>
        ///  Generates the formatted SOAP envelope
        /// </summary>
		private static XmlDocument createSoapEnvelope(string action, List<SOAPViewerConfig.SOAPParameter> parameters, string serviceNamespace)
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

        /// <summary>
        ///     adds the SOAP evelope to the web request
        /// </summary>
		private static void insertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
		{
			using (Stream stream = webRequest.GetRequestStream())
			{
				soapEnvelopeXml.Save(stream);
			}
		}
	}
}
