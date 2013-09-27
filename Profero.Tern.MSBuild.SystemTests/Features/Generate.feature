Feature: Generate
	In order to easily manage my database schema
	As a developer
	I want a migration script to automatically build with my application

Scenario: Building creates migration script
	Given I have cleaned the web project
	When I build the web project
	Then the output directory should contain the migration script

@modifyscripts
Scenario: Migration script is not regenerated if version scripts are unchanged
	Given I have built the web project
	And recorded the last modified date of the migration script
	When I build the web project again
	Then the last modified date of the migration script should not change

@modifyscripts
Scenario: Changing a script regenerates migration script
	Given I have built the web project
	And recorded the last modified date of the migration script
	When I modify a version script
	And I build the web project again
	Then the last modified date of the migration script should change

@modifyscripts
Scenario: Adding a script regenerates migration script
	Given I have built the web project
	And recorded the last modified date of the migration script
	When I add a version script
	And I build the web project again
	Then the last modified date of the migration script should change