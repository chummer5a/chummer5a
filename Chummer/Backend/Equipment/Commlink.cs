using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace Chummer.Backend.Equipment
{
	/// <summary>
	/// Commlink Device.
	/// </summary>
	public class Commlink : Gear
	{
		private bool _blnIsLivingPersona = false;
		private bool _blnActiveCommlink = false;
		private int _intAttack = 0;
		private int _intSleaze = 0;
		private int _intDataProcessing = 0;
		private int _intFirewall = 0;
		private string _strOverclocked = "None";

		#region Constructor, Create, Save, Load, and Print Methods
		public Commlink(Character objCharacter) : base(objCharacter)
		{
		}

		/// Create a Commlink from an XmlNode and return the TreeNodes for it.
		/// <param name="objXmlGear">XmlNode to create the object from.</param>
		/// <param name="objCharacter">Character the Gear is being added to.</param>
		/// <param name="objNode">TreeNode to populate a TreeView.</param>
		/// <param name="intRating">Gear Rating.</param>
		/// <param name="blnAddImprovements">Whether or not Improvements should be added to the character.</param>
		/// <param name="blnCreateChildren">Whether or not child Gear should be created.</param>
		public void Create(XmlNode objXmlGear, Character objCharacter, TreeNode objNode, int intRating, bool blnAddImprovements = true, bool blnCreateChildren = true)
		{
            base.Create(objXmlGear, objCharacter, objNode, intRating, new List<Weapon>(), new List<TreeNode>(), "", false, false, blnAddImprovements, blnCreateChildren);

			_intDeviceRating = Convert.ToInt32(objXmlGear["devicerating"].InnerText);
            
			_intAttack = Convert.ToInt32(objXmlGear["attack"].InnerText);
			_intSleaze= Convert.ToInt32(objXmlGear["sleaze"].InnerText);
			_intDataProcessing = Convert.ToInt32(objXmlGear["dataprocessing"].InnerText);
			_intFirewall = Convert.ToInt32(objXmlGear["firewall"].InnerText);
		}

		/// <summary>
		/// Copy a piece of Gear.
		/// </summary>
		/// <param name="objGear">Gear object to copy.</param>
		/// <param name="objNode">TreeNode created by copying the item.</param>
		/// <param name="objWeapons">List of Weapons created by copying the item.</param>
		/// <param name="objWeaponNodes">List of Weapon TreeNodes created by copying the item.</param>
		public void Copy(Commlink objGear, TreeNode objNode, List<Weapon> objWeapons, List<TreeNode> objWeaponNodes)
		{
            base.Copy(objGear, objNode, new List<Weapon>(), new List<TreeNode>());
			_strOverclocked = objGear.Overclocked;
			_intDeviceRating = objGear.DeviceRating;
			_intAttack = objGear.Attack;
			_intDataProcessing = objGear.DataProcessing;
			_intFirewall = objGear.Firewall;
			_intSleaze = objGear.Sleaze;
            _blnIsLivingPersona = objGear.IsLivingPersona;
            _blnActiveCommlink = objGear.IsActive;
    }

		/// <summary>
		/// Core code to Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public new void SaveInner(XmlTextWriter objWriter)
		{
            base.SaveInner(objWriter);
			objWriter.WriteElementString("overclocked", _blnHomeNode.ToString());
			objWriter.WriteElementString("devicerating", _intDeviceRating.ToString());
			objWriter.WriteElementString("attack", _intAttack.ToString());
			objWriter.WriteElementString("sleaze", _intSleaze.ToString());
			objWriter.WriteElementString("dataprocessing", _intDataProcessing.ToString());
			objWriter.WriteElementString("firewall", _intFirewall.ToString());
			objWriter.WriteElementString("livingpersona", _blnIsLivingPersona.ToString());
			objWriter.WriteElementString("active", _blnActiveCommlink.ToString());
		}

		/// <summary>
		/// Load the Gear from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public new void Load(XmlNode objNode, bool blnCopy = false)
		{
            base.Load(objNode, blnCopy);
			objNode.TryGetField("overclocked", out _strOverclocked);
			_intDeviceRating = Convert.ToInt32(objNode["devicerating"].InnerText);
			objNode.TryGetField("attack", out _intAttack);
			objNode.TryGetField("sleaze", out _intSleaze);
			objNode.TryGetField("dataprocessing", out _intDataProcessing);
			objNode.TryGetField("firewall", out _intFirewall);
            objNode.TryGetField("livingpersona", out _blnIsLivingPersona);
            objNode.TryGetField("active", out _blnActiveCommlink);
		}

		/// <summary>
		/// Core code to Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public new void PrintInner(XmlTextWriter objWriter, bool blnIsCommlink = true, bool blnIsPersona = false)
		{
            base.PrintInner(objWriter, true, IsLivingPersona);

			objWriter.WriteElementString("attack", _intAttack.ToString());
			objWriter.WriteElementString("sleaze", _intSleaze.ToString());
			objWriter.WriteElementString("dataprocessing", _intDataProcessing.ToString());
			objWriter.WriteElementString("firewall", _intFirewall.ToString());
			objWriter.WriteElementString("devicerating", TotalDeviceRating.ToString());
			objWriter.WriteElementString("processorlimit", ProcessorLimit.ToString());
			objWriter.WriteElementString("active", _blnActiveCommlink.ToString());
		}
		#endregion

		#region Properties
		/// <summary>
		/// Device Rating.
		/// </summary>
		public new int DeviceRating
		{
			get
			{
				return _intDeviceRating;
			}
			set
			{
				_intDeviceRating = value;
			}
		}

		/// <summary>
		/// Attack.
		/// </summary>
		public int Attack
		{
			get
			{
				return _intAttack;
			}
			set
			{
				_intAttack = value;
			}
		}

		/// <summary>
		/// Sleaze.
		/// </summary>
		public int Sleaze
		{
			get
			{
				return _intSleaze;
			}
			set
			{
				_intSleaze = value;
			}
		}

		/// <summary>
		/// Data Processing.
		/// </summary>
		public int DataProcessing
		{
			get
			{
				return _intDataProcessing;
			}
			set
			{
				_intDataProcessing = value;
			}
		}

		/// <summary>
		/// Firewall.
		/// </summary>
		public int Firewall
		{
			get
			{
				return _intFirewall;
			}
			set
			{
				_intFirewall = value;
			}
		}

		/// <summary>
		/// Whether or not this Commlink is a Living Persona. This should only be set by the character when printing.
		/// </summary>
		public bool IsLivingPersona
		{
			get
			{
				return _blnIsLivingPersona;
			}
			set
			{
				_blnIsLivingPersona = value;
			}
		}

		/// <summary>
		/// Whether or not this Commlink is active and counting towards the character's Matrix Initiative.
		/// </summary>
		public bool IsActive
		{
			get
			{
				return _blnActiveCommlink;
			}
			set
			{
				_blnActiveCommlink = value;
			}
		}
		#endregion

		#region Complex Properties
		/// <summary>
		/// Total Device Rating including Commlink Upgrades.
		/// </summary>
		public int TotalDeviceRating
		{
			get
			{
				int intDeviceRating = _intDeviceRating;

				// Adjust the stat to include the A.I.'s Home Node bonus.
				if (_blnHomeNode)
				{
					decimal decBonus = Math.Ceiling(_objCharacter.CHA.TotalValue / 2m);
					int intBonus = Convert.ToInt32(decBonus, GlobalOptions.Instance.CultureInfo);
					intDeviceRating += intBonus;
				}

				return intDeviceRating;
			}
		}

		/// <summary>
		/// Get the total data processing this or any submodule pocess
		/// </summary>
		public int TotalDataProcessing
		{
			get
			{
				int rating = _intDataProcessing;
				foreach (Gear child in _objChildren)
				{
					Commlink link = (Commlink) child;
					if (link != null)
					{
						rating = Math.Max(rating, link.TotalDataProcessing);
					}
				}
				if (_objCharacter.Overclocker && Overclocked == "DataProc")
				{
					rating++;
				}
				return rating;
			}
		}

		/// <summary>
		/// Get the highest sleaze this module or any submodule pocess
		/// </summary>
		public int TotalAttack
		{
			get
			{
				int rating = _intAttack;
				foreach (Gear child in _objChildren)
				{
					Commlink link = (Commlink)child;
					if (link != null)
					{
						rating = Math.Max(rating, link.TotalAttack);
					}
				}
				if (_objCharacter.Overclocker && Overclocked == "Attack")
				{
					rating++;
				}
				return rating;
			}
		}

		/// <summary>
		/// Get the highest sleaze this module or any submodule pocess
		/// </summary>
		public int TotalSleaze
		{
			get
			{
				int rating = _intSleaze;
				foreach (Gear child in _objChildren)
				{
					Commlink link = (Commlink)child;
					if (link != null)
					{
						rating = Math.Max(rating, link.TotalSleaze);
					}
				}
				if (_objCharacter.Overclocker && Overclocked == "Sleaze")
				{
					rating++;
				}
				return rating;
			}
		}

		/// <summary>
		/// Get the highest firewall attribute this or any submodule pocess
		/// </summary>
		public int TotalFirewall
		{
			get
			{
				int rating = _intFirewall;
				foreach (Gear child in _objChildren)
				{
					Commlink link = (Commlink)child;
					if (link != null)
					{
						rating = Math.Max(rating, link.TotalFirewall);
					}
				}
				if (_objCharacter.Overclocker && Overclocked == "Firewall")
				{
					rating++;
				}
				return rating;
			}
		}

		/// <summary>
		/// Commlink's Processor Limit.
		/// </summary>
		public int ProcessorLimit
		{
			get
			{
				return TotalDeviceRating;
			}
		}

		/// <summary>
		/// ASDF attribute boosted by Overclocker.
		/// </summary>
		public string Overclocked
		{
			get
			{
				return _strOverclocked;
			}
			set
			{
				_strOverclocked = value;
			}
		}
		#endregion
	}
}