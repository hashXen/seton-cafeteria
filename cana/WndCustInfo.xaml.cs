using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Cana
{
    /// <summary>
    /// Interaction logic for WndCustInfo.xaml
    /// </summary>
    public partial class WndCustInfo : Window
    {
        Cafeteria cafeteria;

        public WndCustInfo(Cafeteria cafeteria, string cardUid)
        {
            this.cafeteria = cafeteria;

            InitializeComponent();
            this.textBoxCardUid.Text = cardUid;
        }
        public WndCustInfo()
        {
            this.Closing += WndCustInfo_Closing;
            InitializeComponent();
        }

        private void WndCustInfo_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.cafeteria.Dispose();
        }

        private void buttonSubmit_Click(object sender, RoutedEventArgs e)
        {
            double balance;
            Customer newCust = new Customer();

            if (textBoxFirstName.Text == "" || textBoxLastName.Text == "" ||
                textBoxBalance.Text == "" || textBoxCardUid.Text == "")
            {
                MessageBox.Show("Please finish all required fields before clicking the Submit button");
            }
            else
            {
                if (double.TryParse(textBoxBalance.Text, out balance))
                {
                    newCust.FirstName = textBoxFirstName.Text;
                    newCust.LastName = textBoxLastName.Text;
                    newCust.Balance = balance;
                    newCust.CardUid = textBoxCardUid.Text;
                    newCust.NickName = textBoxNickName.Text;
                    this.cafeteria.InsertNewCustomer(newCust);

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please make sure that the field \"Balance\" is numeric");
                    textBoxBalance.Text = "";
                }
            }
        }
    }
}
