using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using OfficeOpenXml;

namespace MarriageAgencyStatistics.Formatters
{
    public static class BrideForeverExcel
    {
        public static void UpdateExcel(IEnumerable<(User, Bonus, OnlineStatistics, SentEmailStatistics)> values, DateTime date, string path)
        {
            using (var excelPackage = new ExcelPackage())
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add($"Данные {date.Date:d}");

                worksheet.Cells[1, 1].Value = "Модель";
                worksheet.Cells[1, 2].Value = "Онлайн";
                worksheet.Cells[1, 3].Value = "Письма";
                worksheet.Cells[1, 4].Value = "Бонусы";
                worksheet.Cells[1, 5].Value = "За месяц";

                int i = 1;
                foreach (var value in values)
                {
                    i++;
                    worksheet.Cells[i, 1].Value = value.Item1;
                    worksheet.Cells[i, 2].Value = value.Item3.TotalMinutesOnline <= 0 ? "-" : $"{value.Item3.TotalMinutesOnline / 60}h {value.Item3.TotalMinutesOnline % 60}m";
                    worksheet.Cells[i, 3].Value = value.Item4.SentEmails <= 0 ? "-" : $"{value.Item4.SentEmails}";
                    worksheet.Cells[i, 4].Value = value.Item2.Today;
                    worksheet.Cells[i, 5].Value = value.Item2.LastMonth;
                }

                excelPackage.SaveAs(new FileInfo(path));
            }
        }
    }
}
