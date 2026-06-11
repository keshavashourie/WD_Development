SET serveroutput on
--
-- Copyright Siemens 2023  
--
-- 10.28.2021  
--
-- This procedure fixes an invalid configuration for workflowsteps used in Change Mgt (CM).  
-- It should be used when a customer is upgrading CEP from a version prior to v8.7 and 
-- also happens to be using Change Management (CM).  If they are not using CM 
-- running this script is not necessary.
--
-- Load the stored procedure in your oracle database.  Once loaded you can run the
-- stored procedure with the following command.
--
-- exec csiCMUpdateSpecs
-- 
create or replace procedure csiCMUpdateSpecs as
        
	v_WorkflowStepName varchar2(30);
	v_WorkflowStepId varchar2(16);
	v_NewSpecId varchar2(16);
	v_OldSpecId varchar2(16);
    v_RecordsUpdated Integer;
    v_ErrorCode NUMBER;
    v_ErrorMessage VARCHAR2(32000);
    v_WorkflowName varchar2(30);
                
    Cursor workflow_cursor  IS 
    select BusinessProcessWorkflowName
      from BusinessProcessWorkflowbase;
            
    Cursor workflowStep_cursor(p_WorkflowName varchar2) IS
    select workflowstep.WorkflowStepName, workflowstep.WorkflowStepId, workflowstep.SpecId
      from workflowstep, BusinessProcessWorkflowBase
     where workflowstep.WorkflowId = BusinessProcessWorkflowBase.RevOfRcdId
       and BusinessProcessWorkflowBase.BusinessProcessWorkflowName = p_WorkflowName;    
    
Begin
    
    Begin        

        FOR cWorkflow IN workflow_cursor LOOP
            Begin
                dbms_output.put_line('Processing workflow "' || cWorkflow.BusinessProcessWorkflowName || '"');
                FOR cWorkflowStep IN workflowStep_cursor(cWorkflow.BusinessProcessWorkflowName) LOOP
                   v_WorkflowStepName := cWorkflowStep.WorkflowStepName;
                   v_WorkflowStepId := cWorkflowStep.WorkflowStepId;
                   v_OldSpecId := cWorkflowStep.SpecId;
                   
                   select BusinessProcessSpec.BusinessProcessSpecId 
                     into v_NewSpecId
                     from BusinessProcessSpec,
                          BusinessProcessSpecBase 
                    where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
                      and BusinessProcessSpecBase.BusinessProcessSpecName = v_WorkflowStepName
                      and BusinessProcessSpec.Revision = '1';
                   
                   If(v_NewSpecId != v_OldSpecId AND v_NewSpecId is not null) Then
                    Begin
                        update workflowstep set specid=v_NewSpecId where workflowStepid = v_WorkflowStepId;
                        v_RecordsUpdated := SQL%rowcount; 
                        dbms_output.put_line('Workflowstep ' || cWorkflow.BusinessProcessWorkflowName || '.' ||v_WorkflowStepName || '(' || v_WorkflowStepId || ')' || ' was updated.  OldSpecId=' || v_OldSpecId || ' NewSpecId=' || v_NewSpecId || '. ' || v_RecordsUpdated || ' record was updated.');                    
                    End;            
                   Else
                    Begin
                        If(v_NewSpecId is null) then
                         Begin
                            dbms_output.put_line('Problem looking up workflowstep ' || cWorkflow.BusinessProcessWorkflowName || '.' ||v_WorkflowStepName || '(' || v_WorkflowStepId || ')' || ' the workflow step was not updated.' );                                     
                         End;
                        Else
                         Begin
                            dbms_output.put_line('Workflowstep ' || cWorkflow.BusinessProcessWorkflowName || '.' ||v_WorkflowStepName || '(' || v_WorkflowStepId || ')' || ' has already been updated.  No changes were applied.');                                     
                         End;
                        End If;
                    End;
                   End If; 
                End Loop;
            End;
        End Loop;
        Commit;
    End;
    Exception When Others Then
     Begin
        v_ErrorCode := SQLCODE;
        v_ErrorMessage := SQLERRM;
        dbms_output.put_line('Problem executing procedure csiCMUpdateSpecs. Error: ' || DBMS_UTILITY.FORMAT_ERROR_BACKTRACE || v_ErrorCode || ' : ' || ' ErrorMessage: ' || v_ErrorMessage);                        
        Rollback;
     End;

End;









