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
using ChummerHub.Models.V1;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ChummerHub.Controllers.V1
{
    public class Utils
    {
        public static void DbUpdateExceptionHandler(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, ILogger logger)
        {
            var proposedValues = entry.CurrentValues;
            var databaseValues = entry.GetDatabaseValues();
            var setvalues = databaseValues;
            string msg = string.Empty;
            //ResultBase res;
            try
            {
                bool throwexception = false;
                foreach (var property in proposedValues.Properties)
                {
                    object proposedValue = null;
                    object databaseValue = null;
                    if (proposedValues.Properties?.Contains(property) == true)
                        proposedValue = proposedValues[property];
                    if (databaseValues == null)
                        setvalues = databaseValues;
                    if (databaseValues?.Properties?.Contains(property) == true)
                    {
                        databaseValue = databaseValues[property];
                    }

                    msg += "\r\n" + "property: \"" + property + "\""+ "\r\n";
                    msg += $"\tproposedValue: \"{proposedValue}\"\r\n";
                    msg += $"\tdatabaseValue: \"{databaseValue}\"\r\n";
                    if ((databaseValues != null) && ((databaseValue == null)
                                                    || (String.IsNullOrEmpty(databaseValue.ToString()))))
                    {
                        setvalues = databaseValues;
                        setvalues[property] = proposedValue;
                        msg += $"\tSetting databaseValue to proposed Value: \"{proposedValue}\"\r\n";
                        logger.LogDebug(msg);
                    }
                    else if (databaseValues == null)
                    {
                        setvalues = proposedValues;
                        setvalues[property] = proposedValue;
                        msg += $"\tSetting databaseValue to proposed Values (since they are null): \"{proposedValue}\"\r\n";
                        logger.LogDebug(msg);
                    }
                    else
                    {
                        throwexception = true;
                        logger.LogError(msg);
                    }
                    
                    // TODO: decide which value should be written to database
                    // proposedValues[property] = <value to be saved>;
                    
                }
                //if (databaseValues != null)
                entry.OriginalValues.SetValues(setvalues);
                if (throwexception)
                {
                    var e = new NotSupportedException(
                        "(Codepoint 3) Don't know how to handle concurrency conflicts for "
                        + entry.Metadata.Name + ": " + msg);
                    throw e;
                }

            }
            catch (Exception exception)
            {
                var e = new NotSupportedException(
                    "(Codepoint 2) Don't know how to handle concurrency conflicts for "
                    + entry.Metadata.Name + ": " + msg, exception);
                throw e;
            }
            
        }
    }
}
