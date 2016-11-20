<?xml version="1.0" encoding="UTF-8" ?>
<!-- Game Master character summary sheet -->
<!-- Created by Keith Rudolph, krudolph@gmail.com -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
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
						font-family: tahoma, trebuchet ms, arial;
						font-size: 8pt;
					}
					.tableborder
					{
						border: solid 2px #1c4a2d;
					}
					.conditionmonitorbox
					{
						border: solid 2px #1c4a2d;
						width: 30px;
						height: 30px;
						text-align: right;
						vertical-align: text-bottom;
						font-weight: bold;
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
				</style>
			</head>
			<body>
					<table width="100%" cellspacing="0" cellpadding="2" border="0" style="border: solid 2px #000000;">
						<tr>
							<td>
								<strong><xsl:value-of select="name" /></strong> (<xsl:value-of select="metatype" />)
								&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;
								<strong>Movement:</strong> <xsl:value-of select="movement" />
								<table width="100%" cellspacing="0" cellpadding="2">
									<tr>
										<td width="9%" align="center"><strong>B</strong></td>
										<td width="9%" align="center"><strong>A</strong></td>
										<td width="9%" align="center"><strong>R</strong></td>
										<td width="9%" align="center"><strong>S</strong></td>
										<td width="9%" align="center"><strong>C</strong></td>
										<td width="9%" align="center"><strong>I</strong></td>
										<td width="9%" align="center"><strong>L</strong></td>
										<td width="9%" align="center"><strong>W</strong></td>
										<td width="9%" align="center"><strong>E</strong></td>
										<xsl:if test="magenabled = 'True'">
											<td width="9%" align="center"><strong>MAG</strong></td>
										</xsl:if>
										<xsl:if test="resenabled = 'True'">
											<td width="9%" align="center"><strong>RES</strong></td>
										</xsl:if>
										<td width="9%" align="center"><strong>ESS</strong></td>
									</tr>
									<tr>
										<td width="9%" align="center">
											<xsl:value-of select="attributes/attribute[name = 'BOD']/base" />
											<xsl:if test="attributes/attribute[name = 'BOD']/total != attributes/attribute[name = 'BOD']/base">
												(<xsl:value-of select="attributes/attribute[name = 'BOD']/total" />)
											</xsl:if>
										</td>
										<td width="9%" align="center">
											<xsl:value-of select="attributes/attribute[name = 'AGI']/base" />
											<xsl:if test="attributes/attribute[name = 'AGI']/total != attributes/attribute[name = 'AGI']/base">
												(<xsl:value-of select="attributes/attribute[name = 'AGI']/total" />)
											</xsl:if>
										</td>
										<td width="9%" align="center">
											<xsl:value-of select="attributes/attribute[name = 'REA']/base" />
											<xsl:if test="attributes/attribute[name = 'REA']/total != attributes/attribute[name = 'REA']/base">
												(<xsl:value-of select="attributes/attribute[name = 'REA']/total" />)
											</xsl:if>
										</td>
										<td width="9%" align="center">
											<xsl:value-of select="attributes/attribute[name = 'STR']/base" />
											<xsl:if test="attributes/attribute[name = 'STR']/total != attributes/attribute[name = 'STR']/base">
												(<xsl:value-of select="attributes/attribute[name = 'STR']/total" />)
											</xsl:if>
										</td>
										<td width="9%" align="center">
											<xsl:value-of select="attributes/attribute[name = 'CHA']/base" />
											<xsl:if test="attributes/attribute[name = 'CHA']/total != attributes/attribute[name = 'CHA']/base">
												(<xsl:value-of select="attributes/attribute[name = 'CHA']/total" />)
											</xsl:if>
										</td>
										<td width="9%" align="center">
											<xsl:value-of select="attributes/attribute[name = 'INT']/base" />
											<xsl:if test="attributes/attribute[name = 'INT']/total != attributes/attribute[name = 'INT']/base">
												(<xsl:value-of select="attributes/attribute[name = 'INT']/total" />)
											</xsl:if>
										</td>
										<td width="9%" align="center">
											<xsl:value-of select="attributes/attribute[name = 'LOG']/base" />
											<xsl:if test="attributes/attribute[name = 'LOG']/total != attributes/attribute[name = 'LOG']/base">
												(<xsl:value-of select="attributes/attribute[name = 'LOG']/total" />)
											</xsl:if>
										</td>
										<td width="9%" align="center">
											<xsl:value-of select="attributes/attribute[name = 'WIL']/base" />
											<xsl:if test="attributes/attribute[name = 'WIL']/total != attributes/attribute[name = 'WIL']/base">
												(<xsl:value-of select="attributes/attribute[name = 'WIL']/total" />)
											</xsl:if>
										</td>
										<td width="9%" align="center">
											<xsl:value-of select="attributes/attribute[name = 'EDG']/base" />
											<xsl:if test="attributes/attribute[name = 'EDG']/total != attributes/attribute[name = 'EDG']/base">
												(<xsl:value-of select="attributes/attribute[name = 'EDG']/total" />)
											</xsl:if>
										</td>
										<xsl:if test="magenabled = 'True'">
										<td width="9%" align="center">
											<xsl:value-of select="attributes/attribute[name = 'MAG']/base" />
											<xsl:if test="attributes/attribute[name = 'MAG']/total != attributes/attribute[name = 'MAG']/base">
												(<xsl:value-of select="attributes/attribute[name = 'MAG']/total" />)
											</xsl:if>
										</td>
										</xsl:if>
										<xsl:if test="resenabled = 'True'">
										<td width="9%" align="center">
											<xsl:value-of select="attributes/attribute[name = 'RES']/base" />
											<xsl:if test="attributes/attribute[name = 'RES']/total != attributes/attribute[name = 'RES']/base">
												(<xsl:value-of select="attributes/attribute[name = 'RES']/total" />)
											</xsl:if>
										</td>
										</xsl:if>
										<td width="9%" align="center">
											<xsl:value-of select="attributes/attribute[name = 'ESS']/base" />
											<xsl:if test="attributes/attribute[name = 'ESS']/total != attributes/attribute[name = 'ESS']/base">
												(<xsl:value-of select="attributes/attribute[name = 'ESS']/total" />)
											</xsl:if>
										</td>
									</tr>
									<tr colspan="11">
										<td width="9%" align="center" valign="top"><strong>INIT</strong></td>
										<td width="9%" align="center" valign="top"><strong>RIGGER</strong></td>
										<td width="9%" align="center" valign="top"><strong>ASTRAL</strong></td>
										<td width="9%" align="center" valign="top"><strong>MATRIX AR</strong></td>
										<td width="9%" align="center" valign="top"><strong>MX COLD</strong></td>
										<td width="9%" align="center" valign="top"><strong>MX HOT</strong></td>
										<td width="9%" align="center" valign="top"><strong>CM</strong></td>
										<td width="9%" align="center" valign="top"><strong>PHYSICAL</strong></td>
										<td width="9%" align="center" valign="top"><strong>MENTAL</strong></td>
										<td width="9%" align="center" valign="top"><strong>SOCIAL</strong></td>
									</tr>
									<tr colspan="11">
									</tr>
									<tr colspan="11">
										<td width="9%" align="center" valign="top">
											<xsl:value-of select="init" />
										</td>
										<td width="9%" align="center" valign="top">
											<xsl:value-of select="riggerinit" />
										</td>
										<td width="9%" align="center" valign="top">
											<xsl:value-of select="astralinit" />
										</td>
										<td width="9%" align="center" valign="top">
											<xsl:value-of select="matrixarinit" />
										</td>
										<td width="9%" align="center" valign="top">
											<xsl:value-of select="matrixcoldinit" />
										</td>
										<td width="9%" align="center" valign="top">
											<xsl:value-of select="matrixhotinit" />
										</td>
										<td width="9%" align="center" valign="top">
											<xsl:value-of select="physicalcm" />/<xsl:value-of select="stuncm" />
										</td>
										<td width="9%" align="center" valign="top">
											<xsl:value-of select="limitphysical" />
										</td>
										<td width="9%" align="center" valign="top">
											<xsl:value-of select="limitmental" />
										</td>
										<td width="9%" align="center" valign="top">
											<xsl:value-of select="limitsocial" />
										</td>
									</tr>
								</table>
								
								<xsl:if test="qualities/quality">
									<p><strong>Qualities: </strong>
									<xsl:for-each select="qualities/quality">
										<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
										<xsl:if test="position() != last()">, </xsl:if>
									</xsl:for-each></p>
								</xsl:if>
								
								<p><strong>Active Skills: </strong>
								<xsl:for-each select="skills/skill[knowledge = 'False' and (rating &gt; 0 or total &gt; 0)]">
									<xsl:sort select="name" />
									<xsl:value-of select="name" />
									<xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
									<xsl:text> </xsl:text><xsl:value-of select="total" />
									<xsl:if test="spec != '' and exotic = 'False'"> (<xsl:value-of select="specializedrating" />)</xsl:if>
									<xsl:if test="position() != last()">, </xsl:if>
								</xsl:for-each></p>
								
								<xsl:if test="skills/skill[knowledge = 'True' and (rating &gt; 0 or total &gt; 0 or rating = 0)]">
									<p><strong>Knowledge Skills: </strong>
									<xsl:for-each select="skills/skill[knowledge = 'True' and (rating &gt; 0 or total &gt; 0 or rating = 0)]">
										<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
										<xsl:if test="rating = 0"><xsl:text> </xsl:text>N</xsl:if>
										<xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="total" /></xsl:if>
										<xsl:if test="spec != ''"> (<xsl:value-of select="specializedrating" />)</xsl:if>
										<xsl:if test="position() != last()">, </xsl:if>
									</xsl:for-each></p>
								</xsl:if>
								
								<table width="100%" cellspacing="0" cellpadding="0">
									<tr>
										<td width="49%" valign="top">
											<table width="100%" cellspacing="0" cellpadding="2">
												<tr>
													<td><strong>Weapon</strong></td>
													<td align="center"><strong>Pool</strong></td>
													<td align="center"><strong>ACC</strong></td>
													<td align="center"><strong>DMG</strong></td>
													<td align="center"><strong>AP</strong></td>
													<td align="center"><strong>Mode</strong></td>
													<td align="center"><strong>RC</strong></td>
												</tr>
												<xsl:for-each select="weapons/weapon">
													<xsl:sort select="name" />
													<tr>
														<xsl:if test="position() mod 2 != 1">
															<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
														</xsl:if>
														<td><xsl:value-of select="name" /></td>
														<td align="center"><xsl:value-of select="dicepool" /></td>
														<td align="center"><xsl:value-of select="accuracy" /></td>
														<td align="center"><xsl:value-of select="damage" /></td>
														<td align="center"><xsl:value-of select="ap" /></td>
														<td align="center"><xsl:value-of select="mode" /></td>
														<td align="center"><xsl:value-of select="rc" /></td>
													</tr>
													<xsl:if test="accessories/accessory or mods/weaponmod">
														<tr>
															<td colspan="5" class="indent">
																<xsl:if test="position() mod 2 != 1">
																	<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
																</xsl:if>
																<xsl:for-each select="accessories/accessory">
																	<xsl:sort select="name" />
																					<xsl:value-of select="name" />
																					<xsl:if test="position() != last()">, </xsl:if>
																</xsl:for-each>
																<xsl:if test="accessories/accessory and mods/weaponmod">, </xsl:if>
																<xsl:for-each select="mods/weaponmod">
																	<xsl:sort select="name" />
																					<xsl:value-of select="name" />
																					<xsl:if test="rating > 0">
																					Rating <xsl:value-of select="rating" />
																					</xsl:if>
																					<xsl:if test="position() != last()">, </xsl:if>
																</xsl:for-each>
															</td>
														</tr>
													</xsl:if>
												</xsl:for-each>
											</table>
										</td>
										<td width=".5%">&#160;</td>
										<td width="25%" valign="top">
											<table width="100%" cellspacing="0" cellpadding="2">
												<tr>
													<td><strong>Combat Skill</strong></td>
													<td align="center"><strong>Rtg</strong></td>
												</tr>
												<xsl:for-each select="skills/skill[skillcategory = 'Combat Active']">
													<xsl:sort select="name" />
													<tr>
														<xsl:if test="position() mod 2 != 1">
															<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
														</xsl:if>
														<td>
															<xsl:value-of select="name" />
															<xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
														</td>
														<td align="center"><xsl:value-of select="total" /></td>
													</tr>
												</xsl:for-each>
											</table>
										</td>
										<td width=".5%">&#160;</td>
										<td width="25%" valign="top">
											<table width="100%" cellspacing="0" cellpadding="2">
												<tr>
													<td><strong>Armor (<xsl:value-of select="armor" />)</strong></td>
													<td align="center"> </td>
												</tr>
												<xsl:for-each select="armors/armor">
													<xsl:sort select="name" />
													<tr>
														<xsl:if test="position() mod 2 != 1">
															<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
														</xsl:if>
														<td><xsl:value-of select="name" /></td>
														<td align="center"><xsl:value-of select="armor" /></td>
													</tr>
													<xsl:if test="armormods/armormod">
														<tr>
															<td colspan="2" class="indent">
																(w/
																<xsl:for-each select="armormods/armormod">
																	<xsl:sort select="name" />
																	<xsl:value-of select="name" />
																	<xsl:if test="position() != last()">, </xsl:if>
																</xsl:for-each>)
															</td>
														</tr>
													</xsl:if>
													<xsl:if test="gears/gear">
														<tr>
															<xsl:if test="position() mod 2 != 1">
																<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
															</xsl:if>
															<td colspan="2" class="indent">
																<xsl:for-each select="gears/gear">
																	<xsl:sort select="name" />
																	<xsl:value-of select="name" />
																	<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
																	<xsl:if test="rating != 0"> Rating <xsl:value-of select="rating" /></xsl:if>
																	<xsl:if test="qty &gt; 1"> x<xsl:value-of select="qty" /></xsl:if>
																	<xsl:if test="children/gear">
																		(<xsl:for-each select="children/gear">
																			<xsl:value-of select="name" />
																			<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
																			<xsl:if test="rating != 0"> Rating <xsl:value-of select="rating" /></xsl:if>
																			<xsl:if test="children/gear">
																				[<xsl:for-each select="children/gear">
																					<xsl:sort select="name" />
																					<xsl:value-of select="name" />
																					<xsl:if test="rating != 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
																					<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
																					<xsl:if test="position() != last()">, </xsl:if>
																				</xsl:for-each>]
																			</xsl:if>
																		</xsl:for-each>)
																	</xsl:if>
																</xsl:for-each>
															</td>
														</tr>
													</xsl:if>
												</xsl:for-each>
											</table>
										</td>
									</tr>
								</table>
								
								<xsl:if test="martialarts/martialart">
									<p><strong>Martial Arts: </strong>
									<xsl:for-each select="martialarts/martialart">
										<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="martialartadvantages/martialartadvantage">
										(
										<xsl:for-each select="martialartadvantages/martialartadvantage">
											<xsl:value-of select="." /><xsl:if test="position() != last()">, </xsl:if>
										</xsl:for-each>
										)
										</xsl:if>
									</xsl:for-each></p>
									<xsl:if test="martialartmaneuvers/martialartmaneuver">
										<p><strong>Maneuvers: </strong>
										<xsl:for-each select="martialartmaneuvers/martialartmaneuver">
											<xsl:sort select="name" />
											<xsl:value-of select="name" /><xsl:if test="position() != last()">, </xsl:if>
										</xsl:for-each></p>
									</xsl:if>
								</xsl:if>

								<xsl:if test="spells/spell">
									<p><strong>Spells: </strong>
									<xsl:for-each select="spells/spell">
										<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
										(<xsl:value-of select="type" />, <xsl:value-of select="range" />, <xsl:value-of select="damage" />, <xsl:value-of select="duration" />, <xsl:value-of select="dv" />)
										<xsl:if test="position() != last()">, </xsl:if>
									</xsl:for-each></p>
								</xsl:if>
								
								<xsl:if test="powers/power">
									<p><strong>Powers: </strong>
									<xsl:for-each select="powers/power">
										<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
										<xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
										<xsl:if test="position() != last()">, </xsl:if>
									</xsl:for-each></p>
								</xsl:if>
								
								<xsl:if test="critterpowers/critterpower">
									<p><strong>Critter Powers: </strong>
									<xsl:for-each select="critterpowers/critterpower">
										<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
										<xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
										<xsl:if test="position() != last()">, </xsl:if>
									</xsl:for-each></p>
								</xsl:if>
								
								<xsl:if test="techprograms/techprogram">
									<p><strong>Complex Forms: </strong>
									<xsl:for-each select="techprograms/techprogram">
										<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
										<xsl:text> </xsl:text><xsl:value-of select="rating" />
										<xsl:if test="programoptions/programoption">
											(<xsl:for-each select="programoptions/programoption">
												<xsl:sort select="name" />
												<xsl:value-of select="name" />
												<xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
												<xsl:if test="position() != last()">, </xsl:if>
											</xsl:for-each>)
										</xsl:if>
										<xsl:if test="position() != last()">, </xsl:if>
									</xsl:for-each></p>
								</xsl:if>
								
								<xsl:if test="cyberwares/cyberware">
									<p><strong>Cyberware/Bioware: </strong>
									<xsl:for-each select="cyberwares/cyberware">
										<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="rating != 0"> Rating <xsl:value-of select="rating" /></xsl:if>
										<xsl:if test="children/cyberware">
											(w/
											<xsl:for-each select="children/cyberware">
												<xsl:value-of select="name" />
												<xsl:if test="rating != 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
												<xsl:if test="position() != last()">, </xsl:if>
											</xsl:for-each>)
										</xsl:if>
										<xsl:if test="position() != last()">, </xsl:if>
									</xsl:for-each></p>
								</xsl:if>
								
								<xsl:if test="gears/gear">
									<p><strong>Gear: </strong>
									<xsl:for-each select="gears/gear">
										<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="qty &gt; 1"> x<xsl:value-of select="qty" /></xsl:if>
										<xsl:if test="children/gear">
											[<xsl:call-template name="gearplugin">
											<xsl:with-param name="gear" select="." />
										</xsl:call-template>]
										</xsl:if>
										<xsl:if test="position() != last()">, </xsl:if>
									</xsl:for-each></p>
								</xsl:if>
								
								<p><strong>Nuyen: </strong><xsl:value-of select="nuyen" /></p>
								
								<xsl:if test="notes != ''">
									<p><strong>Notes: </strong><xsl:value-of select="notes" /></p>
								</xsl:if>
							</td>
						</tr>
					</table>
			</body>
		</html>
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
</xsl:stylesheet>