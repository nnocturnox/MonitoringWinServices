<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" indent="yes" />

	<xsl:template match="/">
		<html>
			<head>
				<title>Services Configuration</title>
				<style>
					table {
					font-family: Arial, sans-serif;
					border-collapse: collapse;
					width: 100%;
					}
					th {
					background-color: #4CAF50;
					color: white;
					padding: 8px;
					text-align: left;
					}
					td {
					border: 1px solid #ddd;
					padding: 8px;
					}
					tr:nth-child(even) {
					background-color: #f2f2f2;
					}
					tr:hover {
					background-color: #ddd;
					}
				</style>
			</head>
			<body>
				<h2>Services Configuration</h2>
				<table>
					<tr>
						<th>Name</th>
						<th>Description</th>
						<th>Status</th>
						<th>Startup Type</th>
						<th>Check Interval</th>
						<th>Monitored Service</th>
					</tr>
					<xsl:for-each select="Configuration/Services/Service">
						<tr>
							<td>
								<xsl:value-of select="Name" />
							</td>
							<td>
								<xsl:value-of select="Description" />
							</td>
							<td>
								<xsl:value-of select="Status" />
							</td>
							<td>
								<xsl:value-of select="StartupType" />
							</td>
							<td>
								<xsl:value-of select="CheckInterval" />
							</td>
							<td>
								<xsl:if test="Name = 'MonitorService'">
									<xsl:value-of select="MonitoredService" />
								</xsl:if>
							</td>
						</tr>
					</xsl:for-each>
				</table>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>

