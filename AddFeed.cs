using System;
using System.Drawing;
using System.Windows.Forms;
using MaxwellGPUIdle.Properties;

namespace MaxwellGPUIdle
{
    public partial class AddFeed : Form
    {
        private bool can_add;
        private string currentURLInput;

        public AddFeed()
        {
            InitializeComponent();
        }

        private void AddFeed_Load(object sender, EventArgs e)
        {
        }

        private void FeedSubmitButton_Click(object sender, EventArgs e)
        {
            // Get value of field
            if (!can_add)
            {
                return;
            }
            try
            {
                var potential_url = this.currentURLInput;
                if (Helper.ValidateExecutableName(potential_url))
                {
                    Settings.Default.KnownGPUProcesses.Add(potential_url);
                    Settings.Default.Save();
                    MaxwellGPUIdle.ProcessIcon.ni.ContextMenuStrip = new ContextMenus().CreateFeedsMenu();
                    this.Close();
                }
            }
            catch (Exception exception)
            {
                Program.ExceptionHandler(exception);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var text = textBox1.Text;
            //if (text.Contains(".exe"))
            //{
            //    text += ".exe";
            //}
            can_add = Helper.ValidateExecutableName(text);
            if (can_add)
            {
                textBox1.BackColor = Color.Empty;
                can_add = true;
            }
            else
            {
                textBox1.BackColor = Color.Red;
                can_add = false;
            }
            currentURLInput = text;
        }
    }
}
