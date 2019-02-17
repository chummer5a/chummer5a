<?xml version="1.0" encoding="utf-8" ?>
<!-- Skills listed in alphabetically order -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:include href="xt.Skills.xslt"/>
  <xsl:include href="xt.SkillGroups.xslt"/>

  <xsl:template name="skills1">
    <xsl:variable name="items" select="skills/skill[knowledge = 'False' and (rating &gt; 0 or total &gt; 0)]"/>
    <xsl:variable name="halfcut" select="round((count($items) + count(skills/skillgroup)) div 2)"/>
    <xsl:for-each select="$items[position() &lt;= $halfcut]">
      <xsl:sort select="name"/>
      <xsl:if test="$PrintSkillCategoryNames = 'True'">
        <xsl:choose>
          <xsl:when test="position() = 1">
            <tr>
              <td colspan="6" style="border-bottom:solid black 1px;">
                <strong>
                  <xsl:value-of select="skillcategory" /><xsl:text> </xsl:text><xsl:value-of select="$lang.Skills"/>
                </strong>
              </td>
            </tr>
          </xsl:when>
          <xsl:when test="skillcategory != preceding-sibling::skill[1]/skillcategory">
            <tr>
              <td colspan="6" style="border-bottom:solid black 1px;">
                <strong>
                  <xsl:value-of select="skillcategory" /><xsl:text> </xsl:text><xsl:value-of select="$lang.Skills"/>
                </strong>
              </td>
            </tr>
          </xsl:when>
          <xsl:otherwise/>
        </xsl:choose>
      </xsl:if>
      <xsl:call-template name="skills"/>
    </xsl:for-each>
  </xsl:template>
  
  <xsl:template name="skills2">
    <xsl:variable name="items" select="skills/skill[knowledge = 'False' and (rating &gt; 0 or total &gt; 0)]"/>
    <xsl:variable name="halfcut" select="round((count($items) + count(skills/skillgroup)) div 2)"/>
    <xsl:for-each select="$items[position() &gt; $halfcut]">
      <xsl:sort select="name"/>
      <xsl:if test="$PrintSkillCategoryNames = 'True'">
        <xsl:choose>
          <xsl:when test="position() = 1">
            <tr>
              <td colspan="6" style="border-bottom:solid black 1px;">
                <strong>
                  <xsl:value-of select="skillcategory" /><xsl:text> </xsl:text><xsl:value-of select="$lang.Skills"/>
                </strong>
              </td>
            </tr>
          </xsl:when>
          <xsl:when test="skillcategory != preceding-sibling::skill[1]/skillcategory">
            <tr>
              <td colspan="6" style="border-bottom:solid black 1px;">
                <strong>
                  <xsl:value-of select="skillcategory" /><xsl:text> </xsl:text><xsl:value-of select="$lang.Skills"/>
                </strong>
              </td>
            </tr>
          </xsl:when>
          <xsl:otherwise/>
        </xsl:choose>
      </xsl:if>
      <xsl:call-template name="skills"/>
    </xsl:for-each>
    <xsl:if test="count(skills/skillgroup) &gt; 0">
      <xsl:if test="$PrintSkillCategoryNames = 'True'">
        <tr>
          <td colspan="6" style="border-bottom:solid black 1px;">
            <strong><xsl:text> </xsl:text><xsl:value-of select="$lang.SkillGroups"/></strong>
          </td>
        </tr>
      </xsl:if>
      <xsl:for-each select="skills/skillgroup">
        <xsl:sort select="name"/>
        <xsl:call-template name="skillgroups"/>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <xsl:template name="skills3">
    <xsl:for-each select="skills/skill[knowledge = 'True' and islanguage = 'True']">
      <xsl:sort select="name" />
      <xsl:call-template name="skills"/>
    </xsl:for-each>
    <xsl:for-each select="skills/skill[knowledge = 'True' and islanguage != 'True']">
      <xsl:sort select="name" />
      <xsl:call-template name="skills"/>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
