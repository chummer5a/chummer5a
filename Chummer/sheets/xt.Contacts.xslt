<?xml version="1.0" encoding="utf-8" ?>
<!-- Character contacts -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:template name="Contacts">
    <style type="text/css">
      h5 {
        position: relative;
        text-align: center;
      }
      h5 span {
        background: #fff;
        padding: 0 0.65em;
        position: relative;
        z-index: 1;
      }
      h5:before {
        background: linear-gradient(to right, lightgrey, black, lightgrey);
        content: "";
        display: block;
        height: 0.1em;
        position: absolute;
        top: 50%;
        width: 100%;
      }
    </style>
    <style media="print">
      h5 {
        text-align: center;
        text-decoration: underline;
      }
    </style>

    <xsl:variable name="cntcntct">
      <xsl:value-of select="count(contacts/contact[type='Contact'])"/>
    </xsl:variable>

    <xsl:variable name="sortedcontacts">
      <xsl:for-each select="contacts/contact">
        <xsl:sort select="type"/>
        <xsl:sort select="name"/>
        <xsl:copy-of select="current()"/>
      </xsl:for-each>
    </xsl:variable>

    <xsl:for-each select="msxsl:node-set($sortedcontacts)/contact">
      <xsl:choose>
        <xsl:when test="type != preceding-sibling::contact[1]/type">
          <xsl:call-template name="contactcategory"/>
        </xsl:when>
        <xsl:when test="position() = 1">
          <xsl:if test="$cntcntct != last()">
            <xsl:call-template name="contactcategory"/>
          </xsl:if>
        </xsl:when>
      </xsl:choose>

      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td style="text-align: left"><xsl:value-of select="name"/></td>
        <td style="text-align: center"><xsl:value-of select="location"/></td>
        <td style="text-align: center"><xsl:value-of select="role"/></td>
        <td style="text-align: center"><xsl:value-of select="connection"/></td>
        <td style="text-align: center"><xsl:value-of select="loyalty"/></td>
      </tr>

      <xsl:if test="$ProduceNotes">
      <xsl:if test="metatype != '' or sex != '' or age != '' or preferredpayment != '' or hobbiesvice != '' or personallife != '' or contacttype != ''">
      <tr>
        <xsl:if test="position() mod 2 != 1">
          <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
        </xsl:if>
        <td/>
        <td colspan="4">
          <table>
            <xsl:if test="metatype != ''">
              <tr>
                <td style="text-align: right; padding-right: 1em; width: 25%;">
                  <xsl:value-of select="$lang.Metatype"/>: 
                </td>
                <td style="text-align: left;">
                  <xsl:value-of select="metatype"/>
                </td>
              </tr>
            </xsl:if>
            <xsl:if test="sex != ''">
            <tr>
              <td style="text-align: right; padding-right: 1em; width: 25%;">
                <xsl:value-of select="$lang.Sex"/>: 
              </td>
              <td style="text-align: left;">
                <xsl:value-of select="sex"/>
              </td>
            </tr>
            </xsl:if>
            <xsl:if test="age != ''">
            <tr>
              <td style="text-align: right; padding-right: 1em; width: 25%;">
                <xsl:value-of select="$lang.Age"/>: 
              </td>
              <td style="text-align: left;">
                <xsl:value-of select="age"/>
              </td>
            </tr>
            </xsl:if>
            <xsl:if test="preferredpayment != ''">
            <tr>
              <td style="text-align: right; padding-right: 1em; width: 25%;">
                <xsl:value-of select="$lang.PreferredPayment"/>: 
              </td>
              <td style="text-align: left;">
                <xsl:value-of select="preferredpayment"/>
              </td>
            </tr>
            </xsl:if>
            <xsl:if test="hobbiesvice != ''">
            <tr>
              <td style="text-align: right; padding-right: 1em; width: 25%;">
                <xsl:value-of select="$lang.HobbiesVice"/>: 
              </td>
              <td style="text-align: left;">
                <xsl:value-of select="hobbiesvice"/>
              </td>
            </tr>
            </xsl:if>
            <xsl:if test="personallife != ''">
            <tr>
              <td style="text-align: right; padding-right: 1em; width: 25%;">
                <xsl:value-of select="$lang.PersonalLife"/>: 
              </td>
              <td style="text-align: left;">
                <xsl:value-of select="personallife"/>
              </td>
            </tr>
            </xsl:if>
            <xsl:if test="contacttype != ''">
            <tr>
              <td style="text-align: right; padding-right: 1em; width: 25%;">
                <xsl:value-of select="$lang.Type"/>: 
              </td>
              <td style="text-align: left;">
                <xsl:value-of select="contacttype"/>
              </td>
            </tr>
            </xsl:if>
          </table>
        </td>
      </tr>
      </xsl:if>

      <xsl:if test="notes != ''">
        <tr>
          <xsl:if test="position() mod 2 != 1">
            <xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
          </xsl:if>
          <td colspan="100%" style="padding: 0 2%; text-align: justify;">
            <xsl:call-template name="PreserveLineBreaks">
              <xsl:with-param name="text" select="notes"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
      </xsl:if>

      <xsl:call-template name="Xline">
        <xsl:with-param name="cntl" select="last()-position()"/>
        <xsl:with-param name="nte" select="notes != '' and $ProduceNotes"/>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="contactcategory">
    <xsl:variable name="type">
      <xsl:choose>
        <xsl:when test="type='Contact'"><xsl:value-of select="$lang.Contacts"/></xsl:when>
        <xsl:when test="type='Enemy'"><xsl:value-of select="$lang.Enemies"/></xsl:when>
<!-- can't get this code to work - are the flags sent?
        <xsl:when test="blackmail">Blackmailed</xsl:when>
        <xsl:when test="family">Family</xsl:when>
        <xsl:when test="group">Group</xsl:when>
  Note: if group is true connection is supplied as group(connection) -->
        <xsl:when test="type='Pet'"><xsl:value-of select="$lang.Pets"/></xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$lang.Type"/>: <xsl:value-of select="type"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="Xline"/>
    <tr><td colspan="100%">
      <h5><span><xsl:value-of select="$type"/></span></h5>
    </td></tr>
  </xsl:template>

</xsl:stylesheet>
