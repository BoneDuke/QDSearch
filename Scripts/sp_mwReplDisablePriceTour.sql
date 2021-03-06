if exists (select 1 from sys.objects where name like '%mwReplDisablePriceTour%' and type = 'p')
begin
	DROP PROCEDURE [dbo].[mwReplDisablePriceTour]
end
GO

create proc [dbo].[mwReplDisablePriceTour] @tourkey int, @rqId int
as
begin
	-- <date>2012-09-20</date>
	-- <version>2009.2.16.1</version>

	declare @mwSearchType int
	select @mwSearchType = ltrim(rtrim(isnull(SS_ParmValue, ''))) from dbo.systemsettings 
		where SS_ParmName = 'MWDivideByCountry'

	if @mwSearchType = 0
	begin
		if (@rqId is not null)
			insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'Start update mwPriceDataTable.'
			
		update mwPriceDataTable
		set pt_isenabled = 0
		where pt_tourkey = @tourkey
	end
	else
	begin
		declare @tableName varchar(100), @tokey int, @cnkey int, @ctkey int
		declare @sql nvarchar(max)

		select 
			@tokey = to_key, 
			@cnkey = to_cnkey, 
			@ctkey = isnull(tl_ctdeparturekey, 0)
		from 
			tp_tours with(nolock)
			inner join tbl_TurList with(nolock) on to_trkey = tl_key
		where
			to_key = @tourkey
			
		if (@tokey is not null)
		begin
			set @tableName = dbo.mwGetPriceTableName(@cnkey, @ctkey)
		
			print (@tableName)
		
			if (@rqId is not null)
				insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'Start update mwPriceDataTable.'
				
			declare @counter int, @counterOut int, @params nvarchar(max)
			set @counter = -1
			set @params = '@counterOut int output'
			while(@counter <> 0)
			begin
				set @sql = 'update top (50000)  ' + @tableName + ' set pt_isenabled = 0 where pt_isenabled = 1 and pt_tourkey = ' + ltrim(str(@tokey)) + '; set @counterOut = @@ROWCOUNT'
				EXECUTE sp_executesql @sql, @params, @counterOut = @counter output
			end

			set @counter = -1
			set @params = '@counterOut int output'
			while(@counter <> 0)
			begin
				set @sql = 'update top (50000) mwPriceDataTable set pt_isenabled = 0 where pt_isenabled = 1 and pt_tourkey = ' + ltrim(str(@tokey)) + '; set @counterOut = @@ROWCOUNT'
				EXECUTE sp_executesql @sql, @params, @counterOut = @counter output
			end
				
			--set @sql = 'update ' + @tableName + ' set pt_isenabled = 0 where pt_tourkey = ' + ltrim(str(@tokey))
			--exec (@sql)
			end
	end

	if (@rqId is not null)
			insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'Start update mwSpoDataTable.'
			
	update mwSpoDataTable
	set sd_isenabled = 0	
	where sd_tourkey = @tourkey
	and sd_isenabled = 1
end
GO

grant exec on [dbo].[mwReplDisablePriceTour] to public
GO