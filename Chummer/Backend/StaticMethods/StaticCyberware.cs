using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chummer.Backend.Equipment;

namespace Chummer.Backend.StaticMethods
{
    /// <summary>
    /// Contains Methods to handle Cyberware
    /// </summary>
    public static class StaticCyberware
    {
        /// <summary>
        /// Moves modular cyberware from one mount to another or to the Character
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="objModularCyberware">The Ware to move</param>
        /// <param name="strSelectedParentID">The ID of the Target, if "None" it will be moved to the Character</param>
        /// <returns></returns>
        public static bool ChangeCyberwareMount(Character objCharacter, Cyberware objModularCyberware, string strSelectedParentID)
        {
            //TODO: Someone who has a clue of WTF is happening here write some comments. I just cuted it down into smaller less indented pieces.
            Cyberware objOldParent = objModularCyberware.Parent;

            if (objOldParent != null)
                objModularCyberware.ChangeModularEquip(false);

            if (strSelectedParentID == "None")
            {
                RemoveCyberwareFromMount();
                return true;
            }

            Cyberware objNewParent = objCharacter.Cyberware.DeepFindById(strSelectedParentID);
            if (objNewParent != null)
            {
                AddCyberWareToNewMount();
                return true;
            }

            VehicleMod objNewVehicleModParent = objCharacter.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedParentID);
            if (objNewVehicleModParent == null)
                objNewParent = objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedParentID, out objNewVehicleModParent);

            if (objNewVehicleModParent != null || objNewParent != null)
            {
                AddCyberwareToNewVehicleMount();
                return true;
            }

            if (objOldParent != null)
            {
                objOldParent.Children.Remove(objModularCyberware);

                objCharacter.Cyberware.Add(objModularCyberware);

                return true;
            }

            return true;

            #region Local Functions

            void RemoveCyberwareFromMount()
            {
                if (objOldParent == null) return;

                objOldParent.Children.Remove(objModularCyberware);

                objCharacter.Cyberware.Add(objModularCyberware);
            }

            void AddCyberWareToNewMount()
            {
                if (objOldParent != null)
                    objOldParent.Children.Remove(objModularCyberware);
                else
                    objCharacter.Cyberware.Remove(objModularCyberware);

                objNewParent.Children.Add(objModularCyberware);

                objModularCyberware.ChangeModularEquip(true);
            }

            void AddCyberwareToNewVehicleMount()
            {
                if (objOldParent != null)
                    objOldParent.Children.Remove(objModularCyberware);
                else
                    objCharacter.Cyberware.Remove(objModularCyberware);

                if (objNewParent != null)
                    objNewParent.Children.Add(objModularCyberware);
                else
                {
                    objNewVehicleModParent?.Cyberware.Add(objModularCyberware);
                }
            }

            #endregion
        }

        /// <summary>
        /// Moves modular vehicle cyberware from one mount to another or to the Vehicle.
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="objModularCyberware">The Ware to move</param>
        /// <param name="strSelectedParentID">The ID of the new target. If "None" then it will be moved to the Vehicle</param>
        /// <returns></returns>
        public static bool ChangeVehicleCyberwareMount(Character objCharacter, Cyberware objModularCyberware, string strSelectedParentID)
        {
            #region Main Method

            //TODO: Someone who has a clue of WTF is happening here write some comments. I just cuted it down into smaller less indented pieces.
            objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == objModularCyberware.InternalId, out VehicleMod objOldParentVehicleMod);

            Cyberware objOldParent = objModularCyberware.Parent;
            if (objOldParent != null)
                objModularCyberware.ChangeModularEquip(false);
            if (strSelectedParentID == "None")
            {
                MoveCyberwareFromMountToCharacter();
                return true;
            }

            var objNewParent = objCharacter.Cyberware.DeepFindById(strSelectedParentID);
            if (objNewParent != null)
            {
                MoveModularCyberwareToNewMount();
                return true;
            }

            var objNewVehicleModParent = objCharacter.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedParentID);

            if (objNewVehicleModParent == null)
                objNewParent = objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedParentID, out objNewVehicleModParent);

            if (objNewVehicleModParent != null || objNewParent != null)
            {
                MoveVehicleModAndVehicleCyberwareToNewParent();
                return true;
            }

            if (objOldParent != null)
                objOldParent.Children.Remove(objModularCyberware);
            else
                objOldParentVehicleMod.Cyberware.Remove(objModularCyberware);

            objCharacter.Cyberware.Add(objModularCyberware);
            return true;


            #endregion

            //These can probably be merged with the local functions from ChangeCyberwareMount(), the main difference is that these sometimes target the Vehicle instead of the Character.
            #region Local Functions


            void MoveCyberwareFromMountToCharacter()
            {
                if (objOldParent != null)
                    objOldParent.Children.Remove(objModularCyberware);
                else
                    objOldParentVehicleMod.Cyberware.Remove(objModularCyberware);

                objCharacter.Cyberware.Add(objModularCyberware);
            }

            void MoveModularCyberwareToNewMount()
            {
                if (objOldParent != null)
                    objOldParent.Children.Remove(objModularCyberware);
                else
                    objOldParentVehicleMod.Cyberware.Remove(objModularCyberware);

                objNewParent.Children.Add(objModularCyberware);

                objModularCyberware.ChangeModularEquip(true);
            }

            void MoveVehicleModAndVehicleCyberwareToNewParent()
            {
                if (objOldParent != null)
                    objOldParent.Children.Remove(objModularCyberware);
                else
                    objOldParentVehicleMod.Cyberware.Remove(objModularCyberware);

                if (objNewParent != null)
                    objNewParent.Children.Add(objModularCyberware);
                else
                {
                    objNewVehicleModParent?.Cyberware.Add(objModularCyberware);
                }
            }

            #endregion
        }


        #region Private Methods


        #endregion
    }
}
