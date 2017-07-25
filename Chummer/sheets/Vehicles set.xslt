<?xml version="1.0" encoding="UTF-8" ?>
<!-- Vehicle sheet based on the Shadowrun 4th Edition Character Sheet -->
<!-- Created by KeyMasterOfGozer -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:include href="xs.fnx.xslt"/>
  <xsl:include href="xs.TitleName.xslt"/>

  <xsl:include href="xt.ConditionMonitor.xslt"/>
  <xsl:include href="xt.PreserveLineBreaks.xslt"/>
  <xsl:include href="xt.RangedWeapons.xslt"/>
  <xsl:include href="xt.RowSummary.xslt"/>
  <xsl:include href="xt.RuleLine.xslt"/>

<!-- Set local control variables if global versions have not already been defined  -->
  <xsl:variable name="ProduceNotes" select="true()"/>
<!-- End of setting local control variables to default local values  -->

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
        <meta http-equiv="x-ua-compatible" content="IE=Edge"/>
        <style type="text/css">
          * {
            font-family: segoe, tahoma, 'trebuchet ms', arial;
            font-size: 9pt;
            margin: 0;
            text-align: left;
            vertical-align: top;
          }
          .tablestyle {
            border-collapse: collapse;
            border-color: #1c4a2d;
            border-style: solid;
            border-width: 2px;
            cellpadding: 0;
            cellspacing: 0;
            width: 100%;
          }
          .tableborderbottom {
            border-collapse: collapse;
            border-bottom: solid 2px #1c4a2d;
            cellpadding: 0;
            cellspacing: 0;
          }
          .tablebordertop {
            border-collapse: collapse;
            border-top: solid 2px #1c4a2d;
            cellpadding: 0;
            cellspacing: 0;
          }
          .tablebordertopleft {
            border-collapse: collapse;
            border-top: solid 2px #1c4a2d;
            border-left: solid 2px #1c4a2d;
            cellpadding: 0;
            cellspacing: 0;
          }
          .tableborderright {
            border-collapse: collapse;
            border-right: solid 2px #1c4a2d;
            cellpadding: 0;
            cellspacing: 0;
          }
          .tableborderleft {
            border-collapse: collapse;
            border-left: solid 2px #1c4a2d;
            cellpadding: 0;
            cellspacing: 0;
          }
          .AttribName {
            text-align: right;
            font-weight: bold;
            padding-right: 3px;
          }
          .AttribValue {
            text-align: left;
            padding-left: 3px;
          }
          .block {
            page-break-inside: avoid;
          }
          .hseparator {
            height: 5px;
          }
          .ItemListHeader {
            text-align: left;
            font-weight: bold;
            text-decoration:underline;
          }
          th {
            text-align: center;
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
        <xsl:for-each select="vehicles/vehicle">
          <xsl:sort select="name"/>
          <xsl:call-template name="vehicles">
            <xsl:with-param name="vehicle"/>
            <xsl:with-param name="VehicleNumber">VehicleBlock<xsl:value-of select="position()"/></xsl:with-param>
          </xsl:call-template>
        </xsl:for-each>
      </body>
    </html>
  </xsl:template>

  <xsl:template name="vehicles">
      <xsl:param name="vehicle"/>
      <xsl:param name="VehicleNumber"/>
    <div class="block">
      <xsl:attribute name="id">
        <xsl:value-of select="$VehicleNumber"/>
      </xsl:attribute>
      <xsl:variable name="cntmods">
        <xsl:if test="mods/mod">
          <xsl:value-of select="count(mods/mod) + 1"/>
        </xsl:if>
      </xsl:variable>
      <xsl:variable name="cntgear">
        <xsl:if test="gears/gear">
          <xsl:value-of select="count(gears/gear) + 1"/>
        </xsl:if>
      </xsl:variable>
      <table class="tablestyle">
        <tr><td>
          <table width="100%" class="tableborderbottom">
            <xsl:choose>
              <xsl:when test="($cntmods + $cntgear) &gt; 7">
                <tr>
                  <td width="40%">
                    <table width="100%">
                      <xsl:call-template name="StatsBlock"/>
                    </table>
                  </td>
                  <td width="60%">
                    <table width="100%" cellpadding="3">
                      <tr><td colspan="100%" class="tableborderleft">
                        <xsl:call-template name="SkillsBlock"/>
                      </td></tr>
                      <xsl:if test="$cntmods &gt; 1">
                        <tr><td colspan="100%" class="tablebordertopleft">
                          <xsl:call-template name="VehicleMods"/>
                        </td></tr>
                      </xsl:if>
                      <xsl:if test="$cntgear &gt; 1">
                        <tr><td colspan="100%" class="tablebordertopleft">
                          <xsl:call-template name="VehicleGear"/>
                        </td></tr>
                      </xsl:if>
                    </table>
                  </td>
                </tr>
              </xsl:when>
              <xsl:otherwise>
                <tr>
                  <td width="40%">
                    <table width="100%" class="tableborderright">
                      <xsl:call-template name="StatsBlock"/>
                    </table>
                  </td>
                  <td width="60%">
                    <table width="100%" cellpadding="3">
                      <tr><td colspan="100%">
                        <xsl:call-template name="SkillsBlock"/>
                      </td></tr>
                      <xsl:if test="$cntmods &gt; 1">
                        <tr><td colspan="100%" class="tablebordertop">
                          <xsl:call-template name="VehicleMods"/>
                        </td></tr>
                      </xsl:if>
                      <xsl:if test="$cntgear &gt; 1">
                        <tr><td colspan="100%" class="tablebordertop">
                          <xsl:call-template name="VehicleGear"/>
                        </td></tr>
                      </xsl:if>
                    </table>
                  </td>
                </tr>
              </xsl:otherwise>
            </xsl:choose>
          </table>
        </td></tr>
        <xsl:if test="mods/mod/weapons/weapon[type = 'Ranged'] or weapons/weapon[type = 'Ranged']">
          <!-- Vehicle Ranged Weapon Section -->
          <tr><td>
            <table width="100%">
              <tr>
                <th width="20%" style="text-align: left">
                  <xsl:value-of select="$lang.Weapon"/>
                </th>
                <th width="8%">
                  <xsl:value-of select="$lang.Pool"/>
                </th>
                <th width="11%">
                  <xsl:value-of select="$lang.Accuracy"/>
                </th>
                <th width="16%">
                  <xsl:value-of select="$lang.Damage"/>
                </th>
                <th width="9%">
                  <xsl:value-of select="$lang.AP"/>
                </th>
                <th width="8%">
                  <xsl:value-of select="$lang.Mode"/>
                </th>
                <th width="4%">
                  <xsl:value-of select="$lang.RC"/>
                </th>
                <th width="7%">
                  <xsl:value-of select="$lang.Ammo"/>
                </th>
                <th width="7%">
                  [<xsl:value-of select="$lang.Loaded"/>]
                </th>
                <th width="10%">
                  <xsl:value-of select="$lang.Source"/>
                </th>
              </tr>
              <xsl:for-each select="mods/mod/weapons/weapon[type = 'Ranged']">
                <xsl:sort select="name"/>
                <xsl:call-template name="RangedWeapons">
                  <xsl:with-param name="weapon" select="weapon"/>
                </xsl:call-template>
              </xsl:for-each>
              <xsl:for-each select="weapons/weapon[type = 'Ranged']">
                <xsl:sort select="name"/>
                <xsl:call-template name="RangedWeapons">
                  <xsl:with-param name="weapon" select="weapon"/>
                </xsl:call-template>
              </xsl:for-each>
            </table>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.RangedWeapons"/>
              <xsl:with-param name="incl" select="'N'"/>
            </xsl:call-template>
          </td></tr>
          <tr><td colspan="100%" class="hseparator"></td></tr>
          <!-- End Vehicle Ranged Weapons Section -->
        </xsl:if>
        <xsl:if test="notes != ''">
          <tr><td colspan="100%">
            <strong><xsl:value-of select="$lang.Notes"/>: </strong>
            <xsl:value-of select="notes"/>
          </td></tr>
          <tr><td colspan="100%" class="hseparator"></td></tr>
        </xsl:if>
        <tr><td colspan="100%" style="padding-left: 1%; padding-right: 1%">
          <xsl:call-template name="ConditionMonitor">
            <xsl:with-param name="PenaltyBox"><xsl:value-of select="physicalcm"/></xsl:with-param>
            <xsl:with-param name="Offset">0</xsl:with-param>
            <xsl:with-param name="CMWidth">24</xsl:with-param>
            <xsl:with-param name="TotalBoxes"><xsl:value-of select="physicalcm"/></xsl:with-param>
            <xsl:with-param name="DamageTaken"><xsl:value-of select="physicalcmfilled"/></xsl:with-param>
            <xsl:with-param name="OverFlow">0</xsl:with-param>
          </xsl:call-template>
        </td></tr>
      </table>
      <xsl:call-template name="RowSummary">
        <xsl:with-param name="text" select="concat($lang.Vehicle,'/',$lang.Drone)"/>
        <xsl:with-param name="name" select="$VehicleNumber"/>
      </xsl:call-template>
      <table><tr><td/></tr></table>
    </div>
  </xsl:template>

  <xsl:template name="StatsBlock">
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.Vehicle"/></td>
        <td class="AttribValue"><xsl:value-of select="name"/></td>
      </tr>
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.Name"/></td>
        <td class="AttribValue"><xsl:value-of select="vehiclename"/></td>
      </tr>
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.Category"/></td>
        <td class="AttribValue"><xsl:value-of select="category"/></td>
      </tr>
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.Source"/></td>
        <td class="AttribValue">
          <xsl:value-of select="source"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="page"/>
        </td>
      </tr>
      <tr><td colspan="100%" class="hseparator"></td></tr>
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.VehicleBody"/></td>
        <td class="AttribValue"><xsl:value-of select="body"/></td>
      </tr>
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.Armor"/></td>
        <td class="AttribValue"><xsl:value-of select="armor"/></td>
      </tr>
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.Seats"/></td>
        <td class="AttribValue"><xsl:value-of select="seats"/></td>
      </tr>
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.Pilot"/></td>
        <td class="AttribValue"><xsl:value-of select="pilot"/></td>
      </tr>
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.Sensor"/></td>
        <td class="AttribValue"><xsl:value-of select="sensor"/></td>
      </tr>
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.DeviceRating"/></td>
        <td class="AttribValue"><xsl:value-of select="devicerating"/></td>
      </tr>
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.Acceleration"/></td>
        <td class="AttribValue"><xsl:value-of select="accel"/></td>
      </tr>
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.Speed"/></td>
        <td class="AttribValue"><xsl:value-of select="speed"/></td>
      </tr>
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.Handling"/></td>
        <td class="AttribValue"><xsl:value-of select="handling"/></td>
      </tr>
      <tr>
        <td class="AttribName"><xsl:value-of select="$lang.VehicleCost"/></td>
        <td class="AttribValue">
          <xsl:call-template name="fnx-fmt-nmbr">
            <xsl:with-param name="nmbr" select="cost"/>
          </xsl:call-template><xsl:value-of select="$lang.NuyenSymbol"/>
        </td>
      </tr>
  </xsl:template>

  <xsl:template name="SkillsBlock">
    <table width="100%">
      <tr>
        <th width="50%" class="ItemListHeader">
          <xsl:value-of select="$lang.Applicable"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="$lang.ActiveSkills"/>
        </th>
        <th width="20%">
          <strong><xsl:value-of select="$lang.Pool"/></strong>
        </th>
        <th width="10%">
          <strong><xsl:value-of select="$lang.Rtg"/></strong>
        </th>
        <th width="20%">
          <strong><xsl:value-of select="$lang.Attribute"/></strong>
        </th>
      </tr>
      <xsl:call-template name="VehicleSkills">
        <xsl:with-param name="Category"><xsl:value-of select="category"/></xsl:with-param>
      </xsl:call-template>
    </table>
  </xsl:template>

  <xsl:template name="VehicleMods">
    <table width="100%">
      <tr><td class="ItemListHeader">
        <xsl:value-of select="$lang.Modifications"/>
      </td></tr>
      <xsl:for-each select="mods/mod">
        <xsl:sort select="name"/>
        <tr><td>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <xsl:value-of select="name"/>
          <xsl:if test="rating != 0">
            (<xsl:value-of select="$lang.Rating"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="rating"/>)
          </xsl:if>
          <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
        </td></tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="VehicleGear">
    <table>
      <tr><td class="ItemListHeader">
        <xsl:value-of select="$lang.Gear"/>
      </td></tr>
      <xsl:for-each select="gears/gear">
        <xsl:sort select="name"/>
        <tr><td>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <xsl:value-of select="name"/>
          <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
          <xsl:if test="rating > 0">
            <xsl:text> </xsl:text>
            <xsl:value-of select="$lang.Rating"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="rating"/>
          </xsl:if>
          <xsl:if test="qty > 1">  x<xsl:value-of select="qty"/></xsl:if>
          <xsl:if test="children/gear">
            (<xsl:for-each select="children/gear">
              <xsl:sort select="name"/>
              <xsl:value-of select="name"/>
              <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
              <xsl:if test="rating > 0">
                <xsl:text> </xsl:text>
                <xsl:value-of select="$lang.Rating"/>
                <xsl:text> </xsl:text>
                <xsl:value-of select="rating"/>
              </xsl:if>
              <xsl:if test="children/gear">
                [<xsl:for-each select="children/gear">
                  <xsl:sort select="name"/>
                  <xsl:value-of select="name"/>
                  <xsl:if test="rating != 0">
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="rating"/>
                  </xsl:if>
                  <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
                  <xsl:if test="position() != last()">, </xsl:if>
                </xsl:for-each>]
              </xsl:if>
              <xsl:if test="position() != last()">, </xsl:if>
            </xsl:for-each>)
          </xsl:if>
        </td></tr>
      </xsl:for-each>
    </table>
  </xsl:template>

<!-- **** VehicleSkills(Category)
    Given a Category(e.g.: Trucks, Cars, Bikes, etc.), builds a list of Skills that
    might be applicable to this category (stored in the temporary variable "SkillStack"),
    then uses the list to fill in the Skills Grid.
-->  
  <xsl:template name="VehicleSkills">
      <xsl:param name="Category"/>
    <xsl:variable name="SkillStack">
      <xsl:choose>
        <xsl:when test="category = 'Bikes'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Ground Craft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Automotive Mechanic</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Cars'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Ground Craft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Automotive Mechanic</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Trucks'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Ground Craft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Automotive Mechanic</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Hovercraft'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Aircraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Ground Craft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Watercraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Aeronautics Mechanic</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Watercraft'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Watercraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Nautical Mechanic</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Gliders and FPMV'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Aircraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Aeronautics Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Parachuting</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'LAVs'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Aircraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Aeronautics Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Parachuting</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Military, Security and Medical Craft'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Aerospace</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Aircraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Walker</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Ground Craft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Watercraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Aeronautics Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Automotive Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Nautical Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Parachuting</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Boats &amp; Subs'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Watercraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Nautical Mechanic</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Winged Planes'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Aircraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Aeronautics Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Parachuting</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Rotorcraft'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Aircraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Aeronautics Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Parachuting</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Zeppelin'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Aircraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Aeronautics Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Parachuting</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Drones: Micro'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Aircraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Walker</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Ground Craft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Watercraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Aeronautics Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Automotive Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Nautical Mechanic</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Drones: Mini'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Aircraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Walker</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Ground Craft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Watercraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Aeronautics Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Automotive Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Nautical Mechanic</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Drones: Small'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Aircraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Walker</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Ground Craft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Watercraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Aeronautics Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Automotive Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Nautical Mechanic</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Drones: Medium'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Aircraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Walker</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Ground Craft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Watercraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Aeronautics Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Automotive Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Nautical Mechanic</xsl:with-param></xsl:call-template>
        </xsl:when>
        <xsl:when test="category = 'Drones: Large'">
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Aircraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Walker</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Ground Craft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Pilot Watercraft</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Aeronautics Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Automotive Mechanic</xsl:with-param></xsl:call-template>
          <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Nautical Mechanic</xsl:with-param></xsl:call-template>
        </xsl:when>
      </xsl:choose>
      <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Gunnery</xsl:with-param></xsl:call-template>
      <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Navigation</xsl:with-param></xsl:call-template>
      <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Hardware</xsl:with-param></xsl:call-template>
      <xsl:call-template name="SkillPull"><xsl:with-param name="SkillName">Electronic Warfare</xsl:with-param></xsl:call-template>
    </xsl:variable>
    <xsl:for-each select="msxsl:node-set($SkillStack)/skill">
      <xsl:sort select="name"/>
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td width="50%" style="text-align: left">
          <xsl:value-of select="name"/>
          <xsl:if test="spec != ''"> (<xsl:value-of select="spec"/>)</xsl:if>
        </td>
        <td width="10%" style="text-align:center;">
          <xsl:value-of select="total"/>
          <xsl:if test="spec != '' and exotic = 'False'"> (<xsl:value-of select="total + 2"/>)</xsl:if>
        </td>
        <td width="10%" style="text-align:center;">
          <xsl:value-of select="rating"/>
        </td>
        <td width="10%" style="text-align:center;">
          <xsl:value-of select="attributemod"/> (<xsl:value-of select="displayattribute"/>)
        </td>
      </tr>
    </xsl:for-each>
  </xsl:template>

<!-- **** SkillPull(SkillName)
    Given a Skill's Name, this template returns that skill's node from
    the current vehicle's character's skill list.
-->
  <xsl:template name="SkillPull">
      <xsl:param name="SkillName"/>
    <xsl:for-each select="../../skills/skill[name = $SkillName and (default = 'True' or (rating &gt; 0 and default = 'False'))]">
      <xsl:copy-of select="current()"/>
    </xsl:for-each>
  </xsl:template>

<!-- **** GetProgramRating(ProgramName)
    Given a particular Matrix Program, or Autosoft name, this template will search through this vehicle's gear, and gear mods,
    and this character's gear and gear mods to find the highgest Program Rating available.
-->
  <xsl:template name="GetProgramRating">
      <xsl:param name="ProgramName"/>
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
      <xsl:sort select="rating" order="descending"/>
      <xsl:if test="position() = 1">
        <xsl:value-of select="rating"/>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>