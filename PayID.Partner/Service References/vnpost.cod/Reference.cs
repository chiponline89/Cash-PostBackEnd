﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PayID.Partner.vnpost.cod {
    using System.Data;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="vnpost.cod.ServiceSoap")]
    public interface ServiceSoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GET_MONEY_STATE_BY_ITEM", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GET_MONEY_STATE_BY_ITEM(string ItemCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GET_MONEY_STATE_BY_ITEM", ReplyAction="*")]
        System.Threading.Tasks.Task<System.Data.DataSet> GET_MONEY_STATE_BY_ITEMAsync(string ItemCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GET_MONEY_STATES_BY_PACKAGE", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GET_MONEY_STATES_BY_PACKAGE(string PackageCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GET_MONEY_STATES_BY_PACKAGE", ReplyAction="*")]
        System.Threading.Tasks.Task<System.Data.DataSet> GET_MONEY_STATES_BY_PACKAGEAsync(string PackageCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GET_MONEY_STATES_BY_LIST", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GET_MONEY_STATES_BY_LIST(string ListCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GET_MONEY_STATES_BY_LIST", ReplyAction="*")]
        System.Threading.Tasks.Task<System.Data.DataSet> GET_MONEY_STATES_BY_LISTAsync(string ListCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GET_MONEY_STATE_BY_ITEM_2", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GET_MONEY_STATE_BY_ITEM_2(string ItemCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GET_MONEY_STATE_BY_ITEM_2", ReplyAction="*")]
        System.Threading.Tasks.Task<System.Data.DataSet> GET_MONEY_STATE_BY_ITEM_2Async(string ItemCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GET_MONEY_STATES_BY_LIST_2", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GET_MONEY_STATES_BY_LIST_2(string ListCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GET_MONEY_STATES_BY_LIST_2", ReplyAction="*")]
        System.Threading.Tasks.Task<System.Data.DataSet> GET_MONEY_STATES_BY_LIST_2Async(string ListCode);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ServiceSoapChannel : PayID.Partner.vnpost.cod.ServiceSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ServiceSoapClient : System.ServiceModel.ClientBase<PayID.Partner.vnpost.cod.ServiceSoap>, PayID.Partner.vnpost.cod.ServiceSoap {
        
        public ServiceSoapClient() {
        }
        
        public ServiceSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ServiceSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Data.DataSet GET_MONEY_STATE_BY_ITEM(string ItemCode) {
            return base.Channel.GET_MONEY_STATE_BY_ITEM(ItemCode);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataSet> GET_MONEY_STATE_BY_ITEMAsync(string ItemCode) {
            return base.Channel.GET_MONEY_STATE_BY_ITEMAsync(ItemCode);
        }
        
        public System.Data.DataSet GET_MONEY_STATES_BY_PACKAGE(string PackageCode) {
            return base.Channel.GET_MONEY_STATES_BY_PACKAGE(PackageCode);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataSet> GET_MONEY_STATES_BY_PACKAGEAsync(string PackageCode) {
            return base.Channel.GET_MONEY_STATES_BY_PACKAGEAsync(PackageCode);
        }
        
        public System.Data.DataSet GET_MONEY_STATES_BY_LIST(string ListCode) {
            return base.Channel.GET_MONEY_STATES_BY_LIST(ListCode);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataSet> GET_MONEY_STATES_BY_LISTAsync(string ListCode) {
            return base.Channel.GET_MONEY_STATES_BY_LISTAsync(ListCode);
        }
        
        public System.Data.DataSet GET_MONEY_STATE_BY_ITEM_2(string ItemCode) {
            return base.Channel.GET_MONEY_STATE_BY_ITEM_2(ItemCode);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataSet> GET_MONEY_STATE_BY_ITEM_2Async(string ItemCode) {
            return base.Channel.GET_MONEY_STATE_BY_ITEM_2Async(ItemCode);
        }
        
        public System.Data.DataSet GET_MONEY_STATES_BY_LIST_2(string ListCode) {
            return base.Channel.GET_MONEY_STATES_BY_LIST_2(ListCode);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataSet> GET_MONEY_STATES_BY_LIST_2Async(string ListCode) {
            return base.Channel.GET_MONEY_STATES_BY_LIST_2Async(ListCode);
        }
    }
}
