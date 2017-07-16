using System;
using System.Xml;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Grade of Cyberware or Bioware.
    /// </summary>
    public class Grade
    {
        private string _strName = "Standard";
        private string _strAltName = string.Empty;
        private decimal _decEss = 1.0m;
        private double _dblCost = 1.0;
        private int _intAvail = 0;
        private string _strSource = "SR5";

        #region Constructor and Load Methods
        public Grade()
        {
        }

        /// <summary>
        /// Load the Grade from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("translate", ref _strAltName);
            objNode.TryGetDecFieldQuickly("ess", ref _decEss);
            objNode.TryGetDoubleFieldQuickly("cost", ref _dblCost);
            objNode.TryGetInt32FieldQuickly("avail", ref _intAvail);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
        }
        #endregion

        #region Properties
        /// <summary>
        /// The English name of the Grade.
        /// </summary>
        public string Name
        {
            get
            {
                return _strName;
            }
        }

        /// <summary>
        /// The name of the Grade as it should be displayed in lists.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(_strAltName))
                    return _strAltName;

                return _strName;
            }
        }

        /// <summary>
        /// The Grade's Essence cost multiplier.
        /// </summary>
        public decimal Essence
        {
            get
            {
                return _decEss;
            }
        }

        /// <summary>
        /// The Grade's cost multiplier.
        /// </summary>
        public double Cost
        {
            get
            {
                return _dblCost;
            }
        }

        /// <summary>
        /// The Grade's Availability modifier.
        /// </summary>
        public int Avail
        {
            get
            {
                return _intAvail;
            }
        }

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get
            {
                return _strSource;
            }
        }

        /// <summary>
        /// Whether or not the Grade is for Adapsin.
        /// </summary>
        public bool Adapsin
        {
            get
            {
                return _strName.Contains("(Adapsin)");
            }
        }

        /// <summary>
        /// Whether or not the Grade is for the Burnout's Way.
        /// </summary>
        public bool Burnout
        {
            get
            {
                return _strName.Contains("(Burnout's Way)");
            }
        }

        /// <summary>
        /// Whether or not this is a Second-Hand Grade.
        /// </summary>
        public bool SecondHand
        {
            get
            {
                return _strName.Contains("Used");
            }
        }
        #endregion
    }
}