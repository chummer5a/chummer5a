<?xml version="1.0" encoding="UTF-8" ?>
<!-- Format Critter Powers list of Character Sheet -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:template name="CritterPowers">
    <tr>
      <th width="30%" style="text-align: left">
        <xsl:value-of select="$lang.CritterPower"/>
      </th>
      <th width="10%"><xsl:value-of select="$lang.Category"/></th>
      <th width="6%"><xsl:value-of select="$lang.Type"/></th>
      <th width="8%"><xsl:value-of select="$lang.Action"/></th>
      <th width="17%"><xsl:value-of select="$lang.Range"/></th>
      <th width="8%"><xsl:value-of select="$lang.Rating"/></th>
      <th width="11%"><xsl:value-of select="$lang.Duration"/></th>
      <th width="10%"/>
    </tr>

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
          <xsl:value-of select="category"/>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="type"/>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="action"/>
        </td>
        <td style="text-align: center">
          <xsl:call-template name="fnx-range">
            <xsl:with-param name="code" select="range"/>
          </xsl:call-template>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="rating"/>
        </td>
        <td style="text-align: center">
          <xsl:call-template name="fnx-duration">
            <xsl:with-param name="code" select="duration"/>
          </xsl:call-template>
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
</xsl:stylesheet>
