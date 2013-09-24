Feature: Tracking Disabled
	In order to keep my database schema clean
	As a DBA
	I want to be able take responsibility for script idempotence

Scenario: Tracking information should not be stored when tracking is disabled
	Given I have created an empty database
	And the version scripts
	| Schema  | Version | Script                                         |
	| default | 1.0     | CREATE TABLE [Test] (ID INTEGER NOT NULL)      |
	| default | 1.5     | ALTER TABLE [Test] ADD [Name] VARCHAR(32) NULL |
	But I have disabled tracking
	When I apply all the version scripts
	Then the database should contain
	| Table | Column |
	| Test  | ID     |
	| Test  | Name   |
	But the database should not contain
	| Table           |
	| DatabaseVersion |