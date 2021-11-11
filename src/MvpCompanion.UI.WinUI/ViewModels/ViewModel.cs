using CommonHelpers.Common;
using MvpCompanion.UI.WinUI.Models;
using System.Collections.Generic;
using System.Linq;

namespace MvpCompanion.UI.WinUI.ViewModels
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
