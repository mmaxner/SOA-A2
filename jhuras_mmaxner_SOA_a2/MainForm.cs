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
using System.Xml.Linq;

namespace SOA___Assignment_2___Web_Services
{
	public partial class MainForm : System.Windows.Forms.Form
	{
        const string _CONFIG_FILENAME = "config.xml";
        XDocument _soapConfig;


        public MainForm()
		{
			InitializeComponent();

            // load all settings in the xml config file
            loadSoapConfig();

            // populate combo box 1 with whatever is in the txt file
            populateServices();

			// populate combo box 2 with a list of methods from the first url in the txt file (selected in combo box 1)
			populateActions(cmbService.SelectedText);
		}

		private async void btnInvoke_ClickAsync(object sender, EventArgs e)
		{
            string url = (cmbService.SelectedItem as ComboBoxItem).Value;
            string action = (cmbMethod.SelectedItem as ComboBoxItem).Value;
            List<string> parameters = txtArguments.Text.Split(',').OfType<string>().ToList(); // convert parameters to separated list

            string result = WebServiceFramework.CallWebService(url, action);
			if (!string.IsNullOrEmpty(result))
			{
				using (Stream resultStream = GenerateStreamFromString(result))
				{
					await parseXML(resultStream);
				}
			}
		}

        private void loadSoapConfig()
        {
            try
            {
                _soapConfig = XDocument.Load(_CONFIG_FILENAME);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK);
            }
        }

		private void populateServices()
		{
            List<string> services = _soapConfig.Descendants("services").Elements("service").Elements("name").Select(e => e.Value).ToList();
            List<string> urls = _soapConfig.Descendants("services").Elements("service").Elements("url").Select(e => e.Value).ToList();
            for (int i = 0; i < urls.Count; i++)
            {
                cmbService.Items.Add(new ComboBoxItem() { Text = services[i], Value = urls[i] });
            }

            if (cmbService.Items.Count > 0)
            {
                cmbService.SelectedIndex = 0;
            }
        }

		private void populateActions(string serviceName)
		{
            List<string> actions = _soapConfig.Descendants("services").Elements("service").Elements("action").Elements("name").Select(e => e.Value).ToList();
            foreach (var action in actions)
            {
                cmbMethod.Items.Add(new ComboBoxItem() { Text = action, Value = action });
            }

            if (cmbMethod.Items.Count > 0)
            {
                cmbMethod.SelectedIndex = 0;
            }
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
