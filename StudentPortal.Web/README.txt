Project based on tutorial: https://www.youtube.com/watch?v=_uSw8sh7xKs

Useful PackageManagerConsole (PM) commands:
1. add-migration "<name>"
EF: Create new migration with the specified name.

2. update-database
EF: Update the database with created migrations.

3. update-package -reinstall
Reinstal the packages with NuGet (e.g. when project doesn't run after cloning)

Available connection strings:
- Server=(localdb)\\MSSQLLocalDB;Database=StudentPortalWebMVC;Trusted_Connection=True;TrustServerCertificate=True
- Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=StudentPortalWebMVC;Integrated Security=SSPI