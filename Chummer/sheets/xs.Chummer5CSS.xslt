<?xml version="1.0" encoding="utf-8" ?>
<!-- CSS stylesheet definitions -->

<!-- These are the common CSS type definitions for the Chummer5a character sheets.
     They are not used by all the character sheets, just ones that have multiple
     common definitions such as Notes and the Shadowrun 5* sheets.

     To create a language specific version:
     1) this member should be copied to the appropriate language sub-folder and changes made.
     2) The character sheet XSL stylesheets that import these definitions have to be
        modified, changing the xsl:import statement:
          from href="../xs.Chummer5CSS.xslt" to href="xs.Chummer5CSS.xslt"
        (FYI: this tells the processor to import the member from the language sub-folder
         instead of the parent directory - indicated by the ../ before the member name)

     Changes made to these definitions should also be made to the language specific versions.
     (Note: currently these are German and Portuguese character sheets.)
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:template name="Chummer5CSS">
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
