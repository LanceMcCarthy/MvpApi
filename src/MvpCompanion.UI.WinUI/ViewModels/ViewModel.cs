using System.Collections.Generic;
using System.Linq;
using Telerik.Core;

namespace MvpCompanion.UI.WinUI.ViewModels
{
    public class ViewModel : ViewModelBase
    {
        private IList<Product> productsList;

        public IList<Product> ProductsList => productsList ??= ProductsGenerator.GetData(50).ToList();
    }
}
