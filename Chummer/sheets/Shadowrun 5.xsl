<?xml version="1.0" encoding="UTF-8" ?>
<!-- Character sheet based on the Shadowrun 5th Edition Character Sheet -->
<!-- Created by Keith Rudolph, krudolph@gmail.com -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
	<xsl:include href="Shadowrun 5 Base.xslt"/>
	<xsl:template name="skills1">
		<xsl:variable name="items" select="skills/skill[knowledge = 'False' and (rating &gt; 0 or total &gt; 0)]"/>
		<xsl:variable name="halfcut" select="round(count($items) div 2)"/>
		<xsl:for-each select="$items[position() &lt;= $halfcut]">
			<xsl:sort select="name" />
											<tr>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
												<td width="45%" valign="top">
													<xsl:value-of select="name" />
													<xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:value-of select="total" />
													<xsl:if test="spec != '' and exotic = 'False'"> (<xsl:value-of select="specializedrating" />)</xsl:if>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="rating" />
												</td>
												<td width="20%" style="text-align:center;" valign="top">
													<xsl:value-of select="attributemod" /> (<xsl:value-of select="displayattribute" />)
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="ratingmod" />
												</td>
											</tr>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template name="skills2">
		<xsl:variable name="items" select="skills/skill[knowledge = 'False' and (rating &gt; 0 or total &gt; 0)]"/>
		<xsl:variable name="halfcut" select="round(count($items) div 2)"/>
		<xsl:for-each select="$items[position() &gt; $halfcut]">
			<xsl:sort select="name" />
											<tr>
												<xsl:if test="position() mod 2 != 1">
													<xsl:attribute name="bgcolor">#e4e4e4</xsl:attribute>
												</xsl:if>
												<td width="45%" valign="top">
													<xsl:value-of select="name" />
													<xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:value-of select="total" />
													<xsl:if test="spec != '' and exotic = 'False'"> (<xsl:value-of select="specializedrating" />)</xsl:if>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="rating" />
												</td>
												<td width="20%" style="text-align:center;" valign="top">
													<xsl:value-of select="attributemod" /> (<xsl:value-of select="displayattribute" />)
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:value-of select="ratingmod" />
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
												<td width="45%" valign="top">
													<xsl:if test="islanguage = 'True'">Language: </xsl:if>
													<xsl:value-of select="name" />
													<xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															N
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="total" />
															<xsl:if test="spec != ''"> (<xsl:value-of select="specializedrating" />)</xsl:if>
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															N
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="rating" />
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="20%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															N
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="attributemod" /> (<xsl:value-of select="displayattribute" />)
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															N
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="ratingmod" />
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
												<td width="45%" valign="top">
													<xsl:if test="islanguage = 'islanguage'">Language: </xsl:if>
													<xsl:value-of select="name" />
													<xsl:if test="spec != ''"> (<xsl:value-of select="spec" />)</xsl:if>
												</td>
												<td width="15%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															N
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="total" />
															<xsl:if test="spec != ''"> (<xsl:value-of select="specializedrating" />)</xsl:if>
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															N
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="rating" />
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="20%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															N
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="attributemod" /> (<xsl:value-of select="displayattribute" />)
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td width="10%" style="text-align:center;" valign="top">
													<xsl:choose>
														<xsl:when test="islanguage = 'True' and rating = 0">
															N
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="ratingmod" />
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