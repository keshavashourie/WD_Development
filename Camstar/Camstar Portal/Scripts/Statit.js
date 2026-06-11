/*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
##
##     STATIT:  Statistical and Graphical Data Analysis System
##
##     Copyright (C) 2009 Statit Software -- All Rights Reserved
##
##     This file contains copyrighted source code of Statit Software, Inc.
##     Use, duplication, and disclosure are subject to a License Agreement
##     with Statit Software, Inc.
##
##- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
##
##
##     File:         JSTips.js
##     Author:       Jon Trumble
##
##     Description:  Self-contained codeset for generating Javascript tips
##
############################################################################# */

// Global Settings for All Tips
var GTipAllowMove = true;
var GTipAllowResize = false;
var GTipTransparency = 95;
var touchOS = ('ontouchstart' in document.documentElement) ? true : false; 

// Tip Object Definition
var oTips = [];
var gahrefs=[];
var sbWidth = 0;
var pics = new Array();
function TipObject() {

    var newdiv = document.createElement('div');
    newdiv.setAttribute('id','hover'+oTips.length);
    tipobjhtml = '<div id="ctrltop' + oTips.length + '" name="ctrltop' + oTips.length + '" class="ctrltop"></div>' + 
                 '<div id="hovertext' + oTips.length + '" name="hovertext' + oTips.length + '" class="hovertext"></div>'


    
    newdiv.className="mybox"
    newdiv.innerHTML = tipobjhtml
    document.body.appendChild(newdiv)
    this.obj = document.getElementById("hover"+oTips.length)

    this.textelem = document.getElementById("hovertext"+oTips.length);
    this.ctrltop = document.getElementById("ctrltop"+oTips.length);
    this.tipbutton = document.getElementById("tipbuttonx"+oTips.length);
    this.keepup = false;
    this.displayed = false;
    this.tiptext = null;

    if (!touchOS) {
       addEvent(this.obj,"mousedown", getPosition);
       addEvent(this.obj,"mouseover", getPosition);
       addEvent(this.obj,"mouseup", returnFalse);
    }
    
    addEvent(this.obj,"contextmenu", returnFalse);
       
    this.obj.style.cssText = "background-color: transparent; background-color: Lavender; position: absolute; display: none; filter:alpha(opacity=" + GTipTransparency + "); -moz-opacity:0."+ GTipTransparency + ";" +
                "-khtml-opacity: 0."+ GTipTransparency + "; opacity: 0."+ GTipTransparency + "; _zoom: 1;"
    this.textelem.style.cssText = "background-color: Lavender; position: relative; overflow:auto; font-size:8pt; font-family:Tahoma, Arial; " +
                "border-width:0; margin-top:0px; margin-bottom:0px; padding-top:2px; padding-left:4px; padding-right:4px;"
    this.ctrltop.style.cssText = "background-color: Lavender; position: relative; overflow:auto; font-size:8pt; font-family:Tahoma, Arial; " +
                "border-width:0; margin-top:0px; margin-bottom:0px; text-align:right; font-weight:700; cursor:pointer;"                

}

function addEvent(obj, evType, fn){
    if (obj.addEventListener){
        obj.addEventListener(evType, fn, true);
        return true;
    }
    else if (obj.attachEvent){
        var r = obj.attachEvent("on"+evType, fn);
        return r;
    }
    else {
        return false;
    }
}

function preloadImages() {

}

function returnFalse(event) {

    if (event && event.stopPropagation) event.stopPropagation();

    return false;
}

