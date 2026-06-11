using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Namespace responsible for communication with XceleratorShare APIs
/// </summary>
namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class LCSProject
    {
        private string _name;
        private string _id;
        public string Name 
        { get => _name; 
          set 
          {
                if (value != null && value.Length > 0) 
                {
                    _name = value;
                }
                else
                {
                    throw new Exception("Please set valid Name");
                }
          }
        }

        public string Id
        {
            get => _id;
            set
            {
                if (value != null && value.Length > 0)
                {
                    _id = value;
                }
                else
                {
                    throw new Exception("Please set valid ID");
                }
            }
        }

        public LCSProject(string id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}
