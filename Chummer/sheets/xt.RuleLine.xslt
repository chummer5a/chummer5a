<?xml version="1.0" encoding="UTF-8" ?>
<!-- Produce horizontal blank line -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!--
  ***  Xline(cntl,nte,height)
    Parameters:
      cntl:  Indicates that the last row is being processed
            0 = last row therefore no line printed / non-zero = not last row
      nte:  Indicates if notes are being printed
            True = default to Wide gap / False = use provided height
      height:  Height of line to be printed
            L  Line only (default)
            N  Narrow gap
            R  Regular gap
            W  Wide gap
            X  eXtra wide gap
            Z  Suppress print
  ***  Xline
-->
  <xsl:template name="Xline">
      <xsl:param name="cntl" select="'1'"/>
      <xsl:param name="nte" select="false()"/>
      <xsl:param name="height" select="'L'"/>

    <xsl:variable name="ht">
      <xsl:choose>
        <xsl:when test="$cntl = '0'">Z</xsl:when>
        <xsl:when test="$nte">R</xsl:when>
        <xsl:when test="contains('LNRWXZ',$height)">
          <xsl:value-of select="$height"/>
        </xsl:when>
        <xsl:when test="contains('lnrwxz',$height)">
          <xsl:call-template name="fnx-uc">
            <xsl:with-param name="string" select="$height"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="fnx-vet-nmbr">
            <xsl:with-param name="nmbr" select="$height"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="cnt">
      <xsl:choose>
        <xsl:when test="$ht = 'L'">1</xsl:when>
        <xsl:when test="$ht = 'N'">2</xsl:when>
        <xsl:when test="$ht = 'R'">4</xsl:when>
        <xsl:when test="$ht = 'W'">6</xsl:when>
        <xsl:when test="$ht = 'X'">8</xsl:when>
        <xsl:when test="$ht = 'Z'">0</xsl:when>
        <xsl:when test="$ht = 'NaN'">0</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="substring-before($ht,'.')"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="RuleLine">
      <xsl:with-param name="count" select="$cnt"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="RuleLine">
    <xsl:param name="count" select="1"/>

    <xsl:if test="$count > 0">
      <tr><td colspan="100%"><hr style="
        background: inherit;
        border: none;
        color: white;
        display: block;
        font-size: 0;
        height: 1px;
        line-height: 0;
        margin: -1px 0;
        padding: 0;
        width: 100%;
      "/></td></tr>
      <xsl:call-template name="RuleLine">
        <xsl:with-param name="count" select="$count - 1"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>