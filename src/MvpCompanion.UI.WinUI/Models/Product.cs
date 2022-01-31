using System;
using Telerik.Core;

namespace MvpCompanion.UI.WinUI
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
                return this.id;
            }
            set
            {
                this.id = value;
                this.OnPropertyChanged();
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
                this.OnPropertyChanged();
            }
        }

        public double UnitPrice
        {
            get
            {
                return this.unitPrice;
            }
            set
            {
                this.unitPrice = value;
                this.OnPropertyChanged();
            }
        }

        public DateTime Date
        {
            get
            {
                return this.date;
            }
            set
            {
                this.date = value;
                this.OnPropertyChanged();
            }
        }

        public bool Discontinued
        {
            get
            {
                return this.discontinued;
            }
            set
            {
                this.discontinued = value;
                this.OnPropertyChanged();
            }
        }
    }
}
