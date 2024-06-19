using MinerWebApp.Models;
using ModelStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Http;

namespace MinerWebApp.Controllers
{
    /**
     * MinerController is an ASP.NET Web controller class.
     * It is a Web Service that accepts transactions from the transaction program and generates blocks to add those transactions to the blockchain.
     * It allow the transaction program to asynchronously add new transactions.
     * MinerController do logs message informations, details and errors to a file by using LogHelper class.
     * MinerController contains a method to post a transcation.
     */
    public class MinerController : ApiController
    {
        //private fields
        DataModel dataModel;
        static bool isStart = false;
		static Queue<TransactionModel> q = new Queue<TransactionModel>();
        //private fields of LogHelper object to help log message to file
        LogHelper logHelper = new LogHelper();

        /**
         * PostingTransaction rest service adds a new transaction.
         * If a thread has not initially started, it will start the mining thread on demand.
         * Also, it enqueue the new transaction by using Queue.
         * This will run in the background if there are blocks to mine.
         * The thread will runs until the queue of current transactions is empty.
         */
        [Route("api/Miner/PostingTransaction/{sender}/{receiver}/{amount}")]
        [HttpPost]
        public void PostingTransaction(string sender, string receiver, string amount)
        {
            //initialize the model
            dataModel = new DataModel();
            //check if thread is not initially first start yet 
            if (isStart == false)
            {
                //creates the reserved bank account in the blockchain
                dataModel.ReservedBankAcct();

                //creates a mining thread
                Thread miningThread = new Thread(new ThreadStart(startMiningThread));
                //start the mining thread
                miningThread.Start();

                //set boolean to true
                isStart = true;
            }

            //creates a transaction model and set all its fields
            TransactionModel transactionModel = new TransactionModel();
            transactionModel.sender = uint.Parse(sender);
            transactionModel.receiver = uint.Parse(receiver);
            transactionModel.amount = float.Parse(amount);

            //enqueue new transaction to the queue
            q.Enqueue(transactionModel);
		}

