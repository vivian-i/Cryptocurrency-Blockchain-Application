using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using RestSharp;
using MinerWebApp.Models;
using ModelStructure;

namespace TransactionGeneratorWpfApp
{
    /**
     * MainWindow is the interaction logic for MainWindow.xaml.
     * It is the client gui application and a transaction generator.
     * It display how many blocks exist and what the current balances of an accounts is. 
     * It also create transactions to be submitted to the Miner web application.
     */
    public partial class MainWindow : Window
    {
        //private fields 
        static Mutex mutex = new Mutex();
        DataModel dataModel;
        //private fields of LogHelper object to help log message to file
        LogHelper logHelper = new LogHelper();

        //The main window
        public MainWindow()
        {
            //initialize component
            InitializeComponent();
            //get the current block state
            getCurrBlockState();
        }

        /**
         * This method is called when user click on create transaction button.
         * When user enter values for sender, receiver and amount, and clicks on create transation button,
         * A transaction will happens if no error occured.
         * It uses a static mutex to ensure that, at once, only one holder of the mutex executes.
         * This method represents a resource that must be synchronized so that only one thread at a time can enter.
         */
        private void CreateTransactionBtn_Click(object sender, RoutedEventArgs e)
        {
            //use mutex wait one to block the current thread until it receives a signal
            mutex.WaitOne();

            try
            {
                //check if sender, receiver and amount text box is not empty
                if (String.IsNullOrEmpty(SenderTxt.Text) == false && String.IsNullOrEmpty(ReceiverTxt.Text) == false && String.IsNullOrEmpty(AmountTxt.Text) == false)
                {
                    //check if the inputted values is of the correct type
                    bool isSenderAnUint = uint.TryParse(SenderTxt.Text, out uint numericValue1);
                    bool isReceiverAnUint = uint.TryParse(ReceiverTxt.Text, out uint numericValue2);
                    bool isAmountAFloat = float.TryParse(AmountTxt.Text, out float numericValue3);

                    //check for any format exception or if the inputted values is of the correct type
                    if (isSenderAnUint && isReceiverAnUint && isAmountAFloat)
                    {
                        //check if the inputted sender and receiver is not the same user id
                        if (SenderTxt.Text != ReceiverTxt.Text)
                        {
                            //set base url
                            string URL = "https://localhost:44334/";
                            //use RestClient and set the URL
                            RestClient client = new RestClient(URL);
                            //set up and call the API method
                            RestRequest request = new RestRequest("api/Miner/PostingTransaction/" + SenderTxt.Text + "/" + ReceiverTxt.Text + "/" + AmountTxt.Text);
                            //use IRestResponse and set the request in the client post method
                            IRestResponse resp = client.Post(request);

                            //check if response is succesful
                            if (resp.IsSuccessful)
                            {
                                //log message to file
                                logHelper.log("[INFO] CreateTransactionBtn_Click() - Succesfully called PostingTransaction method from the miner web app.");
                            }
                            //if response is not succesful, log the error message to file
                            else
                            {
                                //log error message to file
                                logHelper.log(resp.Content);
                            }

                            //get the current block state
                            getCurrBlockState();

                        }
                        //if the inputted sender and receiver is the same user id, log error message to user, console and file
                        else
                        {
                            MessageBox.Show("Error - sender and receiver id is the same. it must be a diffrent user id.");
                            logHelper.log("[ERROR] CreateTransactionBtn_Click() - sender and receiver id is the same. it must be a diffrent user id.");
                            Console.WriteLine("Error - sender and receiver id is the same. it must be a diffrent user id.");
                        }
                    }
                    //if the inputted values is not of the correct type, log error message to user, console and file
                    else
                    {
                        MessageBox.Show("Error - the inputted values is of the correct type.");
                        logHelper.log("[ERROR] CreateTransactionBtn_Click() - the inputted values is of the correct type.");
                        Console.WriteLine("Error - the inputted values is of the correct type.");
                    }
                }
                //if sender, receiver and amount text box is empty, log error message to user, console and file
                else
                {
                    MessageBox.Show("Error - the inputted values is empty.");
                    logHelper.log("[ERROR] CreateTransactionBtn_Click() - the inputted values is empty.");
                    Console.WriteLine("Error - the inputted values is empty.");
                }
            }
            //catch a json reader exception and log error to user, console and file
            catch (JsonReaderException)
            {
                MessageBox.Show("Error - json reader exception occured. the inputted values is not valid.");
                logHelper.log("[ERROR] CreateTransactionBtn_Click() - json reader exception occured. the inputted values is not valid.");
                Console.WriteLine("Error - json reader exception occured. the inputted values is not valid.");
            }
            //catch other exception and log error to user, console and file
            catch (Exception)
            {
                MessageBox.Show("Error - an exception occured.");
                logHelper.log("[ERROR] CreateTransactionBtn_Click() - an exception occured.");
                Console.WriteLine("Error - an exception occured.");
            }

            //get the current block state
            getCurrBlockState();
            //to allow another clients to post a transaction, release the mutex
            mutex.ReleaseMutex();
            
        }

