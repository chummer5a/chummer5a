<?xml version="1.0" encoding="UTF-8" ?>
<!-- Contacts List -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="xs.fnx.xslt"/>
  <xsl:include href="xs.TitleName.xslt"/>

  <xsl:include href="xt.Contacts.xslt"/>
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

    <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
      <head>
        <meta http-equiv="x-ua-compatible" content="IE=Edge"/>
        <style type="text/css">
          * {
            font-family: 'courier new', tahoma, 'trebuchet ms', arial;
            font-size: 10pt;
            margin: 0;
            text-align: center;
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
            width: 100%;
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
        <div id="ContactsBlock">
          <table class="tablestyle">
            <tr style="font-weight: bold; text-decoration: underline; text-transform: uppercase;">
              <td width="25%" style="text-align: left">
                <xsl:value-of select="$lang.Name"/>
              </td>
              <td width="25%">
                <xsl:value-of select="$lang.Location"/>
              </td>
              <td width="25%">
                <xsl:value-of select="$lang.Archetype"/>
              </td>
              <td width="15%">
                <xsl:value-of select="$lang.Connection"/>
              </td>
              <td width="10%">
                <xsl:value-of select="$lang.Loyalty"/>
              </td>
            </tr>
            <xsl:call-template name="Contacts"/>
          </table>
        </div>
      </body>
    </html>
  </xsl:template>

</xsl:stylesheet>