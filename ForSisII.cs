using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace nn
{
    public partial class ForSisII : UserControl
    {
        public ForSisII()
        {
            InitializeComponent();
            values = new double[36];
            for (int i = 0; i < 36; i++)
                values[i] = 1;
        }
        private double[] values;
        /// <summary>
        /// инвертирует цвет квадратика по клику
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InvertColor(object sender, EventArgs e)
        {
            Panel panel = ((Panel)sender);
            if (panel.BackColor == Color.White)
                panel.BackColor = Color.Black;
            else
                panel.BackColor = Color.White;
            String numString = (panel.Name.Substring(5));
            int index = int.Parse(numString) - 2;
            values[index] = panel.BackColor == Color.Black ? 0 : 1;
        }
        /// <summary>
        /// получает массив со значениями, белый закодирован единицей
        /// </summary>
        /// <returns></returns>
        public double[] GetValues()
        {
            return values;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (object p in panel1.Controls)
            {
                if(p!=button1)
                    ((Panel)p).BackColor = Color.White;
            }
            for (int i = 0; i < 36; i++)
            {                
                values[i] = 1;
            }
        }
    }
}
