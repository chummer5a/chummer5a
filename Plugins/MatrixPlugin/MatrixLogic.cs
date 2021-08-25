using Chummer;
using Chummer.Backend.Equipment;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MatrixPlugin
{
    public class MatrixLogic : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Character _character;

        public List<IHasMatrixAttributes> Persons;
        public List<Gear> Software;
        public List<MatrixAction> Actions;

        private int _currentActionIndex = 0;
        public int currentActionIndex
        {
            get => _currentActionIndex;
            set
            {
                _currentActionIndex = value;
                OnPropertyChanged();
                OnPropertyChanged("currentAction");
                OnPropertyChanged("ActionDicePool");
                OnPropertyChanged("DefenceDicePool");
            }
        }
        public MatrixAction currentAction
        {
            get => Actions[_currentActionIndex];
            set
            {
                if (Actions.Contains(value))
                {
                    _currentActionIndex = Actions.IndexOf(value);
                    OnPropertyChanged();
                }
            }
        }

        public IHasMatrixAttributes currentPerson
        {
            get => _character.ActiveCommlink;
            set
            {
                if (_character.ActiveCommlink != value)
                {
                    _character.ActiveCommlink = value;
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
            get => _character.Overclocker;
        }

        public string OverClocked
        {
            get => currentPerson.Overclocked;
            set
            {
                currentPerson.Overclocked = value;
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
            get => Parse(currentPerson.Attack);
            set
            {
                if (currentPerson.Attack != value.ToString())
                {
                    currentPerson.Attack = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalAttack");
                }
            }
        }

        public int AttackMod
        {
            get => Parse(currentPerson.ModAttack) + (currentPerson.Overclocked == "Attack"?1:0);
            set
            {
                if (currentPerson.ModAttack != value.ToString())
                {
                    currentPerson.ModAttack = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalAttack");
                }
            }
        }

        public int TotalAttack
        {
            get => Parse(currentPerson.Attack) + Parse(currentPerson.ModAttack) + (currentPerson.Overclocked == "Attack" ? 1 : 0);
        }

        public int Sleaze
        {
            get => Parse(currentPerson.Sleaze);
            set
            {
                if (currentPerson.Sleaze != value.ToString())
                {
                    currentPerson.Sleaze = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalSleaze");
                }
            }
        }

        public int SleazeMod
        {
            get => Parse(currentPerson.ModSleaze) + (currentPerson.Overclocked == "Sleaze" ? 1 : 0);
            set
            {
                if (currentPerson.ModSleaze != value.ToString())
                {
                    currentPerson.ModSleaze = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalSleaze");
                }
            }
        }

        public int TotalSleaze
        {
            get => Parse(currentPerson.Sleaze) + Parse(currentPerson.ModSleaze) + (currentPerson.Overclocked == "Sleaze" ? 1 : 0);
        }

        public int DataProcessing
        {
            get => Parse(currentPerson.DataProcessing);
            set
            {
                if (currentPerson.DataProcessing != value.ToString())
                {
                    currentPerson.DataProcessing = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalDataProcessing");
                }
            }
        }

        public int DataProcessingMod
        {
            get => Parse(currentPerson.ModDataProcessing) + (currentPerson.Overclocked == "Data Processing" ? 1 : 0);
            set
            {
                if (currentPerson.ModDataProcessing != value.ToString())
                {
                    currentPerson.ModDataProcessing = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalDataProcessing");
                }
            }
        }

        public int TotalDataProcessing
        {
            get => Parse(currentPerson.DataProcessing) + Parse(currentPerson.ModDataProcessing) + (currentPerson.Overclocked == "DataProcessing" ? 1 : 0);
        }

        public int Firewall
        {
            get => Parse(currentPerson.Firewall);
            set
            {
                if (currentPerson.Firewall != value.ToString())
                {
                    currentPerson.Firewall = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalFirewall");
                }
            }
        }

        public int FirewallMod
        {
            get => Parse(currentPerson.ModFirewall) + (currentPerson.Overclocked == "Firewall" ? 1 : 0);
            set
            {
                if (currentPerson.ModFirewall != value.ToString())
                {
                    currentPerson.ModFirewall = value.ToString();
                    OnPropertyChanged();
                    OnPropertyChanged("TotalFirewall");
                }
            }
        }

        public int TotalFirewall
        {
            get => Parse(currentPerson.Firewall) + Parse(currentPerson.ModFirewall) + (currentPerson.Overclocked == "Firewall" ? 1 : 0);
        }

        public int ActionDicePool
        {
            get =>
                getTotalAttribute(currentAction.ActionAttribute) +
                getTotalSkill(currentAction.ActionSkill) +
                currentAction.ActionModifier;
        }

        public int DefenceDicePool
        {
            get =>
                getTotalSkill(currentAction.DefenceSkill) +
                getTotalAttribute(currentAction.DefenceAttribute) +
                currentAction.DefenceModifier;
        }

        public int getTotalMatrixAttribute(string attribute)
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

        public int getTotalSkill(string skill)
        {
            if (_character.SkillsSection.GetActiveSkill(skill) != null)
                return _character.SkillsSection.GetActiveSkill(skill).TotalBaseRating;
            return 0;
        }
        public int getTotalAttribute(string attribute)
        {
            if (_character.GetAttribute(attribute) != null)
                return _character.GetAttribute(attribute).TotalValue;
            return 0;
        }

        private int Parse(string value)
        {
            int result = 0;
            int.TryParse(value, out result);
            return result;
        }

        public MatrixLogic(Character character, List<MatrixAction> matrixActions)
        {
            _character = character;
            Persons = new List<IHasMatrixAttributes>();
            Software = new List<Gear>();
            Actions = matrixActions;
            foreach (Gear gear in character.Gear)
                if (gear.Category == "Cyberdecks")
                    Persons.Add(gear);
                else if (gear.Category.Contains("Program"))
                    Software.Add(gear);
                else if (gear.Children.Count > 0)
                    foreach(Gear child in gear.Children)
                        if (gear.Category.Contains("Program"))
                            Software.Add(gear);
        }

        private void AddModifier(string attribute,int value)
        {
            int prevValue = 0;
            switch (attribute)
            {
                case "Attack":
                    if (!int.TryParse(currentPerson.ModAttack, out prevValue))
                        prevValue = 0;
                    currentPerson.ModAttack = (prevValue + value).ToString();
                    break;
                case "Sleaze":
                    if (!int.TryParse(currentPerson.ModSleaze, out prevValue))
                        prevValue = 0;
                    currentPerson.ModSleaze = (prevValue + value).ToString();
                    break;
                case "Data Processing":
                    if (!int.TryParse(currentPerson.ModDataProcessing, out prevValue))
                        prevValue = 0;
                    currentPerson.ModDataProcessing = (prevValue + value).ToString();
                    break;
                case "Firewall":
                    if (!int.TryParse(currentPerson.ModFirewall,out prevValue))
                        prevValue = 0;
                    currentPerson.ModFirewall = (prevValue + value).ToString();
                    break;
                default:
                    break;
            }
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
