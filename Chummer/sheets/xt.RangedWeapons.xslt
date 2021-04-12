<?xml version="1.0" encoding="UTF-8" ?>
<!-- Ranged weapons -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:template name="RangedWeapons">
      <xsl:param name="weapon"/>
    <tr>
      <xsl:if test="position() mod 2 != 1">
        <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
      </xsl:if>
      <td style="text-align: left">
        <xsl:value-of select="name"/>
        <xsl:if test="weaponname != ''"> ("<xsl:value-of select="weaponname"/>") </xsl:if>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="dicepool"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="accuracy"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="damage"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="ap"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="mode"/>
      </td>
      <td style="text-align: center">
        <xsl:value-of select="rc"/>
      </td>
      <xsl:choose>
        <xsl:when test="skill= 'Throwing Weapons'">
          <td colspan="2" style="text-align: center">
            <xsl:value-of select="$lang.Qty"/>:
            <xsl:call-template name="fnx-fmt-nmbr">
              <xsl:with-param name="nmbr" select="availableammo"/>
            </xsl:call-template>
          </td>
        </xsl:when>
      <xsl:otherwise>
      <td style="text-align: center">
        <xsl:choose>
          <xsl:when test="ammo = '0' or ammo = ''">
            <xsl:text>-</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="ammo"/>
          </xsl:otherwise>
        </xsl:choose>
      </td>
      <td style="text-align: center">
        <xsl:choose>
          <xsl:when test="ammo = '0' or ammo = ''">
            <xsl:text>&#160;</xsl:text>
          </xsl:when>
          <xsl:when test="clips= ''">
            <xsl:text>&#160;</xsl:text>
          </xsl:when>
          <xsl:when test="clips/clip/count = ''">
            <xsl:text> [0]</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            [<xsl:value-of select="clips/clip/count"/>]
          </xsl:otherwise>
        </xsl:choose>
      </td>
      </xsl:otherwise>
      </xsl:choose>
      <td style="text-align: center">
        <xsl:value-of select="source"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="page"/>
      </td>
    </tr>

    <xsl:if test="ranges/name != '' or alternateranges/name != ''">
      <tr style="text-align: center">
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td />
        <td colspan="6">
          <table class="tablestyle">
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
        </td>
        <td colspan="3"/>
      </tr>
    </xsl:if>

    <xsl:if test="accessories/accessory or mods/weaponmod">
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td colspan="100%" class="indent">
          <xsl:for-each select="accessories/accessory">
            <xsl:sort select="name"/>
            <xsl:value-of select="name"/>
            <xsl:if test="last() &gt; 1"><xsl:text>; </xsl:text></xsl:if>
          </xsl:for-each>
          <xsl:for-each select="mods/weaponmod">
            <xsl:sort select="name"/>
            <xsl:value-of select="name"/>
            <xsl:if test="rating > 0">
              <xsl:text> </xsl:text>
              <xsl:value-of select="$lang.Rating"/>
              <xsl:text> </xsl:text>
              <xsl:value-of select="rating"/>
            </xsl:if>
            <xsl:if test="last() &gt; 1"><xsl:text>; </xsl:text></xsl:if>
          </xsl:for-each>
        </td>
      </tr>
    </xsl:if>

    <xsl:if test="underbarrel/weapon">
      <xsl:for-each select="underbarrel/weapon">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td class="indent">
            <xsl:value-of select="$lang.Under"/>:
            <xsl:value-of select="name"/>
            <xsl:if test="weaponname != ''"> ("<xsl:value-of select="weaponname"/>") </xsl:if>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="dicepool"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="accuracy"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="damage"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="ap"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="mode"/>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="rc"/>
          </td>
          <td style="text-align: center">
            <xsl:choose>
              <xsl:when test="ammo = '0' or ammo = ''">
                <xsl:text>-</xsl:text>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="ammo"/>
              </xsl:otherwise>
            </xsl:choose>
          </td>
          <td style="text-align: center">
            <xsl:choose>
              <xsl:when test="ammo = '0' or ammo = ''">
                <xsl:text>&#160;</xsl:text>
              </xsl:when>
              <xsl:when test="clips = ''">
                <xsl:text>&#160;</xsl:text>
              </xsl:when>
              <xsl:when test="clips/clip/count = ''">
                <xsl:text> [0]</xsl:text>
              </xsl:when>
              <xsl:otherwise>
                [<xsl:value-of select="clips/clip/count"/>]
              </xsl:otherwise>
            </xsl:choose>
          </td>
          <td style="text-align: center">
            <xsl:value-of select="source"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="page"/>
          </td>
        </tr>
        <xsl:if test="ranges/name != '' or alternateranges/name != ''">
          <tr style="text-align: center">
            <xsl:if test="position() mod 2 != 1">
              <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
            </xsl:if>
            <td />
            <td colspan="6">
              <table class="tablestyle">
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
            </td>
            <td colspan="3"/>
          </tr>
        </xsl:if>
        <xsl:if test="accessories/accessory">
          <tr>
            <xsl:if test="position() mod 2 != 1">
              <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
            </xsl:if>
            <td colspan="100%" class="indent">
              <xsl:for-each select="accessories/accessory">
                <xsl:sort select="name"/>
                <xsl:value-of select="name"/>
                <xsl:if test="last() &gt; 1">; </xsl:if>
              </xsl:for-each>
              <xsl:for-each select="mods/weaponmod">
                <xsl:sort select="name"/>
                <xsl:value-of select="name"/>
                <xsl:if test="rating > 0">&#160;<xsl:value-of select="$lang.Rating"/>&#160;<xsl:value-of select="rating"/></xsl:if>
                <xsl:if test="last() &gt; 1">; </xsl:if>
              </xsl:for-each>
            </td>
          </tr>
        </xsl:if>
      </xsl:for-each>
    </xsl:if>

    <xsl:if test="notes != '' and $ProduceNotes">
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td colspan="100%" class="notesrow2">
          <xsl:call-template name="PreserveLineBreaks">
            <xsl:with-param name="text" select="notes"/>
          </xsl:call-template>
        </td>
      </tr>
    </xsl:if>
    <xsl:call-template name="Xline">
      <xsl:with-param name="cntl" select="last()-position()"/>
    </xsl:call-template>
  </xsl:template>

</xsl:stylesheet>
