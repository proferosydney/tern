Feature: Skip Conditions
	In order to integrate Tern with legacy systems
	As a DBA
	I want to be able to script steps but keep their versions

Scenario: Script is not run when skip is matched
	Given the version scripts
	| Schema  | Version | Script                                                             | Skip                                                                                                |
	| default | 1.0     | CREATE TABLE [Test] (ID INTEGER NOT NULL, [Name] VARCHAR(32) NULL) | EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Test')                           |
	| default | 1.1     | ALTER TABLE [Test] ADD [Name] VARCHAR(32) NULL                     | EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Test' AND COLUMN_NAME = 'Name') |
	And I have created an empty database
	When I apply all the version scripts
	Then the database should contain
	| Table | Column |
	| Test  | ID     |
	| Test  | Name   |
	And the DatabaseVersion table should contain
	| Schema  | Version |
	| default | 1.0     |
	| default | 1.1     |
