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
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;

namespace Chummer
{
    public partial class TestDataEntries : Form
    {
        public TestDataEntries()
        {
            InitializeComponent();
            _sbdOutputBuilder = Utils.StringBuilderPool.Get();
        }

        private bool _blnAddExceptionInfoToErrors;
        private readonly StringBuilder _sbdOutputBuilder;
        private readonly Character _objCharacter = new Character();

        private void cmdTest_Click(object sender, EventArgs e)
        {
            cmdTest.Enabled = false;
            _blnAddExceptionInfoToErrors = chkAddExceptionInfoToErrors.Checked;
            txtOutput.Text = _blnAddExceptionInfoToErrors
                ? "Testing " + cboTest.Text + " with exception info printed." + Environment.NewLine + Environment.NewLine + "Please wait..."
                : "Testing " + cboTest.Text + "." + Environment.NewLine + Environment.NewLine + "Please wait...";
            _sbdOutputBuilder.Clear();
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

            if (_sbdOutputBuilder.Length == 0)
                _sbdOutputBuilder.Append("Validation finished with no errors.");
            txtOutput.Text = _sbdOutputBuilder.ToString();
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
                    string strName = objXmlGear["name"]?.InnerTextViaPool() ?? objXmlGear["id"]?.InnerTextViaPool();
                    if (string.IsNullOrEmpty(strName))
                        continue;
                    Utils.DoEventsSafe();
                    try
                    {
                        using (Vehicle objTemp = new Vehicle(_objCharacter))
                        {
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
                                        _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name, ". Exception: ", e.ToString());
                                    else
                                        _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (_blnAddExceptionInfoToErrors)
                            _sbdOutputBuilder.AppendLine(strName, " general failure. Exception: ", e.ToString());
                        else
                            _sbdOutputBuilder.AppendLine(strName, " general failure");
                    }
                }
            }

            // Vehicle Mods.
            if (xmlVehicleModsList?.Count > 0)
            {
                foreach (XmlNode objXmlGear in xmlVehicleModsList)
                {
                    pgbProgress.PerformStep();
                    string strName = objXmlGear["name"]?.InnerTextViaPool() ?? objXmlGear["id"]?.InnerTextViaPool();
                    if (string.IsNullOrEmpty(strName))
                        continue;
                    Utils.DoEventsSafe();
                    try
                    {
                        using (VehicleMod objTemp = new VehicleMod(_objCharacter))
                        {
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
                                        _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name, ". Exception: ", e.ToString());
                                    else
                                        _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name);
                                }
                            }

