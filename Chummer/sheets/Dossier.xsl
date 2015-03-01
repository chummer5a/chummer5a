<?xml version="1.0" encoding="UTF-8" ?>
<!-- Dossier character summary sheet -->
<!-- Created by Jeff Halket, modified by Keith Rudolph, krudolph@gmail.com -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	
	<xsl:template match="/characters">
		<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
			<head>
				<meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
				<title>Dossier</title>
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
				<xsl:for-each select="character">
					<table width="100%" cellspacing="0" cellpadding="2">
						<tr>
							<td class="label">Name:</td>
							<td class="value"><xsl:value-of select="name"/></td>
							<td rowspan="10" width="40%" align="center">
								<xsl:if test="mugshot != ''">
									<img src="{mugshot}" />
								</xsl:if>
							</td>
						</tr>
						<tr>
							<td class="label">Alias:</td>
							<td class="value"><xsl:value-of select="alias"/></td>
						</tr>
						<tr>
							<td class="label">Metatype:</td>
							<td class="value"><xsl:value-of select="metatype"/></td>
						</tr>
						<tr>
							<td class="label">Gender:</td>
							<td class="value"><xsl:value-of select="sex"/></td>
						</tr>
						<tr>
							<td class="label">Height:</td>
							<td class="value"><xsl:value-of select="height"/></td>
						</tr>
						<tr>
							<td class="label">Weight:</td>
							<td class="value"><xsl:value-of select="weight"/></td>
						</tr>
						<tr>
							<td class="label">Age:</td>
							<td class="value"><xsl:value-of select="age"/></td>
						</tr>
						<tr>
							<td class="label">Hair Color:</td>
							<td class="value"><xsl:value-of select="hair"/></td>
						</tr>
						<tr>
							<td class="label">Eye Color:</td>
							<td class="value"><xsl:value-of select="eyes"/></td>
						</tr>
						<tr>
							<td class="label">Skin Color:</td>
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
				</xsl:for-each>
			</body>
		</html>
	</xsl:template>
	
	<xsl:template name="PreserveLineBreaks">
		<xsl:param name="text"/>
		<xsl:choose>
			<xsl:when test="contains($text,'&#xA;')">
				<xsl:value-of select="substring-before($text,'&#xA;')"/>
				<br/>
				<xsl:call-template name="PreserveLineBreaks">
					<xsl:with-param name="text">
						<xsl:value-of select="substring-after($text,'&#xA;')"/>
					</xsl:with-param>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$text"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>