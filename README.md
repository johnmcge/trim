trim.exe only works when **ffmpeg.exe** is in your path. trim.exe makes it easier to use
ffmpeg to remove a section from the begining or end of a video clip.

usage:\
trim filname.mp4 COMMAND timeString

where COMMAND is either:\
&nbsp;&nbsp; -b  (trim the begining of the video)\
&nbsp;&nbsp; -e  (trim the end of the video)


Examples:

remove the first 7 seconds of a video file:\
trim.exe filename.mp4 -b 7 

remove the first 2 minutes and 38 seconds of a video file:\
trim.exe filename.mp4 -b 2:38

remove everything after the 8 second mark:\
trim.exe filename.mp4 -e 8

remove everything after 12 minutes and 38 seconds:\
trim.exe filename.mp4 -e 12:38

