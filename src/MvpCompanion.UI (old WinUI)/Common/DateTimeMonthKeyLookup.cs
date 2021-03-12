using MvpApi.Common.Models;
using Telerik.Data.Core;

namespace MvpCompanion.UI.Common
{
    public class DateTimeMonthKeyLookup : IKeyLookup
    {
        public object GetKey(object instance)
        {
            return (instance as ContributionsModel)?.StartDate?.Date;
        }
    }
}
