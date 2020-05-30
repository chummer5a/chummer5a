using ChummerHub.Models.V1;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ChummerHub.Controllers.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Utils'
    public class Utils
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Utils'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Utils.DbUpdateConcurrencyExceptionHandler(EntityEntry, ILogger)'
        public static ResultBase DbUpdateExceptionHandler(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, ILogger logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Utils.DbUpdateConcurrencyExceptionHandler(EntityEntry, ILogger)'
        {
            var proposedValues = entry.CurrentValues;
            var databaseValues = entry.GetDatabaseValues();
            string msg = string.Empty;
#pragma warning disable CS0168 // The variable 'res' is declared but never used
            ResultBase res;
#pragma warning restore CS0168 // The variable 'res' is declared but never used
            try
            {

                foreach (var property in proposedValues.Properties)
                {
                    object proposedValue = null;
                    object databaseValue = null;
                    if (proposedValues.Properties?.Contains(property) == true)
                        proposedValue = proposedValues[property];
                    if (databaseValues?.Properties?.Contains(property) == true)
                        databaseValue = databaseValues[property];

                    msg += Environment.NewLine + "property: " + property + Environment.NewLine;
                    msg += "\tproposedValue: " + proposedValue + Environment.NewLine;
                    msg += "\tdatabaseValue: " + databaseValue + Environment.NewLine;
                    logger.LogError(msg);
                    // TODO: decide which value should be written to database
                    // proposedValues[property] = <value to be saved>;
                }
                entry.OriginalValues.SetValues(databaseValues);
                var e = new NotSupportedException(
                    "(Codepoint 3) Don't know how to handle concurrency conflicts for "
                    + entry.Metadata.Name + ": " + msg);
                throw e;

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
