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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using NLog;

namespace Chummer.Backend.Equipment
{
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class LifestyleQuality : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, IHasSource
    {
        private Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strExtra = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private bool _blnContributeToLP = true;
        private bool _blnPrint = true;
        private int _intLP;
        private string _strCost = string.Empty;
        private int _intMultiplier;
        private int _intBaseMultiplier;
        private List<string> _lstAllowedFreeLifestyles = new List<string>();
        private Lifestyle _objParentLifestyle;
        private QualityType _objLifestyleQualityType = QualityType.Positive;
        private QualitySource _objLifestyleQualitySource = QualitySource.Selected;
        private XmlNode _nodBonus;
        private readonly Character _objCharacter;
        private bool _blnFree;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a LifestyleQualityType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static QualityType ConvertToLifestyleQualityType(string strValue)
        {
            switch (strValue)
            {
                case "Negative":
                    return QualityType.Negative;
                case "Positive":
                    return QualityType.Positive;
                case "Contracts":
                    return QualityType.Contracts;
                default:
                    return QualityType.Entertainment;
            }
        }

        /// <summary>
        /// Convert a string to a LifestyleQualitySource.
        /// </summary>
#if DEBUG
        /// <param name="strValue">String value to convert.</param>
        public static QualitySource ConvertToLifestyleQualitySource(string strValue)
        {
            switch (strValue)
            {
                default:
                    return QualitySource.Selected;
            }
#else
        public QualitySource ConvertToLifestyleQualitySource()
        {
            return QualitySource.Selected;
#endif
        }
    #endregion

        #region Constructor, Create, Save, Load, and Print Methods
    public LifestyleQuality(Character objCharacter)
        {
            // Create the GUID for the new LifestyleQuality.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Create a LifestyleQuality from an XmlNode.
        /// </summary>
        /// <param name="objXmlLifestyleQuality">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character object the LifestyleQuality will be added to.</param>
        /// <param name="objParentLifestyle">Lifestyle object to which the LifestyleQuality will be added.</param>
        /// <param name="objLifestyleQualitySource">Source of the LifestyleQuality.</param>
        /// <param name="strExtra">Forced value for the LifestyleQuality's Extra string (also used by its bonus node).</param>
        public void Create(XmlNode objXmlLifestyleQuality, Lifestyle objParentLifestyle, Character objCharacter, QualitySource objLifestyleQualitySource, string strExtra = "")
        {
            _objParentLifestyle = objParentLifestyle;
            if (!objXmlLifestyleQuality.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlLifestyleQuality });
                Utils.BreakIfDebug();
            }
            else
                _objCachedMyXmlNode = null;
            if (objXmlLifestyleQuality.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("lp", ref _intLP);
            objXmlLifestyleQuality.TryGetStringFieldQuickly("cost", ref _strCost);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("multiplier", ref _intMultiplier);
            objXmlLifestyleQuality.TryGetInt32FieldQuickly("multiplierbaseonly", ref _intBaseMultiplier);
            if (objXmlLifestyleQuality.TryGetStringFieldQuickly("category", ref _strCategory))
            {
                _objLifestyleQualityType = ConvertToLifestyleQualityType(_strCategory);
            }
            _objLifestyleQualitySource = objLifestyleQualitySource;
            objXmlLifestyleQuality.TryGetBoolFieldQuickly("print", ref _blnPrint);
            objXmlLifestyleQuality.TryGetBoolFieldQuickly("contributetolimit", ref _blnContributeToLP);
            if (!objXmlLifestyleQuality.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlLifestyleQuality.TryGetStringFieldQuickly("notes", ref _strNotes);
            objXmlLifestyleQuality.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlLifestyleQuality.TryGetStringFieldQuickly("page", ref _strPage);
            string strAllowedFreeLifestyles = string.Empty;
            if (objXmlLifestyleQuality.TryGetStringFieldQuickly("allowed", ref strAllowedFreeLifestyles))
                _lstAllowedFreeLifestyles = strAllowedFreeLifestyles.Split(',').ToList();
            _strExtra = strExtra;
            int intParenthesesIndex = _strExtra.IndexOf('(');
            if (intParenthesesIndex != -1)
            {
                _strExtra = intParenthesesIndex + 1 < strExtra.Length ? strExtra.Substring(intParenthesesIndex + 1).TrimEndOnce(')') : string.Empty;
            }

            // If the item grants a bonus, pass the information to the Improvement Manager.
            XmlNode xmlBonus = objXmlLifestyleQuality["bonus"];
            if (xmlBonus != null)
            {
                string strOldFoced = ImprovementManager.ForcedValue;
                if (!string.IsNullOrEmpty(_strExtra))
                    ImprovementManager.ForcedValue = _strExtra;
                if (!ImprovementManager.CreateImprovements(objCharacter, Improvement.ImprovementSource.Quality, InternalId, xmlBonus, false, 1, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    ImprovementManager.ForcedValue = strOldFoced;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
                ImprovementManager.ForcedValue = strOldFoced;
            }

            // Built-In Qualities appear as grey text to show that they cannot be removed.
            if (objLifestyleQualitySource == QualitySource.BuiltIn)
            {
                Free = true;
            }
        }

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail => _objCachedSourceDetail ?? (_objCachedSourceDetail =
                                                new SourceString(Source, Page(GlobalOptions.Language), GlobalOptions.Language));

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("lifestylequality");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("multiplier", _intMultiplier.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("basemultiplier", _intBaseMultiplier.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("lp", _intLP.ToString());
            objWriter.WriteElementString("contributetolimit", _blnContributeToLP.ToString());
            objWriter.WriteElementString("print", _blnPrint.ToString());
            objWriter.WriteElementString("lifestylequalitytype", _objLifestyleQualityType.ToString());
            objWriter.WriteElementString("lifestylequalitysource", _objLifestyleQualitySource.ToString());
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("allowed", string.Join(",", _lstAllowedFreeLifestyles));
            if (_nodBonus != null)
                objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", string.Empty);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();

            if (OriginSource != QualitySource.BuiltIn)
                _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="objParentLifestyle">Lifestyle object to which this LifestyleQuality belongs.</param>
        public void Load(XmlNode objNode, Lifestyle objParentLifestyle)
        {
            ParentLifestyle = objParentLifestyle;
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            if(!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("lp", ref _intLP);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetInt32FieldQuickly("multiplier", ref _intMultiplier);
            objNode.TryGetInt32FieldQuickly("basemultiplier", ref _intBaseMultiplier);
            objNode.TryGetBoolFieldQuickly("contributetolimit", ref _blnContributeToLP);
            objNode.TryGetBoolFieldQuickly("print", ref _blnPrint);
            if (objNode["lifestylequalitytype"] != null)
                _objLifestyleQualityType = ConvertToLifestyleQualityType(objNode["lifestylequalitytype"].InnerText);
#if DEBUG
            if (objNode["lifestylequalitysource"] != null)
                _objLifestyleQualitySource = ConvertToLifestyleQualitySource(objNode["lifestylequalitysource"].InnerText);
#else
            _objLifestyleQualitySource = QualitySource.Selected;
#endif
            if (!objNode.TryGetStringFieldQuickly("category", ref _strCategory))
            {
                _strCategory = GetNode()?["category"]?.InnerText ?? string.Empty;
            }
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            string strAllowedFreeLifestyles = string.Empty;
            if (!objNode.TryGetStringFieldQuickly("allowed", ref strAllowedFreeLifestyles))
            {
                strAllowedFreeLifestyles = GetNode()?["allowed"]?.InnerText ?? string.Empty;
            }
            _lstAllowedFreeLifestyles = strAllowedFreeLifestyles.Split(',').ToList();
            _nodBonus = objNode["bonus"];
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            
            LegacyShim();
        }

        /// <summary>
        /// Performs actions based on the character's last loaded AppVersion attribute.
        /// </summary>
        private void LegacyShim()
        {
            //Unstored Cost and LP values prior to 5.190.2 nightlies.
            if (_objCharacter.LastSavedVersion <= new Version("5.190.0"))
            {
                XmlDocument objXmlDocument = XmlManager.Load("lifestyles.xml");
                XmlNode objLifestyleQualityNode = GetNode() ??
                                                  objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + _strName + "\"]");
                if (objLifestyleQualityNode == null)
                {
                    List<ListItem> lstQualities = new List<ListItem>();
                    using (XmlNodeList xmlQualityList = objXmlDocument.SelectNodes("/chummer/qualities/quality"))
                        if (xmlQualityList != null)
                            foreach (XmlNode xmlNode in xmlQualityList)
                            {
                                lstQualities.Add(new ListItem(xmlNode["id"]?.InnerText, xmlNode["translate"]?.InnerText ?? xmlNode["name"]?.InnerText));
                            }
                    frmSelectItem frmSelect = new frmSelectItem
                    {
                        GeneralItems = lstQualities,
                        Description = string.Format(LanguageManager.GetString("String_CannotFindLifestyleQuality", GlobalOptions.Language), _strName)
                    };
                    frmSelect.ShowDialog();
                    if (frmSelect.DialogResult == DialogResult.Cancel)
                        return;

                    objLifestyleQualityNode = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + frmSelect.SelectedItem + "\"]");
                }
                int intTemp = 0;
                string strTemp = string.Empty;
                if (objLifestyleQualityNode.TryGetStringFieldQuickly("cost", ref strTemp))
                    CostString = strTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("lp", ref intTemp))
                    LP = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("areamaximum", ref intTemp))
                    AreaMaximum = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("comfortsmaximum", ref intTemp))
                    ComfortMaximum = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("securitymaximum", ref intTemp))
                    SecurityMaximum = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("area", ref intTemp))
                    Area = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("comforts", ref intTemp))
                    Comfort = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("security", ref intTemp))
                    Security = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("multiplier", ref intTemp))
                    Multiplier = intTemp;
                if (objLifestyleQualityNode.TryGetInt32FieldQuickly("multiplierbaseonly", ref intTemp))
                    BaseMultiplier = intTemp;
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (!AllowPrint)
                return;
            objWriter.WriteStartElement("quality");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("formattedname", FormattedDisplayName(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(Extra, strLanguageToPrint));
            objWriter.WriteElementString("lp", LP.ToString(objCulture));
            objWriter.WriteElementString("cost", Cost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            string strLifestyleQualityType = Type.ToString();
            if (strLanguageToPrint != GlobalOptions.DefaultLanguage)
            {
                XmlNode objNode = XmlManager.Load("lifestyles.xml", strLanguageToPrint).SelectSingleNode("/chummer/categories/category[. = \"" + strLifestyleQualityType + "\"]");
                strLifestyleQualityType = objNode?.Attributes?["translate"]?.InnerText ?? strLifestyleQualityType;
            }
            objWriter.WriteElementString("lifestylequalitytype", strLifestyleQualityType);
            objWriter.WriteElementString("lifestylequalitytype_english", Type.ToString());
            objWriter.WriteElementString("lifestylequalitysource", OriginSource.ToString());
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
#endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this LifestyleQuality in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D");

        /// <summary>
        /// LifestyleQuality's name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                    _objCachedMyXmlNode = null;
                _strName = value;
            }
        }

