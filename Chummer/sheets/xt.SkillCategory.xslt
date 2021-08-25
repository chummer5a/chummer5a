<?xml version="1.0" encoding="utf-8" ?>
<!-- Format Skill Category heading for Skills section of Character Sheet -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template name="skillcategorytitle">
      <xsl:param name="mindthegap" select="false()" />

      <xsl:if test="$PrintSkillCategoryNames">
        <xsl:choose>
          <xsl:when test="position() = 1 and $mindthegap">
            <tr>
              <td colspan="6" style="border-bottom:solid black 1px; padding-top: 1em;">
                <strong>
                  <xsl:value-of select="skillcategory" />
                </strong>
              </td>
            </tr>
          </xsl:when>
          <xsl:when test="position() = 1">
            <tr>
              <td colspan="6" style="border-bottom:solid black 1px;">
                <strong>
                  <xsl:value-of select="skillcategory" />
                </strong>
              </td>
            </tr>
          </xsl:when>
          <xsl:when test="skillcategory != preceding-sibling::skill[1]/skillcategory">
            <tr>
              <td colspan="6" style="border-bottom:solid black 1px; padding-top: 1em;">
                <strong>
                  <xsl:value-of select="skillcategory" />
                </strong>
              </td>
            </tr>
          </xsl:when>
          <xsl:otherwise />
        </xsl:choose>
      </xsl:if>
  </xsl:template>
</xsl:stylesheet>