function showTip(event) {

    if (isIE6Plus()) {
        var obj=event.srcElement
    //    var tiptext =obj.tip
    }
    else {
        var obj=event.target
    //    var tiptext =obj.attributes["tip"].value
    }
    var tiptext =obj.attributes["tip"].value

    if (!(tiptext)) return false;

    tiptext=tiptext.replace(/&amp;/g,"&")
    tiptext=tiptext.replace(/&gt;/g,">")
    tiptext=tiptext.replace(/&lt;/g,"<")
    tiptext=tiptext.replace(/\^bb/g,'<b>')
    tiptext=tiptext.replace(/\^eb/g,'</b>')
    tiptext=tiptext.replace(/\^hr &n/g,'<!-- hr --><br>')
    tiptext=tiptext.replace(/\^hr&n/g,'<!-- hr --><br>')
    tiptext=tiptext.replace(/\^hr/g,'<!-- hr --><br>')
    tiptext=tiptext.replace(/\^cr/g,'<font color="Red">')
    tiptext=tiptext.replace(/\^cb/g,'<font color="Blue">')
    tiptext=tiptext.replace(/\^cn/g,'</font>')
    tiptext=tiptext.replace(/&n^et/g,'^et') // Safeguard against an empty row right before the end of a table..

    var tabletiptext
    while (tiptext.indexOf("^bt") >= 0) {
        tabletiptext=tiptext.substr(tiptext.indexOf("^bt")+3,tiptext.length)
        if (tiptext.indexOf("^et") >= 0) {
            tabletiptext=tabletiptext.substr(0,tabletiptext.indexOf("^et"))
        }
        tabletiptext='<table style="background-color:lavender; border-width:0;"><tr><td style="background-color:transparent; font-size:8pt; font-family:Tahoma Arial;">' + tabletiptext + "</td></tr></table>"
        tabletiptext=tabletiptext.replace(/&n/g,'</td></tr><tr><td style="background-color:transparent; font-size:8pt; font-family:Tahoma Arial;">')
        tabletiptext=tabletiptext.replace(/&t/g,'</td><td style="background-color:transparent; font-size:8pt; font-family:Tahoma Arial;">')

        if (tiptext.indexOf("^et") >= 0) {
            tiptext=tiptext.substr(0,tiptext.indexOf("^bt")) + tabletiptext + tiptext.substr(tiptext.indexOf("^et")+3,tiptext.length)
        }
        else {
            tiptext=tiptext.substr(0,tiptext.indexOf("^bt")) + tabletiptext
        }
    }
    // Outside of table structures, turn &n to a basic <br>
    tiptext=tiptext.replace(/&n/g,"<br>")
    // Outside of table structures, turn &t to a 4 consecutive non breaking spaces
    tiptext=tiptext.replace(/&t/g,"&nbsp;&nbsp;&nbsp;&nbsp;")

    addTip(obj,tiptext,true, event)

}

