using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Freedom.Client.Infrastructure.LookupData
{
    public class MonthsOfYearDataProvider : DataSourceProvider
    {
        protected override void BeginQuery()
        {
            QueryWorker(null);
        }

        private void QueryWorker(object obj)
        {
            try
            {
                List<KeyValuePair<int, string>> data =
                    CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                        .Where(monthName => !string.IsNullOrEmpty(monthName))
                        .Select((s, i) => new KeyValuePair<int, string>(i + 1, s))
                        .ToList();

                OnQueryFinished(data);
            }
            catch (Exception e)
            {
                OnQueryFinished(null, e, null, obj);
            }
        }
    }
}
