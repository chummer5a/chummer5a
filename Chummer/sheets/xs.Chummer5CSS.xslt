<?xml version="1.0" encoding="utf-8" ?>
<!-- CSS stylesheet definitions -->

<!-- These are the common CSS type definitions for the Chummer5a character sheets.

     They are not used by the Fancy Blocks and Vehicles character sheets - there is so little
     overlap between those sheets and these common definitions that it is simpler to keep
	 the separate definitions.

     Note: Individual character sheets may override some of these definitions,
     e.g. changing font size or font family used, or have additional definitions.
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:template name="Chummer5CSS">
    <style type="text/css">
      * {
        font-family: 'courier new', tahoma, 'trebuchet ms', arial;
        font-size: 10pt;
        margin: 0;
        text-align: left;
        vertical-align: top;
        }
        html {
        height: 100%;
        margin: 0px;  /* this affects the margin on the html before sending to printer */
        }
        body {
        color-adjust: exact !important;
        -webkit-print-color-adjust: exact !important;
        print-color-adjust: exact !important;
        }
        .tablestyle {
        border-collapse: collapse;
        border-color: #1c4a2d;
        border-style: solid;
        border-width: 0.5mm;
        cellpadding: 2;
        cellspacing: 0;
        width: 100%;
        }
        .attributecell p {
        padding: 0.25em;
        margin: 0.25em;
        border: solid 0.0625em #1c4a2d;
        text-transform: uppercase;
        }
        .indent {
        padding-left: 2mm;
        }
        .notesrow {
        text-align: justify;
        }
        .notesrow2 {
        padding-left: 2mm;
        padding-right: 2mm;
        text-align: justify;
        }
        th {
        text-align: center;
        text-transform: uppercase;
        }
        .title {
        font-weight: bold;
        text-transform: uppercase;
        }
        .upper {
        text-transform: uppercase;
        }
        .block {
        bottom-padding: 0;
        page-break-inside: avoid !important;
        margin: 1em 0 0 0;  /* to keep the page break from cutting too close to the text in the div */
        }
      }
    </style>
    <style media="print">
      @page {
      size: auto;
      margin-top: 0.5in;
      margin-left: 0.5in;
      margin-right: 0.5in;
      margin-bottom: 0.5in;
      }
      .block {
      bottom-padding: 0.75;
      page-break-inside: avoid !important;
      margin: 4px 0 4px 0;  /* to keep the page break from cutting too close to the text in the div */
      }
    </style>
<!-- ** remove use of uppercase in titles ** -->
    <xsl:if test="lang = 'de' or lang = 'pt'">
      <style type="text/css">
        * {
          th {
          text-align: center;
          }
          .title {
          font-weight: bold;
          }
          .upper {
          }
        }
      </style>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
