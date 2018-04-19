
var backgroundImage;
var youtubeVideo;
var youtubeVideoBlock;
var screenImage;
var titleBlock;
var headsetImage;

var primaryColor;
var secondaryColor;

var screenShotArray;
var screenShotIndex;
var slideshowActive = true;

function init()
{
    backgroundImage = document.getElementById("backgroundImage");
    screenImage = document.getElementById("screenshotImage");
    youtubeVideo = document.getElementById("youtubeVideo");
    youtubeVideoBlock = document.getElementById("youtubeVideoBlock");
    titleBlock = document.getElementById("titleBlock");
    headsetImage = document.getElementById("headsetImage");
    
    document.addEventListener('scroll', onScroll);
    
    backgroundImage.style.opacity = "0.0";
    backgroundImage.style.filter = "alpha(opacity=0)"; /* For IE8 and earlier (from 0 to 100) */
   
    primaryColor = [255, 255, 255];
    secondaryColor = [15, 39, 79];
    
    setUpScreenshots();
    playScreenshotSlideshow(true);
    
    onResize();
    setTimeout(onResize, 200); 
}

function onResize()
{
    //default titleBlock min-width = 500px
    //dafault headsetImage width = 400px
    //default youtube video width and height: 560px, 315px
    
    if (isMobile())
    {
        titleBlock.style.width = "90%";
        headsetImage.style.width = "90%";
        
        youtubeVideo.style.width = "60vw";
        youtubeVideo.style.height = "33vw";
    }
    else
    {
        titleBlock.style.width = "500px";
        headsetImage.style.width = "400px";
        
        youtubeVideo.style.width = "560px";
        youtubeVideo.style.height = "315px";
    }
    
    var windowHeight = window.innerHeight;
    var windowWidth = window.innerWidth;
    
    var titleHeight = titleBlock.offsetHeight;
    var titleWidth = titleBlock.offsetWidth;
    
    var youtubeHeight = youtubeVideoBlock.offsetHeight;
    var youtubeWidth = youtubeVideoBlock.offsetWidth;
    
    if (windowWidth < (1150) || isMobile()) 
    {
    	var firstBlockHeight = firstBlock.offsetHeight;
    	var firstBlockWidth = firstBlock.offsetWidth;
    	
    	var windowHeight = window.innerHeight;
    	var windowWidth = window.innerWidth;
    	
    	if ((titleHeight + youtubeHeight) > windowHeight)
    	{
    		windowHeight = titleHeight + youtubeHeight + 200;
    		firstBlock.style.height = windowHeight + "px";
    	}
    	else
    	{
    		firstBlock.style.height = "100vh";
    	}
    	
    	titleBlock.style.top = ((windowHeight - (titleHeight + youtubeHeight)) / 2) + "px";
    	titleBlock.style.left = ((windowWidth - titleWidth) / 2) + "px";
    	
    	youtubeVideoBlock.style.top = (((windowHeight - (titleHeight + youtubeHeight)) / 2) + titleHeight + 20) + "px";
    	youtubeVideoBlock.style.left = ((windowWidth - youtubeWidth) / 2) + "px";
    }
    else
    {
    	titleBlock.style.top = "25%";
    	titleBlock.style.left = (windowWidth / 2 - titleWidth) + "px";
    	
    	youtubeVideoBlock.style.top = "25%";
    	youtubeVideoBlock.style.left = (windowWidth / 2) + "px";
    }
}

function onScroll(event)
{
    var scrollPercentage = event.target.scrollingElement.scrollTop / screen.height;
    scrollPercentage = scrollPercentage < 1.0 ? scrollPercentage : 1.0;
    var alpha = scrollPercentage < 0.5 ? scrollPercentage : 0.5;
    
    backgroundImage.style.opacity = alpha.toString();
    backgroundImage.style.filter = "alpha(opacity=" + (alpha * 100.0).toString() + ")";
    
    primaryPercent = 1.0 - scrollPercentage;
    secondaryPercent = scrollPercentage;
    
    var r = Math.round(primaryColor[0] * primaryPercent + secondaryColor[0] * secondaryPercent);
    var g = Math.round(primaryColor[1] * primaryPercent + secondaryColor[1] * secondaryPercent);
    var b = Math.round(primaryColor[2] * primaryPercent + secondaryColor[2] * secondaryPercent);
    document.body.style.backgroundColor = "rgb(" + r + "," + g + "," + b + ")";
}

function isMobile() 
{
    if (navigator.userAgent.match(/Android/i)
        || navigator.userAgent.match(/webOS/i)
        || navigator.userAgent.match(/iPhone/i)
        || navigator.userAgent.match(/iPad/i)
        || navigator.userAgent.match(/iPod/i)
        || navigator.userAgent.match(/BlackBerry/i)
        || navigator.userAgent.match(/Windows Phone/i)) {
        return true;
    }
    else 
    {
        return window.innerWidth <= 800;
    }
}

function playScreenshotSlideshow(initial)
{
	if (slideshowActive)
	{
		if (!initial)
		{
			setImageLocation("400px", 0);
		
	    	screenShotIndex = (screenShotIndex + 1) % screenShotArray.length;
	    	screenImage.src = screenShotArray[screenShotIndex];
	    	
	    	setTimeout(function() {
	    		setImageLocation("0px", 1);
	    	}, 1000);  
	    }
	    
	    setTimeout(function() {
    		setImageLocation("-800px", 0);
    	}, 4000);  
	    
    	setTimeout(function() {
    		playScreenshotSlideshow(false);
    	}, 5000);  
    } 
}

function toggleSlideshow()
{
	if (slideshowActive)
	{
		slideshowActive = false;
		document.getElementById("prevImageButton").style.display = "inline-block";
		document.getElementById("nextImageButton").style.display = "inline-block";
		document.getElementById("loadingImage").src = "images/play.png";
			
		screenshotImage.style.marginLeft = "0px";
		screenshotImage.style.opacity = "1"; 
	}
	else
	{
		document.getElementById("prevImageButton").style.display = "none";
		document.getElementById("nextImageButton").style.display = "none";
		document.getElementById("loadingImage").src = "images/pause.ico";
		
		setTimeout(function() {
			slideshowActive = true;
    		playScreenshotSlideshow(true);
    	}, 5000);  
	}
}

function setImageLocation(margin, opacity) 
{
	if (slideshowActive)
	{
		screenshotImage.style.marginLeft = margin;
		screenshotImage.style.opacity = opacity; 
	}
}

function nextImage()
{
	screenShotIndex = (screenShotIndex + 1) % screenShotArray.length;
    screenImage.src = screenShotArray[screenShotIndex];
}

function prevImage() 
{
	screenShotIndex -= 1;
	if (screenShotIndex < 0)
	{
		screenShotIndex = screenShotArray.length - 1;
	}
    screenImage.src = screenShotArray[screenShotIndex];
}

function setUpScreenshots()
{
    screenShotIndex = 0;
    
    screenShotArray = 
    [
        "images/apartmentKitchen.png",
        "images/apartmentLivingRoom.png",
        "images/apartmentMasterBath.png",
        "images/apartmentMasterBed.png",
        "images/boxRoom.png",
        "images/keyboard.png",
        "images/mainRoomLobby.png",
        "images/mainRoomMusic.png",
        "images/newRooms.png",
        "images/teleporting.png",
    ];
}

init();
