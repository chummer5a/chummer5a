<?xml version="1.0" encoding="utf-8" ?>
<!-- Skills listed in Rating order (descending) -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:include href="xt.SkillCategory.xslt" />
  <xsl:include href="xt.Skills.xslt" />
  <xsl:include href="xt.SkillGroups.xslt" />

  <xsl:template name="skills1">
    <xsl:variable name="skillcut" select="number(round((count(skills/skill[knowledge = 'False' and total &gt; 0]) + count(skills/skillgroup)) div 2))" />
    <xsl:variable name="sortedskills">
      <xsl:choose>
        <xsl:when test="$PrintSkillCategoryNames">
          <xsl:for-each select="skills/skill[knowledge = 'False' and total &gt; 0]">
            <xsl:sort select="skillcategory" />
            <xsl:sort select="rating" order="descending" />
            <xsl:sort select="name" />
            <xsl:if test="position() &lt;= $skillcut">
              <xsl:copy-of select="current()" />
            </xsl:if>
          </xsl:for-each>
        </xsl:when>
        <xsl:otherwise>
          <xsl:for-each select="skills/skill[knowledge = 'False' and total &gt; 0]">
            <xsl:sort select="rating" order="descending" />
            <xsl:sort select="name" />
            <xsl:if test="position() &lt;= $skillcut">
              <xsl:copy-of select="current()" />
            </xsl:if>
          </xsl:for-each>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:for-each select="msxsl:node-set($sortedskills)/skill">
      <xsl:call-template name="skillcategorytitle" />
      <xsl:call-template name="skills" />
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="skills2">
    <xsl:variable name="skillcut" select="number(round((count(skills/skill[knowledge = 'False' and total &gt; 0]) + count(skills/skillgroup)) div 2))" />
    <xsl:variable name="sortedskills">
      <xsl:choose>
        <xsl:when test="$PrintSkillCategoryNames">
          <xsl:for-each select="skills/skill[knowledge = 'False' and total &gt; 0]">
            <xsl:sort select="skillcategory" />
            <xsl:sort select="rating" order="descending" />
            <xsl:sort select="name" />
            <xsl:if test="position() &gt; $skillcut">
              <xsl:copy-of select="current()" />
            </xsl:if>
          </xsl:for-each>
        </xsl:when>
        <xsl:otherwise>
          <xsl:for-each select="skills/skill[knowledge = 'False' and total &gt; 0]">
            <xsl:sort select="rating" order="descending" />
            <xsl:sort select="name" />
            <xsl:if test="position() &gt; $skillcut">
              <xsl:copy-of select="current()" />
            </xsl:if>
          </xsl:for-each>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:for-each select="msxsl:node-set($sortedskills)/skill">
      <xsl:call-template name="skillcategorytitle" />
      <xsl:call-template name="skills" />
    </xsl:for-each>
    <xsl:if test="count(skills/skillgroup) &gt; 0">
      <xsl:if test="$PrintSkillCategoryNames">
        <tr>
          <td colspan="6" style="border-bottom:solid black 1px; padding-top: 1em;">
            <strong><xsl:value-of select="$lang.SkillGroups" /></strong>
          </td>
        </tr>
      </xsl:if>
      <xsl:for-each select="skills/skillgroup">
        <xsl:sort select="rating" order="descending" />
        <xsl:sort select="name" />
        <xsl:call-template name="skillgroups" />
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <xsl:template name="skills3">
    <xsl:variable name="sortedlanguageskills">
      <xsl:for-each select="skills/skill[knowledge = 'True' and isnativelanguage = 'True']">
        <xsl:sort select="name" />
        <xsl:copy-of select="current()" />
      </xsl:for-each>
      <xsl:if test="$PrintSkillCategoryNames">
        <xsl:for-each select="skills/skill[knowledge = 'True' and islanguage = 'True' and isnativelanguage != 'True']">
          <xsl:sort select="rating" order="descending" />
          <xsl:sort select="name" />
          <xsl:copy-of select="current()" />
        </xsl:for-each>
      </xsl:if>
    </xsl:variable>
	<xsl:variable name="arelanguages" select="number(count(msxsl:node-set($sortedlanguageskills)/skill))" />
    <xsl:variable name="sortedknowledgeskills">
      <xsl:choose>
        <xsl:when test="$PrintSkillCategoryNames">
          <xsl:for-each select="skills/skill[knowledge = 'True' and islanguage != 'True' and isnativelanguage != 'True']">
            <xsl:sort select="skillcategory" />
            <xsl:sort select="rating" order="descending" />
            <xsl:sort select="name" />
            <xsl:copy-of select="current()" />
          </xsl:for-each>
        </xsl:when>
        <xsl:otherwise>
          <xsl:for-each select="skills/skill[knowledge = 'True' and (islanguage != 'True' or isnativelanguage != 'True')]">
            <xsl:sort select="rating" order="descending" />
            <xsl:sort select="name" />
            <xsl:copy-of select="current()" />
          </xsl:for-each>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:for-each select="msxsl:node-set($sortedlanguageskills)/skill">
      <xsl:call-template name="skillcategorytitle" />
      <xsl:call-template name="skills" />
    </xsl:for-each>
    <xsl:for-each select="msxsl:node-set($sortedknowledgeskills)/skill">
      <xsl:call-template name="skillcategorytitle">
        <xsl:with-param name="mindthegap" select="$arelanguages &gt; 0" />
      </xsl:call-template>
      <xsl:call-template name="skills" />
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
