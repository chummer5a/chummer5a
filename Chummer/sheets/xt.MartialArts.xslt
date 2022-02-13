<?xml version="1.0" encoding="UTF-8" ?>
<!-- Format Martial Arts list of Character Sheet -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:template name="MartialArtsList">
    <tr>
      <th width="90%" style="text-align: left">
        <xsl:value-of select="$lang.MartialArt" />
      </th>
      <th width="10%" />
    </tr>
    <xsl:call-template name="martialarts" />
  </xsl:template>

  <xsl:template name="martialarts">
    <xsl:for-each select="martialarts/martialart">
      <xsl:sort select="fullname" />
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td><xsl:value-of select="fullname" /></td>
        <td style="text-align: center">
          <xsl:value-of select="source" />
          <xsl:text> </xsl:text>
          <xsl:value-of select="page" />
        </td>
      </tr>
      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes" />
            </xsl:call-template>
          </td>
        </tr>
        <xsl:if test="martialarttechniques/martialarttechnique">
          <xsl:call-template name="Xline" />
        </xsl:if>
      </xsl:if>
      <xsl:for-each select="martialarttechniques/martialarttechnique">
        <xsl:sort select="name" />
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td class="indent">
            <xsl:value-of select="name" />
          </td>
          <td style="text-align: center">
            <xsl:value-of select="source" />
            <xsl:text> </xsl:text>
            <xsl:value-of select="page" />
          </td>
        </tr>
        <xsl:if test="notes != '' and $ProduceNotes">
          <tr>
            <td colspan="100%" class="notesrow2">
              <xsl:call-template name="PreserveLineBreaks">
                <xsl:with-param name="text" select="notes" />
              </xsl:call-template>
            </td>
          </tr>
        </xsl:if>
        <xsl:call-template name="Xline">
          <xsl:with-param name="cntl" select="last()-position()" />
          <xsl:with-param name="nte" select="notes != '' and $ProduceNotes" />
        </xsl:call-template>
      </xsl:for-each>
      <xsl:call-template name="Xline">
        <xsl:with-param name="cntl" select="last()-position()" />
        <xsl:with-param name="nte" select="notes != '' and $ProduceNotes" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:if test="martialartmaneuvers/martialartmaneuver">
      <tr><td colspan="100%"><strong><xsl:value-of select="$lang.Maneuvers" /></strong></td></tr>
      <tr><td colspan="100%" class="indent">
        <xsl:for-each select="martialartmaneuvers/martialartmaneuver">
          <xsl:sort select="name" />
          <xsl:value-of select="name" />
          <xsl:text> </xsl:text>
          <xsl:value-of select="source" />
          <xsl:text> </xsl:text>
          <xsl:value-of select="page" />
          <xsl:if test="last() &gt; 1">; </xsl:if>
        </xsl:for-each>
      </td></tr>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
