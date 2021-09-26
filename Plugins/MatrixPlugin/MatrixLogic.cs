using Chummer;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MatrixPlugin
{
    public class MatrixLogic : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Character _character;

        public List<Gear> Persons;
        public List<Gear> Software;
        public List<MatrixAction> Actions;
        public MatrixLogic(Character character, List<MatrixAction> matrixActions)
        {
            _character = character;
            _character.PropertyChanged += _character_PropertyChanged;
            _character.Gear.CollectionChanged += Gear_CollectionChanged;
            Actions = matrixActions;
            Persons = new List<Gear>();
            Software = new List<Gear>();

            //Load all CyberDecks,Commlinks and Programs to the Lists
            AddEquipment(character.Gear);
        }

        private void AddEquipment(IList gears)
        {
            foreach (Gear gear in gears)
                if (gear.Category == "Cyberdecks" || gear.Category == "Commlinks")
                    Persons.Add(gear);
                else if (gear.Category.Contains("Program"))
                    Software.Add(gear);
                else if (gear.Children.Count > 0)
                    AddEquipment(gear.Children);
        }

        private void Gear_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                AddEquipment(e.NewItems);
            if (e.OldItems != null)
            foreach(Gear gear in e.OldItems)
            {
                if (gear.Category == "Cyberdecks" || gear.Category == "Commlinks")
                    Persons.Remove(gear);
                else if (gear.Category.Contains("Program"))
                    Software.Remove(gear);
            }
        }

        private void _character_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Character.WoundModifier))
            {
                OnPropertyChanged(nameof(WoundModifier));
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
            if (_character.SkillsSection.GetActiveSkill(skill) != null)
                return _character.SkillsSection.GetActiveSkill(skill).TotalBaseRating;
            return 0;
        }
        public int GetTotalAttribute(string attribute)
        {
            if (_character.GetAttribute(attribute) != null)
                return _character.GetAttribute(attribute).TotalValue;
            return 0;
        }

        private int Parse(string value)
        {
            int.TryParse(value, out int result);
            if (result == 0)
            {
                CharacterAttrib attribute = _character.GetAttribute(value.Replace("{", "").Replace("}", ""));
                if (attribute != null)
                    result = attribute.Base;
            }
            return result;
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

        #region Properties

        public IHasMatrixAttributes CurrentPerson
        {
            get => _character.ActiveCommlink;
            set
            {
                if (_character.ActiveCommlink != value)
                {
                    _character.ActiveCommlink = value;
                    OnPropertyChanged();
                }
            }
        }

        private int currentActionIndex = 0;
        public int CurrentActionIndex
        {
            get => currentActionIndex;
            set
            {
                currentActionIndex = value;
                OnPropertyChanged();
            }
        }
        public MatrixAction CurrentAction => Actions[currentActionIndex];
        
        public bool OverClocker => _character.Overclocker;
        public int WoundModifier => _character.WoundModifier;

        public string OverClocked
        {
            get => CurrentPerson.Overclocked;
            set
            {
                CurrentPerson.Overclocked = value;
                OnPropertyChanged();
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
                }
            }
        }

        public int TotalAttack => Parse(CurrentPerson.Attack) + Parse(CurrentPerson.ModAttack) + (CurrentPerson.Overclocked == "Attack" ? 1 : 0);
        

        public int Sleaze
        {
            get => Parse(CurrentPerson.Sleaze);
            set
            {
                if (CurrentPerson.Sleaze != value.ToString())
                {
                    CurrentPerson.Sleaze = value.ToString();
                    OnPropertyChanged();
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
                }
            }
        }

        public int TotalSleaze => Parse(CurrentPerson.Sleaze) + Parse(CurrentPerson.ModSleaze) + (CurrentPerson.Overclocked == "Sleaze" ? 1 : 0);

        public int DataProcessing
        {
            get => Parse(CurrentPerson.DataProcessing);
            set
            {
                if (CurrentPerson.DataProcessing != value.ToString())
                {
                    CurrentPerson.DataProcessing = value.ToString();
                    OnPropertyChanged();
                }
            }
        }

        public int DataProcessingMod
        {
            get => Parse(CurrentPerson.ModDataProcessing) + (CurrentPerson.Overclocked == "DataProcessing" ? 1 : 0);
            set
            {
                if (CurrentPerson.ModDataProcessing != value.ToString())
                {
                    CurrentPerson.ModDataProcessing = value.ToString();
                    OnPropertyChanged();
                }
            }
        }

        public int TotalDataProcessing => Parse(CurrentPerson.DataProcessing) + Parse(CurrentPerson.ModDataProcessing) + (CurrentPerson.Overclocked == "DataProcessing" ? 1 : 0);
        

        public int Firewall
        {
            get => Parse(CurrentPerson.Firewall);
            set
            {
                if (CurrentPerson.Firewall != value.ToString())
                {
                    CurrentPerson.Firewall = value.ToString();
                    OnPropertyChanged();
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
                }
            }
        }

        public int TotalFirewall => Parse(CurrentPerson.Firewall) + Parse(CurrentPerson.ModFirewall) + (CurrentPerson.Overclocked == "Firewall" ? 1 : 0);
        
        public int ActionDicePool => GetTotalAttribute(CurrentAction.ActionAttribute) +
                GetTotalSkill(CurrentAction.ActionSkill) +
                CurrentAction.ActionModifier +
                WoundModifier +
                ActionModifier;
        
        public int DefenceDicePool => GetTotalSkill(CurrentAction.DefenceSkill) +
                GetTotalAttribute(CurrentAction.DefenceAttribute) +
                WoundModifier +
                CurrentAction.DefenceModifier;

        private int actionModifier;
        public int ActionModifier
        {
            get => actionModifier;
            set
            {
                actionModifier = value;
                OnPropertyChanged();
            }
        }
        #endregion Properties

        #region DependencyGraphTrees
        private static readonly DependencyGraphNode<string, MatrixLogic> _graphOverClocked =
                new DependencyGraphNode<string, MatrixLogic>(nameof(OverClocked),
                    new DependencyGraphNode<string, MatrixLogic>(nameof(OverClocker),
                        new DependencyGraphNode<string, MatrixLogic>(nameof(CurrentPerson))
                        )
                    
                );

        private static readonly DependencyGraphNode<string, MatrixLogic> _graphAttack =
                new DependencyGraphNode<string, MatrixLogic>(nameof(TotalAttack),
                    new DependencyGraphNode<string, MatrixLogic>(nameof(Attack),
                        new DependencyGraphNode<string, MatrixLogic>(nameof(CurrentPerson))
                        ),
                    new DependencyGraphNode<string, MatrixLogic>(nameof(AttackMod),
                        _graphOverClocked
                    )
                );

        private static readonly DependencyGraphNode<string, MatrixLogic> _graphSleaze =
                new DependencyGraphNode<string, MatrixLogic>(nameof(TotalSleaze),
                    new DependencyGraphNode<string, MatrixLogic>(nameof(Sleaze),
                        new DependencyGraphNode<string, MatrixLogic>(nameof(CurrentPerson))
                        ),
                    new DependencyGraphNode<string, MatrixLogic>(nameof(SleazeMod),
                        _graphOverClocked
                    )
                );

        private static readonly DependencyGraphNode<string, MatrixLogic> _graphDataProcessing =
                new DependencyGraphNode<string, MatrixLogic>(nameof(TotalDataProcessing),
                    new DependencyGraphNode<string, MatrixLogic>(nameof(DataProcessing),
                        new DependencyGraphNode<string, MatrixLogic>(nameof(CurrentPerson))
                        ),
                    new DependencyGraphNode<string, MatrixLogic>(nameof(DataProcessingMod),
                        _graphOverClocked
                    )
                );

        private static readonly DependencyGraphNode<string, MatrixLogic> _graphFirewall =
                new DependencyGraphNode<string, MatrixLogic>(nameof(TotalFirewall),
                    new DependencyGraphNode<string, MatrixLogic>(nameof(Firewall),
                        new DependencyGraphNode<string, MatrixLogic>(nameof(CurrentPerson))
                        ),
                    new DependencyGraphNode<string, MatrixLogic>(nameof(FirewallMod),
                        _graphOverClocked
                    )
                );

        private static readonly DependencyGraphNode<string, MatrixLogic> _graphCurrentAction =
                new DependencyGraphNode<string, MatrixLogic>(nameof(CurrentAction),
                    new DependencyGraphNode<string, MatrixLogic>(nameof(CurrentActionIndex))
                );

        private static readonly DependencyGraph<string, MatrixLogic> _MatrixLogicDependacyGraph =
            new DependencyGraph<string, MatrixLogic>(
                new DependencyGraphNode<string, MatrixLogic>(nameof(ActionDicePool),
                    new DependencyGraphNode<string, MatrixLogic>(nameof(ActionModifier),
                        _graphCurrentAction,
                        _graphAttack,
                        _graphSleaze,
                        _graphDataProcessing,
                        _graphFirewall
                        ),
                        new DependencyGraphNode<string, MatrixLogic>(nameof(WoundModifier)),
                    new DependencyGraphNode<string, MatrixLogic>(nameof(_character)+"."+nameof(Character.WoundModifier))
                    ),
                new DependencyGraphNode<string, MatrixLogic>(nameof(DefenceDicePool),
                    _graphCurrentAction,
                    _graphAttack,
                    _graphSleaze,
                    _graphDataProcessing,
                    _graphFirewall,
                    new DependencyGraphNode<string, MatrixLogic>(nameof(WoundModifier)),
                    new DependencyGraphNode<string, MatrixLogic>(nameof(Character.WoundModifier))
                    )
                );

        #endregion DependencyGraphTrees
        protected void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }
        protected void OnMultiplePropertyChanged(params string[] lstPropertyNames)
        {
            ICollection<string> lstNamesOfChangedProperties = null;
            foreach (string strPropertyName in lstPropertyNames)
            {
                if (lstNamesOfChangedProperties == null)
                    lstNamesOfChangedProperties = _MatrixLogicDependacyGraph.GetWithAllDependents(this, strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in _MatrixLogicDependacyGraph.GetWithAllDependents(this, strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if (lstNamesOfChangedProperties == null || lstNamesOfChangedProperties.Count == 0)
                return;

            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }
        }
    }
}