                            try
                            {
                                string _ = objTemp.TotalAvail(GlobalSettings.CultureInfo, GlobalSettings.DefaultLanguage);
                            }
                            catch (Exception e)
                            {
                                if (_blnAddExceptionInfoToErrors)
                                    _sbdOutputBuilder.AppendLine(strName, " failed TotalAvail. Exception: ", e.ToString());
                                else
                                    _sbdOutputBuilder.AppendLine(strName, " failed TotalAvail");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (_blnAddExceptionInfoToErrors)
                            _sbdOutputBuilder.AppendLine(strName, " general failure. Exception: ", e.ToString());
                        else
                            _sbdOutputBuilder.AppendLine(strName, " general failure");
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
                    string strName = objXmlGear["name"]?.InnerTextViaPool() ?? objXmlGear["id"]?.InnerTextViaPool();
                    if (string.IsNullOrEmpty(strName))
                        continue;
                    Utils.DoEventsSafe();
                    try
                    {
                        using (Weapon objTemp = new Weapon(_objCharacter))
                        {
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
                                        _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name, ". Exception: ", e.ToString());
                                    else
                                        _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (_blnAddExceptionInfoToErrors)
                            _sbdOutputBuilder.AppendLine(strName, " general failure. Exception: ", e.ToString());
                        else
                            _sbdOutputBuilder.AppendLine(strName, " general failure");
                    }
                }
            }

            // Weapon Accessories.
            if (xmlAccessoryList?.Count > 0)
            {
                foreach (XmlNode objXmlGear in xmlAccessoryList)
                {
                    pgbProgress.PerformStep();
                    string strName = objXmlGear["name"]?.InnerTextViaPool() ?? objXmlGear["id"]?.InnerTextViaPool();
                    if (string.IsNullOrEmpty(strName))
                        continue;
                    Utils.DoEventsSafe();
                    try
                    {
                        using (WeaponAccessory objTemp = new WeaponAccessory(_objCharacter))
                        {
                            objTemp.Create(objXmlGear, new ValueTuple<string, string>(string.Empty, string.Empty), 0, true, true, false);

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
                                        _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name, ". Exception: ", e.ToString());
                                    else
                                        _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (_blnAddExceptionInfoToErrors)
                            _sbdOutputBuilder.AppendLine(strName, " general failure. Exception: ", e.ToString());
                        else
                            _sbdOutputBuilder.AppendLine(strName, " general failure");
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
                    string strName = objXmlGear["name"]?.InnerTextViaPool() ?? objXmlGear["id"]?.InnerTextViaPool();
                    if (string.IsNullOrEmpty(strName))
                        continue;
                    Utils.DoEventsSafe();
                    try
                    {
                        List<Weapon> lstWeapons = new List<Weapon>(1);
                        try
                        {
                            using (Armor objTemp = new Armor(_objCharacter))
                            {
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
                                            _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name, ". Exception: ", e.ToString());
                                        else
                                            _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            foreach (Weapon objWeapon in lstWeapons)
                                objWeapon.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        if (_blnAddExceptionInfoToErrors)
                            _sbdOutputBuilder.AppendLine(strName, " general failure. Exception: ", e.ToString());
                        else
                            _sbdOutputBuilder.AppendLine(strName, " general failure");
                    }
                }
            }

            // Armor Mods.
            if (xmlArmorModsList?.Count > 0)
            {
                foreach (XmlNode objXmlGear in xmlArmorModsList)
                {
                    pgbProgress.PerformStep();
                    string strName = objXmlGear["name"]?.InnerTextViaPool() ?? objXmlGear["id"]?.InnerTextViaPool();
                    if (string.IsNullOrEmpty(strName))
                        continue;
                    Utils.DoEventsSafe();
                    try
                    {
                        List<Weapon> lstWeapons = new List<Weapon>(1);
                        try
                        {
                            using (ArmorMod objTemp = new ArmorMod(_objCharacter, null))
                            {
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
                                            _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name, ". Exception: ", e.ToString());
                                        else
                                            _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            foreach (Weapon objWeapon in lstWeapons)
                                objWeapon.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        if (_blnAddExceptionInfoToErrors)
                            _sbdOutputBuilder.AppendLine(strName, " general failure. Exception: ", e.ToString());
                        else
                            _sbdOutputBuilder.AppendLine(strName, " general failure");
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
                        string strName = objXmlGear["name"]?.InnerTextViaPool() ?? objXmlGear["id"]?.InnerTextViaPool();
                        if (string.IsNullOrEmpty(strName))
                            continue;
                        Utils.DoEventsSafe();
                        try
                        {
                            List<Weapon> lstWeapons = new List<Weapon>(1);
                            try
                            {
                                using (Gear objTemp = new Gear(_objCharacter))
                                {
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
                                                _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name, ". Exception: ", e.ToString());
                                            else
                                                _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name);
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                foreach (Weapon objWeapon in lstWeapons)
                                    objWeapon.Dispose();
                            }
                        }
                        catch (Exception e)
                        {
                            if (_blnAddExceptionInfoToErrors)
                                _sbdOutputBuilder.AppendLine(strName, " general failure. Exception: ", e.ToString());
                            else
                                _sbdOutputBuilder.AppendLine(strName, " general failure");
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

            Grade objTestGrade = _objCharacter.GetGradeByName(objSource, "Standard", true);

            using (XmlNodeList xmlGearList = objXmlDocument.SelectNodes("/chummer/" + strPrefix + "s/" + strPrefix))
            {
                if (xmlGearList?.Count > 0)
                {
                    pgbProgress.Maximum = xmlGearList.Count;
                    // Gear.
                    foreach (XmlNode objXmlGear in xmlGearList)
                    {
                        pgbProgress.PerformStep();
                        string strName = objXmlGear["name"]?.InnerTextViaPool() ?? objXmlGear["id"]?.InnerTextViaPool();
                        if (string.IsNullOrEmpty(strName))
                            continue;
                        Utils.DoEventsSafe();
                        try
                        {
                            List<Weapon> lstWeapons = new List<Weapon>(1);
                            List<Vehicle> lstVehicles = new List<Vehicle>(1);
                            try
                            {
                                using (Cyberware objTemp = new Cyberware(_objCharacter))
                                {
                                    objTemp.Create(objXmlGear, objTestGrade, objSource, 1, lstWeapons, lstVehicles, false);

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
                                                _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name, ". Exception: ", e.ToString());
                                            else
                                                _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name);
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                foreach (Weapon objWeapon in lstWeapons)
                                    objWeapon.Dispose();

                                foreach (Vehicle objVehicle in lstVehicles)
                                    objVehicle.Dispose();
                            }
                        }
                        catch (Exception e)
                        {
                            if (_blnAddExceptionInfoToErrors)
                                _sbdOutputBuilder.AppendLine(strName, " general failure. Exception: ", e.ToString());
                            else
                                _sbdOutputBuilder.AppendLine(strName, " general failure");
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
                        string strName = objXmlGear["name"]?.InnerTextViaPool() ?? objXmlGear["id"]?.InnerTextViaPool();
                        if (string.IsNullOrEmpty(strName))
                            continue;
                        Utils.DoEventsSafe();
                        try
                        {
                            List<Weapon> lstWeapons = new List<Weapon>(1);
                            try
                            {
                                using (Quality objTemp = new Quality(_objCharacter))
                                {
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
                                                _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name, ". Exception: ", e.ToString());
                                            else
                                                _sbdOutputBuilder.AppendLine(strName, " failed ", objProperty.Name);
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                foreach (Weapon objWeapon in lstWeapons)
                                    objWeapon.Dispose();
                            }
                        }
                        catch (Exception e)
                        {
                            if (_blnAddExceptionInfoToErrors)
                                _sbdOutputBuilder.AppendLine(strName, " general failure. Exception: ", e.ToString());
                            else
                                _sbdOutputBuilder.AppendLine(strName, " general failure");
                        }
                    }
                }
            }
        }

        private void TestMetatype(string strFile)
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
                        string strName = objXmlMetatype["name"]?.InnerTextViaPool() ?? objXmlMetatype["id"]?.InnerTextViaPool();
                        if (string.IsNullOrEmpty(strName))
                            continue;
                        Utils.DoEventsSafe();

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
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["bodmin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["bodmax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["bodaug"].InnerTextViaPool(), intForce));
                                _objCharacter.AGI.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["agimin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["agimax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["agiaug"].InnerTextViaPool(), intForce));
                                _objCharacter.REA.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["reamin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["reamax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["reaaug"].InnerTextViaPool(), intForce));
                                _objCharacter.STR.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["strmin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["strmax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["straug"].InnerTextViaPool(), intForce));
                                _objCharacter.CHA.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["chamin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["chamax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["chaaug"].InnerTextViaPool(), intForce));
                                _objCharacter.INT.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["intmin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["intmax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["intaug"].InnerTextViaPool(), intForce));
                                _objCharacter.LOG.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["logmin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["logmax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["logaug"].InnerTextViaPool(), intForce));
                                _objCharacter.WIL.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["wilmin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["wilmax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["wilaug"].InnerTextViaPool(), intForce));
                                _objCharacter.MAG.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magaug"].InnerTextViaPool(), intForce));
                                _objCharacter.MAGAdept.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magaug"].InnerTextViaPool(), intForce));
                                _objCharacter.RES.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["resmin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["resmax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["resaug"].InnerTextViaPool(), intForce));
                                _objCharacter.EDG.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["edgmin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["edgmax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["edgaug"].InnerTextViaPool(), intForce));
                                _objCharacter.ESS.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["essmin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["essmax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["essaug"].InnerTextViaPool(), intForce));
                                _objCharacter.DEP.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["depmin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["depmax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["depaug"].InnerTextViaPool(), intForce));
                            }
                            else
                            {
                                _objCharacter.BOD.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["bodmin"].InnerTextViaPool(), intForce, -3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["bodmin"].InnerTextViaPool(), intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["bodmin"].InnerTextViaPool(), intForce, 3));
                                _objCharacter.AGI.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["agimin"].InnerTextViaPool(), intForce, -3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["agimin"].InnerTextViaPool(), intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["agimin"].InnerTextViaPool(), intForce, 3));
                                _objCharacter.REA.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["reamin"].InnerTextViaPool(), intForce, -3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["reamin"].InnerTextViaPool(), intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["reamin"].InnerTextViaPool(), intForce, 3));
                                _objCharacter.STR.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["strmin"].InnerTextViaPool(), intForce, -3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["strmin"].InnerTextViaPool(), intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["strmin"].InnerTextViaPool(), intForce, 3));
                                _objCharacter.CHA.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["chamin"].InnerTextViaPool(), intForce, -3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["chamin"].InnerTextViaPool(), intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["chamin"].InnerTextViaPool(), intForce, 3));
                                _objCharacter.INT.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["intmin"].InnerTextViaPool(), intForce, -3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["intmin"].InnerTextViaPool(), intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["intmin"].InnerTextViaPool(), intForce, 3));
                                _objCharacter.LOG.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["logmin"].InnerTextViaPool(), intForce, -3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["logmin"].InnerTextViaPool(), intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["logmin"].InnerTextViaPool(), intForce, 3));
                                _objCharacter.WIL.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["wilmin"].InnerTextViaPool(), intForce, -3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["wilmin"].InnerTextViaPool(), intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["wilmin"].InnerTextViaPool(), intForce, 3));
                                _objCharacter.MAG.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerTextViaPool(), intForce, -3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerTextViaPool(), intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerTextViaPool(), intForce, 3));
                                _objCharacter.MAGAdept.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerTextViaPool(), intForce, -3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerTextViaPool(), intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["magmin"].InnerTextViaPool(), intForce, 3));
                                _objCharacter.RES.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["resmin"].InnerTextViaPool(), intForce, -3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["resmin"].InnerTextViaPool(), intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["resmin"].InnerTextViaPool(), intForce, 3));
                                _objCharacter.EDG.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["edgmin"].InnerTextViaPool(), intForce, -3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["edgmin"].InnerTextViaPool(), intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["edgmin"].InnerTextViaPool(), intForce, 3));
                                _objCharacter.ESS.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["essmin"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["essmax"].InnerTextViaPool(), intForce),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["essaug"].InnerTextViaPool(), intForce));
                                _objCharacter.DEP.AssignLimits(
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["depmin"].InnerTextViaPool(), intForce, -3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["depmin"].InnerTextViaPool(), intForce, 3),
                                    CommonFunctions.ExpressionToInt(objXmlMetatype["depmin"].InnerTextViaPool(), intForce, 3));
                            }

                            /* If we're working with a Critter, set the Attributes to their default values.
                            if (strFile == "critters.xml")
                            {
                                _objCharacter.BOD.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["bodmin"].InnerTextViaPool(), intForce, 0));
                                _objCharacter.AGI.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["agimin"].InnerTextViaPool(), intForce, 0));
                                _objCharacter.REA.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["reamin"].InnerTextViaPool(), intForce, 0));
                                _objCharacter.STR.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["strmin"].InnerTextViaPool(), intForce, 0));
                                _objCharacter.CHA.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["chamin"].InnerTextViaPool(), intForce, 0));
                                _objCharacter.INT.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["intmin"].InnerTextViaPool(), intForce, 0));
                                _objCharacter.LOG.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["logmin"].InnerTextViaPool(), intForce, 0));
                                _objCharacter.WIL.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["wilmin"].InnerTextViaPool(), intForce, 0));
                                _objCharacter.MAG.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["magmin"].InnerTextViaPool(), intForce, 0));
                                _objCharacter.RES.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["resmin"].InnerTextViaPool(), intForce, 0));
                                _objCharacter.EDG.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["edgmin"].InnerTextViaPool(), intForce, 0));
                                _objCharacter.ESS.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["essmax"].InnerTextViaPool(), intForce, 0));
                                _objCharacter.DEP.Value = Convert.ToInt32(ExpressionToString(objXmlMetatype["depmin"].InnerTextViaPool(), intForce, 0));
                            }

                            // Sprites can never have Physical Attributes or WIL.
                            if (objXmlMetatype["name"].InnerTextViaPool().EndsWith("Sprite"))
                            {
                                _objCharacter.BOD.AssignLimits("0", "0", "0");
                                _objCharacter.AGI.AssignLimits("0", "0", "0");
                                _objCharacter.REA.AssignLimits("0", "0", "0");
                                _objCharacter.STR.AssignLimits("0", "0", "0");
                            }

                            _objCharacter.Metatype = strName;
                            _objCharacter.MetatypeCategory = objXmlMetatype["category"].InnerTextViaPool();
                            _objCharacter.Metavariant = string.Empty;
                            _objCharacter.MetatypeBP = 400;

                            if (objXmlMetatype["movement"] != null)
                                _objCharacter.Movement = objXmlMetatype["movement"].InnerTextViaPool();
                            // Load the Qualities file.
                            XmlDocument objXmlQualityDocument = XmlManager.Load("qualities.xml");

                            // Determine if the Metatype has any bonuses.
                            if (objXmlMetatype.InnerXml.Contains("bonus"))
                                objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Metatype, strName, objXmlMetatype.SelectSingleNode("bonus"), false, 1, strName);

                            // Create the Qualities that come with the Metatype.
                            foreach (XmlNode objXmlQualityItem in objXmlMetatype.SelectNodes("qualities/positive/quality"))
                            {
                                XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = " + objXmlQualityItem.InnerTextViaPool().CleanXPath() + "]");
                                List<Weapon> lstWeapons = new List<Weapon>(1);
                                Quality objQuality = new Quality(_objCharacter);
                                string strForceValue = string.Empty;
                                if (objXmlQualityItem.Attributes["select"] != null)
                                    strForceValue = objXmlQualityItem.Attributes["select"].InnerTextViaPool();
                                QualitySource objSource = new QualitySource();
                                objSource = QualitySource.Metatype;
                                if (objXmlQualityItem.Attributes["removable"] != null)
                                    objSource = QualitySource.MetatypeRemovable;
                                objQuality.Create(objXmlQuality, _objCharacter, objSource, lstWeapons, strForceValue);
                                _objCharacter.Qualities.Add(objQuality);
                            }
                            foreach (XmlNode objXmlQualityItem in objXmlMetatype.SelectNodes("qualities/negative/quality"))
                            {
                                XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = " + objXmlQualityItem.InnerTextViaPool().CleanXPath() + "]");
                                List<Weapon> lstWeapons = new List<Weapon>(1);
                                Quality objQuality = new Quality(_objCharacter);
                                string strForceValue = string.Empty;
                                if (objXmlQualityItem.Attributes["select"] != null)
                                    strForceValue = objXmlQualityItem.Attributes["select"].InnerTextViaPool();
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
                            XmlNode objXmlCritter = objXmlDocument.TryGetNodeByNameOrId("/chummer/metatypes/metatype", _objCharacter.Metatype);

                            objXmlDocument = XmlManager.Load("critterpowers.xml");
                            foreach (XmlNode objXmlPower in objXmlCritter.SelectNodes("powers/power"))
                            {
                                XmlNode objXmlCritterPower = objXmlDocument.TryGetNodeByNameOrId("/chummer/powers/power", objXmlPower.InnerTextViaPool());
                                CritterPower objPower = new CritterPower(_objCharacter);
                                try
                                {
                                    string strForcedValue = objXmlPower.Attributes?["select"]?.InnerTextViaPool() ?? string.Empty;
                                    int intRating = 0;

                                    if (objXmlPower.Attributes["rating"] != null)
                                        intRating = CommonFunctions.ExpressionToInt(objXmlPower.Attributes["rating"].InnerTextViaPool(), intForce, 0, 0);

                                    objPower.Create(objXmlCritterPower, intRating, strForcedValue);
                                    _objCharacter.CritterPowers.Add(objPower);
                                }
                                catch
                                {
                                    objPower.Remove(false);
                                    throw;
                                }
                            }

                            // Set the Skill Ratings for the Critter.
                            foreach (XmlNode objXmlSkill in objXmlCritter.SelectNodes("skills/skill"))
                            {
                                string strInnerText = objXmlSkill.InnerTextViaPool();
                                if (ExoticSkill.IsExoticSkillName(_objCharacter, strInnerText))
                                {
                                    //Skill objExotic = new Skill(_objCharacter);
                                    //objExotic.ExoticSkill = true;
                                    //objExotic.Attribute = "AGI";
                                    //if (objXmlSkill.Attributes["spec"] != null)
                                    //{
                                    //SkillSpecialization objSpec = new SkillSpecialization(objXmlSkill.Attributes["spec"].InnerTextViaPool());
                                    //objExotic.Specializations.Add(objSpec);
                                    //}
                                    //if (Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerTextViaPool(), Convert.ToInt32(intForce), 0)) > 6)
                                    //    objExotic.RatingMaximum = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerTextViaPool(), Convert.ToInt32(intForce), 0));
                                    //objExotic.Rating = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerTextViaPool(), Convert.ToInt32(intForce), 0));
                                    //objExotic.Name = objXmlSkill.InnerTextViaPool();
                                    //_objCharacter.Skills.Add(objExotic);
                                }
                                else
                                {
                                    _objCharacter.SkillsSection.Skills.ForEachWithBreak(objSkill =>
                                    {
                                        if (objSkill.Name == strInnerText)
                                        {
                                            if (objXmlSkill.Attributes?["spec"] != null)
                                            {
                                                //SkillSpecialization objSpec = new SkillSpecialization(objXmlSkill.Attributes["spec"].InnerTextViaPool());
                                                //objSkill.Specializations.Add(objSpec);
                                            }

                                            //if (Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerTextViaPool(), Convert.ToInt32(intForce), 0)) > 6)
                                            //    objSkill.RatingMaximum = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerTextViaPool(), Convert.ToInt32(intForce), 0));
                                            //objSkill.Rating = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerTextViaPool(), Convert.ToInt32(intForce), 0));
                                            return false;
                                        }

                                        return true;
                                    });
                                }
                            }

                            //TODO: Sorry, whenever we get critter book...
                            // Set the Skill Group Ratings for the Critter.
                            //foreach (XmlNode objXmlSkill in objXmlCritter.SelectNodes("skills/group"))
                            //{
                            //    foreach (SkillGroup objSkill in _objCharacter.SkillGroups)
                            //    {
                            //        if (objSkill.Name == objXmlSkill.InnerTextViaPool())
                            //        {
                            //            objSkill.RatingMaximum = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerTextViaPool(), Convert.ToInt32(intForce), 0));
                            //            objSkill.Rating = Convert.ToInt32(ExpressionToString(objXmlSkill.Attributes["rating"].InnerTextViaPool(), Convert.ToInt32(intForce), 0));
                            //            break;
                            //        }
                            //    }
                            //}

                            // Set the Knowledge Skill Ratings for the Critter.
                            //foreach (XmlNode objXmlSkill in objXmlCritter.SelectNodes("skills/knowledge"))
                            //{
                            //    Skill objKnowledge = new Skill(_objCharacter);
                            //    objKnowledge.Name = objXmlSkill.InnerTextViaPool();
                            //    objKnowledge.KnowledgeSkill = true;
                            //    if (objXmlSkill.Attributes["spec"] != null)
                            //                   {
                            //                       //SkillSpecialization objSpec = new SkillSpecialization(objXmlSkill.Attributes["spec"].InnerTextViaPool());
                            //                       //objKnowledge.Specializations.Add(objSpec);
                            //                   }
                            //    objKnowledge.SkillCategory = objXmlSkill.Attributes["category"].InnerTextViaPool();
                            //    //if (Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerTextViaPool()) > 6)
                            //    //    objKnowledge.RatingMaximum = Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerTextViaPool());
                            //    //objKnowledge.Rating = Convert.ToInt32(objXmlSkill.Attributes["rating"].InnerTextViaPool());
                            //    _objCharacter.Skills.Add(objKnowledge);
                            //}

                            // If this is a Critter with a Force (which dictates their Skill Rating/Maximum Skill Rating), set their Skill Rating Maximums.
                            if (intForce > 0)
                            {
                                int intMaxRating = intForce;
                                // Determine the highest Skill Rating the Critter has.
                                _objCharacter.SkillsSection.Skills.ForEach(objSkill =>
                                {
                                    int intLoopRatingMaximum = objSkill.RatingMaximum;
                                    if (intLoopRatingMaximum > intMaxRating)
                                        intMaxRating = intLoopRatingMaximum;
                                });

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
                                string strForceValue = objXmlComplexForm.Attributes?["select"]?.InnerTextViaPool() ?? string.Empty;
                                XmlNode objXmlComplexFormData = objXmlProgramDocument.TryGetNodeByNameOrId("/chummer/complexforms/complexform", objXmlComplexForm.InnerTextViaPool());
                                ComplexForm objComplexForm = new ComplexForm(_objCharacter);
                                try
                                {
                                    objComplexForm.Create(objXmlComplexFormData, strForceValue);
                                    _objCharacter.ComplexForms.Add(objComplexForm);
                                }
                                catch
                                {
                                    objComplexForm.Remove(false);
                                    throw;
                                }
                            }

                            // Add any Gear the Critter comes with (typically Programs for A.I.s)
                            XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");
                            foreach (XmlNode objXmlGear in objXmlCritter.SelectNodes("gears/gear"))
                            {
                                int intRating = 0;
                                if (objXmlGear.Attributes["rating"] != null)
                                    intRating = CommonFunctions.ExpressionToInt(objXmlGear.Attributes["rating"].InnerTextViaPool(), intForce, 0, 0);
                                string strForceValue = objXmlGear.Attributes?["select"]?.InnerTextViaPool() ?? string.Empty;
                                XmlNode objXmlGearItem = objXmlGearDocument.TryGetNodeByNameOrId("/chummer/gears/gear", objXmlGear.InnerTextViaPool());
                                List<Weapon> lstWeapons = new List<Weapon>(1);
                                Gear objGear = new Gear(_objCharacter);
                                try
                                {
                                    objGear.Create(objXmlGearItem, intRating, lstWeapons, strForceValue);
                                    objGear.Cost = "0";
                                    _objCharacter.Gear.Add(objGear);
                                    foreach (Weapon objWeapon in lstWeapons)
                                        _objCharacter.Weapons.Add(objWeapon);
                                }
                                catch
                                {
                                    foreach (Weapon objWeapon in lstWeapons)
                                        objWeapon.Remove(false);
                                    objGear.Remove(false);
                                    throw;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (_blnAddExceptionInfoToErrors)
                                _sbdOutputBuilder.AppendLine(strName, " general failure. Exception: ", e.ToString());
                            else
                                _sbdOutputBuilder.AppendLine(strName, " general failure");
                        }
                    }
                }
            }
        }
    }
}
