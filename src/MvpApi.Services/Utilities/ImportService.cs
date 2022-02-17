using MvpApi.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.TextBased.Csv;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace MvpApi.Services.Utilities
{
    public class ImportService
    {
        private int? _indexColumnStartDate;
        private int? _indexColumnTitle;
        private int? _indexColumnDescription;
        private int? _indexColumnReferenceUrl;
        private int? _indexColumnContributionTypeName;
        private int? _indexColumnContributionType;
        private int? _indexColumnContributionTechnology;
        private int? _indexColumnAnnualQuantity;
        private int? _indexColumnSecondAnnualQuantity;
        private int? _indexColumnAnnualReach;
        private int? _indexColumnVisibility;

        private const int TitleRowIndex = 0;
        private const int DataRowIndexStart = 1;

        public ImportService() { }

        public ContributionViewModel ImportContributionsFile(
            string filePath, 
            IList<ContributionTypeModel> contributionTypes,
            IList<ContributionTechnologyModel> contributionTechnologies,
            IList<VisibilityViewModel> visibilities)
        {
            if (!File.Exists(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (contributionTypes == null) 
                throw new ArgumentNullException(nameof(contributionTypes));

            if (contributionTechnologies == null) 
                throw new ArgumentNullException(nameof(contributionTechnologies));

            if (visibilities == null) 
                throw new ArgumentNullException(nameof(visibilities));

            if (!contributionTypes.Any())
                throw new ArgumentOutOfRangeException(nameof(contributionTypes), "You must have items in this list in order to import data.");
            
            if (!contributionTechnologies.Any())
                throw new ArgumentOutOfRangeException(nameof(contributionTechnologies), "You must have items in this list in order to import data.");

            if (!visibilities.Any())
                throw new ArgumentOutOfRangeException(nameof(contributionTechnologies), "You must have items in this list in order to import data.");

            Workbook workbook = null;

            var fileExtension = Path.GetExtension(filePath);

            if (fileExtension.Contains("xlsx"))
            {
                using (Stream input = new FileStream(filePath, FileMode.Open))
                {
                    workbook = new XlsxFormatProvider().Import(input);
                }
            }
            else if (fileExtension.Contains("csv"))
            {
                using (Stream input = new FileStream(filePath, FileMode.Open))
                {
                    workbook = new CsvFormatProvider().Import(input);
                }
            }
            else
            {
                throw new NotSupportedException("Only CSV and XLSX files are supported at this time.");
            }
            
            var worksheet = workbook?.ActiveWorksheet;

            if (worksheet == null)
                return null;
            
            // Title Row Parsing - iterate over the 1st row for column titles and try to identify what each column is
            IdentifyColumnIndices(worksheet);

            // Data Rows Parsing - iterate over the data rows and try to read the values
            var contributions = GetRowDataValues(worksheet, contributionTypes, contributionTechnologies, visibilities);
            
            // Return the same result that an actual API call would

            return new ContributionViewModel(contributions, contributions.Count, contributions.Count);
            
        }

        private void IdentifyColumnIndices(Worksheet worksheet)
        {
            for (int i = 0; i < worksheet.Columns.Count - 1; i++)
            {
                var cell = worksheet.Cells[TitleRowIndex, i].GetValue();

                if (cell.Value.RawValue.ToLower().Contains("date"))
                {
                    _indexColumnStartDate = i;

                    Trace.WriteLine($"Found the StartDate column at index {i}.", "CSV Import");
                }

                if (cell.Value.RawValue.ToLower().Contains("title"))
                {
                    _indexColumnTitle = i;

                    Trace.WriteLine($"Found the Title column at index {i}.", "CSV Import");
                }

                if (cell.Value.RawValue.ToLower().Contains("description"))
                {
                    _indexColumnDescription = i;

                    Trace.WriteLine($"Found the Description column at index {i}.", "CSV Import");
                }

                if (cell.Value.RawValue.ToLower().Contains("url") ||
                    cell.Value.RawValue.ToLower().Contains("link"))
                {
                    _indexColumnReferenceUrl = i;

                    Trace.WriteLine($"Found the ReferenceUrl column at index {i}.", "CSV Import");
                }

                if (cell.Value.RawValue.ToLower().Contains("visibility"))
                {
                    _indexColumnVisibility = i;

                    Trace.WriteLine($"Found the Visibility column at index {i}.", "CSV Import");
                }

                if (cell.Value.RawValue.ToLower() == "contribution type")
                {
                    _indexColumnContributionType = i;

                    Trace.WriteLine($"Found the ContributionType column at index {i}.", "CSV Import");
                }

                if (cell.Value.RawValue.ToLower() == "contribution type name")
                {
                    _indexColumnContributionTypeName = i;

                    Trace.WriteLine($"Found the ContributionTypeName column at index {i}.", "CSV Import");
                }

                if (cell.Value.RawValue.ToLower().Contains("technology") ||
                    cell.Value.RawValue.ToLower().Contains("tech"))
                {
                    _indexColumnContributionTechnology = i;

                    Trace.WriteLine($"Found the ContributionTechnology column at index {i}.", "CSV Import");
                }

                if (cell.Value.RawValue.ToLower() == "annual quantity")
                {
                    _indexColumnAnnualQuantity = i;

                    Trace.WriteLine($"Found the AnnualQuantity column at index {i}.", "CSV Import");
                }

                if (cell.Value.RawValue.ToLower() == "second annual quantity")
                {
                    _indexColumnSecondAnnualQuantity = i;

                    Trace.WriteLine($"Found the SecondAnnualQuantity column at index {i}.", "CSV Import");
                }

                if (cell.Value.RawValue.ToLower() == "annual reach")
                {
                    _indexColumnAnnualReach = i;

                    Trace.WriteLine($"Found the AnnualReach column at index {i}.", "CSV Import");
                }
            }
        }

        private List<ContributionsModel> GetRowDataValues(
            Worksheet worksheet, 
            IList<ContributionTypeModel> contributionTypes,
            IList<ContributionTechnologyModel> contributionTechnologies,
            IList<VisibilityViewModel> visibilities)
        {
            var contributions = new List<ContributionsModel>();
            

            for (var i = DataRowIndexStart; i < worksheet.Rows.Count - 1; i++)
            {
                var activity = new ContributionsModel();

                // ******************* DateTime values ******************* //

                if (_indexColumnStartDate != null)
                {
                    var cell = worksheet.Cells[i, _indexColumnStartDate.Value].GetValue();

                    if (cell.Value.RawValue == null)
                        continue;

                    activity.StartDate = Convert.ToDateTime(cell.Value.RawValue);
                }

                // ******************* string values ******************* //

                if (_indexColumnTitle != null)
                {
                    var cell = worksheet.Cells[i, _indexColumnTitle.Value].GetValue();

                    if (cell.Value.RawValue == null)
                        continue;

                    activity.Title = cell.Value.RawValue;
                }

                if (_indexColumnDescription != null)
                {
                    var cell = worksheet.Cells[i, _indexColumnDescription.Value].GetValue();

                    if (cell.Value.RawValue == null)
                        continue;

                    activity.Description = cell.Value.RawValue;
                }

                if (_indexColumnReferenceUrl != null)
                {
                    var cell = worksheet.Cells[i, _indexColumnReferenceUrl.Value].GetValue();

                    if (cell.Value.RawValue == null)
                        continue;

                    activity.ReferenceUrl = cell.Value.RawValue;
                }

                if (_indexColumnContributionTypeName != null)
                {
                    var cell = worksheet.Cells[i, _indexColumnContributionTypeName.Value].GetValue();

                    if (cell.Value.RawValue == null)
                        continue;

                    activity.ContributionTypeName = cell.Value.RawValue;

                    // Bonus: See if we can also set the contribution type now that we have a valid name
                    if (activity.ContributionType == null)
                    {
                        var matchingType = contributionTypes.FirstOrDefault(ct => string.Equals(ct.Name, cell.Value.RawValue, StringComparison.CurrentCultureIgnoreCase));

                        if (matchingType != null)
                        {
                            activity.ContributionType = matchingType;
                        }
                    }
                }

                // ******************* numerical values ******************* //

                if (_indexColumnAnnualQuantity != null)
                {
                    var cell = worksheet.Cells[i, _indexColumnAnnualQuantity.Value].GetValue();

                    if (cell.Value.RawValue == null)
                        continue;

                    activity.AnnualQuantity = Convert.ToInt32(cell.Value.RawValue);
                }

                if (_indexColumnSecondAnnualQuantity != null)
                {
                    var cell = worksheet.Cells[i, _indexColumnSecondAnnualQuantity.Value].GetValue();

                    if (cell.Value.RawValue == null)
                        continue;

                    activity.SecondAnnualQuantity = Convert.ToInt32(cell.Value.RawValue);
                }

                if (_indexColumnAnnualReach != null)
                {
                    var cell = worksheet.Cells[i, _indexColumnAnnualReach.Value].GetValue();

                    if (cell.Value.RawValue == null)
                        continue;

                    activity.AnnualReach = Convert.ToInt32(cell.Value.RawValue);
                }

                // ******************* complex data type values ******************* //

                // If we were not able to set the contributionType earlier using the name, try to set it now
                if (_indexColumnContributionType != null && activity.ContributionType == null)
                {
                    var cell = worksheet.Cells[i, _indexColumnContributionType.Value].GetValue();

                    if (cell.Value.RawValue == null)
                        continue;

                    var matchingType = contributionTypes.FirstOrDefault(ct => string.Equals(ct.Name, cell.Value.RawValue, StringComparison.CurrentCultureIgnoreCase));

                    if (matchingType != null)
                    {
                        activity.ContributionType = matchingType;
                    }

                    // Bonus - If we were not able to set ContributionTypeName earlier, set it now
                    if (string.IsNullOrEmpty(activity.ContributionTypeName))
                    {
                        activity.ContributionTypeName = matchingType.Name;
                    }
                }

                if (_indexColumnContributionTechnology != null)
                {
                    var cell = worksheet.Cells[i, _indexColumnContributionTechnology.Value].GetValue();

                    if (cell.Value.RawValue == null)
                        continue;

                    var matchingTech = contributionTechnologies.FirstOrDefault(ct => string.Equals(ct.Name, cell.Value.RawValue, StringComparison.CurrentCultureIgnoreCase));

                    if (matchingTech != null)
                    {
                        activity.ContributionTechnology = matchingTech;
                    }
                }

                if (_indexColumnVisibility != null)
                {
                    var cell = worksheet.Cells[i, _indexColumnVisibility.Value].GetValue();

                    if (cell.Value.RawValue == null)
                        continue;

                    var id = Convert.ToInt32(cell.Value.RawValue);

                    var matchingVisibility = visibilities.FirstOrDefault(ct => ct.Id.Equals(id));

                    if (matchingVisibility != null)
                    {
                        activity.Visibility = matchingVisibility;
                    }
                }


                contributions.Add(activity);
            }

            return contributions;
        }
    }
}
