using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DemoDataBuilder.ComponentModel;
using DemoDataBuilder.Extensions;
using DemoDataBuilder.InputModel;
using GemBox.Spreadsheet;
using log4net;

namespace DemoDataBuilder.Importer
{
    public class SheetImporter : IFacilityDataImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _fileName;
        private readonly string _sheetName;

        private Dictionary<int, PropertyInfo> _propertyMap;

        public SheetImporter(string fileName, string sheetName)
        {
            _fileName = fileName;
            _sheetName = sheetName;
        }

        private void BuildPropertyMap<T>(ExcelWorksheet worksheet, int headerRow)
        {
            _propertyMap = new Dictionary<int, PropertyInfo>();

            Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

            foreach (PropertyInfo property in typeof (T).GetProperties().Where(pi => pi.CanRead && pi.CanWrite))
            {
                properties[property.Name.ToKey()] = property;

                foreach (AlternateNameAttribute attribute in Attribute.GetCustomAttributes(property, typeof(AlternateNameAttribute)))
                {
                    string alternateName = attribute.AlternateName.ToKey();

                    if (!string.IsNullOrWhiteSpace(alternateName) && !properties.ContainsKey(alternateName))
                        properties.Add(alternateName, property);
                }
            }

            CellRange range = worksheet.GetUsedCellRange(true);

            for (int col = range.FirstColumnIndex; col <= range.LastColumnIndex; col++)
            {
                string fieldName = Convert.ToString(worksheet.Cells[headerRow, col].Value).ToKey();

                if (!string.IsNullOrWhiteSpace(fieldName) && properties.ContainsKey(fieldName))
                {
                    _propertyMap.Add(col, properties[fieldName]);

                    Log.DebugFormat("Header '{0}' in {1} was mapped to {2}",
                                    worksheet.Cells[headerRow, col].Value,
                                    CellRange.RowColumnToPosition(headerRow, col),
                                    properties[fieldName].Name);
                }
                else
                {
                    Log.DebugFormat("Header '{0}' in {1} was not mapped",
                                    worksheet.Cells[headerRow, col].Value,
                                    CellRange.RowColumnToPosition(headerRow, col));
                }
            }
        }

        private IEnumerable<T> GetData<T>() where T : new()
        {
            Log.InfoFormat("Importing Sheet '{0}' of file {1}...", _sheetName, _fileName);

            List<T> result = new List<T>();

            ExcelFile excelFile = new ExcelFile();

            excelFile.LoadXlsx(_fileName, XlsxOptions.None);

            ExcelWorksheet worksheet = excelFile.Worksheets[_sheetName];

            CellRange range = worksheet.GetUsedCellRange(true);

            BuildPropertyMap<T>(worksheet, range.FirstRowIndex);

            for (int row = range.FirstRowIndex + 1; row <= range.LastRowIndex; row++)
            {
                bool rowHasData = false;

                T data = new T();

                foreach (KeyValuePair<int, PropertyInfo> mapping in _propertyMap)
                {
                    string stringValue = Convert.ToString(worksheet.Cells[row, mapping.Key].Value);

                    if (string.IsNullOrWhiteSpace(stringValue)) continue;

                    mapping.Value.SetValue(data, stringValue, null);

                    rowHasData = true;
                }

                if (rowHasData)
                    result.Add(data);
            }

            return result;
        }

        IEnumerable<FacilityData> IFacilityDataImporter.GetData()
        {
            return GetData<FacilityData>();
        }
    }
}
