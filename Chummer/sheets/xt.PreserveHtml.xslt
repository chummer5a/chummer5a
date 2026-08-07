<?xml version="1.0" encoding="UTF-8" ?>
<!-- Preserve line breaks (newlines -> <br/>). SECURITY: keep escaping ON; do not add disable-output-escaping (output is rendered by MSHTML/wkhtmltopdf). -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template name="PreserveHtml">
      <xsl:param name="text" />
    <xsl:choose>
      <xsl:when test="contains($text,'&#xA;')">
        <xsl:value-of select="substring-before($text,'&#xA;')" />
        <br />
        <xsl:call-template name="PreserveHtml">
          <xsl:with-param name="text">
            <xsl:value-of select="substring-after($text,'&#xA;')" />
          </xsl:with-param>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$text" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