function addTip(oOwner, tiptext, calledFromChart, event) {

    // Verify that the oOwner does have its functions properly overridden

    if (!(oOwner.onmouseout))  addEvent(oOwner, "mouseout", hideTip)
    if (!(oOwner.onmousedown))  addEvent(oOwner, "mousedown", onMouseDownTip)
    if (!(oOwner.onmouseup))  addEvent(oOwner, "mouseup", returnFalse)
    if (!(oOwner.onmouseup))  addEvent(oOwner, "click", tryMenu)
    if (!(oOwner.oncontextmenu))  addEvent(oOwner, "oncontextmenu", returnFalse)

    var thistip = null;
    for (var i=0; i< oTips.length; i++) {
        if (tiptext == oTips[i].tiptext) {
            thistip = oTips[i];
            oOwner.tipid = i;
            i=oTips.length;
        }
    }
    if (thistip == null) {
        thistip = new TipObject();
        oTips[oTips.length] = thistip;
        thistip.tiptext = tiptext;
        oOwner.tipid = oTips.length-1;
    }

    if (!(thistip.displayed)) {
        thistip.textelem.innerHTML=tiptext;

        if (!(thistip.displayed)) {
            thistip.obj.style.width="auto"
        }

        thistip.obj.style.display="inline";
        thistip.displayed=true;

        thistip.obj.style.top='1px'
        thistip.obj.style.left='1px'

        var ev
        if (window.event)
            ev = window.event
        else
            ev=event

        var aWidth=thistip.obj.offsetWidth;
        var aHeight=thistip.obj.offsetHeight;
        var mY = 0;
        if (!touchOS) {
           if (ev.clientY)
               mY = ev.clientY + document.body.scrollTop;
           else
               mY = ev.y + document.body.scrollTop;

           var mX = 0;
           if (ev.clientY)
               mX = ev.clientX + document.body.scrollLeft;
           else
               mX = ev.x + document.body.scrollLeft;
        }
        else { 
         /* Touch OS's (at least iPad) don't actually scroll, they zoom around instead. */
           if (ev.pageY)
              mY = ev.pageY;
           else
              mY = ev.y;

           var mX = 0;
           if (ev.pageX)
              mX = ev.pageX;
           else
              mX = ev.x;
        }

        var sY = 0;
        var sX = 0;
        if (document.body.offsetHeight)
            sY = document.body.offsetHeight
        else
            sY = window.innerHeight

        if (document.body.offsetWidth)
            sX = document.body.offsetWidth
        else
            sX = window.innerWidth
        
        // Direction
        var goVert = 'S'
        if ((mY+aHeight) > sY+document.body.scrollTop-20) goVert='N' // 20 should account for the horizontal scroll bar

        var goHor = 'R'
        if ((mX+aWidth) > sX+ document.body.scrollLeft-20) goHor='L' // 20 should account for the vertical scroll bar
          
        if (goVert=='S')
            thistip.obj.style.top = mY+2 + 'px';
        else {
            var t = mY-aHeight
            if (t<0) t=1
            thistip.obj.style.top = t-2 + 'px';
        }

        if (goHor=='R')
            thistip.obj.style.left = mX+2 + 'px';
        else {
            var l = mX-aWidth
            if (l<0) l=1
            thistip.obj.style.left = l-2 + 'px';
        }

        // Redo this when its closer to the location that we want it in!
        var aWidth=thistip.obj.offsetWidth;
        var aHeight=thistip.obj.offsetHeight;

        // Special Handling for HR
        var iWidth = thistip.obj.offsetWidth
        var tmpreplaced = thistip.tiptext.replace(/<!-- hr --><br>/g,'<hr width=' + iWidth + ' />')
        thistip.textelem.innerHTML=tmpreplaced;

    }

}

function decodeURL(url) {
   var returnurl=url
   try {
      returnurl=decodeURIComponent(url);
   }
   catch(e) {
      returnurl=unescape(url);
   }
   return returnurl
}

