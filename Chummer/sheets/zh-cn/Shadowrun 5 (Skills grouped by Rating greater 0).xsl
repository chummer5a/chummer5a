<?xml version="1.0" encoding="utf-8" ?>
<!-- Character sheet with skills listed by Rating more than zero (descending) -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="xz.language.xslt" />

  <xsl:import href="../Shadowrun 5 set.xslt" />
  <xsl:import href="../xs.SkillsGroupedByRating.xslt" />

  <!-- Set global control variables -->
  <xsl:variable name="MinimumRating" select="number(1)" />
  <xsl:variable name="PrintSkillCategoryNames" select="boolean(false())" />
</xsl:stylesheet>
