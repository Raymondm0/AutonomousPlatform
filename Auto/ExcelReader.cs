using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using WinFormsApp_Draft;


namespace WinFormsApp_Draft.Auto
{
    class ExcelReader
    {
        public string GetCellValue(int i, int j)
        {
            try
            {
                Row row = Form1.mWorksheet.Descendants<Row>().ElementAt(i);
                Cell cell = row.Descendants<Cell>().ElementAt(j);
                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                {
                    if (Form1.mWorkbookPart.SharedStringTablePart != null)
                    {
                        int index = int.Parse(cell.InnerText);
                        return Form1.mSharedStringTable.ElementAt(index).InnerText;
                    }
                }
                return cell.InnerText;
            }
            catch
            {
                return null;
            }
        }

        public void init_Properties()
        {
            try
            {
                int rounds = Form1.mWorksheet.Descendants<Row>().Count() - 1;
                int param_num = Form1.mWorksheet.Descendants<Row>().ElementAt(0).Count();
                Form1.properties = new Form1.sheet_properties { parameters = param_num, rounds = rounds };
            }
            catch { }
        }
               
        public List<string> GetRowData(int round_num) 
        {
            List<string> round_list = new List<string>();
            Row row = Form1.mWorksheet.Descendants<Row>().ElementAt(round_num);
            foreach (Cell cell in row.Elements<Cell>())
            {
                round_list.Add(cell.InnerText);
            }
            return round_list;
        }

        public void init_ParamNames()
        {
            Form1.param_names = new List<string>();
            Row row = Form1.mWorksheet.Descendants<Row>().ElementAt(0);
            foreach (Cell cell in row.Elements<Cell>())
            {
                int index = int.Parse(cell.InnerText);
                Form1.param_names.Add(Form1.mSharedStringTable.ElementAt(index).InnerText);
            }
        }

        public void printData(RichTextBox richTextBox)
        {
            try
            {
                if (Form1.param_list != null)
                {
                    List<List<string>> para = Form1.param_list;
                    for (int i = 0; i < para.Count; i++)
                    {
                        for (int j = 0; j < para[0].Count; j++)
                        {
                            richTextBox.Text += para[i][j] + " ";
                        }
                        richTextBox.Text += "\n";
                    }
                }
            }
            catch (Exception e) 
            {
                richTextBox.Text = e.Message;
            }
        }
    }
}
