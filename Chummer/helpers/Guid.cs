namespace Chummer.helpers
{
    public class Guid
    {
        /// <summary>
        /// Tests whether a given string is a Guid. Returns false if not. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsGuid(string value)
        {
            System.Guid x;
            return System.Guid.TryParse(value, out x);
        }
    }
}