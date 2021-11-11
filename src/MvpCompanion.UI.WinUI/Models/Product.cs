using System;
using Telerik.Core;

namespace MvpCompanion.UI.WinUI.Models
{
    public class Product : ViewModelBase
    {
        private int id;
        private string name;
        private double unitPrice;
        private DateTime date;
        private bool discontinued;

        public Product()
        {
        }

        public Product(Random random)
        {
            ProductsGenerator.SetRandomPropertyValues(this, random);
        }

        public Product(int ID, string Name, double UnitPrice, DateTime Date,
            bool Discontinued)
        {
            this.ID = ID;
            this.Name = Name;
            this.UnitPrice = UnitPrice;
            this.Date = Date;
            this.Discontinued = Discontinued;
        }

        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public double UnitPrice
        {
            get
            {
                return unitPrice;
            }
            set
            {
                unitPrice = value;
                OnPropertyChanged();
            }
        }

        public DateTime Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;
                OnPropertyChanged();
            }
        }

        public bool Discontinued
        {
            get
            {
                return discontinued;
            }
            set
            {
                discontinued = value;
                OnPropertyChanged();
            }
        }
    }
}
