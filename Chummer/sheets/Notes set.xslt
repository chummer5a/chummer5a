<?xml version="1.0" encoding="utf-8" ?>
<!-- Character notes -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="xs.fnx.xslt"/>
  <xsl:include href="xs.fnxTests.xslt"/>
  <xsl:include href="xs.TitleName.xslt"/>

  <xsl:include href="xt.ComplexForms.xslt"/>
  <xsl:include href="xt.CritterPowers.xslt"/>
  <xsl:include href="xt.Lifestyles.xslt"/>
  <xsl:include href="xt.MetamagicArts.xslt"/>
  <xsl:include href="xt.Metamagics.xslt"/>
  <xsl:include href="xt.Notes.xslt"/>
  <xsl:include href="xt.Nothing2Show.xslt"/>
  <xsl:include href="xt.PreserveLineBreaks.xslt"/>
  <xsl:include href="xt.Qualities.xslt"/>
  <xsl:include href="xt.RowSummary.xslt"/>
  <xsl:include href="xt.RuleLine.xslt"/>
  <xsl:include href="xt.Spells.xslt"/>
  <xsl:include href="xt.TableTitle.xslt"/>

  <xsl:template match="/characters/character">
    <xsl:variable name="TitleName">
      <xsl:call-template name="TitleName">
        <xsl:with-param name="name" select="name"/>
        <xsl:with-param name="alias" select="alias"/>
      </xsl:call-template>
    </xsl:variable>
    <title><xsl:value-of select="$TitleName"/></title>

    <xsl:text disable-output-escaping="yes"><![CDATA[<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">]]></xsl:text>
    <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
      <head>
        <meta http-equiv="x-ua-compatible" content="IE=Edge"/>
        <meta charset="UTF-8" />
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
          .upper {
          text-transform: uppercase;
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
        </style>
        <style media="print">
           @page {
            size: auto;
            margin-top: 0.5in;
            margin-left: 0.5in;
            margin-right: 0.5in;
            margin-bottom: 0.75in;
          }
        </style>
      </head>

      <body>
        <xsl:if test="qualities/quality/notes != ''">
          <div id="QualitiesBlock">
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name" select="$lang.Qualities"/>
            </xsl:call-template>
            <table class="tablestyle">
              <tr><th width="80%"/><th width="10%"/><th width="10%"/></tr>
              <xsl:call-template name="Qualities"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Qualities"/>
            <xsl:with-param name="blockname" select="'QualitiesBlock'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="lifestyles/lifestyle/notes != ''">
          <div id="LifestyleBlock">
            <table><tr><td/></tr></table>
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name" select="$lang.Lifestyle"/>
            </xsl:call-template>
            <table class="tablestyle">
              <xsl:call-template name="Lifestyles"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.Lifestyle"/>
            <xsl:with-param name="blockname" select="'LifestyleBlock'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="magenabled = 'True'">
          <xsl:if test="initiationgrades/initiationgrade/notes != ''">
            <div id="InitiationBlock">
              <table><tr><td/></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.InitiationNotes"/>
              </xsl:call-template>
              <table class="tablestyle">
                <xsl:call-template name="gradenotes"/>
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.Initiation"/>
              <xsl:with-param name="blockname" select="'InitiationBlock'"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="arts/art/notes != ''">
            <div id="MetamagicArtsBlock">
              <table><tr><td/></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.Arts"/>
              </xsl:call-template>
              <table class="tablestyle">
                <tr><th width="80%"/><th width="10%"/><th width="10%"/></tr>
                <xsl:call-template name="MetamagicArts"/>
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.Arts"/>
              <xsl:with-param name="blockname" select="'MetamagicArtsBlock'"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="metamagics/metamagic/notes != ''">
            <div id="MetamagicsBlock">
              <table><tr><td/></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.Metamagics"/>
              </xsl:call-template>
              <table class="tablestyle">
                <tr><th width="80%"/><th width="10%"/><th width="10%"/></tr>
                <xsl:call-template name="Metamagics"/>
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.Metamagics"/>
              <xsl:with-param name="blockname" select="'MetamagicsBlock'"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="spells/spell/notes != ''">
            <div id="SpellsBlock">
              <table><tr><td/></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.Spells"/>
              </xsl:call-template>
              <table class="tablestyle">
                <xsl:call-template name="Spells"/>
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.Spells"/>
              <xsl:with-param name="blockname" select="'SpellsBlock'"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:if>

        <xsl:if test="resenabled = 'True'">
          <xsl:if test="initiationgrades/initiationgrade/notes != ''">
            <div id="SubmersionBlock">
              <table><tr><td/></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.SubmersionNotes"/>
              </xsl:call-template>
              <table class="tablestyle">
                <xsl:call-template name="gradenotes"/>
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.Submersion"/>
              <xsl:with-param name="blockname" select="'SubmersionBlock'"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="metamagics/metamagic/notes != ''">
            <div id="EchoesBlock">
              <table><tr><td/></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.Echoes"/>
              </xsl:call-template>
              <table class="tablestyle">
                <xsl:call-template name="Metamagics"/>
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.Echoes"/>
              <xsl:with-param name="blockname" select="'EchoesBlock'"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:if test="complexforms/complexform/notes != ''">
            <div id="ComplexFormsBlock">
              <table><tr><td/></tr></table>
              <xsl:call-template name="TableTitle">
                <xsl:with-param name="name" select="$lang.ComplexForms"/>
              </xsl:call-template>
              <table class="tablestyle">
                <xsl:call-template name="ComplexForms"/>
              </table>
            </div>
            <xsl:call-template name="RowSummary">
              <xsl:with-param name="text" select="$lang.ComplexForms"/>
              <xsl:with-param name="blockname" select="'ComplexFormsBlock'"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:if>

        <xsl:if test="critterpowers/critterpower">
          <div id="CritterBlock">
            <table><tr><td/></tr></table>
            <xsl:call-template name="TableTitle">
              <xsl:with-param name="name" select="$lang.CritterPowers"/>
            </xsl:call-template>
            <table class="tablestyle">
              <xsl:call-template name="CritterPowers"/>
            </table>
          </div>
          <xsl:call-template name="RowSummary">
            <xsl:with-param name="text" select="$lang.CritterPowers"/>
            <xsl:with-param name="blockname" select="'CritterBlock'"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="concat(concept,description,background,notes,gamenotes) !=''">
          <xsl:call-template name="notes"/>
        </xsl:if>

        <xsl:choose>
          <xsl:when test="qualities/quality/notes != ''"/>
          <xsl:when test="initiationgrades/initiationgrade/notes != ''"/>
          <xsl:when test="metamagics/metamagic/notes != ''"/>
          <xsl:when test="spells/spell/notes != ''"/>
          <xsl:when test="complexforms/complexform/notes != ''"/>
          <xsl:when test="lifestyles/lifestyle/notes != ''"/>
          <xsl:when test="critterpowers/critterpower != ''"/>
          <xsl:when test="concat(concept,description,background,notes,gamenotes) !=''"/>
          <xsl:otherwise>
            <xsl:call-template name="nothing2show">
              <xsl:with-param name="namethesheet" select="$lang.Nothing2Show4Notes"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </body>
    </html>

  </xsl:template>

  <xsl:template name="gradenotes">
    <xsl:for-each select="initiationgrades/initiationgrade[notes != '']">
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td colspan="100%" class="notesrow">
          <u><xsl:value-of select="$lang.Grade"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="grade"/></u>
          <xsl:text> </xsl:text>
          <xsl:call-template name="PreserveLineBreaks">
             <xsl:with-param name="text" select="notes"/>
          </xsl:call-template>
        </td>
      </tr>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>
