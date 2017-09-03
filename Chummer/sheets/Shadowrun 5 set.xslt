<?xml version="1.0" encoding="UTF-8" ?>
<!-- Character sheet based on the Shadowrun 5th Edition Character Sheet (English - US) -->
<!-- Created by Keith Rudolph, krudolph@gmail.com -->
<!-- Version -496 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:include href="xs.fnx.xslt"/>
  <xsl:include href="xs.TitleName.xslt"/>

  <xsl:include href="xt.Calendar.xslt"/>
  <xsl:include href="xt.ConditionMonitor.xslt"/>
  <xsl:include href="xt.Contacts.xslt"/>
  <xsl:include href="xt.Expenses.xslt"/>
  <xsl:include href="xt.Lifestyles.xslt"/>
  <xsl:include href="xt.MovementRate.xslt"/>
  <xsl:include href="xt.Notes.xslt"/>
  <xsl:include href="xt.PreserveLineBreaks.xslt"/>
  <xsl:include href="xt.Qualities.xslt"/>
  <xsl:include href="xt.RangedWeapons.xslt"/>
  <xsl:include href="xt.RowSummary.xslt"/>
  <xsl:include href="xt.RuleLine.xslt"/>
  <xsl:include href="xt.Spells.xslt"/>
  <xsl:include href="xt.TableTitle.xslt"/>

<!-- Set local control variables if global versions have not already been defined  -->
  <!-- print core details (supress notes, calendar, expenses, and additional mugshots) -->
  <xsl:variable name="CorePrint" select="false()"/>
  <!-- minimum skill rating to print - used to prevent printing of zero ratings -->
  <xsl:variable name="MinimumRating" select="0"/>
  <!-- supress notes if producing core print -->
  <xsl:variable name="ProduceNotes" select="not($CorePrint)"/>
