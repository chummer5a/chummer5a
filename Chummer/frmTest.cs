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
 using System.Linq;
 using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
 using Chummer.Backend.Equipment;
 using Chummer.Backend.Skills;

namespace Chummer
{
    public partial class frmTest : Form
    {
        public frmTest()
        {
            InitializeComponent();
        }

        private bool _blnAddExceptionInfoToErrors;
        private readonly StringBuilder _objOutputBuilder = new StringBuilder();
        private readonly Character _objCharacter = new Character();

        private void cmdTest_Click(object sender, EventArgs e)
        {
            cmdTest.Enabled = false;
            _blnAddExceptionInfoToErrors = chkAddExceptionInfoToErrors.Checked;
            txtOutput.Text = _blnAddExceptionInfoToErrors
                ? "Testing " + cboTest.Text + " with exception info printed." + Environment.NewLine + Environment.NewLine + "Please wait..."
                : "Testing " + cboTest.Text + "." + Environment.NewLine + Environment.NewLine + "Please wait...";
            _objOutputBuilder.Clear();
            switch (cboTest.Text)
            {
                case "armor.xml":
                    TestArmor();
                    break;
                case "bioware.xml":
                case "cyberware.xml":
                    TestCyberware(cboTest.Text);
                    break;
                case "critters.xml":
                case "metatypes.xml":
                    TestMetatype(cboTest.Text);
                    break;
                case "gear.xml":
                    TestGear();
                    break;
                case "qualities.xml":
                    TestQuality();
                    break;
                case "vehicles.xml":
                    TestVehicles();
                    break;
                case "weapons.xml":
                    TestWeapons();
                    break;
            }

            if (_objOutputBuilder.Length == 0)
                _objOutputBuilder.Append("Validation finished with no errors.");
            txtOutput.Text = _objOutputBuilder.ToString();
            cmdTest.Enabled = true;
        }

