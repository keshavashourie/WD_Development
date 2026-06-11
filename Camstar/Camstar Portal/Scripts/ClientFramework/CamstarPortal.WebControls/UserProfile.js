// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
Type.registerNamespace("CamstarPortal.WebControls");

/******************* CamstarPortal.WebControls.SortOrder *******************/
CamstarPortal.WebControls.SortOrder = function() { };

CamstarPortal.WebControls.SortOrder.prototype =
{
    Descending: 0,
    Ascending: 1
};

CamstarPortal.WebControls.SortOrder.registerEnum("CamstarPortal.WebControls.SortOrder", false);

/******************* CamstarPortal.WebControls.SortOrder *******************/
CamstarPortal.WebControls.UserProfileColumn = function() { };

CamstarPortal.WebControls.UserProfileColumn.prototype =
{
    Primary: 0,
    Organization: 1,
    Role: 2
};

CamstarPortal.WebControls.UserProfileColumn.registerEnum("CamstarPortal.WebControls.UserProfileColumn", false);

/******************* CamstarPortal.WebControls.UserDetail *******************/
CamstarPortal.WebControls.UserDetail = function (primary, organization, role)
{
	this._primary = primary;
	this._organization = organization;
	this._role = role;
}

CamstarPortal.WebControls.UserDetail.prototype = 
{
    dispose: function() 
    {        
        this.primary = null;
        this.organization = null;
        this.role = null;
    },
    
    get_primary: function() { return this._primary; },
    
    get_organization: function() { return this._organization; },
    
    get_role: function() { return this._role; },

    get_isStatic: function () { return true; }
}

CamstarPortal.WebControls.UserDetail.registerClass('CamstarPortal.WebControls.UserDetail', null, Sys.IDisposable, Camstar.UI.IUIComponent);

CamstarPortal.WebControls.UserDetails = function(element) 
{ 
    CamstarPortal.WebControls.UserDetails.initializeBase(this, [element]);
    
    this._table = null;
    this._items = null;
    this._currentSortedColumn = null;
    this._currentSortOrder = null;
    this._tableId = null;
}

/******************* CamstarPortal.WebControls.UserDetails *******************/
CamstarPortal.WebControls.UserDetails.prototype =
{
	initialize: function (primary, organization, role)
	{
		this._items = new Array();
		this._currentSortedColumn = eval(CamstarPortal.WebControls.UserProfileColumn.Primary);
		this._currentSortOrder = eval(CamstarPortal.WebControls.SortOrder.Descending);
		this._tableId = "tblUserProfileDetails";

		this._table = $get(this._tableId);

		var primary;
		var organization;
		var role;

		for (var x = 1; x < this._table.rows.length; x++)
		{
			organization = $(this._table.rows[x].cells[0]).html();
			role = $(this._table.rows[x].cells[1]).html();
			primary = $(this._table.rows[x].cells[2]).html();

			this._items[this._items.length++] = new CamstarPortal.WebControls.UserDetail(primary, organization, role);
		}
	},

	dispose: function ()
	{
	},

	sort: function (selectedColumn)
	{
		if (this._items.length > 0)
		{
			//update default sort column and sort order
			this._currentSortOrder = (this._currentSortedColumn == selectedColumn && this._currentSortOrder == eval(CamstarPortal.WebControls.SortOrder.Descending)) ?
				eval(CamstarPortal.WebControls.SortOrder.Ascending) : eval(CamstarPortal.WebControls.SortOrder.Descending);

			//sort based on column & order - check this._currentSortOrder here since sort functions don't have scope to access this._currentSortOrder
			var orderSuffix = this._currentSortOrder == eval(CamstarPortal.WebControls.SortOrder.Descending) ? 'DESC' : 'ASC';
			switch (selectedColumn)
			{
				case 0:
					this._items.sort(eval('this.sortByOrganization_' + orderSuffix)); break;
				case 1:
					this._items.sort(eval('this.sortByRole_' + orderSuffix)); break;
				case 2:
					this._items.sort(eval('this.sortByPrimary_' + orderSuffix)); break;
			}

			this._currentSortedColumn = selectedColumn;

			this.updateTable();

			this._table.rows[0].cells[0].className = "";
			this._table.rows[0].cells[1].className = "";
			this._table.rows[0].cells[2].className = "";

			var img = this._table.rows[0].cells[selectedColumn].childNodes[1];

			if (this._currentSortOrder == eval(CamstarPortal.WebControls.SortOrder.Ascending))
			{
				img.src = "images/userprofile/asc.png";
			}
			else
			{
				img.src = "images/userprofile/desc.png";
			}
			this._table.rows[0].cells[selectedColumn].className = "tdSelected";
		}
	},

	sortByPrimary_DESC: function (a, b)
	{
		var x = a.get_primary().toLowerCase();
		var y = b.get_primary().toLowerCase();

		return x < y ? 1 : x > y ? -1 : 0;
	},

	sortByPrimary_ASC: function (a, b)
	{
		var x = a.get_primary().toLowerCase();
		var y = b.get_primary().toLowerCase();

		return x < y ? -1 : x > y ? 1 : 0;
	},

	sortByOrganization_DESC: function (a, b)
	{
		var x = a.get_organization().toLowerCase();
		var y = b.get_organization().toLowerCase();

		return x < y ? 1 : x > y ? -1 : 0;
	},

	sortByOrganization_ASC: function (a, b)
	{
		var x = a.get_organization().toLowerCase();
		var y = b.get_organization().toLowerCase();

		return x < y ? -1 : x > y ? 1 : 0;
	},

	sortByRole_DESC: function (a, b)
	{
		var x = a.get_role().toLowerCase();
		var y = b.get_role().toLowerCase();

		return x < y ? 1 : x > y ? -1 : 0;
	},

	sortByRole_ASC: function (a, b)
	{
		var x = a.get_role().toLowerCase();
		var y = b.get_role().toLowerCase();

		return x < y ? -1 : x > y ? 1 : 0;
	},

	updateTable: function ()
	{
		for (var x = 0; x < this._items.length; x++)
		{
			$(this._table.rows[x + 1].cells[0]).html(this._items[x].get_organization());
			$(this._table.rows[x + 1].cells[1]).html(this._items[x].get_role());
			$(this._table.rows[x + 1].cells[2]).html(this._items[x].get_primary());
		}
	}
}

