![API Home Page](https://github.com/usbr/HdbWebApi/blob/master/Docs/HomPage.png)

This codebase generates the main interface, documentation, and testing service for the available Hydrologic Database (HDB) data services. This service is being developed using a technology stack comprised of Windows/.Net, IIS, Dapper, and Swagger. Contact the developer for feedback, data service questions, or the development of new data services within this web page.

This Application Programming Interface (API) is preliminary or provisional and is subject to revision. It is currently in development and as such, frequent updates, downtimes, and loss of functionality are to be expected. The API has not received final approval by Reclamation. No warranty, expressed or implied, is made as to the functionality of the API nor shall the fact of release constitute any such warranty. The API is provided on the condition that neither Reclamation nor the U.S. Government shall be held liable for any damages resulting from the authorized or unauthorized use of the API.

This codebase was adapated from the example posted at [this](https://github.com/MirzaMerdovic/WebApiStartTemplate) GitHub site.

## Migration notes:

Controllers and tests ported.

Current punchlist:
punchlist items to match new api with old:
- old swagger was at swagger/ui/index not just ui/ or index.html
- old swagger top level organization of HDB Tables, Connections, Timeseries Data and Testing Sandbox doesn't exist. The order of swagger endpoints also doesn't match.
- capitalization of endpoints is different, please restore all lowercase endpoints in swagger and in the API generally
- old datatypes endpoint returns 0 instead of null for agen_id
- old modelruns idtype was a dropdown rather than text entry
- old connect endpoint was top level, not under /hdb
- old connect endpoint has a different return type and structure. If the additional column names in the new result are a database difference, that's ok, but fix the return type and make the column names lowercase. I note the old results had that weird uppercase around and after the first _, we can avoid that!
- old select endpoint is completely missing, please build
- old series endpoints are missing:
-- old post series/r-write and m-write
-- old delete series/r-delete and m-delete
-- old cgi endpoint

Other things to look for:
CRUD functionality
Hydromet pass through
Stored procedure access?
Test with RiverWare?




