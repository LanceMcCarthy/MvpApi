using MvpApi.Common.Models;
using Telerik.Data.Core;

namespace MvpApi.Uwp.Common
{
    public class TechnologyKeyLookup : IKeyLookup
    {
        public object GetKey(object instance)
        {
            return (instance as ContributionsModel)?.ContributionTechnology.Name;
        }
    }
}
