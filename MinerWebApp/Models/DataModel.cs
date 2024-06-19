using Newtonsoft.Json;
using RestSharp;
using ModelStructure;
using System.Collections.Generic;

namespace MinerWebApp.Models
{
    /**
     * DataModel is a model class of the miner web application.
     * It contains method of ReservedBankAcct, SubmitNewBlock, GetTransactedCurrCoinBalance and GetBlockList.
     * It is a class that helps call or connect with the blockchain server.
     */
    public class DataModel
    {
        //private fields
        RestClient client;
        string URL;
        //private fields of LogHelper object to help log message to file
        LogHelper logHelper = new LogHelper();

        //default constructor
        public DataModel()
        {
            //set the base url
            URL = "https://localhost:44347/";
            //use RestClient and set the URL
            client = new RestClient(URL);
        }

        /**
         * ReservedBankAcct method request the blockchain server ReservedBankAcct rest service.
         * It creates a block at the start of the chain for the reserved bank. 
         */
        public void ReservedBankAcct()
        {
            //set up and call the API method
            RestRequest request = new RestRequest("api/Blockchain/ReservedBankAcct");
            //use IRestResponse and set the request in the client post method
            IRestResponse resp = client.Post(request);

            //check if response is succesful
            if (resp.IsSuccessful)
            {
                //log message to file
                logHelper.log("[INFO] ReservedBankAcct() - Succesfully called ReservedBankAcct method from the blockchain web app.");
            }
            //if response is not succesful, log the error message to file
            else
            {
                //log error message to file
                logHelper.log(resp.Content);
            }
        }

        /**
         * SubmitNewBlock method request the blockchain server SubmitNewBlock rest service.
         * It submits a new block to the blockchain.
         */
        public void SubmitNewBlock(BlockModel blockModel)
        {
            //set up and call the API method
            RestRequest request = new RestRequest("api/Blockchain/SubmitNewBlock/");
            //add json body to the request
            request.AddJsonBody(blockModel);
            //use IRestResponse and set the request in the client post method
            IRestResponse resp = client.Post(request);

            //check if response is succesful
            if (resp.IsSuccessful)
            {
                //log message to file
                logHelper.log("[INFO] SubmitNewBlock() - Succesfully called SubmitNewBlock method from the blockchain web app.");
            }
            //if response is not succesful, log the error message to file
            else
            {
                //log error message to file
                logHelper.log(resp.Content);
            }
        }

        /**
         * GetTransactedCurrCoinBalance method request the blockchain server GetTransactedCurrCoinBalance rest service.
         * It retrieves the coin balance of the inputted user id and return it.
         */
        public float GetTransactedCurrCoinBalance(uint user)
        {
            //set up and call the API method
            RestRequest request = new RestRequest("api/Blockchain/GetTransactedCurrCoinBalance/" + user.ToString());
            //set the request in the client get method
            IRestResponse resp = client.Get(request);

            //initialize the balance
            float bal = 0;
            //check if response is succesful
            if (resp.IsSuccessful)
            {
                //deserialize the object using json convert
                bal = JsonConvert.DeserializeObject<float>(resp.Content);
            }
            //if response is not succesful, log the error message to file
            else
            {
                //log error message to file
                logHelper.log(resp.Content);
            }

            //return the coin balance
            return bal;
        }

        /**
         * GetBlockList method request the blockchain server GetBlockList rest service.
         * It retrieves the blockchain.
         */
        public List<BlockModel> GetBlockList()
        {
            //set up and call the API method
            RestRequest request = new RestRequest("api/Blockchain/GetBlockList");
            //set the request in the client get method
            IRestResponse resp = client.Get(request);

            //initialize the list
            List<BlockModel> blockList = null;
            //check if response is succesful
            if (resp.IsSuccessful)
            {
                //deserialize the object using json convert
                blockList = JsonConvert.DeserializeObject<List<BlockModel>>(resp.Content);
            }
            //if response is not succesful, log the error message to file
            else
            {
                //log error message to file
                logHelper.log(resp.Content);
            }

            //return the blockchain
            return blockList;
        }
    }
}