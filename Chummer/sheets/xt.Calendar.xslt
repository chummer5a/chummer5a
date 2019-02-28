<?xml version="1.0" encoding="UTF-8" ?>
<!-- Calendar -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template name="Calendar">

    <xsl:for-each select="calendar/week">
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:value-of select="year"/>
          <xsl:text>, </xsl:text>
          <xsl:value-of select="$lang.Month"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="month"/>
          <xsl:text>, </xsl:text>
          <xsl:value-of select="$lang.Week"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="week"/>
        </td>
        <td>
          <xsl:call-template name="PreserveLineBreaks">
            <xsl:with-param name="text" select="notes"/>
          </xsl:call-template>
        </td>
      </tr>
      <xsl:call-template name="Xline">
        <xsl:with-param name="cntl" select="last()-position()"/>
      </xsl:call-template>
    </xsl:for-each>

  </xsl:template>
</xsl:stylesheet>