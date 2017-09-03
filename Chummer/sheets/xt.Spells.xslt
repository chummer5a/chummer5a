<?xml version="1.0" encoding="UTF-8" ?>
<!-- Format Spells list of Character Sheet -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:template name="Spells">
    <style type="text/css">
      h5 {
        position: relative;
        text-align: center;
      }
      h5 span {
        background: #fff;
        padding: 0 5px;
        position: relative;
        z-index: 1;
      }
      h5:before {
        background: linear-gradient(to right, lightgrey, black, lightgrey);
        content: "";
        display: block;
        height: 1px;
        position: absolute;
        top: 50%;
        width: 100%;
      }
      th {
        text-align: center;
      }
    </style>
    <style media="print">
      h5 {
        text-align: center;
        text-decoration: underline;
      }
    </style>

    <tr>
      <th width="23%" style="text-align: left">
        <xsl:value-of select="$lang.Spell"/>
      </th>
      <th width="6%"><xsl:value-of select="$lang.Type"/></th>
      <th width="20%"><xsl:value-of select="$lang.Range"/></th>
      <th width="12%"><xsl:value-of select="$lang.Damage"/></th>
      <th width="15%"><xsl:value-of select="$lang.Duration"/></th>
      <th width="5%"><xsl:value-of select="$lang.Drain"/></th>
      <th width="9%"><xsl:value-of select="$lang.DV"/></th>
      <th width="10%"/>
    </tr>
    <xsl:variable name="sortedspells">
      <xsl:for-each select="spells/spell">
        <xsl:sort select="category"/>
        <xsl:sort select="name"/>
        <xsl:copy-of select="current()"/>
      </xsl:for-each>
    </xsl:variable>
    <xsl:for-each select="msxsl:node-set($sortedspells)/spell">
      <xsl:if test="position() = 1 or category != preceding-sibling::spell[1]/category">
        <xsl:variable name="cat">
          <xsl:choose>
            <xsl:when test="category_english = 'Combat'">
              <xsl:value-of select="$lang.CombatSpells"/>
            </xsl:when>
            <xsl:when test="category_english = 'Detection'">
              <xsl:value-of select="$lang.DetectionSpells"/>
            </xsl:when>
            <xsl:when test="category_english = 'Health'">
              <xsl:value-of select="$lang.HealthSpells"/>
            </xsl:when>
            <xsl:when test="category_english = 'Illusion'">
              <xsl:value-of select="$lang.IllusionSpells"/>
            </xsl:when>
            <xsl:when test="category_english = 'Manipulation'">
              <xsl:value-of select="$lang.ManipulationSpells"/>
            </xsl:when>
            <xsl:when test="category_english = 'Enchantments'">
              <xsl:value-of select="$lang.Enchantments"/>
            </xsl:when>
            <xsl:when test="category_english = 'Ritual'">
              <xsl:value-of select="$lang.Rituals"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$lang.Unknown"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="Xline"/>
        <tr><td colspan="100%">
          <h5><span><xsl:value-of select="$cat"/></span></h5>
        </td></tr>
      </xsl:if>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td valign="top">
          <xsl:value-of select="name"/>
          <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
        </td>
        <td style="text-align: center" valign="top">
          <xsl:value-of select="type"/>
        </td>
        <td style="text-align: center" valign="top">
          <xsl:choose>
            <xsl:when test="range = $lang.tstRange1">
              <xsl:value-of select="$lang.Touch"/>
            </xsl:when>
            <xsl:when test="range = $lang.tstRange2">
              <xsl:value-of select="$lang.LineofSight"/>
            </xsl:when>
            <xsl:when test="range = $lang.tstRange3 or range = $lang.tstRange4">
              <xsl:value-of select="$lang.LineofSight"/> (<xsl:value-of select="$lang.Area"/>)
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="range"/>
            </xsl:otherwise>
          </xsl:choose>
        </td>
        <td style="text-align: center" valign="top">
          <xsl:choose>
            <xsl:when test="damage = $lang.tstDamage1">
              <xsl:value-of select="$lang.Physical"/>
            </xsl:when>
            <xsl:when test="damage = $lang.tstDamage2">
              <xsl:value-of select="$lang.Stun"/>
            </xsl:when>
            <xsl:when test="damage = '0'">-</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="damage"/>
            </xsl:otherwise>
          </xsl:choose>
        </td>
        <td style="text-align: center" valign="top">
          <xsl:choose>
            <xsl:when test="duration = $lang.tstDuration1">
              <xsl:value-of select="$lang.Instantaneous"/>
            </xsl:when>
            <xsl:when test="duration = $lang.tstDuration2">
              <xsl:value-of select="$lang.Permanent"/>
            </xsl:when>
            <xsl:when test="duration = $lang.tstDuration3">
              <xsl:value-of select="$lang.Sustained"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="duration"/>
            </xsl:otherwise>
          </xsl:choose>
        </td>
        <td style="text-align: center" valign="top">
          <xsl:value-of select="dv"/>
        </td>
        <td style="text-align: center" valign="top">
          <xsl:choose>
            <xsl:when test="category_english='Rituals' or category_english='Enchantments'">
              &#x20;
            </xsl:when>
            <xsl:when test="contains(descriptors,',')">
              <xsl:value-of select="substring-before(descriptors,',')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="descriptors"/>
            </xsl:otherwise>
          </xsl:choose>
        </td>
        <td style="text-align: center; font-size: 95%" valign="top">
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
          <td colspan="100%" style="padding: 0 2%; text-align: justify;">
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
</xsl:stylesheet>