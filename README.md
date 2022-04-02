# recipe-shopper-api
Backend for a project where you can create recipes and link the products to shops and monitor the prices

# Setting up
The aim is to have a dockerfile written already doing this, but in the meantime, you'll need to have a postgres db running. I've been using a docker container locally to run it and have configured 2 databases (normal and tests).

Once you have them running, you'll need to update the connection strings in `appsettings.json` to point to these two dbs.

Next, you will need to apply migrations to your database which can be done in the Package Manager Console (PMC). Simply run `Update-Database` and the migrations should apply to your db(s). You may need to do this to each db by appending a flag (`-Connection [ConnectionStringName]` I think) to point to the appropriate db to apply the migrations to.

Run the app and hopefully it is all working.
