TMI PivotViewer README
June 11, 2013

The TMI PivotViewer control is developed in Visual Studio 2012. The TmiPivotViewer solution file contains two projects:

TmiPivotDevelopment: Includes a metadata CSV file and low-resolution images in the Silverlight XAP package to simplify local development.

TmiPivotDeployment: Functionally identical, but excludes metadata and images. Intended for use in production environment, where XAP will be
cracked open and filled with the latest metadata and images as necessary.