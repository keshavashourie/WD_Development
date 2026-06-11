<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions">
  <!-- This stylesheet creates an HTML MetaData report.  The HTML ouput can be opened in Microsoft Word.  The headers created support creating a table of contents.-->
  <xsl:output method="html" version="1.0" encoding="UTF-8" indent="yes"/>
  <!-- If this paramater is set to true then ObjectChanges objects are excluded from the report -->
  <xsl:param name="OmitChangesObject"/>
  <xsl:template match="//Import">
    <xsl:variable name="V_CDOCount" select="count(CDODefinitions/CDODefinition)"></xsl:variable>
    <xsl:variable name="V_CLFCount" select="count(CLFDefinitions/CLFDefinition)"></xsl:variable>
    <xsl:variable name="V_TableCount" select="count(DBTableDefinitions/DBTableDefinition)"></xsl:variable>
    <xsl:variable name="V_QueryCount" select="count(QueryDefinitions/QueryDefinition)"></xsl:variable>
    <xsl:variable name="V_LabelCount" select="count(LabelCategories/LabelCategory)"></xsl:variable>

    <html>
      <head/>
      <style type="text/css">
        <xsl:comment>
          * { font-family:Verdana;
          font-size:10pt;
          }
          .csiTitle { font-family:Verdana;
          font-size:18pt;
          font-style:normal;
          font-weight:bold;
          text-align:left;
          }
          .csiHeading1 { font-size:16pt;
          font-weight:bold;
          }
          .csiHeading1Div { background-color:#a0c0ff;
          border:1px black solid;
          text-align:center;
          width:100%;
          }
          .csiHeading2 { font-size:14pt;
          font-style:italic;
          font-weight:bold;
          }
          .csiHeading3 { font-size:13pt;
          font-weight:bold;
          }
          .csiHeading4 { font-family:Arial;
          font-size:14pt;
          font-weight:bold;
          }
          .csiHeading5 { font-family:Times New Roman;
          font-size:13pt;
          font-style:italic;
          font-weight:bold;
          }
          .csiHeading6 { font-size:11pt;
          font-weight:bold;
          }
          .csiTable { border:thin solid black; border-collapse: collapse; font-family:Arial Narrow; font-size:8pt; empty-cells:show; }
          .csiTableHeaderCell { border:thin solid black; border-collapse: collapse; text-align:center; }
          .csiTableBodyCell { border:thin solid black; border-collapse: collapse; text-align:left;}
          .csiNavAnchor { font-family:Verdana;
          }
          .csiCodeBlock { font-family:Courier;
          }
        </xsl:comment>
      </style>

      <h1 class="csiTitle">Metadata Report</h1>
      <h2 class="csiHeading4">
        Base Version: <xsl:value-of select="/InSiteMetaData/Header/Versions/Base" />
      </h2>
      <h2 class="csiHeading4">
        Modified Version: <xsl:value-of select="/InSiteMetaData/Header/Versions/Modified" />
      </h2>
      <br />
      <h2 class="csiHeading2">
        <xsl:value-of select="/InSiteMetaData/Header/Message" />
      </h2>
      <br />

      <!-- CDOs -->
      <xsl:if test="$V_CDOCount &gt; 0">
        <div class="csiTitle">
          <span class="csiHeading1">
            <xsl:value-of select="string('CDO Definitions')"/>
          </span>
        </div>
        <xsl:for-each select="//CDODefinitions/CDODefinition">
          <xsl:sort select="@Name" order="ascending"/>
          <h2>
            <a>
              <xsl:attribute name="href">
                <xsl:value-of select="concat('#',@Name)"/>
              </xsl:attribute>
              <xsl:value-of select="@Name"/>
            </a>
            <br/>
          </h2>
        </xsl:for-each>
        <br />
      </xsl:if>
      
      <!-- CLF Definitions -->
      <xsl:if test="$V_CLFCount &gt; 0">
        <div class="csiTitle">
          <span class="csiHeading1">
            <xsl:value-of select="string('CLF Definitions')"/>
          </span>
        </div>
        <xsl:for-each select="//CLFDefinitions/CLFDefinition">
          <xsl:sort select="@Name" order="ascending"/>
          <h2>
            <a>
              <xsl:attribute name="href">
                <xsl:value-of select="concat('#',@Name)"/>
              </xsl:attribute>
              <xsl:value-of select="@Name"/>
            </a>
            <br/>
          </h2>
        </xsl:for-each>
        <br />
      </xsl:if>

      <!-- DBTableDefinitions -->
      <xsl:if test="$V_TableCount &gt; 0">
        <div class="csiTitle">
          <span class="csiHeading1">
            <xsl:value-of select="string('DB Table Definitions')"/>
          </span>
        </div>
        <xsl:for-each select="//DBTableDefinitions/DBTableDefinition">
          <xsl:sort select="@Name" order="ascending"/>
          <h2>
            <a>
              <xsl:attribute name="href">
                <xsl:value-of select="concat('#',@Name)"/>
              </xsl:attribute>
              <xsl:value-of select="@Name"/>
            </a>
            <br/>
          </h2>
        </xsl:for-each>
        <br />
      </xsl:if>

      <!-- Query Defs -->
      <xsl:if test="$V_QueryCount &gt; 0">
        <div class="csiTitle">
          <span class="csiHeading1">
            <xsl:value-of select="string('Query Definitions')"/>
          </span>
        </div>
        <xsl:for-each select="//QueryDefinitions/QueryDefinition">
          <xsl:sort select="@Name" order="ascending"/>
          <h2>
            <a>
              <xsl:attribute name="href">
                <xsl:value-of select="concat('#',@Name)"/>
              </xsl:attribute>
              <xsl:value-of select="@Name"/>
            </a>
            <br/>
          </h2>
        </xsl:for-each>
        <br />
      </xsl:if>
      
      <!-- Labels -->
      <xsl:if test="$V_LabelCount &gt; 0">
        <div class="csiTitle">
          <span class="csiHeading1">
            <xsl:value-of select="string('Label Categories')"/>
          </span>
        </div>
        <xsl:for-each select="//LabelCategories/LabelCategory">
          <xsl:sort select="@Name" order="ascending"/>
          <h2>
            <a>
              <xsl:attribute name="href">
                <xsl:value-of select="concat('#',@Name)"/>
              </xsl:attribute>
              <xsl:value-of select="@Name"/>
            </a>
            <br/>
          </h2>
        </xsl:for-each>
        <br />
      </xsl:if>
      
      <!--	-->
      <!-- Lables -->
      <!--
		<div class="csiTitle">
			<span class="csiHeading1">
				<a class="csiHeading1">				
					<xsl:attribute name="href">#Labels</xsl:attribute>
					<xsl:value-of select="string('Labels')"/>
				</a>
			</span>
		</div>
		<br />-->
      <xsl:apply-templates/>
    </html>
  </xsl:template>
  <xsl:template name="CDODefinition" match="/InSiteMetaData/Import/CDODefinitions">
    <div>
      <h1 class="csiHeading1Div">CDO Definitions</h1>
    </div>
    <xsl:for-each select="CDODefinition">
      <xsl:sort select="@Name" order="ascending"/>
      <xsl:if test="$OmitChangesObject=0 or contains(@Name, 'Changes')!=true()">
        <xsl:variable name="CDODefinitionName" select="@Name"/>
        <h1>
          <xsl:attribute name="Name" select="$CDODefinitionName"/>
          <a>
            <xsl:attribute name="name">
              <xsl:value-of select="$CDODefinitionName"/>
            </xsl:attribute>
            <xsl:value-of select="$CDODefinitionName"/> - <xsl:value-of select="@Action"/>
          </a>
        </h1>
        <p/>
        <xsl:if test="Attributes">
          <p style="color:black;font-size:16px">
            <xsl:choose>
              <xsl:when test="string-length(normalize-space(Attributes/FeatureMembership/New))>0">
                <xsl:text>Feature Membership:  </xsl:text>
                <xsl:value-of select="Attributes/FeatureMembership/New"/>
                <br/>
              </xsl:when>
              <xsl:when test="string-length(normalize-space(Attributes/FeatureMembership))>0">
                <xsl:text>Feature Membership:  </xsl:text>
                <xsl:value-of select="Attributes/FeatureMembership"/>
                <br/>
              </xsl:when>
            </xsl:choose>
            <xsl:if test="string-length(normalize-space(Attributes/CDOUsageMask/NewValue))>0">
              <xsl:text>CDO Usage Mask:  </xsl:text>
              <xsl:value-of select="Attributes/CDOUsageMask/NewValue"/>
              <br/>
            </xsl:if>
            <xsl:if test="string-length(normalize-space(Attributes/ParentCDO/Name))>0">
              <xsl:text>Parent CDO:  </xsl:text>
              <xsl:value-of select="Attributes/ParentCDO/Name"/>
              <br/>
            </xsl:if>
            <xsl:if test="string-length(normalize-space(Attributes/DefaultDBTable/Name))>0">
              <xsl:text>Table:  </xsl:text>
              <xsl:value-of select="Attributes/DefaultDBTable/Name"/>
              <br/>
            </xsl:if>
            <br/>
            <xsl:call-template name="Description">
              <xsl:with-param name="Description" select="Attributes/CDODescription"/>
            </xsl:call-template>
          </p>
        </xsl:if>
        <xsl:if test="CDOFieldDefinitions">
          <xsl:call-template name="CDOFieldDefinitions"/>
        </xsl:if>
        <h2>
          <xsl:if test="EventsAndMethods/Methods/Method">
            <u>
              <xsl:text>Methods</xsl:text>
            </u>
          </xsl:if>
        </h2>
        <xsl:for-each select="EventsAndMethods/Methods/Method">
          <h3>
            <xsl:value-of select="@Name"/>
          </h3>
          <!--<xsl:choose>
							<xsl:when test="string-length(normalize-space(EventsAndMethods/Methods/Method/Attributes/FeatureMembership/New))>0">
								<xsl:text>Feature Membership:  </xsl:text>
								<xsl:value-of select="EventsAndMethods/Methods/Method/Attributes/FeatureMembership/New"/>
							</xsl:when>
							<xsl:when test="string-length(normalize-space(EventsAndMethods/Methods/Method/Attributes/FeatureMembership))>0">
								<xsl:text>Feature Membership:  </xsl:text>
								<xsl:value-of select="EventsAndMethods/Methods/Method/Attributes/FeatureMembership"/>
							</xsl:when>
						</xsl:choose>
						<br/>-->
          <xsl:call-template name="CLFFunctions"/>
        </xsl:for-each>
        <h2>
          <xsl:if test="CDOMapDefinitions/CDOMapDefinition">
            <u>
              <xsl:text>Maps</xsl:text>
            </u>
          </xsl:if>
        </h2>
        <xsl:for-each select="CDOMapDefinitions/CDOMapDefinition">
          <h3>
            <xsl:value-of select="@Name"/>
          </h3>
          <xsl:value-of select="Attributes/Description"/>
          <xsl:call-template name="Maps"/>
        </xsl:for-each>
        <hr/>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="Description">
    <xsl:param name="Description"/>
    <xsl:choose>
      <xsl:when test="string-length(normalize-space($Description/NewValue))>0">
        <xsl:value-of select="concat('', $Description/NewValue)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat('', $Description)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="CDOFieldDefinitions">
    <div>
      <div>
        <table class="csiTable">
          <tr>
            <th class="csiTableHeaderCell">
              <xsl:text>Field</xsl:text>
            </th>
            <th class="csiTableHeaderCell">
              <xsl:text>Label</xsl:text>
            </th>
            <th class="csiTableHeaderCell">
              <xsl:text> Column </xsl:text>
            </th>
            <th class="csiTableHeaderCell">
              <xsl:text>Description</xsl:text>
            </th>
            <th class="csiTableHeaderCell">
              <xsl:text>Data Type</xsl:text>
            </th>
            <th class="csiTableHeaderCell">
              <xsl:text> Constraint </xsl:text>
            </th>
            <th class="csiTableHeaderCell">
              <xsl:text>Value Expression</xsl:text>
            </th>
          </tr>
          <!-- Table -->
          <xsl:for-each select="CDOFieldDefinitions/CDOFieldDefinition">
            <xsl:sort select="@Name" order="ascending"/>
            <tr>
              <td class="csiTableBodyCell">
                <xsl:value-of select="@Name"/>
              </td>
              <td class="csiTableBodyCell">
                <xsl:value-of select="Attributes/Label/Text"/>
              </td>
              <td class="csiTableBodyCell">
                <xsl:value-of select="Attributes/DBColumn/Name"/>
              </td>
              <td class="csiTableBodyCell">
                <!--<xsl:value-of select="Attributes/FieldDescription"/>-->
                <xsl:call-template name="Description">
                  <xsl:with-param name="Description" select="Attributes/FieldDescription"/>
                </xsl:call-template>
              </td>
              <td class="csiTableBodyCell">
                <xsl:if test="Attributes/FieldDef">
                  <xsl:choose>
                    <xsl:when test="Attributes/FieldDef/@Datatype='Object'">
                      <xsl:value-of select="concat(Attributes/FieldDef/@Datatype,'.')"/>
                      <a>
                        <xsl:attribute name="href">
                          <xsl:value-of select="concat('#',Attributes/FieldDef)"/>
                        </xsl:attribute>
                        <xsl:value-of select="Attributes/FieldDef"/>
                      </a>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="concat(Attributes/FieldDef/@Datatype, '.', Attributes/FieldDef)"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:if>
              </td>
              <td class="csiTableBodyCell">
                <xsl:if test="Attributes/Required">
                  <xsl:choose>
                    <xsl:when test="Attributes/Required/@Name|Attributes/Required/NewValue/@Name='Not Required'">
                      <xsl:text>Not Required</xsl:text>
                    </xsl:when>
                    <xsl:when test="Attributes/Required/@Name|Attributes/Required/NewValue/@Name='System Required'">
                      <xsl:text>System</xsl:text>
                    </xsl:when>
                    <xsl:when test="Attributes/Required/@Name|Attributes/Required/NewValue/@Name='User Required'">
                      <xsl:text>User</xsl:text>
                    </xsl:when>
                  </xsl:choose>
                </xsl:if>
                <xsl:if test="Attributes/ExposeToUI='True'">
                  <xsl:text> - User Field</xsl:text>
                </xsl:if>
              </td>
              <td class="csiTableBodyCell">
                <xsl:if test="string-length(Attributes/CurrentValueExpression/text())>0">
                  <xsl:text>CVE: </xsl:text>
                  <xsl:value-of select="Attributes/CurrentValueExpression"/>
                  <br/>
                </xsl:if>
                <xsl:if test="string-length(Attributes/DefaultValueExpression/text())>0">
                  <xsl:text>DVE: </xsl:text>
                  <xsl:value-of select="Attributes/DefaultValueExpression"/>
                  <br/>
                </xsl:if>
                <xsl:if test="string-length(Attributes/DefaultValue/text())>0 and Attributes/DefaultValue/text() !='0'">
                  <xsl:text>Default: </xsl:text>
                  <xsl:value-of select="Attributes/DefaultValue"/>
                  <br/>
                </xsl:if>
              </td>
            </tr>
          </xsl:for-each>
        </table>
        <!-- Table -->
      </div>
      <xsl:if test="CDOFieldDefinitions/CDOFieldDefinition/Events/Event">
        <xsl:for-each select="CDOFieldDefinitions/CDOFieldDefinition/Events/Event">
          <h3>
            <xsl:text>Field: </xsl:text>
            <xsl:value-of select="concat(parent::*/parent::*/@Name, ' Events')"/>
          </h3>
          <xsl:value-of select="concat(@Name, ': ', CLFDefinition/@Name|Attributes/CLFDefinition/@Name)"/>
          <xsl:call-template name="CLFFunctions"/>
        </xsl:for-each>
      </xsl:if>
    </div>
  </xsl:template>
  <xsl:template name="CLFFunctions">
    <div>
      <div>
        <p>
          <xsl:text>CLF:  </xsl:text>
          <xsl:value-of select="CLFDefinition/@Name|Attributes/CLFDefinition/@Name"/>
          <br/>
          <xsl:choose>
            <xsl:when test="string-length(normalize-space(Attributes/FeatureMembership/New))>0">
              <xsl:text>Feature Membership:  </xsl:text>
              <xsl:value-of select="Attributes/FeatureMembership/New"/>
            </xsl:when>
            <xsl:when test="string-length(normalize-space(Attributes/FeatureMembership))>0">
              <xsl:text>Feature Membership:  </xsl:text>
              <xsl:value-of select="Attributes/FeatureMembership"/>
            </xsl:when>
          </xsl:choose>
          <br/>
        </p>
        <table class="csiTable">
          <tr>
            <th class="csiTableHeaderCell">
              <xsl:text>Function</xsl:text>
            </th>
            <th class="csiTableHeaderCell">
              <xsl:text>Parameter</xsl:text>
            </th>
            <th class="csiTableHeaderCell">
              <xsl:text>Value</xsl:text>
            </th>
          </tr>
          <!-- Table -->
          <xsl:for-each select="CLFDefinition/Attributes/CLFFunctions/CLFFunction|Attributes/CLFDefinition/Attributes/CLFFunctions/CLFFunction|CLFDefinition/CLFFunctions/CLFFunction">
            <xsl:for-each select="Attributes/FunctionParameters/FunctionParameter|FunctionParameters/FunctionParameter|Attributes/FunctionParameters[not(FunctionParameter)]">
              <tr>
                <td class="csiTableBodyCell">
                  <xsl:choose>
                    <xsl:when test="position()=1">
                      <xsl:value-of select="parent::*/parent::*/parent::*/@Name|parent::*/parent::*/@Name"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:text> </xsl:text>
                    </xsl:otherwise>
                  </xsl:choose>
                </td>
                <td class="csiTableBodyCell">
                  <xsl:value-of select="@Name"/>
                </td>
                <td class="csiTableBodyCell">
                  <xsl:choose>
                    <xsl:when test="contains(@Name, 'QueryName')">
                      <a>
                        <xsl:attribute name="href">
                          <xsl:value-of select="concat('#',substring-before(substring-after(concat('#',Attributes/Value),'&quot;'),'&quot;'))"/>
                        </xsl:attribute>
                        <xsl:value-of select="Attributes/Value"/>
                      </a>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="Attributes/Value"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </td>
              </tr>
            </xsl:for-each>
          </xsl:for-each>
        </table>
        <!-- Table -->
      </div>
    </div>
  </xsl:template>
  <xsl:template name="Maps">
    <div>
      <div>
        <table class="csiTable">
          <tr>
            <th class="csiTableHeaderCell">
              <xsl:text>Source Field</xsl:text>
            </th>
            <th class="csiTableHeaderCell">
              <xsl:text>Target Field</xsl:text>
            </th>
          </tr>
          <!-- Table -->
          <xsl:for-each select="CDOFieldMapDefinitions/CDOFieldMapDefinition">
            <tr>
              <td class="csiTableBodyCell">
                <xsl:value-of select="Attributes/SourceCDOField/Name"/>
              </td>
              <td class="csiTableBodyCell">
                <xsl:value-of select="Attributes/TargetCDOField/Name"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
        <!-- Table -->
      </div>
    </div>
  </xsl:template>
  <xsl:template match="//Header"/>
  <xsl:template match="//CLFDefinitions">
    <xsl:if test="count(descendant::*)>0">
      <hr/>
      <h1 class="csiHeading1Div">CLF Definitions</h1>
      <xsl:for-each select="CLFDefinition">
        <xsl:sort select="@Name" order="ascending"/>
        <h2>
          <a>
            <xsl:attribute name="name">
              <xsl:value-of select="@Name"/>
            </xsl:attribute>
            <xsl:value-of select="@Name"/>
          </a>
        </h2>
        <p style="color:black;font-size:16px">
          <xsl:value-of select="Attributes/CLFDescription"/>
          <br/>
          <br/>
          <xsl:if test="CLFFunctions/BaseCLFFunctions">
            <table class="csiTable">
              <tr>
                <th colspan="3">Base Functions</th>
              </tr>
              <tr>
                <th class="csiTableHeaderCell">
                  <xsl:text>Function</xsl:text>
                </th>
                <th class="csiTableHeaderCell">
                  <xsl:text>Parameter</xsl:text>
                </th>
                <th class="csiTableHeaderCell">
                  <xsl:text>Value</xsl:text>
                </th>
              </tr>
              <xsl:for-each select="CLFFunctions/BaseCLFFunctions/CLFFunction">
                <xsl:for-each select="Attributes/FunctionParameters/FunctionParameter">
                  <tr>
                    <td class="csiTableBodyCell">
                      <xsl:choose>
                        <xsl:when test="position()=1">
                          <xsl:value-of select="parent::*/parent::*/parent::*/@Name|parent::*/parent::*/@Name"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:text> </xsl:text>
                        </xsl:otherwise>
                      </xsl:choose>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="@Name"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:choose>
                        <xsl:when test="contains(@Name, 'QueryName')">
                          <a>
                            <xsl:attribute name="href">
                              <xsl:value-of select="concat('#',substring-before(substring-after(concat('#',Attributes/Value),'&quot;'),'&quot;'))"/>
                            </xsl:attribute>
                            <xsl:value-of select="Attributes/Value"/>
                          </a>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="Attributes/Value"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </td>
                  </tr>
                </xsl:for-each>
              </xsl:for-each>
            </table>
          </xsl:if>
          <br/>
          <xsl:if test="CLFFunctions/NewCLFFunctions">
            <table class="csiTable">
              <tr>
                <th colspan="3">New Functions</th>
              </tr>
              <tr>
                <th class="csiTableHeaderCell">
                  <xsl:text>Function</xsl:text>
                </th>
                <th class="csiTableHeaderCell">
                  <xsl:text>Parameter</xsl:text>
                </th>
                <th class="csiTableHeaderCell">
                  <xsl:text>Value</xsl:text>
                </th>
              </tr>
              <xsl:for-each select="CLFFunctions/NewCLFFunctions/CLFFunction">
                <xsl:for-each select="Attributes/FunctionParameters/FunctionParameter">
                  <tr>
                    <td class="csiTableBodyCell">
                      <xsl:choose>
                        <xsl:when test="position()=1">
                          <xsl:value-of select="parent::*/parent::*/parent::*/@Name|parent::*/parent::*/@Name"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:text> </xsl:text>
                        </xsl:otherwise>
                      </xsl:choose>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="@Name"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:choose>
                        <xsl:when test="contains(@Name, 'QueryName')">
                          <a>
                            <xsl:attribute name="href">
                              <xsl:value-of select="concat('#',substring-before(substring-after(concat('#',Attributes/Value),'&quot;'),'&quot;'))"/>
                            </xsl:attribute>
                            <xsl:value-of select="Attributes/Value"/>
                          </a>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="Attributes/Value"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </td>
                  </tr>
                </xsl:for-each>
              </xsl:for-each>
            </table>
          </xsl:if>
        </p>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>
  <xsl:template match="//FunctionDefinitions">
    <xsl:if test="count(descendant::*)>0">
      <hr/>
      <h1 class="csiHeading1Div">Function Definitions</h1>
      <xsl:for-each select="FunctionDefinition">
        <xsl:sort select="@Name" order="ascending"/>
        <h2>
          <a>
            <xsl:attribute name="name">
              <xsl:value-of select="@Name"/>
            </xsl:attribute>
            <xsl:value-of select="@Name"/>
          </a>
        </h2>
        <p style="color:black;font-size:16px">
          <xsl:value-of select="Attributes/Description"/>
          <br/>
          <br/>
          <xsl:if test="Parameters/CLFParameter">
            <table class="csiTable">
              <tbody>
                <tr>
                  <th class="csiTableHeaderCell">Parameter</th>
                  <th class="csiTableHeaderCell">Direction</th>
                  <th class="csiTableHeaderCell">Data Type</th>
                  <th class="csiTableHeaderCell">Type Multiplicity</th>
                  <th class="csiTableHeaderCell">Param Multiplicity</th>
                  <th class="csiTableHeaderCell">Default Value</th>
                </tr>
                <xsl:for-each select="Parameters/CLFParameter">
                  <tr>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="@Name"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/Direction"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/DataType"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/TypeMultiplicity"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/ParamMultiplicity"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/DefaultValue"/>
                    </td>
                  </tr>
                </xsl:for-each>
              </tbody>
            </table>
          </xsl:if>
        </p>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>
  <xsl:template match="//DBTableDefinitions">
    <xsl:if test="count(descendant::*)>0">
      <hr/>
      <h1 class="csiHeading1Div">DB Table Definitions</h1>
      <xsl:for-each select="DBTableDefinition">
        <xsl:sort select="@Name" order="ascending"/>
        <h2>
          <a>
            <xsl:attribute name="name">
              <xsl:value-of select="@Name"/>
            </xsl:attribute>
            <xsl:value-of select="@Name"/>
          </a>
        </h2>
        <p style="color:black;font-size:16px">
          <xsl:value-of select="Attributes/DBTableDescription"/>
          <br/>
          <br/>
          <xsl:if test="Columns/DBColumnDefinition">
            <table class="csiTable">
              <tbody>
                <tr>
                  <th class="csiTableHeaderCell">Column Name</th>
                  <th class="csiTableHeaderCell">Column Description</th>
                  <th class="csiTableHeaderCell">SQL Type</th>
                  <th class="csiTableHeaderCell">Primary Key Sequence</th>
                  <th class="csiTableHeaderCell">Allow System To Update</th>
                  <th class="csiTableHeaderCell">Allow System To Delete</th>
                  <th class="csiTableHeaderCell">Precision</th>
                  <th class="csiTableHeaderCell">Scale</th>
                  <th class="csiTableHeaderCell">Force To Upper</th>
                  <th class="csiTableHeaderCell">Force To Lower</th>
                  <th class="csiTableHeaderCell">Associated Column</th>
                </tr>
                <xsl:for-each select="Columns/DBColumnDefinition">
                  <tr>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/DBColumnName"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/DBColumnDescription"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/SQLType/Name"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/PrimaryKeySequence"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/AllowSystemToUpdate"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/AllowSystemToDelete"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/Precision"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/Scale"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/ForceToUpper"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/ForceToLower"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/AssociatedDBColumn/Id"/>
                    </td>
                  </tr>
                </xsl:for-each>
              </tbody>
            </table>
          </xsl:if>
        </p>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>
  <xsl:template match="//FieldDefinitions"/>
  <xsl:template match="//QueryDefinitions">
    <xsl:if test="count(descendant::*)>0">
      <hr/>
      <h1 class="csiHeading1Div">Query Definitions</h1>
      <xsl:for-each select="QueryDefinition">
        <xsl:sort select="@Name" order="ascending"/>
        <h2>
          <a>
            <xsl:attribute name="name">
              <xsl:value-of select="@Name"/>
            </xsl:attribute>
            <xsl:value-of select="@Name"/>
          </a>
        </h2>
        <p style="color:black;font-size:16px">
          <xsl:if test="Attributes/Description">
            <xsl:value-of select="Attributes/Description"/>
            <br/>
          </xsl:if>
          <xsl:for-each select="QueryTexts/QueryText">
            <xsl:value-of select="concat(./parent::QueryTexts/parent::QueryDefinition/@Name, ' : ', @Name)"/>
            <br/>
            <table class="csiTable">
              <tbody>
                <tr class="csiTableBodyCell">
                  <td class="csiCodeBlock">
                    <xsl:choose>
                      <xsl:when test="Attributes/QueryText/NewValue">
                        <xsl:value-of select="Attributes/QueryText/NewValue"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="Attributes/QueryText"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </td>
                </tr>
              </tbody>
            </table>
            <br/>
          </xsl:for-each>
          <xsl:if test="Parameters/QueryDefParm">
            <table class="csiTable">
              <tbody>
                <tr>
                  <th class="csiTableHeaderCell">Parameter</th>
                  <th class="csiTableHeaderCell">Data Type</th>
                </tr>
                <xsl:for-each select="Parameters/QueryDefParm">
                  <tr>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="@Name"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/CPPDataType"/>
                    </td>
                  </tr>
                </xsl:for-each>
              </tbody>
            </table>
          </xsl:if>
        </p>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>
  <xsl:template match="//Dictionaries"/>
  <xsl:template match="//LabelCategories">
    <xsl:if test="count(descendant::*)>0">
      <hr/>
      <h1 class="csiHeading1Div">
        <a>
          <xsl:attribute name="name">
            <xsl:value-of select="string('Labels')"/>
          </xsl:attribute>
          <xsl:value-of select="string('Labels')"/>
        </a>
      </h1>
      <xsl:for-each select="LabelCategory">
        <xsl:sort select="@Name" order="ascending"/>
        <h2>
          <a>
            <xsl:attribute name="name">
              <xsl:value-of select="@Name"/>
            </xsl:attribute>
            <xsl:value-of select="@Name"/>
          </a>
        </h2>
        <p style="color:black;font-size:16px">
          <xsl:if test="LabelDefinitions/LabelDefinition">
            <table class="csiTable">
              <tbody>
                <tr>
                  <th class="csiTableHeaderCell">Label Name</th>
                  <th class="csiTableHeaderCell">Value</th>
                </tr>
                <xsl:for-each select="LabelDefinitions/LabelDefinition">
                  <tr>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="@Name"/>
                    </td>
                    <td class="csiTableBodyCell">
                      <xsl:value-of select="Attributes/DefaultLabelValue"/>
                    </td>
                  </tr>
                </xsl:for-each>
              </tbody>
            </table>
          </xsl:if>
        </p>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
