if exists (select 1 from sys.objects where name like '%mwReplDeletePriceTour%' and type = 'p')
begin
	DROP PROCEDURE [dbo].[mwReplDeletePriceTour]
end
GO

create proc [dbo].[mwReplDeletePriceTour] @tokey int, @rqId int
as
begin
	declare @mwSearchType int
	select @mwSearchType = ltrim(rtrim(isnull(SS_ParmValue, ''))) from dbo.systemsettings 
		where SS_ParmName = 'MWDivideByCountry'

	if @mwSearchType = 0
	begin
		if (@rqId is not null)
			insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'Start insert into mwDeleted.'
		
		insert into mwDeleted (del_key) 
			select pt_pricekey 
			from mwPriceDataTable with(nolock) 
			where pt_tourkey = @tokey

		if (@rqId is not null)
			insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'Start update mwPriceDataTable.'
								
		update mwPriceDataTable
		set pt_isenabled = 0 
		where pt_isenabled > 0 and pt_tourkey = @tokey
	end
	else
	begin
		declare @tablename varchar(100), @sql varchar(8000)
		declare dCur cursor for select name from sysobjects with(nolock) where name like 'mwPriceDataTable%' and xtype = 'u'
		open dCur

		if (@rqId is not null)
			insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'Start insert into mwDeleted and update mwPriceDataTables.'

		fetch next from dCur into @tablename

		while (@@fetch_status=0)
		begin
		
			set @sql = 'insert into mwDeleted (del_key) 
				select pt_pricekey 
				from ' + @tableName + ' with(nolock) 
				where pt_tourkey = ' + ltrim(str(@tokey))
			exec (@sql)

			set @sql = 'update ' + @tableName + '
				set pt_isenabled = 0 
				where pt_isenabled > 0 and pt_tourkey = ' + ltrim(str(@tokey))
			exec (@sql)

			fetch next from dCur into @tablename
		end	

		update mwPriceDataTable
		set pt_isenabled = 0 
		where pt_isenabled > 0 and pt_tourkey = @tokey
	end
	
	if (@rqId is not null)
		insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'Start update mwSpoDataTable.'
			
	update mwSpoDataTable
	set sd_isenabled = 0 
	where sd_isenabled > 0 and sd_tourkey = @tokey
	
	if (@rqId is not null)
		insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'Start delete from TP_Prices.'
	delete from TP_Prices where tp_tokey = @tokey
	
	if (@rqId is not null)
		insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'Start delete from TP_ServiceLists.'
	delete from TP_ServiceLists where tl_tokey = @tokey
	
	if (@rqId is not null)
		insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'Start delete from TP_Services.'
	delete from TP_Services where ts_tokey = @tokey
	
	if (@rqId is not null)
		insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'Start delete from TP_Lists.'
	delete from TP_Lists where ti_tokey = @tokey
	
	if (@rqId is not null)
		insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'Start delete from TP_Tours.'
	delete from TP_Tours where to_key = @tokey
end
GO

grant exec on [dbo].[mwReplDeletePriceTour] to public
GO