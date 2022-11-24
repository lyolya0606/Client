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
using LibraryWithAlgorithms;

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

        private bool CheckInput() {
            string data = textBox1.Text;
            data = data.Trim();
            string[] stringInputArray;
            stringInputArray = data.Split(' ');
            int num;

            foreach (string i in stringInputArray) {
                if (!int.TryParse(i, out num)) {
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show("The data must be numbers!", "Error!", buttons, MessageBoxIcon.Error);
                    return false;
                }

                if (int.Parse(i) / 10 > 0) {
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show("The numbers must be less than 10!", "Error!", buttons, MessageBoxIcon.Error);
                    return false;
                }
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
            string data = textBox1.Text;
            data = data.Trim();
            string[] stringInputArray;
            stringInputArray = data.Split(' ');
            List<int> intInputList = new List<int>();

            foreach (string i in stringInputArray) {
                intInputList.Add(int.Parse(i));
            }
            return intInputList;
        }

        private void Button1_Click(object sender, EventArgs e) {
            if (!CheckInput()) return;
            button1.Enabled = true;
            string alg = comboBoxAlg.Text;
            int buffer = int.Parse(comboBoxBuffer.Text);
            int numOfFilled = (int)numericUpDownFilledCells.Value;
            List<int> intInputList = ConvertData();
            LRU lru = new LRU(buffer);
            int result = lru.LRUAlgorithm(intInputList, buffer, numOfFilled);
            List<string> steps = lru.GetSteps();
            textBoxResult.Text = result.ToString();
            FillTable(steps, buffer);
            DrawChart(intInputList, numOfFilled);
        }

        private void FillTable(List<string> steps, int buffer) {
            dataGridView1.DataSource = null;
            DataTable dotTable = new DataTable();
            dotTable.Columns.Add("page", typeof(char));
            dotTable.Columns.Add(" ", typeof(char));
            for (int i = 1; i < buffer + 1; i++) {
                dotTable.Columns.Add(i.ToString(), typeof(char));
            }
            dotTable.Columns.Add("interrupt", typeof(char));

            switch (buffer) {
                case (int)BufferSize.SizeThree:
                    for (int i = 0; i < steps.Count; i++) {
                        dotTable.Rows.Add(steps[i][0], steps[i][1], steps[i][2], steps[i][4], steps[i][6], steps[i][8]);
                    }
                    break;
                case (int)BufferSize.SizeFour:
                    for (int i = 0; i < steps.Count; i++) {
                        dotTable.Rows.Add(steps[i][0], steps[i][1], steps[i][2], steps[i][4], steps[i][6], steps[i][8], steps[i][10]);
                    }
                    break;
                case (int)BufferSize.SizeFive:
                    for (int i = 0; i < steps.Count; i++) {
                        dotTable.Rows.Add(steps[i][0], steps[i][1], steps[i][2], steps[i][4], steps[i][6], steps[i][8], steps[i][10], steps[i][12]);
                    }
                    break;
            }
            dataGridView1.DataSource = dotTable;
        }

        private void DrawChart(List<int> input, int numOfFilled) {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            LRU lruForThree = new LRU((int)BufferSize.SizeThree);
            int resForThree = lruForThree.LRUAlgorithm(input, (int)BufferSize.SizeThree, numOfFilled);
            chart1.Series[0].Points.AddXY((int)BufferSize.SizeThree, resForThree);

            LRU lruForFour = new LRU((int)BufferSize.SizeFour);
            int resForFour = lruForFour.LRUAlgorithm(input, (int)BufferSize.SizeFour, numOfFilled);
            chart1.Series[0].Points.AddXY((int)BufferSize.SizeFour, resForFour);

            LRU lruForFive = new LRU((int)BufferSize.SizeFive);
            int resForFive = lruForFive.LRUAlgorithm(input, (int)BufferSize.SizeFive, numOfFilled);
            chart1.Series[0].Points.AddXY((int)BufferSize.SizeFive, resForFive);

        }

        private void ReadDataFromFile(StreamReader streamReader) {
            string readData;
            try {
                readData = streamReader.ReadLine();
            } catch (FormatException) {
                MessageBox.Show("File has incorrect data!", "Warning!");
                return;
            }
            readData = readData.Trim();
            string[] stringInputArray;
            stringInputArray = readData.Split(' ');
            int num;

            foreach (string i in stringInputArray) {
                if (!int.TryParse(i, out num) || (int.Parse(i) / 10 > 0)) {
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show("File has incorrect data!", "Error!", buttons, MessageBoxIcon.Error);
                    return;
                }
            }
            textBox1.Text = readData;
        }

        private void ReadFromFileToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog() {
                InitialDirectory = @"C:\Users\lyolya\source\repos\coursework"
            };
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
            if (textBox1.Text == "") {
                MessageBox.Show("Data is empty!", "Error!", buttons, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog() {
                InitialDirectory = @"C:\Users\lyolya\source\repos\coursework"
            };
            saveFileDialog.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                using (var sr = new StreamWriter(saveFileDialog.FileName)) {
                    sr.WriteLine(textBox1.Text);
                }
                MessageBox.Show("File was successfully saved!", "Success!", buttons, MessageBoxIcon.Information);
            } else {
                MessageBox.Show("File was not saved!", "Error!", buttons, MessageBoxIcon.Error);
            }
        }
    }
}
