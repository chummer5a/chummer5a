<?xml version="1.0" encoding="UTF-8" ?>
<!-- Calendar -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="xs.fnx.xslt"/>
  <xsl:include href="xs.TitleName.xslt"/>

  <xsl:include href="xt.Calendar.xslt"/>
  <xsl:include href="xt.PreserveLineBreaks.xslt"/>
  <xsl:include href="xt.RuleLine.xslt"/>

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
            text-align: center;
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
        <div id="calendarblock">
          <table class="tablestyle">
            <tr>
              <th style="width: 30%">
                <xsl:value-of select="$lang.Date"/>
              </th>
              <th style="width: 70%; text-align: left;">
                <xsl:value-of select="$lang.Notes"/>
              </th>
            </tr>
            <xsl:call-template name="Calendar"/>
          </table>
        </div>
      </body>
    </html>
  </xsl:template>

</xsl:stylesheet>