using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SOA___Assignment_2___Web_Services
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        public MainForm()
        {
            InitializeComponent();
			// populate combo box 1 with whatever is in the txt file
			populateServices();

			// populate combo box 2 with a list of methods from the first url in the txt file (selected in combo box 1)
			populateActions("replace me");
		}

		private async void btnInvoke_ClickAsync(object sender, EventArgs e)
		{
			//string url = (cmbService.SelectedItem as ComboBoxItem).Value;
			//string action = (cmbMethod.SelectedItem as ComboBoxItem).Value;
			//List<string> parameters = txtArguments.Text.Split(',').OfType<string>().ToList(); // convert parameters to separated list

			// todo - remove magic numbers, very bad
			string url = "http://www.dneonline.com/calculator.asmx";
			string action = "Add";

			string result = WebServiceFramework.CallWebService(url, action);
			if (!string.IsNullOrEmpty(result))
			{
				using (Stream resultStream = GenerateStreamFromString(result))
				{
					await parseXML(resultStream);
				}
			}
		}

		private void populateServices()
		{
			// todo
		}

		private void populateActions(string serviceName)
		{
			// todo
		}

		//https://stackoverflow.com/questions/1879395/how-do-i-generate-a-stream-from-a-string
		private Stream GenerateStreamFromString(string s)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		// from msdn <3
		private async Task parseXML(System.IO.Stream stream)
		{
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.Async = true;

			using (XmlReader reader = XmlReader.Create(stream, settings))
			{
				while (await reader.ReadAsync())
				{
					switch (reader.NodeType)
					{
						case XmlNodeType.Element:
							Console.WriteLine("Start Element {0}", reader.Name);
							break;
						case XmlNodeType.Text:
							Console.WriteLine("Text Node: {0}",
									 await reader.GetValueAsync());
							break;
						case XmlNodeType.EndElement:
							Console.WriteLine("End Element {0}", reader.Name);
							break;
						default:
							Console.WriteLine("Other node {0} with value {1}",
											reader.NodeType, reader.Value);
							break;
					}
				}
			}
		}
	}
}
