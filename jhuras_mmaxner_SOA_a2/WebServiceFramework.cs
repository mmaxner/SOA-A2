using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using System.IO;

namespace SOA___Assignment_2___Web_Services
{
	//https://stackoverflow.com/questions/4791794/client-to-send-soap-request-and-received-response
	public class WebServiceFramework
	{
		public static string CallWebService(string url, string action)
		{
			string soapResult = string.Empty;
			XmlDocument soapEnvelopeXml = CreateSoapEnvelope(action);
			HttpWebRequest webRequest = CreateWebRequest(url, action);
			InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

			// begin async call to web request.
			IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

			// suspend this thread until call is complete. You might want to
			// do something usefull here like update your UI.
			asyncResult.AsyncWaitHandle.WaitOne();

			// get the response from the completed web request.
			//using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
			//{
			//	result = webResponse.GetResponseStream();
			//}
			using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
			{
				using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
				{
					soapResult = rd.ReadToEnd();
				}
			}

			return soapResult;
		}

		private static HttpWebRequest CreateWebRequest(string url, string action)
		{
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
			webRequest.Headers.Add("SOAPAction", "http://tempuri.org/" + action);
			webRequest.ContentType = "text/xml;charset=\"utf-8\"";
			webRequest.Accept = "text/xml";
			webRequest.Method = "POST";
			return webRequest;
		}

		private static XmlDocument CreateSoapEnvelope(string action)
		{
			XmlDocument soapEnvelopeDocument = new XmlDocument();
			string loadXmlData =
				@"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
					<soap:Body>";

			loadXmlData += string.Format(@"<{0} xmlns=""http://tempuri.org/"">", action);

			// do this part in a for loop for however many arguments we need?
			loadXmlData += @"<intA>12</intA>
							<intB>32</intB>";
			loadXmlData += string.Format(@"</{0}>", action);
			loadXmlData +=
					@"</soap:Body>
				</soap:Envelope>";
			soapEnvelopeDocument.LoadXml(loadXmlData);
			return soapEnvelopeDocument;
		}

		private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
		{
			using (Stream stream = webRequest.GetRequestStream())
			{
				soapEnvelopeXml.Save(stream);
			}
		}
	}
}
