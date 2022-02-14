<?xml version="1.0" encoding="utf-8" ?>
<!-- Isolate Portuguese (pt-br) locale literals -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="lang"  select="string('pt')" />
  <xsl:variable name="locale"  select="string('pt-br')" />

  <!-- individual words -->
  <xsl:variable name="lang.Acceleration"  select="string('Aceleração')" />
  <xsl:variable name="lang.Accel"      select="string('Acel')" />
  <xsl:variable name="lang.Accessories"  select="string('Acessórios')" />
  <xsl:variable name="lang.Accuracy"    select="string('Precisão')" />
  <xsl:variable name="lang.Acid"      select="string('Ácido')" />
  <xsl:variable name="lang.Action"      select="string('Ação')" />
  <xsl:variable name="lang.Addiction"  select="string('Vício')" />
  <xsl:variable name="lang.Adept"      select="string('Adepto')" />
  <xsl:variable name="lang.Age"      select="string('Idade')" />
  <xsl:variable name="lang.AGI"      select="string('AGI')" />
  <xsl:variable name="lang.Agility"    select="string('Agilidade')" />
  <xsl:variable name="lang.AI"      select="string('IA')" />
  <xsl:variable name="lang.Alias"      select="string('Pseudônimo')" />
  <xsl:variable name="lang.Ammo"      select="string('Munição')" />
  <xsl:variable name="lang.Amount"    select="string('Quantidade')" />
  <xsl:variable name="lang.AP"      select="string('PA')" />
  <xsl:variable name="lang.Applicable"  select="string('Aplicável')" />
  <xsl:variable name="lang.Apprentice"  select="string('Apprentiz')" />
  <xsl:variable name="lang.AR"      select="string('RA')" />
  <xsl:variable name="lang.Archetype"    select="string('Arquétipo')" />
  <xsl:variable name="lang.Area"      select="string('Área')" />
  <xsl:variable name="lang.Armor"      select="string('Armadura')" />
  <xsl:variable name="lang.Arts"      select="string('Artes')" />
  <xsl:variable name="lang.as"      select="string('como')" />
  <xsl:variable name="lang.ASDF"    select="string('A/S/D/F')" />
  <xsl:variable name="lang.Astral"    select="string('Astral')" />
  <xsl:variable name="lang.Attack"    select="string('Ataque')" />
  <xsl:variable name="lang.ATT"      select="string('ATQ')" />
  <xsl:variable name="lang.Attribute"    select="string('Atributo')" />
  <xsl:variable name="lang.Attributes"  select="string('Atributos')" />
  <xsl:variable name="lang.Available"    select="string('Disponível')" />
  <xsl:variable name="lang.Awakened"    select="string('Despertou')" />
  <xsl:variable name="lang.Aware"    select="string('Consciente')" />
  <xsl:variable name="lang.Background"  select="string('Antecedentes')" />
  <xsl:variable name="lang.Base"      select="string('Base')" />
  <xsl:variable name="lang.Bioware"    select="string('Biônico')" />
  <xsl:variable name="lang.BOD"      select="string('COR')" />
  <xsl:variable name="lang.Body"      select="string('Corpo')" />
  <xsl:variable name="lang.Bonus"      select="string('Bônus')" />
  <xsl:variable name="lang.Bound"      select="string('Vinculado')" />
  <xsl:variable name="lang.Calendar"    select="string('Calendário')" />
  <xsl:variable name="lang.Career"    select="string('Carreira')" />
  <xsl:variable name="lang.Category"    select="string('Categoria')" />
  <xsl:variable name="lang.CHA"      select="string('CAR')" />
  <xsl:variable name="lang.Charisma"    select="string('Carisma')" />
  <xsl:variable name="lang.CM"      select="string('MC')" />
  <xsl:variable name="lang.Cold"      select="string('Frio')" />
  <xsl:variable name="lang.Combat"    select="string('Combate')" />
  <xsl:variable name="lang.Commlink"    select="string('Com-Link')" />
  <xsl:variable name="lang.Composure"    select="string('Compostura')" />
  <xsl:variable name="lang.Concept"    select="string('Conceito')" />
  <xsl:variable name="lang.Connection"  select="string('Conexão')" />
  <xsl:variable name="lang.Contact"    select="string('Contato')" />
  <xsl:variable name="lang.ContactDrug"    select="string('Contato')" />
  <xsl:variable name="lang.Contacts"    select="string('Contatos')" />
  <xsl:variable name="lang.Cost"      select="string('Custo')" />
  <xsl:variable name="lang.Critter"    select="string('Criatura')" />
  <xsl:variable name="lang.Critters"    select="string('Criaturas')" />
  <xsl:variable name="lang.Cyberware"    select="string('Cibernético')" />
  <xsl:variable name="lang.Damage"    select="string('Dano')" />
  <xsl:variable name="lang.Data"      select="string('Dados')" />
  <xsl:variable name="lang.Date"      select="string('Data')" />
  <xsl:variable name="lang.Day"      select="string('Dia')" />
  <xsl:variable name="lang.Days"      select="string('Dias')" />
  <xsl:variable name="lang.Dead"      select="string('Morto')" />
  <xsl:variable name="lang.Defense"      select="string('Defesa')" />
  <xsl:variable name="lang.DEP"  select="string('PRO')" />
  <xsl:variable name="lang.Depth"  select="string('Profundidade')" />
  <xsl:variable name="lang.Description"  select="string('Descrição')" />
  <xsl:variable name="lang.Detection"    select="string('Detecção')" />
  <xsl:variable name="lang.Device"    select="string('Dispositivo')" />
  <xsl:variable name="lang.Devices"    select="string('Dispositivos')" />
  <xsl:variable name="lang.Direct"      select="string('Direto')" />
  <xsl:variable name="lang.Down"      select="string('Caído')" />
  <xsl:variable name="lang.DP"        select="string('PD')" />
  <xsl:variable name="lang.Drain"      select="string('Dreno')" />
  <xsl:variable name="lang.Drone"      select="string('Drone')" />
  <xsl:variable name="lang.Duration"    select="string('Duração')" />
  <xsl:variable name="lang.DV"      select="string('VD')" />
  <xsl:variable name="lang.E"        select="string('E')" />
  <xsl:variable name="lang.Echo"    select="string('Eco')" />
  <xsl:variable name="lang.Echoes"    select="string('Ecos')" />
  <xsl:variable name="lang.EDG"      select="string('TRU')" />
  <xsl:variable name="lang.Edge"      select="string('Trunfo')" />
  <xsl:variable name="lang.Electricity"    select="string('Eletricidade')" />
  <xsl:variable name="lang.Enchanter"    select="string('Enchanter')" />
  <xsl:variable name="lang.Enemies"    select="string('Inimigos')" />
  <xsl:variable name="lang.Enhancements"  select="string('Melhorias')" />
  <xsl:variable name="lang.Entries"    select="string('Entradas')" />
  <xsl:variable name="lang.Equipped"    select="string('Equipado')" />
  <xsl:variable name="lang.ESS"      select="string('ESS')" />
  <xsl:variable name="lang.Essence"    select="string('Essência')" />
  <xsl:variable name="lang.Expenses"    select="string('Despesas')" />
  <xsl:variable name="lang.Explorer"    select="string('Explorador')" />
  <xsl:variable name="lang.Eyes"      select="string('Olhos')" />
  <xsl:variable name="lang.Falling"    select="string('Queda')" />
  <xsl:variable name="lang.Fatigue"      select="string('Fadiga')" />
  <xsl:variable name="lang.Fettered"      select="string('Aprisionado')" />
  <xsl:variable name="lang.Fire"    select="string('Fogo')" />
  <xsl:variable name="lang.Firewall"    select="string('Firewall')" />
  <xsl:variable name="lang.Fly"      select="string('Voo')" />
  <xsl:variable name="lang.Foci"      select="string('Foco')" />
  <xsl:variable name="lang.FWL"      select="string('FWL')" />
  <xsl:variable name="lang.Force"      select="string('Poder')" />
  <xsl:variable name="lang.FV"      select="string('VE')" />
  <xsl:variable name="lang.Gear"      select="string('Equipamento')" />
  <xsl:variable name="lang.Gender"      select="string('Gênero')" />
  <xsl:variable name="lang.Grade"      select="string('Classe')" />
  <xsl:variable name="lang.Hair"      select="string('Cabelos')" />
  <xsl:variable name="lang.Handling"    select="string('Manejo')" />
  <xsl:variable name="lang.Health"    select="string('Saúde')" />
  <xsl:variable name="lang.Heavy"    select="string('Pesado')" />
  <xsl:variable name="lang.Height"    select="string('Altura')" />
  <xsl:variable name="lang.hit"      select="string('sucesso')" />
  <xsl:variable name="lang.Illusion"    select="string('Ilusão')" />
  <xsl:variable name="lang.Implant"    select="string('Implante')" />
  <xsl:variable name="lang.Indirect"      select="string('Indireto')" />
  <xsl:variable name="lang.Info"      select="string('Info')" />
  <xsl:variable name="lang.Ingestion"      select="string('Ingestão')" />
  <xsl:variable name="lang.Inhalation"      select="string('Inalação')" />
  <xsl:variable name="lang.Init"      select="string('Inic')" />
  <xsl:variable name="lang.Initiation"  select="string('Iniciação')" />
  <xsl:variable name="lang.Initiative"  select="string('Iniciativa')" />
  <xsl:variable name="lang.Injection"      select="string('Injeção')" />
  <xsl:variable name="lang.INT"      select="string('INT')" />
  <xsl:variable name="lang.Intentions"  select="string('Intenções')" />
  <xsl:variable name="lang.Intuition"    select="string('Intuição')" />
  <xsl:variable name="lang.Instantaneous"  select="string('Instantâneo')" />
  <xsl:variable name="lang.Karma"      select="string('Carma')" />
  <xsl:variable name="lang.L"        select="string('L')" />
  <xsl:variable name="lang.Level"      select="string('Nível')" />
  <xsl:variable name="lang.Lifestyle"    select="string('Estilo de Vida')" />
  <xsl:variable name="lang.Limit"      select="string('Limite')" />
  <xsl:variable name="lang.Limits"    select="string('Limites')" />
  <xsl:variable name="lang.Loaded"    select="string('Carregado')" />
  <xsl:variable name="lang.Location"    select="string('Localização')" />
  <xsl:variable name="lang.LOG"      select="string('LOG')" />
  <xsl:variable name="lang.Logic"      select="string('Lógica')" />
  <xsl:variable name="lang.Loyalty"    select="string('Lealdade')" />
  <xsl:variable name="lang.M"        select="string('M')" />
  <xsl:variable name="lang.MAG"      select="string('MAG')" />
  <xsl:variable name="lang.Magic"      select="string('Magia')" />
  <xsl:variable name="lang.Magician"      select="string('Mágico')" />
  <xsl:variable name="lang.Mana"    select="string('Mana')" />
  <xsl:variable name="lang.Maneuvers"    select="string('Manobras')" />
  <xsl:variable name="lang.Manipulation"  select="string('Manipulação')" />
  <xsl:variable name="lang.Manual"    select="string('Manual')" />
  <xsl:variable name="lang.Memory"    select="string('Memória')" />
  <xsl:variable name="lang.Mental"    select="string('Mental')" />
  <xsl:variable name="lang.Metamagics"  select="string('Metamágicas')" />
  <xsl:variable name="lang.Metatype"    select="string('Metatipo')" />
  <xsl:variable name="lang.Mod"      select="string('Mod')" />
  <xsl:variable name="lang.Mode"      select="string('Modo')" />
  <xsl:variable name="lang.Model"      select="string('Modelo')" />
  <xsl:variable name="lang.Modifications"  select="string('Modificações')" />
  <xsl:variable name="lang.Month"      select="string('Mês')" />
  <xsl:variable name="lang.Months"    select="string('Meses')" />
  <xsl:variable name="lang.Mount"    select="string('Montar')" />
  <xsl:variable name="lang.Movement"    select="string('Movimento')" />
  <xsl:variable name="lang.Mugshot"    select="string('Retrato')" />
  <xsl:variable name="lang.Name"      select="string('Nome')" />
  <xsl:variable name="lang.Native"    select="string('Nativo')" />
  <xsl:variable name="lang.Negative"    select="string('Negativo')" />
  <xsl:variable name="lang.No"      select="string('Não')" />
  <xsl:variable name="lang.None"      select="string('None')" />
  <xsl:variable name="lang.Notes"      select="string('Observações')" />
  <xsl:variable name="lang.Notoriety"    select="string('Notoriedade')" />
  <xsl:variable name="lang.Nuyen"      select="string('Neoiene')" />
  <xsl:variable name="lang.Other"      select="string('Outros')" />
  <xsl:variable name="lang.Overflow"      select="string('Exceder')" />
  <xsl:variable name="lang.OVR"      select="string('Exced')" />
  <xsl:variable name="lang.Pathogen"    select="string('Patogênico')" />
  <xsl:variable name="lang.Permanent"    select="string('Permanente')" />
  <xsl:variable name="lang.Persona"    select="string('Persona')" />
  <xsl:variable name="lang.Pets"      select="string('Mascotes')" />
  <xsl:variable name="lang.Physical"    select="string('Físico')" />
  <xsl:variable name="lang.Physiological"  select="string('Físico')" />
  <xsl:variable name="lang.Pilot"      select="string('Piloto')" />
  <xsl:variable name="lang.Player"    select="string('Jogador')" />
  <xsl:variable name="lang.Points"    select="string('Pontos')" />
  <xsl:variable name="lang.Pool"      select="string('Pilha')" />
  <xsl:variable name="lang.Positive"    select="string('Positivo')" />
  <xsl:variable name="lang.Power"      select="string('Poder')" />
  <xsl:variable name="lang.Powers"    select="string('Poderes')" />
  <xsl:variable name="lang.Priorities"  select="string('Prioridades')" />
  <xsl:variable name="lang.Processor"    select="string('Processador')" />
  <xsl:variable name="lang.Program"    select="string('Programa')" />
  <xsl:variable name="lang.Programs"    select="string('Programas')" />
  <xsl:variable name="lang.Psychological"  select="string('Psicológico')" />
  <xsl:variable name="lang.Qty"      select="string('Qtd')" />
  <xsl:variable name="lang.Quality"    select="string('Qualidade')" />
  <xsl:variable name="lang.Qualities"    select="string('Qualidades')" />
  <xsl:variable name="lang.Radiation"      select="string('Radiação')" />
  <xsl:variable name="lang.Range"      select="string('Distância')" />
  <xsl:variable name="lang.Rating"    select="string('Nível')" />
  <xsl:variable name="lang.RC"      select="string('CR')" />
  <xsl:variable name="lang.Reaction"    select="string('Reação')" />
  <xsl:variable name="lang.REA"      select="string('REA')" />
  <xsl:variable name="lang.Reach"      select="string('Alcance')" />
  <xsl:variable name="lang.Reason"    select="string('Razão')" />
  <xsl:variable name="lang.Registered"  select="string('Registrado')" />
  <xsl:variable name="lang.Requires"    select="string('Requer')" />
  <xsl:variable name="lang.RES"      select="string('RES')" />
  <xsl:variable name="lang.Resistance"    select="string('Resistência')" />
  <xsl:variable name="lang.Resistances"    select="string('Resistências')" />
  <xsl:variable name="lang.Resonance"    select="string('Ressonância')" />
  <xsl:variable name="lang.Resources"    select="string('Recursos')" />
  <xsl:variable name="lang.Rigger"    select="string('Fusor')" />
  <xsl:variable name="lang.Rtg"      select="string('NVL')" />
  <xsl:variable name="lang.Run"      select="string('Correr')" />
  <xsl:variable name="lang.S"        select="string('C')" />
  <xsl:variable name="lang.Seats"      select="string('Assentos')" />
  <xsl:variable name="lang.Self"      select="string('Próprio')" />
  <xsl:variable name="lang.Services"    select="string('Serviços')" />
  <xsl:variable name="lang.Sensor"    select="string('Sensor')" />
  <xsl:variable name="lang.Show"      select="string('Mostrar: ')" />
  <xsl:variable name="lang.Skill"      select="string('Perícia')" />
  <xsl:variable name="lang.Skills"    select="string('Perícias')" />
  <xsl:variable name="lang.Skin"      select="string('Pele')" />
  <xsl:variable name="lang.Sleaze"    select="string('Subversão')" />
  <xsl:variable name="lang.SLZ"      select="string('CRP')" />
  <xsl:variable name="lang.Social"    select="string('Social')" />
  <xsl:variable name="lang.Sonic"      select="string('Sônico')" />
  <xsl:variable name="lang.Source"    select="string('Fonte')" />
  <xsl:variable name="lang.Special"    select="string('Especial')" />
  <xsl:variable name="lang.Speed"      select="string('Veloc.')" />
  <xsl:variable name="lang.Spell"      select="string('Feitiço')" />
  <xsl:variable name="lang.Spells"    select="string('Feitiços')" />
  <xsl:variable name="lang.Spirit"    select="string('Espírito')" />
  <xsl:variable name="lang.Spirits"    select="string('Espíritos')" />
  <xsl:variable name="lang.Sprite"    select="string('Sprite')" />
  <xsl:variable name="lang.Sprites"    select="string('Sprites')" />
  <xsl:variable name="lang.Standard"    select="string('Norma')" />
  <xsl:variable name="lang.Stream"    select="string('Fluxo')" />
  <xsl:variable name="lang.STR"      select="string('FOR')" />
  <xsl:variable name="lang.Strength"    select="string('Força')" />
  <xsl:variable name="lang.Stun"      select="string('Atordoamento')" />
  <xsl:variable name="lang.Submersion"  select="string('Submersão')" />
  <xsl:variable name="lang.Sustained"    select="string('Sustentado')" />
  <xsl:variable name="lang.Swim"      select="string('Nadar')" />
  <xsl:variable name="lang.Target"    select="string('Alvo')" />
  <xsl:variable name="lang.Tasks"    select="string('Tasks')" />
  <xsl:variable name="lang.Total"      select="string('Total')" />
  <xsl:variable name="lang.Touch"      select="string('Toque')" />
  <xsl:variable name="lang.Toxin"      select="string('Toxina')" />
  <xsl:variable name="lang.Tradition"    select="string('Tradição')" />
  <xsl:variable name="lang.Type"      select="string('Tipo')" />
  <xsl:variable name="lang.Unbound"    select="string('Desvinculado')" />
  <xsl:variable name="lang.Unknown"    select="string('Desconhecido')" />
  <xsl:variable name="lang.Unregistered"  select="string('Não Registrado')" />
  <xsl:variable name="lang.Under"      select="string('Sob o Cano')" />
  <xsl:variable name="lang.Vehicle"    select="string('Veículo')" />
  <xsl:variable name="lang.Vehicles"    select="string('Veículos')" />
  <xsl:variable name="lang.VR"      select="string('RV')" />
  <xsl:variable name="lang.W"        select="string('V')" />
  <xsl:variable name="lang.Walk"      select="string('Andar')" />
  <xsl:variable name="lang.Weaknesses"    select="string('Fraquezas')" />
  <xsl:variable name="lang.Weapon"    select="string('Arma')" />
  <xsl:variable name="lang.Weapons"    select="string('Armas')" />
  <xsl:variable name="lang.Week"      select="string('Semana')" />
  <xsl:variable name="lang.Weeks"      select="string('Semanas')" />
  <xsl:variable name="lang.Weight"    select="string('Peso')" />
  <xsl:variable name="lang.WIL"      select="string('VON')" />
  <xsl:variable name="lang.Willpower"    select="string('Vontade')" />
  <xsl:variable name="lang.with"      select="string('com')" />
  <xsl:variable name="lang.Yes"      select="string('Sim')" />

  <!-- multiple word phrases / composite words -->
  <xsl:variable name="lang.ActiveSkills"    select="string('Perícias Ativas')" />
  <xsl:variable name="lang.AdeptPowers"    select="string('Poderes de Adepto')" />
  <xsl:variable name="lang.AIandAdvanced"    select="string('Programas de IA e Avançados')" />
  <xsl:variable name="lang.AlreadyAddicted"  select="string('Já Viciado')" />
  <xsl:variable name="lang.ArmorValue"    select="string('Valor')" />
  <xsl:variable name="lang.AspectedMagician"    select="string('Mago Observado')" />
  <xsl:variable name="lang.AstralInitiative"  select="string('Iniciativa Astral')" />
  <xsl:variable name="lang.AstralReputation"    select="string('Astral Reputation')" />
  <xsl:variable name="lang.CombatSkill"    select="string('Perícia de Combate')" />
  <xsl:variable name="lang.ComplexForm"    select="string('Forma Complexa')" />
  <xsl:variable name="lang.ComplexForms"    select="string('Formas Complexas')" />
  <xsl:variable name="lang.ConditionMonitor"  select="string('Monitor de Condição')" />
  <xsl:variable name="lang.ContactList"    select="string('Lista de Contatos')" />
  <xsl:variable name="lang.CoreTrack"  select="string('Faixa de Dano do Núcleo')" />
  <xsl:variable name="lang.CritterPower"    select="string('Poder de Criatura')" />
  <xsl:variable name="lang.CritterPowers"    select="string('Poderes de Criatura')" />
  <xsl:variable name="lang.CurrentEdge"    select="string('Pontos de Trunfo Atual')" />
  <xsl:variable name="lang.CurrentForm"    select="string('Forma Atual')" />
  <xsl:variable name="lang.DamageType"  select="string('Tipo de Dano')" />
  <xsl:variable name="lang.DataProc"      select="string('Datagrama')" />
  <xsl:variable name="lang.DataProcessing"  select="string('Datagrama')" />
  <xsl:variable name="lang.DecreaseAttribute"    select="string('Reduzir Atributo')" />
  <xsl:variable name="lang.DerivedAttributes"  select="string('Atributos Derivados')" />
  <xsl:variable name="lang.DeviceRating"    select="string('Nível')" />
  <xsl:variable name="lang.FadingValue"    select="string('Valor de Enfraquecimento')" />
  <xsl:variable name="lang.HobbiesVice"    select="string('Passatempos/Vícios')" />
  <xsl:variable name="lang.IDcredsticks"    select="string('Identidade/Bastões de Crédito')" />
  <xsl:variable name="lang.InitiateGrade"    select="string('Classe de Iniciado')" />
  <xsl:variable name="lang.InitiationNotes"  select="string('Notas de Classificação de Iniciação')" />
  <xsl:variable name="lang.JudgeIntentions"  select="string('Julgar Intenções')" />
  <xsl:variable name="lang.KnowledgeSkills"  select="string('Perícias de Conhecimento')" />
  <xsl:variable name="lang.LiftCarry"      select="string('Erguer/Carregar')" />
  <xsl:variable name="lang.LineofSight"    select="string('Linha de Visão')" />
  <xsl:variable name="lang.LinkedSIN"      select="string('SIN Conectado')" />
  <xsl:variable name="lang.MartialArt"    select="string('Arte Marcial')" />
  <xsl:variable name="lang.MartialArts"    select="string('Artes Marciais')" />
  <xsl:variable name="lang.MatrixAR"      select="string('Matriz RA')" />
  <xsl:variable name="lang.MatrixCold"    select="string('Matriz Fechado')" />
  <xsl:variable name="lang.MatrixDevices"    select="string('Matriz Dispositivos')" />
  <xsl:variable name="lang.MatrixHot"      select="string('Matriz Aberto')" />
  <xsl:variable name="lang.MatrixTrack"    select="string('Faixa de Dano da Matriz')" />
  <xsl:variable name="lang.MeleeWeapons"    select="string('Armas Corpo a Corpo')" />
  <xsl:variable name="lang.MentalAttributes"  select="string('Atributos Mentais')" />
  <xsl:variable name="lang.MysticAdept"    select="string('Adepto Místico')" />
  <xsl:variable name="lang.NotAddictedYet"  select="string('Ainda Não Viciado')" />
  <xsl:variable name="lang.Nothing2Show4Devices"    select="string('Nenhum Dispositivo para listar')" />
  <xsl:variable name="lang.Nothing2Show4Notes"    select="string('Nenhuma Observação para listar')" />
  <xsl:variable name="lang.Nothing2Show4SpiritsSprites"    select="string('Nenhum Espíritos/Sprites para listar')" />
  <xsl:variable name="lang.Nothing2Show4Vehicles"    select="string('Nenhum Veículo para listar')" />
  <xsl:variable name="lang.OptionalPowers"    select="string('Poderes Opcionais')" />
  <xsl:variable name="lang.OtherArmor"      select="string('Outras Armaduras')" />
  <xsl:variable name="lang.OtherMugshots"    select="string('Outros Retratos')" />
  <xsl:variable name="lang.PageBreak"      select="string('Quebra de Página: ')" />
  <xsl:variable name="lang.PersonalData"    select="string('Dados Pessoais')" />
  <xsl:variable name="lang.PersonalLife"    select="string('Vida Pessoal')" />
  <xsl:variable name="lang.PhysicalAttributes"  select="string('Atributos Físicos')" />
  <xsl:variable name="lang.PhysicalNaturalRecovery"  select="string('Pilha de Recuperação Natural (1 dia)')" />
  <xsl:variable name="lang.PhysicalTrack"  select="string('Faixa de Dano Físico')" />
  <xsl:variable name="lang.PreferredPayment"    select="string('Método de Pagamento Preferido')" />
  <xsl:variable name="lang.PrimaryArm"    select="string('Mão Primária')" />
  <xsl:variable name="lang.PublicAwareness"  select="string('Consciência Pública')" />
  <xsl:variable name="lang.RangedWeapons"    select="string('Armas à Distância')" />
  <xsl:variable name="lang.RemainingAvailable"  select="string('Restante Disponível')" />
  <xsl:variable name="lang.ResistDrain"    select="string('Resistir Dreno com')" />
  <xsl:variable name="lang.ResistFading"    select="string('Resistir Enfraquecimetno com')" />
  <xsl:variable name="lang.RiggerInitiative"  select="string('Iniciativa de Fusor')" />
  <xsl:variable name="lang.SelectedGear"    select="string('Engrenagem Selecionada')" />
  <xsl:variable name="lang.SkillGroup"    select="string('Grupo de Perícia')" />
  <xsl:variable name="lang.SkillGroups"    select="string('Grupos de Perícia')" />
  <xsl:variable name="lang.SpecialAttributes"  select="string('Atributos Especiais')" />
  <xsl:variable name="lang.StreetCred"    select="string('Crédito de Rua')" />
  <xsl:variable name="lang.StreetName"    select="string('Nome de Rua')" />
  <xsl:variable name="lang.StunNaturalRecovery"  select="string('Pilha de Recuperação Natural (1 hora)')" />
  <xsl:variable name="lang.StunTrack"    select="string('Faixa de Dano de Atordoamento')" />
  <xsl:variable name="lang.SubmersionGrade"  select="string('Classe de Submersão')" />
  <xsl:variable name="lang.SubmersionNotes"  select="string('Notas de Submersão')" />
  <xsl:variable name="lang.ToggleColors"  select="string('Alternar cores')" />
  <xsl:variable name="lang.TotalArmor"  select="string('Total de armaduras e acessórios mais altos equipados')" />
  <xsl:variable name="lang.ToxinsAndPathogens"  select="string('Toxinas e Patogênicos')" />
  <xsl:variable name="lang.UnnamedCharacter"  select="string('Personagem Sem Nome')" />
  <xsl:variable name="lang.VehicleBody"    select="string('Corpo')" />
  <xsl:variable name="lang.VehicleCost"    select="string('Custo de Veículo')" />
  <xsl:variable name="lang.WildReputation"    select="string('Wild Reputation')" />

  <!-- "limits" list -->
  <xsl:variable name="lang.AstralLimit"    select="string('Limite Astral')" />
  <xsl:variable name="lang.MentalLimit"    select="string('Limite Mental')" />
  <xsl:variable name="lang.PhysicalLimit"    select="string('Limite Físico')" />
  <xsl:variable name="lang.SocialLimit"    select="string('Limite Social')" />

  <!-- spell types list -->
  <xsl:variable name="lang.CombatSpells"    select="string('Feitiços de Combate')" />
  <xsl:variable name="lang.DetectionSpells"  select="string('Feitiços de Detecção')" />
  <xsl:variable name="lang.Enchantments"    select="string('Encantamentos')" />
  <xsl:variable name="lang.HealthSpells"     select="string('Feitiços de Saúde')" />
  <xsl:variable name="lang.IllusionSpells"   select="string('Feitiços de Ilusão')" />
  <xsl:variable name="lang.ManipulationSpells" select="string('Feitiços de Manipulação')" />
  <xsl:variable name="lang.Rituals"      select="string('Ritual')" />

  <!-- test values -->
  <xsl:variable name="lang.tstDamage1"  select="string('F')" />
  <xsl:variable name="lang.tstDamage2"  select="string('A')" />
  <xsl:variable name="lang.tstDuration1"  select="string('I')" />
  <xsl:variable name="lang.tstDuration2"  select="string('P')" />
  <xsl:variable name="lang.tstDuration3"  select="string('S')" />
  <xsl:variable name="lang.tstRange1"    select="string('T')" />
  <xsl:variable name="lang.tstRange2"    select="string('LDV')" />
  <xsl:variable name="lang.tstRange3"    select="string('LDV(A)')" />
  <xsl:variable name="lang.tstRange4"    select="string('LDV (A)')" />
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
    <xsl:variable name="lang.marks"    select="string(',.')" />
</xsl:stylesheet>
