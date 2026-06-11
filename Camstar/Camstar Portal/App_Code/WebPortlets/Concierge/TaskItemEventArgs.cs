// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Text;

namespace Camstar.WebPortal.WebPortlets.Concierge
{
    public class TaskItemEventArgs : EventArgs 
    {

        public virtual string Identifier
        {
            get { return mIdentifier; }
            set { mIdentifier = value; }
        }

        public virtual string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        public virtual string PageURL
        {
            get { return mPageURL; }
            set { mPageURL = value; }
        }

        private string mIdentifier;
        private string mName;
        private string mPageURL;
    }
}
