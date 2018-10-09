<?xml version="1.0" encoding="utf-8" ?>
<!-- Character lifestyles -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:template name="Lifestyles">

    <tr class="title">
      <th width="40%" style="text-align: left">
        <xsl:value-of select="$lang.Lifestyle"/>
      </th>
      <th width="10%" style="text-align: center">
        <xsl:value-of select="$lang.Level"/>
      </th>
      <th width="10%" style="text-align: center">
        <xsl:value-of select="$lang.Cost"/>
      </th>
      <th width="20%" style="text-align: center">
        <xsl:value-of select="$lang.Duration"/>
      </th>
      <th width="10%"/>
      <th width="10%"/>
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
        <td style="text-align: center;white-space: nowrap;">
          <xsl:value-of select="totalmonthlycost"/>
          <xsl:value-of select="$lang.NuyenSymbol"/>
        </td>
        <td style="text-align: center">
          <xsl:value-of select="months"/>&#160;
          <xsl:choose>
            <xsl:when test="increment = 'Day'">
              <xsl:choose>
                <xsl:when test="months = '1'">
                  <xsl:value-of select="$lang.Day"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$lang.Days"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="increment = 'Week'">
              <xsl:choose>
                <xsl:when test="months = '1'">
                  <xsl:value-of select="$lang.Week"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$lang.Weeks"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="months = '1'">
                  <xsl:value-of select="$lang.Month"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$lang.Months"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
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
