<?xml version="1.0" encoding="UTF-8" ?>
<!-- Karma and Nuyen expenses list-->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:template name="Expenses">
      <xsl:param name="type"/>
      <xsl:param name="sfx" select="''"/>

    <xsl:for-each select="expenses/expense[type = $type]">
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td><xsl:value-of select="date"/></td>
        <td/>
        <td style="text-align:right">
          <xsl:call-template name="fnx-fmt-nmbr">
            <xsl:with-param name="nmbr" select="amount"/>
          </xsl:call-template>
          <xsl:value-of select="$sfx"/>
        </td>
        <td/>
        <td><xsl:value-of select="reason"/></td>
      </tr>
    </xsl:for-each>

  </xsl:template>
</xsl:stylesheet>