        /**
         * This method is called when user click on get balance button.
         * When user enter values of user id and clicks on get balance button,
         * Balance of a user id is displayed.
         * Any valid currently unused number is an account and its amount will be displayed to 0.
         */
        private void GetBlockStateBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //check if user id text box is not empty
                if (string.IsNullOrEmpty(UserTxt.Text) == false)
                {
                    //check if the inputted values is of the correct type
                    bool isUserAnUint = uint.TryParse(UserTxt.Text, out uint numericValue1);
                    //check for any format exception or if the inputted values is of the correct type
                    if (isUserAnUint)
                    {
                        //get the current coin balance
                        float currCoinBal = dataModel.GetTransactedCurrCoinBalance(Convert.ToUInt32(UserTxt.Text));
                        //set the balance value in the gui
                        BalanceTxt.Text = currCoinBal.ToString();
                        //get the current block state
                        getCurrBlockState();
                    }
                    //if the inputted values is not of the correct type, log error message to user, console and file
                    else
                    {
                        MessageBox.Show("Error - the inputted values is of the correct type.");
                        logHelper.log("[ERROR] GetBlockStateBtn_Click() - the inputted values is of the correct type.");
                        Console.WriteLine("Error - the inputted values is of the correct type.");
                    }
                }
                //if user id text box is empty, log error message to user, console and file
                else
                {
                    MessageBox.Show("Error - the inputted values is empty.");
                    logHelper.log("[ERROR] GetBlockStateBtn_Click() - the inputted values is empty.");
                    Console.WriteLine("Error - the inputted values is empty.");
                }
            }
            //catch a json reader exception and log error to user, console and file
            catch (JsonReaderException)
            {
                MessageBox.Show("Error - json reader exception occured. the inputted values is not valid.");
                logHelper.log("[ERROR] GetBlockStateBtn_Click() - json reader exception occured. the inputted values is not valid.");
                Console.WriteLine("Error - json reader exception occured. the inputted values is not valid.");
            }
            //catch other exception and log error to user, console and file
            catch (Exception)
            {
                MessageBox.Show("Error - an exception occured.");
                logHelper.log("[ERROR] GetBlockStateBtn_Click() - an exception occured.");
                Console.WriteLine("Error - an exception occured.");
            }
        }

        /**
         * getCurrBlockState method retreives and set the current block state.
         * It gets the blockchain and set the total block number text.
         * Also, if the blockchain contains nothing, a reserved bank account is created at the start of the chain.
         */
        private void getCurrBlockState()
        {
            //creates a new data model
            dataModel = new DataModel();
            //get the blockchain
            List<BlockModel> blockList = dataModel.GetBlockList();
            //if the blockchain contains nothing, creates a reserved bank account at the start of the chain
            if (blockList == null || blockList.Count == 0)
            {
                //creates a reserved bank account at the start of the chain
                dataModel.ReservedBankAcct();
            }
            //get the blockchain
            blockList = dataModel.GetBlockList();
            //set the values of total number of block in the gui
            TotalBlockNum.Text = blockList.Count.ToString();
        }
}
}