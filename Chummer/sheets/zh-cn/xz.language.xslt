<?xml version="1.0" encoding="utf-8" ?>
<!-- Isolate Chinese (zh) locale literals -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="lang"  select="'zh'"/>
  <xsl:variable name="locale"  select="'zh-cn'"/>

  <!-- individual words -->
  <xsl:variable name="lang.Acceleration"  select="'加速'"/>
  <xsl:variable name="lang.Accel"      select="'加速'"/>
  <xsl:variable name="lang.Accessories"  select="'配件'"/>
  <xsl:variable name="lang.Accuracy"    select="'准度'"/>
  <xsl:variable name="lang.Acid"      select="'Acid'"/>
  <xsl:variable name="lang.Action"      select="'Action'"/>
  <xsl:variable name="lang.Addiction"  select="'Addiction'"/>
  <xsl:variable name="lang.Adept"      select="'Adept'"/>
  <xsl:variable name="lang.Age"      select="'年龄'"/>
  <xsl:variable name="lang.AGI"      select="'AGI'"/>
  <xsl:variable name="lang.Agility"    select="'敏捷'"/>
  <xsl:variable name="lang.AI"      select="'AI'"/>
  <xsl:variable name="lang.Alias"      select="'绰号'"/>
  <xsl:variable name="lang.Ammo"      select="'弹药'"/>
  <xsl:variable name="lang.Amount"    select="'数量'"/>
  <xsl:variable name="lang.AP"      select="'AP'"/>
  <xsl:variable name="lang.Applicable"  select="'可用'"/>
  <xsl:variable name="lang.Apprentice"  select="'Apprentice'"/>
  <xsl:variable name="lang.AR"      select="'AR'"/>
  <xsl:variable name="lang.Archetype"    select="'职业'"/>
  <xsl:variable name="lang.Area"      select="'范围'"/>
  <xsl:variable name="lang.Armor"      select="'护甲'"/>
  <xsl:variable name="lang.Arts"      select="'技艺'"/>
  <xsl:variable name="lang.as"      select="'作为'"/>
  <xsl:variable name="lang.ASDF"    select="'A/S/D/F'"/>
  <xsl:variable name="lang.Astral"    select="'星界'"/>
  <xsl:variable name="lang.Attack"    select="'攻击性'"/>
  <xsl:variable name="lang.ATT"      select="'ATT'"/>
  <xsl:variable name="lang.Attribute"    select="'属性'"/>
  <xsl:variable name="lang.Attributes"  select="'属性'"/>
  <xsl:variable name="lang.Available"    select="'可获得性'"/>
  <xsl:variable name="lang.Awakened"    select="'Awakened'"/>
  <xsl:variable name="lang.Aware"    select="'Aware'"/>
  <xsl:variable name="lang.Background"  select="'背景'"/>
  <xsl:variable name="lang.Base"      select="'基础'"/>
  <xsl:variable name="lang.Bioware"    select="'生体改造'"/>
  <xsl:variable name="lang.BOD"      select="'BOD'"/>
  <xsl:variable name="lang.Body"      select="'体质'"/>
  <xsl:variable name="lang.Bonus"      select="'Bonus'"/>
  <xsl:variable name="lang.Bound"      select="'被缚精魂'"/>
  <xsl:variable name="lang.Calendar"    select="'日程表'"/>
  <xsl:variable name="lang.Career"    select="'职业'"/>
  <xsl:variable name="lang.Category"    select="'类别'"/>
  <xsl:variable name="lang.CHA"      select="'CHA'"/>
  <xsl:variable name="lang.Charisma"    select="'魅力'"/>
  <xsl:variable name="lang.CM"      select="'CM'"/>
  <xsl:variable name="lang.Cold"      select="'Cold'"/>
  <xsl:variable name="lang.Combat"    select="'战斗法术'"/>
  <xsl:variable name="lang.Commlink"    select="'矩阵设备'"/>
  <xsl:variable name="lang.Composure"    select="'沉着'"/>
  <xsl:variable name="lang.Concept"    select="'概念'"/>
  <xsl:variable name="lang.Connection"  select="'联系'"/>
  <xsl:variable name="lang.Contact"    select="'人脉'"/>
  <xsl:variable name="lang.ContactDrug"    select="'触'"/>
  <xsl:variable name="lang.Contacts"    select="'人脉'"/>
  <xsl:variable name="lang.Cost"      select="'售价'"/>
  <xsl:variable name="lang.Critter"    select="'怪物'"/>
  <xsl:variable name="lang.Critters"    select="'怪物'"/>
  <xsl:variable name="lang.Cyberware"    select="'赛博改造'"/>
  <xsl:variable name="lang.Damage"    select="'伤害'"/>
  <xsl:variable name="lang.Data"      select="'数据'"/>
  <xsl:variable name="lang.Date"      select="'日期'"/>
  <xsl:variable name="lang.Day"      select="'天'"/>
  <xsl:variable name="lang.Days"      select="'天'"/>
  <xsl:variable name="lang.Dead"      select="'死亡'"/>
  <xsl:variable name="lang.Defense"      select="'Defense'"/>
  <xsl:variable name="lang.DEP"  select="'DEP'"/>
  <xsl:variable name="lang.Depth"  select="'深度'"/>
  <xsl:variable name="lang.Description"  select="'形象描述'"/>
  <xsl:variable name="lang.Detection"    select="'侦测法术'"/>
  <xsl:variable name="lang.Device"    select="'设备'"/>
  <xsl:variable name="lang.Devices"    select="'设备'"/>
  <xsl:variable name="lang.Direct"      select="'Direct'"/>
  <xsl:variable name="lang.Down"      select="'倒'"/>
  <xsl:variable name="lang.DP"        select="'DP'"/>
  <xsl:variable name="lang.Drain"      select="'耗竭'"/>
  <xsl:variable name="lang.Drone"      select="'无人机'"/>
  <xsl:variable name="lang.Duration"    select="'持续'"/>
  <xsl:variable name="lang.DV"      select="'DV'"/>
  <xsl:variable name="lang.E"        select="'E'"/>
  <xsl:variable name="lang.Echo"    select="'Echo'"/>
  <xsl:variable name="lang.Echoes"    select="'回声'"/>
  <xsl:variable name="lang.EDG"      select="'EDG'"/>
  <xsl:variable name="lang.Edge"      select="'极限'"/>
  <xsl:variable name="lang.Electricity"    select="'Electricity'"/>
  <xsl:variable name="lang.Enchanter"    select="'Enchanter'"/>
  <xsl:variable name="lang.Enemies"    select="'敌人'"/>
  <xsl:variable name="lang.Enhancements"  select="'增强'"/>
  <xsl:variable name="lang.Entries"    select="'记录'"/>
  <xsl:variable name="lang.Equipped"    select="'已装备'"/>
  <xsl:variable name="lang.ESS"      select="'ESS'"/>
  <xsl:variable name="lang.Essence"    select="'精华'"/>
  <xsl:variable name="lang.Expenses"    select="'花销'"/>
  <xsl:variable name="lang.Explorer"    select="'Explorer'"/>
  <xsl:variable name="lang.Eyes"      select="'瞳色'"/>
  <xsl:variable name="lang.Falling"    select="'Falling'"/>
  <xsl:variable name="lang.Fatigue"      select="'Fatigue'"/>
  <xsl:variable name="lang.Fettered"      select="'Fettered'"/>
  <xsl:variable name="lang.Fire"    select="'Fire'"/>
  <xsl:variable name="lang.Firewall"    select="'防火墙'"/>
  <xsl:variable name="lang.Fly"      select="'飞行'"/>
  <xsl:variable name="lang.Foci"      select="'Foci'"/>
  <xsl:variable name="lang.FWL"      select="'FWL'"/>
  <xsl:variable name="lang.Force"      select="'强度'"/>
  <xsl:variable name="lang.FV"      select="'FV'"/>
  <xsl:variable name="lang.Gear"      select="'装备'"/>
  <xsl:variable name="lang.Gender"      select="'性别'"/>
  <xsl:variable name="lang.Grade"      select="'品级'"/>
  <xsl:variable name="lang.Hair"      select="'发色'"/>
  <xsl:variable name="lang.Handling"    select="'操纵性'"/>
  <xsl:variable name="lang.Health"    select="'生命法术'"/>
  <xsl:variable name="lang.Heavy"    select="'Heavy'"/>
  <xsl:variable name="lang.Height"    select="'身高'"/>
  <xsl:variable name="lang.hit"      select="'成功数'"/>
  <xsl:variable name="lang.Illusion"    select="'幻象法术'"/>
  <xsl:variable name="lang.Implant"    select="'植入'"/>
  <xsl:variable name="lang.Indirect"      select="'Indirect'"/>
  <xsl:variable name="lang.Info"      select="'信息'"/>
  <xsl:variable name="lang.Ingestion"      select="'Ingestion'"/>
  <xsl:variable name="lang.Inhalation"      select="'Inhalation'"/>
  <xsl:variable name="lang.Init"      select="'Init'"/>
  <xsl:variable name="lang.Initiation"  select="'启蒙'"/>
  <xsl:variable name="lang.Initiative"  select="'主动性'"/>
  <xsl:variable name="lang.Injection"      select="'Injection'"/>
  <xsl:variable name="lang.INT"      select="'INT'"/>
  <xsl:variable name="lang.Intentions"  select="'察言观色'"/>
  <xsl:variable name="lang.Intuition"    select="'直觉'"/>
  <xsl:variable name="lang.Instantaneous"  select="'即时法术'"/>
  <xsl:variable name="lang.Karma"      select="'业力'"/>
  <xsl:variable name="lang.L"        select="'L'"/>
  <xsl:variable name="lang.Level"      select="'级别'"/>
  <xsl:variable name="lang.Lifestyle"    select="'生活方式'"/>
  <xsl:variable name="lang.Limit"      select="'上限'"/>
  <xsl:variable name="lang.Limits"    select="'界限'"/>
  <xsl:variable name="lang.Loaded"    select="'已装'"/>
  <xsl:variable name="lang.Location"    select="'位置'"/>
  <xsl:variable name="lang.LOG"      select="'LOG'"/>
  <xsl:variable name="lang.Logic"      select="'逻辑'"/>
  <xsl:variable name="lang.Loyalty"    select="'忠诚'"/>
  <xsl:variable name="lang.M"        select="'M'"/>
  <xsl:variable name="lang.MAG"      select="'MAG'"/>
  <xsl:variable name="lang.Magician"      select="'Magician'"/>
  <xsl:variable name="lang.Magic"      select="'魔法'"/>
  <xsl:variable name="lang.Mana"    select="'Mana'"/>
  <xsl:variable name="lang.Maneuvers"    select="'战术'"/>
  <xsl:variable name="lang.Manipulation"  select="'操纵法术'"/>
  <xsl:variable name="lang.Manual"    select="'Manual'"/>
  <xsl:variable name="lang.Memory"    select="'记忆'"/>
  <xsl:variable name="lang.Mental"    select="'精神'"/>
  <xsl:variable name="lang.Metamagics"  select="'超魔'"/>
  <xsl:variable name="lang.Metatype"    select="'泛形态'"/>
  <xsl:variable name="lang.Mod"      select="'改造'"/>
  <xsl:variable name="lang.Mode"      select="'模式'"/>
  <xsl:variable name="lang.Model"      select="'模块'"/>
  <xsl:variable name="lang.Modifications"  select="'改造'"/>
  <xsl:variable name="lang.Month"      select="'月'"/>
  <xsl:variable name="lang.Months"    select="'月'"/>
  <xsl:variable name="lang.Mount"    select="'Mount'"/>
  <xsl:variable name="lang.Movement"    select="'移动速度'"/>
  <xsl:variable name="lang.Mugshot"    select="'肖像'"/>
  <xsl:variable name="lang.Name"      select="'姓名'"/>
  <xsl:variable name="lang.Native"    select="'母语'"/>
  <xsl:variable name="lang.Negative"    select="'Negative'"/>
  <xsl:variable name="lang.No"      select="'不'"/>
  <xsl:variable name="lang.None"      select="'None'"/>
  <xsl:variable name="lang.Notes"      select="'备注'"/>
  <xsl:variable name="lang.Notoriety"    select="'恶名'"/>
  <xsl:variable name="lang.Nuyen"      select="'新円'"/>
  <xsl:variable name="lang.Other"      select="'其他'"/>
  <xsl:variable name="lang.Overflow"      select="'Overflow'"/>
  <xsl:variable name="lang.OVR"      select="'溢&#160;'"/>
  <xsl:variable name="lang.Pathogen"    select="'Pathogen'"/>
  <xsl:variable name="lang.Permanent"    select="'永久法术'"/>
  <xsl:variable name="lang.Persona"    select="'化身'"/>
  <xsl:variable name="lang.Pets"      select="'Pets'"/>
  <xsl:variable name="lang.Physical"    select="'物理'"/>
  <xsl:variable name="lang.Physiological"  select="'Physiological'"/>
  <xsl:variable name="lang.Pilot"      select="'自驾系统'"/>
  <xsl:variable name="lang.Player"    select="'玩家'"/>
  <xsl:variable name="lang.Points"    select="'能力点'"/>
  <xsl:variable name="lang.Pool"      select="'骰池'"/>
  <xsl:variable name="lang.Positive"    select="'Positive'"/>
  <xsl:variable name="lang.Power"      select="'异能'"/>
  <xsl:variable name="lang.Powers"    select="'异能'"/>
  <xsl:variable name="lang.Priorities"  select="'优先级'"/>
  <xsl:variable name="lang.Processor"    select="'程序'"/>
  <xsl:variable name="lang.Program"    select="'程序'"/>
  <xsl:variable name="lang.Programs"    select="'程序'"/>
  <xsl:variable name="lang.Psychological"  select="'Psychological'"/>
  <xsl:variable name="lang.Qty"      select="'数量'"/>
  <xsl:variable name="lang.Quality"    select="'数量'"/>
  <xsl:variable name="lang.Qualities"    select="'特质'"/>
  <xsl:variable name="lang.Radiation"      select="'Radiation'"/>
  <xsl:variable name="lang.Range"      select="'范围'"/>
  <xsl:variable name="lang.Rating"    select="'等级'"/>
  <xsl:variable name="lang.RC"      select="'RC'"/>
  <xsl:variable name="lang.Reaction"    select="'反应'"/>
  <xsl:variable name="lang.REA"      select="'REA'"/>
  <xsl:variable name="lang.Reach"      select="'触及'"/>
  <xsl:variable name="lang.Reason"    select="'原因'"/>
  <xsl:variable name="lang.Registered"  select="'注册网精'"/>
  <xsl:variable name="lang.Requires"    select="'要求'"/>
  <xsl:variable name="lang.RES"      select="'RES'"/>
  <xsl:variable name="lang.Resistance"    select="'Resistance'"/>
  <xsl:variable name="lang.Resistances"    select="'Resistances'"/>
  <xsl:variable name="lang.Resonance"    select="'共鸣'"/>
  <xsl:variable name="lang.Resources"    select="'资金'"/>
  <xsl:variable name="lang.Rigger"    select="'机师'"/>
  <xsl:variable name="lang.Rtg"      select="'等级'"/>
  <xsl:variable name="lang.Run"      select="'狂奔'"/>
  <xsl:variable name="lang.S"        select="'S'"/>
  <xsl:variable name="lang.Seats"      select="'座位'"/>
  <xsl:variable name="lang.Self"      select="'Self'"/>
  <xsl:variable name="lang.Services"    select="'服务'"/>
  <xsl:variable name="lang.Sensor"    select="'传感器'"/>
  <xsl:variable name="lang.Sex"      select="'性别'"/>
  <xsl:variable name="lang.Show"      select="'Show: '"/>
  <xsl:variable name="lang.Skill"      select="'技能'"/>
  <xsl:variable name="lang.Skills"    select="'技能'"/>
  <xsl:variable name="lang.Skin"      select="'肤色'"/>
  <xsl:variable name="lang.Sleaze"    select="'隐匿性'"/>
  <xsl:variable name="lang.SLZ"      select="'SLZ'"/>
  <xsl:variable name="lang.Social"    select="'社交'"/>
  <xsl:variable name="lang.Sonic"      select="'Sonic'"/>
  <xsl:variable name="lang.Source"    select="'来源'"/>
  <xsl:variable name="lang.Special"    select="'特'"/>
  <xsl:variable name="lang.Speed"      select="'速度'"/>
  <xsl:variable name="lang.Spell"      select="'法术'"/>
  <xsl:variable name="lang.Spells"    select="'法术'"/>
  <xsl:variable name="lang.Spirit"    select="'精魂'"/>
  <xsl:variable name="lang.Spirits"    select="'精魂'"/>
  <xsl:variable name="lang.Sprite"    select="'网精'"/>
  <xsl:variable name="lang.Sprites"    select="'网精'"/>
  <xsl:variable name="lang.Standard"    select="'Standard'"/>
  <xsl:variable name="lang.Stream"    select="'Stream'"/>
  <xsl:variable name="lang.STR"      select="'STR'"/>
  <xsl:variable name="lang.Strength"    select="'力量'"/>
  <xsl:variable name="lang.Stun"      select="'晕眩'"/>
  <xsl:variable name="lang.Submersion"  select="'Submersion'"/>
  <xsl:variable name="lang.Sustained"    select="'持续法术'"/>
  <xsl:variable name="lang.Swim"      select="'游泳'"/>
  <xsl:variable name="lang.Target"    select="'目标'"/>
  <xsl:variable name="lang.Tasks"    select="'Tasks'"/>
  <xsl:variable name="lang.Total"      select="'总计'"/>
  <xsl:variable name="lang.Touch"      select="'接触法术'"/>
  <xsl:variable name="lang.Toxin"      select="'Toxin'"/>
  <xsl:variable name="lang.Tradition"    select="'法术流派'"/>
  <xsl:variable name="lang.Type"      select="'类别'"/>
  <xsl:variable name="lang.Unbound"    select="'无拘精魂'"/>
  <xsl:variable name="lang.Unknown"    select="'未知'"/>
  <xsl:variable name="lang.Unregistered"  select="'未注册网精'"/>
  <xsl:variable name="lang.Under"      select="'下挂'"/>
  <xsl:variable name="lang.Vehicle"    select="'载具'"/>
  <xsl:variable name="lang.Vehicles"    select="'载具'"/>
  <xsl:variable name="lang.VR"      select="'VR'"/>
  <xsl:variable name="lang.W"        select="'W'"/>
  <xsl:variable name="lang.Walk"      select="'行走'"/>
  <xsl:variable name="lang.Weaknesses"    select="'Weaknesses'"/>
  <xsl:variable name="lang.Weapon"    select="'武器'"/>
  <xsl:variable name="lang.Weapons"    select="'武器'"/>
  <xsl:variable name="lang.Week"      select="'周'"/>
  <xsl:variable name="lang.Weeks"      select="'周'"/>
  <xsl:variable name="lang.Weight"    select="'重量'"/>
  <xsl:variable name="lang.WIL"      select="'WIL'"/>
  <xsl:variable name="lang.Willpower"    select="'意志'"/>
  <xsl:variable name="lang.with"      select="'和'"/>
  <xsl:variable name="lang.Yes"      select="'是'"/>

  <!-- multiple word phrases / composite words -->
  <xsl:variable name="lang.ActiveSkills"    select="'行动技能'"/>
  <xsl:variable name="lang.AdeptPowers"    select="'修士异能'"/>
  <xsl:variable name="lang.AIandAdvanced"    select="'AI程序和高级程序'"/>
  <xsl:variable name="lang.AlreadyAddicted"  select="'Already Addicted'"/>
  <xsl:variable name="lang.ArmorValue"    select="'护甲值'"/>
  <xsl:variable name="lang.AspectedMagician"    select="'Aspected Magician'"/>
  <xsl:variable name="lang.AstralInitiative"  select="'星界主动性'"/>
  <xsl:variable name="lang.AstralReputation"    select="'Astral Reputation'"/>
  <xsl:variable name="lang.CombatSkill"    select="'战斗法术'"/>
  <xsl:variable name="lang.ComplexForm"    select="'复合程式'"/>
  <xsl:variable name="lang.ComplexForms"    select="'复合程式'"/>
  <xsl:variable name="lang.ConditionMonitor"  select="'CM'"/>
  <xsl:variable name="lang.ContactList"    select="'人脉表'"/>
  <xsl:variable name="lang.CoreTrack"  select="'Core Damage Track'"/>
  <xsl:variable name="lang.CritterPower"    select="'Critter Power'"/>
  <xsl:variable name="lang.CritterPowers"    select="'怪物能力'"/>
  <xsl:variable name="lang.CurrentEdge"    select="'当前极限点'"/>
  <xsl:variable name="lang.CurrentForm"    select="'当前形'"/>
  <xsl:variable name="lang.DamageType"  select="'Damage Type'"/>
  <xsl:variable name="lang.DataProc"      select="'数据处理'"/>
  <xsl:variable name="lang.DataProcessing"  select="'数据处理'"/>
  <xsl:variable name="lang.DecreaseAttribute"    select="'Decrease Attribute'"/>
  <xsl:variable name="lang.DerivedAttributes"  select="'Derived Attributes'"/>
  <xsl:variable name="lang.DeviceRating"    select="'DR'"/>
  <xsl:variable name="lang.FadingValue"    select="'衰褪值'"/>
  <xsl:variable name="lang.HobbiesVice"    select="'Hobbies/Vice'"/>
  <xsl:variable name="lang.IDcredsticks"    select="'ID/信用棒'"/>
  <xsl:variable name="lang.InitiateGrade"    select="'启蒙阶层'"/>
  <xsl:variable name="lang.InitiationNotes"  select="'Initiation Grade Notes'"/>
  <xsl:variable name="lang.JudgeIntentions"  select="'察言观色'"/>
  <xsl:variable name="lang.KnowledgeSkills"  select="'知识技能'"/>
  <xsl:variable name="lang.LiftCarry"      select="'举重/负重'"/>
  <xsl:variable name="lang.LineofSight"    select="'视线内'"/>
  <xsl:variable name="lang.LinkedSIN"      select="'关联SIN'"/>
  <xsl:variable name="lang.MartialArt"    select="'武术流派'"/>
  <xsl:variable name="lang.MartialArts"    select="'武术'"/>
  <xsl:variable name="lang.MatrixAR"      select="'矩阵 AR'"/>
  <xsl:variable name="lang.MatrixCold"    select="'矩阵（冷模）'"/>
  <xsl:variable name="lang.MatrixDevices"    select="'Matrix Devices'"/>
  <xsl:variable name="lang.MatrixHot"      select="'矩阵（热模）'"/>
  <xsl:variable name="lang.MatrixTrack"    select="'矩阵伤害'"/>
  <xsl:variable name="lang.MeleeWeapons"    select="'近战武器'"/>
  <xsl:variable name="lang.MentalAttributes"  select="'精神属性'"/>
  <xsl:variable name="lang.MysticAdept"    select="'Mystic Adept'"/>
  <xsl:variable name="lang.NotAddictedYet"  select="'Not Addicted Yet'"/>
  <xsl:variable name="lang.Nothing2Show4Devices"    select="'No Devices to list'"/>
  <xsl:variable name="lang.Nothing2Show4Notes"    select="'No Notes to list'"/>
  <xsl:variable name="lang.Nothing2Show4SpiritsSprites"    select="'No Spirits/Sprites to list'"/>
  <xsl:variable name="lang.Nothing2Show4Vehicles"    select="'No Vehicles to list'"/>
  <xsl:variable name="lang.OptionalPowers"    select="'Optional Powers'"/>
  <xsl:variable name="lang.OtherArmor"      select="'其他护甲'"/>
  <xsl:variable name="lang.OtherMugshots"    select="'其他肖像'"/>
  <xsl:variable name="lang.PageBreak"      select="'Page Break: '"/>
  <xsl:variable name="lang.PersonalData"    select="'个人信息'"/>
  <xsl:variable name="lang.PersonalLife"    select="'Personal Life'"/>
  <xsl:variable name="lang.PhysicalAttributes"  select="'物理属性'"/>
  <xsl:variable name="lang.PhysicalNaturalRecovery"  select="'Natural Recovery Pool (1 day)'"/>
  <xsl:variable name="lang.PhysicalTrack"  select="'物理伤害'"/>
  <xsl:variable name="lang.PreferredPayment"    select="'Preferred Payment Method'"/>
  <xsl:variable name="lang.PrimaryArm"    select="'利手'"/>
  <xsl:variable name="lang.PublicAwareness"  select="'名气'"/>
  <xsl:variable name="lang.RangedWeapons"    select="'远程武器'"/>
  <xsl:variable name="lang.RemainingAvailable"  select="'剩余可用'"/>
  <xsl:variable name="lang.ResistDrain"    select="'抵抗耗竭'"/>
  <xsl:variable name="lang.ResistFading"    select="'抵抗衰褪'"/>
  <xsl:variable name="lang.RiggerInitiative"  select="'机师主动性'"/>
  <xsl:variable name="lang.SelectedGear"    select="'Selected Gear'"/>
  <xsl:variable name="lang.SkillGroup"    select="'技能组'"/>
  <xsl:variable name="lang.SkillGroups"    select="'技能组'"/>
  <xsl:variable name="lang.SpecialAttributes"  select="'特殊属性'"/>
  <xsl:variable name="lang.StreetCred"    select="'信誉'"/>
  <xsl:variable name="lang.StreetName"    select="'Street Name'"/>
  <xsl:variable name="lang.StunNaturalRecovery"  select="'Natural Recovery Pool (1 hour)'"/>
  <xsl:variable name="lang.StunTrack"    select="'晕眩伤害'"/>
  <xsl:variable name="lang.SubmersionGrade"  select="'深潜阶层'"/>
  <xsl:variable name="lang.SubmersionNotes"  select="'Submersion Notes'"/>
  <xsl:variable name="lang.ToggleColors"  select="'Toggle Colors'"/>
  <xsl:variable name="lang.TotalArmor"  select="'Total of equipped single highest armor and accessories'"/>
  <xsl:variable name="lang.ToxinsAndPathogens"  select="'Toxins and Pathogens'"/>
  <xsl:variable name="lang.UnnamedCharacter"  select="'未命名角色'"/>
  <xsl:variable name="lang.VehicleBody"    select="'机体'"/>
  <xsl:variable name="lang.VehicleCost"    select="'载具售价'"/>
  <xsl:variable name="lang.WildReputation"    select="'Wild Reputation'"/>

  <!-- "limits" list -->
  <xsl:variable name="lang.AstralLimit"    select="'星界界限'"/>
  <xsl:variable name="lang.MentalLimit"    select="'精神界限'"/>
  <xsl:variable name="lang.PhysicalLimit"    select="'肉体界限'"/>
  <xsl:variable name="lang.SocialLimit"    select="'社交界限'"/>

  <!-- spell types list -->
  <xsl:variable name="lang.CombatSpells"    select="'战斗法术'"/>
  <xsl:variable name="lang.DetectionSpells"  select="'侦测法术'"/>
  <xsl:variable name="lang.Enchantments"    select="'附魔'"/>
  <xsl:variable name="lang.HealthSpells"     select="'生命法术'"/>
  <xsl:variable name="lang.IllusionSpells"   select="'幻象法术'"/>
  <xsl:variable name="lang.ManipulationSpells" select="'操纵法术'"/>
  <xsl:variable name="lang.Rituals"      select="'仪式'"/>

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
