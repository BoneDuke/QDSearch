if exists (select 1 from sys.objects where name like '%mwCleaner%' and type = 'p')
begin
	DROP PROCEDURE [dbo].[mwCleaner]
end
GO

create proc [dbo].[mwCleaner] @priceCount int = 1000000, @deleteToday smallint = 0
as
begin
	--<DATE>2013-07-31</DATE>
	--<VERSION>9.2.20.1</VERSION>
	declare @counter bigint
	declare @deletedRowCount bigint

	insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Запуск mwCleaner', 1)

	declare @today datetime
	set @today = getdate()
	if (@deleteToday <> 1)
	begin
		set @today = dateadd(day, -1, @today)
	end

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[mwReplDeletedPricesTemp]') AND type in (N'U'))
	begin
		delete from mwReplDeletedPricesTemp
		where rdp_date < DATEADD(MONTH, -1, @today)

		if not exists(select top 1 1 from mwReplDeletedPricesTemp)
		begin
			truncate table mwReplDeletedPricesTemp
		end
	end

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Megatec_StateDataPaging]') AND type in (N'U'))
	begin
		while (1 = 1)
		begin
			delete top (@priceCount) from Megatec_StateDataPaging
			where SDP_Date < DATEADD(MONTH, -1, @today)	

			if (@@ROWCOUNT = 0)
				break
		end
	end

	if dbo.mwReplIsSubscriber() > 0 or dbo.mwReplIsPublisher() = 0
	begin
	truncate table CacheQuotas
	end
	
	if dbo.mwReplIsSubscriber() <= 0
	begin
		-- очистка таблиц только на основной базе в случае репликации или если репликации нет

		-- Удаляем записи из таблицы TP_ServiceTours, если таких туров больше нету
		-- Тут количество записей будет не большим, поэтому можно не делить на пачки, туры удаляются редко в ДЦ
		SELECT ST_Id 
		into #Keys
		FROM TP_ServiceTours with(nolock)
		where not exists (select top 1 1 from TP_Tours with(nolock) where TO_Key = ST_TOKey)
		and st_tokey not in (select CP_PriceTourKey from CalculatingPriceLists with(nolock) where CP_StartTime is not null)
	
		delete TP_ServiceTours WHERE ST_Id in (select x.ST_Id from #Keys as x)
	
		drop table #Keys
	
		-- Удаляем неактуальные цены
		set @counter = 0
		while(1 = 1)
		begin
	
			delete 
			from dbo.tp_prices
			where tp_key in (SELECT top (@priceCount/10) tp_key 
							 from dbo.tp_prices with(nolock) 
							 WHERE tp_dateend < @today 
								   and tp_tokey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
								   and tp_tokey not in (select CP_PriceTourKey from CalculatingPriceLists where CP_StartTime is not null) --за исключением отложенного расчета
							)
					
			set @deletedRowCount = @@ROWCOUNT
			if @deletedRowCount = 0
			begin
				insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление tp_prices завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
				--print 'Удаление tp_prices завершено. Удалено ' + ltrim(str(@counter)) + ' записей'
				break
			end
			else
			begin
				print 'Удалено из tp_prices ' + ltrim(str(@deletedRowCount)) + ' записей'
				set @counter = @counter + @deletedRowCount
			end
		end

		-- Удаляем неактуальные удаленные цены из TP_PricesDeleted (ДЦ)
		set @counter = 0
		while(1 = 1)
		begin
			delete 
			from dbo.tp_pricesDeleted
			where tpd_id in (select top (@priceCount/5) tpd_id 
							 from dbo.tp_pricesDeleted with(nolock)
							 where tpd_dateend < @today 
								   and tpd_tokey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
								   and tpd_tokey not in (select CP_PriceTourKey from CalculatingPriceLists where CP_StartTime is not null) --за исключением отложенного расчета
			)
		
			set @deletedRowCount = @@ROWCOUNT
			if @deletedRowCount = 0
			begin
				insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление tp_pricesDeleted завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
				print 'Удаление tp_pricesDeleted завершено. Удалено ' + ltrim(str(@counter)) + ' записей'
				break
			end
			else
			begin
				print 'Удалено из tp_pricesDeleted ' + ltrim(str(@deletedRowCount)) + ' записей'
				set @counter = @counter + @deletedRowCount
			end
		end	
	
		-- Удаляем неактуальные удаленные цены из TP_PriceComponents (ДЦ)
		set @counter = 0
		while(1 = 1)
		begin
			delete 
			from dbo.TP_PriceComponents
			where PC_ID in (SELECT top (@priceCount/50) PC_ID 
							FROM dbo.TP_PriceComponents with(nolock) 
							WHERE PC_TourDate < @today
							and pc_tokey not in (select CP_PriceTourKey from CalculatingPriceLists where CP_StartTime is not null) --за исключением отложенного расчета
			)
		
			set @deletedRowCount = @@ROWCOUNT
			if @deletedRowCount = 0
			begin
				insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление TP_PriceComponents завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
				--print 'Удаление TP_PriceComponents завершено. Удалено ' + ltrim(str(@counter)) + ' записей'
				break
			end
			else
			begin
				print 'Удалено из TP_PriceComponents ' + ltrim(str(@deletedRowCount)) + ' записей'
				set @counter = @counter + @deletedRowCount
			end			
		end	
	
		-- Удаляем неактуальные удаленные цены из TP_ServiceCalculateParametrs (ДЦ)
		set @counter = 0
		while(1 = 1)
		begin
			delete 
			from dbo.TP_ServiceCalculateParametrs
			where SCP_ID in (select top (@priceCount) SCP_ID 
							 from dbo.TP_ServiceCalculateParametrs 
							 WHERE SCP_DateCheckIn < @today)
						 
			set @deletedRowCount = @@ROWCOUNT
			if @deletedRowCount = 0
			begin
				insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление TP_ServiceCalculateParametrs завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
				break
			end
			else
				set @counter = @counter + @deletedRowCount
		end	
	end

	-- Удаляем неактуальные удаленные цены из tp_turdates
	set @counter = 0
	while (1 = 1)
	begin
		delete 
		from dbo.tp_turdates
		where td_key in (select top (@priceCount/10) td_key 
							from dbo.tp_turdates with(nolock) 
							where td_date < @today 
							and td_tokey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
							and td_tokey not in (select CP_PriceTourKey from CalculatingPriceLists where CP_StartTime is not null) --за исключением отложенного расче
		)
			
		set @deletedRowCount = @@ROWCOUNT
		if @deletedRowCount = 0
		begin
			insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление tp_turdates завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
			break
		end
		else
			set @counter = @counter + @deletedRowCount
	end
		
	if dbo.mwReplIsSubscriber() <= 0
	begin
		-- tp_servicelists, tp_lists, tp_services		
		create table #tikeys (tikey int, tokey int)
		CREATE NONCLUSTERED INDEX [IX_Index1]
		ON #tikeys ([tokey])
		INCLUDE ([tikey])

		declare @toKey int
		declare tourCursor cursor local fast_forward for
		select to_key
		from tp_tours with(nolock)
		where TO_Key not in (select CP_PriceTourKey from CalculatingPriceLists with(nolock) where CP_StartTime is not null)
		and TO_Key not in (select to_key from tp_tours with(nolock) where to_update <> 0)
		order by to_key desc

		set @counter = 0

		open tourCursor
		fetch tourCursor into @toKey
		while (@@FETCH_STATUS = 0)
		begin
			insert into #tikeys 
			select ti_key, ti_tokey 
			from dbo.tp_lists with(nolock) 
			where not exists (select 1 from tp_prices with(nolock) where ti_key = tp_tikey and tp_tokey = TI_TOKey)
			and not exists (select 1 from TP_PricesDeleted with(nolock) where ti_key = tpd_tikey and tpd_tokey = TI_TOKey)
			and ti_tokey = @toKey

			delete 
			from dbo.tp_servicelists
			where exists (select 1 from #tikeys where tikey = TL_TIKey and tokey = TL_TOKey)
			and tl_tokey = @toKey

			set @counter = @counter + @@ROWCOUNT

			delete 
			from dbo.tp_lists
			where ti_key in (select tikey from #tikeys where tokey = TI_TOKey) 
			and ti_tokey = @toKey

			set @counter = @counter + @@ROWCOUNT

			delete 
			from dbo.tp_services 
			where TS_Key not in (select TL_TSKey from TP_ServiceLists with(nolock) where TL_TOKey = TS_TOKey)
			and TS_TOKey = @toKey

			set @counter = @counter + @@ROWCOUNT

			truncate table #tikeys

			fetch tourCursor into @toKey
		end
		close tourCursor
		deallocate tourCursor

		drop table #tikeys

		insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление tp_servicelists, tp_lists, tp_services завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)		
	end
	else
	begin
		exec dbo.mwClearOldData
	end

	declare @mwSearchType int
	select @mwSearchType = isnull(SS_ParmValue, 1) from dbo.systemsettings with(nolock) 
	where SS_ParmName = 'MWDivideByCountry'
	
	if dbo.mwReplIsSubscriber() <= 0
	begin
		-- Удаляем неактуальные туры
		set @counter = 0
		while(1 = 1)
		begin

			delete 
			from dbo.TP_Tours
			where to_key in (SELECT TOP 1 TO_Key 
							 FROM TP_Tours 
							 WHERE to_datevalid < @today
							 and to_key not in (select CP_PriceTourKey from CalculatingPriceLists where CP_StartTime is not null) --за исключением отложенного расчета
			)
			
			set @deletedRowCount = @@ROWCOUNT
			if @deletedRowCount = 0
			begin
				insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление TP_Tours завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)		
				break
			end
			else
				set @counter = @counter + @deletedRowCount
		end

		create table #tours
		(
			xKey int identity(1,1),
			xToKey int
		)
	
		insert into #tours (xToKey)
		select TO_Key
		from tp_tours with(nolock) where to_update = 0 and exists(select top 1 1 from dbo.tp_turdates with(nolock) where td_tokey = to_key and td_date < @today)

		declare @currentKey int, @maxKey int
		set @currentKey = 0
		select @maxKey = MAX(xKey) from #tours
		while (@currentKey < @maxKey)
		begin
			set @currentKey = @currentKey + 1
		
			update dbo.tp_tours
			set to_pricecount = (select count(1) from dbo.tp_prices with(nolock) where tp_tokey = to_key), 
				to_updatetime = getdate()
			where to_key = (select xToKey from #tours where xKey = @currentKey)
		
		end
	
		drop table #tours
		insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Обновление tp_tours завершено. Обновлено ' + ltrim(@deletedRowCount) + ' записей', 1)
	end

	if dbo.mwReplIsPublisher() <= 0
	begin

		if exists(select 1 from mwReplQueue with(nolock) where rq_tokey not in (select to_key from tp_tours with(nolock)) and rq_mode <> 4)
		begin
			delete 
			from mwReplQueue
			where rq_tokey not in (select to_key from tp_tours with(nolock))
			and rq_mode <> 4
		end

		if(@mwSearchType = 0)
		begin
			set @counter = 0
			while(1 = 1)
			begin
				delete top (@priceCount) from dbo.mwPriceDataTable where pt_tourdate < @today and pt_tourkey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
							and pt_tourkey not in (select CP_PriceTourKey from CalculatingPriceLists where CP_StartTime is not null) --за исключением отложенного расчета
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
				delete top (@priceCount) from dbo.mwSpoDataTable where sd_tourkey not in (select pt_tourkey from dbo.mwPriceDataTable with(nolock)) and sd_tourkey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
							and sd_tourkey not in (select CP_PriceTourKey from CalculatingPriceLists where CP_StartTime is not null) --за исключением отложенного расчета
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
				delete top (@priceCount) from dbo.mwPriceDurations where not exists(select 1 from dbo.mwPriceDataTable with(nolock) where pt_tourkey = sd_tourkey and pt_days = sd_days and pt_nights = sd_nights) and sd_tourkey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
							and sd_tourkey not in (select CP_PriceTourKey from CalculatingPriceLists where CP_StartTime is not null) --за исключением отложенного расчета
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
					set @sql = 'delete top (' + ltrim(rtrim(str(@priceCount))) + ') from ' + @objName + ' where pt_tourdate < @today and pt_tourkey not in (select to_key from tp_tours with(nolock) where to_update <> 0) and pt_tourkey not in (select CP_PriceTourKey from CalculatingPriceLists where CP_StartTime is not null); set @counterOut = @@ROWCOUNT'
					set @params = '@today datetime, @counterOut int output'
				
					EXECUTE sp_executesql @sql, @params, @today = @today, @counterOut = @counterPart output
				
					if @counterPart = 0
					begin
						insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление ' + @objName + ' завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)	
						break
					end
					else
						set @counter = @counter + @counterPart
				end

				set @counter = 0
				while(1 = 1)
				begin
					set @sql = 'delete top (' + ltrim(rtrim(str(@priceCount))) + ') from dbo.mwSpoDataTable where sd_cnkey = ' + ltrim(rtrim(str(@cnkey))) + ' and sd_ctkeyfrom = ' + ltrim(rtrim(str(@ctkeyfrom))) + ' and sd_tourkey not in (select pt_tourkey from ' + @objName + ' with(nolock)) and sd_tourkey not in (select to_key from tp_tours with(nolock) where to_update <> 0) and sd_tourkey not in (select CP_PriceTourKey from CalculatingPriceLists where CP_StartTime is not null); set @counterOut = @@ROWCOUNT'
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
				fetch next from delCursor into @cnkey, @ctkeyfrom
			end
			close delCursor
			deallocate delCursor

			while(1 = 1)
			begin
				set @sql = 'delete top (' + ltrim(rtrim(str(@priceCount))) + ') from mwPriceDataTable where pt_tourdate < @today and pt_tourkey not in (select to_key from tp_tours with(nolock) where to_update <> 0) and pt_tourkey not in (select CP_PriceTourKey from CalculatingPriceLists where CP_StartTime is not null); set @counterOut = @@ROWCOUNT'
				set @params = '@today datetime, @counterOut int output'
				
				EXECUTE sp_executesql @sql, @params, @today = @today, @counterOut = @counterPart output
				
				if @counterPart = 0
				begin
					insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление mwPriceDataTable завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)	
					break
				end
				else
					set @counter = @counter + @counterPart
			end

			exec SftWebPsDB.dbo.UpdateAllSearchFilter
		end
	
		set @counter = 0
		while(1 = 1)
		begin
			delete top (@priceCount) from dbo.mwPriceHotels where sd_tourkey not in (select sd_tourkey from dbo.mwSpoDataTable with(nolock)) and sd_tourkey not in (select to_key from tp_tours with(nolock) where to_update <> 0)
						and sd_tourkey not in (select CP_PriceTourKey from CalculatingPriceLists where CP_StartTime is not null)
			set @deletedRowCount = @@ROWCOUNT
			if @deletedRowCount = 0
			begin
				insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление mwPriceHotels завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
				break
			end
			else
				set @counter = @counter + @deletedRowCount
		end
	end

	-- Удаляем неактуальные логи (остаются логи за последние 7 дней)
	set @counter = 0
	while(1 = 1)
	begin
		delete top (@priceCount) from dbo.SystemLog where SL_DATE < DATEADD(day, -7, @today)
		set @deletedRowCount = @@ROWCOUNT
		if @deletedRowCount = 0
		begin
			insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Удаление systemLog завершено. Удалено ' + ltrim(str(@counter)) + ' записей', 1)
			break
		end
		else
			set @counter = @counter + @deletedRowCount
	end

	insert into SystemLog (SL_Type, SL_Date, SL_Message, SL_AppID) values(1, GETDATE(), 'Окончание выполнения mwCleaner', 1)
end
GO

grant exec on [dbo].[mwCleaner] to public
GO