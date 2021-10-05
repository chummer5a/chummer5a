<?xml version="1.0" encoding="utf-8" ?>
<!-- Isolate German (de) locale literals -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="lang"  select="string('de')" />
  <xsl:variable name="locale"  select="string('de-de')" />

  <!-- individual words -->
  <xsl:variable name="lang.Acceleration"  select="string('Beschleunigung')" />
  <xsl:variable name="lang.Accel"      select="string('Beschl.')" />
  <xsl:variable name="lang.Accessories"  select="string('Zubehör')" />
  <xsl:variable name="lang.Accuracy"    select="string('Präzision')" />
  <xsl:variable name="lang.Acid"      select="string('Säure')" />
  <xsl:variable name="lang.Action"      select="string('Aktion')" />
  <xsl:variable name="lang.Addiction"  select="string('Abhängigkeit')" />
  <xsl:variable name="lang.Adept"      select="string('Adept')" />
  <xsl:variable name="lang.Age"      select="string('Alter')" />
  <xsl:variable name="lang.AGI"      select="string('GES')" />
  <xsl:variable name="lang.Agility"    select="string('Geschicklichkeit')" />
  <xsl:variable name="lang.AI"      select="string('KI')" />
  <xsl:variable name="lang.Alias"      select="string('Alias')" />
  <xsl:variable name="lang.Ammo"      select="string('Munition')" />
  <xsl:variable name="lang.Amount"    select="string('Menge')" />
  <xsl:variable name="lang.AP"      select="string('DK')" />
  <xsl:variable name="lang.Applicable"  select="string('Anwendbare')" />
  <xsl:variable name="lang.Apprentice"  select="string('Lehrling')" />
  <xsl:variable name="lang.AR"      select="string('AR')" />
  <xsl:variable name="lang.Archetype"    select="string('Archetyp')" />
  <xsl:variable name="lang.Area"      select="string('Fläche')" />
  <xsl:variable name="lang.Armor"      select="string('Panzerung')" />
  <xsl:variable name="lang.Arts"      select="string('Künste')" />
  <xsl:variable name="lang.as"      select="string('als')" />
  <xsl:variable name="lang.ASDF"    select="string('A/S/D/F')" />
  <xsl:variable name="lang.Astral"    select="string('Astral')" />
  <xsl:variable name="lang.Attack"    select="string('Angriff')" />
  <xsl:variable name="lang.ATT"      select="string('ANG')" />
  <xsl:variable name="lang.Attribute"    select="string('Attribut')" />
  <xsl:variable name="lang.Attributes"  select="string('Attribute')" />
  <xsl:variable name="lang.Available"    select="string('Verfügbar')" />
  <xsl:variable name="lang.Awakened"    select="string('Erwacht')" />
  <xsl:variable name="lang.Aware"    select="string('Bewusst')" />
  <xsl:variable name="lang.Background"  select="string('Hintergrund')" />
  <xsl:variable name="lang.Base"      select="string('Basis')" />
  <xsl:variable name="lang.Bioware"    select="string('Bioware')" />
  <xsl:variable name="lang.BOD"      select="string('KON')" />
  <xsl:variable name="lang.Body"      select="string('Konstitution')" />
  <xsl:variable name="lang.Bonus"      select="string('Bonus')" />
  <xsl:variable name="lang.Bound"      select="string('Gebunden')" />
  <xsl:variable name="lang.Calendar"    select="string('Kalender')" />
  <xsl:variable name="lang.Career"    select="string('Karriere')" />
  <xsl:variable name="lang.Category"    select="string('Kategorie')" />
  <xsl:variable name="lang.CHA"      select="string('CHA')" />
  <xsl:variable name="lang.Charisma"    select="string('Charisma')" />
  <xsl:variable name="lang.CM"      select="string('SM')" />
  <xsl:variable name="lang.Cold"      select="string('Kälte')" />
  <xsl:variable name="lang.Combat"    select="string('Kampf')" />
  <xsl:variable name="lang.Commlink"    select="string('Kommlink')" />
  <xsl:variable name="lang.Composure"    select="string('Selbstbeherrsch.')" />
  <xsl:variable name="lang.Concept"    select="string('Konzept')" />
  <xsl:variable name="lang.Connection"  select="string('Einfluss')" />
  <xsl:variable name="lang.Contact"    select="string('Kontakt')" />
  <xsl:variable name="lang.ContactDrug"    select="string('Kontakt')" />
  <xsl:variable name="lang.Contacts"    select="string('Kontakte')" />
  <xsl:variable name="lang.Cost"      select="string('Kosten')" />
  <xsl:variable name="lang.Critter"    select="string('Critter')" />
  <xsl:variable name="lang.Critters"    select="string('Critter')" />
  <xsl:variable name="lang.Cyberware"    select="string('Cyberware')" />
  <xsl:variable name="lang.Damage"    select="string('Schaden')" />
  <xsl:variable name="lang.Data"      select="string('Daten')" />
  <xsl:variable name="lang.Date"      select="string('Datum')" />
  <xsl:variable name="lang.Day"      select="string('Tag')" />
  <xsl:variable name="lang.Days"      select="string('Tage')" />
  <xsl:variable name="lang.Dead"      select="string('Tot')" />
  <xsl:variable name="lang.Defense"      select="string('Abwehr')" />
  <xsl:variable name="lang.DEP"  select="string('DEP')" />
  <xsl:variable name="lang.Depth"  select="string('Mächtigkeit')" />
  <xsl:variable name="lang.Description"  select="string('Beschreibung')" />
  <xsl:variable name="lang.Detection"    select="string('Wahrnehmung')" />
  <xsl:variable name="lang.Device"    select="string('Gerät')" />
  <xsl:variable name="lang.Devices"    select="string('Geräte')" />
  <xsl:variable name="lang.Direct"      select="string('Direkt')" />
  <xsl:variable name="lang.Down"      select="string('K.O.')" />
  <xsl:variable name="lang.DP"        select="string('DV')" />
  <xsl:variable name="lang.Drain"      select="string('Entzug')" />
  <xsl:variable name="lang.Drone"      select="string('Drohne')" />
  <xsl:variable name="lang.Duration"    select="string('Dauer')" />
  <xsl:variable name="lang.DV"      select="string('Schaden')" />
  <xsl:variable name="lang.E"        select="string('E')" />
  <xsl:variable name="lang.Echo"    select="string('Echo')" />
  <xsl:variable name="lang.Echoes"    select="string('Echos')" />
  <xsl:variable name="lang.EDG"      select="string('EDG')" />
  <xsl:variable name="lang.Edge"      select="string('Edge')" />
  <xsl:variable name="lang.Electricity"    select="string('Elektrizität')" />
  <xsl:variable name="lang.Enchanter"    select="string('Zauberer')" />
  <xsl:variable name="lang.Enemies"    select="string('Feinde')" />
  <xsl:variable name="lang.Enhancements"  select="string('Verbesserungen')" />
  <xsl:variable name="lang.Entries"    select="string('Einträge')" />
  <xsl:variable name="lang.Equipped"    select="string('Angelegt')" />
  <xsl:variable name="lang.ESS"      select="string('ESS')" />
  <xsl:variable name="lang.Essence"    select="string('Essenz')" />
  <xsl:variable name="lang.Expenses"    select="string('Ausgaben')" />
  <xsl:variable name="lang.Explorer"    select="string('Erforscher')" />
  <xsl:variable name="lang.Eyes"      select="string('Augenfarbe')" />
  <xsl:variable name="lang.Falling"    select="string('Fall')" />
  <xsl:variable name="lang.Fatigue"      select="string('Erschöpfung')" />
  <xsl:variable name="lang.Fettered"      select="string('Gefesselt')" />
  <xsl:variable name="lang.Fire"    select="string('Feuer')" />
  <xsl:variable name="lang.Firewall"    select="string('Firewall')" />
  <xsl:variable name="lang.Fly"      select="string('Fliegen')" />
  <xsl:variable name="lang.Foci"      select="string('Foki')" />
  <xsl:variable name="lang.FWL"      select="string('FWL')" />
  <xsl:variable name="lang.Force"      select="string('Kraft')" />
  <xsl:variable name="lang.FV"      select="string('Schwund')" />
  <xsl:variable name="lang.Gear"      select="string('Ausrüstung')" />
  <xsl:variable name="lang.Gender"      select="string('Geschlecht')" />
  <xsl:variable name="lang.Grade"      select="string('Kategorie')" />
  <xsl:variable name="lang.Hair"      select="string('Haarfarbe')" />
  <xsl:variable name="lang.Handling"    select="string('Handling')" />
  <xsl:variable name="lang.Health"    select="string('Heilung')" />
  <xsl:variable name="lang.Heavy"    select="string('Schwer')" />
  <xsl:variable name="lang.Height"    select="string('Größe')" />
  <xsl:variable name="lang.hit"      select="string('Erfolg')" />
  <xsl:variable name="lang.Illusion"    select="string('Illusion')" />
  <xsl:variable name="lang.Implant"    select="string('Implantat')" />
  <xsl:variable name="lang.Indirect"      select="string('Indirekt')" />
  <xsl:variable name="lang.Info"      select="string('Info')" />
  <xsl:variable name="lang.Ingestion"      select="string('Einnahme')" />
  <xsl:variable name="lang.Inhalation"      select="string('Inhalation')" />
  <xsl:variable name="lang.Init"      select="string('Init')" />
  <xsl:variable name="lang.Initiation"  select="string('Initiation')" />
  <xsl:variable name="lang.Initiative"  select="string('Initiative')" />
  <xsl:variable name="lang.Injection"      select="string('Injektion')" />
  <xsl:variable name="lang.INT"      select="string('INT')" />
  <xsl:variable name="lang.Intentions"  select="string('Intentionen')" />
  <xsl:variable name="lang.Intuition"    select="string('Intuition')" />
  <xsl:variable name="lang.Instantaneous"  select="string('Sofort')" />
  <xsl:variable name="lang.Karma"      select="string('Karma')" />
  <xsl:variable name="lang.L"        select="string('L')" />
  <xsl:variable name="lang.Level"      select="string('Stufe')" />
  <xsl:variable name="lang.Lifestyle"    select="string('Lebensstil')" />
  <xsl:variable name="lang.Limit"      select="string('Limit')" />
  <xsl:variable name="lang.Limits"    select="string('Limits')" />
  <xsl:variable name="lang.Loaded"    select="string('Geladen')" />
  <xsl:variable name="lang.Location"    select="string('Ort')" />
  <xsl:variable name="lang.LOG"      select="string('LOG')" />
  <xsl:variable name="lang.Logic"      select="string('Logik')" />
  <xsl:variable name="lang.Loyalty"    select="string('Loyalität')" />
  <xsl:variable name="lang.M"        select="string('M')" />
  <xsl:variable name="lang.MAG"      select="string('MAG')" />
  <xsl:variable name="lang.Magic"      select="string('Magie')" />
  <xsl:variable name="lang.Magician"      select="string('Magier')" />
  <xsl:variable name="lang.Mana"    select="string('Mana')" />
  <xsl:variable name="lang.Maneuvers"    select="string('Manöver')" />
  <xsl:variable name="lang.Manipulation"  select="string('Manipulation')" />
  <xsl:variable name="lang.Manual"    select="string('Manuell')" />
  <xsl:variable name="lang.Memory"    select="string('Erinnern')" />
  <xsl:variable name="lang.Mental"    select="string('Geistig')" />
  <xsl:variable name="lang.Metamagics"  select="string('Metamagie')" />
  <xsl:variable name="lang.Metatype"    select="string('Metatyp')" />
  <xsl:variable name="lang.Mod"      select="string('Mod')" />
  <xsl:variable name="lang.Mode"      select="string('Modus')" />
  <xsl:variable name="lang.Model"      select="string('Modell')" />
  <xsl:variable name="lang.Modifications"  select="string('Modifikationen')" />
  <xsl:variable name="lang.Month"      select="string('Monat')" />
  <xsl:variable name="lang.Months"    select="string('Monate')" />
  <xsl:variable name="lang.Mount"    select="string('Halterung')" />
  <xsl:variable name="lang.Movement"    select="string('Bewegung')" />
  <xsl:variable name="lang.Mugshot"    select="string('Portrait')" />
  <xsl:variable name="lang.Name"      select="string('Name')" />
  <xsl:variable name="lang.Native"    select="string('Muttersprache')" />
  <xsl:variable name="lang.Negative"    select="string('Negativ')" />
  <xsl:variable name="lang.No"      select="string('Nein')" />
  <xsl:variable name="lang.None"      select="string('Keiner')" />
  <xsl:variable name="lang.Notes"      select="string('Notizen')" />
  <xsl:variable name="lang.Notoriety"    select="string('Schlechter Ruf')" />
  <xsl:variable name="lang.Nuyen"      select="string('Nuyen')" />
  <xsl:variable name="lang.Other"      select="string('Andere')" />
  <xsl:variable name="lang.Overflow"      select="string('Überzählig')" />
  <xsl:variable name="lang.OVR"      select="string('ÜbS&#160;')" />
  <xsl:variable name="lang.Pathogen"    select="string('Pathogen')" />
  <xsl:variable name="lang.Permanent"    select="string('Permanent')" />
  <xsl:variable name="lang.Persona"    select="string('Persona')" />
  <xsl:variable name="lang.Pets"      select="string('Haustiere')" />
  <xsl:variable name="lang.Physical"    select="string('Körperlich')" />
  <xsl:variable name="lang.Physiological"  select="string('Körperlich')" />
  <xsl:variable name="lang.Pilot"      select="string('Pilot')" />
  <xsl:variable name="lang.Player"    select="string('Spieler')" />
  <xsl:variable name="lang.Points"    select="string('Punkte')" />
  <xsl:variable name="lang.Pool"      select="string('Pool')" />
  <xsl:variable name="lang.Positive"    select="string('Positiv')" />
  <xsl:variable name="lang.Power"      select="string('Kraft')" />
  <xsl:variable name="lang.Powers"    select="string('Kräfte')" />
  <xsl:variable name="lang.Priorities"  select="string('Prioritäten')" />
  <xsl:variable name="lang.Processor"    select="string('Prozessor')" />
  <xsl:variable name="lang.Program"    select="string('Programm')" />
  <xsl:variable name="lang.Programs"    select="string('Programme')" />
  <xsl:variable name="lang.Psychological"  select="string('Psychisch')" />
  <xsl:variable name="lang.Qty"      select="string('Anz')" />
  <xsl:variable name="lang.Quality"    select="string('Gabe/Handicap')" />
  <xsl:variable name="lang.Qualities"    select="string('Gaben/Handicaps')" />
  <xsl:variable name="lang.Radiation"      select="string('Strahlung')" />
  <xsl:variable name="lang.Range"      select="string('Reichweite')" />
  <xsl:variable name="lang.Rating"    select="string('Stufe')" />
  <xsl:variable name="lang.RC"      select="string('RK')" />
  <xsl:variable name="lang.Reaction"    select="string('Reaktion')" />
  <xsl:variable name="lang.REA"      select="string('REA')" />
  <xsl:variable name="lang.Reach"      select="string('Reichweite')" />
  <xsl:variable name="lang.Reason"    select="string('Grund')" />
  <xsl:variable name="lang.Registered"  select="string('Registriert')" />
  <xsl:variable name="lang.Requires"    select="string('Benötigt')" />
  <xsl:variable name="lang.RES"      select="string('RES')" />
  <xsl:variable name="lang.Resistance"    select="string('Widerstand')" />
  <xsl:variable name="lang.Resistances"    select="string('Widerstände')" />
  <xsl:variable name="lang.Resonance"    select="string('Resonanz')" />
  <xsl:variable name="lang.Resources"    select="string('Ressourcen')" />
  <xsl:variable name="lang.Rigger"    select="string('Rigger')" />
  <xsl:variable name="lang.Rtg"      select="string('St.')" />
  <xsl:variable name="lang.Run"      select="string('Laufen')" />
  <xsl:variable name="lang.S"        select="string('K')" />
  <xsl:variable name="lang.Seats"      select="string('Sitze')" />
  <xsl:variable name="lang.Self"      select="string('Selbst')" />
  <xsl:variable name="lang.Services"    select="string('Dienste')" />
  <xsl:variable name="lang.Sensor"    select="string('Sensor')" />
  <xsl:variable name="lang.Show"      select="string('Zeigen: ')" />
  <xsl:variable name="lang.Skill"      select="string('Fertigkeit')" />
  <xsl:variable name="lang.Skills"    select="string('Fertigkeiten')" />
  <xsl:variable name="lang.Skin"      select="string('Hautfarbe')" />
  <xsl:variable name="lang.Sleaze"    select="string('Schleicher')" />
  <xsl:variable name="lang.SLZ"      select="string('SCH')" />
  <xsl:variable name="lang.Social"    select="string('Sozial')" />
  <xsl:variable name="lang.Sonic"      select="string('Schall')" />
  <xsl:variable name="lang.Source"    select="string('Quelle')" />
  <xsl:variable name="lang.Special"    select="string('Spezial')" />
  <xsl:variable name="lang.Speed"      select="string('Geschw.')" />
  <xsl:variable name="lang.Spell"      select="string('Zauber')" />
  <xsl:variable name="lang.Spells"    select="string('Zauber')" />
  <xsl:variable name="lang.Spirit"    select="string('Geist')" />
  <xsl:variable name="lang.Spirits"    select="string('Geister')" />
  <xsl:variable name="lang.Sprite"    select="string('Sprite')" />
  <xsl:variable name="lang.Sprites"    select="string('Sprites')" />
  <xsl:variable name="lang.Standard"    select="string('Standard')" />
  <xsl:variable name="lang.Stream"    select="string('Stream')" />
  <xsl:variable name="lang.STR"      select="string('STR')" />
  <xsl:variable name="lang.Strength"    select="string('Stärke')" />
  <xsl:variable name="lang.Stun"      select="string('Geistig')" />
  <xsl:variable name="lang.Submersion"  select="string('Wandlung')" />
  <xsl:variable name="lang.Sustained"    select="string('Aufrechterhaltend')" />
  <xsl:variable name="lang.Swim"      select="string('Schwimmen')" />
  <xsl:variable name="lang.Target"    select="string('Ziel')" />
  <xsl:variable name="lang.Tasks"    select="string('Aufgaben')" />
  <xsl:variable name="lang.Total"      select="string('Summe')" />
  <xsl:variable name="lang.Touch"      select="string('Berührung')" />
  <xsl:variable name="lang.Toxin"      select="string('Toxin')" />
  <xsl:variable name="lang.Tradition"    select="string('Tradition')" />
  <xsl:variable name="lang.Type"      select="string('Typ')" />
  <xsl:variable name="lang.Unbound"    select="string('Ungebunden')" />
  <xsl:variable name="lang.Unknown"    select="string('Unbekannt')" />
  <xsl:variable name="lang.Unregistered"  select="string('Unregistriert')" />
  <xsl:variable name="lang.Under"      select="string('Unterlauf')" />
  <xsl:variable name="lang.Vehicle"    select="string('Fahrzeug')" />
  <xsl:variable name="lang.Vehicles"    select="string('Fahrzeuge')" />
  <xsl:variable name="lang.VR"      select="string('VR')" />
  <xsl:variable name="lang.W"        select="string('W')" />
  <xsl:variable name="lang.Walk"      select="string('Gehen')" />
  <xsl:variable name="lang.Weaknesses"    select="string('Schwächen')" />
  <xsl:variable name="lang.Weapon"    select="string('Waffe')" />
  <xsl:variable name="lang.Weapons"    select="string('Waffen')" />
  <xsl:variable name="lang.Week"      select="string('Woche')" />
  <xsl:variable name="lang.Weeks"      select="string('Wochen')" />
  <xsl:variable name="lang.Weight"    select="string('Gewicht')" />
  <xsl:variable name="lang.WIL"      select="string('WIL')" />
  <xsl:variable name="lang.Willpower"    select="string('Willenskraft')" />
  <xsl:variable name="lang.with"      select="string('mit')" />
  <xsl:variable name="lang.Yes"      select="string('Ja')" />

  <!-- multiple word phrases / composite words -->
  <xsl:variable name="lang.ActiveSkills"    select="string('Aktionsfertigkeiten')" />
  <xsl:variable name="lang.AdeptPowers"    select="string('Adeptenkräfte')" />
  <xsl:variable name="lang.AIandAdvanced"    select="string('AI Programme und fortgeschrittene Programme')" />
  <xsl:variable name="lang.AlreadyAddicted"  select="string('Schon abhängig')" />
  <xsl:variable name="lang.ArmorValue"    select="string('Panzerungswert')" />
  <xsl:variable name="lang.AspectedMagician"    select="string('Angesehener Magier')" />
  <xsl:variable name="lang.AstralInitiative"  select="string('Astrale Initiative')" />
  <xsl:variable name="lang.AstralReputation"    select="string('Astral Reputation')" />
  <xsl:variable name="lang.CombatSkill"    select="string('Kampffertigkeit')" />
  <xsl:variable name="lang.ComplexForm"    select="string('Komplexe Form')" />
  <xsl:variable name="lang.ComplexForms"    select="string('Komplexe Formen')" />
  <xsl:variable name="lang.ConditionMonitor"  select="string('Zustandsmonitor')" />
  <xsl:variable name="lang.ContactList"    select="string('Kontaktliste')" />
  <xsl:variable name="lang.CoreTrack"  select="string('Kern Zustandsmonitor')" />
  <xsl:variable name="lang.CritterPower"    select="string('Kräuterstrom')" />
  <xsl:variable name="lang.CritterPowers"    select="string('Critterkräfte')" />
  <xsl:variable name="lang.CurrentEdge"    select="string('Aktuelles Edge')" />
  <xsl:variable name="lang.CurrentForm"    select="string('Aktuelle Form')" />
  <xsl:variable name="lang.DamageType"  select="string('Schadensart')" />
  <xsl:variable name="lang.DataProc"      select="string('Datenverarb.')" />
  <xsl:variable name="lang.DataProcessing"  select="string('Datenverarbeitung')" />
  <xsl:variable name="lang.DecreaseAttribute"    select="string('Attribut senken')" />
  <xsl:variable name="lang.DerivedAttributes"  select="string('Abgeleitete Attribute')" />
  <xsl:variable name="lang.DeviceRating"    select="string('Gerätestufe')" />
  <xsl:variable name="lang.FadingValue"    select="string('Schwund')" />
  <xsl:variable name="lang.HobbiesVice"    select="string('Hobbys/Untugend')" />
  <xsl:variable name="lang.IDcredsticks"    select="string('ID/Credsticks')" />
  <xsl:variable name="lang.InitiateGrade"    select="string('Initiationsgrad')" />
  <xsl:variable name="lang.InitiationNotes"  select="string('Initiationsnotizen')" />
  <xsl:variable name="lang.JudgeIntentions"  select="string('Menschenkenntnis')" />
  <xsl:variable name="lang.KnowledgeSkills"  select="string('Wissensfertigkeiten')" />
  <xsl:variable name="lang.LiftCarry"      select="string('Heben/Tragen')" />
  <xsl:variable name="lang.LineofSight"    select="string('Blickfeld')" />
  <xsl:variable name="lang.LinkedSIN"      select="string('Verbundene SIN')" />
  <xsl:variable name="lang.MartialArt"    select="string('Kampfkunst')" />
  <xsl:variable name="lang.MartialArts"    select="string('Kampfkünste')" />
  <xsl:variable name="lang.MatrixAR"      select="string('Matrix AR')" />
  <xsl:variable name="lang.MatrixCold"    select="string('Matrix Kalt')" />
  <xsl:variable name="lang.MatrixDevices"    select="string('Matrix Geräte')" />
  <xsl:variable name="lang.MatrixHot"      select="string('Matrix Heiß')" />
  <xsl:variable name="lang.MatrixTrack"    select="string('Matrix Zustandsmonitor')" />
  <xsl:variable name="lang.MeleeWeapons"    select="string('Nahkampfwaffen')" />
  <xsl:variable name="lang.MentalAttributes"  select="string('Geistige Attribute')" />
  <xsl:variable name="lang.MysticAdept"    select="string('Mystischer Adept')" />
  <xsl:variable name="lang.NotAddictedYet"  select="string('Noch nicht abhängig')" />
  <xsl:variable name="lang.Nothing2Show4Devices"    select="string('Keine Geräte zur Liste')" />
  <xsl:variable name="lang.Nothing2Show4Notes"    select="string('Keine Notizen zur Liste')" />
  <xsl:variable name="lang.Nothing2Show4SpiritsSprites"    select="string('Keine Geister/Sprites zur Liste')" />
  <xsl:variable name="lang.Nothing2Show4Vehicles"    select="string('Keine Fahrzeuge zur Liste')" />
  <xsl:variable name="lang.OptionalPowers"    select="string('Optional Powers')" />
  <xsl:variable name="lang.OtherArmor"      select="string('Andere Panzerung')" />
  <xsl:variable name="lang.OtherMugshots"    select="string('Andere Portraits')" />
  <xsl:variable name="lang.PageBreak"      select="string('Seitenumbruch: ')" />
  <xsl:variable name="lang.PersonalData"    select="string('Charakterdaten')" />
  <xsl:variable name="lang.PersonalLife"    select="string('Privatleben')" />
  <xsl:variable name="lang.PhysicalAttributes"  select="string('Körperliche Attribute')" />
  <xsl:variable name="lang.PhysicalNaturalRecovery"  select="string('Natürliche Genesung Pool (1 Tag)')" />
  <xsl:variable name="lang.PhysicalTrack"  select="string('Körperlicher Zustandsmonitor')" />
  <xsl:variable name="lang.PreferredPayment"    select="string('Bevorzugte Zahlungsmethode')" />
  <xsl:variable name="lang.PrimaryArm"    select="string('Primärer Arm')" />
  <xsl:variable name="lang.PublicAwareness"  select="string('Prominenz')" />
  <xsl:variable name="lang.RangedWeapons"    select="string('Fernkampfwaffen')" />
  <xsl:variable name="lang.RemainingAvailable"  select="string('Verbleibend Verfügbar')" />
  <xsl:variable name="lang.ResistDrain"    select="string('Entzugswiderstand mit')" />
  <xsl:variable name="lang.ResistFading"    select="string('Schwundwiderstand mit')" />
  <xsl:variable name="lang.RiggerInitiative"  select="string('Rigger AR Initiative')" />
  <xsl:variable name="lang.SelectedGear"    select="string('Gewählte Ausrüstung')" />
  <xsl:variable name="lang.SkillGroup"    select="string('Fertigkeitsgruppe')" />
  <xsl:variable name="lang.SkillGroups"    select="string('Fertigkeitsgruppen')" />
  <xsl:variable name="lang.SpecialAttributes"  select="string('Spezialattribute')" />
  <xsl:variable name="lang.StreetCred"    select="string('Straßenruf')" />
  <xsl:variable name="lang.StreetName"    select="string('Straßenname')" />
  <xsl:variable name="lang.StunNaturalRecovery"  select="string('Natürliche Genesung Pool (1 Stunde)')" />
  <xsl:variable name="lang.StunTrack"    select="string('Geistiger Zustandsmonitor')" />
  <xsl:variable name="lang.SubmersionGrade"  select="string('Wandlungsgrad')" />
  <xsl:variable name="lang.SubmersionNotes"  select="string('Wandlungnotizen')" />
  <xsl:variable name="lang.ToggleColors"  select="string('Farben umschalten')" />
  <xsl:variable name="lang.TotalArmor"  select="string('Insgesamt ausgestattete höchste Rüstung und Zubehör')" />
  <xsl:variable name="lang.ToxinsAndPathogens"  select="string('Toxine und Pathogene')" />
  <xsl:variable name="lang.UnnamedCharacter"  select="string('Unbenannter Charakter')" />
  <xsl:variable name="lang.VehicleBody"    select="string('Rumpf')" />
  <xsl:variable name="lang.VehicleCost"    select="string('Fahrzeugkosten')" />
  <xsl:variable name="lang.WildReputation"    select="string('Wild Reputation')" />

  <!-- "limits" list -->
  <xsl:variable name="lang.AstralLimit"    select="string('Astrales Limit')" />
  <xsl:variable name="lang.MentalLimit"    select="string('Geistiges Limit')" />
  <xsl:variable name="lang.PhysicalLimit"    select="string('Körperliches Limit')" />
  <xsl:variable name="lang.SocialLimit"    select="string('Soziales Limit')" />

  <!-- spell types list -->
  <xsl:variable name="lang.CombatSpells"    select="string('Kampfzauber')" />
  <xsl:variable name="lang.DetectionSpells"  select="string('Wahrnehmungszauber')" />
  <xsl:variable name="lang.Enchantments"    select="string('Verzauberungen')" />
  <xsl:variable name="lang.HealthSpells"     select="string('Heilzauber')" />
  <xsl:variable name="lang.IllusionSpells"   select="string('Illusionszauber')" />
  <xsl:variable name="lang.ManipulationSpells" select="string('Manipulationszauber')" />
  <xsl:variable name="lang.Rituals"      select="string('Rituale')" />

  <!-- test values -->
  <xsl:variable name="lang.tstDamage1"  select="string('K')" />
  <xsl:variable name="lang.tstDamage2"  select="string('G')" />
  <xsl:variable name="lang.tstDuration1"  select="string('S')" />
  <xsl:variable name="lang.tstDuration2"  select="string('P')" />
  <xsl:variable name="lang.tstDuration3"  select="string('A')" />
  <xsl:variable name="lang.tstRange1"    select="string('B')" />
  <xsl:variable name="lang.tstRange2"    select="string('BF')" />
  <xsl:variable name="lang.tstRange3"    select="string('BF(F)')" />
  <xsl:variable name="lang.tstRange4"    select="string('BF (F)')" />
  <xsl:variable name="lang.tstRange5"    select="string('S')" />
  <xsl:variable name="lang.tstRange6"    select="string('S(F)')" />
  <xsl:variable name="lang.tstRange7"    select="string('S (F)')" />
  <xsl:variable name="lang.tstRange8"    select="string('B(F)')" />
  <xsl:variable name="lang.tstRange9"    select="string('B (F)')" />
  <xsl:variable name="lang.tstRange10"    select="string('Special')" />

  <!-- miscellaneous signs and symbols -->
    <!-- currency symbol -->
  <xsl:variable name="lang.NuyenSymbol"  select="string('&#165;')" />
  <!-- diacrtic marks: decimal mark and grouping separator -->
    <xsl:variable name="lang.marks"    select="string(',.')" />
</xsl:stylesheet>
