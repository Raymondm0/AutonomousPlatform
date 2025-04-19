using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;


namespace WinFormsApp_Draft.Auto
{
    class ExcelReader
    {
        /// <summary>
        /// returns string value of cell; before iteration, define which worksheet the cell belongs to
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="workbookPart"></param>
        /// <returns></returns>
        public string GetCellValue(Cell cell, SharedStringTablePart stringTablePart)
        {
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                if (stringTablePart != null)
                {
                    int index = int.Parse(cell.InnerText);
                    return stringTablePart.SharedStringTable.ElementAt(index).InnerText;
                }
            }
            return cell.InnerText;
        }

        /// <summary>
        /// returns numeric value of cell; if data read is string, will return index in SharedStringTable
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="worksheet"></param>
        /// <returns></returns>
        public double GetCellValue(int i, int j, Worksheet worksheet)
        {
            Row row = worksheet.Descendants<Row>().ElementAt(i);
            Cell cell = row.Descendants<Cell>().ElementAt(j);
            return Convert.ToDouble(cell.InnerText);
        }
        
        /// <summary>
        /// returns list of parameter heads
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="stringTablePart"></param>
        /// <returns></returns>
        public List<string> getHeaders(Worksheet worksheet,SharedStringTablePart stringTablePart)
        {
            List<string> headers = new List<string>();
            Row row = worksheet.Descendants<Row>().First();

            foreach(Cell cell in row.Elements<Cell>())
            {
                string header;
                int index = int.Parse(cell.InnerText);
                header = stringTablePart.SharedStringTable.ElementAt(index).InnerText;
                headers.Add(header);
            }
            return headers;
        }

        
    }
}
