<?xml version="1.0" encoding="utf-8" ?>
<!-- Isolate French (fr) locale literals -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="lang"  select="string('fr')" />
  <xsl:variable name="locale"  select="string('fr-fr')" />

  <!-- individual words -->
  <xsl:variable name="lang.Acceleration"  select="string('Accélération')" />
  <xsl:variable name="lang.Accel"      select="string('Accél')" />
  <xsl:variable name="lang.Accessories"  select="string('Accessoires')" />
  <xsl:variable name="lang.Accuracy"    select="string('Précision')" />
  <xsl:variable name="lang.Acid"      select="string('Acide')" />
  <xsl:variable name="lang.Action"      select="string('Action')" />
  <xsl:variable name="lang.Addiction"  select="string('Dépendance')" />
  <xsl:variable name="lang.Adept"      select="string('Adept')" />
  <xsl:variable name="lang.Age"      select="string('Âge')" />
  <xsl:variable name="lang.AGI"      select="string('AGI')" />
  <xsl:variable name="lang.Agility"    select="string('Agilité')" />
  <xsl:variable name="lang.AI"      select="string('AI')" />
  <xsl:variable name="lang.Alias"      select="string('Pseudonyme')" />
  <xsl:variable name="lang.Ammo"      select="string('Munitions')" />
  <xsl:variable name="lang.Amount"    select="string('Somme')" />
  <xsl:variable name="lang.AP"      select="string('PA')" />
  <xsl:variable name="lang.Applicable"  select="string('En vigueur')" />
  <xsl:variable name="lang.Apprentice"  select="string('Apprenti')" />
  <xsl:variable name="lang.AR"      select="string('RA')" />
  <xsl:variable name="lang.Archetype"    select="string('Archétype')" />
  <xsl:variable name="lang.Area"      select="string('Zone')" />
  <xsl:variable name="lang.Armor"      select="string('Armure')" />
  <xsl:variable name="lang.Arts"      select="string('Arts')" />
  <xsl:variable name="lang.as"      select="string('comme')" />
  <xsl:variable name="lang.ASDF"    select="string('A/C/D/F')" />
  <xsl:variable name="lang.Astral"    select="string('Astral')" />
  <xsl:variable name="lang.Attack"    select="string('Attaque')" />
  <xsl:variable name="lang.ATT"      select="string('ATT')" />
  <xsl:variable name="lang.Attribute"    select="string('Attribut')" />
  <xsl:variable name="lang.Attributes"  select="string('Attributs')" />
  <xsl:variable name="lang.Available"    select="string('Disponible')" />
  <xsl:variable name="lang.Awakened"    select="string('Éveillé')" />
  <xsl:variable name="lang.Aware"    select="string('Conscient')" />
  <xsl:variable name="lang.Background"  select="string('Background')" />
  <xsl:variable name="lang.Base"      select="string('Base')" />
  <xsl:variable name="lang.Bioware"    select="string('Bioware')" />
  <xsl:variable name="lang.BOD"      select="string('CON')" />
  <xsl:variable name="lang.Body"      select="string('Constitution')" />
  <xsl:variable name="lang.Bonus"      select="string('Bonus')" />
  <xsl:variable name="lang.Bound"      select="string('Lié')" />
  <xsl:variable name="lang.Calendar"    select="string('Calendrier')" />
  <xsl:variable name="lang.Career"    select="string('Carrière')" />
  <xsl:variable name="lang.Category"    select="string('Catégorie')" />
  <xsl:variable name="lang.CHA"      select="string('CHA')" />
  <xsl:variable name="lang.Charisma"    select="string('Charisme')" />
  <xsl:variable name="lang.CM"      select="string('MC')" />
  <xsl:variable name="lang.Cold"      select="string('Froid')" />
  <xsl:variable name="lang.Combat"    select="string('Combat')" />
  <xsl:variable name="lang.Commlink"    select="string('Commlink')" />
  <xsl:variable name="lang.Composure"    select="string('Sang-Froid')" />
  <xsl:variable name="lang.Concept"    select="string('Concept')" />
  <xsl:variable name="lang.Connection"  select="string('Connexion')" />
  <xsl:variable name="lang.Contact"    select="string('Contact')" />
  <xsl:variable name="lang.ContactDrug"    select="string('Contact')" />
  <xsl:variable name="lang.Contacts"    select="string('Contacts')" />
  <xsl:variable name="lang.Cost"      select="string('Coût')" />
  <xsl:variable name="lang.Critter"    select="string('Créature')" />
  <xsl:variable name="lang.Critters"    select="string('Créatures')" />
  <xsl:variable name="lang.Cyberware"    select="string('Cyberware')" />
  <xsl:variable name="lang.Damage"    select="string('Dégât')" />
  <xsl:variable name="lang.Data"      select="string('Donnée')" />
  <xsl:variable name="lang.Date"      select="string('Date')" />
  <xsl:variable name="lang.Day"      select="string('Jour')" />
  <xsl:variable name="lang.Days"      select="string('Jours')" />
  <xsl:variable name="lang.Dead"      select="string('Mort')" />
  <xsl:variable name="lang.Defense"      select="string('Défense')" />
  <xsl:variable name="lang.DEP"  select="string('DEP')" />
  <xsl:variable name="lang.Depth"  select="string('Depth')" />
  <xsl:variable name="lang.Description"  select="string('Description')" />
  <xsl:variable name="lang.Detection"    select="string('Détection')" />
  <xsl:variable name="lang.Device"    select="string('Appareil')" />
  <xsl:variable name="lang.Devices"    select="string('Appareils')" />
  <xsl:variable name="lang.Direct"      select="string('Direct')" />
  <xsl:variable name="lang.Down"      select="string('A terre')" />
  <xsl:variable name="lang.DP"        select="string('TD')" />
  <xsl:variable name="lang.Drain"      select="string('Drain')" />
  <xsl:variable name="lang.Drone"      select="string('Drone')" />
  <xsl:variable name="lang.Duration"    select="string('Durée')" />
  <xsl:variable name="lang.DV"      select="string('VD')" />
  <xsl:variable name="lang.E"        select="string('E')" />
  <xsl:variable name="lang.Echo"    select="string('Écho')" />
  <xsl:variable name="lang.Echoes"    select="string('Échos')" />
  <xsl:variable name="lang.EDG"      select="string('CHC')" />
  <xsl:variable name="lang.Edge"      select="string('Chance')" />
  <xsl:variable name="lang.Electricity"    select="string('Électricité')" />
  <xsl:variable name="lang.Enchanter"    select="string('Enchanteur')" />
  <xsl:variable name="lang.Enemies"    select="string('Ennemis')" />
  <xsl:variable name="lang.Enhancements"  select="string('Améliorations')" />
  <xsl:variable name="lang.Entries"    select="string('Entrées')" />
  <xsl:variable name="lang.Equipped"    select="string('Équipé')" />
  <xsl:variable name="lang.ESS"      select="string('ESS')" />
  <xsl:variable name="lang.Essence"    select="string('Essence')" />
  <xsl:variable name="lang.Expenses"    select="string('Dépenses')" />
  <xsl:variable name="lang.Explorer"    select="string('Explorateur')" />
  <xsl:variable name="lang.Eyes"      select="string('Yeux')" />
  <xsl:variable name="lang.Falling"    select="string('Chute')" />
  <xsl:variable name="lang.Fatigue"      select="string('Fatigue')" />
  <xsl:variable name="lang.Fettered"      select="string('Enchaîné')" />
  <xsl:variable name="lang.Fire"    select="string('Feu')" />
  <xsl:variable name="lang.Firewall"    select="string('Firewall')" />
  <xsl:variable name="lang.Fly"      select="string('Mouche')" />
  <xsl:variable name="lang.Foci"      select="string('Focus')" />
  <xsl:variable name="lang.FWL"      select="string('FWL')" />
  <xsl:variable name="lang.Force"      select="string('Force')" />
  <xsl:variable name="lang.FV"      select="string('VT')" />
  <xsl:variable name="lang.Gear"      select="string('Équipement')" />
  <xsl:variable name="lang.Gender"      select="string('Gent')" />
  <xsl:variable name="lang.Grade"      select="string('Qualité')" />
  <xsl:variable name="lang.Hair"      select="string('Cheveux')" />
  <xsl:variable name="lang.Handling"    select="string('Manipulation')" />
  <xsl:variable name="lang.Health"    select="string('Santé')" />
  <xsl:variable name="lang.Heavy"    select="string('Lourd')" />
  <xsl:variable name="lang.Height"    select="string('Taille')" />
  <xsl:variable name="lang.hit"      select="string('Succès')" />
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
  <xsl:variable name="lang.Instantaneous"  select="string('Instantané')" />
  <xsl:variable name="lang.Karma"      select="string('Karma')" />
  <xsl:variable name="lang.L"        select="string('L')" />
  <xsl:variable name="lang.Level"      select="string('Niveau')" />
  <xsl:variable name="lang.Lifestyle"    select="string('Niveaux de vie')" />
  <xsl:variable name="lang.Limit"      select="string('Limite')" />
  <xsl:variable name="lang.Limits"    select="string('Limites')" />
  <xsl:variable name="lang.Loaded"    select="string('Chargé')" />
  <xsl:variable name="lang.Location"    select="string('Emplacement')" />
  <xsl:variable name="lang.LOG"      select="string('LOG')" />
  <xsl:variable name="lang.Logic"      select="string('Logique')" />
  <xsl:variable name="lang.Loyalty"    select="string('Loyauté')" />
  <xsl:variable name="lang.M"        select="string('M')" />
  <xsl:variable name="lang.MAG"      select="string('MAG')" />
  <xsl:variable name="lang.Magic"      select="string('Magie')" />
  <xsl:variable name="lang.Magician"      select="string('Magicien')" />
  <xsl:variable name="lang.Mana"    select="string('Mana')" />
  <xsl:variable name="lang.Maneuvers"    select="string('Manœuvres')" />
  <xsl:variable name="lang.Manipulation"  select="string('Manipulation')" />
  <xsl:variable name="lang.Manual"    select="string('Manuel')" />
  <xsl:variable name="lang.Memory"    select="string('Mémoire')" />
  <xsl:variable name="lang.Mental"    select="string('Mentale')" />
  <xsl:variable name="lang.Metamagics"  select="string('Métamagies')" />
  <xsl:variable name="lang.Metatype"    select="string('Métatype')" />
  <xsl:variable name="lang.Mod"      select="string('Mod')" />
  <xsl:variable name="lang.Mode"      select="string('Mode')" />
  <xsl:variable name="lang.Model"      select="string('Modèle')" />
  <xsl:variable name="lang.Modifications"  select="string('Modificateurs')" />
  <xsl:variable name="lang.Month"      select="string('Mois')" />
  <xsl:variable name="lang.Months"    select="string('Mois')" />
  <xsl:variable name="lang.Mount"    select="string('Monter')" />
  <xsl:variable name="lang.Movement"    select="string('Mouvement')" />
  <xsl:variable name="lang.Mugshot"    select="string('Portrait')" />
  <xsl:variable name="lang.Name"      select="string('Nom')" />
  <xsl:variable name="lang.Native"    select="string('Maternelle')" />
  <xsl:variable name="lang.Negative"    select="string('Négatif')" />
  <xsl:variable name="lang.No"      select="string('Non')" />
  <xsl:variable name="lang.None"      select="string('Aucun')" />
  <xsl:variable name="lang.Notes"      select="string('Remarques')" />
  <xsl:variable name="lang.Notoriety"    select="string('Rumeur')" />
  <xsl:variable name="lang.Nuyen"      select="string('Nuyen')" />
  <xsl:variable name="lang.Other"      select="string('Autre')" />
  <xsl:variable name="lang.Overflow"      select="string('Déborder')" />
  <xsl:variable name="lang.OVR"      select="string('SUR&#160;')" />
  <xsl:variable name="lang.Pathogen"    select="string('Pathogènes')" />
  <xsl:variable name="lang.Permanent"    select="string('Permanent')" />
  <xsl:variable name="lang.Persona"    select="string('Persona')" />
  <xsl:variable name="lang.Pets"      select="string('Animaux de compagnie')" />
  <xsl:variable name="lang.Physical"    select="string('Physique')" />
  <xsl:variable name="lang.Physiological"  select="string('Physiologique')" />
  <xsl:variable name="lang.Pilot"      select="string('Pilote')" />
  <xsl:variable name="lang.Player"    select="string('Joueur')" />
  <xsl:variable name="lang.Points"    select="string('Points')" />
  <xsl:variable name="lang.Pool"      select="string('Réserve')" />
  <xsl:variable name="lang.Positive"    select="string('Positif')" />
  <xsl:variable name="lang.Power"      select="string('Puissance')" />
  <xsl:variable name="lang.Powers"    select="string('Pouvoirs')" />
  <xsl:variable name="lang.Priorities"  select="string('Priorités')" />
  <xsl:variable name="lang.Processor"    select="string('Processeur')" />
  <xsl:variable name="lang.Program"    select="string('Programme')" />
  <xsl:variable name="lang.Programs"    select="string('Programmes')" />
  <xsl:variable name="lang.Psychological"  select="string('Psychologique')" />
  <xsl:variable name="lang.Qty"      select="string('Qté')" />
  <xsl:variable name="lang.Quality"    select="string('Trait')" />
  <xsl:variable name="lang.Qualities"    select="string('Traits')" />
  <xsl:variable name="lang.Radiation"      select="string('Radiation')" />
  <xsl:variable name="lang.Range"      select="string('Portée')" />
  <xsl:variable name="lang.Rating"    select="string('Puissance')" />
  <xsl:variable name="lang.RC"      select="string('CR')" />
  <xsl:variable name="lang.Reaction"    select="string('Réaction')" />
  <xsl:variable name="lang.REA"      select="string('RÉA')" />
  <xsl:variable name="lang.Reach"      select="string('Atteindre')" />
  <xsl:variable name="lang.Reason"    select="string('Raison')" />
  <xsl:variable name="lang.Registered"  select="string('Enregistré')" />
  <xsl:variable name="lang.Requires"    select="string('Requis')" />
  <xsl:variable name="lang.RES"      select="string('RES')" />
  <xsl:variable name="lang.Resistance"    select="string('Résistance')" />
  <xsl:variable name="lang.Resistances"    select="string('Résistances')" />
  <xsl:variable name="lang.Resonance"    select="string('Résonance')" />
  <xsl:variable name="lang.Resources"    select="string('Ressources')" />
  <xsl:variable name="lang.Rigger"    select="string('Interfacé')" />
  <xsl:variable name="lang.Rtg"      select="string('Ind')" />
  <xsl:variable name="lang.Run"      select="string('Courir')" />
  <xsl:variable name="lang.S"        select="string('F')" />
  <xsl:variable name="lang.Seats"      select="string('Places')" />
  <xsl:variable name="lang.Self"      select="string('Soi')" />
  <xsl:variable name="lang.Services"    select="string('Services')" />
  <xsl:variable name="lang.Sensor"    select="string('Senseur')" />
  <xsl:variable name="lang.Show"      select="string('Montrer: ')" />
  <xsl:variable name="lang.Skill"      select="string('Compétence')" />
  <xsl:variable name="lang.Skills"    select="string('Compétences')" />
  <xsl:variable name="lang.Skin"      select="string('Peau')" />
  <xsl:variable name="lang.Sleaze"    select="string('Corruption')" />
  <xsl:variable name="lang.SLZ"      select="string('SLZ')" />
  <xsl:variable name="lang.Social"    select="string('Sociale')" />
  <xsl:variable name="lang.Sonic"      select="string('Sonique')" />
  <xsl:variable name="lang.Source"    select="string('Source')" />
  <xsl:variable name="lang.Special"    select="string('Spécial')" />
  <xsl:variable name="lang.Speed"      select="string('Vitesse')" />
  <xsl:variable name="lang.Spell"      select="string('Sort')" />
  <xsl:variable name="lang.Spells"    select="string('Sorts')" />
  <xsl:variable name="lang.Spirit"    select="string('Esprit')" />
  <xsl:variable name="lang.Spirits"    select="string('Esprits')" />
  <xsl:variable name="lang.Sprite"    select="string('Sprite')" />
  <xsl:variable name="lang.Sprites"    select="string('Sprites')" />
  <xsl:variable name="lang.Standard"    select="string('Standard')" />
  <xsl:variable name="lang.Stream"    select="string('Grille')" />
  <xsl:variable name="lang.STR"      select="string('FOR')" />
  <xsl:variable name="lang.Strength"    select="string('Force')" />
  <xsl:variable name="lang.Stun"      select="string('Étourdi')" />
  <xsl:variable name="lang.Submersion"  select="string('Submersion')" />
  <xsl:variable name="lang.Sustained"    select="string('Maintenu')" />
  <xsl:variable name="lang.Swim"      select="string('Nager')" />
  <xsl:variable name="lang.Target"    select="string('Cible')" />
  <xsl:variable name="lang.Tasks"    select="string('Tasks')" />
  <xsl:variable name="lang.Total"      select="string('Total')" />
  <xsl:variable name="lang.Touch"      select="string('Toucher')" />
  <xsl:variable name="lang.Toxin"      select="string('Toxines')" />
  <xsl:variable name="lang.Tradition"    select="string('Tradition')" />
  <xsl:variable name="lang.Type"      select="string('Type')" />
  <xsl:variable name="lang.Unbound"    select="string('Non lié')" />
  <xsl:variable name="lang.Unknown"    select="string('Inconnu')" />
  <xsl:variable name="lang.Unregistered"  select="string('Non enregistré')" />
  <xsl:variable name="lang.Under"      select="string('Sous')" />
  <xsl:variable name="lang.Vehicle"    select="string('Véhicule')" />
  <xsl:variable name="lang.Vehicles"    select="string('Véhicules')" />
  <xsl:variable name="lang.VR"      select="string('RV')" />
  <xsl:variable name="lang.W"        select="string('V')" />
  <xsl:variable name="lang.Walk"      select="string('Marche')" />
  <xsl:variable name="lang.Weaknesses"    select="string('Faiblesses')" />
  <xsl:variable name="lang.Weapon"    select="string('Arme')" />
  <xsl:variable name="lang.Weapons"    select="string('Armes')" />
  <xsl:variable name="lang.Week"      select="string('Semaine')" />
  <xsl:variable name="lang.Weeks"      select="string('Semaines')" />
  <xsl:variable name="lang.Weight"    select="string('Poids')" />
  <xsl:variable name="lang.WIL"      select="string('VOL')" />
  <xsl:variable name="lang.Willpower"    select="string('Volonté')" />
  <xsl:variable name="lang.with"      select="string('avec')" />
  <xsl:variable name="lang.Yes"      select="string('Oui')" />

  <!-- multiple word phrases / composite words -->
  <xsl:variable name="lang.ActiveSkills"    select="string('Compétences actives')" />
  <xsl:variable name="lang.AdeptPowers"    select="concat('Pouvoirs d',$APOS,'adeptes')" />
  <xsl:variable name="lang.AIandAdvanced"    select="string('IA Programmes et Programmes Avancés')" />
  <xsl:variable name="lang.AlreadyAddicted"  select="string('Déjà adonné')" />
  <xsl:variable name="lang.ArmorValue"    select="concat('Valeur d',$APOS,'armure')" />
  <xsl:variable name="lang.AspectedMagician"    select="string('Magicien spécialisé')" />
  <xsl:variable name="lang.AstralInitiative"  select="string('Initiative astrale')" />
  <xsl:variable name="lang.AstralReputation"    select="string('Reputation astrale')" />
  <xsl:variable name="lang.CombatSkill"    select="string('Compétences de combat')" />
  <xsl:variable name="lang.ComplexForm"    select="string('Forme complexe')" />
  <xsl:variable name="lang.ComplexForms"    select="string('Formes complexes')" />
  <xsl:variable name="lang.ConditionMonitor"  select="string('Moniteur de conditions')" />
  <xsl:variable name="lang.ContactList"    select="string('Liste de contacts')" />
  <xsl:variable name="lang.CoreTrack"  select="string('Core Damage Track')" />
  <xsl:variable name="lang.CritterPower"    select="string('Pouvoir de créature')" />
  <xsl:variable name="lang.CritterPowers"    select="string('Pouvoirs de créature')" />
  <xsl:variable name="lang.CurrentEdge"    select="string('Points de chance courants')" />
  <xsl:variable name="lang.CurrentForm"    select="string('Forme courante')" />
  <xsl:variable name="lang.DamageType"  select="string('Type de dommage')" />
  <xsl:variable name="lang.DataProc"      select="string('Trait. données')" />
  <xsl:variable name="lang.DataProcessing"  select="string('Traitement de données')" />
  <xsl:variable name="lang.DecreaseAttribute"    select="string('Decrease Attribute')" />
  <xsl:variable name="lang.DerivedAttributes"  select="string('Attributs dérivés')" />
  <xsl:variable name="lang.DeviceRating"    select="string('Évaluation')" />
  <xsl:variable name="lang.FadingValue"    select="string('Technodrain')" />
  <xsl:variable name="lang.HobbiesVice"    select="string('Loisirs/Vice')" />
  <xsl:variable name="lang.IDcredsticks"    select="string('Identité et créditubes')" />
  <xsl:variable name="lang.InitiateGrade"    select="concat('Grade d',$APOS,'initiation')" />
  <xsl:variable name="lang.InitiationNotes"   select="concat('Remarques d',$APOS,'initiation')" />
  <xsl:variable name="lang.JudgeIntentions"  select="string('Jauger les intentions')" />
  <xsl:variable name="lang.KnowledgeSkills"  select="string('Compétences de connaissance')" />
  <xsl:variable name="lang.LiftCarry"      select="string('Soulever / transporter')" />
  <xsl:variable name="lang.LineofSight"    select="string('Ligne de mire')" />
  <xsl:variable name="lang.LinkedSIN"      select="string('SIN lié')" />
  <xsl:variable name="lang.MartialArt"    select="string('Art martial')" />
  <xsl:variable name="lang.MartialArts"    select="string('Arts martiaux')" />
  <xsl:variable name="lang.MatrixAR"      select="string('Init. RA')" />
  <xsl:variable name="lang.MatrixCold"    select="string('Init. Cold SIM')" />
  <xsl:variable name="lang.MatrixDevices"    select="string('Equipement matriciel')" />
  <xsl:variable name="lang.MatrixHot"      select="string('Init. Hot SIM')" />
  <xsl:variable name="lang.MatrixTrack"    select="string('Moniteur de condition matricielle')" />
  <xsl:variable name="lang.MeleeWeapons"    select="string('Armes de corps à corps')" />
  <xsl:variable name="lang.MentalAttributes"  select="string('Attributs mentaux')" />
  <xsl:variable name="lang.MysticAdept"    select="string('Adepte mystique')" />
  <xsl:variable name="lang.NotAddictedYet"  select="string('Pas encore accro')" />
  <xsl:variable name="lang.Nothing2Show4Devices"    select="string('Aucun appareil à répertorier')" />
  <xsl:variable name="lang.Nothing2Show4Notes"    select="string('Pas de notes à la liste')" />
  <xsl:variable name="lang.Nothing2Show4SpiritsSprites"    select="string('Aucun Esprits/Sprites à la liste')" />
  <xsl:variable name="lang.Nothing2Show4Vehicles"    select="string('Aucun véhicule à la liste')" />
  <xsl:variable name="lang.OptionalPowers"    select="string('Pouvoirs optionels')" />
  <xsl:variable name="lang.OtherArmor"      select="string('Autre armure')" />
  <xsl:variable name="lang.OtherMugshots"    select="string('Autres portraits')" />
  <xsl:variable name="lang.PageBreak"      select="string('Saut de page : ')" />
  <xsl:variable name="lang.PersonalData"    select="string('Données personnelles')" />
  <xsl:variable name="lang.PersonalLife"    select="string('Vie privée')" />
  <xsl:variable name="lang.PhysicalAttributes"  select="string('Attributs physiques')" />
  <xsl:variable name="lang.PhysicalNaturalRecovery"  select="string('Guérison naturelle (1 jour)')" />
  <xsl:variable name="lang.PhysicalTrack"  select="string('Moniteur physique')" />
  <xsl:variable name="lang.PreferredPayment"    select="string('Paiement préféré')" />
  <xsl:variable name="lang.PrimaryArm"    select="string('Main directrice')" />
  <xsl:variable name="lang.PublicAwareness"  select="string('Renommée')" />
  <xsl:variable name="lang.RangedWeapons"    select="string('Armes à distance')" />
  <xsl:variable name="lang.RemainingAvailable"  select="string('Disponible')" />
  <xsl:variable name="lang.ResistDrain"    select="string('Résister au drain avec')" />
  <xsl:variable name="lang.ResistFading"    select="string('Résister au technodrain avec')" />
  <xsl:variable name="lang.RiggerInitiative"  select="string('Init. interfacé')" />
  <xsl:variable name="lang.SelectedGear"    select="string('Équipement sélectionné')" />
  <xsl:variable name="lang.SkillGroup"    select="string('Groupe de compétences')" />
  <xsl:variable name="lang.SkillGroups"    select="string('Groupes de compétences')" />
  <xsl:variable name="lang.SpecialAttributes"  select="string('Attributs spéciaux')" />
  <xsl:variable name="lang.StreetCred"    select="string('Crédibilité')" />
  <xsl:variable name="lang.StreetName"    select="string('Nom de rue')" />
  <xsl:variable name="lang.StunNaturalRecovery"  select="string('Guérison naturelle (1 heure)')" />
  <xsl:variable name="lang.StunTrack"    select="string('Moniteur de étourdissant')" />
  <xsl:variable name="lang.SubmersionGrade"  select="string('Degré de submersion')" />
  <xsl:variable name="lang.SubmersionNotes"  select="string('Notes de submersion')" />
  <xsl:variable name="lang.ToggleColors"  select="string('Basculer les couleurs')" />
  <xsl:variable name="lang.TotalArmor"  select="string('Total des meilleurs armures et accessoires équipés')" />
  <xsl:variable name="lang.ToxinsAndPathogens"  select="string('Toxines et pathogènes')" />
  <xsl:variable name="lang.UnnamedCharacter"  select="string('Personnage sans nom')" />
  <xsl:variable name="lang.VehicleBody"    select="string('Structure du véhicule')" />
  <xsl:variable name="lang.VehicleCost"    select="string('Coût du véhicule')" />
  <xsl:variable name="lang.WildReputation"    select="string('Wild Reputation')" />

  <!-- "limits" list -->
  <xsl:variable name="lang.AstralLimit"    select="string('Limite astrale')" />
  <xsl:variable name="lang.MentalLimit"    select="string('Limite mentale')" />
  <xsl:variable name="lang.PhysicalLimit"    select="string('Limite physique')" />
  <xsl:variable name="lang.SocialLimit"    select="string('Limite sociale')" />

  <!-- spell types list -->
  <xsl:variable name="lang.CombatSpells"    select="string('Sorts de combat')" />
  <xsl:variable name="lang.DetectionSpells"  select="string('Sorts de détection')" />
  <xsl:variable name="lang.Enchantments"    select="string('Enchantements')" />
  <xsl:variable name="lang.HealthSpells"     select="string('Sorts de santé')" />
  <xsl:variable name="lang.IllusionSpells"   select="concat('Sorts d',$APOS,'illusion')" />
  <xsl:variable name="lang.ManipulationSpells" select="string('Sorts de manipulation')" />
  <xsl:variable name="lang.Rituals"      select="string('Rituels')" />

  <!-- test values -->
  <xsl:variable name="lang.tstDamage1"  select="string('P')" />
  <xsl:variable name="lang.tstDamage2"  select="string('E')" />
  <xsl:variable name="lang.tstDuration1"  select="string('I')" />
  <xsl:variable name="lang.tstDuration2"  select="string('P')" />
  <xsl:variable name="lang.tstDuration3"  select="string('M')" />
  <xsl:variable name="lang.tstRange1"    select="string('T')" />
  <xsl:variable name="lang.tstRange2"    select="string('CDV')" />
  <xsl:variable name="lang.tstRange3"    select="string('CDV(Z)')" />
  <xsl:variable name="lang.tstRange4"    select="string('CDV (Z)')" />
  <xsl:variable name="lang.tstRange5"    select="string('S')" />
  <xsl:variable name="lang.tstRange6"    select="string('S(Z)')" />
  <xsl:variable name="lang.tstRange7"    select="string('S (Z)')" />
  <xsl:variable name="lang.tstRange8"    select="string('T(Z)')" />
  <xsl:variable name="lang.tstRange9"    select="string('T (Z)')" />
  <xsl:variable name="lang.tstRange10"    select="string('Special')" />

  <!-- miscellaneous signs and symbols -->
    <!-- currency symbol -->
  <xsl:variable name="lang.NuyenSymbol"  select="string('&#165;')" />
    <!-- diacrtic marks: decimal mark and grouping separator -->
  <xsl:variable name="lang.marks"      select="string(',.')" />
    <!-- single quote for use with d' and l' -->
  <xsl:variable name="APOS">'</xsl:variable>
</xsl:stylesheet>
