using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp_Draft.Auto
{
    public class TipLayout
    {
        public void init_btns(int row_n, int col_n, TableLayoutPanel TLP, TextBox response)
        {
            TLP.ColumnCount = col_n;
            TLP.RowCount = row_n;
            set_size(row_n, col_n, TLP);

            Button[,] buttons = new Button[row_n, col_n];

            for (int i = 0; i < row_n ; i++)
            {
                for (int j = 0; j < col_n ; j++)
                {
                    buttons[i, j] = new Button();
                    buttons[i, j].Text = $"{ i * col_n + j + 1 }";
                    buttons[i, j].Dock = DockStyle.Fill;
                    buttons[i, j].BackColor = Color.ForestGreen;

                    TLP.Controls.Add(buttons[i, j], j, i);
                    buttons[i, j].Click += (sender, e) =>
                    {
                        Button button = (Button)sender;
                        pressed(button, response);
                        
                    };
                }
            }
        }

        private void set_size(int row_n, int col_n, TableLayoutPanel TLP)
        {
            TLP.ColumnStyles.Clear();
            TLP.RowStyles.Clear();
            for (int i = 0; i < col_n; i++)
            {
                TLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / col_n));
            }
            for (int j = 0; j < row_n ; j++)
            {
                TLP.RowStyles.Add(new ColumnStyle(SizeType.Percent, 100F / row_n));
            }
        }

        private void pressed(Button button, TextBox response)
        {
            if (button.BackColor == Color.Red)
            {
                button.BackColor = Color.ForestGreen;
            }
            else
            { 
                button.BackColor = Color.Red;
            }

            if (response.InvokeRequired)
            {
                response.Invoke(new Action(() =>
                {
                    response.Text = button.Text;
                }));
            }
            else
            {
                response.Text = button.Text;
            }
        }
    }
}