<!-- End of setting local control variables to default local values  -->

  <xsl:variable name="rtglit">
    <xsl:variable name="r1">
      <xsl:call-template name="fnx-lc">
        <xsl:with-param name="string" select="$lang.Rating"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="concat(' ',$r1)"/>
  </xsl:variable>

  <xsl:template match="/characters/character">
    <xsl:variable name="TitleName">
      <xsl:call-template name="TitleName">
        <xsl:with-param name="name" select="name"/>
        <xsl:with-param name="alias" select="alias"/>
      </xsl:call-template>
    </xsl:variable>
    <title><xsl:value-of select="$TitleName"/></title>

    <xsl:text disable-output-escaping="yes"><![CDATA[<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">]]></xsl:text>
    <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
      <head>
        <meta http-equiv="x-ua-compatible" content="IE=Edge"/>
        <style type="text/css">
          * {
            font-family: segoe, tahoma, 'trebuchet ms', arial;
            font-size: 8pt;
            margin: 0;
            text-align: left;
            vertical-align: top;
          }
          html {
            height: 100%;
            margin: 0px;  /* this affects the margin on the html before sending to printer */
          }
          .tablestyle {
            border-collapse: collapse;
            border-color: #1c4a2d;
            border-style: solid;
            border-width: 0.5mm;
            cellpadding: 2;
            cellspacing: 0;
            width: 100%;
          }
          .attributecell p {
            padding: 2px;
            margin: 4px;
            border: solid 1px #1c4a2d;
            text-transform: uppercase;
          }
          .indent {
            padding-left: 2mm;
          }
          .notesrow {
            text-align: justify;
          }
          .notesrow2 {
            padding-left: 2mm;
            padding-right: 2mm;
            text-align: justify;
          }
          th {
            text-align: center;
            text-transform: uppercase;
          }
          .title {
            font-weight: bold;
            text-transform: uppercase;
          }
          .upper {
            text-transform: uppercase;
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
          .block {
            bottom-padding: 0;
            margin: 4px 0 4px 0;  /* to keep the page break from cutting too close to the text in the div */
          }
        </style>
      </head>
      <body>
          <div class="block" id="PersonalDataBlock" style="line-height: 12pt">
            <table class="tablestyle">
              <tr>
                <td width="10%" class="title">
                  <xsl:value-of select="$lang.Name"/>:
                </td>
                <td colspan="5">
                  <xsl:choose>
                    <xsl:when test="alias = '' or name = '' or name = $lang.UnnamedCharacter">
                      <xsl:value-of select="$TitleName"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="name"/>
                      <xsl:text> </xsl:text>
                      <xsl:value-of select="$lang.as"/>
                      <xsl:text> </xsl:text>
                      <strong><xsl:value-of select="alias"/></strong>
                    </xsl:otherwise>
                  </xsl:choose>
                </td>
                <td width="10%" class="title">
                  <xsl:value-of select="$lang.Player"/>:
                </td>
                <td colspan="3"><xsl:value-of select="playername"/></td>
              </tr>
              <tr>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Metatype"/>:
                </td>
                <td colspan="3">
                  <xsl:value-of select="metatype"/>
                  <xsl:if test="metavariant != ''"> (<xsl:value-of select="metavariant"/>)</xsl:if>
                </td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Age"/>:
                </td>
                <td width="10%"><xsl:value-of select="age"/></td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Sex"/>:
                </td>
                <td width="10%"><xsl:value-of select="sex"/></td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Nuyen"/>:
                </td>
                <td width="10%">
                <xsl:call-template name="fnx-fmt-nmbr">
                  <xsl:with-param name="nmbr" select="nuyen"/>
                </xsl:call-template><xsl:value-of select="$lang.NuyenSymbol"/>
                </td>
              </tr>
              <tr>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Height"/>:
                </td>
                <td width="10%"><xsl:value-of select="height"/></td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Weight"/>:
                </td>
                <td width="10%"><xsl:value-of select="weight"/></td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Hair"/>:
                </td>
                <td width="10%"><xsl:value-of select="hair"/></td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Eyes"/>:
                </td>
                <td width="10%"><xsl:value-of select="eyes"/></td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Skin"/>:
                </td>
                <td width="10%"><xsl:value-of select="skin"/></td>
              </tr>
              <tr>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Karma"/>:
                </td>
                <td width="10%"><xsl:value-of select="karma"/></td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Career"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$lang.Karma"/>:
                </td>
                <td width="10%"><xsl:value-of select="totalkarma"/></td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.StreetCred"/>:
                </td>
                <td width="10%"><xsl:value-of select="totalstreetcred"/></td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Notoriety"/>:
                </td>
                <td width="10%"><xsl:value-of select="totalnotoriety"/></td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.PublicAwareness"/>:
                </td>
                <td width="10%"><xsl:value-of select="totalpublicawareness"/></td>
              </tr>
              <tr>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Composure"/>:
                </td>
                <td width="10%"><xsl:value-of select="composure"/></td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.JudgeIntentions"/>:
                </td>
                <td width="10%"><xsl:value-of select="judgeintentions"/></td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.LiftCarry"/>:
                </td>
                <td width="10%"><xsl:value-of select="liftandcarry"/></td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.LiftCarry"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$lang.Weight"/>:
                </td>
                <td width="10%">
                  <xsl:value-of select="liftweight"/> kg/<xsl:value-of
                    select="carryweight"/> kg
                </td>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.PrimaryArm"/>:
                </td>
                <td width="10%"><xsl:value-of select="primaryarm"/></td>
              </tr>
              <tr>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Memory"/>:
                </td>
                <td width="10%"><xsl:value-of select="memory"/></td>
                <td width="10%"/>
                <td width="10%"/>
                <td width="10%" class="upper">
                  <xsl:value-of select="$lang.Movement"/>:
                </td>
                <td colspan="4">
                  <xsl:call-template name="MovementRate"/>
                </td>
                <td width="10%"/>
              </tr>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.PersonalData"/>
            <xsl:with-param name="blockname" select="'PersonalDataBlock'"/>
          </xsl:call-template>

          <div class="block" id="AttributesBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="25%">
                  <xsl:value-of select="$lang.Physical"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$lang.Attributes"/>
                </th>
                <th width="25%">
                  <xsl:value-of select="$lang.Mental"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$lang.Attributes"/>
                </th>
                <th width="25%">
                  <xsl:value-of select="$lang.Special"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$lang.Attributes"/>
                </th>
                <th width="25%">
                  <xsl:value-of select="$lang.Initiative"/>
                </th>
              </tr>
              <tr>
                <td width="25%" class="attributecell">
                  <p> <xsl:value-of select="$lang.Body"/>:
                    <xsl:value-of select="attributes/attribute[name_english = 'BOD']/base"/>
                    <xsl:if test="attributes/attribute[name_english = 'BOD']/total != attributes/attribute[name_english = 'BOD']/base"
                    > (<xsl:value-of select="attributes/attribute[name_english = 'BOD']/total"
                    />) </xsl:if>
                  </p>
                </td>
                <td width="25%" class="attributecell">
                  <p> <xsl:value-of select="$lang.Charisma"/>:
                    <xsl:value-of select="attributes/attribute[name_english = 'CHA']/base"/>
                    <xsl:if test="attributes/attribute[name_english = 'CHA']/total != attributes/attribute[name_english = 'CHA']/base"
                    > (<xsl:value-of
                      select="attributes/attribute[name_english = 'CHA']/total"
                    />) </xsl:if>
                  </p>
                </td>
                <td width="25%" class="attributecell">
                  <p> <xsl:value-of select="$lang.Edge"/>:
                    <xsl:value-of select="attributes/attribute[name_english = 'EDG']/base"/>
                    <xsl:if test="attributes/attribute[name_english = 'EDG']/total != attributes/attribute[name_english = 'EDG']/base"
                    > (<xsl:value-of
                      select="attributes/attribute[name_english = 'EDG']/total"
                    />) </xsl:if>
                  </p>
                </td>
                <td width="25%" class="attributecell">
                  <p> <xsl:value-of select="$lang.Initiative"/>:
                  <xsl:value-of select="init"/></p>
                </td>
              </tr>
              <tr>
                <td width="25%" class="attributecell">
                  <p> <xsl:value-of select="$lang.Agility"/>:
                    <xsl:value-of select="attributes/attribute[name_english = 'AGI']/base"/>
                    <xsl:if test="attributes/attribute[name_english = 'AGI']/total != attributes/attribute[name_english = 'AGI']/base"
                    > (<xsl:value-of
                      select="attributes/attribute[name_english = 'AGI']/total"
                    />) </xsl:if>
                  </p>
                </td>
                <td width="25%" class="attributecell">
                  <p> <xsl:value-of select="$lang.Intuition"/>:
                    <xsl:value-of select="attributes/attribute[name_english = 'INT']/base"/>
                    <xsl:if test="attributes/attribute[name_english = 'INT']/total != attributes/attribute[name_english = 'INT']/base"
                    > (<xsl:value-of
                      select="attributes/attribute[name_english = 'INT']/total"
                    />) </xsl:if>
                  </p>
                </td>
                <td width="25%" class="attributecell">
                  <p>  <xsl:value-of select="$lang.CurrentEdge"/>:
                  </p>
                </td>
                <td width="25%" class="attributecell">
                  <p>
                    <xsl:choose>
                      <xsl:when test="magenabled = 'True'">
                        <xsl:value-of select="$lang.AstralInitiative"/>:
                        <xsl:value-of select="astralinit"/>
                      </xsl:when>
                      <xsl:otherwise>
                        &#160;
                      </xsl:otherwise>
                    </xsl:choose>
                  </p>
                </td>
              </tr>
              <tr>
                <td width="25%" class="attributecell">
                  <p> <xsl:value-of select="$lang.Reaction"/>:
                    <xsl:value-of select="attributes/attribute[name_english = 'REA']/base"/>
                    <xsl:if test="attributes/attribute[name_english = 'REA']/total != attributes/attribute[name_english = 'REA']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'REA']/total"/>)
                    </xsl:if>
                  </p>
                </td>
                <td width="25%" class="attributecell">
                  <p> <xsl:value-of select="$lang.Logic"/>:
                    <xsl:value-of select="attributes/attribute[name_english = 'LOG']/base"/>
                    <xsl:if test="attributes/attribute[name_english = 'LOG']/total != attributes/attribute[name_english = 'LOG']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'LOG']/total"/>)
                    </xsl:if>
                  </p>
                </td>
                <td width="25%" class="attributecell">
                  <p> <xsl:value-of select="$lang.Essence"/>:
                    <xsl:value-of select="totaless"/>
                  </p>
                </td>
                <td width="25%" class="attributecell">
                  <p>  <xsl:value-of select="$lang.RiggerInitiative"/>:
                    <xsl:value-of select="riggerinit"/>
                  </p>
                </td>
              </tr>
              <tr>
                <td width="25%" class="attributecell">
                  <p> <xsl:value-of select="$lang.Strength"/>:
                    <xsl:value-of select="attributes/attribute[name_english = 'STR']/base"/>
                    <xsl:if test="attributes/attribute[name_english = 'STR']/total != attributes/attribute[name_english = 'STR']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'STR']/total"/>)
                    </xsl:if>
                  </p>
                </td>
                <td width="25%" class="attributecell">
                  <p> <xsl:value-of select="$lang.Willpower"/>:
                    <xsl:value-of select="attributes/attribute[name_english = 'WIL']/base"/>
                    <xsl:if test="attributes/attribute[name_english = 'WIL']/total != attributes/attribute[name_english = 'WIL']/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'WIL']/total"/>)
                    </xsl:if>
                  </p>
                </td>
                <td width="25%" class="attributecell">
                  <p>
                    <xsl:choose>
                      <xsl:when test="magenabled = 'True'">
                        <xsl:value-of select="$lang.Magic"/>:
                        <xsl:value-of select="attributes/attribute[name_english = 'MAG']/base"/>
                        <xsl:if test="attributes/attribute[name_english = 'MAG']/total != attributes/attribute[name_english = 'MAG']/base">
                          (<xsl:value-of select="attributes/attribute[name_english = 'MAG']/total"/>)
                        </xsl:if>
                      </xsl:when>
                      <xsl:when test="resenabled = 'True'">
                        <xsl:value-of select="$lang.Resonance"/>:
                        <xsl:value-of select="attributes/attribute[name_english = 'RES']/base"/>
                        <xsl:if test="attributes/attribute[name_english = 'RES']/total != attributes/attribute[name_english = 'RES']/base">
                          (<xsl:value-of select="attributes/attribute[name_english = 'RES']/total"/>)
                        </xsl:if>
                      </xsl:when>
                      <xsl:otherwise>
                        &#160;
                      </xsl:otherwise>
                    </xsl:choose>
                  </p>
                </td>
                <td width="25%" class="attributecell">
                  <p> <xsl:value-of select="$lang.MatrixAR"/>:
                    <xsl:value-of select="matrixarinit"/><br />
                    <xsl:value-of select="$lang.MatrixCold"/>:
                    <xsl:value-of select="matrixcoldinit"/><br />
                    <xsl:value-of select="$lang.MatrixHot"/>:
                    <xsl:value-of select="matrixhotinit"/>
                  </p>
                </td>
              </tr>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Attributes"/>
            <xsl:with-param name="blockname" select="'AttributesBlock'"/>
          </xsl:call-template>

          <div class="block" id="LimitsBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="25%">
                  <xsl:value-of select="$lang.PhysicalLimit"/>:
                  <xsl:value-of select="limitphysical"/>
                </th>
                <th width="25%" style="border-left: solid 1px #1c4a2d; border-right: solid 1px #1c4a2d;"  valign="top">
                  <xsl:value-of select="$lang.MentalLimit"/>:
                  <xsl:value-of select="limitmental"/>
                </th>
                <th width="25%" style="border-right: solid 1px #1c4a2d;">
                  <xsl:value-of select="$lang.SocialLimit"/>:
                  <xsl:value-of select="limitsocial"/>
                </th>
                <th width="25%">
                  <xsl:value-of select="$lang.AstralLimit"/>:
                  <xsl:value-of select="limitastral"/>
                </th>
              </tr>
              <tr>
                <td>
                  <table>
                    <xsl:call-template name="limitmodifiersphys"/>
                    <tr><td/></tr>
                  </table>
                </td>
                <td style="border-left: solid 1px #1c4a2d; border-right: solid 1px #1c4a2d;">
                  <table>
                    <xsl:call-template name="limitmodifiersment"/>
                    <tr><td/></tr>
                  </table>
                </td>
                <td style="border-right: solid 1px #1c4a2d;">
                  <table>
                    <xsl:call-template name="limitmodifierssoc"/>
                    <tr><td/></tr>
                  </table>
                </td>
                <td>
                  <table>
                    <xsl:call-template name="limitmodifiersast"/>
                    <tr><td/></tr>
                  </table>
                </td>
              </tr>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Limits"/>
            <xsl:with-param name="blockname" select="'LimitsBlock'"/>
          </xsl:call-template>

          <div class="block" id="SkillsBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <td width="33%">
                  <table width="100%" >
                    <tr>
                      <th colspan="100%">
                        <xsl:value-of select="$lang.ActiveSkills"/>
                      </th>
                    </tr>
                    <tr>
                      <th width="70%" style="text-align: left">
                        <xsl:value-of select="$lang.Skill"/>
                      </th>
                      <th width="10%">
                        <xsl:value-of select="$lang.Rtg"/>
                      </th>
                      <th width="20%">
                        <xsl:value-of select="$lang.Pool"/>
                      </th>
                    </tr>
                    <xsl:call-template name="skills1"/>
                  </table>
                </td>
                <td width="33%" style="border-left: solid 1px #1c4a2d; border-right: solid 1px #1c4a2d;">
                  <table width="100%" >
                    <tr>
                      <th colspan="100%">
                        <xsl:value-of select="$lang.ActiveSkills"/>
                      </th>
                    </tr>
                    <tr>
                      <th width="70%" style="text-align: left">
                        <xsl:value-of select="$lang.Skill"/>
                      </th>
                      <th width="10%">
                        <xsl:value-of select="$lang.Rtg"/>
                      </th>
                      <th width="20%">
                        <xsl:value-of select="$lang.Pool"/>
                      </th>
                    </tr>
                    <xsl:call-template name="skills2"/>
                  </table>
                </td>
                <td width="33%">
                  <table width="100%" >
                    <tr>
                      <th colspan="100%">
                        <xsl:value-of select="$lang.KnowledgeSkills"/>
                      </th>
                    </tr>
                    <tr>
                      <th width="70%" style="text-align: left">
                        <xsl:value-of select="$lang.Skill"/>
                      </th>
                      <th width="10%">
                        <xsl:value-of select="$lang.Rtg"/>
                      </th>
                      <th width="20%">
                        <xsl:value-of select="$lang.Pool"/>
                      </th>
                    </tr>
                    <xsl:call-template name="skills3"/>
                  </table>
                </td>
              </tr>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Skills"/>
            <xsl:with-param name="blockname" select="'SkillsBlock'"/>
          </xsl:call-template>

          <div class="block" id="QualitiesBlock">
            <table><tr><td/></tr></table>
            <table width="100%" style="border-width: 0; border-collapse: collapse;">
              <tr>
                <td width="67%" style="text-align: center">
                  <table class="tablestyle">
                    <tr>
                      <th width="80%" style="text-align: left">
                        <xsl:value-of select="$lang.Quality"/>
                      </th>
                      <th width="10%"/>
                      <th width="10%"/>
                    </tr>
                    <xsl:call-template name="Qualities"/>
                  </table>
                  <xsl:call-template name="RowSummary">
                    <xsl:with-param name="text" select="$lang.Qualities"/>
                    <xsl:with-param name="blockname" select="'QualitiesBlock'"/>
                  </xsl:call-template>
                </td>
                <td width="33%">
                  <table class="tablestyle">
                    <tr>
                      <td width="3%"/>
                      <td width="46%" style="font-weight: bold; text-transform: uppercase">
                        <xsl:value-of select="$lang.PhysicalTrack1"/>
                      </td>
                      <th width="3%"/>
                      <td width="46%" style="font-weight: bold; text-transform: uppercase">
                        <xsl:value-of select="$lang.StunTrack1"/>
                      </td>
                      <td width="2%"/>
                    </tr>
                    <tr class="title">
                      <td/>
                      <td><xsl:value-of select="$lang.PhysicalTrack2"/></td>
                      <td/>
                      <td><xsl:value-of select="$lang.StunTrack2"/></td>
                      <td/>
                    </tr>
                    <tr>
                      <td/>
                      <td>
                        <xsl:call-template name="ConditionMonitor">
                          <xsl:with-param name="PenaltyBox">
                            <xsl:value-of select="cmthreshold"/>
                          </xsl:with-param>
                          <xsl:with-param name="Offset">
                            <xsl:value-of select="cmthresholdoffset"/>
                          </xsl:with-param>
                          <xsl:with-param name="TotalBoxes">
                            <xsl:value-of select="physicalcm"/>
                          </xsl:with-param>
                          <xsl:with-param name="DamageTaken">
                            <xsl:value-of select="physicalcmfilled"/>
                          </xsl:with-param>
                          <xsl:with-param name="OverFlow">
                            <xsl:value-of select="cmoverflow"/>
                          </xsl:with-param>
                        </xsl:call-template>
                      </td>
                      <td/>
                      <td>
                        <xsl:call-template name="ConditionMonitor">
                          <xsl:with-param name="PenaltyBox">
                            <xsl:value-of select="cmthreshold"/>
                          </xsl:with-param>
                          <xsl:with-param name="Offset">
                            <xsl:value-of select="cmthresholdoffset"/>
                          </xsl:with-param>
                          <xsl:with-param name="TotalBoxes">
                            <xsl:value-of select="stuncm"/>
                          </xsl:with-param>
                          <xsl:with-param name="DamageTaken">
                            <xsl:value-of select="stuncmfilled"/>
                          </xsl:with-param>
                        </xsl:call-template>
                      </td>
                      <td/>
                    </tr>
                    <tr><td colspan="100%" style="line-height: 5px"><xsl:text>&#x20;</xsl:text></td></tr>
                  </table>
                  <xsl:call-template name="RowSummary">
                    <xsl:with-param name="text" select="$lang.ConditionMonitor"/>
                    <xsl:with-param name="buttons" select="'N'"/>
                  </xsl:call-template>
                </td>
              </tr>
            </table>
          </div>

        <xsl:if test="powers/power">
          <div class="block" id="PowersBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="40%"><xsl:value-of select="$lang.Power"/></th>
                <th width="20%"><xsl:value-of select="$lang.Rating"/></th>
                <th width="20%">
                  <xsl:value-of select="$lang.Points"/>
                  (<xsl:value-of select="$lang.Total"/>)
                </th>
                <th width="20%"/>
              </tr>
              <xsl:call-template name="powers"/>
              <xsl:if test="enhancements != ''">
                <tr>
                  <th><xsl:value-of select="$lang.Enhancements"/></th>
                </tr>
                <xsl:for-each select="enhancements/enhancement">
                  <xsl:sort select="name"/>
                  <tr>
                    <td width="90%" class="indent">
                      <xsl:value-of select="name"/>
                    </td>
                    <td width="10%" style="text-align: center">
                      <xsl:value-of select="source"/>
                      <xsl:text> </xsl:text>
                      <xsl:value-of select="page"/>
                    </td>
                  </tr>
                  <xsl:if test="notes != '' and $ProduceNotes">
                    <tr><td colspan="100%" class="notesrow2">
                      <xsl:call-template name="PreserveLineBreaks">
                        <xsl:with-param name="text" select="notes"/>
                      </xsl:call-template>
                    </td></tr>
                  </xsl:if>
                  <xsl:call-template name="Xline">
                    <xsl:with-param name="cntl" select="last()-position()"/>
                    <xsl:with-param name="nte" select="notes != '' and $ProduceNotes"/>
                  </xsl:call-template>
                </xsl:for-each>
              </xsl:if>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.AdeptPowers"/>
            <xsl:with-param name="blockname" select="'PowersBlock'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="martialarts/martialart">
          <div class="block" id="MartialArtsBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="80%" style="text-align: left">
                  <xsl:value-of select="$lang.MartialArt"/>
                </th>
                <th width="20%"/>
              </tr>
              <xsl:call-template name="martialarts"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.MartialArts"/>
            <xsl:with-param name="blockname" select="'MartialArtsBlock'"/>
          </xsl:call-template>
        </xsl:if>

