<?xml version="1.0" encoding="UTF-8" ?>
<!-- Condition Monitor Box Template -->
<!-- Created by KeyMasterOfGozer -->
<!-- Version -500 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<!--
  ***  ConditionMonitor(PenaltyBox,CMWidth,TotalBoxes,OverFlow,DamageTaken)
    Parameters:
      PenaltyBox:  This is the number of hit boxes for each penalty.  3 by default.
      Offset:    The number of boxes that appear before the first Condition Monitor penalty.
      CMWidth:  The number of boxes wide to draw the Condition Monitor  You could set this to the same value
            of PenaltyBox to make the box dynamically wide to match the penalties.  Default is 3.
      TotalBoxes:  This is the number of boxes to draw with no OverFlow.
      OverFlow:  This will draw additional boxes past the TotalBoxes Marked OVR,OVR...OVR,DEAD.
      DamageTaken:This will mark first boxes as "greyed" to indicate previous damage already taken.

  This Template builds a Condition Monitor by calling "ConditionRow", which then recursively calls itself to build 
  successive Rows.  Each Row calls "ConditionBox", which recursively calls iteself to build each box needed in the Row.
-->
  <xsl:template name="ConditionMonitor">
      <xsl:param name="PenaltyBox">3</xsl:param>
      <xsl:param name="Offset">0</xsl:param>
      <xsl:param name="CMWidth">3</xsl:param>
      <xsl:param name="TotalBoxes">10</xsl:param>
      <xsl:param name="OverFlow">0</xsl:param>
      <xsl:param name="DamageTaken">0</xsl:param>
    <style type="text/css">
      .conditionmonitorbox {
        border: solid 2px #1c4a2d;
        width: 29px;
        height: 24px;
        text-align: right;
        vertical-align: text-bottom;
        font-weight: bold;
      }
      .conditionmonitorboxfilled {
        border: solid 2px #1c4a2d;
        width: 29px;
        height: 24px;
        text-align: right;
        vertical-align: text-bottom;
        font-weight: bold;
        background-color: #bbbbbb;
      }
      .conditionmonitorboxnotused {
        border: solid 0px red;
        width: 29px;
        height: 24px;
        text-align: right;
        vertical-align: text-bottom;
        font-weight: bold;
        background-color: #FFFFFF;
      }
      .indent {
        padding-left: 20px;
      }
      .block {
        page-break-inside: avoid;
      }
    </style>

    <table cellspacing="0" cellpadding="0" border="0">
      <tr><td>
        <xsl:call-template name="ConditionRow">
          <xsl:with-param name="PenaltyBox"><xsl:value-of select="$PenaltyBox" /></xsl:with-param>
          <xsl:with-param name="Offset"><xsl:value-of select="$Offset" /></xsl:with-param>
          <xsl:with-param name="CMWidth"><xsl:value-of select="$CMWidth" /></xsl:with-param>
          <xsl:with-param name="TotalBoxes"><xsl:value-of select="$TotalBoxes" /></xsl:with-param>
          <xsl:with-param name="DamageTaken"><xsl:value-of select="$DamageTaken" /></xsl:with-param>
          <xsl:with-param name="OverFlow"><xsl:value-of select="$OverFlow" /></xsl:with-param>
          <xsl:with-param name="LowBox">1</xsl:with-param>
          <xsl:with-param name="HighBox"><xsl:value-of select="$CMWidth" /></xsl:with-param>
        </xsl:call-template>
        </td></tr>
    </table>
  </xsl:template>

