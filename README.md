### Ex5 ANPR Camera Image File Service (Service Worker App) ###

**Assumptions.**

1. Files can be placed in existing directory structure.
2. Full directory trees can be created at/copied to the target directory structure and containing files must be processed.
3. Files cannot be processed more than once, meaning that before processing, we must check to see if we have that file on system. Since filenames are not unique between cameras, we store the and compare against filepath. This is enforced at the application and database level.
4. Database needs to be built on startup.
5. Search is by date only not datetime.


**Issues.**

1. I did not make a Windows Service, I made a Worker. I realise that this is not the requirement. However, the business logic is basically the same. My apologies for this over-sight. I was using VS Code and the dotnet comment line to create projects and it does not have Windows Service as a project type. Its a little late now to convert te project, though I think it would be straight forward.
2. I used Sqlite for the database. Obviously, this has consequences to the solution. Principally because there are no Date or Time types.
3. Regarding placing directory trees within the watched structure. Drag and drop works, cut and paste does not. This seems to be a known limitation of FileWatcher (multiple events, stack overflow etc). With more time I could overcome this limitation
4. There are no unit tests. 


**Notes.**

1. The folder watched by default is ..\WatchFolder (one folder above the project folder it's created by the app if it doesn't exist)
2. I used azurite for Azure Blob Storage emulation. Remember to start azurite with (azurite --location = ./Azurite in the project folder)
3. The Fetch Camera Date Range queries can be found in the Queries folder of the project.
4. I used Sqlite for the database.
5. You'll need a tool like Postman to view the API (I dont have swagger set up).
