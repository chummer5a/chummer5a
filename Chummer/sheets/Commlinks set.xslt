<?xml version="1.0" encoding="UTF-8" ?>
<!-- Character sheet for Commlinks based on the ones created by ChinaGreenElvis -->
<!-- Created by Keith Rudolph, krudolph@gmail.com -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:include href="xs.fnx.xslt"/>
  <xsl:include href="xs.TitleName.xslt"/>

  <xsl:include href="xt.ConditionMonitor.xslt"/>
  <xsl:include href="xt.PreserveLineBreaks.xslt"/>

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
            font-family: segoe condensed, tahoma, trebuchet ms, arial;
            font-size: 8pt;
            margin: 0;
            text-align: left;
          }
          html {
            height: 100%;
            margin: 0px;  /* this affects the margin on the html before sending to printer */
          }
          .commlinkcategory {
            font-family: segoe condensed, tahoma, trebuchet ms, arial;
            font-size: 8pt;
            text-align: right;
          }
          .tableborder {
            border: solid 2px #1c4a2d;
            border-collapse: collapse;
            padding: 5px;
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
        <xsl:for-each select="gears/gear[iscommlink = 'True' or isnexus = 'True']">
          <xsl:call-template name="commlink">
            <xsl:with-param name="rendercommlink"><xsl:value-of select="."/></xsl:with-param>
          </xsl:call-template>
        </xsl:for-each>
        <xsl:for-each select="armors/armor/gears/gear[iscommlink = 'True' or isnexus = 'True']">
          <xsl:call-template name="commlink">
            <xsl:with-param name="rendercommlink"><xsl:value-of select="."/></xsl:with-param>
          </xsl:call-template>
        </xsl:for-each>
        <xsl:for-each select="cyberwares/cyberware/gears/gear[iscommlink = 'True' or isnexus = 'True']">
          <xsl:call-template name="commlink">
            <xsl:with-param name="rendercommlink"><xsl:value-of select="."/></xsl:with-param>
          </xsl:call-template>
        </xsl:for-each>
        <xsl:for-each select="cyberwares/cyberware/children/cyberware/gears/gear[iscommlink = 'True' or isnexus = 'True']">
          <xsl:call-template name="commlink">
            <xsl:with-param name="rendercommlink"><xsl:value-of select="."/></xsl:with-param>
          </xsl:call-template>
        </xsl:for-each>
        <xsl:for-each select="weapons/weapon/accessories/accessory/gears/gear[iscommlink = 'True' or isnexus = 'True']">
          <xsl:call-template name="commlink">
            <xsl:with-param name="rendercommlink"><xsl:value-of select="."/></xsl:with-param>
          </xsl:call-template>
        </xsl:for-each>
      </body>
    </html>
  </xsl:template>
  
  <xsl:template name="commlink">
      <xsl:param name="rendercommlink"/>
    <div class="block" id="PersonalDataBlock">
      <table width="650px" cellspacing="0" cellpadding="0" border="0">
        <tr>
          <td width="100%" class="tableborder" colspan="4">
            <table width="100%" border="0">
              <tr>
                <td width="50%">
                  <strong>
                  <xsl:value-of select="name"/>
                  <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
                  </strong>
                </td>
                <td width="50%" align="right" text-align="right">
                  <div align="right" text-align="right" class="commlinkcategory">
                    <strong>
                      <xsl:value-of select="category"/>
                    </strong>
                  </div>
                </td>
              </tr>
            </table>
          </td>
        </tr>
        <tr>
          <td width="25%" class="tableborder">
            <strong><xsl:value-of select="$lang.LinkedSIN"/></strong>
          </td>
          <td width="75%" class="tableborder" colspan="3">
            <xsl:value-of select="children/gear[issin = 'True']/extra"/>&#160;
          </td>
        </tr>
        <tr>
          <td width="25%" class="tableborder">
            <strong><xsl:value-of select="$lang.Model"/></strong>
          </td>
          <td width="75%" class="tableborder" colspan="3">
            <xsl:value-of select="name"/>
            <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>&#160;
          </td>
        </tr>
        <tr>
          <td width="25%" class="tableborder">
            <strong><xsl:value-of select="$lang.Attack"/></strong>
          </td>
          <td width="25%" class="tableborder">
            <xsl:choose>
              <xsl:when test="isnexus = 'True'">
                <xsl:value-of select="children/gear[contains(name, 'Response')]/name"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="attack"/>
              </xsl:otherwise>
            </xsl:choose>
          </td>
          <td width="50%" colspan="2" rowspan="6" valign="top" class="tableborder">
            <xsl:choose>
              <xsl:when test="ispersona = 'True'">
                <table width="100%">
                  <tr>
                    <td width="50%" style="text-align:center;">
                      <strong><xsl:value-of select="concat($lang.PhysicalTrack1,' ',$lang.PhysicalTrack2)"/></strong>
                    </td>
                    <td width="50%" style="text-align:center;">
                      <strong><xsl:value-of select="concat($lang.StunTrack1,' ',$lang.StunTrack2)"/></strong>
                    </td>
                  </tr>
                  <tr>
                    <td width="50%" valign="top">
                      <xsl:call-template name="ConditionMonitor">
                        <xsl:with-param name="PenaltyBox"><xsl:value-of select="../../cmthreshold"/></xsl:with-param>
                        <xsl:with-param name="Offset"><xsl:value-of select="../../cmthresholdoffset"/></xsl:with-param>
                        <xsl:with-param name="CMWidth">3</xsl:with-param>
                        <xsl:with-param name="TotalBoxes"><xsl:value-of select="../../physicalcm"/></xsl:with-param>
                        <xsl:with-param name="DamageTaken"><xsl:value-of select="../../physicalcmfilled"/></xsl:with-param>
                        <xsl:with-param name="OverFlow"><xsl:value-of select="../../cmoverflow"/></xsl:with-param>
                      </xsl:call-template>
                    </td>
                    <td widht="50%" valign="top">
                      <xsl:call-template name="ConditionMonitor">
                        <xsl:with-param name="PenaltyBox"><xsl:value-of select="../../cmthreshold"/></xsl:with-param>
                        <xsl:with-param name="Offset"><xsl:value-of select="../../cmthresholdoffset"/></xsl:with-param>
                        <xsl:with-param name="CMWidth">3</xsl:with-param>
                        <xsl:with-param name="TotalBoxes"><xsl:value-of select="../../stuncm"/></xsl:with-param>
                        <xsl:with-param name="DamageTaken"><xsl:value-of select="../../stuncmfilled"/></xsl:with-param>
                        <xsl:with-param name="OverFlow">0</xsl:with-param>
                      </xsl:call-template>
                    </td>
                  </tr>
                </table>
              </xsl:when>
              <xsl:otherwise>
                <strong><xsl:value-of select="$lang.ConditionMonitor"/></strong>
                <xsl:call-template name="ConditionMonitor">
                  <xsl:with-param name="PenaltyBox">3</xsl:with-param>
                  <xsl:with-param name="Offset">0</xsl:with-param>
                  <xsl:with-param name="CMWidth">3</xsl:with-param>
                  <xsl:with-param name="TotalBoxes"><xsl:value-of select="conditionmonitor"/></xsl:with-param>
                  <xsl:with-param name="DamageTaken">0</xsl:with-param>
                  <xsl:with-param name="OverFlow">0</xsl:with-param>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </td>
        </tr>
        <tr>
          <td width="25%" class="tableborder">
            <strong><xsl:value-of select="$lang.Sleaze"/></strong>
          </td>
          <td width="25%" class="tableborder">
            <xsl:choose>
              <xsl:when test="isnexus = 'True'">
                <xsl:value-of select="children/gear[contains(name, 'System')]/name"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="sleaze"/>
              </xsl:otherwise>
            </xsl:choose>
          </td>
        </tr>
        <tr>
          <td width="25%" class="tableborder">
            <strong><xsl:value-of select="$lang.DataProcessing"/></strong>
          </td>
          <td width="25%" class="tableborder">
            <xsl:choose>
              <xsl:when test="isnexus = 'True'">
                <xsl:value-of select="children/gear[contains(name, 'Signal')]/name"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="dataprocessing"/>
              </xsl:otherwise>
            </xsl:choose>
          </td>
        </tr>
        <tr>
          <td width="25%" class="tableborder">
            <strong><xsl:value-of select="$lang.Firewall"/></strong>
          </td>
          <td width="25%" class="tableborder">
            <xsl:choose>
              <xsl:when test="isnexus = 'True'">
                <xsl:value-of select="children/gear[contains(name, 'Firewall')]/name"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="firewall"/>
              </xsl:otherwise>
            </xsl:choose>
          </td>
        </tr>
        <tr>
          <td width="25%" class="tableborder">
            <xsl:choose>
              <xsl:when test="isnexus = 'True'">
                <strong><xsl:value-of select="concat($lang.Persona,' ',$lang.Limit)"/></strong>
              </xsl:when>
              <xsl:otherwise>
                <strong><xsl:value-of select="concat($lang.Processor,' ',$lang.Limit)"/></strong>
              </xsl:otherwise>
            </xsl:choose>
          </td>
          <td width="25%" class="tableborder">
            <xsl:choose>
              <xsl:when test="isnexus = 'True'">
                <xsl:value-of select="children/gear[contains(name, 'Persona Limit')]/name"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="processorlimit"/>
              </xsl:otherwise>
            </xsl:choose>
          </td>
        </tr>
        <tr>
          <td width="50%" class="tableborder" colspan="2">
            <strong><xsl:value-of select="$lang.Programs"/>/<xsl:value-of select="$lang.Data"/></strong>
          </td>
        </tr>
        <tr>
          <td colspan="4" class="tableborder">
            <xsl:for-each select="children/gear">
              <xsl:sort select="name"/>
              <xsl:value-of select="name"/>
              <xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="rating"/></xsl:if>
              <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
              <xsl:if test="children/gear">
                [<xsl:for-each select="children/gear">
                  <xsl:sort select="name"/>
                  <xsl:value-of select="name"/>
                  <xsl:if test="rating != 0"><xsl:text> </xsl:text><xsl:value-of select="rating"/></xsl:if>
                  <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
                  <xsl:if test="position() != last()">; </xsl:if>
                </xsl:for-each>]
              </xsl:if>
              <xsl:if test="position() != last()">; </xsl:if>
            </xsl:for-each>
            <xsl:if test="ispersona = 'True'">
              <xsl:for-each select="../../techprograms/techprogram">
                <xsl:sort select="name"/>
                <xsl:value-of select="name"/>
                <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
                <xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="rating"/></xsl:if>
                <xsl:if test="programoptions/programoption">
                  [<xsl:for-each select="programoptions/programoption">
                    <xsl:sort select="name"/>
                    <xsl:value-of select="name"/>
                    <xsl:if test="rating &gt; 0"><xsl:text> </xsl:text><xsl:value-of select="rating"/></xsl:if>
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
              <xsl:value-of select="notes"/>
            </td>
          </tr>
        </xsl:if>
      </table>
    </div>
    <br/>
  </xsl:template>
</xsl:stylesheet>