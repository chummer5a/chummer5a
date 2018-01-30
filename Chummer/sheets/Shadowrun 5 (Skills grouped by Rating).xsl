<?xml version="1.0" encoding="utf-8" ?>
<!-- Character sheet with skills listed by Rating (descending) -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="xz.language.xslt"/>

  <xsl:import href="Shadowrun 5 set.xslt"/>
  <xsl:import href="Shadowrun 5 set CSS.xslt"/>
  <xsl:import href="xs.SkillsGroupedByPool.xslt"/>

  <!-- Set global control variables -->
  <xsl:variable name="PrintSkillCategoryNames" select="false()"/>
</xsl:stylesheet>
