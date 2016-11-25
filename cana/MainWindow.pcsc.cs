using System;
using System.Windows;
using PCSC;
using PCSC.Iso7816;
using Extensions;

namespace Cana
{
    public partial class MainWindow : Window
    {
        private static readonly IContextFactory _contextFactory = ContextFactory.Instance;
        private SCardReader rfidReader;
        private SCardContext context;
        private SCardMonitor monitor;
        private CommandApdu apdu;
        private string readerName;
        private Customer currentCust;
        
        public void InitializeNFC()
        {
            ShowOnStatusBar("Initializing card reader...");
            context = new SCardContext();
            context.Establish(SCardScope.System);
            var readerNames = context.GetReaders();
            if (readerNames == null || readerNames.Length < 1)
            {
                MessageBoxResult result = MessageBox.Show("No NFC readers detected, please reconnect the reader and run the program again",
                    "NFC reader not found", MessageBoxButton.OK, MessageBoxImage.Stop);
                Environment.Exit(-1);
            }
            readerName = readerNames[0];
            rfidReader = new SCardReader(context);
            apdu = new CommandApdu(IsoCase.Case2Short, rfidReader.ActiveProtocol)
            {
                CLA = 0xFF,
                Instruction = InstructionCode.GetData,
                P1 = 0x00,
                P2 = 0x00,
                Le = 0
            };
            monitor = new SCardMonitor(_contextFactory, SCardScope.System);
            monitor.Start(readerName);

            // This event is triggered whenever card insertion happens
            monitor.CardInserted += (sender, args) => OnCardInsertion();
        }
        public void OnCardInsertion()
        {
            cafeteria.RefreshCustomers();
            string cardUid = RetrieveCardUID();
            if (cardUid == "No uid received, please reswipe the card" || cardUid == "Failed to retrieve card UID")
            {
                ShowOnStatusBar(cardUid);
            }
            else
            {
                    ShowOnStatusBar(String.Format("Last scanned: Card UID: {0}", cardUid));   // In case it's a new card and no info was put in
                    currentCust = cafeteria.GetCustomer(cardUid);
                if (currentCust != null)
                {
                    ShowOnStatusBar(String.Format("Last scanned: Card UID: {0}   Owner: {1} {2}   Customer ID: {3}", cardUid, currentCust.FirstName, currentCust.LastName, currentCust.CustomerID));
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        btnEnableEditing.IsEnabled = true;

                        textBoxFirstName.Text = currentCust.FirstName;
                        textBoxLastName.Text = currentCust.LastName;
                        textBoxBalance.Text = currentCust.Balance.ToString();
                        textBoxCardUid.Text = currentCust.CardUid;

                        textBlockBalanceBefore.Text = currentCust.Balance.InDollars();
                        textBlockBalanceAfter.Text = currentCust.Balance.InDollars();
                    });
                }
            }
        }

        public string RetrieveCardUID()
        {
            var sc = rfidReader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);

            if (sc != SCardError.Success)
            {
                MessageBox.Show("The reader did not have enough time to read, \nplease reswipe the card", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return "Failed to retrieve card UID";
            }
            var receivePci = new SCardPCI(); // IO returned protocol control information.
            var sendPci = SCardPCI.GetPci(rfidReader.ActiveProtocol);

            var receiveBuffer = new byte[256];
            var command = apdu.ToArray();

            sc = rfidReader.Transmit(sendPci, command, receivePci, ref receiveBuffer); 

            if (sc != SCardError.Success)
            {
                MessageBox.Show("Error: " + SCardHelper.StringifyError(sc));
                return "Failed to retrieve card UID";
            }
            var responseApdu = new ResponseApdu(receiveBuffer, IsoCase.Case2Short, rfidReader.ActiveProtocol);
            rfidReader.EndTransaction(SCardReaderDisposition.Leave);
            rfidReader.Disconnect(SCardReaderDisposition.Reset);
            return responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No uid received, please reswipe the card";
        }
    }
}