function tryMenu(event) {

    if (isIE6Plus()) {
        var obj=event.srcElement
    }
    else {
        var obj=event.target
    }

    if (oTips[obj.tipid])
        if (oTips[obj.tipid].displayed) hideTip(event);

    if ((obj.href.indexOf("&@&http") >= 0) ||  (obj.href.indexOf("&@&Label") >= 0) || (obj.href.indexOf("&@&\\") >= 0) || (obj.href.indexOf("&@&%2F") >= 0) || (obj.href.indexOf("&@&/") >= 0)) {

        if (oTips[obj.tipid])
            if (oTips[obj.tipid].keepup) return false;


        /* We have identified a menu, next separate the fields */
        sArr=obj.href.split(/&@&/g,-1)
        // We will have Title=, Label=, and nothing in the string as the field identifiers  nothing will mean its an href
        var ahrefs=[];
        var alabels=[];
        var asizes=[];
        var awins=[];
        var stitle;
        for (var i=0;i<sArr.length;i++) {
            if (sArr[i].indexOf("Label=") == 0) {
               alabels[alabels.length] = decodeURL(sArr[i].substr(6,sArr[i].length));
            }
            else if (sArr[i].indexOf("Window=") == 0)
               awins[awins.length] = sArr[i].substr(7,sArr[i].length);
            else if (sArr[i].indexOf("Size=") == 0)
               asizes[asizes.length] = sArr[i].substr(5,sArr[i].length);
            else if (sArr[i].indexOf("Title=") == 0)
               stitle = decodeURL(sArr[i].substr(6,sArr[i].length));
            else
               ahrefs[ahrefs.length] = sArr[i];
        }
        gahrefs=ahrefs;
        
        var tiptext='<div style="border-bottom-width:1px; border-bottom-style:solid; border-bottom-color:black; font-weight:700; font-family:Tahoma; font-size:8pt;">' + stitle + "</div>"

        if (obj.id=="") obj.id = obj.uniqueID

        if (ahrefs.length==1) {
            var wid=asizes[0].substring(0,asizes[0].indexOf(","))
            var hei=asizes[0].substring(asizes[0].indexOf(",")+1,asizes[0].length)
            fOpenTip(ahrefs[0], awins[0], wid, hei, obj.id);
            return false;
        }

        for (var i=0;i<ahrefs.length;i++) {
            if (awins[i] == "" && asizes[i] == "") {
               //ahrefs[i]=decodeURL(ahrefs[i])
               //ahrefs[i]=ahrefs[i].replace(/%/g,"%25",-1)
               var aTag = '<li><a style="font-weight:700; font-family:Tahoma; font-size:8pt;" href="' + ahrefs[i] +
                  '" target="basefrm" onclick="fCloseTip(\'' + obj.id + '\')">' + alabels[i] + '</a></li>';
               tiptext=tiptext + aTag;
            } else {
               //ahrefs[i]=decodeURL(ahrefs[i])
               //ahrefs[i]=ahrefs[i].replace(/%/g,"%25",-1)
               var aTag = '<li><a style="font-weight:700; font-family:Tahoma; font-size:8pt;" href="javascript:fOpenTip(gahrefs[' + i + "],'" + awins[i] + "'," + asizes[i] + ",'" +
                  obj.id + '\')">' + alabels[i] + "</a></li>";
               tiptext = tiptext + aTag;
            }
        }

        addTip(obj, tiptext, true, event)

        oTips[obj.tipid].keepup=true;
        if (touchOS)
           oTips[obj.tipid].ctrltop.innerHTML='<div onclick="XoutTip(' + obj.tipid + ')">X&nbsp;</div>' // Tried ontouchstart but iPad didn't perform well with it.
        else
           oTips[obj.tipid].ctrltop.innerHTML='<div>X&nbsp;</div>'
           
        if (oTips[obj.tipid].textelem.offsetWidth)
           oTips[obj.tipid].ctrltop.style.width=oTips[obj.tipid].textelem.offsetWidth + "px"
        else
           oTips[obj.tipid].ctrltop.style.width=oTips[obj.tipid].textelem.clientWidth + "px"        
        if (event.preventDefault) event.preventDefault();
        return false;
    }
    else if (oTips[obj.tipid]) {
        if (oTips[obj.tipid].displayed) hideTip(event);
    }
    return true;

}

function fCloseTip(objid) {
    var obj = document.getElementById(objid);
    hideTipObj(obj,true);
}

function getSessionID() {
   var sSessionID
   var start = document.cookie.indexOf("ASPSESSIONID");
   if (start>0) sSessionID=document.cookie.substring(start,document.cookie.length)
   start = sSessionID.indexOf("=");
   if (start>0) sSessionID=sSessionID.substring(start+1,sSessionID.length)
   start = sSessionID.indexOf(";");
   if (start>0) sSessionID=sSessionID.substring(0,start)
   return sSessionID;
}

