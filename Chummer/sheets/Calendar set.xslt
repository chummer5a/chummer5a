<?xml version="1.0" encoding="UTF-8" ?>
<!-- Calendar -->
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
					th {
						text-align: center;
						text-decoration: underline;
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
						<tr><td colspan="100%" class="bigheader">
							<xsl:value-of select="$TitleName"/>: <xsl:value-of select="$lang.Calendar"/>
						</td></tr>
					</table>
				</div>

				<table><tr><td/></tr></table>
				<div class="block" id="calendarblock">
					<table class="tablestyle">
						<tr>
							<th style="width: 30%">
								<xsl:value-of select="$lang.Date"/>
							</th>
							<th style="width: 70%; text-align: left;">
								<xsl:value-of select="$lang.Notes"/>
							</th>
						</tr>
						<xsl:call-template name="Calendar"/>
					</table>
				</div>
			</body>
		</html>
	</xsl:template>

</xsl:stylesheet>