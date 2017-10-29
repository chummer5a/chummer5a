using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// List of Grades for either Cyberware or Bioware.
    /// </summary>
    public class GradeList : IEnumerable<Grade>
    {
        private List<Grade> _lstGrades = new List<Grade>();

        #region Methods
        /// <summary>
        /// Fill the list of CyberwareGrades from the XML files.
        /// </summary>
        /// <param name="objSource">Source to load the Grades from, either Bioware or Cyberware.</param>
        public void LoadList(Improvement.ImprovementSource objSource, CharacterOptions objCharacterOptions = null)
        {
            _lstGrades.Clear();
            string strXmlFile = "cyberware.xml";
            if (objSource == Improvement.ImprovementSource.Bioware)
                strXmlFile = "bioware.xml";
            XmlDocument objXMlDocument = XmlManager.Load(strXmlFile);

            string strBookFilter = string.Empty;
            if (objCharacterOptions != null)
                strBookFilter = "[(" + objCharacterOptions.BookXPath() + ")]";
            foreach (XmlNode objNode in objXMlDocument.SelectNodes("/chummer/grades/grade" + strBookFilter))
            {
                Grade objGrade = new Grade();
                objGrade.Load(objNode);
                _lstGrades.Add(objGrade);
            }
        }

        /// <summary>
        /// Retrieve the Standard Grade from the list.
        /// </summary>
        public Grade GetGrade(string strGrade)
        {
            foreach (Grade objGrade in _lstGrades)
            {
                if (objGrade.Name == strGrade)
                {
                    return objGrade;
                }
            }

            return null;
        }
        #endregion

        #region Enumeration Methods
        public IEnumerator<Grade> GetEnumerator()
        {
            return _lstGrades.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