function fOpenTip(strUrl,winID,winHSize,winVSize,objid){
    //strUrl=decodeURL(strUrl)
    //strUrl=strUrl.replace(/%/g,"%25",-1)
    //strUrl=strUrl.replace("#","%23")
    
    winID=winID + "_" + getSessionID()
    
    if(winHSize == ''){
        win1 = open(strUrl, winID, "toolbar=no, scrollbars=yes, location=no, statusbar=no, menubar=no, personalbar=no, resizable=yes, left = 50, top = 25");
        win1.focus()
    }
    else {
        win1 = open(strUrl, winID, "toolbar=no, scrollbars=yes, location=no, statusbar=no, menubar=no, personalbar=no, resizable=yes, left = 50, top = 25, width=" + winHSize + ", height=" + winVSize);
        win1.focus()
    }

    var obj = document.getElementById(objid)
    if (obj) hideTipObj(obj,true);
}

function hideTip(event) {

    if (isIE6Plus()) {
        var obj=event.srcElement
    }
    else {
        var obj=event.target
    }
    hideTipObj(obj,false);
}


function hideTipObj(obj,bForce) {
    var cnt=0

    while ((obj.tipid==null) && cnt<10) {
        cnt++;
        obj=obj.parentNode
        if (!(obj)) return false;
    }

    if (oTips[obj.tipid]) {
        if (!(oTips[obj.tipid].keepup)  || bForce) {
            XoutTip(obj.tipid);
        }
    }
}

function onMouseDownTip(event) {

    if (event.button==2 || event.which>=2) {
        if (isIE6Plus()) {
            var obj=event.srcElement
        }
        else {
            var obj=event.target
            if (event.preventDefault) event.preventDefault();
            event.stopPropagation();
        }

        var cnt=0
        while ((obj.tipid==null) && cnt<10) {
            cnt++;
            obj=obj.parentNode
            if (!(obj)) return false;
        }

        oTips[obj.tipid].keepup=true;
        oTips[obj.tipid].ctrltop.innerHTML="X&nbsp;"
        if (oTips[obj.tipid].textelem.offsetWidth)
           oTips[obj.tipid].ctrltop.style.width=oTips[obj.tipid].textelem.offsetWidth + "px"
        else
           oTips[obj.tipid].ctrltop.style.width=oTips[obj.tipid].textelem.clientWidth + "px"
        return false;
    }
}

function XoutTip(tipid) {

    if (oTips[tipid]) {
        oTips[tipid].obj.style.display="none";
        oTips[tipid].displayed=false;
        oTips[tipid].keepup=false;

        if (isIE6Plus()) {
            oTips[tipid].obj.removeNode(true);
        }
        else
        {
            var len = document.body.childNodes.length;
            for(var i = 0; i < len; i++) {
                if(document.body.childNodes[i].id == "hover"+tipid)
                    document.body.removeChild(document.body.childNodes[i]);
            }
        }

        if (oTips.length>1) oTips[tipid]=oTips[oTips.length-1]
        oTips.length = oTips.length-1

    }
}

function isIE6Plus() {
  // only for Win IE 6+
  var strBrwsr= navigator.userAgent.toLowerCase();
  if(strBrwsr.indexOf("msie") > -1 && strBrwsr.indexOf("mac") < 0){
    if(parseInt(strBrwsr.charAt(strBrwsr.indexOf("msie")+5)) < 6) return false;
    return true;
  }
  return false;
}

// Global object to hold drag information.

var dragIt = new Object();

