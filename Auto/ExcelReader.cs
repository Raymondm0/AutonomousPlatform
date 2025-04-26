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
                Row row = MainForm.mWorksheet.Descendants<Row>().ElementAt(i);
                Cell cell = row.Descendants<Cell>().ElementAt(j);
                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                {
                    if (MainForm.mWorkbookPart.SharedStringTablePart != null)
                    {
                        int index = int.Parse(cell.InnerText);
                        return MainForm.mSharedStringTable.ElementAt(index).InnerText;
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
                int rounds = MainForm.mWorksheet.Descendants<Row>().Count() - 1;
                int param_num = MainForm.mWorksheet.Descendants<Row>().ElementAt(0).Count();
                MainForm.properties = new MainForm.sheet_properties { parameters = param_num, rounds = rounds };
            }
            catch { }
        }
               
        public List<string> GetRowData(int round_num) 
        {
            List<string> round_list = new List<string>();
            Row row = MainForm.mWorksheet.Descendants<Row>().ElementAt(round_num);
            foreach (Cell cell in row.Elements<Cell>())
            {
                round_list.Add(cell.InnerText);
            }
            return round_list;
        }

        public void init_ParamNames()
        {
            MainForm.param_names = new List<string>();
            Row row = MainForm.mWorksheet.Descendants<Row>().ElementAt(0);
            foreach (Cell cell in row.Elements<Cell>())
            {
                int index = int.Parse(cell.InnerText);
                MainForm.param_names.Add(MainForm.mSharedStringTable.ElementAt(index).InnerText);
            }
        }

        public void printData(RichTextBox richTextBox)
        {
            try
            {
                if (MainForm.param_list != null)
                {
                    List<List<string>> para = MainForm.param_list;
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
