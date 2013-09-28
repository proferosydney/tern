-- This file is an example of a migration version script. It can be deleted to make way for your own version scripts.
-- Skip: EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TernExample')

-- You can also use dates for versions, just change VersioningStyle to Date
CREATE TABLE [TernExample]
(
	ID INT IDENTITY(1,1) NOT NULL
)