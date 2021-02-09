<?xml version="1.0" encoding="utf-8" ?>
<!-- CSS stylesheet definitions -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:template name="Shadowrun5CSS">
    <style type="text/css">
      * {
        font-family: segoe, tahoma, 'trebuchet ms', arial;
        font-size: 8.25pt;
        margin: 0;
        text-align: left;
        vertical-align: top;
        }
        html {
        height: 100%;
        margin: 0em;  /* this affects the margin on the html before sending to printer */
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
        }
        .title {
        font-weight: bold;
        }
        .upper {
        }
        .block {
        bottom-padding: 0;
        page-break-inside: avoid;
        margin: 1em 0 0 0;  /* to keep the page break from cutting too close to the text in the div */
        }
        .mugshot {
        width: auto;
        max-width: 100%;
        object-fit: scale-down;
        image-rendering: optimizeQuality;
        }
        @media screen and (-ms-high-contrast: active), (-ms-high-contrast: none) {
        .mugshot {
        width: 100%;
        max-width: inherit;
        object-fit: scale-down;
        image-rendering: optimizeQuality;
        }
      }
    </style>
    <!--[if IE]
        <style type="text/css">
        .mugshot {
          width: 100%;
          max-width: inherit;
          object-fit: scale-down;
          image-rendering: optimizeQuality;
          }
        </style>
        -->
    <style media="print">
      @page {
      size: auto;
      margin-top: 0.5in;
      margin-left: 0.5in;
      margin-right: 0.5in;
      margin-bottom: 0.5in;
      }
    </style>
  </xsl:template>
</xsl:stylesheet>
