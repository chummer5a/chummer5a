<?xml version="1.0" encoding="UTF-8" ?>
<!-- Character notes -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/characters/character">
		<xsl:variable name="TitleName">
			<xsl:call-template name="TitleName">
				<xsl:with-param name="name" select="name"/>
				<xsl:with-param name="alias" select="alias"/>
			</xsl:call-template>
		</xsl:variable>
		<title><xsl:value-of select="$TitleName"/></title>

		<xsl:text disable-output-escaping="yes"><![CDATA[<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">]]></xsl:text>
		<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
			<head>
				<meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
				<style type="text/css">
					* {
						font-family: 'courier new', tahoma, 'trebuchet ms', arial;
						font-size: 10pt;
						margin: 0;
						text-align: left;
						vertical-align: top;
					}
					html {
						height: 100%;
						margin: 0px;  /* this affects the margin on the html before sending to printer */
					}
					.tablestyle {
						border-collapse: collapse;
						border-color: #1c4a2d;
						border-style: solid;
						border-width: 0.5mm;
						cellpadding: 2;
						cellspacing: 0;
						width: 100%;
					}
					.bigheader {
						background-color: #e4e4e4;
						font-size: 150%;
						font-weight: bold;
						text-align: center;
						padding-bottom: 2mm;
						padding-top: 2mm;
						vertical-align: center;
					}
					.block {
						page-break-inside: avoid;
					}
				</style>
				<style media="print">
				   @page {
						size: auto;
						margin-top: 0.5in;
						margin-left: 0.5in;
						margin-right: 0.5in;
						margin-bottom: 0.75in;
					}
					.block {
						bottom-padding: 0.75;
						page-break-inside: avoid !important;
						margin: 4px 0 4px 0;  /* to keep the page break from cutting too close to the text in the div */
					}
				</style>
			</head>

			<body>
				<div id="TitleBlock">
					<table class="tablestyle">
						<tr><td class="bigheader">
							<xsl:value-of select="$TitleName"/>: <xsl:value-of select="$lang.Notes"/>
						</td></tr>
					</table>
				</div>

				<xsl:if test="qualities/quality/notes!=''">
					<table><tr><td/></tr></table>
					<xsl:call-template name="TableTitle">
						<xsl:with-param name="name" select="$lang.Qualities"/>
					</xsl:call-template>
					<div class="block" id="QualitiesBlock">
						<table class="tablestyle">
							<tr>
								<th width="80%"/>
								<th width="10%"/>
								<th width="10%"/>
							</tr>
							<xsl:call-template name="Qualities"/>
						</table>
					</div>
					<xsl:call-template name="RowSummary">
						<xsl:with-param name="text" select="$lang.Qualities"/>
						<xsl:with-param name="bnme" select="'QualitiesBlock'"/>
					</xsl:call-template>
				</xsl:if>

				<xsl:if test="spells/spell/notes != ''">
					<table><tr><td/></tr></table>
					<xsl:call-template name="TableTitle">
						<xsl:with-param name="name" select="$lang.Spells"/>
					</xsl:call-template>
					<div class="block" id="SpellsBlock">
						<table class="tablestyle">
							<xsl:call-template name="Spells"/>
						</table>
					</div>
					<xsl:call-template name="RowSummary">
						<xsl:with-param name="text" select="$lang.Spells"/>
						<xsl:with-param name="bnme" select="'SpellsBlock'"/>
					</xsl:call-template>
				</xsl:if>

				<xsl:if test="lifestyles/lifestyle/notes != ''">
					<table><tr><td/></tr></table>
					<xsl:call-template name="TableTitle">
						<xsl:with-param name="name" select="$lang.Lifestyle"/>
					</xsl:call-template>
					<div class="block" id="LifestyleBlock">
						<table class="tablestyle">
							<xsl:call-template name="Lifestyles"/>
						</table>
					</div>
					<xsl:call-template name="RowSummary">
						<xsl:with-param name="text" select="$lang.Lifestyle"/>
						<xsl:with-param name="bnme" select="'LifestyleBlock'"/>
					</xsl:call-template>
				</xsl:if>

				<xsl:if test="concat(concept,description,background,notes,gamenotes) !=''">
					<xsl:call-template name="notes"/>
				</xsl:if>
			</body>
		</html>

	</xsl:template>
</xsl:stylesheet>