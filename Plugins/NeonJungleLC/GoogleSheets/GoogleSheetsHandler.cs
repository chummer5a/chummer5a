using Chummer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using NeonJungleLC.GoogleSheets;
using NLog;

namespace NeonJungleLC.GoogleSheets
{

    /// <summary>
    /// This example uses a service Account to access sheets. The Owner of the sheet must register a google developer account (free)
    /// https://developers.google.com/?authuser=0&pli=1
    /// enable the google sheets api and autorize it
    /// https://developers.google.com/sheets/api/guides/authorizing
    /// we need an api key. you can just download the json (will look like the neonjunglelc.json, that is included in the project
    /// This is all (of course) just a suggestion. Use it - or don't... ;)
    /// </summary>
    public class GoogleSheetsHandler
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
      
        private SheetsService MySheetsService { get; set; }


        //as example, THIS sheet will be red here: https://docs.google.com/spreadsheets/d/1JVtVNq8e9tY-ZiLeqGoccS3Pmu-sT-orernUrh-uLrw/edit?usp=sharing
        internal int? GetLockedFlags(Character char2check)
        {
            if (char2check == null)
                throw new ArgumentNullException(nameof(char2check));
            ServiceAccountCredential credential;

            string serviceAccountEmail = "chummer@neonjunglelc.iam.gserviceaccount.com";
            string privatekey =
                "-----BEGIN PRIVATE KEY-----\nMIIEvwIBADANBgkqhkiG9w0BAQEFAASCBKkwggSlAgEAAoIBAQDZRimJQ8Mu0g68\noVoiWla/oIYqRUQwuwNf1z5BB7XujD6B3uU9q+op+aF25aXhy4oOXXXLAMZIkRiF\nsevJOlpO8YaXkUsrLvF/JWIX8FpEJqmkPs4HWlqn11/Swm4S744Rv/G/aF72VcF/\ni+qy7qZlU0I3dgbqJKmTJ5Xn2/vVVSAjdBCRHqd7xTDNlrwT47AxngLPxuvuqI62\nBCmA+/o/04+m1DNQj6p4M3ZnvgFNqSrNrskS1E4TCuSQuSEF4Vq3n0HW4h+Sz7lj\nvBFCw1o0iweaXj+odtXC1Am+m9AWALOOsP1ka2F2VZK7leKvLl89oN1noKCFtbog\nxwn7csfPAgMBAAECggEAAODIe38DdfKfSyimn6tLD4YplHuyWLFqXtkKoTTi8uuC\npgeN3CXOKqczT9O3Tkwtcpe2DzRCfP+Y9K78UexcVApf2yYFfFIP6MZOZWqlzQlh\nOGKBisPjDLcR96aD1rsyfiQHeFCYb+hbEhwxW3j9SlP5VYrG9jzmn3K1fIS/Aw/Z\nBOlyotEOq+sBpoSmFYz0bFbpBAd3gALUxAYu5y7NNzDnUB7HNbkJpSURaYwhUPJR\nKvmqsnBATYPNSmqiAskn5emGFKhQ2ZrqaQ6db/NlTL2/ZDtpvc2t93wxiXf+be52\n/KK8AfNgxxCI4KdiDQMmaWdAbzetWiIV/BU+6DezsQKBgQD3wowRzWw/Tk+Sbxqd\n0mm3waVaaJhXmZPudAa5gHrl+eQCEKQzyzKhJeQt4LXtUei1pTlCzQm44yEJTtup\ntE3z3aqjmXDMfioUeKcMGBeVYMEJXtqik9DaSeZq6TBF00X5pzkFsTIUk5Vt2wB3\nbpA+rinNxQD8pJTG+kNTnhF+JQKBgQDggA4oT576CkQA2HGo3iFDCfeAQfdk2kNm\neErONdO+TXUfsH5kogG3kdXzfHL23THi+rr028uR2m2MDfkwZFqVXP4214Bnj3xO\nOG6KGn8JC2mW89dW2NcDDi5+bjDvggD7i0WUz3mnZJepOzWccYIvJXKRGvcFBuZO\nIf6rPwwp4wKBgQCFjlnzPskbVxuN1FaEvOhAJiL8mWWF00PrRqBZXujhD9PZSR86\nzE5+j+5wzLFFPOI1CNvVJrIW+FjWq6u9z3Q2AUf66LvlgB0u69sgqdwMqhtk8bzp\n5sSu5ydOemWLPlh6O6qBZwOYE/Z5QZT+zJr1Lu1Z/tiJWC7bFA03Bf5oAQKBgQCS\nMhIGIkOO5NyACHOL2ouikn0AovSuUoyN0Ew2mUr4pIxfRJoqgm3H00qWszZSmJ0C\nCPFyvyeEJdAs4nSiFNgAaHyLzi6qQgBbF9i0SqjrhOkQCl7zCWaLcNLKNovbjLeD\nF6EKVUoNvi4dYJd691glx6ch44N1XJbhzrV/YyFSrQKBgQDB7VDh0yGMcVK+UY6f\n44WZd0qb4pippOEAerojC9YjdmEs0/lfE4ert6LWIqSMriPu2qzskOPOYVALNLHw\n5vTluydZawLAl4/ZKBN8uSrRamn/RPlnaZtiM5bjYF/lFyUVKtDdI52NnLhpeYUj\ntqcwl6hu0vJLasyCtdsWo7W/gA==\n-----END PRIVATE KEY-----\n";
            credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(serviceAccountEmail)
                {
                    Scopes = Scopes,
                    HttpClientFactory = new ProxySupportedHttpClientFactory()
                }.FromPrivateKey(privatekey));


            // Create Google Sheets API service.
            MySheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Chummer",
                HttpClientFactory = new ProxySupportedHttpClientFactory()
            });
      
   
            // Define request parameters. You get the Id when you open the sheet in the browser.
            // It is shown in the title bar.
            String spreadsheetId = "1JVtVNq8e9tY-ZiLeqGoccS3Pmu-sT-orernUrh-uLrw";
            String range = "TestSheetName!A1:E";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                MySheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
            
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            int returnvalue = 0;
            if (values != null && values.Count > 0)
            {
                Console.WriteLine("Text, Value");
                foreach (var row in values)
                {
                    // Print columns A and E, which correspond to indices 0 and 4.
                    Console.WriteLine("{0}, {1}", row[0], row[2]);
                }

                string stringvalue = values[0].ToArray()[1].ToString();
                int temp;
                if (Int32.TryParse(stringvalue, out temp))
                    returnvalue = temp;
            }
            else
            {
                Console.WriteLine("No data found.");
            }
            Console.Read();

            return returnvalue;
        }
    }
}
