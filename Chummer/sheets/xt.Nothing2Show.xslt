<?xml version="1.0" encoding="utf-8" ?>
<!-- Format Nothing to Show page -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template name="nothing2show">
      <xsl:param name="namethesheet" select="boolean(false())" />

    <style type="text/css">
      * {
        font-family: 'courier new', tahoma, 'trebuchet ms', arial;
        font-size: 10pt;
        text-align: center;
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
    </style>

    <div>
      <table class="tablestyle">
        <tr><td style="text-align: center">
          <br />
          <br />
          <xsl:value-of select="$namethesheet" />
          <br />
          <br />
          <br />
		</td></tr>
	  </table>
	</div>
  </xsl:template>
</xsl:stylesheet>
