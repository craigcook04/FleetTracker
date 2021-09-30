# Troubleshooting
Some common problems you may run into with the trolley tracker page are an out of service notice with no text OR the webpage is broken and components are missing.
1. Attempt to access the admin dashboard making sure to use the correct credentials, if it cannot authenticate then it is likely a server issue which can consist of a lot of things. Some such things:
    
    * System service 'sstrolley' on Owen's server needs to be restarted.

    * Application may have lost connection to the production database. Check that the database is running and that the schema is intact. Worst case the database will have to be wiped and the system service restarted in order to seed the database again.
    
    * Code has been altered in production and now needs to be redployed from source.

