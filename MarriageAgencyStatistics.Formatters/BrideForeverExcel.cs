using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Core.DataProviders;
using OfficeOpenXml;

namespace MarriageAgencyStatistics.Formatters
{
    public class BrideForeverExcel
    {
        public void UpdateUserBonuses(IEnumerable<(User, Bonus)> userBonuses)
        {
            using (var excelPackage = new ExcelPackage())
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add("Bonuses");

                int i = 1;
                foreach (var userBonus in userBonuses)
                {
                    i++;
                    worksheet.Cells[i, 1].Value = userBonus.Item1;
                    worksheet.Cells[i, 3].Value = userBonus.Item2.Today;
                    worksheet.Cells[i, 4].Value = userBonus.Item2.LastMonth;
                }

                excelPackage.SaveAs(new FileInfo(@"E:\BrideForever.xlsx"));
            }
        }
    }
}
