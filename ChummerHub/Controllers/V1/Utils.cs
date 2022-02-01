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
