<?xml version="1.0" encoding="utf-8" ?>
<!-- Format Skill Groups section of Character Sheet -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template name="skillgroups">

    <tr>
      <xsl:if test="position() mod 2 != 1">
        <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
      </xsl:if>
      <td valign="top" style="valign: top; text-align: right;">
        * <xsl:value-of select="name" />
      </td>
      <td colspan="2" style="valign: top; text-align: center;">
        <xsl:value-of select="rating" />
      </td>
    </tr>
  </xsl:template>
</xsl:stylesheet>