<!--
        *                      *
        ***                      ***
        *****       gear details      *****
        ***                      ***
        *                        *
-->
        <xsl:if test="cyberwares/cyberware">
          <div class="block" id="CyberwareBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="50%" style="text-align: left">
                  <xsl:value-of select="$lang.Implant"/>
                </th>
                <th width="20%"><xsl:value-of select="$lang.Essence"/></th>
                <th width="20%"><xsl:value-of select="$lang.Grade"/></th>
                <th width="10%"/>
              </tr>
              <xsl:call-template name="cyberware"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text"
                    select="concat($lang.Cyberware,'/',$lang.Bioware)"/>
            <xsl:with-param name="blockname" select="'CyberwareBlock'"/>
          </xsl:call-template>
        </xsl:if>

          <div class="block" id="ArmorBlock">
            <table><tr><td/></tr></table>
            <table width="100%" class="tablestyle">
              <tr>
                <th width="60%" style="text-align: left">
                  <xsl:value-of select="$lang.Armor"/>
                </th>
                <th width="10%">
                  <xsl:value-of select="$lang.ArmorValue"/>
                </th>
                <th width="10%">
                  <xsl:value-of select="$lang.Equipped"/>
                </th>
                <th width="10%"/>
                <th width="10%"/>
              </tr>
              <xsl:call-template name="armor"/>
              <xsl:variable name="sumarmor">
                <xsl:value-of select="sum(armors/armor[equipped='True']/armor)"/>
              </xsl:variable>
              <xsl:variable name="otherarmor" select="armor - $sumarmor"/>
              <xsl:if test="$otherarmor != 0">
                <tr>
                  <td>
                    <xsl:value-of select="$lang.Other"/>
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="$lang.Modifiers"/>
                  </td>
                  <td style="text-align: center">
                    <xsl:call-template name="fnx-pad-l">
                      <xsl:with-param name="string" select="$otherarmor"/>
                      <xsl:with-param name="length" select="2"/>
                    </xsl:call-template>
                  </td>
                  <td/>
                  <td/>
                  <td/>
                </tr>
              </xsl:if>
              <tr>
                <td style="font-weight: bold">
                  <xsl:value-of select="$lang.Total"/>
                </td>
                <td style="font-weight: bold; text-align: center; text-decoration: overline;">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="armor"/>
                    <xsl:with-param name="length" select="2"/>
                  </xsl:call-template>
                </td>
                <td/>
                <td/>
                <td/>
              </tr>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Armor"/>
            <xsl:with-param name="blockname" select="'ArmorBlock'"/>
          </xsl:call-template>

        <xsl:if test="weapons/weapon[type = 'Ranged']">
          <div class="block" id="RangedWeaponsBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="20%" style="text-align: left">
                  <xsl:value-of select="$lang.Weapon"/>
                </th>
                <th width="8%"><xsl:value-of select="$lang.Pool"/></th>
                <th width="11%"><xsl:value-of select="$lang.Accuracy"/></th>
                <th width="16%"><xsl:value-of select="$lang.Damage"/></th>
                <th width="9%"><xsl:value-of select="$lang.AP"/></th>
                <th width="8%"><xsl:value-of select="$lang.Mode"/></th>
                <th width="4%"><xsl:value-of select="$lang.RC"/></th>
                <th width="7%">
                  <xsl:value-of select="$lang.Ammo"/>
                </th>
                <th width="7%">
                  [<xsl:value-of select="$lang.Loaded"/>]
                </th>
                <th width="10%"/>
              </tr>
              <xsl:for-each select="weapons/weapon[type = 'Ranged']">
                <xsl:sort select="name"/>
                <xsl:call-template name="RangedWeapons">
                  <xsl:with-param name="weapon" select="weapons/weapon"/>
                </xsl:call-template>
              </xsl:for-each>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.RangedWeapons"/>
            <xsl:with-param name="blockname" select="'RangedWeaponsBlock'"/>
          </xsl:call-template>
        </xsl:if>

          <div class="block" id="MeleeWeaponsBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="30%" style="text-align: left">
                  <xsl:value-of select="$lang.Weapon"/>
                </th>
                <th width="8%"><xsl:value-of select="$lang.Pool"/></th>
                <th width="11%"><xsl:value-of select="$lang.Accuracy"/></th>
                <th width="13%"><xsl:value-of select="$lang.Damage"/></th>
                <th width="8%"><xsl:value-of select="$lang.AP"/></th>
                <th width="10%"><xsl:value-of select="$lang.Reach"/></th>
                <th width="10%"/>
                <th width="10%"/>
              </tr>
              <xsl:variable name="sortedcopy">
                <xsl:for-each select="weapons/weapon[type = 'Melee']">
                  <xsl:sort select="name"/>
                  <xsl:copy-of select="current()"/>
                </xsl:for-each>
              </xsl:variable>
              <xsl:for-each select="msxsl:node-set($sortedcopy)/weapon">
                <xsl:if test="position()=1 or name != preceding-sibling::weapon[1]/name">
                  <xsl:call-template name="meleeweapons">
                    <xsl:with-param name="weapon" select="weapons/weapon"/>
                  </xsl:call-template>
                </xsl:if>
              </xsl:for-each>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.MeleeWeapons"/>
            <xsl:with-param name="blockname" select="'MeleeWeaponsBlock'"/>
          </xsl:call-template>

          <div class="block" id="GearBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <td width="33%">
                  <table width="100%" >
                    <tr>
                      <th width="58%" style="text-align: left">
                        <xsl:value-of select="$lang.Name"/>
                      </th>
                      <th width="12%"><xsl:value-of select="$lang.Rtg"/></th>
                      <th width="12%"><xsl:value-of select="$lang.Qty"/></th>
                      <th width="18%"/>
                    </tr>
                    <xsl:call-template name="gear1"/>
                  </table>
                </td>
                <td width="33%" style="border-left: solid 1px #1c4a2d; border-right: solid 1px #1c4a2d;">
                  <table width="100%" >
                    <tr>
                      <th width="58%" style="text-align: left">
                        <xsl:value-of select="$lang.Name"/>
                      </th>
                      <th width="12%"><xsl:value-of select="$lang.Rtg"/></th>
                      <th width="12%"><xsl:value-of select="$lang.Qty"/></th>
                      <th width="18%"/>
                    </tr>
                    <xsl:call-template name="gear2"/>
                  </table>
                </td>
                <td width="33%">
                  <table width="100%" >
                    <tr>
                      <th width="58%" style="text-align: left">
                        <xsl:value-of select="$lang.Name"/>
                      </th>
                      <th width="12%"><xsl:value-of select="$lang.Rtg"/></th>
                      <th width="12%"><xsl:value-of select="$lang.Qty"/></th>
                      <th width="18%"/>
                    </tr>
                    <xsl:call-template name="gear3"/>
                  </table>
                </td>
              </tr>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Gear"/>
            <xsl:with-param name="blockname" select="'GearBlock'"/>
          </xsl:call-template>

        <xsl:if test="gears/gear[iscommlink = 'True']">
          <div class="block" id="DevicesBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="30%" style="text-align: left">
                  <xsl:value-of select="$lang.Device"/>
                </th>
                <th width="16%"><xsl:value-of select="$lang.Category"/></th>
                <th width="6%"><xsl:value-of select="$lang.DeviceRating"/></th>
                <th width="8%"><xsl:value-of select="$lang.Attack"/></th>
                <th width="8%"><xsl:value-of select="$lang.Sleaze"/></th>
                <th width="12%"><xsl:value-of select="$lang.DataProc"/></th>
                <th width="10%"><xsl:value-of select="$lang.Firewall"/></th>
                <th width="10%"/>
              </tr>
              <xsl:call-template name="commlink"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="concat($lang.Devices,'/',$lang.Programs)"/>
            <xsl:with-param name="blockname" select="'DevicesBlock'"/>
          </xsl:call-template>
        </xsl:if>

          <xsl:for-each select="vehicles/vehicle">
            <xsl:sort select="name"/>
            <xsl:call-template name="vehicles">
              <xsl:with-param name="vehicle"/>
              <xsl:with-param name="VehicleNumber">VehicleBlock<xsl:value-of select="position()"/></xsl:with-param>
            </xsl:call-template>
          </xsl:for-each>

        <xsl:if test="lifestyles != ''">
          <div class="block" id="LifestyleBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <xsl:call-template name="Lifestyles"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Lifestyle"/>
            <xsl:with-param name="blockname" select="'LifestyleBlock'"/>
          </xsl:call-template>
        </xsl:if>
