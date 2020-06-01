<?xml version="1.0" encoding="UTF-8" ?>
<!-- Test Value templates -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- fnx-damage : translate damage code to literal -->
  <xsl:template name="fnx-damage">
      <xsl:param name="code"/>
    <xsl:choose>
      <xsl:when test="$code = $lang.tstDamage1">
        <xsl:value-of select="$lang.Physical"/>
      </xsl:when>
      <xsl:when test="$code = $lang.tstDamage2">
        <xsl:value-of select="$lang.Stun"/>
      </xsl:when>
      <xsl:when test="$code = '0'">-</xsl:when>
      <xsl:when test="$code = $lang.None">-</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$code"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

<!-- fnx-damage : translate duration code to literal -->
  <xsl:template name="fnx-duration">
      <xsl:param name="code"/>
    <xsl:choose>
      <xsl:when test="$code = $lang.tstDuration1">
        <xsl:value-of select="$lang.Instantaneous"/>
      </xsl:when>
      <xsl:when test="$code = $lang.tstDuration2">
        <xsl:value-of select="$lang.Permanent"/>
      </xsl:when>
      <xsl:when test="$code = $lang.tstDuration3">
        <xsl:value-of select="$lang.Sustained"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$code"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

<!-- fnx-damage : translate range code to literal -->
  <xsl:template name="fnx-range">
      <xsl:param name="code"/>
    <xsl:choose>
      <xsl:when test="$code = $lang.tstRange1">
        <xsl:value-of select="$lang.Touch"/>
      </xsl:when>
      <xsl:when test="$code = $lang.tstRange8 or range = $lang.tstRange9">
        <xsl:value-of select="$lang.Touch"/> (<xsl:value-of select="$lang.Area"/>)
      </xsl:when>
      <xsl:when test="$code = $lang.tstRange2">
        <xsl:value-of select="$lang.LineofSight"/>
      </xsl:when>
      <xsl:when test="$code = $lang.tstRange3 or range = $lang.tstRange4">
        <xsl:value-of select="$lang.LineofSight"/> (<xsl:value-of select="$lang.Area"/>)
      </xsl:when>
      <xsl:when test="$code = $lang.tstRange5">
        <xsl:value-of select="$lang.Self"/>
      </xsl:when>
      <xsl:when test="$code = $lang.tstRange6 or range = $lang.tstRange7">
        <xsl:value-of select="$lang.Self"/> (<xsl:value-of select="$lang.Area"/>)
      </xsl:when>
      <xsl:when test="$code = $lang.tstRange10">
        <xsl:value-of select="$lang.Special"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$code"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