        private void TestVehicles()
        {
            _objCharacter.ResetCharacter();
            XmlDocument objXmlDocument = XmlManager.Load("vehicles.xml");
            pgbProgress.Minimum = 0;
            pgbProgress.Value = 0;
            XmlNodeList xmlVehicleList = objXmlDocument.SelectNodes("/chummer/vehicles/vehicle");
            XmlNodeList xmlVehicleModsList = objXmlDocument.SelectNodes("/chummer/mods/mod");
            pgbProgress.Maximum = xmlVehicleList?.Count ?? 0;
            pgbProgress.Maximum += xmlVehicleModsList?.Count ?? 0;

            // Vehicles.
            if (xmlVehicleList?.Count > 0)
            {
                foreach (XmlNode objXmlGear in xmlVehicleList)
                {
                    pgbProgress.PerformStep();
                    string strName = objXmlGear["name"]?.InnerText ?? objXmlGear["id"]?.InnerText;
                    if (string.IsNullOrEmpty(strName))
                        continue;
                    Application.DoEvents();
                    try
                    {
                        Vehicle objTemp = new Vehicle(_objCharacter);
                        objTemp.Create(objXmlGear);

                        Type objType = objTemp.GetType();

                        foreach (PropertyInfo objProperty in objType.GetProperties())
                        {
                            try
                            {
                                objProperty.GetValue(objTemp, null);
                            }
                            catch (Exception e)
                            {
                                if (_blnAddExceptionInfoToErrors)
                                    _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name + ". Exception: " + e);
                                else
                                    _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (_blnAddExceptionInfoToErrors)
                            _objOutputBuilder.AppendLine(strName + " general failure. Exception: " + e);
                        else
                            _objOutputBuilder.AppendLine(strName + " general failure");
                    }
                }
            }

            // Vehicle Mods.
            if (xmlVehicleModsList?.Count > 0)
            {
                foreach (XmlNode objXmlGear in xmlVehicleModsList)
                {
                    pgbProgress.PerformStep();
                    string strName = objXmlGear["name"]?.InnerText ?? objXmlGear["id"]?.InnerText;
                    if (string.IsNullOrEmpty(strName))
                        continue;
                    Application.DoEvents();
                    try
                    {
                        VehicleMod objTemp = new VehicleMod(_objCharacter);
                        objTemp.Create(objXmlGear, 1, null);

                        Type objType = objTemp.GetType();

                        foreach (PropertyInfo objProperty in objType.GetProperties())
                        {
                            try
                            {
                                objProperty.GetValue(objTemp, null);
                            }
                            catch (Exception e)
                            {
                                if (_blnAddExceptionInfoToErrors)
                                    _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name + ". Exception: " + e);
                                else
                                    _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name);
                            }
                        }

                        try
                        {
                            string _ = objTemp.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.DefaultLanguage);
                        }
                        catch (Exception e)
                        {
                            if (_blnAddExceptionInfoToErrors)
                                _objOutputBuilder.AppendLine(strName + " failed TotalAvail. Exception: " + e);
                            else
                                _objOutputBuilder.AppendLine(strName + " failed TotalAvail");
                        }
                    }
                    catch (Exception e)
                    {
                        if (_blnAddExceptionInfoToErrors)
                            _objOutputBuilder.AppendLine(strName + " general failure. Exception: " + e);
                        else
                            _objOutputBuilder.AppendLine(strName + " general failure");
                    }
                }
            }
        }

        private void TestWeapons()
        {
            _objCharacter.ResetCharacter();
            XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");
            pgbProgress.Minimum = 0;
            pgbProgress.Value = 0;
            XmlNodeList xmlWeaponList = objXmlDocument.SelectNodes("/chummer/weapons/weapon");
            XmlNodeList xmlAccessoryList = objXmlDocument.SelectNodes("/chummer/accessories/accessory");
            pgbProgress.Maximum = xmlWeaponList?.Count ?? 0;
            pgbProgress.Maximum += xmlAccessoryList?.Count ?? 0;

            // Weapons.
            if (xmlWeaponList?.Count > 0)
            {
                foreach (XmlNode objXmlGear in xmlWeaponList)
                {
                    pgbProgress.PerformStep();
                    string strName = objXmlGear["name"]?.InnerText ?? objXmlGear["id"]?.InnerText;
                    if (string.IsNullOrEmpty(strName))
                        continue;
                    Application.DoEvents();
                    try
                    {
                        Weapon objTemp = new Weapon(_objCharacter);
                        objTemp.Create(objXmlGear, null, true, false, true);

                        Type objType = objTemp.GetType();

                        foreach (PropertyInfo objProperty in objType.GetProperties())
                        {
                            try
                            {
                                objProperty.GetValue(objTemp, null);
                            }
                            catch (Exception e)
                            {
                                if (_blnAddExceptionInfoToErrors)
                                    _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name + ". Exception: " + e);
                                else
                                    _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (_blnAddExceptionInfoToErrors)
                            _objOutputBuilder.AppendLine(strName + " general failure. Exception: " + e);
                        else
                            _objOutputBuilder.AppendLine(strName + " general failure");
                    }
                }
            }

            // Weapon Accessories.
            if (xmlAccessoryList?.Count > 0)
            {
                foreach (XmlNode objXmlGear in xmlAccessoryList)
                {
                    pgbProgress.PerformStep();
                    string strName = objXmlGear["name"]?.InnerText ?? objXmlGear["id"]?.InnerText;
                    if (string.IsNullOrEmpty(strName))
                        continue;
                    Application.DoEvents();
                    try
                    {
                        WeaponAccessory objTemp = new WeaponAccessory(_objCharacter);
                        objTemp.Create(objXmlGear, new Tuple<string, string>(string.Empty, string.Empty), 0, true, true, false);

                        Type objType = objTemp.GetType();

                        foreach (PropertyInfo objProperty in objType.GetProperties())
                        {
                            try
                            {
                                objProperty.GetValue(objTemp, null);
                            }
                            catch (Exception e)
                            {
                                if (_blnAddExceptionInfoToErrors)
                                    _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name + ". Exception: " + e);
                                else
                                    _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (_blnAddExceptionInfoToErrors)
                            _objOutputBuilder.AppendLine(strName + " general failure. Exception: " + e);
                        else
                            _objOutputBuilder.AppendLine(strName + " general failure");
                    }
                }
            }
        }

        private void TestArmor()
        {
            _objCharacter.ResetCharacter();
            XmlDocument objXmlDocument = XmlManager.Load("armor.xml");
            pgbProgress.Minimum = 0;
            pgbProgress.Value = 0;
            XmlNodeList xmlArmorList = objXmlDocument.SelectNodes("/chummer/armors/armor");
            XmlNodeList xmlArmorModsList = objXmlDocument.SelectNodes("/chummer/mods/mod");
            pgbProgress.Maximum = xmlArmorList?.Count ?? 0;
            pgbProgress.Maximum += xmlArmorModsList?.Count ?? 0;

            // Armor.
            if (xmlArmorList?.Count > 0)
            {
                foreach (XmlNode objXmlGear in xmlArmorList)
                {
                    pgbProgress.PerformStep();
                    string strName = objXmlGear["name"]?.InnerText ?? objXmlGear["id"]?.InnerText;
                    if (string.IsNullOrEmpty(strName))
                        continue;
                    Application.DoEvents();
                    try
                    {
                        Armor objTemp = new Armor(_objCharacter);
                        List<Weapon> lstWeapons = new List<Weapon>(1);
                        objTemp.Create(objXmlGear, 0, lstWeapons, true, true, true);

                        Type objType = objTemp.GetType();

                        foreach (PropertyInfo objProperty in objType.GetProperties())
                        {
                            try
                            {
                                objProperty.GetValue(objTemp, null);
                            }
                            catch (Exception e)
                            {
                                if (_blnAddExceptionInfoToErrors)
                                    _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name + ". Exception: " + e);
                                else
                                    _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (_blnAddExceptionInfoToErrors)
                            _objOutputBuilder.AppendLine(strName + " general failure. Exception: " + e);
                        else
                            _objOutputBuilder.AppendLine(strName + " general failure");
                    }
                }
            }

            // Armor Mods.
            if (xmlArmorModsList?.Count > 0)
            {
                foreach (XmlNode objXmlGear in xmlArmorModsList)
                {
                    pgbProgress.PerformStep();
                    string strName = objXmlGear["name"]?.InnerText ?? objXmlGear["id"]?.InnerText;
                    if (string.IsNullOrEmpty(strName))
                        continue;
                    Application.DoEvents();
                    try
                    {
                        ArmorMod objTemp = new ArmorMod(_objCharacter);
                        List<Weapon> lstWeapons = new List<Weapon>(1);
                        objTemp.Create(objXmlGear, 1, lstWeapons, true, true);

                        Type objType = objTemp.GetType();

                        foreach (PropertyInfo objProperty in objType.GetProperties())
                        {
                            try
                            {
                                objProperty.GetValue(objTemp, null);
                            }
                            catch (Exception e)
                            {
                                if (_blnAddExceptionInfoToErrors)
                                    _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name + ". Exception: " + e);
                                else
                                    _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (_blnAddExceptionInfoToErrors)
                            _objOutputBuilder.AppendLine(strName + " general failure. Exception: " + e);
                        else
                            _objOutputBuilder.AppendLine(strName + " general failure");
                    }
                }
            }
        }

        private void TestGear()
        {
            _objCharacter.ResetCharacter();
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            pgbProgress.Minimum = 0;
            pgbProgress.Value = 0;
            using (XmlNodeList xmlGearList = objXmlDocument.SelectNodes("/chummer/gears/gear"))
            {
                if (xmlGearList?.Count > 0)
                {
                    pgbProgress.Maximum = xmlGearList.Count;

                    // Gear.
                    foreach (XmlNode objXmlGear in xmlGearList)
                    {
                        pgbProgress.PerformStep();
                        string strName = objXmlGear["name"]?.InnerText ?? objXmlGear["id"]?.InnerText;
                        if (string.IsNullOrEmpty(strName))
                            continue;
                        Application.DoEvents();
                        try
                        {
                            Gear objTemp = new Gear(_objCharacter);
                            List<Weapon> lstWeapons = new List<Weapon>(1);
                            objTemp.Create(objXmlGear, 1, lstWeapons, string.Empty, false);

                            Type objType = objTemp.GetType();

                            foreach (PropertyInfo objProperty in objType.GetProperties())
                            {
                                try
                                {
                                    objProperty.GetValue(objTemp, null);
                                }
                                catch (Exception e)
                                {
                                    if (_blnAddExceptionInfoToErrors)
                                        _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name + ". Exception: " + e);
                                    else
                                        _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (_blnAddExceptionInfoToErrors)
                                _objOutputBuilder.AppendLine(strName + " general failure. Exception: " + e);
                            else
                                _objOutputBuilder.AppendLine(strName + " general failure");
                        }
                    }
                }
            }
        }

        private void TestCyberware(string strFile)
        {
            string strPrefix = "cyberware";
            Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Cyberware;
            if (strFile == "bioware.xml")
            {
                strPrefix = "bioware";
                objSource = Improvement.ImprovementSource.Bioware;
            }

            _objCharacter.ResetCharacter();
            XmlDocument objXmlDocument = XmlManager.Load(strFile);
            pgbProgress.Minimum = 0;
            pgbProgress.Value = 0;

            Grade objTestGrade = _objCharacter.GetGradeList(objSource, true).FirstOrDefault(x => x.Name == "Standard");

            using (XmlNodeList xmlGearList = objXmlDocument.SelectNodes("/chummer/" + strPrefix + "s/" + strPrefix))
            {
                if (xmlGearList?.Count > 0)
                {
                    pgbProgress.Maximum = xmlGearList.Count;
                    // Gear.
                    foreach (XmlNode objXmlGear in xmlGearList)
                    {
                        pgbProgress.PerformStep();
                        string strName = objXmlGear["name"]?.InnerText ?? objXmlGear["id"]?.InnerText;
                        if (string.IsNullOrEmpty(strName))
                            continue;
                        Application.DoEvents();
                        try
                        {
                            Cyberware objTemp = new Cyberware(_objCharacter);
                            List<Weapon> lstWeapons = new List<Weapon>(1);
                            List<Vehicle> objVehicles = new List<Vehicle>(1);
                            objTemp.Create(objXmlGear, objTestGrade, objSource, 1, lstWeapons, objVehicles, false);

                            Type objType = objTemp.GetType();

                            foreach (PropertyInfo objProperty in objType.GetProperties())
                            {
                                try
                                {
                                    objProperty.GetValue(objTemp, null);
                                }
                                catch (Exception e)
                                {
                                    if (_blnAddExceptionInfoToErrors)
                                        _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name + ". Exception: " + e);
                                    else
                                        _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (_blnAddExceptionInfoToErrors)
                                _objOutputBuilder.AppendLine(strName + " general failure. Exception: " + e);
                            else
                                _objOutputBuilder.AppendLine(strName + " general failure");
                        }
                    }
                }
            }
        }

        private void TestQuality()
        {
            _objCharacter.ResetCharacter();
            XmlDocument objXmlDocument = XmlManager.Load("qualities.xml");
            pgbProgress.Minimum = 0;
            pgbProgress.Value = 0;

            using (XmlNodeList xmlQualitiesList = objXmlDocument.SelectNodes("/chummer/qualities/quality"))
            {
                if (xmlQualitiesList?.Count > 0)
                {
                    pgbProgress.Maximum = xmlQualitiesList.Count;

                    // Qualities.
                    foreach (XmlNode objXmlGear in xmlQualitiesList)
                    {
                        pgbProgress.PerformStep();
                        string strName = objXmlGear["name"]?.InnerText ?? objXmlGear["id"]?.InnerText;
                        if (string.IsNullOrEmpty(strName))
                            continue;
                        Application.DoEvents();
                        try
                        {
                            List<Weapon> lstWeapons = new List<Weapon>(1);
                            Quality objTemp = new Quality(_objCharacter);
                            objTemp.Create(objXmlGear, QualitySource.Selected, lstWeapons);

                            Type objType = objTemp.GetType();

                            foreach (PropertyInfo objProperty in objType.GetProperties())
                            {
                                try
                                {
                                    objProperty.GetValue(objTemp, null);
                                }
                                catch (Exception e)
                                {
                                    if (_blnAddExceptionInfoToErrors)
                                        _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name + ". Exception: " + e);
                                    else
                                        _objOutputBuilder.AppendLine(strName + " failed " + objProperty.Name);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (_blnAddExceptionInfoToErrors)
                                _objOutputBuilder.AppendLine(strName + " general failure. Exception: " + e);
                            else
                                _objOutputBuilder.AppendLine(strName + " general failure");
                        }
                    }
                }
            }
        }

        void TestMetatype(string strFile)
        {
            XmlDocument objXmlDocument = XmlManager.Load(strFile);

            pgbProgress.Minimum = 0;
            pgbProgress.Value = 0;
            using (XmlNodeList xmlMetatypeList = objXmlDocument.SelectNodes("/chummer/metatypes/metatype"))
            {
                if (xmlMetatypeList?.Count > 0)
                {
                    pgbProgress.Maximum = xmlMetatypeList.Count;

                    foreach (XmlNode objXmlMetatype in xmlMetatypeList)
                    {
                        pgbProgress.PerformStep();
                        string strName = objXmlMetatype["name"]?.InnerText ?? objXmlMetatype["id"]?.InnerText;
                        if (string.IsNullOrEmpty(strName))
                            continue;
                        Application.DoEvents();

                        objXmlDocument = XmlManager.Load(strFile);
                        _objCharacter.ResetCharacter();
                        try
                        {
                            int intForce = 0;
                            if (objXmlMetatype["forcecreature"] != null)
                                intForce = 1;

                            // Set Metatype information.
                            if (strFile != "critters.xml" || strName == "Ally Spirit")
                            {
                                _objCharacter.BOD.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["bodmin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["bodmax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["bodaug"].InnerText, intForce));
                                _objCharacter.AGI.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["agimin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["agimax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["agiaug"].InnerText, intForce));
                                _objCharacter.REA.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["reamin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["reamax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["reaaug"].InnerText, intForce));
                                _objCharacter.STR.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["strmin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["strmax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["straug"].InnerText, intForce));
                                _objCharacter.CHA.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["chamin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["chamax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["chaaug"].InnerText, intForce));
                                _objCharacter.INT.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["intmin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["intmax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["intaug"].InnerText, intForce));
                                _objCharacter.LOG.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["logmin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["logmax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["logaug"].InnerText, intForce));
                                _objCharacter.WIL.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["wilmin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["wilmax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["wilaug"].InnerText, intForce));
                                _objCharacter.MAG.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magaug"].InnerText, intForce));
                                _objCharacter.MAGAdept.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magaug"].InnerText, intForce));
                                _objCharacter.RES.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["resmin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["resmax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["resaug"].InnerText, intForce));
                                _objCharacter.EDG.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["edgmin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["edgmax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["edgaug"].InnerText, intForce));
                                _objCharacter.ESS.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["essmin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["essmax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["essaug"].InnerText, intForce));
                                _objCharacter.DEP.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["depmin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["depmax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["depaug"].InnerText, intForce));
                            }
                            else
                            {
                                int intMinModifier = -3;
                                _objCharacter.BOD.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["bodmin"].InnerText, intForce, intMinModifier),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["bodmin"].InnerText, intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["bodmin"].InnerText, intForce, 3));
                                _objCharacter.AGI.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["agimin"].InnerText, intForce, intMinModifier),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["agimin"].InnerText, intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["agimin"].InnerText, intForce, 3));
                                _objCharacter.REA.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["reamin"].InnerText, intForce, intMinModifier),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["reamin"].InnerText, intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["reamin"].InnerText, intForce, 3));
                                _objCharacter.STR.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["strmin"].InnerText, intForce, intMinModifier),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["strmin"].InnerText, intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["strmin"].InnerText, intForce, 3));
                                _objCharacter.CHA.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["chamin"].InnerText, intForce, intMinModifier),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["chamin"].InnerText, intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["chamin"].InnerText, intForce, 3));
                                _objCharacter.INT.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["intmin"].InnerText, intForce, intMinModifier),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["intmin"].InnerText, intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["intmin"].InnerText, intForce, 3));
                                _objCharacter.LOG.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["logmin"].InnerText, intForce, intMinModifier),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["logmin"].InnerText, intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["logmin"].InnerText, intForce, 3));
                                _objCharacter.WIL.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["wilmin"].InnerText, intForce, intMinModifier),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["wilmin"].InnerText, intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["wilmin"].InnerText, intForce, 3));
                                _objCharacter.MAG.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerText, intForce, intMinModifier),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerText, intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerText, intForce, 3));
                                _objCharacter.MAGAdept.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerText, intForce, intMinModifier),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerText, intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerText, intForce, 3));
                                _objCharacter.RES.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["resmin"].InnerText, intForce, intMinModifier),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["resmin"].InnerText, intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["resmin"].InnerText, intForce, 3));
                                _objCharacter.EDG.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["edgmin"].InnerText, intForce, intMinModifier),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["edgmin"].InnerText, intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["edgmin"].InnerText, intForce, 3));
                                _objCharacter.ESS.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["essmin"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["essmax"].InnerText, intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["essaug"].InnerText, intForce));
                                _objCharacter.DEP.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["depmin"].InnerText, intForce, intMinModifier),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["depmin"].InnerText, intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["depmin"].InnerText, intForce, 3));
                            }

                            /* If we're working with a Critter, set the Attributes to their default values.
                            if (strFile == "critters.xml")
                            {
                                _objCharacter.BOD.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["bodmin"].InnerText, intForce, 0));
                                _objCharacter.AGI.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["agimin"].InnerText, intForce, 0));
                                _objCharacter.REA.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["reamin"].InnerText, intForce, 0));
                                _objCharacter.STR.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["strmin"].InnerText, intForce, 0));
                                _objCharacter.CHA.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["chamin"].InnerText, intForce, 0));
                                _objCharacter.INT.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["intmin"].InnerText, intForce, 0));
                                _objCharacter.LOG.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["logmin"].InnerText, intForce, 0));
                                _objCharacter.WIL.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["wilmin"].InnerText, intForce, 0));
                                _objCharacter.MAG.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["magmin"].InnerText, intForce, 0));
                                _objCharacter.RES.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["resmin"].InnerText, intForce, 0));
                                _objCharacter.EDG.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["edgmin"].InnerText, intForce, 0));
                                _objCharacter.ESS.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["essmax"].InnerText, intForce, 0));
                                _objCharacter.DEP.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["depmin"].InnerText, intForce, 0));
                            }

                            // Sprites can never have Physical Attributes or WIL.
                            if (objXmlMetatype["name"].InnerText.EndsWith("Sprite"))
                            {
                                _objCharacter.BOD.AssignLimits("0", "0", "0");
                                _objCharacter.AGI.AssignLimits("0", "0", "0");
                                _objCharacter.REA.AssignLimits("0", "0", "0");
                                _objCharacter.STR.AssignLimits("0", "0", "0");
                            }

                            _objCharacter.Metatype = strName;
                            _objCharacter.MetatypeCategory = objXmlMetatype["category"].InnerText;
                            _objCharacter.Metavariant = string.Empty;
                            _objCharacter.MetatypeBP = 400;

                            if (objXmlMetatype["movement"] != null)
                                _objCharacter.Movement = objXmlMetatype["movement"].InnerText;
                            // Load the Qualities file.
                            XmlDocument objXmlQualityDocument = XmlManager.Load("qualities.xml");

                            // Determine if the Metatype has any bonuses.
                            if (objXmlMetatype.InnerXml.Contains("bonus"))
                                objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Metatype, strName, objXmlMetatype.SelectSingleNode("bonus"), false, 1, strName);

                            // Create the Qualities that come with the Metatype.
                            foreach (XmlNode objXmlQualityItem in objXmlMetatype.SelectNodes("qualities/positive/quality"))
                            {
                                XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = " + objXmlQualityItem.InnerText.CleanXPath() + "]");
                                List<Weapon> lstWeapons = new List<Weapon>(1);
                                Quality objQuality = new Quality(_objCharacter);
                                string strForceValue = string.Empty;
                                if (objXmlQualityItem.Attributes["select"] != null)
                                    strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                                QualitySource objSource = new QualitySource();
                                objSource = QualitySource.Metatype;
                                if (objXmlQualityItem.Attributes["removable"] != null)
                                    objSource = QualitySource.MetatypeRemovable;
                                objQuality.Create(objXmlQuality, _objCharacter, objSource, lstWeapons, strForceValue);
                                _objCharacter.Qualities.Add(objQuality);
                            }
                            foreach (XmlNode objXmlQualityItem in objXmlMetatype.SelectNodes("qualities/negative/quality"))
                            {
                                XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = " + objXmlQualityItem.InnerText.CleanXPath() + "]");
                                List<Weapon> lstWeapons = new List<Weapon>(1);
                                Quality objQuality = new Quality(_objCharacter);
                                string strForceValue = string.Empty;
                                if (objXmlQualityItem.Attributes["select"] != null)
                                    strForceValue = objXmlQualityItem.Attributes["select"].InnerText;
                                QualitySource objSource = new QualitySource();
                                objSource = QualitySource.Metatype;
                                if (objXmlQualityItem.Attributes["removable"] != null)
                                    objSource = QualitySource.MetatypeRemovable;
                                objQuality.Create(objXmlQuality, _objCharacter, objSource, lstWeapons, strForceValue);
                                _objCharacter.Qualities.Add(objQuality);
                            }

                            /* Run through the character's Attributes one more time and make sure their value matches their minimum value.
                            if (strFile == "metatypes.xml")
                            {
                                _objCharacter.BOD.Value = _objCharacter.BOD.TotalMinimum;
                                _objCharacter.AGI.Value = _objCharacter.AGI.TotalMinimum;
                                _objCharacter.REA.Value = _objCharacter.REA.TotalMinimum;
                                _objCharacter.STR.Value = _objCharacter.STR.TotalMinimum;
                                _objCharacter.CHA.Value = _objCharacter.CHA.TotalMinimum;
                                _objCharacter.INT.Value = _objCharacter.INT.TotalMinimum;
                                _objCharacter.LOG.Value = _objCharacter.LOG.TotalMinimum;
                                _objCharacter.WIL.Value = _objCharacter.WIL.TotalMinimum;
                            }
                            */

                            // Add any Critter Powers the Metatype/Critter should have.
                            XmlNode objXmlCritter = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = " + _objCharacter.Metatype.CleanXPath() + "]");

                            objXmlDocument = XmlManager.Load("critterpowers.xml");
                            foreach (XmlNode objXmlPower in objXmlCritter.SelectNodes("powers/power"))
                            {
                                XmlNode objXmlCritterPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = " + objXmlPower.InnerText.CleanXPath() + "]");
                                CritterPower objPower = new CritterPower(_objCharacter);
                                string strForcedValue = objXmlPower.Attributes?["select"]?.InnerText ?? string.Empty;
                                int intRating = 0;

                                if (objXmlPower.Attributes["rating"] != null)
                                    intRating = CommonFunctions.ExpressionToInt(objXmlPower.Attributes["rating"].InnerText, intForce, 0, 0);

                                objPower.Create(objXmlCritterPower, intRating, strForcedValue);
                                _objCharacter.CritterPowers.Add(objPower);
                            }

                            // Set the Skill Ratings for the Critter.
                            foreach (XmlNode objXmlSkill in objXmlCritter.SelectNodes("skills/skill"))
                            {
                                if (objXmlSkill.InnerText.Contains("Exotic"))
                                {
                                    //Skill objExotic = new Skill(_objCharacter);
                                    //objExotic.ExoticSkill = true;
                                    //objExotic.Attribute = "AGI";
                                    //if (objXmlSkill.Attributes["spec"] != null)
                                    //{
                                    //SkillSpecialization objSpec = new SkillSpecialization(objXmlSkill.Attributes["spec"].InnerText);
                                    //objExotic.Specializations.Add(objSpec);
                                    //}
                                    //if (Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(intForce), 0)) > 6)
                                    //    objExotic.RatingMaximum = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(intForce), 0));
                                    //objExotic.Rating = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(intForce), 0));
                                    //objExotic.Name = objXmlSkill.InnerText;
                                    //_objCharacter.Skills.Add(objExotic);
                                }
                                else
                                {
                                    foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                                    {
                                        if (objSkill.Name == objXmlSkill.InnerText)
                                        {
                                            if (objXmlSkill.Attributes["spec"] != null)
                                            {
                                                //SkillSpecialization objSpec = new SkillSpecialization(objXmlSkill.Attributes["spec"].InnerText);
                                                //objSkill.Specializations.Add(objSpec);
                                            }

                                            //if (Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(intForce), 0)) > 6)
                                            //    objSkill.RatingMaximum = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(intForce), 0));
                                            //objSkill.Rating = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(intForce), 0));
                                            break;
                                        }
                                    }
                                }
                            }

                            //TODO: Sorry, whenever we get critter book...
                            // Set the Skill Group Ratings for the Critter.
                            //foreach (XmlNode objXmlSkill in objXmlCritter.SelectNodes("skills/group"))
                            //{
                            //    foreach (SkillGroup objSkill in _objCharacter.SkillGroups)
                            //    {
                            //        if (objSkill.Name == objXmlSkill.InnerText)
                            //        {
                            //            objSkill.RatingMaximum = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(intForce), 0));
                            //            objSkill.Rating = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerText, Convert.ToInt32(intForce), 0));
                            //            break;
                            //        }
                            //    }
                            //}

                            // Set the Knowledge Skill Ratings for the Critter.
                            //foreach (XmlNode objXmlSkill in objXmlCritter.SelectNodes("skills/knowledge"))
                            //{
                            //    Skill objKnowledge = new Skill(_objCharacter);
                            //    objKnowledge.Name = objXmlSkill.InnerText;
                            //    objKnowledge.KnowledgeSkill = true;
                            //    if (objXmlSkill.Attributes["spec"] != null)
                            //                   {
                            //                       //SkillSpecialization objSpec = new SkillSpecialization(objXmlSkill.Attributes["spec"].InnerText);
                            //                       //objKnowledge.Specializations.Add(objSpec);
                            //                   }
                            //    objKnowledge.SkillCategory = objXmlSkill.Attributes["category"].InnerText;
                            //    //if (Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerText) > 6)
                            //    //    objKnowledge.RatingMaximum = Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerText);
                            //    //objKnowledge.Rating = Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerText);
                            //    _objCharacter.Skills.Add(objKnowledge);
                            //}

                            // If this is a Critter with a Force (which dictates their Skill Rating/Maximum Skill Rating), set their Skill Rating Maximums.
                            if (intForce > 0)
                            {
                                int intMaxRating = intForce;
                                // Determine the highest Skill Rating the Critter has.
                                foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                                {
                                    if (objSkill.RatingMaximum > intMaxRating)
                                        intMaxRating = objSkill.RatingMaximum;
                                }

                                // Now that we know the upper limit, set all of the Skill Rating Maximums to match.
                                //foreach (Skill objSkill in _objCharacter.Skills)
                                //    objSkill.RatingMaximum = intMaxRating;
                                //foreach (SkillGroup objGroup in _objCharacter.SkillGroups)
                                //    objGroup.RatingMaximum = intMaxRating;

                                // Set the MaxSkillRating for the character so it can be used later when they add new Knowledge Skills or Exotic Skills.

                            }

                            // Add any Complex Forms the Critter comes with (typically Sprites)
                            XmlDocument objXmlProgramDocument = XmlManager.Load("complexforms.xml");
                            foreach (XmlNode objXmlComplexForm in objXmlCritter.SelectNodes("complexforms/complexform"))
                            {
                                string strForceValue = objXmlComplexForm.Attributes?["select"]?.InnerText ?? string.Empty;
                                XmlNode objXmlComplexFormData = objXmlProgramDocument.SelectSingleNode("/chummer/complexforms/complexform[name = " + objXmlComplexForm.InnerText.CleanXPath() + "]");
                                ComplexForm objComplexForm = new ComplexForm(_objCharacter);
                                objComplexForm.Create(objXmlComplexFormData, strForceValue);
                                _objCharacter.ComplexForms.Add(objComplexForm);
                            }

                            // Add any Gear the Critter comes with (typically Programs for A.I.s)
                            XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");
                            foreach (XmlNode objXmlGear in objXmlCritter.SelectNodes("gears/gear"))
                            {
                                int intRating = 0;
                                if (objXmlGear.Attributes["rating"] != null)
                                    intRating = CommonFunctions.ExpressionToInt(objXmlGear.Attributes["rating"].InnerText, intForce, 0, 0);
                                string strForceValue = objXmlGear.Attributes?["select"]?.InnerText ?? string.Empty;
                                XmlNode objXmlGearItem = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = " + objXmlGear.InnerText.CleanXPath() + "]");
                                Gear objGear = new Gear(_objCharacter);
                                List<Weapon> lstWeapons = new List<Weapon>(1);
                                objGear.Create(objXmlGearItem, intRating, lstWeapons, strForceValue);
                                objGear.Cost = "0";
                                _objCharacter.Gear.Add(objGear);
                            }
                        }
                        catch (Exception e)
                        {
                            if (_blnAddExceptionInfoToErrors)
                                _objOutputBuilder.AppendLine(strName + " general failure. Exception: " + e);
                            else
                                _objOutputBuilder.AppendLine(strName + " general failure");
                        }
                    }
                }
            }
        }
    }
}