        /**
         * startMiningThread method is the start of mining thread.
         * It validate all the transaction details,
         * Insert the transaction details into a block,
         * Pull down the last block from the current blockchain, and insert the hash of that block into the new block,
         * Brute force a valid hash,
         * Insert the now valid hash and hash offset into the block, and
         * Submit the block to the Bank Server for inclusion into the blockchain.
         */
        public void startMiningThread()
        {
            //infinite loop
            while (true)
            {
                try
                {
                    //check if the queue is greater than 0 and contains a new transaction
                    if (q.Count > 0)
                    {
                        //dequeue the new transaction
                        TransactionModel transactionModel = q.Dequeue();
                        //log message to file
                        logHelper.log($"[INFO] startMiningThread() - dequeue a transaction from {transactionModel.sender} to {transactionModel.receiver} with amount of {transactionModel.amount}.");

                        //validate the transaction details. check if the amount is greater than 0, sender id and receiver id is greater or equal than 0
                        if (transactionModel.amount > 0 && transactionModel.sender >= 0 && transactionModel.receiver >= 0)
                        {
                            //get the current coin balance of the transaction
                            float coinBal = dataModel.GetTransactedCurrCoinBalance(transactionModel.sender);
                            //log message to file
                            logHelper.log($"[INFO] startMiningThread() - all transaction details are validated. the current coin balance is {coinBal}.");

                            //check if its coin balance is enough for transaction
                            if (coinBal >= transactionModel.amount)
                            {
                                //log message to file
                                logHelper.log($"[INFO] startMiningThread() - the current coin balance is enough for transaction to occurs.");

                                //get the blockchain
                                List<BlockModel> blockList = dataModel.GetBlockList();
                                ////Pull down the last block from the current blockchain,
                                BlockModel prevBlock = blockList.Last();

                                //create new block and insert the transaction details
                                BlockModel blockModel = new BlockModel();
                                blockModel.blockId = prevBlock.blockId + 1;
                                blockModel.prevBlockHashStr = prevBlock.currBlockHashStr;
                                blockModel.walletIdFrom = transactionModel.sender;
                                blockModel.walletIdTo = transactionModel.receiver;
                                blockModel.amount = transactionModel.amount;

                                //brute force a valid hash
                                blockModel = newHashGenerator(blockModel);
                                //validate and submit new block to the blockchain if it is a correct block
                                dataModel.SubmitNewBlock(blockModel);
                            }
                        }
                    }
                }
                //if it contains a null value, catch a null reference exception and throw a http response exception
                catch (NullReferenceException)
                {
                    //create an error response
                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    httpResponseMessage.Content = new StringContent("[ERROR] startMiningThread() - null reference exception occured.");
                    //throw a http response exception
                    throw new HttpResponseException(httpResponseMessage);
                }
                //catch a json reader exception and throw a http response exception
                catch (JsonReaderException)
                {
                    //create an error response
                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    httpResponseMessage.Content = new StringContent("[ERROR] startMiningThread() - json reader exception occured. the inputted values is not valid.");
                    //throw a http response exception
                    throw new HttpResponseException(httpResponseMessage);
                }
                //if it contains an invalid operation, catch an invalid operation exception and throw a http response exception
                catch (InvalidOperationException)
                {
                    //create an error response
                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    httpResponseMessage.Content = new StringContent("[ERROR] startMiningThread() - invalid operation exception occured.");
                    //throw a http response exception
                    throw new HttpResponseException(httpResponseMessage);
                }
                //if the argument is out of range, catch an argument out of range exception and throw a http response exception
                catch (ArgumentOutOfRangeException)
                {
                    //create an error response
                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    httpResponseMessage.Content = new StringContent("[ERROR] startMiningThread() - argument out of range exception occured.");
                    //throw a http response exception
                    throw new HttpResponseException(httpResponseMessage);
                }
                //catch an exception and throw a http response exception
                catch (Exception)
                {
                    //create an error response
                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    httpResponseMessage.Content = new StringContent("[ERROR] startMiningThread() - an exception occured.");
                    //throw a http response exception
                    throw new HttpResponseException(httpResponseMessage);
                }
            }
        }

        /**
         * newHashGenerator method brute force a valid hash that starts with 12345 and ends with 54321.
         * It increments the hash offset to the next multiple of 5,
         * Concatenate all elements of the block (minus the hash you’re trying to create) into a string,
         * Create a SHA256 hash of that string, and 
         * Check to see if the hash is valid.
         */
        public BlockModel newHashGenerator(BlockModel blockModel)
        {
            //initialize the variables
            uint validBlockOffset = 0;
            string validHashStr = "";
            //using SHA256 for hashes for verification
            SHA256 sha256 = SHA256.Create();
            //loop until a valid offset that is a multiple of 5 and a hash that starts with 12345 and ends with 54321 is generated
            while (validHashStr.StartsWith("12345") == false)
            {
                //create block offset that is a multiple of 5
                validBlockOffset = validBlockOffset + 5;

                //create the new block fields string
                string newBlockStr = blockModel.blockId.ToString() + blockModel.walletIdFrom.ToString() + blockModel.walletIdTo.ToString() + blockModel.amount.ToString() + validBlockOffset + blockModel.prevBlockHashStr;
                //compute the sha256Hash
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(newBlockStr));
                //get the unsigned integer using bit converted from eight bytes in a byte array and make it as a string
                validHashStr = BitConverter.ToUInt64(hash, 0).ToString();
            }

            //set the valid block off set and its hash
            blockModel.blockOffset = validBlockOffset;
            blockModel.currBlockHashStr = validHashStr;

            //log message to file
            logHelper.log($"[INFO] newHashGenerator() - trying to generate a new block with block id:{blockModel.blockId}, from:{blockModel.walletIdFrom}, to:{blockModel.walletIdTo}, " +
                $"amount:{blockModel.amount}, offset:{blockModel.blockOffset}, curr hash:{blockModel.currBlockHashStr}, prev hash:{blockModel.prevBlockHashStr}.");

            //return the new block 
            return blockModel;
        }
	}
}