Feature: Collisions
	In order to integrate Tern with legacy systems
	As a DBA
	I want to be able to script steps but keep their versions

Scenario: Intermediate versions cannot be added after deployment
	Given I have created an empty database
	And I have applied the version scripts
	| Schema  | Version | Script                                         |
	| default | 1.0     | CREATE TABLE [Test] (ID INTEGER NOT NULL)      |
	| default | 1.5     | ALTER TABLE [Test] ADD [Name] VARCHAR(32) NULL |
	When I add the version scripts
	| Schema  | Version | Script                                                 |
	| default | 1.1     | ALTER TABLE [Test] ADD [Description] VARCHAR(256) NULL |
	Then attempting to apply the version scripts again should fail
	And the DatabaseVersion table should contain 2 rows

Scenario: Scripts cannot be modified after deployment
	Given I have created an empty database
	And I have applied the version scripts
	| Schema  | Version | Script                                         |
	| default | 1.0     | CREATE TABLE [Test] (ID INTEGER NOT NULL)      |
	| default | 1.1     | ALTER TABLE [Test] ADD [Name] VARCHAR(32) NULL |
	When I modify the Script
	| Schema  | Version | Script                                                     |
	| default | 1.1     | ALTER TABLE [Test] ADD [Description] VARCHAR(256) NOT NULL |
	Then attempting to apply the version scripts again should fail
	And the DatabaseVersion table should contain 2 rows