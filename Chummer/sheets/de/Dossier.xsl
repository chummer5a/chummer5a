<?xml version="1.0" encoding="UTF-8" ?>
<!-- Dossier (German) -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:import href="xz.de.xslt"/>

	<xsl:include href="../Dossier set.xslt"/>

	<xsl:include href="../xs.TitleName.xslt"/>

	<xsl:include href="../xt.PreserveLineBreaks.xslt"/>

	<!-- Set global variables -->
	<xsl:variable name="lang" select="'de'"/>
</xsl:stylesheet>