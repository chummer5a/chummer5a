<?xml version="1.0" encoding="UTF-8" ?>
<!-- Vehicles (French) -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:import href="xz.fr.xslt"/>

	<xsl:include href="../Vehicles set.xslt"/>

	<xsl:include href="../xs.fnx.xslt"/>
	<xsl:include href="../xs.TitleName.xslt"/>

	<xsl:include href="../xt.ConditionMonitor.xslt"/>
	<xsl:include href="../xt.PreserveLineBreaks.xslt"/>
	<xsl:include href="../xt.RangedWeapons.xslt"/>
	<xsl:include href="../xt.RowSummary.xslt"/>
	<xsl:include href="../xt.RuleLine.xslt"/>

	<!-- Set global variables -->
	<xsl:variable name="lang" select="'fr'"/>
	<xsl:variable name="ProduceNotes" select="true()"/>
</xsl:stylesheet>