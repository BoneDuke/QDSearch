if exists (select 1 from sys.objects where name like '%mwReplDisableDeletedPrices%' and type = 'p')
begin
	DROP PROCEDURE [dbo].[mwReplDisableDeletedPrices]
end
GO

create procedure [dbo].[mwReplDisableDeletedPrices]
--<DATE>2013-12-12</DATE>
--<VERSION>9.2.20.3</VERSION>
as
begin
	declare @cnKey int
	declare @ctKeyFrom int
	declare @toKey int
	declare @sql varchar (500)
	declare @wasError as bit
	declare @errorText as nvarchar(max)

	set @wasError = 0

	select top 100000 * into #mwReplDeletedPricesTemp from dbo.mwReplDeletedPricesTemp with(nolock) order by rdp_date desc
	create index x_pricekey on #mwReplDeletedPricesTemp(rdp_pricekey);

	begin try
	if (dbo.mwReplIsSubscriber() > 0 or (dbo.mwReplIsPublisher() <= 0 and dbo.mwReplIsSubscriber() <= 0))
		and (exists(select top 1 1 from #mwReplDeletedPricesTemp))
	begin

		update [dbo].[mwPriceDataTable]
		set pt_isenabled = 0
		where exists(select 1 from #mwReplDeletedPricesTemp r where r.rdp_pricekey = pt_pricekey and r.rdp_cnkey = pt_cnkey and r.rdp_ctdeparturekey = pt_ctkeyfrom)
		and pt_isenabled = 1

		create table #delKeys (xKey int)
		create table #tourKeyDirections (xToKey int, xCnKey int, xCtKey int)

		if exists(select 1 from SystemSettings where SS_ParmName = 'MWDivideByCountry' and SS_ParmValue = 1)
		begin
			declare @wasErrorInCycle as bit
			set @wasErrorInCycle = 0

			begin try
				create table #directions (xTableName varchar(200), xCnKey int, xCtKey int)
				insert into #directions (xTableName, xCnKey, xCtKey)
				select distinct dbo.mwGetPriceTableName(rdp_cnkey, rdp_ctdeparturekey) as ptn_tablename, rdp_cnkey, rdp_ctdeparturekey
				from #mwReplDeletedPricesTemp with(nolock)
				
				--Используется секционирование ценовых таблиц
				declare mwPriceDataTableNameCursor cursor local fast_forward for
				select xTableName, xCnKey, xCtKey
				from #directions

				declare @mwPriceDataTableName varchar(200);
				open mwPriceDataTableNameCursor;
				fetch next from mwPriceDataTableNameCursor into @mwPriceDataTableName, @cnKey, @ctKeyFrom

				while @@FETCH_STATUS = 0
				begin
					if exists (select * from sys.tables where @mwPriceDataTableName like '%' + name)
					begin

							set @sql = 'insert into #tourKeyDirections (xToKey, xCnKey, xCtKey)
								select distinct pt_tourkey, pt_cnkey, pt_ctkeyfrom
								from [dbo].[' + @mwPriceDataTableName + ']
								where exists(select 1 from #mwReplDeletedPricesTemp r where r.rdp_pricekey = pt_pricekey and r.rdp_cnkey = ' + ltrim(str(@cnKey)) + ' and r.rdp_ctdeparturekey = ' + ltrim(str(@ctKeyFrom)) + ')
								and pt_tourkey not in (select xToKey from #tourKeyDirections)'
								print (@sql)
								exec (@sql)

							set @sql='
								update ' + @mwPriceDataTableName + ' 
								set pt_isenabled = 0
								where exists(select 1 from #mwReplDeletedPricesTemp r where r.rdp_pricekey = pt_pricekey)
								and pt_isenabled <> 0';
							print (@sql)
							exec (@sql)

						--set @sql = 'insert into #delKeys (xKey) 
						--	select sd_key
						--	from mwSpoDataTable
						--	where sd_isenabled = 1
						--	and sd_cnkey = ' + ltrim(str(@cnKey)) + ' and sd_ctkeyfrom = ' + ltrim(str(@ctKeyFrom)) + '
						--	and sd_hdkey not in (select distinct pt_hdkey from [dbo].[' + @mwPriceDataTableName + '] with(nolock) where pt_isenabled = 1 and pt_tourkey = sd_tourkey)'
						--exec (@sql)

						--set @sql = 'update mwSpoDataTable set sd_isenabled = 0 where sd_key in (select xKey from #delKeys)'
						--exec (@sql)

						--truncate table #delKeys
					end

					fetch next from mwPriceDataTableNameCursor into @mwPriceDataTableName, @cnKey, @ctKeyFrom
				end
			end try
			begin catch
				set @wasErrorInCycle = 1
				set @errorText = ERROR_MESSAGE()
			end catch

			-- release resources
			close mwPriceDataTableNameCursor
			deallocate mwPriceDataTableNameCursor

			declare cacheCursor cursor local fast_forward for
			select distinct  xToKey, xCnKey, xCtKey
			from #tourKeyDirections

			open cacheCursor
			fetch cacheCursor into @toKey, @cnKey, @ctKeyFrom
			while (@@FETCH_STATUS = 0)
			begin
				exec SftWebPsDB.dbo.UpdateSearchFilter @toKey, 1
				exec SftWebPsDB.dbo.ClearSearchCache @cnKey, @ctKeyFrom
				fetch cacheCursor into @toKey, @cnKey, @ctKeyFrom
			end
			close cacheCursor
			deallocate cacheCursor

			if @wasError = 1
			begin
				-- rethrow error after resources release
				raiserror(@errorText, 16, 1)
			end
		end
		else
		begin
			--Секционирование не используется
			update dbo.mwPriceDataTable 
			set pt_isenabled = 0
			where exists(select 1 from #mwReplDeletedPricesTemp r where r.rdp_pricekey = pt_pricekey);
		end
	end

	end try
	begin catch
		set @wasError = 1
		set @errorText = ERROR_MESSAGE()
	end catch

	if @wasError = 0
	begin
		-- delete from source table only if processing was successful
		delete from mwReplDeletedPricesTemp
		where exists(select top 1 1 from #mwReplDeletedPricesTemp r where r.rdp_pricekey = mwReplDeletedPricesTemp.rdp_pricekey)
	end

	-- release resources
	drop index x_pricekey on #mwReplDeletedPricesTemp;
	drop table #mwReplDeletedPricesTemp;

	if @wasError = 1
	begin
		-- rethrow error after resources release
		raiserror(@errorText, 16, 1)
	end
end
GO

grant exec on [dbo].[mwReplDisableDeletedPrices] to public
GO