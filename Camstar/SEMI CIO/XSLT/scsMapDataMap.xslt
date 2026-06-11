<xsl:stylesheet version="1.0"
 xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
 <xsl:output omit-xml-declaration="no" indent="yes"/>
 <xsl:param name="pGroupSize" select="100"/>
 <xsl:param name="countSubstrateMaps">
   <xsl:value-of select="count(/MapData/SubstrateMaps/SubstrateMap)"/>
 </xsl:param>
 <xsl:param name="FromCount" select ="1"/>

  <xsl:template match="/*[name()='MapData']">
    <__InSite __encryption="2">
      <__service __serviceType="scsMapDataMaint">
        <__utcOffset></__utcOffset>
        <__inputData />
        <__perform>
          <__eventName>New</__eventName>
        </__perform>
        <__inputData>
          <ObjectChanges>
            <MapDataLayouts>
                <xsl:for-each select="/*[name()='MapData']/*[name()='Layouts']/*[name()='Layout']">
                  <__listItem __listItemAction="add">
                    <xsl:if test="@LayoutId">
                      <LayoutId>
                        <xsl:value-of select="@LayoutId"/>
                      </LayoutId>
                    </xsl:if>
                    <xsl:if test="@DefaultUnits">
                      <DefaultUnits>
                        <xsl:value-of select="@DefaultUnits"/>
                      </DefaultUnits>
                    </xsl:if>
                    <xsl:if test="*[name()='Dimension']/@X">
                      <DimensionX>
                        <xsl:value-of select="*[name()='Dimension']/@X"/>
                      </DimensionX>
                    </xsl:if>
                    <xsl:if test="*[name()='Dimension']/@Y">
                      <DimensionY>
                        <xsl:value-of select="*[name()='Dimension']/@Y"/>
                      </DimensionY>
                    </xsl:if>
                    <xsl:if test="*[name()='DeviceSize']/@X">
                      <DeviceSizeX>
                        <xsl:value-of select="*[name()='DeviceSize']/@X"/>
                      </DeviceSizeX>
                    </xsl:if>
                    <xsl:if test="*[name()='DeviceSize']/@Y">
                      <DeviceSizeY>
                        <xsl:value-of select="*[name()='DeviceSize']/@Y"/>
                      </DeviceSizeY>
                    </xsl:if>
                    <xsl:if test="*[name()='DeviceSize']/@Units">
                      <DeviceSizeUnits>
                        <xsl:value-of select="*[name()='DeviceSize']/@Units"/>
                      </DeviceSizeUnits>
                    </xsl:if>
                    <xsl:if test="*[name()='StepSize']/@X">
                      <StepSizeX>
                        <xsl:value-of select="*[name()='StepSize']/@X"/>
                      </StepSizeX>
                    </xsl:if>
                    <xsl:if test="*[name()='StepSize']/@Y">
                      <StepSizeY>
                        <xsl:value-of select="*[name()='StepSize']/@Y"/>
                      </StepSizeY>
                    </xsl:if>
                    <xsl:if test="*[name()='StepSize']/@Units">
                      <StepSizeUnits>
                        <xsl:value-of select="*[name()='StepSize']/@Units"/>
                      </StepSizeUnits>
                    </xsl:if>
                    <xsl:if test="*[name()='LowerLeft']/@X">
                      <LowerLeftX>
                        <xsl:value-of select="*[name()='LowerLeft']/@X"/>
                      </LowerLeftX>
                    </xsl:if>
                    <xsl:if test="*[name()='LowerLeft']/@Y">
                      <LowerLeftY>
                        <xsl:value-of select="*[name()='LowerLeft']/@Y"/>
                      </LowerLeftY>
                    </xsl:if>
                    <xsl:if test="*[name()='LowerLeft']/@Units">
                      <LowerLeftUnits>
                        <xsl:value-of select="*[name()='LowerLeft']/@Units"/>
                      </LowerLeftUnits>
                    </xsl:if>
                    <xsl:if test="*[name()='Z']/@Height">
                      <ZHeight>
                        <xsl:value-of select="*[name()='Z']/@Height"/>
                      </ZHeight>
                    </xsl:if>
                    <xsl:if test="*[name()='Z']/@Order">
                      <ZOrder>
                        <xsl:value-of select="*[name()='Z']/@Order"/>
                      </ZOrder>
                    </xsl:if>
                    <xsl:if test="*[name()='Z']/@Units">
                      <ZUnits>
                        <xsl:value-of select="*[name()='Z']/@Units"/>
                      </ZUnits>
                    </xsl:if>
                    <xsl:if test="*[name()='TopImage']">
                      <TopImage>
                        <xsl:value-of select="*[name()='TopImage']"/>
                      </TopImage>
                    </xsl:if>
                    <xsl:if test="*[name()='BottomImage']">
                      <BottomImage>
                        <xsl:value-of select="*[name()='BottomImage']"/>
                      </BottomImage>
                    </xsl:if>
                    <xsl:if test="*[name()='ProductId']">
                      <ProductId>
                        <xsl:value-of select="*[name()='ProductId']"/>
                      </ProductId>
                    </xsl:if>
                    <xsl:if test="@TopLevel">
                      <TopLevel>
                        <xsl:value-of select="@TopLevel"/>
                      </TopLevel>
                    </xsl:if>
                    <xsl:if test="*[name()='ChildLayouts']">
                      <ChildLayouts>
                        <xsl:for-each select="*[name()='ChildLayouts']/*[name()='ChildLayout']">
                          <__listItem __listItemAction="add">
                            <xsl:if test="@LayoutId">
                              <LayoutId>
                                <xsl:value-of select="@LayoutId"/>
                              </LayoutId>
                            </xsl:if>
                          </__listItem>
                        </xsl:for-each>
                      </ChildLayouts>
                    </xsl:if>
                  </__listItem>
                </xsl:for-each>
            </MapDataLayouts>
            <MapDataSubstrates>
                <xsl:for-each select="/*[name()='MapData']/*[name()='Substrates']/*[name()='Substrate']">
                  <__listItem __listItemAction="add">
                    <xsl:if test="@SubstrateType">
                      <SubstrateType>
                        <xsl:value-of select="@SubstrateType"/>
                      </SubstrateType>
                    </xsl:if>
                    <xsl:if test="@SubstrateId">
                      <SubstrateId>
                        <xsl:value-of select="@SubstrateId"/>
                      </SubstrateId>
                    </xsl:if>
                    <xsl:if test="*[name()='LotId']">
                      <LotId>
                        <xsl:value-of select="*[name()='LotId']"/>
                      </LotId>
                    </xsl:if>
                    <xsl:if test="*[name()='CarrierType']">
                      <CarrierType>
                        <xsl:value-of select="*[name()='CarrierType']"/>
                      </CarrierType>
                    </xsl:if>
                    <xsl:if test="*[name()='CarrierId']">
                      <CarrierId>
                        <xsl:value-of select="*[name()='CarrierId']"/>
                      </CarrierId>
                    </xsl:if>
                    <xsl:if test="*[name()='SlotNumber']">
                      <SlotNumber>
                        <xsl:value-of select="*[name()='SlotNumber']"/>
                      </SlotNumber>
                    </xsl:if>
                    <xsl:if test="*[name()='SubstrateNumber']">
                      <SubstrateNumber>
                        <xsl:value-of select="*[name()='SubstrateNumber']"/>
                      </SubstrateNumber>
                    </xsl:if>
                    <xsl:if test="*[name()='GoodDevices']">
                      <GoodDevices>
                        <xsl:value-of select="*[name()='GoodDevices']"/>
                      </GoodDevices>
                    </xsl:if>
                    <xsl:if test="*[name()='SupplierName']">
                      <SupplierName>
                        <xsl:value-of select="*[name()='SupplierName']"/>
                      </SupplierName>
                    </xsl:if>
                    <xsl:if test="*[name()='Status']">
                      <Status>
                        <xsl:value-of select="*[name()='Status']"/>
                      </Status>
                    </xsl:if>
                    <xsl:if test="*[name()='AliasIds']">
                      <AliasIds>
                        <xsl:for-each select="*[name()='AliasIds']/*[name()='AliasId']">
                          <__listItem __listItemAction="add">
                            <xsl:if test="@Type">
                              <AliasIdType>
                                <xsl:value-of select="@Type"/>
                              </AliasIdType>
                            </xsl:if>
                            <xsl:if test="@Value">
                              <AliasIdValue>
                                <xsl:value-of select="@Value"/>
                              </AliasIdValue>
                            </xsl:if>
                          </__listItem>
                        </xsl:for-each>
                      </AliasIds>
                    </xsl:if>
                  </__listItem>
                </xsl:for-each>
            </MapDataSubstrates>
            <MapDataSubstrateMaps>
              <xsl:for-each select="/*[name()='MapData']/*[name()='SubstrateMaps']/*[name()='SubstrateMap']">
                <__listItem __listItemAction="add">
                  <xsl:if test="@SubstrateType">
                    <SubstrateType>
                      <xsl:value-of select="@SubstrateType"/>
                    </SubstrateType>
                  </xsl:if>
                  <xsl:if test="@SubstrateId">
                    <SubstrateId>
                      <xsl:value-of select="@SubstrateId"/>
                    </SubstrateId>
                  </xsl:if>
                  <xsl:if test="@LayoutSpecifier">
                    <LayoutSpecifier>
                      <xsl:value-of select="@LayoutSpecifier"/>
                    </LayoutSpecifier>
                  </xsl:if>
                  <xsl:if test="@SubstrateSide">
                    <SubstrateSide>
                      <xsl:value-of select="@SubstrateSide"/>
                    </SubstrateSide>
                  </xsl:if>
                  <xsl:if test="@Orientation">
                    <Orientation>
                      <xsl:value-of select="@Orientation"/>
                    </Orientation>
                  </xsl:if>
                  <xsl:if test="@OriginLocation">
                    <OriginLocation>
                      <xsl:value-of select="@OriginLocation"/>
                    </OriginLocation>
                  </xsl:if>
                  <xsl:if test="@AxisDirection">
                    <AxisDirection>
                      <xsl:value-of select="@AxisDirection"/>
                    </AxisDirection>
                  </xsl:if>
                  <xsl:if test="*[name()='Overlay']">
                    <Overlays>
                      <xsl:for-each select="*[name()='Overlay']">
                        <__listItem __listItemAction="add">
                          <xsl:if test="@MapName">
                            <MapName>
                              <xsl:value-of select="@MapName"/>
                            </MapName>
                          </xsl:if>
                          <xsl:if test="@MapVersion">
                            <MapVersion>
                              <xsl:value-of select="@MapVersion"/>
                            </MapVersion>
                          </xsl:if>
                          <xsl:if test="*[name()='ReferenceDevices']">
                            <ReferenceDevices>
                              <xsl:for-each select="*[name()='ReferenceDevices']/*[name()='ReferenceDevice']">
                                <__listItem __listItemAction="add">
                                  <xsl:if test="@Name">
                                    <Name>
                                      <xsl:value-of select="@Name"/>
                                    </Name>
                                  </xsl:if>
                                  <xsl:if test="*[name()='Coordinates']/@X">
                                    <CoordinateX>
                                      <xsl:value-of select="*[name()='Coordinates']/@X"/>
                                    </CoordinateX>
                                  </xsl:if>
                                  <xsl:if test="*[name()='Coordinates']/@Y">
                                    <CoordinateY>
                                      <xsl:value-of select="*[name()='Coordinates']/@Y"/>
                                    </CoordinateY>
                                  </xsl:if>
                                  <xsl:if test="*[name()='Position']/@X">
                                    <PositionX>
                                      <xsl:value-of select="*[name()='Position']/@X"/>
                                    </PositionX>
                                  </xsl:if>
                                  <xsl:if test="*[name()='Position']/@Y">
                                    <PositionY>
                                      <xsl:value-of select="*[name()='Position']/@Y"/>
                                    </PositionY>
                                  </xsl:if>
                                  <xsl:if test="*[name()='Position']/@Units">
                                    <PositionUnits>
                                      <xsl:value-of select="*[name()='Position']/@Units"/>
                                    </PositionUnits>
                                  </xsl:if>
                                </__listItem>
                              </xsl:for-each>
                            </ReferenceDevices>
                          </xsl:if>
                          <xsl:if test="*[name()='BinCodeMap']">
                            <BinCodeMaps>
                              <xsl:for-each select="*[name()='BinCodeMap']">
                                <__listItem __listItemAction="add">
                                  <xsl:if test="@BinType">
                                    <BinType>
                                      <xsl:value-of select="@BinType"/>
                                    </BinType>
                                  </xsl:if>
                                  <xsl:if test="@NullBin">
                                    <NullBin>
                                      <xsl:value-of select="@NullBin"/>
                                    </NullBin>
                                  </xsl:if>
                                  <xsl:if test="@MapType">
                                    <MapType>
                                      <xsl:value-of select="@MapType"/>
                                    </MapType>
                                  </xsl:if>
                                  <xsl:if test="*[name()='BinCode']">
                                    <BinCodes>
                                      <xsl:for-each select="*[name()='BinCode']">
                                        <__listItem __listItemAction="add">
                                          <xsl:if test=".">
                                            <BinCodeValues>
                                              <xsl:value-of select="."/>
                                            </BinCodeValues>
                                          </xsl:if>
                                          <xsl:if test="@X">
                                            <BinCodeX>
                                              <xsl:value-of select="@X"/>
                                            </BinCodeX>
                                          </xsl:if>
                                          <xsl:if test="@Y">
                                            <BinCodeY>
                                              <xsl:value-of select="@Y"/>
                                            </BinCodeY>
                                          </xsl:if>
                                          <xsl:if test="@Number">
                                            <BinCodeNumber>
                                              <xsl:value-of select="@Number"/>
                                            </BinCodeNumber>
                                          </xsl:if>
                                        </__listItem>
                                      </xsl:for-each>
                                    </BinCodes>
                                  </xsl:if>
                                  <xsl:if test="*[name()='BinDefinitions']">
                                    <BinDefinitions>
                                      <xsl:for-each select="*[name()='BinDefinitions']/*[name()='BinDefinition']">
                                        <__listItem __listItemAction="add">
                                          <xsl:if test="@BinCode">
                                            <BinCode>
                                              <xsl:value-of select="@BinCode"/>
                                            </BinCode>
                                          </xsl:if>
                                          <xsl:if test="@BinCount">
                                            <BinCount>
                                              <xsl:value-of select="@BinCount"/>
                                            </BinCount>
                                          </xsl:if>
                                          <xsl:if test="@BinDescription">
                                            <BinDescription>
                                              <xsl:value-of select="@BinDescription"/>
                                            </BinDescription>
                                          </xsl:if>
                                          <xsl:if test="@BinQuality">
                                            <BinQuality>
                                              <xsl:value-of select="@BinQuality"/>
                                            </BinQuality>
                                          </xsl:if>
                                          <xsl:if test="@Pick">
                                            <Pick>
                                              <xsl:value-of select="@Pick"/>
                                            </Pick>
                                          </xsl:if>
                                        </__listItem>
                                      </xsl:for-each>
                                    </BinDefinitions>
                                  </xsl:if>
                                </__listItem>
                              </xsl:for-each>
                            </BinCodeMaps>
                          </xsl:if>
                          <xsl:if test="*[name()='DeviceIdMap']">
                            <DeviceIdMaps>
                              <xsl:for-each select="*[name()='DeviceIdMap']">
                                <__listItem __listItemAction="add">
                                  <xsl:if test="*[name()='Id']">
                                    <DeviceIds>
                                      <xsl:for-each select="*[name()='Id']">
                                        <__listItem __listItemAction="add">
                                          <xsl:if test="@X">
                                            <IDX>
                                              <xsl:value-of select="@X"/>
                                            </IDX>
                                          </xsl:if>
                                          <xsl:if test="@Y">
                                            <IDY>
                                              <xsl:value-of select="@Y"/>
                                            </IDY>
                                          </xsl:if>
                                          <xsl:if test=".">
                                            <IDValue>
                                              <xsl:value-of select="."/>
                                            </IDValue>
                                          </xsl:if>
                                        </__listItem>
                                      </xsl:for-each>
                                    </DeviceIds>
                                  </xsl:if>
                                </__listItem>
                              </xsl:for-each>
                            </DeviceIdMaps>
                          </xsl:if>
                          <xsl:if test="*[name()='TransferMap']">
                            <TransferMaps>
                              <xsl:for-each select="*[name()='TransferMap']">
                                <__listItem __listItemAction="add">
                                  <xsl:if test="@FromSubstrateType">
                                    <FromSubstrateType>
                                      <xsl:value-of select="@FromSubstrateType"/>
                                    </FromSubstrateType>
                                  </xsl:if>
                                  <xsl:if test="@FromSubstrateId">
                                    <FromSubstrateId>
                                      <xsl:value-of select="@FromSubstrateId"/>
                                    </FromSubstrateId>
                                  </xsl:if>
                                  <xsl:if test="@FromLayoutSpecifier">
                                    <FromLayoutSpecifier>
                                      <xsl:value-of select="@FromLayoutSpecifier"/>
                                    </FromLayoutSpecifier>
                                  </xsl:if>
                                  <xsl:if test="*[name()='T']">
                                    <Transfers>
                                      <xsl:for-each select="*[name()='T']">
                                        <__listItem __listItemAction="add">
                                          <xsl:if test="@FX">
                                            <FromX>
                                              <xsl:value-of select="@FX"/>
                                            </FromX>
                                          </xsl:if>
                                          <xsl:if test="@FY">
                                            <FromY>
                                              <xsl:value-of select="@FY"/>
                                            </FromY>
                                          </xsl:if>
                                          <xsl:if test="@TX">
                                            <ToX>
                                              <xsl:value-of select="@TX"/>
                                            </ToX>
                                          </xsl:if>
                                          <xsl:if test="@TY">
                                            <ToY>
                                              <xsl:value-of select="@TY"/>
                                            </ToY>
                                          </xsl:if>
                                        </__listItem>
                                      </xsl:for-each>
                                    </Transfers>
                                  </xsl:if>
                                </__listItem>
                              </xsl:for-each>
                            </TransferMaps>
                          </xsl:if>
                        </__listItem>
                      </xsl:for-each>
                    </Overlays>
                  </xsl:if>
                </__listItem>
              </xsl:for-each>
            </MapDataSubstrateMaps>
          </ObjectChanges>
        </__inputData>
        <__execute>
        </__execute>
        <__requestData>
          <CompletionMsg>
          </CompletionMsg>
          <ObjectToChange>
          </ObjectToChange>
        </__requestData>
      </__service>
    </__InSite>
  </xsl:template>
  
  <xsl:template match="/*[not(name()='MapData')]">
    <xsl:copy-of select="/*"/>
  </xsl:template>
</xsl:stylesheet>