
ALTER proc [dbo].[mwReplUpdatePriceEnabledAndValue] @tokey int, @calcKey int, @rqId int = null
as
begin
	-- <date>2012-09-20</date>
	-- <version>2009.2.16.1</version>
	
	declare @ctFromKey int, @cnKey int
	declare @tableName varchar(500)
	declare @mwSearchType int
	declare @source varchar(200), @sql nvarchar(max)
	set @source = ''
	
	if dbo.mwReplIsSubscriber() > 0 and len(dbo.mwReplPublisherDB()) > 0
		set @source = '[mt].' + dbo.mwReplPublisherDB() + '.'
	
	if (@rqId is not null)
		insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'Start mwReplUpdatePriceEnabledAndValue'
		
	select @mwSearchType = isnull(SS_ParmValue, 1) from dbo.systemsettings with(nolock) 
	where SS_ParmName = 'MWDivideByCountry'
	
	if (@mwSearchType = 0)
	begin
		set @tableName = 'mwPriceDataTable'
	end
	else
	begin
		select @ctFromKey = TL_CTDepartureKey, @cnKey = TO_CNKey
		from Turlist join TP_Tours on TL_KEY = TO_TRKey
		where TO_Key = @tokey
		
		set @tableName = dbo.mwGetPriceTableName(@cnKey, @ctFromKey)		
	end
	
	set @sql = 'update ' + @tableName + ' set pt_isenabled = 1, pt_price = tp_gross'
	set @sql = @sql + ' from ' + @source + 'dbo.tp_prices'
	set @sql = @sql + ' where pt_pricekey = tp_key and tp_calculatingkey = ' + ltrim(STR(@calcKey))
	print (@sql)
	exec (@sql)

	set @sql = 'update mwPriceDataTable set pt_isenabled = 1, pt_price = tp_gross'
	set @sql = @sql + ' from ' + @source + 'dbo.tp_prices'
	set @sql = @sql + ' where pt_pricekey = tp_key and tp_calculatingkey = ' + ltrim(STR(@calcKey))
	print (@sql)
	exec (@sql)

	set @sql = 'update ' + @tableName + ' set pt_tourcreated = to_datecreated'
	set @sql = @sql + ' from ' + @source + 'dbo.tp_tours'
	set @sql = @sql + ' where to_key = ' + ltrim(STR(@tokey))
	set @sql = @sql + ' and pt_tourkey = to_key'
	print (@sql)
	exec (@sql)

	set @sql = 'update mwPriceDataTable set pt_tourcreated = to_datecreated'
	set @sql = @sql + ' from ' + @source + 'dbo.tp_tours'
	set @sql = @sql + ' where to_key = ' + ltrim(STR(@tokey))
	set @sql = @sql + ' and pt_tourkey = to_key'
	print (@sql)
	exec (@sql)

	set @sql = 'update mwSpoDataTable set sd_tourcreated = to_datecreated'
	set @sql = @sql + ' from ' + @source + 'dbo.tp_tours'
	set @sql = @sql + ' where to_key = ' + ltrim(STR(@tokey))
	set @sql = @sql + ' and sd_tourkey = to_key'
	print (@sql)
	exec (@sql)
	
	if (@rqId is not null)
		insert into mwReplQueueHistory([rqh_rqid], [rqh_text]) select @rqId, 'End mwReplUpdatePriceEnabledAndValue'
end
