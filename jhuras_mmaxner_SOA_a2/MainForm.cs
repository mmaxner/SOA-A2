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
        //const int ARGUMENT_LABEL_LOCATION = 110;
        readonly Point ARGUMENT_LABEL_LOCATION = new Point(110, 95);
        // const int ARGUMENT_LABEL_TOP = 95;
        readonly Size ARGUMENT_LABEL_SIZE = new Size(100, 20);
        const int ARGUMENT_VERTICAL_OFFSET = 30;
        //const int ARGUMENT_LABEL_HEIGHT = 20;
        //const int ARGUMENT_LABEL_WIDTH = 100;
        readonly Point ARGUMENT_TEXTBOX_OFFSET = new Point(100, 0);
        readonly Size ARGUMENT_TEXTBOX_SIZE = new Size(150, 20);
        //const int ARGUMENT_TEXTBOX_LEFT_OFFSET = 110;
        //const int ARGUMENT_VERTICAL_OFFSET = 30;
        XDocument _soapConfig;

        public List<SOAPArgument> CurrentArguments = new List<SOAPArgument>();

        public MainForm()
		{
			InitializeComponent();
            /*gridArguments.Columns.Add("argumentName", "Argument");
            gridArguments.Columns.Add("argumentValue", "Value");*/
            

            // load all settings in the xml config file
            loadSoapConfig();

            // populate combo box 1 with whatever is in the txt file
            populateServices();

			// populate combo box 2 with a list of methods from the first url in the txt file (selected in combo box 1)
			populateActions(((ComboBoxItem)cmbService.SelectedItem).Text);

            populateArguments(((ComboBoxItem)cmbService.SelectedItem).Text, ((ComboBoxItem)cmbMethod.SelectedItem).Text);
        }

        public void GenerateArgumentControls()
        {
            for (int i = 0; i < CurrentArguments.Count; i++)
            {
                Label label = new Label();
                label.Location = ARGUMENT_LABEL_LOCATION;
                label.Location.Offset(0, ARGUMENT_VERTICAL_OFFSET * i);
                label.Size = ARGUMENT_LABEL_SIZE;
                label.Text = CurrentArguments[i].uiName;

                TextBox text = new TextBox();
                text.Location = ARGUMENT_LABEL_LOCATION;
                text.Location.Offset(0, ARGUMENT_VERTICAL_OFFSET);
                text.Location.Offset(ARGUMENT_TEXTBOX_OFFSET);
                text.Size = ARGUMENT_TEXTBOX_SIZE;
                
                text.DataBindings.Add("Text", CurrentArguments[i], "value");

                this.Controls.Add(label);
                this.Controls.Add(text);
            }
        }

		private async void btnInvoke_ClickAsync(object sender, EventArgs e)
		{
            string url = (cmbService.SelectedItem as ComboBoxItem).Value;
            string action = (cmbMethod.SelectedItem as ComboBoxItem).Value;
            //List<string> parameters = txtArguments.Text.Split(',').OfType<string>().ToList(); // convert parameters to separated list

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
            List<string> actions = _soapConfig.Descendants("services").Elements("service").Elements("name").Where(e => e.Value == serviceName).Ancestors("service").Elements("action").Elements("name").Select(f => f.Value).ToList();

            foreach (var action in actions)
            {
                cmbMethod.Items.Add(new ComboBoxItem() { Text = action, Value = action });
            }

            if (cmbMethod.Items.Count > 0)
            {
                cmbMethod.SelectedIndex = 0;
            }
        }

        private void populateArguments(string serviceName, string methodName)
        {
            CurrentArguments = _soapConfig.Descendants("services")
                .Elements("service")
                .Elements("name")
                .Where(e => e.Value == serviceName)
                .Ancestors("service")
                .Elements("action")
                .Elements("name")
                .Where(f => f.Value == methodName)
                .Ancestors("action")
                .Elements("parameter")
                .Select(g => new SOAPArgument(g.Element("dataName").Value, g.Element("uiName").Value))
                .ToList();


            GenerateArgumentControls();
            /*foreach (var argument in arguments)
            {
                gridArguments.Rows.Add(argument.uiName, string.Empty);
            }*/
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
