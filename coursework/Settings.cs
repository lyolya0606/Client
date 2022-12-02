using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace coursework {
    public partial class Settings : Form {
        public Settings() {
            InitializeComponent();
            textBoxAddress.Text = ConfigurationManager.AppSettings["serviceAddress"];
            textBoxName.Text = ConfigurationManager.AppSettings["serviceName"];
        }

        private void Button1_Click(object sender, EventArgs e) {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Clear();
            config.AppSettings.Settings.Add("serviceAddress", textBoxAddress.Text);
            config.AppSettings.Settings.Add("serviceName", textBoxName.Text);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            this.Close();

        }
    }
}
