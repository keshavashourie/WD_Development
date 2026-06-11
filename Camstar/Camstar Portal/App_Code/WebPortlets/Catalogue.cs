// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Text;

namespace Camstar.WebPortal.WebPortlets
{
    public class Catalogue
    {
        public Catalogue(string name)
        {
            this.Name = name;
        }


        public virtual string Name
        {
            get {return _name;}
            set {_name = value; }
        }

        public virtual string Title
        {
            get { return _name; }
        }

        public override string ToString()
        {
            return Name;
        }
        private string _name;
        public static Catalogue EventRecording = new Catalogue("Event Recording");
        public static Catalogue Triage = new Catalogue("Triage");
        public static Catalogue CAPARecording = new Catalogue("CAPA Recording");
        public static Catalogue CamstarIntelligence = new Catalogue("Intelligence");
        public static Catalogue QualityRecordActions = new Catalogue("Quality Record Actions");
        public static Catalogue ProcessModel = new Catalogue("Process Model");
        public static Catalogue PMModeling = new Catalogue("Process Model Modeling");
    }
}
