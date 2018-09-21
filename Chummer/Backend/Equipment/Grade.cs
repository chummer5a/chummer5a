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
using System.Diagnostics;
using System.Xml;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Grade of Cyberware or Bioware.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class Grade : IHasName, IHasInternalId, IHasXmlNode
    {
        private Guid _guidSourceId = Guid.Empty;
        private Guid _guidId;
        private string _strName = "Standard";
        private decimal _decEss = 1.0m;
        private decimal _decCost = 1.0m;
        private int _intAvail;
        private string _strSource = "SR5";
        private int _intDeviceRating = 2;
	    private int _intAddictionThreshold;
		private readonly Improvement.ImprovementSource _eSource;

        #region Constructor and Load Methods
        public Grade(Improvement.ImprovementSource eSource)
        {
            _guidId = Guid.NewGuid();
            _eSource = eSource;
        }

        /// <summary>
        /// Load the Grade from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (!objNode.TryGetField("id", Guid.TryParse, out _guidSourceId))
            {
                XmlNode xmlDataNode = XmlManager.Load(_eSource == Improvement.ImprovementSource.Bioware ? "bioware.xml" : "cyberware.xml", GlobalOptions.Language).SelectSingleNode("/chummer/grades/grade[name = " + Name.CleanXPath() + "]");
                if (xmlDataNode?.TryGetField("id", Guid.TryParse, out _guidSourceId) != true)
                    _guidSourceId = Guid.NewGuid();
            }
            objNode.TryGetDecFieldQuickly("ess", ref _decEss);
            objNode.TryGetDecFieldQuickly("cost", ref _decCost);
            objNode.TryGetInt32FieldQuickly("avail", ref _intAvail);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetField("addictionthreshold", out _intAddictionThreshold);
			if (!objNode.TryGetInt32FieldQuickly("devicerating", ref _intDeviceRating))
            {
                if (Name.Contains("Alphaware"))
                    _intDeviceRating = 3;
                else if (Name.Contains("Betaware"))
                    _intDeviceRating = 4;
                else if (Name.Contains("Deltaware"))
                    _intDeviceRating = 5;
                else if (Name.Contains("Gammaware"))
                    _intDeviceRating = 6;
                else
                    _intDeviceRating = 2;
            }
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = XmlManager.Load(_eSource == Improvement.ImprovementSource.Bioware ? "bioware.xml" : "cyberware.xml", strLanguage).SelectSingleNode("/chummer/grades/grade[id = \"" + SourceId.ToString("D") + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this grade.
        /// </summary>
        public string InternalId => _guidId == Guid.Empty ? string.Empty : _guidId.ToString("D");

        /// <summary>
        /// Identifier of the grade within data files.
        /// </summary>
        public Guid SourceId => _guidSourceId;

        /// <summary>
        /// The English name of the Grade.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// The name of the Grade as it should be displayed in lists.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The Grade's Essence cost multiplier.
        /// </summary>
        public decimal Essence => _decEss;

        /// <summary>
        /// Device rating of the grade.
        /// </summary>
        public int DeviceRating => _intDeviceRating;

        /// <summary>
        /// The Grade's cost multiplier.
        /// </summary>
        public decimal Cost => _decCost;

        /// <summary>
        /// The Grade's Availability modifier.
        /// </summary>
        public int Avail => _intAvail;

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source => _strSource;

        /// <summary>
        /// Whether or not the Grade is for Adapsin.
        /// </summary>
        public bool Adapsin => _strName.Contains("(Adapsin)");

        /// <summary>
        /// Whether or not the Grade is for the Burnout's Way.
        /// </summary>
        public bool Burnout => _strName.Contains("Burnout's Way");

        /// <summary>
        /// Whether or not this is a Second-Hand Grade.
        /// </summary>
        public bool SecondHand => _strName.Contains("Used");

        /// <summary>
        /// The Grade's Addiction Threshold Modifier. Used for Drugs.
        /// </summary>
        public int AddictionThreshold
        {
            get { return _intAddictionThreshold; }
            set { _intAddictionThreshold = value; }
        }
		#endregion
	}
}
