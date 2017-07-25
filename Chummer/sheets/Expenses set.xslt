<?xml version="1.0" encoding="UTF-8" ?>
<!-- Expenses -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="xs.fnx.xslt"/>
  <xsl:include href="xs.TitleName.xslt"/>

  <xsl:include href="xt.Expenses.xslt"/>
  <xsl:include href="xt.PreserveLineBreaks.xslt"/>
  <xsl:include href="xt.RowSummary.xslt"/>
  <xsl:include href="xt.TableTitle.xslt"/>

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
            font-size: 10pt;
            margin: 0;
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
            cellpadding: 2;
            cellspacing: 0;
            width: 100%;
          }
          th {
            text-decoration: underline;
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
        <div id="KarmaExpensesBlock">
          <xsl:call-template name="TableTitle">
            <xsl:with-param name="name" select="concat($lang.Karma,' ',$lang.Expenses)"/>
          </xsl:call-template>
          <table class="tablestyle">
            <tr>
              <th width="26%"><xsl:value-of select="$lang.Date"/></th>
              <th width="6%"/>
              <th width="6%"><xsl:value-of select="$lang.Karma"/></th>
              <th width="4%"/>
              <th width="58%" style="text-align: left">
                <xsl:value-of select="$lang.Reason"/>
              </th>
            </tr>
            <xsl:call-template name="Expenses">
              <xsl:with-param name="type" select="$lang.Karma"/>
              <xsl:with-param name="sfx" select="'&#160;&#160;'"/>
            </xsl:call-template>
            <tr>
              <td><xsl:value-of select="$lang.RemainingAvailable"/></td>
              <td/>
              <td style="text-decoration: overline">
                <xsl:call-template name="fnx-fmt-nmbr">
                  <xsl:with-param name="nmbr" select="karma"/>
                  <xsl:with-param name="wdth" select="3"/>
                </xsl:call-template>
              </td>
              <td/>
              <td/>
            </tr>
          </table>
        </div>
        <xsl:call-template name="RowSummary">
          <xsl:with-param name="text" select="concat($lang.Karma,'&#160;',$lang.Expenses)"/>
          <xsl:with-param name="blockname" select="'KarmaExpensesBlock'"/>
        </xsl:call-template>

        <table><tr><td/></tr></table>

        <div id="NuyenExpensesBlock">
          <xsl:call-template name="TableTitle">
            <xsl:with-param name="name" select="concat($lang.Nuyen,' ',$lang.Expenses)"/>
          </xsl:call-template>
          <table class="tablestyle">
            <tr>
              <th width="26%"><xsl:value-of select="$lang.Date"/></th>
              <th width="2%"/>
              <th width="10%" style="text-align: right;">
                <xsl:value-of select="$lang.Nuyen"/>
              </th>
              <th width="4%"/>
              <th width="58%" style="text-align:left;">
                <xsl:value-of select="$lang.Reason"/>
              </th>
            </tr>
            <xsl:call-template name="Expenses">
              <xsl:with-param name="type" select="$lang.Nuyen"/>
            </xsl:call-template>
            <tr>
              <td><xsl:value-of select="$lang.RemainingAvailable"/></td>
              <td/>
              <td style="text-align: right; text-decoration: overline;">
                <xsl:call-template name="fnx-fmt-nmbr">
                  <xsl:with-param name="nmbr" select="nuyen"/>
                  <xsl:with-param name="wdth" select="7"/>
                </xsl:call-template>
              </td>
              <td/>
              <td/>
            </tr>
          </table>
        </div>
        <xsl:call-template name="RowSummary">
          <xsl:with-param name="text" select="concat($lang.Nuyen,'&#160;',$lang.Expenses)"/>
          <xsl:with-param name="blockname" select="'NuyenExpensesBlock'"/>
        </xsl:call-template>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>