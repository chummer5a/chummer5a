using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chummer;
using NLog;

namespace NeonJungleLC
{
    public class NeonJungleFlagsCalculator
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public Character MyCharacter { get; private set; }
        public NeonJungleFlagsCalculator(Character character)
        {
            MyCharacter = character;
        }

        internal int? GetUsedFlags()
        {
            if (MyCharacter == null)
                throw new ArgumentNullException(nameof(MyCharacter));

            var tempFile2Check = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
            tempFile2Check = Path.Combine(tempFile2Check, "tempCalcChummerSave.chum5");

            MyCharacter.Save(tempFile2Check, false, true);

            //Do whatever you want with the tempFile2Check
            return new Random().Next(1, 20);
        }
    }
}
