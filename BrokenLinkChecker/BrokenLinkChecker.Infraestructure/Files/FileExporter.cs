using BrokenLinkChecker.Domain;
using CsvHelper;
using Microsoft.VisualBasic;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenLinkChecker.Infraestructure.Files
{
    public class FileExporter
    {
        public string Destination { get; set; }

        public FileExporter(string Destination)
        {
            this.Destination = Destination;
        }

        /// <summary>
        /// Generate a CSV file using the library https://joshclose.github.io/CsvHelper/
        /// </summary>
        /// <typeparam name="T">Collection of data we want to export</typeparam>
        /// <param name="Contacts">Collection of data we want to export</param>
        public void ExpExportFileCSV<T>(List<T> Contacts)
        {
            using (var writer = new StreamWriter(Destination))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(Contacts);
            }
        }
        /// <summary>
        /// Generate an excel file using the library https://github.com/nissl-lab/npoi
        /// </summary>
        /// <param name="Contacts">Collection of data we want to export</param>
        /// <param name="Fields">List of columns that I want to export</param>
        public void ExpExportFileExcel(List<PageLink> Contacts, List<string> Fields)
        {
            ICell cell;
            IRow row;
            int rowNum = 0;
            int cellNum = 0;
            FileStream stream;
            IWorkbook wb = new XSSFWorkbook();


            using (stream = new FileStream(Destination, FileMode.Create, FileAccess.Write))
            {
                ISheet sheet = wb.CreateSheet("BrokenLinkChecker");
                ICreationHelper cH = wb.GetCreationHelper();
                row = sheet.CreateRow(0);

                foreach (string c in Fields)
                {
                    cell = row.CreateCell(cellNum++);
                    cell.SetCellValue(c);
                }

                foreach (var item in Contacts)
                {
                    cellNum = 0;
                    row = sheet.CreateRow(++rowNum);
                    cell = row.CreateCell(cellNum++, CellType.String);
                    cell.SetCellValue(item.LinkUrl);

                    cell = row.CreateCell(cellNum++, CellType.String);
                    cell.SetCellValue(item.LinkText);

                    cell = row.CreateCell(cellNum++, CellType.String);
                    cell.SetCellValue(item.PageName);
                                      
                }
                wb.Write(stream, false);
            }
        }
    }
}