<!--
  ***  ConditionRow(PenaltyBox,CMWidth,TotalBoxes,OverFlow,DamageTaken,LowBox,HighBox)
    Parameters:
      PenaltyBox:  This is the number of hit boxes for each penalty.  3 by default.
      Offset:    The number of boxes that appear before the first Condition Monitor penalty.
      CMWidth:  The number of boxes wide to draw the Condition Monitor  You could set this to the same value
            of PenaltyBox to make the box dynamically wide to match the penalties.  Default is 3.
      TotalBoxes:  This is the number of boxes to draw with no OverFlow.
      OverFlow:  This will draw additional boxes past the TotalBoxes Marked OVR,OVR...OVR,DEAD.
      DamageTaken:This will mark first boxes as "greyed" to indicate previous damage already taken.
      LowBox:    This is the number of the first box in this Row.
      HighBox:  This is the last box in this Row.

  This will draw the number of boxes from LowBox to HighBox by calling "ConditionBox", which will recursively
  call itself, then ConditionRow Compares it's HighBox to TotalBoxes+OverFlow to determine if more Rows are
  needed.  If more are needed, it calls itself incrementing LowBox and HighBox by CMWidth.
-->
  <xsl:template name="ConditionRow">
      <xsl:param name="PenaltyBox">3</xsl:param>
      <xsl:param name="Offset">0</xsl:param>
      <xsl:param name="CMWidth">3</xsl:param>
      <xsl:param name="TotalBoxes">10</xsl:param>
      <xsl:param name="DamageTaken">0</xsl:param>
      <xsl:param name="OverFlow">0</xsl:param>
      <xsl:param name="LowBox">1</xsl:param>
      <xsl:param name="HighBox">3</xsl:param>
    <tr>
      <xsl:call-template name="ConditionBox">
        <xsl:with-param name="PenaltyBox"><xsl:value-of select="$PenaltyBox" /></xsl:with-param>
        <xsl:with-param name="Offset"><xsl:value-of select="$Offset" /></xsl:with-param>
        <xsl:with-param name="CMWidth"><xsl:value-of select="$CMWidth" /></xsl:with-param>
        <xsl:with-param name="TotalBoxes"><xsl:value-of select="$TotalBoxes" /></xsl:with-param>
        <xsl:with-param name="DamageTaken"><xsl:value-of select="$DamageTaken" /></xsl:with-param>
        <xsl:with-param name="OverFlow"><xsl:value-of select="$OverFlow" /></xsl:with-param>
        <xsl:with-param name="LowBox"><xsl:value-of select="$LowBox" /></xsl:with-param>
        <xsl:with-param name="HighBox"><xsl:value-of select="$HighBox" /></xsl:with-param>
      </xsl:call-template>
    </tr>
    <xsl:if test="$HighBox &lt; ($TotalBoxes + $OverFlow)">
      <xsl:call-template name="ConditionRow">
        <xsl:with-param name="PenaltyBox"><xsl:value-of select="$PenaltyBox" /></xsl:with-param>
        <xsl:with-param name="Offset"><xsl:value-of select="$Offset" /></xsl:with-param>
        <xsl:with-param name="CMWidth"><xsl:value-of select="$CMWidth" /></xsl:with-param>
        <xsl:with-param name="TotalBoxes"><xsl:value-of select="$TotalBoxes" /></xsl:with-param>
        <xsl:with-param name="DamageTaken"><xsl:value-of select="$DamageTaken" /></xsl:with-param>
        <xsl:with-param name="OverFlow"><xsl:value-of select="$OverFlow" /></xsl:with-param>
        <xsl:with-param name="LowBox"><xsl:value-of select="$LowBox + $CMWidth" /></xsl:with-param>
        <xsl:with-param name="HighBox"><xsl:value-of select="$HighBox + $CMWidth" /></xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
    
