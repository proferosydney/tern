Feature: Transactions Disabled
	In order to support changes that cannot run under a transaction
	As a DBA
	I want to be able to disable the use of transactions

Scenario: Changes should not be rolled back when a non-transacted migration fails
	Given I have created an empty database
	And the version scripts
	| Schema  | Version | Script                                             |
	| default | 1.0     | CREATE TABLE [Test] (ID INTEGER NOT NULL)          |
	| default | 1.1     | INSERT INTO [Test] VALUES (1)                      |
	| default | 1.2     | ALTER TABLE [Test] ADD [Name] VARCHAR(32) NOT NULL |
	But I have disabled transactions
	Then attempting to apply the version scripts again should fail
	And the database should contain
	| Table |
	| Test  |
	But the database should not contain
	| Table | Column |
	| Test  | Name   |