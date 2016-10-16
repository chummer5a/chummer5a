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
			_strName = objXmlGear["name"].InnerText;
			_strCategory = objXmlGear["category"].InnerText;
			_strAvail = objXmlGear["avail"].InnerText;
			objXmlGear.TryGetField("cost", out _strCost);
			objXmlGear.TryGetField("cost3", out _strCost3, "");
			objXmlGear.TryGetField("cost6", out _strCost6, "");
			objXmlGear.TryGetField("cost10", out _strCost10, "");
			objXmlGear.TryGetField("armorcapacity", out _strArmorCapacity);
			_nodBonus = objXmlGear["bonus"];
			_intMaxRating = Convert.ToInt32(objXmlGear["rating"].InnerText);
			_intRating = intRating;
			_strSource = objXmlGear["source"].InnerText;
			_strPage = objXmlGear["page"].InnerText;
			_intDeviceRating = Convert.ToInt32(objXmlGear["devicerating"].InnerText);
            
            
			_intAttack = Convert.ToInt32(objXmlGear["attack"].InnerText);
			_intSleaze= Convert.ToInt32(objXmlGear["sleaze"].InnerText);
			_intDataProcessing = Convert.ToInt32(objXmlGear["dataprocessing"].InnerText);
			_intFirewall = Convert.ToInt32(objXmlGear["firewall"].InnerText);

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("gear.xml");
				XmlNode objGearNode = objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + _strName + "\"]");
				if (objGearNode != null)
				{
					if (objGearNode["translate"] != null)
						_strAltName = objGearNode["translate"].InnerText;
					if (objGearNode["altpage"] != null)
						_strAltPage = objGearNode["altpage"].InnerText;
				}

				if (_strAltName.StartsWith("Stacked Focus"))
					_strAltName = _strAltName.Replace("Stacked Focus", LanguageManager.Instance.GetString("String_StackedFocus"));

				objGearNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
				if (objGearNode != null)
				{
					if (objGearNode.Attributes["translate"] != null)
						_strAltCategory = objGearNode.Attributes["translate"].InnerText;
				}

				if (_strAltCategory.StartsWith("Stacked Focus"))
					_strAltCategory = _strAltCategory.Replace("Stacked Focus", LanguageManager.Instance.GetString("String_StackedFocus"));
			}

			string strSource = _guiID.ToString();

			objNode.Text = DisplayNameShort;
			objNode.Tag = _guiID.ToString();

			// If the item grants a bonus, pass the information to the Improvement Manager.
			if (objXmlGear["bonus"] != null)
			{
				ImprovementManager objImprovementManager;
				if (blnAddImprovements)
					objImprovementManager = new ImprovementManager(objCharacter);
				else
					objImprovementManager = new ImprovementManager(null);

				if (!objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Gear, strSource, objXmlGear["bonus"], false, 1, DisplayNameShort))
				{
					_guiID = Guid.Empty;
					return;
				}
				if (objImprovementManager.SelectedValue != "")
				{
					_strExtra = objImprovementManager.SelectedValue;
					objNode.Text += " (" + objImprovementManager.SelectedValue + ")";
				}
			}

			// Check to see if there are any child elements.
			if (objXmlGear.InnerXml.Contains("<gears>") && blnCreateChildren)
			{
				// Create Gear using whatever information we're given.
				foreach (XmlNode objXmlChild in objXmlGear.SelectNodes("gears/gear"))
				{
					Gear objChild = new Gear(_objCharacter);
					TreeNode objChildNode = new TreeNode();
					objChild.Name = objXmlChild["name"].InnerText;
					objChild.Category = objXmlChild["category"].InnerText;
					objChild.Avail = "0";
					objChild.Cost = "0";
					objChild.Source = _strSource;
					objChild.Page = _strPage;
					objChild.Parent = this;
					_objChildren.Add(objChild);

					objChildNode.Text = objChild.DisplayName;
					objChildNode.Tag = objChild.InternalId;
					objNode.Nodes.Add(objChildNode);
					objNode.Expand();
				}

				XmlDocument objXmlGearDocument = XmlManager.Instance.Load("gear.xml");
				CreateChildren(objXmlGearDocument, objXmlGear, this, objNode, objCharacter, blnCreateChildren);
			}

			// Add the Copy Protection and Registration plugins to the Matrix program. This does not apply if Unwired is not enabled, Hacked is selected, or this is a Suite being added (individual programs will add it to themselves).
			if (blnCreateChildren)
			{
				if ((_strCategory == "Matrix Programs" || _strCategory == "Skillsofts" || _strCategory == "Autosofts" || _strCategory == "Autosofts, Agent" || _strCategory == "Autosofts, Drone") && objCharacter.Options.BookEnabled("UN") && !_strName.StartsWith("Suite:"))
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("gear.xml");

					if (_objCharacter.Options.AutomaticCopyProtection)
					{
						Gear objPlugin1 = new Gear(_objCharacter);
						TreeNode objPlugin1Node = new TreeNode();
						objPlugin1.Create(objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"Copy Protection\"]"), objCharacter, objPlugin1Node, _intRating, null, null);
						if (_intRating == 0)
							objPlugin1.Rating = 1;
						objPlugin1.Avail = "0";
						objPlugin1.Cost = "0";
						objPlugin1.Cost3 = "0";
						objPlugin1.Cost6 = "0";
						objPlugin1.Cost10 = "0";
						objPlugin1.Capacity = "[0]";
						objPlugin1.Parent = this;
						_objChildren.Add(objPlugin1);
						objNode.Nodes.Add(objPlugin1Node);
					}

					if (_objCharacter.Options.AutomaticRegistration)
					{
						Gear objPlugin2 = new Gear(_objCharacter);
						TreeNode objPlugin2Node = new TreeNode();
						objPlugin2.Create(objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"Registration\"]"), objCharacter, objPlugin2Node, 0, null, null);
						objPlugin2.Avail = "0";
						objPlugin2.Cost = "0";
						objPlugin2.Cost3 = "0";
						objPlugin2.Cost6 = "0";
						objPlugin2.Cost10 = "0";
						objPlugin2.Capacity = "[0]";
						objPlugin2.Parent = this;
						_objChildren.Add(objPlugin2);
						objNode.Nodes.Add(objPlugin2Node);
						objNode.Expand();
					}

					if ((objCharacter.Metatype == "A.I." || objCharacter.MetatypeCategory == "Technocritters" || objCharacter.MetatypeCategory == "Protosapients"))
					{
						Gear objPlugin3 = new Gear(_objCharacter);
						TreeNode objPlugin3Node = new TreeNode();
						objPlugin3.Create(objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"Ergonomic\"]"), objCharacter, objPlugin3Node, 0, null, null);
						objPlugin3.Avail = "0";
						objPlugin3.Cost = "0";
						objPlugin3.Cost3 = "0";
						objPlugin3.Cost6 = "0";
						objPlugin3.Cost10 = "0";
						objPlugin3.Capacity = "[0]";
						objPlugin3.Parent = this;
						_objChildren.Add(objPlugin3);
						objNode.Nodes.Add(objPlugin3Node);

						Gear objPlugin4 = new Gear(_objCharacter);
						TreeNode objPlugin4Node = new TreeNode();
						objPlugin4.Create(objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"Optimization\" and category = \"Program Options\"]"), objCharacter, objPlugin4Node, _intRating, null, null);
						if (_intRating == 0)
							objPlugin4.Rating = 1;
						objPlugin4.Avail = "0";
						objPlugin4.Cost = "0";
						objPlugin4.Cost3 = "0";
						objPlugin4.Cost6 = "0";
						objPlugin4.Cost10 = "0";
						objPlugin4.Capacity = "[0]";
						objPlugin4.Parent = this;
						_objChildren.Add(objPlugin4);
						objNode.Nodes.Add(objPlugin4Node);
						objNode.Expand();
					}
				}
			}
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
			_strName = objGear.Name;
			_strCategory = objGear.Category;
			_intMaxRating = objGear.MaxRating;
			_intMinRating = objGear.MinRating;
			_intRating = objGear.Rating;
			_intQty = objGear.Quantity;
			_strCapacity = objGear.Capacity;
			_strArmorCapacity = objGear.ArmorCapacity;
			_strAvail = objGear.Avail;
			_strAvail3 = objGear.Avail3;
			_strAvail6 = objGear.Avail6;
			_strAvail10 = objGear.Avail10;
			_intCostFor = objGear.CostFor;
			_strOverclocked = objGear.Overclocked;
			_intDeviceRating = objGear.DeviceRating;
			_intAttack = objGear.Attack;
			_intDataProcessing = objGear.DataProcessing;
			_intFirewall = objGear.Firewall;
			_intSleaze = objGear.Sleaze;
			_strCost = objGear.Cost;
			_strCost3 = objGear.Cost3;
			_strCost6 = objGear.Cost6;
			_strCost10 = objGear.Cost10;
			_strSource = objGear.Source;
			_strPage = objGear.Page;
			_strExtra = objGear.Extra;
			_blnBonded = objGear.Bonded;
			_blnEquipped = objGear.Equipped;
			_blnHomeNode = objGear.HomeNode;
			_nodBonus = objGear.Bonus;
			_nodWeaponBonus = objGear.WeaponBonus;
			_guiWeaponID = Guid.Parse(objGear.WeaponID);
			_strNotes = objGear.Notes;
			_strLocation = objGear.Location;

			objNode.Text = DisplayName;
			objNode.Tag = _guiID.ToString();

			foreach (Gear objGearChild in objGear.Children)
			{
				TreeNode objChildNode = new TreeNode();
				Gear objChild = new Gear(_objCharacter);
				if (objGearChild.GetType() == typeof(Commlink))
				{
					Commlink objCommlink = new Commlink(_objCharacter);
					objCommlink.Copy(objGearChild, objChildNode, objWeapons, objWeaponNodes);
					objChild = objCommlink;
				}
				else
					objChild.Copy(objGearChild, objChildNode, objWeapons, objWeaponNodes);
				_objChildren.Add(objChild);

				objNode.Nodes.Add(objChildNode);
				objNode.Expand();
			}
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public new void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("gear");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("category", _strCategory);
			objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
			objWriter.WriteElementString("maxrating", _intMaxRating.ToString());
			objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteElementString("qty", _intQty.ToString());
			objWriter.WriteElementString("avail", _strAvail);
			objWriter.WriteElementString("cost", _strCost);
			objWriter.WriteElementString("extra", _strExtra);
			objWriter.WriteElementString("bonded", _blnBonded.ToString());
			objWriter.WriteElementString("equipped", _blnEquipped.ToString());
			objWriter.WriteElementString("homenode", _blnHomeNode.ToString());
			objWriter.WriteElementString("overclocked", _blnHomeNode.ToString());
			if (_nodBonus != null)
				objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
			else
				objWriter.WriteElementString("bonus", "");
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			objWriter.WriteElementString("devicerating", _intDeviceRating.ToString());
			objWriter.WriteElementString("attack", _intAttack.ToString());
			objWriter.WriteElementString("sleaze", _intSleaze.ToString());
			objWriter.WriteElementString("dataprocessing", _intDataProcessing.ToString());
			objWriter.WriteElementString("firewall", _intFirewall.ToString());
			objWriter.WriteElementString("gearname", _strGearName);
			objWriter.WriteStartElement("children");
			foreach (Gear objGear in _objChildren)
			{
				// Use the Gear's SubClass if applicable.
				if (objGear.GetType() == typeof(Commlink))
				{
					Commlink objCommlink = new Commlink(_objCharacter);
					objCommlink = (Commlink)objGear;
					objCommlink.Save(objWriter);
				}
				else
				{
					objGear.Save(objWriter);
				}
			}
			objWriter.WriteEndElement();
			objWriter.WriteElementString("location", _strLocation);
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteElementString("discountedcost", DiscountCost.ToString());
			objWriter.WriteElementString("active", _blnActiveCommlink.ToString());
			objWriter.WriteEndElement();
		}

		/// <summary>
		/// Load the Gear from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public new void Load(XmlNode objNode, bool blnCopy = false)
		{
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strCategory = objNode["category"].InnerText;
			objNode.TryGetField("armorcapacity", out _strArmorCapacity).ToString();
			_intMaxRating = Convert.ToInt32(objNode["maxrating"].InnerText);
			_intRating = Convert.ToInt32(objNode["rating"].InnerText);
			_intQty = Convert.ToInt32(objNode["qty"].InnerText);
			_strAvail = objNode["avail"].InnerText;
			_strCost = objNode["cost"].InnerText;
			_strExtra = objNode["extra"].InnerText;
			objNode.TryGetField("overclocked", out _strOverclocked);
			objNode.TryGetField("bonded", out _blnBonded);
			objNode.TryGetField("equipped", out _blnEquipped);
			objNode.TryGetField("homenode", out _blnHomeNode);
			_nodBonus = objNode["bonus"];
			_strSource = objNode["source"].InnerText;
			objNode.TryGetField("page", out _strPage);
			_intDeviceRating = Convert.ToInt32(objNode["devicerating"].InnerText);
			objNode.TryGetField("attack", out _intAttack);
			objNode.TryGetField("sleaze", out _intSleaze);
			objNode.TryGetField("dataprocessing", out _intDataProcessing);
			objNode.TryGetField("firewall", out _intFirewall);
			objNode.TryGetField("gearname", out _strGearName);

			if (objNode.InnerXml.Contains("<gear>"))
			{
				XmlNodeList nodChildren = objNode.SelectNodes("children/gear");
				foreach (XmlNode nodChild in nodChildren)
				{
					switch (nodChild["category"].InnerText)
					{
						case "Commlinks":
						case "Commlink Accessories":
						case "Cyberdecks":
						case "Rigger Command Consoles":
							Commlink objCommlink = new Commlink(_objCharacter);
							objCommlink.Load(nodChild, blnCopy);
							objCommlink.Parent = this;
							_objChildren.Add(objCommlink);
							break;
						default:
							Gear objGear = new Gear(_objCharacter);
							objGear.Load(nodChild, blnCopy);
							objGear.Parent = this;
							_objChildren.Add(objGear);
							break;
					}
				}
			}
			objNode.TryGetField("location", out _strLocation);
			objNode.TryGetField("notes", out _strNotes);
			objNode.TryGetField("discountedcost", out _blnDiscountCost);
			objNode.TryGetField("active", out _blnActiveCommlink);

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("gear.xml");
				XmlNode objGearNode = objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + _strName + "\"]");
				if (objGearNode != null)
				{
					if (objGearNode["translate"] != null)
						_strAltName = objGearNode["translate"].InnerText;
					if (objGearNode["altpage"] != null)
						_strAltPage = objGearNode["altpage"].InnerText;
				}

				if (_strAltName.StartsWith("Stacked Focus"))
					_strAltName = _strAltName.Replace("Stacked Focus", LanguageManager.Instance.GetString("String_StackedFocus"));

				objGearNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
				if (objGearNode != null)
				{
					if (objGearNode.Attributes["translate"] != null)
						_strAltCategory = objGearNode.Attributes["translate"].InnerText;
				}

				if (_strAltCategory.StartsWith("Stacked Focus"))
					_strAltCategory = _strAltCategory.Replace("Stacked Focus", LanguageManager.Instance.GetString("String_StackedFocus"));
			}

			if (blnCopy)
			{
				_guiID = Guid.NewGuid();
				_strLocation = string.Empty;
				_blnHomeNode = false;
			}
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public new void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("gear");
			objWriter.WriteElementString("name", DisplayNameShort);
			objWriter.WriteElementString("name_english", _strName);
			if (DisplayCategory.EndsWith("s"))
				objWriter.WriteElementString("category", DisplayCategory.Substring(0,DisplayCategory.Length -1));
			else
				objWriter.WriteElementString("category", DisplayCategory);
			objWriter.WriteElementString("category_english", _strCategory);
			objWriter.WriteElementString("iscommlink", true.ToString());
			objWriter.WriteElementString("ispersona", IsLivingPersona.ToString());
			//objWriter.WriteElementString("isnexus", (_strCategory == "Nexus").ToString());
			objWriter.WriteElementString("isammo", (_strCategory == "Ammunition").ToString());
			objWriter.WriteElementString("isprogram", IsProgram.ToString());
			objWriter.WriteElementString("isos", false.ToString());
			objWriter.WriteElementString("issin", false.ToString());
			objWriter.WriteElementString("maxrating", _intMaxRating.ToString());
			objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteElementString("attack", _intAttack.ToString());
			objWriter.WriteElementString("sleaze", _intSleaze.ToString());
			objWriter.WriteElementString("dataprocessing", _intDataProcessing.ToString());
			objWriter.WriteElementString("firewall", _intFirewall.ToString());
			objWriter.WriteElementString("qty", _intQty.ToString());
			objWriter.WriteElementString("avail", TotalAvail(true));
			objWriter.WriteElementString("avail_english", TotalAvail(true, true));
			objWriter.WriteElementString("cost", TotalCost.ToString());
			objWriter.WriteElementString("owncost", OwnCost.ToString());
			objWriter.WriteElementString("extra", LanguageManager.Instance.TranslateExtra(_strExtra));
			objWriter.WriteElementString("bonded", _blnBonded.ToString());
			objWriter.WriteElementString("equipped", _blnEquipped.ToString());
			objWriter.WriteElementString("homenode", _blnHomeNode.ToString());
			objWriter.WriteElementString("gearname", _strGearName);
			objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
			objWriter.WriteElementString("devicerating", TotalDeviceRating.ToString());
			objWriter.WriteElementString("processorlimit", ProcessorLimit.ToString());
			objWriter.WriteElementString("conditionmonitor", ConditionMonitor.ToString());
			objWriter.WriteElementString("active", _blnActiveCommlink.ToString());
			objWriter.WriteStartElement("children");
			foreach (Gear objGear in _objChildren)
			{
				if (objGear.Category != "Commlink Upgrade" && objGear.Category != "Commlink Operating System Upgrade")
				{
					// Use the Gear's SubClass if applicable.
					if (objGear.GetType() == typeof(Commlink))
					{
						Commlink objCommlink = new Commlink(_objCharacter);
						objCommlink = (Commlink)objGear;
						objCommlink.Print(objWriter);
					}
					else
					{
						objGear.Print(objWriter);
					}
				}
			}
			objWriter.WriteEndElement();
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
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