<!--
  *** ConditionBox(PenaltyBox,CMWidth,TotalBoxes,OverFlow,DamageTaken,LowBox,HighBox)
    Parameters:
      PenaltyBox:  This is the number of hit boxes for each penalty.  3 by default.
      Offset:    The number of boxes that appear before the first Condition Monitor penalty.
      CMWidth:  The number of boxes wide to draw the Condition Monitor  You could set this to the same value
            of PenaltyBox to make the box dynamically wide to match the penalties.  Default is 3.
      TotalBoxes:  This is the number of boxes to draw with no OverFlow.
      OverFlow:  This will draw additional boxes past the TotalBoxes Marked OVR,OVR...OVR,DEAD.
      DamageTaken:This will mark first boxes as "greyed" to indicate previous damage already taken.
      LowBox:    This is the number of the first box in this Row.
      HighBox:  This is the last box in this Row.
      
    This template draws a box, which can be colored based on it having Damage Taken, and also, it determines
  if this box needs text in it, such as markings for OverFlow, or Penalties.  Then it determins if more 
  boxes are needed for this Row(Is LowBox less than HighBox), if more are needed, it recursively calls
  itself incrementing LowBox and HighBox by CMWidth.
-->
  <xsl:template name="ConditionBox">
      <xsl:param name="PenaltyBox">3</xsl:param>
      <xsl:param name="Offset">0</xsl:param>
      <xsl:param name="CMWidth">3</xsl:param>
      <xsl:param name="TotalBoxes">10</xsl:param>
      <xsl:param name="DamageTaken">0</xsl:param>
      <xsl:param name="OverFlow">0</xsl:param>
      <xsl:param name="LowBox">1</xsl:param>
      <xsl:param name="HighBox">3</xsl:param>
    <td>
      <xsl:attribute name="class">
        <xsl:choose> <!-- Choose Background Color for this Box. -->
          <xsl:when test="$LowBox &lt;= $DamageTaken">conditionmonitorboxfilled</xsl:when>
          <xsl:when test="$LowBox &gt; ($TotalBoxes + $OverFlow)">conditionmonitorboxnotused</xsl:when>
          <xsl:otherwise>conditionmonitorbox</xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <!-- Determine any text needed in this Box. -->
      <xsl:choose>
        <xsl:when test="$LowBox &gt; ($TotalBoxes + $OverFlow)">&#160;</xsl:when>
        <!-- Last Box of OverFlow needs DEAD -->
        <xsl:when test="$LowBox = ($TotalBoxes + $OverFlow) and $OverFlow &gt; 0">
          <text><br/><xsl:value-of select="$lang.Dead"/></text>
        </xsl:when>
        <!-- Boxes of OverFlow are Marked OVR -->
        <xsl:when test="$LowBox &gt; $TotalBoxes">
          <text><br/><xsl:value-of select="$lang.OVR"/></text>
        </xsl:when>
        <!-- Last Normal Box -->
        <xsl:when test="$LowBox = $TotalBoxes">
          <text><br/><xsl:value-of select="$lang.Down"/></text>
        </xsl:when>
        <!-- Boxes that incur a penalty are shown with the penalty -->
        <xsl:when test="($LowBox - $Offset) mod $PenaltyBox = 0 and $LowBox &gt; $Offset">
          <xsl:value-of select="(($LowBox - $Offset) div $PenaltyBox) * -1" />
        </xsl:when>
        <!-- This box is empty -->
        <xsl:otherwise>&#160;</xsl:otherwise>
      </xsl:choose>
    </td>
    <xsl:if test="$LowBox &lt; $HighBox">
      <xsl:call-template name="ConditionBox">
        <xsl:with-param name="PenaltyBox"><xsl:value-of select="$PenaltyBox" /></xsl:with-param>
        <xsl:with-param name="Offset"><xsl:value-of select="$Offset" /></xsl:with-param>
        <xsl:with-param name="CMWidth"><xsl:value-of select="$CMWidth" /></xsl:with-param>
        <xsl:with-param name="TotalBoxes"><xsl:value-of select="$TotalBoxes" /></xsl:with-param>
        <xsl:with-param name="DamageTaken"><xsl:value-of select="$DamageTaken" /></xsl:with-param>
        <xsl:with-param name="OverFlow"><xsl:value-of select="$OverFlow" /></xsl:with-param>
        <xsl:with-param name="LowBox"><xsl:value-of select="$LowBox + 1" /></xsl:with-param>
        <xsl:with-param name="HighBox"><xsl:value-of select="$HighBox" /></xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
