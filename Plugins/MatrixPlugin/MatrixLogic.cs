using Chummer;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace MatrixPlugin
{
    public class MatrixLogic : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Character character;

        public List<IHasMatrixAttributes> Persons;
        public List<Gear> Software;
        public List<MatrixAction> Actions;

        private int actionModifier;

        private int currentActionIndex = 0;
        public int CurrentActionIndex
        {
            get => currentActionIndex;
            set
            {
                currentActionIndex = value;
                OnPropertyChanged();
                OnPropertyChanged("currentAction");
                OnPropertyChanged("ActionDicePool");
                OnPropertyChanged("DefenceDicePool");
            }
        }
        public MatrixAction CurrentAction
        {
            get => Actions[currentActionIndex];
        }

        public IHasMatrixAttributes CurrentPerson
        {
            get => character.ActiveCommlink;
            set
            {
                if (character.ActiveCommlink != value)
                {
                    character.ActiveCommlink = value;
                    OnPropertyChanged();
                    OnPropertyChanged("Attack");
                    OnPropertyChanged("Sleaze");
                    OnPropertyChanged("DataProcessing");
                    OnPropertyChanged("Firewall");
                    OnPropertyChanged("AttackMod");
                    OnPropertyChanged("SleazeMod");
                    OnPropertyChanged("DataProcessingMod");
                    OnPropertyChanged("FirewallMod");
                    OnPropertyChanged("TotalAttack");
                    OnPropertyChanged("TotalSleaze");
                    OnPropertyChanged("TotalDataProcessing");
                    OnPropertyChanged("TotalFirewall");
                }
            }
        }

        public bool OverClocker
        {
            get => character.Overclocker;
        }

        public string OverClocked
        {
            get => CurrentPerson.Overclocked;
            set
            {
                CurrentPerson.Overclocked = value;
                OnPropertyChanged();
                OnPropertyChanged("AttackMod");
                OnPropertyChanged("SleazeMod");
                OnPropertyChanged("DataProcessingMod");
                OnPropertyChanged("FirewallMod");
                OnPropertyChanged("TotalAttack");
                OnPropertyChanged("TotalSleaze");
                OnPropertyChanged("TotalDataProcessing");
                OnPropertyChanged("TotalFirewall");
            }
        }

        public int Attack
        {
            get => Parse(CurrentPerson.Attack);
            set
            {
                if (CurrentPerson.Attack != value.ToString())
                {
                    CurrentPerson.Attack = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalAttack");
                }
            }
        }

        public int AttackMod
        {
            get => Parse(CurrentPerson.ModAttack) + (CurrentPerson.Overclocked == "Attack"?1:0);
            set
            {
                if (CurrentPerson.ModAttack != value.ToString())
                {
                    CurrentPerson.ModAttack = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalAttack");
                }
            }
        }

        public int TotalAttack
        {
            get => Parse(CurrentPerson.Attack) + Parse(CurrentPerson.ModAttack) + (CurrentPerson.Overclocked == "Attack" ? 1 : 0);
        }

        public int Sleaze
        {
            get => Parse(CurrentPerson.Sleaze);
            set
            {
                if (CurrentPerson.Sleaze != value.ToString())
                {
                    CurrentPerson.Sleaze = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalSleaze");
                }
            }
        }

        public int SleazeMod
        {
            get => Parse(CurrentPerson.ModSleaze) + (CurrentPerson.Overclocked == "Sleaze" ? 1 : 0);
            set
            {
                if (CurrentPerson.ModSleaze != value.ToString())
                {
                    CurrentPerson.ModSleaze = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalSleaze");
                }
            }
        }

        public int TotalSleaze
        {
            get => Parse(CurrentPerson.Sleaze) + Parse(CurrentPerson.ModSleaze) + (CurrentPerson.Overclocked == "Sleaze" ? 1 : 0);
        }

        public int DataProcessing
        {
            get => Parse(CurrentPerson.DataProcessing);
            set
            {
                if (CurrentPerson.DataProcessing != value.ToString())
                {
                    CurrentPerson.DataProcessing = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalDataProcessing");
                }
            }
        }

        public int DataProcessingMod
        {
            get => Parse(CurrentPerson.ModDataProcessing) + (CurrentPerson.Overclocked == "Data Processing" ? 1 : 0);
            set
            {
                if (CurrentPerson.ModDataProcessing != value.ToString())
                {
                    CurrentPerson.ModDataProcessing = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalDataProcessing");
                }
            }
        }

        public int TotalDataProcessing
        {
            get => Parse(CurrentPerson.DataProcessing) + Parse(CurrentPerson.ModDataProcessing) + (CurrentPerson.Overclocked == "DataProcessing" ? 1 : 0);
        }

        public int Firewall
        {
            get => Parse(CurrentPerson.Firewall);
            set
            {
                if (CurrentPerson.Firewall != value.ToString())
                {
                    CurrentPerson.Firewall = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalFirewall");
                }
            }
        }

        public int FirewallMod
        {
            get => Parse(CurrentPerson.ModFirewall) + (CurrentPerson.Overclocked == "Firewall" ? 1 : 0);
            set
            {
                if (CurrentPerson.ModFirewall != value.ToString())
                {
                    CurrentPerson.ModFirewall = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalFirewall");
                }
            }
        }

        public int TotalFirewall
        {
            get => Parse(CurrentPerson.Firewall) + Parse(CurrentPerson.ModFirewall) + (CurrentPerson.Overclocked == "Firewall" ? 1 : 0);
        }

        public int ActionDicePool
        {
            get =>
                GetTotalAttribute(CurrentAction.ActionAttribute) +
                GetTotalSkill(CurrentAction.ActionSkill) +
                CurrentAction.ActionModifier +
                ActionModifier;
        }

        public int DefenceDicePool
        {
            get =>
                GetTotalSkill(CurrentAction.DefenceSkill) +
                GetTotalAttribute(CurrentAction.DefenceAttribute) +
                CurrentAction.DefenceModifier +
                ActionModifier;
        }
        public int ActionModifier
        {
            get => actionModifier;
            set
            {
                actionModifier = value;
                OnPropertyChanged();
                OnPropertyChanged("ActionDicePool");
                OnPropertyChanged("DefenceDicePool");
            }
        }

        public int GetTotalMatrixAttribute(string attribute)
        {
            switch (attribute)
            {
                case "Attack":
                    return this.TotalAttack;
                case "Sleaze":
                    return this.TotalSleaze;
                case "Data Processing":
                    return this.TotalDataProcessing;
                case "Firewall":
                    return this.TotalFirewall;
            }
            return 0;
        }

        public int GetTotalSkill(string skill)
        {
            if (character.SkillsSection.GetActiveSkill(skill) != null)
                return character.SkillsSection.GetActiveSkill(skill).TotalBaseRating;
            return 0;
        }
        public int GetTotalAttribute(string attribute)
        {
            if (character.GetAttribute(attribute) != null)
                return character.GetAttribute(attribute).TotalValue;
            return 0;
        }

        private int Parse(string value)
        {
            int.TryParse(value, out int result);
            if (result == 0)
            {
                CharacterAttrib attribute = character.GetAttribute(value.Replace("{", "").Replace("}", ""));
                if (attribute != null)
                    result = attribute.Base;
            }
            return result;
        }

        public MatrixLogic(Character character, List<MatrixAction> matrixActions)
        {
            this.character = character;
            Actions = matrixActions;
            Persons = new List<IHasMatrixAttributes>();
            Software = new List<Gear>();
            //Load all CyberDecks,Commlinks and Programs to the Lists
            foreach (Gear gear in character.Gear)
                if (gear.Category == "Cyberdecks" || gear.Category == "Commlinks")
                    Persons.Add(gear);
                else if (gear.Category.Contains("Program"))
                    Software.Add(gear);
                else if (gear.Children.Count > 0)
                    Software.AddRange(from Gear child in gear.Children
                                      where gear.Category.Contains("Program")
                                      select gear);
        }
        
        public void ActivateSoftware(string software,bool enable)
        {
            switch (software)
            {
                //AttributeSoft
                case "Decryption":
                    AttackMod += enable ? 1 : -1;
                    break;
                case "Stealth":
                    SleazeMod += enable ? 1 : -1;
                    break;
                case "Toolbox":
                    DataProcessingMod += enable ? 1 : -1;
                    break;
                case "Encryption":
                    FirewallMod += enable ? 1 : -1;
                    break;
                //ActionRelatedSoftware
                case "Shredder":
                case "Edit":
                    Actions.Find(i => i.Name == "Edit File").LimitModifier += enable ? +2 : -2;
                    break;
                case "Search":
                    Actions.Find(i => i.Name == "Matrix Search").ActionModifier += enable ? +2 : -2;
                    break;
                case "Exploit":
                    Actions.Find(i => i.Name.Contains("Hack on the Fly")).LimitModifier += enable ? +2 : -2;
                    break;
                case "Paintjob":
                    Actions.Find(i => i.Name.Contains("Erase Marks")).LimitModifier += enable ? +2 : -2;
                    break;
                case "Tarball":
                    Actions.Find(i => i.Name.Contains("Crash Program")).LimitModifier += enable ? +2 : -2;
                    Actions.Find(i => i.Name.Contains("Crash Program")).ActionModifier += enable ? +1 : -1;
                    break;
                default:
                    break;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
