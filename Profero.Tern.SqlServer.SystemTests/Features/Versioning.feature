Feature: Versioning
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Background:
	Given the version scripts
	| Schema  | Version | Script                                         |
	| default | 1.0     | CREATE TABLE [Test] (ID INTEGER NOT NULL)      |
	| default | 1.1     | ALTER TABLE [Test] ADD [Name] VARCHAR(32) NULL |

Scenario: Applying scripts makes the correct changes
	Given I have created an empty database
	When I apply all the version scripts
	Then the database should contain
	| Table | Column |
	| Test  | ID     |
	| Test  | Name   |
	And the DatabaseVersion table should contain
	| Schema  | Version |
	| default | 1.0     |
	| default | 1.1     |

Scenario: Generated script is idempotent
	Given I have created an empty database
	When I apply all the version scripts
	And I apply all the version scripts again
	Then the database should contain
	| Table | Column |
	| Test  | ID     |
	| Test  | Name   |
	And the DatabaseVersion table should contain 2 rows