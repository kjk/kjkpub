# Summary
A web server usually has a number of static assets (JavaScript files, css files, images).
For easier deployment:
* combine all assets into a single .zip file
* potentially embed the .zip file into final executable
* add code that serves assets either from filesystem (during development) or .zip file (in production)

# Packaging static assets into a single file
Deploying a new version of the application to the server involves copying all necessary files to the server. The less files to copy, the simpler that process is.
Go has great support for zip format, so it’s easy to use a single .zip file. 
