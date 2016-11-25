using System;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Windows;
using System.Configuration;
using System.Data.SqlClient;
using Extensions;
using System.ComponentModel;

namespace Cana
{
    [Table(Name = "Customers")]
    public class Customer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _LastName;
        [Column(Storage = "_LastName")]
        public string LastName
        {
            get
            {
                return this._LastName;
            }
            set
            {
                this._LastName = value;
                OnPropertyChanged("LastName");
            }
        }
        private string _FirstName;
        [Column(Storage = "_FirstName")]
        public string FirstName
        {
            get
            {
                return this._FirstName;
            }
            set
            {
                this._FirstName = value;
                OnPropertyChanged("FirstName");
            }
        }
        private string _NickName;
        [Column(Storage = "_NickName")]
        public string NickName
        {
            get
            {
                return this._NickName;
            }
            set
            {
                this._NickName = value;
                OnPropertyChanged("NickName");
            }
        }
        private int _CustomerID;
        [Column(Storage = "_CustomerID", IsPrimaryKey = true)]
        public int CustomerID
        {
            get
            {
                return this._CustomerID;
            }
            set
            {
                this._CustomerID = value;
                OnPropertyChanged("CustomerID");
            }

        }
        private double _Balance;
        [Column(Storage = "_Balance")]
        public double Balance
        {
            get
            {
                return this._Balance;
            }
            set
            {
                this._Balance = value;
                OnPropertyChanged("Balance");
            }
        }
        private string _CardUid;
        [Column(Storage = "_CardUid")]
        public string CardUid
        {
            get
            {
                return this._CardUid;
            }
            set
            {
                this._CardUid = value;
                OnPropertyChanged("CardUid");
            }
        }
        /*       public string GetBalanceString()        // Use Double.InDollars()
               {
                   // Without the CultureInfo modifier, a negative balance will have parenthesis around after the negative sign
                   CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
                   culture.NumberFormat.CurrencyNegativePattern = 1;
                   string balanceStr = String.Format(culture, "{0:C2}", this.Balance);
                   return balanceStr;
               }*/
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        public override string ToString()
        {
      //      string balanceStr = GetBalanceString();

            return String.Format("Customer: {0} {1}\nCard UID: {2}\nBalance: {3}\nIndex ID: {4}", this.FirstName,
                                this.LastName, this.CardUid, this.Balance.InDollars(), this.CustomerID);
        }
    }
}
