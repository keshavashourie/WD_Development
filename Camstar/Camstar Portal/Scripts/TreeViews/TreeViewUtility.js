// Copyright Siemens 2023  

//TODO: Complete CSOM (Client-Side Object Model)


var mstree_PopulateNodeDoCallBack = null;
var mstree_ProcessNodeDate = null;
var mstree_ToggleNode = null;

var mCurrentNode = null;

function csitree_getTreeById(treeId)
{
}

function csitree_getNodeById(nodeId)
{
    return new TreeViewNode(nodeId);
}

function csitree_getTreeByNodeId(nodeId)
{
}

function csitree_toggleNode(data, index, node, lineType, children)
{
    var img = node.childNodes[0];
    
    if (!mCurrentNode)
    {
        mCurrentNode = csitree_getNodeById(node.id)
    }
    
    if (children.style.display == "none")
    {
        children.style.display = "";
        img.src = "images/treeview/expanded.png";
    }
    else
    {
        children.style.display = "none";
        img.src = "images/treeview/collapsed.png";
    }
}

function csitree_populateNodeDoCallBack(context, param)
{   
    mCurrentNode = csitree_getNodeById(context.node.id);
    param = param + "|" + mCurrentNode.getType();
    mCurrentNode.loadPrompt();
    
    alert(mstree_PopulateNodeDoCallBack);
    mstree_PopulateNodeDoCallBack(context, param);
}

function csitree_processNodeData(result, context)
{
    mstree_ProcessNodeData(result, context);
}

function csitree_overrideCallbackMethods()
{
    mstree_ToggleNode = TreeView_ToggleNode;
    mstree_PopulateNodeDoCallBack = TreeView_PopulateNodeDoCallBack;
    mstree_ProcessNodeData = TreeView_ProcessNodeData;
    
    TreeView_ToggleNode = csitree_toggleNode;
    TreeView_PopulateNodeDoCallBack = csitree_populateNodeDoCallBack;
    TreeView_ProcessNodeData = csitree_processNodeData;
}

function csitree_load(treeId)
{
    csitree_applyStyles(treeId, 0);
    csitree_overrideCallbackMethods();
}

function csitree_receiveCallBackFromTreeConsumer(returnValue)
{
    var element = document.createElement(returnValue);
    var id = element.getAttribute("id");
    
    document.getElementById(id).innerHTML = returnValue;
}

function csitree_nodeClicked(nodeId)
{
    var arg;
    
    mCurrentNode = csitree_getNodeById(nodeId);
    
    arg = mCurrentNode.getDataPath() + "||" + mCurrentNode.getType();
    
    csitree_sendCallToTreeConsumer(arg);
}

function csitree_addButtonClick(button)
{
    if (mCurrentNode)
    {   
        var arg = mCurrentNode.getDataPath() + "||" + mCurrentNode.getType();

        switch(button.value)
        {
            case "Add Phase":
                arg += "||1"; break;
            case "Add Plan":
                arg += "||2"; break;
            case "Add Activity":
                arg += "||3"; break;
            default:break;            
        }
        
        csitree_sendCallToTreeConsumer(arg);
    }
    
    return false;
}

function csitree_applyStyles(nodeId, level)
{
    if (nodeId != "")
    {
        var divElement = $get(nodeId);
        
        var elements = divElement.childNodes;
        
        for(var x = 0; x < elements.length; x++)
        {
            if (elements[x].tagName == "TABLE")
            {
                elements[x].cellPadding = 0;
                elements[x].cellSpacing = 0;
                elements[x].className = "level" + level.toString();
            }
            else if (elements[x].tagName == "DIV")
            {
                csitree_applyStyles(elements[x].id, level + 1);
            }
        }
    }   
}

function csitree_onMouseIn(element)
{
    if (element.getAttribute("selected") == "0")
    {
        var parentTree = element.parentElement.parentElement.parentElement.parentElement;
   
        parentTree.rows[0].cells[0].className = "tl";
        parentTree.rows[0].cells[1].className = "tm";
        parentTree.rows[0].cells[2].className = "tr";
        
        parentTree.rows[1].cells[0].className = "ml";
        parentTree.rows[1].cells[2].className = "mr";
        
        parentTree.rows[2].cells[0].className = "bl";
        parentTree.rows[2].cells[1].className = "bm";
        parentTree.rows[2].cells[2].className = "br";
    }
}

function csitree_onMouseOut(element)
{
    if (element.getAttribute("selected") == "0")
    {
        var parentTree = element.parentElement.parentElement.parentElement.parentElement;
        
        parentTree.rows[0].cells[0].className = "";
        parentTree.rows[0].cells[1].className = "";
        parentTree.rows[0].cells[2].className = "";
        
        parentTree.rows[1].cells[0].className = "";
        parentTree.rows[1].cells[2].className = "";
        
        parentTree.rows[2].cells[0].className = "";
        parentTree.rows[2].cells[1].className = "";
        parentTree.rows[2].cells[2].className = "";
    }
}

function csitree_setSelected(element)
{
    
     var parentTree = element.parentElement.parentElement.parentElement.parentElement;
   
    parentTree.rows[0].cells[0].className = "tl";
    parentTree.rows[0].cells[1].className = "tm";
    parentTree.rows[0].cells[2].className = "tr";
    
    parentTree.rows[1].cells[0].className = "ml";
    parentTree.rows[1].cells[2].className = "mr";
    
    parentTree.rows[2].cells[0].className = "bl";
    parentTree.rows[2].cells[1].className = "bm";
    parentTree.rows[2].cells[2].className = "br";
    
    element.setAttribute("selected", "1");
}


