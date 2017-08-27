<?xml version="1.0" encoding="UTF-8" ?>
<!-- Display the mugshot on the character sheet -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template name="MugShot">
    <xsl:if test="mainmugshotbase64 != ''">
      <tr>
        <td colspan="5" style="text-align:center;">
          <br />
          <img src="data:image/png;base64,{mainmugshotbase64}"/>
        </td>
      </tr>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>