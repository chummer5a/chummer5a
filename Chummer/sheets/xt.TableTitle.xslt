<?xml version="1.0" encoding="UTF-8" ?>
<!-- Format table name for use in printed character sheet -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<!--
  ***  TableTitle(name)
    Parameters:
      name:  The name of table.
  ***  TableTitle
-->
  <xsl:template name="TableTitle">
      <xsl:param name="name" select="''"/>

    <style type="text/css">
      .tabletitle {
        border-collapse: collapse;
        border-color: #1c4a2d;
        border-style: solid;
        border-width: 0.5mm 0.5mm 0 0.5mm;
        font-size: 125%;
        font-weight: bold;
        page-break-after: avoid;
        page-break-before: auto;
        page-break-inside: avoid;
        text-align: center;
        vertical-align: bottom;
        width: 100%;
      }
    </style>
    <style media="print">
      .tabletitle {
        visibility: visible;
      }
    </style>
    <style media="screen">
      .tabletitle {
        display: none;
        visibility: hidden;
      }
    </style>

    <table class="tabletitle">
      <tr><td colspan="100%" style="text-align: center; font-size: 125%;">
        <xsl:value-of select="$name"/>
      </td></tr>
    </table>

  </xsl:template>
</xsl:stylesheet>