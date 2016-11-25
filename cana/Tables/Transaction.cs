using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;
using Extensions;
using System.ComponentModel;

namespace Cana
{
    [Table(Name = "Transactions")]
    public class Transaction : INotifyPropertyChanged
    {
        private int _TransactionID;
        [Column(Storage = "_TransactionID", IsPrimaryKey = true)]
        public int TransactionID
        {
            get
            {
                return this._TransactionID;
            }
            set
            {
                this._TransactionID = value;
                OnPropertyChanged("TransactionID");
            }
        }
        private int _CustomerID;
        [Column(Storage = "_CustomerID")]
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
        private DateTime _TimeOfOrder;
        [Column(Storage = "_TimeOfOrder")]
        public DateTime TimeOfOrder
        {
            get
            {
                return this._TimeOfOrder;
            }
            set
            {
                this._TimeOfOrder = value;
                OnPropertyChanged("TimeOfOrder");
            }
        }
        private double _OrderAmount;
        [Column(Storage = "_OrderAmount")]
        public double OrderAmount
        {
            get
            {
                return this._OrderAmount;
            }
            set
            {
                this._OrderAmount = value;
            }
        }
        private double _BalanceBeforeOrder;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        [Column(Storage = "_BalanceBeforeOrder")]
        public double BalanceBeforeOrder
        {
            get
            {
                return this._BalanceBeforeOrder;
            }
            set
            {
                this._BalanceBeforeOrder = value;
            }
        }
        public override string ToString()
        {
            string str = string.Format("TransactionID: {0};\nCustomerID: {1};\nTimeOfOrder: " +
                "{2};\nOrderAmount: {3};\nBalanceBeforeOrder: {4}", TransactionID, CustomerID, TimeOfOrder, OrderAmount.InDollars(),
                BalanceBeforeOrder.InDollars());
            return str;
        }
    }
}