<!-- ** ** ** end of gear ** ** ** -->

<!--
        *                      *
        ***                      ***
        *****     magic user details      *****
        ***                      ***
        *                      *
-->
      <xsl:if test="magenabled = 'True'">
        <xsl:if test="tradition/name != ''">
          <div class="block" id="TraditionBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="22%" style="text-align: left">
                  <xsl:value-of select="$lang.Tradition"/>
                </th>
                <th width="13%">
                  <xsl:value-of select="$lang.Drain"/>
                </th>
                <th width="11%" style="font-size: 95%">
                  <xsl:value-of select="$lang.Combat"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$lang.Spirit"/>
                </th>
                <th width="11%" style="font-size: 95%">
                  <xsl:value-of select="$lang.Detection"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$lang.Spirit"/>
                </th>
                <th width="11%" style="font-size: 95%">
                  <xsl:value-of select="$lang.Health"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$lang.Spirit"/>
                </th>
                <th width="11%" style="font-size: 95%">
                  <xsl:value-of select="$lang.Illusion"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$lang.Spirit"/>
                </th>
                <th width="11%" style="font-size: 95%">
                  <xsl:value-of select="$lang.Manipulation"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$lang.Spirit"/>
                </th>
                <th width="10%"/>
              </tr>
              <xsl:call-template name="tradition"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Tradition"/>
            <xsl:with-param name="blockname" select="'TraditionBlock'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="initiategrade > 0">
          <div class="block" id="InitiationBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="80%" style="text-align: left">
                  <xsl:value-of select="$lang.InitiateGrade"/>:
                  <xsl:value-of select="initiategrade"/>
                </th>
                <th width="10%"/>
                <th width="10%"/>
              </tr>
              <xsl:if test="arts != ''">
                <tr>
                  <th width="100%"><xsl:value-of select="$lang.Arts"/></th>
                </tr>
                <tr><td>
                  <xsl:for-each select="arts/art">
                    <xsl:sort select="name"/>
                    <xsl:value-of select="name"/>
                    <xsl:value-of select="source"/>
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="page"/>
                    <xsl:if test="notes !=''">
                      <br><xsl:value-of select="notes"/></br>
                    </xsl:if>
                    <xsl:if test="last() &gt; 1">; </xsl:if>
                  </xsl:for-each>
                </td></tr>
              </xsl:if>
              <xsl:if test="metamagics/metamagic">
                <tr>
                  <td width="80%">
                    <strong><xsl:value-of select="$lang.Metamagics"/></strong>
                  </td>
                  <td/>
                  <td/>
                </tr>
                <xsl:for-each select="metamagics/metamagic">
                  <xsl:sort select="name"/>
                  <xsl:if test="position() != 1">
                    <xsl:call-template name="Xline"/>
                  </xsl:if>
                  <tr>
                    <td style="text-align: left">
                      <xsl:value-of select="name"/>
                    </td>
                    <td/>
                    <td style="text-align: center">
                      <xsl:value-of select="source"/>
                      <xsl:text> </xsl:text>
                      <xsl:value-of select="page"/>
                    </td>
                  </tr>
                  <xsl:if test="notes != '' and $ProduceNotes">
                    <tr><td colspan="100%" class="notesrow2">
                      <xsl:call-template name="PreserveLineBreaks">
                        <xsl:with-param name="text" select="notes"/>
                      </xsl:call-template>
                    </td></tr>
                  </xsl:if>
                </xsl:for-each>
              </xsl:if>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Initiation"/>
            <xsl:with-param name="blockname" select="'InitiationBlock'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="spells/spell">
          <div class="block" id="SpellsBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <xsl:call-template name="Spells"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Spells"/>
            <xsl:with-param name="blockname" select="'SpellsBlock'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="spirits/spirit">
          <div class="block" id="SpiritsBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="25%" style="text-align: left">
                  <xsl:value-of select="$lang.Spirit"/>
                </th>
                <th width="25%"><xsl:value-of select="$lang.Force"/></th>
                <th width="25%"><xsl:value-of select="$lang.Services"/></th>
                <th width="25%">
                  <xsl:value-of select="$lang.Bound"/>/<xsl:value-of select="$lang.Unbound"/>
                </th>
              </tr>
              <xsl:for-each select="spirits/spirit">
                <xsl:sort select="name"/>
                <tr>
                  <td><xsl:value-of select="name"/></td>
                  <td style="text-align: center"><xsl:value-of select="force"/></td>
                  <td style="text-align: center"><xsl:value-of select="services"/></td>
                  <td style="text-align: center">
                    <xsl:choose>
                      <xsl:when test="bound = 'True'">
                        <xsl:value-of select="$lang.Bound"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$lang.Unbound"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </td>
                </tr>
                <xsl:if test="notes != '' and $ProduceNotes">
                  <tr>
                    <xsl:if test="position() mod 2 != 1">
                      <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
                    </xsl:if>
                    <td colspan="100%" class="notesrow2">
                      <xsl:call-template name="PreserveLineBreaks">
                        <xsl:with-param name="text" select="notes"/>
                      </xsl:call-template>
                    </td>
                  </tr>
                </xsl:if>
              </xsl:for-each>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Spirits"/>
            <xsl:with-param name="blockname" select="'SpiritsBlock'"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:if>
<!-- ** ** ** end of magic user details ** ** ** -->

<!--
      *                      *
      ***                      ***
      *****     Technomancer details    *****
      ***                      ***
      *                      *
