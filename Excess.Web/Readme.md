Castle.Windsor ASP.NET MVC bootstrapping package
------------------------------------------------

This package simplifies bootstrapping of `Castle.Windsor` container in your ASP.NET MVC 4 application. 
To install this package, use following NuGet command:

    PM> Install-Package Castle.Windsor.Web.Mvc

Package contains `WindsorControllerFactory`, `ControllersInstaller` and `WindsorActivator` classes for your MVC application.

All controllers are now resolved with controller factory. To register your custom components 
just [create another Installer class](http://docs.castleproject.org/Windsor.Installers.ashx)
implementing `IWindsorInstaller` interface.

Fo more informations [how to use Windsor](http://docs.castleproject.org/Windsor.MainPage.ashx) see the
[documentation](http://docs.castleproject.org/Windsor.MainPage.ashx).

[![endorse](https://api.coderwall.com/rarous/endorsecount.png)](https://coderwall.com/rarous)
