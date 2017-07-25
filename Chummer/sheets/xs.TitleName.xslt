<?xml version="1.0" encoding="UTF-8" ?>
<!-- Determine the name to be used as the sytlesheet title -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:template name="TitleName">
      <xsl:param name="name" select="''"/>
      <xsl:param name="alias" select="''"/>
      <xsl:param name="PreferName" select="'N'"/>
      <xsl:param name="Default" select="$lang.UnnamedCharacter"/>
    <xsl:choose>
      <xsl:when test="$PreferName != 'Y'"> 
        <!-- use alias, if present, in preference to character name -->
        <xsl:choose>
          <xsl:when test="$alias != ''">
            <xsl:value-of select="$alias"/>
          </xsl:when>
          <xsl:when test="$name != ''">
            <xsl:value-of select="$name"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$Default"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$alias != ''">
        <!-- use character name, if present, instead of alias -->
        <xsl:choose>
          <xsl:when test="$name = '' or $name = $Default">
            <xsl:value-of select="$alias"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$name"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <!-- use character name, unless blank when default is used -->
        <xsl:choose>
          <xsl:when test="$name != ''">
            <xsl:value-of select="$name"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$Default"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>