for /f %%f in ('dir /b c:\') do sqlcmd -S grantreporting.claimscon.org -U CCDiamond -P fsdCC!12 -d cc -i %%f -o %%f.csv