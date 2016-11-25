using System;
using System.Globalization;
using System.Windows;
using Extensions;

using System.Linq;
using System.Data.Linq;

namespace Cana
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    // Card reader variables and functions are defined in MainWindow.pcsc.cs
    // Data management is in MainWindow.db.cs
    // The class "Cafeteria" is a derivative of System.Data.Linq.DataContext
    
    public partial class MainWindow : Window
    {
        Customer[] resultCusts = new Customer[4];

        public MainWindow()
        {
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeNFC();
            InitializeDBConnection();
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cafeteria.Dispose();
        }
        private void ShowOnStatusBar(string info)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                this.lowerLeftTextBlock.Text = info;
            });
        }
        private void DisableEditControls()
        {
            textBoxFirstName.IsEnabled = false;
            textBoxNickName.IsEnabled = false;
            textBoxLastName.IsEnabled = false;
            textBoxBalance.IsEnabled = false;
            btnEditSubmit.IsEnabled = false;

        }
        private void EnableEditControls()
        {
            textBoxFirstName.IsEnabled = true;
            textBoxNickName.IsEnabled = true;
            textBoxLastName.IsEnabled = true;
            textBoxBalance.IsEnabled = true;
            btnEditSubmit.IsEnabled = true;
        }
        private void btnEnableEditing_Click(object sender, RoutedEventArgs e)
        {
            if (btnEditSubmit.IsEnabled == false)
            {
                EnableEditControls();
                btnEnableEditing.Content = "Disable Editing";
            }
            else
            {
                UpdateEdit();
                DisableEditControls();
                btnEnableEditing.Content = "Enable Editing";
            }
        }

        private void btnEditSubmit_Click(object sender, RoutedEventArgs e) //FIX
        {
            DisableEditControls();
            if(textBoxFirstName.Text == "" || textBoxLastName.Text == ""
                || textBoxBalance.Text == "" || textBoxCardUid.Text == "")
            {
                MessageBox.Show("Please finish all fields before submiting");
                EnableEditControls();
                return;
            }
            btnEnableEditing.IsEnabled = false;
            double balance;
            if (currentCust != null)
            {
                if (double.TryParse(textBoxBalance.Text, out balance))
                {
                    currentCust.FirstName = textBoxFirstName.Text;
                    currentCust.LastName = textBoxLastName.Text;
                    currentCust.NickName = textBoxNickName.Text;
                    currentCust.Balance = balance;
                    cafeteria.Submit();
                    UpdateBalanceText();
                    ShowOnStatusBar("Customer info successfully updated.");
                }
                else
                {
                    MessageBox.Show("Please make sure that the field \"Balance\" is numeric");
                    textBoxBalance.Text = "";
                }
            }
            else
            {
                MessageBox.Show("Please select a customer first.");
            }
            EnableEditControls();
            btnEnableEditing.IsEnabled = true;
        }
        private void UpdateBalanceText()
        {
            string balanceStr = currentCust.Balance.InDollars();
            textBlockBalanceBefore.Text = balanceStr;
            textBlockBalanceAfter.Text = balanceStr;
            textBoxBalance.Text = currentCust.Balance.ToString();
        }
        private void textBoxOrderAmount_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            double orderAmount;
            if (currentCust != null)
            {
                if (double.TryParse(textBoxOrderAmount.Text, out orderAmount))
                {
                    string balanceStr = (currentCust.Balance - orderAmount).InDollars();
                    textBlockBalanceAfter.Text = balanceStr;
                    return;
                }
                else if (textBoxOrderAmount.Text == "")
                {
                    textBlockBalanceAfter.Text = currentCust.Balance.InDollars();
                    return;
                }
                else if (textBoxOrderAmount.Text == "-" || textBoxOrderAmount.Text == "-." ||
                    textBoxOrderAmount.Text == "+" || textBoxOrderAmount.Text == ".")
                {
                    return;
                }
                else
                {
                    MessageBox.Show("Invalid input. Please make sure that the \"Amount\" field is numeric");
                    textBoxOrderAmount.Text = "";
                    return;
                }
            }
            else if(textBoxOrderAmount.Text == "")
            {
                return;
            }
            else
            {
                MessageBox.Show("Please select a customer first.");
                textBoxOrderAmount.Text = "";
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            double orderAmount;
            if(currentCust != null)
            {
                if (Double.TryParse(textBoxOrderAmount.Text, out orderAmount))
                {
                    if(orderAmount < 0)
                    {
                        MessageBoxResult result =
                            MessageBox.Show(string.Format("Negative order amount detected. Click Yes to REFILL {0} {1}'s "+
                            "card by {2}, or No to cancel",
                            currentCust.FirstName, currentCust.LastName, (-orderAmount).InDollars()), 
                            "Refill?", MessageBoxButton.YesNo);
                        if(result == MessageBoxResult.No)
                        {
                            textBoxOrderAmount.Text = "";
                            return;
                        }
                    }
                        cafeteria.PlaceOrder(currentCust, orderAmount);
                    if (orderAmount < 0)
                    {
                        ShowOnStatusBar(String.Format("Refilled successfully for {0} {1}, amount: {2}.", currentCust.FirstName,
                            currentCust.LastName, (-orderAmount).InDollars()));
                    }
                    else
                    {
                        ShowOnStatusBar(String.Format("Order placed for {0} {1}, amount: {2}.", currentCust.FirstName,
                            currentCust.LastName, orderAmount.InDollars()));
                    }
                        textBoxOrderAmount.Text = "";
                        UpdateBalanceText();
                }
                else
                {
                    MessageBox.Show("Invalid input.");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Please select a customer first.");
            }
        }
        private void UndoPayment_Click(object sender, RoutedEventArgs e)
        {
            if(currentCust == null)
            {
                MessageBox.Show("Please select a customer first.");
                return;
            }
            cafeteria.UndoLastTransaction();
            ShowOnStatusBar("Transaction cancelled.");
        }

        private void RedoPayment_Click(object sender, RoutedEventArgs e)
        {
            if (currentCust == null)
            {
                MessageBox.Show("Please select a customer first.");
                return;
            }
            cafeteria.RedoLastTransaction();
            ShowOnStatusBar("Transaction redone");
        }

        private void textBoxLookup_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int customerID;
            string input = textBoxLookup.Text;
            if (int.TryParse(input, out customerID))
            {
                Customer cust = cafeteria.GetCustomer(customerID);
                if (cust == null)
                {
                    linkResult1.Inlines.Clear();
                    return;
                }
                else
                {
                    resultCusts[0] = cust;
                    if (resultCusts[0].NickName == null || resultCusts[0].NickName == "")
                    {
                        linkResult1.Inlines.Clear();
                        linkResult1.Inlines.Add(string.Format("{0} {1}", resultCusts[0].FirstName, resultCusts[0].LastName));
                    }
                    else
                    {
                        linkResult1.Inlines.Clear();
                        linkResult1.Inlines.Add(string.Format("{0} {1}", resultCusts[0].NickName, resultCusts[0].LastName));
                    }
                }
            }
            else if (input == "")
            {
                linkResult1.Inlines.Clear();
                linkResult2.Inlines.Clear();
                linkResult3.Inlines.Clear();
                linkResult4.Inlines.Clear();
            }
            else
            {
                linkResult1.Inlines.Clear();
                linkResult2.Inlines.Clear();
                linkResult3.Inlines.Clear();
                linkResult4.Inlines.Clear();
                var list = cafeteria.ListCustomerWithIncompleteName(input);
                if (list == null)
                {
                    return;
                }
                var listArray = list.ToArray();
                switch (list.Count())
                {
                    case 0:
                        return;
                    case 1:
                        resultCusts[0] = list.Single();
                        if (resultCusts[0].NickName == null || resultCusts[0].NickName == "")
                        {
                            linkResult1.Inlines.Clear();
                            linkResult1.Inlines.Add(string.Format("{0} {1}", resultCusts[0].FirstName, resultCusts[0].LastName));
                        }
                        else
                        {
                            linkResult1.Inlines.Clear();
                            linkResult1.Inlines.Add(string.Format("{0} {1}", resultCusts[0].NickName, resultCusts[0].LastName));
                        }
                        break;
                    case 2:
                        resultCusts[0] = listArray[0];
                        resultCusts[1] = listArray[1];
                        if (resultCusts[0].NickName == null || resultCusts[0].NickName == "")
                        {
                            linkResult1.Inlines.Clear();
                            linkResult1.Inlines.Add(string.Format("{0} {1}", resultCusts[0].FirstName, resultCusts[0].LastName));
                        }
                        else
                        {
                            linkResult1.Inlines.Clear();
                            linkResult1.Inlines.Add(string.Format("{0} {1}", resultCusts[0].NickName, resultCusts[0].LastName));
                        }
                        if (resultCusts[1].NickName == null || resultCusts[1].NickName == "")
                        {
                            linkResult2.Inlines.Clear();
                            linkResult2.Inlines.Add(string.Format("{0} {1}", resultCusts[1].FirstName, resultCusts[1].LastName));
                        }
                        else
                        {
                            linkResult2.Inlines.Clear();
                            linkResult2.Inlines.Add(string.Format("{0} {1}", resultCusts[1].NickName, resultCusts[1].LastName));
                        }
                        break;
                    case 3:
                        resultCusts[0] = listArray[0];
                        resultCusts[1] = listArray[1];
                        resultCusts[2] = listArray[2];

                        if (resultCusts[0].NickName == null || resultCusts[0].NickName == "")
                        {
                            linkResult1.Inlines.Clear();
                            linkResult1.Inlines.Add(string.Format("{0} {1}", resultCusts[0].FirstName, resultCusts[0].LastName));
                        }
                        else
                        {
                            linkResult1.Inlines.Clear();
                            linkResult1.Inlines.Add(string.Format("{0} {1}", resultCusts[0].NickName, resultCusts[0].LastName));
                        }
                        if (resultCusts[1].NickName == null || resultCusts[1].NickName == "")
                        {
                            linkResult2.Inlines.Clear();
                            linkResult2.Inlines.Add(string.Format("{0} {1}", resultCusts[1].FirstName, resultCusts[1].LastName));
                        }
                        else
                        {
                            linkResult2.Inlines.Clear();
                            linkResult2.Inlines.Add(string.Format("{0} {1}", resultCusts[1].NickName, resultCusts[1].LastName));
                        }
                        if (resultCusts[2].NickName == null || resultCusts[2].NickName == "")
                        {
                            linkResult2.Inlines.Clear();
                            linkResult2.Inlines.Add(string.Format("{0} {1}", resultCusts[2].FirstName, resultCusts[2].LastName));
                        }
                        else
                        {
                            linkResult2.Inlines.Clear();
                            linkResult2.Inlines.Add(string.Format("{0} {1}", resultCusts[2].NickName, resultCusts[2].LastName));
                        }
                        break;
                    case 4:
                    default:
                        resultCusts[0] = listArray[0];
                        resultCusts[1] = listArray[1];
                        resultCusts[2] = listArray[2];
                        resultCusts[3] = listArray[3];
                        if (resultCusts[0].NickName == null || resultCusts[0].NickName == "")
                        {
                            linkResult1.Inlines.Clear();
                            linkResult1.Inlines.Add(string.Format("{0} {1}", resultCusts[0].FirstName, resultCusts[0].LastName));
                        }
                        else
                        {
                            linkResult1.Inlines.Clear();
                            linkResult1.Inlines.Add(string.Format("{0} {1}", resultCusts[0].NickName, resultCusts[0].LastName));
                        }
                        if (resultCusts[1].NickName == null || resultCusts[1].NickName == "")
                        {
                            linkResult2.Inlines.Clear();
                            linkResult2.Inlines.Add(string.Format("{0} {1}", resultCusts[1].FirstName, resultCusts[1].LastName));
                        }
                        else
                        {
                            linkResult2.Inlines.Clear();
                            linkResult2.Inlines.Add(string.Format("{0} {1}", resultCusts[1].NickName, resultCusts[1].LastName));
                        }
                        if (resultCusts[2].NickName == null || resultCusts[2].NickName == "")
                        {
                            linkResult3.Inlines.Clear();
                            linkResult3.Inlines.Add(string.Format("{0} {1}", resultCusts[2].FirstName, resultCusts[2].LastName));
                        }
                        else
                        {
                            linkResult3.Inlines.Clear();
                            linkResult3.Inlines.Add(string.Format("{0} {1}", resultCusts[2].NickName, resultCusts[2].LastName));
                        }
                        if (resultCusts[3].NickName == null || resultCusts[3].NickName == "")
                        {
                            linkResult4.Inlines.Clear();
                            linkResult4.Inlines.Add(string.Format("{0} {1}", resultCusts[3].FirstName, resultCusts[3].LastName));
                        }
                        else
                        {
                            linkResult4.Inlines.Clear();
                            linkResult4.Inlines.Add(string.Format("{0} {1}", resultCusts[3].NickName, resultCusts[3].LastName));
                        }
                        break;
                }
            }

        }
        private void UpdateEdit()
        {
            textBoxFirstName.Text = currentCust.FirstName;
            textBoxNickName.Text = currentCust.NickName;
            textBoxLastName.Text = currentCust.LastName;
            textBoxCardUid.Text = currentCust.CardUid;
            textBoxBalance.Text = currentCust.Balance.ToString();
        }
        private void linkResult1_Click(object sender, RoutedEventArgs e)
        {
            currentCust = resultCusts[0];
            textBoxLookup.Text = "";
            UpdateBalanceText();
            UpdateEdit();
            btnEnableEditing.IsEnabled = true;
        }

        private void linkResult2_Click(object sender, RoutedEventArgs e)
        {
            currentCust = resultCusts[1];
            textBoxLookup.Text = "";
            UpdateBalanceText();
            UpdateEdit();
            btnEnableEditing.IsEnabled = true;
        }

        private void linkResult3_Click(object sender, RoutedEventArgs e)
        {
            currentCust = resultCusts[2];
            textBoxLookup.Text = "";
            UpdateBalanceText();
            UpdateEdit();
            btnEnableEditing.IsEnabled = true;
        }

        private void linkResult4_Click(object sender, RoutedEventArgs e)
        {
            currentCust = resultCusts[3];
            textBoxLookup.Text = "";
            UpdateBalanceText();
            UpdateEdit();
            btnEnableEditing.IsEnabled = true;
        }
    }
}
