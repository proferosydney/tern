Requirements

	SpecFlow VS Extension (VS Gallery)
	Machine.Specifications VS Extension (VS Gallery)

Known Issues

1. Tests in MSBuild.SystemTests fail with the error:
	The type initializer for 'Microsoft.Web.Deployment.DeploymentManager' threw an exception.

	This is caused by a rogue provider installed by SQL 2012. Remove the "Microsoft.Data.Tools..." key from both:

	HKLM\Software\Microsoft\IIS Extensions\msdeploy\3\extensibility
	HKLM\Software\Wow6432Node\Microsoft\IIS Extensions\msdeploy\3\extensibility


	More information: http://stackoverflow.com/questions/6351289