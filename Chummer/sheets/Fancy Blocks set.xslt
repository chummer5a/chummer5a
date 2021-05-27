<?xml version="1.0" encoding="utf-8" ?>
<!-- Character sheet with fancy blocks for the modularity-->
<!-- Created by AngelForest -->
<!-- Prototype by Adam Schmidt -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:include href="xt.PreserveHtml.xslt"/>
  <xsl:include href="xt.PreserveLineBreaks.xslt"/>
  <xsl:include href="xs.TitleName.xslt"/>
  <xsl:include href="xs.fnx.xslt"/>

  <xsl:output method="html" indent="yes" version="4.0"/>

  <xsl:template match="/characters/character">
    <xsl:variable name="ImageFormat" select="imageformat" />
    <xsl:variable name="TitleName">
      <xsl:call-template name="TitleName">
        <xsl:with-param name="name" select="name"/>
        <xsl:with-param name="alias" select="alias"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:text disable-output-escaping="yes"><![CDATA[<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">]]></xsl:text>
    <html>
      <head>
        <meta http-equiv="x-ua-compatible" content="IE=Edge"/>
        <meta charset="UTF-8" />
        <title><xsl:value-of select="$TitleName" /></title>
        <style type="text/css">
            *
            {
            font-family: Arial, Helvetica, sans-serif;
            font-size: 12px;
            vertical-align: top;
            }
            body
            {
            color-adjust: exact !important;
            -webkit-print-color-adjust: exact !important;
            print-color-adjust: exact !important;
            }
            hr
            {
            color: lightgrey;
            height: 1px;
            margin-left: 2px;
            margin-right: 2px;
            }
            ul
            {
            margin-top: 0px;
            margin-bottom: 0px;
            margin-left: 20px;
            padding-left: 0px;
            list-style-type: none;
            }
            li
            {
            margin-top: 2px;
            }
            .fill33
            {
            width: 33%;
            }
            .fill66
            {
            width: 66%;
            }
            {
            .fill100
            width: 100%;
            }
            table.stats
            {
            border-style: solid;
            border-width: 1px;
            border-color: grey;
            width: 100%;
            border-collapse: collapse;
            }
            table.stats td
            {
            padding: 2px;
            }
            table.stats .bigheader
            {
            color: white;
            background-color: grey;
            font-weight: normal;
            font-variant: small-caps;
            font-size: 110%;
            text-align: center;
            padding-top: 1px;
            padding-bottom: 2px;
            }
            tr:nth-child(odd) {
            background: #eee
            }
            .smallheader
            {
            color: grey;
            font-weight: bold;
            }
            td
            {
            page-break-inside: avoid;
            }
            strong
            {
            font-size: 105%;
            }
            @media screen
            {
            .page_breaker_off, .page_breaker_on
            {
            display: initial;
            text-align: left;
            }
            .page_breaker_off td, .page_breaker_on td
            {
            border-style: solid;
            border-width: 1px;
            border-color: lightgrey;
            }
            }
            @media print
            {
            *
            {
            font-size: 10px;
            }
            .page_breaker_off
            {
            page-break-before: auto;
            display: none;
            }
            .page_breaker_on
            {
            page-break-before: always;
            visibility: hidden;
            }
            .noprint
            {
            display: none;
            }
            }
            .mugshot {
            height: auto;
            width: auto;
            max-width: 100%;
            object-fit: scale-down;
            image-rendering: optimizeQuality;
            }
            @media screen and (-ms-high-contrast: active), (-ms-high-contrast: none) {
            .mugshot {
            height: auto;
            width: inherit;
            max-width: 100%;
            object-fit: scale-down;
            }
            }
        </style>
        <!--[if IE]
        <style type="text/css">
        .mugshot {
          height: auto;
          width: inherit;
          max-width: 100%;
          object-fit: scale-down;
          }
        </style>
        -->

        <style type="text/css" id="style_colored_headers">
          table.general {border-color: #6a6f29;}
          table.general .bigheader {background-color: #6a6f29;}
          table.general hr {color: #6a6f29;}
          table.armory {border-color: #9e2121;}
          table.armory .bigheader {background-color: #9e2121;}
          table.armory hr {color: #9e2121;}
          table.machine {border-color: #512b1b;}
          table.machine .bigheader {background-color: #512b1b;}
          table.machine hr {color: #512b1b;}
          table.magic {border-color: #00afef;}
          table.magic .bigheader {background-color: #00afef;}
          table.magic hr {color: #00afef;}
          table.matrix {border-color: #eb7907;}
          table.matrix .bigheader {background-color: #eb7907;}
          table.matrix hr {color: #eb7907;}
          table.gear {border-color: #92509e;}
          table.gear .bigheader {background-color: #92509e;}
          table.gear hr {color: #92509e;}
          table.description {border-color: #00693b;}
          table.description .bigheader {background-color: #00693b;}
          table.description hr {color: #00693b;}
        </style>

        <!-- btw, ie version is about 6 and this shit doesn't support normal DOM methods -->
        <script type="text/javascript">
          <xsl:text>
            function toggle_page_breaker(breaker) {
              if (breaker.className == 'page_breaker_off') {
                breaker.className = 'page_breaker_on';
                breaker.getElementsByTagName('span')[0].innerHTML = 'ON';
              }
              else {
                breaker.className = 'page_breaker_off';
                breaker.getElementsByTagName('span')[0].innerHTML = 'OFF';
              }
            }

            function toggle_colors() {
              var ss = document.getElementById('style_colored_headers');
              ss.disabled = !ss.disabled;
            }
          </xsl:text>
        </script>
      </head>

      <body>
        <table id="maintable">
          <tr>
            <td class="fill100 noprint">
              <button onClick="toggle_colors();"><xsl:value-of select="$lang.PersonalData" /></button>
            </td>
          </tr>
          <tr>
            <td class="fill33">
              <xsl:call-template name="print_personal_data" />
            </td>
            <td class="fill33">
              <xsl:call-template name="print_attributes" />
            </td>
            <td class="fill33">
              <xsl:call-template name="print_mugshot_and_priorities">
                <xsl:with-param name="ImageFormat" select="$ImageFormat" />
              </xsl:call-template>
            </td>
          </tr>
          <xsl:call-template name="page_breaker" />
          <tr>
            <td class="fill66" colspan="2">
              <xsl:call-template name="print_active_skills" />
            </td>
            <td class="fill33">
              <xsl:call-template name="print_knowledge_skills" />
            </td>
          </tr>
          <xsl:call-template name="page_breaker" />
          <tr>
            <xsl:choose>
              <xsl:when test="count(qualities/quality) &gt; count(contacts/contact) or count(contacts/contact) &lt; 4">
                <td class="fill66" colspan="2">
                  <xsl:call-template name="print_qualities">
                    <xsl:with-param name="double_size" select="true()" />
                  </xsl:call-template>
                </td>
                <td class="fill33">
                  <xsl:call-template name="print_contacts">
                    <xsl:with-param name="double_size" select="false()" />
                  </xsl:call-template>
                </td>
              </xsl:when>
              <xsl:otherwise>
                <td class="fill66" colspan="2">
                  <xsl:call-template name="print_contacts">
                    <xsl:with-param name="double_size" select="true()" />
                  </xsl:call-template>
                </td>
                <td class="fill33">
                  <xsl:call-template name="print_qualities">
                    <xsl:with-param name="double_size" select="false()" />
                  </xsl:call-template>
                </td>
              </xsl:otherwise>
            </xsl:choose>
          </tr>
          <xsl:call-template name="page_breaker" />
          <tr>
            <td class="fill66" colspan="2">
              <xsl:call-template name="print_ranged_weapons" />
              <br />
              <xsl:call-template name="print_melee_weapons" />
            </td>
            <td class="fill33">
              <xsl:call-template name="print_armor" />
              <br />
              <xsl:call-template name="print_martial_arts" />
            </td>
          </tr>
          <xsl:call-template name="page_breaker" />
          <tr>
            <td class="fill66" colspan="2">
              <xsl:call-template name="print_vehicles" />
              <br />
              <xsl:call-template name="print_matrix_devices" />
            </td>
            <td class="fill33">
              <xsl:call-template name="print_implants" />
            </td>
          </tr>
          <xsl:if test="resenabled='True'">
            <xsl:call-template name="page_breaker" />
            <tr>
              <td class="fill66" colspan="2">
                <xsl:call-template name="print_sprites" />
              </td>
              <td class="fill33">
                <xsl:call-template name="print_complex_forms" />
                <br />
                <xsl:call-template name="print_submersion" />
              </td>
            </tr>
          </xsl:if>
          <xsl:if test="depenabled='True'">
            <xsl:call-template name="page_breaker" />
            <tr>
              <td class="fill100">
                <xsl:call-template name="print_ai_programs" />
              </td>
            </tr>
          </xsl:if>
          <xsl:if test="magenabled = 'True'">
            <xsl:call-template name="page_breaker" />
            <xsl:choose>
              <!--simplified layour for adepts-->
              <xsl:when test="qualities/quality[name='Adept']">
                <td class="fill33">
                  <xsl:call-template name="print_magic" />
                  <br />
                  <xsl:call-template name="print_foci" />
                </td>
                <td class="fill33">
                  <xsl:call-template name="print_adept_powers" />
                </td>
                <td class="fill33">
                  <xsl:call-template name="print_initiation" />
                </td>
              </xsl:when>
              <!--everyone else usually have many speels and/or spirits, so we move other tabs to the right-->
              <xsl:otherwise>
                <tr>
                  <td class="fill66" colspan="2">
                    <xsl:call-template name="print_spells" />
                    <br />
                    <xsl:call-template name="print_spirits" />
                  </td>
                  <td class="fill33">
                    <xsl:call-template name="print_magic" />
                    <br />
                    <xsl:if test="qualities/quality[name='Mystic Adept']">
                      <xsl:call-template name="print_adept_powers" />
                      <br />
                    </xsl:if>
                    <xsl:call-template name="print_foci" />
                    <br />
                    <xsl:call-template name="print_initiation" />
                  </td>
                </tr>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
          <xsl:call-template name="page_breaker" />
          <tr>
            <td class="fill66" colspan="2">
              <xsl:call-template name="print_other_gear" />
            </td>
            <td class="fill33">
              <xsl:call-template name="print_ids" />
              <br />
              <xsl:call-template name="print_lifestyle" />
            </td>
          </tr>
          <xsl:call-template name="page_breaker" />
          <tr>
            <td class="fill100" colspan="3">
              <xsl:call-template name="print_description">
                <xsl:with-param name="ImageFormat" select="$ImageFormat" />
              </xsl:call-template>
            </td>
          </tr>
        </table>
      </body>
    </html>
  </xsl:template>

  <xsl:template name="print_personal_data">
    <table class="stats general">
      <tr><td colspan="4"><div class="bigheader">[<xsl:value-of select="$lang.PersonalData" />]</div></td></tr>
      <tr><td><xsl:value-of select="$lang.Name" /></td><td colspan="3"><strong><xsl:value-of select="name" /></strong></td></tr>
      <tr><td><xsl:value-of select="$lang.Alias" /></td><td colspan="3"><strong><xsl:value-of select="alias" /></strong></td></tr>
      <tr>
        <td><xsl:value-of select="$lang.Metatype" /></td>
        <td colspan="3">
          <strong>
            <xsl:value-of select="metatype" />
            <xsl:if test="metavariant != ''"> (<xsl:value-of select="metavariant" />)</xsl:if>
          </strong>
        </td>
      </tr>
      <tr>
        <td><xsl:value-of select="$lang.Gender"/></td><td><strong><xsl:value-of select="gender" /></strong></td>
        <xsl:choose>
          <xsl:when test="qualities/quality[name='Mystic Adept']"><td><xsl:value-of select="$lang.Special" /></td><td><strong>Mystic Adept</strong></td></xsl:when>
          <xsl:when test="qualities/quality[name='Adept']"><td><xsl:value-of select="$lang.Special" /></td><td><strong>Adept</strong></td></xsl:when>
          <xsl:when test="qualities/quality[name='Aspected Magician']"><td><xsl:value-of select="$lang.Special" /></td><td><strong>Aspected Magician</strong></td></xsl:when>
          <xsl:when test="qualities/quality[name='Magician']"><td><xsl:value-of select="$lang.Special" /></td><td><strong>Magician</strong></td></xsl:when>
          <xsl:when test="qualities/quality[name='Enchanter']"><td><xsl:value-of select="$lang.Special" /></td><td><strong>Enchanter</strong></td></xsl:when>
          <xsl:when test="qualities/quality[name='Explorer']"><td><xsl:value-of select="$lang.Special" /></td><td><strong>Explorer</strong></td></xsl:when>
          <xsl:when test="qualities/quality[name='Apprentice']"><td><xsl:value-of select="$lang.Special" /></td><td><strong>Apprentice</strong></td></xsl:when>
          <xsl:when test="qualities/quality[name='Aware']"><td><xsl:value-of select="$lang.Special" /></td><td><strong>Aware</strong></td></xsl:when>
          <xsl:when test="qualities/quality[name='Technomancer']"><td><xsl:value-of select="$lang.Special" /></td><td><strong>Technomancer</strong></td></xsl:when>
          <xsl:otherwise><td></td><td></td></xsl:otherwise>
        </xsl:choose>
      </tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Age" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="age" /></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Skin" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="skin" /></strong></td></tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Hair" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="hair" /></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Eyes" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="eyes" /></strong></td></tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Height" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="height" /></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Weight" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="weight" /></strong></td></tr>
      <tr><td colspan="4"><hr /></td></tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Karma" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="karma" /></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Nuyen" /></td>
        <td style="white-space: nowrap;"><strong>
          <xsl:value-of select="nuyen"/>
          <xsl:value-of select="$lang.NuyenSymbol"/>
        </strong></td></tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.StreetCred" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="totalstreetcred" /></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Career" /><xsl:text> </xsl:text><xsl:value-of select="$lang.Karma" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="totalkarma" /></strong></td></tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Notoriety" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="totalnotoriety" /></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.PublicAwareness" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="totalpublicawareness" /></strong></td></tr>
      <xsl:if test="totalastralreputation != '0' or totalwildreputation != '0'">
        <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.AstralReputation" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="totalastralreputation" /></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.WildReputation" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="totalwildreputation" /></strong></td></tr>
      </xsl:if>
      <tr><td colspan="4"><hr /></td></tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Composure" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="composure" /></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.JudgeIntentions" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="judgeintentions" /></strong></td></tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Memory" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="memory" /></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.LiftCarry" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="liftandcarry" /></strong></td></tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Movement" /></td>
        <td><strong><xsl:value-of select="movement" /></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.LiftCarry" /><xsl:text> </xsl:text><xsl:value-of select="$lang.Weight" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="liftweight" />/<xsl:value-of select="carryweight" /></strong></td></tr>
      <tr><td colspan="4"><hr /></td></tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.PhysicalLimit" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="limitphysical" /></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.MentalLimit" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="limitmental" /></strong></td></tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.SocialLimit" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="limitsocial" /></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.AstralLimit" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="limitastral" /></strong></td></tr>
    </table>
  </xsl:template>

  <xsl:template name="print_attributes">
    <table class="stats general">
      <tr><td colspan="4"><div class="bigheader">[<xsl:value-of select="$lang.Attributes" />]</div></td></tr>
      <xsl:if test="attributes/attributecategory">
        <tr>
          <td style="white-space: nowrap;">
            <xsl:value-of select="$lang.CurrentForm" />
          </td>
          <td style="white-space: nowrap;" colspan="3">
            <strong><xsl:value-of select="attributes/attributecategory"/></strong>
          </td>
        </tr>
      </xsl:if>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Body" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="attributes/attribute[name_english = 'BOD' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'BOD' and ../attributecategory_english = metatypecategory]))]/base" />
                  <xsl:if test="attributes/attribute[name_english = 'BOD' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'BOD' and ../attributecategory_english = metatypecategory]))]/total != attributes/attribute[name_english = 'BOD' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'BOD' and ../attributecategory_english = metatypecategory]))]/base">
                    (<xsl:value-of select="attributes/attribute[name_english = 'BOD' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'BOD' and ../attributecategory_english = metatypecategory]))]/total" />)
                  </xsl:if></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Willpower" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="attributes/attribute[name_english = 'WIL' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'WIL' and ../attributecategory_english = metatypecategory]))]/base" />
                  <xsl:if test="attributes/attribute[name_english = 'WIL' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'WIL' and ../attributecategory_english = metatypecategory]))]/total != attributes/attribute[name_english = 'WIL' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'WIL' and ../attributecategory_english = metatypecategory]))]/base">
                    (<xsl:value-of select="attributes/attribute[name_english = 'WIL' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'WIL' and ../attributecategory_english = metatypecategory]))]/total" />)
                  </xsl:if></strong></td>
      </tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Agility" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="attributes/attribute[name_english = 'AGI' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'AGI' and ../attributecategory_english = metatypecategory]))]/base" />
                  <xsl:if test="attributes/attribute[name_english = 'AGI' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'AGI' and ../attributecategory_english = metatypecategory]))]/total != attributes/attribute[name_english = 'AGI' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'AGI' and ../attributecategory_english = metatypecategory]))]/base">
                    (<xsl:value-of select="attributes/attribute[name_english = 'AGI' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'AGI' and ../attributecategory_english = metatypecategory]))]/total" />)
                  </xsl:if></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Logic" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="attributes/attribute[name_english = 'LOG' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'LOG' and ../attributecategory_english = metatypecategory]))]/base" />
                  <xsl:if test="attributes/attribute[name_english = 'LOG' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'LOG' and ../attributecategory_english = metatypecategory]))]/total != attributes/attribute[name_english = 'LOG' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'LOG' and ../attributecategory_english = metatypecategory]))]/base">
                    (<xsl:value-of select="attributes/attribute[name_english = 'LOG' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'LOG' and ../attributecategory_english = metatypecategory]))]/total" />)
                  </xsl:if></strong></td>
      </tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Reaction" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="attributes/attribute[name_english = 'REA' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'REA' and ../attributecategory_english = metatypecategory]))]/base" />
                  <xsl:if test="attributes/attribute[name_english = 'REA' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'REA' and ../attributecategory_english = metatypecategory]))]/total != attributes/attribute[name_english = 'REA' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'REA' and ../attributecategory_english = metatypecategory]))]/base">
                    (<xsl:value-of select="attributes/attribute[name_english = 'REA' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'REA' and ../attributecategory_english = metatypecategory]))]/total" />)
                  </xsl:if></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Intuition" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="attributes/attribute[name_english = 'INT' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'INT' and ../attributecategory_english = metatypecategory]))]/base" />
                  <xsl:if test="attributes/attribute[name_english = 'INT' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'INT' and ../attributecategory_english = metatypecategory]))]/total != attributes/attribute[name_english = 'INT' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'INT' and ../attributecategory_english = metatypecategory]))]/base">
                    (<xsl:value-of select="attributes/attribute[name_english = 'INT' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'INT' and ../attributecategory_english = metatypecategory]))]/total" />)
                  </xsl:if></strong></td>
      </tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Strength" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="attributes/attribute[name_english = 'STR' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'STR' and ../attributecategory_english = metatypecategory]))]/base" />
                  <xsl:if test="attributes/attribute[name_english = 'STR' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'STR' and ../attributecategory_english = metatypecategory]))]/total != attributes/attribute[name_english = 'STR' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'STR' and ../attributecategory_english = metatypecategory]))]/base">
                    (<xsl:value-of select="attributes/attribute[name_english = 'STR' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'STR' and ../attributecategory_english = metatypecategory]))]/total" />)
                  </xsl:if></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Charisma" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="attributes/attribute[name_english = 'CHA' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'CHA' and ../attributecategory_english = metatypecategory]))]/base" />
                  <xsl:if test="attributes/attribute[name_english = 'CHA' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'CHA' and ../attributecategory_english = metatypecategory]))]/total != attributes/attribute[name_english = 'CHA' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'CHA' and ../attributecategory_english = metatypecategory]))]/base">
                    (<xsl:value-of select="attributes/attribute[name_english = 'CHA' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'CHA' and ../attributecategory_english = metatypecategory]))]/total" />)
                  </xsl:if></strong></td>
      </tr>
      <tr>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Edge" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="attributes/attribute[name_english = 'EDG' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'EDG' and ../attributecategory_english = metatypecategory]))]/base" />
                  <xsl:if test="attributes/attribute[name_english = 'EDG' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'EDG' and ../attributecategory_english = metatypecategory]))]/total != attributes/attribute[name_english = 'EDG' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'EDG' and ../attributecategory_english = metatypecategory]))]/base">
                    (<xsl:value-of select="attributes/attribute[name_english = 'EDG' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'EDG' and ../attributecategory_english = metatypecategory]))]/total" />)
                  </xsl:if></strong></td>
        <td style="white-space: nowrap;"><xsl:value-of select="$lang.Essence" /></td>
        <td style="white-space: nowrap;"><strong><xsl:value-of select="totaless" /></strong></td>
      </tr>
      <xsl:if test="magenabled = 'True'">
        <tr>
          <td style="white-space: nowrap;"><xsl:value-of select="$lang.Magic" /></td>
          <td style="white-space: nowrap;"><strong><xsl:value-of select="attributes/attribute[name_english = 'MAG' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'MAG' and ../attributecategory_english = metatypecategory]))]/base" />
                    <xsl:if test="attributes/attribute[name_english = 'MAG' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'MAG' and ../attributecategory_english = metatypecategory]))]/total != attributes/attribute[name_english = 'MAG' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'MAG' and ../attributecategory_english = metatypecategory]))]/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'MAG' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'MAG' and ../attributecategory_english = metatypecategory]))]/total" />)
                    </xsl:if>
            <xsl:if test="attributes/attribute[name_english = 'MAGAdept' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'MAGAdept' and ../attributecategory_english = metatypecategory]))]">
              | <xsl:value-of select="attributes/attribute[name_english = 'MAGAdept' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'MAGAdept' and ../attributecategory_english = metatypecategory]))]/base"/>
              <xsl:if test="attributes/attribute[name_english = 'MAGAdept' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'MAGAdept' and ../attributecategory_english = metatypecategory]))]/total != attributes/attribute[name_english = 'MAGAdept']/base">
                (<xsl:value-of select="attributes/attribute[name_english = 'MAGAdept' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'MAGAdept' and ../attributecategory_english = metatypecategory]))]/total"/>)
              </xsl:if>
            </xsl:if>
          </strong></td>
          <td colspan="2" />
        </tr>
      </xsl:if>
      <xsl:if test="resenabled = 'True'">
        <tr>
          <td style="white-space: nowrap;"><xsl:value-of select="$lang.Resonance" /></td>
          <td style="white-space: nowrap;"><strong><xsl:value-of select="attributes/attribute[name_english = 'RES' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'RES' and ../attributecategory_english = metatypecategory]))]/base" />
                    <xsl:if test="attributes/attribute[name_english = 'RES' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'RES' and ../attributecategory_english = metatypecategory]))]/total != attributes/attribute[name_english = 'RES' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'RES' and ../attributecategory_english = metatypecategory]))]/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'RES' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'RES' and ../attributecategory_english = metatypecategory]))]/total" />)
                    </xsl:if></strong></td><td colspan="2" /></tr>
      </xsl:if>
      <xsl:if test="depenabled = 'True'">
        <tr>
          <td style="white-space: nowrap;"><xsl:value-of select="$lang.Depth" /></td>
          <td style="white-space: nowrap;"><strong><xsl:value-of select="attributes/attribute[name_english = 'DEP' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'DEP' and ../attributecategory_english = metatypecategory]))]/base" />
                    <xsl:if test="attributes/attribute[name_english = 'DEP' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'DEP' and ../attributecategory_english = metatypecategory]))]/total != attributes/attribute[name_english = 'DEP' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'DEP' and ../attributecategory_english = metatypecategory]))]/base">
                      (<xsl:value-of select="attributes/attribute[name_english = 'DEP' and (../attributecategory_english = metatypecategory or not(../attribute[name_english = 'DEP' and ../attributecategory_english = metatypecategory]))]/total" />)
                    </xsl:if></strong></td><td colspan="2" /></tr>
      </xsl:if>
      <tr><td colspan="4"><hr /></td></tr>
            <tr><td colspan="2"><xsl:value-of select="$lang.Initiative" /></td><td colspan="2"><strong><xsl:value-of select="init" /></strong></td></tr>
            <tr><td colspan="2"><xsl:value-of select="$lang.AstralInitiative" /></td><td colspan="2"><strong><xsl:value-of select="astralinit" /></strong></td></tr>
            <tr><td colspan="2"><xsl:value-of select="$lang.RiggerInitiative" /></td><td colspan="2"><strong><xsl:value-of select="riggerinit" /></strong></td></tr>
            <tr><td colspan="2"><xsl:value-of select="$lang.MatrixAR" /></td><td colspan="2"><strong><xsl:value-of select="matrixarinit" /></strong></td></tr>
            <tr><td colspan="2"><xsl:value-of select="$lang.MatrixCold" /></td><td colspan="2"><strong><xsl:value-of select="matrixcoldinit" /></strong></td></tr>
            <tr><td colspan="2"><xsl:value-of select="$lang.MatrixHot" /></td><td colspan="2"><strong><xsl:value-of select="matrixhotinit" /></strong></td></tr>
            <tr><td colspan="4"><hr /></td></tr>
            <tr><td colspan="3">
              <xsl:choose>
                <xsl:when test="physicalcmiscorecm = 'True'">
                  <xsl:value-of select="$lang.CoreTrack" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$lang.PhysicalTrack" />
                </xsl:otherwise>
              </xsl:choose>
            </td><td><strong><xsl:value-of select="physicalcm" /></strong></td></tr>
            <tr><td colspan="3"><xsl:value-of select="$lang.Overflow"/></td><td><strong><xsl:value-of select="cmoverflow - 1"/></strong></td></tr>
            <tr><td colspan="3"><xsl:value-of select="$lang.PhysicalNaturalRecovery" /></td><td><strong><xsl:value-of select="physicalcmnaturalrecovery" /></strong></td></tr>
            <tr><td colspan="3">
              <xsl:choose>
                <xsl:when test="stuncmismatrixcm = 'True'">
                  <xsl:value-of select="$lang.MatrixTrack" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:if test="physicalcmiscorecm != 'True'">
                    <xsl:value-of select="$lang.StunTrack" />
                  </xsl:if>
                </xsl:otherwise>
              </xsl:choose>
            </td><td>
              <xsl:if test="physicalcmiscorecm != 'True' or stuncmismatrixcm = 'True'">
                  <xsl:value-of select="stuncm" />
              </xsl:if>
            </td></tr>
            <tr><td colspan="3">
              <xsl:if test="physicalcmiscorecm != 'True' or stuncmismatrixcm = 'True'">
                  <xsl:value-of select="$lang.StunNaturalRecovery" />
              </xsl:if>
            </td><td>
              <xsl:if test="physicalcmiscorecm != 'True' or stuncmismatrixcm = 'True'">
                  <xsl:value-of select="stuncmnaturalrecovery"/>
              </xsl:if>
            </td></tr>
    </table>
  </xsl:template>

  <xsl:template name="print_mugshot_and_priorities">
    <xsl:param name="ImageFormat" />
    <table class="stats general">
      <xsl:if test="mainmugshotbase64 != ''">
        <tr><td colspan="2"><div class="bigheader">[<xsl:value-of select="$lang.Mugshot" />]</div></td></tr>
        <tr><td colspan="2" style="text-align:center; width: 100%;">
          <img src="data:image/{$ImageFormat};base64,{mainmugshotbase64}" class="mugshot" />
        </td></tr>
      </xsl:if>
      <xsl:if test="prioritymetatype != ''">
        <tr><td colspan="2"><div class="bigheader">[<xsl:value-of select="$lang.Priorities" />]</div></td></tr>
                        <tr><td><xsl:value-of select="$lang.Metatype" /></td><td><strong><xsl:value-of select="prioritymetatype" /></strong></td></tr>
                        <tr><td><xsl:value-of select="$lang.Attributes" /></td><td><strong><xsl:value-of select="priorityattributes" /></strong></td></tr>
                        <tr><td><xsl:value-of select="$lang.Special" /></td><td><strong><xsl:value-of select="priorityspecial" /></strong></td></tr>
                        <tr><td><xsl:value-of select="$lang.Skills" /></td><td><strong><xsl:value-of select="priorityskills" /></strong></td></tr>
                        <tr><td><xsl:value-of select="$lang.Resources" /></td><td><strong><xsl:value-of select="priorityresources" /></strong></td></tr>
      </xsl:if>
    </table>
  </xsl:template>

  <xsl:template name="print_active_skills">
    <table class="stats general">
      <tr><td colspan="2"><div class="bigheader">[<xsl:value-of select="$lang.ActiveSkills" />]</div></td></tr>
      <tr>
        <td style="width:50%;">
          <table style="width:100%; border-collapse: collapse;">
            <xsl:call-template name="print_half_active_skills">
              <xsl:with-param name="condition" select="true()" />
            </xsl:call-template>
          </table>
        </td>
        <td style="width:50%;">
          <table style="width:100%; border-collapse: collapse;">
            <xsl:call-template name="print_half_active_skills">
              <xsl:with-param name="condition" select="false()" />
            </xsl:call-template>
            <xsl:if test="(count(skills/skillgroup) &gt; 0)">
              <tr>
                <td colspan="3">
                  <strong>
                    <u><xsl:text> </xsl:text><xsl:value-of select="$lang.SkillGroups"/></u>
                  </strong>
                </td>
              </tr>
              <xsl:for-each select="skills/skillgroup">
                <xsl:sort select="name"/>

                <tr>
                  <xsl:call-template name="make_grey_lines" />
                  <td valign="top" style="valign: top; text-align: right;">
                    * <xsl:value-of select="name"/>
                  </td>
                  <td colspan="2" style="valign: top; text-align: center;">
                    <xsl:value-of select="rating"/>
                  </td>
                </tr>
              </xsl:for-each>
            </xsl:if>
          </table>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template name="print_half_active_skills">
    <xsl:param name="condition" />

    <xsl:variable name="sorted_skills">
      <xsl:for-each select="skills/skill[knowledge = 'False' and (rating &gt; 0 or total &gt; 0)]">
        <xsl:sort select="skillcategory" />
        <xsl:sort select="name" />

        <xsl:copy-of select="current()"/>
      </xsl:for-each>
    </xsl:variable>

    <xsl:variable name="skills_half_count" select="ceiling((count(msxsl:node-set($sorted_skills)/skill) + count(skills/skillgroup)) div 2) + 1" />

    <tr class="smallheader"><td><xsl:value-of select="$lang.Skill"/></td><td><xsl:value-of select="$lang.Rtg"/></td><td><xsl:value-of select="$lang.Pool"/></td></tr>
    <xsl:for-each select="msxsl:node-set($sorted_skills)/skill">
      <xsl:sort select="skillcategory" />
      <xsl:sort select="name" />

      <xsl:if test="(position() &lt; $skills_half_count)=$condition">
        <xsl:if test="skillcategory != preceding-sibling::skill[1]/skillcategory or position()=1">
          <tr><td colspan="3"><strong><u><xsl:value-of select="skillcategory" /><xsl:text> </xsl:text><xsl:value-of select="$lang.Skills"/></u></strong></td></tr>
        </xsl:if>

        <tr>
          <xsl:call-template name="make_grey_lines" />
          <td>
            <xsl:value-of select="name" />
            <xsl:if test="grouped = 'True'">*</xsl:if>
            <xsl:if test="spec!='' and exotic = 'True'">
              (<xsl:value-of select="spec" />)
            </xsl:if>
            <span style="color:grey;"><xsl:text> </xsl:text><xsl:value-of select="displayattribute" /></span>
            <xsl:if test="exotic = 'False' and count(skillspecializations/skillspecialization) &gt; 0">
              <p style="padding-left: 1em">
                <xsl:for-each select="skillspecializations/skillspecialization">
                  <xsl:if test="position() != 1">
                    <br />
                  </xsl:if>
                  (<xsl:value-of select="name"/> +<xsl:value-of select="specbonus"/>)
                </xsl:for-each>
              </p>
            </xsl:if>
          </td>
          <td><xsl:value-of select="rating" /></td>
          <td>
            <xsl:value-of select="total" />
          </td>
        </tr>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="print_knowledge_skills">
    <table class="stats general">
      <xsl:variable name="sorted_skills">
        <xsl:for-each select="skills/skill[knowledge = 'True']">
          <xsl:sort select="skillcategory" />
          <xsl:sort select="name" />

          <xsl:copy-of select="current()"/>
        </xsl:for-each>
      </xsl:variable>

      <tr><td colspan="3"><div class="bigheader">[<xsl:value-of select="$lang.KnowledgeSkills"/>]</div></td></tr>
      <tr class="smallheader"><td><xsl:value-of select="$lang.Skill"/></td><td><xsl:value-of select="$lang.Rtg"/></td><td><xsl:value-of select="$lang.Pool"/></td></tr>
      <xsl:for-each select="msxsl:node-set($sorted_skills)/skill">

        <xsl:if test="skillcategory!=preceding-sibling::skill[1]/skillcategory or position()=1">
          <tr><td colspan="3"><strong><u><xsl:value-of select="skillcategory" /></u></strong></td></tr>
        </xsl:if>
        <tr>
          <xsl:call-template name="make_grey_lines" />
          <td>
            <xsl:value-of select="name" />
            <xsl:if test="spec!='' and exotic = 'True'">
              (<xsl:value-of select="spec" />)
            </xsl:if>
            <xsl:if test="exotic = 'False' and count(skillspecializations/skillspecialization) &gt; 0">
              <p style="padding-left: 1em">
                <xsl:for-each select="skillspecializations/skillspecialization">
                  <xsl:if test="position() != 1">
                    <br />
                  </xsl:if>
                  (<xsl:value-of select="name"/> +<xsl:value-of select="specbonus"/>)
                </xsl:for-each>
              </p>
            </xsl:if>
          </td>
          <xsl:choose>
            <xsl:when test="skillcategory_english='Language' and rating=0">
              <td colspan="2"><xsl:value-of select="$lang.Native"/></td>
            </xsl:when>
            <xsl:otherwise>
              <td><xsl:value-of select="rating" /></td>
              <td>
                <xsl:value-of select="total" />
              </td>
            </xsl:otherwise>
          </xsl:choose>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="print_qualities">
    <xsl:param name="double_size" />

    <table class="stats general">
      <xsl:choose>
        <xsl:when test="$double_size">
          <tr><td colspan="2"><div class="bigheader">[<xsl:value-of select="$lang.Qualities"/>]</div></td></tr>
          <tr class="smallheader"><td><xsl:value-of select="$lang.Positive"/></td><td><xsl:value-of select="$lang.Negative"/></td></tr>
          <tr>
            <td style="width:50%;">
              <xsl:call-template name="print_qualities_by_type">
                <xsl:with-param name="quality_type" select="'Positive'" />
              </xsl:call-template>
            </td>
            <td style="width:50%;">
              <xsl:call-template name="print_qualities_by_type">
                <xsl:with-param name="quality_type" select="'Negative'" />
              </xsl:call-template>
            </td>
          </tr>
        </xsl:when>
        <xsl:otherwise>
          <tr><td><div class="bigheader">[<xsl:value-of select="$lang.Qualities"/>]</div></td></tr>
          <tr class="smallheader"><td><xsl:value-of select="$lang.Positive"/></td></tr>
          <tr><td>
            <xsl:call-template name="print_qualities_by_type">
              <xsl:with-param name="quality_type" select="'Positive'" />
            </xsl:call-template>
          </td></tr>
          <tr class="smallheader"><td><xsl:value-of select="$lang.Negative"/></td></tr>
          <tr><td>
            <xsl:call-template name="print_qualities_by_type">
              <xsl:with-param name="quality_type" select="'Negative'" />
            </xsl:call-template>
          </td></tr>
        </xsl:otherwise>
      </xsl:choose>
    </table>
  </xsl:template>

  <xsl:template name="print_qualities_by_type">
    <xsl:param name="quality_type" />

    <ul style="margin-left:0px;">
      <xsl:for-each select="qualities/quality[qualitytype_english=$quality_type]">
        <xsl:sort select="qualitysource='Metatype'" order='descending' />
        <xsl:sort select="name" />

        <li>
          <xsl:if test="qualitysource='Metatype'">
            <xsl:attribute name="style">color:grey;</xsl:attribute>
          </xsl:if>
          <xsl:value-of select="name" />
          <xsl:if test="extra!=''"> (<xsl:value-of select="normalize-space(extra)"/>)</xsl:if>
          <xsl:call-template name="print_source_page" />
          <xsl:call-template name="print_notes" />
        </li>
      </xsl:for-each>
    </ul>
  </xsl:template>

  <xsl:template name="print_contacts">
    <xsl:param name="double_size" />

    <xsl:variable name="sorted_contacts">
      <xsl:for-each select="contacts/contact">
        <xsl:sort select="name" />

        <xsl:copy-of select="current()" />
      </xsl:for-each>
    </xsl:variable>

    <table class="stats general">
      <xsl:choose>
        <xsl:when test="$double_size">
          <tr><td colspan="2"><div class="bigheader">[<xsl:value-of select="$lang.Contacts"/>]</div></td></tr>
          <tr>
            <td style="width:50%;">
              <table style="width:100%; border-collapse: collapse;">
                <tr class="smallheader"><td><xsl:value-of select="$lang.Contact"/></td><td><xsl:value-of select="$lang.Location"/></td><td>C/L</td></tr>
                <xsl:call-template name="print_half_contacts">
                  <xsl:with-param name="contacts" select="$sorted_contacts" />
                  <xsl:with-param name="condition" select="true()" />
                </xsl:call-template>
              </table>
            </td>
            <td style="width:50%;">
              <table style="width:100%; border-collapse: collapse;">
                <tr class="smallheader"><td><xsl:value-of select="$lang.Contact"/></td><td><xsl:value-of select="$lang.Location"/></td><td>C/L</td></tr>
                <xsl:call-template name="print_half_contacts">
                  <xsl:with-param name="contacts" select="$sorted_contacts" />
                  <xsl:with-param name="condition" select="false()" />
                </xsl:call-template>
              </table>
            </td>
          </tr>
        </xsl:when>
        <xsl:otherwise>
          <tr><td colspan="3"><div class="bigheader">[<xsl:value-of select="$lang.Contacts"/>]</div></td></tr>
          <tr class="smallheader"><td><xsl:value-of select="$lang.Contact"/></td><td><xsl:value-of select="$lang.Location"/></td><td>C/L</td></tr>
          <xsl:call-template name="print_half_contacts">
            <xsl:with-param name="contacts" select="$sorted_contacts" />
            <xsl:with-param name="condition" select="true()" />
          </xsl:call-template>
          <xsl:call-template name="print_half_contacts">
            <xsl:with-param name="contacts" select="$sorted_contacts" />
            <xsl:with-param name="condition" select="false()" />
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </table>
  </xsl:template>

  <xsl:template name="print_half_contacts">
    <xsl:param name="contacts" />
    <xsl:param name="condition" />

    <xsl:variable name="half_count" select="ceiling(count(msxsl:node-set($contacts)/contact) div 2) + 1" />

    <xsl:for-each select="msxsl:node-set($contacts)/contact">
      <xsl:if test="(position() &lt; $half_count)=$condition">
        <tr>
          <xsl:call-template name="make_grey_lines" />
          <td>
            <xsl:value-of select="name" />
            <xsl:if test="role != ''"> (<xsl:value-of select="role" />)</xsl:if>
          </td>
          <td><xsl:value-of select="location" /></td>
          <td><xsl:value-of select="connection" />/<xsl:value-of select="loyalty" /></td>
        </tr>
        <xsl:if test="notes!=''">
          <tr>
            <xsl:call-template name="make_grey_lines" />
            <td colspan="3">
              <xsl:call-template name="print_notes"><xsl:with-param name="linebreak" select="false()" /></xsl:call-template>
            </td>
          </tr>
        </xsl:if>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="print_ranged_weapons">
    <xsl:variable name="sorted_ranged">
      <xsl:for-each select="weapons/weapon[type='Ranged']">
        <xsl:sort select="location" />
        <xsl:sort select="name" />

        <xsl:copy>
          <xsl:if test="../../gears/gear[name=current()/name]">
            <xsl:element name="qty"><xsl:value-of select="../../gears/gear[name=current()/name]/qty" /></xsl:element>
          </xsl:if>
          <xsl:copy-of select="*" />
        </xsl:copy>
      </xsl:for-each>
    </xsl:variable>

    <xsl:variable name="sorted_ammunition">
      <xsl:for-each select="gears/gear[isammo='True']">
        <xsl:sort select="name" />

        <xsl:if test="count(../../weapons/weapon[name=current()/name]) = 0">
          <xsl:copy-of select="current()" />
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>

    <xsl:if test="count(msxsl:node-set($sorted_ranged)/*) &gt; 0  or  count(msxsl:node-set($sorted_ammunition)/*) &gt; 0">

      <xsl:variable name="need_location" select="count(msxsl:node-set($sorted_ranged)/weapon[location!='']) &gt; 0" />

      <table class="stats armory">
        <tr><td colspan="8"><div class="bigheader">[<xsl:value-of select="$lang.RangedWeapons"/>]</div></td></tr>
        <tr class="smallheader"><td><xsl:value-of select="$lang.Weapon"/></td><td><xsl:value-of select="$lang.DV"/></td><td><xsl:value-of select="$lang.AP"/></td><td><xsl:value-of select="$lang.Mode"/></td><td><xsl:value-of select="$lang.RC"/></td><td><xsl:value-of select="$lang.Ammo"/></td><td><xsl:value-of select="$lang.Accuracy"/></td><td><xsl:value-of select="$lang.Pool"/></td></tr>

        <xsl:for-each select="msxsl:node-set($sorted_ranged)/weapon">
          <xsl:if test="$need_location and (position()=1 or location!=preceding-sibling::weapon[1]/location)">
            <tr>
              <td colspan="8">
                <xsl:call-template name="print_location" />
              </td>
            </tr>
          </xsl:if>

          <xsl:call-template name="print_ranged_weapon_stats" />
        </xsl:for-each>

        <xsl:if test="count(msxsl:node-set($sorted_ammunition)/*) &gt; 0">
          <tr><td colspan="8"><hr /></td></tr>

          <xsl:for-each select="msxsl:node-set($sorted_ammunition)/*">

            <tr>
              <td>
                <xsl:value-of select="name" />
                <xsl:if test="extra!=''"> (<xsl:value-of select="extra" />)</xsl:if>
                <xsl:if test="qty &gt; 1">
                  <xsl:text> ×</xsl:text><xsl:value-of select="qty" />
                </xsl:if>
                <xsl:call-template name="print_source_page" />
                <xsl:call-template name="print_notes" />
              </td>
              <td><xsl:value-of select="weaponbonusdamage" /></td>
              <td><xsl:value-of select="weaponbonusap" /></td>
              <td colspan="5"></td>
            </tr>
          </xsl:for-each>
        </xsl:if>
      </table>

    </xsl:if>
  </xsl:template>

  <xsl:template name="print_ranged_weapon_stats">
    <xsl:param name="is_mount" select="false()" />

    <tr>
      <xsl:if test="$is_mount">
        <td rowspan="2">
          <xsl:call-template name="short_mount_name" />
        </td>
      </xsl:if>
      <td rowspan="2">
        <xsl:call-template name="print_nested" />
        <ul>
          <xsl:for-each select="accessories/accessory">
            <xsl:sort select="included" order="descending" />
            <xsl:sort select="name" />

            <li><xsl:call-template name="print_nested" /></li>
          </xsl:for-each>
        </ul>
      </td>
      <td><xsl:value-of select="damage" /></td>
      <td><xsl:value-of select="ap" /></td>
      <td><xsl:value-of select="mode" /></td>
      <td><xsl:value-of select="rc" /></td>
      <td><xsl:value-of select="ammo" /></td>
      <td><xsl:value-of select="accuracy" /></td>
      <td><xsl:value-of select="dicepool" /></td>
    </tr>
    <tr>
      <td colspan="7">
        <xsl:if test="ranges/name != '' or alternateranges/name != ''">
          <table style="border-style:solid; border-width:1px; border-color:lightgrey; width:100%; cellpadding: 2; cellspacing: 0;">
            <tr>
              <th style="text-align: center; vertical-align: middle; width: 36%;">
                <xsl:value-of select="$lang.Range"/>
              </th>
              <th style="text-align: center; vertical-align: middle; width: 16%;">
                <xsl:value-of select="$lang.S"/>
              </th>
              <th style="text-align: center; vertical-align: middle; width: 16%;">
                <xsl:value-of select="$lang.M"/>
              </th>
              <th style="text-align: center; vertical-align: middle; width: 16%;">
                <xsl:value-of select="$lang.L"/>
              </th>
              <th style="text-align: center; vertical-align: middle; width: 16%;">
                <xsl:value-of select="$lang.E"/>
              </th>
            </tr>
            <xsl:for-each select="ranges | alternateranges">
              <xsl:if test="name != ''">
                <tr>
                  <td style="text-align: center; vertical-align: middle;">
                    <xsl:value-of select="name"/>
                  </td>
                  <td style="text-align: center; vertical-align: middle;">
                    <xsl:value-of select="short"/>
                  </td>
                  <td style="text-align: center; vertical-align: middle;">
                    <xsl:value-of select="medium"/>
                  </td>
                  <td style="text-align: center; vertical-align: middle;">
                    <xsl:value-of select="long"/>
                  </td>
                  <td style="text-align: center; vertical-align: middle;">
                    <xsl:value-of select="extreme"/>
                  </td>
                </tr>
              </xsl:if>
            </xsl:for-each>
          </table>
        </xsl:if>
      </td>
    </tr>
    <xsl:if test="count(underbarrel/weapon) &gt; 0">
      <tr><td colspan="8"><ul>
      <xsl:if test="count(underbarrel/weapon[type='Ranged']) &gt; 0">
        <li><table>
          <tr class="smallheader"><td>
            <xsl:value-of select="$lang.Under"/>
          </td><td><xsl:value-of select="$lang.DV"/></td><td><xsl:value-of select="$lang.AP"/></td><td><xsl:value-of select="$lang.Mode"/></td><td><xsl:value-of select="$lang.RC"/></td><td><xsl:value-of select="$lang.Ammo"/></td><td><xsl:value-of select="$lang.Accuracy"/></td><td><xsl:value-of select="$lang.Pool"/></td></tr>
          <xsl:for-each select="underbarrel/weapon[type='Ranged']">
            <xsl:sort select="name" />
            <xsl:call-template name="print_ranged_weapon_stats" />
          </xsl:for-each>
        </table></li>
      </xsl:if>
      <xsl:if test="count(underbarrel/weapon[type='Melee']) &gt; 0">
        <li><table>
          <tr class="smallheader">
            <td>
              <xsl:value-of select="$lang.Under"/>
            </td><td><xsl:value-of select="$lang.DV"/></td><td><xsl:value-of select="$lang.AP"/></td><td><xsl:value-of select="$lang.Mode"/></td><td><xsl:value-of select="$lang.RC"/></td><td><xsl:value-of select="$lang.Ammo"/></td><td><xsl:value-of select="$lang.Accuracy"/></td><td><xsl:value-of select="$lang.Pool"/></td></tr>
          <xsl:for-each select="underbarrel/weapon[type='Melee']">
            <xsl:sort select="name" />
            <xsl:call-template name="print_melee_weapon_stats" />
          </xsl:for-each>
        </table></li>
      </xsl:if>
      </ul></td></tr>
    </xsl:if>
  </xsl:template>

  <xsl:template name="print_melee_weapons">
    <xsl:variable name="sorted_melee">
      <xsl:for-each select="weapons/weapon[type='Melee']">
        <xsl:sort select="location" />
        <xsl:sort select="name" />

        <xsl:copy-of select="current()" />
      </xsl:for-each>
    </xsl:variable>

    <xsl:variable name="need_location" select="count(msxsl:node-set($sorted_melee)/weapon[location!='']) &gt; 0" />

    <table class="stats armory">
      <tr><td colspan="6"><div class="bigheader">[<xsl:value-of select="$lang.MeleeWeapons"/>]</div></td></tr>
      <tr class="smallheader"><td><xsl:value-of select="$lang.Weapon"/></td><td><xsl:value-of select="$lang.DV"/></td><td><xsl:value-of select="$lang.AP"/></td><td><xsl:value-of select="$lang.Reach"/></td><td><xsl:value-of select="$lang.Accuracy"/></td><td><xsl:value-of select="$lang.Pool"/></td></tr>

      <xsl:for-each select="msxsl:node-set($sorted_melee)/weapon">
        <xsl:if test="$need_location and (position()=1 or location!=preceding-sibling::weapon[1]/location)">
          <tr>
            <td colspan="6">
              <xsl:call-template name="print_location" />
            </td>
          </tr>
        </xsl:if>

        <xsl:call-template name="print_melee_weapon_stats" />
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="print_melee_weapon_stats">
    <xsl:param name="is_mount" select="false()" />

    <tr>
      <xsl:if test="$is_mount">
        <td>
          <xsl:call-template name="short_mount_name" />
        </td>
      </xsl:if>
      <td>
        <xsl:value-of select="name" />
        <xsl:call-template name="print_source_page" />
        <xsl:call-template name="print_notes" />
        <ul>
          <xsl:for-each select="accessories/accessory">
            <xsl:sort select="included" order="descending" />
            <xsl:sort select="name" />

            <li><xsl:call-template name="print_nested" /></li>
          </xsl:for-each>
        </ul>
      </td>
      <td><xsl:value-of select="damage" /></td>
      <td><xsl:value-of select="ap" /></td>
      <td><xsl:value-of select="reach" /></td>
      <td><xsl:value-of select="accuracy" /></td>
      <td><xsl:value-of select="dicepool" /></td>
    </tr>
  </xsl:template>

  <xsl:template name="print_armor">
    <xsl:variable name="sorted_armor">
      <xsl:for-each select="armors/armor">
        <xsl:sort select="location" />
        <xsl:sort select="name" />

        <xsl:copy-of select="current()" />
      </xsl:for-each>
    </xsl:variable>

    <xsl:variable name="need_location" select="count(msxsl:node-set($sorted_armor)/armor[location!='']) &gt; 0" />

    <table class="stats armory">
      <tr><td colspan="3"><div class="bigheader">[<xsl:value-of select="$lang.Armor"/>]</div></td></tr>
      <tr class="smallheader"><td></td><td><xsl:value-of select="$lang.Armor"/></td><td><xsl:value-of select="$lang.ArmorValue"/></td></tr>

      <xsl:for-each select="msxsl:node-set($sorted_armor)/armor">
        <xsl:if test="$need_location and (position()=1 or location!=preceding-sibling::armor[1]/location)">
          <tr>
            <td colspan="3">
              <xsl:call-template name="print_location" />
            </td>
          </tr>
        </xsl:if>

        <tr>
          <td>
            <xsl:choose>
              <xsl:when test="equipped='True'"> &#x26AB;</xsl:when>
              <xsl:otherwise> &#x26AA;</xsl:otherwise>
            </xsl:choose>
          </td>
          <td>

            <xsl:value-of select="name" />
            <xsl:call-template name="print_source_page" />
            <xsl:call-template name="print_notes" />
            <ul>
              <xsl:for-each select="armormods/armormod|gears/gear">
                <xsl:sort select="included" order="descending" />
                <xsl:sort select="name" />

                <li><xsl:call-template name="print_nested" /></li>
              </xsl:for-each>
            </ul>
          </td>
          <td style="text-align: right">
            <xsl:value-of select="armor" />
          </td>
        </tr>
      </xsl:for-each>

      <tr>
        <td></td>
        <td><strong><xsl:value-of select="$lang.Total"/></strong></td>
        <td style="text-align: right"><strong><xsl:value-of select="armor" /></strong></td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template name="print_martial_arts">
    <xsl:if test="count(martialarts/martialart) &gt; 0">

      <table class="stats armory">
        <tr><td><div class="bigheader">[<xsl:value-of select="$lang.MartialArts"/>]</div></td></tr>
        <tr><td>
          <ul>
            <xsl:for-each select="martialarts/martialart">
              <xsl:sort select="name" />

              <li>
                <xsl:value-of select="name" />
                <xsl:call-template name="print_source_page" />
                <xsl:call-template name="print_notes" />
                <ul>
                  <xsl:for-each select="martialarttechniques/martialarttechnique">
                    <xsl:sort select="name" />
                    <li>
                      <xsl:value-of select="name" />
                      <xsl:call-template name="print_source_page" />
                      <xsl:call-template name="print_notes" />
                    </li>
                  </xsl:for-each>
                </ul>
              </li>
            </xsl:for-each>
          </ul>
        </td></tr>
      </table>

    </xsl:if>
  </xsl:template>

  <xsl:template name="print_vehicles">
    <xsl:if test="count(vehicles/vehicle) &gt; 0">

      <table class="stats machine">
        <tr><td colspan="2"><div class="bigheader">[<xsl:value-of select="$lang.Vehicles"/>]</div></td></tr>
        <xsl:for-each select="vehicles/vehicle">
          <xsl:sort select="name" />

          <xsl:if test="position()!=1">
            <tr><td colspan="2"><hr /></td></tr>
          </xsl:if>

          <tr>
            <td>
              <strong><xsl:value-of select="name" /></strong>
              <xsl:call-template name="print_source_page" />
              <xsl:call-template name="print_notes" />
              <ul>
                <xsl:if test="count(mods/mod) &gt; 0">
                  <li><strong><xsl:value-of select="$lang.Mod"/></strong></li>
                  <xsl:for-each select="mods/mod[not(contains(name, 'Weapon Mount'))]">
                    <xsl:sort select="included" order="descending" />
                    <xsl:sort select="name" />

                    <li><xsl:call-template name="print_nested" /></li>
                  </xsl:for-each>
                </xsl:if>
                <xsl:if test="count(gears/gear) &gt; 0">
                  <li><strong><xsl:value-of select="$lang.Gear"/></strong></li>
                  <xsl:for-each select="gears/gear">
                    <xsl:sort select="name" />

                    <li><xsl:call-template name="print_nested" /></li>
                  </xsl:for-each>
                </xsl:if>
              </ul>
            </td>
            <td>
              <table style="width:100%;">
                <tr><td><xsl:value-of select="$lang.Handling"/></td><td><xsl:value-of select="handling" /></td><td><xsl:value-of select="$lang.Body"/></td><td><xsl:value-of select="body" /></td></tr>
                <tr><td><xsl:value-of select="$lang.Acceleration"/></td><td><xsl:value-of select="accel" /></td><td><xsl:value-of select="$lang.Armor"/></td><td><xsl:value-of select="armor" /></td></tr>
                <tr><td><xsl:value-of select="$lang.Speed"/></td><td><xsl:value-of select="speed" /></td><td><xsl:value-of select="$lang.Sensor"/></td><td><xsl:value-of select="sensor" /></td></tr>
                <tr><td><xsl:value-of select="$lang.Pilot"/></td><td><xsl:value-of select="pilot" /></td><td><xsl:value-of select="$lang.DeviceRating"/></td><td><xsl:value-of select="devicerating" /></td></tr>
                <tr>
                  <td><xsl:value-of select="$lang.PhysicalTrack"/></td><td><xsl:value-of select="physicalcm" /></td>
                  <xsl:if test="seats &gt; 0"><td><xsl:value-of select="$lang.Seats"/></td><td><xsl:value-of select="seats" /></td></xsl:if>
                </tr>
                <tr><td><xsl:value-of select="$lang.MatrixTrack"/></td><td><xsl:value-of select="matrixcm" /></td></tr>
              </table>
            </td>
          </tr>

          <xsl:if test="count(mods/mod[contains(name, 'Weapon Mount')]/weapons/weapon[type='Ranged']) &gt; 0">
            <tr>
              <td colspan="2">
                <table>
                  <tr class="smallheader"><td><xsl:value-of select="$lang.Mount"/></td><td><xsl:value-of select="$lang.Weapon"/></td><td><xsl:value-of select="$lang.DV"/></td><td><xsl:value-of select="$lang.AP"/></td><td><xsl:value-of select="$lang.Mode"/></td><td><xsl:value-of select="$lang.RC"/></td><td><xsl:value-of select="$lang.Ammo"/></td><td><xsl:value-of select="$lang.Accuracy"/></td><td><xsl:value-of select="$lang.Pool"/></td></tr>
                  <xsl:for-each select="mods/mod[contains(name, 'Weapon Mount')]/weapons/weapon[type='Ranged']">
                    <xsl:sort select="name" />

                    <xsl:call-template name="print_ranged_weapon_stats">
                      <xsl:with-param name="is_mount" select="true()" />
                    </xsl:call-template>
                  </xsl:for-each>
                </table>
              </td>
            </tr>
          </xsl:if>

          <xsl:if test="count(mods/mod[contains(name, 'Weapon Mount')]/weapons/weapon[type='Melee']) &gt; 0">
            <tr>
              <td colspan="2">
                <table>
                  <tr class="smallheader"><td><xsl:value-of select="$lang.Weapon"/></td><td><xsl:value-of select="$lang.Weapon"/></td><td><xsl:value-of select="$lang.DV"/></td><td><xsl:value-of select="$lang.AP"/></td><td><xsl:value-of select="$lang.Reach"/></td><td><xsl:value-of select="$lang.Accuracy"/></td><td><xsl:value-of select="$lang.Pool"/></td></tr>
                  <xsl:for-each select="mods/mod[contains(name, 'Weapon Mount')]/weapons/weapon[type='Melee']">
                    <xsl:sort select="name" />

                    <xsl:call-template name="print_melee_weapon_stats">
                      <xsl:with-param name="is_mount" select="true()" />
                    </xsl:call-template>
                  </xsl:for-each>
                </table>
              </td>
            </tr>
          </xsl:if>
        </xsl:for-each>
      </table>

    </xsl:if>
  </xsl:template>

  <xsl:template name="short_mount_name">
    <xsl:choose>
      <xsl:when test="contains(../../name, 'Heavy')"><xsl:value-of select="$lang.Heavy"/></xsl:when>
      <xsl:otherwise><xsl:value-of select="$lang.Standard"/></xsl:otherwise>
    </xsl:choose>
    <xsl:if test="contains(../../name, 'Manual')">
      <xsl:text>, </xsl:text><xsl:value-of select="$lang.Manual"/>
    </xsl:if>
  </xsl:template>

  <xsl:template name="print_implants">
    <xsl:if test="count(cyberwares/cyberware) &gt; 0">

      <table class="stats machine">
        <tr><td colspan="3"><div class="bigheader">[<xsl:value-of select="$lang.Cyberware"/>]</div></td></tr>
        <tr class="smallheader"><td><xsl:value-of select="$lang.Implant"/></td><td><xsl:value-of select="$lang.Essence"/></td><td><xsl:value-of select="$lang.Grade"/></td></tr>

        <xsl:for-each select="cyberwares/cyberware">
          <xsl:sort select="name" />

          <tr>
            <td>
              <xsl:call-template name="print_imp_nested" />
            </td>
            <td><xsl:value-of select="ess" /></td>
            <td><xsl:value-of select="grade" /></td>
          </tr>
        </xsl:for-each>
      </table>

    </xsl:if>
  </xsl:template>

  <xsl:template name="print_imp_nested">
    <xsl:choose>
      <!-- Cyberlimb name contains its stats in parenthesis, so I split it to two lines -->
      <xsl:when test="category='Cyberlimb' and contains(name, '(')">
        <xsl:value-of select="substring-before(name, '(')" />
        <xsl:if test="rating &gt; 0"> [R<xsl:value-of select="rating" />]</xsl:if>
        <xsl:call-template name="print_source_page" />
        <br />
        (<xsl:value-of select="substring-after(name, '(')" />
        <xsl:call-template name="print_notes" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="name" />
        <xsl:if test="rating &gt; 0"> [R<xsl:value-of select="rating" />]</xsl:if>
        <xsl:if test="extra != ''"> (<xsl:value-of select="extra"/>)</xsl:if>
        <xsl:call-template name="print_source_page" />
        <xsl:call-template name="print_notes" />
      </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="count(children/cyberware) &gt; 0 or count(gears/gear) &gt; 0">
      <ul>
        <xsl:for-each select="children/cyberware">
          <xsl:sort select="name" />
          <li><xsl:call-template name="print_imp_nested" /></li>
        </xsl:for-each>
        <xsl:for-each select="gears/gear">
          <xsl:sort select="name" />
          <li><xsl:call-template name="print_nested" /></li>
        </xsl:for-each>
      </ul>
    </xsl:if>
  </xsl:template>

  <xsl:template name="print_matrix_devices">
    <xsl:variable name="sorted_devices">
      <xsl:for-each select="//gear[category_english='Commlinks' or category_english='Commlink' or category_english='Cyberdecks' or category_english='Rigger Command Consoles']">
        <xsl:sort select="category" />
        <xsl:sort select="name" />

        <xsl:copy-of select="current()" />
      </xsl:for-each>
    </xsl:variable>

    <xsl:if test="count(msxsl:node-set($sorted_devices)/*) &gt; 0">

      <table class="stats matrix">
        <tr><td colspan="5"><div class="bigheader">[<xsl:value-of select="$lang.MatrixDevices"/>]</div></td></tr>
        <tr class="smallheader"><td><xsl:value-of select="$lang.Device"/></td><td><xsl:value-of select="$lang.DeviceRating"/></td><td>CM</td><td><xsl:value-of select="$lang.ASDF"/></td><td><xsl:value-of select="$lang.Programs"/></td></tr>

        <xsl:for-each select="msxsl:node-set($sorted_devices)/*">
          <xsl:call-template name="print_matrix_device_stats" />
        </xsl:for-each>
      </table>

    </xsl:if>
  </xsl:template>

  <xsl:template name="print_matrix_device_stats">
    <xsl:variable name="sorted_everything">
      <xsl:for-each select="children/gear[category_english='Specialty Condition']">
        <xsl:sort select="name" />

        <xsl:copy>
          <xsl:element name="category_tag"><xsl:value-of select="$lang.Special"/></xsl:element>
          <xsl:copy-of select="*" />
        </xsl:copy>
      </xsl:for-each>

      <xsl:for-each select="children/gear[category_english='Cyberdeck Modules' or category_english='Electronic Modification']">
        <xsl:sort select="name" />

        <xsl:copy>
          <xsl:element name="category_tag"><xsl:value-of select="$lang.Modifications"/></xsl:element>
          <xsl:copy-of select="*" />
        </xsl:copy>
      </xsl:for-each>

      <xsl:for-each select="children/gear[category_english='Common Programs' or category_english='Hacking Programs' or category_english='Builtin Programs' or category_english='Software']">
        <xsl:sort select="category_english='Builtin Programs'" order="descending"/>
        <xsl:sort select="name" />

        <xsl:copy>
          <xsl:element name="category_tag"><xsl:value-of select="$lang.Programs"/></xsl:element>
          <xsl:copy-of select="*" />
        </xsl:copy>
      </xsl:for-each>

      <xsl:for-each select="children/gear[category_english!='Specialty Condition' and category_english!='Cyberdeck Modules' and category_english!='Electronic Modification' and category_english!='Builtin Programs' and category_english!='Common Programs' and category_english!='Hacking Programs' and category_english!='Software']">
        <xsl:sort select="name" />

        <xsl:copy>
          <xsl:element name="category_tag"><xsl:value-of select="$lang.Gear"/></xsl:element>
          <xsl:copy-of select="*" />
        </xsl:copy>
      </xsl:for-each>
    </xsl:variable>

    <tr>
      <td>
        <strong><xsl:value-of select="name" /></strong>
        <xsl:if test="qty &gt; 1"> ×<xsl:value-of select="qty" /></xsl:if>
        <xsl:call-template name="print_source_page" />
      </td>
      <td><xsl:value-of select="devicerating" /></td>
      <td><xsl:value-of select="8 + ceiling(devicerating div 2)" /></td>
      <td>
        <xsl:value-of select="attack" />/<xsl:value-of select="sleaze" />/<xsl:value-of select="dataprocessing" />/<xsl:value-of select="firewall" />
      </td>
      <td><xsl:value-of select="processorlimit" /></td>
    </tr>
    <xsl:if test="notes!=''">
      <tr>
        <td colspan="5">
          <xsl:call-template name="print_notes"><xsl:with-param name="linebreak" select="false()" /></xsl:call-template>
        </td>
      </tr>
    </xsl:if>
    <tr>
      <td>
        <ul>
          <xsl:call-template name="print_matrix_device_nested_list_by_half">
            <xsl:with-param name="list" select="$sorted_everything" />
            <xsl:with-param name="condition" select="true()" />
          </xsl:call-template>
        </ul>
      </td>
      <td colspan="4">
        <ul>
          <xsl:call-template name="print_matrix_device_nested_list_by_half">
            <xsl:with-param name="list" select="$sorted_everything" />
            <xsl:with-param name="condition" select="false()" />
          </xsl:call-template>
        </ul>
      </td>
    </tr>
  </xsl:template>

  <xsl:template name="print_matrix_device_nested_list_by_half">
    <xsl:param name="list" />
    <xsl:param name="condition" />

    <xsl:variable name="list_half_count" select="ceiling(count(msxsl:node-set($list)/*) div 2) + 1" />

    <xsl:for-each select="msxsl:node-set($list)/*">
      <xsl:if test="(position() &lt; $list_half_count)=$condition">

        <xsl:if test="( category_tag!=preceding-sibling::gear[1]/category_tag or position()=1 ) and category_tag!='Special'">
          <li><strong><xsl:value-of select="category_tag" /></strong></li>
        </xsl:if>
        <li>
          <xsl:if test="category_english='Builtin Programs'">
            <xsl:attribute name="style">color:grey;</xsl:attribute>
          </xsl:if>
          <xsl:call-template name="print_nested" />
        </li>

      </xsl:if>
    </xsl:for-each>

  </xsl:template>

  <xsl:template name="print_sprites">
    <xsl:if test="count(spirits/spirit) &gt; 0">

      <table class="stats matrix">
        <tr><td colspan="2"><div class="bigheader">[<xsl:value-of select="$lang.Sprites"/>]</div></td></tr>
        <xsl:for-each select="spirits/spirit">
          <xsl:sort select="name" />
          <xsl:sort select="crittername" />

          <tr>
            <td>
              <xsl:value-of select="name" />
              <xsl:if test="crittername!=''"> (<xsl:value-of select="crittername" />)</xsl:if>
              <xsl:choose>
                <xsl:when test="bound='True'"> (<xsl:value-of select="$lang.Bound"/>)</xsl:when>
                <xsl:otherwise> (<xsl:value-of select="$lang.Unbound"/>)</xsl:otherwise>
              </xsl:choose>
              <xsl:call-template name="print_source_page" />
              <xsl:call-template name="print_notes" />
              <br />
              <table>
                <tr class="smallheader"><td><xsl:value-of select="$lang.Skill"/></td><td><xsl:value-of select="$lang.Pool"/></td></tr>
                <xsl:for-each select="skills/skill">
                  <tr>
                    <td>
                      <xsl:value-of select="name" />
                      <span style="color:grey; text-transform: uppercase;"><xsl:text> </xsl:text>
                        <xsl:choose>
                          <xsl:when test="attr=cha">A</xsl:when>
                          <xsl:when test="attr=int">S</xsl:when>
                          <xsl:when test="attr=log">D</xsl:when>
                          <xsl:when test="attr=wil">F</xsl:when>
                        </xsl:choose>
                      </span>
                    </td>
                    <td><xsl:value-of select="pool" /></td>
                  </tr>
                </xsl:for-each>
              </table>
            </td>
            <td>
              <table style="width:100%;">
                <td><xsl:value-of select="$lang.Level"/></td><td><xsl:value-of select="force" /></td><td><xsl:value-of select="$lang.Tasks"/></td><td><xsl:value-of select="services" /></td>
                <tr><td><xsl:value-of select="$lang.Attack"/></td><td><xsl:value-of select="spiritattributes/cha" /></td><td><xsl:value-of select="$lang.DataProcessing"/></td><td><xsl:value-of select="spiritattributes/log" /></td></tr>
                <tr><td><xsl:value-of select="$lang.Sleaze"/></td><td><xsl:value-of select="spiritattributes/int" /></td><td><xsl:value-of select="$lang.Firewall"/></td><td><xsl:value-of select="spiritattributes/wil" /></td></tr>
                <tr>
                  <td><xsl:value-of select="$lang.MatrixTrack"/></td><td><xsl:value-of select="8 + ceiling(force div 2)" /></td>
                  <td><xsl:value-of select="$lang.Initiative"/></td><td><xsl:value-of select="spiritattributes/ini" /><xsl:text>+4d6</xsl:text></td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td>
              <xsl:value-of select="$lang.Powers"/>
              <xsl:choose>
                <xsl:when test="count(powers/critterpower) &gt; 0">
                  <ul style="margin-left:5px;">
                    <xsl:for-each select="powers/critterpower">
                      <li>
                        <xsl:value-of select="name" />
                        <xsl:call-template name="print_source_page" />
                      </li>
                    </xsl:for-each>
                  </ul>
                </xsl:when>
                <xsl:otherwise>
                  <span style="margin-left:5px;">
                    <xsl:value-of select="$lang.None"/>
                  </span>
                </xsl:otherwise>
              </xsl:choose>
            </td>
            <td>
              <xsl:if test="count(optionalpowers/critterpower) &gt; 0">
                <strong>
                  <xsl:value-of select="$lang.OptionalPowers"/>
                </strong>
                <ul style="margin-left:5px;">
                  <xsl:for-each select="optionalpowers/critterpower">
                    <li>
                      <xsl:value-of select="name" />
                      <xsl:if test="extra!=''">
                        (<xsl:value-of select="extra" />)
                      </xsl:if>
                      <xsl:call-template name="print_source_page" />
                    </li>
                  </xsl:for-each>
                </ul>
              </xsl:if>
              <xsl:if test="count(weaknesses/critterpower) &gt; 0">
                <strong>
                  <xsl:value-of select="$lang.Weaknesses"/>
                </strong>
                <ul style="margin-left:5px;">
                  <xsl:for-each select="weaknesses/critterpower">
                    <li>
                      <xsl:value-of select="name" />
                      <xsl:if test="extra!=''">
                        (<xsl:value-of select="extra" />)
                      </xsl:if>
                      <xsl:call-template name="print_source_page" />
                    </li>
                  </xsl:for-each>
                </ul>
              </xsl:if>
            </td>
          </tr>

          <xsl:if test="position() != last()">
            <tr><td colspan="2"><hr /></td></tr>
          </xsl:if>
        </xsl:for-each>
      </table>

    </xsl:if>
  </xsl:template>

  <xsl:template name="print_complex_forms">
    <xsl:if test="count(complexforms/complexform) &gt; 0">

      <table class="stats matrix">
        <tr><td colspan="4"><div class="bigheader">[<xsl:value-of select="$lang.ComplexForms"/>]</div></td></tr>
        <tr class="smallheader"><td><xsl:value-of select="$lang.Name"/></td><td><xsl:value-of select="$lang.Target"/></td><td><xsl:value-of select="$lang.Duration"/></td><td><xsl:value-of select="$lang.FadingValue"/></td></tr>

        <xsl:for-each select="complexforms/complexform">
          <xsl:sort select="name" />

          <tr>
            <td>
              <xsl:value-of select="name" />
              <xsl:if test="extra!=''"> (<xsl:value-of select="extra" />)</xsl:if>
            </td>
            <td><xsl:value-of select="target" /></td>
            <td style="text-align: center"><xsl:value-of select="duration" /></td>
            <td style="text-align: center"><xsl:value-of select="fv" /></td>
          </tr>
          <tr>
            <td colspan="4">
              <xsl:call-template name="print_source_page" />
              <xsl:call-template name="print_notes" />
            </td>
          </tr>
        </xsl:for-each>
      </table>

    </xsl:if>
  </xsl:template>

  <xsl:template name="print_submersion">
    <xsl:if test="submersiongrade &gt; 0">

      <table class="stats matrix">
        <tr><td><div class="bigheader">[<xsl:value-of select="$lang.Submersion"/>]</div></td></tr>
        <tr><td>
          <strong><xsl:value-of select="$lang.SubmersionGrade"/><xsl:text> </xsl:text><xsl:value-of select="submersiongrade" /></strong>
          <br />
            <strong><xsl:value-of select="$lang.Echoes"/></strong>
            <ul>
              <xsl:for-each select="metamagics/metamagic">
                <xsl:sort select="name" />

                <li>
                  <xsl:value-of select="name" />
                  <xsl:call-template name="print_source_page" />
                  <xsl:call-template name="print_notes" />
                </li>
              </xsl:for-each>
            </ul>
        </td></tr>
      </table>

    </xsl:if>
  </xsl:template>

  <xsl:template name="print_ai_programs">
    <xsl:if test="count(aiprograms/aiprogram) &gt; 0">

      <table class="stats matrix">
        <tr>
          <td colspan="2">
            <div class="bigheader">[<xsl:value-of select="$lang.AIandAdvanced"/>]</div>
          </td>
        </tr>
        <tr class="smallheader">
          <td><xsl:value-of select="$lang.Name"/></td>
          <td><xsl:value-of select="$lang.Requires"/></td>
        </tr>

        <xsl:for-each select="aiprograms/aiprogram">
          <xsl:sort select="name" />

          <tr>
            <td>
              <xsl:value-of select="name" />
              <xsl:if test="extra!=''">
                (<xsl:value-of select="extra" />)
              </xsl:if>
              <xsl:call-template name="print_source_page" />
              <xsl:call-template name="print_notes" />
            </td>
            <td>
              <xsl:value-of select="requiresprogram" />
            </td>
          </tr>
        </xsl:for-each>
      </table>

    </xsl:if>
  </xsl:template>

  <xsl:template name="print_magic">
    <table class="stats magic">
      <tr><td colspan="4"><div class="bigheader">[<xsl:value-of select="$lang.Magic"/>]</div></td></tr>
      <tr>
        <td><xsl:value-of select="$lang.Awakened"/></td>
        <td colspan="3"><strong>
          <xsl:choose>
            <xsl:when test="qualities/quality[name='Adept']"><xsl:value-of select="$lang.Adept"/></xsl:when>
            <xsl:when test="qualities/quality[name='Mystic Adept']"><xsl:value-of select="$lang.MysticAdept"/></xsl:when>
            <xsl:when test="qualities/quality[name='Magician']"><xsl:value-of select="$lang.Magician"/></xsl:when>
            <xsl:when test="qualities/quality[name='Aspected Magician']"><xsl:value-of select="$lang.AspectedMagician"/></xsl:when>
            <xsl:when test="qualities/quality[name='Enchanter']"><xsl:value-of select="$lang.Enchanter"/></xsl:when>
            <xsl:when test="qualities/quality[name='Explorer']"><xsl:value-of select="$lang.Explorer"/></xsl:when>
            <xsl:when test="qualities/quality[name='Apprentice']"><xsl:value-of select="$lang.Apprentice"/></xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="magenabled = 'True'"><xsl:value-of select="$lang.Aware"/></xsl:when>
                <xsl:otherwise><xsl:value-of select="$lang.Other"/></xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </strong></td>
      </tr>
      <xsl:if test="tradition and tradition/istechnomancertradition = 'False'">
        <tr>
          <td><xsl:value-of select="$lang.Tradition"/></td>
          <td colspan="3">
            <strong><xsl:value-of select="tradition/name" /> <span style="color:grey; font-size: 10px; vertical-align: bottom;"> (<xsl:value-of select="tradition/spiritform" />)</span></strong>
            <span style="color:grey;">
              <xsl:text> </xsl:text><xsl:value-of select="tradition/source" />
              <xsl:text> </xsl:text><xsl:value-of select="tradition/page" />
            </span>
          </td>
        </tr>
        <tr>
          <td><xsl:value-of select="$lang.Combat"/></td><td><strong><xsl:value-of select="tradition/spiritcombat" /></strong></td>
          <td><xsl:value-of select="$lang.Detection"/></td><td><strong><xsl:value-of select="tradition/spiritdetection" /></strong></td>
        </tr>
        <tr>
          <td><xsl:value-of select="$lang.Health"/></td><td><strong><xsl:value-of select="tradition/spirithealth" /></strong></td>
          <td><xsl:value-of select="$lang.Illusion"/></td><td><strong><xsl:value-of select="tradition/spiritillusion" /></strong></td>
        </tr>
        <tr>
          <td><xsl:value-of select="$lang.Manipulation"/></td><td><strong><xsl:value-of select="tradition/spiritmanipulation" /></strong></td>
          <td><xsl:value-of select="$lang.Drain"/></td><td><strong><xsl:value-of select="tradition/drainvalue" /></strong></td>
        </tr>
      </xsl:if>
    </table>
  </xsl:template>

  <xsl:template name="print_spells">
    <xsl:variable name="sorted_spells">
      <xsl:for-each select="spells/spell">
        <xsl:sort select="category" />
        <xsl:sort select="name" />

        <xsl:copy-of select="current()" />
      </xsl:for-each>
    </xsl:variable>

    <xsl:if test="count(msxsl:node-set($sorted_spells)/*) &gt; 0">

      <table class="stats magic">
        <tr><td colspan="7"><div class="bigheader">[<xsl:value-of select="$lang.Spells"/>]</div></td></tr>
        <tr class="smallheader"><td><xsl:value-of select="$lang.Spell"/></td><td><xsl:value-of select="$lang.Type"/></td><td><xsl:value-of select="$lang.Range"/></td><td><xsl:value-of select="$lang.Duration"/></td><td><xsl:value-of select="$lang.Damage"/></td><td><xsl:value-of select="$lang.Drain"/></td><td><xsl:value-of select="$lang.Description"/></td></tr>
        <xsl:for-each select="msxsl:node-set($sorted_spells)/spell">

          <xsl:if test="category != preceding-sibling::spell[1]/category or position()=1">
            <tr><td colspan="7"><xsl:value-of select="category" /><xsl:text> </xsl:text><xsl:value-of select="$lang.Spells"/></td></tr>
          </xsl:if>

          <tr>
            <td>
              <xsl:value-of select="name" />
              <xsl:if test="extra!=''"> (<xsl:value-of select="extra" />)</xsl:if>
              <xsl:call-template name="print_source_page" />
              <xsl:call-template name="print_notes" />
            </td>
            <td><xsl:value-of select="type" /></td>
            <td><xsl:value-of select="range" /></td>
            <td><xsl:value-of select="duration" /></td>
            <td><xsl:value-of select="damage" /></td>
            <td><xsl:value-of select="dv" /></td>
            <td><xsl:value-of select="descriptors" /></td>
          </tr>
        </xsl:for-each>
      </table>

    </xsl:if>
  </xsl:template>

  <xsl:template name="print_spirits">
    <xsl:if test="count(spirits/spirit) &gt; 0">

      <table class="stats magic">
        <tr><td colspan="2"><div class="bigheader">[<xsl:value-of select="$lang.Spirits"/>]</div></td></tr>
        <xsl:for-each select="spirits/spirit">
          <xsl:sort select="name" />
          <xsl:sort select="crittername" />

          <tr>
            <td>
              <xsl:value-of select="name" />
              <xsl:if test="crittername!=''"> (<xsl:value-of select="crittername" />)</xsl:if>
              <xsl:choose>
                <xsl:when test="fettered = 'True'"> (<xsl:value-of select="$lang.Fettered"/>)</xsl:when>
                <xsl:when test="bound"> (<xsl:value-of select="$lang.Bound"/>)</xsl:when>
                <xsl:otherwise> (<xsl:value-of select="$lang.Unbound"/>)</xsl:otherwise>
              </xsl:choose>
              <xsl:call-template name="print_source_page" />
              <xsl:call-template name="print_notes" />
              <br />
              <table>
                <tr class="smallheader"><td><xsl:value-of select="$lang.Skill"/></td><td><xsl:value-of select="$lang.Pool"/></td></tr>
                <xsl:for-each select="skills/skill">
                  <tr>
                    <td>
                      <xsl:value-of select="name" />
                      <span style="color:grey; text-transform: uppercase;"><xsl:text> </xsl:text><xsl:value-of select="attr" /></span>
                    </td>
                    <td><xsl:value-of select="pool" /></td>
                  </tr>
                </xsl:for-each>
              </table>
            </td>
            <td>
              <table style="width:100%;">
                <td><xsl:value-of select="$lang.Force"/></td><td><xsl:value-of select="force" /></td><td><xsl:value-of select="$lang.Services"/></td><td><xsl:value-of select="services" /></td>
                <tr><td><xsl:value-of select="$lang.Body"/></td><td><xsl:value-of select="spiritattributes/bod" /></td><td><xsl:value-of select="$lang.Willpower"/></td><td><xsl:value-of select="spiritattributes/wil" /></td></tr>
                <tr><td><xsl:value-of select="$lang.Agility"/></td><td><xsl:value-of select="spiritattributes/agi" /></td><td><xsl:value-of select="$lang.Logic"/></td><td><xsl:value-of select="spiritattributes/log" /></td></tr>
                <tr><td><xsl:value-of select="$lang.Reaction"/></td><td><xsl:value-of select="spiritattributes/rea" /></td><td><xsl:value-of select="$lang.Intuition"/></td><td><xsl:value-of select="spiritattributes/int" /></td></tr>
                <tr><td><xsl:value-of select="$lang.Strength"/></td><td><xsl:value-of select="spiritattributes/str" /></td><td><xsl:value-of select="$lang.Charisma"/></td><td><xsl:value-of select="spiritattributes/cha" /></td></tr>
                <tr><td><xsl:value-of select="$lang.PhysicalTrack"/></td><td><xsl:value-of select="ceiling(spiritattributes/bod div 2) + 8" /></td><td><xsl:value-of select="$lang.StunTrack"/></td><td><xsl:value-of select="ceiling(spiritattributes/wil div 2) + 8" /></td></tr>
                <tr><td><xsl:value-of select="$lang.Initiative"/></td><td><xsl:value-of select="spiritattributes/ini" /><xsl:text> + 2d6</xsl:text></td></tr>
              </table>
            </td>
          </tr>
          <tr>
            <td>
              <xsl:value-of select="$lang.Powers"/>
              <xsl:choose>
                <xsl:when test="count(powers/critterpower) &gt; 0">
                  <ul style="margin-left:5px;">
                    <xsl:for-each select="powers/critterpower">
                      <li>
                        <xsl:value-of select="name" />
                        <xsl:call-template name="print_source_page" />
                      </li>
                    </xsl:for-each>
                  </ul>
                </xsl:when>
                <xsl:otherwise>
                  <span style="margin-left:5px;">
                    <xsl:value-of select="$lang.None"/>
                  </span>
                </xsl:otherwise>
              </xsl:choose>
            </td>
            <td>
              <xsl:if test="count(optionalpowers/critterpower) &gt; 0">
                <xsl:value-of select="$lang.OptionalPowers"/>
                <ul style="margin-left:5px;">
                  <xsl:for-each select="optionalpowers/critterpower">
                    <li>
                      <xsl:value-of select="name" />
                      <xsl:if test="extra!=''">
                        (<xsl:value-of select="extra" />)
                      </xsl:if>
                      <xsl:call-template name="print_source_page" />
                    </li>
                  </xsl:for-each>
                </ul>
              </xsl:if>
              <xsl:if test="count(weaknesses/critterpower) &gt; 0">
                <xsl:value-of select="$lang.Weaknesses"/>
                <ul style="margin-left:5px;">
                  <xsl:for-each select="weaknesses/critterpower">
                    <li>
                      <xsl:value-of select="name" />
                      <xsl:if test="extra!=''">
                        (<xsl:value-of select="extra" />)
                      </xsl:if>
                      <xsl:call-template name="print_source_page" />
                    </li>
                  </xsl:for-each>
                </ul>
              </xsl:if>
            </td>
          </tr>
          <xsl:if test="position() != last()">
            <tr><td colspan="2"><hr /></td></tr>
          </xsl:if>
        </xsl:for-each>
      </table>

    </xsl:if>
  </xsl:template>

  <xsl:template name="print_adept_powers">
    <table class="stats magic">
      <tr><td><div class="bigheader">[<xsl:value-of select="$lang.AdeptPowers"/>]</div></td></tr>
      <xsl:for-each select="powers/power">
        <xsl:sort select="name" />

        <tr><td><xsl:call-template name="print_nested" /></td></tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="print_foci">
    <xsl:if test="count(gears/gear[category_english='Foci']) &gt; 0">

      <table class="stats magic">
        <tr><td><div class="bigheader">[<xsl:value-of select="$lang.Foci"/>]</div></td></tr>
        <xsl:for-each select="gears/gear[category_english='Foci']">
          <xsl:sort select="name" />
          <tr><td><xsl:call-template name="print_nested" /></td></tr>
        </xsl:for-each>
      </table>

    </xsl:if>
  </xsl:template>

  <xsl:template name="print_initiation">
    <xsl:if test="initiategrade &gt; 0">

      <table class="stats magic">
        <tr><td><div class="bigheader">[<xsl:value-of select="$lang.Initiation"/>]</div></td></tr>
        <tr><td>
          <strong><xsl:value-of select="$lang.InitiateGrade"/>: <xsl:value-of select="initiategrade" /></strong>
          <br />
          <xsl:if test="count(arts/art) &gt; 0">
            <strong><xsl:value-of select="$lang.Arts"/></strong>
            <ul>
              <xsl:for-each select="arts/art">
                <xsl:sort select="name" />

                <li>
                  <xsl:value-of select="name" />
                  <xsl:call-template name="print_source_page" />
                  <xsl:call-template name="print_notes" />
                </li>
              </xsl:for-each>
            </ul>
            <br />
          </xsl:if>
          <xsl:if test="count(metamagics/metamagic) &gt; 0">
            <strong><xsl:value-of select="$lang.Metamagics"/></strong>
            <ul>
              <xsl:for-each select="metamagics/metamagic">
                <xsl:sort select="name" />

                <li>
                  <xsl:value-of select="name" />
                  <xsl:call-template name="print_source_page" />
                  <xsl:call-template name="print_notes" />
                </li>
              </xsl:for-each>
            </ul>
          </xsl:if>
        </td></tr>
      </table>

    </xsl:if>
  </xsl:template>

  <xsl:template name="print_other_gear">
    <xsl:variable name="other_gears">
      <xsl:for-each select="gears/gear[category_english!='ID/Credsticks' and isammo!='True' and category_english!='Foci' and category_english!='Commlinks' and category_english!='Commlink' and category_english!='Cyberdecks' and category_english!='Rigger Command Consoles']">
        <xsl:sort select="location" />
        <xsl:sort select="name" />

        <xsl:copy-of select="current()"/>
      </xsl:for-each>
    </xsl:variable>

    <table class="stats gear">
      <tr><td colspan="2"><div class="bigheader">[<xsl:value-of select="$lang.Gear"/>]</div></td></tr>
      <tr>
        <td style="width:50%;">
          <xsl:call-template name="print_half_gear">
            <xsl:with-param name="gear_list" select="$other_gears" />
            <xsl:with-param name="condition" select="true()" />
          </xsl:call-template>
        </td>
        <td style="width:50%;">
          <xsl:call-template name="print_half_gear">
            <xsl:with-param name="gear_list" select="$other_gears" />
            <xsl:with-param name="condition" select="false()" />
          </xsl:call-template>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template name="print_half_gear">
    <xsl:param name="gear_list" />
    <xsl:param name="condition" />

    <xsl:variable name="gears_half_count" select="ceiling(count(msxsl:node-set($gear_list)/gear) div 2) + 1" />
    <xsl:variable name="need_location" select="count(msxsl:node-set($gear_list)/gear[location!='']) &gt; 0" />
    <xsl:variable name="need_equip_mark" select="count(msxsl:node-set($gear_list)/gear[equipped='False']) &gt; 0" />

    <ul style="margin-left:0px;">
      <xsl:for-each select="msxsl:node-set($gear_list)/gear">
        <xsl:if test="(position() &lt; $gears_half_count)=$condition">
          <xsl:if test="$need_location and (position()=1 or location!=preceding-sibling::gear[1]/location)">
            <li>
              <xsl:call-template name="print_location" />
            </li>
          </xsl:if>

          <li>
            <xsl:call-template name="print_nested">
              <xsl:with-param name="need_equip_mark" select="$need_equip_mark" />
            </xsl:call-template>
          </li>
        </xsl:if>
      </xsl:for-each>
    </ul>
  </xsl:template>

  <xsl:template name="print_ids">
    <table class="stats gear">
      <tr><td><div class="bigheader">[<xsl:value-of select="$lang.IDcredsticks"/>]</div></td></tr>
      <xsl:for-each select="gears/gear[category_english = 'ID/Credsticks']">
        <xsl:sort select="contains(name_english, 'SIN') or contains(name_english, 'License')" order="descending" />
        <xsl:sort select="name" />
        <tr><td><xsl:call-template name="print_nested" /></td></tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="print_lifestyle">
    <table class="stats gear">
      <tr><td><div class="bigheader">[<xsl:value-of select="$lang.Lifestyle"/>]</div></td></tr>
      <xsl:for-each select="lifestyles/lifestyle">
        <xsl:sort select="name" />
        <tr>
          <td>
            <strong>
              <xsl:value-of select="name" />
              <xsl:if test="baselifestyle != ''"> (<xsl:value-of select="baselifestyle" />)</xsl:if>
              <xsl:if test="lifestylename != ''"> (<xsl:value-of select="lifestylename" />)</xsl:if>
            </strong>
            <xsl:call-template name="print_source_page" />
            <xsl:call-template name="print_notes" />
            <br />
            <xsl:choose>
              <xsl:when test="purchased='True'">
                [<xsl:value-of select="$lang.Permanent"/>]
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$lang.Cost"/>: <xsl:value-of select="cost" /><xsl:value-of select="$lang.NuyenSymbol"/> (<xsl:value-of select="totalmonthlycost" /><xsl:value-of select="$lang.NuyenSymbol"/>) × <xsl:value-of select="months" /> = <xsl:value-of select="totalcost" /><xsl:value-of select="$lang.NuyenSymbol"/>;
              </xsl:otherwise>
            </xsl:choose>
            <br />
            <ul>
            <xsl:for-each select="qualities/lifestylequality">
              <li>
                <xsl:value-of select="current()" />
              </li>
            </xsl:for-each>
            </ul>
          </td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="print_description">
    <xsl:param name="ImageFormat" />
    <xsl:if test="description!=''">
      <table class="stats description">
        <tr><td><div class="bigheader">[<xsl:value-of select="$lang.Description"/>]</div></td></tr>
        <tr><td>
          <xsl:call-template name="PreserveHtml">
            <xsl:with-param name="text" select="description" />
          </xsl:call-template>
        </td></tr>
      </table>
      <br />
    </xsl:if>
    <xsl:if test="background!=''">
      <table class="stats description">
        <tr><td><div class="bigheader">[<xsl:value-of select="$lang.Background"/>]</div></td></tr>
        <tr><td>
          <xsl:call-template name="PreserveHtml">
            <xsl:with-param name="text" select="background" />
          </xsl:call-template>
        </td></tr>
      </table>
      <br />
    </xsl:if>
    <xsl:if test="concept!=''">
      <table class="stats description">
        <tr><td><div class="bigheader">[<xsl:value-of select="$lang.Concept"/>]</div></td></tr>
        <tr><td>
          <xsl:call-template name="PreserveHtml">
            <xsl:with-param name="text" select="concept" />
          </xsl:call-template>
        </td></tr>
      </table>
      <br />
    </xsl:if>

    <xsl:if test="hasothermugshots = 'True'">
      <table class="stats description">
        <tr><td>
          <div class="bigheader">[<xsl:value-of select="$lang.OtherMugshots"/>]</div>
        </td></tr>
        <tr><td>
          <table width="100%" cellspacing="0" cellpadding="0" border="0" style="empty-cells:show;">
            <tr>
              <td width="33%" style="text-align:center;">
                <table width="100%" cellspacing="0" cellpadding="0" border="0" style="empty-cells:show;">
                  <xsl:for-each select="othermugshots/mugshot[position() mod 3 = 1]">
                    <tr>
                      <td width="100%" style="text-align:center; width: 100%;">
                        <img src="data:image/{$ImageFormat};base64,{stringbase64}" class="mugshot" />
                      </td>
                    </tr>
                  </xsl:for-each>
                </table>
              </td>
              <td width="33%" style="text-align:center;">
                <table width="100%" cellspacing="0" cellpadding="0" border="0" style="empty-cells:show;">
                  <xsl:if test="count(othermugshots/mugshot[position() mod 3 = 2]) = 0">
                    <tr><td/></tr>
                  </xsl:if>
                  <xsl:for-each select="othermugshots/mugshot[position() mod 3 = 2]">
                    <tr>
                      <td width="100%" style="text-align:center; width: 100%;">
                        <img src="data:image/{$ImageFormat};base64,{stringbase64}" class="mugshot" />
                      </td>
                    </tr>
                  </xsl:for-each>
                </table>
              </td>
              <td width="33%" style="text-align:center;">
                <table width="100%" cellspacing="0" cellpadding="0" border="0" style="empty-cells:show;">
                  <xsl:if test="count(othermugshots/mugshot[position() mod 3 = 0]) = 0">
                    <tr><td/></tr>
                  </xsl:if>
                  <xsl:for-each select="othermugshots/mugshot[position() mod 3 = 0]">
                    <tr>
                      <td width="100%" style="text-align:center; width: 100%;">
                        <img src="data:image/{$ImageFormat};base64,{stringbase64}" class="mugshot" />
                      </td>
                    </tr>
                  </xsl:for-each>
                </table>
              </td>
            </tr>
          </table>
        </td></tr>
      </table>
      <br />
    </xsl:if>

    <xsl:if test="notes!='' or gamenotes!=''">
      <table class="stats description">
        <tr><td><div class="bigheader">[<xsl:value-of select="$lang.Notes"/>]</div></td></tr>
        <tr><td>
          <xsl:call-template name="PreserveHtml">
            <xsl:with-param name="text" select="notes" />
          </xsl:call-template>
        </td></tr>
        <xsl:if test="notes!='' and gamenotes!=''">
          <tr><td><hr /></td></tr>
        </xsl:if>
        <tr><td>
          <xsl:call-template name="PreserveHtml">
            <xsl:with-param name="text" select="gamenotes" />
          </xsl:call-template>
        </td></tr>
      </table>
      <br />
    </xsl:if>

    <xsl:if test="count(calendar/week) &gt; 0">
      <table class="stats description">
        <tr><td colspan="2"><div class="bigheader">[<xsl:value-of select="$lang.Calendar"/>]</div></td></tr>
        <tr class="smallheader"><td><xsl:value-of select="$lang.Date"/></td><td><xsl:value-of select="$lang.Notes"/></td></tr>
        <xsl:for-each select="calendar/week">
          <tr>
            <td style="white-space:pre;">
                <xsl:value-of select="year" /><xsl:text>, </xsl:text>
                <xsl:value-of select="$lang.Month"/><xsl:text> </xsl:text><xsl:value-of select="month" /><xsl:text>, </xsl:text>
                <xsl:value-of select="$lang.Week"/><xsl:text> </xsl:text><xsl:value-of select="week" />
            </td>
            <td style="width:100%;">
              <xsl:call-template name="PreserveLineBreaks">
                <xsl:with-param name="text" select="notes" />
              </xsl:call-template>
            </td>
          </tr>
        </xsl:for-each>
      </table>
      <br />
    </xsl:if>
  </xsl:template>

  <xsl:template name="print_nested">
    <xsl:param name="need_equip_mark" select="false()" />

    <xsl:variable name="is_long_extra" select="name_english='Custom Fit (Stack)'" />

    <span>
      <xsl:if test="included and included='True'">
        <xsl:attribute name="style">color:grey;</xsl:attribute>
      </xsl:if>
      <xsl:if test="$need_equip_mark">
        <xsl:choose>
          <xsl:when test="equipped='True'">&#x26AB; </xsl:when>
          <xsl:otherwise>&#x26AA; </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
      <xsl:value-of select="name" />
      <xsl:if test="extra!='' and not($is_long_extra)">
        (<xsl:value-of select="extra" />)
      </xsl:if>
      <xsl:if test="rating &gt; 0">
        <xsl:text> [R</xsl:text><xsl:value-of select="rating" />]
      </xsl:if>
      <xsl:if test="qty and qty &gt; 1">
        <xsl:text> ×</xsl:text><xsl:value-of select="qty" />
      </xsl:if>
      <xsl:call-template name="print_source_page" />
      <xsl:if test="extra!='' and $is_long_extra">
        <br />
        (<xsl:value-of select="extra" />)
      </xsl:if>
      <xsl:call-template name="print_notes" />
    </span>

    <xsl:if test="count(children/gear) &gt; 0">
      <xsl:variable name="child_need_equip_mark" select="count(children/gear[equipped='False']) &gt; 0" />
      <ul>
        <xsl:for-each select="children/gear">
          <xsl:sort select="name" />

          <li>
            <xsl:call-template name="print_nested">
              <xsl:with-param name="need_equip_mark" select="$child_need_equip_mark" />
            </xsl:call-template>
          </li>
        </xsl:for-each>
      </ul>
    </xsl:if>
  </xsl:template>

  <xsl:template name="print_source_page">
    <span style="color:grey;">
      <xsl:text> </xsl:text><xsl:value-of select="source" /><xsl:text> </xsl:text><xsl:value-of select="page" />
    </span>
  </xsl:template>

  <xsl:template name="print_notes">
    <xsl:param name="linebreak" select="true()" />

    <xsl:if test="notes!=''">
      <xsl:if test="$linebreak">
        <br />
      </xsl:if>
      <span style="color:darkgreen;">
        <sup><i>
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template></i></sup>
      </span>
    </xsl:if>
  </xsl:template>

  <xsl:template name="print_location">
    <strong>
      <xsl:choose>
        <xsl:when test="location=''"><xsl:value-of select="$lang.SelectedGear"/></xsl:when>
        <xsl:otherwise><xsl:value-of select="location" /></xsl:otherwise>
      </xsl:choose>
    </strong>
  </xsl:template>

  <xsl:template name="make_grey_lines">
    <xsl:if test="(position() mod 2) = 0">
      <xsl:attribute name="style">background-color:lightgrey;</xsl:attribute>
    </xsl:if>
  </xsl:template>

  <xsl:template name="page_breaker">
    <tr class="page_breaker_off" onClick="toggle_page_breaker(this);">
      <td colspan="3">
        <xsl:value-of select="$lang.PageBreak"/>: <span class="page_breaker_status">OFF</span>
      </td>
    </tr>
  </xsl:template>

</xsl:stylesheet>
