<?xml version="1.0" encoding="UTF-8" ?>
<!-- Calendar (French) -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:import href="xz.fr.xslt"/>

    <xsl:include href="../Calendar set.xslt"/>

    <xsl:include href="../xs.fnx.xslt"/>
    <xsl:include href="../xs.TitleName.xslt"/>

    <xsl:include href="../xt.Calendar.xslt"/>
    <xsl:include href="../xt.PreserveLineBreaks.xslt"/>
    <xsl:include href="../xt.RuleLine.xslt"/>

    <!-- Set global variables -->
    <xsl:variable name="lang" select="'fr'"/>
</xsl:stylesheet>