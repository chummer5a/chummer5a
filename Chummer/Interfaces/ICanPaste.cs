
namespace Chummer
{
    public interface ICanPaste
    {
        /// <summary>
        /// Does this object allow the current clipboard content to be pasted into it?
        /// </summary>
        bool AllowPasteXml { get; }

        bool AllowPasteObject(object input);
    }
}
