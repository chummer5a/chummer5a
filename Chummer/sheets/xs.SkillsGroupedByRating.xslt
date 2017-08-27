<?xml version="1.0" encoding="UTF-8" ?>
<!-- Skills grouped by Skill Category listed by Rating (descending) -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:include href="xt.MugShot.xslt"/>
  <xsl:include href="xt.Skills.xslt"/>

  <xsl:template name="skills1">
    <xsl:variable name="skillcut" select="round(count(skills/skill[knowledge = 'False' and (rating &gt;= $MinimumRating)]) div 2)"/>
    <xsl:variable name="sortedskills">
      <xsl:for-each select="skills/skill[knowledge = 'False' and (rating &gt;= $MinimumRating)]">
        <xsl:sort select="skillcategory"/>
        <xsl:sort select="rating" order="descending" />
        <xsl:if test="position() &lt;= $skillcut">
          <xsl:copy-of select="current()"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:for-each select="msxsl:node-set($sortedskills)/skill">
      <xsl:choose>
        <xsl:when test="position() = 1">
          <tr><td colspan="6" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="skillcategory" /> Skills</strong></td></tr>
        </xsl:when>
        <xsl:when test="skillcategory != preceding-sibling::skill[1]/skillcategory">
          <tr><td colspan="6" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="skillcategory" /> Skills</strong></td></tr>
        </xsl:when>
        <xsl:otherwise/>
      </xsl:choose>
      <xsl:call-template name="skills"/>
    </xsl:for-each>
  </xsl:template>
  
  <xsl:template name="skills2">
    <xsl:variable name="skillcut" select="round(count(skills/skill[knowledge = 'False' and (rating &gt;= $MinimumRating)]) div 2)"/>
    <xsl:variable name="sortedskills">
      <xsl:for-each select="skills/skill[knowledge = 'False' and (rating &gt;= $MinimumRating)]">
        <xsl:sort select="skillcategory" />
        <xsl:sort select="rating" order="descending" />
        <xsl:if test="position() &gt; $skillcut">
          <xsl:copy-of select="current()"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:for-each select="msxsl:node-set($sortedskills)/skill">
      <xsl:choose>
        <xsl:when test="position() = 1">
          <tr><td colspan="6" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="skillcategory" /> Skills</strong></td></tr>
        </xsl:when>
        <xsl:when test="skillcategory != preceding-sibling::skill[1]/skillcategory">
          <tr><td colspan="6" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="skillcategory" /> Skills</strong></td></tr>
        </xsl:when>
        <xsl:otherwise/>
      </xsl:choose>
      <xsl:call-template name="skills"/>
    </xsl:for-each>
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
    <xsl:call-template name="MugShot"/>
  </xsl:template>
</xsl:stylesheet>
