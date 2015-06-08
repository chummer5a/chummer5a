using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using Chummer;

namespace Chummer
{
    internal static class XMLExtensions
    {
        private static CommonFunctions _objCommonFunctions = new CommonFunctions();
        
        /// <summary>
        /// This method is syntaxtic sugar for atempting to read a data field
        /// from an XmlNode. This version sets the output variable to its 
        /// default value in case of a failed read and can be used for 
        /// initializing variables
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="node">The XmlNode to read from</param>
        /// <param name="field">The field to try and extract from the XmlNode</param>
        /// <param name="read">The variable to save the read to</param>
        /// <returns>true if successful read</returns>
        public static bool TryGetField<T>(this XmlNode node, String field, out T read) where T : IConvertible
        {
            /*
             * This extension method allows easier access of xml, instead of
             * the old TryCatch blocks, not even logging the error
             * 
             * It works because most of the types we read from the XmlNode is 
             * IConvertible that can be converted to or from string with just
             * a type argument, first known at runtime (not true, but generics)
             * 
             * because it is now a generic method, instead of 
             * try{convert();}
             * catch{noop();}
             * 
             * We can do some acctual error checking instead of relying on exceptions
             * in case anything happens. We could do that before, but typing 10
             * lines to read a single variable 100 times would be insane
             * 
             * That means this should be an order of magnitude faster in case of 
             * missing fields and a little bit slower in case of fields being there 
             * 
             * To use this method, call it like this
             * 
             * aXmlNode.TryGetField("fieldname", out myVariable);
             * 
             * The compiler will fill out <T> itself, unless you specificaly 
             * tell it to be something else
             * 
             * in case you need to act on weither the read was successfull
             * do it like this
             * if(aXmlNode.TryGetField("fieldname", out myVariable))
             * {
             *     success();
             * }
             * else
             * {
             *     failure();
             * }
             */
            if (node[field] == null)
            {
#if DEBUG
                //Extra magic in debug builds, but can provide errors in release
                //builds due to inlining
                System.Reflection.MethodBase mth 
                    = new StackTrace().GetFrame(1).GetMethod();
                String errorMsg = String.Format
                (
                    "Tried to read missing field \"{0}\" of type \"{1}\" in {1}.{2}",
                    field,
                    typeof (T),
                    mth.ReflectedType.Name,
                    mth
                );
#else           //So if DEBUG flag is missing we don't reflect info
                String errorMsg = String.Format("Tried to read missing field \"{0}\" of type \"{1}\"", field, typeof(T));
#endif
                _objCommonFunctions.LogWrite(CommonFunctions.LogType.Error, "Chummer.XmlExtensions", errorMsg);
                //Assign something
                read = default(T);
                return false;
            }

            String fieldValue = node[field].InnerText;
            try
            {
                read = (T) Convert.ChangeType(fieldValue, typeof (T));
                return true;
            }
            catch (Exception)
            {
                //If we are debugging, great
                if (Debugger.IsAttached)
                    Debugger.Break();

                //Otherwise just log it
#if DEBUG
                System.Reflection.MethodBase mth 
                    = new StackTrace().GetFrame(1).GetMethod();
                String errorMsg = String.Format("Tried to read missing field \"{0}\" in {1}.{2}", field, mth.ReflectedType.Name, mth);
#else
                String errorMsg = String.Format("Tried to read missing field \"{0}\"", field);
#endif
                _objCommonFunctions.LogWrite(CommonFunctions.LogType.Error, "Chummer.XmlExtensions", errorMsg);
                
                //Finaly, we have to assign an out parameter something, so default
                //null or 0 most likeley
                read = default(T);
                return false;
            }

        }

        /// <summary>
        /// This method is syntaxtic sugar for atempting to read a data field
        /// from an XmlNode. This version preserves the output variable in case
        /// of a failed read 
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="node">The XmlNode to read from</param>
        /// <param name="field">The field to try and extract from the XmlNode</param>
        /// <param name="read">The variable to save the read to, if successful</param>
        /// <returns>true if successful read</returns>
        public static bool TryPreserveField<T>(this XmlNode node, String field, ref T read) where T : IConvertible
        {
            T value;
            if (node.TryGetField(field, out value))
            {
                read = value;
                return true;
            }

            return false;

        }
    }
}