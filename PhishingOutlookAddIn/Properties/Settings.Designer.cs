﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PhishingOutlookAddIn.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("phishing@yourbusiness.com")]
        public string phishingEmailAddress {
            get {
                return ((string)(this["phishingEmailAddress"]));
            }
            set {
                this["phishingEmailAddress"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("POTENTIAL PHISHING")]
        public string phishingEmailSubject {
            get {
                return ((string)(this["phishingEmailSubject"]));
            }
            set {
                this["phishingEmailSubject"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Deleted Items")]
        public string phishingEmailFolder {
            get {
                return ((string)(this["phishingEmailFolder"]));
            }
            set {
                this["phishingEmailFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public int phishingEmailMaxReported {
            get {
                return ((int)(this["phishingEmailMaxReported"]));
            }
            set {
                this["phishingEmailMaxReported"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool addinDebug {
            get {
                return ((bool)(this["addinDebug"]));
            }
            set {
                this["addinDebug"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool phishingEmailDeleteComplete {
            get {
                return ((bool)(this["phishingEmailDeleteComplete"]));
            }
            set {
                this["phishingEmailDeleteComplete"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("IT")]
        public string organizationName {
            get {
                return ((string)(this["organizationName"]));
            }
            set {
                this["organizationName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool phishingEmailConfirmationPrompt {
            get {
                return ((bool)(this["phishingEmailConfirmationPrompt"]));
            }
            set {
                this["phishingEmailConfirmationPrompt"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool showSettings {
            get {
                return ((bool)(this["showSettings"]));
            }
            set {
                this["showSettings"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"The Phishing Outlook Add-In will provide an end-user{0}the ability to report Phishing eMails to the IT group,{0}sending the Phish email as an attachment to the Phish{0}Reporting eMail.The Phish eMail will be moved or{0}deleted depending on how the Add-In is configured.{0}{0}For additional help with the plugin, please contact your{0}IT support team.
")]
        public string aboutInfo {
            get {
                return ((string)(this["aboutInfo"]));
            }
            set {
                this["aboutInfo"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://en.wikipedia.org/wiki/Phishing")]
        public string phishingInformationURL {
            get {
                return ((string)(this["phishingInformationURL"]));
            }
            set {
                this["phishingInformationURL"] = value;
            }
        }
    }
}
