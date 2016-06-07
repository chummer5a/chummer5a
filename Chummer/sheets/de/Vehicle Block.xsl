<?xml version="1.0" encoding="UTF-8" ?>
<!-- Vehicle sheet based on the Shadowrun 5th Edition Character Sheet -->
<!-- Created by KeyMasterOfGozer -->
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
					.tablebordertop
					{
						border-top: solid 2px #1c4a2d;
					}
					.tableborderbottom
					{
						border-bottom: solid 2px #1c4a2d;
					}
					.tableborderleft
					{
						border-left: solid 2px #1c4a2d;
					}
					.tableborderright
					{
						border-right: solid 2px #1c4a2d;
					}
					.AttribName
					{
						text-align: right;
            font-weight: bold;
            padding-right: 3px;
					}
					.AttribValue
					{
						text-align: left;
            padding-left: 3px;
					}
					.ItemList
					{
						text-align: left;
            padding-left: 3px;
					}
					.ItemListHeader
					{
						text-align: left;
            padding-left: 3px;
            font-weight: bold;
            text-decoration:underline;
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
						filter: progid:DXImageTransform.Microsoft.gradient(startColorstr='#3a6349', endColorstr='#769582'); /* for IE */
						background: -webkit-gradient(linear, left top, left bottom, from(#3a6349), to(#769582)); /* for webkit browsers */
						background: -moz-linear-gradient(top,  #3a6349,  #769582); /* for firefox 3.6+ */ 
						background-color: #3a6349;
						color: #ffffff;
						font-weight: bold;
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
          .zalomit {
            page-break-after: always;
          }

          .common1 {
            width: 17cm;
            display: block;
            color: red;
            text-align: center;
            background: #FAEBD7;
            font-weight: bold;
            border: 1px red solid;
          }

          @page {
            margin: 1.9cm;
            size: 21cm 27.9cm;
          }

          @media print {
            .zalomit {
              page-break-after: always;
            }
            .sectionhide {
              visibility: hidden;
              display:none;
            }
            .rowsummarybutton {
              visibility: hidden;
              display:none;
            }
           
            .table {
              width: 17cm;
            }
          } 
          @media screen {
            .sectionhide {
              visibility: visible;
            }
            .rowsummarybutton {
              visibility: visible;
            }
          } 
			</style>
      <script>
      <xsl:text>
      function zalomit(what,idx)
      {
         var elem = document.getElementById(idx); 
         if(elem.style.pageBreakBefore=='always')  {
            txt="NEIN";
            elem.style.pageBreakBefore = 'auto';
         }
         else {
            txt="JA";
            elem.style.pageBreakBefore = 'always';
         }
         what.innerHTML = "Seitenumbruch: " + txt;
      }
      function showhide(what,idx)
      {
         var elem = document.getElementById(idx); 
         if(elem.className!='sectionhide')  {
           txt="NEIN";
           elem.className = 'sectionhide';
        //   elem.style.display = 'none';
         }
         else {
           txt="JA";
           elem.className = 'block';
          // elem.style.display = 'block';
         }
         what.innerHTML = "Zeigen: " + txt;
      }
      </xsl:text>
      </script>
			</head>
			<body>

          <xsl:for-each select="vehicles/vehicle">
						<xsl:sort select="name" />
						<xsl:call-template name="vehicles">
							<xsl:with-param name="vehicle" />
							<xsl:with-param name="VehicleNumber">VehicleBlock<xsl:value-of select="position()"/></xsl:with-param>
						</xsl:call-template>
					</xsl:for-each>
	
			</body>
		</html>
	</xsl:template>
  <!--
  Place Buttons for Show and Page Break
  -->
  <xsl:template name="SectionHeading">
    <xsl:param name="SectionName">Name Blank</xsl:param>
    <xsl:param name="SectionBlock">Block Blank</xsl:param>
										<table>	
                      <tr>
												<td class="rowsummary" width="75%">
													<xsl:value-of select="$SectionName" />
												</td>
                        <td class="rowsummarybutton" width="150" onClick="showhide(this,'$SectionBlock');">Zeigen: JA</td>
                        <td class="rowsummarybutton" width="150" onClick="zalomit(this,'$SectionBlock');">Seitenumbruch: NEIN</td>
											</tr>
										</table>
	</xsl:template>
  
	<xsl:template name="vehicles">
		<xsl:param name="vehicle" />
		<xsl:param name="VehicleNumber" />
				<div class="block">
          <xsl:attribute name="id"><xsl:value-of select="$VehicleNumber" /></xsl:attribute>
<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
  <tr>
    <td>
      <table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborderright tableborderbottom">
        <tr>
          <td>
            <!-- ** This Table shows the Stats Block -->
            <table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborderright" style="padding-right:3px;">
              <tr>
                <td class="AttribName">Fahrzeug</td>
                <td class="AttribValue"><xsl:value-of select="name" /></td>
                <td class="AttribName">System</td>
                <td class="AttribValue"><xsl:value-of select="system" /></td>
              </tr>
              <tr>
                <td class="AttribName">Name</td>
                <td class="AttribValue"><xsl:value-of select="vehiclename" /></td>
                <td class="AttribName">Firewall</td>
                <td class="AttribValue"><xsl:value-of select="firewall" /></td>
              </tr>
              <tr>
                <td class="AttribName">Kategorie</td>
                <td class="AttribValue"><xsl:value-of select="category" /></td>
                <td class="AttribName">Reaktion</td>
                <td class="AttribValue"><xsl:value-of select="response" /></td>
              </tr>
              <tr>
                <td class="AttribName">Quelle</td>
                <td class="AttribValue"><xsl:value-of select="source" /> <xsl:value-of select="page" /></td>
                <td class="AttribName">Signal</td>
                <td class="AttribValue"><xsl:value-of select="signal" /></td>
              </tr>
              <tr><td colspan="4" class="hseparator"></td></tr>
              <tr>
                <td class="AttribName">Rumpf</td>
                <td class="AttribValue"><xsl:value-of select="body" /></td>
                <td class="AttribName">Clearsight</td>
                <td class="AttribValue"><xsl:call-template name="GetProgramRating"><xsl:with-param name="ProgramName">Clearsight</xsl:with-param></xsl:call-template></td>
              </tr>
              <tr>
                <td class="AttribName">Panzerung</td>
                <td class="AttribValue"><xsl:value-of select="armor" /></td>
                <td class="AttribName">Abwehr</td>
                <td class="AttribValue"><xsl:call-template name="GetProgramRating"><xsl:with-param name="ProgramName">Abwehr</xsl:with-param></xsl:call-template></td>
              </tr>
              <tr>
                <td class="AttribName">Pilot</td>
                <td class="AttribValue"><xsl:value-of select="pilot" /></td>
                <td class="AttribName">Elektronische Kriegsführung</td>
                <td class="AttribValue"><xsl:call-template name="GetProgramRating"><xsl:with-param name="ProgramName">Elektronische Kriegsführung</xsl:with-param></xsl:call-template></td>
              </tr>
              <tr>
                <td class="AttribName">Sensor</td>
                <td class="AttribValue"><xsl:value-of select="sensor" /></td>
                <td class="AttribName">Manövrierbarkeit</td>
                <td class="AttribValue"><xsl:call-template name="GetProgramRating"><xsl:with-param name="ProgramName">Manövrierbarkeit</xsl:with-param></xsl:call-template></td>
              </tr>
              <tr>
                <td class="AttribName">Sensor Signal</td>
                <td class="AttribValue"><xsl:value-of select="sensorsignal" /></td>
                <td class="AttribName">Zielerfassung</td>
                <td class="AttribValue"><xsl:call-template name="GetProgramRating"><xsl:with-param name="ProgramName">Zielerfassung</xsl:with-param></xsl:call-template></td>
              </tr>
              <tr>
                <td class="AttribName">Gerätestufe</td>
                <td class="AttribValue"><xsl:value-of select="devicerating" /></td>
                <td class="AttribName">Analyse</td>
                <td class="AttribValue"><xsl:call-template name="GetProgramRating"><xsl:with-param name="ProgramName">Analyse</xsl:with-param></xsl:call-template></td>
              </tr>
              <tr>
                <td class="AttribName">Beschleunigung</td>
                <td class="AttribValue"><xsl:value-of select="accel" /></td>
                <td class="AttribName">Verschlüsselung</td>
                <td class="AttribValue"><xsl:call-template name="GetProgramRating"><xsl:with-param name="ProgramName">Verschlüsselung</xsl:with-param></xsl:call-template></td>
              </tr>
              <tr>
                <td class="AttribName">Geschwindigkeit</td>
                <td class="AttribValue"><xsl:value-of select="speed" /></td>
                <td class="AttribName">Scan</td>
                <td class="AttribValue"><xsl:call-template name="GetProgramRating"><xsl:with-param name="ProgramName">Scan</xsl:with-param></xsl:call-template></td>
              </tr>
              <tr>
                <td class="AttribName">Handling</td>
                <td class="AttribValue"><xsl:value-of select="handling" /></td>
                <td class="AttribName">Panzerung</td>
                <td class="AttribValue"><xsl:call-template name="GetProgramRating"><xsl:with-param name="ProgramName">Panzerung</xsl:with-param></xsl:call-template></td>
              </tr>
              <tr>
                <td class="AttribName">Fahrzeugwert</td>
                <td class="AttribValue"><xsl:value-of select="cost" /></td>
                <td class="AttribName">ECCM</td>
                <td class="AttribValue"><xsl:call-template name="GetProgramRating"><xsl:with-param name="ProgramName">ECCM</xsl:with-param></xsl:call-template></td>
              </tr>
            </table>
          </td>
          <td>
            <table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborderleft" style="padding-left:3px;">
              <tr>
                <td colspan="2" class="tableborderbottom">
                <!-- Skills Block -->
										<table width="100%" cellspacing="0" cellpadding="0" border="0">
                      <tr><td colspan="4" class="ItemListHeader">Anwendbare Aktionsfertigkeit</td></tr>
											<tr>
												<td width="50%">
													<strong>Fertigkeit</strong>
												</td>
												<td width="20%" style="text-align:center;">
													<strong>Pool</strong>
												</td>
												<td width="10%" style="text-align:center;">
													<strong>Wert</strong>
												</td>
												<td width="20%" style="text-align:center;">
													<strong>Att.</strong>
												</td>
											</tr>
											<xsl:call-template name="VehicleSkills">
                        <xsl:with-param name="Category"><xsl:value-of select="category" /></xsl:with-param>
                      </xsl:call-template>
										</table>
                <!-- End Skills Block -->
                </td>
              </tr>
              <tr>
                <td>
			<xsl:if test="mods/mod">  <!-- Vehicle Mods Section -->
                  <table class="ItemList">
                    <tr><td class="ItemListHeader">Modifikationen</td></tr>
            <xsl:for-each select="mods/mod">
              <xsl:sort select="name" />
                    <tr><td>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
                      <xsl:value-of select="name" />
                      <xsl:if test="rating != 0"> Stufe <xsl:value-of select="rating" /></xsl:if>
                    </td></tr>
            </xsl:for-each>
                  </table>
      </xsl:if>
                </td>
                <td>
		  <xsl:if test="gears/gear"> <!-- Vehicle Gear Section -->
                  <table class="ItemList">
                    <tr><td class="ItemListHeader">Ausrüstung</td></tr>
            <xsl:for-each select="gears/gear">
              <xsl:sort select="name" />
                    <tr><td>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
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
                          Stufe <xsl:value-of select="rating" />
                          </xsl:if>
                          <xsl:if test="children/gear">
                            [<xsl:for-each select="children/gear">
                              <xsl:sort select="name" />
                              <xsl:value-of select="name" />
                              <xsl:if test="rating != 0"><xsl:text> </xsl:text><xsl:value-of select="rating" /></xsl:if>
                              <xsl:if test="extra != ''"> (<xsl:value-of select="extra" />)</xsl:if>
                              <xsl:if test="position() != last()">, </xsl:if>
                            </xsl:for-each>]
                          </xsl:if>
                          <xsl:if test="position() != last()">, </xsl:if>
                        </xsl:for-each>)
                      </xsl:if>
                    </td></tr>
            </xsl:for-each>
                  </table>
      </xsl:if>  <!-- End Vehicle Gear Section -->
                </td>
              </tr>
            </table>
          </td>
        </tr>
      </table>
    </td>
    <!--td rowspan="2">
    **DMG
    </td-->
  </tr>
  <tr>
    <td colspan="2">
      <!-- Vehicle Ranged Weapon Section -->
      <xsl:if test="mods/mod/weapons/weapon[type = 'Ranged'] or weapons/weapon[type = 'Ranged']">
        <tr>
          <td>
										<table width="100%" cellspacing="0" cellpadding="0" border="0" class="tableborder">
											<tr>
												<td width="20%">
													<strong>Waffe</strong>
												</td>
												<td width="15%" style="text-align:center;">
													<strong>Schaden</strong>
												</td>
												<td width="12%" style="text-align:center;">
													<strong>PB</strong>
												</td>
												<td width="13%" style="text-align:center;">
													<strong>Modus</strong>
												</td>
												<td width="12%" style="text-align:center;">
													<strong>RK</strong>
												</td>
												<td width="15%" style="text-align:center;">
													<strong>Muni</strong>
												</td>
												<td width="13%" style="text-align:center;">
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
												<td class="rowsummary" colspan="7">
													Fernkampfwaffen
												</td>
											</tr>
										</table>
          </td>
        </tr>
      </xsl:if>  <!-- End Vehicle Ranged Weapons Section -->
    </td>
  </tr>
  <xsl:if test="notes != ''">
	<tr>
		<td colspan="2">
			<p><br /><strong>Notizen: </strong><xsl:value-of select="notes" /><br /><br /></p>
		</td>
	</tr>
  </xsl:if>
  <tr>
    <td colspan="2">
      <xsl:call-template name="ConditionMonitor">
        <xsl:with-param name="PenaltyBox"><xsl:value-of select="physicalcm" /></xsl:with-param>
		<xsl:with-param name="Offset">0</xsl:with-param>
        <xsl:with-param name="CMWidth">24</xsl:with-param>
        <xsl:with-param name="TotalBoxes"><xsl:value-of select="physicalcm" /></xsl:with-param>
        <xsl:with-param name="DamageTaken"><xsl:value-of select="physicalcmfilled" /></xsl:with-param>
        <xsl:with-param name="OverFlow">0</xsl:with-param>
      </xsl:call-template>
    </td>
  </tr>
  <tr>
    <td colspan="2">
      <table width="100%" cellspacing="0" cellpadding="0" border="0">
        <tr>
          <td class="rowsummary">
            Fahrzeug/Drohne
          </td>
          <td class="rowsummarybutton" colspan="1">
             <xsl:attribute name="onClick">showhide(this,'<xsl:value-of select="$VehicleNumber" />');</xsl:attribute>
             Zeigen: JA
          </td>
          <td class="rowsummarybutton" width="50%" colspan="1">
             <xsl:attribute name="onClick">zalomit(this,'<xsl:value-of select="$VehicleNumber" />');</xsl:attribute>
             Seitenumbruch: NEIN
          </td>
        </tr>
      </table>
    </td>
  </tr>
</table>
<table width="100%" cellspacing="0" cellpadding="0" border="0">
  <tr>
    <td class="hseparator" />
  </tr>
</table>
				</div>
	</xsl:template>
 

<!-- This is the Ranged Weapon Template from the main xslt.  -->
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
				<xsl:if test="accessories/accessory or mods/weaponmod">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="7" class="indent">
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
										Stufe <xsl:value-of select="rating" />
										</xsl:if>
										<xsl:if test="position() != last()">, </xsl:if>
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
									<td style="text-align:center;" valign="top">K: <xsl:value-of select="ranges/short" /></td>
									<td style="text-align:center;" valign="top">M: <xsl:value-of select="ranges/medium" /></td>
									<td style="text-align:center;" valign="top">W: <xsl:value-of select="ranges/long" /></td>
									<td style="text-align:center;" valign="top">E: <xsl:value-of select="ranges/extreme" /></td>
									<td colspan="2"></td>
								</tr>
				</xsl:if>

				<xsl:if test="underbarrel/weapon">
											<tr>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
												<td width="20%" valign="top">
													Unt. <xsl:value-of select="underbarrel/weapon/name" />
													<xsl:if test="underbarrel/weapon/weaponname != ''">
														("<xsl:value-of select="underbarrel/weapon/weaponname" />")
													</xsl:if>
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:value-of select="underbarrel/weapon/damage" />
												</td>
												<td width="12%" style="text-align:center;" valign="top">
													<xsl:value-of select="underbarrel/weapon/ap" />
												</td>
												<td width="13%" style="text-align:center;" valign="top">
													<xsl:value-of select="underbarrel/weapon/mode" />
												</td>
												<td width="12%" style="text-align:center;" valign="top">
													<xsl:value-of select="underbarrel/weapon/rc" />
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:value-of select="underbarrel/weapon/ammo" />
												</td>
												<td width="13%" style="text-align:center;" valign="top">
													<xsl:value-of select="underbarrel/weapon/source" /><xsl:text> </xsl:text><xsl:value-of select="underbarrel/weapon/page" />
												</td>
											</tr>
				<xsl:if test="underbarrel/weapon/accessories/accessory">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td colspan="7" class="indent">
					<xsl:for-each select="underbarrel/weapon/accessories/accessory">
						<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="position() != last()">, </xsl:if>
					</xsl:for-each>
					<xsl:if test="underbarrel/weapon/accessories/accessory and mods/weaponmod">, </xsl:if>
					<xsl:for-each select="underbarrel/weapon/mods/weaponmod">
						<xsl:sort select="name" />
										<xsl:value-of select="name" />
										<xsl:if test="rating > 0">
										Stufe <xsl:value-of select="rating" />
										</xsl:if>
										<xsl:if test="position() != last()">, </xsl:if>
					</xsl:for-each>
									</td>
								</tr>
				</xsl:if>
				<xsl:if test="underbarrel/weapon/ranges/short != ''">
								<tr>
									<xsl:if test="position() mod 2 != 1">
										<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
									</xsl:if>
									<td></td>
									<td style="text-align:center;" valign="top">K: <xsl:value-of select="underbarrel/weapon/ranges/short" /></td>
									<td style="text-align:center;" valign="top">M: <xsl:value-of select="underbarrel/weapon/ranges/medium" /></td>
									<td style="text-align:center;" valign="top">W: <xsl:value-of select="underbarrel/weapon/ranges/long" /></td>
									<td style="text-align:center;" valign="top">E: <xsl:value-of select="underbarrel/weapon/ranges/extreme" /></td>
									<td colspan="2"></td>
								</tr>
				</xsl:if>
				</xsl:if>
	</xsl:template>
  
  
<!--
  **** VehicleSkills(Category)
    Given a Category(e.g.: Trucks,Cars,Bikes,etc.), this builds a list of Skills that might be applicable to this Vehicle Category
    (Stored in the temporary variable "SkillStack"), then uses taht list to fill in the Skills Grid.
-->  
<xsl:template name="VehicleSkills">
  <xsl:param name="Category" />
  <xsl:variable name="SkillStack">
    <xsl:choose>
      <xsl:when test="category = 'Motorräder'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Bodenfahrzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fahrzeugmechanik</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Autos'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Bodenfahrzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fahrzeugmechanik</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'LKWs'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Bodenfahrzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fahrzeugmechanik</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Hovercrafts'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Flugzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Bodenfahrzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Schiffe</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Luftfahrtmechanik</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Wasserfahrzeuge'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Schiffe</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Seefahrtmechanik</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Gleiter und EPLFs'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Flugzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Luftfahrtmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fallschirmspringen</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Tiefflieger'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Flugzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Luftfahrtmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fallschirmspringen</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Militär-, Sicherheits- und Krankenfahrzeuge'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Raumfahrzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Flugzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Läufer</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Bodenfahrzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Schiffe</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Luftfahrtmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fahrzeugmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Seefahrtmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fallschirmspringen</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Boote und U-Boote'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Schiffe</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Seefahrtmechanik</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Flugzeuge'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Flugzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Luftfahrtmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fallschirmspringen</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Rotormaschinen'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Flugzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Luftfahrtmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fallschirmspringen</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Zeppeline'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Flugzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Luftfahrtmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fallschirmspringen</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Mikrodrohnen'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Flugzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Läufer</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Bodenfahrzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Schiffe</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Luftfahrtmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fahrzeugmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Seefahrtmechanik</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Minidrohnen'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Flugzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Läufer</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Bodenfahrzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Schiffe</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Luftfahrtmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fahrzeugmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Seefahrtmechanik</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Kleine Drohnen'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Flugzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Läufer</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Bodenfahrzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Schiffe</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Luftfahrtmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fahrzeugmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Seefahrtmechanik</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Mittlere Drohnen'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Flugzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Läufer</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Bodenfahrzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Schiffe</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Luftfahrtmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fahrzeugmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Seefahrtmechanik</xsl:with-param></xsl:call-template>
      </xsl:when>
      <xsl:when test="category = 'Große Drohnen'">
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Flugzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Läufer</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Bodenfahrzeuge</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Schiffe</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Luftfahrtmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Fahrzeugmechanik</xsl:with-param></xsl:call-template>
        <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Seefahrtmechanik</xsl:with-param></xsl:call-template>
      </xsl:when>
    </xsl:choose>
    <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Geschütze</xsl:with-param></xsl:call-template>
    <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Navigation</xsl:with-param></xsl:call-template>
    <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Hardware</xsl:with-param></xsl:call-template>
    <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Elektronische Kriegsführung</xsl:with-param></xsl:call-template>
  </xsl:variable>
  <xsl:for-each select="msxsl:node-set($SkillStack)/skill">
    <xsl:sort select="name" />
											<tr>
                        <xsl:if test="position() mod 2 != 1">
                          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
                        </xsl:if>
												<td width="50%" valign="top">
													<xsl:value-of select="name" />
													<xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="total" />
													<xsl:if test="spec != '' and exotic = 'False'"> (<xsl:value-of select="total + 2" />)</xsl:if>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="rating" />
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="attributemod" /> (<xsl:value-of select="displayattribute" />)
												</td>
											</tr>
  </xsl:for-each>
</xsl:template>

<!--
  **** SkillPull(SkillName)
    Given a Skill's Name, this tempalte returns that skill's node from the currecnt vehicle's character's skill list.
-->
<xsl:template name="SkillPull">
  <xsl:param name="SkillName" />
  <xsl:for-each select="../../skills/skill[name = $SkillName and (default = 'True' or (rating &gt; 0 and default = 'False'))]">
		<xsl:copy-of select="current()"/>
  </xsl:for-each>
</xsl:template>

<!--
  **** GetProgramRating(ProgramName)
    Given a particular Matrix Program, or Autosoft name, this template will search through this vehicle's gear, and gear mods,
      and this character's gear and gear mods to find the highgest Program Rating available.
-->  
<xsl:template name="GetProgramRating">
  <xsl:param name="ProgramName" />
  <xsl:variable name="ProgramStack">
    <xsl:for-each select="../../gears/gear[name = $ProgramName and (isprogram = 'True')]">
      <xsl:copy-of select="current()"/>
    </xsl:for-each>
    <xsl:for-each select="../../gears/gear/children/gear[name = $ProgramName and (isprogram = 'True')]">
      <xsl:copy-of select="current()"/>
    </xsl:for-each>
    <xsl:for-each select="gears/gear[name = $ProgramName and (isprogram = 'True')]">
      <xsl:copy-of select="current()"/>
    </xsl:for-each>
    <xsl:for-each select="gears/gear/children/gear[name = $ProgramName and category = (isprogram = 'True')]">
      <xsl:copy-of select="current()"/>
    </xsl:for-each>
  </xsl:variable>
  <xsl:for-each select="msxsl:node-set($ProgramStack)/gear">
  <xsl:sort select="rating" order="descending" />
      <xsl:if test="position() = 1">
        <xsl:value-of select="rating" />
      </xsl:if>
  </xsl:for-each>
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