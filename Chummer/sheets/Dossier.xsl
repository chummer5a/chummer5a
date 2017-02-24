<?xml version="1.0" encoding="UTF-8" ?>
<!-- Dossier (English - US) -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:import href="xz.en-us.xslt"/>

	<xsl:include href="Dossier set.xslt"/>

	<xsl:include href="xs.TitleName.xslt"/>

	<xsl:include href="xt.PreserveLineBreaks.xslt"/>

	<!-- Set global variables -->
	<xsl:variable name="lang" select="'en'"/>
</xsl:stylesheet>