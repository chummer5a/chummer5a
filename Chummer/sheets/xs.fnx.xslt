<?xml version="1.0" encoding="UTF-8" ?>
<!-- String manipulation templates -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:param name="lowercase" select="'abcdefghijklmnopqrstuvwxyzàèìòùáéíóúýâêîôûãñõäëïöüÿ¥æœçðø'"/>
  <xsl:param name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZÀÈÌÒÙÁÉÍÓÚÝÂÊÎÔÛÃÑÕÄËÏÖÜŸÅÆŒÇÐØ'"/>
  <xsl:param name="en-marks" select="'.,'"/>

<!-- fnx-lc : convert string to all lowercase -->
  <xsl:template name="fnx-lc">
      <xsl:param name="string"/>
    <xsl:value-of select="translate($string, $uppercase, $lowercase)"/>
  </xsl:template>

<!-- fnx-uc : convert string to all uppercase -->
  <xsl:template name="fnx-uc">
      <xsl:param name="string"/>
    <xsl:value-of select="translate($string, $lowercase, $uppercase)"/>
  </xsl:template>

<!-- fnx-uc1st : capitalize the first character of a string and make the rest lower case -->
  <xsl:template name="fnx-uc1st">
      <xsl:param name="string"/>
    <xsl:call-template name="fnx-uc">
      <xsl:with-param name="string" select="substring($string, 1, 1)"/>
    </xsl:call-template>
    <xsl:call-template name="fnx-lc">
      <xsl:with-param name="string" select="substring($string, 2)"/>
    </xsl:call-template>    
  </xsl:template>

<!-- fnx-pad-l : pad left side of character string 
    Parameters:
      string  the string to be padded.
      count  the length of the required string.
-->
  <xsl:template name="fnx-pad-l">
      <xsl:param name="string"/>
      <xsl:param name="length" select='0'/>
    <xsl:call-template name="fnx-repeat">
      <xsl:with-param name="count" select="$length - string-length($string)"/>
    </xsl:call-template>
    <xsl:value-of select="$string"/>
  </xsl:template>

<!-- fnx-pad-r : pad right side of character string 
    Parameters:
      string  the string to be padded.
      count  the length of the required string.
-->
  <xsl:template name="fnx-pad-r">
      <xsl:param name="string"/>
      <xsl:param name="length" select='0'/>
    <xsl:value-of select="$string"/>
    <xsl:call-template name="fnx-repeat">
      <xsl:with-param name="count" select="$length - string-length($string)"/>
    </xsl:call-template>
  </xsl:template>

  <!-- fnx-repeat : repeat a character string
    Parameters:
      string  the string to be repeated (default: space).
      count  the number of times to repeat the string.
-->
  <xsl:template name="fnx-repeat">
      <xsl:param name="string" select="'&#160;'"/>
      <xsl:param name="count" select='0'/>
    <xsl:if test="$count &gt; 0">
      <xsl:value-of select="$string"/>
      <xsl:call-template name="fnx-repeat">
        <xsl:with-param name="string" select="$string"/>        
        <xsl:with-param name="count" select="$count - 1"/>
      </xsl:call-template>
      </xsl:if>
  </xsl:template>

<!-- fnx-replace : replace all occurances of a string by a new value
    Parameters:
      string  the string to be modified.
      replace  the string to be replaced.
      by    the replacement string.
-->
  <xsl:template name="fnx-replace">
      <xsl:param name="string"/>
      <xsl:param name="replace"/>
      <xsl:param name="by"/>
    <xsl:choose>
      <xsl:when test="contains($string, $replace)">
        <xsl:value-of select="substring-before($string,$replace)"/>
        <xsl:value-of select="$by"/>
        <xsl:call-template name="fnx-replace">
          <xsl:with-param name="string" select="substring-after($string,$replace)"/>
          <xsl:with-param name="replace" select="$replace"/>
          <xsl:with-param name="by" select="$by"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$string"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

<!-- fnx-vet-nmbr : verify a string as numeric
    Parameters:
      nmbr  the number to be formatted.
      dec    the number of decimal places (defaults to zero).
      wdth  right-justify to this many characters (defaults to 0: no padding)
      mark  the decimal mark and separator to be used.
-->
  <xsl:template name="fnx-vet-nmbr">
      <xsl:param name="nmbr" select="0"/>
      <xsl:param name="mark" select='$lang.marks'/>

<!--  1) remove any spaces or "+" sign -->
    <xsl:variable name="n1" select="translate($nmbr, '+ ', '')"/>
<!--  2) remove any separators -->
    <xsl:variable name="n2" select="translate($n1,substring($mark,2),'')"/>
<!--  3) convert decimal mark to English usage -->
    <xsl:variable name="n3" select="translate($n2,$mark,$en-marks)"/>

<!--  Return number (or 'NaN' if not valid) -->
    <xsl:value-of select="string(number($n3))"/>
  </xsl:template>

<!-- fnx-fmt-nmbr : format a number with appropriate decimal mark and separator value
    Parameters:
      nmbr  the number to be formatted.
      dec    the number of decimal places (defaults to zero).
      wdth  right-justify to this many characters (defaults to 0: no padding)
      mark  the decimal mark and separator to be used.
-->
  <xsl:template name="fnx-fmt-nmbr">
      <xsl:param name="nmbr" select="0"/>
      <xsl:param name="dec" select="0"/>
      <xsl:param name="wdth" select="0"/>
      <xsl:param name="mark" select='$lang.marks'/>

    <xsl:variable name="mask">
      <xsl:choose>
        <xsl:when test="$dec &gt; 0">
          <xsl:variable name="decs">
            <xsl:call-template name="fnx-repeat">
              <xsl:with-param name="string" select="'0'"/>        
              <xsl:with-param name="count" select="$dec"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:value-of select="concat('###,##0.',decs)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'###,##0'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="n" select='format-number($nmbr,$mask)'/>

    <xsl:variable name="nbr">
      <xsl:choose>
        <xsl:when test="$mark = '$en-marks'">
          <xsl:value-of select="$n"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="translate($n,$en-marks,$mark)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    
    <xsl:call-template name="fnx-repeat">
      <xsl:with-param name="string" select="'&#160;'"/>        
      <xsl:with-param name="count" select="$wdth - string-length($nbr)"/>
    </xsl:call-template>
    <xsl:value-of select="$nbr"/>
  </xsl:template>

</xsl:stylesheet>