-->
      <xsl:if test="resenabled = 'True'">
        <div class="block" id="StreamBlock">
          <table><tr><td/></tr></table>
          <table class="tablestyle">
            <tr>
              <th width="25%" style="text-align: left">
                <xsl:value-of select="$lang.Stream"/>
              </th>
              <th width="10%">
                <xsl:value-of select="$lang.Drain"/>
              </th>
              <th width="55%"/>
              <th width="10%"/>
            </tr>
            <tr>
              <td><xsl:value-of select="stream"/></td>
              <td><xsl:value-of select="drain"/></td>
              <td/>
              <td style="text-align:center;">
                <xsl:value-of select="tradition/source" />
                <xsl:text> </xsl:text>
                <xsl:value-of select="tradition/page" />
              </td>
            </tr>
          </table>
        </div>
        <xsl:call-template name="RowSummary">
          <xsl:with-param name="text" select="$lang.Stream"/>
          <xsl:with-param name="blockname" select="'StreamBlock'"/>
        </xsl:call-template>

        <xsl:if test="submersiongrade > 0">
          <div class="block" id="EchoesBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th style="text-align: left">
                  <xsl:value-of select="$lang.SubmersionGrade"/>:
                  <xsl:value-of select="submersiongrade"/>
                </th>
                <th width="10%"/>
                <th width="10%"/>
              </tr>
              <xsl:if test="metamagics/metamagic">
                <tr>
                  <td width="80%" style="text-align: left">
                    <strong><xsl:value-of select="$lang.Echoes"/></strong>
                  </td>
                  <td width="10%"/>
                  <td width="10%"/>
                </tr>
                <xsl:for-each select="metamagics/metamagic">
                  <xsl:sort select="name"/>
                  <xsl:if test="position() != 1">
                    <xsl:call-template name="Xline"/>
                  </xsl:if>
                  <tr>
                    <td><xsl:value-of select="name"/></td>
                    <td/>
                    <td style="text-align: center">
                      <xsl:value-of select="source"/>
                      <xsl:text> </xsl:text>
                      <xsl:value-of select="page"/>
                    </td>
                  </tr>
                  <xsl:if test="notes != '' and $ProduceNotes">
                    <tr><td colspan="100%" class="notesrow2">
                      <xsl:call-template name="PreserveLineBreaks">
                        <xsl:with-param name="text" select="notes"/>
                      </xsl:call-template>
                    </td></tr>
                  </xsl:if>
                </xsl:for-each>
              </xsl:if>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Echoes"/>
            <xsl:with-param name="blockname" select="'EchoesBlock'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="complexforms/complexform">
          <div class="block" id="ComplexFormsBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="40%" style="text-align: left">
                  <xsl:value-of select="$lang.ComplexForm"/>
                </th>
                <th width="15%"><xsl:value-of select="$lang.Target"/></th>
                <th width="15%"><xsl:value-of select="$lang.Duration"/></th>
                <th width="15%"><xsl:value-of select="$lang.FV"/></th>
                <th width="5%"/>
                <th width="10%"/>
              </tr>
              <xsl:call-template name="complexforms"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.ComplexForms"/>
            <xsl:with-param name="blockname" select="'ComplexFormsBlock'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="spirits/spirit">
          <div class="block" id="SpritesBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="25%" style="text-align: left">
                  <xsl:value-of select="$lang.Sprite"/>
                </th>
                <th width="25%"><xsl:value-of select="$lang.Rating"/></th>
                <th width="25%"><xsl:value-of select="$lang.Services"/></th>
                <th width="25%">
                  <xsl:value-of select="$lang.Registered"/>/<xsl:value-of select="$lang.Unregistered"/>
                </th>
              </tr>
              <xsl:for-each select="spirits/spirit">
                <xsl:sort select="name"/>
                <tr>
                  <td><xsl:value-of select="name"/></td>
                  <td style="text-align: center"><xsl:value-of select="force"/></td>
                  <td style="text-align: center"><xsl:value-of select="services"/></td>
                  <td style="text-align: center">
                    <xsl:choose>
                      <xsl:when test="bound = 'True'">
                        <xsl:value-of select="$lang.Registered"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$lang.Unregistered"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </td>
                </tr>
                <xsl:if test="notes != '' and $ProduceNotes">
                  <tr>
                    <xsl:if test="position() mod 2 != 1">
                      <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
                    </xsl:if>
                    <td colspan="100%" class="notesrow2">
                      <xsl:call-template name="PreserveLineBreaks">
                        <xsl:with-param name="text" select="notes"/>
                      </xsl:call-template>
                    </td>
                  </tr>
                </xsl:if>
              </xsl:for-each>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Sprites"/>
            <xsl:with-param name="blockname" select="'SpritesBlock'"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:if>
<!-- ** ** ** end of Technomancer details ** ** ** -->

        <xsl:if test="aiprograms/aiprogram">
          <div class="block" id="AIProgramBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="40%" style="text-align: left">
                  <xsl:value-of select="$lang.Name"/>
                </th>
                <th width="40%"><xsl:value-of select="$lang.Requires"/>&#160;<xsl:value-of select="$lang.Program"/></th>
                <th/>
                <th width="10%" style="text-align:center;"></th>
              </tr>
              <xsl:call-template name="aiprograms"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text"
              select="$lang.AIandAdvanced"/>
            <xsl:with-param name="blockname" select="'AIProgramBlock'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="contacts/contact">
          <div class="block" id="ContactsBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="25%" style="text-align: left">
                  <xsl:value-of select="$lang.Contact"/>
                </th>
                <th width="25%"><xsl:value-of select="$lang.Location"/></th>
                <th width="25%"><xsl:value-of select="$lang.Archetype"/></th>
                <th width="15%"><xsl:value-of select="$lang.Connection"/></th>
                <th width="10%"><xsl:value-of select="$lang.Loyalty"/></th>
              </tr>
              <xsl:call-template name="Contacts"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Contacts"/>
            <xsl:with-param name="blockname" select="'ContactsBlock'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="critterpowers/critterpower">
          <div class="block" id="CritterBlock">
            <table><tr><td/></tr></table>
            <table class="tablestyle">
              <tr>
                <th width="50%" style="text-align: left">
                  <xsl:value-of select="$lang.Critter"/>
                </th>
                <th width="30%"><xsl:value-of select="$lang.Rating"/></th>
                <th width="20%"/>
              </tr>
              <xsl:call-template name="critterpowers"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Critters"/>
            <xsl:with-param name="blockname" select="'CritterBlock'"/>
          </xsl:call-template>
        </xsl:if>

<!--
        *                      *
        ***                      ***
        *****      notes and such      *****
        ***                      ***
        *                      *
-->
      <xsl:if test="$ProduceNotes">
        <xsl:if test="concat(concept,description,background,notes,gamenotes) != ''">
          <xsl:call-template name="notes"/>
        </xsl:if>

        <xsl:if test="hasothermugshots = 'yes'">
          <table><tr><td/></tr></table>
          <div class="block" id="OtherMugshotsBlock">
            <xsl:call-template name="othermugshots"/>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.OtherMugshots"/>
            <xsl:with-param name="blockname" select="'OtherMugshotsBlock'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="calendar/week">
          <table><tr><td/></tr></table>
          <div class="block" id="CalendarBlock">
            <table class="tablestyle">
              <tr>
                <th width="20%" style="text-align: left">
                  <xsl:value-of select="$lang.Date"/>
                </th>
                <th width="80%" style="text-align: left">
                  <xsl:value-of select="$lang.Notes"/>
                </th>
              </tr>
              <xsl:call-template name="Calendar"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Calendar"/>
            <xsl:with-param name="blockname" select="'CalendarBlock'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="expenses/expense">
          <table><tr><td/></tr></table>
          <div class="block" id="ExpensesBlock">
            <xsl:call-template name="expenselists"/>
          </div>
        </xsl:if>

      </xsl:if>
