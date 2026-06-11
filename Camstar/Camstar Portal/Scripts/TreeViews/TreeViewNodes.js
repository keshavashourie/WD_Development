// Copyright Siemens 2023  

//TODO: Complete CSOM (Client-Side Object Model)


function TreeViewNodes()
{
    var me = this;
    var mNodes = new Array();
    
    me.addTreeNode = function(node)
    {
        mNodes.push(node);
    }
    
    me.deleteTreeNode = function(node)
    {
        for (var x = 0; x < mRequestQueue.length; x++)
        {
            if (mNodes[x].Id == node.Id)
            {
                var removedNode = mNodes[x];
                
                mNodes.splice(x, 1);
                return removedNode;
            }
        }
    }
    
    me.count = function()
    {
        return mNodes.length;
    }
}
