using MvpCompanion.Shared.Models;
using Telerik.Data.Core;

namespace MvpCompanion.Shared.Common
{
    public class TechnologyKeyLookup : IKeyLookup
    {
        public object GetKey(object instance)
        {
            return (instance as ContributionsModel)?.ContributionTechnology.Name;
        }
    }
}
