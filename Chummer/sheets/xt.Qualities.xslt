<?xml version="1.0" encoding="UTF-8" ?>
<!-- Character contacts -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:template name="Qualities">
    <xsl:param name="quality_type" />

    <xsl:for-each select="qualities/quality[qualitytype_english=$quality_type]">
      <xsl:sort select="qualitysource='Metatype'" order='descending' />
      <xsl:sort select="substring-after(notes, 'Prefix: ')"/>
      <xsl:sort select="(source = 'RF') and (page &gt; 110) and (page &lt; 124)"/>
      <xsl:sort select="name" />
      <xsl:variable select="substring-after(notes, 'Prefix: ')" name="prefix" />
      <xsl:variable select="substring-before(notes, 'Prefix: ')" name="notesbody" />
      <tr style="text-align: left" valign="top">
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:if test="$prefix != ''"><xsl:value-of select="$prefix"/> - </xsl:if>
          <xsl:value-of select="name" />
          <xsl:if test="(source = 'RF') and (page &gt; 110) and (page &lt; 124)" > (metagenic)</xsl:if>
          <xsl:if test="extra != ''">: <xsl:value-of select="extra" /></xsl:if>
        </td>
        <td />
        <td style="text-align: center">
          <xsl:value-of select="source" />
          <xsl:text> </xsl:text>
          <xsl:value-of select="page" />
        </td>
      </tr>
      <xsl:if test="$notesbody != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100" style="padding: 0 2%; text-align: justify;">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="$notesbody" />
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
      <xsl:call-template name="Xline">
        <xsl:with-param name="cntl" select="last()-position()" />
        <xsl:with-param name="nte" select="$notesbody != '' and $ProduceNotes" />
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
