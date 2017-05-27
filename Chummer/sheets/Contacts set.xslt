<?xml version="1.0" encoding="UTF-8" ?>
<!-- Contact List -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/characters/character">
		<xsl:variable name="TitleName">
			<xsl:call-template name="TitleName">
				<xsl:with-param name="name" select="name"/>
				<xsl:with-param name="alias" select="alias"/>
			</xsl:call-template>
		</xsl:variable>

		<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
			<head>
				<meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
				<title>
					<xsl:value-of select="$TitleName"/>
				</title>
				<style type="text/css">
					* {
						font-family: 'courier new', tahoma, 'trebuchet ms', arial;
						font-size: 10pt;
						text-align: center;
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
					<table class="tablestyle" style="border-bottom: 0">
						<tr><td class="bigheader">
							<xsl:value-of select="$TitleName"/>: <xsl:value-of select="$lang.ContactList"/>
						</td></tr>
					</table>
				</div>

				<div class="block" id="ContactsBlock">
					<table class="tablestyle">
						<tr style="font-weight: bold; text-decoration: underline; text-transform: uppercase;">
							<td width="25%" style="text-align: left">
								<xsl:value-of select="$lang.Name"/>
							</td>
							<td width="25%">
								<xsl:value-of select="$lang.Location"/>
							</td>
							<td width="25%">
								<xsl:value-of select="$lang.Archetype"/>
							</td>
							<td width="15%">
								<xsl:value-of select="$lang.Connection"/>
							</td>
							<td width="10%">
								<xsl:value-of select="$lang.Loyalty"/>
							</td>
						</tr>
						<xsl:call-template name="Contacts"/>
					</table>
				</div>
			</body>
		</html>
	</xsl:template>

</xsl:stylesheet>