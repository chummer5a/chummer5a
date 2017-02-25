<?xml version="1.0" encoding="UTF-8" ?>
<!-- Character notes -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template name="notes">

		<xsl:if test="notes != '' or gamenotes != ''">
			<table><tr><td/></tr></table>
			<xsl:call-template name="TableTitle">
				<xsl:with-param name="name" select="$lang.Notes"/>
			</xsl:call-template>
			<div class="block" id="NotesBlock">
				<table class="tablestyle">
					<tr><td colspan="100%" style="text-align: justify">
						<xsl:call-template name="PreserveLineBreaks">
							<xsl:with-param name="text" select="notes"/>
						</xsl:call-template>
					</td></tr>
					<xsl:if test="notes != '' and gamenotes != ''">
						<tr><td>
							<hr style="
									background: inherit;
									border: none;
									border-top: 1px dashed lightgrey;
									color: white;
									display: block;
									font-size: 0;
									height: 1px;
									line-height: 0;
									margin: 1px 0;
									padding: 0;
									width: 100%;
							"/>
						</td></tr>
					</xsl:if>
					<tr><td colspan="100%" style="text-align: justify">
						<xsl:call-template name="PreserveLineBreaks">
							<xsl:with-param name="text" select="gamenotes"/>
						</xsl:call-template>
					</td></tr>
				</table>
			</div>
			<xsl:call-template name="RowSummary">
				<xsl:with-param name="text" select="$lang.Notes"/>
				<xsl:with-param name="bnme" select="'NotesBlock'"/>
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="concept != ''">
			<table><tr><td/></tr></table>
			<xsl:call-template name="TableTitle">
				<xsl:with-param name="name" select="$lang.Concept"/>
			</xsl:call-template>
			<div class="block" id="ConceptBlock">
				<table class="tablestyle">
					<tr><td colspan="100%" style="text-align: justify">
						<xsl:call-template name="PreserveLineBreaks">
							<xsl:with-param name="text" select="concept"/>
						</xsl:call-template>
					</td></tr>
				</table>
			</div>
			<xsl:call-template name="RowSummary">
				<xsl:with-param name="text" select="$lang.Concept"/>
				<xsl:with-param name="bnme" select="'ConceptBlock'"/>
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="description != ''">
			<table><tr><td/></tr></table>
			<xsl:call-template name="TableTitle">
				<xsl:with-param name="name" select="$lang.Description"/>
			</xsl:call-template>
			<div class="block" id="DescriptionBlock">
				<table class="tablestyle">
					<tr><td colspan="100%" style="text-align: justify">
						<xsl:call-template name="PreserveLineBreaks">
							<xsl:with-param name="text" select="description"/>
						</xsl:call-template>
					</td></tr>
				</table>
			</div>
			<xsl:call-template name="RowSummary">
				<xsl:with-param name="text" select="$lang.Description"/>
				<xsl:with-param name="bnme" select="'DescriptionBlock'"/>
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="background != ''">
			<table><tr><td/></tr></table>
			<xsl:call-template name="TableTitle">
				<xsl:with-param name="name" select="$lang.Background"/>
			</xsl:call-template>
			<div class="block" id="BackgroundBlock">
				<table class="tablestyle">
					<tr><td colspan="100%" style="text-align: justify">
						<xsl:call-template name="PreserveLineBreaks">
							<xsl:with-param name="text" select="background"/>
						</xsl:call-template>
					</td></tr>
				</table>
			</div>
			<xsl:call-template name="RowSummary">
				<xsl:with-param name="text" select="$lang.Background"/>
				<xsl:with-param name="bnme" select="'BackgroundBlock'"/>
			</xsl:call-template>
		</xsl:if>

	</xsl:template>
</xsl:stylesheet>