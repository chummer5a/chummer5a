<?xml version="1.0" encoding="UTF-8" ?>
<!-- Produce horizontal blank line -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template name="Xline">
			<xsl:param name="cntl" select="'1'"/>
			<xsl:param name="nte" select="false()"/>
			<xsl:param name="height" select="'*'"/>
		<xsl:variable name="ht">
			<xsl:choose>
				<xsl:when test="contains('LNRWXlnrwx',$height)">
					<xsl:value-of select="$height"/>
				</xsl:when>
				<xsl:when test="$nte">R</xsl:when>
				<xsl:otherwise>N</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:call-template name="RuleLine">
			<xsl:with-param name="cntl" select="$cntl"/>
			<xsl:with-param name="height" select="$ht"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="RuleLine">
			<xsl:param name="cntl" select="'1'"/>
			<xsl:param name="height" select="'L'"/>

		<xsl:variable name="h1">
			<xsl:call-template name="fnx-vet-nmbr">
				<xsl:with-param name="nmbr" select="$height"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="h2">
			<xsl:choose>
				<xsl:when test="$h1 = 'NaN'">
					<xsl:call-template name="fnx-uc">
						<xsl:with-param name="string" select="$height"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="substring-before($h1,'.')"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:choose>
<!--		no print if last row -->
			<xsl:when test="$cntl = '0'"/>
<!--		narrow height -->
			<xsl:when test="$h2 = 'N' or $h2 = '-6'">
				<tr><td colspan="100%"><hr style="
					background: inherit;
					border: none;
					border-top: 1px white;
					color: white;
					display: block;
					font-size: 0;
					height: 1px;
					line-height: 0;
					margin: -6px 0;
					padding: 0;
					width: 100%;
				"/></td></tr>
			</xsl:when>
<!--		regular height -->
			<xsl:when test="$h2 = 'R' or $h2 = '-4'">
				<tr><td colspan="100%"><hr style="
					background: inherit;
					border: none;
					border-top: 1px white;
					color: white;
					display: block;
					font-size: 0;
					height: 1px;
					line-height: 0;
					margin: -4px 0;
					padding: 0;
					width: 100%;
				"/></td></tr>
			</xsl:when>
<!--		wide height -->
			<xsl:when test="$h2 = 'W' or $h2 = '-1'">
				<tr><td colspan="100%"><hr style="
					background: inherit;
					border: none;
					border-top: 1px white;
					color: white;
					display: block;
					font-size: 0;
					height: 1px;
					line-height: 0;
					margin: -1px 0;
					padding: 0;
					width: 100%;
				"/></td></tr>
			</xsl:when>
<!--		extra height -->
			<xsl:when test="$h2 = 'X' or $h2 = '4'">
				<tr><td colspan="100%"><hr style="
					background: inherit;
					border: none;
					border-top: 1px white;
					color: white;
					display: block;
					font-size: 0;
					height: 1px;
					line-height: 0;
					margin: 4px 0;
					padding: 0;
					width: 100%;
				"/></td></tr>
			</xsl:when>
<!--		line only (also the default if invalid parameters) -->
			<xsl:otherwise>
				<tr><td colspan="100%"><hr style="
					background: inherit;
					border: none;
					border-top: 1px white;
					color: white;
					display: block;
					font-size: 0;
					height: 1px;
					line-height: 0;
					margin: -7px 0;
					padding: 0;
					width: 100%;
				"/></td></tr>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>
</xsl:stylesheet>