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
        public void LoadList(Improvement.ImprovementSource objSource)
        {
            string strXmlFile = "cyberware.xml";
            if (objSource == Improvement.ImprovementSource.Bioware)
                strXmlFile = "bioware.xml";
            XmlDocument objXMlDocument = XmlManager.Instance.Load(strXmlFile);
            
            foreach (XmlNode objNode in objXMlDocument.SelectNodes("/chummer/grades/grade"))
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
            Grade objReturn = new Grade();
            foreach (Grade objGrade in _lstGrades)
            {
                if (objGrade.Name == strGrade)
                {
                    objReturn = objGrade;
                    break;
                }
            }

            return objReturn;
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