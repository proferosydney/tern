Feature: Package
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Package includes migration script
	When I package the web project
	Then the package should contain a the migration script

Scenario: Package includes parameter for connection string
	When I package the web project
	Then the package should contain a parameter "default-Migration Connection String"