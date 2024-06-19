using ModelStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace BlockchainWebApp.Controllers
{
    /**
     * BlockchainController is an ASP.NET Web controller class.
     * It is the primary repository of your blockchain
     * BlockchainController do logs message informations, details and errors to a file by using LogHelper class.
     * BlockchainController contains method of ReservedBankAcct, SubmitNewBlock, GetTransactedCurrCoinBalance and GetBlockList.
     */
    public class BlockchainController : ApiController
    {
        //private fields of LogHelper object to help log message to file
        private LogHelper logHelper = new LogHelper();

        /**
         * ReservedBankAcct rest service creates an initial start at the chain.
         * The bank server will need to initialise the blockchain with a transaction of 0 from ID 0 to ID 0.
         * ReservedBankAcct give us a start to the chain, like a bank that will contains infinite amount of money.
         */
        [Route("api/Blockchain/ReservedBankAcct")]
        [HttpPost]
        public void ReservedBankAcct()
        {
            try
            {
                //create an initial block for the bank
                BlockModel blockModel = new BlockModel();
                blockModel.blockId = 0;
                blockModel.walletIdFrom = 0;
                blockModel.walletIdTo = 0;
                blockModel.amount = 0;
                blockModel.prevBlockHashStr = "";//none

                //initialize the variables
                string validHashStr = "";
                uint validBlockOffset = 0;
                int initValue = 0;
                string initValueStr = initValue.ToString();

                //using SHA256 for hashes for verification
                SHA256 sha256 = SHA256.Create();
                //loop until a valid offset that is a multiple of 5 and a hash that starts with 12345 and ends with 54321 is generated
                while (validHashStr.StartsWith("12345") == false)
                {
                    //creates block offset that is a multiple of 5 
                    validBlockOffset = validBlockOffset + 5;

                    //create a string containing an initial value for bank which are block id, walletIdFrom, walletIdTo, amount, blockOffset and prevBlockHashStr
                    string initBlockStr = initValueStr + initValueStr + initValueStr + initValueStr + validBlockOffset + "";
                    //compute the sha256Hash
                    byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(initBlockStr));
                    //get the unsigned integer using bit converted from eight bytes in a byte array and make it as a string
                    validHashStr = BitConverter.ToUInt64(hash, 0).ToString();
                }

                //set the valid block offset and hash
                blockModel.blockOffset = validBlockOffset;
                blockModel.currBlockHashStr = validHashStr;
                //add the initial block for the bank to the blockchain
                Blocks.blockList.Add(blockModel);

                //log message to file
                logHelper.log($"[INFO] ReservedBankAcct() - creating a reserved bank account in the blockchain with block id:{blockModel.blockId}, from:{blockModel.walletIdFrom}, to:{blockModel.walletIdTo}, " +
                    $"amount:{blockModel.amount}, offset:{blockModel.blockOffset}, curr hash:{blockModel.currBlockHashStr}, prev hash:{blockModel.prevBlockHashStr}.");
            }
            //catch an exception and log error message to file and throw a http response exception
            catch (Exception)
            {
                //create an error response
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                httpResponseMessage.Content = new StringContent("[ERROR] ReservedBankAcct() - unable to create a reserved bank account in the blockchain.");
                //throw a http response exception
                throw new HttpResponseException(httpResponseMessage);
            }
        }

        /**
        * SubmitNewBlock rest service submits a correct new block to the blockhain.
        * This rest service validates a block before it is added to the blockchain.
        * A block is validated to not trust any given user.
        * It checks its block hash validation using SHA256.
        * If a block is correct do add new block to the blockchain, otherwise it will not be added and will be rejected.
        */
        [Route("api/Blockchain/SubmitNewBlock")]
        [HttpPost]
        public void SubmitNewBlock([FromBody] BlockModel blockModel)
        {
            try
            {
                //log message to file
                logHelper.log("[INFO] ValidateNewBlock() - validating the new block.");

                //set bool false to check if it is a correct block
                bool isNewBlockCreatedSuccesfully = false;
                //use boolean to check if the block ID is be a number higher than all other block IDs
                bool isNewBlockIdHigher = Blocks.blockList.Any(z => z.blockId < blockModel.blockId);

                //using SHA256 for hashes for verification
                SHA256 sha256 = SHA256.Create();
                //create the block fields string
                string blockValStr = blockModel.blockId.ToString() + blockModel.walletIdFrom.ToString() + blockModel.walletIdTo.ToString() + blockModel.amount.ToString() + blockModel.blockOffset + blockModel.prevBlockHashStr;
                //compute the sha256Hash
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockValStr));
                //get the unsigned integer using bit converted from eight bytes in a byte array and make it as a string
                string validHashStr = BitConverter.ToUInt64(hash, 0).ToString();

                //checks the block if it will be successful to create. if it is valid, set bool to true
                if (isNewBlockIdHigher == true//The block ID must be a number higher than all other block IDs
                    && (blockModel.walletIdFrom >= 0)//no sender number can be negative
                    && (blockModel.walletIdTo >= 0)//no receiver number can be negative
                    && (blockModel.amount > 0.0)//amount must be greater than 0
                    && (blockModel.blockOffset >= 0 && blockModel.blockOffset % 5 == 0)//the block offset must not be a negative number and must be divisible by 5
                    && (blockModel.currBlockHashStr.StartsWith("12345"))//block hash must start with 12345 and end with 54321
                    && (blockModel.currBlockHashStr == validHashStr)//the block hash must be valid. the hash generated must be equal to the one that the block has.
                    && (blockModel.prevBlockHashStr == Blocks.blockList.Last().currBlockHashStr)//the prev block hash must match the last block in the current chain
                    && (GetTransactedCurrCoinBalance(blockModel.walletIdFrom) >= 0)//no sender number can be negative
                    && (GetTransactedCurrCoinBalance(blockModel.walletIdFrom) >= blockModel.amount)//the sender must have at least as many coins as the transaction amount
                    )
                {
                    //set bool to true as its a correct block
                    isNewBlockCreatedSuccesfully = true;
                    //log message to file
                    logHelper.log("[INFO] ValidateNewBlock() - the new block is a correct block.");
                }
                else
                {
                    //log message to file
                    logHelper.log("[ERROR] ValidateNewBlock() - the new block is NOT a correct block.");
                }

                //if a block is correct, add new block to the blockchain and log message to file
                if (isNewBlockCreatedSuccesfully == true)
                {
                    //adds a block to the blockchain
                    Blocks.blockList.Add(blockModel);
                    //log message to file
                    logHelper.log($"[INFO] SubmitNewBlock() - A new block is added to the blockchain with block id:{blockModel.blockId}, from:{blockModel.walletIdFrom}, to:{blockModel.walletIdTo}, " +
                        $"amount:{blockModel.amount}, offset:{blockModel.blockOffset}, curr hash:{blockModel.currBlockHashStr}, prev hash:{blockModel.prevBlockHashStr}.");
                }
                //if a block is incorrect, log error message to file
                else
                {
                    //log message to file
                    logHelper.log($"[ERROR] SubmitNewBlock() - it is NOT a correct block. a new block is NOT submitted to the blockchain.");
                }
            }
            //catch an exception and log error message to file and throw a http response exception
            catch (Exception)
            {
                //create an error response
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                httpResponseMessage.Content = new StringContent("[ERROR] SubmitNewBlock() - unable to submit a new block to the blockchain.");
                //throw a http response exception
                throw new HttpResponseException(httpResponseMessage);
            }
        }

        /**
        * GetTransactedCurrCoinBalance rest service returns the balance of the inputted user id.
        * If the user id is 0 it returns an infinite amount as its the reserved bank account.
        * Otherwise, it loops the blockchain.
        * Then, if the inputted user id is equal to the sender or receiver id, calculate its balance.
        * Otherwise its a user that does not have any amount of money.
        */
        [Route("api/Blockchain/GetTransactedCurrCoinBalance/{user}")]
        [HttpGet]
        public float GetTransactedCurrCoinBalance(uint user)
        {
            //log message to file
            logHelper.log("[INFO] GetTransactedCurrCoinBalance() - retreiving the transacted current coin balance..");
            
            //create a variable for the balance
            float currCoinBal = 0;

            try
            {
                //check if user id is not 0
                if (user != 0)
                {
                    //loop through the blockchain
                    for (int i = 0; i < Blocks.blockList.Count; i++)
                    {
                        //if user id is equal to sender id, calculate its balance
                        if (Blocks.blockList[i].walletIdTo == user)
                        {
                            // add if they are recieving funds
                            currCoinBal = currCoinBal + Blocks.blockList[i].amount;
                        }
                        //if user id is equal to receiver id, calculate its balance
                        if (Blocks.blockList[i].walletIdFrom == user)
                        {
                            // subtract if the user is sending funds
                            currCoinBal = currCoinBal - Blocks.blockList[i].amount;
                        }
                    }

                    //log message to file
                    logHelper.log($"[INFO] GetTransactedCurrCoinBalance() - user {user} has {currCoinBal} coin.");
                }
                //if user id is 0, it is the reserved bank account. set the balance to infinite float
                else
                {
                    //log message to file
                    logHelper.log($"[INFO] GetTransactedCurrCoinBalance() - user {user} is a reserved bank account. it has infinite amount of coin.");
                    //set the balance to infinite float
                    currCoinBal = float.MaxValue;
                }
            }
            //catch an exception and log error message to file and throw a http response exception
            catch (Exception)
            {
                //create an error response
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                httpResponseMessage.Content = new StringContent("[ERROR] GetTransactedCurrCoinBalance() - unable to retrieve the coin balance.");
                //throw a http response exception
                throw new HttpResponseException(httpResponseMessage);
            }

            //return the balance
            return currCoinBal;
        }

        /**
        * GetBlockList rest service retrives a block from the blockhain.
        * It returns the blockchain.
        */
        [Route("api/Blockchain/GetBlockList")]
        [HttpGet]
        public List<BlockModel> GetBlockList()
        {
            try
            {
                //return the blockchain
                return Blocks.blockList;
            }
            //catch an exception and log error message to file and throw a http response exception
            catch (Exception)
            {
                //create an error response
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                httpResponseMessage.Content = new StringContent("[ERROR] GetBlockList() - unable to retrieve the blockchain.");
                //throw a http response exception
                throw new HttpResponseException(httpResponseMessage);
            }
        }
    }
}