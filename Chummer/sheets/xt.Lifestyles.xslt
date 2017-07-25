<?xml version="1.0" encoding="UTF-8" ?>
<!-- Character contacts -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:template name="Lifestyles">

    <tr style="font-weight: bold; text-transform: uppercase;">
      <td width="50%">
        <xsl:value-of select="$lang.Lifestyle"/>
      </td>
      <td width="10%" style="text-align: center">
        <xsl:value-of select="$lang.Level"/>
      </td>
      <td width="10%" style="text-align: center">
        <xsl:value-of select="$lang.Cost"/>
      </td>
      <td width="10%" style="text-align: center">
        <xsl:value-of select="$lang.Months"/>
      </td>
      <td width="10%"/>
      <td width="10%"/>
    </tr>

    <xsl:for-each select="lifestyles/lifestyle">
      <xsl:sort select="name"/>
      <tr>
        <td>
          <xsl:value-of select="name"/>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="baselifestyle"/>
        </td>
        <td style="text-align: center">
          <xsl:call-template name="fnx-fmt-nmbr">
            <xsl:with-param name="nmbr" select="totalmonthlycost"/>        
          </xsl:call-template>
          <xsl:value-of select="$lang.NuyenSymbol"/>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="months"/>
        </td>
        <td/>
        <td style="text-align: center">
          <xsl:value-of select="source"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="page"/>
        </td>
      </tr>

      <tr><td colspan="100%" style="padding: 0 2%; text-align: justify;">
        <xsl:for-each select="qualities/quality">
          <xsl:value-of select="name"/>
          <text>; </text>
        </xsl:for-each>
      </td></tr>

      <xsl:if test="notes != '' and $ProduceNotes">
        <xsl:call-template name="RuleLine"/>
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" style="text-align: justify">
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