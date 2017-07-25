<?xml version="1.0" encoding="UTF-8" ?>
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
        padding: 0 5px;
        position: relative;
        z-index: 1;
      }
      h5:before {
        background: linear-gradient(to right, lightgrey, black, lightgrey);
        content: "";
        display: block;
        height: 1px;
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

      <xsl:if test="notes != '' and $ProduceNotes">
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