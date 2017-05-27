<?xml version="1.0" encoding="UTF-8" ?>
<!-- Notes (Japanese) -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:import href="xz.jp.xslt"/>

	<xsl:include href="../Notes set.xslt"/>

	<xsl:include href="../xs.fnx.xslt"/>
	<xsl:include href="../xs.TitleName.xslt"/>

	<xsl:include href="../xt.Lifestyles.xslt"/>
	<xsl:include href="../xt.Notes.xslt"/>
	<xsl:include href="../xt.PreserveLineBreaks.xslt"/>
	<xsl:include href="../xt.Qualities.xslt"/>
	<xsl:include href="../xt.RowSummary.xslt"/>
	<xsl:include href="../xt.RuleLine.xslt"/>
	<xsl:include href="../xt.Spells.xslt"/>
	<xsl:include href="../xt.TableTitle.xslt"/>

	<!-- Set global variables -->
	<xsl:variable name="lang" select="'jp'"/>
	<xsl:variable name="ProduceNotes" select="true()"/>
</xsl:stylesheet>