<?xml version="1.0" encoding="utf-8" ?>
<!-- Isolate Japanese (jp) locale literals -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="lang"  select="string('jp')" />
  <xsl:variable name="locale"  select="string('ja-jp')" />

  <!-- individual words -->
  <xsl:variable name="lang.Acceleration"  select="string('Acceleration')" />
  <xsl:variable name="lang.Accel"      select="string('Accel')" />
  <xsl:variable name="lang.Accessories"  select="string('Accessories')" />
  <xsl:variable name="lang.Accuracy"    select="string('Accuracy')" />
  <xsl:variable name="lang.Acid"      select="string('Acid')" />
  <xsl:variable name="lang.Action"      select="string('Action')" />
  <xsl:variable name="lang.Addiction"  select="string('Addiction')" />
  <xsl:variable name="lang.Adept"      select="string('Adept')" />
  <xsl:variable name="lang.Age"      select="string('Age')" />
  <xsl:variable name="lang.AGI"      select="string('AGI')" />
  <xsl:variable name="lang.Agility"    select="string('Agility')" />
  <xsl:variable name="lang.AI"      select="string('AI')" />
  <xsl:variable name="lang.Alias"      select="string('Alias')" />
  <xsl:variable name="lang.Ammo"      select="string('Ammo')" />
  <xsl:variable name="lang.Amount"    select="string('Amount')" />
  <xsl:variable name="lang.AP"      select="string('AP')" />
  <xsl:variable name="lang.Applicable"  select="string('Applicable')" />
  <xsl:variable name="lang.Apprentice"  select="string('Apprentice')" />
  <xsl:variable name="lang.AR"      select="string('AR')" />
  <xsl:variable name="lang.Archetype"    select="string('Archetype')" />
  <xsl:variable name="lang.Area"      select="string('Area')" />
  <xsl:variable name="lang.Armor"      select="string('Armor')" />
  <xsl:variable name="lang.Arts"      select="string('Arts')" />
  <xsl:variable name="lang.as"      select="string('as')" />
  <xsl:variable name="lang.ASDF"    select="string('A/S/D/F')" />
  <xsl:variable name="lang.Astral"    select="string('Astral')" />
  <xsl:variable name="lang.Attack"    select="string('Attack')" />
  <xsl:variable name="lang.ATT"      select="string('ATT')" />
  <xsl:variable name="lang.Attribute"    select="string('Attribute')" />
  <xsl:variable name="lang.Attributes"  select="string('Attributes')" />
  <xsl:variable name="lang.Available"    select="string('Available')" />
  <xsl:variable name="lang.Awakened"    select="string('Awakened')" />
  <xsl:variable name="lang.Aware"    select="string('Aware')" />
  <xsl:variable name="lang.Background"  select="string('Background')" />
  <xsl:variable name="lang.Base"      select="string('Base')" />
  <xsl:variable name="lang.Bioware"    select="string('Bioware')" />
  <xsl:variable name="lang.BOD"      select="string('BOD')" />
  <xsl:variable name="lang.Body"      select="string('Body')" />
  <xsl:variable name="lang.Bonus"      select="string('Bonus')" />
  <xsl:variable name="lang.Bound"      select="string('Bound')" />
  <xsl:variable name="lang.Calendar"    select="string('Calendar')" />
  <xsl:variable name="lang.Career"    select="string('Career')" />
  <xsl:variable name="lang.Category"    select="string('Category')" />
  <xsl:variable name="lang.CHA"      select="string('CHA')" />
  <xsl:variable name="lang.Charisma"    select="string('Charisma')" />
  <xsl:variable name="lang.CM"      select="string('CM')" />
  <xsl:variable name="lang.Cold"      select="string('Cold')" />
  <xsl:variable name="lang.Combat"    select="string('Combat')" />
  <xsl:variable name="lang.Commlink"    select="string('Commlink')" />
  <xsl:variable name="lang.Composure"    select="string('Composure')" />
  <xsl:variable name="lang.Concept"    select="string('Concept')" />
  <xsl:variable name="lang.Connection"  select="string('Connection')" />
  <xsl:variable name="lang.Contact"    select="string('Contact')" />
  <xsl:variable name="lang.ContactDrug"    select="string('Contact')" />
  <xsl:variable name="lang.Contacts"    select="string('Contacts')" />
  <xsl:variable name="lang.Cost"      select="string('Cost')" />
  <xsl:variable name="lang.Critter"    select="string('Critter')" />
  <xsl:variable name="lang.Critters"    select="string('Critters')" />
  <xsl:variable name="lang.Cyberware"    select="string('Cyberware')" />
  <xsl:variable name="lang.Damage"    select="string('Damage')" />
  <xsl:variable name="lang.Data"      select="string('Data')" />
  <xsl:variable name="lang.Date"      select="string('Date')" />
  <xsl:variable name="lang.Day"      select="string('天')" />
  <xsl:variable name="lang.Days"      select="string('天')" />
  <xsl:variable name="lang.Dead"      select="string('Dead')" />
  <xsl:variable name="lang.Defense"      select="string('Defense')" />
  <xsl:variable name="lang.DEP"  select="string('DEP')" />
  <xsl:variable name="lang.Depth"  select="string('Depth')" />
  <xsl:variable name="lang.Description"  select="string('Description')" />
  <xsl:variable name="lang.Detection"    select="string('Detection')" />
  <xsl:variable name="lang.Device"    select="string('Device')" />
  <xsl:variable name="lang.Devices"    select="string('Devices')" />
  <xsl:variable name="lang.Direct"      select="string('Direct')" />
  <xsl:variable name="lang.Down"      select="string('Down')" />
  <xsl:variable name="lang.DP"        select="string('DP')" />
  <xsl:variable name="lang.Drain"      select="string('Drain')" />
  <xsl:variable name="lang.Drone"      select="string('Drone')" />
  <xsl:variable name="lang.Duration"    select="string('Duration')" />
  <xsl:variable name="lang.DV"      select="string('DV')" />
  <xsl:variable name="lang.E"        select="string('E')" />
  <xsl:variable name="lang.Echo"    select="string('Echo')" />
  <xsl:variable name="lang.Echoes"    select="string('Echoes')" />
  <xsl:variable name="lang.EDG"      select="string('EDG')" />
  <xsl:variable name="lang.Edge"      select="string('Edge')" />
  <xsl:variable name="lang.Electricity"    select="string('Electricity')" />
  <xsl:variable name="lang.Enchanter"    select="string('Enchanter')" />
  <xsl:variable name="lang.Enemies"    select="string('Enemies')" />
  <xsl:variable name="lang.Enhancements"  select="string('Enhancements')" />
  <xsl:variable name="lang.Entries"    select="string('Entries')" />
  <xsl:variable name="lang.Equipped"    select="string('Equipped')" />
  <xsl:variable name="lang.ESS"      select="string('ESS')" />
  <xsl:variable name="lang.Essence"    select="string('Essence')" />
  <xsl:variable name="lang.Expenses"    select="string('Expenses')" />
  <xsl:variable name="lang.Explorer"    select="string('Explorer')" />
  <xsl:variable name="lang.Eyes"      select="string('Eyes')" />
  <xsl:variable name="lang.Falling"    select="string('Falling')" />
  <xsl:variable name="lang.Fatigue"      select="string('Fatigue')" />
  <xsl:variable name="lang.Fettered"      select="string('Fettered')" />
  <xsl:variable name="lang.Fire"    select="string('Fire')" />
  <xsl:variable name="lang.Firewall"    select="string('Firewall')" />
  <xsl:variable name="lang.Fly"      select="string('Fly')" />
  <xsl:variable name="lang.Foci"      select="string('Foci')" />
  <xsl:variable name="lang.FWL"      select="string('FWL')" />
  <xsl:variable name="lang.Force"      select="string('Force')" />
  <xsl:variable name="lang.FV"      select="string('FV')" />
  <xsl:variable name="lang.Gear"      select="string('Gear')" />
  <xsl:variable name="lang.Gender"      select="string('Gender')" />
  <xsl:variable name="lang.Grade"      select="string('Grade')" />
  <xsl:variable name="lang.Hair"      select="string('Hair')" />
  <xsl:variable name="lang.Handling"    select="string('Handling')" />
  <xsl:variable name="lang.Health"    select="string('Health')" />
  <xsl:variable name="lang.Heavy"    select="string('Heavy')" />
  <xsl:variable name="lang.Height"    select="string('Height')" />
  <xsl:variable name="lang.hit"      select="string('hit')" />
  <xsl:variable name="lang.Illusion"    select="string('Illusion')" />
  <xsl:variable name="lang.Implant"    select="string('Implant')" />
  <xsl:variable name="lang.Indirect"      select="string('Indirect')" />
  <xsl:variable name="lang.Info"      select="string('Info')" />
  <xsl:variable name="lang.Ingestion"      select="string('Ingestion')" />
  <xsl:variable name="lang.Inhalation"      select="string('Inhalation')" />
  <xsl:variable name="lang.Init"      select="string('Init')" />
  <xsl:variable name="lang.Initiation"  select="string('Initiation')" />
  <xsl:variable name="lang.Initiative"  select="string('Initiative')" />
  <xsl:variable name="lang.Injection"      select="string('Injection')" />
  <xsl:variable name="lang.INT"      select="string('INT')" />
  <xsl:variable name="lang.Intentions"  select="string('Intentions')" />
  <xsl:variable name="lang.Intuition"    select="string('Intuition')" />
  <xsl:variable name="lang.Instantaneous"  select="string('Instantaneous')" />
  <xsl:variable name="lang.Karma"      select="string('Karma')" />
  <xsl:variable name="lang.L"        select="string('L')" />
  <xsl:variable name="lang.Level"      select="string('Level')" />
  <xsl:variable name="lang.Lifestyle"    select="string('Lifestyle')" />
  <xsl:variable name="lang.Limit"      select="string('Limit')" />
  <xsl:variable name="lang.Limits"    select="string('Limits')" />
  <xsl:variable name="lang.Loaded"    select="string('Loaded')" />
  <xsl:variable name="lang.Location"    select="string('Location')" />
  <xsl:variable name="lang.LOG"      select="string('LOG')" />
  <xsl:variable name="lang.Logic"      select="string('Logic')" />
  <xsl:variable name="lang.Loyalty"    select="string('Loyalty')" />
  <xsl:variable name="lang.M"        select="string('M')" />
  <xsl:variable name="lang.MAG"      select="string('MAG')" />
  <xsl:variable name="lang.Magic"      select="string('Magic')" />
  <xsl:variable name="lang.Magician"      select="string('Magician')" />
  <xsl:variable name="lang.Mana"    select="string('Mana')" />
  <xsl:variable name="lang.Maneuvers"    select="string('Maneuvers')" />
  <xsl:variable name="lang.Manipulation"  select="string('Manipulation')" />
  <xsl:variable name="lang.Manual"    select="string('Manual')" />
  <xsl:variable name="lang.Memory"    select="string('Memory')" />
  <xsl:variable name="lang.Mental"    select="string('Mental')" />
  <xsl:variable name="lang.Metamagics"  select="string('Metamagics')" />
  <xsl:variable name="lang.Metatype"    select="string('Metatype')" />
  <xsl:variable name="lang.Mod"      select="string('Mod')" />
  <xsl:variable name="lang.Mode"      select="string('Mode')" />
  <xsl:variable name="lang.Model"      select="string('Model')" />
  <xsl:variable name="lang.Modifications"  select="string('Modifications')" />
  <xsl:variable name="lang.Month"      select="string('月')" />
  <xsl:variable name="lang.Months"    select="string('月')" />
  <xsl:variable name="lang.Mount"    select="string('Mount')" />
  <xsl:variable name="lang.Movement"    select="string('Movement')" />
  <xsl:variable name="lang.Mugshot"    select="string('Portrait')" />
  <xsl:variable name="lang.Name"      select="string('Name')" />
  <xsl:variable name="lang.Native"    select="string('Native')" />
  <xsl:variable name="lang.Negative"    select="string('Negative')" />
  <xsl:variable name="lang.No"      select="string('No')" />
  <xsl:variable name="lang.None"      select="string('None')" />
  <xsl:variable name="lang.Notes"      select="string('Notes')" />
  <xsl:variable name="lang.Notoriety"    select="string('Notoriety')" />
  <xsl:variable name="lang.Nuyen"      select="string('Nuyen')" />
  <xsl:variable name="lang.Other"      select="string('Other')" />
  <xsl:variable name="lang.Overflow"      select="string('Overflow')" />
  <xsl:variable name="lang.OVR"      select="string('OVR&#160;')" />
  <xsl:variable name="lang.Pathogen"    select="string('Pathogen')" />
  <xsl:variable name="lang.Permanent"    select="string('Permanent')" />
  <xsl:variable name="lang.Persona"    select="string('Persona')" />
  <xsl:variable name="lang.Pets"      select="string('Pets')" />
  <xsl:variable name="lang.Physical"    select="string('Physical')" />
  <xsl:variable name="lang.Physiological"  select="string('Physiological')" />
  <xsl:variable name="lang.Pilot"      select="string('Pilot')" />
  <xsl:variable name="lang.Player"    select="string('Player')" />
  <xsl:variable name="lang.Points"    select="string('Points')" />
  <xsl:variable name="lang.Pool"      select="string('Pool')" />
  <xsl:variable name="lang.Positive"    select="string('Positive')" />
  <xsl:variable name="lang.Power"      select="string('Power')" />
  <xsl:variable name="lang.Powers"    select="string('Powers')" />
  <xsl:variable name="lang.Priorities"  select="string('Priorities')" />
  <xsl:variable name="lang.Processor"    select="string('Processor')" />
  <xsl:variable name="lang.Program"    select="string('Program')" />
  <xsl:variable name="lang.Programs"    select="string('Programs')" />
  <xsl:variable name="lang.Psychological"  select="string('Psychological')" />
  <xsl:variable name="lang.Qty"      select="string('Qty')" />
  <xsl:variable name="lang.Quality"    select="string('Quality')" />
  <xsl:variable name="lang.Qualities"    select="string('Qualities')" />
  <xsl:variable name="lang.Radiation"      select="string('Radiation')" />
  <xsl:variable name="lang.Range"      select="string('Range')" />
  <xsl:variable name="lang.Rating"    select="string('Rating')" />
  <xsl:variable name="lang.RC"      select="string('RC')" />
  <xsl:variable name="lang.Reaction"    select="string('Reaction')" />
  <xsl:variable name="lang.REA"      select="string('REA')" />
  <xsl:variable name="lang.Reach"      select="string('Reach')" />
  <xsl:variable name="lang.Reason"    select="string('Reason')" />
  <xsl:variable name="lang.Registered"  select="string('Registered')" />
  <xsl:variable name="lang.Requires"    select="string('Requires')" />
  <xsl:variable name="lang.RES"      select="string('RES')" />
  <xsl:variable name="lang.Resistance"    select="string('Resistance')" />
  <xsl:variable name="lang.Resistances"    select="string('Resistances')" />
  <xsl:variable name="lang.Resonance"    select="string('Resonance')" />
  <xsl:variable name="lang.Resources"    select="string('Resources')" />
  <xsl:variable name="lang.Rigger"    select="string('Rigger')" />
  <xsl:variable name="lang.Rtg"      select="string('Rtg')" />
  <xsl:variable name="lang.Run"      select="string('Run')" />
  <xsl:variable name="lang.S"        select="string('S')" />
  <xsl:variable name="lang.Seats"      select="string('Seats')" />
  <xsl:variable name="lang.Self"      select="string('Self')" />
  <xsl:variable name="lang.Services"    select="string('Services')" />
  <xsl:variable name="lang.Sensor"    select="string('Sensor')" />
  <xsl:variable name="lang.Show"      select="string('Show: ')" />
  <xsl:variable name="lang.Skill"      select="string('Skill')" />
  <xsl:variable name="lang.Skills"    select="string('Skills')" />
  <xsl:variable name="lang.Skin"      select="string('Skin')" />
  <xsl:variable name="lang.Sleaze"    select="string('Sleaze')" />
  <xsl:variable name="lang.SLZ"      select="string('SLZ')" />
  <xsl:variable name="lang.Social"    select="string('Social')" />
  <xsl:variable name="lang.Sonic"      select="string('Sonic')" />
  <xsl:variable name="lang.Source"    select="string('Source')" />
  <xsl:variable name="lang.Special"    select="string('Special')" />
  <xsl:variable name="lang.Speed"      select="string('Speed')" />
  <xsl:variable name="lang.Spell"      select="string('Spell')" />
  <xsl:variable name="lang.Spells"    select="string('Spells')" />
  <xsl:variable name="lang.Spirit"    select="string('Spirit')" />
  <xsl:variable name="lang.Spirits"    select="string('Spirits')" />
  <xsl:variable name="lang.Sprite"    select="string('Sprite')" />
  <xsl:variable name="lang.Sprites"    select="string('Sprites')" />
  <xsl:variable name="lang.Standard"    select="string('Standard')" />
  <xsl:variable name="lang.Stream"    select="string('Stream')" />
  <xsl:variable name="lang.STR"      select="string('STR')" />
  <xsl:variable name="lang.Strength"    select="string('Strength')" />
  <xsl:variable name="lang.Stun"      select="string('Stun')" />
  <xsl:variable name="lang.Submersion"  select="string('Submersion')" />
  <xsl:variable name="lang.Sustained"    select="string('Sustained')" />
  <xsl:variable name="lang.Swim"      select="string('Swim')" />
  <xsl:variable name="lang.Target"    select="string('Target')" />
  <xsl:variable name="lang.Tasks"    select="string('Tasks')" />
  <xsl:variable name="lang.Total"      select="string('Total')" />
  <xsl:variable name="lang.Touch"      select="string('Touch')" />
  <xsl:variable name="lang.Toxin"      select="string('Toxin')" />
  <xsl:variable name="lang.Tradition"    select="string('Tradition')" />
  <xsl:variable name="lang.Type"      select="string('Type')" />
  <xsl:variable name="lang.Unbound"    select="string('Unbound')" />
  <xsl:variable name="lang.Unknown"    select="string('Unknown')" />
  <xsl:variable name="lang.Unregistered"  select="string('Unregistered')" />
  <xsl:variable name="lang.Under"      select="string('Under')" />
  <xsl:variable name="lang.Vehicle"    select="string('Vehicle')" />
  <xsl:variable name="lang.Vehicles"    select="string('Vehicles')" />
  <xsl:variable name="lang.VR"      select="string('VR')" />
  <xsl:variable name="lang.W"        select="string('W')" />
  <xsl:variable name="lang.Walk"      select="string('Walk')" />
  <xsl:variable name="lang.Weaknesses"    select="string('Weaknesses')" />
  <xsl:variable name="lang.Weapon"    select="string('Weapon')" />
  <xsl:variable name="lang.Weapons"    select="string('Weapons')" />
  <xsl:variable name="lang.Week"      select="string('周')" />
  <xsl:variable name="lang.Weeks"      select="string('周')" />
  <xsl:variable name="lang.Weight"    select="string('Weight')" />
  <xsl:variable name="lang.WIL"      select="string('WIL')" />
  <xsl:variable name="lang.Willpower"    select="string('Willpower')" />
  <xsl:variable name="lang.with"      select="string('with')" />
  <xsl:variable name="lang.Yes"      select="string('Yes')" />

  <!-- multiple word phrases / composite words -->
  <xsl:variable name="lang.ActiveSkills"    select="string('Active Skills')" />
  <xsl:variable name="lang.AdeptPowers"    select="string('Adept Powers')" />
  <xsl:variable name="lang.AIandAdvanced"    select="string('AI Programs and Advanced Programs')" />
  <xsl:variable name="lang.AlreadyAddicted"  select="string('Already Addicted')" />
  <xsl:variable name="lang.ArmorValue"    select="string('Value')" />
  <xsl:variable name="lang.AspectedMagician"    select="string('Aspected Magician')" />
  <xsl:variable name="lang.AstralInitiative"  select="string('Astral Initiative')" />
  <xsl:variable name="lang.AstralReputation"    select="string('Astral Reputation')" />
  <xsl:variable name="lang.CombatSkill"    select="string('Combat Skill')" />
  <xsl:variable name="lang.ComplexForm"    select="string('Complex Form')" />
  <xsl:variable name="lang.ComplexForms"    select="string('Complex Forms')" />
  <xsl:variable name="lang.ConditionMonitor"  select="string('Condition Monitor')" />
  <xsl:variable name="lang.ContactList"    select="string('Contact List')" />
  <xsl:variable name="lang.CoreTrack"  select="string('Core Damage Track')" />
  <xsl:variable name="lang.CritterPower"    select="string('Critter Power')" />
  <xsl:variable name="lang.CritterPowers"    select="string('Critter Powers')" />
  <xsl:variable name="lang.CurrentEdge"    select="string('Current Edge Points')" />
  <xsl:variable name="lang.CurrentForm"    select="string('Current Form')" />
  <xsl:variable name="lang.DamageType"  select="string('Damage Type')" />
  <xsl:variable name="lang.DataProc"      select="string('Data Proc.')" />
  <xsl:variable name="lang.DataProcessing"  select="string('Data Processing')" />
  <xsl:variable name="lang.DecreaseAttribute"    select="string('Decrease Attribute')" />
  <xsl:variable name="lang.DerivedAttributes"  select="string('Derived Attributes')" />
  <xsl:variable name="lang.DeviceRating"    select="string('Rating')" />
  <xsl:variable name="lang.FadingValue"    select="string('Fading Value')" />
  <xsl:variable name="lang.HobbiesVice"    select="string('Hobbies/Vice')" />
  <xsl:variable name="lang.IDcredsticks"    select="string('ID/クレッドスティック')" />
  <xsl:variable name="lang.InitiateGrade"    select="string('Initiate Grade')" />
  <xsl:variable name="lang.InitiationNotes"  select="string('Initiation Grade Notes')" />
  <xsl:variable name="lang.JudgeIntentions"  select="string('Judge Intentions')" />
  <xsl:variable name="lang.KnowledgeSkills"  select="string('Knowledge Skills')" />
  <xsl:variable name="lang.LiftCarry"      select="string('Lift/Carry')" />
  <xsl:variable name="lang.LineofSight"    select="string('Line of Sight')" />
  <xsl:variable name="lang.LinkedSIN"      select="string('Linked SIN')" />
  <xsl:variable name="lang.MartialArt"    select="string('Martial Art')" />
  <xsl:variable name="lang.MartialArts"    select="string('Martial Arts')" />
  <xsl:variable name="lang.MatrixAR"      select="string('Matrix AR')" />
  <xsl:variable name="lang.MatrixCold"    select="string('Matrix Cold')" />
  <xsl:variable name="lang.MatrixDevices"    select="string('Matrix Devices')" />
  <xsl:variable name="lang.MatrixHot"      select="string('Matrix Hot')" />
  <xsl:variable name="lang.MatrixTrack"    select="string('Matrix Damage Track')" />
  <xsl:variable name="lang.MeleeWeapons"    select="string('Melee Weapons')" />
  <xsl:variable name="lang.MentalAttributes"  select="string('Mental Attributes')" />
  <xsl:variable name="lang.MysticAdept"    select="string('Mystic Adept')" />
  <xsl:variable name="lang.NotAddictedYet"  select="string('Not Addicted Yet')" />
  <xsl:variable name="lang.Nothing2Show4Devices"    select="string('No Devices to list')" />
  <xsl:variable name="lang.Nothing2Show4Notes"    select="string('No Notes to list')" />
  <xsl:variable name="lang.Nothing2Show4SpiritsSprites"    select="string('No Spirits/Sprites to list')" />
  <xsl:variable name="lang.Nothing2Show4Vehicles"    select="string('No Vehicles to list')" />
  <xsl:variable name="lang.OptionalPowers"    select="string('Optional Powers')" />
  <xsl:variable name="lang.OtherArmor"      select="string('Other Armor')" />
  <xsl:variable name="lang.OtherMugshots"    select="string('Other Portraits')" />
  <xsl:variable name="lang.PageBreak"      select="string('Page Break: ')" />
  <xsl:variable name="lang.PersonalData"    select="string('Personal Data')" />
  <xsl:variable name="lang.PersonalLife"    select="string('Personal Life')" />
  <xsl:variable name="lang.PhysicalAttributes"  select="string('Physical Attributes')" />
  <xsl:variable name="lang.PhysicalNaturalRecovery"  select="string('Natural Recovery Pool (1 day)')" />
  <xsl:variable name="lang.PhysicalTrack"  select="string('Physical Damage Track')" />
  <xsl:variable name="lang.PreferredPayment"    select="string('Preferred Payment Method')" />
  <xsl:variable name="lang.PrimaryArm"    select="string('Primary Arm')" />
  <xsl:variable name="lang.PublicAwareness"  select="string('Public Awareness')" />
  <xsl:variable name="lang.RangedWeapons"    select="string('Ranged Weapons')" />
  <xsl:variable name="lang.RemainingAvailable"  select="string('Remaining Available')" />
  <xsl:variable name="lang.ResistDrain"    select="string('Resist Drain with')" />
  <xsl:variable name="lang.ResistFading"    select="string('Resist Fading with')" />
  <xsl:variable name="lang.RiggerInitiative"  select="string('Rigger Initiative')" />
  <xsl:variable name="lang.SelectedGear"    select="string('Selected Gear')" />
  <xsl:variable name="lang.SkillGroup"    select="string('Skill Group')" />
  <xsl:variable name="lang.SkillGroups"   select="string('Skill Groups')" />
  <xsl:variable name="lang.SpecialAttributes"  select="string('Special Attributes')" />
  <xsl:variable name="lang.StreetCred"    select="string('Street Cred')" />
  <xsl:variable name="lang.StreetName"    select="string('Street Name')" />
  <xsl:variable name="lang.StunNaturalRecovery"  select="string('Natural Recovery Pool (1 hour)')" />
  <xsl:variable name="lang.StunTrack"    select="string('Stun Damage Track')" />
  <xsl:variable name="lang.SubmersionGrade"  select="string('Submersion Grade')" />
  <xsl:variable name="lang.SubmersionNotes"  select="string('Submersion Notes')" />
  <xsl:variable name="lang.ToggleColors"  select="string('Toggle Colors')" />
  <xsl:variable name="lang.TotalArmor"  select="string('Total of equipped single highest armor and accessories')" />
  <xsl:variable name="lang.ToxinsAndPathogens"  select="string('Toxins and Pathogens')" />
  <xsl:variable name="lang.UnnamedCharacter"  select="string('Unnamed Character')" />
  <xsl:variable name="lang.VehicleBody"    select="string('Body')" />
  <xsl:variable name="lang.VehicleCost"    select="string('Vehicle Cost')" />
  <xsl:variable name="lang.WildReputation"    select="string('Wild Reputation')" />

  <!-- "limits" list -->
  <xsl:variable name="lang.AstralLimit"    select="string('Astral Limit')" />
  <xsl:variable name="lang.MentalLimit"    select="string('Mental Limit')" />
  <xsl:variable name="lang.PhysicalLimit"    select="string('Physical Limit')" />
  <xsl:variable name="lang.SocialLimit"    select="string('Social Limit')" />

  <!-- spell types list -->
  <xsl:variable name="lang.CombatSpells"    select="string('Combat Spells')" />
  <xsl:variable name="lang.DetectionSpells"  select="string('Detection Spells')" />
  <xsl:variable name="lang.Enchantments"    select="string('Enchantments')" />
  <xsl:variable name="lang.HealthSpells"     select="string('Health Spells')" />
  <xsl:variable name="lang.IllusionSpells"   select="string('Illusion Spells')" />
  <xsl:variable name="lang.ManipulationSpells" select="string('Manipulation Spells')" />
  <xsl:variable name="lang.Rituals"      select="string('Rituals')" />

  <!-- test values -->
  <xsl:variable name="lang.tstDamage1"  select="string('P')" />
  <xsl:variable name="lang.tstDamage2"  select="string('S')" />
  <xsl:variable name="lang.tstDuration1"  select="string('I')" />
  <xsl:variable name="lang.tstDuration2"  select="string('P')" />
  <xsl:variable name="lang.tstDuration3"  select="string('S')" />
  <xsl:variable name="lang.tstRange1"    select="string('T')" />
  <xsl:variable name="lang.tstRange2"    select="string('LOS')" />
  <xsl:variable name="lang.tstRange3"    select="string('LOS(A)')" />
  <xsl:variable name="lang.tstRange4"    select="string('LOS (A)')" />
  <xsl:variable name="lang.tstRange5"    select="string('S')" />
  <xsl:variable name="lang.tstRange6"    select="string('S(A)')" />
  <xsl:variable name="lang.tstRange7"    select="string('S (A)')" />
  <xsl:variable name="lang.tstRange8"    select="string('T(A)')" />
  <xsl:variable name="lang.tstRange9"    select="string('T (A)')" />
  <xsl:variable name="lang.tstRange10"    select="string('Special')" />

  <!-- miscellaneous signs and symbols -->
    <!-- currency symbol -->
  <xsl:variable name="lang.NuyenSymbol"  select="string('&#165;')" />
    <!-- diacrtic marks: decimal mark and grouping separator -->
  <xsl:variable name="lang.marks"      select="string('.,')" />
</xsl:stylesheet>
