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
-- This script can be run using MSSQL Mgt studio or as a script against MSSQL database.
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiCMUpdateSpecs' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiCMUpdateSpecs
GO
CREATE PROCEDURE csiCMUpdateSpecs
AS
BEGIN

    SET NOCOUNT ON;
	Declare @WorkflowName varchar(30)
	Declare @WorkflowStepName varchar(30)
	Declare @WorkflowStepId varchar(16)
	Declare @NewSpecId varchar(16)
	Declare @OldSpecId varchar(16)
	Declare @RecordsUpdated Integer

	Begin Try
	 	Declare workflow_cursor cursor for 
		 select BusinessProcessWorkflowName
		  from BusinessProcessWorkflowbase

		Open workflow_cursor
		Fetch next 
		 from workflow_cursor 
		 into @WorkflowName

		Begin
			While @@FETCH_STATUS = 0
			BEGIN
				Print('Processing workflow ' + @WorkflowName)

				Declare workflowStep_cursor cursor for
				 select workflowstep.WorkflowStepName, workflowstep.WorkflowStepId, workflowstep.SpecId
				   from workflowstep, BusinessProcessWorkflowBase
				  where workflowstep.WorkflowId = BusinessProcessWorkflowBase.RevOfRcdId
					and BusinessProcessWorkflowBase.BusinessProcessWorkflowName = @WorkflowName

				Open workflowStep_cursor
				Fetch next 
				 from workflowStep_cursor 
				 into @WorkflowStepName, @WorkflowStepId, @OldSpecId

				While @@FETCH_STATUS = 0
				BEGIN
					select @NewSpecId=BusinessProcessSpec.BusinessProcessSpecId 
					  from BusinessProcessSpec,
						   BusinessProcessSpecBase 
					 where BusinessProcessSpec.BusinessProcessSpecBaseId = BusinessProcessSpecBase.BusinessProcessSpecBaseId
					   and BusinessProcessSpecBase.BusinessProcessSpecName = @WorkflowStepName
					   and BusinessProcessSpec.Revision = '1'

					If(@NewSpecId <> @OldSpecId AND @NewSpecId is not null)
					 Begin
						update workflowstep set specid=@NewSpecId where workflowStepid = @WorkflowStepId
						set @RecordsUpdated = @@RowCount
						PRINT('Workflowstep '+ @WorkflowName + '.' + @WorkflowStepName +'('+@WorkflowStepId+')' + ' was updated.  OldSpecId=' + @OldSpecId + ' NewSpecId=' + @NewSpecId + '. ' + str(@RecordsUpdated) + ' record was updated.')
					 End
					Else
					 Begin
						If(@NewSpecId is null)
						 Begin
							Print('Problem looking up workflowstep ' + @WorkflowName + '.' +@WorkflowStepName + '(' + @WorkflowStepId + ')' + ' the workflow step was not updated.' );                                     
						 End
						Else
						 Begin
							Print('Workflowstep ' + @WorkflowName + '.' +@WorkflowStepName + '(' + @WorkflowStepId + ')' + ' has already been updated.    No changes were applied.' ); 
						 End
					 End
					Fetch next 
					 from workflowStep_cursor 
					 into @WorkflowStepName, @WorkflowStepId,@OldSpecId
				END
				close workflowStep_cursor
				deallocate workflowStep_cursor

				fetch next 
				 from workflow_cursor 
				 into @WorkflowName

			END
		End 
	End Try
	Begin Catch
		Print('Problem executing procedure csiCMUpdateSpecs. Error Code: ' + str(ERROR_NUMBER()) + ' : ' + ' ErrorMessage: ' + ERROR_MESSAGE() + '. The error occurred on line ' + str(ERROR_LINE()))
		rollback
	End Catch;

	close workflow_cursor
	deallocate workflow_cursor
End
Go

exec csiCMUpdateSpecs
drop procedure csiCMUpdateSpecs
