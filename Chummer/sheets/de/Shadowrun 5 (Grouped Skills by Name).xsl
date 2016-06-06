<?xml version="1.0" encoding="UTF-8" ?>
<!-- Character sheet based on the Shadowrun 5th Edition Character Sheet -->
<!-- Created by Keith Rudolph, krudolph@gmail.com -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
	<xsl:include href="Shadowrun 5 Base.xslt"/>
	<xsl:template name="skills1">
  	<xsl:variable name="skillcut" select="round(count(skills/skill[knowledge = 'False' and (rating &gt; 0 or total &gt; 0)]) div 2)"/>
    <xsl:variable name="sortedskills">
      <xsl:for-each select="skills/skill[knowledge = 'False' and (rating &gt; 0 or total &gt; 0)]">
			  <xsl:sort select="skillcategory" />
			  <xsl:sort select="name" order="ascending" />
        <xsl:if test="position() &lt;= $skillcut">
          <xsl:copy-of select="current()"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
		<xsl:for-each select="msxsl:node-set($sortedskills)/skill">
                <xsl:choose>
									<xsl:when test="position() = 1">
										<tr><td colspan="6" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="skillcategory" /></strong></td></tr>
									</xsl:when>
                  <xsl:otherwise>
                    <xsl:choose>
                      <xsl:when test="skillcategory != preceding-sibling::skill[1]/skillcategory">
                        <tr><td colspan="6" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="skillcategory" /></strong></td></tr>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:otherwise>
                </xsl:choose>
											<tr>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
												<td width="50%" valign="top">
													<xsl:value-of select="name" />
													<xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="total" />
													<xsl:if test="spec != '' and exotic = 'False'"> (<xsl:value-of select="total + 2" />)</xsl:if>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<strong><xsl:value-of select="rating" /></strong>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="attributemod" />
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="ratingmod" />
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="poolmod" />
												</td>
											</tr>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="skills2">
  	<xsl:variable name="skillcut" select="round(count(skills/skill[knowledge = 'False' and (rating &gt; 0 or total &gt; 0)]) div 2)"/>
    <xsl:variable name="sortedskills">
      <xsl:for-each select="skills/skill[knowledge = 'False' and (rating &gt; 0 or total &gt; 0)]">
			  <xsl:sort select="skillcategory" />
			  <xsl:sort select="name" order="ascending" />
        <xsl:if test="position() &gt; $skillcut">
          <xsl:copy-of select="current()"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
		<xsl:for-each select="msxsl:node-set($sortedskills)/skill">
                <xsl:choose>
									<xsl:when test="position() = 1">
										<tr><td colspan="6" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="skillcategory" /></strong></td></tr>
									</xsl:when>
                  <xsl:otherwise>
                    <xsl:choose>
                      <xsl:when test="skillcategory != preceding-sibling::skill[1]/skillcategory">
                        <tr><td colspan="6" style="border-bottom:solid black 1px;"><strong><xsl:value-of select="skillcategory" /></strong></td></tr>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:otherwise>
                </xsl:choose>
											<tr>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
												<td width="50%" valign="top">
													<xsl:value-of select="name" />
													<xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="total" />
													<xsl:if test="spec != '' and exotic = 'False'"> (<xsl:value-of select="total + 2" />)</xsl:if>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<strong><xsl:value-of select="rating" /></strong>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="attributemod" />
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="ratingmod" />
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="poolmod" />
												</td>
											</tr>
		</xsl:for-each>
	</xsl:template>

	
	<xsl:template name="skills3">
		<xsl:for-each select="skills/skill[knowledge = 'True' and islanguage = 'True']">
			<xsl:sort select="name" />
											<tr>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
												<td width="50%" valign="top">
													<xsl:if test="islanguage = 'True'">Sprache: </xsl:if>
													<xsl:value-of select="name" />
													<xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															M
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="total" />
															<xsl:if test="spec != ''"> (<xsl:value-of select="total + 2" />)</xsl:if>
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															<strong>M</strong>
														</xsl:when>
														<xsl:otherwise>
															<strong><xsl:value-of select="rating" /></strong>
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															M
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="attributemod" />
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															M
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="ratingmod" />
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															M
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="poolmod" />
														</xsl:otherwise>
													</xsl:choose>
												</td>
											</tr>
		</xsl:for-each>
		<xsl:for-each select="skills/skill[knowledge = 'True' and islanguage != 'True']">
			<xsl:sort select="name" />
											<tr>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
												<td width="50%" valign="top">
													<xsl:if test="islanguage = 'True'">Sprache: </xsl:if>
													<xsl:value-of select="name" />
													<xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															M
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="total" />
															<xsl:if test="spec != ''"> (<xsl:value-of select="total + 2" />)</xsl:if>
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															<strong>M</strong>
														</xsl:when>
														<xsl:otherwise>
															<strong><xsl:value-of select="rating" /></strong>
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															M
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="attributemod" />
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															M
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="ratingmod" />
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															M
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="poolmod" />
														</xsl:otherwise>
													</xsl:choose>
												</td>
											</tr>
		</xsl:for-each>
		<xsl:if test="mugshot != ''">
											<tr>
												<td colspan="5" style="text-align:center;">
													<br />
													<img src="data:image/png;base64,{mugshotbase64}" />
												</td>
											</tr>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>