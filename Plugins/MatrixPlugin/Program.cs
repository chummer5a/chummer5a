using System.Xml.Serialization;

namespace MatrixPlugin
{
    /// <summary>
    /// Class with representation of Common and Hacking Programs
    /// </summary>
    [XmlType("gear")]
    public class Program
    {

        [XmlElement("id")]
        public string ID { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlArray("modifiers")]
        public ProgramModifier[] Modifiers { get; set; }

        /// <summary>
        /// MatrixLogic object is need for changing modifiers of attributes
        /// </summary>
        [XmlIgnore]
        public MatrixLogic logic { get; set; }

        private bool isActive;
        [XmlIgnore]
        public bool IsActive
        {
            get => isActive;
            // when changed state of isActive we Activate all inner modifiers
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    if (logic != null && Modifiers != null)
                    {
                        foreach (var modifier in Modifiers)
                            modifier.Activate(logic, isActive);
                    }
                }
            }
        }

        public Program()
        {
        }

        /// <summary>
        /// Represents a modifiers that apply common and hacking programs
        /// if ActionName is empty all other properties work on all Matrix Logic,
        /// else all properties work only on Actions with that name
        /// </summary>
        [XmlType("modifier")]
        public class ProgramModifier
        {
            /// <summary>
            /// Name of matrix action
            /// Can be empty
            /// </summary>
            [XmlAttribute("action")]
            public string ActionName { get; set; }

            /// <summary>
            /// Name of attribute or another property
            /// which program change
            /// </summary>
            [XmlAttribute("attribute")]
            public string AttributeName { get; set; }

            [XmlAttribute("value")]
            public int Value { get; set; }

            public ProgramModifier()
            {
            }

            /// <summary>
            /// Activate or deactivate current modifier for MatrixLogic object
            /// </summary>
            /// <param name="logic">Matrix Logic which will be changed</param>
            /// <param name="state">New state of modifier</param>
            public void Activate(MatrixLogic logic, bool state)
            {
                int dValue = state ? Value : -Value;
                //If ActionName is not empty Modifier work on Actions
                if (!string.IsNullOrEmpty(ActionName))
                {
                    foreach (var action in logic.Actions.FindAll(x => x.Name.Contains(ActionName)))
                    {
                        switch (AttributeName)
                        {
                            case "Dicepool":
                                action.ActionModifier += dValue;
                                break;
                            case "DefenceDicepool":
                                action.DefenceModifier += dValue;
                                break;
                            case "Limit":
                                action.LimitModifier += dValue;
                                break;
                            default:
                                break;
                        }
                    }
                }
                //Else modifier work on all Logic
                else
                    switch (AttributeName)
                    {
                        case "Attack":
                            logic.AttackMod += dValue;
                            break;
                        case "Sleaze":
                            logic.SleazeMod += dValue;
                            break;
                        case "DataProcessing":
                            logic.DataProcessingMod += dValue;
                            break;
                        case "Firewall":
                            logic.FirewallMod += dValue;
                            break;
                        default:
                            break;
                    }
            }
        }
    }
}
