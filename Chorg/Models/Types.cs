namespace Chorg.Models
{
    public enum ContentType { UNDEFINED, GENERAL, TAXI, SID, STAR, APP }

    public interface ITextSearch
    {
        /// <summary>
        /// Check whether the object identify itself with the given string. Returns so.
        /// </summary>
        /// <param name="s">Search string</param>
        /// <returns>Applies?</returns>
        bool SearchPredicate(string s);
    }
}
