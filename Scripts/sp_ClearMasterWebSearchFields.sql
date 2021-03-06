

ALTER PROCEDURE [dbo].[ClearMasterWebSearchFields]
	@tokey int, -- ключ тура
	@calcKey int = null
as
begin
	--<VERSION>2009.2.19</VERSION>
	--<DATE>2013-04-10</DATE>

	update dbo.TP_Tours set TO_Update = 1, TO_Progress = 0 where TO_Key = @tokey

	if(@calcKey is null)
		exec dbo.mwEnablePriceTour @tokey, 0, @calcKey
		
	-- если есть репликация и это не подписчик, прекратим выполнение
	if dbo.mwReplIsPublisher() > 0
	begin	
		update dbo.TP_Tours set TO_Update = 0, TO_Progress = 100, TO_UpdateTime = GetDate() where TO_Key = @tokey
		return
	end
		
	update dbo.TP_Tours set TO_Progress = 10 where TO_Key = @tokey

	declare @tableName as nvarchar(150)
	declare tCur cursor for
	select name from sys.tables where name like 'mwPriceDataTable%'

	declare @sql as nvarchar(max)
	declare @condition as nvarchar(300)
	
	if(@calcKey is not null)
	begin		
		set @condition = 'pt_pricekey in (select tp_key from tp_prices with(nolock) where tp_calculatingkey = ' + STR(@calcKey) + ')'
	end
	else
	begin
		set @condition = 'pt_tourkey = ' + STR(@tokey)
	end

	open tCur
	fetch next from tCur into @tableName
	
	while @@fetch_status = 0
	begin
	
		set @sql = '
			while (1 = 1)
			begin
				delete top (100000) from #tableName where #condition
				if (@@ROWCOUNT = 0)
					break
			end
		'
		
		set @sql = REPLACE(@sql, '#tableName', @tableName)
		set @sql = REPLACE(@sql, '#condition', @condition)
		
		print @sql
		exec (@sql)
	
		fetch next from tCur into @tableName
	
	end
	
	close tCur
	deallocate tCur

	update dbo.TP_Tours set TO_Progress = 25 where TO_Key = @tokey

	update dbo.TP_Tours set TO_Progress = 50 where TO_Key = @tokey

	if(@calcKey is null)		
	begin
		while(1 = 1)
		begin
			delete top(100000) from dbo.mwPriceDurations where sd_tourkey = @tokey
			if (@@ROWCOUNT = 0)
				break
		end
	end

	update dbo.TP_Tours set TO_Progress = 75 where TO_Key = @tokey

	if(@calcKey is null)		
	begin
		while (1 = 1)
		begin
			delete top(100000) from dbo.mwPriceHotels where sd_tourkey = @tokey
			if (@@ROWCOUNT = 0)
				break
		end
	end

	update dbo.TP_Tours set TO_Update = 0, TO_Progress = 100, TO_UpdateTime = GetDate() where TO_Key = @tokey
end
