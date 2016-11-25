using System;
using System.Linq;
using System.Data.Linq;
using System.Windows;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Linq.Mapping;

namespace Cana
{
    public class Cafeteria : DataContext
    {
        public Table<Customer> Customers;
        public Table<Transaction> Transactions;
        private WndCustInfo wndNewCust;
        private Transaction lastTransaction;

        public Cafeteria(string fileOrServerOrConnection) : base(fileOrServerOrConnection)
        {
            Customers = GetTable<Customer>();
            Transactions = GetTable<Transaction>();
        }
        public void RefreshTransactions()
        {
            try
            {
                Refresh(RefreshMode.OverwriteCurrentValues, Transactions);
            }
            catch (SqlException se)
            {
                MessageBoxResult result = MessageBox.Show(String.Format("Error: {0} \nRetry?", se.Message), "Failed to refresh table",
                    MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                    RefreshTransactions();
            }
        }
        public void RefreshCustomers()  // RECEIVING FROM DB
        {
            try
            {
                Refresh(RefreshMode.OverwriteCurrentValues, Customers);
            }
            catch (SqlException se)
            {
                MessageBoxResult result = MessageBox.Show(String.Format("Error: {0} \nRetry?", se.Message), "Failed to refresh table",
                    MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                    RefreshCustomers();
            }
        }
        public void Submit()   // SENDING TO DB
        {
            try
            {
                SubmitChanges();
            }
            catch (Exception e)
            {
                MessageBoxResult result = MessageBox.Show(String.Format("Error: {0} \nRetry?", e.Message), "Failed to submit changes",
                                MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    Submit();
                }
            }
        }
        public Customer GetCustomer(string cardUid)
        {
            RefreshCustomers();
            Customer matchingCust;
            try
            {
                matchingCust = Customers.Single(c => c.CardUid == cardUid);
            }
            catch
            {
                MessageBoxResult result = MessageBox.Show("No matching customer found. Assign this card to a new customer?", "No such UID",
                                                        MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {

                        wndNewCust = new WndCustInfo(this, cardUid);
                        wndNewCust.ShowDialog();  // wndNewCust will insert the NewCust into Customers and reset it

                    });
                }
                return null;
            }
            return matchingCust;
        }
        public Customer GetCustomer(int customerID)
        {
            RefreshCustomers();
            Customer matchingCust;
            try
            {
                matchingCust = Customers.Single(c => c.CustomerID == customerID);
            }
            catch
            {
                return null;
            }
            return matchingCust;
        }
        public Transaction GetTransaction(int transactionID)
        {
            Transaction matchingTrans;
            try
            {
                matchingTrans = Transactions.Single(t => t.TransactionID == transactionID);
            }
            catch
            {
                MessageBox.Show(string.Format("No such transaction ID: {0}", transactionID), "Transaction not found",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return null;
            }
            return matchingTrans;
        }
        public void InsertNewCustomer(Customer cust)
        {
            try
            {
                RefreshCustomers();
                if (Customers.Count() != 0)
                {
                    cust.CustomerID = (from customer in Customers
                                       where customer.CustomerID == (Customers.Max(c1 => c1.CustomerID))
                                       select customer).SingleOrDefault().CustomerID + 1;
                }
                else
                {
                    cust.CustomerID = 0;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            Customers.InsertOnSubmit(cust);
            Submit();
        }
        private void AssignNewCardToExistingCustomer(Customer cust, string cardUid)
        {
            try
            {
                RefreshCustomers();
                cust.CardUid = cardUid;
                Customers.InsertOnSubmit(cust);
                Submit();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }
        private double ChangeBalance(string cardUid, double newBalance)
        {
            Customer cust = GetCustomer(cardUid);
            double oldBalance = cust.Balance;
            cust.Balance = newBalance;
            Submit();
            return oldBalance;
        }
        private double ChangeBalance(int customerID, double newBalance)
        {
            Customer cust = GetCustomer(customerID);
            double oldBalance = cust.Balance;
            cust.Balance = newBalance;
            Submit();
            return oldBalance;
        }
        private double ChangeBalance(Customer cust, double newBalance)
        {
            double oldBalance = cust.Balance;
            cust.Balance = newBalance;
            Submit();
            return oldBalance;
        }
        public void PlaceOrder(Customer cust, double orderAmount)
        {
            Transaction trans = new Transaction();
            trans.TimeOfOrder = DateTime.Now;
            trans.OrderAmount = orderAmount;
            trans.CustomerID = cust.CustomerID;
            trans.BalanceBeforeOrder = cust.Balance;
            try
            {
                if (Transactions.Count() != 0)
                {
                    trans.TransactionID = Transactions.Max(t => t.TransactionID) + 1;
                }
                else
                {
                    trans.TransactionID = 0;
                }
                ChangeBalance(cust, cust.Balance - orderAmount);
                Transactions.InsertOnSubmit(trans);
                Submit();
            }
            catch(NullReferenceException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }
        public void DeleteCustomer(Customer cust)
        {
            this.Customers.DeleteOnSubmit(cust);
            Submit();
        }
        public void DeleteCustomer(string cardUid)
        {
            this.Customers.DeleteOnSubmit(GetCustomer(cardUid));
            Submit();
        }
        public void DeleteCustomer(int customerID)
        {
            this.Customers.DeleteOnSubmit(GetCustomer(customerID));
            Submit();
        }
 /*       public void DeleteTransaction(Transaction trans)        // Defining function WILL TRIGGER InvalidOperationException even when
        {                                                         // only DeleteTransaction(int) is used for unknown reasons.
            Transactions.DeleteOnSubmit(trans);                  
            Submit();
        }*/
        public void DeleteTransaction(int transactionID)
        {
            Transaction trans = GetTransaction(transactionID);
            Transactions.DeleteOnSubmit(trans);
            Submit();
        }
        public void UndoLastTransaction()
        {
            Transaction trans = Transactions.OrderByDescending(i => i.TransactionID).FirstOrDefault();
            if (trans == null)
                return;
            lastTransaction = new Transaction();
            lastTransaction.CustomerID = trans.CustomerID;
            lastTransaction.TransactionID = trans.TransactionID;
            lastTransaction.OrderAmount = trans.OrderAmount;
            lastTransaction.TimeOfOrder = trans.TimeOfOrder;
            lastTransaction.BalanceBeforeOrder = trans.BalanceBeforeOrder;

            Customer cust = GetCustomer(trans.CustomerID);
            cust.Balance = trans.BalanceBeforeOrder;
            DeleteTransaction(trans.TransactionID);

            Submit();
        }
        public IQueryable<Customer> ListCustomerWithFirstName(string firstName)
        {
            var list = from cust in Customers where cust.FirstName == firstName select cust;
            return list;
        }
        public IQueryable<Customer> ListCustomerWithIncompleteName(string incompleteName)
        {
            var listFirst = from cust in Customers where cust.FirstName.Contains(incompleteName) select cust;
            var listNick = from cust in Customers where cust.NickName.Contains(incompleteName) select cust;
            var listLast = from cust in Customers where cust.LastName.Contains(incompleteName) select cust;

            return listFirst.Union(listNick.Union(listLast));
        }
        public IQueryable<Customer> ListCustomerWithLastName(string lastName)
        {
            var list = from cust in Customers where cust.LastName == lastName select cust;
            return list;
        }
        public void RedoLastTransaction()
        {
            if(lastTransaction == null)
                return;
            var laterTransactions = from trans in Transactions    // In case other transactions has taken place in other stations
                                    where trans.TransactionID > lastTransaction.TransactionID
                                    select trans;
            if (laterTransactions.Count() != 0)
            {
                foreach (Transaction trans in laterTransactions)
                {
                    trans.TransactionID++;      // Possible DuplicateKeyException. needs to be tested with a second machine
                }
            }
            Transactions.InsertOnSubmit(lastTransaction);
            Submit();
        }
        public void DeleteAllCustomers()
        {
            //       this.Customers.DeleteAllOnSubmit(Customers);          // Inefficient
            //       Submit();
            this.ExecuteCommand("TRUNCATE TABLE Customers");

        }
        public void DeleteAllTransactions()
        {
            //       this.Transactions.DeleteAllOnSubmit(Transactions);    // Takes ages
            //       Submit();
            this.ExecuteCommand("TRUNCATE TABLE Transactions");
        }
        public void PlaceOrder(string cardUid, double orderAmount)
        {
            Customer cust = GetCustomer(cardUid);
            Transaction trans = new Transaction();
            DateTime now = DateTime.Now;
            trans.TimeOfOrder = DateTime.Now;
            trans.OrderAmount = orderAmount;
            trans.CustomerID = cust.CustomerID;
            trans.BalanceBeforeOrder = cust.Balance;
            try
            {
                if (Transactions.Count() != 0)
                {
                    trans.TransactionID = Transactions.Max(t => t.TransactionID) + 1;
                }
                else
                {
                    trans.TransactionID = 0;
                }
                ChangeBalance(cust, cust.Balance - orderAmount);
                Transactions.InsertOnSubmit(trans);
                Submit();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }
        public void PlaceOrder(int customerID, double orderAmount)
        {
            Customer cust = GetCustomer(customerID);
            Transaction trans = new Transaction();
            DateTime now = DateTime.Now;
            trans.TimeOfOrder = DateTime.Now;
            trans.OrderAmount = orderAmount;
            trans.CustomerID = cust.CustomerID;
            trans.BalanceBeforeOrder = cust.Balance;
            try
            {
                if (Transactions.Count() != 0)
                {
                    trans.TransactionID = Transactions.Max(t => t.TransactionID) + 1;
                }
                else
                {
                    trans.TransactionID = 0;
                }
                ChangeBalance(cust, cust.Balance - orderAmount);
                Transactions.InsertOnSubmit(trans);
                Submit();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
        }
    }
}