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
        readonly Point ARGUMENT_LABEL_LOCATION = new Point(10, 15);
        readonly Size ARGUMENT_LABEL_SIZE = new Size(100, 20);
        readonly Size ARGUMENT_OFFSET = new Size(0, 30);
        readonly Size ARGUMENT_TEXTBOX_OFFSET = new Size(100, 0);
        readonly Size ARGUMENT_TEXTBOX_SIZE = new Size(150, 20);
        XDocument _soapConfig;

        public static List<SOAPArgument> CurrentArguments = new List<SOAPArgument>();

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
            grpArgumentControls.Controls.Clear();
            for (int i = 0; i < CurrentArguments.Count; i++)
            {
                Label label = new Label();
                Point location = ARGUMENT_LABEL_LOCATION;
                for ( int j = 0; j < i; j++)
                {
                    location += ARGUMENT_OFFSET;
                }
                label.Location = location;
                label.Size = ARGUMENT_LABEL_SIZE;
                label.Text = CurrentArguments[i].uiName;

                Control control = null;

                switch (CurrentArguments[i].type)
                {
                    case "list":
                        ComboBox womboCombo = new ComboBox();

                        string XMLList = WebServiceFramework.CallWebService((cmbService.SelectedItem as ComboBoxItem).Value, CurrentArguments[i].listSource.serviceName, new List<SOAPArgument>());
                        

                        BindingSource bindingSource1 = new BindingSource();
                        XDocument docList = XDocument.Parse(XMLList);
                        IEnumerable<XElement> dataList = docList.Descendants().Where(x => x.Name.LocalName == CurrentArguments[i].listSource.dataMember);
                        IEnumerable<XElement> displayList = docList.Descendants().Where(x => x.Name.LocalName == CurrentArguments[i].listSource.displayMember);

                        Dictionary<string, string> dataSource = new Dictionary<string, string>();

                        for (int j = 0; j < dataList.Count(); j++)
                        {
                            dataSource.Add(dataList.ElementAt(j).Value
                                , displayList.ElementAt(j).Value);
                        }

                        bindingSource1.DataSource = dataSource;
                        womboCombo.DataSource = bindingSource1;
                        womboCombo.DisplayMember = "Value";
                        womboCombo.ValueMember = "Key";

                        womboCombo.DataBindings.Add("SelectedValue", CurrentArguments[i], "value");

                        control = womboCombo;
                        break;
                    case "date":
                        /*DateTimePicker dicker = new DateTimePicker();
                        dicker.Format = DateTimePickerFormat.Custom();*/
                        


                        break;
                    case "int":
                    case "string":
                    default:
                        TextBox text = new TextBox();
                        text.DataBindings.Add("Text", CurrentArguments[i], "value");
                        control = text;
                        break;
                }

                control.Location = location + ARGUMENT_TEXTBOX_OFFSET;
                control.Size = ARGUMENT_TEXTBOX_SIZE;

                grpArgumentControls.Controls.Add(label);
                grpArgumentControls.Controls.Add(control);
            }
        }

		private async void btnInvoke_ClickAsync(object sender, EventArgs e)
		{
            string url = (cmbService.SelectedItem as ComboBoxItem).Value;
            string action = (cmbMethod.SelectedItem as ComboBoxItem).Value;

            string result = WebServiceFramework.CallWebService(url, action, CurrentArguments);
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
            cmbMethod.Items.Clear();

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
                .Select(g =>
                {
                    string dataName = g.Element("dataName").Value;
                    string uiName = g.Element("uiName").Value;
                    string type = g.Element("type").Value;
                    SOAPArgument.ArgumentListSource listSource = null;
                    if (type == "list")
                    {
                        var ls = g.Element("listsource");
                        listSource = new SOAPArgument.ArgumentListSource(ls.Element("servicename").Value, ls.Element("displaymember").Value, ls.Element("datamember").Value);

                    }
                    return new SOAPArgument(dataName, uiName, type, listSource);
                })
                .ToList();


            GenerateArgumentControls();
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
                            gridResponse.Rows.Add(await reader.GetValueAsync());
                            
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
        private void cmbService_SelectedIndexChanged(object sender, EventArgs e)
        {
            populateActions(((ComboBoxItem)cmbService.SelectedItem).Text);
        }

        private void cmbMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            populateArguments(((ComboBoxItem)cmbService.SelectedItem).Text, ((ComboBoxItem)cmbMethod.SelectedItem).Text);
        }
    }
}
