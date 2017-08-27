<?xml version="1.0" encoding="UTF-8" ?>
<!-- Skills grouped by Skill Category with Ratings more than zero listed by Rating (descending) -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="xz.language.xslt"/>

  <xsl:import href="Shadowrun 5 set.xslt"/>
  <xsl:import href="xs.SkillsGroupedByRating.xslt"/>

  <!-- Set global control variables -->
  <xsl:variable name="MinimumRating" select="1"/>
</xsl:stylesheet>