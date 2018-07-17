﻿using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerApps.Samples
{
   public partial class SampleProgram
    {
        [STAThread] // Added to support UX
        static void Main(string[] args)
        {
            CrmServiceClient service = null;
        
            try
            {
                service = SampleHelpers.Connect("Connect");
                if (service.IsReady)
                {
                    #region Sample Code
                    #region Set up
                    SetUpSample(service);
                    #endregion Set up
                    #region Demonstrate

                    Console.WriteLine("Enabling auditing on the organization and account entities.");

                    // Enable auditing on the organization.
                    // First, get the organization's ID from the system user record.
                    Guid orgId = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).OrganizationId;

                    // Next, retrieve the organization's record.
                    Organization org = service.Retrieve(Organization.EntityLogicalName, orgId,
                        new ColumnSet(new string[] { "organizationid", "isauditenabled" })) as Organization;

                    // Finally, enable auditing on the organization.
                    bool organizationAuditingFlag = org.IsAuditEnabled.Value;
                    org.IsAuditEnabled = true;
                    service.Update(org);

                    // Enable auditing on account entities.
                    bool accountAuditingFlag = EnableEntityAuditing(Account.EntityLogicalName, true);

                    Console.WriteLine("Retrieving the account change history.\n");
                    // Retrieve the audit history for the account and display it.
                    RetrieveRecordChangeHistoryRequest changeRequest = new RetrieveRecordChangeHistoryRequest();
                    changeRequest.Target = new EntityReference(Account.EntityLogicalName, _newAccountId);

                    RetrieveRecordChangeHistoryResponse changeResponse =
                        (RetrieveRecordChangeHistoryResponse)service.Execute(changeRequest);

                    AuditDetailCollection details = changeResponse.AuditDetailCollection;

                    foreach (AttributeAuditDetail detail in details.AuditDetails)
                    {
                        // Display some of the detail information in each audit record. 
                        DisplayAuditDetails(detail);
                    }

                    // Update the Telephone1 attribute in the Account entity record.
                    Account accountToUpdate = new Account();
                    accountToUpdate.AccountId = _newAccountId;
                    accountToUpdate.Telephone1 = "123-555-5555";
                    service.Update(accountToUpdate);
                    Console.WriteLine("Updated the Telephone1 field in the Account entity.");

                    // Retrieve the attribute change history.
                    Console.WriteLine("Retrieving the attribute change history for Telephone1.");

                    var attributeChangeHistoryRequest = new RetrieveAttributeChangeHistoryRequest
                    {
                        Target = new EntityReference(
                            Account.EntityLogicalName, _newAccountId),
                        AttributeLogicalName = "telephone1"
                    };

                    var attributeChangeHistoryResponse =
                        (RetrieveAttributeChangeHistoryResponse)service.Execute(attributeChangeHistoryRequest);

                    // Display the attribute change history.
                    details = attributeChangeHistoryResponse.AuditDetailCollection;

                    foreach (var detail in details.AuditDetails)
                    {
                        DisplayAuditDetails(detail);
                    }

                    // Save an Audit record ID for later use.
                    Guid auditSampleId = details.AuditDetails.First().AuditRecord.Id;
                    #endregion Retrieve the Attribute Change History

                    #region Retrieve the Audit Details
                    Console.WriteLine("Retrieving audit details for an audit record.");

                    // Retrieve the audit details and display them.
                    var auditDetailsRequest = new RetrieveAuditDetailsRequest
                    {
                        AuditId = auditSampleId
                    };

                    var auditDetailsResponse =
                        (RetrieveAuditDetailsResponse)service.Execute(auditDetailsRequest);
                    DisplayAuditDetails(auditDetailsResponse.AuditDetail);

                    #endregion Retrieve the Audit Details

                    #region Revert Auditing
                    // Set the organization and account auditing flags back to the old values
                    org.IsAuditEnabled = organizationAuditingFlag;
                    service.Update(org);
                
                    EnableEntityAuditing(Account.EntityLogicalName, accountAuditingFlag);

                    #endregion Revert Auditing

                    #region Clean up
                    CleanUpSample(service);
                    #endregion Clean up

                }
                #endregion Demonstrate

                else
                {
                    const string UNABLE_TO_LOGIN_ERROR = "Unable to Login to Dynamics CRM";
                    if (service.LastCrmError.Equals(UNABLE_TO_LOGIN_ERROR))
                    {
                        Console.WriteLine("Check the connection string values in cds/App.config.");
                        throw new Exception(service.LastCrmError);
                    }
                    else
                    {
                        throw service.LastCrmException;
                    }
                }
            }
            catch (Exception ex)
            {
                SampleHelpers.HandleException(ex);
            }

            finally
            {
                if (service != null)
                    service.Dispose();

                Console.WriteLine("Press <Enter> to exit.");
                Console.ReadLine();
            }

        }

        private static bool EnableEntityAuditing(object entityLogicalName, bool v)
        {
            throw new NotImplementedException();
        }
    }
}