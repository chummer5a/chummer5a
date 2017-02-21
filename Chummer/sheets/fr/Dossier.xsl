<?xml version="1.0" encoding="UTF-8" ?>
<!-- Dossier character summary sheet -->
<!-- Created by Jeff Halket, modified by Keith Rudolph, krudolph@gmail.com -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:include href="xt.PreserveLineBreaks.xslt"/>
	<xsl:include href="xt.TitleName.xslt"/>

	<xsl:template match="/characters/character">
		<xsl:variable name="TitleName">
			<xsl:call-template name="TitleName">
				<xsl:with-param name="name" select="name"/>
				<xsl:with-param name="alias" select="alias"/>
			</xsl:call-template>
		</xsl:variable>

		<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
			<head>
				<meta http-equiv="x-ua-compatible" content="IE=Edge"/>
				<title><xsl:value-of select="$TitleName" /></title>
				<style type="text/css">
					*
					{
						font-family: courier new, tahoma, trebuchet ms, arial;
						font-size: 10pt;
					}
					.indent
					{
						padding-left: 20px;
					}
					.hseparator
					{
						height: 5px;
					}
					.vseparator
					{
						width: 5px;
					}
					.label
					{
						font-weight: bold;
						width: 15%;
					}
					.value
					{
					}
				</style>
			</head>
			<body>
					<table width="100%" cellspacing="0" cellpadding="2">
						<tr>
							<td class="label">Nom:</td>
							<td class="value"><xsl:value-of select="name"/></td>
							<td rowspan="10" width="40%" align="center">
								<xsl:if test="mainmugshotbase64 != ''">
									<img src="data:image/png;base64,{mainmugshotbase64}" />
								</xsl:if>
							</td>
						</tr>
						<tr>
							<td class="label">Pseudo:</td>
							<td class="value"><xsl:value-of select="alias"/></td>
						</tr>
						<tr>
							<td class="label">Métatype:</td>
							<td class="value"><xsl:value-of select="metatype"/></td>
						</tr>
						<tr>
							<td class="label">Genre:</td>
							<td class="value"><xsl:value-of select="sex"/></td>
						</tr>
						<tr>
							<td class="label">Taille:</td>
							<td class="value"><xsl:value-of select="height"/></td>
						</tr>
						<tr>
							<td class="label">Poids:</td>
							<td class="value"><xsl:value-of select="weight"/></td>
						</tr>
						<tr>
							<td class="label">Age:</td>
							<td class="value"><xsl:value-of select="age"/></td>
						</tr>
						<tr>
							<td class="label">Couleur Cheveux:</td>
							<td class="value"><xsl:value-of select="hair"/></td>
						</tr>
						<tr>
							<td class="label">Couleur Yeux:</td>
							<td class="value"><xsl:value-of select="eyes"/></td>
						</tr>
						<tr>
							<td class="label">Couleur Peau:</td>
							<td class="value"><xsl:value-of select="skin"/></td>
						</tr>
						<tr>
							<td colspan="3" height="15"></td>
						</tr>
						<tr>
							<td class="label" valign="top">Description:</td>
							<td class="value" colspan="2">
								<xsl:call-template name="PreserveLineBreaks">
									<xsl:with-param name="text" select="description" />
								</xsl:call-template>
							</td>
						</tr>
						<tr>
							<td colspan="3" height="15"></td>
						</tr>
						<tr>
							<td class="label" valign="top">Notes:</td>
							<td class="value" colspan="2">
								<xsl:call-template name="PreserveLineBreaks">
									<xsl:with-param name="text" select="notes" />
								</xsl:call-template>
							</td>
						</tr>
						<tr>
							<td colspan="3" height="15"></td>
						</tr>
						<tr>
							<td class="label" valign="top">Background:</td>
							<td class="value" colspan="2">
								<xsl:call-template name="PreserveLineBreaks">
									<xsl:with-param name="text" select="background" />
								</xsl:call-template>
							</td>
						</tr>
					</table>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>