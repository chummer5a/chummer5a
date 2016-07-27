using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
	/// <summary>
	/// Vehicle.
	/// </summary>
	public class Vehicle
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
		private string _strCategory = "";
		private int _intHandling = 0;
		private int _intOffroadHandling = 0;
		private int _intAccel = 0;
		private int _intSpeed = 0;
		private int _intPilot = 0;
		private int _intBody = 0;
		private int _intArmor = 0;
		private int _intSensor = 0;
		private int _intSeats = 0;
		private string _strAvail = "";
		private string _strCost = "";
		private string _strSource = "";
		private string _strPage = "";
		private string _strVehicleName = "";
		private int _intAddSlots = 0;
		private int _intDroneModSlots = 0;
		private int _intAddPowertrainModSlots = 0;
		private int _intAddProtectionModSlots = 0;
		private int _intAddWeaponModSlots = 0;
		private int _intAddBodyModSlots = 0;
		private int _intAddElectromagneticModSlots = 0;
		private int _intAddCosmeticModSlots = 0;
		private int _intDeviceRating = 3;
		private bool _blnHomeNode = false;
		private List<VehicleMod> _lstVehicleMods = new List<VehicleMod>();
		private List<Gear> _lstGear = new List<Gear>();
		private List<Weapon> _lstWeapons = new List<Weapon>();
		private string _strNotes = "";
		private string _strAltName = "";
		private string _strAltCategory = "";
		private string _strAltPage = "";
		private List<string> _lstLocations = new List<string>();
		private bool _blnDealerConnectionDiscount = false;
		private bool _blnBlackMarketDiscount = false;

		private readonly Character _objCharacter;

		// Condition Monitor Progress.
		private int _intPhysicalCMFilled = 0;
		private int _intMatrixCMFilled = 0;

		#region Constructor, Create, Save, Load, and Print Methods
		public Vehicle(Character objCharacter)
		{
			// Create the GUID for the new Vehicle.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}

		/// Create a Vehicle from an XmlNode and return the TreeNodes for it.
		/// <param name="objXmlVehicle">XmlNode of the Vehicle to create.</param>
		/// <param name="objNode">TreeNode to add to a TreeView.</param>
		/// <param name="cmsVehicle">ContextMenuStrip to attach to Weapon Mounts.</param>
		/// <param name="cmsVehicleGear">ContextMenuStrip to attach to Gear.</param>
		/// <param name="cmsVehicleWeapon">ContextMenuStrip to attach to Vehicle Weapons.</param>
		/// <param name="cmsVehicleWeaponAccessory">ContextMenuStrip to attach to Weapon Accessories.</param>
		/// <param name="blnCreateChildren">Whether or not child items should be created.</param>
		public void Create(XmlNode objXmlVehicle, TreeNode objNode, ContextMenuStrip cmsVehicle, ContextMenuStrip cmsVehicleGear, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, bool blnCreateChildren = true)
		{
			_strName = objXmlVehicle["name"].InnerText;
			_strCategory = objXmlVehicle["category"].InnerText;
			//Some vehicles have different Offroad Handling speeds. If so, we want to split this up for use with mods and such later.
			if (objXmlVehicle["handling"].InnerText.Contains('/'))
			{
				_intHandling = Convert.ToInt32(objXmlVehicle["handling"].InnerText.Split('/')[0]);
				_intOffroadHandling = Convert.ToInt32(objXmlVehicle["handling"].InnerText.Split('/')[1]);
			}
			else
			{
				_intHandling = Convert.ToInt32(objXmlVehicle["handling"].InnerText);
			}
			_intAccel = Convert.ToInt32(objXmlVehicle["accel"].InnerText);
			_intSpeed = Convert.ToInt32(objXmlVehicle["speed"].InnerText);
			_intPilot = Convert.ToInt32(objXmlVehicle["pilot"].InnerText);
			_intBody = Convert.ToInt32(objXmlVehicle["body"].InnerText);
			_intArmor = Convert.ToInt32(objXmlVehicle["armor"].InnerText);
			_intSensor = Convert.ToInt32(objXmlVehicle["sensor"].InnerText);
			objXmlVehicle.TryGetField("devicerating", out _intDeviceRating);
			objXmlVehicle.TryGetField("seats", out _intSeats);
			objXmlVehicle.TryGetField("modslots", out _intDroneModSlots,_intBody);
			objXmlVehicle.TryGetField("powertrainmodslots", out _intAddPowertrainModSlots);
			objXmlVehicle.TryGetField("protectionmodslots", out _intAddProtectionModSlots);
			objXmlVehicle.TryGetField("weaponmodslots", out _intAddWeaponModSlots);
			objXmlVehicle.TryGetField("bodymodslots", out _intAddBodyModSlots);
			objXmlVehicle.TryGetField("electromagneticmodslots", out _intAddElectromagneticModSlots);
			objXmlVehicle.TryGetField("cosmeticmodslots", out _intAddCosmeticModSlots);
			_strAvail = objXmlVehicle["avail"].InnerText;
			_strCost = objXmlVehicle["cost"].InnerText;
			// Check for a Variable Cost.
			if (objXmlVehicle["cost"].InnerText.StartsWith("Variable"))
			{
				int intMin = 0;
				int intMax = 0;
				string strCost = objXmlVehicle["cost"].InnerText.Replace("Variable(", string.Empty).Replace(")", string.Empty);
				if (strCost.Contains("-"))
				{
					string[] strValues = strCost.Split('-');
					intMin = Convert.ToInt32(strValues[0]);
					intMax = Convert.ToInt32(strValues[1]);
				}
				else
					intMin = Convert.ToInt32(strCost.Replace("+", string.Empty));

				if (intMin != 0 || intMax != 0)
				{
					frmSelectNumber frmPickNumber = new frmSelectNumber();
					if (intMax == 0)
						intMax = 1000000;
					frmPickNumber.Minimum = intMin;
					frmPickNumber.Maximum = intMax;
					frmPickNumber.Description = LanguageManager.Instance.GetString("String_SelectVariableCost").Replace("{0}", DisplayNameShort);
					frmPickNumber.AllowCancel = false;
					frmPickNumber.ShowDialog();
					_strCost = frmPickNumber.SelectedValue.ToString();
				}
			}
			_strSource = objXmlVehicle["source"].InnerText;
			_strPage = objXmlVehicle["page"].InnerText;

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("vehicles.xml");
				XmlNode objVehicleNode = objXmlDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + _strName + "\"]");
				if (objVehicleNode != null)
				{
					if (objVehicleNode["translate"] != null)
						_strAltName = objVehicleNode["translate"].InnerText;
					if (objVehicleNode["altpage"] != null)
						_strAltPage = objVehicleNode["altpage"].InnerText;
				}

				objVehicleNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
				if (objVehicleNode != null)
				{
					if (objVehicleNode.Attributes["translate"] != null)
						_strAltCategory = objVehicleNode.Attributes["translate"].InnerText;
				}
			}

			objNode.Text = DisplayName;
			objNode.Tag = _guiID.ToString();

			// If there are any VehicleMods that come with the Vehicle, add them.
			if (objXmlVehicle.InnerXml.Contains("<mods>") && blnCreateChildren)
			{
				XmlDocument objXmlDocument = new XmlDocument();
				objXmlDocument = XmlManager.Instance.Load("vehicles.xml");

				XmlNodeList objXmlModList = objXmlVehicle.SelectNodes("mods/name");
				foreach (XmlNode objXmlVehicleMod in objXmlModList)
				{
					XmlNode objXmlMod = objXmlDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + objXmlVehicleMod.InnerText + "\"]");
					if (objXmlMod != null)
					{
						TreeNode objModNode = new TreeNode();
						VehicleMod objMod = new VehicleMod(_objCharacter);
						int intRating = 0;

						if (objXmlVehicleMod.Attributes["rating"] != null)
							intRating = Convert.ToInt32(objXmlVehicleMod.Attributes["rating"].InnerText);

						if (objXmlVehicleMod.Attributes["select"] != null)
							objMod.Extra = objXmlVehicleMod.Attributes["select"].InnerText;

						objMod.Create(objXmlMod, objModNode, intRating);
						objMod.IncludedInVehicle = true;

						_lstVehicleMods.Add(objMod);
						objModNode.ForeColor = SystemColors.GrayText;
						objModNode.ContextMenuStrip = cmsVehicle;

						objNode.Nodes.Add(objModNode);
						objNode.Expand();
					}
				}
				if (objXmlVehicle.SelectSingleNode("mods/addslots") != null)
					_intAddSlots = Convert.ToInt32(objXmlVehicle.SelectSingleNode("mods/addslots").InnerText);
			}

			// If there is any Gear that comes with the Vehicle, add them.
			if (objXmlVehicle.InnerXml.Contains("<gears>") && blnCreateChildren)
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("gear.xml");

				XmlNodeList objXmlGearList = objXmlVehicle.SelectNodes("gears/gear");
				foreach (XmlNode objXmlVehicleGear in objXmlGearList)
				{
					XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlVehicleGear.InnerText + "\"]");
					if (objXmlGear != null)
					{
						TreeNode objGearNode = new TreeNode();
						Gear objGear = new Gear(_objCharacter);
						int intRating = 0;
						int intQty = 1;
						string strForceValue = "";

						if (objXmlVehicleGear.Attributes["rating"] != null)
							intRating = Convert.ToInt32(objXmlVehicleGear.Attributes["rating"].InnerText);

						int intMaxRating = intRating;
						if (objXmlVehicleGear.Attributes["maxrating"] != null)
							intMaxRating = Convert.ToInt32(objXmlVehicleGear.Attributes["maxrating"].InnerText);

						if (objXmlVehicleGear.Attributes["qty"] != null)
							intQty = Convert.ToInt32(objXmlVehicleGear.Attributes["qty"].InnerText);

						if (objXmlVehicleGear.Attributes["select"] != null)
							strForceValue = objXmlVehicleGear.Attributes["select"].InnerText;
						else
							strForceValue = "";

						List<Weapon> objWeapons = new List<Weapon>();
						List<TreeNode> objWeaponNodes = new List<TreeNode>();
						objGear.Create(objXmlGear, _objCharacter, objGearNode, intRating, objWeapons, objWeaponNodes, strForceValue);
						objGear.Cost = "0";
						objGear.Quantity = intQty;
						objGear.MaxRating = intMaxRating;
						objGear.IncludedInParent = true;
						objGearNode.Text = objGear.DisplayName;
						objGearNode.ContextMenuStrip = cmsVehicleGear;

						foreach (Weapon objWeapon in objWeapons)
							objWeapon.VehicleMounted = true;

						_lstGear.Add(objGear);

						objNode.Nodes.Add(objGearNode);
						objNode.Expand();
					}
				}
			}

			// If there are any Weapons that come with the Vehicle, add them.
			if (objXmlVehicle.InnerXml.Contains("<weapons>") && blnCreateChildren)
			{
				XmlDocument objXmlWeaponDocument = XmlManager.Instance.Load("weapons.xml");

				foreach (XmlNode objXmlWeapon in objXmlVehicle.SelectNodes("weapons/weapon"))
				{
					bool blnAttached = false;
					TreeNode objWeaponNode = new TreeNode();
					Weapon objWeapon = new Weapon(_objCharacter);

					XmlNode objXmlWeaponNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlWeapon["name"].InnerText + "\"]");
					objWeapon.Create(objXmlWeaponNode, _objCharacter, objWeaponNode, cmsVehicleWeapon, cmsVehicleWeaponAccessory);
					objWeapon.Cost = 0;
					objWeapon.VehicleMounted = true;

					// Find the first free Weapon Mount in the Vehicle.
					foreach (VehicleMod objMod in _lstVehicleMods)
					{
						if ((objMod.Name.Contains("Weapon Mount") || (!String.IsNullOrEmpty(objMod.WeaponMountCategories) && objMod.WeaponMountCategories.Contains(objWeapon.Category) && objMod.Weapons.Count == 0)))
						{
							objMod.Weapons.Add(objWeapon);
							foreach (TreeNode objModNode in objNode.Nodes)
							{
								if (objModNode.Tag.ToString() == objMod.InternalId)
								{
									objWeaponNode.ContextMenuStrip = cmsVehicleWeapon;
									objModNode.Nodes.Add(objWeaponNode);
									objModNode.Expand();
									blnAttached = true;
									break;
								}
							}
							break;
						}
					}

					// If a free Weapon Mount could not be found, just attach it to the first one found and let the player deal with it.
					if (!blnAttached)
					{
						foreach (VehicleMod objMod in _lstVehicleMods)
						{
							if (objMod.Name.Contains("Weapon Mount") || (!String.IsNullOrEmpty(objMod.WeaponMountCategories) && objMod.WeaponMountCategories.Contains(objWeapon.Category)))
							{
								objMod.Weapons.Add(objWeapon);
								foreach (TreeNode objModNode in objNode.Nodes)
								{
									if (objModNode.Tag.ToString() == objMod.InternalId)
									{
										objWeaponNode.ContextMenuStrip = cmsVehicleWeapon;
										objModNode.Nodes.Add(objWeaponNode);
										objModNode.Expand();
										blnAttached = true;
										break;
									}
								}
								break;
							}
						}
					}

					// Look for Weapon Accessories.
					if (objXmlWeapon["accessories"] != null)
					{
						foreach (XmlNode objXmlAccessory in objXmlWeapon.SelectNodes("accessories/accessory"))
						{
							XmlNode objXmlAccessoryNode = objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" + objXmlAccessory["name"].InnerText + "\"]");
							WeaponAccessory objMod = new WeaponAccessory(_objCharacter);
							TreeNode objModNode = new TreeNode();
							string strMount = "";
							int intRating = 0;
							if (objXmlAccessory["mount"] != null)
								strMount = objXmlAccessory["mount"].InnerText;
							objMod.Create(objXmlAccessoryNode, objModNode, strMount,intRating);
							objMod.Cost = "0";
							objModNode.ContextMenuStrip = cmsVehicleWeaponAccessory;

							objWeapon.WeaponAccessories.Add(objMod);

							objWeaponNode.Nodes.Add(objModNode);
							objWeaponNode.Expand();
						}
					}
				}
			}
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("vehicle");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("category", _strCategory);
			objWriter.WriteElementString("handling", _intHandling.ToString());
			objWriter.WriteElementString("offroadhandling", _intOffroadHandling.ToString());
			objWriter.WriteElementString("accel", _intAccel.ToString());
			objWriter.WriteElementString("speed", _intSpeed.ToString());
			objWriter.WriteElementString("pilot", _intPilot.ToString());
			objWriter.WriteElementString("body", _intBody.ToString());
			objWriter.WriteElementString("seats", _intSeats.ToString());
			objWriter.WriteElementString("armor", _intArmor.ToString());
			objWriter.WriteElementString("sensor", _intSensor.ToString());
			objWriter.WriteElementString("devicerating", TotalDeviceRating.ToString());
			objWriter.WriteElementString("avail", _strAvail);
			objWriter.WriteElementString("cost", _strCost);
			objWriter.WriteElementString("addslots", _intAddSlots.ToString());
			objWriter.WriteElementString("modslots", _intDroneModSlots.ToString());
			objWriter.WriteElementString("powertrainmodslots", _intAddPowertrainModSlots.ToString());
			objWriter.WriteElementString("protectionmodslots", _intAddProtectionModSlots.ToString());
			objWriter.WriteElementString("weaponmodslots", _intAddWeaponModSlots.ToString());
			objWriter.WriteElementString("bodymodslots", _intAddBodyModSlots.ToString());
			objWriter.WriteElementString("electromagneticmodslots", _intAddElectromagneticModSlots.ToString());
			objWriter.WriteElementString("cosmeticmodslots", _intAddCosmeticModSlots.ToString());
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			objWriter.WriteElementString("physicalcmfilled", _intPhysicalCMFilled.ToString());
			objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString());
			objWriter.WriteElementString("vehiclename", _strVehicleName);
			objWriter.WriteElementString("homenode", _blnHomeNode.ToString());
			objWriter.WriteStartElement("mods");
			foreach (VehicleMod objMod in _lstVehicleMods)
				objMod.Save(objWriter);
			objWriter.WriteEndElement();
			objWriter.WriteStartElement("gears");
			foreach (Gear objGear in _lstGear)
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
			objWriter.WriteStartElement("weapons");
			foreach (Weapon objWeapon in _lstWeapons)
				objWeapon.Save(objWriter);
			objWriter.WriteEndElement();
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteElementString("discountedcost", DealerConnectionDiscount.ToString());
			if (_lstLocations.Count > 0)
			{
				// <locations>
				objWriter.WriteStartElement("locations");
				foreach (string strLocation in _lstLocations)
				{
					objWriter.WriteElementString("location", strLocation);
				}
				// </locations>
				objWriter.WriteEndElement();
			}
			objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		/// <summary>
		/// Load the Vehicle from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode, bool blnCopy = false)
		{
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strCategory = objNode["category"].InnerText;
			//Some vehicles have different Offroad Handling speeds. If so, we want to split this up for use with mods and such later.
			if (objNode["handling"].InnerText.Contains('/'))
			{
				_intHandling = Convert.ToInt32(objNode["handling"].InnerText.Split('/')[0]);
				_intOffroadHandling = Convert.ToInt32(objNode["handling"].InnerText.Split('/')[1]);
			}
			else
			{
				_intHandling = Convert.ToInt32(objNode["handling"].InnerText);
				if (objNode.InnerXml.Contains("offroadhandling"))
				{
					_intOffroadHandling = Convert.ToInt32(objNode["offroadhandling"].InnerText);
				}
			}
			_intAccel = Convert.ToInt32(objNode["accel"].InnerText);
			objNode.TryGetField("seats", out _intSeats);
			_intSpeed = Convert.ToInt32(objNode["speed"].InnerText);
			_intPilot = Convert.ToInt32(objNode["pilot"].InnerText);
			_intBody = Convert.ToInt32(objNode["body"].InnerText);
			_intArmor = Convert.ToInt32(objNode["armor"].InnerText);
			_intSensor = Convert.ToInt32(objNode["sensor"].InnerText);
			objNode.TryGetField("devicerating", out _intDeviceRating);
			_strAvail = objNode["avail"].InnerText;
			_strCost = objNode["cost"].InnerText;
			objNode.TryGetField("addslots", out _intAddSlots);
			objNode.TryGetField("modslots", out _intDroneModSlots);
			objNode.TryGetField("powertrainmodslots", out _intAddPowertrainModSlots);
			objNode.TryGetField("protectionmodslots", out _intAddProtectionModSlots);
			objNode.TryGetField("weaponmodslots", out _intAddWeaponModSlots);
			objNode.TryGetField("bodymodslots", out _intAddBodyModSlots);
			objNode.TryGetField("electromagneticmodslots", out _intAddElectromagneticModSlots);
			objNode.TryGetField("cosmeticmodslots", out _intAddCosmeticModSlots);
			_strSource = objNode["source"].InnerText;
			objNode.TryGetField("page", out _strPage);
			objNode.TryGetField("matrixcmfilled", out _intMatrixCMFilled);
			objNode.TryGetField("physicalcmfilled", out _intPhysicalCMFilled);
			objNode.TryGetField("vehiclename", out _strVehicleName);
			objNode.TryGetField("homenode", out _blnHomeNode);

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("vehicles.xml");
				XmlNode objVehicleNode = objXmlDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + _strName + "\"]");
				if (objVehicleNode != null)
				{
					if (objVehicleNode["translate"] != null)
						_strAltName = objVehicleNode["translate"].InnerText;
					if (objVehicleNode["altpage"] != null)
						_strAltPage = objVehicleNode["altpage"].InnerText;
				}

				objVehicleNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
				if (objVehicleNode != null)
				{
					if (objVehicleNode.Attributes["translate"] != null)
						_strAltCategory = objVehicleNode.Attributes["translate"].InnerText;
				}
			}

			if (objNode.InnerXml.Contains("<mods>"))
			{
				XmlNodeList nodChildren = objNode.SelectNodes("mods/mod");
				foreach (XmlNode nodChild in nodChildren)
				{
					VehicleMod objMod = new VehicleMod(_objCharacter);
					objMod.Load(nodChild, blnCopy);
					_lstVehicleMods.Add(objMod);
				}
			}

			if (objNode.InnerXml.Contains("<gears>"))
			{
				XmlNodeList nodChildren = objNode.SelectNodes("gears/gear");
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
							_lstGear.Add(objCommlink);
							break;
						default:
							Gear objGear = new Gear(_objCharacter);
							objGear.Load(nodChild, blnCopy);
							_lstGear.Add(objGear);
							break;
					}
				}
			}

			if (objNode.InnerXml.Contains("<weapons>"))
			{
				XmlNodeList nodChildren = objNode.SelectNodes("weapons/weapon");
				foreach (XmlNode nodChild in nodChildren)
				{
					Weapon objWeapon = new Weapon(_objCharacter);
					objWeapon.Load(nodChild, blnCopy);
					objWeapon.VehicleMounted = true;
					if (objWeapon.UnderbarrelWeapons.Count > 0)
					{
						foreach (Weapon objUnderbarrel in objWeapon.UnderbarrelWeapons)
							objUnderbarrel.VehicleMounted = true;
					}
					_lstWeapons.Add(objWeapon);
				}
			}

			objNode.TryGetField("notes", out _strNotes);
			objNode.TryGetField("dealerconnection", out _blnDealerConnectionDiscount);

			if (objNode["locations"] != null)
			{
				// Locations.
				foreach (XmlNode objXmlLocation in objNode.SelectNodes("locations/location"))
				{
					_lstLocations.Add(objXmlLocation.InnerText);
				}
			}

			if (blnCopy)
			{
				_guiID = Guid.NewGuid();
				_blnHomeNode = false;
			}
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("vehicle");
			objWriter.WriteElementString("name", DisplayNameShort);
			objWriter.WriteElementString("category", DisplayCategory);
			objWriter.WriteElementString("handling", TotalHandling.ToString());
			objWriter.WriteElementString("accel", TotalAccel.ToString());
			objWriter.WriteElementString("speed", TotalSpeed.ToString());
			objWriter.WriteElementString("pilot", Pilot.ToString());
			objWriter.WriteElementString("body", TotalBody.ToString());
			objWriter.WriteElementString("armor", TotalArmor.ToString());
			objWriter.WriteElementString("seats", _intSeats.ToString());
			objWriter.WriteElementString("sensor", _intSensor.ToString());
			objWriter.WriteElementString("avail", CalculatedAvail);
			objWriter.WriteElementString("cost", TotalCost.ToString());
			objWriter.WriteElementString("owncost", OwnCost.ToString());
			objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
			objWriter.WriteElementString("physicalcm", PhysicalCM.ToString());
			objWriter.WriteElementString("matrixcm", ConditionMonitor.ToString());
			objWriter.WriteElementString("physicalcmfilled", _intPhysicalCMFilled.ToString());
			objWriter.WriteElementString("vehiclename", _strVehicleName);
			objWriter.WriteElementString("devicerating", TotalDeviceRating.ToString());
			objWriter.WriteElementString("maneuver", Maneuver.ToString());
			objWriter.WriteElementString("homenode", _blnHomeNode.ToString());
			objWriter.WriteStartElement("mods");
			foreach (VehicleMod objMod in _lstVehicleMods)
				objMod.Print(objWriter);
			objWriter.WriteEndElement();
			objWriter.WriteStartElement("gears");
			foreach (Gear objGear in _lstGear)
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
			objWriter.WriteEndElement();
			objWriter.WriteStartElement("weapons");
			foreach (Weapon objWeapon in _lstWeapons)
				objWeapon.Print(objWriter);
			objWriter.WriteEndElement();
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this piece of Gear in the Character.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// Matrix Condition Monitor for the Vehicle.
		/// </summary>
		public int ConditionMonitor
		{
			get
			{
				double dblSystem = Math.Ceiling(Convert.ToDouble(DeviceRating, GlobalOptions.Instance.CultureInfo) / 2);
				int intSystem = Convert.ToInt32(dblSystem, GlobalOptions.Instance.CultureInfo);
				return 8 + intSystem;
			}
		}

		/// <summary>
		/// Name.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		/// <summary>
		/// Translated Category.
		/// </summary>
		public string DisplayCategory
		{
			get
			{
				string strReturn = _strCategory;
				if (_strAltCategory != string.Empty)
					strReturn = _strAltCategory;

				return strReturn;
			}
		}

		/// <summary>
		/// Category.
		/// </summary>
		public string Category
		{
			get
			{
				return _strCategory;
			}
			set
			{
				_strCategory = value;
			}
		}

		/// <summary>
		/// Is this vehicle a drone?
		/// </summary>
		public Boolean IsDrone
		{
			get
			{
				return Category.Contains("Drone");
			}
		}

		/// <summary>
		/// Handling.
		/// </summary>
		public int Handling
		{
			get
			{
				return _intHandling;
			}
			set
			{
				_intHandling = value;
			}
		}


		/// <summary>
		/// Seats.
		/// </summary>
		public int Seats
		{
			get
			{
				return _intSeats;
			}
			set
			{
				_intSeats = value;
			}
		}


		/// <summary>
		/// Offroad Handling.
		/// </summary>
		public int OffroadHandling
		{
			get
			{
				return _intOffroadHandling;
			}
			set
			{
				_intOffroadHandling = value;
			}
		}

		/// <summary>
		/// Acceleration.
		/// </summary>
		public int Accel
		{
			get
			{
				return _intAccel;
			}
			set
			{
				_intAccel = value;
			}
		}

		/// <summary>
		/// Speed.
		/// </summary>
		public int Speed
		{
			get
			{
				return _intSpeed;
			}
			set
			{
				_intSpeed = value;
			}
		}

		/// <summary>
		/// Pilot.
		/// </summary>
		public int Pilot
		{
			get
			{
				int intReturn = _intPilot;
				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					if (!objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null)
					{
						// Set the Vehicle's Pilot to the Modification's bonus.
						if (objMod.Bonus.InnerXml.Contains("<pilot>"))
						{
							int intTest = Convert.ToInt32(objMod.Bonus["pilot"].InnerText.Replace("Rating", objMod.Rating.ToString()));
							if (intTest > intReturn)
								intReturn = intTest;
						}
					}
				}
				return intReturn;
			}
			set
			{
				_intPilot = value;
			}
		}

		/// <summary>
		/// Body.
		/// </summary>
		public int Body
		{
			get
			{
				return _intBody;
			}
			set
			{
				_intBody = value;
			}
		}

		/// <summary>
		/// Armor.
		/// </summary>
		public int Armor
		{
			get
			{
				return _intArmor;
			}
			set
			{
				_intArmor = value;
			}
		}

		/// <summary>
		/// Sensor.
		/// </summary>
		public int BaseSensor
		{
			get
			{
				return _intSensor;
			}
			set
			{
				_intSensor = value;
			}
		}

		/// <summary>
		/// Device Rating.
		/// </summary>
		public int DeviceRating
		{
			get
			{
				int intDeviceRating = _intDeviceRating;

				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					if (objMod.Bonus != null)
					{
						// Add the Modification's Device Rating to the Vehicle's base Device Rating.
						if (objMod.Bonus.InnerXml.Contains("<devicerating>"))
							intDeviceRating += Convert.ToInt32(objMod.Bonus["devicerating"].InnerText);
					}
				}

				// Device Rating cannot go below 1.
				if (intDeviceRating < 1)
					intDeviceRating = 1;

				return intDeviceRating;
			}
			set
			{
				_intDeviceRating = value;
			}
		}

		/// <summary>
		/// Base Physical Boxes. 12 for vehicles, 6 for Drones.
		/// </summary>
		public int BaseMatrixBoxes
		{
			get
			{
				int baseMatrixBoxes = 8;
				return baseMatrixBoxes;
			}
		}

		/// <summary>
		/// Physical Condition Monitor boxes.
		/// </summary>
		public int MatrixCM
		{
			get
			{
				return BaseMatrixBoxes + Convert.ToInt32(Math.Ceiling(Convert.ToDouble(_intDeviceRating, GlobalOptions.Instance.CultureInfo) / 2.0));
			}
		}

		/// <summary>
		/// Matrix Condition Monitor boxes filled.
		/// </summary>
		public int MatrixCMFilled
		{
			get
			{
				return _intMatrixCMFilled;
			}
			set
			{
				_intMatrixCMFilled = value;
			}
		}

		/// <summary>
		/// Base Physical Boxes. 12 for vehicles, 6 for Drones.
		/// </summary>
		public int BasePhysicalBoxes
		{
			get
			{
				int basePhysicalBoxes = 12;

				if (this.IsDrone)
				{
					basePhysicalBoxes = 6;
				}
				return basePhysicalBoxes;
			}
		}

		/// <summary>
		/// Physical Condition Monitor boxes.
		/// </summary>
		public int PhysicalCM
		{
			get
			{
				return BasePhysicalBoxes + Convert.ToInt32(Math.Ceiling(Convert.ToDouble(_intBody, GlobalOptions.Instance.CultureInfo) / 2.0));
			}
		}

		/// <summary>
		/// Physical Condition Monitor boxes filled.
		/// </summary>
		public int PhysicalCMFilled
		{
			get
			{
				return _intPhysicalCMFilled;
			}
			set
			{
				_intPhysicalCMFilled = value;
			}
		}

		/// <summary>
		/// Availability.
		/// </summary>
		public string Avail
		{
			get
			{
				return _strAvail;
			}
			set
			{
				_strAvail = value;
			}
		}

		/// <summary>
		/// Cost.
		/// </summary>
		public string Cost
		{
			get
			{
				return _strCost;
			}
			set
			{
				_strCost = value;
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
			set
			{
				_strSource = value;
			}
		}

		/// <summary>
		/// Sourcebook Page Number.
		/// </summary>
		public string Page
		{
			get
			{
				string strReturn = _strPage;
				if (_strAltPage != string.Empty)
					strReturn = _strAltPage;

				return strReturn;
			}
			set
			{
				_strPage = value;
			}
		}

		/// <summary>
		/// Vehicle Modifications applied to the Vehicle.
		/// </summary>
		public List<VehicleMod> Mods
		{
			get
			{
				return _lstVehicleMods;
			}
		}

		/// <summary>
		/// Gear applied to the Vehicle.
		/// </summary>
		public List<Gear> Gear
		{
			get
			{
				return _lstGear;
			}
		}

		/// <summary>
		/// Weapons applied to the Vehicle through Gear.
		/// </summary>
		public List<Weapon> Weapons
		{
			get
			{
				return _lstWeapons;
			}
		}

		/// <summary>
		/// Calculated Availablility of the Vehicle.
		/// </summary>
		public string CalculatedAvail
		{
			get
			{
				string strReturn = _strAvail;

				// Translate the Avail string.
				strReturn = strReturn.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted"));
				strReturn = strReturn.Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));

				return strReturn;
			}
		}

		/// <summary>
		/// Number of Slots the Vehicle has for Modifications.
		/// </summary>
		public int Slots
		{
			get
			{
				// A Vehicle has 4 or BODY slots, whichever is higher.
				if (TotalBody > 4)
					return TotalBody + _intAddSlots;
				else
					return 4 + _intAddSlots;
			}
		}

		/// <summary>
		/// Calculate the Vehicle's Sensor Rating based on the items within its Sensor.
		/// </summary>
		public int CalculatedSensor
		{
			get
			{
				int intSensor = _intSensor;
				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					if (!objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null)
					{
						if (objMod.Bonus.InnerXml.Contains("<sensor>"))
						{
							if (objMod.Bonus["sensor"].InnerText.Contains("+"))
							{
								string strAccel = objMod.Bonus["sensor"].InnerText.Replace("Rating", objMod.Rating.ToString()).Replace("+", string.Empty);
								intSensor += Convert.ToInt32(strAccel, GlobalOptions.Instance.CultureInfo);
							}
							else
							{
								string strAccel = objMod.Bonus["sensor"].InnerText.Replace("Rating", objMod.Rating.ToString()).Replace("+", string.Empty);
								intSensor = Convert.ToInt32(strAccel, GlobalOptions.Instance.CultureInfo);
							}
						}
					}
				}
				
				// Step through all the Gear looking for the Sensor Array that was built it. Set the rating to the current Sensor value.
				// The display value of this gets updated by UpdateSensor when RefreshSelectedVehicle gets called.
				foreach (Gear objGear in _lstGear)
				{
					if (objGear.Category == "Sensors" && objGear.Name == "Sensor Array" && objGear.IncludedInParent)
					{
						if (intSensor != _intSensor)
							objGear.Rating = intSensor;
						else objGear.Rating = _intSensor;
					}
					break;
				}

				return intSensor;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}

		/// <summary>
		/// A custom name for the Vehicle assigned by the player.
		/// </summary>
		public string VehicleName
		{
			get
			{
				return _strVehicleName;
			}
			set
			{
				_strVehicleName = value;
			}
		}

		/// <summary>
		/// The name of the object as it should appear on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
		{
			get
			{
				string strReturn = _strName;
				if (_strAltName != string.Empty)
					strReturn = _strAltName;

				return strReturn;
			}
		}

		/// <summary>
		/// Display name.
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = DisplayNameShort;

				if (_strVehicleName != "")
				{
					strReturn += " (\"" + _strVehicleName + "\")";
				}

				return strReturn;
			}
		}

		/// <summary>
		/// Whether or not the Vehicle is an A.I.'s Home Node.
		/// </summary>
		public bool HomeNode
		{
			get
			{
				return _blnHomeNode;
			}
			set
			{
				_blnHomeNode = value;
			}
		}

		/// <summary>
		/// Locations.
		/// </summary>
		public List<string> Locations
		{
			get
			{
				return _lstLocations;
			}
		}

		/// <summary>
		/// Whether or not the Vehicle's cost should be discounted by 10% through the Dealer Connection Quality.
		/// </summary>
		public bool BlackMarketDiscount
		{
			get
			{
				return _blnBlackMarketDiscount;
			}
			set
			{
				_blnBlackMarketDiscount = value;
			}
		}

		/// <summary>
		/// Whether or not the Vehicle's cost should be discounted by 10% through the Dealer Connection Quality.
		/// </summary>
		public bool DealerConnectionDiscount
		{
			get
			{
				foreach (Improvement objImprovement in _objCharacter.Improvements)
				{
					if (objImprovement.ImproveType == Improvement.ImprovementType.DealerConnection)
					{
						if (
							(objImprovement.ImprovedName == "Drones" && (
								_strCategory.StartsWith("Drones"))) ||
							(objImprovement.ImprovedName == "Aircraft" && (
								_strCategory == "Fixed-Wing Aircraft" ||
								_strCategory == "LTAV" ||
								_strCategory == "Rotorcraft" ||
								_strCategory == "VTOL/VSTOL")) ||
							(objImprovement.ImprovedName == "Watercraft" && (
								_strCategory == "Boats" ||
								_strCategory == "Submarines")) ||
							(objImprovement.ImprovedName == "Groundcraft" && (
								_strCategory == "Bikes" ||
								_strCategory == "Cars" ||
								_strCategory == "Trucks" ||
								_strCategory == "Municipal/Construction" ||
								_strCategory == "Corpsec/Police/Military"))
							)
						{
							_blnDealerConnectionDiscount = true;
						}
					}
				}
				return _blnDealerConnectionDiscount;
			}
			set
			{
				_blnDealerConnectionDiscount = value;
			}
		}
		#endregion

		#region Complex Properties
		/// <summary>
		/// The number of Slots on the Vehicle that are used by Mods.
		/// </summary>
		public int SlotsUsed
		{
			get
			{
				int intSlotsUsed = 0;
				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					// Mods that are included with a Vehicle by default do not count toward the Slots used.
					if (!objMod.IncludedInVehicle && objMod.Installed)
						intSlotsUsed += objMod.CalculatedSlots;
				}

				return intSlotsUsed;
			}
		}

		/// <summary>
		/// Total Number of Slots the Drone has for Modifications. (Rigger 5)
		/// </summary>
		public int DroneModSlots
		{
			get
			{
				int intDroneModSlots = _intDroneModSlots;
				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					// Mods that are included with a Vehicle by default do not count toward the Slots used.
					if (!objMod.IncludedInVehicle && objMod.Installed)
					{
						if (objMod.CalculatedSlots < 0)
							intDroneModSlots -= objMod.CalculatedSlots;
					}
				}
				return intDroneModSlots;
			}
		}

		/// <summary>
		/// The number of Slots on the Drone that are used by Mods.
		/// </summary>
		public int DroneModSlotsUsed
		{
			get
			{
				int intModSlotsUsed = 0;

				bool blnHandling = false;
				bool blnSpeed = false;
				bool blnAccel = false;
				bool blnArmor = false;
				bool blnSensor = false;

				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					if (!objMod.IncludedInVehicle && objMod.Installed)
					{
						int intActualSlots = 0;

						if (objMod.CalculatedSlots > 0)
						{
							if (objMod.Category == "Handling")
							{
								intActualSlots = objMod.CalculatedSlots - _intHandling;
								if (!blnHandling)
								{
									blnHandling = true;
									intActualSlots -= 1;
								}
							}
							else if (objMod.Category == "Speed")
							{
								intActualSlots = objMod.CalculatedSlots - _intSpeed;
								if (!blnSpeed)
								{
									blnSpeed = true;
									intActualSlots -= 1;
								}
							}
							else if (objMod.Category == "Acceleration")
							{
								intActualSlots = objMod.CalculatedSlots - _intAccel;
								if (!blnAccel)
								{
									blnAccel = true;
									intActualSlots -= 1;
								}
							}
							else if (objMod.Category == "Armor")
							{
								int intArmorDiff = objMod.Rating - _intArmor;
								double dblDivThree = intArmorDiff / 3;
								int intThird = (int)Math.Ceiling(dblDivThree);

								if (!blnArmor)
								{
									blnArmor = true;
									intActualSlots = intThird - 1;
								}
								else
								{
									intActualSlots = intThird;
								}
							}
							else if (objMod.Category == "Sensor")
							{
								intActualSlots = objMod.CalculatedSlots - _intSensor;
								if (!blnSensor)
								{
									blnSensor = true;
									intActualSlots -= 1;
								}
							}
							else
							{
								intActualSlots = objMod.CalculatedSlots;
							}

							if (intActualSlots < 0)
								intActualSlots = 0;

							intModSlotsUsed += intActualSlots;
						}
					}
				}
				return intModSlotsUsed;
			}
		}


		/// <summary>
		/// Total cost of the Vehicle including all after-market Modification.
		/// </summary>
		public int TotalCost
		{
			get
			{
				int intCost = Convert.ToInt32(_strCost);
				if (BlackMarketDiscount)
					intCost = Convert.ToInt32(Convert.ToDouble(intCost, GlobalOptions.Instance.CultureInfo) * 0.9);

				if (DealerConnectionDiscount)
					intCost = Convert.ToInt32(Convert.ToDouble(intCost, GlobalOptions.Instance.CultureInfo) * 0.9);

				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					// Do not include the price of Mods that are part of the base configureation.
					if (!objMod.IncludedInVehicle)
					{
						objMod.VehicleCost = Convert.ToInt32(_strCost);
						objMod.Body = _intBody;
						objMod.Speed = _intSpeed;
						objMod.Accel = _intAccel;

						intCost += objMod.TotalCost;
					}
					else
					{
						// If the Mod is a part of the base config, check the items attached to it since their cost still counts.
						foreach (Weapon objWeapon in objMod.Weapons)
							intCost += objWeapon.TotalCost;
						foreach (Cyberware objCyberware in objMod.Cyberware)
							intCost += objCyberware.TotalCost;
					}
				}

				foreach (Gear objGear in _lstGear)
				{
					intCost += objGear.TotalCost;
				}

				return intCost;
			}
		}

		/// <summary>
		/// The cost of just the Vehicle itself.
		/// </summary>
		public int OwnCost
		{
			get
			{
				int intCost = Convert.ToInt32(_strCost);

				if (BlackMarketDiscount)
					intCost = Convert.ToInt32(Convert.ToDouble(intCost, GlobalOptions.Instance.CultureInfo) * 0.9);

				if (DealerConnectionDiscount)
					intCost = Convert.ToInt32(Convert.ToDouble(intCost, GlobalOptions.Instance.CultureInfo) * 0.9);

				return intCost;
			}
		}

		/// <summary>
		/// Total Speed of the Vehicle including Modifications.
		/// </summary>
		public int TotalSpeed
		{
			get
			{
				int intTotalSpeed = _intSpeed;
				int intTotalArmor = 0;
				int intPenalty = 0;

				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					if (!objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null)
					{
						if (objMod.Bonus.InnerXml.Contains("<speed>"))
						{
							//Increase the vehicles base Speed by the Modification's value.
							if (objMod.Bonus["speed"].InnerText.Contains("+"))
							{
								intTotalSpeed += Convert.ToInt32(objMod.Bonus["speed"].InnerText.Replace("Rating", objMod.Rating.ToString()).Replace("+", string.Empty));
							}
							// Add the Vehicle's base Speed to the Modification's Speed adjustment.
							else
							{
								intTotalSpeed = Convert.ToInt32(objMod.Bonus["speed"].InnerText.Replace("Rating", objMod.Rating.ToString()));
							}
						}
						if (objMod.Bonus.InnerXml.Contains("<armor>"))
						{
							if (IsDrone && GlobalOptions.Instance.Dronemods)
							{
								intTotalArmor = Convert.ToInt32(objMod.Bonus["armor"].InnerText.Replace("Rating", objMod.Rating.ToString()));
							}
						}
					}
				}

				if (intTotalArmor > _intBody * 3)
				{
					// Reduce speed of the drone if there is too much armor
					int intExcess = intTotalArmor - (_intBody * 3);
					double dblResult = intExcess / 3;
					intPenalty = (int) Math.Floor(dblResult);
				}

				return intTotalSpeed - intPenalty;
			}
		}

		/// <summary>
		/// Total Accel of the Vehicle including Modifications.
		/// </summary>
		public int TotalAccel
		{
			get
			{
				int intTotalAccel = _intAccel;
				int intTotalArmor = 0;
				int intPenalty = 0;


				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					if (!objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null)
					{
						// Multiply the Vehicle's base Accel by the Modification's Accel multiplier.
						if (objMod.Bonus.InnerXml.Contains("<accel>"))
						{
							if (objMod.Bonus["accel"].InnerText.Contains("+"))
							{
								string strAccel = objMod.Bonus["accel"].InnerText.Replace("Rating", objMod.Rating.ToString()).Replace("+", string.Empty);
								intTotalAccel += Convert.ToInt32(strAccel, GlobalOptions.Instance.CultureInfo);
							}
							else
							{
								string strAccel = objMod.Bonus["accel"].InnerText.Replace("Rating", objMod.Rating.ToString()).Replace("+", string.Empty);
								intTotalAccel = Convert.ToInt32(strAccel, GlobalOptions.Instance.CultureInfo);
							}
						}
						if (objMod.Bonus.InnerXml.Contains("<armor>"))
						{
							if (IsDrone && GlobalOptions.Instance.Dronemods)
							{
								intTotalArmor = Convert.ToInt32(objMod.Bonus["armor"].InnerText.Replace("Rating", objMod.Rating.ToString()));
							}
						}
					}
				}

				if (intTotalArmor > _intBody * 3)
				{
					// Reduce speed of the drone if there is too much armor
					int intExcess = intTotalArmor - (_intBody * 3);
					double dblResult = intExcess / 6;
					intPenalty = (int)Math.Floor(dblResult);
				}

				return intTotalAccel - intPenalty;
			}
		}

		/// <summary>
		/// Total Body of the Vehicle including Modifications.
		/// </summary>
		public int TotalBody
		{
			get
			{
				int intBody = _intBody;

				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					if (!objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null)
					{
						// Add the Modification's Body to the Vehicle's base Body.
						
						if (objMod.Bonus.InnerXml.Contains("<body>") && objMod.Bonus["body"].InnerText.Contains("Rating"))
						{
							// If the cost is determined by the Rating, evaluate the expression.
							XmlDocument objXmlDocument = new XmlDocument();
							XPathNavigator nav = objXmlDocument.CreateNavigator();

							string strBody = objMod.Bonus["body"].InnerText.Replace("Rating", objMod.Rating.ToString());
							XPathExpression xprBody = nav.Compile(strBody);
							intBody += Convert.ToInt32(nav.Evaluate(xprBody).ToString());
						}
					}
				}

				return intBody;
			}
		}

		/// <summary>
		/// Total Handling of the Vehicle including Modifications.
		/// </summary>
		public string TotalHandling
		{
			get
			{
				string strHandling = "";
				int intBaseHandling = _intHandling;
				int intBaseOffroadHandling = _intOffroadHandling;
				int intPenalty = 0;
				int intTotalArmor = 0;

				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					if (!objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null)
					{
						// Add the Modification's Handling to the Vehicle's base Handling.
						if (objMod.Bonus.InnerXml.Contains("<handling>"))
						{
							if (objMod.Bonus["handling"].InnerText.Contains("+"))
							{
								string strHandle = objMod.Bonus["handling"].InnerText.Replace("Rating", objMod.Rating.ToString()).Replace("+", string.Empty);
								intBaseHandling += Convert.ToInt32(strHandle, GlobalOptions.Instance.CultureInfo);
							}
							else intBaseHandling = Convert.ToInt32(objMod.Bonus["handling"].InnerText.Replace("Rating", objMod.Rating.ToString()));
						}
						if (objMod.Bonus.InnerXml.Contains("<offroadhandling>"))
						{
							if (objMod.Bonus["offroadhandling"].InnerText.Contains("+"))
							{
								string strHandle = objMod.Bonus["offroadhandling"].InnerText.Replace("Rating", objMod.Rating.ToString()).Replace("+", string.Empty);
								intBaseOffroadHandling += Convert.ToInt32(strHandle, GlobalOptions.Instance.CultureInfo);
							}
							else intBaseOffroadHandling = Convert.ToInt32(objMod.Bonus["offroadhandling"].InnerText.Replace("Rating", objMod.Rating.ToString()));
						}
						if (objMod.Bonus.InnerXml.Contains("<armor>"))
						{
							if (IsDrone && GlobalOptions.Instance.Dronemods)
							{
								intTotalArmor = Convert.ToInt32(objMod.Bonus["armor"].InnerText.Replace("Rating", objMod.Rating.ToString()));
							}
						}
					}
				}

				if (intTotalArmor > _intBody * 3)
				{
					// Reduce speed of the drone if there is too much armor
					int intExcess = intTotalArmor - (_intBody * 3);
					double dblResult = intExcess / 3;
					intPenalty = (int)Math.Floor(dblResult);
				}

				if (_intOffroadHandling > 0)
				{
					strHandling = ((intBaseHandling - intPenalty).ToString() + '/' + (intBaseOffroadHandling - intPenalty).ToString());
				}
				else
				{
					strHandling = ((intBaseHandling - intPenalty).ToString());
				}

				return strHandling;
			}
		}

		/// <summary>
		/// Total Armor of the Vehicle including Modifications.
		/// </summary>
		public int TotalArmor
		{
			get
			{
				int intModArmor = 0;
				bool blnArmorMod = false;

				// Rigger5 Drone Armor starts at 0. All other vehicles start with their base armor.
				if (IsDrone && GlobalOptions.Instance.Dronemods)
					intModArmor = 0;
				else
					intModArmor = _intArmor;

				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					if (!objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null)
					{
						blnArmorMod = true;
						// Add the Modification's Armor to the Vehicle's base Armor. 
						if (objMod.Bonus.InnerXml.Contains("<armor>"))
						{
							//intBaseArmor = 0;
							intModArmor += Convert.ToInt32(objMod.Bonus["armor"].InnerText.Replace("Rating", objMod.Rating.ToString()));
						}
					}
				}
				// Drones have no theoretical armor cap in the optional rules, otherwise, it's capped
				if (!IsDrone || !GlobalOptions.Instance.Dronemods)
				{
					intModArmor = Math.Min(MaxArmor, intModArmor);
				}
				else if (!blnArmorMod)
				{
					// We're a drone, but we didn't have any mods, so keep the base value
					intModArmor = _intArmor;
				}
				return intModArmor;
			}
		}

		/// <summary>
		/// Maximum amount of each Armor type the Vehicle can hold.
		/// </summary>
		public int MaxArmor
		{
			get
			{
				int intReturn = 0;

				// Rigger 5 says max armor is Body + starting Armor, p159
				intReturn = _intBody + _intArmor;

				if (IsDrone)
				{
					if (_objCharacter.Options.DroneArmorMultiplierEnabled)
					{
						intReturn = _intArmor*_objCharacter.Options.DroneArmorMultiplier;
					}
					else
					{
						intReturn = _intArmor*2;
					}
				}

				// If ignoring the rules, do not limit Armor to the Vehicle's standard rules.
				if (_objCharacter.IgnoreRules)
					intReturn = 99;

				return intReturn;
			}
		}

		/// <summary>
		/// Check if the vehicle is over capacity in any category
		/// </summary>
		public bool OverR5Capacity
		{
			get
			{
				bool blnOverCapacity = false;
				string[] arrCategories = new string[6] { "Powertrain", "Protection", "Weapons", "Body", "Electromagnetic", "Cosmetic" };

				foreach (string strCategory in arrCategories)
				{
					if (CalcCategoryUsed(strCategory) > CalcCategoryAvail(strCategory))
						blnOverCapacity = true;
				}

				return blnOverCapacity;
			}
		}


		/// <summary>
		/// Calculate remaining Powertrain slots
		/// </summary>
		public int PowertrainModSlots
		{
			get
			{
				int intPowertrain = _intBody + _intAddPowertrainModSlots;

				foreach (VehicleMod objMod in _lstVehicleMods)
				{

					if (!objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == "Powertrain"))
					{
						// Subtract the Modification's Slots from the Vehicle's base Body.
						if (objMod.CalculatedSlots > 0)
							intPowertrain -= Convert.ToInt32(objMod.CalculatedSlots);
					}
				}

				return intPowertrain;
			}
		}

		/// <summary>
		/// Calculate remaining Protection slots
		/// </summary>
		public int ProtectionModSlots
		{
			get
			{
				int intProtection = _intBody + _intAddProtectionModSlots;

				foreach (VehicleMod objMod in _lstVehicleMods)
				{

					if (!objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == "Protection"))
					{
						// Subtract the Modification's Slots from the Vehicle's base Body.
						if (objMod.CalculatedSlots > 0)
							intProtection -= Convert.ToInt32(objMod.CalculatedSlots);
					}
				}

				return intProtection;
			}
		}

		/// <summary>
		/// Calculate remaining Weapon slots
		/// </summary>
		public int WeaponModSlots
		{
			get
			{
				int intWeaponsmod = _intBody + _intAddWeaponModSlots;

				foreach (VehicleMod objMod in _lstVehicleMods)
				{

					if (!objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == "Weapons"))
					{
						// Subtract the Modification's Slots from the Vehicle's base Body.
						if (objMod.CalculatedSlots > 0)
							intWeaponsmod -= Convert.ToInt32(objMod.CalculatedSlots);
					}
				}

				return intWeaponsmod;
			}
		}

		/// <summary>
		/// Calculate remaining Bodymod slots
		/// </summary>
		public int BodyModSlots
		{
			get
			{
				int intBodymod = _intBody + _intAddBodyModSlots;

				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					if (!objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == "Body"))
					{
						// Subtract the Modification's Slots from the Vehicle's base Body.
						if (objMod.CalculatedSlots > 0)
							intBodymod -= Convert.ToInt32(objMod.CalculatedSlots);
					}
				}

				return intBodymod;
			}
		}

		/// <summary>
		/// Calculate remaining Electromagnetic slots
		/// </summary>
		public int ElectromagneticModSlots
		{
			get
			{
				int intElectromagnetic = _intBody + _intAddBodyModSlots;

				foreach (VehicleMod objMod in _lstVehicleMods)
				{

					if (!objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == "Electromagnetic"))
					{
						// Subtract the Modification's Slots from the Vehicle's base Body.
						if (objMod.CalculatedSlots > 0)
							intElectromagnetic -= Convert.ToInt32(objMod.CalculatedSlots);
					}
				}

				return intElectromagnetic;
			}
		}

		/// <summary>
		/// Calculate remaining Cosmetic slots
		/// </summary>
		public int CosmeticModSlots
		{
			get
			{
				int intCosmetic = _intBody +_intAddCosmeticModSlots;

				foreach (VehicleMod objMod in _lstVehicleMods)
				{
					if (!objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == "Cosmetic"))
					{
						// Subtract the Modification's Slots from the Vehicle's base Body.
						if (objMod.CalculatedSlots > 0)
							intCosmetic -= Convert.ToInt32(objMod.CalculatedSlots);
					}
				}

				return intCosmetic;
			}
		}

		/// <summary>
		/// Vehicle's Maneuver AutoSoft Rating.
		/// </summary>
		public int Maneuver
		{
			get
			{
				int intReturn = 0;
				Gear objGear = FindGearByName("Maneuver", _lstGear);
				if (objGear != null)
				{
					intReturn = objGear.Rating;
				}

				return intReturn;
			}
		}

		/// <summary>
		/// Total Device Rating.
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
		#endregion

		#region Methods

		/// <summary>
		/// Calculate remaining slots by provided Category
		/// </summary>
		public int CalcCategoryUsed(string strCategory)
		{
			int intBase = 0;

			foreach (VehicleMod objMod in _lstVehicleMods)
			{
				if (!objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == strCategory))
				{
					// Subtract the Modification's Slots from the Vehicle's base Body.
					if (objMod.CalculatedSlots > 0)
						intBase += Convert.ToInt32(objMod.CalculatedSlots);
				}
			}

			return intBase;
		}

		/// <summary>
		/// Total Number of Slots a Vehicle has used for Modifications. (Rigger 5)
		/// </summary>
		public int CalcCategoryAvail(string strCategory)
		{
			int intBase = _intBody;
			foreach (VehicleMod objMod in _lstVehicleMods)
			{
				// Mods that are included with a Vehicle by default do not count toward the Slots used.
				if (!objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == strCategory))
				{
					if (objMod.CalculatedSlots < 0)
						intBase -= objMod.CalculatedSlots;
				}
			}
			return intBase;
		}

		/// <summary>
		/// Whether or not the Vehicle has the Modular Electronics Vehicle Modification installed.
		/// </summary>
		public bool HasModularElectronics()
		{
			bool blnReturn = false;
			foreach (VehicleMod objMod in _lstVehicleMods)
			{
				if (objMod.Name == "Modular Electronics")
				{
					blnReturn = true;
					break;
				}
			}
			return blnReturn;
		}

		/// <summary>
		/// Locate a piece of Gear.
		/// </summary>
		/// <param name="strName">Name of the Gear to find.</param>
		/// <param name="lstGear">List of Gear to search.</param>
		private Gear FindGearByName(string strName, List<Gear> lstGear)
		{
			Gear objReturn = new Gear(_objCharacter);
			foreach (Gear objGear in lstGear)
			{
				if (objGear.Name == strName)
					objReturn = objGear;
				else
				{
					if (objGear.Children.Count > 0)
						objReturn = FindGearByName(strName, objGear.Children);
				}

				if (objReturn.InternalId != Guid.Empty.ToString() && objReturn.Name != "")
					return objReturn;
			}

			return objReturn;
		}
		#endregion
	}
}