function dragStart(event,tipid, sDir) {

  var x, y;
  if (sbWidth==0) sbWidth=21;

  // If an element id was given, find it. Otherwise use the element being
  // clicked on.
  dragIt.tip=oTips[tipid]

  dragIt.startWidth = dragIt.tip.obj.offsetWidth
  // Get cursor position with respect to the page.

  if (isIE6Plus()) {
    x = window.event.clientX + document.documentElement.scrollLeft
      + document.body.scrollLeft;
    y = window.event.clientY + document.documentElement.scrollTop
      + document.body.scrollTop;
  }
  else {
    x = event.clientX + window.scrollX;
    y = event.clientY + window.scrollY;
  }

  // Save starting positions of cursor and element.

  dragIt.StartX = x;
  dragIt.StartY = y;
  dragIt.StartLeft  = parseInt(dragIt.tip.obj.style.left, 10);
  dragIt.StartTop   = parseInt(dragIt.tip.obj.style.top,  10);
  dragIt.StartHeight = dragIt.tip.obj.offsetHeight;
  dragIt.StartWidth  = dragIt.tip.obj.offsetWidth;
  dragIt.MoveDir = sDir


  if (isNaN(dragIt.StartLeft)) dragIt.StartLeft = 0;
  if (isNaN(dragIt.StartTop))  dragIt.StartTop  = 0;

  dragIt.tip.obj.style.zIndex = ++dragIt.tip.obj.style.zIndex;

  // Capture mousemove and mouseup events on the page.

  if (isIE6Plus()) {
    document.attachEvent("onmousemove", dragGo);
    document.attachEvent("onmouseup",   dragStop);
    window.event.cancelBubble = true;
    window.event.returnValue = false;
  }
  else {
    document.addEventListener("mousemove", dragGo,   true);
    document.addEventListener("mouseup",   dragStop, true);
    if (event.preventDefault) event.preventDefault();
    event.stopPropagation();
    event.cancelBubble(true);
    event.returnValue = false;
  }
  return false;
}


function dragGo(event) {

  var x, y;

  // Get cursor position with respect to the page.

  if (isIE6Plus()) {
    x = window.event.clientX + document.documentElement.scrollLeft
      + document.body.scrollLeft;
    y = window.event.clientY + document.documentElement.scrollTop
      + document.body.scrollTop;
  }
  else {
    x = event.clientX + window.scrollX;
    y = event.clientY + window.scrollY;
  }


  // Move drag element by the same amount the cursor has moved.
  if (dragIt.MoveDir.length == 0) {
    dragIt.tip.obj.style.left = (dragIt.StartLeft + x - dragIt.StartX) + "px";
    dragIt.tip.obj.style.top  = (dragIt.StartTop  + y - dragIt.StartY) + "px";
    dragIt.tip.obj.style.width = dragIt.startWidth + "px";
  }
  else {
    var maxheight = 400
    var maxwidth = 400
    var minheight = 40
    var minwidth = 100

    if (GTipAllowResize) {

        if (dragIt.MoveDir.indexOf("n") >= 0) {
            // Expand the height
            var newheight = (dragIt.StartHeight + dragIt.StartY - y)
            if ((newheight >= minheight) && (newheight <= maxheight)) {
                dragIt.tip.obj.style.height = newheight + "px"
                dragIt.tip.textelem.style.height = (newheight-22) + "px";
                if (isIE6Plus()) dragIt.tip.textelem.style.top = 11
                dragIt.tip.obj.style.top = (dragIt.StartTop - dragIt.StartY + y) + "px";
            }
        }
        if (dragIt.MoveDir.indexOf("s") >= 0) {
            // Expand the height
            var newheight = (dragIt.StartHeight + y - dragIt.StartY)
            if ((newheight >= minheight) && (newheight <= maxheight)) {
                dragIt.tip.obj.style.height = newheight + "px"
                dragIt.tip.textelem.style.height = (newheight-22) + "px";
                if (isIE6Plus()) dragIt.tip.textelem.style.top = 11
            }
        }
        if (dragIt.MoveDir.indexOf("e") >= 0) {
            // Expand the width
            var newwidth = (dragIt.StartWidth - dragIt.StartX + x)
            if ((newwidth >= minwidth) && (newwidth <= maxwidth)) {
                dragIt.tip.obj.style.width = newwidth + "px";
                if (dragIt.tip.obj.offsetWidth) newwidth= dragIt.tip.obj.offsetWidth
           }
        }
        if (dragIt.MoveDir.indexOf("w") >= 0) {
            // Expand the width, move left according to the amount moved
            var newwidth = (dragIt.StartWidth + dragIt.StartX - x)
            if ((newwidth >= minwidth) && (newwidth <= maxwidth)) {
                dragIt.tip.obj.style.width = newwidth + "px";
                // For Image sizes, sometimes the width that we set won't be the actual width of the object (due to HTML sizing restrictions)
                if (dragIt.tip.obj.offsetWidth) newwidth= dragIt.tip.obj.offsetWidth
                dragIt.tip.obj.style.left = (dragIt.StartLeft + x - dragIt.StartX) + "px";
            }
        }
        if ((dragIt.MoveDir.indexOf("e") < 0) && (dragIt.MoveDir.indexOf("w") < 0)) {
            dragIt.tip.obj.style.width = dragIt.startWidth + "px";
        }

    }

  }

  if (isIE6Plus()) {
    event.cancelBubble = true;
    event.returnValue = false;
  }
  else {
    if (event.preventDefault) event.preventDefault();
    event.stopPropagation();
    event.cancelBubble(true);
    event.returnValue = false;
  }
  return false;
}

