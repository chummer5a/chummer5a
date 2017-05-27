<?xml version="1.0" encoding="UTF-8" ?>
<!-- Character sheet with fancy blocks (French) -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:import href="xz.fr.xslt"/>

	<xsl:include href="../Fancy Blocks set.xslt"/>

	<xsl:include href="../xt.PreserveLineBreaks.xslt"/>
	<xsl:include href="../xs.TitleName.xslt"/>

	<!-- Set global variables -->
	<xsl:variable name="lang" select="'fr'"/>
</xsl:stylesheet>