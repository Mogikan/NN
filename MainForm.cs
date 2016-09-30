using nn.classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace nn
{
    public partial class MainForm : Form
    {
        private TrainingSet _trainingSet;
        private TrainingSet _testSet;
        private NeuralNetwork _network;
        private int _indexToTest;
        private static Random _randomGenerator  = new Random();

        private int _step = 1;
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
             
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = @"Text files (*.txt)|*.txt" })
            {
                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _trainingSet = new TrainingSet(openFileDialog.FileName, _step);

                    widthLabel.Text = string.Format("Width = {0}", _trainingSet.SampleWidth);
                    heightLabel.Text = string.Format("Height = {0}", _trainingSet.SampleHeight);
                    sizeLabel.Text = string.Format("Size = {0}", _trainingSet.SampleSize);

                    classesLabel.Text = string.Format("Classes count = {0}", _trainingSet.ClassesCount);
                    countLabel.Text = string.Format("Samples count = {0}", _trainingSet.SamplesCount);

                    richTextBox1.Text = string.Format("Path: {0}", openFileDialog.FileName);

                    
                    //DrawBitmaps();
                    textBox1.Text = String.Empty;
                    groupBox2.Enabled = true;
                    groupBox3.Enabled = false;
                    groupBox4.Enabled = false;
                    button5.Enabled = true;
                    button4.Enabled = vectorNumber.Enabled = false;
                }
            }
            
        }

       

        private void button1_Click(object sender, EventArgs e)
        {
            if(_trainingSet != null)
            {
                string layers = textBox1.Text;
                string[] buffer = layers.Split(new Char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                int[] networkParams = new int[buffer.Length + 1];
                for (int i = 0; i < buffer.Length; i++)
                {
                    int.TryParse(buffer[i], out networkParams[i]);
                }
                networkParams[buffer.Length] = _trainingSet.ClassesCount;
                _network = new NeuralNetwork(_trainingSet.SampleSize, networkParams);
                
                groupBox3.Enabled = true;
                groupBox4.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int iterationsCount;
            double e0, delta;
            int.TryParse(textBox2.Text, out iterationsCount);
            double.TryParse(textBox3.Text, NumberStyles.Any,CultureInfo.InvariantCulture,out delta);
            double.TryParse(textBox4.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out e0);
            chart1.Series[0].Points.Clear();
            _network.Delta = delta;
            _network.Train(_trainingSet.Samples,
                _trainingSet.Answers, 
                iterationsCount, 
                e0, 
                _trainingSet.SamplesCount, 
                chart1, 
                5);
            groupBox4.Enabled = true;
            //forSisII1.Enabled = button4.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = @"Text files (*.txt)|*.txt" })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _testSet = new TrainingSet(openFileDialog.FileName, _step);
                    if (_testSet.SampleHeight != _trainingSet.SampleHeight
                        || _testSet.SampleWidth != _trainingSet.SampleWidth)
                    {
                        MessageBox.Show("Test samples have wrong size", "Invalid test set", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (_testSet.ClassesCount != _trainingSet.ClassesCount)
                    {
                        MessageBox.Show("Classes count doesn't match", "Invalid test set", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    total.Text = _testSet.SamplesCount.ToString();
                    vectorNumber.Enabled = true;
                    button4.Enabled = true;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _indexToTest = int.Parse(vectorNumber.Text);// _randomGenerator.Next(_testSet.SamplesCount);
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(_testSet.SampleWidth, _testSet.SampleHeight);
            for (int x = 0; x < bmp.Height; ++x)
            {
                for (int y = 0; y < bmp.Width; ++y)
                {
                    //bmp.SetPixel(x, y, Color.White);
                    if (_trainingSet.IsBinary)
                    {
                        int val = (int)_testSet.Samples[_indexToTest][x * _trainingSet.SampleWidth + y];
                        bmp.SetPixel(y, x, val > 0 ? Color.Black : Color.White);
                    }
                    else
                    {
                        int val = (int)_testSet.Samples[_indexToTest][x * _trainingSet.SampleWidth + y];
                        Color color = Color.FromArgb(val, val, val);
                        bmp.SetPixel(y, x, Color.FromArgb(val, val, val));
                    }
                }
            }
            pictureBox1.Image = bmp;
            button4.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button5_Click(sender, e);
            double[] netIn = _testSet.Samples[_indexToTest];
            double[] result = new double[_testSet.ClassesCount];
            _network.NetworkOut(netIn, out result);
            listBox1.Items.Clear();
            for (int i = 0; i < _testSet.ClassesCount; i++)
            {
                listBox1.Items.Add(String.Format("{0}:  {1}", i + 1, result[i]));
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            using (var PreviewForm = new PreviewForm(_trainingSet))
            {
                PreviewForm.ShowDialog();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            using (var PreviewForm = new PreviewForm(_testSet))
            {
                PreviewForm.ShowDialog();
            }
        }
    }
}
