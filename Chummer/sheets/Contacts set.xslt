<?xml version="1.0" encoding="UTF-8" ?>
<!-- Contacts List -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="xs.Chummer5CSS.xslt"/>
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
        <meta charset="UTF-8" />
        <xsl:call-template name="Chummer5CSS" />
      </head>

      <body>
        <div id="ContactsBlock">
          <table class="tablestyle">
            <tr class="title" style="font-weight: bold; text-decoration: underline;">
              <th width="25%" style="text-align: left">
                <xsl:value-of select="$lang.Name"/>
              </th>
              <th width="25%"><xsl:value-of select="$lang.Location"/></th>
              <th width="25%"><xsl:value-of select="$lang.Archetype"/></th>
              <th width="15%"><xsl:value-of select="$lang.Connection"/></th>
              <th width="10%"><xsl:value-of select="$lang.Loyalty"/></th>
            </tr>
            <xsl:call-template name="Contacts"/>
          </table>
        </div>
      </body>
    </html>
  </xsl:template>

</xsl:stylesheet>