        /// <summary>
        /// LifestyleQuality's parent lifestyle.
        /// </summary>
        public Lifestyle ParentLifestyle
        {
            get => _objParentLifestyle;
            set => _objParentLifestyle = value;
        }

        /// <summary>
        /// Extra information that should be applied to the name, like a linked CharacterAttribute.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = value;
        }

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get => _strSource;
            set => _strSource = value;
        }

        /// <summary>
        /// Page Number.
        /// </summary>
        public string Page(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get => _nodBonus;
            set => _nodBonus = value;
        }

        /// <summary>
        /// LifestyleQuality Type.
        /// </summary>
        public QualityType Type => _objLifestyleQualityType;

        /// <summary>
        /// Source of the LifestyleQuality.
        /// </summary>
        public QualitySource OriginSource
        {
            get => _objLifestyleQualitySource;
            set => _objLifestyleQualitySource = value;
        }

        /// <summary>
        /// Number of Build Points the LifestyleQuality costs.
        /// </summary>
        public int LP
        {
            get => Free || !ContributesLP ? 0 : _intLP;
            set => _intLP = value;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (!string.IsNullOrEmpty(Extra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';
            }
            return strReturn;
        }

        public string FormattedDisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayName(strLanguage);

            if (Multiplier > 0)
            {
                strReturn += $" [+{Multiplier}%]";
            }
            else if (Multiplier < 0)
            {
                strReturn += $" [{Multiplier}%]";
            }

            if (Cost > 0)
            {
                strReturn += " [+" + Cost.ToString(_objCharacter.Options.NuyenFormat, objCulture) + "¥]";
            }
            else if (Cost < 0)
            {
                strReturn += " [" + Cost.ToString(_objCharacter.Options.NuyenFormat, objCulture) + "¥]";
            }
            return strReturn;
        }

        /// <summary>
        /// Whether or not the LifestyleQuality appears on the printouts.
        /// </summary>
        public bool AllowPrint
        {
            get => _blnPrint;
            set => _blnPrint = value;
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        /// <summary>
        /// Nuyen cost of the Quality.
        /// </summary>
        public decimal Cost
        {
            get
            {
                if (Free || FreeByLifestyle)
                    return 0;
                if (!decimal.TryParse(CostString, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decReturn))
                {
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(CostString, out bool blnIsSuccess);
                    if (blnIsSuccess)
                        return Convert.ToDecimal(objProcess);
                }
                return decReturn;
            }
        }

        /// <summary>
        /// String for the nuyen cost of the Quality.
        /// </summary>
        public string CostString
        {
            get => string.IsNullOrWhiteSpace(_strCost) ? "0" : _strCost;
            set => _strCost = value;
        }

        /// <summary>
        /// Does the Quality have a Nuyen or LP cost?
        /// </summary>
        public bool Free
        {
            get => _blnFree || OriginSource == QualitySource.BuiltIn;
            set => _blnFree = value;
        }

        public bool ContributesLP
        {
            get => _blnContributeToLP;
            set => _blnContributeToLP = value;
        }

        /// <summary>
        /// Are the costs of this Quality included in base lifestyle costs?
        /// </summary>
        public bool FreeByLifestyle
        {
            get
            {
                if (Type == QualityType.Entertainment || Type == QualityType.Contracts)
                {
                    string strLifestyleEquivalent = Lifestyle.GetEquivalentLifestyle(_objParentLifestyle.BaseLifestyle);
                    if (!string.IsNullOrEmpty(_objParentLifestyle?.BaseLifestyle) &&
                        _lstAllowedFreeLifestyles.Any(strLifestyle => strLifestyle == strLifestyleEquivalent || strLifestyle == _objParentLifestyle.BaseLifestyle))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Comfort LP is increased/reduced by this Quality.
        /// </summary>
        public int Comfort { get; set; }

        /// <summary>
        /// Comfort LP maximum is increased/reduced by this Quality.
        /// </summary>
        public int ComfortMaximum { get; set; }

        /// <summary>
        /// Security LP value is increased/reduced by this Quality.
        /// </summary>
        public int SecurityMaximum { get; set; }

        /// <summary>
        /// Security LP value is increased/reduced by this Quality.
        /// </summary>
        public int Security { get; set; }

        /// <summary>
        /// Percentage by which the quality increases the overall Lifestyle Cost.
        /// </summary>
        public int Multiplier
        {
            get => (Free || FreeByLifestyle) ? 0 : _intMultiplier;
            set => _intMultiplier = value;
        }

        /// <summary>
        /// Percentage by which the quality increases the Lifestyle Cost ONLY, without affecting other qualities.
        /// </summary>
        public int BaseMultiplier
        {
            get => (Free || FreeByLifestyle) ? 0 : _intBaseMultiplier;
            set => _intBaseMultiplier = value;
        }

        /// <summary>
        /// Category of the Quality.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        /// Area/Neighborhood LP Cost/Benefit of the Quality.
        /// </summary>
        public int AreaMaximum { get; set; }

        /// <summary>
        /// Area/Neighborhood minimum is increased/reduced by this Quality.
        /// </summary>
        public int Area { get; set; }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.Instance.LiveCustomData)
            {

                if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage && !GlobalOptions.Instance.LiveCustomData) return _objCachedMyXmlNode;
                _objCachedMyXmlNode = SourceID == Guid.Empty
                    ? XmlManager.Load("lifestyles.xml", strLanguage)
                        .SelectSingleNode($"/chummer/qualities/quality[name = \"{Name}\"]")
                    : XmlManager.Load("lifestyles.xml", strLanguage)
                        .SelectSingleNode($"/chummer/qualities/quality[id = \"{SourceIDString}\" or id = \"{SourceIDString.ToUpperInvariant()}\"]");
                _strCachedXmlNodeLanguage = strLanguage;
                return _objCachedMyXmlNode;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region UI Methods
        public TreeNode CreateTreeNode()
        {
            if (OriginSource == QualitySource.BuiltIn && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = FormattedDisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language),
                Tag = this,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };
            return objNode;
        }

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return Color.SaddleBrown;
                }
                if (OriginSource == QualitySource.BuiltIn)
                {
                    return SystemColors.GrayText;
                }
                return SystemColors.WindowText;
            }
        }
        #endregion

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
