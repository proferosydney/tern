Tern / Ternkey
=========

_Tern_ is a relational database migration library for .NET which generates migration scripts that:

- Are idempotent (you can run them multiple times on the same database)
- Validate the continuity of the migration (like rejecting scripts that have been changed after being deployed)

_Ternkey_ is builds on _Tern_ and adds turnkey (eh?) integration into Web Deploy.

Installation
----

Install the Ternkey package onto your web application project:

```powershell
PM> Install-Package Profero.Ternkey
```

Or if you just want to reference the library directly:

```powershell
PM> Install-Package Profero.Tern
```

This will:

1. Add some starter scripts into _Database\\default\\1.0.sql_
1. Build _obj\\[Configuration]\default-migration.sql_ when the project is built (if the version scripts have changed, that is)
1. Include the script in when deploying or packaging your web project.
1. Define a web deploy parameter `default-Migration Connection String`

Structure
----

The default structure used by Ternkey is:

```text
Database
  /[schema]
    /1.0.sql
    /1.1.sql
    /2.0.sql
```

*schema* is a simple grouping for related scripts. They can, but don't have to, relate to schemas in SQL server. For example, you might want to include one for your application's database and another for ASP.NET's membership tables.

*Version scripts* default to the format `[version]-*.sql`, where version is a 4 (or less) part version number. Each script contains the SQL statements that make the changes. 

Scripts can contain the `GO` batch separator and will be executed in parts using `sp_executesql`.

Metadata
---

Scripts can contain a number of metadata comments at the start:

```sql
-- This is a description of the script and will appear in the DatabaseVersion table
-- Skip: EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Person')

CREATE TABLE [User]
(
  [ID] INTEGER IDENTITY(1,1) NOT NULL
  [Name] NVARCHAR(256) NOT NULL
)
```

The desription, which is unprefixed, will be used recorded in the DatabaseVersion table for auditing purposes.

The skip statement allows _Tern_ to be introduced to untracked databases by skipping version scripts that match a given query.

Idempotency
----

The scripts that _Tern_ generates guarantee idempotency by recording which scripts have been run in a database table. The generated script only runs scripts that have not previously run.

Continuity Safeguards
----

The generated scripts attempt

1. Ensure deployed script content has not changed by comparing it to a computed hash
1. Ensure intermediate versions (eg. 1.0, 2.0, 1.5) have been added after deployment

Untracked Databases
----

The _Skip_ metadata statement (see Metadata) can be used to introduce _Tern_ to an untracked database or as a way of using Tern without recording versions in a database table.

Using _Skip_ statements brings the responsibility of consistancy entirely on the version scripts, since there is no way of ensuring the skip statement is representative of the changes made by the script.

Contribute
----

We welcome bugfixes and ideas around the provider model. In order to run the automated tests you will need:

1. [NuGet 2.7+](http://visualstudiogallery.msdn.microsoft.com/27077b70-9dad-4c64-adcf-c7cf6bc9970c)
1. [SpecFlow Visual Studio Extension](http://visualstudiogallery.msdn.microsoft.com/9915524d-7fb0-43c3-bb3c-a8a14fbd40ee)
1. [Machine.Specifications Visual Studio Extension](http://visualstudiogallery.msdn.microsoft.com/4abcb54b-53b5-4c44-877f-0397556c5c44)

Just clone/fork, open the solution, build and run the tests using Test Explorer or ReSharper.

### Known Issues

> Tests in MSBuild.SystemTests fail with "DeploymentManager" error

```text
The type initializer for 'Microsoft.Web.Deployment.DeploymentManager' threw an exception.
```

This is caused by a rogue provider installed by SQL 2012. Remove the "Microsoft.Data.Tools..." key from both:

> HKLM\Software\Microsoft\IIS Extensions\msdeploy\3\extensibility
> HKLM\Software\Wow6432Node\Microsoft\IIS Extensions\msdeploy\3\extensibility

More information: [http://stackoverflow.com/questions/6351289](http://stackoverflow.com/questions/6351289)

Limitations
----

1. /\* Multiline comments \*/ are not supported as metadata in scripts (only -- single line comments)
1. The connection string will not appear in the Publish dialog and will need to be populated manually via the publish profile (for MSBuild based deployments) or SetParameters/Commandline (for package based deployments)
1. Properties that change options/conventions do not appear in project properties and must be edited manually in the project file
1. Transactional migrations is currently an all-or-nothing option

Planned Features
----

1. Syntax checking, integrated into the Visual Studio build process (already available on a branch)
1. Automated generation of simple change scripts
1. Automated testing of version scripts (with comparison against canonical schema)
1. Property page UI for migration options

What's a Tern?
----

A [Tern](http://en.wikipedia.org/wiki/Tern) is a family of migratory birds. The Arctic Tern, specifically, has the longest known migration path of any animal.

Migration? Geddit? Yeah...

