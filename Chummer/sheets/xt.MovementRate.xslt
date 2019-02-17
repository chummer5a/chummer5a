<?xml version="1.0" encoding="UTF-8" ?>
<!-- Produce Row Summary Line -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template name="MovementRate">
    <xsl:variable name="mv1">
      <xsl:call-template name="formatrate">
        <xsl:with-param name="movrate" select="movementwalk"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="mv2">
      <xsl:call-template name="formatrate">
        <xsl:with-param name="movrate" select="movementswim"/>
        <xsl:with-param name="pfx" select="concat($lang.Swim,': ')"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="mv3">
      <xsl:call-template name="formatrate">
        <xsl:with-param name="movrate" select="movementfly"/>
        <xsl:with-param name="pfx" select="concat($lang.Fly,': ')"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select="concat($mv1,$mv2,$mv3)"/>
  </xsl:template>

  <xsl:template name="formatrate">
      <xsl:param name="movrate"/>
      <xsl:param name="pfx" select="''"/>
    <xsl:choose>
      <xsl:when test="contains($movrate,',')">
        <xsl:value-of select="$pfx"/>
        <xsl:variable name="mv">
          <xsl:call-template name="fnx-replace">
            <xsl:with-param name="string" select="$movrate"/>
            <xsl:with-param name="replace" select="'/ hit'"/>
            <xsl:with-param name="by" select="concat('/',$lang.hit,')')"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="fnx-replace">
          <xsl:with-param name="string" select="$mv"/>
          <xsl:with-param name="replace" select="', '"/>
          <xsl:with-param name="by" select="' ('"/>
        </xsl:call-template>
        &#160;&#160;&#160;
      </xsl:when>
      <xsl:when test="$movrate != '0'">
        <xsl:value-of select="$movrate"/>
        &#160;&#160;&#160;
      </xsl:when>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>