<?xml version="1.0" encoding="utf-8"?>

<!-- 
  Copyright 2013 Profero

  Converts the MSpec (http://github.com/machine/machine.specifications) xml 
  output to NUnit output format.
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes" cdata-section-elements="message stack-trace"/>
  <xsl:strip-space elements="*"/>

  <xsl:template match="MSpec">
    <test-results name="MSpec">
      <xsl:apply-templates />
    </test-results>
  </xsl:template>

  <xsl:template match="assembly|context">
    <test-suite name="{@name}" success="True" asserts="0">
      <xsl:attribute name="time">
        <xsl:call-template name="time-value">
          <xsl:with-param name="value" select="@time" />
        </xsl:call-template>
      </xsl:attribute>
      
      <xsl:attribute name="asserts">
        <xsl:value-of select="count(descendant::specification)"/>
      </xsl:attribute>

      <results>
        <xsl:apply-templates select="concern|context|specification" />
      </results>
    </test-suite>
  </xsl:template>

  <xsl:template match="specification">
    <test-case name="{@name}" asserts="1">
      <xsl:attribute name="time">
        <xsl:call-template name="time-value">
          <xsl:with-param name="value" select="@time" />
        </xsl:call-template>
      </xsl:attribute>
      <xsl:attribute name="executed">
        <xsl:call-template name="status-as-executed-boolean">
          <xsl:with-param name="value" select="@status" />
        </xsl:call-template>
      </xsl:attribute>
      <xsl:if test="not(@status = 'not-implemented' or @status = 'ignored')">
        <xsl:attribute name="success">
          <xsl:call-template name="status-as-success-boolean">
            <xsl:with-param name="value" select="@status" />
          </xsl:call-template>
        </xsl:attribute>
      </xsl:if>
      

      <xsl:apply-templates />
    </test-case>
  </xsl:template>

  <xsl:template match="error">
    <error>
      <message>
        <xsl:value-of select="message"/>
      </message>
      <stack-trace>
        <xsl:value-of select="stack-trace"/>
      </stack-trace>
    </error>
  </xsl:template>

  <xsl:template match="concern">
    <test-suite name="{@name}">
      <xsl:apply-templates />
    </test-suite>
  </xsl:template>

  <xsl:template name="time-value">
    <xsl:param name="value" />
    
    <xsl:value-of select="$value div 1000"/>
  </xsl:template>

  <xsl:template name="status-as-executed-boolean">
    <xsl:param name="value" />

    <xsl:choose>
      <xsl:when test="$value = 'not-implemented' or $value = 'ignored'">False</xsl:when>
      <xsl:otherwise>True</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="status-as-success-boolean">
    <xsl:param name="value" />

    <xsl:choose>
      <xsl:when test="$value = 'failed'">False</xsl:when>
      <xsl:otherwise>True</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
</xsl:stylesheet>