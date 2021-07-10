<?xml version="1.0" encoding="utf-8" ?>
<!-- Character notes -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="xs.Chummer5CSS.xslt" />
  <xsl:include href="xs.fnx.xslt" />
  <xsl:include href="xs.fnxTests.xslt" />
  <xsl:include href="xs.TitleName.xslt" />

  <xsl:include href="xt.ComplexForms.xslt" />
  <xsl:include href="xt.CritterPowers.xslt" />
  <xsl:include href="xt.Lifestyles.xslt" />
  <xsl:include href="xt.MartialArts.xslt" />
  <xsl:include href="xt.MetamagicArts.xslt" />
  <xsl:include href="xt.Metamagics.xslt" />
  <xsl:include href="xt.Notes.xslt" />
  <xsl:include href="xt.Nothing2Show.xslt" />
  <xsl:include href="xt.PreserveHtml.xslt" />
  <xsl:include href="xt.PreserveLineBreaks.xslt" />
  <xsl:include href="xt.Qualities.xslt" />
  <xsl:include href="xt.RowSummary.xslt" />
  <xsl:include href="xt.RuleLine.xslt" />
  <xsl:include href="xt.Spells.xslt" />
  <xsl:include href="xt.TableTitle.xslt" />

  <xsl:template match="/characters/character">
    <xsl:variable name="TitleName">
      <xsl:call-template name="TitleName">
        <xsl:with-param name="name" select="name" />
        <xsl:with-param name="alias" select="alias" />
      </xsl:call-template>
    </xsl:variable>
    <title><xsl:value-of select="$TitleName" /></title>

    <xsl:text disable-output-escaping="yes"><![CDATA[<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">]]></xsl:text>
    <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
      <head>
        <meta http-equiv="x-ua-compatible" content="IE=Edge" />
        <meta charset="UTF-8" />
        <xsl:call-template name="Chummer5CSS" />
      </head>

      <body>
        <xsl:if test="qualities/quality/notes != ''">
          <div id="QualitiesBlock">
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name" select="$lang.Qualities" />
            </xsl:call-template>
            <table class="tablestyle">
              <tr><th width="80%" /><th width="10%" /><th width="10%" /></tr>
              <xsl:call-template name="Qualities" />
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Qualities" />
            <xsl:with-param name="blockname" select="'QualitiesBlock'" />
          </xsl:call-template>
        </xsl:if>

        <xsl:variable name="powerslist" select="powers/power[notes != ''] or powers/power/enhancements/enhancement[notes != '']" />
        <xsl:if test="$powerslist">
          <div id="PowersBlock">
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name" select="$lang.Power" />
            </xsl:call-template>
            <table class="tablestyle">
              <tr><th width="80%" />
			  <th width="10%" />
			  <th width="10%" /></tr>
              <xsl:call-template name="Qualities" />
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.AdeptPowers" />
            <xsl:with-param name="blockname" select="'PowersBlock'" />
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="lifestyles/lifestyle/notes != ''">
          <div id="LifestyleBlock">
            <table><tr><td /></tr></table>
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name" select="$lang.Lifestyle" />
            </xsl:call-template>
            <table class="tablestyle">
              <xsl:call-template name="Lifestyles" />
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Lifestyle" />
            <xsl:with-param name="blockname" select="'LifestyleBlock'" />
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="magenabled = 'True'">
          <xsl:if test="initiationgrades/initiationgrade/notes != ''">
            <div id="InitiationBlock">
              <table><tr><td /></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.InitiationNotes" />
              </xsl:call-template>
              <table class="tablestyle">
                <xsl:call-template name="gradenotes" />
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.Initiation" />
              <xsl:with-param name="blockname" select="'InitiationBlock'" />
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="arts/art/notes != ''">
            <div id="MetamagicArtsBlock">
              <table><tr><td /></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.Arts" />
              </xsl:call-template>
              <table class="tablestyle">
                <tr><th width="80%" /><th width="10%" /><th width="10%" /></tr>
                <xsl:call-template name="MetamagicArts" />
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.Arts" />
              <xsl:with-param name="blockname" select="'MetamagicArtsBlock'" />
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="metamagics/metamagic/notes != ''">
            <div id="MetamagicsBlock">
              <table><tr><td /></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.Metamagics" />
              </xsl:call-template>
              <table class="tablestyle">
                <tr><th width="80%" /><th width="10%" /><th width="10%" /></tr>
                <xsl:call-template name="Metamagics" />
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.Metamagics" />
              <xsl:with-param name="blockname" select="'MetamagicsBlock'" />
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="spells/spell/notes != ''">
            <div id="SpellsBlock">
              <table><tr><td /></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.Spells" />
              </xsl:call-template>
              <table class="tablestyle">
                <xsl:call-template name="Spells" />
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.Spells" />
              <xsl:with-param name="blockname" select="'SpellsBlock'" />
            </xsl:call-template>
          </xsl:if>
        </xsl:if>

        <xsl:if test="resenabled = 'True'">
          <xsl:if test="initiationgrades/initiationgrade/notes != ''">
            <div id="SubmersionBlock">
              <table><tr><td /></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.SubmersionNotes" />
              </xsl:call-template>
              <table class="tablestyle">
                <xsl:call-template name="gradenotes" />
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.Submersion" />
              <xsl:with-param name="blockname" select="'SubmersionBlock'" />
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="metamagics/metamagic/notes != ''">
            <div id="EchoesBlock">
              <table><tr><td /></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.Echoes" />
              </xsl:call-template>
              <table class="tablestyle">
                <xsl:call-template name="Metamagics" />
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.Echoes" />
              <xsl:with-param name="blockname" select="'EchoesBlock'" />
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="complexforms/complexform/notes != ''">
            <div id="ComplexFormsBlock">
              <table><tr><td /></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.ComplexForms" />
              </xsl:call-template>
              <table class="tablestyle">
                <xsl:call-template name="ComplexForms" />
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.ComplexForms" />
              <xsl:with-param name="blockname" select="'ComplexFormsBlock'" />
            </xsl:call-template>
          </xsl:if>
        </xsl:if>

        <xsl:variable name="martialartslist" select="martialarts/martialart[notes != ''] or martialarts/martialart/martialarttechniques/martialarttechnique[notes != '']" />
        <xsl:if test="$martialartslist">
          <div class="block" id="MartialArtsBlock">
            <table><tr><td /></tr></table>
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name" select="$lang.MartialArts" />
            </xsl:call-template>
            <table class="tablestyle">
              <xsl:call-template name="MartialArtsList" />
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.MartialArts" />
            <xsl:with-param name="blockname" select="'MartialArtsBlock'" />
          </xsl:call-template>
        </xsl:if>

        <xsl:variable name="cyberlist" select="cyberwares/cyberware[notes != '' or //*[iscommlink != 'True']/notes != '']" />
        <xsl:if test="$cyberlist">
          <div class="block" id="CyberwareBlock">
            <table><tr><td /></tr></table>
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name"
                    select="concat($lang.Cyberware,'/',$lang.Bioware)" />
            </xsl:call-template>
            <table class="tablestyle">
              <tr>
                <th width="90%" style="text-align: left">
                  <xsl:value-of select="$lang.Implant" />
                </th>
                <th width="10%" />
              </tr>
              <xsl:for-each select="cyberwares/cyberware[notes != '' or //*[iscommlink != 'True']/notes != '']">
                <xsl:sort select="name" />
                <xsl:call-template name="cybernotes">
                  <xsl:with-param name="level" select="0" />
                </xsl:call-template>
              </xsl:for-each>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="concat($lang.Cyberware,'/',$lang.Bioware)" />
            <xsl:with-param name="blockname" select="'CyberwareBlock'" />
          </xsl:call-template>
        </xsl:if>

        <xsl:variable name="armorlist" select="armors/armor[notes != '' or //*[iscommlink != 'True']/notes != '']" />
        <xsl:if test="$armorlist">
          <div class="block" id="ArmorBlock">
            <table><tr><td /></tr></table>
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name" select="$lang.Armor" />
            </xsl:call-template>
            <table class="tablestyle">
              <tr>
                <th width="90%" style="text-align: left">
                  <xsl:value-of select="$lang.Armor" />
                </th>
                <th width="10%" />
              </tr>
              <xsl:for-each select="armors/armor[notes != '' or //*[iscommlink != 'True']/notes != '']">
                <xsl:sort select="name" />
                <xsl:call-template name="armornotes" />
              </xsl:for-each>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Armor" />
            <xsl:with-param name="blockname" select="'ArmorBlock'" />
          </xsl:call-template>
        </xsl:if>

        <xsl:variable name="rangedlist" select="weapons/weapon[type = 'Ranged' and (notes != '' or //*[iscommlink != 'True']/notes != '')]" />
        <xsl:if test="$rangedlist">
          <div class="block" id="RangedWeaponsBlock">
            <table><tr><td /></tr></table>
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name" select="$lang.RangedWeapons" />
            </xsl:call-template>
            <table class="tablestyle">
              <tr>
                <th width="90%" style="text-align: left">
                  <xsl:value-of select="$lang.Weapon" />
                </th>
                <th width="10%" />
              </tr>
              <xsl:for-each select="weapons/weapon[type = 'Ranged' and (notes != '' or //*[iscommlink != 'True']/notes != '')]">
                <xsl:sort select="fullname" />
                <xsl:call-template name="weaponnotes">
                  <xsl:with-param name="level" select="0" />
                </xsl:call-template>
              </xsl:for-each>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.RangedWeapons" />
            <xsl:with-param name="blockname" select="'RangedWeaponsBlock'" />
          </xsl:call-template>
        </xsl:if>

        <xsl:variable name="meleelist" select="weapons/weapon[type = 'Melee' and (notes != '' or //*[iscommlink != 'True']/notes != '')]" />
        <xsl:if test="$meleelist">
          <div class="block" id="MeleeWeaponsBlock">
            <table><tr><td /></tr></table>
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name" select="$lang.MeleeWeapons" />
            </xsl:call-template>
            <table class="tablestyle">
              <tr>
                <th width="90%" style="text-align: left">
                  <xsl:value-of select="$lang.Weapon" />
                </th>
                <th width="10%" />
              </tr>
              <xsl:for-each select="weapons/weapon[type = 'Melee' and (notes != '' or //*[iscommlink != 'True']/notes != '')]">
                <xsl:sort select="fullname" />
                <xsl:call-template name="weaponnotes">
                  <xsl:with-param name="level" select="0" />
                </xsl:call-template>
              </xsl:for-each>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.MeleeWeapons" />
            <xsl:with-param name="blockname" select="'MeleeWeaponsBlock'" />
          </xsl:call-template>
        </xsl:if>

        <xsl:variable name="gearlist" select="gears/gear[iscommlink != 'True' and (notes != '' or //*[iscommlink != 'True']/notes != '')]" />
        <xsl:if test="$gearlist">
          <div class="block" id="GearBlock">
            <table><tr><td /></tr></table>
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name" select="$lang.Gear" />
            </xsl:call-template>
            <table class="tablestyle">
              <tr>
                <th width="90%" style="text-align: left">
                  <xsl:value-of select="$lang.Gear" />
                </th>
                <th width="10%" />
              </tr>
              <xsl:for-each select="gears/gear[iscommlink != 'True' and (notes != '' or //*[iscommlink != 'True']/notes != '')]">
<!--
                <xsl:sort select="location" />
-->
                <xsl:sort select="name" />
                <xsl:call-template name="gearnotes">
                  <xsl:with-param name="excludeCommlinks" select="True" />
                  <xsl:with-param name="level" select="0" />
                </xsl:call-template>
              </xsl:for-each>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Gear" />
            <xsl:with-param name="blockname" select="'GearBlock'" />
          </xsl:call-template>
        </xsl:if>

        <xsl:variable name="devicelist" select="//gears/gear[iscommlink = 'True' and (notes != '' or //notes != '')]" />
        <xsl:if test="$devicelist">
          <div class="block" id="DevicesBlock">
            <table><tr><td /></tr></table>
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name" select="concat($lang.Devices,'/',$lang.Programs)" />
            </xsl:call-template>
            <table class="tablestyle">
              <tr>
                <th width="90%" style="text-align: left">
                  <xsl:value-of select="$lang.Device" />
                </th>
                <th width="10%" />
              </tr>
              <xsl:for-each select="//gears/gear[(iscommlink = 'True' and (notes != '' or //notes != '')) or //*[iscommlink = 'True']/notes != '']">
                <xsl:sort select="name" />
                <xsl:call-template name="gearnotes">
                  <xsl:with-param name="excludeCommlinks" select="False" />
                  <xsl:with-param name="level" select="0" />
                </xsl:call-template>
              </xsl:for-each>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="concat($lang.Devices,'/',$lang.Programs)" />
            <xsl:with-param name="blockname" select="'DevicesBlock'" />
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="critterpowers/critterpower">
          <div id="CritterBlock">
            <table><tr><td /></tr></table>
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name" select="$lang.CritterPowers" />
            </xsl:call-template>
            <table class="tablestyle">
              <xsl:call-template name="CritterPowers" />
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.CritterPowers" />
            <xsl:with-param name="blockname" select="'CritterBlock'" />
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="concat(concept,description,background,notes,gamenotes) !=''">
          <xsl:call-template name="notes" />
        </xsl:if>

        <xsl:choose>
          <xsl:when test="qualities/quality/notes != ''" />
          <xsl:when test="$powerslist" />
          <xsl:when test="initiationgrades/initiationgrade/notes != ''" />
          <xsl:when test="metamagics/metamagic/notes != ''" />
          <xsl:when test="spells/spell/notes != ''" />
          <xsl:when test="complexforms/complexform/notes != ''" />
          <xsl:when test="lifestyles/lifestyle/notes != ''" />
          <xsl:when test="$martialartslist" />
          <xsl:when test="$cyberlist" />
          <xsl:when test="$armorlist" />
          <xsl:when test="$rangedlist" />
          <xsl:when test="$meleelist" />
          <xsl:when test="$gearlist" />
          <xsl:when test="$devicelist" />
          <xsl:when test="critterpowers/critterpower != ''" />
          <xsl:when test="concat(concept,description,background,notes,gamenotes) !=''" />
          <xsl:otherwise>
            <xsl:call-template name="nothing2show">
              <xsl:with-param name="namethesheet" select="$lang.Nothing2Show4Notes" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </body>
    </html>
  </xsl:template>

  <xsl:template name="gradenotes">
    <xsl:for-each select="initiationgrades/initiationgrade[notes != '']">
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td colspan="100%" class="notesrow">
          <u><xsl:value-of select="$lang.Grade" />
          <xsl:text> </xsl:text>
          <xsl:value-of select="grade" /></u>
          <xsl:text> </xsl:text>
          <xsl:call-template name="PreserveLineBreaks">
             <xsl:with-param name="text" select="notes" />
          </xsl:call-template>
        </td>
      </tr>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="cybernotes">
    <xsl:param name="level" />
    <tr style="text-align: left" valign="top">
      <xsl:if test="position() mod 2 != 1">
        <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
      </xsl:if>
      <td style="padding: 0 {$level * 2}%;">
	    <xsl:value-of select="name" />
        <xsl:if test="location != ''"> (<xsl:value-of select="location" />)</xsl:if>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="source" />
        <xsl:text> </xsl:text>
        <xsl:value-of select="page" />
      </td>
    </tr>
    <xsl:if test="notes != ''">
      <tr>
        <xsl:if test="position() mod 2 != 1"><xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute></xsl:if>
        <td colspan="100" style="padding: 0 {($level + 1) * 2}%; text-align: justify;">
          <xsl:call-template name="PreserveLineBreaks">
            <xsl:with-param name="text" select="notes" />
          </xsl:call-template>
        </td>
      </tr>
    </xsl:if>
    <xsl:for-each select="children/cyberware[notes != '' or //*[iscommlink != 'True']/notes != '']">
      <xsl:sort select="name" />
      <xsl:call-template name="cybernotes">
        <xsl:with-param name="level" select="$level + 1"></xsl:with-param>
      </xsl:call-template>
    </xsl:for-each>
    <xsl:for-each select="gears/gear[iscommlink != 'True' and (notes != '' or //*[iscommlink != 'True']/notes != '')]">
      <xsl:sort select="name" />
      <xsl:call-template name="gearnotes">
        <xsl:with-param name="excludeCommlinks" select="True" />
        <xsl:with-param name="level" select="$level + 1" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:call-template name="Xline">
      <xsl:with-param name="cntl" select="last()-position()" />
      <xsl:with-param name="nte" select="notes != '' and $ProduceNotes" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="armornotes">
    <tr style="text-align: left" valign="top">
      <xsl:if test="position() mod 2 != 1"><xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute></xsl:if>
      <td>
	    <xsl:value-of select="name" />
        <xsl:if test="armorname != ''"> ("<xsl:value-of select="armorname" />") </xsl:if>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="source" />
        <xsl:text> </xsl:text>
        <xsl:value-of select="page" />
      </td>
    </tr>
    <xsl:if test="notes != ''">
      <tr>
        <xsl:if test="position() mod 2 != 1"><xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute></xsl:if>
        <td colspan="100" style="padding: 0 2%; text-align: justify;">
          <xsl:call-template name="PreserveLineBreaks">
            <xsl:with-param name="text" select="notes" />
          </xsl:call-template>
        </td>
      </tr>
    </xsl:if>
    <xsl:for-each select="armormods/armormod[notes != '' or //*[iscommlink != 'True']/notes != '']">
      <xsl:sort select="name" />
      <tr>
        <xsl:if test="position() mod 2 != 1"><xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute></xsl:if>
        <td style="padding: 0 2%;"><xsl:value-of select="name" /></td>
        <td style="text-align: center">
          <xsl:value-of select="source" />
          <xsl:text> </xsl:text>
          <xsl:value-of select="page" />
        </td>
        <td />
      </tr>
      <xsl:if test="notes != ''">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100" style="padding: 0 4%; text-align: justify;">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes" />
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
      <xsl:for-each select="gears/gear[iscommlink != 'True' and (notes != '' or //*[iscommlink != 'True']/notes != '')]">
        <xsl:sort select="name" />
        <xsl:call-template name="gearnotes">
          <xsl:with-param name="excludeCommlinks" select="True" />
          <xsl:with-param name="level" select="2" />
        </xsl:call-template>
      </xsl:for-each>
    </xsl:for-each>
    <xsl:for-each select="gears/gear[iscommlink != 'True' and (notes != '' or //*[iscommlink != 'True']/notes != '')]">
      <xsl:sort select="name" />
      <xsl:call-template name="gearnotes">
        <xsl:with-param name="excludeCommlinks" select="True" />
        <xsl:with-param name="level" select="1" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:call-template name="Xline">
      <xsl:with-param name="cntl" select="last()-position()" />
      <xsl:with-param name="nte" select="notes != '' and $ProduceNotes" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="weaponnotes">
      <xsl:param name="level" />
    <tr style="text-align: left" valign="top">
      <xsl:if test="position() mod 2 != 1"><xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute></xsl:if>
      <td style="padding: 0 {$level * 2}%;">
        <xsl:value-of select="name" />
        <xsl:if test="weaponname != ''"> ("<xsl:value-of select="weaponname" />") </xsl:if>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="source" />
        <xsl:text> </xsl:text>
        <xsl:value-of select="page" />
      </td>
    </tr>
    <xsl:if test="notes != ''">
      <tr>
        <xsl:if test="position() mod 2 != 1"><xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute></xsl:if>
        <td colspan="100" style="padding: 0 {($level + 1) * 2}%; text-align: justify;">
          <xsl:call-template name="PreserveLineBreaks">
            <xsl:with-param name="text" select="notes" />
          </xsl:call-template>
        </td>
      </tr>
    </xsl:if>
    <xsl:for-each select="accessories/accessory[notes != '' or //notes != '']">
      <xsl:sort select="name" />
      <tr>
        <xsl:if test="position() mod 2 != 1"><xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute></xsl:if>
        <td style="padding: 0 {($level + 1) * 2}%;"><xsl:value-of select="name" /></td>
        <td style="text-align: center">
          <xsl:value-of select="source" />
          <xsl:text> </xsl:text>
          <xsl:value-of select="page" />
        </td>
      </tr>
      <xsl:if test="notes != ''">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100" style="padding: 0 {($level + 2) * 2}%; text-align: justify;">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes" />
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
      <xsl:for-each select="gears/gear[iscommlink != 'True' and (notes != '' or //*[iscommlink != 'True']/notes != '')]">
        <xsl:sort select="name" />
        <xsl:call-template name="gearnotes">
          <xsl:with-param name="excludeCommlinks" select="True" />
          <xsl:with-param name="level" select="$level + 2" />
        </xsl:call-template>
      </xsl:for-each>
    </xsl:for-each>
    <xsl:for-each select="underbarrel[notes != '' or //*[iscommlink != 'True']/notes != '']">
      <xsl:sort select="fullname" />
      <xsl:call-template name="gearnotes">
        <xsl:with-param name="excludeCommlinks" select="True" />
        <xsl:with-param name="level" select="$level + 1" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:call-template name="Xline">
      <xsl:with-param name="cntl" select="last()-position()" />
      <xsl:with-param name="nte" select="notes != '' and $ProduceNotes" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="gearnotes">
    <xsl:param name="level" />
    <xsl:param name="excludeCommlinks" />
<!--
    <xsl:choose>
      <xsl:when test="location = ''" />
      <xsl:when test="position() = 1 or location != preceding-sibling::gear[1]/location">
        <tr>
          <td colspan="100%" style="border-bottom:solid black 0.1em;">
            <strong><xsl:value-of select="location" /></strong>
          </td>
        </tr>
      </xsl:when>
    </xsl:choose>
-->
    <tr style="text-align: left" valign="top">
      <xsl:if test="position() mod 2 != 1"><xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute></xsl:if>
      <td style="padding: 0 {$level * 2}%;">
	    <xsl:value-of select="name" />
        <xsl:if test="gearname != ''"> ("<xsl:value-of select="gearname" />") </xsl:if>
        <xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="source" />
        <xsl:text> </xsl:text>
        <xsl:value-of select="page" />
      </td>
    </tr>
    <xsl:if test="notes != ''">
      <tr>
        <xsl:if test="position() mod 2 != 1"><xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute></xsl:if>
        <td colspan="100" style="padding: 0 {($level + 1) * 2}%; text-align: justify;">
          <xsl:call-template name="PreserveLineBreaks">
            <xsl:with-param name="text" select="notes" />
          </xsl:call-template>
        </td>
      </tr>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="$excludeCommlinks = 'True'">
        <xsl:for-each select="children/gear[notes != '' or //*[iscommlink != 'True']/notes != '']">
          <xsl:sort select="name" />
          <xsl:call-template name="gearnotes">
            <xsl:with-param name="level" select="$level + 1" />
          </xsl:call-template>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:for-each select="children/gear[notes != '' or //*[iscommlink = 'True']/notes != '']">
          <xsl:sort select="name" />
          <xsl:call-template name="gearnotes">
            <xsl:with-param name="level" select="$level + 1" />
          </xsl:call-template>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:call-template name="Xline">
      <xsl:with-param name="cntl" select="last()-position()" />
      <xsl:with-param name="nte" select="notes != '' and $ProduceNotes" />
    </xsl:call-template>
  </xsl:template>

<!-- remove remove remove remove remove
  <xsl:template name="mainnotes">
      <xsl:if test="position() mod 2 != 1"><xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute></xsl:if>
      <td colspan="100" style="padding: 0 2; text-align: justify;">
        <xsl:call-template name="PreserveLineBreaks">
          <xsl:with-param name="text" select="notes" />
        </xsl:call-template>
      </td>
  </xsl:template>

  <xsl:template name="subnotes">
    <tr>
      <xsl:if test="position() mod 2 != 1"><xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute></xsl:if>
      <td colspan="100" style="padding: 0 4; text-align: justify;">
        <xsl:call-template name="PreserveLineBreaks">
          <xsl:with-param name="text" select="notes" />
        </xsl:call-template>
      </td>
    </tr>
  </xsl:template>

  <xsl:template name="producenotes">
  </xsl:template>
-->
</xsl:stylesheet>
