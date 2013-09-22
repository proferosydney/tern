Feature: Transactions Enabled
	In order to protect against script errors
	As a DBA
	I want to be able to rollback migration if it fails

Scenario: Changes should be rolled back when a transacted migration fails
	Given I have created an empty database
	And the version scripts
	| Schema  | Version | Script                                             |
	| default | 1.0     | CREATE TABLE [Test] (ID INTEGER NOT NULL)          |
	| default | 1.1     | INSERT INTO [Test] VALUES (1)                      |
	| default | 1.2     | ALTER TABLE [Test] ADD [Name] VARCHAR(32) NOT NULL |
	Then attempting to apply the version scripts again should fail
	And the database should not contain
	| Table |
	| Test  |