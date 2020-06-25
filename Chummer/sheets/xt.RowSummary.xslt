<?xml version="1.0" encoding="utf-8" ?>
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
      filter: progid:DXImageTransform.Microsoft.gradient(startColorstr='#1c4a2d', endColorstr='#38945a'); /* for IE */
      background: -webkit-gradient(linear, left top, left bottom, from(#1c4a2d), to(#38945a)); /* for webkit browsers */
      background: -moz-linear-gradient(top,  #1c4a2d,  #38945a); /* for firefox 3.6+ */
      background-color: #1c4a2d;
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
      margin-left: 2.5em;
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
      bottom-padding: 0;
      page-break-inside: avoid;
      margin: 1em 0 0 0;  /* to keep the page break from cutting too close to the text in the div */
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
      visibility: hidden;
      height: 1em;
      max-height: 1em;
      bottom-padding: 0;
      page-break-inside: avoid;
      margin: 0 0 0 0;
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
            elem.className = 'block';
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
      <td width="30%" class="upper" style="text-align: left">
        <xsl:value-of select="$txt"/>
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
        <td width="30%" class="rowsummarybutton" style="text-align: left" onClick="showhide(this,{blk},{lang.Yes},{lang.No},{lang.Show});">
          <xsl:value-of select="concat($lang.Show,$lang.Yes)"/>
        </td>
        <td width="40%" class="rowsummarybutton" style="text-align: left" onClick="zalomit(this,{blk},{lang.Yes},{lang.No},{lang.PageBreak});">
          <xsl:value-of select="concat($lang.PageBreak,$lang.No)"/>
        </td>
      </xsl:if>
    </tr>
    </table>

  </xsl:template>
</xsl:stylesheet>
