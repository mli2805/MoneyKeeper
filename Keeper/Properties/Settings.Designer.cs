﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Keeper.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Keeper.dbx")]
        public string DbxFile {
            get {
                return ((string)(this["DbxFile"]));
            }
            set {
                this["DbxFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\Temp\\dr5g6-egr6e-wegffs-w34ger\\")]
        public string TemporaryTxtDbPath {
            get {
                return ((string)(this["TemporaryTxtDbPath"]));
            }
            set {
                this["TemporaryTxtDbPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("DB\\")]
        public string DbFolder {
            get {
                return ((string)(this["DbFolder"]));
            }
            set {
                this["DbFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("j:\\Keeper\\")]
        public string KeeperFolder {
            get {
                return ((string)(this["KeeperFolder"]));
            }
            set {
                this["KeeperFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Backup\\")]
        public string BackupFolder {
            get {
                return ((string)(this["BackupFolder"]));
            }
            set {
                this["BackupFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ToDo.txt")]
        public string ToDoFile {
            get {
                return ((string)(this["ToDoFile"]));
            }
            set {
                this["ToDoFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("KeeperRegPays.json")]
        public string RegularPaymentsFile {
            get {
                return ((string)(this["RegularPaymentsFile"]));
            }
            set {
                this["RegularPaymentsFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10000")]
        public decimal IgnoreMonthlyDepositProfitBelowByr {
            get {
                return ((decimal)(this["IgnoreMonthlyDepositProfitBelowByr"]));
            }
            set {
                this["IgnoreMonthlyDepositProfitBelowByr"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50")]
        public decimal LargeExpenseUsd {
            get {
                return ((decimal)(this["LargeExpenseUsd"]));
            }
            set {
                this["LargeExpenseUsd"] = value;
            }
        }
    }
}
