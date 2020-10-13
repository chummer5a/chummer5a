using Chummer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeonJungleLC
{
    public class GoogleSheetsHandler
    {
        public GoogleSheetsHandler()
        {
            //to get the credentials: open this while logged on with your google account and follow the steps accordingly
            //https://developers.google.com/sheets/api/quickstart/dotnet
        }

        internal int? GetUsedFlags(Character char2check)
        {
            if (char2check == null)
                throw new ArgumentNullException(nameof(char2check));

            var tempFile2Check = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
            tempFile2Check = Path.Combine(tempFile2Check, "tempCalcChummerSave.chum5");

            char2check.Save(tempFile2Check, false, true);

            //Do whatever you want with the tempFile2Check
            return new Random().Next(1, 20);
        }

        internal int? GetLockedFlags(Character char2check)
        {
            if (char2check == null)
                throw new ArgumentNullException(nameof(char2check));

                       

            //Do whatever you want with the tempFile2Check
            return new Random().Next(1, 20);
        }
    }
}
