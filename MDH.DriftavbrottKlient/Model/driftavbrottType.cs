namespace SE.MDH.DriftavbrottKlient.Model
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.mdh.se/xsd/se.mdh.modell-driftavbrott-1.0.0.xsd")]
    [System.Xml.Serialization.XmlRootAttribute("driftavbrott", Namespace="http://www.mdh.se/xsd/se.mdh.modell-driftavbrott-1.0.0.xsd", IsNullable=false)]
    public partial class driftavbrottType {
        
        private string kanalField;
        
        private System.DateTime startField;
        
        private System.DateTime slutField;
        
        private nivaType nivaField;
        
        private string meddelande_svField;
        
        private string meddelande_enField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string kanal {
            get {
                return this.kanalField;
            }
            set {
                this.kanalField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public System.DateTime start {
            get {
                return this.startField;
            }
            set {
                this.startField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public System.DateTime slut {
            get {
                return this.slutField;
            }
            set {
                this.slutField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public nivaType niva {
            get {
                return this.nivaField;
            }
            set {
                this.nivaField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string meddelande_sv {
            get {
                return this.meddelande_svField;
            }
            set {
                this.meddelande_svField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string meddelande_en {
            get {
                return this.meddelande_enField;
            }
            set {
                this.meddelande_enField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.mdh.se/xsd/se.mdh.modell-driftavbrott-1.0.0.xsd")]
    public enum nivaType {
        
        /// <remarks/>
        INFO,
        
        /// <remarks/>
        WARN,
        
        /// <remarks/>
        ERROR,
    }
}