CamstarPortal.WebControls.UserDetails.registerClass('CamstarPortal.WebControls.UserDetails', null, Sys.IDisposable);

/******************* CamstarPortal.WebControls.UserProfile *******************/
CamstarPortal.WebControls.UserProfile = function(element) 
{ 
    CamstarPortal.WebControls.UserProfile.initializeBase(this, [element]);
    
    this._container = null;
    this._userDetails = null;
    this._modal = null;
    this._closeButtonId;
    this._closeButton;
    this._orgHeaderId;
    this._roleHeaderId;
    this._primaryHeaderId;
    this._loaded = false;
}

CamstarPortal.WebControls.UserProfile.prototype =
{
    initialize: function ()
    {
        CamstarPortal.WebControls.UserProfile.callBaseMethod(this, 'initialize');
    },

    dispose: function ()
    {
        CamstarPortal.WebControls.UserProfile.callBaseMethod(this, 'dispose');
    },

    sort: function (column)
    {
        this._userDetails.sort(column);
    },

    sortOrg: function ()
    {
        this.sort(0);
    },

    sortRoles: function ()
    {
        this.sort(1);
    },

    sortPrimary: function ()
    {
        this.sort(2);
    },

    show: function ()
    {
        this._container = $("#tblUserProfileContainer");

        this._userDetails = new CamstarPortal.WebControls.UserDetails();
        this._userDetails.initialize();

        if (!this._modal)
        {
            this._modal = new CamstarPortal.WebControls.Modal();
        }

        this._windowChangeDelegate = Function.createDelegate(this, this.center);
        this._closeDelegate = Function.createDelegate(this, this.hide);
        this._sortOrgDelegate = Function.createDelegate(this, this.sortOrg);
        this._sortRolesDelegate = Function.createDelegate(this, this.sortRoles);
        this._sortPrimaryDelegate = Function.createDelegate(this, this.sortPrimary);

        $addHandler($get(this.get_closeButtonId()), 'click', this._closeDelegate);
        $addHandler($get(this.get_orgHeaderId()), 'click', this._sortOrgDelegate);
        $addHandler($get(this.get_roleHeaderId()), 'click', this._sortRolesDelegate);
        $addHandler($get(this.get_primaryHeaderId()), 'click', this._sortPrimaryDelegate);

        $("#DynamicContentDiv").append(this._container);

        $addHandlers(window,
        {
            'resize': this._windowChangeDelegate,
            'scroll': this._windowChangeDelegate,
            'DOMMouseScroll': this._windowChangeDelegate
        }, this);

        this.center();
        this._modal.show();

        this._container.fadeIn();
    },

    hide: function ()
    {
        $(this._container).hide();
        this._modal.hide();

        return false;
    },

    center: function ()
    {
        $(this._container).css({ 'top': ((window.getViewportHeight() / 2) - 360), 'left': ((window.getViewportWidth() / 2) - 400) });
    },

    get_closeButtonId: function () { return "ctl00_WebPartManager_UserProfile_CancelButton"; },

    get_orgHeaderId: function () { return "ctl00_WebPartManager_UserProfile_OrganizationHeaderColumn"; },

    get_roleHeaderId: function () { return "ctl00_WebPartManager_UserProfile_RoleHeaderColumn"; },

    get_primaryHeaderId: function () { return "ctl00_WebPartManager_UserProfile_PrimaryHeaderColumn"; }
}

// Optional descriptor for JSON serialization.
CamstarPortal.WebControls.UserProfile.descriptor =
{
    properties:
    [
        { name: 'closeButtonId', type: String }, 
        { name: 'orgHeaderId', type: String },
        { name: 'roleHeaderId', type: String }, 
        { name: 'primaryHeaderId', type: String }
    ]
}

CamstarPortal.WebControls.UserProfile.registerClass('CamstarPortal.WebControls.UserProfile', Camstar.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
