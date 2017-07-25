<?xml version="1.0" encoding="UTF-8" ?>
<!-- Skills listed in alphabetically order -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:include href="xt.MugShot.xslt"/>
  <xsl:include href="xt.Skills.xslt"/>

  <xsl:template name="skills1">
    <xsl:variable name="items" select="skills/skill[knowledge = 'False' and (rating &gt; 0 or total &gt; 0)]"/>
    <xsl:variable name="halfcut" select="round(count($items) div 2)"/>
    <xsl:for-each select="$items[position() &lt;= $halfcut]">
      <xsl:sort select="name"/>
      <xsl:call-template name="skills"/>
    </xsl:for-each>
  </xsl:template>
  
  <xsl:template name="skills2">
    <xsl:variable name="items" select="skills/skill[knowledge = 'False' and (rating &gt; 0 or total &gt; 0)]"/>
    <xsl:variable name="halfcut" select="round(count($items) div 2)"/>
    <xsl:for-each select="$items[position() &gt; $halfcut]">
      <xsl:sort select="name"/>
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
