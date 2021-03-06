
ALTER proc [dbo].[mwCleaner] @priceCount int = 10000, @deleteToday smallint = 0
as
begin
	--<DATE>2012-10-15</DATE>
	--<VERSION>9.2.16.3</VERSION>
	declare @counter bigint
	declare @deletedRowCount bigint

	insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Запуск mwCleaner', 1)

	truncate table CacheQuotas

	declare @today datetime
	set @today = GETDATE()
	--set @today = CAST(CONVERT(varchar(20), GETDATE(), 112) as datetime)
	--if (@deleteToday = 1)
	--begin
	--	set @today = dateadd(day, 1, @today)
	--end
	
	-- Удаляем записи из таблицы TP_ServiceTours, если таких туров больше нету
	-- Тут количество записей будет не большим, поэтому можно не делить на пачки, туры удаляются редко в ДЦ
	delete TP_ServiceTours
	where not exists (select top 1 1 from TP_Tours with(nolock) where TO_Key = ST_TOKey)
	
	-- Удаляем неактуальные цены
	set @counter = 0
	while(1 = 1)
	begin
		delete top (@priceCount * 100) from dbo.tp_prices where tp_dateend < @today and tp_tokey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
		set @deletedRowCount = @@ROWCOUNT
		if @deletedRowCount = 0
		begin
			insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление tp_prices завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
			break
		end
		else
			set @counter = @counter + @deletedRowCount
	end

	-- Удаляем неактуальные удаленные цены из TP_PricesDeleted (ДЦ)
	set @counter = 0
	while(1 = 1)
	begin
		delete top (@priceCount * 100) from dbo.tp_pricesDeleted where tpd_dateend < @today and tpd_tokey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
		set @deletedRowCount = @@ROWCOUNT
		if @deletedRowCount = 0
		begin
			insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление tp_pricesDeleted завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
			break
		end
		else
			set @counter = @counter + @deletedRowCount
	end	
	
	-- Удаляем неактуальные удаленные цены из TP_PriceComponents (ДЦ)
	set @counter = 0
	while(1 = 1)
	begin
		delete top (@priceCount * 100) from dbo.TP_PriceComponents where PC_TourDate < @today
		set @deletedRowCount = @@ROWCOUNT
		if @deletedRowCount = 0
		begin
			insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление TP_PriceComponents завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
			break
		end
		else
			set @counter = @counter + @deletedRowCount
	end	
	
	-- Удаляем неактуальные удаленные цены из TP_ServiceCalculateParametrs (ДЦ)
	set @counter = 0
	while(1 = 1)
	begin
		delete top (@priceCount * 100) from dbo.TP_ServiceCalculateParametrs where SCP_DateCheckIn < @today
		set @deletedRowCount = @@ROWCOUNT
		if @deletedRowCount = 0
		begin
			insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление TP_ServiceCalculateParametrs завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
			break
		end
		else
			set @counter = @counter + @deletedRowCount
	end	

	if dbo.mwReplIsSubscriber() <= 0
	begin
		set @counter = 0
		while (1 = 1)
		begin
			delete top (@priceCount) from dbo.tp_turdates where td_date < @today and td_tokey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
			set @deletedRowCount = @@ROWCOUNT
			if @deletedRowCount = 0
			begin
				insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление tp_turdates завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
				break
			end
			else
				set @counter = @counter + @deletedRowCount
		end
		
		set @counter = 0
		while (1 = 1)
		begin
			delete top (@priceCount) from dbo.tp_servicelists where tl_tikey not in (select tp_tikey from tp_prices with(nolock) where tp_tokey = tl_tokey union select TPD_TIKey from TP_PricesDeleted with(nolock) where TPD_TOKey = tl_tokey) and tl_tokey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
			set @deletedRowCount = @@ROWCOUNT
			if @deletedRowCount = 0
			begin
				insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление tp_servicelists завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
				break
			end
			else
				set @counter = @counter + @deletedRowCount
		end
		
		set @counter = 0
		while (1 = 1)
		begin
			delete top (@priceCount) from dbo.tp_lists where ti_key not in (select tp_tikey from tp_prices with(nolock) where tp_tokey = ti_tokey union select TPD_TIKey from TP_PricesDeleted with(nolock) where TPD_TOKey = ti_tokey) and ti_tokey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
			set @deletedRowCount = @@ROWCOUNT
			if @deletedRowCount = 0
			begin
				insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление tp_lists завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
				break
			end
			else
				set @counter = @counter + @deletedRowCount
		end
		
		set @counter = 0
		while (1 = 1)
		begin
			delete top (@priceCount) from dbo.tp_services where ts_key not in (select tl_tskey from tp_servicelists with(nolock) where tl_tokey = ts_tokey) and ts_tokey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
			set @deletedRowCount = @@ROWCOUNT
			if @deletedRowCount = 0
			begin
				insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление tp_services завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
				break
			end
			else
				set @counter = @counter + @deletedRowCount
		end
	end
	else
	begin
		exec dbo.mwCleanerQuotes
	end

	declare @mwSearchType int
	select @mwSearchType = isnull(SS_ParmValue, 1) from dbo.systemsettings with(nolock) 
	where SS_ParmName = 'MWDivideByCountry'
	
	-- Удаляем неактуальные туры
	set @counter = 0
	while(1 = 1)
	begin
		delete top (@priceCount / 100) from dbo.TP_Tours where to_datevalid < @today
		set @deletedRowCount = @@ROWCOUNT
		if @deletedRowCount = 0
		begin
			insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление TP_Tours завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)		
			break
		end
		else
			set @counter = @counter + @deletedRowCount
	end
	
	update top (@priceCount / 100) dbo.tp_tours set to_pricecount = 
		(select count(1) from dbo.tp_prices with(nolock) where tp_tokey = to_key), to_updatetime = getdate()
	where to_update = 0 and exists(select 1 from dbo.tp_turdates with(nolock) where td_tokey = to_key and td_date < @today)
	set @deletedRowCount = @@ROWCOUNT

	insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Обновление tp_tours завершено. Обновлено ' + ltrim(@deletedRowCount) + ' записей', 1)

	if(@mwSearchType = 0)
	begin
			set @counter = 0
			while(1 = 1)
			begin
				delete top (@priceCount * 100) from dbo.mwPriceDataTable where pt_tourdate < @today and pt_tourkey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
				set @deletedRowCount = @@ROWCOUNT
				if @deletedRowCount = 0
				begin
					insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление mwPriceDataTable завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)	
					break
				end
				else
					set @counter = @counter + @deletedRowCount
			end
			
			set @counter = 0
			while(1 = 1)
			begin
				delete top (@priceCount * 100) from dbo.mwSpoDataTable where sd_tourkey not in (select pt_tourkey from dbo.mwPriceDataTable with(nolock)) and sd_tourkey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
				set @deletedRowCount = @@ROWCOUNT
				if @deletedRowCount = 0
				begin
					insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление mwSpoDataTable завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)	
					break
				end
				else
					set @counter = @counter + @deletedRowCount
			end
			
			set @counter = 0
			while(1 = 1)
			begin
				delete top (@priceCount * 100) from dbo.mwPriceDurations where not exists(select 1 from dbo.mwPriceDataTable with(nolock) where pt_tourkey = sd_tourkey and pt_days = sd_days and pt_nights = sd_nights) and sd_tourkey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
				set @deletedRowCount = @@ROWCOUNT
				if @deletedRowCount = 0
				begin
					insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление mwPriceDurations завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)	
					break
				end
				else
					set @counter = @counter + @deletedRowCount
			end
	end
	else
	begin
		declare @objName nvarchar(50), @counterPart int
		declare @sql nvarchar(500), @params nvarchar(500)
		declare delCursor cursor fast_forward read_only for select distinct sd_cnkey, sd_ctkeyfrom from dbo.mwSpoDataTable
		declare @cnkey int, @ctkeyfrom int
		open delCursor
		fetch next from delCursor into @cnkey, @ctkeyfrom
		while(@@fetch_status = 0)
		begin
			set @objName = dbo.mwGetPriceTableName(@cnkey, @ctkeyfrom)
			set @counter = 0
			while(1 = 1)
			begin
				set @sql = 'delete top (' + ltrim(rtrim(str(@priceCount * 100))) + ') from ' + @objName + ' where pt_tourdate < ''' + convert(varchar(20), @today, 120) + ''' and pt_tourkey not in (select to_key from tp_tours with(nolock) where to_update <> 0); set @counterOut = @@ROWCOUNT'
				set @params = '@counterOut int output'
				
				EXECUTE sp_executesql @sql, @params, @counterOut = @counterPart output

				if @counterPart = 0
				begin
					insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление ' + @objName + ' завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)	
					break
				end
				else
					set @counter = @counter + @counterPart
			end

			exec ClearSearchCache null, @cnKey, @ctKeyFrom
			
			--exec sp_executesql @sql
			--set @objName = dbo.mwGetPriceTableName(@cnkey, @ctkeyfrom)
			set @counter = 0
			while(1 = 1)
			begin
				set @sql = 'delete top (' + ltrim(rtrim(str(@priceCount * 100))) + ') from dbo.mwSpoDataTable where sd_cnkey = ' + ltrim(rtrim(str(@cnkey))) + ' and sd_ctkeyfrom = ' + ltrim(rtrim(str(@ctkeyfrom))) + ' and sd_tourkey not in (select pt_tourkey from ' + @objName + ' with(nolock)) and sd_tourkey not in (select to_key from tp_tours with(nolock) where to_update <> 0); set @counterOut = @@ROWCOUNT'
				set @params = '@counterOut int output'
				EXECUTE sp_executesql @sql, @params, @counterOut = @counterPart output
				
				if @counterPart = 0
				begin
					insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление mwSpoDataTable завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)	
					break
				end
				else
					set @counter = @counter + @counterPart
			end
			--exec sp_executesql @sql
			--set @sql = 'delete from dbo.mwPriceDurations where sd_cnkey = ' + ltrim(rtrim(str(@cnkey))) + ' and sd_ctkeyfrom = ' + ltrim(rtrim(str(@ctkeyfrom))) + ' and not exists(select 1 from ' + @objName + ' where pt_tourkey = sd_tourkey and pt_days = sd_days and pt_nights = sd_nights) and sd_tourkey not in (select to_key from tp_tours where to_update <> 0)'
			--exec sp_executesql @sql
			fetch next from delCursor into @cnkey, @ctkeyfrom
		end
		close delCursor
		deallocate delCursor
	end 

	set @counter = 0
	while(1 = 1)
	begin
		delete top (@priceCount) from dbo.mwPriceHotels with(rowlock) where sd_tourkey not in (select sd_tourkey from dbo.mwSpoDataTable with(nolock)) and sd_tourkey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
		set @deletedRowCount = @@ROWCOUNT
		if @deletedRowCount = 0
		begin
			insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление mwPriceHotels завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
			break
		end
		else
			set @counter = @counter + @deletedRowCount
	end
	
	insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Окончание выполнения mwCleaner', 1)
end
