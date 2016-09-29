using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using nn.classes;
using System.IO;

namespace nn
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        }
        private double[][] sample;
        private double[][] ideals;
        private int sampleCount, inp, outp;
        private NeuralNetwork network;
        public virtual void LoadSample(String Path)
        {
            using (FileStream fileStream = new FileStream(Path, FileMode.Open))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {

                    ///*
                    String content = streamReader.ReadToEnd();
                    content = content.Replace('\n', ' ');
                    content = content.Replace('\r', ' ');
                    String[] buffer = content.Split(new Char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                    int.TryParse(buffer[0], out sampleCount);
                    sample = new double[sampleCount][];
                    ideals = new double[sampleCount][];
                    for (int i = 0; i < sampleCount; i++)
                    {
                        sample[i] = new double[36];
                        ideals[i] = new double[3];
                        String current = buffer[i + 1];
                        String[] bitBuffer = current.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int j;
                        for (j = 0; j < 36; j++)
                        {
                            double.TryParse(bitBuffer[j], out sample[i][j]);
                        }
                        int ind;
                        int.TryParse(bitBuffer[j], out ind);
                        ideals[i][ind - 1] = 1;
                    }
                    //*/
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadSample(ofd.FileName);
            }
            groupBox2.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {

            String s = textBox2.Text;
            String[] buffer = s.Split(new Char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            int[] arg = new int[buffer.Length + 1];
            arg[buffer.Length] = 3;
            for (int i = 0; i < buffer.Length; i++)
                int.TryParse(buffer[i], out arg[i]);
            network = new NeuralNetwork(36, arg);
            button1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int iterationsCount;
            double e0, delta;
            int.TryParse(textBox3.Text, out iterationsCount);
            double.TryParse(textBox4.Text, out delta);
            double.TryParse(textBox5.Text, out e0);
            network.Delta = delta;
            network.Train(sample, ideals, iterationsCount, e0, sampleCount, chart1, 20);
            forSisII1.Enabled = button4.Enabled = true;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            double[] vect = forSisII1.GetValues();
            double[] ans;
            network.NetworkOut(vect, out ans);
            pot1.Text = "= - " + ans[0].ToString();
            pot2.Text = "I - " + ans[1].ToString();
            pot3.Text = "* - " + ans[2].ToString();
        }
    }
}
