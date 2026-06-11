// Copyright Siemens 2023  

//TODO: Complete CSOM (Client-Side Object Model)


function TreeViewNode(nodeId)
{
    var me = this;
    var mNodeId = null;
    var mTableElement = null;
    var mSpanElement = null;
    var mType = null;
    var mOriginalText = null;
    var mIsLoaded = false;
    
    var getNodeTableElement = function()
    {
        var parent = $get(mNodeId).parentNode;
        
        while(parent && parent.tagName.toLowerCase() != "table")
        {
            parent = parent.parentNode ? parent.parentNode : parent.parentElement;
        }
        
        mTableElement = parent;
    }
    
    var getNodeSpanElement = function()
    {
        var element;
        
        for (var x = 0; x < mTableElement.rows[0].cells.length; x++)
        {
            if (mTableElement.rows[0].cells[x].childNodes.length > 0)
            {
                element = mTableElement.rows[0].cells[x].childNodes[0];
                
                if (element.getAttribute("type") != "")
                {
                    mSpanElement = element;
                }
            }
        }
    }
    
    me.getText = function() { return mSpanElement.innerHTML; }
    
    me.setText = function(text) { mSpanElement.innerHTML = text; }
    
    me.getPrompt = function() { return mSpanElement.getAttribute("prompt"); }
    
    me.getType = function() { return mSpanElement.getAttribute("type"); }

    me.getDataPath = function() { return mSpanElement.getAttribute("datapath"); }
    
    me.loadOriginalText = function() { if (mOriginalText) me.setText(mOriginalText); }
    
    me.loadPrompt = function() 
    {
        mOriginalText = me.getText();
        me.setText(me.getPrompt());
    }

    mNodeId = nodeId;
    getNodeTableElement();
    getNodeSpanElement();
}
