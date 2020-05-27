<?xml version="1.0" encoding="utf-8" ?>
<!-- Isolate French (fr) locale literals -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="lang"  select="'fr'"/>
  <xsl:variable name="locale"  select="'fr-fr'"/>

  <!-- individual words -->
  <xsl:variable name="lang.Acceleration"  select="'Accélération'"/>
  <xsl:variable name="lang.Accel"      select="'Accél'"/>
  <xsl:variable name="lang.Accessories"  select="'Accessoires'"/>
  <xsl:variable name="lang.Accuracy"    select="'Précision'"/>
  <xsl:variable name="lang.Acid"      select="'Acide'"/>
  <xsl:variable name="lang.Action"      select="'Action'"/>
  <xsl:variable name="lang.Addiction"  select="'Dépendance'"/>
  <xsl:variable name="lang.Adept"      select="'Adept'"/>
  <xsl:variable name="lang.Age"      select="'Âge'"/>
  <xsl:variable name="lang.AGI"      select="'AGI'"/>
  <xsl:variable name="lang.Agility"    select="'Agilité'"/>
  <xsl:variable name="lang.AI"      select="'AI'"/>
  <xsl:variable name="lang.Alias"      select="'Pseudonyme'"/>
  <xsl:variable name="lang.Ammo"      select="'Munitions'"/>
  <xsl:variable name="lang.Amount"    select="'Somme'"/>
  <xsl:variable name="lang.AP"      select="'PA'"/>
  <xsl:variable name="lang.Applicable"  select="'En vigueur'"/>
  <xsl:variable name="lang.Apprentice"  select="'Apprenti'"/>
  <xsl:variable name="lang.AR"      select="'RA'"/>
  <xsl:variable name="lang.Archetype"    select="'Archétype'"/>
  <xsl:variable name="lang.Area"      select="'Zone'"/>
  <xsl:variable name="lang.Armor"      select="'Armure'"/>
  <xsl:variable name="lang.Arts"      select="'Arts'"/>
  <xsl:variable name="lang.as"      select="'comme'"/>
  <xsl:variable name="lang.ASDF"    select="'A/C/D/F'"/>
  <xsl:variable name="lang.Astral"    select="'Astral'"/>
  <xsl:variable name="lang.Attack"    select="'Attaque'"/>
  <xsl:variable name="lang.ATT"      select="'ATT'"/>
  <xsl:variable name="lang.Attribute"    select="'Attribut'"/>
  <xsl:variable name="lang.Attributes"  select="'Attributs'"/>
  <xsl:variable name="lang.Available"    select="'Disponible'"/>
  <xsl:variable name="lang.Awakened"    select="'Éveillé'"/>
  <xsl:variable name="lang.Aware"    select="'Conscient'"/>
  <xsl:variable name="lang.Background"  select="'Background'"/>
  <xsl:variable name="lang.Base"      select="'Base'"/>
  <xsl:variable name="lang.Bioware"    select="'Bioware'"/>
  <xsl:variable name="lang.BOD"      select="'CON'"/>
  <xsl:variable name="lang.Body"      select="'Constitution'"/>
  <xsl:variable name="lang.Bonus"      select="'Bonus'"/>
  <xsl:variable name="lang.Bound"      select="'Lié'"/>
  <xsl:variable name="lang.Calendar"    select="'Calendrier'"/>
  <xsl:variable name="lang.Career"    select="'Carrière'"/>
  <xsl:variable name="lang.Category"    select="'Catégorie'"/>
  <xsl:variable name="lang.CHA"      select="'CHA'"/>
  <xsl:variable name="lang.Charisma"    select="'Charisme'"/>
  <xsl:variable name="lang.CM"      select="'MC'"/>
  <xsl:variable name="lang.Cold"      select="'Froid'"/>
  <xsl:variable name="lang.Combat"    select="'Combat'"/>
  <xsl:variable name="lang.Commlink"    select="'Commlink'"/>
  <xsl:variable name="lang.Composure"    select="'Sang-Froid'"/>
  <xsl:variable name="lang.Concept"    select="'Concept'"/>
  <xsl:variable name="lang.Connection"  select="'Connexion'"/>
  <xsl:variable name="lang.Contact"    select="'Contact'"/>
  <xsl:variable name="lang.ContactDrug"    select="'Contact'"/>
  <xsl:variable name="lang.Contacts"    select="'Contacts'"/>
  <xsl:variable name="lang.Cost"      select="'Coût'"/>
  <xsl:variable name="lang.Critter"    select="'Créature'"/>
  <xsl:variable name="lang.Critters"    select="'Créatures'"/>
  <xsl:variable name="lang.Cyberware"    select="'Cyberware'"/>
  <xsl:variable name="lang.Damage"    select="'Dégât'"/>
  <xsl:variable name="lang.Data"      select="'Donnée'"/>
  <xsl:variable name="lang.Date"      select="'Date'"/>
  <xsl:variable name="lang.Day"      select="'Jour'"/>
  <xsl:variable name="lang.Days"      select="'Jours'"/>
  <xsl:variable name="lang.Dead"      select="'Mort'"/>
  <xsl:variable name="lang.Defense"      select="'Défense'"/>
  <xsl:variable name="lang.DEP"  select="'DEP'"/>
  <xsl:variable name="lang.Depth"  select="'Depth'"/>
  <xsl:variable name="lang.Description"  select="'Description'"/>
  <xsl:variable name="lang.Detection"    select="'Détection'"/>
  <xsl:variable name="lang.Device"    select="'Appareil'"/>
  <xsl:variable name="lang.Devices"    select="'Appareils'"/>
  <xsl:variable name="lang.Direct"      select="'Direct'"/>
  <xsl:variable name="lang.Down"      select="'A terre'"/>
  <xsl:variable name="lang.DP"        select="'TD'"/>
  <xsl:variable name="lang.Drain"      select="'Drain'"/>
  <xsl:variable name="lang.Drone"      select="'Drone'"/>
  <xsl:variable name="lang.Duration"    select="'Durée'"/>
  <xsl:variable name="lang.DV"      select="'VD'"/>
  <xsl:variable name="lang.E"        select="'E'"/>
  <xsl:variable name="lang.Echo"    select="'Écho'"/>
  <xsl:variable name="lang.Echoes"    select="'Échos'"/>
  <xsl:variable name="lang.EDG"      select="'CHC'"/>
  <xsl:variable name="lang.Edge"      select="'Chance'"/>
  <xsl:variable name="lang.Electricity"    select="'Électricité'"/>
  <xsl:variable name="lang.Enchanter"    select="'Enchanteur'"/>
  <xsl:variable name="lang.Enemies"    select="'Ennemis'"/>
  <xsl:variable name="lang.Enhancements"  select="'Améliorations'"/>
  <xsl:variable name="lang.Entries"    select="'Entrées'"/>
  <xsl:variable name="lang.Equipped"    select="'Équipé'"/>
  <xsl:variable name="lang.ESS"      select="'ESS'"/>
  <xsl:variable name="lang.Essence"    select="'Essence'"/>
  <xsl:variable name="lang.Expenses"    select="'Dépenses'"/>
  <xsl:variable name="lang.Explorer"    select="'Explorateur'"/>
  <xsl:variable name="lang.Eyes"      select="'Yeux'"/>
  <xsl:variable name="lang.Falling"    select="'Chute'"/>
  <xsl:variable name="lang.Fatigue"      select="'Fatigue'"/>
  <xsl:variable name="lang.Fettered"      select="'Enchaîné'"/>
  <xsl:variable name="lang.Fire"    select="'Feu'"/>
  <xsl:variable name="lang.Firewall"    select="'Firewall'"/>
  <xsl:variable name="lang.Fly"      select="'Mouche'"/>
  <xsl:variable name="lang.Foci"      select="'Focus'"/>
  <xsl:variable name="lang.FWL"      select="'FWL'"/>
  <xsl:variable name="lang.Force"      select="'Force'"/>
  <xsl:variable name="lang.FV"      select="'VT'"/>
  <xsl:variable name="lang.Gear"      select="'Équipement'"/>
  <xsl:variable name="lang.Grade"      select="'Qualité'"/>
  <xsl:variable name="lang.Hair"      select="'Cheveux'"/>
  <xsl:variable name="lang.Handling"    select="'Manipulation'"/>
  <xsl:variable name="lang.Health"    select="'Santé'"/>
  <xsl:variable name="lang.Heavy"    select="'Lourd'"/>
  <xsl:variable name="lang.Height"    select="'Taille'"/>
  <xsl:variable name="lang.hit"      select="'Succès'"/>
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
  <xsl:variable name="lang.Instantaneous"  select="'Instantané'"/>
  <xsl:variable name="lang.Karma"      select="'Karma'"/>
  <xsl:variable name="lang.L"        select="'L'"/>
  <xsl:variable name="lang.Level"      select="'Niveau'"/>
  <xsl:variable name="lang.Lifestyle"    select="'Niveaux de vie'"/>
  <xsl:variable name="lang.Limit"      select="'Limite'"/>
  <xsl:variable name="lang.Limits"    select="'Limites'"/>
  <xsl:variable name="lang.Loaded"    select="'Chargé'"/>
  <xsl:variable name="lang.Location"    select="'Emplacement'"/>
  <xsl:variable name="lang.LOG"      select="'LOG'"/>
  <xsl:variable name="lang.Logic"      select="'Logique'"/>
  <xsl:variable name="lang.Loyalty"    select="'Loyauté'"/>
  <xsl:variable name="lang.M"        select="'M'"/>
  <xsl:variable name="lang.MAG"      select="'MAG'"/>
  <xsl:variable name="lang.Magic"      select="'Magie'"/>
  <xsl:variable name="lang.Magician"      select="'Magicien'"/>
  <xsl:variable name="lang.Mana"    select="'Mana'"/>
  <xsl:variable name="lang.Maneuvers"    select="'Manœuvres'"/>
  <xsl:variable name="lang.Manipulation"  select="'Manipulation'"/>
  <xsl:variable name="lang.Manual"    select="'Manuel'"/>
  <xsl:variable name="lang.Memory"    select="'Mémoire'"/>
  <xsl:variable name="lang.Mental"    select="'Mentale'"/>
  <xsl:variable name="lang.Metamagics"  select="'Métamagies'"/>
  <xsl:variable name="lang.Metatype"    select="'Métatype'"/>
  <xsl:variable name="lang.Mod"      select="'Mod'"/>
  <xsl:variable name="lang.Mode"      select="'Mode'"/>
  <xsl:variable name="lang.Model"      select="'Modèle'"/>
  <xsl:variable name="lang.Modifications"  select="'Modificateurs'"/>
  <xsl:variable name="lang.Month"      select="'Mois'"/>
  <xsl:variable name="lang.Months"    select="'Mois'"/>
  <xsl:variable name="lang.Mount"    select="'Monter'"/>
  <xsl:variable name="lang.Movement"    select="'Mouvement'"/>
  <xsl:variable name="lang.Mugshot"    select="'Portrait'"/>
  <xsl:variable name="lang.Name"      select="'Nom'"/>
  <xsl:variable name="lang.Native"    select="'Maternelle'"/>
  <xsl:variable name="lang.Negative"    select="'Négatif'"/>
  <xsl:variable name="lang.No"      select="'Non'"/>
  <xsl:variable name="lang.None"      select="'Aucun'"/>
  <xsl:variable name="lang.Notes"      select="'Remarques'"/>
  <xsl:variable name="lang.Notoriety"    select="'Rumeur'"/>
  <xsl:variable name="lang.Nuyen"      select="'Nuyen'"/>
  <xsl:variable name="lang.Other"      select="'Autre'"/>
  <xsl:variable name="lang.Overflow"      select="'Déborder'"/>
  <xsl:variable name="lang.OVR"      select="'SUR&#160;'"/>
  <xsl:variable name="lang.Pathogen"    select="'Pathogènes'"/>
  <xsl:variable name="lang.Permanent"    select="'Permanent'"/>
  <xsl:variable name="lang.Persona"    select="'Persona'"/>
  <xsl:variable name="lang.Pets"      select="'Animaux de compagnie'"/>
  <xsl:variable name="lang.Physical"    select="'Physique'"/>
  <xsl:variable name="lang.Physiological"  select="'Physiologique'"/>
  <xsl:variable name="lang.Pilot"      select="'Pilote'"/>
  <xsl:variable name="lang.Player"    select="'Joueur'"/>
  <xsl:variable name="lang.Points"    select="'Points'"/>
  <xsl:variable name="lang.Pool"      select="'Réserve'"/>
  <xsl:variable name="lang.Positive"    select="'Positif'"/>
  <xsl:variable name="lang.Power"      select="'Puissance'"/>
  <xsl:variable name="lang.Powers"    select="'Pouvoirs'"/>
  <xsl:variable name="lang.Priorities"  select="'Priorités'"/>
  <xsl:variable name="lang.Processor"    select="'Processeur'"/>
  <xsl:variable name="lang.Program"    select="'Programme'"/>
  <xsl:variable name="lang.Programs"    select="'Programmes'"/>
  <xsl:variable name="lang.Psychological"  select="'Psychologique'"/>
  <xsl:variable name="lang.Qty"      select="'Qté'"/>
  <xsl:variable name="lang.Quality"    select="'Trait'"/>
  <xsl:variable name="lang.Qualities"    select="'Traits'"/>
  <xsl:variable name="lang.Radiation"      select="'Radiation'"/>
  <xsl:variable name="lang.Range"      select="'Portée'"/>
  <xsl:variable name="lang.Rating"    select="'Puissance'"/>
  <xsl:variable name="lang.RC"      select="'CR'"/>
  <xsl:variable name="lang.Reaction"    select="'Réaction'"/>
  <xsl:variable name="lang.REA"      select="'RÉA'"/>
  <xsl:variable name="lang.Reach"      select="'Atteindre'"/>
  <xsl:variable name="lang.Reason"    select="'Raison'"/>
  <xsl:variable name="lang.Registered"  select="'Enregistré'"/>
  <xsl:variable name="lang.Requires"    select="'Requis'"/>
  <xsl:variable name="lang.RES"      select="'RES'"/>
  <xsl:variable name="lang.Resistance"    select="'Résistance'"/>
  <xsl:variable name="lang.Resistances"    select="'Résistances'"/>
  <xsl:variable name="lang.Resonance"    select="'Résonance'"/>
  <xsl:variable name="lang.Resources"    select="'Ressources'"/>
  <xsl:variable name="lang.Rigger"    select="'Interfacé'"/>
  <xsl:variable name="lang.Rtg"      select="'Ind'"/>
  <xsl:variable name="lang.Run"      select="'Courir'"/>
  <xsl:variable name="lang.S"        select="'F'"/>
  <xsl:variable name="lang.Seats"      select="'Places'"/>
  <xsl:variable name="lang.Self"      select="'Soi'"/>
  <xsl:variable name="lang.Services"    select="'Services'"/>
  <xsl:variable name="lang.Sensor"    select="'Senseur'"/>
  <xsl:variable name="lang.Sex"      select="'Sexe'"/>
  <xsl:variable name="lang.Show"      select="'Montrer: '"/>
  <xsl:variable name="lang.Skill"      select="'Compétence'"/>
  <xsl:variable name="lang.Skills"    select="'Compétences'"/>
  <xsl:variable name="lang.Skin"      select="'Peau'"/>
  <xsl:variable name="lang.Sleaze"    select="'Corruption'"/>
  <xsl:variable name="lang.SLZ"      select="'SLZ'"/>
  <xsl:variable name="lang.Social"    select="'Sociale'"/>
  <xsl:variable name="lang.Sonic"      select="'Sonique'"/>
  <xsl:variable name="lang.Source"    select="'Source'"/>
  <xsl:variable name="lang.Special"    select="'Spécial'"/>
  <xsl:variable name="lang.Speed"      select="'Vitesse'"/>
  <xsl:variable name="lang.Spell"      select="'Sort'"/>
  <xsl:variable name="lang.Spells"    select="'Sorts'"/>
  <xsl:variable name="lang.Spirit"    select="'Esprit'"/>
  <xsl:variable name="lang.Spirits"    select="'Esprits'"/>
  <xsl:variable name="lang.Sprite"    select="'Sprite'"/>
  <xsl:variable name="lang.Sprites"    select="'Sprites'"/>
  <xsl:variable name="lang.Standard"    select="'Standard'"/>
  <xsl:variable name="lang.Stream"    select="'Grille'"/>
  <xsl:variable name="lang.STR"      select="'FOR'"/>
  <xsl:variable name="lang.Strength"    select="'Force'"/>
  <xsl:variable name="lang.Stun"      select="'Étourdi'"/>
  <xsl:variable name="lang.Submersion"  select="'Submersion'"/>
  <xsl:variable name="lang.Sustained"    select="'Maintenu'"/>
  <xsl:variable name="lang.Swim"      select="'Nager'"/>
  <xsl:variable name="lang.Target"    select="'Cible'"/>
  <xsl:variable name="lang.Tasks"    select="'Tasks'"/>
  <xsl:variable name="lang.Total"      select="'Total'"/>
  <xsl:variable name="lang.Touch"      select="'Toucher'"/>
  <xsl:variable name="lang.Toxin"      select="'Toxines'"/>
  <xsl:variable name="lang.Tradition"    select="'Tradition'"/>
  <xsl:variable name="lang.Type"      select="'Type'"/>
  <xsl:variable name="lang.Unbound"    select="'Non lié'"/>
  <xsl:variable name="lang.Unknown"    select="'Inconnu'"/>
  <xsl:variable name="lang.Unregistered"  select="'Non enregistré'"/>
  <xsl:variable name="lang.Under"      select="'Sous'"/>
  <xsl:variable name="lang.Vehicle"    select="'Véhicule'"/>
  <xsl:variable name="lang.Vehicles"    select="'Véhicules'"/>
  <xsl:variable name="lang.VR"      select="'RV'"/>
  <xsl:variable name="lang.W"        select="'V'"/>
  <xsl:variable name="lang.Walk"      select="'Marche'"/>
  <xsl:variable name="lang.Weaknesses"    select="'Faiblesses'"/>
  <xsl:variable name="lang.Weapon"    select="'Arme'"/>
  <xsl:variable name="lang.Weapons"    select="'Armes'"/>
  <xsl:variable name="lang.Week"      select="'Semaine'"/>
  <xsl:variable name="lang.Weeks"      select="'Semaines'"/>
  <xsl:variable name="lang.Weight"    select="'Poids'"/>
  <xsl:variable name="lang.WIL"      select="'VOL'"/>
  <xsl:variable name="lang.Willpower"    select="'Volonté'"/>
  <xsl:variable name="lang.with"      select="'avec'"/>
  <xsl:variable name="lang.Yes"      select="'Oui'"/>

  <!-- multiple word phrases / composite words -->
  <xsl:variable name="lang.ActiveSkills"    select="'Compétences actives'"/>
  <xsl:variable name="lang.AdeptPowers"    select="concat('Pouvoirs d',$APOS,'adeptes')"/>
  <xsl:variable name="lang.AIandAdvanced"    select="'IA Programmes et Programmes Avancés'"/>
  <xsl:variable name="lang.AlreadyAddicted"  select="'Déjà adonné'"/>
  <xsl:variable name="lang.ArmorValue"    select="concat('Valeur d',$APOS,'armure')"/>
  <xsl:variable name="lang.AspectedMagician"    select="'Magicien spécialisé'"/>
  <xsl:variable name="lang.AstralInitiative"  select="'Initiative astrale'"/>
  <xsl:variable name="lang.CombatSkill"    select="'Compétences de combat'"/>
  <xsl:variable name="lang.ComplexForm"    select="'Forme complexe'"/>
  <xsl:variable name="lang.ComplexForms"    select="'Formes complexes'"/>
  <xsl:variable name="lang.ConditionMonitor"  select="'Moniteur de conditions'"/>
  <xsl:variable name="lang.ContactList"    select="'Liste de contacts'"/>
  <xsl:variable name="lang.CoreTrack"  select="'Core Damage Track'"/>
  <xsl:variable name="lang.CritterPower"    select="'Pouvoir de créature'"/>
  <xsl:variable name="lang.CritterPowers"    select="'Pouvoirs de créature'"/>
  <xsl:variable name="lang.CurrentEdge"    select="'Points de chance courants'"/>
  <xsl:variable name="lang.CurrentForm"    select="'Forme courante'"/>
  <xsl:variable name="lang.DamageType"  select="'Type de dommage'"/>
  <xsl:variable name="lang.DataProc"      select="'Trait. données'"/>
  <xsl:variable name="lang.DataProcessing"  select="'Traitement de données'"/>
  <xsl:variable name="lang.DecreaseAttribute"    select="'Decrease Attribute'"/>
  <xsl:variable name="lang.DerivedAttributes"  select="'Attributs dérivés'"/>
  <xsl:variable name="lang.DeviceRating"    select="'Évaluation'"/>
  <xsl:variable name="lang.FadingValue"    select="'Technodrain'"/>
  <xsl:variable name="lang.HobbiesVice"    select="'Loisirs/Vice'"/>
  <xsl:variable name="lang.IDcredsticks"    select="'Identité et créditubes'"/>
  <xsl:variable name="lang.InitiateGrade"    select="concat('Grade d',$APOS,'initiation')"/>
  <xsl:variable name="lang.InitiationNotes"   select="concat('Remarques d',$APOS,'initiation')"/>
  <xsl:variable name="lang.JudgeIntentions"  select="'Jauger les intentions'"/>
  <xsl:variable name="lang.KnowledgeSkills"  select="'Compétences de connaissance'"/>
  <xsl:variable name="lang.LiftCarry"      select="'Soulever / transporter'"/>
  <xsl:variable name="lang.LineofSight"    select="'Ligne de mire'"/>
  <xsl:variable name="lang.LinkedSIN"      select="'SIN lié'"/>
  <xsl:variable name="lang.MartialArt"    select="'Art martial'"/>
  <xsl:variable name="lang.MartialArts"    select="'Arts martiaux'"/>
  <xsl:variable name="lang.MatrixAR"      select="'Init. RA'"/>
  <xsl:variable name="lang.MatrixCold"    select="'Init. Cold SIM'"/>
  <xsl:variable name="lang.MatrixDevices"    select="'Equipement matriciel'"/>
  <xsl:variable name="lang.MatrixHot"      select="'Init. Hot SIM'"/>
  <xsl:variable name="lang.MatrixTrack"    select="'Moniteur de condition matricielle'"/>
  <xsl:variable name="lang.MeleeWeapons"    select="'Armes de corps à corps'"/>
  <xsl:variable name="lang.MentalAttributes"  select="'Attributs mentaux'"/>
  <xsl:variable name="lang.MysticAdept"    select="'Adepte mystique'"/>
  <xsl:variable name="lang.NotAddictedYet"  select="'Pas encore accro'"/>
  <xsl:variable name="lang.Nothing2Show4Devices"    select="'Aucun appareil à répertorier'"/>
  <xsl:variable name="lang.Nothing2Show4Notes"    select="'Pas de notes à la liste'"/>
  <xsl:variable name="lang.Nothing2Show4SpiritsSprites"    select="'Aucun Esprits/Sprites à la liste'"/>
  <xsl:variable name="lang.Nothing2Show4Vehicles"    select="'Aucun véhicule à la liste'"/>
  <xsl:variable name="lang.OptionalPowers"    select="'Pouvoirs optionels'"/>
  <xsl:variable name="lang.OtherArmor"      select="'Autre armure'"/>
  <xsl:variable name="lang.OtherMugshots"    select="'Autres portraits'"/>
  <xsl:variable name="lang.PageBreak"      select="'Saut de page : '"/>
  <xsl:variable name="lang.PersonalData"    select="'Données personnelles'"/>
  <xsl:variable name="lang.PersonalLife"    select="'Vie privée'"/>
  <xsl:variable name="lang.PhysicalAttributes"  select="'Attributs physiques'"/>
  <xsl:variable name="lang.PhysicalNaturalRecovery"  select="'Guérison naturelle (1 jour)'"/>
  <xsl:variable name="lang.PhysicalTrack"  select="'Moniteur physique'"/>
  <xsl:variable name="lang.PreferredPayment"    select="'Paiement préféré'"/>
  <xsl:variable name="lang.PrimaryArm"    select="'Main directrice'"/>
  <xsl:variable name="lang.PublicAwareness"  select="'Renommée'"/>
  <xsl:variable name="lang.RangedWeapons"    select="'Armes à distance'"/>
  <xsl:variable name="lang.RemainingAvailable"  select="'Disponible'"/>
  <xsl:variable name="lang.ResistDrain"    select="'Résister au drain avec'"/>
  <xsl:variable name="lang.ResistFading"    select="'Résister au technodrain avec'"/>
  <xsl:variable name="lang.RiggerInitiative"  select="'Init. interfacé'"/>
  <xsl:variable name="lang.SelectedGear"    select="'Équipement sélectionné'"/>
  <xsl:variable name="lang.SkillGroup"    select="'Groupe de compétences'"/>
  <xsl:variable name="lang.SkillGroups"    select="'Groupes de compétences'"/>
  <xsl:variable name="lang.SpecialAttributes"  select="'Attributs spéciaux'"/>
  <xsl:variable name="lang.StreetCred"    select="'Crédibilité'"/>
  <xsl:variable name="lang.StreetName"    select="'Nom de rue'"/>
  <xsl:variable name="lang.StunNaturalRecovery"  select="'Guérison naturelle (1 heure)'"/>
  <xsl:variable name="lang.StunTrack"    select="'Moniteur de étourdissant'"/>
  <xsl:variable name="lang.SubmersionGrade"  select="'Degré de submersion'"/>
  <xsl:variable name="lang.SubmersionNotes"  select="'Notes de submersion'"/>
  <xsl:variable name="lang.ToggleColors"  select="'Basculer les couleurs'"/>
  <xsl:variable name="lang.TotalArmor"  select="'Total des meilleurs armures et accessoires équipés'"/>
  <xsl:variable name="lang.ToxinsAndPathogens"  select="'Toxines et pathogènes'"/>
  <xsl:variable name="lang.UnnamedCharacter"  select="'Personnage sans nom'"/>
  <xsl:variable name="lang.VehicleBody"    select="'Structure du véhicule'"/>
  <xsl:variable name="lang.VehicleCost"    select="'Coût du véhicule'"/>

  <!-- "limits" list -->
  <xsl:variable name="lang.AstralLimit"    select="'Limite astrale'"/>
  <xsl:variable name="lang.MentalLimit"    select="'Limite mentale'"/>
  <xsl:variable name="lang.PhysicalLimit"    select="'Limite physique'"/>
  <xsl:variable name="lang.SocialLimit"    select="'Limite sociale'"/>

  <!-- spell types list -->
  <xsl:variable name="lang.CombatSpells"    select="'Sorts de combat'"/>
  <xsl:variable name="lang.DetectionSpells"  select="'Sorts de détection'"/>
  <xsl:variable name="lang.Enchantments"    select="'Enchantements'"/>
  <xsl:variable name="lang.HealthSpells"     select="'Sorts de santé'"/>
  <xsl:variable name="lang.IllusionSpells"   select="concat('Sorts d',$APOS,'illusion')"/>
  <xsl:variable name="lang.ManipulationSpells" select="'Sorts de manipulation'"/>
  <xsl:variable name="lang.Rituals"      select="'Rituels'"/>

  <!-- test values -->
  <xsl:variable name="lang.tstDamage1"  select="'P'"/>
  <xsl:variable name="lang.tstDamage2"  select="'E'"/>
  <xsl:variable name="lang.tstDuration1"  select="'I'"/>
  <xsl:variable name="lang.tstDuration2"  select="'P'"/>
  <xsl:variable name="lang.tstDuration3"  select="'M'"/>
  <xsl:variable name="lang.tstRange1"    select="'T'"/>
  <xsl:variable name="lang.tstRange2"    select="'CDV'"/>
  <xsl:variable name="lang.tstRange3"    select="'CDV(Z)'"/>
  <xsl:variable name="lang.tstRange4"    select="'CDV (Z)'"/>
  <xsl:variable name="lang.tstRange5"    select="'S'"/>
  <xsl:variable name="lang.tstRange6"    select="'S(Z)'"/>
  <xsl:variable name="lang.tstRange7"    select="'S (Z)'"/>
  <xsl:variable name="lang.tstRange8"    select="'T(Z)'"/>
  <xsl:variable name="lang.tstRange9"    select="'T (Z)'"/>
  <xsl:variable name="lang.tstRange10"    select="'Special'"/>

  <!-- miscellaneous signs and symbols -->
    <!-- currency symbol -->
  <xsl:variable name="lang.NuyenSymbol"  select="'&#165;'"/>
    <!-- diacrtic marks: decimal mark and grouping separator -->
  <xsl:variable name="lang.marks"      select="',.'"/>
    <!-- single quote for use with d' and l' -->
  <xsl:variable name="APOS">'</xsl:variable>
</xsl:stylesheet>
