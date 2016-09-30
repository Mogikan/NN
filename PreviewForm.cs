using nn.classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nn
{
    public partial class PreviewForm : Form
    {
        public PreviewForm(TrainingSet trainingSet)
        {
            InitializeComponent();
            this.Load += PreviewForm_Load;
            _trainingSet = trainingSet;
        }

        private TrainingSet _trainingSet;

        private void PreviewForm_Load(object sender, EventArgs e)
        {
            flowLayoutPanel1.BeginInvoke((Action)(() => DrawBitmaps()));
        }

        private void DrawBitmaps()
        {
            flowLayoutPanel1.Controls.Clear();
            for (int i = 0; i < _trainingSet.SamplesCount; i++)
            {
                PictureBox box = new PictureBox()
                {
                    Size = new Size()
                    { Height = _trainingSet.SampleHeight + 6, Width = _trainingSet.SampleWidth + 6 }
                };

                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(_trainingSet.SampleWidth, _trainingSet.SampleHeight);
                for (int x = 0; x < bmp.Height; ++x)
                {
                    for (int y = 0; y < bmp.Width; ++y)
                    {
                        //bmp.SetPixel(x, y, Color.White);
                        if (_trainingSet.IsBinary)
                        {
                            int val = (int)_trainingSet.Samples[i][x * _trainingSet.SampleWidth + y];
                            bmp.SetPixel(y, x, val > 0 ? Color.Black : Color.White);
                        }
                        else
                        {
                            int val = (int)_trainingSet.Samples[i][x * _trainingSet.SampleWidth + y];
                            Color color = Color.FromArgb(val, val, val);
                            bmp.SetPixel(y, x, Color.FromArgb(val, val, val));
                        }
                    }
                }

                flowLayoutPanel1.Controls.Add(box);

                box.Image = bmp;
            }
        }
    }
}
