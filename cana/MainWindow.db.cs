using System;
using System.Linq;
using System.Data.Linq;
using System.Windows;
using System.Configuration;
using System.Data.SqlClient;

namespace Cana
{
    public partial class MainWindow : Window
    {
        private Cafeteria cafeteria;
        private string connString;
        public Customer newCust;
        private void RetrieveConnectionString()
        {
            try
            {
                connString = ConfigurationManager.ConnectionStrings["CanaDB"].ConnectionString;
            }
            catch (NullReferenceException)
            {
                MessageBoxResult result = MessageBox.Show("No connection string found in the configuration file. Do you want to set it up?",
                "SQL Connection String Not Found", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    if (SetConnectionString() == false)
                    {
                        Environment.Exit(-1);  //Exit if the test of connection string fails
                    }
                    //Otherwise, great, it works
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }
        private bool SetConnectionString()
        {
            WndConnString wcs = new WndConnString();
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStrings = config.ConnectionStrings.ConnectionStrings["CanaDB"];
            wcs.ShowDialog();
            this.connString = wcs.txtConnString.Text;

            // Test if the connection string actually works
            try
            {
                using (SqlConnection connTest = new SqlConnection(connString))
                {
                    connTest.Open();
                }
            }
            catch
            {
                MessageBox.Show("Sorry, this connection string doesn't seem to work. "
                    + "Please check the connection string or/and the status of the server",
                    "Connection string invalid or connection error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            // Update new connection string to app.config
            var connectionString = config.ConnectionStrings.ConnectionStrings["CanaDB"];
            if (connectionString == null)
            {
                //Add connection string to config file if it's not already there
                config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("CanaDB", connString));
            }
            config.Save();

            return true;
        }
        private void InitializeDBConnection()
        {
            ShowOnStatusBar("Setting up connection to database...");
            RetrieveConnectionString();
            try
            {
                cafeteria = new Cafeteria(connString);
                ShowOnStatusBar("Ready.");
            }
            catch (Exception e)
            {
                MessageBoxResult result = MessageBox.Show(String.Format("Error: {0} \n\nRetry?", e.Message), "Connection Error",
                    MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                {
                    InitializeDBConnection();
                }
                else
                {
                    Environment.Exit(-1);
                }

            }
        }
    }
}