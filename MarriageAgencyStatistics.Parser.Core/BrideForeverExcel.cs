using System.Collections.Generic;
using System.IO;
using System.Text;
using MarriageAgencyStatistics.Domain.BrideForever;
using OfficeOpenXml;

namespace MarriageAgencyStatistics.Parser.Core
{
    public class BrideForeverExcel
    {
        static BrideForeverExcel()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

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