<?xml version="1.0" encoding="UTF-8" ?>
<!-- Format Complex Forms list of Character Sheet -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:template name="ComplexForms">
    <tr>
      <th width="40%" style="text-align: left">
        <xsl:value-of select="$lang.ComplexForm" />
      </th>
      <th width="15%" style="text-align: center">
        <xsl:value-of select="$lang.Target" />
      </th>
      <th width="15%" style="text-align: center">
        <xsl:value-of select="$lang.Duration" />
      </th>
      <th width="15%" style="text-align: center">
        <xsl:value-of select="$lang.FV" />
      </th>
      <th width="5%" />
      <th width="10%" />
    </tr>

    <xsl:for-each select="complexforms/complexform">
      <xsl:sort select="fullname" />
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:value-of select="fullname" />
        </td>
        <td style="text-align: center">
          <xsl:value-of select="target" />
        </td>
        <td style="text-align: center">
          <xsl:value-of select="duration" />
        </td>
        <td style="text-align: center">
          <xsl:value-of select="fv" />
        </td>
        <td />
        <td style="text-align: center">
          <xsl:value-of select="source" />
          <xsl:text> </xsl:text>
          <xsl:value-of select="page" />
        </td>
      </tr>
      <xsl:if test="notes != '' and $ProduceNotes">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" class="notesrow2">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes" />
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
      <xsl:call-template name="Xline">
        <xsl:with-param name="cntl" select="last()-position()" />
        <xsl:with-param name="nte" select="notes != '' and $ProduceNotes" />
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
