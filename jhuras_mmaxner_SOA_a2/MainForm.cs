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

            // load all settings in the xml config file
            loadSoapConfig();

            // populate combo box 1 with whatever is in the txt file
            populateServices();

			// populate combo box 2 with a list of methods from the first url in the txt file (selected in combo box 1)
			populateActions(((ComboBoxItem)cmbService.SelectedItem).Text);
            
            populateArguments(((ComboBoxItem)cmbService.SelectedItem).Text, ((ComboBoxItem)cmbMethod.SelectedItem).Text);
        }

        /// <summary>
        ///     Creates a set of controls for manipulating arguments to be sent in a SOAP call, based on CurrentArguments.
        ///     Attaches thse controls to grpArgumentControls, after clearing existing controls from it.
        /// </summary>
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
                    label.Text = CurrentArguments[i].uiName;

                    Control control = null;

                    // based on the type of parameter, create and data bind a control for the user to enter a value with
                    switch (CurrentArguments[i].type)
                    {
                        case "list":
                            // lists are displayed in a combobox
                            // the items in the combo box are populated by another action, detailed in the list source of the Current Argument
                            ComboBox ListPicker = new ComboBox();

                            // retrieve the list of items from the service
                            string XMLList = WebServiceFramework.CallWebService(
                                (cmbService.SelectedItem as ComboBoxItem).Value,
                                CurrentArguments[i].listSource.serviceName,
                                new List<SOAPArgument>(),
                                _currentServiceNamespace);

                            // create a binding source, find the values for display/data
                            BindingSource bindingSource1 = new BindingSource();
                            XDocument docList = XDocument.Parse(XMLList);
                            IEnumerable<XElement> dataList = docList.Descendants().Where(x => x.Name.LocalName == CurrentArguments[i].listSource.dataMember);
                            IEnumerable<XElement> displayList = docList.Descendants().Where(x => x.Name.LocalName == CurrentArguments[i].listSource.displayMember);

                            // store the list in a dictionary
                            Dictionary<string, string> dataSource = new Dictionary<string, string>();
                            for (int j = 0; j < dataList.Count(); j++)
                            {
                                dataSource.Add(dataList.ElementAt(j).Value
                                    , displayList.ElementAt(j).Value);
                            }

                            // set the data source to the dictionary, and bind to the CurrentArgument
                            bindingSource1.DataSource = dataSource;
                            ListPicker.DataSource = bindingSource1;
                            ListPicker.DisplayMember = "Value";
                            ListPicker.ValueMember = "Key";
                            ListPicker.DataBindings.Add("SelectedValue", CurrentArguments[i], "value");

                            control = ListPicker;
                            break;
                        case "date":
                            // dates use a datetimepicker
                            DateTimePicker DatePicker = new DateTimePicker();
                            DatePicker.Format = DateTimePickerFormat.Custom;
                            DatePicker.CustomFormat = "yyyy-MM-dd";
                            CurrentArguments[i].value = DateTime.Now.ToString("yyyy-MM-dd");

                            Binding binding = new Binding("Value", CurrentArguments[i], "value", true);

                            DatePicker.DataBindings.Add(binding);

                            control = DatePicker;
                            break;
                        case "int":
                            // ints used a numeric up/down
                            NumericUpDown NumberPicker = new NumericUpDown();
                            NumberPicker.Minimum = int.MinValue;
                            NumberPicker.Maximum = int.MaxValue;
                            CurrentArguments[i].value = "0";

                            NumberPicker.DataBindings.Add("Value", CurrentArguments[i], "value", true, DataSourceUpdateMode.OnPropertyChanged);
                            control = NumberPicker;
                            break;
                        case "string":
                        default:
                            // strings, and anything else, are a free form textbox
                            TextBox Text = new TextBox();
                            Text.DataBindings.Add("Text", CurrentArguments[i], "value");
                            control = Text;
                            break;
                    }

                    // set the location of the control so that multiple controls don't overlap
                    control.Location = location + ARGUMENT_TEXTBOX_OFFSET;
                    control.Size = ARGUMENT_TEXTBOX_SIZE;
                    // add the control to the group box
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
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error connecting to SOAP service", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
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
                    if (reader.NodeType == XmlNodeType.Text)
                    {
                        txtResults.AppendText(await reader.GetValueAsync() + Environment.NewLine);
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