function dragStop(event) {

  if (isIE6Plus()) {
    document.detachEvent("onmousemove", dragGo);
    document.detachEvent("onmouseup",   dragStop);
  }
  else {
    document.removeEventListener("mousemove", dragGo,   true);
    document.removeEventListener("mouseup",   dragStop, true);
  }

  if (isIE6Plus()) {
      event.cancelBubble = true;
      event.returnValue = false;
    }
  else {
    if (event.preventDefault) event.preventDefault();
    event.stopPropagation();
    event.cancelBubble(true);
  }
  return false;

}


function getPosition(event) {


    var xPos, yPos, offset, dir;
    dir = "";

    if (isIE6Plus())
        var el=event.srcElement
    else
        var el=event.target

    while (!(el.className.toLowerCase() == 'mybox')) {
       el = el.parentNode
    }

    if (isIE6Plus()) {
        xPos = window.event.clientX + document.documentElement.scrollLeft
          + document.body.scrollLeft;
        yPos = window.event.clientY + document.documentElement.scrollTop
          + document.body.scrollTop;
    }
    else {
        xPos = event.clientX + window.scrollX;
        yPos = event.clientY + window.scrollY;
    }

    var tipid // = el.id.substr(5,el.id.length)

    for (var i=0;i<oTips.length;i++) {
        if (oTips[i].obj.id==el.id) {
            tipid=i;
            break;
        }
    }

    if (event.type=='mousedown') {
        if ((event.button<2) || (event.which<2)) {
            // Is this the close button??
            if ((yPos<=11+el.offsetTop) && (xPos >= el.offsetLeft+el.offsetWidth-11)) {
                XoutTip(tipid)
                return true;
            }
            // Is this the drag area??
            if (yPos<=11+el.offsetTop) {
                dragStart(event,tipid, "");
                return true;
            }
        }

        offset=11;

        if ((event.button>=2) || (event.which>=2)) {
            if (yPos<offset+el.offsetTop) dir += "n";
            else if (yPos > el.offsetTop+el.offsetHeight-offset) dir += "s";
            if (xPos<offset+el.offsetLeft) dir += "w";
            else if (xPos > el.offsetLeft+el.offsetWidth-offset) dir += "e";
            if (dir.length>0) dragStart(event,tipid, dir);
        }
    }

}

function scrollbarWidth() {
    var div = $('<div style="width:50px;height:50px;overflow:hidden;position:absolute;top:-200px;left:-200px;"><div style="height:100px;"></div>');
    // Append our div, do our calculation and then remove it
    $('body').append(div);
    var w1 = $('div', div).innerWidth();
    div.css('overflow-y', 'scroll');
    var w2 = $('div', div).innerWidth();
    $(div).remove();
    return (w1 - w2);
}

function checkContextMenu() {
  // Check if Tip Exists, if so return false.
  if (oTips.length > 0) return false;
  return true;

}
