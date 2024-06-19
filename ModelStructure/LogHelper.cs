using System;
using System.IO;
using System.Security;

namespace ModelStructure
{
    /**
     * LogHelper is a class that contains a log method.
     * The log method is used to help log an information, details and error message for the bank business-tier.
     */
    public class LogHelper
    {
        /**
         * log method logs an information, details or error message to a file.
         * It receive a message in string and save the message to a file in this tutorial main parent Document.
         */
        public void log(string message)
        {
            try
            {
                //set docPath as the documents path 
                string docPath = new FileInfo(AppDomain.CurrentDomain.BaseDirectory).Directory.Parent.FullName;
                //set txtFileName as the local text file path for the log messages
                string txtFileName = "Log-Cryptocurrency-Blockchain-App.txt";
                //set the pathFileName as the document and local text file path for the log messages
                string pathFileName = Path.Combine(docPath, txtFileName);

                //Append text to an existing file in document path
                using (StreamWriter outputFile = new StreamWriter(pathFileName, true))
                {
                    //write the date along with its message to the file
                    outputFile.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + message);
                }
            }
            //catch argument null exception if path is a null reference
            catch (ArgumentNullException)
            {
                //write error message to console
                Console.WriteLine("Argument null exception occured. The path file is null.");
            }
            //catch argument exception if the path is not valid, such as null or empty
            catch (ArgumentException)
            {
                //write error message to console
                Console.WriteLine("Argument exception occured. It has an invalid path.");
            }
            //catch security exception if it does not have the permission needed
            catch (SecurityException)
            {
                //write error message to console
                Console.WriteLine("Security exception occured. It needed a permission to access it.");
            }
            //catch directory not found exception if the directory is not found or does not exist
            catch (DirectoryNotFoundException)
            {
                //write error message to console
                Console.WriteLine("Directory not found exception occured. Cannot found the directory.");
            }
        }
    }
}