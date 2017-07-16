<?xml version="1.0" encoding="utf-8"?>
<!-- Export for Squad Manager (http://stauder-online.de/sr/english.htm) -->
<!-- Version -500 -->
<!-- ext:xml -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.stauer-online.de/sr/characters.xsd">
    <xsl:output method="xml" version="1.0" encoding="utf-8" indent="yes"/>
    <xsl:key name="loadedWeapon" match="weapons/weapon" use="ammoloaded" />
    <xsl:template match="/characters/character">
        <Shadowrun>
            <Character>
                <Name>
                    <xsl:choose>
                        <xsl:when test="string-length(alias) &gt; 0">
                            <xsl:value-of select="alias"/>
                        </xsl:when>
                        <xsl:when test="string-length(name) &gt; 0">
                            <xsl:value-of select="name"/>
                        </xsl:when>
                        <xsl:otherwise>Chummer</xsl:otherwise>
                    </xsl:choose>
                </Name>
                <Story>
                    <xsl:if test="string-length(eyes) &gt; 0">
                        <xsl:text>Eyes: </xsl:text>
                        <xsl:value-of select="eyes" />
                        <xsl:text>
</xsl:text>
                    </xsl:if>
                    <xsl:if test="string-length(hair) &gt; 0">
                        <xsl:text>Hair: </xsl:text>
                        <xsl:value-of select="hair" />
                        <xsl:text>
</xsl:text>
                    </xsl:if>
                    <xsl:if test="string-length(skin) &gt; 0">
                        <xsl:text>Skin: </xsl:text>
                        <xsl:value-of select="skin" />
                        <xsl:text>
</xsl:text>
                    </xsl:if>
                    <xsl:if test="string-length(concept) &gt; 0">
                        <xsl:text>
</xsl:text>
                        <xsl:value-of select="concept"/>
                        <xsl:text>
</xsl:text>
                    </xsl:if>
                    <xsl:if test="string-length(description) &gt; 0">
                        <xsl:text>
</xsl:text>
                        <xsl:value-of select="description"/>
                        <xsl:text>
</xsl:text>
                    </xsl:if>
                    <xsl:if test="string-length(background) &gt; 0">
                        <xsl:text>
</xsl:text>
                        <xsl:value-of select="background"/>
                        <xsl:text>
</xsl:text>
                    </xsl:if>
                    <xsl:text>
</xsl:text>
                    <xsl:if test="string-length(notes) &gt; 0">
                        <xsl:value-of select="notes"/>
                        <xsl:text>
</xsl:text>
                    </xsl:if>
                    <xsl:if test="string-length(playername) &gt; 0">
                        <xsl:text>
</xsl:text>
                        <xsl:text>Player: </xsl:text>
                        <xsl:value-of select="playername" />
                    </xsl:if>
                </Story>
                <Metatype>
                    <xsl:choose>
                        <xsl:when test="metatype_english = 'Human'">Norm</xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="metatype_english"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </Metatype>
                <xsl:if test="string-length(age) &gt; 0">
                    <!-- Chummer stores age as free string, Squad Manager expects years as a number -->
                    <xsl:if test="number(age) = number(age)">
                        <Age>
                            <xsl:value-of select="age"/>
                        </Age>
                    </xsl:if>
                </xsl:if>
                <xsl:if test="string-length(sex) &gt; 0">
                    <!-- Chummer stores sex as free string, Squad Manager expects one of "Male", "Female", "None" or "Unknown" -->
                    <xsl:choose>
                        <xsl:when test="translate(sex, 'm', 'M') = 'M'">
                            <Gender>Male</Gender>
                        </xsl:when>
                        <xsl:when test="translate(sex, 'm', 'M') = 'Male'">
                            <Gender>Male</Gender>
                        </xsl:when>
                        <xsl:when test="translate(sex, 'm', 'M') = 'Männlich'">
                            <!-- German -->
                            <Gender>Male</Gender>
                        </xsl:when>
                        <xsl:when test="translate(sex, 'f', 'F') = 'F'">
                            <Gender>Female</Gender>
                        </xsl:when>
                        <xsl:when test="translate(sex, 'f', 'F') = 'Female'">
                            <Gender>Female</Gender>
                        </xsl:when>
                        <xsl:when test="translate(sex, 'w', 'W') = 'W'">
                            <!-- German -->
                            <Gender>Female</Gender>
                        </xsl:when>
                        <xsl:when test="translate(sex, 'w', 'W') = 'Weiblich'">
                            <!-- German -->
                            <Gender>Female</Gender>
                        </xsl:when>
                        <!-- otherwise ignore -->
                    </xsl:choose>
                </xsl:if>
                <!--<xsl:if test="string-length(height) &gt; 0">
                    <Size>
                        <xsl:value-of select="substring((substring-before(height, &quot;'&quot;) div 3.28) + (substring-before(substring-after(height, &quot;'&quot;), '&quot;') div 39.37), 1, 4)"/>
                    </Size>
                </xsl:if>
                <xsl:if test="string-length(weight) &gt; 0">
                    <Weight>
                        <xsl:value-of select="substring(weight div 2.2, 1, 5)"/>
                    </Weight>
                </xsl:if>-->
                <xsl:if test="string-length(mugshotbase64) &gt; 0">
                    <Picture>
                        <xsl:value-of select="mugshotbase64"/>
                    </Picture>
                </xsl:if>
                <xsl:if test="string-length(eyes) &gt; 0">
                    <EyeColor>
                        <xsl:value-of select="eyes"/>
                    </EyeColor>
                </xsl:if>
                <xsl:if test="string-length(hair) &gt; 0">
                    <HairColor>
                        <xsl:value-of select="hair"/>
                    </HairColor>
                </xsl:if>
                <xsl:if test="nuyen &gt; 0">
                    <Money>
                        <xsl:value-of select="nuyen"/>
                    </Money>
                </xsl:if>
                <xsl:choose>
                    <xsl:when test="totalkarma &gt; 0">
                        <TotalKarma>
                            <xsl:value-of select="totalkarma"/>
                        </TotalKarma>
                    </xsl:when>
                    <xsl:when test="count(expenses/expense[type = 'Karma' and refund = 'False' and amount &gt; 0]) &gt; 0">
                        <TotalKarma>
                            <xsl:value-of select="sum(expenses/expense[type = 'Karma' and refund = 'False' and amount &gt; 0]/amount)"/>
                        </TotalKarma>
                    </xsl:when>
                </xsl:choose>
                <xsl:if test="karma &gt; 0">
                    <RemainingKarma>
                        <xsl:value-of select="karma"/>
                    </RemainingKarma>
                </xsl:if>
                <xsl:if test="streetcred &gt; 0">
                    <StreetCred>
                        <xsl:value-of select="streetcred"/>
                    </StreetCred>
                </xsl:if>
                <xsl:if test="notoriety &gt; 0">
                    <Notoriety>
                        <xsl:value-of select="notoriety"/>
                    </Notoriety>
                </xsl:if>
                <Awakened>
                    <xsl:choose>
                        <xsl:when test="magician = 'True'  and adept  = 'True'">Mystic Adept</xsl:when>
                        <xsl:when test="magician = 'True'  and adept != 'True'">Magician</xsl:when>
                        <xsl:when test="magician != 'True' and adept  = 'True'">Adept</xsl:when>
                        <xsl:when test="technomancer = 'True'">Technomancer</xsl:when>
                        <xsl:otherwise>Mundane</xsl:otherwise>
                    </xsl:choose>
                </Awakened>
                <xsl:if test="magenabled = 'True'">
                    <Magic>
                        <xsl:value-of select="attributes/attribute[name = 'MAG']/total" />
                    </Magic>
                </xsl:if>
                <xsl:if test="resenabled = 'True'">
                    <Resonance>
                        <xsl:value-of select="attributes/attribute[name = 'RES']/total" />
                    </Resonance>
                </xsl:if>
                <Essence>
                    <xsl:choose>
                        <xsl:when test="string-length(totaless) &gt; 0 and number(translate(totaless, ',', '.')) = number(translate(totaless, ',', '.'))">
                            <xsl:value-of select="number(translate(totaless, ',', '.'))"/>
                        </xsl:when>
                        <xsl:otherwise>6</xsl:otherwise>
                    </xsl:choose>
                </Essence>
                <Edge>
                    <xsl:value-of select="attributes/attribute[name = 'EDG']/total"/>
                </Edge>
                <Passes>
                    <xsl:value-of select="1 + sum(improvements/improvement[improvementttype = 'InitiativePass' and unique = 'initiativepass']/val)"/>
                </Passes>
                <CurrentPhysicalDamage>
                    <xsl:value-of select="physicalcmfilled"/>
                </CurrentPhysicalDamage>
                <CurrentStunDamage>
                    <xsl:value-of select="stuncmfilled"/>
                </CurrentStunDamage>

                <Attributes>
                    <Attribute type="Body">
                        <naturalValue>
                            <xsl:value-of select="attributes/attribute[name = 'BOD']/base"/>
                        </naturalValue>
                        <augmentedValue>
                            <xsl:value-of select="attributes/attribute[name = 'BOD']/total" />
                        </augmentedValue>
                        <naturalMaximum>
                            <xsl:value-of select="attributes/attribute[name = 'BOD']/max"/>
                        </naturalMaximum>
                    </Attribute>
                    <Attribute type="Agility">
                        <naturalValue>
                            <xsl:value-of select="attributes/attribute[name = 'AGI']/base"/>
                        </naturalValue>
                        <augmentedValue>
                            <xsl:value-of select="attributes/attribute[name = 'AGI']/total" />
                        </augmentedValue>
                        <naturalMaximum>
                            <xsl:value-of select="attributes/attribute[name = 'AGI']/max"/>
                        </naturalMaximum>
                    </Attribute>
                    <Attribute type="Reaction">
                        <naturalValue>
                            <xsl:value-of select="attributes/attribute[name = 'REA']/base"/>
                        </naturalValue>
                        <augmentedValue>
                            <xsl:value-of select="attributes/attribute[name = 'REA']/total" />
                        </augmentedValue>
                        <naturalMaximum>
                            <xsl:value-of select="attributes/attribute[name = 'REA']/max"/>
                        </naturalMaximum>
                    </Attribute>
                    <Attribute type="Strength">
                        <naturalValue>
                            <xsl:value-of select="attributes/attribute[name = 'STR']/base"/>
                        </naturalValue>
                        <augmentedValue>
                            <xsl:value-of select="attributes/attribute[name = 'STR']/total" />
                        </augmentedValue>
                        <naturalMaximum>
                            <xsl:value-of select="attributes/attribute[name = 'STR']/max"/>
                        </naturalMaximum>
                    </Attribute>
                    <Attribute type="Charisma">
                        <naturalValue>
                            <xsl:value-of select="attributes/attribute[name = 'CHA']/base"/>
                        </naturalValue>
                        <augmentedValue>
                            <xsl:value-of select="attributes/attribute[name = 'CHA']/total" />
                        </augmentedValue>
                        <naturalMaximum>
                            <xsl:value-of select="attributes/attribute[name = 'CHA']/max"/>
                        </naturalMaximum>
                    </Attribute>
                    <Attribute type="Intuition">
                        <naturalValue>
                            <xsl:value-of select="attributes/attribute[name = 'INT']/base"/>
                        </naturalValue>
                        <augmentedValue>
                            <xsl:value-of select="attributes/attribute[name = 'INT']/total" />
                        </augmentedValue>
                        <naturalMaximum>
                            <xsl:value-of select="attributes/attribute[name = 'INT']/max"/>
                        </naturalMaximum>
                    </Attribute>
                    <Attribute type="Logic">
                        <naturalValue>
                            <xsl:value-of select="attributes/attribute[name = 'LOG']/base"/>
                        </naturalValue>
                        <augmentedValue>
                            <xsl:value-of select="attributes/attribute[name = 'LOG']/total" />
                        </augmentedValue>
                        <naturalMaximum>
                            <xsl:value-of select="attributes/attribute[name = 'LOG']/max"/>
                        </naturalMaximum>
                    </Attribute>
                    <Attribute type="Willpower">
                        <naturalValue>
                            <xsl:value-of select="attributes/attribute[name = 'WIL']/base"/>
                        </naturalValue>
                        <augmentedValue>
                            <xsl:value-of select="attributes/attribute[name = 'WIL']/total" />
                        </augmentedValue>
                        <naturalMaximum>
                            <xsl:value-of select="attributes/attribute[name = 'WIL']/max"/>
                        </naturalMaximum>
                    </Attribute>
                </Attributes>

                <xsl:if test="count(skillgroups/skillgroup[rating &gt; 0]) + count(skills/skill[rating &gt; 0 and substring(skillcategory_english, string-length(skillcategory_english) - 5) = 'Active' and grouped = 'False']) &gt; 0">
                    <Skills>
                        <xsl:for-each select="skillgroups/skillgroup[rating &gt; 0]">
                            <xsl:element name="Skill">
                                <xsl:attribute name="name">
                                    <xsl:value-of select="name"/>
                                </xsl:attribute>
                                <Rank>
                                    <xsl:value-of select="rating"/>
                                </Rank>
                                <LinkedAttribute>
                                    <!-- For the conversion, we just assume the first attribute to be linked to the group and manually set every actually used attribute, so this value is of no real interest. -->
                                    <xsl:apply-templates select="../../skills/skill[skillgroup_english = current()/name][1]/attribute" />
                                </LinkedAttribute>
                                <Group>
                                    <xsl:for-each select="../../skills/skill[skillgroup_english = current()/name]">
                                        <xsl:element name="Skill">
                                            <xsl:attribute name="name">
                                                <xsl:value-of select="name"/>
                                            </xsl:attribute>
                                            <LinkedAttribute>
                                                <xsl:apply-templates select="attribute" />
                                            </LinkedAttribute>
                                        </xsl:element>
                                    </xsl:for-each>
                                </Group>
                            </xsl:element>
                        </xsl:for-each>
                        <xsl:for-each select="skills/skill[rating &gt; 0 and substring(skillcategory_english, string-length(skillcategory_english) - 5) = 'Active' and grouped = 'False']">
                            <xsl:element name="Skill">
                                <xsl:attribute name="name">
                                    <xsl:value-of select="name"/>
                                </xsl:attribute>
                                <Rank>
                                    <xsl:value-of select="rating"/>
                                </Rank>
                                <LinkedAttribute>
                                    <xsl:apply-templates select="attribute"/>
                                </LinkedAttribute>
                                <xsl:if test="string-length(spec) &gt; 0">
                                    <Specialization>
                                        <xsl:value-of select="spec"/>
                                    </Specialization>
                                </xsl:if>
                            </xsl:element>
                        </xsl:for-each>
                    </Skills>
                </xsl:if>

                <xsl:if test="count(qualities/quality) &gt; 0">
                    <Qualities>
                        <xsl:for-each select="qualities/quality">
                            <!-- newer file format has fully detailed qualities -->
                            <xsl:element name="Quality">
                                <xsl:attribute name="name">
                                    <xsl:value-of select="name"/>
                                </xsl:attribute>
                                <xsl:attribute name="type">
                                    <xsl:value-of select="translate(qualitytype_english, 'NP', 'np')"/>
                                </xsl:attribute>
                                <xsl:if test="string-length(extra) &gt; 0">
                                    <xsl:attribute name="specialization">
                                        <xsl:value-of select="extra"/>
                                    </xsl:attribute>
                                </xsl:if>
                                <xsl:attribute name="costs">
                                    <xsl:value-of select="number(format-number(bp, '###0.0###;#'))"/>
                                </xsl:attribute>
                            </xsl:element>
                        </xsl:for-each>
                    </Qualities>
                </xsl:if>

                <xsl:if test="count(contacts/contact) &gt; 0">
                    <Contacts>
                        <xsl:for-each select="contacts/contact">
                            <xsl:element name="Contact">
                                <xsl:attribute name="name">
                                    <xsl:value-of select="name"/>
                                </xsl:attribute>
                                <xsl:attribute name="connection">
                                    <xsl:value-of select="connection"/>
                                </xsl:attribute>
                                <xsl:attribute name="loyalty">
                                    <xsl:value-of select="loyalty"/>
                                </xsl:attribute>
                                <xsl:if test="membership + areaofinfluence + magicalresources + matrixresources &gt; 0">
                                    <xsl:attribute name="modifiedConnection">
                                        <xsl:value-of select="connection + membership + areaofinfluence + magicalresources + matrixresources"/>
                                    </xsl:attribute>
                                </xsl:if>
                                <xsl:if test="string-length(file) &gt; 0">
                                    <xsl:attribute name="filename">
                                        <xsl:value-of select="file"/>
                                    </xsl:attribute>
                                </xsl:if>
                                <Type>
                                    <xsl:value-of select="type"/>
                                </Type>
                                <Description>
                                    <xsl:value-of select="notes"/>
                                </Description>
                            </xsl:element>
                        </xsl:for-each>
                    </Contacts>
                </xsl:if>

                <xsl:if test="count(gears/gear[name = 'Fake SIN']) &gt; 0">
                    <SINs>
                        <xsl:for-each select="gears/gear[name = 'Fake SIN']">
                            <xsl:element name="SIN">
                                <xsl:attribute name="name">
                                    <xsl:value-of select="extra"/>
                                </xsl:attribute>
                                <xsl:attribute name="rating">
                                    <xsl:value-of select="rating"/>
                                </xsl:attribute>
                            </xsl:element>
                        </xsl:for-each>
                    </SINs>
                </xsl:if>

                <xsl:if test="count(gears/gear[category_english = 'Commlink']) &gt; 0">
                    <Devices>
                        <xsl:for-each select="gears/gear[category_english = 'Commlink']">
                            <xsl:element name="Commlink">
                                <xsl:attribute name="Name">
                                    <xsl:value-of select="name"/>
                                </xsl:attribute>
                                <Response>
                                    <xsl:choose>
                                        <xsl:when test="count(children/gear[category_english = 'Commlink Upgrade' and ./response &gt; 0]) &gt; 0">
                                            <xsl:value-of select="children/gear[category_english = 'Commlink Upgrade' and not(../gear[category_english = 'Commlink Upgrade']/response &gt; ./respose)]/response"/>
                                        </xsl:when>
                                        <xsl:otherwise>
                                            <xsl:value-of select="response"/>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </Response>
                                <Signal>
                                    <xsl:choose>
                                        <xsl:when test="count(children/gear[category_english = 'Commlink Upgrade' and ./signal &gt; 0]) &gt; 0">
                                            <xsl:value-of select="children/gear[category_english = 'Commlink Upgrade' and not(../gear[category_english = 'Commlink Upgrade']/signal &gt; ./signal)]/signal"/>
                                        </xsl:when>
                                        <xsl:otherwise>
                                            <xsl:value-of select="signal"/>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </Signal>
                                <System>
                                    <xsl:choose>
                                        <xsl:when test="count(children/gear[category_english = 'Commlink Operating System Upgrade' and ./system &gt; 0]) &gt; 0">
                                            <xsl:value-of select="children/gear[category_english = 'Commlink Operating System Upgrade' and not(../gear[category_english = 'Commlink Operating System Upgrade']/system &gt; ./system)]/system"/>
                                        </xsl:when>
                                        <xsl:when test="count(children/gear[category_english = 'Commlink Operating System']) &gt; 0">
                                            <xsl:value-of select="children/gear[category_english = 'Commlink Operating System']/system"/>
                                        </xsl:when>
                                        <xsl:otherwise>0</xsl:otherwise>
                                    </xsl:choose>
                                </System>
                                <Firewall>
                                    <xsl:choose>
                                        <xsl:when test="count(children/gear[category_english = 'Commlink Operating System Upgrade' and ./firewall &gt; 0]) &gt; 0">
                                            <xsl:value-of select="children/gear[category_english = 'Commlink Operating System Upgrade' and not(../gear[category_english = 'Commlink Operating System Upgrade']/firewall &gt; ./firewall)]/firewall"/>
                                        </xsl:when>
                                        <xsl:when test="count(children/gear[category_english = 'Commlink Operating System']) &gt; 0">
                                            <xsl:value-of select="children/gear[category_english = 'Commlink Operating System']/firewall"/>
                                        </xsl:when>
                                        <xsl:otherwise>0</xsl:otherwise>
                                    </xsl:choose>
                                </Firewall>
                                <xsl:if test="contains(children/gear[category_english = 'Sim Modules']/name_english, 'Hot')">
                                    <HotSimCapable>true</HotSimCapable>
                                </xsl:if>
                                <xsl:if test="count(children/gear[category_english = 'Matrix Programs']) &gt; 0">
                                    <Programs>
                                        <xsl:for-each select="children/gear[category_english = 'Matrix Programs']">
                                            <xsl:element name="Program">
                                                <xsl:attribute name="Name">
                                                    <xsl:value-of select="name"/>
                                                </xsl:attribute>
                                                <xsl:attribute name="Rank">
                                                    <xsl:value-of select="rating"/>
                                                </xsl:attribute>
                                                <xsl:attribute name="Type">
                                                    <xsl:choose>
                                                        <xsl:when test="name_english = 'Pilot'">Agent</xsl:when>
                                                        <xsl:when test="substring(avail_english, string-length(avail_english)) = 'R'">Hacking</xsl:when>
                                                        <xsl:otherwise>Common</xsl:otherwise>
                                                    </xsl:choose>
                                                </xsl:attribute>
                                            </xsl:element>
                                        </xsl:for-each>
                                    </Programs>
                                </xsl:if>
                            </xsl:element>
                        </xsl:for-each>
                    </Devices>
                </xsl:if>

                <xsl:if test="count(armors/armor) &gt; 0">
                    <Armors>
                        <xsl:for-each select="armors/armor">
                            <xsl:element name="Armor">
                                <xsl:attribute name="Name">
                                    <xsl:choose>
                                        <xsl:when test="armorname and string-length(armorname) &gt; 0">
                                            <xsl:value-of select="armorname" />
                                        </xsl:when>
                                        <xsl:otherwise>
                                            <xsl:value-of select="name"/>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </xsl:attribute>
                                <xsl:attribute name="active">
                                    <xsl:value-of select="translate(equipped, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')"/>
                                </xsl:attribute>
                                <xsl:choose>
                                    <xsl:when test="contains(name_english, 'Form-Fitting')">
                                        <xsl:attribute name="FormFitting">true</xsl:attribute>
                                    </xsl:when>
                                    <xsl:when test="substring(b, 1, 1) = '+'">
                                        <xsl:attribute name="Additive">true</xsl:attribute>
                                    </xsl:when>
                                </xsl:choose>
                                <Ballistic>
                                    <xsl:value-of select="b"/>
                                </Ballistic>
                                <Impact>
                                    <xsl:value-of select="i"/>
                                </Impact>
                                <xsl:for-each select="armormods/armormod">
                                    <xsl:choose>
                                        <xsl:when test="name_english = 'Chemical Protection'">
                                            <ChemicalProtection>
                                                <xsl:value-of select="current()/rating"/>
                                            </ChemicalProtection>
                                        </xsl:when>
                                        <xsl:when test="name_english = 'Fire Resistance'">
                                            <FireResistance>
                                                <xsl:value-of select="current()/rating"/>
                                            </FireResistance>
                                        </xsl:when>
                                        <xsl:when test="name_english = 'Insulation'">
                                            <Insulation>
                                                <xsl:value-of select="current()/rating"/>
                                            </Insulation>
                                        </xsl:when>
                                        <xsl:when test="name_english = 'Nonconductivity'">
                                            <Nonconductivity>
                                                <xsl:value-of select="current()/rating"/>
                                            </Nonconductivity>
                                        </xsl:when>
                                        <xsl:when test="name_english = 'Thermal Damping'">
                                            <ThermalDamping>
                                                <xsl:value-of select="current()/rating"/>
                                            </ThermalDamping>
                                        </xsl:when>
                                    </xsl:choose>
                                </xsl:for-each>
                            </xsl:element>
                        </xsl:for-each>
                    </Armors>
                </xsl:if>

                <xsl:if test="count(gears/gear[category_english = 'Ammunition' and substring(name_english, 1, 5) = 'Ammo:']) &gt; 0">
                    <Ammunitions>
                        <xsl:for-each select="gears/gear[category_english = 'Ammunition' and substring(name_english, 1, 5) = 'Ammo:']">
                            <xsl:element name="Ammunition">
                                <xsl:attribute name="name">
                                    <xsl:value-of select="substring(name_english, 7)" />
                                </xsl:attribute>
                                <xsl:attribute name="id">
                                    <xsl:value-of select="translate(page, '0123456789abcdefABCDEF-', '0123456789123456123456')" />
                                </xsl:attribute>
                                <Amount>
                                    <xsl:choose>
                                        <xsl:when test="count(key('loadedWeapon', guid)) &gt; 0">
                                            <xsl:value-of select="qty + sum(key('loadedWeapon', guid)/ammoremaining)" />
                                        </xsl:when>
                                        <xsl:otherwise>
                                            <xsl:value-of select="qty" />
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </Amount>
                                <xsl:element name="DV">
                                    <xsl:choose>
                                        <xsl:when test="count(weaponbonus/damage_english) &gt; 0">
                                            <xsl:value-of select="weaponbonus/damage_english" />
                                        </xsl:when>
                                        <xsl:when test="count(weaponbonus/damagereplace) &gt; 0">
                                            <xsl:attribute name="isFixed">true</xsl:attribute>
                                            <xsl:choose>
                                                <xsl:when test="substring(weaponbonus/damagereplace, string-length(weaponbonus/damagereplace) - 3) = 'S(e)'">
                                                    <xsl:value-of select="substring(weaponbonus/damagereplace, 1, string-length(weaponbonus/damagereplace) - 4)"/>
                                                </xsl:when>
                                                <xsl:when test="substring(weaponbonus/damagereplace, string-length(weaponbonus/damagereplace) - 3) = 'P(f)'">
                                                    <xsl:value-of select="substring(weaponbonus/damagereplace, 1, string-length(weaponbonus/damagereplace) - 4)"/>
                                                </xsl:when>
                                                <xsl:otherwise>
                                                    <xsl:value-of select="substring(weaponbonus/damagereplace, 1, string-length(weaponbonus/damagereplace) - 1)"/>
                                                </xsl:otherwise>
                                            </xsl:choose>
                                        </xsl:when>
                                        <xsl:otherwise>0</xsl:otherwise>
                                    </xsl:choose>
                                </xsl:element>
                                <AP>
                                    <xsl:choose>
                                        <xsl:when test="count(weaponbonus/ap) &gt; 0">
                                            <xsl:value-of select="weaponbonus/ap" />
                                        </xsl:when>
                                        <xsl:when test="count(weaponbonus/apreplace) &gt; 0">
                                            <!-- this is only the case for SnS for now, which is handled differently anyway -->
                                            <xsl:text>0</xsl:text>
                                        </xsl:when>
                                        <xsl:otherwise>0</xsl:otherwise>
                                    </xsl:choose>
                                </AP>
                                <xsl:if test="count(weaponbonus/damgereplace) + count(weaponbonus/damagetype) &gt; 0">
                                    <ChangeDamage>
                                        <xsl:choose>
                                            <xsl:when test="contains(weaponbonus/damagetype, 'S')">changeToStun</xsl:when>
                                            <xsl:when test="contains(weaponbonus/damagetype, 'P')">changeToPhysical</xsl:when>
                                            <xsl:when test="contains(weaponbonus/damagereplace, 'S')">changeToStun</xsl:when>
                                            <xsl:when test="contains(weaponbonus/damagereplace, 'P')">changeToPhysical</xsl:when>
                                            <xsl:otherwise>noChange</xsl:otherwise>
                                        </xsl:choose>
                                    </ChangeDamage>
                                </xsl:if>
                                <Caliber>
                                    <xsl:choose>
                                        <xsl:when test="string-length(extra) &gt; 0">
                                            <xsl:apply-templates select="extra" />
                                        </xsl:when>
                                        <xsl:otherwise>
                                            <!-- Older chummer file versions didn't include target type information. Arbitrary value to keep the file intact. -->
                                            <xsl:text>HeavyPistol</xsl:text>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </Caliber>
                                <Damage>
                                    <xsl:choose>
                                        <xsl:when test="contains(weaponbonus/damagereplace, '(e)') or contains(weaponbonus/damagetype, '(e)')">
                                            <xsl:text>Electrical</xsl:text>
                                        </xsl:when>
                                        <xsl:when test="contains(weaponbonus/damagereplace, '(f)') or contains(weaponbonus/damagetype, '(f)')">
                                            <xsl:text>FlechetteOnly</xsl:text>
                                        </xsl:when>
                                        <xsl:when test="contains(name_english, 'Flamethrower')">
                                            <xsl:text>Fire</xsl:text>
                                        </xsl:when>
                                        <xsl:when test="contains(name_english, 'Gauss')">
                                            <xsl:text>Gauss</xsl:text>
                                        </xsl:when>
                                        <xsl:otherwise>Bullets</xsl:otherwise>
                                    </xsl:choose>
                                </Damage>
                                <xsl:if test="string-length(notes) &gt; 0">
                                    <Comment>
                                        <xsl:value-of select="notes" />
                                    </Comment>
                                </xsl:if>
                            </xsl:element>
                        </xsl:for-each>
                    </Ammunitions>
                </xsl:if>

                <!-- Weapons -->
                <xsl:if test="count(weapons/weapon[type != 'Melee' and category_english != 'Gear']) &gt; 0">
                    <Weapons>
                        <xsl:for-each select="weapons/weapon[type != 'Melee' and category_english != 'Gear']">
                            <xsl:element name="Weapon">
                                <xsl:attribute name="name">
                                    <xsl:value-of select="name"/>
                                </xsl:attribute>
                                <xsl:element name="DV">
                                    <xsl:choose>
                                        <xsl:when test="substring(damage_english, string-length(damage_english) - 3) = 'S(e)'">
                                            <xsl:attribute name="physical">false</xsl:attribute>
                                            <xsl:value-of select="substring(damage_english, 1, string-length(damage_english) - 4)"/>
                                        </xsl:when>
                                        <xsl:when test="substring(damage, string-length(damage) - 3) = 'S(e)'">
                                            <xsl:attribute name="physical">false</xsl:attribute>
                                            <xsl:value-of select="substring(damage, 1, string-length(damage) - 4)"/>
                                        </xsl:when>
                                        <xsl:when test="substring(damage_english, string-length(damage_english) - 3) = 'P(f)'">
                                            <xsl:value-of select="substring(damage_english, 1, string-length(damage_english) - 4)"/>
                                        </xsl:when>
                                        <xsl:when test="substring(damage, string-length(damage) - 3) = 'P(f)'">
                                            <xsl:value-of select="substring(damage, 1, string-length(damage) - 4)"/>
                                        </xsl:when>
                                        <xsl:when test="not(number(substring(damage, 1, 1)) = number(substring(damage, 1, 1)))">
                                            <!-- no direct DV, assume something like "as toxin", and try to reconstruct the netto value -->
                                            <xsl:choose>
                                                <xsl:when test="contains(damage, 'Toxin')">
                                                    <!-- search for the first suitable toxin in the gear listing and use its values, 0 if nothing is found -->
                                                    <xsl:choose>
                                                        <xsl:when test="count(/character/gears/gear[name_english = 'Narcoject']) &gt; 0">
                                                            <xsl:attribute name="physical">false</xsl:attribute>
                                                            <xsl:text>10</xsl:text>
                                                        </xsl:when>
                                                        <xsl:when test="count(/character/gears/gear[name_english = 'Atropine']) &gt; 0">5</xsl:when>
                                                        <xsl:when test="count(/character/gears/gear[name_english = 'Naga Venom']) &gt; 0">6</xsl:when>
                                                        <xsl:when test="count(/character/gears/gear[name_english = 'Nova Scorpion Venom']) &gt; 0">12</xsl:when>
                                                        <xsl:when test="count(/character/gears/gear[name_english = 'Cyanide']) &gt; 0">8</xsl:when>
                                                        <xsl:otherwise>0</xsl:otherwise>
                                                    </xsl:choose>
                                                </xsl:when>
                                                <!-- We give up, we couldn't reconstruct any DV... -->
                                                <xsl:otherwise>0</xsl:otherwise>
                                            </xsl:choose>
                                        </xsl:when>
                                        <xsl:otherwise>
                                            <xsl:choose>
                                                <xsl:when test="substring(damage_english, string-length(damage_english)) = 'S'">
                                                    <xsl:attribute name="physical">false</xsl:attribute>
                                                </xsl:when>
                                                <xsl:when test="substring(damage, string-length(damage)) = 'S'">
                                                    <xsl:attribute name="physical">false</xsl:attribute>
                                                </xsl:when>
                                            </xsl:choose>
                                            <xsl:value-of select="substring(damage, 1, string-length(damage) - 1)"/>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </xsl:element>
                                <AP>
                                    <xsl:choose>
                                        <xsl:when test="ap = '-'">0</xsl:when>
                                        <xsl:when test="contains(ap, 'half')">0</xsl:when>
                                        <xsl:otherwise>
                                            <xsl:value-of select="ap"/>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </AP>
                                <xsl:choose>
                                    <xsl:when test="substring(damage_english, string-length(damage_english) - 3) = 'S(e)'">
                                        <DamageType>Electrical</DamageType>
                                    </xsl:when>
                                    <xsl:when test="substring(damage, string-length(damage) - 3) = 'S(e)'">
                                        <DamageType>Electrical</DamageType>
                                    </xsl:when>
                                    <xsl:when test="substring(damage_english, string-length(damage_english) - 3) = 'P(f)'">
                                        <DamageType>FlechetteOnly</DamageType>
                                    </xsl:when>
                                    <xsl:when test="substring(damage, string-length(damage) - 3) = 'P(f)'">
                                        <DamageType>FlechetteOnly</DamageType>
                                    </xsl:when>
                                    <xsl:when test="category_english = 'Flamethrowers'">
                                        <DamageType>Fire</DamageType>
                                    </xsl:when>
                                    <xsl:when test="contains(damage_english, 'Toxin') or contains(damage_english, 'Chemical')">
                                        <DamageType>Chemical</DamageType>
                                    </xsl:when>
                                    <xsl:when test="contains(damage, 'Toxin') or contains(damage, 'Chemical')">
                                        <DamageType>Chemical</DamageType>
                                    </xsl:when>
                                    <xsl:when test="contains(damage_english, 'Grenade') or contains(damage_english, 'Mortar')">
                                        <DamageType>Explosives</DamageType>
                                    </xsl:when>
                                    <xsl:when test="contains(damage, 'Grenade') or contains(damage, 'Mortar')">
                                        <DamageType>Explosives</DamageType>
                                    </xsl:when>
                                    <xsl:when test="category_english = 'Laser Weapons'">
                                        <DamageType>Laser</DamageType>
                                    </xsl:when>
                                    <xsl:when test="contains(name_english, 'Gauss')">
                                        <DamageType>Gauss</DamageType>
                                    </xsl:when>
                                </xsl:choose>
                                <AmmoSize>
                                    <xsl:choose>
                                        <xsl:when test="number(ammo) = number(ammo)">
                                            <xsl:value-of select="ammo"/>
                                        </xsl:when>
                                        <xsl:otherwise>
                                            <xsl:value-of select="substring-before(ammo, '(')"/>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </AmmoSize>
                                <AmmunitionType>
                                    <xsl:choose>
                                        <xsl:when test="substring-before(substring-after(ammo_english, '('), ')') = 'b'">BreakAction</xsl:when>
                                        <xsl:when test="substring-before(substring-after(ammo_english, '('), ')') = 'c'">Clip</xsl:when>
                                        <xsl:when test="substring-before(substring-after(ammo_english, '('), ')') = 'd'">Drum</xsl:when>
                                        <xsl:when test="substring-before(substring-after(ammo_english, '('), ')') = 'ml'">MuzzleLoader</xsl:when>
                                        <xsl:when test="substring-before(substring-after(ammo_english, '('), ')') = 'm'">InternalMagazine</xsl:when>
                                        <xsl:when test="substring-before(substring-after(ammo_english, '('), ')') = 'cy'">Cylinder</xsl:when>
                                        <xsl:when test="substring-before(substring-after(ammo_english, '('), ')') = 'belt'">Belt</xsl:when>

                                        <xsl:when test="substring-before(substring-after(ammo, '('), ')') = 'b'">BreakAction</xsl:when>
                                        <xsl:when test="substring-before(substring-after(ammo, '('), ')') = 'c'">Clip</xsl:when>
                                        <xsl:when test="substring-before(substring-after(ammo, '('), ')') = 'd'">Drum</xsl:when>
                                        <xsl:when test="substring-before(substring-after(ammo, '('), ')') = 'ml'">MuzzleLoader</xsl:when>
                                        <xsl:when test="substring-before(substring-after(ammo, '('), ')') = 'm'">InternalMagazine</xsl:when>
                                        <xsl:when test="substring-before(substring-after(ammo, '('), ')') = 'cy'">Cylinder</xsl:when>
                                        <xsl:when test="substring-before(substring-after(ammo, '('), ')') = 'belt'">Belt</xsl:when>
                                        <xsl:otherwise>InternalMagazine</xsl:otherwise>
                                    </xsl:choose>
                                </AmmunitionType>
                                <FireModes>
                                    <xsl:value-of select="translate(mode, '/', ' ')"/>
                                </FireModes>
                                <CurrentFireMode>
                                    <xsl:choose>
                                        <xsl:when test="contains(mode, '/')">
                                            <xsl:value-of select="substring-before(mode, '/')" />
                                        </xsl:when>
                                        <xsl:otherwise>
                                            <xsl:value-of select="mode" />
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </CurrentFireMode>
                                <Type>
                                    <xsl:apply-templates select="category_english"/>
                                </Type>
                                <xsl:if test="string-length(weaponname) + string-length(notes) &gt; 0">
                                    <Comment>
                                        <xsl:if test="string-length(weaponname) &gt; 0">
                                            <xsl:text>Name: </xsl:text>
                                            <xsl:value-of select="weaponname" />
                                        </xsl:if>
                                        <xsl:value-of select="notes" />
                                    </Comment>
                                </xsl:if>
                                <xsl:if test="ammoloaded != '00000000-0000-0000-0000-000000000000'">
                                    <xsl:element name="AmmunitionLoaded">
                                        <xsl:attribute name="remaining">
                                            <xsl:value-of select="ammoremaining" />
                                        </xsl:attribute>
                                        <xsl:value-of select="translate(substring-before(ammoloaded, '-'), '0123456789abcdefABCDEF-', '0123456789123456123456')" />
                                    </xsl:element>
                                </xsl:if>
                            </xsl:element>
                        </xsl:for-each>
                    </Weapons>
                </xsl:if>
            </Character>
        </Shadowrun>
    </xsl:template>

    <xsl:template match="attribute">
        <xsl:choose>
            <xsl:when test=". = 'BOD'">Body</xsl:when>
            <xsl:when test=". = 'AGI'">Agility</xsl:when>
            <xsl:when test=". = 'REA'">Reaction</xsl:when>
            <xsl:when test=". = 'STR'">Strength</xsl:when>
            <xsl:when test=". = 'CHA'">Charisma</xsl:when>
            <xsl:when test=". = 'INT'">Intuition</xsl:when>
            <xsl:when test=". = 'LOG'">Logic</xsl:when>
            <xsl:when test=". = 'WIL'">Willpower</xsl:when>
            <xsl:when test=". = 'EDG'">Edge</xsl:when>
            <xsl:when test=". = 'MAG'">Magic</xsl:when>
            <xsl:when test=". = 'RES'">Resonance</xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="."/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="category_english">
        <xsl:choose>
            <xsl:when test=". = 'Blades'">Blade</xsl:when>
            <xsl:when test=". = 'Clubs'">Clubs</xsl:when>
            <xsl:when test=". = 'Exotic Melee Weapons'">ExoticMeleeWeapon</xsl:when>
            <xsl:when test=". = 'Exotic Ranged Weapons'">ExoticRangedWeapon</xsl:when>
            <xsl:when test=". = 'Unarmed'">Unarmed</xsl:when>
            <xsl:when test=". = 'Bows'">Bow</xsl:when>
            <xsl:when test=". = 'Crossbows'">HeavyCrossbow</xsl:when>
            <xsl:when test=". = 'Throwing Weapons'">Throwing</xsl:when>
            <xsl:when test=". = 'Tasers'">Taser</xsl:when>
            <xsl:when test=". = 'Hold-Outs'">HoldOut</xsl:when>
            <xsl:when test=". = 'Holdouts'">HoldOut</xsl:when>
            <xsl:when test=". = 'Light Pistols'">LightPistol</xsl:when>
            <xsl:when test=". = 'Heavy Pistols'">HeavyPistol</xsl:when>
            <xsl:when test=". = 'Machine Pistols'">MachinePistol</xsl:when>
            <xsl:when test=". = 'Submachine Guns'">SubmachineGun</xsl:when>
            <xsl:when test=". = 'Assault Rifles'">AssaultRifle</xsl:when>
            <xsl:when test=". = 'Battle Rifles'">Battle Rifle</xsl:when>
            <xsl:when test=". = 'Sports Rifles'">SportRifle</xsl:when>
            <xsl:when test=". = 'Sniper Rifles'">SniperRifle</xsl:when>
            <xsl:when test=". = 'Shotguns'">Shotgun</xsl:when>
            <xsl:when test=". = 'Light Machine Guns'">LightMachineGun</xsl:when>
            <xsl:when test=". = 'Medium Machine Guns'">MediumMachineGun</xsl:when>
            <xsl:when test=". = 'Heavy Machine Guns'">HeavyMachineGun</xsl:when>
            <xsl:when test=". = 'Assault Cannons'">AssaultCannon</xsl:when>
            <xsl:when test=". = 'Grenade Launchers'">GrenadeLauncher</xsl:when>
            <xsl:when test=". = 'Mortar Launchers'">Mortar</xsl:when>
            <xsl:when test=". = 'Missile Launchers'">MissileLauncher</xsl:when>
            <!-- Chummer uses weapon categories, Squad Manager uses range categories -->
            <xsl:when test=". = 'Special Weapons'">ExoticRangedWeapon</xsl:when>
            <xsl:when test=". = 'Flamethrowers'">ExoticRangedWeapon</xsl:when>
            <xsl:when test=". = 'Laser Weapons'">ExoticRangedWeapon</xsl:when>
            <xsl:when test=". = 'Vehicle Weapons'">Other</xsl:when>
            <xsl:otherwise>Other</xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    <xsl:template match="extra">
        <xsl:choose>
            <xsl:when test=". = 'Blades'">Blade</xsl:when>
            <xsl:when test=". = 'Clubs'">Clubs</xsl:when>
            <xsl:when test=". = 'Exotic Melee Weapons'">ExoticMeleeWeapon</xsl:when>
            <xsl:when test=". = 'Exotic Ranged Weapons'">ExoticRangedWeapon</xsl:when>
            <xsl:when test=". = 'Unarmed'">Unarmed</xsl:when>
            <xsl:when test=". = 'Bows'">Bow</xsl:when>
            <xsl:when test=". = 'Crossbows'">HeavyCrossbow</xsl:when>
            <xsl:when test=". = 'Throwing Weapons'">Throwing</xsl:when>
            <xsl:when test=". = 'Tasers'">Taser</xsl:when>
            <xsl:when test=". = 'Hold-Outs'">HoldOut</xsl:when>
            <xsl:when test=". = 'Holdouts'">HoldOut</xsl:when>
            <xsl:when test=". = 'Light Pistols'">LightPistol</xsl:when>
            <xsl:when test=". = 'Heavy Pistols'">HeavyPistol</xsl:when>
            <xsl:when test=". = 'Machine Pistols'">MachinePistol</xsl:when>
            <xsl:when test=". = 'Submachine Guns'">SubmachineGun</xsl:when>
            <xsl:when test=". = 'Assault Rifles'">AssaultRifle</xsl:when>
            <xsl:when test=". = 'Battle Rifles'">Battle Rifle</xsl:when>
            <xsl:when test=". = 'Sports Rifles'">SportRifle</xsl:when>
            <xsl:when test=". = 'Sniper Rifles'">SniperRifle</xsl:when>
            <xsl:when test=". = 'Shotguns'">Shotgun</xsl:when>
            <xsl:when test=". = 'Light Machine Guns'">LightMachineGun</xsl:when>
            <xsl:when test=". = 'Medium Machine Guns'">MediumMachineGun</xsl:when>
            <xsl:when test=". = 'Heavy Machine Guns'">HeavyMachineGun</xsl:when>
            <xsl:when test=". = 'Assault Cannons'">AssaultCannon</xsl:when>
            <xsl:when test=". = 'Grenade Launchers'">GrenadeLauncher</xsl:when>
            <xsl:when test=". = 'Mortar Launchers'">Mortar</xsl:when>
            <xsl:when test=". = 'Missile Launchers'">MissileLauncher</xsl:when>
            <!-- Chummer uses weapon categories, Squad Manager uses range categories -->
            <xsl:when test=". = 'Special Weapons'">ExoticRangedWeapon</xsl:when>
            <xsl:when test=". = 'Flamethrowers'">ExoticRangedWeapon</xsl:when>
            <xsl:when test=". = 'Laser Weapons'">ExoticRangedWeapon</xsl:when>
            <xsl:when test=". = 'Vehicle Weapons'">Other</xsl:when>
            <xsl:otherwise>Other</xsl:otherwise>
        </xsl:choose>
    </xsl:template>
</xsl:stylesheet>
