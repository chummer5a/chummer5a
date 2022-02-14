<?xml version="1.0" encoding="UTF-8" ?>
<!-- Spirits and Sprites List -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:include href="xs.Chummer5CSS.xslt" />
  <xsl:include href="xs.fnx.xslt" />
  <xsl:include href="xs.TitleName.xslt" />

  <xsl:include href="xt.Nothing2Show.xslt" />
  <xsl:include href="xt.PreserveLineBreaks.xslt" />
  <xsl:include href="xt.RowSummary.xslt" />
  <xsl:include href="xt.RuleLine.xslt" />

  <xsl:template match="/characters/character">
    <xsl:variable name="TitleName">
      <xsl:call-template name="TitleName">
        <xsl:with-param name="name" select="name" />
        <xsl:with-param name="alias" select="alias" />
      </xsl:call-template>
    </xsl:variable>
    <title><xsl:value-of select="$TitleName" /></title>

    <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
      <head>
        <meta http-equiv="X-UA-Compatible" content="IE=edge" />
        <meta charset="UTF-8" />
        <xsl:call-template name="Chummer5CSS" />
      </head>

      <body>
        <xsl:choose>
          <xsl:when test="magenabled='True' and count(spirits/spirit) &gt; 0">
            <xsl:for-each select="spirits/spirit">
              <xsl:sort select="name" />
              <xsl:call-template name="spiritdetails">
                <xsl:with-param name="SpiritNumber">SpiritBlock<xsl:value-of select="position()" /></xsl:with-param>
              </xsl:call-template>
            </xsl:for-each>
          </xsl:when>
          <xsl:when test="resenabled='True' and count(spirits/spirit) &gt; 0">
            <xsl:for-each select="spirits/spirit">
              <xsl:sort select="name" />
              <xsl:call-template name="spritedetails">
                <xsl:with-param name="SpriteNumber">SpriteBlock<xsl:value-of select="position()" /></xsl:with-param>
              </xsl:call-template>
            </xsl:for-each>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="nothing2show">
              <xsl:with-param name="namethesheet" select="$lang.Nothing2Show4SpiritsSprites" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </body>
    </html>
  </xsl:template>

  <xsl:template name="spiritdetails">
      <xsl:param name="SpiritNumber" />
    <div class="block" id="{SpiritNumber}">
      <table class="tablestyle">
        <tr>
          <td width="50%" style="text-align: left">
            <xsl:value-of select="name" />
            <xsl:if test="crittername != ''">: <xsl:value-of select="crittername" /></xsl:if>
          </td>
          <td width="12%" style="text-align: left">
            <xsl:value-of select="$lang.Force" />:
            <xsl:value-of select="force" />
          </td>
          <td width="14%">
            <xsl:value-of select="$lang.Services" />:
            <xsl:value-of select="services" />
          </td>
          <td width="14%" style="text-align: center">
            <xsl:choose>
              <xsl:when test="fettered = 'True'">
                <xsl:value-of select="$lang.Fettered" />
              </xsl:when>
              <xsl:when test="bound = 'True'">
                <xsl:value-of select="$lang.Bound" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$lang.Unbound" />
              </xsl:otherwise>
            </xsl:choose>
          </td>
          <td width="10%" style="text-align: center">
            <xsl:value-of select="source" />
            <xsl:text> </xsl:text>
            <xsl:value-of select="page" />
          </td>
        </tr>
        <xsl:call-template name="Xline">
          <xsl:with-param name="height" select="'N'" />
        </xsl:call-template>

        <xsl:if test="notes != ''">
          <tr>
            <td colspan="100%" style="text-align: justify;">
              <xsl:call-template name="PreserveLineBreaks">
                <xsl:with-param name="text" select="notes" />
              </xsl:call-template>
            </td>
          </tr>
          <xsl:call-template name="Xline">
            <xsl:with-param name="height" select="'N'" />
          </xsl:call-template>
        </xsl:if>

        <tr>
          <td colspan="1">
            <table width="100%">
              <tr>
                <td width="75%" style="text-align: left">
                  <xsl:value-of select="$lang.Skill" />
                </td>
                <td width="20%" style="text-align: right">
                  <xsl:value-of select="$lang.Pool" />
                </td>
                <td width="5%" />
              </tr>
              <xsl:for-each select="skills/skill">
                <tr>
                  <td style="text-align: left">
                    <xsl:text>&#160;</xsl:text>
                    <xsl:value-of select="name" />
                  </td>
                  <td style="text-align: right">
                    <xsl:call-template name="fnx-pad-l">
                      <xsl:with-param name="string" select="pool" />
                      <xsl:with-param name="length" select="2" />
                    </xsl:call-template>
                    <xsl:text>&#160;</xsl:text>
                  </td>
                  <td />
                </tr>
              </xsl:for-each>
            </table>
          </td>
          <td colspan="4">
            <table width="100%">
              <tr>
                <td width="35%" style="text-align: left">
                  <xsl:value-of select="$lang.Attributes" />
                </td>
                <td width="10%" style="text-align: right">
                  <xsl:value-of select="$lang.Pool" />
                </td>
                <td width="5%" />
                <td width="35%" style="text-align: left">
                  <xsl:value-of select="$lang.Attributes" />
                </td>
                <td width="10%" style="text-align: right">
                  <xsl:value-of select="$lang.Pool" />
                </td>
                <td width="5%" />
              </tr>
              <tr>
                <td style="text-align: left">
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="$lang.Body" />
                </td>
                <td style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="spiritattributes/bod" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                  <xsl:text>&#160;</xsl:text>
                </td>
                <td />
                <td style="text-align: left">
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="$lang.Willpower" />
                </td>
                <td style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="spiritattributes/wil" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                  <xsl:text>&#160;</xsl:text>
                </td>
                <td />
              </tr>
              <tr>
                <td style="text-align: left">
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="$lang.Agility" />
                </td>
                <td style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="spiritattributes/agi" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                  <xsl:text>&#160;</xsl:text>
                </td>
                <td />
                <td style="text-align: left">
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="$lang.Logic" />
                </td>
                <td style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="spiritattributes/log" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                  <xsl:text>&#160;</xsl:text>
                </td>
                <td />
              </tr>
              <tr>
                <td style="text-align: left">
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="$lang.Reaction" />
                </td>
                <td style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="spiritattributes/rea" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                  <xsl:text>&#160;</xsl:text>
                </td>
                <td />
                <td style="text-align: left">
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="$lang.Intuition" />
                </td>
                <td style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="spiritattributes/int" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                  <xsl:text>&#160;</xsl:text>
                </td>
                <td />
              </tr>
              <tr>
                <td style="text-align: left">
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="$lang.Strength" />
                </td>
                <td style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="spiritattributes/str" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                  <xsl:text>&#160;</xsl:text>
                </td>
                <td />
                <td style="text-align: left">
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="$lang.Charisma" />
                </td>
                <td style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="spiritattributes/cha" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                  <xsl:text>&#160;</xsl:text>
                </td>
                <td />
              </tr>
            </table>
            <table width="100%">
              <tr>
                <td width="75%" style="text-align: left">
                  <xsl:value-of select="$lang.Initiative" />
                </td>
                <td width="20%" style="text-align: right">
                  <xsl:value-of select="spiritattributes/ini" /><xsl:text>+2d6</xsl:text>
                </td>
                <td width="5%" />
              </tr>
              <tr>
                <td width="75%" style="text-align: left">
                  <xsl:value-of select="$lang.PhysicalTrack" />
                </td>
                <td width="20%" style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="ceiling(spiritattributes/bod div 2) + 8" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                </td>
                <td width="5%" />
              </tr>
              <tr>
                <td width="75%" style="text-align: left">
                  <xsl:value-of select="$lang.StunTrack" />
                </td>
                <td width="20%" style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="ceiling(spiritattributes/wil div 2) + 8" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                </td>
                <td width="5%" />
              </tr>
            </table>
          </td>
        </tr>
        <xsl:call-template name="Xline" />

        <tr>
          <td colspan="1">
            <table width="100%">
              <tr>
                <td width="75%" style="text-align: left">
                  <xsl:value-of select="$lang.Powers" />
                </td>
                <td width="20%" />
                <td width="5%" />
              </tr>
              <xsl:for-each select="powers/critterpower">
                <tr>
                  <td style="text-align: left">
                    <xsl:text>&#160;</xsl:text>
                    <xsl:value-of select="name" />
                    <xsl:if test="extra!=''"> (<xsl:value-of select="extra" />)</xsl:if>
                  </td>
                  <td style="text-align: center">
                    <xsl:value-of select="source" />
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="page" />
                    <xsl:text>&#160;</xsl:text>
                  </td>
                  <td />
                </tr>
              </xsl:for-each>
            </table>
          </td>
          <td colspan="4">
            <table width="100%">
              <tr>
                <td width="75%" style="text-align: left">
                  <xsl:value-of select="$lang.OptionalPowers" />
                </td>
                <td width="20%" />
                <td width="5%" />
              </tr>
              <xsl:for-each select="optionalpowers/critterpower">
                <tr>
                  <td style="text-align: left">
                    <xsl:text>&#160;</xsl:text>
                    <xsl:value-of select="name" />
                    <xsl:if test="extra!=''"> (<xsl:value-of select="extra" />)</xsl:if>
                  </td>
                  <td style="text-align: center">
                    <xsl:value-of select="source" />
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="page" />
                    <xsl:text>&#160;</xsl:text>
                  </td>
                  <td />
                </tr>
              </xsl:for-each>
            </table>
          </td>

          <xsl:if test="count(weaknesses/critterpower) &gt; 0">
            <tr>
              <td colspan="1">
                <table width="100%">
                  <xsl:call-template name="Xline" />
                  <tr>
                    <td width="75%" style="text-align: left">
                      <xsl:value-of select="$lang.Weaknesses" />
                    </td>
                    <td width="20%" />
                    <td width="5%" />
                  </tr>
                  <xsl:for-each select="weaknesses/critterpower">
                    <tr>
                      <td style="text-align: left">
                        <xsl:text>&#160;</xsl:text>
                        <xsl:value-of select="name" />
                        <xsl:if test="extra!=''"> (<xsl:value-of select="extra" />)</xsl:if>
                      </td>
                      <td style="text-align: center">
                        <xsl:value-of select="source" />
                        <xsl:text> </xsl:text>
                        <xsl:value-of select="page" />
                        <xsl:text>&#160;</xsl:text>
                      </td>
                      <td />
                    </tr>
                  </xsl:for-each>
                </table>
              </td>
              <td colspan="4" />
            </tr>
          </xsl:if>
        </tr>
      </table>
    </div>
    <xsl:call-template name="RowSummary">
      <xsl:with-param name="text" select="$lang.Spirit" />
      <xsl:with-param name="name" select="$SpiritNumber" />
    </xsl:call-template>
    <table><tr><td /></tr></table>
  </xsl:template>

  <xsl:template name="spritedetails">
      <xsl:param name="SpriteNumber" />
    <div class="block" id="{SpriteNumber}">
      <table class="tablestyle">
        <tr>
          <td width="50%" style="text-align: left">
            <xsl:value-of select="name" />
            <xsl:if test="crittername != ''">: <xsl:value-of select="crittername" /></xsl:if>
          </td>
          <td width="12%" style="text-align: left">
            <xsl:value-of select="$lang.Level" />:
            <xsl:value-of select="force" />
          </td>
          <td width="13%">
            <xsl:value-of select="$lang.Tasks" />:
            <xsl:value-of select="services" />
          </td>
          <td width="15%" style="text-align: center">
            <xsl:choose>
              <xsl:when test="bound = 'True'">
                <xsl:value-of select="$lang.Registered" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$lang.Unregistered" />
              </xsl:otherwise>
            </xsl:choose>
          </td>
          <td width="10%" style="text-align: center">
            <xsl:value-of select="source" />
            <xsl:text> </xsl:text>
            <xsl:value-of select="page" />
          </td>
        </tr>
        <xsl:call-template name="Xline">
          <xsl:with-param name="height" select="'N'" />
        </xsl:call-template>

        <xsl:if test="notes != ''">
          <tr>
            <td colspan="100%" style="text-align: justify;">
              <xsl:call-template name="PreserveLineBreaks">
                <xsl:with-param name="text" select="notes" />
              </xsl:call-template>
            </td>
          </tr>
          <xsl:call-template name="Xline">
            <xsl:with-param name="height" select="'N'" />
          </xsl:call-template>
        </xsl:if>

        <tr>
          <td colspan="1">
            <table width="100%">
              <tr>
                <td width="75%" style="text-align: left">
                  <xsl:value-of select="$lang.Skill" />
                </td>
                <td width="20%" style="text-align: right">
                  <xsl:value-of select="$lang.Pool" />
                </td>
                <td width="5%" />
              </tr>
              <xsl:for-each select="skills/skill">
                <tr>
                  <td width="75%" style="text-align: left">
                    <xsl:text>&#160;</xsl:text>
                    <xsl:value-of select="name" />
                  </td>
                  <td width="20%" style="text-align: right">
                    <xsl:call-template name="fnx-pad-l">
                      <xsl:with-param name="string" select="pool" />
                      <xsl:with-param name="length" select="2" />
                    </xsl:call-template>
                    <xsl:text>&#160;</xsl:text>
                  </td>
                  <td width="5%" />
                </tr>
              </xsl:for-each>
            </table>
            <table width="100%">
              <xsl:call-template name="Xline" />
              <tr>
                <td width="75%" style="text-align: left">
                  <xsl:value-of select="$lang.Powers" />
                </td>
                <td width="20%" />
                <td width="5%" />
              </tr>
              <xsl:for-each select="powers/critterpower">
                <tr>
                  <td style="text-align: left">
                    <xsl:text>&#160;</xsl:text>
                    <xsl:value-of select="name" />
                    <xsl:if test="extra!=''"> (<xsl:value-of select="extra" />)</xsl:if>
                  </td>
                  <td style="text-align: center">
                    <xsl:value-of select="source" />
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="page" />
                    <xsl:text>&#160;</xsl:text>
                  </td>
                  <td />
                </tr>
              </xsl:for-each>
            </table>
          </td>
          <td colspan="4">
            <table width="100%">
              <tr>
                <td width="75%" style="text-align: left">
                  <xsl:value-of select="$lang.Attributes" />
                </td>
                <td width="20%" style="text-align: right">
                  <xsl:value-of select="$lang.Pool" />
                </td>
                <td width="5%" />
              </tr>
              <tr>
                <td style="text-align: left">
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="$lang.Attack" />
                </td>
                <td style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="spiritattributes/cha" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                  <xsl:text>&#160;</xsl:text>
                </td>
                <td />
              </tr>
              <tr>
                <td style="text-align: left">
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="$lang.DataProcessing" />
                </td>
                <td style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="spiritattributes/log" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                  <xsl:text>&#160;</xsl:text>
                </td>
                <td />
              </tr>
              <tr>
                <td style="text-align: left">
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="$lang.Firewall" />
                </td>
                <td style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="spiritattributes/wil" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                  <xsl:text>&#160;</xsl:text>
                </td>
                <td />
              </tr>
              <tr>
                <td style="text-align: left">
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="$lang.Sleaze" />
                </td>
                <td style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="spiritattributes/int" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                  <xsl:text>&#160;</xsl:text>
                </td>
                <td />
              </tr>
            </table>
            <table width="100%">
              <tr>
                <td width="75%" style="text-align: left">
                  <xsl:value-of select="$lang.Initiative" />
                </td>
                <td width="20%" style="text-align: right">
                  <xsl:value-of select="spiritattributes/ini" /><xsl:text>+4d6</xsl:text>
                </td>
                <td width="5%" />
              </tr>
              <tr>
                <td width="75%" style="text-align: left">
                  <xsl:value-of select="$lang.MatrixTrack" />
                </td>
                <td width="20%" style="text-align: right">
                  <xsl:call-template name="fnx-pad-l">
                    <xsl:with-param name="string" select="8 + ceiling(force div 2)" />
                    <xsl:with-param name="length" select="2" />
                  </xsl:call-template>
                </td>
                <td width="5%" />
              </tr>
            </table>
          </td>
        </tr>
      </table>
    </div>
    <xsl:call-template name="RowSummary">
      <xsl:with-param name="text" select="$lang.Sprite" />
      <xsl:with-param name="name" select="$SpriteNumber" />
    </xsl:call-template>
    <table><tr><td /></tr></table>
  </xsl:template>
</xsl:stylesheet>
