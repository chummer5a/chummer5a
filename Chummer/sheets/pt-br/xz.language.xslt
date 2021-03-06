<?xml version="1.0" encoding="utf-8" ?>
<!-- Isolate Portuguese (pt-br) locale literals -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="lang"  select="'pt'"/>
  <xsl:variable name="locale"  select="'pt-br'"/>

  <!-- individual words -->
  <xsl:variable name="lang.Acceleration"  select="'Aceleração'"/>
  <xsl:variable name="lang.Accel"      select="'Acel'"/>
  <xsl:variable name="lang.Accessories"  select="'Acessórios'"/>
  <xsl:variable name="lang.Accuracy"    select="'Precisão'"/>
  <xsl:variable name="lang.Acid"      select="'Ácido'"/>
  <xsl:variable name="lang.Action"      select="'Ação'"/>
  <xsl:variable name="lang.Addiction"  select="'Vício'"/>
  <xsl:variable name="lang.Adept"      select="'Adepto'"/>
  <xsl:variable name="lang.Age"      select="'Idade'"/>
  <xsl:variable name="lang.AGI"      select="'AGI'"/>
  <xsl:variable name="lang.Agility"    select="'Agilidade'"/>
  <xsl:variable name="lang.AI"      select="'IA'"/>
  <xsl:variable name="lang.Alias"      select="'Pseudônimo'"/>
  <xsl:variable name="lang.Ammo"      select="'Munição'"/>
  <xsl:variable name="lang.Amount"    select="'Quantidade'"/>
  <xsl:variable name="lang.AP"      select="'PA'"/>
  <xsl:variable name="lang.Applicable"  select="'Aplicável'"/>
  <xsl:variable name="lang.Apprentice"  select="'Apprentiz'"/>
  <xsl:variable name="lang.AR"      select="'RA'"/>
  <xsl:variable name="lang.Archetype"    select="'Arquétipo'"/>
  <xsl:variable name="lang.Area"      select="'Área'"/>
  <xsl:variable name="lang.Armor"      select="'Armadura'"/>
  <xsl:variable name="lang.Arts"      select="'Artes'"/>
  <xsl:variable name="lang.as"      select="'como'"/>
  <xsl:variable name="lang.ASDF"    select="'A/S/D/F'"/>
  <xsl:variable name="lang.Astral"    select="'Astral'"/>
  <xsl:variable name="lang.Attack"    select="'Ataque'"/>
  <xsl:variable name="lang.ATT"      select="'ATQ'"/>
  <xsl:variable name="lang.Attribute"    select="'Atributo'"/>
  <xsl:variable name="lang.Attributes"  select="'Atributos'"/>
  <xsl:variable name="lang.Available"    select="'Disponível'"/>
  <xsl:variable name="lang.Awakened"    select="'Despertou'"/>
  <xsl:variable name="lang.Aware"    select="'Consciente'"/>
  <xsl:variable name="lang.Background"  select="'Antecedentes'"/>
  <xsl:variable name="lang.Base"      select="'Base'"/>
  <xsl:variable name="lang.Bioware"    select="'Biônico'"/>
  <xsl:variable name="lang.BOD"      select="'COR'"/>
  <xsl:variable name="lang.Body"      select="'Corpo'"/>
  <xsl:variable name="lang.Bonus"      select="'Bônus'"/>
  <xsl:variable name="lang.Bound"      select="'Vinculado'"/>
  <xsl:variable name="lang.Calendar"    select="'Calendário'"/>
  <xsl:variable name="lang.Career"    select="'Carreira'"/>
  <xsl:variable name="lang.Category"    select="'Categoria'"/>
  <xsl:variable name="lang.CHA"      select="'CAR'"/>
  <xsl:variable name="lang.Charisma"    select="'Carisma'"/>
  <xsl:variable name="lang.CM"      select="'MC'"/>
  <xsl:variable name="lang.Cold"      select="'Frio'"/>
  <xsl:variable name="lang.Combat"    select="'Combate'"/>
  <xsl:variable name="lang.Commlink"    select="'Com-Link'"/>
  <xsl:variable name="lang.Composure"    select="'Compostura'"/>
  <xsl:variable name="lang.Concept"    select="'Conceito'"/>
  <xsl:variable name="lang.Connection"  select="'Conexão'"/>
  <xsl:variable name="lang.Contact"    select="'Contato'"/>
  <xsl:variable name="lang.ContactDrug"    select="'Contato'"/>
  <xsl:variable name="lang.Contacts"    select="'Contatos'"/>
  <xsl:variable name="lang.Cost"      select="'Custo'"/>
  <xsl:variable name="lang.Critter"    select="'Criatura'"/>
  <xsl:variable name="lang.Critters"    select="'Criaturas'"/>
  <xsl:variable name="lang.Cyberware"    select="'Cibernético'"/>
  <xsl:variable name="lang.Damage"    select="'Dano'"/>
  <xsl:variable name="lang.Data"      select="'Dados'"/>
  <xsl:variable name="lang.Date"      select="'Data'"/>
  <xsl:variable name="lang.Day"      select="'Dia'"/>
  <xsl:variable name="lang.Days"      select="'Dias'"/>
  <xsl:variable name="lang.Dead"      select="'Morto'"/>
  <xsl:variable name="lang.Defense"      select="'Defesa'"/>
  <xsl:variable name="lang.DEP"  select="'PRO'"/>
  <xsl:variable name="lang.Depth"  select="'Profundidade'"/>
  <xsl:variable name="lang.Description"  select="'Descrição'"/>
  <xsl:variable name="lang.Detection"    select="'Detecção'"/>
  <xsl:variable name="lang.Device"    select="'Dispositivo'"/>
  <xsl:variable name="lang.Devices"    select="'Dispositivos'"/>
  <xsl:variable name="lang.Direct"      select="'Direto'"/>
  <xsl:variable name="lang.Down"      select="'Caído'"/>
  <xsl:variable name="lang.DP"        select="'PD'"/>
  <xsl:variable name="lang.Drain"      select="'Dreno'"/>
  <xsl:variable name="lang.Drone"      select="'Drone'"/>
  <xsl:variable name="lang.Duration"    select="'Duração'"/>
  <xsl:variable name="lang.DV"      select="'VD'"/>
  <xsl:variable name="lang.E"        select="'E'"/>
  <xsl:variable name="lang.Echo"    select="'Eco'"/>
  <xsl:variable name="lang.Echoes"    select="'Ecos'"/>
  <xsl:variable name="lang.EDG"      select="'TRU'"/>
  <xsl:variable name="lang.Edge"      select="'Trunfo'"/>
  <xsl:variable name="lang.Electricity"    select="'Eletricidade'"/>
  <xsl:variable name="lang.Enchanter"    select="'Enchanter'"/>
  <xsl:variable name="lang.Enemies"    select="'Inimigos'"/>
  <xsl:variable name="lang.Enhancements"  select="'Melhorias'"/>
  <xsl:variable name="lang.Entries"    select="'Entradas'"/>
  <xsl:variable name="lang.Equipped"    select="'Equipado'"/>
  <xsl:variable name="lang.ESS"      select="'ESS'"/>
  <xsl:variable name="lang.Essence"    select="'Essência'"/>
  <xsl:variable name="lang.Expenses"    select="'Despesas'"/>
  <xsl:variable name="lang.Explorer"    select="'Explorador'"/>
  <xsl:variable name="lang.Eyes"      select="'Olhos'"/>
  <xsl:variable name="lang.Falling"    select="'Queda'"/>
  <xsl:variable name="lang.Fatigue"      select="'Fadiga'"/>
  <xsl:variable name="lang.Fettered"      select="'Aprisionado'"/>
  <xsl:variable name="lang.Fire"    select="'Fogo'"/>
  <xsl:variable name="lang.Firewall"    select="'Firewall'"/>
  <xsl:variable name="lang.Fly"      select="'Voo'"/>
  <xsl:variable name="lang.Foci"      select="'Foco'"/>
  <xsl:variable name="lang.FWL"      select="'FWL'"/>
  <xsl:variable name="lang.Force"      select="'Poder'"/>
  <xsl:variable name="lang.FV"      select="'VE'"/>
  <xsl:variable name="lang.Gear"      select="'Equipamento'"/>
  <xsl:variable name="lang.Gender"      select="'Gênero'"/>
  <xsl:variable name="lang.Grade"      select="'Classe'"/>
  <xsl:variable name="lang.Hair"      select="'Cabelos'"/>
  <xsl:variable name="lang.Handling"    select="'Manejo'"/>
  <xsl:variable name="lang.Health"    select="'Saúde'"/>
  <xsl:variable name="lang.Heavy"    select="'Pesado'"/>
  <xsl:variable name="lang.Height"    select="'Altura'"/>
  <xsl:variable name="lang.hit"      select="'sucesso'"/>
  <xsl:variable name="lang.Illusion"    select="'Ilusão'"/>
  <xsl:variable name="lang.Implant"    select="'Implante'"/>
  <xsl:variable name="lang.Indirect"      select="'Indireto'"/>
  <xsl:variable name="lang.Info"      select="'Info'"/>
  <xsl:variable name="lang.Ingestion"      select="'Ingestão'"/>
  <xsl:variable name="lang.Inhalation"      select="'Inalação'"/>
  <xsl:variable name="lang.Init"      select="'Inic'"/>
  <xsl:variable name="lang.Initiation"  select="'Iniciação'"/>
  <xsl:variable name="lang.Initiative"  select="'Iniciativa'"/>
  <xsl:variable name="lang.Injection"      select="'Injeção'"/>
  <xsl:variable name="lang.INT"      select="'INT'"/>
  <xsl:variable name="lang.Intentions"  select="'Intenções'"/>
  <xsl:variable name="lang.Intuition"    select="'Intuição'"/>
  <xsl:variable name="lang.Instantaneous"  select="'Instantâneo'"/>
  <xsl:variable name="lang.Karma"      select="'Carma'"/>
  <xsl:variable name="lang.L"        select="'L'"/>
  <xsl:variable name="lang.Level"      select="'Nível'"/>
  <xsl:variable name="lang.Lifestyle"    select="'Estilo de Vida'"/>
  <xsl:variable name="lang.Limit"      select="'Limite'"/>
  <xsl:variable name="lang.Limits"    select="'Limites'"/>
  <xsl:variable name="lang.Loaded"    select="'Carregado'"/>
  <xsl:variable name="lang.Location"    select="'Localização'"/>
  <xsl:variable name="lang.LOG"      select="'LOG'"/>
  <xsl:variable name="lang.Logic"      select="'Lógica'"/>
  <xsl:variable name="lang.Loyalty"    select="'Lealdade'"/>
  <xsl:variable name="lang.M"        select="'M'"/>
  <xsl:variable name="lang.MAG"      select="'MAG'"/>
  <xsl:variable name="lang.Magic"      select="'Magia'"/>
  <xsl:variable name="lang.Magician"      select="'Mágico'"/>
  <xsl:variable name="lang.Mana"    select="'Mana'"/>
  <xsl:variable name="lang.Maneuvers"    select="'Manobras'"/>
  <xsl:variable name="lang.Manipulation"  select="'Manipulação'"/>
  <xsl:variable name="lang.Manual"    select="'Manual'"/>
  <xsl:variable name="lang.Memory"    select="'Memória'"/>
  <xsl:variable name="lang.Mental"    select="'Mental'"/>
  <xsl:variable name="lang.Metamagics"  select="'Metamágicas'"/>
  <xsl:variable name="lang.Metatype"    select="'Metatipo'"/>
  <xsl:variable name="lang.Mod"      select="'Mod'"/>
  <xsl:variable name="lang.Mode"      select="'Modo'"/>
  <xsl:variable name="lang.Model"      select="'Modelo'"/>
  <xsl:variable name="lang.Modifications"  select="'Modificações'"/>
  <xsl:variable name="lang.Month"      select="'Mês'"/>
  <xsl:variable name="lang.Months"    select="'Meses'"/>
  <xsl:variable name="lang.Mount"    select="'Montar'"/>
  <xsl:variable name="lang.Movement"    select="'Movimento'"/>
  <xsl:variable name="lang.Mugshot"    select="'Retrato'"/>
  <xsl:variable name="lang.Name"      select="'Nome'"/>
  <xsl:variable name="lang.Native"    select="'Nativo'"/>
  <xsl:variable name="lang.Negative"    select="'Negativo'"/>
  <xsl:variable name="lang.No"      select="'Não'"/>
  <xsl:variable name="lang.None"      select="'None'"/>
  <xsl:variable name="lang.Notes"      select="'Observações'"/>
  <xsl:variable name="lang.Notoriety"    select="'Notoriedade'"/>
  <xsl:variable name="lang.Nuyen"      select="'Neoiene'"/>
  <xsl:variable name="lang.Other"      select="'Outros'"/>
  <xsl:variable name="lang.Overflow"      select="'Exceder'"/>
  <xsl:variable name="lang.OVR"      select="'Exced'"/>
  <xsl:variable name="lang.Pathogen"    select="'Patogênico'"/>
  <xsl:variable name="lang.Permanent"    select="'Permanente'"/>
  <xsl:variable name="lang.Persona"    select="'Persona'"/>
  <xsl:variable name="lang.Pets"      select="'Mascotes'"/>
  <xsl:variable name="lang.Physical"    select="'Físico'"/>
  <xsl:variable name="lang.Physiological"  select="'Físico'"/>
  <xsl:variable name="lang.Pilot"      select="'Piloto'"/>
  <xsl:variable name="lang.Player"    select="'Jogador'"/>
  <xsl:variable name="lang.Points"    select="'Pontos'"/>
  <xsl:variable name="lang.Pool"      select="'Pilha'"/>
  <xsl:variable name="lang.Positive"    select="'Positivo'"/>
  <xsl:variable name="lang.Power"      select="'Poder'"/>
  <xsl:variable name="lang.Powers"    select="'Poderes'"/>
  <xsl:variable name="lang.Priorities"  select="'Prioridades'"/>
  <xsl:variable name="lang.Processor"    select="'Processador'"/>
  <xsl:variable name="lang.Program"    select="'Programa'"/>
  <xsl:variable name="lang.Programs"    select="'Programas'"/>
  <xsl:variable name="lang.Psychological"  select="'Psicológico'"/>
  <xsl:variable name="lang.Qty"      select="'Qtd'"/>
  <xsl:variable name="lang.Quality"    select="'Qualidade'"/>
  <xsl:variable name="lang.Qualities"    select="'Qualidades'"/>
  <xsl:variable name="lang.Radiation"      select="'Radiação'"/>
  <xsl:variable name="lang.Range"      select="'Distância'"/>
  <xsl:variable name="lang.Rating"    select="'Nível'"/>
  <xsl:variable name="lang.RC"      select="'CR'"/>
  <xsl:variable name="lang.Reaction"    select="'Reação'"/>
  <xsl:variable name="lang.REA"      select="'REA'"/>
  <xsl:variable name="lang.Reach"      select="'Alcance'"/>
  <xsl:variable name="lang.Reason"    select="'Razão'"/>
  <xsl:variable name="lang.Registered"  select="'Registrado'"/>
  <xsl:variable name="lang.Requires"    select="'Requer'"/>
  <xsl:variable name="lang.RES"      select="'RES'"/>
  <xsl:variable name="lang.Resistance"    select="'Resistência'"/>
  <xsl:variable name="lang.Resistances"    select="'Resistências'"/>
  <xsl:variable name="lang.Resonance"    select="'Ressonância'"/>
  <xsl:variable name="lang.Resources"    select="'Recursos'"/>
  <xsl:variable name="lang.Rigger"    select="'Fusor'"/>
  <xsl:variable name="lang.Rtg"      select="'NVL'"/>
  <xsl:variable name="lang.Run"      select="'Correr'"/>
  <xsl:variable name="lang.S"        select="'C'"/>
  <xsl:variable name="lang.Seats"      select="'Assentos'"/>
  <xsl:variable name="lang.Self"      select="'Próprio'"/>
  <xsl:variable name="lang.Services"    select="'Serviços'"/>
  <xsl:variable name="lang.Sensor"    select="'Sensor'"/>
  <xsl:variable name="lang.Show"      select="'Mostrar: '"/>
  <xsl:variable name="lang.Skill"      select="'Perícia'"/>
  <xsl:variable name="lang.Skills"    select="'Perícias'"/>
  <xsl:variable name="lang.Skin"      select="'Pele'"/>
  <xsl:variable name="lang.Sleaze"    select="'Subversão'"/>
  <xsl:variable name="lang.SLZ"      select="'CRP'"/>
  <xsl:variable name="lang.Social"    select="'Social'"/>
  <xsl:variable name="lang.Sonic"      select="'Sônico'"/>
  <xsl:variable name="lang.Source"    select="'Fonte'"/>
  <xsl:variable name="lang.Special"    select="'Especial'"/>
  <xsl:variable name="lang.Speed"      select="'Veloc.'"/>
  <xsl:variable name="lang.Spell"      select="'Feitiço'"/>
  <xsl:variable name="lang.Spells"    select="'Feitiços'"/>
  <xsl:variable name="lang.Spirit"    select="'Espírito'"/>
  <xsl:variable name="lang.Spirits"    select="'Espíritos'"/>
  <xsl:variable name="lang.Sprite"    select="'Sprite'"/>
  <xsl:variable name="lang.Sprites"    select="'Sprites'"/>
  <xsl:variable name="lang.Standard"    select="'Norma'"/>
  <xsl:variable name="lang.Stream"    select="'Fluxo'"/>
  <xsl:variable name="lang.STR"      select="'FOR'"/>
  <xsl:variable name="lang.Strength"    select="'Força'"/>
  <xsl:variable name="lang.Stun"      select="'Atordoamento'"/>
  <xsl:variable name="lang.Submersion"  select="'Submersão'"/>
  <xsl:variable name="lang.Sustained"    select="'Sustentado'"/>
  <xsl:variable name="lang.Swim"      select="'Nadar'"/>
  <xsl:variable name="lang.Target"    select="'Alvo'"/>
  <xsl:variable name="lang.Tasks"    select="'Tasks'"/>
  <xsl:variable name="lang.Total"      select="'Total'"/>
  <xsl:variable name="lang.Touch"      select="'Toque'"/>
  <xsl:variable name="lang.Toxin"      select="'Toxina'"/>
  <xsl:variable name="lang.Tradition"    select="'Tradição'"/>
  <xsl:variable name="lang.Type"      select="'Tipo'"/>
  <xsl:variable name="lang.Unbound"    select="'Desvinculado'"/>
  <xsl:variable name="lang.Unknown"    select="'Desconhecido'"/>
  <xsl:variable name="lang.Unregistered"  select="'Não Registrado'"/>
  <xsl:variable name="lang.Under"      select="'Sob o Cano'"/>
  <xsl:variable name="lang.Vehicle"    select="'Veículo'"/>
  <xsl:variable name="lang.Vehicles"    select="'Veículos'"/>
  <xsl:variable name="lang.VR"      select="'RV'"/>
  <xsl:variable name="lang.W"        select="'V'"/>
  <xsl:variable name="lang.Walk"      select="'Andar'"/>
  <xsl:variable name="lang.Weaknesses"    select="'Fraquezas'"/>
  <xsl:variable name="lang.Weapon"    select="'Arma'"/>
  <xsl:variable name="lang.Weapons"    select="'Armas'"/>
  <xsl:variable name="lang.Week"      select="'Semana'"/>
  <xsl:variable name="lang.Weeks"      select="'Semanas'"/>
  <xsl:variable name="lang.Weight"    select="'Peso'"/>
  <xsl:variable name="lang.WIL"      select="'VON'"/>
  <xsl:variable name="lang.Willpower"    select="'Vontade'"/>
  <xsl:variable name="lang.with"      select="'com'"/>
  <xsl:variable name="lang.Yes"      select="'Sim'"/>

  <!-- multiple word phrases / composite words -->
  <xsl:variable name="lang.ActiveSkills"    select="'Perícias Ativas'"/>
  <xsl:variable name="lang.AdeptPowers"    select="'Poderes de Adepto'"/>
  <xsl:variable name="lang.AIandAdvanced"    select="'Programas de IA e Avançados'"/>
  <xsl:variable name="lang.AlreadyAddicted"  select="'Já Viciado'"/>
  <xsl:variable name="lang.ArmorValue"    select="'Valor'"/>
  <xsl:variable name="lang.AspectedMagician"    select="'Mago Observado'"/>
  <xsl:variable name="lang.AstralInitiative"  select="'Iniciativa Astral'"/>
  <xsl:variable name="lang.AstralReputation"    select="'Astral Reputation'"/>
  <xsl:variable name="lang.CombatSkill"    select="'Perícia de Combate'"/>
  <xsl:variable name="lang.ComplexForm"    select="'Forma Complexa'"/>
  <xsl:variable name="lang.ComplexForms"    select="'Formas Complexas'"/>
  <xsl:variable name="lang.ConditionMonitor"  select="'Monitor de Condição'"/>
  <xsl:variable name="lang.ContactList"    select="'Lista de Contatos'"/>
  <xsl:variable name="lang.CoreTrack"  select="'Faixa de Dano do Núcleo'"/>
  <xsl:variable name="lang.CritterPower"    select="'Poder de Criatura'"/>
  <xsl:variable name="lang.CritterPowers"    select="'Poderes de Criatura'"/>
  <xsl:variable name="lang.CurrentEdge"    select="'Pontos de Trunfo Atual'"/>
  <xsl:variable name="lang.CurrentForm"    select="'Forma Atual'"/>
  <xsl:variable name="lang.DamageType"  select="'Tipo de Dano'"/>
  <xsl:variable name="lang.DataProc"      select="'Datagrama'"/>
  <xsl:variable name="lang.DataProcessing"  select="'Datagrama'"/>
  <xsl:variable name="lang.DecreaseAttribute"    select="'Reduzir Atributo'"/>
  <xsl:variable name="lang.DerivedAttributes"  select="'Atributos Derivados'"/>
  <xsl:variable name="lang.DeviceRating"    select="'Nível'"/>
  <xsl:variable name="lang.FadingValue"    select="'Valor de Enfraquecimento'"/>
  <xsl:variable name="lang.HobbiesVice"    select="'Passatempos/Vícios'"/>
  <xsl:variable name="lang.IDcredsticks"    select="'Identidade/Bastões de Crédito'"/>
  <xsl:variable name="lang.InitiateGrade"    select="'Classe de Iniciado'"/>
  <xsl:variable name="lang.InitiationNotes"  select="'Notas de Classificação de Iniciação'"/>
  <xsl:variable name="lang.JudgeIntentions"  select="'Julgar Intenções'"/>
  <xsl:variable name="lang.KnowledgeSkills"  select="'Perícias de Conhecimento'"/>
  <xsl:variable name="lang.LiftCarry"      select="'Erguer/Carregar'"/>
  <xsl:variable name="lang.LineofSight"    select="'Linha de Visão'"/>
  <xsl:variable name="lang.LinkedSIN"      select="'SIN Conectado'"/>
  <xsl:variable name="lang.MartialArt"    select="'Arte Marcial'"/>
  <xsl:variable name="lang.MartialArts"    select="'Artes Marciais'"/>
  <xsl:variable name="lang.MatrixAR"      select="'Matriz RA'"/>
  <xsl:variable name="lang.MatrixCold"    select="'Matriz Fechado'"/>
  <xsl:variable name="lang.MatrixDevices"    select="'Matriz Dispositivos'"/>
  <xsl:variable name="lang.MatrixHot"      select="'Matriz Aberto'"/>
  <xsl:variable name="lang.MatrixTrack"    select="'Faixa de Dano da Matriz'"/>
  <xsl:variable name="lang.MeleeWeapons"    select="'Armas Corpo a Corpo'"/>
  <xsl:variable name="lang.MentalAttributes"  select="'Atributos Mentais'"/>
  <xsl:variable name="lang.MysticAdept"    select="'Adepto Místico'"/>
  <xsl:variable name="lang.NotAddictedYet"  select="'Ainda Não Viciado'"/>
  <xsl:variable name="lang.Nothing2Show4Devices"    select="'Nenhum Dispositivo para listar'"/>
  <xsl:variable name="lang.Nothing2Show4Notes"    select="'Nenhuma Observação para listar'"/>
  <xsl:variable name="lang.Nothing2Show4SpiritsSprites"    select="'Nenhum Espíritos/Sprites para listar'"/>
  <xsl:variable name="lang.Nothing2Show4Vehicles"    select="'Nenhum Veículo para listar'"/>
  <xsl:variable name="lang.OptionalPowers"    select="'Poderes Opcionais'"/>
  <xsl:variable name="lang.OtherArmor"      select="'Outras Armaduras'"/>
  <xsl:variable name="lang.OtherMugshots"    select="'Outros Retratos'"/>
  <xsl:variable name="lang.PageBreak"      select="'Quebra de Página: '"/>
  <xsl:variable name="lang.PersonalData"    select="'Dados Pessoais'"/>
  <xsl:variable name="lang.PersonalLife"    select="'Vida Pessoal'"/>
  <xsl:variable name="lang.PhysicalAttributes"  select="'Atributos Físicos'"/>
  <xsl:variable name="lang.PhysicalNaturalRecovery"  select="'Pilha de Recuperação Natural (1 dia)'"/>
  <xsl:variable name="lang.PhysicalTrack"  select="'Faixa de Dano Físico'"/>
  <xsl:variable name="lang.PreferredPayment"    select="'Método de Pagamento Preferido'"/>
  <xsl:variable name="lang.PrimaryArm"    select="'Mão Primária'"/>
  <xsl:variable name="lang.PublicAwareness"  select="'Consciência Pública'"/>
  <xsl:variable name="lang.RangedWeapons"    select="'Armas à Distância'"/>
  <xsl:variable name="lang.RemainingAvailable"  select="'Restante Disponível'"/>
  <xsl:variable name="lang.ResistDrain"    select="'Resistir Dreno com'"/>
  <xsl:variable name="lang.ResistFading"    select="'Resistir Enfraquecimetno com'"/>
  <xsl:variable name="lang.RiggerInitiative"  select="'Iniciativa de Fusor'"/>
  <xsl:variable name="lang.SelectedGear"    select="'Engrenagem Selecionada'"/>
  <xsl:variable name="lang.SkillGroup"    select="'Grupo de Perícia'"/>
  <xsl:variable name="lang.SkillGroups"    select="'Grupos de Perícia'"/>
  <xsl:variable name="lang.SpecialAttributes"  select="'Atributos Especiais'"/>
  <xsl:variable name="lang.StreetCred"    select="'Crédito de Rua'"/>
  <xsl:variable name="lang.StreetName"    select="'Nome de Rua'"/>
  <xsl:variable name="lang.StunNaturalRecovery"  select="'Pilha de Recuperação Natural (1 hora)'"/>
  <xsl:variable name="lang.StunTrack"    select="'Faixa de Dano de Atordoamento'"/>
  <xsl:variable name="lang.SubmersionGrade"  select="'Classe de Submersão'"/>
  <xsl:variable name="lang.SubmersionNotes"  select="'Notas de Submersão'"/>
  <xsl:variable name="lang.ToggleColors"  select="'Alternar cores'"/>
  <xsl:variable name="lang.TotalArmor"  select="'Total de armaduras e acessórios mais altos equipados'"/>
  <xsl:variable name="lang.ToxinsAndPathogens"  select="'Toxinas e Patogênicos'"/>
  <xsl:variable name="lang.UnnamedCharacter"  select="'Personagem Sem Nome'"/>
  <xsl:variable name="lang.VehicleBody"    select="'Corpo'"/>
  <xsl:variable name="lang.VehicleCost"    select="'Custo de Veículo'"/>
  <xsl:variable name="lang.WildReputation"    select="'Wild Reputation'"/>

  <!-- "limits" list -->
  <xsl:variable name="lang.AstralLimit"    select="'Limite Astral'"/>
  <xsl:variable name="lang.MentalLimit"    select="'Limite Mental'"/>
  <xsl:variable name="lang.PhysicalLimit"    select="'Limite Físico'"/>
  <xsl:variable name="lang.SocialLimit"    select="'Limite Social'"/>

  <!-- spell types list -->
  <xsl:variable name="lang.CombatSpells"    select="'Feitiços de Combate'"/>
  <xsl:variable name="lang.DetectionSpells"  select="'Feitiços de Detecção'"/>
  <xsl:variable name="lang.Enchantments"    select="'Encantamentos'"/>
  <xsl:variable name="lang.HealthSpells"     select="'Feitiços de Saúde'"/>
  <xsl:variable name="lang.IllusionSpells"   select="'Feitiços de Ilusão'"/>
  <xsl:variable name="lang.ManipulationSpells" select="'Feitiços de Manipulação'"/>
  <xsl:variable name="lang.Rituals"      select="'Ritual'"/>

  <!-- test values -->
  <xsl:variable name="lang.tstDamage1"  select="'F'"/>
  <xsl:variable name="lang.tstDamage2"  select="'A'"/>
  <xsl:variable name="lang.tstDuration1"  select="'I'"/>
  <xsl:variable name="lang.tstDuration2"  select="'P'"/>
  <xsl:variable name="lang.tstDuration3"  select="'S'"/>
  <xsl:variable name="lang.tstRange1"    select="'T'"/>
  <xsl:variable name="lang.tstRange2"    select="'LDV'"/>
  <xsl:variable name="lang.tstRange3"    select="'LDV(A)'"/>
  <xsl:variable name="lang.tstRange4"    select="'LDV (A)'"/>
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
    <xsl:variable name="lang.marks"    select="',.'"/>
</xsl:stylesheet>
