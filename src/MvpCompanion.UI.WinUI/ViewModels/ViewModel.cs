using System.Collections.Generic;
using System.Linq;
using Telerik.Core;

namespace MvpCompanion.UI.WinUI
{
    public class ViewModel : ViewModelBase
    {
        IList<Product> productsList;

        public IList<Product> ProductsList
        {
            get
            {
                if (productsList == null)
                {
                    productsList = ProductsGenerator.GetData(50).ToList();
                }

                return productsList;
            }
        }
    }
}
