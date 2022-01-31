using Telerik.Data.Core;

namespace MvpCompanion.UI.WinUI
{
    public class CustomIKeyLookup : IKeyLookup
    {
        public object GetKey(object instance)
        {
            return (instance as Product).Name[0];
        }
    }
}
