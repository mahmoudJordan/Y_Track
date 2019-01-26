


// tracker class
function ___youtubeTracker___() {

    this.currentVideoId = __getCurrentPageVideoId.call(this);

    function _sendChatterMessage(messageQueryString) {
        __callAjax("www.youtube_chatter_dummy.com?" + messageQueryString);
    }

    function __callAjax(url, callback) {
        var xmlhttp;
        // compatible with IE7+, Firefox, Chrome, Opera, Safari
        xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function () {
            if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
                if (callback && typeof callback === 'function')
                    callback(xmlhttp.responseText);
            }
        }
        xmlhttp.open("GET", url, true);
        xmlhttp.send();
    }


    function __parseQuery(queryString) {

        if (!queryString) return null;

        var query = {};
        var pairs = (queryString[0] === '?' ? queryString.substr(1) : queryString).split('&');
        for (var i = 0; i < pairs.length; i++) {
            var pair = pairs[i].split('=');
            query[decodeURIComponent(pair[0])] = decodeURIComponent(pair[1] || '');
        }
        return query;
    }


    function __getCurrentPageVideoId() {
        // getting the current nav href to grep the query string
        var currentPageHref = window.location.href;
        // getting the currentPage query string
        var queryString = currentPageHref.split("?")[1];
        // if the href does not contain query string stop it 
        if (!queryString) return null;
        //getting the video id and return it
        var queryTokens = __parseQuery.call(this, queryString);
        // if the queryString does not contain video id stop it 
        if (typeof queryTokens['v'] === 'undefined') return null
        else return queryTokens['v'];

    }




    this.beginIntercepting = function () {
        // intercepting video playback request to add the video id to parameters
        let oldXHROpen = window.XMLHttpRequest.prototype.open;
        window.XMLHttpRequest.prototype.open = function (method, url, async, user, password) {
            if (url.indexOf("/videoplayback?") > -1 && url.indexOf("__VIDEO__ID__") === -1 ) {
                //adding the video id to the intercepted query string id
                url += "&__VIDEO__ID__=" + __getCurrentPageVideoId.call(this);
                //console.log("url : " + url);
            }
            return oldXHROpen.apply(this, arguments);
        }
    }


    this.beginTrackVideoId = function () {
        // detect the user change his mind to go to another video
        // store video id on load
        this.currentVideoId = __getCurrentPageVideoId.call(this);
        // listen for changes
        setInterval(function () {
            // is there is a new video id in the url ?
            if (this.currentVideoId != __getCurrentPageVideoId.call(this)) {

                // contruct the message
                var queryMessage = "";
                queryMessage += "__ACTION__=__VIDEO__CHANGED__";
                queryMessage += "&__VIDEO__ID__=" + this.currentVideoId

                // check whether this.currentVideoId is defined in case the visitor came from main page
                if (this.currentVideoId) {
                    // notify the proxy chatter
                    _sendChatterMessage(queryMessage);
                }
                // update the new ID
                this.currentVideoId = __getCurrentPageVideoId.call(this);
            }
        }, 50);
    }

    this.beginTrackWindowClosed = function () {
        // detect if the tab/window is closed
        window.addEventListener("beforeunload", function (e) {
            var queryMessage = "";
            queryMessage += "__ACTION__=__VIDEO__CLOSED__";
            queryMessage += "&__VIDEO__ID__=" + this.currentVideoId;
            _sendChatterMessage(queryMessage);
        });

    }
}




// checking if the tracker is already definded (in case the user came from the main youtube page)
if (!window.__tracker) {
    //activating the tracker
    var __tracker = new ___youtubeTracker___();
    __tracker.beginIntercepting();
    __tracker.beginTrackWindowClosed();
    __tracker.beginTrackVideoId();

    // adding the tracker to the main window object
    this.window.__tracker = __tracker;
    console.log("___youtubeTracker___ is working");
}







