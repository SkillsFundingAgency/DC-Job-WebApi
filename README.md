# DC-Job-WebApi
This is Web api used by admin ui and user interface to add/manage ilr files to the job queue.
# Usage
Setup the appsettings file with relevant settings and run the app.
    
      "JobQueueManagerSettings": {
        "ConnectionString": "" // Connection string for the database which contains Jobs to be processed e.g. JobScheduler
      } 
    }

# Dependencies 
* JobScheduler Database with jobs data 
* ESFA.DC.JobQueueManager
