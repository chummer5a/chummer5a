<?xml version="1.0" encoding="utf-8" ?>
<!-- Isolate Chinese (zh) locale literals -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="lang"  select="string('zh')" />
  <xsl:variable name="locale"  select="string('zh-cn')" />

  <!-- individual words -->
  <xsl:variable name="lang.Acceleration"  select="string('加速')" />
  <xsl:variable name="lang.Accel"      select="string('加速')" />
  <xsl:variable name="lang.Accessories"  select="string('配件')" />
  <xsl:variable name="lang.Accuracy"    select="string('准度')" />
  <xsl:variable name="lang.Acid"      select="string('Acid')" />
  <xsl:variable name="lang.Action"      select="string('Action')" />
  <xsl:variable name="lang.Addiction"  select="string('Addiction')" />
  <xsl:variable name="lang.Adept"      select="string('Adept')" />
  <xsl:variable name="lang.Age"      select="string('年龄')" />
  <xsl:variable name="lang.AGI"      select="string('AGI')" />
  <xsl:variable name="lang.Agility"    select="string('敏捷')" />
  <xsl:variable name="lang.AI"      select="string('AI')" />
  <xsl:variable name="lang.Alias"      select="string('绰号')" />
  <xsl:variable name="lang.Ammo"      select="string('弹药')" />
  <xsl:variable name="lang.Amount"    select="string('数量')" />
  <xsl:variable name="lang.AP"      select="string('AP')" />
  <xsl:variable name="lang.Applicable"  select="string('可用')" />
  <xsl:variable name="lang.Apprentice"  select="string('Apprentice')" />
  <xsl:variable name="lang.AR"      select="string('AR')" />
  <xsl:variable name="lang.Archetype"    select="string('职业')" />
  <xsl:variable name="lang.Area"      select="string('范围')" />
  <xsl:variable name="lang.Armor"      select="string('护甲')" />
  <xsl:variable name="lang.Arts"      select="string('技艺')" />
  <xsl:variable name="lang.as"      select="string('作为')" />
  <xsl:variable name="lang.ASDF"    select="string('A/S/D/F')" />
  <xsl:variable name="lang.Astral"    select="string('星界')" />
  <xsl:variable name="lang.Attack"    select="string('攻击性')" />
  <xsl:variable name="lang.ATT"      select="string('ATT')" />
  <xsl:variable name="lang.Attribute"    select="string('属性')" />
  <xsl:variable name="lang.Attributes"  select="string('属性')" />
  <xsl:variable name="lang.Available"    select="string('可获得性')" />
  <xsl:variable name="lang.Awakened"    select="string('Awakened')" />
  <xsl:variable name="lang.Aware"    select="string('Aware')" />
  <xsl:variable name="lang.Background"  select="string('背景')" />
  <xsl:variable name="lang.Base"      select="string('基础')" />
  <xsl:variable name="lang.Bioware"    select="string('生体改造')" />
  <xsl:variable name="lang.BOD"      select="string('BOD')" />
  <xsl:variable name="lang.Body"      select="string('体质')" />
  <xsl:variable name="lang.Bonus"      select="string('Bonus')" />
  <xsl:variable name="lang.Bound"      select="string('被缚精魂')" />
  <xsl:variable name="lang.Calendar"    select="string('日程表')" />
  <xsl:variable name="lang.Career"    select="string('职业')" />
  <xsl:variable name="lang.Category"    select="string('类别')" />
  <xsl:variable name="lang.CHA"      select="string('CHA')" />
  <xsl:variable name="lang.Charisma"    select="string('魅力')" />
  <xsl:variable name="lang.CM"      select="string('CM')" />
  <xsl:variable name="lang.Cold"      select="string('Cold')" />
  <xsl:variable name="lang.Combat"    select="string('战斗法术')" />
  <xsl:variable name="lang.Commlink"    select="string('矩阵设备')" />
  <xsl:variable name="lang.Composure"    select="string('沉着')" />
  <xsl:variable name="lang.Concept"    select="string('概念')" />
  <xsl:variable name="lang.Connection"  select="string('联系')" />
  <xsl:variable name="lang.Contact"    select="string('人脉')" />
  <xsl:variable name="lang.ContactDrug"    select="string('触')" />
  <xsl:variable name="lang.Contacts"    select="string('人脉')" />
  <xsl:variable name="lang.Cost"      select="string('售价')" />
  <xsl:variable name="lang.Critter"    select="string('怪物')" />
  <xsl:variable name="lang.Critters"    select="string('怪物')" />
  <xsl:variable name="lang.Cyberware"    select="string('赛博改造')" />
  <xsl:variable name="lang.Damage"    select="string('伤害')" />
  <xsl:variable name="lang.Data"      select="string('数据')" />
  <xsl:variable name="lang.Date"      select="string('日期')" />
  <xsl:variable name="lang.Day"      select="string('天')" />
  <xsl:variable name="lang.Days"      select="string('天')" />
  <xsl:variable name="lang.Dead"      select="string('死亡')" />
  <xsl:variable name="lang.Defense"      select="string('Defense')" />
  <xsl:variable name="lang.DEP"  select="string('DEP')" />
  <xsl:variable name="lang.Depth"  select="string('深度')" />
  <xsl:variable name="lang.Description"  select="string('形象描述')" />
  <xsl:variable name="lang.Detection"    select="string('侦测法术')" />
  <xsl:variable name="lang.Device"    select="string('设备')" />
  <xsl:variable name="lang.Devices"    select="string('设备')" />
  <xsl:variable name="lang.Direct"      select="string('Direct')" />
  <xsl:variable name="lang.Down"      select="string('倒')" />
  <xsl:variable name="lang.DP"        select="string('DP')" />
  <xsl:variable name="lang.Drain"      select="string('耗竭')" />
  <xsl:variable name="lang.Drone"      select="string('无人机')" />
  <xsl:variable name="lang.Duration"    select="string('持续')" />
  <xsl:variable name="lang.DV"      select="string('DV')" />
  <xsl:variable name="lang.E"        select="string('E')" />
  <xsl:variable name="lang.Echo"    select="string('Echo')" />
  <xsl:variable name="lang.Echoes"    select="string('回声')" />
  <xsl:variable name="lang.EDG"      select="string('EDG')" />
  <xsl:variable name="lang.Edge"      select="string('极限')" />
  <xsl:variable name="lang.Electricity"    select="string('Electricity')" />
  <xsl:variable name="lang.Enchanter"    select="string('Enchanter')" />
  <xsl:variable name="lang.Enemies"    select="string('敌人')" />
  <xsl:variable name="lang.Enhancements"  select="string('增强')" />
  <xsl:variable name="lang.Entries"    select="string('记录')" />
  <xsl:variable name="lang.Equipped"    select="string('已装备')" />
  <xsl:variable name="lang.ESS"      select="string('ESS')" />
  <xsl:variable name="lang.Essence"    select="string('精华')" />
  <xsl:variable name="lang.Expenses"    select="string('花销')" />
  <xsl:variable name="lang.Explorer"    select="string('Explorer')" />
  <xsl:variable name="lang.Eyes"      select="string('瞳色')" />
  <xsl:variable name="lang.Falling"    select="string('Falling')" />
  <xsl:variable name="lang.Fatigue"      select="string('Fatigue')" />
  <xsl:variable name="lang.Fettered"      select="string('Fettered')" />
  <xsl:variable name="lang.Fire"    select="string('Fire')" />
  <xsl:variable name="lang.Firewall"    select="string('防火墙')" />
  <xsl:variable name="lang.Fly"      select="string('飞行')" />
  <xsl:variable name="lang.Foci"      select="string('Foci')" />
  <xsl:variable name="lang.FWL"      select="string('FWL')" />
  <xsl:variable name="lang.Force"      select="string('强度')" />
  <xsl:variable name="lang.FV"      select="string('FV')" />
  <xsl:variable name="lang.Gear"      select="string('装备')" />
  <xsl:variable name="lang.Gender"      select="string('性别')" />
  <xsl:variable name="lang.Grade"      select="string('品级')" />
  <xsl:variable name="lang.Hair"      select="string('发色')" />
  <xsl:variable name="lang.Handling"    select="string('操纵性')" />
  <xsl:variable name="lang.Health"    select="string('生命法术')" />
  <xsl:variable name="lang.Heavy"    select="string('Heavy')" />
  <xsl:variable name="lang.Height"    select="string('身高')" />
  <xsl:variable name="lang.hit"      select="string('成功数')" />
  <xsl:variable name="lang.Illusion"    select="string('幻象法术')" />
  <xsl:variable name="lang.Implant"    select="string('植入')" />
  <xsl:variable name="lang.Indirect"      select="string('Indirect')" />
  <xsl:variable name="lang.Info"      select="string('信息')" />
  <xsl:variable name="lang.Ingestion"      select="string('Ingestion')" />
  <xsl:variable name="lang.Inhalation"      select="string('Inhalation')" />
  <xsl:variable name="lang.Init"      select="string('Init')" />
  <xsl:variable name="lang.Initiation"  select="string('启蒙')" />
  <xsl:variable name="lang.Initiative"  select="string('主动性')" />
  <xsl:variable name="lang.Injection"      select="string('Injection')" />
  <xsl:variable name="lang.INT"      select="string('INT')" />
  <xsl:variable name="lang.Intentions"  select="string('察言观色')" />
  <xsl:variable name="lang.Intuition"    select="string('直觉')" />
  <xsl:variable name="lang.Instantaneous"  select="string('即时法术')" />
  <xsl:variable name="lang.Karma"      select="string('业力')" />
  <xsl:variable name="lang.L"        select="string('L')" />
  <xsl:variable name="lang.Level"      select="string('级别')" />
  <xsl:variable name="lang.Lifestyle"    select="string('生活方式')" />
  <xsl:variable name="lang.Limit"      select="string('上限')" />
  <xsl:variable name="lang.Limits"    select="string('界限')" />
  <xsl:variable name="lang.Loaded"    select="string('已装')" />
  <xsl:variable name="lang.Location"    select="string('位置')" />
  <xsl:variable name="lang.LOG"      select="string('LOG')" />
  <xsl:variable name="lang.Logic"      select="string('逻辑')" />
  <xsl:variable name="lang.Loyalty"    select="string('忠诚')" />
  <xsl:variable name="lang.M"        select="string('M')" />
  <xsl:variable name="lang.MAG"      select="string('MAG')" />
  <xsl:variable name="lang.Magician"      select="string('Magician')" />
  <xsl:variable name="lang.Magic"      select="string('魔法')" />
  <xsl:variable name="lang.Mana"    select="string('Mana')" />
  <xsl:variable name="lang.Maneuvers"    select="string('Maneuvers')" />
  <xsl:variable name="lang.Manipulation"  select="string('操纵法术')" />
  <xsl:variable name="lang.Manual"    select="string('Manual')" />
  <xsl:variable name="lang.Memory"    select="string('记忆')" />
  <xsl:variable name="lang.Mental"    select="string('精神')" />
  <xsl:variable name="lang.Metamagics"  select="string('超魔')" />
  <xsl:variable name="lang.Metatype"    select="string('泛形态')" />
  <xsl:variable name="lang.Mod"      select="string('改造')" />
  <xsl:variable name="lang.Mode"      select="string('模式')" />
  <xsl:variable name="lang.Model"      select="string('模块')" />
  <xsl:variable name="lang.Modifications"  select="string('改造')" />
  <xsl:variable name="lang.Month"      select="string('月')" />
  <xsl:variable name="lang.Months"    select="string('月')" />
  <xsl:variable name="lang.Mount"    select="string('Mount')" />
  <xsl:variable name="lang.Movement"    select="string('移动速度')" />
  <xsl:variable name="lang.Mugshot"    select="string('肖像')" />
  <xsl:variable name="lang.Name"      select="string('姓名')" />
  <xsl:variable name="lang.Native"    select="string('母语')" />
  <xsl:variable name="lang.Negative"    select="string('Negative')" />
  <xsl:variable name="lang.No"      select="string('不')" />
  <xsl:variable name="lang.None"      select="string('None')" />
  <xsl:variable name="lang.Notes"      select="string('备注')" />
  <xsl:variable name="lang.Notoriety"    select="string('恶名')" />
  <xsl:variable name="lang.Nuyen"      select="string('新円')" />
  <xsl:variable name="lang.Other"      select="string('其他')" />
  <xsl:variable name="lang.Overflow"      select="string('Overflow')" />
  <xsl:variable name="lang.OVR"      select="string('溢&#160;')" />
  <xsl:variable name="lang.Pathogen"    select="string('Pathogen')" />
  <xsl:variable name="lang.Permanent"    select="string('永久法术')" />
  <xsl:variable name="lang.Persona"    select="string('化身')" />
  <xsl:variable name="lang.Pets"      select="string('Pets')" />
  <xsl:variable name="lang.Physical"    select="string('物理')" />
  <xsl:variable name="lang.Physiological"  select="string('Physiological')" />
  <xsl:variable name="lang.Pilot"      select="string('自驾系统')" />
  <xsl:variable name="lang.Player"    select="string('玩家')" />
  <xsl:variable name="lang.Points"    select="string('能力点')" />
  <xsl:variable name="lang.Pool"      select="string('骰池')" />
  <xsl:variable name="lang.Positive"    select="string('Positive')" />
  <xsl:variable name="lang.Power"      select="string('异能')" />
  <xsl:variable name="lang.Powers"    select="string('异能')" />
  <xsl:variable name="lang.Priorities"  select="string('优先级')" />
  <xsl:variable name="lang.Processor"    select="string('程序')" />
  <xsl:variable name="lang.Program"    select="string('程序')" />
  <xsl:variable name="lang.Programs"    select="string('程序')" />
  <xsl:variable name="lang.Psychological"  select="string('Psychological')" />
  <xsl:variable name="lang.Qty"      select="string('数量')" />
  <xsl:variable name="lang.Quality"    select="string('数量')" />
  <xsl:variable name="lang.Qualities"    select="string('特质')" />
  <xsl:variable name="lang.Radiation"      select="string('Radiation')" />
  <xsl:variable name="lang.Range"      select="string('范围')" />
  <xsl:variable name="lang.Rating"    select="string('等级')" />
  <xsl:variable name="lang.RC"      select="string('RC')" />
  <xsl:variable name="lang.Reaction"    select="string('反应')" />
  <xsl:variable name="lang.REA"      select="string('REA')" />
  <xsl:variable name="lang.Reach"      select="string('触及')" />
  <xsl:variable name="lang.Reason"    select="string('原因')" />
  <xsl:variable name="lang.Registered"  select="string('注册网精')" />
  <xsl:variable name="lang.Requires"    select="string('要求')" />
  <xsl:variable name="lang.RES"      select="string('RES')" />
  <xsl:variable name="lang.Resistance"    select="string('Resistance')" />
  <xsl:variable name="lang.Resistances"    select="string('Resistances')" />
  <xsl:variable name="lang.Resonance"    select="string('共鸣')" />
  <xsl:variable name="lang.Resources"    select="string('资金')" />
  <xsl:variable name="lang.Rigger"    select="string('机师')" />
  <xsl:variable name="lang.Rtg"      select="string('等级')" />
  <xsl:variable name="lang.Run"      select="string('狂奔')" />
  <xsl:variable name="lang.S"        select="string('S')" />
  <xsl:variable name="lang.Seats"      select="string('座位')" />
  <xsl:variable name="lang.Self"      select="string('Self')" />
  <xsl:variable name="lang.Services"    select="string('服务')" />
  <xsl:variable name="lang.Sensor"    select="string('传感器')" />
  <xsl:variable name="lang.Show"      select="string('Show: ')" />
  <xsl:variable name="lang.Skill"      select="string('技能')" />
  <xsl:variable name="lang.Skills"    select="string('技能')" />
  <xsl:variable name="lang.Skin"      select="string('肤色')" />
  <xsl:variable name="lang.Sleaze"    select="string('隐匿性')" />
  <xsl:variable name="lang.SLZ"      select="string('SLZ')" />
  <xsl:variable name="lang.Social"    select="string('社交')" />
  <xsl:variable name="lang.Sonic"      select="string('Sonic')" />
  <xsl:variable name="lang.Source"    select="string('来源')" />
  <xsl:variable name="lang.Special"    select="string('特')" />
  <xsl:variable name="lang.Speed"      select="string('速度')" />
  <xsl:variable name="lang.Spell"      select="string('法术')" />
  <xsl:variable name="lang.Spells"    select="string('法术')" />
  <xsl:variable name="lang.Spirit"    select="string('精魂')" />
  <xsl:variable name="lang.Spirits"    select="string('精魂')" />
  <xsl:variable name="lang.Sprite"    select="string('网精')" />
  <xsl:variable name="lang.Sprites"    select="string('网精')" />
  <xsl:variable name="lang.Standard"    select="string('Standard')" />
  <xsl:variable name="lang.Stream"    select="string('Stream')" />
  <xsl:variable name="lang.STR"      select="string('STR')" />
  <xsl:variable name="lang.Strength"    select="string('力量')" />
  <xsl:variable name="lang.Stun"      select="string('晕眩')" />
  <xsl:variable name="lang.Submersion"  select="string('Submersion')" />
  <xsl:variable name="lang.Sustained"    select="string('持续法术')" />
  <xsl:variable name="lang.Swim"      select="string('游泳')" />
  <xsl:variable name="lang.Target"    select="string('目标')" />
  <xsl:variable name="lang.Tasks"    select="string('Tasks')" />
  <xsl:variable name="lang.Total"      select="string('总计')" />
  <xsl:variable name="lang.Touch"      select="string('接触法术')" />
  <xsl:variable name="lang.Toxin"      select="string('Toxin')" />
  <xsl:variable name="lang.Tradition"    select="string('法术流派')" />
  <xsl:variable name="lang.Type"      select="string('类别')" />
  <xsl:variable name="lang.Unbound"    select="string('无拘精魂')" />
  <xsl:variable name="lang.Unknown"    select="string('未知')" />
  <xsl:variable name="lang.Unregistered"  select="string('未注册网精')" />
  <xsl:variable name="lang.Under"      select="string('下挂')" />
  <xsl:variable name="lang.Vehicle"    select="string('载具')" />
  <xsl:variable name="lang.Vehicles"    select="string('载具')" />
  <xsl:variable name="lang.VR"      select="string('VR')" />
  <xsl:variable name="lang.W"        select="string('W')" />
  <xsl:variable name="lang.Walk"      select="string('行走')" />
  <xsl:variable name="lang.Weaknesses"    select="string('Weaknesses')" />
  <xsl:variable name="lang.Weapon"    select="string('武器')" />
  <xsl:variable name="lang.Weapons"    select="string('武器')" />
  <xsl:variable name="lang.Week"      select="string('周')" />
  <xsl:variable name="lang.Weeks"      select="string('周')" />
  <xsl:variable name="lang.Weight"    select="string('重量')" />
  <xsl:variable name="lang.WIL"      select="string('WIL')" />
  <xsl:variable name="lang.Willpower"    select="string('意志')" />
  <xsl:variable name="lang.with"      select="string('和')" />
  <xsl:variable name="lang.Yes"      select="string('是')" />

  <!-- multiple word phrases / composite words -->
  <xsl:variable name="lang.ActiveSkills"    select="string('行动技能')" />
  <xsl:variable name="lang.AdeptPowers"    select="string('修士异能')" />
  <xsl:variable name="lang.AIandAdvanced"    select="string('AI程序和高级程序')" />
  <xsl:variable name="lang.AlreadyAddicted"  select="string('Already Addicted')" />
  <xsl:variable name="lang.ArmorValue"    select="string('护甲值')" />
  <xsl:variable name="lang.AspectedMagician"    select="string('Aspected Magician')" />
  <xsl:variable name="lang.AstralInitiative"  select="string('星界主动性')" />
  <xsl:variable name="lang.AstralReputation"    select="string('Astral Reputation')" />
  <xsl:variable name="lang.CombatSkill"    select="string('战斗法术')" />
  <xsl:variable name="lang.ComplexForm"    select="string('复合程式')" />
  <xsl:variable name="lang.ComplexForms"    select="string('复合程式')" />
  <xsl:variable name="lang.ConditionMonitor"  select="string('CM')" />
  <xsl:variable name="lang.ContactList"    select="string('人脉表')" />
  <xsl:variable name="lang.CoreTrack"  select="string('Core Damage Track')" />
  <xsl:variable name="lang.CritterPower"    select="string('Critter Power')" />
  <xsl:variable name="lang.CritterPowers"    select="string('怪物能力')" />
  <xsl:variable name="lang.CurrentEdge"    select="string('当前极限点')" />
  <xsl:variable name="lang.CurrentForm"    select="string('当前形')" />
  <xsl:variable name="lang.DamageType"  select="string('Damage Type')" />
  <xsl:variable name="lang.DataProc"      select="string('数据处理')" />
  <xsl:variable name="lang.DataProcessing"  select="string('数据处理')" />
  <xsl:variable name="lang.DecreaseAttribute"    select="string('Decrease Attribute')" />
  <xsl:variable name="lang.DerivedAttributes"  select="string('Derived Attributes')" />
  <xsl:variable name="lang.DeviceRating"    select="string('DR')" />
  <xsl:variable name="lang.FadingValue"    select="string('衰褪值')" />
  <xsl:variable name="lang.HobbiesVice"    select="string('Hobbies/Vice')" />
  <xsl:variable name="lang.IDcredsticks"    select="string('ID/信用棒')" />
  <xsl:variable name="lang.InitiateGrade"    select="string('启蒙阶层')" />
  <xsl:variable name="lang.InitiationNotes"  select="string('Initiation Grade Notes')" />
  <xsl:variable name="lang.JudgeIntentions"  select="string('察言观色')" />
  <xsl:variable name="lang.KnowledgeSkills"  select="string('知识技能')" />
  <xsl:variable name="lang.LiftCarry"      select="string('举重/负重')" />
  <xsl:variable name="lang.LineofSight"    select="string('视线内')" />
  <xsl:variable name="lang.LinkedSIN"      select="string('关联SIN')" />
  <xsl:variable name="lang.MartialArt"    select="string('武术流派')" />
  <xsl:variable name="lang.MartialArts"    select="string('武术')" />
  <xsl:variable name="lang.MatrixAR"      select="string('矩阵 AR')" />
  <xsl:variable name="lang.MatrixCold"    select="string('矩阵（冷模）')" />
  <xsl:variable name="lang.MatrixDevices"    select="string('Matrix Devices')" />
  <xsl:variable name="lang.MatrixHot"      select="string('矩阵（热模）')" />
  <xsl:variable name="lang.MatrixTrack"    select="string('矩阵伤害')" />
  <xsl:variable name="lang.MeleeWeapons"    select="string('近战武器')" />
  <xsl:variable name="lang.MentalAttributes"  select="string('精神属性')" />
  <xsl:variable name="lang.MysticAdept"    select="string('Mystic Adept')" />
  <xsl:variable name="lang.NotAddictedYet"  select="string('Not Addicted Yet')" />
  <xsl:variable name="lang.Nothing2Show4Devices"    select="string('No Devices to list')" />
  <xsl:variable name="lang.Nothing2Show4Notes"    select="string('No Notes to list')" />
  <xsl:variable name="lang.Nothing2Show4SpiritsSprites"    select="string('No Spirits/Sprites to list')" />
  <xsl:variable name="lang.Nothing2Show4Vehicles"    select="string('No Vehicles to list')" />
  <xsl:variable name="lang.OptionalPowers"    select="string('Optional Powers')" />
  <xsl:variable name="lang.OtherArmor"      select="string('其他护甲')" />
  <xsl:variable name="lang.OtherMugshots"    select="string('其他肖像')" />
  <xsl:variable name="lang.PageBreak"      select="string('Page Break: ')" />
  <xsl:variable name="lang.PersonalData"    select="string('个人信息')" />
  <xsl:variable name="lang.PersonalLife"    select="string('Personal Life')" />
  <xsl:variable name="lang.PhysicalAttributes"  select="string('物理属性')" />
  <xsl:variable name="lang.PhysicalNaturalRecovery"  select="string('Natural Recovery Pool (1 day)')" />
  <xsl:variable name="lang.PhysicalTrack"  select="string('物理伤害')" />
  <xsl:variable name="lang.PreferredPayment"    select="string('Preferred Payment Method')" />
  <xsl:variable name="lang.PrimaryArm"    select="string('利手')" />
  <xsl:variable name="lang.PublicAwareness"  select="string('名气')" />
  <xsl:variable name="lang.RangedWeapons"    select="string('远程武器')" />
  <xsl:variable name="lang.RemainingAvailable"  select="string('剩余可用')" />
  <xsl:variable name="lang.ResistDrain"    select="string('抵抗耗竭')" />
  <xsl:variable name="lang.ResistFading"    select="string('抵抗衰褪')" />
  <xsl:variable name="lang.RiggerInitiative"  select="string('机师主动性')" />
  <xsl:variable name="lang.SelectedGear"    select="string('Selected Gear')" />
  <xsl:variable name="lang.SkillGroup"    select="string('技能组')" />
  <xsl:variable name="lang.SkillGroups"    select="string('技能组')" />
  <xsl:variable name="lang.SpecialAttributes"  select="string('特殊属性')" />
  <xsl:variable name="lang.StreetCred"    select="string('信誉')" />
  <xsl:variable name="lang.StreetName"    select="string('Street Name')" />
  <xsl:variable name="lang.StunNaturalRecovery"  select="string('Natural Recovery Pool (1 hour)')" />
  <xsl:variable name="lang.StunTrack"    select="string('晕眩伤害')" />
  <xsl:variable name="lang.SubmersionGrade"  select="string('深潜阶层')" />
  <xsl:variable name="lang.SubmersionNotes"  select="string('Submersion Notes')" />
  <xsl:variable name="lang.ToggleColors"  select="string('Toggle Colors')" />
  <xsl:variable name="lang.TotalArmor"  select="string('Total of equipped single highest armor and accessories')" />
  <xsl:variable name="lang.ToxinsAndPathogens"  select="string('Toxins and Pathogens')" />
  <xsl:variable name="lang.UnnamedCharacter"  select="string('未命名角色')" />
  <xsl:variable name="lang.VehicleBody"    select="string('机体')" />
  <xsl:variable name="lang.VehicleCost"    select="string('载具售价')" />
  <xsl:variable name="lang.WildReputation"    select="string('Wild Reputation')" />

  <!-- "limits" list -->
  <xsl:variable name="lang.AstralLimit"    select="string('星界界限')" />
  <xsl:variable name="lang.MentalLimit"    select="string('精神界限')" />
  <xsl:variable name="lang.PhysicalLimit"    select="string('肉体界限')" />
  <xsl:variable name="lang.SocialLimit"    select="string('社交界限')" />

  <!-- spell types list -->
  <xsl:variable name="lang.CombatSpells"    select="string('战斗法术')" />
  <xsl:variable name="lang.DetectionSpells"  select="string('侦测法术')" />
  <xsl:variable name="lang.Enchantments"    select="string('附魔')" />
  <xsl:variable name="lang.HealthSpells"     select="string('生命法术')" />
  <xsl:variable name="lang.IllusionSpells"   select="string('幻象法术')" />
  <xsl:variable name="lang.ManipulationSpells" select="string('操纵法术')" />
  <xsl:variable name="lang.Rituals"      select="string('仪式')" />

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