<!-- ** ** ** end of notes ** ** ** -->
      </body>
    </html>
  </xsl:template>

  <xsl:template name="limitmodifiersphys">
    <xsl:for-each select="limitmodifiersphys/limitmodifier">
      <xsl:sort select="name"/>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:value-of select="name"/>
          <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
        </td>
      </tr>
      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="limitmodifiersment">
    <xsl:for-each select="limitmodifiersment/limitmodifier">
      <xsl:sort select="name"/>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:value-of select="name"/>
          <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
        </td>
      </tr>
      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="limitmodifierssoc">
    <xsl:for-each select="limitmodifierssoc/limitmodifier">
      <xsl:sort select="name"/>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:value-of select="name"/>
          <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
        </td>
      </tr>
      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="limitmodifiersast">
    <xsl:for-each select="limitmodifiersast/limitmodifier">
      <xsl:sort select="name"/>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:value-of select="name"/>
          <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
        </td>
      </tr>
      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="armor">
    <xsl:for-each select="armors/armor">
      <xsl:sort select="name"/>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:value-of select="name"/>
          <xsl:if test="armorname != ''"> ("<xsl:value-of select="armorname"/>") </xsl:if>
        </td>
        <td style="text-align: center">
          <xsl:call-template name="fnx-pad-l">
            <xsl:with-param name="string" select="armor"/>
            <xsl:with-param name="length" select="2"/>
          </xsl:call-template>
        </td>
        <td style="text-align: center">
          <xsl:choose>
            <xsl:when test="equipped = 'True'">**</xsl:when>
            <xsl:otherwise> </xsl:otherwise>
          </xsl:choose>
        </td>
        <td/>
        <td style="text-align: center">
          <xsl:value-of select="source"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="page"/>
        </td>
      </tr>
      <xsl:if test="armormods/armormod">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="indent">
            <xsl:for-each select="armormods/armormod">
              <xsl:sort select="name"/>
              <xsl:value-of select="name"/>
              <xsl:if test="rating != 0">
                <xsl:text> </xsl:text>
                <xsl:value-of select="rating"/>
              </xsl:if>
              <xsl:if test="last() &gt; 1">; </xsl:if>
            </xsl:for-each>
          </td>
        </tr>
      </xsl:if>

      <xsl:if test="gears/gear">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="indent">
            <xsl:for-each select="gears/gear">
              <xsl:sort select="name"/>
              <xsl:value-of select="name"/>
              <xsl:if test="rating != 0">
                <xsl:text> </xsl:text>
                <xsl:value-of select="$lang.Rating"/>
                <xsl:text> </xsl:text>
                <xsl:value-of select="rating"/>
              </xsl:if>
              <xsl:if test="children/gear">
                <xsl:text> </xsl:text>
                <xsl:value-of select="$lang.with"/>
                <xsl:text> </xsl:text>
                <xsl:call-template name="gearplugin">
                  <xsl:with-param name="gear" select="."/>
                </xsl:call-template>
              </xsl:if>
              <xsl:if test="last() &gt; 1">; </xsl:if>
            </xsl:for-each>
          </td>
        </tr>
      </xsl:if>

      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
      <xsl:call-template name="Xline">
        <xsl:with-param name="cntl" select="last()-position()"/>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="meleeweapons">
      <xsl:param name="weapon"/>
    <tr>
      <xsl:if test="position() mod 2 != 1">
        <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
      </xsl:if>
      <td>
        <xsl:value-of select="name"/>
        <xsl:if test="weaponname != ''"> ("<xsl:value-of select="weaponname"/>") </xsl:if>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="dicepool"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="accuracy"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="damage"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="ap"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="reach"/>
      </td>
      <td/>
      <td style="text-align: center">
        <xsl:value-of select="source"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="page"/>
      </td>
    </tr>
    <xsl:if test="accessories/accessory or mods/weaponmod">
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td colspan="100%" class="indent">
          <xsl:for-each select="accessories/accessory">
            <xsl:sort select="name"/>
            <xsl:value-of select="name"/>
            <xsl:if test="last() &gt; 1">; </xsl:if>
          </xsl:for-each>
          <xsl:for-each select="mods/weaponmod">
            <xsl:sort select="name"/>
            <xsl:value-of select="name"/>
            <xsl:if test="rating > 0">
              <xsl:text> </xsl:text>
              <xsl:value-of select="Rating"/>
              <xsl:text> </xsl:text>
              <xsl:value-of select="rating"/>
            </xsl:if>
            <xsl:if test="last() &gt; 1">; </xsl:if>
          </xsl:for-each>
        </td>
      </tr>
    </xsl:if>

    <xsl:if test="notes != '' and $ProduceNotes">
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td colspan="100%" class="notesrow2">
          <xsl:call-template name="PreserveLineBreaks">
            <xsl:with-param name="text" select="notes"/>
          </xsl:call-template>
        </td>
      </tr>
    </xsl:if>
    <xsl:call-template name="Xline">
      <xsl:with-param name="cntl" select="last()-position()"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="cyberware">
    <xsl:for-each select="cyberwares/cyberware">
      <xsl:sort select="name"/>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td style="text-align: left">
          <xsl:value-of select="name"/>
          <xsl:if test="rating != 0">
            <xsl:text> </xsl:text>
            <xsl:value-of select="Rating"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="rating"/>
          </xsl:if>
          <xsl:if test="location != ''"> (<xsl:value-of select="location"/>)</xsl:if>
        </td>
        <td style="text-align: center"><xsl:value-of select="ess"/></td>
        <td style="text-align: center"><xsl:value-of select="grade"/></td>
        <td style="text-align: center">
          <xsl:value-of select="source"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="page"/>
        </td>
      </tr>
      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
      <xsl:if test="children/cyberware">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="indent">
            <xsl:for-each select="children/cyberware">
              <xsl:value-of select="name"/>
              <xsl:if test="rating != 0">
                <xsl:text> </xsl:text>
                <xsl:value-of select="rating"/>
              </xsl:if>
              <xsl:if test="last() &gt; 1">; </xsl:if>
            </xsl:for-each>
          </td>
        </tr>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="commlink">
    <xsl:for-each select="gears/gear[iscommlink = 'True']">
      <xsl:call-template name="devicedetails"/>
    </xsl:for-each>
    <xsl:for-each select="armors/armor/gears/gear[iscommlink = 'True']">
      <xsl:call-template name="devicedetails"/>
    </xsl:for-each>
    <xsl:for-each select="cyberwares/cyberware/gears/gear[iscommlink = 'True']">
      <xsl:call-template name="devicedetails"/>
    </xsl:for-each>
    <xsl:for-each select="cyberwares/cyberware/children/cyberware/gears/gear[iscommlink = 'True']">
      <xsl:call-template name="devicedetails"/>
    </xsl:for-each>
    <xsl:for-each select="weapons/weapon/accessories/accessory/gears/gear[iscommlink = 'True']">
      <xsl:call-template name="devicedetails"/>
    </xsl:for-each>
    <xsl:if test="gears/gear[isprogram = 'True']">
      <tr><td colspan="100%">
        <span class="title"><xsl:value-of select="Programs"/></span><br/>
        <xsl:for-each select="gears/gear[isprogram = 'True']">
          <xsl:sort select="name"/>
          <xsl:call-template name="programdetails"/>
        </xsl:for-each>
      </td></tr>
    </xsl:if>
    <xsl:if test="armors/armor/gears/gear[isprogram = 'True']">
      <tr><td colspan="100%">
        <span class="title"><xsl:value-of select="Programs"/></span><br/>
        <xsl:for-each select="armors/armor/gears/gear[isprogram = 'True']">
          <xsl:sort select="name"/>
          <xsl:call-template name="programdetails"/>
        </xsl:for-each>
      </td></tr>
    </xsl:if>
    <xsl:if test="cyberwares/cyberware/gears/gear[isprogram = 'True']">
      <tr><td colspan="100%">
        <span class="title"><xsl:value-of select="Programs"/></span><br/>
        <xsl:for-each select="cyberwares/cyberware/gears/gear[isprogram = 'True']">
          <xsl:sort select="name"/>
          <xsl:call-template name="programdetails"/>
        </xsl:for-each>
      </td></tr>
    </xsl:if>
    <xsl:if test="cyberwares/cyberware/children/cyberwear/gears/gear[isprogram = 'True']">
      <tr><td colspan="100%">
        <span class="title"><xsl:value-of select="Programs"/></span><br/>
        <xsl:for-each select="cyberwares/cyberware/children/cyberwear/gears/gear[isprogram = 'True']">
          <xsl:sort select="name"/>
          <xsl:call-template name="programdetails"/>
        </xsl:for-each>
      </td></tr>
    </xsl:if>
    <xsl:if test="weapons/weapon/accessories/accessory/gears/gear[isprogram = 'True']">
      <tr><td colspan="100%">
        <span class="title"><xsl:value-of select="Programs"/></span><br/>
        <xsl:for-each select="weapons/weapon/accessories/accessory/gears/gear[isprogram = 'True']">
          <xsl:sort select="name"/>
          <xsl:call-template name="programdetails"/>
        </xsl:for-each>
      </td></tr>
    </xsl:if>
  </xsl:template>

  <xsl:template name="devicedetails">
    <tr>
      <xsl:if test="position() mod 2 != 1">
        <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
      </xsl:if>
      <td>
        <xsl:value-of select="name"/>
        <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="category"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="devicerating"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="attack"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="sleaze"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="dataprocessing"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="firewall"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="source"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="page"/>
      </td>
    </tr>
    <xsl:if test="children/gear">
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td colspan="100%" class="indent">
          <strong><xsl:value-of select="Accessories"/></strong>
        </td>
      </tr>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td colspan="100%" class="indent">
          <xsl:call-template name="gearplugin">
            <xsl:with-param name="gear" select="."/>
            <xsl:with-param name="rtg" select="$rtglit"/>
          </xsl:call-template>
        </td>
      </tr>
      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
        <xsl:call-template name="Xline">
          <xsl:with-param name="cntl" select="last()-position()"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template name="programdetails">
    <xsl:value-of select="name"/>
    <xsl:if test="gearname != ''"> "<xsl:value-of select="gearname"/>"</xsl:if>
    <xsl:if test="rating &gt; 0">
      <xsl:text> </xsl:text>
      <xsl:value-of select="rating"/>
    </xsl:if>
    <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
    <xsl:if test="children/gear">
      <xsl:call-template name="gearplugin">
        <xsl:with-param name="gear" select="."/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="tradition">
    <tr>
      <td style="vertical-align: top">
        <xsl:value-of select="tradition/name" />
        <xsl:if test="tradition/spiritform != ''">
          <span style="color: grey; font-size: 6pt; vertical-align: bottom;">
            <xsl:text> </xsl:text>
            <xsl:value-of select="tradition/spiritform" />
          </span>
        </xsl:if>
      </td>
      <td style="vertical-align:top; text-align:center;">
        <xsl:choose>
          <xsl:when test="qualities/quality[name='Adept']">
            <xsl:value-of select="$lang.BOD"/> + <xsl:value-of select="$lang.WIL"/>
            (<xsl:value-of select="attributes/attribute[name_english='BOD']/total + attributes/attribute[name_english='WIL']/total"/>)
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="tradition/drain" />
          </xsl:otherwise>
        </xsl:choose>
      </td>
      <td style="vertical-align:top; text-align:center;">
        <xsl:value-of select="tradition/spiritcombat" />
      </td>
      <td style="vertical-align:top; text-align:center;">
        <xsl:value-of select="tradition/spiritdetection" />
      </td>
      <td style="vertical-align:top; text-align:center;">
        <xsl:value-of select="tradition/spirithealth" />
      </td>
      <td style="vertical-align:top; text-align:center;">
        <xsl:value-of select="tradition/spiritillusion" />
      </td>
      <td style="vertical-align:top; text-align:center;">
        <xsl:value-of select="tradition/spiritmanipulation" />
      </td>
      <td style="vertical-align:top; text-align:center;">
        <xsl:value-of select="tradition/source" />
        <xsl:text> </xsl:text>
        <xsl:value-of select="tradition/page" />
      </td>
    </tr>
  </xsl:template>

  <xsl:template name="aiprograms">
    <xsl:for-each select="aiprograms/aiprogram">
      <xsl:sort select="name"/>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:value-of select="name"/>
          <xsl:if test="gearname != ''"> "<xsl:value-of select="gearname"/>"</xsl:if>
          <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
          <xsl:if test="programoptions/programoption">
            (<xsl:for-each select="programoptions/programoption">
              <xsl:sort select="name"/>
              <xsl:value-of select="name"/>
              <xsl:if test="rating &gt; 0">
                <xsl:text> </xsl:text>
                <xsl:value-of select="rating"/>
              </xsl:if>
              <xsl:if test="last() &gt; 1">; </xsl:if>
            </xsl:for-each>)
          </xsl:if>
        </td>
        <td>
          <xsl:if test="requiresprogram != ''">
            <xsl:value-of select="requiresprogram"/>
          </xsl:if>
        </td>
        <td/>
        <td style="text-align:center;">
          <xsl:value-of select="source"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="page"/>
        </td>
      </tr>
      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="vehicles">
      <xsl:param name="vehicle"/>
      <xsl:param name="VehicleNumber"/>
    <div class="block">
      <xsl:attribute name="id">
        <xsl:value-of select="$VehicleNumber"/>
      </xsl:attribute>
      <table><tr><td/></tr></table>
      <table class="tablestyle">
        <tr>
          <th width="35%" style="text-align: left">
            <xsl:value-of select="$lang.Vehicle"/>
          </th>
          <th width="7%" style="font-size: 95%">
            <xsl:value-of select="$lang.Handling"/>
          </th>
          <th width="6%" style="font-size: 95%">
            <xsl:value-of select="$lang.Accel"/>
          </th>
          <th width="4%" style="font-size: 95%">
            <xsl:value-of select="$lang.Speed"/>
          </th>
          <th width="6%" style="font-size: 95%">
            <xsl:value-of select="$lang.Pilot"/>
          </th>
          <th width="5%" style="font-size: 95%">
            <xsl:value-of select="$lang.VehicleBody"/>
          </th>
          <th width="5%" style="font-size: 95%">
            <xsl:value-of select="$lang.Armor"/>
          </th>
          <th width="7%" style="font-size: 95%">
            <xsl:value-of select="$lang.Sensor"/>
          </th>
          <th width="3%" style="font-size: 95%">
            <xsl:value-of select="$lang.CM"/>
          </th>
          <th width="5%" style="font-size: 95%">
            <xsl:value-of select="$lang.Seats"/>
          </th>
          <th width="7%" style="font-size: 95%">
            <xsl:value-of select="$lang.Device"/>
          </th>
          <th width="10%"/>
        </tr>
        <tr>
          <td>
            <xsl:value-of select="name"/>
            <xsl:if test="vehiclename != ''"> ("<xsl:value-of select="vehiclename"/>") </xsl:if>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="handling"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="accel"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="speed"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="pilot"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="body"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="armor"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="sensor"/>
            <xsl:if test="sensorsignal != ''"> (<xsl:value-of select="sensorsignal"/>)</xsl:if>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="physicalcm"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="seats"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="devicerating"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="source"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="page"/>
          </td>
        </tr>
        <xsl:if test="mods/mod">
          <tr><td colspan="100%" class="indent">
            <xsl:for-each select="mods/mod">
              <xsl:sort select="name"/>
              <xsl:value-of select="name"/>
              <xsl:if test="rating != 0">
                <xsl:text> </xsl:text>
                <xsl:value-of select="$lang.Rating"/>
                <xsl:text> </xsl:text>
                <xsl:value-of select="rating"/>
              </xsl:if>
              <xsl:if test="last() &gt; 1">; </xsl:if>
              <xsl:if test="cyberwares/cyberware">
                <xsl:for-each select="cyberwares/cyberware">
                  <xsl:sort select="name"/>
                  <xsl:value-of select="name"/>
                  <xsl:if test="rating != 0">
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="$lang.Rating"/>
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="rating"/>
                  </xsl:if>
                  <xsl:if test="last() &gt; 1">; </xsl:if>
                </xsl:for-each>
              </xsl:if>
            </xsl:for-each>
          </td></tr>
        </xsl:if>
        <xsl:if test="gears/gear">
          <tr><td colspan="100%" class="indent">
            <xsl:for-each select="gears/gear">
              <xsl:sort select="name"/>
              <xsl:value-of select="name"/>
              <xsl:if test="rating != 0">
                <xsl:text> </xsl:text>
                <xsl:value-of select="$lang.Rating"/>
                <xsl:text> </xsl:text>
                <xsl:value-of select="rating"/>
              </xsl:if>
              <xsl:if test="children/gear">
                <xsl:text> </xsl:text>
                <xsl:value-of select="$lang.with"/>
                <xsl:text> </xsl:text>
                <xsl:call-template name="gearplugin">
                  <xsl:with-param name="gear" select="."/>
                </xsl:call-template>
              </xsl:if>
              <xsl:if test="last() &gt; 1">; </xsl:if>
            </xsl:for-each>
          </td></tr>
        </xsl:if>

        <xsl:if test="mods/mod/weapons/weapon[type = 'Ranged'] or weapons/weapon[type = 'Ranged']">
          <tr><td colspan="100%" style="padding: 0 2mm 1mm">
            <table class="tablestyle">
              <tr>
                <th width="20%" style="text-align: left">
                  <xsl:value-of select="$lang.Weapon"/>
                </th>
                <th width="8%"><xsl:value-of select="$lang.Pool"/></th>
                <th width="11%"><xsl:value-of select="$lang.Accuracy"/></th>
                <th width="16%"><xsl:value-of select="$lang.Damage"/></th>
                <th width="9%"><xsl:value-of select="$lang.AP"/></th>
                <th width="8%"><xsl:value-of select="$lang.Mode"/></th>
                <th width="4%"><xsl:value-of select="$lang.RC"/></th>
                <th width="7%"><xsl:value-of select="$lang.Ammo"/></th>
                <th width="7%">[<xsl:value-of select="$lang.Loaded"/>]</th>
                <th width="10%"/>
              </tr>
              <xsl:for-each select="mods/mod/weapons/weapon[type = 'Ranged']">
                <xsl:sort select="name"/>
                <xsl:call-template name="RangedWeapons">
                  <xsl:with-param name="weapon" select="weapon"/>
                </xsl:call-template>
              </xsl:for-each>
              <xsl:for-each select="weapons/weapon[type = 'Ranged']">
                <xsl:sort select="name"/>
                <xsl:call-template name="RangedWeapons">
                  <xsl:with-param name="weapon" select="weapon"/>
                </xsl:call-template>
              </xsl:for-each>
              <xsl:call-template name="RowSummary">
                <xsl:with-param name="text" select="$lang.RangedWeapons"/>
                <xsl:with-param name="buttons" select="'N'"/>
              </xsl:call-template>
            </table>
          </td></tr>
        </xsl:if>

        <xsl:if test="notes != '' and $ProduceNotes">
          <tr>
            <xsl:if test="position() mod 2 != 1">
              <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
            </xsl:if>
            <td colspan="100%" class="notesrow">
              <xsl:call-template name="PreserveLineBreaks">
                <xsl:with-param name="text" select="notes"/>
              </xsl:call-template>
            </td>
          </tr>
        </xsl:if>
      </table>
    </div>
    <xsl:call-template name="RowSummary">
      <xsl:with-param name="text" select="concat($lang.Vehicle,'/',$lang.Drone)"/>
      <xsl:with-param name="blockname" select="$VehicleNumber"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="powers">
    <xsl:for-each select="powers/power">
      <xsl:sort select="name"/>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:value-of select="name"/>
          <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
        </td>
        <td style="text-align: center">
          <xsl:if test="rating &gt; 0">
            <xsl:text> </xsl:text>
            <xsl:value-of select="rating"/>
          </xsl:if>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="pointsperlevel"/> (<xsl:value-of select="totalpoints"/>) </td>
        <td style="text-align: center">
          <xsl:value-of select="source"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="page"/>
        </td>
      </tr>
      <xsl:if test="enhancements != ''">
        <tr>
          <td colspan="100%" class="indent">
            <xsl:for-each select="enhancements/enhancement">
              <xsl:sort select="name"/>
              <xsl:value-of select="name"/>
              <xsl:text> </xsl:text>
              <xsl:value-of select="source"/>
              <xsl:text> </xsl:text>
              <xsl:value-of select="page"/>
              <xsl:if test="notes != ''">(<xsl:value-of select="notes"/>)</xsl:if>
              <xsl:if test="last() &gt; 1">
                ;
              </xsl:if>
            </xsl:for-each>
          </td>
        </tr>
      </xsl:if>
      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
      <xsl:call-template name="Xline">
        <xsl:with-param name="cntl" select="last()-position()"/>
        <xsl:with-param name="nte" select="notes != '' and $ProduceNotes"/>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="critterpowers">
    <xsl:for-each select="critterpowers/critterpower">
      <xsl:sort select="name"/>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:value-of select="name"/>
          <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="rating"/>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="source"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="page"/>
        </td>
      </tr>
      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
      <xsl:call-template name="Xline">
        <xsl:with-param name="cntl" select="last()-position()"/>
        <xsl:with-param name="nte" select="notes != '' and $ProduceNotes"/>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="complexforms">
    <xsl:for-each select="complexforms/complexform">
      <xsl:sort select="name"/>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:value-of select="name"/>
          <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
          <xsl:if test="programoptions/programoption"> (<xsl:for-each select="programoptions/programoption">
            <xsl:sort select="name"/>
            <xsl:value-of select="name"/>
            <xsl:if test="rating &gt; 0">
              <xsl:text> </xsl:text>
              <xsl:value-of select="rating"/>
            </xsl:if>
            <xsl:if test="last() &gt; 1">; </xsl:if>
          </xsl:for-each>)
          </xsl:if>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="target"/>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="duration"/>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="fv"/>
        </td>
        <td/>
        <td style="text-align: center">
          <xsl:value-of select="source"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="page"/>
        </td>
      </tr>
      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
      <xsl:call-template name="Xline">
        <xsl:with-param name="cntl" select="last()-position()"/>
        <xsl:with-param name="nte" select="notes != '' and $ProduceNotes"/>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="martialarts">
    <xsl:for-each select="martialarts/martialart">
      <xsl:sort select="name"/>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:value-of select="name"/>
          <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="source"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="page"/>
        </td>
      </tr>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td class="indent">
          <xsl:for-each select="martialartadvantages/martialartadvantage">
            <xsl:sort select="."/>
            <xsl:value-of select="name"/><xsl:if test="notes != ''"> - <xsl:value-of select="notes"/></xsl:if>
            <xsl:if test="position() != last()"><br /></xsl:if>
          </xsl:for-each>
        </td>
        <td></td>
      </tr>
      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
      <xsl:call-template name="Xline">
        <xsl:with-param name="cntl" select="last()-position()"/>
        <xsl:with-param name="nte" select="notes != '' and $ProduceNotes"/>
      </xsl:call-template>
    </xsl:for-each>
    <xsl:if test="martialartmaneuvers/martialartmaneuver">
      <tr><td colspan="100%"><strong><xsl:value-of select="$lang.Maneuvers"/></strong></td></tr>
      <tr><td colspan="100%" class="indent">
        <xsl:for-each select="martialartmaneuvers/martialartmaneuver">
          <xsl:sort select="name"/>
          <xsl:value-of select="name"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="source"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="page"/>
          <xsl:if test="last() &gt; 1">; </xsl:if>
        </xsl:for-each>
      </td></tr>
    </xsl:if>
  </xsl:template>

  <xsl:template name="othermugshots">
    <table class="tablestyle" style="border-collapse: none;">
      <tr>
        <td width="33%" style="text-align:center;">
          <table class="tablestyle" style="border-width: 0; empty-cells:show;">
            <xsl:for-each select="othermugshots/mugshot[position() mod 3 = 1]">
              <tr><td style="text-align:center;">
                <img>
                  <xsl:attribute name="src">
                    data:image/png;base64,<xsl:value-of select='stringbase64' />
                  </xsl:attribute>
                </img>
              </td></tr>
            </xsl:for-each>
          </table>
        </td>
        <td width="33%" style="text-align:center;">
          <table class="tablestyle" style="border-width: 0; empty-cells:show;">
            <xsl:if test="count(othermugshots/mugshot[position() mod 3 = 2]) = 0">
              <tr><td/></tr>
            </xsl:if>
            <xsl:for-each select="othermugshots/mugshot[position() mod 3 = 2]">
              <tr><td style="text-align:center;">
                <img>
                  <xsl:attribute name="src">
                    data:image/png;base64,<xsl:value-of select='stringbase64' />
                  </xsl:attribute>
                </img>
              </td></tr>
            </xsl:for-each>
          </table>
        </td>
        <td width="33%" style="text-align:center;">
          <table class="tablestyle" style="border-width: 0; empty-cells:show;">
            <xsl:if test="count(othermugshots/mugshot[position() mod 3 = 0]) = 0">
              <tr><td/></tr>
            </xsl:if>
            <xsl:for-each select="othermugshots/mugshot[position() mod 3 = 0]">
              <tr><td style="text-align:center;">
                <img>
                  <xsl:attribute name="src">
                    data:image/png;base64,<xsl:value-of select='stringbase64' />
                  </xsl:attribute>
                </img>
              </td></tr>
            </xsl:for-each>
          </table>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template name="expenselists">
    <table width="100%" style="border-width: 0; border-collapse: collapse;">
      <tr>
        <td width="50%" class="block" id="KarmaExpensesBlock">
          <table class="tablestyle">
            <tr>
              <th width="35%" style="text-align: left">
                <xsl:value-of select="$lang.Date"/>
              </th>
              <th width="4%"/>
              <th width="11%">
                <xsl:value-of select="$lang.Amount"/>
              </th>
              <th width="4%"/>
              <th width="46%" style="text-align: left">
                <xsl:value-of select="$lang.Reason"/>
              </th>
            </tr>
            <xsl:call-template name="Expenses">
              <xsl:with-param name="type" select="'Karma'" />
              <xsl:with-param name="sfx" select="'&#x20;&#x20;&#x20;&#x20;'" />
            </xsl:call-template>
          </table>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text"
              select="concat($lang.Karma,'&#x20;',$lang.Expenses)"/>
            <xsl:with-param name="blockname" select="'KarmaExpensesBlock'"/>
          </xsl:call-template>
        </td>
        <td width="50%" class="block" id="NuyenExpensesBlock">
          <table class="tablestyle">
            <tr>
              <th width="35%" style="text-align: left">
                <xsl:value-of select="$lang.Date"/>
              </th>
              <th width="4%"/>
              <th width="11%"><xsl:value-of select="$lang.Amount"/></th>
              <th width="4%"/>
              <th width="46%" style="text-align: left">
                <xsl:value-of select="$lang.Reason"/>
              </th>
            </tr>
            <xsl:call-template name="Expenses">
              <xsl:with-param name="type" select="'Nuyen'" />
              <xsl:with-param name="sfx" select="' '" />
            </xsl:call-template>
          </table>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text"
              select="concat($lang.Nuyen,'&#x20;',$lang.Expenses)"/>
            <xsl:with-param name="blockname" select="'NuyenExpensesBlock'"/>
          </xsl:call-template>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template name="gear1">
    <xsl:variable name="halfcut" select="round(count(gears/gear[iscommlink != 'True' or location != '']) div 3)"/>
    <xsl:variable name="sortedcopy">
      <xsl:for-each select="gears/gear[iscommlink != 'True' or location != '']">
        <xsl:sort select="location"/>
        <xsl:sort select="name"/>
        <xsl:if test="position() &lt;= $halfcut">
          <xsl:copy-of select="current()"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:call-template name="geardetail">
      <xsl:with-param name="sortedcopy" select="$sortedcopy"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="gear2">
    <xsl:variable name="halfcut" select="round(count(gears/gear[iscommlink != 'True' or location != '']) div 3)"/>
    <xsl:variable name="sortedcopy">
      <xsl:for-each select="gears/gear[iscommlink != 'True' or location != '']">
        <xsl:sort select="location"/>
        <xsl:sort select="name"/>
        <xsl:if test="position() &gt; $halfcut and position() &lt;= $halfcut * 2">
          <xsl:copy-of select="current()"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:call-template name="geardetail">
      <xsl:with-param name="sortedcopy" select="$sortedcopy"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="gear3">
    <xsl:variable name="halfcut" select="round(count(gears/gear[iscommlink != 'True' or location != '']) div 3)"/>
    <xsl:variable name="sortedcopy">
      <xsl:for-each select="gears/gear[iscommlink != 'True' or location != '']">
        <xsl:sort select="location"/>
        <xsl:sort select="name"/>
        <xsl:if test="position() &gt; $halfcut * 2">
          <xsl:copy-of select="current()"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:call-template name="geardetail">
      <xsl:with-param name="sortedcopy" select="$sortedcopy"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="geardetail">
      <xsl:param name="sortedcopy"/>
    <xsl:for-each select="msxsl:node-set($sortedcopy)/gear">
      <xsl:choose>
        <xsl:when test="location != preceding-sibling::gear[1]/location">
          <tr>
            <td colspan="100%" style="border-bottom:solid black 1px;">
              <strong><xsl:value-of select="location"/></strong>
            </td>
          </tr>
        </xsl:when>
        <xsl:when test="position() = 1">
          <tr>
            <td colspan="100%" style="border-bottom:solid black 1px;">
              <strong><xsl:value-of select="location"/></strong>
            </td>
          </tr>
        </xsl:when>
      </xsl:choose>

      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:value-of select="name"/>
          <xsl:if test="gearname != ''"> "<xsl:value-of select="gearname"/>"</xsl:if>
          <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
        </td>
        <td style="text-align: center">
          <xsl:choose>
            <xsl:when test="rating = 0">-</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="rating"/>
            </xsl:otherwise>
          </xsl:choose>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="qty"/>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="source"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="page"/>
        </td>
      </tr>

      <xsl:if test="children/gear">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="indent">
            <xsl:call-template name="gearplugin">
              <xsl:with-param name="gear" select="."/>
              <xsl:with-param name="rtg" select="$rtglit"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>

      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="gearplugin">
      <xsl:param name="gear"/>
      <xsl:param name="rtg" select="''"/>
      <xsl:param name="xtra" select="'A'"/>
    <xsl:for-each select="children/gear">
      <xsl:sort select="name"/>
      <xsl:value-of select="name"/>
      <xsl:if test="gearname != ''"> "<xsl:value-of select="gearname"/>"</xsl:if>
      <xsl:if test="extra != '' and $xtra = 'B'">
        (<xsl:value-of select="extra"/>)
      </xsl:if>
      <xsl:if test="rating != 0">
        <xsl:value-of select="$rtg"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="rating"/>
      </xsl:if>
      <xsl:if test="extra != '' and $xtra = 'A'">
        (<xsl:value-of select="extra"/>)
      </xsl:if>
      <xsl:if test="qty &gt; 1"> x<xsl:value-of select="qty"/></xsl:if>
      <xsl:choose>
        <xsl:when test="children/gear">
          <xsl:text> </xsl:text>
          <xsl:value-of select="$lang.with"/>
          <xsl:text> </xsl:text>
          <xsl:call-template name="gearplugin">
            <xsl:with-param name="gear" select="."/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="position() != last()">, </xsl:when>
        <xsl:otherwise>; </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>
