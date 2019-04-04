# Ytrack(C#)

Y Track is a simple open source tool to capture youtube media traffic requested by web browsers, it's simply take a copy from each media "packet" recieved from any of youtube servers. 

the idea behind Y track is that each time the user visits a youtube webpage to watch a video , the whole video is recevied and rendered by the web browser as a consequetive packets, however if the user closed the browser tab (or the browser itself) and the session has ended the video packets will be cleared by the browser itself so next time the user come back to watch the same video the browser will request, download and render it again . 

Y Track capture all recevied media packets from youtube server before the browser clear them, giving the user a "library" to store and watch every video without requesting it again by the web browser . 

Y Track provides top-notch video management library to ensure high-usability and smooth user experince , it's a light-weight software which will never take much resources to operate .


the Application was written in C# WPF , it is an Open Source application , I'm open for any idea that may improve it . you can fork me in Github or send a Message if you think you found a Bug or you want to add a feature.


# Libraries and tools used to build it
- [Titanium Web Broxy](https://github.com/justcoding121/Titanium-Web-Proxy)
- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode)
- [html-agility-pack](https://github.com/zzzprojects/html-agility-pack)
