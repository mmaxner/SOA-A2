﻿using System;
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
        private readonly string _CONFIG_FILENAME = "config.xml";
        private readonly Point ARGUMENT_LABEL_LOCATION = new Point(10, 15);
        private readonly Size ARGUMENT_LABEL_SIZE = new Size(100, 20);
        private readonly Size ARGUMENT_OFFSET = new Size(0, 30);
        private readonly Size ARGUMENT_TEXTBOX_OFFSET = new Size(100, 0);
        private readonly Size ARGUMENT_TEXTBOX_SIZE = new Size(150, 20);

        private string _currentServiceNamespace;
        private XDocument _soapConfig;

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
            try
            {
                _currentServiceNamespace = _soapConfig
                .Descendants("services")
                .Elements("service")
                .Elements("name")
                .Where(x => x.Value == ((ComboBoxItem)cmbService.SelectedItem).Text) // service name
                .Ancestors("service")
                .Elements("namespace")
                .Select(y => y.Value)
                .FirstOrDefault();

                grpArgumentControls.Controls.Clear();
                for (int i = 0; i < CurrentArguments.Count; i++)
                {
                    Label label = new Label();
                    Point location = ARGUMENT_LABEL_LOCATION;
                    for (int j = 0; j < i; j++)
                    {
                        location += ARGUMENT_OFFSET;
                    }
                    label.Location = location;
                    label.Size = ARGUMENT_LABEL_SIZE;
                    label.Text = string.Format("{0} ({1})", CurrentArguments[i].UIName, CurrentArguments[i].DataType);

                    Control control = null;

                    switch (CurrentArguments[i].DataType)
                    {
                        case "list":
                            ComboBox ListPicker = new ComboBox();

                            string XMLList = WebServiceFramework.CallWebService(
                                (cmbService.SelectedItem as ComboBoxItem).Value,
                                CurrentArguments[i].ListSource.ServiceName,
                                new List<SOAPArgument>(),
                                _currentServiceNamespace);

                            BindingSource bindingSource1 = new BindingSource();
                            XDocument docList = XDocument.Parse(XMLList);
                            IEnumerable<XElement> dataList = docList.Descendants().Where(x => x.Name.LocalName == CurrentArguments[i].ListSource.DataMember);
                            IEnumerable<XElement> displayList = docList.Descendants().Where(x => x.Name.LocalName == CurrentArguments[i].ListSource.DisplayMember);

                            Dictionary<string, string> dataSource = new Dictionary<string, string>();

                            for (int j = 0; j < dataList.Count(); j++)
                            {
                                dataSource.Add(dataList.ElementAt(j).Value
                                    , displayList.ElementAt(j).Value);
                            }

                            bindingSource1.DataSource = dataSource;
                            ListPicker.DataSource = bindingSource1;
                            ListPicker.DisplayMember = "Value";
                            ListPicker.ValueMember = "Key";

                            ListPicker.DataBindings.Add("SelectedValue", CurrentArguments[i], "value");

                            control = ListPicker;
                            break;
                        case "date":
                            DateTimePicker DatePicker = new DateTimePicker();
                            DatePicker.Format = DateTimePickerFormat.Custom;
                            DatePicker.CustomFormat = "yyyy-MM-dd";
                            CurrentArguments[i].Value = DateTime.Now.ToString("yyyy-MM-dd");

                            Binding binding = new Binding("Value", CurrentArguments[i], "value", true);

                            DatePicker.DataBindings.Add(binding);

                            control = DatePicker;
                            break;
                        case "int":
                            NumericUpDown NumberPicker = new NumericUpDown();
                            NumberPicker.Minimum = int.MinValue;
                            NumberPicker.Maximum = int.MaxValue;
                            CurrentArguments[i].Value = "0";

                            NumberPicker.DataBindings.Add("Value", CurrentArguments[i], "value", true, DataSourceUpdateMode.OnPropertyChanged);
                            control = NumberPicker;
                            break;
                        case "string":
                        default:
                            TextBox Text = new TextBox();
                            Text.DataBindings.Add("Text", CurrentArguments[i], "value");
                            control = Text;
                            break;
                    }

                    control.Location = location + ARGUMENT_TEXTBOX_OFFSET;
                    control.Size = ARGUMENT_TEXTBOX_SIZE;

                    grpArgumentControls.Controls.Add(label);
                    grpArgumentControls.Controls.Add(control);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error connecting to SOAP service", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

        }

		private async void btnInvoke_ClickAsync(object sender, EventArgs e)
		{
            try
            {
                string url = (cmbService.SelectedItem as ComboBoxItem).Value;
                string action = (cmbMethod.SelectedItem as ComboBoxItem).Value;
				string errorMessage = string.Empty;
				bool isAllValidData = true;

				foreach (SOAPArgument argument in CurrentArguments)
				{
					errorMessage = isValidData(argument, argument.DataType);
					if (!string.IsNullOrEmpty(errorMessage))
					{
						isAllValidData = false;
						break;
					}
				}

				if (isAllValidData)
				{
					string result = WebServiceFramework.CallWebService(url, action, CurrentArguments, _currentServiceNamespace);
					if (!string.IsNullOrEmpty(result))
						txtResults.Clear();
					{
						using (Stream resultStream = GenerateStreamFromString(result))
						{
							await parseXML(resultStream);
						}
					}
				}
				else
				{
					MessageBox.Show(this, errorMessage, "Error parsing argument(s)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
				}
			}
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error connecting to SOAP service", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
		}

		private string isValidData(SOAPArgument argument, string dataType)
		{
			string errorMessage = string.Empty;
			bool isValidData = false;

			switch (dataType)
			{
				case "int":
					isValidData = int.TryParse(argument.Value.ToString(), out int i);
					break;
				case "date":
					isValidData = DateTime.TryParse(argument.Value.ToString(), out DateTime j);
					break;
				case "decimal":
					isValidData = decimal.TryParse(argument.Value.ToString(), out decimal k);
					break;
				case "list":
					isValidData = true;
					break;
				default:
					break;
			}

			if (!isValidData)
			{
				errorMessage = string.Format("Cannot convert \"{0}\" to the data type \"{1}\"", argument.Value.ToString(), dataType);
			}

			return errorMessage;
		}

        private void loadSoapConfig()
        {
            try
            {
                _soapConfig = XDocument.Load(_CONFIG_FILENAME);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading SOAP config file", MessageBoxButtons.OK);
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

            List<string> actions = _soapConfig.Descendants("services")
                .Elements("service")
                .Elements("name")
                .Where(e => e.Value == serviceName)
                .Ancestors("service")
                .Elements("action")
                .Elements("name")
                .Select(f => f.Value).ToList();

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
                            txtResults.AppendText(await reader.GetValueAsync() + Environment.NewLine);
                            //txtResults.Text += await reader.GetValueAsync() + ;
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
            string serviceName = ((ComboBoxItem)cmbService.SelectedItem).Text;
            populateActions(serviceName);
            _currentServiceNamespace = _soapConfig.Descendants("services").Elements("service").Elements("name").Where(x => x.Value == serviceName).Ancestors("service").Elements("namespace").Select(y => y.Value).FirstOrDefault();
        }

        private void cmbMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            populateArguments(((ComboBoxItem)cmbService.SelectedItem).Text, ((ComboBoxItem)cmbMethod.SelectedItem).Text);
        }
    }
}
