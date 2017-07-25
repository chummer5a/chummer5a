<?xml version="1.0" encoding="UTF-8" ?>
<!-- Isolate French (fr) locale literals -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="lang"  select="'fr'"/>
  <xsl:variable name="locale"  select="'fr'"/>

  <!-- individual words -->
  <xsl:variable name="lang.Acceleration"  select="'Accélération'"/>
  <xsl:variable name="lang.Accel"      select="'Accél'"/>
  <xsl:variable name="lang.Accessories"  select="'Accessoires'"/>
  <xsl:variable name="lang.Accuracy"    select="'Précision'"/>
  <xsl:variable name="lang.Age"      select="'Âge'"/>
  <xsl:variable name="lang.Agility"    select="'Agilité'"/>
  <xsl:variable name="lang.AGI"      select="'AGI'"/>
  <xsl:variable name="lang.AI"      select="'AI'"/>
  <xsl:variable name="lang.Alias"      select="'Pseudonyme'"/>
  <xsl:variable name="lang.Ammo"      select="'Munitions'"/>
  <xsl:variable name="lang.Amount"    select="'Somme'"/>
  <xsl:variable name="lang.AP"      select="'PA'"/>
  <xsl:variable name="lang.Applicable"  select="'En vigueur'"/>
  <xsl:variable name="lang.AR"      select="'RA'"/>
  <xsl:variable name="lang.Archetype"    select="'Archétype'"/>
  <xsl:variable name="lang.Area"      select="'Zone'"/>
  <xsl:variable name="lang.Armor"      select="'Armure'"/>
  <xsl:variable name="lang.Arts"      select="'Arts'"/>
  <xsl:variable name="lang.as"      select="'comme'"/>
  <xsl:variable name="lang.Astral"    select="'Astral'"/>
  <xsl:variable name="lang.Attack"    select="'Attaque'"/>
  <xsl:variable name="lang.ATT"      select="'ATT'"/>
  <xsl:variable name="lang.Attribute"    select="'Attribut'"/>
  <xsl:variable name="lang.Attributes"  select="'Attributs'"/>
  <xsl:variable name="lang.Available"    select="'Disponible'"/>
  <xsl:variable name="lang.Background"  select="'Background'"/>
  <xsl:variable name="lang.Base"      select="'Base'"/>
  <xsl:variable name="lang.Bioware"    select="'Bioware'"/>
  <xsl:variable name="lang.Body"      select="'Corps'"/>
  <xsl:variable name="lang.BOD"      select="'CON'"/>
  <xsl:variable name="lang.Bonus"      select="'Bonus'"/>
  <xsl:variable name="lang.Bound"      select="'Lié'"/>
  <xsl:variable name="lang.Calendar"    select="'Calendrier'"/>
  <xsl:variable name="lang.Career"    select="'Carrière'"/>
  <xsl:variable name="lang.Category"    select="'Catégorie'"/>
  <xsl:variable name="lang.Charisma"    select="'Charisme'"/>
  <xsl:variable name="lang.CHA"      select="'CHA'"/>
  <xsl:variable name="lang.CM"      select="'MC'"/>
  <xsl:variable name="lang.Combat"    select="'Combat'"/>
  <xsl:variable name="lang.Commlink"    select="'Commlink'"/>
  <xsl:variable name="lang.Composure"    select="'Sang-Froid'"/>
  <xsl:variable name="lang.Concept"    select="'Concept'"/>
  <xsl:variable name="lang.Connection"  select="'Connexion'"/>
  <xsl:variable name="lang.Contact"    select="'Contact'"/>
  <xsl:variable name="lang.Contacts"    select="'Contacts'"/>
  <xsl:variable name="lang.Cost"      select="'Coût'"/>
  <xsl:variable name="lang.Critter"    select="'Créature'"/>
  <xsl:variable name="lang.Critters"    select="'Créatures'"/>
  <xsl:variable name="lang.Cyberware"    select="'Cyberware'"/>
  <xsl:variable name="lang.Damage"    select="'Dégât'"/>
  <xsl:variable name="lang.Data"      select="'Donnée'"/>
  <xsl:variable name="lang.Date"      select="'Date'"/>
  <xsl:variable name="lang.Dead"      select="'Mort'"/>
  <xsl:variable name="lang.Description"  select="'Description'"/>
  <xsl:variable name="lang.Detection"    select="'Détection'"/>
  <xsl:variable name="lang.Device"    select="'Appareil'"/>
  <xsl:variable name="lang.Devices"    select="'Appareils'"/>
  <xsl:variable name="lang.Down"      select="'A Terre'"/>
  <xsl:variable name="lang.Drain"      select="'Drain'"/>
  <xsl:variable name="lang.Drone"      select="'Drone'"/>
  <xsl:variable name="lang.Duration"    select="'Durée'"/>
  <xsl:variable name="lang.DV"      select="'VD'"/>
  <xsl:variable name="lang.E"        select="'E'"/>
  <xsl:variable name="lang.Echoes"    select="'Échos'"/>
  <xsl:variable name="lang.Edge"      select="'Chance'"/>
  <xsl:variable name="lang.EDG"      select="'CHN'"/>
  <xsl:variable name="lang.Enemies"    select="'Ennemis'"/>
  <xsl:variable name="lang.Enhancements"  select="'Améliorations'"/>
  <xsl:variable name="lang.Entries"    select="'Entrées'"/>
  <xsl:variable name="lang.Equipped"    select="'Équipé'"/>
  <xsl:variable name="lang.Essence"    select="'Essence'"/>
  <xsl:variable name="lang.ESS"      select="'ESS'"/>
  <xsl:variable name="lang.Expenses"    select="'Dépenses'"/>
  <xsl:variable name="lang.Eyes"      select="'Yeux'"/>
  <xsl:variable name="lang.Firewall"    select="'Firewall'"/>
  <xsl:variable name="lang.Fly"      select="'Mouche'"/>
  <xsl:variable name="lang.FWL"      select="'FWL'"/>
  <xsl:variable name="lang.Force"      select="'Force'"/>
  <xsl:variable name="lang.FV"      select="'VT'"/>
  <xsl:variable name="lang.Gear"      select="'Équipement'"/>
  <xsl:variable name="lang.Grade"      select="'Qualité'"/>
  <xsl:variable name="lang.Hair"      select="'Cheveux'"/>
  <xsl:variable name="lang.Handling"    select="'Manipulation'"/>
  <xsl:variable name="lang.Health"    select="'Santé'"/>
  <xsl:variable name="lang.Height"    select="'Taille'"/>
  <xsl:variable name="lang.hit"      select="'Succès'"/>
  <xsl:variable name="lang.Illusion"    select="'Illusion'"/>
  <xsl:variable name="lang.Implant"    select="'Implant'"/>
  <xsl:variable name="lang.Info"      select="'Info'"/>
  <xsl:variable name="lang.Initiation"  select="'Initiation'"/>
  <xsl:variable name="lang.Initiative"  select="'Initiative'"/>
  <xsl:variable name="lang.Init"      select="'Init'"/>
  <xsl:variable name="lang.Intentions"  select="'Intentions'"/>
  <xsl:variable name="lang.Intuition"    select="'Intuition'"/>
  <xsl:variable name="lang.INT"      select="'INT'"/>
  <xsl:variable name="lang.Instantaneous"  select="'Instantané'"/>
  <xsl:variable name="lang.Karma"      select="'Karma'"/>
  <xsl:variable name="lang.L"        select="'L'"/>
  <xsl:variable name="lang.Level"      select="'Niveau'"/>
  <xsl:variable name="lang.Lifestyle"    select="'Niveaux de Vie'"/>
  <xsl:variable name="lang.Limit"      select="'Limite'"/>
  <xsl:variable name="lang.Limits"    select="'Limites'"/>
  <xsl:variable name="lang.Loaded"    select="'Chargé'"/>
  <xsl:variable name="lang.Location"    select="'Emplacement'"/>
  <xsl:variable name="lang.Logic"      select="'Logique'"/>
  <xsl:variable name="lang.LOG"      select="'LOG'"/>
  <xsl:variable name="lang.Loyalty"    select="'Loyauté'"/>
  <xsl:variable name="lang.M"        select="'M'"/>
  <xsl:variable name="lang.Magic"      select="'Magie'"/>
  <xsl:variable name="lang.MAG"      select="'MAG'"/>
  <xsl:variable name="lang.Maneuvers"    select="'Manœuvres'"/>
  <xsl:variable name="lang.Manipulation"  select="'Manipulation'"/>
  <xsl:variable name="lang.Memory"    select="'Mémoire'"/>
  <xsl:variable name="lang.Mental"    select="'Mentale'"/>
  <xsl:variable name="lang.Metamagics"  select="'Métamagies'"/>
  <xsl:variable name="lang.Metatype"    select="'Métatype'"/>
  <xsl:variable name="lang.Mod"      select="'Mod'"/>
  <xsl:variable name="lang.Mode"      select="'Mode'"/>
  <xsl:variable name="lang.Model"      select="'Modèle'"/>
  <xsl:variable name="lang.Modifications"  select="'Modificateurs'"/>
  <xsl:variable name="lang.Modifiers"    select="'Modificateurs'"/>
  <xsl:variable name="lang.Month"      select="'Mois'"/>
  <xsl:variable name="lang.Months"    select="'Mois'"/>
  <xsl:variable name="lang.Movement"    select="'Mouvement'"/>
  <xsl:variable name="lang.Name"      select="'Nom'"/>
  <xsl:variable name="lang.Native"    select="'Maternelle'"/>
  <xsl:variable name="lang.No"      select="'Non'"/>
  <xsl:variable name="lang.Notes"      select="'Remarques'"/>
  <xsl:variable name="lang.Notoriety"    select="'Rumeur'"/>
  <xsl:variable name="lang.Nuyen"      select="'Nuyen'"/>
  <xsl:variable name="lang.Other"      select="'Autre'"/>
  <xsl:variable name="lang.OVR"      select="'OVR&#160;'"/>
  <xsl:variable name="lang.Permanent"    select="'Permanent'"/>
  <xsl:variable name="lang.Persona"    select="'Persona'"/>
  <xsl:variable name="lang.Physical"    select="'Physique'"/>
  <xsl:variable name="lang.Pilot"      select="'Pilote'"/>
  <xsl:variable name="lang.Player"    select="'Joueur'"/>
  <xsl:variable name="lang.Points"    select="'Points'"/>
  <xsl:variable name="lang.Pool"      select="'Réserve'"/>
  <xsl:variable name="lang.Power"      select="'Puissance'"/>
  <xsl:variable name="lang.Powers"    select="'Pouvoirs'"/>
  <xsl:variable name="lang.Priorities"  select="'Priorités'"/>
  <xsl:variable name="lang.Processor"    select="'Processeur'"/>
  <xsl:variable name="lang.Program"    select="'Programme'"/>
  <xsl:variable name="lang.Programs"    select="'Programmes'"/>
  <xsl:variable name="lang.Qty"      select="'Qté'"/>
  <xsl:variable name="lang.Quality"    select="'Trait'"/>
  <xsl:variable name="lang.Qualities"    select="'Traits'"/>
  <xsl:variable name="lang.Range"      select="'Portée'"/>
  <xsl:variable name="lang.Rating"    select="'Puissance'"/>
  <xsl:variable name="lang.RC"      select="'CR'"/>
  <xsl:variable name="lang.Reaction"    select="'Réaction'"/>
  <xsl:variable name="lang.REA"      select="'RÉA'"/>
  <xsl:variable name="lang.Reach"      select="'Atteindre'"/>
  <xsl:variable name="lang.Reason"    select="'Raison'"/>
  <xsl:variable name="lang.Registered"  select="'Enregistré'"/>
  <xsl:variable name="lang.Requires"    select="'Requis'"/>
  <xsl:variable name="lang.Resonance"    select="'Résonance'"/>
  <xsl:variable name="lang.RES"      select="'RES'"/>
  <xsl:variable name="lang.Resources"    select="'Ressources'"/>
  <xsl:variable name="lang.Rigger"    select="'Interfacé'"/>
  <xsl:variable name="lang.Rtg"      select="'Ind'"/>
  <xsl:variable name="lang.Run"      select="'Courir'"/>
  <xsl:variable name="lang.S"        select="'F'"/>
  <xsl:variable name="lang.Seats"      select="'Places'"/>
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
  <xsl:variable name="lang.Source"    select="'Source'"/>
  <xsl:variable name="lang.Special"    select="'Spécial'"/>
  <xsl:variable name="lang.Speed"      select="'Vitesse'"/>
  <xsl:variable name="lang.Spell"      select="'Sort'"/>
  <xsl:variable name="lang.Spells"    select="'Sorts'"/>
  <xsl:variable name="lang.Spirit"    select="'Esprit'"/>
  <xsl:variable name="lang.Spirits"    select="'Esprits'"/>
  <xsl:variable name="lang.Sprite"    select="'Sprite'"/>
  <xsl:variable name="lang.Sprites"    select="'Sprites'"/>
  <xsl:variable name="lang.Stream"    select="'Grille'"/>
  <xsl:variable name="lang.Strength"    select="'Force'"/>
  <xsl:variable name="lang.STR"      select="'FOR'"/>
  <xsl:variable name="lang.Stun"      select="'Étourdi'"/>
  <xsl:variable name="lang.Sustained"    select="'Maintenu'"/>
  <xsl:variable name="lang.Swim"      select="'Nager'"/>
  <xsl:variable name="lang.Target"    select="'Cible'"/>
  <xsl:variable name="lang.Total"      select="'Total'"/>
  <xsl:variable name="lang.Touch"      select="'Toucher'"/>
  <xsl:variable name="lang.Tradition"    select="'Tradition'"/>
  <xsl:variable name="lang.Track"      select="'Piste'"/>
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
  <xsl:variable name="lang.Weapon"    select="'Arme'"/>
  <xsl:variable name="lang.Weapons"    select="'Armes'"/>
  <xsl:variable name="lang.Week"      select="'Semaine'"/>
  <xsl:variable name="lang.Weight"    select="'Poids'"/>
  <xsl:variable name="lang.Willpower"    select="'Volonté'"/>
  <xsl:variable name="lang.WIL"      select="'VOL'"/>
  <xsl:variable name="lang.with"      select="'avec'"/>
  <xsl:variable name="lang.Yes"      select="'Oui'"/>

  <!-- multiple word phrases / composite words -->
  <xsl:variable name="lang.ActiveSkills"    select="'Compétences Actives'"/>
  <xsl:variable name="lang.AdeptPowers"    select="'Pouvoirs adeptes'"/>
  <xsl:variable name="lang.AIandAdvanced"    select="'IA Programmes et Programmes Avancés'"/>
  <xsl:variable name="lang.ArmorValue"    select="concat('Valeur d','$APOS','armure')"/>
  <xsl:variable name="lang.AstralInitiative"  select="'Initiative Astrale'"/>
  <xsl:variable name="lang.CombatSkill"    select="'Compétences de combat'"/>
  <xsl:variable name="lang.ComplexForm"    select="'Forme Complexe'"/>
  <xsl:variable name="lang.ComplexForms"    select="'Formes Complexes'"/>
  <xsl:variable name="lang.ConditionMonitor"  select="'Moniteur de conditions'"/>
  <xsl:variable name="lang.ContactList"    select="'Liste de contacts'"/>
  <xsl:variable name="lang.CritterPowers"    select="'Pouvoirs de Créature'"/>
  <xsl:variable name="lang.CurrentEdge"    select="'Points de chance courants'"/>
  <xsl:variable name="lang.DataProcessing"  select="'Traitement de Données'"/>
  <xsl:variable name="lang.DP"        select="'TD'"/>
  <xsl:variable name="lang.DataProc"      select="'Trait. Données'"/>
  <xsl:variable name="lang.DerivedAttributes"  select="'Attributs Dérivés'"/>
  <xsl:variable name="lang.DeviceRating"    select="'Évaluation'"/>
  <xsl:variable name="lang.FadingValue"    select="'Technodrain'"/>
  <xsl:variable name="lang.InitiateGrade"    select="'Initier la note'"/>
  <xsl:variable name="lang.JudgeIntentions"  select="'Jauger les intentions'"/>
  <xsl:variable name="lang.KnowledgeSkills"  select="'Compétences de Connaissance'"/>
  <xsl:variable name="lang.LiftCarry"      select="'Soulever/Transporter'"/>
  <xsl:variable name="lang.LineofSight"    select="'Ligne de mire'"/>
  <xsl:variable name="lang.LinkedSIN"      select="'SIN Lié'"/>
  <xsl:variable name="lang.MartialArt"    select="'Art Martial'"/>
  <xsl:variable name="lang.MartialArts"    select="'Arts Martiaux'"/>
  <xsl:variable name="lang.MatrixAR"      select="'RA Matricielle'"/>
  <xsl:variable name="lang.MatrixCold"    select="'Cold SIM'"/>
  <xsl:variable name="lang.MatrixHot"      select="'Hot SIM'"/>
  <xsl:variable name="lang.MeleeWeapons"    select="'Armes de corps á corps'"/>
  <xsl:variable name="lang.OtherMugshots"    select="'Autres portraits'"/>
  <xsl:variable name="lang.PageBreak"      select="'Saut de page: '"/>
  <xsl:variable name="lang.PersonalData"    select="'Données personnelles'"/>
  <xsl:variable name="lang.PhysicalTrack1"  select="'&#160;&#160;Moniteur de'"/>
  <xsl:variable name="lang.PhysicalTrack2"  select="'Condition Physique'"/>
  <xsl:variable name="lang.PrimaryArm"    select="'Bras primaire'"/>
  <xsl:variable name="lang.PublicAwareness"  select="'Renommée'"/>
  <xsl:variable name="lang.RangedWeapons"    select="'Armes á distance'"/>
  <xsl:variable name="lang.RemainingAvailable"  select="'Restant Disponible'"/>
  <xsl:variable name="lang.ResistDrain"    select="'Résister au drain avec'"/>
  <xsl:variable name="lang.ResistFading"    select="'Résister au technodrain avec'"/>
  <xsl:variable name="lang.RiggerInitiative"  select="'Init. interfacé'"/>
  <xsl:variable name="lang.SkillGroup"    select="'Groupe de compétence'"/>
  <xsl:variable name="lang.StreetCred"    select="'Crédibilité'"/>
  <xsl:variable name="lang.StreetName"    select="'Nom de rue'"/>
  <xsl:variable name="lang.StunTrack1"    select="'&#160;&#160;&#160;&#160;Moniteur de'"/>
  <xsl:variable name="lang.StunTrack2"    select="'Condition Étourdissant'"/>
  <xsl:variable name="lang.SubmersionGrade"  select="'Degré de submersion'"/>
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
  <xsl:variable name="lang.IllusionSpells"   select="concat('Sorts d','$APOS','illusion')"/>
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

  <!-- miscellaneous signs and symbols -->
    <!-- currency symbol -->
  <xsl:variable name="lang.NuyenSymbol"  select="'&#165;'"/>
    <!-- diacrtic marks: decimal mark and grouping separator -->
  <xsl:variable name="lang.marks"      select="',.'"/>
    <!-- single quote for use with d' and l' -->
  <xsl:variable name="APOS">'</xsl:variable>
</xsl:stylesheet>