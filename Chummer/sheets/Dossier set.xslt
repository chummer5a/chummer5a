<?xml version="1.0" encoding="UTF-8" ?>
<!-- Dossier character summary sheet -->
<!-- Created by Jeff Halket, modified by Keith Rudolph, krudolph@gmail.com -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="xs.TitleName.xslt"/>

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
            font-family: 'courier new', tahoma, 'trebuchet ms', arial;
            font-size: 9pt;
            margin: 0;
            vertical-align: top;
          }
          html {
            height: 100%;
            margin: 0px;  /* this affects the margin on the html before sending to printer */
          }
          .label {
            font-weight: bold;
            width: 15%;
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
        </style>
      </head>

      <body>
        <div id="DossierBlock">
          <table width="100%" cellspacing="0" cellpadding="2">
            <tr>
              <td class="label"><xsl:value-of select="$lang.Name"/>:</td>
              <td><xsl:value-of select="name"/></td>
              <td rowspan="10" width="40%" align="center">
                <xsl:if test="mainmugshotbase64 != ''">
                  <img src="data:image/png;base64,{mainmugshotbase64}" />
                </xsl:if>
              </td>
            </tr>
            <tr>
              <td class="label"><xsl:value-of select="$lang.Alias"/>:</td>
              <td><xsl:value-of select="alias"/></td>
            </tr>
            <tr>
              <td class="label"><xsl:value-of select="$lang.Metatype"/>:</td>
              <td><xsl:value-of select="metatype"/></td>
            </tr>
            <tr>
              <td class="label"><xsl:value-of select="$lang.Sex"/>:</td>
              <td><xsl:value-of select="sex"/></td>
            </tr>
            <tr>
              <td class="label"><xsl:value-of select="$lang.Height"/>:</td>
              <td><xsl:value-of select="height"/></td>
            </tr>
            <tr>
              <td class="label"><xsl:value-of select="$lang.Weight"/>:</td>
              <td><xsl:value-of select="weight"/></td>
            </tr>
            <tr>
              <td class="label"><xsl:value-of select="$lang.Age"/>:</td>
              <td><xsl:value-of select="age"/></td>
            </tr>
            <tr>
              <td class="label"><xsl:value-of select="$lang.Hair"/>:</td>
              <td><xsl:value-of select="hair"/></td>
            </tr>
            <tr>
              <td class="label"><xsl:value-of select="$lang.Eyes"/>:</td>
              <td><xsl:value-of select="eyes"/></td>
            </tr>
            <tr>
              <td class="label"><xsl:value-of select="$lang.Skin"/>:</td>
              <td><xsl:value-of select="skin"/></td>
            </tr>
            <tr>
              <td class="label"><xsl:value-of select="$lang.PrimaryArm"/>:</td>
              <td><xsl:value-of select="primaryarm"/></td>
            </tr>
            <tr><td colspan="100%" height="15"></td></tr>
            <tr>
              <td class="label"><xsl:value-of select="$lang.Description"/>:</td>
              <td colspan="2" style="text-align: justify">
                <xsl:call-template name="PreserveLineBreaks">
                  <xsl:with-param name="text" select="description" />
                </xsl:call-template>
              </td>
            </tr>
            <tr><td colspan="100%" height="15"></td></tr>
            <tr>
              <td class="label"><xsl:value-of select="$lang.Notes"/>:</td>
              <td colspan="2" style="text-align: justify">
                <xsl:call-template name="PreserveLineBreaks">
                  <xsl:with-param name="text" select="notes" />
                </xsl:call-template>
                <xsl:call-template name="PreserveLineBreaks">
                  <xsl:with-param name="text" select="gamenotes" />
                </xsl:call-template>
              </td>
            </tr>
            <tr><td colspan="100%" height="15"></td></tr>
            <tr>
              <td class="label"><xsl:value-of select="$lang.Background"/>:</td>
              <td colspan="2" style="text-align: justify">
                <xsl:call-template name="PreserveLineBreaks">
                  <xsl:with-param name="text" select="background" />
                </xsl:call-template>
              </td>
            </tr>
          </table>
        </div>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>