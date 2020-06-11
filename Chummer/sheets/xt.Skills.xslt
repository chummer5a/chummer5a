<?xml version="1.0" encoding="utf-8" ?>
<!-- Format Knowledge Skills column of Character Sheet -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template name="skills">

    <tr>
      <xsl:if test="position() mod 2 != 1">
        <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
      </xsl:if>
      <td valign="top">
        <xsl:value-of select="name"/>
        <xsl:if test="grouped = 'True'">*</xsl:if>
        <xsl:if test="spec != '' and exotic = 'True'"> (<xsl:value-of select="spec"/>)</xsl:if>
        <span style="color: grey; font-size: 6pt; vertical-align: bottom;">
          <xsl:text> </xsl:text>
          <xsl:value-of select="displayattribute"/>
          <xsl:choose>
            <xsl:when test="ratingmod &gt; 0">
              +<xsl:value-of select="ratingmod"/>
            </xsl:when>
            <xsl:when test="ratingmod &lt; 0">
              <xsl:text> </xsl:text>
              <xsl:value-of select="ratingmod"/>
            </xsl:when>
          </xsl:choose>
        </span>
        <xsl:if test="exotic = 'False' and count(skillspecializations/skillspecialization) &gt; 0">
          <p class="indent">
            <xsl:for-each select="skillspecializations/skillspecialization">
              <xsl:if test="position() != 1">
                <br />
              </xsl:if>
              (<xsl:value-of select="name"/> +<xsl:value-of select="specbonus"/>)
            </xsl:for-each>
          </p>
        </xsl:if>
      </td>
      <xsl:choose>
        <xsl:when test="islanguage = 'True' and rating = 0">
          <td colspan="2" style="valign: top; text-align: center;">
            <xsl:value-of select="$lang.Native"/>
          </td>
        </xsl:when>
        <xsl:otherwise>
          <td style="valign: top; text-align: center;">
            <xsl:value-of select="rating"/>
          </td>
          <td style="valign: top; text-align: center;">
            <xsl:value-of select="total"/>
          </td>
        </xsl:otherwise>
      </xsl:choose>
    </tr>

  </xsl:template>
</xsl:stylesheet>
