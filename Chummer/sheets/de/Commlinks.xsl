<?xml version="1.0" encoding="UTF-8" ?>
<!-- Character sheet for Commlinks based on the ones created by ChinaGreenElvis -->
<!-- Created by Keith Rudolph, krudolph@gmail.com -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
	<xsl:include href="ConditionMonitor.xslt"/>
	<xsl:template match="/characters/character">
		<xsl:text disable-output-escaping="yes"><![CDATA[<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">]]></xsl:text>
		<html>
			<head>
				<meta http-equiv="x-ua-compatible" content="IE=Edge"/>
				<title><xsl:value-of select="name" /></title>
				<style type="text/css">
					*
					{
						font-family: segoe condensed, tahoma, trebuchet ms, arial;
						font-size: 8pt;
						text-align: left;
					}
					.tableborder
					{
						border: solid 2px #1c4a2d;
						height: 10px;
						padding: 5px;
					}
					.conditionmonitorbox
					{
						border: solid 2px #1c4a2d;
						width: 24px;
						height: 24px;
						text-align: right;
						vertical-align: text-bottom;
						font-weight: bold;
					}
					.conditionmonitorboxfilled
					{
						border: solid 2px #1c4a2d;
						width: 24px;
						height: 24px;
						text-align: right;
						vertical-align: text-bottom;
						font-weight: bold;
						background-color: #bbbbbb;
					}
					.conditionmonitorboxnotused
					{
						border: solid 0px #1c4a2d;
						width: 24px;
						height: 24px;
						text-align: right;
						vertical-align: text-bottom;
						font-weight: bold;
						background-color: #FFFFFF;
					}
					.rowsummary
					{
						filter: progid:DXImageTransform.Microsoft.gradient(startColorstr='#3a6349', endColorstr='#769582'); /* for IE */
						background: -webkit-gradient(linear, left top, left bottom, from(#3a6349), to(#769582)); /* for webkit browsers */
						background: -moz-linear-gradient(top,  #3a6349,  #769582); /* for firefox 3.6+ */ 
						background-color: #3a6349;
						color: #ffffff;
						font-weight: bold;
						font-style: italic;
					}
					.rowsummarybutton
					{
						color: #ffffff;
						font-weight: bold;
						margin-left: 20px;
					}
					.attributecell
					{
					}
					.attributecell p
					{
						padding: 2px;
						margin: 4px;
						border: solid 1px #1c4a2d;
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
					.block
					{
						page-break-inside: avoid;
					}
					.zalomit
					{
						page-break-after: always;
					}
					
					@media print
					{
						.zalomit
						{
							page-break-after: always;
						}
						.sectionhide
						{
							visibility: hidden;
							display:none;
						}
						.rowsummarybutton
						{
							visibility: hidden;
							display: none;
						}
						.table
						{
							width: 100%;
						}
					}
					
					@media screen {
						.sectionhide
						{
							visibility: visible;
						}
						.rowsummarybutton
						{
							visibility: visible;
						}
					}
				</style>
			</head>
			<body>
				<xsl:for-each select="gears/gear[iscommlink = 'True' or isnexus = 'True']">
					<xsl:call-template name="commlink">
						<xsl:with-param name="rendercommlink"><xsl:value-of select="." /></xsl:with-param>
					</xsl:call-template>
				</xsl:for-each>
				<xsl:for-each select="armors/armor/gears/gear[iscommlink = 'True' or isnexus = 'True']">
					<xsl:call-template name="commlink">
						<xsl:with-param name="rendercommlink"><xsl:value-of select="." /></xsl:with-param>
					</xsl:call-template>
				</xsl:for-each>
				<xsl:for-each select="cyberwares/cyberware/gears/gear[iscommlink = 'True' or isnexus = 'True']">
					<xsl:call-template name="commlink">
						<xsl:with-param name="rendercommlink"><xsl:value-of select="." /></xsl:with-param>
					</xsl:call-template>
				</xsl:for-each>
				<xsl:for-each select="cyberwares/cyberware/children/cyberware/gears/gear[iscommlink = 'True' or isnexus = 'True']">
					<xsl:call-template name="commlink">
						<xsl:with-param name="rendercommlink"><xsl:value-of select="." /></xsl:with-param>
					</xsl:call-template>
				</xsl:for-each>
				<xsl:for-each select="weapons/weapon/accessories/accessory/gears/gear[iscommlink = 'True' or isnexus = 'True']">
					<xsl:call-template name="commlink">
						<xsl:with-param name="rendercommlink"><xsl:value-of select="." /></xsl:with-param>
					</xsl:call-template>
				</xsl:for-each>
			</body>
		</html>
	</xsl:template>
	
	<xsl:template name="commlink">
		<xsl:param name="rendercommlink" />
					<div class="block" id="PersonalDataBlock">
						<table width="650px" cellspacing="0" cellpadding="0" border="0">
							<tr>
								<td width="100%" class="tableborder" colspan="4">
									<strong>
									<xsl:value-of select="name" />
									<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
									</strong>
								</td>
							</tr>
							<tr>
								<td width="25%" class="tableborder">
									<strong>Verbundene SIN</strong>
								</td>
								<td width="75%" class="tableborder" colspan="3">
									<xsl:value-of select="children/gear[issin = 'True']/extra" />&#160;
								</td>
							</tr>
							<tr>
								<td width="25%" class="tableborder">
									<strong>Model</strong>
								</td>
								<td width="75%" class="tableborder" colspan="3">
									<xsl:value-of select="name" />
									<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>&#160;
								</td>
							</tr>
							<tr>
								<td width="25%" class="tableborder">
									<strong>BS</strong>
								</td>
								<td width="75%" class="tableborder" colspan="3">
									<xsl:value-of select="children/gear[isos = 'True']/name" />&#160;
								</td>
							</tr>
							<tr>
								<td width="25%" class="tableborder">
									<strong>Prozessor</strong>
								</td>
								<td width="25%" class="tableborder">
									<xsl:choose>
										<xsl:when test="isnexus = 'True'">
											<xsl:value-of select="children/gear[contains(name, 'Response')]/name" />
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="response" />
										</xsl:otherwise>
									</xsl:choose>
								</td>
								<td width="50%" colspan="2" rowspan="6" valign="top" class="tableborder">
									<xsl:choose>
										<xsl:when test="ispersona = 'True'">
											<table width="100%">
												<tr>
													<td width="50%" style="text-align:center;">
														<strong>KÖRPERLICHE SCHADENSLEISTE</strong>
													</td>
													<td width="50%" style="text-align:center;">
														<strong>GEISTIGE SCHADENSLEISTE</strong>
													</td>
												</tr>
												<tr>
													<td width="50%" valign="top">
														<xsl:call-template name="ConditionMonitor">
															<xsl:with-param name="PenaltyBox"><xsl:value-of select="../../cmthreshold" /></xsl:with-param>
															<xsl:with-param name="Offset"><xsl:value-of select="../../cmthresholdoffset" /></xsl:with-param>
															<xsl:with-param name="CMWidth">3</xsl:with-param>
															<xsl:with-param name="TotalBoxes"><xsl:value-of select="../../physicalcm" /></xsl:with-param>
															<xsl:with-param name="DamageTaken"><xsl:value-of select="../../physicalcmfilled" /></xsl:with-param>
															<xsl:with-param name="OverFlow"><xsl:value-of select="../../cmoverflow" /></xsl:with-param>
														</xsl:call-template>
													</td>
													<td widht="50%" valign="top">
														<xsl:call-template name="ConditionMonitor">
															<xsl:with-param name="PenaltyBox"><xsl:value-of select="../../cmthreshold" /></xsl:with-param>
															<xsl:with-param name="Offset"><xsl:value-of select="../../cmthresholdoffset" /></xsl:with-param>
															<xsl:with-param name="CMWidth">3</xsl:with-param>
															<xsl:with-param name="TotalBoxes"><xsl:value-of select="../../stuncm" /></xsl:with-param>
															<xsl:with-param name="DamageTaken"><xsl:value-of select="../../stuncmfilled" /></xsl:with-param>
															<xsl:with-param name="OverFlow">0</xsl:with-param>
														</xsl:call-template>
													</td>
												</tr>
											</table>
										</xsl:when>
										<xsl:otherwise>
											<strong>Zustandsmonitor</strong>
											<xsl:call-template name="ConditionMonitor">
												<xsl:with-param name="PenaltyBox">3</xsl:with-param>
												<xsl:with-param name="Offset">0</xsl:with-param>
												<xsl:with-param name="CMWidth">3</xsl:with-param>
												<xsl:with-param name="TotalBoxes"><xsl:value-of select="conditionmonitor" /></xsl:with-param>
												<xsl:with-param name="DamageTaken">0</xsl:with-param>
												<xsl:with-param name="OverFlow">0</xsl:with-param>
											</xsl:call-template>
										</xsl:otherwise>
									</xsl:choose>
								</td>
							</tr>
							<tr>
								<td width="25%" class="tableborder">
									<strong>System</strong>
								</td>
								<td width="25%" class="tableborder">
									<xsl:choose>
										<xsl:when test="isnexus = 'True'">
											<xsl:value-of select="children/gear[contains(name, 'System')]/name" />
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="system" />
										</xsl:otherwise>
									</xsl:choose>
								</td>
							</tr>
							<tr>
								<td width="25%" class="tableborder">
									<strong>Signal</strong>
								</td>
								<td width="25%" class="tableborder">
									<xsl:choose>
										<xsl:when test="isnexus = 'True'">
											<xsl:value-of select="children/gear[contains(name, 'Signal')]/name" />
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="signal" />
										</xsl:otherwise>
									</xsl:choose>
								</td>
							</tr>
							<tr>
								<td width="25%" class="tableborder">
									<strong>Firewall</strong>
								</td>
								<td width="25%" class="tableborder">
									<xsl:choose>
										<xsl:when test="isnexus = 'True'">
											<xsl:value-of select="children/gear[contains(name, 'Firewall')]/name" />
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="firewall" />
										</xsl:otherwise>
									</xsl:choose>
								</td>
							</tr>
							<tr>
								<td width="25%" class="tableborder">
									<xsl:choose>
										<xsl:when test="isnexus = 'True'">
											<strong>Personalimit</strong>
										</xsl:when>
										<xsl:otherwise>
											<strong>Prozessorlimit.</strong>
										</xsl:otherwise>
									</xsl:choose>
								</td>
								<td width="25%" class="tableborder">
									<xsl:choose>
										<xsl:when test="isnexus = 'True'">
											<xsl:value-of select="children/gear[contains(name, 'Persona Limit')]/name" />
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="processorlimit" />
										</xsl:otherwise>
									</xsl:choose>
								</td>
							</tr>
							<tr>
								<td width="50%" class="tableborder" colspan="2">
									<strong>Programme/Daten</strong>
								</td>
							</tr>
							<tr>
								<td colspan="4" class="tableborder">
									<xsl:for-each select="children/gear[isprogram = 'True']">
										<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
										<xsl:if test="children/gear">
											[<xsl:for-each select="children/gear">
												<xsl:sort select="name" />
												<xsl:value-of select="name" />
												<xsl:if test="rating != 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
												<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
												<xsl:if test="position() != last()">; </xsl:if>
											</xsl:for-each>]
										</xsl:if>
										<xsl:if test="position() != last()">; </xsl:if>
									</xsl:for-each>
									<xsl:if test="ispersona = 'True'">
										<xsl:for-each select="../../techprograms/techprogram">
											<xsl:sort select="name" />
											<xsl:value-of select="name" />
											<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
											<xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
											<xsl:if test="programoptions/programoption">
												[<xsl:for-each select="programoptions/programoption">
													<xsl:sort select="name" />
													<xsl:value-of select="name" />
													<xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
													<xsl:if test="position() != last()">; </xsl:if>
												</xsl:for-each>]
											</xsl:if>
											<xsl:if test="position() != last()">; </xsl:if>
										</xsl:for-each>
									</xsl:if>
									&#160;
								</td>
							</tr>
							<xsl:if test="notes != ''">
							<tr>
								<td colspan="4" class="tableborder">
									<xsl:value-of select="notes" />
								</td>
							</tr>
							</xsl:if>
						</table>
					</div>
					<br />
	</xsl:template>
</xsl:stylesheet>
