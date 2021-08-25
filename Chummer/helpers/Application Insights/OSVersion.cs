/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using NLog;

namespace Chummer.helpers.Application_Insights
{
    public static class OsVersion
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        public static string GetOsInfo()
        {
            try
            {
                //Get Operating system information.
                OperatingSystem os = Environment.OSVersion;
                //Get version information about the os.
                Version vs = os.Version;

                //Variable to hold our return value
                string operatingSystem = string.Empty;

                switch (os.Platform)
                {
                    case PlatformID.Win32Windows:
                        //This is a pre-NT version of Windows
                        switch (vs.Minor)
                        {
                            case 0:
                                operatingSystem = "95";
                                break;

                            case 10:
                                operatingSystem = vs.Revision.ToString() == "2222A" ? "98SE" : "98";
                                break;

                            case 90:
                                operatingSystem = "Me";
                                break;
                        }

                        break;

                    case PlatformID.Win32NT:
                        switch (vs.Major)
                        {
                            case 3:
                                operatingSystem = "NT 3.51";
                                break;

                            case 4:
                                operatingSystem = "NT 4.0";
                                break;

                            case 5:
                                operatingSystem = vs.Minor == 0 ? "2000" : "XP";
                                break;

                            case 6:
                                switch (vs.Minor)
                                {
                                    case 0:
                                        operatingSystem = "Vista";
                                        break;

                                    case 1:
                                        operatingSystem = "7";
                                        break;

                                    case 2:
                                        operatingSystem = "8";
                                        break;

                                    default:
                                        operatingSystem = "8.1";
                                        break;
                                }
                                break;

                            case 10:
                                operatingSystem = "10";
                                break;
                        }

                        break;
                }
                //Make sure we actually got something in our OS check
                //We don't want to just return " Service Pack 2" or " 32-bit"
                //That information is useless without the OS version.
                if (!string.IsNullOrEmpty(operatingSystem))
                {
                    //Got something.  Let's prepend "Windows" and get more info.
                    operatingSystem = "Windows " + operatingSystem;
                    //See if there's a service pack installed.
                    if (!string.IsNullOrEmpty(os.ServicePack))
                    {
                        //Append it to the OS name.  i.e. "Windows XP Service Pack 3"
                        operatingSystem += " " + os.ServicePack;
                    }
                    //Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
                    //operatingSystem += " " + getOSArchitecture().ToString() + "-bit";
                }
                //Return the information we've gathered.
                return operatingSystem;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return "unknown";
        }
    }
}
