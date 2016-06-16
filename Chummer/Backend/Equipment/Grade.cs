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
		private string _strAltName = "";
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
			_strName = objNode["name"].InnerText;
			if (objNode["translate"] != null)
				_strAltName = objNode["translate"].InnerText;
			_decEss = Convert.ToDecimal(objNode["ess"].InnerText, GlobalOptions.Instance.CultureInfo);
			_dblCost = Convert.ToDouble(objNode["cost"].InnerText, GlobalOptions.Instance.CultureInfo);
			_intAvail = Convert.ToInt32(objNode["avail"].InnerText, GlobalOptions.Instance.CultureInfo);
			_strSource = objNode["source"].InnerText;
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
				if (_strAltName != string.Empty)
					return _strAltName;
				else
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