<?xml version="1.0" encoding="UTF-8" ?>
<!-- Game Master character summary sheet -->
<!-- Created by Keith Rudolph, krudolph@gmail.com -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="xs.fnx.xslt"/>
  <xsl:include href="xs.TitleName.xslt"/>

  <xsl:include href="xt.MovementRate.xslt"/>
  <xsl:include href="xt.PreserveLineBreaks.xslt"/>

  <xsl:template match="/characters/character">
    <xsl:variable name="TitleName">
      <xsl:call-template name="TitleName">
        <xsl:with-param name="name" select="name"/>
        <xsl:with-param name="alias" select="alias"/>
      </xsl:call-template>
    </xsl:variable>
    <title><xsl:value-of select="$TitleName"/></title>

    <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
      <head>
        <meta http-equiv="x-ua-compatible" content="IE=Edge"/>
        <style type="text/css">
          * {
            font-family: tahoma, 'trebuchet ms', arial;
            font-size: 8pt;
            margin: 0;
          }
          .indent {
            padding-left: 20px;
          }
        </style>
        <style media="print">
           @page {
            size: auto;
            margin-top: 0.5in;
            margin-left: 0.5in;
            margin-right: 0.5in;
            margin-bottom: 0.75in;
          }
        </style>
      </head>

      <body>
        <div id="GameMasterBlock">
          <table width="100%" cellspacing="0" cellpadding="2" border="0" style="border: solid 2px #000000;">
            <tr><td>
              <strong><xsl:value-of select="name" /></strong> (<xsl:value-of select="metatype" />)
              &#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;
              <strong>
                <xsl:value-of select="$lang.Movement" />:
              </strong>
              <xsl:call-template name="MovementRate"/>
              <table width="100%" cellspacing="0" cellpadding="2">
                <tr>
                  <td width="9%" align="center"><strong><xsl:value-of select="$lang.BOD"/></strong></td>
                  <td width="9%" align="center"><strong><xsl:value-of select="$lang.AGI"/></strong></td>
                  <td width="9%" align="center"><strong><xsl:value-of select="$lang.REA"/></strong></td>
                  <td width="9%" align="center"><strong><xsl:value-of select="$lang.STR"/></strong></td>
                  <td width="9%" align="center"><strong><xsl:value-of select="$lang.CHA"/></strong></td>
                  <td width="9%" align="center"><strong><xsl:value-of select="$lang.INT"/></strong></td>
                  <td width="9%" align="center"><strong><xsl:value-of select="$lang.LOG"/></strong></td>
                  <td width="9%" align="center"><strong><xsl:value-of select="$lang.WIL"/></strong></td>
                  <td width="9%" align="center"><strong><xsl:value-of select="$lang.EDG"/></strong></td>
                  <xsl:if test="magenabled = 'True'">
                    <td width="9%" align="center"><strong><xsl:value-of select="$lang.MAG"/></strong></td>
                  </xsl:if>
                  <xsl:if test="resenabled = 'True'">
                    <td width="9%" align="center"><strong><xsl:value-of select="$lang.RES"/></strong></td>
                  </xsl:if>
                  <td width="9%" align="center"><strong><xsl:value-of select="$lang.ESS"/></strong></td>
                </tr>
                <tr>
                  <td width="9%" align="center">
                    <xsl:value-of select="attributes/attribute[name_english = 'BOD']/base" />
                    <xsl:if test="attributes/attribute[name_english = 'BOD']/total != attributes/attribute[name_english = 'BOD']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'BOD']/total" />)
                    </xsl:if>
                  </td>
                  <td width="9%" align="center">
                    <xsl:value-of select="attributes/attribute[name_english = 'AGI']/base" />
                    <xsl:if test="attributes/attribute[name_english = 'AGI']/total != attributes/attribute[name_english = 'AGI']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'AGI']/total" />)
                    </xsl:if>
                  </td>
                  <td width="9%" align="center">
                    <xsl:value-of select="attributes/attribute[name_english = 'REA']/base" />
                    <xsl:if test="attributes/attribute[name_english = 'REA']/total != attributes/attribute[name_english = 'REA']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'REA']/total" />)
                    </xsl:if>
                  </td>
                  <td width="9%" align="center">
                    <xsl:value-of select="attributes/attribute[name_english = 'STR']/base" />
                    <xsl:if test="attributes/attribute[name_english = 'STR']/total != attributes/attribute[name_english = 'STR']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'STR']/total" />)
                    </xsl:if>
                  </td>
                  <td width="9%" align="center">
                    <xsl:value-of select="attributes/attribute[name_english = 'CHA']/base" />
                    <xsl:if test="attributes/attribute[name_english = 'CHA']/total != attributes/attribute[name_english = 'CHA']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'CHA']/total" />)
                    </xsl:if>
                  </td>
                  <td width="9%" align="center">
                    <xsl:value-of select="attributes/attribute[name_english = 'INT']/base" />
                    <xsl:if test="attributes/attribute[name_english = 'INT']/total != attributes/attribute[name_english = 'INT']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'INT']/total" />)
                    </xsl:if>
                  </td>
                  <td width="9%" align="center">
                    <xsl:value-of select="attributes/attribute[name_english = 'LOG']/base" />
                    <xsl:if test="attributes/attribute[name_english = 'LOG']/total != attributes/attribute[name_english = 'LOG']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'LOG']/total" />)
                    </xsl:if>
                  </td>
                  <td width="9%" align="center">
                    <xsl:value-of select="attributes/attribute[name_english = 'WIL']/base" />
                    <xsl:if test="attributes/attribute[name_english = 'WIL']/total != attributes/attribute[name_english = 'WIL']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'WIL']/total" />)
                    </xsl:if>
                  </td>
                  <td width="9%" align="center">
                    <xsl:value-of select="attributes/attribute[name_english = 'EDG']/base" />
                    <xsl:if test="attributes/attribute[name_english = 'EDG']/total != attributes/attribute[name_english = 'EDG']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'EDG']/total" />)
                    </xsl:if>
                  </td>
                  <xsl:if test="magenabled = 'True'">
                  <td width="9%" align="center">
                    <xsl:value-of select="attributes/attribute[name_english = 'MAG']/base" />
                    <xsl:if test="attributes/attribute[name_english = 'MAG']/total != attributes/attribute[name_english = 'MAG']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'MAG']/total" />)
                    </xsl:if>
                  </td>
                  </xsl:if>
                  <xsl:if test="resenabled = 'True'">
                  <td width="9%" align="center">
                    <xsl:value-of select="attributes/attribute[name_english = 'RES']/base" />
                    <xsl:if test="attributes/attribute[name_english = 'RES']/total != attributes/attribute[name_english = 'RES']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'RES']/total" />)
                    </xsl:if>
                  </td>
                  </xsl:if>
                  <td width="9%" align="center">
                    <xsl:value-of select="totaless" />
                    <xsl:if test="attributes/attribute[name_english = 'ESS']/total != totaless">
                      (<xsl:value-of select="attributes/attribute[name_english = 'ESS']/total" />)
                    </xsl:if>
                  </td>
                </tr>
                <tr colspan="11">
                  <td width="9%" align="center" valign="top"><strong><xsl:value-of select="$lang.Init"/></strong></td>
                  <td width="9%" align="center" valign="top"><strong><xsl:value-of select="$lang.Rigger"/></strong></td>
                  <td width="9%" align="center" valign="top"><strong><xsl:value-of select="$lang.Astral"/></strong></td>
                  <td width="9%" align="center" valign="top">
                    <strong>
                      <xsl:value-of select="$lang.MatrixAR"/>
                    </strong>
                  </td>
                  <td width="9%" align="center" valign="top">
                    <strong>
                      <xsl:value-of select="$lang.MatrixCold"/>
                    </strong>
                  </td>
                  <td width="9%" align="center" valign="top">
                    <strong>
                      <xsl:value-of select="$lang.MatrixHot"/>
                    </strong>
                  </td>
                  <td width="9%" align="center" valign="top"><strong><xsl:value-of select="$lang.CM"/></strong></td>
                  <td width="9%" align="center" valign="top"><strong><xsl:value-of select="$lang.PhysicalLimit"/></strong></td>
                  <td width="9%" align="center" valign="top"><strong><xsl:value-of select="$lang.MentalLimit"/></strong></td>
                  <td width="9%" align="center" valign="top"><strong><xsl:value-of select="$lang.SocialLimit"/></strong></td>
                </tr>
                <tr colspan="11">
                </tr>
                <tr colspan="11">
                  <td width="9%" align="center" valign="top">
                    <xsl:value-of select="init" />
                  </td>
                  <td width="9%" align="center" valign="top">
                    <xsl:value-of select="riggerinit" />
                  </td>
                  <td width="9%" align="center" valign="top">
                    <xsl:value-of select="astralinit" />
                  </td>
                  <td width="9%" align="center" valign="top">
                    <xsl:value-of select="matrixarinit" />
                  </td>
                  <td width="9%" align="center" valign="top">
                    <xsl:value-of select="matrixcoldinit" />
                  </td>
                  <td width="9%" align="center" valign="top">
                    <xsl:value-of select="matrixhotinit" />
                  </td>
                  <td width="9%" align="center" valign="top">
                    <xsl:value-of select="physicalcm" />/<xsl:value-of select="stuncm" />
                  </td>
                  <td width="9%" align="center" valign="top">
                    <xsl:value-of select="limitphysical" />
                  </td>
                  <td width="9%" align="center" valign="top">
                    <xsl:value-of select="limitmental" />
                  </td>
                  <td width="9%" align="center" valign="top">
                    <xsl:value-of select="limitsocial" />
                  </td>
                </tr>
              </table>

              <xsl:if test="qualities/quality">
                <p><strong><xsl:value-of select="$lang.Qualities"/>: </strong>
                <xsl:for-each select="qualities/quality">
                  <xsl:sort select="name" />
                  <xsl:value-of select="name" />
                  <xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
                  <xsl:if test="position() != last()">, </xsl:if>
                </xsl:for-each></p>
              </xsl:if>

              <p><strong><xsl:value-of select="$lang.ActiveSkills"/>: </strong>
              <xsl:for-each select="skills/skill[knowledge = 'False' and (rating &gt; 0 or total &gt; 0)]">
                <xsl:sort select="name" />
                <xsl:value-of select="name" />
                <xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
                <xsl:text> </xsl:text><xsl:value-of select="total" />
                <xsl:if test="spec != '' and exotic = 'False'"> (<xsl:value-of select="specializedrating" />)</xsl:if>
                <xsl:if test="position() != last()">, </xsl:if>
              </xsl:for-each></p>

              <xsl:if test="skills/skill[knowledge = 'True' and (rating &gt; 0 or total &gt; 0 or rating = 0)]">
                <p><strong><xsl:value-of select="$lang.KnowledgeSkills"/>: </strong>
                <xsl:for-each select="skills/skill[knowledge = 'True' and (rating &gt; 0 or total &gt; 0 or rating = 0)]">
                  <xsl:sort select="name" />
                  <xsl:value-of select="name" />
                  <xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
                  <xsl:if test="rating = 0"><xsl:text> </xsl:text><xsl:value-of select="substring($lang.Native,1,1)"/></xsl:if>
                  <xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="total" /></xsl:if>
                  <xsl:if test="spec != ''"> (<xsl:value-of select="specializedrating" />)</xsl:if>
                  <xsl:if test="position() != last()">, </xsl:if>
                </xsl:for-each></p>
              </xsl:if>

              <table width="100%" cellspacing="0" cellpadding="0">
                <tr>
                  <td width="49%" valign="top">
                    <table width="100%" cellspacing="0" cellpadding="2">
                      <tr>
                        <td><strong><xsl:value-of select="$lang.Weapon"/></strong></td>
                        <td align="center"><strong><xsl:value-of select="$lang.Pool"/></strong></td>
                        <td align="center"><strong><xsl:value-of select="$lang.Accuracy"/></strong></td>
                        <td align="center"><strong><xsl:value-of select="$lang.Damage"/></strong></td>
                        <td align="center"><strong><xsl:value-of select="$lang.AP"/></strong></td>
                        <td align="center"><strong><xsl:value-of select="$lang.Mode"/></strong></td>
                        <td align="center"><strong><xsl:value-of select="$lang.RC"/></strong></td>
                      </tr>
                      <xsl:for-each select="weapons/weapon">
                        <xsl:sort select="name" />
                        <tr>
                          <xsl:if test="position() mod 2 != 1">
                            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
                          </xsl:if>
                          <td><xsl:value-of select="name" /></td>
                          <td align="center"><xsl:value-of select="dicepool" /></td>
                          <td align="center"><xsl:value-of select="accuracy" /></td>
                          <td align="center"><xsl:value-of select="damage" /></td>
                          <td align="center"><xsl:value-of select="ap" /></td>
                          <td align="center"><xsl:value-of select="mode" /></td>
                          <td align="center"><xsl:value-of select="rc" /></td>
                        </tr>
                        <xsl:if test="accessories/accessory or mods/weaponmod">
                          <tr>
                            <td colspan="5" class="indent">
                              <xsl:if test="position() mod 2 != 1">
                                <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
                              </xsl:if>
                              <xsl:for-each select="accessories/accessory">
                                <xsl:sort select="name" />
                                        <xsl:value-of select="name" />
                                        <xsl:if test="position() != last()">, </xsl:if>
                              </xsl:for-each>
                              <xsl:if test="accessories/accessory and mods/weaponmod">, </xsl:if>
                              <xsl:for-each select="mods/weaponmod">
                                <xsl:sort select="name" />
                                        <xsl:value-of select="name" />
                                        <xsl:if test="rating > 0">
                                        <xsl:value-of select="$lang.Rating"/> <xsl:value-of select="rating" />
                                        </xsl:if>
                                        <xsl:if test="position() != last()">, </xsl:if>
                              </xsl:for-each>
                            </td>
                          </tr>
                        </xsl:if>
                      </xsl:for-each>
                    </table>
                  </td>
                  <td width=".5%">&#160;</td>
                  <td width="25%" valign="top">
                    <table width="100%" cellspacing="0" cellpadding="2">
                      <tr>
                        <td><strong><xsl:value-of select="$lang.CombatSkill"/></strong></td>
                        <td align="center"><strong><xsl:value-of select="$lang.Rtg"/></strong></td>
                      </tr>
                      <xsl:for-each select="skills/skill[skillcategory = 'Combat Active']">
                        <xsl:sort select="name" />
                        <tr>
                          <xsl:if test="position() mod 2 != 1">
                            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
                          </xsl:if>
                          <td>
                            <xsl:value-of select="name" />
                            <xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
                          </td>
                          <td align="center"><xsl:value-of select="total" /></td>
                        </tr>
                      </xsl:for-each>
                    </table>
                  </td>
                  <td width=".5%">&#160;</td>
                  <td width="25%" valign="top">
                    <table width="100%" cellspacing="0" cellpadding="2">
                      <tr>
                        <td><strong><xsl:value-of select="$lang.Armor"/> (<xsl:value-of select="armor" />)</strong></td>
                        <td align="center"> </td>
                      </tr>
                      <xsl:for-each select="armors/armor">
                        <xsl:sort select="name" />
                        <tr>
                          <xsl:if test="position() mod 2 != 1">
                            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
                          </xsl:if>
                          <td><xsl:value-of select="name" /></td>
                          <td align="center"><xsl:value-of select="armor" /></td>
                        </tr>
                        <xsl:if test="armormods/armormod">
                          <tr>
                            <td colspan="2" class="indent">
                              (<xsl:for-each select="armormods/armormod">
                                <xsl:sort select="name" />
                                <xsl:value-of select="name" />
                                <xsl:if test="position() != last()">, </xsl:if>
                              </xsl:for-each>)
                            </td>
                          </tr>
                        </xsl:if>
                        <xsl:if test="gears/gear">
                          <tr>
                            <xsl:if test="position() mod 2 != 1">
                              <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
                            </xsl:if>
                            <td colspan="2" class="indent">
                              <xsl:for-each select="gears/gear">
                                <xsl:sort select="name" />
                                <xsl:value-of select="name" />
                                <xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
                                <xsl:if test="rating != 0"> <xsl:value-of select="$lang.Rating"/> <xsl:value-of select="rating" /></xsl:if>
                                <xsl:if test="qty &gt; 1"> x<xsl:value-of select="qty" /></xsl:if>
                                <xsl:if test="children/gear">
                                  (<xsl:for-each select="children/gear">
                                    <xsl:value-of select="name" />
                                    <xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
                                    <xsl:if test="rating != 0"> <xsl:value-of select="$lang.Rating"/> <xsl:value-of select="rating" /></xsl:if>
                                    <xsl:if test="children/gear">
                                      [<xsl:for-each select="children/gear">
                                        <xsl:sort select="name" />
                                        <xsl:value-of select="name" />
                                        <xsl:if test="rating != 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
                                        <xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
                                        <xsl:if test="position() != last()">, </xsl:if>
                                      </xsl:for-each>]
                                    </xsl:if>
                                  </xsl:for-each>)
                                </xsl:if>
                              </xsl:for-each>
                            </td>
                          </tr>
                        </xsl:if>
                      </xsl:for-each>
                    </table>
                  </td>
                </tr>
              </table>

              <xsl:if test="martialarts/martialart">
                <p><strong><xsl:value-of select="$lang.MartialArts"/>: </strong>
                <xsl:for-each select="martialarts/martialart">
                  <xsl:sort select="name" />
                  <xsl:value-of select="name" />
                  <xsl:if test="martialartadvantages/martialartadvantage">
                  (
                  <xsl:for-each select="martialartadvantages/martialartadvantage">
                    <xsl:value-of select="." /><xsl:if test="position() != last()">, </xsl:if>
                  </xsl:for-each>
                  )
                  </xsl:if>
                </xsl:for-each></p>
                <xsl:if test="martialartmaneuvers/martialartmaneuver">
                  <p><strong><xsl:value-of select="$lang.Maneuvers"/>: </strong>
                  <xsl:for-each select="martialartmaneuvers/martialartmaneuver">
                    <xsl:sort select="name" />
                    <xsl:value-of select="name" /><xsl:if test="position() != last()">, </xsl:if>
                  </xsl:for-each></p>
                </xsl:if>
              </xsl:if>

              <xsl:if test="spells/spell">
                <p><strong><xsl:value-of select="$lang.Spells"/>: </strong>
                <xsl:for-each select="spells/spell">
                  <xsl:sort select="name" />
                  <xsl:value-of select="name" />
                  <xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
                  (<xsl:value-of select="type" />, <xsl:value-of select="range" />, <xsl:value-of select="damage" />, <xsl:value-of select="duration" />, <xsl:value-of select="dv" />)
                  <xsl:if test="position() != last()">, </xsl:if>
                </xsl:for-each></p>
              </xsl:if>

              <xsl:if test="powers/power">
                <p><strong><xsl:value-of select="$lang.Powers"/>: </strong>
                <xsl:for-each select="powers/power">
                  <xsl:sort select="name" />
                  <xsl:value-of select="name" />
                  <xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
                  <xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
                  <xsl:if test="position() != last()">, </xsl:if>
                </xsl:for-each></p>
              </xsl:if>

              <xsl:if test="critterpowers/critterpower">
                <p><strong><xsl:value-of select="$lang.CritterPowers"/>: </strong>
                <xsl:for-each select="critterpowers/critterpower">
                  <xsl:sort select="name" />
                  <xsl:value-of select="name" />
                  <xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
                  <xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
                  <xsl:if test="position() != last()">, </xsl:if>
                </xsl:for-each></p>
              </xsl:if>

              <xsl:if test="techprograms/techprogram">
                <p><strong><xsl:value-of select="$lang.ComplexForms"/>: </strong>
                <xsl:for-each select="techprograms/techprogram">
                  <xsl:sort select="name" />
                  <xsl:value-of select="name" />
                  <xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
                  <xsl:text> </xsl:text>(<xsl:value-of select="target" />, <xsl:value-of select="duration" />, <xsl:value-of select="fv" />)
                  <xsl:if test="programoptions/programoption">
                    (<xsl:for-each select="programoptions/programoption">
                      <xsl:sort select="name" />
                      <xsl:value-of select="name" />
                      <xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
                      <xsl:if test="position() != last()">, </xsl:if>
                    </xsl:for-each>)
                  </xsl:if>
                  <xsl:if test="position() != last()">, </xsl:if>
                </xsl:for-each></p>
              </xsl:if>

              <xsl:if test="aiprograms/aiprogram">
                <p>
                <strong><xsl:value-of select="$lang.AIandAdvanced"/></strong>
                <xsl:for-each select="aiprograms/aiprogram">
                  <xsl:sort select="name" />
                  <xsl:value-of select="name" />
                  <xsl:if test="extra != ''">
                    (<xsl:value-of select="extra" />)
                  </xsl:if>
                  <xsl:if test="programoptions/programoption">
                    (<xsl:for-each select="programoptions/programoption">
                      <xsl:sort select="name" />
                      <xsl:value-of select="name" />
                      <xsl:if test="rating &gt; 0">
                        <xsl:text> </xsl:text>
                        <xsl:value-of select="rating" />
                      </xsl:if>
                      <xsl:if test="position() != last()">, </xsl:if>
                    </xsl:for-each>)
                  </xsl:if>
                  <xsl:if test="position() != last()">, </xsl:if>
                  </xsl:for-each>
                </p>
              </xsl:if>

              <xsl:if test="cyberwares/cyberware">
                <p><strong><xsl:value-of select="$lang.Cyberware"/>/<xsl:value-of select="$lang.Bioware"/>: </strong>
                <xsl:for-each select="cyberwares/cyberware">
                  <xsl:sort select="name" />
                  <xsl:value-of select="name" />
                  <xsl:if test="rating != 0"><xsl:text> </xsl:text><xsl:value-of select="$lang.Rating"/><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
                  <xsl:if test="children/cyberware">
                    (<xsl:for-each select="children/cyberware">
                      <xsl:value-of select="name" />
                      <xsl:if test="rating != 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
                      <xsl:if test="position() != last()">, </xsl:if>
                    </xsl:for-each>)
                  </xsl:if>
                  <xsl:if test="position() != last()">, </xsl:if>
                </xsl:for-each></p>
              </xsl:if>

              <xsl:if test="gears/gear">
                <p><strong><xsl:value-of select="$lang.Gear"/>: </strong>
                <xsl:for-each select="gears/gear">
                  <xsl:sort select="name" />
                  <xsl:value-of select="name" />
                  <xsl:if test="qty &gt; 1"> x<xsl:value-of select="qty" /></xsl:if>
                  <xsl:if test="children/gear">
                    [<xsl:call-template name="gearplugin">
                    <xsl:with-param name="gear" select="." />
                  </xsl:call-template>]
                  </xsl:if>
                  <xsl:if test="position() != last()">, </xsl:if>
                </xsl:for-each></p>
              </xsl:if>

              <p><strong><xsl:value-of select="$lang.Nuyen"/>: </strong>
                <xsl:call-template name="fnx-fmt-nmbr">
                  <xsl:with-param name="nmbr" select="nuyen"/>
                </xsl:call-template>
              </p>

              <xsl:if test="notes != ''">
                <p><strong><xsl:value-of select="$lang.Notes"/>: </strong>
                  <xsl:call-template name="PreserveLineBreaks">
                    <xsl:with-param name="text" select="notes"/>
                  </xsl:call-template>
                </p>
              </xsl:if>
            </td></tr>
          </table>
        </div>
      </body>
    </html>
  </xsl:template>

  <xsl:template name="gearplugin">
      <xsl:param name="gear" />
    <xsl:for-each select="children/gear">
      <xsl:sort select="name" />
      <xsl:value-of select="name" />
      <xsl:if test="rating != 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
      <xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
      <xsl:if test="children/gear">
        [<xsl:call-template name="gearplugin">
          <xsl:with-param name="gear" select="." />
        </xsl:call-template>]
      </xsl:if>
      <xsl:if test="position() != last()">; </xsl:if>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
