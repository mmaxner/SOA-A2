using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace SOA___Assignment_2___Web_Services
{
	public partial class MainForm : System.Windows.Forms.Form
	{
        private readonly Point ARGUMENT_LABEL_LOCATION = new Point(10, 15);
        private readonly Size ARGUMENT_LABEL_SIZE = new Size(100, 20);
        private readonly Size ARGUMENT_OFFSET = new Size(0, 30);
        private readonly Size ARGUMENT_TEXTBOX_OFFSET = new Size(100, 0);
        private readonly Size ARGUMENT_TEXTBOX_SIZE = new Size(150, 20);

        public SOAPViewerConfig config;
        public SOAPViewerConfig.SOAPService CurrentService;
        public SOAPViewerConfig.SOAPAction CurrentAction;

        private const string CONFIG_FILENAME = "config.xml";
		private const string ERROR_CANNOT_CONVERT_ARGUMENT = "Cannot convert \"{0}\" to the data type \"{1}\"";
		private const string ERROR_CONNECTING_TO_SERVICE = "Error connecting to SOAP service";
		private const string ERROR_PARSING_ARGUMENT = "Error parsing argument(s)";
		private const string ERROR_DEFAULT = "Something went wrong";
		private const string DATA_TYPE_IDENTIFIER_INT = "int";
		private const string DATA_TYPE_IDENTIFIER_DECIMAL = "decimal";
		private const string DATA_TYPE_IDENTIFIER_STRING = "string";
		private const string DATA_TYPE_IDENTIFIER_DATETIME = "date";
		private const string DATA_TYPE_IDENTIFIER_LIST = "list";

        public MainForm()
		{
			InitializeComponent();

            // load all settings in the xml config file
            loadSoapConfig();

            // populate combo box 1 with whatever is in the txt file
            populateServices();

			// populate combo box 2 with a list of methods from the first url in the txt file (selected in combo box 1)
			populateActions();
            
            populateArguments();
        }

        /// <summary>
        ///     Creates a set of controls for manipulating arguments to be sent in a SOAP call, based on CurrentArguments.
        ///     Attaches thse controls to grpArgumentControls, after clearing existing controls from it.
        /// </summary>
        public void GenerateArgumentControls()
        {
            try
            {
                grpArgumentControls.Controls.Clear();
                for (int p = 0; p < CurrentAction.Parameters.Count; p++)
                {
                    Label label = new Label();
                    Point location = ARGUMENT_LABEL_LOCATION;
                    for (int j = 0; j < p; j++)
                    {
                        location += ARGUMENT_OFFSET;
                    }
                    label.Location = location;
                    label.Size = ARGUMENT_LABEL_SIZE;
                    label.Text = string.Format("{0} ({1})", CurrentAction.Parameters[p].UIName, CurrentAction.Parameters[p].DataType);

                    Control control = null;

                    // based on the type of parameter, create and data bind a control for the user to enter a value with
                    switch (CurrentAction.Parameters[p].DataType)
                    {
                        // lists are displayed in a combobox
                        // the items in the combo box are populated by another action, detailed in the list source of the Current Argument
                        case DATA_TYPE_IDENTIFIER_LIST:
                            ComboBox listPicker = new ComboBox();

                            // retrieve the list of items from the service
                            string XMLList = WebServiceFramework.CallWebService(
                                CurrentService.URL,
                                CurrentAction.Parameters[p].ListSource.ServiceName,
                                new List<SOAPViewerConfig.SOAPParameter>(),
                                CurrentService.NameSpace);

                            // create a binding source, find the values for display/data
                            BindingSource bindingSource1 = new BindingSource();
                            XDocument docList = XDocument.Parse(XMLList);
                            IEnumerable<XElement> dataList = docList.Descendants().Where(x => x.Name.LocalName == CurrentAction.Parameters[p].ListSource.DataMember);
                            IEnumerable<XElement> displayList = docList.Descendants().Where(x => x.Name.LocalName == CurrentAction.Parameters[p].ListSource.DisplayMember);

                            // store the list in a dictionary
                            Dictionary<string, string> dataSource = new Dictionary<string, string>();
                            for (int j = 0; j < dataList.Count(); j++)
                            {
                                dataSource.Add(dataList.ElementAt(j).Value
                                    , displayList.ElementAt(j).Value);
                            }

                            // set the data source to the dictionary, and bind to the CurrentArgument
                            bindingSource1.DataSource = dataSource;
                            listPicker.DataSource = bindingSource1;
                            listPicker.DisplayMember = "Value";
                            listPicker.ValueMember = "Key";

                            listPicker.DataBindings.Add("SelectedValue", CurrentAction.Parameters[p], "Value");

                            control = listPicker;
                            break;
                        case DATA_TYPE_IDENTIFIER_DATETIME:
                            DateTimePicker datePicker = new DateTimePicker();
                            datePicker.Format = DateTimePickerFormat.Custom;
                            datePicker.CustomFormat = "yyyy-MM-dd";
                            CurrentAction.Parameters[p].Value = DateTime.Now.ToString("yyyy-MM-dd");

                            Binding binding = new Binding("Value", CurrentAction.Parameters[p], "Value", true);

                            datePicker.DataBindings.Add(binding);

                            control = datePicker;
                            break;
                        case DATA_TYPE_IDENTIFIER_INT:
                            NumericUpDown numberPicker = new NumericUpDown();
                            numberPicker.Minimum = int.MinValue;
                            numberPicker.Maximum = int.MaxValue;
                            CurrentAction.Parameters[p].Value = "0";

                            numberPicker.DataBindings.Add("Value", CurrentAction.Parameters[p], "Value", true, DataSourceUpdateMode.OnPropertyChanged);
                            control = numberPicker;
                            break;
                        case DATA_TYPE_IDENTIFIER_STRING:
                        default:
                            TextBox textBox = new TextBox();
                            textBox.DataBindings.Add("Text", CurrentAction.Parameters[p], "Value");
                            control = textBox;
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
            catch (System.Net.WebException ex)
            {
                MessageBox.Show(this, ex.Message, ERROR_CONNECTING_TO_SERVICE, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, ERROR_DEFAULT, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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

				foreach (SOAPViewerConfig.SOAPParameter argument in CurrentAction.Parameters)
				{
					errorMessage = isValidData(argument);
					if (!string.IsNullOrEmpty(errorMessage))
					{
						isAllValidData = false;
						break;
					}
				}

				if (isAllValidData)
				{
					string result = WebServiceFramework.CallWebService(CurrentService.URL, CurrentAction.Name, CurrentAction.Parameters, CurrentService.NameSpace);
					if (!string.IsNullOrEmpty(result))
					{
						txtResults.Clear();
						using (Stream resultStream = GenerateStreamFromString(result))
						{
							await parseAndDisplayResponse(resultStream);
						}
					}
				}
				else
				{
					MessageBox.Show(this, errorMessage, ERROR_PARSING_ARGUMENT, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
				}
			}
			catch (System.Net.WebException ex)
			{
				MessageBox.Show(this, ex.Message, ERROR_CONNECTING_TO_SERVICE, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}
			catch (Exception ex)
            {
				MessageBox.Show(this, ex.Message, ERROR_DEFAULT, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
		}

		private string isValidData(SOAPViewerConfig.SOAPParameter argument)
		{
			string errorMessage = string.Empty;
			bool isValidData = false;
			bool isCustomValid = true;

			switch (argument.DataType)
			{
				case DATA_TYPE_IDENTIFIER_INT:
					isValidData = int.TryParse(argument.Value.ToString(), out int i);
					break;
				case DATA_TYPE_IDENTIFIER_DATETIME:
					isValidData = DateTime.TryParse(argument.Value.ToString(), out DateTime j);
					break;
				case DATA_TYPE_IDENTIFIER_DECIMAL:
					isValidData = decimal.TryParse(argument.Value.ToString(), out decimal k);
					break;
				case DATA_TYPE_IDENTIFIER_STRING:
				case DATA_TYPE_IDENTIFIER_LIST:
					isValidData = true;
					break;
				default:
					break;
			}

			if (!isValidData)
			{
				errorMessage = string.Format(ERROR_CANNOT_CONVERT_ARGUMENT, argument.Value.ToString(), argument.DataType);
			}

			if (isValidData)
			{
				if (argument.CustomValidationExpressions.Any())
				{
					foreach (string expressionCombo in argument.CustomValidationExpressions)
					{
						string expression = expressionCombo.Split(',')[0];
						string value = expressionCombo.Split(',')[1];
						int convertedValue = 0;
						switch (expression)
						{
							case "equal":
								isCustomValid = argument.Value == value;
								break;
							case "not_equal":
								isCustomValid = argument.Value != value;
								break;
							case "greater_than":
								if (int.TryParse(value, out convertedValue))
								{
									isCustomValid = int.Parse(argument.Value) > convertedValue;
								}
								else
								{
									isCustomValid = false;
								}
								break;
							case "less_than":
								if (int.TryParse(value, out convertedValue))
								{
									isCustomValid = int.Parse(argument.Value) < convertedValue;
								}
								else
								{
									isCustomValid = false;
								}
								break;
							default:
								break;
						}

						if (!isCustomValid)
						{
							errorMessage = string.Format("The following custom validation has not been met:{0}{1} {2} {3}", Environment.NewLine, argument.UIName, expression, value);
							break;
						}
					}
				}
			}

			return errorMessage;
		}

        private void loadSoapConfig()
        {
            try
            {
                config = SOAPViewerConfig.LoadFromFile(CONFIG_FILENAME);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading SOAP config file", MessageBoxButtons.OK);
            }
        }

		private void populateServices()
		{
            for (int i = 0; i < config.Services.Count; i++)
            {
                cmbService.Items.Add(new ComboBoxItem() { Text = config.Services[i].Name, Value = i.ToString() });
            }
            if (cmbService.Items.Count > 0)
            {
                cmbService.SelectedIndex = 0;
            }
        }

		private void populateActions()
		{
            cmbMethod.Items.Clear();

            for (int a = 0; a < CurrentService.Actions.Count; a++)
            {
                cmbMethod.Items.Add(new ComboBoxItem() { Text = CurrentService.Actions[a].Name, Value = a.ToString() });
            }

            if (cmbMethod.Items.Count > 0)
            {
                cmbMethod.SelectedIndex = 0;
            }
        }

        private void populateArguments()
        {
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
		private async Task parseAndDisplayResponse(System.IO.Stream stream)
		{
            SOAPViewerConfig.SOAPAction.ResultDisplayProperties displayProperties = CurrentAction.DisplayProperties;

            XmlReaderSettings settings = new XmlReaderSettings();
			settings.Async = true;
			bool ignoreNextText = false;

			using (XmlReader reader = XmlReader.Create(stream, settings))
			{
				while (await reader.ReadAsync())
				{
					switch (reader.NodeType)
					{
						case XmlNodeType.Element:
							if (displayProperties.IgnoreElementList != null && displayProperties.IgnoreElementList.Contains(reader.Name))
							{
								ignoreNextText = true;
							}
							else if (displayProperties.AddPrefixForElementList != null && displayProperties.AddPrefixForElementList.ContainsKey(reader.Name))
							{
								txtResults.AppendText(string.Format("{0}: ", displayProperties.AddPrefixForElementList[reader.Name]));
							}
							break;
						case XmlNodeType.Text:
							if (!ignoreNextText)
							{
								string resultText = await reader.GetValueAsync();
								if (displayProperties.TrimAllWhitespace)
								{
									// remove all excess whitespace
									resultText = Regex.Replace(resultText, @"[ ]{2,}", Environment.NewLine, RegexOptions.Multiline); // when there's 2 or more spaces, interpret as a new line
								}

								txtResults.AppendText(resultText + Environment.NewLine);

								if (displayProperties.AddSpacingOnAllElements)
								{
									txtResults.AppendText(Environment.NewLine);
								}
							}
							else
							{
								ignoreNextText = false;
							}
                            break;
						case XmlNodeType.EndElement:
							break;
						default:
							break;
					}
				}
			}
		}
        private void cmbService_SelectedIndexChanged(object sender, EventArgs e)
        {
            int serviceIndex = int.Parse(((ComboBoxItem)cmbService.SelectedItem).Value);
            CurrentService = config.Services[serviceIndex];
            populateActions();
        }

        private void cmbMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            int ActionIndex = int.Parse(((ComboBoxItem)cmbMethod.SelectedItem).Value);
            CurrentAction = CurrentService.Actions[ActionIndex];
			populateArguments();
        }
    }
}
