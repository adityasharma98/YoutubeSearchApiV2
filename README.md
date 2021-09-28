# YoutubeSearchApiV2

There are 2 parts to this project:

1. A service that fetches video information for the query string football every 10 seconds,
stores data in a mongo db database.

2. 2 endpoints which return the following things
   a. api/GetAllVideos?pageNumber=anyNumber>0
      fetches all the videos stored in the db that map to the corresponding pageNumber
   b. api/SearchInVideos?queryString=anyValidString&pageNumber=anyNumber>0
      fetches all the videos stored in dp matching the query string as well as the pageNumber

# How to Run

clone the repository and follow the steps:
1. user terminal to cd into the root folder of the repository(folder containing the docker-compose.yml file)
2. run the command docker-compose up -d
3. on chrome type the following 2 URLS for both end points:
   - localhost:3000/api/GetAllVideos?pageNumber=anyNumber>0 
   - localhost:3000/api/SearchInVideos?queryString=anyValidString&pageNumber=anyNumber>0
