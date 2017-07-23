<?xml version="1.0" encoding="UTF-8" ?>
<!-- Produce Row Summary Line -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<!--
  ***  RowSummary(text,blockname,buttons)
    Parameters:
      text:    The text describing the row summary (this is translated to upper case).
      blockname:  The block name. If present then used as is.
            If blank, defaults to value of text, any spaces are removed and
            "Block" is added to complete the block name.
      buttons:  Include "buttons" text (defaults to 'Y')
  ***  RowSummary
-->

  <xsl:template name="RowSummary">
      <xsl:param name="text" select="''"/>
      <xsl:param name="blockname" select="''"/>
      <xsl:param name="buttons" select="'Y'"/>

    <style type="text/css">
      .rowsummary {
        filter: progid:DXImageTransform.Microsoft.gradient(startColorstr='#3a6349', endColorstr='#769582'); /* for IE */
        background: -webkit-gradient(linear, left top, left bottom, from(#3a6349), to(#769582)); /* for webkit browsers */
        background: -moz-linear-gradient(top,  #3a6349,  #769582); /* for firefox 3.6+ */ 
        background-color: #3a6349;
        border-width: 0 0.5mm 0.5mm 0.5mm;
        border-top-width: 0;
        color: #ffffff;
        font-weight: bold;
        font-style: italic;
        page-break-inside: avoid;
        text-align: left;
        width: 100%;
      }
      .rowsummarybutton {
        color: #ffffff;
        font-weight: bold;
        margin-left: 20px;
      }
      .rowblock {
        page-break-inside: avoid;
      }
    </style>
    <style media="print">
      .rowsummary, .noPrint * {
        display: none;
      }
      .rowsummarybutton {
        visibility: hidden;
        display: none;
      }
      .zalomit {
        visibility: hidden;
      }
      .sectionhide {
        visibility: hidden;
        display: none;
      }
    </style>
    <style media="screen">
      .rowsummarybutton {
        visibility: visible;
      }
      .zalomit {
        page-break-after: auto;
      }
      .sectionhide {
        visibility: visible;
      }
    </style>

    <script type="text/javascript">
      <xsl:text>
        function zalomit(what,idx,Yes,No,lit)
        {
          var elem = document.getElementById(idx); 
          if (elem.style.pageBreakAfter == 'always') {
            txt = No;
            elem.style.pageBreakAfter = 'auto';
          }
          else {
            txt = Yes;
            elem.style.pageBreakAfter = 'always';
          }
          what.innerHTML = lit + txt;
        }
        function showhide(what,idx,Yes,No,lit)
        {
          var elem = document.getElementById(idx); 
          if (elem.className != 'sectionhide') {
            txt = No;
            elem.className = 'sectionhide';
          }
          else {
            txt = Yes;
            elem.className = 'rowblock';
          }
          what.innerHTML = lit + txt;
        }
      </xsl:text>
    </script>

    <xsl:variable name="txt">
      <xsl:variable name="t1" select="translate($text,'&#160;&#x9;&#xD;&#xA;&#8239;','&#x20;')"/>
      <xsl:variable name="t2" select="normalize-space($t1)"/>
      <xsl:choose>
        <xsl:when test="$t2 != ''">
          <xsl:value-of select="$t2"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$lang.Unknown"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <table class="rowsummary">
    <tr>
      <td width="30%">
        <xsl:call-template name="fnx-uc">
          <xsl:with-param name="string" select="$txt"/>
        </xsl:call-template>
      </td>
      <xsl:if test="$buttons != 'N'">
        <xsl:variable name="blk">
          <xsl:choose>
            <xsl:when test="$blockname != ''">
              <xsl:value-of select="$blockname"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="b1" select="translate($txt,'&#x20;&#160;&#x9;&#xD;&#xA;&#8239;&amp;/','')"/>
              <xsl:value-of select="concat($b1,'Block')"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <td width="18%" class="rowsummarybutton" style="text-align: left">
          <xsl:attribute name="onClick">
            showhide(this,'<xsl:value-of select="$blk"/>',
                    '<xsl:value-of select="$lang.Yes"/>',
                    '<xsl:value-of select="$lang.No"/>',
                    '<xsl:value-of select="$lang.Show"/>');
          </xsl:attribute>
          <xsl:value-of select="concat($lang.Show,$lang.Yes)"/>
        </td>
        <td width="27%" class="rowsummarybutton" style="text-align: left">
          <xsl:attribute name="onClick">
            zalomit(this,'<xsl:value-of select="$blk"/>',
                    '<xsl:value-of select="$lang.Yes"/>',
                    '<xsl:value-of select="$lang.No"/>',
                    '<xsl:value-of select="$lang.PageBreak"/>');
          </xsl:attribute>
          <xsl:value-of select="concat($lang.PageBreak,$lang.No)"/>
        </td>
        <td width="25%"/>
      </xsl:if>
    </tr>
    </table>

  </xsl:template>
</xsl:stylesheet>