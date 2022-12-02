using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.ServiceModel;
using LibraryWithLRU_NRU;
using System.Configuration;

namespace coursework {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }

        private enum BufferSize {
            SizeThree = 3,
            SizeFour = 4,
            SizeFive = 5
        }
        ChannelFactory<IAlgorithm> factory;

        private bool CheckInput() {
            string data = textBoxData.Text.Trim();
            string[] stringInputArray = data.Split(' ');
            string bitM = textBoxBitM.Text.Trim();
            string[] stringBitM = bitM.Split(' ');
            int num;

            foreach (string i in stringInputArray) {
                if (!int.TryParse(i, out num)) {
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show("The data must be numbers!", "Error!", buttons, MessageBoxIcon.Error);
                    return false;
                }

                if ((int.Parse(i) / 10 > 0) || int.Parse(i) < 0) {
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show("The numbers must be from 0 to 9!", "Error!", buttons, MessageBoxIcon.Error);
                    return false;
                }
            }

            foreach (string i in stringBitM) {
                if ((!int.TryParse(i, out num) || (int.Parse(i) != 0) && (int.Parse(i) != 1))) {
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show("Bit M must contain only 0 and 1!", "Error!", buttons, MessageBoxIcon.Error);
                    return false;
                }

                //if ((int.Parse(i) != 0) && (int.Parse(i) != 1)) {
                //    MessageBoxButtons buttons = MessageBoxButtons.OK;
                //    MessageBox.Show("Bit M must contain only 0 and 1!", "Error!", buttons, MessageBoxIcon.Error);
                //    return false;
                //}
            }

            if (stringInputArray.Length != stringBitM.Length) {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("Data and bit M must contain the same number of elements!", "Error!", buttons, MessageBoxIcon.Error);
                return false;
            }

            string alg = comboBoxAlg.Text;
            if (alg == "") {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("Please, choose an algorithm!", "Warning!", buttons, MessageBoxIcon.Error);
                return false;
            }

            if (!int.TryParse(comboBoxBuffer.Text, out num)) {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("Please, choose the block size!", "Warning!", buttons, MessageBoxIcon.Error);
                return false;
            }
            int buffer = int.Parse(comboBoxBuffer.Text);

            if (buffer > stringInputArray.Length) {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("The sequence of numbers should NOT be less than the block size!", "Error!", buttons, MessageBoxIcon.Error);
                return false;
            }
            return true;

        }

        private List<int> ConvertData() {
            string data = textBoxData.Text.Trim();
            string[] stringInputArray = data.Split(' ');
            List<int> intInputList = new List<int>();

            foreach (string i in stringInputArray) {
                intInputList.Add(int.Parse(i));
            }
            return intInputList;
        }

        private List<bool> ConvertBitM() {
            string bitM = textBoxBitM.Text.Trim();
            string[] stringInputArray = bitM.Split(' ');
            List<bool> boolListBitM = new List<bool>();

            foreach (string i in stringInputArray) {
                boolListBitM.Add(Convert.ToBoolean(int.Parse(i)));
            }
            return boolListBitM;
        }

        private List<bool> MakeBoolListForLRU(List<int> intInputList) {
            List<bool> bitMForLRU = new List<bool>();
            for (int i = 0; i < intInputList.Count; i++) {
                bitMForLRU.Add(false);
            }
            return bitMForLRU;
        }

        private void Block() {
            buttonConnect.ForeColor = Color.Red;
            buttonConnect.Text = "Connect to the server";
            comboBoxAlg.Enabled = false;
            comboBoxBuffer.Enabled = false;
            numericUpDownFilledCells.Enabled = false;
            textBoxData.Enabled = false;
            button1.Enabled = false;
            textBoxResult.Enabled = false;
            textBoxBitM.Enabled = false;
            saveToFileToolStripMenuItem.Enabled = false;
            readFromFileToolStripMenuItem.Enabled = false;
            saveResultToFileToolStripMenuItem.Enabled = false;
        }

        private void Unblock() {
            buttonConnect.ForeColor = Color.Black;
            buttonConnect.Text = "Connected";
            comboBoxAlg.Enabled = true;
            comboBoxBuffer.Enabled = true;
            numericUpDownFilledCells.Enabled = true;
            textBoxData.Enabled = true;
            button1.Enabled = true;
            textBoxResult.Enabled = true;
            textBoxBitM.Enabled = true;
            saveToFileToolStripMenuItem.Enabled = true;
            readFromFileToolStripMenuItem.Enabled = true;
        }

        private void Button1_Click(object sender, EventArgs e) {
            if (!CheckInput()) return;
            string alg = comboBoxAlg.Text;
            int buffer = int.Parse(comboBoxBuffer.Text);
            int numOfFilled = (int)numericUpDownFilledCells.Value;
            List<int> intInputList = ConvertData();
            var service = factory.CreateChannel();
            List<bool> modifiedBit;
            int result;
            List<string> steps;
            if (alg == "NRU") {
                modifiedBit = ConvertBitM();
            } else {
                modifiedBit = MakeBoolListForLRU(intInputList);
            }
            try {
                result = service.GetInterrupts(intInputList, modifiedBit, buffer, numOfFilled);
                steps = service.GetSteps();
            } catch (EndpointNotFoundException) {
                Block();
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("The server is not connected. Check the server operation!", "Error!", buttons, MessageBoxIcon.Error);
                return;
            }
            textBoxResult.Text = result.ToString();
            List<bool> modifiedBitLRU = MakeBoolListForLRU(intInputList);
            List<bool> modifiedBitNRU = ConvertBitM();
            FillTable(steps, buffer);
            DrawChart(intInputList, modifiedBitLRU, modifiedBitNRU, numOfFilled);
            saveResultToFileToolStripMenuItem.Enabled = true;
        }

        private void FillTable(List<string> steps, int buffer) {
            dataGridView1.DataSource = null;
            DataTable dotTable = new DataTable();
            dotTable.Columns.Add("page", typeof(string));
            dotTable.Columns.Add(" ", typeof(char));
            for (int i = 1; i < buffer + 1; i++) {
                dotTable.Columns.Add(i.ToString(), typeof(string));
            }
            dotTable.Columns.Add("interrupt", typeof(char));


            switch (buffer) {
                case (int)BufferSize.SizeThree:
                for (int i = 0; i < steps.Count; i++) {
                    dotTable.Rows.Add(steps[i][0].ToString() + steps[i][1].ToString(), steps[i][2], steps[i][3].ToString() + steps[i][4].ToString(), steps[i][6].ToString() + steps[i][7].ToString(), steps[i][9].ToString() + steps[i][10].ToString(), steps[i][12]);
                }
                break;
                case (int)BufferSize.SizeFour:
                for (int i = 0; i < steps.Count; i++) {
                    dotTable.Rows.Add(steps[i][0].ToString() + steps[i][1].ToString(), steps[i][2], steps[i][3].ToString() + steps[i][4].ToString(), steps[i][6].ToString() + steps[i][7].ToString(), steps[i][9].ToString() + steps[i][10].ToString(), steps[i][12].ToString() + steps[i][13].ToString(), steps[i][15]);
                }
                break;
                case (int)BufferSize.SizeFive:
                for (int i = 0; i < steps.Count; i++) {
                    dotTable.Rows.Add(steps[i][0].ToString() + steps[i][1].ToString(), steps[i][2], steps[i][3].ToString() + steps[i][4].ToString(), steps[i][6].ToString() + steps[i][7].ToString(), steps[i][9].ToString() + steps[i][10].ToString(), steps[i][12].ToString() + steps[i][13].ToString(), steps[i][15].ToString() + steps[i][16].ToString(), steps[i][18]);
                }
                break;
            }
            dataGridView1.DataSource = dotTable;
        }

        private void DrawChart(List<int> input, List<bool> modifiedBitLRU, List<bool> modifiedBitNRU, int numOfFilled) {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            try {
                var service = factory.CreateChannel();
                for (int i = (int)BufferSize.SizeThree; i < (int)BufferSize.SizeFive + 1; i++) {
                    int result = service.GetInterrupts(input, modifiedBitLRU, i, numOfFilled);
                    chart1.Series[0].Points.AddXY(i, result);
                }
                for (int i = (int)BufferSize.SizeThree; i < (int)BufferSize.SizeFive + 1; i++) {
                    int result = service.GetInterrupts(input, modifiedBitNRU, i, numOfFilled);
                    chart1.Series[1].Points.AddXY(i, result);
                }
            } catch (EndpointNotFoundException) {
                Block();
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("The server is not connected. Check the server operation!", "Error!", buttons, MessageBoxIcon.Error);
                return;
            }
            //int result = service.GetInterrupts(input, modifiedBit, buffer, numOfFilled);
            //chart1.Series[0].Points.Clear();
            //chart1.Series[1].Points.Clear();
            //LRU lruForThree = new LRU((int)BufferSize.SizeThree);
            //int resForThree = lruForThree.LRUAlgorithm(input, (int)BufferSize.SizeThree, numOfFilled);
            //chart1.Series[0].Points.AddXY((int)BufferSize.SizeThree, resForThree);

            //LRU lruForFour = new LRU((int)BufferSize.SizeFour);
            //int resForFour = lruForFour.LRUAlgorithm(input, (int)BufferSize.SizeFour, numOfFilled);
            //chart1.Series[0].Points.AddXY((int)BufferSize.SizeFour, resForFour);

            //LRU lruForFive = new LRU((int)BufferSize.SizeFive);
            //int resForFive = lruForFive.LRUAlgorithm(input, (int)BufferSize.SizeFive, numOfFilled);
            //chart1.Series[0].Points.AddXY((int)BufferSize.SizeFive, resForFive);

        }

        private void ReadDataFromFile(StreamReader streamReader) {
            string readData;
            string readBitM;
            try {
                readData = streamReader.ReadLine();
                readBitM = streamReader.ReadLine();
            } catch (FormatException) {
                MessageBox.Show("File has incorrect data!", "Warning!");
                return;
            }
            string data = readData.Trim();
            string[] stringInputArray = data.Split(' ');
            string bitM = readBitM.Trim();
            string[] stringBitM = bitM.Split(' ');
            int num;

            foreach (string i in stringInputArray) {
                if (!int.TryParse(i, out num) || (int.Parse(i) / 10 > 0)) {
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show("File has incorrect data!", "Error!", buttons, MessageBoxIcon.Error);
                    return;
                }
            }

            foreach (string i in stringBitM) {
                if (!int.TryParse(i, out num) || (int.Parse(i) != 0) && (int.Parse(i) != 1)) {
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show("File has incorrect data!", "Error!", buttons, MessageBoxIcon.Error);
                    return;
                }
            }
            textBoxData.Text = readData;
            textBoxBitM.Text = readBitM;
        }

        private void ReadFromFileToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //{
            //    InitialDirectory = @"C:\Users\lyolya\source\repos\coursework"
            //};
            openFileDialog.Filter = "txt files (*.txt)|*.txt";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != DialogResult.OK) {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("File was not read!", "Error!", buttons, MessageBoxIcon.Error);
                return;
            }
            using (StreamReader streamReader = new StreamReader(openFileDialog.FileName)) {
                ReadDataFromFile(streamReader);
            }
        }

        private void SaveToFileToolStripMenuItem_Click(object sender, EventArgs e) {
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            if (textBoxData.Text == "") {
                MessageBox.Show("Data is empty!", "Error!", buttons, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            //{
            //    InitialDirectory = @"C:\Users\lyolya\source\repos\coursework"
            //};
            saveFileDialog.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                using (var sr = new StreamWriter(saveFileDialog.FileName)) {
                    sr.WriteLine(textBoxData.Text);
                    sr.WriteLine(textBoxBitM.Text);

                }
                MessageBox.Show("File was successfully saved!", "Success!", buttons, MessageBoxIcon.Information);
            } else {
                MessageBox.Show("File was not saved!", "Error!", buttons, MessageBoxIcon.Error);
            }
        }

        private void ButtonConnect_Click(object sender, EventArgs e) {
            try {
                Uri tcpUri = new Uri($"net.tcp://{ConfigurationManager.AppSettings["serviceAddress"]}/{ConfigurationManager.AppSettings["serviceName"]}");
            //Uri tcpUri = new Uri($"net.tcp://{serviceAddress}/{serviceName}");

                EndpointAddress address = new EndpointAddress(tcpUri);
                NetTcpBinding clientBinding = new NetTcpBinding();
                //ChannelFactory<IAlgorithm> factory = new ChannelFactory<IAlgorithm>(clientBinding, address);
                factory = new ChannelFactory<IAlgorithm>(clientBinding, address);

                var service = factory.CreateChannel();
                List<int> testInput = new List<int>() { 3, 4, 1, 5, 8 };
                List<bool> testBitM = new List<bool>() { true, true, false, false, true };
                int v = service.GetInterrupts(testInput, testBitM, 3, 2);
            } catch (Exception) {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("The server is not connected. Check the server operation!", "Error!", buttons, MessageBoxIcon.Error);
                return;
            }
            Unblock();


            //var service = factory.CreateChannel();
            //List<int> x = new List<int>() { 3, 4, 1, 5, 8, 2, 5, 9 };
            //List<bool> n = new List<bool>() { true, true, false, false, true, true, false, false };
            //int v = service.GetInterrupts(x, n, 4, 2);
            //textBox1.Text = v.ToString();

        }

        //private void ComboBoxAlg_SelectedIndexChanged(object sender, EventArgs e) {
        //    if (comboBoxAlg.Text == "NRU") {
        //        textBoxBitM.Enabled = true;
        //        label6.Enabled = true;
        //    } else {
        //        textBoxBitM.Enabled = false;
        //        label6.Enabled = false;
        //    }
        //}

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e) {
            About about = new About();
            about.ShowDialog();

        }

        private void ComboBoxAlg_SelectedIndexChanged(object sender, EventArgs e) {
            if (comboBoxAlg.Text == "NRU") {
                label6.Enabled = true;
            } else {
                label6.Enabled = false;
            }
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e) {
            Settings settings = new Settings();
            settings.ShowDialog();
            Block();
        }

        private void SaveResultToFileToolStripMenuItem_Click(object sender, EventArgs e) {
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            //{
            //    InitialDirectory = @"C:\Users\lyolya\source\repos\coursework"
            //};
            saveFileDialog.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                using (var sr = new StreamWriter(saveFileDialog.FileName)) {
                    sr.WriteLine(textBoxResult.Text);

                }
                MessageBox.Show("File was successfully saved!", "Success!", buttons, MessageBoxIcon.Information);
            } else {
                MessageBox.Show("File was not saved!", "Error!", buttons, MessageBoxIcon.Error);
            }
        }
    }
}
