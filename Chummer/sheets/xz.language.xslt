<?xml version="1.0" encoding="utf-8" ?>
<!-- Isolate US English (en-us) locale literals -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="lang"  select="'en'"/>
  <xsl:variable name="locale"  select="'en-us'"/>

  <!-- individual words -->
  <xsl:variable name="lang.Acceleration"  select="'Acceleration'"/>
  <xsl:variable name="lang.Accel"      select="'Accel'"/>
  <xsl:variable name="lang.Accessories"  select="'Accessories'"/>
  <xsl:variable name="lang.Accuracy"    select="'Accuracy'"/>
  <xsl:variable name="lang.Acid"      select="'Acid'"/>
  <xsl:variable name="lang.Action"      select="'Action'"/>
  <xsl:variable name="lang.Addiction"  select="'Addiction'"/>
  <xsl:variable name="lang.Adept"      select="'Adept'"/>
  <xsl:variable name="lang.Age"      select="'Age'"/>
  <xsl:variable name="lang.AGI"      select="'AGI'"/>
  <xsl:variable name="lang.Agility"    select="'Agility'"/>
  <xsl:variable name="lang.AI"      select="'AI'"/>
  <xsl:variable name="lang.Alias"      select="'Alias'"/>
  <xsl:variable name="lang.Ammo"      select="'Ammo'"/>
  <xsl:variable name="lang.Amount"    select="'Amount'"/>
  <xsl:variable name="lang.AP"      select="'AP'"/>
  <xsl:variable name="lang.Applicable"  select="'Applicable'"/>
  <xsl:variable name="lang.Apprentice"  select="'Apprentice'"/>
  <xsl:variable name="lang.AR"      select="'AR'"/>
  <xsl:variable name="lang.Archetype"    select="'Archetype'"/>
  <xsl:variable name="lang.Area"      select="'Area'"/>
  <xsl:variable name="lang.Armor"      select="'Armor'"/>
  <xsl:variable name="lang.Arts"      select="'Arts'"/>
  <xsl:variable name="lang.as"      select="'as'"/>
  <xsl:variable name="lang.ASDF"    select="'A/S/D/F'"/>
  <xsl:variable name="lang.Astral"    select="'Astral'"/>
  <xsl:variable name="lang.Attack"    select="'Attack'"/>
  <xsl:variable name="lang.ATT"      select="'ATT'"/>
  <xsl:variable name="lang.Attribute"    select="'Attribute'"/>
  <xsl:variable name="lang.Attributes"  select="'Attributes'"/>
  <xsl:variable name="lang.Available"    select="'Available'"/>
  <xsl:variable name="lang.Awakened"    select="'Awakened'"/>
  <xsl:variable name="lang.Aware"    select="'Aware'"/>
  <xsl:variable name="lang.Background"  select="'Background'"/>
  <xsl:variable name="lang.Base"      select="'Base'"/>
  <xsl:variable name="lang.Bioware"    select="'Bioware'"/>
  <xsl:variable name="lang.BOD"      select="'BOD'"/>
  <xsl:variable name="lang.Body"      select="'Body'"/>
  <xsl:variable name="lang.Bonus"      select="'Bonus'"/>
  <xsl:variable name="lang.Bound"      select="'Bound'"/>
  <xsl:variable name="lang.Calendar"    select="'Calendar'"/>
  <xsl:variable name="lang.Career"    select="'Career'"/>
  <xsl:variable name="lang.Category"    select="'Category'"/>
  <xsl:variable name="lang.CHA"      select="'CHA'"/>
  <xsl:variable name="lang.Charisma"    select="'Charisma'"/>
  <xsl:variable name="lang.CM"      select="'CM'"/>
  <xsl:variable name="lang.Cold"      select="'Cold'"/>
  <xsl:variable name="lang.Combat"    select="'Combat'"/>
  <xsl:variable name="lang.Commlink"    select="'Commlink'"/>
  <xsl:variable name="lang.Composure"    select="'Composure'"/>
  <xsl:variable name="lang.Concept"    select="'Concept'"/>
  <xsl:variable name="lang.Connection"  select="'Connection'"/>
  <xsl:variable name="lang.Contact"    select="'Contact'"/>
  <xsl:variable name="lang.ContactDrug"    select="'Contact'"/>
  <xsl:variable name="lang.Contacts"    select="'Contacts'"/>
  <xsl:variable name="lang.Cost"      select="'Cost'"/>
  <xsl:variable name="lang.Critter"    select="'Critter'"/>
  <xsl:variable name="lang.Critters"    select="'Critters'"/>
  <xsl:variable name="lang.Cyberware"    select="'Cyberware'"/>
  <xsl:variable name="lang.Damage"    select="'Damage'"/>
  <xsl:variable name="lang.Data"      select="'Data'"/>
  <xsl:variable name="lang.Date"      select="'Date'"/>
  <xsl:variable name="lang.Day"      select="'Day'"/>
  <xsl:variable name="lang.Days"      select="'Days'"/>
  <xsl:variable name="lang.Dead"      select="'Dead'"/>
  <xsl:variable name="lang.Defense"      select="'Defense'"/>
  <xsl:variable name="lang.DEP"  select="'DEP'"/>
  <xsl:variable name="lang.Depth"  select="'Depth'"/>
  <xsl:variable name="lang.Description"  select="'Description'"/>
  <xsl:variable name="lang.Detection"    select="'Detection'"/>
  <xsl:variable name="lang.Device"    select="'Device'"/>
  <xsl:variable name="lang.Devices"    select="'Devices'"/>
  <xsl:variable name="lang.Direct"      select="'Direct'"/>
  <xsl:variable name="lang.Down"      select="'Down'"/>
  <xsl:variable name="lang.DP"        select="'DP'"/>
  <xsl:variable name="lang.Drain"      select="'Drain'"/>
  <xsl:variable name="lang.Drone"      select="'Drone'"/>
  <xsl:variable name="lang.Duration"    select="'Duration'"/>
  <xsl:variable name="lang.DV"      select="'DV'"/>
  <xsl:variable name="lang.E"        select="'E'"/>
  <xsl:variable name="lang.Echo"    select="'Echo'"/>
  <xsl:variable name="lang.Echoes"    select="'Echoes'"/>
  <xsl:variable name="lang.EDG"      select="'EDG'"/>
  <xsl:variable name="lang.Edge"      select="'Edge'"/>
  <xsl:variable name="lang.Electricity"    select="'Electricity'"/>
  <xsl:variable name="lang.Enchanter"    select="'Enchanter'"/>
  <xsl:variable name="lang.Enemies"    select="'Enemies'"/>
  <xsl:variable name="lang.Enhancements"  select="'Enhancements'"/>
  <xsl:variable name="lang.Entries"    select="'Entries'"/>
  <xsl:variable name="lang.Equipped"    select="'Equipped'"/>
  <xsl:variable name="lang.ESS"      select="'ESS'"/>
  <xsl:variable name="lang.Essence"    select="'Essence'"/>
  <xsl:variable name="lang.Expenses"    select="'Expenses'"/>
  <xsl:variable name="lang.Explorer"    select="'Explorer'"/>
  <xsl:variable name="lang.Eyes"      select="'Eyes'"/>
  <xsl:variable name="lang.Falling"    select="'Falling'"/>
  <xsl:variable name="lang.Fatigue"      select="'Fatigue'"/>
  <xsl:variable name="lang.Fettered"      select="'Fettered'"/>
  <xsl:variable name="lang.Fire"    select="'Fire'"/>
  <xsl:variable name="lang.Firewall"    select="'Firewall'"/>
  <xsl:variable name="lang.Fly"      select="'Fly'"/>
  <xsl:variable name="lang.Foci"      select="'Foci'"/>
  <xsl:variable name="lang.FWL"      select="'FWL'"/>
  <xsl:variable name="lang.Force"      select="'Force'"/>
  <xsl:variable name="lang.FV"      select="'FV'"/>
  <xsl:variable name="lang.Gear"      select="'Gear'"/>
  <xsl:variable name="lang.Gender"      select="'Gender'"/>
  <xsl:variable name="lang.Grade"      select="'Grade'"/>
  <xsl:variable name="lang.Hair"      select="'Hair'"/>
  <xsl:variable name="lang.Handling"    select="'Handling'"/>
  <xsl:variable name="lang.Health"    select="'Health'"/>
  <xsl:variable name="lang.Heavy"    select="'Heavy'"/>
  <xsl:variable name="lang.Height"    select="'Height'"/>
  <xsl:variable name="lang.hit"      select="'hit'"/>
  <xsl:variable name="lang.Illusion"    select="'Illusion'"/>
  <xsl:variable name="lang.Implant"    select="'Implant'"/>
  <xsl:variable name="lang.Indirect"      select="'Indirect'"/>
  <xsl:variable name="lang.Info"      select="'Info'"/>
  <xsl:variable name="lang.Ingestion"      select="'Ingestion'"/>
  <xsl:variable name="lang.Inhalation"      select="'Inhalation'"/>
  <xsl:variable name="lang.Init"      select="'Init'"/>
  <xsl:variable name="lang.Initiation"  select="'Initiation'"/>
  <xsl:variable name="lang.Initiative"  select="'Initiative'"/>
  <xsl:variable name="lang.Injection"      select="'Injection'"/>
  <xsl:variable name="lang.INT"      select="'INT'"/>
  <xsl:variable name="lang.Intentions"  select="'Intentions'"/>
  <xsl:variable name="lang.Intuition"    select="'Intuition'"/>
  <xsl:variable name="lang.Instantaneous"  select="'Instantaneous'"/>
  <xsl:variable name="lang.Karma"      select="'Karma'"/>
  <xsl:variable name="lang.L"        select="'L'"/>
  <xsl:variable name="lang.Level"      select="'Level'"/>
  <xsl:variable name="lang.Lifestyle"    select="'Lifestyle'"/>
  <xsl:variable name="lang.Limit"      select="'Limit'"/>
  <xsl:variable name="lang.Limits"    select="'Limits'"/>
  <xsl:variable name="lang.Loaded"    select="'Loaded'"/>
  <xsl:variable name="lang.Location"    select="'Location'"/>
  <xsl:variable name="lang.LOG"      select="'LOG'"/>
  <xsl:variable name="lang.Logic"      select="'Logic'"/>
  <xsl:variable name="lang.Loyalty"    select="'Loyalty'"/>
  <xsl:variable name="lang.M"        select="'M'"/>
  <xsl:variable name="lang.MAG"      select="'MAG'"/>
  <xsl:variable name="lang.Magic"      select="'Magic'"/>
  <xsl:variable name="lang.Magician"      select="'Magician'"/>
  <xsl:variable name="lang.Mana"    select="'Mana'"/>
  <xsl:variable name="lang.Maneuvers"    select="'Maneuvers'"/>
  <xsl:variable name="lang.Manipulation"  select="'Manipulation'"/>
  <xsl:variable name="lang.Manual"    select="'Manual'"/>
  <xsl:variable name="lang.Memory"    select="'Memory'"/>
  <xsl:variable name="lang.Mental"    select="'Mental'"/>
  <xsl:variable name="lang.Metamagics"  select="'Metamagics'"/>
  <xsl:variable name="lang.Metatype"    select="'Metatype'"/>
  <xsl:variable name="lang.Mod"      select="'Mod'"/>
  <xsl:variable name="lang.Mode"      select="'Mode'"/>
  <xsl:variable name="lang.Model"      select="'Model'"/>
  <xsl:variable name="lang.Modifications"  select="'Modifications'"/>
  <xsl:variable name="lang.Month"      select="'Month'"/>
  <xsl:variable name="lang.Months"    select="'Months'"/>
  <xsl:variable name="lang.Mount"    select="'Mount'"/>
  <xsl:variable name="lang.Movement"    select="'Movement'"/>
  <xsl:variable name="lang.Mugshot"    select="'Portrait'"/>
  <xsl:variable name="lang.Name"      select="'Name'"/>
  <xsl:variable name="lang.Native"    select="'Native'"/>
  <xsl:variable name="lang.Negative"    select="'Negative'"/>
  <xsl:variable name="lang.No"      select="'No'"/>
  <xsl:variable name="lang.None"      select="'None'"/>
  <xsl:variable name="lang.Notes"      select="'Notes'"/>
  <xsl:variable name="lang.Notoriety"    select="'Notoriety'"/>
  <xsl:variable name="lang.Nuyen"      select="'Nuyen'"/>
  <xsl:variable name="lang.Other"      select="'Other'"/>
  <xsl:variable name="lang.Overflow"      select="'Overflow'"/>
  <xsl:variable name="lang.OVR"      select="'OVR&#160;'"/>
  <xsl:variable name="lang.Pathogen"    select="'Pathogen'"/>
  <xsl:variable name="lang.Permanent"    select="'Permanent'"/>
  <xsl:variable name="lang.Persona"    select="'Persona'"/>
  <xsl:variable name="lang.Pets"      select="'Pets'"/>
  <xsl:variable name="lang.Physical"    select="'Physical'"/>
  <xsl:variable name="lang.Physiological"  select="'Physiological'"/>
  <xsl:variable name="lang.Pilot"      select="'Pilot'"/>
  <xsl:variable name="lang.Player"    select="'Player'"/>
  <xsl:variable name="lang.Points"    select="'Points'"/>
  <xsl:variable name="lang.Pool"      select="'Pool'"/>
  <xsl:variable name="lang.Positive"    select="'Positive'"/>
  <xsl:variable name="lang.Power"      select="'Power'"/>
  <xsl:variable name="lang.Powers"    select="'Powers'"/>
  <xsl:variable name="lang.Priorities"  select="'Priorities'"/>
  <xsl:variable name="lang.Processor"    select="'Processor'"/>
  <xsl:variable name="lang.Program"    select="'Program'"/>
  <xsl:variable name="lang.Programs"    select="'Programs'"/>
  <xsl:variable name="lang.Psychological"  select="'Psychological'"/>
  <xsl:variable name="lang.Qty"      select="'Qty'"/>
  <xsl:variable name="lang.Quality"    select="'Quality'"/>
  <xsl:variable name="lang.Qualities"    select="'Qualities'"/>
  <xsl:variable name="lang.Radiation"      select="'Radiation'"/>
  <xsl:variable name="lang.Range"      select="'Range'"/>
  <xsl:variable name="lang.Rating"    select="'Rating'"/>
  <xsl:variable name="lang.RC"      select="'RC'"/>
  <xsl:variable name="lang.Reaction"    select="'Reaction'"/>
  <xsl:variable name="lang.REA"      select="'REA'"/>
  <xsl:variable name="lang.Reach"      select="'Reach'"/>
  <xsl:variable name="lang.Reason"    select="'Reason'"/>
  <xsl:variable name="lang.Registered"  select="'Registered'"/>
  <xsl:variable name="lang.Requires"    select="'Requires'"/>
  <xsl:variable name="lang.RES"      select="'RES'"/>
  <xsl:variable name="lang.Resistance"    select="'Resistance'"/>
  <xsl:variable name="lang.Resistances"    select="'Resistances'"/>
  <xsl:variable name="lang.Resonance"    select="'Resonance'"/>
  <xsl:variable name="lang.Resources"    select="'Resources'"/>
  <xsl:variable name="lang.Rigger"    select="'Rigger'"/>
  <xsl:variable name="lang.Rtg"      select="'Rtg'"/>
  <xsl:variable name="lang.Run"      select="'Run'"/>
  <xsl:variable name="lang.S"        select="'S'"/>
  <xsl:variable name="lang.Seats"      select="'Seats'"/>
  <xsl:variable name="lang.Self"      select="'Self'"/>
  <xsl:variable name="lang.Services"    select="'Services'"/>
  <xsl:variable name="lang.Sensor"    select="'Sensor'"/>
  <xsl:variable name="lang.Sex"      select="'Sex'"/>
  <xsl:variable name="lang.Show"      select="'Show: '"/>
  <xsl:variable name="lang.Skill"      select="'Skill'"/>
  <xsl:variable name="lang.Skills"    select="'Skills'"/>
  <xsl:variable name="lang.Skin"      select="'Skin'"/>
  <xsl:variable name="lang.Sleaze"    select="'Sleaze'"/>
  <xsl:variable name="lang.SLZ"      select="'SLZ'"/>
  <xsl:variable name="lang.Social"    select="'Social'"/>
  <xsl:variable name="lang.Sonic"      select="'Sonic'"/>
  <xsl:variable name="lang.Source"    select="'Source'"/>
  <xsl:variable name="lang.Special"    select="'Special'"/>
  <xsl:variable name="lang.Speed"      select="'Speed'"/>
  <xsl:variable name="lang.Spell"      select="'Spell'"/>
  <xsl:variable name="lang.Spells"    select="'Spells'"/>
  <xsl:variable name="lang.Spirit"    select="'Spirit'"/>
  <xsl:variable name="lang.Spirits"    select="'Spirits'"/>
  <xsl:variable name="lang.Sprite"    select="'Sprite'"/>
  <xsl:variable name="lang.Sprites"    select="'Sprites'"/>
  <xsl:variable name="lang.Standard"    select="'Standard'"/>
  <xsl:variable name="lang.Stream"    select="'Stream'"/>
  <xsl:variable name="lang.STR"      select="'STR'"/>
  <xsl:variable name="lang.Strength"    select="'Strength'"/>
  <xsl:variable name="lang.Stun"      select="'Stun'"/>
  <xsl:variable name="lang.Submersion"  select="'Submersion'"/>
  <xsl:variable name="lang.Sustained"    select="'Sustained'"/>
  <xsl:variable name="lang.Swim"      select="'Swim'"/>
  <xsl:variable name="lang.Target"    select="'Target'"/>
  <xsl:variable name="lang.Tasks"    select="'Tasks'"/>
  <xsl:variable name="lang.Total"      select="'Total'"/>
  <xsl:variable name="lang.Touch"      select="'Touch'"/>
  <xsl:variable name="lang.Toxin"      select="'Toxin'"/>
  <xsl:variable name="lang.Tradition"    select="'Tradition'"/>
  <xsl:variable name="lang.Type"      select="'Type'"/>
  <xsl:variable name="lang.Unbound"    select="'Unbound'"/>
  <xsl:variable name="lang.Unknown"    select="'Unknown'"/>
  <xsl:variable name="lang.Unregistered"  select="'Unregistered'"/>
  <xsl:variable name="lang.Under"      select="'Under'"/>
  <xsl:variable name="lang.Vehicle"    select="'Vehicle'"/>
  <xsl:variable name="lang.Vehicles"    select="'Vehicles'"/>
  <xsl:variable name="lang.VR"      select="'VR'"/>
  <xsl:variable name="lang.W"        select="'W'"/>
  <xsl:variable name="lang.Walk"      select="'Walk'"/>
  <xsl:variable name="lang.Weaknesses"    select="'Weaknesses'"/>
  <xsl:variable name="lang.Weapon"    select="'Weapon'"/>
  <xsl:variable name="lang.Weapons"    select="'Weapons'"/>
  <xsl:variable name="lang.Week"      select="'Week'"/>
  <xsl:variable name="lang.Weeks"      select="'Weeks'"/>
  <xsl:variable name="lang.Weight"    select="'Weight'"/>
  <xsl:variable name="lang.WIL"      select="'WIL'"/>
  <xsl:variable name="lang.Willpower"    select="'Willpower'"/>
  <xsl:variable name="lang.with"      select="'with'"/>
  <xsl:variable name="lang.Yes"      select="'Yes'"/>

  <!-- multiple word phrases / composite words -->
  <xsl:variable name="lang.ActiveSkills"    select="'Active Skills'"/>
  <xsl:variable name="lang.AdeptPowers"    select="'Adept Powers'"/>
  <xsl:variable name="lang.AIandAdvanced"    select="'AI Programs and Advanced Programs'"/>
  <xsl:variable name="lang.AlreadyAddicted"  select="'Already Addicted'"/>
  <xsl:variable name="lang.ArmorValue"    select="'Value'"/>
  <xsl:variable name="lang.AspectedMagician"    select="'Aspected Magician'"/>
  <xsl:variable name="lang.AstralInitiative"  select="'Astral Initiative'"/>
  <xsl:variable name="lang.AstralReputation"    select="'Astral Reputation'"/>
  <xsl:variable name="lang.CombatSkill"    select="'Combat Skill'"/>
  <xsl:variable name="lang.ComplexForm"    select="'Complex Form'"/>
  <xsl:variable name="lang.ComplexForms"    select="'Complex Forms'"/>
  <xsl:variable name="lang.ConditionMonitor"  select="'Condition Monitor'"/>
  <xsl:variable name="lang.ContactList"    select="'Contact List'"/>
  <xsl:variable name="lang.CoreTrack"  select="'Core Damage Track'"/>
  <xsl:variable name="lang.CritterPower"    select="'Critter Power'"/>
  <xsl:variable name="lang.CritterPowers"    select="'Critter Powers'"/>
  <xsl:variable name="lang.CurrentEdge"    select="'Current Edge Points'"/>
  <xsl:variable name="lang.CurrentForm"    select="'Current Form'"/>
  <xsl:variable name="lang.DamageType"  select="'Damage Type'"/>
  <xsl:variable name="lang.DataProc"      select="'Data Proc.'"/>
  <xsl:variable name="lang.DataProcessing"  select="'Data Processing'"/>
  <xsl:variable name="lang.DecreaseAttribute"    select="'Decrease Attribute'"/>
  <xsl:variable name="lang.DerivedAttributes"  select="'Derived Attributes'"/>
  <xsl:variable name="lang.DeviceRating"    select="'Rating'"/>
  <xsl:variable name="lang.FadingValue"    select="'Fading Value'"/>
  <xsl:variable name="lang.HobbiesVice"    select="'Hobbies/Vice'"/>
  <xsl:variable name="lang.IDcredsticks"    select="'ID/Credsticks'"/>
  <xsl:variable name="lang.InitiateGrade"    select="'Initiate Grade'"/>
  <xsl:variable name="lang.InitiationNotes"  select="'Initiation Grade Notes'"/>
  <xsl:variable name="lang.JudgeIntentions"  select="'Judge Intentions'"/>
  <xsl:variable name="lang.KnowledgeSkills"  select="'Knowledge Skills'"/>
  <xsl:variable name="lang.LiftCarry"      select="'Lift/Carry'"/>
  <xsl:variable name="lang.LineofSight"    select="'Line of Sight'"/>
  <xsl:variable name="lang.LinkedSIN"      select="'Linked SIN'"/>
  <xsl:variable name="lang.MartialArt"    select="'Martial Art'"/>
  <xsl:variable name="lang.MartialArts"    select="'Martial Arts'"/>
  <xsl:variable name="lang.MatrixAR"      select="'Matrix AR'"/>
  <xsl:variable name="lang.MatrixCold"    select="'Matrix Cold'"/>
  <xsl:variable name="lang.MatrixDevices"    select="'Matrix Devices'"/>
  <xsl:variable name="lang.MatrixHot"      select="'Matrix Hot'"/>
  <xsl:variable name="lang.MatrixTrack"    select="'Matrix Damage Track'"/>
  <xsl:variable name="lang.MeleeWeapons"    select="'Melee Weapons'"/>
  <xsl:variable name="lang.MentalAttributes"  select="'Mental Attributes'"/>
  <xsl:variable name="lang.MysticAdept"    select="'Mystic Adept'"/>
  <xsl:variable name="lang.NotAddictedYet"  select="'Not Addicted Yet'"/>
  <xsl:variable name="lang.Nothing2Show4Devices"    select="'No Devices to list'"/>
  <xsl:variable name="lang.Nothing2Show4Notes"    select="'No Notes to list'"/>
  <xsl:variable name="lang.Nothing2Show4SpiritsSprites"    select="'No Spirits/Sprites to list'"/>
  <xsl:variable name="lang.Nothing2Show4Vehicles"    select="'No Vehicles to list'"/>
  <xsl:variable name="lang.OptionalPowers"    select="'Optional Powers'"/>
  <xsl:variable name="lang.OtherArmor"      select="'Other Armor'"/>
  <xsl:variable name="lang.OtherMugshots"    select="'Other Portraits'"/>
  <xsl:variable name="lang.PageBreak"      select="'Page Break: '"/>
  <xsl:variable name="lang.PersonalData"    select="'Personal Data'"/>
  <xsl:variable name="lang.PersonalLife"    select="'Personal Life'"/>
  <xsl:variable name="lang.PhysicalAttributes"  select="'Physical Attributes'"/>
  <xsl:variable name="lang.PhysicalNaturalRecovery"  select="'Natural Recovery Pool (1 day)'"/>
  <xsl:variable name="lang.PhysicalTrack"  select="'Physical Damage Track'"/>
  <xsl:variable name="lang.PreferredPayment"    select="'Preferred Payment Method'"/>
  <xsl:variable name="lang.PrimaryArm"    select="'Primary Arm'"/>
  <xsl:variable name="lang.PublicAwareness"  select="'Public Awareness'"/>
  <xsl:variable name="lang.RangedWeapons"    select="'Ranged Weapons'"/>
  <xsl:variable name="lang.RemainingAvailable"  select="'Remaining Available'"/>
  <xsl:variable name="lang.ResistDrain"    select="'Resist Drain with'"/>
  <xsl:variable name="lang.ResistFading"    select="'Resist Fading with'"/>
  <xsl:variable name="lang.RiggerInitiative"  select="'Rigger Initiative'"/>
  <xsl:variable name="lang.SelectedGear"    select="'Selected Gear'"/>
  <xsl:variable name="lang.SkillGroup"    select="'Skill Group'"/>
  <xsl:variable name="lang.SkillGroups"   select="'Skill Groups'"/>
  <xsl:variable name="lang.SpecialAttributes"  select="'Special Attributes'"/>
  <xsl:variable name="lang.StreetCred"    select="'Street Cred'"/>
  <xsl:variable name="lang.StreetName"    select="'Street Name'"/>
  <xsl:variable name="lang.StunNaturalRecovery"  select="'Natural Recovery Pool (1 hour)'"/>
  <xsl:variable name="lang.StunTrack"    select="'Stun Damage Track'"/>
  <xsl:variable name="lang.SubmersionGrade"  select="'Submersion Grade'"/>
  <xsl:variable name="lang.SubmersionNotes"  select="'Submersion Notes'"/>
  <xsl:variable name="lang.ToggleColors"  select="'Toggle Colors'"/>
  <xsl:variable name="lang.TotalArmor"  select="'Total of equipped single highest armor and accessories'"/>
  <xsl:variable name="lang.ToxinsAndPathogens"  select="'Toxins and Pathogens'"/>
  <xsl:variable name="lang.UnnamedCharacter"  select="'Unnamed Character'"/>
  <xsl:variable name="lang.VehicleBody"    select="'Body'"/>
  <xsl:variable name="lang.VehicleCost"    select="'Vehicle Cost'"/>
  <xsl:variable name="lang.WildReputation"    select="'Wild Reputation'"/>

  <!-- "limits" list -->
  <xsl:variable name="lang.AstralLimit"    select="'Astral Limit'"/>
  <xsl:variable name="lang.MentalLimit"    select="'Mental Limit'"/>
  <xsl:variable name="lang.PhysicalLimit"    select="'Physical Limit'"/>
  <xsl:variable name="lang.SocialLimit"    select="'Social Limit'"/>

  <!-- spell types list -->
  <xsl:variable name="lang.CombatSpells"    select="'Combat Spells'"/>
  <xsl:variable name="lang.DetectionSpells"  select="'Detection Spells'"/>
  <xsl:variable name="lang.Enchantments"    select="'Enchantments'"/>
  <xsl:variable name="lang.HealthSpells"     select="'Health Spells'"/>
  <xsl:variable name="lang.IllusionSpells"   select="'Illusion Spells'"/>
  <xsl:variable name="lang.ManipulationSpells" select="'Manipulation Spells'"/>
  <xsl:variable name="lang.Rituals"      select="'Rituals'"/>

  <!-- test values -->
  <xsl:variable name="lang.tstDamage1"  select="'P'"/>
  <xsl:variable name="lang.tstDamage2"  select="'S'"/>
  <xsl:variable name="lang.tstDuration1"  select="'I'"/>
  <xsl:variable name="lang.tstDuration2"  select="'P'"/>
  <xsl:variable name="lang.tstDuration3"  select="'S'"/>
  <xsl:variable name="lang.tstRange1"    select="'T'"/>
  <xsl:variable name="lang.tstRange2"    select="'LOS'"/>
  <xsl:variable name="lang.tstRange3"    select="'LOS(A)'"/>
  <xsl:variable name="lang.tstRange4"    select="'LOS (A)'"/>
  <xsl:variable name="lang.tstRange5"    select="'S'"/>
  <xsl:variable name="lang.tstRange6"    select="'S(A)'"/>
  <xsl:variable name="lang.tstRange7"    select="'S (A)'"/>
  <xsl:variable name="lang.tstRange8"    select="'T(A)'"/>
  <xsl:variable name="lang.tstRange9"    select="'T (A)'"/>
  <xsl:variable name="lang.tstRange10"    select="'Special'"/>

  <!-- miscellaneous signs and symbols -->
    <!-- currency symbol -->
  <xsl:variable name="lang.NuyenSymbol"  select="'&#165;'"/>
    <!-- diacrtic marks: decimal mark and grouping separator -->
  <xsl:variable name="lang.marks"      select="'.,'"/>
</xsl:stylesheet>
