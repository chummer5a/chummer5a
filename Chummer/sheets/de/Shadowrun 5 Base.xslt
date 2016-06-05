<?xml version="1.0" encoding="UTF-8" ?>
<!-- Character sheet based on the Shadowrun 5th Edition Character Sheet -->
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
					.notesrow
					{
						padding-left: 10px;
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
				<script type="text/javascript">
					<xsl:text>
						function zalomit(what,idx)
						{
							var elem = document.getElementById(idx); 
							if (elem.style.pageBreakAfter == 'always') {
								txt = "Nein";
								elem.style.pageBreakAfter = 'auto';
							}
							else {
								txt = "Ja";
								elem.style.pageBreakAfter = 'always';
							}
							what.innerHTML = "Seitenumbruch: " + txt;
						}
						function showhide(what,idx)
						{
							var elem = document.getElementById(idx); 
							if (elem.className != 'sectionhide') {
								txt = "Nein";
								elem.className = 'sectionhide';
							}
							else {
								txt = "Ja";
								elem.className = 'block';
							}
							what.innerHTML = "Zeigen: " + txt;
						}
					</xsl:text>
				</script>
			</head>
			<body>
				<div class="block" id="PersonalDataBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td width="100%" class="tableborder">
							<table width="100%" cellspacing="0" cellpadding="2" border="0">
								<tr>
									<td colspan="4">
										<strong>Name/Alias:</strong><xsl:text> </xsl:text><xsl:value-of select="name" />
										<xsl:if test="alias != ''"> (<xsl:value-of select="alias" />)</xsl:if>
									</td>
									<td colspan="4">
										<strong>Spieler:</strong><xsl:text> </xsl:text><xsl:value-of select="playername" />
									</td>
									<td colspan="2">
										Bewegung: <xsl:value-of select="movement" />
									</td>
								</tr>
								<tr>
									<td colspan="4">
										Metatyp:
										<xsl:value-of select="metatype" />
										<xsl:if test="metavariant != ''">
											(<xsl:value-of select="metavariant" />)
										</xsl:if>
									</td>
									<td width="10%">Alter:</td>
									<td width="10%">
										<xsl:value-of select="age" />
									</td>
									<td width="10%">Geschlecht:</td>
									<td width="10%">
										<xsl:value-of select="sex" />
									</td>
									<td width="10%">Nuyen:</td>
									<td width="10%">
										<xsl:value-of select="nuyen" />
									</td>
								</tr>
								<tr>
									<td width="10%">Größe:</td>
									<td width="10%">
										<xsl:value-of select="height" />
									</td>
									<td width="10%">Gewicht:</td>
									<td width="10%">
										<xsl:value-of select="weight" />
									</td>
									<td width="10%">Haarfarbe:</td>
									<td width="10%">
										<xsl:value-of select="hair" />
									</td>
									<td width="10%">Augenfarbe:</td>
									<td width="10%">
										<xsl:value-of select="eyes" />
									</td>
									<td width="10%">Hautfarbe:</td>
									<td width="10%">
										<xsl:value-of select="skin" />
									</td>
								</tr>
								<tr>
									<td width="10%">Karma:</td>
									<td width="10%">
										<xsl:value-of select="karma" />
									</td>
									<td width="10%">Ges. Karma:</td>
									<td width="10%">
										<xsl:value-of select="totalkarma" />
									</td>
									<td width="10%">Straßenruf:</td>
									<td width="10%">
										<xsl:value-of select="totalstreetcred" />
									</td>
									<td width="10%">Schlechter Ruf:</td>
									<td width="10%">
										<xsl:value-of select="totalnotoriety" />
									</td>
									<td width="10%">Prominenz:</td>
									<td width="10%">
										<xsl:value-of select="totalpublicawareness" />
									</td>
								</tr>
								<tr>
									<td width="10%">Selbstbeherrsch.:</td>
									<td width="10%">
										<xsl:value-of select="composure" />
									</td>
									<td width="10%">Menschenkenntnis:</td>
									<td width="10%">
										<xsl:value-of select="judgeintentions" />
									</td>
									<td width="10%">Heben/Tragen:</td>
									<td width="10%">
										<xsl:value-of select="liftandcarry" />
									</td>
									<td width="10%">Heben/Tragen Gewicht:</td>
									<td width="10%">
										<xsl:value-of select="liftweight" /> kg / <xsl:value-of select="carryweight" /> kg
									</td>
									<td width="10%">Erinnern:</td>
									<td width="10%">
										<xsl:value-of select="memory" />
									</td>
								</tr>
								<tr>
									<td class="rowsummary" colspan="10">
										Charakterdaten
										<span class="rowsummarybutton" onClick="showhide(this,'PersonalDataBlock');" colspan="2">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'PersonalDataBlock');" colspan="2">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
				
				<div class="block" id="AttributesBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td width="100%" class="tableborder">
							<table width="100%" cellspacing="0" cellpadding="0" border="0">
								<tr>
									<td width="25%" style="text-align:center;" class="attributecell">
										<strong>Körperliche Attribute</strong>
									</td>
									<td width="25%" style="text-align:center;" class="attributecell">
										<strong>Geistige Attribute</strong>
									</td>
									<td width="25%" style="text-align:center;" class="attributecell">
										<strong>Besondere Attribute</strong>
									</td>
									<td width="25%" style="text-align:center;" class="attributecell">
										<strong>Initiative</strong>
									</td>
								</tr>
								<tr>
									<td width="25%" class="attributecell">
										<p>
											Konstitution: <xsl:value-of select="attributes/attribute[name = 'BOD']/base" />
											<xsl:if test="attributes/attribute[name = 'BOD']/total != attributes/attribute[name = 'BOD']/base">
												(<xsl:value-of select="attributes/attribute[name = 'BOD']/total" />)
											</xsl:if>
										</p>
									</td>
									<td width="25%" class="attributecell">
										<p>
											Charisma: <xsl:value-of select="attributes/attribute[name = 'CHA']/base" />
											<xsl:if test="attributes/attribute[name = 'CHA']/total != attributes/attribute[name = 'CHA']/base">
												(<xsl:value-of select="attributes/attribute[name = 'CHA']/total" />)
											</xsl:if>
										</p>
									</td>
									<td width="25%" class="attributecell">
										<p>
											Edge: <xsl:value-of select="attributes/attribute[name = 'EDG']/base" />
											<xsl:if test="attributes/attribute[name = 'EDG']/total != attributes/attribute[name = 'EDG']/base">
												(<xsl:value-of select="attributes/attribute[name = 'EDG']/total" />)
											</xsl:if>
										</p>
									</td>
									<td width="25%" class="attributecell">
										<p>Initiative: <xsl:value-of select="init"/></p>
									</td>
								</tr>
								<tr>
									<td width="25%" class="attributecell">
										<p>
											Geschicklichkeit: <xsl:value-of select="attributes/attribute[name = 'AGI']/base" />
											<xsl:if test="attributes/attribute[name = 'AGI']/total != attributes/attribute[name = 'AGI']/base">
												(<xsl:value-of select="attributes/attribute[name = 'AGI']/total" />)
											</xsl:if>
										</p>
									</td>
									<td width="25%" class="attributecell">
										<p>
											Intuition: <xsl:value-of select="attributes/attribute[name = 'INT']/base" />
											<xsl:if test="attributes/attribute[name = 'INT']/total != attributes/attribute[name = 'INT']/base">
												(<xsl:value-of select="attributes/attribute[name = 'INT']/total" />)
											</xsl:if>
										</p>
									</td>
									<td width="25%" class="attributecell">
										<p>
											Aktuelles Edge:
										</p>
									</td>
									<td width="25%" class="attributecell">
										<p>
											<xsl:choose>
												<xsl:when test="magenabled = 'True'">
											Astrale Ini: <xsl:value-of select="astralinit" />
												</xsl:when>
												<xsl:otherwise>
											&#160;
												</xsl:otherwise>
											</xsl:choose>
										</p>
									</td>
								</tr>
								<tr>
									<td width="25%" class="attributecell">
										<p>
											Reaktion: <xsl:value-of select="attributes/attribute[name = 'REA']/base" />
											<xsl:if test="attributes/attribute[name = 'REA']/total != attributes/attribute[name = 'REA']/base">
												(<xsl:value-of select="attributes/attribute[name = 'REA']/total" />)
											</xsl:if>
										</p>
									</td>
									<td width="25%" class="attributecell">
										<p>
											Logik: <xsl:value-of select="attributes/attribute[name = 'LOG']/base" />
											<xsl:if test="attributes/attribute[name = 'LOG']/total != attributes/attribute[name = 'LOG']/base">
												(<xsl:value-of select="attributes/attribute[name = 'LOG']/total" />)
											</xsl:if>
										</p>
									</td>
									<td width="25%" class="attributecell">
										<p>
											Essenz: <xsl:value-of select="attributes/attribute[name = 'ESS']/base" />
										</p>
									</td>
									<td width="25%" class="attributecell">
										<p>
											Rigger AR: <xsl:value-of select="riggerinit" />
										</p>
									</td>
								</tr>
								<tr>
									<td width="25%" class="attributecell" valign="top">
										<p>
											Stärke: <xsl:value-of select="attributes/attribute[name = 'STR']/base" />
											<xsl:if test="attributes/attribute[name = 'STR']/total != attributes/attribute[name = 'STR']/base">
												(<xsl:value-of select="attributes/attribute[name = 'STR']/total" />)
											</xsl:if>
										</p>
									</td>
									<td width="25%" class="attributecell" valign="top">
										<p>
											Willenskraft: <xsl:value-of select="attributes/attribute[name = 'WIL']/base" />
											<xsl:if test="attributes/attribute[name = 'WIL']/total != attributes/attribute[name = 'WIL']/base">
												(<xsl:value-of select="attributes/attribute[name = 'WIL']/total" />)
											</xsl:if>
										</p>
									</td>
									<td width="25%" class="attributecell" valign="top">
										<p>
											<xsl:if test="magenabled = 'True'">
												Magie: <xsl:value-of select="attributes/attribute[name = 'MAG']/base" />
												<xsl:if test="attributes/attribute[name = 'MAG']/total != attributes/attribute[name = 'MAG']/base">
													(<xsl:value-of select="attributes/attribute[name = 'MAG']/total" />)
												</xsl:if>
											</xsl:if>
											<xsl:if test="resenabled = 'True'">
												Resonanz: <xsl:value-of select="attributes/attribute[name = 'RES']/base" />
												<xsl:if test="attributes/attribute[name = 'RES']/total != attributes/attribute[name = 'RES']/base">
													(<xsl:value-of select="attributes/attribute[name = 'RES']/total" />)
												</xsl:if>
											</xsl:if>
											&#160;
										</p>
									</td>
									<td width="25%" class="attributecell">
										<p>
											Matrix AR: <xsl:value-of select="matrixarinit"/><br />
											Matrix kalt: <xsl:value-of select="matrixcoldinit"/><br />
											Matrix heiß: <xsl:value-of select="matrixhotinit"/>
										</p>
									</td>
								</tr>
								<tr>
									<td class="rowsummary" colspan="4">
										Attribute
										<span class="rowsummarybutton" onClick="showhide(this,'AttributesBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'AttributesBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
				
				<div class="block" id="SkillsBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td width="100%" class="tableborder">
							<table width="100%" cellspacing="0" cellpadding="0" border="0">
								<tr>
									<td width="33%" style="text-align:center;" valign="top">
										<strong>Aktionsfertigkeiten</strong>
										<table width="100%" cellspacing="0" cellpadding="0" border="0">
											<tr>
												<td width="50%">
													<strong>Fertigkeit</strong>
												</td>
												<td width="10%" style="text-align:center;">
													<strong>Pool</strong>
												</td>
												<td width="10%" style="text-align:center;">
													<strong>W.</strong>
												</td>
												<td width="20%" style="text-align:center;">
													<strong>Att.</strong>
												</td>
												<td width="10%" style="text-align:center;">
													<strong>Mod.</strong>
												</td>
											</tr>
											<xsl:call-template name="skills1" />
										</table>
									</td>
									<td width="33%" style="border-left: solid 1px #1c4a2d; border-right: solid 1px #1c4a2d; text-align:center;" valign="top">
										<strong>Aktionsfertigkeiten</strong>
										<table width="100%" cellspacing="0" cellpadding="0" border="0">
											<tr>
												<td width="50%">
													<strong>Fertigkeit</strong>
												</td>
												<td width="10%" style="text-align:center;">
													<strong>Pool</strong>
												</td>
												<td width="10%" style="text-align:center;">
													<strong>W.</strong>
												</td>
												<td width="20%" style="text-align:center;">
													<strong>Att.</strong>
												</td>
												<td width="10%" style="text-align:center;">
													<strong>Mod.</strong>
												</td>
											</tr>
											<xsl:call-template name="skills2" />
										</table>
									</td>
									<td width="33%" style="text-align:center;" valign="top">
										<strong>Wissensfertigkeiten</strong>
										<table width="100%" cellspacing="0" cellpadding="0" border="0">
											<tr>
												<td width="50%">
													<strong>Fertigkeit</strong>
												</td>
												<td width="10%" style="text-align:center;">
													<strong>Pool</strong>
												</td>
												<td width="10%" style="text-align:center;">
													<strong>W.</strong>
												</td>
												<td width="20%" style="text-align:center;">
													<strong>Att.</strong>
												</td>
												<td width="10%" style="text-align:center;">
													<strong>Mod.</strong>
												</td>
											</tr>
											<xsl:call-template name="skills3" />
										</table>
									</td>
								</tr>
								<tr>
									<td class="rowsummary" colspan="3">
										Fertigkeiten
										<span class="rowsummarybutton" onClick="showhide(this,'SkillsBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'SkillsBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
				
				<div class="block" id="LimitsBlock">
					<table width="100%" cellspacing="0" cellpadding="0" class="tableborder">
						<tr>
							<td width="100%">
								<table width="100%" cellspacing="0" cellpadding="0" border="0">
									<tr>
										<td width="25%" style="text-align:center;" valign="top">
											<strong>Körperliches Limit: <xsl:value-of
												select="limitphysical"/></strong>
										</td>
										<td width="25%"
											style="border-left: solid 1px #1c4a2d; border-right: solid 1px #1c4a2d; text-align:center;"
											valign="top">
											<strong>Geistiges Limit: <xsl:value-of select="limitmental"
												/></strong>
										</td>
										<td width="25%" style="border-right: solid 1px #1c4a2d; text-align:center;" valign="top">
											<strong>Soziales Limit: <xsl:value-of select="limitsocial"
												/></strong>
										</td>
										<td width="25%" style="text-align:center;" valign="top">
											<strong>Astrales Limit: <xsl:value-of select="limitastral"
											/></strong>
										</td>
									</tr>
									<tr>
										<td width="25%" style="text-align:left;" valign="top">
											<table>
												<xsl:call-template name="limitmodifiersphys"/>
												<tr>
												<td/>
												</tr>
											</table>
										</td>
										<td width="25"
											style="border-left: solid 1px #1c4a2d; border-right: solid 1px #1c4a2d; text-align:left;"
											valign="top">
											<table>
												<xsl:call-template name="limitmodifiersment"/>
												<tr>
												<td/>
												</tr>
											</table>
										</td>
										<td width="25%" style="border-right: solid 1px #1c4a2d; text-align:left;" valign="top">
											<table>
												<xsl:call-template name="limitmodifierssoc"/>
												<tr>
												<td/>
												</tr>
											</table>
										</td>
										<td width="25%" style="text-align:left;" valign="top">
											<table>
												<xsl:call-template name="limitmodifiersast"/>
												<tr>
													<td/>
												</tr>
											</table>
										</td>
									</tr>
								</table>
								<table width="100%" cellspacing="0" cellpadding="0" border="0">
									<tr>
										<td class="rowsummary" colspan="3"> Limits
										<span class="rowsummarybutton" onClick="showhide(this,'LimitsBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'LimitsBlock');" colspan="1">Seitenumbruch: Nein</span>
										</td>
									</tr>
								</table>
							</td>
						</tr>
						<tr>
							<td class="hseparator"/>
						</tr>
					</table>
				</div>
				
				<div class="block" id="ContactConditionBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td width="100%">
							<table width="100%" cellspacing="0" cellpadding="0" border="0">
								<tr>
									<td width="67%" style="text-align:center;" valign="top">
										<table width="100%" cellspacing="0" cellpadding="0" border="0">
											<tr>
												<td>
													<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
														<tr>
															<td width="20%">
																<strong>Kontaktname</strong>
															</td>
															<td width="20%" style="text-align:center;">
																<strong>Ort</strong>
															</td>
															<td width="20%" style="text-align:center;">
																<strong>Archetyp</strong>
															</td>
															<td width="10%" style="text-align:center;">
																<strong>Typ</strong>
															</td>
															<td width="15%" style="text-align:center;">
																<strong>Einfluss</strong>
															</td>
															<td width="15%" style="text-align:center;">
																<strong>Loyalität</strong>
															</td>
														</tr>
														<xsl:call-template name="contacts" />
														<tr>
															<td class="rowsummary" colspan="6">
																Connections &amp; Feinde
															</td>
														</tr>
													</table>
												</td>
											</tr>
											<tr>
												<td class="hseparator"></td>
											</tr>
											<tr>
												<td>
													<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
														<tr>
															<td width="80%">
																<strong>Gaben/Handicaps</strong>
															</td>
															<td width="20%">
															</td>
														</tr>
														<xsl:call-template name="qualities" />
														<tr>
															<td class="rowsummary" colspan="2">
																Gaben/Handicaps
															</td>
														</tr>
													</table>
												</td>
											</tr>
											<tr>
												<td class="hseparator"></td>
											</tr>
											<tr>
												<td>
													<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
														<tr>
															<td width="50%">
																<strong>Panzerung</strong>
															</td>
															<td width="30%" style="text-align:center;">
																<strong>Panzerungswert</strong>
															</td>
															<td width="20%" style="text-align:center;">
															</td>
														</tr>
														<tr>
															<td width="50%">
																<strong>Gesamt</strong>
															</td>
															<td width="30%" style="text-align:center;">
																<xsl:value-of select="armor"/>
															</td>
															<td width="20%" style="text-align:center;">
															</td>
														</tr>
														<xsl:call-template name="armor" />
														<tr>
															<td class="rowsummary" colspan="3">
																Panzerung
																<span class="rowsummarybutton" onClick="showhide(this,'ContactConditionBlock');" colspan="1">Zeigen: Ja</span>
																<span class="rowsummarybutton" onClick="zalomit(this,'ContactConditionBlock');" colspan="1">Seitenumbruch: Nein</span>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</table>
									</td>
									<td class="vseparator">&#160;&#160;</td>
									<td width="33%" style="text-align:center;" valign="top">
										<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
											<tr>
												<td width="50%" style="text-align:center;">
													<strong>Körperliche Schadensleiste</strong>
												</td>
												<td width="50%" style="text-align:center;">
													<strong>Geistige Schadensleiste</strong>
												</td>
											</tr>
											<tr>
												<td width="50%" valign="top">
      <xsl:call-template name="ConditionMonitor">
        <xsl:with-param name="PenaltyBox"><xsl:value-of select="cmthreshold" /></xsl:with-param>
		<xsl:with-param name="Offset"><xsl:value-of select="cmthresholdoffset" /></xsl:with-param>
        <xsl:with-param name="CMWidth">3</xsl:with-param>
        <xsl:with-param name="TotalBoxes"><xsl:value-of select="physicalcm" /></xsl:with-param>
        <xsl:with-param name="DamageTaken"><xsl:value-of select="physicalcmfilled" /></xsl:with-param>
        <xsl:with-param name="OverFlow"><xsl:value-of select="cmoverflow" /></xsl:with-param>
      </xsl:call-template>
												</td>
												<td width="50%" valign="top">
      <xsl:call-template name="ConditionMonitor">
        <xsl:with-param name="PenaltyBox"><xsl:value-of select="cmthreshold" /></xsl:with-param>
		<xsl:with-param name="Offset"><xsl:value-of select="cmthresholdoffset" /></xsl:with-param>
        <xsl:with-param name="CMWidth">3</xsl:with-param>
        <xsl:with-param name="TotalBoxes"><xsl:value-of select="stuncm" /></xsl:with-param>
        <xsl:with-param name="DamageTaken"><xsl:value-of select="stuncmfilled" /></xsl:with-param>
        <xsl:with-param name="OverFlow">0</xsl:with-param>
      </xsl:call-template>
												</td>
											</tr>
											<tr>
												<td class="rowsummary" colspan="2">
													Zustandsmonitor
												</td>
											</tr>
										</table>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
				
				<div class="block" id="WeaponBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td>
							<table width="100%" cellspacing="0" cellpadding="0" border="0">
								<tr>
									<td width="100%" valign="top">
										<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
											<tr>
												<td width="20%">
													<strong>Waffe</strong>
												</td>
												<td width="8%" style="text-align:center;">
													<strong>Pool</strong>
												</td>
												<td width="11%" style="text-align:center;">
													<strong>Präzision</strong>
												</td>
												<td width="13%" style="text-align:center;">
													<strong>Schaden</strong>
												</td>
												<td width="8%" style="text-align:center;">
													<strong>DK</strong>
												</td>
												<td width="9%" style="text-align:center;">
													<strong>Modus</strong>
												</td>
												<td width="9%" style="text-align:center;">
													<strong>RK</strong>
												</td>
												<td width="11%" style="text-align:center;">
													<strong>Munition</strong>
												</td>
												<td width="11%" style="text-align:center;">
												</td>
											</tr>
											<xsl:for-each select="weapons/weapon[type = 'Ranged']">
												<xsl:sort select="name" />
												<xsl:call-template name="rangedweapons">
													<xsl:with-param name="weapon" select="weapons/weapon" />
												</xsl:call-template>
											</xsl:for-each>
											<tr>
												<td class="rowsummary" colspan="9">
													Fernkampfwaffen
													<span class="rowsummarybutton" onClick="showhide(this,'WeaponBlock');" colspan="2">Zeigen: Ja</span>
													<span class="rowsummarybutton" onClick="zalomit(this,'WeaponBlock');" colspan="2">Seitenumbruch: Nein</span>
												</td>
											</tr>
										</table>
									</td>
								</tr>
							</table>
							<table width="100%" cellspacing="0" cellpadding="0" border="0">
								<tr>
									<td width="100%" valign="top">
										<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
											<tr>
												<td width="20%">
													<strong>Waffe</strong>
												</td>
												<td width="8%" style="text-align:center;">
													<strong>Pool</strong>
												</td>
												<td width="11%" style="text-align:center;">
													<strong>Präzision</strong>
												</td>
												<td width="13%" style="text-align:center;">
													<strong>Schaden</strong>
												</td>
												<td width="8%" style="text-align:center;">
													<strong>DK</strong>
												</td>
												<td width="9%" style="text-align:center;">
													<strong>Reichweite</strong>
												</td>
												<td width="20%" style="text-align:center;">
												</td>
												<td width="11%" style="text-align:center;">
												</td>
											</tr>
											<xsl:for-each select="weapons/weapon[type = 'Melee']">
												<xsl:sort select="name" />
												<xsl:call-template name="meleeweapons">
													<xsl:with-param name="weapon" select="weapons/weapon" />
												</xsl:call-template>
											</xsl:for-each>
											<tr>
												<td class="rowsummary" colspan="8">
													Nahkampfwaffen
												</td>
											</tr>
										</table>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					
					<xsl:if test="cyberwares/cyberware">
				<div class="block" id="CyberwareBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td>
							<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
								<tr>
									<td width="50%">
										<strong>Implantat</strong>
									</td>
									<td width="20%" style="text-align:center;">
										<strong>Essenz</strong>
									</td>
									<td width="20%" style="text-align:center;">
										<strong>Stufe</strong>
									</td>
									<td width="10%" style="text-align:center;">
									</td>
								</tr>
								<xsl:call-template name="cyberware" />
								<tr>
									<td class="rowsummary" colspan="4">
										Cyberware/Bioware
										<span class="rowsummarybutton" onClick="showhide(this,'CyberwareBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'CyberwareBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>
					
					<xsl:for-each select="vehicles/vehicle">
						<xsl:sort select="name" />
						<xsl:call-template name="vehicles">
							<xsl:with-param name="vehicle" />
							<xsl:with-param name="VehicleNumber">Fahrzeugblock<xsl:value-of select="position()"/></xsl:with-param>
						</xsl:call-template>
					</xsl:for-each>
					
					<xsl:if test="spells/spell">
				<div class="block" id="SpellBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td>
							<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
								<tr>
									<td width="25%">
										<strong>Zauber</strong>
									</td>
									<td width="13%" style="text-align:center;">
										<strong>Typ</strong>
									</td>
									<td width="13%" style="text-align:center;">
										<strong>Reichweite</strong>
									</td>
									<td width="13%" style="text-align:center;">
										<strong>Schaden</strong>
									</td>
									<td width="13%" style="text-align:center;">
										<strong>Dauer</strong>
									</td>
									<td width="13%" style="text-align:center;">
										<strong>Entzug</strong>
									</td>
									<td width="10%" style="text-align:center;">
									</td>
								</tr>
								<xsl:call-template name="spells" />
								<tr>
									<td class="rowsummary" colspan="7">
										Zaubersprüche
										<span class="rowsummarybutton" onClick="showhide(this,'SpellBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'SpellBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>
					
					<xsl:if test="initiategrade > 0 or submersiongrade > 0">
				<div class="block" id="InitiationBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td>
							<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
								<tr>
									<td>
										<xsl:choose>
											<xsl:when test="submersiongrade > 0">
												<strong>Wandlungsgrad: </strong><xsl:value-of select="submersiongrade" />
											</xsl:when>
											<xsl:otherwise>
												<strong>Initiationsgrad: </strong><xsl:value-of select="initiategrade" />
											</xsl:otherwise>
										</xsl:choose>
									</td>
								</tr>
								<tr>
									<td>
										<xsl:choose>
											<xsl:when test="submersiongrade > 0">
												<strong>Echos</strong>
											</xsl:when>
											<xsl:otherwise>
												<strong>Metamagie</strong>
											</xsl:otherwise>
										</xsl:choose>
									</td>
								</tr>
								<tr>
									<td>
										<xsl:for-each select="metamagics/metamagic">
											<xsl:sort select="name" />
											<xsl:value-of select="name" /><xsl:text> </xsl:text>
											<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
											<xsl:if test="notes != ''">(<xsl:value-of select="notes" />)</xsl:if>
											<xsl:if test="position() != last()">; </xsl:if>
										</xsl:for-each>
									</td>
								</tr>
								<tr>
									<td class="rowsummary">
										<xsl:choose>
											<xsl:when test="submersiongrade > 0">
												Wandlung
											</xsl:when>
											<xsl:otherwise>
												Initiation
											</xsl:otherwise>
										</xsl:choose>
										<span class="rowsummarybutton" onClick="showhide(this,'InitiationBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'InitiationBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>

				<div class="block" id="ComlinkBlock">
					<table width="100%" cellspacing="0" cellpadding="0" border="0">
						<tr>
							<td>
								<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
									<tr>
										<td width="18%">
											<strong>Kommlink</strong>
										</td>
										<td width="16%" style="text-align:center;">
											<strong>Gerätestufe</strong>
										</td>
										<td width="14%" style="text-align:center;">
											<strong>Angriff</strong>
										</td>
										<td width="14%" style="text-align:center;">
											<strong>Schleicher</strong>
										</td>
										<td width="14%" style="text-align:center;">
											<strong>Datenverarb.</strong>
										</td>
										<td width="14%" style="text-align:center;">
											<strong>Firewall</strong>
										</td>
										<td width="10%" style="text-align:center;">
										</td>
									</tr>
									<xsl:call-template name="commlink" />
									<tr>
										<td class="rowsummary" colspan="7">
											Kommlink/Programme
											<span class="rowsummarybutton" onClick="showhide(this,'ComlinkBlock');" colspan="1">Zeigen: Ja</span>
											<span class="rowsummarybutton" onClick="zalomit(this,'ComlinkBlock');" colspan="1">Seitenumbruch: Nein</span>
										</td>
									</tr>
								</table>
							</td>
						</tr>
						<tr>
							<td class="hseparator"></td>
						</tr>
					</table>
				</div>
					
					<xsl:if test="spirits/spirit">
				<div class="block" id="SpiritBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td>
							<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
								<tr>
									<td width="25%">
										<xsl:choose>
											<xsl:when test="magenabled = 'True'">
												<strong>Geist</strong>
											</xsl:when>
											<xsl:otherwise>
												<strong>Sprite</strong>
											</xsl:otherwise>
										</xsl:choose>
									</td>
									<td width="25%" style="text-align:center;">
										<xsl:choose>
											<xsl:when test="magenabled = 'True'">
												<strong>Kraft</strong>
											</xsl:when>
											<xsl:otherwise>
												<strong>Stufe</strong>
											</xsl:otherwise>
										</xsl:choose>
									</td>
									<td width="25%" style="text-align:center;">
										<strong>Dienste</strong>
									</td>
									<td width="25%" style="text-align:center;">
										<xsl:choose>
											<xsl:when test="magenabled = 'True'">
												<strong>Gebunden/Ungebunden</strong>
											</xsl:when>
											<xsl:otherwise>
												<strong>Registriert/Unregistriert</strong>
											</xsl:otherwise>
										</xsl:choose>
									</td>
								</tr>
								<xsl:for-each select="spirits/spirit">
									<xsl:sort select="name" />
								<tr>
									<td width="25%">
										<xsl:value-of select="name" />
									</td>
									<td width="25%" style="text-align:center;">
										<xsl:value-of select="force" />
									</td>
									<td width="25%" style="text-align:center;">
										<xsl:value-of select="services" />
									</td>
									<td width="25%" style="text-align:center;">
										<xsl:choose>
											<xsl:when test="../../magenabled = 'True'">
												<xsl:choose>
													<xsl:when test="bound = 'True'">Gebunden</xsl:when>
													<xsl:otherwise>Ungebunden</xsl:otherwise>
												</xsl:choose>
											</xsl:when>
											<xsl:otherwise>
												<xsl:choose>
													<xsl:when test="bound = 'True'">Registriert</xsl:when>
													<xsl:otherwise>Unregistriert</xsl:otherwise>
												</xsl:choose>
											</xsl:otherwise>
										</xsl:choose>
									</td>
								</tr>
								<xsl:if test="notes != ''">
									<tr>
										<xsl:if test="position() mod 2 != 1">
											<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
										</xsl:if>
										<td colspan="4" class="notesrow">
											<xsl:call-template name="PreserveLineBreaks">
												<xsl:with-param name="text" select="notes" />
											</xsl:call-template>
										</td>
									</tr>
								</xsl:if>
								</xsl:for-each>
								<tr>
									<td class="rowsummary" colspan="4">
										<xsl:choose>
											<xsl:when test="magenabled = 'True'">
												<strong>Geister</strong>
											</xsl:when>
											<xsl:otherwise>
												<strong>Sprites</strong>
											</xsl:otherwise>
										</xsl:choose>
										<span class="rowsummarybutton" onClick="showhide(this,'SpiritBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'SpiritBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>
					
					<xsl:if test="powers/power">
				<div class="block" id="PowerBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td>
							<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
								<tr>
									<td width="40%">
										<strong>Bezeichnung</strong>
									</td>
									<td width="20%" style="text-align:center;">
										<strong>Stufe</strong>
									</td>
									<td width="20%" style="text-align:center;">
										<strong>Punkte (Summe)</strong>
									</td>
									<td width="20%" style="text-align:center;">
									</td>
								</tr>
								<xsl:call-template name="powers" />
								<tr>
									<td class="rowsummary" colspan="4">
										Adeptenkräfte
										<span class="rowsummarybutton" onClick="showhide(this,'PowerBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'PowerBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>
					
					<xsl:if test="critterpowers/critterpower">
				<div class="block" id="CritterBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td>
							<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
								<tr>
									<td width="50%">
										<strong>Bezeichnung</strong>
									</td>
									<td width="30%" style="text-align:center;">
										<strong>Stufe</strong>
									</td>
									<td width="20%" style="text-align:center;">
									</td>
								</tr>
								<xsl:call-template name="critterpowers" />
								<tr>
									<td class="rowsummary" colspan="3">
										Critterkräfte
										<span class="rowsummarybutton" onClick="showhide(this,'CritterBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'CritterBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>
					
					<xsl:if test="complexforms/complexform">
				<div class="block" id="ProgramBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td>
							<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
								<tr>
									<td width="30%">
										<strong>Bezeichnung</strong>
									</td>
									<td width="15%">
										<strong>Ziel</strong>
									</td>
									<td width="15%" style="text-align:center;">
										<strong>Dauer</strong>
									</td>
									<td width="15%" style="text-align:center;">
										<strong>Schwund</strong>
									</td>
									<td width="20%" style="text-align:center;">
									</td>
								</tr>
								<xsl:call-template name="complexforms" />
								<tr>
									<td class="rowsummary" colspan="5">
										Komplexe Formen
										<span class="rowsummarybutton" onClick="showhide(this,'ProgramBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'ProgramBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>
					
					<xsl:if test="martialarts/martialart">
				<div class="block" id="MartialArtsBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td>
							<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
								<tr>
									<td width="50%">
										<strong>Bezeichnung</strong>
									</td>
									<td width="30%" style="text-align:center;">
										<strong>Stufe</strong>
									</td>
									<td width="20%" style="text-align:center;">
									</td>
								</tr>
								<xsl:call-template name="martialarts" />
								<tr>
									<td class="rowsummary" colspan="3">
										Kampfkünste
										<span class="rowsummarybutton" onClick="showhide(this,'MartialArtsBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'MartialArtsBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>
					
					<xsl:if test="lifestyles != ''">
				<div class="block" id="LifestylesBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td width="100%" class="tableborder">
							<table width="100%" cellspacing="0" cellpadding="0" border="0">
								<tr>
									<td width="50%">
										<strong>Lebensstil</strong>
									</td>
									<td width="30%" style="text-align:center;">
										<strong>Monate</strong>
									</td>
									<td width="20%"></td>
								</tr>
								<xsl:for-each select="lifestyles/lifestyle">
									<xsl:sort select="name" />
								<tr>
									<td width="50%">
										<xsl:value-of select="name" />
										<xsl:if test="lifestylename != ''"> ("<xsl:value-of select="lifestylename" />")</xsl:if>
									</td>
									<td width="30%" style="text-align:center;">
										<xsl:value-of select="months" />
									</td>
									<td width="20%">
										<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
									</td>
								</tr>
								<xsl:if test="comforts != ''">
								<tr>
									<td colspan="3" class="indent">
										Komfort: <xsl:value-of select="comforts" />,
										Unterhaltung: <xsl:value-of select="entertainment" />,
										Grundversorgung: <xsl:value-of select="necessities" />,
										Wohngegend: <xsl:value-of select="neighborhood" />,
										Sicherheit: <xsl:value-of select="security" />
										<xsl:if test="qualities/quality">
											<br />Lebensstilgaben/-handicaps: 
											<xsl:for-each select="qualities/quality">
												<xsl:value-of select="." />
												<xsl:if test="position() != last()">; </xsl:if>
											</xsl:for-each>
										</xsl:if>
									</td>
								</tr>
								</xsl:if>
								<xsl:if test="notes != ''">
									<tr>
										<xsl:if test="position() mod 2 != 1">
											<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
										</xsl:if>
										<td colspan="3" class="notesrow">
											<xsl:call-template name="PreserveLineBreaks">
												<xsl:with-param name="text" select="notes" />
											</xsl:call-template>
										</td>
									</tr>
								</xsl:if>
								</xsl:for-each>
								<tr>
									<td class="rowsummary" colspan="3">
										Lebensstil
										<span class="rowsummarybutton" onClick="showhide(this,'LifestylesBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'LifestylesBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>
				
				<div class="block" id="GearBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td width="100%" class="tableborder">
							<table width="100%" cellspacing="0" cellpadding="0" border="0">
								<tr>
									<td width="33%" style="text-align:center;" valign="top">
										<table width="100%" cellspacing="0" cellpadding="0" border="0">
											<tr>
												<td width="55%">
													<strong>Bezeichnung</strong>
												</td>
												<td width="15%" style="text-align:center;">
													<strong>Stufe</strong>
												</td>
												<td width="15%" style="text-align:center;">
													<strong>Anz.</strong>
												</td>
												<td width="15%" style="text-align:center;">
												</td>
											</tr>
											<xsl:call-template name="gear1" />
										</table>
									</td>
									<td width="33%" style="border-left: solid 1px #1c4a2d; border-right: solid 1px #1c4a2d; text-align:center;" valign="top">
										<table width="100%" cellspacing="0" cellpadding="0" border="0">
											<tr>
												<td width="55%">
													<strong>Bezeichnung</strong>
												</td>
												<td width="15%" style="text-align:center;">
													<strong>Stufe</strong>
												</td>
												<td width="15%" style="text-align:center;">
													<strong>Anz.</strong>
												</td>
												<td width="15%" style="text-align:center;">
												</td>
											</tr>
											<xsl:call-template name="gear2" />
										</table>
									</td>
									<td width="33%" style="text-align:center;" valign="top">
										<table width="100%" cellspacing="0" cellpadding="0" border="0">
											<tr>
												<td width="55%">
													<strong>Bezeichnung</strong>
												</td>
												<td width="15%" style="text-align:center;">
													<strong>Stufe</strong>
												</td>
												<td width="15%" style="text-align:center;">
													<strong>Anz.</strong>
												</td>
												<td width="15%" style="text-align:center;">
												</td>
											</tr>
											<xsl:call-template name="gear3" />
										</table>
									</td>
								</tr>
								<tr>
									<td class="rowsummary" colspan="3">
										Ausrüstung
										<span class="rowsummarybutton" onClick="showhide(this,'GearBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'GearBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					
					<xsl:if test="description != ''">
				<div class="block" id="DescriptionBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td width="100%" class="tableborder">
							<table width="100%" cellspacing="0" cellpadding="0" border="0">
								<tr>
									<td>
										<xsl:call-template name="PreserveLineBreaks">
											<xsl:with-param name="text" select="description" />
										</xsl:call-template>
									</td>
								</tr>
								<tr>
									<td class="rowsummary">
										Beschreibung
										<span class="rowsummarybutton" onClick="showhide(this,'DescriptionBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'DescriptionBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>
					
					<xsl:if test="background != ''">
				<div class="block" id="BackgroundBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td width="100%" class="tableborder">
							<table width="100%" cellspacing="0" cellpadding="0" border="0">
								<tr>
									<td>
										<xsl:call-template name="PreserveLineBreaks">
											<xsl:with-param name="text" select="background" />
										</xsl:call-template>
									</td>
								</tr>
								<tr>
									<td class="rowsummary">
										Hintergrund
										<span class="rowsummarybutton" onClick="showhide(this,'BackgroundBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'BackgroundBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>
					
					<xsl:if test="concept != ''">
				<div class="block" id="ConceptBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td width="100%" class="tableborder">
							<table width="100%" cellspacing="0" cellpadding="0" border="0">
								<tr>
									<td>
										<xsl:call-template name="PreserveLineBreaks">
											<xsl:with-param name="text" select="concept" />
										</xsl:call-template>
									</td>
								</tr>
								<tr>
									<td class="rowsummary">
										Charakterkonzept
										<span class="rowsummarybutton" onClick="showhide(this,'ConceptBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'ConceptBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>
					
					<xsl:if test="notes != ''">
				<div class="block" id="NotesBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td width="100%" class="tableborder">
							<table width="100%" cellspacing="0" cellpadding="0" border="0">
								<tr>
									<td>
										<xsl:call-template name="PreserveLineBreaks">
											<xsl:with-param name="text" select="notes" />
										</xsl:call-template>
									</td>
								</tr>
								<tr>
									<td class="rowsummary">
										Notizen
										<span class="rowsummarybutton" onClick="showhide(this,'NotesBlock');" colspan="1">Zeigen: Ja</span>
										<span class="rowsummarybutton" onClick="zalomit(this,'NotesBlock');" colspan="1">Seitenumbruch: Nein</span>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>
					
					<xsl:if test="expenses">
				<div class="block" id="ExpensesBlock">
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td>
							<table width="100%" cellspacing="0" cellpadding="0" border="0">
								<tr>
									<td width="50%" valign="top">
										<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
											<tr>
												<td width="30%">
													<strong>Datum</strong>
												</td>
												<td width="20%" style="text-align:center;">
													<strong>Menge</strong>
												</td>
												<td width="50%" style="text-align:center;">
													<strong>Grund</strong>
												</td>
											</tr>
											<xsl:for-each select="expenses/expense[type = 'Karma']">
												<tr>
													<td>
														<xsl:value-of select="date" />
													</td>
													<td style="text-align:right;">
														<xsl:value-of select="amount" />&#160;&#160;&#160;
													</td>
													<td>
														<xsl:value-of select="reason" />
													</td>
												</tr>
											</xsl:for-each>
											<tr>
												<td class="rowsummary" colspan="3">
													Karma Ausgaben
													<span class="rowsummarybutton" onClick="showhide(this,'ExpensesBlock');" colspan="1">Zeigen: Ja</span>
													<span class="rowsummarybutton" onClick="zalomit(this,'ExpensesBlock');" colspan="1">Seitenumbruch: Nein</span>
												</td>
											</tr>
										</table>
									</td>
									<td class="vseparator">&#160;&#160;</td>
									<td width="50%" valign="top">
										<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
											<tr>
												<td width="30%">
													<strong>Datum</strong>
												</td>
												<td width="20%" style="text-align:center;">
													<strong>Menge</strong>
												</td>
												<td width="50%" style="text-align:center;">
													<strong>Grund</strong>
												</td>
											</tr>
											<xsl:for-each select="expenses/expense[type = 'Nuyen']">
												<tr>
													<td>
														<xsl:value-of select="date" />
													</td>
													<td style="text-align:right;">
														<xsl:value-of select="amount" />¥&#160;&#160;&#160;
													</td>
													<td>
														<xsl:value-of select="reason" />
													</td>
												</tr>
											</xsl:for-each>
											<tr>
												<td class="rowsummary" colspan="3">
													Nuyen Ausgaben
												</td>
											</tr>
										</table>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
					</xsl:if>
			</body>
		</html>
	</xsl:template>
	
	<xsl:template name="contacts">
		<xsl:for-each select="contacts/contact">
			<xsl:sort select="name" />
														<tr class="textrow" valign="top">
															<xsl:if test="position() mod 2 != 1">
																<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
															</xsl:if>
															<td width="20%">
																<xsl:value-of select="name" />
															</td>
															<td width="20%" style="text-align:center;" valign="top">
																<xsl:value-of select="location" />
															</td>
															<td width="20%" style="text-align:center;" valign="top">
																<xsl:value-of select="role" />
															</td>
															<td width="10%" style="text-align:center;" valign="top">
																<xsl:value-of select="type" />
															</td>
															<td width="15%" style="text-align:center;" valign="top">
																<xsl:value-of select="connection" />
															</td>
															<td width="15%" style="text-align:center;" valign="top">
																<xsl:value-of select="loyalty" />
															</td>
														</tr>
														<xsl:if test="notes != ''">
															<tr>
																<xsl:if test="position() mod 2 != 1">
																	<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
																</xsl:if>
																<td colspan="4" class="notesrow">
																	<xsl:call-template name="PreserveLineBreaks">
																		<xsl:with-param name="text" select="notes" />
																	</xsl:call-template>
																</td>
															</tr>
														</xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="qualities">
		<xsl:for-each select="qualities/quality">
			<xsl:sort select="name" />
														<tr>
															<xsl:if test="position() mod 2 != 1">
																<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
															</xsl:if>
															<td valign="top" width="80%">
																<xsl:value-of select="name" />
																<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
															</td>
															<td valign="top" width="20%">
																<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
															</td>
														</tr>
														<xsl:if test="notes != ''">
															<tr>
																<xsl:if test="position() mod 2 != 1">
																	<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
																</xsl:if>
																<td colspan="3" class="notesrow">
																	<xsl:call-template name="PreserveLineBreaks">
																		<xsl:with-param name="text" select="notes" />
																	</xsl:call-template>
																</td>
															</tr>
														</xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="limitmodifiersphys">
		<xsl:for-each select="limitmodifiersphys/limitmodifier">
			<xsl:sort select="name"/>
			<tr>
				<xsl:if test="position() mod 2 != 1">
					<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
				</xsl:if>
				<td valign="top" width="100%">
					<xsl:value-of select="name"/>
					<xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
				</td>
			</tr>
			<xsl:if test="notes != ''">
				<tr>
					<xsl:if test="position() mod 2 != 1">
						<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
					</xsl:if>
					<td colspan="1" class="notesrow">
						<xsl:call-template name="PreserveLineBreaks">
							<xsl:with-param name="text" select="notes"/>
						</xsl:call-template>
					</td>
				</tr>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="limitmodifiersment">
		<xsl:for-each select="limitmodifiersment/limitmodifier">
			<xsl:sort select="name"/>
			<tr>
				<xsl:if test="position() mod 2 != 1">
					<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
				</xsl:if>
				<td valign="top" width="100%">
					<xsl:value-of select="name"/>
					<xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
				</td>
			</tr>
			<xsl:if test="notes != ''">
				<tr>
					<xsl:if test="position() mod 2 != 1">
						<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
					</xsl:if>
					<td colspan="1" class="notesrow">
						<xsl:call-template name="PreserveLineBreaks">
							<xsl:with-param name="text" select="notes"/>
						</xsl:call-template>
					</td>
				</tr>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="limitmodifierssoc">
		<xsl:for-each select="limitmodifierssoc/limitmodifier">
			<xsl:sort select="name"/>
			<tr>
				<xsl:if test="position() mod 2 != 1">
					<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
				</xsl:if>
				<td valign="top" width="100%">
					<xsl:value-of select="name"/>
					<xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
				</td>
			</tr>
			<xsl:if test="notes != ''">
				<tr>
					<xsl:if test="position() mod 2 != 1">
						<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
					</xsl:if>
					<td colspan="1" class="notesrow">
						<xsl:call-template name="PreserveLineBreaks">
							<xsl:with-param name="text" select="notes"/>
						</xsl:call-template>
					</td>
				</tr>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="limitmodifiersast">
		<xsl:for-each select="limitmodifiersast/limitmodifier">
			<xsl:sort select="name"/>
			<tr>
				<xsl:if test="position() mod 2 != 1">
					<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
				</xsl:if>
				<td valign="top" width="100%">
					<xsl:value-of select="name"/>
					<xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
				</td>
			</tr>
			<xsl:if test="notes != ''">
				<tr>
					<xsl:if test="position() mod 2 != 1">
						<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
					</xsl:if>
					<td colspan="1" class="notesrow">
						<xsl:call-template name="PreserveLineBreaks">
							<xsl:with-param name="text" select="notes"/>
						</xsl:call-template>
					</td>
				</tr>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="rangedweapons">
		<xsl:param name="weapon" />
											<tr>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
												<td width="20%" valign="top">
													<xsl:value-of select="name" />
													<xsl:if test="weaponname != ''">
														("<xsl:value-of select="weaponname" />")
													</xsl:if>
												</td>
												<td width="8%" style="text-align:center;" valign="top">
													<xsl:value-of select="dicepool" />
												</td>
												<td width="11%" style="text-align:center;" valign="top">
													<xsl:value-of select="accuracy" />
												</td>
												<td width="13%" style="text-align:center;" valign="top">
													<xsl:value-of select="damage" />
												</td>
												<td width="8%" style="text-align:center;" valign="top">
													<xsl:value-of select="ap" />
												</td>
												<td width="8%" style="text-align:center;" valign="top">
													<xsl:value-of select="mode" />
												</td>
												<td width="8%" style="text-align:center;" valign="top">
													<xsl:value-of select="rc" />
												</td>
												<td width="11%" style="text-align:center;" valign="top">
													<xsl:value-of select="ammo" />
												</td>
												<td width="11%" style="text-align:center;" valign="top">
													<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
												</td>
											</tr>
				<xsl:if test="accessories/accessory or mods/weaponmod">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="9" class="indent">
					<xsl:for-each select="accessories/accessory">
						<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="position() != last()">; </xsl:if>
					</xsl:for-each>
					<xsl:if test="accessories/accessory and mods/weaponmod">; </xsl:if>
					<xsl:for-each select="mods/weaponmod">
						<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="rating > 0">
										Stufe <xsl:value-of select="rating" />
										</xsl:if>
										<xsl:if test="position() != last()">; </xsl:if>
					</xsl:for-each>
									</td>
								</tr>
				</xsl:if>
				<xsl:if test="ranges/short != ''">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td></td>
									<td style="text-align:center;" valign="top">S: <xsl:value-of select="ranges/short" /></td>
									<td style="text-align:center;" valign="top">M: <xsl:value-of select="ranges/medium" /></td>
									<td style="text-align:center;" valign="top">L: <xsl:value-of select="ranges/long" /></td>
									<td style="text-align:center;" valign="top">E: <xsl:value-of select="ranges/extreme" /></td>
									<td colspan="4"></td>
								</tr>
				</xsl:if>

				<xsl:if test="underbarrel/weapon">
					<xsl:for-each select="underbarrel/weapon">
											<tr>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
												<td width="20%" valign="top">
													Unterlauf <xsl:value-of select="name" />
													<xsl:if test="weaponname != ''">
														("<xsl:value-of select="weaponname" />")
													</xsl:if>
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:value-of select="damage" />
												</td>
												<td width="12%" style="text-align:center;" valign="top">
													<xsl:value-of select="ap" />
												</td>
												<td width="13%" style="text-align:center;" valign="top">
													<xsl:value-of select="mode" />
												</td>
												<td width="12%" style="text-align:center;" valign="top">
													<xsl:value-of select="rc" />
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:value-of select="ammo" />
												</td>
												<td width="13%" style="text-align:center;" valign="top">
													<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
												</td>
											</tr>
				<xsl:if test="accessories/accessory">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="9" class="indent">
					<xsl:for-each select="accessories/accessory">
						<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="position() != last()">; </xsl:if>
					</xsl:for-each>
					<xsl:if test="accessories/accessory and mods/weaponmod">; </xsl:if>
					<xsl:for-each select="mods/weaponmod">
						<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="rating > 0">
										Stufe <xsl:value-of select="rating" />
										</xsl:if>
										<xsl:if test="position() != last()">; </xsl:if>
					</xsl:for-each>
									</td>
								</tr>
				</xsl:if>
				<xsl:if test="ranges/short != ''">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td></td>
									<td style="text-align:center;" valign="top">S: <xsl:value-of select="ranges/short" /></td>
									<td style="text-align:center;" valign="top">M: <xsl:value-of select="ranges/medium" /></td>
									<td style="text-align:center;" valign="top">L: <xsl:value-of select="ranges/long" /></td>
									<td style="text-align:center;" valign="top">E: <xsl:value-of select="ranges/extreme" /></td>
									<td colspan="4"></td>
								</tr>
				</xsl:if>
					</xsl:for-each>
				</xsl:if>
				
				<xsl:if test="notes != ''">
					<tr>
						<xsl:if test="position() mod 2 != 1">
							<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
						</xsl:if>
						<td colspan="9" class="notesrow">
							<xsl:call-template name="PreserveLineBreaks">
								<xsl:with-param name="text" select="notes" />
							</xsl:call-template>
						</td>
					</tr>
				</xsl:if>
	</xsl:template>
	
	<xsl:template name="meleeweapons">
		<xsl:param name="weapon" />
											<tr>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
												<td width="20%" valign="top">
													<xsl:value-of select="name" />
													<xsl:if test="weaponname != ''">
														("<xsl:value-of select="weaponname" />")
													</xsl:if>
												</td>
												<td width="8%" style="text-align:center;" valign="top">
													<xsl:value-of select="dicepool" />
												</td>
												<td width="11%" style="text-align:center;" valign="top">
													<xsl:value-of select="accuracy" />
												</td>
												<td width="13%" style="text-align:center;" valign="top">
													<xsl:value-of select="damage" />
												</td>
												<td width="8%" style="text-align:center;" valign="top">
													<xsl:value-of select="ap" />
												</td>
												<td width="9%" style="text-align:center;" valign="top">
													<xsl:value-of select="reach" />
												</td>
												<td width="20%" style="text-align:center;" valign="top">
												</td>
												<td width="11%" style="text-align:center;" valign="top">
													<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
												</td>
											</tr>
				<xsl:if test="accessories/accessory or mods/weaponmod">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="5" class="indent">
					<xsl:for-each select="accessories/accessory">
						<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="position() != last()">; </xsl:if>
					</xsl:for-each>
					<xsl:if test="accessories/accessory and mods/weaponmod">; </xsl:if>
					<xsl:for-each select="mods/weaponmod">
						<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="rating > 0">
										Stufe <xsl:value-of select="rating" />
										</xsl:if>
										<xsl:if test="position() != last()">; </xsl:if>
					</xsl:for-each>
									</td>
								</tr>
				</xsl:if>
				
				<xsl:if test="notes != ''">
					<tr>
						<xsl:if test="position() mod 2 != 1">
							<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
						</xsl:if>
						<td colspan="5" class="notesrow">
							<xsl:call-template name="PreserveLineBreaks">
								<xsl:with-param name="text" select="notes" />
							</xsl:call-template>
						</td>
					</tr>
				</xsl:if>
	</xsl:template>
	
	<xsl:template name="armor">
		<xsl:for-each select="armors/armor">
			<xsl:sort select="name"/>
			<tr>
				<xsl:if test="position() mod 2 != 1">
					<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
				</xsl:if>
				<td width="50%" valign="top">
					<xsl:value-of select="name"/>
					<xsl:if test="armorname != ''"> ("<xsl:value-of select="armorname"/>") </xsl:if>
				</td>
				<td width="30%" style="text-align:center;" valign="top">
					<xsl:value-of select="armor"/>
				</td>
				<td width="20%" style="text-align:center;" valign="top">
					<xsl:value-of select="source"/>
					<xsl:text> </xsl:text>
					<xsl:value-of select="page"/>
				</td>
			</tr>
			<xsl:if test="armormods/armormod">
				<tr>
					<xsl:if test="position() mod 2 != 1">
						<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
					</xsl:if>
					<td colspan="3" class="indent">
						<xsl:for-each select="armormods/armormod">
							<xsl:sort select="name"/>
							<xsl:value-of select="name"/>
							<xsl:if test="rating != 0">
								<xsl:text> </xsl:text>
								<xsl:value-of select="rating"/>
							</xsl:if>
							<xsl:if test="position() != last()">; </xsl:if>
						</xsl:for-each>
					</td>
				</tr>
			</xsl:if>
				
			<xsl:if test="gears/gear">
				<tr>
					<xsl:if test="position() mod 2 != 1">
						<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
					</xsl:if>
					<td colspan="3" class="indent">
						<xsl:for-each select="gears/gear">
							<xsl:sort select="name" />
							<xsl:value-of select="name" />
							<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
							<xsl:if test="rating != 0"> Rating <xsl:value-of select="rating" /></xsl:if>
							<xsl:if test="qty &gt; 1"> x<xsl:value-of select="qty" /></xsl:if>
							<xsl:if test="children/gear">
								[<xsl:call-template name="gearplugin">
									<xsl:with-param name="gear" select="." />
								</xsl:call-template>]
							</xsl:if>
							<xsl:if test="position() != last()">; </xsl:if>
						</xsl:for-each>
					</td>
				</tr>
			</xsl:if>
				
			<xsl:if test="notes != ''">
				<tr>
					<xsl:if test="position() mod 2 != 1">
						<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
					</xsl:if>
					<td colspan="3" class="notesrow">
						<xsl:call-template name="PreserveLineBreaks">
							<xsl:with-param name="text" select="notes" />
						</xsl:call-template>
					</td>
				</tr>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="cyberware">
		<xsl:for-each select="cyberwares/cyberware">
			<xsl:sort select="name" />
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td width="50%" valign="top">
										<xsl:value-of select="name" />
										<xsl:if test="rating != 0"> Stufe <xsl:value-of select="rating" /></xsl:if>
										<xsl:if test="location != ''"> (<xsl:value-of select="location" />)</xsl:if>
									</td>
									<td width="20%" style="text-align:center;" valign="top">
										<xsl:value-of select="ess" />
									</td>
									<td width="20%" style="text-align:center;" valign="top">
										<xsl:value-of select="grade" />
									</td>
									<td width="20%" style="text-align:center;" valign="top">
										<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
									</td>
								</tr>
								<xsl:if test="notes != ''">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="4" class="notesrow">
										<xsl:call-template name="PreserveLineBreaks">
											<xsl:with-param name="text" select="notes" />
										</xsl:call-template>
									</td>
								</tr>
								</xsl:if>
				<xsl:if test="children/cyberware">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="4" class="indent">
					<xsl:for-each select="children/cyberware">
										<xsl:value-of select="name" />
										<xsl:if test="rating != 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
										<xsl:if test="position() != last()">; </xsl:if>
										<xsl:if test="notes != ''"> (<xsl:value-of select="notes" />)</xsl:if>
					</xsl:for-each>
									</td>
								</tr>
				</xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="commlink">
		<xsl:for-each select="gears/gear[iscommlink = 'True']">
			<tr>
				<xsl:if test="position() mod 2 != 1">
					<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
				</xsl:if>
				<td width="18%" valign="top">
					<xsl:value-of select="name"/> (<xsl:value-of select="category"/>)
					<xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
				</td>
				<td width="16%" style="text-align:center;" valign="top">
					<xsl:value-of select="devicerating"/>
				</td>
				<td width="14%" style="text-align:center;" valign="top">
					<xsl:value-of select="attack"/>
				</td>
				<td width="14%" style="text-align:center;" valign="top">
					<xsl:value-of select="sleaze"/>
				</td>
				<td width="14%" style="text-align:center;" valign="top">
					<xsl:value-of select="dataprocessing"/>
				</td>
				<td width="14%" style="text-align:center;" valign="top">
					<xsl:value-of select="firewall"/>
				</td>
				<td width="10%" style="text-align:center;" valign="top">
					<xsl:value-of select="source"/>
					<xsl:text> </xsl:text>
					<xsl:value-of select="page"/>
				</td>
			</tr>
			<tr>
				<xsl:if test="position() mod 2 != 1">
					<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
				</xsl:if>
				<td colspan="7" class="indent">
					<strong>Zubehör</strong>
				</td>
			</tr>
			<tr>
				<xsl:if test="position() mod 2 != 1">
					<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
				</xsl:if>
				<td colspan="7" class="indent">
					<xsl:for-each select="children/gear">
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
				</td>
			</tr>
			<xsl:if test="notes != ''">
				<tr>
					<xsl:if test="position() mod 2 != 1">
						<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
					</xsl:if>
					<td colspan="7" class="notesrow">
						<xsl:call-template name="PreserveLineBreaks">
							<xsl:with-param name="text" select="notes" />
						</xsl:call-template>
					</td>
				</tr>
			</xsl:if>
		</xsl:for-each>
		<xsl:for-each select="armors/armor/gears/gear[iscommlink = 'True']">
			<tr>
				<xsl:if test="position() mod 2 != 1">
					<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
				</xsl:if>
				<td width="18%" valign="top">
					<xsl:value-of select="name" />
					<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
				</td>
				<td width="18%" style="text-align:center;" valign="top">
					<xsl:value-of select="response" />
				</td>
				<td width="18%" style="text-align:center;" valign="top">
					<xsl:value-of select="system" />
				</td>
				<td width="18%" style="text-align:center;" valign="top">
					<xsl:value-of select="firewall" />
				</td>
				<td width="18%" style="text-align:center;" valign="top">
					<xsl:value-of select="signal" />
				</td>
				<td width="10%" style="text-align:center;" valign="top">
					<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
				</td>
			</tr>
			<tr>
				<xsl:if test="position() mod 2 != 1">
					<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
				</xsl:if>
				<td colspan="7" class="indent">
					<strong>Zubehör</strong>
				</td>
			</tr>
			<tr>
				<xsl:if test="position() mod 2 != 1">
					<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
				</xsl:if>
				<td colspan="7" class="indent">
					<xsl:for-each select="children/gear">
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
				</td>
			</tr>
			<xsl:if test="notes != ''">
				<tr>
					<xsl:if test="position() mod 2 != 1">
						<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
					</xsl:if>
					<td colspan="7" class="notesrow">
						<xsl:call-template name="PreserveLineBreaks">
							<xsl:with-param name="text" select="notes" />
						</xsl:call-template>
					</td>
				</tr>
			</xsl:if>
		</xsl:for-each>
		<xsl:for-each select="cyberwares/cyberware/gears/gear[iscommlink = 'True']">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td width="18%" valign="top">
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
									</td>
									<td width="18%" style="text-align:center;" valign="top">
										<xsl:value-of select="response" />
									</td>
									<td width="18%" style="text-align:center;" valign="top">
										<xsl:value-of select="system" />
									</td>
									<td width="18%" style="text-align:center;" valign="top">
										<xsl:value-of select="firewall" />
									</td>
									<td width="18%" style="text-align:center;" valign="top">
										<xsl:value-of select="signal" />
									</td>
									<td width="10%" style="text-align:center;" valign="top">
										<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
									</td>
								</tr>
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="7" class="indent">
										<strong>Zubehör</strong>
									</td>
								</tr>
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="7" class="indent">
			<xsl:for-each select="children/gear">
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
									</td>
								</tr>
								<xsl:if test="notes != ''">
									<tr>
										<xsl:if test="position() mod 2 != 1">
											<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
										</xsl:if>
										<td colspan="7" class="notesrow">
											<xsl:call-template name="PreserveLineBreaks">
												<xsl:with-param name="text" select="notes" />
											</xsl:call-template>
										</td>
									</tr>
								</xsl:if>
		</xsl:for-each>
		<xsl:for-each select="cyberwares/cyberware/children/cyberware/gears/gear[iscommlink = 'True']">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td width="18%" valign="top">
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
									</td>
									<td width="18%" style="text-align:center;" valign="top">
										<xsl:value-of select="response" />
									</td>
									<td width="18%" style="text-align:center;" valign="top">
										<xsl:value-of select="system" />
									</td>
									<td width="18%" style="text-align:center;" valign="top">
										<xsl:value-of select="firewall" />
									</td>
									<td width="18%" style="text-align:center;" valign="top">
										<xsl:value-of select="signal" />
									</td>
									<td width="10%" style="text-align:center;" valign="top">
										<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
									</td>
								</tr>
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="6" class="indent">
										<strong>Zubehör</strong>
									</td>
								</tr>
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="6" class="indent">
			<xsl:for-each select="children/gear">
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
									</td>
								</tr>
								<xsl:if test="notes != ''">
									<tr>
										<xsl:if test="position() mod 2 != 1">
											<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
										</xsl:if>
										<td colspan="6" class="notesrow">
											<xsl:call-template name="PreserveLineBreaks">
												<xsl:with-param name="text" select="notes" />
											</xsl:call-template>
										</td>
									</tr>
								</xsl:if>
		</xsl:for-each>
		<xsl:for-each select="weapons/weapon/accessories/accessory/gears/gear[iscommlink = 'True']">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td width="18%" valign="top">
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
									</td>
									<td width="18%" style="text-align:center;" valign="top">
										<xsl:value-of select="response" />
									</td>
									<td width="18%" style="text-align:center;" valign="top">
										<xsl:value-of select="system" />
									</td>
									<td width="18%" style="text-align:center;" valign="top">
										<xsl:value-of select="firewall" />
									</td>
									<td width="18%" style="text-align:center;" valign="top">
										<xsl:value-of select="signal" />
									</td>
									<td width="10%" style="text-align:center;" valign="top">
										<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
									</td>
								</tr>
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="6" class="indent">
										<strong>Zubehör</strong>
									</td>
								</tr>
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="6" class="indent">
			<xsl:for-each select="children/gear">
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
									</td>
								</tr>
								<xsl:if test="notes != ''">
									<tr>
										<xsl:if test="position() mod 2 != 1">
											<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
										</xsl:if>
										<td colspan="6" class="notesrow">
											<xsl:call-template name="PreserveLineBreaks">
												<xsl:with-param name="text" select="notes" />
											</xsl:call-template>
										</td>
									</tr>
								</xsl:if>
		</xsl:for-each>
		<xsl:if test="gears/gear[isprogram = 'True']">
								<tr>
									<td colspan="6">
									<strong>Programme</strong><br />
			<xsl:for-each select="gears/gear[isprogram = 'True']">
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
									</td>
								</tr>
		</xsl:if>
		<xsl:if test="armors/armor/gears/gear[isprogram = 'True']">
								<tr>
									<td colspan="6">
									<strong>Programme</strong><br />
			<xsl:for-each select="armors/armor/gears/gear[isprogram = 'True']">
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
									</td>
								</tr>
		</xsl:if>
		<xsl:if test="cyberwares/cyberware/gears/gear[isprogram = 'True']">
								<tr>
									<td colspan="6">
									<strong>Programme</strong><br />
			<xsl:for-each select="cyberwares/cyberware/gears/gear[isprogram = 'True']">
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
									</td>
								</tr>
		</xsl:if>
		<xsl:if test="cyberwares/cyberware/children/cyberwear/gears/gear[isprogram = 'True']">
								<tr>
									<td colspan="6">
									<strong>Programme</strong><br />
			<xsl:for-each select="cyberwares/cyberware/children/cyberware/gears/gear[isprogram = 'True']">
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
									</td>
								</tr>
		</xsl:if>
		<xsl:if test="weapons/weapon/accessories/accessory/gears/gear[isprogram = 'True']">
								<tr>
									<td colspan="6">
									<strong>Programme</strong><br />
			<xsl:for-each select="weapons/weapon/accessories/accessory/gears/gear[isprogram = 'True']">
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
									</td>
								</tr>
		</xsl:if>
	</xsl:template>
	
	<xsl:template name="spells">
								<tr>
									<td colspan="7" valign="top">
										Tradition: <xsl:value-of select="tradition" />&#160;&#160;&#160;
										Entzugswiderstand mit <xsl:value-of select="drain" />
									</td>
								</tr>
    <xsl:variable name="sortedlist">
      <xsl:for-each select="spells/spell">
			  <xsl:sort select="category" />
			  <xsl:sort select="name" />
          <xsl:copy-of select="current()"/>
      </xsl:for-each>
    </xsl:variable>
		<xsl:for-each select="msxsl:node-set($sortedlist)/spell">
                <xsl:choose>
									<xsl:when test="position() = 1">
										<tr><td colspan="7" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="category" /></strong></td></tr>
									</xsl:when>
                  <xsl:otherwise>
                    <xsl:choose>
                      <xsl:when test="category != preceding-sibling::spell[1]/category">
                        <tr><td colspan="7" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="category" /></strong></td></tr>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:otherwise>
                </xsl:choose>
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td width="25%" valign="top">
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
									</td>
									<td width="13%" style="text-align:center;" valign="top">
										<xsl:value-of select="type" />
									</td>
									<td width="13%" style="text-align:center;" valign="top">
										<xsl:value-of select="range" />
									</td>
									<td width="13%" style="text-align:center;" valign="top">
										<xsl:choose>
											<xsl:when test="damage = '0'">-</xsl:when>
											<xsl:otherwise><xsl:value-of select="damage" /></xsl:otherwise>
										</xsl:choose>
									</td>
									<td width="13%" style="text-align:center;" valign="top">
										<xsl:value-of select="duration" />
									</td>
									<td width="13%" style="text-align:center;" valign="top">
										<xsl:value-of select="dv" />
									</td>
									<td width="10%" style="text-align:center;" valign="top">
										<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
									</td>
								</tr>
								<xsl:if test="notes != ''">
									<tr>
										<xsl:if test="position() mod 2 != 1">
											<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
										</xsl:if>
										<td colspan="7" class="notesrow">
											<xsl:call-template name="PreserveLineBreaks">
												<xsl:with-param name="text" select="notes" />
											</xsl:call-template>
										</td>
									</tr>
								</xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="powers">
		<xsl:for-each select="powers/power">
			<xsl:sort select="name" />
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td width="40%">
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
									</td>
									<td width="20%" style="text-align:center;">
										<xsl:if test="rating &gt; 0">
											<xsl:value-of select="rating" />
										</xsl:if>
									</td>
									<td width="20%" style="text-align:center;">
										<xsl:value-of select="pointsperlevel" /> (<xsl:value-of select="totalpoints" />)
									</td>
									<td width="20%" style="text-align:center;">
										<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
									</td>
								</tr>
								<xsl:if test="notes != ''">
									<tr>
										<xsl:if test="position() mod 2 != 1">
											<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
										</xsl:if>
										<td colspan="4" class="notesrow">
											<xsl:call-template name="PreserveLineBreaks">
												<xsl:with-param name="text" select="notes" />
											</xsl:call-template>
										</td>
									</tr>
								</xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="critterpowers">
		<xsl:for-each select="critterpowers/critterpower">
			<xsl:sort select="name" />
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td width="50%">
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
									</td>
									<td width="30%" style="text-align:center;">
										<xsl:value-of select="rating" />
									</td>
									<td width="20%" style="text-align:center;">
										<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
									</td>
								</tr>
								<xsl:if test="notes != ''">
									<tr>
										<xsl:if test="position() mod 2 != 1">
											<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
										</xsl:if>
										<td colspan="3" class="notesrow">
											<xsl:call-template name="PreserveLineBreaks">
												<xsl:with-param name="text" select="notes" />
											</xsl:call-template>
										</td>
									</tr>
								</xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="complexforms">
								<tr>
									<td colspan="5" valign="top">
										Resonanzraum: <xsl:value-of select="stream" />&#160;&#160;&#160;
										Schwundwiderstand mit <xsl:value-of select="drain" />
									</td>
								</tr>
		<xsl:for-each select="complexforms/complexform">
			<xsl:sort select="name" />
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td width="30%">
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
										<xsl:if test="programoptions/programoption">
											(<xsl:for-each select="programoptions/programoption">
												<xsl:sort select="name" />
												<xsl:value-of select="name" />
												<xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
												<xsl:if test="position() != last()">; </xsl:if>
											</xsl:for-each>)
										</xsl:if>
									</td>
									<td width="15%">
										<xsl:value-of select="target" />
									</td>
									<td width="15%" style="text-align:center;">
										<xsl:value-of select="duration" />
									</td>
									<td width="15%" style="text-align:center;">
										<xsl:value-of select="fv" />
									</td>
									<td width="20%" style="text-align:center;">
										<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
									</td>
								</tr>
								<xsl:if test="notes != ''">
									<tr>
										<xsl:if test="position() mod 2 != 1">
											<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
										</xsl:if>
										<td colspan="4" class="notesrow">
											<xsl:call-template name="PreserveLineBreaks">
												<xsl:with-param name="text" select="notes" />
											</xsl:call-template>
										</td>
									</tr>
								</xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="martialarts">
		<xsl:for-each select="martialarts/martialart">
			<xsl:sort select="name" />
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td width="50%">
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
									</td>
									<td width="30%" style="text-align:center;">
										<xsl:value-of select="rating" />
									</td>
									<td width="20%" style="text-align:center;">
										<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
									</td>
								</tr>
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="3" class="indent">
										<xsl:for-each select="martialartadvantages/martialartadvantage">
											<xsl:sort select="." />
											<xsl:value-of select="." />
											<xsl:if test="position() != last()">; </xsl:if>
										</xsl:for-each>
									</td>
								</tr>
								<xsl:if test="notes != ''">
									<tr>
										<xsl:if test="position() mod 2 != 1">
											<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
										</xsl:if>
										<td colspan="3" class="notesrow">
											<xsl:call-template name="PreserveLineBreaks">
												<xsl:with-param name="text" select="notes" />
											</xsl:call-template>
										</td>
									</tr>
								</xsl:if>
		</xsl:for-each>
								<tr>
									<td colspan="3">
										<strong>Manöver</strong>
									</td>
								</tr>
								<tr>
									<td colspan="3" class="indent">
										<xsl:for-each select="martialartmaneuvers/martialartmaneuver">
											<xsl:sort select="name" />
											<xsl:value-of select="name" /><xsl:text> </xsl:text><xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
											<xsl:if test="position() != last()">; </xsl:if>
										</xsl:for-each>
									</td>
								</tr>
	</xsl:template>
	
	<xsl:template name="vehicles">
		<xsl:param name="vehicle" />
		<xsl:param name="VehicleNumber" />
		  <div class="block">
        <xsl:attribute name="id"><xsl:value-of select="$VehicleNumber" /></xsl:attribute>
				<table width="100%" cellspacing="0" cellpadding="0" border="0">
					<tr>
						<td>
							<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
								<tr>
									<td width="23%">
										<strong>Fahrzeug</strong>
									</td>
									<td width="8%" style="text-align:center;">
										<strong>Handl.</strong>
									</td>
									<td width="8%" style="text-align:center;">
										<strong>Beschl.</strong>
									</td>
									<td width="7%" style="text-align:center;">
										<strong>Geschw.</strong>
									</td>
									<td width="7%" style="text-align:center;">
										<strong>Pilot</strong>
									</td>
									<td width="7%" style="text-align:center;">
										<strong>Rumpf</strong>
									</td>
									<td width="7%" style="text-align:center;">
										<strong>Panz.</strong>
									</td>
									<td width="7%" style="text-align:center;">
										<strong>Sensor</strong>
									</td>
									<td width="5%" style="text-align:center;">
										<strong>SM</strong>
									</td>
									<td width="7%" style="text-align:center;">
										<strong>Seats</strong>
									</td>
									<td width="6%" style="text-align:center;">
										<strong>G-Stuf.</strong>
									</td>
									<td width="8%" style="text-align:center;">
									</td>
								</tr>
								<tr>
									<td width="23%" valign="top">
										<xsl:value-of select="name" />
										<xsl:if test="vehiclename != ''">
											("<xsl:value-of select="vehiclename" />")
										</xsl:if>
									</td>
									<td width="8%" style="text-align:center;" valign="top">
										<xsl:value-of select="handling" />
									</td>
									<td width="8%" style="text-align:center;" valign="top">
										<xsl:value-of select="accel" />
									</td>
									<td width="7%" style="text-align:center;" valign="top">
										<xsl:value-of select="speed" />
									</td>
									<td width="7%" style="text-align:center;" valign="top">
										<xsl:value-of select="pilot" />
									</td>
									<td width="7%" style="text-align:center;" valign="top">
										<xsl:value-of select="body" />
									</td>
									<td width="7%" style="text-align:center;" valign="top">
										<xsl:value-of select="armor" />
									</td>
									<td width="7%" style="text-align:center;" valign="top">
										<xsl:value-of select="sensor" /> (<xsl:value-of select="sensorsignal" />)
									</td>
									<td width="6%" style="text-align:center;" valign="top">
										<xsl:value-of select="physicalcm" />
									</td>
									<td width="7%" style="text-align:center;" valign="top">
										<xsl:value-of select="seats" />
									</td>
									<td width="7%" style="text-align:center;" valign="top">
										<xsl:value-of select="devicerating" />
									</td>
									<td width="8%" style="text-align:center;" valign="top">
										<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
									</td>
								</tr>
				<xsl:if test="mods/mod">
								<tr>
									<td colspan="15" class="indent">
					<xsl:for-each select="mods/mod">
						<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="rating != 0"> Stufe <xsl:value-of select="rating" /></xsl:if>
										<xsl:if test="cyberwares/cyberware">
											(<xsl:for-each select="cyberwares/cyberware">
												<xsl:sort select="name" />
												<xsl:value-of select="name" />
												<xsl:if test="rating != 0"> Stufe <xsl:value-of select="rating" /></xsl:if>
												<xsl:if test="position() != last()">; </xsl:if>
											</xsl:for-each>)
										</xsl:if>
										<xsl:if test="position() != last()">; </xsl:if>
					</xsl:for-each>
									</td>
								</tr>
				</xsl:if>
				<xsl:if test="gears/gear">
								<tr>
									<td colspan="15" class="indent">
					<xsl:for-each select="gears/gear">
						<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
										<xsl:if test="rating > 0"> Stufe <xsl:value-of select="rating" /></xsl:if>
										<xsl:if test="qty > 1">
											x<xsl:value-of select="qty" />
										</xsl:if>
										
										<xsl:if test="children/gear">
											(<xsl:for-each select="children/gear">
												<xsl:sort select="name" />
												<xsl:value-of select="name" />
												<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
												<xsl:if test="rating > 0">
												Rating <xsl:value-of select="rating" />
												</xsl:if>
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
											</xsl:for-each>)
										</xsl:if>
										
										<xsl:if test="position() != last()">; </xsl:if>
					</xsl:for-each>
									</td>
								</tr>
				</xsl:if>
				
				<xsl:if test="mods/mod/weapons/weapon[type = 'Ranged'] or weapons/weapon[type = 'Ranged']">
								<tr>
									<td colspan="15" class="indent">
										<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
											<tr>
												<td width="20%">
													<strong>Waffe</strong>
												</td>
												<td width="8%" style="text-align:center;">
													<strong>Pool</strong>
												</td>
												<td width="11%" style="text-align:center;">
													<strong>Präzision</strong>
												</td>
												<td width="13%" style="text-align:center;">
													<strong>Schaden</strong>
												</td>
												<td width="8%" style="text-align:center;">
													<strong>DK</strong>
												</td>
												<td width="9%" style="text-align:center;">
													<strong>Modus</strong>
												</td>
												<td width="9%" style="text-align:center;">
													<strong>RK</strong>
												</td>
												<td width="11%" style="text-align:center;">
													<strong>Muni</strong>
												</td>
												<td width="11%" style="text-align:center;">
												</td>
											</tr>
						<xsl:for-each select="mods/mod/weapons/weapon[type = 'Ranged']">
							<xsl:sort select="name" />
							<xsl:call-template name="rangedweapons">
								<xsl:with-param name="weapon" select="weapon" />
							</xsl:call-template>
						</xsl:for-each>
						<xsl:for-each select="weapons/weapon[type = 'Ranged']">
							<xsl:sort select="name" />
							<xsl:call-template name="rangedweapons">
								<xsl:with-param name="weapon" select="weapon" />
							</xsl:call-template>
						</xsl:for-each>
											<tr>
												<td class="rowsummary" colspan="9">
													Fernkampfwaffen
												</td>
											</tr>
										</table>
									</td>
								</tr>
				</xsl:if>
				
								<xsl:if test="notes != ''">
									<tr>
										<xsl:if test="position() mod 2 != 1">
											<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
										</xsl:if>
										<td colspan="15" class="notesrow">
											<xsl:call-template name="PreserveLineBreaks">
												<xsl:with-param name="text" select="notes" />
											</xsl:call-template>
										</td>
									</tr>
								</xsl:if>
														
								<tr>
									<td class="rowsummary" colspan="15">
                    <table width="100%" cellspacing="0" cellpadding="0" border="0">
                      <tr>
                        <td class="rowsummary">
                          Fahrzeug/Drohne
                        </td>
                        <td class="rowsummarybutton" colspan="1">
                           <xsl:attribute name="onClick">showhide(this,'<xsl:value-of select="$VehicleNumber" />');</xsl:attribute>
                           Zeigen: Ja
                        </td>
                        <td class="rowsummarybutton" width="50%" colspan="1">
                           <xsl:attribute name="onClick">zalomit(this,'<xsl:value-of select="$VehicleNumber" />');</xsl:attribute>
                           Seitenumbruch: Nein
                        </td>
                      </tr>
                    </table>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td class="hseparator"></td>
					</tr>
				</table>
				</div>
	</xsl:template>
	
	<xsl:template name="gearplugin">
		<xsl:param name="gear" />
		<xsl:for-each select="children/gear">
			<xsl:sort select="name" />
							<xsl:value-of select="name" />
							<xsl:if test="rating != 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
							<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
							<xsl:if test="children/gear">
								[<xsl:call-template name="gearplugin">
									<xsl:with-param name="gear" select="." />
								</xsl:call-template>]
							</xsl:if>
							<xsl:if test="position() != last()">; </xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="gear1">
		<xsl:variable name="halfcut" select="round(count(gears/gear[iscommlink != 'True']) div 3)"/>
    <xsl:variable name="sortedcopy">
      <xsl:for-each select="gears/gear[iscommlink != 'True']">
			  <xsl:sort select="location" />
			  <xsl:sort select="name" />
        <xsl:if test="position() &lt;= $halfcut">
          <xsl:copy-of select="current()"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:for-each select="msxsl:node-set($sortedcopy)/gear">
                <xsl:choose>
									<xsl:when test="position() = 1">
										<tr><td colspan="4" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="location" /></strong></td></tr>
									</xsl:when>
                  <xsl:otherwise>
                    <xsl:choose>
                      <xsl:when test="location != preceding-sibling::gear[1]/location">
                        <tr><td colspan="4" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="location" /></strong></td></tr>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:otherwise>
                </xsl:choose>
 											<tr>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
												<td width="55%" valign="top">
													<xsl:value-of select="name" />
													<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="rating = 0">-</xsl:when>
														<xsl:otherwise><xsl:value-of select="rating" /></xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:value-of select="qty" />
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
												</td>
											</tr>
				<xsl:if test="children/gear">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="4" class="indent">
										<xsl:call-template name="gearplugin">
											<xsl:with-param name="gear" select="." />
										</xsl:call-template>
									</td>
								</tr>
				</xsl:if>
											<xsl:if test="notes != ''">
												<tr>
													<xsl:if test="position() mod 2 != 1">
														<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
													</xsl:if>
													<td colspan="4" class="notesrow">
														<xsl:call-template name="PreserveLineBreaks">
															<xsl:with-param name="text" select="notes" />
														</xsl:call-template>
													</td>
												</tr>
											</xsl:if>
		  </xsl:for-each>
	</xsl:template>
	
	<xsl:template name="gear2">
		<xsl:variable name="halfcut" select="round(count(gears/gear[iscommlink != 'True']) div 3)"/>
    <xsl:variable name="sortedcopy">
      <xsl:for-each select="gears/gear[iscommlink != 'True']">
			  <xsl:sort select="location" />
			  <xsl:sort select="name" />
        <xsl:if test="position() &gt; $halfcut and position() &lt;= $halfcut * 2">
          <xsl:copy-of select="current()"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:for-each select="msxsl:node-set($sortedcopy)/gear">
                <xsl:choose>
									<xsl:when test="position() = 1">
										<tr><td colspan="4" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="location" /></strong></td></tr>
									</xsl:when>
                  <xsl:otherwise>
                    <xsl:choose>
                      <xsl:when test="location != preceding-sibling::gear[1]/location">
                        <tr><td colspan="4" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="location" /></strong></td></tr>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:otherwise>
                </xsl:choose>
											<tr>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
												<td width="55%" valign="top">
													<xsl:value-of select="name" />
													<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="rating = 0">-</xsl:when>
														<xsl:otherwise><xsl:value-of select="rating" /></xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:value-of select="qty" />
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
												</td>
											</tr>
				<xsl:if test="children/gear">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="4" class="indent">
										<xsl:call-template name="gearplugin">
											<xsl:with-param name="gear" select="." />
										</xsl:call-template>
									</td>
								</tr>
				</xsl:if>
				
											<xsl:if test="notes != ''">
												<tr>
													<xsl:if test="position() mod 2 != 1">
														<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
													</xsl:if>
													<td colspan="4" class="notesrow">
														<xsl:call-template name="PreserveLineBreaks">
															<xsl:with-param name="text" select="notes" />
														</xsl:call-template>
													</td>
												</tr>
											</xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="gear3">
		<xsl:variable name="halfcut" select="round(count(gears/gear[iscommlink != 'True']) div 3)"/>
    <xsl:variable name="sortedcopy">
      <xsl:for-each select="gears/gear[iscommlink != 'True']">
			  <xsl:sort select="location" />
			  <xsl:sort select="name" />
        <xsl:if test="position() &gt; $halfcut * 2">
          <xsl:copy-of select="current()"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:for-each select="msxsl:node-set($sortedcopy)/gear">
                <xsl:choose>
									<xsl:when test="position() = 1">
										<tr><td colspan="4" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="location" /></strong></td></tr>
									</xsl:when>
                  <xsl:otherwise>
                    <xsl:choose>
                      <xsl:when test="location != preceding-sibling::gear[1]/location">
                        <tr><td colspan="4" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="location" /></strong></td></tr>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:otherwise>
                </xsl:choose>
											<tr>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
												<td width="55%" valign="top">
													<xsl:value-of select="name" />
													<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="rating = 0">-</xsl:when>
														<xsl:otherwise><xsl:value-of select="rating" /></xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:value-of select="qty" />
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
												</td>
											</tr>
				<xsl:if test="children/gear">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="4" class="indent">
										<xsl:call-template name="gearplugin">
											<xsl:with-param name="gear" select="." />
										</xsl:call-template>
									</td>
								</tr>
				</xsl:if>
				
											<xsl:if test="notes != ''">
												<tr>
													<xsl:if test="position() mod 2 != 1">
														<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
													</xsl:if>
													<td colspan="4" class="notesrow">
														<xsl:call-template name="PreserveLineBreaks">
															<xsl:with-param name="text" select="notes" />
														</xsl:call-template>
													</td>
												</tr>
											</xsl:if>
		</xsl:for-each>
	</xsl:template>
	
  
  
<!--
  **** EdgeBox(LowBox,HighBox)
    Params:
      *LowBox: This is the number of the first box in this Row
      *HighBox: this is the last box in this Row
      
    This template draws a circle.  Then it determins if more boxes are needed for this 
      Row(Is LowBox less than HighBox), if more are needed, it recursively calls itself 
      incrementing LowBox and HighBox by CMWidth.
-->
<xsl:template name="EdgeBox">
  <xsl:param name="LowBox">1</xsl:param>
  <xsl:param name="HighBox">3</xsl:param>
  <xsl:if test="$LowBox &lt;= $HighBox">
    O
  </xsl:if>
  <xsl:if test="$LowBox &lt; $HighBox">
    <xsl:call-template name="EdgeBox">
      <xsl:with-param name="LowBox"><xsl:value-of select="$LowBox + 1" /></xsl:with-param>
      <xsl:with-param name="HighBox"><xsl:value-of select="$HighBox" /></xsl:with-param>
    </xsl:call-template>
  </xsl:if>
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
